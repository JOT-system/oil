Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

''' <summary>
''' 日報明細
''' </summary>
''' <remarks></remarks>
Public Class GRT00005NIPPO
    Inherits Page

    '共通関数宣言(BASEDLL)
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
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
    ''' <summary>
    ''' UPROFview・Detail取得
    ''' </summary>
    Private CS0052DetailView As New CS0052DetailView                'UPROFview・Detail取得
    ''' <summary>
    ''' 日報共通クラス
    ''' </summary>
    Private T0005COM As New GRT0005COM                              '日報共通
    Private MA002UPDATE As New GRMA002UPDATE                '車両台帳更新
    Private MB003UPDATE As New GRMB003UPDATE                '従業員（配送時間）更新
    Private T0004UPDATE As New GRT0004UPDATE                '配送受注、荷主受注ＤＢ更新
    Private T0005UPDATE As New GRT0005UPDATE                '日報ＤＢ更新
    Private CS0044L1INSERT As New CS0044L1INSERT

    '検索結果格納ds
    Private T0005tbl As DataTable                           'Grid格納用テーブル
    Private T0005INPtbl As DataTable                        '日報テーブル（GridView用）
    Private T0005WEEKtbl As DataTable                       '一週間前データ用テーブル
    Private S0013tbl As DataTable                           'データフィールド
    Private ML002tbl As DataTable                           '勘定科目判定テーブル

    '共通処理結果

    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     'リターンコード
    Private WW_DUMMY As String                                      'リターンコード

    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROWCOUNT As Integer = 20                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10               'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID
    Private Const CONST_DETAIL_REP_COLUMN As Integer = 3            '詳細部の列数

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
                        Case "WF_ButtonUPDATE"                          '■ 更新ボタン押下時処理
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"                             '■ 終了ボタン押下時処理
                            WF_ButtonEND_Click()
                        Case "WF_ButtonCSV"                             '■ ダウンロードボタン押下時処理
                            WF_Print_Click("XLSX")
                        Case "WF_ButtonPrint"                           '■ 印刷ボタン押下時処理
                            WF_Print_Click("pdf")
                        Case "WF_ButtonFIRST"                           '■ 最始行ボタンクリック時処理
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                            '■ 最終行ボタンクリック時処理
                            WF_ButtonLAST_Click()
                        Case "WF_UPDATE"                                '■ 明細 更新ボタン押下時処理
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                                 '■ 明細 クリアボタン押下時処理
                            WF_CLEAR_Click()
                        Case "WF_ButtonSel"                             '■ 左ボックス 選択ボタン押下時処理
                            WF_ButtonSel_Click()
                            WF_WORKKBN_OnChange()
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
                        Case "WF_WORKKBNChange"                         '■ 入力領域　作業区分変更時処理
                            WF_WORKKBN_OnChange()
                        Case "WF_EXCEL_UPLOAD"
                            Master.Output(C_MESSAGE_NO.FILE_UPLOAD_ERROR, C_MESSAGE_TYPE.ERR)
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '〇初期化処理
                Initialize()

            End If
        Finally
            If Not IsNothing(T0005tbl) Then
                T0005tbl.Dispose()
                T0005tbl = Nothing
            End If
            If Not IsNothing(T0005INPtbl) Then
                T0005INPtbl.Dispose()
                T0005INPtbl = Nothing
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
    Public Sub Initialize()

        WF_YMD.Focus()
        WF_FIELD.Value = ""
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '○初期値設定
        MAPrefelence(WW_DUMMY)

        '■■■ 選択情報　設定処理 ■■■
        '〇左Boxへの初期値設定
        leftview.activeListBox()

        '〇右Boxへの初期値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        rightview.selectIndex(GRIS0004RightBox.RIGHT_TAB_INDEX.LS_ERROR_LIST)
        '■■■ 画面（GridView）表示項目取得 ■■■
        SetInitialGridData()
        '○Detail初期設定
        InitialRepeater()
        '〇新規の場合、ヘッダ項目属性（入力）変更
        If work.WF_T5_YMD.Text = "" Then
            WF_Head_LINECNT.Enabled = False
        Else
            WF_Head_LINECNT.Enabled = True
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
        If IsNothing(T0005INPtbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        End If
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            If T0005INPtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                T0005INPtbl.Rows(i)("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If String.IsNullOrEmpty(WF_GridPosition.Text) OrElse
            Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
            WW_GridPosition = 1
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) < WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) >= 0 Then
                WW_GridPosition = WW_GridPosition - CONST_SCROLLROWCOUNT
            End If
        End If

        '○画面（GridView）表示
        Using WW_TBLview As DataView = New DataView(T0005INPtbl)

            'ソート
            WW_TBLview.Sort = "LINECNT"
            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString

            '一覧作成
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRT00005WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = WW_TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()

            '○クリア
            If WW_TBLview.Count = 0 Then
                WF_GridPosition.Text = "1"
            Else
                WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
            End If
        End Using

        WF_YMD.Focus()
        '新規の場合、ヘッダ項目属性（入力）変更
        If work.WF_T5_YMD.Text = "" Then
            WF_Head_LINECNT.Enabled = False
        Else
            WF_Head_LINECNT.Enabled = True
        End If
    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""
        Dim WW_DATE As Date
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_CHECKREPORT As String = String.Empty
        rightview.setErrorReport("")

        '■■■ テーブルデータ復元 ■■■
        If IsNothing(T0005INPtbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        End If

        '■■■ 前画面（T00005I）テーブルデータ復元 ■■■
        If IsNothing(T0005tbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        End If

        If IsNothing(T0005WEEKtbl) Then
            '○画面表示データ復元
            If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        End If

        '---------------------------------
        'ヘッダ項目のチェック
        '---------------------------------
        Dim WW_ERR_FLG As Boolean = False
        If IsNothing(S0013tbl) Then
            S0013tbl = New DataTable
        End If

        If WF_Head_LINECNT.Text <> "" Then
            If WF_Head_LINECNT.Text <> work.WF_T5I_LINECNT.Text Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できません(選択№エラー)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 選択Noは、 , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 参照コピーの場合、空白を指定 , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 変更の場合、一覧画面で選択された選択Noを指定 , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 上記以外許されません。 , "
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERR_FLG = True
            End If
        End If

        '・キー項目(出庫日：YMD)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", WF_YMD.Text, WW_RTN, WW_CHECKREPORT, S0013tbl)
        If isNormal(WW_RTN) Then
            If work.WF_SEL_STYMD.Text <= WF_YMD.Text AndAlso WF_YMD.Text <= work.WF_SEL_ENDYMD.Text Then
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できません(出庫日エラー)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 検索画面で指定した開始から終了の範囲外です ,"
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERR_FLG = True
            End If
        Else
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できません(出庫日エラー)"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_CHECKREPORT & " , "
            rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
            WW_ERR_FLG = True
        End If

        Dim WW_TEXT As String = ""
        Dim WW_ERR_RTN As String = ""
        CodeToName("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_ERR_RTN)
        If Not isNormal(WW_ERR_RTN) Then
            WF_STAFFCODE_TEXT.Text = ""
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できません(乗務員エラー)"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & WF_STAFFCODE.Text & ") ,"
            rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
            WW_ERR_FLG = True
        End If

        CodeToName("DELFLG", WF_DELFLG_H.Text, WW_TEXT, WW_ERR_RTN)
        If Not isNormal(WW_ERR_RTN) Then
            WF_DELFLG_H_TEXT.Text = ""
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できません(削除フラグエラー)"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタに存在しません。(" & WF_DELFLG_H.Text & ") ,"
            rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
            WW_ERR_FLG = True
        End If

        If WW_ERR_FLG = True Then
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If


        '明細行のエラー有無チェック
        If WF_DELFLG_H.Text = C_DELETE_FLG.ALIVE Then
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                If T0005INPtbl.Rows(i)("HDKBN") = "D" Then
                    If T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED OrElse
                       T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED Then
                        '■■■ 前画面（T00005I）用にテーブルデータ保存 ■■■
                        If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・エラーが存在します。(明細行にエラーが存在するため更新できません)"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　　=" & WF_YMD.Text & "  "
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　　=" & WF_STAFFCODE.Text & "  "
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員名　=" & WF_STAFFCODE_TEXT.Text & "  "
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号=" & T0005INPtbl.Rows(i)("SEQ") & "  "
                        rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)

                        Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                        Exit Sub
                    End If
                End If
            Next
        End If

        Dim WW_ERR As String = C_MESSAGE_NO.NORMAL
        WW_RTN = C_MESSAGE_NO.NORMAL

        '-----------------------------------
        'キー項目の変更チェック
        '-----------------------------------
        Dim WW_KeyChange As String = "OFF"

        If WF_Head_LINECNT.Text = work.WF_T5I_LINECNT.Text Then
            '選択№に変更なし
            If WF_YMD.Text = work.WF_T5_YMD.Text AndAlso
               WF_STAFFCODE.Text = work.WF_T5_STAFFCODE.Text Then
                'キー項目変更なし
                '最終チェック
                'データの上書き（削除、追加）
                WW_KeyChange = "0"
            Else
                'キー項目変更あり
                'キー項目の入れ替え
                '再チェック（キー変更のため）
                '最終チェック
                '前回データを削除
                '今回データを追加
                WW_KeyChange = "1"
            End If
        Else
            If Trim(WF_Head_LINECNT.Text) = "" Then
                '選択№に変更あり（参照コピー）
                'キー項目変更あり
                'キー項目の入れ替え
                '再チェック（キー変更のため）
                '最終チェック
                '今回データを追加
                WW_KeyChange = "2"
            Else
                '■■■ 自画面（T00005）用にテーブルデータ保存 ■■■
                If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(選択№は変更できません)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 参照コピーの場合、選択№をクリアしてくさい"
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        End If

        Dim WW_UPD As Boolean = False
        '〇キー項目変更の種別に応じて処理を行う。
        If WW_KeyChange = "0" Then
            '-------------------------------------------------------------------------------------------------------
            'キー項目変更なし
            '-------------------------------------------------------------------------------------------------------

            '全体チェック（ヘッダの削除フラグ='1'の時、チェックしない）
            If WF_DELFLG_H.Text = C_DELETE_FLG.ALIVE Then
                '全明細復活
                For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                    If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                        If T0005INPtbl.Rows(i)("DELFLG") <> WF_DELFLG_H.Text Then
                            For Each row As DataRow In T0005INPtbl.Rows
                                row("DELFLG") = WF_DELFLG_H.Text

                                CheckInputRowData(row, WW_RTN)

                                If isNormal(WW_RTN) OrElse WW_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
                                    row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                Else
                                    row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                    WW_ERR = WW_RTN
                                End If
                            Next

                            'エラーの場合
                            If Not isNormal(WW_ERR) Then
                                '■■■ 自画面（T00005）用にテーブルデータ保存 ■■■
                                If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                                Exit Sub
                            End If

                            WW_UPD = True
                            Exit For
                        End If
                    End If
                Next
                '〇全項目チェック処理
                CheckListTable(WW_RTN)

                If Not isNormal(WW_ERR) Then
                    '■■■ 前画面（T00005I）用にテーブルデータ保存 ■■■
                    If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If
            End If

            'ヘッダの削除フラグ又は、明細行に変更があるかチェック
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                    If T0005INPtbl.Rows(i)("DELFLG") <> WF_DELFLG_H.Text Then
                        T0005INPtbl.Rows(i)("DELFLG") = WF_DELFLG_H.Text
                        WW_UPD = True
                        Exit For
                    End If
                ElseIf T0005INPtbl.Rows(i)("HDKBN") = "D" Then
                    If T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        WW_UPD = True
                        Exit For
                    End If
                End If
            Next

            '変更がなければ、何もせず一覧画面に戻る
            If WW_UPD Then
                '更新あり
                '①前回データを削除（タイムスタンプがゼロ(=0)なら物理削除、ゼロ以外(<>0)なら論理削除）
                Dim WW_TIMSTP As String = String.Empty
                '前回データのキー項目を検索
                For Each row As DataRow In T0005tbl.Rows
                    If row("HDKBN") = "H" Then
                        If row("YMD") = work.WF_T5_YMD.Text AndAlso
                           row("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                           row("SELECT") = "1" Then
                            WW_TIMSTP = row("TIMSTP")
                            Exit For
                        End If
                    End If
                Next

                If WW_TIMSTP = 0 Then
                    'タイムスタンプがゼロ(=0)なら物理削除
                    For i As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                        If T0005tbl.Rows(i)("YMD") = work.WF_T5_YMD.Text AndAlso
                           T0005tbl.Rows(i)("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                           T0005tbl.Rows(i)("SELECT") = "1" Then
                            T0005tbl.Rows(i).Delete()
                        End If
                    Next
                Else
                    'タイムスタンプがゼロ以外(<>0)なら論理削除
                    For i As Integer = 0 To T0005tbl.Rows.Count - 1
                        If T0005tbl.Rows(i)("YMD") = work.WF_T5_YMD.Text AndAlso
                           T0005tbl.Rows(i)("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                           T0005tbl.Rows(i)("SELECT") = "1" Then
                            T0005tbl.Rows(i)("LINECNT") = "0"
                            T0005tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            T0005tbl.Rows(i)("SELECT") = "0"
                            T0005tbl.Rows(i)("HIDDEN") = "1"
                            T0005tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        End If
                    Next
                End If

                '〇ツーマンセルの矢崎編集処理()
                If WF_DELFLG_H.Text = C_DELETE_FLG.ALIVE Then
                    '２マン編集
                    EditTwoManRecordByYazaki(WW_RTN)
                End If

                '②今回データを追加（下記、共通処理）
            Else
                '終了処理
                WF_ButtonEND_Click()
            End If
        ElseIf WW_KeyChange = "1" Then
            '-------------------------------------------------------------------------------------------------------
            'キー項目（出庫日、乗務員）変更あり
            '-------------------------------------------------------------------------------------------------------
            '重複チェック
            Dim WW_REDUNDANCY As Boolean = False
            '出庫日、乗務員、で全体（T0005tbl）を検索し、存在チェックすればエラー
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                If T0005tbl.Rows(i)("HDKBN") = "H" Then
                    If T0005tbl.Rows(i)("YMD") = WF_YMD.Text AndAlso
                       T0005tbl.Rows(i)("STAFFCODE") = WF_STAFFCODE.Text AndAlso
                       T0005tbl.Rows(i)("SELECT") = "1" Then
                        WW_REDUNDANCY = True
                        Exit For
                    End If
                End If
            Next

            '選択№が重複している場合、エラー
            If Not WW_REDUNDANCY Then
                '------------------------------------------------------------
                'キー項目変更あり重複なし、キー項目の入れ替え後、再チェック
                '------------------------------------------------------------
                WW_ERRLIST = New List(Of String)

                For Each row As DataRow In T0005INPtbl.Rows
                    'キー項目の入れ替え＆新規追加するためタイムスタンプをゼロに
                    row("TIMSTP") = "0"
                    row("YMD") = WF_YMD.Text
                    row("STAFFCODE") = WF_STAFFCODE.Text
                    CodeToName("STAFFCODE", row("STAFFCODE"), row("STAFFNAMES"), WW_RTN)
                    If row("HDKBN") = "H" Then
                        row("DELFLG") = WF_DELFLG_H.Text
                    End If

                    'キー項目変更に伴い、再度明細行のチェック（明細行チェックＯＫ後、キー項目変更が変更される場合ありのため）
                    CheckInputRowData(row, WW_RTN)

                    If isNormal(WW_RTN) OrElse WW_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
                        row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    Else
                        row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        WW_ERR = WW_RTN
                    End If
                Next

                'エラーの場合
                If Not isNormal(WW_ERR) Then
                    '■■■ 前画面（T00005I）用にテーブルデータ保存 ■■■
                    If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

                '全体チェック
                CheckListTable(WW_RTN)
                'エラーの場合
                If Not isNormal(WW_RTN) Then
                    If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

                '-------------------------------------------------
                'チェックＯＫならば、前回データ削除
                '-------------------------------------------------
                '①前回データを削除（タイムスタンプがゼロ(=0)なら物理削除、ゼロ以外(<>0)なら論理削除）
                Dim WW_TIMSTP As String = Nothing
                '前回データのキー項目を検索
                For i As Integer = 0 To T0005tbl.Rows.Count - 1
                    If T0005tbl.Rows(i)("HDKBN") = "H" Then
                        If T0005tbl.Rows(i)("YMD") = work.WF_T5_YMD.Text AndAlso
                           T0005tbl.Rows(i)("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                           T0005tbl.Rows(i)("SELECT") = "1" Then
                            WW_TIMSTP = T0005tbl.Rows(i)("TIMSTP")
                            Exit For
                        End If
                    End If
                Next

                If WW_TIMSTP <> Nothing Then
                    If WW_TIMSTP = 0 Then
                        'タイムスタンプがゼロ(=0)なら物理削除
                        For i As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                            If T0005tbl.Rows(i)("YMD") = work.WF_T5_YMD.Text AndAlso
                               T0005tbl.Rows(i)("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                               T0005tbl.Rows(i)("SELECT") = "1" Then
                                T0005tbl.Rows(i).Delete()
                            End If
                        Next
                    Else
                        WW_DATE = Date.Now
                        'タイムスタンプがゼロ以外(<>0)なら論理削除
                        For i As Integer = 0 To T0005tbl.Rows.Count - 1
                            If T0005tbl.Rows(i)("YMD") = work.WF_T5_YMD.Text AndAlso
                               T0005tbl.Rows(i)("STAFFCODE") = work.WF_T5_STAFFCODE.Text AndAlso
                               T0005tbl.Rows(i)("SELECT") = "1" Then
                                T0005tbl.Rows(i)("LINECNT") = "0"
                                T0005tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                T0005tbl.Rows(i)("SELECT") = "0"
                                T0005tbl.Rows(i)("HIDDEN") = "1"
                                T0005tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                            End If
                        Next
                    End If
                End If

            Else
                '■■■ 前画面（T00005I）用にテーブルデータ保存 ■■■ 
                If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(同じ出庫日、乗務員が存在します)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & WF_YMD.Text & "  "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & WF_STAFFCODE.Text & "  "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員名=" & WF_STAFFCODE_TEXT.Text & "  "
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)

                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub

            End If
        ElseIf WW_KeyChange = "2" Then
            '-------------------------------------------------------------------------------------------------------
            '選択№に変更あり（参照コピー）
            '-------------------------------------------------------------------------------------------------------

            '重複チェック
            Dim WW_REDUNDANCY As Boolean = False
            '出庫日、乗務員、で全体（T0005tbl）を検索し、存在チェックすればエラー
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                If T0005tbl.Rows(i)("HDKBN") = "H" Then
                    If T0005tbl.Rows(i)("YMD") = WF_YMD.Text AndAlso
                       T0005tbl.Rows(i)("STAFFCODE") = WF_STAFFCODE.Text AndAlso
                       T0005tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE Then
                        WW_REDUNDANCY = True
                        Exit For
                    End If
                End If
            Next

            '選択№が重複している場合、エラー
            If Not WW_REDUNDANCY Then
                '-------------------------------------------------------
                'キー項目変更あり重複なし、キー項目の入替え後、再チェック
                '-------------------------------------------------------
                WW_ERRLIST = New List(Of String)

                '選択№の最大値を取得
                Dim WW_MAXLINECNT As Integer = T0005tbl.Compute("MAX(LINECNT)", "HDKBN = 'H'")
                WF_Head_LINECNT.Text = WW_MAXLINECNT + 1

                For Each row As DataRow In T0005INPtbl.Rows
                    'キー項目の入れ替え＆新規追加するためタイムスタンプをゼロに
                    row("TIMSTP") = "0"
                    row("YMD") = WF_YMD.Text
                    row("STAFFCODE") = WF_STAFFCODE.Text
                    CodeToName("STAFFCODE", row("STAFFCODE"), row("STAFFNAMES"), WW_RTN)
                    row("SUBSTAFFCODE") = ""
                    row("SUBSTAFFNAMES") = ""
                    If row("HDKBN") = "H" Then
                        row("DELFLG") = WF_DELFLG_H.Text
                    End If

                    'キー項目変更に伴い、再度明細行のチェック（明細行チェックＯＫ後、キー項目変更が変更される場合ありのため）
                    CheckInputRowData(row, WW_RTN)

                    If isNormal(WW_RTN) OrElse WW_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
                        row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    Else
                        row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        WW_ERR = WW_RTN
                    End If

                Next

                'エラーの場合
                If Not isNormal(WW_ERR) Then
                    '■■■ 画面（T00005）用にテーブルデータ保存 ■■■
                    If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

                '全体チェック
                CheckListTable(WW_RTN)
                If Not isNormal(WW_RTN) Then
                    If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End If

            Else
                '■■■ 画面（T00005）用にテーブルデータ保存 ■■■
                If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(同じ出庫日、乗務員が存在します)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & WF_YMD.Text & "  "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & WF_STAFFCODE.Text & "  "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員名=" & WF_STAFFCODE_TEXT.Text & "  "
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)

                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub

            End If
        End If

        '削除フラグ='1'の明細を削除
        For i As Integer = T0005INPtbl.Rows.Count - 1 To 0 Step -1
            If T0005INPtbl.Rows(i)("HDKBN") = "D" AndAlso
               T0005INPtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                T0005INPtbl.Rows(i).Delete()
            End If
        Next

        '③項番(LineCnt)設定
        Dim WW_LINECNT As Integer = 0
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            'SELECT=0（対象外）1（対象）、HIDDEN=0（表示）1（非表示）
            'ヘッダを対象、表示に
            If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                T0005INPtbl.Rows(i)("LINECNT") = WF_Head_LINECNT.Text
                T0005INPtbl.Rows(i)("SELECT") = "1"
                T0005INPtbl.Rows(i)("HIDDEN") = "0"
                T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                T0005INPtbl.Rows(i)("LINECNT") = 0
                T0005INPtbl.Rows(i)("SELECT") = "1"
                T0005INPtbl.Rows(i)("HIDDEN") = "1"
                T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            If WF_DELFLG_H.Text = C_DELETE_FLG.DELETE Then
                T0005INPtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
            End If
            T0005INPtbl.Rows(i)("TIMSTP") = "0"
        Next

        Dim WW_HeadIdx As Integer = 0
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                WW_HeadIdx = i
                T0005INPtbl.Rows(WW_HeadIdx)("ORDERUMU") = ""
            End If
            If T0005INPtbl.Rows(i)("ORDERUMU") = "無" Then
                T0005INPtbl.Rows(i)("ORDERUMU") = ""
                T0005INPtbl.Rows(WW_HeadIdx)("ORDERUMU") = "無"
            End If
            If T0005INPtbl.Rows(i)("TERMKBN") = "" Then
                For j As Integer = 0 To T0005INPtbl.Rows.Count - 1
                    If T0005INPtbl.Rows(i)("NIPPONO") = T0005INPtbl.Rows(j)("NIPPONO") Then
                        T0005INPtbl.Rows(i)("TERMKBN") = T0005INPtbl.Rows(j)("TERMKBN")
                        CodeToName("TERMKBN", T0005INPtbl.Rows(j)("TERMKBN"), T0005INPtbl.Rows(i)("TERMKBNNAMES"), WW_ERR)
                        Exit For
                    End If
                Next
            End If

            T0005INPtbl.Rows(i)("SHARYOTYPEF") = Mid(T0005INPtbl.Rows(i)("TSHABANF"), 1, 1)
            T0005INPtbl.Rows(i)("TSHABANF") = Mid(T0005INPtbl.Rows(i)("TSHABANF"), 2, 19)
            T0005INPtbl.Rows(i)("SHARYOTYPEB") = Mid(T0005INPtbl.Rows(i)("TSHABANB"), 1, 1)
            T0005INPtbl.Rows(i)("TSHABANB") = Mid(T0005INPtbl.Rows(i)("TSHABANB"), 2, 19)
            T0005INPtbl.Rows(i)("SHARYOTYPEB2") = Mid(T0005INPtbl.Rows(i)("TSHABANB2"), 1, 1)
            T0005INPtbl.Rows(i)("TSHABANB2") = Mid(T0005INPtbl.Rows(i)("TSHABANB2"), 2, 19)
        Next

        '④今回データを追加
        T0005tbl.Merge(T0005INPtbl)

        Dim WW_SEQ As Integer = 0
        WW_LINECNT = 0
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

        '■■■ 前画面（T00005I）用にテーブルデータ保存 ■■■
        If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '〇勤怠画面からの遷移時に処理を行う
        If work.WF_T5_FROMMAPID.Text.Contains(GRT00005WRKINC.MAPID7) Then
            '★★★★★★★★★★★★★★★★★★★★★★★★★★★
            '勤怠画面から遷移した場合、ＤＢ更新して勤怠画面に戻る
            '★★★★★★★★★★★★★★★★★★★★★★★★★★★
            NIPPO_Update(WW_RESULT)
            If Not isNormal(WW_RESULT) Then Exit Sub
            '★★★ 画面遷移先URL取得 ★★★
            '画面遷移実行
            Master.MAPvariant = work.WF_T5_FROMMAPVARIANT.Text
            Master.TransitionPrevPage()
        Else
            '★★★★★★★★★★★★★★★★★★★★★★★★
            '日報一覧へ戻る
            '★★★★★★★★★★★★★★★★★★★★★★★★
            WF_ButtonEND_Click()

        End If

    End Sub
    ''' <summary>
    ''' 日報更新処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks>勤怠画面より遷移した場合のみ処理される</remarks>
    Protected Sub NIPPO_Update(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR

        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            'トランザクション
            Dim SQLtrn As SqlClient.SqlTransaction = Nothing

            '--------------------------------------------------------------------
            'ＤＢ更新
            '--------------------------------------------------------------------
            SQLcon.Open() 'DataBase接続(Open)
            'トランザクション開始
            'SQLtrn = SQLcon.BeginTransaction
            SQLtrn = Nothing

            '車両台帳更新（走行キロ）
            MA002UPDATE.SQLcon = SQLcon
            MA002UPDATE.SQLtrn = SQLtrn
            MA002UPDATE.T0005tbl = T0005tbl
            MA002UPDATE.UPDUSERID = Master.USERID
            MA002UPDATE.UPDTERMID = Master.USERTERMID
            MA002UPDATE.Update()
            If Not isNormal(MA002UPDATE.ERR) Then
                Master.Output(MA002UPDATE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            ''従業員マスタ（配送用）更新（配送受注用）
            'MB003UPDATE.SQLcon = SQLcon
            'MB003UPDATE.SQLtrn = SQLtrn
            'MB003UPDATE.SORG = work.WF_SEL_UORG.Text
            'MB003UPDATE.T0005tbl = T0005tbl
            'MB003UPDATE.UPDUSERID = Master.USERID
            'MB003UPDATE.UPDTERMID = Master.USERTERMID
            'MB003UPDATE.Update()
            'If Not isNormal(MB003UPDATE.ERR) Then
            '    Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
            '    Exit Sub
            'End If

            '配送受注、荷主受注更新
            Dim WW_T0004tbl As DataTable = New DataTable
            T0004UPDATE.SQLcon = SQLcon
            T0004UPDATE.SQLtrn = SQLtrn
            T0004UPDATE.T0005tbl = T0005tbl
            T0004UPDATE.UPDUSERID = Master.USERID
            T0004UPDATE.UPDTERMID = Master.USERTERMID
            T0004UPDATE.ListBoxGSHABAN = work.CreateSHABANLists(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN)
            T0004UPDATE.Update()
            If isNormal(T0004UPDATE.ERR) Then
                T0005tbl = T0004UPDATE.RTNTbl
            Else
                Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If

            '日報ＤＢ更新

            '統計DB出力用項目設定
            Dim WW_T0005DELtbl As DataTable = T0005tbl.Clone
            Dim WW_T0005SELtbl As DataTable = T0005tbl.Clone
            Dim WW_T0005CHKtbl As DataTable = T0005tbl.Clone
            '削除データの退避
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            WW_T0005DELtbl = CS0026TBLSORT.sort()
            '有効データのみ
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            WW_T0005SELtbl = CS0026TBLSORT.sort()
            '副乗務員データのみ（チェック用）
            CS0026TBLSORT.TABLE = WW_T0005SELtbl
            CS0026TBLSORT.FILTER = "CREWKBN = '2'"
            WW_T0005CHKtbl = CS0026TBLSORT.sort()

            '-------------------------------------------
            '乗務区分の変更　11/29
            '　※副乗務員を正乗務員として処理する→T0005REEDITが副乗務員の場合、正乗務員とペアで処理しているため
            '　　個別日報（副乗務員のみ）変更の場合、正乗務員として処理させる
            '    また、矢崎車端の場合、正乗務員を変更すると副乗務員を再編集しているため正副が存在し正常に処理されるため除外する
            '    （副乗務員のみの場合、正乗務員として処理する）
            Dim WW_CREWKBN_CHG As Boolean = False
            Dim WW_T0005SELtblCpy As DataTable = WW_T0005SELtbl.Copy

            '副乗務員のみの場合だけ処理する
            If WW_T0005CHKtbl.Rows.Count = WW_T0005SELtbl.Rows.Count Then
                For Each T5row As DataRow In WW_T0005SELtbl.Rows
                    If T5row("CREWKBN") = "2" Then
                        T5row("CREWKBN") = "1"
                        WW_CREWKBN_CHG = True
                    End If
                Next
            End If
            '-------------------------------------------
            '有効データ＋１週間前
            WW_T0005SELtbl.Merge(T0005WEEKtbl)

            'トリップ判定・回送判定・出荷日内荷積荷卸回数判定
            T0005COM.ReEditT0005(WW_T0005SELtbl, WW_DUMMY)

            '有効データと１週間前データの分離
            Dim WW_Filter As String = ""
            '有効データの抽出
            CS0026TBLSORT.TABLE = WW_T0005SELtbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "YMD >= #" & work.WF_T5_YMD.Text & "#"
            T0005tbl = CS0026TBLSORT.sort()

            '-------------------------------------------
            '乗務区分の戻し
            If WW_CREWKBN_CHG Then
                For Each T5Cpyrow As DataRow In WW_T0005SELtblCpy.Rows
                    For Each T5row As DataRow In T0005tbl.Rows
                        If T5Cpyrow("CAMPCODE") = T5row("CAMPCODE") AndAlso
                           T5Cpyrow("SHIPORG") = T5row("SHIPORG") AndAlso
                           T5Cpyrow("TERMKBN") = T5row("TERMKBN") AndAlso
                           T5Cpyrow("YMD") = T5row("YMD") AndAlso
                           T5Cpyrow("STAFFCODE") = T5row("STAFFCODE") AndAlso
                           T5Cpyrow("SEQ") = T5row("SEQ") AndAlso
                           T5Cpyrow("HDKBN") = T5row("HDKBN") Then
                            T5row("CREWKBN") = T5Cpyrow("CREWKBN")
                            Exit For
                        End If
                    Next
                Next
            End If
            '-------------------------------------------
            '有効レコード＋削除レコード（元に戻す）
            T0005tbl.Merge(WW_T0005DELtbl)
            '〇使用したワークテーブルの解放
            WW_T0005SELtblCpy.Dispose()
            WW_T0005SELtblCpy = Nothing
            WW_T0005CHKtbl.Dispose()
            WW_T0005CHKtbl = Nothing

            WW_T0005DELtbl.Dispose()
            WW_T0005DELtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing

            Dim WW_DATE As Date = Date.Now
            '〇T00005更新
            T0005UPDATE.SQLcon = SQLcon
            T0005UPDATE.SQLtrn = SQLtrn
            T0005UPDATE.T0005tbl = T0005tbl
            T0005UPDATE.ENTRYDATE = WW_DATE
            T0005UPDATE.UPDUSERID = Master.USERID
            T0005UPDATE.UPDTERMID = Master.USERTERMID
            T0005UPDATE.Update()
            If T0005UPDATE.ERR = C_MESSAGE_NO.NORMAL Then
                T0005tbl = T0005UPDATE.T0005tbl
            Else
                Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "例外発生")
                Exit Sub
            End If

            ''統計ＤＢ更新

            '削除データの退避
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            Dim WW_T00052L0001DELtbl As DataTable = CS0026TBLSORT.sort()
            '有効データのみ
            CS0026TBLSORT.FILTER = "SELECT = '1'"
            Dim WW_T00052L0001SELtbl As DataTable = CS0026TBLSORT.sort()

            '削除データ（削除処理）
            Dim WW_DATENOW As DateTime = Date.Now
            '日報ＤＢ更新
            Dim SQLStr As String =
                        " UPDATE L0001_TOKEI        " _
                      & " SET DELFLG         = '1'  " _
                      & "   , UPDYMD         = @P05 " _
                      & "   , UPDUSER        = @P06 " _
                      & "   , UPDTERMID      = @P07 " _
                      & "   , RECEIVEYMD     = @P08 " _
                      & " WHERE CAMPCODE     = @P01 " _
                      & "   and DENTYPE      = @P02 " _
                      & "   and NACSHUKODATE = @P03 " _
                      & "   and KEYSTAFFCODE = @P04 " _
                      & "   and DELFLG      <> '1'  "
            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)

                For Each T0005row As DataRow In WW_T00052L0001DELtbl.Rows
                    If T0005row("HDKBN") = "H" Then
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
                            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI")

                            CS0011LOGWRITE.INFSUBCLASS = "L0001_Delete"                 'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:UPDATE L0001_TOKEI"            '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                            Exit Sub
                        End Try
                    End If
                Next
            End Using
            ' L00001統計ＤＢ
            Dim L00001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L00001tbl)
            '〇 L00001統計ＤＢ編集
            T0005COM.EditL00001(WW_T00052L0001SELtbl, L00001tbl, WW_DUMMY)
            '〇 L00001統計ＤＢサマリー
            T0005COM.SumL00001(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, Master.USERID, L00001tbl, WW_DUMMY)

            WW_DATENOW = Date.Now
            For Each L00001row As DataRow In L00001tbl.Rows
                L00001row("INITYMD") = WW_DATENOW                '登録年月日
                L00001row("UPDYMD") = WW_DATENOW                 '更新年月日
                L00001row("UPDUSER") = Master.USERID             '更新ユーザＩＤ
                L00001row("UPDTERMID") = Master.USERTERMID       '更新端末
                L00001row("RECEIVEYMD") = C_DEFAULT_YMD          '集信日時
            Next

            '統計DB出力
            CS0044L1INSERT.SQLCON = SQLcon
            CS0044L1INSERT.CS0044L1Insert(L00001tbl)
            If Not isNormal(CS0044L1INSERT.ERR) Then
                O_RTN = CS0044L1INSERT.ERR
                Exit Sub
            End If

            L00001tbl.Dispose()
            L00001tbl = Nothing
            WW_T00052L0001SELtbl.Dispose()
            WW_T00052L0001SELtbl = Nothing
            WW_T00052L0001DELtbl.Dispose()
            WW_T00052L0001DELtbl = Nothing
            '○GridViewデータをテーブルに保存
            If Not Master.SaveTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

            '○Close
            If Not IsNothing(WW_T0004tbl) Then
                WW_T0004tbl.Dispose()
                WW_T0004tbl = Nothing
            End If
        End Using

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  ﾀﾞｳﾝﾛｰﾄﾞ(EXCEL,PDF出力)・一覧印刷ボタン処理   
    ''' </summary>
    ''' <param name="OutType">出力形式</param>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click(ByVal OutType As String)

        'テーブルデータ 復元
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "CAMPCODE, SHIPORG, TERMKBN, YMD, STAFFCODE, SEQ"
        CS0026TBLSORT.FILTER = "HDKBN='D'"
        Dim WW_TBL As DataTable = CS0026TBLSORT.sort()

        '帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId               'PARAM02:帳票ID
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'PARAM03:プロフID
        CS0030REPORT.FILEtyp = OutType                              'PARAM04:出力ファイル形式
        CS0030REPORT.TBLDATA = WW_TBL                               'PARAM05:データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, "CS0022REPORT", C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

        'Close
        WW_TBL.Dispose()
        WW_TBL = Nothing

    End Sub
    ''' <summary>
    ''' 終了ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        If work.WF_T5_FROMMAPID.Text.Contains(GRT00005WRKINC.MAPID7) Then
            '勤怠画面から遷移して来た場合
            Master.MAPvariant = work.WF_T5_FROMMAPVARIANT.Text

            '★★★ 画面遷移 ★★★
            Master.TransitionPrevPage()
        Else
            '日報一覧画面から遷移して来た場合
            Master.MAPID = GRT00005WRKINC.MAPID
            Master.TransitionPrevPage()
        End If
    End Sub
    ''' <summary>
    ''' 先頭頁ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○データリカバリ 
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○データリカバリ 
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ソート
        Using WW_TBLview As DataView = New DataView(T0005INPtbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '最終頁に移動
            If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
            Else
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
            End If
        End Using
    End Sub

    ''' <summary>
    ''' 詳細一覧　更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '■■■ 項目チェック準備 ■■■
        rightview.SetErrorReport("")
        '○データリカバリ 
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○DetailBoxをT0005INPRowへ退避
        Dim T0005INProw As DataRow = DetailBoxToT0005INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        '■■■ 項目チェック ■■■
        '○ 初期処理
        WW_ERRLIST = New List(Of String)

        'T0005INPtbl内容　チェック
        '　※チェックOKデータをT0005tblへ格納する
        If T0005INProw("WORKKBN") = "A1" Then T0005INProw("CTRL") = "ON1"

        CheckInputRowData(T0005INProw, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        End If

        '■■■ GridView更新 ■■■
        '○チェック"1023"以外はデータ(T0005INProw)を一覧(T0005tbl)へ反映
        If WW_ERRCODE <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            UpdateListTable(T0005INProw, WW_ERRCODE)
            CheckListTable(WW_ERRCODE)
            If Not isNormal(WW_ERRCODE) Then
                Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
            End If

            For Each row As DataRow In T0005INPtbl.Rows
                If row("HDKBN") = "H" Then Continue For
                '〇単項目チェック
                CheckInputRowData(row, WW_ERRCODE)
                '〇エラー処理（ワーニングは対象外）
                If Not isNormal(WW_ERRCODE) AndAlso WW_ERRCODE <> C_MESSAGE_NO.WORNING_RECORD_EXIST Then
                    row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ABORT)
                End If
            Next
        End If

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○Detailクリア
        If WW_ERRCODE <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then WF_CLEAR_Click()
        '〇ヘッダーの再表示
        If isNormal(WW_ERRCODE) Then
            For Each row As DataRow In T0005INPtbl.Rows
                If row("HDKBN") = "H" Then
                    WF_SOUDISTANCE.Text = row("SOUDISTANCE")
                    WF_STDATE.Text = row("STDATE")
                    WF_STTIME.Text = row("STTIME")
                    WF_ENDDATE.Text = row("ENDDATE")
                    WF_ENDTIME.Text = row("ENDTIME")
                    WF_TOTALTOLL.Text = row("TOTALTOLL")
                    WF_STAFFCODE.Text = row("STAFFCODE")
                    WF_STAFFCODE_TEXT.Text = row("STAFFNAMES")
                    WF_WORKTIME.Text = row("WORKTIME")
                    WF_KYUYU.Text = row("KYUYU")
                    CodeToName("DELFLG", WF_DELFLG_H.Text, WF_DELFLG_H_TEXT.Text, WW_DUMMY)
                End If
            Next
        End If

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_FIELD.Value = "WF_YMD"
        WF_YMD.Focus()

    End Sub
    ''' <summary>
    '''  detailbox クリアボタン処理   
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○データリカバリ 
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '〇OPERATION欄（INDEX:1)の選択時処理
        For Each row As DataRow In T0005INPtbl.Rows
            Select Case row(1)
                Case C_LIST_OPERATION_CODE.NODATA
                    row(1) = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    row(1) = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    row(1) = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    row(1) = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    row(1) = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○detailboxヘッダークリア
        WF_LINECNT.Text = String.Empty
        WF_SEQ.Text = String.Empty
        WF_DELFLG.Text = String.Empty
        WF_DELFLG_TEXT.Text = String.Empty
        WF_OILTYPE.Text = String.Empty
        WF_OILTYPE_TEXT.Text = String.Empty
        WF_PRODUCT1.Text = String.Empty
        WF_PRODUCT1_TEXT.Text = String.Empty

        '○Detail初期設定
        InitialRepeater()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_STDATE"
        WF_STDATE.Focus()

    End Sub

    ''' <summary>
    ''' キャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = String.Empty
        WF_LeftboxOpen.Value = String.Empty
        WF_LeftMViewChange.Value = String.Empty

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()

        Dim WW_LEFTVIEW As Integer = -1
        '〇LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WW_LEFTVIEW)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_YMD"
                            .WF_Calendar.Text = WF_YMD.Text
                        Case Else
                            Dim WW_DATE As String = String.Empty
                            FindRepeaterItem(WF_FIELD.Value, WW_DATE)
                            .WF_Calendar.Text = WW_DATE
                    End Select
                    .ActiveCalendar()
                ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CARCODE Then
                    Dim prmData As Hashtable = Nothing
                    If WF_FIELD_REP.Value = "TSHABANF" Then
                        prmData = work.CreateCarCodeParam(work.WF_SEL_CAMPCODE.Text, True)
                    ElseIf WF_FIELD_REP.Value = "TSHABANB" OrElse WF_FIELD_REP.Value = "TSHABANB2" Then
                        prmData = work.CreateCarCodeParam(work.WF_SEL_CAMPCODE.Text, False)
                    Else
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    End If
                    .SetTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveTable()
                    WF_LeftboxOpen.Value = "OpenCTbl"
                ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_STAFFCODE Then
                    '従業員の場合テーブル表記する
                    Dim prmData As Hashtable = work.CreateSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, WF_YMD.Text, WF_YMD.Text)
                    .SetTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveTable()
                    WF_LeftboxOpen.Value = "OpenSTbl"
                Else

                    Dim prmData As Hashtable = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            If WF_FIELD_REP.Value = "WF_OORG" Then
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.UPDATE)
                            Else
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                            End If
                        Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                            If WF_FIELD_REP.Value = "TORICODE" Then
                                prmData = work.CreateCustomerParam(work.WF_SEL_CAMPCODE.Text)
                            Else
                                prmData = work.CreateDemandParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                            End If
                            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DISPLAY_FORMAT) = GL0006GoodsList.C_VIEW_FORMAT_PATTERN.BOTH
                        Case LIST_BOX_CLASSIFICATION.LC_DISTINATION
                            Dim SHIPCODE As String = String.Empty
                            FindRepeaterItem("TORICODE", SHIPCODE)
                            If WF_FIELD_REP.Value = "TODOKECODE" Then
                                prmData = work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, SHIPCODE, "1")
                            Else
                                prmData = work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, SHIPCODE, "2")
                            End If
                            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DISPLAY_FORMAT) = GL0006GoodsList.C_VIEW_FORMAT_PATTERN.BOTH
                        Case LIST_BOX_CLASSIFICATION.LC_GOODS
                            prmData = work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DISPLAY_FORMAT) = GL0006GoodsList.C_VIEW_FORMAT_PATTERN.BOTH
                        Case LIST_BOX_CLASSIFICATION.LC_WORKLORRY
                            prmData = work.CreateWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                        Case Else
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, WW_LEFTVIEW)
                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()

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
        WF_WORKKBN_OnChange()
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
            rightview.SelectIndex(WF_RightViewChange.Value)
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
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()
        '○画面表示データ復元
        If IsNothing(T0005tbl) Then If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If IsNothing(T0005WEEKtbl) Then If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        '○データリカバリ
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()
        '○画面表示データ復元
        If IsNothing(T0005tbl) Then If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '○データリカバリ
        If IsNothing(T0005WEEKtbl) Then If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub
        '○データリカバリ
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

    End Sub

    ''' <summary>
    ''' GridView 明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_FILED_OBJ As Object

        '○処理準備
        'テーブルデータ 復元(Xmlファイルより復元)
        If IsNothing(T0005INPtbl) Then If Not Master.RecoverTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        'LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        '■■■ Grid内容(T0005tbl)よりDetail編集 ■■■

        WF_LINECNT.Text = T0005INPtbl.Rows(WW_LINECNT)("LINECNT")
        WF_SEQ.Text = T0005INPtbl.Rows(WW_LINECNT)("SEQ")
        WF_DELFLG.Text = T0005INPtbl.Rows(WW_LINECNT)("DELFLG")
        CodeToName("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT
        WF_OILTYPE.Text = T0005INPtbl.Rows(WW_LINECNT)("OILTYPE1")
        CodeToName("OILTYPE", WF_OILTYPE.Text, WW_TEXT, WW_DUMMY)
        WF_OILTYPE_TEXT.Text = WW_TEXT
        WF_PRODUCT1.Text = T0005INPtbl.Rows(WW_LINECNT)("PRODUCT11")
        CodeToName("PRODUCT1", WF_PRODUCT1.Text, WW_TEXT, WW_DUMMY)
        WF_PRODUCT1_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each item As RepeaterItem In WF_DViewRep1.Items
            For WI_REP_CNT As Integer = 1 To CONST_DETAIL_REP_COLUMN
                '左
                WW_FILED_OBJ = CType(item.FindControl("WF_Rep1_FIELD_" & WI_REP_CNT), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = FormatRepeaterItem(WW_FILED_OBJ.text, T0005INPtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(item.FindControl("WF_Rep1_VALUE_" & WI_REP_CNT), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CodeToName(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                    CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & WI_REP_CNT), Label).Text = WW_TEXT
                End If
            Next
        Next

        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each row As DataRow In T0005INPtbl.Rows
            Select Case row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        For Each row As DataRow In T0005INPtbl.Rows
            If row("LINECNT") = WF_GridDBclick.Text Then
                Select Case row("OPERATION")
                    Case C_LIST_OPERATION_CODE.NODATA
                        row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.NODISP
                        row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.SELECTED
                        row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.UPDATING
                        row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    Case C_LIST_OPERATION_CODE.ERRORED
                        row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    Case Else
                End Select
            End If
        Next

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        '〇フィールドの入力可否設定
        WF_WORKKBN_OnChange()
    End Sub

    ''' <summary>
    ''' 作業区分変更時処理 (入力項目制御)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_WORKKBN_OnChange()

        Dim WW_FIELD As Object = Nothing
        Dim WW_WORKKBN As Object = Nothing
        For Each item As RepeaterItem In WF_DViewRep1.Items
            For WI_REP_CNT As Integer = 1 To CONST_DETAIL_REP_COLUMN
                WW_FIELD = CType(item.FindControl("WF_Rep1_FIELD_" & WI_REP_CNT), System.Web.UI.WebControls.Label)
                If WW_FIELD.text = "WORKKBN" Then
                    WW_WORKKBN = CType(item.FindControl("WF_Rep1_VALUE_" & WI_REP_CNT), System.Web.UI.WebControls.TextBox)
                End If
            Next
        Next
        '〇作業区分に応じて詳細の入力項目を制御する
        Select Case WW_WORKKBN.text
            Case "A1", "Z1"
                '始業、終業
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", False, "N")
                IsEntryRepeaterItem("SHUKADATE", False, "N")
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", False, "N")
                IsEntryRepeaterItem("DROPNO", False, "N")
                IsEntryRepeaterItem("DETAILNO", False, "N")
                IsEntryRepeaterItem("TORICODE", False, "N")
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", False, "N")
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", False, "N")
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", False, "N")
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", False, "N")
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", False, "Y")
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEF", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB2", False, "Y")
                IsEntryRepeaterItem("TSHABANF", False, "Y")
                IsEntryRepeaterItem("TSHABANB", False, "Y")
                IsEntryRepeaterItem("TSHABANB2", False, "Y")
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", False, "N")
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", False, "N")
                IsEntryRepeaterItem("OILTYPE2", False, "N")
                IsEntryRepeaterItem("OILTYPE3", False, "N")
                IsEntryRepeaterItem("OILTYPE4", False, "N")
                IsEntryRepeaterItem("OILTYPE5", False, "N")
                IsEntryRepeaterItem("OILTYPE6", False, "N")
                IsEntryRepeaterItem("OILTYPE7", False, "N")
                IsEntryRepeaterItem("OILTYPE8", False, "N")
                IsEntryRepeaterItem("PRODUCT11", False, "N")
                IsEntryRepeaterItem("PRODUCT12", False, "N")
                IsEntryRepeaterItem("PRODUCT13", False, "N")
                IsEntryRepeaterItem("PRODUCT14", False, "N")
                IsEntryRepeaterItem("PRODUCT15", False, "N")
                IsEntryRepeaterItem("PRODUCT16", False, "N")
                IsEntryRepeaterItem("PRODUCT17", False, "N")
                IsEntryRepeaterItem("PRODUCT18", False, "N")
                IsEntryRepeaterItem("PRODUCT21", False, "N")
                IsEntryRepeaterItem("PRODUCT22", False, "N")
                IsEntryRepeaterItem("PRODUCT23", False, "N")
                IsEntryRepeaterItem("PRODUCT24", False, "N")
                IsEntryRepeaterItem("PRODUCT25", False, "N")
                IsEntryRepeaterItem("PRODUCT26", False, "N")
                IsEntryRepeaterItem("PRODUCT27", False, "N")
                IsEntryRepeaterItem("PRODUCT28", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE1", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE2", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE3", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE4", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE5", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE6", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE7", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE8", False, "N")
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", False, "N")
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", False, "Y")
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", False, "N")
                IsEntryRepeaterItem("SURYO2", False, "N")
                IsEntryRepeaterItem("SURYO3", False, "N")
                IsEntryRepeaterItem("SURYO4", False, "N")
                IsEntryRepeaterItem("SURYO5", False, "N")
                IsEntryRepeaterItem("SURYO6", False, "N")
                IsEntryRepeaterItem("SURYO7", False, "N")
                IsEntryRepeaterItem("SURYO8", False, "N")
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", False, "N")
                IsEntryRepeaterItem("RUIDISTANCE", False, "N")
                IsEntryRepeaterItem("STMATER", False, "N")
                IsEntryRepeaterItem("ENDMATER", False, "N")
                IsEntryRepeaterItem("JIDISTANCE", False, "N")
                IsEntryRepeaterItem("KUDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPJIDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPKUDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSJIDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSKUDISTANCE", False, "N")
                IsEntryRepeaterItem("CASH", False, "N")
                IsEntryRepeaterItem("ETC", False, "N")
                IsEntryRepeaterItem("TICKET", False, "N")
                IsEntryRepeaterItem("PRATE", False, "N")
                IsEntryRepeaterItem("TOTALTOLL", False, "N")
                IsEntryRepeaterItem("KYUYU", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
            Case "B2"
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", False, "N")
                IsEntryRepeaterItem("SHUKADATE", True)
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", True)
                IsEntryRepeaterItem("DROPNO", False, "N")
                IsEntryRepeaterItem("DETAILNO", False, "N")
                IsEntryRepeaterItem("TORICODE", False, "N")
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", False, "N")
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", False, "N")
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", False, "N")
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", True)
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", False, "Y")
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEF", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB2", False, "Y")
                IsEntryRepeaterItem("TSHABANF", False, "Y")
                IsEntryRepeaterItem("TSHABANB", False, "Y")
                IsEntryRepeaterItem("TSHABANB2", False, "Y")
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", True)
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", False, "N")
                IsEntryRepeaterItem("OILTYPE2", False, "N")
                IsEntryRepeaterItem("OILTYPE3", False, "N")
                IsEntryRepeaterItem("OILTYPE4", False, "N")
                IsEntryRepeaterItem("OILTYPE5", False, "N")
                IsEntryRepeaterItem("OILTYPE6", False, "N")
                IsEntryRepeaterItem("OILTYPE7", False, "N")
                IsEntryRepeaterItem("OILTYPE8", False, "N")
                IsEntryRepeaterItem("PRODUCT11", False, "N")
                IsEntryRepeaterItem("PRODUCT12", False, "N")
                IsEntryRepeaterItem("PRODUCT13", False, "N")
                IsEntryRepeaterItem("PRODUCT14", False, "N")
                IsEntryRepeaterItem("PRODUCT15", False, "N")
                IsEntryRepeaterItem("PRODUCT16", False, "N")
                IsEntryRepeaterItem("PRODUCT17", False, "N")
                IsEntryRepeaterItem("PRODUCT18", False, "N")
                IsEntryRepeaterItem("PRODUCT21", False, "N")
                IsEntryRepeaterItem("PRODUCT22", False, "N")
                IsEntryRepeaterItem("PRODUCT23", False, "N")
                IsEntryRepeaterItem("PRODUCT24", False, "N")
                IsEntryRepeaterItem("PRODUCT25", False, "N")
                IsEntryRepeaterItem("PRODUCT26", False, "N")
                IsEntryRepeaterItem("PRODUCT27", False, "N")
                IsEntryRepeaterItem("PRODUCT28", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE1", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE2", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE3", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE4", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE5", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE6", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE7", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE8", False, "N")
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", False, "N")
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", False, "Y")
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", False, "N")
                IsEntryRepeaterItem("SURYO2", False, "N")
                IsEntryRepeaterItem("SURYO3", False, "N")
                IsEntryRepeaterItem("SURYO4", False, "N")
                IsEntryRepeaterItem("SURYO5", False, "N")
                IsEntryRepeaterItem("SURYO6", False, "N")
                IsEntryRepeaterItem("SURYO7", False, "N")
                IsEntryRepeaterItem("SURYO8", False, "N")
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", True)
                IsEntryRepeaterItem("RUIDISTANCE", True)
                IsEntryRepeaterItem("STMATER", False, "N")
                IsEntryRepeaterItem("ENDMATER", False, "N")
                IsEntryRepeaterItem("JIDISTANCE", True)
                IsEntryRepeaterItem("KUDISTANCE", True)
                IsEntryRepeaterItem("IPPDISTANCE", True)
                IsEntryRepeaterItem("KOSDISTANCE", True)
                IsEntryRepeaterItem("IPPJIDISTANCE", True)
                IsEntryRepeaterItem("IPPKUDISTANCE", True)
                IsEntryRepeaterItem("KOSJIDISTANCE", True)
                IsEntryRepeaterItem("KOSKUDISTANCE", True)
                IsEntryRepeaterItem("CASH", False, "N")
                IsEntryRepeaterItem("ETC", False, "N")
                IsEntryRepeaterItem("TICKET", False, "N")
                IsEntryRepeaterItem("PRATE", False, "N")
                IsEntryRepeaterItem("TOTALTOLL", False, "N")
                IsEntryRepeaterItem("KYUYU", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
            Case "B3"
                '荷卸
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", True)
                IsEntryRepeaterItem("SHUKADATE", True)
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", True)
                IsEntryRepeaterItem("DROPNO", True)
                IsEntryRepeaterItem("DETAILNO", False, "N")
                IsEntryRepeaterItem("TORICODE", True)
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", True)
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", True)
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", True)
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", True)
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", False, "Y")
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEF", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB2", False, "Y")
                IsEntryRepeaterItem("TSHABANF", False, "Y")
                IsEntryRepeaterItem("TSHABANB", False, "Y")
                IsEntryRepeaterItem("TSHABANB2", False, "Y")
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", True)
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", True)
                IsEntryRepeaterItem("OILTYPE2", True)
                IsEntryRepeaterItem("OILTYPE3", True)
                IsEntryRepeaterItem("OILTYPE4", True)
                IsEntryRepeaterItem("OILTYPE5", True)
                IsEntryRepeaterItem("OILTYPE6", True)
                IsEntryRepeaterItem("OILTYPE7", True)
                IsEntryRepeaterItem("OILTYPE8", True)
                IsEntryRepeaterItem("PRODUCT11", True)
                IsEntryRepeaterItem("PRODUCT12", True)
                IsEntryRepeaterItem("PRODUCT13", True)
                IsEntryRepeaterItem("PRODUCT14", True)
                IsEntryRepeaterItem("PRODUCT15", True)
                IsEntryRepeaterItem("PRODUCT16", True)
                IsEntryRepeaterItem("PRODUCT17", True)
                IsEntryRepeaterItem("PRODUCT18", True)
                IsEntryRepeaterItem("PRODUCT21", True)
                IsEntryRepeaterItem("PRODUCT22", True)
                IsEntryRepeaterItem("PRODUCT23", True)
                IsEntryRepeaterItem("PRODUCT24", True)
                IsEntryRepeaterItem("PRODUCT25", True)
                IsEntryRepeaterItem("PRODUCT26", True)
                IsEntryRepeaterItem("PRODUCT27", True)
                IsEntryRepeaterItem("PRODUCT28", True)
                IsEntryRepeaterItem("PRODUCTCODE1", True)
                IsEntryRepeaterItem("PRODUCTCODE2", True)
                IsEntryRepeaterItem("PRODUCTCODE3", True)
                IsEntryRepeaterItem("PRODUCTCODE4", True)
                IsEntryRepeaterItem("PRODUCTCODE5", True)
                IsEntryRepeaterItem("PRODUCTCODE6", True)
                IsEntryRepeaterItem("PRODUCTCODE7", True)
                IsEntryRepeaterItem("PRODUCTCODE8", True)
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", True)
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", False, "Y")
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", True)
                IsEntryRepeaterItem("SURYO2", True)
                IsEntryRepeaterItem("SURYO3", True)
                IsEntryRepeaterItem("SURYO4", True)
                IsEntryRepeaterItem("SURYO5", True)
                IsEntryRepeaterItem("SURYO6", True)
                IsEntryRepeaterItem("SURYO7", True)
                IsEntryRepeaterItem("SURYO8", True)
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", True)
                IsEntryRepeaterItem("RUIDISTANCE", True)
                IsEntryRepeaterItem("STMATER", False, "N")
                IsEntryRepeaterItem("ENDMATER", False, "N")
                IsEntryRepeaterItem("JIDISTANCE", True)
                IsEntryRepeaterItem("KUDISTANCE", True)
                IsEntryRepeaterItem("IPPDISTANCE", True)
                IsEntryRepeaterItem("KOSDISTANCE", True)
                IsEntryRepeaterItem("IPPJIDISTANCE", True)
                IsEntryRepeaterItem("IPPKUDISTANCE", True)
                IsEntryRepeaterItem("KOSJIDISTANCE", True)
                IsEntryRepeaterItem("KOSKUDISTANCE", True)
                IsEntryRepeaterItem("CASH", False, "N")
                IsEntryRepeaterItem("ETC", False, "N")
                IsEntryRepeaterItem("TICKET", False, "N")
                IsEntryRepeaterItem("PRATE", False, "N")
                IsEntryRepeaterItem("TOTALTOLL", False, "N")
                IsEntryRepeaterItem("KYUYU", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
            Case "B4", "B5", "B9", "BA", "BB", "BX", "BY", "G1"
                '点検
                '洗車
                '他手待
                '待機
                '休憩
                '他作業
                '配送作業
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", False, "N")
                IsEntryRepeaterItem("SHUKADATE", False, "N")
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", False, "N")
                IsEntryRepeaterItem("DROPNO", False, "N")
                IsEntryRepeaterItem("DETAILNO", False, "N")
                If WW_WORKKBN.text = "BY" Then
                    IsEntryRepeaterItem("TORICODE", True)
                Else
                    IsEntryRepeaterItem("TORICODE", False, "N")
                End If
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", False, "N")
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", False, "N")
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", False, "N")
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", False, "N")
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", False, "Y")
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEF", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB2", False, "Y")
                IsEntryRepeaterItem("TSHABANF", False, "Y")
                IsEntryRepeaterItem("TSHABANB", False, "Y")
                IsEntryRepeaterItem("TSHABANB2", False, "Y")
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", False, "N")
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", False, "N")
                IsEntryRepeaterItem("OILTYPE2", False, "N")
                IsEntryRepeaterItem("OILTYPE3", False, "N")
                IsEntryRepeaterItem("OILTYPE4", False, "N")
                IsEntryRepeaterItem("OILTYPE5", False, "N")
                IsEntryRepeaterItem("OILTYPE6", False, "N")
                IsEntryRepeaterItem("OILTYPE7", False, "N")
                IsEntryRepeaterItem("OILTYPE8", False, "N")
                IsEntryRepeaterItem("PRODUCT11", False, "N")
                IsEntryRepeaterItem("PRODUCT12", False, "N")
                IsEntryRepeaterItem("PRODUCT13", False, "N")
                IsEntryRepeaterItem("PRODUCT14", False, "N")
                IsEntryRepeaterItem("PRODUCT15", False, "N")
                IsEntryRepeaterItem("PRODUCT16", False, "N")
                IsEntryRepeaterItem("PRODUCT17", False, "N")
                IsEntryRepeaterItem("PRODUCT18", False, "N")
                IsEntryRepeaterItem("PRODUCT21", False, "N")
                IsEntryRepeaterItem("PRODUCT22", False, "N")
                IsEntryRepeaterItem("PRODUCT23", False, "N")
                IsEntryRepeaterItem("PRODUCT24", False, "N")
                IsEntryRepeaterItem("PRODUCT25", False, "N")
                IsEntryRepeaterItem("PRODUCT26", False, "N")
                IsEntryRepeaterItem("PRODUCT27", False, "N")
                IsEntryRepeaterItem("PRODUCT28", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE1", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE2", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE3", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE4", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE5", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE6", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE7", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE8", False, "N")
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", False, "N")
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", False, "Y")
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", False, "N")
                IsEntryRepeaterItem("SURYO2", False, "N")
                IsEntryRepeaterItem("SURYO3", False, "N")
                IsEntryRepeaterItem("SURYO4", False, "N")
                IsEntryRepeaterItem("SURYO5", False, "N")
                IsEntryRepeaterItem("SURYO6", False, "N")
                IsEntryRepeaterItem("SURYO7", False, "N")
                IsEntryRepeaterItem("SURYO8", False, "N")
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", True)
                IsEntryRepeaterItem("RUIDISTANCE", True)
                IsEntryRepeaterItem("STMATER", False, "N")
                IsEntryRepeaterItem("ENDMATER", False, "N")
                IsEntryRepeaterItem("JIDISTANCE", False, "N")
                IsEntryRepeaterItem("KUDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPJIDISTANCE", False, "N")
                IsEntryRepeaterItem("IPPKUDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSJIDISTANCE", False, "N")
                IsEntryRepeaterItem("KOSKUDISTANCE", False, "N")
                IsEntryRepeaterItem("CASH", False, "N")
                IsEntryRepeaterItem("ETC", False, "N")
                IsEntryRepeaterItem("TICKET", False, "N")
                IsEntryRepeaterItem("PRATE", False, "N")
                IsEntryRepeaterItem("TOTALTOLL", False, "N")
                IsEntryRepeaterItem("KYUYU", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
            Case "F1"
                '出庫
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", False, "N")
                IsEntryRepeaterItem("SHUKADATE", False, "N")
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", False, "N")
                IsEntryRepeaterItem("DROPNO", False, "N")
                IsEntryRepeaterItem("DETAILNO", False, "N")
                IsEntryRepeaterItem("TORICODE", False, "N")
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", False, "N")
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", False, "N")
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", False, "N")
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", False, "N")
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", True)
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", True)
                IsEntryRepeaterItem("SHARYOTYPEF", True)
                IsEntryRepeaterItem("SHARYOTYPEB", True)
                IsEntryRepeaterItem("SHARYOTYPEB2", True)
                IsEntryRepeaterItem("TSHABANF", True)
                IsEntryRepeaterItem("TSHABANB", True)
                IsEntryRepeaterItem("TSHABANB2", True)
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", False, "N")
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", False, "N")
                IsEntryRepeaterItem("OILTYPE2", False, "N")
                IsEntryRepeaterItem("OILTYPE3", False, "N")
                IsEntryRepeaterItem("OILTYPE4", False, "N")
                IsEntryRepeaterItem("OILTYPE5", False, "N")
                IsEntryRepeaterItem("OILTYPE6", False, "N")
                IsEntryRepeaterItem("OILTYPE7", False, "N")
                IsEntryRepeaterItem("OILTYPE8", False, "N")
                IsEntryRepeaterItem("PRODUCT11", False, "N")
                IsEntryRepeaterItem("PRODUCT12", False, "N")
                IsEntryRepeaterItem("PRODUCT13", False, "N")
                IsEntryRepeaterItem("PRODUCT14", False, "N")
                IsEntryRepeaterItem("PRODUCT15", False, "N")
                IsEntryRepeaterItem("PRODUCT16", False, "N")
                IsEntryRepeaterItem("PRODUCT17", False, "N")
                IsEntryRepeaterItem("PRODUCT18", False, "N")
                IsEntryRepeaterItem("PRODUCT21", False, "N")
                IsEntryRepeaterItem("PRODUCT22", False, "N")
                IsEntryRepeaterItem("PRODUCT23", False, "N")
                IsEntryRepeaterItem("PRODUCT24", False, "N")
                IsEntryRepeaterItem("PRODUCT25", False, "N")
                IsEntryRepeaterItem("PRODUCT26", False, "N")
                IsEntryRepeaterItem("PRODUCT27", False, "N")
                IsEntryRepeaterItem("PRODUCT28", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE1", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE2", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE3", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE4", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE5", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE6", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE7", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE8", False, "N")
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", False, "N")
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", True)
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", False, "N")
                IsEntryRepeaterItem("SURYO2", False, "N")
                IsEntryRepeaterItem("SURYO3", False, "N")
                IsEntryRepeaterItem("SURYO4", False, "N")
                IsEntryRepeaterItem("SURYO5", False, "N")
                IsEntryRepeaterItem("SURYO6", False, "N")
                IsEntryRepeaterItem("SURYO7", False, "N")
                IsEntryRepeaterItem("SURYO8", False, "N")
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", True)
                IsEntryRepeaterItem("RUIDISTANCE", True)
                IsEntryRepeaterItem("STMATER", True)
                IsEntryRepeaterItem("ENDMATER", False, "N")
                IsEntryRepeaterItem("JIDISTANCE", True)
                IsEntryRepeaterItem("KUDISTANCE", True)
                IsEntryRepeaterItem("IPPDISTANCE", True)
                IsEntryRepeaterItem("KOSDISTANCE", True)
                IsEntryRepeaterItem("IPPJIDISTANCE", True)
                IsEntryRepeaterItem("IPPKUDISTANCE", True)
                IsEntryRepeaterItem("KOSJIDISTANCE", True)
                IsEntryRepeaterItem("KOSKUDISTANCE", True)
                IsEntryRepeaterItem("CASH", False, "N")
                IsEntryRepeaterItem("ETC", False, "N")
                IsEntryRepeaterItem("TICKET", False, "N")
                IsEntryRepeaterItem("PRATE", False, "N")
                IsEntryRepeaterItem("TOTALTOLL", False, "N")
                IsEntryRepeaterItem("KYUYU", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
            Case "F3"
                '帰庫
                IsEntryRepeaterItem("CAMPCODE", True)
                IsEntryRepeaterItem("CAMPNAMES", False, "N")
                IsEntryRepeaterItem("YMD", True)
                IsEntryRepeaterItem("TODOKEDATE", False, "N")
                IsEntryRepeaterItem("SHUKADATE", False, "N")
                IsEntryRepeaterItem("SHIPORG", True)
                IsEntryRepeaterItem("SHIPORGNAMES", False, "N")
                IsEntryRepeaterItem("TRIPNO", False, "N")
                IsEntryRepeaterItem("DROPNO", False, "N")
                IsEntryRepeaterItem("DETAILNO", False, "N")
                IsEntryRepeaterItem("TORICODE", False, "N")
                IsEntryRepeaterItem("TORINAMES", False, "N")
                IsEntryRepeaterItem("URIKBN", False, "N")
                IsEntryRepeaterItem("URIKBNNAMES", False, "N")
                IsEntryRepeaterItem("STORICODE", False, "N")
                IsEntryRepeaterItem("STORINAMES", False, "N")
                IsEntryRepeaterItem("TODOKECODE", False, "N")
                IsEntryRepeaterItem("TODOKENAMES", False, "N")
                IsEntryRepeaterItem("SHUKABASHO", False, "N")
                IsEntryRepeaterItem("SHUKABASHONAMES", False, "N")
                IsEntryRepeaterItem("STAFFCODE", True)
                IsEntryRepeaterItem("STAFFNAMES", False, "N")
                IsEntryRepeaterItem("SUBSTAFFCODE", False, "N")
                IsEntryRepeaterItem("SUBSTAFFNAMES", False, "N")
                IsEntryRepeaterItem("CREWKBN", False, "Y")
                IsEntryRepeaterItem("CREWKBNNAMES", False, "N")
                IsEntryRepeaterItem("GSHABAN", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEF", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB", False, "Y")
                IsEntryRepeaterItem("SHARYOTYPEB2", False, "Y")
                IsEntryRepeaterItem("TSHABANF", False, "Y")
                IsEntryRepeaterItem("TSHABANB", False, "Y")
                IsEntryRepeaterItem("TSHABANB2", False, "Y")
                IsEntryRepeaterItem("GSHABANLICNPLTNO", False, "N")
                IsEntryRepeaterItem("CONTCHASSIS", False, "N")
                IsEntryRepeaterItem("CONTCHASSISLICNPLTNO", False, "N")
                IsEntryRepeaterItem("OILTYPE1", False, "N")
                IsEntryRepeaterItem("OILTYPE2", False, "N")
                IsEntryRepeaterItem("OILTYPE3", False, "N")
                IsEntryRepeaterItem("OILTYPE4", False, "N")
                IsEntryRepeaterItem("OILTYPE5", False, "N")
                IsEntryRepeaterItem("OILTYPE6", False, "N")
                IsEntryRepeaterItem("OILTYPE7", False, "N")
                IsEntryRepeaterItem("OILTYPE8", False, "N")
                IsEntryRepeaterItem("PRODUCT11", False, "N")
                IsEntryRepeaterItem("PRODUCT12", False, "N")
                IsEntryRepeaterItem("PRODUCT13", False, "N")
                IsEntryRepeaterItem("PRODUCT14", False, "N")
                IsEntryRepeaterItem("PRODUCT15", False, "N")
                IsEntryRepeaterItem("PRODUCT16", False, "N")
                IsEntryRepeaterItem("PRODUCT17", False, "N")
                IsEntryRepeaterItem("PRODUCT18", False, "N")
                IsEntryRepeaterItem("PRODUCT21", False, "N")
                IsEntryRepeaterItem("PRODUCT22", False, "N")
                IsEntryRepeaterItem("PRODUCT23", False, "N")
                IsEntryRepeaterItem("PRODUCT24", False, "N")
                IsEntryRepeaterItem("PRODUCT25", False, "N")
                IsEntryRepeaterItem("PRODUCT26", False, "N")
                IsEntryRepeaterItem("PRODUCT27", False, "N")
                IsEntryRepeaterItem("PRODUCT28", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE1", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE2", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE3", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE4", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE5", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE6", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE7", False, "N")
                IsEntryRepeaterItem("PRODUCTCODE8", False, "N")
                IsEntryRepeaterItem("PRODUCT1NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT2NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT3NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT4NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT5NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT6NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT7NAMES", False, "N")
                IsEntryRepeaterItem("PRODUCT8NAMES", False, "N")
                IsEntryRepeaterItem("TAXKBN", False, "N")
                IsEntryRepeaterItem("TAXKBNNAMES", False, "N")
                IsEntryRepeaterItem("NIPPONO", False, "Y")
                IsEntryRepeaterItem("WORKKBN", True)
                IsEntryRepeaterItem("WORKKBNNAMES", False, "N")
                IsEntryRepeaterItem("STDATE", True)
                IsEntryRepeaterItem("STTIME", True)
                IsEntryRepeaterItem("ENDDATE", True)
                IsEntryRepeaterItem("ENDTIME", True)
                IsEntryRepeaterItem("WORKTIME", False, "N")
                IsEntryRepeaterItem("MOVETIME", False, "N")
                IsEntryRepeaterItem("ACTTIME", False, "N")
                IsEntryRepeaterItem("SURYO1", False, "N")
                IsEntryRepeaterItem("SURYO2", False, "N")
                IsEntryRepeaterItem("SURYO3", False, "N")
                IsEntryRepeaterItem("SURYO4", False, "N")
                IsEntryRepeaterItem("SURYO5", False, "N")
                IsEntryRepeaterItem("SURYO6", False, "N")
                IsEntryRepeaterItem("SURYO7", False, "N")
                IsEntryRepeaterItem("SURYO8", False, "N")
                IsEntryRepeaterItem("TOTALSURYO", False, "N")
                IsEntryRepeaterItem("STANI1", False, "N")
                IsEntryRepeaterItem("STANI2", False, "N")
                IsEntryRepeaterItem("STANI3", False, "N")
                IsEntryRepeaterItem("STANI4", False, "N")
                IsEntryRepeaterItem("STANI5", False, "N")
                IsEntryRepeaterItem("STANI6", False, "N")
                IsEntryRepeaterItem("STANI7", False, "N")
                IsEntryRepeaterItem("STANI8", False, "N")
                IsEntryRepeaterItem("STANI1NAMES", False, "N")
                IsEntryRepeaterItem("STANI2NAMES", False, "N")
                IsEntryRepeaterItem("STANI3NAMES", False, "N")
                IsEntryRepeaterItem("STANI4NAMES", False, "N")
                IsEntryRepeaterItem("STANI5NAMES", False, "N")
                IsEntryRepeaterItem("STANI6NAMES", False, "N")
                IsEntryRepeaterItem("STANI7NAMES", False, "N")
                IsEntryRepeaterItem("STANI8NAMES", False, "N")
                IsEntryRepeaterItem("SOUDISTANCE", True)
                IsEntryRepeaterItem("RUIDISTANCE", True)
                IsEntryRepeaterItem("STMATER", False, "N")
                IsEntryRepeaterItem("ENDMATER", True)
                IsEntryRepeaterItem("JIDISTANCE", True)
                IsEntryRepeaterItem("KUDISTANCE", True)
                IsEntryRepeaterItem("IPPDISTANCE", True)
                IsEntryRepeaterItem("KOSDISTANCE", True)
                IsEntryRepeaterItem("IPPJIDISTANCE", True)
                IsEntryRepeaterItem("IPPKUDISTANCE", True)
                IsEntryRepeaterItem("KOSJIDISTANCE", True)
                IsEntryRepeaterItem("KOSKUDISTANCE", True)
                IsEntryRepeaterItem("CASH", True)
                IsEntryRepeaterItem("ETC", True)
                IsEntryRepeaterItem("TICKET", True)
                IsEntryRepeaterItem("PRATE", True)
                IsEntryRepeaterItem("TOTALTOLL", True)
                IsEntryRepeaterItem("KYUYU", True)
                IsEntryRepeaterItem("TUMIOKIKBN", False, "N")
                IsEntryRepeaterItem("TUMIOKIKBNNAMES", False, "N")
                IsEntryRepeaterItem("TERMKBN", True)
                IsEntryRepeaterItem("TERMKBNNAMES", False, "N")
                IsEntryRepeaterItem("DELFLG", True)
                IsEntryRepeaterItem("ORDERNO", False, "N")
                IsEntryRepeaterItem("ORDERUMU", False, "N")
                IsEntryRepeaterItem("LATITUDE", False, "N")
                IsEntryRepeaterItem("LONGITUDE", False, "N")
                IsEntryRepeaterItem("SEQ", True)
        End Select

    End Sub

    ''' <summary>
    ''' 入力可否の設定
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_ST">入力可否</param>
    ''' <param name="I_EFFECT">表示可否（入力可否との複合判定）</param>
    ''' <remarks></remarks>
    Protected Sub IsEntryRepeaterItem(ByVal I_FIELD As String, ByVal I_ST As Boolean, Optional ByVal I_EFFECT As String = "N")

        Dim WW_FILED As Object = Nothing
        Dim WW_VALUE As Object = Nothing

        '指定された項目の値を取得
        For Each item As RepeaterItem In WF_DViewRep1.Items
            For idx As Integer = 1 To CONST_DETAIL_REP_COLUMN
                WW_FILED = CType(item.FindControl("WF_Rep1_FIELD_" & idx), Label)
                If WW_FILED.text = I_FIELD Then
                    WW_VALUE = CType(item.FindControl("WF_Rep1_VALUE_" & idx), TextBox)
                    WW_VALUE.enabled = I_ST
                    If I_ST = False AndAlso I_EFFECT = "N" Then
                        CType(item.FindControl("WF_Rep1_VALUE_" & idx), TextBox).Text = ""
                        CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & idx), Label).Text = ""
                    End If
                    Exit Sub
                End If
            Next idx
        Next item

    End Sub

    ''' <summary>
    ''' 明細行特殊編集処理
    ''' </summary>
    ''' <param name="I_FIELD">対象フィールド名</param>
    ''' <param name="I_VALUE">対象の項目値</param>
    ''' <returns>編集後の項目値</returns>
    ''' <remarks></remarks>
    Protected Function FormatRepeaterItem(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        Select Case I_FIELD
            Case "SEQ"
                Try
                    Return Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                    Return I_VALUE
                End Try
            Case Else
                Return I_VALUE
        End Select
    End Function

    ''' <summary>
    ''' 一覧情報初期設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialGridData()

        Dim WW_T0005tbl As DataTable
        Dim WW_WORKTIME As Integer = 0

        '■■■ 画面表示用データ取得 ■■■
        '○処理準備
        '前画面のテーブルデータ 復元(TEXTファイルより復元)
        '○画面表示データ復元
        If IsNothing(T0005tbl) Then If Not Master.RecoverTable(T0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '前画面のテーブルデータ 復元(TEXTファイルより復元)
        If IsNothing(T0005WEEKtbl) Then If Not Master.RecoverTable(T0005WEEKtbl, work.WF_SEL_XMLsaveF9.Text) Then Exit Sub

        Try
            WW_ERRLIST = New List(Of String)

            '対象データ抽出
            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, SEQ"
            '削除データの退避
            CS0026TBLSORT.FILTER = "YMD       = '" & work.WF_T5_YMD.Text & "' and " _
                                 & "STAFFCODE = '" & work.WF_T5_STAFFCODE.Text & "' and " _
                                 & "SELECT    = '1'"
            WW_T0005tbl = CS0026TBLSORT.sort()

            T0005COM.AddColumnT0005tbl(T0005INPtbl)

            For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                Dim T0005INProw As DataRow = T0005INPtbl.NewRow
                T0005INProw.ItemArray = WW_T0005tbl.Rows(i).ItemArray
                T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '※統一車番を１フィールドに連結し格納
                T0005INProw("TSHABANF") = T0005INProw("SHARYOTYPEF") & T0005INProw("TSHABANF")
                T0005INProw("TSHABANB") = T0005INProw("SHARYOTYPEB") & T0005INProw("TSHABANB")
                T0005INProw("TSHABANB2") = T0005INProw("SHARYOTYPEB2") & T0005INProw("TSHABANB2")

                If T0005INProw("HDKBN") = "H" Then
                    WF_YMD.Text = T0005INProw("YMD")
                    WF_SOUDISTANCE.Text = T0005INProw("SOUDISTANCE")
                    WF_STDATE.Text = T0005INProw("STDATE")
                    WF_STTIME.Text = T0005INProw("STTIME")
                    WF_ENDDATE.Text = T0005INProw("ENDDATE")
                    WF_ENDTIME.Text = T0005INProw("ENDTIME")
                    WF_TOTALTOLL.Text = T0005INProw("TOTALTOLL")
                    WF_STAFFCODE.Text = T0005INProw("STAFFCODE")
                    WF_STAFFCODE_TEXT.Text = T0005INProw("STAFFNAMES")
                    WF_WORKTIME.Text = T0005INProw("WORKTIME")
                    WF_KYUYU.Text = T0005INProw("KYUYU")
                    WF_DELFLG_H.Text = T0005INProw("DELFLG")
                    CodeToName("DELFLG", WF_DELFLG_H.Text, WF_DELFLG_H_TEXT.Text, WW_DUMMY)

                ElseIf T0005INProw("HDKBN") = "D" Then
                    '休憩時間
                    If T0005INProw("WORKKBN") = "BB" Then
                        WW_WORKTIME = WW_WORKTIME +
                                      DateDiff("n",
                                              T0005INProw("STDATE") + " " + T0005INProw("STTIME"),
                                              T0005INProw("ENDDATE") + " " + T0005INProw("ENDTIME")
                                             )
                    End If
                End If

                T0005INPtbl.Rows.Add(T0005INProw)

            Next

            WF_BREAKTIME.Text = T0005COM.MinutestoHHMM(WW_WORKTIME)
            WF_Head_LINECNT.Text = work.WF_T5I_LINECNT.Text
            '〇全体チェック処理
            CheckListTable(WW_ERRCODE)
            '〇単項目チェック処理
            For Each T0005INProw As DataRow In T0005INPtbl.Rows
                CheckInputRowData(T0005INProw, WW_ERRCODE)
                If WW_ERRCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
                    Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.WAR)
                ElseIf Not isNormal(WW_ERRCODE) Then
                    T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.WAR)
                End If
            Next

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, "T0005_NIPPO SELECT", C_MESSAGE_TYPE.ABORT)
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '項番(LineCnt)設定
        Dim WW_LINECNT As Integer = 0
        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            'SELECT=0（対象外）1（対象）、HIDDEN=0（表示）1（非表示）
            'ヘッダを対象外、非表示に
            If T0005INPtbl.Rows(i)("HDKBN") = "D" Then
                WW_LINECNT = WW_LINECNT + 1
                T0005INPtbl.Rows(i)("LINECNT") = WW_LINECNT
                T0005INPtbl.Rows(i)("SELECT") = "1"
                T0005INPtbl.Rows(i)("HIDDEN") = "0"
            Else
                T0005INPtbl.Rows(i)("LINECNT") = 0
                T0005INPtbl.Rows(i)("SELECT") = "1"
                T0005INPtbl.Rows(i)("HIDDEN") = "1"
            End If
        Next

        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(T0005INPtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        '■■■ 画面（GridView）表示 ■■■
        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(T0005INPtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRT00005WRKINC.MAPID
            CS0013ProfView.VARI = work.WF_SEL_VIEWID_DTL.Text
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        WW_T0005tbl.Dispose()
        WW_T0005tbl = Nothing

    End Sub

    ''' <summary>
    ''' 明細情報初期設定処理(空明細作成,イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InitialRepeater()
        Using dataTable As DataTable = New DataTable
            '■■■ Detail変数設定 ■■■
            Master.CreateEmptyTable(dataTable, work.WF_SEL_XMLsaveF2.Text)
            dataTable.Rows.Add(dataTable.NewRow())

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = work.WF_SEL_VIEWID_DTL.Text
            CS0052DetailView.SRCDATA = dataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then Exit Sub
            WF_DetailMView.ActiveViewIndex = 0
            '■■■ Detailイベント設定 ■■■
            Dim WW_ATTR As String = ""
            Dim WW_ATTR2 As String = ""
            For row As Integer = 0 To CS0052DetailView.ROWMAX - 1
                For col As Integer = 1 To CS0052DetailView.COLMAX

                    'ダブルクリック時コード検索イベント追加
                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then
                        Dim repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label)
                        Dim repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox)
                        GetFieldAttributes(repField.Text, WW_ATTR, WW_ATTR2)
                        If WW_ATTR <> "" AndAlso repValue.ReadOnly = False Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", WW_ATTR)
                            Dim repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), System.Web.UI.WebControls.Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                        If WW_ATTR2 <> "" AndAlso repValue.ReadOnly = False Then
                            repValue.Attributes.Remove("onchange")
                            repValue.Attributes.Add("onchange", WW_ATTR2)
                        End If
                    End If
                Next col
            Next row
            WF_DViewRep1.Visible = True
        End Using
    End Sub

    ''' <summary>
    '''  LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""
        Dim WW_SelectOIlTYPE As String = ""
        Dim WW_SelectPRODUCT1 As String = ""
        Dim WW_SelectTSHABANF As String = ""
        Dim WW_SelectTSHABANB As String = ""
        Dim WW_SelectTSHABANB2 As String = ""
        Dim WW_SelectTSHABANFTEXT As String = ""
        Dim WW_SelectTSHABANBTEXT As String = ""
        Dim WW_SelectTSHABANB2TEXT As String = ""

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
        End If
        Dim WW_ACTIVE_VALUE As String() = leftview.GetActiveValue
        WW_SelectValue = WW_ACTIVE_VALUE(0)
        WW_SelectTEXT = WW_ACTIVE_VALUE(1)

        Select Case WF_FIELD.Value
            Case "WF_YMD"
                '出庫日 
                WF_YMD.Text = WW_SelectValue
                WF_YMD.Focus()
            Case "WF_STDATE"
                '開始日 
                WF_STDATE.Text = WW_SelectValue
                WF_STDATE.Focus()
            Case "WF_ENDDATE"
                '終了日 
                WF_ENDDATE.Text = WW_SelectValue
                WF_ENDDATE.Focus()
            Case "WF_STAFFCODE"
                '従業員  
                For Each Data As String In WW_ACTIVE_VALUE
                    Select Case Data.Split("=")(0)
                        Case "CODE" : WF_STAFFCODE.Text = Data.Split("=")(1)
                        Case "NAMES" : WF_STAFFCODE_TEXT.Text = Data.Split("=")(1)
                    End Select
                Next
                WF_STAFFCODE.Focus()
            Case "WF_CREWKBN"
                '乗務区分 
                WF_CREWKBN_TEXT.Text = WW_SelectTEXT
                WF_CREWKBN.Text = WW_SelectValue
                WF_CREWKBN.Focus()
            Case "WF_DELFLG_H"
                '削除フラグ（ヘッダ） 
                WF_DELFLG_H_TEXT.Text = WW_SelectTEXT
                WF_DELFLG_H.Text = WW_SelectValue
                WF_DELFLG_H.Focus()
            Case "WF_DELFLG"
                '削除フラグ（明細） 
                WF_DELFLG_TEXT.Text = WW_SelectTEXT
                WF_DELFLG.Text = WW_SelectValue
                WF_DELFLG.Focus()
            Case "WF_OILTYPE"
                '油種（明細） 
                WF_OILTYPE_TEXT.Text = WW_SelectTEXT
                WF_OILTYPE.Text = WW_SelectValue
                WF_OILTYPE.Focus()
            Case "WF_PRODUCT1"
                '品名１（明細） 
                WF_PRODUCT1_TEXT.Text = WW_SelectTEXT
                WF_PRODUCT1.Text = WW_SelectValue
                WF_PRODUCT1.Focus()
        End Select

        If Not String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD_REP.Value
                Case "TSHABANF", "TSHABANB", "TSHABANB2"
                    For Each Data As String In WW_ACTIVE_VALUE
                        Select Case Data.Split("=")(0)
                            Case "TSHABAN" : WW_SelectValue = Data.Split("=")(1)
                            Case "LICNPLTNO" : WW_SelectTEXT = Data.Split("=")(1)
                        End Select
                    Next
                Case "STDATE", "ENDDATE", "SHUKADATE", "TODOKEDATE"
                    WW_SelectTEXT = ""
                Case "PRODUCTCODE1", "PRODUCTCODE2", "PRODUCTCODE3", "PRODUCTCODE4", "PRODUCTCODE5", "PRODUCTCODE6", "PRODUCTCODE7", "PRODUCTCODE8"
                    CodeToName(WF_FIELD_REP.Value.Replace("CODE", "2"), WW_SelectValue, WW_SelectTEXT, WW_DUMMY)
            End Select

            For Each item As RepeaterItem In WF_DViewRep1.Items
                For columidx As Integer = 1 To CONST_DETAIL_REP_COLUMN
                    If CType(item.FindControl("WF_Rep1_FIELD_" & columidx), Label).Text = WF_FIELD_REP.Value Then
                        CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Text = WW_SelectValue
                        CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & columidx), Label).Text = WW_SelectTEXT
                        CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Focus()
                        Exit For
                    End If
                Next
            Next

            If WF_FIELD_REP.Value = "GSHABAN" Then
                Dim list As ListBox = work.CreateWorkLorryList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                Dim GSHABANVALUE As String = String.Empty
                Dim GSHABANTEXT As String = String.Empty
                For Each item As ListItem In list.Items
                    If item.Value.StartsWith(WW_SelectValue) Then
                        GSHABANVALUE = item.Value
                        GSHABANTEXT = item.Text
                        Exit For
                    End If
                Next
                Dim WW_SpritValue() As String = GSHABANVALUE.Split(",")
                Dim WW_SpritText() As String = GSHABANTEXT.Split("　")

                WW_SelectValue = WW_SpritValue(0)
                WW_SelectTSHABANF = WW_SpritValue(1)
                WW_SelectTSHABANB = WW_SpritValue(2)
                WW_SelectTSHABANB2 = WW_SpritValue(3)

                WW_SelectTEXT = WW_SpritText(1)
                WW_SelectTSHABANFTEXT = WW_SpritText(1)
                WW_SelectTSHABANBTEXT = WW_SpritText(2)
                WW_SelectTSHABANB2TEXT = WW_SpritText(3)

                For Each item As RepeaterItem In WF_DViewRep1.Items
                    For columidx As Integer = 1 To CONST_DETAIL_REP_COLUMN
                        Dim WW_FIELD As String = CType(item.FindControl("WF_Rep1_FIELD_" & columidx), Label).Text
                        Dim WW_VALUE As String = CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Text
                        If WW_FIELD = "TSHABANF" Then
                            If String.IsNullOrEmpty(WW_VALUE) Then
                                CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Text = WW_SelectTSHABANF
                                CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & columidx), Label).Text = WW_SelectTSHABANFTEXT
                            End If
                        End If
                        If WW_FIELD = "TSHABANB" Then
                            If String.IsNullOrEmpty(WW_VALUE) Then
                                CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Text = WW_SelectTSHABANB
                                CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & columidx), Label).Text = WW_SelectTSHABANBTEXT
                            End If
                        End If
                        If WW_FIELD = "TSHABANB2" Then
                            If String.IsNullOrEmpty(WW_VALUE) Then
                                CType(item.FindControl("WF_Rep1_VALUE_" & columidx), TextBox).Text = WW_SelectTSHABANB2
                                CType(item.FindControl("WF_Rep1_VALUE_TEXT_" & columidx), Label).Text = WW_SelectTSHABANB2TEXT
                            End If
                        End If
                    Next
                Next
            End If

        End If
        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_FIELD_REP.Value = ""
        WF_FIELD.Value = ""

    End Sub

    ''' <summary>
    ''' イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_ATTR"></param>
    ''' <param name="O_ATTR2"></param>
    ''' <remarks></remarks>
    Protected Sub GetFieldAttributes(ByVal I_FIELD As String, ByRef O_ATTR As String, ByRef O_ATTR2 As String)

        O_ATTR = ""
        O_ATTR2 = ""
        Select Case I_FIELD
            Case "WORKKBN"
                '作業区分
                O_ATTR = "REF_Field_DBclick('WORKKBN', 'WF_Rep_FIELD' , 901);"
                O_ATTR2 = "WorkKbnChange();"
            Case "DELFLG"
                '削除フラグ
                O_ATTR = "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' ," & LIST_BOX_CLASSIFICATION.LC_DELFLG & ");"
            Case "TORICODE"
                '取引先名
                O_ATTR = "REF_Field_DBclick('TORICODE', 'WF_Rep_FIELD' ," & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & ");"
            Case "TODOKECODE"
                '届先名
                O_ATTR = "REF_Field_DBclick('TODOKECODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "SHUKABASHO"
                '出荷場所名
                O_ATTR = "REF_Field_DBclick('SHUKABASHO', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "PRODUCT21", "PRODUCT22", "PRODUCT23", "PRODUCT24", "PRODUCT25", "PRODUCT26", "PRODUCT27", "PRODUCT28" _
               , "PRODUCTCODE1", "PRODUCTCODE2", "PRODUCTCODE3", "PRODUCTCODE4", "PRODUCTCODE5", "PRODUCTCODE6", "PRODUCTCODE7", "PRODUCTCODE8"
                '品名
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "CREWKBN"
                '乗務区分
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , 902);"
            Case "GSHABAN"
                '業務車番
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_WORKLORRY & ");"
            Case "STDATE", "ENDDATE"
                '開始、終了日付
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "TUMIOKIKBN"
                '積置区分
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , 903);"
            Case "URIKBN"
                '売上計上基準
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , 904);"
            Case "TODOKEDATE"
                '届日
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "TSHABANF"
                '統一車番（前）
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CARCODE & ");"
            Case "TSHABANB"
                '統一車番（後）
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CARCODE & ");"
            Case "TSHABANB2"
                '統一車番（後）２
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CARCODE & ");"
            Case "TAXKBN"
                '税区分
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , 905);"
            Case "SHUKADATE"
                '出荷日
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "STORICODE"
                '請求取引先
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & ");"
            Case "CONTCHASSIS"
                'コンテナシャーシ
                O_ATTR = "REF_Field_DBclick('" & I_FIELD & "', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_WORKLORRY & ");"
        End Select
    End Sub

    ''' <summary>
    '''  詳細情報からT0005INProwを作成する。
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Function DetailBoxToT0005INPtbl(ByRef O_RTN As String) As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim T0005INProw As DataRow = T0005INPtbl.NewRow

        '初期クリア
        For Each col As DataColumn In T0005INProw.Table.Columns
            If col.DataType.Name.ToString = "String" Then T0005INProw(col.ColumnName) = ""
        Next

        '○Detail設定処理（まずは、変更前の全項目を保存した非表示の画面からデータを復元）
        If WF_LINECNT.Text <> "" Then
            T0005INProw.ItemArray = T0005INPtbl.Rows(WF_LINECNT.Text).ItemArray
        End If

        T0005INProw("CAMPCODE") = If(String.IsNullOrEmpty(T0005INProw("CAMPCODE")), work.WF_SEL_CAMPCODE.Text, T0005INProw("CAMPCODE"))
        T0005INProw("SHIPORG") = If(String.IsNullOrEmpty(T0005INProw("SHIPORG")), work.WF_SEL_UORG.Text, T0005INProw("SHIPORG"))
        T0005INProw("TERMKBN") = If(String.IsNullOrEmpty(T0005INProw("TERMKBN")), GRT00005WRKINC.TERM_TYPE.HAND, T0005INProw("TERMKBN"))
        T0005INProw("CREWKBN") = If(String.IsNullOrEmpty(T0005INProw("CREWKBN")), "1", T0005INProw("CREWKBN"))

        '------------------------------
        '入力した内容で書き換える
        '------------------------------
        '■■■ HEADよりT0005INPtbl編集 ■■■
        Master.EraseCharToIgnore(WF_YMD.Text)
        T0005INProw("YMD") = WF_YMD.Text

        Master.EraseCharToIgnore(WF_STAFFCODE.Text)
        If T0005INProw("STAFFCODE") = "" Then
            T0005INProw("STAFFCODE") = WF_STAFFCODE.Text
        End If

        If T0005INProw("STAFFCODE") = WF_STAFFCODE.Text Then
            T0005INProw("STAFFCODE") = WF_STAFFCODE.Text
        Else
            T0005INProw("STAFFCODE") = WF_STAFFCODE.Text
            T0005INProw("SUBSTAFFCODE") = ""
            T0005INProw("SUBSTAFFNAMES") = ""
        End If

        '■■■ DetailよりT0005INPtbl編集 ■■■
        T0005INProw("HDKBN") = "D"
        T0005INProw("TIMSTP") = "0"
        T0005INProw("SELECT") = "1"
        T0005INProw("HIDDEN") = "0"
        If IsDBNull(T0005INProw("EXTRACTCNT")) Then
            T0005INProw("EXTRACTCNT") = "0"
        End If

        If WF_LINECNT.Text = "" Then
            T0005INProw("LINECNT") = 0
        Else
            Master.EraseCharToIgnore(WF_LINECNT.Text)
            T0005INProw("LINECNT") = WF_LINECNT.Text
        End If

        Master.EraseCharToIgnore(WF_SEQ.Text)
        T0005INProw("SEQ") = WF_SEQ.Text

        Master.EraseCharToIgnore(WF_DELFLG.Text)
        T0005INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_DELFLG.Text) Then
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できないレコード(削除フラグ)です。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグを入力してください。"
            rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            Return Nothing
            Exit Function
        End If

        If String.IsNullOrEmpty(WF_SEQ.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMA0002INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"                       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Return Nothing

            Exit Function
        End If

        '○Detail設定処理
        For Each item As RepeaterItem In WF_DViewRep1.Items
            For WI_REP_CNT As Integer = 1 To CONST_DETAIL_REP_COLUMN
                If CType(item.FindControl("WF_Rep1_FIELD_" & WI_REP_CNT), Label).Text <> "" Then
                    Dim WW_Text As String = CType(item.FindControl("WF_Rep1_VALUE_" & WI_REP_CNT), TextBox).Text
                    T0005INProw(CType(item.FindControl("WF_Rep1_FIELD_" & WI_REP_CNT), Label).Text) = Master.EraseCharToIgnore(WW_Text)
                End If
            Next
        Next

        SetNameValue(T0005INProw)
        Return T0005INProw

    End Function

    ''' <summary>
    ''' 明細情報名称取得処理
    ''' </summary>
    ''' <param name="IO_ROW">名称設定する行情報</param>
    ''' <remarks></remarks>
    Protected Sub SetNameValue(ByRef IO_ROW As DataRow)

        '○名称付与
        '会社名称
        IO_ROW("CAMPNAMES") = ""
        CodeToName("CAMPCODE", IO_ROW("CAMPCODE"), IO_ROW("CAMPNAMES"), WW_DUMMY)
        '出荷部署
        IO_ROW("SHIPORGNAMES") = ""
        CodeToName("SHIPORG", IO_ROW("SHIPORG"), IO_ROW("SHIPORGNAMES"), WW_DUMMY)
        '端末区分
        IO_ROW("TERMKBNNAMES") = ""
        CodeToName("TERMKBN", IO_ROW("TERMKBN"), IO_ROW("TERMKBNNAMES"), WW_DUMMY)
        '乗務員
        IO_ROW("STAFFNAMES") = ""
        CodeToName("STAFFCODE", IO_ROW("STAFFCODE"), IO_ROW("STAFFNAMES"), WW_DUMMY)
        '作業区分名
        IO_ROW("WORKKBNNAMES") = ""
        CodeToName("WORKKBN", IO_ROW("WORKKBN"), IO_ROW("WORKKBNNAMES"), WW_DUMMY)
        '乗務区分名
        IO_ROW("CREWKBNNAMES") = ""
        CodeToName("CREWKBN", IO_ROW("CREWKBN"), IO_ROW("CREWKBNNAMES"), WW_DUMMY)
        '取引先名称
        IO_ROW("TORINAMES") = ""
        CodeToName("TORICODE", IO_ROW("TORICODE"), IO_ROW("TORINAMES"), WW_DUMMY)
        '出荷場所名
        IO_ROW("SHUKABASHONAMES") = ""
        CodeToName("SHUKABASHO", IO_ROW("SHUKABASHO"), IO_ROW("SHUKABASHONAMES"), WW_DUMMY)
        '届先名称
        IO_ROW("TODOKENAMES") = ""
        CodeToName("TODOKECODE", IO_ROW("TODOKECODE"), IO_ROW("TODOKENAMES"), WW_DUMMY)
        '品名１～８名称 
        For WI_GOODS_CNT As Integer = 1 To 8
            '〇品名コード（会社コード＋油種＋品名１コード＋品名２コード）から油種・品名１コード・品名２コードを取得する
            If Not String.IsNullOrEmpty(IO_ROW("PRODUCTCODE" & WI_GOODS_CNT)) Then
                IO_ROW("PRODUCT" & WI_GOODS_CNT & "NAMES") = String.Empty
                IO_ROW("OILTYPE" & WI_GOODS_CNT) = Mid(IO_ROW("PRODUCTCODE" & WI_GOODS_CNT).ToString, 3, 2)
                IO_ROW("PRODUCT1" & WI_GOODS_CNT) = Mid(IO_ROW("PRODUCTCODE" & WI_GOODS_CNT).ToString, 5, 2)
                IO_ROW("PRODUCT2" & WI_GOODS_CNT) = Mid(IO_ROW("PRODUCTCODE" & WI_GOODS_CNT).ToString, 7)
                CodeToName("PRODUCT2" & WI_GOODS_CNT, IO_ROW("PRODUCTCODE" & WI_GOODS_CNT), IO_ROW("PRODUCT" & WI_GOODS_CNT & "NAMES"), WW_DUMMY)
            End If
        Next
        '実績区分名称
        IO_ROW("JISSKIKBNNAMES") = ""
        CodeToName("JISSKIKBN", IO_ROW("JISSKIKBN"), IO_ROW("JISSKIKBNNAMES"), WW_DUMMY)
        '積置区分名称
        IO_ROW("TUMIOKIKBNNAMES") = ""
        CodeToName("TUMIOKIKBN", IO_ROW("TUMIOKIKBN"), IO_ROW("TUMIOKIKBNNAMES"), WW_DUMMY)
        '請求取引先名称
        IO_ROW("STORICODENAMES") = ""
        CodeToName("STORICODE", IO_ROW("STORICODE"), IO_ROW("STORICODENAMES"), WW_DUMMY)
        'コンテナシャーシ
        IO_ROW("CONTCHASSISLICNPLTNO") = ""
        CodeToName("CONTCHASSIS", IO_ROW("CONTCHASSIS"), IO_ROW("CONTCHASSISLICNPLTNO"), WW_DUMMY)
        '業務車番
        IO_ROW("GSHABANLICNPLTNO") = ""
        CodeToName("CONTCHASSIS", IO_ROW("GSHABAN"), IO_ROW("GSHABANLICNPLTNO"), WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 更新行　単項目チェック
    ''' </summary>
    ''' <param name="T0005INProw" >チェック対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckInputRowData(ByRef T0005INProw As DataRow, ByRef O_RTN As String)

        '○インターフェイス初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_TEXT As String = ""
        Dim WW_YMD_FLG As String = "0"
        Dim WW_STDATE_FLG As String = "0"
        Dim WW_ENDDATE_FLG As String = "0"
        Dim WW_STTIME_FLG As String = "0"
        Dim WW_ENDTIME_FLG As String = "0"
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
        Dim WW_CHECKREPORT As String = String.Empty
        WW_ERRLIST = New List(Of String)
        If IsNothing(S0013tbl) Then S0013tbl = New DataTable
        '■■■ 単項目チェック(ヘッダー情報) ■■■

        '〇権限チェック（更新権限）
        If Not String.IsNullOrEmpty(T0005INProw("SHIPORG")) Then
            '出荷部署
            'ユーザの部署と選択した配属部署が同一なら更新可能
            If work.WF_SEL_UORG.Text = Master.USER_ORG Then
                CS0012AUTHORorg.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0012AUTHORorg.ORGCODE = T0005INProw("SHIPORG")
                CS0012AUTHORorg.STYMD = Date.Now
                CS0012AUTHORorg.ENDYMD = Date.Now
                CS0012AUTHORorg.CS0012AUTHORorg()
                If Not (isNormal(CS0012AUTHORorg.ERR) AndAlso CS0012AUTHORorg.PERMITCODE = C_PERMISSION.UPDATE) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(権限無)です。"
                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
            End If
        End If

        '・キー項目(会社コード：CAMPCODE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", T0005INProw("CAMPCODE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If

        '・キー項目(出庫日：YMD)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YMD", T0005INProw("YMD"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(出庫日エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_YMD_FLG = "1"
        End If

        '・キー項目(乗務員：STAFFCODE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", T0005INProw("STAFFCODE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If
        '②LeftBox存在チェック
        If T0005INProw("STAFFCODE") <> "" Then
            CodeToName("STAFFCODE", T0005INProw("STAFFCODE"), WW_TEXT, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("STAFFCODE") & ")"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
            T0005INProw("STAFFNAMES") = WW_TEXT
            '乗務員＝副乗務員はエラー
            If T0005INProw("STAFFCODE") = T0005INProw("SUBSTAFFCODE") Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(乗務員エラー)です。"
                WW_CheckMES2 = "乗務員と副乗務員が同じ"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(作業区分：WORKKBN)
        '①必須・項目属性チェック
        If T0005INProw("HDKBN") = "D" Then
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKKBN", T0005INProw("WORKKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
            If Not isNormal(WW_RTN) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
            '②LeftBox存在チェック
            If T0005INProw("WORKKBN") <> "" Then
                CodeToName("WORKKBN", T0005INProw("WORKKBN"), WW_TEXT, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(作業区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("WORKKBN") & ")"
                    WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
            End If
        End If

        '・キー項目(開始日：STDATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", T0005INProw("STDATE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始日エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_STDATE_FLG = "1"
        End If

        '・キー項目(開始時刻：STTIME)
        '①必須・項目属性チェック
        Dim WW_OVALUE As String = String.Empty
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", T0005INProw("STTIME"), WW_RTN, WW_CHECKREPORT, WW_OVALUE, S0013tbl)
        If isNormal(WW_RTN) Then
            T0005INProw("STTIME") = WW_OVALUE
            If T0005INProw("STTIME") <> "" Then
                T0005INProw("STTIME") = CDate(T0005INProw("STTIME")).ToString("HH:mm")
                If T0005INProw("WORKKBN") = "A1" AndAlso T0005INProw("CTRL") <> "OFF" Then
                    If Master.ExistCheckTable(work.WF_SEL_CAMPCODE.Text, "STTIMEA1", S0013tbl) Then
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIMEA1", T0005INProw("STTIME"), WW_RTN, WW_CHECKREPORT, WW_OVALUE, S0013tbl)
                        If Not isNormal(WW_RTN) Then
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(開始時刻エラー)です。"
                            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        End If
                    End If
                    If T0005INProw("CTRL") = "ON2" Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・警告が存在します。(出庫時刻＝始業時刻)です。"
                        WW_CheckMES2 = "始業時刻が正しいか確認してください。(" & T0005INProw("STTIME") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                    End If
                End If
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(開始時刻エラー)です。"
                WW_CheckMES2 = "必須入力項目"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                WW_STTIME_FLG = "1"
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始時刻エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_STTIME_FLG = "1"
        End If

        '・キー項目(終了日：ENDDATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", T0005INProw("ENDDATE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了日エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_ENDDATE_FLG = "1"
        End If

        '・キー項目(終了時刻：ENDTIME)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", T0005INProw("ENDTIME"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If isNormal(WW_RTN) Then
            If T0005INProw("ENDTIME") <> "" Then
                T0005INProw("ENDTIME") = CDate(T0005INProw("ENDTIME")).ToString("HH:mm")
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(終了時刻エラー)です。"
                WW_CheckMES2 = "必須入力項目"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                WW_ENDTIME_FLG = "1"
            End If
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了時刻エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_ENDTIME_FLG = "1"
        End If

        '・キー項目(車端区分：TERMKBN)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TERMKBN", T0005INProw("TERMKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(車端区分エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If
        '②LeftBox存在チェック
        If T0005INProw("TERMKBN") <> "" Then
            CodeToName("TERMKBN", T0005INProw("TERMKBN"), WW_TEXT, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(車端区分エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
            T0005INProw("TERMKBNNAMES") = WW_TEXT
        End If

        If T0005INProw("WORKKBN") = "F1" Then
            If T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                '・キー項目(日報番号：NIPPONO)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIPPONO", T0005INProw("NIPPONO"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(日報番号エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(業務車番：GSHABAN)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", T0005INProw("GSHABAN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("GSHABAN") <> "" Then
                    CodeToName("GSHABAN", T0005INProw("GSHABAN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("GSHABAN") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(統一車番（前）：TSHABANF)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABANF", T0005INProw("TSHABANF"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(統一車番（前）エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TSHABANF") <> "" Then
                    CodeToName("TSHABANF", T0005INProw("TSHABANF"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(統一車番（前）エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TSHABANF") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(統一車番（後）：TSHABANB)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABANB", T0005INProw("TSHABANB"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(統一車番（後）エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TSHABANB") <> "" Then
                    CodeToName("TSHABANB", T0005INProw("TSHABANB"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(統一車番（後）エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TSHABANB") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(統一車番（後）2：TSHABANB2)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TSHABANB2", T0005INProw("TSHABANB2"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(統一車番（後）２エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TSHABANB2") <> "" Then
                    CodeToName("TSHABANB", T0005INProw("TSHABANB2"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(統一車番（後）２エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TSHABANB2") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(乗務区分：CREWKBN)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CREWKBN", T0005INProw("CREWKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(乗務区分エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '■容器検査期限、車検期限チェック（八戸、大井川、水島のみ）
                Dim WW_HPRSINSNYMDF As String = ""
                Dim WW_HPRSINSNYMDB As String = ""
                Dim WW_HPRSINSNYMDB2 As String = ""
                Dim WW_LICNYMDF As String = ""
                Dim WW_LICNYMDB As String = ""
                Dim WW_LICNYMDB2 As String = ""
                Dim WW_HPR As String = "OFF"
                Dim WW_LICNPLTNOF As String = ""
                Dim WW_LICNPLTNOB As String = ""
                Dim WW_LICNPLTNOB2 As String = ""
                If WW_YMD_FLG = "0" AndAlso T0005INProw("YMD") <> "" Then

                    If T0005COM.IsInspectionOrg(work.WF_SEL_CAMPCODE.Text, T0005INProw("SHIPORG"), WW_RTN) Then
                        If IsNothing(work.CreateTSHABANList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN).Items.FindByValue(T0005INProw("GSHABAN"))) Then
                            If work.CreateSHABAN2OILList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, O_RTN).Items.FindByValue(T0005INProw("GSHABAN")).Text = "02" Then
                                Dim sublist As ListBox = work.GetShabanSubTable(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)(T0005INProw("GSHABAN"))
                                WW_HPRSINSNYMDF = sublist.Items.FindByText("HPRSINSNYMDF").Value.Replace("-", "/")
                                WW_HPRSINSNYMDB = sublist.Items.FindByText("HPRSINSNYMDB").Value.Replace("-", "/")
                                WW_HPRSINSNYMDB2 = sublist.Items.FindByText("HPRSINSNYMDB2").Value.Replace("-", "/")
                                WW_LICNYMDF = sublist.Items.FindByText("LICNYMDF").Value.Replace("-", "/")
                                WW_LICNYMDB = sublist.Items.FindByText("LICNYMDB").Value.Replace("-", "/")
                                WW_LICNYMDB2 = sublist.Items.FindByText("LICNYMDB2").Value.Replace("-", "/")
                                WW_LICNPLTNOF = sublist.Items.FindByText("LICNPLTNOF").Value
                                WW_LICNPLTNOB = sublist.Items.FindByText("LICNPLTNOB").Value
                                WW_LICNPLTNOB2 = sublist.Items.FindByText("LICNPLTNOB2").Value
                                WW_HPR = "ON"

                            End If
                        End If

                        '高圧のみ
                        If WW_HPR = "ON" Then

                            '容器検査年月日チェック（２カ月前から警告、４日前はエラー）
                            '車検年月日チェック（１カ月前から警告、４日前はエラー）
                            '------ 車両前 -------------------------------------------------------------------------
                            '車検チェック
                            If T0005INProw("SHARYOTYPEF") = "A" OrElse
                               T0005INProw("SHARYOTYPEF") = "C" OrElse
                               T0005INProw("SHARYOTYPEF") = "D" Then
                                If IsDate(WW_LICNYMDF) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_LICNYMDF))
                                    If CDate(WW_LICNYMDF) < T0005INProw("YMD") Then
                                        '車検切れ
                                        WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDF).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDF).AddMonths(-1) < T0005INProw("YMD") Then
                                        '1カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If
                            End If

                            '容器チェック
                            If T0005INProw("SHARYOTYPEF") = "B" OrElse
                               T0005INProw("SHARYOTYPEF") = "D" Then
                                If IsDate(WW_HPRSINSNYMDF) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_HPRSINSNYMDF))
                                    If CDate(WW_HPRSINSNYMDF) < T0005INProw("YMD") Then
                                        '容器検査切れ
                                        WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDF).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDF).AddMonths(-2) < T0005INProw("YMD") Then
                                        '2カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOF & " " & T0005INProw("TSHABANF") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If

                            End If

                            '------ 車両後 -------------------------------------------------------------------------
                            '車検チェック
                            If T0005INProw("SHARYOTYPEB") = "A" OrElse
                               T0005INProw("SHARYOTYPEB") = "C" OrElse
                               T0005INProw("SHARYOTYPEB") = "D" Then
                                If IsDate(WW_LICNYMDB) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_LICNYMDB))
                                    If CDate(WW_LICNYMDB) < T0005INProw("YMD") Then
                                        '車検切れ
                                        WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDB).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDB).AddMonths(-1) < T0005INProw("YMD") Then
                                        '1カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If
                            End If

                            '容器チェック
                            If T0005INProw("SHARYOTYPEB") = "B" OrElse
                               T0005INProw("SHARYOTYPEB") = "D" Then
                                If IsDate(WW_HPRSINSNYMDB) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_HPRSINSNYMDB))
                                    If CDate(WW_HPRSINSNYMDB) < T0005INProw("YMD") Then
                                        '容器検査切れ
                                        WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDB).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDB).AddMonths(-2) < T0005INProw("YMD") Then
                                        '2カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB & " " & T0005INProw("TSHABANB") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If

                            End If

                            '------ 車両後２ -------------------------------------------------------------------------

                            '車検チェック
                            If T0005INProw("SHARYOTYPEB2") = "A" OrElse
                               T0005INProw("SHARYOTYPEB2") = "C" OrElse
                               T0005INProw("SHARYOTYPEB2") = "D" Then
                                If IsDate(WW_LICNYMDB2) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_LICNYMDB2))
                                    If CDate(WW_LICNYMDB2) < T0005INProw("YMD") Then
                                        '車検切れ
                                        WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDB2).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_LICNYMDB2).AddMonths(-1) < T0005INProw("YMD") Then
                                        '1カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If
                            End If

                            '容器チェック
                            If T0005INProw("SHARYOTYPEB2") = "B" OrElse
                               T0005INProw("SHARYOTYPEB2") = "D" Then
                                If IsDate(WW_HPRSINSNYMDB2) Then
                                    Dim WW_days As String = DateDiff("d", T0005INProw("YMD"), CDate(WW_HPRSINSNYMDB2))
                                    If CDate(WW_HPRSINSNYMDB2) < T0005INProw("YMD") Then
                                        '容器検査切れ
                                        WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDB2).AddDays(-4) < T0005INProw("YMD") Then
                                        '４日前はエラー
                                        WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    ElseIf CDate(WW_HPRSINSNYMDB2).AddMonths(-2) < T0005INProw("YMD") Then
                                        '2カ月前から警告
                                        WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.WORNING_RECORD_EXIST)
                                    End If
                                Else
                                    'エラー
                                    WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB2 & " " & T0005INProw("TSHABANB2") & ")"
                                    WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If

                            End If
                        End If
                    End If
                End If
            End If
        End If

        '-------------------------------------------------------------------------------
        '荷積、荷卸しの場合
        '-------------------------------------------------------------------------------
        If T0005INProw("WORKKBN") = "B2" OrElse T0005INProw("WORKKBN") = "B3" Then
            If T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                '・キー項目(出荷場所：SHUKABASHO)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", T0005INProw("SHUKABASHO"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("SHUKABASHO") <> "" Then
                    CodeToName("SHUKABASHO", T0005INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("SHUKABASHO") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(出荷日：SHUKADATE)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADATE", T0005INProw("SHUKADATE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(出荷日エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(トリップ：TRIPNO)
                '①必須・項目属性チェック
                If InStr(T0005INProw("TRIPNO"), "新") <= 0 Then
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", T0005INProw("TRIPNO"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                    If Not isNormal(WW_RTN) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(トリップエラー)です。"
                        WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(コンテナシャーシ：CONTCHASSIS)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTCHASSIS", T0005INProw("CONTCHASSIS"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(コンテナシャーシエラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

            End If
        End If

        '------------------------------------
        '配送作業、配送ボタン（NJS）しの場合
        '------------------------------------
        If T0005INProw("WORKKBN") = "BY" OrElse T0005INProw("WORKKBN") = "G1" Then
            If T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                '②LeftBox存在チェック
                If T0005INProw("TORICODE") <> "" Then
                    CodeToName("TORICODE", T0005INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TORICODE") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If
            End If
        End If

        '----------------------
        '荷卸しの場合
        '----------------------
        If T0005INProw("WORKKBN") = "B3" Then
            If T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                '・キー項目(取引先：TORICODE)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", T0005INProw("TORICODE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TORICODE") <> "" Then
                    CodeToName("TORICODE", T0005INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TORICODE") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(請求取引先：STORICODE)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STORICODE", T0005INProw("STORICODE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(請求取引先エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(届先：TODOKECODE)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", T0005INProw("TODOKECODE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TODOKECODE") <> "" Then
                    CodeToName("TODOKECODE", T0005INProw("TODOKECODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TODOKECODE") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(届日：TODOKEDATE)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKEDATE", T0005INProw("TODOKEDATE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(ドロップ：DROPNO)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DROPNO", T0005INProw("DROPNO"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(ドロップエラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(品名１：PRODUCTCODE1)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE1"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE1") <> "" Then
                    CodeToName("PRODUCT21", T0005INProw("PRODUCTCODE1"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE1") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名２：PRODUCTCODE2)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE2"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名２エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE2") <> "" Then
                    CodeToName("PRODUCT22", T0005INProw("PRODUCTCODE2"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名２エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE2") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名３：PRODUCTCODE3)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE3"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名３エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE3") <> "" Then
                    CodeToName("PRODUCT23", T0005INProw("PRODUCTCODE3"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名３エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE3") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名４：PRODUCTCODE4)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE4"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名４エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE4") <> "" Then
                    CodeToName("PRODUCT24", T0005INProw("PRODUCTCODE4"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名４エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE4") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名５：PRODUCTCODE5)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE5"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名５エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE5") <> "" Then
                    CodeToName("PRODUCT25", T0005INProw("PRODUCTCODE5"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名５エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE5") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名６：PRODUCTCODE6)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE6"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名６エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE6") <> "" Then
                    CodeToName("PRODUCT26", T0005INProw("PRODUCTCODE6"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名６エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE6") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名７：PRODUCTCODE7)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE7"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名７エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE7") <> "" Then
                    CodeToName("PRODUCT27", T0005INProw("PRODUCTCODE7"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名７エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE7") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(品名８：PRODUCTCODE8)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", T0005INProw("PRODUCTCODE8"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名８エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("PRODUCTCODE8") <> "" Then
                    CodeToName("PRODUCT28", T0005INProw("PRODUCTCODE8"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(品名８エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("PRODUCTCODE8") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If

                '・キー項目(数量１：SURYO1)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO1"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO1") = Format(Val(T0005INProw("SURYO1")), "#,0.000")
                    If Val(T0005INProw("SURYO1")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量１エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO1")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量１エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量２：SURYO2)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO2"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO2") = Format(Val(T0005INProw("SURYO2")), "#,0.000")
                    If Val(T0005INProw("SURYO2")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量２エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO2")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量２エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量３：SURYO3)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO3"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO3") = Format(Val(T0005INProw("SURYO3")), "#,0.000")

                    If Val(T0005INProw("SURYO3")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量３エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO3")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量３エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量４：SURYO4)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO4"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO4") = Format(Val(T0005INProw("SURYO4")), "#,0.000")

                    If Val(T0005INProw("SURYO4")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量４エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO4")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量４エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量５：SURYO5)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO5"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO5") = Format(Val(T0005INProw("SURYO5")), "#,0.000")
                    If Val(T0005INProw("SURYO5")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量５エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO5")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量５エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量６：SURYO6)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO6"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO6") = Format(Val(T0005INProw("SURYO6")), "#,0.000")
                    If Val(T0005INProw("SURYO6")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量６エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO6")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量６エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量７：SURYO7)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO7"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO7") = Format(Val(T0005INProw("SURYO7")), "#,0.000")
                    If Val(T0005INProw("SURYO7")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量７エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO7")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量７エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '・キー項目(数量８：SURYO8)
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", T0005INProw("SURYO8"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If isNormal(WW_RTN) Then
                    T0005INProw("SURYO8") = Format(Val(T0005INProw("SURYO8")), "#,0.000")
                    If Val(T0005INProw("SURYO8")) < 0 Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(数量８エラー)です。"
                        WW_CheckMES2 = T0005INProw("SURYO8")
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量８エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If

                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TAXKBN", T0005INProw("TAXKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("TAXKBN") <> "" Then
                    CodeToName("TAXKBN", T0005INProw("TAXKBN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(税区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TAXKBN") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                    T0005INProw("TAXKBNNAMES") = WW_TEXT
                End If

                '・キー項目(売上計上区分：URIKBN)
                If T0005INProw("URIKBN") = "" Then
                    GS0029T3CNTLget.CAMPCODE = T0005INProw("CAMPCODE")
                    GS0029T3CNTLget.TORICODE = T0005INProw("TORICODE")
                    GS0029T3CNTLget.OILTYPE = T0005INProw("OILTYPE1")
                    GS0029T3CNTLget.ORDERORG = T0005INProw("SHIPORG")
                    GS0029T3CNTLget.KIJUNDATE = T0005INProw("YMD")
                    GS0029T3CNTLget.GS0029T3CNTLget()
                    If GS0029T3CNTLget.ERR = C_MESSAGE_NO.NORMAL Then
                        T0005INProw("URIKBN") = GS0029T3CNTLget.URIKBN
                    End If
                End If
                '①必須・項目属性チェック
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "URIKBN", T0005INProw("URIKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
                If Not isNormal(WW_RTN) Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(売上計上基準エラー)です。"
                    WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '②LeftBox存在チェック
                If T0005INProw("URIKBN") <> "" Then
                    CodeToName("URIKBN", T0005INProw("URIKBN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(売上計上基準エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("URIKBN") & ")"
                        WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                    T0005INProw("URIKBNNAMES") = WW_TEXT
                End If

            End If
        End If


        '・キー項目(開始メータ：STMATER)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STMATER", T0005INProw("STMATER"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(開始メータエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("STMATER") = Format(Val(T0005INProw("STMATER")), "#,0.00")
            If Val(T0005INProw("STMATER")) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(開始メータエラー)です。"
                WW_CheckMES2 = T0005INProw("STMATER")
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(終了メータ：ENDMATER)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDMATER", T0005INProw("ENDMATER"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(終了メータエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("ENDMATER") = Format(Val(T0005INProw("ENDMATER")), "#,0.00")
            If Val(T0005INProw("ENDMATER")) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(終了メータエラー)です。"
                WW_CheckMES2 = T0005INProw("ENDMATER")
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(走行距離：SOUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SOUDISTANCE", T0005INProw("SOUDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("SOUDISTANCE") = Format(Val(T0005INProw("SOUDISTANCE")), "#,0.00")

            If Val(T0005INProw("SOUDISTANCE")) > 700 AndAlso (T0005INProw("WORKKBN") <> "F3" AndAlso T0005INProw("WORKKBN") <> "G1" AndAlso T0005INProw("HDKBN") = "D") Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
                WW_CheckMES2 = "走行距離が700キロを超過しています(" & T0005INProw("SOUDISTANCE") & ")"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            ElseIf Val(T0005INProw("SOUDISTANCE")) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(走行距離エラー)です。"
                WW_CheckMES2 = T0005INProw("SOUDISTANCE")
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If
        '・キー項目(累積走行距離：RUIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RUIDISTANCE", T0005INProw("RUIDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(累積走行距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("RUIDISTANCE") = Format(Val(T0005INProw("RUIDISTANCE")), "#,0.00")
        End If

        '・キー項目(実車距離：JIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JIDISTANCE", T0005INProw("JIDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(実車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("JIDISTANCE") = Format(Val(T0005INProw("JIDISTANCE")), "#,0.00")
        End If

        '・キー項目(空車距離：KUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KUDISTANCE", T0005INProw("KUDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(空車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("KUDISTANCE") = Format(Val(T0005INProw("KUDISTANCE")), "#,0.00")
        End If

        '・キー項目(一般距離：IPPDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPDISTANCE", T0005INProw("IPPDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("IPPDISTANCE") = Format(Val(T0005INProw("IPPDISTANCE")), "#,0.00")
        End If

        '・キー項目(高速距離：KOSDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSDISTANCE", T0005INProw("KOSDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("KOSDISTANCE") = Format(Val(T0005INProw("KOSDISTANCE")), "#,0.00")
        End If

        '・キー項目(一般・実車距離：IPPJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPJIDISTANCE", T0005INProw("IPPJIDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般・実車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("IPPJIDISTANCE") = Format(Val(T0005INProw("IPPJIDISTANCE")), "#,0.00")
        End If

        '・キー項目(一般・空車距離：IPPJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "IPPKUDISTANCE", T0005INProw("IPPKUDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(一般・空車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("IPPKUDISTANCE") = Format(Val(T0005INProw("IPPKUDISTANCE")), "#,0.00")
        End If

        '・キー項目(高速・実車距離：KOSJIDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSJIDISTANCE", T0005INProw("KOSJIDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速・実車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("KOSJIDISTANCE") = Format(Val(T0005INProw("KOSJIDISTANCE")), "#,0.00")
        End If

        '・キー項目(高速・空車距離：KOSKUDISTANCE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOSKUDISTANCE", T0005INProw("KOSKUDISTANCE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(高速・空車距離エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("KOSKUDISTANCE") = Format(Val(T0005INProw("KOSKUDISTANCE")), "#,0.00")
        End If

        '・キー項目(通行料・現金：CASH)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CASH", T0005INProw("CASH"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・現金エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("CASH") = Format(Val(T0005INProw("CASH")), "#,0")
        End If

        '・キー項目(通行料・ETC：ETC)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ETC", T0005INProw("ETC"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・ETCエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("ETC") = Format(Val(T0005INProw("ETC")), "#,0")
        End If

        '・キー項目(通行料・回数券：TICKET)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TICKET", T0005INProw("TICKET"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・回数券エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("TICKET") = Format(Val(T0005INProw("TICKET")), "#,0")
        End If

        '・キー項目(通行料・プレート：PRATE)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRATE", T0005INProw("PRATE"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料・プレートエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("PRATE") = Format(Val(T0005INProw("PRATE")), "#,0")
        End If

        '・キー項目(通行料：TOTALTOLL)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOTALTOLL", T0005INProw("TOTALTOLL"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("TOTALTOLL") = Format(Val(T0005INProw("TOTALTOLL")), "#,0")
            If Val(T0005INProw("TOTALTOLL")) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(通行料エラー)です。"
                WW_CheckMES2 = T0005INProw("TOTALTOLL")
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(給油：KYUYU)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KYUYU", T0005INProw("KYUYU"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(通行料エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        Else
            T0005INProw("KYUYU") = Format(Val(T0005INProw("KYUYU")), "#,0.00")
            If Val(T0005INProw("KYUYU")) < 0 Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(給油エラー)です。"
                WW_CheckMES2 = T0005INProw("KYUYU")
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            ElseIf Val(T0005INProw("KYUYU")) > 500 AndAlso T0005INProw("WORKKBN") = "F3" Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(給油エラー)です。"
                WW_CheckMES2 = "500ℓ超です(" & T0005INProw("KYUYU") & ")"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(積置区分：TUMIOKIKBN)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN", T0005INProw("TUMIOKIKBN"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(積置区分エラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If
        '②LeftBox存在チェック
        If T0005INProw("TUMIOKIKBN") <> "" Then
            CodeToName("TUMIOKIKBN", T0005INProw("TUMIOKIKBN"), WW_TEXT, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(積置区分エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("TUMIOKIKBN") & ")"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '・キー項目(削除フラグ：DELFLG)
        '①必須・項目属性チェック
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DELFLG", T0005INProw("DELFLG"), WW_RTN, WW_CHECKREPORT, S0013tbl)
        If Not isNormal(WW_RTN) Then
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード((削除フラグエラー)です。"
            WriteErrorMessage(WW_CheckMES1, WW_CHECKREPORT, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
        End If
        '②LeftBox存在チェック
        If T0005INProw("DELFLG") <> "" Then
            CodeToName("DELFLG", T0005INProw("DELFLG"), WW_TEXT, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0005INProw("DELFLG") & ")"
                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            End If
        End If

        '■■■ 関連チェック■■■
        If T0005INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
            '大小比較チェック
            If WW_YMD_FLG = "0" AndAlso
               WW_STDATE_FLG = "0" AndAlso
               WW_ENDDATE_FLG = "0" AndAlso
               WW_STTIME_FLG = "0" AndAlso
               WW_ENDTIME_FLG = "0" Then
                '〇開始日＋開始時刻と終了日＋終了時刻の大小比較
                If T0005INProw("STDATE") <> "" AndAlso
                   T0005INProw("STTIME") <> "" AndAlso
                   T0005INProw("ENDDATE") <> "" AndAlso
                   T0005INProw("ENDTIME") <> "" Then
                    If T0005INProw("STDATE") > T0005INProw("ENDDATE") Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(開始日付 ＞ 終了日付)です。"
                        WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If

                    If IsDate(T0005INProw("STDATE") & " " & T0005INProw("STTIME")) AndAlso
                       IsDate(T0005INProw("ENDDATE") & " " & T0005INProw("ENDTIME")) Then

                        If CDate(T0005INProw("STDATE") & " " & T0005INProw("STTIME")) >
                           CDate(T0005INProw("ENDDATE") & " " & T0005INProw("ENDTIME")) Then
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(開始時刻 ＞ 終了時刻)です。"
                            WriteErrorMessage(WW_CheckMES1, "", WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        End If

                    End If
                    '〇作業区分：始業(A1)、終業(Z1)、出庫(F1)、帰庫(F3)の時、同時確認
                    If T0005INProw("WORKKBN") = "A1" OrElse
                       T0005INProw("WORKKBN") = "F1" OrElse
                       T0005INProw("WORKKBN") = "F3" OrElse
                       T0005INProw("WORKKBN") = "Z1" Then
                        If IsDate(T0005INProw("STDATE") & " " & T0005INProw("STTIME")) AndAlso
                           IsDate(T0005INProw("ENDDATE") & " " & T0005INProw("ENDTIME")) Then

                            If CDate(T0005INProw("STDATE") & " " & T0005INProw("STTIME")) =
                               CDate(T0005INProw("ENDDATE") & " " & T0005INProw("ENDTIME")) Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(開始時刻 ≠ 終了時刻)です。"
                                WW_CheckMES2 = "始業、出庫、帰庫、終業は、開始時刻＝終了時刻として下さい"
                                WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                            End If

                        End If
                    End If
                End If
            End If

            '荷卸の時、届日確認
            If T0005INProw("WORKKBN") = "B3" Then

                If T0005INProw("TODOKEDATE") = "" Then
                    WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                    WW_CheckMES2 = "届日未入力"
                    WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                ElseIf T0005INProw("TODOKEDATE") < T0005INProw("YMD") Then
                    WW_CheckMES1 = "・更新できないレコード(届日エラー)です。"
                    WW_CheckMES2 = "出庫日 ＞ 届日です"
                    WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '〇荷卸の時、品名必須チェック
                If T0005INProw("PRODUCTCODE1") = "" AndAlso
                   T0005INProw("PRODUCTCODE2") = "" AndAlso
                   T0005INProw("PRODUCTCODE3") = "" AndAlso
                   T0005INProw("PRODUCTCODE4") = "" AndAlso
                   T0005INProw("PRODUCTCODE5") = "" AndAlso
                   T0005INProw("PRODUCTCODE6") = "" AndAlso
                   T0005INProw("PRODUCTCODE7") = "" AndAlso
                   T0005INProw("PRODUCTCODE8") = "" Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(品名エラー)です。"
                    WW_CheckMES2 = "品名未入力（品名１～８）"
                    WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
                '〇荷卸の時、品名と数量チェック
                If Val(T0005INProw("SURYO1")) = 0 AndAlso
                   Val(T0005INProw("SURYO2")) = 0 AndAlso
                   Val(T0005INProw("SURYO3")) = 0 AndAlso
                   Val(T0005INProw("SURYO4")) = 0 AndAlso
                   Val(T0005INProw("SURYO5")) = 0 AndAlso
                   Val(T0005INProw("SURYO6")) = 0 AndAlso
                   Val(T0005INProw("SURYO7")) = 0 AndAlso
                   Val(T0005INProw("SURYO8")) = 0 Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(数量エラー)です。"
                    WW_CheckMES2 = "荷卸数量未入力（数量１～８）"
                    WriteErrorMessage(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, T0005INProw, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                End If
            End If

            '荷卸
            If T0005INProw("WORKKBN") = "B3" Then
                SetOrderForHOrder(T0005INProw, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    If WW_ERRCODE = C_MESSAGE_NO.DB_ERROR Then
                        WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                Else
                    T0005INProw("ORDERUMU") = C_LIST_OPERATION_CODE.NODATA
                End If
            End If
        End If

        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.WORNING_RECORD_EXIST) >= 0 Then
                O_RTN = C_MESSAGE_NO.WORNING_RECORD_EXIST
            End If
        End If

    End Sub

    ''' <summary>
    ''' T0005tblへの更新（一覧への更新）
    ''' </summary>
    ''' <param name="T0005INProw" >設定する行データ</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub UpdateListTable(ByRef T0005INProw As DataRow, ByRef O_RTN As String)

        Dim WW_CREWKBN As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_NIPPONO As String = ""
        Dim WW_SOUDISTANCE As Decimal = 0

        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each Row As DataRow In T0005INPtbl.Rows
            Select Case Row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        isUpdatingRow(T0005INProw, WW_RTN_SW)

        If WW_RTN_SW = "有" OrElse (WW_RTN_SW = "無" AndAlso Not isNormal(O_RTN)) Then

            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1

                If T0005INPtbl.Rows(i)("HDKBN") = "D" AndAlso
                    T0005INPtbl.Rows(i)("SEQ") = T0005INProw("SEQ") Then

                    WW_SOUDISTANCE = Val(Replace(T0005INProw("SOUDISTANCE"), ",", "")) - Val(Replace(T0005INPtbl.Rows(i)("SOUDISTANCE"), ",", ""))
                    If T0005INProw("WORKKBN") <> "F3" Then
                        For j As Integer = 0 To T0005INPtbl.Rows.Count - 1
                            If T0005INPtbl.Rows(j)("WORKKBN") = "F3" Then
                                T0005INPtbl.Rows(j)("SOUDISTANCE") = Val(Replace(T0005INPtbl.Rows(j)("SOUDISTANCE"), ",", "")) + WW_SOUDISTANCE
                                Exit For
                            End If
                        Next
                    End If

                    WW_CREWKBN = T0005INPtbl.Rows(i)("CREWKBN")
                    WW_GSHABAN = T0005INPtbl.Rows(i)("GSHABAN")
                    WW_NIPPONO = T0005INPtbl.Rows(i)("NIPPONO")

                    '出庫の場合、入力された業務車番、統一車番、日報番号を同一（～帰庫まで）レコードに反映する
                    If T0005INProw("WORKKBN") = "F1" Then
                        If T0005INProw("CREWKBN") = WW_CREWKBN AndAlso
                           T0005INProw("GSHABAN") = WW_GSHABAN AndAlso
                           T0005INProw("NIPPONO") = WW_NIPPONO Then
                        Else

                            For Each updRow As DataRow In T0005INPtbl.Rows
                                If updRow("CREWKBN") = WW_CREWKBN AndAlso
                                   updRow("GSHABAN") = WW_GSHABAN AndAlso
                                   updRow("NIPPONO") = WW_NIPPONO Then

                                    If updRow("HDKBN") = "H" Then
                                        updRow("CREWKBN") = T0005INProw("CREWKBN")
                                        updRow("CREWKBNNAMES") = T0005INProw("CREWKBNNAMES")
                                    Else
                                        updRow("GSHABAN") = T0005INProw("GSHABAN")
                                        updRow("SHARYOTYPEF") = T0005INProw("SHARYOTYPEF")
                                        updRow("TSHABANF") = T0005INProw("TSHABANF")
                                        updRow("SHARYOTYPEB") = T0005INProw("SHARYOTYPEB")
                                        updRow("TSHABANB") = T0005INProw("TSHABANB")
                                        updRow("SHARYOTYPEB2") = T0005INProw("SHARYOTYPEB2")
                                        updRow("TSHABANB2") = T0005INProw("TSHABANB2")
                                        updRow("NIPPONO") = T0005INProw("NIPPONO")
                                        updRow("CREWKBN") = T0005INProw("CREWKBN")
                                        updRow("CREWKBNNAMES") = T0005INProw("CREWKBNNAMES")
                                    End If
                                End If
                            Next
                        End If
                    End If
                    '〇名称設定
                    SetNameValue(T0005INProw)
                    '〇行をテーブルに反映させる
                    SetUpdateValue(T0005INProw, i)

                    If O_RTN = C_MESSAGE_NO.NORMAL Then
                        T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    Else
                        T0005INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If

                    Exit For
                End If
            Next

            '終了日時を次のレコードの開始日時に設定（但し、次の開始日時の方が小さい場合のみ。また、最終レコードは無視）
            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "SEQ"
            'A1Z1データ取得
            CS0026TBLSORT.FILTER = "WORKKBN = 'A1' or WORKKBN = 'Z1'"
            Dim WW_A1Z1tbl As DataTable = CS0026TBLSORT.sort()
            'A1Z1以外データ取得
            CS0026TBLSORT.FILTER = "WORKKBN <> 'A1' and WORKKBN <> 'Z1'"
            Dim WW_T5WKtbl As DataTable = CS0026TBLSORT.sort()
            '〇サマリー処理（業務車番と日報番号単位でサマリーし、先頭レコードにサマリー後の値を設定しなおす）
            Dim WW_JIDISTANCE As Decimal = 0
            Dim WW_KUDISTANCE As Decimal = 0
            Dim WW_IPPDISTANCE As Decimal = 0
            Dim WW_KOSDISTANCE As Decimal = 0
            Dim WW_IPPJIDISTANCE As Decimal = 0
            Dim WW_IPPKUDISTANCE As Decimal = 0
            Dim WW_KOSJIDISTANCE As Decimal = 0
            Dim WW_KOSKUDISTANCE As Decimal = 0
            For Each T5WKrow As DataRow In WW_T5WKtbl.Rows
                '〇詳細データで区分が帰庫(F3)の場合
                If T5WKrow("HDKBN") = "D" AndAlso
                   T5WKrow("WORKKBN") <> "F3" Then
                    If T5WKrow("GSHABAN") = T0005INProw("GSHABAN") AndAlso
                       T5WKrow("NIPPONO") = T0005INProw("NIPPONO") Then
                        WW_JIDISTANCE += T5WKrow("JIDISTANCE")
                        WW_KUDISTANCE += T5WKrow("KUDISTANCE")
                        WW_IPPDISTANCE += T5WKrow("IPPDISTANCE")
                        WW_KOSDISTANCE += T5WKrow("KOSDISTANCE")
                        WW_IPPJIDISTANCE += T5WKrow("IPPJIDISTANCE")
                        WW_IPPKUDISTANCE += T5WKrow("IPPKUDISTANCE")
                        WW_KOSJIDISTANCE += T5WKrow("KOSJIDISTANCE")
                        WW_KOSKUDISTANCE += T5WKrow("KOSKUDISTANCE")
                    End If
                End If
            Next
            'サマリー後の値を再設定
            For Each T5WKrow As DataRow In WW_T5WKtbl.Rows
                '〇詳細データで区分が帰庫(F3)の場合
                If T5WKrow("HDKBN") = "D" AndAlso
                   T5WKrow("WORKKBN") = "F3" Then
                    If T5WKrow("GSHABAN") = T0005INProw("GSHABAN") AndAlso
                       T5WKrow("NIPPONO") = T0005INProw("NIPPONO") Then
                        T5WKrow("JIDISTANCE") = WW_JIDISTANCE
                        T5WKrow("KUDISTANCE") = WW_KUDISTANCE
                        T5WKrow("IPPDISTANCE") = WW_IPPDISTANCE
                        T5WKrow("KOSDISTANCE") = WW_KOSDISTANCE
                        T5WKrow("IPPJIDISTANCE") = WW_IPPJIDISTANCE
                        T5WKrow("IPPKUDISTANCE") = WW_IPPKUDISTANCE
                        T5WKrow("KOSJIDISTANCE") = WW_KOSJIDISTANCE
                        T5WKrow("KOSKUDISTANCE") = WW_KOSKUDISTANCE
                        Exit For
                    End If
                End If
            Next

            T0005INPtbl = WW_T5WKtbl.Copy
            T0005INPtbl.Merge(WW_A1Z1tbl)

            WW_A1Z1tbl.Dispose()
            WW_A1Z1tbl = Nothing
            WW_T5WKtbl.Dispose()
            WW_T5WKtbl = Nothing

            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, SEQ"
            CS0026TBLSORT.FILTER = ""
            T0005INPtbl = CS0026TBLSORT.sort()
        End If

        If WW_RTN_SW = "新" Then

            '新規の入力の場合、ヘッダーがなければレコードを作成する
            If Val(work.WF_SEL_BUTTON.Text) = GRT00005WRKINC.LC_BTN_TYPE.BTN_NEW AndAlso T0005INPtbl.Rows.Count = 0 Then
                Dim WW_T0005INProw As DataRow = T0005INPtbl.NewRow
                T0005COM.InitialT5INPRow(WW_T0005INProw)
                WW_T0005INProw("LINECNT") = 0
                If O_RTN = C_MESSAGE_NO.NORMAL Then
                    WW_T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Else
                    WW_T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
                WW_T0005INProw("TIMSTP") = "0"
                WW_T0005INProw("SELECT") = 1
                WW_T0005INProw("HIDDEN") = 1
                WW_T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0005INProw("YMD") = WF_YMD.Text
                WW_T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text
                WW_T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.HAND
                WW_T0005INProw("STDATE") = ""
                FindRepeaterItem("STDATE", WW_T0005INProw("STDATE"))
                WW_T0005INProw("ENDDATE") = ""
                FindRepeaterItem("ENDDATE", WW_T0005INProw("ENDDATE"))
                WW_T0005INProw("STTIME") = ""
                FindRepeaterItem("STTIME", WW_T0005INProw("STTIME"))
                WW_T0005INProw("ENDTIME") = ""
                FindRepeaterItem("STTIME", WW_T0005INProw("ENDTIME"))
                WW_T0005INProw("NIPPONO") = ""
                FindRepeaterItem("NIPPONO", WW_T0005INProw("NIPPONO"))
                WW_T0005INProw("STAFFCODE") = WF_STAFFCODE.Text
                WW_T0005INProw("GSHABAN") = ""
                FindRepeaterItem("GSHABAN", WW_T0005INProw("GSHABAN"))
                WW_T0005INProw("CREWKBN") = "1"
                WW_T0005INProw("HDKBN") = "H"
                WW_T0005INProw("STMATER") = ""
                FindRepeaterItem("STMATER", WW_T0005INProw("STMATER"))
                WW_T0005INProw("ENDMATER") = ""
                FindRepeaterItem("ENDMATER", WW_T0005INProw("ENDMATER"))
                WW_T0005INProw("SOUDISTANCE") = ""
                FindRepeaterItem("SOUDISTANCE", WW_T0005INProw("SOUDISTANCE"))
                WW_T0005INProw("CASH") = WF_TOTALTOLL.Text
                WW_T0005INProw("TOTALTOLL") = WF_TOTALTOLL.Text
                WW_T0005INProw("KYUYU") = Val(WF_KYUYU.Text).ToString("#,0.00")
                WW_T0005INProw("DELFLG") = WF_DELFLG_H.Text
                SetNameValue(WW_T0005INProw)
                T0005INPtbl.Rows.Add(WW_T0005INProw)
            End If

            Dim WW_LineCnt_Max As Integer = 0
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                If T0005INPtbl.Rows(i)("LINECNT") > WW_LineCnt_Max Then WW_LineCnt_Max = WW_LineCnt_Max + 1
            Next

            'KEY設定
            T0005INProw("LINECNT") = WW_LineCnt_Max + 1
            If O_RTN = C_MESSAGE_NO.NORMAL Then
                T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                T0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

            T0005INProw("TIMSTP") = "0"
            T0005INProw("SELECT") = 1
            T0005INProw("HIDDEN") = 0
            T0005INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T0005INProw("SHIPORG") = work.WF_SEL_UORG.Text
            T0005INProw("TERMKBN") = GRT00005WRKINC.TERM_TYPE.HAND

            T0005INProw("HDKBN") = "D"
            SetNameValue(T0005INProw)
            T0005INPtbl.Rows.Add(T0005INProw)

            CS0026TBLSORT.TABLE = T0005INPtbl
            CS0026TBLSORT.SORTING = "YMD ASC, STAFFCODE ASC, HDKBN DESC, STDATE ASC, STTIME ASC, SEQ ASC"
            CS0026TBLSORT.FILTER = ""
            T0005INPtbl = CS0026TBLSORT.sort()
            '出庫の場合、入力された業務車番、統一車番、日報番号を同一（～帰庫まで）レコードに反映する
            Dim WW_F1flg As Boolean = False
            Dim WW_STpos As Integer = 0
            For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                Dim WW_F1row As DataRow = T0005INPtbl.Rows(i)
                If WW_F1row("WORKKBN") = "F1" Then
                    WW_F1flg = True
                    For j As Integer = WW_STpos To T0005INPtbl.Rows.Count - 1
                        Dim WW_T5row As DataRow = T0005INPtbl.Rows(j)
                        If WW_T5row("HDKBN") = "H" Then
                            WW_T5row("CREWKBN") = WW_F1row("CREWKBN")
                            WW_T5row("CREWKBNNAMES") = WW_F1row("CREWKBNNAMES")
                            Continue For
                        End If
                        If WW_F1row("CREWKBN") = WW_T5row("CREWKBN") AndAlso
                           WW_F1row("GSHABAN") = WW_T5row("GSHABAN") AndAlso
                           WW_F1row("NIPPONO") = WW_T5row("NIPPONO") Then
                            If WW_T5row("WORKKBN") = "A1" OrElse WW_T5row("WORKKBN") = "Z1" Then
                                WW_T5row("CREWKBN") = WW_F1row("CREWKBN")
                                WW_T5row("CREWKBNNAMES") = WW_F1row("CREWKBNNAMES")
                            Else
                                WW_T5row("GSHABAN") = WW_F1row("GSHABAN")
                                WW_T5row("SHARYOTYPEF") = WW_F1row("SHARYOTYPEF")
                                WW_T5row("TSHABANF") = WW_F1row("TSHABANF")
                                WW_T5row("SHARYOTYPEB") = WW_F1row("SHARYOTYPEB")
                                WW_T5row("TSHABANB") = WW_F1row("TSHABANB")
                                WW_T5row("SHARYOTYPEB2") = WW_F1row("SHARYOTYPEB2")
                                WW_T5row("TSHABANB2") = WW_F1row("TSHABANB2")
                                WW_T5row("NIPPONO") = WW_F1row("NIPPONO")
                                WW_T5row("CREWKBN") = WW_F1row("CREWKBN")
                                WW_T5row("CREWKBNNAMES") = WW_F1row("CREWKBNNAMES")
                            End If
                            If WW_T5row("WORKKBN") = "Z1" Then
                                WW_STpos = j + 1
                                i = WW_STpos
                                Exit For
                            End If
                        End If
                    Next
                End If
            Next

            If Not WW_F1flg Then
                WW_CREWKBN = ""
                Dim WW_CREWKBNNAME As String = ""
                For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
                    If T0005INPtbl.Rows(i)("HDKBN") = "H" Then
                        WW_CREWKBN = T0005INPtbl.Rows(i)("CREWKBN")
                        WW_CREWKBNNAME = T0005INPtbl.Rows(i)("CREWKBNNAMES")
                        Exit For
                    End If
                Next
                For i As Integer = WW_STpos To T0005INPtbl.Rows.Count - 1
                    If T0005INPtbl.Rows(i)("HDKBN") = "D" Then
                        T0005INPtbl.Rows(i)("CREWKBN") = WW_CREWKBN
                        T0005INPtbl.Rows(i)("CREWKBNNAMES") = WW_CREWKBNNAME
                    End If
                Next
            End If
        End If

    End Sub

    ''' <summary>
    ''' 更新項目設定
    ''' </summary>
    ''' <param name="T0005INProw">設定したい行</param>
    ''' <param name="I_ROWIDX">設定箇所</param>
    ''' <remarks></remarks>
    Protected Sub SetUpdateValue(ByVal T0005INProw As DataRow, ByVal I_ROWIDX As Integer)

        '〇項目設定
        With T0005INPtbl.Rows(I_ROWIDX)
            .Item("ORDERUMU") = T0005INProw.Item("ORDERUMU")
            .Item("WORKKBN") = T0005INProw.Item("WORKKBN")
            .Item("WORKKBNNAMES") = T0005INProw.Item("WORKKBNNAMES")
            .Item("CREWKBN") = T0005INProw.Item("CREWKBN")
            .Item("CREWKBNNAMES") = T0005INProw.Item("CREWKBNNAMES")
            .Item("TERMKBN") = T0005INProw.Item("TERMKBN")
            .Item("TERMKBNNAMES") = T0005INProw.Item("TERMKBNNAMES")
            .Item("SEQ") = T0005INProw.Item("SEQ")
            .Item("NIPPONO") = T0005INProw.Item("NIPPONO")
            .Item("GSHABAN") = T0005INProw.Item("GSHABAN")
            .Item("GSHABANLICNPLTNO") = T0005INProw.Item("GSHABANLICNPLTNO")
            .Item("STDATE") = T0005INProw.Item("STDATE")
            .Item("STTIME") = T0005INProw.Item("STTIME")
            .Item("ENDDATE") = T0005INProw.Item("ENDDATE")
            .Item("ENDTIME") = T0005INProw.Item("ENDTIME")
            .Item("WORKTIME") = T0005INProw.Item("WORKTIME")
            .Item("STMATER") = T0005INProw.Item("STMATER")
            .Item("ENDMATER") = T0005INProw.Item("ENDMATER")
            .Item("TORICODE") = T0005INProw.Item("TORICODE")
            .Item("TORINAMES") = T0005INProw.Item("TORINAMES")
            .Item("SHUKABASHO") = T0005INProw.Item("SHUKABASHO")
            .Item("SHUKABASHONAMES") = T0005INProw.Item("SHUKABASHONAMES")
            .Item("SHUKADATE") = T0005INProw.Item("SHUKADATE")
            .Item("TODOKECODE") = T0005INProw.Item("TODOKECODE")
            .Item("TODOKENAMES") = T0005INProw.Item("TODOKENAMES")
            .Item("TODOKEDATE") = T0005INProw.Item("TODOKEDATE")
            .Item("OILTYPE1") = T0005INProw.Item("OILTYPE1")
            .Item("PRODUCT11") = T0005INProw.Item("PRODUCT11")
            .Item("PRODUCT21") = T0005INProw.Item("PRODUCT21")
            .Item("PRODUCTCODE1") = T0005INProw.Item("PRODUCTCODE1")
            .Item("PRODUCT1NAMES") = T0005INProw.Item("PRODUCT1NAMES")
            .Item("SURYO1") = Val(T0005INProw.Item("SURYO1"))
            .Item("OILTYPE2") = T0005INProw.Item("OILTYPE2")
            .Item("PRODUCT12") = T0005INProw.Item("PRODUCT12")
            .Item("PRODUCT22") = T0005INProw.Item("PRODUCT22")
            .Item("PRODUCTCODE2") = T0005INProw.Item("PRODUCTCODE2")
            .Item("PRODUCT2NAMES") = T0005INProw.Item("PRODUCT2NAMES")
            .Item("SURYO2") = Val(T0005INProw.Item("SURYO2"))
            .Item("OILTYPE3") = T0005INProw.Item("OILTYPE3")
            .Item("PRODUCT13") = T0005INProw.Item("PRODUCT13")
            .Item("PRODUCT23") = T0005INProw.Item("PRODUCT23")
            .Item("PRODUCTCODE3") = T0005INProw.Item("PRODUCTCODE3")
            .Item("PRODUCT3NAMES") = T0005INProw.Item("PRODUCT3NAMES")
            .Item("SURYO3") = Val(T0005INProw.Item("SURYO3"))
            .Item("OILTYPE4") = T0005INProw.Item("OILTYPE4")
            .Item("PRODUCT14") = T0005INProw.Item("PRODUCT14")
            .Item("PRODUCT24") = T0005INProw.Item("PRODUCT24")
            .Item("PRODUCTCODE4") = T0005INProw.Item("PRODUCTCODE4")
            .Item("PRODUCT4NAMES") = T0005INProw.Item("PRODUCT4NAMES")
            .Item("SURYO4") = Val(T0005INProw.Item("SURYO4"))
            .Item("OILTYPE5") = T0005INProw.Item("OILTYPE5")
            .Item("PRODUCT15") = T0005INProw.Item("PRODUCT15")
            .Item("PRODUCT25") = T0005INProw.Item("PRODUCT25")
            .Item("PRODUCTCODE5") = T0005INProw.Item("PRODUCTCODE5")
            .Item("PRODUCT5NAMES") = T0005INProw.Item("PRODUCT5NAMES")
            .Item("SURYO5") = Val(T0005INProw.Item("SURYO5"))
            .Item("OILTYPE6") = T0005INProw.Item("OILTYPE6")
            .Item("PRODUCT16") = T0005INProw.Item("PRODUCT16")
            .Item("PRODUCT26") = T0005INProw.Item("PRODUCT26")
            .Item("PRODUCTCODE6") = T0005INProw.Item("PRODUCTCODE6")
            .Item("PRODUCT6NAMES") = T0005INProw.Item("PRODUCT6NAMES")
            .Item("SURYO6") = Val(T0005INProw.Item("SURYO6"))
            .Item("OILTYPE7") = T0005INProw.Item("OILTYPE7")
            .Item("PRODUCT17") = T0005INProw.Item("PRODUCT17")
            .Item("PRODUCT27") = T0005INProw.Item("PRODUCT27")
            .Item("PRODUCTCODE7") = T0005INProw.Item("PRODUCTCODE7")
            .Item("PRODUCT7NAMES") = T0005INProw.Item("PRODUCT7NAMES")
            .Item("SURYO7") = Val(T0005INProw.Item("SURYO7"))
            .Item("OILTYPE8") = T0005INProw.Item("OILTYPE8")
            .Item("PRODUCT18") = T0005INProw.Item("PRODUCT18")
            .Item("PRODUCT28") = T0005INProw.Item("PRODUCT28")
            .Item("PRODUCTCODE8") = T0005INProw.Item("PRODUCTCODE8")
            .Item("PRODUCT8NAMES") = T0005INProw.Item("PRODUCT8NAMES")
            .Item("SURYO8") = Val(T0005INProw.Item("SURYO8"))
            .Item("TOTALSURYO") = Val(T0005INProw.Item("SURYO1")) + Val(T0005INProw.Item("SURYO2")) + Val(T0005INProw.Item("SURYO4")) + Val(T0005INProw.Item("SURYO5")) + Val(T0005INProw.Item("SURYO6")) + Val(T0005INProw.Item("SURYO7")) + Val(T0005INProw.Item("SURYO8"))
            .Item("TOTALTOLL") = T0005INProw.Item("TOTALTOLL")
            .Item("KYUYU") = T0005INProw.Item("KYUYU")
            .Item("SOUDISTANCE") = T0005INProw.Item("SOUDISTANCE")
            .Item("ORDERNO") = T0005INProw.Item("ORDERNO")
            .Item("DETAILNO") = T0005INProw.Item("DETAILNO")
            .Item("TRIPNO") = T0005INProw.Item("TRIPNO")
            .Item("DROPNO") = T0005INProw.Item("DROPNO")
            .Item("TUMIOKIKBN") = T0005INProw.Item("TUMIOKIKBN")
            .Item("TUMIOKIKBNNAMES") = T0005INProw.Item("TUMIOKIKBNNAMES")
            .Item("URIKBN") = T0005INProw.Item("URIKBN")
            .Item("URIKBNNAMES") = T0005INProw.Item("URIKBNNAMES")

            .Item("STORICODE") = T0005INProw.Item("STORICODE")
            .Item("STORICODENAMES") = T0005INProw.Item("STORICODENAMES")
            .Item("CONTCHASSIS") = T0005INProw.Item("CONTCHASSIS")
            .Item("CONTCHASSISLICNPLTNO") = T0005INProw.Item("CONTCHASSISLICNPLTNO")

            .Item("SHARYOTYPEF") = Mid(T0005INProw.Item("TSHABANF"), 1, 1)
            .Item("TSHABANF") = T0005INProw.Item("TSHABANF")
            .Item("SHARYOTYPEB") = Mid(T0005INProw.Item("TSHABANB"), 1, 1)
            .Item("TSHABANB") = T0005INProw.Item("TSHABANB")
            .Item("SHARYOTYPEB2") = Mid(T0005INProw.Item("TSHABANB2"), 1, 1)
            .Item("TSHABANB2") = T0005INProw.Item("TSHABANB2")
            .Item("TAXKBN") = T0005INProw.Item("TAXKBN")
            .Item("TAXKBNNAMES") = T0005INProw.Item("TAXKBNNAMES")
            .Item("LATITUDE") = T0005INProw.Item("LATITUDE")
            .Item("LONGITUDE") = T0005INProw.Item("LONGITUDE")
            .Item("DELFLG") = T0005INProw.Item("DELFLG")

            .Item("HOLIDAYKBN") = T0005INProw.Item("HOLIDAYKBN")
            .Item("TORITYPE01") = T0005INProw.Item("TORITYPE01")
            .Item("TORITYPE02") = T0005INProw.Item("TORITYPE02")
            .Item("TORITYPE03") = T0005INProw.Item("TORITYPE03")
            .Item("TORITYPE04") = T0005INProw.Item("TORITYPE04")
            .Item("TORITYPE05") = T0005INProw.Item("TORITYPE05")
            .Item("SUPPLIERKBN") = T0005INProw.Item("SUPPLIERKBN")
            .Item("SUPPLIER") = T0005INProw.Item("SUPPLIER")
            .Item("MANGOILTYPE") = T0005INProw.Item("MANGOILTYPE")
            .Item("MANGMORG1") = T0005INProw.Item("MANGMORG1")
            .Item("MANGSORG1") = T0005INProw.Item("MANGSORG1")
            .Item("MANGUORG1") = T0005INProw.Item("MANGUORG1")
            .Item("BASELEASE1") = T0005INProw.Item("BASELEASE1")
            .Item("MANGMORG2") = T0005INProw.Item("MANGMORG2")
            .Item("MANGSORG2") = T0005INProw.Item("MANGSORG2")
            .Item("MANGUORG2") = T0005INProw.Item("MANGUORG2")
            .Item("BASELEASE2") = T0005INProw.Item("BASELEASE2")
            .Item("MANGMORG3") = T0005INProw.Item("MANGMORG3")
            .Item("MANGSORG3") = T0005INProw.Item("MANGSORG3")
            .Item("MANGUORG3") = T0005INProw.Item("MANGUORG3")
            .Item("BASELEASE3") = T0005INProw.Item("BASELEASE3")
            .Item("STAFFKBN") = T0005INProw.Item("STAFFKBN")
            .Item("MORG") = T0005INProw.Item("MORG")
            .Item("HORG") = T0005INProw.Item("HORG")
            .Item("SUBSTAFFKBN") = T0005INProw.Item("SUBSTAFFKBN")
            .Item("SUBMORG") = T0005INProw.Item("SUBMORG")
            .Item("SUBHORG") = T0005INProw.Item("SUBHORG")
            .Item("ORDERORG") = T0005INProw.Item("ORDERORG")
        End With
    End Sub

    ''' <summary>
    ''' 変更有無チェック
    ''' </summary>
    ''' <param name="T0005INProw" >チェック対象行</param>
    ''' <param name="O_RTN">変更有無　無：変更なし　有：変更あり　新：新規行</param>
    ''' <remarks></remarks>
    Protected Sub isUpdatingRow(ByVal T0005INProw As DataRow, ByRef O_RTN As String)

        O_RTN = "無"

        If T0005INPtbl.Rows.Count = 0 Then
            O_RTN = "新"
        End If

        For Each row As DataRow In T0005INPtbl.Rows

            If row("HDKBN") = "D" AndAlso
                row("SEQ") = T0005INProw("SEQ") Then

                If row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                    If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                        If row("JIDISTANCE") <> 0 Then
                            row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                        End If
                        If row("KUDISTANCE") <> 0 Then
                            row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                        End If
                        If row("IPPDISTANCE") <> 0 Then
                            row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                            row("KOSDISTANCE") = 0
                        End If
                        If row("KOSDISTANCE") <> 0 Then
                            row("IPPDISTANCE") = 0
                            row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                        End If
                        If row("IPPJIDISTANCE") <> 0 Then
                            row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                            row("KOSJIDISTANCE") = 0
                        End If
                        If row("IPPKUDISTANCE") <> 0 Then
                            row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                            row("KOSKUDISTANCE") = 0
                        End If
                    End If
                    O_RTN = "有"
                Else
                    Select Case T0005INProw("WORKKBN")
                        Case "A1", "Z1"
                            O_RTN = "有"
                            Exit For
                        Case "B2"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For

                        Case "B3"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For

                        Case "B4", "B5", "B9", "BA", "BB", "BX"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For

                        Case "BY", "G1"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For

                        Case "F1"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For

                        Case "F3"
                            If row("SOUDISTANCE") <> T0005INProw("SOUDISTANCE") Then
                                If row("JIDISTANCE") <> 0 Then
                                    row("JIDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("KUDISTANCE") <> 0 Then
                                    row("KUDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSDISTANCE") = 0
                                End If
                                If row("KOSDISTANCE") <> 0 Then
                                    row("IPPDISTANCE") = 0
                                    row("KOSDISTANCE") = T0005INProw("SOUDISTANCE")
                                End If
                                If row("IPPJIDISTANCE") <> 0 Then
                                    row("IPPJIDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSJIDISTANCE") = 0
                                End If
                                If row("IPPKUDISTANCE") <> 0 Then
                                    row("IPPKUDISTANCE") = T0005INProw("SOUDISTANCE")
                                    row("KOSKUDISTANCE") = 0
                                End If
                            End If
                            O_RTN = "有"
                            Exit For
                    End Select
                End If
                Exit For
            Else
                O_RTN = "新"
            End If
        Next
    End Sub

    ''' <summary>
    ''' T0005tbl全体関連チェック
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckListTable(ByRef O_RTN As String)

        Dim WW_WORKTIME As Integer = 0
        Dim WW_MOVETIME As Integer = 0
        Dim WW_SOUDISTANCE As Decimal = 0
        Dim WW_JIDISTANCE As Decimal = 0
        Dim WW_KUDISTANCE As Decimal = 0
        Dim WW_IPPDISTANCE As Decimal = 0
        Dim WW_KOSDISTANCE As Decimal = 0
        Dim WW_IPPJIDISTANCE As Decimal = 0
        Dim WW_IPPKUDISTANCE As Decimal = 0
        Dim WW_KOSJIDISTANCE As Decimal = 0
        Dim WW_KOSKUDISTANCE As Decimal = 0
        Dim WW_STMATER As Decimal = 0
        Dim WW_ENDMATER As Decimal = 0
        Dim WW_KYUYU As Decimal = 0
        Dim WW_TOTALTOLL As Integer = 0

        Dim WW_STDATE As DateTime
        Dim WW_ENDDATE As DateTime

        Dim WW_B2CNT As Integer = 0
        Dim WW_B3CNT As Integer = 0
        Dim WW_F1CNT As Integer = 0
        Dim WW_F3CNT As Integer = 0
        Dim WW_A1CNT As Integer = 0
        Dim WW_Z1CNT As Integer = 0
        '---------------------------------------------------
        '■明細行番号（並び順）の振り直し()
        '---------------------------------------------------
        Dim WW_SEQ As Integer = 0

        O_RTN = C_MESSAGE_NO.NORMAL

        '運行日、従業員、タイトル区分（ヘッダ、明細）、開始時刻の順番で処理する
        Dim WW_TBLview As New DataView(T0005INPtbl)
        WW_TBLview.Sort = "YMD ASC, STAFFCODE ASC, HDKBN DESC, STDATE ASC, STTIME ASC, ENDDATE ASC, ENDTIME ASC"

        For i As Integer = 0 To WW_TBLview.Count - 1
            If WW_TBLview.Item(i)("HDKBN") = "H" Then
                WW_TBLview.Item(i)("SEQ") = "001"
                WW_TBLview.Item(i)("LINECNT") = 0
                Continue For
            End If
            WW_SEQ = WW_SEQ + 1
            WW_TBLview.Item(i)("SEQ") = WW_SEQ.ToString("000")
            WW_TBLview.Item(i)("LINECNT") = WW_SEQ

            If WW_TBLview.Item(i)("DELFLG") = C_DELETE_FLG.ALIVE Then
                Select Case WW_TBLview.Item(i)("WORKKBN")
                    Case "B2"
                        WW_B2CNT += 1
                    Case "B3"
                        WW_B3CNT += 1
                    Case "F1"
                        WW_F1CNT += 1
                    Case "F3"
                        WW_F3CNT += 1
                    Case "A1"
                        WW_A1CNT += 1
                    Case "Z1"
                        WW_Z1CNT += 1
                End Select
            End If
        Next

        '始業、終業を除く（退避）
        T0005INPtbl = WW_TBLview.ToTable()

        WW_TBLview.RowFilter = "(HDKBN = 'D' and DELFLG = '0') and (WORKKBN <> 'A1' and WORKKBN <> 'G1' and WORKKBN <> 'Z1')"

        WW_ERRLIST = New List(Of String)

        'ヘッダーレコードは、1レコード目（0番）、明細レコードは、2レコード目（1番～最後）が時系列であること前提
        For i As Integer = 0 To WW_TBLview.Count - 1
            If WW_TBLview.Item(i)("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                WW_TBLview.Item(i)("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If

            '前後の日時大小関係
            WW_STDATE = WW_TBLview.Item(i)("STDATE") + " " + WW_TBLview.Item(i)("STTIME")
            WW_ENDDATE = WW_TBLview.Item(i)("ENDDATE") + " " + WW_TBLview.Item(i)("ENDTIME")
            If WW_ENDDATE < WW_STDATE Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(開始日付＞終了日付エラー)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号 =" & WW_TBLview.Item(i)("SEQ") & "  "
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                WW_TBLview.Item(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If
            '〇日付を昇順に並べているのが前提。前行の終了が自行の開始より後ならエラー
            If i >= 1 Then
                WW_ENDDATE = WW_TBLview.Item(i - 1)("ENDDATE") + " " + WW_TBLview.Item(i - 1)("ENDTIME")
                WW_STDATE = WW_TBLview.Item(i)("STDATE") + " " + WW_TBLview.Item(i)("STTIME")
                If WW_ENDDATE > WW_STDATE Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・エラーが存在します。(終了日付＞開始日付エラー)"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号 =" & WW_TBLview.Item(i - 1)("SEQ") & "  "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号 =" & WW_TBLview.Item(i)("SEQ") & "  "
                    rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                    WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                    WW_TBLview.Item(i - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_TBLview.Item(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If

                '移動時間
                WW_MOVETIME = DateDiff("n",
                                      WW_TBLview.Item(i - 1)("ENDDATE") + " " + WW_TBLview.Item(i - 1)("ENDTIME"),
                                      WW_TBLview.Item(i)("STDATE") + " " + WW_TBLview.Item(i)("STTIME")
                                     )
                WW_TBLview.Item(i)("MOVETIME") = T0005COM.MinutestoHHMM(WW_MOVETIME)

            End If

            '作業時間
            WW_WORKTIME = DateDiff("n",
                                  WW_TBLview.Item(i)("STDATE") + " " + WW_TBLview.Item(i)("STTIME"),
                                  WW_TBLview.Item(i)("ENDDATE") + " " + WW_TBLview.Item(i)("ENDTIME")
                                 )
            WW_TBLview.Item(i)("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            Dim WW_ACT As Integer = WW_MOVETIME + WW_WORKTIME
            WW_TBLview.Item(i)("ACTTIME") = T0005COM.MinutestoHHMM(WW_ACT)

            If WW_TBLview.Item(i)("WORKKBN") = "F3" Then
                '走行距離
                WW_SOUDISTANCE = WW_SOUDISTANCE + WW_TBLview.Item(i)("SOUDISTANCE")
                WW_JIDISTANCE = WW_JIDISTANCE + WW_TBLview.Item(i)("JIDISTANCE")
                WW_KUDISTANCE = WW_KUDISTANCE + WW_TBLview.Item(i)("KUDISTANCE")
                WW_IPPDISTANCE = WW_IPPDISTANCE + WW_TBLview.Item(i)("IPPDISTANCE")
                WW_KOSDISTANCE = WW_KOSDISTANCE + WW_TBLview.Item(i)("KOSDISTANCE")
                WW_IPPJIDISTANCE = WW_IPPJIDISTANCE + WW_TBLview.Item(i)("IPPJIDISTANCE")
                WW_IPPKUDISTANCE = WW_IPPKUDISTANCE + WW_TBLview.Item(i)("IPPKUDISTANCE")
                WW_KOSJIDISTANCE = WW_KOSJIDISTANCE + WW_TBLview.Item(i)("KOSJIDISTANCE")
                WW_KOSKUDISTANCE = WW_KOSKUDISTANCE + WW_TBLview.Item(i)("KOSKUDISTANCE")
            End If
            WW_STMATER = WW_STMATER + WW_TBLview.Item(i)("STMATER")
            WW_ENDMATER = WW_ENDMATER + WW_TBLview.Item(i)("ENDMATER")
            WW_KYUYU = WW_KYUYU + WW_TBLview.Item(i)("KYUYU")
            WW_TOTALTOLL = WW_TOTALTOLL + WW_TBLview.Item(i)("TOTALTOLL")
        Next

        For i As Integer = 0 To T0005INPtbl.Rows.Count - 1
            For j As Integer = 0 To WW_TBLview.Count - 1
                If T0005INPtbl.Rows(i)("HDKBN") = "D" AndAlso T0005INPtbl.Rows(i)("SEQ") = WW_TBLview.Item(j)("SEQ") Then
                    T0005INPtbl.Rows(i).ItemArray = WW_TBLview.Item(j).Row.ItemArray
                End If
            Next
        Next

        WW_TBLview.RowFilter = "(HDKBN = 'D' and DELFLG = '0')"

        If WW_TBLview.Count > 0 Then
            Dim idx As Integer
            For idx = 0 To T0005INPtbl.Rows.Count - 1
                If T0005INPtbl.Rows(idx)("HDKBN") = "H" Then
                    Exit For
                End If
            Next
            'ヘッダレコード編集
            T0005INPtbl.Rows(idx)("STDATE") = WW_TBLview.Item(0)("STDATE")
            T0005INPtbl.Rows(idx)("STTIME") = WW_TBLview.Item(0)("STTIME")
            T0005INPtbl.Rows(idx)("ENDDATE") = WW_TBLview.Item(WW_TBLview.Count - 1)("ENDDATE")
            T0005INPtbl.Rows(idx)("ENDTIME") = WW_TBLview.Item(WW_TBLview.Count - 1)("ENDTIME")
            '稼働時間計算
            WW_STDATE = T0005INPtbl.Rows(idx)("STDATE") & " " & T0005INPtbl.Rows(idx)("STTIME")
            WW_ENDDATE = T0005INPtbl.Rows(idx)("ENDDATE") & " " & T0005INPtbl.Rows(idx)("ENDTIME")
            WW_WORKTIME = DateDiff("n", WW_STDATE, WW_ENDDATE)
            T0005INPtbl.Rows(idx)("WORKTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            T0005INPtbl.Rows(idx)("MOVETIME") = 0
            T0005INPtbl.Rows(idx)("ACTTIME") = T0005COM.MinutestoHHMM(WW_WORKTIME)
            '走行距離
            T0005INPtbl.Rows(idx)("SOUDISTANCE") = WW_SOUDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("JIDISTANCE") = WW_JIDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("KUDISTANCE") = WW_KUDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("IPPDISTANCE") = WW_IPPDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("KOSDISTANCE") = WW_KOSDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("IPPJIDISTANCE") = WW_IPPJIDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("IPPKUDISTANCE") = WW_IPPKUDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("KOSJIDISTANCE") = WW_KOSJIDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("KOSKUDISTANCE") = WW_KOSKUDISTANCE.ToString("0.00")
            T0005INPtbl.Rows(idx)("KYUYU") = Format(Val(WW_KYUYU), "0.00")
            T0005INPtbl.Rows(idx)("TOTALTOLL") = Format(Val(WW_TOTALTOLL), "0")
            '〇B2、B3のいずれかが１件でもある場合、F1、F3は必須
            If WW_B2CNT > 0 OrElse WW_B3CNT > 0 Then
                If WW_F1CNT = 0 Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・エラーが存在します。(作業区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫（F1）が存在しません,"
                    rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                    WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                    T0005INPtbl.Rows(idx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If

                If WW_F3CNT = 0 Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・エラーが存在します。(作業区分エラー)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 帰庫（F3）が存在しません,"
                    rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                    WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                    T0005INPtbl.Rows(idx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
                End If
            End If

            If WW_A1CNT = 0 Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(作業区分エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 始業（A1）が存在しません,"
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                T0005INPtbl.Rows(idx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If

            If WW_Z1CNT = 0 Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(作業区分エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終業（Z1）が存在しません,"
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                T0005INPtbl.Rows(idx)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            End If


            '48時間（2日）以上の勤務はエラー（最終レコードをエラーとする）
            If WW_WORKTIME > 2880 Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・エラーが存在します。(稼働時間エラー)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 稼働時間が４８時間を超過しています。  "
                rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERRLIST.Add(C_MESSAGE_NO.BOX_ERROR_EXIST)
                T0005INPtbl.Rows(T0005INPtbl.Rows.Count - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST
            Else
                If T0005INPtbl.Rows(T0005INPtbl.Rows.Count - 1)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                    T0005INPtbl.Rows(T0005INPtbl.Rows.Count - 1)("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If
            End If

        End If

        CS0026TBLSORT.TABLE = T0005INPtbl
        CS0026TBLSORT.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, SEQ"
        CS0026TBLSORT.FILTER = ""
        T0005INPtbl = CS0026TBLSORT.sort()


        WW_TBLview.Dispose()
        WW_TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 詳細情報から項目を取得する
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_VALUE">項目値</param>
    ''' <remarks></remarks>
    Protected Sub FindRepeaterItem(ByVal I_FIELD As String, ByRef O_VALUE As String)

        'タブから指定された項目の値を取得
        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1
            If CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                O_VALUE = CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text
                Exit For
            End If
            If CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                O_VALUE = CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text
                Exit For
            End If
            If CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                O_VALUE = CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢを検索し、受注番号を取得する
    ''' </summary>
    ''' <param name="IO_ROW">設定用テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub SetOrderForHOrder(ByRef IO_ROW As DataRow, ByRef O_RTN As String)

        Dim SQLStr As String = ""
        Dim SQLWhere As String = ""
        Dim SQLSort As String = ""

        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                SQLStr =
                           " SELECT isnull(rtrim(ORDERNO), '')     as ORDERNO " _
                         & "       ,isnull(rtrim(ORDERORG),'')     as ORDERORG " _
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

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader(), WW_T0004tbl As New DataTable
                        WW_T0004tbl.Columns.Add("ORDERNO", GetType(String))
                        WW_T0004tbl.Columns.Add("ORDERORG", GetType(String))

                        WW_T0004tbl.Load(SQLdr)

                        If WW_T0004tbl.Rows.Count > 0 Then
                            IO_ROW("ORDERNO") = WW_T0004tbl.Rows(0)("ORDERNO")
                            IO_ROW("ORDERORG") = WW_T0004tbl.Rows(0)("ORDERORG")
                        Else
                            IO_ROW("ORDERNO") = C_LIST_OPERATION_CODE.NODATA
                        End If

                    End Using
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "SetOrderForHOrder"                   'SUBクラス名
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
    ''' T0005tbl編集（矢崎のみ） ２マンの再編集（トリップ毎に存在をチェックし、存在しない場合の日報を削除
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub EditTwoManRecordByYazaki(ByRef O_RTN As String)
        Dim WW_Cols As String() = {"YMD", "NIPPONO", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = T0005INPtbl.Clone
        Dim WW_T0005INPtbl As DataTable = T0005INPtbl.Clone
        Dim WW_TWOMANtbl As DataTable = T0005INPtbl.Clone
        Dim WW_TBLview As DataView

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '正乗務員のレコードが存在するか確認し存在すれば、２マン編集
            WW_TBLview = New DataView(T0005INPtbl)
            WW_TBLview.Sort = "YMD, NIPPONO, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'H' and CREWKBN = '2'"

            If WW_TBLview.Count > 0 Then
                Exit Sub
            End If

            'A1に日報番号が入っていない（古いデータ）は、正しく処理できないため処理を中止する
            WW_TBLview = New DataView(T0005INPtbl)
            WW_TBLview.Sort = "YMD, NIPPONO, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'D' and WORKKBN = 'A1' and NIPPONO = ''"
            If WW_TBLview.Count > 0 Then
                Exit Sub
            End If

            '２マン対象データ抽出
            WW_TBLview = New DataView(T0005INPtbl)
            WW_TBLview.Sort = "YMD, NIPPONO, STAFFCODE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'D' and TERMKBN = '" & GRT00005WRKINC.TERM_TYPE.YAZAKI & "'"

            WW_T0005INPtbl = WW_TBLview.ToTable

            '出庫日、乗務員でグループ化しキーテーブル作成
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            '+++++++++++++++++++++++++++++++++
            '出庫日、乗務員毎に処理
            Dim WW_IDX As Integer = 0
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                WW_T0005tbl.Clear()
                For i As Integer = WW_IDX To WW_T0005INPtbl.Rows.Count - 1
                    If WW_KEYrow("YMD") = WW_T0005INPtbl.Rows(i)("YMD") And
                        WW_KEYrow("NIPPONO") = WW_T0005INPtbl.Rows(i)("NIPPONO") And
                        WW_KEYrow("STAFFCODE") = WW_T0005INPtbl.Rows(i)("STAFFCODE") Then
                        Dim WW_Row As DataRow = WW_T0005tbl.NewRow
                        WW_Row.ItemArray = WW_T0005INPtbl.Rows(i).ItemArray
                        WW_T0005tbl.Rows.Add(WW_Row)
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                Dim WW_TWOMANTRIP As Integer = 1
                Dim WW_F1 As Integer = 0            '出庫
                Dim WW_B2POS As Integer = 0         '積荷
                Dim WW_B3POS As Integer = 0         '荷卸

                For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                    Dim T5row As DataRow = WW_T0005tbl.Rows(i)
                    If T5row("WORKKBN") = "F1" Then
                        WW_F1 += 1
                        '出庫が出現するたびに（複数回の場合の考慮）２マン用トリップをクリアする
                        If WW_F1 > 1 Then WW_TWOMANTRIP = 1
                    End If
                    T5row("TWOMANTRIP") = WW_TWOMANTRIP

                    If T5row("WORKKBN") = "B2" Then
                        WW_B2POS = i '最後のB2のポジション

                        '荷積の前に荷卸があったら次のトリップ
                        If WW_B2POS > WW_B3POS Then
                            If WW_B3POS = 0 Then
                                '初めての荷積は次の荷積まで同一トリップ（加算しない）
                            Else
                                WW_TWOMANTRIP += 1
                                T5row("TWOMANTRIP") = WW_TWOMANTRIP
                            End If
                        End If
                    End If

                    If T5row("WORKKBN") = "B3" Then
                        WW_B3POS = i '最初のB3のポジション
                    End If

                Next
                '配送受注を検索し、副乗務員の存在をチェック
                Dim WW_SUBSTAFFCODE As String = ""
                Dim WW_SUBSTAFFCODE_SV As String = ""
                Dim WW_SUBSTAFFCODE2 As String = ""
                Dim WW_B2cnt As Integer = 0
                Dim WW_B3First As Boolean = False
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
                            WW_B3First = True
                            WW_STtrip = WW_T5row("TWOMANTRIP")
                        End If
                    ElseIf WW_T5row("WORKKBN") = "B2" Then
                        WW_B2cnt += 1
                        CheckTwoManRecord(WW_T5row, WW_B3First, WW_SUBSTAFFCODE, WW_ERRCODE)
                        If WW_ERRCODE = C_MESSAGE_NO.DB_ERROR Then
                            O_RTN = WW_ERRCODE
                            Exit Sub
                        End If

                        If WW_SUBSTAFFCODE <> "" Then
                            WW_SUBSTAFFCODE_SV = WW_SUBSTAFFCODE
                            If WW_B2cnt = 1 Then
                                If Not WW_B3First Then
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
                End If
                If Not WW_TWOMAN Then
                    If WW_STtrip2 <> 0 AndAlso WW_ENDtrip2 <> 0 Then
                        WW_SUBSTAFFCODE_SV = WW_SUBSTAFFCODE2
                        WW_STtrip = WW_STtrip2
                        WW_ENDtrip = WW_ENDtrip2
                        WW_TWOMAN = True
                    End If
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
                            CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_DUMMY)
                            TWOMANrow("SUBSTAFFCODE") = ""
                            TWOMANrow("SUBSTAFFNAMES") = ""
                            TWOMANrow("CREWKBN") = "2"
                            TWOMANrow("CREWKBNNAMES") = ""
                            CodeToName("CREWKBN", WW_T0005tbl.Rows(i)("CREWKBN"), WW_T0005tbl.Rows(i)("CREWKBNNAMES"), WW_DUMMY)

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
                            CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_DUMMY)
                            TWOMANrow("SHIPORGNAMES") = ""
                            CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_DUMMY)
                            TWOMANrow("TERMKBNNAMES") = ""
                            CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_DUMMY)
                            TWOMANrow("WORKKBNNAMES") = ""
                            CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_DUMMY)
                            TWOMANrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_DUMMY)
                            TWOMANrow("CREWKBNNAMES") = ""
                            CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_DUMMY)
                            WW_WORKtbl.Rows.Add(TWOMANrow)

                            Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                            TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                            TWOMANrow2("WORKKBN") = "A1"
                            TWOMANrow2("WORKKBNNAMES") = ""
                            CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_DUMMY)
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
                            CodeToName("CAMPCODE", TWOMANrow("CAMPCODE"), TWOMANrow("CAMPNAMES"), WW_DUMMY)
                            TWOMANrow("SHIPORGNAMES") = ""
                            CodeToName("SHIPORG", TWOMANrow("SHIPORG"), TWOMANrow("SHIPORGNAMES"), WW_DUMMY)
                            TWOMANrow("TERMKBNNAMES") = ""
                            CodeToName("TERMKBN", TWOMANrow("TERMKBN"), TWOMANrow("TERMKBNNAMES"), WW_DUMMY)
                            TWOMANrow("WORKKBNNAMES") = ""
                            CodeToName("WORKKBN", TWOMANrow("WORKKBN"), TWOMANrow("WORKKBNNAMES"), WW_DUMMY)
                            TWOMANrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TWOMANrow("STAFFCODE"), TWOMANrow("STAFFNAMES"), WW_DUMMY)
                            TWOMANrow("CREWKBNNAMES") = ""
                            CodeToName("CREWKBN", TWOMANrow("CREWKBN"), TWOMANrow("CREWKBNNAMES"), WW_DUMMY)
                            WW_WORKtbl.Rows.Add(TWOMANrow)

                            Dim TWOMANrow2 As DataRow = WW_WORKtbl.NewRow
                            TWOMANrow2.ItemArray = TWOMANrow.ItemArray
                            TWOMANrow2("WORKKBN") = "Z1"
                            TWOMANrow2("WORKKBNNAMES") = ""
                            CodeToName("WORKKBN", TWOMANrow2("WORKKBN"), TWOMANrow2("WORKKBNNAMES"), WW_DUMMY)
                            WW_WORKtbl.Rows.Add(TWOMANrow2)

                            Exit For
                        Next

                        WW_TWOMANtbl.Merge(WW_WORKtbl)
                    End If
                End If

            Next
            WW_Cols = Nothing
            WW_Cols = {"YMD", "STAFFCODE"}
            WW_TBLview = New DataView(WW_TWOMANtbl)
            WW_TBLview.Sort = "HDKBN DESC, YMD, STAFFCODE, NIPPONO, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'D'"
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            '①前回データを削除（タイムスタンプがゼロ(=0)なら物理削除、ゼロ以外(<>0)なら論理削除）
            Dim WW_TIMSTP As String = ""
            WW_T0005tbl.Clear()
            '前回データのキー項目を検索
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                '対象データを別テーブルに抽出
                For i As Integer = T0005tbl.Rows.Count - 1 To 0 Step -1
                    If T0005tbl.Rows(i)("YMD") = WW_KEYrow("YMD") AndAlso
                        T0005tbl.Rows(i)("STAFFCODE") = WW_KEYrow("STAFFCODE") AndAlso
                        T0005tbl.Rows(i)("SELECT") = "1" Then
                        Dim WW_Row As DataRow = WW_T0005tbl.NewRow
                        WW_Row.ItemArray = T0005tbl.Rows(i).ItemArray
                        WW_T0005tbl.Rows.Add(WW_Row)

                        'コピーしたら元データを消す
                        T0005tbl.Rows(i).Delete()
                    End If
                Next
            Next

            WW_Cols = Nothing
            WW_Cols = {"YMD", "NIPPONO", "STAFFCODE"}
            WW_TBLview = New DataView(WW_TWOMANtbl)
            WW_TBLview.Sort = "HDKBN DESC, YMD, STAFFCODE, NIPPONO, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_TBLview.RowFilter = "HDKBN = 'D'"
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            '抽出したデータから削除処理を行う（日付、乗務員、日報番号単位）
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                For i As Integer = WW_T0005tbl.Rows.Count - 1 To 0 Step -1
                    If WW_T0005tbl.Rows(i)("HDKBN") = "H" Then
                        If WW_T0005tbl.Rows(i)("TIMSTP") = 0 Then
                            'タイムスタンプがゼロ(=0)なら物理削除
                            WW_T0005tbl.Rows(i).Delete()
                        Else
                            'タイムスタンプがゼロ以外(<>0)なら論理削除
                            WW_T0005tbl.Rows(i)("LINECNT") = "0"
                            WW_T0005tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_T0005tbl.Rows(i)("SELECT") = "0"
                            WW_T0005tbl.Rows(i)("HIDDEN") = "1"
                            WW_T0005tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        End If
                        Continue For
                    End If

                    If WW_T0005tbl.Rows(i)("YMD") = WW_KEYrow("YMD") AndAlso
                        WW_T0005tbl.Rows(i)("STAFFCODE") = WW_KEYrow("STAFFCODE") AndAlso
                        WW_T0005tbl.Rows(i)("NIPPONO") = WW_KEYrow("NIPPONO") AndAlso
                        WW_T0005tbl.Rows(i)("SELECT") = "1" Then
                        If WW_T0005tbl.Rows(i)("TIMSTP") = 0 Then
                            'タイムスタンプがゼロ(=0)なら物理削除
                            WW_T0005tbl.Rows(i).Delete()
                        Else
                            'タイムスタンプがゼロ以外(<>0)なら論理削除
                            WW_T0005tbl.Rows(i)("LINECNT") = "0"
                            WW_T0005tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_T0005tbl.Rows(i)("SELECT") = "0"
                            WW_T0005tbl.Rows(i)("HIDDEN") = "1"
                            WW_T0005tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        End If
                    End If
                Next
            Next

            '削除データを元に戻して、自テーブルから削除する
            For i As Integer = WW_T0005tbl.Rows.Count - 1 To 0 Step -1
                If WW_T0005tbl.Rows(i)("SELECT") = "0" Then
                    Dim WW_Row As DataRow = T0005tbl.NewRow
                    WW_Row.ItemArray = WW_T0005tbl.Rows(i).ItemArray
                    T0005tbl.Rows.Add(WW_Row)

                    WW_T0005tbl.Rows(i).Delete()
                End If
            Next

            WW_T0005tbl.Merge(WW_TWOMANtbl)

            '------------------------------------------------------------
            '■出庫日、従業員 単位
            '  出庫日、従業員毎に集約し直す
            '------------------------------------------------------------
            CreateHeaderRecordForT0005(WW_T0005tbl)

            '---------------------------------------------------
            '■出庫日、従業員 単位
            '  明細行番号（並び順）の振り直し
            '---------------------------------------------------
            Dim WW_SEQ As Integer = 1

            For i As Integer = 0 To WW_T0005tbl.Rows.Count - 1
                '行番号の採番
                If WW_T0005tbl.Rows(i)("HDKBN") = "H" Then
                    WW_SEQ = 1
                    WW_T0005tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                    Continue For
                End If
                WW_T0005tbl.Rows(i)("SEQ") = WW_SEQ.ToString("000")
                WW_SEQ = WW_SEQ + 1
            Next

            '２マンレコードの追加（ヘッダあり）
            T0005INPtbl.Merge(WW_T0005tbl)

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
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "例外発生")
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
    ''' 配送受注テーブルを検索し、副乗車員を取得する
    ''' </summary>
    ''' <param name="I_ROW">チェック対象行</param>
    ''' <param name="I_IS_B3_FIRST">B3が先頭か</param>
    ''' <param name="O_STAFFCODE">副乗務員コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckTwoManRecord(ByRef I_ROW As DataRow, ByVal I_IS_B3_FIRST As Boolean, ByRef O_STAFFCODE As String, ByRef O_RTN As String)

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
                    If I_IS_B3_FIRST Then
                        'B3が先の場合、トリップ№－１で検索
                        PARA5.Value = (Val(I_ROW("TRIPNO")) - 1).ToString("000")
                    Else
                        PARA5.Value = I_ROW("TRIPNO")
                    End If
                    PARA6.Value = I_ROW("STAFFCODE")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        O_STAFFCODE = ""

                        While SQLdr.Read
                            O_STAFFCODE = SQLdr("SUBSTAFFCODE")
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "Chk_Twoman"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ' ***     
    ''' <summary>
    ''' ヘッダレコード作成
    ''' </summary>
    ''' <param name="IO_TBL">作成するテーブル</param>
    ''' <remarks></remarks>
    Protected Sub CreateHeaderRecordForT0005(ByRef IO_TBL As DataTable)

        Dim WW_LINECNT As Integer = 0
        Dim WW_Cols As String() = {"YMD", "STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_T0005tbl As DataTable = IO_TBL.Clone
        Dim WW_TBLview As DataView
        Dim WW_T0005row As DataRow

        Try
            '更新元（削除）データをキープ
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT"
            CS0026TBLSORT.FILTER = "SELECT = '0'"
            Dim WW_T0005DELtbl As DataTable = CS0026TBLSORT.sort()

            '出庫日、乗務員でグループ化しキーテーブル作成
            CS0026TBLSORT.TABLE = IO_TBL
            CS0026TBLSORT.SORTING = "SELECT"
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and SELECT = '1'"
            Dim WW_T0005SELtbl As DataTable = CS0026TBLSORT.sort()
            'キーテーブル作成
            WW_TBLview = New DataView(WW_T0005SELtbl)
            WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

            '抽出後のテーブルに置き換える（ヘッダなし、明細のみ）
            IO_TBL = WW_T0005SELtbl.Copy()

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
                Dim WW_DEL_FLG As Boolean = True

                '初期化
                WW_T0005row = WW_T0005tbl.NewRow
                T0005COM.InitialT5INPRow(WW_T0005row)
                WW_T0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                WW_T0005row("SHIPORG") = work.WF_SEL_UORG.Text

                For Each WW_SELrow As DataRow In WW_T0005SELtbl.Rows
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
                            WW_DEL_FLG = False
                        End If
                    Else

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
                If WW_DEL_FLG Then
                    WW_T0005row("DELFLG") = C_DELETE_FLG.DELETE
                Else
                    WW_T0005row("DELFLG") = C_DELETE_FLG.ALIVE
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
                CodeToName("CAMPCODE", WW_T0005row("CAMPCODE"), WW_T0005row("CAMPNAMES"), WW_DUMMY)
                WW_T0005row("SHIPORGNAMES") = ""
                CodeToName("SHIPORG", WW_T0005row("SHIPORG"), WW_T0005row("SHIPORGNAMES"), WW_DUMMY)
                WW_T0005row("TERMKBNNAMES") = ""
                CodeToName("TERMKBN", WW_T0005row("TERMKBN"), WW_T0005row("TERMKBNNAMES"), WW_DUMMY)
                WW_T0005row("STAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("STAFFCODE"), WW_T0005row("STAFFNAMES"), WW_DUMMY)
                WW_T0005row("SUBSTAFFNAMES") = ""
                CodeToName("STAFFCODE", WW_T0005row("SUBSTAFFCODE"), WW_T0005row("SUBSTAFFNAMES"), WW_DUMMY)
                WW_T0005row("CREWKBNNAMES") = ""
                CodeToName("CREWKBN", WW_T0005row("CREWKBN"), WW_T0005row("CREWKBNNAMES"), WW_DUMMY)
                WW_T0005row("JISSKIKBNNAMES") = ""
                CodeToName("JISSKIKBN", WW_T0005row("JISSKIKBN"), WW_T0005row("JISSKIKBNNAMES"), WW_DUMMY)

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
            CS0026TBLSORT.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN, SEQ"
            CS0026TBLSORT.FILTER = ""
            IO_TBL = CS0026TBLSORT.sort()

            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
            WW_TBLview.Dispose()
            WW_TBLview = Nothing
            WW_T0005DELtbl.Dispose()
            WW_T0005DELtbl = Nothing
            WW_T0005SELtbl.Dispose()
            WW_T0005SELtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0005_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub
    ''' <summary>
    ''' 名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得

        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE = "" Then
        Else
            Select Case I_FIELD

                Case "WORKKBN"
                    '作業区分名称 "WORKKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ　DELFLG
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                Case "TORICODE"
                    '取引先名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateCustomerParam(work.WF_SEL_CAMPCODE.Text))

                Case "TODOKECODE"
                    '届先名（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "1"))

                Case "SHUKABASHO"
                    '出荷場所名称（マスタ）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, "", "2"))

                Case "PRODUCT21", "PRODUCT22", "PRODUCT23", "PRODUCT24", "PRODUCT25", "PRODUCT26", "PRODUCT27", "PRODUCT28"
                    '品名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, False))
                Case "PRODUCTCODE1", "PRODUCTCODE2", "PRODUCTCODE3", "PRODUCTCODE4", "PRODUCTCODE5", "PRODUCTCODE6", "PRODUCTCODE7", "PRODUCTCODE8"
                    '品名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, True))

                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.CreateSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text, WF_YMD.Text, WF_YMD.Text))

                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)

                Case "SHIPORG"
                    '出荷部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateShipORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE))

                Case "TERMKBN"
                    '端末区分名 TERMKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TERMKBN"))

                Case "JISSKIKBN"
                    '実績登録区分名 JISSKIKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "JISSKIKBN"))

                Case "CREWKBN"
                    '乗務区分名 CREWKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))

                Case "GSHABAN"
                    '業務車番
                    Dim list As ListBox = work.CreateWorkLorryList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text)
                    For i As Integer = 0 To list.Items.Count - 1
                        Dim WW_SPRIT() As String = list.Items(i).Value.Split(",")
                        If WW_SPRIT(0) = I_VALUE Then
                            Dim WW_SPRIT2() As String = list.Items(i).Text.Split("　")
                            O_TEXT = WW_SPRIT2(1)
                            O_RTN = C_MESSAGE_NO.NORMAL
                            Exit For
                        End If
                    Next

                Case "TSHABANF"
                    '統一車番名（前）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.CreateCarCodeParam(work.WF_SEL_CAMPCODE.Text, True))

                Case "TSHABANB"
                    '統一車番名（後）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.CreateCarCodeParam(work.WF_SEL_CAMPCODE.Text, False))

                Case "TUMIOKIKBN"
                    '積置区分名称 TUMIOKIKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN"))

                Case "OILTYPE"
                    '油種名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                Case "PRODUCT1"
                    '品名１名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoods1Param(work.WF_SEL_CAMPCODE.Text, WF_OILTYPE.Text))

                Case "URIKBN"
                    '売上計上基準名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))

                Case "STANI1", "STANI2", "STANI3", "STANI4", "STANI5", "STANI6", "STANI7", "STANI8"
                    '請求単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI"))

                Case "TAXKBN"
                    '税区分名称 TAXKBN
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))

                Case "STORICODE"
                    '請求取引先名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateDemandParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text))

                Case "CONTCHASSIS"
                    'コンテナシャーシ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.CreateWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_UORG.Text))

            End Select
        End If


    End Sub

    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub MapRefelence(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ 選択画面の入力初期値設定 ■■■
        Select Case Context.Handler.ToString().ToUpper
            Case C_PREV_MAP_LIST.T00005I
                '一覧画面からの画面遷移

            Case C_PREV_MAP_LIST.T00007,
                 C_PREV_MAP_LIST.T00007JKT,
                 C_PREV_MAP_LIST.T00007KNK,
                 C_PREV_MAP_LIST.T00007NJS
                '〇ユーザ権限情報取得
                work.WF_SEL_PERMIT_ORG.Text = Master.USER_ORG
                Master.MAPvariant = "Admin"

        End Select

        '○Grid情報保存先のファイル名
        work.WF_SEL_XMLsaveF2.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & CS0050Session.USERID & "-T00005-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        '■■■ 画面モード（更新・参照）設定  ■■■
        '
        If work.WF_SEL_MAPpermitcode.Text = C_PERMISSION.UPDATE Then
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

    End Sub

    ''' <summary>
    ''' エラーレポート登録
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="IO_LINE_ERROR"></param>
    ''' <param name="T0005INProw"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub WriteErrorMessage(ByVal I_MESSAGE1 As String, ByVal I_MESSAGE2 As String, ByRef IO_LINE_ERROR As String, ByRef T0005INProw As DataRow, ByVal I_ERRCD As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日     =" & T0005INProw("YMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日報番号   =" & T0005INProw("NIPPONO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員     =" & T0005INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員名   =" & T0005INProw("STAFFNAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 作業区分   =" & T0005INProw("WORKKBNNAMES") & "  "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細行番号 =" & T0005INProw("SEQ") & "  "
        rightview.AddErrorReport(ControlChars.NewLine & WW_ERR_MES)
        WW_ERRLIST.Add(I_ERRCD)
        If IO_LINE_ERROR <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            IO_LINE_ERROR = I_ERRCD
        End If

    End Sub

End Class
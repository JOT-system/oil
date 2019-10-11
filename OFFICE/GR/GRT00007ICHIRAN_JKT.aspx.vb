Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 営業勤怠一覧
''' </summary>
''' <remarks></remarks>
Public Class GRT00007ICHIRAN_JKT
    Inherits Page

    '共通宣言
    Private CS0009MESSAGEout As New CS0009MESSAGEout                'メッセージ取得
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0021PROFXLS As New CS0021PROFXLS                      'UPRFXLS取得
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'UPLOAD_XLSデータ取得
    Private CS0026TblSort As New CS0026TBLSORT                      '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    Private CS0036FCHECK As New CS0036FCHECK                        '項目チェック
    Private CS0044L1INSERT As New CS0044L1INSERT                    '統計DB出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    Private T0005COM As New GRT0005COM                              '勤怠共通
    Private T0007COM As New GRT0007COM                              '勤怠共通
    Private T0007UPDATE As New GRT0007UPDATE                        '勤怠更新

    'CSV検索結果格納ds
    Private T0007tbl As DataTable                                   '勤怠テーブル（GridView用）
    Private T0007INPtbl As DataTable                                '勤怠テーブル（取込用）
    Private T0007WKtbl As DataTable                                 '勤怠テーブル（ワーク）
    Private T0007PARMtbl As DataTable                               '条件選択画面パラメータ（保存用）
    Private T0007row As DataRow                                     '勤怠行
    Private T0007INProw As DataRow                                  '勤怠行（取込用）
    Private S0013tbl As DataTable                                   'データフィールド
    Private L0001tbl As DataTable                                   '統計ＤＢ
    Private L0001row As DataRow                                     '統計行
    Private T0005tbl As DataTable                                   '日報テーブル
    Private T0005row As DataRow                                     '日報行

    Private WW_GridCnt As Integer = 33

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Dim WW_ERRLIST As List(Of String)                               'インポート中の１セット分のエラー

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■ 作業用データベース設定 ■■■
        T0007tbl = New DataTable
        T0007INPtbl = New DataTable
        T0007PARMtbl = New DataTable
        T0007WKtbl = New DataTable
        S0013tbl = New DataTable
        L0001tbl = New DataTable
        T0005tbl = New DataTable

        Try

            If IsPostBack Then

                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    '○ 画面表示データ復元
                    T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
                    If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonNIPPO"           '日報一括取込ボタン押下
                            WF_ButtonNIPPO_Click()
                        Case "WF_ButtonCALC"            '一括残業計算ボタン押下
                            WF_ButtonCALC_Click()
                        Case "WF_ButtonSAVE"            '一時保存ボタン押下
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ﾀﾞｳﾝﾛｰﾄﾞボタン押下
                            WF_ButtonPrint_Click("XLSX")
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click("pdf")
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_GRID_Scrole()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_GRID_Scrole()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
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
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                    End Select

                    'スクロール処理
                    Scrole_SUB()

                End If
            Else
                '○ 初期化処理
                Initialize()
                Scrole_SUB()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(T0007tbl) Then
                T0007tbl.Clear()
                T0007tbl.Dispose()
                T0007tbl = Nothing
            End If

            If Not IsNothing(T0007INPtbl) Then
                T0007INPtbl.Clear()
                T0007INPtbl.Dispose()
                T0007INPtbl = Nothing
            End If

            If Not IsNothing(T0007PARMtbl) Then
                T0007PARMtbl.Clear()
                T0007PARMtbl.Dispose()
                T0007PARMtbl = Nothing
            End If

            If Not IsNothing(T0007WKtbl) Then
                T0007WKtbl.Clear()
                T0007WKtbl.Dispose()
                T0007WKtbl = Nothing
            End If

            If Not IsNothing(S0013tbl) Then
                S0013tbl.Clear()
                S0013tbl.Dispose()
                S0013tbl = Nothing
            End If

            If Not IsNothing(L0001tbl) Then
                L0001tbl.Clear()
                L0001tbl.Dispose()
                L0001tbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00007WRKINC.MAPIDIJKT

        WF_WORKDATE.Focus()
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
        rightview.COMPCODE = work.WF_T7SEL_CAMPCODE.Text
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

        'Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        Dim WW_ERR_CODE As String = ""
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        '○ 検索画面からの遷移
        MAPrefelence(WW_MSG, WW_ERRCODE)
        WW_ERR_CODE = WW_ERRCODE

        '更新ボタン非活性（エラー）の場合、メッセージ出力（但し、すでにあるエラーメッセージを優先する）
        If WW_ERR_CODE <> C_MESSAGE_NO.NORMAL And rightview.getErrorReport() = "" Then
            Master.output(WW_ERR_CODE, C_MESSAGE_TYPE.ERR)
        End If
        If WW_MSG <> "" Then
            WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_MSG
            rightview.addErrorReport(WW_ERR_REPORT)
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

        '検索画面から遷移した場合
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007S Then
            If work.WF_T7SEL_BUTTON.Text = "RESTART" Then

                Dim WW_FILENAME As String = work.WF_T7I_XMLsaveF.Text
                work.WF_T7I_XMLsaveF.Text = work.WF_T7SEL_XMLsaveTmp.Text
                WF_GRID_Scrole()

                work.WF_T7I_XMLsaveF.Text = WW_FILENAME
                If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
                    Exit Sub
                End If
            Else
                '○ 先頭行に合わせる
                WF_GridPosition.Text = "1"

                GRID_INITset()
            End If
        Else
            '勤怠個別画面から遷移した場合
            WF_GridPosition.Text = work.WF_T7I_GridPosition.Text
            WF_GRID_Scrole()
        End If

        '○ 画面表示データ保存
        'If Not Master.SaveTable(T0007tbl) Then
        '    Exit Sub
        'End If

        Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
        '月末日からその月の日数の取得し、画面表示件数（月末調整、合計の２行分プラス）とする
        WW_GridCnt = Val(dt.AddMonths(1).AddDays(-1).ToString("dd")) + 2
        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T0007tbl)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= 1 and LINECNT <= " & WW_GridCnt

        CS0013ProfView.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = False
        'CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

        WF_ButtonExtract_Click()

    End Sub

    ''' <summary>
    ''' 一時保存ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonSAVE_Click()

        '画面表示を取得
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '一時保存ファイルに出力
        If Not Master.SaveTable(T0007tbl, work.WF_T7SEL_XMLsaveTmp.Text) Then
            Exit Sub
        End If

        '一時保存ファイルに出力
        T0007PARMtbl_ColumnsAdd(T0007PARMtbl)

        Dim WW_T0007PARMrow As DataRow = T0007PARMtbl.NewRow
        WW_T0007PARMrow("LINECNT") = 1
        WW_T0007PARMrow("OPERATION") = ""
        WW_T0007PARMrow("TIMSTP") = ""
        WW_T0007PARMrow("SELECT") = 1
        WW_T0007PARMrow("HIDDEN") = 0
        '会社コード　
        WW_T0007PARMrow("CAMPCODE") = work.WF_T7SEL_CAMPCODE.Text
        '対象年月　
        WW_T0007PARMrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text
        '配属部署
        WW_T0007PARMrow("HORG") = work.WF_T7SEL_HORG.Text
        '職務区分
        WW_T0007PARMrow("STAFFKBN") = work.WF_T7SEL_STAFFKBN.Text
        '従業員コード
        WW_T0007PARMrow("STAFFCODE") = work.WF_T7SEL_STAFFCODE.Text
        '従業員名称
        WW_T0007PARMrow("STAFFNAME") = work.WF_T7SEL_STAFFNAME.Text
        T0007PARMtbl.Rows.Add(WW_T0007PARMrow)

        If Not Master.SaveTable(T0007PARMtbl, work.WF_T7SEL_XMLsavePARM.Text) Then
            Exit Sub
        End If

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_WORKDATE"
        WF_WORKDATE.Focus()

    End Sub

    ''' <summary>
    ''' 絞り込みボタン処理
    ''' </summary>    
    Protected Sub WF_ButtonExtract_Click()

        '次画面への絞り込み条件伝達
        If IsDate(WF_WORKDATE.Text) Then
            work.WF_T7I_Head_WORKDATE.Text = CDate(WF_WORKDATE.Text).ToString("yyyy/MM/dd")
        Else
            work.WF_T7I_Head_WORKDATE.Text = ""
        End If
        work.WF_T7I_Head_STAFFCODE.Text = WF_STAFFCODE.Text

        '乗務員
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)

        '○テーブルデータ 復元（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
            If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
                Exit Sub
            End If
        End If

        Dim WW_T0007HEADtbl As DataTable = T0007tbl.Clone
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = "HDKBN = 'H'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, HDKBN DESC"
        WW_T0007HEADtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = "HDKBN = 'D'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, HDKBN DESC"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_WORKDATE As String = ""
        '○絞り込み操作（GridView明細Hidden設定）
        For i As Integer = 0 To WW_T0007HEADtbl.Rows.Count - 1
            Dim WW_T0007row As DataRow = WW_T0007HEADtbl.Rows(i)
            If WW_T0007row("SELECT") = "1" Then
                WW_T0007row("HIDDEN") = 1

                '従業員・日付の絞込判定　（絞込指定があれば、月調整、合計を非表示）
                If (WF_STAFFCODE.Text = "") And (WF_WORKDATE.Text = "") Then
                    WW_T0007row("HIDDEN") = 0
                End If

                If (WF_STAFFCODE.Text <> "") And (WF_WORKDATE.Text = "") Then
                    If WW_T0007row("STAFFCODE") Like WF_STAFFCODE.Text & "*" Then
                        WW_T0007row("HIDDEN") = 0
                    End If
                End If

                If (WF_STAFFCODE.Text = "") And (WF_WORKDATE.Text <> "") Then
                    If IsDate(WF_WORKDATE.Text) Then
                        WW_WORKDATE = CDate(WF_WORKDATE.Text).ToString("yyyy/MM/dd")
                    Else
                        WW_WORKDATE = ""
                    End If
                    If WW_T0007row("WORKDATE") = WW_WORKDATE Then
                        If WW_T0007row("RECODEKBN") = "0" Then
                            WW_T0007row("HIDDEN") = 0
                        Else
                            WW_T0007row("HIDDEN") = 1
                        End If
                    End If
                End If

                If (WF_STAFFCODE.Text <> "") And (WF_WORKDATE.Text <> "") Then
                    If IsDate(WF_WORKDATE.Text) Then
                        WW_WORKDATE = CDate(WF_WORKDATE.Text).ToString("yyyy/MM/dd")
                    Else
                        WW_WORKDATE = ""
                    End If

                    If WW_T0007row("STAFFCODE") Like WF_STAFFCODE.Text & "*" And
                       WW_T0007row("WORKDATE") = WW_WORKDATE Then
                        If WW_T0007row("RECODEKBN") = "0" Then
                            WW_T0007row("HIDDEN") = 0
                        Else
                            WW_T0007row("HIDDEN") = 1
                        End If
                    End If
                End If
            End If
        Next
        T0007tbl.Merge(WW_T0007HEADtbl)

        If (WF_WORKDATE.Text <> "") Then
            pnlListArea.Attributes.Remove("data-sum")
        Else
            pnlListArea.Attributes.Add("data-sum", "1")
        End If

        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            WF_GridPosition.Text = "1"
        End If

        '○GridViewデータをテーブルに保存（絞込みボタン押下の時のみ）
        If WF_ButtonClick.Value = "WF_ButtonExtract" Then
            If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
                Exit Sub
            End If
        End If

        WW_T0007HEADtbl.Clear()
        WW_T0007HEADtbl = Nothing

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_WORKDATE"
        WF_WORKDATE.Focus()

    End Sub

    ''' <summary>
    ''' 一残業計算ボタン処理
    ''' </summary>    
    Protected Sub WF_ButtonCALC_Click()

        '入力（開始日、終了日）チェック
        DaysCheck(WW_RTN_SW)
        If WW_RTN_SW = "ERR" Then
            Exit Sub
        End If

        '画面表示を取得
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '画面指定の日付（From、To）
        Dim WW_STDATE As String = work.WF_T7SEL_TAISHOYM.Text & "/" & CInt(WF_NIPPO_FROM.Text).ToString("00")
        Dim WW_ENDDATE As String = work.WF_T7SEL_TAISHOYM.Text & "/" & CInt(WF_NIPPO_TO.Text).ToString("00")

        Dim WW_T0007SELtbl As DataTable = New DataTable
        Dim WW_T0007tbl As DataTable = T0007tbl.Clone
        Dim WW_Filter As String = ""

        '----------------------------------------------
        '残業計算
        '----------------------------------------------
        WW_Filter = "SELECT = '1' and RECODEKBN = '0' and WORKDATE >= #" & WW_STDATE & "# and WORKDATE <= #" & WW_ENDDATE & "#"
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        For Each WW_T7row As DataRow In WW_T0007SELtbl.Rows
            If WW_T7row("HDKBN") = "H" And WW_T7row("BINDTIME") = "12:00" Then
                Dim WW_WORKINGH As String = ""
                WORKINGHget(WW_T7row, WW_WORKINGH, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Exit Sub
                End If
                WW_T7row("BINDTIME") = WW_WORKINGH
                WW_T7row("BINDTIMEMIN") = T0007COM.HHMMtoMinutes(WW_WORKINGH)
            End If

        Next
        T0007COM.T0007_KintaiCalc_JKT(WW_T0007SELtbl, T0007tbl)

        For Each WW_T7row As DataRow In WW_T0007SELtbl.Rows
            WW_T7row("TIMSTP") = "0"
            If WW_T7row("STATUS") Like "*Ｂ勤再計算*" Then
                WW_T7row("STATUS") = Replace(WW_T7row("STATUS"), ",Ｂ勤再計算", "")
                WW_T7row("STATUS") = Replace(WW_T7row("STATUS"), "Ｂ勤再計算", "")
            End If
            If WW_T7row("HDKBN") = "D" Then
                WW_T7row("OPERATION") = "更新"
            End If
        Next

        CS0026TblSort.TABLE = WW_T0007SELtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_IDX As Integer = 0
        Dim WW_KEYINP As String = ""
        Dim WW_KEYTBL As String = ""
        For Each T0007INProw As DataRow In WW_T0007SELtbl.Rows
            WW_KEYINP = T0007INProw("STAFFCODE") & T0007INProw("WORKDATE") & T0007INProw("RECODEKBN")
            If T0007INProw("OPERATION") = "更新" And T0007INProw("HDKBN") = "H" Then

                For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
                    Dim T0007row As DataRow = T0007tbl.Rows(i)
                    WW_KEYTBL = T0007row("STAFFCODE") & T0007row("WORKDATE") & T0007row("RECODEKBN")
                    If WW_KEYTBL < WW_KEYINP Then
                        Continue For
                    End If

                    If WW_KEYTBL = WW_KEYINP Then
                        T0007row("OPERATION") = T0007INProw("OPERATION")
                        T0007row("SELECT") = "0"
                        T0007row("HIDDEN") = "1" '非表示
                        T0007row("DELFLG") = "1"
                    End If

                    If WW_KEYTBL > WW_KEYINP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '当画面で生成したデータ（タイムスタンプ＝0）に対する変更は、変更前を物理削除する　
        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T0007row As DataRow = T0007tbl.Rows(i)
            If T0007row("TIMSTP") = "0" And
               T0007row("SELECT") = "0" Then
                T0007row.Delete()
            End If
        Next

        '更新対象の勤怠ヘッダのコピー
        T0007tbl.Merge(WW_T0007SELtbl)

        '----------------------------------------------
        '合計レコード編集
        '----------------------------------------------
        T0007COM.T0007_TotalRecodeCreate(T0007tbl)

        '----------------------------------------------
        '月調整レコード作成
        '----------------------------------------------
        T0007COM.T0007_ChoseiRecodeCreate(T0007tbl)

        'ソート
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_LINECNT As Integer = 0
        For Each WW_TBLrow As DataRow In T0007tbl.Rows
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("DELFLG") = "0" Then
                    WW_TBLrow("SELECT") = "1"
                    WW_TBLrow("HIDDEN") = "0"      '表示
                    WW_LINECNT += 1
                    WW_TBLrow("LINECNT") = WW_LINECNT
                End If
            End If
        Next

        WW_T0007SELtbl.Dispose()
        WW_T0007SELtbl = Nothing

        If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

        '重複チェック
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        T0007COM.T0007_DuplCheck(T0007tbl, WW_MSG, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_MSG

            rightview.addErrorReport(WW_ERR_REPORT)

            CS0011LOGWRITE.INFSUBCLASS = "T0007_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0007_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = WW_ERR_REPORT
            CS0011LOGWRITE.MESSAGENO = WW_ERRCODE
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        End If
    End Sub

    ''' <summary>
    ''' 日報一括取込ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonNIPPO_Click()

        '入力（開始日、終了日）チェック
        DaysCheck(WW_RTN_SW)
        If WW_RTN_SW = "ERR" Then
            Exit Sub
        End If

        '画面表示を取得
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '画面指定の日付（From、To）
        Dim WW_STDATE As String = work.WF_T7SEL_TAISHOYM.Text & "/" & CInt(WF_NIPPO_FROM.Text).ToString("00")
        Dim WW_ENDDATE As String = work.WF_T7SEL_TAISHOYM.Text & "/" & CInt(WF_NIPPO_TO.Text).ToString("00")

        '画面指定のFrom、Toの範囲で全員分（従業員コードを空白）の日報取得（パラメタ「NEW」は、最新の日報を取得する）
        Dim T0005tbl As DataTable = New DataTable
        Dim WW_STAFFCODE As String = ""
        T00005ALLget(WW_STAFFCODE, WW_STDATE, WW_ENDDATE, T0005tbl, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        Dim WW_T0007SELtbl As DataTable = New DataTable
        Dim WW_T0007tbl As DataTable = T0007tbl.Clone
        Dim WW_Filter As String = ""

        WW_Filter = "SELECT = '1' and HDKBN = 'H' and STATUS like '*日報*' and RECODEKBN = '0' and WORKDATE >= #" & WW_STDATE & "# and WORKDATE <= #" & WW_ENDDATE & "#"
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        '明細にステータスを設定
        CS0026TblSort.TABLE = WW_T0007SELtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_IDX As Integer = 0
        Dim WW_KEYINP As String = ""
        Dim WW_KEYTBL As String = ""
        For Each T0007INProw As DataRow In WW_T0007SELtbl.Rows
            WW_KEYINP = T0007INProw("STAFFCODE") & T0007INProw("WORKDATE") & "0"
            If T0007INProw("STATUS") Like "*日報*" Then

                For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
                    Dim T0007row As DataRow = T0007tbl.Rows(i)
                    WW_KEYTBL = T0007row("STAFFCODE") & T0007row("WORKDATE") & T0007row("RECODEKBN")
                    If WW_KEYTBL < WW_KEYINP Then
                        Continue For
                    End If

                    If WW_KEYTBL = WW_KEYINP Then
                        T0007row("STATUS") = T0007INProw("STATUS")
                    End If

                    If WW_KEYTBL > WW_KEYINP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '明細を削除し、新たに日報から明細を作成
        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T7row As DataRow = T0007tbl.Rows(i)
            If T7row("WORKDATE") >= WW_STDATE And T7row("WORKDATE") <= WW_ENDDATE Then
                If T7row("STATUS") Like "*日報*" Then
                    If T7row("HDKBN") = "D" And T7row("RECODEKBN") = "0" Then
                        T7row.Delete()
                    End If
                End If
            End If
        Next

        '日報を勤怠フォーマットに変換し、マージする
        Using iT0005view As DataView = New DataView(T0005tbl)
            iT0005view.Sort = "YMD, STAFFCODE, WORKKBN"
            NIPPOget_T7Format("NEW", WW_T0007tbl, iT0005view)
        End Using
        CS0026TblSort.TABLE = WW_T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN, NIPPOLINKCODE"
        WW_T0007tbl = CS0026TblSort.sort()

        Dim WW_T7KEY As String = ""
        Dim WW_T5KEY As String = ""
        Dim WW_LOOP As Integer = 0
        Dim WW_T0007tbl2 As DataTable = T0007tbl.Clone
        For Each WW_T7row As DataRow In WW_T0007SELtbl.Rows
            WW_T7KEY = WW_T7row("STAFFCODE") & WW_T7row("WORKDATE")
            For i As Integer = WW_LOOP To WW_T0007tbl.Rows.Count - 1
                Dim WW_T5row As DataRow = WW_T0007tbl.Rows(i)

                WW_T5KEY = WW_T5row("STAFFCODE") & WW_T5row("WORKDATE")
                If WW_T5KEY < WW_T7KEY Then
                    Continue For
                End If
                If WW_T5KEY = WW_T7KEY Then
                    Dim ADDrow As DataRow = WW_T0007tbl2.NewRow
                    ADDrow.ItemArray = WW_T5row.ItemArray
                    WW_T0007tbl2.Rows.Add(ADDrow)
                End If
                If WW_T5KEY > WW_T7KEY Then
                    WW_LOOP = i
                    Exit For
                End If
            Next
        Next

        T0007tbl.Merge(WW_T0007tbl2)

        '------------------------------------------------------------------
        '日報を取得し、作業（始業、終業、休憩、その他）レコード作成
        '------------------------------------------------------------------
        CreWORKKBN(T0007tbl, T0005tbl, WW_STDATE, WW_ENDDATE)

        '--------------------------------------------
        'ヘッダ編集（日報有の分）
        '--------------------------------------------
        HeadEdit(T0007tbl, T0005tbl, WW_STDATE, WW_ENDDATE)

        '--------------------------------------------
        '拘束開始編集（日報有の分）
        '--------------------------------------------
        BindStDateSet(T0007tbl, WW_STDATE, WW_ENDDATE)

        '--------------------------------------------
        '合計編集（日報有の分）
        '--------------------------------------------
        T0007_TTLEdit(T0007tbl)

        '--------------------------------------------
        '所定労働日数初期設定（全員分）
        '--------------------------------------------
        T0007_WORKNISSUEdit(T0007tbl)

        '----------------------------------------------
        '残業計算
        '----------------------------------------------
        WW_Filter = "SELECT = '1' and STATUS like '*日報*' and RECODEKBN = '0' and WORKDATE >= #" & WW_STDATE & "# and WORKDATE <= #" & WW_ENDDATE & "#"
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        T0007COM.T0007_KintaiCalc_JKT(WW_T0007SELtbl, T0007tbl)

        For Each WW_T7row As DataRow In WW_T0007SELtbl.Rows
            WW_T7row("TIMSTP") = "0"
            WW_T7row("STATUS") = ""
            If WW_T7row("HDKBN") = "D" Then
                WW_T7row("OPERATION") = "更新"
            End If
        Next

        CS0026TblSort.TABLE = WW_T0007SELtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        WW_T0007SELtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007tbl = CS0026TblSort.sort()

        WW_IDX = 0
        WW_KEYINP = ""
        WW_KEYTBL = ""
        For Each T0007INProw As DataRow In WW_T0007SELtbl.Rows
            WW_KEYINP = T0007INProw("STAFFCODE") & T0007INProw("WORKDATE") & T0007INProw("RECODEKBN")
            If T0007INProw("OPERATION") = "更新" And T0007INProw("HDKBN") = "H" Then

                For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
                    Dim T0007row As DataRow = T0007tbl.Rows(i)
                    WW_KEYTBL = T0007row("STAFFCODE") & T0007row("WORKDATE") & T0007row("RECODEKBN")
                    If WW_KEYTBL < WW_KEYINP Then
                        Continue For
                    End If

                    If WW_KEYTBL = WW_KEYINP Then
                        T0007row("OPERATION") = T0007INProw("OPERATION")
                        T0007row("SELECT") = "0"
                        T0007row("HIDDEN") = "1" '非表示
                        T0007row("DELFLG") = "1"
                    End If

                    If WW_KEYTBL > WW_KEYINP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '当画面で生成したデータ（タイムスタンプ＝0）に対する変更は、変更前を物理削除する　
        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T0007row As DataRow = T0007tbl.Rows(i)
            If T0007row("TIMSTP") = "0" And
               T0007row("SELECT") = "0" Then
                T0007row.Delete()
            End If
        Next

        '更新対象の勤怠ヘッダのコピー
        T0007tbl.Merge(WW_T0007SELtbl)

        '----------------------------------------------
        '合計レコード編集
        '----------------------------------------------
        T0007COM.T0007_TotalRecodeCreate(T0007tbl)

        '----------------------------------------------
        '月調整レコード作成
        '----------------------------------------------
        T0007COM.T0007_ChoseiRecodeCreate(T0007tbl)

        'ソート
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_LINECNT As Integer = 0
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_TBLrow As DataRow = T0007tbl.Rows(i)
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("DELFLG") = "0" Then
                    WW_TBLrow("SELECT") = "1"
                    WW_TBLrow("HIDDEN") = "0"      '表示
                    WW_LINECNT += 1
                    WW_TBLrow("LINECNT") = WW_LINECNT
                End If

            End If

            If WW_TBLrow("HDKBN") = "H" Then
                If T0007COM.CheckHOLIDAY(WW_TBLrow("HOLIDAYKBN"), WW_TBLrow("PAYKBN")) Then

                    For j As Integer = i - 1 To 0 Step -1
                        Dim WW_row As DataRow = T0007tbl.Rows(j)
                        If WW_row("HDKBN") = "D" Then
                            Continue For
                        End If

                        Dim WW_DATE As Date = CDate(WW_TBLrow("WORKDATE")).AddDays(-1)
                        If WW_row("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd") And
                            WW_row("STAFFCODE") = WW_TBLrow("STAFFCODE") Then
                            If WW_row("ENDDATE") >= WW_TBLrow("WORKDATE") Then
                                If InStr(WW_row("STATUS"), "Ｂ勤再計算") > 0 Then
                                Else
                                    If WW_row("STATUS") = "" Then
                                        WW_row("STATUS") = WW_row("STATUS") & "Ｂ勤再計算"
                                    Else
                                        WW_row("STATUS") = WW_row("STATUS") & ",Ｂ勤再計算"
                                    End If
                                    Exit For
                                End If
                            Else
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
        Next

        WW_T0007SELtbl.Dispose()
        WW_T0007SELtbl = Nothing
        WW_T0007tbl.Dispose()
        WW_T0007tbl = Nothing
        WW_T0007tbl2.Dispose()
        WW_T0007tbl2 = Nothing
        T0005tbl.Dispose()
        T0005tbl = Nothing

        If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

        '重複チェック
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        T0007COM.T0007_DuplCheck(T0007tbl, WW_MSG, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_MSG

            rightview.AddErrorReport(WW_ERR_REPORT)

            CS0011LOGWRITE.INFSUBCLASS = "T0007_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0007_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                '
            CS0011LOGWRITE.TEXT = WW_ERR_REPORT
            CS0011LOGWRITE.MESSAGENO = WW_ERRCODE
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        End If

    End Sub

    ''' <summary>
    ''' 入力（開始日、終了日）チェック
    ''' </summary>
    ''' <param name="oRtn"></param>
    Protected Sub DaysCheck(ByRef oRtn As String)
        oRtn = ""

        Dim WW_ERR_REPORT As String = ""
        Dim WW_MESSAGE As Label = Nothing
        Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")

        If IsNumeric(WF_NIPPO_FROM.Text) Then
            If Val(WF_NIPPO_FROM.Text) >= 1 And Val(WF_NIPPO_FROM.Text) <= dt.AddMonths(1).AddDays(-1).ToString("dd") Then
            Else
                '数値範囲エラー
                Master.output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)

                CS0009MESSAGEout.MESSAGENO = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.ERR
                CS0009MESSAGEout.MESSAGEBOX = WW_MESSAGE
                CS0009MESSAGEout.CS0009MESSAGEout()

                If isNormal(CS0009MESSAGEout.ERR) Then
                    WW_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
                End If

                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・対象期間（開始日）エラーです。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_MESSAGE.Text & " ,"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & "入力範囲は、１日～月末です" & " ,"
                WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES

                rightview.addErrorReport(WW_ERR_REPORT)

                WF_NIPPO_FROM.Focus()
                oRtn = "ERR"
                Exit Sub
            End If
        Else
            '数値エラー
            Master.output(C_MESSAGE_NO.NUMERIC_VALUE_ERROR, C_MESSAGE_TYPE.ERR)

            CS0009MESSAGEout.MESSAGENO = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
            CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.ERR
            CS0009MESSAGEout.MESSAGEBOX = WW_MESSAGE
            CS0009MESSAGEout.CS0009MESSAGEout()

            If isNormal(CS0009MESSAGEout.ERR) Then
                WW_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
            End If

            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・対象期間（開始日）エラーです。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_MESSAGE.Text & " ,"
            WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES

            rightview.addErrorReport(WW_ERR_REPORT)

            WF_NIPPO_FROM.Focus()
            oRtn = "ERR"
            Exit Sub
        End If

        If IsNumeric(WF_NIPPO_TO.Text) Then
            If Val(WF_NIPPO_TO.Text) >= 1 And Val(WF_NIPPO_TO.Text) <= dt.AddMonths(1).AddDays(-1).ToString("dd") Then
            Else
                '数値範囲エラー
                Master.output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)

                CS0009MESSAGEout.MESSAGENO = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.ERR
                CS0009MESSAGEout.MESSAGEBOX = WW_MESSAGE
                CS0009MESSAGEout.CS0009MESSAGEout()

                If isNormal(CS0009MESSAGEout.ERR) Then
                    WW_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
                End If

                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・対象期間（終了日）エラーです。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_MESSAGE.Text & " ,"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & "範囲は、１日～月末です" & " ,"
                WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES

                rightview.addErrorReport(WW_ERR_REPORT)

                WF_NIPPO_TO.Focus()
                oRtn = "ERR"
                Exit Sub
            End If
        Else
            '数値エラー

            Master.output(C_MESSAGE_NO.NUMERIC_VALUE_ERROR, C_MESSAGE_TYPE.ERR)

            CS0009MESSAGEout.MESSAGENO = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
            CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.ERR
            CS0009MESSAGEout.MESSAGEBOX = WW_MESSAGE
            CS0009MESSAGEout.CS0009MESSAGEout()

            If isNormal(CS0009MESSAGEout.ERR) Then
                WW_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
            End If

            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・対象期間（終了日）エラーです。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_MESSAGE.Text & " ,"
            WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES

            rightview.addErrorReport(WW_ERR_REPORT)

            WF_NIPPO_TO.Focus()
            oRtn = "ERR"
            Exit Sub
        End If

        If Val(WF_NIPPO_FROM.Text) > Val(WF_NIPPO_TO.Text) Then
            '日付指定エラー

            Master.output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)

            CS0009MESSAGEout.MESSAGENO = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
            CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.ERR
            CS0009MESSAGEout.MESSAGEBOX = WW_MESSAGE
            CS0009MESSAGEout.CS0009MESSAGEout()

            If isNormal(CS0009MESSAGEout.ERR) Then
                WW_MESSAGE.Text = CS0009MESSAGEout.MESSAGEBOX.text
            End If

            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・対象期間（開始日＞終了日）エラーです。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & WW_MESSAGE.Text & " ,"
            WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES

            rightview.addErrorReport(WW_ERR_REPORT)

            WF_NIPPO_FROM.Focus()
            oRtn = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' 更新ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonUPDATE_Click()

        'DataBase接続文字
        Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
        'トランザクション
        Dim SQLtrn As SqlClient.SqlTransaction = Nothing

        Dim WW_ERR As String = "OFF"

        If Master.MAPpermitcode <> C_PERMISSION.UPDATE Then

            Master.output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ERR, "更新権限がありません。")
            Exit Sub
        End If

        'WF_MESSAGE.Text = ""

        'テーブルデータ 復元(TEXTファイルより復元)
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        'ソート
        '勤怠以外（日報）を退避
        Dim T0007NIPPOtbl As DataTable = T0007tbl.Clone
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = "DATAKBN = 'N'"
        CS0026TblSort.SORTING = "DATAKBN, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007NIPPOtbl = CS0026TblSort.sort()

        '勤怠のみ抽出
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = "DATAKBN = 'K'"
        CS0026TblSort.SORTING = "DATAKBN, STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        '初期データ作成（初期データが存在しない場合のみ作成する）
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_TBLrow As DataRow = T0007tbl.Rows(i)
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("DBUMUFLG") = "0" Then
                    WW_TBLrow("OPERATION") = "更新"
                End If
            End If
        Next

        '更新を明細行に反映する。また、明細が更新の場合、月合計も更新する
        Dim WW_KEYHEAD As String = ""
        Dim WW_KEYDTL As String = ""
        Dim WW_LOOP As Integer = 0
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_TBLrow As DataRow = T0007tbl.Rows(i)
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("OPERATION") = "更新" Then
                    WW_KEYHEAD = WW_TBLrow("STAFFCODE") & WW_TBLrow("WORKDATE") & WW_TBLrow("RECODEKBN")

                    For j As Integer = WW_LOOP To T0007tbl.Rows.Count - 1
                        Dim WW_DTLrow As DataRow = T0007tbl.Rows(j)
                        If WW_DTLrow("HDKBN") = "D" Then
                            WW_KEYDTL = WW_DTLrow("STAFFCODE") & WW_DTLrow("WORKDATE") & WW_DTLrow("RECODEKBN")
                            If WW_KEYDTL < WW_KEYHEAD Then
                                Continue For
                            End If

                            '日別レコードの明細に更新を設定
                            If WW_KEYDTL = WW_KEYHEAD Then
                                WW_DTLrow("OPERATION") = "更新"
                            End If

                            If WW_KEYDTL > WW_KEYHEAD Then
                                WW_LOOP = j
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
        Next

        WW_KEYHEAD = ""
        WW_KEYDTL = ""
        WW_LOOP = 0
        Dim OLD_STAFF As String = ""
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_TBLrow As DataRow = T0007tbl.Rows(i)
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" And WW_TBLrow("OPERATION") = "更新" And WW_TBLrow("RECODEKBN") = "0" Then
                    If OLD_STAFF = WW_TBLrow("STAFFCODE") Then
                        OLD_STAFF = WW_TBLrow("STAFFCODE")
                        Continue For
                    End If
                    WW_KEYHEAD = WW_TBLrow("STAFFCODE") & "2"

                    For j As Integer = WW_LOOP To T0007tbl.Rows.Count - 1
                        Dim WW_DTLrow As DataRow = T0007tbl.Rows(j)
                        WW_KEYDTL = WW_DTLrow("STAFFCODE") & WW_DTLrow("RECODEKBN")
                        If WW_KEYDTL < WW_KEYHEAD Then
                            Continue For
                        End If

                        If WW_KEYDTL = WW_KEYHEAD Then
                            WW_DTLrow("OPERATION") = "更新"
                        End If

                        If WW_KEYDTL > WW_KEYHEAD Then
                            WW_LOOP = j
                            Exit For
                        End If
                    Next

                    OLD_STAFF = WW_TBLrow("STAFFCODE")
                End If
            End If
        Next

        '重複チェック
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        T0007COM.T0007_DuplCheck(T0007tbl, WW_MSG, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            WW_ERR_REPORT = "内部処理エラー" & ControlChars.NewLine & WW_MSG

            rightview.addErrorReport(WW_ERR_REPORT)

            CS0011LOGWRITE.INFSUBCLASS = "T0007_DuplCheck"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0007_DuplCheck"                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = WW_ERR_REPORT
            CS0011LOGWRITE.MESSAGENO = WW_ERRCODE
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力

            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '--------------------------------------------------------------------
        'ＤＢ更新
        '--------------------------------------------------------------------
        SQLcon.Open() 'DataBase接続(Open)
        'トランザクション開始
        'SQLtrn = SQLcon.BeginTransaction
        SQLtrn = Nothing

        '更新SQL文･･･マスタへ更新
        Dim WW_T0007UPDtbl As New DataTable
        Dim WW_DATENOW As DateTime = Date.Now

        '----------------------------------------------------------------------------------------------------
        '勤怠ＤＢ追加
        '----------------------------------------------------------------------------------------------------
        Dim WW_T0007tbl As DataTable = New DataTable
        T0007UPDATE.T0007UPDtbl_ColumnsAdd(WW_T0007tbl)

        '※ＤＢ登録済で変更発生したもの（元データがSELECT='0'（対象外）として保存されている）

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and SELECT = '1' and RECODEKBN <> '1' "
        CS0026TblSort.SORTING = "CAMPCODE, TAISHOYM, STAFFCODE, WORKDATE, HDKBN DESC"
        WW_T0007UPDtbl = CS0026TblSort.sort()

        Try
            '勤怠DB編集
            T0007_InsertEdit(WW_T0007UPDtbl, WW_DATENOW, WW_T0007tbl, WW_DUMMY)

            '勤怠DB出力

            For Each WW_T7Row As DataRow In WW_T0007tbl.Rows
                If WW_T7Row("HDKBN") = "H" Then

                    '-----------------------------
                    '勤怠ＤＢ削除処理
                    '-----------------------------
                    T0007UPDATE.T0007_Delete(SQLcon, SQLtrn, WW_T7Row, WW_DATENOW, WW_ERRCODE, Master.USERID, Master.USERTERMID)
                    If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                        'SQLtrn.Rollback()
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI")
                        Exit Sub
                    End If
                End If

                '-----------------------------
                '勤怠ＤＢ追加
                '-----------------------------
                T0007Insert(WW_T7Row, SQLcon)

            Next

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "WF_ButtonUPDATE_Click"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSERT T0007_KINTAI"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString())
            Exit Sub

        End Try

        WW_T0007UPDtbl.Dispose()
        WW_T0007UPDtbl = Nothing
        WW_T0007tbl.Dispose()
        WW_T0007tbl = Nothing

        '----------------------------------------------------------------------------------------------------
        '統計ＤＢ追加
        '----------------------------------------------------------------------------------------------------
        Dim WW_RESULT As String = ""
        Try
            '統計DB格納テーブル作成
            L0001tbl = New DataTable
            CS0044L1INSERT.CS0044L1ColmnsAdd(L0001tbl)

            '有効データのみ
            Dim WW_T0007SELtbl As DataTable = T0007tbl.Clone
            CS0026TblSort.TABLE = T0007tbl
            CS0026TblSort.FILTER = "SELECT = '1' and RECODEKBN <> '1'"
            CS0026TblSort.SORTING = "SELECT, WORKDATE, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_T0007SELtbl = CS0026TblSort.sort()

            '削除データ（削除処理）
            For Each T0007row As DataRow In WW_T0007SELtbl.Rows
                If T0007row("HDKBN") = "H" And T0007row("OPERATION") = "更新" Then
                    Try
                        '日報ＤＢ更新
                        Dim SQLStr As String =
                                    "UPDATE L0001_TOKEI " _
                                  & "SET DELFLG         = '1' " _
                                  & "  , UPDYMD         = @P05 " _
                                  & "  , UPDUSER        = @P06 " _
                                  & "  , UPDTERMID      = @P07 " _
                                  & "  , RECEIVEYMD     = @P08  " _
                                  & "WHERE CAMPCODE     = @P01 " _
                                  & "  and DENTYPE      = @P02 " _
                                  & "  and NACSHUKODATE = @P03 " _
                                  & "  and KEYSTAFFCODE = @P04 " _
                                  & "  and DELFLG      <> '1'  "

                        If T0007row("RECODEKBN") = "2" Then
                            SQLStr = SQLStr & " and ACACHANTEI in ('AMD','AMC')"
                        End If

                        Dim SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
                        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)

                        PARA01.Value = work.WF_T7SEL_CAMPCODE.Text
                        PARA02.Value = "T07"
                        PARA03.Value = T0007row("WORKDATE")
                        PARA04.Value = T0007row("STAFFCODE")
                        PARA05.Value = WW_DATENOW
                        PARA06.Value = Master.USERID
                        PARA07.Value = Master.USERTERMID
                        PARA08.Value = C_DEFAULT_YMD

                        SQLcmd.ExecuteNonQuery()

                        'CLOSE
                        SQLcmd.Dispose()
                        SQLcmd = Nothing

                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI")

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

            'その他（乗務員）
            T0007COM.L0001tblEtcEdit(Master.USERID, WW_T0007SELtbl, L0001tbl, WW_RESULT)

            '休憩（乗務員）
            T0007COM.L0001tblBreakEdit(Master.USERID, WW_T0007SELtbl, L0001tbl, WW_RESULT)

            '日別合計（休日）（乗務員）
            T0007COM.L0001tblDailyTtlEdit(Master.USERID, WW_T0007SELtbl, L0001tbl, WW_RESULT)

            '事務員・勤務
            T0007COM.L0001tblJimEdit(Master.USERID, WW_T0007SELtbl, L0001tbl, WW_RESULT)

            '月合計（ジャーナル）
            T0007COM.L0001tbMonthlylTtlEdit(Master.USERID, WW_T0007SELtbl, L0001tbl, WW_RESULT)


            '■■■ 統計ＤＢ出力 ■■■
            '
            For i As Integer = 0 To L0001tbl.Rows.Count - 1

                L0001row = L0001tbl.Rows(i)

                L0001row("INITYMD") = WW_DATENOW '登録年月日
                L0001row("UPDYMD") = WW_DATENOW  '更新年月日
                L0001row("UPDUSER") = Master.USERID  '更新ユーザＩＤ
                L0001row("UPDTERMID") = Master.USERTERMID   '更新端末
                L0001row("RECEIVEYMD") = C_DEFAULT_YMD  '集信日時

                L0001row("PAYYENDTIME") = "00:00"    '予定退社時刻
                L0001row("PAYAPPLYID") = ""    '申請ID
                L0001row("PAYRIYU") = ""       '理由
                L0001row("PAYRIYUETC") = ""    '理由その他

            Next

            CS0044L1INSERT.SQLCON = SQLcon
            CS0044L1INSERT.CS0044L1Insert(L0001tbl)

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "WF_ButtonUPDATE_Click"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSERT L0001_TOKEI"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, ex.ToString())
            Exit Sub

        End Try

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, WORKDATE, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T0007row As DataRow = T0007tbl.Rows(i)
            If T0007row("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If T0007row("SELECT") = "0" Then
                    T0007row.Delete()
                Else
                    If T0007row("SELECT") = "1" And
                        T0007row("RECODEKBN") <> "1" And
                        T0007row("OPERATION") = "更新" Then
                        T0007row("DBUMUFLG") = "1"
                    End If

                    If T0007row("OPERATION") = "更新" Then
                        T0007row("OPERATION") = ""
                        If T0007row("STATUS") <> "合計" Then
                            T0007row("STATUS") = ""
                        End If
                    End If
                End If
            End If
        Next

        T0007tbl.Merge(T0007NIPPOtbl)

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

        If WW_ERR = "ON" Then
            '○メッセージ表示
            Master.output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○Close
        SQLcon.Close()
        SQLcon.Dispose()
        SQLcon = Nothing

    End Sub

    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理                                         
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 
    Protected Sub WF_ButtonPrint_Click(ByVal iOutType As String)

        '右ボックスの選択レポートIDを取得
        If String.IsNullOrEmpty(rightview.getReportId()) Then
            '未選択の場合はそのまま終了
            Return
        End If

        'テーブルデータ 復元(TEXTファイルより復元)
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)

        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '〇Excel用追加項目クリア
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            '車両区分（SHARYOKBN 1:単車、2:トレーラ）
            '給与油種区分（OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素
            For WW_SHARYOKBN As Integer = 1 To 2
                For WW_OILPAYKBN As Integer = 1 To 10
                    Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    T0007tbl.Rows(i)(WW_UNLOADCNTTTL) = ""
                    T0007tbl.Rows(i)(WW_HAIDISTANCETTL) = ""
                Next
            Next
        Next

        '〇日別明細or月合計要求判定　…　Excel定義に月合計項目が有効ならば、月合計判定("ON")
        Dim wTTLFLG As String = ""      '月合計判定
        Excelhantei(rightview.getReportId(), wTTLFLG)

        Dim cpT0007tbl As DataTable = T0007tbl.Copy

        '絞り込み
        If WF_STAFFCODE.Text <> "" Then
            cpT0007tbl = (From list In cpT0007tbl Where list.Item("STAFFCODE") = WF_STAFFCODE.Text).CopyToDataTable
        End If

        If WF_WORKDATE.Text <> "" Then
            If IsDate(WF_WORKDATE.Text) Then
                cpT0007tbl = (From list In cpT0007tbl Where list.Item("WORKDATE") = CDate(WF_WORKDATE.Text).ToString("yyyy/MM/dd")).CopyToDataTable
            End If
        End If

        '〇データ抽出（日別・月合計共通処理）
        Dim WW_TBLview As DataView
        'WW_TBLview = New DataView(T0007tbl)
        WW_TBLview = New DataView(cpT0007tbl)
        WW_TBLview.Sort = "CAMPCODE, HORG, STAFFCODE, WORKDATE"

        If wTTLFLG = "月合計" Then
            WW_TBLview.RowFilter = "HDKBN='H' and RECODEKBN = '2' and SELECT = '1'"
        Else
            WW_TBLview.RowFilter = "HDKBN='H' and RECODEKBN = '0' and SELECT = '1'"
        End If
        Dim WW_TBL As DataTable = WW_TBLview.ToTable

        '〇月合計編集　…　単トレ・油種情報付与
        If wTTLFLG = "月合計" Then
            'WW_TBLview = New DataView(T0007tbl)
            WW_TBLview = New DataView(cpT0007tbl)
            WW_TBLview.Sort = "STAFFCODE, HDKBN, RECODEKBN, SELECT"
            WW_TBLview.RowFilter = "HDKBN='D' and RECODEKBN = '2' and SELECT = '1'"
            Dim WW_TBL_DETAIL As DataTable = WW_TBLview.ToTable

            WW_TBLview = New DataView(WW_TBL_DETAIL)
            WW_TBLview.Sort = "STAFFCODE"
            For i As Integer = 0 To WW_TBL.Rows.Count - 1
                WW_TBLview.RowFilter = "STAFFCODE='" & WW_TBL.Rows(i)("STAFFCODE") & "'"
                For j As Integer = 0 To WW_TBLview.Count - 1
                    '車両区分（SHARYOKBN 1:単車、2:トレーラ）
                    '給与油種区分（OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素
                    Dim WW_SHARYOKBN As String = Val(WW_TBLview.Item(j)("SHARYOKBN")).ToString("00")
                    Dim WW_OILPAYKBN As String = WW_TBLview.Item(j)("OILPAYKBN")
                    If (WW_SHARYOKBN = "01" OrElse WW_SHARYOKBN = "02") AndAlso
                        WW_OILPAYKBN >= "01" AndAlso WW_OILPAYKBN <= "10" Then

                        Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_SHARYOKBN & WW_OILPAYKBN
                        Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SHARYOKBN & WW_OILPAYKBN

                        WW_TBL.Rows(i)(WW_UNLOADCNTTTL) = WW_TBLview.Item(j)("UNLOADCNTTTL")
                        WW_TBL.Rows(i)(WW_HAIDISTANCETTL) = WW_TBLview.Item(j)("HAIDISTANCETTL")
                    End If
                Next
            Next
        End If

        '○ 帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text         '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()             '帳票ID
        CS0030REPORT.FILEtyp = iOutType                             '出力ファイル形式
        CS0030REPORT.TBLDATA = WW_TBL                               'データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If CS0030REPORT.ERR = C_MESSAGE_NO.NORMAL Then
        Else
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0022REPORT")
            Exit Sub
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

        ''Gridview表示書式設定
        'WF_GRID_Format()

        'Close
        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_TBL.Dispose()
        WW_TBL = Nothing
        cpT0007tbl.Dispose()
        cpT0007tbl = Nothing

    End Sub

    ' ***  終了ボタン処理                                                        
    Protected Sub WF_ButtonEND_Click()

        ''■■■ 画面戻先URL取得 ■■■
        'CS0017RETURNURLget.MAPID = GRT00007WRKINC.MAPIDIJKT
        'CS0017RETURNURLget.VARI = work.WF_T7SEL_MAPvariant.Text
        'CS0017RETURNURLget.CS0017RETURNURLget()
        'If CS0017RETURNURLget.ERR = C_MESSAGE_NO.NORMAL Then
        'Else
        '    Master.output(CS0017RETURNURLget.ERR, C_MESSAGE_TYPE.ABORT, "CS0017RETURNURLget")
        '    Exit Sub
        'End If

        ''次画面の変数セット
        'HttpContext.Current.Session("MAPvariant") = CS0017RETURNURLget.VARI_RETURN

        ''画面遷移実行
        'Server.Transfer(CS0017RETURNURLget.URL)

        Master.transitionPrevPage()

    End Sub

    ' ***  先頭頁ボタン処理                                                      
    Protected Sub WF_ButtonFIRST_Click()

        '○データリカバリ 
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '先頭頁に移動
        WF_GridPosition.Text = "1"
        WF_GRID_Scrole()

    End Sub

    ' ***  最終頁ボタン処理                                                      
    Protected Sub WF_ButtonLAST_Click()

        '○データリカバリ 
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '○対象データ件数取得
        Using WW_TBLview As DataView = New DataView(T0007tbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '最終頁に移動
            '月初日をセット
            Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
            '月末日からその月の日数の取得し、画面表示件数（月末調整、合計の２行分プラス）とする
            WW_GridCnt = Val(dt.AddMonths(1).AddDays(-1).ToString("dd")) + 2

            If WW_TBLview.Count Mod WW_GridCnt = 0 Then
                WF_GridPosition.Text = WW_TBLview.Count - WW_GridCnt + 1
            Else
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod WW_GridCnt) + 1
            End If
        End Using

        WF_GRID_Scrole()

    End Sub

    ' ***  leftBOX選択ボタン処理(ListBox値 ---> detailbox)　　　                 
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValues As String() = Nothing

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValues = leftview.getActiveValue
        End If

        'Select Case WF_LEFTMView.ActiveViewIndex

        '    Case 0          '乗務員
        '        If WF_ListBoxSTAFF.SelectedIndex >= 0 Then
        '            WF_SelectedIndex.Value = WF_ListBoxSTAFF.SelectedIndex
        '            WW_SelectValue = WF_ListBoxSTAFF.Items(WF_SelectedIndex.Value).Value
        '            WW_SelectTEXT = WF_ListBoxSTAFF.Items(WF_SelectedIndex.Value).Text
        '        End If
        '    Case 1          'カレンダー
        '        WW_SelectValue = CDate(WF_Calendar.Text).ToString("yyyy/MM/dd")
        '        WW_SelectTEXT = ""

        'End Select

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"
                '出荷部署 
                WF_STAFFCODE_TEXT.Text = WW_SelectValues(1)
                WF_STAFFCODE.Text = WW_SelectValues(0)
                WF_STAFFCODE.Focus()
            Case "WF_WORKDATE"
                '出庫日 
                WF_WORKDATE.Text = WW_SelectValues(0)
                WF_WORKDATE.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' ***  leftBOXキャンセルボタン処理　　　                                     
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_WORKDATE"
                '日付　 
                WF_WORKDATE.Focus()
            Case "WF_STAFFCODE"
                '従業員コード　 
                WF_STAFFCODE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''★★★★★★★★★★★★★★★★★★★★★
    ''GridView制御
    ''★★★★★★★★★★★★★★★★★★★★★
    '' ***  GRIDViewに表示するFieldの限定処理
    'Protected Sub GRID_Field_Cut(ByRef T0007tblGrid As DataTable)
    '    Dim WW_find As String = "OFF"

    '    'T0007tblGrid = T0007tbl.Copy

    '    For i As Integer = 0 To T0007tbl.Columns.Count - 1
    '        WW_find = "OFF"
    '        For j As Integer = 0 To WF_GRID.Columns.Count - 1
    '            Dim WW_COLUMNtext As New BoundField
    '            WW_COLUMNtext = WF_GRID.Columns.Item(j)
    '            If WW_COLUMNtext.DataField = T0007tbl.Columns(i).ColumnName Then
    '                WW_find = "ON"
    '                Exit For
    '            End If
    '        Next
    '        If WW_find = "OFF" Then
    '            T0007tblGrid.Columns.Remove(T0007tbl.Columns(i).ColumnName)
    '        End If
    '    Next

    'End Sub

    ' ***  GridViewダブルクリック処理                                            
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer

        '○処理準備
        'テーブルデータ 復元(TEXTファイルより復元)
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        'LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try
        work.WF_T7KIN_LINECNT.Text = WW_LINECNT

        '次画面に現状を保持情報を送る
        'Grid表示位置（先頭行）
        work.WF_T7I_GridPosition.Text = WF_GridPosition.Text
        '絞込条件をセッション変数に保存
        work.WF_T7I_Head_STAFFCODE.Text = WF_STAFFCODE.Text
        work.WF_T7I_Head_WORKDATE.Text = WF_WORKDATE.Text
        work.WF_T7I_Head_NIPPO_FROM.Text = WF_NIPPO_FROM.Text
        work.WF_T7I_Head_NIPPO_TO.Text = WF_NIPPO_TO.Text

        Dim state As String = ""
        '■■■ Grid内容(MA0003tbl)よりセッション変数編集 ■■■
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_T0007row As DataRow = T0007tbl.Rows(i)
            If WW_T0007row("SELECT") = "1" AndAlso WW_T0007row("LINECNT") = WW_LINECNT Then
                '勤務年月日　
                work.WF_T7KIN_WORKDATE.Text = WW_T0007row("WORKDATE")
                '従業員コード
                work.WF_T7KIN_STAFFCODE.Text = WW_T0007row("STAFFCODE")
                'レコード区分
                work.WF_T7KIN_RECODEKBN.Text = WW_T0007row("RECODEKBN")
                '状態
                state = WW_T0007row("STATUS")
                Exit For
            End If
        Next

        If state = "月調整" Then
            Return
        End If

        '画面遷移
        Master.transitionPage(work.WF_T7SEL_CAMPCODE.Text)

    End Sub

    '' ***  GridView 明細行書式設定処理
    'Private Sub WF_GRID_Format()

    '    Dim S0010tbl As DataTable = New DataTable

    '    '■GridViewヘッダー書式設定（列書式）
    '    For i As Integer = 0 To WF_GRID.HeaderRow.Cells.Count - 1
    '        'GridViewのthead, tbody生成
    '        WF_GRID.HeaderRow.TableSection = System.Web.UI.WebControls.TableRowSection.TableHeader

    '        'タイムスタンプ列を隠す
    '        WF_GRID.HeaderRow.Cells(2).Style.Add("display", "none")
    '        '画面選択列を隠す
    '        WF_GRID.HeaderRow.Cells(3).Style.Add("display", "none")
    '        '抽出列を隠す
    '        WF_GRID.HeaderRow.Cells(4).Style.Add("display", "none")

    '        '項番・操作・タイムスタンプ列の書式設定
    '        If i >= 5 Then
    '            '幅設定
    '            Dim WW_GridView As GridView = WF_GRID
    '            Dim WW_COLUMNtext As New BoundField
    '            WW_COLUMNtext = WW_GridView.Columns.Item(i)
    '            CS0049UPROFview.TBL = S0010tbl
    '            CS0049UPROFview.MAPID = GRT00007WRKINC.MAPIDIJKT
    '            CS0049UPROFview.VARI = WF_T7SEL_VIEWID.Text
    '            CS0049UPROFview.FIELD = WW_COLUMNtext.DataField
    '            CS0049UPROFview.CS0049UPROFview()
    '            If CS0049UPROFview.ERR = C_MESSAGE_NO.NORMAL Then
    '                If CS0049UPROFview.EFFECT = "Y" Then
    '                    WF_GRID.HeaderRow.Cells(i).Style.Value = "table-layout:fixed;word-break:keep-all;height:1.5em;font-size:small;overflow:hidden;color:white;background-color:rgb(22,54,92);border: 1px solid White;"
    '                    '幅について…フォントサイズ、強調などの影響によりヘッダーと明細にズレが生じるためピクセル(絶対値)で指定
    '                    '　※１文字を１６pxとして計算
    '                    WF_GRID.HeaderRow.Cells(i).Style.Add("width", (CS0049UPROFview.LENGTH * 16).ToString & "px")


    '                    WF_GRID.HeaderRow.Cells(i).Style.Remove("display")
    '                Else
    '                    WF_GRID.HeaderRow.Cells(i).Style.Add("display", "none")
    '                End If
    '            Else
    '                Master.output(CS0049UPROFview.ERR, C_MESSAGE_TYPE.ABORT, "CS0049UPROFview")
    '                Exit Sub
    '            End If
    '        Else
    '            '幅について…フォントサイズ、強調などの影響によりヘッダーと明細にズレが生じるためピクセル(絶対値)で指定
    '            '　※１文字を１６pxとして計算
    '            WF_GRID.HeaderRow.Cells(0).Style.Value = "height:1.5em;width:3em;font-size:small;overflow:hidden;color:rgb(0,32,96);background-color:rgb(149,179,215);border: 1px solid White;"
    '            WF_GRID.HeaderRow.Cells(1).Style.Value = "height:1.5em;width:7em;font-size:small;overflow:hidden;color:rgb(0,32,96);background-color:rgb(149,179,215);border: 1px solid White;"
    '            WF_GRID.HeaderRow.Cells(2).Style.Value = "height:1.5em;width:8em;font-size:small;overflow:hidden;color:rgb(0,32,96);background-color:rgb(149,179,215);border: 1px solid White;"
    '            WF_GRID.HeaderRow.Cells(3).Style.Value = "height:1.5em;width:8em;font-size:small;overflow:hidden;color:rgb(0,32,96);background-color:rgb(149,179,215);border: 1px solid White;"
    '            WF_GRID.HeaderRow.Cells(4).Style.Value = "height:1.5em;width:8em;font-size:small;overflow:hidden;color:rgb(0,32,96);background-color:rgb(149,179,215);border: 1px solid White;"
    '        End If
    '    Next

    '    '■GridView明細書式設定（列書式）…New
    '    For i As Integer = 0 To WF_GRID.HeaderRow.Cells.Count - 1
    '        If i < 5 Then
    '            For j As Integer = 0 To WF_GRID.Rows.Count - 1
    '                '幅について…フォントサイズ、強調などの影響によりヘッダーと明細にズレが生じるためピクセル(絶対値)で指定
    '                '　※１文字を１６pxとして計算
    '                WF_GRID.Rows(j).Cells(0).Style.Value = "height:1.2em;width:3em;font-size:small;overflow:hidden;color:black;text-align:center;border: 1px solid White;"
    '                WF_GRID.Rows(j).Cells(1).Style.Value = "height:1.2em;width:7em;font-size:small;overflow:hidden;color:black;text-align:center;border: 1px solid White;"
    '                WF_GRID.Rows(j).Cells(2).Style.Value = "height:1.2em;width:8em;font-size:small;overflow:hidden;color:black;text-align:center;border: 1px solid White;"
    '                WF_GRID.Rows(j).Cells(3).Style.Value = "height:1.2em;width:8em;font-size:small;overflow:hidden;color:black;text-align:center;border: 1px solid White;"
    '                WF_GRID.Rows(j).Cells(4).Style.Value = "height:1.2em;width:8em;font-size:small;overflow:hidden;color:black;text-align:center;border: 1px solid White;"
    '            Next
    '        Else
    '            '幅設定
    '            Dim WW_GridView As GridView = WF_GRID
    '            Dim WW_COLUMNtext As New BoundField
    '            WW_COLUMNtext = WW_GridView.Columns.Item(i)
    '            CS0049UPROFview.TBL = S0010tbl
    '            CS0049UPROFview.MAPID = GRT00007WRKINC.MAPIDIJKT
    '            CS0049UPROFview.VARI = WF_T7SEL_VIEWID.Text
    '            CS0049UPROFview.FIELD = WW_COLUMNtext.DataField
    '            CS0049UPROFview.CS0049UPROFview()
    '            If CS0049UPROFview.ERR = C_MESSAGE_NO.NORMAL Then
    '                For j As Integer = 0 To WF_GRID.Rows.Count - 1
    '                    'GridViewのthead, tbody生成
    '                    WF_GRID.Rows(j).TableSection = System.Web.UI.WebControls.TableRowSection.TableBody

    '                    'タイムスタンプ列を隠す
    '                    WF_GRID.Rows(j).Cells(2).Style.Add("display", "none")
    '                    '画面抽出列を隠す
    '                    WF_GRID.Rows(j).Cells(3).Style.Add("display", "none")
    '                    '抽出列を隠す
    '                    WF_GRID.Rows(j).Cells(4).Style.Add("display", "none")

    '                    If CS0049UPROFview.EFFECT = "Y" Then
    '                        '幅について…フォントサイズ、強調などの影響によりヘッダーと明細にズレが生じるためピクセル(絶対値)で指定
    '                        '　※１文字を１６pxとして計算
    '                        Dim WW_Style As String = ""
    '                        WW_Style = "width:" & (CS0049UPROFview.LENGTH * 16).ToString & "px;"
    '                        WW_Style = WW_Style & "text-align:" & CS0049UPROFview.ALIGN & ";"
    '                        WW_Style = WW_Style & "table-layout:fixed;word-break:keep-all;overflow:hidden;height:1.2em;font-size:small;color:black;border: 1px solid White;"
    '                        WF_GRID.Rows(j).Cells(i).Style.Value = WW_Style
    '                        WF_GRID.Rows(j).Cells(i).Style.Remove("display")
    '                    Else
    '                        WF_GRID.Rows(j).Cells(i).Style.Add("display", "none")
    '                    End If
    '                Next
    '            Else
    '                Master.output(CS0049UPROFview.ERR, C_MESSAGE_TYPE.ABORT, "CS0049UPROFview")
    '                Exit Sub
    '            End If
    '        End If
    '    Next

    '    '■Gridview明細ダブルクリック時のイベントリスナ追加　…　性能対策(行Hiddenの場合は対象外)
    '    For i = 0 To WF_GRID.Rows.Count - 1
    '        Dim WW_Function As String = "GridDbClick(this," & (Integer.Parse(WF_GRID.Rows(i).Cells(0).Text)).ToString & ");"
    '        WF_GRID.Rows(i).Attributes.Add("ondblclick", WW_Function)

    '        WF_GRID.Rows(i).BackColor = Color.White

    '        For j As Integer = 0 To WF_GRID.HeaderRow.Cells.Count - 1
    '            Dim WW_COLUMNtext As New BoundField
    '            WW_COLUMNtext = WF_GRID.Columns.Item(j)
    '            If WW_COLUMNtext.DataField = "STATUS" Then
    '                If WF_GRID.Rows(i).Cells(j).Text = "月調整" Then
    '                    WF_GRID.Rows(i).Attributes.Remove("ondblclick")
    '                End If
    '                If WF_GRID.Rows(i).Cells(j).Text = "合計" Then
    '                    WF_GRID.Rows(i).BackColor = Color.LightBlue
    '                End If
    '            End If
    '        Next
    '    Next

    '    S0010tbl.Dispose()
    '    S0010tbl = Nothing

    'End Sub

    ' *** GridView用データ取得                                                   
    Private Sub GRID_INITset()

        'ソート文字列取得
        Dim WW_SORT As String = ""
        CS0026TblSort.COMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0026TblSort.PROFID = Master.PROF_VIEW
        CS0026TblSort.TAB = ""
        CS0026TblSort.MAPID = Master.MAPID
        CS0026TblSort.VARI = Master.VIEWID
        CS0026TblSort.getSorting()
        If CS0026TblSort.ERR = C_MESSAGE_NO.NORMAL Then
            WW_SORT = "ORDER BY " & CS0026TblSort.SORTING
        End If
        '■■■ 画面表示用データ取得 ■■■

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            '■テーブル検索結果をテーブル退避
            '勤怠DB更新用テーブル
            T0007COM.T0007tbl_ColumnsAdd(T0007tbl)

            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr0 As String = ""

            'テンポラリーテーブルを作成する
            SQLStr0 = "CREATE TABLE #MBtemp " _
                    & " ( " _
                    & "  CAMPCODE nvarchar(20)," _
                    & "  STAFFCODE nvarchar(20)," _
                    & "  HORG nvarchar(20)," _
                    & " ) "

            Dim SQLcmd1 As New SqlCommand(SQLStr0, SQLcon)
            SQLcmd1.CommandTimeout = 300
            SQLcmd1.ExecuteNonQuery()
            SQLcmd1.Dispose()
            SQLcmd1 = Nothing

            '検索SQL文（乗務員のみ）
            SQLStr0 = " SELECT  isnull(rtrim(MB1.CAMPCODE),'')      as  CAMPCODE,    " _
                   & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE    " _
                   & " from   MB001_STAFF MB1                                            " _
                   & " INNER JOIN S0012_SRVAUTHOR X " _
                   & "   ON    X.TERMID       = @TERMID " _
                   & "   and   X.CAMPCODE     = @CAMPCODE " _
                   & "   and   X.OBJECT       = 'SRVORG' " _
                   & "   and   X.STYMD       <= @NOW " _
                   & "   and   X.ENDYMD      >= @NOW " _
                   & "   and   X.DELFLG      <> '1' " _
                   & " INNER JOIN S0006_ROLE Y " _
                   & "   ON    Y.CAMPCODE      = X.CAMPCODE " _
                   & "   and   Y.OBJECT        = 'SRVORG' " _
                   & "   and   Y.ROLE          = X.ROLE" _
                   & "   and   Y.STYMD        <= @NOW " _
                   & "   and   Y.ENDYMD       >= @NOW " _
                   & "   and   Y.DELFLG       <> '1' " _
                   & " INNER JOIN (select CODE from M0006_STRUCT ORG " _
                   & "             where ORG.CAMPCODE = @CAMPCODE " _
                   & "              and  ORG.OBJECT   = 'ORG' " _
                   & "              and  ORG.STRUCT   = '勤怠管理組織' " _
                   & "              and  ORG.GRCODE01 = @HORG " _
                   & "              and  ORG.STYMD   <= @NOW " _
                   & "              and  ORG.ENDYMD  >= @NOW " _
                   & "              and  ORG.DELFLG  <> '1'  " _
                   & "            ) Z " _
                   & "   ON    Z.CODE         = Y.CODE " _
                   & "  and    Z.CODE         = MB1.HORG " _
                   & " where  MB1.CAMPCODE    =  @CAMPCODE                               " _
                   & "   and  MB1.STAFFKBN    like '03%'                                 " _
                   & "   and  MB1.STYMD      <=  @SEL_ENDYMD                             " _
                   & "   and  MB1.ENDYMD     >=  @SEL_STYMD                              " _
                   & "   and  MB1.DELFLG     <>  '1'                                     " _
                   & " group by MB1.CAMPCODE, MB1.STAFFCODE                              "

            Dim WW_MBtbl As DataTable = New DataTable
            WW_MBtbl.Columns.Add("CAMPCODE", GetType(String))
            WW_MBtbl.Columns.Add("STAFFCODE", GetType(String))

            Dim SQLcmd2 As New SqlCommand(SQLStr0, SQLcon)
            Dim P2_CAMPCODE As SqlParameter = SQLcmd2.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim P2_SEL_STYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
            Dim P2_SEL_ENDYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
            Dim P2_HORG As SqlParameter = SQLcmd2.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
            Dim P2_TERMID As SqlParameter = SQLcmd2.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar)
            Dim P2_NOW As SqlParameter = SQLcmd2.Parameters.Add("@NOW", System.Data.SqlDbType.Date)

            P2_CAMPCODE.Value = work.WF_T7SEL_CAMPCODE.Text
            P2_SEL_STYMD.Value = work.WF_T7SEL_TAISHOYM.Text & "/01"
            Dim wDATE2 As Date
            Try
                wDATE2 = work.WF_T7SEL_TAISHOYM.Text & "/01"
            Catch ex As Exception
                wDATE2 = Date.Now
            End Try
            P2_SEL_ENDYMD.Value = work.WF_T7SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE2.Year, wDATE2.Month).ToString("00")
            P2_HORG.Value = work.WF_T7SEL_HORG.Text
            P2_TERMID.Value = CS0050SESSION.APSV_ID

            P2_NOW.Value = Date.Now


            Dim SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

            WW_MBtbl.Load(SQLdr2)

            '一旦テンポラリテーブルに出力
            Dim bc As New SqlClient.SqlBulkCopy(SQLcon)
            bc.DestinationTableName = "#MBtemp"
            bc.WriteToServer(WW_MBtbl)

            SQLcmd2.Dispose()
            SQLcmd2 = Nothing
            bc.Close()
            bc = Nothing


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
                 "Select * FROM (" _
               & " Select 0 As LINECNT , " _
               & "       '' as OPERATION , " _
               & "       '1' as HIDDEN , " _
               & "       TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint) , " _
               & "       ''  as STATUS, " _
               & "       isnull(rtrim(CAL.CAMPCODE),'')  as CAMPCODE, " _
               & "       isnull(rtrim(M1.NAMES),'')  as CAMPNAMES, " _
               & "       @TAISHOYM as TAISHOYM , " _
               & "       isnull(rtrim(MB.STAFFCODE),'') as STAFFCODE, " _
               & "       isnull(rtrim(MB2.STAFFNAMES),'') as STAFFNAMES , " _
               & "       isnull(rtrim(CAL.WORKINGYMD),'') as WORKDATE , " _
               & "       isnull(rtrim(CAL.WORKINGWEEK),'') as WORKINGWEEK , " _
               & "       isnull(rtrim(F1.VALUE1),'') as WORKINGWEEKNAMES , " _
               & "       isnull(rtrim(A.HDKBN),'H') as HDKBN , " _
               & "       isnull(rtrim(CAL.RECODEKBN),'0') as RECODEKBN , " _
               & "       isnull(rtrim(F2.VALUE1),'') as RECODEKBNNAMES , " _
               & "       isnull(A.SEQ,'0') as SEQ , " _
               & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
               & "       isnull(rtrim(A.NIPPOLINKCODE),'') as NIPPOLINKCODE , " _
               & "       isnull(rtrim(MB2.MORG),'') as MORG , " _
               & "       isnull(rtrim(M2M.NAMES),'') as MORGNAMES , " _
               & "       isnull(rtrim(MB2.HORG),'') as HORG , " _
               & "       isnull(rtrim(M2H.NAMES),'') as HORGNAMES , " _
               & "       isnull(rtrim(MB2.HORG),'') as SORG , " _
               & "       isnull(rtrim(M2H.NAMES),'') as SORGNAMES , " _
               & "       isnull(rtrim(MB2.STAFFKBN),'') as STAFFKBN , " _
               & "       isnull(rtrim(F8.VALUE1),'') as STAFFKBNNAMES , " _
               & "      (case when isnull(rtrim(A.HOLIDAYKBN),'') <> '' " _
               & "       then (case when isnull(rtrim(A.HOLIDAYKBN),'') = isnull(rtrim(CAL.WORKINGKBN),'') " _
               & "             then isnull(rtrim(A.HOLIDAYKBN),'') " _
               & "             else isnull(rtrim(CAL.WORKINGKBN),'') " _
               & "             end ) " _
               & "       else  isnull(rtrim(CAL.WORKINGKBN),'') " _
               & "       end ) as HOLIDAYKBN , " _
               & "       isnull(rtrim(F9.VALUE1),'') as HOLIDAYKBNNAMES , " _
               & "       isnull(rtrim(A.PAYKBN),'00') as PAYKBN , " _
               & "       isnull(rtrim(F10.VALUE1),'') as PAYKBNNAMES , " _
               & "       isnull(rtrim(A.SHUKCHOKKBN),'0') as SHUKCHOKKBN , " _
               & "       isnull(rtrim(F11.VALUE1),'') as SHUKCHOKKBNNAMES , " _
               & "       isnull(rtrim(A.WORKKBN),'')  as WORKKBN , " _
               & "       isnull(rtrim(F4.VALUE2),'') as WORKKBNNAMES , " _
               & "       isnull(rtrim(A.STDATE),'') as STDATE , " _
               & "       isnull(rtrim(A.STTIME),'') as STTIME , " _
               & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE , " _
               & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME , " _
               & "       isnull(A.WORKTIME,0) as WORKTIME , " _
               & "       isnull(A.MOVETIME,0) as MOVETIME , " _
               & "       isnull(A.ACTTIME,0) as ACTTIME , " _
               & "       isnull(rtrim(A.BINDSTDATE),'') as BINDSTDATE , " _
               & "       isnull(A.BINDTIME,'0') as BINDTIMEMIN , " _
               & "       isnull(B4.WORKINGH,'00:00:00') as BINDTIME , " _
               & "       isnull(A.NIPPOBREAKTIME,0) as NIPPOBREAKTIME , " _
               & "       isnull(A.BREAKTIME,0) as BREAKTIME , " _
               & "       isnull(A.BREAKTIMECHO,0) as BREAKTIMECHO , " _
               & "       isnull(A.NIPPOBREAKTIME,0) + isnull(A.BREAKTIME,0) + isnull(A.BREAKTIMECHO,0) as BREAKTIMETTL , " _
               & "       isnull(A.NIGHTTIME,0) as NIGHTTIME , " _
               & "       isnull(A.NIGHTTIMECHO,0) as NIGHTTIMECHO , " _
               & "       isnull(A.NIGHTTIME,0) + isnull(A.NIGHTTIMECHO,0) as NIGHTTIMETTL , " _
               & "       isnull(A.ORVERTIME,0) as ORVERTIME , " _
               & "       isnull(A.ORVERTIMECHO,0) as ORVERTIMECHO , " _
               & "       isnull(A.ORVERTIME,0) + isnull(A.ORVERTIMECHO,0) as ORVERTIMETTL , " _
               & "       isnull(A.WNIGHTTIME,0) as WNIGHTTIME , " _
               & "       isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMECHO , " _
               & "       isnull(A.WNIGHTTIME,0) + isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMETTL , " _
               & "       isnull(A.SWORKTIME,0) as SWORKTIME , " _
               & "       isnull(A.SWORKTIMECHO,0) as SWORKTIMECHO , " _
               & "       isnull(A.SWORKTIME,0) + isnull(A.SWORKTIMECHO,0) as SWORKTIMETTL , " _
               & "       isnull(A.SNIGHTTIME,0) as SNIGHTTIME , " _
               & "       isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMECHO , " _
               & "       isnull(A.SNIGHTTIME,0) + isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMETTL , " _
               & "       isnull(A.HWORKTIME,0) as HWORKTIME , " _
               & "       isnull(A.HWORKTIMECHO,0) as HWORKTIMECHO , " _
               & "       isnull(A.HWORKTIME,0) + isnull(A.HWORKTIMECHO,0) as HWORKTIMETTL , " _
               & "       isnull(A.HNIGHTTIME,0) as HNIGHTTIME , " _
               & "       isnull(A.HNIGHTTIMECHO,0) as HNIGHTTIMECHO , " _
               & "       isnull(A.HNIGHTTIME,0) + isnull(A.HNIGHTTIMECHO,0) as HNIGHTTIMETTL , " _
               & "       isnull(A.WORKNISSU,0) as WORKNISSU , " _
               & "       isnull(A.WORKNISSUCHO,0) as WORKNISSUCHO , " _
               & "       isnull(A.WORKNISSU, 0) + isnull(A.WORKNISSUCHO, 0) as WORKNISSUTTL , " _
               & "       isnull(A.SHOUKETUNISSU,0) as SHOUKETUNISSU , " _
               & "       isnull(A.SHOUKETUNISSUCHO,0) as SHOUKETUNISSUCHO , " _
               & "       isnull(A.SHOUKETUNISSU, 0) + isnull(A.SHOUKETUNISSUCHO, 0) as SHOUKETUNISSUTTL , " _
               & "       isnull(A.KUMIKETUNISSU,0) as KUMIKETUNISSU , " _
               & "       isnull(A.KUMIKETUNISSUCHO,0) as KUMIKETUNISSUCHO , " _
               & "       isnull(A.KUMIKETUNISSU, 0) + isnull(A.KUMIKETUNISSUCHO, 0) as KUMIKETUNISSUTTL , " _
               & "       isnull(A.ETCKETUNISSU,0) as ETCKETUNISSU , " _
               & "       isnull(A.ETCKETUNISSUCHO,0) as ETCKETUNISSUCHO , " _
               & "       isnull(A.ETCKETUNISSU, 0) + isnull(A.ETCKETUNISSUCHO, 0) as ETCKETUNISSUTTL , " _
               & "       isnull(A.NENKYUNISSU,0) as NENKYUNISSU , " _
               & "       isnull(A.NENKYUNISSUCHO,0) as NENKYUNISSUCHO , " _
               & "       isnull(A.NENKYUNISSU, 0) + isnull(A.NENKYUNISSUCHO, 0) as NENKYUNISSUTTL , " _
               & "       isnull(A.TOKUKYUNISSU,0) as TOKUKYUNISSU , " _
               & "       isnull(A.TOKUKYUNISSUCHO,0) as TOKUKYUNISSUCHO , " _
               & "       isnull(A.TOKUKYUNISSU, 0) + isnull(A.TOKUKYUNISSUCHO, 0) as TOKUKYUNISSUTTL , " _
               & "       isnull(A.CHIKOKSOTAINISSU,0) as CHIKOKSOTAINISSU , " _
               & "       isnull(A.CHIKOKSOTAINISSUCHO,0) as CHIKOKSOTAINISSUCHO , " _
               & "       isnull(A.CHIKOKSOTAINISSU, 0) + isnull(A.CHIKOKSOTAINISSUCHO, 0) as CHIKOKSOTAINISSUTTL , " _
               & "       isnull(A.STOCKNISSU,0) as STOCKNISSU , " _
               & "       isnull(A.STOCKNISSUCHO,0) as STOCKNISSUCHO , " _
               & "       isnull(A.STOCKNISSU, 0) + isnull(A.STOCKNISSUCHO, 0) as STOCKNISSUTTL , " _
               & "       isnull(A.KYOTEIWEEKNISSU,0) as KYOTEIWEEKNISSU , " _
               & "       isnull(A.KYOTEIWEEKNISSUCHO,0) as KYOTEIWEEKNISSUCHO , " _
               & "       isnull(A.KYOTEIWEEKNISSU, 0) + isnull(A.KYOTEIWEEKNISSUCHO, 0) as KYOTEIWEEKNISSUTTL , " _
               & "       isnull(A.WEEKNISSU,0) as WEEKNISSU , " _
               & "       isnull(A.WEEKNISSUCHO,0) as WEEKNISSUCHO , " _
               & "       isnull(A.WEEKNISSU, 0) + isnull(A.WEEKNISSUCHO, 0) as WEEKNISSUTTL , " _
               & "       isnull(A.DAIKYUNISSU,0) as DAIKYUNISSU , " _
               & "       isnull(A.DAIKYUNISSUCHO,0) as DAIKYUNISSUCHO , " _
               & "       isnull(A.DAIKYUNISSU, 0) + isnull(A.DAIKYUNISSUCHO, 0) as DAIKYUNISSUTTL , " _
               & "       isnull(A.NENSHINISSU,0) as NENSHINISSU , " _
               & "       isnull(A.NENSHINISSUCHO,0) as NENSHINISSUCHO , " _
               & "       isnull(A.NENSHINISSU, 0) + isnull(A.NENSHINISSUCHO, 0) as NENSHINISSUTTL , " _
               & "       isnull(A.SHUKCHOKNNISSU,0) as SHUKCHOKNNISSU , " _
               & "       isnull(A.SHUKCHOKNNISSUCHO,0) as SHUKCHOKNNISSUCHO , " _
               & "       isnull(A.SHUKCHOKNNISSU, 0) + isnull(A.SHUKCHOKNNISSUCHO, 0) as SHUKCHOKNNISSUTTL , " _
               & "       isnull(A.SHUKCHOKNISSU,0) as SHUKCHOKNISSU , " _
               & "       isnull(A.SHUKCHOKNISSUCHO,0) as SHUKCHOKNISSUCHO , " _
               & "       isnull(A.SHUKCHOKNISSU, 0) + isnull(A.SHUKCHOKNISSUCHO, 0) as SHUKCHOKNISSUTTL , " _
               & "       isnull(A.SHUKCHOKNHLDNISSU,0) as SHUKCHOKNHLDNISSU , " _
               & "       isnull(A.SHUKCHOKNHLDNISSUCHO,0) as SHUKCHOKNHLDNISSUCHO , " _
               & "       isnull(A.SHUKCHOKNHLDNISSU, 0) + isnull(A.SHUKCHOKNHLDNISSUCHO, 0) as SHUKCHOKNHLDNISSUTTL , " _
               & "       isnull(A.SHUKCHOKHLDNISSU,0) as SHUKCHOKHLDNISSU , " _
               & "       isnull(A.SHUKCHOKHLDNISSUCHO,0) as SHUKCHOKHLDNISSUCHO , " _
               & "       isnull(A.SHUKCHOKHLDNISSU, 0) + isnull(A.SHUKCHOKHLDNISSUCHO, 0) as SHUKCHOKHLDNISSUTTL , " _
               & "       isnull(A.TOKSAAKAISU,0) as TOKSAAKAISU , " _
               & "       isnull(A.TOKSAAKAISUCHO,0) as TOKSAAKAISUCHO , " _
               & "       isnull(A.TOKSAAKAISU, 0) + isnull(A.TOKSAAKAISUCHO, 0) as TOKSAAKAISUTTL , " _
               & "       isnull(A.TOKSABKAISU,0) as TOKSABKAISU , " _
               & "       isnull(A.TOKSABKAISUCHO,0) as TOKSABKAISUCHO , " _
               & "       isnull(A.TOKSABKAISU, 0) + isnull(A.TOKSABKAISUCHO, 0) as TOKSABKAISUTTL , " _
               & "       isnull(A.TOKSACKAISU,0) as TOKSACKAISU , " _
               & "       isnull(A.TOKSACKAISUCHO,0) as TOKSACKAISUCHO , " _
               & "       isnull(A.TOKSACKAISU, 0) + isnull(A.TOKSACKAISUCHO, 0) as TOKSACKAISUTTL , " _
               & "       isnull(A.TENKOKAISU,0) as TENKOKAISU , " _
               & "       isnull(A.TENKOKAISUCHO,0) as TENKOKAISUCHO , " _
               & "       isnull(A.TENKOKAISU, 0) + isnull(A.TENKOKAISUCHO, 0) as TENKOKAISUTTL , " _
               & "       isnull(A.HOANTIME,0) as HOANTIME , " _
               & "       isnull(A.HOANTIMECHO,0) as HOANTIMECHO , " _
               & "       isnull(A.HOANTIME, 0) + isnull(A.HOANTIMECHO, 0) as HOANTIMETTL , " _
               & "       isnull(A.KOATUTIME,0) as KOATUTIME , " _
               & "       isnull(A.KOATUTIMECHO,0) as KOATUTIMECHO , " _
               & "       isnull(A.KOATUTIME, 0) + isnull(A.KOATUTIMECHO, 0) as KOATUTIMETTL , " _
               & "       isnull(A.TOKUSA1TIME,0) as TOKUSA1TIME , " _
               & "       isnull(A.TOKUSA1TIMECHO,0) as TOKUSA1TIMECHO , " _
               & "       isnull(A.TOKUSA1TIME, 0) + isnull(A.TOKUSA1TIMECHO, 0) as TOKUSA1TIMETTL , " _
               & "       isnull(A.HAYADETIME,0) as HAYADETIME , " _
               & "       isnull(A.HAYADETIMECHO,0) as HAYADETIMECHO , " _
               & "       isnull(A.HAYADETIME, 0) + isnull(A.HAYADETIMECHO, 0) as HAYADETIMETTL , " _
               & "       isnull(A.PONPNISSU,0) as PONPNISSU , " _
               & "       isnull(A.PONPNISSUCHO,0) as PONPNISSUCHO , " _
               & "       isnull(A.PONPNISSU, 0) + isnull(A.PONPNISSUCHO, 0) as PONPNISSUTTL , " _
               & "       isnull(A.BULKNISSU,0) as BULKNISSU , " _
               & "       isnull(A.BULKNISSUCHO,'0') as BULKNISSUCHO , " _
               & "       isnull(A.BULKNISSU, 0) + isnull(A.BULKNISSUCHO, 0) as BULKNISSUTTL , " _
               & "       isnull(A.TRAILERNISSU,0) as TRAILERNISSU , " _
               & "       isnull(A.TRAILERNISSUCHO,0) as TRAILERNISSUCHO , " _
               & "       isnull(A.TRAILERNISSU, 0) + isnull(A.TRAILERNISSUCHO, 0) as TRAILERNISSUTTL , " _
               & "       isnull(A.BKINMUKAISU,0) as BKINMUKAISU , " _
               & "       isnull(A.BKINMUKAISUCHO,0) as BKINMUKAISUCHO , " _
               & "       isnull(A.BKINMUKAISU, 0) + isnull(A.BKINMUKAISUCHO, 0) as BKINMUKAISUTTL , " _
               & "       isnull(rtrim(A.SHARYOKBN),'') as SHARYOKBN , " _
               & "       isnull(rtrim(F6.VALUE1),'') as SHARYOKBNNAMES , " _
               & "       isnull(rtrim(A.OILPAYKBN),'') as OILPAYKBN , " _
               & "       isnull(rtrim(F7.VALUE1),'') as OILPAYKBNNAMES , " _
               & "       isnull(A.UNLOADCNT,0) as UNLOADCNT , " _
               & "       isnull(A.UNLOADCNTCHO,0) as UNLOADCNTCHO , " _
               & "       isnull(A.UNLOADCNT, 0) + isnull(A.UNLOADCNTCHO, 0) as UNLOADCNTTTL , " _
               & "       isnull(A.HAIDISTANCE,0) as HAIDISTANCE , " _
               & "       isnull(A.HAIDISTANCECHO,0) as HAIDISTANCECHO , " _
               & "       isnull(A.HAIDISTANCE,0) + isnull(A.HAIDISTANCECHO, 0) as HAIDISTANCETTL , " _
               & "       isnull(A.KAIDISTANCE,0) as KAIDISTANCE , " _
               & "       isnull(A.KAIDISTANCECHO,0) as KAIDISTANCECHO , " _
               & "       isnull(A.KAIDISTANCE, 0) + isnull(A.KAIDISTANCECHO, 0) as KAIDISTANCETTL , " _
               & "       isnull(rtrim(A.DELFLG),'0') as DELFLG , " _
               & "       isnull(rtrim(format(T05.UPDYMD,'yyyyMMddHHmmss')),'')  as T5ENTRYDATE , " _
               & "       isnull(rtrim(T05.L1KAISO),'') as L1KAISO , " _
               & "       isnull(rtrim(T05.LATITUDE),'') as LATITUDE , " _
               & "       isnull(rtrim(T05.LONGITUDE),'') as LONGITUDE , " _
               & "       'K' as DATAKBN , " _
               & "       '' as SHIPORG , " _
               & "       '' as SHIPORGNAMES , " _
               & "       '' as NIPPONO , " _
               & "       '' as GSHABAN , " _
               & "       '0' as RUIDISTANCE , " _
               & "       '0' as JIDISTANCE , " _
               & "       '0' as KUDISTANCE , " _
               & "       isnull(A.HAISOTIME,0) as HAISOTIME , " _
               & "       isnull(A.NENMATUNISSU,0) as NENMATUNISSU , " _
               & "       isnull(A.NENMATUNISSUCHO,0) as NENMATUNISSUCHO , " _
               & "       isnull(A.NENMATUNISSU, 0) + isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUTTL , " _
               & "       isnull(A.SHACHUHAKKBN,'0') as SHACHUHAKKBN , " _
               & "       isnull(A.SHACHUHAKNISSU,0) as SHACHUHAKNISSU , " _
               & "       isnull(A.SHACHUHAKNISSUCHO,0) as SHACHUHAKNISSUCHO , " _
               & "       isnull(A.JIKYUSHATIME,0) as JIKYUSHATIME , " _
               & "       isnull(A.JIKYUSHATIMECHO,0) as JIKYUSHATIMECHO , " _
               & "       isnull(A.JIKYUSHATIME,0) + isnull(A.JIKYUSHATIMECHO,0) as JIKYUSHATIMETTL , " _
               & "       isnull(A.MODELDISTANCE,0) as MODELDISTANCE , " _
               & "       isnull(A.MODELDISTANCECHO,0) as MODELDISTANCECHO , " _
               & "       isnull(A.MODELDISTANCE,0) + isnull(A.MODELDISTANCECHO, 0) as MODELDISTANCETTL , " _
               & "       isnull(T10.SAVECNT,0) as T10SAVECNT , " _
               & "       isnull(T10.SHARYOKBN1,'') as T10SHARYOKBN1 , " _
               & "       isnull(T10.OILPAYKBN1,'') as T10OILPAYKBN1 , " _
               & "       isnull(T10.SHUKABASHO1,'') as T10SHUKABASHO1 , " _
               & "       isnull(T10.TODOKECODE1,'') as T10TODOKECODE1 , " _
               & "       isnull(T10.MODELDISTANCE1,0) as T10MODELDISTANCE1 , " _
               & "       isnull(T10.MODIFYKBN1,'') as T10MODIFYKBN1 , " _
               & "       isnull(T10.SHARYOKBN2,'') as T10SHARYOKBN2 , " _
               & "       isnull(T10.OILPAYKBN2,'') as T10OILPAYKBN2 , " _
               & "       isnull(T10.SHUKABASHO2,'') as T10SHUKABASHO2 , " _
               & "       isnull(T10.TODOKECODE2,'') as T10TODOKECODE2 , " _
               & "       isnull(T10.MODELDISTANCE2,0) as T10MODELDISTANCE2 , " _
               & "       isnull(T10.MODIFYKBN2,'') as T10MODIFYKBN2 , " _
               & "       isnull(T10.SHARYOKBN3,'') as T10SHARYOKBN3 , " _
               & "       isnull(T10.OILPAYKBN3,'') as T10OILPAYKBN3 , " _
               & "       isnull(T10.SHUKABASHO3,'') as T10SHUKABASHO3 , " _
               & "       isnull(T10.TODOKECODE3,'') as T10TODOKECODE3 , " _
               & "       isnull(T10.MODELDISTANCE3,0) as T10MODELDISTANCE3 , " _
               & "       isnull(T10.MODIFYKBN3,'') as T10MODIFYKBN3 , " _
               & "       isnull(T10.SHARYOKBN4,'') as T10SHARYOKBN4 , " _
               & "       isnull(T10.OILPAYKBN4,'') as T10OILPAYKBN4 , " _
               & "       isnull(T10.SHUKABASHO4,'') as T10SHUKABASHO4 , " _
               & "       isnull(T10.TODOKECODE4,'') as T10TODOKECODE4 , " _
               & "       isnull(T10.MODELDISTANCE4,0) as T10MODELDISTANCE4 , " _
               & "       isnull(T10.MODIFYKBN4,'') as T10MODIFYKBN4 , " _
               & "       isnull(T10.SHARYOKBN5,'') as T10SHARYOKBN5 , " _
               & "       isnull(T10.OILPAYKBN5,'') as T10OILPAYKBN5 , " _
               & "       isnull(T10.SHUKABASHO5,'') as T10SHUKABASHO5 , " _
               & "       isnull(T10.TODOKECODE5,'') as T10TODOKECODE5 , " _
               & "       isnull(T10.MODELDISTANCE5,0) as T10MODELDISTANCE5 , " _
               & "       isnull(T10.MODIFYKBN5,'') as T10MODIFYKBN5 , " _
               & "       isnull(T10.SHARYOKBN6,'') as T10SHARYOKBN6 , " _
               & "       isnull(T10.OILPAYKBN6,'') as T10OILPAYKBN6 , " _
               & "       isnull(T10.SHUKABASHO6,'') as T10SHUKABASHO6 , " _
               & "       isnull(T10.TODOKECODE6,'') as T10TODOKECODE6 , " _
               & "       isnull(T10.MODELDISTANCE6,0) as T10MODELDISTANCE6 , " _
               & "       isnull(T10.MODIFYKBN6,'') as T10MODIFYKBN6 , " _
               & "       isnull(A.HDAIWORKTIME,0) as HDAIWORKTIME , " _
               & "       isnull(A.HDAIWORKTIMECHO,0) as HDAIWORKTIMECHO , " _
               & "       isnull(A.HDAIWORKTIME, 0) + isnull(A.HDAIWORKTIMECHO, 0) as HDAIWORKTIMETTL , " _
               & "       isnull(A.HDAINIGHTTIME,0) as HDAINIGHTTIME , " _
               & "       isnull(A.HDAINIGHTTIMECHO,0) as HDAINIGHTTIMECHO , " _
               & "       isnull(A.HDAINIGHTTIME, 0) + isnull(A.HDAINIGHTTIMECHO, 0) as HDAINIGHTTIMETTL , " _
               & "       isnull(A.SDAIWORKTIME,0) as SDAIWORKTIME , " _
               & "       isnull(A.SDAIWORKTIMECHO,0) as SDAIWORKTIMECHO , " _
               & "       isnull(A.SDAIWORKTIME, 0) + isnull(A.SDAIWORKTIMECHO, 0) as SDAIWORKTIMETTL , " _
               & "       isnull(A.SDAINIGHTTIME,0) as SDAINIGHTTIME , " _
               & "       isnull(A.SDAINIGHTTIMECHO,0) as SDAINIGHTTIMECHO , " _
               & "       isnull(A.SDAINIGHTTIME, 0) + isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMETTL , " _
               & "       isnull(A.WWORKTIME,0) as WWORKTIME , " _
               & "       isnull(A.WWORKTIMECHO,0) as WWORKTIMECHO , " _
               & "       isnull(A.WWORKTIME, 0) + isnull(A.WWORKTIMECHO, 0) as WWORKTIMETTL , " _
               & "       isnull(A.JYOMUTIME,0) as JYOMUTIME , " _
               & "       isnull(A.JYOMUTIMECHO,0) as JYOMUTIMECHO , " _
               & "       isnull(A.JYOMUTIME, 0) + isnull(A.JYOMUTIMECHO, 0) as JYOMUTIMETTL , " _
               & "       isnull(A.HWORKNISSU,0) as HWORKNISSU , " _
               & "       isnull(A.HWORKNISSUCHO,0) as HWORKNISSUCHO , " _
               & "       isnull(A.HWORKNISSU, 0) + isnull(A.HWORKNISSUCHO, 0) as HWORKNISSUTTL , " _
               & "       isnull(A.KAITENCNT,0) as KAITENCNT , " _
               & "       isnull(A.KAITENCNTCHO,0) as KAITENCNTCHO , " _
               & "       isnull(A.KAITENCNT, 0) + isnull(A.KAITENCNTCHO, 0) as KAITENCNTTTL , " _
               & "       isnull(A.SENJYOCNT,0) as SENJYOCNT , " _
               & "       isnull(A.SENJYOCNTCHO,0) as SENJYOCNTCHO , " _
               & "       isnull(A.SENJYOCNT, 0) + isnull(A.SENJYOCNTCHO, 0) as SENJYOCNTTTL , " _
               & "       isnull(A.UNLOADADDCNT1,0) as UNLOADADDCNT1 , " _
               & "       isnull(A.UNLOADADDCNT1CHO,0) as UNLOADADDCNT1CHO , " _
               & "       isnull(A.UNLOADADDCNT1, 0) + isnull(A.UNLOADADDCNT1CHO, 0) as UNLOADADDCNT1TTL , " _
               & "       isnull(A.UNLOADADDCNT2,0) as UNLOADADDCNT2 , " _
               & "       isnull(A.UNLOADADDCNT2CHO,0) as UNLOADADDCNT2CHO , " _
               & "       isnull(A.UNLOADADDCNT2, 0) + isnull(A.UNLOADADDCNT2CHO, 0) as UNLOADADDCNT2TTL , " _
               & "       isnull(A.UNLOADADDCNT3,0) as UNLOADADDCNT3 , " _
               & "       isnull(A.UNLOADADDCNT3CHO,0) as UNLOADADDCNT3CHO , " _
               & "       isnull(A.UNLOADADDCNT3, 0) + isnull(A.UNLOADADDCNT3CHO, 0) as UNLOADADDCNT3TTL , " _
               & "       isnull(A.UNLOADADDCNT4,0) as UNLOADADDCNT4 , " _
               & "       isnull(A.UNLOADADDCNT4CHO,0) as UNLOADADDCNT4CHO , " _
               & "       isnull(A.UNLOADADDCNT4, 0) + isnull(A.UNLOADADDCNT4CHO, 0) as UNLOADADDCNT4TTL , " _
               & "       isnull(A.LOADINGCNT1,0) as LOADINGCNT1 , " _
               & "       isnull(A.LOADINGCNT1CHO,0) as LOADINGCNT1CHO , " _
               & "       isnull(A.LOADINGCNT1, 0) + isnull(A.LOADINGCNT1CHO, 0) as LOADINGCNT1TTL , " _
               & "       isnull(A.LOADINGCNT2,0) as LOADINGCNT2 , " _
               & "       isnull(A.LOADINGCNT2CHO,0) as LOADINGCNT2CHO , " _
               & "       isnull(A.LOADINGCNT2, 0) + isnull(A.LOADINGCNT2CHO, 0) as LOADINGCNT2TTL , " _
               & "       isnull(A.SHORTDISTANCE1,0) as SHORTDISTANCE1 , " _
               & "       isnull(A.SHORTDISTANCE1CHO,0) as SHORTDISTANCE1CHO , " _
               & "       isnull(A.SHORTDISTANCE1, 0) + isnull(A.SHORTDISTANCE1CHO, 0) as SHORTDISTANCE1TTL , " _
               & "       isnull(A.SHORTDISTANCE2,0) as SHORTDISTANCE2 , " _
               & "       isnull(A.SHORTDISTANCE2CHO,0) as SHORTDISTANCE2CHO , " _
               & "       isnull(A.SHORTDISTANCE2, 0) + isnull(A.SHORTDISTANCE2CHO, 0) as SHORTDISTANCE2TTL , " _
               & "       isnull(MB3.SEQ, 0) as ORGSEQ , " _
               & "      (case when isnull(rtrim(A.RECODEKBN),'') <> '' " _
               & "       then  '1'  " _
               & "       else  '0' " _
               & "       end ) as DBUMUFLG  " _
               & " FROM #MBtemp MB " _
               & " INNER JOIN ( select  CAMPCODE,                                                                       " _
               & "                      WORKINGYMD,                                                                     " _
               & "                      WORKINGWEEK,                                                                    " _
               & "                      WORKINGKBN,                                                                     " _
               & "                      '0'                                 as  RECODEKBN                               " _
               & "               from   MB005_CALENDAR                                                                  " _
               & "               where  CAMPCODE                            =   @CAMPCODE                               " _
               & "                 and  format(WORKINGYMD,'yyyy/MM')        =   @TAISHOYM                               " _
               & "                 and  DELFLG                              <>  '1'                                     " _
               & "              UNION ALL                                                                               " _
               & "              select   @CAMPCODE                          as CAMPCODE,                                " _
               & "                      EOMONTH(@YMD01, 0)                  as WORKINGYMD,                              " _
               & "                      ''                                  as WORKINGWEEK,                             " _
               & "                      ''                                  as WORKINGKBN,                              " _
               & "                      '2'                                 as RECODEKBN                                " _
               & "            ) CAL                                                                                     " _
               & "   ON    CAL.CAMPCODE                                     =   @CAMPCODE                               " _
               & "   and   CAL.WORKINGYMD                                   >=  @SEL_STYMD                              " _
               & "   and   CAL.WORKINGYMD                                   <=  @SEL_ENDYMD                             " _
               & " LEFT JOIN MB001_STAFF MB2 " _
               & "   ON    MB2.CAMPCODE     = @CAMPCODE " _
               & "   and   MB2.STAFFCODE    = MB.STAFFCODE " _
               & "   and   MB2.STYMD       <= CAL.WORKINGYMD " _
               & "   and   MB2.ENDYMD      >= CAL.WORKINGYMD " _
               & "   and   MB2.DELFLG      <> '1' " _
               & " LEFT JOIN T0007_KINTAI A " _
               & "   ON    A.CAMPCODE     = @CAMPCODE " _
               & "   and   A.WORKDATE     = CAL.WORKINGYMD " _
               & "   and   A.STAFFCODE    = MB.STAFFCODE " _
               & "   and   A.RECODEKBN    = CAL.RECODEKBN " _
               & "   and   A.DELFLG      <> '1' " _
               & " LEFT JOIN MB002_STAFFORG MB3 " _
               & "   ON    MB3.CAMPCODE     = @CAMPCODE " _
               & "   and   MB3.STAFFCODE    = A.STAFFCODE " _
               & "   and   MB3.SORG         = @SEL_HORG " _
               & "   and   MB3.DELFLG      <> '1' " _
               & " LEFT JOIN M0001_CAMP M1 " _
               & "   ON    M1.CAMPCODE    = @CAMPCODE " _
               & "   and   M1.STYMD      <= @NOW " _
               & "   and   M1.ENDYMD     >= @NOW " _
               & "   and   M1.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2M " _
               & "   ON    M2M.CAMPCODE   = @CAMPCODE " _
               & "   and   M2M.ORGCODE    = MB2.MORG " _
               & "   and   M2M.STYMD      <= @NOW " _
               & "   and   M2M.ENDYMD     >= @NOW " _
               & "   and   M2M.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2H " _
               & "   ON    M2H.CAMPCODE   = @CAMPCODE " _
               & "   and   M2H.ORGCODE    = MB2.HORG " _
               & "   and   M2H.STYMD      <= @NOW " _
               & "   and   M2H.ENDYMD     >= @NOW " _
               & "   and   M2H.DELFLG     <> '1' " _
               & " LEFT JOIN MB004_WORKINGH B4 " _
               & "   ON    B4.CAMPCODE    = @CAMPCODE " _
               & "   and   B4.HORG        = MB2.HORG " _
               & "   and   B4.STAFFKBN    = MB2.STAFFKBN " _
               & "   and   B4.STYMD      <= CAL.WORKINGYMD " _
               & "   and   B4.ENDYMD     >= CAL.WORKINGYMD " _
               & "   and   B4.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F1 " _
               & "   ON    F1.CAMPCODE    = @CAMPCODE " _
               & "   and   F1.CLASS       = 'WORKINGWEEK' " _
               & "   and   F1.KEYCODE     = CAL.WORKINGWEEK " _
               & "   and   F1.STYMD      <= @NOW " _
               & "   and   F1.ENDYMD     >= @NOW " _
               & "   and   F1.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F2 " _
               & "   ON    F2.CAMPCODE    = @CAMPCODE " _
               & "   and   F2.CLASS       = 'RECODEKBN' " _
               & "   and   F2.KEYCODE     = CAL.RECODEKBN " _
               & "   and   F2.STYMD      <= @NOW " _
               & "   and   F2.ENDYMD     >= @NOW " _
               & "   and   F2.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F4 " _
               & "   ON    F4.CAMPCODE    = @CAMPCODE " _
               & "   and   F4.CLASS       = 'WORKKBN' " _
               & "   and   F4.KEYCODE     = A.WORKKBN " _
               & "   and   F4.STYMD      <= @NOW " _
               & "   and   F4.ENDYMD     >= @NOW " _
               & "   and   F4.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F6 " _
               & "   ON    F6.CAMPCODE    = @CAMPCODE " _
               & "   and   F6.CLASS       = 'SHARYOKBN' " _
               & "   and   F6.KEYCODE     = A.SHARYOKBN " _
               & "   and   F6.STYMD      <= @NOW " _
               & "   and   F6.ENDYMD     >= @NOW " _
               & "   and   F6.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F7 " _
               & "   ON    F7.CAMPCODE    = @CAMPCODE " _
               & "   and   F7.CLASS       = 'OILPAYKBN' " _
               & "   and   F7.KEYCODE     = A.OILPAYKBN " _
               & "   and   F7.STYMD      <= @NOW " _
               & "   and   F7.ENDYMD     >= @NOW " _
               & "   and   F7.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F8 " _
               & "   ON    F8.CAMPCODE    = @CAMPCODE " _
               & "   and   F8.CLASS       = 'STAFFKBN' " _
               & "   and   F8.KEYCODE     = MB2.STAFFKBN " _
               & "   and   F8.STYMD      <= @NOW " _
               & "   and   F8.ENDYMD     >= @NOW " _
               & "   and   F8.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F9 " _
               & "   ON    F9.CAMPCODE    = @CAMPCODE " _
               & "   and   F9.CLASS       = 'HOLIDAYKBN' " _
               & "   and   F9.KEYCODE     = case when isnull(rtrim(A.HOLIDAYKBN),'') <> '' " _
               & "                               then (case when isnull(rtrim(A.HOLIDAYKBN),'') = isnull(rtrim(CAL.WORKINGKBN),'') " _
               & "                                     then isnull(rtrim(A.HOLIDAYKBN),'') " _
               & "                                     else isnull(rtrim(CAL.WORKINGKBN),'') " _
               & "                                     end) " _
               & "                               else isnull(rtrim(CAL.WORKINGKBN),'') end " _
               & "   and   F9.STYMD      <= @NOW " _
               & "   and   F9.ENDYMD     >= @NOW " _
               & "   and   F9.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F10 " _
               & "   ON    F10.CAMPCODE    = @CAMPCODE " _
               & "   and   F10.CLASS       = 'PAYKBN' " _
               & "   and   F10.KEYCODE     = isnull(A.PAYKBN,'00') " _
               & "   and   F10.STYMD      <= @NOW " _
               & "   and   F10.ENDYMD     >= @NOW " _
               & "   and   F10.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F11 " _
               & "   ON    F11.CAMPCODE    = @CAMPCODE " _
               & "   and   F11.CLASS       = 'SHUKCHOKKBN' " _
               & "   and   F11.KEYCODE     = isnull(A.SHUKCHOKKBN,'0') " _
               & "   and   F11.STYMD      <= @NOW " _
               & "   and   F11.ENDYMD     >= @NOW " _
               & "   and   F11.DELFLG     <> '1' " _
               & " LEFT JOIN T0005_NIPPO T05 " _
               & "   ON    T05.CAMPCODE    = @CAMPCODE " _
               & "   and   T05.STAFFCODE   = MB.STAFFCODE " _
               & "   and   T05.YMD         = CAL.WORKINGYMD " _
               & "   and   T05.DELFLG     <> '1' " _
               & "   and   T05.SEQ         = 1 " _
               & "   and   T05.ENTRYDATE   = (select MAX(ENTRYDATE) from T0005_NIPPO where CAMPCODE = @CAMPCODE and STAFFCODE = MB.STAFFCODE and YMD = CAL.WORKINGYMD and DELFLG <> '1' and SEQ = 1) " _
               & " LEFT JOIN T0010_MODELDISTANCE T10 " _
               & "   ON    T10.CAMPCODE    = @CAMPCODE " _
               & "   and   T10.TAISHOYM    = @TAISHOYM " _
               & "   and   T10.STAFFCODE   = MB.STAFFCODE " _
               & "   and   T10.WORKDATE    = CAL.WORKINGYMD " _
               & "   and   T10.DELFLG     <> '1' " _
               & "   and   A.HDKBN         = 'H' " _
               & "   and   A.RECODEKBN     = '0' " _
               & " WHERE   MB.CAMPCODE     = @CAMPCODE " _
               & ") TBL " _
               & "WHERE 1 = 1 "

            Dim SQLWhere As String = ""
            If work.WF_T7SEL_STAFFKBN.Text <> Nothing Then
                SQLWhere = SQLWhere & " and STAFFKBN = '" & Trim(work.WF_T7SEL_STAFFKBN.Text) & "' "
            End If
            If work.WF_T7SEL_STAFFCODE.Text <> Nothing Then
                SQLWhere = SQLWhere & " and STAFFCODE = '" & Trim(work.WF_T7SEL_STAFFCODE.Text) & "' "
            End If
            If work.WF_T7SEL_STAFFNAME.Text <> Nothing Then
                SQLWhere = SQLWhere & " and STAFFNAMES like '%" & Trim(work.WF_T7SEL_STAFFNAME.Text) & "%' "
            End If

            If WW_SORT = "" Then
                WW_SORT = "ORDER BY ORGSEQ, HORG, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, HDKBN DESC"
            End If

            SQLStr = SQLStr & SQLWhere & WW_SORT
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            Dim P_NOW As SqlParameter = SQLcmd.Parameters.Add("@NOW", System.Data.SqlDbType.Date)
            Dim P_YMD01 As SqlParameter = SQLcmd.Parameters.Add("@YMD01", System.Data.SqlDbType.NVarChar)
            Dim P_HORG As SqlParameter = SQLcmd.Parameters.Add("@SEL_HORG", System.Data.SqlDbType.NVarChar)
            Dim P_SEL_STYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
            Dim P_SEL_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)

            P_CAMPCODE.Value = work.WF_T7SEL_CAMPCODE.Text
            P_TAISHOYM.Value = work.WF_T7SEL_TAISHOYM.Text

            P_NOW.Value = Date.Now
            P_YMD01.Value = work.WF_T7SEL_TAISHOYM.Text & "/01"
            P_SEL_STYMD.Value = work.WF_T7SEL_TAISHOYM.Text & "/01"
            Dim wDATE As Date
            Try
                wDATE = work.WF_T7SEL_TAISHOYM.Text & "/01"
            Catch ex As Exception
                wDATE = Date.Now
            End Try
            P_SEL_ENDYMD.Value = work.WF_T7SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
            P_HORG.Value = work.WF_T7SEL_HORG.Text

            SQLcmd.CommandTimeout = 300
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            T0007tbl.Load(SQLdr)


            '**********************************************************
            '前月分（２日前まで）を取得
            '**********************************************************
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            P_CAMPCODE = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            P_TAISHOYM = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            P_NOW = SQLcmd.Parameters.Add("@NOW", System.Data.SqlDbType.Date)
            P_YMD01 = SQLcmd.Parameters.Add("@YMD01", System.Data.SqlDbType.NVarChar)
            P_HORG = SQLcmd.Parameters.Add("@SEL_HORG", System.Data.SqlDbType.NVarChar)
            P_SEL_STYMD = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
            P_SEL_ENDYMD = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)

            Dim dtBef As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")

            P_CAMPCODE.Value = work.WF_T7SEL_CAMPCODE.Text
            P_TAISHOYM.Value = dtBef.AddMonths(-1).ToString("yyyy/MM")
            P_NOW.Value = Date.Now

            P_YMD01.Value = dtBef.AddMonths(-1).ToString("yyyy/MM/dd")
            P_SEL_STYMD.Value = dtBef.AddDays(-3).ToString("yyyy/MM/dd")
            Try
                wDATE = dtBef.AddMonths(-1).ToString("yyyy/MM/dd")
            Catch ex As Exception
                wDATE = Date.Now
            End Try
            P_SEL_ENDYMD.Value = wDATE.ToString("yyyy/MM") & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
            P_HORG.Value = work.WF_T7SEL_HORG.Text

            SQLcmd.CommandTimeout = 300
            SQLdr = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            '勤怠DB更新用テーブル
            Dim T0007BEFtbl As DataTable = T0007tbl.Clone

            T0007BEFtbl.Load(SQLdr)

            '日別のヘッダのみ抽出
            Dim WW_SEL As String = "HDKBN = 'H' and RECODEKBN = '0'"
            CS0026TblSort.TABLE = T0007BEFtbl
            CS0026TblSort.FILTER = WW_SEL
            CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            T0007BEFtbl = CS0026TblSort.sort()

            '**********************************************************
            '翌月１日分を取得
            '**********************************************************
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            P_CAMPCODE = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            P_TAISHOYM = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            P_NOW = SQLcmd.Parameters.Add("@NOW", System.Data.SqlDbType.Date)
            P_YMD01 = SQLcmd.Parameters.Add("@YMD01", System.Data.SqlDbType.NVarChar)
            P_HORG = SQLcmd.Parameters.Add("@SEL_HORG", System.Data.SqlDbType.NVarChar)
            P_SEL_STYMD = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
            P_SEL_ENDYMD = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)

            Dim dtAft As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")

            P_CAMPCODE.Value = work.WF_T7SEL_CAMPCODE.Text
            P_TAISHOYM.Value = dtBef.AddMonths(1).ToString("yyyy/MM")
            P_NOW.Value = Date.Now

            P_YMD01.Value = dtAft.AddMonths(1).ToString("yyyy/MM/dd")
            P_SEL_STYMD.Value = dtAft.AddMonths(1).ToString("yyyy/MM/dd")
            Try
                wDATE = dtAft.AddMonths(1).ToString("yyyy/MM/dd")
            Catch ex As Exception
                wDATE = Date.Now
            End Try
            P_SEL_ENDYMD.Value = dtAft.AddMonths(1).ToString("yyyy/MM/dd")
            P_HORG.Value = work.WF_T7SEL_HORG.Text

            SQLcmd.CommandTimeout = 300
            SQLdr = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            '勤怠DB更新用テーブル
            Dim T0007AFTtbl As DataTable = T0007tbl.Clone

            T0007AFTtbl.Load(SQLdr)

            '日別のヘッダのみ抽出
            WW_SEL = "HDKBN = 'H' and RECODEKBN = '0'"
            CS0026TblSort.TABLE = T0007AFTtbl
            CS0026TblSort.FILTER = WW_SEL
            CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            T0007AFTtbl = CS0026TblSort.sort()

            '前月の1週間前と翌月の１日分をマージ
            T0007tbl.Merge(T0007BEFtbl)
            T0007tbl.Merge(T0007AFTtbl)


            Dim WW_cnt As Integer = 0
            Dim WW_T0007WKtbl As DataTable = T0007tbl.Clone
            For i As Integer = 0 To T0007tbl.Rows.Count - 1
                T0007row = WW_T0007WKtbl.NewRow
                T0007row.ItemArray = T0007tbl.Rows(i).ItemArray

                If T0007row("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                    If T0007row("HDKBN") = "H" Then
                        T0007row("SELECT") = "1"
                        T0007row("HIDDEN") = "0"      '表示
                        WW_cnt += 1
                        T0007row("LINECNT") = WW_cnt
                    Else
                        T0007row("SELECT") = "1"
                        T0007row("HIDDEN") = "1"      '非表示
                        T0007row("LINECNT") = "0"
                    End If
                Else
                    T0007row("SELECT") = "0"      '対象外
                    T0007row("HIDDEN") = "1"      '非表示
                    T0007row("LINECNT") = "0"
                End If

                T0007row("SEQ") = CInt(T0007row("SEQ")).ToString("000")
                If IsDate(T0007row("WORKDATE")) Then
                    T0007row("WORKDATE") = CDate(T0007row("WORKDATE")).ToString("yyyy/MM/dd")
                Else
                    T0007row("WORKDATE") = DBNull.Value
                End If
                If IsDate(T0007row("STDATE")) Then
                    T0007row("STDATE") = CDate(T0007row("STDATE")).ToString("yyyy/MM/dd")
                Else
                    T0007row("STDATE") = T0007row("WORKDATE")
                End If
                If IsDate(T0007row("STTIME")) Then
                    T0007row("STTIME") = CDate(T0007row("STTIME")).ToString("HH:mm")
                Else
                    T0007row("STTIME") = "00:00"
                End If
                If IsDate(T0007row("ENDDATE")) Then
                    T0007row("ENDDATE") = CDate(T0007row("ENDDATE")).ToString("yyyy/MM/dd")
                Else
                    T0007row("ENDDATE") = T0007row("WORKDATE")
                End If
                If IsDate(T0007row("ENDTIME")) Then
                    T0007row("ENDTIME") = CDate(T0007row("ENDTIME")).ToString("HH:mm")
                Else
                    T0007row("ENDTIME") = "00:00"
                End If
                If IsDate(T0007row("BINDSTDATE")) Then
                    T0007row("BINDSTDATE") = CDate(T0007row("BINDSTDATE")).ToString("HH:mm")
                Else
                    T0007row("BINDSTDATE") = "00:00"
                End If

                If T0007row("BINDTIMEMIN") = 0 Then
                    If IsDate(T0007row("BINDTIME")) Then
                        If T0007row("HOLIDAYKBN") = "0" Then
                            T0007row("BINDTIME") = CDate(T0007row("BINDTIME")).ToString("hh:mm")
                        Else
                            T0007row("BINDTIME") = "00:00"
                        End If
                    Else
                        T0007row("BINDTIME") = "00:00"
                    End If
                Else
                    If T0007row("HOLIDAYKBN") = "0" Then
                        T0007row("BINDTIME") = T0007COM.formatHHMM(T0007row("BINDTIMEMIN"))
                    Else
                        T0007row("BINDTIME") = "00:00"
                    End If
                End If

                T0007row("WORKTIME") = T0007COM.formatHHMM(T0007row("WORKTIME"))
                T0007row("MOVETIME") = T0007COM.formatHHMM(T0007row("MOVETIME"))
                T0007row("ACTTIME") = T0007COM.formatHHMM(T0007row("ACTTIME"))
                T0007row("HAISOTIME") = T0007COM.formatHHMM(T0007row("HAISOTIME"))
                T0007row("JIKYUSHATIME") = T0007COM.formatHHMM(T0007row("JIKYUSHATIME"))
                T0007row("JIKYUSHATIMECHO") = T0007COM.formatHHMM(T0007row("JIKYUSHATIMECHO"))
                T0007row("JIKYUSHATIMETTL") = T0007COM.formatHHMM(T0007row("JIKYUSHATIMETTL"))

                T0007row("NIPPOBREAKTIME") = T0007COM.formatHHMM(T0007row("NIPPOBREAKTIME"))
                T0007row("BREAKTIME") = T0007COM.formatHHMM(T0007row("BREAKTIME"))
                T0007row("BREAKTIMECHO") = T0007COM.formatHHMM(T0007row("BREAKTIMECHO"))
                T0007row("BREAKTIMETTL") = T0007COM.formatHHMM(T0007row("BREAKTIMETTL"))
                T0007row("NIGHTTIME") = T0007COM.formatHHMM(T0007row("NIGHTTIME"))
                T0007row("NIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("NIGHTTIMECHO"))
                T0007row("NIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("NIGHTTIMETTL"))
                T0007row("ORVERTIME") = T0007COM.formatHHMM(T0007row("ORVERTIME"))
                T0007row("ORVERTIMECHO") = T0007COM.formatHHMM(T0007row("ORVERTIMECHO"))
                T0007row("ORVERTIMETTL") = T0007COM.formatHHMM(T0007row("ORVERTIMETTL"))
                T0007row("WNIGHTTIME") = T0007COM.formatHHMM(T0007row("WNIGHTTIME"))
                T0007row("WNIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("WNIGHTTIMECHO"))
                T0007row("WNIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("WNIGHTTIMETTL"))
                T0007row("SWORKTIME") = T0007COM.formatHHMM(T0007row("SWORKTIME"))
                T0007row("SWORKTIMECHO") = T0007COM.formatHHMM(T0007row("SWORKTIMECHO"))
                T0007row("SWORKTIMETTL") = T0007COM.formatHHMM(T0007row("SWORKTIMETTL"))
                T0007row("SNIGHTTIME") = T0007COM.formatHHMM(T0007row("SNIGHTTIME"))
                T0007row("SNIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("SNIGHTTIMECHO"))
                T0007row("SNIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("SNIGHTTIMETTL"))
                T0007row("HWORKTIME") = T0007COM.formatHHMM(T0007row("HWORKTIME"))
                T0007row("HWORKTIMECHO") = T0007COM.formatHHMM(T0007row("HWORKTIMECHO"))
                T0007row("HWORKTIMETTL") = T0007COM.formatHHMM(T0007row("HWORKTIMETTL"))
                T0007row("HNIGHTTIME") = T0007COM.formatHHMM(T0007row("HNIGHTTIME"))
                T0007row("HNIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("HNIGHTTIMECHO"))
                T0007row("HNIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("HNIGHTTIMETTL"))
                T0007row("HOANTIME") = T0007COM.formatHHMM(T0007row("HOANTIME"))
                T0007row("HOANTIMECHO") = T0007COM.formatHHMM(T0007row("HOANTIMECHO"))
                T0007row("HOANTIMETTL") = T0007COM.formatHHMM(T0007row("HOANTIMETTL"))
                T0007row("KOATUTIME") = T0007COM.formatHHMM(T0007row("KOATUTIME"))
                T0007row("KOATUTIMECHO") = T0007COM.formatHHMM(T0007row("KOATUTIMECHO"))
                T0007row("KOATUTIMETTL") = T0007COM.formatHHMM(T0007row("KOATUTIMETTL"))
                T0007row("TOKUSA1TIME") = T0007COM.formatHHMM(T0007row("TOKUSA1TIME"))
                T0007row("TOKUSA1TIMECHO") = T0007COM.formatHHMM(T0007row("TOKUSA1TIMECHO"))
                T0007row("TOKUSA1TIMETTL") = T0007COM.formatHHMM(T0007row("TOKUSA1TIMETTL"))
                T0007row("HAYADETIME") = T0007COM.formatHHMM(T0007row("HAYADETIME"))
                T0007row("HAYADETIMECHO") = T0007COM.formatHHMM(T0007row("HAYADETIMECHO"))
                T0007row("HAYADETIMETTL") = T0007COM.formatHHMM(T0007row("HAYADETIMETTL"))
                '近石
                T0007row("HDAIWORKTIME") = T0007COM.formatHHMM(T0007row("HDAIWORKTIME"))
                T0007row("HDAIWORKTIMECHO") = T0007COM.formatHHMM(T0007row("HDAIWORKTIMECHO"))
                T0007row("HDAIWORKTIMETTL") = T0007COM.formatHHMM(T0007row("HDAIWORKTIMETTL"))
                T0007row("HDAINIGHTTIME") = T0007COM.formatHHMM(T0007row("HDAINIGHTTIME"))
                T0007row("HDAINIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("HDAINIGHTTIMECHO"))
                T0007row("HDAINIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("HDAINIGHTTIMETTL"))
                T0007row("SDAIWORKTIME") = T0007COM.formatHHMM(T0007row("SDAIWORKTIME"))
                T0007row("SDAIWORKTIMECHO") = T0007COM.formatHHMM(T0007row("SDAIWORKTIMECHO"))
                T0007row("SDAIWORKTIMETTL") = T0007COM.formatHHMM(T0007row("SDAIWORKTIMETTL"))
                T0007row("SDAINIGHTTIME") = T0007COM.formatHHMM(T0007row("SDAINIGHTTIME"))
                T0007row("SDAINIGHTTIMECHO") = T0007COM.formatHHMM(T0007row("SDAINIGHTTIMECHO"))
                T0007row("SDAINIGHTTIMETTL") = T0007COM.formatHHMM(T0007row("SDAINIGHTTIMETTL"))
                T0007row("WWORKTIME") = T0007COM.formatHHMM(T0007row("WWORKTIME"))
                T0007row("WWORKTIMECHO") = T0007COM.formatHHMM(T0007row("WWORKTIMECHO"))
                T0007row("WWORKTIMETTL") = T0007COM.formatHHMM(T0007row("WWORKTIMETTL"))
                T0007row("JYOMUTIME") = T0007COM.formatHHMM(T0007row("JYOMUTIME"))
                T0007row("JYOMUTIMECHO") = T0007COM.formatHHMM(T0007row("JYOMUTIMECHO"))
                T0007row("JYOMUTIMETTL") = T0007COM.formatHHMM(T0007row("JYOMUTIMETTL"))

                T0007row("HAIDISTANCE") = Int(T0007row("HAIDISTANCE"))
                T0007row("HAIDISTANCECHO") = Int(T0007row("HAIDISTANCECHO"))
                T0007row("HAIDISTANCETTL") = Int(T0007row("HAIDISTANCETTL"))
                T0007row("KAIDISTANCE") = Int(T0007row("KAIDISTANCE"))
                T0007row("KAIDISTANCETTL") = Int(T0007row("KAIDISTANCETTL"))

                If work.WF_T7SEL_LIMITFLG.Text = "0" Then
                    '日報取込判定
                    If T0007row("T5ENTRYDATE") <> "" AndAlso T0007row("HDKBN") = "H" AndAlso T0007row("RECODEKBN") = "0" AndAlso T0007row("NIPPOLINKCODE") = "" Then
                        T0007row("STATUS") = "日報取込"
                    End If

                    If T0007row("T5ENTRYDATE") <> "" AndAlso T0007row("HDKBN") = "H" AndAlso T0007row("RECODEKBN") = "0" AndAlso T0007row("NIPPOLINKCODE") <> "" AndAlso T0007row("NIPPOLINKCODE") <> T0007row("T5ENTRYDATE") Then
                        T0007row("STATUS") = "日報変更"
                    End If
                End If

                If T0007row("SHACHUHAKKBN") = "1" Then
                    T0007row("SHACHUHAKKBNNAMES") = "✔"
                Else
                    T0007row("SHACHUHAKKBNNAMES") = ""
                End If

                WW_T0007WKtbl.Rows.Add(T0007row)
            Next
            T0007tbl = WW_T0007WKtbl.Copy

            WW_T0007WKtbl.Dispose()
            WW_T0007WKtbl = Nothing

            SQLdr.Dispose() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0007_KINTAI Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '開始日（月初）
        Dim WW_STDATE As String = work.WF_T7SEL_TAISHOYM.Text & "/01"
        '終了日（月末）
        Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
        Dim WW_ENDDATE As String = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")

        '現在紐づいている日報を取得
        Dim T0005tbl As DataTable = New DataTable
        Dim WW_STAFFCODE As String = ""
        '従業員コードを空白とし、全員取得（※パラメタ「OLD」は、NIPPOLINKCODEで紐づく日報を取得する）
        T00005ALLget(WW_STAFFCODE, WW_STDATE, WW_ENDDATE, T0005tbl, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        Dim WW_T0007SELtbl As DataTable = T0007tbl.Clone
        Dim WW_Filter As String = ""

        WW_Filter = "SELECT = '1' and HDKBN = 'H' and RECODEKBN = '0' and NIPPOLINKCODE <> '' "
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN, NIPPOLINKCODE"
        WW_T0007SELtbl = CS0026TblSort.sort()

        '日報を勤怠フォーマットに変換し、マージする
        Dim T0005WKtbl As DataTable = T0005tbl.Clone
        Dim iT0005view As DataView
        iT0005view = New DataView(T0005tbl)
        iT0005view.Sort = "YMD, STAFFCODE, WORKKBN"

        Dim WW_T0007tbl As DataTable = T0007tbl.Clone
        NIPPOget_T7Format("OLD", WW_T0007tbl, iT0005view)

        CS0026TblSort.TABLE = WW_T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, RECODEKBN, NIPPOLINKCODE"
        WW_T0007tbl = CS0026TblSort.sort()

        Dim WW_T7KEY As String = ""
        Dim WW_T5KEY As String = ""
        Dim WW_LOOP As Integer = 0
        Dim WW_T0007tbl2 As DataTable = T0007tbl.Clone
        For Each WW_T7row As DataRow In WW_T0007SELtbl.Rows
            WW_T7KEY = WW_T7row("STAFFCODE") & WW_T7row("WORKDATE")
            For i As Integer = WW_LOOP To WW_T0007tbl.Rows.Count - 1
                Dim WW_T5row As DataRow = WW_T0007tbl.Rows(i)

                WW_T5KEY = WW_T5row("STAFFCODE") & WW_T5row("WORKDATE")
                If WW_T5KEY < WW_T7KEY Then
                    Continue For
                End If
                If WW_T5KEY = WW_T7KEY Then
                    Dim ADDrow As DataRow = WW_T0007tbl2.NewRow
                    ADDrow.ItemArray = WW_T5row.ItemArray
                    WW_T0007tbl2.Rows.Add(ADDrow)
                End If
                If WW_T5KEY > WW_T7KEY Then
                    WW_LOOP = i
                    Exit For
                End If
            Next
        Next

        T0007tbl.Merge(WW_T0007tbl2)

        WW_T0007tbl.Dispose()
        WW_T0007tbl = Nothing
        WW_T0007tbl2.Dispose()
        WW_T0007tbl2 = Nothing
        WW_T0007SELtbl.Dispose()
        WW_T0007SELtbl = Nothing
        T0005WKtbl.Dispose()
        T0005WKtbl = Nothing
        iT0005view.Dispose()
        iT0005view = Nothing
        '--------------------------------------------
        '合計明細レコードの作成
        '--------------------------------------------
        CreTTLDTL(T0007tbl)

        '--------------------------------------------
        '所定労働日数初期設定（全員分）
        '--------------------------------------------
        T0007_WORKNISSUEdit(T0007tbl)

        '--------------------------------------------
        '合計編集（全員分）
        '--------------------------------------------
        '月合計（ヘッダ）レコードの集計
        T0007COM.T0007_TotalRecodeEdit(T0007tbl)

        '調整レコードの再作成
        T0007COM.T0007_ChoseiRecodeCreate(T0007tbl)


        'ソート
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_IDX As Integer = 0
        Dim WW_LINECNT As Integer = 0
        For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
            Dim WW_TBLrow As DataRow = T0007tbl.Rows(i)
            If WW_TBLrow("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If WW_TBLrow("HDKBN") = "H" Then
                    WW_TBLrow("SELECT") = "1"
                    WW_TBLrow("HIDDEN") = "0"      '表示
                    WW_LINECNT += 1
                    WW_TBLrow("LINECNT") = WW_LINECNT
                End If
            End If

            If work.WF_T7SEL_LIMITFLG.Text = "0" Then
                If WW_TBLrow("HDKBN") = "H" AndAlso WW_TBLrow("RECODEKBN") = "0" Then
                    If T0007COM.CheckHOLIDAY(WW_TBLrow("HOLIDAYKBN"), WW_TBLrow("PAYKBN")) Then
                        For j As Integer = i - 1 To 0 Step -1
                            Dim WW_row As DataRow = T0007tbl.Rows(j)
                            If WW_row("HDKBN") = "D" Then
                                Continue For
                            End If

                            Dim WW_DATE As Date = CDate(WW_TBLrow("WORKDATE")).AddDays(-1)
                            If WW_row("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd") And
                                WW_row("STAFFCODE") = WW_TBLrow("STAFFCODE") Then
                                If WW_row("ENDDATE") >= WW_TBLrow("WORKDATE") Then
                                    If WW_row("STATUS") = "" Then
                                        WW_row("STATUS") = WW_row("STATUS") & "Ｂ勤再計算"
                                    Else
                                        WW_row("STATUS") = WW_row("STATUS") & ",Ｂ勤再計算"
                                    End If
                                    Exit For
                                End If
                            Else
                                Exit For
                            End If
                        Next
                    End If
                End If
            End If
        Next

        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '絞込みボタン処理（GridViewの表示）を行う
        Scrole_SUB()

        '重複チェック
        Dim WW_MSG As String = ""
        Dim WW_ERR_REPORT As String = ""
        T0007COM.T0007_DuplCheck(T0007tbl, WW_MSG, WW_ERRCODE)
        WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_MSG

        rightview.addErrorReport(WW_ERR_REPORT)

        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        End If

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        '〇フィールドダブルクリック処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) OrElse WF_LeftMViewChange.Value = "" Then
        Else
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
                            Case "WF_WORKDATE"        '申請年月
                                If WF_WORKDATE.Text <> "" Then
                                    .WF_Calendar.Text = WF_WORKDATE.Text
                                Else
                                    .WF_Calendar.Text = work.WF_T7SEL_TAISHOYM.Text & "/01"
                                End If

                        End Select
                        .activeCalendar()

                    Case Else
                        '上記以外

                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_T7SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_STAFFCODE"              '乗務員
                                prmData = work.getStaffCodeList(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text, work.WF_T7SEL_HORG.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "WF_STAFFCODE"          '乗務員
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ' ***  GridView マウスホイール時処理 (GridViewスクロール)
    Protected Sub WF_GRID_Scrole()

        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)

        '○データリカバリ
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

    End Sub

    ' ***  GridView スクロールSUB
    Protected Sub Scrole_SUB()

        Dim WW_GridPosition As Integer                           '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        'ソート
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "LINECNT"
        T0007tbl = CS0026TblSort.sort()

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            Dim WW_T0007row As DataRow = T0007tbl.Rows(i)
            If WW_T0007row("HIDDEN") = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                WW_T0007row("EXTRACTCNT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        '月初日をセット
        Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
        '月末日からその月の日数の取得し、画面表示件数（月末調整、合計の２行分プラス）とする
        WW_GridCnt = Val(dt.AddMonths(1).AddDays(-1).ToString("dd")) + 2

        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + WW_GridCnt) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + WW_GridCnt
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - WW_GridCnt) > 0 Then
                WW_GridPosition = WW_GridPosition - WW_GridCnt
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T0007tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and EXTRACTCNT >= " & WW_GridPosition.ToString & " and EXTRACTCNT <= " & (WW_GridPosition + WW_GridCnt - 1).ToString

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = False
        'CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("EXTRACTCNT")
        End If

        ''○ Gridview表示書式設定（POSTBack時に画面が崩れる為）
        'WF_GRID_Format()

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

    End Sub

    ' ***  月合計レコード編集
    Protected Sub T0007_TTLEdit(ByRef ioTbl As DataTable)

        Dim WW_IDX As Integer = 0

        Try
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            ioTbl = CS0026TblSort.sort()

            Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
            Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
            Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
            For i As Integer = 0 To ioTbl.Rows.Count - 1
                Dim ioTblrow As DataRow = ioTbl.Rows(i)

                '削除レコードを取得
                If ioTblrow("SELECT") = "0" Then
                    Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                    DELrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007DELtbl.Rows.Add(DELrow)
                End If

                '勤怠のヘッダレコードを取得
                If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" Then
                    Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                    HEADrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007HEADtbl.Rows.Add(HEADrow)
                End If

                '勤怠の明細レコードを取得
                If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                    Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                    DTLrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007DTLtbl.Rows.Add(DTLrow)
                End If
            Next


            '月合計（明細）レコードの作成（油種別の走行距離）
            WW_IDX = 0
            Dim WW_WKDTLtbl As DataTable = ioTbl.Clone
            Dim WW_T0007tbl As DataTable = ioTbl.Clone
            Dim NIPPOtbl As DataTable = New DataTable

            'T7準備（月合計ヘッダ）
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl)
            iT0007view.Sort = "WORKDATE, STAFFCODE, HDKBN, RECODEKBN "
            iT0007view.RowFilter = "HDKBN = 'H' and RECODEKBN ='2'"
            Dim wT0007tbl As DataTable = iT0007view.ToTable

            'T7準備（合計明細レコード）
            Dim iT0007DTLview As DataView
            iT0007DTLview = New DataView(WW_T0007DTLtbl)
            iT0007DTLview.Sort = "RECODEKBN, STAFFCODE"

            'T5準備（日報レコード）
            Dim iT0005view As DataView
            iT0005view = New DataView(ioTbl)
            iT0005view.Sort = "RECODEKBN, DATAKBN, STAFFCODE"

            For Each WW_HEADrow As DataRow In wT0007tbl.Rows
                '該当する合計明細レコード抽出
                iT0007DTLview.RowFilter = "RECODEKBN = '2' and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "'"
                WW_WKDTLtbl = iT0007DTLview.ToTable()
                For i As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                    Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(i)
                    WW_WKDTLrow("UNLOADCNT") = 0
                    WW_WKDTLrow("UNLOADCNTCHO") = 0
                    WW_WKDTLrow("UNLOADCNTTTL") = 0
                    WW_WKDTLrow("HAIDISTANCE") = 0
                    WW_WKDTLrow("HAIDISTANCECHO") = 0
                    WW_WKDTLrow("HAIDISTANCETTL") = 0
                Next

                '--------------------------------------------
                '日報取得（存在するレコード全て）
                '--------------------------------------------
                iT0005view.RowFilter = "RECODEKBN = '0' and DATAKBN = 'N' and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "'"
                Dim T0005tbl As DataTable = iT0005view.ToTable()

                '日報の荷卸回数、配送キロを明細レコードに集計（車両区分、油種別）
                '荷卸がない場合、回送キロをてヘッダレコードに集計
                Dim WW_OILPAYKBN As String = ""
                Dim WW_SHARYOKBN As String = ""
                For Each WW_NIPPOrow As DataRow In T0005tbl.Rows
                    WW_OILPAYKBN = WW_NIPPOrow("OILPAYKBN")
                    WW_SHARYOKBN = WW_NIPPOrow("SHARYOKBN")
                    If WW_NIPPOrow("WORKKBN") = "B3" And WW_NIPPOrow("OILPAYKBN2") <> "10" Then
                        '水素以外
                        For i As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                            Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(i)
                            If WW_WKDTLrow("STAFFCODE") = WW_NIPPOrow("STAFFCODE") And
                               WW_WKDTLrow("SHARYOKBN") = WW_NIPPOrow("SHARYOKBN") And
                               WW_WKDTLrow("OILPAYKBN") = WW_NIPPOrow("OILPAYKBN") Then
                                WW_WKDTLrow("UNLOADCNT") = Val(WW_WKDTLrow("UNLOADCNT")) + 1
                                WW_WKDTLrow("UNLOADCNTTTL") = Val(WW_WKDTLrow("UNLOADCNTTTL")) + 1
                            End If
                        Next
                    End If

                    If WW_NIPPOrow("WORKKBN") = "F3" Then
                        '明細（車両区分、油種毎）に配送キロを設定
                        If WW_NIPPOrow("L1KAISO") <> "回送" Or
                           WW_NIPPOrow("OILPAYKBN2") = "10" Then
                            For i As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                                Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(i)
                                If WW_WKDTLrow("STAFFCODE") = WW_NIPPOrow("STAFFCODE") And
                                   WW_WKDTLrow("SHARYOKBN") = WW_SHARYOKBN And
                                   WW_WKDTLrow("OILPAYKBN") = WW_OILPAYKBN Then
                                    WW_WKDTLrow("HAIDISTANCE") = Val(WW_WKDTLrow("HAIDISTANCE")) + WW_NIPPOrow("HAIDISTANCE")
                                    WW_WKDTLrow("HAIDISTANCETTL") = Val(WW_WKDTLrow("HAIDISTANCETTL")) + WW_NIPPOrow("HAIDISTANCE")
                                End If
                            Next
                        End If

                        WW_OILPAYKBN = ""
                        WW_SHARYOKBN = ""
                    End If
                Next

                '合計明細レコードの累積
                WW_T0007tbl.Merge(WW_WKDTLtbl)
            Next

            '合計明細を削除
            CS0026TblSort.TABLE = WW_T0007DTLtbl
            CS0026TblSort.FILTER = "RECODEKBN <> '2'"
            CS0026TblSort.SORTING = "RECODEKBN, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '月合計（明細）のマージ
            ioTbl.Merge(WW_T0007tbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007tbl.Dispose()
            WW_T0007tbl = Nothing
            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            iT0007view.Dispose()
            iT0007view = Nothing
            iT0007DTLview.Dispose()
            iT0007DTLview = Nothing
            iT0005view.Dispose()
            iT0005view = Nothing
            WW_WKDTLtbl.Dispose()
            WW_WKDTLtbl = Nothing
            wT0007tbl.Dispose()
            wT0007tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_TTLEdit"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00001"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  所定労働日数設定レコード編集
    Protected Sub T0007_WORKNISSUEdit(ByRef ioTbl As DataTable)

        Dim WW_WORKNISSU As Integer = 0
        Dim WW_WORKNISSU2 As Integer = 0

        Try
            '所定労働日数（初期値取得）
            WORKNISSUget(WW_WORKNISSU, WW_ERRCODE)
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Exit Sub
            End If

            For Each WW_HEADrow As DataRow In ioTbl.Rows

                If WW_HEADrow("SELECT") = "1" And WW_HEADrow("HDKBN") = "H" And WW_HEADrow("RECODEKBN") = "2" Then
                    If WW_HEADrow("WORKNISSUTTL") = 0 Then
                        '所定労働日数（初期値取得）
                        WORKNISSUget2(WW_HEADrow, WW_WORKNISSU2, WW_ERRCODE)
                        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                            Exit Sub
                        End If

                        If WW_WORKNISSU2 > 0 Then
                            WW_HEADrow("WORKNISSU") = WW_WORKNISSU2
                            WW_HEADrow("WORKNISSUTTL") = WW_WORKNISSU2
                        End If

                        If WW_WORKNISSU2 = 0 Then
                            WW_HEADrow("WORKNISSU") = WW_WORKNISSU
                            WW_HEADrow("WORKNISSUTTL") = WW_WORKNISSU
                        End If
                    End If
                End If
            Next

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_WORKNISSUEdit"          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00001"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub



    '★★★★★★★★★★★★★★★★★★★★★
    'ＤＢ操作
    '★★★★★★★★★★★★★★★★★★★★★

    ' ***  ワークテーブル用カラム設定
    Protected Sub T0007PARMtbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        'T0007項目作成
        iTbl.Clear()
        iTbl.Columns.Add("LINECNT", GetType(Integer))
        iTbl.Columns.Add("OPERATION", GetType(String))
        iTbl.Columns.Add("TIMSTP", GetType(String))
        iTbl.Columns.Add("SELECT", GetType(Integer))
        iTbl.Columns.Add("HIDDEN", GetType(Integer))

        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("TAISHOYM", GetType(String))
        iTbl.Columns.Add("HORG", GetType(String))
        iTbl.Columns.Add("STAFFKBN", GetType(String))
        iTbl.Columns.Add("STAFFCODE", GetType(String))
        iTbl.Columns.Add("STAFFNAME", GetType(String))

        'ds.EnforceConstraints = False                          'DS制約チェックをはずす(性能対策)
    End Sub

    '' ***  勤怠ＤＢ取得
    'Protected Sub T0007_Select(ByRef iRow As DataRow, ByRef oRtn As String)

    '    'オブジェクト内容検索
    '    Try
    '        Dim SQLStr As String = ""
    '        'DataBase接続文字
    '        Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
    '        SQLcon.Open() 'DataBase接続(Open)

    '        '検索SQL文
    '        SQLStr =
    '             "SELECT TIMSTP = cast(A.UPDTIMSTP  as bigint) " _
    '            & " FROM T0007_KINTAI AS A					   " _
    '            & " WHERE A.CAMPCODE         = @P01            " _
    '            & "  and  A.TAISHOYM         = @P02            " _
    '            & "  and  A.STAFFCODE        = @P03            " _
    '            & "  and  A.WORBN            = @P05            " _
    '            & "  and  A.RECKDATE         = @P04            " _
    '            & "  and  A.HDKODEKBN        = @P06            " _
    '            & "  and  A.DELFLG          <> '1'             "

    '        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

    '        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
    '        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
    '        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
    '        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
    '        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
    '        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
    '        '○関連受注指定
    '        PARA01.Value = iRow("CAMPCODE")
    '        PARA02.Value = iRow("TAISHOYM")
    '        PARA03.Value = iRow("STAFFCODE")
    '        PARA04.Value = iRow("WORKDATE")
    '        PARA05.Value = iRow("HDKBN")
    '        PARA06.Value = iRow("RECODEKBN")

    '        '■SQL実行
    '        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '        Dim WW_TIMSTP As String = ""
    '        Dim WW_FIND As String = "OFF"
    '        While SQLdr.Read
    '            WW_TIMSTP = SQLdr("TIMSTP")
    '            If iRow("TIMSTP") = SQLdr("TIMSTP") Then
    '                WW_FIND = "ON"
    '            End If
    '        End While
    '        If WW_FIND = "ON" Then
    '            oRtn = C_MESSAGE_NO.NORMAL
    '        Else
    '            oRtn = "00002"

    '            Dim WW_ERR_MES As String = ""
    '            WW_ERR_MES = "タイムスタンプ不一致"
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　  =" & iRow("LINECNT") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 勤務年月日=" & iRow("WORKDATE") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員　  =" & iRow("STAFFCODE") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾀｲﾑｽﾀﾝﾌﾟ旧=" & iRow("TIMSTP") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾀｲﾑｽﾀﾝﾌﾟ新=" & WW_TIMSTP & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> HD区分    =" & iRow("HDKBN") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾚｺｰﾄﾞ区分 =" & iRow("RECODEKBN") & " , "
    '            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　  =" & iRow("DELFLG") & " "

    '            CS0011LOGWRITE.INFSUBCLASS = "T0007_Select"                'SUBクラス名
    '            CS0011LOGWRITE.INFPOSI = "T0007_Select"                    '
    '            CS0011LOGWRITE.NIWEA = "E"                                 '
    '            CS0011LOGWRITE.TEXT = WW_ERR_MES
    '            CS0011LOGWRITE.MESSAGENO = "00001"
    '            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        End If

    '        SQLdr.Close()
    '        SQLdr = Nothing

    '        SQLcmd.Dispose()
    '        SQLcmd = Nothing

    '        SQLcon.Close() 'DataBase接続(Close)
    '        SQLcon.Dispose()
    '        SQLcon = Nothing

    '    Catch ex As Exception
    '        CS0011LOGWRITE.INFSUBCLASS = "T0007_Select"                'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "T0007_KINTAI SELECT"
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

    '        oRtn = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub

    '    End Try


    'End Sub


    ' ***  勤怠ＤＢ編集                                                          ***
    Private Sub T0007_InsertEdit(ByRef iTbl As DataTable, ByVal iDATENOW As Date, ByRef oTBL As DataTable, ByRef oRtn As String)

        Dim WW_KEYCODE As New List(Of String)
        Dim WW_VALUE As New List(Of String)

        oRtn = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To iTbl.Rows.Count - 1
            Dim iRow As DataRow = iTbl.Rows(i)

            Dim WW_row As DataRow = oTBL.NewRow
            WW_row("CAMPCODE") = iRow("CAMPCODE")                                           '会社コード
            WW_row("TAISHOYM") = iRow("TAISHOYM")                                           '対象年月
            WW_row("STAFFCODE") = iRow("STAFFCODE")                                         '従業員コード
            WW_row("WORKDATE") = iRow("WORKDATE")                                           '日付コード
            WW_row("HDKBN") = iRow("HDKBN")                                                 'ヘッダ・明細区分
            WW_row("RECODEKBN") = iRow("RECODEKBN")                                         'レコード区分
            WW_row("SEQ") = iRow("SEQ")                                                     '明細行番号
            WW_row("ENTRYDATE") = iDATENOW.ToString("yyyyMMddHHmmssfff")                    'エントリー日時
            WW_row("NIPPOLINKCODE") = iRow("NIPPOLINKCODE")                                 '日報連結コード
            WW_row("MORG") = iRow("MORG")                                                   '管理部署
            WW_row("HORG") = iRow("HORG")                                                   '配属部署
            WW_row("SORG") = iRow("SORG")                                                   '作業部署
            WW_row("STAFFKBN") = iRow("STAFFKBN")                                           '社員区分
            WW_row("HOLIDAYKBN") = iRow("HOLIDAYKBN")                                       '休日区分
            WW_row("PAYKBN") = iRow("PAYKBN")                                               '勤怠区分
            WW_row("SHUKCHOKKBN") = iRow("SHUKCHOKKBN")                                     '宿日直区分
            WW_row("WORKKBN") = iRow("WORKKBN")                                             '作業区分
            If IsDate(iRow("STDATE")) Then
                WW_row("STDATE") = iRow("STDATE")                                           '開始日
            Else
                WW_row("STDATE") = DBNull.Value
            End If
            If IsDate(iRow("STTIME")) Then
                WW_row("STTIME") = iRow("STTIME")                                           '開始時刻
            Else
                WW_row("STTIME") = DBNull.Value                                             '開始時刻
            End If
            If IsDate(iRow("ENDDATE")) Then
                WW_row("ENDDATE") = iRow("ENDDATE")                                         '終了日
            Else
                WW_row("ENDDATE") = DBNull.Value
            End If
            If IsDate(iRow("ENDTIME")) Then
                WW_row("ENDTIME") = iRow("ENDTIME")                                         '終了時刻
            Else
                WW_row("ENDTIME") = DBNull.Value                                            '終了時刻
            End If
            WW_row("WORKTIME") = T0007COM.HHMMtoMinutes(iRow("WORKTIME"))                   '作業時間
            WW_row("MOVETIME") = T0007COM.HHMMtoMinutes(iRow("MOVETIME"))                   '移動時間
            WW_row("ACTTIME") = T0007COM.HHMMtoMinutes(iRow("ACTTIME"))                     '稼働時間
            WW_row("NIPPOBREAKTIME") = T0007COM.HHMMtoMinutes(iRow("NIPPOBREAKTIME"))       '日報休憩時間
            If IsDate(iRow("BINDSTDATE")) Then
                WW_row("BINDSTDATE") = iRow("BINDSTDATE")                                   '拘束開始時刻
            Else
                WW_row("BINDSTDATE") = DBNull.Value                                         '拘束開始時刻
            End If
            WW_row("BINDTIME") = T0007COM.HHMMtoMinutes(iRow("BINDTIME"))                   '拘束時間（分）
            WW_row("BREAKTIME") = T0007COM.HHMMtoMinutes(iRow("BREAKTIME"))                 '休憩時間（分）
            WW_row("BREAKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("BREAKTIMECHO"))           '休憩調整時間（分）
            WW_row("NIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("NIGHTTIME"))                 '所定深夜時間（分）
            WW_row("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("NIGHTTIMECHO"))           '所定深夜調整時間（分）
            WW_row("ORVERTIME") = T0007COM.HHMMtoMinutes(iRow("ORVERTIME"))                 '平日残業時間（分）
            WW_row("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(iRow("ORVERTIMECHO"))           '平日残業調整時間（分）
            WW_row("WNIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("WNIGHTTIME"))               '平日深夜時間（分）
            WW_row("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("WNIGHTTIMECHO"))         '平日深夜調整時間（分）
            WW_row("SWORKTIME") = T0007COM.HHMMtoMinutes(iRow("SWORKTIME"))                 '日曜出勤時間（分）
            WW_row("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("SWORKTIMECHO"))           '日曜出勤調整時間（分）
            WW_row("SNIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("SNIGHTTIME"))               '日曜深夜時間（分）
            WW_row("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("SNIGHTTIMECHO"))         '日曜深夜調整時間（分）
            WW_row("HWORKTIME") = T0007COM.HHMMtoMinutes(iRow("HWORKTIME"))                 '休日出勤時間（分）
            WW_row("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("HWORKTIMECHO"))           '休日出勤調整時間（分）
            WW_row("HNIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("HNIGHTTIME"))               '休日深夜時間（分）
            WW_row("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("HNIGHTTIMECHO"))         '休日深夜調整時間（分）
            WW_row("WORKNISSU") = iRow("WORKNISSU")                                         '所労
            WW_row("WORKNISSUCHO") = iRow("WORKNISSUCHO")                                   '所労調整
            WW_row("SHOUKETUNISSU") = iRow("SHOUKETUNISSU")                                 '傷欠
            WW_row("SHOUKETUNISSUCHO") = iRow("SHOUKETUNISSUCHO")                           '傷欠調整
            WW_row("KUMIKETUNISSU") = iRow("KUMIKETUNISSU")                                 '組欠
            WW_row("KUMIKETUNISSUCHO") = iRow("KUMIKETUNISSUCHO")                           '組欠調整
            WW_row("ETCKETUNISSU") = iRow("ETCKETUNISSU")                                   '他欠
            WW_row("ETCKETUNISSUCHO") = iRow("ETCKETUNISSUCHO")                             '他欠調整
            WW_row("NENKYUNISSU") = iRow("NENKYUNISSU")                                     '年休
            WW_row("NENKYUNISSUCHO") = iRow("NENKYUNISSUCHO")                               '年休調整
            WW_row("TOKUKYUNISSU") = iRow("TOKUKYUNISSU")                                   '特休
            WW_row("TOKUKYUNISSUCHO") = iRow("TOKUKYUNISSUCHO")                             '特休調整
            WW_row("CHIKOKSOTAINISSU") = iRow("CHIKOKSOTAINISSU")                           '遅早
            WW_row("CHIKOKSOTAINISSUCHO") = iRow("CHIKOKSOTAINISSUCHO")                     '遅早調整
            WW_row("STOCKNISSU") = iRow("STOCKNISSU")                                       'ストック休暇
            WW_row("STOCKNISSUCHO") = iRow("STOCKNISSUCHO")                                 'ストック休暇調整
            WW_row("KYOTEIWEEKNISSU") = iRow("KYOTEIWEEKNISSU")                             '協定週休
            WW_row("KYOTEIWEEKNISSUCHO") = iRow("KYOTEIWEEKNISSUCHO")                       '協定週休調整
            WW_row("WEEKNISSU") = iRow("WEEKNISSU")                                         '週休
            WW_row("WEEKNISSUCHO") = iRow("WEEKNISSUCHO")                                   '週休調整
            WW_row("DAIKYUNISSU") = iRow("DAIKYUNISSU")                                     '代休
            WW_row("DAIKYUNISSUCHO") = iRow("DAIKYUNISSUCHO")                               '代休調整
            WW_row("NENSHINISSU") = iRow("NENSHINISSU")                                     '年始出勤
            WW_row("NENSHINISSUCHO") = iRow("NENSHINISSUCHO")                               '年始出勤調整
            WW_row("SHUKCHOKNNISSU") = iRow("SHUKCHOKNNISSU")                               '宿日直年始
            WW_row("SHUKCHOKNNISSUCHO") = iRow("SHUKCHOKNNISSUCHO")                         '宿日直年始調整
            WW_row("SHUKCHOKNISSU") = iRow("SHUKCHOKNISSU")                                 '宿日直通常
            WW_row("SHUKCHOKNISSUCHO") = iRow("SHUKCHOKNISSUCHO")                           '宿日直通常調整
            WW_row("SHUKCHOKNHLDNISSU") = 0                                                 '宿日直年始（翌日休み）
            WW_row("SHUKCHOKNHLDNISSUCHO") = 0                                              '宿日直年始調整（翌日休み）
            WW_row("SHUKCHOKHLDNISSU") = 0                                                  '宿日直通常（翌日休み）
            WW_row("SHUKCHOKHLDNISSUCHO") = 0                                               '宿日直通常調整（翌日休み）
            WW_row("TOKSAAKAISU") = iRow("TOKSAAKAISU")                                     '特作A
            WW_row("TOKSAAKAISUCHO") = iRow("TOKSAAKAISUCHO")                               '特作A調整
            WW_row("TOKSABKAISU") = iRow("TOKSABKAISU")                                     '特作B
            WW_row("TOKSABKAISUCHO") = iRow("TOKSABKAISUCHO")                               '特作B調整
            WW_row("TOKSACKAISU") = iRow("TOKSACKAISU")                                     '特作C
            WW_row("TOKSACKAISUCHO") = iRow("TOKSACKAISUCHO")                               '特作C調整
            WW_row("TENKOKAISU") = iRow("TENKOKAISU")                                       '点呼回数
            WW_row("TENKOKAISUCHO") = iRow("TENKOKAISUCHO")                                 '点呼回数調整
            WW_row("HOANTIME") = T0007COM.HHMMtoMinutes(iRow("HOANTIME"))                   '保安検査（分）
            WW_row("HOANTIMECHO") = T0007COM.HHMMtoMinutes(iRow("HOANTIMECHO"))             '保安検査調整（分）
            WW_row("KOATUTIME") = T0007COM.HHMMtoMinutes(iRow("KOATUTIME"))                 '高圧作業（分）
            WW_row("KOATUTIMECHO") = T0007COM.HHMMtoMinutes(iRow("KOATUTIMECHO"))           '高圧作業調整（分）
            WW_row("TOKUSA1TIME") = T0007COM.HHMMtoMinutes(iRow("TOKUSA1TIME"))             '特作Ⅰ（分）
            WW_row("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(iRow("TOKUSA1TIMECHO"))       '特作Ⅰ調整（分）
            WW_row("HAYADETIME") = T0007COM.HHMMtoMinutes(iRow("HAYADETIME"))               '時差出勤手当（分）
            WW_row("HAYADETIMECHO") = T0007COM.HHMMtoMinutes(iRow("HAYADETIMECHO"))         '時差出勤手当調整（分）
            WW_row("PONPNISSU") = iRow("PONPNISSU")                                         'ポンプ
            WW_row("PONPNISSUCHO") = iRow("PONPNISSUCHO")                                   'ポンプ調整
            WW_row("BULKNISSU") = iRow("BULKNISSU")                                         'バルク
            WW_row("BULKNISSUCHO") = iRow("BULKNISSUCHO")                                   'バルク調整
            WW_row("TRAILERNISSU") = iRow("TRAILERNISSU")                                   'トレーラ
            WW_row("TRAILERNISSUCHO") = iRow("TRAILERNISSUCHO")                             'トレーラ調整
            WW_row("BKINMUKAISU") = iRow("BKINMUKAISU")                                     'B勤務
            WW_row("BKINMUKAISUCHO") = iRow("BKINMUKAISUCHO")                               'B勤務調整
            WW_row("SHARYOKBN") = iRow("SHARYOKBN")                                         '単車・トレーラ区分
            WW_row("OILPAYKBN") = iRow("OILPAYKBN")                                         '油種給与区分
            WW_row("UNLOADCNT") = iRow("UNLOADCNT")                                         '荷卸回数
            WW_row("UNLOADCNTCHO") = iRow("UNLOADCNTCHO")                                   '荷卸回数調整
            WW_row("HAIDISTANCE") = iRow("HAIDISTANCE")                                     '配送距離
            WW_row("HAIDISTANCECHO") = iRow("HAIDISTANCECHO")                               '配送調整距離
            WW_row("KAIDISTANCE") = iRow("KAIDISTANCE")                                     '回送作業距離
            WW_row("KAIDISTANCECHO") = iRow("KAIDISTANCECHO")                               '回送作業調整距離
            'NJS専用（一部、JKT）
            WW_row("HAISOTIME") = T0007COM.HHMMtoMinutes(iRow("HAISOTIME"))                 '配送時間（分）
            WW_row("NENMATUNISSU") = iRow("NENMATUNISSU")                                   '年末日数
            WW_row("NENMATUNISSUCHO") = iRow("NENMATUNISSUCHO")                             '年末日数調整
            WW_row("SHACHUHAKKBN") = iRow("SHACHUHAKKBN")                                   '車中泊区分
            WW_row("SHACHUHAKNISSU") = iRow("SHACHUHAKNISSU")                               '車中泊日数
            WW_row("SHACHUHAKNISSUCHO") = iRow("SHACHUHAKNISSUCHO")                         '車中泊日数調整
            WW_row("MODELDISTANCE") = iRow("MODELDISTANCE")                                 'モデル距離
            WW_row("MODELDISTANCECHO") = iRow("MODELDISTANCECHO")                           'モデル距離調整
            WW_row("JIKYUSHATIME") = T0007COM.HHMMtoMinutes(iRow("JIKYUSHATIME"))           '時給者作業時間（分）
            WW_row("JIKYUSHATIMECHO") = T0007COM.HHMMtoMinutes(iRow("JIKYUSHATIMECHO"))     '時給者作業時間調整（分）

            '近石専用
            WW_row("HDAIWORKTIME") = T0007COM.HHMMtoMinutes(iRow("HDAIWORKTIME"))           '代休出勤（分）
            WW_row("HDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("HDAIWORKTIMECHO"))     '代休出勤調整（分）
            WW_row("HDAINIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("HDAINIGHTTIME"))         '代休深夜（分）
            WW_row("HDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("HDAINIGHTTIMECHO"))   '代休深夜調整（分）
            WW_row("SDAIWORKTIME") = T0007COM.HHMMtoMinutes(iRow("SDAIWORKTIME"))           '代休出勤（分）
            WW_row("SDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("SDAIWORKTIMECHO"))     '代休出勤調整（分）
            WW_row("SDAINIGHTTIME") = T0007COM.HHMMtoMinutes(iRow("SDAINIGHTTIME"))         '代休深夜（分）
            WW_row("SDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(iRow("SDAINIGHTTIMECHO"))   '代休深夜調整（分）
            WW_row("WWORKTIME") = T0007COM.HHMMtoMinutes(iRow("WWORKTIME"))                 '所定内時間（分）
            WW_row("WWORKTIMECHO") = T0007COM.HHMMtoMinutes(iRow("WWORKTIMECHO"))           '所定内時間調整（分）
            WW_row("JYOMUTIME") = T0007COM.HHMMtoMinutes(iRow("JYOMUTIME"))                 '乗務時間（分）
            WW_row("JYOMUTIMECHO") = T0007COM.HHMMtoMinutes(iRow("JYOMUTIMECHO"))           '乗務時間調整（分）
            WW_row("HWORKNISSU") = iRow("HWORKNISSU")                                       '休日出勤日数
            WW_row("HWORKNISSUCHO") = iRow("HWORKNISSUCHO")                                 '休日出勤日数調整
            WW_row("KAITENCNT") = iRow("KAITENCNT")                                         '回転数
            WW_row("KAITENCNTCHO") = iRow("KAITENCNTCHO")                                   '回転数調整
            'ＪＫＴ専用
            WW_row("SENJYOCNT") = iRow("SENJYOCNT")                                         '洗浄回数
            WW_row("SENJYOCNTCHO") = iRow("SENJYOCNTCHO")                                   '洗浄回数調整
            WW_row("UNLOADADDCNT1") = iRow("UNLOADADDCNT1")                                 '危険品荷卸１回数
            WW_row("UNLOADADDCNT1CHO") = iRow("UNLOADADDCNT1CHO")                           '危険品荷卸１回数調整
            WW_row("UNLOADADDCNT2") = iRow("UNLOADADDCNT2")                                 '危険品荷卸２回数
            WW_row("UNLOADADDCNT2CHO") = iRow("UNLOADADDCNT2CHO")                           '危険品荷卸２回数調整
            WW_row("UNLOADADDCNT3") = iRow("UNLOADADDCNT3")                                 '危険品荷卸３回数
            WW_row("UNLOADADDCNT3CHO") = iRow("UNLOADADDCNT3CHO")                           '危険品荷卸３回数調整
            WW_row("UNLOADADDCNT4") = 0                                                     '危険品荷卸４回数
            WW_row("UNLOADADDCNT4CHO") = 0                                                  '危険品荷卸４回数調整
            WW_row("LOADINGCNT1") = iRow("LOADINGCNT1")                                     '危険品積込１回数
            WW_row("LOADINGCNT1CHO") = iRow("LOADINGCNT1CHO")                               '危険品積込１回数調整
            WW_row("LOADINGCNT2") = 0                                                       '危険品積込２回数
            WW_row("LOADINGCNT2CHO") = 0                                                    '危険品積込２回数調整
            WW_row("SHORTDISTANCE1") = iRow("SHORTDISTANCE1")                               '危険品積込１回数
            WW_row("SHORTDISTANCE1CHO") = iRow("SHORTDISTANCE1CHO")                         '危険品積込１回数調整
            WW_row("SHORTDISTANCE2") = iRow("SHORTDISTANCE2")                               '危険品積込２回数
            WW_row("SHORTDISTANCE2CHO") = iRow("SHORTDISTANCE2CHO")                         '危険品積込２回数調整

            WW_row("DELFLG") = iRow("DELFLG")                                               '削除フラグ
            WW_row("INITYMD") = iDATENOW                                                    '登録年月日
            WW_row("UPDYMD") = iDATENOW                                                     '更新年月日
            WW_row("UPDUSER") = Master.USERID                                               '更新ユーザＩＤ
            WW_row("UPDTERMID") = Master.USERTERMID                                         '更新端末
            WW_row("RECEIVEYMD") = C_DEFAULT_YMD                                            '集信日時

            oTBL.Rows.Add(WW_row)
        Next

    End Sub

    ' ***  従業員ＤＢ取得
    Protected Sub MB001_Select(ByRef iRow As DataRow,
                               ByRef oSTAFFKBN As String,
                               ByRef oMORG As String,
                               ByRef oHORG As String,
                               ByRef oWORKINGH As String,
                               ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        'オブジェクト内容検索
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)


            Dim WW_TBL As DataTable = New DataTable
            WW_TBL.Columns.Add("STAFFKBN", GetType(String))
            WW_TBL.Columns.Add("MORG", GetType(String))
            WW_TBL.Columns.Add("HORG", GetType(String))
            WW_TBL.Columns.Add("WORKINGH", GetType(String))

            '検索SQL文
            SQLStr =
                 "SELECT isnull(rtrim(A.STAFFKBN),'') as STAFFKBN " _
               & "      ,isnull(rtrim(A.MORG),'') as MORG " _
               & "      ,isnull(rtrim(A.HORG),'') as HORG " _
               & "      ,isnull(B.WORKINGH,'00:00:00') as WORKINGH " _
               & " FROM MB001_STAFF AS A  " _
               & " INNER JOIN MB004_WORKINGH AS B  " _
               & "    on   B.CAMPCODE     = A.CAMPCODE " _
               & "   and   B.HORG         = A.HORG " _
               & "   and   B.STAFFKBN     = A.STAFFKBN " _
               & "   and   B.STYMD       <= @STYMD " _
               & "   and   B.ENDYMD      >= @ENDYMD " _
               & "   and   B.DELFLG      <> '1' " _
               & " WHERE   A.CAMPCODE     = @CAMPCODE " _
               & "   and   A.STAFFCODE    = @STAFFCODE " _
               & "   and   A.STYMD       <= @STYMD " _
               & "   and   A.ENDYMD      >= @ENDYMD " _
               & "   and   A.DELFLG      <> '1' "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            '○関連受注指定
            PARA01.Value = iRow("CAMPCODE")
            PARA02.Value = iRow("STAFFCODE")
            PARA03.Value = Date.Now
            PARA04.Value = Date.Now

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            WW_TBL.Load(SQLdr)
            For i As Integer = 0 To WW_TBL.Rows.Count - 1
                oSTAFFKBN = WW_TBL.Rows(i)("STAFFKBN")
                oMORG = WW_TBL.Rows(i)("MORG")
                oHORG = WW_TBL.Rows(i)("HORG")
                oWORKINGH = CDate(WW_TBL.Rows(i)("WORKINGH")).ToString("HH:mm")
            Next

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB001_Select"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB001_STAFF SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  カレンダーＤＢ取得
    Protected Sub MB005_Select(ByRef iRow As DataRow,
                               ByRef oWORKINGWEEK As String,
                               ByRef oWORKINGKBN As String,
                               ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        'オブジェクト内容検索
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(rtrim(A.WORKINGWEEK),'') as WORKINGWEEK, " _
               & "        isnull(rtrim(A.WORKINGKBN),'') as WORKINGKBN  " _
               & "  from  MB005_CALENDAR A " _
               & "  where A.CAMPCODE = @CAMPCODE " _
               & "    and A.WORKINGYMD = @WORKDATE " _
               & "    and A.DELFLG <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@WORKDATE", System.Data.SqlDbType.NVarChar)
            '○関連受注指定
            PARA01.Value = iRow("CAMPCODE")
            PARA02.Value = iRow("WORKDATE")

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oWORKINGWEEK = SQLdr("WORKINGWEEK")
                oWORKINGKBN = SQLdr("WORKINGKBN")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB005_CALENDAR"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB005_CALENDAR SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  所定労働日数取得
    Protected Sub WORKNISSUget(ByRef oWORKNISSU As String,
                               ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select count(*) as WORKNISSU " _
               & "  from  MB005_CALENDAR A " _
               & " where  CAMPCODE = @CAMPCODE " _
               & "   and  format(WORKINGYMD,'yyyy/MM') = @TAISHOYM " _
               & "   and  WORKINGKBN = '0'  " _
               & "   and  DELFLG <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            '○関連受注指定
            PARA01.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA02.Value = work.WF_T7SEL_TAISHOYM.Text

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oWORKNISSU = SQLdr("WORKNISSU")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB005_CALENDAR"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB005_CALENDAR SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try



    End Sub

    ' ***  所定労働日数取得２
    Protected Sub WORKNISSUget2(ByRef iRow As DataRow,
                                ByRef oWORKNISSU As String,
                                ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(A.WORKINGN,0) as WORKINGN " _
               & "  from  MB004_WORKINGH A " _
               & " where  CAMPCODE  = @CAMPCODE " _
               & "   and  HORG      = @HORG " _
               & "   and  STAFFKBN  = @STAFFKBN " _
               & "   and  A.STYMD  <= @STYMD " _
               & "   and  A.ENDYMD >= @ENDYMD " _
               & "   and  DELFLG   <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@STAFFKBN", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            '○関連受注指定
            PARA01.Value = iRow("CAMPCODE")
            PARA02.Value = iRow("HORG")
            PARA03.Value = iRow("STAFFKBN")
            PARA04.Value = iRow("WORKDATE")
            PARA05.Value = iRow("WORKDATE")

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oWORKNISSU = SQLdr("WORKINGN")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB004_WORKINGH"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB004_WORKINGH SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  所定労働時間取得
    Protected Sub WORKINGHget(ByRef iRow As DataRow,
                                ByRef oWORKINGH As String,
                                ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim WW_MB004tbl As DataTable = New DataTable

            WW_MB004tbl.Columns.Add("WORKINGH", GetType(String))

            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(A.WORKINGH,'00:00:00') as WORKINGH " _
               & "  from  MB004_WORKINGH A " _
               & " where  CAMPCODE  = @CAMPCODE " _
               & "   and  HORG      = @HORG " _
               & "   and  STAFFKBN  = @STAFFKBN " _
               & "   and  A.STYMD  <= @STYMD " _
               & "   and  A.ENDYMD >= @ENDYMD " _
               & "   and  DELFLG   <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@STAFFKBN", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            '○関連受注指定
            PARA01.Value = iRow("CAMPCODE")
            PARA02.Value = iRow("HORG")
            PARA03.Value = iRow("STAFFKBN")
            PARA04.Value = iRow("WORKDATE")
            PARA05.Value = iRow("WORKDATE")

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            WW_MB004tbl.Load(SQLdr)

            oWORKINGH = "12:00"
            For Each MB4row As DataRow In WW_MB004tbl.Rows
                If IsDate(MB4row("WORKINGH")) Then
                    oWORKINGH = CDate(MB4row("WORKINGH")).ToString("hh:mm")
                End If
            Next

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            WW_MB004tbl.Dispose()
            WW_MB004tbl = Nothing
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB004_WORKINGH"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB004_WORKINGH SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  T0007tbl出力
    Public Sub T0007Insert(ByVal iRow As DataRow, ByRef I_SQLcon As SqlConnection)

        '検索SQL文
        '〇配送受注DB登録
        Dim SQLStr As String =
                  " INSERT INTO T0007_KINTAI " _
                & " ( " _
                & "        CAMPCODE," _
                & "        TAISHOYM," _
                & "        STAFFCODE," _
                & "        WORKDATE," _
                & "        HDKBN," _
                & "        RECODEKBN," _
                & "        SEQ," _
                & "        ENTRYDATE," _
                & "        NIPPOLINKCODE," _
                & "        MORG," _
                & "        HORG," _
                & "        SORG," _
                & "        STAFFKBN," _
                & "        HOLIDAYKBN," _
                & "        PAYKBN," _
                & "        SHUKCHOKKBN," _
                & "        WORKKBN," _
                & "        STDATE," _
                & "        STTIME," _
                & "        ENDDATE," _
                & "        ENDTIME," _
                & "        WORKTIME," _
                & "        MOVETIME," _
                & "        ACTTIME," _
                & "        BINDSTDATE," _
                & "        BINDTIME," _
                & "        NIPPOBREAKTIME," _
                & "        BREAKTIME," _
                & "        BREAKTIMECHO," _
                & "        NIGHTTIME," _
                & "        NIGHTTIMECHO," _
                & "        ORVERTIME," _
                & "        ORVERTIMECHO," _
                & "        WNIGHTTIME," _
                & "        WNIGHTTIMECHO," _
                & "        SWORKTIME," _
                & "        SWORKTIMECHO," _
                & "        SNIGHTTIME," _
                & "        SNIGHTTIMECHO," _
                & "        HWORKTIME," _
                & "        HWORKTIMECHO," _
                & "        HNIGHTTIME," _
                & "        HNIGHTTIMECHO," _
                & "        WORKNISSU," _
                & "        WORKNISSUCHO," _
                & "        SHOUKETUNISSU," _
                & "        SHOUKETUNISSUCHO," _
                & "        KUMIKETUNISSU," _
                & "        KUMIKETUNISSUCHO," _
                & "        ETCKETUNISSU," _
                & "        ETCKETUNISSUCHO," _
                & "        NENKYUNISSU," _
                & "        NENKYUNISSUCHO," _
                & "        TOKUKYUNISSU," _
                & "        TOKUKYUNISSUCHO," _
                & "        CHIKOKSOTAINISSU," _
                & "        CHIKOKSOTAINISSUCHO," _
                & "        STOCKNISSU," _
                & "        STOCKNISSUCHO," _
                & "        KYOTEIWEEKNISSU," _
                & "        KYOTEIWEEKNISSUCHO," _
                & "        WEEKNISSU," _
                & "        WEEKNISSUCHO," _
                & "        DAIKYUNISSU," _
                & "        DAIKYUNISSUCHO," _
                & "        NENSHINISSU," _
                & "        NENSHINISSUCHO," _
                & "        SHUKCHOKNNISSU," _
                & "        SHUKCHOKNNISSUCHO," _
                & "        SHUKCHOKNISSU," _
                & "        SHUKCHOKNISSUCHO," _
                & "        SHUKCHOKNHLDNISSU," _
                & "        SHUKCHOKNHLDNISSUCHO," _
                & "        SHUKCHOKHLDNISSU," _
                & "        SHUKCHOKHLDNISSUCHO," _
                & "        TOKSAAKAISU," _
                & "        TOKSAAKAISUCHO," _
                & "        TOKSABKAISU," _
                & "        TOKSABKAISUCHO," _
                & "        TOKSACKAISU," _
                & "        TOKSACKAISUCHO," _
                & "        TENKOKAISU," _
                & "        TENKOKAISUCHO," _
                & "        HOANTIME," _
                & "        HOANTIMECHO," _
                & "        KOATUTIME," _
                & "        KOATUTIMECHO," _
                & "        TOKUSA1TIME," _
                & "        TOKUSA1TIMECHO," _
                & "        HAYADETIME," _
                & "        HAYADETIMECHO," _
                & "        PONPNISSU," _
                & "        PONPNISSUCHO," _
                & "        BULKNISSU," _
                & "        BULKNISSUCHO," _
                & "        TRAILERNISSU," _
                & "        TRAILERNISSUCHO," _
                & "        BKINMUKAISU," _
                & "        BKINMUKAISUCHO," _
                & "        SHARYOKBN," _
                & "        OILPAYKBN," _
                & "        UNLOADCNT," _
                & "        UNLOADCNTCHO," _
                & "        HAIDISTANCE," _
                & "        HAIDISTANCECHO," _
                & "        KAIDISTANCE," _
                & "        KAIDISTANCECHO," _
                & "        ORVERTIMEADD," _
                & "        WNIGHTTIMEADD," _
                & "        SWORKTIMEADD," _
                & "        SNIGHTTIMEADD," _
                & "        YENDTIME," _
                & "        APPLYID," _
                & "        RIYU," _
                & "        RIYUETC," _
                & "        HAISOTIME," _
                & "        NENMATUNISSU," _
                & "        NENMATUNISSUCHO," _
                & "        SHACHUHAKKBN," _
                & "        SHACHUHAKNISSU," _
                & "        SHACHUHAKNISSUCHO," _
                & "        MODELDISTANCE," _
                & "        MODELDISTANCECHO," _
                & "        JIKYUSHATIME," _
                & "        JIKYUSHATIMECHO," _
                & "        HDAIWORKTIME," _
                & "        HDAIWORKTIMECHO," _
                & "        HDAINIGHTTIME," _
                & "        HDAINIGHTTIMECHO," _
                & "        SDAIWORKTIME," _
                & "        SDAIWORKTIMECHO," _
                & "        SDAINIGHTTIME," _
                & "        SDAINIGHTTIMECHO," _
                & "        WWORKTIME," _
                & "        WWORKTIMECHO," _
                & "        JYOMUTIME," _
                & "        JYOMUTIMECHO," _
                & "        HWORKNISSU," _
                & "        HWORKNISSUCHO," _
                & "        KAITENCNT," _
                & "        KAITENCNTCHO," _
                & "        SENJYOCNT," _
                & "        SENJYOCNTCHO," _
                & "        UNLOADADDCNT1," _
                & "        UNLOADADDCNT1CHO," _
                & "        UNLOADADDCNT2," _
                & "        UNLOADADDCNT2CHO," _
                & "        UNLOADADDCNT3," _
                & "        UNLOADADDCNT3CHO," _
                & "        UNLOADADDCNT4," _
                & "        UNLOADADDCNT4CHO," _
                & "        LOADINGCNT1," _
                & "        LOADINGCNT1CHO," _
                & "        LOADINGCNT2," _
                & "        LOADINGCNT2CHO," _
                & "        SHORTDISTANCE1," _
                & "        SHORTDISTANCE1CHO," _
                & "        SHORTDISTANCE2," _
                & "        SHORTDISTANCE2CHO," _
                & "        DELFLG," _
                & "        INITYMD," _
                & "        UPDYMD," _
                & "        UPDUSER," _
                & "        UPDTERMID," _
                & "        RECEIVEYMD " _
                & " ) " _
                & " VALUES(  " _
                & "        @CAMPCODE," _
                & "        @TAISHOYM," _
                & "        @STAFFCODE," _
                & "        @WORKDATE," _
                & "        @HDKBN," _
                & "        @RECODEKBN," _
                & "        @SEQ," _
                & "        @ENTRYDATE," _
                & "        @NIPPOLINKCODE," _
                & "        @MORG," _
                & "        @HORG," _
                & "        @SORG," _
                & "        @STAFFKBN," _
                & "        @HOLIDAYKBN," _
                & "        @PAYKBN," _
                & "        @SHUKCHOKKBN," _
                & "        @WORKKBN," _
                & "        @STDATE," _
                & "        @STTIME," _
                & "        @ENDDATE," _
                & "        @ENDTIME," _
                & "        @WORKTIME," _
                & "        @MOVETIME," _
                & "        @ACTTIME," _
                & "        @BINDSTDATE," _
                & "        @BINDTIME," _
                & "        @NIPPOBREAKTIME," _
                & "        @BREAKTIME," _
                & "        @BREAKTIMECHO," _
                & "        @NIGHTTIME," _
                & "        @NIGHTTIMECHO," _
                & "        @ORVERTIME," _
                & "        @ORVERTIMECHO," _
                & "        @WNIGHTTIME," _
                & "        @WNIGHTTIMECHO," _
                & "        @SWORKTIME," _
                & "        @SWORKTIMECHO," _
                & "        @SNIGHTTIME," _
                & "        @SNIGHTTIMECHO," _
                & "        @HWORKTIME," _
                & "        @HWORKTIMECHO," _
                & "        @HNIGHTTIME," _
                & "        @HNIGHTTIMECHO," _
                & "        @WORKNISSU," _
                & "        @WORKNISSUCHO," _
                & "        @SHOUKETUNISSU," _
                & "        @SHOUKETUNISSUCHO," _
                & "        @KUMIKETUNISSU," _
                & "        @KUMIKETUNISSUCHO," _
                & "        @ETCKETUNISSU," _
                & "        @ETCKETUNISSUCHO," _
                & "        @NENKYUNISSU," _
                & "        @NENKYUNISSUCHO," _
                & "        @TOKUKYUNISSU," _
                & "        @TOKUKYUNISSUCHO," _
                & "        @CHIKOKSOTAINISSU," _
                & "        @CHIKOKSOTAINISSUCHO," _
                & "        @STOCKNISSU," _
                & "        @STOCKNISSUCHO," _
                & "        @KYOTEIWEEKNISSU," _
                & "        @KYOTEIWEEKNISSUCHO," _
                & "        @WEEKNISSU," _
                & "        @WEEKNISSUCHO," _
                & "        @DAIKYUNISSU," _
                & "        @DAIKYUNISSUCHO," _
                & "        @NENSHINISSU," _
                & "        @NENSHINISSUCHO," _
                & "        @SHUKCHOKNNISSU," _
                & "        @SHUKCHOKNNISSUCHO," _
                & "        @SHUKCHOKNISSU," _
                & "        @SHUKCHOKNISSUCHO," _
                & "        @SHUKCHOKNHLDNISSU," _
                & "        @SHUKCHOKNHLDNISSUCHO," _
                & "        @SHUKCHOKHLDNISSU," _
                & "        @SHUKCHOKHLDNISSUCHO," _
                & "        @TOKSAAKAISU," _
                & "        @TOKSAAKAISUCHO," _
                & "        @TOKSABKAISU," _
                & "        @TOKSABKAISUCHO," _
                & "        @TOKSACKAISU," _
                & "        @TOKSACKAISUCHO," _
                & "        @TENKOKAISU," _
                & "        @TENKOKAISUCHO," _
                & "        @HOANTIME," _
                & "        @HOANTIMECHO," _
                & "        @KOATUTIME," _
                & "        @KOATUTIMECHO," _
                & "        @TOKUSA1TIME," _
                & "        @TOKUSA1TIMECHO," _
                & "        @HAYADETIME," _
                & "        @HAYADETIMECHO," _
                & "        @PONPNISSU," _
                & "        @PONPNISSUCHO," _
                & "        @BULKNISSU," _
                & "        @BULKNISSUCHO," _
                & "        @TRAILERNISSU," _
                & "        @TRAILERNISSUCHO," _
                & "        @BKINMUKAISU," _
                & "        @BKINMUKAISUCHO," _
                & "        @SHARYOKBN," _
                & "        @OILPAYKBN," _
                & "        @UNLOADCNT," _
                & "        @UNLOADCNTCHO," _
                & "        @HAIDISTANCE," _
                & "        @HAIDISTANCECHO," _
                & "        @KAIDISTANCE," _
                & "        @KAIDISTANCECHO," _
                & "        @ORVERTIMEADD," _
                & "        @WNIGHTTIMEADD," _
                & "        @SWORKTIMEADD," _
                & "        @SNIGHTTIMEADD," _
                & "        @YENDTIME," _
                & "        @APPLYID," _
                & "        @RIYU," _
                & "        @RIYUETC," _
                & "        @HAISOTIME," _
                & "        @NENMATUNISSU," _
                & "        @NENMATUNISSUCHO," _
                & "        @SHACHUHAKKBN," _
                & "        @SHACHUHAKNISSU," _
                & "        @SHACHUHAKNISSUCHO," _
                & "        @MODELDISTANCE," _
                & "        @MODELDISTANCECHO," _
                & "        @JIKYUSHATIME," _
                & "        @JIKYUSHATIMECHO," _
                & "        @HDAIWORKTIME," _
                & "        @HDAIWORKTIMECHO," _
                & "        @HDAINIGHTTIME," _
                & "        @HDAINIGHTTIMECHO," _
                & "        @SDAIWORKTIME," _
                & "        @SDAIWORKTIMECHO," _
                & "        @SDAINIGHTTIME," _
                & "        @SDAINIGHTTIMECHO," _
                & "        @WWORKTIME," _
                & "        @WWORKTIMECHO," _
                & "        @JYOMUTIME," _
                & "        @JYOMUTIMECHO," _
                & "        @HWORKNISSU," _
                & "        @HWORKNISSUCHO," _
                & "        @KAITENCNT," _
                & "        @KAITENCNTCHO," _
                & "        @SENJYOCNT," _
                & "        @SENJYOCNTCHO," _
                & "        @UNLOADADDCNT1," _
                & "        @UNLOADADDCNT1CHO," _
                & "        @UNLOADADDCNT2," _
                & "        @UNLOADADDCNT2CHO," _
                & "        @UNLOADADDCNT3," _
                & "        @UNLOADADDCNT3CHO," _
                & "        @UNLOADADDCNT4," _
                & "        @UNLOADADDCNT4CHO," _
                & "        @LOADINGCNT1," _
                & "        @LOADINGCNT1CHO," _
                & "        @LOADINGCNT2," _
                & "        @LOADINGCNT2CHO," _
                & "        @SHORTDISTANCE1," _
                & "        @SHORTDISTANCE1CHO," _
                & "        @SHORTDISTANCE2," _
                & "        @SHORTDISTANCE2CHO," _
                & "        @DELFLG," _
                & "        @INITYMD," _
                & "        @UPDYMD," _
                & "        @UPDUSER," _
                & "        @UPDTERMID," _
                & "        @RECEIVEYMD); "

        Dim SQLcmd As New SqlCommand(SQLStr, I_SQLcon)
        Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar, 7)
        Dim P_STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_WORKDATE As SqlParameter = SQLcmd.Parameters.Add("@WORKDATE", System.Data.SqlDbType.Date)
        Dim P_HDKBN As SqlParameter = SQLcmd.Parameters.Add("@HDKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_RECODEKBN As SqlParameter = SQLcmd.Parameters.Add("@RECODEKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_SEQ As SqlParameter = SQLcmd.Parameters.Add("@SEQ", System.Data.SqlDbType.Int)
        Dim P_ENTRYDATE As SqlParameter = SQLcmd.Parameters.Add("@ENTRYDATE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NIPPOLINKCODE As SqlParameter = SQLcmd.Parameters.Add("@NIPPOLINKCODE", System.Data.SqlDbType.NVarChar, 200)
        Dim P_MORG As SqlParameter = SQLcmd.Parameters.Add("@MORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_HORG As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_SORG As SqlParameter = SQLcmd.Parameters.Add("@SORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_STAFFKBN As SqlParameter = SQLcmd.Parameters.Add("@STAFFKBN", System.Data.SqlDbType.NVarChar, 5)
        Dim P_HOLIDAYKBN As SqlParameter = SQLcmd.Parameters.Add("@HOLIDAYKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_PAYKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_SHUKCHOKKBN As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_WORKKBN As SqlParameter = SQLcmd.Parameters.Add("@WORKKBN", System.Data.SqlDbType.NVarChar, 2)
        Dim P_STDATE As SqlParameter = SQLcmd.Parameters.Add("@STDATE", System.Data.SqlDbType.Date)
        Dim P_STTIME As SqlParameter = SQLcmd.Parameters.Add("@STTIME", System.Data.SqlDbType.Time)
        Dim P_ENDDATE As SqlParameter = SQLcmd.Parameters.Add("@ENDDATE", System.Data.SqlDbType.Date)
        Dim P_ENDTIME As SqlParameter = SQLcmd.Parameters.Add("@ENDTIME", System.Data.SqlDbType.Time)
        Dim P_WORKTIME As SqlParameter = SQLcmd.Parameters.Add("@WORKTIME", System.Data.SqlDbType.Int)
        Dim P_MOVETIME As SqlParameter = SQLcmd.Parameters.Add("@MOVETIME", System.Data.SqlDbType.Int)
        Dim P_ACTTIME As SqlParameter = SQLcmd.Parameters.Add("@ACTTIME", System.Data.SqlDbType.Int)
        Dim P_BINDSTDATE As SqlParameter = SQLcmd.Parameters.Add("@BINDSTDATE", System.Data.SqlDbType.Time)
        Dim P_BINDTIME As SqlParameter = SQLcmd.Parameters.Add("@BINDTIME", System.Data.SqlDbType.Int)
        Dim P_NIPPOBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@NIPPOBREAKTIME", System.Data.SqlDbType.Int)
        Dim P_BREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@BREAKTIME", System.Data.SqlDbType.Int)
        Dim P_BREAKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@BREAKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_NIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@NIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_NIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@NIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_ORVERTIME As SqlParameter = SQLcmd.Parameters.Add("@ORVERTIME", System.Data.SqlDbType.Int)
        Dim P_ORVERTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@ORVERTIMECHO", System.Data.SqlDbType.Int)
        Dim P_WNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@WNIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_WNIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@WNIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_SWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@SWORKTIME", System.Data.SqlDbType.Int)
        Dim P_SWORKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@SWORKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_SNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@SNIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_SNIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@SNIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_HWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@HWORKTIME", System.Data.SqlDbType.Int)
        Dim P_HWORKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HWORKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_HNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@HNIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_HNIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HNIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_WORKNISSU As SqlParameter = SQLcmd.Parameters.Add("@WORKNISSU", System.Data.SqlDbType.Int)
        Dim P_WORKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@WORKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHOUKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHOUKETUNISSU", System.Data.SqlDbType.Int)
        Dim P_SHOUKETUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHOUKETUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_KUMIKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@KUMIKETUNISSU", System.Data.SqlDbType.Int)
        Dim P_KUMIKETUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@KUMIKETUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_ETCKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@ETCKETUNISSU", System.Data.SqlDbType.Int)
        Dim P_ETCKETUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@ETCKETUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_NENKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@NENKYUNISSU", System.Data.SqlDbType.Int)
        Dim P_NENKYUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@NENKYUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_TOKUKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@TOKUKYUNISSU", System.Data.SqlDbType.Int)
        Dim P_TOKUKYUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@TOKUKYUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_CHIKOKSOTAINISSU As SqlParameter = SQLcmd.Parameters.Add("@CHIKOKSOTAINISSU", System.Data.SqlDbType.Int)
        Dim P_CHIKOKSOTAINISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@CHIKOKSOTAINISSUCHO", System.Data.SqlDbType.Int)
        Dim P_STOCKNISSU As SqlParameter = SQLcmd.Parameters.Add("@STOCKNISSU", System.Data.SqlDbType.Int)
        Dim P_STOCKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@STOCKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_KYOTEIWEEKNISSU As SqlParameter = SQLcmd.Parameters.Add("@KYOTEIWEEKNISSU", System.Data.SqlDbType.Int)
        Dim P_KYOTEIWEEKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@KYOTEIWEEKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_WEEKNISSU As SqlParameter = SQLcmd.Parameters.Add("@WEEKNISSU", System.Data.SqlDbType.Int)
        Dim P_WEEKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@WEEKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_DAIKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@DAIKYUNISSU", System.Data.SqlDbType.Int)
        Dim P_DAIKYUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@DAIKYUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_NENSHINISSU As SqlParameter = SQLcmd.Parameters.Add("@NENSHINISSU", System.Data.SqlDbType.Int)
        Dim P_NENSHINISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@NENSHINISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNNISSU", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNISSU", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNHLDNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNHLDNISSU", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKNHLDNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKNHLDNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKHLDNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKHLDNISSU", System.Data.SqlDbType.Int)
        Dim P_SHUKCHOKHLDNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHUKCHOKHLDNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_TOKSAAKAISU As SqlParameter = SQLcmd.Parameters.Add("@TOKSAAKAISU", System.Data.SqlDbType.Int)
        Dim P_TOKSAAKAISUCHO As SqlParameter = SQLcmd.Parameters.Add("@TOKSAAKAISUCHO", System.Data.SqlDbType.Int)
        Dim P_TOKSABKAISU As SqlParameter = SQLcmd.Parameters.Add("@TOKSABKAISU", System.Data.SqlDbType.Int)
        Dim P_TOKSABKAISUCHO As SqlParameter = SQLcmd.Parameters.Add("@TOKSABKAISUCHO", System.Data.SqlDbType.Int)
        Dim P_TOKSACKAISU As SqlParameter = SQLcmd.Parameters.Add("@TOKSACKAISU", System.Data.SqlDbType.Int)
        Dim P_TOKSACKAISUCHO As SqlParameter = SQLcmd.Parameters.Add("@TOKSACKAISUCHO", System.Data.SqlDbType.Int)
        Dim P_TENKOKAISU As SqlParameter = SQLcmd.Parameters.Add("@TENKOKAISU", System.Data.SqlDbType.Decimal)
        Dim P_TENKOKAISUCHO As SqlParameter = SQLcmd.Parameters.Add("@TENKOKAISUCHO", System.Data.SqlDbType.Decimal)
        Dim P_HOANTIME As SqlParameter = SQLcmd.Parameters.Add("@HOANTIME", System.Data.SqlDbType.Int)
        Dim P_HOANTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HOANTIMECHO", System.Data.SqlDbType.Int)
        Dim P_KOATUTIME As SqlParameter = SQLcmd.Parameters.Add("@KOATUTIME", System.Data.SqlDbType.Int)
        Dim P_KOATUTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@KOATUTIMECHO", System.Data.SqlDbType.Int)
        Dim P_TOKUSA1TIME As SqlParameter = SQLcmd.Parameters.Add("@TOKUSA1TIME", System.Data.SqlDbType.Int)
        Dim P_TOKUSA1TIMECHO As SqlParameter = SQLcmd.Parameters.Add("@TOKUSA1TIMECHO", System.Data.SqlDbType.Int)
        Dim P_HAYADETIME As SqlParameter = SQLcmd.Parameters.Add("@HAYADETIME", System.Data.SqlDbType.Int)
        Dim P_HAYADETIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HAYADETIMECHO", System.Data.SqlDbType.Int)
        Dim P_PONPNISSU As SqlParameter = SQLcmd.Parameters.Add("@PONPNISSU", System.Data.SqlDbType.Int)
        Dim P_PONPNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@PONPNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_BULKNISSU As SqlParameter = SQLcmd.Parameters.Add("@BULKNISSU", System.Data.SqlDbType.Int)
        Dim P_BULKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@BULKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_TRAILERNISSU As SqlParameter = SQLcmd.Parameters.Add("@TRAILERNISSU", System.Data.SqlDbType.Int)
        Dim P_TRAILERNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@TRAILERNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_BKINMUKAISU As SqlParameter = SQLcmd.Parameters.Add("@BKINMUKAISU", System.Data.SqlDbType.Int)
        Dim P_BKINMUKAISUCHO As SqlParameter = SQLcmd.Parameters.Add("@BKINMUKAISUCHO", System.Data.SqlDbType.Int)
        Dim P_SHARYOKBN As SqlParameter = SQLcmd.Parameters.Add("@SHARYOKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_OILPAYKBN As SqlParameter = SQLcmd.Parameters.Add("@OILPAYKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_UNLOADCNT As SqlParameter = SQLcmd.Parameters.Add("@UNLOADCNT", System.Data.SqlDbType.Int)
        Dim P_UNLOADCNTCHO As SqlParameter = SQLcmd.Parameters.Add("@UNLOADCNTCHO", System.Data.SqlDbType.Int)
        Dim P_HAIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@HAIDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_HAIDISTANCECHO As SqlParameter = SQLcmd.Parameters.Add("@HAIDISTANCECHO", System.Data.SqlDbType.Decimal)
        Dim P_KAIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@KAIDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_KAIDISTANCECHO As SqlParameter = SQLcmd.Parameters.Add("@KAIDISTANCECHO", System.Data.SqlDbType.Decimal)
        Dim P_ORVERTIMEADD As SqlParameter = SQLcmd.Parameters.Add("@ORVERTIMEADD", System.Data.SqlDbType.Int)
        Dim P_WNIGHTTIMEADD As SqlParameter = SQLcmd.Parameters.Add("@WNIGHTTIMEADD", System.Data.SqlDbType.Int)
        Dim P_SWORKTIMEADD As SqlParameter = SQLcmd.Parameters.Add("@SWORKTIMEADD", System.Data.SqlDbType.Int)
        Dim P_SNIGHTTIMEADD As SqlParameter = SQLcmd.Parameters.Add("@SNIGHTTIMEADD", System.Data.SqlDbType.Int)
        Dim P_YENDTIME As SqlParameter = SQLcmd.Parameters.Add("@YENDTIME", System.Data.SqlDbType.Time)
        Dim P_APPLYID As SqlParameter = SQLcmd.Parameters.Add("@APPLYID", System.Data.SqlDbType.NVarChar, 30)
        Dim P_RIYU As SqlParameter = SQLcmd.Parameters.Add("@RIYU", System.Data.SqlDbType.NVarChar, 2)
        Dim P_RIYUETC As SqlParameter = SQLcmd.Parameters.Add("@RIYUETC", System.Data.SqlDbType.NVarChar, 200)

        Dim P_HAISOTIME As SqlParameter = SQLcmd.Parameters.Add("@HAISOTIME", System.Data.SqlDbType.Int)
        Dim P_NENMATUNISSU As SqlParameter = SQLcmd.Parameters.Add("@NENMATUNISSU", System.Data.SqlDbType.Int)
        Dim P_NENMATUNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@NENMATUNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_SHACHUHAKKBN As SqlParameter = SQLcmd.Parameters.Add("@SHACHUHAKKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_SHACHUHAKNISSU As SqlParameter = SQLcmd.Parameters.Add("@SHACHUHAKNISSU", System.Data.SqlDbType.Int)
        Dim P_SHACHUHAKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@SHACHUHAKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_MODELDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@MODELDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_MODELDISTANCECHO As SqlParameter = SQLcmd.Parameters.Add("@MODELDISTANCECHO", System.Data.SqlDbType.Decimal)
        Dim P_JIKYUSHATIME As SqlParameter = SQLcmd.Parameters.Add("@JIKYUSHATIME", System.Data.SqlDbType.Int)
        Dim P_JIKYUSHATIMECHO As SqlParameter = SQLcmd.Parameters.Add("@JIKYUSHATIMECHO", System.Data.SqlDbType.Int)

        Dim P_HDAIWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@HDAIWORKTIME", System.Data.SqlDbType.Int)
        Dim P_HDAIWORKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HDAIWORKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_HDAINIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@HDAINIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_HDAINIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@HDAINIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_SDAIWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@SDAIWORKTIME", System.Data.SqlDbType.Int)
        Dim P_SDAIWORKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@SDAIWORKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_SDAINIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@SDAINIGHTTIME", System.Data.SqlDbType.Int)
        Dim P_SDAINIGHTTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@SDAINIGHTTIMECHO", System.Data.SqlDbType.Int)
        Dim P_WWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@WWORKTIME", System.Data.SqlDbType.Int)
        Dim P_WWORKTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@WWORKTIMECHO", System.Data.SqlDbType.Int)
        Dim P_JYOMUTIME As SqlParameter = SQLcmd.Parameters.Add("@JYOMUTIME", System.Data.SqlDbType.Int)
        Dim P_JYOMUTIMECHO As SqlParameter = SQLcmd.Parameters.Add("@JYOMUTIMECHO", System.Data.SqlDbType.Int)
        Dim P_HWORKNISSU As SqlParameter = SQLcmd.Parameters.Add("@HWORKNISSU", System.Data.SqlDbType.Int)
        Dim P_HWORKNISSUCHO As SqlParameter = SQLcmd.Parameters.Add("@HWORKNISSUCHO", System.Data.SqlDbType.Int)
        Dim P_KAITENCNT As SqlParameter = SQLcmd.Parameters.Add("@KAITENCNT", System.Data.SqlDbType.Int)
        Dim P_KAITENCNTCHO As SqlParameter = SQLcmd.Parameters.Add("@KAITENCNTCHO", System.Data.SqlDbType.Int)

        Dim P_SENJYOCNT As SqlParameter = SQLcmd.Parameters.Add("@SENJYOCNT", System.Data.SqlDbType.Int)
        Dim P_SENJYOCNTCHO As SqlParameter = SQLcmd.Parameters.Add("@SENJYOCNTCHO", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT1 As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT1", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT1CHO As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT1CHO", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT2 As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT2", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT2CHO As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT2CHO", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT3 As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT3", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT3CHO As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT3CHO", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT4 As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT4", System.Data.SqlDbType.Int)
        Dim P_UNLOADADDCNT4CHO As SqlParameter = SQLcmd.Parameters.Add("@UNLOADADDCNT4CHO", System.Data.SqlDbType.Int)
        Dim P_LOADINGCNT1 As SqlParameter = SQLcmd.Parameters.Add("@LOADINGCNT1", System.Data.SqlDbType.Int)
        Dim P_LOADINGCNT1CHO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGCNT1CHO", System.Data.SqlDbType.Int)
        Dim P_LOADINGCNT2 As SqlParameter = SQLcmd.Parameters.Add("@LOADINGCNT2", System.Data.SqlDbType.Int)
        Dim P_LOADINGCNT2CHO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGCNT2CHO", System.Data.SqlDbType.Int)
        Dim P_SHORTDISTANCE1 As SqlParameter = SQLcmd.Parameters.Add("@SHORTDISTANCE1", System.Data.SqlDbType.Int)
        Dim P_SHORTDISTANCE1CHO As SqlParameter = SQLcmd.Parameters.Add("@SHORTDISTANCE1CHO", System.Data.SqlDbType.Int)
        Dim P_SHORTDISTANCE2 As SqlParameter = SQLcmd.Parameters.Add("@SHORTDISTANCE2", System.Data.SqlDbType.Int)
        Dim P_SHORTDISTANCE2CHO As SqlParameter = SQLcmd.Parameters.Add("@SHORTDISTANCE2CHO", System.Data.SqlDbType.Int)

        Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
        Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", System.Data.SqlDbType.DateTime)
        Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
        Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar, 20)
        Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar, 30)
        Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

        P_CAMPCODE.Value = iRow("CAMPCODE")
        P_TAISHOYM.Value = iRow("TAISHOYM")
        P_STAFFCODE.Value = iRow("STAFFCODE")
        P_WORKDATE.Value = iRow("WORKDATE")
        P_HDKBN.Value = iRow("HDKBN")
        P_RECODEKBN.Value = iRow("RECODEKBN")
        P_SEQ.Value = iRow("SEQ")
        P_ENTRYDATE.Value = iRow("ENTRYDATE")
        P_NIPPOLINKCODE.Value = iRow("NIPPOLINKCODE")
        P_MORG.Value = iRow("MORG")
        P_HORG.Value = iRow("HORG")
        P_SORG.Value = iRow("SORG")
        P_STAFFKBN.Value = iRow("STAFFKBN")
        P_HOLIDAYKBN.Value = iRow("HOLIDAYKBN")
        P_PAYKBN.Value = iRow("PAYKBN")
        P_SHUKCHOKKBN.Value = iRow("SHUKCHOKKBN")
        P_WORKKBN.Value = iRow("WORKKBN")
        P_STDATE.Value = iRow("STDATE")
        P_STTIME.Value = iRow("STTIME")
        P_ENDDATE.Value = iRow("ENDDATE")
        P_ENDTIME.Value = iRow("ENDTIME")
        P_WORKTIME.Value = iRow("WORKTIME")
        P_MOVETIME.Value = iRow("MOVETIME")
        P_ACTTIME.Value = iRow("ACTTIME")
        P_BINDSTDATE.Value = iRow("BINDSTDATE")
        P_BINDTIME.Value = iRow("BINDTIME")
        P_NIPPOBREAKTIME.Value = iRow("NIPPOBREAKTIME")
        P_BREAKTIME.Value = iRow("BREAKTIME")
        P_BREAKTIMECHO.Value = iRow("BREAKTIMECHO")
        P_NIGHTTIME.Value = iRow("NIGHTTIME")
        P_NIGHTTIMECHO.Value = iRow("NIGHTTIMECHO")
        P_ORVERTIME.Value = iRow("ORVERTIME")
        P_ORVERTIMECHO.Value = iRow("ORVERTIMECHO")
        P_WNIGHTTIME.Value = iRow("WNIGHTTIME")
        P_WNIGHTTIMECHO.Value = iRow("WNIGHTTIMECHO")
        P_SWORKTIME.Value = iRow("SWORKTIME")
        P_SWORKTIMECHO.Value = iRow("SWORKTIMECHO")
        P_SNIGHTTIME.Value = iRow("SNIGHTTIME")
        P_SNIGHTTIMECHO.Value = iRow("SNIGHTTIMECHO")
        P_HWORKTIME.Value = iRow("HWORKTIME")
        P_HWORKTIMECHO.Value = iRow("HWORKTIMECHO")
        P_HNIGHTTIME.Value = iRow("HNIGHTTIME")
        P_HNIGHTTIMECHO.Value = iRow("HNIGHTTIMECHO")
        P_WORKNISSU.Value = iRow("WORKNISSU")
        P_WORKNISSUCHO.Value = iRow("WORKNISSUCHO")
        P_SHOUKETUNISSU.Value = iRow("SHOUKETUNISSU")
        P_SHOUKETUNISSUCHO.Value = iRow("SHOUKETUNISSUCHO")
        P_KUMIKETUNISSU.Value = iRow("KUMIKETUNISSU")
        P_KUMIKETUNISSUCHO.Value = iRow("KUMIKETUNISSUCHO")
        P_ETCKETUNISSU.Value = iRow("ETCKETUNISSU")
        P_ETCKETUNISSUCHO.Value = iRow("ETCKETUNISSUCHO")
        P_NENKYUNISSU.Value = iRow("NENKYUNISSU")
        P_NENKYUNISSUCHO.Value = iRow("NENKYUNISSUCHO")
        P_TOKUKYUNISSU.Value = iRow("TOKUKYUNISSU")
        P_TOKUKYUNISSUCHO.Value = iRow("TOKUKYUNISSUCHO")
        P_CHIKOKSOTAINISSU.Value = iRow("CHIKOKSOTAINISSU")
        P_CHIKOKSOTAINISSUCHO.Value = iRow("CHIKOKSOTAINISSUCHO")
        P_STOCKNISSU.Value = iRow("STOCKNISSU")
        P_STOCKNISSUCHO.Value = iRow("STOCKNISSUCHO")
        P_KYOTEIWEEKNISSU.Value = iRow("KYOTEIWEEKNISSU")
        P_KYOTEIWEEKNISSUCHO.Value = iRow("KYOTEIWEEKNISSUCHO")
        P_WEEKNISSU.Value = iRow("WEEKNISSU")
        P_WEEKNISSUCHO.Value = iRow("WEEKNISSUCHO")
        P_DAIKYUNISSU.Value = iRow("DAIKYUNISSU")
        P_DAIKYUNISSUCHO.Value = iRow("DAIKYUNISSUCHO")
        P_NENSHINISSU.Value = iRow("NENSHINISSU")
        P_NENSHINISSUCHO.Value = iRow("NENSHINISSUCHO")
        P_SHUKCHOKNNISSU.Value = iRow("SHUKCHOKNNISSU")
        P_SHUKCHOKNNISSUCHO.Value = iRow("SHUKCHOKNNISSUCHO")
        P_SHUKCHOKNISSU.Value = iRow("SHUKCHOKNISSU")
        P_SHUKCHOKNISSUCHO.Value = iRow("SHUKCHOKNISSUCHO")
        P_SHUKCHOKNHLDNISSU.Value = iRow("SHUKCHOKNHLDNISSU")
        P_SHUKCHOKNHLDNISSUCHO.Value = iRow("SHUKCHOKNHLDNISSUCHO")
        P_SHUKCHOKHLDNISSU.Value = iRow("SHUKCHOKHLDNISSU")
        P_SHUKCHOKHLDNISSUCHO.Value = iRow("SHUKCHOKHLDNISSUCHO")
        P_TOKSAAKAISU.Value = iRow("TOKSAAKAISU")
        P_TOKSAAKAISUCHO.Value = iRow("TOKSAAKAISUCHO")
        P_TOKSABKAISU.Value = iRow("TOKSABKAISU")
        P_TOKSABKAISUCHO.Value = iRow("TOKSABKAISUCHO")
        P_TOKSACKAISU.Value = iRow("TOKSACKAISU")
        P_TOKSACKAISUCHO.Value = iRow("TOKSACKAISUCHO")
        P_TENKOKAISU.Value = iRow("TENKOKAISU")
        P_TENKOKAISUCHO.Value = iRow("TENKOKAISUCHO")
        P_HOANTIME.Value = iRow("HOANTIME")
        P_HOANTIMECHO.Value = iRow("HOANTIMECHO")
        P_KOATUTIME.Value = iRow("KOATUTIME")
        P_KOATUTIMECHO.Value = iRow("KOATUTIMECHO")
        P_TOKUSA1TIME.Value = iRow("TOKUSA1TIME")
        P_TOKUSA1TIMECHO.Value = iRow("TOKUSA1TIMECHO")
        P_HAYADETIME.Value = iRow("HAYADETIME")
        P_HAYADETIMECHO.Value = iRow("HAYADETIMECHO")
        P_PONPNISSU.Value = iRow("PONPNISSU")
        P_PONPNISSUCHO.Value = iRow("PONPNISSUCHO")
        P_BULKNISSU.Value = iRow("BULKNISSU")
        P_BULKNISSUCHO.Value = iRow("BULKNISSUCHO")
        P_TRAILERNISSU.Value = iRow("TRAILERNISSU")
        P_TRAILERNISSUCHO.Value = iRow("TRAILERNISSUCHO")
        P_BKINMUKAISU.Value = iRow("BKINMUKAISU")
        P_BKINMUKAISUCHO.Value = iRow("BKINMUKAISUCHO")
        P_SHARYOKBN.Value = iRow("SHARYOKBN")
        P_OILPAYKBN.Value = iRow("OILPAYKBN")
        P_UNLOADCNT.Value = iRow("UNLOADCNT")
        P_UNLOADCNTCHO.Value = iRow("UNLOADCNTCHO")
        P_HAIDISTANCE.Value = iRow("HAIDISTANCE")
        P_HAIDISTANCECHO.Value = iRow("HAIDISTANCECHO")
        P_KAIDISTANCE.Value = iRow("KAIDISTANCE")
        P_KAIDISTANCECHO.Value = iRow("KAIDISTANCECHO")
        P_ORVERTIMEADD.Value = 0
        P_WNIGHTTIMEADD.Value = 0
        P_SWORKTIMEADD.Value = 0
        P_SNIGHTTIMEADD.Value = 0
        P_YENDTIME.Value = "00:00"
        P_APPLYID.Value = ""
        P_RIYU.Value = ""
        P_RIYUETC.Value = ""

        P_HAISOTIME.Value = iRow("HAISOTIME")
        P_NENMATUNISSU.Value = iRow("NENMATUNISSU")
        P_NENMATUNISSUCHO.Value = iRow("NENMATUNISSUCHO")
        P_SHACHUHAKKBN.Value = iRow("SHACHUHAKKBN")
        P_SHACHUHAKNISSU.Value = iRow("SHACHUHAKNISSU")
        P_SHACHUHAKNISSUCHO.Value = iRow("SHACHUHAKNISSUCHO")
        P_MODELDISTANCE.Value = iRow("MODELDISTANCE")
        P_MODELDISTANCECHO.Value = iRow("MODELDISTANCECHO")
        P_JIKYUSHATIME.Value = iRow("JIKYUSHATIME")
        P_JIKYUSHATIMECHO.Value = iRow("JIKYUSHATIMECHO")

        P_HDAIWORKTIME.Value = iRow("HDAIWORKTIME")
        P_HDAIWORKTIMECHO.Value = iRow("HDAIWORKTIMECHO")
        P_HDAINIGHTTIME.Value = iRow("HDAINIGHTTIME")
        P_HDAINIGHTTIMECHO.Value = iRow("HDAINIGHTTIMECHO")
        P_SDAIWORKTIME.Value = iRow("SDAIWORKTIME")
        P_SDAIWORKTIMECHO.Value = iRow("SDAIWORKTIMECHO")
        P_SDAINIGHTTIME.Value = iRow("SDAINIGHTTIME")
        P_SDAINIGHTTIMECHO.Value = iRow("SDAINIGHTTIMECHO")
        P_WWORKTIME.Value = iRow("WWORKTIME")
        P_WWORKTIMECHO.Value = iRow("WWORKTIMECHO")
        P_JYOMUTIME.Value = iRow("JYOMUTIME")
        P_JYOMUTIMECHO.Value = iRow("JYOMUTIMECHO")
        P_HWORKNISSU.Value = iRow("HWORKNISSU")
        P_HWORKNISSUCHO.Value = iRow("HWORKNISSUCHO")
        P_KAITENCNT.Value = iRow("KAITENCNT")
        P_KAITENCNTCHO.Value = iRow("KAITENCNTCHO")

        P_SENJYOCNT.Value = iRow("SENJYOCNT")
        P_SENJYOCNTCHO.Value = iRow("SENJYOCNTCHO")
        P_UNLOADADDCNT1.Value = iRow("UNLOADADDCNT1")
        P_UNLOADADDCNT1CHO.Value = iRow("UNLOADADDCNT1CHO")
        P_UNLOADADDCNT2.Value = iRow("UNLOADADDCNT2")
        P_UNLOADADDCNT2CHO.Value = iRow("UNLOADADDCNT2CHO")
        P_UNLOADADDCNT3.Value = iRow("UNLOADADDCNT3")
        P_UNLOADADDCNT3CHO.Value = iRow("UNLOADADDCNT3CHO")
        P_UNLOADADDCNT4.Value = 0
        P_UNLOADADDCNT4CHO.Value = 0
        P_LOADINGCNT1.Value = iRow("LOADINGCNT1")
        P_LOADINGCNT1CHO.Value = iRow("LOADINGCNT1CHO")
        P_LOADINGCNT2.Value = 0
        P_LOADINGCNT2CHO.Value = 0
        P_SHORTDISTANCE1.Value = iRow("SHORTDISTANCE1")
        P_SHORTDISTANCE1CHO.Value = iRow("SHORTDISTANCE1CHO")
        P_SHORTDISTANCE2.Value = iRow("SHORTDISTANCE2")
        P_SHORTDISTANCE2CHO.Value = iRow("SHORTDISTANCE2CHO")

        P_DELFLG.Value = iRow("DELFLG")
        P_INITYMD.Value = iRow("INITYMD")
        P_UPDYMD.Value = iRow("UPDYMD")
        P_UPDUSER.Value = iRow("UPDUSER")
        P_UPDTERMID.Value = iRow("UPDTERMID")
        P_RECEIVEYMD.Value = iRow("RECEIVEYMD")

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

        'CLOSE
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub

    '★★★★★★★★★★★★★★★★★★★★★
    '共通処理部品
    '★★★★★★★★★★★★★★★★★★★★★

    ' ***  名称設定処理   LeftBoxより名称取得＆チェック
    Protected Sub CODENAME_get(ByVal I_FIELD As String,
                               ByRef I_VALUE As String,
                               ByRef O_TEXT As String,
                               ByRef O_RTN As String)

        '○名称取得

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_T7SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD

                Case "WORKKBN"          '作業区分名称 
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKKBN"
                    leftview.CodeToName(I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "STAFFCODE"        '乗務員名
                    prmData = work.getStaffCodeList(work.WF_T7SEL_CAMPCODE.Text, work.WF_T7SEL_TAISHOYM.Text, work.WF_T7SEL_HORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "CAMPCODE"         '会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "ORG"              '出荷部署名
                    prmData = work.CreateHORGParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "CREWKBN"          '乗務区分名
                    prmData = work.CreateCREWKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "WORKINGWEEK"      '曜日名
                    prmData = work.CreateWORKINGWEEKParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "RECODEKBN"        'レコード区分名
                    prmData = work.CreateRECODEKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "SHARYOKBN"        '車両区分名
                    prmData = work.CreateSHARYOKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "OILPAYKBN"        '油種給与区分
                    prmData = work.CreateOILPAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "STAFFKBN"         '職務区分
                    prmData = work.CreateStaffKbnParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "HOLIDAYKBN"       '休日区分
                    prmData = work.CreateHOLIDAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "PAYKBN"           '勤怠区分
                    prmData = work.CreatePAYKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "SHUKCHOKKBN"      '宿日直区分
                    prmData = work.CreateSHUKCHOKKBNParam(work.WF_T7SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    Protected Sub LIST_search(ByRef I_LISTBOX As ListBox, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        For i As Integer = 0 To I_LISTBOX.Items.Count - 1
            If I_LISTBOX.Items(i).Value = I_VALUE Then
                O_TEXT = I_LISTBOX.Items(i).Text
                O_RTN = "OK"
                Exit For
            End If
        Next

    End Sub

    ' ***  Datarowを項目毎にカンマ区切りの文字列に変換
    Protected Sub DatarowToCsv(ByVal iRow As DataRow, ByRef oCsv As String)
        Dim CSVstr As String = ""
        For i = 0 To iRow.ItemArray.Count - 1
            If i = 0 Then
                CSVstr = CSVstr & iRow.ItemArray(i).ToString
            Else
                CSVstr = CSVstr & ControlChars.Tab & iRow.ItemArray(i).ToString
            End If
        Next

        oCsv = CSVstr

    End Sub

    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If WF_RightViewChange.Value = Nothing Or WF_RightViewChange.Value = "" Then
        Else
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

    ' ***  EXCELファイルアップロード入力処理
    Protected Sub WF_FILEUPLOAD()

        '○ 初期処理
        Dim WW_ERR_REPORT As String = ""
        T0007COM.T0007tbl_ColumnsAdd(T0007tbl)

        '○ T0007tbl復元
        If Not Master.RecoverTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '○UPLOAD_XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text      '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If CS0023XLSUPLOAD.ERR = C_MESSAGE_NO.NORMAL Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
            Exit Sub
        End If

        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Rows.Count - 1
            CS0023XLSTBLrow.ItemArray = CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSTBLrow.Item(j)) Or IsNothing(CS0023XLSTBLrow.Item(j)) Then
                    CS0023XLSTBLrow.Item(j) = ""
                End If
            Next
            CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '〇日別明細or月合計要求判定　…　Excel定義に月合計項目が有効ならば、月合計判定("ON")　★★★★★　　追加　　★★★★★
        Dim wTTLFLG As String = ""      '月合計判定
        Excelhantei(CS0023XLSUPLOAD.REPORTID, wTTLFLG)
        If wTTLFLG = "ERR" Then
            Master.output(C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○T0007INPtblカラム設定
        T0007COM.T0007tbl_ColumnsAdd(T0007INPtbl)

        '○インポートレコードの列情報有り無し判定(インポートレイアウトがテーブル項目を含むか判定)
        T0007INPmake(CS0023XLSUPLOAD.TBLDATA, wTTLFLG)

        '○INPデータチェック
        Dim WW_ERR As String = C_MESSAGE_NO.NORMAL

        Dim WW_LINECNT As Integer = 0
        Dim WW_MAXERR As String = C_MESSAGE_NO.NORMAL
        For i As Integer = 0 To T0007INPtbl.Rows.Count - 1
            T0007INProw = T0007INPtbl.Rows(i)
            T0007INProw_CHEK(WW_ERRCODE)
            If WW_ERRCODE = "10023" Then
                T0007INProw("OPERATION") = "エラー"
                T0007INProw("SELECT") = "0"
                WW_MAXERR = WW_ERRCODE
            End If
            If WW_ERRCODE = "10018" Then
                T0007INProw("OPERATION") = "エラー"
                T0007INProw("SELECT") = "0"
                WW_MAXERR = "10023"
            End If
            If WW_ERRCODE = C_MESSAGE_NO.NORMAL Then
                T0007INProw("OPERATION") = "更新"
                T0007INProw("SELECT") = "1"
            End If
        Next

        '■■■ Excelデータ毎にチェック＆更新 ■■■

        '重大エラーの場合、INPUTから削除
        For i As Integer = T0007INPtbl.Rows.Count - 1 To 0 Step -1
            If T0007INPtbl.Rows(i)("SELECT") = "0" Then
                T0007INPtbl.Rows(i).Delete()
            End If
        Next

        '画面表示の従業員のみを抽出
        Dim WW_Cols As String() = {"STAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_TBLview As DataView

        WW_TBLview = New DataView(T0007tbl)
        WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)

        Dim WW_FIND As String = "OFF"
        For i As Integer = T0007INPtbl.Rows.Count - 1 To 0 Step -1
            WW_FIND = "OFF"
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                If WW_KEYrow("STAFFCODE") = T0007INPtbl.Rows(i)("STAFFCODE") Then
                    WW_FIND = "ON"
                    Exit For
                End If
            Next
            If WW_FIND = "OFF" Then
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・更新できないレコード(従業員エラー)です。"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 画面選択されていない従業員です。 ,"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 勤務年月日  =" & T0007INPtbl.Rows(i)("WORKDATE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員コード=" & T0007INPtbl.Rows(i)("STAFFCODE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員      =" & T0007INPtbl.Rows(i)("STAFFNAMES") & " , "
                WW_ERR_REPORT = WW_ERR_REPORT & ControlChars.NewLine & WW_ERR_MES
                WW_MAXERR = "10023"

                rightview.addErrorReport(WW_ERR_REPORT)

                T0007INPtbl.Rows(i).Delete()
            End If
        Next
        WW_KEYtbl.Dispose()
        WW_KEYtbl = Nothing
        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        Dim WW_T0007tbl As DataTable = T0007INPtbl.Clone
        Dim WW_T0007row As DataRow
        Dim WW_BREAKTIME As Integer = 0
        Dim WW_SEQ As Integer = 0
        Dim WW_WORKTIME As Integer = 0
        Dim WW_DATE_SV As String = ""
        Dim WW_TIME_SV As String = ""
        Dim WW_TIME As String = ""
        Dim WW_date As DateTime = Nothing
        For Each WW_HEADrow As DataRow In T0007INPtbl.Rows

            If WW_HEADrow("RECODEKBN") = "0" And WW_HEADrow("HDKBN") = "H" And WW_HEADrow("OPERATION") = "更新" Then
                Dim WW_NIPPONO As String = ""
                Dim WW_F1CNT As Integer = 0

                WW_HEADrow("SEQ") = "0"

                WW_BREAKTIME = 0
                WW_SEQ = 0
                '日報取得
                Dim T0005tbl As DataTable = New DataTable
                T00005ALLget(WW_HEADrow("STAFFCODE"), WW_HEADrow("WORKDATE"), WW_HEADrow("WORKDATE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Exit Sub
                End If

                Dim WW_WORKKBN As String = ""
                Dim WW_KAISO As String = ""
                Dim WW_SUISOKBN As String = ""
                Dim WW_B3CNT As Integer = 0
                Dim WW_DISTANCE As Double = 0
                For i As Integer = 0 To T0005tbl.Rows.Count - 1
                    Dim WW_NIPPOrow As DataRow = T0005tbl.Rows(i)

                    WW_HEADrow("NIPPOLINKCODE") = WW_NIPPOrow("UPDYMD")

                    '休憩の合計
                    If WW_NIPPOrow("WORKKBN") = "BB" Then
                        WW_BREAKTIME = WW_BREAKTIME + WW_NIPPOrow("WORKTIME")
                    End If

                    If WW_NIPPOrow("WORKKBN") = "A1" Then
                        '--------------------------------------------------------------------------------
                        '始業レコード作成
                        '--------------------------------------------------------------------------------
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WW_HEADrow("STDATE")
                        WW_T0007row("STTIME") = WW_HEADrow("STTIME")
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WW_HEADrow("STDATE")
                        WW_T0007row("ENDTIME") = WW_HEADrow("STTIME")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "A1"
                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                        WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                        If WW_T0007row("ORGSEQ").ToString = "" Then
                            WW_T0007row("ORGSEQ") = 0
                        End If

                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                        WW_WORKKBN = "A1"
                        Continue For
                    End If

                    If WW_NIPPOrow("WORKKBN") = "F1" Then
                        WW_F1CNT += 1
                        '直前がA1（出社の場合）
                        If WW_WORKKBN = "A1" Then
                            If WW_NIPPOrow("STDATE") = WW_DATE_SV And
                               WW_NIPPOrow("STTIME") = WW_TIME_SV Then
                            Else
                                '--------------------------------------------------------------------------------
                                '他作業レコード作成
                                '--------------------------------------------------------------------------------
                                WW_T0007row = WW_T0007tbl.NewRow
                                T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                                '開始日時、前のレコードの終了日時
                                WW_T0007row("STDATE") = WW_DATE_SV
                                WW_T0007row("STTIME") = WW_TIME_SV
                                '終了日時、後ろレコードの開始日時
                                WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                                WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                                'その他の項目は、現在のレコードをコピーする
                                WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                                WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                                WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                                WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                                WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                                WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                                WW_T0007row("MORG") = WW_HEADrow("MORG")
                                WW_T0007row("HORG") = WW_HEADrow("HORG")
                                WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                                WW_SEQ += 1
                                WW_T0007row("SEQ") = WW_SEQ
                                WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                                WW_T0007row("HIDDEN") = "1"
                                WW_T0007row("HDKBN") = "D"
                                WW_T0007row("DATAKBN") = "K"
                                WW_T0007row("RECODEKBN") = "0"
                                WW_T0007row("WORKKBN") = "BX"

                                '作業時間
                                WW_WORKTIME = DateDiff("n",
                                                      WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                                      WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                                     )
                                WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                                WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                                WW_T0007row("CAMPNAMES") = ""
                                CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                                WW_T0007row("WORKKBNNAMES") = ""
                                CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                                WW_T0007row("STAFFNAMES") = ""
                                CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                                WW_T0007row("HOLIDAYKBNNAMES") = ""
                                CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                                WW_T0007row("PAYKBNNAMES") = ""
                                CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                                WW_T0007row("SHUKCHOKKBNNAMES") = ""
                                CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                                WW_T0007row("MORGNAMES") = ""
                                CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                                WW_T0007row("HORGNAMES") = ""
                                CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                                WW_T0007row("SORGNAMES") = ""
                                CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                                WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                                If WW_T0007row("ORGSEQ").ToString = "" Then
                                    WW_T0007row("ORGSEQ") = 0
                                End If

                                WW_T0007tbl.Rows.Add(WW_T0007row)
                            End If
                            Continue For
                        End If
                    End If

                    If WW_NIPPOrow("WORKKBN") = "F3" Then
                        WW_NIPPONO = WW_NIPPOrow("NIPPONO")
                        WW_DATE_SV = WW_NIPPOrow("ENDDATE")
                        WW_TIME_SV = WW_NIPPOrow("ENDTIME")
                        WW_DISTANCE = WW_NIPPOrow("SOUDISTANCE")
                        WW_SUISOKBN = WW_NIPPOrow("SUISOKBN")
                        WW_KAISO = WW_NIPPOrow("L1KAISO")

                        Continue For
                    End If

                    If WW_NIPPOrow("WORKKBN") = "B3" Then
                        If WW_NIPPOrow("SUISOKBN") <> "1" Then
                            '荷卸（B3）をカウントする（水素はカウントしない）
                            WW_B3CNT += 1
                        End If
                    End If

                    WW_WORKKBN = ""
                Next
                '最終レコードの追加
                If T0005tbl.Rows.Count > 0 Then

                    Dim WW_BREAK_FLG As String = "OFF"
                    If T0007COM.HHMMtoMinutes(WW_HEADrow("BREAKTIME")) > 0 Then
                        WW_BREAK_FLG = "ON"
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                        WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                        '終了日時、後ろレコードの開始日時
                        WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                        WW_T0007row("ENDDATE") = WW_date.AddMinutes(T0007COM.HHMMtoMinutes(WW_HEADrow("BREAKTIME"))).ToString("yyyy/MM/dd")
                        WW_T0007row("ENDTIME") = WW_date.AddMinutes(T0007COM.HHMMtoMinutes(WW_HEADrow("BREAKTIME"))).ToString("HH:mm")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BB"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("BREAKTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                        WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                        If WW_T0007row("ORGSEQ").ToString = "" Then
                            WW_T0007row("ORGSEQ") = 0
                        End If

                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                    End If


                    '--------------------------------------------------------------------------------
                    '他作業（＋１０分）レコード作成（最後のデータ）
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                    If WW_BREAK_FLG = "OFF" Then
                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                        WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                    Else
                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WW_DATE_SV
                        WW_T0007row("STTIME") = WW_TIME_SV
                    End If
                    '拘束時間（＋１０分）
                    WW_T0007row("ENDDATE") = WW_date.AddMinutes(10).ToString("yyyy/MM/dd")
                    WW_T0007row("ENDTIME") = WW_date.AddMinutes(10).ToString("HH:mm")

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "BX"
                    WW_T0007row("DELFLG") = "0"

                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                    WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                    If WW_T0007row("ORGSEQ").ToString = "" Then
                        WW_T0007row("ORGSEQ") = 0
                    End If

                    WW_T0007tbl.Rows.Add(WW_T0007row)

                    WW_DATE_SV = WW_T0007row("ENDDATE")
                    WW_TIME_SV = WW_T0007row("ENDTIME")
                    '--------------------------------------------------------------------------------
                    '他作業（＋？？分）レコード作成（退社時間との差）
                    '--------------------------------------------------------------------------------
                    If CDate(WW_DATE_SV & " " & WW_TIME_SV) < CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME")) Then
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WW_DATE_SV
                        WW_T0007row("STTIME") = WW_TIME_SV
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WW_HEADrow("ENDDATE")
                        WW_T0007row("ENDTIME") = WW_HEADrow("ENDTIME")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BX"
                        WW_T0007row("DELFLG") = "0"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                        WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                        If WW_T0007row("ORGSEQ").ToString = "" Then
                            WW_T0007row("ORGSEQ") = 0
                        End If

                        WW_T0007tbl.Rows.Add(WW_T0007row)

                        WW_DATE_SV = WW_T0007row("ENDDATE")
                        WW_TIME_SV = WW_T0007row("ENDTIME")
                    End If
                    '--------------------------------------------------------------------------------
                    '終業レコード作成（最後のデータ）
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = WW_DATE_SV
                    WW_T0007row("STTIME") = WW_TIME_SV
                    '終了日時、後ろレコードの開始日時
                    WW_T0007row("ENDDATE") = WW_DATE_SV
                    WW_T0007row("ENDTIME") = WW_TIME_SV

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "Z1"
                    WW_T0007row("DELFLG") = "0"

                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                    WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                    If WW_T0007row("ORGSEQ").ToString = "" Then
                        WW_T0007row("ORGSEQ") = 0
                    End If

                    WW_T0007tbl.Rows.Add(WW_T0007row)
                End If

                WW_WORKTIME = DateDiff("n",
                                    WW_HEADrow("STDATE") + " " + WW_HEADrow("STTIME"),
                                    WW_HEADrow("ENDDATE") + " " + WW_HEADrow("ENDTIME")
                                   )
                WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                WW_HEADrow("NIPPOBREAKTIME") = T0007COM.formatHHMM(WW_BREAKTIME)
                WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_BREAKTIME + T0007COM.HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                WW_HEADrow("UNLOADCNT") = WW_B3CNT
                WW_HEADrow("UNLOADCNTTTL") = WW_B3CNT
                If WW_KAISO = "回送" And WW_SUISOKBN <> "1" Then
                    WW_HEADrow("HAIDISTANCE") = "0.00"
                    WW_HEADrow("HAIDISTANCETTL") = "0.00"
                    WW_HEADrow("KAIDISTANCE") = WW_DISTANCE
                    WW_HEADrow("KAIDISTANCETTL") = WW_DISTANCE + WW_HEADrow("KAIDISTANCECHO")
                Else
                    WW_HEADrow("HAIDISTANCE") = WW_DISTANCE
                    WW_HEADrow("HAIDISTANCETTL") = WW_DISTANCE + WW_HEADrow("HAIDISTANCECHO")
                    WW_HEADrow("KAIDISTANCE") = "0.00"
                    WW_HEADrow("KAIDISTANCETTL") = "0.00"
                End If

                Dim iT0005view As DataView
                iT0005view = New DataView(T0005tbl)
                iT0005view.Sort = "YMD, STAFFCODE, WORKKBN"
                NIPPOget_T7Format("NEW", WW_T0007tbl, iT0005view)

            End If

            If WW_HEADrow("RECODEKBN") = "2" And WW_HEADrow("OPERATION") = "更新" Then
                '月初日をセット
                Dim dt As Date = CDate(WW_HEADrow("TAISHOYM") & "/01")
                '月末を算出
                WW_HEADrow("WORKDATE") = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")

                Dim WW_UPD_FLG As String = "OFF"
                Dim wUNLOADCNT As Integer = 0
                Dim wUNLOADCNTCHO As Integer = 0
                Dim wHAIDISTANCE As Double = 0
                Dim wHAIDISTANCECHO As Double = 0

                For Each T0007row As DataRow In T0007tbl.Rows
                    If T0007row("WORKDATE") = WW_HEADrow("WORKDATE") And
                       T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                       T0007row("RECODEKBN") = WW_HEADrow("RECODEKBN") And
                       T0007row("SELECT") = "1" Then

                        If T0007row("HDKBN") = "H" Then
                            WW_T0007row = WW_T0007tbl.NewRow
                            WW_T0007row.ItemArray = T0007row.ItemArray
                            WW_T0007row("OPERATION") = "更新"
                            WW_T0007row("TIMSTP") = 0
                            WW_T0007row("WORKNISSUCHO") = Val(WW_HEADrow("WORKNISSUTTL")) - T0007row("WORKNISSU")
                            WW_T0007row("WORKNISSUTTL") = Val(T0007row("WORKNISSU")) + Val(T0007row("WORKNISSUCHO"))
                            WW_T0007row("SHOUKETUNISSUCHO") = Val(WW_HEADrow("SHOUKETUNISSUTTL")) - T0007row("SHOUKETUNISSU")
                            WW_T0007row("SHOUKETUNISSUTTL") = Val(T0007row("SHOUKETUNISSU")) + Val(T0007row("SHOUKETUNISSUCHO"))
                            WW_T0007row("KUMIKETUNISSUCHO") = Val(WW_HEADrow("KUMIKETUNISSUTTL")) - T0007row("KUMIKETUNISSU")
                            WW_T0007row("KUMIKETUNISSUTTL") = Val(T0007row("KUMIKETUNISSU")) + Val(T0007row("KUMIKETUNISSUCHO"))
                            WW_T0007row("ETCKETUNISSUCHO") = Val(WW_HEADrow("ETCKETUNISSUTTL")) - T0007row("ETCKETUNISSU")
                            WW_T0007row("ETCKETUNISSUTTL") = Val(T0007row("ETCKETUNISSU")) + Val(T0007row("ETCKETUNISSUCHO"))
                            WW_T0007row("NENKYUNISSUCHO") = Val(WW_HEADrow("NENKYUNISSUTTL")) - T0007row("NENKYUNISSU")
                            WW_T0007row("NENKYUNISSUTTL") = Val(T0007row("NENKYUNISSU")) + Val(T0007row("NENKYUNISSUCHO"))
                            WW_T0007row("TOKUKYUNISSUCHO") = Val(WW_HEADrow("TOKUKYUNISSUTTL")) - T0007row("TOKUKYUNISSU")
                            WW_T0007row("TOKUKYUNISSUTTL") = Val(T0007row("TOKUKYUNISSU")) + Val(T0007row("TOKUKYUNISSUCHO"))
                            WW_T0007row("CHIKOKSOTAINISSUCHO") = Val(WW_HEADrow("CHIKOKSOTAINISSUTTL")) - T0007row("CHIKOKSOTAINISSU")
                            WW_T0007row("CHIKOKSOTAINISSUTTL") = Val(T0007row("CHIKOKSOTAINISSU")) + Val(T0007row("CHIKOKSOTAINISSUCHO"))
                            WW_T0007row("STOCKNISSUCHO") = Val(WW_HEADrow("STOCKNISSUTTL")) - T0007row("STOCKNISSU")
                            WW_T0007row("STOCKNISSUTTL") = Val(T0007row("STOCKNISSU")) + Val(T0007row("STOCKNISSUCHO"))
                            WW_T0007row("KYOTEIWEEKNISSUCHO") = Val(WW_HEADrow("KYOTEIWEEKNISSUTTL")) - T0007row("KYOTEIWEEKNISSU")
                            WW_T0007row("KYOTEIWEEKNISSUTTL") = Val(T0007row("KYOTEIWEEKNISSU")) + Val(T0007row("KYOTEIWEEKNISSUCHO"))
                            WW_T0007row("WEEKNISSUCHO") = Val(WW_HEADrow("WEEKNISSUTTL")) - T0007row("WEEKNISSU")
                            WW_T0007row("WEEKNISSUTTL") = Val(T0007row("WEEKNISSU")) + Val(T0007row("WEEKNISSUCHO"))
                            WW_T0007row("DAIKYUNISSUCHO") = Val(WW_HEADrow("DAIKYUNISSUTTL")) - T0007row("DAIKYUNISSU")
                            WW_T0007row("DAIKYUNISSUTTL") = Val(T0007row("DAIKYUNISSU")) + Val(T0007row("DAIKYUNISSUCHO"))
                            WW_T0007row("NENSHINISSUCHO") = Val(WW_HEADrow("NENSHINISSUTTL")) - T0007row("NENSHINISSU")
                            WW_T0007row("NENSHINISSUTTL") = Val(T0007row("NENSHINISSU")) + Val(T0007row("NENSHINISSUCHO"))
                            WW_T0007row("SHUKCHOKNNISSUCHO") = Val(WW_HEADrow("SHUKCHOKNNISSUTTL")) - T0007row("SHUKCHOKNNISSU")
                            WW_T0007row("SHUKCHOKNNISSUTTL") = Val(T0007row("SHUKCHOKNNISSU")) + Val(T0007row("SHUKCHOKNNISSUCHO"))
                            WW_T0007row("SHUKCHOKNISSUCHO") = Val(WW_HEADrow("SHUKCHOKNISSUTTL")) - T0007row("SHUKCHOKNISSU")
                            WW_T0007row("SHUKCHOKNISSUTTL") = Val(T0007row("SHUKCHOKNISSU")) + Val(T0007row("SHUKCHOKNISSUCHO"))

                            WW_T0007row("SHUKCHOKNHLDNISSUCHO") = Val(WW_HEADrow("SHUKCHOKNHLDNISSUTTL")) - T0007row("SHUKCHOKNHLDNISSU")
                            WW_T0007row("SHUKCHOKNHLDNISSUTTL") = Val(T0007row("SHUKCHOKNHLDNISSU")) + Val(T0007row("SHUKCHOKNHLDNISSUCHO"))
                            WW_T0007row("SHUKCHOKHLDNISSUCHO") = Val(WW_HEADrow("SHUKCHOKHLDNISSUTTL")) - T0007row("SHUKCHOKHLDNISSU")
                            WW_T0007row("SHUKCHOKHLDNISSUTTL") = Val(T0007row("SHUKCHOKHLDNISSU")) + Val(T0007row("SHUKCHOKHLDNISSUCHO"))

                            WW_T0007row("TOKSAAKAISUCHO") = Val(WW_HEADrow("TOKSAAKAISUTTL")) - T0007row("TOKSAAKAISU")
                            WW_T0007row("TOKSAAKAISUTTL") = Val(T0007row("TOKSAAKAISU")) + Val(T0007row("TOKSAAKAISUCHO"))
                            WW_T0007row("TOKSABKAISUCHO") = Val(WW_HEADrow("TOKSABKAISUTTL")) - T0007row("TOKSABKAISU")
                            WW_T0007row("TOKSABKAISUTTL") = Val(T0007row("TOKSABKAISU")) + Val(T0007row("TOKSABKAISUCHO"))
                            WW_T0007row("TOKSACKAISUCHO") = Val(WW_HEADrow("TOKSACKAISUTTL")) - T0007row("TOKSACKAISU")
                            WW_T0007row("TOKSACKAISUTTL") = Val(T0007row("TOKSACKAISU")) + Val(T0007row("TOKSACKAISUCHO"))
                            WW_T0007row("TENKOKAISUCHO") = Val(WW_HEADrow("TENKOKAISUTTL")) - T0007row("TENKOKAISU")
                            WW_T0007row("TENKOKAISUTTL") = Val(T0007row("TENKOKAISU")) + Val(T0007row("TENKOKAISUCHO"))

                            WW_T0007row("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("NIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("NIGHTTIME"))
                            WW_T0007row("NIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("NIGHTTIME")) + T0007COM.HHMMtoMinutes(T0007row("NIGHTTIMECHO"))
                            WW_T0007row("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("ORVERTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("ORVERTIME"))
                            WW_T0007row("ORVERTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("ORVERTIME")) + T0007COM.HHMMtoMinutes(T0007row("ORVERTIMECHO"))
                            WW_T0007row("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("WNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("WNIGHTTIME"))
                            WW_T0007row("WNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("WNIGHTTIME")) + T0007COM.HHMMtoMinutes(T0007row("WNIGHTTIMECHO"))
                            WW_T0007row("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("SWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("SWORKTIME"))
                            WW_T0007row("SWORKTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("SWORKTIME")) + T0007COM.HHMMtoMinutes(T0007row("SWORKTIMECHO"))
                            WW_T0007row("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("SNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("SNIGHTTIME"))
                            WW_T0007row("SNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("SNIGHTTIME")) + T0007COM.HHMMtoMinutes(T0007row("SNIGHTTIMECHO"))
                            WW_T0007row("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("HWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("HWORKTIME"))
                            WW_T0007row("HWORKTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("HWORKTIME")) + T0007COM.HHMMtoMinutes(T0007row("HWORKTIMECHO"))
                            WW_T0007row("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("HNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("HNIGHTTIME"))
                            WW_T0007row("HNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("HNIGHTTIME")) + T0007COM.HHMMtoMinutes(T0007row("HNIGHTTIMECHO"))

                            WW_T0007row("HOANTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("HOANTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("HOANTIME"))
                            WW_T0007row("HOANTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("HOANTIME")) + T0007COM.HHMMtoMinutes(T0007row("HOANTIMECHO"))
                            WW_T0007row("KOATUTIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("KOATUTIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("KOATUTIME"))
                            WW_T0007row("KOATUTIMETTL") = T0007COM.HHMMtoMinutes(T0007row("KOATUTIME")) + T0007COM.HHMMtoMinutes(T0007row("KOATUTIMECHO"))
                            WW_T0007row("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("TOKUSA1TIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("TOKUSA1TIME"))
                            WW_T0007row("TOKUSA1TIMETTL") = T0007COM.HHMMtoMinutes(T0007row("TOKUSA1TIME")) + T0007COM.HHMMtoMinutes(T0007row("TOKUSA1TIMECHO"))
                            WW_T0007row("HAYADETIMECHO") = T0007COM.HHMMtoMinutes(WW_HEADrow("HAYADETIMETTL")) - T0007COM.HHMMtoMinutes(T0007row("HAYADETIME"))
                            WW_T0007row("HAYADETIMETTL") = T0007COM.HHMMtoMinutes(T0007row("HAYADETIME")) + T0007COM.HHMMtoMinutes(T0007row("HAYADETIMECHO"))
                            WW_T0007row("PONPNISSUCHO") = Val(WW_HEADrow("PONPNISSUTTL")) - Val(T0007row("PONPNISSU"))
                            WW_T0007row("PONPNISSUTTL") = Val(T0007row("PONPNISSU")) + Val(T0007row("PONPNISSUCHO"))
                            WW_T0007row("BULKNISSUCHO") = Val(WW_HEADrow("BULKNISSUTTL")) - T0007row("BULKNISSU")
                            WW_T0007row("BULKNISSUTTL") = Val(T0007row("BULKNISSU")) + Val(T0007row("BULKNISSUCHO"))
                            WW_T0007row("TRAILERNISSUCHO") = Val(WW_HEADrow("TRAILERNISSUTTL")) - T0007row("TRAILERNISSU")
                            WW_T0007row("TRAILERNISSUTTL") = Val(T0007row("TRAILERNISSU")) + Val(T0007row("TRAILERNISSUCHO"))
                            WW_T0007row("BKINMUKAISUCHO") = Val(WW_HEADrow("BKINMUKAISUTTL")) - T0007row("BKINMUKAISU")
                            WW_T0007row("BKINMUKAISUTTL") = Val(T0007row("BKINMUKAISU")) + Val(T0007row("BKINMUKAISUCHO"))

                            WW_T0007row("NIGHTTIMECHO") = T0007COM.formatHHMM(WW_T0007row("NIGHTTIMECHO"))
                            WW_T0007row("NIGHTTIMETTL") = T0007COM.formatHHMM(WW_T0007row("NIGHTTIMETTL"))
                            WW_T0007row("ORVERTIMECHO") = T0007COM.formatHHMM(WW_T0007row("ORVERTIMECHO"))
                            WW_T0007row("ORVERTIMETTL") = T0007COM.formatHHMM(WW_T0007row("ORVERTIMETTL"))
                            WW_T0007row("WNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T0007row("WNIGHTTIMECHO"))
                            WW_T0007row("WNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T0007row("WNIGHTTIMETTL"))
                            WW_T0007row("SWORKTIMECHO") = T0007COM.formatHHMM(WW_T0007row("SWORKTIMECHO"))
                            WW_T0007row("SWORKTIMETTL") = T0007COM.formatHHMM(WW_T0007row("SWORKTIMETTL"))
                            WW_T0007row("SNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T0007row("SNIGHTTIMECHO"))
                            WW_T0007row("SNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T0007row("SNIGHTTIMETTL"))
                            WW_T0007row("HWORKTIMECHO") = T0007COM.formatHHMM(WW_T0007row("HWORKTIMECHO"))
                            WW_T0007row("HWORKTIMETTL") = T0007COM.formatHHMM(WW_T0007row("HWORKTIMETTL"))
                            WW_T0007row("HNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T0007row("HNIGHTTIMECHO"))
                            WW_T0007row("HNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T0007row("HNIGHTTIMETTL"))
                            WW_T0007row("HOANTIMECHO") = T0007COM.formatHHMM(WW_T0007row("HOANTIMECHO"))
                            WW_T0007row("HOANTIMETTL") = T0007COM.formatHHMM(WW_T0007row("HOANTIMETTL"))
                            WW_T0007row("KOATUTIMECHO") = T0007COM.formatHHMM(WW_T0007row("KOATUTIMECHO"))
                            WW_T0007row("KOATUTIMETTL") = T0007COM.formatHHMM(WW_T0007row("KOATUTIMETTL"))
                            WW_T0007row("TOKUSA1TIMECHO") = T0007COM.formatHHMM(WW_T0007row("TOKUSA1TIMECHO"))
                            WW_T0007row("TOKUSA1TIMETTL") = T0007COM.formatHHMM(WW_T0007row("TOKUSA1TIMETTL"))
                            WW_T0007row("HAYADETIMECHO") = T0007COM.formatHHMM(WW_T0007row("HAYADETIMECHO"))
                            WW_T0007row("HAYADETIMETTL") = T0007COM.formatHHMM(WW_T0007row("HAYADETIMETTL"))
                            CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                            WW_T0007row("STAFFKBNNAMES") = ""
                            CODENAME_get("STAFFKBN", WW_T0007row("STAFFKBN"), WW_T0007row("STAFFKBNNAMES"), WW_DUMMY)
                            WW_T0007row("MORGNAMES") = ""
                            CODENAME_get("ORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                            WW_T0007row("HORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                            WW_T0007row("HOLIDAYKBNNAMES") = ""
                            CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("PAYKBNNAMES") = ""
                            CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("SHUKCHOKKBNNAMES") = ""
                            CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)

                            WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                            If WW_T0007row("ORGSEQ").ToString = "" Then
                                WW_T0007row("ORGSEQ") = 0
                            End If

                            WW_T0007tbl.Rows.Add(WW_T0007row)
                        End If

                        If T0007row("HDKBN") = "D" Then

                            WW_T0007row = WW_T0007tbl.NewRow
                            WW_T0007row.ItemArray = T0007row.ItemArray
                            WW_T0007row("TIMSTP") = 0

                            '車両区分（SHARYOKBN 1:単車、2:トレーラ）
                            '給与油種区分（OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素
                            'UNLOADCNTTTL0101～UNLOADCNTTTL0110変数名を動的に作成
                            'UNLOADCNTTTL0201～UNLOADCNTTTL0210変数名を動的に作成
                            'HAIDISTANCETTL0101～HAIDISTANCETTL0110変数名を動的に作成
                            'HAIDISTANCETTL0201～HAIDISTANCETTL0210変数名を動的に作成
                            Dim WW_SHARYOKBN As String = Val(T0007row("SHARYOKBN")).ToString("00")
                            Dim WW_OILPAYKBN As String = T0007row("OILPAYKBN")
                            If (WW_SHARYOKBN = "01" OrElse WW_SHARYOKBN = "02") AndAlso
                                WW_OILPAYKBN >= "01" AndAlso WW_OILPAYKBN <= "10" Then
                                Dim WW_UNLOADCNT As String = "UNLOADCNTTTL" & CInt(T0007row("SHARYOKBN")).ToString("00") & T0007row("OILPAYKBN")
                                Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & CInt(T0007row("SHARYOKBN")).ToString("00") & T0007row("OILPAYKBN")
                                If WW_HEADrow(WW_UNLOADCNT) <> T0007row("UNLOADCNTTTL") OrElse
                                   WW_HEADrow(WW_HAIDISTANCETTL) <> T0007row("HAIDISTANCETTL") Then
                                    WW_T0007row("UNLOADCNTCHO") = Val(WW_HEADrow(WW_UNLOADCNT)) - T0007row("UNLOADCNT")
                                    WW_T0007row("HAIDISTANCECHO") = Val(WW_HEADrow(WW_HAIDISTANCETTL)) - T0007row("HAIDISTANCE")
                                End If
                            End If

                            WW_T0007row("OPERATION") = "更新"
                            WW_T0007row("UNLOADCNTTTL") = Val(WW_T0007row("UNLOADCNT")) + Val(WW_T0007row("UNLOADCNTCHO"))
                            WW_T0007row("HAIDISTANCETTL") = Val(WW_T0007row("HAIDISTANCE")) + Val(WW_T0007row("HAIDISTANCECHO"))

                            WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                            If WW_T0007row("ORGSEQ").ToString = "" Then
                                WW_T0007row("ORGSEQ") = 0
                            End If

                            WW_T0007tbl.Rows.Add(WW_T0007row)

                            wUNLOADCNT += WW_T0007row("UNLOADCNT")
                            wUNLOADCNTCHO += WW_T0007row("UNLOADCNTCHO")
                            wHAIDISTANCE += WW_T0007row("HAIDISTANCE")
                            wHAIDISTANCECHO += WW_T0007row("HAIDISTANCECHO")
                            WW_UPD_FLG = "ON"
                        End If
                    End If
                Next
                If WW_UPD_FLG = "ON" Then
                    For Each T0007WKrow As DataRow In WW_T0007tbl.Rows
                        If T0007WKrow("WORKDATE") = WW_HEADrow("WORKDATE") And
                           T0007WKrow("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                           T0007WKrow("RECODEKBN") = "2" And T0007WKrow("HDKBN") = "H" Then
                            T0007WKrow("UNLOADCNT") = wUNLOADCNT
                            T0007WKrow("UNLOADCNTCHO") = wUNLOADCNTCHO
                            T0007WKrow("UNLOADCNTTTL") = wUNLOADCNT + wUNLOADCNTCHO
                            T0007WKrow("HAIDISTANCE") = wHAIDISTANCE
                            T0007WKrow("HAIDISTANCECHO") = wHAIDISTANCECHO
                            T0007WKrow("HAIDISTANCETTL") = wHAIDISTANCE + wHAIDISTANCECHO
                        End If
                    Next
                End If

            End If
        Next

        '日別は、日報をマージし残業計算を行う
        If wTTLFLG = "日別" Then
            T0007INPtbl.Merge(WW_T0007tbl)
            '正常データのみ抽出し、残業計算を行う

            '翌月１日のデータを抽出し、IMPtblとマージし残業計算に渡す
            Dim WW_Filter As String = ""
            Dim WW_T0007AFTtbl As DataTable = T0007tbl.Clone
            Dim dtAft As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
            WW_Filter = "HDKBN = 'H' and RECODEKBN = '0' and WORKDATE = #" & dtAft.AddMonths(1).ToString("yyyy/MM/dd") & "#"
            CS0026TblSort.TABLE = T0007tbl
            CS0026TblSort.FILTER = WW_Filter
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007AFTtbl = CS0026TblSort.sort()

            WW_T0007AFTtbl.Merge(T0007INPtbl)

            T0007COM.T0007_KintaiCalc_JKT(T0007INPtbl, WW_T0007AFTtbl)

            WW_T0007AFTtbl.Dispose()
            WW_T0007AFTtbl = Nothing

            For Each WW_T7row As DataRow In T0007INPtbl.Rows
                WW_T7row("TIMSTP") = "0"
                If WW_T7row("HDKBN") = "D" Then
                    WW_T7row("OPERATION") = "更新"
                End If
            Next

        End If

        CS0026TblSort.TABLE = T0007INPtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007INPtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        T0007tbl = CS0026TblSort.sort()

        Dim WW_IDX As Integer = 0
        Dim WW_KEY_INP As String = ""
        Dim WW_KEY_TBL As String = ""
        For Each T0007INProw As DataRow In T0007INPtbl.Rows
            WW_KEY_INP = T0007INProw("STAFFCODE") & T0007INProw("WORKDATE") & T0007INProw("RECODEKBN")

            If T0007INProw("OPERATION") = "更新" And T0007INProw("HDKBN") = "H" Then

                For i As Integer = WW_IDX To T0007tbl.Rows.Count - 1
                    Dim T0007row As DataRow = T0007tbl.Rows(i)
                    WW_KEY_TBL = T0007row("STAFFCODE") & T0007row("WORKDATE") & T0007row("RECODEKBN")
                    If WW_KEY_TBL < WW_KEY_INP Then
                        Continue For
                    End If

                    If WW_KEY_TBL = WW_KEY_INP Then
                        T0007row("OPERATION") = T0007INProw("OPERATION")
                        T0007row("SELECT") = "0"
                        T0007row("HIDDEN") = "1" '非表示
                        T0007row("DELFLG") = "1"
                    End If

                    If WW_KEY_TBL > WW_KEY_INP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next
        '当画面で生成したデータ（タイムスタンプ＝0）に対する変更は、変更前を物理削除する　
        For i As Integer = T0007tbl.Rows.Count - 1 To 0 Step -1
            Dim T0007row As DataRow = T0007tbl.Rows(i)
            If T0007row("TIMSTP") = "0" And
               T0007row("SELECT") = "0" Then
                T0007row.Delete()
            End If
        Next

        '--------------------------------------------
        '合計明細編集
        '--------------------------------------------
        If wTTLFLG = "月合計" Then
            T0007INPtbl = WW_T0007tbl.Copy
        End If


        T0007tbl.Merge(T0007INPtbl)

        '----------------------------------------------
        '合計レコード編集
        '----------------------------------------------
        If wTTLFLG = "日別" Then
            T0007_TTLEdit(T0007tbl)
            T0007COM.T0007_TotalRecodeCreate(T0007tbl)
        Else
            T0007COM.T0007_TotalRecodeEdit(T0007tbl)
        End If

        '----------------------------------------------
        '月調整レコード作成
        '----------------------------------------------
        T0007COM.T0007_ChoseiRecodeCreate(T0007tbl)

        'ソート
        CS0026TblSort.TABLE = T0007tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "ORGSEQ, STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To T0007tbl.Rows.Count - 1
            T0007row = T0007tbl.Rows(i)
            If T0007row("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text Then
                If T0007row("SELECT") = "1" Then
                    If T0007row("HDKBN") = "H" And T0007row("DELFLG") = "0" Then
                        T0007row("SELECT") = "1"
                        T0007row("HIDDEN") = "0"      '表示
                        WW_LINECNT += 1
                        T0007row("LINECNT") = WW_LINECNT
                    Else
                        T0007row("SELECT") = "1"
                        T0007row("HIDDEN") = "1"      '非表示
                        T0007row("LINECNT") = "0"
                    End If
                End If
            End If
        Next


        '■■■ GridView更新 ■■■
        '○GridViewデータをテーブルに保存
        If Not Master.SaveTable(T0007tbl, work.WF_T7I_XMLsaveF.Text) Then
            Exit Sub
        End If

        '絞込みボタン処理（GridViewの表示）を行う
        WF_ButtonExtract_Click()

        '■■■ 画面終了 ■■■
        '○メッセージ表示
        If WW_MAXERR = C_MESSAGE_NO.NORMAL Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_MAXERR, C_MESSAGE_TYPE.ERR)
        End If

        '○Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_WORKDATE"
        WF_WORKDATE.Focus()

    End Sub



    ' ******************************************************************************
    ' ***  日報を取得し作業区分（その他）レコード作成
    ' ***  ※１．始業、終業レコードを追加する
    ' ***  　２．日報が複数存在する場合（車両の乗り換）、乗り換の間にその他作業レコードを追加する
    ' ******************************************************************************
    Public Sub CreWORKKBN(ByRef ioTbl As DataTable, ByRef iT0005tbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String)
        Dim WW_WORKTIME As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_T0007tbl As DataTable = ioTbl.Clone
        Dim WW_T0007row As DataRow
        Dim WW_TIME As String = ""
        Dim WW_DATE_SV As String = ""
        Dim WW_TIME_SV As String = ""
        Dim WW_date As DateTime = Nothing

        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        ioTbl = CS0026TblSort.sort()

        Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
        Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
        Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
        For i As Integer = 0 To ioTbl.Rows.Count - 1
            Dim ioTblrow As DataRow = ioTbl.Rows(i)

            '削除レコードを取得
            If ioTblrow("SELECT") = "0" Then
                Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                DELrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DELtbl.Rows.Add(DELrow)
            End If

            '勤怠のヘッダレコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" Then
                Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                HEADrow.ItemArray = ioTblrow.ItemArray
                WW_T0007HEADtbl.Rows.Add(HEADrow)
            End If

            '勤怠の明細レコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                DTLrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DTLtbl.Rows.Add(DTLrow)
            End If
        Next



        '日報の変更を同一従業員の合計レコード（ヘッダ、明細）に反映
        '従業員+日付+レコード区分でソート
        CS0026TblSort.TABLE = WW_T0007HEADtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        WW_T0007HEADtbl = CS0026TblSort.sort()
        Dim wSTATUS As String = ""
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows

            If WW_HEADrow("RECODEKBN") = "2" Then
                WW_HEADrow("STATUS") = wSTATUS
                wSTATUS = ""
            Else
                If (WW_HEADrow("STATUS") Like "*日報取込*" And wSTATUS = "") Or (WW_HEADrow("STATUS") Like "*日報変更*") Then
                    wSTATUS = WW_HEADrow("STATUS")
                End If
            End If
        Next
        CS0026TblSort.TABLE = WW_T0007HEADtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl = CS0026TblSort.sort()

        '日報変更が発生した場合、作成済日報情報(DTL)を削除
        '　　（日報変更が発生したデータは始業（A1）、終業（Z1）、その他（BX）を再作成する。よって既存のデータから除外）
        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If WW_HEADrow("STATUS") Like "*日報変更*" Then
                Dim WW_MATCH As String = "OFF"
                For i As Integer = WW_IDX To WW_T0007DTLtbl.Rows.Count - 1
                    Dim WW_DTLrow As DataRow = WW_T0007DTLtbl.Rows(i)
                    If WW_HEADrow("WORKDATE") = WW_DTLrow("WORKDATE") And
                       WW_HEADrow("STAFFCODE") = WW_DTLrow("STAFFCODE") Then
                        WW_DTLrow("STATUS") = WW_HEADrow("STATUS")
                        WW_MATCH = "ON"
                    Else
                        If WW_MATCH = "ON" Then
                            WW_IDX = i
                            Exit For
                        End If
                    End If
                Next
            End If
        Next

        '作り直すデータを削除
        Dim WW_Filter As String = "(DATAKBN = 'K' and (STATUS = '' or STATUS = 'Ｂ勤再計算')) or RECODEKBN = '2' or DATAKBN = 'N' or "
        WW_Filter = WW_Filter & "(WORKDATE < #" & iSTDATE & "# or WORKDATE > #" & iENDDATE & "#)"
        CS0026TblSort.TABLE = WW_T0007DTLtbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "DATAKBN, STATUS, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007DTLtbl = CS0026TblSort.sort()

        'T5準備
        Dim iT0005view As DataView
        iT0005view = New DataView(iT0005tbl)
        iT0005view.Sort = "YMD, STAFFCODE"

        'T7準備
        Dim iT0007view As DataView
        iT0007view = New DataView(WW_T0007HEADtbl)
        iT0007view.Sort = "RECODEKBN, STATUS, WORKDATE"
        iT0007view.RowFilter = "RECODEKBN ='0' and STATUS Like '*日報*' and WORKDATE >= #" & iSTDATE & "# and WORKDATE <= #" & iENDDATE & "#"
        Dim wT0007tbl As DataTable = iT0007view.ToTable

        'T7ディテイル作成
        Dim WW_BREAKTIME As Integer = 0
        Dim WW_SEQ As Integer = 0
        For Each WW_HEADrow As DataRow In wT0007tbl.Rows
            Dim WW_NIPPONO As String = ""
            Dim WW_A1CNT As Integer = 0
            Dim WW_F1CNT As Integer = 0

            WW_BREAKTIME = 0
            WW_SEQ = 0

            iT0005view.RowFilter = "YMD = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "'"
            Dim T0005tbl As DataTable = iT0005view.ToTable()
            '該当する日報を抽出し、新しいテーブルを作成する

            Dim WW_WORKKBN As String = ""
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim WW_NIPPOrow As DataRow = T0005tbl.Rows(i)

                '休憩の合計
                If WW_NIPPOrow("WORKKBN") = "BB" Then
                    WW_BREAKTIME = WW_BREAKTIME + WW_NIPPOrow("WORKTIME")
                End If

                If WW_NIPPOrow("WORKKBN") = "A1" And WW_A1CNT = 0 Then
                    WW_A1CNT += 1
                    '--------------------------------------------------------------------------------
                    '始業レコード作成
                    '--------------------------------------------------------------------------------
                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = WW_NIPPOrow("STDATE")
                    WW_T0007row("STTIME") = WW_NIPPOrow("STTIME")
                    '終了日時、後ろレコードの開始日時
                    WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                    WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "A1"
                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                    WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                    If WW_T0007row("ORGSEQ").ToString = "" Then
                        WW_T0007row("ORGSEQ") = 0
                    End If

                    WW_T0007tbl.Rows.Add(WW_T0007row)

                    WW_DATE_SV = WW_T0007row("ENDDATE")
                    WW_TIME_SV = WW_T0007row("ENDTIME")
                    WW_WORKKBN = "A1"
                    Continue For
                End If

                If WW_NIPPOrow("WORKKBN") = "F1" Then
                    WW_F1CNT += 1
                    '直前がA1（出社の場合）
                    If WW_WORKKBN = "A1" Then

                        If WW_NIPPOrow("STDATE") = WW_DATE_SV And
                           WW_NIPPOrow("STTIME") = WW_TIME_SV Then
                        Else
                            '--------------------------------------------------------------------------------
                            '他作業レコード作成
                            '--------------------------------------------------------------------------------
                            WW_T0007row = WW_T0007tbl.NewRow
                            T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                            '開始日時、前のレコードの終了日時
                            WW_T0007row("STDATE") = WW_DATE_SV
                            WW_T0007row("STTIME") = WW_TIME_SV
                            '終了日時、後ろレコードの開始日時
                            WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                            WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                            'その他の項目は、現在のレコードをコピーする
                            WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                            WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                            WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                            WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                            WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                            WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                            WW_T0007row("MORG") = WW_HEADrow("MORG")
                            WW_T0007row("HORG") = WW_HEADrow("HORG")
                            WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                            WW_SEQ += 1
                            WW_T0007row("SEQ") = WW_SEQ
                            WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                            WW_T0007row("HIDDEN") = "1"
                            WW_T0007row("HDKBN") = "D"
                            WW_T0007row("DATAKBN") = "K"
                            WW_T0007row("RECODEKBN") = "0"
                            WW_T0007row("WORKKBN") = "BX"

                            '作業時間
                            WW_WORKTIME = DateDiff("n",
                                                  WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                                  WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                                 )
                            WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                            WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                            WW_T0007row("CAMPNAMES") = ""
                            CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                            WW_T0007row("WORKKBNNAMES") = ""
                            CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("STAFFNAMES") = ""
                            CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                            WW_T0007row("HOLIDAYKBNNAMES") = ""
                            CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("PAYKBNNAMES") = ""
                            CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                            WW_T0007row("SHUKCHOKKBNNAMES") = ""
                            CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                            WW_T0007row("MORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                            WW_T0007row("HORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                            WW_T0007row("SORGNAMES") = ""
                            CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                            WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                            If WW_T0007row("ORGSEQ").ToString = "" Then
                                WW_T0007row("ORGSEQ") = 0
                            End If

                            WW_T0007tbl.Rows.Add(WW_T0007row)
                        End If
                        Continue For
                    End If
                End If

                If WW_NIPPOrow("WORKKBN") = "F3" Then
                    WW_NIPPONO = WW_NIPPOrow("NIPPONO")
                    WW_DATE_SV = WW_NIPPOrow("ENDDATE")
                    WW_TIME_SV = WW_NIPPOrow("ENDTIME")

                    Continue For
                End If

                '--------------------------------------------------------------------------------
                '出庫が２回目以降は、前の日報と後ろの日報の間に、その他作業レコードを作成する
                '--------------------------------------------------------------------------------
                If WW_F1CNT > 1 Then
                    If WW_NIPPOrow("WORKKBN") = "F1" Then
                        '初期化
                        WW_T0007row = WW_T0007tbl.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                        '開始日時、前のレコードの終了日時
                        WW_T0007row("STDATE") = WW_DATE_SV
                        WW_T0007row("STTIME") = WW_TIME_SV
                        '終了日時、後ろレコードの開始日時
                        WW_T0007row("ENDDATE") = WW_NIPPOrow("STDATE")
                        WW_T0007row("ENDTIME") = WW_NIPPOrow("STTIME")

                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                        WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("SORG") = WW_NIPPOrow("SHIPORG")
                        WW_SEQ += 1
                        WW_T0007row("SEQ") = WW_SEQ
                        WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "0"
                        WW_T0007row("WORKKBN") = "BX"

                        '作業時間
                        WW_WORKTIME = DateDiff("n",
                                              WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                              WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                             )
                        WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                        WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                        WW_T0007row("CAMPNAMES") = ""
                        CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                        WW_T0007row("WORKKBNNAMES") = ""
                        CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("STAFFNAMES") = ""
                        CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                        WW_T0007row("HOLIDAYKBNNAMES") = ""
                        CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("PAYKBNNAMES") = ""
                        CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                        WW_T0007row("SHUKCHOKKBNNAMES") = ""
                        CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                        WW_T0007row("MORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                        WW_T0007row("HORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                        WW_T0007row("SORGNAMES") = ""
                        CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                        WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                        If WW_T0007row("ORGSEQ").ToString = "" Then
                            WW_T0007row("ORGSEQ") = 0
                        End If

                        WW_T0007tbl.Rows.Add(WW_T0007row)
                    End If
                End If

                WW_WORKKBN = ""
            Next
            '最終レコードの追加
            If T0005tbl.Rows.Count > 0 Then
                Dim WW_BREAK_FLG As String = "OFF"
                Dim WW_MAXBREAKTIME As Integer = 60
                If WW_MAXBREAKTIME - WW_BREAKTIME > 0 Then
                    WW_BREAK_FLG = "ON"

                    WW_T0007row = WW_T0007tbl.NewRow
                    T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                    WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                    '終了日時、後ろレコードの開始日時

                    '６０分－休憩時間
                    WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                    WW_T0007row("ENDDATE") = WW_date.AddMinutes(WW_MAXBREAKTIME - WW_BREAKTIME).ToString("yyyy/MM/dd")
                    WW_T0007row("ENDTIME") = WW_date.AddMinutes(WW_MAXBREAKTIME - WW_BREAKTIME).ToString("HH:mm")

                    'その他の項目は、現在のレコードをコピーする
                    WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                    WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                    WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                    WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                    WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                    WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                    WW_T0007row("MORG") = WW_HEADrow("MORG")
                    WW_T0007row("HORG") = WW_HEADrow("HORG")
                    WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                    WW_SEQ += 1
                    WW_T0007row("SEQ") = WW_SEQ
                    WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                    WW_T0007row("HIDDEN") = "1"
                    WW_T0007row("HDKBN") = "D"
                    WW_T0007row("DATAKBN") = "K"
                    WW_T0007row("RECODEKBN") = "0"
                    WW_T0007row("WORKKBN") = "BB"

                    '作業時間
                    WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                    WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_T0007row("BREAKTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                    WW_T0007row("CAMPNAMES") = ""
                    CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                    WW_T0007row("WORKKBNNAMES") = ""
                    CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("STAFFNAMES") = ""
                    CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                    WW_T0007row("HOLIDAYKBNNAMES") = ""
                    CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("PAYKBNNAMES") = ""
                    CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                    WW_T0007row("SHUKCHOKKBNNAMES") = ""
                    CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                    WW_T0007row("MORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                    WW_T0007row("HORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                    WW_T0007row("SORGNAMES") = ""
                    CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                    WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                    If WW_T0007row("ORGSEQ").ToString = "" Then
                        WW_T0007row("ORGSEQ") = 0
                    End If

                    WW_T0007tbl.Rows.Add(WW_T0007row)

                    WW_DATE_SV = WW_T0007row("ENDDATE")
                    WW_TIME_SV = WW_T0007row("ENDTIME")
                End If

                '--------------------------------------------------------------------------------
                '他作業（＋１０分）レコード作成（最後のデータ）
                '--------------------------------------------------------------------------------
                WW_T0007row = WW_T0007tbl.NewRow
                T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                If WW_BREAK_FLG = "OFF" Then
                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDDATE")
                    WW_T0007row("STTIME") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("ENDTIME")
                Else
                    '開始日時、前のレコードの終了日時
                    WW_T0007row("STDATE") = WW_DATE_SV
                    WW_T0007row("STTIME") = WW_TIME_SV
                End If
                '拘束時間（＋１０分）、袖ヶ浦２の場合のみ20分とする
                WW_date = CDate(WW_T0007row("STDATE") & " " & WW_T0007row("STTIME"))
                WW_T0007row("ENDDATE") = WW_date.AddMinutes(10).ToString("yyyy/MM/dd")
                WW_T0007row("ENDTIME") = WW_date.AddMinutes(10).ToString("HH:mm")

                'その他の項目は、現在のレコードをコピーする
                WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                WW_T0007row("MORG") = WW_HEADrow("MORG")
                WW_T0007row("HORG") = WW_HEADrow("HORG")
                WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                WW_SEQ += 1
                WW_T0007row("SEQ") = WW_SEQ
                WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                WW_T0007row("HIDDEN") = "1"
                WW_T0007row("HDKBN") = "D"
                WW_T0007row("DATAKBN") = "K"
                WW_T0007row("RECODEKBN") = "0"
                WW_T0007row("WORKKBN") = "BX"
                WW_T0007row("DELFLG") = "0"

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                      WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                      WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                     )
                WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                WW_T0007row("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                WW_T0007row("WORKKBNNAMES") = ""
                CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                WW_T0007row("STAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                WW_T0007row("HOLIDAYKBNNAMES") = ""
                CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("PAYKBNNAMES") = ""
                CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("SHUKCHOKKBNNAMES") = ""
                CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                WW_T0007row("MORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                WW_T0007row("HORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                WW_T0007row("SORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                If WW_T0007row("ORGSEQ").ToString = "" Then
                    WW_T0007row("ORGSEQ") = 0
                End If

                WW_T0007tbl.Rows.Add(WW_T0007row)

                WW_DATE_SV = WW_T0007row("ENDDATE")
                WW_TIME_SV = WW_T0007row("ENDTIME")
                '--------------------------------------------------------------------------------
                '終業レコード作成（最後のデータ）
                '--------------------------------------------------------------------------------
                WW_T0007row = WW_T0007tbl.NewRow
                T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)

                '開始日時、前のレコードの終了日時
                WW_T0007row("STDATE") = WW_DATE_SV
                WW_T0007row("STTIME") = WW_TIME_SV
                '終了日時、後ろレコードの開始日時
                WW_T0007row("ENDDATE") = WW_DATE_SV
                WW_T0007row("ENDTIME") = WW_TIME_SV

                'その他の項目は、現在のレコードをコピーする
                WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                WW_T0007row("OPERATION") = WW_HEADrow("OPERATION")
                WW_T0007row("STATUS") = WW_HEADrow("STATUS")
                WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                WW_T0007row("MORG") = WW_HEADrow("MORG")
                WW_T0007row("HORG") = WW_HEADrow("HORG")
                WW_T0007row("SORG") = T0005tbl.Rows(T0005tbl.Rows.Count - 1)("SHIPORG")
                WW_SEQ += 1
                WW_T0007row("SEQ") = WW_SEQ
                WW_T0007row("WORKINGWEEK") = WW_HEADrow("WORKINGWEEK")
                WW_T0007row("HIDDEN") = "1"
                WW_T0007row("HDKBN") = "D"
                WW_T0007row("DATAKBN") = "K"
                WW_T0007row("RECODEKBN") = "0"
                WW_T0007row("WORKKBN") = "Z1"
                WW_T0007row("DELFLG") = "0"

                '作業時間
                WW_WORKTIME = DateDiff("n",
                                          WW_T0007row("STDATE") + " " + WW_T0007row("STTIME"),
                                          WW_T0007row("ENDDATE") + " " + WW_T0007row("ENDTIME")
                                         )
                WW_T0007row("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                WW_T0007row("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)

                WW_T0007row("CAMPNAMES") = ""
                CODENAME_get("CAMPCODE", WW_T0007row("CAMPCODE"), WW_T0007row("CAMPNAMES"), WW_DUMMY)
                WW_T0007row("WORKKBNNAMES") = ""
                CODENAME_get("WORKKBN", WW_T0007row("WORKKBN"), WW_T0007row("WORKKBNNAMES"), WW_DUMMY)
                WW_T0007row("STAFFNAMES") = ""
                CODENAME_get("STAFFCODE", WW_T0007row("STAFFCODE"), WW_T0007row("STAFFNAMES"), WW_DUMMY)
                WW_T0007row("HOLIDAYKBNNAMES") = ""
                CODENAME_get("HOLIDAYKBN", WW_T0007row("HOLIDAYKBN"), WW_T0007row("HOLIDAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("PAYKBNNAMES") = ""
                CODENAME_get("PAYKBN", WW_T0007row("PAYKBN"), WW_T0007row("PAYKBNNAMES"), WW_DUMMY)
                WW_T0007row("SHUKCHOKKBNNAMES") = ""
                CODENAME_get("SHUKCHOKKBN", WW_T0007row("SHUKCHOKKBN"), WW_T0007row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                WW_T0007row("MORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("MORG"), WW_T0007row("MORGNAMES"), WW_DUMMY)
                WW_T0007row("HORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("HORG"), WW_T0007row("HORGNAMES"), WW_DUMMY)
                WW_T0007row("SORGNAMES") = ""
                CODENAME_get("HORG", WW_T0007row("SORG"), WW_T0007row("SORGNAMES"), WW_DUMMY)

                WW_T0007row("ORGSEQ") = WW_HEADrow("ORGSEQ")
                If WW_T0007row("ORGSEQ").ToString = "" Then
                    WW_T0007row("ORGSEQ") = 0
                End If

                WW_T0007tbl.Rows.Add(WW_T0007row)
            End If
        Next


        ioTbl = WW_T0007DELtbl.Copy
        ioTbl.Merge(WW_T0007HEADtbl)
        ioTbl.Merge(WW_T0007DTLtbl)
        ioTbl.Merge(WW_T0007tbl)

        'ソート
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, HDKBN DESC, ENDDATE, ENDTIME, WORKKBN"
        ioTbl = CS0026TblSort.sort()

        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing
        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007tbl.Dispose()
        WW_T0007tbl = Nothing
        wT0007tbl.Dispose()
        wT0007tbl = Nothing
        iT0005view.Dispose()
        iT0005view = Nothing
        T0005tbl.Dispose()
        T0005tbl = Nothing

    End Sub

    ' ***  ヘッダレコード編集
    Public Sub HeadEdit(ByRef ioTbl As DataTable, ByRef iT0005tbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_SUISOKBN As String = ""

        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        ioTbl = CS0026TblSort.sort()

        Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
        Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
        Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
        For i As Integer = 0 To ioTbl.Rows.Count - 1
            Dim ioTblrow As DataRow = ioTbl.Rows(i)

            '削除レコードを取得
            If ioTblrow("SELECT") = "0" Then
                Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                DELrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DELtbl.Rows.Add(DELrow)
            End If

            '勤怠のヘッダレコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" Then
                Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                HEADrow.ItemArray = ioTblrow.ItemArray
                WW_T0007HEADtbl.Rows.Add(HEADrow)
            End If

            '勤怠の明細レコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                DTLrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DTLtbl.Rows.Add(DTLrow)
            End If
        Next


        'T7準備
        Dim iT0007view As DataView = New DataView(WW_T0007DTLtbl)
        iT0007view.Sort = "WORKDATE, STAFFCODE, RECODEKBN, WORKKBN"

        'T5準備
        Dim iT0005view As DataView = New DataView(iT0005tbl)
        iT0005view.Sort = "YMD, STAFFCODE, WORKKBN"

        '勤怠ヘッダの集計
        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If WW_HEADrow("STATUS") Like "*日報*" And WW_HEADrow("RECODEKBN") = "0" And
               WW_HEADrow("WORKDATE") >= iSTDATE And WW_HEADrow("WORKDATE") <= iENDDATE Then
            Else
                Continue For
            End If


            '日報取得
            '該当する日報を抽出し、新しいテーブルを作成する
            iT0007view.RowFilter = "WORKDATE = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "' and RECODEKBN = '0' and WORKKBN in ('A1','Z1','BB')"

            Dim WW_BREAKTIME As Integer = 0
            Dim WW_MATCH As String = "OFF"

            '勤怠レコードの必要情報からヘッダを集計
            For i As Integer = WW_IDX To iT0007view.Count - 1
                Dim WW_DTLrow As DataRow = iT0007view.Item(i).Row

                WW_DTLrow("PAYKBN") = "00"          '勤怠区分：通常
                If WW_DTLrow("WORKKBN") = "A1" Then
                    '出社レコードより開始日、開始時間を取得
                    WW_HEADrow("STDATE") = WW_DTLrow("STDATE")
                    WW_HEADrow("STTIME") = WW_DTLrow("STTIME")
                End If

                If WW_DTLrow("WORKKBN") = "Z1" Then
                    '退社レコードの終了日、終了時間を取得
                    WW_HEADrow("ENDDATE") = WW_DTLrow("ENDDATE")
                    WW_HEADrow("ENDTIME") = WW_DTLrow("ENDTIME")
                End If

                If WW_DTLrow("WORKKBN") = "BB" Then
                    '休憩レコードを取得
                    WW_BREAKTIME += T0007COM.HHMMtoMinutes(WW_DTLrow("BREAKTIME"))
                End If
                WW_MATCH = "ON"
            Next
            If WW_MATCH = "ON" Then
                WW_HEADrow("BREAKTIME") = T0007COM.formatHHMM(WW_BREAKTIME)
                WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_BREAKTIME)
                WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                If IsDBNull(WW_HEADrow("STDATE")) Or
                    IsDBNull(WW_HEADrow("ENDDATE")) Or
                    IsDBNull(WW_HEADrow("STTIME")) Or
                    IsDBNull(WW_HEADrow("ENDTIME")) Then
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(0)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(0)
                Else
                    Dim WW_WORKTIME As Integer = 0
                    WW_WORKTIME = DateDiff("n",
                                         WW_HEADrow("STDATE") + " " + WW_HEADrow("STTIME"),
                                         WW_HEADrow("ENDDATE") + " " + WW_HEADrow("ENDTIME")
                                        )
                    WW_HEADrow("WORKTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                    WW_HEADrow("ACTTIME") = T0007COM.formatHHMM(WW_WORKTIME)
                End If
            End If

            '日報取得
            '該当する日報を抽出し、新しいテーブルを作成する
            iT0005view.RowFilter = "YMD = #" & WW_HEADrow("WORKDATE") & "# and STAFFCODE ='" & WW_HEADrow("STAFFCODE") & "' and WORKKBN in ('F3','B3','BB')"

            Dim WW_BREAKTIME2 As Integer = 0
            Dim WW_HAISO As Integer = 0
            Dim WW_KAISO As Integer = 0
            Dim WW_B3CNT As Integer = 0
            Dim WW_UNLOADADDCNT1 As Integer = 0
            Dim WW_UNLOADADDCNT2 As Integer = 0
            Dim WW_UNLOADADDCNT3 As Integer = 0
            Dim WW_UNLOADADDCNT4 As Integer = 0
            Dim WW_LOADINGCNT1 As Integer = 0
            Dim WW_LOADINGCNT2 As Integer = 0
            Dim WW_SHORTDISTANCE1 As Integer = 0
            Dim WW_SHORTDISTANCE2 As Integer = 0

            '日報レコードの必要情報からヘッダを集計
            For i As Integer = 0 To iT0005view.Count - 1
                Dim WW_NIPPOrow As DataRow = iT0005view.Item(i).Row
                If WW_NIPPOrow("WORKKBN") = "F3" Then
                    '帰庫（F3）に持っている総走行キロを取得
                    If WW_NIPPOrow("L1KAISO") = "回送" And WW_NIPPOrow("SUISOKBN") <> "1" Then
                        WW_KAISO = WW_KAISO + WW_NIPPOrow("SOUDISTANCE")
                    Else
                        WW_HAISO = WW_HAISO + WW_NIPPOrow("SOUDISTANCE")
                    End If
                    WW_HEADrow("SHACHUHAKKBN") = "0"
                    WW_HEADrow("SHACHUHAKKBNNAMES") = ""
                    If T0005COM.ShakoCheck(work.WF_T7SEL_CAMPCODE.Text, WW_NIPPOrow("LATITUDE"), WW_NIPPOrow("LONGITUDE")) = "OK" Then
                        '帰庫が車庫の場合、車中泊ではない
                    Else
                        '帰庫が車庫以外の場合、車中泊
                        WW_HEADrow("SHACHUHAKKBN") = "1"
                        WW_HEADrow("SHACHUHAKKBNNAMES") = "✔"
                    End If
                End If

                If WW_NIPPOrow("WORKKBN") = "BB" Then
                    '休憩（BB）レコードの作業時間（休憩時間）を全て加算
                    WW_BREAKTIME2 += WW_NIPPOrow("WORKTIME")
                End If
                If WW_NIPPOrow("WORKKBN") = "B3" Then
                    If WW_NIPPOrow("SUISOKBN") <> "1" Then
                        '荷卸（B3）をカウントする（水素はカウントしない）
                        WW_B3CNT += 1
                    End If
                End If

                '危険品荷卸回数
                Select Case WW_NIPPOrow("UNLOADADDTANKA")
                    Case "0"
                    Case "100"
                        WW_UNLOADADDCNT1 += 1
                    Case "200"
                        WW_UNLOADADDCNT2 += 1
                    Case "800"
                        WW_UNLOADADDCNT3 += 1
                    Case Else
                        'WW_UNLOADADDCNT4 += 1
                End Select

                '危険品積込回数
                Select Case WW_NIPPOrow("LOADINGTANKA")
                    Case "0"
                    Case "1000"
                        WW_LOADINGCNT1 += 1
                    Case Else
                        'WW_LOADINGCNT2 += 1
                End Select

                WW_HEADrow("NIPPOLINKCODE") = WW_NIPPOrow("UPDYMD")
            Next
            '日報の休憩
            WW_HEADrow("NIPPOBREAKTIME") = T0007COM.formatHHMM(WW_BREAKTIME2)
            '勤怠の休憩＋日報の休憩を合計に
            WW_HEADrow("BREAKTIMETTL") = T0007COM.formatHHMM(WW_BREAKTIME + WW_BREAKTIME2)

            WW_HEADrow("UNLOADCNT") = WW_B3CNT
            WW_HEADrow("UNLOADCNTTTL") = WW_B3CNT
            WW_HEADrow("KAIDISTANCE") = WW_KAISO
            WW_HEADrow("KAIDISTANCETTL") = WW_KAISO + WW_HEADrow("KAIDISTANCECHO")
            WW_HEADrow("HAIDISTANCE") = WW_HAISO
            WW_HEADrow("HAIDISTANCETTL") = WW_HAISO + WW_HEADrow("HAIDISTANCECHO")

            WW_HEADrow("UNLOADADDCNT1") = WW_UNLOADADDCNT1
            WW_HEADrow("UNLOADADDCNT1TTL") = WW_UNLOADADDCNT1 + +WW_HEADrow("UNLOADADDCNT1CHO")
            WW_HEADrow("UNLOADADDCNT2") = WW_UNLOADADDCNT2
            WW_HEADrow("UNLOADADDCNT2TTL") = WW_UNLOADADDCNT2 + +WW_HEADrow("UNLOADADDCNT2CHO")
            WW_HEADrow("UNLOADADDCNT3") = WW_UNLOADADDCNT3
            WW_HEADrow("UNLOADADDCNT3TTL") = WW_UNLOADADDCNT3 + +WW_HEADrow("UNLOADADDCNT3CHO")
            WW_HEADrow("UNLOADADDCNT4") = 0
            WW_HEADrow("UNLOADADDCNT4TTL") = 0 + +WW_HEADrow("UNLOADADDCNT4CHO")
            WW_HEADrow("LOADINGCNT1") = WW_LOADINGCNT1
            WW_HEADrow("LOADINGCNT1TTL") = WW_LOADINGCNT1 + +WW_HEADrow("LOADINGCNT1CHO")
            WW_HEADrow("LOADINGCNT2") = 0
            WW_HEADrow("LOADINGCNT2TTL") = 0 + +WW_HEADrow("LOADINGCNT2CHO")
            WW_HEADrow("SHORTDISTANCE1") = WW_SHORTDISTANCE1
            WW_HEADrow("SHORTDISTANCE1TTL") = WW_SHORTDISTANCE1 + +WW_HEADrow("SHORTDISTANCE1CHO")
            WW_HEADrow("SHORTDISTANCE2") = WW_SHORTDISTANCE2
            WW_HEADrow("SHORTDISTANCE2TTL") = WW_SHORTDISTANCE2 + +WW_HEADrow("SHORTDISTANCE2CHO")
        Next

        '勤怠ヘッダのコピー
        ioTbl = WW_T0007HEADtbl.Copy

        '勤怠明細のマージ
        ioTbl.Merge(WW_T0007DTLtbl)

        '更新元（削除）データの戻し
        ioTbl.Merge(WW_T0007DELtbl)

        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing

        iT0005view.Dispose()
        iT0005view = Nothing
        iT0007view.Dispose()
        iT0007view = Nothing
    End Sub

    ' ***  ヘッダレコード編集
    Public Sub BindStDateSet(ByRef ioTbl As DataTable, ByVal iSTDATE As String, ByVal iENDDATE As String)

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_SUISOKBN As String = ""

        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        ioTbl = CS0026TblSort.sort()

        Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
        Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
        Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
        For i As Integer = 0 To ioTbl.Rows.Count - 1
            Dim ioTblrow As DataRow = ioTbl.Rows(i)

            '削除レコードを取得
            If ioTblrow("SELECT") = "0" Then
                Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                DELrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DELtbl.Rows.Add(DELrow)
            End If

            '勤怠のヘッダレコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" Then
                Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                HEADrow.ItemArray = ioTblrow.ItemArray
                WW_T0007HEADtbl.Rows.Add(HEADrow)
            End If

            '勤怠の明細レコードを取得
            If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                DTLrow.ItemArray = ioTblrow.ItemArray
                WW_T0007DTLtbl.Rows.Add(DTLrow)
            End If
        Next


        '勤怠のヘッダレコードを取得
        '前月
        Dim WW_ZDAtE As String = CDate(iSTDATE).AddMonths(-1).ToString("yyyy/MM")
        Dim WW_TDAtE As String = CDate(iSTDATE).ToString("yyyy/MM")

        Dim WW_T0007HEADtbl2 As DataTable = New DataTable
        Dim WW_T0007HEADtbl3 As DataTable = New DataTable
        '前月分は、SELECT='0'（対象外）HIDDEN='1'で登録されている
        Dim WW_Filter As String = "HDKBN = 'H' and RECODEKBN = '0' and TAISHOYM = '" & WW_ZDAtE & "'"
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl2 = CS0026TblSort.sort()

        WW_Filter = "SELECT = '1' and HDKBN = 'H' and RECODEKBN = '0' and TAISHOYM = '" & WW_TDAtE & "'"
        CS0026TblSort.TABLE = ioTbl
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007HEADtbl3 = CS0026TblSort.sort()

        WW_T0007HEADtbl2.Merge(WW_T0007HEADtbl)

        '直前、翌日取得用VIEW
        Dim iT0007view As DataView
        iT0007view = New DataView(WW_T0007HEADtbl2)
        iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

        '勤怠ヘッダの集計

        WW_IDX = 0
        For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
            If WW_HEADrow("STATUS") Like "*日報*" And WW_HEADrow("RECODEKBN") = "0" And
               WW_HEADrow("WORKDATE") >= iSTDATE And WW_HEADrow("WORKDATE") <= iENDDATE Then
            Else
                Continue For
            End If

            '直前の勤務
            Dim WW_ZENFLG As String = "OFF"
            Dim WW_ZENFLG2 As String = "OFF"
            Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))
            Dim WW_ZENDATE As String = dt.AddDays(-1).ToString("yyyy/MM/dd")

            iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
            If iT0007view.Count > 0 Then
                '前日が休みか判定
                If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                    '稼動しているか判定
                    If Val(iT0007view.Item(0).Row("ACTTIME")) = 0 Then
                        '休みで、稼働なし
                        WW_ZENFLG = "ON"
                    End If
                End If
            End If

            '前日が休みで稼働なしの場合、前々日を確認
            If WW_ZENFLG = "ON" Then
                '前々日以前を検索
                WW_ZENDATE = dt.AddDays(-2).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
                If iT0007view.Count > 0 Then
                    '前日が休みか判定
                    If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                        '1:法定休日、2:法定外休日
                        '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                        '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                        '稼動しているか判定
                        If Val(iT0007view.Item(0).Row("ACTTIME")) = 0 Then
                            '休みで、稼働なし
                            WW_ZENFLG2 = "ON"
                        End If
                    Else
                        '稼働日で日を跨いでいれば拘束開始を決定する
                        If iT0007view.Item(0).Row("STDATE") <> iT0007view.Item(0).Row("ENDDATE") Then
                            If WW_HEADrow("STTIME") < "08:00" Then
                                WW_HEADrow("BINDSTDATE") = "08:00"
                            End If
                        End If
                    End If
                End If
            End If

            '前々日が休みで稼働なしの場合、前々日を確認
            If WW_ZENFLG2 = "ON" Then
                '前々日以前を検索
                WW_ZENDATE = dt.AddDays(-3).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_ZENDATE & "#"
                If iT0007view.Count > 0 Then
                    '前日が休みか判定
                    If T0007COM.CheckHOLIDAY(iT0007view.Item(0).Row("HOLIDAYKBN"), iT0007view.Item(0).Row("PAYKBN")) Then
                        '1:法定休日、2:法定外休日
                        '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                        '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休

                        '以降は処理しない２連休までの対応とする
                    Else
                        '稼働日で日を跨いでいれば拘束開始を決定する
                        If iT0007view.Item(0).Row("STDATE") <> iT0007view.Item(0).Row("ENDDATE") Then
                            If WW_HEADrow("STTIME") < "08:00" Then
                                WW_HEADrow("BINDSTDATE") = "08:00"
                            End If
                        End If
                    End If
                End If
            End If
        Next

        '勤怠ヘッダのコピー
        ioTbl = WW_T0007HEADtbl.Copy

        '勤怠明細のマージ
        ioTbl.Merge(WW_T0007DTLtbl)

        '更新元（削除）データの戻し
        ioTbl.Merge(WW_T0007DELtbl)

        WW_T0007HEADtbl.Dispose()
        WW_T0007HEADtbl = Nothing
        WW_T0007HEADtbl2.Dispose()
        WW_T0007HEADtbl2 = Nothing
        WW_T0007HEADtbl3.Dispose()
        WW_T0007HEADtbl3 = Nothing
        WW_T0007DTLtbl.Dispose()
        WW_T0007DTLtbl = Nothing
        WW_T0007DELtbl.Dispose()
        WW_T0007DELtbl = Nothing

        iT0007view.Dispose()
        iT0007view = Nothing
    End Sub

    ' ***  合計明細作成
    Protected Sub CreTTLDTL(ByRef ioTBL As DataTable)

        '合計に明細レコードが存在するか？
        Dim WW_HEADtbl As DataTable = ioTBL.Clone
        Dim WW_TTLDTLtbl As DataTable = ioTBL.Clone
        Dim WW_Filter As String = ""

        WW_Filter = "HDKBN = 'H' and RECODEKBN = '2' and SELECT = '1'"
        CS0026TblSort.TABLE = ioTBL
        CS0026TblSort.FILTER = WW_Filter
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        WW_HEADtbl = CS0026TblSort.sort()

        Dim TTLDTLview As DataView
        TTLDTLview = New DataView(ioTBL)
        TTLDTLview.Sort = "SELECT, STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"

        For Each WW_HEADrow As DataRow In WW_HEADtbl.Rows
            WW_Filter = ""
            WW_Filter = WW_Filter & "WORKDATE  = '" & WW_HEADrow("WORKDATE") & "' and "
            WW_Filter = WW_Filter & "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and "
            WW_Filter = WW_Filter & "RECODEKBN = '" & WW_HEADrow("RECODEKBN") & "' and "
            WW_Filter = WW_Filter & "HDKBN     = 'D'" & " and "
            WW_Filter = WW_Filter & "SELECT    = '1'"
            TTLDTLview.RowFilter = WW_Filter
            WW_TTLDTLtbl = TTLDTLview.ToTable

            If WW_TTLDTLtbl.Rows.Count = 0 Then
                For i As Integer = 1 To 2
                    For j As Integer = 1 To 10
                        Dim WW_T0007row As DataRow = ioTBL.NewRow
                        T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, WW_T0007row)
                        'その他の項目は、現在のレコードをコピーする
                        WW_T0007row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        WW_T0007row("WORKDATE") = WW_HEADrow("WORKDATE")
                        WW_T0007row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        WW_T0007row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        WW_T0007row("MORG") = WW_HEADrow("MORG")
                        WW_T0007row("HORG") = WW_HEADrow("HORG")
                        WW_T0007row("HIDDEN") = "1"
                        WW_T0007row("HDKBN") = "D"
                        WW_T0007row("DATAKBN") = "K"
                        WW_T0007row("RECODEKBN") = "2"
                        WW_T0007row("SHARYOKBN") = i.ToString
                        WW_T0007row("OILPAYKBN") = j.ToString("00")
                        WW_T0007row("SEQ") = i * 10 + j
                        T0007tbl.Rows.Add(WW_T0007row)
                    Next
                Next
            End If
        Next

        WW_HEADtbl.Dispose()
        WW_HEADtbl = Nothing
        WW_TTLDTLtbl.Dispose()
        WW_TTLDTLtbl = Nothing
        TTLDTLview.Dispose()
        TTLDTLview = Nothing

    End Sub


    ' ***  T0007INProwチェック
    Protected Sub T0007INProw_CHEK(ByRef RTN As String)

        '○インターフェイス初期値設定
        RTN = C_MESSAGE_NO.NORMAL

        Dim WW_RESULT As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_LINEerr As String = C_MESSAGE_NO.NORMAL
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '従業員マスタ取得
        Dim WW_STAFFKBN As String = ""
        Dim WW_MORG As String = ""
        Dim WW_HORG As String = ""
        Dim WW_WORKINGH As String = ""
        Dim WW_WORKINGKBN As String = ""
        Dim WW_WORKINGWEEK As String = ""
        Dim WW_TIMEstr() As String = {}

        WW_ERRLIST = New List(Of String)

        '■■■ 単項目チェック(処理前提) ■■■
        CS0036FCHECK.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0036FCHECK.MAPID = GRT00007WRKINC.MAPIDIJKT
        CS0036FCHECK.TBL = S0013tbl

        ' *** ﾚｺｰﾄﾞ区分(RECODEKBN)
        '①必須・項目属性チェック
        CS0036FCHECK.FIELD = "RECODEKBN"
        CS0036FCHECK.VALUE = T0007INProw("RECODEKBN")
        CS0036FCHECK.CS0036FCHECK()
        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            T0007INProw("RECODEKBN") = CS0036FCHECK.VALUEOUT
            CODENAME_get("RECODEKBN", T0007INProw("RECODEKBN"), WW_TEXT, WW_RTN_SW)
            If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(レコード区分エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0007INProw("RECODEKBN") & ") "
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            End If
            T0007INProw("RECODEKBNNAMES") = WW_TEXT
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(レコード区分エラー)です。"
            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
        End If


        '■■■ 単項目チェック(共通情報) ■■■

        ' *** 会社コード(CAMPCODE)
        CS0036FCHECK.FIELD = "CAMPCODE"
        CS0036FCHECK.VALUE = T0007INProw("CAMPCODE")
        CS0036FCHECK.CS0036FCHECK()
        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            T0007INProw("CAMPCODE") = CS0036FCHECK.VALUEOUT
            CODENAME_get("CAMPCODE", T0007INProw("CAMPCODE"), WW_TEXT, WW_RTN_SW)
            If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0007INProw("CAMPCODE") & ") "
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            End If
            T0007INProw("CAMPNAMES") = WW_TEXT
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
        End If

        ' *** 従業員コード(STAFFCODE)、配属部署(HORG)、管理部署(MORG)、社員区分(STAFFKBN)
        CS0036FCHECK.FIELD = "STAFFCODE"
        CS0036FCHECK.VALUE = T0007INProw("STAFFCODE")
        CS0036FCHECK.CS0036FCHECK()
        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            '従業員存在チェック＆名称設定
            T0007INProw("STAFFCODE") = CS0036FCHECK.VALUEOUT
            CODENAME_get("STAFFCODE", T0007INProw("STAFFCODE"), WW_TEXT, WW_RTN_SW)
            If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(従業員エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T0007INProw("STAFFCODE") & ") "
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            End If
            T0007INProw("STAFFNAMES") = WW_TEXT

            '社員区分取得＆名称設定
            MB001_Select(T0007INProw, WW_STAFFKBN, WW_MORG, WW_HORG, WW_WORKINGH, WW_DUMMY)
            T0007INProw("STAFFKBN") = WW_STAFFKBN
            CODENAME_get("STAFFKBN", T0007INProw("STAFFKBN"), WW_TEXT, WW_DUMMY)
            T0007INProw("STAFFKBNNAMES") = WW_TEXT
            '管理部署取得＆名称設定
            T0007INProw("MORG") = WW_MORG
            CODENAME_get("ORG", T0007INProw("MORG"), WW_TEXT, WW_DUMMY)
            T0007INProw("MORGNAMES") = WW_TEXT
            '配属部署取得＆名称設定
            T0007INProw("HORG") = WW_HORG
            CODENAME_get("ORG", T0007INProw("HORG"), WW_TEXT, WW_DUMMY)
            T0007INProw("HORGNAMES") = WW_TEXT
        Else
            'エラーレポート編集
            WW_CheckMES1 = "・更新できないレコード(従業員エラー)です。"
            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
        End If

        '■■■ 単項目チェック(日別情報) ■■■

        If T0007INProw("RECODEKBN") = "0" Then

            T0007INProw("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text

            ' *** 勤務年月日(WORKDATE)、曜日(WORKINGWEEK)
            If T0007INProw("WORKDATE") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(勤務年月日無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "WORKDATE"
                CS0036FCHECK.VALUE = T0007INProw("WORKDATE")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("WORKDATE") = CS0036FCHECK.VALUEOUT
                    '曜日設定＆名称設定
                    MB005_Select(T0007INProw, WW_WORKINGWEEK, WW_WORKINGKBN, WW_DUMMY)
                    T0007INProw("WORKINGWEEK") = WW_WORKINGWEEK
                    CODENAME_get("WORKINGWEEK", T0007INProw("WORKINGWEEK"), WW_TEXT, WW_DUMMY)
                    T0007INProw("WORKINGWEEKNAMES") = WW_TEXT

                    If IsDate(T0007INProw("WORKDATE")) Then
                        If CDate(T0007INProw("WORKDATE")).ToString("yyyy/MM") <> work.WF_T7SEL_TAISHOYM.Text Then
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(勤務年月日不正)です。"
                            WW_CheckMES2 = T0007INProw("WORKDATE")
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(勤務年月日エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 休日区分(HOLIDAYKBN)
            T0007INProw("HOLIDAYKBN") = WW_WORKINGKBN

            If T0007INProw("HOLIDAYKBN") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(休日区分無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HOLIDAYKBN"
                CS0036FCHECK.VALUE = T0007INProw("HOLIDAYKBN")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    '名称取得
                    T0007INProw("HOLIDAYKBN") = CS0036FCHECK.VALUEOUT
                    CODENAME_get("HOLIDAYKBN", T0007INProw("HOLIDAYKBN"), WW_TEXT, WW_DUMMY)
                    T0007INProw("HOLIDAYKBNNAMES") = WW_TEXT
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(休日区分エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 勤怠区分(PAYKBN)
            If T0007INProw("PAYKBN") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(勤怠区分無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "PAYKBN"
                CS0036FCHECK.VALUE = T0007INProw("PAYKBN")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("PAYKBN") = CS0036FCHECK.VALUEOUT
                    '名称取得
                    CODENAME_get("PAYKBN", T0007INProw("PAYKBN"), WW_TEXT, WW_DUMMY)
                    T0007INProw("PAYKBNNAMES") = WW_TEXT
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 宿日直区分(SHUKCHOKKBN)
            If T0007INProw("SHUKCHOKKBN") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(宿日直区分無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHUKCHOKKBN"
                CS0036FCHECK.VALUE = T0007INProw("SHUKCHOKKBN")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHUKCHOKKBN") = CS0036FCHECK.VALUEOUT
                    '名称取得
                    CODENAME_get("SHUKCHOKKBN", T0007INProw("SHUKCHOKKBN"), WW_TEXT, WW_DUMMY)
                    T0007INProw("SHUKCHOKKBNNAMES") = WW_TEXT
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(宿日直区分エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 出社日(STDATE)
            If T0007INProw("STDATE") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(出社日無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "STDATE"
                CS0036FCHECK.VALUE = T0007INProw("STDATE")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("STDATE") = CS0036FCHECK.VALUEOUT
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(出社日エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 出社時刻(STTIME)
            If T0007INProw("STTIME") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(出社時間無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "STTIME"
                CS0036FCHECK.VALUE = T0007INProw("STTIME")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("STTIME") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(出社時間エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 拘束開始時刻(BINDSTDATE)　　　　　補完
            If T0007INProw("BINDSTDATE") = Nothing Then
                T0007INProw("BINDSTDATE") = T0007INProw("STTIME")
            End If

            CS0036FCHECK.FIELD = "BINDSTDATE"
            CS0036FCHECK.VALUE = T0007INProw("BINDSTDATE")
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                T0007INProw("BINDSTDATE") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(拘束開始時刻エラー)です。"
                WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            End If

            ' *** 所定拘束(BINDTIME)　　　　　補完
            If T0007INProw("BINDTIME") = Nothing Then
                If WW_WORKINGKBN = 0 Then
                    T0007INProw("BINDTIME") = WW_WORKINGH
                Else
                    T0007INProw("BINDTIME") = "00:00"
                End If
            End If

            CS0036FCHECK.FIELD = "BINDTIME"
            CS0036FCHECK.VALUE = T0007INProw("BINDTIME")
            CS0036FCHECK.CS0036FCHECK()
            If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                T0007INProw("BINDTIME") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
            Else
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(拘束時間エラー)です。"
                WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            End If

            ' *** 退社日(ENDDATE)
            If T0007INProw("ENDDATE") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(退社日無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "ENDDATE"
                CS0036FCHECK.VALUE = T0007INProw("ENDDATE")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("ENDDATE") = CS0036FCHECK.VALUEOUT
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(退社日エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 退社時刻(ENDTIME)
            If T0007INProw("ENDTIME") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(退社時刻無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "ENDTIME"
                CS0036FCHECK.VALUE = T0007INProw("ENDTIME")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("ENDTIME") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(退社時刻エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 稼働時間(ACTTIME)
            If WW_ERRLIST.IndexOf("10023") = 0 Then
                '稼働時間設定
                Dim WW_STDATEstr As String = T0007INProw("STDATE") & " " & T0007INProw("STTIME")
                Dim WW_ENDDATEstr As String = T0007INProw("ENDDATE") & " " & T0007INProw("ENDTIME")
                If IsDate(WW_STDATEstr) And IsDate(WW_ENDDATEstr) Then
                    T0007INProw("ACTTIME") = DateDiff("n", WW_STDATEstr, WW_ENDDATEstr)
                End If
            End If

            ' *** 休憩時間(BREAKTIME)
            If T0007INProw("BREAKTIME") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(休憩時間無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "BREAKTIME"
                CS0036FCHECK.VALUE = T0007INProw("BREAKTIME")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("BREAKTIME") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(休憩時間エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 特作Ⅰ(TOKUSA1TIME)
            If T0007INProw("TOKUSA1TIME") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(特作Ⅰ無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "TOKUSA1TIME"
                CS0036FCHECK.VALUE = T0007INProw("TOKUSA1TIME")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("TOKUSA1TIME") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                    T0007INProw("TOKUSA1TIMETTL") = CDate(CS0036FCHECK.VALUEOUT).ToString("HH:mm")
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 洗浄回数(SENJYOCNT)
            If T0007INProw("SENJYOCNT") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(洗浄回数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SENJYOCNT"
                CS0036FCHECK.VALUE = T0007INProw("SENJYOCNT")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SENJYOCNT") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("SENJYOCNTTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(洗浄回数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数１(UNLOADADDCNT1)
            If T0007INProw("UNLOADADDCNT1") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT1"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT1")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT1") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("UNLOADADDCNT1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数２(UNLOADADDCNT2)
            If T0007INProw("UNLOADADDCNT2") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT2"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT2")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT2") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("UNLOADADDCNT2TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数３(UNLOADADDCNT3)
            If T0007INProw("UNLOADADDCNT3") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数３無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT3"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT3")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT3") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("UNLOADADDCNT3TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数３エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            '' *** 危険品荷卸回数４(UNLOADADDCNT4)
            'If T0007INProw("UNLOADADDCNT4") = Nothing Then
            '    'エラーレポート編集
            '    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数４無)です。"
            '    WW_CheckMES2 = ""
            '    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            'Else
            '    CS0036FCHECK.FIELD = "UNLOADADDCNT4"
            '    CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT4")
            '    CS0036FCHECK.CS0036FCHECK()
            '    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            '        T0007INProw("UNLOADADDCNT4") = Val(CS0036FCHECK.VALUEOUT)
            '        T0007INProw("UNLOADADDCNT4TTL") = Val(CS0036FCHECK.VALUEOUT)
            '    Else
            '        'エラーレポート編集
            '        WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数４エラー)です。"
            '        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            '        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            '    End If
            'End If

            ' *** 危険品積込回数１(LOADINGCNT1)
            If T0007INProw("LOADINGCNT1") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品積込回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "LOADINGCNT1"
                CS0036FCHECK.VALUE = T0007INProw("LOADINGCNT1")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("LOADINGCNT1") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("LOADINGCNT1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品積込回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品積込回数１(SHORTDISTANCE1)
            If T0007INProw("SHORTDISTANCE1") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHORTDISTANCE1"
                CS0036FCHECK.VALUE = T0007INProw("SHORTDISTANCE1")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHORTDISTANCE1") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("SHORTDISTANCE1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品積込回数２(SHORTDISTANCE2)
            If T0007INProw("SHORTDISTANCE2") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHORTDISTANCE2"
                CS0036FCHECK.VALUE = T0007INProw("SHORTDISTANCE2")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHORTDISTANCE2") = Val(CS0036FCHECK.VALUEOUT)
                    T0007INProw("SHORTDISTANCE2TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 荷卸回数(UNLOADCNT)　…　計算項目
            ' *** 平日残業時間(ORVERTIME)　…　計算項目
            ' *** 平日深夜時間(WNIGHTTIME)　…　計算項目
            ' *** 休日出勤時間(HWORKTIME)　…　計算項目
            ' *** 休日深夜時間(HNIGHTTIME)　…　計算項目
            ' *** 日曜出勤時間(SWORKTIME)　…　計算項目
            ' *** 日曜深夜時間(SNIGHTTIME)　…　計算項目
            ' *** 所定深夜時間(NIGHTTIME)　…　計算項目
            ' *** 配送距離(HAIDISTANCE)　…　計算項目
            ' *** 回送距離(KAIDISTANCE)　…　計算項目


        End If


        '■■■ 単項目チェック(月合計情報) ■■■

        If T0007INProw("RECODEKBN") = "2" Then

            ' *** 対象年月(TAISHOYM)
            If T0007INProw("TAISHOYM") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(対象年月無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                If IsDate(T0007INProw("TAISHOYM") & "/01") Then
                    T0007INProw("TAISHOYM") = CDate(T0007INProw("TAISHOYM") & "/01").ToString("yyyy/MM")
                    If T0007INProw("TAISHOYM") <> work.WF_T7SEL_TAISHOYM.Text Then
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(対象年月不正)です。"
                        WW_CheckMES2 = T0007INProw("TAISHOYM")
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(対象年月不正)です。"
                    WW_CheckMES2 = T0007INProw("TAISHOYM")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 所労合計(WORKNISSUTTL)
            If T0007INProw("WORKNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(所労日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "WORKNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("WORKNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("WORKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(所労日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 年休合計(NENKYUNISSUTTL)
            If T0007INProw("NENKYUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(年休日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "NENKYUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("NENKYUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("NENKYUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(年休日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 協約週休合計(KYOTEIWEEKNISSUTTL)
            If T0007INProw("KYOTEIWEEKNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(協定週休日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "KYOTEIWEEKNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("KYOTEIWEEKNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("KYOTEIWEEKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(協定週休日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 傷欠合計(SHOUKETUNISSUTTL)
            If T0007INProw("SHOUKETUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(傷欠日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHOUKETUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("SHOUKETUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHOUKETUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(傷欠日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 特休合計(TOKUKYUNISSUTTL)
            If T0007INProw("TOKUKYUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(特休日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "TOKUKYUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("TOKUKYUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("TOKUKYUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(特休日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            '' *** 週休合計(WEEKNISSUTTL)
            'If T0007INProw("WEEKNISSUTTL") = Nothing Then
            '    'エラーレポート編集
            '    WW_CheckMES1 = "・更新できないレコード(週休日数無)です。"
            '    WW_CheckMES2 = ""
            '    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            'Else
            '    CS0036FCHECK.FIELD = "WEEKNISSUTTL"
            '    CS0036FCHECK.VALUE = T0007INProw("WEEKNISSUTTL")
            '    CS0036FCHECK.CS0036FCHECK()
            '    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            '        T0007INProw("WEEKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
            '    Else
            '        'エラーレポート編集
            '        WW_CheckMES1 = "・更新できないレコード(週休日数エラー)です。"
            '        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            '        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            '    End If
            'End If

            ' *** 組欠合計(KUMIKETUNISSUTTL)
            If T0007INProw("KUMIKETUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(組欠日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "KUMIKETUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("KUMIKETUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("KUMIKETUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(組欠日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 遅早合計(CHIKOKSOTAINISSUTTL)
            If T0007INProw("CHIKOKSOTAINISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(遅早日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "CHIKOKSOTAINISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("CHIKOKSOTAINISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("CHIKOKSOTAINISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(遅早日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 代休合計(DAIKYUNISSUTTL)
            If T0007INProw("DAIKYUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(代休日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "DAIKYUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("DAIKYUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("DAIKYUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(代休日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 他欠合計(ETCKETUNISSUTTL)
            If T0007INProw("ETCKETUNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(他欠日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "ETCKETUNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("ETCKETUNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("ETCKETUNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_CheckMES1 = "・更新できないレコード(他欠日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** ストック休暇合計(STOCKNISSUTTL)
            If T0007INProw("STOCKNISSUTTL") = Nothing Then
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_CheckMES1 = "・更新できないレコード(ストック休暇日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "STOCKNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("STOCKNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("STOCKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(ストック休暇日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 年始出勤合計(NENSHINISSUTTL)
            If T0007INProw("NENSHINISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(年始出勤日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "NENSHINISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("NENSHINISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("NENSHINISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(年始出勤日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 宿日直通常合計(SHUKCHOKNISSUTTL)
            If T0007INProw("SHUKCHOKNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(宿日直通常日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHUKCHOKNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("SHUKCHOKNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHUKCHOKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(宿日直通常日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 宿日直年始合計(SHUKCHOKNNISSUTTL)
            If T0007INProw("SHUKCHOKNNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(宿日直年始日数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHUKCHOKNNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("SHUKCHOKNNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHUKCHOKNNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(宿日直年始日数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 車中泊日数合計(SHACHUHAKNISSUTTL)
            If T0007INProw("SHACHUHAKNISSUTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(車中泊回数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHACHUHAKNISSUTTL"
                CS0036FCHECK.VALUE = T0007INProw("SHACHUHAKNISSUTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHACHUHAKNISSUTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(車中泊回数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 洗浄回数合計(SENJYOCNTTTL)
            If T0007INProw("SENJYOCNTTTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(洗浄回数無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SENJYOCNTTTL"
                CS0036FCHECK.VALUE = T0007INProw("SENJYOCNTTTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SENJYOCNTTTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(洗浄回数エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数１合計(UNLOADADDCNT1TTL)
            If T0007INProw("UNLOADADDCNT1TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT1TTL"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT1TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数２合計(UNLOADADDCNT2TTL)
            If T0007INProw("UNLOADADDCNT2TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT2TTL"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT2TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT2TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数２エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品荷卸回数３合計(UNLOADADDCNT3TTL)
            If T0007INProw("UNLOADADDCNT3TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数３無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADADDCNT3TTL"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT3TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADADDCNT3TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数３エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            '' *** 危険品荷卸回数４合計(UNLOADADDCNT4TTL)
            'If T0007INProw("UNLOADADDCNT4TTL") = Nothing Then
            '    'エラーレポート編集
            '    WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数４無)です。"
            '    WW_CheckMES2 = ""
            '    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            'Else
            '    CS0036FCHECK.FIELD = "UNLOADADDCNT4TTL"
            '    CS0036FCHECK.VALUE = T0007INProw("UNLOADADDCNT4TTL")
            '    CS0036FCHECK.CS0036FCHECK()
            '    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
            '        T0007INProw("UNLOADADDCNT4TTL") = Val(CS0036FCHECK.VALUEOUT)
            '    Else
            '        'エラーレポート編集
            '        WW_CheckMES1 = "・更新できないレコード(危険品荷卸回数４エラー)です。"
            '        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
            '        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            '    End If
            'End If

            ' *** 危険品積込回数１合計(LOADINGCNT1TTL)
            If T0007INProw("LOADINGCNT1TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品積込回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "LOADINGCNT1TTL"
                CS0036FCHECK.VALUE = T0007INProw("LOADINGCNT1TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("LOADINGCNT1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品積込回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品積込回数１合計(SHORTDISTANCE1TTL)
            If T0007INProw("SHORTDISTANCE1TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品積込回数１無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHORTDISTANCE1TTL"
                CS0036FCHECK.VALUE = T0007INProw("SHORTDISTANCE1TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHORTDISTANCE1TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品積込回数１エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 危険品積込回数２合計(SHORTDISTANCE2TTL)
            If T0007INProw("SHORTDISTANCE2TTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(危険品積込回数２無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "SHORTDISTANCE2TTL"
                CS0036FCHECK.VALUE = T0007INProw("SHORTDISTANCE2TTL")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("SHORTDISTANCE2TTL") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(危険品積込回数２エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 平日残業時間合計(ORVERTIMETTL)
            If T0007INProw("ORVERTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(平日残業無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("ORVERTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "ORVERTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                        CS0036FCHECK.FIELD = "ORVERTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(平日残業エラー)です。"
                                WW_CheckMES2 = T0007INProw("ORVERTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(平日残業エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(平日残業エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(平日残業エラー)です。"
                    WW_CheckMES2 = T0007INProw("ORVERTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 平日深夜時間合計(WNIGHTTIMETTL)
            If T0007INProw("WNIGHTTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(平日深夜無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("WNIGHTTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "WNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "WNIGHTTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(平日深夜エラー)です。"
                                WW_CheckMES2 = T0007INProw("WNIGHTTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(平日深夜エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(平日深夜エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(平日深夜エラー)です。"
                    WW_CheckMES2 = T0007INProw("WNIGHTTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 所定深夜時間合計(NIGHTTIMETTL)
            If T0007INProw("NIGHTTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(所定深夜無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("NIGHTTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "NIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "NIGHTTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(所定深夜エラー)です。"
                                WW_CheckMES2 = T0007INProw("NIGHTTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(所定深夜エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(所定深夜エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(所定深夜エラー)です。"
                    WW_CheckMES2 = T0007INProw("NIGHTTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 休日出勤時間合計(HWORKTIMETTL)
            If T0007INProw("HWORKTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(休日出勤無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("HWORKTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "HWORKTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "HWORKTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(休日出勤エラー)です。"
                                WW_CheckMES2 = T0007INProw("HWORKTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(休日出勤エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(休日出勤エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(休日出勤エラー)です。"
                    WW_CheckMES2 = T0007INProw("HWORKTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 休日深夜時間合計(HNIGHTTIMETTL)
            If T0007INProw("HNIGHTTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(休日深夜無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("HNIGHTTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "HNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "HNIGHTTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(休日深夜エラー)です。"
                                WW_CheckMES2 = T0007INProw("HNIGHTTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(休日深夜エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(休日深夜エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(休日深夜エラー)です。"
                    WW_CheckMES2 = T0007INProw("HNIGHTTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 日曜出勤時間合計(SWORKTIMETTL)
            If T0007INProw("SWORKTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(日曜出勤無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("SWORKTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "SWORKTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "SWORKTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(日曜出勤エラー)です。"
                                WW_CheckMES2 = T0007INProw("SWORKTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(日曜出勤エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(日曜出勤エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(日曜出勤エラー)です。"
                    WW_CheckMES2 = T0007INProw("SWORKTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 日曜深夜時間合計(SNIGHTTIMETTL)
            If T0007INProw("SNIGHTTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(日曜深夜無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("SNIGHTTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "SNIGHTTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "SNIGHTTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(日曜深夜エラー)です。"
                                WW_CheckMES2 = T0007INProw("SNIGHTTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(日曜深夜エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(日曜深夜エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(日曜深夜エラー)です。"
                    WW_CheckMES2 = T0007INProw("SNIGHTTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 時給者所定内時間合計(JIKYUSHATIMETTL)
            If T0007INProw("JIKYUSHATIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(時給者所定内時間無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("JIKYUSHATIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "JIKYUSHATIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "JIKYUSHATIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(時給者所定内時間エラー)です。"
                                WW_CheckMES2 = T0007INProw("SNIGHTTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(時給者所定内時間エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(時給者所定内時間エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(時給者所定内時間エラー)です。"
                    WW_CheckMES2 = T0007INProw("SNIGHTTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 休憩時間合計(BREAKTIMETTL)　…　計算値

            ' *** 特作Ⅰ合計(TOKUSA1TIMETTL)
            If T0007INProw("TOKUSA1TIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(特作I無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("TOKUSA1TIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then

                    CS0036FCHECK.FIELD = "TOKUSA1TIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "TOKUSA1TIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(特作Iエラー)です。"
                                WW_CheckMES2 = T0007INProw("TOKUSA1TIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(特作Iエラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If

                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(特作Iエラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(特作Iエラー)です。"
                    WW_CheckMES2 = T0007INProw("TOKUSA1TIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 保安検査合計(HOANTIMETTL)
            If T0007INProw("HOANTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(保安検査無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("HOANTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "HOANTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "HOANTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                                WW_CheckMES2 = T0007INProw("HOANTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If

                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                    WW_CheckMES2 = T0007INProw("HOANTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 高圧検査合計(KOATUTIMETTL)
            If T0007INProw("KOATUTIMETTL") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(高圧作業無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                WW_TIMEstr = T0007INProw("KOATUTIMETTL").Split(":")
                If WW_TIMEstr.Length = 2 Then
                    CS0036FCHECK.FIELD = "KOATUTIMETTL"
                    CS0036FCHECK.VALUE = WW_TIMEstr(0)
                    CS0036FCHECK.CS0036FCHECK()
                    If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then

                        CS0036FCHECK.FIELD = "KOATUTIMETTL"
                        CS0036FCHECK.VALUE = WW_TIMEstr(1)
                        CS0036FCHECK.CS0036FCHECK()
                        If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                            If Val(WW_TIMEstr(1)) < 60 Then
                            Else
                                'エラーレポート編集
                                WW_CheckMES1 = "・更新できないレコード(高圧作業エラー)です。"
                                WW_CheckMES2 = T0007INProw("KOATUTIMETTL")
                                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                            End If
                        Else
                            'エラーレポート編集
                            WW_CheckMES1 = "・更新できないレコード(高圧作業エラー)です。"
                            WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                            ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                        End If
                    Else
                        'エラーレポート編集
                        WW_CheckMES1 = "・更新できないレコード(高圧作業エラー)です。"
                        WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                        ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                    End If

                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(高圧作業エラー)です。"
                    WW_CheckMES2 = T0007INProw("KOATUTIMETTL")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸01(UNLOADCNTTTL0101)
            If T0007INProw("UNLOADCNTTTL0101") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸01無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0101"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0101")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0101") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸01エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸02(UNLOADCNTTTL0102)
            If T0007INProw("UNLOADCNTTTL0102") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸02無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0102"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0102")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0102") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸02エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸03(UNLOADCNTTTL0103)
            If T0007INProw("UNLOADCNTTTL0103") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸03無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0103"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0103")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0103") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸03エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸04(UNLOADCNTTTL0104)
            If T0007INProw("UNLOADCNTTTL0104") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸04無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0104"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0104")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0104") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸04エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸05(UNLOADCNTTTL0105)
            If T0007INProw("UNLOADCNTTTL0105") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸05無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0105"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0105")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0105") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸05エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸06(UNLOADCNTTTL0106)
            If T0007INProw("UNLOADCNTTTL0106") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸06無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0106"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0106")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0106") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸06エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸07(UNLOADCNTTTL0107)
            If T0007INProw("UNLOADCNTTTL0107") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸07無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0107"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0107")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0107") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸07エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸08(UNLOADCNTTTL0108)
            If T0007INProw("UNLOADCNTTTL0108") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸08無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0108"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0108")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0108") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸08エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸09(UNLOADCNTTTL0109)
            If T0007INProw("UNLOADCNTTTL0109") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸09無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0109"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0109")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0109") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸09エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・荷卸10(UNLOADCNTTTL0110)
            If T0007INProw("UNLOADCNTTTL0110") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・荷卸10無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0110"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0110")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0110") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・荷卸10エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸01(UNLOADCNTTTL0201)
            If T0007INProw("UNLOADCNTTTL0201") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸01無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0201"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0201")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0201") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸01エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸02(UNLOADCNTTTL0202)
            If T0007INProw("UNLOADCNTTTL0202") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸02無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0202"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0202")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0202") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸02エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸03(UNLOADCNTTTL0203)
            If T0007INProw("UNLOADCNTTTL0203") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸03無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0203"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0203")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0203") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸03エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸04(UNLOADCNTTTL0204)
            If T0007INProw("UNLOADCNTTTL0204") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸04無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0204"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0204")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0204") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸04エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸05(UNLOADCNTTTL0205)
            If T0007INProw("UNLOADCNTTTL0205") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸05無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0205"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0205")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0205") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸05エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸06(UNLOADCNTTTL0206)
            If T0007INProw("UNLOADCNTTTL0206") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸06無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0206"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0206")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0206") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸06エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸07(UNLOADCNTTTL0207)
            If T0007INProw("UNLOADCNTTTL0207") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸07無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0207"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0207")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0207") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸07エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸08(UNLOADCNTTTL0208)
            If T0007INProw("UNLOADCNTTTL0208") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸08無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0208"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0208")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0208") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸08エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸09(UNLOADCNTTTL0209)
            If T0007INProw("UNLOADCNTTTL0209") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸09無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0209"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0209")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0209") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸09エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・荷卸10(UNLOADCNTTTL0210)
            If T0007INProw("UNLOADCNTTTL0210") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸10無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "UNLOADCNTTTL0210"
                CS0036FCHECK.VALUE = T0007INProw("UNLOADCNTTTL0210")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("UNLOADCNTTTL0210") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・荷卸10エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離01(HAIDISTANCETTL0101)
            If T0007INProw("HAIDISTANCETTL0101") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離01無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0101"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0101")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0101") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離01エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離02(HAIDISTANCETTL0102)
            If T0007INProw("HAIDISTANCETTL0102") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離02無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0102"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0102")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0102") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離02エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離03(HAIDISTANCETTL0103)
            If T0007INProw("HAIDISTANCETTL0103") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離03無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0103"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0103")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0103") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離03エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離04(HAIDISTANCETTL0104)
            If T0007INProw("HAIDISTANCETTL0104") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離04無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0104"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0104")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0104") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離04エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離05(HAIDISTANCETTL0105)
            If T0007INProw("HAIDISTANCETTL0105") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離05無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0105"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0105")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0105") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離05エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離06(HAIDISTANCETTL0106)
            If T0007INProw("HAIDISTANCETTL0106") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離06無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0106"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0106")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0106") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離06エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離07(HAIDISTANCETTL0107)
            If T0007INProw("HAIDISTANCETTL0107") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離07無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0107"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0107")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0107") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離07エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離08(HAIDISTANCETTL0108)
            If T0007INProw("HAIDISTANCETTL0108") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離08無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0108"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0108")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0108") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離08エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離09(HAIDISTANCETTL0109)
            If T0007INProw("HAIDISTANCETTL0109") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離09無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0109"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0109")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0109") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離09エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** 単車・配送距離10(HAIDISTANCETTL0110)
            If T0007INProw("HAIDISTANCETTL0110") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(単車・配送距離10無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0110"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0110")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0110") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(単車・配送距離10エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離01(HAIDISTANCETTL0201)
            If T0007INProw("HAIDISTANCETTL0201") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離01無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0201"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0201")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0201") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離01エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離02(HAIDISTANCETTL0202)
            If T0007INProw("HAIDISTANCETTL0202") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離02無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0202"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0202")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0202") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離02エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離03(HAIDISTANCETTL0203)
            If T0007INProw("HAIDISTANCETTL0203") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離03無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0203"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0203")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0203") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離03エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離04(HAIDISTANCETTL0204)
            If T0007INProw("HAIDISTANCETTL0204") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離04無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0204"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0204")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0204") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離04エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離05(HAIDISTANCETTL0205)
            If T0007INProw("HAIDISTANCETTL0205") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離05無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0205"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0205")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0205") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離05エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離06(HAIDISTANCETTL0206)
            If T0007INProw("HAIDISTANCETTL0206") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離06無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0206"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0206")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0206") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離06エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離07(HAIDISTANCETTL0207)
            If T0007INProw("HAIDISTANCETTL0207") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離07無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0207"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0207")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0207") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離07エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離08(HAIDISTANCETTL0208)
            If T0007INProw("HAIDISTANCETTL0208") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離08エラー)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0208"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0208")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0208") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離08エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離09(HAIDISTANCETTL0209)
            If T0007INProw("HAIDISTANCETTL0209") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離09無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0209"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0209")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0209") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離09エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If

            ' *** トレーラ・配送距離10(HAIDISTANCETTL0210)
            If T0007INProw("HAIDISTANCETTL0210") = Nothing Then
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離10無)です。"
                WW_CheckMES2 = ""
                ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
            Else
                CS0036FCHECK.FIELD = "HAIDISTANCETTL0210"
                CS0036FCHECK.VALUE = T0007INProw("HAIDISTANCETTL0210")
                CS0036FCHECK.CS0036FCHECK()
                If CS0036FCHECK.ERR = C_MESSAGE_NO.NORMAL Then
                    T0007INProw("HAIDISTANCETTL0210") = Val(CS0036FCHECK.VALUEOUT)
                Else
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(トレーラ・配送距離10エラー)です。"
                    WW_CheckMES2 = CS0036FCHECK.CHECKREPORT
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If


        End If


        '■■■ 関連チェック(日別情報) ■■■

        If T0007INProw("RECODEKBN") = "0" Then
            If IsDate(T0007INProw("STDATE")) And IsDate(T0007INProw("STTIME")) And
                IsDate(T0007INProw("ENDDATE")) And IsDate(T0007INProw("ENDTIME")) Then
                Dim WW_STDATE As Date = CDate(T0007INProw("STDATE") & " " & T0007INProw("STTIME"))
                Dim WW_ENDDATE As Date = CDate(T0007INProw("ENDDATE") & " " & T0007INProw("ENDTIME"))
                If WW_STDATE > WW_ENDDATE Then
                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(開始時刻　＞　終了時刻)です。"
                    WW_CheckMES2 = WW_STDATE.ToString("yyyy/MM/dd HH:mm") & " > " & WW_ENDDATE.ToString("yyyy/MM/dd HH:mm")
                    ERRMSG_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, "10023")
                End If
            End If
        End If


        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf("10023") >= 0 Then
                RTN = "10023"
            ElseIf WW_ERRLIST.IndexOf("10018") >= 0 Then
                RTN = "10018"
            End If
        End If

    End Sub

    ' ***  エラーレポート編集
    Protected Sub ERRMSG_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByVal I_ERRCD As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 勤務年月日  =" & T0007INProw("WORKDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員コード=" & T0007INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員      =" & T0007INProw("STAFFNAMES") & " , "

        rightview.addErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> "10023" Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ' ***  T0005データ取得処理
    Public Sub T00005ALLget(ByVal iSTAFFCODE As String, ByVal iYmdFrom As String, ByVal iYmdTo As String, ByRef oTbl As DataTable, ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL
        '■ 画面表示用データ取得

        'オブジェクト内容検索
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            T0007COM.T0005tbl_ColumnsAdd(oTbl)

            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr0 As String = ""
            Dim SQLStr01 As String = ""

            'テンポラリーテーブルを作成する
            SQLStr0 = "CREATE TABLE #MBtemp " _
                    & " ( " _
                    & "  CAMPCODE nvarchar(20)," _
                    & "  STAFFCODE nvarchar(20)," _
                    & "  HORG nvarchar(20)," _
                    & " ) "

            Dim SQLcmd1 As New SqlCommand(SQLStr0, SQLcon)
            SQLcmd1.CommandTimeout = 300
            SQLcmd1.ExecuteNonQuery()
            SQLcmd1.Dispose()
            SQLcmd1 = Nothing

            SQLStr0 = " SELECT  isnull(rtrim(MB1.CAMPCODE),'')      as  CAMPCODE,                                       " _
               & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE                              " _
               & " from   MB001_STAFF MB1                                                                 " _
               & " INNER JOIN S0012_SRVAUTHOR X " _
               & "   ON    X.TERMID      = @P6 " _
               & "   and   X.CAMPCODE    = @P1 " _
               & "   and   X.OBJECT      = 'SRVORG' " _
               & "   and   X.STYMD      <= @P5 " _
               & "   and   X.ENDYMD     >= @P5 " _
               & "   and   X.DELFLG     <> '1' " _
               & " INNER JOIN S0006_ROLE Y " _
               & "   ON    Y.CAMPCODE     = X.CAMPCODE " _
               & "   and   Y.OBJECT       = 'SRVORG' " _
               & "   and   Y.ROLE         = X.ROLE" _
               & "   and   Y.STYMD       <= @P5 " _
               & "   and   Y.ENDYMD      >= @P5 " _
               & "   and   Y.DELFLG      <> '1' " _
               & " INNER JOIN (select CODE from M0006_STRUCT ORG " _
               & "             where ORG.CAMPCODE = @P1 " _
               & "              and  ORG.OBJECT   = 'ORG' " _
               & "              and  ORG.STRUCT   = '勤怠管理組織' " _
               & "              and  ORG.GRCODE01 = @P2 " _
               & "              and  ORG.STYMD   <= @P5 " _
               & "              and  ORG.ENDYMD  >= @P5 " _
               & "              and  ORG.DELFLG  <> '1'  " _
               & "            ) Z " _
               & "   ON   Z.CODE   = Y.CODE   " _
               & "   and  Z.CODE   = MB1.HORG "

            If iSTAFFCODE = "" Then
                SQLStr01 = " where  MB1.CAMPCODE                       =  @P1                       " _
                       & "     and  MB1.STYMD                         <=  @P3                       " _
                       & "     and  MB1.ENDYMD                        >=  @P4                       " _
                       & "     and  MB1.DELFLG                        <>  '1'                       " _
                       & " group by MB1.CAMPCODE, MB1.STAFFCODE                                     "
            Else
                SQLStr01 = " where  MB1.CAMPCODE                       =  @P1                       " _
                       & "     and  MB1.STAFFCODE                      =  @P7                       " _
                       & "     and  MB1.STYMD                         <=  @P3                       " _
                       & "     and  MB1.ENDYMD                        >=  @P4                       " _
                       & "     and  MB1.DELFLG                        <>  '1'                       " _
                       & " group by MB1.CAMPCODE, MB1.STAFFCODE                                     "
            End If

            Dim WW_MBtbl As DataTable = New DataTable
            WW_MBtbl.Columns.Add("CAMPCODE", GetType(String))
            WW_MBtbl.Columns.Add("STAFFCODE", GetType(String))

            SQLStr0 = SQLStr0 & SQLStr01
            Dim SQLcmd2 As New SqlCommand(SQLStr0, SQLcon)
            Dim P_CAMP As SqlParameter = SQLcmd2.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim P_HORG As SqlParameter = SQLcmd2.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            Dim P_END As SqlParameter = SQLcmd2.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim P_ST As SqlParameter = SQLcmd2.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim P_NOW As SqlParameter = SQLcmd2.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim P_TERM As SqlParameter = SQLcmd2.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
            Dim P_STAFF As SqlParameter = SQLcmd2.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar)
            P_CAMP.Value = work.WF_T7SEL_CAMPCODE.Text
            P_HORG.Value = work.WF_T7SEL_HORG.Text
            P_END.Value = iYmdTo
            P_ST.Value = iYmdFrom
            P_NOW.Value = Date.Now
            P_TERM.Value = CS0050SESSION.APSV_ID
            P_STAFF.Value = iSTAFFCODE

            SQLcmd2.CommandTimeout = 300
            Dim SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

            WW_MBtbl.Load(SQLdr2)

            '一旦テンポラリテーブルに出力
            Dim bc As New SqlClient.SqlBulkCopy(SQLcon)
            bc.DestinationTableName = "#MBtemp"
            bc.WriteToServer(WW_MBtbl)

            SQLcmd2.Dispose()
            SQLcmd2 = Nothing
            bc.Close()
            bc = Nothing


            Dim SQLStr As String =
                  "SELECT 0 as LINECNT , " _
                & "       '' as OPERATION , " _
                & "       '1' as HIDDEN , " _
                & "       TIMSTP = cast(A.UPDTIMSTP as bigint) , " _
                & "       isnull(rtrim(A.CAMPCODE),'')  as CAMPCODE, " _
                & "       isnull(rtrim(A.SHIPORG),'') as SHIPORG , " _
                & "       '' as SHIPORGNAMES , " _
                & "       isnull(rtrim(A.TERMKBN),'') as TERMKBN, " _
                & "       '' as TERMKBNNAMES , " _
                & "       isnull(rtrim(A.YMD),'') as YMD , " _
                & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
                & "       isnull(rtrim(A.NIPPONO),'') as NIPPONO , " _
                & "       isnull(A.SEQ,'0') as SEQ , " _
                & "       isnull(rtrim(A.WORKKBN),'') as WORKKBN , " _
                & "       isnull(rtrim(F1.VALUE2),'') as WORKKBNNAMES , " _
                & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE , " _
                & "       isnull(rtrim(B3.STAFFNAMES),'') as STAFFNAMES , " _
                & "       isnull(rtrim(A.SUBSTAFFCODE),'') as SUBSTAFFCODE , " _
                & "       isnull(rtrim(B2.STAFFNAMES),'') as SUBSTAFFNAMES , " _
                & "       isnull(rtrim(A.CREWKBN),'') as CREWKBN , " _
                & "       '' as CREWKBNNAMES , " _
                & "       isnull(rtrim(A.GSHABAN),'') as GSHABAN , " _
                & "       isnull(rtrim(MA4.LICNPLTNO2),'') as GSHABANLICNPLTNO , " _
                & "       isnull(rtrim(A.STDATE),'')  as STDATE , " _
                & "       isnull(rtrim(A.STTIME),'')  as STTIME , " _
                & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE , " _
                & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME , " _
                & "       isnull(rtrim(A.WORKTIME),'') as WORKTIME , " _
                & "       isnull(rtrim(A.MOVETIME),'') as MOVETIME , " _
                & "       isnull(rtrim(A.ACTTIME),'') as ACTTIME , " _
                & "       isnull(A.PRATE,'0') as PRATE , " _
                & "       isnull(A.CASH,'0') as CASH , " _
                & "       isnull(A.TICKET,'0') as TICKET , " _
                & "       isnull(A.ETC,'0') as ETC , " _
                & "       isnull(A.TOTALTOLL,'0') as TOTALTOLL , " _
                & "       isnull(A.STMATER,'0') as STMATER , " _
                & "       isnull(A.ENDMATER,'0') as ENDMATER , " _
                & "       isnull(A.RUIDISTANCE,'0') as RUIDISTANCE , " _
                & "       isnull(A.SOUDISTANCE,'0') as SOUDISTANCE , " _
                & "       isnull(A.JIDISTANCE,'0') as JIDISTANCE , " _
                & "       isnull(A.KUDISTANCE,'0') as KUDISTANCE , " _
                & "       isnull(A.IPPDISTANCE,'0') as IPPDISTANCE , " _
                & "       isnull(A.KOSDISTANCE,'0') as KOSDISTANCE , " _
                & "       isnull(A.IPPJIDISTANCE,'0') as IPPJIDISTANCE , " _
                & "       isnull(A.IPPKUDISTANCE,'0') as IPPKUDISTANCE , " _
                & "       isnull(A.KOSJIDISTANCE,'0') as KOSJIDISTANCE , " _
                & "       isnull(A.KOSKUDISTANCE,'0') as KOSKUDISTANCE , " _
                & "       isnull(A.KYUYU,'0') as KYUYU , " _
                & "       isnull(rtrim(A.TORICODE),'') as TORICODE , " _
                & "       isnull(rtrim(A.SHUKABASHO),'') as SHUKABASHO , " _
                & "       '' as SHUKABASHONAMES , " _
                & "       isnull(rtrim(A.TODOKECODE),'') as TODOKECODE , " _
                & "       '' as TODOKENAMES , " _
                & "       isnull(rtrim(A.TODOKEDATE),'') as TODOKEDATE , " _
                & "       isnull(rtrim(A.OILTYPE1),'') as OILTYPE1 , " _
                & "       isnull(rtrim(A.PRODUCT11),'') as PRODUCT11 , " _
                & "       isnull(rtrim(A.PRODUCT21),'') as PRODUCT21 , " _
                & "       isnull(rtrim(F41.VALUE1),'') as PRODUCT1NAMES , " _
                & "       isnull(rtrim(A.STANI1),'') as STANI1 , " _
                & "       '' as STANI1NAMES , " _
                & "       isnull(A.SURYO1,'0') as SURYO1 , " _
                & "       isnull(rtrim(A.OILTYPE2),'') as OILTYPE2 , " _
                & "       isnull(rtrim(A.PRODUCT12),'') as PRODUCT12 , " _
                & "       isnull(rtrim(A.PRODUCT22),'') as PRODUCT22 , " _
                & "       isnull(rtrim(F42.VALUE1),'') as PRODUCT2NAMES , " _
                & "       isnull(rtrim(A.STANI2),'') as STANI2 , " _
                & "       '' as STANI2NAMES , " _
                & "       isnull(A.SURYO2,'0') as SURYO2 , " _
                & "       isnull(rtrim(A.OILTYPE3),'') as OILTYPE3 , " _
                & "       isnull(rtrim(A.PRODUCT13),'') as PRODUCT13 , " _
                & "       isnull(rtrim(A.PRODUCT23),'') as PRODUCT23 , " _
                & "       isnull(rtrim(F43.VALUE1),'') as PRODUCT3NAMES , " _
                & "       isnull(rtrim(A.STANI3),'') as STANI3 , " _
                & "       '' as STANI3NAMES , " _
                & "       isnull(A.SURYO3,'0') as SURYO3 , " _
                & "       isnull(rtrim(A.OILTYPE4),'') as OILTYPE4 , " _
                & "       isnull(rtrim(A.PRODUCT14),'') as PRODUCT14 , " _
                & "       isnull(rtrim(A.PRODUCT24),'') as PRODUCT24 , " _
                & "       isnull(rtrim(F44.VALUE1),'') as PRODUCT4NAMES , " _
                & "       isnull(rtrim(A.STANI4),'') as STANI4 , " _
                & "       '' as STANI4NAMES , " _
                & "       isnull(A.SURYO4,'0') as SURYO4 , " _
                & "       isnull(rtrim(A.OILTYPE5),'') as OILTYPE5 , " _
                & "       isnull(rtrim(A.PRODUCT15),'') as PRODUCT15 , " _
                & "       isnull(rtrim(A.PRODUCT25),'') as PRODUCT25 , " _
                & "       isnull(rtrim(F45.VALUE1),'') as PRODUCT5NAMES , " _
                & "       isnull(rtrim(A.STANI5),'') as STANI5 , " _
                & "       '' as STANI5NAMES , " _
                & "       isnull(A.SURYO5,'0') as SURYO5 , " _
                & "       isnull(rtrim(A.OILTYPE6),'') as OILTYPE6 , " _
                & "       isnull(rtrim(A.PRODUCT16),'') as PRODUCT16 , " _
                & "       isnull(rtrim(A.PRODUCT26),'') as PRODUCT26 , " _
                & "       isnull(rtrim(F46.VALUE1),'') as PRODUCT6NAMES , " _
                & "       isnull(rtrim(A.STANI6),'') as STANI6 , " _
                & "       '' as STANI6NAMES , " _
                & "       isnull(A.SURYO6,'0') as SURYO6 , " _
                & "       isnull(rtrim(A.OILTYPE7),'') as OILTYPE7 , " _
                & "       isnull(rtrim(A.PRODUCT17),'') as PRODUCT17 , " _
                & "       isnull(rtrim(A.PRODUCT27),'') as PRODUCT27 , " _
                & "       isnull(rtrim(F47.VALUE1),'') as PRODUCT7NAMES , " _
                & "       isnull(rtrim(A.STANI7),'') as STANI7 , " _
                & "       '' as STANI7NAMES , " _
                & "       isnull(A.SURYO7,'0') as SURYO7 , " _
                & "       isnull(rtrim(A.OILTYPE8),'') as OILTYPE8 , " _
                & "       isnull(rtrim(A.PRODUCT18),'') as PRODUCT18 , " _
                & "       isnull(rtrim(A.PRODUCT28),'') as PRODUCT28 , " _
                & "       isnull(rtrim(F48.VALUE1),'') as PRODUCT8NAMES , " _
                & "       isnull(rtrim(A.STANI8),'') as STANI8 , " _
                & "       '' as STANI8NAMES , " _
                & "       isnull(A.SURYO8,'0') as SURYO8 , " _
                & "       isnull(A.TOTALSURYO,'0') as TOTALSURYO , " _
                & "       isnull(rtrim(A.TUMIOKIKBN),'') as TUMIOKIKBN , " _
                & "       '' as TUMIOKIKBNNAMES , " _
                & "       isnull(rtrim(A.ORDERNO),'') as ORDERNO , " _
                & "       isnull(rtrim(A.DETAILNO),'') as DETAILNO , " _
                & "       isnull(rtrim(A.TRIPNO),'') as TRIPNO , " _
                & "       isnull(rtrim(A.DROPNO),'') as DROPNO , " _
                & "       isnull(rtrim(A.JISSKIKBN),'') as JISSKIKBN , " _
                & "       '' as JISSKIKBNNAMES , " _
                & "       isnull(rtrim(A.URIKBN),'') as URIKBN , " _
                & "       '' as URIKBNNAMES , " _
                & "       isnull(rtrim(A.DELFLG),'') as DELFLG , " _
                & "       isnull(rtrim(A.SHARYOTYPEF),'') as SHARYOTYPEF , " _
                & "       isnull(rtrim(A.TSHABANF),'') as TSHABANF , " _
                & "       isnull(rtrim(A.SHARYOTYPEB),'') as SHARYOTYPEB , " _
                & "       isnull(rtrim(A.TSHABANB),'') as TSHABANB , " _
                & "       isnull(rtrim(A.SHARYOTYPEB2),'') as SHARYOTYPEB2 , " _
                & "       isnull(rtrim(A.TSHABANB2),'') as TSHABANB2 , " _
                & "       isnull(rtrim(A.TAXKBN),'') as TAXKBN , " _
                & "       '' as TAXKBNNAMES , " _
                & "       isnull(rtrim(A.LATITUDE),'') as LATITUDE , " _
                & "       isnull(rtrim(A.LONGITUDE),'') as LONGITUDE , " _
                & "       isnull(rtrim(MA6.SHARYOKBN),'') as SHARYOKBN , " _
                & "       isnull(rtrim(F2.VALUE1),'') as SHARYOKBNNAMES , " _
                & "       case when F10.VALUE1 is null then   " _
                & "            isnull(rtrim(MA6.OILKBN),'') " _
                & "       else " _
                & "            isnull(rtrim(F10.VALUE1),'') " _
                & "       END  as OILPAYKBN , " _
                & "       case when F10.VALUE1 is null then   " _
                & "            isnull(rtrim(F5.VALUE1),'')  " _
                & "       else " _
                & "            isnull(rtrim(F5.VALUE1),'')  " _
                & "       END as OILPAYKBNNAMES , " _
                & "       isnull(rtrim(MA6.SUISOKBN),'0') as SUISOKBN , " _
                & "       isnull(rtrim(F6.VALUE1),'') as SUISOKBNNAMES , " _
                & "       isnull(rtrim(A.L1KAISO),'') as L1KAISO , " _
                & "       isnull(rtrim(CAL.WORKINGWEEK),'') as WORKINGWEEK , " _
                & "       isnull(rtrim(F7.VALUE1),'') as WORKINGWEEKNAMES , " _
                & "       isnull(rtrim(CAL.WORKINGKBN),'') as HOLIDAYKBN , " _
                & "       isnull(rtrim(F8.VALUE1),'') as HOLIDAYKBNNAMES , " _
                & "       isnull(rtrim(B3.MORG),'') as MORG , " _
                & "       isnull(rtrim(M2M.NAMES),'') as MORGNAMES , " _
                & "       isnull(rtrim(B3.HORG),'') as HORG , " _
                & "       isnull(rtrim(M2H.NAMES),'') as HORGNAMES , " _
                & "       isnull(rtrim(A.SHIPORG),'') as SORG , " _
                & "       isnull(rtrim(M2S.NAMES),'') as SORGNAMES , " _
                & "       isnull(rtrim(B3.STAFFKBN),'') as STAFFKBN , " _
                & "       isnull(rtrim(F9.VALUE1),'') as STAFFKBNNAMES , " _
                & "       isnull(rtrim(P1.MODEL),'0') as MODELDISTANCE1 , " _
                & "       isnull(rtrim(P2.MODEL),'0') as MODELDISTANCE2 , " _
                & "       isnull(rtrim(P3.MODEL),'0') as MODELDISTANCE3 , " _
                & "       isnull(rtrim(A.L1HAISOGROUP),'') as wHaisoGroup , " _
                & "       isnull(rtrim(MD21.UNLOADADDTANKA),'0') as UNLOADADDTANKA , " _
                & "       isnull(rtrim(MD22.LOADINGTANKA),'0') as LOADINGTANKA , " _
                & "       isnull(rtrim(format(A.UPDYMD,'yyyyMMddHHmmss')),'') as UPDYMD " _
                & " FROM #MBtemp B " _
                & " INNER JOIN T0005_NIPPO A " _
                & "   ON    A.CAMPCODE    = @P1 " _
                & "   and   A.STAFFCODE   = B.STAFFCODE " _
                & "   and   A.YMD        <= @P3 " _
                & "   and   A.YMD        >= @P4 " _
                & "   and   A.DELFLG     <> '1' " _
                & " LEFT JOIN MB001_STAFF B2 " _
                & "   ON    B2.CAMPCODE    = A.CAMPCODE " _
                & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
                & "   and   B2.STYMD      <= A.YMD " _
                & "   and   B2.ENDYMD     >= A.YMD " _
                & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                & "   and   B2.DELFLG     <> '1' " _
                & " LEFT JOIN MB001_STAFF B3 " _
                & "   ON    B3.CAMPCODE    = A.CAMPCODE " _
                & "   and   B3.STAFFCODE   = A.STAFFCODE " _
                & "   and   B3.STYMD      <= A.YMD " _
                & "   and   B3.ENDYMD     >= A.YMD " _
                & "   and   B3.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.STAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                & "   and   B3.DELFLG     <> '1' " _
                & " LEFT JOIN M0002_ORG M2M " _
                & "   ON    M2M.CAMPCODE   = A.CAMPCODE " _
                & "   and   M2M.ORGCODE    = B3.MORG " _
                & "   and   M2M.STYMD      <= A.YMD " _
                & "   and   M2M.ENDYMD     >= A.YMD " _
                & "   and   M2M.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B3.MORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                & "   and   M2M.DELFLG     <> '1' " _
                & " LEFT JOIN M0002_ORG M2H " _
                & "   ON    M2H.CAMPCODE   = A.CAMPCODE " _
                & "   and   M2H.ORGCODE    = B3.HORG " _
                & "   and   M2H.STYMD      <= A.YMD " _
                & "   and   M2H.ENDYMD     >= A.YMD " _
                & "   and   M2H.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B3.HORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                & "   and   M2H.DELFLG     <> '1' " _
                & " LEFT JOIN M0002_ORG M2S " _
                & "   ON    M2S.CAMPCODE   = A.CAMPCODE " _
                & "   and   M2S.ORGCODE    = A.SHIPORG " _
                & "   and   M2S.STYMD      <= A.YMD " _
                & "   and   M2S.ENDYMD     >= A.YMD " _
                & "   and   M2S.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                & "   and   M2S.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F1 " _
                & "   ON    F1.CAMPCODE    = @P1 " _
                & "   and   F1.CLASS       = 'WORKKBN' " _
                & "   and   F1.KEYCODE     = A.WORKKBN " _
                & "   and   F1.STYMD      <= @P5 " _
                & "   and   F1.ENDYMD     >= @P5 " _
                & "   and   F1.DELFLG     <> '1' " _
                & " LEFT JOIN MA006_SHABANORG MA6 " _
                & "   ON    MA6.CAMPCODE    = A.CAMPCODE " _
                & "   and   MA6.MANGUORG    = A.SHIPORG " _
                & "   and   MA6.GSHABAN     = A.GSHABAN " _
                & "   and   MA6.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F2 " _
                & "   ON    F2.CAMPCODE    = @P1 " _
                & "   and   F2.CLASS       = 'SHARYOKBN' " _
                & "   and   F2.KEYCODE     = MA6.SHARYOKBN " _
                & "   and   F2.STYMD      <= @P5 " _
                & "   and   F2.ENDYMD     >= @P5 " _
                & "   and   F2.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F5 " _
                & "   ON    F5.CAMPCODE    = @P1 " _
                & "   and   F5.CLASS       = 'OILPAYKBN' " _
                & "   and   F5.KEYCODE     = MA6.OILKBN " _
                & "   and   F5.STYMD      <= @P5 " _
                & "   and   F5.ENDYMD     >= @P5 " _
                & "   and   F5.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F41 " _
                & "   ON    F41.CAMPCODE    = @P1 " _
                & "   and   F41.CLASS       = 'PRODUCT1' " _
                & "   and   F41.KEYCODE     = A.PRODUCT11 " _
                & "   and   F41.STYMD      <= @P5 " _
                & "   and   F41.ENDYMD     >= @P5 " _
                & "   and   F41.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F42 " _
                & "   ON    F42.CAMPCODE    = @P1 " _
                & "   and   F42.CLASS       = 'PRODUCT1' " _
                & "   and   F42.KEYCODE     = A.PRODUCT12 " _
                & "   and   F42.STYMD      <= @P5 " _
                & "   and   F42.ENDYMD     >= @P5 " _
                & "   and   F42.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F43 " _
                & "   ON    F43.CAMPCODE    = @P1 " _
                & "   and   F43.CLASS       = 'PRODUCT1' " _
                & "   and   F43.KEYCODE     = A.PRODUCT13 " _
                & "   and   F43.STYMD      <= @P5 " _
                & "   and   F43.ENDYMD     >= @P5 " _
                & "   and   F43.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F44 " _
                & "   ON    F44.CAMPCODE    = @P1 " _
                & "   and   F44.CLASS       = 'PRODUCT1' " _
                & "   and   F44.KEYCODE     = A.PRODUCT14 " _
                & "   and   F44.STYMD      <= @P5 " _
                & "   and   F44.ENDYMD     >= @P5 " _
                & "   and   F44.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F45 " _
                & "   ON    F45.CAMPCODE    = @P1 " _
                & "   and   F45.CLASS       = 'PRODUCT1' " _
                & "   and   F45.KEYCODE     = A.PRODUCT15 " _
                & "   and   F45.STYMD      <= @P5 " _
                & "   and   F45.ENDYMD     >= @P5 " _
                & "   and   F45.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F46 " _
                & "   ON    F46.CAMPCODE    = @P1 " _
                & "   and   F46.CLASS       = 'PRODUCT1' " _
                & "   and   F46.KEYCODE     = A.PRODUCT16 " _
                & "   and   F46.STYMD      <= @P5 " _
                & "   and   F46.ENDYMD     >= @P5 " _
                & "   and   F46.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F47 " _
                & "   ON    F47.CAMPCODE    = @P1 " _
                & "   and   F47.CLASS       = 'PRODUCT1' " _
                & "   and   F47.KEYCODE     = A.PRODUCT17 " _
                & "   and   F47.STYMD      <= @P5 " _
                & "   and   F47.ENDYMD     >= @P5 " _
                & "   and   F47.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F48 " _
                & "   ON    F48.CAMPCODE    = @P1 " _
                & "   and   F48.CLASS       = 'PRODUCT1' " _
                & "   and   F48.KEYCODE     = A.PRODUCT18 " _
                & "   and   F48.STYMD      <= @P5 " _
                & "   and   F48.ENDYMD     >= @P5 " _
                & "   and   F48.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F6 " _
                & "   ON    F6.CAMPCODE    = @P1 " _
                & "   and   F6.CLASS       = 'SUISOKBN' " _
                & "   and   F6.KEYCODE     = isnull(MA6.SUISOKBN,'0') " _
                & "   and   F6.STYMD      <= @P5 " _
                & "   and   F6.ENDYMD     >= @P5 " _
                & "   and   F6.DELFLG     <> '1' " _
                & " LEFT JOIN MA004_SHARYOC MA4 " _
                & "   ON    MA4.CAMPCODE    = A.CAMPCODE " _
                & "   and   MA4.SHARYOTYPE  = A.SHARYOTYPEF " _
                & "   and   MA4.TSHABAN     = A.TSHABANF " _
                & "   and   MA4.STYMD      <= A.YMD " _
                & "   and   MA4.ENDYMD     >= A.YMD " _
                & "   and   MA4.DELFLG     <> '1' " _
                & " LEFT JOIN MB005_CALENDAR CAL " _
                & "   ON    CAL.CAMPCODE    = A.CAMPCODE " _
                & "   and   CAL.WORKINGYMD  = A.YMD " _
                & "   and   CAL.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F7 " _
                & "   ON    F7.CAMPCODE    = @P1 " _
                & "   and   F7.CLASS       = 'WORKINGWEEK' " _
                & "   and   F7.KEYCODE     = CAL.WORKINGWEEK " _
                & "   and   F7.STYMD      <= @P5 " _
                & "   and   F7.ENDYMD     >= @P5 " _
                & "   and   F7.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F8 " _
                & "   ON    F8.CAMPCODE    = @P1 " _
                & "   and   F8.CLASS       = 'HOLIDAYKBN' " _
                & "   and   F8.KEYCODE     = CAL.WORKINGKBN " _
                & "   and   F8.STYMD      <= @P5 " _
                & "   and   F8.ENDYMD     >= @P5 " _
                & "   and   F8.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F9 " _
                & "   ON    F9.CAMPCODE    = @P1 " _
                & "   and   F9.CLASS       = 'STAFFKBN' " _
                & "   and   F9.KEYCODE     = B3.STAFFKBN " _
                & "   and   F9.STYMD      <= @P5 " _
                & "   and   F9.ENDYMD     >= @P5 " _
                & "   and   F9.DELFLG     <> '1' " _
                & " LEFT JOIN MC012_MODEL P1 " _
                & "   ON    P1.CAMPCODE    = A.CAMPCODE " _
                & "   and   P1.UORG        = A.SHIPORG " _
                & "   and   P1.MODELPATTERN= '1' " _
                & "   and   P1.TODOKECODE  = A.TODOKECODE " _
                & "   and   P1.DELFLG     <> '1' " _
                & "   and   A.WORKKBN      = 'B3' " _
                & " LEFT JOIN MC012_MODEL P2 " _
                & "   ON    P2.CAMPCODE    = A.CAMPCODE " _
                & "   and   P2.UORG        = A.SHIPORG " _
                & "   and   P2.MODELPATTERN= '2' " _
                & "   and   P2.SHUKABASHO  = A.SHUKABASHO " _
                & "   and   P2.TODOKECODE  = A.TODOKECODE " _
                & "   and   P2.DELFLG     <> '1' " _
                & "   and   A.WORKKBN      = 'B3' " _
                & " LEFT JOIN MC012_MODEL P3 " _
                & "   ON    P3.CAMPCODE    = A.CAMPCODE " _
                & "   and   P3.UORG        = A.SHIPORG " _
                & "   and   P3.MODELPATTERN= '3' " _
                & "   and   P3.SHUKABASHO  = A.SHUKABASHO " _
                & "   and   P3.DELFLG     <> '1' " _
                & "   and   A.WORKKBN      = 'B2' " _
                & " LEFT JOIN MD002_PRODORG MD21 " _
                & "   ON    MD21.CAMPCODE    = A.CAMPCODE " _
                & "   and   MD21.UORG        = A.SHIPORG " _
                & "   and   'B3'             = A.WORKKBN " _
                & "   and   MD21.PRODUCTCODE = A.CAMPCODE + A.OILTYPE1 + A.PRODUCT11 + A.PRODUCT21 " _
                & "   and   MD21.STYMD      <= @P5 " _
                & "   and   MD21.ENDYMD     >= @P5 " _
                & "   and   MD21.DELFLG     <> '1' " _
                & " LEFT JOIN MD002_PRODORG MD22 " _
                & "   ON    MD22.CAMPCODE    = A.CAMPCODE " _
                & "   and   MD22.UORG        = A.SHIPORG " _
                & "   and   'B2'             = A.WORKKBN " _
                & "   and   MD22.PRODUCTCODE = A.CAMPCODE + A.OILTYPE1 + A.PRODUCT11 + A.PRODUCT21 " _
                & "   and   MD22.STYMD      <= @P5 " _
                & "   and   MD22.ENDYMD     >= @P5 " _
                & "   and   MD22.DELFLG     <> '1' " _
                & " LEFT JOIN MC001_FIXVALUE F10 " _
                & "   ON    F10.CAMPCODE    = @P1 " _
                & "   and   F10.CLASS       = 'PAYTORICODE' " _
                & "   and   F10.KEYCODE     = A.L1TORICODE " _
                & "   and   F10.STYMD      <= @P5 " _
                & "   and   F10.ENDYMD     >= @P5 " _
                & "   and   F10.DELFLG     <> '1' " _
                & " WHERE   B.CAMPCODE    = @P1 " _
                & " ORDER BY A.YMD , A.STAFFCODE , A.STDATE , A.STTIME, A.ENDDATE , A.ENDTIME"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar)
            PARA1.Value = work.WF_T7SEL_CAMPCODE.Text
            PARA2.Value = work.WF_T7SEL_HORG.Text
            PARA3.Value = iYmdTo
            PARA4.Value = iYmdFrom
            PARA5.Value = Date.Now
            PARA6.Value = CS0050SESSION.APSV_ID
            PARA7.Value = iSTAFFCODE

            SQLcmd.CommandTimeout = 300
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            oTbl.Load(SQLdr)

            Dim WW_T5tbl As DataTable = oTbl.Clone
            For i As Integer = 0 To oTbl.Rows.Count - 1
                T0005row = WW_T5tbl.NewRow
                T0005row.ItemArray = oTbl.Rows(i).ItemArray
                T0005row("SELECT") = "1"

                If IsDate(T0005row("YMD")) Then
                    T0005row("YMD") = CDate(T0005row("YMD")).ToString("yyyy/MM/dd")
                Else
                    T0005row("YMD") = DBNull.Value
                End If
                If IsDate(T0005row("STDATE")) Then
                    T0005row("STDATE") = CDate(T0005row("STDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("STDATE") = DBNull.Value
                End If
                If IsDate(T0005row("STTIME")) Then
                    T0005row("STTIME") = CDate(T0005row("STTIME")).ToString("HH:mm")
                Else
                    T0005row("STTIME") = DBNull.Value
                End If
                If IsDate(T0005row("ENDDATE")) Then
                    T0005row("ENDDATE") = CDate(T0005row("ENDDATE")).ToString("yyyy/MM/dd")
                Else
                    T0005row("ENDDATE") = DBNull.Value
                End If
                If IsDate(T0005row("ENDTIME")) Then
                    T0005row("ENDTIME") = CDate(T0005row("ENDTIME")).ToString("HH:mm")
                Else
                    T0005row("ENDTIME") = DBNull.Value
                End If
                T0005row("SOUDISTANCE") = Int(T0005row("SOUDISTANCE"))

                WW_T5tbl.Rows.Add(T0005row)

            Next

            oTbl = WW_T5tbl.Copy

            SQLdr.Dispose() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            WW_T5tbl.Dispose()
            WW_T5tbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ' *** GridView用（日報）データ取得                                                   
    Private Sub NIPPOget_T7Format(ByVal iKBN As String, ByRef ioT7tbl As DataTable, ByVal iT0005view As DataView)

        Dim WW_CONVERT As String = ""

        For i As Integer = 0 To iT0005view.Count - 1
            Dim T5row As DataRow = iT0005view.Item(i).Row

            If T5row("WORKKBN") = "A1" OrElse T5row("WORKKBN") = "Z1" Then
                Continue For
            End If

            Dim T0007row As DataRow = ioT7tbl.NewRow

            T0007row("LINECNT") = "0"
            T0007row("OPERATION") = ""
            T0007row("TIMSTP") = "0"
            T0007row("SELECT") = "1"
            T0007row("HIDDEN") = "1"
            T0007row("EXTRACTCNT") = ""
            If iKBN = "OLD" Then
                T0007row("STATUS") = ""
            Else
                T0007row("STATUS") = "日報取込"
            End If
            T0007row("CAMPCODE") = T5row("CAMPCODE")
            T0007row("CAMPNAMES") = T5row("CAMPNAMES")
            T0007row("TAISHOYM") = work.WF_T7SEL_TAISHOYM.Text
            T0007row("STAFFCODE") = T5row("STAFFCODE")
            T0007row("STAFFNAMES") = T5row("STAFFNAMES")
            T0007row("WORKDATE") = T5row("YMD")
            T0007row("WORKINGWEEK") = T5row("WORKINGWEEK")
            T0007row("WORKINGWEEKNAMES") = T5row("WORKINGWEEKNAMES")
            T0007row("HDKBN") = "D"
            T0007row("RECODEKBN") = "0"
            T0007row("RECODEKBNNAMES") = ""
            T0007row("SEQ") = T5row("SEQ")
            T0007row("ENTRYDATE") = "              "
            T0007row("NIPPOLINKCODE") = T5row("UPDYMD")
            T0007row("MORG") = T5row("MORG")
            T0007row("MORGNAMES") = T5row("MORGNAMES")
            T0007row("HORG") = T5row("HORG")
            T0007row("HORGNAMES") = T5row("HORGNAMES")
            T0007row("SORG") = T5row("SORG")
            T0007row("SORGNAMES") = T5row("SORGNAMES")
            T0007row("STAFFKBN") = T5row("STAFFKBN")
            T0007row("STAFFKBNNAMES") = T5row("STAFFKBNNAMES")
            T0007row("HOLIDAYKBN") = T5row("HOLIDAYKBN")
            T0007row("HOLIDAYKBNNAMES") = T5row("HOLIDAYKBNNAMES")
            T0007row("PAYKBN") = ""
            T0007row("PAYKBNNAMES") = ""
            T0007row("SHUKCHOKKBN") = ""
            T0007row("SHUKCHOKKBNNAMES") = ""
            T0007row("WORKKBN") = T5row("WORKKBN")
            T0007row("WORKKBNNAMES") = T5row("WORKKBNNAMES")
            T0007row("STDATE") = T5row("STDATE")
            T0007row("STTIME") = T5row("STTIME")
            T0007row("ENDDATE") = T5row("ENDDATE")
            T0007row("ENDTIME") = T5row("ENDTIME")
            T0007row("WORKTIME") = T0007COM.formatHHMM(T5row("WORKTIME"))
            T0007row("MOVETIME") = T0007COM.formatHHMM(T5row("MOVETIME"))
            T0007row("ACTTIME") = T0007COM.formatHHMM(T5row("ACTTIME"))
            T0007row("BINDSTDATE") = "00:00"
            T0007row("BINDTIME") = "0"
            T0007row("NIPPOBREAKTIME") = "0"
            T0007row("BREAKTIME") = "0"
            T0007row("BREAKTIMECHO") = "0"
            T0007row("BREAKTIMETTL") = "0"
            T0007row("NIGHTTIME") = "0"
            T0007row("NIGHTTIMECHO") = "0"
            T0007row("NIGHTTIMETTL") = "0"
            T0007row("ORVERTIME") = "0"
            T0007row("ORVERTIMECHO") = "0"
            T0007row("ORVERTIMETTL") = "0"
            T0007row("WNIGHTTIME") = "0"
            T0007row("WNIGHTTIMECHO") = "0"
            T0007row("WNIGHTTIMETTL") = "0"
            T0007row("SWORKTIME") = "0"
            T0007row("SWORKTIMECHO") = "0"
            T0007row("SWORKTIMETTL") = "0"
            T0007row("SNIGHTTIME") = "0"
            T0007row("SNIGHTTIMECHO") = "0"
            T0007row("SNIGHTTIMETTL") = "0"
            T0007row("HWORKTIME") = "0"
            T0007row("HWORKTIMECHO") = "0"
            T0007row("HWORKTIMETTL") = "0"
            T0007row("HNIGHTTIME") = "0"
            T0007row("HNIGHTTIMECHO") = "0"
            T0007row("HNIGHTTIMETTL") = "0"
            T0007row("WORKNISSU") = "0"
            T0007row("WORKNISSUCHO") = "0"
            T0007row("WORKNISSUTTL") = "0"
            T0007row("SHOUKETUNISSU") = "0"
            T0007row("SHOUKETUNISSUCHO") = "0"
            T0007row("SHOUKETUNISSUTTL") = "0"
            T0007row("KUMIKETUNISSU") = "0"
            T0007row("KUMIKETUNISSUCHO") = "0"
            T0007row("KUMIKETUNISSUTTL") = "0"
            T0007row("ETCKETUNISSU") = "0"
            T0007row("ETCKETUNISSUCHO") = "0"
            T0007row("ETCKETUNISSUTTL") = "0"
            T0007row("NENKYUNISSU") = "0"
            T0007row("NENKYUNISSUCHO") = "0"
            T0007row("NENKYUNISSUTTL") = "0"
            T0007row("TOKUKYUNISSU") = "0"
            T0007row("TOKUKYUNISSUCHO") = "0"
            T0007row("TOKUKYUNISSUTTL") = "0"
            T0007row("CHIKOKSOTAINISSU") = "0"
            T0007row("CHIKOKSOTAINISSUCHO") = "0"
            T0007row("CHIKOKSOTAINISSUTTL") = "0"
            T0007row("STOCKNISSU") = "0"
            T0007row("STOCKNISSUCHO") = "0"
            T0007row("STOCKNISSUTTL") = "0"
            T0007row("KYOTEIWEEKNISSU") = "0"
            T0007row("KYOTEIWEEKNISSUCHO") = "0"
            T0007row("KYOTEIWEEKNISSUTTL") = "0"
            T0007row("WEEKNISSU") = "0"
            T0007row("WEEKNISSUCHO") = "0"
            T0007row("WEEKNISSUTTL") = "0"
            T0007row("DAIKYUNISSU") = "0"
            T0007row("DAIKYUNISSUCHO") = "0"
            T0007row("DAIKYUNISSUTTL") = "0"
            T0007row("NENSHINISSU") = "0"
            T0007row("NENSHINISSUCHO") = "0"
            T0007row("NENSHINISSUTTL") = "0"
            T0007row("SHUKCHOKNNISSU") = "0"
            T0007row("SHUKCHOKNNISSUCHO") = "0"
            T0007row("SHUKCHOKNNISSUTTL") = "0"
            T0007row("SHUKCHOKNISSU") = "0"
            T0007row("SHUKCHOKNISSUCHO") = "0"
            T0007row("SHUKCHOKNISSUTTL") = "0"

            T0007row("SHUKCHOKNHLDNISSU") = "0"
            T0007row("SHUKCHOKNHLDNISSUCHO") = "0"
            T0007row("SHUKCHOKNHLDNISSUTTL") = "0"
            T0007row("SHUKCHOKHLDNISSU") = "0"
            T0007row("SHUKCHOKHLDNISSUCHO") = "0"
            T0007row("SHUKCHOKHLDNISSUTTL") = "0"

            T0007row("TOKSAAKAISU") = "0"
            T0007row("TOKSAAKAISUCHO") = "0"
            T0007row("TOKSAAKAISUTTL") = "0"
            T0007row("TOKSABKAISU") = "0"
            T0007row("TOKSABKAISUCHO") = "0"
            T0007row("TOKSABKAISUTTL") = "0"
            T0007row("TOKSACKAISU") = "0"
            T0007row("TOKSACKAISUCHO") = "0"
            T0007row("TOKSACKAISUTTL") = "0"
            T0007row("TENKOKAISU") = "0"
            T0007row("TENKOKAISUCHO") = "0"
            T0007row("TENKOKAISUTTL") = "0"
            T0007row("HOANTIME") = "0"
            T0007row("HOANTIMECHO") = "0"
            T0007row("HOANTIMETTL") = "0"
            T0007row("KOATUTIME") = "0"
            T0007row("KOATUTIMECHO") = "0"
            T0007row("KOATUTIMETTL") = "0"
            T0007row("TOKUSA1TIME") = "0"
            T0007row("TOKUSA1TIMECHO") = "0"
            T0007row("TOKUSA1TIMETTL") = "0"
            T0007row("HAYADETIME") = "0"
            T0007row("HAYADETIMECHO") = "0"
            T0007row("HAYADETIMETTL") = "0"
            T0007row("PONPNISSU") = "0"
            T0007row("PONPNISSUCHO") = "0"
            T0007row("PONPNISSUTTL") = "0"
            T0007row("BULKNISSU") = "0"
            T0007row("BULKNISSUCHO") = "0"
            T0007row("BULKNISSUTTL") = "0"
            T0007row("TRAILERNISSU") = "0"
            T0007row("TRAILERNISSUCHO") = "0"
            T0007row("TRAILERNISSUTTL") = "0"
            T0007row("BKINMUKAISU") = "0"
            T0007row("BKINMUKAISUCHO") = "0"
            T0007row("BKINMUKAISUTTL") = "0"
            If T5row("WORKKBN") = "B3" Then
                T0007row("SHARYOKBN") = T5row("SHARYOKBN")
                T0007row("SHARYOKBNNAMES") = T5row("SHARYOKBNNAMES")
                T0007row("OILPAYKBN") = T5row("OILPAYKBN")
                T0007row("OILPAYKBNNAMES") = T5row("OILPAYKBNNAMES")
                If T5row("SUISOKBN") = "1" OrElse T5row("SUISOKBN") = "2" Then
                    '水素の場合、荷卸なし
                    T0007row("UNLOADCNT") = "0"
                    T0007row("UNLOADCNTCHO") = "0"
                    T0007row("UNLOADCNTTTL") = "0"
                Else
                    T0007row("UNLOADCNT") = "1"
                    T0007row("UNLOADCNTCHO") = "0"
                    T0007row("UNLOADCNTTTL") = "1"
                End If
            Else
                T0007row("SHARYOKBN") = ""
                T0007row("SHARYOKBNNAMES") = ""
                T0007row("OILPAYKBN") = ""
                T0007row("OILPAYKBNNAMES") = ""
                T0007row("UNLOADCNT") = "0"
                T0007row("UNLOADCNTCHO") = "0"
                T0007row("UNLOADCNTTTL") = "0"
            End If
            T0007row("SHARYOKBN2") = T5row("SHARYOKBN")
            T0007row("SHARYOKBNNAMES2") = T5row("SHARYOKBNNAMES")
            T0007row("OILPAYKBN2") = T5row("OILPAYKBN")
            T0007row("OILPAYKBNNAMES2") = T5row("OILPAYKBNNAMES")
            If T5row("L1KAISO") = "回送" AndAlso T5row("SUISOKBN") <> "1" Then
                T0007row("HAIDISTANCE") = "0"
                T0007row("HAIDISTANCECHO") = "0"
                T0007row("HAIDISTANCETTL") = "0"
                T0007row("KAIDISTANCE") = Int(T5row("SOUDISTANCE"))
                T0007row("KAIDISTANCECHO") = "0"
                T0007row("KAIDISTANCETTL") = Int(T5row("SOUDISTANCE"))
            Else
                T0007row("HAIDISTANCE") = Int(T5row("SOUDISTANCE"))
                T0007row("HAIDISTANCECHO") = "0"
                T0007row("HAIDISTANCETTL") = Int(T5row("SOUDISTANCE"))
                T0007row("KAIDISTANCE") = "0"
                T0007row("KAIDISTANCECHO") = "0"
                T0007row("KAIDISTANCETTL") = "0"
            End If
            T0007row("DELFLG") = "0"

            T0007row("DATAKBN") = "N"
            T0007row("SHIPORG") = T5row("SHIPORG")
            T0007row("SHIPORGNAMES") = T5row("SHIPORGNAMES")
            T0007row("NIPPONO") = T5row("NIPPONO")
            T0007row("GSHABAN") = T5row("GSHABAN")
            T0007row("RUIDISTANCE") = T5row("RUIDISTANCE")
            T0007row("JIDISTANCE") = T5row("JIDISTANCE")
            T0007row("KUDISTANCE") = T5row("KUDISTANCE")
            T0007row("L1KAISO") = T5row("L1KAISO")
            T0007row("LATITUDE") = T5row("LATITUDE")
            T0007row("LONGITUDE") = T5row("LONGITUDE")

            'ポイント取得
            T0007row("MODELDISTANCE") = 0
            T0007row("MODELDISTANCECHO") = 0
            T0007row("MODELDISTANCETTL") = 0
            T0007row("wHaisoGroup") = T5row("wHaisoGroup")

            T0007row("TRIPNO") = T5row("TRIPNO")

            If T0007row("ORGSEQ").ToString = "" Then
                T0007row("ORGSEQ") = 0
            End If

            ioT7tbl.Rows.Add(T0007row)
        Next

    End Sub

    ' ***  Excel日別・月別判定                                 
    Protected Sub Excelhantei(ByVal IMAP As String, ByRef ORTN As String)

        ORTN = ""       '月合計判定
        Dim wMonthSW As String = ""     '月判定
        Dim wDaySW As String = ""       '月合計判定

        '〇日別明細or月合計要求判定　…　Excel定義に月合計項目が有効ならば、月合計判定("月合計")
        CS0021PROFXLS.CAMPCODE = work.WF_T7SEL_CAMPCODE.Text
        CS0021PROFXLS.PROFID = Master.PROF_REPORT
        CS0021PROFXLS.MAPID = Master.MAPID
        CS0021PROFXLS.REPORTID = IMAP
        CS0021PROFXLS.CS0021PROFXLS()
        If Not isNormal(CS0021PROFXLS.ERR) Then
            Master.output(CS0021PROFXLS.ERR, C_MESSAGE_TYPE.ERR, "CS0021PROFXLS")
            Exit Sub
        End If

        For i As Integer = 0 To CS0021PROFXLS.FIELD.Count - 1
            If CS0021PROFXLS.EFFECT(i) = "Y" AndAlso CS0021PROFXLS.POSIX(i) > 0 AndAlso CS0021PROFXLS.POSIY(i) > 0 Then
                If (CS0021PROFXLS.FIELD(i) = "TOKUSA1TIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HOANTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "KOATUTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKSAAKAISUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKSABKAISUTTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "TOKSACKAISUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "ORVERTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "WNIGHTTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HWORKTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HNIGHTTIMETTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "SWORKTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "SNIGHTTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "NIGHTTIMETTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "WORKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "NENKYUNISSUTTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "KYOTEIWEEKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "SHOUKETUNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKUKYUNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "WEEKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "KUMIKETUNISSUTTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "CHIKOKSOTAINISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "DAIKYUNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "ETCKETUNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "STOCKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "NENSHINISSUTTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "SHUKCHOKNNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "SHUKCHOKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "PONPNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "BULKNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TRAILERNISSUTTL") OrElse
                   (CS0021PROFXLS.FIELD(i) = "BKINMUKAISUTTL") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0101") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0102") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0103") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0104") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0105") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0106") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0107") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0108") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0109") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0110") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0201") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0202") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0203") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0204") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0205") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0206") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0207") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0208") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0209") OrElse
                   (CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0210") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0101") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0102") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0103") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0104") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0105") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0106") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0107") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0108") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0109") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0110") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0201") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0202") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0203") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0204") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0205") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0206") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0207") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0208") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0209") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0210") Then
                    wMonthSW = "月合計"
                End If

                If (CS0021PROFXLS.FIELD(i) = "WORKDATE") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HOLIDAYKBN") OrElse
                   (CS0021PROFXLS.FIELD(i) = "PAYKBN") OrElse
                   (CS0021PROFXLS.FIELD(i) = "SHUKCHOKKBN") OrElse
                   (CS0021PROFXLS.FIELD(i) = "STDATE") OrElse
                   (CS0021PROFXLS.FIELD(i) = "STTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "ENDDATE") OrElse
                   (CS0021PROFXLS.FIELD(i) = "ENDTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "BINDTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "BINDSTDATE") OrElse
                   (CS0021PROFXLS.FIELD(i) = "BREAKTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKUSA1TIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "HOANTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "KOATUTIME") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKSAAKAISU") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKSABKAISU") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TOKSACKAISU") OrElse
                   (CS0021PROFXLS.FIELD(i) = "TENKOKAISU") Then
                    wDaySW = "日別"
                End If

            End If

        Next

        If wMonthSW <> "" AndAlso wDaySW <> "" Then
            ORTN = "ERR"           'Excel定義で月合計項目と日別項目が同時に有効となっている  
        Else
            If wMonthSW = "" Then
                ORTN = wDaySW
            Else
                ORTN = wMonthSW
            End If
        End If

    End Sub


    ' ***  Excel⇒T0007INP　作成
    Protected Sub T0007INPmake(ByRef iTBL As DataTable, ByRef iTTLFLG As String)

        Dim WW_DATE As Date
        Dim WW_TEXT As String = ""
        Dim WW_VALUE As String = ""

        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For i As Integer = 0 To iTBL.Columns.Count - 1
            WW_COLUMNS.Add(iTBL.Columns.Item(i).ColumnName.ToString)
        Next

        '○ExcelデータよりT0007INPtbl作成
        For i As Integer = 0 To iTBL.Rows.Count - 1

            'エリア初期化
            T0007INProw = T0007INPtbl.NewRow
            T0007COM.INProw_Init(work.WF_T7SEL_CAMPCODE.Text, T0007INProw)

            '++++  共通  ++++

            T0007INProw("LINECNT") = 0
            T0007INProw("OPERATION") = "更新"
            T0007INProw("TIMSTP") = "0"
            T0007INProw("SELECT") = 1
            T0007INProw("HIDDEN") = 0

            T0007INProw("EXTRACTCNT") = "0"

            T0007INProw("HDKBN") = "H"
            If iTTLFLG = "月合計" Then
                T0007INProw("RECODEKBN") = "2"
            Else
                T0007INProw("RECODEKBN") = "0"
            End If
            T0007INProw("DATAKBN") = "K"

            ' *** 会社コード(CAMPCODE)
            If WW_COLUMNS.IndexOf("CAMPCODE") < 0 Then
                T0007INProw("CAMPCODE") = work.WF_T7SEL_CAMPCODE.Text
            Else
                T0007INProw("CAMPCODE") = iTBL.Rows(i)("CAMPCODE")
            End If

            ' *** 配属部署(HORG)
            If WW_COLUMNS.IndexOf("HORG") < 0 Then
                T0007INProw("HORG") = ""
            Else
                T0007INProw("HORG") = iTBL.Rows(i)("HORG")
            End If

            ' *** 管理部署(MORG)
            If WW_COLUMNS.IndexOf("MORG") < 0 Then
                T0007INProw("MORG") = ""
            Else
                T0007INProw("MORG") = iTBL.Rows(i)("MORG")
            End If

            ' *** ﾚｺｰﾄﾞ区分(RECODEKBN)
            If WW_COLUMNS.IndexOf("RECODEKBN") < 0 Then
                T0007INProw("RECODEKBN") = "0"
            Else
                T0007INProw("RECODEKBN") = iTBL.Rows(i)("RECODEKBN")
            End If

            ' *** 従業員コード(STAFFCODE)
            If WW_COLUMNS.IndexOf("STAFFCODE") < 0 Then
                T0007INProw("STAFFCODE") = ""
            Else
                T0007INProw("STAFFCODE") = iTBL.Rows(i)("STAFFCODE")
            End If

            ' *** 社員区分(STAFFKBN)
            If WW_COLUMNS.IndexOf("STAFFKBN") < 0 Then
                T0007INProw("STAFFKBN") = ""
            Else
                T0007INProw("STAFFKBN") = iTBL.Rows(i)("STAFFKBN")
            End If

            '++++  日別  ++++
            ' *** 勤務年月日(WORKDATE)
            If WW_COLUMNS.IndexOf("WORKDATE") < 0 Then
                T0007INProw("WORKDATE") = ""
            Else
                If IsDate(iTBL.Rows(i)("WORKDATE")) Then
                    WW_DATE = iTBL.Rows(i)("WORKDATE")
                    T0007INProw("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0007INProw("WORKDATE") = ""
                End If
            End If

            ' *** 曜日(WORKINGWEEK)
            If WW_COLUMNS.IndexOf("WORKINGWEEK") < 0 Then
                T0007INProw("WORKINGWEEK") = ""
            Else
                T0007INProw("WORKINGWEEK") = iTBL.Rows(i)("WORKINGWEEK")
            End If

            ' *** 休日区分(HOLIDAYKBN)
            If WW_COLUMNS.IndexOf("HOLIDAYKBN") < 0 Then
                T0007INProw("HOLIDAYKBN") = ""
            Else
                T0007INProw("HOLIDAYKBN") = ""
            End If

            ' *** 勤怠区分(PAYKBN)
            If WW_COLUMNS.IndexOf("PAYKBN") < 0 Then
                T0007INProw("PAYKBN") = ""
            Else
                T0007INProw("PAYKBN") = iTBL.Rows(i)("PAYKBN")
            End If

            ' *** 宿日直区分(SHUKCHOKKBN)
            If WW_COLUMNS.IndexOf("SHUKCHOKKBN") < 0 Then
                T0007INProw("SHUKCHOKKBN") = ""
            Else
                T0007INProw("SHUKCHOKKBN") = iTBL.Rows(i)("SHUKCHOKKBN")
            End If

            ' *** 出社日(STDATE)
            If WW_COLUMNS.IndexOf("STDATE") < 0 Then
                T0007INProw("STDATE") = ""
            Else
                If IsDate(iTBL.Rows(i)("STDATE")) Then
                    WW_DATE = iTBL.Rows(i)("STDATE")
                    T0007INProw("STDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0007INProw("STDATE") = ""
                End If
            End If

            ' *** 出社時刻(STTIME)
            If WW_COLUMNS.IndexOf("STTIME") < 0 Then
                T0007INProw("STTIME") = ""
            Else
                If IsDate(iTBL.Rows(i)("STTIME")) Then
                    WW_DATE = iTBL.Rows(i)("STTIME")
                    T0007INProw("STTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0007INProw("STTIME") = ""
                End If
            End If

            ' *** 拘束開始時刻(BINDSTDATE)
            If WW_COLUMNS.IndexOf("BINDSTDATE") < 0 Then
                T0007INProw("BINDSTDATE") = ""
            Else
                T0007INProw("BINDSTDATE") = iTBL.Rows(i)("BINDSTDATE")
            End If

            ' *** 稼働時間(ACTTIME)
            If WW_COLUMNS.IndexOf("ACTTIME") < 0 Then
                T0007INProw("ACTTIME") = ""
            Else
                T0007INProw("ACTTIME") = iTBL.Rows(i)("ACTTIME")
            End If

            ' *** 所定拘束(BINDTIME)
            If WW_COLUMNS.IndexOf("BINDTIME") < 0 Then
                T0007INProw("BINDTIME") = ""
            Else
                T0007INProw("BINDTIME") = iTBL.Rows(i)("BINDTIME")
            End If

            ' *** 退社日(ENDDATE)
            If WW_COLUMNS.IndexOf("ENDDATE") < 0 Then
                T0007INProw("ENDDATE") = ""
            Else
                If IsDate(iTBL.Rows(i)("ENDDATE")) Then
                    WW_DATE = iTBL.Rows(i)("ENDDATE")
                    T0007INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Else
                    T0007INProw("ENDDATE") = ""
                End If
            End If

            ' *** 退社時刻(ENDTIME)
            If WW_COLUMNS.IndexOf("ENDTIME") < 0 Then
                T0007INProw("ENDTIME") = ""
            Else
                If IsDate(iTBL.Rows(i)("ENDTIME")) Then
                    WW_DATE = iTBL.Rows(i)("ENDTIME")
                    T0007INProw("ENDTIME") = WW_DATE.ToString("HH:mm")
                Else
                    T0007INProw("ENDTIME") = ""
                End If
            End If

            ' *** 休憩時間(BREAKTIME)
            If WW_COLUMNS.IndexOf("BREAKTIME") < 0 Then
                T0007INProw("BREAKTIME") = ""
            Else
                T0007INProw("BREAKTIME") = iTBL.Rows(i)("BREAKTIME")
            End If

            ' *** 特作(TOKUSA1TIME)
            If WW_COLUMNS.IndexOf("TOKUSA1TIME") < 0 Then
                T0007INProw("TOKUSA1TIME") = ""
            Else
                T0007INProw("TOKUSA1TIME") = iTBL.Rows(i)("TOKUSA1TIME")
            End If

            ' *** 洗浄回数(SENJYOCNT)
            If WW_COLUMNS.IndexOf("SENJYOCNT") < 0 Then
                T0007INProw("SENJYOCNT") = ""
            Else
                T0007INProw("SENJYOCNT") = iTBL.Rows(i)("SENJYOCNT")
            End If

            ' *** 危険品100(UNLOADADDCNT1)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT1") < 0 Then
                T0007INProw("UNLOADADDCNT1") = ""
            Else
                T0007INProw("UNLOADADDCNT1") = iTBL.Rows(i)("UNLOADADDCNT1")
            End If

            ' *** 危険品200(UNLOADADDCNT2)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT2") < 0 Then
                T0007INProw("UNLOADADDCNT2") = ""
            Else
                T0007INProw("UNLOADADDCNT2") = iTBL.Rows(i)("UNLOADADDCNT2")
            End If

            ' *** 危険品800(UNLOADADDCNT3)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT3") < 0 Then
                T0007INProw("UNLOADADDCNT3") = ""
            Else
                T0007INProw("UNLOADADDCNT3") = iTBL.Rows(i)("UNLOADADDCNT3")
            End If

            '' *** 危険品1000(UNLOADADDCNT4)
            'If WW_COLUMNS.IndexOf("UNLOADADDCNT4") < 0 Then
            '    T0007INProw("UNLOADADDCNT4") = ""
            'Else
            '    T0007INProw("UNLOADADDCNT4") = iTBL.Rows(i)("UNLOADADDCNT4")
            'End If

            ' *** 危険品1000(LOADINGCNT1)
            If WW_COLUMNS.IndexOf("LOADINGCNT1") < 0 Then
                T0007INProw("LOADINGCNT1") = ""
            Else
                T0007INProw("LOADINGCNT1") = iTBL.Rows(i)("LOADINGCNT1")
            End If

            ' *** 特殊作業(SHORTDISTANCE1)
            If WW_COLUMNS.IndexOf("SHORTDISTANCE1") < 0 Then
                T0007INProw("SHORTDISTANCE1") = ""
            Else
                T0007INProw("SHORTDISTANCE1") = iTBL.Rows(i)("SHORTDISTANCE1")
            End If

            ' *** 特殊作業(SHORTDISTANCE2)
            If WW_COLUMNS.IndexOf("SHORTDISTANCE2") < 0 Then
                T0007INProw("SHORTDISTANCE2") = ""
            Else
                T0007INProw("SHORTDISTANCE2") = iTBL.Rows(i)("SHORTDISTANCE2")
            End If

            ' *** 荷卸回数(UNLOADCNT)
            If WW_COLUMNS.IndexOf("UNLOADCNT") < 0 Then
                T0007INProw("UNLOADCNT") = ""
            Else
                T0007INProw("UNLOADCNT") = iTBL.Rows(i)("UNLOADCNT")
            End If

            ' *** 平日残業時間(ORVERTIME)
            If WW_COLUMNS.IndexOf("ORVERTIME") < 0 Then
                T0007INProw("ORVERTIME") = ""
            Else
                T0007INProw("ORVERTIME") = iTBL.Rows(i)("ORVERTIME")
            End If

            ' *** 平日深夜時間(WNIGHTTIME)
            If WW_COLUMNS.IndexOf("WNIGHTTIME") < 0 Then
                T0007INProw("WNIGHTTIME") = ""
            Else
                T0007INProw("WNIGHTTIME") = iTBL.Rows(i)("WNIGHTTIME")
            End If

            ' *** 休日出勤時間(HWORKTIME)
            If WW_COLUMNS.IndexOf("WNIGHTTIME") < 0 Then
                T0007INProw("HWORKTIME") = ""
            Else
                T0007INProw("HWORKTIME") = iTBL.Rows(i)("HWORKTIME")
            End If

            ' *** 休日深夜時間(HNIGHTTIME)
            If WW_COLUMNS.IndexOf("HNIGHTTIME") < 0 Then
                T0007INProw("HNIGHTTIME") = ""
            Else
                T0007INProw("HNIGHTTIME") = iTBL.Rows(i)("HNIGHTTIME")
            End If

            ' *** 日曜出勤時間(SWORKTIME)
            If WW_COLUMNS.IndexOf("SWORKTIME") < 0 Then
                T0007INProw("SWORKTIME") = ""
            Else
                T0007INProw("SWORKTIME") = iTBL.Rows(i)("SWORKTIME")
            End If

            ' *** 日曜深夜時間(SNIGHTTIME)
            If WW_COLUMNS.IndexOf("SNIGHTTIME") < 0 Then
                T0007INProw("SNIGHTTIME") = ""
            Else
                T0007INProw("SNIGHTTIME") = iTBL.Rows(i)("SNIGHTTIME")
            End If

            ' *** 所定深夜時間(NIGHTTIME)
            If WW_COLUMNS.IndexOf("NIGHTTIME") < 0 Then
                T0007INProw("NIGHTTIME") = ""
            Else
                T0007INProw("NIGHTTIME") = iTBL.Rows(i)("NIGHTTIME")
            End If

            ' *** 時給者所定時間(JIKYUSHATIME)
            If WW_COLUMNS.IndexOf("JIKYUSHATIME") < 0 Then
                T0007INProw("JIKYUSHATIME") = ""
            Else
                T0007INProw("JIKYUSHATIME") = iTBL.Rows(i)("JIKYUSHATIME")
            End If

            ' *** 配送距離(HAIDISTANCE)
            If WW_COLUMNS.IndexOf("HAIDISTANCE") < 0 Then
                T0007INProw("HAIDISTANCE") = ""
            Else
                T0007INProw("HAIDISTANCE") = iTBL.Rows(i)("HAIDISTANCE")
            End If

            ' *** 回送距離(KAIDISTANCE)
            If WW_COLUMNS.IndexOf("KAIDISTANCE") < 0 Then
                T0007INProw("KAIDISTANCE") = ""
            Else
                T0007INProw("KAIDISTANCE") = iTBL.Rows(i)("KAIDISTANCE")
            End If

            '++++  月合計  ++++
            ' *** 対象年月(TAISHOYM)
            If WW_COLUMNS.IndexOf("TAISHOYM") < 0 Then
                T0007INProw("TAISHOYM") = ""
            Else
                T0007INProw("TAISHOYM") = iTBL.Rows(i)("TAISHOYM")
            End If

            ' *** 所労合計(WORKNISSUTTL)
            If WW_COLUMNS.IndexOf("WORKNISSUTTL") < 0 Then
                T0007INProw("WORKNISSUTTL") = ""
            Else
                T0007INProw("WORKNISSUTTL") = iTBL.Rows(i)("WORKNISSUTTL")
            End If

            ' *** 年休合計(NENKYUNISSUTTL)
            If WW_COLUMNS.IndexOf("NENKYUNISSUTTL") < 0 Then
                T0007INProw("NENKYUNISSUTTL") = ""
            Else
                T0007INProw("NENKYUNISSUTTL") = iTBL.Rows(i)("NENKYUNISSUTTL")
            End If

            ' *** 協約週休合計(KYOTEIWEEKNISSUTTL)
            If WW_COLUMNS.IndexOf("KYOTEIWEEKNISSUTTL") < 0 Then
                T0007INProw("KYOTEIWEEKNISSUTTL") = ""
            Else
                T0007INProw("KYOTEIWEEKNISSUTTL") = iTBL.Rows(i)("KYOTEIWEEKNISSUTTL")
            End If

            ' *** 傷欠合計(SHOUKETUNISSUTTL)
            If WW_COLUMNS.IndexOf("SHOUKETUNISSUTTL") < 0 Then
                T0007INProw("SHOUKETUNISSUTTL") = ""
            Else
                T0007INProw("SHOUKETUNISSUTTL") = iTBL.Rows(i)("SHOUKETUNISSUTTL")
            End If

            ' *** 特休合計(TOKUKYUNISSUTTL)
            If WW_COLUMNS.IndexOf("TOKUKYUNISSUTTL") < 0 Then
                T0007INProw("TOKUKYUNISSUTTL") = ""
            Else
                T0007INProw("TOKUKYUNISSUTTL") = iTBL.Rows(i)("TOKUKYUNISSUTTL")
            End If

            ' *** 週休合計(WEEKNISSUTTL)
            If WW_COLUMNS.IndexOf("WEEKNISSUTTL") < 0 Then
                T0007INProw("WEEKNISSUTTL") = ""
            Else
                T0007INProw("WEEKNISSUTTL") = iTBL.Rows(i)("WEEKNISSUTTL")
            End If

            ' *** 組欠合計(KUMIKETUNISSUTTL)
            If WW_COLUMNS.IndexOf("KUMIKETUNISSUTTL") < 0 Then
                T0007INProw("KUMIKETUNISSUTTL") = ""
            Else
                T0007INProw("KUMIKETUNISSUTTL") = iTBL.Rows(i)("KUMIKETUNISSUTTL")
            End If

            ' *** 遅早合計(CHIKOKSOTAINISSUTTL)
            If WW_COLUMNS.IndexOf("CHIKOKSOTAINISSUTTL") < 0 Then
                T0007INProw("CHIKOKSOTAINISSUTTL") = ""
            Else
                T0007INProw("CHIKOKSOTAINISSUTTL") = iTBL.Rows(i)("CHIKOKSOTAINISSUTTL")
            End If

            ' *** 代休合計(DAIKYUNISSUTTL)
            If WW_COLUMNS.IndexOf("DAIKYUNISSUTTL") < 0 Then
                T0007INProw("DAIKYUNISSUTTL") = ""
            Else
                T0007INProw("DAIKYUNISSUTTL") = iTBL.Rows(i)("DAIKYUNISSUTTL")
            End If

            ' *** 他欠合計(ETCKETUNISSUTTL)
            If WW_COLUMNS.IndexOf("ETCKETUNISSUTTL") < 0 Then
                T0007INProw("ETCKETUNISSUTTL") = ""
            Else
                T0007INProw("ETCKETUNISSUTTL") = iTBL.Rows(i)("ETCKETUNISSUTTL")
            End If

            ' *** ストック休暇合計(STOCKNISSUTTL)
            If WW_COLUMNS.IndexOf("STOCKNISSUTTL") < 0 Then
                T0007INProw("STOCKNISSUTTL") = ""
            Else
                T0007INProw("STOCKNISSUTTL") = iTBL.Rows(i)("STOCKNISSUTTL")
            End If

            ' *** 年始出勤合計(NENSHINISSUTTL)
            If WW_COLUMNS.IndexOf("NENSHINISSUTTL") < 0 Then
                T0007INProw("NENSHINISSUTTL") = ""
            Else
                T0007INProw("NENSHINISSUTTL") = iTBL.Rows(i)("NENSHINISSUTTL")
            End If

            ' *** 宿日直通常合計(SHUKCHOKNISSUTTL)
            If WW_COLUMNS.IndexOf("SHUKCHOKNISSUTTL") < 0 Then
                T0007INProw("SHUKCHOKNISSUTTL") = ""
            Else
                T0007INProw("SHUKCHOKNISSUTTL") = iTBL.Rows(i)("SHUKCHOKNISSUTTL")
            End If

            ' *** 宿日直年始合計(SHUKCHOKNNISSUTTL)
            If WW_COLUMNS.IndexOf("SHUKCHOKNNISSUTTL") < 0 Then
                T0007INProw("SHUKCHOKNNISSUTTL") = ""
            Else
                T0007INProw("SHUKCHOKNNISSUTTL") = iTBL.Rows(i)("SHUKCHOKNNISSUTTL")
            End If

            ' *** 宿日直通常合計(SHUKCHOKHLDNISSUTTL)
            If WW_COLUMNS.IndexOf("SHUKCHOKHLDNISSUTTL") < 0 Then
                T0007INProw("SHUKCHOKHLDNISSUTTL") = ""
            Else
                T0007INProw("SHUKCHOKHLDNISSUTTL") = iTBL.Rows(i)("SHUKCHOKHLDNISSUTTL")
            End If

            ' *** 宿日直年始合計(SHUKCHOKNHLDNISSUTTL)
            If WW_COLUMNS.IndexOf("SHUKCHOKHLDNISSUTTL") < 0 Then
                T0007INProw("SHUKCHOKNHLDNISSUTTL") = ""
            Else
                T0007INProw("SHUKCHOKNHLDNISSUTTL") = iTBL.Rows(i)("SHUKCHOKNHLDNISSUTTL")
            End If

            ' *** 車中泊回数合計(SHACHUHAKNISSUTTL)
            If WW_COLUMNS.IndexOf("SHACHUHAKNISSUTTL") < 0 Then
                T0007INProw("SHACHUHAKNISSUTTL") = ""
            Else
                T0007INProw("SHACHUHAKNISSUTTL") = iTBL.Rows(i)("SHACHUHAKNISSUTTL")
            End If

            ' *** 洗浄回数合計(SENJYOCNTTTL)
            If WW_COLUMNS.IndexOf("SENJYOCNTTTL") < 0 Then
                T0007INProw("SENJYOCNTTTL") = ""
            Else
                T0007INProw("SENJYOCNTTTL") = iTBL.Rows(i)("SENJYOCNTTTL")
            End If

            ' *** 平日残業時間合計(ORVERTIMETTL)
            If WW_COLUMNS.IndexOf("ORVERTIMETTL") < 0 Then
                T0007INProw("ORVERTIMETTL") = ""
            Else
                T0007INProw("ORVERTIMETTL") = iTBL.Rows(i)("ORVERTIMETTL")
            End If

            ' *** 平日深夜時間合計(WNIGHTTIMETTL)
            If WW_COLUMNS.IndexOf("WNIGHTTIMETTL") < 0 Then
                T0007INProw("WNIGHTTIMETTL") = ""
            Else
                T0007INProw("WNIGHTTIMETTL") = iTBL.Rows(i)("WNIGHTTIMETTL")
            End If

            ' *** 所定深夜時間合計(NIGHTTIMETTL)
            If WW_COLUMNS.IndexOf("NIGHTTIMETTL") < 0 Then
                T0007INProw("NIGHTTIMETTL") = ""
            Else
                T0007INProw("NIGHTTIMETTL") = iTBL.Rows(i)("NIGHTTIMETTL")
            End If

            ' *** 休日出勤時間合計(HWORKTIMETTL)
            If WW_COLUMNS.IndexOf("HWORKTIMETTL") < 0 Then
                T0007INProw("HWORKTIMETTL") = ""
            Else
                T0007INProw("HWORKTIMETTL") = iTBL.Rows(i)("HWORKTIMETTL")
            End If

            ' *** 休日深夜時間合計(HNIGHTTIMETTL)
            If WW_COLUMNS.IndexOf("HNIGHTTIMETTL") < 0 Then
                T0007INProw("HNIGHTTIMETTL") = ""
            Else
                T0007INProw("HNIGHTTIMETTL") = iTBL.Rows(i)("HNIGHTTIMETTL")
            End If

            ' *** 日曜出勤時間合計(SWORKTIMETTL)
            If WW_COLUMNS.IndexOf("SWORKTIMETTL") < 0 Then
                T0007INProw("SWORKTIMETTL") = ""
            Else
                T0007INProw("SWORKTIMETTL") = iTBL.Rows(i)("SWORKTIMETTL")
            End If

            ' *** 日曜深夜時間合計(SNIGHTTIMETTL)
            If WW_COLUMNS.IndexOf("SNIGHTTIMETTL") < 0 Then
                T0007INProw("SNIGHTTIMETTL") = ""
            Else
                T0007INProw("SNIGHTTIMETTL") = iTBL.Rows(i)("SNIGHTTIMETTL")
            End If

            ' *** 休憩時間合計(BREAKTIMETTL)
            If WW_COLUMNS.IndexOf("BREAKTIMETTL") < 0 Then
                T0007INProw("BREAKTIMETTL") = ""
            Else
                T0007INProw("BREAKTIMETTL") = iTBL.Rows(i)("BREAKTIMETTL")
            End If

            ' *** 時給者所定内合計(JIKYUSHATIMETTL)
            If WW_COLUMNS.IndexOf("JIKYUSHATIMETTL") < 0 Then
                T0007INProw("JIKYUSHATIMETTL") = ""
            Else
                T0007INProw("JIKYUSHATIMETTL") = iTBL.Rows(i)("JIKYUSHATIMETTL")
            End If

            ' *** 特作Ⅰ合計(TOKUSA1TIMETTL)
            If WW_COLUMNS.IndexOf("TOKUSA1TIMETTL") < 0 Then
                T0007INProw("TOKUSA1TIMETTL") = ""
            Else
                T0007INProw("TOKUSA1TIMETTL") = iTBL.Rows(i)("TOKUSA1TIMETTL")
            End If

            ' *** 危険品100合計(UNLOADADDCNT1TTL)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT1TTL") < 0 Then
                T0007INProw("UNLOADADDCNT1TTL") = ""
            Else
                T0007INProw("UNLOADADDCNT1TTL") = iTBL.Rows(i)("UNLOADADDCNT1TTL")
            End If

            ' *** 危険品200合計(UNLOADADDCNT2TTL)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT2TTL") < 0 Then
                T0007INProw("UNLOADADDCNT2TTL") = ""
            Else
                T0007INProw("UNLOADADDCNT2TTL") = iTBL.Rows(i)("UNLOADADDCNT2TTL")
            End If

            ' *** 危険品800合計(UNLOADADDCNT3TTL)
            If WW_COLUMNS.IndexOf("UNLOADADDCNT3TTL") < 0 Then
                T0007INProw("UNLOADADDCNT3TTL") = ""
            Else
                T0007INProw("UNLOADADDCNT3TTL") = iTBL.Rows(i)("UNLOADADDCNT3TTL")
            End If

            '' *** 危険品1000合計(UNLOADADDCNT4TTL)
            'If WW_COLUMNS.IndexOf("UNLOADADDCNT4TTL") < 0 Then
            '    T0007INProw("UNLOADADDCNT4TTL") = ""
            'Else
            '    T0007INProw("UNLOADADDCNT4TTL") = iTBL.Rows(i)("UNLOADADDCNT4TTL")
            'End If

            ' *** 危険品1000合計(LOADINGCNT1TTL)
            If WW_COLUMNS.IndexOf("LOADINGCNT1TTL") < 0 Then
                T0007INProw("LOADINGCNT1TTL") = ""
            Else
                T0007INProw("LOADINGCNT1TTL") = iTBL.Rows(i)("LOADINGCNT1TTL")
            End If

            ' *** 短距離手当１合計(SHORTDISTANCE1TTL)
            If WW_COLUMNS.IndexOf("SHORTDISTANCE1TTL") < 0 Then
                T0007INProw("SHORTDISTANCE1TTL") = ""
            Else
                T0007INProw("SHORTDISTANCE1TTL") = iTBL.Rows(i)("SHORTDISTANCE1TTL")
            End If

            ' *** 短距離手当２合計(SHORTDISTANCE2TTL)
            If WW_COLUMNS.IndexOf("SHORTDISTANCE2TTL") < 0 Then
                T0007INProw("SHORTDISTANCE2TTL") = ""
            Else
                T0007INProw("SHORTDISTANCE2TTL") = iTBL.Rows(i)("SHORTDISTANCE2TTL")
            End If

            '車両区分（SHARYOKBN 1:単車、2:トレーラ）
            '給与油種区分（OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素
            For WW_SHARYOKBN As Integer = 1 To 2
                For WW_OILPAYKBN As Integer = 1 To 10
                    'UNLOADCNTTTL0101～UNLOADCNTTTL0110変数名を動的に作成
                    'UNLOADCNTTTL0201～UNLOADCNTTTL0210変数名を動的に作成
                    'HAIDISTANCETTL0101～HAIDISTANCETTL0110変数名を動的に作成
                    'HAIDISTANCETTL0201～HAIDISTANCETTL0210変数名を動的に作成
                    Dim WW_UNLOADCNT As String = "UNLOADCNTTTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    '荷卸回数（単車、トレーラ）
                    If WW_COLUMNS.IndexOf(WW_UNLOADCNT) < 0 Then
                        T0007INProw(WW_UNLOADCNT) = ""
                    Else
                        T0007INProw(WW_UNLOADCNT) = iTBL.Rows(i)(WW_UNLOADCNT)
                    End If

                    '配送距離（単車、トレーラ）
                    If WW_COLUMNS.IndexOf(WW_HAIDISTANCETTL) < 0 Then
                        T0007INProw(WW_HAIDISTANCETTL) = ""
                    Else
                        T0007INProw(WW_HAIDISTANCETTL) = iTBL.Rows(i)(WW_HAIDISTANCETTL)
                    End If
                Next
            Next

            T0007INPtbl.Rows.Add(T0007INProw)

        Next

    End Sub

    '' ***  IPアドレスの取得処理
    'Protected Sub GetIpAddr(ByVal I_ORG As String, ByRef O_IPADDR As String, ByRef O_RTN As String)

    '    Try
    '        O_RTN = C_MESSAGE_NO.NORMAL
    '        O_IPADDR = ""

    '        'DataBase接続文字
    '        Dim SQLcon As SqlConnection = CS0050SESSION.getConnection
    '        SQLcon.Open() 'DataBase接続(Open)

    '        Dim SQLStr As String

    '        '検索SQL文
    '        SQLStr =
    '                    " SELECT IPADDR " &
    '                    " FROM S0001_TERM " &
    '                    " WHERE TERMORG      =  '" & I_ORG & "'" &
    '                    " AND   TERMCLASS    = '1' " &
    '                    " AND   STYMD        <= getdate() " &
    '                    " AND   ENDYMD       >= getdate() " &
    '                    " AND   DELFLG       <> '1' "

    '        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
    '        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

    '        While SQLdr.Read
    '            O_IPADDR = SQLdr("IPADDR")
    '        End While

    '        SQLdr.Dispose()
    '        SQLdr = Nothing

    '        SQLcmd.Dispose()
    '        SQLcmd = Nothing

    '        SQLcon.Close()
    '        SQLcon.Dispose()
    '        SQLcon = Nothing

    '    Catch ex As Exception
    '        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0001_TERM SELECT")
    '        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
    '        CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
    '        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
    '        CS0011LOGWRITE.TEXT = ex.ToString()
    '        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
    '        O_RTN = C_MESSAGE_NO.DB_ERROR
    '        Exit Sub
    '    End Try

    'End Sub

    ''　指定されたサーバー（IPアドレス）に対しPINGコマンド実行
    'Private Function CheckforPing(ByVal iServName As String) As Boolean
    '    '接続確認（ping）
    '    Dim p As New System.Net.NetworkInformation.Ping()
    '    Dim reply As System.Net.NetworkInformation.PingReply = p.Send(iServName, 3000)
    '    If reply.Status = System.Net.NetworkInformation.IPStatus.Success Then
    '        Return True
    '    Else
    '        Return False
    '    End If
    'End Function

    ' ***  条件抽出画面情報退避
    Protected Sub MAPrefelence(ByRef O_MSG As String, ByRef O_RTN As String)

        O_MSG = ""
        O_RTN = C_MESSAGE_NO.NORMAL


        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007S Then                                                    '条件画面からの画面遷移

            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()
            work.WF_T7I_XMLsaveF.Text = Master.XMLsaveF

            ''○T0007tbl情報保存先のファイル名
            'work.WF_T7I_XMLsaveF.Text = HttpContext.Current.Session("FILEdir") & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & CS0050SESSION.USERID & "-T00007I-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

            '日報取込範囲設定
            Dim dt As Date = CDate(work.WF_T7SEL_TAISHOYM.Text & "/01")
            work.WF_T7I_Head_NIPPO_FROM.Text = "01"
            work.WF_T7I_Head_NIPPO_TO.Text = dt.AddMonths(1).AddDays(-1).ToString("dd")

            WF_NIPPO_FROM.Text = work.WF_T7I_Head_NIPPO_FROM.Text
            WF_NIPPO_TO.Text = work.WF_T7I_Head_NIPPO_TO.Text
        End If

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007JKT Then

            '勤怠個別画面から遷移した場合
            '保存しておいた、GridViewの表示開始位置、絞込み条件の乗務員、日付を設定し直す
            WF_GridPosition.Text = work.WF_T7I_GridPosition.Text
            WF_STAFFCODE.Text = work.WF_T7I_Head_STAFFCODE.Text
            WF_WORKDATE.Text = work.WF_T7I_Head_WORKDATE.Text
            WF_NIPPO_FROM.Text = work.WF_T7I_Head_NIPPO_FROM.Text
            WF_NIPPO_TO.Text = work.WF_T7I_Head_NIPPO_TO.Text

            '乗務員
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)

        End If

        '■■■ 画面モード（更新・参照）設定  ■■■
        '事務員勤怠登録（条件）画面から遷移した場合
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            If work.WF_T7SEL_LIMITFLG.Text = "0" Then
                '対象月の締前は更新ＯＫ
                WF_MAPpermitcode.Value = "TRUE"

                If work.WF_T7SEL_PERMITCODE.Text = C_PERMISSION.UPDATE Then
                    '更新権限あり
                    WF_MAPpermitcode.Value = "TRUE"
                Else
                    WF_MAPpermitcode.Value = "FALSE"
                    O_RTN = "10050"

                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・選択した配属部署は、更新権限がありません。"
                    O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MES
                End If

                If WF_MAPpermitcode.Value = "TRUE" Then
                    '選択した配属部署とサーバー部署が異なる場合、相手サーバーの状態を確認する
                    If work.WF_T7SEL_SRVSTAT.Text <> "MYSRV" Then

                        If work.WF_T7SEL_SRVSTAT.Text = "NOTHING" Then
                            WF_MAPpermitcode.Value = "FALSE"
                            O_RTN = "10049"

                            Dim WW_ERR_MES As String = ""
                            WW_ERR_MES = "・選択した配属部署の勤怠は更新できません。"
                            O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MES
                        Else

                            'pingでサーバーの生死確認
                            If work.WF_T7SEL_SRVSTAT.Text = "START" Then
                                '該当するサーバーが生きていたら、更新不可とする
                                WF_MAPpermitcode.Value = "FALSE"
                                O_RTN = "10049"

                                Dim WW_ERR_MES As String = ""
                                WW_ERR_MES = "・選択した配属部署の勤怠は更新できません。"
                                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 当該部署サーバーで更新して下さい。 , "
                                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ※重複入力を抑止 , "
                                O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MES
                            Else
                                '該当するサーバーが死んでいたら、更新可とする
                                WF_MAPpermitcode.Value = "TRUE"
                            End If
                        End If

                    End If
                End If

            Else
                '対象月の締後は更新できない
                WF_MAPpermitcode.Value = "FALSE"
                O_RTN = "10051"

                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・勤怠締後は更新できません。"
                O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MES
            End If

        Else
            WF_MAPpermitcode.Value = "FALSE"
            O_RTN = "10050"

            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・営業勤怠登録の更新権限がありません。"
            O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MES
        End If

    End Sub

End Class

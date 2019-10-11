Imports System.Data.SqlClient
Imports System.Drawing
Imports OFFICE.GRIS0005LeftBox

Public Class GRTA0009LMKINTAICREW
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' テーブルデータソー
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                      'テーブルデータソート
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' セッション情報管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報管理
    ''' <summary>
    ''' 固定値リスト取得
    ''' </summary>
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              'Leftボックス用固定値リスト取得
    ''' <summary>
    ''' 勤怠共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通

    '検索結果格納ds
    Private TA0009tbl As DataTable                                  'Grid格納用テーブル
    Private TA0009SUMtbl As DataTable                               'Grid格納用テーブル
    Private TA0009VIEWtbl As DataTable                              'Grid格納用テーブル
    Private MB0005tbl As DataTable                                  'Grid格納用テーブル
    Private SELECTORtbl As DataTable                                'TREE選択作成作業テーブル

    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String                                     '
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String                                      '
    ''' <summary>
    ''' 一覧最大表示件数（一画面）
    ''' </summary>
    Private Const CONST_DSPROWCOUNT As Integer = 40                 '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    ''' <summary>
    ''' 詳細部タブID
    ''' </summary>
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        'カレンダーテーブル取得
        GetCalendar(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        If IsPostBack Then

            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonEMS"                 '■ EMSボタンクリック時処理
                        WF_ButtonEMS_Click()
                    Case "WF_ButtonINQ"                 '■ 照会ボタンクリック時処理
                        WF_ButtonINQ_Click()
                    Case "WF_ButtonXLS"                 '■ ダウンロードボタンクリック時処理
                        WF_ButtonXLS_Click()
                    Case "WF_ButtonFIRST"               '■ 最始行ボタンクリック時処理
                        WF_ButtonFIRST_Click()
                    Case "WF_ButtonLAST"                '■ 最終行ボタンクリック時処理
                        WF_ButtonLAST_Click()
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_ButtonSel"                 '■ 左ボックス選択ボタンクリック時処理
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '■ 左ボックスキャンセルボタンクリック時処理
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"             '■ 入力領域ダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_TextChange"                '■ 入力領域内容変更時時処理
                        WW_LeftBoxReSet()
                    Case "WF_ListboxDBclick"            '■ 左ボックスダブルクリック時処理
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"        '■ 左ボックス選択処理
                        WF_LEFTBOX_SELECT_Click()
                    Case "WF_MEMOChange"                '■ 右ボックスメモ欄変更時処理
                        WF_RIGHTBOX_Change()
                    Case "WF_SELECTOR_CHG"              '■ セレクタ変更ラジオボタンクリック処理
                        WF_Selector_Change_Click()
                    Case "WF_SELECTOR_SW_Click"         '■ セレクタ変更ラジオボタンクリック処理
                        SELECTOR_Click()
                    Case "WF_CHECKBOX_CHG"              '■ チェックボックス変更時処理
                End Select
            End If
            '○ 一覧再表示処理
            DisplayGrid()
        Else
            '〇初期化処理
            Initialize()
        End If

        '○Close
        If Not IsNothing(MB0005tbl) Then
            MB0005tbl.Dispose()
            MB0005tbl = Nothing
        End If
        If Not IsNothing(SELECTORtbl) Then
            SELECTORtbl.Dispose()
            SELECTORtbl = Nothing
        End If
        If Not IsNothing(TA0009SUMtbl) Then
            TA0009SUMtbl.Dispose()
            TA0009SUMtbl = Nothing
        End If
        If Not IsNothing(TA0009VIEWtbl) Then
            TA0009VIEWtbl.Dispose()
            TA0009VIEWtbl = Nothing
        End If
        If Not IsNothing(TA0009tbl) Then
            TA0009tbl.Dispose()
            TA0009tbl = Nothing
        End If
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定

        rightview.resetindex()
        leftview.activeListBox()
        '〇 条件抽出画面情報退避
        MAPrefelence()
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False

        '初期値（区分、稼動MAX、残業MAX取得
        SetInitialValue()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        GetMapData()

        '○画面表示データ保存
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0009tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0009tbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0009tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0009WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If
        '〇カレンダーの色設定
        setCalendarColor(pnlListArea.FindControl("pnlListArea_HR").Controls(0), MB0005tbl)

        '〇セレクタ初期表示処理
        WF_SelectorMView.ActiveViewIndex = 0

        'EMSボタン非表示設定
        If work.WF_SEL_CAMPCODE.Text = GRTA0009WRKINC.C_COMP_ENEX Then
            'ENEX以外は、非表示
            WF_EMS.Value = "TRUE"
        End If

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        If IsNothing(TA0009VIEWtbl) Then
            If Not Master.RecoverTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        End If

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0009VIEWtbl.Rows.Count - 1
            If TA0009VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0009VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Using WW_TBLview As DataView = New DataView(TA0009VIEWtbl)

            'ソート
            WW_TBLview.Sort = "LINECNT"
            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
            '一覧作成

            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0009WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = WW_TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.CS0013ProfView()
            '〇カレンダーの書式設定処理
            SetCalendarColor(pnlListArea.FindControl("pnlListArea_HR").Controls(0), MB0005tbl)

            '○クリア
            If WW_TBLview.Count = 0 Then
                WF_GridPosition.Text = "1"
            Else
                WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
            End If
        End Using
    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(EMS向け出力)ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEMS_Click()

        '20190319 aono開発中

        '○表示データをTA0009tblへ設定
        'Private Sub TA0009_get()を改変

        '■ 表示元データ(条件によるサマリーデータ)取得
        'カラム設定
        AddColumnToTA0009tbl(TA0009tbl)

        Dim TA0009SELtbl As DataTable = TA0009tbl.Clone
        '--------------------------------
        '締め前（L0001_TOKEIより取得
        '--------------------------------
        '①自部署で作業した乗務員の日報、勤怠を自サーバーから取得（応援者を含む）
        GetTA0009Wk(TA0009tbl)

        '②他部署の乗務員の日報、勤怠を本社サーバーから取得（上記で取得した応援者のみ抽出）
        GetTA0009Wk2(TA0009tbl)

        '--------------------------------
        '締め後（L0004_SUMMARYKより取得
        '--------------------------------
        GetTA0009WkSum3(TA0009tbl)

        '①で取得した応援者分を除く（②で取得し直したため）
        CS0026TblSort.TABLE = TA0009tbl
        CS0026TblSort.FILTER = "SUPPORTKBN = '0'"
        CS0026TblSort.SORTING = "NACOFFICESORG, PAYSTAFFCODE,NACSHUKODATE"
        TA0009tbl = CS0026TblSort.sort()

        'DataTableのコピー
        'TA0009tblテーブルから抽出したデータをTA0009tbl4EMSテーブルへCopy
        Dim TA0009tblView = New DataView(TA0009tbl)
        TA0009tblView.RowFilter = "NACHAISTDATE > '1950/01/01' or PAYKBN <> '00'"
        TA0009tblView.Sort = "PAYSTAFFCODE ASC, KEIJOYMD ASC"
        Dim TA0009tbl4EMS As DataTable = TA0009tblView.ToTable
        TA0009tblView.Dispose()
        TA0009tblView = Nothing

        '列追加
        TA0009tbl4EMS.Columns.Add("SAGYOUC", Type.GetType("System.String")) '作業部署
        TA0009tbl4EMS.Columns.Add("SAGYOUN", Type.GetType("System.String")) '作業部署名
        '残業時間
        TA0009tbl4EMS.Columns.Add("ZANGYOU", Type.GetType("System.String"))
        '拘束時間
        TA0009tbl4EMS.Columns.Add("KOUSOKU", Type.GetType("System.String"))
        'ハンドル時間
        TA0009tbl4EMS.Columns.Add("HANDORU", Type.GetType("System.String"))
        'デジタコ実績区分
        TA0009tbl4EMS.Columns.Add("JISSEKIKBN", Type.GetType("System.String"))

        Dim WW_TBLview As DataView
        Dim WW_GRPtbl As DataTable

        Dim WW_Cols As String() = {"NACOFFICESORG", "NACOFFICESORGNAME"}
        WW_TBLview = New DataView(TA0009tbl4EMS)
        WW_TBLview.Sort = "NACOFFICESORG"
        '出荷部署、出荷部署名でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        'データテーブルの行数分ループ
        Dim row As DataRow
        Dim rowNext As DataRow
        For i As Integer = 0 To TA0009tbl4EMS.Rows.Count - 1

            row = TA0009tbl4EMS.Rows(i)
            If i < TA0009tbl4EMS.Rows.Count - 1 Then rowNext = TA0009tbl4EMS.Rows(i + 1)

            '作業部署埋め込み
            row("SAGYOUC") = work.WF_SEL_SORG.Text
            For Each rowTMP As DataRow In WW_GRPtbl.Rows
                If rowTMP("NACOFFICESORG") = row("SAGYOUC") Then
                    row("SAGYOUN") = rowTMP("NACOFFICESORGNAME")
                End If
            Next

            '一時退避変数
            Dim calcTmp As Integer

            If CDate(row("PAYSHUSHADATE")).ToString("HHmmss") = "000000" AndAlso
               CDate(row("PAYTAISHADATE")).ToString("HHmmss") = "000000" Then
                '------ 日報から ---------------
                '残業時間
                '所定労働時間より大きければ稼働時間（退社－出社）－休憩－所定労働時間
                Dim WW_WORKTIME As Integer = DateDiff("n", row("NACHAISTDATE"), row("NACHAIENDDATE")) - CInt(row("NACTTLBREAKTIME"))
                If WW_WORKTIME > 455 Then
                    calcTmp = WW_WORKTIME - 455
                    If calcTmp <> 0 Then row("ZANGYOU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                End If
                '拘束時間
                calcTmp = DateDiff("n", row("NACHAISTDATE"), row("NACHAIENDDATE"))
                If calcTmp <> 0 Then row("KOUSOKU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                'ハンドル時間
                calcTmp = CInt(row("NACJITTLETIME")) + CInt(row("NACKUTTLTIME")) 'ハンドル時間の計算
                If calcTmp <> 0 Then row("HANDORU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                'デジタコ実績区分
                If DateDiff("n", row("NACHAISTDATE"), row("NACHAIENDDATE")) > 0 Then '拘束時間がゼロより大きいとき
                    row("JISSEKIKBN") = "日報仮"
                Else
                    row("JISSEKIKBN") = ""
                End If
            Else
                '------ 勤怠から ---------------
                '残業時間
                calcTmp = CInt(row("PAYORVERTIME")) + CInt(row("PAYWNIGHTTIME")) + CInt(row("PAYWSWORKTIME")) + CInt(row("PAYSNIGHTTIME")) + CInt(row("PAYHWORKTIME")) + CInt(row("PAYHNIGHTTIME"))
                If calcTmp <> 0 Then row("ZANGYOU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                '拘束時間
                calcTmp = DateDiff("n", row("PAYSHUSHADATE"), row("PAYTAISHADATE"))
                If calcTmp <> 0 Then row("KOUSOKU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                'ハンドル時間
                calcTmp = CInt(row("NACJITTLETIME")) + CInt(row("NACKUTTLTIME"))
                If calcTmp <> 0 Then row("HANDORU") = CStr(Int(calcTmp / 60)) & ":" & Format(calcTmp Mod 60, "00")
                '日報マーク
                row("JISSEKIKBN") = ""
            End If

        Next

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0009WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = "乗務員・労働時間管理表4EMS"    '帳票ID 固定
        CS0030REPORT.FILEtyp = "CSV"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0009tbl4EMS                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        TA0009tbl4EMS.Dispose()
        TA0009tbl4EMS = Nothing
        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_GRPtbl.Dispose()
        WW_GRPtbl = Nothing

    End Sub

    ''' <summary>
    ''' 照会ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINQ_Click()

        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If Not Master.RecoverTable(TA0009tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        '数値以外を除く
        Dim reg As New Regex("[^0-9]")

        WF_MAXORVERTIME.Text = reg.Replace(WF_MAXORVERTIME.Text, "")
        If WF_MAXORVERTIME.Text = "" Then WF_MAXORVERTIME.Text = 0

        WF_MAXWORKTIME.Text = reg.Replace(WF_MAXWORKTIME.Text, "")
        If WF_MAXWORKTIME.Text = "" Then WF_MAXWORKTIME.Text = 0

        '○T00009VIEWtbl取得
        GetViewTA0009()

        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        WF_SaveX.Value = 0
        WF_SaveY.Value = 0

    End Sub
    ''' <summary>
    ''' セレクタ変更ラジオボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_Selector_Change_Click()
        WF_SelectorMView.ActiveViewIndex = WF_SELECTOR_Chg.Value
        WF_SELECTOR_Chg.Value = String.Empty
    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If Not Master.RecoverTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0009WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0009VIEWtbl                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If Not Master.RecoverTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        CS0026TblSort.TABLE = TA0009VIEWtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "NACOFFICESORG,PAYSTAFFCODE,RECKBN"
        TA0009VIEWtbl = CS0026TblSort.sort()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0009WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0009VIEWtbl                    'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub
    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 画面遷移実行
        Master.transitionPrevPage()
    End Sub
    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()
        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If Not Master.RecoverTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        '■ GridView表示
        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If Not Master.RecoverTable(TA0009VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        '○ソート
        Using WW_TBLview As DataView = New DataView(TA0009VIEWtbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '■ GridView表示
            '○ 最終頁に移動
            If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
            Else
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
            End If
        End Using
    End Sub
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇フィールドダブルクリック時処理
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    .activeCalendar()
                Else
                    Dim prmData As Hashtable = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case 901
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TA0009_RECKBN")
                    End Select
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)

                    .activeListBox()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '〇ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' '〇TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '〇右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim values As String() = leftview.getActiveValue

        Select Case WF_FIELD.Value
            Case "WF_RECKBN"
                '会社コード　 
                WF_RECKBN.Text = values(1)
                WF_RECKBN.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>　                                     
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_RECKBN"
                'レコード区分　 
                WF_RECKBN.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理関連                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' T00009VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0009()

        '〇 T00009ALLよりデータ抽出
        Dim WW_Sort As String = ""
        Dim WW_Filter As String = ""

        Using WW_View As New DataView(TA0009tbl)

            WW_Sort = "NACOFFICESORG,PAYSTAFFCODE,RECKBN"
            If Not (String.IsNullOrEmpty(WF_SELECTOR_PosiORG.Value) OrElse WF_SELECTOR_PosiORG.Value = GRTA0009WRKINC.ALL_SELECTOR.CODE) Then
                WW_Filter = WW_Filter & "NACOFFICESORG = '" & WF_SELECTOR_PosiORG.Value & "'"
            End If

            If Not (String.IsNullOrEmpty(WF_SELECTOR_PosiORG.Value) OrElse WF_SELECTOR_PosiSTAFF.Value = GRTA0009WRKINC.ALL_SELECTOR.CODE) Then
                If WW_Filter <> "" Then WW_Filter = WW_Filter & " and "
                WW_Filter = WW_Filter & "PAYSTAFFCODE = '" & WF_SELECTOR_PosiSTAFF.Value & "'"
            End If

            WW_View.Sort = WW_Sort
            WW_View.RowFilter = WW_Filter

            TA0009VIEWtbl = WW_View.ToTable

        End Using

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0009VIEWrow As DataRow In TA0009VIEWtbl.Rows
            TA0009VIEWrow("LINECNT") = 0
            TA0009VIEWrow("MAXORVERTIME") = WF_MAXORVERTIME.Text
            TA0009VIEWrow("MAXWORKTIME") = WF_MAXWORKTIME.Text

            If TA0009VIEWrow("RECKBNNAME") = WF_RECKBN.Text OrElse WF_RECKBN.Text = "" Then
                TA0009VIEWrow("SELECT") = "1"
                TA0009VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0009VIEWrow("LINECNT") = WW_LINECNT
                If TA0009VIEWrow("RECKBN") = "01" Then
                    TA0009VIEWrow("TTLSA") = MinutesToHHMM((WF_MAXORVERTIME.Text) * 60 - HHMMToMinutes(TA0009VIEWrow("TTL")))
                ElseIf TA0009VIEWrow("RECKBN") = "02" Then
                    TA0009VIEWrow("TTLSA") = MinutesToHHMM((WF_MAXWORKTIME.Text) * 60 - HHMMToMinutes(TA0009VIEWrow("TTL")))
                End If
            Else
                TA0009VIEWrow("SELECT") = "0"
                TA0009VIEWrow("HIDDEN") = "1"      '表示
                If TA0009VIEWrow("RECKBN") = "01" Then
                    TA0009VIEWrow("TTLSA") = MinutesToHHMM((WF_MAXORVERTIME.Text) * 60 - HHMMToMinutes(TA0009VIEWrow("TTL")))
                ElseIf TA0009VIEWrow("RECKBN") = "02" Then
                    TA0009VIEWrow("TTLSA") = MinutesToHHMM((WF_MAXWORKTIME.Text) * 60 - HHMMToMinutes(TA0009VIEWrow("TTL")))
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 表示元データ(TA0009tbl)取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetMapData()

        '■ 表示元データ(条件によるサマリーデータ)取得
        'カラム設定
        AddColumnToTA0009tbl(TA0009tbl)

        Dim TA0009SELtbl As DataTable = TA0009tbl.Clone
        '--------------------------------
        '締め前（L0001_TOKEIより取得
        '--------------------------------
        '①自部署で作業した乗務員の日報、勤怠を自サーバーから取得（応援者を含む）
        GetTA0009Wk(TA0009tbl)

        '②他部署の乗務員の日報、勤怠を本社サーバーから取得（上記で取得した応援者のみ抽出）
        GetTA0009Wk2(TA0009tbl)

        '--------------------------------
        '締め後（L0004_SUMMARYKより取得
        '--------------------------------
        GetTA0009WkSum3(TA0009tbl)

        '①で取得した応援者分を除く（②で取得し直したため）
        CS0026TblSort.TABLE = TA0009tbl
        CS0026TblSort.FILTER = "SUPPORTKBN = '0'"
        CS0026TblSort.SORTING = "NACOFFICESORG, PAYSTAFFCODE,NACSHUKODATE"
        TA0009tbl = CS0026TblSort.sort()

        '縦並びを乗務員別に横並び（１日～３１日）にする
        SummaryTA0009WK(TA0009SELtbl)

        TA0009tbl = TA0009SELtbl.Copy
        TA0009SELtbl.Dispose()
        TA0009SELtbl = Nothing

        If TA0009tbl.Rows.Count > 65000 Then
            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ERR)
            TA0009tbl.Clear()
            Exit Sub
        End If

        '■ セレクター作成
        InitialSelector()

        '■ ソート
        CS0026TblSort.TABLE = TA0009tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "NACOFFICESORG,PAYSTAFFCODE,NACSHUKODATE"
        TA0009tbl = CS0026TblSort.sort()

        Dim wCNT As Integer = 0
        For Each TA0009row As DataRow In TA0009tbl.Rows
            If TA0009row("RECKBNNAME") = WF_RECKBN.Text Then
                wCNT = wCNT + 1
                TA0009row("LINECNT") = wCNT
                TA0009row("SELECT") = "1"
                TA0009row("HIDDEN") = "0"
            Else
                TA0009row("LINECNT") = "0"
                TA0009row("SELECT") = "0"
                TA0009row("HIDDEN") = "1"
            End If
        Next

    End Sub
    ''' <summary>
    ''' 部署一覧取得
    ''' </summary>
    ''' <returns>部署一覧</returns>
    ''' <remarks></remarks>
    Private Function GetORGList() As List(Of String)
        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As New List(Of String)
        Try

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(2000)
                SQLStr.AppendLine(" SELECT          S06.CAMPCODE , S06.CODE                ")
                SQLStr.AppendLine(" FROM            S0006_ROLE S06                         ")
                SQLStr.AppendLine(" INNER JOIN      M0002_ORG M02                          ")
                SQLStr.AppendLine(" ON              M02.CAMPCODE      =  S06.CAMPCODE      ")
                SQLStr.AppendLine("             and M02.ORGCODE       =  S06.CODE          ")
                SQLStr.AppendLine("             and M02.ORGLEVEL     in ('00010','00100')  ")
                SQLStr.AppendLine("             and M02.STYMD         <= @P03              ")
                SQLStr.AppendLine("             and M02.ENDYMD        >= @P03              ")
                SQLStr.AppendLine("             and M02.DELFLG        <> '1'               ")
                SQLStr.AppendLine(" LEFT JOIN       M0006_STRUCT M06 ON                    ")
                SQLStr.AppendLine("                 M06.CAMPCODE = S06.CAMPCODE            ")
                SQLStr.AppendLine("             and M06.OBJECT  = @P05                     ")
                SQLStr.AppendLine("             and M06.STRUCT  = @P06                     ")
                SQLStr.AppendLine("             and M06.STYMD  <= @P03                     ")
                SQLStr.AppendLine("             and M06.ENDYMD >= @P03                     ")
                SQLStr.AppendLine("             and M06.DELFLG <> '1'                      ")
                SQLStr.AppendLine(" WHERE                                                  ")
                SQLStr.AppendLine("                 S06.CAMPCODE      =  @P02              ")
                SQLStr.AppendLine("             and S06.OBJECT        = 'ORG'              ")
                SQLStr.AppendLine("             and S06.ROLE          =  @P01              ")
                SQLStr.AppendLine("             and S06.CODE       like  @P04 +'%'         ")
                SQLStr.AppendLine("             and S06.PERMITCODE    = '2'                ")
                SQLStr.AppendLine("             and S06.STYMD         <= @P03              ")
                SQLStr.AppendLine("             and S06.ENDYMD        >= @P03              ")
                SQLStr.AppendLine("             and S06.DELFLG        <> '1'               ")
                SQLStr.AppendLine("             and isnull(M06.GRCODE01,'') <> S06.CODE    ")
                SQLStr.AppendLine(" GROUP BY        S06.CAMPCODE , S06.CODE                ")

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)

                    Dim parm4 As String = ""

                    If work.WF_SEL_SORG.Text = "" Then
                        parm4 = ""
                    Else
                        Dim orgCode As String = ""
                        Dim retCode As String = ""
                        T0007COM.ConvORGCODE(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SORG.Text, orgCode, retCode)
                        If retCode = C_MESSAGE_NO.NORMAL Then
                            parm4 = orgCode
                        Else
                            parm4 = work.WF_SEL_SORG.Text
                        End If
                    End If

                    With SQLcmd.Parameters
                        .Add("@P01", SqlDbType.NVarChar, 20).Value = Master.ROLE_ORG
                        .Add("@P02", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@P03", SqlDbType.Date).Value = Date.Now
                        .Add("@P04", SqlDbType.NVarChar, 20).Value = parm4
                        .Add("@P05", SqlDbType.NVarChar, 20).Value = C_ROLE_VARIANT.USER_ORG
                        .Add("@P06", SqlDbType.NVarChar, 20).Value = "勤怠管理組織_営業所"
                    End With

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            W_ORGlst.Add(SQLdr("CODE"))
                        End While
                    End Using

                End Using
            End Using
            Return W_ORGlst

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0006_ROLE SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0006_ROLE Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return Nothing
            Exit Function
        End Try
    End Function

    ''' <summary>
    ''' 表示元データ(条件によるサマリー前データ)取得
    ''' </summary>
    ''' <param name="IO_TBL">表示元データテーブル</param>
    ''' <remarks></remarks>
    Private Sub GetTA0009Wk(ByRef IO_TBL As DataTable)

        '○初期クリア
        'TA0009tbl値設定
        Dim wINT As Integer
        Dim wDATE As Date
        Dim wDATETime As DateTime

        Dim W_ORGLst As List(Of String) = GetORGList()
        Using SQLcon As SqlConnection = CS0050Session.getConnection

            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As New StringBuilder(10000)
            SQLStr.AppendLine("     SELECT ")
            SQLStr.AppendLine("          isnull(rtrim(L01.CAMPCODE), '')             as CAMPCODE ")
            SQLStr.AppendLine("        , isnull(rtrim(M01.NAMES), '')                as CAMPNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.KEIJOYMD), '1950/01/01') as KEIJOYMD ")
            SQLStr.AppendLine("        , isnull(rtrim(M06.CODE), isnull(rtrim(L01.ACKEIJOORG), '')) as ACKEIJOORG ")
            SQLStr.AppendLine("        , isnull((select isnull(rtrim(M02.NAMES), '') from M0002_ORG M02  ")
            SQLStr.AppendLine("          where M02.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M02.ORGCODE = L01.ACKEIJOORG  ")
            SQLStr.AppendLine("          and M02.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02.DELFLG <> '1' ),'') as ACKEIJOORGNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.DENYMD), '1950/01/01') as DENYMD ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.DENNO), '') as DENNO ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.KANRENDENNO), '') as KANRENDENNO ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.DTLNO), '') as DTLNO ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.ACACHANTEI), '') as ACACHANTEI ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MC1_09.VALUE1), '') from  MC001_FIXVALUE MC1_09  ")
            SQLStr.AppendLine("          where MC1_09.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_09.CLASS = 'ACHANTEI'  ")
            SQLStr.AppendLine("          and MC1_09.KEYCODE = L01.ACACHANTEI  ")
            SQLStr.AppendLine("          and MC1_09.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_09.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_09.DELFLG <> '1' ) as ACACHANTEINAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACSHUKODATE), '1950/01/01') as NACSHUKODATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACHAISTDATE), '1950/01/01') as NACHAISTDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACHAIENDDATE), '1950/01/01') as NACHAIENDDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACHAIWORKTIME), '0') as NACHAIWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACGESSTDATE), '1950/01/01') as NACGESSTDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACGESENDDATE), '1950/01/01') as NACGESENDDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACGESWORKTIME), '0') as NACGESWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACCHOWORKTIME), '0') as NACCHOWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACTTLWORKTIME), '0') as NACTTLWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACOUTWORKTIME), '0') as NACOUTWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACJITTLETIME), '0') as NACJITTLETIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACKUTTLTIME), '0') as NACKUTTLTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACBREAKSTDATE), '1950/01/01') as NACBREAKSTDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACBREAKENDDATE), '1950/01/01') as NACBREAKENDDATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACBREAKTIME), '0') as NACBREAKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACCHOBREAKTIME), '0') as NACCHOBREAKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACTTLBREAKTIME), '0') as NACTTLBREAKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.NACOFFICESORG), '') as NACOFFICESORG ")
            SQLStr.AppendLine("        , isnull((select isnull(rtrim(M02_22.NAMES), '') from M0002_ORG M02_22  ")
            SQLStr.AppendLine("          where M02_22.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M02_22.ORGCODE = L01.NACOFFICESORG  ")
            SQLStr.AppendLine("          and M02_22.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_22.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_22.DELFLG <> '1' ),'') as NACOFFICESORGNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYSHUSHADATE), '1950/01/01') as PAYSHUSHADATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYTAISHADATE), '1950/01/01') as PAYTAISHADATE ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYSTAFFKBN), '') as PAYSTAFFKBN ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MC1_29.VALUE1), '') from MC001_FIXVALUE MC1_29  ")
            SQLStr.AppendLine("          where MC1_29.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_29.CLASS = 'STAFFKBN'  ")
            SQLStr.AppendLine("          and MC1_29.KEYCODE = L01.PAYSTAFFKBN  ")
            SQLStr.AppendLine("          and MC1_29.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_29.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_29.DELFLG <> '1' ) as PAYSTAFFKBNNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYSTAFFCODE), '') as PAYSTAFFCODE ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MB1_4.STAFFNAMES), '') from MB001_STAFF MB1_4  ")
            SQLStr.AppendLine("          where MB1_4.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MB1_4.STAFFCODE = L01.PAYSTAFFCODE  ")
            SQLStr.AppendLine("          and MB1_4.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MB1_4.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MB1_4.DELFLG <> '1' ) as PAYSTAFFCODENAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYMORG), '') as PAYMORG ")
            SQLStr.AppendLine("        , (select isnull(rtrim(M02_20.NAMES), '') from  M0002_ORG M02_20  ")
            SQLStr.AppendLine("          where M02_20.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M02_20.ORGCODE = L01.PAYMORG  ")
            SQLStr.AppendLine("          and M02_20.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_20.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_20.DELFLG <> '1' ) as PAYMORGNAME  ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYHORG), '') as PAYHORG ")
            SQLStr.AppendLine("        , (select isnull(rtrim(M02_21.NAMES), '') from M0002_ORG M02_21  ")
            SQLStr.AppendLine("          where M02_21.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M02_21.ORGCODE = L01.PAYHORG  ")
            SQLStr.AppendLine("          and M02_21.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_21.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M02_21.DELFLG <> '1' ) as PAYHORGNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYHOLIDAYKBN), '') as PAYHOLIDAYKBN ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MC1_40.VALUE1), '') from MC001_FIXVALUE MC1_40  ")
            SQLStr.AppendLine("          where MC1_40.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_40.CLASS = 'HOLIDAYKBN'  ")
            SQLStr.AppendLine("          and MC1_40.KEYCODE = L01.PAYHOLIDAYKBN  ")
            SQLStr.AppendLine("          and MC1_40.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_40.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_40.DELFLG <> '1' ) as PAYHOLIDAYKBNNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYKBN), '') as PAYKBN ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MC1_31.VALUE1), '') from MC001_FIXVALUE MC1_31  ")
            SQLStr.AppendLine("          where MC1_31.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_31.CLASS = 'PAYKBN'  ")
            SQLStr.AppendLine("          and MC1_31.KEYCODE = L01.PAYKBN  ")
            SQLStr.AppendLine("          and MC1_31.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_31.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_31.DELFLG <> '1' ) as PAYKBNNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYSHUKCHOKKBN), '') as PAYSHUKCHOKKBN ")
            SQLStr.AppendLine("        , isnull((select distinct isnull(rtrim(MC1_32.VALUE1), '') from MC001_FIXVALUE MC1_32  ")
            SQLStr.AppendLine("          where (MC1_32.CLASS = 'SHUKCHOKKBN'  ")
            SQLStr.AppendLine("          or   MC1_32.CLASS = 'T0009_SHUKCHOKKBN')  ")
            SQLStr.AppendLine("          and MC1_32.KEYCODE = L01.PAYSHUKCHOKKBN  ")
            SQLStr.AppendLine("          and MC1_32.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_32.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_32.DELFLG <> '1' ),'') as PAYSHUKCHOKKBNNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYJYOMUKBN), '') as PAYJYOMUKBN ")
            SQLStr.AppendLine("        , (select isnull(rtrim(MC1_33.VALUE1), '') from MC001_FIXVALUE MC1_33  ")
            SQLStr.AppendLine("          where MC1_33.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_33.CLASS = 'JYOMUKBN'  ")
            SQLStr.AppendLine("          and MC1_33.KEYCODE = L01.PAYJYOMUKBN  ")
            SQLStr.AppendLine("          and MC1_33.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_33.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_33.DELFLG <> '1' ) as PAYJYOMUKBNNAME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYWORKTIME), '0') as PAYWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYNIGHTTIME), '0') as PAYNIGHTTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYORVERTIME), '0') as PAYORVERTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYWNIGHTTIME), '0') as PAYWNIGHTTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYWSWORKTIME), '0') as PAYWSWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYSNIGHTTIME), '0') as PAYSNIGHTTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYHWORKTIME), '0') as PAYHWORKTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYHNIGHTTIME), '0') as PAYHNIGHTTIME ")
            SQLStr.AppendLine("        , isnull(rtrim(L01.PAYBREAKTIME), '0') as PAYBREAKTIME ")
            SQLStr.AppendLine("        , (case when isnull(rtrim(M06.CODE), L01.ACKEIJOORG) = L01.PAYHORG then '0' else '1' end) as SUPPORTKBN ")
            SQLStr.AppendLine("       FROM       L0001_TOKEI L01 ")
            SQLStr.AppendLine("       INNER JOIN M0001_CAMP M01 ON ")
            SQLStr.AppendLine("              M01.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M01.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M01.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M01.DELFLG <> '1'  ")
            SQLStr.AppendLine("       LEFT JOIN M0006_STRUCT M06 ON ")
            SQLStr.AppendLine("              M06.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and M06.OBJECT = @P06 ")
            SQLStr.AppendLine("          and M06.STRUCT = @P07 ")
            SQLStr.AppendLine("          and M06.GRCODE01 = L01.ACKEIJOORG ")
            SQLStr.AppendLine("          and M06.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M06.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and M06.DELFLG <> '1'  ")
            SQLStr.AppendLine("       WHERE  ")
            SQLStr.AppendLine("              L01.CAMPCODE       = @P02  ")
            SQLStr.AppendLine("          and (rtrim(isnull(M06.CODE, L01.ACKEIJOORG)) = @P05  ")
            SQLStr.AppendLine("                                  or    L01.PAYHORG = @P05)  ")
            SQLStr.AppendLine("          and L01.INQKBN         = '1'  ")
            SQLStr.AppendLine("          and L01.NACSHUKODATE  <= @P03  ")
            SQLStr.AppendLine("          and L01.NACSHUKODATE  >= @P04  ")
            SQLStr.AppendLine("          and L01.ACACHANTEI    IN ('HSC','HSD','KSC','KSD','RSC','RSD','HRC','HRD','HJC','HJD','HLC','HLD','KJC','KJD','KLC','KLD','ERC','ERD') ")
            SQLStr.AppendLine("          and L01.PAYSTAFFKBN like '03%'  ")
            SQLStr.AppendLine("          and L01.DELFLG        <> '1'  ")
            SQLStr.AppendLine("       ORDER BY ")
            SQLStr.AppendLine("              L01.PAYHORG, L01.PAYSTAFFCODE, L01.NACSHUKODATE, L01.ACACHANTEI DESC ")

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)

                '抽出条件(サーバー部署)List毎にデータ抽出
                For Each WI_ORG As String In W_ORGLst
                    '部署変換
                    Dim WW_ORG As String = ""
                    Dim WW_RTN As String = ""
                    ConvORGCode(WI_ORG, WW_ORG, WW_RTN)
                    If Not isNormal(WW_RTN) Then Exit Sub

                    '勤怠締テーブル取得
                    Dim WW_LIMITFLG As String = "0"
                    Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                    T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                       WW_ORG,
                                       work.WF_SEL_STYM.Text,
                                       WW_LIMITFLG,
                                       WW_ERR_RTN)
                    If Not isNormal(WW_ERR_RTN) Then
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                        Exit Sub
                    End If

                    '締まっていたらサマリーテーブルから取得するためスキップする
                    If WW_LIMITFLG = "1" Then Continue For

                    Try
                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = C_MAX_YMD
                        PARA04.Value = C_DEFAULT_YMD
                        PARA05.Value = WI_ORG
                        PARA06.Value = C_ROLE_VARIANT.USER_ORG
                        PARA07.Value = "勤怠管理組織_営業所"

                        '月末
                        Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")
                        PARA03.Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA04.Value = work.WF_SEL_STYM.Text & "/" & "01"

                        SQLcmd.CommandTimeout = 300
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            'ブレークKey
                            Dim WW_NACSHUKODATE As String = ""
                            Dim WW_ACKEIJOORG As String = ""
                            Dim WW_PAYSTAFFCODE As String = ""
                            '判定Key
                            Dim wNACSHUKODATE As String = ""
                            Dim wACKEIJOORG As String = ""
                            Dim wPAYSTAFFCODE As String = ""
                            Dim wSUM_NACHAIWORKTIME As Integer = 0                                                '実績・配送作業時間
                            Dim wSUM_NACGESWORKTIME As Integer = 0                                                '実績・下車作業時間
                            Dim wSUM_NACCHOWORKTIME As Integer = 0                                                '実績・勤怠調整時間
                            Dim wSUM_NACTTLWORKTIME As Integer = 0                                                '実績・配送合計時間Σ

                            Dim wSUM_NACOUTWORKTIME As Integer = 0                                                '実績・就業外時間
                            Dim wSUM_NACJITTLETIME As Integer = 0                                                 '実績・実車時間合計Σ
                            Dim wSUM_NACKUTTLTIME As Integer = 0                                                  '実績・空車時間合計Σ

                            Dim wSUM_NACBREAKTIME As Integer = 0                                                  '実績・休憩時間
                            Dim wSUM_NACCHOBREAKTIME As Integer = 0                                               '実績・休憩調整時間
                            Dim wSUM_NACTTLBREAKTIME As Integer = 0                                               '実績・休憩合計時間Σ
                            Dim wSUM_NACOFFICESORG As String = ""                                                 '実績・従業作業部署
                            Dim wSUM_NACOFFICESORGNAME As String = ""                                             '実績・従業作業部署名称
                            Dim wSUM_PAYWORKTIME As Integer = 0                                                   '所定労働時間
                            Dim wSUM_PAYNIGHTTIME As Integer = 0                                                  '所定深夜時間
                            Dim wSUM_PAYORVERTIME As Integer = 0                                                  '平日残業時間
                            Dim wSUM_PAYWNIGHTTIME As Integer = 0                                                 '平日深夜時間
                            Dim wSUM_PAYWSWORKTIME As Integer = 0                                                 '日曜出勤時間
                            Dim wSUM_PAYSNIGHTTIME As Integer = 0                                                 '日曜深夜時間
                            Dim wSUM_PAYHWORKTIME As Integer = 0                                                  '休日出勤時間
                            Dim wSUM_PAYHNIGHTTIME As Integer = 0                                                 '休日深夜時間
                            Dim wSUM_PAYBREAKTIME As Integer = 0                                                  '休憩時間
                            Dim wSUM_PAYSHUSHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYTAISHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYKBN As String = ""
                            Dim wSUM_PAYKBNNAME As String = ""
                            Dim wSUM_PAYSHUKCHOKKBN As String = ""
                            Dim wSUM_PAYSHUKCHOKKBNNAME As String = ""
                            Dim wSEQ As Integer = 0

                            'TA0009tbl.Clear()
                            Dim TA0009row As DataRow = Nothing
                            While SQLdr.Read

                                '〇判定Key作成
                                If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = SQLdr("NACSHUKODATE")
                                    wNACSHUKODATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACSHUKODATE = C_DEFAULT_YMD
                                End If
                                wACKEIJOORG = SQLdr("ACKEIJOORG")                                                 '計上部署
                                wPAYSTAFFCODE = SQLdr("PAYSTAFFCODE")                                             '従業員

                                '〇Keyブレーク時のレコード設定
                                If WW_NACSHUKODATE = wNACSHUKODATE AndAlso
                                   WW_ACKEIJOORG = wACKEIJOORG AndAlso
                                   WW_PAYSTAFFCODE = wPAYSTAFFCODE Then
                                Else
                                    '〇１件目
                                    If WW_NACSHUKODATE = "" AndAlso
                                       WW_ACKEIJOORG = "" AndAlso
                                       WW_PAYSTAFFCODE = "" Then

                                    Else
                                        '〇レコード出力
                                        '合計値セット
                                        TA0009row("NACOFFICESORG") = TA0009row("ACKEIJOORG")                                '実績・作業部署
                                        TA0009row("NACOFFICESORGNAME") = TA0009row("ACKEIJOORGNAME")                        '実績・作業部署名称
                                        TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                        TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                        TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                        TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                        TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                        TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                        TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                        TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                        TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                        TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                        TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                        TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                        TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                        TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                        TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                        TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                        TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                        TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                        TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                        TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                        TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                        TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                        TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                        TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                        TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                        TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                        TA0009row("RECKBN") = ""                           'レコード区分
                                        TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                        TA0009row("DAY01") = ""                            '1日
                                        TA0009row("DAY02") = ""                            '2日
                                        TA0009row("DAY03") = ""                            '3日
                                        TA0009row("DAY04") = ""                            '4日
                                        TA0009row("DAY05") = ""                            '5日
                                        TA0009row("DAY06") = ""                            '6日
                                        TA0009row("DAY07") = ""                            '7日
                                        TA0009row("DAY08") = ""                            '8日
                                        TA0009row("DAY09") = ""                            '9日
                                        TA0009row("DAY10") = ""                            '10日
                                        TA0009row("DAY11") = ""                            '11日
                                        TA0009row("DAY12") = ""                            '12日
                                        TA0009row("DAY13") = ""                            '13日
                                        TA0009row("DAY14") = ""                            '14日
                                        TA0009row("DAY15") = ""                            '15日
                                        TA0009row("DAY16") = ""                            '16日
                                        TA0009row("DAY17") = ""                            '17日
                                        TA0009row("DAY18") = ""                            '18日
                                        TA0009row("DAY19") = ""                            '19日
                                        TA0009row("DAY20") = ""                            '20日
                                        TA0009row("DAY21") = ""                            '21日
                                        TA0009row("DAY22") = ""                            '22日
                                        TA0009row("DAY23") = ""                            '23日
                                        TA0009row("DAY24") = ""                            '24日
                                        TA0009row("DAY25") = ""                            '25日
                                        TA0009row("DAY26") = ""                            '26日
                                        TA0009row("DAY27") = ""                            '27日
                                        TA0009row("DAY28") = ""                            '28日
                                        TA0009row("DAY29") = ""                            '29日
                                        TA0009row("DAY30") = ""                            '30日
                                        TA0009row("DAY31") = ""                            '31日
                                        TA0009row("TTL") = ""                              '累計
                                        TA0009row("TTLSA") = ""                            '累計差
                                        TA0009row("HOLKBN01") = ""
                                        TA0009row("HOLKBN02") = ""
                                        TA0009row("HOLKBN03") = ""
                                        TA0009row("HOLKBN04") = ""
                                        TA0009row("HOLKBN05") = ""
                                        TA0009row("HOLKBN06") = ""
                                        TA0009row("HOLKBN07") = ""
                                        TA0009row("HOLKBN08") = ""
                                        TA0009row("HOLKBN09") = ""
                                        TA0009row("HOLKBN10") = ""
                                        TA0009row("HOLKBN11") = ""
                                        TA0009row("HOLKBN12") = ""
                                        TA0009row("HOLKBN13") = ""
                                        TA0009row("HOLKBN14") = ""
                                        TA0009row("HOLKBN15") = ""
                                        TA0009row("HOLKBN16") = ""
                                        TA0009row("HOLKBN17") = ""
                                        TA0009row("HOLKBN18") = ""
                                        TA0009row("HOLKBN19") = ""
                                        TA0009row("HOLKBN20") = ""
                                        TA0009row("HOLKBN21") = ""
                                        TA0009row("HOLKBN22") = ""
                                        TA0009row("HOLKBN23") = ""
                                        TA0009row("HOLKBN24") = ""
                                        TA0009row("HOLKBN25") = ""
                                        TA0009row("HOLKBN26") = ""
                                        TA0009row("HOLKBN27") = ""
                                        TA0009row("HOLKBN28") = ""
                                        TA0009row("HOLKBN29") = ""
                                        TA0009row("HOLKBN30") = ""
                                        TA0009row("HOLKBN31") = ""

                                        IO_TBL.Rows.Add(TA0009row)

                                        wSUM_NACHAIWORKTIME = 0                                                '実績・配送作業時間
                                        wSUM_NACGESWORKTIME = 0                                                '実績・下車作業時間
                                        wSUM_NACCHOWORKTIME = 0                                                '実績・勤怠調整時間
                                        wSUM_NACTTLWORKTIME = 0                                                '実績・配送合計時間Σ

                                        wSUM_NACOUTWORKTIME = 0                                                '実績・就業外時間
                                        wSUM_NACJITTLETIME = 0                                                 '実績・実車時間合計Σ
                                        wSUM_NACKUTTLTIME = 0                                                  '実績・空車時間合計Σ

                                        wSUM_NACBREAKTIME = 0                                                  '実績・休憩時間
                                        wSUM_NACCHOBREAKTIME = 0                                               '実績・休憩調整時間
                                        wSUM_NACTTLBREAKTIME = 0                                               '実績・休憩合計時間Σ
                                        wSUM_NACOFFICESORG = ""                                                '実績・従業作業部署
                                        wSUM_NACOFFICESORGNAME = ""                                            '実績・従業作業部署名称
                                        wSUM_PAYWORKTIME = 0                                                   '所定労働時間
                                        wSUM_PAYNIGHTTIME = 0                                                  '所定深夜時間
                                        wSUM_PAYORVERTIME = 0                                                  '平日残業時間
                                        wSUM_PAYWNIGHTTIME = 0                                                 '平日深夜時間
                                        wSUM_PAYWSWORKTIME = 0                                                 '日曜出勤時間
                                        wSUM_PAYSNIGHTTIME = 0                                                 '日曜深夜時間
                                        wSUM_PAYHWORKTIME = 0                                                  '休日出勤時間
                                        wSUM_PAYHNIGHTTIME = 0                                                 '休日深夜時間
                                        wSUM_PAYBREAKTIME = 0                                                  '休憩時間
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                        wSUM_PAYKBN = ""
                                        wSUM_PAYKBNNAME = ""
                                        wSUM_PAYSHUKCHOKKBN = ""
                                        wSUM_PAYSHUKCHOKKBNNAME = ""
                                    End If

                                    '〇新レコード準備(固定項目設定)
                                    TA0009row = IO_TBL.NewRow

                                    wSEQ = 0

                                    'ブレイクキー設定
                                    WW_NACSHUKODATE = wNACSHUKODATE
                                    WW_ACKEIJOORG = wACKEIJOORG
                                    WW_PAYSTAFFCODE = wPAYSTAFFCODE

                                    wSUM_NACOFFICESORG = ""                                                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = ""                                                     '実績・従業作業部署名称
                                    wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    wSUM_PAYTAISHADATE = C_DEFAULT_YMD

                                    '固定項目
                                    TA0009row("LINECNT") = 0                                                        'DBの固定フィールド(2017/11/9)
                                    TA0009row("OPERATION") = ""                                                     'DBの固定フィールド(2017/11/9)
                                    TA0009row("TIMSTP") = 0                                                         'DBの固定フィールド(2017/11/9)
                                    TA0009row("SELECT") = "0"                                                       'DBの固定フィールド(2017/11/9)
                                    TA0009row("HIDDEN") = 0                                                         'DBの固定フィールド(2017/11/9)

                                    '画面固有項目
                                    TA0009row("CAMPCODE") = SQLdr("CAMPCODE")                                       '会社
                                    TA0009row("CAMPNAME") = SQLdr("CAMPNAME")                                       '会社名称
                                    If IsDate(SQLdr("KEIJOYMD")) AndAlso SQLdr("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                        wDATE = SQLdr("KEIJOYMD")
                                        TA0009row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("KEIJOYMD") = C_DEFAULT_YMD
                                    End If
                                    If IsDate(SQLdr("DENYMD")) AndAlso SQLdr("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                        wDATE = SQLdr("DENYMD")
                                        TA0009row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("DENYMD") = C_DEFAULT_YMD
                                    End If
                                    TA0009row("DENNO") = SQLdr("DENNO")                                             '伝票番号
                                    TA0009row("KANRENDENNO") = SQLdr("KANRENDENNO")                                 '関連伝票No＋明細No
                                    TA0009row("DTLNO") = SQLdr("DTLNO")                                             '明細番号
                                    TA0009row("ACACHANTEI") = SQLdr("ACACHANTEI")                                   '仕訳決定
                                    TA0009row("ACACHANTEINAME") = SQLdr("ACACHANTEINAME")                           '仕訳決定名称
                                    If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                        wDATE = SQLdr("NACSHUKODATE")
                                        TA0009row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("NACSHUKODATE") = C_DEFAULT_YMD
                                    End If

                                    TA0009row("NACHAISTDATE") = C_DEFAULT_YMD                                        '実績・配送作業開始日時
                                    TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD                                       '実績・配送作業終了日時

                                    TA0009row("NACGESSTDATE") = C_DEFAULT_YMD                                        '実績・下車作業開始日時
                                    TA0009row("NACGESENDDATE") = C_DEFAULT_YMD                                       '実績・下車作業終了日時

                                    TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD                                      '実績・休憩開始日時
                                    TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD                                     '実績・休憩終了日時

                                    TA0009row("PAYSHUSHADATE") = C_DEFAULT_YMD
                                    TA0009row("PAYTAISHADATE") = C_DEFAULT_YMD
                                End If

                                TA0009row("ACKEIJOORG") = SQLdr("ACKEIJOORG")                                   '計上部署
                                TA0009row("ACKEIJOORGNAME") = SQLdr("ACKEIJOORGNAME")                           '計上部署名称

                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBNNAME") = SQLdr("PAYSTAFFKBNNAME")                         '社員区分名称
                                TA0009row("PAYSTAFFCODE") = SQLdr("PAYSTAFFCODE")                               '従業員
                                TA0009row("PAYSTAFFCODENAME") = SQLdr("PAYSTAFFCODENAME")                       '従業員名称
                                TA0009row("PAYMORG") = SQLdr("PAYMORG")                                         '従業員管理部署
                                TA0009row("PAYMORGNAME") = SQLdr("PAYMORGNAME")                                 '従業員管理部署名称
                                TA0009row("PAYHORG") = SQLdr("PAYHORG")                                         '従業員配属部署
                                TA0009row("PAYHORGNAME") = SQLdr("PAYHORGNAME")                                 '従業員配属部署名称
                                TA0009row("PAYHOLIDAYKBN") = SQLdr("PAYHOLIDAYKBN")                             '休日区分
                                TA0009row("PAYHOLIDAYKBNNAME") = SQLdr("PAYHOLIDAYKBNNAME")                     '休日区分名称
                                TA0009row("PAYKBN") = SQLdr("PAYKBN")                                           '勤怠区分
                                TA0009row("PAYKBNNAME") = SQLdr("PAYKBNNAME")                                   '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = SQLdr("PAYSHUKCHOKKBN")                           '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = SQLdr("PAYSHUKCHOKKBNNAME")                   '宿日直区分名称
                                TA0009row("PAYJYOMUKBN") = SQLdr("PAYJYOMUKBN")                                 '乗務区分
                                TA0009row("PAYJYOMUKBNNAME") = SQLdr("PAYJYOMUKBNNAME")                         '乗務区分名称
                                TA0009row("SUPPORTKBN") = SQLdr("SUPPORTKBN")                                   '応援者区分

                                If SQLdr("ACACHANTEI") = "HSD" OrElse SQLdr("ACACHANTEI") = "HSC" Then
                                    '実績・配送作業開始日時
                                    If IsDate(SQLdr("NACHAISTDATE")) AndAlso SQLdr("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACHAISTDATE")
                                        TA0009row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACHAISTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・配送作業終了日時
                                    If IsDate(SQLdr("NACHAIENDDATE")) AndAlso SQLdr("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACHAIENDDATE")
                                        TA0009row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD
                                    End If

                                End If

                                If SQLdr("ACACHANTEI") = "KSD" OrElse SQLdr("ACACHANTEI") = "KSC" Then
                                    '実績・下車作業開始日時
                                    If IsDate(SQLdr("NACGESSTDATE")) AndAlso SQLdr("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACGESSTDATE")
                                        TA0009row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACGESSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・下車作業終了日時
                                    If IsDate(SQLdr("NACGESENDDATE")) AndAlso SQLdr("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACGESENDDATE")
                                        TA0009row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACGESENDDATE") = C_DEFAULT_YMD
                                    End If
                                End If

                                If SQLdr("ACACHANTEI") = "RSD" OrElse SQLdr("ACACHANTEI") = "RSC" Then
                                    '休憩開始日時
                                    If IsDate(SQLdr("NACBREAKSTDATE")) AndAlso SQLdr("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACBREAKSTDATE")
                                        TA0009row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '休憩終了日時
                                    If IsDate(SQLdr("NACBREAKENDDATE")) AndAlso SQLdr("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACBREAKENDDATE")
                                        TA0009row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                    End If
                                End If


                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "ERC" Then
                                    wSUM_NACOFFICESORG = SQLdr("NACOFFICESORG")                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = SQLdr("NACOFFICESORGNAME")                 '実績・従業作業部署名称
                                End If

                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "ERC" Then
                                    '出社日時
                                    If IsDate(SQLdr("PAYSHUSHADATE")) AndAlso SQLdr("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYSHUSHADATE")
                                        wSUM_PAYSHUSHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    End If

                                    '退社日時
                                    If IsDate(SQLdr("PAYTAISHADATE")) AndAlso SQLdr("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYTAISHADATE")
                                        wSUM_PAYTAISHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                    End If

                                    wSUM_PAYKBN = SQLdr("PAYKBN")                                       '勤怠区分
                                    wSUM_PAYKBNNAME = SQLdr("PAYKBNNAME")                               '勤怠区分名称
                                    wSUM_PAYSHUKCHOKKBN = SQLdr("PAYSHUKCHOKKBN")                       '宿日直区分
                                    wSUM_PAYSHUKCHOKKBNNAME = SQLdr("PAYSHUKCHOKKBNNAME")               '宿日直区分名称
                                End If


                                wINT = Val(SQLdr("NACHAIWORKTIME"))
                                wSUM_NACHAIWORKTIME = wSUM_NACHAIWORKTIME + wINT                                      '実績・配送作業時間

                                wINT = Val(SQLdr("NACGESWORKTIME"))
                                wSUM_NACGESWORKTIME = wSUM_NACGESWORKTIME + wINT                                      '実績・下車作業時間

                                wINT = Val(SQLdr("NACCHOWORKTIME"))
                                wSUM_NACCHOWORKTIME = wSUM_NACCHOWORKTIME + wINT                                      '実績・勤怠調整時間

                                wINT = Val(SQLdr("NACTTLWORKTIME"))
                                wSUM_NACTTLWORKTIME = wSUM_NACTTLWORKTIME + wINT                                      '実績・配送合計時間Σ

                                wINT = Val(SQLdr("NACOUTWORKTIME"))
                                wSUM_NACOUTWORKTIME = wSUM_NACOUTWORKTIME + wINT                                      '実績・就業外時間

                                wINT = Val(SQLdr("NACBREAKTIME"))
                                wSUM_NACBREAKTIME = wSUM_NACBREAKTIME + wINT                                          '実績・休憩時間

                                wINT = Val(SQLdr("NACCHOBREAKTIME"))
                                wSUM_NACCHOBREAKTIME = wSUM_NACCHOBREAKTIME + wINT                                    '実績・休憩調整時間

                                wINT = Val(SQLdr("NACTTLBREAKTIME"))
                                wSUM_NACTTLBREAKTIME = wSUM_NACTTLBREAKTIME + wINT                                    '実績・休憩合計時間Σ

                                wINT = Val(SQLdr("NACJITTLETIME"))
                                wSUM_NACJITTLETIME = wSUM_NACJITTLETIME + wINT                                        '実績・実車時間合計Σ

                                wINT = Val(SQLdr("NACKUTTLTIME"))
                                wSUM_NACKUTTLTIME = wSUM_NACKUTTLTIME + wINT                                          '実績・空車時間合計Σ

                                wINT = Val(SQLdr("PAYWORKTIME"))
                                wSUM_PAYWORKTIME = wSUM_PAYWORKTIME + wINT                                           '所定労働時間

                                wINT = Val(SQLdr("PAYNIGHTTIME"))
                                wSUM_PAYNIGHTTIME = wSUM_PAYNIGHTTIME + wINT                                         '所定深夜時間

                                wINT = Val(SQLdr("PAYORVERTIME"))
                                wSUM_PAYORVERTIME = wSUM_PAYORVERTIME + wINT                                         '平日残業時間

                                wINT = Val(SQLdr("PAYWNIGHTTIME"))
                                wSUM_PAYWNIGHTTIME = wSUM_PAYWNIGHTTIME + wINT                                       '平日深夜時間

                                wINT = Val(SQLdr("PAYWSWORKTIME"))
                                wSUM_PAYWSWORKTIME = wSUM_PAYWSWORKTIME + wINT                                       '日曜出勤時間

                                wINT = Val(SQLdr("PAYSNIGHTTIME"))
                                wSUM_PAYSNIGHTTIME = wSUM_PAYSNIGHTTIME + wINT                                       '日曜深夜時間

                                wINT = Val(SQLdr("PAYHWORKTIME"))
                                wSUM_PAYHWORKTIME = wSUM_PAYHWORKTIME + wINT                                         '休日出勤時間

                                wINT = Val(SQLdr("PAYHNIGHTTIME"))
                                wSUM_PAYHNIGHTTIME = wSUM_PAYHNIGHTTIME + wINT                                       '休日深夜時間

                                wINT = Val(SQLdr("PAYBREAKTIME"))
                                wSUM_PAYBREAKTIME = wSUM_PAYBREAKTIME + wINT                                         '休憩時間
                            End While
                            '〇最終レコード出力
                            If Not (WW_NACSHUKODATE = "" AndAlso
                                    WW_ACKEIJOORG = "" AndAlso
                                    WW_PAYSTAFFCODE = "") Then

                                '〇レコード出力
                                '合計値セット
                                TA0009row("NACOFFICESORG") = TA0009row("ACKEIJOORG")                                '実績・作業部署
                                TA0009row("NACOFFICESORGNAME") = TA0009row("ACKEIJOORGNAME")                        '実績・作業部署名称
                                TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                TA0009row("RECKBN") = ""                           'レコード区分
                                TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                TA0009row("DAY01") = ""                            '1日
                                TA0009row("DAY02") = ""                            '2日
                                TA0009row("DAY03") = ""                            '3日
                                TA0009row("DAY04") = ""                            '4日
                                TA0009row("DAY05") = ""                            '5日
                                TA0009row("DAY06") = ""                            '6日
                                TA0009row("DAY07") = ""                            '7日
                                TA0009row("DAY08") = ""                            '8日
                                TA0009row("DAY09") = ""                            '9日
                                TA0009row("DAY10") = ""                            '10日
                                TA0009row("DAY11") = ""                            '11日
                                TA0009row("DAY12") = ""                            '12日
                                TA0009row("DAY13") = ""                            '13日
                                TA0009row("DAY14") = ""                            '14日
                                TA0009row("DAY15") = ""                            '15日
                                TA0009row("DAY16") = ""                            '16日
                                TA0009row("DAY17") = ""                            '17日
                                TA0009row("DAY18") = ""                            '18日
                                TA0009row("DAY19") = ""                            '19日
                                TA0009row("DAY20") = ""                            '20日
                                TA0009row("DAY21") = ""                            '21日
                                TA0009row("DAY22") = ""                            '22日
                                TA0009row("DAY23") = ""                            '23日
                                TA0009row("DAY24") = ""                            '24日
                                TA0009row("DAY25") = ""                            '25日
                                TA0009row("DAY26") = ""                            '26日
                                TA0009row("DAY27") = ""                            '27日
                                TA0009row("DAY28") = ""                            '28日
                                TA0009row("DAY29") = ""                            '29日
                                TA0009row("DAY30") = ""                            '30日
                                TA0009row("DAY31") = ""                            '31日
                                TA0009row("TTL") = ""                              '累計
                                TA0009row("TTLSA") = ""                            '累計差
                                TA0009row("HOLKBN01") = ""
                                TA0009row("HOLKBN02") = ""
                                TA0009row("HOLKBN03") = ""
                                TA0009row("HOLKBN04") = ""
                                TA0009row("HOLKBN05") = ""
                                TA0009row("HOLKBN06") = ""
                                TA0009row("HOLKBN07") = ""
                                TA0009row("HOLKBN08") = ""
                                TA0009row("HOLKBN09") = ""
                                TA0009row("HOLKBN10") = ""
                                TA0009row("HOLKBN11") = ""
                                TA0009row("HOLKBN12") = ""
                                TA0009row("HOLKBN13") = ""
                                TA0009row("HOLKBN14") = ""
                                TA0009row("HOLKBN15") = ""
                                TA0009row("HOLKBN16") = ""
                                TA0009row("HOLKBN17") = ""
                                TA0009row("HOLKBN18") = ""
                                TA0009row("HOLKBN19") = ""
                                TA0009row("HOLKBN20") = ""
                                TA0009row("HOLKBN21") = ""
                                TA0009row("HOLKBN22") = ""
                                TA0009row("HOLKBN23") = ""
                                TA0009row("HOLKBN24") = ""
                                TA0009row("HOLKBN25") = ""
                                TA0009row("HOLKBN26") = ""
                                TA0009row("HOLKBN27") = ""
                                TA0009row("HOLKBN28") = ""
                                TA0009row("HOLKBN29") = ""
                                TA0009row("HOLKBN30") = ""
                                TA0009row("HOLKBN31") = ""

                                IO_TBL.Rows.Add(TA0009row)
                            End If
                        End Using


                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI SELECT")
                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:L0001_TOKEI Select"           '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' 表示元データ(条件によるサマリー前データ)取得
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Private Sub GetTA0009Wk2(ByRef IO_TBL As DataTable)

        '○初期クリア
        'TA0009tbl値設定
        Dim wINT As Integer
        Dim wDATE As Date
        Dim wDATETime As DateTime
        Dim WW_Cols As String() = {"PAYHORG", "PAYSTAFFCODE"}
        Dim WW_KEYtbl As DataTable
        Dim WW_TBLview As DataView

        '抽出条件(サーバー部署)List作成
        'キーテーブル作成
        WW_TBLview = New DataView(IO_TBL)
        WW_TBLview.RowFilter = "SUPPORTKBN = '1'"
        WW_KEYtbl = WW_TBLview.ToTable(True, WW_Cols)
        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        Using SQLcon As SqlConnection = CS0050Session.getConnection()

            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As New StringBuilder(10000)
            SQLStr.AppendLine(" SELECT ")
            SQLStr.AppendLine("       isnull(rtrim(L01.CAMPCODE), '') as CAMPCODE ")
            SQLStr.AppendLine("     , isnull(rtrim(M01.NAMES), '') as CAMPNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.KEIJOYMD), '1950/01/01') as KEIJOYMD ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYHORG), '') as ACKEIJOORG ")
            SQLStr.AppendLine("     , isnull((select isnull(rtrim(M02.NAMES), '') from M0002_ORG M02  ")
            SQLStr.AppendLine("       where M02.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and M02.ORGCODE = L01.PAYHORG  ")
            SQLStr.AppendLine("       and M02.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02.DELFLG <> '1' ),'') as ACKEIJOORGNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.DENYMD), '1950/01/01') as DENYMD ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.DENNO), '') as DENNO ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.KANRENDENNO), '') as KANRENDENNO ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.DTLNO), '') as DTLNO ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.ACACHANTEI), '') as ACACHANTEI ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MC1_09.VALUE1), '') from  MC001_FIXVALUE MC1_09  ")
            SQLStr.AppendLine("       where MC1_09.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and MC1_09.CLASS = 'ACHANTEI'  ")
            SQLStr.AppendLine("       and MC1_09.KEYCODE = L01.ACACHANTEI  ")
            SQLStr.AppendLine("       and MC1_09.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_09.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_09.DELFLG <> '1' ) as ACACHANTEINAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACSHUKODATE), '1950/01/01') as NACSHUKODATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACHAISTDATE), '1950/01/01') as NACHAISTDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACHAIENDDATE), '1950/01/01') as NACHAIENDDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACHAIWORKTIME), '0') as NACHAIWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACGESSTDATE), '1950/01/01') as NACGESSTDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACGESENDDATE), '1950/01/01') as NACGESENDDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACGESWORKTIME), '0') as NACGESWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACCHOWORKTIME), '0') as NACCHOWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACTTLWORKTIME), '0') as NACTTLWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACOUTWORKTIME), '0') as NACOUTWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACJITTLETIME), '0') as NACJITTLETIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACKUTTLTIME), '0') as NACKUTTLTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACBREAKSTDATE), '1950/01/01') as NACBREAKSTDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACBREAKENDDATE), '1950/01/01') as NACBREAKENDDATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACBREAKTIME), '0') as NACBREAKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACCHOBREAKTIME), '0') as NACCHOBREAKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACTTLBREAKTIME), '0') as NACTTLBREAKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.NACOFFICESORG), '') as NACOFFICESORG ")
            SQLStr.AppendLine("     , isnull((select isnull(rtrim(M02_22.NAMES), '') from M0002_ORG M02_22  ")
            SQLStr.AppendLine("       where M02_22.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and M02_22.ORGCODE = L01.NACOFFICESORG  ")
            SQLStr.AppendLine("       and M02_22.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_22.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_22.DELFLG <> '1' ),'') as NACOFFICESORGNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYSHUSHADATE), '1950/01/01') as PAYSHUSHADATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYTAISHADATE), '1950/01/01') as PAYTAISHADATE ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYSTAFFKBN), '') as PAYSTAFFKBN ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MC1_29.VALUE1), '') from MC001_FIXVALUE MC1_29  ")
            SQLStr.AppendLine("       where MC1_29.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("       and MC1_29.CLASS = 'STAFFKBN'  ")
            SQLStr.AppendLine("       and MC1_29.KEYCODE = L01.PAYSTAFFKBN  ")
            SQLStr.AppendLine("       and MC1_29.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_29.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_29.DELFLG <> '1' ) as PAYSTAFFKBNNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYSTAFFCODE), '') as PAYSTAFFCODE ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MB1_4.STAFFNAMES), '') from MB001_STAFF MB1_4  ")
            SQLStr.AppendLine("       where MB1_4.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and MB1_4.STAFFCODE = L01.PAYSTAFFCODE  ")
            SQLStr.AppendLine("       and MB1_4.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MB1_4.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MB1_4.DELFLG <> '1' ) as PAYSTAFFCODENAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYMORG), '') as PAYMORG ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(M02_20.NAMES), '') from  M0002_ORG M02_20  ")
            SQLStr.AppendLine("       where M02_20.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and M02_20.ORGCODE = L01.PAYMORG  ")
            SQLStr.AppendLine("       and M02_20.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_20.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_20.DELFLG <> '1' ) as PAYMORGNAME  ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYHORG), '') as PAYHORG ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(M02_21.NAMES), '') from M0002_ORG M02_21  ")
            SQLStr.AppendLine("       where M02_21.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and M02_21.ORGCODE = L01.PAYHORG  ")
            SQLStr.AppendLine("       and M02_21.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_21.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M02_21.DELFLG <> '1' ) as PAYHORGNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYHOLIDAYKBN), '') as PAYHOLIDAYKBN ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MC1_40.VALUE1), '') from MC001_FIXVALUE MC1_40  ")
            SQLStr.AppendLine("       where MC1_40.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("       and MC1_40.CLASS = 'HOLIDAYKBN'  ")
            SQLStr.AppendLine("       and MC1_40.KEYCODE = L01.PAYHOLIDAYKBN  ")
            SQLStr.AppendLine("       and MC1_40.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_40.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_40.DELFLG <> '1' ) as PAYHOLIDAYKBNNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYKBN), '') as PAYKBN ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MC1_31.VALUE1), '') from MC001_FIXVALUE MC1_31  ")
            SQLStr.AppendLine("       where MC1_31.CAMPCODE =  L01.CAMPCODE  ")
            SQLStr.AppendLine("       and MC1_31.CLASS = 'PAYKBN'  ")
            SQLStr.AppendLine("       and MC1_31.KEYCODE = L01.PAYKBN  ")
            SQLStr.AppendLine("       and MC1_31.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_31.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_31.DELFLG <> '1' ) as PAYKBNNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYSHUKCHOKKBN), '') as PAYSHUKCHOKKBN ")
            SQLStr.AppendLine("     ,isnull((select distinct isnull(rtrim(MC1_32.VALUE1), '') from MC001_FIXVALUE MC1_32  ")
            SQLStr.AppendLine("       where (MC1_32.CLASS = 'SHUKCHOKKBN'  ")
            SQLStr.AppendLine("       or   MC1_32.CLASS = 'T0009_SHUKCHOKKBN')  ")
            SQLStr.AppendLine("       and MC1_32.CAMPCODE = L01.CAMPCODE ")
            SQLStr.AppendLine("       and MC1_32.KEYCODE = L01.PAYSHUKCHOKKBN  ")
            SQLStr.AppendLine("       and MC1_32.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_32.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_32.DELFLG <> '1' ),'') as PAYSHUKCHOKKBNNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYJYOMUKBN), '') as PAYJYOMUKBN ")
            SQLStr.AppendLine("     ,(select isnull(rtrim(MC1_33.VALUE1), '') from MC001_FIXVALUE MC1_33  ")
            SQLStr.AppendLine("       where MC1_33.CAMPCODE = 'Default'  ")
            SQLStr.AppendLine("       and MC1_33.CLASS = 'JYOMUKBN'  ")
            SQLStr.AppendLine("       and MC1_33.KEYCODE = L01.PAYJYOMUKBN  ")
            SQLStr.AppendLine("       and MC1_33.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_33.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and MC1_33.DELFLG <> '1' ) as PAYJYOMUKBNNAME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYWORKTIME), '0') as PAYWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYNIGHTTIME), '0') as PAYNIGHTTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYORVERTIME), '0') as PAYORVERTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYWNIGHTTIME), '0') as PAYWNIGHTTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYWSWORKTIME), '0') as PAYWSWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYSNIGHTTIME), '0') as PAYSNIGHTTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYHWORKTIME), '0') as PAYHWORKTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYHNIGHTTIME), '0') as PAYHNIGHTTIME ")
            SQLStr.AppendLine("     , isnull(rtrim(L01.PAYBREAKTIME), '0') as PAYBREAKTIME ")
            SQLStr.AppendLine("     , '0' as SUPPORTKBN ")
            SQLStr.AppendLine("    FROM L0001_TOKEI L01 ")
            SQLStr.AppendLine("    INNER JOIN M0001_CAMP M01  ")
            SQLStr.AppendLine("       ON M01.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("       and M01.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M01.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("       and M01.DELFLG <> '1'  ")
            SQLStr.AppendLine("    WHERE  ")
            SQLStr.AppendLine("           L01.CAMPCODE = @P02  ")
            SQLStr.AppendLine("       and L01.PAYHORG  = @P05  ")
            SQLStr.AppendLine("       and L01.INQKBN = '1'  ")
            SQLStr.AppendLine("       and L01.NACSHUKODATE <= @P03  ")
            SQLStr.AppendLine("       and L01.NACSHUKODATE >= @P04  ")
            SQLStr.AppendLine("       and L01.ACACHANTEI IN ('HSC','HSD','KSC','KSD','RSC','RSD','HRC','HRD','HJC','HJD','HLC','HLD','KJC','KJD','KLC','KLD','ERC','ERD') ")
            SQLStr.AppendLine("       and L01.PAYSTAFFKBN like '03%'  ")
            SQLStr.AppendLine("       and L01.PAYSTAFFCODE  = @P06  ")
            SQLStr.AppendLine("       and L01.DELFLG <> '1'  ")
            SQLStr.AppendLine("    ORDER BY ")
            SQLStr.AppendLine("           L01.PAYHORG, L01.PAYSTAFFCODE, L01.NACSHUKODATE, L01.ACACHANTEI DESC ")

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)

                '抽出条件(サーバー部署)List毎にデータ抽出
                For wSTAFFcnt As Integer = 0 To WW_KEYtbl.Rows.Count - 1
                    Try
                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = C_MAX_YMD
                        PARA04.Value = C_DEFAULT_YMD
                        PARA05.Value = WW_KEYtbl.Rows(wSTAFFcnt)("PAYHORG")
                        PARA06.Value = WW_KEYtbl.Rows(wSTAFFcnt)("PAYSTAFFCODE")
                        '月末
                        Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")
                        PARA03.Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA04.Value = work.WF_SEL_STYM.Text & "/" & "01"

                        SQLcmd.CommandTimeout = 300
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            'ブレークKey
                            Dim WW_NACSHUKODATE As String = ""
                            Dim WW_ACKEIJOORG As String = ""
                            Dim WW_PAYSTAFFCODE As String = ""
                            '判定Key
                            Dim wNACSHUKODATE As String = ""
                            Dim wACKEIJOORG As String = ""
                            Dim wPAYSTAFFCODE As String = ""
                            Dim wSUM_NACHAIWORKTIME As Integer = 0                                                '実績・配送作業時間
                            Dim wSUM_NACGESWORKTIME As Integer = 0                                                '実績・下車作業時間
                            Dim wSUM_NACCHOWORKTIME As Integer = 0                                                '実績・勤怠調整時間
                            Dim wSUM_NACTTLWORKTIME As Integer = 0                                                '実績・配送合計時間Σ

                            Dim wSUM_NACOUTWORKTIME As Integer = 0                                                '実績・就業外時間
                            Dim wSUM_NACJITTLETIME As Integer = 0                                                 '実績・実車時間合計Σ
                            Dim wSUM_NACKUTTLTIME As Integer = 0                                                  '実績・空車時間合計Σ

                            Dim wSUM_NACBREAKTIME As Integer = 0                                                  '実績・休憩時間
                            Dim wSUM_NACCHOBREAKTIME As Integer = 0                                               '実績・休憩調整時間
                            Dim wSUM_NACTTLBREAKTIME As Integer = 0                                               '実績・休憩合計時間Σ
                            Dim wSUM_NACOFFICESORG As String = ""                                                 '実績・従業作業部署
                            Dim wSUM_NACOFFICESORGNAME As String = ""                                             '実績・従業作業部署名称
                            Dim wSUM_PAYWORKTIME As Integer = 0                                                   '所定労働時間
                            Dim wSUM_PAYNIGHTTIME As Integer = 0                                                  '所定深夜時間
                            Dim wSUM_PAYORVERTIME As Integer = 0                                                  '平日残業時間
                            Dim wSUM_PAYWNIGHTTIME As Integer = 0                                                 '平日深夜時間
                            Dim wSUM_PAYWSWORKTIME As Integer = 0                                                 '日曜出勤時間
                            Dim wSUM_PAYSNIGHTTIME As Integer = 0                                                 '日曜深夜時間
                            Dim wSUM_PAYHWORKTIME As Integer = 0                                                  '休日出勤時間
                            Dim wSUM_PAYHNIGHTTIME As Integer = 0                                                 '休日深夜時間
                            Dim wSUM_PAYBREAKTIME As Integer = 0                                                  '休憩時間
                            Dim wSUM_PAYSHUSHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYTAISHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYKBN As String = ""
                            Dim wSUM_PAYKBNNAME As String = ""
                            Dim wSUM_PAYSHUKCHOKKBN As String = ""
                            Dim wSUM_PAYSHUKCHOKKBNNAME As String = ""
                            Dim wSEQ As Integer = 0

                            'TA0009tbl.Clear()
                            Dim TA0009row As DataRow = Nothing

                            While SQLdr.Read

                                '〇判定Key作成
                                If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = SQLdr("NACSHUKODATE")
                                    wNACSHUKODATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACSHUKODATE = C_DEFAULT_YMD
                                End If
                                wACKEIJOORG = SQLdr("ACKEIJOORG")                                                 '計上部署
                                wPAYSTAFFCODE = SQLdr("PAYSTAFFCODE")                                             '従業員

                                '〇Keyブレーク時のレコード設定
                                If WW_NACSHUKODATE = wNACSHUKODATE AndAlso
                                   WW_ACKEIJOORG = wACKEIJOORG AndAlso
                                   WW_PAYSTAFFCODE = wPAYSTAFFCODE Then
                                Else
                                    '〇１件目
                                    If WW_NACSHUKODATE = "" AndAlso
                                       WW_ACKEIJOORG = "" AndAlso
                                       WW_PAYSTAFFCODE = "" Then

                                    Else
                                        '〇レコード出力
                                        '合計値セット
                                        TA0009row("NACOFFICESORG") = TA0009row("PAYHORG")                                   '実績・作業部署
                                        TA0009row("NACOFFICESORGNAME") = TA0009row("PAYHORGNAME")                           '実績・作業部署名称
                                        TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                        TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                        TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                        TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                        TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                        TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                        TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                        TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                        TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                        TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                        TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                        TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                        TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                        TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                        TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                        TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                        TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                        TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                        TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                        TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                        TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                        TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                        TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                        TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                        TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                        TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                        TA0009row("RECKBN") = ""                           'レコード区分
                                        TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                        TA0009row("DAY01") = ""                            '1日
                                        TA0009row("DAY02") = ""                            '2日
                                        TA0009row("DAY03") = ""                            '3日
                                        TA0009row("DAY04") = ""                            '4日
                                        TA0009row("DAY05") = ""                            '5日
                                        TA0009row("DAY06") = ""                            '6日
                                        TA0009row("DAY07") = ""                            '7日
                                        TA0009row("DAY08") = ""                            '8日
                                        TA0009row("DAY09") = ""                            '9日
                                        TA0009row("DAY10") = ""                            '10日
                                        TA0009row("DAY11") = ""                            '11日
                                        TA0009row("DAY12") = ""                            '12日
                                        TA0009row("DAY13") = ""                            '13日
                                        TA0009row("DAY14") = ""                            '14日
                                        TA0009row("DAY15") = ""                            '15日
                                        TA0009row("DAY16") = ""                            '16日
                                        TA0009row("DAY17") = ""                            '17日
                                        TA0009row("DAY18") = ""                            '18日
                                        TA0009row("DAY19") = ""                            '19日
                                        TA0009row("DAY20") = ""                            '20日
                                        TA0009row("DAY21") = ""                            '21日
                                        TA0009row("DAY22") = ""                            '22日
                                        TA0009row("DAY23") = ""                            '23日
                                        TA0009row("DAY24") = ""                            '24日
                                        TA0009row("DAY25") = ""                            '25日
                                        TA0009row("DAY26") = ""                            '26日
                                        TA0009row("DAY27") = ""                            '27日
                                        TA0009row("DAY28") = ""                            '28日
                                        TA0009row("DAY29") = ""                            '29日
                                        TA0009row("DAY30") = ""                            '30日
                                        TA0009row("DAY31") = ""                            '31日
                                        TA0009row("TTL") = ""                              '累計
                                        TA0009row("TTLSA") = ""                            '累計差
                                        TA0009row("HOLKBN01") = ""
                                        TA0009row("HOLKBN02") = ""
                                        TA0009row("HOLKBN03") = ""
                                        TA0009row("HOLKBN04") = ""
                                        TA0009row("HOLKBN05") = ""
                                        TA0009row("HOLKBN06") = ""
                                        TA0009row("HOLKBN07") = ""
                                        TA0009row("HOLKBN08") = ""
                                        TA0009row("HOLKBN09") = ""
                                        TA0009row("HOLKBN10") = ""
                                        TA0009row("HOLKBN11") = ""
                                        TA0009row("HOLKBN12") = ""
                                        TA0009row("HOLKBN13") = ""
                                        TA0009row("HOLKBN14") = ""
                                        TA0009row("HOLKBN15") = ""
                                        TA0009row("HOLKBN16") = ""
                                        TA0009row("HOLKBN17") = ""
                                        TA0009row("HOLKBN18") = ""
                                        TA0009row("HOLKBN19") = ""
                                        TA0009row("HOLKBN20") = ""
                                        TA0009row("HOLKBN21") = ""
                                        TA0009row("HOLKBN22") = ""
                                        TA0009row("HOLKBN23") = ""
                                        TA0009row("HOLKBN24") = ""
                                        TA0009row("HOLKBN25") = ""
                                        TA0009row("HOLKBN26") = ""
                                        TA0009row("HOLKBN27") = ""
                                        TA0009row("HOLKBN28") = ""
                                        TA0009row("HOLKBN29") = ""
                                        TA0009row("HOLKBN30") = ""
                                        TA0009row("HOLKBN31") = ""

                                        IO_TBL.Rows.Add(TA0009row)

                                        wSUM_NACHAIWORKTIME = 0                                                '実績・配送作業時間
                                        wSUM_NACGESWORKTIME = 0                                                '実績・下車作業時間
                                        wSUM_NACCHOWORKTIME = 0                                                '実績・勤怠調整時間
                                        wSUM_NACTTLWORKTIME = 0                                                '実績・配送合計時間Σ

                                        wSUM_NACOUTWORKTIME = 0                                                '実績・就業外時間
                                        wSUM_NACJITTLETIME = 0                                                 '実績・実車時間合計Σ
                                        wSUM_NACKUTTLTIME = 0                                                  '実績・空車時間合計Σ

                                        wSUM_NACBREAKTIME = 0                                                  '実績・休憩時間
                                        wSUM_NACCHOBREAKTIME = 0                                               '実績・休憩調整時間
                                        wSUM_NACTTLBREAKTIME = 0                                               '実績・休憩合計時間Σ
                                        wSUM_NACOFFICESORG = ""                                                '実績・従業作業部署
                                        wSUM_NACOFFICESORGNAME = ""                                            '実績・従業作業部署名称
                                        wSUM_PAYWORKTIME = 0                                                   '所定労働時間
                                        wSUM_PAYNIGHTTIME = 0                                                  '所定深夜時間
                                        wSUM_PAYORVERTIME = 0                                                  '平日残業時間
                                        wSUM_PAYWNIGHTTIME = 0                                                 '平日深夜時間
                                        wSUM_PAYWSWORKTIME = 0                                                 '日曜出勤時間
                                        wSUM_PAYSNIGHTTIME = 0                                                 '日曜深夜時間
                                        wSUM_PAYHWORKTIME = 0                                                  '休日出勤時間
                                        wSUM_PAYHNIGHTTIME = 0                                                 '休日深夜時間
                                        wSUM_PAYBREAKTIME = 0                                                  '休憩時間
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                        wSUM_PAYKBN = ""
                                        wSUM_PAYKBNNAME = ""
                                        wSUM_PAYSHUKCHOKKBN = ""
                                        wSUM_PAYSHUKCHOKKBNNAME = ""
                                    End If

                                    '〇新レコード準備(固定項目設定)
                                    TA0009row = IO_TBL.NewRow

                                    wSEQ = 0

                                    'ブレイクキー設定
                                    WW_NACSHUKODATE = wNACSHUKODATE
                                    WW_ACKEIJOORG = wACKEIJOORG
                                    WW_PAYSTAFFCODE = wPAYSTAFFCODE

                                    wSUM_NACOFFICESORG = ""                                                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = ""                                                     '実績・従業作業部署名称
                                    wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    wSUM_PAYTAISHADATE = C_DEFAULT_YMD

                                    '固定項目
                                    TA0009row("LINECNT") = 0                                                        'DBの固定フィールド(2017/11/9)
                                    TA0009row("OPERATION") = ""                                                     'DBの固定フィールド(2017/11/9)
                                    TA0009row("TIMSTP") = 0                                                         'DBの固定フィールド(2017/11/9)
                                    TA0009row("SELECT") = "0"                                                       'DBの固定フィールド(2017/11/9)
                                    TA0009row("HIDDEN") = 0                                                         'DBの固定フィールド(2017/11/9)

                                    '画面固有項目
                                    TA0009row("CAMPCODE") = SQLdr("CAMPCODE")                                       '会社
                                    TA0009row("CAMPNAME") = SQLdr("CAMPNAME")                                       '会社名称
                                    If IsDate(SQLdr("KEIJOYMD")) AndAlso SQLdr("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                        wDATE = SQLdr("KEIJOYMD")
                                        TA0009row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("KEIJOYMD") = C_DEFAULT_YMD
                                    End If
                                    If IsDate(SQLdr("DENYMD")) AndAlso SQLdr("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                        wDATE = SQLdr("DENYMD")
                                        TA0009row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("DENYMD") = C_DEFAULT_YMD
                                    End If
                                    TA0009row("DENNO") = SQLdr("DENNO")                                             '伝票番号
                                    TA0009row("KANRENDENNO") = SQLdr("KANRENDENNO")                                 '関連伝票No＋明細No
                                    TA0009row("DTLNO") = SQLdr("DTLNO")                                             '明細番号
                                    TA0009row("ACACHANTEI") = SQLdr("ACACHANTEI")                                   '仕訳決定
                                    TA0009row("ACACHANTEINAME") = SQLdr("ACACHANTEINAME")                           '仕訳決定名称
                                    If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                        wDATE = SQLdr("NACSHUKODATE")
                                        TA0009row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("NACSHUKODATE") = C_DEFAULT_YMD
                                    End If

                                    TA0009row("NACHAISTDATE") = C_DEFAULT_YMD                                        '実績・配送作業開始日時
                                    TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD                                       '実績・配送作業終了日時

                                    TA0009row("NACGESSTDATE") = C_DEFAULT_YMD                                        '実績・下車作業開始日時
                                    TA0009row("NACGESENDDATE") = C_DEFAULT_YMD                                       '実績・下車作業終了日時

                                    TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD                                      '実績・休憩開始日時
                                    TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD                                     '実績・休憩終了日時

                                    TA0009row("PAYSHUSHADATE") = C_DEFAULT_YMD
                                    TA0009row("PAYTAISHADATE") = C_DEFAULT_YMD
                                End If

                                TA0009row("ACKEIJOORG") = SQLdr("ACKEIJOORG")                                   '計上部署
                                TA0009row("ACKEIJOORGNAME") = SQLdr("ACKEIJOORGNAME")                           '計上部署名称

                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBNNAME") = SQLdr("PAYSTAFFKBNNAME")                         '社員区分名称
                                TA0009row("PAYSTAFFCODE") = SQLdr("PAYSTAFFCODE")                               '従業員
                                TA0009row("PAYSTAFFCODENAME") = SQLdr("PAYSTAFFCODENAME")                       '従業員名称
                                TA0009row("PAYMORG") = SQLdr("PAYMORG")                                         '従業員管理部署
                                TA0009row("PAYMORGNAME") = SQLdr("PAYMORGNAME")                                 '従業員管理部署名称
                                TA0009row("PAYHORG") = SQLdr("PAYHORG")                                         '従業員配属部署
                                TA0009row("PAYHORGNAME") = SQLdr("PAYHORGNAME")                                 '従業員配属部署名称
                                TA0009row("PAYHOLIDAYKBN") = SQLdr("PAYHOLIDAYKBN")                             '休日区分
                                TA0009row("PAYHOLIDAYKBNNAME") = SQLdr("PAYHOLIDAYKBNNAME")                     '休日区分名称
                                TA0009row("PAYKBN") = SQLdr("PAYKBN")                                           '勤怠区分
                                TA0009row("PAYKBNNAME") = SQLdr("PAYKBNNAME")                                   '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = SQLdr("PAYSHUKCHOKKBN")                           '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = SQLdr("PAYSHUKCHOKKBNNAME")                   '宿日直区分名称
                                TA0009row("PAYJYOMUKBN") = SQLdr("PAYJYOMUKBN")                                 '乗務区分
                                TA0009row("PAYJYOMUKBNNAME") = SQLdr("PAYJYOMUKBNNAME")                         '乗務区分名称
                                TA0009row("SUPPORTKBN") = SQLdr("SUPPORTKBN")                                   '応援者区分

                                If SQLdr("ACACHANTEI") = "HSD" OrElse SQLdr("ACACHANTEI") = "HSC" Then
                                    '実績・配送作業開始日時
                                    If IsDate(SQLdr("NACHAISTDATE")) AndAlso SQLdr("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACHAISTDATE")
                                        TA0009row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACHAISTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・配送作業終了日時
                                    If IsDate(SQLdr("NACHAIENDDATE")) AndAlso SQLdr("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACHAIENDDATE")
                                        TA0009row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD
                                    End If

                                End If

                                If SQLdr("ACACHANTEI") = "KSD" OrElse SQLdr("ACACHANTEI") = "KSC" Then
                                    '実績・下車作業開始日時
                                    If IsDate(SQLdr("NACGESSTDATE")) AndAlso SQLdr("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACGESSTDATE")
                                        TA0009row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACGESSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・下車作業終了日時
                                    If IsDate(SQLdr("NACGESENDDATE")) AndAlso SQLdr("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACGESENDDATE")
                                        TA0009row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACGESENDDATE") = C_DEFAULT_YMD
                                    End If
                                End If

                                If SQLdr("ACACHANTEI") = "RSD" OrElse SQLdr("ACACHANTEI") = "RSC" Then
                                    '休憩開始日時
                                    If IsDate(SQLdr("NACBREAKSTDATE")) AndAlso SQLdr("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACBREAKSTDATE")
                                        TA0009row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '休憩終了日時
                                    If IsDate(SQLdr("NACBREAKENDDATE")) AndAlso SQLdr("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("NACBREAKENDDATE")
                                        TA0009row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                    End If
                                End If


                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "ERC" Then
                                    wSUM_NACOFFICESORG = SQLdr("NACOFFICESORG")                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = SQLdr("NACOFFICESORGNAME")                 '実績・従業作業部署名称
                                End If

                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "ERC" Then
                                    '出社日時
                                    If IsDate(SQLdr("PAYSHUSHADATE")) AndAlso SQLdr("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYSHUSHADATE")
                                        wSUM_PAYSHUSHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    End If

                                    '退社日時
                                    If IsDate(SQLdr("PAYTAISHADATE")) AndAlso SQLdr("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYTAISHADATE")
                                        wSUM_PAYTAISHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                    End If

                                    wSUM_PAYKBN = SQLdr("PAYKBN")                                       '勤怠区分
                                    wSUM_PAYKBNNAME = SQLdr("PAYKBNNAME")                               '勤怠区分名称
                                    wSUM_PAYSHUKCHOKKBN = SQLdr("PAYSHUKCHOKKBN")                       '宿日直区分
                                    wSUM_PAYSHUKCHOKKBNNAME = SQLdr("PAYSHUKCHOKKBNNAME")               '宿日直区分名称
                                End If


                                wINT = Val(SQLdr("NACHAIWORKTIME"))
                                wSUM_NACHAIWORKTIME = wSUM_NACHAIWORKTIME + wINT                                      '実績・配送作業時間

                                wINT = Val(SQLdr("NACGESWORKTIME"))
                                wSUM_NACGESWORKTIME = wSUM_NACGESWORKTIME + wINT                                      '実績・下車作業時間

                                wINT = Val(SQLdr("NACCHOWORKTIME"))
                                wSUM_NACCHOWORKTIME = wSUM_NACCHOWORKTIME + wINT                                      '実績・勤怠調整時間

                                wINT = Val(SQLdr("NACTTLWORKTIME"))
                                wSUM_NACTTLWORKTIME = wSUM_NACTTLWORKTIME + wINT                                      '実績・配送合計時間Σ

                                wINT = Val(SQLdr("NACOUTWORKTIME"))
                                wSUM_NACOUTWORKTIME = wSUM_NACOUTWORKTIME + wINT                                      '実績・就業外時間

                                wINT = Val(SQLdr("NACBREAKTIME"))
                                wSUM_NACBREAKTIME = wSUM_NACBREAKTIME + wINT                                          '実績・休憩時間

                                wINT = Val(SQLdr("NACCHOBREAKTIME"))
                                wSUM_NACCHOBREAKTIME = wSUM_NACCHOBREAKTIME + wINT                                    '実績・休憩調整時間

                                wINT = Val(SQLdr("NACTTLBREAKTIME"))
                                wSUM_NACTTLBREAKTIME = wSUM_NACTTLBREAKTIME + wINT                                    '実績・休憩合計時間Σ

                                wINT = Val(SQLdr("NACJITTLETIME"))
                                wSUM_NACJITTLETIME = wSUM_NACJITTLETIME + wINT                                        '実績・実車時間合計Σ

                                wINT = Val(SQLdr("NACKUTTLTIME"))
                                wSUM_NACKUTTLTIME = wSUM_NACKUTTLTIME + wINT                                          '実績・空車時間合計Σ

                                wINT = Val(SQLdr("PAYWORKTIME"))
                                wSUM_PAYWORKTIME = wSUM_PAYWORKTIME + wINT                                           '所定労働時間

                                wINT = Val(SQLdr("PAYNIGHTTIME"))
                                wSUM_PAYNIGHTTIME = wSUM_PAYNIGHTTIME + wINT                                         '所定深夜時間

                                wINT = Val(SQLdr("PAYORVERTIME"))
                                wSUM_PAYORVERTIME = wSUM_PAYORVERTIME + wINT                                         '平日残業時間

                                wINT = Val(SQLdr("PAYWNIGHTTIME"))
                                wSUM_PAYWNIGHTTIME = wSUM_PAYWNIGHTTIME + wINT                                       '平日深夜時間

                                wINT = Val(SQLdr("PAYWSWORKTIME"))
                                wSUM_PAYWSWORKTIME = wSUM_PAYWSWORKTIME + wINT                                       '日曜出勤時間

                                wINT = Val(SQLdr("PAYSNIGHTTIME"))
                                wSUM_PAYSNIGHTTIME = wSUM_PAYSNIGHTTIME + wINT                                       '日曜深夜時間

                                wINT = Val(SQLdr("PAYHWORKTIME"))
                                wSUM_PAYHWORKTIME = wSUM_PAYHWORKTIME + wINT                                         '休日出勤時間

                                wINT = Val(SQLdr("PAYHNIGHTTIME"))
                                wSUM_PAYHNIGHTTIME = wSUM_PAYHNIGHTTIME + wINT                                       '休日深夜時間

                                wINT = Val(SQLdr("PAYBREAKTIME"))
                                wSUM_PAYBREAKTIME = wSUM_PAYBREAKTIME + wINT                                         '休憩時間


                            End While

                            '〇最終レコード出力

                            If WW_NACSHUKODATE = "" AndAlso
                               WW_ACKEIJOORG = "" AndAlso
                               WW_PAYSTAFFCODE = "" Then

                            Else
                                '〇レコード出力
                                '合計値セット
                                TA0009row("NACOFFICESORG") = TA0009row("PAYHORG")                                   '実績・作業部署
                                TA0009row("NACOFFICESORGNAME") = TA0009row("PAYHORGNAME")                           '実績・作業部署名称
                                TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                TA0009row("RECKBN") = ""                           'レコード区分
                                TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                TA0009row("DAY01") = ""                            '1日
                                TA0009row("DAY02") = ""                            '2日
                                TA0009row("DAY03") = ""                            '3日
                                TA0009row("DAY04") = ""                            '4日
                                TA0009row("DAY05") = ""                            '5日
                                TA0009row("DAY06") = ""                            '6日
                                TA0009row("DAY07") = ""                            '7日
                                TA0009row("DAY08") = ""                            '8日
                                TA0009row("DAY09") = ""                            '9日
                                TA0009row("DAY10") = ""                            '10日
                                TA0009row("DAY11") = ""                            '11日
                                TA0009row("DAY12") = ""                            '12日
                                TA0009row("DAY13") = ""                            '13日
                                TA0009row("DAY14") = ""                            '14日
                                TA0009row("DAY15") = ""                            '15日
                                TA0009row("DAY16") = ""                            '16日
                                TA0009row("DAY17") = ""                            '17日
                                TA0009row("DAY18") = ""                            '18日
                                TA0009row("DAY19") = ""                            '19日
                                TA0009row("DAY20") = ""                            '20日
                                TA0009row("DAY21") = ""                            '21日
                                TA0009row("DAY22") = ""                            '22日
                                TA0009row("DAY23") = ""                            '23日
                                TA0009row("DAY24") = ""                            '24日
                                TA0009row("DAY25") = ""                            '25日
                                TA0009row("DAY26") = ""                            '26日
                                TA0009row("DAY27") = ""                            '27日
                                TA0009row("DAY28") = ""                            '28日
                                TA0009row("DAY29") = ""                            '29日
                                TA0009row("DAY30") = ""                            '30日
                                TA0009row("DAY31") = ""                            '31日
                                TA0009row("TTL") = ""                              '累計
                                TA0009row("TTLSA") = ""                            '累計差
                                TA0009row("HOLKBN01") = ""
                                TA0009row("HOLKBN02") = ""
                                TA0009row("HOLKBN03") = ""
                                TA0009row("HOLKBN04") = ""
                                TA0009row("HOLKBN05") = ""
                                TA0009row("HOLKBN06") = ""
                                TA0009row("HOLKBN07") = ""
                                TA0009row("HOLKBN08") = ""
                                TA0009row("HOLKBN09") = ""
                                TA0009row("HOLKBN10") = ""
                                TA0009row("HOLKBN11") = ""
                                TA0009row("HOLKBN12") = ""
                                TA0009row("HOLKBN13") = ""
                                TA0009row("HOLKBN14") = ""
                                TA0009row("HOLKBN15") = ""
                                TA0009row("HOLKBN16") = ""
                                TA0009row("HOLKBN17") = ""
                                TA0009row("HOLKBN18") = ""
                                TA0009row("HOLKBN19") = ""
                                TA0009row("HOLKBN20") = ""
                                TA0009row("HOLKBN21") = ""
                                TA0009row("HOLKBN22") = ""
                                TA0009row("HOLKBN23") = ""
                                TA0009row("HOLKBN24") = ""
                                TA0009row("HOLKBN25") = ""
                                TA0009row("HOLKBN26") = ""
                                TA0009row("HOLKBN27") = ""
                                TA0009row("HOLKBN28") = ""
                                TA0009row("HOLKBN29") = ""
                                TA0009row("HOLKBN30") = ""
                                TA0009row("HOLKBN31") = ""

                                IO_TBL.Rows.Add(TA0009row)
                            End If
                        End Using
                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI SELECT")
                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:L0001_TOKEI Select"           '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
        End Using

        WW_KEYtbl.Dispose()
        WW_KEYtbl = Nothing

    End Sub

    ''' <summary>
    ''' サマリー後データ取得
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Private Sub GetTA0009WkSum3(ByRef IO_TBL As DataTable)

        '○初期クリア
        'TA0009tbl値設定
        Dim wINT As Integer
        Dim wDATE As Date
        Dim wDATETime As DateTime

        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As List(Of String) = GetORGList()

        Using SQLcon As SqlConnection = CS0050Session.getConnection
            Try

                SQLcon.Open() 'DataBase接続(Open)
                '検索SQL文
                Dim SQLStr As New StringBuilder(10000)
                SQLStr.AppendLine(" SELECT ")
                SQLStr.AppendLine("       isnull(rtrim(L04.CAMPCODE), '') as CAMPCODE ")
                SQLStr.AppendLine("     , isnull(rtrim(M01.NAMES), '') as CAMPNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.KEIJOYMD), '1950/01/01') as KEIJOYMD ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHORG), '') as ACKEIJOORG ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHORGNAME), '') as ACKEIJOORGNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.DENYMD), '1950/01/01') as DENYMD ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.DENNO), '') as DENNO ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.KANRENDENNO), '') as KANRENDENNO ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.DTLNO), '') as DTLNO ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.ACACHANTEI), '') as ACACHANTEI ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.ACACHANTEINAME), '') as ACACHANTEINAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACSHUKODATE), '1950/01/01') as NACSHUKODATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACHAISTDATE), '1950/01/01') as NACHAISTDATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACHAIENDDATE), '1950/01/01') as NACHAIENDDATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACHAIWORKTIME), '0') as NACHAIWORKTIME ")
                SQLStr.AppendLine("     , '1950/01/01' as NACGESSTDATE ")
                SQLStr.AppendLine("     , '1950/01/01' as NACGESENDDATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACGESWORKTIME), '0') as NACGESWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACCHOWORKTIME), '0') as NACCHOWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACTTLWORKTIME), '0') as NACTTLWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACOUTWORKTIME), '0') as NACOUTWORKTIME ")
                SQLStr.AppendLine("     , isnull((select sum(L03.NACJITTLETIME) from L0003_SUMMARYN L03 ")
                SQLStr.AppendLine("       where L03.CAMPCODE = L04.CAMPCODE ")
                SQLStr.AppendLine("         and L03.NACSHUKODATE = L04.NACSHUKODATE ")
                SQLStr.AppendLine("         and L03.KEYSTAFFCODE = L04.PAYSTAFFCODE ")
                SQLStr.AppendLine("         and L03.DELFLG <> '1' ")
                SQLStr.AppendLine("      ),0) as NACJITTLETIME ")
                SQLStr.AppendLine("     ,isnull((select sum(L032.NACKUTTLTIME) from L0003_SUMMARYN L032 ")
                SQLStr.AppendLine("       where L032.CAMPCODE = L04.CAMPCODE ")
                SQLStr.AppendLine("         and L032.NACSHUKODATE = L04.NACSHUKODATE ")
                SQLStr.AppendLine("         and L032.KEYSTAFFCODE = L04.PAYSTAFFCODE ")
                SQLStr.AppendLine("         and L032.DELFLG <> '1' ")
                SQLStr.AppendLine("      ),0) as NACKUTTLTIME ")
                SQLStr.AppendLine("     , '1950/01/01' as NACBREAKSTDATE ")
                SQLStr.AppendLine("     , '1950/01/01' as NACBREAKENDDATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACBREAKTIME), '0') as NACBREAKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACCHOBREAKTIME), '0') as NACCHOBREAKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACTTLBREAKTIME), '0') as NACTTLBREAKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACOFFICESORG), '') as NACOFFICESORG ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.NACOFFICESORG), '') as NACOFFICESORGNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSHUSHADATE), '1950/01/01') as PAYSHUSHADATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYTAISHADATE), '1950/01/01') as PAYTAISHADATE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSTAFFKBN), '') as PAYSTAFFKBN ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSTAFFKBN), '') as PAYSTAFFKBNNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSTAFFCODE), '') as PAYSTAFFCODE ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSTAFFCODENAME), '') as PAYSTAFFCODENAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYMORG), '') as PAYMORG ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYMORGNAME), '') as PAYMORGNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHORG), '') as PAYHORG ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHORGNAME), '') as PAYHORGNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHOLIDAYKBN), '') as PAYHOLIDAYKBN ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHOLIDAYKBNNAME), '') as PAYHOLIDAYKBNNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYKBN), '') as PAYKBN ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYKBNNAME), '') as PAYKBNNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSHUKCHOKKBN), '') as PAYSHUKCHOKKBN ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSHUKCHOKKBNNAME), '') as PAYSHUKCHOKKBNNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYJYOMUKBN), '') as PAYJYOMUKBN ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYJYOMUKBNNAME), '') as PAYJYOMUKBNNAME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYWORKTIME), '0') as PAYWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYNIGHTTIME), '0') as PAYNIGHTTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYORVERTIME), '0') as PAYORVERTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYWNIGHTTIME), '0') as PAYWNIGHTTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYWSWORKTIME), '0') as PAYWSWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYSNIGHTTIME), '0') as PAYSNIGHTTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHWORKTIME), '0') as PAYHWORKTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYHNIGHTTIME), '0') as PAYHNIGHTTIME ")
                SQLStr.AppendLine("     , isnull(rtrim(L04.PAYBREAKTIME), '0') as PAYBREAKTIME ")
                SQLStr.AppendLine("     , '0' as SUPPORTKBN ")
                SQLStr.AppendLine("    FROM L0004_SUMMARYK L04 ")
                SQLStr.AppendLine("    INNER JOIN M0001_CAMP M01  ")
                SQLStr.AppendLine("       ON M01.CAMPCODE = L04.CAMPCODE  ")
                SQLStr.AppendLine("       and M01.STYMD  <= L04.NACSHUKODATE  ")
                SQLStr.AppendLine("       and M01.ENDYMD >= L04.NACSHUKODATE  ")
                SQLStr.AppendLine("       and M01.DELFLG <> '1'  ")
                SQLStr.AppendLine("    WHERE  ")
                SQLStr.AppendLine("           L04.CAMPCODE = @P02  ")
                SQLStr.AppendLine("       and L04.NACSHUKODATE <= @P03  ")
                SQLStr.AppendLine("       and L04.NACSHUKODATE >= @P04  ")
                SQLStr.AppendLine("       and L04.PAYHORG  = @P05  ")
                SQLStr.AppendLine("       and L04.RECODEKBN  = '0'  ")
                SQLStr.AppendLine("       and L04.PAYSTAFFKBN like '03%'  ")
                SQLStr.AppendLine("       and L04.DELFLG <> '1'  ")
                SQLStr.AppendLine("    ORDER BY ")
                SQLStr.AppendLine("           L04.PAYHORG, L04.PAYSTAFFCODE, L04.NACSHUKODATE ")

                Using SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)

                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)

                    '抽出条件(サーバー部署)List毎にデータ抽出
                    For wORGcnt As Integer = 0 To W_ORGlst.Count - 1
                        '部署変換
                        Dim WW_ORG As String = ""
                        Dim WW_RTN As String = ""
                        ConvORGCode(W_ORGlst(wORGcnt), WW_ORG, WW_RTN)
                        If Not isNormal(WW_RTN) Then Exit Sub

                        '勤怠締テーブル取得
                        Dim WW_LIMITFLG As String = "0"
                        Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                           WW_ORG,
                                           work.WF_SEL_STYM.Text,
                                           WW_LIMITFLG,
                                           WW_ERR_RTN)
                        If Not isNormal(WW_ERR_RTN) Then
                            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                            Exit Sub
                        End If

                        '締まっていなければ統計テーブルから取得するためスキップ
                        If WW_LIMITFLG = "0" Then Continue For
                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = C_MAX_YMD
                        PARA04.Value = C_DEFAULT_YMD
                        PARA05.Value = W_ORGlst(wORGcnt)
                        '月末
                        Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")
                        PARA03.Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA04.Value = work.WF_SEL_STYM.Text & "/" & "01"

                        SQLcmd.CommandTimeout = 300
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            'ブレークKey
                            Dim WW_NACSHUKODATE As String = ""
                            Dim WW_ACKEIJOORG As String = ""
                            Dim WW_PAYSTAFFCODE As String = ""
                            '判定Key
                            Dim wNACSHUKODATE As String = ""
                            Dim wACKEIJOORG As String = ""
                            Dim wPAYSTAFFCODE As String = ""
                            Dim wSUM_NACHAIWORKTIME As Integer = 0                                                '実績・配送作業時間
                            Dim wSUM_NACGESWORKTIME As Integer = 0                                                '実績・下車作業時間
                            Dim wSUM_NACCHOWORKTIME As Integer = 0                                                '実績・勤怠調整時間
                            Dim wSUM_NACTTLWORKTIME As Integer = 0                                                '実績・配送合計時間Σ

                            Dim wSUM_NACOUTWORKTIME As Integer = 0                                                '実績・就業外時間
                            Dim wSUM_NACJITTLETIME As Integer = 0                                                 '実績・実車時間合計Σ
                            Dim wSUM_NACKUTTLTIME As Integer = 0                                                  '実績・空車時間合計Σ

                            Dim wSUM_NACBREAKTIME As Integer = 0                                                  '実績・休憩時間
                            Dim wSUM_NACCHOBREAKTIME As Integer = 0                                               '実績・休憩調整時間
                            Dim wSUM_NACTTLBREAKTIME As Integer = 0                                               '実績・休憩合計時間Σ
                            Dim wSUM_NACOFFICESORG As String = ""                                                 '実績・従業作業部署
                            Dim wSUM_NACOFFICESORGNAME As String = ""                                             '実績・従業作業部署名称
                            Dim wSUM_PAYWORKTIME As Integer = 0                                                   '所定労働時間
                            Dim wSUM_PAYNIGHTTIME As Integer = 0                                                  '所定深夜時間
                            Dim wSUM_PAYORVERTIME As Integer = 0                                                  '平日残業時間
                            Dim wSUM_PAYWNIGHTTIME As Integer = 0                                                 '平日深夜時間
                            Dim wSUM_PAYWSWORKTIME As Integer = 0                                                 '日曜出勤時間
                            Dim wSUM_PAYSNIGHTTIME As Integer = 0                                                 '日曜深夜時間
                            Dim wSUM_PAYHWORKTIME As Integer = 0                                                  '休日出勤時間
                            Dim wSUM_PAYHNIGHTTIME As Integer = 0                                                 '休日深夜時間
                            Dim wSUM_PAYBREAKTIME As Integer = 0                                                  '休憩時間
                            Dim wSUM_PAYSHUSHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYTAISHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYKBN As String = ""
                            Dim wSUM_PAYKBNNAME As String = ""
                            Dim wSUM_PAYSHUKCHOKKBN As String = ""
                            Dim wSUM_PAYSHUKCHOKKBNNAME As String = ""
                            Dim wSEQ As Integer = 0

                            'TA0009tbl.Clear()
                            Dim TA0009row As DataRow = Nothing
                            While SQLdr.Read

                                '〇判定Key作成
                                If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = SQLdr("NACSHUKODATE")
                                    wNACSHUKODATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACSHUKODATE = C_DEFAULT_YMD
                                End If
                                wACKEIJOORG = SQLdr("ACKEIJOORG")                                                 '計上部署
                                wPAYSTAFFCODE = SQLdr("PAYSTAFFCODE")                                             '従業員

                                '〇Keyブレーク時のレコード設定
                                If WW_NACSHUKODATE = wNACSHUKODATE AndAlso
                                   WW_ACKEIJOORG = wACKEIJOORG AndAlso
                                   WW_PAYSTAFFCODE = wPAYSTAFFCODE Then
                                Else
                                    '〇１件目
                                    If WW_NACSHUKODATE = "" AndAlso
                                       WW_ACKEIJOORG = "" AndAlso
                                       WW_PAYSTAFFCODE = "" Then

                                    Else
                                        '〇レコード出力
                                        '合計値セット
                                        TA0009row("NACOFFICESORG") = TA0009row("ACKEIJOORG")                                '実績・作業部署
                                        TA0009row("NACOFFICESORGNAME") = TA0009row("ACKEIJOORGNAME")                        '実績・作業部署名称
                                        TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                        TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                        TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                        TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                        TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                        TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                        TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                        TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                        TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                        TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                        TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                        TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                        TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                        TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                        TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                        TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                        TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                        TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                        TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                        TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                        TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                        TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                        TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                        TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                        TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                        TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                        TA0009row("RECKBN") = ""                           'レコード区分
                                        TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                        TA0009row("DAY01") = ""                            '1日
                                        TA0009row("DAY02") = ""                            '2日
                                        TA0009row("DAY03") = ""                            '3日
                                        TA0009row("DAY04") = ""                            '4日
                                        TA0009row("DAY05") = ""                            '5日
                                        TA0009row("DAY06") = ""                            '6日
                                        TA0009row("DAY07") = ""                            '7日
                                        TA0009row("DAY08") = ""                            '8日
                                        TA0009row("DAY09") = ""                            '9日
                                        TA0009row("DAY10") = ""                            '10日
                                        TA0009row("DAY11") = ""                            '11日
                                        TA0009row("DAY12") = ""                            '12日
                                        TA0009row("DAY13") = ""                            '13日
                                        TA0009row("DAY14") = ""                            '14日
                                        TA0009row("DAY15") = ""                            '15日
                                        TA0009row("DAY16") = ""                            '16日
                                        TA0009row("DAY17") = ""                            '17日
                                        TA0009row("DAY18") = ""                            '18日
                                        TA0009row("DAY19") = ""                            '19日
                                        TA0009row("DAY20") = ""                            '20日
                                        TA0009row("DAY21") = ""                            '21日
                                        TA0009row("DAY22") = ""                            '22日
                                        TA0009row("DAY23") = ""                            '23日
                                        TA0009row("DAY24") = ""                            '24日
                                        TA0009row("DAY25") = ""                            '25日
                                        TA0009row("DAY26") = ""                            '26日
                                        TA0009row("DAY27") = ""                            '27日
                                        TA0009row("DAY28") = ""                            '28日
                                        TA0009row("DAY29") = ""                            '29日
                                        TA0009row("DAY30") = ""                            '30日
                                        TA0009row("DAY31") = ""                            '31日
                                        TA0009row("TTL") = ""                              '累計
                                        TA0009row("TTLSA") = ""                            '累計差
                                        TA0009row("HOLKBN01") = ""
                                        TA0009row("HOLKBN02") = ""
                                        TA0009row("HOLKBN03") = ""
                                        TA0009row("HOLKBN04") = ""
                                        TA0009row("HOLKBN05") = ""
                                        TA0009row("HOLKBN06") = ""
                                        TA0009row("HOLKBN07") = ""
                                        TA0009row("HOLKBN08") = ""
                                        TA0009row("HOLKBN09") = ""
                                        TA0009row("HOLKBN10") = ""
                                        TA0009row("HOLKBN11") = ""
                                        TA0009row("HOLKBN12") = ""
                                        TA0009row("HOLKBN13") = ""
                                        TA0009row("HOLKBN14") = ""
                                        TA0009row("HOLKBN15") = ""
                                        TA0009row("HOLKBN16") = ""
                                        TA0009row("HOLKBN17") = ""
                                        TA0009row("HOLKBN18") = ""
                                        TA0009row("HOLKBN19") = ""
                                        TA0009row("HOLKBN20") = ""
                                        TA0009row("HOLKBN21") = ""
                                        TA0009row("HOLKBN22") = ""
                                        TA0009row("HOLKBN23") = ""
                                        TA0009row("HOLKBN24") = ""
                                        TA0009row("HOLKBN25") = ""
                                        TA0009row("HOLKBN26") = ""
                                        TA0009row("HOLKBN27") = ""
                                        TA0009row("HOLKBN28") = ""
                                        TA0009row("HOLKBN29") = ""
                                        TA0009row("HOLKBN30") = ""
                                        TA0009row("HOLKBN31") = ""

                                        IO_TBL.Rows.Add(TA0009row)

                                        wSUM_NACHAIWORKTIME = 0                                                '実績・配送作業時間
                                        wSUM_NACGESWORKTIME = 0                                                '実績・下車作業時間
                                        wSUM_NACCHOWORKTIME = 0                                                '実績・勤怠調整時間
                                        wSUM_NACTTLWORKTIME = 0                                                '実績・配送合計時間Σ

                                        wSUM_NACOUTWORKTIME = 0                                                '実績・就業外時間
                                        wSUM_NACJITTLETIME = 0                                                 '実績・実車時間合計Σ
                                        wSUM_NACKUTTLTIME = 0                                                  '実績・空車時間合計Σ

                                        wSUM_NACBREAKTIME = 0                                                  '実績・休憩時間
                                        wSUM_NACCHOBREAKTIME = 0                                               '実績・休憩調整時間
                                        wSUM_NACTTLBREAKTIME = 0                                               '実績・休憩合計時間Σ
                                        wSUM_NACOFFICESORG = ""                                                '実績・従業作業部署
                                        wSUM_NACOFFICESORGNAME = ""                                            '実績・従業作業部署名称
                                        wSUM_PAYWORKTIME = 0                                                   '所定労働時間
                                        wSUM_PAYNIGHTTIME = 0                                                  '所定深夜時間
                                        wSUM_PAYORVERTIME = 0                                                  '平日残業時間
                                        wSUM_PAYWNIGHTTIME = 0                                                 '平日深夜時間
                                        wSUM_PAYWSWORKTIME = 0                                                 '日曜出勤時間
                                        wSUM_PAYSNIGHTTIME = 0                                                 '日曜深夜時間
                                        wSUM_PAYHWORKTIME = 0                                                  '休日出勤時間
                                        wSUM_PAYHNIGHTTIME = 0                                                 '休日深夜時間
                                        wSUM_PAYBREAKTIME = 0                                                  '休憩時間
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                        wSUM_PAYKBN = ""
                                        wSUM_PAYKBNNAME = ""
                                        wSUM_PAYSHUKCHOKKBN = ""
                                        wSUM_PAYSHUKCHOKKBNNAME = ""
                                    End If

                                    '〇新レコード準備(固定項目設定)
                                    TA0009row = IO_TBL.NewRow

                                    wSEQ = 0

                                    'ブレイクキー設定
                                    WW_NACSHUKODATE = wNACSHUKODATE
                                    WW_ACKEIJOORG = wACKEIJOORG
                                    WW_PAYSTAFFCODE = wPAYSTAFFCODE

                                    wSUM_NACOFFICESORG = ""                                                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = ""                                                     '実績・従業作業部署名称
                                    wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    wSUM_PAYTAISHADATE = C_DEFAULT_YMD

                                    '固定項目
                                    TA0009row("LINECNT") = 0                                                        'DBの固定フィールド(2017/11/9)
                                    TA0009row("OPERATION") = C_LIST_OPERATION_CODE.NODATA                           'DBの固定フィールド(2017/11/9)
                                    TA0009row("TIMSTP") = 0                                                         'DBの固定フィールド(2017/11/9)
                                    TA0009row("SELECT") = "0"                                                       'DBの固定フィールド(2017/11/9)
                                    TA0009row("HIDDEN") = 0                                                         'DBの固定フィールド(2017/11/9)

                                    '画面固有項目
                                    TA0009row("CAMPCODE") = SQLdr("CAMPCODE")                                       '会社
                                    TA0009row("CAMPNAME") = SQLdr("CAMPNAME")                                       '会社名称
                                    If IsDate(SQLdr("KEIJOYMD")) AndAlso SQLdr("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                        wDATE = SQLdr("KEIJOYMD")
                                        TA0009row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("KEIJOYMD") = C_DEFAULT_YMD
                                    End If
                                    If IsDate(SQLdr("DENYMD")) AndAlso SQLdr("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                        wDATE = SQLdr("DENYMD")
                                        TA0009row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("DENYMD") = C_DEFAULT_YMD
                                    End If
                                    TA0009row("DENNO") = SQLdr("DENNO")                                             '伝票番号
                                    TA0009row("KANRENDENNO") = SQLdr("KANRENDENNO")                                 '関連伝票No＋明細No
                                    TA0009row("DTLNO") = SQLdr("DTLNO")                                             '明細番号
                                    TA0009row("ACACHANTEI") = SQLdr("ACACHANTEI")                                   '仕訳決定
                                    TA0009row("ACACHANTEINAME") = SQLdr("ACACHANTEINAME")                           '仕訳決定名称
                                    If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                        wDATE = SQLdr("NACSHUKODATE")
                                        TA0009row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0009row("NACSHUKODATE") = C_DEFAULT_YMD
                                    End If

                                    TA0009row("NACHAISTDATE") = C_DEFAULT_YMD                                        '実績・配送作業開始日時
                                    TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD                                       '実績・配送作業終了日時

                                    TA0009row("NACGESSTDATE") = C_DEFAULT_YMD                                        '実績・下車作業開始日時
                                    TA0009row("NACGESENDDATE") = C_DEFAULT_YMD                                       '実績・下車作業終了日時

                                    TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD                                      '実績・休憩開始日時
                                    TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD                                     '実績・休憩終了日時

                                    TA0009row("PAYSHUSHADATE") = C_DEFAULT_YMD
                                    TA0009row("PAYTAISHADATE") = C_DEFAULT_YMD
                                End If

                                TA0009row("ACKEIJOORG") = SQLdr("ACKEIJOORG")                                   '計上部署
                                TA0009row("ACKEIJOORGNAME") = SQLdr("ACKEIJOORGNAME")                           '計上部署名称

                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0009row("PAYSTAFFKBNNAME") = SQLdr("PAYSTAFFKBNNAME")                         '社員区分名称
                                TA0009row("PAYSTAFFCODE") = SQLdr("PAYSTAFFCODE")                               '従業員
                                TA0009row("PAYSTAFFCODENAME") = SQLdr("PAYSTAFFCODENAME")                       '従業員名称
                                TA0009row("PAYMORG") = SQLdr("PAYMORG")                                         '従業員管理部署
                                TA0009row("PAYMORGNAME") = SQLdr("PAYMORGNAME")                                 '従業員管理部署名称
                                TA0009row("PAYHORG") = SQLdr("PAYHORG")                                         '従業員配属部署
                                TA0009row("PAYHORGNAME") = SQLdr("PAYHORGNAME")                                 '従業員配属部署名称
                                TA0009row("PAYHOLIDAYKBN") = SQLdr("PAYHOLIDAYKBN")                             '休日区分
                                TA0009row("PAYHOLIDAYKBNNAME") = SQLdr("PAYHOLIDAYKBNNAME")                     '休日区分名称
                                TA0009row("PAYKBN") = SQLdr("PAYKBN")                                           '勤怠区分
                                TA0009row("PAYKBNNAME") = SQLdr("PAYKBNNAME")                                   '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = SQLdr("PAYSHUKCHOKKBN")                           '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = SQLdr("PAYSHUKCHOKKBNNAME")                   '宿日直区分名称
                                TA0009row("PAYJYOMUKBN") = SQLdr("PAYJYOMUKBN")                                 '乗務区分
                                TA0009row("PAYJYOMUKBNNAME") = SQLdr("PAYJYOMUKBNNAME")                         '乗務区分名称
                                TA0009row("SUPPORTKBN") = SQLdr("SUPPORTKBN")                                   '応援者区分

                                '実績・配送作業開始日時
                                If IsDate(SQLdr("NACHAISTDATE")) AndAlso SQLdr("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACHAISTDATE")
                                    TA0009row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACHAISTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・配送作業終了日時
                                If IsDate(SQLdr("NACHAIENDDATE")) AndAlso SQLdr("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACHAIENDDATE")
                                    TA0009row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACHAIENDDATE") = C_DEFAULT_YMD
                                End If


                                '実績・下車作業開始日時
                                If IsDate(SQLdr("NACGESSTDATE")) AndAlso SQLdr("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACGESSTDATE")
                                    TA0009row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACGESSTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・下車作業終了日時
                                If IsDate(SQLdr("NACGESENDDATE")) AndAlso SQLdr("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACGESENDDATE")
                                    TA0009row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACGESENDDATE") = C_DEFAULT_YMD
                                End If

                                '休憩開始日時
                                If IsDate(SQLdr("NACBREAKSTDATE")) AndAlso SQLdr("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACBREAKSTDATE")
                                    TA0009row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                End If

                                '休憩終了日時
                                If IsDate(SQLdr("NACBREAKENDDATE")) AndAlso SQLdr("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACBREAKENDDATE")
                                    TA0009row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0009row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                End If


                                wSUM_NACOFFICESORG = SQLdr("NACOFFICESORG")                         '実績・従業作業部署
                                wSUM_NACOFFICESORGNAME = SQLdr("NACOFFICESORGNAME")                 '実績・従業作業部署名称

                                '出社日時
                                If IsDate(SQLdr("PAYSHUSHADATE")) AndAlso SQLdr("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("PAYSHUSHADATE")
                                    wSUM_PAYSHUSHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                End If

                                '退社日時
                                If IsDate(SQLdr("PAYTAISHADATE")) AndAlso SQLdr("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("PAYTAISHADATE")
                                    wSUM_PAYTAISHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                End If

                                wSUM_PAYKBN = SQLdr("PAYKBN")                                       '勤怠区分
                                wSUM_PAYKBNNAME = SQLdr("PAYKBNNAME")                               '勤怠区分名称
                                wSUM_PAYSHUKCHOKKBN = SQLdr("PAYSHUKCHOKKBN")                       '宿日直区分
                                wSUM_PAYSHUKCHOKKBNNAME = SQLdr("PAYSHUKCHOKKBNNAME")               '宿日直区分名称

                                wINT = Val(SQLdr("NACHAIWORKTIME"))
                                wSUM_NACHAIWORKTIME = wSUM_NACHAIWORKTIME + wINT                                      '実績・配送作業時間

                                wINT = Val(SQLdr("NACGESWORKTIME"))
                                wSUM_NACGESWORKTIME = wSUM_NACGESWORKTIME + wINT                                      '実績・下車作業時間

                                wINT = Val(SQLdr("NACCHOWORKTIME"))
                                wSUM_NACCHOWORKTIME = wSUM_NACCHOWORKTIME + wINT                                      '実績・勤怠調整時間

                                wINT = Val(SQLdr("NACTTLWORKTIME"))
                                wSUM_NACTTLWORKTIME = wSUM_NACTTLWORKTIME + wINT                                      '実績・配送合計時間Σ

                                wINT = Val(SQLdr("NACOUTWORKTIME"))
                                wSUM_NACOUTWORKTIME = wSUM_NACOUTWORKTIME + wINT                                      '実績・就業外時間

                                wINT = Val(SQLdr("NACBREAKTIME"))
                                wSUM_NACBREAKTIME = wSUM_NACBREAKTIME + wINT                                          '実績・休憩時間

                                wINT = Val(SQLdr("NACCHOBREAKTIME"))
                                wSUM_NACCHOBREAKTIME = wSUM_NACCHOBREAKTIME + wINT                                    '実績・休憩調整時間

                                wINT = Val(SQLdr("NACTTLBREAKTIME"))
                                wSUM_NACTTLBREAKTIME = wSUM_NACTTLBREAKTIME + wINT                                    '実績・休憩合計時間Σ

                                wINT = Val(SQLdr("NACJITTLETIME"))
                                wSUM_NACJITTLETIME = wSUM_NACJITTLETIME + wINT                                        '実績・実車時間合計Σ

                                wINT = Val(SQLdr("NACKUTTLTIME"))
                                wSUM_NACKUTTLTIME = wSUM_NACKUTTLTIME + wINT                                          '実績・空車時間合計Σ

                                wINT = Val(SQLdr("PAYWORKTIME"))
                                wSUM_PAYWORKTIME = wSUM_PAYWORKTIME + wINT                                           '所定労働時間

                                wINT = Val(SQLdr("PAYNIGHTTIME"))
                                wSUM_PAYNIGHTTIME = wSUM_PAYNIGHTTIME + wINT                                         '所定深夜時間

                                wINT = Val(SQLdr("PAYORVERTIME"))
                                wSUM_PAYORVERTIME = wSUM_PAYORVERTIME + wINT                                         '平日残業時間

                                wINT = Val(SQLdr("PAYWNIGHTTIME"))
                                wSUM_PAYWNIGHTTIME = wSUM_PAYWNIGHTTIME + wINT                                       '平日深夜時間

                                wINT = Val(SQLdr("PAYWSWORKTIME"))
                                wSUM_PAYWSWORKTIME = wSUM_PAYWSWORKTIME + wINT                                       '日曜出勤時間

                                wINT = Val(SQLdr("PAYSNIGHTTIME"))
                                wSUM_PAYSNIGHTTIME = wSUM_PAYSNIGHTTIME + wINT                                       '日曜深夜時間

                                wINT = Val(SQLdr("PAYHWORKTIME"))
                                wSUM_PAYHWORKTIME = wSUM_PAYHWORKTIME + wINT                                         '休日出勤時間

                                wINT = Val(SQLdr("PAYHNIGHTTIME"))
                                wSUM_PAYHNIGHTTIME = wSUM_PAYHNIGHTTIME + wINT                                       '休日深夜時間

                                wINT = Val(SQLdr("PAYBREAKTIME"))
                                wSUM_PAYBREAKTIME = wSUM_PAYBREAKTIME + wINT                                         '休憩時間
                            End While

                            '〇最終レコード出力

                            If WW_NACSHUKODATE = "" AndAlso
                               WW_ACKEIJOORG = "" AndAlso
                               WW_PAYSTAFFCODE = "" Then

                            Else
                                '〇レコード出力
                                '合計値セット
                                TA0009row("NACOFFICESORG") = TA0009row("ACKEIJOORG")                                '実績・作業部署
                                TA0009row("NACOFFICESORGNAME") = TA0009row("ACKEIJOORGNAME")                        '実績・作業部署名称
                                TA0009row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                TA0009row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                TA0009row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                TA0009row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                TA0009row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                TA0009row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時

                                TA0009row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                TA0009row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                TA0009row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                TA0009row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                TA0009row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                TA0009row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                TA0009row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                TA0009row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                TA0009row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                TA0009row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                TA0009row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                TA0009row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                TA0009row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                TA0009row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                TA0009row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                TA0009row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                TA0009row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                TA0009row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                TA0009row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                TA0009row("TAISHYM") = work.WF_SEL_STYM.Text
                                TA0009row("RECKBN") = ""                           'レコード区分
                                TA0009row("RECKBNNAME") = ""                       'レコード区分名称
                                TA0009row("DAY01") = ""                            '1日
                                TA0009row("DAY02") = ""                            '2日
                                TA0009row("DAY03") = ""                            '3日
                                TA0009row("DAY04") = ""                            '4日
                                TA0009row("DAY05") = ""                            '5日
                                TA0009row("DAY06") = ""                            '6日
                                TA0009row("DAY07") = ""                            '7日
                                TA0009row("DAY08") = ""                            '8日
                                TA0009row("DAY09") = ""                            '9日
                                TA0009row("DAY10") = ""                            '10日
                                TA0009row("DAY11") = ""                            '11日
                                TA0009row("DAY12") = ""                            '12日
                                TA0009row("DAY13") = ""                            '13日
                                TA0009row("DAY14") = ""                            '14日
                                TA0009row("DAY15") = ""                            '15日
                                TA0009row("DAY16") = ""                            '16日
                                TA0009row("DAY17") = ""                            '17日
                                TA0009row("DAY18") = ""                            '18日
                                TA0009row("DAY19") = ""                            '19日
                                TA0009row("DAY20") = ""                            '20日
                                TA0009row("DAY21") = ""                            '21日
                                TA0009row("DAY22") = ""                            '22日
                                TA0009row("DAY23") = ""                            '23日
                                TA0009row("DAY24") = ""                            '24日
                                TA0009row("DAY25") = ""                            '25日
                                TA0009row("DAY26") = ""                            '26日
                                TA0009row("DAY27") = ""                            '27日
                                TA0009row("DAY28") = ""                            '28日
                                TA0009row("DAY29") = ""                            '29日
                                TA0009row("DAY30") = ""                            '30日
                                TA0009row("DAY31") = ""                            '31日
                                TA0009row("TTL") = ""                              '累計
                                TA0009row("TTLSA") = ""                            '累計差
                                TA0009row("HOLKBN01") = ""
                                TA0009row("HOLKBN02") = ""
                                TA0009row("HOLKBN03") = ""
                                TA0009row("HOLKBN04") = ""
                                TA0009row("HOLKBN05") = ""
                                TA0009row("HOLKBN06") = ""
                                TA0009row("HOLKBN07") = ""
                                TA0009row("HOLKBN08") = ""
                                TA0009row("HOLKBN09") = ""
                                TA0009row("HOLKBN10") = ""
                                TA0009row("HOLKBN11") = ""
                                TA0009row("HOLKBN12") = ""
                                TA0009row("HOLKBN13") = ""
                                TA0009row("HOLKBN14") = ""
                                TA0009row("HOLKBN15") = ""
                                TA0009row("HOLKBN16") = ""
                                TA0009row("HOLKBN17") = ""
                                TA0009row("HOLKBN18") = ""
                                TA0009row("HOLKBN19") = ""
                                TA0009row("HOLKBN20") = ""
                                TA0009row("HOLKBN21") = ""
                                TA0009row("HOLKBN22") = ""
                                TA0009row("HOLKBN23") = ""
                                TA0009row("HOLKBN24") = ""
                                TA0009row("HOLKBN25") = ""
                                TA0009row("HOLKBN26") = ""
                                TA0009row("HOLKBN27") = ""
                                TA0009row("HOLKBN28") = ""
                                TA0009row("HOLKBN29") = ""
                                TA0009row("HOLKBN30") = ""
                                TA0009row("HOLKBN31") = ""

                                IO_TBL.Rows.Add(TA0009row)
                            End If

                        End Using

                    Next
                End Using
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0004_SUMMARYK SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:L0004_SUMMARYK Select"         '
                CS0011LOGWRITE.NIWEA = "A"                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = "00003"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 表示元データ(条件によるサマリー）
    ''' </summary>
    ''' <param name="O_TBL"></param>
    ''' <remarks></remarks>
    Private Sub SummaryTA0009WK(ByRef O_TBL As DataTable)

        'レコード集計（31日分）
        Dim W_FIRST As String = "OFF"
        Dim W_OVERCNT() As Integer = {0, 0, 0}
        Dim W_TTL() As Integer = {0, 0, 0}
        Dim W_TTLSA() As Integer = {0, 0, 0}
        Dim W_ACTTIME() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim W_ORVERTIME() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Dim W_HANDLTIME() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        Dim W_NIPPO() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        Dim W_WEEK() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        For i As Integer = 0 To MB0005tbl.Rows.Count - 1
            W_WEEK(i) = MB0005tbl.Rows(i)("WORKINGKBNNAME")
        Next

        Dim W_STAFFCODE As String = ""
        Dim W_TA0009row As DataRow = Nothing
        Dim oTA0009row As DataRow = Nothing
        For i As Integer = 0 To TA0009tbl.Rows.Count - 1
            Dim TA0009row As DataRow = TA0009tbl.Rows(i)

            If TA0009row("PAYSTAFFCODE") <> W_STAFFCODE AndAlso W_STAFFCODE <> "" Then
                oTA0009row = O_TBL.NewRow
                oTA0009row.ItemArray = W_TA0009row.ItemArray
                oTA0009row("RECKBN") = "01"
                oTA0009row("RECKBNNAME") = ""
                CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
                oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
                oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
                oTA0009row("DAY01") = MinutesToHHMM(W_ORVERTIME(0))
                oTA0009row("DAY02") = MinutesToHHMM(W_ORVERTIME(1))
                oTA0009row("DAY03") = MinutesToHHMM(W_ORVERTIME(2))
                oTA0009row("DAY04") = MinutesToHHMM(W_ORVERTIME(3))
                oTA0009row("DAY05") = MinutesToHHMM(W_ORVERTIME(4))
                oTA0009row("DAY06") = MinutesToHHMM(W_ORVERTIME(5))
                oTA0009row("DAY07") = MinutesToHHMM(W_ORVERTIME(6))
                oTA0009row("DAY08") = MinutesToHHMM(W_ORVERTIME(7))
                oTA0009row("DAY09") = MinutesToHHMM(W_ORVERTIME(8))
                oTA0009row("DAY10") = MinutesToHHMM(W_ORVERTIME(9))
                oTA0009row("DAY11") = MinutesToHHMM(W_ORVERTIME(10))
                oTA0009row("DAY12") = MinutesToHHMM(W_ORVERTIME(11))
                oTA0009row("DAY13") = MinutesToHHMM(W_ORVERTIME(12))
                oTA0009row("DAY14") = MinutesToHHMM(W_ORVERTIME(13))
                oTA0009row("DAY15") = MinutesToHHMM(W_ORVERTIME(14))
                oTA0009row("DAY16") = MinutesToHHMM(W_ORVERTIME(15))
                oTA0009row("DAY17") = MinutesToHHMM(W_ORVERTIME(16))
                oTA0009row("DAY18") = MinutesToHHMM(W_ORVERTIME(17))
                oTA0009row("DAY19") = MinutesToHHMM(W_ORVERTIME(18))
                oTA0009row("DAY20") = MinutesToHHMM(W_ORVERTIME(19))
                oTA0009row("DAY21") = MinutesToHHMM(W_ORVERTIME(20))
                oTA0009row("DAY22") = MinutesToHHMM(W_ORVERTIME(21))
                oTA0009row("DAY23") = MinutesToHHMM(W_ORVERTIME(22))
                oTA0009row("DAY24") = MinutesToHHMM(W_ORVERTIME(23))
                oTA0009row("DAY25") = MinutesToHHMM(W_ORVERTIME(24))
                oTA0009row("DAY26") = MinutesToHHMM(W_ORVERTIME(25))
                oTA0009row("DAY27") = MinutesToHHMM(W_ORVERTIME(26))
                oTA0009row("DAY28") = MinutesToHHMM(W_ORVERTIME(27))
                oTA0009row("DAY29") = MinutesToHHMM(W_ORVERTIME(28))
                oTA0009row("DAY30") = MinutesToHHMM(W_ORVERTIME(29))
                oTA0009row("DAY31") = MinutesToHHMM(W_ORVERTIME(30))
                For j As Integer = 0 To W_ORVERTIME.Count - 1
                    W_TTL(0) += W_ORVERTIME(j)
                Next
                oTA0009row("OVERCNT") = ""
                oTA0009row("TTL") = MinutesToHHMM(W_TTL(0))
                W_TTLSA(0) = CInt(WF_MAXORVERTIME.Text) * 60 - W_TTL(0)
                oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(0))
                oTA0009row("HOLKBN01") = W_WEEK(0)
                oTA0009row("HOLKBN02") = W_WEEK(1)
                oTA0009row("HOLKBN03") = W_WEEK(2)
                oTA0009row("HOLKBN04") = W_WEEK(3)
                oTA0009row("HOLKBN05") = W_WEEK(4)
                oTA0009row("HOLKBN06") = W_WEEK(5)
                oTA0009row("HOLKBN07") = W_WEEK(6)
                oTA0009row("HOLKBN08") = W_WEEK(7)
                oTA0009row("HOLKBN09") = W_WEEK(8)
                oTA0009row("HOLKBN10") = W_WEEK(9)
                oTA0009row("HOLKBN11") = W_WEEK(10)
                oTA0009row("HOLKBN12") = W_WEEK(11)
                oTA0009row("HOLKBN13") = W_WEEK(12)
                oTA0009row("HOLKBN14") = W_WEEK(13)
                oTA0009row("HOLKBN15") = W_WEEK(14)
                oTA0009row("HOLKBN16") = W_WEEK(15)
                oTA0009row("HOLKBN17") = W_WEEK(16)
                oTA0009row("HOLKBN18") = W_WEEK(17)
                oTA0009row("HOLKBN19") = W_WEEK(18)
                oTA0009row("HOLKBN20") = W_WEEK(19)
                oTA0009row("HOLKBN21") = W_WEEK(20)
                oTA0009row("HOLKBN22") = W_WEEK(21)
                oTA0009row("HOLKBN23") = W_WEEK(22)
                oTA0009row("HOLKBN24") = W_WEEK(23)
                oTA0009row("HOLKBN25") = W_WEEK(24)
                oTA0009row("HOLKBN26") = W_WEEK(25)
                oTA0009row("HOLKBN27") = W_WEEK(26)
                oTA0009row("HOLKBN28") = W_WEEK(27)
                oTA0009row("HOLKBN29") = W_WEEK(28)
                oTA0009row("HOLKBN30") = W_WEEK(29)
                oTA0009row("HOLKBN31") = W_WEEK(30)

                O_TBL.Rows.Add(oTA0009row)

                oTA0009row = O_TBL.NewRow
                oTA0009row.ItemArray = W_TA0009row.ItemArray
                oTA0009row("RECKBN") = "02"
                oTA0009row("RECKBNNAME") = ""
                CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
                oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
                oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
                oTA0009row("DAY01") = MinutesToHHMM(W_ACTTIME(0))
                oTA0009row("DAY02") = MinutesToHHMM(W_ACTTIME(1))
                oTA0009row("DAY03") = MinutesToHHMM(W_ACTTIME(2))
                oTA0009row("DAY04") = MinutesToHHMM(W_ACTTIME(3))
                oTA0009row("DAY05") = MinutesToHHMM(W_ACTTIME(4))
                oTA0009row("DAY06") = MinutesToHHMM(W_ACTTIME(5))
                oTA0009row("DAY07") = MinutesToHHMM(W_ACTTIME(6))
                oTA0009row("DAY08") = MinutesToHHMM(W_ACTTIME(7))
                oTA0009row("DAY09") = MinutesToHHMM(W_ACTTIME(8))
                oTA0009row("DAY10") = MinutesToHHMM(W_ACTTIME(9))
                oTA0009row("DAY11") = MinutesToHHMM(W_ACTTIME(10))
                oTA0009row("DAY12") = MinutesToHHMM(W_ACTTIME(11))
                oTA0009row("DAY13") = MinutesToHHMM(W_ACTTIME(12))
                oTA0009row("DAY14") = MinutesToHHMM(W_ACTTIME(13))
                oTA0009row("DAY15") = MinutesToHHMM(W_ACTTIME(14))
                oTA0009row("DAY16") = MinutesToHHMM(W_ACTTIME(15))
                oTA0009row("DAY17") = MinutesToHHMM(W_ACTTIME(16))
                oTA0009row("DAY18") = MinutesToHHMM(W_ACTTIME(17))
                oTA0009row("DAY19") = MinutesToHHMM(W_ACTTIME(18))
                oTA0009row("DAY20") = MinutesToHHMM(W_ACTTIME(19))
                oTA0009row("DAY21") = MinutesToHHMM(W_ACTTIME(20))
                oTA0009row("DAY22") = MinutesToHHMM(W_ACTTIME(21))
                oTA0009row("DAY23") = MinutesToHHMM(W_ACTTIME(22))
                oTA0009row("DAY24") = MinutesToHHMM(W_ACTTIME(23))
                oTA0009row("DAY25") = MinutesToHHMM(W_ACTTIME(24))
                oTA0009row("DAY26") = MinutesToHHMM(W_ACTTIME(25))
                oTA0009row("DAY27") = MinutesToHHMM(W_ACTTIME(26))
                oTA0009row("DAY28") = MinutesToHHMM(W_ACTTIME(27))
                oTA0009row("DAY29") = MinutesToHHMM(W_ACTTIME(28))
                oTA0009row("DAY30") = MinutesToHHMM(W_ACTTIME(29))
                oTA0009row("DAY31") = MinutesToHHMM(W_ACTTIME(30))
                For j As Integer = 0 To W_ACTTIME.Count - 1
                    W_TTL(1) += W_ACTTIME(j)
                    If W_ACTTIME(j) > 960 Then
                        W_OVERCNT(1) += 1
                    End If
                Next
                oTA0009row("OVERCNT") = ZeroToSpace(W_OVERCNT(1))
                oTA0009row("TTL") = MinutesToHHMM(W_TTL(1))
                W_TTLSA(1) = CInt(WF_MAXWORKTIME.Text) * 60 - W_TTL(1)
                oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(1))
                oTA0009row("HOLKBN01") = W_WEEK(0)
                oTA0009row("HOLKBN02") = W_WEEK(1)
                oTA0009row("HOLKBN03") = W_WEEK(2)
                oTA0009row("HOLKBN04") = W_WEEK(3)
                oTA0009row("HOLKBN05") = W_WEEK(4)
                oTA0009row("HOLKBN06") = W_WEEK(5)
                oTA0009row("HOLKBN07") = W_WEEK(6)
                oTA0009row("HOLKBN08") = W_WEEK(7)
                oTA0009row("HOLKBN09") = W_WEEK(8)
                oTA0009row("HOLKBN10") = W_WEEK(9)
                oTA0009row("HOLKBN11") = W_WEEK(10)
                oTA0009row("HOLKBN12") = W_WEEK(11)
                oTA0009row("HOLKBN13") = W_WEEK(12)
                oTA0009row("HOLKBN14") = W_WEEK(13)
                oTA0009row("HOLKBN15") = W_WEEK(14)
                oTA0009row("HOLKBN16") = W_WEEK(15)
                oTA0009row("HOLKBN17") = W_WEEK(16)
                oTA0009row("HOLKBN18") = W_WEEK(17)
                oTA0009row("HOLKBN19") = W_WEEK(18)
                oTA0009row("HOLKBN20") = W_WEEK(19)
                oTA0009row("HOLKBN21") = W_WEEK(20)
                oTA0009row("HOLKBN22") = W_WEEK(21)
                oTA0009row("HOLKBN23") = W_WEEK(22)
                oTA0009row("HOLKBN24") = W_WEEK(23)
                oTA0009row("HOLKBN25") = W_WEEK(24)
                oTA0009row("HOLKBN26") = W_WEEK(25)
                oTA0009row("HOLKBN27") = W_WEEK(26)
                oTA0009row("HOLKBN28") = W_WEEK(27)
                oTA0009row("HOLKBN29") = W_WEEK(28)
                oTA0009row("HOLKBN30") = W_WEEK(29)
                oTA0009row("HOLKBN31") = W_WEEK(30)

                O_TBL.Rows.Add(oTA0009row)

                oTA0009row = O_TBL.NewRow
                oTA0009row.ItemArray = W_TA0009row.ItemArray
                oTA0009row("RECKBN") = "03"
                oTA0009row("RECKBNNAME") = ""
                CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
                oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
                oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
                oTA0009row("DAY01") = MinutesToHHMM(W_HANDLTIME(0))
                oTA0009row("DAY02") = MinutesToHHMM(W_HANDLTIME(1))
                oTA0009row("DAY03") = MinutesToHHMM(W_HANDLTIME(2))
                oTA0009row("DAY04") = MinutesToHHMM(W_HANDLTIME(3))
                oTA0009row("DAY05") = MinutesToHHMM(W_HANDLTIME(4))
                oTA0009row("DAY06") = MinutesToHHMM(W_HANDLTIME(5))
                oTA0009row("DAY07") = MinutesToHHMM(W_HANDLTIME(6))
                oTA0009row("DAY08") = MinutesToHHMM(W_HANDLTIME(7))
                oTA0009row("DAY09") = MinutesToHHMM(W_HANDLTIME(8))
                oTA0009row("DAY10") = MinutesToHHMM(W_HANDLTIME(9))
                oTA0009row("DAY11") = MinutesToHHMM(W_HANDLTIME(10))
                oTA0009row("DAY12") = MinutesToHHMM(W_HANDLTIME(11))
                oTA0009row("DAY13") = MinutesToHHMM(W_HANDLTIME(12))
                oTA0009row("DAY14") = MinutesToHHMM(W_HANDLTIME(13))
                oTA0009row("DAY15") = MinutesToHHMM(W_HANDLTIME(14))
                oTA0009row("DAY16") = MinutesToHHMM(W_HANDLTIME(15))
                oTA0009row("DAY17") = MinutesToHHMM(W_HANDLTIME(16))
                oTA0009row("DAY18") = MinutesToHHMM(W_HANDLTIME(17))
                oTA0009row("DAY19") = MinutesToHHMM(W_HANDLTIME(18))
                oTA0009row("DAY20") = MinutesToHHMM(W_HANDLTIME(19))
                oTA0009row("DAY21") = MinutesToHHMM(W_HANDLTIME(20))
                oTA0009row("DAY22") = MinutesToHHMM(W_HANDLTIME(21))
                oTA0009row("DAY23") = MinutesToHHMM(W_HANDLTIME(22))
                oTA0009row("DAY24") = MinutesToHHMM(W_HANDLTIME(23))
                oTA0009row("DAY25") = MinutesToHHMM(W_HANDLTIME(24))
                oTA0009row("DAY26") = MinutesToHHMM(W_HANDLTIME(25))
                oTA0009row("DAY27") = MinutesToHHMM(W_HANDLTIME(26))
                oTA0009row("DAY28") = MinutesToHHMM(W_HANDLTIME(27))
                oTA0009row("DAY29") = MinutesToHHMM(W_HANDLTIME(28))
                oTA0009row("DAY30") = MinutesToHHMM(W_HANDLTIME(29))
                oTA0009row("DAY31") = MinutesToHHMM(W_HANDLTIME(30))
                For j As Integer = 0 To W_HANDLTIME.Count - 1
                    W_TTL(2) += W_HANDLTIME(j)
                    If W_HANDLTIME(j) > 540 Then
                        W_OVERCNT(2) += 1
                    End If
                Next
                oTA0009row("OVERCNT") = ZeroToSpace(W_OVERCNT(2))
                oTA0009row("TTL") = MinutesToHHMM(W_TTL(2))
                oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(2))
                oTA0009row("HOLKBN01") = W_WEEK(0)
                oTA0009row("HOLKBN02") = W_WEEK(1)
                oTA0009row("HOLKBN03") = W_WEEK(2)
                oTA0009row("HOLKBN04") = W_WEEK(3)
                oTA0009row("HOLKBN05") = W_WEEK(4)
                oTA0009row("HOLKBN06") = W_WEEK(5)
                oTA0009row("HOLKBN07") = W_WEEK(6)
                oTA0009row("HOLKBN08") = W_WEEK(7)
                oTA0009row("HOLKBN09") = W_WEEK(8)
                oTA0009row("HOLKBN10") = W_WEEK(9)
                oTA0009row("HOLKBN11") = W_WEEK(10)
                oTA0009row("HOLKBN12") = W_WEEK(11)
                oTA0009row("HOLKBN13") = W_WEEK(12)
                oTA0009row("HOLKBN14") = W_WEEK(13)
                oTA0009row("HOLKBN15") = W_WEEK(14)
                oTA0009row("HOLKBN16") = W_WEEK(15)
                oTA0009row("HOLKBN17") = W_WEEK(16)
                oTA0009row("HOLKBN18") = W_WEEK(17)
                oTA0009row("HOLKBN19") = W_WEEK(18)
                oTA0009row("HOLKBN20") = W_WEEK(19)
                oTA0009row("HOLKBN21") = W_WEEK(20)
                oTA0009row("HOLKBN22") = W_WEEK(21)
                oTA0009row("HOLKBN23") = W_WEEK(22)
                oTA0009row("HOLKBN24") = W_WEEK(23)
                oTA0009row("HOLKBN25") = W_WEEK(24)
                oTA0009row("HOLKBN26") = W_WEEK(25)
                oTA0009row("HOLKBN27") = W_WEEK(26)
                oTA0009row("HOLKBN28") = W_WEEK(27)
                oTA0009row("HOLKBN29") = W_WEEK(28)
                oTA0009row("HOLKBN30") = W_WEEK(29)
                oTA0009row("HOLKBN31") = W_WEEK(30)

                O_TBL.Rows.Add(oTA0009row)

                oTA0009row = O_TBL.NewRow
                oTA0009row.ItemArray = W_TA0009row.ItemArray
                oTA0009row("RECKBN") = "04"
                oTA0009row("RECKBNNAME") = ""
                CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
                oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
                oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
                oTA0009row("DAY01") = W_NIPPO(0)
                oTA0009row("DAY02") = W_NIPPO(1)
                oTA0009row("DAY03") = W_NIPPO(2)
                oTA0009row("DAY04") = W_NIPPO(3)
                oTA0009row("DAY05") = W_NIPPO(4)
                oTA0009row("DAY06") = W_NIPPO(5)
                oTA0009row("DAY07") = W_NIPPO(6)
                oTA0009row("DAY08") = W_NIPPO(7)
                oTA0009row("DAY09") = W_NIPPO(8)
                oTA0009row("DAY10") = W_NIPPO(9)
                oTA0009row("DAY11") = W_NIPPO(10)
                oTA0009row("DAY12") = W_NIPPO(11)
                oTA0009row("DAY13") = W_NIPPO(12)
                oTA0009row("DAY14") = W_NIPPO(13)
                oTA0009row("DAY15") = W_NIPPO(14)
                oTA0009row("DAY16") = W_NIPPO(15)
                oTA0009row("DAY17") = W_NIPPO(16)
                oTA0009row("DAY18") = W_NIPPO(17)
                oTA0009row("DAY19") = W_NIPPO(18)
                oTA0009row("DAY20") = W_NIPPO(19)
                oTA0009row("DAY21") = W_NIPPO(20)
                oTA0009row("DAY22") = W_NIPPO(21)
                oTA0009row("DAY23") = W_NIPPO(22)
                oTA0009row("DAY24") = W_NIPPO(23)
                oTA0009row("DAY25") = W_NIPPO(24)
                oTA0009row("DAY26") = W_NIPPO(25)
                oTA0009row("DAY27") = W_NIPPO(26)
                oTA0009row("DAY28") = W_NIPPO(27)
                oTA0009row("DAY29") = W_NIPPO(28)
                oTA0009row("DAY30") = W_NIPPO(29)
                oTA0009row("DAY31") = W_NIPPO(30)
                oTA0009row("OVERCNT") = ""
                oTA0009row("TTL") = ""
                oTA0009row("TTLSA") = ""
                oTA0009row("HOLKBN01") = W_WEEK(0)
                oTA0009row("HOLKBN02") = W_WEEK(1)
                oTA0009row("HOLKBN03") = W_WEEK(2)
                oTA0009row("HOLKBN04") = W_WEEK(3)
                oTA0009row("HOLKBN05") = W_WEEK(4)
                oTA0009row("HOLKBN06") = W_WEEK(5)
                oTA0009row("HOLKBN07") = W_WEEK(6)
                oTA0009row("HOLKBN08") = W_WEEK(7)
                oTA0009row("HOLKBN09") = W_WEEK(8)
                oTA0009row("HOLKBN10") = W_WEEK(9)
                oTA0009row("HOLKBN11") = W_WEEK(10)
                oTA0009row("HOLKBN12") = W_WEEK(11)
                oTA0009row("HOLKBN13") = W_WEEK(12)
                oTA0009row("HOLKBN14") = W_WEEK(13)
                oTA0009row("HOLKBN15") = W_WEEK(14)
                oTA0009row("HOLKBN16") = W_WEEK(15)
                oTA0009row("HOLKBN17") = W_WEEK(16)
                oTA0009row("HOLKBN18") = W_WEEK(17)
                oTA0009row("HOLKBN19") = W_WEEK(18)
                oTA0009row("HOLKBN20") = W_WEEK(19)
                oTA0009row("HOLKBN21") = W_WEEK(20)
                oTA0009row("HOLKBN22") = W_WEEK(21)
                oTA0009row("HOLKBN23") = W_WEEK(22)
                oTA0009row("HOLKBN24") = W_WEEK(23)
                oTA0009row("HOLKBN25") = W_WEEK(24)
                oTA0009row("HOLKBN26") = W_WEEK(25)
                oTA0009row("HOLKBN27") = W_WEEK(26)
                oTA0009row("HOLKBN28") = W_WEEK(27)
                oTA0009row("HOLKBN29") = W_WEEK(28)
                oTA0009row("HOLKBN30") = W_WEEK(29)
                oTA0009row("HOLKBN31") = W_WEEK(30)

                O_TBL.Rows.Add(oTA0009row)

                W_OVERCNT = {0, 0, 0}
                W_TTL = {0, 0, 0}
                W_TTLSA = {0, 0, 0}
                W_ACTTIME = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                W_ORVERTIME = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                W_HANDLTIME = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                W_NIPPO = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            End If


            Dim WW_DAYS As Integer = CInt(CDate(TA0009row("NACSHUKODATE")).ToString("dd")) - 1

            If CDate(TA0009row("PAYSHUSHADATE")).ToString("HHmmss") = "000000" AndAlso CDate(TA0009row("PAYTAISHADATE")).ToString("HHmmss") = "000000" Then
                '------ 日報から ---------------
                '残業時間
                '所定労働時間より大きければ稼働時間（退社－出社）－休憩－所定労働時間
                Dim WW_WORKTIME As Integer = DateDiff("n", TA0009row("NACHAISTDATE"), TA0009row("NACHAIENDDATE")) - CInt(TA0009row("NACTTLBREAKTIME"))
                If WW_WORKTIME > 455 Then
                    W_ORVERTIME(WW_DAYS) = WW_WORKTIME - 455
                Else
                    W_ORVERTIME(WW_DAYS) = 0
                End If
                W_TTLSA(0) = 0
                '稼働時間
                W_ACTTIME(WW_DAYS) = DateDiff("n", TA0009row("NACHAISTDATE"), TA0009row("NACHAIENDDATE"))
                W_TTLSA(1) = 0
                'ハンドル時間
                W_HANDLTIME(WW_DAYS) = CInt(TA0009row("NACJITTLETIME")) + CInt(TA0009row("NACKUTTLTIME"))
                W_TTLSA(2) = 0
                '日報マーク
                If W_ACTTIME(WW_DAYS) > 0 Then
                    W_NIPPO(WW_DAYS) = "日報仮"
                Else
                    W_NIPPO(WW_DAYS) = ""
                End If
            Else
                '------ 勤怠から ---------------
                '残業時間
                W_ORVERTIME(WW_DAYS) = CInt(TA0009row("PAYORVERTIME")) + CInt(TA0009row("PAYWNIGHTTIME")) + CInt(TA0009row("PAYWSWORKTIME")) + CInt(TA0009row("PAYSNIGHTTIME")) + CInt(TA0009row("PAYHWORKTIME")) + CInt(TA0009row("PAYHNIGHTTIME"))
                W_TTLSA(0) = 0
                '稼働時間
                W_ACTTIME(WW_DAYS) = DateDiff("n", TA0009row("PAYSHUSHADATE"), TA0009row("PAYTAISHADATE"))
                W_TTLSA(1) = 0
                'ハンドル時間
                W_HANDLTIME(WW_DAYS) = CInt(TA0009row("NACJITTLETIME")) + CInt(TA0009row("NACKUTTLTIME"))
                W_TTLSA(2) = 0
                '日報マーク
                W_NIPPO(WW_DAYS) = ""
            End If

            W_STAFFCODE = TA0009row("PAYSTAFFCODE")
            W_TA0009row = TA0009tbl.Rows(i)
        Next

        If TA0009tbl.Rows.Count > 0 Then

            oTA0009row = O_TBL.NewRow
            oTA0009row.ItemArray = W_TA0009row.ItemArray
            oTA0009row("RECKBN") = "01"
            oTA0009row("RECKBNNAME") = ""
            CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
            oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
            oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
            oTA0009row("DAY01") = MinutesToHHMM(W_ORVERTIME(0))
            oTA0009row("DAY02") = MinutesToHHMM(W_ORVERTIME(1))
            oTA0009row("DAY03") = MinutesToHHMM(W_ORVERTIME(2))
            oTA0009row("DAY04") = MinutesToHHMM(W_ORVERTIME(3))
            oTA0009row("DAY05") = MinutesToHHMM(W_ORVERTIME(4))
            oTA0009row("DAY06") = MinutesToHHMM(W_ORVERTIME(5))
            oTA0009row("DAY07") = MinutesToHHMM(W_ORVERTIME(6))
            oTA0009row("DAY08") = MinutesToHHMM(W_ORVERTIME(7))
            oTA0009row("DAY09") = MinutesToHHMM(W_ORVERTIME(8))
            oTA0009row("DAY10") = MinutesToHHMM(W_ORVERTIME(9))
            oTA0009row("DAY11") = MinutesToHHMM(W_ORVERTIME(10))
            oTA0009row("DAY12") = MinutesToHHMM(W_ORVERTIME(11))
            oTA0009row("DAY13") = MinutesToHHMM(W_ORVERTIME(12))
            oTA0009row("DAY14") = MinutesToHHMM(W_ORVERTIME(13))
            oTA0009row("DAY15") = MinutesToHHMM(W_ORVERTIME(14))
            oTA0009row("DAY16") = MinutesToHHMM(W_ORVERTIME(15))
            oTA0009row("DAY17") = MinutesToHHMM(W_ORVERTIME(16))
            oTA0009row("DAY18") = MinutesToHHMM(W_ORVERTIME(17))
            oTA0009row("DAY19") = MinutesToHHMM(W_ORVERTIME(18))
            oTA0009row("DAY20") = MinutesToHHMM(W_ORVERTIME(19))
            oTA0009row("DAY21") = MinutesToHHMM(W_ORVERTIME(20))
            oTA0009row("DAY22") = MinutesToHHMM(W_ORVERTIME(21))
            oTA0009row("DAY23") = MinutesToHHMM(W_ORVERTIME(22))
            oTA0009row("DAY24") = MinutesToHHMM(W_ORVERTIME(23))
            oTA0009row("DAY25") = MinutesToHHMM(W_ORVERTIME(24))
            oTA0009row("DAY26") = MinutesToHHMM(W_ORVERTIME(25))
            oTA0009row("DAY27") = MinutesToHHMM(W_ORVERTIME(26))
            oTA0009row("DAY28") = MinutesToHHMM(W_ORVERTIME(27))
            oTA0009row("DAY29") = MinutesToHHMM(W_ORVERTIME(28))
            oTA0009row("DAY30") = MinutesToHHMM(W_ORVERTIME(29))
            oTA0009row("DAY31") = MinutesToHHMM(W_ORVERTIME(30))
            For j As Integer = 0 To W_ORVERTIME.Count - 1
                W_TTL(0) += W_ORVERTIME(j)
            Next
            oTA0009row("OVERCNT") = ""
            oTA0009row("TTL") = MinutesToHHMM(W_TTL(0))
            W_TTLSA(0) = CInt(WF_MAXORVERTIME.Text) * 60 - W_TTL(0)
            oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(0))
            oTA0009row("HOLKBN01") = W_WEEK(0)
            oTA0009row("HOLKBN02") = W_WEEK(1)
            oTA0009row("HOLKBN03") = W_WEEK(2)
            oTA0009row("HOLKBN04") = W_WEEK(3)
            oTA0009row("HOLKBN05") = W_WEEK(4)
            oTA0009row("HOLKBN06") = W_WEEK(5)
            oTA0009row("HOLKBN07") = W_WEEK(6)
            oTA0009row("HOLKBN08") = W_WEEK(7)
            oTA0009row("HOLKBN09") = W_WEEK(8)
            oTA0009row("HOLKBN10") = W_WEEK(9)
            oTA0009row("HOLKBN11") = W_WEEK(10)
            oTA0009row("HOLKBN12") = W_WEEK(11)
            oTA0009row("HOLKBN13") = W_WEEK(12)
            oTA0009row("HOLKBN14") = W_WEEK(13)
            oTA0009row("HOLKBN15") = W_WEEK(14)
            oTA0009row("HOLKBN16") = W_WEEK(15)
            oTA0009row("HOLKBN17") = W_WEEK(16)
            oTA0009row("HOLKBN18") = W_WEEK(17)
            oTA0009row("HOLKBN19") = W_WEEK(18)
            oTA0009row("HOLKBN20") = W_WEEK(19)
            oTA0009row("HOLKBN21") = W_WEEK(20)
            oTA0009row("HOLKBN22") = W_WEEK(21)
            oTA0009row("HOLKBN23") = W_WEEK(22)
            oTA0009row("HOLKBN24") = W_WEEK(23)
            oTA0009row("HOLKBN25") = W_WEEK(24)
            oTA0009row("HOLKBN26") = W_WEEK(25)
            oTA0009row("HOLKBN27") = W_WEEK(26)
            oTA0009row("HOLKBN28") = W_WEEK(27)
            oTA0009row("HOLKBN29") = W_WEEK(28)
            oTA0009row("HOLKBN30") = W_WEEK(29)
            oTA0009row("HOLKBN31") = W_WEEK(30)

            O_TBL.Rows.Add(oTA0009row)

            oTA0009row = O_TBL.NewRow
            oTA0009row.ItemArray = W_TA0009row.ItemArray
            oTA0009row("RECKBN") = "02"
            oTA0009row("RECKBNNAME") = ""
            CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
            oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
            oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
            oTA0009row("DAY01") = MinutesToHHMM(W_ACTTIME(0))
            oTA0009row("DAY02") = MinutesToHHMM(W_ACTTIME(1))
            oTA0009row("DAY03") = MinutesToHHMM(W_ACTTIME(2))
            oTA0009row("DAY04") = MinutesToHHMM(W_ACTTIME(3))
            oTA0009row("DAY05") = MinutesToHHMM(W_ACTTIME(4))
            oTA0009row("DAY06") = MinutesToHHMM(W_ACTTIME(5))
            oTA0009row("DAY07") = MinutesToHHMM(W_ACTTIME(6))
            oTA0009row("DAY08") = MinutesToHHMM(W_ACTTIME(7))
            oTA0009row("DAY09") = MinutesToHHMM(W_ACTTIME(8))
            oTA0009row("DAY10") = MinutesToHHMM(W_ACTTIME(9))
            oTA0009row("DAY11") = MinutesToHHMM(W_ACTTIME(10))
            oTA0009row("DAY12") = MinutesToHHMM(W_ACTTIME(11))
            oTA0009row("DAY13") = MinutesToHHMM(W_ACTTIME(12))
            oTA0009row("DAY14") = MinutesToHHMM(W_ACTTIME(13))
            oTA0009row("DAY15") = MinutesToHHMM(W_ACTTIME(14))
            oTA0009row("DAY16") = MinutesToHHMM(W_ACTTIME(15))
            oTA0009row("DAY17") = MinutesToHHMM(W_ACTTIME(16))
            oTA0009row("DAY18") = MinutesToHHMM(W_ACTTIME(17))
            oTA0009row("DAY19") = MinutesToHHMM(W_ACTTIME(18))
            oTA0009row("DAY20") = MinutesToHHMM(W_ACTTIME(19))
            oTA0009row("DAY21") = MinutesToHHMM(W_ACTTIME(20))
            oTA0009row("DAY22") = MinutesToHHMM(W_ACTTIME(21))
            oTA0009row("DAY23") = MinutesToHHMM(W_ACTTIME(22))
            oTA0009row("DAY24") = MinutesToHHMM(W_ACTTIME(23))
            oTA0009row("DAY25") = MinutesToHHMM(W_ACTTIME(24))
            oTA0009row("DAY26") = MinutesToHHMM(W_ACTTIME(25))
            oTA0009row("DAY27") = MinutesToHHMM(W_ACTTIME(26))
            oTA0009row("DAY28") = MinutesToHHMM(W_ACTTIME(27))
            oTA0009row("DAY29") = MinutesToHHMM(W_ACTTIME(28))
            oTA0009row("DAY30") = MinutesToHHMM(W_ACTTIME(29))
            oTA0009row("DAY31") = MinutesToHHMM(W_ACTTIME(30))
            For j As Integer = 0 To W_ACTTIME.Count - 1
                W_TTL(1) += W_ACTTIME(j)
                If W_ACTTIME(j) > 960 Then
                    W_OVERCNT(1) += 1
                End If
            Next
            oTA0009row("OVERCNT") = ZeroToSpace(W_OVERCNT(1))
            oTA0009row("TTL") = MinutesToHHMM(W_TTL(1))
            W_TTLSA(1) = CInt(WF_MAXWORKTIME.Text) * 60 - W_TTL(1)
            oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(1))
            oTA0009row("HOLKBN01") = W_WEEK(0)
            oTA0009row("HOLKBN02") = W_WEEK(1)
            oTA0009row("HOLKBN03") = W_WEEK(2)
            oTA0009row("HOLKBN04") = W_WEEK(3)
            oTA0009row("HOLKBN05") = W_WEEK(4)
            oTA0009row("HOLKBN06") = W_WEEK(5)
            oTA0009row("HOLKBN07") = W_WEEK(6)
            oTA0009row("HOLKBN08") = W_WEEK(7)
            oTA0009row("HOLKBN09") = W_WEEK(8)
            oTA0009row("HOLKBN10") = W_WEEK(9)
            oTA0009row("HOLKBN11") = W_WEEK(10)
            oTA0009row("HOLKBN12") = W_WEEK(11)
            oTA0009row("HOLKBN13") = W_WEEK(12)
            oTA0009row("HOLKBN14") = W_WEEK(13)
            oTA0009row("HOLKBN15") = W_WEEK(14)
            oTA0009row("HOLKBN16") = W_WEEK(15)
            oTA0009row("HOLKBN17") = W_WEEK(16)
            oTA0009row("HOLKBN18") = W_WEEK(17)
            oTA0009row("HOLKBN19") = W_WEEK(18)
            oTA0009row("HOLKBN20") = W_WEEK(19)
            oTA0009row("HOLKBN21") = W_WEEK(20)
            oTA0009row("HOLKBN22") = W_WEEK(21)
            oTA0009row("HOLKBN23") = W_WEEK(22)
            oTA0009row("HOLKBN24") = W_WEEK(23)
            oTA0009row("HOLKBN25") = W_WEEK(24)
            oTA0009row("HOLKBN26") = W_WEEK(25)
            oTA0009row("HOLKBN27") = W_WEEK(26)
            oTA0009row("HOLKBN28") = W_WEEK(27)
            oTA0009row("HOLKBN29") = W_WEEK(28)
            oTA0009row("HOLKBN30") = W_WEEK(29)
            oTA0009row("HOLKBN31") = W_WEEK(30)

            O_TBL.Rows.Add(oTA0009row)

            oTA0009row = O_TBL.NewRow
            oTA0009row.ItemArray = W_TA0009row.ItemArray
            oTA0009row("RECKBN") = "03"
            oTA0009row("RECKBNNAME") = ""
            CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
            oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
            oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
            oTA0009row("DAY01") = MinutesToHHMM(W_HANDLTIME(0))
            oTA0009row("DAY02") = MinutesToHHMM(W_HANDLTIME(1))
            oTA0009row("DAY03") = MinutesToHHMM(W_HANDLTIME(2))
            oTA0009row("DAY04") = MinutesToHHMM(W_HANDLTIME(3))
            oTA0009row("DAY05") = MinutesToHHMM(W_HANDLTIME(4))
            oTA0009row("DAY06") = MinutesToHHMM(W_HANDLTIME(5))
            oTA0009row("DAY07") = MinutesToHHMM(W_HANDLTIME(6))
            oTA0009row("DAY08") = MinutesToHHMM(W_HANDLTIME(7))
            oTA0009row("DAY09") = MinutesToHHMM(W_HANDLTIME(8))
            oTA0009row("DAY10") = MinutesToHHMM(W_HANDLTIME(9))
            oTA0009row("DAY11") = MinutesToHHMM(W_HANDLTIME(10))
            oTA0009row("DAY12") = MinutesToHHMM(W_HANDLTIME(11))
            oTA0009row("DAY13") = MinutesToHHMM(W_HANDLTIME(12))
            oTA0009row("DAY14") = MinutesToHHMM(W_HANDLTIME(13))
            oTA0009row("DAY15") = MinutesToHHMM(W_HANDLTIME(14))
            oTA0009row("DAY16") = MinutesToHHMM(W_HANDLTIME(15))
            oTA0009row("DAY17") = MinutesToHHMM(W_HANDLTIME(16))
            oTA0009row("DAY18") = MinutesToHHMM(W_HANDLTIME(17))
            oTA0009row("DAY19") = MinutesToHHMM(W_HANDLTIME(18))
            oTA0009row("DAY20") = MinutesToHHMM(W_HANDLTIME(19))
            oTA0009row("DAY21") = MinutesToHHMM(W_HANDLTIME(20))
            oTA0009row("DAY22") = MinutesToHHMM(W_HANDLTIME(21))
            oTA0009row("DAY23") = MinutesToHHMM(W_HANDLTIME(22))
            oTA0009row("DAY24") = MinutesToHHMM(W_HANDLTIME(23))
            oTA0009row("DAY25") = MinutesToHHMM(W_HANDLTIME(24))
            oTA0009row("DAY26") = MinutesToHHMM(W_HANDLTIME(25))
            oTA0009row("DAY27") = MinutesToHHMM(W_HANDLTIME(26))
            oTA0009row("DAY28") = MinutesToHHMM(W_HANDLTIME(27))
            oTA0009row("DAY29") = MinutesToHHMM(W_HANDLTIME(28))
            oTA0009row("DAY30") = MinutesToHHMM(W_HANDLTIME(29))
            oTA0009row("DAY31") = MinutesToHHMM(W_HANDLTIME(30))
            For j As Integer = 0 To W_HANDLTIME.Count - 1
                W_TTL(2) += W_HANDLTIME(j)
                If W_HANDLTIME(j) > 540 Then
                    W_OVERCNT(2) += 1
                End If
            Next
            oTA0009row("OVERCNT") = ZeroToSpace(W_OVERCNT(2))
            oTA0009row("TTL") = MinutesToHHMM(W_TTL(2))
            oTA0009row("TTLSA") = MinutesToHHMM(W_TTLSA(2))
            oTA0009row("HOLKBN01") = W_WEEK(0)
            oTA0009row("HOLKBN02") = W_WEEK(1)
            oTA0009row("HOLKBN03") = W_WEEK(2)
            oTA0009row("HOLKBN04") = W_WEEK(3)
            oTA0009row("HOLKBN05") = W_WEEK(4)
            oTA0009row("HOLKBN06") = W_WEEK(5)
            oTA0009row("HOLKBN07") = W_WEEK(6)
            oTA0009row("HOLKBN08") = W_WEEK(7)
            oTA0009row("HOLKBN09") = W_WEEK(8)
            oTA0009row("HOLKBN10") = W_WEEK(9)
            oTA0009row("HOLKBN11") = W_WEEK(10)
            oTA0009row("HOLKBN12") = W_WEEK(11)
            oTA0009row("HOLKBN13") = W_WEEK(12)
            oTA0009row("HOLKBN14") = W_WEEK(13)
            oTA0009row("HOLKBN15") = W_WEEK(14)
            oTA0009row("HOLKBN16") = W_WEEK(15)
            oTA0009row("HOLKBN17") = W_WEEK(16)
            oTA0009row("HOLKBN18") = W_WEEK(17)
            oTA0009row("HOLKBN19") = W_WEEK(18)
            oTA0009row("HOLKBN20") = W_WEEK(19)
            oTA0009row("HOLKBN21") = W_WEEK(20)
            oTA0009row("HOLKBN22") = W_WEEK(21)
            oTA0009row("HOLKBN23") = W_WEEK(22)
            oTA0009row("HOLKBN24") = W_WEEK(23)
            oTA0009row("HOLKBN25") = W_WEEK(24)
            oTA0009row("HOLKBN26") = W_WEEK(25)
            oTA0009row("HOLKBN27") = W_WEEK(26)
            oTA0009row("HOLKBN28") = W_WEEK(27)
            oTA0009row("HOLKBN29") = W_WEEK(28)
            oTA0009row("HOLKBN30") = W_WEEK(29)
            oTA0009row("HOLKBN31") = W_WEEK(30)

            O_TBL.Rows.Add(oTA0009row)

            oTA0009row = O_TBL.NewRow
            oTA0009row.ItemArray = W_TA0009row.ItemArray
            oTA0009row("RECKBN") = "04"
            oTA0009row("RECKBNNAME") = ""
            CodeToName("RECKBN", oTA0009row("RECKBN"), oTA0009row("RECKBNNAME"), WW_DUMMY)
            oTA0009row("MAXWORKTIME") = WF_MAXWORKTIME.Text
            oTA0009row("MAXORVERTIME") = WF_MAXORVERTIME.Text
            oTA0009row("DAY01") = W_NIPPO(0)
            oTA0009row("DAY02") = W_NIPPO(1)
            oTA0009row("DAY03") = W_NIPPO(2)
            oTA0009row("DAY04") = W_NIPPO(3)
            oTA0009row("DAY05") = W_NIPPO(4)
            oTA0009row("DAY06") = W_NIPPO(5)
            oTA0009row("DAY07") = W_NIPPO(6)
            oTA0009row("DAY08") = W_NIPPO(7)
            oTA0009row("DAY09") = W_NIPPO(8)
            oTA0009row("DAY10") = W_NIPPO(9)
            oTA0009row("DAY11") = W_NIPPO(10)
            oTA0009row("DAY12") = W_NIPPO(11)
            oTA0009row("DAY13") = W_NIPPO(12)
            oTA0009row("DAY14") = W_NIPPO(13)
            oTA0009row("DAY15") = W_NIPPO(14)
            oTA0009row("DAY16") = W_NIPPO(15)
            oTA0009row("DAY17") = W_NIPPO(16)
            oTA0009row("DAY18") = W_NIPPO(17)
            oTA0009row("DAY19") = W_NIPPO(18)
            oTA0009row("DAY20") = W_NIPPO(19)
            oTA0009row("DAY21") = W_NIPPO(20)
            oTA0009row("DAY22") = W_NIPPO(21)
            oTA0009row("DAY23") = W_NIPPO(22)
            oTA0009row("DAY24") = W_NIPPO(23)
            oTA0009row("DAY25") = W_NIPPO(24)
            oTA0009row("DAY26") = W_NIPPO(25)
            oTA0009row("DAY27") = W_NIPPO(26)
            oTA0009row("DAY28") = W_NIPPO(27)
            oTA0009row("DAY29") = W_NIPPO(28)
            oTA0009row("DAY30") = W_NIPPO(29)
            oTA0009row("DAY31") = W_NIPPO(30)
            oTA0009row("OVERCNT") = ""
            oTA0009row("TTL") = ""
            oTA0009row("TTLSA") = ""
            oTA0009row("HOLKBN01") = W_WEEK(0)
            oTA0009row("HOLKBN02") = W_WEEK(1)
            oTA0009row("HOLKBN03") = W_WEEK(2)
            oTA0009row("HOLKBN04") = W_WEEK(3)
            oTA0009row("HOLKBN05") = W_WEEK(4)
            oTA0009row("HOLKBN06") = W_WEEK(5)
            oTA0009row("HOLKBN07") = W_WEEK(6)
            oTA0009row("HOLKBN08") = W_WEEK(7)
            oTA0009row("HOLKBN09") = W_WEEK(8)
            oTA0009row("HOLKBN10") = W_WEEK(9)
            oTA0009row("HOLKBN11") = W_WEEK(10)
            oTA0009row("HOLKBN12") = W_WEEK(11)
            oTA0009row("HOLKBN13") = W_WEEK(12)
            oTA0009row("HOLKBN14") = W_WEEK(13)
            oTA0009row("HOLKBN15") = W_WEEK(14)
            oTA0009row("HOLKBN16") = W_WEEK(15)
            oTA0009row("HOLKBN17") = W_WEEK(16)
            oTA0009row("HOLKBN18") = W_WEEK(17)
            oTA0009row("HOLKBN19") = W_WEEK(18)
            oTA0009row("HOLKBN20") = W_WEEK(19)
            oTA0009row("HOLKBN21") = W_WEEK(20)
            oTA0009row("HOLKBN22") = W_WEEK(21)
            oTA0009row("HOLKBN23") = W_WEEK(22)
            oTA0009row("HOLKBN24") = W_WEEK(23)
            oTA0009row("HOLKBN25") = W_WEEK(24)
            oTA0009row("HOLKBN26") = W_WEEK(25)
            oTA0009row("HOLKBN27") = W_WEEK(26)
            oTA0009row("HOLKBN28") = W_WEEK(27)
            oTA0009row("HOLKBN29") = W_WEEK(28)
            oTA0009row("HOLKBN30") = W_WEEK(29)
            oTA0009row("HOLKBN31") = W_WEEK(30)

            O_TBL.Rows.Add(oTA0009row)
        End If

    End Sub

    ''' <summary>
    ''' セレクタの初期設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InitialSelector()

        Dim WW_POS As String = ""
        Dim WW_TBLview As DataView
        Dim WW_GRPtbl As DataTable

        'テンポラリDB項目作成
        If IsNothing(SELECTORtbl) Then SELECTORtbl = New DataTable
        SELECTORtbl.Clear()
        SELECTORtbl.Columns.Add("CODE", GetType(String))                        'CODE               コード
        SELECTORtbl.Columns.Add("NAME", GetType(String))                        'NAME               名称

        '---------------------------------------------------
        '組織セレクター作成
        '---------------------------------------------------
        Dim WW_Cols As String() = {"NACOFFICESORG", "NACOFFICESORGNAME"}
        WW_TBLview = New DataView(TA0009tbl)
        WW_TBLview.Sort = "NACOFFICESORG"
        '出荷部署、出荷部署名でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        '組織セレクター作成
        Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = "00000"
        SELECTORrow("NAME") = "全て"
        SELECTORtbl.Rows.Add(SELECTORrow)
        For Each TA0009row As DataRow In WW_GRPtbl.Rows
            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0009row("NACOFFICESORG")
            SELECTORrow("NAME") = TA0009row("NACOFFICESORGNAME") & "(" & TA0009row("NACOFFICESORG") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "CODE, NAME"
        SELECTORtbl = CS0026TblSort.sort()

        '●セレクター設定処理
        WF_ORGselector.DataSource = SELECTORtbl
        WF_ORGselector.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WW_POS = ""
            WF_SELECTOR_PosiORG.Value = ""
        Else
            WW_POS = SELECTORtbl.Rows(0)("CODE")
            WF_SELECTOR_PosiORG.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        Repeater_set("0", WF_ORGselector, "WF_SELorg_VALUE", "WF_SELorg_TEXT", WW_POS)

        '---------------------------------------------------
        '乗務員セレクター作成
        '---------------------------------------------------
        SELECTORtbl.Clear()
        WW_GRPtbl.Clear()
        WW_Cols = {}

        WW_Cols = {"PAYSTAFFCODE", "PAYSTAFFCODENAME"}
        WW_TBLview = New DataView(TA0009tbl)
        WW_TBLview.Sort = "PAYSTAFFCODE"

        '乗務員、乗務員名称でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        SELECTORrow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = "00000"
        SELECTORrow("NAME") = "全て"
        SELECTORtbl.Rows.Add(SELECTORrow)
        For Each TA0009row As DataRow In WW_GRPtbl.Rows
            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0009row("PAYSTAFFCODE")
            SELECTORrow("NAME") = TA0009row("PAYSTAFFCODENAME") & "(" & TA0009row("PAYSTAFFCODE") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "CODE, NAME"
        SELECTORtbl = CS0026TblSort.sort()

        '●セレクター設定処理
        WF_STAFFselector.DataSource = SELECTORtbl
        WF_STAFFselector.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WW_POS = ""
            WF_SELECTOR_PosiSTAFF.Value = ""
        Else
            WW_POS = SELECTORtbl.Rows(0)("CODE")
            WF_SELECTOR_PosiSTAFF.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        Repeater_set("1", WF_STAFFselector, "WF_SELstaff_VALUE", "WF_SELstaff_TEXT", WW_POS)

        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_GRPtbl.Dispose()
        WW_GRPtbl = Nothing

    End Sub
    ''' <summary>
    ''' リピータ項目設定
    ''' </summary>
    ''' <param name="I_KBN"></param>
    ''' <param name="I_SELECTOR_OBJ"></param>
    ''' <param name="I_VALUE_OBJ"></param>
    ''' <param name="I_TEXT_OBJ"></param>
    ''' <param name="I_POS"></param>
    Protected Sub Repeater_set(ByVal I_KBN As String, ByRef I_SELECTOR_OBJ As Object, ByRef I_VALUE_OBJ As String, ByRef I_TEXT_OBJ As String, ByVal I_POS As String)

        For i As Integer = 0 To I_SELECTOR_OBJ.Items.Count - 1
            '値　
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), System.Web.UI.WebControls.Label).Text = SELECTORtbl.Rows(i)("CODE")
            'テキスト
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), System.Web.UI.WebControls.Label).Text = "　" & SELECTORtbl.Rows(i)("NAME")

            '背景色
            If CType(I_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), System.Web.UI.WebControls.Label).Text = I_POS Then
                CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;font-size:1.3rem;"
            Else
                CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;font-size:1.3rem;"
            End If

            'イベント追加
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), System.Web.UI.WebControls.Label).Attributes.Remove("onclick")
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), System.Web.UI.WebControls.Label).Attributes.Add("onclick", "SELECTOR_Click('" & I_KBN & "','" & SELECTORtbl.Rows(i)("CODE") & "');")
        Next

    End Sub
    ''' <summary>
    ''' セレクタークリック(選択変更)処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SELECTOR_Click()

        Dim WW_RADIO As Integer = WF_SelectorMView.ActiveViewIndex
        '■ セレクター表示切替
        '組織
        If WW_RADIO = 0 Then
            For i As Integer = 0 To WF_ORGselector.Items.Count - 1
                '背景色
                If CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTOR_PosiORG.Value Then
                    CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;font-size:1.3rem;"
                Else
                    CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;font-size:1.3rem;"
                End If
            Next

        End If

        '乗務員
        If WW_RADIO = 1 Then
            For i As Integer = 0 To WF_STAFFselector.Items.Count - 1
                '背景色
                If CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTOR_PosiSTAFF.Value Then
                    CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;font-size:1.3rem;"
                Else
                    CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;font-size:1.3rem;"
                End If
            Next

        End If

    End Sub

    ''' <summary>
    ''' TA0009tbl項目設定
    ''' </summary>
    ''' <param name="IO_TBL">列追加対象テーブル</param>
    ''' <remarks></remarks>
    Protected Sub AddColumnToTA0009tbl(ByRef IO_TBL As DataTable)

        '〇新規作成
        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        '○DB項目クリア
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        '○共通項目
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))                   'DBの固定フィールド
        IO_TBL.Columns.Add("OPERATION", GetType(String))                  'DBの固定フィールド
        IO_TBL.Columns.Add("TIMSTP", GetType(String))                     'DBの固定フィールド
        IO_TBL.Columns.Add("SELECT", GetType(Integer))                    'DBの固定フィールド
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))                    'DBの固定フィールド

        '○画面固有項目
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))                   '会社
        IO_TBL.Columns.Add("CAMPNAME", GetType(String))                   '会社名称
        IO_TBL.Columns.Add("KEIJOYMD", GetType(String))                   '計上日付
        IO_TBL.Columns.Add("ACKEIJOORG", GetType(String))                 '計上部署
        IO_TBL.Columns.Add("ACKEIJOORGNAME", GetType(String))             '計上部署名
        IO_TBL.Columns.Add("DENYMD", GetType(String))                     '伝票日付
        IO_TBL.Columns.Add("DENNO", GetType(String))                      '伝票番号
        IO_TBL.Columns.Add("KANRENDENNO", GetType(String))                '関連伝票No＋明細No
        IO_TBL.Columns.Add("DTLNO", GetType(String))                      '明細番号
        IO_TBL.Columns.Add("ACACHANTEI", GetType(String))                 '仕訳決定
        IO_TBL.Columns.Add("ACACHANTEINAME", GetType(String))             '仕訳決定名称

        IO_TBL.Columns.Add("NACSHUKODATE", GetType(String))               '出庫日・作業日

        IO_TBL.Columns.Add("NACHAISTDATE", GetType(String))               '実績・配送作業開始日時
        IO_TBL.Columns.Add("NACHAIENDDATE", GetType(String))              '実績・配送作業終了日時
        IO_TBL.Columns.Add("NACHAIWORKTIME", GetType(String))             '実績・配送作業時間
        IO_TBL.Columns.Add("NACGESSTDATE", GetType(String))               '実績・下車作業開始日時
        IO_TBL.Columns.Add("NACGESENDDATE", GetType(String))              '実績・下車作業終了日時
        IO_TBL.Columns.Add("NACGESWORKTIME", GetType(String))             '実績・下車作業時間
        IO_TBL.Columns.Add("NACCHOWORKTIME", GetType(String))             '実績・勤怠調整時間
        IO_TBL.Columns.Add("NACTTLWORKTIME", GetType(String))             '実績・配送合計時間Σ

        IO_TBL.Columns.Add("NACOUTWORKTIME", GetType(String))             '実績・就業外時間

        IO_TBL.Columns.Add("NACJITTLETIME", GetType(String))              '実績・実車時間合計Σ
        IO_TBL.Columns.Add("NACKUTTLTIME", GetType(String))               '実績・空車時間合計Σ

        IO_TBL.Columns.Add("NACBREAKSTDATE", GetType(String))             '実績・休憩開始日時
        IO_TBL.Columns.Add("NACBREAKENDDATE", GetType(String))            '実績・休憩終了日時
        IO_TBL.Columns.Add("NACBREAKTIME", GetType(String))               '実績・休憩時間
        IO_TBL.Columns.Add("NACCHOBREAKTIME", GetType(String))            '実績・休憩調整時間
        IO_TBL.Columns.Add("NACTTLBREAKTIME", GetType(String))            '実績・休憩合計時間Σ


        IO_TBL.Columns.Add("NACOFFICESORG", GetType(String))              '実績・従業作業部署
        IO_TBL.Columns.Add("NACOFFICESORGNAME", GetType(String))          '実績・従業作業部署名称
        IO_TBL.Columns.Add("NACOFFICETIME", GetType(String))              '実績・従業時間
        IO_TBL.Columns.Add("NACOFFICEBREAKTIME", GetType(String))         '実績・従業休憩時間
        IO_TBL.Columns.Add("PAYSHUSHADATE", GetType(String))              '出社日時
        IO_TBL.Columns.Add("PAYTAISHADATE", GetType(String))              '退社日時
        IO_TBL.Columns.Add("PAYSTAFFKBN", GetType(String))                '社員区分
        IO_TBL.Columns.Add("PAYSTAFFKBNNAME", GetType(String))            '社員区分名称
        IO_TBL.Columns.Add("PAYSTAFFCODE", GetType(String))               '従業員
        IO_TBL.Columns.Add("PAYSTAFFCODENAME", GetType(String))           '従業員名称
        IO_TBL.Columns.Add("PAYMORG", GetType(String))                    '従業員管理部署
        IO_TBL.Columns.Add("PAYMORGNAME", GetType(String))                '従業員管理部署名称
        IO_TBL.Columns.Add("PAYHORG", GetType(String))                    '従業員配属部署
        IO_TBL.Columns.Add("PAYHORGNAME", GetType(String))                '従業員配属部署名称
        IO_TBL.Columns.Add("PAYHOLIDAYKBN", GetType(String))              '休日区分
        IO_TBL.Columns.Add("PAYHOLIDAYKBNNAME", GetType(String))          '休日区分名称
        IO_TBL.Columns.Add("PAYKBN", GetType(String))                     '勤怠区分
        IO_TBL.Columns.Add("PAYKBNNAME", GetType(String))                 '勤怠区分名称
        IO_TBL.Columns.Add("PAYSHUKCHOKKBN", GetType(String))             '宿日直区分
        IO_TBL.Columns.Add("PAYSHUKCHOKKBNNAME", GetType(String))         '宿日直区分名称
        IO_TBL.Columns.Add("PAYJYOMUKBN", GetType(String))                '乗務区分
        IO_TBL.Columns.Add("PAYJYOMUKBNNAME", GetType(String))            '乗務区分名称


        IO_TBL.Columns.Add("PAYWORKTIME", GetType(String))                '所定労働時間
        IO_TBL.Columns.Add("PAYNIGHTTIME", GetType(String))               '所定深夜時間
        IO_TBL.Columns.Add("PAYORVERTIME", GetType(String))               '平日残業時間
        IO_TBL.Columns.Add("PAYWNIGHTTIME", GetType(String))              '平日深夜時間
        IO_TBL.Columns.Add("PAYWSWORKTIME", GetType(String))              '日曜出勤時間
        IO_TBL.Columns.Add("PAYSNIGHTTIME", GetType(String))              '日曜深夜時間
        IO_TBL.Columns.Add("PAYHWORKTIME", GetType(String))               '休日出勤時間
        IO_TBL.Columns.Add("PAYHNIGHTTIME", GetType(String))              '休日深夜時間
        IO_TBL.Columns.Add("PAYBREAKTIME", GetType(String))               '休憩時間

        IO_TBL.Columns.Add("SUPPORTKBN", GetType(String))                 '応援者区分（0:自部署、1:応援者（他部署）
        IO_TBL.Columns.Add("TAISHYM", GetType(String))                    '対象年月
        IO_TBL.Columns.Add("RECKBN", GetType(String))                     'レコード区分（01:拘束、02:残業、03:ハンドル、04:マーク）
        IO_TBL.Columns.Add("RECKBNNAME", GetType(String))                 'レコード区分（01:拘束、02:残業、03:ハンドル、04:マーク）
        IO_TBL.Columns.Add("OVERCNT", GetType(String))                    '超過回数
        IO_TBL.Columns.Add("MAXWORKTIME", GetType(String))                '拘束Max
        IO_TBL.Columns.Add("MAXORVERTIME", GetType(String))               '残業Max
        IO_TBL.Columns.Add("DAY01", GetType(String))                      '1日
        IO_TBL.Columns.Add("DAY02", GetType(String))                      '2日
        IO_TBL.Columns.Add("DAY03", GetType(String))                      '3日
        IO_TBL.Columns.Add("DAY04", GetType(String))                      '4日
        IO_TBL.Columns.Add("DAY05", GetType(String))                      '5日
        IO_TBL.Columns.Add("DAY06", GetType(String))                      '6日
        IO_TBL.Columns.Add("DAY07", GetType(String))                      '7日
        IO_TBL.Columns.Add("DAY08", GetType(String))                      '8日
        IO_TBL.Columns.Add("DAY09", GetType(String))                      '9日
        IO_TBL.Columns.Add("DAY10", GetType(String))                      '10日
        IO_TBL.Columns.Add("DAY11", GetType(String))                      '11日
        IO_TBL.Columns.Add("DAY12", GetType(String))                      '12日
        IO_TBL.Columns.Add("DAY13", GetType(String))                      '13日
        IO_TBL.Columns.Add("DAY14", GetType(String))                      '14日
        IO_TBL.Columns.Add("DAY15", GetType(String))                      '15日
        IO_TBL.Columns.Add("DAY16", GetType(String))                      '16日
        IO_TBL.Columns.Add("DAY17", GetType(String))                      '17日
        IO_TBL.Columns.Add("DAY18", GetType(String))                      '18日
        IO_TBL.Columns.Add("DAY19", GetType(String))                      '19日
        IO_TBL.Columns.Add("DAY20", GetType(String))                      '20日
        IO_TBL.Columns.Add("DAY21", GetType(String))                      '21日
        IO_TBL.Columns.Add("DAY22", GetType(String))                      '22日
        IO_TBL.Columns.Add("DAY23", GetType(String))                      '23日
        IO_TBL.Columns.Add("DAY24", GetType(String))                      '24日
        IO_TBL.Columns.Add("DAY25", GetType(String))                      '25日
        IO_TBL.Columns.Add("DAY26", GetType(String))                      '26日
        IO_TBL.Columns.Add("DAY27", GetType(String))                      '27日
        IO_TBL.Columns.Add("DAY28", GetType(String))                      '28日
        IO_TBL.Columns.Add("DAY29", GetType(String))                      '29日
        IO_TBL.Columns.Add("DAY30", GetType(String))                      '30日
        IO_TBL.Columns.Add("DAY31", GetType(String))                      '31日
        IO_TBL.Columns.Add("TTL", GetType(String))                        '累計
        IO_TBL.Columns.Add("TTLSA", GetType(String))                      '累計差
        IO_TBL.Columns.Add("HOLKBN01", GetType(String))
        IO_TBL.Columns.Add("HOLKBN02", GetType(String))
        IO_TBL.Columns.Add("HOLKBN03", GetType(String))
        IO_TBL.Columns.Add("HOLKBN04", GetType(String))
        IO_TBL.Columns.Add("HOLKBN05", GetType(String))
        IO_TBL.Columns.Add("HOLKBN06", GetType(String))
        IO_TBL.Columns.Add("HOLKBN07", GetType(String))
        IO_TBL.Columns.Add("HOLKBN08", GetType(String))
        IO_TBL.Columns.Add("HOLKBN09", GetType(String))
        IO_TBL.Columns.Add("HOLKBN10", GetType(String))
        IO_TBL.Columns.Add("HOLKBN11", GetType(String))
        IO_TBL.Columns.Add("HOLKBN12", GetType(String))
        IO_TBL.Columns.Add("HOLKBN13", GetType(String))
        IO_TBL.Columns.Add("HOLKBN14", GetType(String))
        IO_TBL.Columns.Add("HOLKBN15", GetType(String))
        IO_TBL.Columns.Add("HOLKBN16", GetType(String))
        IO_TBL.Columns.Add("HOLKBN17", GetType(String))
        IO_TBL.Columns.Add("HOLKBN18", GetType(String))
        IO_TBL.Columns.Add("HOLKBN19", GetType(String))
        IO_TBL.Columns.Add("HOLKBN20", GetType(String))
        IO_TBL.Columns.Add("HOLKBN21", GetType(String))
        IO_TBL.Columns.Add("HOLKBN22", GetType(String))
        IO_TBL.Columns.Add("HOLKBN23", GetType(String))
        IO_TBL.Columns.Add("HOLKBN24", GetType(String))
        IO_TBL.Columns.Add("HOLKBN25", GetType(String))
        IO_TBL.Columns.Add("HOLKBN26", GetType(String))
        IO_TBL.Columns.Add("HOLKBN27", GetType(String))
        IO_TBL.Columns.Add("HOLKBN28", GetType(String))
        IO_TBL.Columns.Add("HOLKBN29", GetType(String))
        IO_TBL.Columns.Add("HOLKBN30", GetType(String))
        IO_TBL.Columns.Add("HOLKBN31", GetType(String))
    End Sub

    ''' <summary>
    ''' TB0005tbl項目設定
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub AddColumnToMB0005Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        '○DB項目クリア
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()
        '○共通項目
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))                 '会社
        IO_TBL.Columns.Add("WORKINGYMD", GetType(String))               '年月日
        IO_TBL.Columns.Add("WORKINGDD", GetType(String))               '日
        IO_TBL.Columns.Add("WORKINGWEEK", GetType(String))              '曜日
        IO_TBL.Columns.Add("WORKINGWEEKNAME", GetType(String))          '曜日名称
        IO_TBL.Columns.Add("WORKINGKBN", GetType(String))               '祝日
        IO_TBL.Columns.Add("WORKINGKBNNAME", GetType(String))           '祝日名称
    End Sub

    ''' <summary>
    ''' 時刻変換
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Function MinutesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        Dim WW_ABS As Integer = System.Math.Abs(I_PARAM)

        WW_HHMM = Int(WW_ABS / 60) * 100 + WW_ABS Mod 60
        If I_PARAM < 0 Then
            WW_HHMM = WW_HHMM * -1
        End If
        Return ZEROtoSpace(Format(WW_HHMM, "0#:##"))
    End Function
    ''' <summary>
    ''' 時刻変換
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Private Function HHMMToMinutes(ByVal I_PARAM As String) As Integer
        Dim WW_TIME As String() = {}
        Dim WW_SIGN As String = "+"
        If Mid(I_PARAM, 1, 1) = "-" Then
            WW_SIGN = "-"
            WW_TIME = I_PARAM.Replace("-", "").Split(":")
        Else
            WW_SIGN = "+"
            WW_TIME = I_PARAM.Split(":")
        End If

        If I_PARAM = Nothing OrElse WW_TIME.Count <> 2 Then
            HHMMToMinutes = 0
        Else
            HHMMToMinutes = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
            If WW_SIGN = "-" Then
                HHMMToMinutes = HHMMToMinutes * -1
            End If
        End If
    End Function

    ''' <summary>
    ''' 変換（0 or 00:00をスペースへ）帳票出力用
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Function ZeroToSpace(ByVal I_PARAM As String) As String
        Dim WW_TIME As String() = I_PARAM.Split(":")

        If WW_TIME.Count > 1 Then
            If I_PARAM = "00:00" Then
                Return ""
            End If
        Else
            If Val(I_PARAM) = 0 Then
                Return ""
            End If
        End If
        Return I_PARAM

    End Function

    ''' <summary>
    ''' 部署コード変換
    ''' </summary>
    ''' <param name="I_ORG">変換前部署コード</param>
    ''' <param name="O_ORG">変換後部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub ConvORGCode(ByVal I_ORG As String, ByRef O_ORG As String, ByRef O_RTN As String)

        O_ORG = I_ORG
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT CODE                              ")
                SQLStr.AppendLine(" FROM   M0006_STRUCT    M06               ")
                SQLStr.AppendLine(" WHERE  M06.CAMPCODE     = @P01           ")
                SQLStr.AppendLine("   AND  M06.OBJECT       = 'ORG'          ")
                SQLStr.AppendLine("   AND  M06.STRUCT       = '勤怠管理組織' ")
                SQLStr.AppendLine("   AND  M06.GRCODE01     = @P02           ")
                SQLStr.AppendLine("   AND  M06.STYMD       <= @P04           ")
                SQLStr.AppendLine("   AND  M06.ENDYMD      >= @P03           ")
                SQLStr.AppendLine("   AND  M06.DELFLG      <> '1'            ")

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P01", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@P02", SqlDbType.NVarChar, 20).Value = I_ORG
                        .Add("@P03", SqlDbType.Date).Value = Date.Now
                        .Add("@P04", SqlDbType.Date).Value = Date.Now
                    End With

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            O_ORG = SQLdr("CODE")
                        End While

                    End Using
                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC001_FIXVALUE SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 勤怠締テーブル取得
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORG">部署コード</param>
    ''' <param name="I_TARGET_YM">対象年月</param>
    ''' <param name="O_LIMITFLG">取得する締フラグ</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Sub GetT00008Data(ByVal I_COMPCODE As String,
                         ByVal I_ORG As String,
                         ByVal I_TARGET_YM As String,
                         ByRef O_LIMITFLG As String,
                         ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection()
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT isnull(A.LIMITFLG,0) as LIMITFLG ")
                SQLStr.AppendLine(" FROM   T0008_KINTAISTAT A               ")
                SQLStr.AppendLine(" WHERE  CAMPCODE  = @CAMPCODE            ")
                SQLStr.AppendLine("   AND  ORGCODE   = @HORG                ")
                SQLStr.AppendLine("   AND  LIMITYM   = @TAISHOYM            ")
                SQLStr.AppendLine("   AND  DELFLG   <> '1'                  ")

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)

                    '○関連受注指定
                    With SQLcmd.Parameters
                        .Add("@CAMPCODE", SqlDbType.NVarChar, 20).Value = I_COMPCODE
                        .Add("@HORG", SqlDbType.NVarChar, 20).Value = I_ORG
                        .Add("@TAISHOYM", SqlDbType.NVarChar, 20).Value = I_TARGET_YM
                    End With

                    '■SQL実行
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            O_LIMITFLG = SQLdr("LIMITFLG")
                        End While

                    End Using
                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0008_KINTAISTAT"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0008_KINTAISTAT SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ''' <summary>
    ''' カレンダーテーブル(MB0005)の取得
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub GetCalendar(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        AddColumnToMB0005Tbl(MB0005tbl)

        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT  FORMAT(A.WORKINGYMD,'yyyy/MM/dd')  as WORKINGYMD ")
                SQLStr.AppendLine("        ,FORMAT(A.WORKINGYMD,'dd') as WORKINGDD ")
                SQLStr.AppendLine("        ,RTRIM(A.WORKINGWEEK) as WORKINGWEEK ")
                SQLStr.AppendLine("        ,RTRIM(B.VALUE1)      as WORKINGWEEKNAME ")
                SQLStr.AppendLine("        ,RTRIM(A.WORKINGKBN)  as WORKINGKBN ")
                SQLStr.AppendLine("        ,RTRIM(C.VALUE1)      as WORKINGKBNNAME ")
                SQLStr.AppendLine("   FROM  MB005_CALENDAR A ")
                SQLStr.AppendLine("  INNER  JOIN MC001_FIXVALUE B ")
                SQLStr.AppendLine("     ON  B.CAMPCODE  = A.CAMPCODE ")
                SQLStr.AppendLine("    AND  B.CLASS     = 'WORKINGWEEK' ")
                SQLStr.AppendLine("    AND  B.KEYCODE   = A.WORKINGWEEK ")
                SQLStr.AppendLine("    AND  B.DELFLG   <> '1' ")
                SQLStr.AppendLine("  INNER  JOIN  MC001_FIXVALUE C ")
                SQLStr.AppendLine("     ON  C.CAMPCODE  = A.CAMPCODE ")
                SQLStr.AppendLine("    AND  C.CLASS     = 'WORKINGKBN' ")
                SQLStr.AppendLine("    AND  C.KEYCODE   = A.WORKINGKBN ")
                SQLStr.AppendLine("    AND  C.DELFLG   <> '1' ")
                SQLStr.AppendLine("  WHERE  A.CAMPCODE    = @CAMPCODE ")
                SQLStr.AppendLine("    AND  A.WORKINGYMD >= @STYMD ")
                SQLStr.AppendLine("    AND  A.WORKINGYMD <= @ENDYMD ")
                SQLStr.AppendLine("    AND  A.DELFLG   <> '1' ")

                Using SQLcmd As New SqlCommand(SQLStr.ToString, SQLcon)
                    Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")

                    '○関連受注指定
                    With SQLcmd.Parameters
                        .Add("@CAMPCODE", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@STYMD", SqlDbType.Date).Value = work.WF_SEL_STYM.Text & "/" & "01"
                        .Add("@ENDYMD", SqlDbType.Date).Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                    End With

                    '■SQL実行
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            Dim MB0005row As DataRow = MB0005tbl.NewRow
                            MB0005row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                            MB0005row("WORKINGYMD") = SQLdr("WORKINGYMD")
                            MB0005row("WORKINGDD") = SQLdr("WORKINGDD")
                            MB0005row("WORKINGWEEK") = SQLdr("WORKINGWEEK")
                            MB0005row("WORKINGWEEKNAME") = SQLdr("WORKINGWEEKNAME")
                            MB0005row("WORKINGKBN") = SQLdr("WORKINGKBN")
                            MB0005row("WORKINGKBNNAME") = Replace(Replace(SQLdr("WORKINGKBNNAME"), "休日", ""), "平日", "")
                            MB0005tbl.Rows.Add(MB0005row)
                        End While

                    End Using
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB005_CALENDAR"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB005_CALENDAR SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' カレンダーによる色の設定
    ''' </summary>
    ''' <param name="IO_TABLE"></param>
    ''' <param name="I_TBL"></param>
    ''' <remarks></remarks>
    Protected Sub SetCalendarColor(ByRef IO_TABLE As Control, ByVal I_TBL As DataTable)

        For Each rows As Control In IO_TABLE.Controls
            If rows.GetType.Name.ToLower = "tableheaderrow" Then
                For Each th As TableHeaderCell In rows.Controls
                    If Not th.Attributes("cellfieldname").StartsWith("DAY") Then Continue For
                    For Each row As DataRow In I_TBL.Rows
                        If th.Attributes("cellfieldname").EndsWith(row("WORKINGDD")) Then
                            If row("WORKINGKBN") <> "0" Then
                                '休日は赤
                                th.ForeColor = Color.Red
                            ElseIf row("WORKINGWEEK") = "6" Then
                                '土曜日は水色
                                th.ForeColor = Color.Aqua
                            End If
                        End If
                    Next
                Next
            End If
        Next

    End Sub
    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">コード値</param>
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

                Case "RECKBN"
                    'レコード区分名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TA0009_RECKBN"))                     'レコード区分名称
            End Select
        End If

    End Sub

    ''' <summary>
    ''' 変数設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()
        '〇定数設定
        WF_YM.Text = CDate(work.WF_SEL_STYM.Text & "/01").ToString("yyyy年MM月")

        '■ 変数設定処理 ■
        '区分
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "RECKBN", WF_RECKBN.Text)
        '拘束MAX
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAXWORKTIME", WF_MAXWORKTIME.Text)
        '残業MAX
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAXORVERTIME", WF_MAXORVERTIME.Text)
    End Sub

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0009S Then                                                    '条件画面からの画面遷移
            If String.IsNullOrEmpty(Master.MAPID) Then Master.MAPID = GRTA0009WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
            '○Grid情報保存先のファイル名 
            work.WF_SEL_XMLsaveF.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0009-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            work.WF_SEL_XMLsaveF2.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0009INQ-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        End If

    End Sub

    ''' <summary>
    ''' 固定値リスト取得
    ''' </summary>
    ''' <param name="cls">固定値コード</param>
    Protected Function GetFixValueList(ByVal cls As String) As ListBox

        Dim retListBox As ListBox = New ListBox
        GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0007FIXVALUElst.CLAS = cls
        GS0007FIXVALUElst.LISTBOX1 = retListBox
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        retListBox = GS0007FIXVALUElst.LISTBOX1

        Return retListBox

    End Function

End Class




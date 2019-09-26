Imports System.IO.Compression
Imports System.Data.SqlClient

Public Class GRTA0002KINTAILIST
    Inherits Page

    'コンスタント
    Const CONST_CAMP_ENEX As String = "02"                  '会社コード（エネックス）
    Const CONST_CAMP_KNK As String = "03"                   '会社コード（近石）
    Const CONST_CAMP_NJS As String = "04"                   '会社コード（NJS）
    Const CONST_CAMP_JKT As String = "05"                   '会社コード（JKT）

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView            'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' テーブルソート
    ''' </summary>    
    Private CS0026TblSort As New CS0026TBLSORT              'テーブルソート
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                '帳票出力(入力：TBL)
    ''' <summary>
    ''' 帳票マージ出力
    ''' </summary>
    Private CS0047XLSMERGE As New CS0047XLSMERGE            '帳票マージ出力
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050SESSION As New CS0050SESSION              'セッション管理
    ''' <summary>
    ''' 勤怠関連共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                      '勤怠共通

    '検索結果格納ds
    Private TA0002ALL As DataTable                          '全データテーブル
    Private TA0002VIEWtbl As DataTable                      'Grid格納用テーブル
    Private T0005tbl As DataTable                           '全データテーブル
    Private T0010tbl As DataTable                           '全データテーブル
    Private SELECTORtbl As DataTable                        'TREE選択作成作業テーブル

    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERRCODE As String = String.Empty             'リターンコード
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
    Private Const CONST_DSPROWCOUNT As Integer = 45         '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 20      'マウススクロール時の増分
    ''' <summary>
    ''' 詳細部タブID
    ''' </summary>
    Private Const CONST_DETAIL_TABID As String = "DTL1"     '詳細部タブID
    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonPDF"                 '■ 印刷ボタンクリック時処理
                        WF_ButtonPDF_Click()
                    Case "WF_ButtonXLS"                 '■ ダウンロードボタンクリック時処理
                        WF_ButtonXLS_Click()
                    Case "WF_ButtonZIP"                 '■ ZIPボタンクリック時処理
                        WF_ButtonZIP_Click()
                    Case "WF_ButtonFIRST"               '■ 最始行ボタンクリック時処理
                        WF_ButtonFIRST_Click()
                    Case "WF_ButtonLAST"                '■ 最終行ボタンクリック時処理
                        WF_ButtonLAST_Click()
                    Case "WF_BACK"                      '■ 戻るボタンクリック時処理
                        WF_BACK_Click()
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_GridDBclick"               '■ GridViewダブルクリック処理
                        WF_Grid_DBclick()
                    Case "WF_RadioButonClick"           '■ 右ボックスラジオボタン選択時処理 
                        WF_RadioButon_Click()
                    Case "WF_MEMOChange"                '■ メモ欄保存処理
                        WF_MEMO_Change()
                    Case "WF_SELECTOR_SW_Click"         '■ セレクタ変更ラジオボタンクリック処理
                        SELECTOR_Click()
                End Select
            End If
            '○ 一覧再表示処理
            DisplayGrid()
        Else
            '〇初期化処理
            Initialize()
        End If

        '■ Close
        If Not IsNothing(T0005tbl) Then
            T0005tbl.Dispose()
            T0005tbl = Nothing
        End If
        If Not IsNothing(T0010tbl) Then
            T0010tbl.Dispose()
            T0010tbl = Nothing
        End If
        If Not IsNothing(SELECTORtbl) Then
            SELECTORtbl.Dispose()
            SELECTORtbl = Nothing
        End If
        If Not IsNothing(TA0002ALL) Then
            TA0002ALL.Dispose()
            TA0002ALL = Nothing
        End If
        If Not IsNothing(TA0002VIEWtbl) Then
            TA0002VIEWtbl.Dispose()
            TA0002VIEWtbl = Nothing
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
        MapRefelence()
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True
        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        WF_SEL_DATE.Text = work.WF_SEL_TAISHOYM.Text
        CodeToName("ORG", work.WF_SEL_HORG.Text, WF_SEL_ORG.Text, WW_DUMMY)

        '■ 全データ取得
        '○TA0002ALL取得
        GetAllTA0002()

        '○表示選択TREE表示
        InitialSelector()

        '○画面表示データ保存
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0002ALL) Then Exit Sub

        '■ GridView表示データ作成
        AddColumnToTA0002Tbl(TA0002VIEWtbl)
        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0002VIEWtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = Master.MAPID
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013PROFview.LEVENT = "ondblclick"
            CS0013PROFview.LFUNC = "ListDbClick"
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.HIDEOPERATIONOPT = True
            CS0013PROFview.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '重複チェック
        Dim WW_MSG As String = C_MESSAGE_NO.NORMAL
        T0007COM.T0007_DuplCheck(TA0002ALL, WW_MSG, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        Else
            rightview.addErrorReport(ControlChars.NewLine & WW_MSG)
        End If

        work.WF_IsHideDetailBox.Text = "1"
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ GridView表示データ作成
        AddColumnToTA0002Tbl(TA0002VIEWtbl)

        If work.WF_IsHideDetailBox.Text = "0" Then
            If Not Master.RecoverTable(TA0002VIEWtbl, work.WF_DTL_XMLsaveF.Text) Then Exit Sub
        Else
            '○TA0002VIEWtbl取得
            GetViewTA0002(WF_SELECTOR_Posi.Value)
        End If

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0002VIEWtbl.Rows.Count - 1
            If TA0002VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0002VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(TA0002VIEWtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        '一覧作成

        If work.WF_IsHideDetailBox.Text = "1" Then

            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString

            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = Master.MAPID
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = WW_TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013PROFview.LEVENT = "ondblclick"
            CS0013PROFview.LFUNC = "ListDbClick"
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.HIDEOPERATIONOPT = True
            CS0013PROFview.CS0013ProfView()

        ElseIf work.WF_IsHideDetailBox.Text = "0" AndAlso WF_ButtonClick.Value <> "WF_GridDBclick" Then

            WW_TBLview.RowFilter = "SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_SCROLLROWCOUNT).ToString

            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = Master.MAPID
            CS0013PROFview.VARI = "詳細"
            CS0013PROFview.SRCDATA = WW_TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea2
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.HIDEOPERATIONOPT = True
            CS0013PROFview.CS0013ProfView()

        End If

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If

    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        Dim WW_Dir As String = ""
        Dim WW_TEMPDir As String = ""

        '■ 作業用フォルダ・作業用ファイルの事前操作
        Try
            '○ 作業フォルダ存在確認＆作成(C:\apple\files\TEXTWORK)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK"
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\ユーザー名)　＆　前回処理ファイル削除
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
                'ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP\部署コード)　＆　前回処理ファイル削除
            WW_TEMPDir = CS0050SESSION.UPLOAD_PATH & "\TEXTWORK\TEMP" & "\" & work.WF_SEL_HORG.Text
            If System.IO.Directory.Exists(WW_TEMPDir) Then
                'TEMPフォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_TEMPDir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_TEMPDir)
            End If

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ 全選択Excel作成（メイン処理）
        '〇 TA0002VIEWtblカラム設定
        AddColumnToTA0002Tbl(TA0002VIEWtbl)

        WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '○TA0002VIEWtbl取得
            TA0002VIEWtbl.Clear()
            GetViewTA0002(CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text)

            '帳票出力用編集
            'ＥＮＥＸの場合
            If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_ENEX Then
                EditListEnex(TA0002VIEWtbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                    Exit Sub
                End If
            End If

            'ＮＪＳの場合
            If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_NJS Then
                EditListNJS(TA0002VIEWtbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                    Exit Sub
                End If
            End If

            '近石の場合
            If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_KNK Then
                EditListKNK(TA0002VIEWtbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                    Exit Sub
                End If
            End If

            'ＪＫＴの場合
            If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_JKT Then
                EditListJKT(TA0002VIEWtbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                    Exit Sub
                End If
            End If

            '○ 帳票出力
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = Master.MAPID                       '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0002VIEWtbl                        'データ参照DataTable
            CS0030REPORT.CS0030REPORT()
            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text & ".xlsx", True)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
                Exit Sub
            End If
        Next

        '■ ダウンロード
        '○ 圧縮実行
        WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        Dim WW_Dir2 As String = ""

        '○ 帳票出力
        CS0047XLSMERGE.DIR = WW_Dir                                   'PARAM01:フォルダー
        CS0047XLSMERGE.CS0047XLSMERGE()
        If isNormal(CS0047XLSMERGE.ERR) Then
            WW_Dir2 = CS0047XLSMERGE.URL                              'PARAM02:出力EXCEL
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = WW_Dir2
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        '■ 画面表示
        '○TA0001VIEWtbl取得
        TA0002VIEWtbl.Clear()
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '○ 正常終了メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '〇 TA0002VIEWtblカラム設定
        AddColumnToTA0002Tbl(TA0002VIEWtbl)

        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '帳票出力用編集
        'ＥＮＥＸの場合
        If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_ENEX Then
            EditListEnex(TA0002VIEWtbl, WW_ERRCODE)
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End If

        'ＮＪＳの場合
        If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_NJS Then
            EditListNJS(TA0002VIEWtbl, WW_ERRCODE)
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End If

        '近石の場合
        If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_KNK Then
            EditListKNK(TA0002VIEWtbl, WW_ERRCODE)
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End If

        'ＪＫＴの場合
        If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_JKT Then
            EditListJKT(TA0002VIEWtbl, WW_ERRCODE)
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End If

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0002VIEWtbl                    'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            Exit Sub
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(ZIP出力)ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonZIP_Click()

        Dim WW_Dir As String = ""
        Dim WW_TEMPDir As String = ""

        '■ 作業用フォルダ・作業用ファイルの事前操作
        Try
            '○ 作業フォルダ存在確認＆作成(C:\apple\files\TEXTWORK)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK"
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\端末名)　＆　前回処理ファイル削除
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
                'ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP)　＆　前回処理ファイル削除
            WW_TEMPDir = CS0050SESSION.UPLOAD_PATH & "\TEXTWORK\TEMP" & "\" & work.WF_SEL_HORG.Text
            If System.IO.Directory.Exists(WW_TEMPDir) Then
                'TEMPフォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_TEMPDir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_TEMPDir)
            End If

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ 全選択Excel作成（メイン処理）
        '〇 TA0001VIEWtblカラム設定
        AddColumnToTA0002Tbl(TA0002VIEWtbl)

        WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '○TA0002VIEWtbl取得
            TA0002VIEWtbl.Clear()
            GetViewTA0002(CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text)

            '帳票出力用編集
            Select Case work.WF_SEL_CAMPCODE.Text
                Case CONST_CAMP_ENEX            'ＥＮＥＸの場合
                    EditListEnex(TA0002VIEWtbl, WW_ERRCODE)
                Case CONST_CAMP_NJS             'ＮＪＳの場合
                    EditListNJS(TA0002VIEWtbl, WW_ERRCODE)
                Case CONST_CAMP_KNK             '近石の場合
                    EditListKNK(TA0002VIEWtbl, WW_ERRCODE)
                Case CONST_CAMP_JKT             'ＪＫＴの場合
                    EditListJKT(TA0002VIEWtbl, WW_ERRCODE)
            End Select
            If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            '○ 帳票出力
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = Master.MAPID                       '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0002VIEWtbl                        'データ参照DataTable
            CS0030REPORT.CS0030REPORT()
            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text & ".xlsx", True)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
                Exit Sub
            End If
        Next

        '■ ダウンロード
        '○ 圧縮実行
        WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        Dim WW_Dir2 As String = CS0050SESSION.UPLOAD_PATH & "\" & "TEXTWORK\TEMP\" & work.WF_SEL_HORG.Text
        ZipFile.CreateFromDirectory(WW_Dir, WW_Dir2 & "\ALL.zip")

        WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/TEXT/TEMP/" & work.WF_SEL_HORG.Text & "/ALL.zip"
        '○ ダウンロード処理へ遷移
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        '■ 画面表示
        '○TA0001VIEWtbl取得
        TA0002VIEWtbl.Clear()
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '○ 正常終了メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
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
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>   
    Protected Sub WF_ButtonFIRST_Click()

        '■ データリカバリ 
        '○データリカバリ 
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If
        '■ GridView表示データ作成
        AddColumnToTA0002Tbl(TA0002VIEWtbl)
        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)
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
        '○データリカバリ 
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ GridView表示データ作成
        AddColumnToTA0002Tbl(TA0002VIEWtbl)
        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(TA0002VIEWtbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '■ GridView表示
        '○ 最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub

    ''' <summary>
    ''' detailbox 戻るボタン処理  
    ''' </summary>
    Protected Sub WF_BACK_Click()

        '○データリカバリ
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ GridView表示データ作成

        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        'pnlListArea.Visible = True

        work.WF_IsHideDetailBox.Text = "1"

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' TA0002全表示データ取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetAllTA0002()

        '■ 画面表示用データ取得
        If IsNothing(TA0002ALL) Then TA0002ALL = New DataTable

        '■ 画面表示用データ取得

        'TA0002テンポラリDB項目作成
        AddColumnToTA0002Tbl(TA0002ALL)

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'テンポラリーテーブルを作成する
                Dim SQLStr0 As String = "CREATE TABLE #MBtemp " _
                        & " ( " _
                        & "  CAMPCODE nvarchar(20)," _
                        & "  STAFFCODE nvarchar(20)," _
                        & "  HORG nvarchar(20)," _
                        & " ) "

                '検索SQL文（乗務員のみ）
                Dim SQLStr1 As String = " SELECT  isnull(rtrim(MB1.CAMPCODE),'')      as  CAMPCODE, " _
                       & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE " _
                       & " from   MB001_STAFF MB1 " _
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
                       & "   ON    Z.CODE      = Y.CODE " _
                       & "  and    Z.CODE      = MB1.HORG " _
                       & " where  MB1.CAMPCODE    =  @CAMPCODE " _
                       & "   and  MB1.STAFFKBN   like '03%' " _
                       & "   and  MB1.STYMD      <=  @SEL_ENDYMD " _
                       & "   and  MB1.ENDYMD     >=  @SEL_STYMD " _
                       & "   and  MB1.DELFLG     <>  '1' " _
                       & " group by MB1.CAMPCODE, MB1.STAFFCODE "


                '検索SQL文     ※乗務員照会も可能にするため、ユーザ権限チェックは行わない。
                Dim SQLStr As String =
                 " SELECT * FROM ( " _
               & " SELECT 0 as LINECNT , " _
               & "       '' as OPERATION , " _
               & "       '1' as HIDDEN , " _
               & "       TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint) , " _
               & "       isnull(rtrim(A.CAMPCODE),'')  as CAMPCODE, " _
               & "       ''  as CAMPNAMES, " _
               & "       @TAISHOYM as TAISHOYM , " _
               & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE, " _
               & "       isnull(rtrim(MB2.STAFFNAMES),'') as STAFFNAMES , " _
               & "       isnull(rtrim(A.WORKDATE),'') as WORKDATE , " _
               & "       isnull(rtrim(CAL.WORKINGWEEK),'0') as WORKINGWEEK , " _
               & "       isnull(rtrim(F1.VALUE1),'') as WORKINGWEEKNAMES , " _
               & "       isnull(rtrim(A.HDKBN),'H') as HDKBN , " _
               & "       isnull(rtrim(A.RECODEKBN),'0') as RECODEKBN , " _
               & "       isnull(rtrim(F3.VALUE1),'') as RECODEKBNNAMES , " _
               & "       isnull(A.SEQ,'0') as SEQ , " _
               & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
               & "       isnull(rtrim(A.NIPPOLINKCODE),'') as NIPPOLINKCODE , " _
               & "       isnull(rtrim(A.MORG),'') as MORG , " _
               & "       '' as MORGNAMES , " _
               & "       isnull(rtrim(A.HORG),'') as HORG , " _
               & "       '' as HORGNAMES , " _
               & "       isnull(rtrim(A.SORG),'') as SORG , " _
               & "       '' as SORGNAMES , " _
               & "       isnull(rtrim(A.STAFFKBN),'') as STAFFKBN , " _
               & "       isnull(rtrim(F8.VALUE1),'') as STAFFKBNNAMES , " _
               & "       isnull(rtrim(A.HOLIDAYKBN),'') as HOLIDAYKBN , " _
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
               & "       isnull(A.BINDTIME,'0') as BINDTIME , " _
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
               & "       0 as JIDISTANCE , " _
               & "       0 as KUDISTANCE , " _
               & "       isnull(A.HAIDISTANCE,0) as HAIDISTANCE , " _
               & "       isnull(A.HAIDISTANCECHO,0) as HAIDISTANCECHO , " _
               & "       isnull(A.HAIDISTANCE,0) + isnull(A.HAIDISTANCECHO, 0) as HAIDISTANCETTL , " _
               & "       isnull(A.KAIDISTANCE,0) as KAIDISTANCE , " _
               & "       isnull(A.KAIDISTANCECHO,0) as KAIDISTANCECHO , " _
               & "       isnull(A.KAIDISTANCE, 0) + isnull(A.KAIDISTANCECHO, 0) as KAIDISTANCETTL , " _
               & "       isnull(A.UNLOADCNT,0) as UNLOADCNT , " _
               & "       isnull(A.UNLOADCNTCHO,0) as UNLOADCNTCHO , " _
               & "       isnull(A.UNLOADCNT, 0) + isnull(A.UNLOADCNTCHO, 0) as UNLOADCNTTTL , " _
               & "       isnull(rtrim(A.DELFLG),'') as DELFLG , " _
               & "       '' as ORVER15 , " _
               & "       '' as ORVER09 , " _
               & "       '' as RYOME , " _
               & "       '' as PRODUCT1 , " _
               & "       '' as PRODUCT1NAMES , " _
               & "       '' as SHUKOTIME , " _
               & "       '' as KIKOTIME , " _
               & "       '' as HANDLETIME , " _
               & "       '' as TRIPNO , " _
               & "       '' as SURYO , " _
               & "       'K' as DATAKBN , " _
               & "       isnull(A.HAISOTIME, 0) as HAISOTIME , " _
               & "       isnull(A.NENMATUNISSU, 0) as NENMATUNISSU , " _
               & "       isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUCHO , " _
               & "       isnull(A.NENMATUNISSU, 0) + isnull(A.NENMATUNISSUCHO, 0) as NENMATUNISSUTTL , " _
               & "       isnull(A.SHACHUHAKKBN, 0) as SHACHUHAKKBN , " _
               & "       '' as SHACHUHAKKBNNAMES , " _
               & "       isnull(A.SHACHUHAKNISSU, 0) as SHACHUHAKNISSU , " _
               & "       isnull(A.SHACHUHAKNISSUCHO, 0) as SHACHUHAKNISSUCHO , " _
               & "       isnull(A.SHACHUHAKNISSU, 0) + isnull(A.SHACHUHAKNISSUCHO, 0) as SHACHUHAKNISSUTTL , " _
               & "       isnull(A.JIKYUSHATIME, 0) as JIKYUSHATIME , " _
               & "       isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMECHO , " _
               & "       isnull(A.JIKYUSHATIME, 0) + isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMETTL , " _
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
               & "       isnull(A.KAITENCNT, 0) as KAITENCNT , " _
               & "       isnull(A.KAITENCNTCHO, 0) as KAITENCNTCHO , " _
               & "       isnull(A.KAITENCNT, 0) + isnull(A.KAITENCNTCHO, 0) as KAITENCNTTTL , " _
               & "       isnull(A.HDAIWORKTIME, 0) as HDAIWORKTIME , " _
               & "       isnull(A.HDAIWORKTIMECHO, 0) as HDAIWORKTIMECHO , " _
               & "       isnull(A.HDAIWORKTIME, 0) + isnull(A.HDAIWORKTIMECHO, 0) as HDAIWORKTIMETTL , " _
               & "       isnull(A.HDAINIGHTTIME, 0) as HDAINIGHTTIME , " _
               & "       isnull(A.HDAINIGHTTIMECHO, 0) as HDAINIGHTTIMECHO , " _
               & "       isnull(A.HDAINIGHTTIME, 0) + isnull(A.HDAINIGHTTIMECHO, 0) as HDAINIGHTTIMETTL , " _
               & "       isnull(A.SDAIWORKTIME, 0) as SDAIWORKTIME , " _
               & "       isnull(A.SDAIWORKTIMECHO, 0) as SDAIWORKTIMECHO , " _
               & "       isnull(A.SDAIWORKTIME, 0) + isnull(A.SDAIWORKTIMECHO, 0) as SDAIWORKTIMETTL , " _
               & "       isnull(A.SDAINIGHTTIME, 0) as SDAINIGHTTIME , " _
               & "       isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMECHO , " _
               & "       isnull(A.SDAINIGHTTIME, 0) + isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMETTL , " _
               & "       isnull(A.WWORKTIME, 0) as WWORKTIME , " _
               & "       isnull(A.WWORKTIMECHO, 0) as WWORKTIMECHO , " _
               & "       isnull(A.WWORKTIME, 0) + isnull(A.WWORKTIMECHO, 0) as WWORKTIMETTL , " _
               & "       isnull(A.JYOMUTIME, 0) as JYOMUTIME , " _
               & "       isnull(A.JYOMUTIMECHO, 0) as JYOMUTIMECHO , " _
               & "       isnull(A.JYOMUTIME, 0) + isnull(A.JYOMUTIMECHO, 0) as JYOMUTIMETTL , " _
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
               & "       isnull(MB3.SEQ, 0) as ORGSEQ " _
               & " FROM #MBtemp MB " _
               & " INNER JOIN T0007_KINTAI A " _
               & "   ON    A.CAMPCODE     = @CAMPCODE " _
               & "   and   A.TAISHOYM     = @TAISHOYM " _
               & "   and   A.STAFFCODE    = MB.STAFFCODE " _
               & "   and   A.RECODEKBN    = '0' " _
               & "   and   A.DELFLG      <> '1' " _
               & " LEFT JOIN MB001_STAFF MB2 " _
               & "   ON    MB2.CAMPCODE     = @CAMPCODE " _
               & "   and   MB2.STAFFCODE    = MB.STAFFCODE " _
               & "   and   MB2.STYMD       <= A.WORKDATE " _
               & "   and   MB2.ENDYMD      >= A.WORKDATE " _
               & "   and   MB2.STYMD        = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @CAMPCODE and STAFFCODE = MB.STAFFCODE and STYMD <= A.WORKDATE and ENDYMD >= A.WORKDATE and DELFLG <> '1' ) " _
               & "   and   MB2.DELFLG      <> '1' " _
               & " LEFT JOIN MB002_STAFFORG MB3 " _
               & "   ON    MB3.CAMPCODE     = @CAMPCODE " _
               & "   and   MB3.STAFFCODE    = MB2.STAFFCODE " _
               & "   and   MB3.SORG         = MB2.HORG " _
               & "   and   MB3.DELFLG      <> '1' " _
               & " LEFT JOIN MB005_CALENDAR CAL " _
               & "   ON    CAL.CAMPCODE    = @CAMPCODE " _
               & "   and   CAL.WORKINGYMD  = A.WORKDATE " _
               & "   and   CAL.DELFLG     <> '1' " _
               & " LEFT JOIN MB004_WORKINGH B4 " _
               & "   ON    B4.CAMPCODE    = @CAMPCODE " _
               & "   and   B4.HORG        = MB2.HORG " _
               & "   and   B4.STAFFKBN    = MB2.STAFFKBN " _
               & "   and   B4.STYMD      <= @STYMD " _
               & "   and   B4.ENDYMD     >= @ENDYMD " _
               & "   and   B4.STYMD      = (SELECT MAX(STYMD) FROM MB004_WORKINGH WHERE CAMPCODE = @CAMPCODE and HORG = MB2.HORG and STAFFKBN = MB2.STAFFKBN and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1') " _
               & "   and   B4.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F1 " _
               & "   ON    F1.CAMPCODE    = @CAMPCODE " _
               & "   and   F1.CLASS       = 'WORKINGWEEK' " _
               & "   and   F1.KEYCODE     = CAL.WORKINGWEEK " _
               & "   and   F1.STYMD      <= @STYMD " _
               & "   and   F1.ENDYMD     >= @ENDYMD " _
               & "   and   F1.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F3 " _
               & "   ON    F3.CAMPCODE    = @CAMPCODE " _
               & "   and   F3.CLASS       = 'RECODEKBN' " _
               & "   and   F3.KEYCODE     = A.RECODEKBN " _
               & "   and   F3.STYMD      <= @STYMD " _
               & "   and   F3.ENDYMD     >= @ENDYMD " _
               & "   and   F3.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F4 " _
               & "   ON    F4.CAMPCODE    = @CAMPCODE " _
               & "   and   F4.CLASS       = 'WORKKBN' " _
               & "   and   F4.KEYCODE     = A.WORKKBN " _
               & "   and   F4.STYMD      <= @STYMD " _
               & "   and   F4.ENDYMD     >= @ENDYMD " _
               & "   and   F4.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F6 " _
               & "   ON    F6.CAMPCODE    = @CAMPCODE " _
               & "   and   F6.CLASS       = 'SHARYOKBN' " _
               & "   and   F6.KEYCODE     = A.SHARYOKBN " _
               & "   and   F6.STYMD      <= @STYMD " _
               & "   and   F6.ENDYMD     >= @ENDYMD " _
               & "   and   F6.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F7 " _
               & "   ON    F7.CAMPCODE    = @CAMPCODE " _
               & "   and   F7.CLASS       = 'OILPAYKBN' " _
               & "   and   F7.KEYCODE     = A.OILPAYKBN " _
               & "   and   F7.STYMD      <= @STYMD " _
               & "   and   F7.ENDYMD     >= @ENDYMD " _
               & "   and   F7.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F8 " _
               & "   ON    F8.CAMPCODE    = @CAMPCODE " _
               & "   and   F8.CLASS       = 'STAFFKBN' " _
               & "   and   F8.KEYCODE     = MB2.STAFFKBN " _
               & "   and   F8.STYMD      <= @STYMD " _
               & "   and   F8.ENDYMD     >= @ENDYMD " _
               & "   and   F8.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F9 " _
               & "   ON    F9.CAMPCODE    = @CAMPCODE " _
               & "   and   F9.CLASS       = 'HOLIDAYKBN' " _
               & "   and   F9.KEYCODE     = A.HOLIDAYKBN " _
               & "   and   F9.STYMD      <= @STYMD " _
               & "   and   F9.ENDYMD     >= @ENDYMD " _
               & "   and   F9.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F10 " _
               & "   ON    F10.CAMPCODE    = @CAMPCODE " _
               & "   and   F10.CLASS       = 'PAYKBN' " _
               & "   and   F10.KEYCODE     = A.PAYKBN " _
               & "   and   F10.STYMD      <= @STYMD " _
               & "   and   F10.ENDYMD     >= @ENDYMD " _
               & "   and   F10.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F11 " _
               & "   ON    F11.CAMPCODE    = @CAMPCODE " _
               & "   and   F11.CLASS       = 'SHUKCHOKKBN' " _
               & "   and   F11.KEYCODE     = A.SHUKCHOKKBN " _
               & "   and   F11.STYMD      <= @STYMD " _
               & "   and   F11.ENDYMD     >= @ENDYMD " _
               & "   and   F11.DELFLG     <> '1' " _
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
                If work.WF_SEL_STAFFKBN.Text <> Nothing Then
                    SQLWhere = SQLWhere & " and STAFFKBN = '" & Trim(work.WF_SEL_STAFFKBN.Text) & "' "
                End If
                If work.WF_SEL_STAFFCODE.Text <> Nothing Then
                    SQLWhere = SQLWhere & " and STAFFCODE = '" & Trim(work.WF_SEL_STAFFCODE.Text) & "' "
                End If
                If work.WF_SEL_STAFFNAME.Text <> Nothing Then
                    SQLWhere = SQLWhere & " and STAFFNAMES like '%" & Trim(work.WF_SEL_STAFFNAME.Text) & "%' "
                End If

                Dim WW_SORT As String = "ORDER BY STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, HDKBN DESC"

                SQLStr = SQLStr & SQLWhere & WW_SORT

                Using SQLcmd1 As New SqlCommand(SQLStr0, SQLcon), SQLcmd2 As New SqlCommand(SQLStr1, SQLcon), SQLcmd As New SqlCommand(SQLStr, SQLcon)

                    Dim P2_CAMPCODE As SqlParameter = SQLcmd2.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim P2_SEL_STYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P2_SEL_ENDYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
                    Dim P2_HORG As SqlParameter = SQLcmd2.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
                    Dim P2_TERMID As SqlParameter = SQLcmd2.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar)
                    Dim P2_NOW As SqlParameter = SQLcmd2.Parameters.Add("@NOW", System.Data.SqlDbType.Date)

                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_STYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P_SEL_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)

                    '〇ワークテーブル作成
                    SQLcmd1.CommandTimeout = 300
                    SQLcmd1.ExecuteNonQuery()


                    P2_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P2_SEL_STYMD.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                    Dim wDATE2 As Date
                    Try
                        wDATE2 = work.WF_SEL_TAISHOYM.Text & "/01"
                    Catch ex As Exception
                        wDATE2 = Date.Now
                    End Try
                    P2_SEL_ENDYMD.Value = work.WF_SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE2.Year, wDATE2.Month).ToString("00")
                    Dim orgCode As String = ""
                    Dim retCode As String = ""
                    T0007COM.ConvORGCODE(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, orgCode, retCode)
                    If retCode = C_MESSAGE_NO.NORMAL Then
                        P2_HORG.Value = orgCode
                    Else
                        P2_HORG.Value = work.WF_SEL_HORG.Text
                    End If
                    P2_TERMID.Value = CS0050SESSION.APSV_ID

                    P2_NOW.Value = Date.Now

                    Using SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader(), WW_MBtbl As DataTable = New DataTable
                        WW_MBtbl.Columns.Add("CAMPCODE", GetType(String))
                        WW_MBtbl.Columns.Add("STAFFCODE", GetType(String))

                        WW_MBtbl.Load(SQLdr2)

                        '一旦テンポラリテーブルに出力
                        Using bc As New SqlClient.SqlBulkCopy(SQLcon)
                            bc.DestinationTableName = "#MBtemp"
                            bc.WriteToServer(WW_MBtbl)
                        End Using
                    End Using

                    P_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P_TAISHOYM.Value = work.WF_SEL_TAISHOYM.Text
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_SEL_STYMD.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                    Dim wDATE As Date
                    Try
                        wDATE = work.WF_SEL_TAISHOYM.Text & "/01"
                    Catch ex As Exception
                        wDATE = Date.Now
                    End Try
                    P_SEL_ENDYMD.Value = work.WF_SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()


                        '■テーブル検索結果をテーブル格納
                        TA0002ALL.Load(SQLdr)
                    End Using

                    For Each TA0002ALLrow As DataRow In TA0002ALL.Rows

                        Dim WW_LINEcnt As Integer = 0
                        TA0002ALLrow("SELECT") = "1"
                        If TA0002ALLrow("HDKBN") = "H" Then
                            TA0002ALLrow("HIDDEN") = "0"      '表示
                            WW_LINEcnt += 1
                            TA0002ALLrow("LINECNT") = WW_LINEcnt
                        Else
                            TA0002ALLrow("HIDDEN") = "1"      '非表示
                            TA0002ALLrow("LINECNT") = 0
                        End If

                        TA0002ALLrow("SEQ") = CInt(TA0002ALLrow("SEQ")).ToString("000")
                        If IsDate(TA0002ALLrow("WORKDATE")) Then
                            TA0002ALLrow("WORKDATE") = CDate(TA0002ALLrow("WORKDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0002ALLrow("WORKDATE") = ""
                        End If
                        If IsDate(TA0002ALLrow("STDATE")) Then
                            TA0002ALLrow("STDATE") = CDate(TA0002ALLrow("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0002ALLrow("STDATE") = ""
                        End If
                        If IsDate(TA0002ALLrow("STTIME")) Then
                            TA0002ALLrow("STTIME") = ZeroToSpace(CDate(TA0002ALLrow("STTIME")).ToString("HH:mm"))
                        Else
                            TA0002ALLrow("STTIME") = ""
                        End If
                        If IsDate(TA0002ALLrow("ENDDATE")) Then
                            TA0002ALLrow("ENDDATE") = CDate(TA0002ALLrow("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            TA0002ALLrow("ENDDATE") = ""
                        End If
                        If IsDate(TA0002ALLrow("ENDTIME")) Then
                            TA0002ALLrow("ENDTIME") = ZeroToSpace(CDate(TA0002ALLrow("ENDTIME")).ToString("HH:mm"))
                        Else
                            TA0002ALLrow("ENDTIME") = ""
                        End If
                        If IsDate(TA0002ALLrow("BINDSTDATE")) Then
                            TA0002ALLrow("BINDSTDATE") = ZeroToSpace(CDate(TA0002ALLrow("BINDSTDATE")).ToString("HH:mm"))
                        Else
                            TA0002ALLrow("BINDSTDATE") = ""
                        End If

                        If TA0002ALLrow("STDATE") <> TA0002ALLrow("ENDDATE") Then
                            TA0002ALLrow("ENDDATE_TXT") = "翌"
                        Else
                            TA0002ALLrow("ENDDATE_TXT") = ""
                        End If

                        If TA0002ALLrow("SHACHUHAKKBN") = "1" Then
                            TA0002ALLrow("SHACHUHAKKBNNAMES") = "✔"
                        Else
                            TA0002ALLrow("SHACHUHAKKBNNAMES") = ""
                        End If
                        TA0002ALLrow("SHACHUHAKKBN") = ZeroToSpace(TA0002ALLrow("SHACHUHAKKBN"))

                        TA0002ALLrow("WORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WORKTIME")))
                        TA0002ALLrow("MOVETIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("MOVETIME")))
                        TA0002ALLrow("ACTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("ACTTIME")))
                        If (TA0002ALLrow("STTIME") = "" AndAlso TA0002ALLrow("ENDTIME") = "") Then
                            TA0002ALLrow("BINDTIME") = ""
                        Else
                            TA0002ALLrow("BINDTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("BINDTIME")))
                        End If
                        TA0002ALLrow("NIPPOBREAKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("NIPPOBREAKTIME")))
                        TA0002ALLrow("BREAKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("BREAKTIME")))
                        TA0002ALLrow("BREAKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("BREAKTIMECHO")))
                        TA0002ALLrow("BREAKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("BREAKTIMETTL")))
                        TA0002ALLrow("NIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("NIGHTTIME")))
                        TA0002ALLrow("NIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("NIGHTTIMECHO")))
                        TA0002ALLrow("NIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("NIGHTTIMETTL")))
                        TA0002ALLrow("ORVERTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("ORVERTIME")))
                        TA0002ALLrow("ORVERTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("ORVERTIMECHO")))
                        TA0002ALLrow("ORVERTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("ORVERTIMETTL")))
                        TA0002ALLrow("WNIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WNIGHTTIME")))
                        TA0002ALLrow("WNIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WNIGHTTIMECHO")))
                        TA0002ALLrow("WNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WNIGHTTIMETTL")))
                        TA0002ALLrow("SWORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SWORKTIME")))
                        TA0002ALLrow("SWORKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SWORKTIMECHO")))
                        TA0002ALLrow("SWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SWORKTIMETTL")))
                        TA0002ALLrow("SNIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SNIGHTTIME")))
                        TA0002ALLrow("SNIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SNIGHTTIMECHO")))
                        TA0002ALLrow("SNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SNIGHTTIMETTL")))
                        TA0002ALLrow("HWORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HWORKTIME")))
                        TA0002ALLrow("HWORKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HWORKTIMECHO")))
                        TA0002ALLrow("HWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HWORKTIMETTL")))
                        TA0002ALLrow("HNIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HNIGHTTIME")))
                        TA0002ALLrow("HNIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HNIGHTTIMECHO")))
                        TA0002ALLrow("HNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HNIGHTTIMETTL")))
                        TA0002ALLrow("HOANTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HOANTIME")))
                        TA0002ALLrow("HOANTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HOANTIMECHO")))
                        TA0002ALLrow("HOANTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HOANTIMETTL")))
                        TA0002ALLrow("KOATUTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("KOATUTIME")))
                        TA0002ALLrow("KOATUTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("KOATUTIMECHO")))
                        TA0002ALLrow("KOATUTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("KOATUTIMETTL")))
                        TA0002ALLrow("TOKUSA1TIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("TOKUSA1TIME")))
                        TA0002ALLrow("TOKUSA1TIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("TOKUSA1TIMECHO")))
                        TA0002ALLrow("TOKUSA1TIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("TOKUSA1TIMETTL")))
                        TA0002ALLrow("HAYADETIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HAYADETIME")))
                        TA0002ALLrow("HAYADETIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HAYADETIMECHO")))
                        TA0002ALLrow("HAYADETIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HAYADETIMETTL")))
                        TA0002ALLrow("HAISOTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HAISOTIME")))
                        TA0002ALLrow("JIKYUSHATIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JIKYUSHATIME")))
                        TA0002ALLrow("JIKYUSHATIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JIKYUSHATIMECHO")))
                        TA0002ALLrow("JIKYUSHATIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JIKYUSHATIMETTL")))

                        TA0002ALLrow("HDAIWORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAIWORKTIME")))
                        TA0002ALLrow("HDAIWORKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAIWORKTIMECHO")))
                        TA0002ALLrow("HDAIWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAIWORKTIMETTL")))
                        TA0002ALLrow("HDAINIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAINIGHTTIME")))
                        TA0002ALLrow("HDAINIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAINIGHTTIMECHO")))
                        TA0002ALLrow("HDAINIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("HDAINIGHTTIMETTL")))
                        TA0002ALLrow("SDAIWORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAIWORKTIME")))
                        TA0002ALLrow("SDAIWORKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAIWORKTIMECHO")))
                        TA0002ALLrow("SDAIWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAIWORKTIMETTL")))
                        TA0002ALLrow("SDAINIGHTTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAINIGHTTIME")))
                        TA0002ALLrow("SDAINIGHTTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAINIGHTTIMECHO")))
                        TA0002ALLrow("SDAINIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("SDAINIGHTTIMETTL")))
                        TA0002ALLrow("WWORKTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WWORKTIME")))
                        TA0002ALLrow("WWORKTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WWORKTIMECHO")))
                        TA0002ALLrow("WWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("WWORKTIMETTL")))
                        TA0002ALLrow("JYOMUTIME") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JYOMUTIME")))
                        TA0002ALLrow("JYOMUTIMECHO") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JYOMUTIMECHO")))
                        TA0002ALLrow("JYOMUTIMETTL") = ZeroToSpace(MinituesToHHMM(TA0002ALLrow("JYOMUTIMETTL")))

                        TA0002ALLrow("JIDISTANCE") = Val(TA0002ALLrow("JIDISTANCE"))
                        TA0002ALLrow("KUDISTANCE") = Val(TA0002ALLrow("KUDISTANCE"))
                        TA0002ALLrow("HAIDISTANCE") = Val(TA0002ALLrow("HAIDISTANCE"))
                        TA0002ALLrow("HAIDISTANCECHO") = Val(TA0002ALLrow("HAIDISTANCECHO"))
                        TA0002ALLrow("HAIDISTANCETTL") = Val(TA0002ALLrow("HAIDISTANCETTL"))
                        TA0002ALLrow("KAIDISTANCE") = Val(TA0002ALLrow("KAIDISTANCE"))
                        TA0002ALLrow("KAIDISTANCETTL") = Val(TA0002ALLrow("KAIDISTANCETTL"))

                        TA0002ALLrow("RYOME") = TA0002ALLrow("RYOME")
                        TA0002ALLrow("PRODUCT1") = TA0002ALLrow("PRODUCT1")
                        TA0002ALLrow("PRODUCT1NAMES") = TA0002ALLrow("PRODUCT1NAMES")
                        TA0002ALLrow("SHUKOTIME") = TA0002ALLrow("SHUKOTIME")
                        TA0002ALLrow("KIKOTIME") = TA0002ALLrow("KIKOTIME")
                        TA0002ALLrow("HANDLETIME") = TA0002ALLrow("HANDLETIME")
                        TA0002ALLrow("TRIPNO") = TA0002ALLrow("TRIPNO")
                        TA0002ALLrow("SURYO") = TA0002ALLrow("SURYO")

                        '名前の取得
                        TA0002ALLrow("CAMPNAMES") = ""
                        CodeToName("CAMPCODE", TA0002ALLrow("CAMPCODE"), TA0002ALLrow("CAMPNAMES"), WW_DUMMY)
                        If TA0002ALLrow("STAFFNAMES") = "" Then
                            TA0002ALLrow("STAFFNAMES") = ""
                            CodeToName("STAFFCODE", TA0002ALLrow("STAFFCODE"), TA0002ALLrow("STAFFNAMES"), WW_DUMMY)
                        End If
                        TA0002ALLrow("MORGNAMES") = ""
                        CodeToName("ORG", TA0002ALLrow("MORG"), TA0002ALLrow("MORGNAMES"), WW_DUMMY)

                        If TA0002ALLrow("HORG") = "" Then
                            TA0002ALLrow("HORG") = work.WF_SEL_HORG.Text
                            TA0002ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0002ALLrow("HORG"), TA0002ALLrow("HORGNAMES"), WW_DUMMY)
                        Else
                            TA0002ALLrow("HORGNAMES") = ""
                            CodeToName("ORG", TA0002ALLrow("HORG"), TA0002ALLrow("HORGNAMES"), WW_DUMMY)
                        End If

                        If TA0002ALLrow("SORG") = "" Then
                            TA0002ALLrow("SORG") = TA0002ALLrow("HORG")
                        End If
                        TA0002ALLrow("SORGNAMES") = ""
                        CodeToName("ORG", TA0002ALLrow("SORG"), TA0002ALLrow("SORGNAMES"), WW_DUMMY)


                        '○表示項目編集
                        If TA0002ALLrow("CAMPNAMES") = Nothing AndAlso TA0002ALLrow("CAMPCODE") = Nothing Then
                            TA0002ALLrow("CAMPCODE_TXT") = ""
                        Else
                            TA0002ALLrow("CAMPCODE_TXT") = TA0002ALLrow("CAMPNAMES") & " (" & TA0002ALLrow("CAMPCODE") & ")"
                        End If

                        TA0002ALLrow("TAISHOYM_TXT") = TA0002ALLrow("TAISHOYM")

                        If TA0002ALLrow("STAFFNAMES") = Nothing AndAlso TA0002ALLrow("STAFFCODE") = Nothing Then
                            TA0002ALLrow("STAFFCODE_TXT") = ""
                        Else
                            TA0002ALLrow("STAFFCODE_TXT") = TA0002ALLrow("STAFFNAMES") & " (" & TA0002ALLrow("STAFFCODE") & ")"
                        End If

                        If IsDate(TA0002ALLrow("WORKDATE")) Then
                            TA0002ALLrow("WORKDATE_TXT") = CDate(TA0002ALLrow("WORKDATE")).ToString("dd")
                        Else
                            TA0002ALLrow("WORKDATE_TXT") = ""
                        End If

                        If TA0002ALLrow("WORKINGWEEKNAMES") = Nothing Then
                            TA0002ALLrow("WORKINGWEEK_TXT") = ""
                        Else
                            TA0002ALLrow("WORKINGWEEK_TXT") = TA0002ALLrow("WORKINGWEEKNAMES")
                        End If

                        TA0002ALLrow("HDKBN_TXT") = TA0002ALLrow("HDKBN")

                        If TA0002ALLrow("RECODEKBNNAMES") = Nothing AndAlso TA0002ALLrow("RECODEKBN") = Nothing Then
                            TA0002ALLrow("RECODEKBN_TXT") = ""
                        Else
                            TA0002ALLrow("RECODEKBN_TXT") = TA0002ALLrow("RECODEKBNNAMES") & " (" & TA0002ALLrow("RECODEKBN") & ")"
                        End If

                        If TA0002ALLrow("WORKKBNNAMES") = Nothing AndAlso TA0002ALLrow("WORKKBN") = Nothing Then
                            TA0002ALLrow("WORKKBN_TXT") = ""
                        Else
                            TA0002ALLrow("WORKKBN_TXT") = TA0002ALLrow("WORKKBNNAMES") & " (" & TA0002ALLrow("WORKKBN") & ")"
                        End If

                        If TA0002ALLrow("SHARYOKBNNAMES") = Nothing AndAlso TA0002ALLrow("SHARYOKBN") = Nothing Then
                            TA0002ALLrow("SHARYOKBN_TXT") = ""
                        Else
                            TA0002ALLrow("SHARYOKBN_TXT") = TA0002ALLrow("SHARYOKBNNAMES") & " (" & TA0002ALLrow("SHARYOKBN") & ")"
                        End If

                        If TA0002ALLrow("OILPAYKBNNAMES") = Nothing AndAlso TA0002ALLrow("OILPAYKBN") = Nothing Then
                            TA0002ALLrow("OILPAYKBN_TXT") = ""
                        Else
                            TA0002ALLrow("OILPAYKBN_TXT") = TA0002ALLrow("OILPAYKBNNAMES") & " (" & TA0002ALLrow("OILPAYKBN") & ")"
                        End If

                        If TA0002ALLrow("STAFFKBNNAMES") = Nothing AndAlso TA0002ALLrow("STAFFKBN") = Nothing Then
                            TA0002ALLrow("STAFFKBN_TXT") = ""
                        Else
                            TA0002ALLrow("STAFFKBN_TXT") = TA0002ALLrow("STAFFKBNNAMES") & " (" & TA0002ALLrow("STAFFKBN") & ")"
                        End If

                        If TA0002ALLrow("MORGNAMES") = Nothing AndAlso TA0002ALLrow("MORG") = Nothing Then
                            TA0002ALLrow("MORG_TXT") = ""
                        Else
                            TA0002ALLrow("MORG_TXT") = TA0002ALLrow("MORGNAMES") & " (" & TA0002ALLrow("MORG") & ")"
                        End If

                        If TA0002ALLrow("HORGNAMES") = Nothing AndAlso TA0002ALLrow("HORG") = Nothing Then
                            TA0002ALLrow("HORG_TXT") = ""
                        Else
                            TA0002ALLrow("HORG_TXT") = TA0002ALLrow("HORGNAMES") & " (" & TA0002ALLrow("HORG") & ")"
                        End If

                        If TA0002ALLrow("SORGNAMES") = Nothing AndAlso TA0002ALLrow("SORG") = Nothing Then
                            TA0002ALLrow("SORG_TXT") = ""
                        Else
                            TA0002ALLrow("SORG_TXT") = TA0002ALLrow("SORGNAMES") & " (" & TA0002ALLrow("SORG") & ")"
                        End If

                        If TA0002ALLrow("HOLIDAYKBNNAMES") = Nothing AndAlso TA0002ALLrow("HOLIDAYKBN") = Nothing Then
                            TA0002ALLrow("HOLIDAYKBN_TXT") = ""
                        Else
                            TA0002ALLrow("HOLIDAYKBN_TXT") = TA0002ALLrow("HOLIDAYKBNNAMES") & " (" & TA0002ALLrow("HOLIDAYKBN") & ")"
                        End If

                        If TA0002ALLrow("PAYKBNNAMES") = Nothing AndAlso TA0002ALLrow("PAYKBN") = Nothing Then
                            TA0002ALLrow("PAYKBN_TXT") = ""
                        Else
                            TA0002ALLrow("PAYKBN_TXT") = TA0002ALLrow("PAYKBNNAMES") & " (" & TA0002ALLrow("PAYKBN") & ")"
                        End If

                        If TA0002ALLrow("SHUKCHOKKBNNAMES") = Nothing AndAlso TA0002ALLrow("SHUKCHOKKBN") = Nothing Then
                            TA0002ALLrow("SHUKCHOKKBN_TXT") = ""
                        Else
                            TA0002ALLrow("SHUKCHOKKBN_TXT") = TA0002ALLrow("SHUKCHOKKBNNAMES") & " (" & TA0002ALLrow("SHUKCHOKKBN") & ")"
                        End If

                        TA0002ALLrow("DELFLG_TXT") = TA0002ALLrow("DELFLG")

                        TA0002ALLrow("PRODUCT1_TXT") = TA0002ALLrow("PRODUCT1")
                        If TA0002ALLrow("PRODUCT1NAMES") = Nothing AndAlso TA0002ALLrow("PRODUCT1") = Nothing Then
                            TA0002ALLrow("PRODUCT1_TXT") = ""
                        Else
                            TA0002ALLrow("PRODUCT1_TXT") = TA0002ALLrow("PRODUCT1NAMES") & " (" & TA0002ALLrow("PRODUCT1") & ")"
                        End If

                    Next

                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0007_KINTAI Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'ソート＆データ抽出
        CS0026TblSort.TABLE = TA0002ALL
        CS0026TblSort.SORTING = "LINECNT, SEQ"
        CS0026TblSort.FILTER = "SELECT = 1"
        TA0002ALL = CS0026TblSort.sort()
    End Sub

    ''' <summary>
    ''' TA0002VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <param name="I_CODE">条件コード</param>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0002(ByVal I_CODE As String)

        '〇 TA0002ALLよりデータ抽出
        CS0026TblSort.TABLE = TA0002ALL
        CS0026TblSort.SORTING = "LINECNT , SEQ ASC"
        CS0026TblSort.FILTER = "STAFFCODE = '" & I_CODE & "'"
        TA0002VIEWtbl = CS0026TblSort.sort()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0002VIEWrow As DataRow In TA0002VIEWtbl.Rows
            TA0002VIEWrow("LINECNT") = 0
        Next

        For Each TA0002VIEWrow As DataRow In TA0002VIEWtbl.Rows
            If TA0002VIEWrow("LINECNT") = 0 AndAlso TA0002VIEWrow("HDKBN") = "H" Then
                TA0002VIEWrow("SELECT") = "1"
                TA0002VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0002VIEWrow("LINECNT") = WW_LINECNT
            End If
        Next

    End Sub

    ''' <summary>
    ''' 帳票用編集（エネックス）
    ''' </summary>
    ''' <param name="IO_TBL">編集テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Protected Sub EditListEnex(ByRef IO_TBL As DataTable, ByRef O_RTN As String)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0002tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0002row As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0002row = WW_TA0002tbl.NewRow
            WW_TA0002row.ItemArray = IO_TBL.Rows(i).ItemArray

            If WW_TA0002row("HDKBN") <> "H" Then Continue For

            If WW_TA0002row("RECODEKBN") = "0" Then

                '--------------------------------------
                '日報項目編集（日報が存在する場合の編集）
                '--------------------------------------
                GetT00005(WW_TA0002row("WORKDATE"), WW_TA0002row("WORKDATE"), WW_TA0002row("STAFFCODE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                Dim WW_F1CNT As Integer = 0
                Dim WW_F3CNT As Integer = 0
                Dim WW_KAIJI As Integer = 0
                Dim WW_B3CNT As Integer = 0
                Dim WW_SHUKOTIME As String = ""
                Dim WW_KIKOTIME As String = ""
                Dim WW_GSHABAN As String = ""
                Dim WW_PRODUCT1 As String = ""
                Dim WW_PRODUCT1NAME As String = ""
                Dim WW_SURYO As Double = 0
                Dim WW_DISTANCE As Double = 0
                Dim WW_MOVETIME As Integer = 0
                Dim WW_BREAKTIME As Integer = 0
                Dim WW_TRIPNO As String = ""

                For Each T0005row As DataRow In T0005tbl.Rows
                    'F1～F3が複数発生する（考慮が必要！）

                    If T0005row("WORKKBN") = "F1" Then
                        '出庫時刻の取得
                        WW_SHUKOTIME = T0005row("STTIME")
                        WW_F1CNT += 1
                    End If

                    If WW_F1CNT > 0 Then
                        'MOVETIME集計（ﾊﾝﾄﾞﾙ時間）
                        WW_MOVETIME += T0005row("MOVETIME")
                    End If

                    If T0005row("WORKKBN") = "B3" Then
                        If T0005row("SUISOKBN") <> "1" Then
                            'カウント（届数）
                            WW_B3CNT += 1
                            '数量合計の集計 
                            WW_SURYO += T0005row("TOTALSURYO")
                        End If
                        'カウント（回転）
                        If WW_TRIPNO <> T0005row("TRIPNO") Then
                            WW_TRIPNO = T0005row("TRIPNO")
                            WW_KAIJI += 1
                        End If
                    End If

                    If T0005row("WORKKBN") = "F3" Then
                        WW_F1CNT = 0

                        '業務車番の取得
                        WW_GSHABAN = T0005row("GSHABAN")
                        '油種区分の取得
                        WW_PRODUCT1 = T0005row("OILPAYKBN")
                        WW_PRODUCT1NAME = T0005row("OILPAYKBNNAMES")

                        WW_F3CNT += 1
                        '帰庫時刻の取得
                        WW_KIKOTIME = T0005row("ENDTIME")
                        '走行キロの取得
                        If T0005row("L1KAISO") <> "回送" OrElse T0005row("SUISOKBN") = "1" Then
                            WW_DISTANCE = T0005row("SOUDISTANCE")
                        Else
                            WW_DISTANCE = 0
                        End If
                        '-----------------------------------
                        '日報項目編集＆出力
                        '-----------------------------------
                        If WW_F3CNT = 1 Then
                            For j As Integer = 0 To T0005tbl.Rows.Count - 1
                                If T0005tbl.Rows(j)("WORKKBN") = "BB" Then
                                    '休憩
                                    WW_BREAKTIME += T0005row("WORKTIME")
                                End If
                            Next
                        End If

                        If WW_F3CNT > 1 Then
                            '２両目以降の場合、レコードコピーして追加
                            WW_TA0002row = WW_TA0002tbl.NewRow
                            SetRowSpece(WW_TA0002row)
                        End If

                        WW_TA0002row("SHARYOKBN") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBN_TXT") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBNNAMES") = T0005row("SHARYOKBNNAMES")
                        WW_TA0002row("RYOME") = WW_F3CNT
                        WW_TA0002row("PRODUCT1") = WW_PRODUCT1
                        WW_TA0002row("GSHABAN") = WW_GSHABAN
                        WW_TA0002row("GSHABAN_TXT") = WW_GSHABAN
                        If WW_PRODUCT1 = "" AndAlso WW_PRODUCT1NAME = "" Then
                            WW_TA0002row("PRODUCT1_TXT") = ""
                        Else
                            WW_TA0002row("PRODUCT1_TXT") = WW_TA0002row("PRODUCT1NAMES") & "(" & WW_PRODUCT1 & ")"
                        End If
                        WW_TA0002row("PRODUCT1NAMES") = WW_PRODUCT1NAME
                        If IsDate(WW_SHUKOTIME) Then
                            WW_TA0002row("SHUKOTIME") = CDate(WW_SHUKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("SHUKOTIME") = ""
                        End If
                        If IsDate(WW_KIKOTIME) Then
                            WW_TA0002row("KIKOTIME") = CDate(WW_KIKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("KIKOTIME") = ""
                        End If
                        WW_TA0002row("HANDLETIME") = MinituesToHHMM(Val(WW_MOVETIME))
                        If WW_MOVETIME > 540 Then
                            If WW_TA0002row("RECODEKBN") = "0" Then
                                '15時間を超える場合
                                WW_TA0002row("ORVER09") = "*"
                                WW_TA0002row("ORVER09_TXT") = "*"
                            Else
                                WW_TA0002row("ORVER09") = ""
                                WW_TA0002row("ORVER09_TXT") = ""
                            End If
                        End If
                        'WW_TA0002row("TRIPNO") = Val(WW_TRIPNO).ToString("#")
                        If WW_B3CNT = 0 Then
                            WW_TA0002row("TRIPNO") = Val(0).ToString("#")
                        Else
                            WW_TA0002row("TRIPNO") = Val(WW_KAIJI).ToString("#")
                        End If
                        WW_TA0002row("SURYO") = ZeroToSpace(Val(WW_SURYO).ToString("#0.000"))

                        WW_TA0002row("HAIDISTANCE") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCECHO") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCETTL") = Val(WW_DISTANCE).ToString("#")

                        WW_TA0002row("UNLOADCNT") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))

                        If WW_F3CNT = 1 Then
                            '--------------------------------------
                            '勤務状況リスト編集 
                            '--------------------------------------
                            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

                            If WW_TA0002row("PAYKBN") = "00" Then
                                WW_TA0002row("PAYKBN_TXT") = ""
                                WW_TA0002row("PAYKBNNAMES") = ""
                            End If

                            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
                            End If

                            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                                WW_TA0002row("HOLIDAYKBNNAMES") = ""
                            End If

                            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
                            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
                            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                                WW_TA0002row("STDATE") = ""
                                WW_TA0002row("ENDDATE") = ""
                            End If

                            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
                            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
                            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
                            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                                If WW_TA0002row("RECODEKBN") = "0" Then
                                    '16時間を超える場合
                                    WW_TA0002row("ORVER15") = "*"
                                    WW_TA0002row("ORVER15_TXT") = "*"
                                Else
                                    WW_TA0002row("ORVER15") = ""
                                    WW_TA0002row("ORVER15_TXT") = ""
                                End If
                            End If

                            '休憩（特別編集）
                            WW_TA0002row("BREAKTIME") = HHMMtoMinutes(WW_TA0002row("BREAKTIME")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIMETTL") = HHMMtoMinutes(WW_TA0002row("BREAKTIMETTL")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIME") = MinituesToHHMM(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMETTL") = MinituesToHHMM(WW_TA0002row("BREAKTIMETTL"))

                            '1:法定休日、2:法定外休日
                            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
                              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                                WW_TA0002row("BINDTIME") = ""
                            Else
                                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
                            End If
                            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
                            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
                            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
                            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
                            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
                            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
                            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
                            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
                            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
                            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
                            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
                            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
                            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
                            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
                            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
                            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
                            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
                            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
                            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
                            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
                            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
                            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
                            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
                            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
                            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
                            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
                            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
                            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
                            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
                            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
                            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
                            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
                            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
                            WW_TA0002row("HAYADETIME") = ZeroToSpace(WW_TA0002row("HAYADETIME"))
                            WW_TA0002row("HAYADETIMECHO") = ZeroToSpace(WW_TA0002row("HAYADETIMECHO"))
                            WW_TA0002row("HAYADETIMETTL") = ZeroToSpace(WW_TA0002row("HAYADETIMETTL"))
                            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
                            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
                            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
                            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
                            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
                            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
                            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
                            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
                            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
                            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
                            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
                            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
                            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
                            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
                            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
                            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
                            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
                            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
                            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

                            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
                            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))
                        End If

                        WW_TA0002tbl.Rows.Add(WW_TA0002row)

                        WW_SHUKOTIME = ""
                        WW_KIKOTIME = ""
                        WW_GSHABAN = ""
                        WW_PRODUCT1 = ""
                        WW_PRODUCT1NAME = ""
                        WW_SURYO = 0
                        WW_DISTANCE = 0
                        WW_B3CNT = 0
                        WW_MOVETIME = 0
                        WW_KAIJI = 0
                        WW_TRIPNO = ""
                    End If

                Next
                '日報が存在する場合、下記の編集（勤怠項目のみ：上記の編集と同じ）は行わない
                If T0005tbl.Rows.Count > 0 Then Continue For
            End If

            '---------------------------------------------------
            '勤務状況リスト編集 （日報が存在しない場合の編集）
            '---------------------------------------------------
            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0002row("PAYKBN") = "00" Then
                WW_TA0002row("PAYKBN_TXT") = ""
                WW_TA0002row("PAYKBNNAMES") = ""
            End If

            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                WW_TA0002row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                WW_TA0002row("STDATE") = ""
                WW_TA0002row("ENDDATE") = ""
            End If

            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                If WW_TA0002row("RECODEKBN") = "0" Then
                    '16時間を超える場合
                    WW_TA0002row("ORVER15") = "*"
                    WW_TA0002row("ORVER15_TXT") = "*"
                Else
                    WW_TA0002row("ORVER15") = ""
                    WW_TA0002row("ORVER15_TXT") = ""
                End If
            End If

            '1:法定休日、2:法定外休日
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                WW_TA0002row("BINDTIME") = ""
            Else
                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
            End If
            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
            WW_TA0002row("HAYADETIME") = ZeroToSpace(WW_TA0002row("HAYADETIME"))
            WW_TA0002row("HAYADETIMECHO") = ZeroToSpace(WW_TA0002row("HAYADETIMECHO"))
            WW_TA0002row("HAYADETIMETTL") = ZeroToSpace(WW_TA0002row("HAYADETIMETTL"))
            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '----------------------------------
        '合計行（勤怠側）の編集
        '----------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        Dim WW_BINDTIME As Integer = 0
        Dim WW_ACTTIME As Integer = 0
        Dim WW_BREAKTIME2 As Integer = 0
        Dim WW_ORVERTIME As Integer = 0
        Dim WW_WNIGHTTIME As Integer = 0
        Dim WW_HWORKTIME As Integer = 0
        Dim WW_HNIGHTTIME As Integer = 0
        Dim WW_SWORKTIME As Integer = 0
        Dim WW_SNIGHTTIME As Integer = 0
        Dim WW_NIGHTTIME As Integer = 0
        Dim WW_TOKUSA1TIME As Integer = 0
        Dim WW_HOANTIME As Integer = 0
        Dim WW_KOATUTIME As Integer = 0
        Dim WW_HAYADETIME As Integer = 0
        Dim WW_TOKSAAKAISU As Integer = 0
        Dim WW_TOKSABKAISU As Integer = 0
        Dim WW_TOKSACKAISU As Integer = 0
        Dim WW_TENKOKAISU As Double = 0
        Dim WW_HAISOTIME As Integer = 0
        Dim WW_SHACHUHAKKBN As Integer = 0

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            WW_BINDTIME += HHMMtoMinutes(TA0002Row("BINDTIME"))
            WW_ACTTIME += HHMMtoMinutes(TA0002Row("ACTTIME"))
            WW_BREAKTIME2 += HHMMtoMinutes(TA0002Row("BREAKTIMETTL"))
            WW_ORVERTIME += HHMMtoMinutes(TA0002Row("ORVERTIMETTL"))
            WW_WNIGHTTIME += HHMMtoMinutes(TA0002Row("WNIGHTTIMETTL"))
            WW_HWORKTIME += HHMMtoMinutes(TA0002Row("HWORKTIMETTL"))
            WW_HNIGHTTIME += HHMMtoMinutes(TA0002Row("HNIGHTTIMETTL"))
            WW_SWORKTIME += HHMMtoMinutes(TA0002Row("SWORKTIMETTL"))
            WW_SNIGHTTIME += HHMMtoMinutes(TA0002Row("SNIGHTTIMETTL"))
            WW_NIGHTTIME += HHMMtoMinutes(TA0002Row("NIGHTTIMETTL"))
            WW_TOKUSA1TIME += HHMMtoMinutes(TA0002Row("TOKUSA1TIMETTL"))
            WW_HOANTIME += HHMMtoMinutes(TA0002Row("HOANTIMETTL"))
            WW_KOATUTIME += HHMMtoMinutes(TA0002Row("KOATUTIMETTL"))
            WW_HAYADETIME += HHMMtoMinutes(TA0002Row("HAYADETIMETTL"))
            WW_TOKSAAKAISU += Val(TA0002Row("TOKSAAKAISUTTL"))
            WW_TOKSABKAISU += Val(TA0002Row("TOKSABKAISUTTL"))
            WW_TOKSACKAISU += Val(TA0002Row("TOKSACKAISUTTL"))
            WW_TENKOKAISU += Val(TA0002Row("TENKOKAISUTTL"))
            WW_HAISOTIME += HHMMtoMinutes(TA0002Row("HAISOTIME"))
            WW_SHACHUHAKKBN += Val(TA0002Row("SHACHUHAKKBN"))
        Next

        '合計の場合
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002row("PAYKBNNAMES") = "合計"
        WW_TA0002row("PAYKBN_TXT") = "合計"

        WW_TA0002row("BINDTIME") = ZeroToSpace(MinituesToHHMM(WW_BINDTIME))
        WW_TA0002row("ACTTIME") = ZeroToSpace(MinituesToHHMM(WW_ACTTIME))
        WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_BREAKTIME2))
        WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_ORVERTIME))
        WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_WNIGHTTIME))
        WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HWORKTIME))
        WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HNIGHTTIME))
        WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SWORKTIME))
        WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SNIGHTTIME))
        WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_NIGHTTIME))
        WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(MinituesToHHMM(WW_TOKUSA1TIME))
        WW_TA0002row("HOANTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HOANTIME))
        WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_KOATUTIME))
        WW_TA0002row("HAYADETIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HAYADETIME))
        WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TOKSAAKAISU)
        WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TOKSABKAISU)
        WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TOKSACKAISU)
        WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(Val(WW_TENKOKAISU).ToString("0.0"))
        WW_TA0002row("HAISOTIME") = ZeroToSpace(MinituesToHHMM(WW_HAISOTIME))
        WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_SHACHUHAKKBN)

        WW_TA0002tbl.Rows.Add(WW_TA0002row)


        '----------------------------------
        '合計行（日報側）の編集
        '----------------------------------
        'テンポラリDB項目作成
        Dim WW_NIPPOtbl As DataTable = New DataTable
        WW_NIPPOtbl.Clear()
        WW_NIPPOtbl.Columns.Add("KEY", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBN", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1NAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("TRIPNO", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("UNLOADCNT", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("SURYO", GetType(Double))
        WW_NIPPOtbl.Columns.Add("HAIDISTANCE", GetType(Double))

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            If TA0002Row("PRODUCT1") = "" Then Continue For

            Dim WW_NIPPOrow As DataRow
            WW_NIPPOrow = WW_NIPPOtbl.NewRow

            WW_NIPPOrow("KEY") = "1"
            WW_NIPPOrow("SHARYOKBN") = TA0002Row("SHARYOKBN")
            WW_NIPPOrow("SHARYOKBNNAMES") = TA0002Row("SHARYOKBNNAMES")
            WW_NIPPOrow("PRODUCT1") = TA0002Row("PRODUCT1")
            WW_NIPPOrow("PRODUCT1NAMES") = TA0002Row("PRODUCT1NAMES")
            WW_NIPPOrow("TRIPNO") = Val(TA0002Row("TRIPNO"))
            WW_NIPPOrow("UNLOADCNT") = Val(TA0002Row("UNLOADCNT"))
            WW_NIPPOrow("SURYO") = Val(TA0002Row("SURYO"))
            WW_NIPPOrow("HAIDISTANCE") = Val(TA0002Row("HAIDISTANCE"))

            WW_NIPPOtbl.Rows.Add(WW_NIPPOrow)

        Next

        '油種別合計行（日報側）の編集
        Dim viw As New DataView(WW_NIPPOtbl)
        Dim isDistinct As Boolean = True
        Dim cols() As String = {"KEY", "PRODUCT1", "PRODUCT1NAMES"}
        viw.Sort = "KEY, PRODUCT1"
        Dim dtFilter As DataTable = viw.ToTable(isDistinct, cols)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}' AND PRODUCT1 = '{1}' AND PRODUCT1NAMES = '{2}'", row("KEY"), row("PRODUCT1"), row("PRODUCT1NAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = row("PRODUCT1")
            WW_TA0002row("PRODUCT1NAMES") = row("PRODUCT1NAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next


        '油種合計行（日報側）の編集
        Dim cols2() As String = {"KEY"}
        dtFilter = viw.ToTable(isDistinct, cols2)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}'", row("KEY"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = "合計"
            WW_TA0002row("PRODUCT1NAMES") = "合計"
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '---------------------------------
        '車種別合計行（日報側）の編集
        '---------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        '車種別合計行（日報側）の編集
        Dim cols3() As String = {"SHARYOKBN", "SHARYOKBNNAMES"}
        dtFilter = viw.ToTable(isDistinct, cols3)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("SHARYOKBN = '{0}' AND SHARYOKBNNAMES = '{1}'", row("SHARYOKBN"), row("SHARYOKBNNAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("GSHABAN") = row("SHARYOKBNNAMES")
            WW_TA0002row("GSHABAN_TXT") = row("SHARYOKBNNAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        IO_TBL = WW_TA0002tbl.Copy

        WW_TA0002tbl.Dispose()
        WW_TA0002tbl = Nothing

    End Sub

    ''' <summary>
    ''' 帳票用編集（ＮＪＳ）
    ''' </summary>
    ''' <param name="IO_TBL">編集テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Protected Sub EditListNJS(ByRef IO_TBL As DataTable, ByRef O_RTN As String)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0002tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0002row As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0002row = WW_TA0002tbl.NewRow
            WW_TA0002row.ItemArray = IO_TBL.Rows(i).ItemArray

            If WW_TA0002row("HDKBN") <> "H" Then Continue For

            If WW_TA0002row("RECODEKBN") = "0" Then

                '--------------------------------------
                '日報項目編集（日報が存在する場合の編集）
                '--------------------------------------
                GetT00010(WW_TA0002row("WORKDATE"), WW_TA0002row("STAFFCODE"), T0010tbl, WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                For Each T0010row As DataRow In T0010tbl.Rows
                    For j As Integer = 1 To T0010row("SAVECNT")
                        If j <> 1 Then
                            WW_TA0002row = WW_TA0002tbl.NewRow
                            SetRowSpece(WW_TA0002row)
                        End If

                        Dim WW_SHARYOKBN As String = "SHARYOKBN" & j.ToString
                        Dim WW_SHARYOKBNNAME As String = "SHARYOKBN" & j.ToString & "NAME"
                        Dim WW_OILPAYKBN As String = "OILPAYKBN" & j.ToString
                        Dim WW_OILPAYKBNNAME As String = "OILPAYKBN" & j.ToString & "NAME"
                        Dim WW_SHUKABASHO As String = "SHUKABASHO" & j.ToString
                        Dim WW_SHUKABASHONAME As String = "SHUKABASHO" & j.ToString & "NAME"
                        Dim WW_TODOKECODE As String = "TODOKECODE" & j.ToString
                        Dim WW_TODOKECODENAME As String = "TODOKECODE" & j.ToString & "NAME"
                        Dim WW_MODELDISTANCE As String = "MODELDISTANCE" & j.ToString
                        Dim WW_MODIFYKBN As String = "MODIFYKBN" & j.ToString

                        WW_TA0002row("TRIPNO") = j.ToString
                        WW_TA0002row("SHARYOKBN") = T0010row(WW_SHARYOKBN)
                        WW_TA0002row("SHARYOKBNNAMES") = T0010row(WW_SHARYOKBNNAME)
                        WW_TA0002row("PRODUCT1") = T0010row(WW_OILPAYKBN)
                        WW_TA0002row("PRODUCT1NAMES") = T0010row(WW_OILPAYKBNNAME)
                        WW_TA0002row("SHUKABASHO") = T0010row(WW_SHUKABASHO)
                        WW_TA0002row("SHUKABASHONAMES") = T0010row(WW_SHUKABASHONAME)
                        WW_TA0002row("TODOKECODE") = T0010row(WW_TODOKECODE)
                        WW_TA0002row("TODOKECODENAMES") = T0010row(WW_TODOKECODENAME)
                        WW_TA0002row("MODELDISTANCE") = ZeroToSpace(T0010row(WW_MODELDISTANCE))
                        If T0010row(WW_MODIFYKBN) = "1" Then
                            WW_TA0002row("MODIFYKBN") = "✔"
                        Else
                            WW_TA0002row("MODIFYKBN") = ZeroToSpace(T0010row(WW_MODIFYKBN))
                        End If
                        WW_TA0002tbl.Rows.Add(WW_TA0002row)
                    Next
                    If Val(T0010row("SAVECNT")) = 0 Then
                        WW_TA0002row("TRIPNO") = ""
                        WW_TA0002row("SHARYOKBN") = ""
                        WW_TA0002row("SHARYOKBNNAMES") = ""
                        WW_TA0002row("PRODUCT1") = ""
                        WW_TA0002row("PRODUCT1NAMES") = ""
                        WW_TA0002row("SHUKABASHO") = ""
                        WW_TA0002row("SHUKABASHONAMES") = ""
                        WW_TA0002row("TODOKECODE") = ""
                        WW_TA0002row("TODOKECODENAMES") = ""
                        WW_TA0002row("MODELDISTANCE") = ""
                        WW_TA0002tbl.Rows.Add(WW_TA0002row)
                    End If
                Next
                'モデル距離が存在する場合、下記の編集（勤怠項目のみ：上記の編集と同じ）は行わない
                If T0010tbl.Rows.Count > 0 Then Continue For
            End If

            '---------------------------------------------------
            '勤務状況リスト編集 （日報が存在しない場合の編集）
            '---------------------------------------------------
            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0002row("PAYKBN") = "00" Then
                WW_TA0002row("PAYKBN_TXT") = ""
                WW_TA0002row("PAYKBNNAMES") = ""
            End If

            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                WW_TA0002row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                WW_TA0002row("STDATE") = ""
                WW_TA0002row("ENDDATE") = ""
            End If

            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                If WW_TA0002row("RECODEKBN") = "0" Then
                    '16時間を超える場合
                    WW_TA0002row("ORVER15") = "*"
                    WW_TA0002row("ORVER15_TXT") = "*"
                Else
                    WW_TA0002row("ORVER15") = ""
                    WW_TA0002row("ORVER15_TXT") = ""
                End If
            End If

            '1:法定休日、2:法定外休日
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                WW_TA0002row("BINDTIME") = ""
            Else
                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
            End If
            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '----------------------------------
        '合計行（勤怠側）の編集
        '----------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        Dim WW_BINDTIME As Integer = 0
        Dim WW_ACTTIME As Integer = 0
        Dim WW_BREAKTIME2 As Integer = 0
        Dim WW_ORVERTIME As Integer = 0
        Dim WW_WNIGHTTIME As Integer = 0
        Dim WW_HWORKTIME As Integer = 0
        Dim WW_HNIGHTTIME As Integer = 0
        Dim WW_SWORKTIME As Integer = 0
        Dim WW_SNIGHTTIME As Integer = 0
        Dim WW_NIGHTTIME As Integer = 0
        Dim WW_TOKUSA1TIME As Integer = 0
        Dim WW_HOANTIME As Integer = 0
        Dim WW_KOATUTIME As Integer = 0
        Dim WW_TOKSAAKAISU As Integer = 0
        Dim WW_TOKSABKAISU As Integer = 0
        Dim WW_TOKSACKAISU As Integer = 0
        Dim WW_TENKOKAISU As Double = 0
        Dim WW_HAISOTIME As Integer = 0
        Dim WW_SHACHUHAKKBN As Integer = 0

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            WW_BINDTIME += HHMMtoMinutes(TA0002Row("BINDTIME"))
            WW_ACTTIME += HHMMtoMinutes(TA0002Row("ACTTIME"))
            WW_BREAKTIME2 += HHMMtoMinutes(TA0002Row("BREAKTIMETTL"))
            WW_ORVERTIME += HHMMtoMinutes(TA0002Row("ORVERTIMETTL"))
            WW_WNIGHTTIME += HHMMtoMinutes(TA0002Row("WNIGHTTIMETTL"))
            WW_HWORKTIME += HHMMtoMinutes(TA0002Row("HWORKTIMETTL"))
            WW_HNIGHTTIME += HHMMtoMinutes(TA0002Row("HNIGHTTIMETTL"))
            WW_SWORKTIME += HHMMtoMinutes(TA0002Row("SWORKTIMETTL"))
            WW_SNIGHTTIME += HHMMtoMinutes(TA0002Row("SNIGHTTIMETTL"))
            WW_NIGHTTIME += HHMMtoMinutes(TA0002Row("NIGHTTIMETTL"))
            WW_TOKUSA1TIME += HHMMtoMinutes(TA0002Row("TOKUSA1TIMETTL"))
            WW_HOANTIME += HHMMtoMinutes(TA0002Row("HOANTIMETTL"))
            WW_KOATUTIME += HHMMtoMinutes(TA0002Row("KOATUTIMETTL"))
            WW_TOKSAAKAISU += Val(TA0002Row("TOKSAAKAISUTTL"))
            WW_TOKSABKAISU += Val(TA0002Row("TOKSABKAISUTTL"))
            WW_TOKSACKAISU += Val(TA0002Row("TOKSACKAISUTTL"))
            WW_TENKOKAISU += Val(TA0002Row("TENKOKAISUTTL"))
            WW_HAISOTIME += HHMMtoMinutes(TA0002Row("HAISOTIME"))
            WW_SHACHUHAKKBN += Val(TA0002Row("SHACHUHAKKBN"))
        Next

        '合計の場合
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002row("PAYKBNNAMES") = "合計"
        WW_TA0002row("PAYKBN_TXT") = "合計"

        WW_TA0002row("BINDTIME") = ZeroToSpace(MinituesToHHMM(WW_BINDTIME))
        WW_TA0002row("ACTTIME") = ZeroToSpace(MinituesToHHMM(WW_ACTTIME))
        WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_BREAKTIME2))
        WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_ORVERTIME))
        WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_WNIGHTTIME))
        WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HWORKTIME))
        WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HNIGHTTIME))
        WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SWORKTIME))
        WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SNIGHTTIME))
        WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_NIGHTTIME))
        WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(MinituesToHHMM(WW_TOKUSA1TIME))
        WW_TA0002row("HOANTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HOANTIME))
        WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_KOATUTIME))
        WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TOKSAAKAISU)
        WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TOKSABKAISU)
        WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TOKSACKAISU)
        WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(Val(WW_TENKOKAISU).ToString("0.0"))
        WW_TA0002row("HAISOTIME") = ZeroToSpace(MinituesToHHMM(WW_HAISOTIME))
        WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_SHACHUHAKKBN)

        WW_TA0002tbl.Rows.Add(WW_TA0002row)


        '----------------------------------
        '合計行（日報側）の編集
        '----------------------------------
        'テンポラリDB項目作成
        Dim WW_NIPPOtbl As DataTable = New DataTable
        WW_NIPPOtbl.Clear()
        WW_NIPPOtbl.Columns.Add("KEY", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBN", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1NAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("MODELDISTANCE", GetType(Double))

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            If TA0002Row("PRODUCT1") = "" Then
                Continue For
            End If

            Dim WW_NIPPOrow As DataRow
            WW_NIPPOrow = WW_NIPPOtbl.NewRow

            WW_NIPPOrow("KEY") = "1"
            WW_NIPPOrow("SHARYOKBN") = TA0002Row("SHARYOKBN")
            WW_NIPPOrow("SHARYOKBNNAMES") = TA0002Row("SHARYOKBNNAMES")
            WW_NIPPOrow("PRODUCT1") = TA0002Row("PRODUCT1")
            WW_NIPPOrow("PRODUCT1NAMES") = TA0002Row("PRODUCT1NAMES")
            WW_NIPPOrow("MODELDISTANCE") = Val(TA0002Row("MODELDISTANCE"))

            WW_NIPPOtbl.Rows.Add(WW_NIPPOrow)

        Next

        '油種別合計行（日報側）の編集
        Dim viw As New DataView(WW_NIPPOtbl)
        Dim isDistinct As Boolean = True
        Dim cols() As String = {"KEY", "SHARYOKBN", "SHARYOKBNNAMES", "PRODUCT1", "PRODUCT1NAMES"}
        viw.Sort = "KEY, PRODUCT1"
        Dim dtFilter As DataTable = viw.ToTable(isDistinct, cols)
        dtFilter.Columns.Add("MODELDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}' AND SHARYOKBN = '{1}' AND SHARYOKBNNAMES = '{2}' AND PRODUCT1 = '{3}' AND PRODUCT1NAMES = '{4}'", row("KEY"), row("SHARYOKBN"), row("SHARYOKBNNAMES"), row("PRODUCT1"), row("PRODUCT1NAMES"))
            row("MODELDISTANCE") = WW_NIPPOtbl.Compute("SUM(MODELDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("SHUKABASHONAMES") = row("SHARYOKBNNAMES") & "・" & row("PRODUCT1NAMES")
            WW_TA0002row("MODELDISTANCE") = ZeroToSpace(Val(row("MODELDISTANCE")).ToString("#"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next


        '油種合計行（日報側）の編集
        Dim cols2() As String = {"KEY"}
        dtFilter = viw.ToTable(isDistinct, cols2)
        dtFilter.Columns.Add("MODELDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}'", row("KEY"))
            row("MODELDISTANCE") = WW_NIPPOtbl.Compute("SUM(MODELDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("SHUKABASHONAMES") = "合計"
            WW_TA0002row("MODELDISTANCE") = ZeroToSpace(Val(row("MODELDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        IO_TBL = WW_TA0002tbl.Copy

        WW_TA0002tbl.Dispose()
        WW_TA0002tbl = Nothing

    End Sub

    ''' <summary>
    ''' 帳票用編集（近石）
    ''' </summary>
    ''' <param name="IO_TBL">編集テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Protected Sub EditListKNK(ByRef IO_TBL As DataTable, ByRef O_RTN As String)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0002tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0002row As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0002row = WW_TA0002tbl.NewRow
            WW_TA0002row.ItemArray = IO_TBL.Rows(i).ItemArray

            If WW_TA0002row("HDKBN") <> "H" Then Continue For

            If WW_TA0002row("RECODEKBN") = "0" Then

                '--------------------------------------
                '日報項目編集（日報が存在する場合の編集）
                '--------------------------------------
                GetT00005(WW_TA0002row("WORKDATE"), WW_TA0002row("WORKDATE"), WW_TA0002row("STAFFCODE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                Dim WW_F1CNT As Integer = 0
                Dim WW_F3CNT As Integer = 0
                Dim WW_KAIJI As Integer = 0
                Dim WW_B3CNT As Integer = 0
                Dim WW_SHUKOTIME As String = ""
                Dim WW_KIKOTIME As String = ""
                Dim WW_GSHABAN As String = ""
                Dim WW_PRODUCT1 As String = ""
                Dim WW_PRODUCT1NAME As String = ""
                Dim WW_SURYO As Double = 0
                Dim WW_DISTANCE As Double = 0
                Dim WW_JIDISTANCE As Double = 0
                Dim WW_KUDISTANCE As Double = 0
                Dim WW_MOVETIME As Integer = 0
                Dim WW_BREAKTIME As Integer = 0
                Dim WW_TRIPNO As String = ""

                For Each T0005row As DataRow In T0005tbl.Rows
                    'F1～F3が複数発生する（考慮が必要！）

                    If T0005row("WORKKBN") = "F1" Then
                        '出庫時刻の取得
                        WW_SHUKOTIME = T0005row("STTIME")
                        WW_F1CNT += 1
                    End If

                    If WW_F1CNT > 0 Then
                        'MOVETIME集計（ﾊﾝﾄﾞﾙ時間）
                        WW_MOVETIME += T0005row("MOVETIME")
                    End If

                    If T0005row("WORKKBN") = "B3" Then
                        If T0005row("SUISOKBN") <> "1" Then
                            'カウント（届数）
                            WW_B3CNT += 1
                            '数量合計の集計 
                            WW_SURYO += T0005row("TOTALSURYO")
                        End If
                        'カウント（回転）
                        If WW_TRIPNO <> T0005row("TRIPNO") Then
                            WW_TRIPNO = T0005row("TRIPNO")
                            WW_KAIJI += 1
                        End If

                    End If

                    If T0005row("WORKKBN") = "F3" Then
                        WW_F1CNT = 0

                        '業務車番の取得
                        WW_GSHABAN = T0005row("GSHABAN")
                        '油種区分の取得
                        WW_PRODUCT1 = T0005row("OILPAYKBN")
                        WW_PRODUCT1NAME = T0005row("OILPAYKBNNAMES")

                        WW_F3CNT += 1
                        '帰庫時刻の取得
                        WW_KIKOTIME = T0005row("ENDTIME")
                        '走行キロの取得
                        'If T0005row("L1KAISO") <> "回送" OrElse T0005row("SUISOKBN") = "1" Then
                        '    WW_DISTANCE = T0005row("SOUDISTANCE")
                        'Else
                        '    WW_DISTANCE = 0
                        'End If
                        WW_DISTANCE = T0005row("SOUDISTANCE")
                        WW_JIDISTANCE = T0005row("JIDISTANCE")
                        WW_KUDISTANCE = T0005row("KUDISTANCE")
                        '-----------------------------------
                        '日報項目編集＆出力
                        '-----------------------------------
                        If WW_F3CNT = 1 Then
                            For j As Integer = 0 To T0005tbl.Rows.Count - 1
                                If T0005tbl.Rows(j)("WORKKBN") = "BB" Then
                                    '休憩
                                    WW_BREAKTIME += T0005row("WORKTIME")
                                End If
                            Next
                        End If

                        If WW_F3CNT > 1 Then
                            '２両目以降の場合、レコードコピーして追加
                            WW_TA0002row = WW_TA0002tbl.NewRow
                            SetRowSpece(WW_TA0002row)
                        End If

                        WW_TA0002row("SHARYOKBN") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBN_TXT") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBNNAMES") = T0005row("SHARYOKBNNAMES")
                        WW_TA0002row("CREWKBNNAMES") = T0005row("CREWKBNNAMES")
                        WW_TA0002row("RYOME") = WW_F3CNT
                        WW_TA0002row("PRODUCT1") = WW_PRODUCT1
                        WW_TA0002row("GSHABAN") = WW_GSHABAN
                        WW_TA0002row("GSHABAN_TXT") = WW_GSHABAN
                        If WW_PRODUCT1 = "" AndAlso WW_PRODUCT1NAME = "" Then
                            WW_TA0002row("PRODUCT1_TXT") = ""
                        Else
                            WW_TA0002row("PRODUCT1_TXT") = WW_TA0002row("PRODUCT1NAMES") & "(" & WW_PRODUCT1 & ")"
                        End If
                        WW_TA0002row("PRODUCT1NAMES") = WW_PRODUCT1NAME
                        If IsDate(WW_SHUKOTIME) Then
                            WW_TA0002row("SHUKOTIME") = CDate(WW_SHUKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("SHUKOTIME") = ""
                        End If
                        If IsDate(WW_KIKOTIME) Then
                            WW_TA0002row("KIKOTIME") = CDate(WW_KIKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("KIKOTIME") = ""
                        End If
                        WW_TA0002row("HANDLETIME") = MinituesToHHMM(Val(WW_MOVETIME))
                        If WW_MOVETIME > 540 Then
                            If WW_TA0002row("RECODEKBN") = "0" Then
                                '15時間を超える場合
                                WW_TA0002row("ORVER09") = "*"
                                WW_TA0002row("ORVER09_TXT") = "*"
                            Else
                                WW_TA0002row("ORVER09") = ""
                                WW_TA0002row("ORVER09_TXT") = ""
                            End If
                        End If
                        'WW_TA0002row("TRIPNO") = Val(WW_TRIPNO).ToString("#")
                        If WW_B3CNT = 0 Then
                            WW_TA0002row("TRIPNO") = Val(0).ToString("#")
                        Else
                            WW_TA0002row("TRIPNO") = Val(WW_KAIJI).ToString("#")
                        End If
                        WW_TA0002row("SURYO") = ZeroToSpace(Val(WW_SURYO).ToString("#0.0"))

                        WW_TA0002row("HAIDISTANCE") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCECHO") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCETTL") = Val(WW_DISTANCE).ToString("#")

                        WW_TA0002row("JIDISTANCE") = Val(WW_JIDISTANCE).ToString("#")
                        WW_TA0002row("KUDISTANCE") = Val(WW_KUDISTANCE).ToString("#")

                        WW_TA0002row("UNLOADCNT") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))


                        If WW_F3CNT = 1 Then
                            '--------------------------------------
                            '勤務状況リスト編集 
                            '--------------------------------------
                            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

                            If WW_TA0002row("PAYKBN") = "00" Then
                                WW_TA0002row("PAYKBN_TXT") = ""
                                WW_TA0002row("PAYKBNNAMES") = ""
                            End If

                            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
                            End If

                            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                                WW_TA0002row("HOLIDAYKBNNAMES") = ""
                            End If

                            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
                            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
                            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                                WW_TA0002row("STDATE") = ""
                                WW_TA0002row("ENDDATE") = ""
                            End If

                            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
                            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
                            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
                            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                                If WW_TA0002row("RECODEKBN") = "0" Then
                                    '16時間を超える場合
                                    WW_TA0002row("ORVER15") = "*"
                                    WW_TA0002row("ORVER15_TXT") = "*"
                                Else
                                    WW_TA0002row("ORVER15") = ""
                                    WW_TA0002row("ORVER15_TXT") = ""
                                End If
                            End If

                            '休憩（特別編集）
                            WW_TA0002row("BREAKTIME") = HHMMtoMinutes(WW_TA0002row("BREAKTIME")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIMETTL") = HHMMtoMinutes(WW_TA0002row("BREAKTIMETTL")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIME") = MinituesToHHMM(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMETTL") = MinituesToHHMM(WW_TA0002row("BREAKTIMETTL"))

                            '1:法定休日、2:法定外休日
                            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
                              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                                WW_TA0002row("BINDTIME") = ""
                            Else
                                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
                            End If
                            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
                            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
                            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
                            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
                            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
                            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
                            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
                            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
                            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
                            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
                            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
                            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
                            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
                            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
                            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
                            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
                            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
                            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
                            WW_TA0002row("SDAIWORKTIME") = ZeroToSpace(WW_TA0002row("SDAIWORKTIME"))
                            WW_TA0002row("SDAIWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SDAIWORKTIMECHO"))
                            WW_TA0002row("SDAIWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SDAIWORKTIMETTL"))
                            WW_TA0002row("SDAINIGHTTIME") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIME"))
                            WW_TA0002row("SDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIMECHO"))
                            WW_TA0002row("SDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIMETTL"))
                            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
                            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
                            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
                            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
                            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
                            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
                            WW_TA0002row("HDAIWORKTIME") = ZeroToSpace(WW_TA0002row("HDAIWORKTIME"))
                            WW_TA0002row("HDAIWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HDAIWORKTIMECHO"))
                            WW_TA0002row("HDAIWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HDAIWORKTIMETTL"))
                            WW_TA0002row("HDAINIGHTTIME") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIME"))
                            WW_TA0002row("HDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIMECHO"))
                            WW_TA0002row("HDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIMETTL"))
                            WW_TA0002row("WWORKTIME") = ZeroToSpace(WW_TA0002row("WWORKTIME"))
                            WW_TA0002row("WWORKTIMECHO") = ZeroToSpace(WW_TA0002row("WWORKTIMECHO"))
                            WW_TA0002row("WWORKTIMETTL") = ZeroToSpace(WW_TA0002row("WWORKTIMETTL"))
                            WW_TA0002row("JYOMUTIME") = ZeroToSpace(WW_TA0002row("JYOMUTIME"))
                            WW_TA0002row("JYOMUTIMECHO") = ZeroToSpace(WW_TA0002row("JYOMUTIMECHO"))
                            WW_TA0002row("JYOMUTIMETTL") = ZeroToSpace(WW_TA0002row("JYOMUTIMETTL"))
                            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
                            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
                            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
                            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
                            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
                            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
                            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
                            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
                            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
                            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
                            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
                            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
                            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
                            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
                            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
                            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
                            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
                            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
                            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
                            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
                            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
                            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
                            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
                            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
                            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
                            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
                            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
                            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

                            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
                            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))

                            WW_TA0002row("JIDISTANCE") = ZeroToSpace(WW_TA0002row("JIDISTANCE"))
                            WW_TA0002row("KUDISTANCE") = ZeroToSpace(WW_TA0002row("KUDISTANCE"))
                        End If

                        WW_TA0002tbl.Rows.Add(WW_TA0002row)

                        WW_SHUKOTIME = ""
                        WW_KIKOTIME = ""
                        WW_GSHABAN = ""
                        WW_PRODUCT1 = ""
                        WW_PRODUCT1NAME = ""
                        WW_SURYO = 0
                        WW_DISTANCE = 0
                        WW_JIDISTANCE = 0
                        WW_KUDISTANCE = 0
                        WW_B3CNT = 0
                        WW_MOVETIME = 0
                        WW_KAIJI = 0
                        WW_TRIPNO = ""
                    End If

                Next
                '日報が存在する場合、下記の編集（勤怠項目のみ：上記の編集と同じ）は行わない
                If T0005tbl.Rows.Count > 0 Then
                    Continue For
                End If
            End If

            '---------------------------------------------------
            '勤務状況リスト編集 （日報が存在しない場合の編集）
            '---------------------------------------------------
            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0002row("PAYKBN") = "00" Then
                WW_TA0002row("PAYKBN_TXT") = ""
                WW_TA0002row("PAYKBNNAMES") = ""
            End If

            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                WW_TA0002row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                WW_TA0002row("STDATE") = ""
                WW_TA0002row("ENDDATE") = ""
            End If

            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                If WW_TA0002row("RECODEKBN") = "0" Then
                    '16時間を超える場合
                    WW_TA0002row("ORVER15") = "*"
                    WW_TA0002row("ORVER15_TXT") = "*"
                Else
                    WW_TA0002row("ORVER15") = ""
                    WW_TA0002row("ORVER15_TXT") = ""
                End If
            End If

            '1:法定休日、2:法定外休日
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                WW_TA0002row("BINDTIME") = ""
            Else
                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
            End If
            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
            WW_TA0002row("SDAIWORKTIME") = ZeroToSpace(WW_TA0002row("SDAIWORKTIME"))
            WW_TA0002row("SDAIWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SDAIWORKTIMECHO"))
            WW_TA0002row("SDAIWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SDAIWORKTIMETTL"))
            WW_TA0002row("SDAINIGHTTIME") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIME"))
            WW_TA0002row("SDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIMECHO"))
            WW_TA0002row("SDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SDAINIGHTTIMETTL"))
            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
            WW_TA0002row("HDAIWORKTIME") = ZeroToSpace(WW_TA0002row("HDAIWORKTIME"))
            WW_TA0002row("HDAIWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HDAIWORKTIMECHO"))
            WW_TA0002row("HDAIWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HDAIWORKTIMETTL"))
            WW_TA0002row("HDAINIGHTTIME") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIME"))
            WW_TA0002row("HDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIMECHO"))
            WW_TA0002row("HDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HDAINIGHTTIMETTL"))
            WW_TA0002row("WWORKTIME") = ZeroToSpace(WW_TA0002row("WWORKTIME"))
            WW_TA0002row("WWORKTIMECHO") = ZeroToSpace(WW_TA0002row("WWORKTIMECHO"))
            WW_TA0002row("WWORKTIMETTL") = ZeroToSpace(WW_TA0002row("WWORKTIMETTL"))
            WW_TA0002row("JYOMUTIME") = ZeroToSpace(WW_TA0002row("JYOMUTIME"))
            WW_TA0002row("JYOMUTIMECHO") = ZeroToSpace(WW_TA0002row("JYOMUTIMECHO"))
            WW_TA0002row("JYOMUTIMETTL") = ZeroToSpace(WW_TA0002row("JYOMUTIMETTL"))
            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
            WW_TA0002row("JIDISTANCE") = ZeroToSpace(WW_TA0002row("JIDISTANCE"))
            WW_TA0002row("KUDISTANCE") = ZeroToSpace(WW_TA0002row("KUDISTANCE"))
            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))

            WW_TA0002row("JIDISTANCE") = ZeroToSpace(WW_TA0002row("JIDISTANCE"))
            WW_TA0002row("KUDISTANCE") = ZeroToSpace(WW_TA0002row("KUDISTANCE"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '----------------------------------
        '合計行（勤怠側）の編集
        '----------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        Dim WW_BINDTIME As Integer = 0
        Dim WW_ACTTIME As Integer = 0
        Dim WW_BREAKTIME2 As Integer = 0
        Dim WW_ORVERTIME As Integer = 0
        Dim WW_WNIGHTTIME As Integer = 0
        Dim WW_HWORKTIME As Integer = 0
        Dim WW_HNIGHTTIME As Integer = 0
        Dim WW_HDAIWORKTIME As Integer = 0
        Dim WW_HDAINIGHTTIME As Integer = 0
        Dim WW_SWORKTIME As Integer = 0
        Dim WW_SNIGHTTIME As Integer = 0
        Dim WW_SDAIWORKTIME As Integer = 0
        Dim WW_SDAINIGHTTIME As Integer = 0
        Dim WW_WWORKTIME As Integer = 0
        Dim WW_JYOMUTIME As Integer = 0
        Dim WW_NIGHTTIME As Integer = 0
        Dim WW_TOKUSA1TIME As Integer = 0
        Dim WW_HOANTIME As Integer = 0
        Dim WW_KOATUTIME As Integer = 0
        Dim WW_TOKSAAKAISU As Integer = 0
        Dim WW_TOKSABKAISU As Integer = 0
        Dim WW_TOKSACKAISU As Integer = 0
        Dim WW_TENKOKAISU As Double = 0
        Dim WW_HAISOTIME As Integer = 0
        Dim WW_SHACHUHAKKBN As Integer = 0

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            WW_BINDTIME += HHMMtoMinutes(TA0002Row("BINDTIME"))
            WW_ACTTIME += HHMMtoMinutes(TA0002Row("ACTTIME"))
            WW_BREAKTIME2 += HHMMtoMinutes(TA0002Row("BREAKTIMETTL"))
            WW_ORVERTIME += HHMMtoMinutes(TA0002Row("ORVERTIMETTL"))
            WW_WNIGHTTIME += HHMMtoMinutes(TA0002Row("WNIGHTTIMETTL"))
            WW_HWORKTIME += HHMMtoMinutes(TA0002Row("HWORKTIMETTL"))
            WW_HNIGHTTIME += HHMMtoMinutes(TA0002Row("HNIGHTTIMETTL"))
            WW_HDAIWORKTIME += HHMMtoMinutes(TA0002Row("HDAIWORKTIMETTL"))
            WW_HDAINIGHTTIME += HHMMtoMinutes(TA0002Row("HDAINIGHTTIMETTL"))
            WW_SWORKTIME += HHMMtoMinutes(TA0002Row("SWORKTIMETTL"))
            WW_SNIGHTTIME += HHMMtoMinutes(TA0002Row("SNIGHTTIMETTL"))
            WW_SDAIWORKTIME += HHMMtoMinutes(TA0002Row("SDAIWORKTIMETTL"))
            WW_SDAINIGHTTIME += HHMMtoMinutes(TA0002Row("SDAINIGHTTIMETTL"))
            WW_WWORKTIME += HHMMtoMinutes(TA0002Row("WWORKTIMETTL"))
            WW_JYOMUTIME += HHMMtoMinutes(TA0002Row("JYOMUTIMETTL"))
            WW_NIGHTTIME += HHMMtoMinutes(TA0002Row("NIGHTTIMETTL"))
            WW_TOKUSA1TIME += HHMMtoMinutes(TA0002Row("TOKUSA1TIMETTL"))
            WW_HOANTIME += HHMMtoMinutes(TA0002Row("HOANTIMETTL"))
            WW_KOATUTIME += HHMMtoMinutes(TA0002Row("KOATUTIMETTL"))
            WW_TOKSAAKAISU += Val(TA0002Row("TOKSAAKAISUTTL"))
            WW_TOKSABKAISU += Val(TA0002Row("TOKSABKAISUTTL"))
            WW_TOKSACKAISU += Val(TA0002Row("TOKSACKAISUTTL"))
            WW_TENKOKAISU += Val(TA0002Row("TENKOKAISUTTL"))
            WW_HAISOTIME += HHMMtoMinutes(TA0002Row("HAISOTIME"))
            WW_SHACHUHAKKBN += Val(TA0002Row("SHACHUHAKKBN"))
        Next

        '合計の場合
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002row("PAYKBNNAMES") = "合計"
        WW_TA0002row("PAYKBN_TXT") = "合計"

        WW_TA0002row("BINDTIME") = ZeroToSpace(MinituesToHHMM(WW_BINDTIME))
        WW_TA0002row("ACTTIME") = ZeroToSpace(MinituesToHHMM(WW_ACTTIME))
        WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_BREAKTIME2))
        WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_ORVERTIME))
        WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_WNIGHTTIME))
        WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HWORKTIME))
        WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HNIGHTTIME))
        WW_TA0002row("HDAIWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HDAIWORKTIME))
        WW_TA0002row("HDAINIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HDAINIGHTTIME))
        WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SWORKTIME))
        WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SNIGHTTIME))
        WW_TA0002row("SDAIWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SDAIWORKTIME))
        WW_TA0002row("SDAINIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SDAINIGHTTIME))
        WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_NIGHTTIME))
        WW_TA0002row("WWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_WWORKTIME))
        WW_TA0002row("JYOMUTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_JYOMUTIME))
        WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(MinituesToHHMM(WW_TOKUSA1TIME))
        WW_TA0002row("HOANTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HOANTIME))
        WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_KOATUTIME))
        WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TOKSAAKAISU)
        WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TOKSABKAISU)
        WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TOKSACKAISU)
        WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(Val(WW_TENKOKAISU).ToString("0.0"))
        WW_TA0002row("HAISOTIME") = ZeroToSpace(MinituesToHHMM(WW_HAISOTIME))
        WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_SHACHUHAKKBN)

        WW_TA0002tbl.Rows.Add(WW_TA0002row)


        '----------------------------------
        '合計行（日報側）の編集
        '----------------------------------
        'テンポラリDB項目作成
        Dim WW_NIPPOtbl As DataTable = New DataTable
        WW_NIPPOtbl.Clear()
        WW_NIPPOtbl.Columns.Add("KEY", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBN", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1NAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("TRIPNO", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("UNLOADCNT", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("SURYO", GetType(Double))
        WW_NIPPOtbl.Columns.Add("HAIDISTANCE", GetType(Double))

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            If TA0002Row("PRODUCT1") = "" Then
                Continue For
            End If

            Dim WW_NIPPOrow As DataRow
            WW_NIPPOrow = WW_NIPPOtbl.NewRow

            WW_NIPPOrow("KEY") = "1"
            WW_NIPPOrow("SHARYOKBN") = TA0002Row("SHARYOKBN")
            WW_NIPPOrow("SHARYOKBNNAMES") = TA0002Row("SHARYOKBNNAMES")
            WW_NIPPOrow("PRODUCT1") = TA0002Row("PRODUCT1")
            WW_NIPPOrow("PRODUCT1NAMES") = TA0002Row("PRODUCT1NAMES")
            WW_NIPPOrow("TRIPNO") = Val(TA0002Row("TRIPNO"))
            WW_NIPPOrow("UNLOADCNT") = Val(TA0002Row("UNLOADCNT"))
            WW_NIPPOrow("SURYO") = Val(TA0002Row("SURYO"))
            WW_NIPPOrow("HAIDISTANCE") = Val(TA0002Row("HAIDISTANCE"))

            WW_NIPPOtbl.Rows.Add(WW_NIPPOrow)

        Next

        '油種別合計行（日報側）の編集
        Dim viw As New DataView(WW_NIPPOtbl)
        Dim isDistinct As Boolean = True
        Dim cols() As String = {"KEY", "PRODUCT1", "PRODUCT1NAMES", "SHARYOKBN", "SHARYOKBNNAMES"}
        viw.Sort = "KEY, PRODUCT1, SHARYOKBN"
        Dim dtFilter As DataTable = viw.ToTable(isDistinct, cols)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}' AND PRODUCT1 = '{1}' AND PRODUCT1NAMES = '{2}' AND SHARYOKBN = '{3}' AND SHARYOKBNNAMES = '{4}'", row("KEY"), row("PRODUCT1"), row("PRODUCT1NAMES"), row("SHARYOKBN"), row("SHARYOKBNNAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = row("PRODUCT1")
            WW_TA0002row("PRODUCT1NAMES") = row("PRODUCT1NAMES")
            WW_TA0002row("SHARYOKBN_TXT") = row("SHARYOKBN")
            WW_TA0002row("SHARYOKBNNAMES") = row("SHARYOKBNNAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.0"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next


        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        '車両合計行（日報側）の編集
        Dim cols2() As String = {"KEY", "SHARYOKBN", "SHARYOKBNNAMES"}
        dtFilter = viw.ToTable(isDistinct, cols2)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}' AND SHARYOKBN = '{1}' AND SHARYOKBNNAMES = '{2}'", row("KEY"), row("SHARYOKBN"), row("SHARYOKBNNAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = "小計"
            WW_TA0002row("PRODUCT1NAMES") = "小計"
            WW_TA0002row("SHARYOKBN_TXT") = row("SHARYOKBN")
            WW_TA0002row("SHARYOKBNNAMES") = row("SHARYOKBNNAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.0"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '合計行（日報側）の編集
        Dim cols3() As String = {"KEY"}
        dtFilter = viw.ToTable(isDistinct, cols3)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}'", row("KEY"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = "合計"
            WW_TA0002row("PRODUCT1NAMES") = "合計"
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.0"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        IO_TBL = WW_TA0002tbl.Copy

        WW_TA0002tbl.Dispose()
        WW_TA0002tbl = Nothing

    End Sub

    ''' <summary>
    ''' 帳票用編集（ＪＫＴ）
    ''' </summary>
    ''' <param name="IO_TBL">編集テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Protected Sub EditListJKT(ByRef IO_TBL As DataTable, ByRef O_RTN As String)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0002tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0002row As DataRow

        O_RTN = C_MESSAGE_NO.NORMAL

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0002row = WW_TA0002tbl.NewRow
            WW_TA0002row.ItemArray = IO_TBL.Rows(i).ItemArray

            If WW_TA0002row("HDKBN") <> "H" Then Continue For

            If WW_TA0002row("RECODEKBN") = "0" Then

                '--------------------------------------
                '日報項目編集（日報が存在する場合の編集）
                '--------------------------------------
                GetT00005(WW_TA0002row("WORKDATE"), WW_TA0002row("WORKDATE"), WW_TA0002row("STAFFCODE"), T0005tbl, WW_ERRCODE)
                If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
                    O_RTN = WW_ERRCODE
                    Exit Sub
                End If

                Dim WW_F1CNT As Integer = 0
                Dim WW_F3CNT As Integer = 0
                Dim WW_KAIJI As Integer = 0
                Dim WW_B3CNT As Integer = 0
                Dim WW_SHUKOTIME As String = ""
                Dim WW_KIKOTIME As String = ""
                Dim WW_GSHABAN As String = ""
                Dim WW_PRODUCT1 As String = ""
                Dim WW_PRODUCT1NAME As String = ""
                Dim WW_SURYO As Double = 0
                Dim WW_DISTANCE As Double = 0
                Dim WW_MOVETIME As Integer = 0
                Dim WW_BREAKTIME As Integer = 0
                Dim WW_TRIPNO As String = ""

                For Each T0005row As DataRow In T0005tbl.Rows
                    'F1～F3が複数発生する（考慮が必要！）

                    If T0005row("WORKKBN") = "F1" Then
                        '出庫時刻の取得
                        WW_SHUKOTIME = T0005row("STTIME")
                        WW_F1CNT += 1
                    End If

                    If WW_F1CNT > 0 Then
                        'MOVETIME集計（ﾊﾝﾄﾞﾙ時間）
                        WW_MOVETIME += T0005row("MOVETIME")
                    End If

                    If T0005row("WORKKBN") = "B3" Then
                        If T0005row("SUISOKBN") <> "1" Then
                            'カウント（届数）
                            WW_B3CNT += 1
                            '数量合計の集計 
                            WW_SURYO += T0005row("TOTALSURYO")
                        End If
                        'カウント（回転）
                        If WW_TRIPNO <> T0005row("TRIPNO") Then
                            WW_TRIPNO = T0005row("TRIPNO")
                            WW_KAIJI += 1
                        End If
                    End If

                    If T0005row("WORKKBN") = "F3" Then
                        WW_F1CNT = 0

                        '業務車番の取得
                        WW_GSHABAN = T0005row("GSHABAN")
                        '油種区分の取得
                        WW_PRODUCT1 = T0005row("OILPAYKBN")
                        WW_PRODUCT1NAME = T0005row("OILPAYKBNNAMES")

                        WW_F3CNT += 1
                        '帰庫時刻の取得
                        WW_KIKOTIME = T0005row("ENDTIME")
                        '走行キロの取得
                        If T0005row("L1KAISO") <> "回送" OrElse T0005row("SUISOKBN") = "1" Then
                            WW_DISTANCE = T0005row("SOUDISTANCE")
                        Else
                            WW_DISTANCE = 0
                        End If
                        '-----------------------------------
                        '日報項目編集＆出力
                        '-----------------------------------
                        If WW_F3CNT = 1 Then
                            For j As Integer = 0 To T0005tbl.Rows.Count - 1
                                If T0005tbl.Rows(j)("WORKKBN") = "BB" Then
                                    '休憩
                                    WW_BREAKTIME += T0005row("WORKTIME")
                                End If
                            Next
                        End If

                        If WW_F3CNT > 1 Then
                            '２両目以降の場合、レコードコピーして追加
                            WW_TA0002row = WW_TA0002tbl.NewRow
                            SetRowSpece(WW_TA0002row)
                        End If

                        WW_TA0002row("SHARYOKBN") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBN_TXT") = T0005row("SHARYOKBN")
                        WW_TA0002row("SHARYOKBNNAMES") = T0005row("SHARYOKBNNAMES")
                        WW_TA0002row("RYOME") = WW_F3CNT
                        WW_TA0002row("PRODUCT1") = WW_PRODUCT1
                        WW_TA0002row("GSHABAN") = WW_GSHABAN
                        WW_TA0002row("GSHABAN_TXT") = WW_GSHABAN
                        If WW_PRODUCT1 = "" AndAlso WW_PRODUCT1NAME = "" Then
                            WW_TA0002row("PRODUCT1_TXT") = ""
                        Else
                            WW_TA0002row("PRODUCT1_TXT") = WW_TA0002row("PRODUCT1NAMES") & "(" & WW_PRODUCT1 & ")"
                        End If
                        WW_TA0002row("PRODUCT1NAMES") = WW_PRODUCT1NAME
                        If IsDate(WW_SHUKOTIME) Then
                            WW_TA0002row("SHUKOTIME") = CDate(WW_SHUKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("SHUKOTIME") = ""
                        End If
                        If IsDate(WW_KIKOTIME) Then
                            WW_TA0002row("KIKOTIME") = CDate(WW_KIKOTIME).ToString("HH:mm")
                        Else
                            WW_TA0002row("KIKOTIME") = ""
                        End If
                        WW_TA0002row("HANDLETIME") = MinituesToHHMM(Val(WW_MOVETIME))
                        If WW_MOVETIME > 540 Then
                            If WW_TA0002row("RECODEKBN") = "0" Then
                                '15時間を超える場合
                                WW_TA0002row("ORVER09") = "*"
                                WW_TA0002row("ORVER09_TXT") = "*"
                            Else
                                WW_TA0002row("ORVER09") = ""
                                WW_TA0002row("ORVER09_TXT") = ""
                            End If
                        End If
                        'WW_TA0002row("TRIPNO") = Val(WW_TRIPNO).ToString("#")
                        If WW_B3CNT = 0 Then
                            WW_TA0002row("TRIPNO") = Val(0).ToString("#")
                        Else
                            WW_TA0002row("TRIPNO") = Val(WW_KAIJI).ToString("#")
                        End If
                        WW_TA0002row("SURYO") = ZeroToSpace(Val(WW_SURYO).ToString("#0.000"))

                        WW_TA0002row("HAIDISTANCE") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCECHO") = Val(WW_DISTANCE).ToString("#")
                        WW_TA0002row("HAIDISTANCETTL") = Val(WW_DISTANCE).ToString("#")

                        WW_TA0002row("UNLOADCNT") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))
                        WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(Val(WW_B3CNT).ToString("#"))

                        If WW_F3CNT = 1 Then
                            '--------------------------------------
                            '勤務状況リスト編集 
                            '--------------------------------------
                            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

                            If WW_TA0002row("PAYKBN") = "00" Then
                                WW_TA0002row("PAYKBN_TXT") = ""
                                WW_TA0002row("PAYKBNNAMES") = ""
                            End If

                            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
                            End If

                            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                                WW_TA0002row("HOLIDAYKBNNAMES") = ""
                            End If

                            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
                            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
                            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                                WW_TA0002row("STDATE") = ""
                                WW_TA0002row("ENDDATE") = ""
                            End If

                            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
                            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
                            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
                            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                                If WW_TA0002row("RECODEKBN") = "0" Then
                                    '16時間を超える場合
                                    WW_TA0002row("ORVER15") = "*"
                                    WW_TA0002row("ORVER15_TXT") = "*"
                                Else
                                    WW_TA0002row("ORVER15") = ""
                                    WW_TA0002row("ORVER15_TXT") = ""
                                End If
                            End If

                            '休憩（特別編集）
                            WW_TA0002row("BREAKTIME") = HHMMtoMinutes(WW_TA0002row("BREAKTIME")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIMETTL") = HHMMtoMinutes(WW_TA0002row("BREAKTIMETTL")) + WW_BREAKTIME
                            WW_TA0002row("BREAKTIME") = MinituesToHHMM(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMETTL") = MinituesToHHMM(WW_TA0002row("BREAKTIMETTL"))

                            '1:法定休日、2:法定外休日
                            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
                              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                                WW_TA0002row("BINDTIME") = ""
                            Else
                                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
                            End If
                            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
                            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
                            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
                            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
                            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
                            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
                            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
                            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
                            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
                            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
                            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
                            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
                            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
                            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
                            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
                            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
                            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
                            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
                            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
                            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
                            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
                            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
                            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
                            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
                            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
                            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
                            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
                            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
                            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
                            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
                            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
                            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
                            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
                            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
                            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
                            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
                            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
                            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
                            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
                            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
                            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
                            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
                            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
                            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
                            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
                            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
                            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
                            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
                            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
                            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
                            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
                            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
                            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

                            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
                            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))
                            WW_TA0002row("SHACHUHAKNISSUTTL") = ZeroToSpace(WW_TA0002row("SHACHUHAKNISSUTTL"))
                            WW_TA0002row("SENJYOCNTTTL") = ZeroToSpace(WW_TA0002row("SENJYOCNTTTL"))
                            WW_TA0002row("UNLOADADDCNT1TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT1TTL"))
                            WW_TA0002row("UNLOADADDCNT2TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT2TTL"))
                            WW_TA0002row("UNLOADADDCNT3TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT3TTL"))
                            WW_TA0002row("UNLOADADDCNT4TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT4TTL"))
                            WW_TA0002row("LOADINGCNT1TTL") = ZeroToSpace(WW_TA0002row("LOADINGCNT1TTL"))
                            WW_TA0002row("LOADINGCNT2TTL") = ZeroToSpace(WW_TA0002row("LOADINGCNT2TTL"))
                            WW_TA0002row("SHORTDISTANCE1TTL") = ZeroToSpace(WW_TA0002row("SHORTDISTANCE1TTL"))
                            WW_TA0002row("SHORTDISTANCE2TTL") = ZeroToSpace(WW_TA0002row("SHORTDISTANCE2TTL"))
                        End If

                        WW_TA0002tbl.Rows.Add(WW_TA0002row)

                        WW_SHUKOTIME = ""
                        WW_KIKOTIME = ""
                        WW_GSHABAN = ""
                        WW_PRODUCT1 = ""
                        WW_PRODUCT1NAME = ""
                        WW_SURYO = 0
                        WW_DISTANCE = 0
                        WW_B3CNT = 0
                        WW_MOVETIME = 0
                        WW_KAIJI = 0
                        WW_TRIPNO = ""
                    End If

                Next
                '日報が存在する場合、下記の編集（勤怠項目のみ：上記の編集と同じ）は行わない
                If T0005tbl.Rows.Count > 0 Then
                    Continue For
                End If
            End If

            '---------------------------------------------------
            '勤務状況リスト編集 （日報が存在しない場合の編集）
            '---------------------------------------------------
            WW_TA0002row("TAISHOYM_TXT") = Mid(WW_TA0002row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0002row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0002row("PAYKBN") = "00" Then
                WW_TA0002row("PAYKBN_TXT") = ""
                WW_TA0002row("PAYKBNNAMES") = ""
            End If

            If WW_TA0002row("SHUKCHOKKBN") = "0" Then
                WW_TA0002row("SHUKCHOKKBN_TXT") = ""
                WW_TA0002row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0002row("HOLIDAYKBN") = "0" Then
                WW_TA0002row("HOLIDAYKBN_TXT") = ""
                WW_TA0002row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0002row("STTIME") = ZeroToSpace(WW_TA0002row("STTIME"))
            WW_TA0002row("ENDTIME") = ZeroToSpace(WW_TA0002row("ENDTIME"))
            If WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "" Then
                WW_TA0002row("STDATE") = ""
                WW_TA0002row("ENDDATE") = ""
            End If

            WW_TA0002row("WORKTIME") = ZeroToSpace(WW_TA0002row("WORKTIME"))
            WW_TA0002row("MOVETIME") = ZeroToSpace(WW_TA0002row("MOVETIME"))
            WW_TA0002row("ACTTIME") = ZeroToSpace(WW_TA0002row("ACTTIME"))
            If HHMMtoMinutes(WW_TA0002row("ACTTIME")) >= 960 Then
                If WW_TA0002row("RECODEKBN") = "0" Then
                    '16時間を超える場合
                    WW_TA0002row("ORVER15") = "*"
                    WW_TA0002row("ORVER15_TXT") = "*"
                Else
                    WW_TA0002row("ORVER15") = ""
                    WW_TA0002row("ORVER15_TXT") = ""
                End If
            End If

            '1:法定休日、2:法定外休日
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If T0007COM.CheckHOLIDAY(WW_TA0002row("HOLIDAYKBN"), WW_TA0002row("PAYKBN")) = True Or
              (WW_TA0002row("STTIME") = "" AndAlso WW_TA0002row("ENDTIME") = "") Then
                WW_TA0002row("BINDTIME") = ""
            Else
                WW_TA0002row("BINDTIME") = ZeroToSpace(WW_TA0002row("BINDTIME"))
            End If
            WW_TA0002row("BINDSTDATE") = ZeroToSpace(WW_TA0002row("BINDSTDATE"))
            WW_TA0002row("BREAKTIME") = ZeroToSpace(WW_TA0002row("BREAKTIME"))
            WW_TA0002row("BREAKTIMECHO") = ZeroToSpace(WW_TA0002row("BREAKTIMECHO"))
            WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(WW_TA0002row("BREAKTIMETTL"))
            WW_TA0002row("NIGHTTIME") = ZeroToSpace(WW_TA0002row("NIGHTTIME"))
            WW_TA0002row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("NIGHTTIMECHO"))
            WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("NIGHTTIMETTL"))
            WW_TA0002row("ORVERTIME") = ZeroToSpace(WW_TA0002row("ORVERTIME"))
            WW_TA0002row("ORVERTIMECHO") = ZeroToSpace(WW_TA0002row("ORVERTIMECHO"))
            WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(WW_TA0002row("ORVERTIMETTL"))
            WW_TA0002row("WNIGHTTIME") = ZeroToSpace(WW_TA0002row("WNIGHTTIME"))
            WW_TA0002row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("WNIGHTTIMECHO"))
            WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("WNIGHTTIMETTL"))
            WW_TA0002row("SWORKTIME") = ZeroToSpace(WW_TA0002row("SWORKTIME"))
            WW_TA0002row("SWORKTIMECHO") = ZeroToSpace(WW_TA0002row("SWORKTIMECHO"))
            WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(WW_TA0002row("SWORKTIMETTL"))
            WW_TA0002row("SNIGHTTIME") = ZeroToSpace(WW_TA0002row("SNIGHTTIME"))
            WW_TA0002row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("SNIGHTTIMECHO"))
            WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("SNIGHTTIMETTL"))
            WW_TA0002row("HWORKTIME") = ZeroToSpace(WW_TA0002row("HWORKTIME"))
            WW_TA0002row("HWORKTIMECHO") = ZeroToSpace(WW_TA0002row("HWORKTIMECHO"))
            WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(WW_TA0002row("HWORKTIMETTL"))
            WW_TA0002row("HNIGHTTIME") = ZeroToSpace(WW_TA0002row("HNIGHTTIME"))
            WW_TA0002row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0002row("HNIGHTTIMECHO"))
            WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0002row("HNIGHTTIMETTL"))
            WW_TA0002row("HOANTIME") = ZeroToSpace(WW_TA0002row("HOANTIME"))
            WW_TA0002row("HOANTIMECHO") = ZeroToSpace(WW_TA0002row("HOANTIMECHO"))
            WW_TA0002row("HOANTIMETTL") = ZeroToSpace(WW_TA0002row("HOANTIMETTL"))
            WW_TA0002row("KOATUTIME") = ZeroToSpace(WW_TA0002row("KOATUTIME"))
            WW_TA0002row("KOATUTIMECHO") = ZeroToSpace(WW_TA0002row("KOATUTIMECHO"))
            WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(WW_TA0002row("KOATUTIMETTL"))
            WW_TA0002row("TOKUSA1TIME") = ZeroToSpace(WW_TA0002row("TOKUSA1TIME"))
            WW_TA0002row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMECHO"))
            WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0002row("TOKUSA1TIMETTL"))
            WW_TA0002row("TOKSAAKAISU") = ZeroToSpace(WW_TA0002row("TOKSAAKAISU"))
            WW_TA0002row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUCHO"))
            WW_TA0002row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSAAKAISUTTL"))
            WW_TA0002row("TOKSABKAISU") = ZeroToSpace(WW_TA0002row("TOKSABKAISU"))
            WW_TA0002row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSABKAISUCHO"))
            WW_TA0002row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSABKAISUTTL"))
            WW_TA0002row("TOKSACKAISU") = ZeroToSpace(WW_TA0002row("TOKSACKAISU"))
            WW_TA0002row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0002row("TOKSACKAISUCHO"))
            WW_TA0002row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0002row("TOKSACKAISUTTL"))
            WW_TA0002row("TENKOKAISU") = ZeroToSpace(WW_TA0002row("TENKOKAISU"))
            WW_TA0002row("TENKOKAISUCHO") = ZeroToSpace(WW_TA0002row("TENKOKAISUCHO"))
            WW_TA0002row("TENKOKAISUTTL") = ZeroToSpace(WW_TA0002row("TENKOKAISUTTL"))
            WW_TA0002row("HAIDISTANCE") = ZeroToSpace(WW_TA0002row("HAIDISTANCE"))
            WW_TA0002row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0002row("HAIDISTANCECHO"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0002row("HAIDISTANCETTL"))
            WW_TA0002row("UNLOADCNT") = ZeroToSpace(WW_TA0002row("UNLOADCNT"))
            WW_TA0002row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0002row("UNLOADCNTCHO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0002row("UNLOADCNTTTL"))
            WW_TA0002row("SURYO") = ZeroToSpace(WW_TA0002row("SURYO"))

            WW_TA0002row("HAISOTIME") = ZeroToSpace(WW_TA0002row("HAISOTIME"))
            WW_TA0002row("SHACHUHAKKBN") = ZeroToSpace(WW_TA0002row("SHACHUHAKKBN"))
            WW_TA0002row("SHACHUHAKNISSUTTL") = ZeroToSpace(WW_TA0002row("SHACHUHAKNISSUTTL"))
            WW_TA0002row("SENJYOCNTTTL") = ZeroToSpace(WW_TA0002row("SENJYOCNTTTL"))
            WW_TA0002row("UNLOADADDCNT1TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT1TTL"))
            WW_TA0002row("UNLOADADDCNT2TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT2TTL"))
            WW_TA0002row("UNLOADADDCNT3TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT3TTL"))
            WW_TA0002row("UNLOADADDCNT4TTL") = ZeroToSpace(WW_TA0002row("UNLOADADDCNT4TTL"))
            WW_TA0002row("LOADINGCNT1TTL") = ZeroToSpace(WW_TA0002row("LOADINGCNT1TTL"))
            WW_TA0002row("LOADINGCNT2TTL") = ZeroToSpace(WW_TA0002row("LOADINGCNT2TTL"))
            WW_TA0002row("SHORTDISTANCE1TTL") = ZeroToSpace(WW_TA0002row("SHORTDISTANCE1TTL"))
            WW_TA0002row("SHORTDISTANCE2TTL") = ZeroToSpace(WW_TA0002row("SHORTDISTANCE2TTL"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '----------------------------------
        '合計行（勤怠側）の編集
        '----------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        Dim WW_BINDTIME As Integer = 0
        Dim WW_ACTTIME As Integer = 0
        Dim WW_BREAKTIME2 As Integer = 0
        Dim WW_ORVERTIME As Integer = 0
        Dim WW_WNIGHTTIME As Integer = 0
        Dim WW_HWORKTIME As Integer = 0
        Dim WW_HNIGHTTIME As Integer = 0
        Dim WW_SWORKTIME As Integer = 0
        Dim WW_SNIGHTTIME As Integer = 0
        Dim WW_NIGHTTIME As Integer = 0
        Dim WW_TOKUSA1TIME As Integer = 0
        Dim WW_HOANTIME As Integer = 0
        Dim WW_KOATUTIME As Integer = 0
        Dim WW_SENJYOCNT As Integer = 0
        Dim WW_SHACHUHAKCNT As Integer = 0
        Dim WW_UNLOADADDCNT1 As Integer = 0
        Dim WW_UNLOADADDCNT2 As Integer = 0
        Dim WW_UNLOADADDCNT3 As Integer = 0
        Dim WW_UNLOADADDCNT4 As Integer = 0
        Dim WW_LOADINGCNT1 As Integer = 0
        Dim WW_LOADINGCNT2 As Integer = 0
        Dim WW_SHORTDISTANCE1 As Integer = 0
        Dim WW_SHORTDISTANCE2 As Integer = 0

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            WW_BINDTIME += HHMMtoMinutes(TA0002Row("BINDTIME"))
            WW_ACTTIME += HHMMtoMinutes(TA0002Row("ACTTIME"))
            WW_BREAKTIME2 += HHMMtoMinutes(TA0002Row("BREAKTIMETTL"))
            WW_ORVERTIME += HHMMtoMinutes(TA0002Row("ORVERTIMETTL"))
            WW_WNIGHTTIME += HHMMtoMinutes(TA0002Row("WNIGHTTIMETTL"))
            WW_HWORKTIME += HHMMtoMinutes(TA0002Row("HWORKTIMETTL"))
            WW_HNIGHTTIME += HHMMtoMinutes(TA0002Row("HNIGHTTIMETTL"))
            WW_SWORKTIME += HHMMtoMinutes(TA0002Row("SWORKTIMETTL"))
            WW_SNIGHTTIME += HHMMtoMinutes(TA0002Row("SNIGHTTIMETTL"))
            WW_NIGHTTIME += HHMMtoMinutes(TA0002Row("NIGHTTIMETTL"))
            WW_TOKUSA1TIME += HHMMtoMinutes(TA0002Row("TOKUSA1TIMETTL"))
            WW_HOANTIME += HHMMtoMinutes(TA0002Row("HOANTIMETTL"))
            WW_KOATUTIME += HHMMtoMinutes(TA0002Row("KOATUTIMETTL"))
            WW_SENJYOCNT += Val(TA0002Row("SENJYOCNTTTL"))
            WW_SHACHUHAKCNT += Val(TA0002Row("SHACHUHAKNISSUTTL"))
            WW_UNLOADADDCNT1 += Val(TA0002Row("UNLOADADDCNT1TTL"))
            WW_UNLOADADDCNT2 += Val(TA0002Row("UNLOADADDCNT2TTL"))
            WW_UNLOADADDCNT3 += Val(TA0002Row("UNLOADADDCNT3TTL"))
            WW_UNLOADADDCNT4 += Val(TA0002Row("UNLOADADDCNT4TTL"))
            WW_LOADINGCNT1 += Val(TA0002Row("LOADINGCNT1TTL"))
            WW_LOADINGCNT2 += Val(TA0002Row("LOADINGCNT2TTL"))
            WW_SHORTDISTANCE1 += Val(TA0002Row("SHORTDISTANCE1TTL"))
            WW_SHORTDISTANCE2 += Val(TA0002Row("SHORTDISTANCE2TTL"))
        Next

        '合計の場合
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002row("PAYKBNNAMES") = "合計"
        WW_TA0002row("PAYKBN_TXT") = "合計"

        WW_TA0002row("BINDTIME") = ZeroToSpace(MinituesToHHMM(WW_BINDTIME))
        WW_TA0002row("ACTTIME") = ZeroToSpace(MinituesToHHMM(WW_ACTTIME))
        WW_TA0002row("BREAKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_BREAKTIME2))
        WW_TA0002row("ORVERTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_ORVERTIME))
        WW_TA0002row("WNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_WNIGHTTIME))
        WW_TA0002row("HWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HWORKTIME))
        WW_TA0002row("HNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HNIGHTTIME))
        WW_TA0002row("SWORKTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SWORKTIME))
        WW_TA0002row("SNIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_SNIGHTTIME))
        WW_TA0002row("NIGHTTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_NIGHTTIME))
        WW_TA0002row("TOKUSA1TIMETTL") = ZeroToSpace(MinituesToHHMM(WW_TOKUSA1TIME))
        WW_TA0002row("HOANTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_HOANTIME))
        WW_TA0002row("KOATUTIMETTL") = ZeroToSpace(MinituesToHHMM(WW_KOATUTIME))
        WW_TA0002row("SENJYOCNTTTL") = ZeroToSpace(WW_SENJYOCNT)
        WW_TA0002row("SHACHUHAKNISSUTTL") = ZeroToSpace(WW_SHACHUHAKCNT)
        WW_TA0002row("UNLOADADDCNT1TTL") = ZeroToSpace(WW_UNLOADADDCNT1)
        WW_TA0002row("UNLOADADDCNT2TTL") = ZeroToSpace(WW_UNLOADADDCNT2)
        WW_TA0002row("UNLOADADDCNT3TTL") = ZeroToSpace(WW_UNLOADADDCNT3)
        WW_TA0002row("UNLOADADDCNT4TTL") = ZeroToSpace(WW_UNLOADADDCNT4)
        WW_TA0002row("LOADINGCNT1TTL") = ZeroToSpace(WW_LOADINGCNT1)
        WW_TA0002row("LOADINGCNT2TTL") = ZeroToSpace(WW_LOADINGCNT2)
        WW_TA0002row("SHORTDISTANCE1TTL") = ZeroToSpace(WW_SHORTDISTANCE1)
        WW_TA0002row("SHORTDISTANCE2TTL") = ZeroToSpace(WW_SHORTDISTANCE2)

        WW_TA0002tbl.Rows.Add(WW_TA0002row)


        '----------------------------------
        '合計行（日報側）の編集
        '----------------------------------
        'テンポラリDB項目作成
        Dim WW_NIPPOtbl As DataTable = New DataTable
        WW_NIPPOtbl.Clear()
        WW_NIPPOtbl.Columns.Add("KEY", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBN", GetType(String))
        WW_NIPPOtbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1", GetType(String))
        WW_NIPPOtbl.Columns.Add("PRODUCT1NAMES", GetType(String))
        WW_NIPPOtbl.Columns.Add("TRIPNO", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("UNLOADCNT", GetType(Integer))
        WW_NIPPOtbl.Columns.Add("SURYO", GetType(Double))
        WW_NIPPOtbl.Columns.Add("HAIDISTANCE", GetType(Double))

        For Each TA0002Row As DataRow In WW_TA0002tbl.Rows
            If TA0002Row("PRODUCT1") = "" Then
                Continue For
            End If

            Dim WW_NIPPOrow As DataRow
            WW_NIPPOrow = WW_NIPPOtbl.NewRow

            WW_NIPPOrow("KEY") = "1"
            WW_NIPPOrow("SHARYOKBN") = TA0002Row("SHARYOKBN")
            WW_NIPPOrow("SHARYOKBNNAMES") = TA0002Row("SHARYOKBNNAMES")
            WW_NIPPOrow("PRODUCT1") = TA0002Row("PRODUCT1")
            WW_NIPPOrow("PRODUCT1NAMES") = TA0002Row("PRODUCT1NAMES")
            WW_NIPPOrow("TRIPNO") = Val(TA0002Row("TRIPNO"))
            WW_NIPPOrow("UNLOADCNT") = Val(TA0002Row("UNLOADCNT"))
            WW_NIPPOrow("SURYO") = Val(TA0002Row("SURYO"))
            WW_NIPPOrow("HAIDISTANCE") = Val(TA0002Row("HAIDISTANCE"))

            WW_NIPPOtbl.Rows.Add(WW_NIPPOrow)

        Next

        '油種別合計行（日報側）の編集
        Dim viw As New DataView(WW_NIPPOtbl)
        Dim isDistinct As Boolean = True
        Dim cols() As String = {"KEY", "PRODUCT1", "PRODUCT1NAMES"}
        viw.Sort = "KEY, PRODUCT1"
        Dim dtFilter As DataTable = viw.ToTable(isDistinct, cols)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}' AND PRODUCT1 = '{1}' AND PRODUCT1NAMES = '{2}'", row("KEY"), row("PRODUCT1"), row("PRODUCT1NAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = row("PRODUCT1")
            WW_TA0002row("PRODUCT1NAMES") = row("PRODUCT1NAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next


        '油種合計行（日報側）の編集
        Dim cols2() As String = {"KEY"}
        dtFilter = viw.ToTable(isDistinct, cols2)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("KEY = '{0}'", row("KEY"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("PRODUCT1_TXT") = "合計"
            WW_TA0002row("PRODUCT1NAMES") = "合計"
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        '---------------------------------
        '車種別合計行（日報側）の編集
        '---------------------------------
        '１行空白行を設定
        WW_TA0002row = WW_TA0002tbl.NewRow
        SetRowSpece(WW_TA0002row)
        WW_TA0002tbl.Rows.Add(WW_TA0002row)

        '車種別合計行（日報側）の編集
        Dim cols3() As String = {"SHARYOKBN", "SHARYOKBNNAMES"}
        dtFilter = viw.ToTable(isDistinct, cols3)
        dtFilter.Columns.Add("TRIPNO", GetType(Integer))
        dtFilter.Columns.Add("UNLOADCNT", GetType(Integer))
        dtFilter.Columns.Add("SURYO", GetType(Double))
        dtFilter.Columns.Add("HAIDISTANCE", GetType(Double))
        For Each row As DataRow In dtFilter.Rows
            Dim expr As String = String.Format("SHARYOKBN = '{0}' AND SHARYOKBNNAMES = '{1}'", row("SHARYOKBN"), row("SHARYOKBNNAMES"))
            row("TRIPNO") = WW_NIPPOtbl.Compute("SUM(TRIPNO)", expr)
            row("UNLOADCNT") = WW_NIPPOtbl.Compute("SUM(UNLOADCNT)", expr)
            row("SURYO") = WW_NIPPOtbl.Compute("SUM(SURYO)", expr)
            row("HAIDISTANCE") = WW_NIPPOtbl.Compute("SUM(HAIDISTANCE)", expr)
        Next

        For Each row As DataRow In dtFilter.Rows
            WW_TA0002row = WW_TA0002tbl.NewRow

            SetRowSpece(WW_TA0002row)

            WW_TA0002row("GSHABAN") = row("SHARYOKBNNAMES")
            WW_TA0002row("GSHABAN_TXT") = row("SHARYOKBNNAMES")
            WW_TA0002row("TRIPNO") = ZeroToSpace(row("TRIPNO"))
            WW_TA0002row("UNLOADCNTTTL") = ZeroToSpace(row("UNLOADCNT"))
            WW_TA0002row("SURYO") = ZeroToSpace(Val(row("SURYO")).ToString("#0.000"))
            WW_TA0002row("HAIDISTANCETTL") = ZeroToSpace(Val(row("HAIDISTANCE")).ToString("#0"))

            WW_TA0002tbl.Rows.Add(WW_TA0002row)
        Next

        IO_TBL = WW_TA0002tbl.Copy

        WW_TA0002tbl.Dispose()
        WW_TA0002tbl = Nothing

    End Sub

    ''' <summary>
    ''' スペース行作成処理
    ''' </summary>
    ''' <param name="IO_ROW">空白行を設定する行情報</param>
    Protected Sub SetRowSpece(ByRef IO_ROW As DataRow)

        For Each col As DataColumn In IO_ROW.Table.Columns
            If col.DataType.Name.ToString = "String" Then
                IO_ROW(col.ColumnName) = String.Empty
            End If
        Next

    End Sub

    ''' <summary>
    ''' セレクター初期設定
    ''' </summary>
    Protected Sub InitialSelector()

        Dim WW_TBLview As DataView
        Dim WW_GRPtbl As DataTable

        If IsNothing(SELECTORtbl) Then SELECTORtbl = New DataTable
        'テンポラリDB項目作成
        SELECTORtbl.Clear()
        SELECTORtbl.Columns.Add("CODE", GetType(String))                        'CODE               コード
        SELECTORtbl.Columns.Add("NAME", GetType(String))                        'NAME               名称
        SELECTORtbl.Columns.Add("ORGSEQ", GetType(Integer))                     'ORGSEQ             順番


        Dim WW_Cols As String() = {"STAFFCODE", "STAFFCODE_TXT", "ORGSEQ"}
        WW_TBLview = New DataView(TA0002ALL)
        WW_TBLview.Sort = "STAFFCODE"
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        For Each TA0002ALLrow As DataRow In WW_GRPtbl.Rows
            Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0002ALLrow("STAFFCODE")
            SELECTORrow("NAME") = TA0002ALLrow("STAFFCODE_TXT")
            SELECTORrow("ORGSEQ") = TA0002ALLrow("ORGSEQ")
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.SORTING = "ORGSEQ, CODE, NAME"
        CS0026TblSort.FILTER = ""
        SELECTORtbl = CS0026TblSort.sort()

        '●セレクター設定処理
        WF_SELECTOR.DataSource = SELECTORtbl
        WF_SELECTOR.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WF_SELECTOR_Posi.Value = ""
        Else
            WF_SELECTOR_Posi.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '値　
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text = SELECTORtbl.Rows(i)("CODE")
            'テキスト
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Text = "　" & SELECTORtbl.Rows(i)("NAME")

            '背景色
            If CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTOR_Posi.Value Then
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If

            'イベント追加
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Attributes.Remove("onclick")
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Attributes.Add("onclick", "SELECTOR_Click('" & SELECTORtbl.Rows(i)("CODE") & "');")
        Next

        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_GRPtbl.Dispose()
        WW_GRPtbl = Nothing

    End Sub

    ''' <summary>
    ''' セレクタークリック(選択変更)処理
    ''' </summary>
    Protected Sub SELECTOR_Click()

        '■ データリカバリ
        '○ TA0002ALLデータリカバリ
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If

        '■ セレクター表示切替
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '背景色
            If CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTOR_Posi.Value Then
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If
        Next

        '■ GridView表示データ作成

        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

    End Sub

    ''' <summary>
    ''' GridView 明細行ダブルクリック時処理
    ''' </summary>
    Protected Sub WF_Grid_DBclick()

        '○ テーブルデータ 復元(Xmlファイルより復元)
        If IsNothing(TA0002ALL) Then
            If Not Master.RecoverTable(TA0002ALL) Then Exit Sub
        End If
        '○TA0002VIEWtbl取得
        GetViewTA0002(WF_SELECTOR_Posi.Value)

        '対象データ抽出(指定日入力）
        Dim WW_WORKDATE As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_NIPPOLINKCODE As String = ""

        Dim WW_TA002tbl As DataTable = TA0002VIEWtbl.Clone
        Dim WW_TA002DTLtbl As DataTable = TA0002VIEWtbl.Clone
        Dim WW_FILTER As String = ""
        WW_FILTER = WW_FILTER & "LINECNT  = '" & WF_GridDBclick.Text & "' and "
        WW_FILTER = WW_FILTER & "SELECT    = '1' and RECODEKBN = '0'"
        CS0026TblSort.TABLE = TA0002VIEWtbl
        CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TblSort.FILTER = WW_FILTER
        WW_TA002tbl = CS0026TblSort.sort()

        '日報取得
        If WW_TA002tbl.Rows.Count > 0 Then
            WW_WORKDATE = WW_TA002tbl.Rows(0)("WORKDATE")
            WW_STAFFCODE = WW_TA002tbl.Rows(0)("STAFFCODE")
            WW_NIPPOLINKCODE = WW_TA002tbl.Rows(0)("NIPPOLINKCODE")

            WW_FILTER = ""
            WW_FILTER = WW_FILTER & "STAFFCODE  = '" & WW_TA002tbl.Rows(0)("STAFFCODE") & "' and "
            WW_FILTER = WW_FILTER & "WORKDATE   = '" & WW_TA002tbl.Rows(0)("WORKDATE") & "' and "
            WW_FILTER = WW_FILTER & "HDKBN      = 'D' "
            CS0026TblSort.TABLE = TA0002VIEWtbl
            CS0026TblSort.SORTING = "SELECT, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            CS0026TblSort.FILTER = WW_FILTER
            WW_TA002DTLtbl = CS0026TblSort.sort()
        Else
            WW_WORKDATE = ""
            WW_STAFFCODE = ""
            WW_NIPPOLINKCODE = ""
        End If

        GetNIPPO(T0005tbl, WW_WORKDATE, WW_STAFFCODE, WW_NIPPOLINKCODE, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        WW_TA002tbl.Merge(WW_TA002DTLtbl)
        WW_TA002tbl.Merge(T0005tbl)

        For Each TA002INProw As DataRow In WW_TA002tbl.Rows

            '指定日入力（編集）
            If TA002INProw("RECODEKBN") = "0" Then
                If TA002INProw("HDKBN") = "H" Then
                    '共通
                    WF_STAFFCODE.Text = TA002INProw("STAFFCODE")
                    WF_STAFFCODE_TEXT.Text = TA002INProw("STAFFNAMES")
                    WF_HORG.Text = TA002INProw("HORG")
                    WF_HORG_TEXT.Text = TA002INProw("HORGNAMES")
                    WF_WORKDATE.Text = TA002INProw("WORKDATE")
                    WF_WORKINGWEEK_TEXT.Text = TA002INProw("WORKINGWEEKNAMES")

                    'エネックス
                    If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_ENEX Then
                        WF_HOLIDAYKBN.Text = TA002INProw("HOLIDAYKBN")
                        WF_HOLIDAYKBN_TEXT.Text = TA002INProw("HOLIDAYKBNNAMES")
                        WF_PAYKBN.Text = TA002INProw("PAYKBN")
                        WF_PAYKBN_TEXT.Text = TA002INProw("PAYKBNNAMES")
                        WF_SHUKCHOKKBN.Text = TA002INProw("SHUKCHOKKBN")
                        WF_SHUKCHOKKBN_TEXT.Text = TA002INProw("SHUKCHOKKBNNAMES")

                        WF_STDATE.Text = TA002INProw("STDATE")
                        WF_STTIME.Text = TA002INProw("STTIME")
                        WF_BINDSTDATE.Text = TA002INProw("BINDSTDATE")
                        WF_BINDTIME.Text = TA002INProw("BINDTIME")
                        WF_ENDDATE.Text = TA002INProw("ENDDATE")
                        WF_ENDTIME.Text = TA002INProw("ENDTIME")

                        WF_NIPPOBREAKTIME.Text = TA002INProw("NIPPOBREAKTIME")
                        WF_BREAKTIME.Text = TA002INProw("BREAKTIME")
                        WF_TOKUSA1TIME.Text = TA002INProw("TOKUSA1TIMETTL")
                        WF_HOANTIME.Text = TA002INProw("HOANTIMETTL")
                        WF_KOATUTIME.Text = TA002INProw("KOATUTIMETTL")
                        WF_HAYADETIME.Text = TA002INProw("HAYADETIMETTL")

                        WF_TOKSAAKAISU.Text = TA002INProw("TOKSAAKAISUTTL")
                        WF_TOKSABKAISU.Text = TA002INProw("TOKSABKAISUTTL")
                        WF_TOKSACKAISU.Text = TA002INProw("TOKSACKAISUTTL")
                        WF_TENKOKAISU.Text = TA002INProw("TENKOKAISUTTL")

                        WF_ORVERTIME.Text = TA002INProw("ORVERTIMETTL")
                        WF_HWORKTIME.Text = TA002INProw("HWORKTIMETTL")
                        WF_WNIGHTTIME.Text = TA002INProw("WNIGHTTIMETTL")
                        WF_HNIGHTTIME.Text = TA002INProw("HNIGHTTIMETTL")
                        WF_NIGHTTIME.Text = TA002INProw("NIGHTTIMETTL")
                        WF_SWORKTIME.Text = TA002INProw("SWORKTIMETTL")
                        WF_SNIGHTTIME.Text = TA002INProw("SNIGHTTIMETTL")

                        WF_HAIDISTANCE.Text = Val(TA002INProw("HAIDISTANCE")).ToString("0")
                        WF_KAIDISTANCE.Text = Val(TA002INProw("KAIDISTANCE")).ToString("0")
                        WF_UNLOADCNT.Text = TA002INProw("UNLOADCNTTTL")

                    End If

                    '近石
                    If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_KNK Then
                        WF_HOLIDAYKBN_KNK.Text = TA002INProw("HOLIDAYKBN")
                        WF_HOLIDAYKBN_TEXT_KNK.Text = TA002INProw("HOLIDAYKBNNAMES")
                        WF_PAYKBN_KNK.Text = TA002INProw("PAYKBN")
                        WF_PAYKBN_TEXT_KNK.Text = TA002INProw("PAYKBNNAMES")
                        WF_SHUKCHOKKBN_KNK.Text = TA002INProw("SHUKCHOKKBN")
                        WF_SHUKCHOKKBN_TEXT_KNK.Text = TA002INProw("SHUKCHOKKBNNAMES")

                        WF_STDATE_KNK.Text = TA002INProw("STDATE")
                        WF_STTIME_KNK.Text = TA002INProw("STTIME")
                        WF_BINDSTDATE_KNK.Text = TA002INProw("BINDSTDATE")
                        WF_ENDDATE_KNK.Text = TA002INProw("ENDDATE")
                        WF_ENDTIME_KNK.Text = TA002INProw("ENDTIME")
                        WF_WWORKTIME_KNK.Text = TA002INProw("WWORKTIME")
                        WF_JYOMUTIME_KNK.Text = TA002INProw("JYOMUTIME")

                        WF_NIPPOBREAKTIME_KNK.Text = TA002INProw("NIPPOBREAKTIME")
                        WF_BREAKTIME_KNK.Text = TA002INProw("BREAKTIME")
                        WF_TOKUSA1TIME_KNK.Text = TA002INProw("TOKUSA1TIMETTL")

                        WF_ORVERTIME_KNK.Text = TA002INProw("ORVERTIMETTL")
                        WF_WNIGHTTIME_KNK.Text = TA002INProw("WNIGHTTIMETTL")
                        WF_HWORKTIME_KNK.Text = TA002INProw("HWORKTIMETTL")
                        WF_HNIGHTTIME_KNK.Text = TA002INProw("HNIGHTTIMETTL")
                        WF_HDAIWORKTIME_KNK.Text = TA002INProw("HDAIWORKTIMETTL")
                        WF_HDAINIGHTTIME_KNK.Text = TA002INProw("HDAINIGHTTIMETTL")
                        WF_SWORKTIME_KNK.Text = TA002INProw("SWORKTIMETTL")
                        WF_SNIGHTTIME_KNK.Text = TA002INProw("SNIGHTTIMETTL")
                        WF_SDAIWORKTIME_KNK.Text = TA002INProw("SDAIWORKTIMETTL")
                        WF_SDAINIGHTTIME_KNK.Text = TA002INProw("SDAINIGHTTIMETTL")
                        WF_NIGHTTIME_KNK.Text = TA002INProw("NIGHTTIMETTL")

                        WF_HAIDISTANCE_KNK.Text = Val(TA002INProw("HAIDISTANCE")).ToString("0")
                        WF_KAIDISTANCE_KNK.Text = Val(TA002INProw("KAIDISTANCE")).ToString("0")
                        WF_KAITENCNT_KNK.Text = TA002INProw("KAITENCNTTTL")
                        WF_UNLOADCNT_KNK.Text = TA002INProw("UNLOADCNTTTL")
                    End If

                    'NJS
                    If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_NJS Then
                        WF_HOLIDAYKBN_NJS.Text = TA002INProw("HOLIDAYKBN")
                        WF_HOLIDAYKBN_TEXT_NJS.Text = TA002INProw("HOLIDAYKBNNAMES")
                        WF_PAYKBN_NJS.Text = TA002INProw("PAYKBN")
                        WF_PAYKBN_TEXT_NJS.Text = TA002INProw("PAYKBNNAMES")
                        WF_SHUKCHOKKBN_NJS.Text = TA002INProw("SHUKCHOKKBN")
                        WF_SHUKCHOKKBN_TEXT_NJS.Text = TA002INProw("SHUKCHOKKBNNAMES")

                        WF_STDATE_NJS.Text = TA002INProw("STDATE")
                        WF_STTIME_NJS.Text = TA002INProw("STTIME")
                        WF_BINDSTDATE_NJS.Text = TA002INProw("BINDSTDATE")
                        WF_BINDTIME_NJS.Text = TA002INProw("BINDTIME")
                        WF_ENDDATE_NJS.Text = TA002INProw("ENDDATE")
                        WF_ENDTIME_NJS.Text = TA002INProw("ENDTIME")

                        WF_NIPPOBREAKTIME_NJS.Text = TA002INProw("NIPPOBREAKTIME")
                        WF_BREAKTIME_NJS.Text = TA002INProw("BREAKTIME")
                        WF_TOKUSA1TIME.Text = TA002INProw("TOKUSA1TIMETTL")
                        WF_HAISOTIME_NJS.Text = TA002INProw("HAISOTIME")
                        If TA002INProw("SHACHUHAKKBN") = "1" Then
                            WF_SHACHUHAKKBN_NJS.Checked = True
                        Else
                            WF_SHACHUHAKKBN_NJS.Checked = False
                        End If
                        Dim WW_MODELDISTANCE109 As Integer = 0
                        Dim WW_MODELDISTANCE204 As Integer = 0
                        Dim WW_MODELDISTANCE209 As Integer = 0
                        Dim WW_MODIFY As String = "OFF"
                        For i As Integer = 1 To 6
                            Dim WW_SHARYOKBN As String = "T10SHARYOKBN" & i.ToString
                            Dim WW_OILPAYKBN As String = "T10OILPAYKBN" & i.ToString
                            Dim WW_SHUKABASHO As String = "T10SHUKABASHO" & i.ToString
                            Dim WW_TODOKECODE As String = "T10TODOKECODE" & i.ToString
                            Dim WW_MODELDISTANCE As String = "T10MODELDISTANCE" & i.ToString
                            Dim WW_MODIFYKBN As String = "T10MODIFYKBN" & i.ToString

                            If TA002INProw(WW_MODIFYKBN) = "1" Then
                                WW_MODIFY = "ON"
                            End If

                            If TA002INProw(WW_SHARYOKBN) = "1" And TA002INProw(WW_OILPAYKBN) = "09" Then
                                WW_MODELDISTANCE109 += Val(TA002INProw(WW_MODELDISTANCE))
                            End If
                            If TA002INProw(WW_SHARYOKBN) = "2" And TA002INProw(WW_OILPAYKBN) = "04" Then
                                WW_MODELDISTANCE204 += Val(TA002INProw(WW_MODELDISTANCE))
                            End If
                            If TA002INProw(WW_SHARYOKBN) = "2" And TA002INProw(WW_OILPAYKBN) = "09" Then
                                WW_MODELDISTANCE209 += Val(TA002INProw(WW_MODELDISTANCE))
                            End If

                        Next
                        If WW_MODIFY = "ON" Then
                            WF_MODIFY_NJS.Checked = True
                        Else
                            WF_MODIFY_NJS.Checked = False
                        End If
                        WF_MODELDISTANCE109_NJS.Text = Val(WW_MODELDISTANCE109).ToString("0")
                        WF_MODELDISTANCE204_NJS.Text = Val(WW_MODELDISTANCE204).ToString("0")
                        WF_MODELDISTANCE209_NJS.Text = Val(WW_MODELDISTANCE209).ToString("0")

                        WF_ORVERTIME_NJS.Text = TA002INProw("ORVERTIMETTL")
                        WF_WNIGHTTIME_NJS.Text = TA002INProw("WNIGHTTIMETTL")
                        WF_HWORKTIME_NJS.Text = TA002INProw("HWORKTIMETTL")
                        WF_HNIGHTTIME_NJS.Text = TA002INProw("HNIGHTTIMETTL")
                        WF_SWORKTIME_NJS.Text = TA002INProw("SWORKTIMETTL")
                        WF_SNIGHTTIME_NJS.Text = TA002INProw("SNIGHTTIMETTL")
                        WF_NIGHTTIME_NJS.Text = TA002INProw("NIGHTTIMETTL")
                        WF_HAIDISTANCE_NJS.Text = TA002INProw("HAIDISTANCETTL")
                        WF_KAIDISTANCE_NJS.Text = TA002INProw("KAIDISTANCETTL")

                    End If

                    'JKT
                    If work.WF_SEL_CAMPCODE.Text = CONST_CAMP_JKT Then
                        WF_HOLIDAYKBN_JKT.Text = TA002INProw("HOLIDAYKBN")
                        WF_HOLIDAYKBN_TEXT_JKT.Text = TA002INProw("HOLIDAYKBNNAMES")
                        WF_PAYKBN_JKT.Text = TA002INProw("PAYKBN")
                        WF_PAYKBN_TEXT_JKT.Text = TA002INProw("PAYKBNNAMES")
                        WF_SHUKCHOKKBN_JKT.Text = TA002INProw("SHUKCHOKKBN")
                        WF_SHUKCHOKKBN_TEXT_JKT.Text = TA002INProw("SHUKCHOKKBNNAMES")

                        WF_STDATE_JKT.Text = TA002INProw("STDATE")
                        WF_STTIME_JKT.Text = TA002INProw("STTIME")
                        WF_BINDSTDATE_JKT.Text = TA002INProw("BINDSTDATE")
                        WF_BINDTIME_JKT.Text = TA002INProw("BINDTIME")
                        WF_ENDDATE_JKT.Text = TA002INProw("ENDDATE")
                        WF_ENDTIME_JKT.Text = TA002INProw("ENDTIME")

                        WF_NIPPOBREAKTIME_JKT.Text = TA002INProw("NIPPOBREAKTIME")
                        WF_BREAKTIME_JKT.Text = TA002INProw("BREAKTIME")
                        WF_TOKUSA1TIME_JKT.Text = TA002INProw("TOKUSA1TIMETTL")
                        If TA002INProw("SHACHUHAKKBN") = "1" Then
                            WF_SHACHUHAKKBN_JKT.Checked = True
                        Else
                            WF_SHACHUHAKKBN_JKT.Checked = False
                        End If
                        WF_SENJYOCNT_JKT.Text = TA002INProw("SENJYOCNTTTL")

                        WF_UNLOADADDCNT1_JKT.Text = TA002INProw("UNLOADADDCNT1TTL")
                        WF_UNLOADADDCNT2_JKT.Text = TA002INProw("UNLOADADDCNT2TTL")
                        WF_UNLOADADDCNT3_JKT.Text = TA002INProw("UNLOADADDCNT3TTL")
                        WF_LOADINGCNT1_JKT.Text = TA002INProw("LOADINGCNT1TTL")
                        WF_SHORTDISTANCE1_JKT.Text = TA002INProw("SHORTDISTANCE1TTL")
                        WF_SHORTDISTANCE2_JKT.Text = TA002INProw("SHORTDISTANCE2TTL")

                        WF_ORVERTIME_JKT.Text = TA002INProw("ORVERTIMETTL")
                        WF_WNIGHTTIME_JKT.Text = TA002INProw("WNIGHTTIMETTL")
                        WF_HWORKTIME_JKT.Text = TA002INProw("HWORKTIMETTL")
                        WF_HNIGHTTIME_JKT.Text = TA002INProw("HNIGHTTIMETTL")
                        WF_SWORKTIME_JKT.Text = TA002INProw("SWORKTIMETTL")
                        WF_SNIGHTTIME_JKT.Text = TA002INProw("SNIGHTTIMETTL")
                        WF_NIGHTTIME_JKT.Text = TA002INProw("NIGHTTIMETTL")


                        WF_JIKYUSHATIME_JKT.Text = TA002INProw("JIKYUSHATIME")
                        WF_HAIDISTANCE_JKT.Text = Val(TA002INProw("HAIDISTANCE")).ToString("0")
                        WF_KAIDISTANCE_JKT.Text = Val(TA002INProw("KAIDISTANCE")).ToString("0")
                        WF_UNLOADCNT_JKT.Text = TA002INProw("UNLOADCNTTTL")
                    End If
                End If
            End If
        Next

        '○画面（GridView）表示
        Dim TA002tblGrid As New DataTable

        TA002tblGrid = WW_TA002tbl.Copy
        CS0026TblSort.TABLE = TA002tblGrid
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TblSort.FILTER = "HDKBN = 'D'"
        TA002tblGrid = CS0026TblSort.sort()

        Dim WW_LINECNT As Integer = 0
        For i As Integer = 0 To TA002tblGrid.Rows.Count - 1
            If TA002tblGrid.Rows(i)("HDKBN") = "D" Then
                WW_LINECNT += 1
                TA002tblGrid.Rows(i)("LINECNT") = WW_LINECNT
            End If
        Next

        '■■■ テーブルデータ保存 ■■■
        If Not Master.SaveTable(TA002tblGrid, work.WF_DTL_XMLsaveF.Text) Then
            Exit Sub
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(TA002tblGrid)

        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = "詳細"
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea2
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ Close
        WW_TA002tbl.Dispose()
        WW_TA002tbl = Nothing
        WW_TA002DTLtbl.Dispose()
        WW_TA002DTLtbl = Nothing
        TA002tblGrid.Dispose()
        TA002tblGrid = Nothing

        'pnlListArea2.Visible = True

        '〇明細の非表示
        work.WF_IsHideDetailBox.Text = "0"

    End Sub

    ''' <summary>
    '''  GridView用（日報）データ取得
    ''' </summary>
    ''' <param name="IO_TBL">日報情報設定テーブル</param>
    ''' <param name="I_WORKDATE">作業日</param>
    ''' <param name="I_STAFFCODE">従業員コード」</param>
    ''' <param name="I_NIPPOLINKCODE">日報コード</param>
    ''' <param name="O_RTN">可否判定</param>
    Private Sub GetNIPPO(ByRef IO_TBL As DataTable, ByVal I_WORKDATE As String, ByVal I_STAFFCODE As String, ByVal I_NIPPOLINKCODE As String, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        '■■■ 画面表示用データ取得 ■■■

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                 " SELECT 0 as LINECNT , " _
               & "       '' as OPERATION , " _
               & "       '1' as HIDDEN , " _
               & "       TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint) , " _
               & "       ''  as STATUS, " _
               & "       isnull(rtrim(CAL.CAMPCODE),'')  as CAMPCODE, " _
               & "       isnull(rtrim(M1.NAMES),'')  as CAMPNAMES, " _
               & "       @TAISHOYM as TAISHOYM , " _
               & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE, " _
               & "       isnull(rtrim(MB.STAFFNAMES),'') as STAFFNAMES , " _
               & "       isnull(rtrim(A.YMD),'') as WORKDATE , " _
               & "       isnull(rtrim(CAL.WORKINGWEEK),'') as WORKINGWEEK , " _
               & "       isnull(rtrim(F1.VALUE1),'') as WORKINGWEEKNAMES , " _
               & "       'D' as HDKBN , " _
               & "       '0' as RECODEKBN , " _
               & "       isnull(rtrim(F2.VALUE1),'') as RECODEKBNNAMES , " _
               & "       isnull(A.SEQ,'0') as SEQ , " _
               & "       isnull(rtrim(A.ENTRYDATE),'') as ENTRYDATE , " _
               & "       '' as NIPPOLINKCODE , " _
               & "       isnull(rtrim(MB.MORG),'') as MORG , " _
               & "       isnull(rtrim(M2M.NAMES),'') as MORGNAMES , " _
               & "       isnull(rtrim(MB.HORG),'') as HORG , " _
               & "       isnull(rtrim(M2H.NAMES),'') as HORGNAMES , " _
               & "       isnull(rtrim(A.SHIPORG),'') as SORG , " _
               & "       isnull(rtrim(M2S.NAMES),'') as SORGNAMES , " _
               & "       isnull(rtrim(MB.STAFFKBN),'') as STAFFKBN , " _
               & "       isnull(rtrim(F8.VALUE1),'') as STAFFKBNNAMES , " _
               & "       isnull(rtrim(CAL.WORKINGKBN),'') as HOLIDAYKBN , " _
               & "       '' as HOLIDAYKBNNAMES , " _
               & "       '' as PAYKBN , " _
               & "       '' as PAYKBNNAMES , " _
               & "       '' as SHUKCHOKKBN , " _
               & "       '' as SHUKCHOKKBNNAMES , " _
               & "       isnull(rtrim(A.WORKKBN),'')  as WORKKBN , " _
               & "       isnull(rtrim(F4.VALUE2),'') as WORKKBNNAMES , " _
               & "       isnull(rtrim(A.STDATE),'') as STDATE , " _
               & "       isnull(rtrim(A.STTIME),'') as STTIME , " _
               & "       isnull(rtrim(A.ENDDATE),'') as ENDDATE , " _
               & "       isnull(rtrim(A.ENDTIME),'') as ENDTIME , " _
               & "       isnull(A.WORKTIME,0) as WORKTIME , " _
               & "       isnull(A.MOVETIME,0) as MOVETIME , " _
               & "       isnull(A.ACTTIME,0) as ACTTIME , " _
               & "       '' as BINDSTDATE , " _
               & "       '00:00:00' as BINDTIME , " _
               & "       0 as NIPPOBREAKTIME , " _
               & "       0 as BREAKTIME , " _
               & "       0 as BREAKTIMECHO , " _
               & "       0 as BREAKTIMETTL , " _
               & "       0 as NIGHTTIME , " _
               & "       0 as NIGHTTIMECHO , " _
               & "       0 as NIGHTTIMETTL , " _
               & "       0 as ORVERTIME , " _
               & "       0 as ORVERTIMECHO , " _
               & "       0 as ORVERTIMETTL , " _
               & "       0 as WNIGHTTIME , " _
               & "       0 as WNIGHTTIMECHO , " _
               & "       0 as WNIGHTTIMETTL , " _
               & "       0 as SWORKTIME , " _
               & "       0 as SWORKTIMECHO , " _
               & "       0 as SWORKTIMETTL , " _
               & "       0 as SNIGHTTIME , " _
               & "       0 as SNIGHTTIMECHO , " _
               & "       0 as SNIGHTTIMETTL , " _
               & "       0 as HWORKTIME , " _
               & "       0 as HWORKTIMECHO , " _
               & "       0 as HWORKTIMETTL , " _
               & "       0 as HNIGHTTIME , " _
               & "       0 as HNIGHTTIMECHO , " _
               & "       0 as HNIGHTTIMETTL , " _
               & "       0 as WORKNISSU , " _
               & "       0 as WORKNISSUCHO , " _
               & "       0 as WORKNISSUTTL , " _
               & "       0 as SHOUKETUNISSU , " _
               & "       0 as SHOUKETUNISSUCHO , " _
               & "       0 as SHOUKETUNISSUTTL , " _
               & "       0 as KUMIKETUNISSU , " _
               & "       0 as KUMIKETUNISSUCHO , " _
               & "       0 as KUMIKETUNISSUTTL , " _
               & "       0 as ETCKETUNISSU , " _
               & "       0 as ETCKETUNISSUCHO , " _
               & "       0 as ETCKETUNISSUTTL , " _
               & "       0 as NENKYUNISSU , " _
               & "       0 as NENKYUNISSUCHO , " _
               & "       0 as NENKYUNISSUTTL , " _
               & "       0 as TOKUKYUNISSU , " _
               & "       0 as TOKUKYUNISSUCHO , " _
               & "       0 as TOKUKYUNISSUTTL , " _
               & "       0 as CHIKOKSOTAINISSU , " _
               & "       0 as CHIKOKSOTAINISSUCHO , " _
               & "       0 as CHIKOKSOTAINISSUTTL , " _
               & "       0 as STOCKNISSU , " _
               & "       0 as STOCKNISSUCHO , " _
               & "       0 as STOCKNISSUTTL , " _
               & "       0 as KYOTEIWEEKNISSU , " _
               & "       0 as KYOTEIWEEKNISSUCHO , " _
               & "       0 as KYOTEIWEEKNISSUTTL , " _
               & "       0 as WEEKNISSU , " _
               & "       0 as WEEKNISSUCHO , " _
               & "       0 as WEEKNISSUTTL , " _
               & "       0 as DAIKYUNISSU , " _
               & "       0 as DAIKYUNISSUCHO , " _
               & "       0 as DAIKYUNISSUTTL , " _
               & "       0 as NENSHINISSU , " _
               & "       0 as NENSHINISSUCHO , " _
               & "       0 as NENSHINISSUTTL , " _
               & "       0 as SHUKCHOKNNISSU , " _
               & "       0 as SHUKCHOKNNISSUCHO , " _
               & "       0 as SHUKCHOKNNISSUTTL , " _
               & "       0 as SHUKCHOKNISSU , " _
               & "       0 as SHUKCHOKNISSUCHO , " _
               & "       0 as SHUKCHOKNISSUTTL , " _
               & "       0 as TOKSAAKAISU , " _
               & "       0 as TOKSAAKAISUCHO , " _
               & "       0 as TOKSAAKAISUTTL , " _
               & "       0 as TOKSABKAISU , " _
               & "       0 as TOKSABKAISUCHO , " _
               & "       0 as TOKSABKAISUTTL , " _
               & "       0 as TOKSACKAISU , " _
               & "       0 as TOKSACKAISUCHO , " _
               & "       0 as TOKSACKAISUTTL , " _
               & "       0 as TENKOKAISU , " _
               & "       0 as TENKOKAISUCHO , " _
               & "       0 as TENKOKAISUTTL , " _
               & "       0 as HOANTIME , " _
               & "       0 as HOANTIMECHO , " _
               & "       0 as HOANTIMETTL , " _
               & "       0 as KOATUTIME , " _
               & "       0 as KOATUTIMECHO , " _
               & "       0 as KOATUTIMETTL , " _
               & "       0 as TOKUSA1TIME , " _
               & "       0 as TOKUSA1TIMECHO , " _
               & "       0 as TOKUSA1TIMETTL , " _
               & "       0 as HAYADETIME , " _
               & "       0 as HAYADETIMECHO , " _
               & "       0 as HAYADETIMETTL , " _
               & "       0 as PONPNISSU , " _
               & "       0 as PONPNISSUCHO , " _
               & "       0 as PONPNISSUTTL , " _
               & "       0 as BULKNISSU , " _
               & "       0 as BULKNISSUCHO , " _
               & "       0 as BULKNISSUTTL , " _
               & "       0 as TRAILERNISSU , " _
               & "       0 as TRAILERNISSUCHO , " _
               & "       0 as TRAILERNISSUTTL , " _
               & "       0 as BKINMUKAISU , " _
               & "       0 as BKINMUKAISUCHO , " _
               & "       0 as BKINMUKAISUTTL , " _
               & "       CASE WORKKBN WHEN 'B3' THEN isnull(rtrim(MA6.SHARYOKBN),'') ELSE '' END as SHARYOKBN , " _
               & "       CASE WORKKBN WHEN 'B3' THEN isnull(rtrim(F9.VALUE1),'') ELSE '' END as SHARYOKBNNAMES , " _
               & "       CASE WORKKBN WHEN 'B3' THEN isnull(rtrim(MA6.OILKBN),'') ELSE '' END as OILPAYKBN , " _
               & "       CASE WORKKBN WHEN 'B3' THEN isnull(rtrim(F11.VALUE1),'') ELSE '' END as OILPAYKBNNAMES , " _
               & "       CASE WORKKBN WHEN 'B3' THEN 1 ELSE 0 END as UNLOADCNT , " _
               & "       0 as UNLOADCNTCHO , " _
               & "       CASE WORKKBN WHEN 'B3' THEN 1 ELSE 0 END as UNLOADCNTTTL , " _
               & "       isnull(A.SOUDISTANCE,0) as HAIDISTANCE , " _
               & "       0 as HAIDISTANCECHO , " _
               & "       isnull(A.SOUDISTANCE,0) as HAIDISTANCETTL , " _
               & "       0 as KAIDISTANCE , " _
               & "       0 as KAIDISTANCECHO , " _
               & "       0 as KAIDISTANCETTL , " _
               & "       isnull(rtrim(A.DELFLG),'0') as DELFLG , " _
               & "       'N' as DATAKBN , " _
               & "       isnull(A.SHIPORG,'') as SHIPORG , " _
               & "       isnull(M2S.NAMES,'') as SHIPORGNAMES , " _
               & "       isnull(A.NIPPONO,0) as NIPPONO , " _
               & "       isnull(A.GSHABAN,'') as GSHABAN , " _
               & "       isnull(A.RUIDISTANCE,0) as RUIDISTANCE , " _
               & "       isnull(A.JIDISTANCE,0) as JIDISTANCE , " _
               & "       isnull(A.KUDISTANCE,0) as KUDISTANCE , " _
               & "       isnull(A.L1HAISOGROUP,0) as HAISOGROUP , " _
               & "       0 as HAISOTIME , " _
               & "       0 as NENMATUNISSU , " _
               & "       0 as NENMATUNISSUCHO , " _
               & "       0 as NENMATUNISSUTTL , " _
               & "       0 as SHACHUHAKKBN , " _
               & "       '' as SHACHUHAKKBNNAMES , " _
               & "       0 as SHACHUHAKNISSU , " _
               & "       0 as SHACHUHAKNISSUCHO , " _
               & "       0 as SHACHUHAKNISSUTTL , " _
               & "       0 as JIKYUSHATIME , " _
               & "       0 as JIKYUSHATIMECHO , " _
               & "       0 as JIKYUSHATIMETTL  " _
               & " FROM T0005_NIPPO A " _
               & " LEFT JOIN MB001_STAFF MB " _
               & "   ON    MB.CAMPCODE     = A.CAMPCODE " _
               & "   and   MB.STAFFCODE    = A.STAFFCODE " _
               & "   and   MB.STYMD       <= A.YMD " _
               & "   and   MB.ENDYMD      >= A.YMD " _
               & "   and   MB.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = A.CAMPCODE and STAFFCODE = A.STAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
               & "   and   MB.DELFLG      <> '1' " _
               & " LEFT JOIN M0001_CAMP M1 " _
               & "   ON    M1.CAMPCODE    = A.CAMPCODE " _
               & "   and   M1.STYMD      <= @STYMD " _
               & "   and   M1.ENDYMD     >= @ENDYMD " _
               & "   and   M1.STYMD       = (SELECT MAX(STYMD) FROM M0001_CAMP WHERE CAMPCODE = A.CAMPCODE and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1' )" _
               & "   and   M1.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2M " _
               & "   ON    M2M.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2M.ORGCODE    = MB.MORG " _
               & "   and   M2M.STYMD      <= @STYMD " _
               & "   and   M2M.ENDYMD     >= @ENDYMD " _
               & "   and   M2M.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = MB.MORG and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1' )" _
               & "   and   M2M.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2H " _
               & "   ON    M2H.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2H.ORGCODE    = MB.HORG " _
               & "   and   M2H.STYMD      <= @STYMD " _
               & "   and   M2H.ENDYMD     >= @ENDYMD " _
               & "   and   M2H.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = MB.HORG and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1' )" _
               & "   and   M2H.DELFLG     <> '1' " _
               & " LEFT JOIN M0002_ORG M2S " _
               & "   ON    M2S.CAMPCODE   = A.CAMPCODE " _
               & "   and   M2S.ORGCODE    = A.SHIPORG " _
               & "   and   M2S.STYMD      <= @STYMD " _
               & "   and   M2S.ENDYMD     >= @ENDYMD " _
               & "   and   M2S.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = A.SHIPORG and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1' )" _
               & "   and   M2S.DELFLG     <> '1' " _
               & " LEFT JOIN MB004_WORKINGH B4 " _
               & "   ON    B4.CAMPCODE    = A.CAMPCODE " _
               & "   and   B4.HORG        = MB.HORG " _
               & "   and   B4.STAFFKBN    = MB.STAFFKBN " _
               & "   and   B4.STYMD      <= @STYMD " _
               & "   and   B4.ENDYMD     >= @ENDYMD " _
               & "   and   B4.STYMD      = (SELECT MAX(STYMD) FROM MB004_WORKINGH WHERE CAMPCODE = A.CAMPCODE and HORG = MB.HORG and STAFFKBN = MB.STAFFKBN and STYMD <= @STYMD and ENDYMD >= @ENDYMD and DELFLG <> '1') " _
               & "   and   B4.DELFLG     <> '1' " _
               & " LEFT JOIN MB005_CALENDAR CAL " _
               & "   ON    CAL.CAMPCODE    = A.CAMPCODE " _
               & "   and   CAL.WORKINGYMD  = A.YMD " _
               & "   and   CAL.DELFLG     <> '1' " _
               & " LEFT JOIN MA006_SHABANORG MA6 " _
               & "   ON    MA6.CAMPCODE    = A.CAMPCODE " _
               & "   and   MA6.MANGUORG    = A.SHIPORG " _
               & "   and   MA6.GSHABAN     = A.GSHABAN " _
               & "   and   MA6.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F1 " _
               & "   ON    F1.CAMPCODE    = A.CAMPCODE " _
               & "   and   F1.CLASS       = 'WORKINGWEEK' " _
               & "   and   F1.KEYCODE     = CAL.WORKINGWEEK " _
               & "   and   F1.STYMD      <= @STYMD " _
               & "   and   F1.ENDYMD     >= @ENDYMD " _
               & "   and   F1.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F2 " _
               & "   ON    F2.CAMPCODE    = A.CAMPCODE " _
               & "   and   F2.CLASS       = 'RECODEKBN' " _
               & "   and   F2.KEYCODE     = '0' " _
               & "   and   F2.STYMD      <= @STYMD " _
               & "   and   F2.ENDYMD     >= @ENDYMD " _
               & "   and   F2.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F4 " _
               & "   ON    F4.CAMPCODE    = A.CAMPCODE " _
               & "   and   F4.CLASS       = 'WORKKBN' " _
               & "   and   F4.KEYCODE     = A.WORKKBN " _
               & "   and   F4.STYMD      <= @STYMD " _
               & "   and   F4.ENDYMD     >= @ENDYMD " _
               & "   and   F4.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F8 " _
               & "   ON    F8.CAMPCODE    = A.CAMPCODE " _
               & "   and   F8.CLASS       = 'STAFFKBN' " _
               & "   and   F8.KEYCODE     = MB.STAFFKBN " _
               & "   and   F8.STYMD      <= @STYMD " _
               & "   and   F8.ENDYMD     >= @ENDYMD " _
               & "   and   F8.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F9 " _
               & "   ON    F9.CAMPCODE    = A.CAMPCODE " _
               & "   and   F9.CLASS       = 'SHARYOKBN' " _
               & "   and   F9.KEYCODE     = MA6.SHARYOKBN " _
               & "   and   F9.STYMD      <= @STYMD " _
               & "   and   F9.ENDYMD     >= @ENDYMD " _
               & "   and   F9.DELFLG     <> '1' " _
               & " LEFT JOIN MC001_FIXVALUE F11 " _
               & "   ON    F11.CAMPCODE    = A.CAMPCODE " _
               & "   and   F11.CLASS       = 'OILPAYKBN' " _
               & "   and   F11.KEYCODE     = MA6.OILKBN " _
               & "   and   F11.STYMD      <= @STYMD " _
               & "   and   F11.ENDYMD     >= @ENDYMD " _
               & "   and   F11.DELFLG     <> '1' " _
               & " WHERE   A.CAMPCODE     = @CAMPCODE " _
               & "   and   A.YMD          = @YMD " _
               & "   and   A.STAFFCODE    = @STAFFCODE " _
               & "   and   A.WORKKBN not in ('A1', 'Z1') " _
               & "   and   A.DELFLG      <> '1' " _
               & " ORDER BY STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, HDKBN DESC "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
                    Dim P_HORG As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_YMD As SqlParameter = SQLcmd.Parameters.Add("@YMD", System.Data.SqlDbType.NVarChar)
                    Dim P_STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar)
                    Dim P_ENTRYDATE As SqlParameter = SQLcmd.Parameters.Add("@ENTRYDATE", System.Data.SqlDbType.NVarChar)
                    P_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P_TAISHOYM.Value = work.WF_SEL_TAISHOYM.Text
                    P_HORG.Value = work.WF_SEL_HORG.Text
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_YMD.Value = I_WORKDATE
                    P_STAFFCODE.Value = I_STAFFCODE
                    P_ENTRYDATE.Value = I_NIPPOLINKCODE
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル退避
                        '勤怠DB更新用テーブル
                        AddColumnsToT0007Tbl(IO_TBL)
                        IO_TBL.Load(SQLdr)
                    End Using

                    Dim WW_LINEcnt As Integer = 0
                    For Each T0007Row As DataRow In IO_TBL.Rows

                        If T0007Row("HDKBN") = "H" Then
                            T0007Row("SELECT") = "1"
                            T0007Row("HIDDEN") = "0"      '表示
                            WW_LINEcnt += 1
                            T0007Row("LINECNT") = WW_LINEcnt
                        Else
                            T0007Row("SELECT") = "1"
                            T0007Row("HIDDEN") = "1"      '非表示
                            T0007Row("LINECNT") = "0"
                        End If
                        T0007Row("SEQ") = CInt(T0007Row("SEQ")).ToString("000")
                        If IsDate(T0007Row("WORKDATE")) Then
                            T0007Row("WORKDATE") = CDate(T0007Row("WORKDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0007Row("WORKDATE") = DBNull.Value
                        End If
                        If IsDate(T0007Row("STDATE")) Then
                            T0007Row("STDATE") = CDate(T0007Row("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0007Row("STDATE") = DBNull.Value
                        End If
                        If IsDate(T0007Row("STTIME")) Then
                            T0007Row("STTIME") = CDate(T0007Row("STTIME")).ToString("HH:mm")
                        Else
                            T0007Row("STTIME") = DBNull.Value
                        End If
                        If IsDate(T0007Row("ENDDATE")) Then
                            T0007Row("ENDDATE") = CDate(T0007Row("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0007Row("ENDDATE") = DBNull.Value
                        End If
                        If IsDate(T0007Row("ENDTIME")) Then
                            T0007Row("ENDTIME") = CDate(T0007Row("ENDTIME")).ToString("HH:mm")
                        Else
                            T0007Row("ENDTIME") = DBNull.Value
                        End If
                        If IsDate(T0007Row("BINDSTDATE")) Then
                            T0007Row("BINDSTDATE") = CDate(T0007Row("BINDSTDATE")).ToString("HH:mm")
                        Else
                            T0007Row("BINDSTDATE") = DBNull.Value
                        End If

                        If IsDate(T0007Row("BINDTIME")) Then
                            T0007Row("BINDTIME") = CDate(T0007Row("BINDTIME")).ToString("hh:mm")
                        Else
                            T0007Row("BINDTIME") = DBNull.Value
                        End If

                        T0007Row("WORKTIME") = MinituesToHHMM(T0007Row("WORKTIME"))
                        T0007Row("MOVETIME") = MinituesToHHMM(T0007Row("MOVETIME"))
                        T0007Row("ACTTIME") = MinituesToHHMM(T0007Row("ACTTIME"))

                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0007_KINTAI Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' T0005データ取得処理
    ''' </summary>
    ''' <param name="I_FROMYMD">開始年月日</param>
    ''' <param name="I_TOYMD">終了年月日</param>
    ''' <param name="I_STAFFCODE">従業員コード</param>
    ''' <param name="O_TBL">データ設定テーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Public Sub GetT00005(ByVal I_FROMYMD As String, ByVal I_TOYMD As String, ByVal I_STAFFCODE As String, ByRef O_TBL As DataTable, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        '■ 画面表示用データ取得

        'オブジェクト内容検索
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            AddColumnToT0005Tbl(O_TBL)

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

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
                   & "       isnull(rtrim(F1.VALUE1),'') as WORKKBNNAMES , " _
                   & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE , " _
                   & "       isnull(rtrim(B.STAFFNAMES),'') as STAFFNAMES , " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'') as SUBSTAFFCODE , " _
                   & "       isnull(rtrim(B2.STAFFNAMES),'') as SUBSTAFFNAMES , " _
                   & "       isnull(rtrim(A.CREWKBN),'') as CREWKBN , " _
                   & "       isnull(rtrim(F10.VALUE1),'') as CREWKBNNAMES , " _
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
                   & "       isnull(rtrim(MA6.SHARYOKBN),'') as SHARYOKBN , " _
                   & "       isnull(rtrim(F2.VALUE1),'') as SHARYOKBNNAMES , " _
                   & "       isnull(rtrim(MA6.OILKBN),'') as OILPAYKBN , " _
                   & "       isnull(rtrim(F5.VALUE1),'') as OILPAYKBNNAMES , " _
                   & "       isnull(rtrim(MA6.SUISOKBN),'0') as SUISOKBN , " _
                   & "       isnull(rtrim(F6.VALUE1),'') as SUISOKBNNAMES , " _
                   & "       isnull(rtrim(A.L1KAISO),'') as L1KAISO  " _
                   & " FROM T0005_NIPPO A " _
                   & " INNER JOIN T0007_KINTAI T7 " _
                   & "   ON    T7.CAMPCODE     = A.CAMPCODE " _
                   & "   and   T7.WORKDATE     = A.YMD " _
                   & "   and   T7.STAFFCODE    = A.STAFFCODE " _
                   & "   and   T7.HDKBN        = 'H' " _
                   & "   and   T7.RECODEKBN    = '0' " _
                   & "   and   T7.DELFLG      <> '1' " _
                   & " LEFT JOIN MB001_STAFF B " _
                   & "   ON    B.CAMPCODE    = @P1 " _
                   & "   and   B.STAFFCODE   = A.STAFFCODE " _
                   & "   and   B.STYMD      <= A.YMD " _
                   & "   and   B.ENDYMD     >= A.YMD " _
                   & "   and   B.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.STAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B.DELFLG     <> '1' " _
                   & " LEFT JOIN MB001_STAFF B2 " _
                   & "   ON    B2.CAMPCODE    = @P1 " _
                   & "   and   B2.STAFFCODE   = A.SUBSTAFFCODE " _
                   & "   and   B2.STYMD      <= A.YMD " _
                   & "   and   B2.ENDYMD     >= A.YMD " _
                   & "   and   B2.STYMD       = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @P1 and STAFFCODE = A.SUBSTAFFCODE and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' ) " _
                   & "   and   B2.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2M " _
                   & "   ON    M2M.CAMPCODE   = A.CAMPCODE " _
                   & "   and   M2M.ORGCODE    = B.MORG " _
                   & "   and   M2M.STYMD      <= A.YMD " _
                   & "   and   M2M.ENDYMD     >= A.YMD " _
                   & "   and   M2M.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B.MORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2M.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2H " _
                   & "   ON    M2H.CAMPCODE   = A.CAMPCODE " _
                   & "   and   M2H.ORGCODE    = B.HORG " _
                   & "   and   M2H.STYMD      <= A.YMD " _
                   & "   and   M2H.ENDYMD     >= A.YMD " _
                   & "   and   M2H.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = B.HORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2H.DELFLG     <> '1' " _
                   & " LEFT JOIN M0002_ORG M2S " _
                   & "   ON    M2S.CAMPCODE   = A.CAMPCODE " _
                   & "   and   M2S.ORGCODE    = A.SHIPORG " _
                   & "   and   M2S.STYMD      <= A.YMD " _
                   & "   and   M2S.ENDYMD     >= A.YMD " _
                   & "   and   M2S.STYMD       = (SELECT MAX(STYMD) FROM M0002_ORG WHERE CAMPCODE = A.CAMPCODE and ORGCODE = A.SHIPORG and STYMD <= A.YMD and ENDYMD >= A.YMD and DELFLG <> '1' )" _
                   & "   and   M2S.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F1 " _
                   & "   ON    F1.CAMPCODE    = A.CAMPCODE " _
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
                   & "   ON    F2.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F2.CLASS       = 'SHARYOKBN' " _
                   & "   and   F2.KEYCODE     = MA6.SHARYOKBN " _
                   & "   and   F2.STYMD      <= @P5 " _
                   & "   and   F2.ENDYMD     >= @P5 " _
                   & "   and   F2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F5 " _
                   & "   ON    F5.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F5.CLASS       = 'OILPAYKBN' " _
                   & "   and   F5.KEYCODE     = MA6.OILKBN " _
                   & "   and   F5.STYMD      <= @P5 " _
                   & "   and   F5.ENDYMD     >= @P5 " _
                   & "   and   F5.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F41 " _
                   & "   ON    F41.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F41.CLASS       = 'PRODUCT1' " _
                   & "   and   F41.KEYCODE     = A.PRODUCT11 " _
                   & "   and   F41.STYMD      <= @P5 " _
                   & "   and   F41.ENDYMD     >= @P5 " _
                   & "   and   F41.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F42 " _
                   & "   ON    F42.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F42.CLASS       = 'PRODUCT1' " _
                   & "   and   F42.KEYCODE     = A.PRODUCT12 " _
                   & "   and   F42.STYMD      <= @P5 " _
                   & "   and   F42.ENDYMD     >= @P5 " _
                   & "   and   F42.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F43 " _
                   & "   ON    F43.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F43.CLASS       = 'PRODUCT1' " _
                   & "   and   F43.KEYCODE     = A.PRODUCT13 " _
                   & "   and   F43.STYMD      <= @P5 " _
                   & "   and   F43.ENDYMD     >= @P5 " _
                   & "   and   F43.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F44 " _
                   & "   ON    F44.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F44.CLASS       = 'PRODUCT1' " _
                   & "   and   F44.KEYCODE     = A.PRODUCT14 " _
                   & "   and   F44.STYMD      <= @P5 " _
                   & "   and   F44.ENDYMD     >= @P5 " _
                   & "   and   F44.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F45 " _
                   & "   ON    F45.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F45.CLASS       = 'PRODUCT1' " _
                   & "   and   F45.KEYCODE     = A.PRODUCT15 " _
                   & "   and   F45.STYMD      <= @P5 " _
                   & "   and   F45.ENDYMD     >= @P5 " _
                   & "   and   F45.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F46 " _
                   & "   ON    F46.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F46.CLASS       = 'PRODUCT1' " _
                   & "   and   F46.KEYCODE     = A.PRODUCT16 " _
                   & "   and   F46.STYMD      <= @P5 " _
                   & "   and   F46.ENDYMD     >= @P5 " _
                   & "   and   F46.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F47 " _
                   & "   ON    F47.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F47.CLASS       = 'PRODUCT1' " _
                   & "   and   F47.KEYCODE     = A.PRODUCT17 " _
                   & "   and   F47.STYMD      <= @P5 " _
                   & "   and   F47.ENDYMD     >= @P5 " _
                   & "   and   F47.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F48 " _
                   & "   ON    F48.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F48.CLASS       = 'PRODUCT1' " _
                   & "   and   F48.KEYCODE     = A.PRODUCT18 " _
                   & "   and   F48.STYMD      <= @P5 " _
                   & "   and   F48.ENDYMD     >= @P5 " _
                   & "   and   F48.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F6 " _
                   & "   ON    F6.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F6.CLASS       = 'SUISOKBN' " _
                   & "   and   F6.KEYCODE     = isnull(MA6.SUISOKBN,'0') " _
                   & "   and   F6.STYMD      <= @P5 " _
                   & "   and   F6.ENDYMD     >= @P5 " _
                   & "   and   F6.DELFLG     <> '1' " _
                   & " LEFT JOIN MA004_SHARYOC MA4 " _
                   & "   ON    MA4.CAMPCODE    = A.CAMPCODE " _
                   & "   and   MA4.SHARYOTYPE  = A.SHARYOTYPEF " _
                   & "   and   MA4.TSHABAN     = A.TSHABANF " _
                   & "   and   MA4.STYMD      <= @P5 " _
                   & "   and   MA4.ENDYMD     >= @P5 " _
                   & "   and   MA4.DELFLG     <> '1' " _
                   & " LEFT JOIN MB005_CALENDAR CAL " _
                   & "   ON    CAL.CAMPCODE    = A.CAMPCODE " _
                   & "   and   CAL.WORKINGYMD  = A.YMD " _
                   & "   and   CAL.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F7 " _
                   & "   ON    F7.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F7.CLASS       = 'WORKINGWEEK' " _
                   & "   and   F7.KEYCODE     = CAL.WORKINGWEEK " _
                   & "   and   F7.STYMD      <= @P5 " _
                   & "   and   F7.ENDYMD     >= @P5 " _
                   & "   and   F7.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F8 " _
                   & "   ON    F8.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F8.CLASS       = 'HOLIDAYKBN' " _
                   & "   and   F8.KEYCODE     = CAL.WORKINGKBN " _
                   & "   and   F8.STYMD      <= @P5 " _
                   & "   and   F8.ENDYMD     >= @P5 " _
                   & "   and   F8.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F9 " _
                   & "   ON    F9.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F9.CLASS       = 'STAFFKBN' " _
                   & "   and   F9.KEYCODE     = B.STAFFKBN " _
                   & "   and   F9.STYMD      <= @P5 " _
                   & "   and   F9.ENDYMD     >= @P5 " _
                   & "   and   F9.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F10 " _
                   & "   ON    F10.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F10.CLASS       = 'CREWKBN' " _
                   & "   and   F10.KEYCODE     = A.CREWKBN " _
                   & "   and   F10.STYMD      <= @P5 " _
                   & "   and   F10.ENDYMD     >= @P5 " _
                   & "   and   F10.DELFLG     <> '1' " _
                   & " WHERE   A.CAMPCODE    = @P1 " _
                   & "   and   A.STAFFCODE   = @P7 " _
                   & "   and   A.YMD        <= @P3 " _
                   & "   and   A.YMD        >= @P4 " _
                   & "   and   A.DELFLG     <> '1' " _
                   & " ORDER BY A.YMD , A.STAFFCODE , MA6.OILKBN , A.STDATE , A.STTIME, A.ENDDATE , A.ENDTIME"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_HORG.Text
                    PARA3.Value = I_TOYMD
                    PARA4.Value = I_FROMYMD
                    PARA5.Value = Date.Now
                    PARA6.Value = CS0050SESSION.APSV_ID
                    PARA7.Value = I_STAFFCODE

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル退避
                        O_TBL.Load(SQLdr)
                    End Using
                    For Each T0005Row As DataRow In O_TBL.Rows
                        T0005Row("SELECT") = "1"
                        T0005Row("HDKBN") = "D"       'ヘッダ、明細区分
                        If IsDate(T0005Row("YMD")) Then
                            T0005Row("YMD") = CDate(T0005Row("YMD")).ToString("yyyy/MM/dd")
                        Else
                            T0005Row("YMD") = DBNull.Value
                        End If
                        If IsDate(T0005Row("STDATE")) Then
                            T0005Row("STDATE") = CDate(T0005Row("STDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0005Row("STDATE") = DBNull.Value
                        End If
                        If IsDate(T0005Row("STTIME")) Then
                            T0005Row("STTIME") = CDate(T0005Row("STTIME")).ToString("HH:mm")
                        Else
                            T0005Row("STTIME") = DBNull.Value
                        End If
                        If IsDate(T0005Row("ENDDATE")) Then
                            T0005Row("ENDDATE") = CDate(T0005Row("ENDDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0005Row("ENDDATE") = DBNull.Value
                        End If
                        If IsDate(T0005Row("ENDTIME")) Then
                            T0005Row("ENDTIME") = CDate(T0005Row("ENDTIME")).ToString("HH:mm")
                        Else
                            T0005Row("ENDTIME") = DBNull.Value
                        End If
                        T0005Row("SOUDISTANCE") = Int(T0005Row("SOUDISTANCE"))
                    Next
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0005_NIPPO Select"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' T0010データ取得処理（モデル距離）
    ''' </summary>
    ''' <param name="I_WORKDATE">作業日付</param>
    ''' <param name="I_STAFFCODE">従業員コード</param>
    ''' <param name="O_TBL">T0010データを設定したテーブル</param>
    ''' <param name="O_RTN">可否判定</param>
    Public Sub GetT00010(ByVal I_WORKDATE As String, ByVal I_STAFFCODE As String, ByRef O_TBL As DataTable, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        '■ 画面表示用データ取得

        'オブジェクト内容検索
        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            AddColumnsToT0010Tbl(O_TBL)

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.CAMPCODE),'')  as CAMPCODE, " _
                   & "       isnull(rtrim(A.TAISHOYM),'') as TAISHOYM , " _
                   & "       isnull(rtrim(A.STAFFCODE),'') as STAFFCODE , " _
                   & "       isnull(rtrim(A.WORKDATE),'') as WORKDATE , " _
                   & "       isnull(rtrim(A.SAVECNT),0) as SAVECNT , " _
                   & "       isnull(rtrim(A.SHARYOKBN1),'') as SHARYOKBN1 , " _
                   & "       isnull(rtrim(F11.VALUE1),'') as SHARYOKBN1NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN1),'') as OILPAYKBN1 , " _
                   & "       isnull(rtrim(F21.VALUE1),'') as OILPAYKBN1NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO1),'') as SHUKABASHO1 , " _
                   & "       isnull(rtrim(MC6S1.NAMES),'') as SHUKABASHO1NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE1),'') as TODOKECODE1 , " _
                   & "       isnull(rtrim(MC6T1.NAMES),'') as TODOKECODE1NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE1),0) as MODELDISTANCE1 , " _
                   & "       isnull(rtrim(A.MODIFYKBN1),'') as MODIFYKBN1 , " _
                   & "       isnull(rtrim(A.SHARYOKBN2),'') as SHARYOKBN2 , " _
                   & "       isnull(rtrim(F12.VALUE1),'') as SHARYOKBN2NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN2),'') as OILPAYKBN2 , " _
                   & "       isnull(rtrim(F22.VALUE1),'') as OILPAYKBN2NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO2),'') as SHUKABASHO2 , " _
                   & "       isnull(rtrim(MC6S2.NAMES),'') as SHUKABASHO2NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE2),'') as TODOKECODE2 , " _
                   & "       isnull(rtrim(MC6T2.NAMES),'') as TODOKECODE2NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE2),0) as MODELDISTANCE2 , " _
                   & "       isnull(rtrim(A.MODIFYKBN2),'') as MODIFYKBN2 , " _
                   & "       isnull(rtrim(A.SHARYOKBN3),'') as SHARYOKBN3 , " _
                   & "       isnull(rtrim(F13.VALUE1),'') as SHARYOKBN3NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN3),'') as OILPAYKBN3 , " _
                   & "       isnull(rtrim(F23.VALUE1),'') as OILPAYKBN3NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO3),'') as SHUKABASHO3 , " _
                   & "       isnull(rtrim(MC6S3.NAMES),'') as SHUKABASHO3NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE3),'') as TODOKECODE3 , " _
                   & "       isnull(rtrim(MC6T3.NAMES),'') as TODOKECODE3NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE3),0) as MODELDISTANCE3 , " _
                   & "       isnull(rtrim(A.MODIFYKBN3),'') as MODIFYKBN3 , " _
                   & "       isnull(rtrim(A.SHARYOKBN4),'') as SHARYOKBN4 , " _
                   & "       isnull(rtrim(F14.VALUE1),'') as SHARYOKBN4NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN4),'') as OILPAYKBN4 , " _
                   & "       isnull(rtrim(F24.VALUE1),'') as OILPAYKBN4NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO4),'') as SHUKABASHO4 , " _
                   & "       isnull(rtrim(MC6S4.NAMES),'') as SHUKABASHO4NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE4),'') as TODOKECODE4 , " _
                   & "       isnull(rtrim(MC6T4.NAMES),'') as TODOKECODE4NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE4),0) as MODELDISTANCE4 , " _
                   & "       isnull(rtrim(A.MODIFYKBN4),'') as MODIFYKBN4 , " _
                   & "       isnull(rtrim(A.SHARYOKBN5),'') as SHARYOKBN5 , " _
                   & "       isnull(rtrim(F15.VALUE1),'') as SHARYOKBN5NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN5),'') as OILPAYKBN5 , " _
                   & "       isnull(rtrim(F25.VALUE1),'') as OILPAYKBN5NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO5),'') as SHUKABASHO5 , " _
                   & "       isnull(rtrim(MC6S5.NAMES),'') as SHUKABASHO5NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE5),'') as TODOKECODE5 , " _
                   & "       isnull(rtrim(MC6T5.NAMES),'') as TODOKECODE5NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE5),0) as MODELDISTANCE5 , " _
                   & "       isnull(rtrim(A.MODIFYKBN5),'') as MODIFYKBN5 , " _
                   & "       isnull(rtrim(A.SHARYOKBN6),'') as SHARYOKBN6 , " _
                   & "       isnull(rtrim(F16.VALUE1),'') as SHARYOKBN6NAME , " _
                   & "       isnull(rtrim(A.OILPAYKBN6),'') as OILPAYKBN6 , " _
                   & "       isnull(rtrim(F26.VALUE1),'') as OILPAYKBN6NAME , " _
                   & "       isnull(rtrim(A.SHUKABASHO6),'') as SHUKABASHO6 , " _
                   & "       isnull(rtrim(MC6S6.NAMES),'') as SHUKABASHO6NAME , " _
                   & "       isnull(rtrim(A.TODOKECODE6),'') as TODOKECODE6 , " _
                   & "       isnull(rtrim(MC6T6.NAMES),'') as TODOKECODE6NAME , " _
                   & "       isnull(rtrim(A.MODELDISTANCE6),0) as MODELDISTANCE6 , " _
                   & "       isnull(rtrim(A.MODIFYKBN6),'') as MODIFYKBN6  " _
                   & " FROM T0010_MODELDISTANCE A " _
                   & " LEFT JOIN MC001_FIXVALUE F11 " _
                   & "   ON    F11.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F11.CLASS       = 'SHARYOKBN' " _
                   & "   and   F11.KEYCODE     = A.SHARYOKBN1 " _
                   & "   and   F11.STYMD      <= @P4 " _
                   & "   and   F11.ENDYMD     >= @P4 " _
                   & "   and   F11.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F12 " _
                   & "   ON    F12.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F12.CLASS       = 'SHARYOKBN' " _
                   & "   and   F12.KEYCODE     = A.SHARYOKBN2 " _
                   & "   and   F12.STYMD      <= @P4 " _
                   & "   and   F12.ENDYMD     >= @P4 " _
                   & "   and   F12.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F13 " _
                   & "   ON    F13.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F13.CLASS       = 'SHARYOKBN' " _
                   & "   and   F13.KEYCODE     = A.SHARYOKBN3 " _
                   & "   and   F13.STYMD      <= @P4 " _
                   & "   and   F13.ENDYMD     >= @P4 " _
                   & "   and   F13.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F14 " _
                   & "   ON    F14.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F14.CLASS       = 'SHARYOKBN' " _
                   & "   and   F14.KEYCODE     = A.SHARYOKBN4 " _
                   & "   and   F14.STYMD      <= @P4 " _
                   & "   and   F14.ENDYMD     >= @P4 " _
                   & "   and   F14.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F15 " _
                   & "   ON    F15.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F15.CLASS       = 'SHARYOKBN' " _
                   & "   and   F15.KEYCODE     = A.SHARYOKBN5 " _
                   & "   and   F15.STYMD      <= @P4 " _
                   & "   and   F15.ENDYMD     >= @P4 " _
                   & "   and   F15.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F16 " _
                   & "   ON    F16.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F16.CLASS       = 'SHARYOKBN' " _
                   & "   and   F16.KEYCODE     = A.SHARYOKBN6 " _
                   & "   and   F16.STYMD      <= @P4 " _
                   & "   and   F16.ENDYMD     >= @P4 " _
                   & "   and   F16.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F21 " _
                   & "   ON    F21.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F21.CLASS       = 'OILPAYKBN' " _
                   & "   and   F21.KEYCODE     = A.OILPAYKBN1 " _
                   & "   and   F21.STYMD      <= @P4 " _
                   & "   and   F21.ENDYMD     >= @P4 " _
                   & "   and   F21.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F22 " _
                   & "   ON    F22.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F22.CLASS       = 'OILPAYKBN' " _
                   & "   and   F22.KEYCODE     = A.OILPAYKBN2 " _
                   & "   and   F22.STYMD      <= @P4 " _
                   & "   and   F22.ENDYMD     >= @P4 " _
                   & "   and   F22.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F23 " _
                   & "   ON    F23.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F23.CLASS       = 'OILPAYKBN' " _
                   & "   and   F23.KEYCODE     = A.OILPAYKBN3 " _
                   & "   and   F23.STYMD      <= @P4 " _
                   & "   and   F23.ENDYMD     >= @P4 " _
                   & "   and   F23.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F24 " _
                   & "   ON    F24.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F24.CLASS       = 'OILPAYKBN' " _
                   & "   and   F24.KEYCODE     = A.OILPAYKBN4 " _
                   & "   and   F24.STYMD      <= @P4 " _
                   & "   and   F24.ENDYMD     >= @P4 " _
                   & "   and   F24.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F25 " _
                   & "   ON    F25.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F25.CLASS       = 'OILPAYKBN' " _
                   & "   and   F25.KEYCODE     = A.OILPAYKBN5 " _
                   & "   and   F25.STYMD      <= @P4 " _
                   & "   and   F25.ENDYMD     >= @P4 " _
                   & "   and   F25.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F26 " _
                   & "   ON    F26.CAMPCODE    = A.CAMPCODE " _
                   & "   and   F26.CLASS       = 'OILPAYKBN' " _
                   & "   and   F26.KEYCODE     = A.OILPAYKBN6 " _
                   & "   and   F26.STYMD      <= @P4 " _
                   & "   and   F26.ENDYMD     >= @P4 " _
                   & "   and   F26.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S1 " _
                   & "   ON    MC6S1.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S1.TODOKECODE = A.SHUKABASHO1 " _
                   & "   and   MC6S1.STYMD      <= @P4 " _
                   & "   and   MC6S1.ENDYMD     >= @P4 " _
                   & "   and   MC6S1.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S2 " _
                   & "   ON    MC6S2.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S2.TODOKECODE = A.SHUKABASHO2 " _
                   & "   and   MC6S2.STYMD      <= @P4 " _
                   & "   and   MC6S2.ENDYMD     >= @P4 " _
                   & "   and   MC6S2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S3 " _
                   & "   ON    MC6S3.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S3.TODOKECODE = A.SHUKABASHO3 " _
                   & "   and   MC6S3.STYMD      <= @P4 " _
                   & "   and   MC6S3.ENDYMD     >= @P4 " _
                   & "   and   MC6S3.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S4 " _
                   & "   ON    MC6S4.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S4.TODOKECODE = A.SHUKABASHO4 " _
                   & "   and   MC6S4.STYMD      <= @P4 " _
                   & "   and   MC6S4.ENDYMD     >= @P4 " _
                   & "   and   MC6S4.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S5 " _
                   & "   ON    MC6S5.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S5.TODOKECODE = A.SHUKABASHO5 " _
                   & "   and   MC6S5.STYMD      <= @P4 " _
                   & "   and   MC6S5.ENDYMD     >= @P4 " _
                   & "   and   MC6S5.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6S6 " _
                   & "   ON    MC6S6.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6S6.TODOKECODE = A.SHUKABASHO6 " _
                   & "   and   MC6S6.STYMD      <= @P4 " _
                   & "   and   MC6S6.ENDYMD     >= @P4 " _
                   & "   and   MC6S6.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T1 " _
                   & "   ON    MC6T1.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T1.TODOKECODE = A.TODOKECODE1 " _
                   & "   and   MC6T1.STYMD      <= @P4 " _
                   & "   and   MC6T1.ENDYMD     >= @P4 " _
                   & "   and   MC6T1.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T2 " _
                   & "   ON    MC6T2.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T2.TODOKECODE = A.TODOKECODE2 " _
                   & "   and   MC6T2.STYMD      <= @P4 " _
                   & "   and   MC6T2.ENDYMD     >= @P4 " _
                   & "   and   MC6T2.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T3 " _
                   & "   ON    MC6T3.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T3.TODOKECODE = A.TODOKECODE3 " _
                   & "   and   MC6T3.STYMD      <= @P4 " _
                   & "   and   MC6T3.ENDYMD     >= @P4 " _
                   & "   and   MC6T3.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T4 " _
                   & "   ON    MC6T4.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T4.TODOKECODE = A.TODOKECODE4 " _
                   & "   and   MC6T4.STYMD      <= @P4 " _
                   & "   and   MC6T4.ENDYMD     >= @P4 " _
                   & "   and   MC6T4.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T5 " _
                   & "   ON    MC6T5.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T5.TODOKECODE = A.TODOKECODE5 " _
                   & "   and   MC6T5.STYMD      <= @P4 " _
                   & "   and   MC6T5.ENDYMD     >= @P4 " _
                   & "   and   MC6T5.DELFLG     <> '1' " _
                   & " LEFT JOIN MC006_TODOKESAKI MC6T6 " _
                   & "   ON    MC6T6.CAMPCODE   = A.CAMPCODE " _
                   & "   and   MC6T6.TODOKECODE = A.TODOKECODE6 " _
                   & "   and   MC6T6.STYMD      <= @P4 " _
                   & "   and   MC6T6.ENDYMD     >= @P4 " _
                   & "   and   MC6T6.DELFLG     <> '1' " _
                   & " WHERE   A.CAMPCODE        = @P1 " _
                   & "   and   A.WORKDATE        = @P2 " _
                   & "   and   A.STAFFCODE       = @P3 " _
                   & "   and   A.DELFLG         <> '1' " _
                   & " ORDER BY A.STAFFCODE , A.WORKDATE"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = I_WORKDATE
                    PARA3.Value = I_STAFFCODE
                    PARA4.Value = Date.Now

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル退避
                        O_TBL.Load(SQLdr)
                    End Using

                    For Each T0010row As DataRow In O_TBL.Rows
                        If IsDate(T0010row("WORKDATE")) Then
                            T0010row("WORKDATE") = CDate(T0010row("WORKDATE")).ToString("yyyy/MM/dd")
                        Else
                            T0010row("WORKDATE") = DBNull.Value
                        End If
                    Next
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0010_MODELDISTANCE Select"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    '''  TA0002ALL（Grid用）カラム設定
    ''' </summary>
    ''' <param name="IO_TBL">TA0002用列追加テーブル</param>
    Protected Sub AddColumnToTA0002Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If
        'T0005DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))
        IO_TBL.Columns.Add("OPERATION", GetType(String))
        IO_TBL.Columns.Add("TIMSTP", GetType(String))
        IO_TBL.Columns.Add("SELECT", GetType(Integer))
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))

        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STAFFNAMES", GetType(String))
        IO_TBL.Columns.Add("CREWKBN", GetType(String))
        IO_TBL.Columns.Add("CREWKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("WORKDATE", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEK", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEKNAMES", GetType(String))
        IO_TBL.Columns.Add("HDKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("NIPPOLINKCODE", GetType(String))
        IO_TBL.Columns.Add("WORKKBN", GetType(String))
        IO_TBL.Columns.Add("WORKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("GSHABANLICNPLTNO", GetType(String))
        IO_TBL.Columns.Add("STDATE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("ENDDATE", GetType(String))
        IO_TBL.Columns.Add("ENDTIME", GetType(String))
        IO_TBL.Columns.Add("WORKTIME", GetType(String))
        IO_TBL.Columns.Add("MOVETIME", GetType(String))
        IO_TBL.Columns.Add("ACTTIME", GetType(String))
        IO_TBL.Columns.Add("NIPPOBREAKTIME", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("JIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL", GetType(String))
        IO_TBL.Columns.Add("STAFFKBN", GetType(String))
        IO_TBL.Columns.Add("STAFFKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("MORG", GetType(String))
        IO_TBL.Columns.Add("MORGNAMES", GetType(String))
        IO_TBL.Columns.Add("HORG", GetType(String))
        IO_TBL.Columns.Add("HORGNAMES", GetType(String))
        IO_TBL.Columns.Add("SORG", GetType(String))
        IO_TBL.Columns.Add("SORGNAMES", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBN", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("PAYKBN", GetType(String))
        IO_TBL.Columns.Add("PAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBN", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("BINDSTDATE", GetType(String))
        IO_TBL.Columns.Add("BINDTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("ORVERTIME", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMECHO", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WORKNISSU", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSU", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSU", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSU", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISU", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("HOANTIME", GetType(String))
        IO_TBL.Columns.Add("HOANTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HOANTIMETTL", GetType(String))
        IO_TBL.Columns.Add("KOATUTIME", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMETTL", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIME", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMECHO", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMETTL", GetType(String))
        IO_TBL.Columns.Add("HAYADETIME", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMECHO", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMETTL", GetType(String))
        IO_TBL.Columns.Add("PONPNISSU", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BULKNISSU", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSU", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISU", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))

        IO_TBL.Columns.Add("CAMPCODE_TXT", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM_TXT", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKDATE_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEK_TXT", GetType(String))
        IO_TBL.Columns.Add("HDKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("RECODEKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("WORKKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("GSHABAN_TXT", GetType(String))
        IO_TBL.Columns.Add("GSHABANLICNPLTNO_TXT", GetType(String))
        IO_TBL.Columns.Add("STAFFKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("MORG_TXT", GetType(String))
        IO_TBL.Columns.Add("HORG_TXT", GetType(String))
        IO_TBL.Columns.Add("SORG_TXT", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("PAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN_TXT", GetType(String))
        IO_TBL.Columns.Add("DELFLG_TXT", GetType(String))

        IO_TBL.Columns.Add("ENDDATE_TXT", GetType(String))
        IO_TBL.Columns.Add("ORVER15", GetType(String))
        IO_TBL.Columns.Add("ORVER15_TXT", GetType(String))
        IO_TBL.Columns.Add("ORVER09", GetType(String))
        IO_TBL.Columns.Add("ORVER09_TXT", GetType(String))
        IO_TBL.Columns.Add("RYOME", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1_TXT", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1NAMES", GetType(String))
        IO_TBL.Columns.Add("SHUKOTIME", GetType(String))
        IO_TBL.Columns.Add("KIKOTIME", GetType(String))
        IO_TBL.Columns.Add("HANDLETIME", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))
        IO_TBL.Columns.Add("SURYO", GetType(String))

        IO_TBL.Columns.Add("DATAKBN", GetType(String))

        IO_TBL.Columns.Add("SHACHUHAKKBN", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("HAISOTIME", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIME", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMECHO", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMETTL", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHONAMES", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKECODENAMES", GetType(String))
        IO_TBL.Columns.Add("MODIFYKBN", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("T10SAVECNT", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN1", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN1", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO1", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE1", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE1", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN1", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN2", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN2", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO2", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE2", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE2", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN2", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN3", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN3", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO3", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE3", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE3", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN3", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN4", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN4", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO4", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE4", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE4", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN4", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN5", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN5", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO5", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE5", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE5", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN5", GetType(String))
        IO_TBL.Columns.Add("T10SHARYOKBN6", GetType(String))
        IO_TBL.Columns.Add("T10OILPAYKBN6", GetType(String))
        IO_TBL.Columns.Add("T10SHUKABASHO6", GetType(String))
        IO_TBL.Columns.Add("T10TODOKECODE6", GetType(String))
        IO_TBL.Columns.Add("T10MODELDISTANCE6", GetType(String))
        IO_TBL.Columns.Add("T10MODIFYKBN6", GetType(String))

        IO_TBL.Columns.Add("KAITENCNT", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTCHO", GetType(String))
        IO_TBL.Columns.Add("KAITENCNTTTL", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WWORKTIME", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIME", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMETTL", GetType(String))

        IO_TBL.Columns.Add("SENJYOCNT", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTCHO", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTTTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2TTL", GetType(String))

        IO_TBL.Columns.Add("ORGSEQ", GetType(String))

    End Sub

    ''' <summary>
    ''' T0007カラム設定
    ''' </summary>
    ''' <param name="IO_TBL">T0007用の列追加テーブル</param>
    Public Sub AddColumnsToT0007Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If

        'T0005DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))
        IO_TBL.Columns.Add("OPERATION", GetType(String))
        IO_TBL.Columns.Add("TIMSTP", GetType(String))
        IO_TBL.Columns.Add("SELECT", GetType(Integer))
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))
        IO_TBL.Columns.Add("EXTRACTCNT", GetType(String))

        IO_TBL.Columns.Add("STATUS", GetType(String))
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STAFFNAMES", GetType(String))
        IO_TBL.Columns.Add("WORKDATE", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEK", GetType(String))
        IO_TBL.Columns.Add("WORKINGWEEKNAMES", GetType(String))
        IO_TBL.Columns.Add("HDKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBN", GetType(String))
        IO_TBL.Columns.Add("RECODEKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("NIPPOLINKCODE", GetType(String))
        IO_TBL.Columns.Add("MORG", GetType(String))
        IO_TBL.Columns.Add("MORGNAMES", GetType(String))
        IO_TBL.Columns.Add("HORG", GetType(String))
        IO_TBL.Columns.Add("HORGNAMES", GetType(String))
        IO_TBL.Columns.Add("SORG", GetType(String))
        IO_TBL.Columns.Add("SORGNAMES", GetType(String))
        IO_TBL.Columns.Add("STAFFKBN", GetType(String))
        IO_TBL.Columns.Add("STAFFKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBN", GetType(String))
        IO_TBL.Columns.Add("HOLIDAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("PAYKBN", GetType(String))
        IO_TBL.Columns.Add("PAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBN", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("WORKKBN", GetType(String))
        IO_TBL.Columns.Add("WORKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("STDATE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("ENDDATE", GetType(String))
        IO_TBL.Columns.Add("ENDTIME", GetType(String))
        IO_TBL.Columns.Add("WORKTIME", GetType(String))
        IO_TBL.Columns.Add("MOVETIME", GetType(String))
        IO_TBL.Columns.Add("ACTTIME", GetType(String))
        IO_TBL.Columns.Add("BINDSTDATE", GetType(String))
        IO_TBL.Columns.Add("BINDTIME", GetType(String))
        IO_TBL.Columns.Add("NIPPOBREAKTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIME", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("BREAKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("NIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("ORVERTIME", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMECHO", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WORKNISSU", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WORKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHOUKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KUMIKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSU", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("ETCKETUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKUKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSU", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("CHIKOKSOTAINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSU", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("STOCKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("KYOTEIWEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSU", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("WEEKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSU", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("DAIKYUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSU", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENSHINISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHUKCHOKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSAAKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSABKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISU", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TOKSACKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISU", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("TENKOKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("HOANTIME", GetType(String))
        IO_TBL.Columns.Add("HOANTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HOANTIMETTL", GetType(String))
        IO_TBL.Columns.Add("KOATUTIME", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("KOATUTIMETTL", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIME", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMECHO", GetType(String))
        IO_TBL.Columns.Add("TOKUSA1TIMETTL", GetType(String))
        IO_TBL.Columns.Add("HAYADETIME", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMECHO", GetType(String))
        IO_TBL.Columns.Add("HAYADETIMETTL", GetType(String))
        IO_TBL.Columns.Add("PONPNISSU", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("PONPNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BULKNISSU", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("BULKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSU", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("TRAILERNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISU", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUCHO", GetType(String))
        IO_TBL.Columns.Add("BKINMUKAISUTTL", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNT", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTCHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADCNTTTL", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("HAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCECHO", GetType(String))
        IO_TBL.Columns.Add("KAIDISTANCETTL", GetType(String))
        IO_TBL.Columns.Add("HAISOGROUP", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))

        IO_TBL.Columns.Add("DATAKBN", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("SHIPORGNAMES", GetType(String))
        IO_TBL.Columns.Add("NIPPONO", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("RUIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("JIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KUDISTANCE", GetType(String))

        IO_TBL.Columns.Add("SHACHUHAKKBN", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("HAISOTIME", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSU", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("NENMATUNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSU", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUCHO", GetType(String))
        IO_TBL.Columns.Add("SHACHUHAKNISSUTTL", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIME", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMECHO", GetType(String))
        IO_TBL.Columns.Add("JIKYUSHATIMETTL", GetType(String))

        IO_TBL.Columns.Add("HDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("HDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAIWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SDAINIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WWORKTIME", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIME", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMECHO", GetType(String))
        IO_TBL.Columns.Add("JYOMUTIMETTL", GetType(String))

        IO_TBL.Columns.Add("SENJYOCNT", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTCHO", GetType(String))
        IO_TBL.Columns.Add("SENJYOCNTTTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT3TTL", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4CHO", GetType(String))
        IO_TBL.Columns.Add("UNLOADADDCNT4TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT1TTL", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2CHO", GetType(String))
        IO_TBL.Columns.Add("LOADINGCNT2TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE1TTL", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2CHO", GetType(String))
        IO_TBL.Columns.Add("SHORTDISTANCE2TTL", GetType(String))

    End Sub

    ''' <summary>
    ''' T00005ALLカラム設定
    ''' </summary>
    ''' <param name="IO_TBL">列追加対象テーブル</param>
    Public Sub AddColumnToT0005Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If

        'T0005DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))
        IO_TBL.Columns.Add("OPERATION", GetType(String))
        IO_TBL.Columns.Add("TIMSTP", GetType(String))
        IO_TBL.Columns.Add("SELECT", GetType(Integer))
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))

        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("SHIPORGNAMES", GetType(String))
        IO_TBL.Columns.Add("TERMKBN", GetType(String))
        IO_TBL.Columns.Add("TERMKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("YMD", GetType(String))
        IO_TBL.Columns.Add("NIPPONO", GetType(String))
        IO_TBL.Columns.Add("HDKBN", GetType(String))
        IO_TBL.Columns.Add("WORKKBN", GetType(String))
        IO_TBL.Columns.Add("WORKKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("STAFFNAMES", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFCODE", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFNAMES", GetType(String))
        IO_TBL.Columns.Add("CREWKBN", GetType(String))
        IO_TBL.Columns.Add("CREWKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("GSHABANLICNPLTNO", GetType(String))
        IO_TBL.Columns.Add("STDATE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("ENDDATE", GetType(String))
        IO_TBL.Columns.Add("ENDTIME", GetType(String))
        IO_TBL.Columns.Add("WORKTIME", GetType(String))
        IO_TBL.Columns.Add("MOVETIME", GetType(String))
        IO_TBL.Columns.Add("ACTTIME", GetType(String))
        IO_TBL.Columns.Add("PRATE", GetType(String))
        IO_TBL.Columns.Add("CASH", GetType(String))
        IO_TBL.Columns.Add("TICKET", GetType(String))
        IO_TBL.Columns.Add("ETC", GetType(String))
        IO_TBL.Columns.Add("TOTALTOLL", GetType(String))
        IO_TBL.Columns.Add("STMATER", GetType(String))
        IO_TBL.Columns.Add("ENDMATER", GetType(String))
        IO_TBL.Columns.Add("RUIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("SOUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("JIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KYUYU", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))
        IO_TBL.Columns.Add("TORINAMES", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHONAMES", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKENAMES", GetType(String))
        IO_TBL.Columns.Add("TODOKEDATE", GetType(String))
        IO_TBL.Columns.Add("OILTYPE1", GetType(String))
        IO_TBL.Columns.Add("PRODUCT11", GetType(String))
        IO_TBL.Columns.Add("PRODUCT21", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO1", GetType(String))
        IO_TBL.Columns.Add("STANI1", GetType(String))
        IO_TBL.Columns.Add("STANI1NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE2", GetType(String))
        IO_TBL.Columns.Add("PRODUCT12", GetType(String))
        IO_TBL.Columns.Add("PRODUCT22", GetType(String))
        IO_TBL.Columns.Add("PRODUCT2NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO2", GetType(String))
        IO_TBL.Columns.Add("STANI2", GetType(String))
        IO_TBL.Columns.Add("STANI2NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE3", GetType(String))
        IO_TBL.Columns.Add("PRODUCT13", GetType(String))
        IO_TBL.Columns.Add("PRODUCT23", GetType(String))
        IO_TBL.Columns.Add("PRODUCT3NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO3", GetType(String))
        IO_TBL.Columns.Add("STANI3", GetType(String))
        IO_TBL.Columns.Add("STANI3NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE4", GetType(String))
        IO_TBL.Columns.Add("PRODUCT14", GetType(String))
        IO_TBL.Columns.Add("PRODUCT24", GetType(String))
        IO_TBL.Columns.Add("PRODUCT4NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO4", GetType(String))
        IO_TBL.Columns.Add("STANI4", GetType(String))
        IO_TBL.Columns.Add("STANI4NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE5", GetType(String))
        IO_TBL.Columns.Add("PRODUCT15", GetType(String))
        IO_TBL.Columns.Add("PRODUCT25", GetType(String))
        IO_TBL.Columns.Add("PRODUCT5NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO5", GetType(String))
        IO_TBL.Columns.Add("STANI5", GetType(String))
        IO_TBL.Columns.Add("STANI5NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE6", GetType(String))
        IO_TBL.Columns.Add("PRODUCT16", GetType(String))
        IO_TBL.Columns.Add("PRODUCT26", GetType(String))
        IO_TBL.Columns.Add("PRODUCT6NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO6", GetType(String))
        IO_TBL.Columns.Add("STANI6", GetType(String))
        IO_TBL.Columns.Add("STANI6NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE7", GetType(String))
        IO_TBL.Columns.Add("PRODUCT17", GetType(String))
        IO_TBL.Columns.Add("PRODUCT27", GetType(String))
        IO_TBL.Columns.Add("PRODUCT7NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO7", GetType(String))
        IO_TBL.Columns.Add("STANI7", GetType(String))
        IO_TBL.Columns.Add("STANI7NAMES", GetType(String))
        IO_TBL.Columns.Add("OILTYPE8", GetType(String))
        IO_TBL.Columns.Add("PRODUCT18", GetType(String))
        IO_TBL.Columns.Add("PRODUCT28", GetType(String))
        IO_TBL.Columns.Add("PRODUCT8NAMES", GetType(String))
        IO_TBL.Columns.Add("SURYO8", GetType(String))
        IO_TBL.Columns.Add("STANI8", GetType(String))
        IO_TBL.Columns.Add("STANI8NAMES", GetType(String))
        IO_TBL.Columns.Add("TOTALSURYO", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBN", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("ORDERNO", GetType(String))
        IO_TBL.Columns.Add("DETAILNO", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))
        IO_TBL.Columns.Add("DROPNO", GetType(String))
        IO_TBL.Columns.Add("JISSKIKBN", GetType(String))
        IO_TBL.Columns.Add("JISSKIKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("URIKBN", GetType(String))
        IO_TBL.Columns.Add("URIKBNNAMES", GetType(String))

        'IO_TBL.Columns.Add("STORICODE", GetType(String))
        'IO_TBL.Columns.Add("STORICODENAMES", GetType(String))
        'IO_TBL.Columns.Add("CONTCHASSIS", GetType(String))
        'IO_TBL.Columns.Add("CONTCHASSISLICNPLTNO", GetType(String))

        IO_TBL.Columns.Add("SHARYOTYPEF", GetType(String))
        IO_TBL.Columns.Add("TSHABANF", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB", GetType(String))
        IO_TBL.Columns.Add("TSHABANB", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB2", GetType(String))
        IO_TBL.Columns.Add("TSHABANB2", GetType(String))
        IO_TBL.Columns.Add("TAXKBN", GetType(String))
        IO_TBL.Columns.Add("TAXKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("LATITUDE", GetType(String))
        IO_TBL.Columns.Add("LONGITUDE", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))

        IO_TBL.Columns.Add("SHARYOKBN", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("SUISOKBN", GetType(String))
        IO_TBL.Columns.Add("SUISOKBNNAMES", GetType(String))
        IO_TBL.Columns.Add("L1KAISO", GetType(String))

    End Sub
    ''' <summary>
    ''' T0010TBL形式の列を追加
    ''' </summary>
    ''' <param name="IO_TBL">列追加対象テーブル</param>
    Public Sub AddColumnsToT0010Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If

        'モデル距離項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("TAISHOYM", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("WORKDATE", GetType(String))
        IO_TBL.Columns.Add("SAVECNT", GetType(Integer))
        IO_TBL.Columns.Add("SHARYOKBN1", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN1NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN1", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN1NAME", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO1", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO1NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE1", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE1NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE1", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN1", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN2", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN2NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN2", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN2NAME", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO2", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO2NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE2", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE2NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE2", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN2", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN3", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN3NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN3", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN3NAME", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO3", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO3NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE3", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE3NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE3", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN3", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN4", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN4NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN4", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN4NAME", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO4", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO4NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE4", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE4NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE4", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN4", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN5", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN5NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN5", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN5NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE5", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE5NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE5", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN5", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN6", GetType(String))
        IO_TBL.Columns.Add("SHARYOKBN6NAME", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN6", GetType(String))
        IO_TBL.Columns.Add("OILPAYKBN6NAME", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO6", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO6NAME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE6", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE6NAME", GetType(String))
        IO_TBL.Columns.Add("MODELDISTANCE6", GetType(Double))
        IO_TBL.Columns.Add("MODIFYKBN6", GetType(String))

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

                Case "WORKKBN"
                    '作業区分名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.getStaffCodeList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text))
                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "ORG"
                    '出荷部署名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.INVALID))
                Case "CREWKBN"
                    '実績登録区分名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))

            End Select
        End If

    End Sub

    ''' <summary>
    ''' 時間変換（分→時:分）
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Function MinituesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        WW_HHMM = Int(I_PARAM / 60) * 100 + I_PARAM Mod 60
        Return Format(WW_HHMM, "0#:##")
    End Function

    ''' <summary>
    ''' 変換（0 or 00:00をスペースへ）帳票出力用
    ''' </summary>
    ''' <param name="I_PARAM">変換元時刻</param>
    ''' <returns></returns>
    Function ZeroToSpace(ByVal I_PARAM As String) As String
        Dim WW_TIME As String() = I_PARAM.Split(":")

        ZeroToSpace = I_PARAM

        If WW_TIME.Count > 1 Then
            If I_PARAM = "00:00" Then
                ZeroToSpace = ""
            End If
        Else
            If Val(I_PARAM) = 0 Then
                ZeroToSpace = ""
            End If
        End If

    End Function

    '変換（時：分→分）
    ''' <summary>
    ''' 変換（時：分→分）
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Private Function HHMMtoMinutes(ByVal I_PARAM As String) As Integer
        Dim WW_TIME As String() = I_PARAM.Split(":")
        If I_PARAM = Nothing Then
            HHMMtoMinutes = 0
        Else
            HHMMtoMinutes = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
        End If

    End Function

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0002S Then                                                    '条件画面からの画面遷移
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            work.WF_DTL_XMLsaveF.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-TA0002-DTL-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        End If

        '※非表示にするCSSクラスをコントロールする
        Select Case work.WF_SEL_CAMPCODE.Text
            Case CONST_CAMP_ENEX
                detailbox.Attributes("class") = "Detail ENEX detailboxOnly"
            Case CONST_CAMP_KNK
                detailbox.Attributes("class") = "Detail KNK detailboxOnly"
            Case CONST_CAMP_NJS
                detailbox.Attributes("class") = "Detail NJS detailboxOnly"
            Case CONST_CAMP_JKT
                detailbox.Attributes("class") = "Detail JKT detailboxOnly"
        End Select
    End Sub

End Class



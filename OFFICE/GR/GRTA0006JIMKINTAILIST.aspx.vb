Imports System.IO.Compression
Imports System.Data.SqlClient

Public Class GRTA0006JIMKINTAILIST
    Inherits Page

    'コンスタント
    Const CONST_CAMP_ENEX As String = "02"                          '会社コード（エネックス）
    Const CONST_CAMP_KNK As String = "03"                           '会社コード（近石）
    Const CONST_CAMP_NJS As String = "04"                           '会社コード（NJS）
    Const CONST_CAMP_JKT As String = "05"                           '会社コード（JKT）

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    '一覧表示
    ''' <summary>
    ''' テーブルソート
    ''' </summary>    
    Private CS0026TblSort As New CS0026TBLSORT                      'テーブルソート
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' 帳票マージ出力
    ''' </summary>
    Private CS0047XLSMERGE As New CS0047XLSMERGE                    '帳票マージ出力
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション管理
    ''' <summary>
    ''' 勤怠共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通

    '検索結果格納ds
    Private TA0006ALL As DataTable                                  '全データテーブル
    Private TA0006VIEWtbl As DataTable                              'Grid格納用テーブル
    Private SELECTORtbl As DataTable                                'TREE選択作成作業テーブル
    '共通処理結果
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
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_RadioButonClick"           '■ 右ボックスラジオボタン選択時処理 
                        WF_RadioButon_Click()
                    Case "WF_MEMOChange"                '■ メモ欄保存処理
                        WF_MEMO_Change()
                    Case "WF_SELECTOR_SW_Click"         '■ セレクタ変更ラジオボタンクリック処理
                        SELECTOR_Click()
                End Select
                '○ 一覧再表示処理
                DisplayGrid()
            End If
        Else
            '〇初期化処理
            Initialize()

        End If

        '■ Close
        If Not IsNothing(TA0006ALL) Then
            TA0006ALL.Dispose()
            TA0006ALL = Nothing
        End If
        If Not IsNothing(TA0006VIEWtbl) Then
            TA0006VIEWtbl.Dispose()
            TA0006VIEWtbl = Nothing
        End If
        If Not IsNothing(SELECTORtbl) Then
            SELECTORtbl.Dispose()
            SELECTORtbl = Nothing
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
        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
        rightview.Initialize(WW_DUMMY)

        WF_SEL_DATE.Text = work.WF_SEL_TAISHOYM.Text
        CodeToName("ORG", work.WF_SEL_HORG.Text, WF_SEL_ORG.Text, WW_DUMMY)

        '■ 全データ取得
        '○TA0006ALL取得
        GetAllTA0006Tbl()

        '○表示選択TREE表示
        InitalSelector()
        '○画面表示データ保存
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0006ALL) Then Exit Sub
        '■ GridView表示データ作成
        AddColumnToTA0006Tbl(TA0006VIEWtbl)
        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0006VIEWtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0006WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '重複チェック
        Dim WW_MSG As String = C_MESSAGE_NO.NORMAL
        T0007COM.T0007_DuplCheck(TA0006ALL, WW_MSG, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        Else
            rightview.addErrorReport(ControlChars.NewLine & WW_MSG)
        End If
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ GridView表示データ作成
        AddColumnToTA0006Tbl(TA0006VIEWtbl)

        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0006VIEWtbl.Rows.Count - 1
            If TA0006VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0006VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Using WW_TBLview As DataView = New DataView(TA0006VIEWtbl)

            'ソート
            WW_TBLview.Sort = "LINECNT"
            WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
            '一覧作成

            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0006WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = WW_TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"
            CS0013ProfView.CS0013ProfView()

            '○クリア
            If WW_TBLview.Count = 0 Then
                WF_GridPosition.Text = "1"
            Else
                WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
            End If
        End Using
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
            WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK"
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\ユーザ名)　＆　前回処理ファイル削除
            WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
                'ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP\部署コード)　＆　前回処理ファイル削除
            WW_TEMPDir = CS0050Session.UPLOAD_PATH & "\TEXTWORK\TEMP" & "\" & work.WF_SEL_HORG.Text
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
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ 全選択Excel作成（メイン処理）

        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '○TA0006VIEWtbl取得
            If Not IsNothing(TA0006VIEWtbl) Then TA0006VIEWtbl.Clear()

            GetViewTA0006Tbl(CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text)

            '帳票出力用編集
            EditList(TA0006VIEWtbl)

            '○ 帳票出力
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = GRTA0006WRKINC.MAPID               '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0006VIEWtbl                        'データ参照DataTable
            CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
            CS0030REPORT.CS0030REPORT()
            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text & ".xlsx", True)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
                Exit Sub
            End If
        Next

        '■ ダウンロード
        '○ 圧縮実行
        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        Dim WW_Dir2 As String = ""

        '○ 帳票出力
        CS0047XLSMERGE.DIR = WW_Dir                                   'PARAM01:フォルダー
        CS0047XLSMERGE.CS0047XLSMERGE()
        If isNormal(CS0047XLSMERGE.ERR) Then
            WW_Dir2 = CS0047XLSMERGE.URL                              'PARAM02:出力EXCEL
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = WW_Dir2
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

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
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ 帳票出力
        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)

        '帳票出力用編集
        EditList(TA0006VIEWtbl)

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0006WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0006VIEWtbl                        'データ参照DataTable
        CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
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
            WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK"
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\ユーザ名)　＆　前回処理ファイル削除
            WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
                'ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP\部署コード)　＆　前回処理ファイル削除
            WW_TEMPDir = CS0050Session.UPLOAD_PATH & "\TEXTWORK\TEMP" & "\" & work.WF_SEL_HORG.Text
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
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ 全選択Excel作成（メイン処理）

        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '○TA0006VIEWtbl取得
            If Not IsNothing(TA0006VIEWtbl) Then TA0006VIEWtbl.Clear()
            GetViewTA0006Tbl(CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text)

            '帳票出力用編集
            EditList(TA0006VIEWtbl)

            '○ 帳票出力
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = GRTA0006WRKINC.MAPID               '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0006VIEWtbl                        'データ参照DataTable
            CS0030REPORT.TARGETDATE = work.WF_SEL_TAISHOYM.Text & "/01"     '対象日付
            CS0030REPORT.CS0030REPORT()
            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text & ".xlsx", True)

            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
                Exit Sub
            End If
        Next

        '■ ダウンロード
        '○ 圧縮実行
        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        Dim WW_Dir2 As String = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK\TEMP\" & work.WF_SEL_HORG.Text
        ZipFile.CreateFromDirectory(WW_Dir, WW_Dir2 & "\ALL.zip")

        WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/TEXT/TEMP/" & work.WF_SEL_HORG.Text & "/ALL.zip"
        '○ ダウンロード処理へ遷移
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

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
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If
        '■ GridView表示データ作成
        AddColumnToTA0006Tbl(TA0006VIEWtbl)
        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)
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
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ GridView表示データ作成
        AddColumnToTA0006Tbl(TA0006VIEWtbl)
        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)

        '○ソート
        Using WW_TBLview As DataView = New DataView(TA0006VIEWtbl)
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

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' TA0006All全表示データ取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetAllTA0006Tbl()

        '■ 画面表示用データ取得
        If IsNothing(TA0006ALL) Then TA0006ALL = New DataTable
        'TA0006テンポラリDB項目作成
        AddColumnToTA0006Tbl(TA0006ALL)

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'テンポラリーテーブルを作成する
                Dim SQLStr0 As String = "CREATE TABLE #MBtemp " _
                        & " ( " _
                        & "  CAMPCODE nvarchar(20)," _
                        & "  STAFFCODE nvarchar(20)," _
                        & "  HORG nvarchar(20)," _
                        & " ) "

                Using SQLcmd1 As New SqlCommand(SQLStr0, SQLcon)
                    SQLcmd1.CommandTimeout = 300
                    SQLcmd1.ExecuteNonQuery()
                End Using

                Dim SQLStr1 As String =
                     " SELECT  isnull(rtrim(MB1.CAMPCODE),'')      as  CAMPCODE,                         " _
                   & "              isnull(rtrim(MB1.STAFFCODE),'')     as  STAFFCODE                         " _
                   & " from   MB001_STAFF MB1                                                                 " _
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
                   & " INNER JOIN M0006_STRUCT   Z  " _
                   & "   ON    Z.CAMPCODE      = @CAMPCODE " _
                   & "   and   Z.OBJECT        = 'ORG' " _
                   & "   and   Z.GRCODE01      = @HORG " _
                   & "   and   Z.STRUCT        = '勤怠管理組織' " _
                   & "   and   Z.STYMD        <= @NOW " _
                   & "   and   Z.ENDYMD       >= @NOW " _
                   & "   and   Z.DELFLG       <> '1'  " _
                   & "   and   Z.CODE          = Y.CODE " _
                   & "   and   Z.CODE          = MB1.HORG " _
                   & " where  MB1.CAMPCODE                         =  @CAMPCODE                               " _
                   & "   and  MB1.STAFFKBN                        not like '03%'                              " _
                   & "   and  MB1.STYMD                           <=  @SEL_ENDYMD                             " _
                   & "   and  MB1.ENDYMD                          >=  @SEL_STYMD                              " _
                   & "   and  MB1.STAFFKBN                    NOT IN  ('01102','01412')                       " _
                   & "   and  MB1.DELFLG                          <>  '1'                                     " _
                   & " group by MB1.CAMPCODE, MB1.STAFFCODE                                                   "


                Dim WW_MBtbl As DataTable = New DataTable
                WW_MBtbl.Columns.Add("CAMPCODE", GetType(String))
                WW_MBtbl.Columns.Add("STAFFCODE", GetType(String))

                Using SQLcmd2 As New SqlCommand(SQLStr1, SQLcon)
                    Dim P2_CAMPCODE As SqlParameter = SQLcmd2.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_SEL_STYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                    Dim P2_SEL_ENDYMD As SqlParameter = SQLcmd2.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
                    Dim P2_HORG As SqlParameter = SQLcmd2.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_TERMID As SqlParameter = SQLcmd2.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar, 20)
                    Dim P2_NOW As SqlParameter = SQLcmd2.Parameters.Add("@NOW", System.Data.SqlDbType.Date)

                    P2_CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    P2_SEL_STYMD.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                    Dim wDATE2 As Date
                    Try
                        wDATE2 = work.WF_SEL_TAISHOYM.Text & "/01"
                    Catch ex As Exception
                        wDATE2 = Date.Now
                    End Try
                    P2_SEL_ENDYMD.Value = work.WF_SEL_TAISHOYM.Text & "/" & DateTime.DaysInMonth(wDATE2.Year, wDATE2.Month).ToString("00")

                    '袖２の場合、袖１に変換
                    Dim orgCode As String = ""
                    Dim retCode As String = ""
                    T0007COM.ConvORGCODE(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, orgCode, retCode)
                    If retCode = C_MESSAGE_NO.NORMAL Then
                        P2_HORG.Value = orgCode
                    Else
                        P2_HORG.Value = work.WF_SEL_HORG.Text
                    End If
                    P2_TERMID.Value = CS0050Session.APSV_ID
                    P2_NOW.Value = Date.Now

                    Dim SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()

                    WW_MBtbl.Load(SQLdr2)
                    SQLdr2.Close()
                    SQLdr2 = Nothing
                    '一旦テンポラリテーブルに出力
                    Dim bc As New SqlClient.SqlBulkCopy(SQLcon)
                    bc.DestinationTableName = "#MBtemp"
                    bc.WriteToServer(WW_MBtbl)
                    bc.Close()
                    bc = Nothing
                End Using

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
                   & "       CASE WHEN A.HORG = '021506' THEN F13.VALUE2 ELSE F13.VALUE1 END AS SHUKCHOKKBNNAMES , " _
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
                   & "       isnull(A.ORVERTIMEADD,0) as ORVERTIMEADD , " _
                   & "       isnull(A.ORVERTIME,0) + isnull(A.ORVERTIMECHO,0) as ORVERTIMETTL , " _
                   & "       isnull(A.WNIGHTTIME,0) as WNIGHTTIME , " _
                   & "       isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMECHO , " _
                   & "       isnull(A.WNIGHTTIMEADD,0) as WNIGHTTIMEADD , " _
                   & "       isnull(A.WNIGHTTIME,0) + isnull(A.WNIGHTTIMECHO,0) as WNIGHTTIMETTL , " _
                   & "       isnull(A.SWORKTIME,0) as SWORKTIME , " _
                   & "       isnull(A.SWORKTIMECHO,0) as SWORKTIMECHO , " _
                   & "       isnull(A.SWORKTIMEADD,0) as SWORKTIMEADD , " _
                   & "       isnull(A.SWORKTIME,0) + isnull(A.SWORKTIMECHO,0) as SWORKTIMETTL , " _
                   & "       isnull(A.SNIGHTTIME,0) as SNIGHTTIME , " _
                   & "       isnull(A.SNIGHTTIMECHO,0) as SNIGHTTIMECHO , " _
                   & "       isnull(A.SNIGHTTIMEADD,0) as SNIGHTTIMEADD , " _
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
                   & "       isnull(rtrim(A.YENDTIME),'') as YENDTIME , " _
                   & "       isnull(rtrim(A.RIYU),'') as RIYU , " _
                   & "       isnull(rtrim(F12.VALUE1),'') as RIYUNAME , " _
                   & "       isnull(rtrim(A.RIYUETC),'') as RIYUETC , " _
                   & "       isnull(A.JIKYUSHATIME, 0) as JIKYUSHATIME , " _
                   & "       isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMECHO , " _
                   & "       isnull(A.JIKYUSHATIME, 0) + isnull(A.JIKYUSHATIMECHO, 0) as JIKYUSHATIMETTL , " _
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
                   & "       isnull(A.SDAINIGHTTIME, 0) + isnull(A.SDAINIGHTTIMECHO, 0) as SDAINIGHTTIMETTL  " _
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
                   & "   and   MB2.STYMD       <= @SEL_ENDYMD " _
                   & "   and   MB2.ENDYMD      >= @SEL_STYMD " _
                   & "   and   MB2.STYMD        = (SELECT MAX(STYMD) FROM MB001_STAFF WHERE CAMPCODE = @CAMPCODE and STAFFCODE = MB.STAFFCODE and STYMD <= @SEL_ENDYMD and ENDYMD >= @SEL_STYMD and DELFLG <> '1' ) " _
                   & "   and   MB2.DELFLG      <> '1' " _
                   & " LEFT JOIN MB005_CALENDAR CAL " _
                   & "   ON    CAL.CAMPCODE    = @CAMPCODE " _
                   & "   and   CAL.WORKINGYMD  = A.WORKDATE " _
                   & "   and   CAL.DELFLG     <> '1' " _
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
                   & "   and   F11.CLASS       = 'T0009_SHUKCHOKKBN' " _
                   & "   and   F11.KEYCODE     = A.SHUKCHOKKBN " _
                   & "   and   F11.STYMD      <= @STYMD " _
                   & "   and   F11.ENDYMD     >= @ENDYMD " _
                   & "   and   F11.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F12 " _
                   & "   ON    F12.CAMPCODE    = @CAMPCODE " _
                   & "   and   F12.CLASS       = 'T0009_RIYU' " _
                   & "   and   F12.KEYCODE     = A.RIYU " _
                   & "   and   F12.STYMD      <= @STYMD " _
                   & "   and   F12.ENDYMD     >= @ENDYMD " _
                   & "   and   F12.DELFLG     <> '1' " _
                   & " LEFT JOIN MC001_FIXVALUE F13" _
                   & "   ON    F13.CAMPCODE    = @CAMPCODE " _
                   & "   and   F13.CLASS       = 'T0009_SHUKCHOKKBN' " _
                   & "   and   F13.KEYCODE     = A.SHUKCHOKKBN " _
                   & "   and   F13.STYMD      <= @STYMD " _
                   & "   and   F13.ENDYMD     >= @STYMD " _
                   & "   and   F13.DELFLG     <> '1' " _
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
                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar, 7)
                Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim P_SEL_STYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_STYMD", System.Data.SqlDbType.Date)
                Dim P_SEL_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@SEL_ENDYMD", System.Data.SqlDbType.Date)
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

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '■テーブル検索結果をテーブル格納
                TA0006ALL.Load(SQLdr)

                For Each TA0006ALLrow As DataRow In TA0006ALL.Rows

                    Dim WW_LINEcnt As Integer = 0
                    TA0006ALLrow("SELECT") = "1"
                    If TA0006ALLrow("HDKBN") = "H" Then
                        TA0006ALLrow("HIDDEN") = "0"      '表示
                        WW_LINEcnt += 1
                        TA0006ALLrow("LINECNT") = WW_LINEcnt
                    Else
                        TA0006ALLrow("HIDDEN") = "1"      '非表示
                        TA0006ALLrow("LINECNT") = 0
                    End If

                    TA0006ALLrow("SEQ") = CInt(TA0006ALLrow("SEQ")).ToString("000")
                    If IsDate(TA0006ALLrow("WORKDATE")) Then
                        TA0006ALLrow("WORKDATE") = CDate(TA0006ALLrow("WORKDATE")).ToString("yyyy/MM/dd")
                    Else
                        TA0006ALLrow("WORKDATE") = ""
                    End If
                    If IsDate(TA0006ALLrow("STDATE")) Then
                        TA0006ALLrow("STDATE") = CDate(TA0006ALLrow("STDATE")).ToString("yyyy/MM/dd")
                    Else
                        TA0006ALLrow("STDATE") = ""
                    End If
                    If IsDate(TA0006ALLrow("STTIME")) Then
                        TA0006ALLrow("STTIME") = ZEROtoSpace(CDate(TA0006ALLrow("STTIME")).ToString("HH:mm"))
                    Else
                        TA0006ALLrow("STTIME") = ""
                    End If
                    If IsDate(TA0006ALLrow("ENDDATE")) Then
                        TA0006ALLrow("ENDDATE") = CDate(TA0006ALLrow("ENDDATE")).ToString("yyyy/MM/dd")
                    Else
                        TA0006ALLrow("ENDDATE") = ""
                    End If
                    If IsDate(TA0006ALLrow("ENDTIME")) Then
                        TA0006ALLrow("ENDTIME") = ZEROtoSpace(CDate(TA0006ALLrow("ENDTIME")).ToString("HH:mm"))
                    Else
                        TA0006ALLrow("ENDTIME") = ""
                    End If
                    If IsDate(TA0006ALLrow("YENDTIME")) Then
                        TA0006ALLrow("YENDTIME") = ZEROtoSpace(CDate(TA0006ALLrow("YENDTIME")).ToString("HH:mm"))
                    Else
                        TA0006ALLrow("YENDTIME") = ""
                    End If
                    If IsDate(TA0006ALLrow("BINDSTDATE")) Then
                        TA0006ALLrow("BINDSTDATE") = ZEROtoSpace(CDate(TA0006ALLrow("BINDSTDATE")).ToString("HH:mm"))
                    Else
                        TA0006ALLrow("BINDSTDATE") = ""
                    End If

                    If TA0006ALLrow("STDATE") <> TA0006ALLrow("ENDDATE") Then
                        TA0006ALLrow("ENDDATE_TXT") = "翌"
                    Else
                        TA0006ALLrow("ENDDATE_TXT") = ""
                    End If

                    TA0006ALLrow("WORKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("WORKTIME")))
                    TA0006ALLrow("MOVETIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("MOVETIME")))
                    TA0006ALLrow("ACTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("ACTTIME")))
                    If (TA0006ALLrow("STTIME") = "" AndAlso TA0006ALLrow("ENDTIME") = "") Then
                        TA0006ALLrow("BINDTIME") = ""
                    Else
                        TA0006ALLrow("BINDTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("BINDTIME")))
                    End If
                    TA0006ALLrow("NIPPOBREAKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("NIPPOBREAKTIME")))
                    TA0006ALLrow("BREAKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("BREAKTIME")))
                    TA0006ALLrow("BREAKTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("BREAKTIMECHO")))
                    TA0006ALLrow("BREAKTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("BREAKTIMETTL")))
                    TA0006ALLrow("NIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("NIGHTTIME")))
                    TA0006ALLrow("NIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("NIGHTTIMECHO")))
                    TA0006ALLrow("NIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("NIGHTTIMETTL")))
                    TA0006ALLrow("ORVERTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("ORVERTIME")))
                    TA0006ALLrow("ORVERTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("ORVERTIMECHO")))
                    TA0006ALLrow("ORVERTIMEADD") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("ORVERTIMEADD")))
                    TA0006ALLrow("ORVERTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("ORVERTIMETTL")))
                    TA0006ALLrow("WNIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("WNIGHTTIME")))
                    TA0006ALLrow("WNIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("WNIGHTTIMECHO")))
                    TA0006ALLrow("WNIGHTTIMEADD") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("WNIGHTTIMEADD")))
                    TA0006ALLrow("WNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("WNIGHTTIMETTL")))
                    TA0006ALLrow("SWORKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SWORKTIME")))
                    TA0006ALLrow("SWORKTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SWORKTIMECHO")))
                    TA0006ALLrow("SWORKTIMEADD") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SWORKTIMEADD")))
                    TA0006ALLrow("SWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SWORKTIMETTL")))
                    TA0006ALLrow("SNIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SNIGHTTIME")))
                    TA0006ALLrow("SNIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SNIGHTTIMECHO")))
                    TA0006ALLrow("SNIGHTTIMEADD") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SNIGHTTIMEADD")))
                    TA0006ALLrow("SNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SNIGHTTIMETTL")))
                    TA0006ALLrow("HWORKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HWORKTIME")))
                    TA0006ALLrow("HWORKTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HWORKTIMECHO")))
                    TA0006ALLrow("HWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HWORKTIMETTL")))
                    TA0006ALLrow("HNIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HNIGHTTIME")))
                    TA0006ALLrow("HNIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HNIGHTTIMECHO")))
                    TA0006ALLrow("HNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HNIGHTTIMETTL")))
                    TA0006ALLrow("HOANTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HOANTIME")))
                    TA0006ALLrow("HOANTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HOANTIMECHO")))
                    TA0006ALLrow("HOANTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HOANTIMETTL")))
                    TA0006ALLrow("KOATUTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("KOATUTIME")))
                    TA0006ALLrow("KOATUTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("KOATUTIMECHO")))
                    TA0006ALLrow("KOATUTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("KOATUTIMETTL")))
                    TA0006ALLrow("TOKUSA1TIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("TOKUSA1TIME")))
                    TA0006ALLrow("TOKUSA1TIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("TOKUSA1TIMECHO")))
                    TA0006ALLrow("TOKUSA1TIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("TOKUSA1TIMETTL")))
                    TA0006ALLrow("HAYADETIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HAYADETIME")))
                    TA0006ALLrow("HAYADETIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HAYADETIMECHO")))
                    TA0006ALLrow("HAYADETIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HAYADETIMETTL")))
                    TA0006ALLrow("JIKYUSHATIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("JIKYUSHATIME")))
                    TA0006ALLrow("JIKYUSHATIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("JIKYUSHATIMECHO")))
                    TA0006ALLrow("JIKYUSHATIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("JIKYUSHATIMETTL")))
                    TA0006ALLrow("HDAIWORKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAIWORKTIME")))
                    TA0006ALLrow("HDAIWORKTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAIWORKTIMECHO")))
                    TA0006ALLrow("HDAIWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAIWORKTIMETTL")))
                    TA0006ALLrow("HDAINIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAINIGHTTIME")))
                    TA0006ALLrow("HDAINIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAINIGHTTIMECHO")))
                    TA0006ALLrow("HDAINIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("HDAINIGHTTIMETTL")))
                    TA0006ALLrow("SDAIWORKTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAIWORKTIME")))
                    TA0006ALLrow("SDAIWORKTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAIWORKTIMECHO")))
                    TA0006ALLrow("SDAIWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAIWORKTIMETTL")))
                    TA0006ALLrow("SDAINIGHTTIME") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAINIGHTTIME")))
                    TA0006ALLrow("SDAINIGHTTIMECHO") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAINIGHTTIMECHO")))
                    TA0006ALLrow("SDAINIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(TA0006ALLrow("SDAINIGHTTIMETTL")))

                    TA0006ALLrow("HAIDISTANCE") = Val(TA0006ALLrow("HAIDISTANCE"))
                    TA0006ALLrow("HAIDISTANCECHO") = Val(TA0006ALLrow("HAIDISTANCECHO"))
                    TA0006ALLrow("HAIDISTANCETTL") = Val(TA0006ALLrow("HAIDISTANCETTL"))
                    TA0006ALLrow("KAIDISTANCE") = Val(TA0006ALLrow("KAIDISTANCE"))
                    TA0006ALLrow("KAIDISTANCETTL") = Val(TA0006ALLrow("KAIDISTANCETTL"))

                    TA0006ALLrow("RYOME") = TA0006ALLrow("RYOME")
                    TA0006ALLrow("PRODUCT1") = TA0006ALLrow("PRODUCT1")
                    TA0006ALLrow("PRODUCT1NAMES") = TA0006ALLrow("PRODUCT1NAMES")
                    TA0006ALLrow("SHUKOTIME") = TA0006ALLrow("SHUKOTIME")
                    TA0006ALLrow("KIKOTIME") = TA0006ALLrow("KIKOTIME")
                    TA0006ALLrow("HANDLETIME") = TA0006ALLrow("HANDLETIME")
                    TA0006ALLrow("TRIPNO") = TA0006ALLrow("TRIPNO")
                    TA0006ALLrow("SURYO") = TA0006ALLrow("SURYO")


                    '名前の取得
                    TA0006ALLrow("CAMPNAMES") = ""
                    CodeToName("CAMPCODE", TA0006ALLrow("CAMPCODE"), TA0006ALLrow("CAMPNAMES"), WW_DUMMY)
                    If TA0006ALLrow("STAFFNAMES") = "" Then
                        TA0006ALLrow("STAFFNAMES") = ""
                        CodeToName("STAFFCODE", TA0006ALLrow("STAFFCODE"), TA0006ALLrow("STAFFNAMES"), WW_DUMMY)
                    End If
                    TA0006ALLrow("MORGNAMES") = ""
                    CodeToName("ORG", TA0006ALLrow("MORG"), TA0006ALLrow("MORGNAMES"), WW_DUMMY)

                    If TA0006ALLrow("HORG") = "" Then
                        TA0006ALLrow("HORG") = work.WF_SEL_HORG.Text
                        TA0006ALLrow("HORGNAMES") = ""
                        CodeToName("ORG", TA0006ALLrow("HORG"), TA0006ALLrow("HORGNAMES"), WW_DUMMY)
                    Else
                        TA0006ALLrow("HORGNAMES") = ""
                        CodeToName("ORG", TA0006ALLrow("HORG"), TA0006ALLrow("HORGNAMES"), WW_DUMMY)
                    End If

                    If TA0006ALLrow("SORG") = "" Then
                        TA0006ALLrow("SORG") = TA0006ALLrow("HORG")
                    End If
                    TA0006ALLrow("SORGNAMES") = ""
                    CodeToName("ORG", TA0006ALLrow("SORG"), TA0006ALLrow("SORGNAMES"), WW_DUMMY)


                    '○表示項目編集
                    If TA0006ALLrow("CAMPNAMES") = Nothing AndAlso TA0006ALLrow("CAMPCODE") = Nothing Then
                        TA0006ALLrow("CAMPCODE_TXT") = ""
                    Else
                        TA0006ALLrow("CAMPCODE_TXT") = TA0006ALLrow("CAMPNAMES") & " (" & TA0006ALLrow("CAMPCODE") & ")"
                    End If

                    TA0006ALLrow("TAISHOYM_TXT") = TA0006ALLrow("TAISHOYM")

                    If TA0006ALLrow("STAFFNAMES") = Nothing AndAlso TA0006ALLrow("STAFFCODE") = Nothing Then
                        TA0006ALLrow("STAFFCODE_TXT") = ""
                    Else
                        TA0006ALLrow("STAFFCODE_TXT") = TA0006ALLrow("STAFFNAMES") & " (" & TA0006ALLrow("STAFFCODE") & ")"
                    End If

                    If IsDate(TA0006ALLrow("WORKDATE")) Then
                        TA0006ALLrow("WORKDATE_TXT") = CDate(TA0006ALLrow("WORKDATE")).ToString("dd")
                    Else
                        TA0006ALLrow("WORKDATE_TXT") = ""
                    End If

                    If TA0006ALLrow("WORKINGWEEKNAMES") = Nothing Then
                        TA0006ALLrow("WORKINGWEEK_TXT") = ""
                    Else
                        TA0006ALLrow("WORKINGWEEK_TXT") = TA0006ALLrow("WORKINGWEEKNAMES")
                    End If

                    TA0006ALLrow("HDKBN_TXT") = TA0006ALLrow("HDKBN")

                    If TA0006ALLrow("RECODEKBNNAMES") = Nothing AndAlso TA0006ALLrow("RECODEKBN") = Nothing Then
                        TA0006ALLrow("RECODEKBN_TXT") = ""
                    Else
                        TA0006ALLrow("RECODEKBN_TXT") = TA0006ALLrow("RECODEKBNNAMES") & " (" & TA0006ALLrow("RECODEKBN") & ")"
                    End If

                    If TA0006ALLrow("WORKKBNNAMES") = Nothing AndAlso TA0006ALLrow("WORKKBN") = Nothing Then
                        TA0006ALLrow("WORKKBN_TXT") = ""
                    Else
                        TA0006ALLrow("WORKKBN_TXT") = TA0006ALLrow("WORKKBNNAMES") & " (" & TA0006ALLrow("WORKKBN") & ")"
                    End If

                    If TA0006ALLrow("SHARYOKBNNAMES") = Nothing AndAlso TA0006ALLrow("SHARYOKBN") = Nothing Then
                        TA0006ALLrow("SHARYOKBN_TXT") = ""
                    Else
                        TA0006ALLrow("SHARYOKBN_TXT") = TA0006ALLrow("SHARYOKBNNAMES") & " (" & TA0006ALLrow("SHARYOKBN") & ")"
                    End If

                    If TA0006ALLrow("OILPAYKBNNAMES") = Nothing AndAlso TA0006ALLrow("OILPAYKBN") = Nothing Then
                        TA0006ALLrow("OILPAYKBN_TXT") = ""
                    Else
                        TA0006ALLrow("OILPAYKBN_TXT") = TA0006ALLrow("OILPAYKBNNAMES") & " (" & TA0006ALLrow("OILPAYKBN") & ")"
                    End If

                    If TA0006ALLrow("STAFFKBNNAMES") = Nothing AndAlso TA0006ALLrow("STAFFKBN") = Nothing Then
                        TA0006ALLrow("STAFFKBN_TXT") = ""
                    Else
                        TA0006ALLrow("STAFFKBN_TXT") = TA0006ALLrow("STAFFKBNNAMES") & " (" & TA0006ALLrow("STAFFKBN") & ")"
                    End If

                    If TA0006ALLrow("MORGNAMES") = Nothing AndAlso TA0006ALLrow("MORG") = Nothing Then
                        TA0006ALLrow("MORG_TXT") = ""
                    Else
                        TA0006ALLrow("MORG_TXT") = TA0006ALLrow("MORGNAMES") & " (" & TA0006ALLrow("MORG") & ")"
                    End If

                    If TA0006ALLrow("HORGNAMES") = Nothing AndAlso TA0006ALLrow("HORG") = Nothing Then
                        TA0006ALLrow("HORG_TXT") = ""
                    Else
                        TA0006ALLrow("HORG_TXT") = TA0006ALLrow("HORGNAMES") & " (" & TA0006ALLrow("HORG") & ")"
                    End If

                    If TA0006ALLrow("SORGNAMES") = Nothing AndAlso TA0006ALLrow("SORG") = Nothing Then
                        TA0006ALLrow("SORG_TXT") = ""
                    Else
                        TA0006ALLrow("SORG_TXT") = TA0006ALLrow("SORGNAMES") & " (" & TA0006ALLrow("SORG") & ")"
                    End If

                    If TA0006ALLrow("HOLIDAYKBNNAMES") = Nothing AndAlso TA0006ALLrow("HOLIDAYKBN") = Nothing Then
                        TA0006ALLrow("HOLIDAYKBN_TXT") = ""
                    Else
                        TA0006ALLrow("HOLIDAYKBN_TXT") = TA0006ALLrow("HOLIDAYKBNNAMES") & " (" & TA0006ALLrow("HOLIDAYKBN") & ")"
                    End If

                    If TA0006ALLrow("PAYKBNNAMES") = Nothing AndAlso TA0006ALLrow("PAYKBN") = Nothing Then
                        TA0006ALLrow("PAYKBN_TXT") = ""
                    Else
                        TA0006ALLrow("PAYKBN_TXT") = TA0006ALLrow("PAYKBNNAMES") & " (" & TA0006ALLrow("PAYKBN") & ")"
                    End If

                    If TA0006ALLrow("SHUKCHOKKBNNAMES") = Nothing AndAlso TA0006ALLrow("SHUKCHOKKBN") = Nothing Then
                        TA0006ALLrow("SHUKCHOKKBN_TXT") = ""
                    Else
                        Dim WW_TEXT As String = ""
                        'CodeToName("SHUKCHOKKBN", TA0006ALLrow("SHUKCHOKKBN"), WW_TEXT, WW_DUMMY)
                        'TA0006ALLrow("SHUKCHOKKBNNAMES") = WW_TEXT

                        TA0006ALLrow("SHUKCHOKKBN_TXT") = TA0006ALLrow("SHUKCHOKKBNNAMES") & " (" & TA0006ALLrow("SHUKCHOKKBN") & ")"
                    End If

                    TA0006ALLrow("DELFLG_TXT") = TA0006ALLrow("DELFLG")

                    TA0006ALLrow("PRODUCT1_TXT") = TA0006ALLrow("PRODUCT1")
                    If TA0006ALLrow("PRODUCT1NAMES") = Nothing AndAlso TA0006ALLrow("PRODUCT1") = Nothing Then
                        TA0006ALLrow("PRODUCT1_TXT") = ""
                    Else
                        TA0006ALLrow("PRODUCT1_TXT") = TA0006ALLrow("PRODUCT1NAMES") & " (" & TA0006ALLrow("PRODUCT1") & ")"
                    End If

                Next

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

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
        CS0026TblSort.TABLE = TA0006ALL
        CS0026TblSort.SORTING = "LINECNT, SEQ"
        CS0026TblSort.FILTER = "SELECT = 1"
        TA0006ALL = CS0026TblSort.sort()
    End Sub

    ''' <summary>
    ''' TA0006VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <param name="I_CODE">条件コード</param>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0006Tbl(ByVal I_CODE As String)

        '〇 TA0006ALLよりデータ抽出
        CS0026TblSort.TABLE = TA0006ALL
        CS0026TblSort.SORTING = "LINECNT , SEQ ASC"
        CS0026TblSort.FILTER = "STAFFCODE = '" & I_CODE & "'"
        TA0006VIEWtbl = CS0026TblSort.sort()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0006VIEWrow As DataRow In TA0006VIEWtbl.Rows
            TA0006VIEWrow("LINECNT") = 0
        Next

        For Each TA0006VIEWrow As DataRow In TA0006VIEWtbl.Rows
            If TA0006VIEWrow("LINECNT") = 0 AndAlso TA0006VIEWrow("HDKBN") = "H" Then
                TA0006VIEWrow("SELECT") = "1"
                TA0006VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0006VIEWrow("LINECNT") = WW_LINECNT
            End If
        Next

    End Sub

    ''' <summary>
    '''  帳票用編集
    ''' </summary>
    ''' <param name="IO_TBL">編集対象テーブル</param>
    ''' <remarks></remarks>
    Protected Sub EditList(ByRef IO_TBL As DataTable)
        Dim WW_LINEcnt As Integer = 0

        Dim WW_TA0006tbl As DataTable = IO_TBL.Clone
        Dim WW_TA0006row As DataRow

        For i As Integer = 0 To IO_TBL.Rows.Count - 1
            WW_TA0006row = WW_TA0006tbl.NewRow
            WW_TA0006row.ItemArray = IO_TBL.Rows(i).ItemArray

            If WW_TA0006row("HDKBN") <> "H" Then Continue For
            '---------------------------------------------------
            '勤務状況リスト編集 （日報が存在しない場合の編集）
            '---------------------------------------------------
            WW_TA0006row("TAISHOYM_TXT") = Mid(WW_TA0006row("TAISHOYM"), 1, 4) & "年" & Mid(WW_TA0006row("TAISHOYM"), 6, 2) & "月"

            If WW_TA0006row("PAYKBN") = "00" Then
                WW_TA0006row("PAYKBN_TXT") = ""
                WW_TA0006row("PAYKBNNAMES") = ""
            End If

            If WW_TA0006row("SHUKCHOKKBN") = "0" Then
                WW_TA0006row("SHUKCHOKKBN_TXT") = ""
                WW_TA0006row("SHUKCHOKKBNNAMES") = ""
            End If

            If WW_TA0006row("HOLIDAYKBN") = "0" Then
                WW_TA0006row("HOLIDAYKBN_TXT") = ""
                WW_TA0006row("HOLIDAYKBNNAMES") = ""
            End If

            WW_TA0006row("STTIME") = ZeroToSpace(WW_TA0006row("STTIME"))
            WW_TA0006row("ENDTIME") = ZeroToSpace(WW_TA0006row("ENDTIME"))
            If WW_TA0006row("STTIME") = "" AndAlso WW_TA0006row("ENDTIME") = "" Then
                WW_TA0006row("STDATE") = ""
                WW_TA0006row("ENDDATE") = ""
            End If

            WW_TA0006row("WORKTIME") = ZeroToSpace(WW_TA0006row("WORKTIME"))
            WW_TA0006row("MOVETIME") = ZeroToSpace(WW_TA0006row("MOVETIME"))
            WW_TA0006row("ACTTIME") = ZeroToSpace(WW_TA0006row("ACTTIME"))
            If HHMMToMinutes(WW_TA0006row("ACTTIME")) >= 960 Then
                If WW_TA0006row("RECODEKBN") = "0" Then
                    '16時間を超える場合
                    WW_TA0006row("ORVER15") = "*"
                    WW_TA0006row("ORVER15_TXT") = "*"
                Else
                    WW_TA0006row("ORVER15") = ""
                    WW_TA0006row("ORVER15_TXT") = ""
                End If
            End If

            '1:法定休日、2:法定外休日
            '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
            '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
            If WW_TA0006row("HOLIDAYKBN") = "1" OrElse
               WW_TA0006row("HOLIDAYKBN") = "2" OrElse
               WW_TA0006row("PAYKBN") = "01" OrElse
               WW_TA0006row("PAYKBN") = "02" OrElse
               WW_TA0006row("PAYKBN") = "04" OrElse
               WW_TA0006row("PAYKBN") = "05" OrElse
               WW_TA0006row("PAYKBN") = "06" OrElse
               WW_TA0006row("PAYKBN") = "07" OrElse
               WW_TA0006row("PAYKBN") = "08" OrElse
               WW_TA0006row("PAYKBN") = "09" OrElse
               WW_TA0006row("PAYKBN") = "11" OrElse
               WW_TA0006row("PAYKBN") = "13" OrElse
               WW_TA0006row("PAYKBN") = "15" OrElse
              (WW_TA0006row("STTIME") = "" AndAlso WW_TA0006row("ENDTIME") = "") Then
                WW_TA0006row("BINDTIME") = ""
            Else
                WW_TA0006row("BINDTIME") = ZeroToSpace(WW_TA0006row("BINDTIME"))
            End If
            WW_TA0006row("BINDSTDATE") = ZeroToSpace(WW_TA0006row("BINDSTDATE"))
            WW_TA0006row("BREAKTIME") = ZeroToSpace(WW_TA0006row("BREAKTIME"))
            WW_TA0006row("BREAKTIMECHO") = ZeroToSpace(WW_TA0006row("BREAKTIMECHO"))
            WW_TA0006row("BREAKTIMETTL") = ZeroToSpace(WW_TA0006row("BREAKTIMETTL"))
            WW_TA0006row("NIGHTTIME") = ZeroToSpace(WW_TA0006row("NIGHTTIME"))
            WW_TA0006row("NIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("NIGHTTIMECHO"))
            WW_TA0006row("NIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("NIGHTTIMETTL"))
            WW_TA0006row("ORVERTIME") = ZeroToSpace(WW_TA0006row("ORVERTIME"))
            WW_TA0006row("ORVERTIMECHO") = ZeroToSpace(WW_TA0006row("ORVERTIMECHO"))
            WW_TA0006row("ORVERTIMEADD") = ZeroToSpace(WW_TA0006row("ORVERTIMEADD"))
            WW_TA0006row("ORVERTIMETTL") = ZeroToSpace(WW_TA0006row("ORVERTIMETTL"))
            WW_TA0006row("WNIGHTTIME") = ZeroToSpace(WW_TA0006row("WNIGHTTIME"))
            WW_TA0006row("WNIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("WNIGHTTIMECHO"))
            WW_TA0006row("WNIGHTTIMEADD") = ZeroToSpace(WW_TA0006row("WNIGHTTIMEADD"))
            WW_TA0006row("WNIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("WNIGHTTIMETTL"))
            WW_TA0006row("SWORKTIME") = ZeroToSpace(WW_TA0006row("SWORKTIME"))
            WW_TA0006row("SWORKTIMECHO") = ZeroToSpace(WW_TA0006row("SWORKTIMECHO"))
            WW_TA0006row("SWORKTIMEADD") = ZeroToSpace(WW_TA0006row("SWORKTIMEADD"))
            WW_TA0006row("SWORKTIMETTL") = ZeroToSpace(WW_TA0006row("SWORKTIMETTL"))
            WW_TA0006row("SNIGHTTIME") = ZeroToSpace(WW_TA0006row("SNIGHTTIME"))
            WW_TA0006row("SNIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("SNIGHTTIMECHO"))
            WW_TA0006row("SNIGHTTIMEADD") = ZeroToSpace(WW_TA0006row("SNIGHTTIMEADD"))
            WW_TA0006row("SNIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("SNIGHTTIMETTL"))
            WW_TA0006row("HWORKTIME") = ZeroToSpace(WW_TA0006row("HWORKTIME"))
            WW_TA0006row("HWORKTIMECHO") = ZeroToSpace(WW_TA0006row("HWORKTIMECHO"))
            WW_TA0006row("HWORKTIMETTL") = ZeroToSpace(WW_TA0006row("HWORKTIMETTL"))
            WW_TA0006row("HNIGHTTIME") = ZeroToSpace(WW_TA0006row("HNIGHTTIME"))
            WW_TA0006row("HNIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("HNIGHTTIMECHO"))
            WW_TA0006row("HNIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("HNIGHTTIMETTL"))
            WW_TA0006row("HOANTIME") = ZeroToSpace(WW_TA0006row("HOANTIME"))
            WW_TA0006row("HOANTIMECHO") = ZeroToSpace(WW_TA0006row("HOANTIMECHO"))
            WW_TA0006row("HOANTIMETTL") = ZeroToSpace(WW_TA0006row("HOANTIMETTL"))
            WW_TA0006row("KOATUTIME") = ZeroToSpace(WW_TA0006row("KOATUTIME"))
            WW_TA0006row("KOATUTIMECHO") = ZeroToSpace(WW_TA0006row("KOATUTIMECHO"))
            WW_TA0006row("KOATUTIMETTL") = ZeroToSpace(WW_TA0006row("KOATUTIMETTL"))
            WW_TA0006row("TOKUSA1TIME") = ZeroToSpace(WW_TA0006row("TOKUSA1TIME"))
            WW_TA0006row("TOKUSA1TIMECHO") = ZeroToSpace(WW_TA0006row("TOKUSA1TIMECHO"))
            WW_TA0006row("TOKUSA1TIMETTL") = ZeroToSpace(WW_TA0006row("TOKUSA1TIMETTL"))
            WW_TA0006row("HAYADETIME") = ZeroToSpace(WW_TA0006row("HAYADETIME"))
            WW_TA0006row("HAYADETIMECHO") = ZeroToSpace(WW_TA0006row("HAYADETIMECHO"))
            WW_TA0006row("HAYADETIMETTL") = ZeroToSpace(WW_TA0006row("HAYADETIMETTL"))
            WW_TA0006row("TOKSAAKAISU") = ZeroToSpace(WW_TA0006row("TOKSAAKAISU"))
            WW_TA0006row("TOKSAAKAISUCHO") = ZeroToSpace(WW_TA0006row("TOKSAAKAISUCHO"))
            WW_TA0006row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TA0006row("TOKSAAKAISUTTL"))
            WW_TA0006row("TOKSABKAISU") = ZeroToSpace(WW_TA0006row("TOKSABKAISU"))
            WW_TA0006row("TOKSABKAISUCHO") = ZeroToSpace(WW_TA0006row("TOKSABKAISUCHO"))
            WW_TA0006row("TOKSABKAISUTTL") = ZeroToSpace(WW_TA0006row("TOKSABKAISUTTL"))
            WW_TA0006row("TOKSACKAISU") = ZeroToSpace(WW_TA0006row("TOKSACKAISU"))
            WW_TA0006row("TOKSACKAISUCHO") = ZeroToSpace(WW_TA0006row("TOKSACKAISUCHO"))
            WW_TA0006row("TOKSACKAISUTTL") = ZeroToSpace(WW_TA0006row("TOKSACKAISUTTL"))
            WW_TA0006row("HAIDISTANCE") = ZeroToSpace(WW_TA0006row("HAIDISTANCE"))
            WW_TA0006row("HAIDISTANCECHO") = ZeroToSpace(WW_TA0006row("HAIDISTANCECHO"))
            WW_TA0006row("HAIDISTANCETTL") = ZeroToSpace(WW_TA0006row("HAIDISTANCETTL"))
            WW_TA0006row("UNLOADCNT") = ZeroToSpace(WW_TA0006row("UNLOADCNT"))
            WW_TA0006row("UNLOADCNTCHO") = ZeroToSpace(WW_TA0006row("UNLOADCNTCHO"))
            WW_TA0006row("UNLOADCNTTTL") = ZeroToSpace(WW_TA0006row("UNLOADCNTTTL"))
            WW_TA0006row("SURYO") = ZeroToSpace(WW_TA0006row("SURYO"))
            WW_TA0006row("YENDTIME") = ZeroToSpace(WW_TA0006row("YENDTIME"))
            WW_TA0006row("JIKYUSHATIME") = ZeroToSpace(WW_TA0006row("JIKYUSHATIME"))
            WW_TA0006row("JIKYUSHATIMECHO") = ZeroToSpace(WW_TA0006row("JIKYUSHATIMECHO"))
            WW_TA0006row("JIKYUSHATIMETTL") = ZeroToSpace(WW_TA0006row("JIKYUSHATIMETTL"))
            WW_TA0006row("HDAIWORKTIME") = ZeroToSpace(WW_TA0006row("HDAIWORKTIME"))
            WW_TA0006row("HDAIWORKTIMECHO") = ZeroToSpace(WW_TA0006row("HDAIWORKTIMECHO"))
            WW_TA0006row("HDAIWORKTIMETTL") = ZeroToSpace(WW_TA0006row("HDAIWORKTIMETTL"))
            WW_TA0006row("HDAINIGHTTIME") = ZeroToSpace(WW_TA0006row("HDAINIGHTTIME"))
            WW_TA0006row("HDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("HDAINIGHTTIMECHO"))
            WW_TA0006row("HDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("HDAINIGHTTIMETTL"))
            WW_TA0006row("SDAIWORKTIME") = ZeroToSpace(WW_TA0006row("SDAIWORKTIME"))
            WW_TA0006row("SDAIWORKTIMECHO") = ZeroToSpace(WW_TA0006row("SDAIWORKTIMECHO"))
            WW_TA0006row("SDAIWORKTIMETTL") = ZeroToSpace(WW_TA0006row("SDAIWORKTIMETTL"))
            WW_TA0006row("SDAINIGHTTIME") = ZeroToSpace(WW_TA0006row("SDAINIGHTTIME"))
            WW_TA0006row("SDAINIGHTTIMECHO") = ZeroToSpace(WW_TA0006row("SDAINIGHTTIMECHO"))
            WW_TA0006row("SDAINIGHTTIMETTL") = ZeroToSpace(WW_TA0006row("SDAINIGHTTIMETTL"))

            WW_TA0006tbl.Rows.Add(WW_TA0006row)
        Next

        '----------------------------------
        '合計行（勤怠側）の編集
        '----------------------------------
        '１行空白行を設定
        WW_TA0006row = WW_TA0006tbl.NewRow
        RowSpeceSet(WW_TA0006row)
        WW_TA0006tbl.Rows.Add(WW_TA0006row)

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
        Dim WW_HAYADETIME As Integer = 0
        Dim WW_HOANTIME As Integer = 0
        Dim WW_KOATUTIME As Integer = 0
        Dim WW_TOKSAAKAISU As Integer = 0
        Dim WW_TOKSABKAISU As Integer = 0
        Dim WW_TOKSACKAISU As Integer = 0
        Dim WW_JIKYUSHATIME As Integer = 0
        Dim WW_HDAIWORKTIME As Integer = 0
        Dim WW_HDAINIGHTTIME As Integer = 0
        Dim WW_SDAIWORKTIME As Integer = 0
        Dim WW_SDAINIGHTTIME As Integer = 0

        Dim WW_ORVERTIMEADD As Integer = 0
        Dim WW_WNIGHTTIMEADD As Integer = 0
        Dim WW_SWORKTIMEADD As Integer = 0
        Dim WW_SNIGHTTIMEADD As Integer = 0

        For Each TA0006Row As DataRow In WW_TA0006tbl.Rows
            WW_BINDTIME += HHMMToMinutes(TA0006Row("BINDTIME"))
            WW_ACTTIME += HHMMToMinutes(TA0006Row("ACTTIME"))
            WW_BREAKTIME2 += HHMMToMinutes(TA0006Row("BREAKTIMETTL"))
            WW_ORVERTIME += HHMMToMinutes(TA0006Row("ORVERTIMETTL"))
            WW_WNIGHTTIME += HHMMToMinutes(TA0006Row("WNIGHTTIMETTL"))
            WW_HWORKTIME += HHMMToMinutes(TA0006Row("HWORKTIMETTL"))
            WW_HNIGHTTIME += HHMMToMinutes(TA0006Row("HNIGHTTIMETTL"))
            WW_SWORKTIME += HHMMToMinutes(TA0006Row("SWORKTIMETTL"))
            WW_SNIGHTTIME += HHMMToMinutes(TA0006Row("SNIGHTTIMETTL"))
            WW_NIGHTTIME += HHMMToMinutes(TA0006Row("NIGHTTIMETTL"))
            WW_TOKUSA1TIME += HHMMToMinutes(TA0006Row("TOKUSA1TIMETTL"))
            WW_HAYADETIME += HHMMToMinutes(TA0006Row("HAYADETIMETTL"))
            WW_HOANTIME += HHMMToMinutes(TA0006Row("HOANTIMETTL"))
            WW_KOATUTIME += HHMMToMinutes(TA0006Row("KOATUTIMETTL"))
            WW_TOKSAAKAISU += Val(TA0006Row("TOKSAAKAISUTTL"))
            WW_TOKSABKAISU += Val(TA0006Row("TOKSABKAISUTTL"))
            WW_TOKSACKAISU += Val(TA0006Row("TOKSACKAISUTTL"))
            WW_JIKYUSHATIME += HHMMToMinutes(TA0006Row("JIKYUSHATIMETTL"))
            WW_HDAIWORKTIME += HHMMToMinutes(TA0006Row("HDAIWORKTIMETTL"))
            WW_HDAINIGHTTIME += HHMMToMinutes(TA0006Row("HDAINIGHTTIMETTL"))
            WW_SDAIWORKTIME += HHMMToMinutes(TA0006Row("SDAIWORKTIMETTL"))
            WW_SDAINIGHTTIME += HHMMToMinutes(TA0006Row("SDAINIGHTTIMETTL"))

            WW_ORVERTIMEADD += HHMMToMinutes(TA0006Row("ORVERTIMEADD"))
            WW_WNIGHTTIMEADD += HHMMToMinutes(TA0006Row("WNIGHTTIMEADD"))
            WW_SWORKTIMEADD += HHMMToMinutes(TA0006Row("SWORKTIMEADD"))
            WW_SNIGHTTIMEADD += HHMMToMinutes(TA0006Row("SNIGHTTIMEADD"))
        Next

        '合計の場合
        WW_TA0006row = WW_TA0006tbl.NewRow
        RowSpeceSet(WW_TA0006row)
        WW_TA0006row("PAYKBNNAMES") = "合計"
        WW_TA0006row("PAYKBN_TXT") = "合計"

        WW_TA0006row("BINDTIME") = ZeroToSpace(MinutesToHHMM(WW_BINDTIME))
        WW_TA0006row("ACTTIME") = ZeroToSpace(MinutesToHHMM(WW_ACTTIME))
        WW_TA0006row("BREAKTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_BREAKTIME2))
        WW_TA0006row("ORVERTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_ORVERTIME))
        WW_TA0006row("WNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_WNIGHTTIME))
        WW_TA0006row("HWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HWORKTIME))
        WW_TA0006row("HNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HNIGHTTIME))
        WW_TA0006row("SWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_SWORKTIME))
        WW_TA0006row("SNIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_SNIGHTTIME))
        WW_TA0006row("NIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_NIGHTTIME))
        WW_TA0006row("TOKUSA1TIMETTL") = ZeroToSpace(MinutesToHHMM(WW_TOKUSA1TIME))
        WW_TA0006row("HAYADETIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HAYADETIME))
        WW_TA0006row("HOANTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HOANTIME))
        WW_TA0006row("KOATUTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_KOATUTIME))
        WW_TA0006row("TOKSAAKAISUTTL") = ZeroToSpace(WW_TOKSAAKAISU)
        WW_TA0006row("TOKSABKAISUTTL") = ZeroToSpace(WW_TOKSABKAISU)
        WW_TA0006row("TOKSACKAISUTTL") = ZeroToSpace(WW_TOKSACKAISU)
        WW_TA0006row("JIKYUSHATIMETTL") = ZeroToSpace(MinutesToHHMM(WW_JIKYUSHATIME))
        WW_TA0006row("HDAIWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HDAIWORKTIME))
        WW_TA0006row("HDAINIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_HDAINIGHTTIME))
        WW_TA0006row("SDAIWORKTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_SDAIWORKTIME))
        WW_TA0006row("SDAINIGHTTIMETTL") = ZeroToSpace(MinutesToHHMM(WW_SDAINIGHTTIME))

        WW_TA0006row("ORVERTIMEADD") = ZeroToSpace(MinutesToHHMM(WW_ORVERTIMEADD))
        WW_TA0006row("WNIGHTTIMEADD") = ZeroToSpace(MinutesToHHMM(WW_WNIGHTTIMEADD))
        WW_TA0006row("SWORKTIMEADD") = ZeroToSpace(MinutesToHHMM(WW_SWORKTIMEADD))
        WW_TA0006row("SNIGHTTIMEADD") = ZeroToSpace(MinutesToHHMM(WW_SNIGHTTIMEADD))

        WW_TA0006tbl.Rows.Add(WW_TA0006row)

        IO_TBL = WW_TA0006tbl.Copy

        WW_TA0006tbl.Dispose()
        WW_TA0006tbl = Nothing

    End Sub

    'スペース行作成処理
    Protected Sub RowSpeceSet(ByRef ioRow As DataRow)

        For Each col As DataColumn In ioRow.Table.Columns
            If col.DataType.Name.ToString = "String" Then
                ioRow(col.ColumnName) = ""
            End If
        Next

    End Sub

    ''' <summary>
    ''' セレクター設定
    ''' </summary>
    Protected Sub InitalSelector()

        Dim WW_TBLview As DataView
        Dim WW_GRPtbl As DataTable

        If IsNothing(SELECTORtbl) Then SELECTORtbl = New DataTable
        'テンポラリDB項目作成
        SELECTORtbl.Clear()
        SELECTORtbl.Columns.Add("CODE", GetType(String))                        'CODE               コード
        SELECTORtbl.Columns.Add("NAME", GetType(String))                        'NAME               名称


        Dim WW_Cols As String() = {"STAFFCODE", "STAFFCODE_TXT"}
        WW_TBLview = New DataView(TA0006ALL)
        WW_TBLview.Sort = "STAFFCODE"
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        For Each TA0006ALLrow As DataRow In WW_GRPtbl.Rows
            Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0006ALLrow("STAFFCODE")
            SELECTORrow("NAME") = TA0006ALLrow("STAFFCODE_TXT")
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.SORTING = "CODE, NAME"
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
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text = SELECTORtbl.Rows(i)("CODE")
            'テキスト
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Text = "　" & SELECTORtbl.Rows(i)("NAME")

            '背景色
            If CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text = WF_SELECTOR_Posi.Value Then
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Style.Value = "height:1.5em;width:13.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Style.Value = "height:1.5em;width:13.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If

            'イベント追加
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Attributes.Remove("onclick")
            CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Attributes.Add("onclick", "SELECTOR_Click('" & SELECTORtbl.Rows(i)("CODE") & "');")
        Next

        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_GRPtbl.Dispose()
        WW_GRPtbl = Nothing

    End Sub

    ' *** セレクタークリック(選択変更)処理
    Protected Sub SELECTOR_Click()

        '■ データリカバリ
        '○ TA0006ALLデータリカバリ
        If IsNothing(TA0006ALL) Then
            If Not Master.RecoverTable(TA0006ALL) Then Exit Sub
        End If

        '■ セレクター表示切替
        For i As Integer = 0 To WF_SELECTOR.Items.Count - 1
            '背景色
            If CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_VALUE"), Label).Text = WF_SELECTOR_Posi.Value Then
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Style.Value = "height:1.5em;width:13.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_SELECTOR.Items(i).FindControl("WF_SELECTOR_TEXT"), Label).Style.Value = "height:1.5em;width:13.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If
        Next

        '■ GridView表示データ作成

        '○TA0006VIEWtbl取得
        GetViewTA0006Tbl(WF_SELECTOR_Posi.Value)

    End Sub


    ''' <summary>
    '''  TA0006ALL（Grid用）カラム設定
    ''' </summary>
    ''' <param name="IO_TBL">列追加対象テーブル</param>
    Protected Sub AddColumnToTA0006Tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then
            IO_TBL.Columns.Clear()
        End If
        'T0007DB項目作成
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
        IO_TBL.Columns.Add("ORVERTIMEADD", GetType(String))
        IO_TBL.Columns.Add("ORVERTIMETTL", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMEADD", GetType(String))
        IO_TBL.Columns.Add("WNIGHTTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SWORKTIME", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMEADD", GetType(String))
        IO_TBL.Columns.Add("SWORKTIMETTL", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIME", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMECHO", GetType(String))
        IO_TBL.Columns.Add("SNIGHTTIMEADD", GetType(String))
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
        IO_TBL.Columns.Add("YENDTIME", GetType(String))
        IO_TBL.Columns.Add("RIYU", GetType(String))
        IO_TBL.Columns.Add("RIYUNAME", GetType(String))
        IO_TBL.Columns.Add("RIYUETC", GetType(String))

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
    End Sub

    ''' <summary>
    ''' T00007ALLカラム設定
    ''' </summary>
    ''' <param name="IO_TBL">列登録対象テーブル</param>
    Public Sub AddColumnToT0007tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

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
        IO_TBL.Columns.Add("DELFLG", GetType(String))

        IO_TBL.Columns.Add("DATAKBN", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("SHIPORGNAMES", GetType(String))
        IO_TBL.Columns.Add("NIPPONO", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("RUIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("JIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KUDISTANCE", GetType(String))

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
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "WORKKBN"))
                Case "DELFLG"
                    '削除フラグ名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "STAFFCODE"
                    '乗務員名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.GetStaffCodeAllList(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_HORG.Text, work.WF_SEL_TAISHOYM.Text))
                Case "CAMPCODE"
                    '会社名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "ORG"
                    '出荷部署名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.INVALID))
                Case "CREWKBN"
                    '実績登録区分名
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CREWKBN"))
                Case "SHUKCHOKKBN"
                    '宿日直区分
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "T0009_SHUKCHOKKBN"))
                Case "SHUKCHOKKBN_HIGASHIKO"
                    '宿日直区分
                    leftview.CodeToName(I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "T0009_SHUKCHOKKBN"))

            End Select
        End If

    End Sub

    ''' <summary>
    ''' 時間変換（分→時:分）
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Function MinutesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        WW_HHMM = Int(I_PARAM / 60) * 100 + I_PARAM Mod 60
        MinutesToHHMM = Format(WW_HHMM, "0#:##")
    End Function

    ''' <summary>
    ''' 変換（0 or 00:00をスペースへ）帳票出力用
    ''' </summary>
    ''' <param name="I_PARAM"></param>
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

    ''' <summary>
    ''' 変換（時：分→分）
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    Private Function HHMMToMinutes(ByVal I_PARAM As String) As Integer
        Dim WW_TIME As String() = I_PARAM.Split(":")
        If I_PARAM = Nothing Then
            HHMMToMinutes = 0
        Else
            HHMMToMinutes = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
        End If

    End Function

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()
        If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0006WRKINC.MAPID
        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0006S Then                                                    '条件画面からの画面遷移
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

    End Sub

End Class






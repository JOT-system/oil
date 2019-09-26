Imports System.IO.Compression
Imports System.Data.SqlClient

Public Class GRTA0001HAISHA
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
    ''' GridView用テーブルソート文字列取得
    ''' </summary>
    Private CS0026TBLSORTget As New CS0026TBLSORT                   'GridView用テーブルソート文字列取得
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' 帳票マージ出力
    ''' </summary>
    Private CS0047XLSMERGE As New CS0047XLSMERGE                    '帳票マージ出力
    ''' <summary>
    ''' セッション情報
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
    ''' <summary>
    ''' 明細項目設定用
    ''' </summary>
    Private CS0052DetailView As New CS0052DetailView                '明細項目設定用

    '検索結果格納
    Private TA0001ALL As DataTable                                  '全データテーブル
    Private TA0001VIEWtbl As DataTable                              'Grid格納用テーブル
    Private TA0001DETAILtbl As DataTable                            'Detail入力用テーブル
    Private SELECTORtbl As DataTable                                'TREE選択作成作業テーブル

    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERR_SW As String                                     '
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
    Private Const CONST_DSPROWCOUNT As Integer = 45                 '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 10              'マウススクロール時の増分
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

        '■ 初期URLがhttp://xxxx/Driversの場合、全ZIP取得ボタンを非活性 現在は未使用
        If HttpContext.Current.Session("DRIVERS") = Nothing Then
            WF_MAPpermitcode.Value = "TRUE"
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If

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
                    Case "WF_MEMOChange"                '■ メモ欄保存処理
                        WF_RIGHTBOX_Change()
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

        If Not IsNothing(TA0001ALL) Then
            TA0001ALL.Dispose()
            TA0001ALL = Nothing
        End If
        If Not IsNothing(TA0001DETAILtbl) Then
            TA0001DETAILtbl.Dispose()
            TA0001DETAILtbl = Nothing
        End If
        If Not IsNothing(TA0001VIEWtbl) Then
            TA0001VIEWtbl.Dispose()
            TA0001VIEWtbl = Nothing
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
        rightview.Initialize(WW_DUMMY)

        '■ 全データ取得
        '○TA0001ALL取得
        GetALLTA0001()

        '○表示選択TREE表示
        InitialSelector()

        '○画面（GridView）表示データ保存
        If Not Master.SaveTable(TA0001ALL) Then Exit Sub
        '■ GridView表示データ作成
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)

        '■ 画面（GridView）表示
        WF_SEL_DATE.Text = work.WF_SEL_SHUKODATEF.Text
        WF_SEL_ORG.Text = work.WF_SEL_SHIPORGNAME.Text

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0001VIEWtbl)
            TBLview.Sort = "LINECNT"
            TBLview.RowFilter = "HIDDEN = 0 and SELECT >= 1 and SELECT < " & (CONST_DSPROWCOUNT).ToString
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0001WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        work.WF_IsHideDetailBox.Text = "1"
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        If IsNothing(TA0001VIEWtbl) Then
            If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
            '■ GridView表示データ作成
            '〇 TA0001VIEWtblカラム設定
            AddColumnsToTA0001Tbl(TA0001VIEWtbl)
            '○TA0001VIEWtbl取得
            GetViewTA0001(WF_SELECTOR_Posi.Value)
        End If

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0001VIEWtbl.Rows.Count - 1
            If TA0001VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0001VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(TA0001VIEWtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRTA0001WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If

    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************

    ''' <summary>
    ''' PDF印刷ボタン押下時処理
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
            WW_TEMPDir = CS0050Session.UPLOAD_PATH & "\TEXTWORK\TEMP" & "\" & work.WF_SEL_SHIPORG.Text
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
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
        '■ 全選択Excel作成（メイン処理）
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For Each item As RepeaterItem In WF_SELECTOR.Items
            '○TA0001VIEWtbl取得
            TA0001VIEWtbl.Clear()
            GetViewTA0001(CType(item.FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text)

            '○ 帳票出力
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = GRTA0001WRKINC.MAPID               '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0001VIEWtbl                    'データ参照DataTable
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.CS0030REPORT()

            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(item.FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text & ".xlsx", True)
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

        '■ 画面表示
        '○TA0001VIEWtbl取得
        TA0001VIEWtbl.Clear()
        GetViewTA0001(WF_SELECTOR_Posi.Value)

        '○ 正常終了メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
    End Sub
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ テーブルデータ 復元
        '○全表示データ 復元
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub

        '■ 帳票出力
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)
        '○帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0001WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0001VIEWtbl                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "f_ExcelPrint", "f_ExcelPrint();", True)
    End Sub
    ''' <summary>
    ''' 圧縮＆ダウンロード処理
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

            '○ ファイル格納フォルダ存在確認＆作成(C:\apple\files\TEXTWORK\端末名)　＆　前回処理ファイル削除
            WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
                'ファイル格納フォルダ内不要ファイル削除(すべて削除)
                For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                    System.IO.File.Delete(tempFile)
                Next
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '○ TEMPフォルダ存在確認＆作成(C:\apple\files\TEXTWORK\TEMP\部署)　＆　前回処理ファイル削除
            WW_TEMPDir = CS0050Session.UPLOAD_PATH & "\TEXTWORK\TEMP\" & work.WF_SEL_SHIPORG.Text
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
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
        '■ 全選択Excel作成（メイン処理）
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        For Each item As RepeaterItem In WF_SELECTOR.Items
            '○TA0001VIEWtbl取得
            TA0001VIEWtbl.Clear()
            GetViewTA0001(CType(item.FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text)
            '○ 帳票出力
            CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
            CS0030REPORT.MAPID = GRTA0001WRKINC.MAPID               '画面ID
            CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
            CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
            CS0030REPORT.TBLDATA = TA0001VIEWtbl                    'データ参照DataTable
            CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
            CS0030REPORT.CS0030REPORT()

            If isNormal(CS0030REPORT.ERR) Then
                'ダウンロードファイル送信準備
                System.IO.File.Copy(CS0030REPORT.FILEpath, WW_Dir & "\" & CType(item.FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text & ".xlsx", True)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
                Exit Sub
            End If
        Next

        '■ ダウンロード
        '○ 圧縮実行
        WW_Dir = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK" & "\" & Master.USERID
        Dim WW_Dir2 As String = CS0050Session.UPLOAD_PATH & "\" & "TEXTWORK\TEMP\" & work.WF_SEL_SHIPORG.Text
        ZipFile.CreateFromDirectory(WW_Dir, WW_Dir2 & "\ALL.zip")

        WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/TEXT/TEMP/" & work.WF_SEL_SHIPORG.Text & "/ALL.zip"
        '○ ダウンロード処理へ遷移
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
        '■ 画面表示
        '○TA0001VIEWtbl取得
        TA0001VIEWtbl.Clear()
        GetViewTA0001(WF_SELECTOR_Posi.Value)

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
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()
        '■ データリカバリ
        '○ T00009ALLデータリカバリ
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)
        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)

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
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)
        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)
        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(TA0001VIEWtbl)
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_BACK_Click()
        '■ GridView表示データ作成
        '○データリカバリ 
        If IsNothing(TA0001ALL) Then If Not Master.RecoverTable(TA0001ALL) Then Exit Sub
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)
        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)
        '○detailboxヘッダークリア
        '出庫日
        WF_SHUKODATE.Text = String.Empty
        '出荷日
        WF_SHUKADATE.Text = String.Empty
        '届日
        WF_TODOKEDATE.Text = String.Empty
        '帰庫日
        WF_KIKODATE.Text = String.Empty
        '両目
        WF_RYOME.Text = String.Empty

        WF_Sel_LINECNT.Text = String.Empty
        WF_SHIPORG.Text = String.Empty
        WF_TORICODE.Text = String.Empty
        WF_OILTYPE.Text = String.Empty
        WF_STORICODE.Text = String.Empty
        WF_ORDERORG.Text = String.Empty
        WF_URIKBN.Text = String.Empty

        '業務車番
        WF_GSHABAN.Text = String.Empty
        WF_TSHABANF.Text = String.Empty
        WF_TSHABANB.Text = String.Empty
        WF_TSHABANB2.Text = String.Empty
        'コンテナシャーシ
        WF_CONTCHASSIS.Text = String.Empty
        '車腹
        WF_SHAFUKU.Text = String.Empty

        '積置区分
        WF_TUMIOKIKBN.Text = String.Empty
        'トリップ
        WF_TRIPNO.Text = String.Empty
        'ドロップ
        WF_DROPNO.Text = String.Empty

        '乗務員
        WF_STAFFCODE.Text = String.Empty
        '副乗務員
        WF_SUBSTAFFCODE.Text = String.Empty
        '出勤時間
        WF_STTIME.Text = String.Empty
        '〇
        work.WF_IsHideDetailBox.Text = "1"
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
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' TA0001All全表示データ取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetALLTA0001()

        Dim WW_ORDERNO As String = String.Empty
        Dim WW_ORDERORG As String = String.Empty
        Dim WW_SHIPORG As String = String.Empty
        Dim WW_SHUKODATE As String = String.Empty
        Dim WW_GSHABAN As String = String.Empty
        Dim WW_RYOME As String = String.Empty
        Dim WW_TRIPNO As String = String.Empty
        Dim WW_DROPNO As String = String.Empty

        Dim WW_DATE As Date
        Dim WW_TIME As DateTime
        Dim WW_INT As Integer


        'TA0001テンポラリDB項目作成
        AddColumnsToTA0001Tbl(TA0001ALL)

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文     ※乗務員照会も可能にするため、ユーザ権限チェックは行わない。
                Dim SQLStr As String =
                     "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,                  " _
                   & "       isnull(rtrim(A.TERMORG),'')           as TERMORG ,                   " _
                   & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,                   " _
                   & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,                  " _
                   & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,                    " _
                   & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,                    " _
                   & "       isnull(rtrim(A.SEQ),'00')             as SEQ ,                       " _
                   & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,                  " _
                   & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,                   " _
                   & "       isnull(rtrim(A.STORICODE),'')         as STORICODE ,                 " _
                   & "       isnull(rtrim(A.ORDERORG),'')          as ORDERORG ,                  " _
                   & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,                 " _
                   & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,                  " _
                   & "       isnull(rtrim(A.KIJUNDATE),'')         as KIJUNDATE ,                 " _
                   & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,                 " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'')        as TUMIOKIKBN ,                " _
                   & "       isnull(rtrim(A.URIKBN),'')            as URIKBN ,                    " _
                   & "       isnull(rtrim(A.STATUS),'')            as STATUS ,                    " _
                   & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,                   " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,                " _
                   & "       isnull(rtrim(A.INTIME),'')            as INTIME ,                    " _
                   & "       isnull(rtrim(A.OUTTIME),'')           as OUTTIME ,                   " _
                   & "       isnull(rtrim(A.SHUKADENNO),'')        as SHUKADENNO ,                " _
                   & "       isnull(rtrim(A.TUMISEQ),'')           as TUMISEQ ,                   " _
                   & "       isnull(rtrim(A.TUMIBA),'')            as TUMIBA ,                    " _
                   & "       isnull(rtrim(A.GATE),'')              as GATE ,                      " _
                   & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,                   " _
                   & "       isnull(rtrim(A.RYOME),'')             as RYOME ,                     " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'')       as CONTCHASSIS ,               " _
                   & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,                   " _
                   & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,                 " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,              " _
                   & "       isnull(rtrim(A.STTIME),'')            as STTIME ,                    " _
                   & "       isnull(rtrim(A.TORIORDERNO),'')       as TORIORDERNO ,               " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,                " _
                   & "       isnull(rtrim(A.TODOKETIME),'')        as TODOKETIME ,                " _
                   & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,                " _
                   & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,                  " _
                   & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,                  " _
                   & "       isnull(rtrim(A.PRATIO),'')            as PRATIO ,                    " _
                   & "       isnull(rtrim(A.SMELLKBN),'')          as SMELLKBN ,                  " _
                   & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,                    " _
                   & "       isnull(rtrim(A.HTANI),'')             as HTANI ,                     " _
                   & "       isnull(rtrim(A.SURYO),'')             as SURYO ,                     " _
                   & "       isnull(rtrim(A.DAISU),'')             as DAISU ,                     " _
                   & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,                    " _
                   & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,                    " _
                   & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,                  " _
                   & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,                  " _
                   & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,                  " _
                   & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,                  " _
                   & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,                  " _
                   & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,                  " _
                   & "       isnull(rtrim(A.TAXKBN),'')            as TAXKBN ,                    " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,               " _
                   & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,                  " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,               " _
                   & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,                  " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,              " _
                   & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,                 " _
                   & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,                    " _
                   & "       isnull(rtrim(MA6.SHARYOINFO1),'')     as SHARYOINFO1 ,               " _
                   & "       isnull(rtrim(MA6.SHARYOINFO2),'')     as SHARYOINFO2 ,               " _
                   & "       isnull(rtrim(MA6.SHARYOINFO3),'')     as SHARYOINFO3 ,               " _
                   & "       isnull(rtrim(MA6.SHARYOINFO4),'')     as SHARYOINFO4 ,               " _
                   & "       isnull(rtrim(MA6.SHARYOINFO5),'')     as SHARYOINFO5 ,               " _
                   & "       isnull(rtrim(MA6.SHARYOINFO6),'')     as SHARYOINFO6 ,               " _
                   & "       isnull(rtrim(MC71.ARRIVTIME),'')      as ARRIVTIME ,                 " _
                   & "       isnull(rtrim(MC71.DISTANCE),'')       as DISTANCE ,                  " _
                   & "       isnull(rtrim(M01.NAMES),'')           as CAMPCODENAME ,              " _
                   & "       isnull(rtrim(M021.NAMES),'')          as ORDERORGNAME ,              " _
                   & "       isnull(rtrim(M022.NAMES),'')          as SHIPORGNAME ,               " _
                   & "       isnull(rtrim(M023.NAMES),'')          as TERMORGNAME ,               " _
                   & "       isnull(rtrim(MC21.NAMES),'')          as TORICODENAME ,              " _
                   & "       isnull(rtrim(MC22.NAMES),'')          as STORICODENAME ,             " _
                   & "       isnull(rtrim(MD1.NAMES),'')           as PRODUCT2NAME ,              " _
                   & "       isnull(rtrim(MC61.NAMES),'')          as TODOKECODENAME ,            " _
                   & "       isnull(rtrim(MC61.POSTNUM1),'')       as POSTNUM1 ,                  " _
                   & "       isnull(rtrim(MC61.POSTNUM2),'')       as POSTNUM2 ,                  " _
                   & "       isnull(rtrim(MC61.ADDR1),'') +                                       " _
                   & "       isnull(rtrim(MC61.ADDR2),'') +                                       " _
                   & "       isnull(rtrim(MC61.ADDR3),'') +                                       " _
                   & "       isnull(rtrim(MC61.ADDR4),'')          as ADDR ,                      " _
                   & "       isnull(rtrim(MC61.ADDR1),'')          as ADDR1 ,                     " _
                   & "       isnull(rtrim(MC61.ADDR2),'')          as ADDR2 ,                     " _
                   & "       isnull(rtrim(MC61.ADDR3),'')          as ADDR3 ,                     " _
                   & "       isnull(rtrim(MC61.ADDR4),'')          as ADDR4 ,                     " _
                   & "       isnull(rtrim(MC61.NOTES1),'')         as NOTES1 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES2),'')         as NOTES2 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES3),'')         as NOTES3 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES4),'')         as NOTES4 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES5),'')         as NOTES5 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES6),'')         as NOTES6 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES7),'')         as NOTES7 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES8),'')         as NOTES8 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES9),'')         as NOTES9 ,                    " _
                   & "       isnull(rtrim(MC61.NOTES10),'')        as NOTES10 ,                   " _
                   & "       isnull(rtrim(MC62.NAMES),'')          as SHUKABASHONAME ,            " _
                   & "       isnull(rtrim(MB11.STAFFNAMES),'')     as STAFFCODENAME ,             " _
                   & "       isnull(rtrim(MB11.NOTES1),'')         as STAFFNOTES1 ,               " _
                   & "       isnull(rtrim(MB11.NOTES2),'')         as STAFFNOTES2 ,               " _
                   & "       isnull(rtrim(MB11.NOTES3),'')         as STAFFNOTES3 ,               " _
                   & "       isnull(rtrim(MB11.NOTES4),'')         as STAFFNOTES4 ,               " _
                   & "       isnull(rtrim(MB11.NOTES5),'')         as STAFFNOTES5 ,               " _
                   & "       isnull(rtrim(MB12.STAFFNAMES),'')     as SUBSTAFFCODENAME ,          " _
                   & "                                                                            " _
                   & "       ''                                    as GSHABANLICNPLTNO ,          " _
                   & "       ''                                    as CONTCHASSISLICNPLTNO ,      " _
                   & "       ''                                    as TUMIOKIKBNNAME ,            " _
                   & "       ''                                    as HTANINAME ,                 " _
                   & "       ''                                    as TAXKBNNAME ,                " _
                   & "       ''                                    as TUMIOKI ,                   " _
                   & "       ''                                    as OILTYPENAME ,               " _
                   & "       ''                                    as URIKBNNAME ,                " _
                   & "       ''                                    as STATUSNAME ,                " _
                   & "       ''                                    as SMELLKBNNAME ,              " _
                   & "       ''                                    as PRODUCT1NAME ,              " _
                   & "                                                                            " _
                   & "       isnull(rtrim(MB21.SEQ),'9999')        as STAFFORGSEQ ,               " _
                   & "       isnull(rtrim(MB22.SEQ),'')            as SUBSTAFFORGSEQ ,            " _
                   & "       isnull(rtrim(MA6.SEQ),'9999')         as SHABANORGSEQ ,              " _
                   & "       isnull(rtrim(MC72.SEQ),'9999')        as TODKORGSEQ ,                " _
                   & "       '0'                                   as LINECNT ,                   " _
                   & "       '0'                                   as HIDDEN ,                    " _
                   & "       '0'                                   as SURYO_SUM ,                 " _
                   & "       '0'                                   as DAISU_SUM                   " _
                   & " FROM        T0004_HORDER AS A                                              " _
                   & " INNER JOIN ( SELECT X2.CAMPCODE , Y2.CODE                                  " _
                   & "               FROM S0012_SRVAUTHOR X2                                      " _
                   & "               INNER JOIN S0006_ROLE Y2                                     " _
                   & "                  ON Y2.CAMPCODE                = X2.CAMPCODE               " _
                   & "                 and Y2.OBJECT                  = 'SRVORG'                  " _
                   & "                 and Y2.ROLE                    = X2.ROLE                   " _
                   & "                 and Y2.STYMD                  <= @P04                      " _
                   & "                 and Y2.ENDYMD                 >= @P04                      " _
                   & "                 and (Y2.PERMITCODE           = '1' or Y2.PERMITCODE = '2') " _
                   & "                 and Y2.DELFLG                 <> '1'                       " _
                   & "               WHERE X2.TERMID                    = @P03                    " _
                   & "                 and X2.OBJECT                    = 'SRVORG'                " _
                   & "                 and X2.STYMD                    <= @P04                    " _
                   & "                 and X2.ENDYMD                   >= @P04                    " _
                   & "                 and X2.DELFLG                   <> '1'                     " _
                   & "               GROUP BY X2.CAMPCODE ,Y2.CODE                                " _
                   & "            ) AS Z2                                                         " _
                   & "    ON Z2.CAMPCODE                         = A.CAMPCODE                     " _
                   & "   and (Z2.CODE                            = A.SHIPORG)                     " _
                   & " INNER JOIN M0001_CAMP M01                                                  " _
                   & "    ON M01.CAMPCODE                        = A.CAMPCODE                     " _
                   & "   and M01.STYMD                          <= A.SHUKODATE                    " _
                   & "   and M01.ENDYMD                         >= A.SHUKODATE                    " _
                   & "   and M01.DELFLG                         <> '1'                            " _
                   & " LEFT JOIN M0002_ORG M021                                                   " _
                   & "    ON M021.CAMPCODE                       = A.CAMPCODE                     " _
                   & "   and M021.ORGCODE                        = A.ORDERORG                     " _
                   & "   and M021.STYMD                         <= A.SHUKODATE                    " _
                   & "   and M021.ENDYMD                        >= A.SHUKODATE                    " _
                   & "   and M021.DELFLG                        <> '1'                            " _
                   & " LEFT JOIN M0002_ORG M022                                                   " _
                   & "    ON M022.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and M022.ORGCODE                      = A.SHIPORG                        " _
                   & "   and M022.STYMD                       <= A.SHUKODATE                      " _
                   & "   and M022.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and M022.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN M0002_ORG M023                                                   " _
                   & "    ON M023.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and M023.ORGCODE                      = A.TERMORG                        " _
                   & "   and M023.STYMD                       <= A.SHUKODATE                      " _
                   & "   and M023.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and M023.DELFLG                      <> '1'                              " _
                   & "  LEFT JOIN MB001_STAFF MB11                                                " _
                   & "    ON MB11.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MB11.STAFFCODE                      = A.STAFFCODE                    " _
                   & "   and MB11.STYMD                       <= A.SHUKODATE                      " _
                   & "   and MB11.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and MB11.DELFLG                      <> '1'                              " _
                   & "  LEFT JOIN MB001_STAFF MB12                                                " _
                   & "    ON MB12.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MB12.STAFFCODE                      = A.SUBSTAFFCODE                 " _
                   & "   and MB12.STYMD                       <= A.SHUKODATE                      " _
                   & "   and MB12.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and MB12.DELFLG                      <> '1'                              " _
                   & " INNER JOIN MC002_TORIHIKISAKI as MC21                                      " _
                   & "    ON MC21.CAMPCODE                     = A.CAMPCODE                       " _
                   & "   and MC21.TORICODE                     = A.TORICODE                       " _
                   & "   and MC21.STYMD                       <= A.SHUKODATE                      " _
                   & "   and MC21.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and MC21.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN MC002_TORIHIKISAKI as MC22                                       " _
                   & "    ON MC22.CAMPCODE                      = A.CAMPCODE                      " _
                   & "   and MC22.TORICODE                      = A.STORICODE                     " _
                   & "   and MC22.STYMD                        <= A.SHUKODATE                     " _
                   & "   and MC22.ENDYMD                       >= A.SHUKODATE                     " _
                   & "   and MC22.DELFLG                       <> '1'                             " _
                   & " LEFT JOIN MD001_PRODUCT as MD1                                             " _
                   & "    ON MD1.CAMPCODE                      = A.CAMPCODE                       " _
                   & "   and MD1.PRODUCTCODE                   = A.PRODUCTCODE                    " _
                   & "   and MD1.STYMD                        <= A.SHUKODATE                      " _
                   & "   and MD1.ENDYMD                       >= A.SHUKODATE                      " _
                   & "   and MD1.DELFLG                       <> '1'                              " _
                   & " LEFT JOIN MC006_TODOKESAKI MC61                                            " _
                   & "    ON MC61.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MC61.TODOKECODE                        = A.TODOKECODE                " _
                   & "   and MC61.STYMD                       <= A.SHUKODATE                      " _
                   & "   and MC61.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and MC61.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN MC006_TODOKESAKI MC62                                            " _
                   & "    ON MC62.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MC62.TODOKECODE                        = A.SHUKABASHO                " _
                   & "   and MC62.STYMD                       <= A.SHUKODATE                      " _
                   & "   and MC62.ENDYMD                      >= A.SHUKODATE                      " _
                   & "   and MC62.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN MA006_SHABANORG MA6                                              " _
                   & "    ON MA6.CAMPCODE                          = A.CAMPCODE                   " _
                   & "   and MA6.GSHABAN                           = A.GSHABAN                    " _
                   & "   and MA6.MANGUORG                          = A.SHIPORG                    " _
                   & "   and MA6.DELFLG                       <> '1'                              " _
                   & " LEFT JOIN MB002_STAFFORG MB21                                              " _
                   & "    ON MB21.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MB21.STAFFCODE                      = A.STAFFCODE                    " _
                   & "   and MB21.SORG                             = A.SHIPORG                    " _
                   & "   and MB21.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN MB002_STAFFORG MB22                                              " _
                   & "    ON MB22.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MB22.STAFFCODE                      = A.SUBSTAFFCODE                 " _
                   & "   and MB22.SORG                             = A.SHIPORG                    " _
                   & "   and MB22.DELFLG                      <> '1'                              " _
                   & " LEFT JOIN MC007_TODKORG MC71                                               " _
                   & "    ON MC71.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MC71.TORICODE                          = A.TORICODE                  " _
                   & "   and MC71.TODOKECODE                        = A.TODOKECODE                " _
                   & "   and MC71.UORG                              = A.SHIPORG                   " _
                   & "   and MC71.DELFLG                       <> '1'                             " _
                   & " LEFT JOIN MC007_TODKORG MC72                                               " _
                   & "    ON MC72.CAMPCODE                          = A.CAMPCODE                  " _
                   & "   and MC72.TORICODE                          = A.TORICODE                  " _
                   & "   and MC72.TODOKECODE                        = A.TODOKECODE                " _
                   & "   and MC72.UORG                              = A.SHIPORG                   " _
                   & "   and MC72.DELFLG                       <> '1'                             " _
                   & " WHERE A.CAMPCODE                        = @P02                             " _
                   & "   and A.SHUKODATE                       = @P05                             " _
                   & "   and (A.STATUS = '2'                  or A.STATUS = '3')                  " _
                   & "   and A.DELFLG                         <> '1'                              "


                '■テーブル検索条件追加
                '条件画面で指定された出荷部署を抽出
                If work.WF_SEL_SHIPORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.SHIPORG          = @P06           		    "
                End If

                SQLStr = SQLStr & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.SHUKADATE ,          " _
                                & " 		 A.ORDERORG  ,A.SHIPORG ,	                    " _
                                & " 		 A.SHUKODATE ,A.TODOKEDATE ,A.GSHABAN ,         " _
                                & " 		 A.RYOME     ,A.TRIPNO  ,A.DROPNO	 ,A.SEQ     "


                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)           '本日
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)           '出庫日(To)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)   '出荷部署
                    PARA02.Value = work.WF_SEL_CAMPCODE.Text
                    PARA03.Value = CS0050Session.APSV_ID
                    PARA04.Value = Date.Now

                    '出庫日
                    If work.WF_SEL_SHUKODATEF.Text = Nothing Then
                        PARA05.Value = C_MAX_YMD
                    Else
                        PARA05.Value = work.WF_SEL_SHUKODATEF.Text
                    End If

                    PARA06.Value = work.WF_SEL_SHIPORG.Text

                    '■SQL実行
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル格納
                        TA0001ALL.Load(SQLdr)

                    End Using
                    For Each TA0001ALLrow As DataRow In TA0001ALL.Rows
                        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
                        '○レコードの初期設定
                        TA0001ALLrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        TA0001ALLrow("TIMSTP") = 0
                        TA0001ALLrow("SELECT") = 1   '1:表示
                        TA0001ALLrow("SEQ") = "00"

                        Try
                            Date.TryParse(TA0001ALLrow("SHUKODATE"), WW_DATE)
                            TA0001ALLrow("SHUKODATE") = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            TA0001ALLrow("SHUKODATE") = ""
                        End Try

                        Try
                            Date.TryParse(TA0001ALLrow("KIKODATE"), WW_DATE)
                            TA0001ALLrow("KIKODATE") = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            TA0001ALLrow("KIKODATE") = ""
                        End Try

                        Try
                            Date.TryParse(TA0001ALLrow("KIJUNDATE"), WW_DATE)
                            TA0001ALLrow("KIJUNDATE") = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            TA0001ALLrow("KIJUNDATE") = ""
                        End Try

                        Try
                            Date.TryParse(TA0001ALLrow("SHUKADATE"), WW_DATE)
                            TA0001ALLrow("SHUKADATE") = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            TA0001ALLrow("SHUKADATE") = ""
                        End Try

                        Try
                            Date.TryParse(TA0001ALLrow("TODOKEDATE"), WW_DATE)
                            TA0001ALLrow("TODOKEDATE") = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            TA0001ALLrow("TODOKEDATE") = ""
                        End Try

                        Try
                            Date.TryParse(TA0001ALLrow("ARRIVTIME"), WW_TIME)
                            TA0001ALLrow("ARRIVTIME") = WW_TIME.ToString("H:mm")
                        Catch ex As Exception
                            TA0001ALLrow("ARRIVTIME") = ""
                        End Try

                        Try
                            Integer.TryParse(TA0001ALLrow("STAFFORGSEQ"), WW_INT)
                            TA0001ALLrow("STAFFORGSEQ") = WW_INT.ToString("0000")
                        Catch ex As Exception
                            TA0001ALLrow("STAFFORGSEQ") = "9999"
                        End Try

                        Try
                            If TA0001ALLrow("SUBSTAFFORGSEQ") <> "" Then
                                Integer.TryParse(TA0001ALLrow("SUBSTAFFORGSEQ"), WW_INT)
                                TA0001ALLrow("SUBSTAFFORGSEQ") = WW_INT.ToString("0000")
                            End If
                        Catch ex As Exception
                            TA0001ALLrow("SUBSTAFFORGSEQ") = ""
                        End Try
                        Try
                            Integer.TryParse(TA0001ALLrow("SHABANORGSEQ"), WW_INT)
                            TA0001ALLrow("SHABANORGSEQ") = WW_INT.ToString("0000")
                        Catch ex As Exception
                            TA0001ALLrow("SHABANORGSEQ") = "9999"
                        End Try

                        Try
                            Integer.TryParse(TA0001ALLrow("TODKORGSEQ"), WW_INT)
                            TA0001ALLrow("TODKORGSEQ") = WW_INT.ToString("0000")
                        Catch ex As Exception
                            TA0001ALLrow("TODKORGSEQ") = "9999"
                        End Try

                        '○項目名称設定
                        '業務車番名称
                        CodeToName("BSHABAN", TA0001ALLrow("GSHABAN"), TA0001ALLrow("GSHABANLICNPLTNO"), O_RTN)
                        'コンテナシャーシ名称
                        CodeToName("CONTENAR", TA0001ALLrow("CONTCHASSIS"), TA0001ALLrow("CONTCHASSISLICNPLTNO"), O_RTN)
                        '積置区分名称
                        CodeToName("TUMIOKIKBN", TA0001ALLrow("TUMIOKIKBN"), TA0001ALLrow("TUMIOKIKBNNAME"), O_RTN)
                        '単位名称
                        CodeToName("HTANI", TA0001ALLrow("HTANI"), TA0001ALLrow("HTANINAME"), O_RTN)
                        '課税区分名称
                        CodeToName("TAXKBN", TA0001ALLrow("TAXKBN"), TA0001ALLrow("TAXKBNNAME"), O_RTN)
                        '油種名称
                        CodeToName("OILTYPE", TA0001ALLrow("OILTYPE"), TA0001ALLrow("OILTYPENAME"), O_RTN)
                        '品名１名称
                        CodeToName("PROD1", TA0001ALLrow("PRODUCT1"), TA0001ALLrow("PRODUCT1NAME"), O_RTN)
                        '売上計上基準名称
                        CodeToName("URIKBN", TA0001ALLrow("URIKBN"), TA0001ALLrow("URIKBNNAME"), O_RTN)
                        '状態名称
                        CodeToName("STATUS", TA0001ALLrow("STATUS"), TA0001ALLrow("STATUSNAME"), O_RTN)
                        '臭有無名称
                        CodeToName("SMELLKBN", TA0001ALLrow("SMELLKBN"), TA0001ALLrow("SMELLKBNNAME"), O_RTN)
                        If TA0001ALLrow("TUMIOKIKBN") = "1" Then
                            If TA0001ALLrow("SHUKODATE") = TA0001ALLrow("SHUKADATE") Then
                                TA0001ALLrow("TUMIOKI") = GRTA0001WRKINC.C_TUMI_NAME.TUMIOKI
                            Else
                                TA0001ALLrow("TUMIOKI") = GRTA0001WRKINC.C_TUMI_NAME.TUMIHAI
                            End If
                        Else
                            TA0001ALLrow("TUMIOKI") = ""
                        End If

                        '○表示項目編集
                        If TA0001ALLrow("CAMPCODENAME") = Nothing AndAlso TA0001ALLrow("CAMPCODE") = Nothing Then
                            TA0001ALLrow("CAMPCODE_TXT") = ""
                        Else
                            TA0001ALLrow("CAMPCODE_TXT") = TA0001ALLrow("CAMPCODENAME") & " (" & TA0001ALLrow("CAMPCODE") & ")"                '会社コード
                        End If

                        If TA0001ALLrow("TORICODENAME") = Nothing AndAlso TA0001ALLrow("TORICODE") = Nothing Then
                            TA0001ALLrow("TORICODE_TXT") = ""
                        Else
                            TA0001ALLrow("TORICODE_TXT") = TA0001ALLrow("TORICODENAME") & " (" & TA0001ALLrow("TORICODE") & ")"                '取引先コード
                        End If

                        If TA0001ALLrow("OILTYPENAME") = Nothing AndAlso TA0001ALLrow("OILTYPE") = Nothing Then
                            TA0001ALLrow("OILTYPE_TXT") = ""
                        Else
                            TA0001ALLrow("OILTYPE_TXT") = TA0001ALLrow("OILTYPENAME") & " (" & TA0001ALLrow("OILTYPE") & ")"                   '油種
                        End If

                        If TA0001ALLrow("ORDERORGNAME") = Nothing AndAlso TA0001ALLrow("ORDERORG") = Nothing Then
                            TA0001ALLrow("ORDERORG_TXT") = ""
                        Else
                            TA0001ALLrow("ORDERORG_TXT") = TA0001ALLrow("ORDERORGNAME") & " (" & TA0001ALLrow("ORDERORG") & ")"                '受注受付部署
                        End If
                        If TA0001ALLrow("SHIPORGNAME") = Nothing AndAlso TA0001ALLrow("SHIPORG") = Nothing Then
                            TA0001ALLrow("SHIPORG_TXT") = ""
                        Else
                            TA0001ALLrow("SHIPORG_TXT") = TA0001ALLrow("SHIPORGNAME") & " (" & TA0001ALLrow("SHIPORG") & ")"                   '出荷部署
                        End If

                        If TA0001ALLrow("GSHABANLICNPLTNO") = Nothing AndAlso TA0001ALLrow("GSHABAN") = Nothing Then
                            TA0001ALLrow("GSHABAN_TXT") = ""
                        Else
                            TA0001ALLrow("GSHABAN_TXT") = TA0001ALLrow("GSHABANLICNPLTNO") & " (" & TA0001ALLrow("GSHABAN") & ")"              '業務車番
                        End If
                        If TA0001ALLrow("STATUSNAME") = Nothing AndAlso TA0001ALLrow("STATUS") = Nothing Then
                            TA0001ALLrow("STATUS_TXT") = ""
                        Else
                            TA0001ALLrow("STATUS_TXT") = TA0001ALLrow("STATUSNAME") & " (" & TA0001ALLrow("STATUS") & ")"                      '状態
                        End If

                        If TA0001ALLrow("TUMIOKIKBNNAME") = Nothing AndAlso TA0001ALLrow("TUMIOKIKBN") = Nothing Then
                            TA0001ALLrow("TUMIOKIKBN_TXT") = ""
                        Else
                            TA0001ALLrow("TUMIOKIKBN_TXT") = TA0001ALLrow("TUMIOKIKBNNAME") & " (" & TA0001ALLrow("TUMIOKIKBN") & ")"          '積置区分
                        End If

                        If TA0001ALLrow("SHUKABASHONAME") = Nothing AndAlso TA0001ALLrow("SHUKABASHO") = Nothing Then
                            TA0001ALLrow("SHUKABASHO_TXT") = ""
                        Else
                            TA0001ALLrow("SHUKABASHO_TXT") = TA0001ALLrow("SHUKABASHONAME") & " (" & TA0001ALLrow("SHUKABASHO") & ")"          '出荷場所
                        End If

                        If TA0001ALLrow("STAFFCODENAME") = Nothing AndAlso TA0001ALLrow("STAFFCODE") Then
                            TA0001ALLrow("STAFFCODE_TXT") = ""
                        Else
                            TA0001ALLrow("STAFFCODE_TXT") = TA0001ALLrow("STAFFCODENAME") & " (" & TA0001ALLrow("STAFFCODE") & ")"             '乗務員コード
                        End If

                        If TA0001ALLrow("SUBSTAFFCODENAME") = Nothing AndAlso TA0001ALLrow("SUBSTAFFCODE") = Nothing Then
                            TA0001ALLrow("SUBSTAFFCODE_TXT") = ""
                        Else
                            TA0001ALLrow("SUBSTAFFCODE_TXT") = TA0001ALLrow("SUBSTAFFCODENAME") & " (" & TA0001ALLrow("SUBSTAFFCODE") & ")"    '副乗務員コード
                        End If

                        If TA0001ALLrow("TODOKECODENAME") = Nothing AndAlso TA0001ALLrow("TODOKECODE") = Nothing Then
                            TA0001ALLrow("TODOKECODE_TXT") = ""
                        Else
                            TA0001ALLrow("TODOKECODE_TXT") = TA0001ALLrow("TODOKECODENAME") & " (" & TA0001ALLrow("TODOKECODE") & ")"          '届先コード
                        End If

                        If TA0001ALLrow("PRODUCT1NAME") = Nothing AndAlso TA0001ALLrow("PRODUCT1") = Nothing Then
                            TA0001ALLrow("PRODUCT1_TXT") = ""
                        Else
                            TA0001ALLrow("PRODUCT1_TXT") = TA0001ALLrow("PRODUCT1NAME") & " (" & TA0001ALLrow("PRODUCT1") & ")"                '品名１
                        End If

                        If TA0001ALLrow("PRODUCT2NAME") = Nothing AndAlso TA0001ALLrow("PRODUCT2") = Nothing Then
                            TA0001ALLrow("PRODUCT2_TXT") = ""
                        Else
                            TA0001ALLrow("PRODUCT2_TXT") = TA0001ALLrow("PRODUCT2NAME") & " (" & TA0001ALLrow("PRODUCT2") & ")"                '品名２
                        End If

                        If TA0001ALLrow("SMELLKBNNAME") = Nothing AndAlso TA0001ALLrow("SMELLKBN") = Nothing Then
                            TA0001ALLrow("SMELLKBN_TXT") = ""
                        Else
                            TA0001ALLrow("SMELLKBN_TXT") = TA0001ALLrow("SMELLKBNNAME") & " (" & TA0001ALLrow("SMELLKBN") & ")"                '臭有無
                        End If

                        If TA0001ALLrow("HTANINAME") = Nothing AndAlso TA0001ALLrow("HTANI") = Nothing Then
                            TA0001ALLrow("HTANI_TXT") = ""
                        Else
                            TA0001ALLrow("HTANI_TXT") = TA0001ALLrow("HTANINAME") & " (" & TA0001ALLrow("HTANI") & ")"                         '配送単位
                        End If

                        If TA0001ALLrow("SURYO") = Nothing AndAlso TA0001ALLrow("HTANINAME") = Nothing Then
                            TA0001ALLrow("SURYO_TXT") = ""
                        Else
                            TA0001ALLrow("SURYO_TXT") = TA0001ALLrow("SURYO") & " " & TA0001ALLrow("HTANINAME")                                '数量+単位
                        End If

                        If TA0001ALLrow("JSURYO") = Nothing AndAlso TA0001ALLrow("HTANINAME") = Nothing Then
                            TA0001ALLrow("JSURYO_TXT") = ""
                        Else
                            TA0001ALLrow("JSURYO_TXT") = TA0001ALLrow("JSURYO") & " " & TA0001ALLrow("HTANINAME")                              '配送実績数量
                        End If

                        If TA0001ALLrow("STORICODENAME") = Nothing AndAlso TA0001ALLrow("STORICODE") = Nothing Then
                            TA0001ALLrow("STORICODE_TXT") = ""
                        Else
                            TA0001ALLrow("STORICODE_TXT") = TA0001ALLrow("STORICODENAME") & " (" & TA0001ALLrow("STORICODE") & ")"             '請求取引先コード
                        End If

                        If TA0001ALLrow("URIKBNNAME") = Nothing AndAlso TA0001ALLrow("URIKBN") = Nothing Then
                            TA0001ALLrow("URIKBN_TXT") = ""
                        Else
                            TA0001ALLrow("URIKBN_TXT") = TA0001ALLrow("URIKBNNAME") & " (" & TA0001ALLrow("URIKBN") & ")"                      '売上計上基準
                        End If

                        If TA0001ALLrow("TERMORGNAME") = Nothing AndAlso TA0001ALLrow("TERMORG") = Nothing Then
                            TA0001ALLrow("TERMORG_TXT") = ""
                        Else
                            TA0001ALLrow("TERMORG_TXT") = TA0001ALLrow("TERMORGNAME") & " (" & TA0001ALLrow("TERMORG") & ")"                   '端末設置部署
                        End If

                        If TA0001ALLrow("CONTCHASSISLICNPLTNO") = Nothing AndAlso TA0001ALLrow("CONTCHASSIS") = Nothing Then
                            TA0001ALLrow("CONTCHASSIS_TXT") = ""
                        Else
                            TA0001ALLrow("CONTCHASSIS_TXT") = TA0001ALLrow("CONTCHASSISLICNPLTNO") & " (" & TA0001ALLrow("CONTCHASSIS") & ")"  'コンテナシャーシ
                        End If

                        If TA0001ALLrow("SHARYOTYPEF") = Nothing AndAlso TA0001ALLrow("TSHABANF") = Nothing Then
                            TA0001ALLrow("TSHABANF_TXT") = ""
                        Else
                            TA0001ALLrow("TSHABANF_TXT") = TA0001ALLrow("SHARYOTYPEF") & TA0001ALLrow("TSHABANF")                              '統一車番(前)
                        End If

                        If TA0001ALLrow("SHARYOTYPEB") = Nothing AndAlso TA0001ALLrow("TSHABANB") = Nothing Then
                            TA0001ALLrow("TSHABANB_TXT") = ""
                        Else
                            TA0001ALLrow("TSHABANB_TXT") = TA0001ALLrow("SHARYOTYPEB") & TA0001ALLrow("TSHABANB")                              '統一車番(後)
                        End If

                        If TA0001ALLrow("SHARYOTYPEB2") = Nothing AndAlso TA0001ALLrow("TSHABANB2") = Nothing Then
                            TA0001ALLrow("TSHABANB2_TXT") = ""
                        Else
                            TA0001ALLrow("TSHABANB2_TXT") = TA0001ALLrow("SHARYOTYPEB2") & TA0001ALLrow("TSHABANB2")                           '統一車番(後)2
                        End If

                        If TA0001ALLrow("TAXKBNNAME") = Nothing AndAlso TA0001ALLrow("TAXKBN") = Nothing Then
                            TA0001ALLrow("TAXKBN_TXT") = ""
                        Else
                            TA0001ALLrow("TAXKBN_TXT") = TA0001ALLrow("TAXKBNNAME") & " (" & TA0001ALLrow("TAXKBN") & ")"                      '税区分
                        End If

                        TA0001ALLrow("TRIPNO_TXT") = TA0001ALLrow("TRIPNO")                                                                 'トリップ
                        TA0001ALLrow("DROPNO_TXT") = TA0001ALLrow("DROPNO")                                                                 'ドロップ
                        TA0001ALLrow("SEQ_TXT") = TA0001ALLrow("SEQ")                                                                       '枝番
                        TA0001ALLrow("KIJUNDATE_TXT") = TA0001ALLrow("KIJUNDATE")                                                           '基準日
                        TA0001ALLrow("ORDERNO_TXT") = TA0001ALLrow("ORDERNO")                                                               '受注番号
                        TA0001ALLrow("DETAILNO_TXT") = TA0001ALLrow("DETAILNO")                                                             '明細№
                        TA0001ALLrow("SHUKODATE_TXT") = TA0001ALLrow("SHUKODATE")                                                           '出庫日
                        TA0001ALLrow("KIKODATE_TXT") = TA0001ALLrow("KIKODATE")                                                             '帰庫日
                        TA0001ALLrow("SHUKADATE_TXT") = TA0001ALLrow("SHUKADATE")                                                           '出荷日
                        TA0001ALLrow("TODOKEDATE_TXT") = TA0001ALLrow("TODOKEDATE")                                                         '届日
                        TA0001ALLrow("TODOKETIME_TXT") = TA0001ALLrow("TODOKETIME")                                                         '時間指定（配送）
                        TA0001ALLrow("GATE_TXT") = TA0001ALLrow("GATE")                                                                     'ゲート
                        TA0001ALLrow("TUMIBA_TXT") = TA0001ALLrow("TUMIBA")                                                                 '積場
                        TA0001ALLrow("TUMISEQ_TXT") = TA0001ALLrow("TUMISEQ")                                                               '積順
                        TA0001ALLrow("SHUKADENNO_TXT") = TA0001ALLrow("SHUKADENNO")                                                         '出荷伝票番号
                        TA0001ALLrow("INTIME_TXT") = TA0001ALLrow("INTIME")                                                                 '時間指定（入構）
                        TA0001ALLrow("OUTTIME_TXT") = TA0001ALLrow("OUTTIME")                                                               '時間指定（出構）
                        TA0001ALLrow("STTIME_TXT") = TA0001ALLrow("STTIME")                                                                 '出勤時間
                        TA0001ALLrow("RYOME_TXT") = TA0001ALLrow("RYOME")                                                                   '両目
                        TA0001ALLrow("CONTNO_TXT") = TA0001ALLrow("CONTNO")                                                                 'コンテナ番号
                        TA0001ALLrow("PRATIO_TXT") = TA0001ALLrow("PRATIO")                                                                 'Ｐ比率
                        TA0001ALLrow("SHAFUKU_TXT") = TA0001ALLrow("SHAFUKU")                                                               '車腹（積載量）
                        TA0001ALLrow("REMARKS1_TXT") = TA0001ALLrow("REMARKS1")                                                             '備考１
                        TA0001ALLrow("REMARKS2_TXT") = TA0001ALLrow("REMARKS2")                                                             '備考２
                        TA0001ALLrow("REMARKS3_TXT") = TA0001ALLrow("REMARKS3")                                                             '備考３
                        TA0001ALLrow("REMARKS4_TXT") = TA0001ALLrow("REMARKS4")                                                             '備考４
                        TA0001ALLrow("REMARKS5_TXT") = TA0001ALLrow("REMARKS5")                                                             '備考５
                        TA0001ALLrow("REMARKS6_TXT") = TA0001ALLrow("REMARKS6")                                                             '備考６
                        TA0001ALLrow("TORIORDERNO_TXT") = TA0001ALLrow("TORIORDERNO")                                                       '荷主受注番号
                        TA0001ALLrow("TUMIOKI_TXT") = TA0001ALLrow("TUMIOKI")                                                               '積置内容
                        TA0001ALLrow("DELFLG_TXT") = TA0001ALLrow("DELFLG")                                                                 '削除
                        TA0001ALLrow("POSTNUM_TXT") = TA0001ALLrow("POSTNUM1") & "-" & TA0001ALLrow("POSTNUM2")                             '郵便番号  OK
                        TA0001ALLrow("ADDR1_TXT") = TA0001ALLrow("ADDR1")                                                                   '住所１    OK
                        TA0001ALLrow("ADDR2_TXT") = TA0001ALLrow("ADDR2")                                                                   '住所２
                        TA0001ALLrow("ADDR3_TXT") = TA0001ALLrow("ADDR3")                                                                   '住所３
                        TA0001ALLrow("ADDR4_TXT") = TA0001ALLrow("ADDR4")                                                                   '住所４
                        TA0001ALLrow("DISTANCE_TXT") = TA0001ALLrow("DISTANCE")                                                             '配送距離   OK
                        TA0001ALLrow("ARRIVTIME_TXT") = TA0001ALLrow("ARRIVTIME")                                                           '所要時間   OK
                        TA0001ALLrow("NOTES1_TXT") = TA0001ALLrow("NOTES1")                                                                 '届先特定要件０１   OK
                        TA0001ALLrow("NOTES2_TXT") = TA0001ALLrow("NOTES2")                                                                 '届先特定要件０２
                        TA0001ALLrow("NOTES3_TXT") = TA0001ALLrow("NOTES3")                                                                 '届先特定要件０３
                        TA0001ALLrow("NOTES4_TXT") = TA0001ALLrow("NOTES4")                                                                 '届先特定要件０４
                        TA0001ALLrow("NOTES5_TXT") = TA0001ALLrow("NOTES5")                                                                 '届先特定要件０５
                        TA0001ALLrow("NOTES6_TXT") = TA0001ALLrow("NOTES6")                                                                 '届先特定要件０６
                        TA0001ALLrow("NOTES7_TXT") = TA0001ALLrow("NOTES7")                                                                 '届先特定要件０７
                        TA0001ALLrow("NOTES8_TXT") = TA0001ALLrow("NOTES8")                                                                 '届先特定要件０８
                        TA0001ALLrow("NOTES9_TXT") = TA0001ALLrow("NOTES9")                                                                 '届先特定要件０９
                        TA0001ALLrow("NOTES10_TXT") = TA0001ALLrow("NOTES10")                                                               '届先特定要件１０
                        TA0001ALLrow("STAFFNOTES1_TXT") = TA0001ALLrow("STAFFNOTES1")                                                       '乗務員特定要件１  OK
                        TA0001ALLrow("STAFFNOTES2_TXT") = TA0001ALLrow("STAFFNOTES2")                                                       '乗務員特定要件２
                        TA0001ALLrow("STAFFNOTES3_TXT") = TA0001ALLrow("STAFFNOTES3")                                                       '乗務員特定要件３
                        TA0001ALLrow("STAFFNOTES4_TXT") = TA0001ALLrow("STAFFNOTES4")                                                       '乗務員特定要件４
                        TA0001ALLrow("STAFFNOTES5_TXT") = TA0001ALLrow("STAFFNOTES5")                                                       '乗務員特定要件５
                        TA0001ALLrow("SHARYOINFO1_TXT") = TA0001ALLrow("SHARYOINFO1")                                                       '車両情報１  OK
                        TA0001ALLrow("SHARYOINFO2_TXT") = TA0001ALLrow("SHARYOINFO2")                                                       '車両情報２
                        TA0001ALLrow("SHARYOINFO3_TXT") = TA0001ALLrow("SHARYOINFO3")                                                       '車両情報３
                        TA0001ALLrow("SHARYOINFO4_TXT") = TA0001ALLrow("SHARYOINFO4")                                                       '車両情報４
                        TA0001ALLrow("SHARYOINFO5_TXT") = TA0001ALLrow("SHARYOINFO5")                                                       '車両情報５
                        TA0001ALLrow("SHARYOINFO6_TXT") = TA0001ALLrow("SHARYOINFO6")                                                       '車両情報６

                    Next

                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ソート
        'ソート文字列取得
        CS0026TBLSORTget.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORTget.PROFID = Master.PROF_VIEW
        CS0026TBLSORTget.MAPID = Master.MAPID
        CS0026TBLSORTget.VARI = Master.MAPvariant
        CS0026TBLSORTget.TAB = ""
        CS0026TBLSORTget.getSorting()

        'ソート＆データ抽出
        CS0026TBLSORTget.TABLE = TA0001ALL
        CS0026TBLSORTget.FILTER = "SELECT = 1"
        TA0001ALL = CS0026TBLSORTget.sort()

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To TA0001ALL.Rows.Count - 1
            Dim TA0001ALLrow As DataRow = TA0001ALL.Rows(i)
            If TA0001ALLrow("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1
                WW_SEQ = 0

                For j As Integer = i To TA0001ALL.Rows.Count - 1

                    If TA0001ALL.Rows(j)("LINECNT") = 0 Then
                        If TA0001ALL.Rows(j)("TORICODE") = TA0001ALLrow("TORICODE") AndAlso
                           TA0001ALL.Rows(j)("OILTYPE") = TA0001ALLrow("OILTYPE") AndAlso
                           TA0001ALL.Rows(j)("KIJUNDATE") = TA0001ALLrow("KIJUNDATE") AndAlso
                           TA0001ALL.Rows(j)("ORDERORG") = TA0001ALLrow("ORDERORG") AndAlso
                           TA0001ALL.Rows(j)("SHIPORG") = TA0001ALLrow("SHIPORG") AndAlso
                           TA0001ALL.Rows(j)("SHUKODATE") = TA0001ALLrow("SHUKODATE") AndAlso
                           TA0001ALL.Rows(j)("GSHABAN") = TA0001ALLrow("GSHABAN") AndAlso
                           TA0001ALL.Rows(j)("RYOME") = TA0001ALLrow("RYOME") AndAlso
                           TA0001ALL.Rows(j)("TRIPNO") = TA0001ALLrow("TRIPNO") AndAlso
                           TA0001ALL.Rows(j)("DROPNO") = TA0001ALLrow("DROPNO") Then

                            WW_SEQ = WW_SEQ + 1

                            If WW_SEQ = 1 Then
                                TA0001ALL.Rows(j)("LINECNT") = WW_LINECNT
                                TA0001ALL.Rows(j)("SEQ") = WW_SEQ.ToString("00")
                                TA0001ALL.Rows(j)("HIDDEN") = 0
                            Else
                                TA0001ALL.Rows(j)("LINECNT") = WW_LINECNT
                                TA0001ALL.Rows(j)("SEQ") = WW_SEQ.ToString("00")
                                TA0001ALL.Rows(j)("HIDDEN") = 1
                            End If
                        End If

                    End If
                Next

            End If

        Next

        '○数量、台数合計の設定
        Dim SURYO_SUM As Decimal = 0
        Dim DAISU_SUM As Long = 0

        CS0026TBLSORTget.TABLE = TA0001ALL
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,RYOME ,TRIPNO ,DROPNO ,SEQ"
        CS0026TBLSORTget.FILTER = ""
        TA0001ALL = CS0026TBLSORTget.sort()

        '最終行から初回行へループ
        For i As Integer = 0 To TA0001ALL.Rows.Count - 1

            Dim TA0001ALLrow As DataRow = TA0001ALL.Rows(i)
            If TA0001ALLrow("SEQ") = "01" AndAlso TA0001ALLrow("DELFLG") <> C_DELETE_FLG.DELETE Then
                SURYO_SUM = 0
                DAISU_SUM = 0

                For j As Integer = i To TA0001ALL.Rows.Count - 1
                    If TA0001ALLrow("TORICODE") = TA0001ALL.Rows(j)("TORICODE") AndAlso
                       TA0001ALLrow("OILTYPE") = TA0001ALL.Rows(j)("OILTYPE") AndAlso
                       TA0001ALLrow("KIJUNDATE") = TA0001ALL.Rows(j)("KIJUNDATE") AndAlso
                       TA0001ALLrow("ORDERORG") = TA0001ALL.Rows(j)("ORDERORG") AndAlso
                       TA0001ALLrow("SHIPORG") = TA0001ALL.Rows(j)("SHIPORG") AndAlso
                       TA0001ALLrow("SHUKODATE") = TA0001ALL.Rows(j)("SHUKODATE") AndAlso
                       TA0001ALLrow("GSHABAN") = TA0001ALL.Rows(j)("GSHABAN") AndAlso
                       TA0001ALLrow("RYOME") = TA0001ALL.Rows(j)("RYOME") AndAlso
                       TA0001ALLrow("TRIPNO") = TA0001ALL.Rows(j)("TRIPNO") AndAlso
                       TA0001ALLrow("DROPNO") = TA0001ALL.Rows(j)("DROPNO") AndAlso
                       TA0001ALL.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then

                        Try
                            SURYO_SUM = SURYO_SUM + CDbl(TA0001ALL.Rows(j)("SURYO"))
                        Catch ex As Exception
                        End Try

                        Try
                            DAISU_SUM = DAISU_SUM + CInt(TA0001ALL.Rows(j)("DAISU"))
                        Catch ex As Exception
                        End Try

                    Else
                        Exit For
                    End If

                Next

                '表示行にサマリ結果を反映
                TA0001ALLrow("SURYO_SUM") = SURYO_SUM.ToString("0.000")
                TA0001ALLrow("DAISU_SUM") = DAISU_SUM.ToString("0")
                TA0001ALLrow("HIDDEN") = 0   '0:表示

            Else
                TA0001ALLrow("HIDDEN") = 1   '1:非表示
            End If

        Next

    End Sub

    ''' <summary>
    ''' A0001VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <param name="I_CODE">検索条件コード</param>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0001(ByVal I_CODE As String)

        '〇 TA0001ALLよりデータ抽出
        Dim WW_FILTER As String = ""
        TA0001VIEWtbl = TA0001ALL.Copy

        Select Case work.WF_SEL_FUNCSEL.Text
            Case GRTA0001WRKINC.C_LIST_FUNSEL.DRIVER        '乗務員別
                WW_FILTER = "STAFFCODE = '" & I_CODE & "' or SUBSTAFFCODE = '" & I_CODE & "'"
            Case GRTA0001WRKINC.C_LIST_FUNSEL.CARNUM        '車番別
                WW_FILTER = "GSHABAN = '" & I_CODE & "'"
            Case Else                                       '出荷場所別
                WW_FILTER = "SHUKABASHO = '" & I_CODE & "' and TUMIOKI <> '" & GRTA0001WRKINC.C_TUMI_NAME.TUMIHAI & "'"
        End Select

        CS0026TBLSORTget.TABLE = TA0001VIEWtbl
        CS0026TBLSORTget.SORTING = "LINECNT , SEQ ASC"
        CS0026TBLSORTget.FILTER = WW_FILTER
        TA0001VIEWtbl = CS0026TBLSORTget.sort
        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0001VIEWrow As DataRow In TA0001VIEWtbl.Rows
            TA0001VIEWrow("LINECNT") = 0
        Next

        For i As Integer = 0 To TA0001VIEWtbl.Rows.Count - 1
            Dim TA0001VIEWrow As DataRow = TA0001VIEWtbl.Rows(i)

            If TA0001VIEWrow("LINECNT") = 0 Then
                WW_LINECNT = WW_LINECNT + 1
                WW_SEQ = 0
                For j As Integer = i To TA0001VIEWtbl.Rows.Count - 1

                    If TA0001VIEWtbl.Rows(j)("LINECNT") = 0 Then
                        If TA0001VIEWtbl.Rows(j)("TORICODE") = TA0001VIEWrow("TORICODE") AndAlso
                           TA0001VIEWtbl.Rows(j)("OILTYPE") = TA0001VIEWrow("OILTYPE") AndAlso
                           TA0001VIEWtbl.Rows(j)("KIJUNDATE") = TA0001VIEWrow("KIJUNDATE") AndAlso
                           TA0001VIEWtbl.Rows(j)("ORDERORG") = TA0001VIEWrow("ORDERORG") AndAlso
                           TA0001VIEWtbl.Rows(j)("SHIPORG") = TA0001VIEWrow("SHIPORG") AndAlso
                           TA0001VIEWtbl.Rows(j)("SHUKODATE") = TA0001VIEWrow("SHUKODATE") AndAlso
                           TA0001VIEWtbl.Rows(j)("GSHABAN") = TA0001VIEWrow("GSHABAN") AndAlso
                           TA0001VIEWtbl.Rows(j)("RYOME") = TA0001VIEWrow("RYOME") AndAlso
                           TA0001VIEWtbl.Rows(j)("TRIPNO") = TA0001VIEWrow("TRIPNO") AndAlso
                           TA0001VIEWtbl.Rows(j)("DROPNO") = TA0001VIEWrow("DROPNO") Then

                            WW_SEQ = WW_SEQ + 1

                            If WW_SEQ = 1 Then
                                TA0001VIEWtbl.Rows(j)("LINECNT") = WW_LINECNT
                                TA0001VIEWtbl.Rows(j)("SEQ") = WW_SEQ.ToString("00")
                                TA0001VIEWtbl.Rows(j)("HIDDEN") = 0
                            Else
                                TA0001VIEWtbl.Rows(j)("LINECNT") = WW_LINECNT
                                TA0001VIEWtbl.Rows(j)("SEQ") = WW_SEQ.ToString("00")
                                TA0001VIEWtbl.Rows(j)("HIDDEN") = 1
                            End If
                        End If

                    End If
                Next

            End If

        Next

    End Sub

    ''' <summary>
    ''' セレクター設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InitialSelector()

        'テンポラリDB項目作成

        If IsNothing(SELECTORtbl) Then SELECTORtbl = New DataTable
        SELECTORtbl.Clear()
        SELECTORtbl.Columns.Add("SEQ", GetType(String))                         'SEQ                表示順番
        SELECTORtbl.Columns.Add("CODE", GetType(String))                        'CODE               コード
        SELECTORtbl.Columns.Add("NAME", GetType(String))                        'NAME               名称
        SELECTORtbl.Columns.Add("CNT", GetType(Integer))                        'CNT                Work

        '○ソート＆データ抽出
        CS0026TBLSORTget.TABLE = TA0001ALL
        CS0026TBLSORTget.SORTING = "LINECNT , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        TA0001ALL = CS0026TBLSORTget.sort()
        '機能選択
        Select Case work.WF_SEL_FUNCSEL.Text
            Case GRTA0001WRKINC.C_LIST_FUNSEL.DRIVER        '乗務員別
                For Each TA0001ALLrow As DataRow In TA0001ALL.Rows

                    Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
                    SELECTORrow("SEQ") = TA0001ALLrow("STAFFORGSEQ")
                    SELECTORrow("CODE") = TA0001ALLrow("STAFFCODE")
                    SELECTORrow("NAME") = TA0001ALLrow("STAFFCODE_TXT")
                    SELECTORrow("CNT") = 0
                    SELECTORtbl.Rows.Add(SELECTORrow)

                    If Not IsNothing(TA0001ALLrow("SUBSTAFFCODE")) Then
                        SELECTORrow = SELECTORtbl.NewRow
                        SELECTORrow("SEQ") = TA0001ALLrow("SUBSTAFFORGSEQ")
                        SELECTORrow("CODE") = TA0001ALLrow("SUBSTAFFCODE")
                        SELECTORrow("NAME") = TA0001ALLrow("SUBSTAFFCODE_TXT")
                        SELECTORrow("CNT") = 0
                        SELECTORtbl.Rows.Add(SELECTORrow)
                    End If
                Next

            Case GRTA0001WRKINC.C_LIST_FUNSEL.CARNUM       '車番別
                For Each TA0001ALLrow As DataRow In TA0001ALL.Rows

                    Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
                    SELECTORrow("SEQ") = TA0001ALLrow("SHABANORGSEQ")
                    SELECTORrow("CODE") = TA0001ALLrow("GSHABAN")
                    SELECTORrow("NAME") = TA0001ALLrow("GSHABAN_TXT")
                    SELECTORrow("CNT") = 0
                    SELECTORtbl.Rows.Add(SELECTORrow)

                Next

            Case Else       '出荷場所別
                For Each TA0001ALLrow As DataRow In TA0001ALL.Rows

                    Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
                    SELECTORrow("SEQ") = TA0001ALLrow("TODKORGSEQ")
                    SELECTORrow("CODE") = TA0001ALLrow("SHUKABASHO")
                    SELECTORrow("NAME") = TA0001ALLrow("SHUKABASHO_TXT")
                    SELECTORrow("CNT") = 0
                    SELECTORtbl.Rows.Add(SELECTORrow)

                Next

        End Select

        Dim WW_BRKey As String = ""
        CS0026TBLSORTget.TABLE = SELECTORtbl
        CS0026TBLSORTget.SORTING = "CODE"
        CS0026TBLSORTget.FILTER = ""
        SELECTORtbl = CS0026TBLSORTget.sort()

        For Each SELECTORrow As DataRow In SELECTORtbl.Rows
            If SELECTORrow("CODE") <> WW_BRKey Then
                SELECTORrow("CNT") = 1
                WW_BRKey = SELECTORrow("CODE")
            End If
        Next
        CS0026TBLSORTget.TABLE = SELECTORtbl
        CS0026TBLSORTget.SORTING = "SEQ , CODE , NAME"
        CS0026TBLSORTget.FILTER = "CNT = 1"
        SELECTORtbl = CS0026TBLSORTget.sort()

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

    End Sub

    ''' <summary>
    ''' セレクタークリック(選択変更)処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SELECTOR_Click()

        '■ データリカバリ
        '○ TA0001ALLデータリカバリ
        If Not Master.RecoverTable(TA0001ALL) Then Exit Sub


        '■ セレクター表示切替
        For Each item As RepeaterItem In WF_SELECTOR.Items
            '背景色
            If CType(item.FindControl("WF_SELECTOR_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTOR_Posi.Value Then
                CType(item.FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(item.FindControl("WF_SELECTOR_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;width:13.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If
        Next

        '■ GridView表示データ作成
        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        '○TA0001VIEWtbl取得
        GetViewTA0001(WF_SELECTOR_Posi.Value)
    End Sub

    ''' <summary>
    ''' GridView 明細行ダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        '■ 処理準備
        '○ GridViewのダブルクリック行位置取得
        Dim WW_LINECNT As Integer                                   'GridViewのダブルクリック行位置
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            WW_LINECNT = 1
        End Try

        '○ テーブルデータ 復元(Xmlファイルより復元)
        If Not Master.RecoverTable(TA0001ALL) Then Exit Sub

        '〇 TA0001VIEWtblカラム設定
        AddColumnsToTA0001Tbl(TA0001VIEWtbl)

        '○ GridView表示データ作成
        GetViewTA0001(WF_SELECTOR_Posi.Value)

        '■ Grid内容(TA0001VIEW)よりDetail編集
        '○ TA0001DETAILカラム設定
        AddColumnsToTA0001Tbl(TA0001DETAILtbl)

        '○ 詳細画面ヘッダー情報設定　＆　TA0001DETAILデータ作成
        For Each TA0001VIEWrow As DataRow In TA0001VIEWtbl.Rows
            '選択された行番号と同一のレコードをキー項目に設定
            If TA0001VIEWrow("LINECNT") = WW_LINECNT AndAlso TA0001VIEWrow("SELECT") = "1" Then
                WF_Sel_LINECNT.Text = TA0001VIEWrow("LINECNT")                  '選択No
                WF_SHUKODATE.Text = TA0001VIEWrow("SHUKODATE_TXT")              '出庫日
                WF_KIKODATE.Text = TA0001VIEWrow("KIKODATE_TXT")                '帰庫日
                WF_SHUKADATE.Text = TA0001VIEWrow("SHUKADATE_TXT")              '出荷日
                WF_TODOKEDATE.Text = TA0001VIEWrow("TODOKEDATE_TXT")            '届日
                WF_RYOME.Text = TA0001VIEWrow("RYOME_TXT")                      '両目
                WF_OILTYPE.Text = TA0001VIEWrow("OILTYPE_TXT")                  '油種
                WF_ORDERORG.Text = TA0001VIEWrow("ORDERORG_TXT")                '受注部署
                WF_SHIPORG.Text = TA0001VIEWrow("SHIPORG_TXT")                  '出荷部署
                WF_TORICODE.Text = TA0001VIEWrow("TORICODE_TXT")                '取引先
                WF_STORICODE.Text = TA0001VIEWrow("STORICODE_TXT")              '販売店
                WF_URIKBN.Text = TA0001VIEWrow("URIKBN_TXT")                    '売上計上基準
                WF_GSHABAN.Text = TA0001VIEWrow("GSHABAN_TXT")                  '業務車番
                WF_CONTCHASSIS.Text = TA0001VIEWrow("CONTCHASSIS_TXT")          'コンテナシャーシ
                WF_SHAFUKU.Text = TA0001VIEWrow("SHAFUKU_TXT")                  '車腹
                WF_TSHABANF.Text = TA0001VIEWrow("TSHABANF_TXT")                '統一車番（前）
                WF_TSHABANB.Text = TA0001VIEWrow("TSHABANB_TXT")                '統一車番（後）
                WF_TSHABANB2.Text = TA0001VIEWrow("TSHABANB2_TXT")              '統一車番（後）２
                WF_TUMIOKIKBN.Text = TA0001VIEWrow("TUMIOKIKBN_TXT")            '積置区分
                WF_TRIPNO.Text = TA0001VIEWrow("TRIPNO_TXT")                    'トリップ
                WF_DROPNO.Text = TA0001VIEWrow("DROPNO_TXT")                    'ドロップ
                WF_STAFFCODE.Text = TA0001VIEWrow("STAFFCODE_TXT")              '乗務員
                WF_SUBSTAFFCODE.Text = TA0001VIEWrow("SUBSTAFFCODE_TXT")        '副乗務員
                WF_STTIME.Text = TA0001VIEWrow("STTIME_TXT")                    '出勤時間

                '一時Table(TA0001DETAILtbl)準備
                TA0001DETAILtbl.ImportRow(TA0001VIEWrow)

            End If
        Next
        '〇明細の初期化
        InitialRepeater(TA0001DETAILtbl)
        'カーソル設定
        WF_SHUKODATE.Focus()
        '〇明細の非表示
        work.WF_IsHideDetailBox.Text = "0"
    End Sub

    ''' <summary>
    ''' Detail初期設定 （明細作成）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InitialRepeater(ByVal DataTable As DataTable)
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""
        Try

            'カラム情報をリピーター作成用に取得
            If IsNothing(DataTable) Then
                DataTable = New DataTable
                Master.CreateEmptyTable(DataTable)
                DataTable.Rows.Add(DataTable.NewRow())
            End If

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            'CS0052DetailView.TABID = ""
            CS0052DetailView.SRCDATA = DataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then Exit Sub

            WF_DetailMView.ActiveViewIndex = 0

            For row As Integer = 0 To CS0052DetailView.ROWMAX - 1
                For col As Integer = 1 To CS0052DetailView.COLMAX

                    'ダブルクリック時コード検索イベント追加
                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then
                        repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label)
                        repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox)
                        GetRepeaterAttributes(repField.Text, repAttr)
                        If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", repAttr)
                            repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), System.Web.UI.WebControls.Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                    End If

                Next col
            Next row

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            DataTable.Dispose()
            DataTable = Nothing
        End Try

    End Sub
    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_ATTR">イベント内容</param>
    ''' <remarks></remarks>
    Protected Sub GetRepeaterAttributes(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""

    End Sub
    ''' <summary>
    ''' カラムの初期設定処理
    ''' </summary>
    ''' <param name="IO_TBL">初期設定テーブル</param>
    ''' <remarks></remarks>
    Protected Sub AddColumnsToTA0001Tbl(ByRef IO_TBL As DataTable)
        '初期処理
        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        'テンポラリDB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))             'DBの固定フィールド
        IO_TBL.Columns.Add("OPERATION", GetType(String))            'DBの固定フィールド
        IO_TBL.Columns.Add("TIMSTP", GetType(Integer))              'DBの固定フィールド
        IO_TBL.Columns.Add("SELECT", GetType(Integer))              'DBの固定フィールド
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))              'DBの固定フィールド

        IO_TBL.Columns.Add("CAMPCODE", GetType(String))             'CAMPCODE           会社コード
        IO_TBL.Columns.Add("CAMPCODENAME", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))             'TORICODE           取引先コード
        IO_TBL.Columns.Add("TORICODENAME", GetType(String))
        IO_TBL.Columns.Add("OILTYPE", GetType(String))              'OILTYPE            油種
        IO_TBL.Columns.Add("OILTYPENAME", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))               'TRIPNO             トリップ
        IO_TBL.Columns.Add("DROPNO", GetType(String))               'DROPNO             ドロップ
        IO_TBL.Columns.Add("SEQ", GetType(String))                  'SEQ                枝番
        IO_TBL.Columns.Add("ORDERORG", GetType(String))             'ORDERORG           受注受付部署
        IO_TBL.Columns.Add("ORDERORGNAME", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))              'SHIPORG            出荷部署
        IO_TBL.Columns.Add("SHIPORGNAME", GetType(String))
        IO_TBL.Columns.Add("KIJUNDATE", GetType(String))            'KIJUNDATE          基準日
        IO_TBL.Columns.Add("ORDERNO", GetType(String))              'ORDERNO            受注番号
        IO_TBL.Columns.Add("DETAILNO", GetType(String))             'DETAILNO           明細№
        IO_TBL.Columns.Add("GSHABAN", GetType(String))              'GSHABAN            業務車番
        IO_TBL.Columns.Add("GSHABANLICNPLTNO", GetType(String))
        IO_TBL.Columns.Add("SHUKODATE", GetType(String))            'SHUKODATE          出庫日
        IO_TBL.Columns.Add("STATUS", GetType(String))               'STATUS             状態
        IO_TBL.Columns.Add("STATUSNAME", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBN", GetType(String))           'TUMIOKIKBN         積置区分
        IO_TBL.Columns.Add("TUMIOKIKBNNAME", GetType(String))
        IO_TBL.Columns.Add("KIKODATE", GetType(String))             'KIKODATE           帰庫日
        IO_TBL.Columns.Add("SHUKADATE", GetType(String))            'SHUKADATE          出荷日
        IO_TBL.Columns.Add("TODOKEDATE", GetType(String))           'TODOKEDATE         届日
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))           'SHUKABASHO         出荷場所
        IO_TBL.Columns.Add("SHUKABASHONAME", GetType(String))
        IO_TBL.Columns.Add("GATE", GetType(String))                 'GATE               ゲート
        IO_TBL.Columns.Add("TUMIBA", GetType(String))               'TUMIBA             積場
        IO_TBL.Columns.Add("TUMISEQ", GetType(String))              'TUMISEQ            積順
        IO_TBL.Columns.Add("SHUKADENNO", GetType(String))           'SHUKADENNO         出荷伝票番号
        IO_TBL.Columns.Add("INTIME", GetType(String))               'INTIME             時間指定（入構）
        IO_TBL.Columns.Add("OUTTIME", GetType(String))              'OUTTIME            時間指定（出構）
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))            'STAFFCODE          乗務員コード
        IO_TBL.Columns.Add("STAFFCODENAME", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFCODE", GetType(String))         'SUBSTAFFCODE       副乗務員コード
        IO_TBL.Columns.Add("SUBSTAFFCODENAME", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))               'STTIME             出勤時間
        IO_TBL.Columns.Add("RYOME", GetType(String))                'RYOME              両目
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))           'TODOKECODE         届先コード
        IO_TBL.Columns.Add("TODOKECODENAME", GetType(String))
        IO_TBL.Columns.Add("TODOKETIME", GetType(String))           'TODOKETIME         時間指定（配送）
        IO_TBL.Columns.Add("PRODUCT1", GetType(String))             'PRODUCT1           品名１
        IO_TBL.Columns.Add("PRODUCT1NAME", GetType(String))
        IO_TBL.Columns.Add("PRODUCT2", GetType(String))             'PRODUCT2           品名２
        IO_TBL.Columns.Add("PRODUCT2NAME", GetType(String))
        IO_TBL.Columns.Add("CONTNO", GetType(String))               'CONTNO             コンテナ番号
        IO_TBL.Columns.Add("PRATIO", GetType(String))               'PRATIO             Ｐ比率
        IO_TBL.Columns.Add("SMELLKBN", GetType(String))             'SMELLKBN           臭有無
        IO_TBL.Columns.Add("SMELLKBNNAME", GetType(String))
        IO_TBL.Columns.Add("SHAFUKU", GetType(String))              'SHAFUKU            車腹（積載量）
        IO_TBL.Columns.Add("HTANI", GetType(String))                'HTANI              配送単位
        IO_TBL.Columns.Add("HTANINAME", GetType(String))
        IO_TBL.Columns.Add("SURYO", GetType(String))                'SURYO              数量
        IO_TBL.Columns.Add("DAISU", GetType(String))                'DAISU              台数
        IO_TBL.Columns.Add("JSURYO", GetType(String))               'JSURYO             配送実績数量
        IO_TBL.Columns.Add("JDAISU", GetType(String))               'JDAISU             配送実績台数
        IO_TBL.Columns.Add("REMARKS1", GetType(String))             'REMARKS1           備考１
        IO_TBL.Columns.Add("REMARKS2", GetType(String))             'REMARKS2           備考２
        IO_TBL.Columns.Add("REMARKS3", GetType(String))             'REMARKS3           備考３
        IO_TBL.Columns.Add("REMARKS4", GetType(String))             'REMARKS4           備考４
        IO_TBL.Columns.Add("REMARKS5", GetType(String))             'REMARKS5           備考５
        IO_TBL.Columns.Add("REMARKS6", GetType(String))             'REMARKS6           備考６
        IO_TBL.Columns.Add("TORIORDERNO", GetType(String))          'TORIORDERNO        荷主受注番号
        IO_TBL.Columns.Add("STORICODE", GetType(String))            'STORICODE          請求取引先コード
        IO_TBL.Columns.Add("STORICODENAME", GetType(String))
        IO_TBL.Columns.Add("URIKBN", GetType(String))               'URIKBN             売上計上基準
        IO_TBL.Columns.Add("URIKBNNAME", GetType(String))
        IO_TBL.Columns.Add("TERMORG", GetType(String))              'TERMORG            端末設置部署
        IO_TBL.Columns.Add("TERMORGNAME", GetType(String))
        IO_TBL.Columns.Add("CONTCHASSIS", GetType(String))          'CONTCHASSIS        コンテナシャーシ
        IO_TBL.Columns.Add("CONTCHASSISLICNPLTNO", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEF", GetType(String))          'SHARYOTYPEF        統一車番(前)(上)
        IO_TBL.Columns.Add("TSHABANF", GetType(String))             'TSHABANF           統一車番(前)(下)
        IO_TBL.Columns.Add("SHARYOTYPEB", GetType(String))          'SHARYOTYPEB        統一車番(後)(上)
        IO_TBL.Columns.Add("TSHABANB", GetType(String))             'TSHABANB           統一車番(後)(下)
        IO_TBL.Columns.Add("SHARYOTYPEB2", GetType(String))         'SHARYOTYPEB2       統一車番(後)(上)2
        IO_TBL.Columns.Add("TSHABANB2", GetType(String))            'TSHABANB2          統一車番(後)(下)2
        IO_TBL.Columns.Add("TAXKBN", GetType(String))               'TAXKBN             税区分
        IO_TBL.Columns.Add("TAXKBNNAME", GetType(String))
        IO_TBL.Columns.Add("TUMIOKI", GetType(String))              'TUMIOKI            積置内容
        IO_TBL.Columns.Add("DELFLG", GetType(String))               'DELFLG             削除
        IO_TBL.Columns.Add("SURYO_SUM", GetType(String))            'SURYO_SUM
        IO_TBL.Columns.Add("DAISU_SUM", GetType(String))            'DAISU_SUM

        IO_TBL.Columns.Add("POSTNUM1", GetType(String))             'POSTNUM1           郵便番号１
        IO_TBL.Columns.Add("POSTNUM2", GetType(String))             'POSTNUM2           郵便番号２
        IO_TBL.Columns.Add("ADDR", GetType(String))                 'ADDR               住所
        IO_TBL.Columns.Add("ADDR1", GetType(String))                'ADDR1              住所１
        IO_TBL.Columns.Add("ADDR2", GetType(String))                'ADDR2              住所２
        IO_TBL.Columns.Add("ADDR3", GetType(String))                'ADDR3              住所３
        IO_TBL.Columns.Add("ADDR4", GetType(String))                'ADDR4              住所４
        IO_TBL.Columns.Add("DISTANCE", GetType(String))             'DISTANCE           配送距離
        IO_TBL.Columns.Add("ARRIVTIME", GetType(String))            'ARRIVTIME          所要時間
        IO_TBL.Columns.Add("NOTES1", GetType(String))               'NOTES01            届先特定要件０１
        IO_TBL.Columns.Add("NOTES2", GetType(String))               'NOTES02            届先特定要件０２
        IO_TBL.Columns.Add("NOTES3", GetType(String))               'NOTES03            届先特定要件０３
        IO_TBL.Columns.Add("NOTES4", GetType(String))               'NOTES04            届先特定要件０４
        IO_TBL.Columns.Add("NOTES5", GetType(String))               'NOTES05            届先特定要件０５
        IO_TBL.Columns.Add("NOTES6", GetType(String))               'NOTES06            届先特定要件０６
        IO_TBL.Columns.Add("NOTES7", GetType(String))               'NOTES07            届先特定要件０７
        IO_TBL.Columns.Add("NOTES8", GetType(String))               'NOTES08            届先特定要件０８
        IO_TBL.Columns.Add("NOTES9", GetType(String))               'NOTES09            届先特定要件０９
        IO_TBL.Columns.Add("NOTES10", GetType(String))              'NOTES10            届先特定要件１０
        IO_TBL.Columns.Add("STAFFNOTES1", GetType(String))          'STAFFNOTES1        乗務員特定要件１
        IO_TBL.Columns.Add("STAFFNOTES2", GetType(String))          'STAFFNOTES2        乗務員特定要件２
        IO_TBL.Columns.Add("STAFFNOTES3", GetType(String))          'STAFFNOTES3        乗務員特定要件３
        IO_TBL.Columns.Add("STAFFNOTES4", GetType(String))          'STAFFNOTES4        乗務員特定要件４
        IO_TBL.Columns.Add("STAFFNOTES5", GetType(String))          'STAFFNOTES5        乗務員特定要件５
        IO_TBL.Columns.Add("SHARYOINFO1", GetType(String))          'SHARYOINFO1        車両情報１
        IO_TBL.Columns.Add("SHARYOINFO2", GetType(String))          'SHARYOINFO2        車両情報２
        IO_TBL.Columns.Add("SHARYOINFO3", GetType(String))          'SHARYOINFO3        車両情報３
        IO_TBL.Columns.Add("SHARYOINFO4", GetType(String))          'SHARYOINFO4        車両情報４
        IO_TBL.Columns.Add("SHARYOINFO5", GetType(String))          'SHARYOINFO5        車両情報５
        IO_TBL.Columns.Add("SHARYOINFO6", GetType(String))          'SHARYOINFO6        車両情報６

        IO_TBL.Columns.Add("CAMPCODE_TXT", GetType(String))         'CAMPCODE           会社コード
        IO_TBL.Columns.Add("TORICODE_TXT", GetType(String))         'TORICODE           取引先コード
        IO_TBL.Columns.Add("OILTYPE_TXT", GetType(String))          'OILTYPE            油種
        IO_TBL.Columns.Add("TRIPNO_TXT", GetType(String))           'TRIPNO             トリップ
        IO_TBL.Columns.Add("DROPNO_TXT", GetType(String))           'DROPNO             ドロップ
        IO_TBL.Columns.Add("SEQ_TXT", GetType(String))              'SEQ                枝番
        IO_TBL.Columns.Add("ORDERORG_TXT", GetType(String))         'ORDERORG           受注受付部署
        IO_TBL.Columns.Add("SHIPORG_TXT", GetType(String))          'SHIPORG            出荷部署
        IO_TBL.Columns.Add("KIJUNDATE_TXT", GetType(String))        'KIJUNDATE          基準日
        IO_TBL.Columns.Add("ORDERNO_TXT", GetType(String))          'ORDERNO            受注番号
        IO_TBL.Columns.Add("DETAILNO_TXT", GetType(String))         'DETAILNO           明細№
        IO_TBL.Columns.Add("GSHABAN_TXT", GetType(String))          'GSHABAN            業務車番
        IO_TBL.Columns.Add("SHUKODATE_TXT", GetType(String))        'SHUKODATE          出庫日
        IO_TBL.Columns.Add("STATUS_TXT", GetType(String))           'STATUS             状態
        IO_TBL.Columns.Add("TUMIOKIKBN_TXT", GetType(String))       'TUMIOKIKBN         積置区分
        IO_TBL.Columns.Add("KIKODATE_TXT", GetType(String))         'KIKODATE           帰庫日
        IO_TBL.Columns.Add("SHUKADATE_TXT", GetType(String))        'SHUKADATE          出荷日
        IO_TBL.Columns.Add("TODOKEDATE_TXT", GetType(String))       'TODOKEDATE         届日
        IO_TBL.Columns.Add("SHUKABASHO_TXT", GetType(String))       'SHUKABASHO         出荷場所
        IO_TBL.Columns.Add("GATE_TXT", GetType(String))             'GATE               ゲート
        IO_TBL.Columns.Add("TUMIBA_TXT", GetType(String))           'TUMIBA             積場
        IO_TBL.Columns.Add("TUMISEQ_TXT", GetType(String))          'TUMISEQ            積順
        IO_TBL.Columns.Add("SHUKADENNO_TXT", GetType(String))       'SHUKADENNO         出荷伝票番号
        IO_TBL.Columns.Add("INTIME_TXT", GetType(String))           'INTIME             時間指定（入構）
        IO_TBL.Columns.Add("OUTTIME_TXT", GetType(String))          'OUTTIME            時間指定（出構）
        IO_TBL.Columns.Add("STAFFCODE_TXT", GetType(String))        'STAFFCODE          乗務員コード
        IO_TBL.Columns.Add("SUBSTAFFCODE_TXT", GetType(String))     'SUBSTAFFCODE       副乗務員コード
        IO_TBL.Columns.Add("STTIME_TXT", GetType(String))           'STTIME             出勤時間
        IO_TBL.Columns.Add("RYOME_TXT", GetType(String))            'RYOME              両目
        IO_TBL.Columns.Add("TODOKECODE_TXT", GetType(String))       'TODOKECODE         届先コード
        IO_TBL.Columns.Add("TODOKETIME_TXT", GetType(String))       'TODOKETIME         時間指定（配送）
        IO_TBL.Columns.Add("PRODUCT1_TXT", GetType(String))         'PRODUCT1           品名１
        IO_TBL.Columns.Add("PRODUCT2_TXT", GetType(String))         'PRODUCT2           品名２
        IO_TBL.Columns.Add("CONTNO_TXT", GetType(String))           'CONTNO             コンテナ番号
        IO_TBL.Columns.Add("PRATIO_TXT", GetType(String))           'PRATIO             Ｐ比率
        IO_TBL.Columns.Add("SMELLKBN_TXT", GetType(String))         'SMELLKBN           臭有無
        IO_TBL.Columns.Add("SHAFUKU_TXT", GetType(String))          'SHAFUKU            車腹（積載量）
        IO_TBL.Columns.Add("HTANI_TXT", GetType(String))            'HTANI              配送単位
        IO_TBL.Columns.Add("SURYO_TXT", GetType(String))            'SURYO              数量
        IO_TBL.Columns.Add("JSURYO_TXT", GetType(String))           'JSURYO             配送実績数量
        IO_TBL.Columns.Add("REMARKS1_TXT", GetType(String))         'REMARKS1           備考１
        IO_TBL.Columns.Add("REMARKS2_TXT", GetType(String))         'REMARKS2           備考２
        IO_TBL.Columns.Add("REMARKS3_TXT", GetType(String))         'REMARKS3           備考３
        IO_TBL.Columns.Add("REMARKS4_TXT", GetType(String))         'REMARKS4           備考４
        IO_TBL.Columns.Add("REMARKS5_TXT", GetType(String))         'REMARKS5           備考５
        IO_TBL.Columns.Add("REMARKS6_TXT", GetType(String))         'REMARKS6           備考６
        IO_TBL.Columns.Add("TORIORDERNO_TXT", GetType(String))      'TORIORDERNO        荷主受注番号
        IO_TBL.Columns.Add("STORICODE_TXT", GetType(String))        'STORICODE          請求取引先コード
        IO_TBL.Columns.Add("URIKBN_TXT", GetType(String))           'URIKBN             売上計上基準
        IO_TBL.Columns.Add("TERMORG_TXT", GetType(String))          'TERMORG            端末設置部署
        IO_TBL.Columns.Add("CONTCHASSIS_TXT", GetType(String))      'CONTCHASSIS        コンテナシャーシ
        IO_TBL.Columns.Add("TSHABANF_TXT", GetType(String))         'TSHABANF           統一車番(前)
        IO_TBL.Columns.Add("TSHABANB_TXT", GetType(String))         'TSHABANB           統一車番(後)
        IO_TBL.Columns.Add("TSHABANB2_TXT", GetType(String))        'TSHABANB2          統一車番(後)2
        IO_TBL.Columns.Add("TAXKBN_TXT", GetType(String))           'TAXKBN             税区分
        IO_TBL.Columns.Add("TUMIOKI_TXT", GetType(String))          'TUMIOKI            積置内容
        IO_TBL.Columns.Add("DELFLG_TXT", GetType(String))           'DELFLG             削除

        IO_TBL.Columns.Add("POSTNUM_TXT", GetType(String))          'POSTNUM_TXT        郵便番号
        IO_TBL.Columns.Add("ADDR1_TXT", GetType(String))            'ADDR1_TXT          住所１
        IO_TBL.Columns.Add("ADDR2_TXT", GetType(String))            'ADDR2_TXT          住所２
        IO_TBL.Columns.Add("ADDR3_TXT", GetType(String))            'ADDR3_TXT          住所３
        IO_TBL.Columns.Add("ADDR4_TXT", GetType(String))            'ADDR4_TXT          住所４
        IO_TBL.Columns.Add("DISTANCE_TXT", GetType(String))         'DISTANCE           配送距離
        IO_TBL.Columns.Add("ARRIVTIME_TXT", GetType(String))        'ARRIVTIME          所要時間
        IO_TBL.Columns.Add("NOTES1_TXT", GetType(String))           'NOTES01            届先特定要件０１
        IO_TBL.Columns.Add("NOTES2_TXT", GetType(String))           'NOTES02            届先特定要件０２
        IO_TBL.Columns.Add("NOTES3_TXT", GetType(String))           'NOTES03            届先特定要件０３
        IO_TBL.Columns.Add("NOTES4_TXT", GetType(String))           'NOTES04            届先特定要件０４
        IO_TBL.Columns.Add("NOTES5_TXT", GetType(String))           'NOTES05            届先特定要件０５
        IO_TBL.Columns.Add("NOTES6_TXT", GetType(String))           'NOTES06            届先特定要件０６
        IO_TBL.Columns.Add("NOTES7_TXT", GetType(String))           'NOTES07            届先特定要件０７
        IO_TBL.Columns.Add("NOTES8_TXT", GetType(String))           'NOTES08            届先特定要件０８
        IO_TBL.Columns.Add("NOTES9_TXT", GetType(String))           'NOTES09            届先特定要件０９
        IO_TBL.Columns.Add("NOTES10_TXT", GetType(String))          'NOTES10            届先特定要件１０
        IO_TBL.Columns.Add("STAFFNOTES1_TXT", GetType(String))      'STAFFNOTES1        乗務員特定要件１
        IO_TBL.Columns.Add("STAFFNOTES2_TXT", GetType(String))      'STAFFNOTES2        乗務員特定要件２
        IO_TBL.Columns.Add("STAFFNOTES3_TXT", GetType(String))      'STAFFNOTES3        乗務員特定要件３
        IO_TBL.Columns.Add("STAFFNOTES4_TXT", GetType(String))      'STAFFNOTES4        乗務員特定要件４
        IO_TBL.Columns.Add("STAFFNOTES5_TXT", GetType(String))      'STAFFNOTES5        乗務員特定要件５
        IO_TBL.Columns.Add("SHARYOINFO1_TXT", GetType(String))      'SHARYOINFO1        車両情報１
        IO_TBL.Columns.Add("SHARYOINFO2_TXT", GetType(String))      'SHARYOINFO2        車両情報２
        IO_TBL.Columns.Add("SHARYOINFO3_TXT", GetType(String))      'SHARYOINFO3        車両情報３
        IO_TBL.Columns.Add("SHARYOINFO4_TXT", GetType(String))      'SHARYOINFO4        車両情報４
        IO_TBL.Columns.Add("SHARYOINFO5_TXT", GetType(String))      'SHARYOINFO5        車両情報５
        IO_TBL.Columns.Add("SHARYOINFO6_TXT", GetType(String))      'SHARYOINFO6        車両情報６

        IO_TBL.Columns.Add("STAFFORGSEQ", GetType(String))          'STAFFORGSEQ        乗務員表示順番
        IO_TBL.Columns.Add("SUBSTAFFORGSEQ", GetType(String))       'SUBSTAFFORGSEQ     副乗務員表示順番
        IO_TBL.Columns.Add("SHABANORGSEQ", GetType(String))         'SHABANORGSEQ       業務車番表示順番
        IO_TBL.Columns.Add("TODKORGSEQ", GetType(String))           'TODKORGSEQ         届先表示順番
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
                Case "GSHABAN", "CONTCHASSIS"
                    '業務車番名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))   '業務車番名称
                Case "OILTYPE"
                    '油種名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "OILTYPE"))                    '油種名称
                Case "URIKBN"
                    '売上計上基準
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))                     '売上計上基準名称
                Case "STATUS"
                    '状態名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STATUS"))                     '状態名称
                Case "TUMIOKIKBN"
                    '積置区分名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN"))                 '積置区分名称
                Case "PROD1"
                    '品名１名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "PROD1"))                      '品名１名称
                Case "SMELLKBN"
                    '臭有無名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "SMELLKBN"))                   '臭有無名称
                Case "DELFLG"
                    '削除フラグ名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))                     '削除フラグ名称
                Case "HTANI"
                    '配送単位名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "HTANI"))                      '配送単位名称
                Case "TAXKBN"
                    '税区分名称
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))                     '税区分名称
            End Select
        End If
    End Sub

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0001S Then
            If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0001WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile() '条件画面からの画面遷移

        End If

    End Sub

End Class






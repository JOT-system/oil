Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' EXCEL書式登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0102XLSFORM
    Inherits Page

    '○ 検索結果格納Table
    Private CO0102tbl As DataTable                          '一覧格納用テーブル
    Private CO0102INPtbl As DataTable                       'チェック用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
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
                    If Not Master.RecoverTable(CO0102tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
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
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
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
            If Not IsNothing(CO0102tbl) Then
                CO0102tbl.Clear()
                CO0102tbl.Dispose()
                CO0102tbl = Nothing
            End If

            If Not IsNothing(CO0102INPtbl) Then
                CO0102INPtbl.Clear()
                CO0102INPtbl.Dispose()
                CO0102INPtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0102WRKINC.MAPID

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0102S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
            WF_PROFID.Text = Master.PROF_REPORT
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
        MAPDataGet()

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(CO0102tbl)

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
    ''' <remarks></remarks>
    Protected Sub MAPDataGet()

        CO0102tbl_ColumnsAdd()

        '○ 処理準備
        '○ Tempディレクトリ作成()
        Dim WW_DIR As String = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & ValueToCode("PROFID", Master.PROF_REPORT, WW_RTN_SW) & "\Temp"
        If Not Directory.Exists(WW_DIR) Then
            Directory.CreateDirectory(WW_DIR)
        End If

        '○ Tempディレクトリの不要データを掃除
        For Each TempFile As String In Directory.GetFiles(WW_DIR, "*", SearchOption.AllDirectories)
            'ファイルパスからファイル名を取得し削除
            File.Delete(TempFile)
        Next

        '○ Tempディレクトリ以下の不要ディレクトリを掃除
        '○ Tempディレクトリ配下の全てのディレクトリを取得
        Dim WW_DEL_DIR As String() = Directory.GetDirectories(WW_DIR, "*", SearchOption.AllDirectories)
        '○ 取得したディレクトリを降順にする(深い階層から削除するため)
        Array.Reverse(WW_DEL_DIR)

        Try
            For Each DeleteDir As String In WW_DEL_DIR
                If Directory.Exists(DeleteDir) Then
                    '削除するディレクトリ内の全ファイル取得
                    Dim WW_DEL_DIR_FILE As String() = Directory.GetFiles(DeleteDir, "*", SearchOption.AllDirectories)

                    'ディレクトリ配下にファイルが存在しない場合削除
                    If WW_DEL_DIR_FILE.Length = 0 Then
                        Directory.Delete(DeleteDir, True)
                    End If
                End If
            Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)

            CS0011LOGWrite.INFSUBCLASS = "Initialize"
            CS0011LOGWrite.INFPOSI = "Temp File Delete"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()
            Exit Sub
        End Try

        '○ 画面表示データ取得
        WW_DIR = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT"
        Dim WW_MAPLIST As ListBox = work.GetMAPIDData(work.WF_SEL_CAMPCODE.Text, Master.ROLE_MAP)

        Dim WW_TARGETID As String() = {}
        '機能選択に従い　DEFAULTを含めるか判断する
        If work.WF_SEL_FUNCSEL.Text = GRCO0102WRKINC.C_LIST_FUNSEL_DEFAULT.VISIBLE Then
            WW_TARGETID = {C_DEFAULT_DATAKEY, Master.PROF_REPORT}
        ElseIf work.WF_SEL_FUNCSEL.Text = GRCO0102WRKINC.C_LIST_FUNSEL_DEFAULT.INVISIBLE Then
            WW_TARGETID = {Master.PROF_REPORT}
        End If

        For Each ProfID As String In WW_TARGETID
            For Each MapID As ListItem In WW_MAPLIST.Items

                '条件画面での選択条件判定
                '画面ID(From)
                If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDF.Text) AndAlso
                    MapID.Value < work.WF_SEL_MAPIDF.Text Then
                    Continue For
                End If
                '画面ID(To)
                If Not String.IsNullOrEmpty(work.WF_SEL_MAPIDT.Text) AndAlso
                    MapID.Value > work.WF_SEL_MAPIDT.Text Then
                    Continue For
                End If

                'Excel書式フォルダー(...\プロフID\画面ID)内の全ファイルリスト取得
                Dim WW_SEARCH_DIR As String = WW_DIR & "\" & ValueToCode("PROFID", ProfID, WW_RTN_SW) & "\" & MapID.Value
                If Not Directory.Exists(WW_SEARCH_DIR) Then
                    Directory.CreateDirectory(WW_SEARCH_DIR)
                End If

                'ヘッダー分作成
                Dim CO0102row As DataRow = CO0102tbl.NewRow

                '固定項目
                CO0102row("LINECNT") = 0
                CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                CO0102row("SELECT") = 1
                CO0102row("HIDDEN") = 0

                '画面毎の項目設定
                CO0102row("TITLEKBN") = "H"
                CO0102row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                CO0102row("CAMPNAMES") = ""
                CO0102row("PROFID") = ProfID
                CO0102row("MAPID") = MapID.Value
                CO0102row("MAPNAMES") = MapID.Text
                CO0102row("SEQ") = 0
                CO0102row("FILENAME") = ""
                CO0102row("FILEPATH") = ""
                CO0102row("DELFLG") = C_DELETE_FLG.ALIVE

                '名称取得
                CODENAME_get("CAMPCODE", CO0102row("CAMPCODE"), CO0102row("CAMPNAMES"), WW_DUMMY)       '会社コード

                CO0102tbl.Rows.Add(CO0102row)

                'ファイルが存在する分明細作成
                For Each FileStr As String In Directory.GetFiles(WW_SEARCH_DIR, "*", SearchOption.AllDirectories)
                    'ファイル名取得
                    Dim WW_FILENAME As String = Mid(FileStr, InStrRev(FileStr, "\") + 1, Len(FileStr))

                    CO0102row = CO0102tbl.NewRow

                    '固定項目
                    CO0102row("LINECNT") = 0
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    CO0102row("SELECT") = 1
                    CO0102row("HIDDEN") = 0

                    '画面毎の項目設定
                    CO0102row("TITLEKBN") = "I"
                    CO0102row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    CO0102row("CAMPNAMES") = ""
                    CO0102row("PROFID") = ProfID
                    CO0102row("MAPID") = MapID.Value
                    CO0102row("MAPNAMES") = MapID.Text
                    CO0102row("SEQ") = 0
                    CO0102row("FILENAME") = WW_FILENAME
                    CO0102row("FILEPATH") = FileStr
                    CO0102row("DELFLG") = C_DELETE_FLG.ALIVE

                    '名称取得
                    CODENAME_get("CAMPCODE", CO0102row("CAMPCODE"), CO0102row("CAMPNAMES"), WW_DUMMY)       '会社コード

                    CO0102tbl.Rows.Add(CO0102row)
                Next
            Next

            'プロフIDがDefaultの場合抜ける
            If Master.PROF_REPORT = C_DEFAULT_DATAKEY Then
                Exit For
            End If
        Next

        '○ 明細項番採番
        CS0026TBLSORT.TABLE = CO0102tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, PROFID, MAPID, FILENAME"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(CO0102tbl)

        Dim WW_SAVEKEY As String = ""
        Dim WW_SEQ As Integer = 0
        For Each CO0102row As DataRow In CO0102tbl.Rows
            Dim WW_KEY As String = CO0102row("PROFID") & "," & CO0102row("MAPID")

            If CO0102row("TITLEKBN") = "H" Then
                Continue For
            End If

            If WW_SAVEKEY <> WW_KEY Then
                WW_SEQ = 0
                WW_SAVEKEY = WW_KEY
            End If

            WW_SEQ = WW_SEQ + 1
            CO0102row("SEQ") = WW_SEQ
        Next

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = CO0102tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H'"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0102tbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' テーブル作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0102tbl_ColumnsAdd()

        If IsNothing(CO0102tbl) Then
            CO0102tbl = New DataTable
        End If

        If CO0102tbl.Columns.Count <> 0 Then
            CO0102tbl.Columns.Clear()
        End If

        CO0102tbl.Clear()

        CO0102tbl.Columns.Add("LINECNT", GetType(Integer))
        CO0102tbl.Columns.Add("OPERATION", GetType(String))
        CO0102tbl.Columns.Add("SELECT", GetType(Integer))
        CO0102tbl.Columns.Add("HIDDEN", GetType(Integer))

        CO0102tbl.Columns.Add("TITLEKBN", GetType(String))
        CO0102tbl.Columns.Add("CAMPCODE", GetType(String))
        CO0102tbl.Columns.Add("CAMPNAMES", GetType(String))
        CO0102tbl.Columns.Add("PROFID", GetType(String))
        CO0102tbl.Columns.Add("MAPID", GetType(String))
        CO0102tbl.Columns.Add("MAPNAMES", GetType(String))
        CO0102tbl.Columns.Add("SEQ", GetType(Integer))
        CO0102tbl.Columns.Add("FILENAME", GetType(String))
        CO0102tbl.Columns.Add("FILEPATH", GetType(String))
        CO0102tbl.Columns.Add("DELFLG", GetType(String))

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        '   ※ 絞込 (Cell(4) : 0=表示対象, 1=非表示対象)
        For Each CO0102row As DataRow In CO0102tbl.Rows
            If CO0102row("HIDDEN") = 0 AndAlso CO0102row("TITLEKBN") = "H" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                CO0102row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(CO0102tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'" _
            & " and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELMAPID.Text)

        '○ 名称取得
        CODENAME_get("MAPID", WF_SELMAPID.Text, WF_SELMAPID_TEXT.Text, WW_DUMMY)

        '○ 絞り込み操作(GridView明細Hidden設定)
        For Each CO0102row As DataRow In CO0102tbl.Rows

            '一度非表示にする
            CO0102row("HIDDEN") = 1

            Dim WW_HANTEI As Boolean = True

            '画面IDによる絞込判定
            If WF_SELMAPID.Text <> "" AndAlso
                WF_SELMAPID.Text <> CO0102row("MAPID") Then
                WW_HANTEI = False
            End If

            '画面(GridView)のHIDDENに結果格納
            If WW_HANTEI Then
                CO0102row("HIDDEN") = 0
            End If
        Next

        '○ 画面先頭を表示
        WF_GridPosition.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_SELMAPID.Focus()

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()
        
        'ファイル作成
        CreateFileDir()

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

        '○ 詳細画面クリア
        DetailBoxClear()

        WF_SELMAPID.Focus()

    End Sub

    ''' <summary>
    ''' 書式フォーマット作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CreateFileDir()

        '○ ファイルを正式フォルダへ格納
        Dim WW_DELETE As Boolean = False
        For Each CO0102row As DataRow In CO0102tbl.Rows
            If Trim(CO0102row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                Trim(CO0102row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then

                If CO0102row("TITLEKBN") = "H" Then
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Continue For
                End If

                'Tempディレクトリ
                Dim WW_Dir_Temp As String = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & ValueToCode("PROFID", Master.PROF_REPORT, WW_RTN_SW) & "\Temp\" & CO0102row("PROFID") & "\" & CO0102row("MAPID")

                '正式ディレクトリ
                Dim WW_Dir As String = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & ValueToCode("PROFID", CO0102row("PROFID"), WW_RTN_SW) & "\" & CO0102row("MAPID")

                'FTP送信準備ディレクトリ
                Dim WW_Dir_Send As String = CS0050SESSION.UPLOAD_PATH & "\SEND\SENDSTOR\" & Master.USERTERMID & "\EXCEL\" & CO0102row("PROFID") & "\" & CO0102row("MAPID")

                Dim WW_Dir_Work As String = ""

                If CO0102row("DELFLG") = C_DELETE_FLG.DELETE Then
                    '削除処理
                    'Tempディレクトリ内該当ファイル削除
                    If InStr(CO0102row("FILEPATH"), "\Temp") > 0 AndAlso
                        File.Exists(WW_Dir_Temp & "\" & CO0102row("FILENAME")) Then
                        File.Delete(WW_Dir_Temp & "\" & CO0102row("FILENAME"))
                    End If

                    '正式ディレクトリ内該当ファイル削除
                    If File.Exists(WW_Dir & "\" & CO0102row("FILENAME")) Then
                        File.Delete(WW_Dir & "\" & CO0102row("FILENAME"))
                    End If

                    WW_DELETE = True
                Else
                    '更新処理
                    'プロフIDディレクトリ作成
                    WW_Dir_Work = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & ValueToCode("PROFID", CO0102row("PROFID"), WW_RTN_SW)
                    If Not Directory.Exists(WW_Dir_Work) Then
                        Directory.CreateDirectory(WW_Dir_Work)
                    End If

                    '(プロフID\)画面IDディレクトリ作成
                    WW_Dir_Work = WW_Dir_Work & "\" & CO0102row("MAPID")
                    If Not Directory.Exists(WW_Dir_Work) Then
                        Directory.CreateDirectory(WW_Dir_Work)
                    End If

                    'Tempディレクトリから正式ディレクトリへファイルを移動
                    If File.Exists(WW_Dir_Temp & "\" & CO0102row("FILENAME")) Then
                        '正式ディレクトリ内の古い該当ファイルを削除
                        If File.Exists(WW_Dir & "\" & CO0102row("FILENAME")) Then
                            File.Delete(WW_Dir & "\" & CO0102row("FILENAME"))
                        End If

                        '該当ファイル移動
                        File.Move(WW_Dir_Temp & "\" & CO0102row("FILENAME"), WW_Dir & "\" & CO0102row("FILENAME"))
                    End If
                End If

                'FTP送信準備ディレクトリにコピー(既に存在しているなら先に削除する)
                If Directory.Exists(WW_Dir_Send) Then
                    My.Computer.FileSystem.DeleteDirectory(WW_Dir_Send, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
                End If
                My.Computer.FileSystem.CopyDirectory(WW_Dir, WW_Dir_Send)

                CO0102row("FILEPATH") = WW_Dir & "\" & CO0102row("FILENAME")

                CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If
        Next

        '○ 削除したファイルをテーブルからも消す
        If WW_DELETE Then
            Dim i As Integer = 0
            Do Until i > (CO0102tbl.Rows.Count - 1)
                If CO0102tbl.Rows(i)("TITLEKBN") = "I" AndAlso
                    CO0102tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                    CO0102tbl.Rows(i).Delete()
                Else
                    i = i + 1
                End If
            Loop

            '○ 明細項番採番
            CS0026TBLSORT.TABLE = CO0102tbl
            CS0026TBLSORT.SORTING = "CAMPCODE, PROFID, MAPID, FILENAME"
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.sort(CO0102tbl)

            Dim WW_SAVEKEY As String = ""
            Dim WW_SEQ As Integer = 0
            For Each CO0102row As DataRow In CO0102tbl.Rows
                Dim WW_KEY As String = CO0102row("PROFID") & "," & CO0102row("MAPID")

                If CO0102row("TITLEKBN") = "H" Then
                    Continue For
                End If

                If WW_SAVEKEY <> WW_KEY Then
                    WW_SEQ = 0
                    WW_SAVEKEY = WW_KEY
                End If

                WW_SEQ = WW_SEQ + 1
                CO0102row("SEQ") = WW_SEQ
            Next
        End If

        '○ メッセージ表示
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
        CS0030REPORT.TBLDATA = CO0102tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = CO0102tbl                        'データ参照Table
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
        WF_SELMAPID.Focus()

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(CO0102tbl)
        TBLview.RowFilter = "HIDDEN = 0 and TITLEKBN = 'H'"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        WF_SELMAPID.Focus()

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
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To CO0102tbl.Rows.Count - 1
            If CO0102tbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        '選択行
        WF_Sel_LINECNT.Text = CO0102tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        WF_CAMPCODE.Text = CO0102tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'プロフID
        WF_PROFID.Text = CO0102tbl.Rows(WW_LINECNT)("PROFID")

        '画面ID
        WF_MAPID.Text = CO0102tbl.Rows(WW_LINECNT)("MAPID")
        CODENAME_get("MAPID", WF_MAPID.Text, WF_MAPID_TEXT.Text, WW_DUMMY)

        Dim EXCELtbl As New DataTable
        CS0026TBLSORT.TABLE = CO0102tbl
        CS0026TBLSORT.SORTING = "SEQ"
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and PROFID = '" & WF_PROFID.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'" _
            & " and TITLEKBN = 'I'"
        CS0026TBLSORT.sort(EXCELtbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = EXCELtbl
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            Dim EXCELrow As DataRow = EXCELtbl.Rows(i)

            'ファイルパス
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = EXCELrow("FILEPATH")

            '項番
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SEQ"), Label).Text = EXCELrow("SEQ")

            'ファイル記号名称
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = EXCELrow("FILENAME")

            '削除
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = EXCELrow("DELFLG")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", "REF_Field_DBclick('" & i & "', 'WF_Rep_DELFLG', '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "')")
        Next

        EXCELtbl.Clear()
        EXCELtbl.Dispose()
        EXCELtbl = Nothing

        '○ 状態をクリア
        For Each CO0102row As DataRow In CO0102tbl.Rows
            Select Case CO0102row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case CO0102tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0102tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0102tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0102tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0102tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0102tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

        WF_MAPID.Focus()
        WF_GridDBclick.Text = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

        WF_SELMAPID.Focus()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks>アップしたファイルを書式フォーマットとして保存</remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ 詳細画面データ取得
        DetailBoxToCO0102INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ ファイル操作
        '   入力 :アップロードフォルダ(...\UPLOAD_TMP\ユーザーID\ファイル.xxx)
        '   出力 :プロフIDTempフォルダ(...\PRINTFORMAT\プロフID\Temp\ファイル.xxx)

        '○ アップロードファイルをTempへ格納
        Dim WW_TEMPFILE_PATH As String = ""

        '○ Tempディレクトリ作成
        WW_TEMPFILE_PATH = CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & ValueToCode("PROFID", Master.PROF_REPORT, WW_RTN_SW) & "\Temp"
        If Not Directory.Exists(WW_TEMPFILE_PATH) Then
            Directory.CreateDirectory(WW_TEMPFILE_PATH)
        End If

        '○ Temp\画面プロフIDのディレクトリ作成
        WW_TEMPFILE_PATH = WW_TEMPFILE_PATH & "\" & WF_PROFID.Text
        If Not Directory.Exists(WW_TEMPFILE_PATH) Then
            Directory.CreateDirectory(WW_TEMPFILE_PATH)
        End If

        '○ Temp\画面プロフID\画面MAPIDのディレクトリ作成
        WW_TEMPFILE_PATH = WW_TEMPFILE_PATH & "\" & WF_MAPID.Text
        If Not Directory.Exists(WW_TEMPFILE_PATH) Then
            Directory.CreateDirectory(WW_TEMPFILE_PATH)
        End If

        '○ アップロードFILE取得
        Dim WW_Dirs As String() = Nothing
        Try
            WW_Dirs = Directory.GetFiles(CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID, "*.*")

            If WW_Dirs.Length = 0 Then
                Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "ファイル")

                CS0011LOGWrite.INFSUBCLASS = "FILE UPLOAD"
                CS0011LOGWrite.INFPOSI = "File Read"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWrite.TEXT = "システム管理者へ連絡して下さい"
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
                Exit Sub
            End If
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ABORT, "ファイル")

            CS0011LOGWrite.INFSUBCLASS = "FILE UPLOAD"
            CS0011LOGWrite.INFPOSI = "File Read"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
            CS0011LOGWrite.CS0011LOGWrite()
            Exit Sub
        End Try

        For Each WW_Dir As String In WW_Dirs
            '○ ファイル名取得
            Dim WW_FILENAME As String = Mid(WW_Dir, InStrRev(WW_Dir, "\") + 1, Len(WW_Dir))

            '○ アップロードしたファイルをTempフォルダへ移動
            If File.Exists(WW_TEMPFILE_PATH & "\" & WW_FILENAME) Then
                '古いファイルが存在する場合削除
                File.Delete(WW_TEMPFILE_PATH & "\" & WW_FILENAME)
            End If

            File.Move(WW_Dir, WW_TEMPFILE_PATH & "\" & WW_FILENAME)

            '○ アップロードファイルの画面反映
            Dim WW_EXISTS As Boolean = False
            For Each CO0102INProw As DataRow In CO0102INPtbl.Rows
                If CO0102INProw("TITLEKBN") = "H" Then
                    Continue For
                End If

                If CO0102INProw("FILENAME") <> WW_FILENAME Then
                    Continue For
                End If

                '同名のファイルが存在する場合上書き
                CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0102INProw("FILEPATH") = WW_TEMPFILE_PATH & "\" & WW_FILENAME
                CO0102INProw("DELFLG") = C_DELETE_FLG.ALIVE

                WW_EXISTS = True
            Next

            '○ 同名のファイルが存在しない場合、明細行追加
            If Not WW_EXISTS Then
                Dim CO0102INProw As DataRow = CO0102INPtbl.NewRow

                CO0102INProw("LINECNT") = 0
                CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0102INProw("SELECT") = 1
                CO0102INProw("HIDDEN") = 0

                CO0102INProw("TITLEKBN") = "I"
                CO0102INProw("CAMPCODE") = WF_CAMPCODE.Text
                CO0102INProw("CAMPNAMES") = ""
                CO0102INProw("PROFID") = WF_PROFID.Text
                CO0102INProw("MAPID") = WF_MAPID.Text
                CO0102INProw("MAPNAMES") = ""
                CO0102INProw("SEQ") = CO0102INPtbl.Rows.Count
                CO0102INProw("FILENAME") = WW_FILENAME
                CO0102INProw("FILEPATH") = WW_TEMPFILE_PATH & "\" & WW_FILENAME
                CO0102INProw("DELFLG") = C_DELETE_FLG.ALIVE

                '名称取得
                CODENAME_get("CAMPCODE", CO0102INProw("CAMPCODE"), CO0102INProw("CAMPNAMES"), WW_DUMMY)         '会社コード
                CODENAME_get("MAPID", CO0102INProw("MAPID"), CO0102INProw("MAPNAMES"), WW_DUMMY)                '画面ID

                CO0102INPtbl.Rows.Add(CO0102INProw)
            End If
        Next

        Dim EXCELtbl As New DataTable
        CS0026TBLSORT.TABLE = CO0102INPtbl
        CS0026TBLSORT.SORTING = "SEQ"
        CS0026TBLSORT.FILTER = "CAMPCODE = '" & WF_CAMPCODE.Text & "'" _
            & " and PROFID = '" & WF_PROFID.Text & "'" _
            & " and MAPID = '" & WF_MAPID.Text & "'" _
            & " and TITLEKBN = 'I'"
        CS0026TBLSORT.sort(EXCELtbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = EXCELtbl
        WF_Repeater.DataBind()

        '○ 明細作成
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            Dim EXCELrow As DataRow = EXCELtbl.Rows(i)

            'ファイルパス
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = EXCELrow("FILEPATH")

            '項番
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SEQ"), Label).Text = EXCELrow("SEQ")

            'ファイル記号名称
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = EXCELrow("FILENAME")

            '削除
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = EXCELrow("DELFLG")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", "REF_Field_DBclick('" & i & "', 'WF_Rep_DELFLG', '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "')")
        Next

        EXCELtbl.Clear()
        EXCELtbl.Dispose()
        EXCELtbl = Nothing

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_MAPID.Focus()

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
        DetailBoxToCO0102INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            CO0102tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

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

        WF_SELMAPID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToCO0102INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_PROFID.Text)            'プロフID
        Master.eraseCharToIgnore(WF_MAPID.Text)             '画面ID

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_MAPID.Text) Then
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

        '○ プロフID = 'Default'は禁止
        If WF_PROFID.Text = C_DEFAULT_DATAKEY Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            WW_CheckERR("プロフID='Default'は更新出来ません。", "")
        End If

        '名称取得
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)         '会社コード
        CODENAME_get("MAPID", WF_MAPID.Text, WF_MAPID_TEXT.Text, WW_DUMMY)                  '画面ID

        Master.CreateEmptyTable(CO0102INPtbl)

        Dim CO0102INProw As DataRow = CO0102INPtbl.NewRow

        CO0102INProw("LINECNT") = 0
        CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        CO0102INProw("SELECT") = 1
        CO0102INProw("HIDDEN") = 0

        CO0102INProw("TITLEKBN") = "H"
        CO0102INProw("CAMPCODE") = WF_CAMPCODE.Text
        CO0102INProw("CAMPNAMES") = ""
        CO0102INProw("PROFID") = WF_PROFID.Text
        CO0102INProw("MAPID") = WF_MAPID.Text
        CO0102INProw("MAPNAMES") = ""
        CO0102INProw("SEQ") = 0
        CO0102INProw("FILENAME") = ""
        CO0102INProw("FILEPATH") = ""
        CO0102INProw("DELFLG") = C_DELETE_FLG.ALIVE

        '名称取得
        CODENAME_get("CAMPCODE", CO0102INProw("CAMPCODE"), CO0102INProw("CAMPNAMES"), WW_DUMMY)         '会社コード
        CODENAME_get("MAPID", CO0102INProw("MAPID"), CO0102INProw("MAPNAMES"), WW_DUMMY)                '画面ID

        CO0102INPtbl.Rows.Add(CO0102INProw)


        '○ 画面情報をテーブルに反映
        For Each reitem As RepeaterItem In WF_Repeater.Items
            '画面(Repeater)の使用禁止文字排除
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_SEQ"), Label).Text)
            Master.eraseCharToIgnore(CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text)

            CO0102INProw = CO0102INPtbl.NewRow

            CO0102INProw("LINECNT") = 0
            CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            CO0102INProw("SELECT") = 1
            CO0102INProw("HIDDEN") = 0

            CO0102INProw("TITLEKBN") = "I"
            CO0102INProw("CAMPCODE") = WF_CAMPCODE.Text
            CO0102INProw("CAMPNAMES") = ""
            CO0102INProw("PROFID") = WF_PROFID.Text
            CO0102INProw("MAPID") = WF_MAPID.Text
            CO0102INProw("MAPNAMES") = ""
            CO0102INProw("SEQ") = 0
            Try
                Integer.TryParse(CType(reitem.FindControl("WF_Rep_SEQ"), Label).Text, CO0102INProw("SEQ"))
            Catch ex As Exception
                CO0102INProw("SEQ") = 0
            End Try
            CO0102INProw("FILENAME") = CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text
            CO0102INProw("FILEPATH") = CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text
            CO0102INProw("DELFLG") = CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text

            '名称取得
            CODENAME_get("CAMPCODE", CO0102INProw("CAMPCODE"), CO0102INProw("CAMPNAMES"), WW_DUMMY)         '会社コード
            CODENAME_get("MAPID", CO0102INProw("MAPID"), CO0102INProw("MAPNAMES"), WW_DUMMY)                '画面ID

            CO0102INPtbl.Rows.Add(CO0102INProw)
        Next

    End Sub

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

        '○ ヘッダー単項目チェック
        For Each CO0102INProw As DataRow In CO0102INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0102INProw("TITLEKBN") <> "H" Then
                Continue For
            End If

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", CO0102INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", CO0102INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'プロフID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "PROFID", CO0102INProw("PROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(プロフIDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If CO0102INProw("PROFID") = C_DEFAULT_DATAKEY Then
                WW_CheckMES1 = "・更新できないレコード(プロフID='Default')です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面ID
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MAPID", CO0102INProw("MAPID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("MAPID", CO0102INProw("MAPID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If CO0102INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

            Exit For
        Next

        '○ 明細単項目チェック
        For Each CO0102INProw As DataRow In CO0102INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0102INProw("TITLEKBN") = "H" Then
                Continue For
            End If

            'ファイル記号名称
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FILENAME", CO0102INProw("FILENAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ファイル記号名称)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除フラグ
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", CO0102INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0102INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, CO0102INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If CO0102INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="CO0102row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal CO0102row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(CO0102row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード       =" & CO0102row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プロフＩＤ       =" & CO0102row("PROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面ＩＤ         =" & CO0102row("MAPID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ファイル記号名称 =" & CO0102row("FILENAME")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub
    ''' <summary>
    ''' CO0102tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0102tbl_UPD()

        '○ 画面状態設定
        For Each CO0102row As DataRow In CO0102tbl.Rows
            Select Case CO0102row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        Dim WW_UPDAT As Integer = 0
        For Each CO0102INProw As DataRow In CO0102INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0102INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0102INProw("OPERATION") = "Insert"
            WW_UPDAT = WW_UPDAT + 1

            'KEY項目が等しい
            For Each CO0102row As DataRow In CO0102tbl.Rows
                If CO0102row("CAMPCODE") = CO0102INProw("CAMPCODE") AndAlso
                    CO0102row("PROFID") = CO0102INProw("PROFID") AndAlso
                    CO0102row("MAPID") = CO0102INProw("MAPID") AndAlso
                    CO0102row("TITLEKBN") = CO0102INProw("TITLEKBN") AndAlso
                    CO0102row("SEQ") = CO0102INProw("SEQ") Then

                    '変更無は操作無
                    If CO0102row("FILENAME") = CO0102INProw("FILENAME") AndAlso
                        CO0102row("FILEPATH") = CO0102INProw("FILEPATH") AndAlso
                        CO0102row("DELFLG") = CO0102INProw("DELFLG") Then
                        CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        WW_UPDAT = WW_UPDAT - 1
                        Exit For
                    End If

                    CO0102INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更レコードが存在する場合、ヘッダー区分も更新対象にする
        If WW_UPDAT > 0 Then
            For Each CO0102INProw As DataRow In CO0102INPtbl.Rows
                If CO0102INProw("TITLEKBN") = "H" Then
                    CO0102INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        End If

        '○ 変更有無判定　&　入力値反映
        For Each CO0102INProw As DataRow In CO0102INPtbl.Rows
            Select Case CO0102INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0102INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0102INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0102INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef CO0102INProw As DataRow)

        For Each CO0102row As DataRow In CO0102tbl.Rows
            '同一(ENDYMD以外が同一KEY)レコード
            If CO0102row("CAMPCODE") = CO0102INProw("CAMPCODE") AndAlso
                CO0102row("PROFID") = CO0102INProw("PROFID") AndAlso
                CO0102row("MAPID") = CO0102INProw("MAPID") AndAlso
                CO0102row("TITLEKBN") = CO0102INProw("TITLEKBN") AndAlso
                CO0102row("SEQ") = CO0102INProw("SEQ") Then

                '画面入力テーブル項目設定
                CO0102INProw("LINECNT") = CO0102row("LINECNT")
                CO0102INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0102INProw("SELECT") = 1
                CO0102INProw("HIDDEN") = 0

                '項目テーブル項目設定
                CO0102row.ItemArray = CO0102INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0102INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef CO0102INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0102row As DataRow = CO0102tbl.NewRow
        CO0102row.ItemArray = CO0102INProw.ItemArray

        '○ 最大項番数を取得
        Dim TBLview As DataView = New DataView(CO0102tbl)
        TBLview.RowFilter = "TITLEKBN = 'H'"

        If CO0102INProw("TITLEKBN") = "H" Then
            CO0102row("LINECNT") = TBLview.Count + 1
        Else
            CO0102row("LINECNT") = 0
        End If

        CO0102row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0102row("SELECT") = 1
        CO0102row("HIDDEN") = 0

        CO0102tbl.Rows.Add(CO0102row)

        TBLview.Dispose()
        TBLview = Nothing

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

        WF_MAPID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each CO0102row As DataRow In CO0102tbl.Rows
            Select Case CO0102row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0102row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(CO0102tbl)

        WF_Sel_LINECNT.Text = ""                            'LINECNT
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_PROFID.Text = Master.PROF_REPORT                 'プロファイルID
        WF_MAPID.Text = ""                                  '画面ID
        WF_MAPID_TEXT.Text = ""                             '画面名称

        '○ 詳細画面初期設定
        DetailInitialize()

    End Sub

    ''' <summary>
    ''' 詳細画面-初期設定 (空明細作成 ＆ イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailInitialize()

        Master.CreateEmptyTable(CO0102INPtbl)

        '○ 明細へデータ貼り付け
        WF_Repeater.Visible = False
        WF_Repeater.DataSource = CO0102INPtbl
        WF_Repeater.DataBind()

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                'フィールドによってパラメーターを変える
                Select Case WF_FIELD.Value
                    Case "WF_SELMAPID", "WF_MAPID"          '画面ID
                        prmData = work.CreateMAPIDParam(work.WF_SEL_CAMPCODE.Text, Master.ROLE_MAP)
                End Select

                .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .activeListBox()
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_SELMAPID"          '画面ID
                WF_SELMAPID.Text = WW_SelectValue
                WF_SELMAPID_TEXT.Text = WW_SelectText
                WF_SELMAPID.Focus()

            Case "WF_MAPID"             '画面ID
                WF_MAPID.Text = WW_SelectValue
                WF_MAPID_TEXT.Text = WW_SelectText
                WF_MAPID.Focus()

            Case "WF_Rep_DELFLG"        '削除
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_DELFLG"), TextBox).Text = WW_SelectValue
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_DELFLG"), TextBox).Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_SELMAPID"          '画面ID
                WF_SELMAPID.Focus()
            Case "WF_MAPID"             '画面ID
                WF_MAPID.Focus()
            Case "WF_Rep_DELFLG"        '削除
                CType(WF_Repeater.Items(WF_FIELD_REP.Value).FindControl("WF_Rep_DELFLG"), TextBox).Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""

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
                Case "MAPID"            '画面ID
                    prmData = work.CreateMAPIDParam(work.WF_SEL_CAMPCODE.Text, Master.ROLE_MAP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    Protected Function ValueToCode(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_RTN As String) As String
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim GS0032 As New GS0032FIXVALUElst
        GS0032.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0032.STDATE = Date.Now
        GS0032.ENDDATE = Date.Now
        Select Case I_FIELD
            Case "PROFID"           'プロフィールID
                GS0032.CLAS = "CO0004_RPRTPROFID"
        End Select
        GS0032.GS0032FIXVALUElst()
        O_RTN = GS0032.ERR

        If isNormal(O_RTN) Then
            Return If(IsNothing(GS0032.VALUE1.Items.FindByText(I_VALUE)), C_DEFAULT_DATAKEY, GS0032.VALUE1.Items.FindByText(I_VALUE).Value)
        End If
        Return String.Empty
    End Function
End Class

Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports OFFICE.GRT00004COM

''' <summary>
''' 配送受注（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00004HORDER
    Inherits System.Web.UI.Page

    Private T00004ds As DataSet                                     '格納ＤＳ
    Private T00004tbl As DataTable                                  'Grid格納用テーブル
    Private T00004INPtbl As DataTable                               'Detail入力用テーブル
    Private T00004UPDtbl As DataTable                               '更新時作業テーブル
    Private T00004SUMtbl As DataTable                               '更新時作業テーブル
    Private T00004WKtbl As DataTable                                '更新時作業テーブル
    Private T00003tbl As DataTable                                  'T3更新作業テーブル

    Private S0013tbl As DataTable                                   'データフィールド

    Private TODOKESAKItbl As Dictionary(Of String, TODOKESAKI)      '届先追加情報テーブル
    Private PRODUCTtbl As Dictionary(Of String, PRODUCT)            '品名追加情報テーブル
    Private STAFFtbl As Dictionary(Of String, STAFF)                '従業員追加情報テーブル

    Private KOUEIMNG As GRW0001KOUEIORDER                           '光英データ管理
    Private KOUEIMASTER As KOUEI_MASTER                             '光英マスターデータ管理

    Private MC006UPDATE As GRT00004COM.GRMC006UPDATE                '届先更新

    '共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    Private CS0010CHARstr As New CS0010CHARget                      '例外文字排除 String Ge
    Private CS0020JOURNAL As New CS0020JOURNAL                      'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作
    Private CS0052DetailView As New CS0052DetailView                'Repeterオブジェクト作成
    Private CS0033AutoNumber As New CS0033AutoNumber                '受注番号取得
    Private CS0026TBLSORTget As New CS0026TBLSORT                   'GridView用テーブルソート文字列取得
    Private GS0029T3CNTLget As New GS0029T3CNTLget                  '荷主受注集計制御マスタ取得

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                     'リターンコード
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    Private WW_ERRLISTCNT As Integer                                'エラーリスト件数               
    Private WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Private WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROW_MAX As Integer = 65000
    Private Const CONST_DSPROWCOUNT As Integer = 40                 '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 20              'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '詳細部タブID

    Private Const C_UPLOAD_EXCEL_REPORTID_NJS As String = "NJS配車表"
    Private Const C_UPLOAD_EXCEL_PROFID_NJS As String = "Default"

    ''' <summary>
    ''' 配車ステータス
    ''' </summary>
    Private Class T4STATUS
        Public Const INIT As String = ""                            '初期値
        Public Const ORDER As String = "1"                          '受注予約
        Public Const MATCHING As String = "2"                       '配車
        Public Const MANNING As String = "2"                        '配乗
        Public Const RESULT As String = "3"                         '実績
        Public Const CANCEL As String = "9"                         '受注キャンセル
    End Class
    Private Const JXORDER_WARNING As String = "W"                   'JXオーダー警告あり

    ''' <summary>
    ''' 車輛タイプ
    ''' </summary>
    Private Class SYARYOTYPE
        Public Const SHARYO_CHAASSIS As String = "A"
        Public Const SHARYO_TANK As String = "B"
        Public Const SHARYO_TRACTOR As String = "C"
        Public Const SHARYO_TRAILER As String = "D"

        Public Const SHARYO_CHAASSIS_YO As String = "E"
        Public Const SHARYO_TANK_YO As String = "F"
        Public Const SHARYO_TRACTOR_YO As String = "G"
        Public Const SHARYO_TRAILER_YO As String = "H"

        ''' <summary>
        ''' 車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly SHARYO_LIST As String() = {"A", "B", "C", "D", "E", "F", "G", "H"}

        ''' <summary>
        ''' 車検切れチェック対象車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly INSPECTION_LIST As String() = {"A", "C", "D", "E", "G", "H"}
        ''' <summary>
        ''' 容器検査切れチェック対象車輛タイプリスト
        ''' </summary>
        Public Shared ReadOnly TANK_LIST As String() = {"B", "D", "F", "H"}
    End Class


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            '■■■ 作業用データベース設定 ■■■
            T00004ds = New DataSet()                                      '初期化
            T00004tbl = T00004ds.Tables.Add("T00004TBL")
            T00004INPtbl = T00004ds.Tables.Add("T00004INPTBL")
            T00004UPDtbl = T00004ds.Tables.Add("T00004UPDtbl")
            T00004SUMtbl = T00004ds.Tables.Add("T00004SUMtbl")
            T00004WKtbl = T00004ds.Tables.Add("T00004WKtbl")
            T00003tbl = T00004ds.Tables.Add("T00003TBL")
            T00004ds.EnforceConstraints = False

            If IsPostBack Then
                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value

                        '********* ヘッダ部 *********
                        Case "WF_ButtonGet"                     '光英受信
                            WF_ButtonGet_Click()
                        Case "WF_ButtonSAVE"                    '一時保存
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonExtract"                 '絞り込み
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonNEW"                     '新規
                            WF_ButtonNEW_Click()
                        Case "WF_ButtonUPDATE"                  'DB更新
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"                     'ﾀﾞｳﾝﾛｰﾄﾞ
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonPrint"                   '一覧印刷
                            WF_Print_Click()
                        Case "WF_ButtonFIRST"                   '先頭頁[image]
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"                    '最終頁[image]
                            WF_ButtonLAST_Click()
                        Case "WF_ButtonEND"                     '終了
                            WF_ButtonEND_Click()

                            '********* 一覧 *********
                        Case "WF_GridDBclick"                   'DBClick
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"                'MouseDown
                            WF_GRID_Scrole()
                        Case "WF_MouseWheelUp"                  'MouseUp
                            WF_GRID_Scrole()
                        Case "WF_UPLOAD_EXCEL"                  'EXCEL_UPLOAD
                            UPLOAD_EXCEL()
                        Case "WF_UPLOAD_KOUEI"                  'CSV_UPLOAD(光英)
                            UPLOAD_KOUEI()

                            '********* 詳細部 *********
                        Case "WF_UPDATE"                        '表更新
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                         'クリア
                            WF_CLEAR_Click()
                        Case "WF_BACK"                          '戻る
                            WF_BACK_Click()

                            '********* 入力フィールド *********
                        Case "WF_Field_DBClick"                 '項目DbClick
                            WF_Field_DBClick()

                            '********* 左BOX *********
                        Case "WF_ButtonSel"                     '選択
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                     'キャンセル
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"                '値選択DbClick
                            WF_Listbox_DBClick()

                            '********* 右BOX *********
                        Case "WF_RadioButonClick"               '選択時
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"                    'メモ欄変更時
                            WF_MEMO_Change()

                            '********* その他はMasterPageで処理 *********
                        Case Else
                    End Select
                    '○一覧再表示処理
                    DisplayGrid()

                End If
            Else
                '〇初期化処理
                Initialize()
            End If

        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
        Catch ex As Exception
            '○一覧再表示処理
            DisplayGrid()
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR)
        Finally

            '○Close
            If Not IsNothing(T00004ds) Then
                For Each tbl In T00004ds.Tables
                    tbl.Dispose()
                    tbl = Nothing
                Next
                T00004ds.Dispose()
                T00004ds = Nothing
            End If

            If Not IsNothing(S0013tbl) Then
                S0013tbl.Dispose()
                S0013tbl = Nothing
            End If

            If Not IsNothing(TODOKESAKItbl) Then
                TODOKESAKItbl.Clear()
                TODOKESAKItbl = Nothing
            End If
            If Not IsNothing(PRODUCTtbl) Then
                PRODUCTtbl.Clear()
                PRODUCTtbl = Nothing
            End If
            If Not IsNothing(STAFFtbl) Then
                STAFFtbl.Clear()
                STAFFtbl = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRT00004WRKINC.MAPID
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_SELTORICODE.Focus()

        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = True

        '光英読込中ファイル一覧クリア
        WF_KoueiLoadFile.Items.Clear()

        '左Boxへの値設定
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '右Boxへの値設定
        rightview.resetindex()
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        rightview.selectIndex(GRIS0004RightBox.RIGHT_TAB_INDEX.LS_ERROR_LIST)

        '〇画面モード（更新・参照）設定 
        If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
            WF_MAPpermitcode.Value = "TRUE"
        Else
            WF_MAPpermitcode.Value = "FALSE"
        End If


        '部署未指定時
        If String.IsNullOrEmpty(work.WF_SEL_ORDERORG.Text) AndAlso String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            'ログインユーザ所属部署の管轄支店部署を設定
            WF_DEFORG.Text = Master.USER_ORG
        ElseIf Not String.IsNullOrEmpty(work.WF_SEL_ORDERORG.Text) AndAlso String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            WF_DEFORG.Text = work.WF_SEL_ORDERORG.Text
        ElseIf String.IsNullOrEmpty(work.WF_SEL_ORDERORG.Text) AndAlso Not String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
            WF_DEFORG.Text = work.WF_SEL_SHIPORG.Text
        Else
            WF_DEFORG.Text = work.WF_SEL_SHIPORG.Text
        End If

        '○業務車番設定
        InitGSHABAN()
        '○コンテナシャーシ設定
        InitCONTCHASSIS()

        '■■■ 画面（GridView）表示項目取得 ■■■
        If work.WF_SEL_RESTART.Text = "RESTART" Then
            '○画面表示データ復元
            Master.RecoverTable(T00004tbl, work.WF_SEL_XMLsaveTmp.Text)

            '光英読込中ファイルリスト復元
            Dim files = work.WF_SEL_KOUEILOADFILE.Text.Split(C_VALUE_SPLIT_DELIMITER)
            For Each file In files.Where(Function(x) Not String.IsNullOrEmpty(x))
                WF_KoueiLoadFile.Items.Add(New ListItem(file))
            Next
        Else
            '○画面表示データ取得
            GRID_INITset()

            '○数量、台数合計の設定
            SUMMRY_SET()
        End If

        '光英受信ボタン非表示設定
        If (Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) AndAlso work.WF_SEL_OILTYPE.Text <> GRT00004WRKINC.C_PRODUCT_OIL) Then
            '油種条件が01(石油)以外は、非表示
            WF_IsHideKoueiButton.Value = "1"
        Else
            Dim T5Com = New GRT0005COM
            If Not T5Com.IsKoueiAvailableOrg(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, GRT00004WRKINC.C_KOUEI_CLASS_CODE, WW_ERRCODE) Then
                'FIXVALUE(T00004_KOUEIORG)が定義されていない場合は、非表示
                WF_IsHideKoueiButton.Value = "1"
            End If
            T5Com = Nothing
        End If

        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)

        '○一覧再表示処理
        DisplayGrid()

        '詳細非表示 
        WF_IsHideDetailBox.Value = "1"

        'テンポラリファイルの削除
        If File.Exists(work.WF_SEL_XMLsaveTmp.Text) Then
            File.Delete(work.WF_SEL_XMLsaveTmp.Text)
        End If
        If File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
            File.Delete(work.WF_SEL_XMLsavePARM.Text)
        End If


    End Sub

    ''' <summary>
    ''' GridView用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00004）を検索し画面表示する一覧を作成する</remarks>
    Private Sub GRID_INITset()

        '○画面表示データ取得
        DBselect_T4SELECT()

        '○ソート
        'ソート文字列取得
        CS0026TBLSORTget.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORTget.MAPID = Master.MAPID
        CS0026TBLSORTget.PROFID = Master.PROF_VIEW
        CS0026TBLSORTget.VARI = Master.MAPvariant
        CS0026TBLSORTget.TAB = ""
        CS0026TBLSORTget.getSorting()

        'ソート＆データ抽出
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = CS0026TBLSORTget.SORTING
        CS0026TBLSORTget.FILTER = "SELECT = 1"
        CS0026TBLSORTget.sort(T00004tbl)

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For i As Integer = 0 To T00004tbl.Rows.Count - 1

            Dim T00004row = T00004tbl.Rows(i)

            If T00004row("LINECNT") = 0 Then

                WW_LINECNT = WW_LINECNT + 1
                WW_SEQ = 0

                For j As Integer = i To T00004tbl.Rows.Count - 1

                    If T00004tbl.Rows(j)("LINECNT") = 0 Then
                        If CompareOrder(T00004row, T00004tbl.Rows(j)) Then

                            WW_SEQ = WW_SEQ + 1
                            T00004tbl.Rows(j)("LINECNT") = WW_LINECNT
                            T00004tbl.Rows(j)("SEQ") = WW_SEQ.ToString("00")

                            If WW_SEQ = 1 Then
                                T00004tbl.Rows(j)("HIDDEN") = 0
                            Else
                                '枝番データは非表示
                                T00004tbl.Rows(j)("HIDDEN") = 1
                            End If
                        Else
                            'Exit For    …　ソート定義に依存するのでExitできない
                        End If

                    End If
                Next

            End If

        Next

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        If T00004tbl.Columns.Count = 0 Then
            '○画面表示データ復元
            If Master.RecoverTable(T00004tbl) <> True Then Exit Sub
        End If
        '　※　絞込（Cells("Hidden")： 0=表示対象 , 1=非表示対象)
        For Each T00004row In T00004tbl.Rows
            If T00004row("HIDDEN") = "0" Then
                WW_DataCNT = WW_DataCNT + 1
            End If

            If Not String.IsNullOrEmpty(T00004row("JXORDERID")) AndAlso
               T00004row("JXORDERSTATUS") = JXORDER_WARNING Then

                'JXオーダーでWARNING（マスタ変換エラー）は警告
                T00004row("OPERATION") = C_LIST_OPERATION_CODE.WARNING
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            If Not Integer.TryParse(WF_GridPosition.Text, WW_GridPosition) Then
                WW_GridPosition = 1
            End If
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(T00004tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= " & WW_GridPosition.ToString & " and LINECNT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString

        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("LINECNT")
        End If

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        CODENAME_get("TORICODE", WF_SELTORICODE.Text, WF_SELTORICODE_TEXT.Text, WW_DUMMY)         '取引先
        CODENAME_get("ORDERORG", WF_SELORDERORG.Text, WF_SELORDERORG_TEXT.Text, WW_DUMMY)         '受注部署

    End Sub

    ''' <summary>
    ''' 光英受信ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonGet_Click()

        rightview.setErrorReport("")
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL

        '受信ファイルリスト
        Dim dicFileList As New Dictionary(Of String, List(Of FileInfo))

        Try

            '光英ファイルFTP受信
            work.GetKoueiFile(work.WF_SEL_SHIPORG.Text, dicFileList, O_RTN)
            If Not isNormal(O_RTN) Then
                Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "光英受信")
                Exit Sub
            End If

            Dim dateF As String = work.WF_SEL_SHUKODATEF.Text.Replace("/", "")
            Dim dateT As String = work.WF_SEL_SHUKODATET.Text.Replace("/", "")
            'Dim fileList = dicFileList.Where(Function(x) x.Key >= dateF AndAlso x.Key <= dateT).SelectMany(Function(x) x.Value).ToList
            Dim fileList = dicFileList.Where(Function(x) x.Key >= dateF AndAlso x.Key <= dateT).Select(Function(x) x.Value.Last).ToList
            If fileList.Count > 0 Then

                '光英ファイルが存在する場合は取込
                UPLOAD_KOUEI(fileList, O_RTN)
            Else
                '○メッセージ表示
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.INF, )
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "光英受信")
            Exit Sub

        End Try

        Try
            '過去ファイル削除
            For Each dat In dicFileList
                If dat.Key < CS0050SESSION.LOGONDATE.Replace("/", "") Then
                    For Each file In dat.Value
                        If file.Exists Then
                            '光英連携が安定稼働するまでは論理削除
                            Dim bakFileName As New FileInfo(file.FullName & ".used")
                            If bakFileName.Exists Then
                                bakFileName.Delete()
                            End If
                            file.MoveTo(bakFileName.FullName)

                            'file.Delete()
                        End If
                    Next
                End If
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "光英受信ファイル削除")
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一時保存ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSAVE_Click()

        '■■■ セッション変数設定 ■■■
        '○入力値チェック
        Dim WW_CONVERT As String = ""
        Dim WW_TEXT As String = ""

        '○画面表示データ復元
        If Not Master.RecoverTable(T00004tbl) Then
            Exit Sub
        End If

        '一時保存ファイルに出力
        If Master.SaveTable(T00004tbl, work.WF_SEL_XMLsaveTmp.Text) = False Then
            Exit Sub
        End If

        '一時保存ファイルに条件パラメータ出力
        Dim T0004PARMtbl As DataTable = New DataTable
        work.PARMtbl_ColumnsAdd(T0004PARMtbl)

        Dim WW_T0004PARMrow As DataRow = T0004PARMtbl.NewRow

        WW_T0004PARMrow("LINECNT") = 1

        '会社コード　
        WW_T0004PARMrow("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
        '出庫日
        WW_T0004PARMrow("SHUKODATEF") = work.WF_SEL_SHUKODATEF.Text
        WW_T0004PARMrow("SHUKODATET") = work.WF_SEL_SHUKODATET.Text
        '出荷日
        WW_T0004PARMrow("SHUKADATEF") = work.WF_SEL_SHUKADATEF.Text
        WW_T0004PARMrow("SHUKADATET") = work.WF_SEL_SHUKADATET.Text
        '届日　
        WW_T0004PARMrow("TODOKEDATEF") = work.WF_SEL_TODOKEDATEF.Text
        WW_T0004PARMrow("TODOKEDATET") = work.WF_SEL_TODOKEDATET.Text
        '受注部署
        WW_T0004PARMrow("ORDERORG") = work.WF_SEL_ORDERORG.Text
        '出荷部署
        WW_T0004PARMrow("SHIPORG") = work.WF_SEL_SHIPORG.Text
        '油種
        WW_T0004PARMrow("OILTYPE") = work.WF_SEL_OILTYPE.Text

        '光英読込中リスト
        Dim sb = New StringBuilder()
        For Each item As ListItem In WF_KoueiLoadFile.Items
            sb.Append(item.Value)
            sb.Append(C_VALUE_SPLIT_DELIMITER)
        Next
        WW_T0004PARMrow("KOUEILOADFILE") = sb.ToString

        T0004PARMtbl.Rows.Add(WW_T0004PARMrow)

        '条件（パラメタファイル）
        If Master.SaveTable(T0004PARMtbl, work.WF_SEL_XMLsavePARM.Text) = False Then
            Master.output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_STAFFCODE.Focus()

    End Sub


    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_LINECNT As Integer

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '○絞り込み操作（GridView明細Hidden設定）
        For Each row In T00004tbl.Rows

            '削除データは対象外
            If row("DELFLG") = C_DELETE_FLG.DELETE Then Continue For

            row("HIDDEN") = 1

            '行番号が相違の場合は絞込判定対象、同一の場合は非表示設定
            If row("LINECNT") <> WW_LINECNT Then
                WW_LINECNT = row("LINECNT")

                'オブジェクト　グループコード　絞込判定
                If (WF_SELTORICODE.Text = "") AndAlso (WF_SELORDERORG.Text = "") Then
                    row("HIDDEN") = 0
                End If

                If (WF_SELTORICODE.Text <> "") AndAlso (WF_SELORDERORG.Text = "") Then
                    If row("TORICODE") = WF_SELTORICODE.Text Then
                        row("HIDDEN") = 0
                    End If
                End If

                If (WF_SELTORICODE.Text = "") AndAlso (WF_SELORDERORG.Text <> "") Then
                    If row("ORDERORG") = WF_SELORDERORG.Text Then
                        row("HIDDEN") = 0
                    End If
                End If

                If (WF_SELTORICODE.Text <> "") AndAlso (WF_SELORDERORG.Text <> "") Then
                    If row("TORICODE") = WF_SELTORICODE.Text AndAlso
                       row("ORDERORG") = WF_SELORDERORG.Text Then
                        row("HIDDEN") = 0
                    End If
                End If
            End If

        Next

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    ''' 新規ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonNEW_Click()

        Dim WW_INDEX As Integer = 0

        '■■■ Detailデータ設定 ■■■

        '一時Table(T00004INPtbl)準備
        Master.CreateEmptyTable(T00004INPtbl)

        WF_REP_LINECNT.Value = ""     '表示LINECNT（打変不可）

        Dim T00004INProw As DataRow
        '空行を4件作成
        For i As Integer = 1 To 4
            T00004INProw = T00004INPtbl.NewRow()
            T00004INProw("LINECNT") = 0
            T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00004INProw("TIMSTP") = 0
            T00004INProw("SELECT") = 1
            T00004INProw("HIDDEN") = 0
            T00004INProw("INDEX") = WW_INDEX

            T00004INProw("TUMIOKI") = ""
            T00004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T00004INProw("CAMPCODENAME") = ""
            T00004INProw("TERMORG") = WF_DEFORG.Text
            T00004INProw("TERMORGNAME") = ""
            T00004INProw("ORDERNO") = ""
            T00004INProw("DETAILNO") = ""
            T00004INProw("TRIPNO") = ""
            T00004INProw("DROPNO") = ""
            T00004INProw("SEQ") = ""
            T00004INProw("TORICODE") = ""
            T00004INProw("TORICODENAME") = ""
            T00004INProw("OILTYPE") = work.WF_SEL_OILTYPE.Text
            T00004INProw("OILTYPENAME") = ""
            T00004INProw("STORICODE") = ""
            T00004INProw("STORICODENAME") = ""
            T00004INProw("ORDERORG") = work.WF_SEL_ORDERORG.Text
            T00004INProw("ORDERORGNAME") = ""
            T00004INProw("SHUKODATE") = work.WF_SEL_SHUKODATEF.Text  '出庫日
            T00004INProw("KIKODATE") = ""
            T00004INProw("KIJUNDATE") = ""
            T00004INProw("SHUKADATE") = work.WF_SEL_SHUKADATEF.Text  '出荷日
            T00004INProw("TUMIOKIKBN") = ""
            T00004INProw("TUMIOKIKBNNAME") = ""
            T00004INProw("URIKBN") = "1"
            T00004INProw("URIKBNNAME") = ""
            T00004INProw("STATUS") = ""
            T00004INProw("STATUSNAME") = ""
            T00004INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
            T00004INProw("SHIPORGNAME") = ""
            T00004INProw("SHUKABASHO") = ""
            T00004INProw("SHUKABASHONAME") = ""
            T00004INProw("INTIME") = ""
            T00004INProw("OUTTIME") = ""
            T00004INProw("SHUKADENNO") = ""
            T00004INProw("TUMISEQ") = ""
            T00004INProw("TUMIBA") = ""
            T00004INProw("GATE") = ""
            T00004INProw("GSHABAN") = ""
            T00004INProw("GSHABANLICNPLTNO") = ""
            T00004INProw("RYOME") = "1"
            T00004INProw("CONTCHASSIS") = ""
            T00004INProw("CONTCHASSISLICNPLTNO") = ""
            T00004INProw("SHAFUKU") = ""
            T00004INProw("STAFFCODE") = ""
            T00004INProw("STAFFCODENAME") = ""
            T00004INProw("SUBSTAFFCODE") = ""
            T00004INProw("SUBSTAFFCODENAME") = ""
            T00004INProw("STTIME") = ""
            T00004INProw("TORIORDERNO") = ""
            T00004INProw("TODOKEDATE") = work.WF_SEL_TODOKEDATEF.Text
            T00004INProw("TODOKETIME") = ""
            T00004INProw("TODOKECODE") = ""
            T00004INProw("TODOKECODENAME") = ""
            T00004INProw("PRODUCT1") = ""
            T00004INProw("PRODUCT1NAME") = ""
            T00004INProw("PRODUCT2") = ""
            T00004INProw("PRODUCT2NAME") = ""
            T00004INProw("PRODUCTCODE") = ""
            T00004INProw("PRODUCTNAME") = ""
            T00004INProw("PRATIO") = ""
            T00004INProw("SMELLKBN") = ""
            T00004INProw("SMELLKBNNAME") = ""
            T00004INProw("CONTNO") = ""
            T00004INProw("SURYO") = ""
            T00004INProw("SURYO_SUM") = ""
            T00004INProw("DAISU") = ""
            T00004INProw("DAISU_SUM") = ""
            T00004INProw("JSURYO") = ""
            T00004INProw("JSURYO_SUM") = ""
            T00004INProw("JDAISU") = ""
            T00004INProw("JDAISU_SUM") = ""
            T00004INProw("REMARKS1") = ""
            T00004INProw("REMARKS2") = ""
            T00004INProw("REMARKS3") = ""
            T00004INProw("REMARKS4") = ""
            T00004INProw("REMARKS5") = ""
            T00004INProw("REMARKS6") = ""
            T00004INProw("SHARYOTYPEF") = ""
            T00004INProw("TSHABANF") = ""
            T00004INProw("SHARYOTYPEB") = ""
            T00004INProw("TSHABANB") = ""
            T00004INProw("SHARYOTYPEB2") = ""
            T00004INProw("TSHABANB2") = ""
            T00004INProw("DELFLG") = ""
            T00004INProw("ADDR") = ""
            T00004INProw("DISTANCE") = ""
            T00004INProw("ARRIVTIME") = ""
            T00004INProw("NOTES1") = ""
            T00004INProw("NOTES2") = ""
            T00004INProw("NOTES3") = ""
            T00004INProw("NOTES4") = ""
            T00004INProw("NOTES5") = ""
            T00004INProw("NOTES6") = ""
            T00004INProw("NOTES7") = ""
            T00004INProw("NOTES8") = ""
            T00004INProw("NOTES9") = ""
            T00004INProw("NOTES10") = ""
            T00004INProw("SHARYOINFO1") = ""
            T00004INProw("SHARYOINFO2") = ""
            T00004INProw("SHARYOINFO3") = ""
            T00004INProw("SHARYOINFO4") = ""
            T00004INProw("SHARYOINFO5") = ""
            T00004INProw("SHARYOINFO6") = ""
            T00004INProw("STAFFNOTES1") = ""
            T00004INProw("STAFFNOTES2") = ""
            T00004INProw("STAFFNOTES3") = ""
            T00004INProw("STAFFNOTES4") = ""
            T00004INProw("STAFFNOTES5") = ""

            T00004INProw("WORK_NO") = ""
            T00004INProw("JXORDERID") = ""
            T00004INProw("JXORDERSTATUS") = ""

            T00004INPtbl.Rows.Add(T00004INProw)


            WW_INDEX = WW_INDEX + 1
        Next

        'ヘッダ初期表示
        Dim WW_TEXT As String = ""
        For Each INProw In T00004INPtbl.Rows

            '出庫日
            WF_SHUKODATE.Text = INProw("SHUKODATE")
            '帰庫日
            WF_KIKODATE.Text = INProw("KIKODATE")
            '両目
            WF_RYOME.Text = INProw("RYOME")

            '出荷日
            WF_SHUKADATE.Text = INProw("SHUKADATE")
            '届日
            WF_TODOKEDATE.Text = INProw("TODOKEDATE")
            '出荷場所
            'WF_SHUKABASHO.Text = INProw("SHUKABASHO")

            '油種
            WF_OILTYPE.Text = INProw("OILTYPE")
            CODENAME_get("OILTYPE", WF_OILTYPE.Text, WW_TEXT, WW_DUMMY)
            WF_OILTYPE_TEXT.Text = WW_TEXT

            '荷主
            WF_TORICODE.Text = INProw("TORICODE")
            CODENAME_get("TORICODE", WF_TORICODE.Text, WW_TEXT, WW_DUMMY)
            WF_TORICODE_TEXT.Text = WW_TEXT
            '販売店
            WF_STORICODE.Text = INProw("STORICODE")
            CODENAME_get("STORICODE", WF_STORICODE.Text, WW_TEXT, WW_DUMMY)
            WF_STORICODE_TEXT.Text = WW_TEXT
            '売上計上基準
            WF_URIKBN.Text = INProw("URIKBN")
            CODENAME_get("URIKBN", WF_URIKBN.Text, WW_TEXT, WW_DUMMY)
            WF_URIKBN_TEXT.Text = WW_TEXT







            '受注組織
            WF_ORDERORG.Text = INProw("ORDERORG")
            CODENAME_get("ORDERORG", WF_ORDERORG.Text, WW_TEXT, WW_DUMMY)
            WF_ORDERORG_TEXT.Text = WW_TEXT

            '出荷組織
            WF_SHIPORG.Text = INProw("SHIPORG")
            CODENAME_get("SHIPORG", WF_SHIPORG.Text, WW_TEXT, WW_DUMMY)
            WF_SHIPORG_TEXT.Text = WW_TEXT


            '業務車番
            WF_GSHABAN.Text = INProw("GSHABAN")
            'コンテナシャーシ
            WF_CONTCHASSIS.Text = INProw("CONTCHASSIS")
            CODENAME_get("CONTCHASSIS", WF_CONTCHASSIS.Text, WW_TEXT, WW_DUMMY)
            WF_CONTCHASSIS_TEXT.Text = WW_TEXT
            '車腹
            WF_SHAFUKU.Text = INProw("SHAFUKU")


            '積置区分
            WF_TUMIOKIKBN.Text = INProw("TUMIOKIKBN")
            CODENAME_get("TUMIOKIKBN", WF_TUMIOKIKBN.Text, WW_TEXT, WW_DUMMY)
            WF_TUMIOKIKBN_TEXT.Text = WW_TEXT
            'トリップ
            WF_TRIPNO.Text = INProw("TRIPNO")
            'ドロップ
            WF_DROPNO.Text = INProw("DROPNO")


            '乗務員
            WF_STAFFCODE.Text = INProw("STAFFCODE")
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WW_TEXT, WW_DUMMY)
            WF_STAFFCODE_TEXT.Text = WW_TEXT
            '副乗務員
            WF_SUBSTAFFCODE.Text = INProw("SUBSTAFFCODE")
            CODENAME_get("SUBSTAFFCODE", WF_SUBSTAFFCODE.Text, WW_TEXT, WW_DUMMY)
            WF_SUBSTAFFCODE_TEXT.Text = WW_TEXT
            '出勤時間
            WF_STTIME.Text = ""

            'JXORDER
            WF_JXORDERID.Text = ""
            WF_JXORDERSTATUS.Text = ""
            Exit For
        Next

        '○Detail初期設定
        Repeater_INIT()

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        'close
        WF_IsHideDetailBox.Value = "0"
        WF_IsKoueiData.Value = "0"

        WF_Sel_LINECNT.Enabled = True
        WF_SHUKODATE.Enabled = True
        WF_SHUKADATE.Enabled = True
        WF_TODOKEDATE.Enabled = True
        WF_KIKODATE.Enabled = True
        WF_RYOME.Enabled = True
        WF_ORDERNO.Enabled = True
        WF_DETAILNO.Enabled = True
        WF_SHIPORG.Enabled = True
        WF_TORICODE.Enabled = True
        WF_OILTYPE.Enabled = True
        WF_STORICODE.Enabled = True
        WF_ORDERORG.Enabled = True
        WF_URIKBN.Enabled = True
        WF_GSHABAN.Enabled = True
        WF_TSHABANF.Enabled = True
        WF_TSHABANB.Enabled = True
        WF_TSHABANB2.Enabled = True
        WF_CONTCHASSIS.Enabled = True
        WF_SHAFUKU.Enabled = True
        WF_TUMIOKIKBN.Enabled = True
        WF_TRIPNO.Enabled = True
        WF_DROPNO.Enabled = True
        WF_STTIME.Enabled = True

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKODATE"
        WF_SHUKODATE.Focus()

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)


        '■■■ DB更新 ■■■

        '【概要】
        '⓪前提　…　T00003tbl格納内容
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB存在レコード（削除対象）」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB非存在レコード（追加対象）」
        '   ※画面選択条件（出庫日、届日）により画面表示／非表示が決定されている
        '①DB最新レコードを、T00003UPDtblへ格納
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示済レコード」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示未レコード」
        '②DB最新レコードによるタイムスタンプチェック（他端末での更新競合）
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示済レコード」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示未レコード」
        '③T00003UPDtblから画面表示レコードを削除（結果は以下内容となる）
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示未レコード」
        '④T00003UPDtblへT00003tblを追加（結果は以下内容となる）
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示未レコード」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB存在レコード（削除対象）」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB非存在レコード（追加対象）」
        '⑤DBレコード削除
        '   ※更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB存在レコード（削除対象）」を利用し
        '   　更新対象受注（取・油種・出荷日・受注組織・出荷組織）を削除する。
        '⑥DBレコード追加（下記レコードを追加）
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）に関連する「画面表示未レコード」
        '   ・更新対象受注（取・油種・出荷日・受注組織・出荷組織）の「画面表示済＆DB非存在レコード（追加対象）」


        Dim WW_DATENOW As Date = Date.Now
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open()

        ' L1統計DB
        Dim cL1TOKEI As L1TOKEI = New L1TOKEI(SQLcon, Master.USERID, Master.USERTERMID)

        ' ***  T00004UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理
        DBupdate_T00004UPDtblget(WW_DUMMY)

        ' ***  T00004SUMtbl更新データ作成
        DBupdate_T00004SUMtblget(WW_DUMMY)

        ' ***  L0001_TOKEIテーブル編集（T00004UPDtblより）
        cL1TOKEI.Edit(T00004UPDtbl, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "伝票番号採番")
        End If

        ' ***  T00003・T00004tbl関連データ削除
        DBupdate_T4DELETE(WW_DATENOW, WW_ERRCODE)

        ' ***  T00004tbl追加
        DBupdate_T4INSERT(WW_DATENOW, WW_ERRCODE)

        ' ***  T00003tbl追加
        DBupdate_T3INSERT(WW_DATENOW, WW_ERRCODE)

        ' ***  L1追加
        cL1TOKEI.Update(T00004UPDtbl, WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT, "統計DB追加")
        End If

        'サマリ処理
        SUMMRY_SET()

        Try
            '行位置が一致するデータ取得
            Dim errCnt = (From tbl In T00004tbl.AsEnumerable
                          Select tbl
                          Where tbl.Field(Of String)("DELFLG") <> C_DELETE_FLG.DELETE _
                            And tbl.Field(Of String)("JXORDERSTATUS") = "")
            If errCnt.Count = 0 Then

                For Each file As ListItem In WF_KoueiLoadFile.Items
                    Dim f = New FileInfo(file.Value)
                    If f.Exists Then
                        '光英連携が安定稼働するまでは論理削除
                        Dim bakFileName As New FileInfo(f.FullName & ".used")
                        If bakFileName.Exists Then
                            bakFileName.Delete()
                        End If
                        f.MoveTo(bakFileName.FullName)
                    End If
                    f = Nothing
                Next
                WF_KoueiLoadFile.Items.Clear()
            End If
        Catch ex As Exception
        End Try

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)
        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()             'PARAM02:帳票ID
        CS0030REPORT.FILEtyp = "pdf"                                'PARAM03:出力ファイル形式
        CS0030REPORT.TBLDATA = T00004tbl                            'PARAM04:データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If isNormal(CS0030REPORT.ERR) Then
        Else
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0022REPORT")
            End If
            Exit Sub
        End If

        '別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)

    End Sub

    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '削除データを除外
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1'"
        CS0026TBLSORTget.sort(T00004tbl)

        '○ 帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text           '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                    'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                           'PARAM01:画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()             'PARAM02:帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                               'PARAM03:出力ファイル形式
        CS0030REPORT.TBLDATA = T00004tbl                            'PARAM04:データ参照tabledata
        CS0030REPORT.CS0030REPORT()

        If isNormal(CS0030REPORT.ERR) Then
        Else
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            Exit Sub
        End If

        '別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

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
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(T00004tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If
    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)
    End Sub


    ''' <summary>
    ''' 詳細画面-表更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()
        Dim WW_ERRWORD As String

        '〇エラーレポート準備
        rightview.setErrorReport("")
        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '●DetailBoxをT00004INPtblへ退避 …　画面Detail --> T4INPtbl
        DetailBoxToINP()

        '前回OPERATIONの設定
        For Each T00004INProw In T00004INPtbl.Rows
            For j As Integer = 0 To T00004tbl.Rows.Count - 1
                If T00004tbl.Rows(j)("LINECNT") = T00004INProw("LINECNT") Then
                    EditOperationText(T00004tbl.Rows(j), False, T00004INProw)
                    Exit For
                End If
            Next
        Next

        WW_ERRLIST = New List(Of String)

        '■■■ 項目チェック ■■■        …　チェック結果：エラーコード（WW_ERR）
        '●チェック処理
        INPtbl_CHEK(WW_ERRCODE)

        '●関連チェック処理
        INPtbl_CHEK_DATE(WW_ERRCODE)

        '■■■ 変更有無チェック ■■■
        '    Grid画面明細：T00004INProw("WORK_NO")、T00004INProw("LINECNT")、T00004INProw("ORDERNO")クリア
        '　　変更発生　　：T00004INProw("OPERATION")へ"更新"or"エラー"を設定

        '●変更有無取得
        Dim WW_Change As String = ""
        Dim WW_GridNew As String = ""
        For CNT As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(CNT)

            '○変更有無判定
            WW_Change = ""

            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            '変更またはエラーの場合、"有"とする
            If T00004INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.UPDATING) OrElse
                T00004INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.WARNING) OrElse
                T00004INProw("OPERATION").ToString.Contains(C_LIST_OPERATION_CODE.ERRORED) Then
                WW_Change = "有"
            Else
                '明細が消された場合は"有"とする
                If Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 AndAlso T00004INProw("WORK_NO") <> "" Then
                    WW_Change = "有"
                Else
                    If WF_REP_Change.Value = "" Then
                        '空更新
                        WW_Change = "有"
                    Else
                        WW_Change = "有"
                    End If
                End If
            End If


            '比較対象がなければ"新"とする
            If T00004tbl.Rows.Count = 0 Then
                WW_Change = "有"
                T00004INProw("WORK_NO") = ""
                T00004INProw("LINECNT") = 0
                T00004INProw("ORDERNO") = C_LIST_OPERATION_CODE.NODATA
            End If

            '①詳細画面で追記された場合は"新"とする
            If (Val(T00004INProw("SURYO")) <> 0 Or Val(T00004INProw("DAISU")) <> 0) And T00004INProw("WORK_NO") = "" Then
                WW_Change = "有"
                T00004INProw("WORK_NO") = ""
                T00004INProw("LINECNT") = 0
                T00004INProw("ORDERNO") = ""
            End If

            '②詳細画面で、行番号クリア操作時（参照コピー）。
            If WF_Sel_LINECNT.Text = "" Then
                WW_Change = "有"
                T00004INProw("WORK_NO") = ""
                T00004INProw("LINECNT") = 0
                T00004INProw("ORDERNO") = ""
            End If

            '③詳細画面で、画面表示単位（受注番号要素）が変更された場合。
            If WF_REP_Change.Value = "1" Then
                WW_Change = "有"
                If T00004INProw("JXORDERID") = "" Then
                    T00004INProw("WORK_NO") = ""
                    T00004INProw("LINECNT") = 0
                    T00004INProw("ORDERNO") = ""
                End If
            End If
            '④詳細画面で、画面表示単位（配送明細要素）が変更された場合。
            If WF_REP_Change.Value = "2" Then
                WW_Change = "有"
            End If

            'エラーは設定しない
            If T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA And (WW_Change = "有" Or WW_Change = "新") Then
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

        Next

        '●チェック処理2       …　新規登録（T00004INProw("WORK_NO") = ""）でT4tblに存在した場合、エラー。
        For CNT As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(CNT)

            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            If T00004INProw("WORK_NO") = "" Then        '新規明細の場合
                For j As Integer = 0 To T00004tbl.Rows.Count - 1

                    '自明細以外　かつ　取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If Val(T00004tbl.Rows(j)("LINECNT")) <> Val(WF_Sel_LINECNT.Text) And
                       T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") And
                       T00004tbl.Rows(j)("SHUKODATE") = T00004INProw("SHUKODATE") And
                       T00004tbl.Rows(j)("GSHABAN") = T00004INProw("GSHABAN") And
                       T00004tbl.Rows(j)("TRIPNO") = T00004INProw("TRIPNO") And
                       T00004tbl.Rows(j)("DROPNO") = T00004INProw("DROPNO") And
                       T00004tbl.Rows(j)("DELFLG") <> "1" And
                       T00004INProw("DELFLG") <> "1" Then

                        Dim WW_ERR_MES As StringBuilder = New StringBuilder()
                        WW_ERR_MES.AppendLine("・更新できないレコード(同一受注)です。")
                        WW_ERR_MES.AppendLine("  --> " & " 同一条件の配車が既に存在します。 , ")
                        WW_ERR_MES.AppendLine("  --> " & " （業務車番、トリップ、ドロップが同一） ")
                        WW_ERR_MES.AppendLine("  --> 項番　　= " & T00004INProw("LINECNT").ToString() & " , ")
                        WW_ERR_MES.AppendLine("  --> 明細番号= " & CNT.ToString("000") & " , ")
                        WW_ERR_MES.AppendLine("  --> 取引先　=" & T00004INProw("TORICODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 届先　　=" & T00004INProw("TODOKECODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出荷場所=" & T00004INProw("SHUKABASHO") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出庫日　=" & T00004INProw("SHUKODATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 届日　　=" & T00004INProw("TODOKEDATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 出荷日　=" & T00004INProw("SHUKADATE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 車番　　=" & T00004INProw("GSHABAN") & " , ")
                        WW_ERR_MES.AppendLine("  --> 乗務員　=" & T00004INProw("STAFFCODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> 品名  　=" & T00004INProw("PRODUCTCODE") & " , ")
                        WW_ERR_MES.AppendLine("  --> ﾄﾘｯﾌﾟ 　=" & T00004INProw("TRIPNO") & " , ")
                        WW_ERR_MES.AppendLine("  --> ﾄﾞﾛｯﾌﾟ　=" & T00004INProw("DROPNO") & " , ")
                        WW_ERR_MES.AppendLine("  --> 削除　　=" & T00004INProw("DELFLG") & " ")
                        rightview.AddErrorReport(WW_ERR_MES.ToString)

                        Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)

                        'エラーメッセージ内の項番、明細番号置き換え
                        WW_ERRWORD = rightview.GetErrorReport()
                        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1
                            '項番
                            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", Val(WF_REP_LINECNT.Value).ToString)
                            '明細番号
                            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", (i + 1).ToString)
                        Next
                        rightview.SetErrorReport(WW_ERRWORD)

                        WW_ERRCODE = C_MESSAGE_NO.BOX_ERROR_EXIST
                        T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                    End If
                Next
            End If

        Next

        '●重大エラー時の処理
        If WW_ERRCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            'エラー箇所置換
            WW_ERRWORD = rightview.GetErrorReport()
            For i As Integer = 0 To T00004INPtbl.Rows.Count - 1
                '項番
                WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00004INPtbl.Rows(i)("LINECNT"))
                '明細番号
                WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", (i + 1).ToString())
            Next
            rightview.SetErrorReport(WW_ERRWORD)

            'メッセージ表示
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)

            Exit Sub
        End If

        '■■■ 更新前処理（入力情報へ受注番号設定、Grid画面の同一行情報を削除）　■■■
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            For j As Integer = 0 To T00004tbl.Rows.Count - 1

                '状態をクリア設定
                EditOperationText(T00004tbl.Rows(j), False)

                If T00004INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    'Grid画面行追加の場合は受注番号を取得
                    If T00004tbl.Rows(j)("TORICODE") = T00004INProw("TORICODE") And
                       T00004tbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") And
                       T00004tbl.Rows(j)("KIJUNDATE") = T00004INProw("KIJUNDATE") And
                       T00004tbl.Rows(j)("ORDERORG") = T00004INProw("ORDERORG") And
                       T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") Then

                        T00004INProw("ORDERNO") = T00004tbl.Rows(j)("ORDERNO")
                        T00004INProw("DETAILNO") = "000"

                    End If


                    '同一行情報を論理削除（T4実態が存在する場合、物理削除。）
                    'If WF_SEL_LINECNT.Text <> "" And T00004tbl.Rows(j)("LINECNT") = Val(WF_REP_LINECNT.Value) And _
                    '   Val(WF_REP_LINECNT.Value) <> 0 And T00004tbl.Rows(j)("DELFLG") <> "1" Then
                    If WF_Sel_LINECNT.Text <> "" And T00004tbl.Rows(j)("LINECNT") = Val(WF_REP_LINECNT.Value) And
                       Val(WF_REP_LINECNT.Value) <> 0 Then

                        T00004tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T00004tbl.Rows(j)("DELFLG") = "1"   '削除
                        T00004tbl.Rows(j)("HIDDEN") = "1"   '非表示
                        T00004tbl.Rows(j)("SELECT") = "0"   '明細表示対象外

                    End If

                End If

            Next
        Next

        'T00004tblの削除データを物理削除
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1' or TIMSTP <> 0 or (DELFLG = '1' and HIDDEN = '0')"
        CS0026TBLSORTget.sort(T00004tbl)

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■
        INPtbl_PreUpdate1()

        '■■■ 更新前処理（入力情報へLINECNTを付番）　■■■
        INPtbl_PreUpdate2()

        '■■■ 更新前処理（入力情報へ暫定受注番号を付番）　■■■
        INPtbl_PreUpdate3()

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00004tbl, False)

        '○サマリ処理 
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)
        SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        WW_ERRWORD = rightview.GetErrorReport()
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00004INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00004INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)

        '○Detailクリア
        'detailboxヘッダークリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        '画面切替設定
        WF_IsHideDetailBox.Value = "1"

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        '○Detail初期設定
        T00004INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

        '○Close
        WF_DViewRep1.Visible = False
        WF_DViewRep1.Dispose()
        WF_DViewRep1 = Nothing

    End Sub

    ''' <summary>
    ''' detailbox クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○detailboxヘッダークリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        '新規ボタン処理
        WF_ButtonNEW_Click()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKODATE"
        WF_SHUKODATE.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_BACK_Click()

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '選択状態クリア
        EditOperationText(T00004tbl, False)

        '○ 画面表示データ保存
        Master.SaveTable(T00004tbl)

        '○detailboxクリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

        '画面切替設定
        WF_IsHideDetailBox.Value = "1"

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        'close
        pnlListArea.Visible = True
        WF_DViewRep1.Visible = False
        WF_DViewRep1.Dispose()
        WF_DViewRep1 = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　
    ' ******************************************************************************

    ''' <summary>
    ''' GridViewサマリ処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SUMMRY_SET()

        Dim SURYO_SUM As Decimal = 0
        Dim DAISU_SUM As Long = 0
        Dim JSURYO_SUM As Decimal = 0
        Dim JDAISU_SUM As Long = 0

        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,RYOME ,TRIPNO ,DROPNO ,SEQ"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)

        '最終行から初回行へループ
        For i As Integer = 0 To T00004tbl.Rows.Count - 1

            Dim T00004row = T00004tbl.Rows(i)

            If T00004row("SEQ") = "01" And T00004row("HIDDEN") <> "1" Then
                SURYO_SUM = 0
                DAISU_SUM = 0
                JSURYO_SUM = 0
                JDAISU_SUM = 0

                For j As Integer = i To T00004tbl.Rows.Count - 1
                    If CompareOrder(T00004row, T00004tbl.Rows(j)) Then
                        If T00004tbl.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then

                            Try
                                SURYO_SUM += CDbl(T00004tbl.Rows(j)("SURYO"))
                            Catch ex As Exception
                            End Try
                            Try
                                JSURYO_SUM += CDbl(T00004tbl.Rows(j)("JSURYO"))
                            Catch ex As Exception
                            End Try

                            DAISU_SUM = 1
                            JDAISU_SUM = 1
                        End If
                    Else
                        Exit For
                    End If

                Next

                '表示行にサマリ結果を反映
                T00004row("SURYO_SUM") = SURYO_SUM.ToString("0.000")
                T00004row("DAISU_SUM") = DAISU_SUM.ToString("0")
                T00004row("JSURYO_SUM") = JSURYO_SUM.ToString("0.000")
                T00004row("JDAISU_SUM") = JDAISU_SUM.ToString("0")
                T00004row("HIDDEN") = 0   '0:表示

            Else
                T00004row("HIDDEN") = 1   '1:非表示
            End If

        Next

    End Sub


    ''' <summary>
    ''' LeftBox項目名称設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CODENAME_set(ByRef T00004row As DataRow)

        Dim koueiFlag As Boolean = False
        If Not String.IsNullOrEmpty(T00004row("JXORDERID")) Then
            koueiFlag = True
        End If

        '○名称付与

        '会社名称
        T00004row("CAMPCODENAME") = ""
        CODENAME_get("CAMPCODE", T00004row("CAMPCODE"), T00004row("CAMPCODENAME"), WW_DUMMY)

        '取引先名称
        T00004row("TORICODENAME") = ""
        CODENAME_get("TORICODE", T00004row("TORICODE"), T00004row("TORICODENAME"), WW_DUMMY)

        '油種名称
        T00004row("OILTYPENAME") = ""
        CODENAME_get("OILTYPE", T00004row("OILTYPE"), T00004row("OILTYPENAME"), WW_DUMMY)

        '販売店名称
        T00004row("STORICODENAME") = ""
        CODENAME_get("STORICODE", T00004row("STORICODE"), T00004row("STORICODENAME"), WW_DUMMY)

        '受注受付部署名称
        T00004row("ORDERORGNAME") = ""
        CODENAME_get("ORDERORG", T00004row("ORDERORG"), T00004row("ORDERORGNAME"), WW_DUMMY)

        '売上計上基準名称
        T00004row("URIKBNNAME") = ""
        CODENAME_get("URIKBN", T00004row("URIKBN"), T00004row("URIKBNNAME"), WW_DUMMY)

        '状態名称
        T00004row("STATUSNAME") = ""
        CODENAME_get("STATUS", T00004row("STATUS"), T00004row("STATUSNAME"), WW_DUMMY)

        '積置区分名称
        T00004row("TUMIOKIKBNNAME") = ""
        CODENAME_get("TUMIOKIKBN", T00004row("TUMIOKIKBN"), T00004row("TUMIOKIKBNNAME"), WW_DUMMY)

        '品名１名称
        T00004row("PRODUCT1NAME") = ""
        CODENAME_get("PRODUCT1", T00004row("PRODUCT1"), T00004row("PRODUCT1NAME"), WW_DUMMY)

        If koueiFlag AndAlso T00004row("PRODUCTCODE").ToString.Contains("!") Then
            '光英オーダーかつコードに『!』が含まれる場合は名称取得しない
            '※JOTコード変換前の光英コード・名称が設定されている為
        Else

            '品名コード名称
            T00004row("PRODUCTNAME") = ""
            CODENAME_get("PRODUCTCODE", T00004row("PRODUCTCODE"), T00004row("PRODUCTNAME"), WW_DUMMY)
            T00004row("PRODUCT2NAME") = T00004row("PRODUCTNAME")
            '品名追加情報
            If T00004row("HTANI") = "" Then
                'List配送単位
                Dim product = GetProduct(T00004row("SHIPORG"), T00004row("PRODUCTCODE"))
                If Not IsNothing(product) Then
                    T00004row("HTANI") = product.HTANI
                End If
            End If
        End If

        '臭有無名称
        T00004row("SMELLKBNNAME") = ""
        CODENAME_get("SMELLKBN", T00004row("SMELLKBN"), T00004row("SMELLKBNNAME"), WW_DUMMY)

        '出荷場所名称
        T00004row("SHUKABASHONAME") = ""
        CODENAME_get("SHUKABASHO", T00004row("SHUKABASHO"), T00004row("SHUKABASHONAME"), WW_DUMMY)

        '出荷部署名称
        T00004row("SHIPORGNAME") = ""
        CODENAME_get("SHIPORG", T00004row("SHIPORG"), T00004row("SHIPORGNAME"), WW_DUMMY)

        If koueiFlag AndAlso T00004row("GSHABAN").ToString.Contains("!") Then
            '光英オーダーかつコードに『!』が含まれる場合は名称取得しない
            '※JOTコード変換前の光英コード・名称が設定されている為
        Else
            '業務車番ナンバー
            T00004row("GSHABANLICNPLTNO") = ""
            CODENAME_get("GSHABAN", T00004row("GSHABAN"), T00004row("GSHABANLICNPLTNO"), WW_DUMMY)
        End If

        'コンテナシャーシナンバー
        T00004row("CONTCHASSISLICNPLTNO") = ""
        CODENAME_get("CONTCHASSIS", T00004row("CONTCHASSIS"), T00004row("CONTCHASSISLICNPLTNO"), WW_DUMMY)

        '乗務員コード
        T00004row("STAFFCODENAME") = ""
        CODENAME_get("STAFFCODE", T00004row("STAFFCODE"), T00004row("STAFFCODENAME"), WW_DUMMY)

        '副乗務員コード名称
        T00004row("SUBSTAFFCODENAME") = ""
        CODENAME_get("SUBSTAFFCODE", T00004row("SUBSTAFFCODE"), T00004row("SUBSTAFFCODENAME"), WW_DUMMY)

        If koueiFlag AndAlso T00004row("TODOKECODE").ToString.Contains("!") Then
            '光英オーダーかつコードに『!』が含まれる場合は名称取得しない
            '※JOTコード変換前の光英コード・名称が設定されている為
        Else
            '届先コード名称
            T00004row("TODOKECODENAME") = ""
            CODENAME_get("TODOKECODE", T00004row("TODOKECODE"), T00004row("TODOKECODENAME"), WW_DUMMY, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00004row("SHIPORG"), T00004row("TORICODE"), "1"))
        End If

        '配送単位名称
        T00004row("HTANINAME") = ""
        CODENAME_get("HTANI", T00004row("HTANI"), T00004row("HTANINAME"), WW_DUMMY)

        '税区分名称
        T00004row("TAXKBNNAME") = ""
        CODENAME_get("TAXKBN", T00004row("TAXKBN"), T00004row("TAXKBNNAME"), WW_DUMMY)

        '届先追加情報
        Dim datTodoke As TODOKESAKI = GetTodoke(T00004row("SHIPORG"), T00004row("TODOKECODE"))
        If Not IsNothing(datTodoke) AndAlso Not IsNothing(datTodoke.TODOKECODE) Then
            T00004row("ARRIVTIME") = datTodoke.ARRIVTIME                '所要時間
            T00004row("DISTANCE") = datTodoke.DISTANCE                  '配送距離（配車用）
            T00004row("ADDR") = datTodoke.ADDR                          '住所
            T00004row("NOTES1") = datTodoke.NOTES1                      '特定要件１
            T00004row("NOTES2") = datTodoke.NOTES2                      '特定要件２
            T00004row("NOTES3") = datTodoke.NOTES3                      '特定要件３
            T00004row("NOTES4") = datTodoke.NOTES4                      '特定要件３
            T00004row("NOTES5") = datTodoke.NOTES5                      '特定要件３
            T00004row("NOTES6") = datTodoke.NOTES6                      '特定要件３
            T00004row("NOTES7") = datTodoke.NOTES7                      '特定要件３
            T00004row("NOTES8") = datTodoke.NOTES8                      '特定要件３
            T00004row("NOTES9") = datTodoke.NOTES9                      '特定要件３
            T00004row("NOTES10") = datTodoke.NOTES10                    '特定要件３
        End If

        If koueiFlag AndAlso T00004row("GSHABAN").ToString.Contains("?") Then
        Else

            ''車両追加情報
            For i As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
                If WF_ListGSHABAN.Items(i).Value = T00004row("GSHABAN") Then
                    If Val(T00004row("SHAFUKU")) = 0 Then
                        T00004row("SHAFUKU") = WF_ListSHAFUKU.Items(i).Value                  'List車腹
                    End If
                    T00004row("SHARYOTYPEF") = Mid(WF_ListTSHABANF.Items(i).Value, 1, 1)  'List統一車番（前）
                    T00004row("TSHABANF") = Mid(WF_ListTSHABANF.Items(i).Value, 2, 19)    'List統一車番（前）
                    T00004row("SHARYOTYPEB") = Mid(WF_ListTSHABANB.Items(i).Value, 1, 1)  'List統一車番（後）
                    T00004row("TSHABANB") = Mid(WF_ListTSHABANB.Items(i).Value, 2, 19)    'List統一車番（後）
                    T00004row("SHARYOTYPEB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 1, 1) 'List統一車番（後）２
                    T00004row("TSHABANB2") = Mid(WF_ListTSHABANB2.Items(i).Value, 2, 19)   'List統一車番（後）２
                    T00004row("SHARYOINFO1") = WF_ListSHARYOINFO1.Items(i).Value          'List車両情報１
                    T00004row("SHARYOINFO2") = WF_ListSHARYOINFO2.Items(i).Value          'List車両情報２
                    T00004row("SHARYOINFO3") = WF_ListSHARYOINFO3.Items(i).Value          'List車両情報３
                    T00004row("SHARYOINFO4") = WF_ListSHARYOINFO4.Items(i).Value          'List車両情報４
                    T00004row("SHARYOINFO5") = WF_ListSHARYOINFO5.Items(i).Value          'List車両情報５
                    T00004row("SHARYOINFO6") = WF_ListSHARYOINFO6.Items(i).Value          'List車両情報６
                    Exit For
                End If
            Next
        End If

        If Not koueiFlag Then
            '従業員追加情報
            Dim datStaff As STAFF = GetStaff(T00004row("SHIPORG"), T00004row("STAFFCODE"))
            If Not IsNothing(datStaff) AndAlso Not IsNothing(datStaff.STAFFCODE) Then
                T00004row("STAFFCODENAME") = datStaff.STAFFNAMES                '
                T00004row("STAFFNOTES1") = datStaff.NOTES1                      '備考１
                T00004row("STAFFNOTES2") = datStaff.NOTES2                      '備考２
                T00004row("STAFFNOTES3") = datStaff.NOTES3                      '備考３
                T00004row("STAFFNOTES4") = datStaff.NOTES4                      '備考４
                T00004row("STAFFNOTES5") = datStaff.NOTES5                      '備考５
            End If
        End If

        If T00004row("TUMIOKIKBN") = "1" Then
            If T00004row("SHUKODATE") = T00004row("SHUKADATE") Then
                T00004row("TUMIOKI") = "積置"
            Else
                T00004row("TUMIOKI") = "積配"
            End If
        Else
            T00004row("TUMIOKI") = ""
        End If

    End Sub

    ''' <summary>
    ''' GridViewダブルクリック処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer                                   'GridViewのダブルクリック行位置

        '○処理準備
        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        'GridViewのダブルクリック行位置取得
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then
            Exit Sub
        End If
        WF_REP_LINECNT.Value = WW_LINECNT

        '■■■ Grid内容(T00004tbl)よりDetail編集 ■■■
        Master.CreateEmptyTable(T00004INPtbl)

        '行位置が一致するデータ取得
        Dim T00004INP = (From tbl In T00004tbl.AsEnumerable Select tbl
                         Where tbl.Field(Of Integer)("LINECNT") = WW_LINECNT _
                           And tbl.Field(Of String)("SELECT") = "1")

        'Dim T00004INP = (From tbl In T00004tbl.AsEnumerable Select tbl
        '                 Where tbl.Field(Of Integer)("LINECNT") = WW_LINECNT)

        'DetailBoxKey画面編集
        If T00004INP.Count > 0 Then
            Dim T00004row = T00004INP.Last

            '１段目
            WF_Sel_LINECNT.Text = T00004row("LINECNT")

            '２段目
            WF_SHUKODATE.Text = T00004row("SHUKODATE")
            WF_KIKODATE.Text = T00004row("KIKODATE")
            WF_ORDERNO.Text = T00004row("ORDERNO")

            '３段目
            WF_SHUKADATE.Text = T00004row("SHUKADATE")
            WF_TODOKEDATE.Text = T00004row("TODOKEDATE")
            WF_RYOME.Text = T00004row("RYOME")
            WF_DETAILNO.Text = T00004row("DETAILNO")

            '４段目
            WF_OILTYPE.Text = T00004row("OILTYPE")
            WF_OILTYPE_TEXT.Text = T00004row("OILTYPENAME")
            WF_ORDERORG.Text = T00004row("ORDERORG")
            WF_ORDERORG_TEXT.Text = T00004row("ORDERORGNAME")
            WF_SHIPORG.Text = T00004row("SHIPORG")
            WF_SHIPORG_TEXT.Text = T00004row("SHIPORGNAME")

            '５段目
            WF_TORICODE.Text = T00004row("TORICODE")
            WF_TORICODE_TEXT.Text = T00004row("TORICODENAME")
            WF_STORICODE.Text = T00004row("STORICODE")
            WF_STORICODE_TEXT.Text = T00004row("STORICODENAME")
            WF_URIKBN.Text = T00004row("URIKBN")
            WF_URIKBN_TEXT.Text = T00004row("URIKBNNAME")

            '６段目・７段目
            '業務車番
            WF_GSHABAN.Text = T00004row("GSHABAN")
            WF_TSHABANF.Text = T00004row("SHARYOTYPEF") & T00004row("TSHABANF")
            CODENAME_get("TSHABANF", WF_TSHABANF.Text, WF_TSHABANF_TEXT.Text, WW_DUMMY)
            WF_TSHABANB.Text = T00004row("SHARYOTYPEB") & T00004row("TSHABANB")
            CODENAME_get("TSHABANB", WF_TSHABANB.Text, WF_TSHABANB_TEXT.Text, WW_DUMMY)
            WF_TSHABANB2.Text = T00004row("SHARYOTYPEB2") & T00004row("TSHABANB2")
            CODENAME_get("TSHABANB2", WF_TSHABANB2.Text, WF_TSHABANB2_TEXT.Text, WW_DUMMY)
            'コンテナシャーシ
            WF_CONTCHASSIS.Text = T00004row("CONTCHASSIS")
            CODENAME_get("CONTCHASSIS", WF_CONTCHASSIS.Text, WF_CONTCHASSIS_TEXT.Text, WW_DUMMY)
            WF_SHAFUKU.Text = T00004row("SHAFUKU")

            '８段目
            WF_TUMIOKIKBN.Text = T00004row("TUMIOKIKBN")
            WF_TUMIOKIKBN_TEXT.Text = T00004row("TUMIOKIKBNNAME")
            WF_TRIPNO.Text = T00004row("TRIPNO")
            WF_DROPNO.Text = T00004row("DROPNO")

            '９段目 乗務員
            WF_STAFFCODE.Text = T00004row("STAFFCODE")
            WF_STAFFCODE_TEXT.Text = T00004row("STAFFCODENAME")
            WF_SUBSTAFFCODE.Text = T00004row("SUBSTAFFCODE")
            WF_SUBSTAFFCODE_TEXT.Text = T00004row("SUBSTAFFCODENAME")
            WF_STTIME.Text = T00004row("STTIME")

            '非表示 JXORDER
            WF_JXORDERID.Text = T00004row("JXORDERID")
            WF_JXORDERSTATUS.Text = T00004row("JXORDERSTATUS")

            For i As Integer = 0 To T00004INP.Count - 1
                T00004INP(i).Item("WORK_NO") = i

                ''○名称付与
                'CODENAME_set(T00004INP(i))
            Next
            '編集済みINPデータをINPtblに設定
            T00004INPtbl = T00004INP.CopyToDataTable

            '○光英データ更新制限設定
            If Not String.IsNullOrEmpty(T00004row("JXORDERID")) Then

                '○detailboxヘッダー
                WF_Sel_LINECNT.Enabled = False
                WF_SHUKODATE.Enabled = False
                WF_SHUKADATE.Enabled = False
                WF_TODOKEDATE.Enabled = False
                WF_KIKODATE.Enabled = False
                WF_RYOME.Enabled = False
                WF_ORDERNO.Enabled = False
                WF_DETAILNO.Enabled = False
                WF_SHIPORG.Enabled = False
                WF_TORICODE.Enabled = False
                WF_OILTYPE.Enabled = False
                WF_STORICODE.Enabled = False
                WF_ORDERORG.Enabled = False
                WF_URIKBN.Enabled = False
                WF_GSHABAN.Enabled = False
                WF_TSHABANF.Enabled = False
                WF_TSHABANB.Enabled = False
                WF_TSHABANB2.Enabled = False
                WF_CONTCHASSIS.Enabled = False
                WF_SHAFUKU.Enabled = False
                WF_TUMIOKIKBN.Enabled = False
                WF_TRIPNO.Enabled = False
                WF_DROPNO.Enabled = False

                WF_IsKoueiData.Value = "1"

            Else

                WF_Sel_LINECNT.Enabled = True
                WF_SHUKODATE.Enabled = True
                WF_SHUKADATE.Enabled = True
                WF_TODOKEDATE.Enabled = True
                WF_KIKODATE.Enabled = True
                WF_RYOME.Enabled = True
                WF_ORDERNO.Enabled = True
                WF_DETAILNO.Enabled = True
                WF_SHIPORG.Enabled = True
                WF_TORICODE.Enabled = True
                WF_OILTYPE.Enabled = True
                WF_STORICODE.Enabled = True
                WF_ORDERORG.Enabled = True
                WF_URIKBN.Enabled = True
                WF_GSHABAN.Enabled = True
                WF_TSHABANF.Enabled = True
                WF_TSHABANB.Enabled = True
                WF_TSHABANB2.Enabled = True
                WF_CONTCHASSIS.Enabled = True
                WF_SHAFUKU.Enabled = True
                WF_TUMIOKIKBN.Enabled = True
                WF_TRIPNO.Enabled = True
                WF_DROPNO.Enabled = True

                WF_IsKoueiData.Value = "0"
            End If
        End If

        '○光英データ
        If String.IsNullOrEmpty(WF_JXORDERID.Text) Then

            '追記行（空行）を4件作成
            For i As Integer = 1 To 4
                Dim T00004INProw = T00004INPtbl.NewRow()
                T00004INProw("SELECT") = 1
                T00004INProw("HIDDEN") = 1
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                T00004INProw("TIMSTP") = 0
                T00004INProw("LINECNT") = WW_LINECNT

                T00004INProw("CAMPCODE") = ""
                T00004INProw("TERMORG") = ""
                T00004INProw("TORICODE") = ""
                T00004INProw("OILTYPE") = ""
                T00004INProw("STORICODE") = ""
                T00004INProw("ORDERORG") = ""
                T00004INProw("KIJUNDATE") = ""
                T00004INProw("SHUKADATE") = ""
                T00004INProw("URIKBN") = ""
                T00004INProw("SHIPORG") = ""

                T00004INProw("TUMIOKI") = ""
                T00004INProw("INDEX") = ""
                T00004INProw("CAMPCODENAME") = ""
                T00004INProw("TERMORGNAME") = ""
                T00004INProw("ORDERNO") = ""
                T00004INProw("DETAILNO") = ""
                T00004INProw("TRIPNO") = ""
                T00004INProw("DROPNO") = ""
                T00004INProw("SEQ") = ""
                T00004INProw("TORICODENAME") = ""
                T00004INProw("OILTYPENAME") = ""
                T00004INProw("STORICODENAME") = ""
                T00004INProw("ORDERORGNAME") = ""
                T00004INProw("SHUKODATE") = ""
                T00004INProw("KIKODATE") = ""
                T00004INProw("TUMIOKIKBN") = ""
                T00004INProw("TUMIOKIKBNNAME") = ""
                T00004INProw("URIKBNNAME") = ""
                T00004INProw("STATUS") = ""
                T00004INProw("STATUSNAME") = ""
                T00004INProw("SHIPORGNAME") = ""
                T00004INProw("SHUKABASHO") = ""
                T00004INProw("SHUKABASHONAME") = ""
                T00004INProw("INTIME") = ""
                T00004INProw("OUTTIME") = ""
                T00004INProw("SHUKADENNO") = ""
                T00004INProw("TUMISEQ") = ""
                T00004INProw("TUMIBA") = ""
                T00004INProw("GATE") = ""
                T00004INProw("GSHABAN") = ""
                T00004INProw("GSHABANLICNPLTNO") = ""
                T00004INProw("RYOME") = ""
                T00004INProw("CONTCHASSIS") = ""
                T00004INProw("CONTCHASSISLICNPLTNO") = ""
                T00004INProw("SHAFUKU") = ""
                T00004INProw("STAFFCODE") = ""
                T00004INProw("STAFFCODENAME") = ""
                T00004INProw("SUBSTAFFCODE") = ""
                T00004INProw("SUBSTAFFCODENAME") = ""
                T00004INProw("STTIME") = ""
                T00004INProw("TORIORDERNO") = ""
                T00004INProw("TODOKEDATE") = ""
                T00004INProw("TODOKETIME") = ""
                T00004INProw("TODOKECODE") = ""
                T00004INProw("TODOKECODENAME") = ""
                T00004INProw("PRODUCT1") = ""
                T00004INProw("PRODUCT1NAME") = ""
                T00004INProw("PRODUCT2") = ""
                T00004INProw("PRODUCTCODE") = ""
                T00004INProw("PRODUCTNAME") = ""
                T00004INProw("PRATIO") = ""
                T00004INProw("SMELLKBN") = ""
                T00004INProw("SMELLKBNNAME") = ""
                T00004INProw("CONTNO") = ""
                T00004INProw("HTANI") = ""
                T00004INProw("HTANINAME") = ""
                T00004INProw("SURYO") = ""
                T00004INProw("SURYO_SUM") = ""
                T00004INProw("DAISU") = ""
                T00004INProw("DAISU_SUM") = ""
                T00004INProw("JSURYO") = ""
                T00004INProw("JSURYO_SUM") = ""
                T00004INProw("JDAISU") = ""
                T00004INProw("JDAISU_SUM") = ""
                T00004INProw("REMARKS1") = ""
                T00004INProw("REMARKS2") = ""
                T00004INProw("REMARKS3") = ""
                T00004INProw("REMARKS4") = ""
                T00004INProw("REMARKS5") = ""
                T00004INProw("REMARKS6") = ""
                T00004INProw("SHARYOTYPEF") = ""
                T00004INProw("TSHABANF") = ""
                T00004INProw("SHARYOTYPEB") = ""
                T00004INProw("TSHABANB") = ""
                T00004INProw("SHARYOTYPEB2") = ""
                T00004INProw("TSHABANB2") = ""
                T00004INProw("TAXKBN") = ""
                T00004INProw("TAXKBNNAME") = ""
                T00004INProw("DELFLG") = ""

                T00004INProw("ADDR") = ""
                T00004INProw("DISTANCE") = ""
                T00004INProw("ARRIVTIME") = ""
                T00004INProw("NOTES1") = ""
                T00004INProw("NOTES2") = ""
                T00004INProw("NOTES3") = ""
                T00004INProw("NOTES4") = ""
                T00004INProw("NOTES5") = ""
                T00004INProw("NOTES6") = ""
                T00004INProw("NOTES7") = ""
                T00004INProw("NOTES8") = ""
                T00004INProw("NOTES9") = ""
                T00004INProw("NOTES10") = ""
                T00004INProw("STAFFNOTES1") = ""
                T00004INProw("STAFFNOTES2") = ""
                T00004INProw("STAFFNOTES3") = ""
                T00004INProw("STAFFNOTES4") = ""
                T00004INProw("STAFFNOTES5") = ""

                T00004INProw("SHARYOINFO1") = ""
                T00004INProw("SHARYOINFO2") = ""
                T00004INProw("SHARYOINFO3") = ""
                T00004INProw("SHARYOINFO4") = ""
                T00004INProw("SHARYOINFO5") = ""
                T00004INProw("SHARYOINFO6") = ""

                T00004INProw("WORK_NO") = ""

                T00004INProw("JXORDERID") = ""
                T00004INProw("JXORDERSTATUS") = ""

                T00004INPtbl.Rows.Add(T00004INProw)
            Next
        End If

        '○Detail初期設定
        Repeater_INIT()

        '■画面WF_GRID状態設定

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        EditOperationText(T00004tbl, WW_LINECNT)

        '○ 画面表示データ保存
        Master.SaveTable(T00004tbl)

        'カーソル設定
        WF_FIELD.Value = "WF_SHUKADATE"
        WF_SHUKODATE.Focus()
        WF_REP_Change.Value = ""         'リピータ変更監視

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

    End Sub

    ''' <summary>
    ''' 詳細画面項目クリア処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ClearDetailBox()

        '○detailboxヘッダークリア
        '出庫日
        WF_SHUKODATE.Text = ""
        '出荷日
        WF_SHUKADATE.Text = ""
        '届日
        WF_TODOKEDATE.Text = ""
        '帰庫日
        WF_KIKODATE.Text = ""

        WF_RYOME.Text = ""
        WF_ORDERNO.Text = ""
        WF_DETAILNO.Text = ""

        WF_SHIPORG.Text = ""
        WF_SHIPORG_TEXT.Text = ""
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_OILTYPE.Text = ""
        WF_OILTYPE_TEXT.Text = ""
        WF_STORICODE.Text = ""
        WF_STORICODE_TEXT.Text = ""
        WF_ORDERORG.Text = ""
        WF_ORDERORG_TEXT.Text = ""
        WF_URIKBN.Text = ""
        WF_URIKBN_TEXT.Text = ""

        '業務車番
        WF_GSHABAN.Text = ""
        'WF_GSHABAN_TEXT.Text = ""
        WF_TSHABANF.Text = ""
        WF_TSHABANF_TEXT.Text = ""
        WF_TSHABANB.Text = ""
        WF_TSHABANB_TEXT.Text = ""
        WF_TSHABANB2.Text = ""
        WF_TSHABANB2_TEXT.Text = ""
        'コンテナシャーシ
        WF_CONTCHASSIS.Text = ""
        WF_CONTCHASSIS_TEXT.Text = ""
        '車腹
        WF_SHAFUKU.Text = ""

        '積置区分
        WF_TUMIOKIKBN.Text = ""
        WF_TUMIOKIKBN_TEXT.Text = ""
        'トリップ
        WF_TRIPNO.Text = ""
        'ドロップ
        WF_DROPNO.Text = ""

        '乗務員
        WF_STAFFCODE.Text = ""
        WF_STAFFCODE_TEXT.Text = ""
        '副乗務員
        WF_SUBSTAFFCODE.Text = ""
        WF_SUBSTAFFCODE_TEXT.Text = ""
        '出勤時間
        WF_STTIME.Text = ""
        'JXORDER
        WF_JXORDERID.Text = ""
        WF_JXORDERSTATUS.Text = ""

        WF_Sel_LINECNT.Text = ""

        WF_IsKoueiData.Value = ""
    End Sub

    ''' <summary>
    ''' 明細行 編集処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    WF_ITEM_FORMAT = CInt(I_VALUE).ToString("00")
                Catch ex As Exception
                End Try
            Case Else
        End Select
    End Function

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        Try
            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.TABID = CONST_DETAIL_TABID
            CS0052DetailView.SRCDATA = T00004INPtbl
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Master.Output(CS0052DetailView.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If

            'リピータの１明細の行数を保存
            WF_REP_ROWSCNT.Value = CS0052DetailView.ROWMAX
            WF_REP_COLSCNT.Value = CS0052DetailView.COLMAX

            Dim WW_T00004INPcnt As Integer
            WW_T00004INPcnt = T00004INPtbl.Select("TORICODE <> ''").Count

            WF_DetailMView.ActiveViewIndex = 0
            For row As Integer = 0 To (T00004INPtbl.Rows.Count * CS0052DetailView.ROWMAX) - 1
                If (row + 1) <= (CS0052DetailView.ROWMAX * WW_T00004INPcnt) Then
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text =
                        ((row \ CS0052DetailView.ROWMAX) + 1).ToString("000")
                Else
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text = ""
                End If
                Dim WW_RepeaterLINE = CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINEPOSITION"), System.Web.UI.WebControls.TextBox)

                For col As Integer = 1 To CS0052DetailView.COLMAX

                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then

                        repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label)
                        repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox)
                        repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_TEXT_" & col), System.Web.UI.WebControls.Label)

                        '値（名称）設定
                        CODENAME_get(repField.Text, repValue.Text, repName.Text, WW_DUMMY)

                        'ダブルクリック時コード検索イベント追加
                        REP_ATTR_get(repField.Text, WW_RepeaterLINE.Text, repAttr)
                        If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", repAttr)
                            repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), System.Web.UI.WebControls.Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                        repValue.Attributes.Remove("onchange")
                        repValue.Attributes.Add("onchange", "f_Rep1_Change(2)")

                        '○光英データ更新制限設定
                        If String.IsNullOrEmpty(T00004INPtbl.Rows(0)("JXORDERID")) Then
                            repValue.Enabled = True
                        Else
                            repValue.Enabled = False
                        End If

                    End If

                Next col

                '■■■ LINE表示設定（1明細目の最終行） ■■■
                If (CS0052DetailView.ROWMAX - 1) > 0 And ((row + 1) Mod CS0052DetailView.ROWMAX) = 0 Then
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINE"), System.Web.UI.WebControls.Label).Style.Remove("display")
                    CType(WF_DViewRep1.Items(row).FindControl("WF_Rep1_LINE"), System.Web.UI.WebControls.Label).Style.Add("display", "block")
                End If
            Next row

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_ATTR">イベント内容</param>
    ''' <remarks></remarks>
    Protected Sub REP_ATTR_get(ByVal I_FIELD As String, ByVal I_INDEX As String, ByRef O_ATTR As String)

        O_ATTR = "Repeater_Gyou(" & I_INDEX & ");"
        Select Case I_FIELD
            Case "SHUKODATE"
                '出庫日
                O_ATTR &= "REF_Field_DBclick('SHUKODATE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "KIKODATE"
                '帰庫日
                O_ATTR &= "REF_Field_DBclick('KIKODATE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_CALENDAR & ");"
            Case "TUMIOKIKBN"
                '積置区分
                O_ATTR &= "REF_Field_DBclick('TUMIOKIKBN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & ");"
            Case "PRODUCT1"
                '品名１
                O_ATTR &= "REF_Field_DBclick('PRODUCT1', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "PRODUCT2"
                '品名２
                O_ATTR &= "REF_Field_DBclick('PRODUCT2', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "PRODUCTCODE"
                '品名コード
                O_ATTR &= "REF_Field_DBclick('PRODUCTCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_GOODS & ");"
            Case "SMELLKBN"
                '臭有無
                O_ATTR &= "REF_Field_DBclick('SMELLKBN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & ");"
            Case "SHUKABASHO"
                '出荷場所
                O_ATTR &= "REF_Field_DBclick('SHUKABASHO', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "SHIPORG"
                '出荷部署
                O_ATTR &= "REF_Field_DBclick('SHIPORG', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_ORG & ");"
            Case "GSHABAN"
                '業務車番
                O_ATTR &= "REF_Field_DBclick('GSHABAN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "CONTCHASSIS"
                'コンテナシャーシ
                O_ATTR &= "REF_Field_DBclick('CONTCHASSIS', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "STAFFCODE"
                '乗務員コード
                O_ATTR &= "REF_Field_DBclick('STAFFCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "SUBSTAFFCODE"
                '副乗務員コード
                O_ATTR &= "REF_Field_DBclick('SUBSTAFFCODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");"
            Case "TODOKECODE"
                '届先コード
                O_ATTR &= "REF_Field_DBclick('TODOKECODE', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DISTINATION & ");"
            Case "DELFLG"
                '削除
                O_ATTR &= "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ");"
            Case "HTANI"
                '配送単位
                O_ATTR &= "REF_Field_DBclick('HTANI', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & ");"
            Case "TAXKBN"
                '税区分
                O_ATTR &= "REF_Field_DBclick('TAXKBN', 'WF_Rep_FIELD' , " & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & ");"
            Case Else
                O_ATTR = String.Empty
        End Select


    End Sub

    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            If Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value) Then
                rightview.SelectIndex(WF_RightViewChange.Value)
                WF_RightViewChange.Value = ""
            End If
        End If
    End Sub

    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' Operation項目編集処理
    ''' </summary>
    ''' <param name="I_ROW" >行データ</param>
    ''' <param name="I_SEL" >TRUE:選択編集（"★"付加）| FALSE:選択解除（"★"除外）</param>
    ''' <param name="O_ROW" >編集後行データ</param>
    ''' <remarks></remarks>
    Protected Sub EditOperationText(ByRef I_ROW As DataRow, ByVal I_SEL As Boolean, Optional ByRef O_ROW As DataRow = Nothing)
        Dim outRow As DataRow
        If IsNothing(O_ROW) Then
            outRow = I_ROW
        Else
            outRow = O_ROW
        End If

        If I_SEL = True Then
            outRow("OPERATION") = I_ROW("OPERATION").ToString.Insert(0, C_LIST_OPERATION_CODE.SELECTED)
        Else
            outRow("OPERATION") = I_ROW("OPERATION").ToString.Replace(C_LIST_OPERATION_CODE.SELECTED, "")
        End If

    End Sub

    ''' <summary>
    ''' Operation項目編集処理
    ''' </summary>
    ''' <param name="I_TBL" >データテーブル</param>
    ''' <param name="I_SEL" >TRUE:選択編集（"★"付加）| FALSE:選択解除（"★"除外）</param>
    ''' <param name="I_LINECNT" >行指定</param>
    ''' <remarks></remarks>
    Protected Sub EditOperationText(ByRef I_TBL As DataTable, ByVal I_SEL As Boolean, Optional ByVal I_LINECNT As String = "")

        For Each row As DataRow In I_TBL.Rows
            If Not String.IsNullOrEmpty(I_LINECNT) Then
                '行位置指定時はその行のみ選択状態
                If row("LINECNT") = I_LINECNT Then
                    EditOperationText(row, True)
                Else
                    EditOperationText(row, False)
                End If
            Else
                EditOperationText(row, I_SEL)
            End If
        Next
    End Sub

#Region "T0003テーブル関連"

    ''' <summary>
    ''' T00003tbl関連データ削除
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks>更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。</remarks>
    Protected Sub DBupdate_T3DELETE(ByRef T00004UPDrow As DataRow, ByVal I_DATENOW As Date, ByVal O_RTN As String)

        '○T00004UPDtbl関連の荷主受注を論理削除
        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･荷主受注の現受注番号を一括論理削除
            Dim SQLStr As String =
                        " UPDATE T0003_NIORDER            " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND TORICODE    = @P02       " _
                    & "    AND OILTYPE     = @P03       " _
                    & "    AND ORDERORG    = @P04       " _
                    & "    AND SHIPORG     = @P05       " _
                    & "    AND KIJUNDATE   = @P06       " _
                    & "    AND DELFLG     <> '1'        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = T00004UPDrow("CAMPCODE")
            PARA02.Value = T00004UPDrow("TORICODE")
            PARA03.Value = T00004UPDrow("OILTYPE")
            PARA04.Value = T00004UPDrow("ORDERORG")
            PARA05.Value = T00004UPDrow("SHIPORG")
            PARA06.Value = T00004UPDrow("KIJUNDATE")

            PARA11.Value = I_DATENOW
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0003_HORDER(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0003_HORDER(old) DEL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' T00003tbl追加
    ''' </summary>
    ''' <param name="I_DATENOW" >T3更新時刻</param>
    ''' <param name="O_RTN" >ERR</param>
    ''' <remarks></remarks>
    Protected Sub DBupdate_T3INSERT(ByVal I_DATENOW As Date, ByRef O_RTN As String)

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_ORDERORG As String = ""
        Dim WW_SHIPORG As String = ""

        '■■■ T00004SUMtblより荷主受注追加 ■■■
        '
        '〇荷主受注DB登録
        Dim SQLStr As String =
                   " INSERT INTO T0003_NIORDER                  " _
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
                 & "              TAXKBN,                       " _
                 & "              PRODUCTCODE)                  " _
                 & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,     " _
                 & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,     " _
                 & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,     " _
                 & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,     " _
                 & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50,     " _
                 & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60,     " _
                 & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68,@P69);         "

        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 15)
        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 10)
        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 2)
        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 25)
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
        Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", System.Data.SqlDbType.NVarChar, 30)

        For Each T00004SUMrow In T00004SUMtbl.Rows

            If T00004SUMrow("DELFLG") = "0" AndAlso
                (T00004SUMrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse T00004SUMrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING) Then
                Try

                    PARA01.Value = T00004SUMrow("CAMPCODE")                           '会社コード(CAMPCODE)
                    PARA02.Value = T00004SUMrow("TERMORG")                            '端末設置部署(TERMORG)
                    PARA03.Value = T00004SUMrow("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
                    PARA04.Value = T00004SUMrow("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
                    PARA05.Value = T00004SUMrow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
                    PARA06.Value = T00004SUMrow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
                    PARA07.Value = T00004SUMrow("SEQ").PadLeft(2, "0")                '枝番(SEQ)
                    PARA08.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")            'エントリー日時(ENTRYDATE)
                    PARA09.Value = T00004SUMrow("TORICODE")                           '取引先コード(TORICODE)
                    PARA10.Value = T00004SUMrow("OILTYPE")                            '油種(OILTYPE)
                    PARA11.Value = T00004SUMrow("STORICODE")                          '請求取引先コード(STORICODE)
                    PARA12.Value = T00004SUMrow("ORDERORG")                           '受注受付部署(ORDERORG)
                    If T00004SUMrow("SHUKODATE") = "" Then                            '出庫日(SHUKODATE)
                        PARA13.Value = "2000/01/01"
                    Else
                        PARA13.Value = RTrim(T00004SUMrow("SHUKODATE"))
                    End If
                    If T00004SUMrow("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
                        PARA14.Value = "2000/01/01"
                    Else
                        PARA14.Value = RTrim(T00004SUMrow("KIKODATE"))
                    End If
                    If T00004SUMrow("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
                        PARA15.Value = "2000/01/01"
                    Else
                        PARA15.Value = RTrim(T00004SUMrow("SHUKADATE"))
                    End If
                    PARA16.Value = T00004SUMrow("TUMIOKIKBN")                         '積置区分(TUMIOKIKBN)
                    PARA17.Value = T00004SUMrow("URIKBN")                             '売上計上基準(URIKBN)
                    PARA18.Value = T00004SUMrow("STATUS")                             '状態(STATUS)
                    PARA19.Value = T00004SUMrow("SHIPORG")                            '出荷部署(SHIPORG)
                    PARA20.Value = T00004SUMrow("SHUKABASHO")                         '出荷場所(SHUKABASHO)
                    PARA21.Value = T00004SUMrow("INTIME")                             '時間指定（入構）(INTIME)
                    PARA22.Value = T00004SUMrow("OUTTIME")                            '時間指定（出構）(OUTTIME)
                    PARA23.Value = T00004SUMrow("SHUKADENNO")                         '出荷伝票番号(SHUKADENNO)
                    If String.IsNullOrWhiteSpace(RTrim(T00004SUMrow("TUMISEQ"))) Then '積順(TUMISEQ)
                        PARA24.Value = 0
                    Else
                        PARA24.Value = T00004SUMrow("TUMISEQ")
                    End If
                    PARA25.Value = T00004SUMrow("TUMIBA")                             '積場(TUMIBA)
                    PARA26.Value = T00004SUMrow("GATE")                               'ゲート(GATE)
                    PARA27.Value = T00004SUMrow("GSHABAN")                            '業務車番(GSHABAN)
                    PARA28.Value = T00004SUMrow("RYOME")                              '両目(RYOME)
                    PARA29.Value = T00004SUMrow("CONTCHASSIS")                        'コンテナシャーシ(CONTCHASSIS)
                    If String.IsNullOrWhiteSpace(RTrim(T00004SUMrow("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
                        PARA30.Value = 0.0
                    Else
                        PARA30.Value = CType(T00004SUMrow("SHAFUKU"), Double)
                    End If
                    PARA31.Value = T00004SUMrow("STAFFCODE")                          '乗務員コード(STAFFCODE)
                    PARA32.Value = T00004SUMrow("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
                    PARA33.Value = T00004SUMrow("STTIME")                             '出勤時間(STTIME)
                    PARA34.Value = ""                                                 '荷主受注番号(TORIORDERNO)
                    If RTrim(T00004SUMrow("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
                        PARA35.Value = "2000/01/01"
                    Else
                        PARA35.Value = RTrim(T00004SUMrow("TODOKEDATE"))
                    End If
                    PARA36.Value = T00004SUMrow("TODOKETIME")                         '時間指定（配送）(TODOKETIME)
                    PARA37.Value = T00004SUMrow("TODOKECODE")                         '届先コード(TODOKECODE)
                    PARA38.Value = T00004SUMrow("PRODUCT1")                           '品名１(PRODUCT1)
                    PARA39.Value = T00004SUMrow("PRODUCT2")                           '品名２(PRODUCT2)
                    If String.IsNullOrWhiteSpace(RTrim(T00004SUMrow("PRATIO"))) Then  'Ｐ比率(PRATIO)
                        PARA40.Value = 0.0
                    Else
                        PARA40.Value = CType(T00004SUMrow("PRATIO"), Double)
                    End If
                    PARA41.Value = T00004SUMrow("SMELLKBN")                           '臭有無(SMELLKBN)
                    PARA42.Value = T00004SUMrow("CONTNO")                             'コンテナ番号(CONTNO)
                    If String.IsNullOrWhiteSpace(RTrim(T00004SUMrow("SURYO"))) Then   '数量(SURYO)
                        PARA43.Value = 0.0
                    Else
                        PARA43.Value = CType(T00004SUMrow("SURYO"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(T00004SUMrow("DAISU"))) Then   '台数(DAISU)
                        PARA44.Value = 0
                    Else
                        PARA44.Value = CType(T00004SUMrow("DAISU"), Double)
                    End If
                    PARA45.Value = 0.0                                                '配送実績数量(JSURYO)
                    PARA46.Value = 0                                                  '配送実績台数(JDAISU)
                    PARA47.Value = T00004SUMrow("REMARKS1")                           '備考１(REMARKS1)
                    PARA48.Value = T00004SUMrow("REMARKS2")                           '備考２(REMARKS2)
                    PARA49.Value = T00004SUMrow("REMARKS3")                           '備考３(REMARKS3)
                    PARA50.Value = T00004SUMrow("REMARKS4")                           '備考４(REMARKS4)
                    PARA51.Value = T00004SUMrow("REMARKS5")                           '備考５(REMARKS5)
                    PARA52.Value = T00004SUMrow("REMARKS6")                           '備考６(REMARKS6)
                    PARA53.Value = T00004SUMrow("DELFLG")                             '削除フラグ(DELFLG)
                    PARA54.Value = I_DATENOW                                          '登録年月日(INITYMD)
                    PARA55.Value = I_DATENOW                                          '更新年月日(UPDYMD)
                    PARA56.Value = Master.USERID                                      '更新ユーザＩＤ(UPDUSER)
                    PARA57.Value = Master.USERTERMID                                  '更新端末(UPDTERMID)
                    PARA58.Value = C_DEFAULT_YMD                                      '集信日時(RECEIVEYMD)
                    '基準日＝出荷日 
                    If T00004SUMrow("KIJUNDATE") = "" Then                            '基準日(KIJUNDATE)
                        PARA59.Value = "2000/01/01"
                    Else
                        PARA59.Value = RTrim(T00004SUMrow("KIJUNDATE"))
                    End If
                    PARA60.Value = T00004SUMrow("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
                    PARA61.Value = T00004SUMrow("TSHABANF")                           '統一車番前(TSHABANF)
                    PARA62.Value = T00004SUMrow("SHARYOTYPEB")                        '統一車番前(SHARYOTYPEB)
                    PARA63.Value = T00004SUMrow("TSHABANB")                           '統一車番前(TSHABANB)
                    PARA64.Value = T00004SUMrow("SHARYOTYPEB2")                       '統一車番前(SHARYOTYPEB2)
                    PARA65.Value = T00004SUMrow("TSHABANB2")                          '統一車番前(TSHABANB2)
                    PARA66.Value = T00004SUMrow("HTANI")                              '配送単位(HTANI)
                    PARA67.Value = ""                                                 '配送実績単位(STANI)
                    PARA68.Value = T00004SUMrow("TAXKBN")                             '税区分(TAXKBN)
                    PARA69.Value = T00004SUMrow("PRODUCTCODE")                        '品名コード(PRODUCTCODE)

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()


                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0003_NIORDER INSERT")
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0003_NIORDER INSERT"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub

                End Try

            End If

        Next

        'CLOSE
        SQLcmd.Dispose()
        SQLcmd = Nothing
        SQLcon.Close()
        SQLcon.Dispose()

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

#End Region

#Region "T0004テーブル関連"
    ''' <summary>
    ''' 画面表示用データ取得
    ''' </summary>
    ''' <remarks>データベース（T00004）を検索し画面表示用データを取得する</remarks>
    Private Sub DBselect_T4SELECT()

        Dim WW_DATE As Date
        Dim WW_TIME As DateTime

        '〇GridView内容をテーブル退避
        'T00004テンポラリDB項目作成
        If T00004tbl Is Nothing Then
            T00004tbl = New DataTable
        End If

        If T00004tbl.Columns.Count = 0 Then
        Else
            T00004tbl.Columns.Clear()
        End If

        '○DB項目クリア
        T00004tbl.Clear()

        '〇画面表示用データ取得
        Try

            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT 0                                     as LINECNT ,        " _
                   & "       ''                                    as OPERATION ,      " _
                   & "       '0'                                   as 'SELECT' ,       " _
                   & "       '0'                                   as HIDDEN ,         " _
                   & "       ''                                    as 'INDEX' ,        " _
                   & "       isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
                   & "       isnull(rtrim(A.TERMORG),'')           as TERMORG ,        " _
                   & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
                   & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
                   & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
                   & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
                   & "       isnull(rtrim(A.SEQ),'00')             as SEQ ,            " _
                   & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
                   & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
                   & "       isnull(rtrim(A.STORICODE),'')         as STORICODE ,      " _
                   & "       isnull(rtrim(A.ORDERORG),'')          as ORDERORG ,       " _
                   & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,      " _
                   & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,       " _
                   & "       isnull(rtrim(A.KIJUNDATE),'')         as KIJUNDATE ,      " _
                   & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,      " _
                   & "       isnull(rtrim(A.TUMIOKIKBN),'')        as TUMIOKIKBN ,     " _
                   & "       isnull(rtrim(A.URIKBN),'')            as URIKBN ,         " _
                   & "       isnull(rtrim(A.STATUS),'')            as STATUS ,         " _
                   & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
                   & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
                   & "       isnull(rtrim(A.INTIME),'')            as INTIME ,         " _
                   & "       isnull(rtrim(A.OUTTIME),'')           as OUTTIME ,        " _
                   & "       isnull(rtrim(A.SHUKADENNO),'')        as SHUKADENNO ,     " _
                   & "       isnull(rtrim(A.TUMISEQ),'')           as TUMISEQ ,        " _
                   & "       isnull(rtrim(A.TUMIBA),'')            as TUMIBA ,         " _
                   & "       isnull(rtrim(A.GATE),'')              as GATE ,           " _
                   & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
                   & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
                   & "       isnull(rtrim(A.CONTCHASSIS),'')       as CONTCHASSIS ,    " _
                   & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
                   & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
                   & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
                   & "       isnull(rtrim(A.STTIME),'')            as STTIME ,         " _
                   & "       isnull(rtrim(A.TORIORDERNO),'')       as TORIORDERNO ,    " _
                   & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,     " _
                   & "       isnull(rtrim(A.TODOKETIME),'')        as TODOKETIME ,     " _
                   & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
                   & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
                   & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
                   & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,    " _
                   & "       isnull(rtrim(A.PRATIO),'')            as PRATIO ,         " _
                   & "       isnull(rtrim(A.SMELLKBN),'')          as SMELLKBN ,       " _
                   & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
                   & "       isnull(rtrim(A.HTANI),'')             as HTANI ,          " _
                   & "       isnull(rtrim(A.SURYO),'')             as SURYO ,          " _
                   & "       isnull(rtrim(A.DAISU),'')             as DAISU ,          " _
                   & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
                   & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
                   & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
                   & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
                   & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
                   & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
                   & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
                   & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
                   & "       isnull(rtrim(A.TAXKBN),'')            as TAXKBN ,         " _
                   & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
                   & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
                   & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
                   & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
                   & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
                   & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
                   & "       isnull(rtrim(A.JXORDERID),'')         as JXORDERID ,         " _
                   & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,         " _
                   & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,        " _
                   & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,    " _
                   & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,    " _
                   & "       isnull(rtrim(C.ARRIVTIME),'')         as ARRIVTIME ,      " _
                   & "       isnull(rtrim(C.DISTANCE),'')          as DISTANCE ,       " _
                   & "       isnull(rtrim(D.ADDR1),'') +              				   " _
                   & "       isnull(rtrim(D.ADDR2),'') +            				   " _
                   & "       isnull(rtrim(D.ADDR3),'') +             				   " _
                   & "       isnull(rtrim(D.ADDR4),'')          	as ADDR ,          " _
                   & "       isnull(rtrim(D.NOTES1),'')        	    as NOTES1 ,        " _
                   & "       isnull(rtrim(D.NOTES2),'')          	as NOTES2 ,        " _
                   & "       isnull(rtrim(D.NOTES3),'')          	as NOTES3 ,        " _
                   & "       isnull(rtrim(D.NOTES4),'')          	as NOTES4 ,        " _
                   & "       isnull(rtrim(D.NOTES5),'')          	as NOTES5 ,        " _
                   & "       isnull(rtrim(D.NOTES6),'')        	    as NOTES6 ,        " _
                   & "       isnull(rtrim(D.NOTES7),'')          	as NOTES7 ,        " _
                   & "       isnull(rtrim(D.NOTES8),'')          	as NOTES8 ,        " _
                   & "       isnull(rtrim(D.NOTES9),'')          	as NOTES9 ,        " _
                   & "       isnull(rtrim(D.NOTES10),'')          	as NOTES10 ,       " _
                   & "       isnull(rtrim(E.NOTES1),'')        	    as STAFFNOTES1 ,   " _
                   & "       isnull(rtrim(E.NOTES2),'')          	as STAFFNOTES2 ,   " _
                   & "       isnull(rtrim(E.NOTES3),'')          	as STAFFNOTES3 ,   " _
                   & "       isnull(rtrim(E.NOTES4),'')          	as STAFFNOTES4 ,   " _
                   & "       isnull(rtrim(E.NOTES5),'')          	as STAFFNOTES5 ,   " _
                   & "       ''                                    as TUMIOKI ,        " _
                   & "       ''                                    as CAMPCODENAME ,   " _
                   & "       ''                                    as TERMORGNAME ,    " _
                   & "       ''                                    as TORICODENAME ,   " _
                   & "       ''                                    as OILTYPENAME ,    " _
                   & "       ''                                    as STORICODENAME ,  " _
                   & "       ''                                    as ORDERORGNAME ,   " _
                   & "       ''                                    as TUMIOKIKBNNAME , " _
                   & "       ''                                    as URIKBNNAME ,     " _
                   & "       ''                                    as STATUSNAME ,     " _
                   & "       ''                                    as SHIPORGNAME ,    " _
                   & "       ''                                    as SHUKABASHONAME , " _
                   & "       ''                                    as GSHABANLICNPLTNO ,    " _
                   & "       ''                                    as CONTCHASSISLICNPLTNO ,    " _
                   & "       ''                                    as STAFFCODENAME ,    " _
                   & "       ''                                    as SUBSTAFFCODENAME ,    " _
                   & "       ''                                    as TODOKECODENAME ,    " _
                   & "       ''                                    as HTANINAME ,    " _
                   & "       ''                                    as TAXKBNNAME ,    " _
                   & "       ''                                    as PRODUCT1NAME ,   " _
                   & "       ''                                    as PRODUCT2NAME ,   " _
                   & "       ''                                    as PRODUCTNAME ,   " _
                   & "       ''                                    as SMELLKBNNAME ,   " _
                   & "       ''                                    as SURYO_SUM ,      " _
                   & "       ''                                    as DAISU_SUM ,      " _
                   & "       ''                                    as JSURYO_SUM ,      " _
                   & "       ''                                    as JDAISU_SUM ,      " _
                   & "       ''                                    as JXORDERSTATUS ,   " _
                   & "       '0'                                   as WORK_NO          " _
                   & "  FROM T0004_HORDER AS A								" _
                   & " INNER JOIN ( SELECT Y.CAMPCODE, Y.CODE               " _
                   & "                FROM S0006_ROLE Y     				" _
                   & "               WHERE Y.CAMPCODE 	 	   = @P01		" _
                   & "                 and Y.OBJECT       	   = 'ORG'		" _
                   & "                 and Y.ROLE              = @P02		" _
                   & "                 and Y.PERMITCODE       in ('1','2')  " _
                   & "                 and Y.STYMD            <= @P03		" _
                   & "                 and Y.ENDYMD           >= @P04		" _
                   & "                 and Y.DELFLG           <> '1'		" _
                   & "            ) AS Z									" _
                   & "    ON Z.CAMPCODE		= A.CAMPCODE    				" _
                   & "   and Z.CODE       	= A.SHIPORG 	    			" _
                   & "  LEFT JOIN MA006_SHABANORG B							" _
                   & "    ON B.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and B.GSHABAN      	= A.GSHABAN 				" _
                   & "   and B.MANGUORG     	= A.SHIPORG 				" _
                   & "   and B.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MC007_TODKORG C 							" _
                   & "    ON C.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and C.TORICODE     	= A.TORICODE 				" _
                   & "   and C.TODOKECODE   	= A.TODOKECODE 				" _
                   & "   and C.UORG         	= A.SHIPORG 				" _
                   & "   and C.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MC006_TODOKESAKI D 						" _
                   & "    ON D.CAMPCODE     	= C.CAMPCODE 				" _
                   & "   and D.TORICODE     	= C.TORICODE				" _
                   & "   and D.TODOKECODE   	= C.TODOKECODE 				" _
                   & "   and D.STYMD           <= A.SHUKODATE				" _
                   & "   and D.ENDYMD          >= A.SHUKODATE				" _
                   & "   and D.DELFLG          <> '1' 						" _
                   & "  LEFT JOIN MB001_STAFF E      						" _
                   & "    ON E.CAMPCODE     	= A.CAMPCODE 				" _
                   & "   and E.STAFFCODE     	= A.STAFFCODE				" _
                   & "   and E.STYMD           <= A.SHUKODATE				" _
                   & "   and E.ENDYMD          >= A.SHUKODATE				" _
                   & "   and E.DELFLG          <> '1' 						" _
                   & " WHERE A.CAMPCODE         = @P01                      " _
                   & "   and A.SHUKADATE       <= @P05                      " _
                   & "   and A.SHUKADATE       >= @P06                      " _
                   & "   and A.TODOKEDATE      <= @P07                      " _
                   & "   and A.TODOKEDATE      >= @P08                      " _
                   & "   and A.SHUKODATE       <= @P09                      " _
                   & "   and A.SHUKODATE       >= @P10                      " _
                   & "   and A.DELFLG          <> '1'                       "

                '■テーブル検索条件追加

                '条件画面で指定された油種を抽出
                If work.WF_SEL_OILTYPE.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.OILTYPE          = @P12           		"
                End If

                '条件画面で指定された受注部署を抽出
                If work.WF_SEL_ORDERORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.ORDERORG         = @P13           		"
                End If

                '条件画面で指定された出荷部署を抽出
                If work.WF_SEL_SHIPORG.Text <> Nothing Then
                    SQLStr = SQLStr & "   and A.SHIPORG          = @P14           		"
                Else
                    '★★★未指定時はユーザ所属支店部署で縛る必要あり
                End If

                SQLStr = SQLStr & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.SHUKADATE ,      " _
                                & " 		 A.ORDERORG  ,A.SHIPORG ,	                " _
                                & " 		 A.SHUKODATE ,A.TODOKEDATE ,A.GSHABAN ,      " _
                                & " 		 A.RYOME     ,A.TRIPNO  ,A.DROPNO	 ,A.SEQ "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)      '権限(to)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)      '権限(from)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出荷日(To)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '出荷日(From)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)      '届日(To)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)      '届日(From)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)      '出庫日(To)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)      '出庫日(From)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)  '油種
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 20)  '受注部署
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                PARA01.Value = work.WF_SEL_CAMPCODE.Text
                PARA02.Value = Master.ROLE_ORG
                PARA03.Value = Date.Now
                PARA04.Value = Date.Now

                '出荷日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKADATET.Text) Then
                    PARA05.Value = C_MAX_YMD
                Else
                    PARA05.Value = work.WF_SEL_SHUKADATET.Text
                End If
                '出荷日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKADATEF.Text) Then
                    PARA06.Value = C_DEFAULT_YMD
                Else
                    PARA06.Value = work.WF_SEL_SHUKADATEF.Text
                End If
                '届日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_TODOKEDATET.Text) Then
                    PARA07.Value = C_MAX_YMD
                Else
                    PARA07.Value = work.WF_SEL_TODOKEDATET.Text
                End If
                '届日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_TODOKEDATEF.Text) Then
                    PARA08.Value = C_DEFAULT_YMD
                Else
                    PARA08.Value = work.WF_SEL_TODOKEDATEF.Text
                End If
                '出庫日(To)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKODATET.Text) Then
                    PARA09.Value = C_MAX_YMD
                Else
                    PARA09.Value = work.WF_SEL_SHUKODATET.Text
                End If
                '出庫日(From)
                If String.IsNullOrWhiteSpace(work.WF_SEL_SHUKODATEF.Text) Then
                    PARA10.Value = C_DEFAULT_YMD
                Else
                    PARA10.Value = work.WF_SEL_SHUKODATEF.Text
                End If

                PARA12.Value = work.WF_SEL_OILTYPE.Text
                PARA13.Value = work.WF_SEL_ORDERORG.Text
                PARA14.Value = work.WF_SEL_SHIPORG.Text

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                'フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next
                '〇テーブル検索結果をテーブル格納
                T00004tbl.Load(SQLdr)

                If T00004tbl.Rows.Count > CONST_DSPROW_MAX Then
                    'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
                    Master.Output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ABORT)
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)

                    T00004tbl.Clear()
                    Exit Sub
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try


        For Each T00004row In T00004tbl.Rows

            '○レコードの初期設定

            T00004row("LINECNT") = 0
            T00004row("SELECT") = 1   '1:表示
            T00004row("HIDDEN") = 0   '0:表示
            T00004row("INDEX") = ""
            T00004row("SEQ") = "00"
            T00004row("WORK_NO") = 0

            If Date.TryParse(T00004row("SHUKODATE"), WW_DATE) Then
                T00004row("SHUKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00004row("SHUKODATE") = ""
            End If

            If Date.TryParse(T00004row("KIKODATE"), WW_DATE) Then
                T00004row("KIKODATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00004row("KIKODATE") = ""
            End If

            If Date.TryParse(T00004row("KIJUNDATE"), WW_DATE) Then
                T00004row("KIJUNDATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00004row("KIJUNDATE") = ""
            End If

            If Date.TryParse(T00004row("SHUKADATE"), WW_DATE) Then
                T00004row("SHUKADATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00004row("SHUKADATE") = ""
            End If

            If Date.TryParse(T00004row("TODOKEDATE"), WW_DATE) Then
                T00004row("TODOKEDATE") = WW_DATE.ToString("yyyy/MM/dd")
            Else
                T00004row("TODOKEDATE") = ""
            End If

            If Date.TryParse(T00004row("ARRIVTIME"), WW_DATE) Then
                T00004row("ARRIVTIME") = WW_TIME.ToString("H:mm")
            Else
                T00004row("ARRIVTIME") = ""
            End If

            '品名コード未登録は会社・油種・品名１・品名２から編集
            If String.IsNullOrEmpty(T00004row("PRODUCTCODE")) Then
                T00004row("PRODUCTCODE") = T00004row("CAMPCODE") + T00004row("OILTYPE") + T00004row("PRODUCT1") + T00004row("PRODUCT2")
            End If

            'JXオーダーの場合はオーダーステータス設定
            If Not String.IsNullOrEmpty(T00004row("JXORDERID")) Then
                SetOrderStatus(T00004row)
            End If

            '○項目名称設定
            CODENAME_set(T00004row)

        Next

    End Sub

    ''' <summary>
    ''' T00004tbl追加
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DBupdate_T4INSERT(ByVal I_DATENOW As Date, ByRef O_RTN As String)

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_ORDERORG As String = ""
        Dim WW_SHIPORG As String = ""


        '■■■ T00004UPDtblより配送受注追加 ■■■
        '
        For Each T00004UPDrow In T00004UPDtbl.Rows

            If T00004UPDrow("DELFLG") = "0" AndAlso
                (T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING) Then
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
                             & "              TAXKBN,                       " _
                             & "              PRODUCTCODE,                  " _
                             & "              JXORDERID)                    " _
                             & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,     " _
                             & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,     " _
                             & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,     " _
                             & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40,     " _
                             & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50,     " _
                             & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60,     " _
                             & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68,@P69,@P70);    "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 15)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 2)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 25)
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
                    Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar, 30)
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
                    Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", System.Data.SqlDbType.NVarChar, 30)

                    PARA01.Value = T00004UPDrow("CAMPCODE")                           '会社コード(CAMPCODE)
                    PARA02.Value = T00004UPDrow("TERMORG")                            '端末設置部署(TERMORG)
                    PARA03.Value = T00004UPDrow("ORDERNO").PadLeft(7, "0")            '受注番号(ORDERNO)
                    PARA04.Value = T00004UPDrow("DETAILNO").PadLeft(3, "0")           '明細№(DETAILNO)
                    PARA05.Value = T00004UPDrow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
                    PARA06.Value = T00004UPDrow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
                    PARA07.Value = T00004UPDrow("SEQ").PadLeft(2, "0")                '枝番(SEQ)
                    PARA08.Value = I_DATENOW.ToString("yyyyMMddHHmmssfff")            'エントリー日時(ENTRYDATE)
                    PARA09.Value = T00004UPDrow("TORICODE")                           '取引先コード(TORICODE)
                    PARA10.Value = T00004UPDrow("OILTYPE")                            '油種(OILTYPE)
                    PARA11.Value = T00004UPDrow("STORICODE")                          '請求取引先コード(STORICODE)
                    PARA12.Value = T00004UPDrow("ORDERORG")                           '受注受付部署(ORDERORG)
                    If T00004UPDrow("SHUKODATE") = "" Then                            '出庫日(SHUKODATE)
                        PARA13.Value = "2000/01/01"
                    Else
                        PARA13.Value = RTrim(T00004UPDrow("SHUKODATE"))
                    End If
                    If T00004UPDrow("KIKODATE") = "" Then                             '帰庫日(KIKODATE)
                        PARA14.Value = "2000/01/01"
                    Else
                        PARA14.Value = RTrim(T00004UPDrow("KIKODATE"))
                    End If
                    If T00004UPDrow("SHUKADATE") = "" Then                            '出荷日(SHUKADATE)
                        PARA15.Value = "2000/01/01"
                    Else
                        PARA15.Value = RTrim(T00004UPDrow("SHUKADATE"))
                    End If
                    PARA16.Value = T00004UPDrow("TUMIOKIKBN")                         '積置区分(TUMIOKIKBN)
                    PARA17.Value = T00004UPDrow("URIKBN")                             '売上計上基準(URIKBN)
                    PARA18.Value = T00004UPDrow("STATUS")                             '状態(STATUS)
                    PARA19.Value = T00004UPDrow("SHIPORG")                            '出荷部署(SHIPORG)
                    PARA20.Value = T00004UPDrow("SHUKABASHO")                         '出荷場所(SHUKABASHO)
                    PARA21.Value = T00004UPDrow("INTIME")                             '時間指定（入構）(INTIME)
                    PARA22.Value = T00004UPDrow("OUTTIME")                            '時間指定（出構）(OUTTIME)
                    PARA23.Value = T00004UPDrow("SHUKADENNO")                         '出荷伝票番号(SHUKADENNO)
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("TUMISEQ"))) Then '積順(TUMISEQ)
                        PARA24.Value = 0
                    Else
                        PARA24.Value = T00004UPDrow("TUMISEQ")
                    End If
                    PARA25.Value = T00004UPDrow("TUMIBA")                             '積場(TUMIBA)
                    PARA26.Value = T00004UPDrow("GATE")                               'ゲート(GATE)
                    PARA27.Value = T00004UPDrow("GSHABAN")                            '業務車番(GSHABAN)
                    PARA28.Value = T00004UPDrow("RYOME")                              '両目(RYOME)
                    PARA29.Value = T00004UPDrow("CONTCHASSIS")                        'コンテナシャーシ(CONTCHASSIS)
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("SHAFUKU"))) Then '車腹（積載量）(SHAFUKU)
                        PARA30.Value = 0.0
                    Else
                        PARA30.Value = CType(T00004UPDrow("SHAFUKU"), Double)
                    End If
                    PARA31.Value = T00004UPDrow("STAFFCODE")                          '乗務員コード(STAFFCODE)
                    PARA32.Value = T00004UPDrow("SUBSTAFFCODE")                       '副乗務員コード(SUBSTAFFCODE)
                    PARA33.Value = T00004UPDrow("STTIME")                             '出勤時間(STTIME)
                    PARA34.Value = ""                                                 '荷主受注番号(TORIORDERNO)
                    If RTrim(T00004UPDrow("TODOKEDATE")) = "" Then                    '届日(TODOKEDATE)
                        PARA35.Value = "2000/01/01"
                    Else
                        PARA35.Value = RTrim(T00004UPDrow("TODOKEDATE"))
                    End If
                    PARA36.Value = T00004UPDrow("TODOKETIME")                         '時間指定（配送）(TODOKETIME)
                    PARA37.Value = T00004UPDrow("TODOKECODE")                         '届先コード(TODOKECODE)
                    PARA38.Value = T00004UPDrow("PRODUCT1")                           '品名１(PRODUCT1)
                    PARA39.Value = T00004UPDrow("PRODUCT2")                           '品名２(PRODUCT2)
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("PRATIO"))) Then  'Ｐ比率(PRATIO)
                        PARA40.Value = 0.0
                    Else
                        PARA40.Value = CType(T00004UPDrow("PRATIO"), Double)
                    End If
                    PARA41.Value = T00004UPDrow("SMELLKBN")                           '臭有無(SMELLKBN)
                    PARA42.Value = T00004UPDrow("CONTNO")                             'コンテナ番号(CONTNO)
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("SURYO"))) Then   '数量(SURYO)
                        PARA43.Value = 0.0
                    Else
                        PARA43.Value = CType(T00004UPDrow("SURYO"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("DAISU"))) Then   '台数(DAISU)
                        PARA44.Value = 0
                    Else
                        PARA44.Value = CType(T00004UPDrow("DAISU"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("JSURYO"))) Then   '配送実績数量(JSURYO)
                        PARA45.Value = 0.0
                    Else
                        PARA45.Value = CType(T00004UPDrow("JSURYO"), Double)
                    End If
                    If String.IsNullOrWhiteSpace(RTrim(T00004UPDrow("JDAISU"))) Then   '配送実績台数(JDAISU)
                        PARA46.Value = 0
                    Else
                        PARA46.Value = CType(T00004UPDrow("DAISU"), Double)
                    End If
                    PARA47.Value = T00004UPDrow("REMARKS1")                           '備考１(REMARKS1)
                    PARA48.Value = T00004UPDrow("REMARKS2")                           '備考２(REMARKS2)
                    PARA49.Value = T00004UPDrow("REMARKS3")                           '備考３(REMARKS3)
                    PARA50.Value = T00004UPDrow("REMARKS4")                           '備考４(REMARKS4)
                    PARA51.Value = T00004UPDrow("REMARKS5")                           '備考５(REMARKS5)
                    PARA52.Value = T00004UPDrow("REMARKS6")                           '備考６(REMARKS6)
                    PARA53.Value = T00004UPDrow("DELFLG")                             '削除フラグ(DELFLG)
                    PARA54.Value = I_DATENOW                                          '登録年月日(INITYMD)
                    PARA55.Value = I_DATENOW                                          '更新年月日(UPDYMD)
                    PARA56.Value = Master.USERID                                      '更新ユーザＩＤ(UPDUSER)
                    PARA57.Value = Master.USERTERMID                                  '更新端末(UPDTERMID)
                    PARA58.Value = C_DEFAULT_YMD                                      '集信日時(RECEIVEYMD)

                    '基準日＝出荷日 7/11
                    If T00004UPDrow("KIJUNDATE") = "" Then                            '基準日(KIJUNDATE)
                        PARA59.Value = "2000/01/01"
                    Else
                        PARA59.Value = RTrim(T00004UPDrow("KIJUNDATE"))
                    End If
                    PARA60.Value = T00004UPDrow("SHARYOTYPEF")                        '統一車番前(SHARYOTYPEF)
                    PARA61.Value = T00004UPDrow("TSHABANF")                           '統一車番前(TSHABANF)
                    PARA62.Value = T00004UPDrow("SHARYOTYPEB")                        '統一車番前(SHARYOTYPEB)
                    PARA63.Value = T00004UPDrow("TSHABANB")                           '統一車番前(TSHABANB)
                    PARA64.Value = T00004UPDrow("SHARYOTYPEB2")                       '統一車番前(SHARYOTYPEB2)
                    PARA65.Value = T00004UPDrow("TSHABANB2")                          '統一車番前(TSHABANB2)
                    PARA66.Value = T00004UPDrow("HTANI")                              '配送単位(HTANI)
                    PARA67.Value = ""                                                 '配送実績単位(STANI)
                    PARA68.Value = T00004UPDrow("TAXKBN")                             '税区分(TAXKBN)
                    PARA69.Value = T00004UPDrow("PRODUCTCODE")                        '品名コード(PRODUCTCODE)
                    PARA70.Value = T00004UPDrow("JXORDERID") & T00004UPDrow("JXORDERSTATUS") 'JXオーダー識別ID(JXORDERID)

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    'CLOSE
                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER INSERT")
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER INSERT"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub

                End Try

                '〇配送受注登録結果を画面情報へ戻す
                For Each T00004row In T00004tbl.Rows
                    If T00004row("CAMPCODE") = T00004UPDrow("CAMPCODE") AndAlso
                       T00004row("TORICODE") = T00004UPDrow("TORICODE") AndAlso
                       T00004row("OILTYPE") = T00004UPDrow("OILTYPE") AndAlso
                       T00004row("KIJUNDATE") = T00004UPDrow("KIJUNDATE") AndAlso
                       T00004row("ORDERORG") = T00004UPDrow("ORDERORG") AndAlso
                       T00004row("SHIPORG") = T00004UPDrow("SHIPORG") AndAlso
                       T00004row("SHUKODATE") = T00004UPDrow("SHUKODATE") AndAlso
                       T00004row("GSHABAN") = T00004UPDrow("GSHABAN") AndAlso
                       T00004row("TRIPNO") = T00004UPDrow("TRIPNO") AndAlso
                       T00004row("DROPNO") = T00004UPDrow("DROPNO") AndAlso
                       T00004row("SEQ") = T00004UPDrow("SEQ") AndAlso
                       T00004row("DELFLG") <> "1" Then

                        T00004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        T00004row("ORDERNO") = T00004UPDrow("ORDERNO")
                        T00004row("DETAILNO") = T00004UPDrow("DETAILNO")
                        Exit For

                    End If
                Next
                Try
                    '更新結果(TIMSTP)再取得（荷主受注） …　連続処理を可能にする。
                    Dim SQLStr As String =
                               " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP    " _
                             & "   FROM T0004_HORDER                           " _
                             & "  WHERE CAMPCODE       = @P01                  " _
                             & "    and TORICODE       = @P02                  " _
                             & "    and OILTYPE        = @P03                  " _
                             & "    and KIJUNDATE      = @P04                  " _
                             & "    and ORDERORG       = @P05                  " _
                             & "    and SHIPORG        = @P06                  " _
                             & "    and SHUKODATE      = @P07                  " _
                             & "    and GSHABAN        = @P08                  " _
                             & "    and TRIPNO         = @P09                  " _
                             & "    and DROPNO         = @P10                  " _
                             & "    and SEQ            = @P11                  " _
                             & "    and DELFLG        <> '1'                   "

                    Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)

                    PARA01.Value = T00004UPDrow("CAMPCODE")
                    PARA02.Value = T00004UPDrow("TORICODE")
                    PARA03.Value = T00004UPDrow("OILTYPE")
                    PARA04.Value = T00004UPDrow("KIJUNDATE")
                    PARA05.Value = T00004UPDrow("ORDERORG")
                    PARA06.Value = T00004UPDrow("SHIPORG")
                    PARA07.Value = T00004UPDrow("SHUKODATE")
                    PARA08.Value = T00004UPDrow("GSHABAN")
                    PARA09.Value = T00004UPDrow("TRIPNO")
                    PARA10.Value = T00004UPDrow("DROPNO")
                    PARA11.Value = T00004UPDrow("SEQ")

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    '画面情報へタイムスタンプ・受注番号をフィードバック
                    While SQLdr.Read
                        For Each T00004row In T00004tbl.Rows
                            If T00004row("CAMPCODE") = T00004UPDrow("CAMPCODE") AndAlso
                               T00004row("TORICODE") = T00004UPDrow("TORICODE") AndAlso
                               T00004row("OILTYPE") = T00004UPDrow("OILTYPE") AndAlso
                               T00004row("KIJUNDATE") = T00004UPDrow("KIJUNDATE") AndAlso
                               T00004row("ORDERORG") = T00004UPDrow("ORDERORG") AndAlso
                               T00004row("SHIPORG") = T00004UPDrow("SHIPORG") AndAlso
                               T00004row("SHUKODATE") = T00004UPDrow("SHUKODATE") AndAlso
                               T00004row("GSHABAN") = T00004UPDrow("GSHABAN") AndAlso
                               T00004row("TRIPNO") = T00004UPDrow("TRIPNO") AndAlso
                               T00004row("DROPNO") = T00004UPDrow("DROPNO") AndAlso
                               T00004row("SEQ") = T00004UPDrow("SEQ") AndAlso
                               T00004row("DELFLG") <> C_DELETE_FLG.DELETE Then

                                T00004row("TIMSTP") = SQLdr("TIMSTP")
                                T00004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                T00004row("ORDERNO") = T00004UPDrow("ORDERNO")
                                T00004row("DETAILNO") = T00004UPDrow("DETAILNO")
                                Exit For

                            End If
                        Next
                    End While

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER INSERT")
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER INSERT"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub

                End Try

            End If

        Next

        '更新→クリア
        For Each T00004row In T00004tbl.Rows
            If T00004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                T00004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If
        Next

        SQLcon.Close()
        SQLcon.Dispose()

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  T00004UPDtbl更新データ（画面表示受注+画面非表示受注）作成　＆　タイムスタンプチェック処理          済
    Protected Sub DBupdate_T00004UPDtblget(ByVal O_RTN As String)

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_ORDERORG As String = ""
        Dim WW_SHIPORG As String = ""

        Dim WW_SHUKODATE As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_TRIPNO As String = ""
        Dim WW_DROPNO As String = ""

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■

        For Each T00004row In T00004tbl.Rows

            If T00004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00004row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = 0 To T00004tbl.Rows.Count - 1
                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00004tbl.Rows(j)("TORICODE") = T00004row("TORICODE") AndAlso
                       T00004tbl.Rows(j)("OILTYPE") = T00004row("OILTYPE") AndAlso
                       T00004tbl.Rows(j)("KIJUNDATE") = T00004row("KIJUNDATE") AndAlso
                       T00004tbl.Rows(j)("ORDERORG") = T00004row("ORDERORG") AndAlso
                       T00004tbl.Rows(j)("SHIPORG") = T00004row("SHIPORG") AndAlso
                       T00004tbl.Rows(j)("DELFLG") <> "1" Then

                        T00004tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                    End If
                Next
            End If

        Next

        '■■■ 受注最新レコード(DB格納)をT00004UPDtblへ格納 ■■■

        'Sort
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)
        '○作業用DBのカラム設定
        '更新元データ
        Master.CreateEmptyTable(T00004UPDtbl)
        '作業用データ
        Master.CreateEmptyTable(T00004WKtbl)

        '○更新対象受注のDB格納レコードを全て取得
        For Each T00004row In T00004tbl.Rows

            If T00004row("TORICODE") = WW_TORICODE AndAlso
               T00004row("OILTYPE") = WW_OILTYPE AndAlso
               T00004row("KIJUNDATE") = WW_KIJUNDATE AndAlso
               T00004row("ORDERORG") = WW_ORDERORG AndAlso
               T00004row("SHIPORG") = WW_SHIPORG Then
            Else
                If T00004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00004row("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    T00004WKtbl.Clear()

                    'オブジェクト内容検索
                    Try
                        'DataBase接続文字
                        Dim SQLcon = CS0050SESSION.getConnection
                        SQLcon.Open() 'DataBase接続(Open)

                        '検索SQL文
                        Dim SQLStr As String =
                             "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
                           & "       isnull(rtrim(A.TERMORG),'')           as TERMORG ,        " _
                           & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
                           & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
                           & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
                           & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
                           & "       isnull(rtrim(A.SEQ),'')               as SEQ ,            " _
                           & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
                           & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
                           & "       isnull(rtrim(A.STORICODE),'')         as STORICODE ,      " _
                           & "       isnull(rtrim(A.ORDERORG),'')          as ORDERORG ,       " _
                           & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,      " _
                           & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,       " _
                           & "       isnull(rtrim(A.KIJUNDATE),'')         as KIJUNDATE ,      " _
                           & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,      " _
                           & "       isnull(rtrim(A.TUMIOKIKBN),'')        as TUMIOKIKBN ,     " _
                           & "       isnull(rtrim(A.URIKBN),'')            as URIKBN ,         " _
                           & "       isnull(rtrim(A.STATUS),'')            as STATUS ,         " _
                           & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
                           & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
                           & "       isnull(rtrim(A.INTIME),'')            as INTIME ,         " _
                           & "       isnull(rtrim(A.OUTTIME),'')           as OUTTIME ,        " _
                           & "       isnull(rtrim(A.SHUKADENNO),'')        as SHUKADENNO ,     " _
                           & "       isnull(rtrim(A.TUMISEQ),'')           as TUMISEQ ,        " _
                           & "       isnull(rtrim(A.TUMIBA),'')            as TUMIBA ,         " _
                           & "       isnull(rtrim(A.GATE),'')              as GATE ,           " _
                           & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
                           & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
                           & "       isnull(rtrim(A.CONTCHASSIS),'')       as CONTCHASSIS ,    " _
                           & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
                           & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
                           & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
                           & "       isnull(rtrim(A.STTIME),'')            as STTIME ,         " _
                           & "       isnull(rtrim(A.TORIORDERNO),'')       as TORIORDERNO ,    " _
                           & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,     " _
                           & "       isnull(rtrim(A.TODOKETIME),'')        as TODOKETIME ,     " _
                           & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
                           & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
                           & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
                           & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE ,    " _
                           & "       isnull(rtrim(A.PRATIO),'')            as PRATIO ,         " _
                           & "       isnull(rtrim(A.SMELLKBN),'')          as SMELLKBN ,       " _
                           & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
                           & "       isnull(rtrim(A.HTANI),'')             as HTANI ,          " _
                           & "       isnull(rtrim(A.SURYO),'')             as SURYO ,          " _
                           & "       isnull(rtrim(A.DAISU),'')             as DAISU ,          " _
                           & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
                           & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
                           & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
                           & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
                           & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
                           & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
                           & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
                           & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
                           & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
                           & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
                           & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
                           & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
                           & "       isnull(rtrim(A.TAXKBN),'')            as TAXKBN ,         " _
                           & "       isnull(rtrim(A.JXORDERID),'')         as JXORDERID ,      " _
                           & "       isnull(rtrim(A.DELFLG),'')            as DELFLG ,         " _
                           & "       TIMSTP = cast(A.UPDTIMSTP  as bigint) ,        " _
                           & "       isnull(rtrim(B.SHARYOINFO1),'')       as SHARYOINFO1 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO2),'')       as SHARYOINFO2 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO3),'')       as SHARYOINFO3 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO4),'')       as SHARYOINFO4 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO5),'')       as SHARYOINFO5 ,    " _
                           & "       isnull(rtrim(B.SHARYOINFO6),'')       as SHARYOINFO6 ,    " _
                           & "       isnull(rtrim(C.ARRIVTIME),'')         as ARRIVTIME ,      " _
                           & "       isnull(rtrim(C.DISTANCE),'')          as DISTANCE ,       " _
                           & "       isnull(rtrim(D.ADDR1),'') +              					" _
                           & "       isnull(rtrim(D.ADDR2),'') +            					" _
                           & "       isnull(rtrim(D.ADDR3),'') +             					" _
                           & "       isnull(rtrim(D.ADDR4),'')          	as ADDR ,           " _
                           & "       isnull(rtrim(D.NOTES1),'')        	    as NOTES1 ,       	" _
                           & "       isnull(rtrim(D.NOTES2),'')          	as NOTES2 ,       	" _
                           & "       isnull(rtrim(D.NOTES3),'')          	as NOTES3 ,       	" _
                           & "       isnull(rtrim(D.NOTES4),'')          	as NOTES4 ,       	" _
                           & "       isnull(rtrim(D.NOTES5),'')          	as NOTES5 ,       	" _
                           & "       isnull(rtrim(E.NOTES1),'')        	    as STAFFNOTES1 ,   	" _
                           & "       isnull(rtrim(E.NOTES2),'')          	as STAFFNOTES2 ,   	" _
                           & "       isnull(rtrim(E.NOTES3),'')          	as STAFFNOTES3 ,   	" _
                           & "       isnull(rtrim(E.NOTES4),'')          	as STAFFNOTES4 ,   	" _
                           & "       isnull(rtrim(E.NOTES5),'')          	as STAFFNOTES5     	" _
                           & "  FROM T0004_HORDER AS A								" _
                           & "  LEFT JOIN MA006_SHABANORG B 						" _
                           & "    ON B.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and B.GSHABAN      	= A.GSHABAN 				" _
                           & "   and B.MANGUORG     	= A.SHIPORG 				" _
                           & "   and B.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MC007_TODKORG C 							" _
                           & "    ON C.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and C.TORICODE     	= A.TORICODE 				" _
                           & "   and C.TODOKECODE   	= A.TODOKECODE 				" _
                           & "   and C.UORG         	= A.SHIPORG 				" _
                           & "   and C.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MC006_TODOKESAKI D 						" _
                           & "    ON D.CAMPCODE     	= C.CAMPCODE 				" _
                           & "   and D.TORICODE     	= C.TORICODE				" _
                           & "   and D.TODOKECODE   	= C.TODOKECODE 				" _
                           & "   and D.STYMD           <= A.SHUKODATE				" _
                           & "   and D.ENDYMD          >= A.SHUKODATE				" _
                           & "   and D.DELFLG          <> '1' 						" _
                           & "  LEFT JOIN MB001_STAFF E     						" _
                           & "    ON E.CAMPCODE     	= A.CAMPCODE 				" _
                           & "   and E.STAFFCODE     	= A.STAFFCODE				" _
                           & "   and E.STYMD           <= A.SHUKODATE				" _
                           & "   and E.ENDYMD          >= A.SHUKODATE				" _
                           & "   and E.DELFLG          <> '1' 						" _
                           & " WHERE A.CAMPCODE         = @P01                      " _
                           & "  and  A.TORICODE         = @P02                      " _
                           & "  and  A.OILTYPE          = @P03           		    " _
                           & "  and  A.ORDERORG         = @P04           		    " _
                           & "  and  A.SHIPORG          = @P05           		    " _
                           & "  and  A.KIJUNDATE        = @P06                      " _
                           & "  and  A.DELFLG          <> '1'                       " _
                           & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.KIJUNDATE ,       " _
                           & " 		    A.ORDERORG  ,A.SHIPORG ,A.GSHABAN           "

                        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)  '荷主
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)  '油種
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)  '受注部署
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)      '出荷日

                        '○関連受注指定
                        PARA01.Value = T00004row("CAMPCODE")        '会社
                        PARA02.Value = T00004row("TORICODE")        '出荷日
                        PARA03.Value = T00004row("OILTYPE")         '油種
                        PARA04.Value = T00004row("ORDERORG")        '受注部署
                        PARA05.Value = T00004row("SHIPORG")         '出荷部署
                        PARA06.Value = T00004row("KIJUNDATE")       '基準日

                        '■SQL実行
                        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■テーブル検索結果をテーブル格納
                        T00004WKtbl.Load(SQLdr)
                        T00004UPDtbl.Merge(T00004WKtbl, False)
                        For Each T00004UPDrow In T00004UPDtbl.Rows
                            T00004UPDrow("LINECNT") = 0
                            T00004UPDrow("SELECT") = 1
                            T00004UPDrow("HIDDEN") = 0
                            T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Next

                        SQLdr.Close()
                        SQLdr = Nothing

                        SQLcmd.Dispose()
                        SQLcmd = Nothing

                        SQLcon.Close() 'DataBase接続(Close)
                        SQLcon.Dispose()
                        SQLcon = Nothing

                    Catch ex As Exception
                        CS0011LOGWRITE.INFSUBCLASS = "DBupdate_T00004UPDtblget"     'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "T0004_HORDER UPDATE"
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        O_RTN = C_MESSAGE_NO.DB_ERROR
                        Exit Sub

                    End Try

                    WW_TORICODE = T00004row("TORICODE")
                    WW_OILTYPE = T00004row("OILTYPE")
                    WW_KIJUNDATE = T00004row("KIJUNDATE")
                    WW_ORDERORG = T00004row("ORDERORG")
                    WW_SHIPORG = T00004row("SHIPORG")

                Else
                End If

            End If
        Next

        '■■■ 受注番号　自動採番 ■■■                  

        'Sort(T00004tbl)
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)

        '○　受注番号　自動採番
        For i As Integer = 0 To T00004tbl.Rows.Count - 1

            Dim T00004row = T00004tbl.Rows(i)


            If T00004row("ORDERNO").ToString.Contains("新") Then
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.ORDERNO
                CS0033AutoNumber.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0033AutoNumber.MORG = T00004row("ORDERORG")
                CS0033AutoNumber.USERID = Master.USERID
                CS0033AutoNumber.getAutoNumber()

                If isNormal(CS0033AutoNumber.ERR) Then
                    '他レコードへ反映
                    For j As Integer = i To T00004tbl.Rows.Count - 1
                        If T00004tbl.Rows(j)("ORDERNO").ToString.Contains("新") Then
                            If T00004tbl.Rows(j)("TORICODE") = T00004row("TORICODE") AndAlso
                               T00004tbl.Rows(j)("OILTYPE") = T00004row("OILTYPE") AndAlso
                               T00004tbl.Rows(j)("KIJUNDATE") = T00004row("KIJUNDATE") AndAlso
                               T00004tbl.Rows(j)("ORDERORG") = T00004row("ORDERORG") AndAlso
                               T00004tbl.Rows(j)("SHIPORG") = T00004row("SHIPORG") Then

                                T00004tbl.Rows(j)("ORDERNO") = CS0033AutoNumber.SEQ
                            Else
                                Exit For
                            End If
                        End If
                    Next

                Else
                    Master.Output(CS0033AutoNumber.ERR, C_MESSAGE_TYPE.ABORT, CS0033AutoNumber.ERR_DETAIL)
                    Exit Sub
                End If
            End If

        Next

        '■■■ 画面非表示レコード+画面表示レコードによりT00004UPDtblを作成 ■■■

        '○T00004UPDtbl内の画面表示レコードを削除(日付による）…　T00004tblとレコード重複しているため

        Dim WW_TODOKEDATEF As Date
        Dim WW_TODOKEDATET As Date
        Dim WW_SHUKODATEF As Date
        Dim WW_SHUKODATET As Date
        '届日（FROM-TO）
        If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATEF.Text) Then
            WW_TODOKEDATEF = C_DEFAULT_YMD
        Else
            WW_TODOKEDATEF = work.WF_SEL_TODOKEDATEF.Text
        End If
        If String.IsNullOrEmpty(work.WF_SEL_TODOKEDATET.Text) Then
            WW_TODOKEDATET = C_MAX_YMD
        Else
            WW_TODOKEDATET = work.WF_SEL_TODOKEDATET.Text
        End If
        '出荷日（FROM-TO）
        If String.IsNullOrEmpty(work.WF_SEL_SHUKODATEF.Text) Then
            WW_SHUKODATEF = C_DEFAULT_YMD
        Else
            WW_SHUKODATEF = work.WF_SEL_SHUKODATEF.Text
        End If
        If String.IsNullOrEmpty(work.WF_SEL_SHUKODATET.Text) Then
            WW_SHUKODATET = C_MAX_YMD
        Else
            WW_SHUKODATET = work.WF_SEL_SHUKODATET.Text
        End If

        WW_FILLstr =
            "TODOKEDATE < #" & WW_TODOKEDATEF & "# or " &
            "TODOKEDATE > #" & WW_TODOKEDATET & "# or " &
            "SHUKODATE < #" & WW_SHUKODATEF & "# or " &
            "SHUKODATE > #" & WW_SHUKODATET & "#    "
        '画面表示レコードを削除
        CS0026TBLSORTget.TABLE = T00004UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = WW_FILLstr
        CS0026TBLSORTget.sort(T00004UPDtbl)

        '○画面表示レコードをマージ
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        CS0026TBLSORTget.sort(T00004WKtbl)
        T00004UPDtbl.Merge(T00004WKtbl, False)

        '○更新・エラーをT00004UPDtblへ反映(DB更新単位：荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署)
        CS0026TBLSORTget.TABLE = T00004UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004UPDtbl)

        For i As Integer = 0 To T00004UPDtbl.Rows.Count - 1
            Dim T00004UPDrow = T00004UPDtbl.Rows(i)

            If T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                For j As Integer = i To T00004UPDtbl.Rows.Count - 1
                    '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00004UPDtbl.Rows(j)("TORICODE") = T00004UPDrow("TORICODE") AndAlso
                       T00004UPDtbl.Rows(j)("OILTYPE") = T00004UPDrow("OILTYPE") AndAlso
                       T00004UPDtbl.Rows(j)("KIJUNDATE") = T00004UPDrow("KIJUNDATE") AndAlso
                       T00004UPDtbl.Rows(j)("ORDERORG") = T00004UPDrow("ORDERORG") AndAlso
                       T00004UPDtbl.Rows(j)("SHIPORG") = T00004UPDrow("SHIPORG") Then

                        T00004UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                    Else
                        Exit For
                    End If
                Next
            End If

            If T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = i To T00004UPDtbl.Rows.Count - 1
                    '荷主、油種、基準日（出荷日or届日）、受注部署、出荷部署が同一
                    If T00004UPDtbl.Rows(j)("TORICODE") = T00004UPDrow("TORICODE") AndAlso
                       T00004UPDtbl.Rows(j)("OILTYPE") = T00004UPDrow("OILTYPE") AndAlso
                       T00004UPDtbl.Rows(j)("KIJUNDATE") = T00004UPDrow("KIJUNDATE") AndAlso
                       T00004UPDtbl.Rows(j)("ORDERORG") = T00004UPDrow("ORDERORG") AndAlso
                       T00004UPDtbl.Rows(j)("SHIPORG") = T00004UPDrow("SHIPORG") AndAlso
                       T00004UPDtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                        T00004UPDtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                    Else
                        Exit For
                    End If
                Next
            End If

        Next

        '○更新対象以外のレコードを削除
        CS0026TBLSORTget.TABLE = T00004UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        CS0026TBLSORTget.sort(T00004UPDtbl)

        '■■■ T00004UPDtblのDetailNO、SEQを再付番 ■■■
        Dim WW_DETAILNO As Integer = 0
        Dim WW_SEQ As Integer = 0

        '○DetailNO再付番
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_ORDERORG = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        WW_DROPNO = ""

        For Each T00004UPDrow In T00004UPDtbl.Rows

            If T00004UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00004UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00004UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00004UPDrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = T00004UPDrow("ORDERORG") AndAlso
                   WW_SHIPORG = T00004UPDrow("SHIPORG") Then

                    WW_DETAILNO += 1
                    T00004UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")
                Else
                    WW_DETAILNO = 1
                    T00004UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")

                    WW_TORICODE = T00004UPDrow("TORICODE")
                    WW_OILTYPE = T00004UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00004UPDrow("KIJUNDATE")
                    WW_ORDERORG = T00004UPDrow("ORDERORG")
                    WW_SHIPORG = T00004UPDrow("SHIPORG")

                End If
            End If

        Next

        '○台数設定
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_ORDERORG = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        For Each T00004UPDrow In T00004UPDtbl.Rows

            If T00004UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00004UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00004UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00004UPDrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = T00004UPDrow("ORDERORG") AndAlso
                   WW_SHIPORG = T00004UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = T00004UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = T00004UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = T00004UPDrow("TRIPNO") Then

                    T00004UPDrow("DAISU") = 0
                Else
                    T00004UPDrow("DAISU") = 1

                    WW_TORICODE = T00004UPDrow("TORICODE")
                    WW_OILTYPE = T00004UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00004UPDrow("KIJUNDATE")
                    WW_ORDERORG = T00004UPDrow("ORDERORG")
                    WW_SHIPORG = T00004UPDrow("SHIPORG")
                    WW_SHUKODATE = T00004UPDrow("SHUKODATE")
                    WW_GSHABAN = T00004UPDrow("GSHABAN")
                    WW_TRIPNO = T00004UPDrow("TRIPNO")
                    WW_DROPNO = T00004UPDrow("DROPNO")

                End If
            End If

        Next

        '○SEQ再付番
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_ORDERORG = ""
        WW_SHIPORG = ""
        WW_SHUKODATE = ""
        WW_GSHABAN = ""
        WW_TRIPNO = ""
        WW_DROPNO = ""
        For Each T00004UPDrow In T00004UPDtbl.Rows

            If T00004UPDrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00004UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = T00004UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00004UPDrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = T00004UPDrow("ORDERORG") AndAlso
                   WW_SHIPORG = T00004UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = T00004UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = T00004UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = T00004UPDrow("TRIPNO") AndAlso
                   WW_DROPNO = T00004UPDrow("DROPNO") Then

                    WW_SEQ += 1
                    T00004UPDrow("SEQ") = WW_SEQ.ToString("00")
                Else
                    WW_SEQ = 1
                    T00004UPDrow("SEQ") = WW_SEQ.ToString("00")

                    WW_TORICODE = T00004UPDrow("TORICODE")
                    WW_OILTYPE = T00004UPDrow("OILTYPE")
                    WW_KIJUNDATE = T00004UPDrow("KIJUNDATE")
                    WW_ORDERORG = T00004UPDrow("ORDERORG")
                    WW_SHIPORG = T00004UPDrow("SHIPORG")
                    WW_SHUKODATE = T00004UPDrow("SHUKODATE")
                    WW_GSHABAN = T00004UPDrow("GSHABAN")
                    WW_TRIPNO = T00004UPDrow("TRIPNO")
                    WW_DROPNO = T00004UPDrow("DROPNO")

                End If
            End If

        Next

        '○close
        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  T00004SUMtbl更新データ作成
    Protected Sub DBupdate_T00004SUMtblget(ByVal O_RTN As String)

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        Dim WW_SORTstr As String = ""
        Dim WW_FILLstr As String = ""

        Dim WW_TORICODE As String = ""
        Dim WW_OILTYPE As String = ""
        Dim WW_SHUKADATE As String = ""
        Dim WW_KIJUNDATE As String = ""
        Dim WW_ORDERORG As String = ""
        Dim WW_SHIPORG As String = ""
        Dim WW_DETAILNO As String = ""

        '■■■ 荷主受注(T00004SUMtbl)作成 ■■■

        'Sort　…　念のため
        CS0026TBLSORTget.TABLE = T00004UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "' or OPERATION = '" & C_LIST_OPERATION_CODE.WARNING & "'"
        CS0026TBLSORTget.sort(T00004UPDtbl)

        '○集計用DBへ構造＆データをコピー
        T00004SUMtbl = T00004UPDtbl.Copy()
        '積置レコード削除
        CS0026TBLSORTget.TABLE = T00004SUMtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = "(TUMIOKIKBN <> '1') or (TUMIOKIKBN = '1' and SHUKODATE <> SHUKADATE)"
        CS0026TBLSORTget.sort(T00004SUMtbl)
        '削除レコード削除
        CS0026TBLSORTget.TABLE = T00004SUMtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1'"
        CS0026TBLSORTget.sort(T00004SUMtbl)

        '○マスク処理
        Dim WW_TODOKEDATE_CNTL As String = ""
        Dim WW_SHUKODATE_CNTL As String = ""
        Dim WW_SHUKABASHO_CNTL As String = ""
        Dim WW_GSHABAN_CNTL As String = ""
        Dim WW_SHAFUKU_CNTL As String = ""
        Dim WW_STAFFCODE_CNTL As String = ""
        Dim WW_TODOKECODE_CNTL As String = ""
        Dim WW_PRODUCT1_CNTL As String = ""
        Dim WW_PRODUCTCODE_CNTL As String = ""
        Dim WW_DAISU_CNTL As String = ""

        Dim WW_TODOKEDATE As String = ""
        Dim WW_SHUKODATE As String = ""
        Dim WW_SHUKABASHO As String = ""
        Dim WW_GSHABAN As String = ""
        Dim WW_SHAFUKU As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_TODOKECODE As String = ""
        Dim WW_TUMIOKIKBN As String = ""
        Dim WW_PRODUCTCODE As String = ""

        For Each T00004SUMrow In T00004SUMtbl.Rows

            '荷主受注集計制御マスタ取得()
            GS0029T3CNTLget.CAMPCODE = T00004SUMrow("CAMPCODE")
            GS0029T3CNTLget.TORICODE = T00004SUMrow("TORICODE")
            GS0029T3CNTLget.OILTYPE = T00004SUMrow("OILTYPE")
            GS0029T3CNTLget.ORDERORG = T00004SUMrow("ORDERORG")
            GS0029T3CNTLget.KIJUNDATE = T00004SUMrow("KIJUNDATE")
            GS0029T3CNTLget.GS0029T3CNTLget()

            If isNormal(GS0029T3CNTLget.ERR) Then
            Else
                Master.Output(GS0029T3CNTLget.ERR, C_MESSAGE_TYPE.ABORT, "荷主受注集計制御マスタ登録なし(" & T00004SUMrow("TORICODE") & ")")
                Exit Sub
            End If

            If GS0029T3CNTLget.CNTL01 <> "1" Then                                '集計区分(積置区分)
                T00004SUMrow("TUMIOKIKBN") = ""
            End If
            If GS0029T3CNTLget.CNTL02 <> "1" Then                                '集計区分(出庫日)
                T00004SUMrow("SHUKODATE") = ""
            End If
            If GS0029T3CNTLget.CNTL03 <> "1" Then                                '集計区分(出荷場所)
                T00004SUMrow("SHUKABASHO") = ""
            End If
            If GS0029T3CNTLget.CNTL04 <> "1" Then                                '集計区分(業務車番)
                T00004SUMrow("GSHABAN") = ""
                T00004SUMrow("SHARYOTYPEF") = ""
                T00004SUMrow("TSHABANF") = ""
                T00004SUMrow("SHARYOTYPEB") = ""
                T00004SUMrow("TSHABANB") = ""
                T00004SUMrow("SHARYOTYPEB2") = ""
                T00004SUMrow("TSHABANB2") = ""
            End If
            If GS0029T3CNTLget.CNTL05 <> "1" Then                                '集計区分(車腹(積載量))
                T00004SUMrow("SHAFUKU") = ""
            End If
            If GS0029T3CNTLget.CNTL06 <> "1" Then                                '集計区分(乗務員コード)
                T00004SUMrow("STAFFCODE") = ""
            End If
            If GS0029T3CNTLget.CNTL07 <> "1" Then                                '集計区分(届先コード)
                T00004SUMrow("TODOKECODE") = ""
            End If
            If GS0029T3CNTLget.CNTL08 <> "1" Then                                '集計区分(品名１)
                T00004SUMrow("PRODUCT1") = ""
            End If
            If GS0029T3CNTLget.CNTL09 <> "1" Then                                '集計区分(品名コード)
                T00004SUMrow("PRODUCTCODE") = ""
            End If

            'TRIPNO ,DROPNO , SEQクリア
            T00004SUMrow("TRIPNO") = "000"
            T00004SUMrow("DROPNO") = "000"
            T00004SUMrow("SEQ") = "00"

        Next

        '■■■ DetailNO毎に台数、数量をサマリ ■■■
        Dim SURYO_SUM As Decimal = 0
        Dim DAISU_SUM As Long = 0
        Dim JSURYO_SUM As Decimal = 0
        Dim JDAISU_SUM As Long = 0

        'sort
        CS0026TBLSORTget.TABLE = T00004SUMtbl
        CS0026TBLSORTget.SORTING = "DELFLG ,TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,TUMIOKIKBN ,SHUKADATE ,TODOKEDATE ,SHUKODATE ,SHUKABASHO ,GSHABAN ,SHAFUKU ,STAFFCODE ,TODOKECODE ,PRODUCTCODE"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004SUMtbl)

        '最終行から初回行へループ
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_ORDERORG = ""
        WW_SHIPORG = ""
        WW_DETAILNO = ""

        For i As Integer = 0 To T00004SUMtbl.Rows.Count - 1

            Dim T00004SUMrow = T00004SUMtbl.Rows(i)

            '受注＋DetailNo毎に集計　…　受注＋DetailNo＋荷主受注集計制御でブレイク
            If WW_TORICODE = T00004SUMrow("TORICODE") AndAlso
               WW_OILTYPE = T00004SUMrow("OILTYPE") AndAlso
               WW_KIJUNDATE = T00004SUMrow("KIJUNDATE") AndAlso
               WW_ORDERORG = T00004SUMrow("ORDERORG") AndAlso
               WW_SHIPORG = T00004SUMrow("SHIPORG") AndAlso
               WW_TODOKEDATE = T00004SUMrow("TODOKEDATE") AndAlso
               WW_SHUKODATE = T00004SUMrow("SHUKODATE") AndAlso
               WW_SHUKADATE = T00004SUMrow("SHUKADATE") AndAlso
               WW_SHUKABASHO = T00004SUMrow("SHUKABASHO") AndAlso
               WW_GSHABAN = T00004SUMrow("GSHABAN") AndAlso
               WW_SHAFUKU = T00004SUMrow("SHAFUKU") AndAlso
               WW_STAFFCODE = T00004SUMrow("STAFFCODE") AndAlso
               WW_TODOKECODE = T00004SUMrow("TODOKECODE") AndAlso
               WW_TUMIOKIKBN = T00004SUMrow("TUMIOKIKBN") AndAlso
               WW_PRODUCTCODE = T00004SUMrow("PRODUCTCODE") Then

                '先頭レコード以外は不要
                T00004SUMrow("SELECT") = "0"

            Else
                SURYO_SUM = 0
                DAISU_SUM = 0
                JSURYO_SUM = 0
                JDAISU_SUM = 0

                For j As Integer = i To T00004SUMtbl.Rows.Count - 1
                    If T00004SUMtbl.Rows(j)("TORICODE") = T00004SUMrow("TORICODE") AndAlso
                       T00004SUMtbl.Rows(j)("OILTYPE") = T00004SUMrow("OILTYPE") AndAlso
                       T00004SUMtbl.Rows(j)("KIJUNDATE") = T00004SUMrow("KIJUNDATE") AndAlso
                       T00004SUMtbl.Rows(j)("ORDERORG") = T00004SUMrow("ORDERORG") AndAlso
                       T00004SUMtbl.Rows(j)("SHIPORG") = T00004SUMrow("SHIPORG") AndAlso
                       T00004SUMtbl.Rows(j)("TODOKEDATE") = T00004SUMrow("TODOKEDATE") AndAlso
                       T00004SUMtbl.Rows(j)("SHUKODATE") = T00004SUMrow("SHUKODATE") AndAlso
                       T00004SUMtbl.Rows(j)("SHUKADATE") = T00004SUMrow("SHUKADATE") AndAlso
                       T00004SUMtbl.Rows(j)("SHUKABASHO") = T00004SUMrow("SHUKABASHO") AndAlso
                       T00004SUMtbl.Rows(j)("GSHABAN") = T00004SUMrow("GSHABAN") AndAlso
                       T00004SUMtbl.Rows(j)("SHAFUKU") = T00004SUMrow("SHAFUKU") AndAlso
                       T00004SUMtbl.Rows(j)("STAFFCODE") = T00004SUMrow("STAFFCODE") AndAlso
                       T00004SUMtbl.Rows(j)("TODOKECODE") = T00004SUMrow("TODOKECODE") AndAlso
                       T00004SUMtbl.Rows(j)("TUMIOKIKBN") = T00004SUMrow("TUMIOKIKBN") AndAlso
                       T00004SUMtbl.Rows(j)("PRODUCTCODE") = T00004SUMrow("PRODUCTCODE") AndAlso
                       T00004SUMtbl.Rows(j)("DELFLG") <> "1" Then

                        Try
                            SURYO_SUM = SURYO_SUM + CDbl(T00004SUMtbl.Rows(j)("SURYO"))
                            JSURYO_SUM = JSURYO_SUM + CDbl(T00004SUMtbl.Rows(j)("JSURYO"))
                        Catch ex As Exception
                        End Try

                        Try
                            DAISU_SUM = DAISU_SUM + CInt(T00004SUMtbl.Rows(j)("DAISU"))
                            JDAISU_SUM = JDAISU_SUM + CInt(T00004SUMtbl.Rows(j)("JDAISU"))
                        Catch ex As Exception
                        End Try

                    Else
                        Exit For
                    End If

                Next

                'サマリ結果を反映
                T00004SUMrow("SURYO") = SURYO_SUM.ToString("0.000")
                T00004SUMrow("DAISU") = DAISU_SUM.ToString("0")
                T00004SUMrow("JSURYO") = JSURYO_SUM.ToString("0.000")
                T00004SUMrow("JDAISU") = JDAISU_SUM.ToString("0")
                T00004SUMrow("SELECT") = "1"

                WW_TORICODE = T00004SUMrow("TORICODE")
                WW_OILTYPE = T00004SUMrow("OILTYPE")
                WW_KIJUNDATE = T00004SUMrow("KIJUNDATE")
                WW_ORDERORG = T00004SUMrow("ORDERORG")
                WW_SHIPORG = T00004SUMrow("SHIPORG")
                WW_DETAILNO = T00004SUMrow("DETAILNO")
                WW_TODOKEDATE = T00004SUMrow("TODOKEDATE")
                WW_SHUKODATE = T00004SUMrow("SHUKODATE")
                WW_SHUKADATE = T00004SUMrow("SHUKADATE")
                WW_SHUKABASHO = T00004SUMrow("SHUKABASHO")
                WW_GSHABAN = T00004SUMrow("GSHABAN")
                WW_SHAFUKU = T00004SUMrow("SHAFUKU")
                WW_STAFFCODE = T00004SUMrow("STAFFCODE")
                WW_TODOKECODE = T00004SUMrow("TODOKECODE")
                WW_TUMIOKIKBN = T00004SUMrow("TUMIOKIKBN")
                WW_PRODUCTCODE = T00004SUMrow("PRODUCTCODE")

            End If

        Next

        '○不要レコード削除
        CS0026TBLSORTget.TABLE = T00004SUMtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        CS0026TBLSORTget.FILTER = "SELECT = '1'"
        CS0026TBLSORTget.sort(T00004SUMtbl)

        '■■■ T00004SUMtblのDetailNO、SEQを再付番 ■■■

        '○DetailNO再付番
        Dim WW_DETAILNOcnt As Integer = 0
        WW_TORICODE = ""
        WW_OILTYPE = ""
        WW_SHUKADATE = ""
        WW_KIJUNDATE = ""
        WW_ORDERORG = ""
        WW_SHIPORG = ""
        For Each T00004SUMrow In T00004SUMtbl.Rows

            If T00004SUMrow("DELFLG") <> "1" Then
                If WW_TORICODE = T00004SUMrow("TORICODE") AndAlso
                   WW_OILTYPE = T00004SUMrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00004SUMrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = T00004SUMrow("ORDERORG") AndAlso
                   WW_SHIPORG = T00004SUMrow("SHIPORG") Then
                    WW_DETAILNOcnt = WW_DETAILNOcnt + 1
                    T00004SUMrow("DETAILNO") = WW_DETAILNOcnt.ToString("000")
                Else
                    WW_DETAILNOcnt = 1
                    T00004SUMrow("DETAILNO") = WW_DETAILNOcnt.ToString("000")

                    WW_TORICODE = T00004SUMrow("TORICODE")
                    WW_OILTYPE = T00004SUMrow("OILTYPE")
                    WW_KIJUNDATE = T00004SUMrow("KIJUNDATE")
                    WW_ORDERORG = T00004SUMrow("ORDERORG")
                    WW_SHIPORG = T00004SUMrow("SHIPORG")

                End If
            End If

        Next

        '○close

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' T00004tbl関連データ削除
    ''' </summary>
    ''' <param name="I_DATENOW">更新時刻</param>
    ''' <param name="O_RTN">RTNCODE</param>
    ''' <remarks>更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。</remarks>
    Protected Sub DBupdate_T4DELETE(ByVal I_DATENOW As Date, ByVal O_RTN As String)

        '■■■ T00004UPDtbl関連の荷主受注・配送受注を論理削除 ■■■　…　削除情報はT00004UPDtblに存在

        'Sort
        CS0026TBLSORTget.TABLE = T00004UPDtbl
        CS0026TBLSORTget.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,TIMSTP , DELFLG, OPERATION"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004UPDtbl)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･配送受注の現受注番号を一括論理削除
            Dim SQLStr As String =
                      " UPDATE T0004_HORDER             " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE CAMPCODE    = @P01       " _
                    & "    AND TORICODE    = @P02       " _
                    & "    AND OILTYPE     = @P03       " _
                    & "    AND ORDERORG    = @P04       " _
                    & "    AND SHIPORG     = @P05       " _
                    & "    AND KIJUNDATE   = @P06       " _
                    & "    AND DELFLG     <> '1'        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            Dim WW_TORICODE As String = ""
            Dim WW_OILTYPE As String = ""
            Dim WW_SHUKADATE As String = ""
            Dim WW_KIJUNDATE As String = ""
            Dim WW_ORDERORG As String = ""
            Dim WW_SHIPORG As String = ""

            For Each T00004UPDrow In T00004UPDtbl.Rows

                If T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00004UPDrow("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    If T00004UPDrow("TORICODE") <> WW_TORICODE OrElse
                       T00004UPDrow("OILTYPE") <> WW_OILTYPE OrElse
                       T00004UPDrow("KIJUNDATE") <> WW_KIJUNDATE OrElse
                       T00004UPDrow("ORDERORG") <> WW_ORDERORG OrElse
                       T00004UPDrow("SHIPORG") <> WW_SHIPORG Then

                        '○T00004UPDtbl関連の配送受注を論理削除

                        PARA01.Value = T00004UPDrow("CAMPCODE")
                        PARA02.Value = T00004UPDrow("TORICODE")
                        PARA03.Value = T00004UPDrow("OILTYPE")
                        PARA04.Value = T00004UPDrow("ORDERORG")
                        PARA05.Value = T00004UPDrow("SHIPORG")
                        PARA06.Value = T00004UPDrow("KIJUNDATE")

                        PARA11.Value = I_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.ExecuteNonQuery()

                        '関連するT3削除
                        DBupdate_T3DELETE(T00004UPDrow, I_DATENOW, O_RTN)

                        'ブレイクキー退避
                        WW_TORICODE = T00004UPDrow("TORICODE")
                        WW_OILTYPE = T00004UPDrow("OILTYPE")
                        WW_KIJUNDATE = T00004UPDrow("KIJUNDATE")
                        WW_ORDERORG = T00004UPDrow("ORDERORG")
                        WW_SHIPORG = T00004UPDrow("SHIPORG")
                    End If
                End If

            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0004_HORDER(old) DEL")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER(old) DEL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region

#Region "T0004テーブル入力関連"
    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToINP()

        '■■■ Detail変数設定 ■■■
        Master.CreateEmptyTable(T00004INPtbl)
        Dim WW_DetailMAX As Integer = 0

        WW_DetailMAX = WF_DViewRep1.Items.Count \ WF_REP_ROWSCNT.Value

        '■■■ DetailよりT00004INPtbl編集 ■■■
        'Detail入力レコード回数ループ
        For i As Integer = 0 To WW_DetailMAX - 1

            'Detail入力テーブル準備
            Dim T00004INProw = T00004INPtbl.NewRow

            T00004INProw("LINECNT") = 0
            T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00004INProw("TIMSTP") = 0
            T00004INProw("SELECT") = 0
            T00004INProw("HIDDEN") = 0

            T00004INProw("INDEX") = ""
            T00004INProw("CAMPCODE") = ""
            T00004INProw("CAMPCODENAME") = ""
            T00004INProw("TERMORG") = ""
            T00004INProw("TERMORGNAME") = ""
            T00004INProw("ORDERNO") = ""
            T00004INProw("DETAILNO") = ""
            T00004INProw("TRIPNO") = ""
            T00004INProw("DROPNO") = ""
            T00004INProw("SEQ") = ""
            T00004INProw("TORICODE") = ""
            T00004INProw("TORICODENAME") = ""
            T00004INProw("OILTYPE") = ""
            T00004INProw("OILTYPENAME") = ""
            T00004INProw("STORICODE") = ""
            T00004INProw("STORICODENAME") = ""
            T00004INProw("ORDERORG") = ""
            T00004INProw("ORDERORGNAME") = ""
            T00004INProw("SHUKODATE") = ""
            T00004INProw("KIKODATE") = ""
            T00004INProw("KIJUNDATE") = ""
            T00004INProw("SHUKADATE") = ""
            T00004INProw("TUMIOKI") = ""
            T00004INProw("TUMIOKIKBN") = ""
            T00004INProw("TUMIOKIKBNNAME") = ""
            T00004INProw("URIKBN") = ""
            T00004INProw("URIKBNNAME") = ""
            T00004INProw("STATUS") = ""
            T00004INProw("STATUSNAME") = ""
            T00004INProw("SHIPORG") = ""
            T00004INProw("SHIPORGNAME") = ""
            T00004INProw("SHUKABASHO") = ""
            T00004INProw("SHUKABASHONAME") = ""
            T00004INProw("INTIME") = ""
            T00004INProw("OUTTIME") = ""
            T00004INProw("SHUKADENNO") = ""
            T00004INProw("TUMISEQ") = ""
            T00004INProw("TUMIBA") = ""
            T00004INProw("GATE") = ""
            T00004INProw("GSHABAN") = ""
            T00004INProw("GSHABANLICNPLTNO") = ""
            T00004INProw("RYOME") = ""
            T00004INProw("CONTCHASSIS") = ""
            T00004INProw("CONTCHASSISLICNPLTNO") = ""
            T00004INProw("SHAFUKU") = ""
            T00004INProw("STAFFCODE") = ""
            T00004INProw("STAFFCODENAME") = ""
            T00004INProw("SUBSTAFFCODE") = ""
            T00004INProw("SUBSTAFFCODENAME") = ""
            T00004INProw("STTIME") = ""
            T00004INProw("TORIORDERNO") = ""
            T00004INProw("TODOKEDATE") = ""
            T00004INProw("TODOKETIME") = ""
            T00004INProw("TODOKECODE") = ""
            T00004INProw("TODOKECODENAME") = ""
            T00004INProw("PRODUCT1") = ""
            T00004INProw("PRODUCT1NAME") = ""
            T00004INProw("PRODUCT2") = ""
            T00004INProw("PRODUCT2NAME") = ""
            T00004INProw("PRODUCTCODE") = ""
            T00004INProw("PRODUCTNAME") = ""
            T00004INProw("PRATIO") = ""
            T00004INProw("SMELLKBN") = ""
            T00004INProw("SMELLKBNNAME") = ""
            T00004INProw("CONTNO") = ""
            T00004INProw("HTANI") = ""
            T00004INProw("HTANINAME") = ""
            T00004INProw("SURYO") = ""
            T00004INProw("SURYO_SUM") = ""
            T00004INProw("DAISU") = ""
            T00004INProw("DAISU_SUM") = ""
            T00004INProw("JSURYO") = ""
            T00004INProw("JSURYO_SUM") = ""
            T00004INProw("JDAISU") = ""
            T00004INProw("JDAISU_SUM") = ""
            T00004INProw("REMARKS1") = ""
            T00004INProw("REMARKS2") = ""
            T00004INProw("REMARKS3") = ""
            T00004INProw("REMARKS4") = ""
            T00004INProw("REMARKS5") = ""
            T00004INProw("REMARKS6") = ""
            T00004INProw("SHARYOTYPEF") = ""
            T00004INProw("TSHABANF") = ""
            T00004INProw("SHARYOTYPEB") = ""
            T00004INProw("TSHABANB") = ""
            T00004INProw("SHARYOTYPEB2") = ""
            T00004INProw("TSHABANB2") = ""
            T00004INProw("TAXKBN") = ""
            T00004INProw("TAXKBNNAME") = ""
            T00004INProw("JXORDERID") = ""
            T00004INProw("DELFLG") = ""

            T00004INProw("ADDR") = ""
            T00004INProw("DISTANCE") = ""
            T00004INProw("ARRIVTIME") = ""
            T00004INProw("NOTES1") = ""
            T00004INProw("NOTES2") = ""
            T00004INProw("NOTES3") = ""
            T00004INProw("NOTES4") = ""
            T00004INProw("NOTES5") = ""
            T00004INProw("NOTES6") = ""
            T00004INProw("NOTES7") = ""
            T00004INProw("NOTES8") = ""
            T00004INProw("NOTES9") = ""
            T00004INProw("NOTES10") = ""
            T00004INProw("STAFFNOTES1") = ""
            T00004INProw("STAFFNOTES2") = ""
            T00004INProw("STAFFNOTES3") = ""
            T00004INProw("STAFFNOTES4") = ""
            T00004INProw("STAFFNOTES5") = ""

            T00004INProw("SHARYOINFO1") = ""
            T00004INProw("SHARYOINFO2") = ""
            T00004INProw("SHARYOINFO3") = ""
            T00004INProw("SHARYOINFO4") = ""
            T00004INProw("SHARYOINFO5") = ""
            T00004INProw("SHARYOINFO6") = ""

            T00004INProw("WORK_NO") = ""

            For j As Integer = (i * WF_REP_ROWSCNT.Value) To ((i + 1) * WF_REP_ROWSCNT.Value - 1)
                If j <= (WF_DViewRep1.Items.Count - 1) Then

                    T00004INProw("WORK_NO") =
                        CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_MEISAINO"), System.Web.UI.WebControls.TextBox).Text

                    For col As Integer = 1 To WF_REP_COLSCNT.Value

                        If CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text <> "" Then
                            T00004INProw(CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text) =
                                CType(WF_DViewRep1.Items(j).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox).Text
                        End If

                    Next

                End If
            Next
            If WF_Sel_LINECNT.Text = "" Then
                T00004INProw("LINECNT") = 0
            Else
                If Not Integer.TryParse(WF_REP_LINECNT.Value, T00004INProw("LINECNT")) Then
                    T00004INProw("LINECNT") = 0
                End If
            End If
            T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00004INProw("TIMSTP") = "0"
            T00004INProw("SELECT") = 1                                      '対象
            T00004INProw("HIDDEN") = 0                                      '表示

            '条件指定・会社コードで置き換え（入力項目を減らす））
            T00004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T00004INProw("TERMORG") = WF_DEFORG.Text
            '出庫日
            Master.EraseCharToIgnore(WF_SHUKODATE.Text)
            T00004INProw("SHUKODATE") = WF_SHUKODATE.Text
            '出荷日
            Master.EraseCharToIgnore(WF_SHUKADATE.Text)
            T00004INProw("SHUKADATE") = WF_SHUKADATE.Text
            '届日
            Master.EraseCharToIgnore(WF_TODOKEDATE.Text)
            T00004INProw("TODOKEDATE") = WF_TODOKEDATE.Text
            '帰庫日
            Master.EraseCharToIgnore(WF_KIKODATE.Text)
            T00004INProw("KIKODATE") = WF_KIKODATE.Text
            '両目
            Master.EraseCharToIgnore(WF_RYOME.Text)
            T00004INProw("RYOME") = WF_RYOME.Text
            '受注番号
            Master.EraseCharToIgnore(WF_ORDERNO.Text)
            T00004INProw("ORDERNO") = WF_ORDERNO.Text
            '明細番号
            Master.EraseCharToIgnore(WF_DETAILNO.Text)
            T00004INProw("DETAILNO") = WF_DETAILNO.Text
            '油種
            Master.EraseCharToIgnore(WF_OILTYPE.Text)
            T00004INProw("OILTYPE") = WF_OILTYPE.Text
            '取引先
            Master.EraseCharToIgnore(WF_TORICODE.Text)
            T00004INProw("TORICODE") = WF_TORICODE.Text
            '受注部署
            Master.EraseCharToIgnore(WF_ORDERORG.Text)
            T00004INProw("ORDERORG") = WF_ORDERORG.Text
            '出荷部署
            Master.EraseCharToIgnore(WF_SHIPORG.Text)
            T00004INProw("SHIPORG") = WF_SHIPORG.Text
            '販売店
            Master.EraseCharToIgnore(WF_STORICODE.Text)
            T00004INProw("STORICODE") = WF_STORICODE.Text
            '売上計上基準
            Master.EraseCharToIgnore(WF_URIKBN.Text)
            T00004INProw("URIKBN") = WF_URIKBN.Text
            '業務車番
            Master.EraseCharToIgnore(WF_GSHABAN.Text)
            T00004INProw("GSHABAN") = WF_GSHABAN.Text
            'コンテナシャーシ
            Master.EraseCharToIgnore(WF_CONTCHASSIS.Text)
            T00004INProw("CONTCHASSIS") = WF_CONTCHASSIS.Text
            '車腹
            Master.EraseCharToIgnore(WF_SHAFUKU.Text)
            T00004INProw("SHAFUKU") = WF_SHAFUKU.Text
            '積置区分
            Master.EraseCharToIgnore(WF_TUMIOKIKBN.Text)
            T00004INProw("TUMIOKIKBN") = WF_TUMIOKIKBN.Text
            'トリップ
            Master.EraseCharToIgnore(WF_TRIPNO.Text)
            T00004INProw("TRIPNO") = WF_TRIPNO.Text
            'ドロップ
            Master.EraseCharToIgnore(WF_DROPNO.Text)
            T00004INProw("DROPNO") = WF_DROPNO.Text
            '乗務員
            Master.EraseCharToIgnore(WF_STAFFCODE.Text)
            T00004INProw("STAFFCODE") = WF_STAFFCODE.Text
            '副乗務員
            Master.EraseCharToIgnore(WF_SUBSTAFFCODE.Text)
            T00004INProw("SUBSTAFFCODE") = WF_SUBSTAFFCODE.Text
            '出勤時間
            Master.EraseCharToIgnore(WF_STTIME.Text)
            T00004INProw("STTIME") = WF_STTIME.Text

            '端末組織
            T00004INProw("TERMORG") = WF_DEFORG.Text
            'JXオーダー
            T00004INProw("JXORDERID") = WF_JXORDERID.Text
            T00004INProw("JXORDERSTATUS") = WF_JXORDERSTATUS.Text

            '○名称付与
            CODENAME_set(T00004INProw)

            '入力テーブル作成
            T00004INPtbl.Rows.Add(T00004INProw)

        Next

    End Sub


    ''' <summary>
    ''' 入力データ登録
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbltoT4tbl(ByRef O_RTNCODE As String)

        '■■■ 数量ゼロは読み飛ばし ■■■
        For i As Integer = T00004INPtbl.Rows.Count - 1 To 0 Step -1
            Dim T00004INProw = T00004INPtbl.Rows(i)
            '出荷前々日以降は、データ取込対象外とする
            If Val(T00004INProw("JSURYO")) = 0 Then
                If Val(T00004INProw("SURYO")) = 0 Then
                    '数量なしは無視
                    T00004INPtbl.Rows(i).Delete()
                End If
            End If
        Next


        '■■■ 項目チェック ■■■
        '●チェック処理
        INPtbl_CHEK(WW_ERRCODE)

        INPtbl_CHEK_DATE(WW_ERRCODE)

        '■■■ 変更有無チェック ■■■    
        '…　Grid画面へ別明細追加：T00004INProw("WORK_NO") = ""
        '　　変更発生　　：T00004INProw("OPERATION")へ"更新"or"エラー"を設定

        '●変更有無取得　　　     ※Excelは全て新規。全て更新とする。
        For Each T00004INProw In T00004INPtbl.Rows
            '数量・台数未設定時は対象外
            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            'エラーは設定しない
            If T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            If T00004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If

            T00004INProw("WORK_NO") = ""
            T00004INProw("LINECNT") = 0

        Next


        '■■■ 更新前処理（入力情報へ乗務員引継ぎ設定 ※JXオーダーのみ対象）　■■■
        For Each T00004INProw In T00004INPtbl.Rows
            '数量・台数未設定時は対象外
            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For
            'JXオーダー以外は対象外
            If String.IsNullOrEmpty(T00004INProw("JXORDERID")) Then Continue For

            '乗務員コードが空なら設定（基本は空）
            If T00004INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA AndAlso
                String.IsNullOrEmpty(T00004INProw("STAFFCODE")) Then
                For Each T00004TBLrow In T00004tbl.Rows
                    If T00004TBLrow("JXORDERID") = T00004INProw("JXORDERID") Then
                        T00004INProw("STAFFCODE") = T00004TBLrow("STAFFCODE")
                        T00004INProw("STAFFCODENAME") = T00004TBLrow("STAFFCODENAME")
                        T00004INProw("SUBSTAFFCODE") = T00004TBLrow("SUBSTAFFCODE")
                        T00004INProw("SUBSTAFFCODENAME") = T00004TBLrow("SUBSTAFFCODENAME")
                    End If
                Next
            End If
        Next

        '■■■ 更新前処理（入力情報へ受注番号設定、Grid画面の同一行情報を削除）　■■■
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)
            '数量・台数未設定時は対象外
            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            For j As Integer = 0 To T00004tbl.Rows.Count - 1

                '状態をクリア設定
                EditOperationText(T00004tbl.Rows(j), False)

                If T00004INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then

                    'Grid画面行追加の場合は受注番号を取得
                    If T00004tbl.Rows(j)("TORICODE") = T00004INProw("TORICODE") AndAlso
                       T00004tbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                       T00004tbl.Rows(j)("KIJUNDATE") = T00004INProw("KIJUNDATE") AndAlso
                       T00004tbl.Rows(j)("ORDERORG") = T00004INProw("ORDERORG") AndAlso
                       T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") Then

                        T00004INProw("ORDERNO") = T00004tbl.Rows(j)("ORDERNO")
                        T00004INProw("DETAILNO") = "000"

                    End If

                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If CompareOrder(T00004tbl.Rows(j), T00004INProw) Then

                        T00004INProw("LINECNT") = T00004tbl.Rows(j)("LINECNT")

                    End If

                    'EXCELは同一受注条件レコードを論理削除（T4実態が存在する場合、物理削除。）
                    If T00004tbl.Rows(j)("GSHABAN") = T00004INProw("GSHABAN") AndAlso
                       T00004tbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                       T00004tbl.Rows(j)("SHUKODATE") = T00004INProw("SHUKODATE") AndAlso
                       T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") AndAlso
                       T00004tbl.Rows(j)("DELFLG") <> "1" Then

                        If Val(T00004tbl.Rows(j)("JSURYO")) = 0 Then
                            T00004tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            T00004tbl.Rows(j)("DELFLG") = "1"   '削除
                            T00004tbl.Rows(j)("HIDDEN") = "1"   '非表示
                            T00004tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                        Else
                            T00004INProw("DELFLG") = "1"
                        End If
                    Else
                        If T00004tbl.Rows(j)("SHIPORG") = WF_DEFORG.Text AndAlso
                            T00004tbl.Rows(j)("ORDERORG") <> T00004tbl.Rows(j)("SHIPORG") Then
                            If T00004tbl.Rows(j)("GSHABAN") = "" AndAlso
                               T00004tbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                               T00004tbl.Rows(j)("SHUKODATE") = T00004INProw("SHUKODATE") AndAlso
                               T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") AndAlso
                               T00004tbl.Rows(j)("DELFLG") <> "1" Then
                                If Val(T00004tbl.Rows(j)("JSURYO")) = 0 Then
                                    T00004tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                                    T00004tbl.Rows(j)("DELFLG") = "1"   '削除
                                    T00004tbl.Rows(j)("HIDDEN") = "1"   '非表示
                                    T00004tbl.Rows(j)("SELECT") = "0"   '明細表示対象外
                                Else
                                    T00004INProw("DELFLG") = "1"
                                End If
                            End If
                        End If
                    End If

                End If

            Next
        Next

        'T00004INPtblの削除データを物理削除
        CS0026TBLSORTget.TABLE = T00004INPtbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1'"
        CS0026TBLSORTget.sort(T00004INPtbl)

        'T00004tblの削除データを物理削除
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = "DELFLG <> '1' or TIMSTP <> 0 "
        CS0026TBLSORTget.sort(T00004tbl)

        '■■■ 更新前処理（入力情報へ操作を反映）　■■■
        INPtbl_PreUpdate1()

        '■■■ 更新前処理（受注画面で自動作成された関連受注を削除）　■■■
        INPtbl_PreUpdateDel()

        '■■■ 更新前処理（入力情報へLINECNTを付番）　■■■
        INPtbl_PreUpdate2()

        '■■■ 更新前処理（入力情報へ暫定受注番号を付番）　■■■
        INPtbl_PreUpdate3()
    End Sub

    ''' <summary>
    ''' 入力データチェック（出庫日範囲）
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_CHEK_DATE(ByRef O_RTNCODE As String)

        '●関連チェック処理
        Dim WW_DATE As Date
        Dim WW_LOGONYMD As Date = CS0050SESSION.LOGONDATE
        For i As Integer = T00004INPtbl.Rows.Count - 1 To 0 Step -1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            If Date.TryParse(T00004INProw("SHUKODATE"), WW_DATE) Then

                '出庫前々日以降は、データ取込対象外とする
                '出荷日<当日は処理対象外（出荷当日までOK）
                If WW_DATE < WW_LOGONYMD Then
                    Dim WW_ERR_MES As String = "・更新できないレコード(過去日データ)です。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00004INProw("TORICODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00004INProw("TODOKECODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00004INProw("SHUKABASHO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00004INProw("SHUKODATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00004INProw("TODOKEDATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00004INProw("SHUKADATE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00004INProw("GSHABAN") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00004INProw("STAFFCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00004INProw("PRODUCTCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00004INProw("TRIPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00004INProw("DROPNO") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00004INProw("DELFLG") & " "
                    rightview.AddErrorReport(WW_ERR_MES)

                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERRCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 車検切れ・容器検査切れチェック対象部署
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Function IsInspectionOrg(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_RTN As String) As Boolean
        ' 車検切れ・容器検査切れチェック対象部署
        Static INSPECTION_CHECK_ORG As List(Of String) = Nothing

        If Not IsNothing(INSPECTION_CHECK_ORG) Then
            Return INSPECTION_CHECK_ORG.Contains(I_ORGCODE)
        End If

        Const CLASS_CODE As String = "INSPECTIONORG"
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using GS0032 As New GS0032FIXVALUElst
                GS0032.CAMPCODE = I_COMPCODE
                GS0032.CLAS = CLASS_CODE
                GS0032.STDATE = Date.Now
                GS0032.ENDDATE = Date.Now
                GS0032.GS0032FIXVALUElst()
                If Not isNormal(GS0032.ERR) Then
                    O_RTN = GS0032.ERR
                    Return False
                End If
                INSPECTION_CHECK_ORG = New List(Of String)
                For Each item As ListItem In GS0032.VALUE1.Items
                    INSPECTION_CHECK_ORG.Add(item.Value)
                Next
                '存在する場合TRUE、しない場合FALSEを帰す
                Return (Not IsNothing(GS0032.VALUE1.Items.FindByValue(I_ORGCODE)))
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GRT0004"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSPECTIONORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return False
        End Try

    End Function

    ''' <summary>
    ''' 更新前処理（受注画面で自動作成された関連受注を削除）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdateDel()

        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            If T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                For j As Integer = 0 To T00004tbl.Rows.Count - 1
                    '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                    If T00004tbl.Rows(j)("TORICODE") = T00004INProw("TORICODE") AndAlso
                       T00004tbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                       T00004tbl.Rows(j)("KIJUNDATE") = T00004INProw("KIJUNDATE") AndAlso
                       T00004tbl.Rows(j)("ORDERORG") = T00004INProw("ORDERORG") AndAlso
                       T00004tbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") AndAlso
                       T00004tbl.Rows(j)("SHUKODATE") = T00004INProw("SHUKODATE") AndAlso
                       T00004tbl.Rows(j)("TRIPNO") = "000" Then

                        T00004tbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        T00004tbl.Rows(j)("DELFLG") = "1"

                    End If

                Next
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へ操作を反映）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate1()

        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            If T00004INProw("WORK_NO") = "" And Val(T00004INProw("SURYO")) = 0 And Val(T00004INProw("DAISU")) = 0 Then
            Else
                If T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                    For j As Integer = i To T00004INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00004INPtbl.Rows(j), T00004INProw) Then

                            T00004INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                        End If
                    Next
                End If

                If T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING Then
                    For j As Integer = 0 To T00004INPtbl.Rows.Count - 1
                        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
                        If CompareOrder(T00004INPtbl.Rows(j), T00004INProw) AndAlso
                           T00004INPtbl.Rows(j)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then

                            T00004INPtbl.Rows(j)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                        End If
                    Next
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へLINECNTを付番）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate2()

        '●LINECNTを付番
        'sort
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)


        Dim WW_ORDERNO As Integer = 0
        Dim WW_DETAILNO As Integer = 0
        Dim WW_LINECNT As Integer = 0
        Dim WW_CNT As Integer = 0

        '受注番号初期値セット
        If T00004tbl.Rows.Count = 0 Then
            WW_LINECNT = 0
        Else
            WW_LINECNT = CInt(T00004tbl.Rows(T00004tbl.Rows.Count - 1)("LINECNT"))
        End If

        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            '新規有効明細
            If T00004INProw("WORK_NO") = "" AndAlso (Val(T00004INProw("SURYO")) <> 0 OrElse Val(T00004INProw("DAISU")) <> 0) Then

                If Val(T00004INProw("LINECNT")) = 0 Then

                    WW_LINECNT = WW_LINECNT + 1
                    WW_CNT = 0

                    '同一条件レコードへも反映
                    For j As Integer = 0 To T00004INPtbl.Rows.Count - 1
                        If T00004INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00004INPtbl.Rows(j)("SURYO")) <> 0 OrElse Val(T00004INPtbl.Rows(j)("DAISU")) <> 0) Then

                            If CompareOrder(T00004INPtbl.Rows(j), T00004INProw) Then

                                WW_CNT = WW_CNT + 1
                                T00004INPtbl.Rows(j)("LINECNT") = WW_LINECNT.ToString("0")
                                T00004INPtbl.Rows(j)("SEQ") = WW_CNT.ToString("00")

                            End If

                        End If
                    Next
                Else
                    Dim WW_FIND = "OFF"
                    WW_CNT = 0
                    '同一条件レコードへも反映
                    For j As Integer = 0 To T00004INPtbl.Rows.Count - 1
                        If T00004INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00004INPtbl.Rows(j)("SURYO")) <> 0 OrElse Val(T00004INPtbl.Rows(j)("DAISU")) <> 0) Then

                            If CompareOrder(T00004INPtbl.Rows(j), T00004INProw) Then

                                If T00004INPtbl.Rows(j)("SEQ") = "01" Then
                                    WW_FIND = "ON"
                                    Exit For
                                End If
                            End If

                        End If
                    Next
                    '枝番（SEQ）="01"が存在しない場合、SEQの振り直す
                    If WW_FIND = "OFF" Then
                        For j As Integer = 0 To T00004INPtbl.Rows.Count - 1
                            If T00004INPtbl.Rows(j)("WORK_NO") = "" AndAlso (Val(T00004INPtbl.Rows(j)("SURYO")) <> 0 OrElse Val(T00004INPtbl.Rows(j)("DAISU")) <> 0) Then

                                If CompareOrder(T00004INPtbl.Rows(j), T00004INProw) Then

                                    WW_CNT = WW_CNT + 1
                                    T00004INPtbl.Rows(j)("SEQ") = WW_CNT.ToString("00")
                                End If

                            End If
                        Next
                    End If
                End If
            End If

        Next

    End Sub

    ''' <summary>
    ''' 更新前処理（入力情報へ暫定受注番号を付番）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub INPtbl_PreUpdate3()

        Dim WW_ORDERNO As Integer = 0
        Dim WW_DETAILNO As Integer = 0

        '●暫定受注番号を付番
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            '数量・台数未設定時は対象外
            If T00004INProw("WORK_NO") = "" AndAlso Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then Continue For

            '追加明細("WORK_NO")
            If T00004INProw("WORK_NO") = "" Then

                WW_ORDERNO = WW_ORDERNO + 1
                WW_DETAILNO = 0

                'T4INPtblへも反映（次レコード処理用）
                For j As Integer = 0 To T00004INPtbl.Rows.Count - 1
                    '数量・台数未設定時は対象外
                    If T00004INPtbl.Rows(j)("WORK_NO") = "" AndAlso Val(T00004INPtbl.Rows(j)("SURYO")) = 0 AndAlso Val(T00004INPtbl.Rows(j)("DAISU")) = 0 Then Continue For

                    If T00004INPtbl.Rows(j)("ORDERNO") = "" Then

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00004INPtbl.Rows(j)("TORICODE") = T00004INProw("TORICODE") AndAlso
                            T00004INPtbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                            T00004INPtbl.Rows(j)("ORDERORG") = T00004INProw("ORDERORG") AndAlso
                            T00004INPtbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") AndAlso
                            T00004INPtbl.Rows(j)("KIJUNDATE") = T00004INProw("KIJUNDATE") Then

                            T00004INPtbl.Rows(j)("ORDERNO") = "新" & WW_ORDERNO.ToString("00")
                            WW_DETAILNO = WW_DETAILNO + 1
                            T00004INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00004INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    Else

                        '受注判定基準により同一受注に、新受注番号を付与
                        If T00004INPtbl.Rows(j)("WORK_NO") = "" AndAlso
                            T00004INPtbl.Rows(j)("TORICODE") = T00004INProw("TORICODE") AndAlso
                            T00004INPtbl.Rows(j)("OILTYPE") = T00004INProw("OILTYPE") AndAlso
                            T00004INPtbl.Rows(j)("ORDERORG") = T00004INProw("ORDERORG") AndAlso
                            T00004INPtbl.Rows(j)("SHIPORG") = T00004INProw("SHIPORG") AndAlso
                            T00004INPtbl.Rows(j)("KIJUNDATE") = T00004INProw("KIJUNDATE") Then

                            WW_DETAILNO = WW_DETAILNO + 1
                            T00004INPtbl.Rows(j)("DETAILNO") = WW_DETAILNO.ToString("000")
                            T00004INPtbl.Rows(j)("WORK_NO") = "0"

                        End If
                    End If

                Next

                '○ステータス設定
                If T00004INProw("STATUS") = T4STATUS.RESULT Then        '実績
                Else
                    If T00004INProw("GSHABAN") <> "" AndAlso
                        T00004INProw("STAFFCODE") <> "" AndAlso
                        T00004INProw("PRODUCTCODE") <> "" Then
                        T00004INProw("STATUS") = T4STATUS.MANNING
                    Else
                        T00004INProw("STATUS") = T4STATUS.ORDER
                    End If
                End If

                T00004INProw("STATUSNAME") = ""
                CODENAME_get("STATUS", T00004INProw("STATUS"), T00004INProw("STATUSNAME"), WW_DUMMY)

                '○T00004INProwをT00004tblへ追加
                T00004tbl.ImportRow(T00004INProw)

            Else

                If T00004INProw("OPERATION") <> C_LIST_OPERATION_CODE.NODATA Then
                    '○ステータス設定
                    If T00004INProw("STATUS") = T4STATUS.RESULT Then        '実績
                    Else
                        If T00004INProw("GSHABAN") <> "" AndAlso
                           T00004INProw("STAFFCODE") <> "" AndAlso
                           T00004INProw("PRODUCTCODE") <> "" Then
                            T00004INProw("STATUS") = T4STATUS.MANNING
                        Else
                            T00004INProw("STATUS") = T4STATUS.ORDER
                        End If
                    End If

                    T00004INProw("STATUSNAME") = ""
                    CODENAME_get("STATUS", T00004INProw("STATUS"), T00004INProw("STATUSNAME"), WW_DUMMY)

                    '○T00004INProwをT00004tblへ追加
                    T00004tbl.ImportRow(T00004INProw)
                End If

            End If

        Next

    End Sub

    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_CHEK(ByRef O_RTNCODE As String)

        '○インターフェイス初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEerr As String = ""
        Dim WW_SEQ As Integer = 0
        Dim WW_CS0024FCHECKVAL As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_TEXT As String = ""

        WW_ERRLIST.Clear()
        If IsNothing(S0013tbl) Then
            S0013tbl = New DataTable
        End If

        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1

            Dim T00004INProw = T00004INPtbl.Rows(i)

            WW_LINEerr = C_MESSAGE_NO.NORMAL

            '初期クリア
            T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA

            '数量・台数未設定時はチェック対象外
            If T00004INProw("WORK_NO") = "" AndAlso T00004INProw("SURYO") = "" AndAlso T00004INProw("DAISU") = "" Then Continue For


            '■■■ 単項目チェック(ヘッダー情報) ■■■

            Dim WW_TORI_FLG As String = ""
            '■キー項目(取引先コード：TORICODE)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TORICODE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TORICODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Val(WW_CS0024FCHECKVAL) = 0 Then
                    CODENAME_get("TORICODE", T00004INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("TORICODENAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    Else
                        WW_TORI_FLG = "OK"
                    End If
                Else
                    T00004INProw("TORICODE") = WW_CS0024FCHECKVAL
                    '○LeftBox存在チェック
                    CODENAME_get("TORICODE", T00004INProw("TORICODE"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("TORICODENAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    Else
                        WW_TORI_FLG = "OK"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            Dim WW_OILTYPE_FLG As String = ""
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("OILTYPE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OILTYPE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                Else
                    T00004INProw("OILTYPE") = WW_CS0024FCHECKVAL
                    If Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) AndAlso work.WF_SEL_OILTYPE.Text <> T00004INProw("OILTYPE") Then
                        WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                        WW_CheckMES2 = " 条件入力で指定された油種と異ります( " & T00004INProw("OILTYPE") & ") "
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    Else
                        WW_OILTYPE_FLG = "OK"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■キー項目(出荷日：SHUKADATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00004INProw("SHUKADATE")) Then
                T00004INProw("SHUKADATE") = T00004INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SHUKADATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SHUKADATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            Dim WW_ORG_FLG As String = ""
            '■キー項目(受注受付部署：ORDERORG)
            '○デフォルト　…　初期登録時のみサーバ組織を設定
            If T00004INProw("ORDERORG") = "" Then
                T00004INProw("ORDERORG") = WF_DEFORG.Text
            End If

            If T00004INProw("ORDERORG") <> "" Then
                '○LeftBoxより名称取得
                CODENAME_get("ORDERORG", T00004INProw("ORDERORG"), WW_TEXT, WW_DUMMY)
                T00004INProw("ORDERORGNAME") = WW_TEXT
                WW_ORG_FLG = "OK"
            End If

            '■キー項目(請求取引先コード：STORICODE)
            '○取引先マスタより初期値取得
            If T00004INProw("STORICODE") = "" Then
                Dim WW_NAMES As String = ""
                GetSTori(WF_TORICODE.Text, T00004INProw("STORICODE"), WW_NAMES)
                WF_STORICODE.Text = T00004INProw("STORICODE")
                WF_STORICODE_TEXT.Text = WW_NAMES
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("STORICODE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STORICODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Val(WW_CS0024FCHECKVAL) = 0 Then
                Else
                    T00004INProw("STORICODE") = WW_CS0024FCHECKVAL

                    '○LeftBox存在チェック
                    CODENAME_get("STORICODE", T00004INProw("STORICODE"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("STORICODENAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(請求取引先コード)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請求取引先コード)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■明細項目(出荷部署：SHIPORG)

            '○デフォルト
            If T00004INProw("SHIPORG") = "" Then
                T00004INProw("SHIPORG") = WF_DEFORG.Text
            End If


            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SHIPORG")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHIPORG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SHIPORG") = WW_CS0024FCHECKVAL

                '○LeftBox存在チェック
                If Not String.IsNullOrEmpty(T00004INProw("SHIPORG")) Then
                    CODENAME_get("SHIPORG", T00004INProw("SHIPORG"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("SHIPORGNAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■明細項目(出庫日：SHUKODATE)
            '○必須・項目属性チェック
            Dim WW_SHUKODATEERR As String = "OFF"
            WW_CS0024FCHECKVAL = T00004INProw("SHUKODATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SHUKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_SHUKODATEERR = "ON"
                WW_CheckMES1 = "・エラーが存在します。(出庫日)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '*******************  業務車番チェック  *********************

            Dim WW_CHKFLG As String = "ON"

            '■明細項目(業務車番：GSHABAN)
            '○必須・項目属性チェック
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text AndAlso
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                '異なる拠点データ投入時はチェック対象外
                If T00004INProw("GSHABAN") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If
            If T00004INProw("JXORDERID") <> "" Then
                'JXオーダー時はチェック対象外
                WW_CHKFLG = "OFF"
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00004INProw("GSHABAN")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GSHABAN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00004INProw("GSHABAN") = WW_CS0024FCHECKVAL

                    '○LeftBox存在チェック
                    If T00004INProw("GSHABAN") <> "" Then
                        CODENAME_get("GSHABAN", T00004INProw("GSHABAN"), WW_TEXT, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(業務車番)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(業務車番)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            '*******************  乗務員チェック  *********************

            '■明細項目(乗務員コード：STAFFCODE)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                '異なる拠点データ投入時はチェック対象外
                If T00004INProw("STAFFCODE") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If
            If T00004INProw("JXORDERID") <> "" Then
                'JXオーダー時はチェック対象外
                WW_CHKFLG = "OFF"
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00004INProw("STAFFCODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    Else
                        T00004INProw("STAFFCODE") = WW_CS0024FCHECKVAL

                        '○LeftBox存在チェック
                        CODENAME_get("STAFFCODE", T00004INProw("STAFFCODE"), WW_TEXT, WW_RTN_SW)
                        T00004INProw("STAFFCODENAME") = WW_TEXT
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(乗務員コード)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            '*******************  副乗務員チェック  *********************

            '■明細項目(副乗務員コード：SUBSTAFFCODE)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                '異なる拠点データ投入時はチェック対象外
                If T00004INProw("SUBSTAFFCODE") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If
            If T00004INProw("JXORDERID") <> "" Then
                'JXオーダー時はチェック対象外
                WW_CHKFLG = "OFF"
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00004INProw("SUBSTAFFCODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SUBSTAFFCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    Else
                        T00004INProw("SUBSTAFFCODE") = WW_CS0024FCHECKVAL

                        '○LeftBox存在チェック
                        CODENAME_get("SUBSTAFFCODE", T00004INProw("SUBSTAFFCODE"), WW_TEXT, WW_RTN_SW)
                        T00004INProw("SUBSTAFFCODENAME") = WW_TEXT
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(副乗務員コード)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            '*******************  出荷場所チェック  *********************

            '■明細項目(出荷場所：SHUKABASHO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SHUKABASHO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                If CInt(WW_CS0024FCHECKVAL) = 0 Then
                Else
                    T00004INProw("SHUKABASHO") = WW_CS0024FCHECKVAL

                    '○LeftBox存在チェック
                    CODENAME_get("SHUKABASHO", T00004INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("SHUKABASHONAME") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                    End If
                End If
            Else
                CODENAME_get("SHUKABASHO", T00004INProw("SHUKABASHO"), WW_TEXT, WW_RTN_SW)
                T00004INProw("SHUKABASHONAME") = WW_TEXT
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・エラーが存在します。(出荷場所)"
                    WW_CheckMES2 = " マスタに存在しません。"
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If
            End If

            '■明細項目(帰庫日：KIKODATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00004INProw("KIKODATE")) Then
                T00004INProw("KIKODATE") = T00004INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("KIKODATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KIKODATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("KIKODATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・エラーが存在します。(帰庫日)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(車腹：SHAFUKU)
            '○デフォルト
            '業務車番より、車腹を再設定
            WW_CHKFLG = "ON"
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                If T00004INProw("SHAFUKU") = "" Then
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                T00004INProw("SHAFUKU") = ""
                Dim item = WF_ListSHAFUKU.Items.FindByText(T00004INProw("GSHABAN"))
                If Not IsNothing(item) Then
                    T00004INProw("SHAFUKU") = item.Value
                End If

                '○必須・項目属性チェック
                WW_CS0024FCHECKVAL = T00004INProw("SHAFUKU")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHAFUKU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'データ存在チェック（上記チェック方法がNUMのため、ゼロ埋めデータが出来てしまう）
                    If String.IsNullOrEmpty(T00004INProw("SHAFUKU")) AndAlso CInt(WW_CS0024FCHECKVAL) <> 0 Then
                        WW_CheckMES1 = "・エラーが存在します。(車腹登録なし)"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                    Else
                        T00004INProw("SHAFUKU") = WW_CS0024FCHECKVAL
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(車腹)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            WW_CHKFLG = "ON"
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                If T00004INProw("TRIPNO") = "" Then
                    WW_SEQ += 1
                    T00004INProw("TRIPNO") = WW_SEQ.ToString("000")
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                '■明細項目(トリップ：TRIPNO)
                '○必須・項目属性チェック
                WW_CS0024FCHECKVAL = T00004INProw("TRIPNO")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRIPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00004INProw("TRIPNO") = WW_CS0024FCHECKVAL
                Else
                    WW_CheckMES1 = "・エラーが存在します。(トリップ)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            '■明細項目(ドロップ：DROPNO)
            '○必須・項目属性チェック
            WW_CHKFLG = "ON"
            If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
               T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                If T00004INProw("DROPNO") = "" Then
                    T00004INProw("DROPNO") = "000"
                    WW_CHKFLG = "OFF"
                End If
            End If

            If WW_CHKFLG = "ON" Then
                WW_CS0024FCHECKVAL = T00004INProw("DROPNO")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DROPNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    T00004INProw("DROPNO") = WW_CS0024FCHECKVAL
                Else
                    WW_CheckMES1 = "・エラーが存在します。(ドロップ)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            End If

            '■明細項目(コンテナシャーシ：CONTCHASSIS)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("CONTCHASSIS")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTCHASSIS", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("CONTCHASSIS") = WW_CS0024FCHECKVAL

                '○LeftBox存在チェック
                If T00004INProw("CONTCHASSIS") <> "" Then
                    CODENAME_get("CONTCHASSIS", T00004INProw("CONTCHASSIS"), WW_TEXT, WW_RTN_SW)
                    T00004INProw("CONTCHASSISLICNPLTNO") = WW_TEXT
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。(コンテナシャーシ)"
                        WW_CheckMES2 = " マスタに存在しません。"
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(コンテナシャーシ)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '*******************  日付・時間チェック  *********************

            '■キー項目(届日：TODOKEDATE)
            '○デフォルト
            If String.IsNullOrEmpty(T00004INProw("TODOKEDATE")) Then
                T00004INProw("TODOKEDATE") = T00004INProw("SHUKODATE")
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TODOKEDATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKEDATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("TODOKEDATE") = WW_CS0024FCHECKVAL      'yyyy/MM/dd
            Else
                WW_CheckMES1 = "・エラーが存在します。(届日エラー)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(時間指定（入構）：INTIME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("INTIME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "INTIME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("INTIME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(時間指定（入構）)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(時間指定（出構）：OUTTIME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("OUTTIME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "OUTTIME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("OUTTIME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(時間指定（出構）)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(出勤時間：STTIME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("STTIME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("STTIME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(出勤時間)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(時間指定（配送）：TODOKETIME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TODOKETIME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKETIME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("TODOKETIME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(時間指定（配送）)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '*******************  品名チェック  *********************

            WW_CHKFLG = "ON"
            If T00004INProw("JXORDERID") <> "" Then
                'JXオーダー時はチェック対象外
                WW_CHKFLG = "OFF"
            End If
            '・明細項目(品名コード：PRODUCTCODE)
            If WW_CHKFLG = "ON" Then

                WW_CS0024FCHECKVAL = T00004INProw("PRODUCTCODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRODUCTCODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If Not String.IsNullOrEmpty(WW_CS0024FCHECKVAL) Then
                        T00004INProw("PRODUCTCODE") = WW_CS0024FCHECKVAL

                        'LeftBox存在チェック
                        CODENAME_get("PRODUCTCODE", T00004INProw("PRODUCTCODE"), WW_TEXT, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。（品名コード）"
                            WW_CheckMES2 = "マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。（品名コード）"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

                '■明細項目(品名１：PRODUCT1)
                '■明細項目(品名２：PRODUCT2)
                If T00004INProw("PRODUCTCODE") <> "" AndAlso T00004INProw("PRODUCTCODE").ToString.Length = 11 Then
                    Dim productCode As String = T00004INProw("PRODUCTCODE").ToString
                    T00004INProw("PRODUCT1") = productCode.Substring(4, 2)
                    T00004INProw("PRODUCT2") = productCode.Substring(6, 5)
                End If
            End If

            '*******************  届先チェック  *********************

            '■明細項目(届先コード：TODOKECODE)
            '○必須・項目属性チェック
            ' JX or COSMOはチェック対象外
            If T00004INProw("TODOKECODE").ToString.StartsWith(C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX) = False AndAlso
               T00004INProw("TODOKECODE").ToString.StartsWith(C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO) = False Then
                WW_CS0024FCHECKVAL = T00004INProw("TODOKECODE")
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If CInt(WW_CS0024FCHECKVAL) = 0 Then
                    Else
                        '                        T00004INProw("TODOKECODE") = WW_CS0024FCHECKVAL

                        '○LeftBox存在チェック
                        CODENAME_get("TODOKECODE", T00004INProw("TODOKECODE"), WW_TEXT, WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00004INProw("SHIPORG"), T00004INProw("TORICODE"), "1"))
                        T00004INProw("TODOKECODENAME") = WW_TEXT
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・エラーが存在します。(届先コード)"
                            WW_CheckMES2 = " マスタに存在しません。"
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(届先コード)"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If

            Else
                '○LeftBox存在チェック
                CODENAME_get("TODOKECODE", T00004INProw("TODOKECODE"), T00004INProw("TODOKECODENAME"), WW_RTN_SW, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, T00004INProw("SHIPORG"), T00004INProw("TORICODE"), "1"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・エラーが存在します。(届先コード)"
                    WW_CheckMES2 = " マスタに存在しません。"
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
                End If
            End If


            '*******************  数量チェック  *********************

            '■明細項目(数量：SURYO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SURYO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SURYO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                'データ存在チェック
                If String.IsNullOrEmpty(T00004INProw("SURYO")) Then
                    T00004INProw("SURYO") = ""
                Else
                    T00004INProw("SURYO") = WW_CS0024FCHECKVAL
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(数量)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(台数：DAISU)
            '○デフォルト
            If T00004INProw("OILTYPE") <> "04" Then
                T00004INProw("DAISU") = 1
            End If

            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("DAISU")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DAISU", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("DAISU") = CInt(WW_CS0024FCHECKVAL)
            Else
                WW_CheckMES1 = "・エラーが存在します。(台数不正)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '*******************  その他チェック  *********************



            '■明細項目(積置区分：TUMIOKIKBN)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TUMIOKIKBN")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("TUMIOKIKBN") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(積置区分)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(Ｐ比率：PRATIO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("PRATIO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PRATIO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                'データ存在チェック
                If String.IsNullOrEmpty(T00004INProw("PRATIO")) Then
                    T00004INProw("PRATIO") = ""
                Else
                    T00004INProw("PRATIO") = WW_CS0024FCHECKVAL
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(Ｐ比率)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(臭有無：SMELLKBN)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SMELLKBN")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SMELLKBN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SMELLKBN") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(臭有無)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(コンテナ番号：CONTNO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("CONTNO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CONTNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("CONTNO") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(コンテナ番号)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(出荷伝票番号：SHUKADENNO)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SHUKADENNO")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKADENNO", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SHUKADENNO") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(出荷伝票番号)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(積順：TUMISEQ)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TUMISEQ")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TUMISEQ", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                'データ存在チェック
                If String.IsNullOrEmpty(T00004INProw("TUMISEQ")) Then
                    T00004INProw("TUMISEQ") = ""
                Else
                    T00004INProw("TUMISEQ") = WW_CS0024FCHECKVAL
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。(積順)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(積場：TUMIBA)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TUMIBA")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TUMIBA", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("TUMIBA") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(積場)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(ゲート：GATE)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("GATE")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "GATE", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("GATE") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(ゲート)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(枝番：SEQ)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("SEQ")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SEQ", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("SEQ") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(枝番)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(両目：RYOME)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("RYOME")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RYOME", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("RYOME") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(両目)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(配送単位：HTANI)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("HTANI")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HTANI", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("HTANI") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(配送単位)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(税区分：TAXKBN)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("TAXKBN")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TAXKBN", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("TAXKBN") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(税区分)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■明細項目(削除フラグ：DELFLG)
            '○必須・項目属性チェック
            WW_CS0024FCHECKVAL = T00004INProw("DELFLG")
            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_CS0024FCHECKVAL, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, S0013tbl)
            If isNormal(WW_CS0024FCHECKERR) Then
                T00004INProw("DELFLG") = WW_CS0024FCHECKVAL
            Else
                WW_CheckMES1 = "・エラーが存在します。(削除フラグ)"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.BOX_ERROR_EXIST, T00004INProw)
            End If

            '■■■ 関連チェック　■■■

            '■数量or台数入力チェック
            If Val(T00004INProw("SURYO")) = 0 AndAlso Val(T00004INProw("DAISU")) = 0 Then
                WW_CheckMES1 = "・更新できないレコード(数量・台数が未入力)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■出庫日・出荷日
            If T00004INProw("TUMIOKIKBN") = "1" Then
            Else
                '通常配送の場合
                If T00004INProw("SHUKODATE") <> "" AndAlso T00004INProw("SHUKADATE") <> "" AndAlso T00004INProw("SHUKODATE") <> T00004INProw("SHUKADATE") Then
                    WW_CheckMES1 = "・更新できないレコード(出庫日・出荷日不一致)です。"
                    WW_CheckMES2 = ""
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                End If
            End If

            '■出庫日・届日
            '大小比較チェック
            If T00004INProw("SHUKODATE") <> "" AndAlso T00004INProw("TODOKEDATE") <> "" AndAlso T00004INProw("SHUKODATE") > T00004INProw("TODOKEDATE") Then
                WW_CheckMES1 = "・更新できないレコード(届日＜出庫日)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■出庫日・帰庫日
            If T00004INProw("SHUKODATE") <> "" AndAlso T00004INProw("KIKODATE") <> "" AndAlso T00004INProw("SHUKODATE") > T00004INProw("KIKODATE") Then
                WW_CheckMES1 = "・更新できないレコード(出庫日 > 帰庫日)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            '■容器検査期限、車検期限チェック（八戸、大井川、水島のみ）
            Dim WW_HPRSINSNYMDF As String = ""
            Dim WW_HPRSINSNYMDB As String = ""
            Dim WW_HPRSINSNYMDB2 As String = ""
            Dim WW_LICNYMDF As String = ""
            Dim WW_LICNYMDB As String = ""
            Dim WW_LICNYMDB2 As String = ""
            Dim WW_LICNPLTNOF As String = ""
            Dim WW_LICNPLTNOB As String = ""
            Dim WW_LICNPLTNOB2 As String = ""
            If WW_SHUKODATEERR = "OFF" AndAlso T00004INProw("SHUKODATE") <> "" Then
                If IsInspectionOrg(work.WF_SEL_CAMPCODE.Text, T00004INProw("SHIPORG").ToString, O_RTNCODE) Then

                    If T00004INProw("OILTYPE") = "02" Then
                        For j As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
                            If WF_ListGSHABAN.Items(j).Value = T00004INProw("GSHABAN") Then
                                If WF_ListOILTYPE.Items(j).Value = T00004INProw("OILTYPE") Then
                                    WW_HPRSINSNYMDF = WF_ListHPRSINSNYMDF.Items(j).Value.Replace("-", "/")
                                    WW_HPRSINSNYMDB = WF_ListHPRSINSNYMDB.Items(j).Value.Replace("-", "/")
                                    WW_HPRSINSNYMDB2 = WF_ListHPRSINSNYMDB2.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDF = WF_ListLICNYMDF.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDB = WF_ListLICNYMDB.Items(j).Value.Replace("-", "/")
                                    WW_LICNYMDB2 = WF_ListLICNYMDB2.Items(j).Value.Replace("-", "/")
                                    WW_LICNPLTNOF = WF_ListLICNPLTNOF.Items(j).Value
                                    WW_LICNPLTNOB = WF_ListLICNPLTNOB.Items(j).Value
                                    WW_LICNPLTNOB2 = WF_ListLICNPLTNOB2.Items(j).Value
                                    Exit For
                                End If
                            End If
                        Next

                        '容器検査年月日チェック（２カ月前から警告、４日前はエラー）
                        '車検年月日チェック（１カ月前から警告、４日前はエラー）
                        '------ 車両前 -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00004INProw("SHARYOTYPEF")) Then
                            If IsDate(WW_LICNYMDF) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_LICNYMDF))
                                If CDate(WW_LICNYMDF) < T00004INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDF).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDF).AddMonths(-1) < T00004INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_LICNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00004INProw("SHARYOTYPEF")) Then
                            If IsDate(WW_HPRSINSNYMDF) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDF))
                                If CDate(WW_HPRSINSNYMDF) < T00004INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDF).AddMonths(-2) < T00004INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & " " & WW_HPRSINSNYMDF & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOF & " " & T00004INProw("SHARYOTYPEF") & T00004INProw("TSHABANF") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If

                        End If

                        '------ 車両後 -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00004INProw("SHARYOTYPEB")) Then
                            If IsDate(WW_LICNYMDB) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_LICNYMDB))
                                If CDate(WW_LICNYMDB) < T00004INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDB).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDB).AddMonths(-1) < T00004INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_LICNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00004INProw("SHARYOTYPEB")) Then
                            If IsDate(WW_HPRSINSNYMDB) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB))
                                If CDate(WW_HPRSINSNYMDB) < T00004INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB).AddMonths(-2) < T00004INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & " " & WW_HPRSINSNYMDB & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB & " " & T00004INProw("SHARYOTYPEB") & T00004INProw("TSHABANB") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If

                        End If

                        '------ 車両後２ -------------------------------------------------------------------------
                        '車検チェック
                        If SYARYOTYPE.INSPECTION_LIST.Contains(T00004INProw("SHARYOTYPEB2")) Then
                            If IsDate(WW_LICNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_LICNYMDB2))
                                If CDate(WW_LICNYMDB2) < T00004INProw("SHUKODATE") Then
                                    '車検切れ
                                    WW_CheckMES1 = "・更新できないレコード(車検切れ)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDB2).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_LICNYMDB2).AddMonths(-1) < T00004INProw("SHUKODATE") Then
                                    '1カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(車検" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_LICNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：車検有効年月日)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If
                        End If

                        '容器チェック
                        If SYARYOTYPE.TANK_LIST.Contains(T00004INProw("SHARYOTYPEB2")) Then
                            If IsDate(WW_HPRSINSNYMDB2) Then
                                Dim WW_days As String = DateDiff("d", T00004INProw("SHUKODATE"), CDate(WW_HPRSINSNYMDB2))
                                If CDate(WW_HPRSINSNYMDB2) < T00004INProw("SHUKODATE") Then
                                    '容器検査切れ
                                    WW_CheckMES1 = "・更新できないレコード(容器検査切れ)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddDays(-4) < T00004INProw("SHUKODATE") Then
                                    '４日前はエラー
                                    WW_CheckMES1 = "・更新できないレコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                                ElseIf CDate(WW_HPRSINSNYMDB2).AddMonths(-2) < T00004INProw("SHUKODATE") Then
                                    '2カ月前から警告
                                    WW_CheckMES1 = "・警告レコード(容器検査" & WW_days & "日前)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & " " & WW_HPRSINSNYMDB2 & ")"
                                    WW_CheckMES2 = ""
                                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.WORNING_RECORD_EXIST, T00004INProw)
                                End If
                            Else
                                'エラー
                                WW_CheckMES1 = "・更新できないレコード(車両マスタ不備：次回容器再検査年月日)です。(" & WW_LICNPLTNOB2 & " " & T00004INProw("SHARYOTYPEB2") & T00004INProw("TSHABANB2") & ")"
                                WW_CheckMES2 = ""
                                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                            End If
                        End If
                    End If
                End If
            End If

            '■■■ 集計制御項目チェック（集計KEY必須チェック） ■■■

            '荷主受注集計制御マスタ取得
            If (WW_LINEerr = C_MESSAGE_NO.NORMAL OrElse WW_LINEerr = C_MESSAGE_NO.WORNING_RECORD_EXIST) AndAlso
               WW_TORI_FLG = "OK" AndAlso
               WW_OILTYPE_FLG = "OK" AndAlso
               WW_ORG_FLG = "OK" Then

                GS0029T3CNTLget.CAMPCODE = T00004INProw("CAMPCODE")
                GS0029T3CNTLget.TORICODE = T00004INProw("TORICODE")
                GS0029T3CNTLget.OILTYPE = T00004INProw("OILTYPE")
                GS0029T3CNTLget.ORDERORG = T00004INProw("ORDERORG")
                GS0029T3CNTLget.KIJUNDATE = Date.Now
                GS0029T3CNTLget.GS0029T3CNTLget()

                If isNormal(GS0029T3CNTLget.ERR) Then
                    If GS0029T3CNTLget.CNTL02 = "1" AndAlso T00004INProw("SHUKODATE") = "" Then     '集計区分(出庫日)
                        WW_CheckMES1 = "・更新できないレコード(出庫日未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If GS0029T3CNTLget.CNTL03 = "1" AndAlso T00004INProw("SHUKABASHO") = "" Then    '集計区分(出荷場所)
                        WW_CheckMES1 = "・更新できないレコード(出荷場所未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If T00004INProw("SHIPORG") <> WF_DEFORG.Text And
                       T00004INProw("ORDERORG") <> T00004INProw("SHIPORG") Then
                        '他部署は、チェックしない
                    Else
                        If GS0029T3CNTLget.CNTL04 = "1" AndAlso T00004INProw("GSHABAN") = "" Then       '集計区分(業務車番)
                            WW_CheckMES1 = "・更新できないレコード(業務車番未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                        End If
                        If GS0029T3CNTLget.CNTL05 = "1" AndAlso T00004INProw("SHAFUKU") = "" Then       '集計区分(車腹(積載量))
                            WW_CheckMES1 = "・更新できないレコード(車腹未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                        End If
                        If GS0029T3CNTLget.CNTL06 = "1" AndAlso T00004INProw("STAFFCODE") = "" Then     '集計区分(乗務員コード)
                            WW_CheckMES1 = "・更新できないレコード(乗務員未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                        End If
                    End If
                    If GS0029T3CNTLget.CNTL07 = "1" AndAlso T00004INProw("TODOKECODE") = "" Then    '集計区分(届先コード)
                        WW_CheckMES1 = "・更新できないレコード(届先未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If GS0029T3CNTLget.CNTL08 = "1" AndAlso T00004INProw("PRODUCT1") = "" Then      '集計区分(品名１)
                        WW_CheckMES1 = "・更新できないレコード(品名１未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If GS0029T3CNTLget.CNTL09 = "1" AndAlso T00004INProw("PRODUCTCODE") = "" Then      '集計区分(品名２)
                        WW_CheckMES1 = "・更新できないレコード(品名２未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If GS0029T3CNTLget.CNTLVALUE = "1" AndAlso T00004INProw("DAISU") = "" Then     '集計区分(数量/台数)
                        WW_CheckMES1 = "・更新できないレコード(台数未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If
                    If GS0029T3CNTLget.CNTLVALUE = "2" AndAlso T00004INProw("SURYO") = "" Then     '集計区分(数量/台数)
                        WW_CheckMES1 = "・更新できないレコード(数量未入力)です。"
                        WW_CheckMES2 = ""
                        ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                    End If

                    '7/11 Miyake ADD
                    T00004INProw("URIKBN") = GS0029T3CNTLget.URIKBN
                    If T00004INProw("URIKBN") = "1" Then                                       '売上計上区分(1:出荷基準、2:着地基準)
                        If T00004INProw("SHUKADATE") = "" Then
                            WW_CheckMES1 = "・更新できないレコード(出荷日未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                        End If
                        T00004INProw("KIJUNDATE") = T00004INProw("SHUKADATE")
                    Else
                        If T00004INProw("TODOKEDATE") = "" Then
                            WW_CheckMES1 = "・更新できないレコード(届日未入力)です。"
                            WW_CheckMES2 = ""
                            ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                        End If
                        T00004INProw("KIJUNDATE") = T00004INProw("TODOKEDATE")
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(荷主受注集計制御マスタ登録なし)です。"
                    WW_CheckMES2 = ""
                    ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                End If

            End If

            '■■■ 権限チェック（更新権限） ■■■

            Dim WW_SHIPORG_ERR As String = ""
            Dim WW_ORDERORG_ERR As String = ""


            '出荷部署
            '受注部署
            If WW_SHIPORG_ERR = "ON" AndAlso WW_ORDERORG_ERR = "ON" Then
                WW_CheckMES1 = "・更新できないレコード(受注部署の権限無)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
                WW_CheckMES1 = "・更新できないレコード(出荷部署の権限無)です。"
                WW_CheckMES2 = ""
                ERRMESSAGE_write(WW_CheckMES1, WW_CheckMES2, WW_LINEerr, i, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, T00004INProw)
            End If

            If T00004INProw("DELFLG") = "" Then
                T00004INProw("DELFLG") = "0"
            End If


            '■■■ 各種設定＆名称設定 ■■■

            If (WW_LINEerr = C_MESSAGE_NO.NORMAL OrElse WW_LINEerr = C_MESSAGE_NO.WORNING_RECORD_EXIST) Then
                If T00004INProw("GSHABAN") <> "" AndAlso
                   T00004INProw("STAFFCODE") <> "" AndAlso
                   T00004INProw("PRODUCTCODE") <> "" Then
                    T00004INProw("STATUS") = T4STATUS.MANNING
                Else
                    T00004INProw("STATUS") = T4STATUS.ORDER
                End If
            Else
                T00004INProw("STATUS") = T4STATUS.INIT
            End If


            '油種
            CODENAME_get("OILTYPE", T00004INProw("OILTYPE"), WW_TEXT, WW_DUMMY)
            T00004INProw("OILTYPENAME") = WW_TEXT

            '会社名称
            CODENAME_get("CAMPCODE", T00004INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
            T00004INProw("CAMPCODENAME") = WW_TEXT

            '品名１名称
            CODENAME_get("PRODUCT1", T00004INProw("PRODUCT1"), WW_TEXT, WW_DUMMY)
            T00004INProw("PRODUCT1NAME") = WW_TEXT

            '品名名称
            CODENAME_get("PRODUCTCODE", T00004INProw("PRODUCTCODE"), WW_TEXT, WW_DUMMY)
            T00004INProw("PRODUCTNAME") = WW_TEXT
            '品名２名称
            T00004INProw("PRODUCT2NAME") = WW_TEXT

            '臭有無名称
            CODENAME_get("SMELLKBN", T00004INProw("SMELLKBN"), WW_TEXT, WW_DUMMY)
            T00004INProw("SMELLKBNNAME") = WW_TEXT

            '売上計上基準名称
            CODENAME_get("URIKBN", T00004INProw("URIKBN"), WW_TEXT, WW_DUMMY)
            T00004INProw("URIKBNNAME") = WW_TEXT

            '状態名称
            CODENAME_get("STATUS", T00004INProw("STATUS"), WW_TEXT, WW_DUMMY)
            T00004INProw("STATUSNAME") = WW_TEXT

            '積置区分名称
            CODENAME_get("TUMIOKIKBN", T00004INProw("TUMIOKIKBN"), WW_TEXT, WW_DUMMY)
            T00004INProw("TUMIOKIKBNNAME") = WW_TEXT

            '業務車番名称
            CODENAME_get("GSHABAN", T00004INProw("GSHABAN"), WW_TEXT, WW_DUMMY)
            T00004INProw("GSHABANLICNPLTNO") = WW_TEXT

            '配送単位名称
            CODENAME_get("HTANI", T00004INProw("HTANI"), WW_TEXT, WW_DUMMY)
            T00004INProw("HTANINAME") = WW_TEXT

            '税区分名称
            CODENAME_get("TAXKBN", T00004INProw("TAXKBN"), WW_TEXT, WW_DUMMY)
            T00004INProw("TAXKBNNAME") = WW_TEXT

            Select Case WW_LINEerr
                Case C_MESSAGE_NO.NORMAL,
                     C_MESSAGE_NO.WORNING_RECORD_EXIST
                    T00004INProw("SELECT") = 1
                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.WARNING
                Case C_MESSAGE_NO.BOX_ERROR_EXIST
                    T00004INProw("SELECT") = 1
                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    T00004INProw("SELECT") = 0
                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next


        If WW_ERRLIST.Count > 0 Then
            If WW_ERRLIST.IndexOf(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR) >= 0 Then
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ElseIf WW_ERRLIST.IndexOf(C_MESSAGE_NO.BOX_ERROR_EXIST) >= 0 Then
                O_RTNCODE = C_MESSAGE_NO.BOX_ERROR_EXIST
            Else
                O_RTNCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST
            End If
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMESSAGE_write(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef i As Integer, ByVal I_ERRCD As String, ByVal T00004INProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = String.Empty
        WW_ERR_MES = I_MESSAGE1
        If Not String.IsNullOrEmpty(I_MESSAGE2) Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　= @L" & i.ToString("0000") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 明細番号= @D" & i.ToString("000") & "D@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　=" & T00004INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　=" & T00004INProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷場所=" & T00004INProw("SHUKABASHO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　=" & T00004INProw("SHUKODATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届日　　=" & T00004INProw("TODOKEDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷日　=" & T00004INProw("SHUKADATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車番　　=" & T00004INProw("GSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 乗務員　=" & T00004INProw("STAFFCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名  　=" & T00004INProw("PRODUCTCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾘｯﾌﾟ 　=" & T00004INProw("TRIPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ﾄﾞﾛｯﾌﾟ　=" & T00004INProw("DROPNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　=" & T00004INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集（JSRフォーマット）
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub ERRMESSAGE_write_NJS(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByRef WW_LINEerr As String, ByRef i As Integer, ByVal I_ERRCD As String, ByVal JSRINProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = String.Empty
        WW_ERR_MES = I_MESSAGE1
        If Not String.IsNullOrEmpty(I_MESSAGE2) Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 項番　　     = @L" & i.ToString("0000") & "L@ , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 契約番号　   =" & JSRINProw("CONTRACTNO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出庫日　　   =" & JSRINProw("SHUKODATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 納入日       =" & JSRINProw("TODOKEDATE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先コード　 =" & JSRINProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先略称　　 =" & JSRINProw("TODOKECODENAME") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 倉庫コード　 =" & JSRINProw("SHUKABASHO") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 倉庫略称　　 =" & JSRINProw("SHUKABASHONAME") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名コード　 =" & JSRINProw("PRODUCTCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車輌コード　 =" & JSRINProw("SHARYOCD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運転手コード1=" & JSRINProw("STAFFCODE1") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 運転手コード2=" & JSRINProw("STAFFCODE2") & " , "
        rightview.AddErrorReport(WW_ERR_MES)

        WW_ERRLIST.Add(I_ERRCD)
        If WW_LINEerr <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            WW_LINEerr = I_ERRCD
        End If

    End Sub

    ''' <summary>
    ''' 同一オーダー判定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function CompareOrder(ByRef src As DataRow, ByRef dst As DataRow) As Boolean

        '取引先、油種、基準日（出荷日or届日）、受注部署、出荷部署、出庫日、業務車番、両目、トリップ、ドロップが同一
        If src("TORICODE") = dst("TORICODE") AndAlso
           src("OILTYPE") = dst("OILTYPE") AndAlso
           src("KIJUNDATE") = dst("KIJUNDATE") AndAlso
           src("ORDERORG") = dst("ORDERORG") AndAlso
           src("SHIPORG") = dst("SHIPORG") AndAlso
           src("SHUKODATE") = dst("SHUKODATE") AndAlso
           src("GSHABAN") = dst("GSHABAN") AndAlso
           src("RYOME") = dst("RYOME") AndAlso
           src("TRIPNO") = dst("TRIPNO") AndAlso
           src("DROPNO") = dst("DROPNO") Then

            Return True
        Else
            Return False
        End If

    End Function

#End Region

#Region "左BOX関連"

    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()

        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then Exit Sub
        If Not Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value) Then Exit Sub

        Dim WW_FIELD As String = ""
        If WF_FIELD_REP.Value = "" Then
            WW_FIELD = WF_FIELD.Value
        Else
            WW_FIELD = WF_FIELD_REP.Value
        End If

        WF_LeftMView.ActiveViewIndex = -1
        If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
            '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
            Dim obj = work.getControl(WW_FIELD)
            Dim txtBox = DirectCast(obj, TextBox)
            leftview.WF_Calendar.Text = txtBox.Text
            leftview.ActiveCalendar()

        ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST Then
            '○画面表示データ復元
            Master.RecoverTable(T00004tbl)
            Select Case WW_FIELD
                Case "WF_GSHABAN"
                    WF_GSHABAN_Rep.Visible = True
                    DataBindGSHABAN()
                    WF_LeftMView.ActiveViewIndex = 0
                Case "WF_CONTCHASSIS"
                    WF_CONTCHASSIS_Rep.Visible = True
                    DataBindCONTCHASSIS()
                    WF_LeftMView.ActiveViewIndex = 1
            End Select
            WF_LeftMView.Visible = True

        Else
            Dim prmData As Hashtable = work.createFIXParam(work.WF_SEL_CAMPCODE.Text)

            'フィールドによってパラメーターを変える
            Select Case WW_FIELD
                Case "WF_CAMPCODE"                              '会社コード
                Case "WF_SELTORICODE"
                    prmData = work.createTORIParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_ORDERORG.Text)
                Case "WF_TORICODE"
                    prmData = work.createTORIParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text)
                Case "WF_OILTYPE"                               '油種
                Case "WF_ORDERORG",
                    "WF_SELORDERORG"                            '受注部署
                    prmData = work.createORGParam(work.WF_SEL_CAMPCODE.Text, True)
                Case "WF_SHIPORG"                               '出荷部署
                    prmData = work.createORGParam(work.WF_SEL_CAMPCODE.Text, False)
                Case "WF_STORICODE"                             '販売店
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STORICODE")
                Case "WF_URIKBN"                                '売上計上基準
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN")
                Case "WF_TUMIOKIKBN"                             '積置区分
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN")
                Case "WF_STAFFCODE",
                    "WF_SUBSTAFFCODE"                       '乗務員・副乗務員
                    prmData = work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text)
                Case "STATUS"                             '状態
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STATUS")
                Case "PRODUCT1"                             '品名１
                    prmData = work.createGoods1Param(work.WF_SEL_CAMPCODE.Text)
                Case "PRODUCTCODE"                             '品名コード
                    prmData = work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, True)
                Case "SMELLKBN"                             '臭有無
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "SMELLKBN")
                Case "TODOKECODE"                            '届先
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, WF_TORICODE.Text, "1")
                Case "SHUKABASHO"                            '出荷場所
                    prmData = work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, WF_SHIPORG.Text, WF_TORICODE.Text, "2")
                Case "HTANI"                             '配送単位
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "HTANI")
                Case "TAXKBN"                             '税区分
                    prmData = work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN")
                Case "DELFLG"                               '削除フラグ
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
            End Select
            leftview.SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
            leftview.ActiveListBox()
        End If
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    '''  LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""
        Dim WW_PARAM1 As String = ""
        Dim WW_PARAM2 As String = ""
        Dim WW_PARAM3 As String = ""
        Dim WW_PARAM4 As String = ""
        Dim WW_PARAM5 As String = ""
        Dim WW_PARAM6 As String = ""
        Dim WW_PARAM7 As String = ""
        Dim WW_PARAM8 As String = ""
        Dim WW_PARAM9 As String = ""
        Dim WW_PARAM10 As String = ""
        Dim WW_PARAM11 As String = ""
        Dim WW_PARAM12 As String = ""
        Dim WW_PARAM13 As String = ""
        Dim WW_PARAM14 As String = ""
        Dim WW_PARAM15 As String = ""
        Dim WW_PARAM16 As String = ""
        Dim WW_PARAM17 As String = ""
        Dim WW_PARAM18 As String = ""
        Dim WW_PARAM19 As String = ""
        Dim WW_PARAM20 As String = ""

        Dim WW_ACTIVE_VALUE As String()
        Select Case WF_LeftMView.ActiveViewIndex
            Case 0
                If Not String.IsNullOrEmpty(WF_SelectedIndex.Value) Then
                    WW_SelectValue = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell1"), System.Web.UI.WebControls.TableCell).Text
                    WW_SelectTEXT = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell7"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM1 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell8"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM2 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell9"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM3 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell10"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM4 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell11"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM5 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell12"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM6 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell13"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM7 = CType(WF_GSHABAN_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_GSHABAN_ItemCell14"), System.Web.UI.WebControls.TableCell).Text
                    WW_PARAM8 = WF_ListSHARYOINFO1.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM9 = WF_ListSHARYOINFO2.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM10 = WF_ListSHARYOINFO3.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM11 = WF_ListSHARYOINFO4.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM12 = WF_ListSHARYOINFO5.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM13 = WF_ListSHARYOINFO6.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM14 = WF_ListSHAFUKU.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM15 = WF_ListTSHABANF.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM16 = WF_ListTSHABANB.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM17 = WF_ListTSHABANB2.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM18 = WF_ListLICNPLTNOF.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM19 = WF_ListLICNPLTNOB.Items(WF_SelectedIndex.Value).Value
                    WW_PARAM20 = WF_ListLICNPLTNOB2.Items(WF_SelectedIndex.Value).Value
                End If
            Case 1
                If Not String.IsNullOrEmpty(WF_SelectedIndex.Value) Then
                    WW_SelectValue = CType(WF_CONTCHASSIS_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_CONTCHASSIS_ItemCell1"), System.Web.UI.WebControls.TableCell).Text
                    WW_SelectTEXT = CType(WF_CONTCHASSIS_Rep.Items(CInt(WF_SelectedIndex.Value)).FindControl("WF_CONTCHASSIS_ItemCell7"), System.Web.UI.WebControls.TableCell).Text
                End If
            Case Else
                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
                    WW_ACTIVE_VALUE = leftview.GetActiveValue
                    WW_SelectValue = WW_ACTIVE_VALUE(0)
                    WW_SelectTEXT = WW_ACTIVE_VALUE(1)
                End If

        End Select

        If WF_FIELD_REP.Value = "" Then
            '変更
            WF_REP_Change.Value = "1"

            Select Case WF_FIELD.Value

                Case "WF_SHUKODATE",
                    "WF_SHUKADATE",
                    "WF_TODOKEDATE",
                    "WF_KIKODATE"
                    'カレンダー関係
                    ' 出庫日
                    ' 出荷日
                    ' 届日
                    ' 帰庫日
                    Dim obj = work.getControl(WF_FIELD.Value)
                    Dim txtBox As TextBox = DirectCast(obj, TextBox)
                    txtBox.Text = WW_SelectValue
                    txtBox.Focus()

                Case "WF_SELTORICODE",
                    "WF_SELORDERORG",
                    "WF_TORICODE",
                    "WF_OILTYPE",
                    "WF_STORICODE",
                    "WF_ORDERORG",
                    "WF_SHIPORG",
                    "WF_URIKBN",
                    "WF_CONTCHASSIS",
                    "WF_TUMIOKIKBN",
                    "WF_STAFFCODE",
                    "WF_SUBSTAFFCODE"

                    '取引先（絞込）
                    '受注受付部署
                    '取引先
                    '油種
                    '販売店
                    '受注受付部署
                    '売上計上基準
                    'コンテナシャーシ
                    '積置区分
                    '乗務員
                    '副乗務員
                    Dim obj = work.getControl(WF_FIELD.Value)
                    Dim objText = work.getControl(WF_FIELD.Value & "_TEXT")
                    Dim txtBox As TextBox = DirectCast(obj, TextBox)
                    Dim lblText As Label = DirectCast(objText, Label)

                    lblText.Text = WW_SelectTEXT
                    txtBox.Text = WW_SelectValue
                    txtBox.Focus()

                    '請求先情報を設定
                    If WF_FIELD.Value = "WF_TORICODE" Then
                        GetSTori(WF_TORICODE.Text, WF_STORICODE.Text, WF_STORICODE_TEXT.Text)
                    End If
                    If WF_FIELD.Value = "WF_STAFFCODE" Then
                        '乗務員の付加情報を設定
                        Dim datStaff As STAFF = GetStaff(WF_SHIPORG.Text, WW_SelectValue)
                        If Not IsNothing(datStaff) Then
                            Repeater_Value("STAFFNOTES1", datStaff.NOTES1, "D")
                            Repeater_Value("STAFFNOTES2", datStaff.NOTES2, "D")
                            Repeater_Value("STAFFNOTES3", datStaff.NOTES3, "D")
                            Repeater_Value("STAFFNOTES4", datStaff.NOTES4, "D")
                            Repeater_Value("STAFFNOTES5", datStaff.NOTES5, "D")
                        End If
                    End If

                Case "WF_GSHABAN"
                    '業務車番
                    WF_GSHABAN.Text = WW_SelectValue
                    Repeater_Value("SHARYOINFO1", WW_PARAM8, "H")
                    Repeater_Value("SHARYOINFO2", WW_PARAM9, "H")
                    Repeater_Value("SHARYOINFO3", WW_PARAM10, "H")
                    Repeater_Value("SHARYOINFO4", WW_PARAM11, "H")
                    Repeater_Value("SHARYOINFO5", WW_PARAM12, "H")
                    Repeater_Value("SHARYOINFO6", WW_PARAM13, "H")
                    WF_SHAFUKU.Text = WW_PARAM14
                    WF_TSHABANF.Text = WW_PARAM15
                    WF_TSHABANB.Text = WW_PARAM16
                    WF_TSHABANB2.Text = WW_PARAM17
                    WF_TSHABANF_TEXT.Text = WW_PARAM18
                    WF_TSHABANB_TEXT.Text = WW_PARAM19
                    WF_TSHABANB2_TEXT.Text = WW_PARAM20

                    WF_GSHABAN.Focus()

            End Select

        Else
            '変更
            WF_REP_Change.Value = "2"
            Dim exitFlg As Boolean = False
            '■■■ ディテール変数設定 ■■■
            For Each repItem In WF_DViewRep1.Items
                '[インデックス]が合致する場合
                If CType(repItem.FindControl("WF_Rep1_LINEPOSITION"), System.Web.UI.WebControls.TextBox).Text = WF_REP_POSITION.Value Then
                    For i As Integer = 1 To WF_REP_COLSCNT.Value
                        If CType(repItem.FindControl("WF_Rep1_FIELD_" & i), System.Web.UI.WebControls.Label).Text = WF_FIELD_REP.Value Then
                            CType(repItem.FindControl("WF_Rep1_VALUE_" & i), System.Web.UI.WebControls.TextBox).Text = WW_SelectValue
                            CType(repItem.FindControl("WF_Rep1_VALUE_TEXT_" & i), System.Web.UI.WebControls.Label).Text = WW_SelectTEXT
                            CType(repItem.FindControl("WF_Rep1_VALUE_" & i), System.Web.UI.WebControls.TextBox).Focus()
                            exitFlg = True
                            Exit For
                        End If
                    Next
                    '項目名が合致する場合
                    If exitFlg = True Then Exit For
                End If
            Next

            '届先コードの付加情報を設定
            If WF_FIELD_REP.Value = "TODOKECODE" Then
                Dim datTodoke As TODOKESAKI = GetTodoke(WF_SHIPORG.Text, WW_SelectValue)
                If Not IsNothing(datTodoke) Then
                    Repeater_Value("ADDR", datTodoke.ADDR, "D")
                    Repeater_Value("ARRIVTIME", datTodoke.ARRIVTIME, "D")
                    Repeater_Value("DISTANCE", datTodoke.DISTANCE, "D")
                    Repeater_Value("NOTES1", datTodoke.NOTES1, "D")
                    Repeater_Value("NOTES2", datTodoke.NOTES2, "D")
                    Repeater_Value("NOTES3", datTodoke.NOTES3, "D")
                    Repeater_Value("NOTES4", datTodoke.NOTES4, "D")
                    Repeater_Value("NOTES5", datTodoke.NOTES5, "D")
                End If
            End If

            '品名の付加情報を設定
            If WF_FIELD_REP.Value = "PRODUCTCODE" Then
                Dim datProduct As PRODUCT = GetProduct(WF_SHIPORG.Text, WW_SelectValue)
                If Not IsNothing(datProduct) Then
                    Repeater_Value("HTANI", datProduct.HTANI, "D")
                End If
            End If

        End If

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_GSHABAN_Rep.Dispose()
        WF_GSHABAN_Rep = Nothing
        WF_CONTCHASSIS_Rep.Dispose()
        WF_CONTCHASSIS_Rep = Nothing

        WF_LeftMView.Visible = False
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_FIELD_REP.Value = ""
        WF_FIELD.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal args As Hashtable = Nothing)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        '入力値が空は終了
        If String.IsNullOrEmpty(I_VALUE) Then Exit Sub
        With leftview
            Select Case I_FIELD
                Case "CAMPCODE"
                    '会社名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "TORICODE"
                    '取引先名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.createTORIParam(work.WF_SEL_CAMPCODE.Text))
                Case "OILTYPE"
                    '油種名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "STORICODE"
                    '販売店名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STORICODE"))
                Case "ORDERORG",
                    "TERMORG"
                    '受注受付部署名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(work.WF_SEL_CAMPCODE.Text, True))
                Case "URIKBN"
                    '売上計上基準名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_URIKBN, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))
                Case "STATUS"
                    '状態名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "STATUS"))
                Case "TUMIOKIKBN"
                    '積置区分名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TUMIOKIKBN"))
                Case "PRODUCT1"
                    '品名１名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoods1Param(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCT2"
                    '品名２名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoods2Param(work.WF_SEL_CAMPCODE.Text))
                Case "PRODUCTCODE"
                    '品名名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.createGoodsParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "SMELLKBN"
                    '臭有無名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "SMELLKBN"))
                Case "SHUKABASHO"
                    '出荷場所名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "2"))
                Case "SHIPORG"
                    '出荷部署名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(work.WF_SEL_CAMPCODE.Text, False))
                Case "GSHABAN"
                    '業務車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "TSHABANF"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "TSHABANB"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "TSHABANB2"
                    '統一車番名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, work.createCarCodeParam(work.WF_SEL_CAMPCODE.Text))
                Case "CONTCHASSIS"
                    'コンテナシャーシ名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_WORKLORRY, I_VALUE, O_TEXT, O_RTN, work.createWorkLorryParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "STAFFCODE", "SUBSTAFFCODE"
                    '乗務員コード/副乗務員コード名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text))
                Case "TODOKECODE"
                    '届先コード名称
                    If IsNothing(args) Then
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.createDistinationParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_SHIPORG.Text, "", "1"))
                    Else
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, args)
                    End If
                Case "DELFLG"
                    '削除名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                Case "HTANI"
                    '単位名称
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "HTANI"))
                Case "TAXKBN"
                    '税区分
                    .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(work.WF_SEL_CAMPCODE.Text, "TAXKBN"))
            End Select
        End With

    End Sub

    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()
        WF_FIELD_REP.Value = ""
        WF_FIELD.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        'メモリ開放
        WF_GSHABAN_Rep.Visible = False
        WF_GSHABAN_Rep.Dispose()
        WF_GSHABAN_Rep = Nothing
        WF_CONTCHASSIS_Rep.Visible = False
        WF_CONTCHASSIS_Rep.Dispose()
        WF_CONTCHASSIS_Rep = Nothing

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' LeftBOX選択値に伴う項目内容変更
    ''' </summary>
    Protected Sub Repeater_Value(ByVal I_FIELD As String, ByVal I_Value As String, I_HDKBN As String)

        Dim WW_ROWF As Integer = 0
        Dim WW_ROWT As Integer = 0
        'リピータの何明細目かを表す
        Dim WW_CNT As Integer = 0
        Dim WW_CNT2 As Integer = 0

        Dim WW_TEXT As String = ""

        If I_HDKBN = "H" Then
        Else
            '選択された行数　－　１明細の行数を繰り返し、何明細目がを判定
            WW_CNT2 = Int32.TryParse(WF_REP_POSITION.Value, WW_CNT2)
            Do While WW_CNT2 > WF_REP_ROWSCNT.Value
                WW_CNT2 = WW_CNT2 - WF_REP_ROWSCNT.Value
                WW_CNT += 1
            Loop
        End If

        WW_ROWF = WW_CNT * WF_REP_ROWSCNT.Value + 1
        WW_ROWT = WW_ROWF + WF_REP_ROWSCNT.Value - 1


        If I_FIELD = "HTANI" Then
            CODENAME_get("HTANI", I_Value, WW_TEXT, WW_DUMMY)
        End If

        For i As Integer = WW_ROWF To WW_ROWT - 1
            For col As Integer = 1 To WF_REP_COLSCNT.Value
                If CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_" & col), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                    CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_" & col), System.Web.UI.WebControls.TextBox).Text = I_Value
                    CType(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_" & col), System.Web.UI.WebControls.Label).Text = WW_TEXT
                    Exit For
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' LeftBox指定されたタブから指定項目の値を取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_ItemFIND(ByVal I_FIELD As String, ByRef O_VALUE As String)
        Dim exitFlg As Boolean = False

        For Each repItems As RepeaterItem In WF_DViewRep1.Items
            Try
                '[インデックス]が合致する場合
                If CType(repItems.FindControl("WF_Rep1_LINEPOSITION"), System.Web.UI.WebControls.TextBox).Text = WF_REP_POSITION.Value Then

                    For i As Integer = 1 To WF_REP_COLSCNT.Value
                        '[フィールド名]が合致する項目の値を取得
                        If CType(repItems.FindControl("WF_Rep1_FIELD_" & i), System.Web.UI.WebControls.Label).Text = I_FIELD Then
                            O_VALUE = CType(repItems.FindControl("WF_Rep1_VALUE_" & i), System.Web.UI.WebControls.TextBox).Text
                            exitFlg = True
                            Exit For
                        End If
                    Next
                    If exitFlg = True Then Exit For
                End If
            Catch ex As Exception
            End Try
        Next
    End Sub


    ''' <summary>
    ''' LeftBox業務車番データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitGSHABAN()

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

        WF_ListGSHABAN.Items.Clear()
        WF_ListKSHABAN.Items.Clear()
        WF_ListTSHABANF.Items.Clear()
        WF_ListTSHABANB.Items.Clear()
        WF_ListTSHABANB2.Items.Clear()
        WF_ListLICNPLTNOF.Items.Clear()
        WF_ListLICNPLTNOB.Items.Clear()
        WF_ListLICNPLTNOB2.Items.Clear()
        WF_ListSHARYOINFO1.Items.Clear()
        WF_ListSHARYOINFO2.Items.Clear()
        WF_ListSHARYOINFO3.Items.Clear()
        WF_ListSHARYOINFO4.Items.Clear()
        WF_ListSHARYOINFO5.Items.Clear()
        WF_ListSHARYOINFO6.Items.Clear()
        WF_ListOILTYPE.Items.Clear()
        WF_ListOILTYPENAME.Items.Clear()
        WF_ListSHAFUKU.Items.Clear()
        WF_ListOWNCODE.Items.Clear()
        WF_ListOWNCODENAME.Items.Clear()
        WF_ListSHARYOSTATUS.Items.Clear()
        WF_ListSHARYOSTATUSNAME.Items.Clear()
        WF_ListHPRSINSNYMDF.Items.Clear()
        WF_ListHPRSINSNYMDB.Items.Clear()
        WF_ListHPRSINSNYMDB2.Items.Clear()
        WF_ListLICNYMDF.Items.Clear()
        WF_ListLICNYMDB.Items.Clear()
        WF_ListLICNYMDB2.Items.Clear()

        '○　業務車番Table設定
        Try
            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     " SELECT isnull(rtrim(A.GSHABAN),'') 		as GSHABAN ,   		    " _
                   & "        isnull(rtrim(A.KOEISHABAN),'') 	as KOEISHABAN ,		    " _
                   & "        isnull(rtrim(A.SHARYOTYPEF),'') +                         " _
                   & "        isnull(rtrim(A.TSHABANF),'')      as TSHABANF ,           " _
                   & "        isnull(rtrim(A.SHARYOTYPEB),'') +                         " _
                   & "        isnull(rtrim(A.TSHABANB),'')      as TSHABANB ,           " _
                   & "        isnull(rtrim(A.SHARYOTYPEB2),'') +                        " _
                   & "        isnull(rtrim(A.TSHABANB2),'')     as TSHABANB2 ,          " _
                   & "        isnull(rtrim(A.TSHABANFNAMES),'') as TSHABANFNAMES ,      " _
                   & "        isnull(rtrim(A.TSHABANBNAMES),'') as TSHABANBNAMES ,      " _
                   & "        isnull(rtrim(A.TSHABANB2NAMES),'') as TSHABANB2NAMES ,    " _
                   & "        isnull(rtrim(A.SHARYOINFO1),'') 	as SHARYOINFO1 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO2),'') 	as SHARYOINFO2 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO3),'') 	as SHARYOINFO3 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO4),'') 	as SHARYOINFO4 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO5),'') 	as SHARYOINFO5 ,        " _
                   & "        isnull(rtrim(A.SHARYOINFO6),'') 	as SHARYOINFO6 ,        " _
                   & "        isnull(rtrim(B.MANGOILTYPE),'') 	as OILTYPE ,            " _
                   & "        isnull(rtrim(C.VALUE1),'') 	    as OILTYPENAME ,        " _
                   & "        isnull(rtrim(B.MANGSHAFUKU),'')	as SHAFUKU ,   	        " _
                   & "        isnull(rtrim(B.MANGOWNCODE),'') 	as OWNCODE ,            " _
                   & "        isnull(rtrim(D.NAMES),'') 	    as OWNCODENAME ,        " _
                   & "        isnull(rtrim(E.KEYCODE),'')       as SHARYOSTATUS ,       " _
                   & "        isnull(rtrim(E.VALUE1),'')        as SHARYOSTATUSNAME ,   " _
                   & "        isnull(rtrim(F.HPRSINSNYMD),'')   as HPRSINSNYMDF,        " _
                   & "        isnull(rtrim(F.LICNYMD),'')       as LICNYMDF,            " _
                   & "        isnull(rtrim(G.HPRSINSNYMD),'')   as HPRSINSNYMDB,        " _
                   & "        isnull(rtrim(G.LICNYMD),'')       as LICNYMDB,            " _
                   & "        isnull(rtrim(H.HPRSINSNYMD),'')   as HPRSINSNYMDB2,       " _
                   & "        isnull(rtrim(H.LICNYMD),'')       as LICNYMDB2            " _
                   & "   FROM MA006_SHABANORG   as A                        " _
                   & "   LEFT JOIN MA002_SHARYOA B 						    " _
                   & "     ON B.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and B.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
                   & "    and B.TSHABAN     = A.TSHABANB 		            " _
                   & "    and B.STYMD      <= @P1                           " _
                   & "    and B.ENDYMD     >= @P1                           " _
                   & "    and B.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC001_FIXVALUE C 					    " _
                   & "     ON C.CAMPCODE   	= B.CAMPCODE       	            " _
                   & "    and C.CLASS       = 'MANGOILTYPE' 			    " _
                   & "    and C.KEYCODE     = B.MANGOILTYPE 			    " _
                   & "    and C.STYMD      <= @P1                           " _
                   & "    and C.ENDYMD     >= @P1                           " _
                   & "    and C.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC002_TORIHIKISAKI D 				    " _
                   & "     ON D.CAMPCODE   	 = B.CAMPCODE    			    " _
                   & "    and D.TORICODE   	 = B.MANGOWNCODE 			    " _
                   & "    and D.STYMD      <= @P1                           " _
                   & "    and D.ENDYMD     >= @P1                           " _
                   & "    and D.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MC001_FIXVALUE E 					    " _
                   & "     ON E.CAMPCODE   	= B.CAMPCODE        		    " _
                   & "    and E.CLASS       = 'SHARYOSTATUS' 			    " _
                   & "    and E.KEYCODE     = B.SHARYOSTATUS 			    " _
                   & "    and E.STYMD      <= @P1                           " _
                   & "    and E.ENDYMD     >= @P1                           " _
                   & "    and E.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC F 						    " _
                   & "     ON F.CAMPCODE  　= A.CAMPCODE 				    " _
                   & "    and F.SHARYOTYPE  = A.SHARYOTYPEF 		        " _
                   & "    and F.TSHABAN     = A.TSHABANF 			        " _
                   & "    and F.STYMD      <= @P1                           " _
                   & "    and F.ENDYMD     >= @P1                           " _
                   & "    and F.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC G 						    " _
                   & "     ON G.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and G.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
                   & "    and G.TSHABAN     = A.TSHABANB 	                " _
                   & "    and G.STYMD      <= @P1                           " _
                   & "    and G.ENDYMD     >= @P1                           " _
                   & "    and G.DELFLG     <> '1' 						    " _
                   & "   LEFT JOIN MA004_SHARYOC H 						    " _
                   & "     ON H.CAMPCODE   	= A.CAMPCODE 				    " _
                   & "    and H.SHARYOTYPE  = A.SHARYOTYPEB2 		        " _
                   & "    and H.TSHABAN     = A.TSHABANB2 	                " _
                   & "    and H.STYMD      <= @P1                           " _
                   & "    and H.ENDYMD     >= @P1                           " _
                   & "    and H.DELFLG     <> '1' 						    " _
                   & "  Where A.CAMPCODE  = @P2                             " _
                   & "    and A.MANGUORG  = @P3                             " _
                   & "    and A.DELFLG   <> '1'                             " _
                   & "  ORDER BY A.SEQ ,A.GSHABAN                           "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = WW_SHUKODATE
                PARA2.Value = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    PARA3.Value = WF_DEFORG.Text
                Else
                    PARA3.Value = work.WF_SEL_SHIPORG.Text
                End If

                '○SQL実行
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力設定
                While SQLdr.Read
                    WF_ListGSHABAN.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                    WF_ListKSHABAN.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("KOEISHABAN")))
                    WF_ListSHARYOINFO1.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO1")))
                    WF_ListSHARYOINFO2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO2")))
                    WF_ListSHARYOINFO3.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO3")))
                    WF_ListSHARYOINFO4.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO4")))
                    WF_ListSHARYOINFO5.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO5")))
                    WF_ListSHARYOINFO6.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOINFO6")))
                    WF_ListOILTYPE.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OILTYPE")))
                    WF_ListOILTYPENAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OILTYPENAME")))
                    WF_ListSHAFUKU.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHAFUKU")))
                    WF_ListOWNCODE.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OWNCODE")))
                    WF_ListOWNCODENAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("OWNCODENAME")))
                    WF_ListSHARYOSTATUS.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOSTATUS")))
                    WF_ListSHARYOSTATUSNAME.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("SHARYOSTATUSNAME")))
                    WF_ListLICNPLTNOF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANFNAMES")))
                    WF_ListLICNPLTNOB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANBNAMES")))
                    WF_ListLICNPLTNOB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB2NAMES")))
                    WF_ListTSHABANF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANF")))
                    WF_ListTSHABANB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB")))
                    WF_ListTSHABANB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("TSHABANB2")))
                    WF_ListHPRSINSNYMDF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDF")))
                    WF_ListHPRSINSNYMDB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDB")))
                    WF_ListHPRSINSNYMDB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("HPRSINSNYMDB2")))
                    WF_ListLICNYMDF.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDF")))
                    WF_ListLICNYMDB.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDB")))
                    WF_ListLICNYMDB2.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("LICNYMDB2")))

                End While

                'Close()
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "GSHABAN SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:GSHABAN Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' LeftBoxコンテナシャーシデータ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitCONTCHASSIS()

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

        WF_ListGSHABAN_CONT.Items.Clear()
        WF_ListOILTYPE_CONT.Items.Clear()
        WF_ListOILTYPENAME_CONT.Items.Clear()
        WF_ListSHAFUKU_CONT.Items.Clear()
        WF_ListOWNCODE_CONT.Items.Clear()
        WF_ListOWNCODENAME_CONT.Items.Clear()
        WF_ListSHARYOSTATUS_CONT.Items.Clear()
        WF_ListSHARYOSTATUSNAME_CONT.Items.Clear()
        WF_ListLICNPLTNOF_CONT.Items.Clear()
        WF_ListLICNPLTNOB_CONT.Items.Clear()

        '○　車番Table設定（コンテナ固定）
        Try
            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                         " SELECT isnull(rtrim(A.GSHABAN),'') 		as GSHABAN ,   		    " _
                       & "        isnull(rtrim(B.MANGOILTYPE),'') 	as OILTYPE ,            " _
                       & "        isnull(rtrim(C.VALUE1),'') 	    as OILTYPENAME ,        " _
                       & "        isnull(rtrim(B.MANGSHAFUKU),'')	as SHAFUKU ,   	        " _
                       & "        isnull(rtrim(B.MANGOWNCODE),'') 	as OWNCODE ,            " _
                       & "        isnull(rtrim(D.NAMES),'') 	    as OWNCODENAME ,        " _
                       & "        isnull(rtrim(E.KEYCODE),'')      as SHARYOSTATUS ,       " _
                       & "        isnull(rtrim(E.VALUE1),'')       as SHARYOSTATUSNAME ,   " _
                       & "        isnull(rtrim(F.LICNPLTNO1),'') +                         " _
                       & "        isnull(rtrim(F.LICNPLTNO2),'')   as LICNPLTNOF ,         " _
                       & "        isnull(rtrim(G.LICNPLTNO1),'') +                         " _
                       & "        isnull(rtrim(G.LICNPLTNO2),'')   as LICNPLTNOB           " _
                       & "   FROM MA006_SHABANORG   as A                        " _
                       & "  INNER JOIN MA002_SHARYOA B 						    " _
                       & "     ON B.CAMPCODE   	 = A.CAMPCODE 				    " _
                       & "    and B.SHARYOTYPE   = A.SHARYOTYPEB 		        " _
                       & "    and B.TSHABAN      = A.TSHABANB 		            " _
                       & "    and B.STYMD       <= @P1                          " _
                       & "    and B.ENDYMD      >= @P1                          " _
                       & "    and B.DELFLG      <> '1' 						    " _
                       & "   LEFT JOIN MC001_FIXVALUE C 					    " _
                       & "     ON C.CAMPCODE     = B.CAMPCODE  			        " _
                       & "    and C.CLASS        = 'MANGOILTYPE' 			    " _
                       & "    and C.KEYCODE      = B.MANGOILTYPE 			    " _
                       & "    and C.STYMD       <= @P1                          " _
                       & "    and C.ENDYMD      >= @P1                          " _
                       & "    and C.DELFLG      <> '1' 						    " _
                       & "   LEFT JOIN MC002_TORIHIKISAKI D 				    " _
                       & "     ON D.CAMPCODE   	 = B.CAMPCODE   			    " _
                       & "    and D.TORICODE   	 = B.MANGOWNCODE 			    " _
                       & "    and D.STYMD       <= @P1                          " _
                       & "    and D.ENDYMD      >= @P1                          " _
                       & "    and D.DELFLG      <> '1' 						    " _
                       & "   LEFT JOIN MC001_FIXVALUE E 					    " _
                       & "     ON E.CAMPCODE   	 = B.CAMPCODE        		    " _
                       & "    and E.CLASS        = 'SHARYOSTATUS' 			    " _
                       & "    and E.KEYCODE      = B.SHARYOSTATUS 			    " _
                       & "    and E.STYMD       <= @P1                          " _
                       & "    and E.ENDYMD      >= @P1                          " _
                       & "    and E.DELFLG      <> '1' 						    " _
                       & "   LEFT JOIN MA004_SHARYOC F 						    " _
                       & "     ON F.CAMPCODE   	 = A.CAMPCODE 				    " _
                       & "    and F.SHARYOTYPE   = A.SHARYOTYPEF 		        " _
                       & "    and F.TSHABAN      = A.TSHABANF 			        " _
                       & "    and F.STYMD       <= @P1                          " _
                       & "    and F.ENDYMD      >= @P1                          " _
                       & "    and F.DELFLG      <> '1' 						    " _
                       & "   LEFT JOIN MA004_SHARYOC G 						    " _
                       & "     ON G.CAMPCODE   	 = A.CAMPCODE 				    " _
                       & "    and G.SHARYOTYPE   = A.SHARYOTYPEB 		        " _
                       & "    and G.TSHABAN      = A.TSHABANB 	                " _
                       & "    and G.STYMD       <= @P1                          " _
                       & "    and G.ENDYMD      >= @P1                          " _
                       & "    and G.DELFLG      <> '1' 						    " _
                       & "  Where A.CAMPCODE   = @P2                            " _
                       & "    and A.MANGUORG   = @P3                            " _
                       & "    and A.DELFLG    <> '1'                            " _
                       & "    and trim(A.SHARYOTYPEF) = ''                      " _
                       & "    and trim(A.TSHABANF) = ''                         " _
                       & "  ORDER BY A.SEQ ,A.GSHABAN                           "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                PARA1.Value = WW_SHUKODATE
                PARA2.Value = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    PARA3.Value = WF_DEFORG.Text
                Else
                    PARA3.Value = work.WF_SEL_SHIPORG.Text
                End If

                '○SQL実行
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力設定
                While SQLdr.Read
                    WF_ListGSHABAN_CONT.Items.Add(New ListItem("", SQLdr("GSHABAN")))
                    WF_ListOILTYPE_CONT.Items.Add(New ListItem("", SQLdr("OILTYPE")))
                    WF_ListOILTYPENAME_CONT.Items.Add(New ListItem("", SQLdr("OILTYPENAME")))
                    WF_ListSHAFUKU_CONT.Items.Add(New ListItem("", SQLdr("SHAFUKU")))
                    WF_ListOWNCODE_CONT.Items.Add(New ListItem("", SQLdr("OWNCODE")))
                    WF_ListOWNCODENAME_CONT.Items.Add(New ListItem("", SQLdr("OWNCODENAME")))
                    WF_ListSHARYOSTATUS_CONT.Items.Add(New ListItem("", SQLdr("SHARYOSTATUS")))
                    WF_ListSHARYOSTATUSNAME_CONT.Items.Add(New ListItem("", SQLdr("SHARYOSTATUSNAME")))
                    WF_ListLICNPLTNOF_CONT.Items.Add(New ListItem("", SQLdr("LICNPLTNOF")))
                    WF_ListLICNPLTNOB_CONT.Items.Add(New ListItem("", SQLdr("LICNPLTNOB")))

                End While

                'Close()
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "CONTCHASSIS SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:CONTCHASSIS Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' LeftBox業務車番DataBind
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DataBindGSHABAN()

        Dim GSHABANtbl As New DataTable()
        Dim GSHABANrow As DataRow

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

        '○カラム設定
        GSHABANtbl.Columns.Add("GSHABAN", GetType(String))
        GSHABANtbl.Columns.Add("OILTYPENAME", GetType(String))
        GSHABANtbl.Columns.Add("SHAFUKU", GetType(String))
        GSHABANtbl.Columns.Add("OWNCODENAME", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOSTATUS", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOSTATUSNAME", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANF", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANB", GetType(String))
        GSHABANtbl.Columns.Add("TSHABANB2", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOF", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOB", GetType(String))
        GSHABANtbl.Columns.Add("LICNPLTNOB2", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO1", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO2", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO3", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO4", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO5", GetType(String))
        GSHABANtbl.Columns.Add("SHARYOINFO6", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS1", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS2", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS3", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS4", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS5", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS6", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS7", GetType(String))
        GSHABANtbl.Columns.Add("HSTATUS8", GetType(String))

        '車両追加情報
        For i As Integer = 0 To WF_ListGSHABAN.Items.Count - 1
            GSHABANrow = GSHABANtbl.NewRow

            'テーブル項目セット
            GSHABANrow("GSHABAN") = WF_ListGSHABAN.Items(i).Value
            GSHABANrow("OILTYPENAME") = WF_ListOILTYPENAME.Items(i).Value
            GSHABANrow("SHAFUKU") = WF_ListSHAFUKU.Items(i).Value
            GSHABANrow("OWNCODENAME") = WF_ListOWNCODENAME.Items(i).Value
            GSHABANrow("SHARYOSTATUS") = WF_ListSHARYOSTATUS.Items(i).Value
            GSHABANrow("SHARYOSTATUSNAME") = WF_ListSHARYOSTATUSNAME.Items(i).Value
            GSHABANrow("LICNPLTNOF") = WF_ListLICNPLTNOF.Items(i).Value
            GSHABANrow("LICNPLTNOB") = WF_ListLICNPLTNOB.Items(i).Value
            GSHABANrow("LICNPLTNOB2") = WF_ListLICNPLTNOB2.Items(i).Value
            GSHABANrow("TSHABANF") = WF_ListTSHABANF.Items(i).Value
            GSHABANrow("TSHABANB") = WF_ListTSHABANB.Items(i).Value
            GSHABANrow("TSHABANB2") = WF_ListTSHABANB2.Items(i).Value
            GSHABANrow("SHARYOINFO1") = WF_ListSHARYOINFO1.Items(i).Value
            GSHABANrow("SHARYOINFO2") = WF_ListSHARYOINFO2.Items(i).Value
            GSHABANrow("SHARYOINFO3") = WF_ListSHARYOINFO3.Items(i).Value
            GSHABANrow("SHARYOINFO4") = WF_ListSHARYOINFO4.Items(i).Value
            GSHABANrow("SHARYOINFO5") = WF_ListSHARYOINFO5.Items(i).Value
            GSHABANrow("SHARYOINFO6") = WF_ListSHARYOINFO6.Items(i).Value
            GSHABANrow("HSTATUS1") = "○"
            GSHABANrow("HSTATUS2") = "○"
            GSHABANrow("HSTATUS3") = "○"
            GSHABANrow("HSTATUS4") = "○"
            GSHABANrow("HSTATUS5") = "○"
            GSHABANrow("HSTATUS6") = "○"
            GSHABANrow("HSTATUS7") = "○"
            GSHABANrow("HSTATUS8") = "○"

            'テーブル追加
            GSHABANtbl.Rows.Add(GSHABANrow)

        Next

        '○配送状況セット
        'ソート
        Dim WW_TBLVIEW As DataView = New DataView(T00004tbl)
        WW_TBLVIEW.Sort = "SHUKODATE , GSHABAN"

        For Each GSHABANrow In GSHABANtbl.Rows

            '業務車番・出庫日が合致する場合
            WW_TBLVIEW.RowFilter = "GSHABAN = '" & GSHABANrow("GSHABAN") & "' and " & "SHUKODATE = '" & WW_SHUKODATE.ToString("yyyy/MM/dd") & "'"

            For Each WW_TBLVIEWrow As DataRowView In WW_TBLVIEW
                Select Case WW_TBLVIEWrow("TRIPNO")
                    Case "001"
                        GSHABANrow("HSTATUS1") = "●"
                    Case "002"
                        GSHABANrow("HSTATUS2") = "●"
                    Case "003"
                        GSHABANrow("HSTATUS3") = "●"
                    Case "004"
                        GSHABANrow("HSTATUS4") = "●"
                    Case "005"
                        GSHABANrow("HSTATUS5") = "●"
                    Case "006"
                        GSHABANrow("HSTATUS6") = "●"
                    Case "007"
                        GSHABANrow("HSTATUS7") = "●"
                    Case "008"
                        GSHABANrow("HSTATUS8") = "●"
                    Case Else
                End Select
            Next

        Next

        '○データバインド
        WF_GSHABAN_Rep.DataSource = GSHABANtbl
        WF_GSHABAN_Rep.DataBind()

        '○イベント設定
        For i As Integer = 0 To WF_GSHABAN_Rep.Items.Count - 1
            Dim WW_SHARYOSTATUS As String = CType(WF_GSHABAN_Rep.Items(i).FindControl("WF_GSHABAN_ItemCell6"), System.Web.UI.WebControls.TableCell).Text
            '車両ステータスが運行可能ならイベント追加
            If String.IsNullOrEmpty(WW_SHARYOSTATUS) OrElse WW_SHARYOSTATUS = "1" Then
                CType(WF_GSHABAN_Rep.Items(i).FindControl("WF_GSHABAN_Items"), System.Web.UI.WebControls.TableRow).Attributes.Add("ondblclick", "Leftbox_Gyou('" & i & "');")
            End If
        Next

        'Close()
        WW_TBLVIEW.Dispose()
        WW_TBLVIEW = Nothing
        GSHABANtbl.Dispose()
        GSHABANtbl = Nothing

    End Sub

    ''' <summary>
    ''' LeftBoxコンテナシャーシDataBind
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DataBindCONTCHASSIS()

        Dim CONTCHASSIStbl As New DataTable()
        Dim CONTCHASSISrow As DataRow

        '出庫日を取得
        Dim WW_SHUKODATE As Date
        If Date.TryParse(WF_SHUKODATE.Text, WW_SHUKODATE) Then
            If WW_SHUKODATE < C_DEFAULT_YMD Then
                WW_SHUKODATE = Date.Now
            End If
        Else
            WW_SHUKODATE = Date.Now
        End If

        '○カラム設定
        CONTCHASSIStbl.Columns.Add("GSHABAN", GetType(String))
        CONTCHASSIStbl.Columns.Add("OILTYPENAME", GetType(String))
        CONTCHASSIStbl.Columns.Add("SHAFUKU", GetType(String))
        CONTCHASSIStbl.Columns.Add("OWNCODENAME", GetType(String))
        CONTCHASSIStbl.Columns.Add("SHARYOSTATUS", GetType(String))
        CONTCHASSIStbl.Columns.Add("SHARYOSTATUSNAME", GetType(String))
        CONTCHASSIStbl.Columns.Add("LICNPLTNO", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS1", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS2", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS3", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS4", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS5", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS6", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS7", GetType(String))
        CONTCHASSIStbl.Columns.Add("HSTATUS8", GetType(String))

        '車両追加情報
        For i As Integer = 0 To WF_ListGSHABAN_CONT.Items.Count - 1
            CONTCHASSISrow = CONTCHASSIStbl.NewRow

            'テーブル項目セット
            CONTCHASSISrow("GSHABAN") = WF_ListGSHABAN_CONT.Items(i).Value
            CONTCHASSISrow("OILTYPENAME") = WF_ListOILTYPENAME_CONT.Items(i).Value
            CONTCHASSISrow("SHAFUKU") = WF_ListSHAFUKU_CONT.Items(i).Value
            CONTCHASSISrow("OWNCODENAME") = WF_ListOWNCODENAME_CONT.Items(i).Value
            CONTCHASSISrow("SHARYOSTATUS") = WF_ListSHARYOSTATUS_CONT.Items(i).Value
            CONTCHASSISrow("SHARYOSTATUSNAME") = WF_ListSHARYOSTATUSNAME_CONT.Items(i).Value
            CONTCHASSISrow("LICNPLTNO") = WF_ListLICNPLTNOF_CONT.Items(i).Value
            CONTCHASSISrow("LICNPLTNO") = WF_ListLICNPLTNOB_CONT.Items(i).Value
            CONTCHASSISrow("HSTATUS1") = "○"
            CONTCHASSISrow("HSTATUS2") = "○"
            CONTCHASSISrow("HSTATUS3") = "○"
            CONTCHASSISrow("HSTATUS4") = "○"
            CONTCHASSISrow("HSTATUS5") = "○"
            CONTCHASSISrow("HSTATUS6") = "○"
            CONTCHASSISrow("HSTATUS7") = "○"
            CONTCHASSISrow("HSTATUS8") = "○"

            'テーブル追加
            CONTCHASSIStbl.Rows.Add(CONTCHASSISrow)

        Next

        '○配送状況セット
        'ソート
        Dim WW_TBLVIEW As DataView = New DataView(T00004tbl)
        WW_TBLVIEW.Sort = "SHUKODATE , GSHABAN"

        For Each CONTCHASSISrow In CONTCHASSIStbl.Rows

            '業務車番・出庫日が合致する場合
            WW_TBLVIEW.RowFilter = "GSHABAN = '" & CONTCHASSISrow("GSHABAN") & "' and " & "SHUKODATE = '" & WW_SHUKODATE.ToString("yyyy/MM/dd") & "'"
            For i As Integer = 1 To WW_TBLVIEW.Count
                If i > 8 Then Exit For
                CONTCHASSISrow("HSTATUS" & i) = "●"
            Next
        Next

        '○データバインド
        WF_CONTCHASSIS_Rep.DataSource = CONTCHASSIStbl
        WF_CONTCHASSIS_Rep.DataBind()

        '○イベント設定
        For i As Integer = 0 To WF_CONTCHASSIS_Rep.Items.Count - 1
            Dim WW_SHARYOSTATUS As String = CType(WF_CONTCHASSIS_Rep.Items(i).FindControl("WF_CONTCHASSIS_ItemCell6"), System.Web.UI.WebControls.TableCell).Text
            '車両ステータスが運行可能ならイベント追加
            If String.IsNullOrEmpty(WW_SHARYOSTATUS) OrElse WW_SHARYOSTATUS = "1" Then
                CType(WF_CONTCHASSIS_Rep.Items(i).FindControl("WF_CONTCHASSIS_Items"), System.Web.UI.WebControls.TableRow).Attributes.Add("ondblclick", "Leftbox_Gyou('" & i & "');")
            End If
        Next

        'Close()
        WW_TBLVIEW.Dispose()
        WW_TBLVIEW = Nothing
        CONTCHASSIStbl.Dispose()
        CONTCHASSIStbl = Nothing

    End Sub

#End Region

#Region "UPLOADファイル"

    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        WW_ERRLIST = New List(Of String)

        '○初期処理
        rightview.SetErrorReport("")

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)


        '■■■ UPLOAD_XLSデータ取得 ■■■
        If work.WF_SEL_CAMPCODE.Text = GRT00004WRKINC.C_CAMPCODE_NJS Then
            XLStoINPtblForNJS(WW_ERRCODE)
        Else
            XLStoINPtbl(WW_ERRCODE)
        End If
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '■■■ INPデータ登録 ■■■
        INPtbltoT4tbl(WW_ERRCODE)

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00004tbl, False)

        '○サマリ処理 
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)
        SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        Dim WW_ERRWORD As String = rightview.GetErrorReport()
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00004INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00004INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)

        '○Detailクリア
        'detailboxヘッダークリア
        ClearDetailBox()

        '■■■ Detailデータ設定 ■■■
        '画面切替設定
        WF_IsHideDetailBox.Value = "1"

        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        '○Detail初期設定
        T00004INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    ''' Excel→T00004tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtbl(ByRef O_RTN As String)

        '■■■ UPLOAD_XLSデータ取得 ■■■   ☆☆☆ 2015/4/30追加
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRT00004WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(String.Empty, Master.PROF_REPORT)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                O_RTN = C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR
                Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            O_RTN = CS0023XLSUPLOAD.ERR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "CS0023XLSUPLOAD")
            Exit Sub
        End If
        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSUPLOADrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Rows.Count - 1
            CS0023XLSUPLOADrow.ItemArray = CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSUPLOADrow.Item(j)) Or IsNothing(CS0023XLSUPLOADrow.Item(j)) Then
                    CS0023XLSUPLOADrow.Item(j) = ""
                End If
            Next
            CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray = CS0023XLSUPLOADrow.ItemArray
        Next

        '○CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each column As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(column.ColumnName)
        Next


        '■■■ エラーレポート準備 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL

        '○T00004INPtblカラム設定
        Master.CreateEmptyTable(T00004INPtbl)

        '○必須項目の指定チェック
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TORICODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKADATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKODATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TRIPNO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("DROPNO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKABASHO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("GSHABAN") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE") AndAlso
            (CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") OrElse
             CS0023XLSUPLOAD.TBLDATA.Columns.Contains("OILTYPE") AndAlso CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCT2")) Then
        Else
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
            rightview.AddErrorReport("・アップロードExcelに『出荷日、出庫日、荷主、(品名コード or 油種・品名２)、出荷場所、車番、乗務員、トリップ、ドロップ』が存在しません。")
            Exit Sub
        End If

        '○ソート処理
        CS0026TBLSORTget.TABLE = CS0023XLSUPLOAD.TBLDATA
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") Then
            CS0026TBLSORTget.SORTING = "TORICODE, SHUKADATE, SHUKODATE, PRODUCTCODE, TRIPNO, DROPNO, SHUKABASHO, GSHABAN, STAFFCODE, PRODUCTCODE"
        ElseIf CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCT2") Then
            CS0026TBLSORTget.SORTING = "TORICODE, SHUKADATE, SHUKODATE, OILTYPE, TRIPNO, DROPNO, SHUKABASHO, GSHABAN, STAFFCODE, PRODUCT2"
        End If
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(CS0023XLSUPLOAD.TBLDATA)

        '■■■ Excelデータ毎にチェック＆更新 ■■■
        Dim WW_INDEX As Integer = 0
        For Each uploadRow In CS0023XLSUPLOAD.TBLDATA.Rows

            '○XLSTBL明細⇒T00004INProw
            Dim T00004INProw = T00004INPtbl.NewRow

            T00004INProw("LINECNT") = 0
            T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            T00004INProw("TIMSTP") = "0"
            T00004INProw("SELECT") = 1
            T00004INProw("HIDDEN") = 0

            T00004INProw("INDEX") = WW_INDEX
            WW_INDEX += WW_INDEX

            T00004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            T00004INProw("TERMORG") = WF_DEFORG.Text


            If WW_COLUMNS.IndexOf("ORDERNO") < 0 Then
                T00004INProw("ORDERNO") = ""
            Else
                T00004INProw("ORDERNO") = uploadRow("ORDERNO")
            End If

            If WW_COLUMNS.IndexOf("DETAILNO") < 0 Then
                T00004INProw("DETAILNO") = ""
            Else
                T00004INProw("DETAILNO") = uploadRow("DETAILNO")
            End If

            If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                T00004INProw("OILTYPE") = ""
            Else
                T00004INProw("OILTYPE") = uploadRow("OILTYPE")
            End If

            If WW_COLUMNS.IndexOf("TRIPNO") < 0 Then
                T00004INProw("TRIPNO") = ""
            Else
                T00004INProw("TRIPNO") = uploadRow("TRIPNO")
            End If

            If WW_COLUMNS.IndexOf("DROPNO") < 0 Then
                T00004INProw("DROPNO") = ""
            Else
                T00004INProw("DROPNO") = uploadRow("DROPNO")
            End If

            If WW_COLUMNS.IndexOf("SEQ") < 0 Then
                T00004INProw("SEQ") = ""
            Else
                T00004INProw("SEQ") = uploadRow("SEQ")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") < 0 Then
                T00004INProw("TORICODE") = ""
            Else
                T00004INProw("TORICODE") = uploadRow("TORICODE")
            End If

            If WW_COLUMNS.IndexOf("STORICODE") < 0 Then
                T00004INProw("STORICODE") = ""
            Else
                T00004INProw("STORICODE") = uploadRow("STORICODE")
            End If

            If WW_COLUMNS.IndexOf("ORDERORG") < 0 Then
                T00004INProw("ORDERORG") = WF_DEFORG.Text
            Else
                T00004INProw("ORDERORG") = uploadRow("ORDERORG")
            End If

            If WW_COLUMNS.IndexOf("SHUKODATE") < 0 Then
                T00004INProw("SHUKODATE") = ""
            Else
                T00004INProw("SHUKODATE") = uploadRow("SHUKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIKODATE") < 0 Then
                T00004INProw("KIKODATE") = ""
            Else
                T00004INProw("KIKODATE") = uploadRow("KIKODATE")
            End If

            If WW_COLUMNS.IndexOf("KIJUNDATE") < 0 Then
                T00004INProw("KIJUNDATE") = ""
            Else
                T00004INProw("KIJUNDATE") = uploadRow("KIJUNDATE")
            End If

            If WW_COLUMNS.IndexOf("SHUKADATE") < 0 Then
                T00004INProw("SHUKADATE") = ""
            Else
                T00004INProw("SHUKADATE") = uploadRow("SHUKADATE")
            End If

            If WW_COLUMNS.IndexOf("TUMIOKIKBN") < 0 Then
                T00004INProw("TUMIOKIKBN") = ""
            Else
                T00004INProw("TUMIOKIKBN") = uploadRow("TUMIOKIKBN")
            End If

            If WW_COLUMNS.IndexOf("URIKBN") < 0 Then
                T00004INProw("URIKBN") = "1"
            Else
                T00004INProw("URIKBN") = uploadRow("URIKBN")
            End If

            If WW_COLUMNS.IndexOf("STATUS") < 0 Then
                T00004INProw("STATUS") = ""
            Else
                T00004INProw("STATUS") = uploadRow("STATUS")
            End If

            If WW_COLUMNS.IndexOf("SHIPORG") < 0 Then
                T00004INProw("SHIPORG") = WF_DEFORG.Text
            Else
                T00004INProw("SHIPORG") = uploadRow("SHIPORG").ToString.PadLeft(WF_DEFORG.Text.Length, "0")
            End If

            If WW_COLUMNS.IndexOf("SHUKABASHO") < 0 Then
                T00004INProw("SHUKABASHO") = ""
            Else
                T00004INProw("SHUKABASHO") = uploadRow("SHUKABASHO")
            End If

            If WW_COLUMNS.IndexOf("INTIME") < 0 Then
                T00004INProw("INTIME") = ""
            Else
                T00004INProw("INTIME") = uploadRow("INTIME")
            End If

            If WW_COLUMNS.IndexOf("OUTTIME") < 0 Then
                T00004INProw("OUTTIME") = ""
            Else
                T00004INProw("OUTTIME") = uploadRow("OUTTIME")
            End If

            If WW_COLUMNS.IndexOf("SHUKADENNO") < 0 Then
                T00004INProw("SHUKADENNO") = ""
            Else
                T00004INProw("SHUKADENNO") = uploadRow("SHUKADENNO")
            End If

            If WW_COLUMNS.IndexOf("TUMISEQ") < 0 Then
                T00004INProw("TUMISEQ") = ""
            Else
                T00004INProw("TUMISEQ") = uploadRow("TUMISEQ")
            End If

            If WW_COLUMNS.IndexOf("TUMIBA") < 0 Then
                T00004INProw("TUMIBA") = ""
            Else
                T00004INProw("TUMIBA") = uploadRow("TUMIBA")
            End If

            If WW_COLUMNS.IndexOf("GATE") < 0 Then
                T00004INProw("GATE") = ""
            Else
                T00004INProw("GATE") = uploadRow("GATE")
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") < 0 Then
                T00004INProw("GSHABAN") = ""
            Else
                T00004INProw("GSHABAN") = uploadRow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("RYOME") < 0 Then
                T00004INProw("RYOME") = "1"
            Else
                If uploadRow("RYOME") = Nothing Then
                    T00004INProw("RYOME") = "1"
                Else
                    T00004INProw("RYOME") = uploadRow("RYOME")
                End If
            End If

            If WW_COLUMNS.IndexOf("CONTCHASSIS") < 0 Then
                T00004INProw("CONTCHASSIS") = ""
            Else
                T00004INProw("CONTCHASSIS") = uploadRow("CONTCHASSIS")
            End If

            If WW_COLUMNS.IndexOf("SHAFUKU") < 0 Then
                T00004INProw("SHAFUKU") = ""
            Else
                T00004INProw("SHAFUKU") = uploadRow("SHAFUKU")
            End If

            If WW_COLUMNS.IndexOf("STAFFCODE") < 0 Then
                T00004INProw("STAFFCODE") = ""
            Else
                T00004INProw("STAFFCODE") = uploadRow("STAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("SUBSTAFFCODE") < 0 Then
                T00004INProw("SUBSTAFFCODE") = ""
            Else
                T00004INProw("SUBSTAFFCODE") = uploadRow("SUBSTAFFCODE")
            End If

            If WW_COLUMNS.IndexOf("STTIME") < 0 Then
                T00004INProw("STTIME") = ""
            Else
                T00004INProw("STTIME") = uploadRow("STTIME")
            End If

            If WW_COLUMNS.IndexOf("TORIORDERNO") < 0 Then
                T00004INProw("TORIORDERNO") = ""
            Else
                T00004INProw("TORIORDERNO") = uploadRow("TORIORDERNO")
            End If

            If WW_COLUMNS.IndexOf("TODOKEDATE") < 0 Then
                T00004INProw("TODOKEDATE") = ""
            Else
                T00004INProw("TODOKEDATE") = uploadRow("TODOKEDATE")
            End If

            If WW_COLUMNS.IndexOf("TODOKETIME") < 0 Then
                T00004INProw("TODOKETIME") = ""
            Else
                T00004INProw("TODOKETIME") = uploadRow("TODOKETIME")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") < 0 Then
                T00004INProw("TODOKECODE") = ""
            Else
                T00004INProw("TODOKECODE") = uploadRow("TODOKECODE")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                T00004INProw("PRODUCT1") = ""
            Else
                T00004INProw("PRODUCT1") = uploadRow("PRODUCT1")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                T00004INProw("PRODUCT2") = ""
            Else
                T00004INProw("PRODUCT2") = uploadRow("PRODUCT2")
            End If

            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                T00004INProw("PRODUCTCODE") = ""
            Else
                T00004INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
            End If

            If WW_COLUMNS.IndexOf("PRATIO") < 0 Then
                T00004INProw("PRATIO") = ""
            Else
                T00004INProw("PRATIO") = uploadRow("PRATIO")
            End If

            If WW_COLUMNS.IndexOf("SMELLKBN") < 0 Then
                T00004INProw("SMELLKBN") = ""
            Else
                T00004INProw("SMELLKBN") = uploadRow("SMELLKBN")
            End If

            If WW_COLUMNS.IndexOf("CONTNO") < 0 Then
                T00004INProw("CONTNO") = ""
            Else
                T00004INProw("CONTNO") = uploadRow("CONTNO")
            End If

            If WW_COLUMNS.IndexOf("HTANI") < 0 Then
                T00004INProw("HTANI") = ""
            Else
                If uploadRow("HTANI") = Nothing Then
                    T00004INProw("HTANI") = ""
                Else
                    T00004INProw("HTANI") = uploadRow("HTANI")
                End If
            End If

            If WW_COLUMNS.IndexOf("SURYO") < 0 Then
                T00004INProw("SURYO") = ""
            Else
                T00004INProw("SURYO") = uploadRow("SURYO")
            End If

            If WW_COLUMNS.IndexOf("DAISU") < 0 Then
                T00004INProw("DAISU") = ""
            Else
                T00004INProw("DAISU") = uploadRow("DAISU")
            End If

            If WW_COLUMNS.IndexOf("JSURYO") < 0 Then
                T00004INProw("JSURYO") = ""
            Else
                T00004INProw("JSURYO") = uploadRow("JSURYO")
            End If

            If WW_COLUMNS.IndexOf("JDAISU") < 0 Then
                T00004INProw("JDAISU") = ""
            Else
                T00004INProw("JDAISU") = uploadRow("JDAISU")
            End If

            If WW_COLUMNS.IndexOf("REMARKS1") < 0 Then
                T00004INProw("REMARKS1") = ""
            Else
                T00004INProw("REMARKS1") = uploadRow("REMARKS1")
            End If

            If WW_COLUMNS.IndexOf("REMARKS2") < 0 Then
                T00004INProw("REMARKS2") = ""
            Else
                T00004INProw("REMARKS2") = uploadRow("REMARKS2")
            End If

            If WW_COLUMNS.IndexOf("REMARKS3") < 0 Then
                T00004INProw("REMARKS3") = ""
            Else
                T00004INProw("REMARKS3") = uploadRow("REMARKS3")
            End If

            If WW_COLUMNS.IndexOf("REMARKS4") < 0 Then
                T00004INProw("REMARKS4") = ""
            Else
                T00004INProw("REMARKS4") = uploadRow("REMARKS4")
            End If

            If WW_COLUMNS.IndexOf("REMARKS5") < 0 Then
                T00004INProw("REMARKS5") = ""
            Else
                T00004INProw("REMARKS5") = uploadRow("REMARKS5")
            End If

            If WW_COLUMNS.IndexOf("REMARKS6") < 0 Then
                T00004INProw("REMARKS6") = ""
            Else
                T00004INProw("REMARKS6") = uploadRow("REMARKS6")
            End If

            If WW_COLUMNS.IndexOf("TAXKBN") < 0 Then
                T00004INProw("TAXKBN") = "0"
            Else
                T00004INProw("TAXKBN") = uploadRow("TAXKBN")
            End If

            If WW_COLUMNS.IndexOf("DELFLG") < 0 Then
                T00004INProw("DELFLG") = "0"
            Else
                T00004INProw("DELFLG") = uploadRow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") < 0 Then
                T00004INProw("SHARYOTYPEF") = ""
            Else
                T00004INProw("SHARYOTYPEF") = uploadRow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") < 0 Then
                T00004INProw("SHARYOTYPEB") = ""
            Else
                T00004INProw("SHARYOTYPEB") = uploadRow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") < 0 Then
                T00004INProw("SHARYOTYPEB2") = ""
            Else
                T00004INProw("SHARYOTYPEB2") = uploadRow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("JXORDERID") < 0 Then
                T00004INProw("JXORDERID") = ""
            Else
                T00004INProw("JXORDERID") = uploadRow("JXORDERID")
            End If

            'Grid追加明細（新規追加と同じ）とする
            T00004INProw("WORK_NO") = ""

            '品名コード未存在時は油種・品名1・品名2から作成
            If WW_COLUMNS.IndexOf("PRODUCTCODE") < 0 Then
                If Not String.IsNullOrEmpty(T00004INProw("OILTYPE")) AndAlso
                    Not String.IsNullOrEmpty(T00004INProw("PRODUCT1")) AndAlso
                    Not String.IsNullOrEmpty(T00004INProw("PRODUCT2")) Then
                    T00004INProw("PRODUCTCODE") = T00004INProw("CAMPCODE") & T00004INProw("OILTYPE") & T00004INProw("PRODUCT1") & T00004INProw("PRODUCT2")
                End If
            ElseIf Not String.IsNullOrEmpty(T00004INProw("PRODUCTCODE")) Then
                '油種未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("OILTYPE") < 0 Then
                    T00004INProw("OILTYPE") = Mid(T00004INProw("PRODUCTCODE").ToString, 3, 2)
                End If
                '品名１未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT1") < 0 Then
                    T00004INProw("PRODUCT1") = Mid(T00004INProw("PRODUCTCODE").ToString, 5, 2)
                End If
                '品名２未存在は品名コードから作成
                If WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
                    T00004INProw("PRODUCT2") = Mid(T00004INProw("PRODUCTCODE").ToString, 7, 5)
                End If

            End If
            'JXオーダーの場合はオーダーステータス設定
            If Not String.IsNullOrEmpty(T00004INProw("JXORDERID")) Then
                SetOrderStatus(T00004INProw)
            End If
            '○名称付与
            CODENAME_set(T00004INProw)

            '入力テーブル追加
            T00004INPtbl.Rows.Add(T00004INProw)

        Next
    End Sub


    ''' <summary>
    ''' NJS Excel→T00004tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub XLStoINPtblForNJS(ByRef O_RTN As String)

        '■■■ UPLOAD_XLSデータ取得 ■■■   ☆☆☆ 2015/4/30追加
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRT00004WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD(C_UPLOAD_EXCEL_REPORTID_NJS, C_UPLOAD_EXCEL_PROFID_NJS)
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                O_RTN = C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR
                Master.Output(O_RTN, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            O_RTN = CS0023XLSUPLOAD.ERR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "CS0023XLSUPLOAD")
            Exit Sub
        End If
        'EXCELデータの初期化（DBNullを撲滅）
        Dim CS0023XLSUPLOADrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For i As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Rows.Count - 1
            CS0023XLSUPLOADrow.ItemArray = CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray

            For j As Integer = 0 To CS0023XLSUPLOAD.TBLDATA.Columns.Count - 1
                If IsDBNull(CS0023XLSUPLOADrow.Item(j)) Or IsNothing(CS0023XLSUPLOADrow.Item(j)) Then
                    CS0023XLSUPLOADrow.Item(j) = ""
                End If
            Next
            CS0023XLSUPLOAD.TBLDATA.Rows(i).ItemArray = CS0023XLSUPLOADrow.ItemArray
        Next

        '○CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each column As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(column.ColumnName)
        Next


        '■■■ エラーレポート準備 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL

        '○T00004INPtblカラム設定
        Master.CreateEmptyTable(T00004INPtbl)

        '○必須項目の指定チェック
        If CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHUKODATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKEDATE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("TODOKECODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("PRODUCTCODE") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SURYO") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("SHARYOCD") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE1") AndAlso
            CS0023XLSUPLOAD.TBLDATA.Columns.Contains("STAFFCODE2") Then
        Else
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            rightview.AddErrorReport("・アップロードExcelに『出庫日、納入日、届先コード、品名コード、数量、車輛コード、運転手コード1、運転手コード2』が存在しません。")
            Exit Sub
        End If

        '○JSRコードマスタ作成
        Using jsrCvt As JSRCODE_MASTER = New JSRCODE_MASTER()
            jsrCvt.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                jsrCvt.ORGCODE = WF_DEFORG.Text
            Else
                jsrCvt.ORGCODE = work.WF_SEL_SHIPORG.Text
            End If
            'JSRコード一括読込
            If jsrCvt.ReadJSRData() = False Then
                O_RTN = jsrCvt.ERR
                Master.Output(O_RTN, C_MESSAGE_TYPE.ABORT, "read JSRCODE")
                Exit Sub
            End If

            '■■■ Excelデータ毎にチェック＆更新 ■■■
            Dim WW_INDEX As Integer = 0
            For Each uploadRow In CS0023XLSUPLOAD.TBLDATA.Rows

                Dim datTodoke = New JSRCODE_MASTER.JSRCODE_TODOKE
                Dim datProduct = New JSRCODE_MASTER.JSRCODE_PRODUCT
                Dim datStaff = New JSRCODE_MASTER.JSRCODE_STAFF
                Dim datSubStaff = New JSRCODE_MASTER.JSRCODE_STAFF
                Dim WW_SHUKODATE As Date
                Dim WW_SHUKADATE As Date
                Dim WW_TODOKEDATE As Date
                Dim WW_KIKODATE As Date
                Dim WW_RELATIVEDAYS3 As Integer
                Dim WW_RELATIVEDAYS4 As Integer
                Dim WW_NUM As Integer

                '2:出庫日
                If Not DateTime.TryParseExact(uploadRow("SHUKODATE"), "yyyyMMdd", Nothing, Nothing, WW_SHUKODATE) Then
                    O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    rightview.AddErrorReport("・アップロードExcel『出庫日』の日付書式が正しくありません。")
                    Exit Sub
                End If
                '3:納入日
                If Not DateTime.TryParseExact(uploadRow("TODOKEDATE"), "yyyyMMdd", Nothing, Nothing, WW_TODOKEDATE) Then
                    O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    rightview.AddErrorReport("・アップロードExcel『納入日』の日付書式が正しくありません。")
                    Exit Sub
                End If
                '20:相対日数１（出荷（積））
                '21:相対日数２（出発）
                '22:相対日数３（納入）
                '23:相対日数４（帰庫）
                '24:相対日数５（点検）
                If WW_COLUMNS.Contains("RELATIVEDAYS3") Then
                    If Not Int32.TryParse(uploadRow("RELATIVEDAYS3"), WW_RELATIVEDAYS3) Then
                        WW_RELATIVEDAYS3 = 0
                    End If
                End If
                If WW_COLUMNS.Contains("RELATIVEDAYS4") Then
                    If Not Int32.TryParse(uploadRow("RELATIVEDAYS4"), WW_RELATIVEDAYS4) Then
                        WW_RELATIVEDAYS4 = 0
                    End If
                End If

                '出庫日    ＝ 出荷日
                '出荷日    ＝ 出庫日
                '届日      ＝ 納入日
                '出荷日    ＝ 出荷日
                WW_SHUKADATE = WW_SHUKODATE
                WW_SHUKODATE = WW_SHUKODATE.AddDays(WW_RELATIVEDAYS3)
                WW_TODOKEDATE = WW_TODOKEDATE
                WW_KIKODATE = WW_TODOKEDATE.AddDays(WW_RELATIVEDAYS4)

                '***** 取込除外条件 *****
                ' ①運転手コード１
                '  -a NULL
                '  -b 0000
                '  -c 0001
                ' ②車輛コード
                '  -a NULL
                '  -b 9XX
                ' ③出庫日
                '  -a 当日以前
                If String.IsNullOrWhiteSpace(uploadRow("STAFFCODE1")) OrElse
                    uploadRow("STAFFCODE1") = "0000" OrElse
                    uploadRow("STAFFCODE1") = "0001" Then
                    Continue For
                End If
                If String.IsNullOrWhiteSpace(uploadRow("SHARYOCD")) OrElse
                    uploadRow("STAFFCODE1").ToString.StartsWith("9") Then
                    Continue For
                End If
                If WW_SHUKODATE <= CS0050SESSION.LOGONDATE Then
                    Continue For
                End If

                If WW_COLUMNS.Contains("TODOKECODE") Then
                    datTodoke = jsrCvt.GetTodokeCode(uploadRow("TODOKECODE"))
                    If IsNothing(datTodoke) Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(届先コード)"
                        Dim WW_CheckMES2 = uploadRow("TODOKECODE")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                    'グループ作業用届先は除外
                    If datTodoke.IsGroupWork Then
                        Continue For
                    End If
                End If
                If WW_COLUMNS.Contains("PRODUCTCODE") Then
                    If jsrCvt.CovertProductCode(uploadRow("PRODUCTCODE"), datProduct) = False Then
                        '品名マスタ登録

                        'Dim WW_CheckMES1 = "・変換エラーが存在します。(品名コード)"
                        'Dim WW_CheckMES2 = uploadRow("PRODUCTCODE")
                        'ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If
                If WW_COLUMNS.Contains("STAFFCODE1") AndAlso
                    Not String.IsNullOrEmpty(uploadRow("STAFFCODE1")) Then
                    If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE1"), datStaff) = False Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード1)"
                        Dim WW_CheckMES2 = uploadRow("STAFFCODE1")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If
                If WW_COLUMNS.Contains("STAFFCODE2") AndAlso
                    Not String.IsNullOrEmpty(uploadRow("STAFFCODE2")) Then
                    If jsrCvt.CovertStaffCode(uploadRow("STAFFCODE2"), datSubStaff) = False Then
                        Dim WW_CheckMES1 = "・変換エラーが存在します。(運転手コード2)"
                        Dim WW_CheckMES2 = uploadRow("STAFFCODE2")
                        ERRMESSAGE_write_NJS(WW_CheckMES1, WW_CheckMES2, WW_DUMMY, WW_INDEX + 1, C_MESSAGE_NO.BOX_ERROR_EXIST, uploadRow)
                    End If
                End If


                '○XLSTBL明細⇒T00004INProw
                Dim T00004INProw = T00004INPtbl.NewRow
                '***** T4項目順に編集 *****
                T00004INProw("LINECNT") = 0
                T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                T00004INProw("TIMSTP") = "0"
                T00004INProw("SELECT") = 1
                T00004INProw("HIDDEN") = 0
                T00004INProw("INDEX") = WW_INDEX
                WW_INDEX += WW_INDEX

                T00004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(datTodoke.TORICODE) Then
                    T00004INProw("TORICODE") = ""
                Else
                    T00004INProw("TORICODE") = datTodoke.TORICODE
                End If
                If String.IsNullOrEmpty(datProduct.OILTYPE) Then
                    T00004INProw("OILTYPE") = ""
                Else
                    T00004INProw("OILTYPE") = datProduct.OILTYPE
                End If
                T00004INProw("ORDERORG") = work.WF_SEL_ORDERORG.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    T00004INProw("SHIPORG") = WF_DEFORG.Text
                Else
                    T00004INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
                End If

                T00004INProw("KIJUNDATE") = ""                                      ' T3CTLから設定
                T00004INProw("ORDERNO") = ""                                        ' 後続で受注番号自動設定
                T00004INProw("DETAILNO") = "001"
                '車番下３桁のみ使用
                T00004INProw("GSHABAN") = uploadRow("SHARYOCD").ToString.PadLeft(20, "0").Substring(20 - 3)
                T00004INProw("TRIPNO") = "001"
                T00004INProw("DROPNO") = "001"
                T00004INProw("SEQ") = "01"
                If IsNothing(WW_SHUKODATE) Then
                    T00004INProw("SHUKODATE") = ""
                Else
                    T00004INProw("SHUKODATE") = WW_SHUKODATE.ToString("yyyy/MM/dd")
                End If
                T00004INProw("STATUS") = ""                                         ' 後続で設定
                If WW_TODOKEDATE = WW_SHUKODATE Then
                    T00004INProw("TUMIOKIKBN") = ""
                Else
                    '届日≠出庫日（出荷）
                    T00004INProw("TUMIOKIKBN") = "1"
                End If
                If IsNothing(WW_KIKODATE) Then
                    T00004INProw("KIKODATE") = ""
                Else
                    T00004INProw("KIKODATE") = WW_KIKODATE.ToString("yyyy/MM/dd")
                End If
                '出庫日→出荷日
                If IsNothing(WW_SHUKADATE) Then
                    T00004INProw("SHUKADATE") = ""
                Else
                    T00004INProw("SHUKADATE") = WW_SHUKADATE.ToString("yyyy/MM/dd")
                End If
                If IsNothing(WW_TODOKEDATE) Then
                    T00004INProw("TODOKEDATE") = ""
                Else
                    T00004INProw("TODOKEDATE") = WW_TODOKEDATE.ToString("yyyy/MM/dd")
                End If
                If String.IsNullOrEmpty(datTodoke.SHUKABASHO) Then
                    T00004INProw("SHUKABASHO") = ""
                Else
                    T00004INProw("SHUKABASHO") = datTodoke.SHUKABASHO
                End If
                T00004INProw("GATE") = ""
                T00004INProw("TUMIBA") = ""
                T00004INProw("TUMISEQ") = ""
                T00004INProw("SHUKADENNO") = ""
                T00004INProw("INTIME") = ""
                T00004INProw("OUTTIME") = ""
                If String.IsNullOrEmpty(datStaff.STAFFCODE) Then
                    T00004INProw("STAFFCODE") = uploadRow("STAFFCODE1")
                Else
                    T00004INProw("STAFFCODE") = datStaff.STAFFCODE
                End If
                If String.IsNullOrEmpty(datSubStaff.STAFFCODE) Then
                    T00004INProw("SUBSTAFFCODE") = uploadRow("STAFFCODE2")
                Else
                    T00004INProw("SUBSTAFFCODE") = datSubStaff.STAFFCODE
                End If
                T00004INProw("STTIME") = ""
                T00004INProw("RYOME") = "1"
                If String.IsNullOrEmpty(datTodoke.TODOKECODE) Then
                    T00004INProw("TODOKECODE") = uploadRow("TODOKECODE")
                Else
                    T00004INProw("TODOKECODE") = datTodoke.TODOKECODE
                End If
                T00004INProw("TODOKETIME") = ""
                If String.IsNullOrEmpty(datProduct.PRODUCT1) Then
                    T00004INProw("PRODUCT1") = ""
                Else
                    T00004INProw("PRODUCT1") = datProduct.PRODUCT1
                End If
                If String.IsNullOrEmpty(datProduct.PRODUCT2) Then
                    T00004INProw("PRODUCT2") = ""
                Else
                    T00004INProw("PRODUCT2") = datProduct.PRODUCT2
                End If
                If String.IsNullOrEmpty(datProduct.PRODUCTCODE) Then
                    T00004INProw("PRODUCTCODE") = uploadRow("PRODUCTCODE")
                Else
                    T00004INProw("PRODUCTCODE") = datProduct.PRODUCTCODE
                End If
                T00004INProw("CONTNO") = ""
                T00004INProw("PRATIO") = ""
                T00004INProw("SMELLKBN") = ""
                T00004INProw("SHAFUKU") = ""
                T00004INProw("HTANI") = ""
                If WW_COLUMNS.Contains("SURYO") Then
                    '数量単位 NJS(L)→JOT(kL)
                    T00004INProw("SURYO") = uploadRow("SURYO") / 1000
                Else
                    T00004INProw("SURYO") = "0"
                End If
                T00004INProw("DAISU") = "1"
                T00004INProw("JSURYO") = "0"
                T00004INProw("JDAISU") = ""
                If WW_COLUMNS.Contains("SYANAINOTES") Then
                    T00004INProw("REMARKS1") = uploadRow("SYANAINOTES")
                Else
                    T00004INProw("REMARKS1") = ""
                End If
                If WW_COLUMNS.Contains("SYAGAINOTES") Then
                    T00004INProw("REMARKS2") = uploadRow("SYAGAINOTES")
                Else
                    T00004INProw("REMARKS2") = ""
                End If
                T00004INProw("REMARKS3") = ""
                T00004INProw("REMARKS4") = ""
                T00004INProw("REMARKS5") = ""
                T00004INProw("REMARKS6") = ""
                T00004INProw("TORIORDERNO") = ""
                T00004INProw("STORICODE") = ""
                T00004INProw("TERMORG") = WF_DEFORG.Text
                ' T3CTLから設定
                T00004INProw("URIKBN") = ""
                T00004INProw("CONTCHASSIS") = ""
                T00004INProw("SHARYOTYPEF") = ""
                T00004INProw("TSHABANF") = ""
                T00004INProw("SHARYOTYPEB") = ""
                T00004INProw("TSHABANB") = ""
                T00004INProw("SHARYOTYPEB2") = ""
                T00004INProw("TSHABANB2") = ""
                T00004INProw("TAXKBN") = "0"
                T00004INProw("DELFLG") = "0"
                T00004INProw("JXORDERID") = ""

                'Grid追加明細（新規追加と同じ）とする
                T00004INProw("WORK_NO") = ""

                '○名称付与
                CODENAME_set(T00004INProw)

                '入力テーブル追加
                T00004INPtbl.Rows.Add(T00004INProw)

                '****************************
                'トリップ増幅
                '  ※レコード編集後に複製
                '****************************
                '個数＝トリップ
                If WW_COLUMNS.Contains("NUM") Then
                    WW_NUM = uploadRow("NUM")
                Else
                    WW_NUM = 1
                End If
                For tripCnt = 2 To WW_NUM
                    Dim T00004INPAddrow = T00004INPtbl.NewRow()
                    T00004INPAddrow.ItemArray = T00004INProw.ItemArray
                    T00004INPAddrow("TRIPNO") = tripCnt.ToString("000")
                    T00004INPtbl.Rows.Add(T00004INPAddrow)
                Next

                '****************************
                '日跨ぎデータ増幅
                '****************************
                If Not IsNothing(WW_SHUKODATE) AndAlso Not IsNothing(WW_KIKODATE) Then
                    Dim CNT As Integer = (WW_KIKODATE - WW_SHUKODATE).Days
                    For i As Integer = 1 To CNT
                        Dim T00004INPAddrow = T00004INPtbl.NewRow()
                        T00004INPAddrow.ItemArray = T00004INProw.ItemArray
                        '
                        T00004INPAddrow("SHUKODATE") = WW_SHUKODATE.AddDays(CNT).ToString("yyyy/MM/dd")
                        T00004INPtbl.Rows.Add(T00004INPAddrow)
                    Next
                End If

            Next

        End Using

    End Sub


    ''' <summary>
    ''' CSV取込（光英）処理      
    ''' </summary>
    ''' <remarks>ドラッグ＆ドロップアップロード</remarks>
    Protected Sub UPLOAD_KOUEI()
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL

        '-----------------------------------------------
        'CSVファイル取得
        '-----------------------------------------------
        Dim tempDir As String = CS0050SESSION.UPLOAD_PATH & "\" & "UPLOAD_TMP" & "\" & CS0050SESSION.USERID
        Dim tempFiles = System.IO.Directory.GetFiles(tempDir, "*_*_*.csv")
        If tempFiles.Count = 0 Then
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Master.Output(O_RTN, C_MESSAGE_TYPE.ERR, "光英 csv read")
            CS0011LOGWRITE.INFSUBCLASS = "UPLOAD_KOUEI"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "光英 csv read"                    '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "ファイル名：「*_yyyymmdd_yyyymmddhhmm.csv」が必要です"
            CS0011LOGWRITE.MESSAGENO = O_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        Dim files = tempFiles.Select(Function(x) New FileInfo(x)).ToList()

        UPLOAD_KOUEI(files, O_RTN)
    End Sub

    ''' <summary>
    ''' CSV取込（光英）処理      
    ''' </summary>
    ''' <param name="I_FILES">CSVファイル</param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_KOUEI(ByVal I_FILES As List(Of FileInfo), ByRef O_RTN As String)

        '■■■ チェック処理 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        '○画面表示データ復元
        Master.RecoverTable(T00004tbl)

        '光英オーダー管理
        KOUEIMNG = New GRW0001KOUEIORDER With {
            .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
            .ORGCODE = work.WF_SEL_SHIPORG.Text
        }
        '光英マスターデータ管理
        KOUEIMASTER = New KOUEI_MASTER With {
            .ORGCODE = work.WF_SEL_SHIPORG.Text
        }
        If KOUEIMASTER.ReadMasterData <> True Then
            Master.Output(KOUEIMASTER.ERR, C_MESSAGE_TYPE.ABORT)
            O_RTN = KOUEIMASTER.ERR
            Exit Sub
        End If

        '-----------------------------------------------
        '●CSV読み込み（データテーブルに格納）
        '-----------------------------------------------
        For Each file In I_FILES

            'CSV読込
            If KOUEIMNG.ReadCSV(file) <> True Then
                Master.Output(KOUEIMNG.ERR, C_MESSAGE_TYPE.ABORT, "読込エラー")
                O_RTN = KOUEIMNG.ERR
                Exit Sub
            End If
            WF_KoueiLoadFile.Items.Add(New ListItem(file.FullName))
        Next

        'CSVDB登録
        If KOUEIMNG.WriteOrder() <> True Then
            Master.Output(KOUEIMNG.ERR, C_MESSAGE_TYPE.ABORT)
            O_RTN = KOUEIMNG.ERR
            Exit Sub
        End If

        '-----------------------------------------------
        '■出荷場所、届先ＤＢ編集
        '-----------------------------------------------
        MC006tbl_Edit(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
            Exit Sub
        End If

        WW_ERRLIST = New List(Of String)
        '光栄オーダー変換編集
        KOUEItoINPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            O_RTN = WW_ERRCODE
        End If

        '■■■ INPデータ登録 ■■■
        INPtbltoT4tbl(WW_ERRCODE)

        '■■■ GridView更新 ■■■
        ' 状態クリア
        EditOperationText(T00004tbl, False)

        '○サマリ処理 
        CS0026TBLSORTget.TABLE = T00004tbl
        CS0026TBLSORTget.SORTING = "LINECNT ASC , SEQ ASC"
        CS0026TBLSORTget.FILTER = ""
        CS0026TBLSORTget.sort(T00004tbl)

        SUMMRY_SET()

        'エラーメッセージ内の項番、明細番号置き換え
        Dim WW_ERRWORD As String = rightview.GetErrorReport()
        For i As Integer = 0 To T00004INPtbl.Rows.Count - 1
            '項番
            WW_ERRWORD = WW_ERRWORD.Replace("@L" & i.ToString("0000") & "L@", T00004INPtbl.Rows(i)("LINECNT"))
            '明細番号
            WW_ERRWORD = WW_ERRWORD.Replace("@D" & i.ToString("000") & "D@", T00004INPtbl.Rows(i)("SEQ"))
        Next
        rightview.SetErrorReport(WW_ERRWORD)

        '○画面表示データ保存
        Master.SaveTable(T00004tbl)


        'leftBOXキャンセルボタン処理
        WF_ButtonCan_Click()

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        KOUEIMNG.Dispose()

        '○Detail初期設定
        T00004INPtbl.Clear()

        'カーソル設定
        WF_FIELD.Value = "WF_SELTORICODE"
        WF_SELTORICODE.Focus()

    End Sub

    ''' <summary>
    '''  MC006tbl（届先マスタ）編集
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub MC006tbl_Edit(ByRef O_RTN As String)

        '光栄オーダーから出荷・届先レコードを抽出
        Dim lstDest = KOUEIMNG.GetOrder().Where(Function(x) _
                                                    x.Value.TRIPSEQ = GRW0001KOUEIORDER.TRIPSEQ_TYPE.DEPT _
                                                Or (x.Value.TRIPSEQ > GRW0001KOUEIORDER.TRIPSEQ_TYPE.DEPT _
                                                    And x.Value.TRIPSEQ < GRW0001KOUEIORDER.TRIPSEQ_TYPE.FIN _
                                                    And x.Value.DEPOFLAG <> "1")) _
                                         .Select(Function(x) x.Value)

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()
            'トランザクション
            Dim SQLtrn As SqlClient.SqlTransaction = Nothing

            O_RTN = C_MESSAGE_NO.NORMAL

            'トランザクション開始
            'SQLtrn = SQLcon.BeginTransaction

            MC006UPDATE = New GRT00004COM.GRMC006UPDATE With {
                .SQLcon = SQLcon,
                .SQLtrn = SQLtrn,
                .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
                .UORG = work.WF_SEL_SHIPORG.Text,
                .UPDUSERID = Master.USERID,
                .UPDTERMID = Master.USERTERMID
            }
            MC006UPDATE.Update(lstDest, KOUEIMASTER)
            If Not isNormal(MC006UPDATE.ERR) Then
                Master.Output(MC006UPDATE.ERR, C_MESSAGE_TYPE.ABORT)
                Exit Sub
            End If
        End Using
    End Sub

    ''' <summary>
    ''' KOUEI CSV→T00004tbl処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub KOUEItoINPtbl(ByRef O_RTN As String)

        '■■■ エラーレポート準備 ■■■
        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 初期処理
        rightview.SetErrorReport("")

        '○T00004INPtblカラム設定
        Master.CreateEmptyTable(T00004INPtbl)

        Dim WW_ROWNUM As Integer = 0
        Dim WW_INDEX As Integer = 0

        '全オーダー取得
        Dim order = KOUEIMNG.GetOrder()
        'トリップ別にまとめる
        Dim tripAll = order.GroupBy((Function(x) x.Value.ORDERID))
        For Each trip In tripAll
            Dim orderId = trip.Key
            Dim rowStart As GRW0001KOUEIORDER.KOUEI_ORDER = Nothing             '始業レコード
            Dim rowShip = New List(Of GRW0001KOUEIORDER.KOUEI_ORDER)            '基地発レコード
            Dim rowTodoke = New List(Of GRW0001KOUEIORDER.KOUEI_ORDER)          '配送先レコード
            Dim rowKiko As GRW0001KOUEIORDER.KOUEI_ORDER = Nothing              '基地戻レコード
            Dim rowEnd As GRW0001KOUEIORDER.KOUEI_ORDER = Nothing               '終業レコード

            Dim tripStatus As Integer = 0

            'トリップ内回順判定
            For Each csv In trip
                Dim csvRow = csv.Value
                Select Case Int32.Parse(csvRow.TRIPSEQ)
                    Case 0 '始業
                        rowStart = csvRow
                    Case 99 '終業
                        rowEnd = csvRow
                    Case Else
                        If rowShip.Count = 0 AndAlso csvRow.OWNERINFO = "|基地" Then
                            '出荷
                            rowShip.Add(csvRow)
                        ElseIf csvRow.DEPOFLAG = "1" Then 'デポフラグ
                            '帰庫
                            rowKiko = csvRow
                        Else
                            '配送
                            rowTodoke.Add(csvRow)
                        End If
                End Select
            Next

            '回順シーケンス不備
            If IsNothing(rowStart) OrElse rowShip.Count = 0 OrElse rowTodoke.Count = 0 OrElse IsNothing(rowKiko) OrElse IsNothing(rowEnd) Then
                Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "光英及び及び石油輸送システム担当へ連絡")
                ErrMsgEditForKOUEI(rowStart, "回順", "回順シーケンス不備", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                tripStatus = -1
                Exit For
            End If

            Dim WW_SHUKODATE As Date
            Dim tmpCodeBaseKey As String = rowTodoke.First.COURSEBASEKEY
            Dim tmpShukoDate As String = tmpCodeBaseKey.Split(GRW0001KOUEIORDER.C_KOUEI_CSV_COLUMS_DELIMITER)(0)
            If Not DateTime.TryParseExact(tmpShukoDate, "yyyyMMdd", Nothing, Nothing, WW_SHUKODATE) Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ErrMsgEditForKOUEI(rowStart, "出庫日", "コードベースキー読込エラー", C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                tripStatus = -1
                Exit For
            End If
            '届先マスタ取得（出荷場所）
            Dim datShukabasho As TODOKESAKI = Nothing
            '光英届先マスタ取得
            Dim tmpShukabasho = KOUEIMASTER.GetTodoke(rowShip.First.KOUEITYPE, rowShip.First.DESTCODE)
            If Not IsNothing(tmpShukabasho) Then
                '光英マスタ存在

                Dim todokeCode As String = tmpShukabasho.GetDBEntryCode
                datShukabasho = GetTodoke(work.WF_SEL_SHIPORG.Text, todokeCode)
                If IsNothing(datShukabasho) Then
                    '届先マスタ未存在
                    '事前に登録処理が動くためこのケースはないハズ
                    datShukabasho = New TODOKESAKI With {
                        .TODOKECODE = "!" & todokeCode & "!",
                        .NAMES = "★マスタエラー★"
                    }
                    ErrMsgEditForKOUEI(rowStart, "出荷場所", "届先マスタ不備", C_MESSAGE_NO.BOX_ERROR_EXIST)
                    tripStatus = 1
                End If
            Else
                '光英マスタ未存在
                '新規届先発生時はこのパターン
                tmpShukabasho = New KOUEI_MASTER.KOUEI_TODOKE With {
                    .KOUEITYPE = rowShip.First.KOUEITYPE,
                    .TODOKESAKICODE = rowShip.First.DESTCODE
                }
                datShukabasho = New TODOKESAKI With {
                    .TODOKECODE = tmpShukabasho.GetDBEntryCode,
                    .NAMES = rowShip.First.DESTNAME
                }

                ErrMsgEditForKOUEI(rowStart, "出荷場所", "光英マスタ未存在", C_MESSAGE_NO.BOX_ERROR_EXIST)
                tripStatus = 1
            End If

            '**************************************** 光英受信時は従業員は一切入ってこないので処理不要 2019/09
            ''従業員マスタ取得（乗務員）
            'Dim datStaff As STAFF = Nothing
            'Dim tmpStaff = KOUEIMASTER.GetStaff2Code(rowStart.KOUEITYPE, rowStart.STAFFCODE)
            'If Not IsNothing(tmpStaff) Then
            '    '光英マスタ存在
            '    Dim staffCode As String = tmpStaff.STAFFNO
            '    datStaff = GetStaff(work.WF_SEL_SHIPORG.Text, staffCode)
            'Else
            '    '光英マスタ未存在
            '    tmpStaff = New KOUEI_MASTER.KOUEI_STAFF With {
            '        .STAFFCODE = rowStart.STAFFCODE
            '    }
            'End If

            ''従業員マスタ取得（副乗務員）
            'Dim datSubStaff As STAFF = Nothing
            'Dim tmpSubStaff = KOUEIMASTER.GetStaff2Code(rowStart.KOUEITYPE, rowStart.SUBSTAFFCODE)
            'If Not IsNothing(tmpSubStaff) Then
            '    '光英マスタ存在
            '    Dim staffCode As String = tmpSubStaff.STAFFNO
            '    datSubStaff = GetStaff(work.WF_SEL_SHIPORG.Text, staffCode)
            'Else
            '    '光英マスタ未存在
            '    tmpSubStaff = New KOUEI_MASTER.KOUEI_STAFF With {
            '        .STAFFCODE = rowStart.SUBSTAFFCODE
            '    }
            'End If

            Dim GSHABAN As String = String.Empty
            ''光英マスタ取得
            Dim tmpSharyo = KOUEIMASTER.GetSharyo(rowStart.KOUEITYPE, rowStart.SHABANB)
            If Not IsNothing(tmpSharyo) Then
                '光英マスタ存在
                Dim shaban = WF_ListKSHABAN.Items.FindByValue(tmpSharyo.REGISTERSHABAN)
                If Not IsNothing(shaban) Then
                    GSHABAN = shaban.Text
                Else
                    '車番部署マスタ未存在
                    GSHABAN = "!" & tmpSharyo.REGISTERSHABAN & "!"
                    ErrMsgEditForKOUEI(rowStart, "業務車番", "車番部署マスタ不備", C_MESSAGE_NO.BOX_ERROR_EXIST)
                    tripStatus = 1
                End If
            Else
                '光英マスタ未存在
                GSHABAN = "!" & rowStart.SHABANB & "!"
                ErrMsgEditForKOUEI(rowStart, "業務車番", "光英マスタ未存在", C_MESSAGE_NO.BOX_ERROR_EXIST)
                tripStatus = 1
            End If

            '配送先毎にドロップ編集
            For Each row In rowTodoke
                Dim WW_TODOKEDATE As Date
                Dim WW_SHUKADATE As Date
                Dim dropStatus As Integer = 0

                '積置コード
                Dim tmpTumiokiCode As String = row.FIELD(85)
                If String.IsNullOrEmpty(tmpTumiokiCode) Then
                    WW_TODOKEDATE = WW_SHUKODATE
                    WW_SHUKADATE = WW_SHUKODATE
                Else
                    '積置又は積配（荷卸）
                    Dim tmpTumiokiCode2 As String = tmpTumiokiCode.Split(GRW0001KOUEIORDER.C_KOUEI_CSV_COLUMS_DELIMITER)(0)
                    Dim tmpTumiFlg As String = Left(tmpTumiokiCode2, 1)
                    Dim tmpDate As String = Mid(tmpTumiokiCode2, 2)
                    Select Case tmpTumiFlg
                        Case "s"
                            If Not DateTime.TryParseExact(tmpDate, "yyyyMMdd", Nothing, Nothing, WW_SHUKADATE) Then
                                ErrMsgEditForKOUEI(rowStart, "出荷日", "出荷日の日付設定が正しくありません。", C_MESSAGE_NO.BOX_ERROR_EXIST)
                                dropStatus = -1
                            End If
                            WW_TODOKEDATE = WW_SHUKODATE
                        Case "t"
                            If Not DateTime.TryParseExact(tmpDate, "yyyyMMdd", Nothing, Nothing, WW_TODOKEDATE) Then
                                ErrMsgEditForKOUEI(rowStart, "届日", "届日の日付設定が正しくありません。", C_MESSAGE_NO.BOX_ERROR_EXIST)
                                dropStatus = -1
                            End If
                            WW_SHUKADATE = WW_SHUKODATE
                    End Select

                End If

                '届先マスタ取得（届先）
                Dim datTodoke As TODOKESAKI = Nothing
                '光英届先マスタ取得
                Dim tmpTodoke = KOUEIMASTER.GetTodoke(row.KOUEITYPE, row.DESTCODE)
                If Not IsNothing(tmpTodoke) Then
                    '光英マスタ存在

                    Dim todokeCode As String = tmpTodoke.GetDBEntryCode
                    datTodoke = GetTodoke(work.WF_SEL_SHIPORG.Text, todokeCode)
                    If IsNothing(datTodoke) Then
                        '届先マスタ未存在
                        '事前に登録処理が動くためこのケースはないハズ
                        datTodoke = New TODOKESAKI With {
                            .TODOKECODE = "!" & todokeCode & "!",
                            .NAMES = "★マスタエラー★"
                        }
                        ErrMsgEditForKOUEI(rowStart, "届先", "届先マスタ不備", C_MESSAGE_NO.BOX_ERROR_EXIST)
                        dropStatus = 1
                    End If
                Else
                    '光英マスタ未存在
                    '新規届先発生時はこのパターン
                    tmpTodoke = New KOUEI_MASTER.KOUEI_TODOKE With {
                        .KOUEITYPE = row.KOUEITYPE,
                        .TODOKESAKICODE = row.DESTCODE
                    }
                    datTodoke = New TODOKESAKI With {
                        .TODOKECODE = tmpTodoke.GetDBEntryCode,
                        .NAMES = row.DESTNAME
                    }
                    ErrMsgEditForKOUEI(rowStart, "届先", "光英マスタ未存在", C_MESSAGE_NO.BOX_ERROR_EXIST)
                    dropStatus = 1
                End If

                '１ドロップにおける複数品目がある場合は枝番としてレコード作成
                ' 品名コード
                Dim productArray() = row.PRODUCTCODE
                ' 品名別数量
                Dim numArray() = row.PRODUCTNUM
                ' 品名別名称
                Dim nameArray() = row.PRODUCTNAME
                '品名リスト作成（品名コード、数量）
                Dim productList = productArray.Zip(numArray, Function(PRODUCTCD, NUM) New With {PRODUCTCD, NUM})

                Dim seq As Integer = 0
                For Each kproduct In productList
                    Dim seqStatus As Integer = 0

                    Dim datProduct As PRODUCT = GetKProduct(work.WF_SEL_SHIPORG.Text, kproduct.PRODUCTCD)
                    If IsNothing(datProduct) Then
                        '品名マスタ未存在
                        datProduct = New PRODUCT With {
                            .OILTYPE = "01",
                            .PRODUCT1 = "11",
                            .PRODUCT2 = "",
                            .PRODUCTCODE = "!" & kproduct.PRODUCTCD & "!",
                            .NAMES = "★" & nameArray(seq) & "★"
                        }
                        ErrMsgEditForKOUEI(rowStart, "品名", "品名部署マスタ不備", C_MESSAGE_NO.BOX_ERROR_EXIST)
                        seqStatus = 1
                    End If

                    Dim T00004INProw = T00004INPtbl.NewRow
                    seq += 1

                    '***** T4項目順に編集 *****
                    T00004INProw("LINECNT") = 0
                    T00004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    T00004INProw("TIMSTP") = "0"
                    T00004INProw("SELECT") = 1
                    If seq = 1 Then
                        T00004INProw("HIDDEN") = 0
                    Else
                        T00004INProw("HIDDEN") = 1
                    End If
                    T00004INProw("INDEX") = WW_INDEX
                    WW_INDEX += WW_INDEX

                    T00004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    Dim KOUEITYPE As String = row.KOUEITYPE
                    If row.KOUEITYPE = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JX OrElse
                       row.KOUEITYPE = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.TG OrElse
                        row.KOUEITYPE = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.JXTG Then
                        T00004INProw("TORICODE") = GRT00004WRKINC.C_TORICODE_JX
                    ElseIf row.KOUEITYPE = GRW0001KOUEIORDER.KOUEITYPE_PREFIX.COSMO Then
                        T00004INProw("TORICODE") = GRT00004WRKINC.C_TORICODE_COSMO
                    End If

                    T00004INProw("OILTYPE") = datProduct.OILTYPE
                    T00004INProw("ORDERORG") = work.WF_SEL_ORDERORG.Text
                    If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                        T00004INProw("SHIPORG") = WF_DEFORG.Text
                    Else
                        T00004INProw("SHIPORG") = work.WF_SEL_SHIPORG.Text
                    End If

                    T00004INProw("KIJUNDATE") = ""                                      ' T3CTLから設定
                    T00004INProw("ORDERNO") = ""                                        ' 後続で受注番号自動設定
                    T00004INProw("DETAILNO") = "001"
                    T00004INProw("GSHABAN") = GSHABAN
                    T00004INProw("TRIPNO") = rowStart.TRIP.PadLeft(3, "0")
                    T00004INProw("DROPNO") = (Int32.Parse(row.TRIPSEQ) - 1).ToString("000")
                    T00004INProw("SEQ") = seq.ToString("00")


                    If IsNothing(WW_SHUKODATE) Then
                        T00004INProw("SHUKODATE") = ""
                    Else
                        T00004INProw("SHUKODATE") = WW_SHUKODATE.ToString("yyyy/MM/dd")
                    End If
                    T00004INProw("STATUS") = ""                                         ' 後続で設定

                    T00004INProw("KIKODATE") = ""
                    If IsNothing(WW_SHUKADATE) Then
                        T00004INProw("SHUKADATE") = ""
                    Else
                        T00004INProw("SHUKADATE") = WW_SHUKADATE.ToString("yyyy/MM/dd")
                    End If
                    If IsNothing(WW_TODOKEDATE) Then
                        T00004INProw("TODOKEDATE") = ""
                    Else
                        T00004INProw("TODOKEDATE") = WW_TODOKEDATE.ToString("yyyy/MM/dd")
                    End If

                    If WW_TODOKEDATE = WW_SHUKADATE Then
                        T00004INProw("TUMIOKIKBN") = ""
                    Else
                        '届日≠出庫日（出荷）
                        T00004INProw("TUMIOKIKBN") = "1"
                    End If

                    T00004INProw("SHUKABASHO") = datShukabasho.TODOKECODE

                    T00004INProw("GATE") = ""
                    T00004INProw("TUMIBA") = ""
                    T00004INProw("TUMISEQ") = ""
                    T00004INProw("SHUKADENNO") = ""
                    T00004INProw("INTIME") = ""
                    T00004INProw("OUTTIME") = ""
                    T00004INProw("STAFFCODE") = ""
                    T00004INProw("SUBSTAFFCODE") = ""
                    'If IsNothing(datStaff) Then
                    '    T00004INProw("STAFFCODE") = tmpStaff.STAFFCODE
                    'Else
                    '    T00004INProw("STAFFCODE") = datStaff.STAFFCODE
                    'End If
                    'If IsNothing(datSubStaff) Then
                    '    T00004INProw("SUBSTAFFCODE") = tmpSubStaff.STAFFCODE
                    'Else
                    '    T00004INProw("SUBSTAFFCODE") = datSubStaff.STAFFCODE
                    'End If
                    T00004INProw("STTIME") = ""
                    T00004INProw("RYOME") = "1"
                    T00004INProw("TODOKECODE") = datTodoke.TODOKECODE
                    T00004INProw("TODOKETIME") = ""
                    T00004INProw("PRODUCT1") = datProduct.PRODUCT1
                    T00004INProw("PRODUCT2") = datProduct.PRODUCT2
                    T00004INProw("PRODUCTCODE") = datProduct.PRODUCTCODE
                    T00004INProw("CONTNO") = ""
                    T00004INProw("PRATIO") = ""
                    T00004INProw("SMELLKBN") = ""
                    T00004INProw("SHAFUKU") = ""
                    T00004INProw("HTANI") = ""
                    T00004INProw("SURYO") = kproduct.NUM
                    T00004INProw("DAISU") = "1"
                    T00004INProw("JSURYO") = "0"
                    T00004INProw("JDAISU") = ""

                    If rowShip.Count = 1 Then
                        T00004INProw("REMARKS1") = ""
                        T00004INProw("REMARKS2") = ""
                        T00004INProw("REMARKS3") = row.MEMO1
                        T00004INProw("REMARKS4") = row.MEMO2
                        T00004INProw("REMARKS5") = row.JOINT
                        T00004INProw("REMARKS6") = ""
                    Else
                        '複数積場がある場合
                        For i = 1 To rowShip.Count - 1
                            '2件目以降の積場を備考に設定（最大2件）
                            '1件目は通常の出荷場所に設定されるので実質3積場まで対応
                            If i > 2 Then Exit For
                            T00004INProw("REMARKS" & i) = String.Format("積合出荷{0}({1})", i, rowShip(i).DESTNAME)
                        Next
                    End If
                    T00004INProw("TORIORDERNO") = ""
                    T00004INProw("STORICODE") = ""
                    T00004INProw("TERMORG") = WF_DEFORG.Text
                    ' T3CTLから設定
                    T00004INProw("URIKBN") = ""
                    T00004INProw("CONTCHASSIS") = ""
                    T00004INProw("SHARYOTYPEF") = ""
                    T00004INProw("TSHABANF") = ""
                    T00004INProw("SHARYOTYPEB") = ""
                    T00004INProw("TSHABANB") = ""
                    T00004INProw("SHARYOTYPEB2") = ""
                    T00004INProw("TSHABANB2") = ""
                    T00004INProw("TAXKBN") = "0"
                    T00004INProw("JXORDERID") = row.ORDERID
                    T00004INProw("DELFLG") = "0"

                    '光英オーダーから名称設定。後続でマスタから設定
                    '※マスタ未設定時用
                    T00004INProw("PRODUCTNAME") = datProduct.NAMES
                    T00004INProw("PRODUCT2NAME") = datProduct.NAMES
                    T00004INProw("TODOKECODENAME") = datTodoke.NAMES
                    T00004INProw("SHUKABASHONAME") = datShukabasho.NAMES
                    T00004INProw("GSHABANLICNPLTNO") = tmpSharyo.LICNPLTNO

                    If tripStatus = 0 AndAlso dropStatus = 0 AndAlso seqStatus = 0 Then
                        T00004INProw("JXORDERSTATUS") = ""
                    Else
                        T00004INProw("JXORDERSTATUS") = JXORDER_WARNING
                        dropStatus = -1
                    End If

                    'Grid追加明細（新規追加と同じ）とする
                    T00004INProw("WORK_NO") = ""

                    '○名称付与
                    CODENAME_set(T00004INProw)

                    '入力テーブル追加
                    T00004INPtbl.Rows.Add(T00004INProw)

                Next

                If dropStatus <> 0 Then
                    '明細で警告発生時は同一ドロップ全ての明細に反映
                    For Each seqRow As DataRow In T00004INPtbl.Rows
                        seqRow("JXORDERSTATUS") = JXORDER_WARNING
                    Next
                End If
            Next

            'トリップ初期化
            rowStart = Nothing
            rowShip = Nothing
            rowKiko = Nothing
            rowEnd = Nothing
            rowTodoke = Nothing

        Next

    End Sub

    ''' <summary>
    ''' エラーメッセージ編集（光英）
    ''' </summary>
    ''' <param name="order"></param>
    ''' <param name="I_FIELDNM"></param>
    ''' <param name="I_MSG"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Sub ErrMsgEditForKOUEI(ByRef order As GRW0001KOUEIORDER.KOUEI_ORDER, ByVal I_FIELDNM As String, I_MSG As String, ByVal I_ERRCD As String)
        'エラーレポート編集
        Dim WW_ERR_MES As String = ""
        Select Case I_ERRCD
            Case C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_ERR_MES = "更新できないレコード(" & I_FIELDNM & ")です。"
            Case C_MESSAGE_NO.BOX_ERROR_EXIST
                WW_ERR_MES = "・エラーが存在します。(" & I_FIELDNM & "エラー)"
        End Select

        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MSG & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & order.ROWNO & "行目 , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 号車   =" & order.SHABANCD & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 便番号 =" & order.TRIP & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 回順   =" & order.TRIPSEQ & "  "
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub
    ''' <summary>
    ''' JXオーダーステータス設定
    ''' </summary>
    ''' <remarks></remarks>
    Sub SetOrderStatus(ByRef T00004row As DataRow)
        Dim tmpOrderId As String = T00004row("JXORDERID")
        'JXオーダーかつ末尾に警告マーク付きの場合
        If tmpOrderId.EndsWith(JXORDER_WARNING) Then
            'JXオーダーステータスにマーク設定
            T00004row("JXORDERID") = tmpOrderId.Substring(0, tmpOrderId.Length - 1)
            T00004row("JXORDERSTATUS") = JXORDER_WARNING
        Else
            T00004row("JXORDERSTATUS") = ""
        End If

    End Sub

#End Region

#Region "その他DB関連"

    ''' <summary>
    ''' 届先データ
    ''' </summary>
    Public Class TODOKESAKI
        Public CAMPCODE As String
        Public UORG As String
        Public TORICODE As String
        Public TODOKECODE As String
        Public NAMES As String
        Public ADDR As String
        Public NOTES1 As String
        Public NOTES2 As String
        Public NOTES3 As String
        Public NOTES4 As String
        Public NOTES5 As String
        Public NOTES6 As String
        Public NOTES7 As String
        Public NOTES8 As String
        Public NOTES9 As String
        Public NOTES10 As String
        Public [CLASS] As String
        Public LATITUDE As String
        Public LONGITUDE As String
        Public ARRIVTIME As String
        Public DISTANCE As String

        Public JSRTODOKECODE As String
        Public SHUKABASHO As String

        Public Function MakeDicKey() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append(CAMPCODE)
            sb.Append(C_VALUE_SPLIT_DELIMITER)
            sb.Append(UORG)
            sb.Append(C_VALUE_SPLIT_DELIMITER)
            sb.Append(TODOKECODE)
            Return sb.ToString
        End Function
    End Class

    ''' <summary>
    ''' 届先データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Function GetTodoke(ByVal I_ORG As String, ByVal I_TODOKECODE As String) As TODOKESAKI
        Dim wkValue As TODOKESAKI = Nothing
        Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, work.WF_SEL_CAMPCODE.Text, I_ORG, I_TODOKECODE)

        If IsNothing(TODOKESAKItbl) Then
            TODOKESAKItbl = New Dictionary(Of String, TODOKESAKI)
        End If
        If Not TODOKESAKItbl.TryGetValue(wkKey, wkValue) Then
            Try
                'DataBase接続文字
                Using SQLcon = CS0050SESSION.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim sb As StringBuilder = New StringBuilder()
                    sb.Append("Select ")
                    sb.Append("  rtrim(A.TORICODE) As TORICODE ")
                    sb.Append("  , rtrim(A.TODOKECODE) As TODOKECODE ")
                    sb.Append("  , rtrim(A.NAMES) As NAMES ")
                    sb.Append("  , rtrim(A.ADDR1) + rtrim(A.ADDR2) + rtrim(A.ADDR3) + rtrim(A.ADDR4) As ADDR ")
                    sb.Append("  , rtrim(A.NOTES1) As NOTES1 ")
                    sb.Append("  , rtrim(A.NOTES2) As NOTES2 ")
                    sb.Append("  , rtrim(A.NOTES3) As NOTES3 ")
                    sb.Append("  , rtrim(A.NOTES4) As NOTES4 ")
                    sb.Append("  , rtrim(A.NOTES5) As NOTES5 ")
                    sb.Append("  , rtrim(A.NOTES6) As NOTES6 ")
                    sb.Append("  , rtrim(A.NOTES7) As NOTES7 ")
                    sb.Append("  , rtrim(A.NOTES8) As NOTES8 ")
                    sb.Append("  , rtrim(A.NOTES9) As NOTES9 ")
                    sb.Append("  , rtrim(A.NOTES10) As NOTES10 ")
                    sb.Append("  , rtrim(A.LATITUDE) As LATITUDE ")
                    sb.Append("  , rtrim(A.LONGITUDE) As LONGITUDE ")
                    sb.Append("  , rtrim(A.Class) As Class ")
                    sb.Append("  , rtrim(B.ARRIVTIME) As ARRIVTIME ")
                    sb.Append("  , rtrim(B.DISTANCE) As DISTANCE ")
                    sb.Append("FROM ")
                    sb.Append("  MC006_TODOKESAKI A ")
                    sb.Append("  INNER JOIN MC007_TODKORG B ")
                    sb.Append("     On B.CAMPCODE = A.CAMPCODE ")
                    sb.Append("    And B.TORICODE = A.TORICODE ")
                    sb.Append("    And B.TODOKECODE = A.TODOKECODE ")
                    sb.Append("    And B.UORG = @ORG ")
                    sb.Append("    And B.DELFLG <> @DELFLG ")
                    sb.Append("Where ")
                    sb.Append("      A.CAMPCODE = @CAMPCODE ")
                    sb.Append("  And A.STYMD <= @STYMD ")
                    sb.Append(" And A.ENDYMD >= @ENDYMD ")
                    sb.Append("  And A.DELFLG <> @DELFLG ")
                    'sb.Append("  And A.TODOKECODE = @TODOKECODE ") -- 一括取得

                    Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                    Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                    Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                    Dim TODOKECODE As SqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", System.Data.SqlDbType.NVarChar)
                    CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                    STYMD.Value = Date.Now
                    ENDYMD.Value = Date.Now
                    DELFLG.Value = C_DELETE_FLG.DELETE
                    ORG.Value = I_ORG
                    TODOKECODE.Value = I_TODOKECODE

                    '○SQL実行
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○出力設定
                    While SQLdr.Read
                        wkValue = New TODOKESAKI With {
                            .CAMPCODE = work.WF_SEL_CAMPCODE.Text,
                            .UORG = I_ORG,
                            .TORICODE = SQLdr("TORICODE"),
                            .TODOKECODE = SQLdr("TODOKECODE"),
                            .NAMES = SQLdr("NAMES"),
                            .ADDR = SQLdr("ADDR"),
                            .NOTES1 = SQLdr("NOTES1"),
                            .NOTES2 = SQLdr("NOTES2"),
                            .NOTES3 = SQLdr("NOTES3"),
                            .NOTES4 = SQLdr("NOTES4"),
                            .NOTES5 = SQLdr("NOTES5"),
                            .NOTES6 = SQLdr("NOTES6"),
                            .NOTES7 = SQLdr("NOTES7"),
                            .NOTES8 = SQLdr("NOTES8"),
                            .NOTES9 = SQLdr("NOTES9"),
                            .NOTES10 = SQLdr("NOTES10"),
                            .LATITUDE = SQLdr("LATITUDE"),
                            .LONGITUDE = SQLdr("LONGITUDE"),
                            .CLASS = SQLdr("CLASS"),
                            .ARRIVTIME = SQLdr("ARRIVTIME"),
                            .DISTANCE = SQLdr("DISTANCE")
                        }

                        TODOKESAKItbl.Item(wkValue.MakeDicKey()) = wkValue
                        'TODOKESAKItbl.Add(tmpKey, wkValue) -- 一括取得のため
                    End While

                    TODOKESAKItbl.TryGetValue(wkKey, wkValue)

                    'Close()
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "TODOKESAKI Select")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DBTODOKESAKI Select"           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Return Nothing
            End Try

        End If

        Return wkValue

    End Function

    ''' <summary>
    ''' 品名データ
    ''' </summary>
    Public Class PRODUCT
        Public CAMPCODE As String
        Public UORG As String
        Public PRODUCTCODE As String
        Public OILTYPE As String
        Public PRODUCT1 As String
        Public PRODUCT2 As String
        Public NAMES As String
        Public HTANI As String                      '配送単位
        Public KPRODUCT As String                   '光英車端用品名コード

        Public Function MakeDicKey() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append(CAMPCODE)
            sb.Append(C_VALUE_SPLIT_DELIMITER)
            sb.Append(UORG)
            sb.Append(C_VALUE_SPLIT_DELIMITER)
            sb.Append(PRODUCTCODE)
            Return sb.ToString
        End Function

    End Class

    ''' <summary>
    ''' 品名データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Function InitProduct(ByVal I_CAMPCODE As String, ByVal I_ORG As String) As Boolean

        If IsNothing(PRODUCTtbl) Then
            PRODUCTtbl = New Dictionary(Of String, PRODUCT)
        End If
        Try
            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim sb As StringBuilder = New StringBuilder()
                sb.Append("Select ")
                sb.Append("  rtrim(A.PRODUCTCODE) As PRODUCTCODE ")
                sb.Append("  , rtrim(A.OILTYPE) As OILTYPE ")
                sb.Append("  , rtrim(A.PRODUCT1) As PRODUCT1 ")
                sb.Append("  , rtrim(A.PRODUCT2) As PRODUCT2 ")
                sb.Append("  , rtrim(A.NAMES) As NAMES ")
                sb.Append("  , rtrim(B.HTANI) As HTANI ")
                sb.Append("  , rtrim(B.KPRODUCT) As KPRODUCT ")
                sb.Append("FROM ")
                sb.Append("  MD001_PRODUCT A ")
                sb.Append("  INNER JOIN MD002_PRODORG B ")
                sb.Append("    On B.PRODUCTCODE = A.PRODUCTCODE ")
                sb.Append("    And B.CAMPCODE = A.CAMPCODE ")
                sb.Append("    And B.UORG = @ORG ")
                sb.Append("    And B.STYMD <= @STYMD ")
                sb.Append(" And B.ENDYMD >= @ENDYMD ")
                sb.Append("    And B.DELFLG <> @DELFLG ")
                sb.Append("WHERE ")
                sb.Append("  A.CAMPCODE = @CAMPCODE ")
                sb.Append("  And A.STYMD <= @STYMD ")
                sb.Append(" And A.ENDYMD >= @ENDYMD ")
                sb.Append("  And A.DELFLG <> @DELFLG ")

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                CAMPCODE.Value = I_CAMPCODE
                STYMD.Value = Date.Now
                ENDYMD.Value = Date.Now
                DELFLG.Value = C_DELETE_FLG.DELETE
                ORG.Value = I_ORG

                '○SQL実行
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力設定
                While SQLdr.Read
                    Dim wkValue = New PRODUCT With {
                        .CAMPCODE = I_CAMPCODE,
                        .UORG = I_ORG,
                        .PRODUCTCODE = SQLdr("PRODUCTCODE"),
                        .OILTYPE = SQLdr("OILTYPE"),
                        .PRODUCT1 = SQLdr("PRODUCT1"),
                        .PRODUCT2 = SQLdr("PRODUCT2"),
                        .NAMES = SQLdr("NAMES"),
                        .HTANI = SQLdr("HTANI"),
                        .KPRODUCT = SQLdr("KPRODUCT")
                    }
                    PRODUCTtbl.Item(wkValue.MakeDicKey) = wkValue
                End While

                'Close()
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "PRODUCT Select")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DBPRODUCT Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return False
        End Try

        Return True

    End Function
    ''' <summary>
    ''' 品名データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Function GetProduct(ByVal I_ORG As String, ByVal I_PRODUCTCODE As String) As PRODUCT
        Dim wkValue As PRODUCT = Nothing
        Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, work.WF_SEL_CAMPCODE.Text, I_ORG, I_PRODUCTCODE)

        If IsNothing(PRODUCTtbl) Then
            If InitProduct(work.WF_SEL_CAMPCODE.Text, I_ORG) <> True Then
                Return Nothing
            End If
        End If

        PRODUCTtbl.TryGetValue(wkKey, wkValue)

        Return wkValue

    End Function

    ''' <summary>
    ''' 光英品名コードDictionary
    ''' </summary>
    Dim KPRODUCTtbl As Dictionary(Of String, PRODUCT)
    ''' <summary>
    ''' 品名データ取得（光英）
    ''' </summary>
    ''' <remarks></remarks>
    Private Function GetKProduct(ByVal I_ORG As String, ByVal I_CODE As String) As PRODUCT
        Dim wkValue As PRODUCT = Nothing
        Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, work.WF_SEL_CAMPCODE.Text, I_ORG, I_CODE)

        If IsNothing(PRODUCTtbl) Then
            If InitProduct(work.WF_SEL_CAMPCODE.Text, I_ORG) <> True Then
                Return Nothing
            End If
        End If

        If IsNothing(KPRODUCTtbl) Then
            KPRODUCTtbl = PRODUCTtbl.Values _
            .Where(Function(x) x.KPRODUCT <> "" And x.CAMPCODE = work.WF_SEL_CAMPCODE.Text And x.UORG = I_ORG) _
            .GroupBy(Function(x As PRODUCT) x.KPRODUCT) _
            .Select(Function(x) x.First()) _
            .ToDictionary(Function(x) String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, x.CAMPCODE, x.UORG, x.KPRODUCT))
        End If

        KPRODUCTtbl.TryGetValue(wkKey, wkValue)

        Return wkValue

    End Function

    ''' <summary>
    ''' 乗務員データ
    ''' </summary>
    Public Class STAFF
        Public STAFFCODE As String
        Public STAFFNAMES As String
        Public NOTES1 As String
        Public NOTES2 As String
        Public NOTES3 As String
        Public NOTES4 As String
        Public NOTES5 As String
    End Class

    ''' <summary>
    ''' 乗務員データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Function GetStaff(ByVal I_ORG As String, ByVal I_STAFFCODE As String) As STAFF
        Dim wkValue As STAFF = Nothing
        Dim wkKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, work.WF_SEL_CAMPCODE.Text, I_ORG, I_STAFFCODE)

        If IsNothing(STAFFtbl) Then
            STAFFtbl = New Dictionary(Of String, STAFF)
        End If
        If Not STAFFtbl.TryGetValue(wkKey, wkValue) Then
            Try
                'DataBase接続文字
                Using SQLcon = CS0050SESSION.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    Dim sb As StringBuilder = New StringBuilder()
                    sb.Append("Select ")
                    sb.Append("  isnull(rtrim(A.STAFFCODE), '') as STAFFCODE ")
                    sb.Append("  , isnull(rtrim(A.STAFFNAMES), '') as STAFFNAMES ")
                sb.Append("  , isnull(rtrim(A.NOTES1), '') as NOTES1 ")
                sb.Append("  , isnull(rtrim(A.NOTES2), '') as NOTES2 ")
                sb.Append("  , isnull(rtrim(A.NOTES3), '') as NOTES3 ")
                sb.Append("  , isnull(rtrim(A.NOTES4), '') as NOTES4 ")
                sb.Append("  , isnull(rtrim(A.NOTES5), '') as NOTES5 ")
                sb.Append("FROM ")
                sb.Append("  MB001_STAFF A ")
                sb.Append("  INNER JOIN MB002_STAFFORG B ")
                sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
                sb.Append("    and B.STAFFCODE = A.STAFFCODE ")
                sb.Append("    and B.SORG = @ORG ")
                sb.Append("    and B.DELFLG <> @DELFLG ")
                sb.Append("Where ")
                sb.Append("  A.CAMPCODE = @CAMPCODE ")
                sb.Append("  and A.STYMD <= @STYMD ")
                sb.Append(" and A.ENDYMD >= @ENDYMD ")
                sb.Append("  and A.DELFLG <> @DELFLG ")
                sb.Append("ORDER BY ")
                sb.Append("  B.SEQ ")
                sb.Append("  , A.STAFFCODE ")

                Dim SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
                Dim STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
                Dim ORG As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
                CAMPCODE.Value = work.WF_SEL_CAMPCODE.Text
                STYMD.Value = Date.Now
                ENDYMD.Value = Date.Now
                DELFLG.Value = C_DELETE_FLG.DELETE
                ORG.Value = I_ORG

                '○SQL実行
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力設定
                While SQLdr.Read
                    wkValue = New STAFF With {
                            .STAFFCODE = SQLdr("STAFFCODE"),
                            .STAFFNAMES = SQLdr("STAFFNAMES"),
                            .NOTES1 = SQLdr("NOTES1"),
                            .NOTES2 = SQLdr("NOTES2"),
                            .NOTES3 = SQLdr("NOTES3"),
                            .NOTES4 = SQLdr("NOTES4"),
                            .NOTES5 = SQLdr("NOTES5")
                        }

                    Dim tmpKey As String = String.Format("{1}{0}{2}{0}{3}", C_VALUE_SPLIT_DELIMITER, work.WF_SEL_CAMPCODE.Text, I_ORG, wkValue.STAFFCODE)

                    STAFFtbl(tmpKey) = wkValue
                End While

                If STAFFtbl.TryGetValue(wkKey, wkValue) Then
                End If

                'Close()
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
                End Using

                Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "STAFF SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:STAFF Select"           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Return Nothing
                End Try

            End If

            Return wkValue

    End Function

    ''' <summary>
    ''' 請求先取得   
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="O_STORICODE">請求先コード</param>
    ''' <param name="O_NAMES">請求先名称</param>
    ''' <remarks></remarks>
    Protected Sub GetSTori(ByVal I_TORICODE As String, ByRef O_STORICODE As String, ByRef O_NAMES As String)

        O_STORICODE = ""
        O_NAMES = ""

        If String.IsNullOrEmpty(I_TORICODE) Then
            Exit Sub
        End If

        Try

            'DataBase接続文字
            Using SQLcon = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                      "       SELECT rtrim(A.STORICODE)  as STORICODE ,       " _
                    & "              rtrim(B.NAMES) 	as NAMES 		    " _
                    & "         FROM MC003_TORIORG      as A 			    " _
                    & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                    & "           ON B.CAMPCODE   	= A.CAMPCODE 		    " _
                    & "          and B.TORICODE   	= A.STORICODE 		    " _
                    & "          and B.STYMD       <= @P1                   " _
                    & " and B.ENDYMD      >= @P1 				    " _
                    & "          and B.DELFLG      <> '1' 				    " _
                    & "        Where A.CAMPCODE     = @P2 				    " _
                    & "          and A.UORG     	= @P3 				    " _
                    & "          and A.TORICODE 	= @P4 				    " _
                    & "          and A.DELFLG      <> '1' 				    "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
                PARA1.Value = Date.Now
                PARA2.Value = work.WF_SEL_CAMPCODE.Text
                If String.IsNullOrEmpty(work.WF_SEL_SHIPORG.Text) Then
                    PARA3.Value = WF_DEFORG.Text
                Else
                    PARA3.Value = work.WF_SEL_SHIPORG.Text
                End If
                PARA4.Value = I_TORICODE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                While SQLdr.Read
                    '○出力編集
                    O_STORICODE = SQLdr("STORICODE")
                    O_NAMES = SQLdr("NAMES")
                End While

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC002_TORIHIKISAKI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "GetSTori"                          'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                     '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

    End Sub

#End Region

End Class






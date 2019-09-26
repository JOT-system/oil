Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRMA0003SHARYOB
    Inherits Page

    '検索結果格納ds
    Private MA0003tbl As DataTable                              'Grid格納用テーブル
    Private MA0003INPtbl As DataTable                           'Detail入力用テーブル
    Private MA0003PDFtbl As DataTable                           'Repeater格納用テーブル
    Private MA003_SHARYOBtbl As DataTable                       '更新用テーブル

    '共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                'プロファイル（GridView）設定
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'UPLOAD_XLSデータ取得
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(APサーバチェックなし)
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0050Session As New CS0050SESSION                  'セッション管理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                 'リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID
    Private Const CONST_MAX_TABID As Integer = 10               '詳細タブ数

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Not Master.RecoverTable(MA0003tbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonPrint"
                            WF_Print_Click()
                        Case "WF_ButtonFIRST"
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"
                            WF_ButtonLAST_Click()
                        Case "WF_UPDATE"
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"
                            WF_CLEAR_Click()
                        Case "WF_ButtonEND"
                            WF_ButtonEND_Click()
                        Case "WF_ButtonSel"
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"
                            WF_ButtonCan_Click()
                        Case "WF_Field_DBClick"
                            WF_Field_DBClick()
                        Case "WF_ListboxDBclick" '○LeftBox処理（ListBoxダブルクリック時）
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick" '○RightBox処理（ラジオボタン選択）
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange" '○RightBox処理（右Boxメモ変更時）
                            WF_MEMO_Change()
                        Case "WF_GridDBclick"
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown" '○スクロール処理
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp" '○スクロール処理
                            WF_GRID_ScroleUp()
                        Case "WF_EXCEL_UPLOAD" '○ファイルアップロード処理
                            UPLOAD_EXCEL()
                        Case "WF_DTAB_Click" '○DetailTab切替処理
                            WF_Detail_TABChange()
                            TAB_DisplayCTRL(WF_SHARYOTYPE.Text)
                        Case "WF_DTAB_PDF_Change"
                            PDF_SELECTchange()
                        Case "WF_DTAB_PDF_Click"
                            DTAB_PDFdisplay()
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
            If Not IsNothing(MA0003tbl) Then
                MA0003tbl.Clear()
                MA0003tbl.Dispose()
                MA0003tbl = Nothing
            End If

            If Not IsNothing(MA0003INPtbl) Then
                MA0003INPtbl.Clear()
                MA0003INPtbl.Dispose()
                MA0003INPtbl = Nothing
            End If

            If Not IsNothing(MA0003INPtbl) Then
                MA0003PDFtbl.Clear()
                MA0003PDFtbl.Dispose()
                MA0003PDFtbl = Nothing
            End If

            If Not IsNothing(MA003_SHARYOBtbl) Then
                MA003_SHARYOBtbl.Clear()
                MA003_SHARYOBtbl.Dispose()
                MA003_SHARYOBtbl = Nothing
            End If
        End Try

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRMA0003WRKINC.MAPID
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_SELSHARYOTYPE.Focus()
        rightview.resetindex()
        leftview.activeListBox()
        WF_DetailMView.ActiveViewIndex = 0
        MAPrefelence()
        '○ヘルプ無
        Master.dispHelp = False
        '○ドラックアンドドロップON
        Master.eventDrop = True
        '○画面編集１

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        'MAP-Detail値表示項目設定
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        '○画面編集２
        Using TBLview As DataView = New DataView(MA0003tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRMA0003WRKINC.MAPID
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

        '詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()
        TAB_DisplayCTRL(WF_SHARYOTYPE.Text)
    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For Each MA0003row As DataRow In MA0003tbl.Rows
            If MA0003row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MA0003row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MA0003tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()

        '一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRMA0003WRKINC.MAPID
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
        WF_SELSHARYOTYPE.Focus()

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○入力値チェック
        Dim WW_TEXT As String = ""
        Dim WW_ERR As String = ""

        '車両タイプ
        If WF_SELSHARYOTYPE.Text <> "" Then
            CODENAME_get("SHARYOTYPE", Left(WF_SELSHARYOTYPE.Text, 1), WF_SELSHARYOTYPE_TEXT.Text, WW_RTN_SW)
        End If

        '管理部署名
        If WF_SELMORG.Text <> "" Then
            CODENAME_get("MANGMORG", WF_SELMORG.Text, WF_SELMORG_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, WF_SELMORG.Text)
                Exit Sub
            End If
        End If

        '○絞り込み操作（GridView明細Hidden設定）
        For Each MA0003row As DataRow In MA0003tbl.Rows

            MA0003row("HIDDEN") = 1

            '車両タイプ・管理組織　絞込判定
            If WF_SELSHARYOTYPE.Text = "" AndAlso WF_SELMORG.Text = "" Then
                MA0003row("HIDDEN") = 0
            End If

            If WF_SELSHARYOTYPE.Text <> "" AndAlso WF_SELMORG.Text = "" Then
                If MA0003row("SHARYOTYPE") & MA0003row("TSHABAN") Like WF_SELSHARYOTYPE.Text & "*" Then
                    MA0003row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHARYOTYPE.Text = "" AndAlso WF_SELMORG.Text <> "" Then
                If MA0003row("MANGMORG") = WF_SELMORG.Text Then
                    MA0003row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHARYOTYPE.Text <> "" AndAlso WF_SELMORG.Text <> "" Then
                If MA0003row("SHARYOTYPE") & MA0003row("TSHABAN") Like WF_SELSHARYOTYPE.Text & "*" AndAlso MA0003row("MANGMORG") = WF_SELMORG.Text Then
                    MA0003row("HIDDEN") = 0
                End If
            End If
        Next

        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_SELSHARYOTYPE.Focus()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○初期値設定
        Dim WW_TIMSTP As Long = 0
        Dim WW_UPDCNT As Integer = 0

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        '○日付歯抜けチェック
        If isNormal(WW_ERRCODE) Then
            DATE_RELATION_CHK(WW_ERRCODE)
        End If

        If isNormal(WW_ERRCODE) Then
            Try
                '○ジャーナル用データ
                Master.CreateEmptyTable(MA003_SHARYOBtbl)
                'メッセージ初期化
                rightview.setErrorReport("")

                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open()       'DataBase接続(Open)

                    Dim MA0002INS As String =
                          "    INSERT INTO MA002_SHARYOA " _
                        & "             (CAMPCODE , " _
                        & "              MANGMORG , " _
                        & "              MANGSORG , " _
                        & "              SHARYOTYPE , " _
                        & "              TSHABAN , " _
                        & "              STYMD , " _
                        & "              ENDYMD , " _
                        & "              MANGOILTYPE , " _
                        & "              MANGPROD1 , " _
                        & "              MANGPROD2 , " _
                        & "              MANGSHAFUKU , " _
                        & "              MANGOWNCODE , " _
                        & "              MANGOWNCONT , " _
                        & "              MANGSUPPL , " _
                        & "              MANGTTLDIST , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER , " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD , " _
                        & "              BASELEASE , " _
                        & "              SHARYOSTATUS ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10," _
                        & "              @P11,@P12,@P13,@P14,@P15,     @P17,@P18,@P19,@P20," _
                        & "              @P21,@P22,@P23,@P24);"

                    Dim MA003MRG As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu " _
                        & "     FROM MA003_SHARYOB " _
                        & "     WHERE    CAMPCODE     = @P01 " _
                        & "       and    SHARYOTYPE   = @P02 " _
                        & "       and    TSHABAN      = @P03 " _
                        & "       and    STYMD        = @P04 ; " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE MA003_SHARYOB " _
                        & "       SET    ENDYMD = @P05 , " _
                        & "              BASERDATE = @P06 , " _
                        & "              BASERAGEYY = @P07 , " _
                        & "              BASERAGEMM = @P08 , " _
                        & "              BASERAGE = @P09 , " _
                        & "              BASELEASE = @P10 , " _
                        & "              FCTRFUELCAPA = @P11 , " _
                        & "              FCTRFUELMATE = @P12 , " _
                        & "              FCTRTMISSION = @P13 , " _
                        & "              FCTRUREA = @P14 , " _
                        & "              FCTRTIRE = @P15 , " _
                        & "              FCTRDPR = @P16 , " _
                        & "              FCTRAXLE = @P17 , " _
                        & "              FCTRSUSP = @P18 , " _
                        & "              FCTRSHFTNUM = @P19 , " _
                        & "              FCTRSMAKER = @P20 , " _
                        & "              FCTRTMAKER = @P21 , " _
                        & "              FCTRRESERVE1 = @P22 , " _
                        & "              FCTRRESERVE2 = @P23 , " _
                        & "              FCTRRESERVE3 = @P24 , " _
                        & "              FCTRRESERVE4 = @P25 , " _
                        & "              FCTRRESERVE5 = @P26 , " _
                        & "              OTNKCELLNO = @P27 , " _
                        & "              OTNKCELPART = @P28 , " _
                        & "              OTNKMATERIAL = @P29 , " _
                        & "              OTNKLVALVE = @P30 , " _
                        & "              OTNKPUMP = @P31 , " _
                        & "              OTNKPIPE = @P32 , " _
                        & "              OTNKBPIPE = @P33 , " _
                        & "              OTNKHTECH = @P34 , " _
                        & "              OTNKDCD = @P35 , " _
                        & "              OTNKDETECTOR = @P36 , " _
                        & "              OTNKPIPESIZE = @P37 , " _
                        & "              OTNKEXHASIZE = @P38 , " _
                        & "              OTNKDISGORGE = @P39 , " _
                        & "              OTNKVAPOR = @P40 , " _
                        & "              OTNKCVALVE = @P41 , " _
                        & "              OTNKTINSNO = @P42 , " _
                        & "              OTNKINSYMD = @P43 , " _
                        & "              OTNKINSSTAT = @P44 , " _
                        & "              HPRSMATR = @P45 , " _
                        & "              HPRSSTRUCT = @P46 , " _
                        & "              HPRSVALVE = @P47 , " _
                        & "              HPRSPIPE = @P48 , " _
                        & "              HPRSPUMP = @P49 , " _
                        & "              HPRSPMPDR = @P50 , " _
                        & "              HPRSRESSRE = @P51 , " _
                        & "              HPRSPIPENUM = @P52 , " _
                        & "              HPRSINSULATE = @P53 , " _
                        & "              HPRSSERNO = @P54 , " _
                        & "              HPRSINSIYMD = @P55 , " _
                        & "              HPRSINSISTAT = @P56 , " _
                        & "              HPRSHOSE = @P57 , " _
                        & "              CHEMCELLNO = @P58 , " _
                        & "              CHEMCELPART = @P59 , " _
                        & "              CHEMMATERIAL = @P60 , " _
                        & "              CHEMSTRUCT = @P61 , " _
                        & "              CHEMPUMP = @P62 , " _
                        & "              CHEMPMPDR = @P63 , " _
                        & "              CHEMDISGORGE = @P64 , " _
                        & "              CHEMPRESDRV = @P65 , " _
                        & "              CHEMTHERM = @P66 , " _
                        & "              CHEMMANOMTR = @P67 , " _
                        & "              CHEMHOSE = @P68 , " _
                        & "              CHEMPRESEQ = @P69 , " _
                        & "              CHEMTINSNO = @P70 , " _
                        & "              CHEMINSYMD = @P71 , " _
                        & "              CHEMINSSTAT = @P72 , " _
                        & "              CONTSHAPE = @P73 , " _
                        & "              CONTPUMP = @P74 , " _
                        & "              CONTPMPDR = @P75 , " _
                        & "              OTHRTERMINAL = @P76 , " _
                        & "              OTHRBMONITOR = @P77 , " _
                        & "              OTHRRADIOCON = @P78 , " _
                        & "              OTHRDOCO = @P79 , " _
                        & "              OTHRPAINTING = @P80 , " _
                        & "              OTHRDRRECORD = @P81 , " _
                        & "              OTHRBSONAR = @P82 , " _
                        & "              OTHRRTARGET = @P83 , " _
                        & "              OTHRTIRE1 = @P84 , " _
                        & "              OTHRTIRE2 = @P85 , " _
                        & "              OTHRTPMS = @P86 , " _
                        & "              OFFCRESERVE1 = @P87 , " _
                        & "              OFFCRESERVE2 = @P88 , " _
                        & "              OFFCRESERVE3 = @P89 , " _
                        & "              OFFCRESERVE4 = @P90 , " _
                        & "              OFFCRESERVE5 = @P91 , " _
                        & "              ACCTRCYCLE = @P92 , " _
                        & "              ACCTLEASE1 = @P93 , " _
                        & "              ACCTLEASE2 = @P94 , " _
                        & "              ACCTLEASE3 = @P95 , " _
                        & "              ACCTLEASE4 = @P96 , " _
                        & "              ACCTLEASE5 = @P97 , " _
                        & "              ACCTLSUPL1 = @P98 , " _
                        & "              ACCTLSUPL2 = @P99 , " _
                        & "              ACCTLSUPL3 = @P100 , " _
                        & "              ACCTLSUPL4 = @P101 , " _
                        & "              ACCTLSUPL5 = @P102 , " _
                        & "              ACCTASST01 = @P103 , " _
                        & "              ACCTASST02 = @P104 , " _
                        & "              ACCTASST03 = @P105 , " _
                        & "              ACCTASST04 = @P106 , " _
                        & "              ACCTASST05 = @P107 , " _
                        & "              ACCTASST06 = @P108 , " _
                        & "              ACCTASST07 = @P109 , " _
                        & "              ACCTASST08 = @P110 , " _
                        & "              ACCTASST09 = @P111 , " _
                        & "              ACCTASST10 = @P112 , " _
                        & "              NOTES = @P113 , " _
                        & "              DELFLG = @P114 , " _
                        & "              UPDYMD = @P116 , " _
                        & "              UPDUSER = @P117 , " _
                        & "              UPDTERMID    = @P118 , " _
                        & "              RECEIVEYMD   = @P119 , " _
                        & "              OTNKTMAKER   = @P120 , " _
                        & "              HPRSTMAKER   = @P121 , " _
                        & "              CHEMTMAKER   = @P122 , " _
                        & "              CONTTMAKER   = @P123  " _
                        & "     WHERE    CAMPCODE       = @P01 " _
                        & "       and    SHARYOTYPE        = @P02 " _
                        & "       and    TSHABAN     = @P03 " _
                        & "       and    STYMD        = @P04 ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO MA003_SHARYOB " _
                        & "             (CAMPCODE , " _
                        & "              SHARYOTYPE , " _
                        & "              TSHABAN , " _
                        & "              STYMD , " _
                        & "              ENDYMD , " _
                        & "              BASERDATE , " _
                        & "              BASERAGEYY , " _
                        & "              BASERAGEMM , " _
                        & "              BASERAGE , " _
                        & "              BASELEASE , " _
                        & "              FCTRFUELCAPA , " _
                        & "              FCTRFUELMATE , " _
                        & "              FCTRTMISSION , " _
                        & "              FCTRUREA , " _
                        & "              FCTRTIRE , " _
                        & "              FCTRDPR , " _
                        & "              FCTRAXLE , " _
                        & "              FCTRSUSP , " _
                        & "              FCTRSHFTNUM , " _
                        & "              FCTRSMAKER , " _
                        & "              FCTRTMAKER , " _
                        & "              FCTRRESERVE1 , " _
                        & "              FCTRRESERVE2 , " _
                        & "              FCTRRESERVE3 , " _
                        & "              FCTRRESERVE4 , " _
                        & "              FCTRRESERVE5 , " _
                        & "              OTNKCELLNO , " _
                        & "              OTNKCELPART , " _
                        & "              OTNKMATERIAL , " _
                        & "              OTNKLVALVE , " _
                        & "              OTNKPUMP , " _
                        & "              OTNKPIPE , " _
                        & "              OTNKBPIPE , " _
                        & "              OTNKHTECH , " _
                        & "              OTNKDCD , " _
                        & "              OTNKDETECTOR , " _
                        & "              OTNKPIPESIZE , " _
                        & "              OTNKEXHASIZE , " _
                        & "              OTNKDISGORGE , " _
                        & "              OTNKVAPOR , " _
                        & "              OTNKCVALVE , " _
                        & "              OTNKTINSNO , " _
                        & "              OTNKINSYMD , " _
                        & "              OTNKINSSTAT , " _
                        & "              HPRSMATR , " _
                        & "              HPRSSTRUCT , " _
                        & "              HPRSVALVE , " _
                        & "              HPRSPIPE , " _
                        & "              HPRSPUMP , " _
                        & "              HPRSPMPDR , " _
                        & "              HPRSRESSRE , " _
                        & "              HPRSPIPENUM , " _
                        & "              HPRSINSULATE , " _
                        & "              HPRSSERNO , " _
                        & "              HPRSINSIYMD , " _
                        & "              HPRSINSISTAT , " _
                        & "              HPRSHOSE , " _
                        & "              CHEMCELLNO , " _
                        & "              CHEMCELPART , " _
                        & "              CHEMMATERIAL , " _
                        & "              CHEMSTRUCT , " _
                        & "              CHEMPUMP , " _
                        & "              CHEMPMPDR , " _
                        & "              CHEMDISGORGE , " _
                        & "              CHEMPRESDRV , " _
                        & "              CHEMTHERM , " _
                        & "              CHEMMANOMTR , " _
                        & "              CHEMHOSE , " _
                        & "              CHEMPRESEQ , " _
                        & "              CHEMTINSNO , " _
                        & "              CHEMINSYMD , " _
                        & "              CHEMINSSTAT , " _
                        & "              CONTSHAPE , " _
                        & "              CONTPUMP , " _
                        & "              CONTPMPDR , " _
                        & "              OTHRTERMINAL , " _
                        & "              OTHRBMONITOR , " _
                        & "              OTHRRADIOCON , " _
                        & "              OTHRDOCO , " _
                        & "              OTHRPAINTING , " _
                        & "              OTHRDRRECORD , " _
                        & "              OTHRBSONAR , " _
                        & "              OTHRRTARGET , " _
                        & "              OTHRTIRE1 , " _
                        & "              OTHRTIRE2 , " _
                        & "              OTHRTPMS , " _
                        & "              OFFCRESERVE1 , " _
                        & "              OFFCRESERVE2 , " _
                        & "              OFFCRESERVE3 , " _
                        & "              OFFCRESERVE4 , " _
                        & "              OFFCRESERVE5 , " _
                        & "              ACCTRCYCLE , " _
                        & "              ACCTLEASE1 , " _
                        & "              ACCTLEASE2 , " _
                        & "              ACCTLEASE3 , " _
                        & "              ACCTLEASE4 , " _
                        & "              ACCTLEASE5 , " _
                        & "              ACCTLSUPL1 , " _
                        & "              ACCTLSUPL2 , " _
                        & "              ACCTLSUPL3 , " _
                        & "              ACCTLSUPL4 , " _
                        & "              ACCTLSUPL5 , " _
                        & "              ACCTASST01 , " _
                        & "              ACCTASST02 , " _
                        & "              ACCTASST03 , " _
                        & "              ACCTASST04 , " _
                        & "              ACCTASST05 , " _
                        & "              ACCTASST06 , " _
                        & "              ACCTASST07 , " _
                        & "              ACCTASST08 , " _
                        & "              ACCTASST09 , " _
                        & "              ACCTASST10 , " _
                        & "              NOTES , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER , " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD , " _
                        & "              OTNKTMAKER , " _
                        & "              HPRSTMAKER , " _
                        & "              CHEMTMAKER , " _
                        & "              CONTTMAKER ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10," _
                        & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20," _
                        & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30," _
                        & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39,@P40," _
                        & "              @P41,@P42,@P43,@P44,@P45,@P46,@P47,@P48,@P49,@P50," _
                        & "              @P51,@P52,@P53,@P54,@P55,@P56,@P57,@P58,@P59,@P60," _
                        & "              @P61,@P62,@P63,@P64,@P65,@P66,@P67,@P68,@P69,@P70," _
                        & "              @P71,@P72,@P73,@P74,@P75,@P76,@P77,@P78,@P79,@P80," _
                        & "              @P81,@P82,@P83,@P84,@P85,@P86,@P87,@P88,@P89,@P90," _
                        & "              @P91,@P92,@P93,@P94,@P95,@P96,@P97,@P98,@P99,@P100," _
                        & "              @P101,@P102,@P103,@P104,@P105,@P106,@P107,@P108,@P109,@P110," _
                        & "              @P111,@P112,@P113,@P114,@P115,@P116,@P117,@P118,@P119,@P120," _
                        & "              @P121,@P122,@P123);" _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

                    Dim MA0003_TMSTP_UPD As String =
                          " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP " _
                        & "     FROM MA003_SHARYOB " _
                        & "     WHERE    CAMPCODE       = @P01 " _
                        & "       and    SHARYOTYPE     = @P02 " _
                        & "       and    TSHABAN        = @P03 " _
                        & "       and    STYMD          = @P04 ; "

                    Using SQLcmd2 As New SqlCommand(MA0002INS, SQLcon),
                        SQLcmd3 As New SqlCommand(MA003MRG, SQLcon),
                        SQLcmd5 As New SqlCommand(MA0003_TMSTP_UPD, SQLcon)

                        Dim PARA201 As SqlParameter = SQLcmd2.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA202 As SqlParameter = SQLcmd2.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                        Dim PARA203 As SqlParameter = SQLcmd2.Parameters.Add("@P03", SqlDbType.NVarChar, 20)
                        Dim PARA204 As SqlParameter = SQLcmd2.Parameters.Add("@P04", SqlDbType.NVarChar, 1)
                        Dim PARA205 As SqlParameter = SQLcmd2.Parameters.Add("@P05", SqlDbType.NVarChar, 19)
                        Dim PARA206 As SqlParameter = SQLcmd2.Parameters.Add("@P06", SqlDbType.DateTime)
                        Dim PARA207 As SqlParameter = SQLcmd2.Parameters.Add("@P07", SqlDbType.DateTime)
                        Dim PARA208 As SqlParameter = SQLcmd2.Parameters.Add("@P08", SqlDbType.NVarChar, 3)
                        Dim PARA209 As SqlParameter = SQLcmd2.Parameters.Add("@P09", SqlDbType.NVarChar, 20)
                        Dim PARA210 As SqlParameter = SQLcmd2.Parameters.Add("@P10", SqlDbType.NVarChar, 20)
                        Dim PARA211 As SqlParameter = SQLcmd2.Parameters.Add("@P11", SqlDbType.Float)
                        Dim PARA212 As SqlParameter = SQLcmd2.Parameters.Add("@P12", SqlDbType.NVarChar, 20)
                        Dim PARA213 As SqlParameter = SQLcmd2.Parameters.Add("@P13", SqlDbType.NVarChar, 20)
                        Dim PARA214 As SqlParameter = SQLcmd2.Parameters.Add("@P14", SqlDbType.NVarChar, 20)
                        Dim PARA215 As SqlParameter = SQLcmd2.Parameters.Add("@P15", SqlDbType.Int)
                        Dim PARA217 As SqlParameter = SQLcmd2.Parameters.Add("@P17", SqlDbType.NVarChar, 1)
                        Dim PARA218 As SqlParameter = SQLcmd2.Parameters.Add("@P18", SqlDbType.DateTime)
                        Dim PARA219 As SqlParameter = SQLcmd2.Parameters.Add("@P19", SqlDbType.DateTime)
                        Dim PARA220 As SqlParameter = SQLcmd2.Parameters.Add("@P20", SqlDbType.Char, 20)
                        Dim PARA221 As SqlParameter = SQLcmd2.Parameters.Add("@P21", SqlDbType.Char, 30)
                        Dim PARA222 As SqlParameter = SQLcmd2.Parameters.Add("@P22", SqlDbType.DateTime)
                        Dim PARA223 As SqlParameter = SQLcmd2.Parameters.Add("@P23", SqlDbType.Char, 20)
                        Dim PARA224 As SqlParameter = SQLcmd2.Parameters.Add("@P24", SqlDbType.Char, 1)

                        Dim PARA301 As SqlParameter = SQLcmd3.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA302 As SqlParameter = SQLcmd3.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                        Dim PARA303 As SqlParameter = SQLcmd3.Parameters.Add("@P03", SqlDbType.NVarChar, 19)
                        Dim PARA304 As SqlParameter = SQLcmd3.Parameters.Add("@P04", SqlDbType.DateTime)
                        Dim PARA305 As SqlParameter = SQLcmd3.Parameters.Add("@P05", SqlDbType.DateTime)
                        Dim PARA306 As SqlParameter = SQLcmd3.Parameters.Add("@P06", SqlDbType.NVarChar, 20)
                        Dim PARA307 As SqlParameter = SQLcmd3.Parameters.Add("@P07", SqlDbType.Int)
                        Dim PARA308 As SqlParameter = SQLcmd3.Parameters.Add("@P08", SqlDbType.Int)
                        Dim PARA309 As SqlParameter = SQLcmd3.Parameters.Add("@P09", SqlDbType.Int)
                        Dim PARA310 As SqlParameter = SQLcmd3.Parameters.Add("@P10", SqlDbType.NVarChar, 3)
                        Dim PARA311 As SqlParameter = SQLcmd3.Parameters.Add("@P11", SqlDbType.BigInt)
                        Dim PARA312 As SqlParameter = SQLcmd3.Parameters.Add("@P12", SqlDbType.NVarChar, 20)
                        Dim PARA313 As SqlParameter = SQLcmd3.Parameters.Add("@P13", SqlDbType.NVarChar, 20)
                        Dim PARA314 As SqlParameter = SQLcmd3.Parameters.Add("@P14", SqlDbType.NVarChar, 20)
                        Dim PARA315 As SqlParameter = SQLcmd3.Parameters.Add("@P15", SqlDbType.NVarChar, 20)
                        Dim PARA316 As SqlParameter = SQLcmd3.Parameters.Add("@P16", SqlDbType.NVarChar, 20)
                        Dim PARA317 As SqlParameter = SQLcmd3.Parameters.Add("@P17", SqlDbType.NVarChar, 20)
                        Dim PARA318 As SqlParameter = SQLcmd3.Parameters.Add("@P18", SqlDbType.NVarChar, 20)
                        Dim PARA319 As SqlParameter = SQLcmd3.Parameters.Add("@P19", SqlDbType.NVarChar, 20)
                        Dim PARA320 As SqlParameter = SQLcmd3.Parameters.Add("@P20", SqlDbType.NVarChar, 20)
                        Dim PARA321 As SqlParameter = SQLcmd3.Parameters.Add("@P21", SqlDbType.NVarChar, 20)
                        Dim PARA322 As SqlParameter = SQLcmd3.Parameters.Add("@P22", SqlDbType.NVarChar, 20)
                        Dim PARA323 As SqlParameter = SQLcmd3.Parameters.Add("@P23", SqlDbType.NVarChar, 20)
                        Dim PARA324 As SqlParameter = SQLcmd3.Parameters.Add("@P24", SqlDbType.NVarChar, 20)
                        Dim PARA325 As SqlParameter = SQLcmd3.Parameters.Add("@P25", SqlDbType.NVarChar, 20)
                        Dim PARA326 As SqlParameter = SQLcmd3.Parameters.Add("@P26", SqlDbType.NVarChar, 20)
                        Dim PARA327 As SqlParameter = SQLcmd3.Parameters.Add("@P27", SqlDbType.NVarChar, 20)
                        Dim PARA328 As SqlParameter = SQLcmd3.Parameters.Add("@P28", SqlDbType.NVarChar, 20)
                        Dim PARA329 As SqlParameter = SQLcmd3.Parameters.Add("@P29", SqlDbType.NVarChar, 20)
                        Dim PARA330 As SqlParameter = SQLcmd3.Parameters.Add("@P30", SqlDbType.NVarChar, 20)
                        Dim PARA331 As SqlParameter = SQLcmd3.Parameters.Add("@P31", SqlDbType.NVarChar, 20)
                        Dim PARA332 As SqlParameter = SQLcmd3.Parameters.Add("@P32", SqlDbType.NVarChar, 20)
                        Dim PARA333 As SqlParameter = SQLcmd3.Parameters.Add("@P33", SqlDbType.NVarChar, 20)
                        Dim PARA334 As SqlParameter = SQLcmd3.Parameters.Add("@P34", SqlDbType.NVarChar, 20)
                        Dim PARA335 As SqlParameter = SQLcmd3.Parameters.Add("@P35", SqlDbType.NVarChar, 20)
                        Dim PARA336 As SqlParameter = SQLcmd3.Parameters.Add("@P36", SqlDbType.NVarChar, 20)
                        Dim PARA337 As SqlParameter = SQLcmd3.Parameters.Add("@P37", SqlDbType.NVarChar, 20)
                        Dim PARA338 As SqlParameter = SQLcmd3.Parameters.Add("@P38", SqlDbType.NVarChar, 20)
                        Dim PARA339 As SqlParameter = SQLcmd3.Parameters.Add("@P39", SqlDbType.NVarChar, 20)
                        Dim PARA340 As SqlParameter = SQLcmd3.Parameters.Add("@P40", SqlDbType.NVarChar, 20)
                        Dim PARA341 As SqlParameter = SQLcmd3.Parameters.Add("@P41", SqlDbType.NVarChar, 20)
                        Dim PARA342 As SqlParameter = SQLcmd3.Parameters.Add("@P42", SqlDbType.NVarChar, 20)
                        Dim PARA343 As SqlParameter = SQLcmd3.Parameters.Add("@P43", SqlDbType.DateTime)
                        Dim PARA344 As SqlParameter = SQLcmd3.Parameters.Add("@P44", SqlDbType.NVarChar, 20)
                        Dim PARA345 As SqlParameter = SQLcmd3.Parameters.Add("@P45", SqlDbType.NVarChar, 20)
                        Dim PARA346 As SqlParameter = SQLcmd3.Parameters.Add("@P46", SqlDbType.NVarChar, 20)
                        Dim PARA347 As SqlParameter = SQLcmd3.Parameters.Add("@P47", SqlDbType.NVarChar, 20)
                        Dim PARA348 As SqlParameter = SQLcmd3.Parameters.Add("@P48", SqlDbType.NVarChar, 20)
                        Dim PARA349 As SqlParameter = SQLcmd3.Parameters.Add("@P49", SqlDbType.NVarChar, 20)
                        Dim PARA350 As SqlParameter = SQLcmd3.Parameters.Add("@P50", SqlDbType.NVarChar, 20)
                        Dim PARA351 As SqlParameter = SQLcmd3.Parameters.Add("@P51", SqlDbType.NVarChar, 20)
                        Dim PARA352 As SqlParameter = SQLcmd3.Parameters.Add("@P52", SqlDbType.NVarChar, 20)
                        Dim PARA353 As SqlParameter = SQLcmd3.Parameters.Add("@P53", SqlDbType.NVarChar, 20)
                        Dim PARA354 As SqlParameter = SQLcmd3.Parameters.Add("@P54", SqlDbType.NVarChar, 20)
                        Dim PARA355 As SqlParameter = SQLcmd3.Parameters.Add("@P55", SqlDbType.DateTime)
                        Dim PARA356 As SqlParameter = SQLcmd3.Parameters.Add("@P56", SqlDbType.NVarChar, 20)
                        Dim PARA357 As SqlParameter = SQLcmd3.Parameters.Add("@P57", SqlDbType.NVarChar, 20)
                        Dim PARA358 As SqlParameter = SQLcmd3.Parameters.Add("@P58", SqlDbType.NVarChar, 20)
                        Dim PARA359 As SqlParameter = SQLcmd3.Parameters.Add("@P59", SqlDbType.NVarChar, 20)
                        Dim PARA360 As SqlParameter = SQLcmd3.Parameters.Add("@P60", SqlDbType.NVarChar, 20)
                        Dim PARA361 As SqlParameter = SQLcmd3.Parameters.Add("@P61", SqlDbType.NVarChar, 20)
                        Dim PARA362 As SqlParameter = SQLcmd3.Parameters.Add("@P62", SqlDbType.NVarChar, 20)
                        Dim PARA363 As SqlParameter = SQLcmd3.Parameters.Add("@P63", SqlDbType.NVarChar, 20)
                        Dim PARA364 As SqlParameter = SQLcmd3.Parameters.Add("@P64", SqlDbType.NVarChar, 20)
                        Dim PARA365 As SqlParameter = SQLcmd3.Parameters.Add("@P65", SqlDbType.NVarChar, 20)
                        Dim PARA366 As SqlParameter = SQLcmd3.Parameters.Add("@P66", SqlDbType.NVarChar, 20)
                        Dim PARA367 As SqlParameter = SQLcmd3.Parameters.Add("@P67", SqlDbType.NVarChar, 20)
                        Dim PARA368 As SqlParameter = SQLcmd3.Parameters.Add("@P68", SqlDbType.NVarChar, 20)
                        Dim PARA369 As SqlParameter = SQLcmd3.Parameters.Add("@P69", SqlDbType.NVarChar, 20)
                        Dim PARA370 As SqlParameter = SQLcmd3.Parameters.Add("@P70", SqlDbType.NVarChar, 20)
                        Dim PARA371 As SqlParameter = SQLcmd3.Parameters.Add("@P71", SqlDbType.DateTime)
                        Dim PARA372 As SqlParameter = SQLcmd3.Parameters.Add("@P72", SqlDbType.NVarChar, 20)
                        Dim PARA373 As SqlParameter = SQLcmd3.Parameters.Add("@P73", SqlDbType.NVarChar, 20)
                        Dim PARA374 As SqlParameter = SQLcmd3.Parameters.Add("@P74", SqlDbType.NVarChar, 20)
                        Dim PARA375 As SqlParameter = SQLcmd3.Parameters.Add("@P75", SqlDbType.NVarChar, 20)
                        Dim PARA376 As SqlParameter = SQLcmd3.Parameters.Add("@P76", SqlDbType.NVarChar, 20)
                        Dim PARA377 As SqlParameter = SQLcmd3.Parameters.Add("@P77", SqlDbType.NVarChar, 20)
                        Dim PARA378 As SqlParameter = SQLcmd3.Parameters.Add("@P78", SqlDbType.NVarChar, 20)
                        Dim PARA379 As SqlParameter = SQLcmd3.Parameters.Add("@P79", SqlDbType.NVarChar, 20)
                        Dim PARA380 As SqlParameter = SQLcmd3.Parameters.Add("@P80", SqlDbType.NVarChar, 20)
                        Dim PARA381 As SqlParameter = SQLcmd3.Parameters.Add("@P81", SqlDbType.NVarChar, 20)
                        Dim PARA382 As SqlParameter = SQLcmd3.Parameters.Add("@P82", SqlDbType.NVarChar, 20)
                        Dim PARA383 As SqlParameter = SQLcmd3.Parameters.Add("@P83", SqlDbType.NVarChar, 20)
                        Dim PARA384 As SqlParameter = SQLcmd3.Parameters.Add("@P84", SqlDbType.NVarChar, 20)
                        Dim PARA385 As SqlParameter = SQLcmd3.Parameters.Add("@P85", SqlDbType.NVarChar, 20)
                        Dim PARA386 As SqlParameter = SQLcmd3.Parameters.Add("@P86", SqlDbType.NVarChar, 20)
                        Dim PARA387 As SqlParameter = SQLcmd3.Parameters.Add("@P87", SqlDbType.NVarChar, 20)
                        Dim PARA388 As SqlParameter = SQLcmd3.Parameters.Add("@P88", SqlDbType.NVarChar, 20)
                        Dim PARA389 As SqlParameter = SQLcmd3.Parameters.Add("@P89", SqlDbType.NVarChar, 20)
                        Dim PARA390 As SqlParameter = SQLcmd3.Parameters.Add("@P90", SqlDbType.NVarChar, 20)
                        Dim PARA391 As SqlParameter = SQLcmd3.Parameters.Add("@P91", SqlDbType.NVarChar, 20)
                        Dim PARA392 As SqlParameter = SQLcmd3.Parameters.Add("@P92", SqlDbType.Money)
                        Dim PARA393 As SqlParameter = SQLcmd3.Parameters.Add("@P93", SqlDbType.NVarChar, 20)
                        Dim PARA394 As SqlParameter = SQLcmd3.Parameters.Add("@P94", SqlDbType.NVarChar, 20)
                        Dim PARA395 As SqlParameter = SQLcmd3.Parameters.Add("@P95", SqlDbType.NVarChar, 20)
                        Dim PARA396 As SqlParameter = SQLcmd3.Parameters.Add("@P96", SqlDbType.NVarChar, 20)
                        Dim PARA397 As SqlParameter = SQLcmd3.Parameters.Add("@P97", SqlDbType.NVarChar, 20)
                        Dim PARA398 As SqlParameter = SQLcmd3.Parameters.Add("@P98", SqlDbType.NVarChar, 20)
                        Dim PARA399 As SqlParameter = SQLcmd3.Parameters.Add("@P99", SqlDbType.NVarChar, 20)
                        Dim PARA3100 As SqlParameter = SQLcmd3.Parameters.Add("@P100", SqlDbType.NVarChar, 20)
                        Dim PARA3101 As SqlParameter = SQLcmd3.Parameters.Add("@P101", SqlDbType.NVarChar, 20)
                        Dim PARA3102 As SqlParameter = SQLcmd3.Parameters.Add("@P102", SqlDbType.NVarChar, 20)
                        Dim PARA3103 As SqlParameter = SQLcmd3.Parameters.Add("@P103", SqlDbType.NVarChar, 20)
                        Dim PARA3104 As SqlParameter = SQLcmd3.Parameters.Add("@P104", SqlDbType.NVarChar, 20)
                        Dim PARA3105 As SqlParameter = SQLcmd3.Parameters.Add("@P105", SqlDbType.NVarChar, 20)
                        Dim PARA3106 As SqlParameter = SQLcmd3.Parameters.Add("@P106", SqlDbType.NVarChar, 20)
                        Dim PARA3107 As SqlParameter = SQLcmd3.Parameters.Add("@P107", SqlDbType.NVarChar, 20)
                        Dim PARA3108 As SqlParameter = SQLcmd3.Parameters.Add("@P108", SqlDbType.NVarChar, 20)
                        Dim PARA3109 As SqlParameter = SQLcmd3.Parameters.Add("@P109", SqlDbType.NVarChar, 20)
                        Dim PARA3110 As SqlParameter = SQLcmd3.Parameters.Add("@P110", SqlDbType.NVarChar, 20)
                        Dim PARA3111 As SqlParameter = SQLcmd3.Parameters.Add("@P111", SqlDbType.NVarChar, 20)
                        Dim PARA3112 As SqlParameter = SQLcmd3.Parameters.Add("@P112", SqlDbType.NVarChar, 20)
                        Dim PARA3113 As SqlParameter = SQLcmd3.Parameters.Add("@P113", SqlDbType.NVarChar, 500)
                        Dim PARA3114 As SqlParameter = SQLcmd3.Parameters.Add("@P114", SqlDbType.NVarChar, 1)
                        Dim PARA3115 As SqlParameter = SQLcmd3.Parameters.Add("@P115", SqlDbType.DateTime)
                        Dim PARA3116 As SqlParameter = SQLcmd3.Parameters.Add("@P116", SqlDbType.DateTime)
                        Dim PARA3117 As SqlParameter = SQLcmd3.Parameters.Add("@P117", SqlDbType.NVarChar, 20)
                        Dim PARA3118 As SqlParameter = SQLcmd3.Parameters.Add("@P118", SqlDbType.NVarChar, 30)
                        Dim PARA3119 As SqlParameter = SQLcmd3.Parameters.Add("@P119", SqlDbType.DateTime)
                        Dim PARA3120 As SqlParameter = SQLcmd3.Parameters.Add("@P120", SqlDbType.NVarChar, 20)
                        Dim PARA3121 As SqlParameter = SQLcmd3.Parameters.Add("@P121", SqlDbType.NVarChar, 20)
                        Dim PARA3122 As SqlParameter = SQLcmd3.Parameters.Add("@P122", SqlDbType.NVarChar, 20)
                        Dim PARA3123 As SqlParameter = SQLcmd3.Parameters.Add("@P123", SqlDbType.NVarChar, 20)

                        Dim PARA501 As SqlParameter = SQLcmd5.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA502 As SqlParameter = SQLcmd5.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                        Dim PARA503 As SqlParameter = SQLcmd5.Parameters.Add("@P03", SqlDbType.NVarChar, 19)
                        Dim PARA504 As SqlParameter = SQLcmd5.Parameters.Add("@P04", SqlDbType.Date)

                        Dim WW_DATENOW As DateTime = Date.Now

                        '○登録更新の実処理
                        For Each MA0003row As DataRow In MA0003tbl.Rows
                            If MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                                MA0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                                '削除は更新しない
                                If MA0003row("DELFLG") = C_DELETE_FLG.DELETE AndAlso
                                    MA0003row("TIMSTP") = "0" Then
                                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                    Continue For
                                End If

                                '○車両番号の採番（新規）
                                If RTrim(MA0003row("TSHABAN")).StartsWith("新") Then
                                    '車両番号を採番する
                                    Dim CS0033 As New CS0033AutoNumber
                                    CS0033.CAMPCODE = work.WF_SEL_CAMPCODE.Text
                                    CS0033.SEQTYPE = RTrim(MA0003row("SHARYOTYPE"))
                                    CS0033.USERID = Master.USERID
                                    CS0033.getAutoNumber()

                                    If isNormal(CS0033.ERR) Then
                                        Dim WW_TSHABAN As String = CS0033.SEQ

                                        '○新規の場合、車両管轄マスタ(MA2)を新規追加しておく

                                        PARA201.Value = MA0003row("CAMPCODE")
                                        PARA202.Value = MA0003row("MANGMORG")
                                        PARA203.Value = MA0003row("MANGSORG")
                                        PARA204.Value = MA0003row("SHARYOTYPE")
                                        PARA205.Value = WW_TSHABAN
                                        PARA206.Value = RTrim(MA0003row("STYMD"))
                                        PARA207.Value = RTrim(MA0003row("ENDYMD"))
                                        PARA208.Value = MA0003row("MANGOILTYPE")
                                        PARA209.Value = MA0003row("MANGPROD1")
                                        PARA210.Value = MA0003row("MANGPROD2")
                                        Try
                                            PARA211.Value = CType(MA0003row("MANGSHAFUKU"), Double)
                                        Catch ex As Exception
                                            PARA211.Value = "0"
                                        End Try
                                        PARA212.Value = MA0003row("MANGOWNCODE")
                                        PARA213.Value = MA0003row("MANGOWNCONT")
                                        PARA214.Value = MA0003row("MANGSUPPL")
                                        Try
                                            Integer.TryParse(MA0003row("MANGTTLDIST"), PARA215.Value)
                                        Catch ex As Exception
                                            PARA215.Value = "0"
                                        End Try
                                        PARA217.Value = MA0003row("DELFLG")
                                        PARA218.Value = WW_DATENOW
                                        PARA219.Value = WW_DATENOW
                                        PARA220.Value = Master.USERID
                                        PARA221.Value = Master.USERTERMID
                                        PARA222.Value = C_DEFAULT_YMD
                                        PARA223.Value = MA0003row("BASELEASE")
                                        PARA224.Value = MA0003row("SHARYOSTATUS")

                                        SQLcmd2.ExecuteNonQuery()
                                        '○統一車番を設定する
                                        MA0003row("TSHABAN") = WW_TSHABAN
                                    Else
                                        Throw New Exception("他の端末と競合し、車両NOが採番できませんでした")
                                        Exit Sub
                                    End If
                                End If
                                '○新規および更新の場合、車両情報マスタ(MA3)を更新する

                                PARA301.Value = MA0003row("CAMPCODE")
                                PARA302.Value = MA0003row("SHARYOTYPE")
                                PARA303.Value = MA0003row("TSHABAN")
                                PARA304.Value = RTrim(MA0003row("STYMD"))
                                PARA305.Value = RTrim(MA0003row("ENDYMD"))
                                PARA306.Value = MA0003row("BASERDATE")
                                PARA307.Value = MA0003row("BASERAGEYY")
                                PARA308.Value = MA0003row("BASERAGEMM")
                                PARA309.Value = MA0003row("BASERAGE")
                                PARA310.Value = MA0003row("BASELEASE")
                                If RTrim(MA0003row("FCTRFUELCAPA")) = "" Then
                                    PARA311.Value = 0
                                Else
                                    PARA311.Value = RTrim(MA0003row("FCTRFUELCAPA"))
                                End If
                                PARA312.Value = MA0003row("FCTRFUELMATE")
                                PARA313.Value = MA0003row("FCTRTMISSION")
                                PARA314.Value = MA0003row("FCTRUREA")
                                PARA315.Value = MA0003row("FCTRTIRE")
                                PARA316.Value = MA0003row("FCTRDPR")
                                PARA317.Value = MA0003row("FCTRAXLE")
                                PARA318.Value = MA0003row("FCTRSUSP")
                                PARA319.Value = MA0003row("FCTRSHFTNUM")
                                PARA320.Value = MA0003row("FCTRSMAKER")
                                PARA321.Value = MA0003row("FCTRTMAKER")
                                PARA322.Value = MA0003row("FCTRRESERVE1")
                                PARA323.Value = MA0003row("FCTRRESERVE2")
                                PARA324.Value = MA0003row("FCTRRESERVE3")
                                PARA325.Value = MA0003row("FCTRRESERVE4")
                                PARA326.Value = MA0003row("FCTRRESERVE5")
                                PARA327.Value = MA0003row("OTNKCELLNO")
                                PARA328.Value = MA0003row("OTNKCELPART")
                                PARA329.Value = MA0003row("OTNKMATERIAL")
                                PARA330.Value = MA0003row("OTNKLVALVE")
                                PARA331.Value = MA0003row("OTNKPUMP")
                                PARA332.Value = MA0003row("OTNKPIPE")
                                PARA333.Value = MA0003row("OTNKBPIPE")
                                PARA334.Value = MA0003row("OTNKHTECH")
                                PARA335.Value = MA0003row("OTNKDCD")
                                PARA336.Value = MA0003row("OTNKDETECTOR")
                                PARA337.Value = MA0003row("OTNKPIPESIZE")
                                PARA338.Value = MA0003row("OTNKEXHASIZE")
                                PARA339.Value = MA0003row("OTNKDISGORGE")
                                PARA340.Value = MA0003row("OTNKVAPOR")
                                PARA341.Value = MA0003row("OTNKCVALVE")
                                PARA342.Value = MA0003row("OTNKTINSNO")
                                If RTrim(MA0003row("OTNKINSYMD")) = "" Then
                                    PARA343.Value = C_DEFAULT_YMD
                                Else
                                    PARA343.Value = RTrim(MA0003row("OTNKINSYMD"))
                                End If
                                PARA344.Value = MA0003row("OTNKINSSTAT")
                                PARA345.Value = MA0003row("HPRSMATR")
                                PARA346.Value = MA0003row("HPRSSTRUCT")
                                PARA347.Value = MA0003row("HPRSVALVE")
                                PARA348.Value = MA0003row("HPRSPIPE")
                                PARA349.Value = MA0003row("HPRSPUMP")
                                PARA350.Value = MA0003row("HPRSPMPDR")
                                PARA351.Value = MA0003row("HPRSRESSRE")
                                PARA352.Value = MA0003row("HPRSPIPENUM")
                                PARA353.Value = MA0003row("HPRSINSULATE")
                                PARA354.Value = MA0003row("HPRSSERNO")
                                If RTrim(MA0003row("HPRSINSIYMD")) = "" Then
                                    PARA355.Value = C_DEFAULT_YMD
                                Else
                                    PARA355.Value = RTrim(MA0003row("HPRSINSIYMD"))
                                End If
                                PARA356.Value = MA0003row("HPRSINSISTAT")
                                PARA357.Value = MA0003row("HPRSHOSE")
                                PARA358.Value = MA0003row("CHEMCELLNO")
                                PARA359.Value = MA0003row("CHEMCELPART")
                                PARA360.Value = MA0003row("CHEMMATERIAL")
                                PARA361.Value = MA0003row("CHEMSTRUCT")
                                PARA362.Value = MA0003row("CHEMPUMP")
                                PARA363.Value = MA0003row("CHEMPMPDR")
                                PARA364.Value = MA0003row("CHEMDISGORGE")
                                PARA365.Value = MA0003row("CHEMPRESDRV")
                                PARA366.Value = MA0003row("CHEMTHERM")
                                PARA367.Value = MA0003row("CHEMMANOMTR")
                                PARA368.Value = MA0003row("CHEMHOSE")
                                PARA369.Value = MA0003row("CHEMPRESEQ")
                                PARA370.Value = MA0003row("CHEMTINSNO")
                                If RTrim(MA0003row("CHEMINSYMD")) = "" Then
                                    PARA371.Value = C_DEFAULT_YMD
                                Else
                                    PARA371.Value = RTrim(MA0003row("CHEMINSYMD"))
                                End If
                                PARA372.Value = MA0003row("CHEMINSSTAT")
                                PARA373.Value = MA0003row("CONTSHAPE")
                                PARA374.Value = MA0003row("CONTPUMP")
                                PARA375.Value = MA0003row("CONTPMPDR")
                                PARA376.Value = MA0003row("OTHRTERMINAL")
                                PARA377.Value = MA0003row("OTHRBMONITOR")
                                PARA378.Value = MA0003row("OTHRRADIOCON")
                                PARA379.Value = MA0003row("OTHRDOCO")
                                PARA380.Value = MA0003row("OTHRPAINTING")
                                PARA381.Value = MA0003row("OTHRDRRECORD")
                                PARA382.Value = MA0003row("OTHRBSONAR")
                                PARA383.Value = MA0003row("OTHRRTARGET")
                                PARA384.Value = MA0003row("OTHRTIRE1")
                                PARA385.Value = MA0003row("OTHRTIRE2")
                                PARA386.Value = MA0003row("OTHRTPMS")
                                PARA387.Value = MA0003row("OFFCRESERVE1")
                                PARA388.Value = MA0003row("OFFCRESERVE2")
                                PARA389.Value = MA0003row("OFFCRESERVE3")
                                PARA390.Value = MA0003row("OFFCRESERVE4")
                                PARA391.Value = MA0003row("OFFCRESERVE5")
                                If RTrim(MA0003row("ACCTRCYCLE")) = "" Then
                                    PARA392.Value = 0
                                Else
                                    PARA392.Value = MA0003row("ACCTRCYCLE")
                                End If
                                PARA393.Value = MA0003row("ACCTLEASE1")
                                PARA394.Value = MA0003row("ACCTLEASE2")
                                PARA395.Value = MA0003row("ACCTLEASE3")
                                PARA396.Value = MA0003row("ACCTLEASE4")
                                PARA397.Value = MA0003row("ACCTLEASE5")
                                PARA398.Value = MA0003row("ACCTLSUPL1")
                                PARA399.Value = MA0003row("ACCTLSUPL2")
                                PARA3100.Value = MA0003row("ACCTLSUPL3")
                                PARA3101.Value = MA0003row("ACCTLSUPL4")
                                PARA3102.Value = MA0003row("ACCTLSUPL5")
                                PARA3103.Value = MA0003row("ACCTASST01")
                                PARA3104.Value = MA0003row("ACCTASST02")
                                PARA3105.Value = MA0003row("ACCTASST03")
                                PARA3106.Value = MA0003row("ACCTASST04")
                                PARA3107.Value = MA0003row("ACCTASST05")
                                PARA3108.Value = MA0003row("ACCTASST06")
                                PARA3109.Value = MA0003row("ACCTASST07")
                                PARA3110.Value = MA0003row("ACCTASST08")
                                PARA3111.Value = MA0003row("ACCTASST09")
                                PARA3112.Value = MA0003row("ACCTASST10")
                                PARA3113.Value = MA0003row("NOTES")
                                PARA3114.Value = MA0003row("DELFLG")
                                PARA3115.Value = WW_DATENOW
                                PARA3116.Value = WW_DATENOW
                                PARA3117.Value = Master.USERID
                                PARA3118.Value = Master.USERTERMID
                                PARA3119.Value = C_DEFAULT_YMD
                                PARA3120.Value = MA0003row("OTNKTMAKER")
                                PARA3121.Value = MA0003row("HPRSTMAKER")
                                PARA3122.Value = MA0003row("CHEMTMAKER")
                                PARA3123.Value = MA0003row("CONTTMAKER")

                                SQLcmd3.ExecuteNonQuery()

                                '結果 --> テーブル(MA0003tbl)反映
                                MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                                '○更新ジャーナル追加
                                Dim MA003_SHARYOBrow As DataRow = MA003_SHARYOBtbl.NewRow
                                MA003_SHARYOBrow("CAMPCODE") = MA0003row("CAMPCODE")
                                MA003_SHARYOBrow("SHARYOTYPE") = MA0003row("SHARYOTYPE")
                                MA003_SHARYOBrow("TSHABAN") = MA0003row("TSHABAN")
                                MA003_SHARYOBrow("STYMD") = RTrim(MA0003row("STYMD"))
                                MA003_SHARYOBrow("ENDYMD") = RTrim(MA0003row("ENDYMD"))
                                MA003_SHARYOBrow("BASERDATE") = MA0003row("BASERDATE")
                                MA003_SHARYOBrow("BASERAGEYY") = MA0003row("BASERAGEYY")
                                MA003_SHARYOBrow("BASERAGEMM") = MA0003row("BASERAGEMM")
                                MA003_SHARYOBrow("BASERAGE") = MA0003row("BASERAGE")
                                MA003_SHARYOBrow("BASELEASE") = MA0003row("BASELEASE")
                                If RTrim(MA0003row("FCTRFUELCAPA")) = "" Then
                                    MA003_SHARYOBrow("FCTRFUELCAPA") = 0
                                Else
                                    MA003_SHARYOBrow("FCTRFUELCAPA") = MA0003row("FCTRFUELCAPA")
                                End If
                                MA003_SHARYOBrow("FCTRFUELMATE") = MA0003row("FCTRFUELMATE")
                                MA003_SHARYOBrow("FCTRTMISSION") = MA0003row("FCTRTMISSION")
                                MA003_SHARYOBrow("FCTRUREA") = MA0003row("FCTRUREA")
                                MA003_SHARYOBrow("FCTRTIRE") = MA0003row("FCTRTIRE")
                                MA003_SHARYOBrow("FCTRDPR") = MA0003row("FCTRDPR")
                                MA003_SHARYOBrow("FCTRAXLE") = MA0003row("FCTRAXLE")
                                MA003_SHARYOBrow("FCTRSUSP") = MA0003row("FCTRSUSP")
                                MA003_SHARYOBrow("FCTRSHFTNUM") = MA0003row("FCTRSHFTNUM")
                                MA003_SHARYOBrow("FCTRSMAKER") = MA0003row("FCTRSMAKER")
                                MA003_SHARYOBrow("FCTRTMAKER") = MA0003row("FCTRTMAKER")
                                MA003_SHARYOBrow("FCTRRESERVE1") = MA0003row("FCTRRESERVE1")
                                MA003_SHARYOBrow("FCTRRESERVE2") = MA0003row("FCTRRESERVE2")
                                MA003_SHARYOBrow("FCTRRESERVE3") = MA0003row("FCTRRESERVE3")
                                MA003_SHARYOBrow("FCTRRESERVE4") = MA0003row("FCTRRESERVE4")
                                MA003_SHARYOBrow("FCTRRESERVE5") = MA0003row("FCTRRESERVE5")
                                MA003_SHARYOBrow("OTNKCELLNO") = MA0003row("OTNKCELLNO")
                                MA003_SHARYOBrow("OTNKCELPART") = MA0003row("OTNKCELPART")
                                MA003_SHARYOBrow("OTNKMATERIAL") = MA0003row("OTNKMATERIAL")
                                MA003_SHARYOBrow("OTNKLVALVE") = MA0003row("OTNKLVALVE")
                                MA003_SHARYOBrow("OTNKPUMP") = MA0003row("OTNKPUMP")
                                MA003_SHARYOBrow("OTNKPIPE") = MA0003row("OTNKPIPE")
                                MA003_SHARYOBrow("OTNKBPIPE") = MA0003row("OTNKBPIPE")
                                MA003_SHARYOBrow("OTNKHTECH") = MA0003row("OTNKHTECH")
                                MA003_SHARYOBrow("OTNKDCD") = MA0003row("OTNKDCD")
                                MA003_SHARYOBrow("OTNKDETECTOR") = MA0003row("OTNKDETECTOR")
                                MA003_SHARYOBrow("OTNKPIPESIZE") = MA0003row("OTNKPIPESIZE")
                                MA003_SHARYOBrow("OTNKEXHASIZE") = MA0003row("OTNKEXHASIZE")
                                MA003_SHARYOBrow("OTNKDISGORGE") = MA0003row("OTNKDISGORGE")
                                MA003_SHARYOBrow("OTNKVAPOR") = MA0003row("OTNKVAPOR")
                                MA003_SHARYOBrow("OTNKCVALVE") = MA0003row("OTNKCVALVE")
                                MA003_SHARYOBrow("OTNKTINSNO") = MA0003row("OTNKTINSNO")
                                If RTrim(MA0003row("OTNKINSYMD")) = "" Then
                                    MA003_SHARYOBrow("OTNKINSYMD") = C_DEFAULT_YMD
                                Else
                                    MA003_SHARYOBrow("OTNKINSYMD") = MA0003row("OTNKINSYMD")
                                End If
                                MA003_SHARYOBrow("OTNKINSSTAT") = MA0003row("OTNKINSSTAT")
                                MA003_SHARYOBrow("HPRSMATR") = MA0003row("HPRSMATR")
                                MA003_SHARYOBrow("HPRSSTRUCT") = MA0003row("HPRSSTRUCT")
                                MA003_SHARYOBrow("HPRSVALVE") = MA0003row("HPRSVALVE")
                                MA003_SHARYOBrow("HPRSPIPE") = MA0003row("HPRSPIPE")
                                MA003_SHARYOBrow("HPRSPUMP") = MA0003row("HPRSPUMP")
                                MA003_SHARYOBrow("HPRSPMPDR") = MA0003row("HPRSPMPDR")
                                MA003_SHARYOBrow("HPRSRESSRE") = MA0003row("HPRSRESSRE")
                                MA003_SHARYOBrow("HPRSPIPENUM") = MA0003row("HPRSPIPENUM")
                                MA003_SHARYOBrow("HPRSINSULATE") = MA0003row("HPRSINSULATE")
                                MA003_SHARYOBrow("HPRSSERNO") = MA0003row("HPRSSERNO")
                                If RTrim(MA0003row("HPRSINSIYMD")) = "" Then
                                    MA003_SHARYOBrow("HPRSINSIYMD") = C_DEFAULT_YMD
                                Else
                                    MA003_SHARYOBrow("HPRSINSIYMD") = MA0003row("HPRSINSIYMD")
                                End If
                                MA003_SHARYOBrow("HPRSINSISTAT") = MA0003row("HPRSINSISTAT")
                                MA003_SHARYOBrow("HPRSHOSE") = MA0003row("HPRSHOSE")
                                MA003_SHARYOBrow("CHEMCELLNO") = MA0003row("CHEMCELLNO")
                                MA003_SHARYOBrow("CHEMCELPART") = MA0003row("CHEMCELPART")
                                MA003_SHARYOBrow("CHEMMATERIAL") = MA0003row("CHEMMATERIAL")
                                MA003_SHARYOBrow("CHEMSTRUCT") = MA0003row("CHEMSTRUCT")
                                MA003_SHARYOBrow("CHEMPUMP") = MA0003row("CHEMPUMP")
                                MA003_SHARYOBrow("CHEMPMPDR") = MA0003row("CHEMPMPDR")
                                MA003_SHARYOBrow("CHEMDISGORGE") = MA0003row("CHEMDISGORGE")
                                MA003_SHARYOBrow("CHEMPRESDRV") = MA0003row("CHEMPRESDRV")
                                MA003_SHARYOBrow("CHEMTHERM") = MA0003row("CHEMTHERM")
                                MA003_SHARYOBrow("CHEMMANOMTR") = MA0003row("CHEMMANOMTR")
                                MA003_SHARYOBrow("CHEMHOSE") = MA0003row("CHEMHOSE")
                                MA003_SHARYOBrow("CHEMPRESEQ") = MA0003row("CHEMPRESEQ")
                                MA003_SHARYOBrow("CHEMTINSNO") = MA0003row("CHEMTINSNO")
                                If RTrim(MA0003row("CHEMINSYMD")) = "" Then
                                    MA003_SHARYOBrow("CHEMINSYMD") = C_DEFAULT_YMD
                                Else
                                    MA003_SHARYOBrow("CHEMINSYMD") = MA0003row("CHEMINSYMD")
                                End If
                                MA003_SHARYOBrow("CHEMINSSTAT") = MA0003row("CHEMINSSTAT")
                                MA003_SHARYOBrow("CONTSHAPE") = MA0003row("CONTSHAPE")
                                MA003_SHARYOBrow("CONTPUMP") = MA0003row("CONTPUMP")
                                MA003_SHARYOBrow("CONTPMPDR") = MA0003row("CONTPMPDR")
                                MA003_SHARYOBrow("OTHRTERMINAL") = MA0003row("OTHRTERMINAL")
                                MA003_SHARYOBrow("OTHRBMONITOR") = MA0003row("OTHRBMONITOR")
                                MA003_SHARYOBrow("OTHRRADIOCON") = MA0003row("OTHRRADIOCON")
                                MA003_SHARYOBrow("OTHRDOCO") = MA0003row("OTHRDOCO")
                                MA003_SHARYOBrow("OTHRPAINTING") = MA0003row("OTHRPAINTING")
                                MA003_SHARYOBrow("OTHRDRRECORD") = MA0003row("OTHRDRRECORD")
                                MA003_SHARYOBrow("OTHRBSONAR") = MA0003row("OTHRBSONAR")
                                MA003_SHARYOBrow("OTHRRTARGET") = MA0003row("OTHRRTARGET")
                                MA003_SHARYOBrow("OTHRTIRE1") = MA0003row("OTHRTIRE1")
                                MA003_SHARYOBrow("OTHRTIRE2") = MA0003row("OTHRTIRE2")
                                MA003_SHARYOBrow("OTHRTPMS") = MA0003row("OTHRTPMS")
                                MA003_SHARYOBrow("OFFCRESERVE1") = MA0003row("OFFCRESERVE1")
                                MA003_SHARYOBrow("OFFCRESERVE2") = MA0003row("OFFCRESERVE2")
                                MA003_SHARYOBrow("OFFCRESERVE3") = MA0003row("OFFCRESERVE3")
                                MA003_SHARYOBrow("OFFCRESERVE4") = MA0003row("OFFCRESERVE4")
                                MA003_SHARYOBrow("OFFCRESERVE5") = MA0003row("OFFCRESERVE5")
                                If RTrim(MA0003row("ACCTRCYCLE")) = "" Then
                                    MA003_SHARYOBrow("ACCTRCYCLE") = 0
                                Else
                                    MA003_SHARYOBrow("ACCTRCYCLE") = MA0003row("ACCTRCYCLE")
                                End If
                                MA003_SHARYOBrow("ACCTLEASE1") = MA0003row("ACCTLEASE1")
                                MA003_SHARYOBrow("ACCTLEASE2") = MA0003row("ACCTLEASE2")
                                MA003_SHARYOBrow("ACCTLEASE3") = MA0003row("ACCTLEASE3")
                                MA003_SHARYOBrow("ACCTLEASE4") = MA0003row("ACCTLEASE4")
                                MA003_SHARYOBrow("ACCTLEASE5") = MA0003row("ACCTLEASE5")
                                MA003_SHARYOBrow("ACCTLSUPL1") = MA0003row("ACCTLSUPL1")
                                MA003_SHARYOBrow("ACCTLSUPL2") = MA0003row("ACCTLSUPL2")
                                MA003_SHARYOBrow("ACCTLSUPL3") = MA0003row("ACCTLSUPL3")
                                MA003_SHARYOBrow("ACCTLSUPL4") = MA0003row("ACCTLSUPL4")
                                MA003_SHARYOBrow("ACCTLSUPL5") = MA0003row("ACCTLSUPL5")
                                MA003_SHARYOBrow("ACCTASST01") = MA0003row("ACCTASST01")
                                MA003_SHARYOBrow("ACCTASST02") = MA0003row("ACCTASST02")
                                MA003_SHARYOBrow("ACCTASST03") = MA0003row("ACCTASST03")
                                MA003_SHARYOBrow("ACCTASST04") = MA0003row("ACCTASST04")
                                MA003_SHARYOBrow("ACCTASST05") = MA0003row("ACCTASST05")
                                MA003_SHARYOBrow("ACCTASST06") = MA0003row("ACCTASST06")
                                MA003_SHARYOBrow("ACCTASST07") = MA0003row("ACCTASST07")
                                MA003_SHARYOBrow("ACCTASST08") = MA0003row("ACCTASST08")
                                MA003_SHARYOBrow("ACCTASST09") = MA0003row("ACCTASST09")
                                MA003_SHARYOBrow("ACCTASST10") = MA0003row("ACCTASST10")
                                MA003_SHARYOBrow("NOTES") = MA0003row("NOTES")
                                MA003_SHARYOBrow("DELFLG") = MA0003row("DELFLG")
                                MA003_SHARYOBrow("OTNKTMAKER") = MA0003row("OTNKTMAKER")
                                MA003_SHARYOBrow("HPRSTMAKER") = MA0003row("HPRSTMAKER")
                                MA003_SHARYOBrow("CHEMTMAKER") = MA0003row("CHEMTMAKER")
                                MA003_SHARYOBrow("CONTTMAKER") = MA0003row("CONTTMAKER")
                                CS0020JOURNAL.TABLENM = "MA003_SHARYOB"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = MA003_SHARYOBrow
                                CS0020JOURNAL.CS0020JOURNAL()
                                If Not isNormal(CS0020JOURNAL.ERR) Then
                                    Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                    CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                    CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                    CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                    CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力
                                    Exit Sub
                                End If

                                '○更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                                PARA501.Value = MA0003row("CAMPCODE")
                                PARA502.Value = MA0003row("SHARYOTYPE")
                                PARA503.Value = MA0003row("TSHABAN")
                                PARA504.Value = RTrim(MA0003row("STYMD"))

                                Using SQLdr5 As SqlDataReader = SQLcmd5.ExecuteReader()
                                    If SQLdr5.Read Then
                                        MA0003row("TIMSTP") = SQLdr5("TIMSTP")
                                    End If
                                End Using
                            End If
                        Next
                    End Using
                End Using
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA003_SHARYOB UPDATE_INSERT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB UPDATE_INSERT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                Exit Sub
            End Try
        End If

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_SELSHARYOTYPE_LABEL.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '帳票出力dll Interface
        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRMA0003WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = MA0003tbl                        'データ参照DataTable
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○ 帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRMA0003WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MA0003tbl                        'データ参照DataTable
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
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        '○ 画面遷移実行
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁遷移ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub
    ''' <summary>
    ''' 最終頁遷移ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(MA0003tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧の明細行ダブルクリック時処理
    ''' </summary>
    ''' <remarks>(GridView ---> detailbox)</remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_FILED_OBJ As Object

        'LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT = WW_LINECNT - 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○Grid内容(MA0003tbl)よりDetail編集 

        WF_Sel_LINECNT.Text = MA0003tbl.Rows(WW_LINECNT)("LINECNT")
        WF_CAMPCODE.Text = MA0003tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_SHARYOTYPE.Text = MA0003tbl.Rows(WW_LINECNT)("SHARYOTYPE")
        WF_TSHABAN.Text = MA0003tbl.Rows(WW_LINECNT)("TSHABAN")
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE.Text, WF_TSHABAN1_TEXT.Text, WW_DUMMY)
        WF_STYMD.Text = MA0003tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = MA0003tbl.Rows(WW_LINECNT)("ENDYMD")
        WF_DELFLG.Text = MA0003tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○タブの処理
        If MA0003tbl.Rows(WW_LINECNT)("TIMSTP") = 0 Then
            'タブ
            WF_Dtab01.Style.Remove("color")
            WF_Dtab01.Style.Add("color", "black")
            WF_Dtab01.Style.Remove("background-color")
            WF_Dtab01.Style.Add("background-color", "rgb(255,255,253)")
            WF_Dtab01.Style.Remove("border")
            WF_Dtab01.Style.Add("border", "1px solid blue")
            WF_Dtab01.Style.Remove("font-weight")
            WF_Dtab01.Style.Add("font-weight", "normal")

            If WF_DetailMView.ActiveViewIndex = 0 Then
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            End If
        Else
            'タブ
            WF_Dtab01.Style.Remove("color")
            WF_Dtab01.Style.Add("color", "black")
            WF_Dtab01.Style.Remove("background-color")
            WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
            WF_Dtab01.Style.Remove("border")
            WF_Dtab01.Style.Add("border", "1px solid black")
            WF_Dtab01.Style.Remove("font-weight")
            WF_Dtab01.Style.Add("font-weight", "normal")

            If WF_DetailMView.ActiveViewIndex = 0 Then
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            End If
        End If

        '○タブ別処理
        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                '左
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_1"), Label).Text = WW_TEXT
                End If

                '中央
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_2"), Label).Text = WW_TEXT
                End If

                '右
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0003tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_3"), Label).Text = WW_TEXT
                End If
            Next
        Next

        '○品名２名の取り直し
        Dim WW_OILTYPE As String = ""
        Dim WW_MANGPROD1 As String = ""
        Dim WW_MANGPROD2 As String = ""
        'タブ別処理(01 管理)から品名1を取得
        Repeater_ItemFIND("MANGOILTYPE", "1", WW_OILTYPE)
        'タブ別処理(01 管理)から品名1を取得
        Repeater_ItemFIND("MANGPROD1", "1", WW_MANGPROD1)
        'タブ別処理(01 管理)から品名2を取得
        Repeater_ItemFIND("MANGPROD2", "1", WW_MANGPROD2)

        CODENAME_get("MANGPROD2", WW_MANGPROD2, WW_TEXT, WW_DUMMY, {WW_OILTYPE, WW_MANGPROD1})

        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = "MANGPROD2" Then
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
                Exit For
            End If
        Next

        '○タブ別処理(11 PDF)
        PDF_Readonly()

        '○タブ（内容）の表示非表示
        TAB_DisplayCTRL(MA0003tbl.Rows(WW_LINECNT)("SHARYOTYPE"))

        '画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0003row As DataRow In MA0003tbl.Rows
            Select Case MA0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MA0003tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MA0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MA0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MA0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MA0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MA0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        WF_GridDBclick.Text = ""

    End Sub
    ''' <summary>
    ''' フィールドフォーマット処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "MANGSHAFUKU"
                Try
                    WF_ITEM_FORMAT = Format(CSng(I_VALUE), "0.000")
                Catch ex As Exception
                End Try
            Case "ACCTRCYCLE"
                Try
                    WF_ITEM_FORMAT = Format(CInt(I_VALUE), "#,#")
                Catch ex As Exception
                End Try
            Case "BASERAGEYY", "BASERAGEMM", "BASERAGE", "FCTRFUELCAPA"
                Try
                    WF_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
            Case Else
        End Select
    End Function
    ''' <summary>
    ''' 詳細項目取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_ItemFIND(ByVal I_FIELD As String, ByVal I_TABNO As String, ByRef O_VALUE As String)

        Dim WW_FIELD_1 As String = "WF_Rep" & I_TABNO & "_FIELD_1"
        Dim WW_FIELD_2 As String = "WF_Rep" & I_TABNO & "_FIELD_2"
        Dim WW_FIELD_3 As String = "WF_Rep" & I_TABNO & "_FIELD_3"
        Dim WW_VALUE_1 As String = "WF_Rep" & I_TABNO & "_VALUE_1"
        Dim WW_VALUE_2 As String = "WF_Rep" & I_TABNO & "_VALUE_2"
        Dim WW_VALUE_3 As String = "WF_Rep" & I_TABNO & "_VALUE_3"

        Dim DViewRep As New Repeater
        Select Case I_TABNO
            Case "1"
                DViewRep = WF_DViewRep1
            Case "2"
                DViewRep = WF_DViewRep2
            Case "3"
                DViewRep = WF_DViewRep3
            Case "4"
                DViewRep = WF_DViewRep4
            Case "5"
                DViewRep = WF_DViewRep5
            Case "6"
                DViewRep = WF_DViewRep6
            Case "7"
                DViewRep = WF_DViewRep7
            Case "8"
                DViewRep = WF_DViewRep8
            Case "9"
                DViewRep = WF_DViewRep9
            Case "10"
                DViewRep = WF_DViewRep10
        End Select
        'タブから指定された項目の値を取得
        For Each reitem As RepeaterItem In DViewRep.Items
            If CType(reitem.FindControl(WW_FIELD_1), Label).Text = I_FIELD Then
                O_VALUE = CType(reitem.FindControl(WW_VALUE_1), TextBox).Text
                Exit For
            End If
            If CType(reitem.FindControl(WW_FIELD_2), Label).Text = I_FIELD Then
                O_VALUE = CType(reitem.FindControl(WW_VALUE_2), TextBox).Text
                Exit For
            End If
            If CType(reitem.FindControl(WW_FIELD_3), Label).Text = I_FIELD Then
                O_VALUE = CType(reitem.FindControl(WW_VALUE_3), TextBox).Text
                Exit For
            End If
        Next

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○エラーレポート準備
        rightview.setErrorReport("")

        'DetailBoxをMA0003INPtblへ退避
        DetailBoxToMA0003INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック 
        INPUT_CHEK(WW_ERRCODE)

        '○入力値テーブル反映(MA0003INPtbl⇒MA0003tbl)
        If isNormal(WW_ERRCODE) Then
            TBL_UPD("MAP", WW_ERRCODE)
        End If

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        '明細-画面クリア
        If isNormal(WW_ERRCODE) Then
            WF_CLEAR_Click()
        End If

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_SELMORG.Focus()

    End Sub
    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMA0003INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Master.CreateEmptyTable(MA0003INPtbl)

        Dim MA0003INProw As DataRow = MA0003INPtbl.NewRow

        '初期クリア
        For Each MA0003INPcol As DataColumn In MA0003INPtbl.Columns

            If IsDBNull(MA0003INProw.Item(MA0003INPcol)) OrElse IsNothing(MA0003INProw.Item(MA0003INPcol)) Then
                Select Case MA0003INPcol.ColumnName
                    Case "LINECNT"
                        MA0003INProw.Item(MA0003INPcol) = 0
                    Case "TIMSTP"
                        MA0003INProw.Item(MA0003INPcol) = 0
                    Case "SELECT"
                        MA0003INProw.Item(MA0003INPcol) = 1
                    Case "HIDDEN"
                        MA0003INProw.Item(MA0003INPcol) = 0
                    Case "WORK_NO"
                        MA0003INProw.Item(MA0003INPcol) = 0
                    Case Else
                        If MA0003INPcol.DataType.Name = "String" Then
                            MA0003INProw.Item(MA0003INPcol) = ""
                        Else
                            MA0003INProw.Item(MA0003INPcol) = 0
                        End If
                End Select
            End If
        Next

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_SHARYOTYPE.Text)        '統一車番(上)
        Master.eraseCharToIgnore(WF_TSHABAN.Text)           '統一車番(下)
        Master.eraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○DetailよりMA0003INPtbl編集 
        If WF_Sel_LINECNT.Text = "" Then
            MA0003INProw("LINECNT") = 0
        Else
            MA0003INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If

        MA0003INProw("CAMPCODE") = WF_CAMPCODE.Text
        MA0003INProw("SHARYOTYPE") = WF_SHARYOTYPE.Text
        MA0003INProw("TSHABAN") = WF_TSHABAN.Text
        MA0003INProw("STYMD") = WF_STYMD.Text
        MA0003INProw("ENDYMD") = WF_ENDYMD.Text
        MA0003INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_SHARYOTYPE.Text) AndAlso
            String.IsNullOrEmpty(WF_TSHABAN.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMA0003INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○タブ別処理
        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                '左
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0003INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
                End If

                '中央
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0003INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
                End If

                '右
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0003INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
                End If
            Next
        Next

        '○名称付与
        MA0003INProw("MANGMORGNAME") = ""                                                                   '管理部署名
        CODENAME_get("MANGMORG", MA0003INProw("MANGMORG"), MA0003INProw("MANGMORGNAME"), WW_DUMMY)
        MA0003INProw("MANGSORGNAME") = ""                                                                   '設置部署名
        CODENAME_get("MANGSORG", MA0003INProw("MANGSORG"), MA0003INProw("MANGSORGNAME"), WW_DUMMY)
        MA0003INProw("MANGOILTYPENAME") = ""                                                                '油種名
        CODENAME_get("MANGOILTYPE", MA0003INProw("MANGOILTYPE"), MA0003INProw("MANGOILTYPENAME"), WW_DUMMY)
        MA0003INProw("MANGOWNCODENAME") = ""                                                                '荷主名
        CODENAME_get("MANGOWNCODE", MA0003INProw("MANGOWNCODE"), MA0003INProw("MANGOWNCODENAME"), WW_DUMMY)
        MA0003INProw("MANGOWNCONTNAME") = ""                                                                '契約区分名
        CODENAME_get("MANGOWNCONT", MA0003INProw("MANGOWNCONT"), MA0003INProw("MANGOWNCONTNAME"), WW_DUMMY)
        MA0003INProw("MANGSUPPLNAME") = ""                                                                  '庸車会社名
        CODENAME_get("MANGSUPPL", MA0003INProw("MANGSUPPL"), MA0003INProw("MANGSUPPLNAME"), WW_DUMMY)
        MA0003INProw("MANGUORGNAME") = ""                                                                   '運用部署名
        MA0003INProw("BASELEASENAME") = ""                                                                  '車両所有名
        CODENAME_get("BASELEASE", MA0003INProw("BASELEASE"), MA0003INProw("BASELEASENAME"), WW_DUMMY)
        MA0003INProw("FCTRAXLENAME") = ""                                                                   'リフトアクスル名
        CODENAME_get("FCTRAXLE", MA0003INProw("FCTRAXLE"), MA0003INProw("FCTRAXLENAME"), WW_DUMMY)
        MA0003INProw("FCTRTMAKER") = ""                                                                     'タンクメーカー
        MA0003INProw("FCTRTMAKERNAME") = ""
        MA0003INProw("FCTRDPRNAME") = ""                                                                    'DPR名
        CODENAME_get("FCTRDPR", MA0003INProw("FCTRDPR"), MA0003INProw("FCTRDPRNAME"), WW_DUMMY)
        MA0003INProw("FCTRFUELMATENAME") = ""                                                               '燃料タンク材質名
        CODENAME_get("FCTRFUELMATE", MA0003INProw("FCTRFUELMATE"), MA0003INProw("FCTRFUELMATENAME"), WW_DUMMY)
        MA0003INProw("FCTRSHFTNUMNAME") = ""                                                                '軸数名
        CODENAME_get("FCTRSHFTNUM", MA0003INProw("FCTRSHFTNUM"), MA0003INProw("FCTRSHFTNUMNAME"), WW_DUMMY)
        MA0003INProw("FCTRSUSPNAME") = ""                                                                   'サスペンション種類名
        CODENAME_get("FCTRSUSP", MA0003INProw("FCTRSUSP"), MA0003INProw("FCTRSUSPNAME"), WW_DUMMY)
        MA0003INProw("FCTRTMISSIONNAME") = ""                                                               'ミッション名
        CODENAME_get("FCTRTMISSION", MA0003INProw("FCTRTMISSION"), MA0003INProw("FCTRTMISSIONNAME"), WW_DUMMY)
        MA0003INProw("FCTRUREANAME") = ""                                                                   '尿素名
        CODENAME_get("FCTRUREA", MA0003INProw("FCTRUREA"), MA0003INProw("FCTRUREANAME"), WW_DUMMY)
        MA0003INProw("OTNKBPIPENAME") = ""                                                                  '後配管名
        CODENAME_get("OTNKBPIPE", MA0003INProw("OTNKBPIPE"), MA0003INProw("OTNKBPIPENAME"), WW_DUMMY)
        MA0003INProw("OTNKVAPORNAME") = ""                                                                  'ベーパー名
        CODENAME_get("OTNKVAPOR", MA0003INProw("OTNKVAPOR"), MA0003INProw("OTNKVAPORNAME"), WW_DUMMY)
        MA0003INProw("OTNKCVALVENAME") = ""                                                                 '中間ﾊﾞﾙﾌﾞ有無名
        CODENAME_get("OTNKCVALVE", MA0003INProw("OTNKCVALVE"), MA0003INProw("OTNKCVALVENAME"), WW_DUMMY)
        MA0003INProw("OTNKDCDNAME") = ""                                                                    'ＤＣＤ装備名
        CODENAME_get("OTNKDCD", MA0003INProw("OTNKDCD"), MA0003INProw("OTNKDCDNAME"), WW_DUMMY)
        MA0003INProw("FCTRSMAKERNAME") = ""                                                                 '車両メーカー
        CODENAME_get("FCTRSMAKER", MA0003INProw("FCTRSMAKER"), MA0003INProw("FCTRSMAKERNAME"), WW_DUMMY)
        MA0003INProw("OTNKDETECTORNAME") = ""                                                               '検水管名
        CODENAME_get("OTNKDETECTOR", MA0003INProw("OTNKDETECTOR"), MA0003INProw("OTNKDETECTORNAME"), WW_DUMMY)
        MA0003INProw("OTNKDISGORGENAME") = ""                                                               '吐出口名
        CODENAME_get("OTNKDISGORGE", MA0003INProw("OTNKDISGORGE"), MA0003INProw("OTNKDISGORGENAME"), WW_DUMMY)
        MA0003INProw("OTNKHTECHNAME") = ""                                                                  'ハイテク種別名
        CODENAME_get("OTNKHTECH", MA0003INProw("OTNKHTECH"), MA0003INProw("OTNKHTECHNAME"), WW_DUMMY)
        MA0003INProw("OTNKLVALVENAME") = ""                                                                 '底弁形式名
        CODENAME_get("OTNKLVALVE", MA0003INProw("OTNKLVALVE"), MA0003INProw("OTNKLVALVENAME"), WW_DUMMY)
        MA0003INProw("OTNKMATERIALNAME") = ""                                                               'タンク材質名
        CODENAME_get("OTNKMATERIAL", MA0003INProw("OTNKMATERIAL"), MA0003INProw("OTNKMATERIALNAME"), WW_DUMMY)
        MA0003INProw("OTNKPIPENAME") = ""                                                                   '配管形態名
        CODENAME_get("OTNKPIPE", MA0003INProw("OTNKPIPE"), MA0003INProw("OTNKPIPENAME"), WW_DUMMY)
        MA0003INProw("OTNKPIPESIZENAME") = ""                                                               '配管サイズ名
        CODENAME_get("OTNKPIPESIZE", MA0003INProw("OTNKPIPESIZE"), MA0003INProw("OTNKPIPESIZENAME"), WW_DUMMY)
        MA0003INProw("OTNKPUMPNAME") = ""                                                                   'ポンプ名
        CODENAME_get("OTNKPUMP", MA0003INProw("OTNKPUMP"), MA0003INProw("OTNKPUMPNAME"), WW_DUMMY)
        MA0003INProw("HPRSPMPDRNAME") = ""                                                                  'ポンプ駆動方法
        CODENAME_get("HPRSPMPDR", MA0003INProw("HPRSPMPDR"), MA0003INProw("HPRSPMPDRNAME"), WW_DUMMY)
        MA0003INProw("HPRSINSULATENAME") = ""                                                               '断熱構造名
        CODENAME_get("HPRSINSULATE", MA0003INProw("HPRSINSULATE"), MA0003INProw("HPRSINSULATENAME"), WW_DUMMY)
        MA0003INProw("HPRSMATRNAME") = ""                                                                   'タンク材質名
        CODENAME_get("HPRSMATR", MA0003INProw("HPRSMATR"), MA0003INProw("HPRSMATRNAME"), WW_DUMMY)
        MA0003INProw("HPRSPIPENAME") = ""                                                                   '配管形状（仮）名
        CODENAME_get("HPRSPIPE", MA0003INProw("HPRSPIPE"), MA0003INProw("HPRSPIPENAME"), WW_DUMMY)
        MA0003INProw("HPRSPIPENUMNAME") = ""                                                                '配管口数名
        CODENAME_get("HPRSPIPENUM", MA0003INProw("HPRSPIPENUM"), MA0003INProw("HPRSPIPENUMNAME"), WW_DUMMY)
        MA0003INProw("HPRSPUMPNAME") = ""                                                                   'ポンプ名
        CODENAME_get("HPRSPUMP", MA0003INProw("HPRSPUMP"), MA0003INProw("HPRSPUMPNAME"), WW_DUMMY)
        MA0003INProw("HPRSRESSRENAME") = ""                                                                 '加圧器名
        CODENAME_get("HPRSRESSRE", MA0003INProw("HPRSRESSRE"), MA0003INProw("HPRSRESSRENAME"), WW_DUMMY)
        MA0003INProw("HPRSSTRUCTNAME") = ""                                                                 'タンク構造名
        CODENAME_get("HPRSSTRUCT", MA0003INProw("HPRSSTRUCT"), MA0003INProw("HPRSSTRUCTNAME"), WW_DUMMY)
        MA0003INProw("HPRSVALVENAME") = ""                                                                  '底弁形式名
        CODENAME_get("HPRSVALVE", MA0003INProw("HPRSVALVE"), MA0003INProw("HPRSVALVENAME"), WW_DUMMY)
        MA0003INProw("CHEMDISGORGENAME") = ""                                                               '吐出口名
        CODENAME_get("CHEMDISGORGE", MA0003INProw("CHEMDISGORGE"), MA0003INProw("CHEMDISGORGENAME"), WW_DUMMY)
        MA0003INProw("CHEMHOSENAME") = ""                                                                   'ホースボックス名
        CODENAME_get("CHEMHOSE", MA0003INProw("CHEMHOSE"), MA0003INProw("CHEMHOSENAME"), WW_DUMMY)
        MA0003INProw("CHEMMANOMTRNAME") = ""                                                                '圧力計名
        CODENAME_get("CHEMMANOMTR", MA0003INProw("CHEMMANOMTR"), MA0003INProw("CHEMMANOMTRNAME"), WW_DUMMY)
        MA0003INProw("CHEMMATERIALNAME") = ""                                                               'タンク材質名
        CODENAME_get("CHEMMATERIAL", MA0003INProw("CHEMMATERIAL"), MA0003INProw("CHEMMATERIALNAME"), WW_DUMMY)
        MA0003INProw("CHEMPMPDRNAME") = ""                                                                  'ポンプ駆動方法名
        CODENAME_get("CHEMPMPDR", MA0003INProw("CHEMPMPDR"), MA0003INProw("CHEMPMPDRNAME"), WW_DUMMY)
        MA0003INProw("CHEMPRESDRVNAME") = ""                                                                '加温装置名
        CODENAME_get("CHEMPRESDRV", MA0003INProw("CHEMPRESDRV"), MA0003INProw("CHEMPRESDRVNAME"), WW_DUMMY)
        MA0003INProw("CHEMPRESEQNAME") = ""                                                                 '均圧配管名
        CODENAME_get("CHEMPRESEQ", MA0003INProw("CHEMPRESEQ"), MA0003INProw("CHEMPRESEQNAME"), WW_DUMMY)
        MA0003INProw("CHEMPUMPNAME") = ""                                                                   'ポンプ名
        CODENAME_get("CHEMPUMP", MA0003INProw("CHEMPUMP"), MA0003INProw("CHEMPUMPNAME"), WW_DUMMY)
        MA0003INProw("CHEMSTRUCTNAME") = ""                                                                 'タンク構造名
        CODENAME_get("CHEMSTRUCT", MA0003INProw("CHEMSTRUCT"), MA0003INProw("CHEMSTRUCTNAME"), WW_DUMMY)
        MA0003INProw("CHEMTHERMNAME") = ""                                                                  '温度計名
        CODENAME_get("CHEMTHERM", MA0003INProw("CHEMTHERM"), MA0003INProw("CHEMTHERMNAME"), WW_DUMMY)
        MA0003INProw("OTHRBMONITORNAME") = ""                                                               'バックモニター名
        CODENAME_get("OTHRBMONITOR", MA0003INProw("OTHRBMONITOR"), MA0003INProw("OTHRBMONITORNAME"), WW_DUMMY)
        MA0003INProw("OTHRBSONARNAME") = ""                                                                 'バックソナー名
        CODENAME_get("OTHRBSONAR", MA0003INProw("OTHRBSONAR"), MA0003INProw("OTHRBSONARNAME"), WW_DUMMY)
        MA0003INProw("FCTRTIRENAME") = ""                                                                   'ＤoCoですCar番号名
        CODENAME_get("FCTRTIRE", MA0003INProw("FCTRTIRE"), MA0003INProw("FCTRTIRENAME"), WW_DUMMY)
        MA0003INProw("OTHRDRRECORDNAME") = ""                                                               'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
        CODENAME_get("OTHRDRRECORD", MA0003INProw("OTHRDRRECORD"), MA0003INProw("OTHRDRRECORDNAME"), WW_DUMMY)
        MA0003INProw("OTHRPAINTINGNAME") = ""                                                               '塗装名
        CODENAME_get("OTHRPAINTING", MA0003INProw("OTHRPAINTING"), MA0003INProw("OTHRPAINTINGNAME"), WW_DUMMY)
        MA0003INProw("OTHRRADIOCONNAME") = ""                                                               '無線（有・無）名
        CODENAME_get("OTHRRADIOCON", MA0003INProw("OTHRRADIOCON"), MA0003INProw("OTHRRADIOCONNAME"), WW_DUMMY)
        MA0003INProw("OTHRRTARGETNAME") = ""                                                                '一括修理非対象車名
        CODENAME_get("OTHRRTARGET", MA0003INProw("OTHRRTARGET"), MA0003INProw("OTHRRTARGETNAME"), WW_DUMMY)
        MA0003INProw("OTHRTERMINALNAME") = ""                                                               '車載端末名
        CODENAME_get("OTHRTERMINAL", MA0003INProw("OTHRTERMINAL"), MA0003INProw("OTHRTERMINALNAME"), WW_DUMMY)
        MA0003INProw("MANGPROD1NAME") = ""                                                                  '品名１
        CODENAME_get("MANGPROD1", MA0003INProw("MANGPROD1"), MA0003INProw("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0003INProw("MANGOILTYPE"))})
        MA0003INProw("MANGPROD2NAME") = ""                                                                  '品名２
        CODENAME_get("MANGPROD2", MA0003INProw("MANGPROD2"), MA0003INProw("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0003INProw("MANGOILTYPE")), CStr(MA0003INProw("MANGPROD1"))})
        MA0003INProw("OTNKEXHASIZENAME") = ""                                                               '吐出口サイズ
        CODENAME_get("OTNKEXHASIZE", MA0003INProw("OTNKEXHASIZE"), MA0003INProw("OTNKEXHASIZENAME"), WW_DUMMY)
        MA0003INProw("HPRSHOSENAME") = ""                                                                   'ホースボックス
        CODENAME_get("HPRSHOSE", MA0003INProw("HPRSHOSE"), MA0003INProw("HPRSHOSENAME"), WW_DUMMY)
        MA0003INProw("CONTSHAPENAME") = ""                                                                  'シャーシ形状
        CODENAME_get("CONTSHAPE", MA0003INProw("CONTSHAPE"), MA0003INProw("CONTSHAPENAME"), WW_DUMMY)
        MA0003INProw("CONTPUMPNAME") = ""                                                                   'ポンプ
        CODENAME_get("CONTPUMP", MA0003INProw("CONTPUMP"), MA0003INProw("CONTPUMPNAME"), WW_DUMMY)
        MA0003INProw("CONTPMPDRNAME") = ""                                                                  'ポンプ駆動方法
        CODENAME_get("CONTPMPDR", MA0003INProw("CONTPMPDR"), MA0003INProw("CONTPMPDRNAME"), WW_DUMMY)
        MA0003INProw("OTHRTPMSNAME") = ""                                                                   'TPMS
        CODENAME_get("OTHRTPMS", MA0003INProw("OTHRTPMS"), MA0003INProw("OTHRTPMSNAME"), WW_DUMMY)
        MA0003INProw("OTNKTMAKERNAME") = ""                                                                 '石油タンクメーカー
        CODENAME_get("OTNKTMAKER", MA0003INProw("OTNKTMAKER"), MA0003INProw("OTNKTMAKERNAME"), WW_DUMMY)
        MA0003INProw("HPRSTMAKERNAME") = ""                                                                 '高圧タンクメーカー
        CODENAME_get("HPRSTMAKER", MA0003INProw("HPRSTMAKER"), MA0003INProw("HPRSTMAKERNAME"), WW_DUMMY)
        MA0003INProw("CHEMTMAKERNAME") = ""                                                                 '化成品タンクメーカー
        CODENAME_get("CHEMTMAKER", MA0003INProw("CHEMTMAKER"), MA0003INProw("CHEMTMAKERNAME"), WW_DUMMY)
        MA0003INProw("CONTTMAKERNAME") = ""                                                                 'コンテナタンクメーカー
        CODENAME_get("CONTTMAKER", MA0003INProw("CONTTMAKER"), MA0003INProw("CONTTMAKERNAME"), WW_DUMMY)
        MA0003INProw("SHARYOSTATUSNAME") = ""                                                               '運行状況
        CODENAME_get("SHARYOSTATUS", MA0003INProw("SHARYOSTATUS"), MA0003INProw("SHARYOSTATUSNAME"), WW_DUMMY)
        MA0003INProw("INSKBNNAME") = ""                                                                     '検査区分
        CODENAME_get("INSKBN", MA0003INProw("INSKBN"), MA0003INProw("INSKBNNAME"), WW_DUMMY)

        MA0003INPtbl.Rows.Add(MA0003INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""

        For Each MA0003row As DataRow In MA0003tbl.Rows
            Select Case MA0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        '○detailboxヘッダークリア
        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WF_SHARYOTYPE.Text = ""
        WF_TSHABAN.Text = ""
        WF_TSHABAN1_TEXT.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        '詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()
        TAB_DisplayCTRL(WF_SHARYOTYPE.Text)

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_STYMD.Focus()

    End Sub
    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()
        Dim dataTable As DataTable = New DataTable
        '○詳細ヘッダーの設定
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WF_CAMPCODE.ReadOnly = True
        WF_CAMPCODE.Style.Add("background-color", "rgb(213,208,181)")

        'カラム情報をリピーター作成用に取得
        Master.CreateEmptyTable(dataTable)
        dataTable.Rows.Add(dataTable.NewRow())

        '○ディテール01（管理）変数設定 
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "MANG"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep1
        CS0052DetailView.COLPREFIX = "WF_Rep1_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール02（連結車番）変数設定 
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "SYAB"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep2
        CS0052DetailView.COLPREFIX = "WF_Rep2_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール03（車両緒元）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "FCTR"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep3
        CS0052DetailView.COLPREFIX = "WF_Rep3_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール04（石油タンク）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "OTNK"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep4
        CS0052DetailView.COLPREFIX = "WF_Rep4_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール05（高圧タンク）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "HPRS"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep5
        CS0052DetailView.COLPREFIX = "WF_Rep5_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール06（化成品タンク）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "CHEM"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep6
        CS0052DetailView.COLPREFIX = "WF_Rep6_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール07（コンテナ）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "CONT"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep7
        CS0052DetailView.COLPREFIX = "WF_Rep7_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール08（車両その他）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "OTHR"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep8
        CS0052DetailView.COLPREFIX = "WF_Rep8_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール09（経理）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "ACCT"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep9
        CS0052DetailView.COLPREFIX = "WF_Rep9_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール10（申請）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "LICN"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep10
        CS0052DetailView.COLPREFIX = "WF_Rep10_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール11（申請書類(PDF)）変数設定 
        Dim O_RTN As String = ""
        GetPDFList(WF_Rep11_PDFselect, O_RTN)               'PDF選択ListBox設定
        If isNormal(O_RTN) Then
            WF_Rep11_PDFselect.SelectedIndex = 0
        Else
            Exit Sub
        End If

        '○ディテール01（管理）イベント設定 
        Dim WW_FIELD As Label = Nothing
        Dim WW_VALUE As TextBox = Nothing
        Dim WW_FIELDNM As Label = Nothing
        Dim WW_ATTR As String = ""

        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                'ダブルクリック時コード検索イベント追加
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_1"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If

                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_2"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If

                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_3"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' 詳細画面　イベント作成
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_ATTR"></param>
    ''' <remarks></remarks>
    Protected Sub ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""
        Select Case I_FIELD
            Case "CAMPCODE"
                '会社コード
                O_ATTR = "REF_Field_DBclick('CAMPCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_COMPANY & "');"
            Case "DELFLG"
                '削除フラグ
                O_ATTR = "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "');"
            Case "MANGUORG"
                '運用部署名
                O_ATTR = "REF_Field_DBclick('MANGUORG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');"
            Case "MANGMORG"
                '管理部署名
                O_ATTR = "REF_Field_DBclick('MANGMORG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');"
            Case "MANGSORG"
                '設置部署名
                O_ATTR = "REF_Field_DBclick('MANGSORG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');"
            Case "MANGOWNCODE"
                '荷主名
                O_ATTR = "REF_Field_DBclick('MANGOWNCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');"
            Case "MANGSUPPL"
                '庸車会社名
                O_ATTR = "REF_Field_DBclick('MANGSUPPL', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');"
            Case "MANGOILTYPE"
                '油種名
                O_ATTR = "REF_Field_DBclick('MANGOILTYPE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_OILTYPE & "');"
            Case "MANGOWNCONT"
                '契約区分名
                O_ATTR = "REF_Field_DBclick('MANGOWNCONT', 'WF_Rep_FIELD' , '901');"
            Case "BASELEASE"
                '車両所有名
                O_ATTR = "REF_Field_DBclick('BASELEASE', 'WF_Rep_FIELD' , '902');"
            Case "SHARYOTYPE"
                '車両タイプ
                O_ATTR = "REF_Field_DBclick('SHARYOTYPE', 'WF_Rep_FIELD' , '910');"
            Case "SHARYOTYPEF"
                '車両タイプ
                O_ATTR = "REF_Field_DBclick('SHARYOTYPEF', 'WF_Rep_FIELD' , '910');"
            Case "SHARYOTYPEB"
                '車両タイプ
                O_ATTR = "REF_Field_DBclick('SHARYOTYPEB', 'WF_Rep_FIELD' , '910');"
            Case "SHARYOTYPEB2"
                '車両タイプ2
                O_ATTR = "REF_Field_DBclick('SHARYOTYPEB2', 'WF_Rep_FIELD' , '910');"
            Case "SHARYOTYPEB3"
                '車両タイプ3
                O_ATTR = "REF_Field_DBclick('SHARYOTYPEB3', 'WF_Rep_FIELD' , '910');"
            Case "FCTRAXLE"
                'リフトアクスル名
                O_ATTR = "REF_Field_DBclick('FCTRAXLE', 'WF_Rep_FIELD' , '903');"
            Case "FCTRTMAKER"
                'タンクメーカー
                O_ATTR = "REF_Field_DBclick('FCTRTMAKER', 'WF_Rep_FIELD' , '904');"
            Case "FCTRDPR"
                'DPR名
                O_ATTR = "REF_Field_DBclick('FCTRDPR', 'WF_Rep_FIELD' , '905');"
            Case "FCTRFUELMATE"
                '燃料タンク材質名
                O_ATTR = "REF_Field_DBclick('FCTRFUELMATE', 'WF_Rep_FIELD' , '906');"
            Case "FCTRSHFTNUM"
                '軸数名
                O_ATTR = "REF_Field_DBclick('FCTRSHFTNUM', 'WF_Rep_FIELD' , '907');"
            Case "FCTRSUSP"
                'サスペンション種類名
                O_ATTR = "REF_Field_DBclick('FCTRSUSP', 'WF_Rep_FIELD' , '908');"
            Case "FCTRTMISSION"
                'ミッション名
                O_ATTR = "REF_Field_DBclick('FCTRTMISSION', 'WF_Rep_FIELD' , '909');"
            Case "FCTRUREA"
                '尿素名
                O_ATTR = "REF_Field_DBclick('FCTRUREA', 'WF_Rep_FIELD' , '911');"
            Case "OTNKBPIPE"
                '後配管名
                O_ATTR = "REF_Field_DBclick('OTNKBPIPE', 'WF_Rep_FIELD' , '912');"
            Case "OTNKCVALVE"
                '中間ﾊﾞﾙﾌﾞ有無名
                O_ATTR = "REF_Field_DBclick('OTNKCVALVE', 'WF_Rep_FIELD' , '913');"
            Case "OTNKDCD"
                'DCD装備名
                O_ATTR = "REF_Field_DBclick('OTNKDCD', 'WF_Rep_FIELD' , '914');"
            Case "FCTRSMAKER"
                '車両メーカー
                O_ATTR = "REF_Field_DBclick('FCTRSMAKER', 'WF_Rep_FIELD' , '915');"
            Case "OTNKDETECTOR"
                '検水管名
                O_ATTR = "REF_Field_DBclick('OTNKDETECTOR', 'WF_Rep_FIELD' , '916');"
            Case "OTNKDISGORGE"
                '吐出口名
                O_ATTR = "REF_Field_DBclick('OTNKDISGORGE', 'WF_Rep_FIELD' , '917');"
            Case "OTNKHTECH"
                'ハイテク種別名
                O_ATTR = "REF_Field_DBclick('OTNKHTECH', 'WF_Rep_FIELD' , '918');"
            Case "OTNKLVALVE"
                '底弁形式名
                O_ATTR = "REF_Field_DBclick('OTNKLVALVE', 'WF_Rep_FIELD' , '919');"
            Case "OTNKMATERIAL"
                'タンク材質名
                O_ATTR = "REF_Field_DBclick('OTNKMATERIAL', 'WF_Rep_FIELD' , '920');"
            Case "OTNKPIPE"
                '配管形態名
                O_ATTR = "REF_Field_DBclick('OTNKPIPE', 'WF_Rep_FIELD' , '921');"
            Case "OTNKPIPESIZE"
                '配管サイズ名
                O_ATTR = "REF_Field_DBclick('OTNKPIPESIZE', 'WF_Rep_FIELD' , '922');"
            Case "OTNKPUMP"
                'ポンプ名
                O_ATTR = "REF_Field_DBclick('OTNKPUMP', 'WF_Rep_FIELD' , '923');"
            Case "HPRSPMPDR"
                'ポンプ駆動方法
                O_ATTR = "REF_Field_DBclick('HPRSPMPDR', 'WF_Rep_FIELD' , '924');"
            Case "OTNKVAPOR"
                'ベーパー名
                O_ATTR = "REF_Field_DBclick('OTNKVAPOR', 'WF_Rep_FIELD' , '925');"
            Case "CHEMDISGORGE"
                '吐出口名
                O_ATTR = "REF_Field_DBclick('CHEMDISGORGE', 'WF_Rep_FIELD' , '926');"
            Case "CHEMHOSE"
                'ホースボックス名
                O_ATTR = "REF_Field_DBclick('CHEMHOSE', 'WF_Rep_FIELD' , '927');"
            Case "CHEMMANOMTR"
                '圧力計名
                O_ATTR = "REF_Field_DBclick('CHEMMANOMTR', 'WF_Rep_FIELD' , '928');"
            Case "CHEMMATERIAL"
                'タンク材質名
                O_ATTR = "REF_Field_DBclick('CHEMMATERIAL', 'WF_Rep_FIELD' , '929');"
            Case "CHEMPMPDR"
                'ポンプ駆動方法名
                O_ATTR = "REF_Field_DBclick('CHEMPMPDR', 'WF_Rep_FIELD' , '930');"
            Case "CHEMPRESDRV"
                '加温装置名
                O_ATTR = "REF_Field_DBclick('CHEMPRESDRV', 'WF_Rep_FIELD' , '931');"
            Case "CHEMPRESEQ"
                '均圧配管名
                O_ATTR = "REF_Field_DBclick('CHEMPRESEQ', 'WF_Rep_FIELD' , '932');"
            Case "CHEMPUMP"
                'ポンプ名
                O_ATTR = "REF_Field_DBclick('CHEMPUMP', 'WF_Rep_FIELD' , '933');"
            Case "CHEMSTRUCT"
                'タンク構造名
                O_ATTR = "REF_Field_DBclick('CHEMSTRUCT', 'WF_Rep_FIELD' , '934');"
            Case "CHEMTHERM"
                '温度計名
                O_ATTR = "REF_Field_DBclick('CHEMTHERM', 'WF_Rep_FIELD' , '935');"
            Case "HPRSINSULATE"
                '断熱構造名
                O_ATTR = "REF_Field_DBclick('HPRSINSULATE', 'WF_Rep_FIELD' , '936');"
            Case "HPRSMATR"
                'タンク材質名
                O_ATTR = "REF_Field_DBclick('HPRSMATR', 'WF_Rep_FIELD' , '937');"
            Case "HPRSPIPE"
                '配管形状（仮）名
                O_ATTR = "REF_Field_DBclick('HPRSPIPE', 'WF_Rep_FIELD' , '938');"
            Case "HPRSPIPENUM"
                '配管口数名
                O_ATTR = "REF_Field_DBclick('HPRSPIPENUM', 'WF_Rep_FIELD' , '939');"
            Case "HPRSPUMP"
                'ポンプ名
                O_ATTR = "REF_Field_DBclick('HPRSPUMP', 'WF_Rep_FIELD' , '940');"
            Case "HPRSRESSRE"
                '加圧器名
                O_ATTR = "REF_Field_DBclick('HPRSRESSRE', 'WF_Rep_FIELD' , '941');"
            Case "HPRSSTRUCT"
                'タンク構造名
                O_ATTR = "REF_Field_DBclick('HPRSSTRUCT', 'WF_Rep_FIELD' , '942');"
            Case "HPRSVALVE"
                '底弁形式名
                O_ATTR = "REF_Field_DBclick('HPRSVALVE', 'WF_Rep_FIELD' , '943');"
            Case "OTHRBMONITOR"
                'バックモニター名
                O_ATTR = "REF_Field_DBclick('OTHRBMONITOR', 'WF_Rep_FIELD' , '944');"
            Case "OTHRBSONAR"
                'バックソナー名
                O_ATTR = "REF_Field_DBclick('OTHRBSONAR', 'WF_Rep_FIELD' , '945');"
            Case "FCTRTIRE"
                'タイヤメーカー名
                O_ATTR = "REF_Field_DBclick('FCTRTIRE', 'WF_Rep_FIELD' , '946');"
            Case "OTHRDRRECORD"
                'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
                O_ATTR = "REF_Field_DBclick('OTHRDRRECORD', 'WF_Rep_FIELD' , '947');"
            Case "OTHRPAINTING"
                '塗装名
                O_ATTR = "REF_Field_DBclick('OTHRPAINTING', 'WF_Rep_FIELD' , '948');"
            Case "OTHRRADIOCON"
                '無線（有・無）名
                O_ATTR = "REF_Field_DBclick('OTHRRADIOCON', 'WF_Rep_FIELD' , '949');"
            Case "OTHRRTARGET"
                '一括修理非対象車名
                O_ATTR = "REF_Field_DBclick('OTHRRTARGET', 'WF_Rep_FIELD' , '950');"
            Case "OTHRTERMINAL"
                '車載端末名
                O_ATTR = "REF_Field_DBclick('OTHRTERMINAL', 'WF_Rep_FIELD' , '951');"
            Case "LICNPLTNO1"
                '登録番号(陸運局)
                O_ATTR = "REF_Field_DBclick('LICNPLTNO1', 'WF_Rep_FIELD' , '952');"
            Case "OTNKEXHASIZE"
                '吐出口サイズ
                O_ATTR = "REF_Field_DBclick('OTNKEXHASIZE', 'WF_Rep_FIELD' , '953');"
            Case "HPRSHOSE"
                'ホースボックス
                O_ATTR = "REF_Field_DBclick('HPRSHOSE', 'WF_Rep_FIELD' , '954');"
            Case "CONTSHAPE"
                'シャーシ形状
                O_ATTR = "REF_Field_DBclick('CONTSHAPE', 'WF_Rep_FIELD' , '955');"
            Case "CONTPUMP"
                'ポンプ
                O_ATTR = "REF_Field_DBclick('CONTPUMP', 'WF_Rep_FIELD' , '956');"
            Case "CONTPMPDR"
                'ポポンプ駆動方法
                O_ATTR = "REF_Field_DBclick('CONTPMPDR', 'WF_Rep_FIELD' , '957');"
            Case "OTHRTPMS"
                'TPMS
                O_ATTR = "REF_Field_DBclick('OTHRTPMS', 'WF_Rep_FIELD' , '958');"
            Case "MANGPROD1"
                '品名１
                O_ATTR = "REF_Field_DBclick('MANGPROD1', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_GOODS & "');"
            Case "MANGPROD2"
                '品名２
                O_ATTR = "REF_Field_DBclick('MANGPROD2', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_GOODS & "');"
            Case "BASERDATE"
                '登録日新規
                O_ATTR = "REF_Field_DBclick('BASERDATE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "OTNKINSYMD"
                'タンク検査年月日
                O_ATTR = "REF_Field_DBclick('OTNKINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "HPRSINSIYMD"
                '容器検査初回年月日
                O_ATTR = "REF_Field_DBclick('HPRSINSIYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "CHEMINSYMD"
                'タンク検査年月日
                O_ATTR = "REF_Field_DBclick('CHEMINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "LICNYMD"
                '車検有効期限年月日
                O_ATTR = "REF_Field_DBclick('LICNYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "TAXLINSYMD"
                '自賠責期限年月日
                O_ATTR = "REF_Field_DBclick('TAXLINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "TAXLINSYMD"
                '自賠責期限年月日
                O_ATTR = "REF_Field_DBclick('TAXLINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "OTNKTINSYMD"
                '気密検査年月日
                O_ATTR = "REF_Field_DBclick('OTNKTINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "OTNKTINSNYMD"
                '次回気密検査検査年月日
                O_ATTR = "REF_Field_DBclick('OTNKTINSNYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "HPRSJINSYMD"
                '定期自主検査年月日
                O_ATTR = "REF_Field_DBclick('HPRSJINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "HPRSINSYMD"
                '容器再検査年月日
                O_ATTR = "REF_Field_DBclick('HPRSINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "HPRSINSNYMD"
                '次回容器再検査年月日
                O_ATTR = "REF_Field_DBclick('HPRSINSNYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "CHEMTINSYMD"
                '気密検査年月日
                O_ATTR = "REF_Field_DBclick('CHEMTINSYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "CHEMTINSNYMD"
                '次回気密検査検査年月日
                O_ATTR = "REF_Field_DBclick('CHEMTINSNYMD', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_CALENDAR & "');"
            Case "OTNKTMAKER"
                '石油タンクメーカー
                O_ATTR = "REF_Field_DBclick('OTNKTMAKER', 'WF_Rep_FIELD' , '959');"
            Case "HPRSTMAKER"
                '高圧タンクメーカー
                O_ATTR = "REF_Field_DBclick('HPRSTMAKER', 'WF_Rep_FIELD' , '960');"
            Case "CHEMTMAKER"
                '化成品タンクメーカー
                O_ATTR = "REF_Field_DBclick('CHEMTMAKER', 'WF_Rep_FIELD' , '961');"
            Case "CONTTMAKER"
                'コンテナタンクメーカー
                O_ATTR = "REF_Field_DBclick('CONTTMAKER', 'WF_Rep_FIELD' , '962');"
            Case "SHARYOSTATUS"
                '運行状況
                O_ATTR = "REF_Field_DBclick('SHARYOSTATUS', 'WF_Rep_FIELD' , '963');"
            Case "INSKBN"
                '検査区分
                O_ATTR = "REF_Field_DBclick('INSKBN', 'WF_Rep_FIELD' , '964');"

        End Select

    End Sub
    ''' <summary>
    ''' タブ指定時表示判定処理
    ''' </summary>
    ''' <param name="I_SHARYOTYPE"></param>
    ''' <remarks></remarks>
    Protected Sub TAB_DisplayCTRL(ByVal I_SHARYOTYPE As String)
        Const C_SHARYOTYPE_FRONT As String = "前"
        Const C_SHARYOTYPE_BACK As String = "後"
        Dim WW_RESULT As String = ""
        Dim WW_SHARYOTYPE2 As String = ""
        Dim WW_MANGOILTYPE As String = ""

        WF_DViewRep1.Visible = False
        WF_DViewRep2.Visible = False
        WF_DViewRep3.Visible = False
        WF_DViewRep4.Visible = False
        WF_DViewRep5.Visible = False
        WF_DViewRep6.Visible = False
        WF_DViewRep7.Visible = False
        WF_DViewRep8.Visible = False
        WF_DViewRep9.Visible = False
        WF_DViewRep10.Visible = False

        'Repeataerより油種を取得
        Repeater_ItemFIND("MANGOILTYPE", "1", WW_MANGOILTYPE)

        GetSharyoType2(I_SHARYOTYPE, WW_SHARYOTYPE2, WW_RTN_SW)
        If isNormal(WW_RTN_SW) Then

            Select Case WF_DetailMView.ActiveViewIndex
                Case 0
                    WF_DViewRep1.Visible = True
                Case 1
                    WF_DViewRep2.Visible = True
                Case 2
                    If WW_SHARYOTYPE2 = C_SHARYOTYPE_FRONT Then
                        WF_DViewRep3.Visible = True
                    End If
                Case 3
                    If WW_SHARYOTYPE2 = C_SHARYOTYPE_BACK AndAlso WW_MANGOILTYPE = "01" Then  '石油
                        WF_DViewRep4.Visible = True
                    End If
                Case 4
                    If WW_SHARYOTYPE2 = C_SHARYOTYPE_BACK AndAlso WW_MANGOILTYPE = "02" Then  '高圧
                        WF_DViewRep5.Visible = True
                    End If
                Case 5
                    If WW_SHARYOTYPE2 = C_SHARYOTYPE_BACK AndAlso WW_MANGOILTYPE = "03" Then  '化成品
                        WF_DViewRep6.Visible = True
                    End If
                Case 6
                    If WW_SHARYOTYPE2 = C_SHARYOTYPE_BACK AndAlso WW_MANGOILTYPE = "04" Then  'コンテナ
                        WF_DViewRep7.Visible = True
                    End If
                Case 7
                    WF_DViewRep8.Visible = True
                Case 8
                    WF_DViewRep9.Visible = True
                Case 9
                    WF_DViewRep10.Visible = True
            End Select
        Else
            '管理は、新規入力するため必ず表示する
            Select Case WF_DetailMView.ActiveViewIndex
                Case 0
                    WF_DViewRep1.Visible = True
            End Select
        End If

    End Sub

    ''' <summary>
    ''' タブ切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        '初期値（書式）変更

        Dim WW_FILED_OBJ As Object
        WW_FILED_OBJ = CType(WF_DViewRep1.Items(0).FindControl("WF_Rep1_VALUE_1"), TextBox)
        If Not WW_FILED_OBJ.ReadOnly Then
            '管理
            WF_Dtab01.Style.Remove("color")
            WF_Dtab01.Style.Add("color", "black")
            WF_Dtab01.Style.Remove("background-color")
            WF_Dtab01.Style.Add("background-color", "rgb(255,255,253)")
            WF_Dtab01.Style.Remove("border")
            WF_Dtab01.Style.Add("border", "1px solid black")
            WF_Dtab01.Style.Remove("font-weight")
            WF_Dtab01.Style.Add("font-weight", "normal")
        Else
            WF_Dtab01.Style.Remove("color")
            WF_Dtab01.Style.Add("color", "black")
            WF_Dtab01.Style.Remove("background-color")
            WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
            WF_Dtab01.Style.Remove("border")
            WF_Dtab01.Style.Add("border", "1px solid black")
            WF_Dtab01.Style.Remove("font-weight")
            WF_Dtab01.Style.Add("font-weight", "normal")
        End If

        '連結車番
        WF_Dtab02.Style.Remove("color")
        WF_Dtab02.Style.Add("color", "black")
        WF_Dtab02.Style.Remove("background-color")
        WF_Dtab02.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab02.Style.Remove("border")
        WF_Dtab02.Style.Add("border", "1px solid black")
        WF_Dtab02.Style.Remove("font-weight")
        WF_Dtab02.Style.Add("font-weight", "normal")

        '車両緒元
        WF_Dtab03.Style.Remove("color")
        WF_Dtab03.Style.Add("color", "black")
        WF_Dtab03.Style.Remove("background-color")
        WF_Dtab03.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab03.Style.Remove("border")
        WF_Dtab03.Style.Add("border", "1px solid black")
        WF_Dtab03.Style.Remove("font-weight")
        WF_Dtab03.Style.Add("font-weight", "normal")

        '石油タンク
        WF_Dtab04.Style.Remove("color")
        WF_Dtab04.Style.Add("color", "black")
        WF_Dtab04.Style.Remove("background-color")
        WF_Dtab04.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab04.Style.Remove("border")
        WF_Dtab04.Style.Add("border", "1px solid black")
        WF_Dtab04.Style.Remove("font-weight")
        WF_Dtab04.Style.Add("font-weight", "normal")

        '高圧タンク
        WF_Dtab05.Style.Remove("color")
        WF_Dtab05.Style.Add("color", "black")
        WF_Dtab05.Style.Remove("background-color")
        WF_Dtab05.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab05.Style.Remove("border")
        WF_Dtab05.Style.Add("border", "1px solid black")
        WF_Dtab05.Style.Remove("font-weight")
        WF_Dtab05.Style.Add("font-weight", "normal")

        '化成品タンク
        WF_Dtab06.Style.Remove("color")
        WF_Dtab06.Style.Add("color", "black")
        WF_Dtab06.Style.Remove("background-color")
        WF_Dtab06.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab06.Style.Remove("border")
        WF_Dtab06.Style.Add("border", "1px solid black")
        WF_Dtab06.Style.Remove("font-weight")
        WF_Dtab06.Style.Add("font-weight", "normal")

        'コンテナ
        WF_Dtab07.Style.Remove("color")
        WF_Dtab07.Style.Add("color", "black")
        WF_Dtab07.Style.Remove("background-color")
        WF_Dtab07.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab07.Style.Remove("border")
        WF_Dtab07.Style.Add("border", "1px solid black")
        WF_Dtab07.Style.Remove("font-weight")
        WF_Dtab07.Style.Add("font-weight", "normal")

        '車両その他
        WF_Dtab08.Style.Remove("color")
        WF_Dtab08.Style.Add("color", "black")
        WF_Dtab08.Style.Remove("background-color")
        WF_Dtab08.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab08.Style.Remove("border")
        WF_Dtab08.Style.Add("border", "1px solid black")
        WF_Dtab08.Style.Remove("font-weight")
        WF_Dtab08.Style.Add("font-weight", "normal")

        '経理
        WF_Dtab09.Style.Remove("color")
        WF_Dtab09.Style.Add("color", "black")
        WF_Dtab09.Style.Remove("background-color")
        WF_Dtab09.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab09.Style.Remove("border")
        WF_Dtab09.Style.Add("border", "1px solid black")
        WF_Dtab09.Style.Remove("font-weight")
        WF_Dtab09.Style.Add("font-weight", "normal")

        '申請
        WF_Dtab10.Style.Remove("color")
        WF_Dtab10.Style.Add("color", "black")
        WF_Dtab10.Style.Remove("background-color")
        WF_Dtab10.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab10.Style.Remove("border")
        WF_Dtab10.Style.Add("border", "1px solid black")
        WF_Dtab10.Style.Remove("font-weight")
        WF_Dtab10.Style.Add("font-weight", "normal")

        '申請書類（PDF）
        WF_Dtab11.Style.Remove("color")
        WF_Dtab11.Style.Add("color", "black")
        WF_Dtab11.Style.Remove("background-color")
        WF_Dtab11.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab11.Style.Remove("border")
        WF_Dtab11.Style.Add("border", "1px solid black")
        WF_Dtab11.Style.Remove("font-weight")
        WF_Dtab11.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                If Not WW_FILED_OBJ.ReadOnly Then
                    '管理
                    WF_Dtab01.Style.Remove("color")
                    WF_Dtab01.Style.Add("color", "blue")
                    WF_Dtab01.Style.Remove("background-color")
                    WF_Dtab01.Style.Add("background-color", "rgb(220,230,240)")
                    WF_Dtab01.Style.Remove("border")
                    WF_Dtab01.Style.Add("border", "1px solid blue")
                    WF_Dtab01.Style.Remove("font-weight")
                    WF_Dtab01.Style.Add("font-weight", "bold")
                Else
                    WF_Dtab01.Style.Remove("color")
                    WF_Dtab01.Style.Add("color", "blue")
                    WF_Dtab01.Style.Remove("background-color")
                    WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
                    WF_Dtab01.Style.Remove("border")
                    WF_Dtab01.Style.Add("border", "1px solid blue")
                    WF_Dtab01.Style.Remove("font-weight")
                    WF_Dtab01.Style.Add("font-weight", "bold")
                End If
            Case 1
                '連結車番
                WF_Dtab02.Style.Remove("color")
                WF_Dtab02.Style.Add("color", "blue")
                WF_Dtab02.Style.Remove("background-color")
                WF_Dtab02.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab02.Style.Remove("border")
                WF_Dtab02.Style.Add("border", "1px solid blue")
                WF_Dtab02.Style.Remove("font-weight")
                WF_Dtab02.Style.Add("font-weight", "bold")
            Case 2
                '車両緒元
                WF_Dtab03.Style.Remove("color")
                WF_Dtab03.Style.Add("color", "blue")
                WF_Dtab03.Style.Remove("background-color")
                WF_Dtab03.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab03.Style.Remove("border")
                WF_Dtab03.Style.Add("border", "1px solid blue")
                WF_Dtab03.Style.Remove("font-weight")
                WF_Dtab03.Style.Add("font-weight", "bold")
            Case 3
                '石油タンク
                WF_Dtab04.Style.Remove("color")
                WF_Dtab04.Style.Add("color", "blue")
                WF_Dtab04.Style.Remove("background-color")
                WF_Dtab04.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab04.Style.Remove("border")
                WF_Dtab04.Style.Add("border", "1px solid blue")
                WF_Dtab04.Style.Remove("font-weight")
                WF_Dtab04.Style.Add("font-weight", "bold")
            Case 4
                '高圧タンク
                WF_Dtab05.Style.Remove("color")
                WF_Dtab05.Style.Add("color", "blue")
                WF_Dtab05.Style.Remove("background-color")
                WF_Dtab05.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab05.Style.Remove("border")
                WF_Dtab05.Style.Add("border", "1px solid blue")
                WF_Dtab05.Style.Remove("font-weight")
                WF_Dtab05.Style.Add("font-weight", "bold")
            Case 5
                '化成品タンク
                WF_Dtab06.Style.Remove("color")
                WF_Dtab06.Style.Add("color", "blue")
                WF_Dtab06.Style.Remove("background-color")
                WF_Dtab06.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab06.Style.Remove("border")
                WF_Dtab06.Style.Add("border", "1px solid blue")
                WF_Dtab06.Style.Remove("font-weight")
                WF_Dtab06.Style.Add("font-weight", "bold")
            Case 6
                'コンテナ
                WF_Dtab07.Style.Remove("color")
                WF_Dtab07.Style.Add("color", "blue")
                WF_Dtab07.Style.Remove("background-color")
                WF_Dtab07.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab07.Style.Remove("border")
                WF_Dtab07.Style.Add("border", "1px solid blue")
                WF_Dtab07.Style.Remove("font-weight")
                WF_Dtab07.Style.Add("font-weight", "bold")
            Case 7
                '車両その他
                WF_Dtab08.Style.Remove("color")
                WF_Dtab08.Style.Add("color", "blue")
                WF_Dtab08.Style.Remove("background-color")
                WF_Dtab08.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab08.Style.Remove("border")
                WF_Dtab08.Style.Add("border", "1px solid blue")
                WF_Dtab08.Style.Remove("font-weight")
                WF_Dtab08.Style.Add("font-weight", "bold")
            Case 8
                '経理
                WF_Dtab09.Style.Remove("color")
                WF_Dtab09.Style.Add("color", "blue")
                WF_Dtab09.Style.Remove("background-color")
                WF_Dtab09.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab09.Style.Remove("border")
                WF_Dtab09.Style.Add("border", "1px solid blue")
                WF_Dtab09.Style.Remove("font-weight")
                WF_Dtab09.Style.Add("font-weight", "bold")
            Case 9
                '申請
                WF_Dtab10.Style.Remove("color")
                WF_Dtab10.Style.Add("color", "blue")
                WF_Dtab10.Style.Remove("background-color")
                WF_Dtab10.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab10.Style.Remove("border")
                WF_Dtab10.Style.Add("border", "1px solid blue")
                WF_Dtab10.Style.Remove("font-weight")
                WF_Dtab10.Style.Add("font-weight", "bold")
            Case 10
                '申請書類（PDF）
                WF_Dtab11.Style.Remove("color")
                WF_Dtab11.Style.Add("color", "blue")
                WF_Dtab11.Style.Remove("background-color")
                WF_Dtab11.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab11.Style.Remove("border")
                WF_Dtab11.Style.Add("border", "1px solid blue")
                WF_Dtab11.Style.Remove("font-weight")
                WF_Dtab11.Style.Add("font-weight", "bold")
        End Select
    End Sub

    ' ******************************************************************************
    ' ***  rightBOX関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '○RightBox処理（ラジオボタン選択）
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
        '○RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()
    End Sub
    ''' <summary>
    ''' 左ボックス選択ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '車両タイプ
                Case "WF_SELSHARYOTYPE"
                    WF_SELSHARYOTYPE.Text = WW_SelectValue
                    WF_SELSHARYOTYPE_TEXT.Text = WW_SelectTEXT
                    WF_SELSHARYOTYPE.Focus()

                    '管理組織
                Case "WF_SELMORG"
                    WF_SELMORG.Text = WW_SelectValue
                    WF_SELMORG_TEXT.Text = WW_SelectTEXT
                    WF_SELMORG.Focus()

                    '会社
                Case "WF_CAMPCODE"
                    WF_CAMPCODE.Text = WW_SelectValue
                    WF_CAMPCODE_TEXT.Text = WW_SelectTEXT
                    WF_CAMPCODE.Focus()

                    '統一車番
                Case "WF_SHARYOTYPE"
                    WF_SHARYOTYPE.Text = WW_SelectValue
                    WF_TSHABAN1_TEXT.Text = WW_SelectTEXT
                    WF_SHARYOTYPE.Focus()

                    '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectTEXT
                    WF_DELFLG.Focus()
                Case "WF_STYMD"
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_STYMD.Text = ""
                        Else
                            WF_STYMD.Text = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try
                    WF_STYMD.Focus()
                Case "WF_ENDYMD"
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ENDYMD.Text = ""
                        Else
                            WF_ENDYMD.Text = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception

                    End Try
                    WF_ENDYMD.Focus()

            End Select

        Else
            Select Case WF_FIELD_REP.Value
                Case "BASERDATE", "OTNKINSYMD", "HPRSINSIYMD", "CHEMINSYMD", "LICNYMD", "TAXLINSYMD", "TAXLINSYMD", "OTNKTINSYMD", "OTNKTINSNYMD", "HPRSJINSYMD", "HPRSINSYMD", "HPRSINSNYMD", "CHEMTINSYMD", "CHEMTINSNYMD"
                    WW_SelectValue = leftview.WF_Calendar.Text
                    WW_SelectTEXT = ""
            End Select

            '○ディテール01（管理）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール02（連結車番）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep2.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep2_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep2_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep2_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep2_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep2_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep2_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep2_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep2_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep2_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep2_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep2_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep2_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール03（車両緒元）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep3.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep3_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep3_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep3_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep3_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep3_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep3_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep3_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep3_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep3_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep3_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep3_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep3_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール04（石油タンク）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep4.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep4_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep4_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep4_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep4_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep4_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep4_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep4_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep4_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep4_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep4_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep4_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep4_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール05（高圧タンク）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep5.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep5_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep5_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep5_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep5_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep5_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep5_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep5_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep5_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep5_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep5_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep5_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep5_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール06（化成品タンク）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep6.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep6_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep6_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep6_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep6_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep6_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep6_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep6_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep6_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep6_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep6_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep6_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep6_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール07（コンテナ）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep7.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep7_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep7_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep7_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep7_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep7_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep7_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep7_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep7_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep7_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep7_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep7_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep7_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール08（車両その他）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep8.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep8_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep8_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep8_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep8_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep8_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep8_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep8_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep8_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep8_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep8_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep8_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep8_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール09（経理）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep9.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep9_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep9_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep9_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep9_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep9_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep9_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep9_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep9_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep9_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep9_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep9_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep9_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール10（申請）変数設定 
            For Each reitem As RepeaterItem In WF_DViewRep10.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep10_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep10_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep10_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep10_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep10_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep10_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep10_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep10_VALUE_3"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep10_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep10_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep10_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep10_VALUE_2"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール11（PDF）変数設定 
            If WF_FIELD_REP.Value = "WF_Rep_DELFLG" Then
                For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
                    '***********  左サイド　***********
                    If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_FIELD.Value Then
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = WW_SelectValue
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Focus()
                        Exit For
                    End If
                Next
            End If

        End If

        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()

        Dim WW_YMD As String = ""
        Dim WW_MANGMORG As String = ""
        Dim WW_LeftMView As Integer = Nothing

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WW_LeftMView)
            Catch ex As Exception
                Exit Sub
            End Try
            '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
            With leftview
                If WW_LeftMView <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text, WW_LeftMView)
                    Select Case WW_LeftMView
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            If WF_FIELD_REP.Value = "" OrElse WF_FIELD_REP.Value = "MANGMORG" Then
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, True)
                            Else
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, False)
                            End If

                        Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                            If WF_FIELD_REP.Value = "MANGOWNCODE" Then
                                prmData = work.CreateTODOParam(work.WF_SEL_CAMPCODE.Text)
                            Else
                                prmData = work.CreateYOTORIParam(work.WF_SEL_CAMPCODE.Text)
                            End If
                        Case LIST_BOX_CLASSIFICATION.LC_GOODS
                            Dim WW_OILTYPE As String = ""
                            Dim WW_MANGPROD1 As String = ""
                            'タブ別処理(01 管理)から品名1を取得
                            Repeater_ItemFIND("MANGOILTYPE", "1", WW_OILTYPE)
                            'タブ別処理(01 管理)から品名1を取得
                            Repeater_ItemFIND("MANGPROD1", "1", WW_MANGPROD1)
                            If WF_FIELD_REP.Value = "MANGPROD1" Then
                                prmData = work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, WW_OILTYPE)
                            Else
                                prmData = work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, WW_OILTYPE, WW_MANGPROD1)
                            End If

                    End Select
                    .setListBox(WW_LeftMView, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            WW_YMD = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            WW_YMD = WF_ENDYMD.Text
                    End Select
                    Select Case WF_FIELD_REP.Value
                        'カレンダーの表示
                        Case "BASERDATE"
                            'タブ別処理(01 管理)から登録日新規を取得
                            Repeater_ItemFIND("BASERDATE", "1", WW_YMD)
                        Case "OTNKINSYMD"
                            'タブ別処理(04 石油タンク)からタンク検査年月日を取得
                            Repeater_ItemFIND("OTNKINSYMD", "4", WW_YMD)
                        Case "HPRSINSIYMD"
                            'タブ別処理(05 高圧タンク)から容器検査初回年月日を取得
                            Repeater_ItemFIND("HPRSINSIYMD", "5", WW_YMD)
                        Case "CHEMINSYMD"
                            'タブ別処理(06 化粧品タンク)からタンク検査年月日を取得
                            Repeater_ItemFIND("CHEMINSYMD", "6", WW_YMD)
                        Case "LICNYMD"
                            'タブ別処理(10 申請)から車検有効期限年月日を取得
                            Repeater_ItemFIND("LICNYMD", "10", WW_YMD)
                        Case "TAXLINSYMD"
                            'タブ別処理(10 申請)から自賠責期限年月日を取得
                            Repeater_ItemFIND("TAXLINSYMD", "10", WW_YMD)
                        Case "OTNKTINSYMD"
                            'タブ別処理(10 申請)から気密検査年月日を取得
                            Repeater_ItemFIND("OTNKTINSYMD", "10", WW_YMD)
                        Case "OTNKTINSNYMD"
                            'タブ別処理(10 申請)から次回気密検査検査年月日を取得
                            Repeater_ItemFIND("OTNKTINSNYMD", "10", WW_YMD)
                        Case "HPRSJINSYMD"
                            'タブ別処理(10 申請)から定期自主検査年月日を取得
                            Repeater_ItemFIND("HPRSJINSYMD", "10", WW_YMD)
                        Case "HPRSINSYMD"
                            'タブ別処理(10 申請)から容器再検査年月日を取得
                            Repeater_ItemFIND("HPRSINSYMD", "10", WW_YMD)
                        Case "HPRSINSNYMD"
                            'タブ別処理(10 申請)から次回容器再検査年月日を取得
                            Repeater_ItemFIND("HPRSINSNYMD", "10", WW_YMD)
                        Case "CHEMTINSYMD"
                            'タブ別処理(10 申請)から気密検査年月日を取得
                            Repeater_ItemFIND("CHEMTINSYMD", "10", WW_YMD)
                        Case "CHEMTINSNYMD"
                            'タブ別処理(10 申請)から次回気密検査検査年月日を取得
                            Repeater_ItemFIND("CHEMTINSNYMD", "10", WW_YMD)
                    End Select
                    .WF_Calendar.Text = WW_YMD
                    .activeCalendar()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○初期処理
        Dim WW_DATE As Date
        rightview.setErrorReport("")

        '○UPLOAD_XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMA0003WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
            Exit Sub
        End If

        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○MA0003INPtblカラム設定
        Master.CreateEmptyTable(MA0003INPtbl)

        '○Excelデータ毎にチェック＆更新
        CS0023XLSTBLrow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒MA0003INProw
            Dim MA0003INProw As DataRow = MA0003INPtbl.NewRow

            '○初期クリア
            For Each MA0003INPcol As DataColumn In MA0003INPtbl.Columns
                If IsDBNull(MA0003INProw.Item(MA0003INPcol)) OrElse IsNothing(MA0003INProw.Item(MA0003INPcol)) Then
                    Select Case MA0003INPcol.ColumnName
                        Case "LINECNT"
                            MA0003INProw.Item(MA0003INPcol) = 0
                        Case "TIMSTP"
                            MA0003INProw.Item(MA0003INPcol) = 0
                        Case "SELECT"
                            MA0003INProw.Item(MA0003INPcol) = 1
                        Case "HIDDEN"
                            MA0003INProw.Item(MA0003INPcol) = 0
                        Case "WORK_NO"
                            MA0003INProw.Item(MA0003INPcol) = 0
                        Case Else
                            If MA0003INPcol.DataType.Name = "String" Then
                                MA0003INProw.Item(MA0003INPcol) = ""
                            Else
                                MA0003INProw.Item(MA0003INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("SHARYOTYPE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TSHABAN") >= 0 Then

                For Each MA0003row As DataRow In MA0003tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MA0003row("CAMPCODE") AndAlso
                       XLSTBLrow("SHARYOTYPE") = MA0003row("SHARYOTYPE") AndAlso
                       XLSTBLrow("TSHABAN") = MA0003row("TSHABAN") Then
                        '最新レコード判定
                        If MA0003row("STYMD") = "" Then
                            If WW_STYMD < MA0003row("STYMD_B") Then
                                WW_STYMD = MA0003row("STYMD_B")
                                MA0003INProw.ItemArray = MA0003row.ItemArray
                            End If
                        Else
                            If MA0003row("STYMD") = XLSTBLrow("STYMD") Then
                                WW_STYMD = MA0003row("STYMD")
                                MA0003INProw.ItemArray = MA0003row.ItemArray
                                Exit For
                            End If
                        End If
                    End If
                Next
            End If

            '○項目セット
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MA0003INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPE") >= 0 Then
                MA0003INProw("SHARYOTYPE") = XLSTBLrow("SHARYOTYPE")
            End If

            If WW_COLUMNS.IndexOf("TSHABAN") >= 0 Then
                MA0003INProw("TSHABAN") = XLSTBLrow("TSHABAN")
            End If

            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    WW_DATE = XLSTBLrow("STYMD")
                    MA0003INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    WW_DATE = XLSTBLrow("ENDYMD")
                    MA0003INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MA0003INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("MANGUORG") >= 0 Then
                MA0003INProw("MANGUORG") = XLSTBLrow("MANGUORG")
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                MA0003INProw("GSHABAN") = XLSTBLrow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MA0003INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 Then
                MA0003INProw("SHARYOTYPEF") = XLSTBLrow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 Then
                MA0003INProw("SHARYOTYPEB") = XLSTBLrow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") >= 0 Then
                MA0003INProw("SHARYOTYPEB2") = XLSTBLrow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB3") >= 0 Then
                MA0003INProw("SHARYOTYPEB3") = XLSTBLrow("SHARYOTYPEB3")
            End If

            If WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                MA0003INProw("TSHABANF") = XLSTBLrow("TSHABANF")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                MA0003INProw("TSHABANB") = XLSTBLrow("TSHABANB")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB2") >= 0 Then
                MA0003INProw("TSHABANB2") = XLSTBLrow("TSHABANB2")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB3") >= 0 Then
                MA0003INProw("TSHABANB3") = XLSTBLrow("TSHABANB3")
            End If

            If WW_COLUMNS.IndexOf("MANGOILTYPE") >= 0 Then
                MA0003INProw("MANGOILTYPE") = XLSTBLrow("MANGOILTYPE")
            End If

            If WW_COLUMNS.IndexOf("BASERAGEYY") >= 0 Then
                MA0003INProw("BASERAGEYY") = XLSTBLrow("BASERAGEYY")
            End If

            If WW_COLUMNS.IndexOf("BASERAGEMM") >= 0 Then
                MA0003INProw("BASERAGEMM") = XLSTBLrow("BASERAGEMM")
            End If

            If WW_COLUMNS.IndexOf("BASERAGE") >= 0 Then
                MA0003INProw("BASERAGE") = XLSTBLrow("BASERAGE")
            End If

            If WW_COLUMNS.IndexOf("BASERDATE") >= 0 Then
                If IsDate(XLSTBLrow("BASERDATE")) Then
                    WW_DATE = XLSTBLrow("BASERDATE")
                    MA0003INProw("BASERDATE") = WW_DATE.ToString("yyyy/MM/dd")
                    Dim WW_DATENOW As Date = Date.Now
                    Dim WW_BASERAGEYY As Integer
                    Dim WW_BASERAGE As Integer
                    Dim WW_BASERAGEMM As Integer
                    WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                    WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                    WW_BASERAGEMM = WW_BASERAGE Mod 12
                    MA0003INProw("BASERAGEMM") = WW_BASERAGEMM
                    MA0003INProw("BASERAGEYY") = WW_BASERAGEYY
                    MA0003INProw("BASERAGE") = WW_BASERAGE
                End If
            End If

            If WW_COLUMNS.IndexOf("MANGMORG") >= 0 Then
                MA0003INProw("MANGMORG") = XLSTBLrow("MANGMORG")
            End If

            If WW_COLUMNS.IndexOf("MANGOWNCONT") >= 0 Then
                MA0003INProw("MANGOWNCONT") = XLSTBLrow("MANGOWNCONT")
            End If

            If WW_COLUMNS.IndexOf("BASELEASE") >= 0 Then
                MA0003INProw("BASELEASE") = XLSTBLrow("BASELEASE")
            End If

            If WW_COLUMNS.IndexOf("MANGSUPPL") >= 0 Then
                MA0003INProw("MANGSUPPL") = XLSTBLrow("MANGSUPPL")
            End If

            If WW_COLUMNS.IndexOf("MANGSHAFUKU") >= 0 Then
                MA0003INProw("MANGSHAFUKU") = XLSTBLrow("MANGSHAFUKU")
            End If

            If WW_COLUMNS.IndexOf("MANGSORG") >= 0 Then
                MA0003INProw("MANGSORG") = XLSTBLrow("MANGSORG")
            End If

            If WW_COLUMNS.IndexOf("MANGOWNCODE") >= 0 Then
                MA0003INProw("MANGOWNCODE") = XLSTBLrow("MANGOWNCODE")
            End If

            If WW_COLUMNS.IndexOf("MANGTTLDIST") >= 0 Then
                MA0003INProw("MANGTTLDIST") = XLSTBLrow("MANGTTLDIST")
            End If

            If WW_COLUMNS.IndexOf("ACCTRCYCLE") >= 0 Then
                MA0003INProw("ACCTRCYCLE") = XLSTBLrow("ACCTRCYCLE")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST06") >= 0 Then
                MA0003INProw("ACCTASST06") = XLSTBLrow("ACCTASST06")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST07") >= 0 Then
                MA0003INProw("ACCTASST07") = XLSTBLrow("ACCTASST07")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST08") >= 0 Then
                MA0003INProw("ACCTASST08") = XLSTBLrow("ACCTASST08")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST09") >= 0 Then
                MA0003INProw("ACCTASST09") = XLSTBLrow("ACCTASST09")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST10") >= 0 Then
                MA0003INProw("ACCTASST10") = XLSTBLrow("ACCTASST10")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE1") >= 0 Then
                MA0003INProw("ACCTLEASE1") = XLSTBLrow("ACCTLEASE1")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE4") >= 0 Then
                MA0003INProw("ACCTLEASE4") = XLSTBLrow("ACCTLEASE4")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL2") >= 0 Then
                MA0003INProw("ACCTLSUPL2") = XLSTBLrow("ACCTLSUPL2")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL5") >= 0 Then
                MA0003INProw("ACCTLSUPL5") = XLSTBLrow("ACCTLSUPL5")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST01") >= 0 Then
                MA0003INProw("ACCTASST01") = XLSTBLrow("ACCTASST01")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST02") >= 0 Then
                MA0003INProw("ACCTASST02") = XLSTBLrow("ACCTASST02")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST03") >= 0 Then
                MA0003INProw("ACCTASST03") = XLSTBLrow("ACCTASST03")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST04") >= 0 Then
                MA0003INProw("ACCTASST04") = XLSTBLrow("ACCTASST04")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST05") >= 0 Then
                MA0003INProw("ACCTASST05") = XLSTBLrow("ACCTASST05")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE2") >= 0 Then
                MA0003INProw("ACCTLEASE2") = XLSTBLrow("ACCTLEASE2")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE5") >= 0 Then
                MA0003INProw("ACCTLEASE5") = XLSTBLrow("ACCTLEASE5")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL3") >= 0 Then
                MA0003INProw("ACCTLSUPL3") = XLSTBLrow("ACCTLSUPL3")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE3") >= 0 Then
                MA0003INProw("ACCTLEASE3") = XLSTBLrow("ACCTLEASE3")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL1") >= 0 Then
                MA0003INProw("ACCTLSUPL1") = XLSTBLrow("ACCTLSUPL1")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL4") >= 0 Then
                MA0003INProw("ACCTLSUPL4") = XLSTBLrow("ACCTLSUPL4")
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSNO") >= 0 Then
                MA0003INProw("CHEMTINSNO") = XLSTBLrow("CHEMTINSNO")
            End If

            If WW_COLUMNS.IndexOf("CHEMCELLNO") >= 0 Then
                MA0003INProw("CHEMCELLNO") = XLSTBLrow("CHEMCELLNO")
            End If

            If WW_COLUMNS.IndexOf("CHEMMATERIAL") >= 0 Then
                MA0003INProw("CHEMMATERIAL") = XLSTBLrow("CHEMMATERIAL")
            End If

            If WW_COLUMNS.IndexOf("CHEMDISGORGE") >= 0 Then
                MA0003INProw("CHEMDISGORGE") = XLSTBLrow("CHEMDISGORGE")
            End If

            If WW_COLUMNS.IndexOf("CHEMPMPDR") >= 0 Then
                MA0003INProw("CHEMPMPDR") = XLSTBLrow("CHEMPMPDR")
            End If

            If WW_COLUMNS.IndexOf("CHEMPUMP") >= 0 Then
                MA0003INProw("CHEMPUMP") = XLSTBLrow("CHEMPUMP")
            End If

            If WW_COLUMNS.IndexOf("CHEMINSSTAT") >= 0 Then
                MA0003INProw("CHEMINSSTAT") = XLSTBLrow("CHEMINSSTAT")
            End If

            If WW_COLUMNS.IndexOf("CHEMCELPART") >= 0 Then
                MA0003INProw("CHEMCELPART") = XLSTBLrow("CHEMCELPART")
            End If

            If WW_COLUMNS.IndexOf("CHEMSTRUCT") >= 0 Then
                MA0003INProw("CHEMSTRUCT") = XLSTBLrow("CHEMSTRUCT")
            End If

            If WW_COLUMNS.IndexOf("CHEMHOSE") >= 0 Then
                MA0003INProw("CHEMHOSE") = XLSTBLrow("CHEMHOSE")
            End If

            If WW_COLUMNS.IndexOf("CHEMPRESDRV") >= 0 Then
                MA0003INProw("CHEMPRESDRV") = XLSTBLrow("CHEMPRESDRV")
            End If

            If WW_COLUMNS.IndexOf("CHEMTHERM") >= 0 Then
                MA0003INProw("CHEMTHERM") = XLSTBLrow("CHEMTHERM")
            End If

            If WW_COLUMNS.IndexOf("CHEMINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMINSYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMINSYMD")
                    MA0003INProw("CHEMINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMMANOMTR") >= 0 Then
                MA0003INProw("CHEMMANOMTR") = XLSTBLrow("CHEMMANOMTR")
            End If

            If WW_COLUMNS.IndexOf("CHEMPRESEQ") >= 0 Then
                MA0003INProw("CHEMPRESEQ") = XLSTBLrow("CHEMPRESEQ")
            End If

            If WW_COLUMNS.IndexOf("FCTRSHFTNUM") >= 0 Then
                MA0003INProw("FCTRSHFTNUM") = XLSTBLrow("FCTRSHFTNUM")
            End If

            If WW_COLUMNS.IndexOf("FCTRFUELCAPA") >= 0 Then
                MA0003INProw("FCTRFUELCAPA") = XLSTBLrow("FCTRFUELCAPA")
            End If

            If WW_COLUMNS.IndexOf("FCTRSUSP") >= 0 Then
                MA0003INProw("FCTRSUSP") = XLSTBLrow("FCTRSUSP")
            End If

            If WW_COLUMNS.IndexOf("FCTRUREA") >= 0 Then
                MA0003INProw("FCTRUREA") = XLSTBLrow("FCTRUREA")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE1") >= 0 Then
                MA0003INProw("FCTRRESERVE1") = XLSTBLrow("FCTRRESERVE1")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE4") >= 0 Then
                MA0003INProw("FCTRRESERVE4") = XLSTBLrow("FCTRRESERVE4")
            End If

            If WW_COLUMNS.IndexOf("FCTRAXLE") >= 0 Then
                MA0003INProw("FCTRAXLE") = XLSTBLrow("FCTRAXLE")
            End If

            If WW_COLUMNS.IndexOf("FCTRFUELMATE") >= 0 Then
                MA0003INProw("FCTRFUELMATE") = XLSTBLrow("FCTRFUELMATE")
            End If

            If WW_COLUMNS.IndexOf("FCTRTIRE") >= 0 Then
                MA0003INProw("FCTRTIRE") = XLSTBLrow("FCTRTIRE")
            End If

            If WW_COLUMNS.IndexOf("FCTRDPR") >= 0 Then
                MA0003INProw("FCTRDPR") = XLSTBLrow("FCTRDPR")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE2") >= 0 Then
                MA0003INProw("FCTRRESERVE2") = XLSTBLrow("FCTRRESERVE2")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE5") >= 0 Then
                MA0003INProw("FCTRRESERVE5") = XLSTBLrow("FCTRRESERVE5")
            End If

            If WW_COLUMNS.IndexOf("FCTRTMISSION") >= 0 Then
                MA0003INProw("FCTRTMISSION") = XLSTBLrow("FCTRTMISSION")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE3") >= 0 Then
                MA0003INProw("FCTRRESERVE3") = XLSTBLrow("FCTRRESERVE3")
            End If

            If WW_COLUMNS.IndexOf("HPRSSERNO") >= 0 Then
                MA0003INProw("HPRSSERNO") = XLSTBLrow("HPRSSERNO")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSISTAT") >= 0 Then
                MA0003INProw("HPRSINSISTAT") = XLSTBLrow("HPRSINSISTAT")
            End If

            If WW_COLUMNS.IndexOf("HPRSSTRUCT") >= 0 Then
                MA0003INProw("HPRSSTRUCT") = XLSTBLrow("HPRSSTRUCT")
            End If

            If WW_COLUMNS.IndexOf("HPRSPIPE") >= 0 Then
                MA0003INProw("HPRSPIPE") = XLSTBLrow("HPRSPIPE")
            End If

            If WW_COLUMNS.IndexOf("HPRSPUMP") >= 0 Then
                MA0003INProw("HPRSPUMP") = XLSTBLrow("HPRSPUMP")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSIYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSIYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSIYMD")
                    MA0003INProw("HPRSINSIYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSMATR") >= 0 Then
                MA0003INProw("HPRSMATR") = XLSTBLrow("HPRSMATR")
            End If

            If WW_COLUMNS.IndexOf("HPRSPIPENUM") >= 0 Then
                MA0003INProw("HPRSPIPENUM") = XLSTBLrow("HPRSPIPENUM")
            End If

            If WW_COLUMNS.IndexOf("HPRSRESSRE") >= 0 Then
                MA0003INProw("HPRSRESSRE") = XLSTBLrow("HPRSRESSRE")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSULATE") >= 0 Then
                MA0003INProw("HPRSINSULATE") = XLSTBLrow("HPRSINSULATE")
            End If

            If WW_COLUMNS.IndexOf("HPRSVALVE") >= 0 Then
                MA0003INProw("HPRSVALVE") = XLSTBLrow("HPRSVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTHRBMONITOR") >= 0 Then
                MA0003INProw("OTHRBMONITOR") = XLSTBLrow("OTHRBMONITOR")
            End If

            If WW_COLUMNS.IndexOf("OTHRDOCO") >= 0 Then
                MA0003INProw("OTHRDOCO") = XLSTBLrow("OTHRDOCO")
            End If

            If WW_COLUMNS.IndexOf("OTHRPAINTING") >= 0 Then
                MA0003INProw("OTHRPAINTING") = XLSTBLrow("OTHRPAINTING")
            End If

            If WW_COLUMNS.IndexOf("OTHRRTARGET") >= 0 Then
                MA0003INProw("OTHRRTARGET") = XLSTBLrow("OTHRRTARGET")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE1") >= 0 Then
                MA0003INProw("OFFCRESERVE1") = XLSTBLrow("OFFCRESERVE1")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE4") >= 0 Then
                MA0003INProw("OFFCRESERVE4") = XLSTBLrow("OFFCRESERVE4")
            End If

            If WW_COLUMNS.IndexOf("OTHRBSONAR") >= 0 Then
                MA0003INProw("OTHRBSONAR") = XLSTBLrow("OTHRBSONAR")
            End If

            If WW_COLUMNS.IndexOf("OTHRDRRECORD") >= 0 Then
                MA0003INProw("OTHRDRRECORD") = XLSTBLrow("OTHRDRRECORD")
            End If

            If WW_COLUMNS.IndexOf("OTHRRADIOCON") >= 0 Then
                MA0003INProw("OTHRRADIOCON") = XLSTBLrow("OTHRRADIOCON")
            End If

            If WW_COLUMNS.IndexOf("OTHRTERMINAL") >= 0 Then
                MA0003INProw("OTHRTERMINAL") = XLSTBLrow("OTHRTERMINAL")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE2") >= 0 Then
                MA0003INProw("OFFCRESERVE2") = XLSTBLrow("OFFCRESERVE2")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE5") >= 0 Then
                MA0003INProw("OFFCRESERVE5") = XLSTBLrow("OFFCRESERVE5")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE3") >= 0 Then
                MA0003INProw("OFFCRESERVE3") = XLSTBLrow("OFFCRESERVE3")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSNO") >= 0 Then
                MA0003INProw("OTNKTINSNO") = XLSTBLrow("OTNKTINSNO")
            End If

            If WW_COLUMNS.IndexOf("OTNKCELLNO") >= 0 Then
                MA0003INProw("OTNKCELLNO") = XLSTBLrow("OTNKCELLNO")
            End If

            If WW_COLUMNS.IndexOf("OTNKMATERIAL") >= 0 Then
                MA0003INProw("OTNKMATERIAL") = XLSTBLrow("OTNKMATERIAL")
            End If

            If WW_COLUMNS.IndexOf("OTNKPIPE") >= 0 Then
                MA0003INProw("OTNKPIPE") = XLSTBLrow("OTNKPIPE")
            End If

            If WW_COLUMNS.IndexOf("OTNKBPIPE") >= 0 Then
                MA0003INProw("OTNKBPIPE") = XLSTBLrow("OTNKBPIPE")
            End If

            If WW_COLUMNS.IndexOf("OTNKDISGORGE") >= 0 Then
                MA0003INProw("OTNKDISGORGE") = XLSTBLrow("OTNKDISGORGE")
            End If

            If WW_COLUMNS.IndexOf("OTNKDCD") >= 0 Then
                MA0003INProw("OTNKDCD") = XLSTBLrow("OTNKDCD")
            End If

            If WW_COLUMNS.IndexOf("OTNKINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKINSYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKINSYMD")
                    MA0003INProw("OTNKINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("OTNKCELPART") >= 0 Then
                MA0003INProw("OTNKCELPART") = XLSTBLrow("OTNKCELPART")
            End If

            If WW_COLUMNS.IndexOf("OTNKPIPESIZE") >= 0 Then
                MA0003INProw("OTNKPIPESIZE") = XLSTBLrow("OTNKPIPESIZE")
            End If

            If WW_COLUMNS.IndexOf("OTNKCVALVE") >= 0 Then
                MA0003INProw("OTNKCVALVE") = XLSTBLrow("OTNKCVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTNKLVALVE") >= 0 Then
                MA0003INProw("OTNKLVALVE") = XLSTBLrow("OTNKLVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTNKINSSTAT") >= 0 Then
                MA0003INProw("OTNKINSSTAT") = XLSTBLrow("OTNKINSSTAT")
            End If

            If WW_COLUMNS.IndexOf("OTNKPUMP") >= 0 Then
                MA0003INProw("OTNKPUMP") = XLSTBLrow("OTNKPUMP")
            End If

            If WW_COLUMNS.IndexOf("OTNKDETECTOR") >= 0 Then
                MA0003INProw("OTNKDETECTOR") = XLSTBLrow("OTNKDETECTOR")
            End If

            If WW_COLUMNS.IndexOf("OTNKVAPOR") >= 0 Then
                MA0003INProw("OTNKVAPOR") = XLSTBLrow("OTNKVAPOR")
            End If

            If WW_COLUMNS.IndexOf("OTNKHTECH") >= 0 Then
                MA0003INProw("OTNKHTECH") = XLSTBLrow("OTNKHTECH")
            End If

            If WW_COLUMNS.IndexOf("TAXATAX") >= 0 Then
                MA0003INProw("TAXATAX") = XLSTBLrow("TAXATAX")
            End If

            If WW_COLUMNS.IndexOf("TAXLINS") >= 0 Then
                MA0003INProw("TAXLINS") = XLSTBLrow("TAXLINS")
            End If

            If WW_COLUMNS.IndexOf("LICNYMD") >= 0 Then
                If IsDate(XLSTBLrow("LICNYMD")) Then
                    WW_DATE = XLSTBLrow("LICNYMD")
                    MA0003INProw("LICNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNPLTNO1") >= 0 Then
                MA0003INProw("LICNPLTNO1") = XLSTBLrow("LICNPLTNO1")
            End If

            If WW_COLUMNS.IndexOf("LICNFRAMENO") >= 0 Then
                MA0003INProw("LICNFRAMENO") = XLSTBLrow("LICNFRAMENO")
            End If

            If WW_COLUMNS.IndexOf("LICNMODEL") >= 0 Then
                MA0003INProw("LICNMODEL") = XLSTBLrow("LICNMODEL")
            End If

            If WW_COLUMNS.IndexOf("LICNLDCAPA") >= 0 Then
                MA0003INProw("LICNLDCAPA") = XLSTBLrow("LICNLDCAPA")
            End If

            If WW_COLUMNS.IndexOf("LICN5LDCAPA") >= 0 Then
                MA0003INProw("LICN5LDCAPA") = XLSTBLrow("LICN5LDCAPA")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKTINSYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKTINSYMD")
                    MA0003INProw("OTNKTINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMTINSYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMTINSYMD")
                    MA0003INProw("CHEMTINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSYMD")
                    MA0003INProw("HPRSINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("TAXVTAX") >= 0 Then
                MA0003INProw("TAXVTAX") = XLSTBLrow("TAXVTAX")
            End If

            If WW_COLUMNS.IndexOf("TAXLINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("TAXLINSYMD")) Then
                    WW_DATE = XLSTBLrow("TAXLINSYMD")
                    MA0003INProw("TAXLINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNPLTNO2") >= 0 Then
                MA0003INProw("LICNPLTNO2") = XLSTBLrow("LICNPLTNO2")
            End If

            If WW_COLUMNS.IndexOf("LICNMNFACT") >= 0 Then
                MA0003INProw("LICNMNFACT") = XLSTBLrow("LICNMNFACT")
            End If

            If WW_COLUMNS.IndexOf("LICNMOTOR") >= 0 Then
                MA0003INProw("LICNMOTOR") = XLSTBLrow("LICNMOTOR")
            End If

            If WW_COLUMNS.IndexOf("LICNTWEIGHT") >= 0 Then
                MA0003INProw("LICNTWEIGHT") = XLSTBLrow("LICNTWEIGHT")
            End If

            If WW_COLUMNS.IndexOf("LICNCWEIGHT") >= 0 Then
                MA0003INProw("LICNCWEIGHT") = XLSTBLrow("LICNCWEIGHT")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKTINSNYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKTINSNYMD")
                    MA0003INProw("OTNKTINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMTINSNYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMTINSNYMD")
                    MA0003INProw("CHEMTINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSNYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSNYMD")
                    MA0003INProw("HPRSINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNWEIGHT") >= 0 Then
                MA0003INProw("LICNWEIGHT") = XLSTBLrow("LICNWEIGHT")
            End If

            If WW_COLUMNS.IndexOf("HPRSJINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSJINSYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSJINSYMD")
                    MA0003INProw("HPRSJINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("MANGPROD1") >= 0 Then
                MA0003INProw("MANGPROD1") = XLSTBLrow("MANGPROD1")
            End If

            If WW_COLUMNS.IndexOf("MANGPROD2") >= 0 Then
                MA0003INProw("MANGPROD2") = XLSTBLrow("MANGPROD2")
            End If

            If WW_COLUMNS.IndexOf("FCTRSMAKER") >= 0 Then
                MA0003INProw("FCTRSMAKER") = XLSTBLrow("FCTRSMAKER")
            End If

            If WW_COLUMNS.IndexOf("FCTRTMAKER") >= 0 Then
                MA0003INProw("FCTRTMAKER") = XLSTBLrow("FCTRTMAKER")
            End If

            If WW_COLUMNS.IndexOf("OTNKEXHASIZE") >= 0 Then
                MA0003INProw("OTNKEXHASIZE") = XLSTBLrow("OTNKEXHASIZE")
            End If

            If WW_COLUMNS.IndexOf("HPRSPMPDR") >= 0 Then
                MA0003INProw("HPRSPMPDR") = XLSTBLrow("HPRSPMPDR")
            End If

            If WW_COLUMNS.IndexOf("HPRSHOSE") >= 0 Then
                MA0003INProw("HPRSHOSE") = XLSTBLrow("HPRSHOSE")
            End If

            If WW_COLUMNS.IndexOf("CONTSHAPE") >= 0 Then
                MA0003INProw("CONTSHAPE") = XLSTBLrow("CONTSHAPE")
            End If

            If WW_COLUMNS.IndexOf("CONTPUMP") >= 0 Then
                MA0003INProw("CONTPUMP") = XLSTBLrow("CONTPUMP")
            End If

            If WW_COLUMNS.IndexOf("CONTPMPDR") >= 0 Then
                MA0003INProw("CONTPMPDR") = XLSTBLrow("CONTPMPDR")
            End If

            If WW_COLUMNS.IndexOf("OTHRTIRE1") >= 0 Then
                MA0003INProw("OTHRTIRE1") = XLSTBLrow("OTHRTIRE1")
            End If

            If WW_COLUMNS.IndexOf("OTHRTIRE2") >= 0 Then
                MA0003INProw("OTHRTIRE2") = XLSTBLrow("OTHRTIRE2")
            End If

            If WW_COLUMNS.IndexOf("OTHRTPMS") >= 0 Then
                MA0003INProw("OTHRTPMS") = XLSTBLrow("OTHRTPMS")
            End If

            If WW_COLUMNS.IndexOf("OTNKTMAKER") >= 0 Then
                MA0003INProw("OTNKTMAKER") = XLSTBLrow("OTNKTMAKER")
            End If

            If WW_COLUMNS.IndexOf("HPRSTMAKER") >= 0 Then
                MA0003INProw("HPRSTMAKER") = XLSTBLrow("HPRSTMAKER")
            End If

            If WW_COLUMNS.IndexOf("CHEMTMAKER") >= 0 Then
                MA0003INProw("CHEMTMAKER") = XLSTBLrow("CHEMTMAKER")
            End If

            If WW_COLUMNS.IndexOf("CONTTMAKER") >= 0 Then
                MA0003INProw("CONTTMAKER") = XLSTBLrow("CONTTMAKER")
            End If

            If WW_COLUMNS.IndexOf("SHARYOSTATUS") >= 0 Then
                MA0003INProw("SHARYOSTATUS") = XLSTBLrow("SHARYOSTATUS")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO1") >= 0 Then
                MA0003INProw("SHARYOINFO1") = XLSTBLrow("SHARYOINFO1")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO2") >= 0 Then
                MA0003INProw("SHARYOINFO2") = XLSTBLrow("SHARYOINFO2")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO3") >= 0 Then
                MA0003INProw("SHARYOINFO3") = XLSTBLrow("SHARYOINFO3")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO4") >= 0 Then
                MA0003INProw("SHARYOINFO4") = XLSTBLrow("SHARYOINFO4")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO5") >= 0 Then
                MA0003INProw("SHARYOINFO5") = XLSTBLrow("SHARYOINFO5")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO6") >= 0 Then
                MA0003INProw("SHARYOINFO6") = XLSTBLrow("SHARYOINFO6")
            End If

            If WW_COLUMNS.IndexOf("INSKBN") >= 0 Then
                MA0003INProw("INSKBN") = XLSTBLrow("INSKBN")
            End If

            '名称付与
            CODENAME_get("BASELEASE", MA0003INProw("BASELEASE"), MA0003INProw("BASELEASENAME"), WW_DUMMY)
            CODENAME_get("CHEMDISGORGE", MA0003INProw("CHEMDISGORGE"), MA0003INProw("CHEMDISGORGENAME"), WW_DUMMY)
            CODENAME_get("CHEMHOSE", MA0003INProw("CHEMHOSE"), MA0003INProw("CHEMHOSENAME"), WW_DUMMY)
            CODENAME_get("CHEMMANOMTR", MA0003INProw("CHEMMANOMTR"), MA0003INProw("CHEMMANOMTRNAME"), WW_DUMMY)
            CODENAME_get("CHEMMATERIAL", MA0003INProw("CHEMMATERIAL"), MA0003INProw("CHEMMATERIALNAME"), WW_DUMMY)
            CODENAME_get("CHEMPMPDR", MA0003INProw("CHEMPMPDR"), MA0003INProw("CHEMPMPDRNAME"), WW_DUMMY)
            CODENAME_get("CHEMPRESDRV", MA0003INProw("CHEMPRESDRV"), MA0003INProw("CHEMPRESDRVNAME"), WW_DUMMY)
            CODENAME_get("CHEMPRESEQ", MA0003INProw("CHEMPRESEQ"), MA0003INProw("CHEMPRESEQNAME"), WW_DUMMY)
            CODENAME_get("CHEMPUMP", MA0003INProw("CHEMPUMP"), MA0003INProw("CHEMPUMPNAME"), WW_DUMMY)
            CODENAME_get("CHEMSTRUCT", MA0003INProw("CHEMSTRUCT"), MA0003INProw("CHEMSTRUCTNAME"), WW_DUMMY)
            CODENAME_get("CHEMTHERM", MA0003INProw("CHEMTHERM"), MA0003INProw("CHEMTHERMNAME"), WW_DUMMY)
            CODENAME_get("FCTRAXLE", MA0003INProw("FCTRAXLE"), MA0003INProw("FCTRAXLENAME"), WW_DUMMY)
            CODENAME_get("FCTRDPR", MA0003INProw("FCTRDPR"), MA0003INProw("FCTRDPRNAME"), WW_DUMMY)
            CODENAME_get("FCTRFUELMATE", MA0003INProw("FCTRFUELMATE"), MA0003INProw("FCTRFUELMATENAME"), WW_DUMMY)
            CODENAME_get("FCTRSHFTNUM", MA0003INProw("FCTRSHFTNUM"), MA0003INProw("FCTRSHFTNUMNAME"), WW_DUMMY)
            CODENAME_get("FCTRSUSP", MA0003INProw("FCTRSUSP"), MA0003INProw("FCTRSUSPNAME"), WW_DUMMY)
            CODENAME_get("FCTRTMISSION", MA0003INProw("FCTRTMISSION"), MA0003INProw("FCTRTMISSIONNAME"), WW_DUMMY)
            CODENAME_get("FCTRUREA", MA0003INProw("FCTRUREA"), MA0003INProw("FCTRUREANAME"), WW_DUMMY)
            CODENAME_get("HPRSINSULATE", MA0003INProw("HPRSINSULATE"), MA0003INProw("HPRSINSULATENAME"), WW_DUMMY)
            CODENAME_get("HPRSMATR", MA0003INProw("HPRSMATR"), MA0003INProw("HPRSMATRNAME"), WW_DUMMY)
            CODENAME_get("HPRSPIPE", MA0003INProw("HPRSPIPE"), MA0003INProw("HPRSPIPENAME"), WW_DUMMY)
            CODENAME_get("HPRSPIPENUM", MA0003INProw("HPRSPIPENUM"), MA0003INProw("HPRSPIPENUMNAME"), WW_DUMMY)
            CODENAME_get("HPRSPUMP", MA0003INProw("HPRSPUMP"), MA0003INProw("HPRSPUMPNAME"), WW_DUMMY)
            CODENAME_get("HPRSRESSRE", MA0003INProw("HPRSRESSRE"), MA0003INProw("HPRSRESSRENAME"), WW_DUMMY)
            CODENAME_get("HPRSSTRUCT", MA0003INProw("HPRSSTRUCT"), MA0003INProw("HPRSSTRUCTNAME"), WW_DUMMY)
            CODENAME_get("HPRSVALVE", MA0003INProw("HPRSVALVE"), MA0003INProw("HPRSVALVENAME"), WW_DUMMY)
            CODENAME_get("MANGMORG", MA0003INProw("MANGMORG"), MA0003INProw("MANGMORGNAME"), WW_DUMMY)
            CODENAME_get("MANGOILTYPE", MA0003INProw("MANGOILTYPE"), MA0003INProw("MANGOILTYPENAME"), WW_DUMMY)
            CODENAME_get("MANGOWNCODE", MA0003INProw("MANGOWNCODE"), MA0003INProw("MANGOWNCODENAME"), WW_DUMMY)
            CODENAME_get("MANGOWNCONT", MA0003INProw("MANGOWNCONT"), MA0003INProw("MANGOWNCONTNAME"), WW_DUMMY)
            CODENAME_get("MANGSORG", MA0003INProw("MANGSORG"), MA0003INProw("MANGSORGNAME"), WW_DUMMY)
            CODENAME_get("MANGSUPPL", MA0003INProw("MANGSUPPL"), MA0003INProw("MANGSUPPLNAME"), WW_DUMMY)
            CODENAME_get("MANGUORG", MA0003INProw("MANGUORG"), MA0003INProw("MANGUORGNAME"), WW_DUMMY)
            CODENAME_get("NOTES", MA0003INProw("NOTES"), MA0003INProw("NOTES"), WW_DUMMY)
            CODENAME_get("OTHRBMONITOR", MA0003INProw("OTHRBMONITOR"), MA0003INProw("OTHRBMONITORNAME"), WW_DUMMY)
            CODENAME_get("OTHRBSONAR", MA0003INProw("OTHRBSONAR"), MA0003INProw("OTHRBSONARNAME"), WW_DUMMY)
            CODENAME_get("FCTRTIRE", MA0003INProw("FCTRTIRE"), MA0003INProw("FCTRTIRENAME"), WW_DUMMY)
            CODENAME_get("OTHRDRRECORD", MA0003INProw("OTHRDRRECORD"), MA0003INProw("OTHRDRRECORDNAME"), WW_DUMMY)
            CODENAME_get("OTHRPAINTING", MA0003INProw("OTHRPAINTING"), MA0003INProw("OTHRPAINTINGNAME"), WW_DUMMY)
            CODENAME_get("OTHRRADIOCON", MA0003INProw("OTHRRADIOCON"), MA0003INProw("OTHRRADIOCONNAME"), WW_DUMMY)
            CODENAME_get("OTHRRTARGET", MA0003INProw("OTHRRTARGET"), MA0003INProw("OTHRRTARGETNAME"), WW_DUMMY)
            CODENAME_get("OTHRTERMINAL", MA0003INProw("OTHRTERMINAL"), MA0003INProw("OTHRTERMINALNAME"), WW_DUMMY)
            CODENAME_get("OTNKBPIPE", MA0003INProw("OTNKBPIPE"), MA0003INProw("OTNKBPIPENAME"), WW_DUMMY)
            CODENAME_get("OTNKCVALVE", MA0003INProw("OTNKCVALVE"), MA0003INProw("OTNKCVALVENAME"), WW_DUMMY)
            CODENAME_get("OTNKDCD", MA0003INProw("OTNKDCD"), MA0003INProw("OTNKDCDNAME"), WW_DUMMY)
            CODENAME_get("OTNKDETECTOR", MA0003INProw("OTNKDETECTOR"), MA0003INProw("OTNKDETECTORNAME"), WW_DUMMY)
            CODENAME_get("OTNKDISGORGE", MA0003INProw("OTNKDISGORGE"), MA0003INProw("OTNKDISGORGENAME"), WW_DUMMY)
            CODENAME_get("OTNKHTECH", MA0003INProw("OTNKHTECH"), MA0003INProw("OTNKHTECHNAME"), WW_DUMMY)
            CODENAME_get("OTNKLVALVE", MA0003INProw("OTNKLVALVE"), MA0003INProw("OTNKLVALVENAME"), WW_DUMMY)
            CODENAME_get("OTNKMATERIAL", MA0003INProw("OTNKMATERIAL"), MA0003INProw("OTNKMATERIALNAME"), WW_DUMMY)
            CODENAME_get("OTNKPIPE", MA0003INProw("OTNKPIPE"), MA0003INProw("OTNKPIPENAME"), WW_DUMMY)
            CODENAME_get("OTNKPIPESIZE", MA0003INProw("OTNKPIPESIZE"), MA0003INProw("OTNKPIPESIZENAME"), WW_DUMMY)
            CODENAME_get("OTNKPUMP", MA0003INProw("OTNKPUMP"), MA0003INProw("OTNKPUMPNAME"), WW_DUMMY)
            CODENAME_get("OTNKVAPOR", MA0003INProw("OTNKVAPOR"), MA0003INProw("OTNKVAPORNAME"), WW_DUMMY)
            CODENAME_get("MANGPROD1", MA0003INProw("MANGPROD1"), MA0003INProw("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0003INProw("MANGOILTYPE"))})
            CODENAME_get("MANGPROD2", MA0003INProw("MANGPROD2"), MA0003INProw("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0003INProw("MANGOILTYPE")), CStr(MA0003INProw("MANGPROD1"))})
            CODENAME_get("FCTRSMAKER", MA0003INProw("FCTRSMAKER"), MA0003INProw("FCTRSMAKERNAME"), WW_DUMMY)
            CODENAME_get("OTNKEXHASIZE", MA0003INProw("OTNKEXHASIZE"), MA0003INProw("OTNKEXHASIZENAME"), WW_DUMMY)
            CODENAME_get("HPRSPMPDR", MA0003INProw("HPRSPMPDR"), MA0003INProw("HPRSPMPDRNAME"), WW_DUMMY)
            CODENAME_get("HPRSHOSE", MA0003INProw("HPRSHOSE"), MA0003INProw("HPRSHOSENAME"), WW_DUMMY)
            CODENAME_get("CONTSHAPE", MA0003INProw("CONTSHAPE"), MA0003INProw("CONTSHAPENAME"), WW_DUMMY)
            CODENAME_get("CONTPUMP", MA0003INProw("CONTPUMP"), MA0003INProw("CONTPUMPNAME"), WW_DUMMY)
            CODENAME_get("CONTPMPDR", MA0003INProw("CONTPMPDR"), MA0003INProw("CONTPMPDRNAME"), WW_DUMMY)
            CODENAME_get("OTHRTPMS", MA0003INProw("OTHRTPMS"), MA0003INProw("OTHRTPMSNAME"), WW_DUMMY)
            CODENAME_get("OTNKTMAKER", MA0003INProw("OTNKTMAKER"), MA0003INProw("OTNKTMAKERNAME"), WW_DUMMY)
            CODENAME_get("HPRSTMAKER", MA0003INProw("HPRSTMAKER"), MA0003INProw("HPRSTMAKERNAME"), WW_DUMMY)
            CODENAME_get("CHEMTMAKER", MA0003INProw("CHEMTMAKER"), MA0003INProw("CHEMTMAKERNAME"), WW_DUMMY)
            CODENAME_get("CONTTMAKER", MA0003INProw("CONTTMAKER"), MA0003INProw("CONTTMAKERNAME"), WW_DUMMY)
            CODENAME_get("SHARYOSTATUS", MA0003INProw("SHARYOSTATUS"), MA0003INProw("SHARYOSTATUSNAME"), WW_DUMMY)
            CODENAME_get("INSKBN", MA0003INProw("INSKBN"), MA0003INProw("INSKBNNAME"), WW_DUMMY)

            MA0003INProw("STYMD_A") = ""
            MA0003INProw("STYMD_B") = ""
            MA0003INProw("STYMD_C") = ""
            MA0003INProw("STYMD_S") = ""
            MA0003INProw("ENDYMD_A") = ""
            MA0003INProw("ENDYMD_B") = ""
            MA0003INProw("ENDYMD_C") = ""
            MA0003INProw("ENDYMD_S") = ""
            MA0003INProw("WORK_NO") = 0

            MA0003INPtbl.Rows.Add(MA0003INProw)

        Next

        '○項目チェック 
        INPUT_CHEK(WW_ERRCODE)

        '○入力値テーブル反映(MA0003INPtbl⇒MA0003tbl)
        TBL_UPD("EXCEL", WW_ERRCODE)

        '○画面表示データ保存
        Master.SaveTable(MA0003tbl)

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_SELMORG.Focus()

        '○Close
        CS0023XLSUPLOAD.TBLDATA.Clear()
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  詳細画面-PDFファイル操作関連                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' PDF読み込み処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_Readonly()

        'PDF内容の表示/非表示
        Dim WW_NONDisplay As String = ""

        'Repeaterバインド準備
        MA0003PDFtbl_ColumnsAdd()

        '○年度算出(有効開始年月日を基準として算出）
        Dim WW_nendoS As String = "0000"
        Dim WW_nendoE As String = "0000"
        Dim WW_dateS As Date
        Dim WW_dateE As Date

        '日付取得
        Try
            Date.TryParse(WF_STYMD.Text, WW_dateS)
        Catch ex As Exception
            WW_dateS = C_DEFAULT_YMD
        End Try
        Try
            Date.TryParse(WF_ENDYMD.Text, WW_dateE)
            '終了日付が当日より大きい場合、当日日付を設定する
            If WW_dateE >= Date.Now Then
                WW_dateE = Date.Now
            End If
        Catch ex As Exception
            WW_dateE = C_DEFAULT_YMD
        End Try


        If WF_STYMD.Text = "" OrElse WW_dateS <= C_DEFAULT_YMD Then
            WW_NONDisplay = "NO-Display"
        End If

        If WF_ENDYMD.Text = "" OrElse WW_dateE <= C_DEFAULT_YMD Then
            WW_NONDisplay = "NO-Display"
        End If

        If WW_NONDisplay <> "NO-Display" Then
            If WW_dateS.ToString("MM") = "01" OrElse WW_dateS.ToString("MM") = "02" OrElse WW_dateS.ToString("MM") = "03" Then
                WW_nendoS = (WW_dateS.Year - 1).ToString()
            Else
                WW_nendoS = (WW_dateS.Year).ToString()
            End If
            If WW_dateE.ToString("MM") = "01" OrElse WW_dateE.ToString("MM") = "02" OrElse WW_dateE.ToString("MM") = "03" Then
                WW_nendoE = (WW_dateE.Year - 1).ToString()
            Else
                WW_nendoE = (WW_dateE.Year).ToString()
            End If
        End If

        '○事前確認
        '統一車番の存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" OrElse WF_TSHABAN.Text = "" Then
            WW_NONDisplay = "NO-Display"
        Else
            For i As Integer = 0 To MA0003tbl.Rows.Count - 1
                If WF_SHARYOTYPE.Text = MA0003tbl.Rows(i)("SHARYOTYPE") OrElse WF_TSHABAN.Text = MA0003tbl.Rows(i)("TSHABAN") Then
                    Exit For
                Else
                    If (i - 1) >= MA0003tbl.Rows.Count Then
                        WW_NONDisplay = "NO-Display"
                    End If
                End If
            Next
        End If

        '○PDF格納Dir検索
        Dim WW_Dir_Temp As String = ""
        Dim WW_PDFFile As String = ""
        Dim WW_Dir_Find As New List(Of String)

        WW_Dir_Temp = CS0050Session.PDF_PATH & "\MA0004_SHARYOC"
        If Directory.Exists(WW_Dir_Temp) Then
            Dim WW_UPfiles As String() = Directory.GetDirectories(WW_Dir_Temp, "*", SearchOption.TopDirectoryOnly)
            For Each tempFile As String In WW_UPfiles
                '統一車番で抽出
                If (WF_Rep11_PDFselect.SelectedValue = "01" AndAlso Left(Right(tempFile, 16), 8) = (WF_SHARYOTYPE.Text & WF_TSHABAN.Text)) OrElse
                   (WF_Rep11_PDFselect.SelectedValue <> "01" AndAlso Left(Right(tempFile, 11), 8) = (WF_SHARYOTYPE.Text & WF_TSHABAN.Text)) Then
                    '書類種類で抽出
                    If Right(tempFile, 2) = WF_Rep11_PDFselect.SelectedValue Then
                        '年度による抽出
                        If WF_Rep11_PDFselect.SelectedValue = "01" Then
                            '01:車検証 … 年度アリフォルダ構成
                            If Val(Left(Right(tempFile, 7), 4)) >= Val(WW_nendoS) AndAlso Val(Left(Right(tempFile, 7), 4)) <= Val(WW_nendoE) Then
                                WW_PDFFile = CS0050Session.PDF_PATH & "\MA0004_SHARYOC"
                                WW_PDFFile = WW_PDFFile & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & Left(Right(tempFile, 7), 4) & "_" & WF_Rep11_PDFselect.SelectedValue

                                Dim WW_PDFFiles As String() = Directory.GetFiles(WW_PDFFile, "*", SearchOption.TopDirectoryOnly)
                                If WW_PDFFiles.Length > 0 Then
                                    WW_Dir_Find.Add(tempFile)
                                End If
                            End If
                        Else
                            '上記以外 … 年度ナシフォルダ構成
                            WW_PDFFile = CS0050Session.PDF_PATH & "\MA0004_SHARYOC"
                            WW_PDFFile = WW_PDFFile & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WF_Rep11_PDFselect.SelectedValue

                            Dim WW_PDFFiles As String() = Directory.GetFiles(WW_PDFFile, "*", SearchOption.TopDirectoryOnly)
                            If WW_PDFFiles.Length > 0 Then
                                WW_Dir_Find.Add(tempFile)
                            End If
                        End If
                    End If
                End If
            Next
        End If

        '年度並べ替え（昇順→降順）…　最新PDFを取得
        WW_Dir_Find.Reverse()


        '○ Dir内の全ファイル取得
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)

        If WW_NONDisplay = "" AndAlso WW_Dir_Find.Count > 0 Then
            'PDF格納ディレクトリ編集
            WW_Dir_Temp = WW_Dir_Find(0).ToString()

            'ディレクトリ内ファイル
            Dim WW_UPfiles As String() = Directory.GetFiles(WW_Dir_Temp, "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                If Right(tempFile, 4) = ".pdf" OrElse Right(tempFile, 4) = ".PDF" Then
                    Dim WW_tempFile As String = tempFile
                    Do
                        If InStr(WW_tempFile, "\") > 0 Then
                            'ファイル名編集
                            WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                        End If

                        If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                            'ファイルパス格納
                            WW_Files_dir.Add(tempFile)
                            'ファイル名格納
                            WW_Files_name.Add(WW_tempFile)
                            Exit Do
                        End If
                    Loop Until InStr(WW_tempFile, "\") = 0
                End If
            Next

            For i As Integer = 0 To WW_Files_dir.Count - 1
                Dim MA0003PDFrow As DataRow = MA0003PDFtbl.NewRow
                MA0003PDFrow("FILENAME") = WW_Files_name.Item(i)
                MA0003PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
                MA0003PDFrow("FILEPATH") = WW_Files_dir.Item(i)
                MA0003PDFtbl.Rows.Add(MA0003PDFrow)
            Next
        End If

        '○バインド 
        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = MA0003PDFtbl
        WF_DViewRepPDF.DataBind()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1
            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.ALIVE
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)
        Next

        '○イベント設定 
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF表示内容変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_SELECTchange()

        PDF_Readonly()

    End Sub
    ''' <summary>
    ''' 詳細画面-PDF表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DTAB_PDFdisplay()

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_DTAB_PDF_DISP_FILE.Value Then
                'ディレクトリが存在しない場合、作成する
                If Not Directory.Exists(CS0050Session.UPLOAD_PATH & "\PRINTWORK\" & Master.USERID) Then
                    Directory.CreateDirectory(CS0050Session.UPLOAD_PATH & "\PRINTWORK\" & Master.USERID)
                End If

                'ダウンロードファイル送信準備
                File.Copy(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text,
                            CS0050Session.UPLOAD_PATH & "\PRINTWORK\" & Master.USERID & "\" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text,
                            True)

                'ダウンロード処理へ遷移
                WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/print/" & Master.USERID & "/" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)

                Exit For
            End If
        Next

        WF_DTAB_PDF_DISP_FILE.Value = ""

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    '''  条件抽出画面情報退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MA0003S Then
            Master.MAPID = GRMA0003WRKINC.MAPID

            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

    End Sub

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベース（MA003_SHARYOB）を検索し画面表示する一覧を作成する</remarks>
    Protected Sub MAPDATAget()

        Dim WW_SHARYOTYPE As String = ""
        Dim WW_TSHABAN As String = ""
        Dim WW_STYMD_B As String = ""

        '○画面表示用データ取得 

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            'テーブル検索結果をテーブル退避
            'MA0003項目作成
            If MA0003tbl Is Nothing Then
                MA0003tbl = New DataTable
            End If

            If MA0003tbl.Columns.Count <> 0 Then
                MA0003tbl.Columns.Clear()
            End If

            '○DB項目クリア
            MA0003tbl.Clear()

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

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
                      " SELECT                                                             " _
                    & "         0                                      as LINECNT      ,   " _
                    & "         ''                                     as OPERATION    ,   " _
                    & "         TIMSTP = cast(isnull(A.UPDTIMSTP,0) as bigint)         ,   " _
                    & "         0                                      as 'SELECT'     ,   " _
                    & "         0                                      as HIDDEN       ,   " _
                    & "         0                                      as WORK_NO      ,   " _
                    & "         isnull(rtrim(A.MANGMORG),'')           as MANGMORG     ,   " _
                    & "         isnull(rtrim(A.MANGSORG),'')           as MANGSORG     ,   " _
                    & "         isnull(rtrim(A.MANGOILTYPE),'')        as MANGOILTYPE  ,   " _
                    & "         isnull(rtrim(A.MANGOWNCODE),'')        as MANGOWNCODE  ,   " _
                    & "         isnull(rtrim(A.MANGPROD1),'')          as MANGPROD1    ,   " _
                    & "         isnull(rtrim(A.MANGPROD2),'')          as MANGPROD2    ,   " _
                    & "         isnull(rtrim(A.MANGOWNCONT),'')        as MANGOWNCONT  ,   " _
                    & "         cast(isnull(A.MANGSHAFUKU,'0') as VarChar) as MANGSHAFUKU  ,   " _
                    & "         isnull(rtrim(A.MANGSUPPL),'')          as MANGSUPPL    ,   " _
                    & "         cast(isnull(A.MANGTTLDIST,'0') as VarChar) as MANGTTLDIST  ,   " _
                    & "         cast(isnull(B.BASERAGE,'') as VarChar) as BASERAGE     ,   " _
                    & "         cast(isnull(B.BASERAGEMM,'') as VarChar) as BASERAGEMM   ,   " _
                    & "         isnull(rtrim(A.BASELEASE),'')          as BASELEASE    ,   " _
                    & "         cast(isnull(B.BASERAGEYY,'') as VarChar) as BASERAGEYY   ,   " _
                    & "         rtrim(B.BASERDATE)                     as BASERDATE    ,   " _
                    & "         isnull(rtrim(B.FCTRDPR),'')            as FCTRDPR      ,   " _
                    & "         isnull(rtrim(B.FCTRAXLE),'')           as FCTRAXLE     ,   " _
                    & "         cast(isnull(B.FCTRFUELCAPA,'') as VarChar) as FCTRFUELCAPA ,   " _
                    & "         isnull(rtrim(B.FCTRFUELMATE),'')       as FCTRFUELMATE ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE1),'')       as FCTRRESERVE1 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE2),'')       as FCTRRESERVE2 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE3),'')       as FCTRRESERVE3 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE4),'')       as FCTRRESERVE4 ,   " _
                    & "         isnull(rtrim(B.FCTRRESERVE5),'')       as FCTRRESERVE5 ,   " _
                    & "         isnull(rtrim(B.FCTRSHFTNUM),'')        as FCTRSHFTNUM  ,   " _
                    & "         isnull(rtrim(B.FCTRSUSP),'')           as FCTRSUSP     ,   " _
                    & "         isnull(rtrim(B.FCTRSMAKER),'')         as FCTRSMAKER   ,   " _
                    & "         isnull(rtrim(B.FCTRTMAKER),'')         as FCTRTMAKER   ,   " _
                    & "         isnull(rtrim(B.FCTRTIRE),'')           as FCTRTIRE     ,   " _
                    & "         isnull(rtrim(B.FCTRTMISSION),'')       as FCTRTMISSION ,   " _
                    & "         isnull(rtrim(B.FCTRUREA),'')           as FCTRUREA     ,   " _
                    & "         isnull(rtrim(B.OTNKBPIPE),'')          as OTNKBPIPE    ,   " _
                    & "         isnull(rtrim(B.OTNKCELLNO),'')         as OTNKCELLNO   ,   " _
                    & "         isnull(rtrim(B.OTNKVAPOR),'')          as OTNKVAPOR    ,   " _
                    & "         isnull(rtrim(B.OTNKCELPART),'')        as OTNKCELPART  ,   " _
                    & "         isnull(rtrim(B.OTNKCVALVE),'')         as OTNKCVALVE   ,   " _
                    & "         isnull(rtrim(B.OTNKDCD),'')            as OTNKDCD      ,   " _
                    & "         isnull(rtrim(B.OTNKDETECTOR),'')       as OTNKDETECTOR ,   " _
                    & "         isnull(rtrim(B.OTNKDISGORGE),'')       as OTNKDISGORGE ,   " _
                    & "         isnull(rtrim(B.OTNKHTECH),'')          as OTNKHTECH    ,   " _
                    & "         isnull(rtrim(B.OTNKINSSTAT),'')        as OTNKINSSTAT  ,   " _
                    & "         CASE WHEN B.OTNKINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(B.OTNKINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                    as OTNKINSYMD   ,   " _
                    & "         isnull(rtrim(B.OTNKLVALVE),'')         as OTNKLVALVE   ,   " _
                    & "         isnull(rtrim(B.OTNKMATERIAL),'')       as OTNKMATERIAL ,   " _
                    & "         isnull(rtrim(B.OTNKPIPE),'')           as OTNKPIPE     ,   " _
                    & "         isnull(rtrim(B.OTNKPIPESIZE),'')       as OTNKPIPESIZE ,   " _
                    & "         isnull(rtrim(B.OTNKPUMP),'')           as OTNKPUMP     ,   " _
                    & "         isnull(rtrim(B.OTNKEXHASIZE),'')       as OTNKEXHASIZE ,   " _
                    & "         isnull(rtrim(B.OTNKTINSNO),'')         as OTNKTINSNO   ,   " _
                    & "         isnull(rtrim(B.OTNKTMAKER),'')         as OTNKTMAKER   ,   " _
                    & "         isnull(rtrim(B.HPRSINSISTAT),'')       as HPRSINSISTAT ,   " _
                    & "         CASE WHEN B.HPRSINSIYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(B.HPRSINSIYMD,'yyyy/MM/dd')               " _
                    & "         END                                    as HPRSINSIYMD  ,   " _
                    & "         isnull(rtrim(B.HPRSINSULATE),'')       as HPRSINSULATE ,   " _
                    & "         isnull(rtrim(B.HPRSMATR),'')           as HPRSMATR     ,   " _
                    & "         isnull(rtrim(B.HPRSPIPE),'')           as HPRSPIPE     ,   " _
                    & "         isnull(rtrim(B.HPRSPIPENUM),'')        as HPRSPIPENUM  ,   " _
                    & "         isnull(rtrim(B.HPRSPUMP),'')           as HPRSPUMP     ,   " _
                    & "         isnull(rtrim(B.HPRSRESSRE),'')         as HPRSRESSRE   ,   " _
                    & "         isnull(rtrim(B.HPRSSERNO),'')          as HPRSSERNO    ,   " _
                    & "         isnull(rtrim(B.HPRSSTRUCT),'')         as HPRSSTRUCT   ,   " _
                    & "         isnull(rtrim(B.HPRSVALVE),'')          as HPRSVALVE    ,   " _
                    & "         isnull(rtrim(B.HPRSPMPDR),'')          as HPRSPMPDR    ,   " _
                    & "         isnull(rtrim(B.HPRSHOSE),'')           as HPRSHOSE     ,   " _
                    & "         isnull(rtrim(B.HPRSTMAKER),'')         as HPRSTMAKER   ,   " _
                    & "         isnull(rtrim(B.CHEMCELLNO),'')         as CHEMCELLNO   ,   " _
                    & "         isnull(rtrim(B.CHEMCELPART),'')        as CHEMCELPART  ,   " _
                    & "         isnull(rtrim(B.CHEMDISGORGE),'')       as CHEMDISGORGE ,   " _
                    & "         isnull(rtrim(B.CHEMHOSE),'')           as CHEMHOSE     ,   " _
                    & "         isnull(rtrim(B.CHEMINSSTAT),'')        as CHEMINSSTAT  ,   " _
                    & "         CASE WHEN B.CHEMINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(B.CHEMINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                    as CHEMINSYMD   ,   " _
                    & "         isnull(rtrim(B.CHEMMANOMTR),'')        as CHEMMANOMTR  ,   " _
                    & "         isnull(rtrim(B.CHEMMATERIAL),'')       as CHEMMATERIAL ,   " _
                    & "         isnull(rtrim(B.CHEMPMPDR),'')          as CHEMPMPDR    ,   " _
                    & "         isnull(rtrim(B.CHEMPRESDRV),'')        as CHEMPRESDRV  ,   " _
                    & "         isnull(rtrim(B.CHEMPRESEQ),'')         as CHEMPRESEQ   ,   " _
                    & "         isnull(rtrim(B.CHEMPUMP),'')           as CHEMPUMP     ,   " _
                    & "         isnull(rtrim(B.CHEMSTRUCT),'')         as CHEMSTRUCT   ,   " _
                    & "         isnull(rtrim(B.CHEMTHERM),'')          as CHEMTHERM    ,   " _
                    & "         isnull(rtrim(B.CHEMTINSNO),'')         as CHEMTINSNO   ,   " _
                    & "         isnull(rtrim(B.CHEMTMAKER),'')         as CHEMTMAKER   ,   " _
                    & "         isnull(rtrim(B.CONTSHAPE),'')          as CONTSHAPE    ,   " _
                    & "         isnull(rtrim(B.CONTPUMP),'')           as CONTPUMP     ,   " _
                    & "         isnull(rtrim(B.CONTPMPDR),'')          as CONTPMPDR    ,   " _
                    & "         isnull(rtrim(B.CONTTMAKER),'')         as CONTTMAKER   ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE1),'')       as OFFCRESERVE1 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE2),'')       as OFFCRESERVE2 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE3),'')       as OFFCRESERVE3 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE4),'')       as OFFCRESERVE4 ,   " _
                    & "         isnull(rtrim(B.OFFCRESERVE5),'')       as OFFCRESERVE5 ,   " _
                    & "         isnull(rtrim(B.OTHRBMONITOR),'')       as OTHRBMONITOR ,   " _
                    & "         isnull(rtrim(B.OTHRBSONAR),'')         as OTHRBSONAR   ,   " _
                    & "         isnull(rtrim(B.OTHRDOCO),'')           as OTHRDOCO     ,   " _
                    & "         isnull(rtrim(B.OTHRDRRECORD),'')       as OTHRDRRECORD ,   " _
                    & "         isnull(rtrim(B.OTHRPAINTING),'')       as OTHRPAINTING ,   " _
                    & "         isnull(rtrim(B.OTHRRADIOCON),'')       as OTHRRADIOCON ,   " _
                    & "         isnull(rtrim(B.OTHRRTARGET),'')        as OTHRRTARGET  ,   " _
                    & "         isnull(rtrim(B.OTHRTERMINAL),'')       as OTHRTERMINAL ,   " _
                    & "         isnull(rtrim(B.OTHRTIRE1),'')          as OTHRTIRE1    ,   " _
                    & "         isnull(rtrim(B.OTHRTIRE2),'')          as OTHRTIRE2    ,   " _
                    & "         isnull(rtrim(B.OTHRTPMS),'')           as OTHRTPMS     ,   " _
                    & "         isnull(rtrim(B.ACCTASST01),'')         as ACCTASST01   ,   " _
                    & "         isnull(rtrim(B.ACCTASST02),'')         as ACCTASST02   ,   " _
                    & "         isnull(rtrim(B.ACCTASST03),'')         as ACCTASST03   ,   " _
                    & "         isnull(rtrim(B.ACCTASST04),'')         as ACCTASST04   ,   " _
                    & "         isnull(rtrim(B.ACCTASST05),'')         as ACCTASST05   ,   " _
                    & "         isnull(rtrim(B.ACCTASST06),'')         as ACCTASST06   ,   " _
                    & "         isnull(rtrim(B.ACCTASST07),'')         as ACCTASST07   ,   " _
                    & "         isnull(rtrim(B.ACCTASST08),'')         as ACCTASST08   ,   " _
                    & "         isnull(rtrim(B.ACCTASST09),'')         as ACCTASST09   ,   " _
                    & "         isnull(rtrim(B.ACCTASST10),'')         as ACCTASST10   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE1),'')         as ACCTLEASE1   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE2),'')         as ACCTLEASE2   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE3),'')         as ACCTLEASE3   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE4),'')         as ACCTLEASE4   ,   " _
                    & "         isnull(rtrim(B.ACCTLEASE5),'')         as ACCTLEASE5   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL1),'')         as ACCTLSUPL1   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL2),'')         as ACCTLSUPL2   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL3),'')         as ACCTLSUPL3   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL4),'')         as ACCTLSUPL4   ,   " _
                    & "         isnull(rtrim(B.ACCTLSUPL5),'')         as ACCTLSUPL5   ,   " _
                    & "         cast(isnull(B.ACCTRCYCLE,'') as VarChar) as ACCTRCYCLE  ,   " _
                    & "         isnull(rtrim(B.NOTES),'')              as NOTES        ,   " _
                    & "         CASE WHEN C.CHEMTINSNYMD IS NULL THEN ''                   " _
                    & "              ELSE FORMAT(C.CHEMTINSNYMD,'yyyy/MM/dd')              " _
                    & "         END                                     as CHEMTINSNYMD,   " _
                    & "         CASE WHEN C.CHEMTINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.CHEMTINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as CHEMTINSYMD ,   " _
                    & "         cast(isnull(C.LICN5LDCAPA,'') as VarChar) as LICN5LDCAPA ,   " _
                    & "         cast(isnull(C.LICNCWEIGHT,'') as VarChar) as LICNCWEIGHT ,   " _
                    & "         isnull(rtrim(C.LICNFRAMENO),'')         as LICNFRAMENO ,   " _
                    & "         cast(isnull(C.LICNLDCAPA,'') as VarChar) as LICNLDCAPA  ,   " _
                    & "         isnull(rtrim(C.LICNMNFACT),'')          as LICNMNFACT  ,   " _
                    & "         isnull(rtrim(C.LICNMODEL),'')           as LICNMODEL   ,   " _
                    & "         isnull(rtrim(C.LICNMOTOR),'')           as LICNMOTOR   ,   " _
                    & "         isnull(rtrim(C.LICNPLTNO1),'')          as LICNPLTNO1  ,   " _
                    & "         isnull(rtrim(C.LICNPLTNO2),'')          as LICNPLTNO2  ,   " _
                    & "         cast(isnull(C.LICNTWEIGHT,'') as VarChar) as LICNTWEIGHT ,   " _
                    & "         cast(isnull(C.LICNWEIGHT,'') as VarChar) as LICNWEIGHT  ,   " _
                    & "         CASE WHEN C.LICNYMD IS NULL THEN ''                        " _
                    & "              ELSE FORMAT(C.LICNYMD,'yyyy/MM/dd')                   " _
                    & "         END                                     as LICNYMD     ,   " _
                    & "         cast(isnull(C.TAXATAX,'') as VarChar)   as TAXATAX     ,   " _
                    & "         cast(isnull(C.TAXLINS,'') as VarChar)   as TAXLINS     ,   " _
                    & "         CASE WHEN C.TAXLINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(C.TAXLINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                     as TAXLINSYMD  ,   " _
                    & "         cast(isnull(C.TAXVTAX,'') as VarChar) as TAXVTAX       ,   " _
                    & "         CASE WHEN C.OTNKTINSNYMD IS NULL THEN ''                   " _
                    & "              ELSE FORMAT(C.OTNKTINSNYMD,'yyyy/MM/dd')              " _
                    & "         END                                     as OTNKTINSNYMD,   " _
                    & "         CASE WHEN C.OTNKTINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.OTNKTINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as OTNKTINSYMD ,   " _
                    & "         CASE WHEN C.HPRSINSNYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.HPRSINSNYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as HPRSINSNYMD ,   " _
                    & "         CASE WHEN C.HPRSINSYMD IS NULL THEN ''                     " _
                    & "              ELSE FORMAT(C.HPRSINSYMD,'yyyy/MM/dd')                " _
                    & "         END                                     as HPRSINSYMD  ,   " _
                    & "         CASE WHEN C.HPRSJINSYMD IS NULL THEN ''                    " _
                    & "              ELSE FORMAT(C.HPRSJINSYMD,'yyyy/MM/dd')               " _
                    & "         END                                     as HPRSJINSYMD ,   " _
                    & "         isnull(rtrim(C.INSKBN),'')              as INSKBN      ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEF),'')         as SHARYOTYPEF ,   " _
                    & "         isnull(rtrim(D.TSHABANF),'')            as TSHABANF    ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEB),'')         as SHARYOTYPEB ,   " _
                    & "         isnull(rtrim(D.TSHABANB),'')            as TSHABANB    ,   " _
                    & "         isnull(rtrim(D.SHARYOTYPEB2),'')        as SHARYOTYPEB2,   " _
                    & "         isnull(rtrim(D.TSHABANB2),'')           as TSHABANB2   ,   " _
                    & "         ''                                      as SHARYOTYPEB3,   " _
                    & "         ''                                      as TSHABANB3   ,   " _
                    & "         isnull(rtrim(D.GSHABAN),'')             as GSHABAN     ,   " _
                    & "         isnull(D.SEQ,'0')                       as SEQ         ,   " _
                    & "         isnull(rtrim(D.MANGUORG),'')            as MANGUORG    ,   " _
                    & "         isnull(rtrim(A.CAMPCODE),'')            as CAMPCODE    ,   " _
                    & "         isnull(rtrim(A.SHARYOTYPE),'')          as SHARYOTYPE  ,   " _
                    & "         isnull(rtrim(A.TSHABAN),'')             as TSHABAN     ,   " _
                    & "         CASE WHEN A.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(B.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD       ,   " _
                    & "         CASE WHEN A.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(B.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD      ,   " _
                    & "         CASE WHEN A.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(A.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_A     ,   " _
                    & "         CASE WHEN A.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(A.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_A    ,   " _
                    & "         CASE WHEN B.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(B.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_B     ,   " _
                    & "         CASE WHEN B.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(B.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_B    ,   " _
                    & "         CASE WHEN C.STYMD IS NULL THEN ''                          " _
                    & "              ELSE FORMAT(C.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD_C     ,   " _
                    & "         CASE WHEN C.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(C.ENDYMD,'yyyy/MM/dd')                    " _
                    & "         END                                     as ENDYMD_C    ,   " _
                    & "         FORMAT(getdate(),'yyyy/MM/dd')          as STYMD_S     ,   " _
                    & "         FORMAT(getdate(),'yyyy/MM/dd')          as ENDYMD_S    ,   " _
                    & "         isnull(rtrim(C.DELFLG),'0')             as DELFLG      ,   " _
                    & "         ''                                      as INITYMD     ,   " _
                    & "         ''                                      as UPDYMD      ,   " _
                    & "         ''                                      as UPDUSER     ,   " _
                    & "         isnull(rtrim(A.SHARYOSTATUS),'')        as SHARYOSTATUS,   " _
                    & "         isnull(rtrim(D.SHARYOINFO1),'')         as SHARYOINFO1 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO2),'')         as SHARYOINFO2 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO3),'')         as SHARYOINFO3 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO4),'')         as SHARYOINFO4 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO5),'')         as SHARYOINFO5 ,   " _
                    & "         isnull(rtrim(D.SHARYOINFO6),'')         as SHARYOINFO6 ,   " _
                    & "         ''                                      as MANGMORGNAME,   " _
                    & "         ''                                      as MANGSORGNAME,   " _
                    & "         ''                                      as MANGOILTYPENAME," _
                    & "         ''                                      as MANGOWNCODENAME," _
                    & "         ''                                      as MANGOWNCONTNAME," _
                    & "         ''                                      as MANGSUPPLNAME,  " _
                    & "         ''                                      as MANGUORGNAME,   " _
                    & "         ''                                      as BASELEASENAME , " _
                    & "         ''                                      as FCTRAXLENAME,   " _
                    & "         ''                                      as FCTRDPRNAME ,   " _
                    & "         ''                                      as FCTRFUELMATENAME," _
                    & "         ''                                      as FCTRSHFTNUMNAME," _
                    & "         ''                                      as FCTRSUSPNAME,   " _
                    & "         ''                                      as FCTRTMISSIONNAME," _
                    & "         ''                                      as FCTRUREANAME,   " _
                    & "         ''                                      as OTNKBPIPENAME,  " _
                    & "         ''                                      as OTNKVAPORNAME,  " _
                    & "         ''                                      as OTNKCVALVENAME, " _
                    & "         ''                                      as OTNKDCDNAME ,   " _
                    & "         ''                                      as OTNKDETECTORNAME," _
                    & "         ''                                      as OTNKDISGORGENAME," _
                    & "         ''                                      as OTNKHTECHNAME,  " _
                    & "         ''                                      as OTNKLVALVENAME, " _
                    & "         ''                                      as OTNKMATERIALNAME," _
                    & "         ''                                      as OTNKPIPENAME,   " _
                    & "         ''                                      as OTNKPIPESIZENAME," _
                    & "         ''                                      as OTNKPUMPNAME,   " _
                    & "         ''                                      as HPRSINSULATENAME," _
                    & "         ''                                      as HPRSMATRNAME,   " _
                    & "         ''                                      as HPRSPIPENAME,   " _
                    & "         ''                                      as HPRSPIPENUMNAME," _
                    & "         ''                                      as HPRSPUMPNAME,   " _
                    & "         ''                                      as HPRSRESSRENAME, " _
                    & "         ''                                      as HPRSSTRUCTNAME, " _
                    & "         ''                                      as HPRSVALVENAME,  " _
                    & "         ''                                      as CHEMDISGORGENAME," _
                    & "         ''                                      as CHEMHOSENAME,   " _
                    & "         ''                                      as CHEMMANOMTRNAME," _
                    & "         ''                                      as CHEMMATERIALNAME," _
                    & "         ''                                      as CHEMPMPDRNAME,  " _
                    & "         ''                                      as CHEMPRESDRVNAME," _
                    & "         ''                                      as CHEMPRESEQNAME, " _
                    & "         ''                                      as CHEMPUMPNAME,   " _
                    & "         ''                                      as CHEMSTRUCTNAME, " _
                    & "         ''                                      as CHEMTHERMNAME,  " _
                    & "         ''                                      as OTHRBMONITORNAME," _
                    & "         ''                                      as OTHRBSONARNAME, " _
                    & "         ''                                      as FCTRTIRENAME,   " _
                    & "         ''                                      as OTHRDRRECORDNAME," _
                    & "         ''                                      as OTHRPAINTINGNAME," _
                    & "         ''                                      as OTHRRADIOCONNAME," _
                    & "         ''                                      as OTHRRTARGETNAME," _
                    & "         ''                                      as OTHRTERMINALNAME," _
                    & "         ''                                      as MANGPROD1NAME,  " _
                    & "         ''                                      as MANGPROD2NAME,  " _
                    & "         ''                                      as FCTRSMAKERNAME, " _
                    & "         ''                                      as FCTRTMAKERNAME, " _
                    & "         ''                                      as OTNKEXHASIZENAME," _
                    & "         ''                                      as HPRSPMPDRNAME,  " _
                    & "         ''                                      as HPRSHOSENAME ,  " _
                    & "         ''                                      as CONTSHAPENAME,  " _
                    & "         ''                                      as CONTPUMPNAME ,  " _
                    & "         ''                                      as CONTPMPDRNAME,  " _
                    & "         ''                                      as OTHRTPMSNAME ,  " _
                    & "         ''                                      as OTNKTMAKERNAME, " _
                    & "         ''                                      as HPRSTMAKERNAME, " _
                    & "         ''                                      as CHEMTMAKERNAME, " _
                    & "         ''                                      as CONTTMAKERNAME, " _
                    & "         ''                                      as INSKBNNAME  ,   " _
                    & "         ''                                      as SHARYOSTATUSNAME" _
                    & " FROM       MA002_SHARYOA       A                                   " _
                    & " INNER JOIN MA003_SHARYOB       B                             ON    " _
                    & "             B.CAMPCODE        = A.CAMPCODE                         " _
                    & "       and   B.SHARYOTYPE      = A.SHARYOTYPE                       " _
                    & "       and   B.TSHABAN         = A.TSHABAN                          " _
                    & "       and   B.STYMD          <= @P05                               " _
                    & "       and   B.ENDYMD         >= @P04                               " _
                    & "       and   B.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                    & " LEFT  JOIN MA004_SHARYOC       C                             ON    " _
                    & "             C.CAMPCODE        = A.CAMPCODE                         " _
                    & "       and   C.SHARYOTYPE      = A.SHARYOTYPE                       " _
                    & "       and   C.TSHABAN         = A.TSHABAN                          " _
                    & "       and   C.STYMD          <= A.ENDYMD                           " _
                    & "       and   C.ENDYMD         >= A.STYMD                            " _
                    & "       and   C.ENDYMD          = (                                  " _
                    & "          select                                                    " _
                    & "                 max(ENDYMD)                                        " _
                    & "          from     MA004_SHARYOC      MXC                           " _
                    & "          where                                                     " _
                    & "                    MXC.CAMPCODE      = A.CAMPCODE                  " _
                    & "                and MXC.SHARYOTYPE    = A.SHARYOTYPE                " _
                    & "                and MXC.TSHABAN       = A.TSHABAN                   " _
                    & "                and MXC.STYMD        <= A.ENDYMD                    " _
                    & "                and MXC.ENDYMD       >= A.STYMD                     " _
                    & "                and MXC.DELFLG       <> '1'                         " _
                    & "       )                                                            " _
                    & "       and   C.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                    & " LEFT  JOIN MA006_SHABANORG     D                             ON    " _
                    & "             D.CAMPCODE        = A.CAMPCODE                         " _
                    & "       and   (                                                      " _
                    & "                (                                                   " _
                    & "                      D.SHARYOTYPEF     = A.SHARYOTYPE              " _
                    & "                  and D.TSHABANF        = A.TSHABAN                 " _
                    & "                )                                                   " _
                    & "             or                                                     " _
                    & "                (                                                   " _
                    & "                      D.SHARYOTYPEB     = A.SHARYOTYPE              " _
                    & "                  and D.TSHABANB        = A.TSHABAN                 " _
                    & "                )                                                   " _
                    & "             or                                                     " _
                    & "                (                                                   " _
                    & "                      D.SHARYOTYPEB2    = A.SHARYOTYPE              " _
                    & "                  and D.TSHABANB2       = A.TSHABAN                 " _
                    & "                )                                                   " _
                    & "             )                                                      " _
                    & "       and   D.DELFLG         <> '" & C_DELETE_FLG.DELETE & "'      " _
                    & " INNER JOIN S0006_ROLE          Y                               ON  " _
                    & "             Y.CAMPCODE        = A.CAMPCODE                         " _
                    & "       and   (                                                      " _
                    & "                   Y.CODE        = A.MANGMORG                       " _
                    & "               or  Y.CODE        = A.MANGSORG                       " _
                    & "             )                                                      " _
                    & "       and   Y.OBJECT          = 'ORG'                              " _
                    & "       and   Y.ROLE            = @P01                               " _
                    & "       and   Y.STYMD          <= @P03                               " _
                    & "       and   Y.ENDYMD         >= @P03                               " _
                    & "       and   Y.DELFLG         <> '1'                                " _
                    & " WHERE                                                              " _
                    & "             A.CAMPCODE        = @P02                               " _
                    & "       and   A.STYMD          <= @P05                               " _
                    & "       and   A.ENDYMD         >= @P04                               " _
                    & "       and   A.DELFLG         <> '1'                                " _
                    & " ORDER BY B.SHARYOTYPE ASC, B.TSHABAN ASC, A.STYMD DESC, B.STYMD DESC, C.STYMD "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Date)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)

                    PARA1.Value = Master.ROLE_ORG
                    PARA2.Value = work.WF_SEL_CAMPCODE.Text
                    PARA3.Value = Date.Now
                    PARA4.Value = work.WF_SEL_STYMD.Text       '有効期限(開始)
                    PARA5.Value = work.WF_SEL_ENDYMD.Text      '有効期限(終了)
                    PARA6.Value = work.WF_SEL_STYMD.Text       '対象年度(開始)
                    PARA7.Value = work.WF_SEL_ENDYMD.Text      '対象年度(終了)

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MA0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'MA0003tbl値設定
                        Dim WW_DATA_CNT As Integer = -1

                        While SQLdr.Read

                            '○テーブル初期化
                            Dim MA0003row As DataRow = MA0003tbl.NewRow()
                            Dim WW_DATE As Date

                            '○データ設定

                            '固定項目
                            WW_DATA_CNT = WW_DATA_CNT + 1
                            MA0003row("WORK_NO") = WW_DATA_CNT.ToString()
                            MA0003row("LINECNT") = 0
                            MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            If IsDBNull(SQLdr("TIMSTP")) Then
                                MA0003row("TIMSTP") = "0"
                            Else
                                MA0003row("TIMSTP") = SQLdr("TIMSTP")
                            End If

                            MA0003row("SELECT") = 1   '1:表示
                            MA0003row("HIDDEN") = 0   '0:表示

                            '画面毎の設定項目
                            MA0003row("CAMPCODE") = SQLdr("CAMPCODE")
                            MA0003row("SHARYOTYPE") = SQLdr("SHARYOTYPE")
                            MA0003row("TSHABAN") = SQLdr("TSHABAN")

                            MA0003row("STYMD") = If(SQLdr("STYMD"), "")
                            MA0003row("ENDYMD") = If(SQLdr("ENDYMD"), "")

                            'デバック用フィールド
                            MA0003row("STYMD_S") = If(SQLdr("STYMD_S"), "")
                            MA0003row("ENDYMD_S") = If(SQLdr("ENDYMD_S"), "")

                            MA0003row("STYMD_A") = If(SQLdr("STYMD_A"), "")
                            MA0003row("ENDYMD_A") = If(SQLdr("ENDYMD_A"), "")

                            MA0003row("STYMD_B") = If(SQLdr("STYMD_B"), "")
                            MA0003row("ENDYMD_B") = If(SQLdr("ENDYMD_B"), "")

                            MA0003row("STYMD_C") = If(SQLdr("STYMD_C"), "")
                            MA0003row("ENDYMD_C") = If(SQLdr("ENDYMD_C"), "")

                            MA0003row("DELFLG") = SQLdr("DELFLG")
                            MA0003row("SHARYOTYPEF") = SQLdr("SHARYOTYPEF")
                            MA0003row("TSHABANF") = SQLdr("TSHABANF")
                            MA0003row("SHARYOTYPEB") = SQLdr("SHARYOTYPEB")
                            MA0003row("TSHABANB") = SQLdr("TSHABANB")
                            MA0003row("SHARYOTYPEB2") = SQLdr("SHARYOTYPEB2")
                            MA0003row("TSHABANB2") = SQLdr("TSHABANB2")
                            MA0003row("SHARYOTYPEB3") = SQLdr("SHARYOTYPEB3")
                            MA0003row("TSHABANB3") = SQLdr("TSHABANB3")
                            MA0003row("GSHABAN") = SQLdr("GSHABAN")
                            MA0003row("SEQ") = SQLdr("SEQ")
                            MA0003row("MANGMORG") = SQLdr("MANGMORG")
                            MA0003row("MANGSORG") = SQLdr("MANGSORG")
                            MA0003row("MANGOILTYPE") = SQLdr("MANGOILTYPE")
                            MA0003row("MANGOWNCODE") = SQLdr("MANGOWNCODE")
                            MA0003row("MANGOWNCONT") = SQLdr("MANGOWNCONT")
                            MA0003row("MANGSHAFUKU") = SQLdr("MANGSHAFUKU")
                            MA0003row("MANGSUPPL") = SQLdr("MANGSUPPL")
                            MA0003row("MANGTTLDIST") = SQLdr("MANGTTLDIST")
                            MA0003row("MANGUORG") = SQLdr("MANGUORG")
                            MA0003row("BASELEASE") = SQLdr("BASELEASE")
                            MA0003row("BASERAGE") = SQLdr("BASERAGE")
                            MA0003row("BASERAGEMM") = SQLdr("BASERAGEMM")
                            MA0003row("BASERAGEYY") = SQLdr("BASERAGEYY")
                            MA0003row("BASERDATE") = SQLdr("BASERDATE")
                            If IsDBNull(SQLdr("BASERDATE")) OrElse SQLdr("BASERDATE") = "" Then
                                MA0003row("BASERDATE") = ""
                                MA0003row("BASERAGEMM") = "0"
                                MA0003row("BASERAGEYY") = "0"
                                MA0003row("BASERAGE") = "0"
                            Else
                                MA0003row("BASERDATE") = SQLdr("BASERDATE")

                                Dim WW_DATENOW As Date = Date.Now
                                Dim WW_BASERAGEYY As Integer
                                Dim WW_BASERAGE As Integer
                                Dim WW_BASERAGEMM As Integer
                                WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                                WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                                WW_BASERAGEMM = WW_BASERAGE Mod 12
                                MA0003row("BASERAGEMM") = WW_BASERAGEMM
                                MA0003row("BASERAGEYY") = WW_BASERAGEYY
                                MA0003row("BASERAGE") = WW_BASERAGE
                            End If
                            MA0003row("FCTRAXLE") = SQLdr("FCTRAXLE")
                            MA0003row("FCTRDPR") = SQLdr("FCTRDPR")
                            MA0003row("FCTRFUELCAPA") = SQLdr("FCTRFUELCAPA")
                            MA0003row("FCTRFUELMATE") = SQLdr("FCTRFUELMATE")
                            MA0003row("FCTRRESERVE1") = SQLdr("FCTRRESERVE1")
                            MA0003row("FCTRRESERVE2") = SQLdr("FCTRRESERVE2")
                            MA0003row("FCTRRESERVE3") = SQLdr("FCTRRESERVE3")
                            MA0003row("FCTRRESERVE4") = SQLdr("FCTRRESERVE4")
                            MA0003row("FCTRRESERVE5") = SQLdr("FCTRRESERVE5")
                            MA0003row("FCTRSHFTNUM") = SQLdr("FCTRSHFTNUM")
                            MA0003row("FCTRSUSP") = SQLdr("FCTRSUSP")
                            MA0003row("FCTRTIRE") = SQLdr("FCTRTIRE")
                            MA0003row("FCTRTMISSION") = SQLdr("FCTRTMISSION")
                            MA0003row("FCTRUREA") = SQLdr("FCTRUREA")
                            MA0003row("OTNKBPIPE") = SQLdr("OTNKBPIPE")
                            MA0003row("OTNKCELLNO") = SQLdr("OTNKCELLNO")
                            MA0003row("OTNKVAPOR") = SQLdr("OTNKVAPOR")
                            MA0003row("OTNKCELPART") = SQLdr("OTNKCELPART")
                            MA0003row("OTNKCVALVE") = SQLdr("OTNKCVALVE")
                            MA0003row("OTNKDCD") = SQLdr("OTNKDCD")
                            MA0003row("OTNKDETECTOR") = SQLdr("OTNKDETECTOR")
                            MA0003row("OTNKDISGORGE") = SQLdr("OTNKDISGORGE")
                            MA0003row("OTNKHTECH") = SQLdr("OTNKHTECH")
                            MA0003row("OTNKINSSTAT") = SQLdr("OTNKINSSTAT")

                            MA0003row("OTNKINSYMD") = If(SQLdr("OTNKINSYMD"), "")
                            MA0003row("OTNKLVALVE") = SQLdr("OTNKLVALVE")
                            MA0003row("OTNKMATERIAL") = SQLdr("OTNKMATERIAL")
                            MA0003row("OTNKPIPE") = SQLdr("OTNKPIPE")
                            MA0003row("OTNKPIPESIZE") = SQLdr("OTNKPIPESIZE")
                            MA0003row("OTNKPUMP") = SQLdr("OTNKPUMP")
                            MA0003row("OTNKTINSNO") = SQLdr("OTNKTINSNO")
                            MA0003row("HPRSINSISTAT") = SQLdr("HPRSINSISTAT")
                            MA0003row("HPRSINSIYMD") = If(SQLdr("HPRSINSIYMD"), "")

                            MA0003row("HPRSINSULATE") = SQLdr("HPRSINSULATE")
                            MA0003row("HPRSMATR") = SQLdr("HPRSMATR")
                            MA0003row("HPRSPIPE") = SQLdr("HPRSPIPE")
                            MA0003row("HPRSPIPENUM") = SQLdr("HPRSPIPENUM")
                            MA0003row("HPRSPUMP") = SQLdr("HPRSPUMP")
                            MA0003row("HPRSRESSRE") = SQLdr("HPRSRESSRE")
                            MA0003row("HPRSSERNO") = SQLdr("HPRSSERNO")
                            MA0003row("HPRSSTRUCT") = SQLdr("HPRSSTRUCT")
                            MA0003row("HPRSVALVE") = SQLdr("HPRSVALVE")
                            MA0003row("CHEMCELLNO") = SQLdr("CHEMCELLNO")
                            MA0003row("CHEMCELPART") = SQLdr("CHEMCELPART")
                            MA0003row("CHEMDISGORGE") = SQLdr("CHEMDISGORGE")
                            MA0003row("CHEMHOSE") = SQLdr("CHEMHOSE")
                            MA0003row("CHEMINSSTAT") = SQLdr("CHEMINSSTAT")
                            MA0003row("CHEMINSYMD") = If(SQLdr("CHEMINSYMD"), "")

                            MA0003row("CHEMMANOMTR") = SQLdr("CHEMMANOMTR")
                            MA0003row("CHEMMATERIAL") = SQLdr("CHEMMATERIAL")
                            MA0003row("CHEMPMPDR") = SQLdr("CHEMPMPDR")
                            MA0003row("CHEMPRESDRV") = SQLdr("CHEMPRESDRV")
                            MA0003row("CHEMPRESEQ") = SQLdr("CHEMPRESEQ")
                            MA0003row("CHEMPUMP") = SQLdr("CHEMPUMP")
                            MA0003row("CHEMSTRUCT") = SQLdr("CHEMSTRUCT")
                            MA0003row("CHEMTHERM") = SQLdr("CHEMTHERM")
                            MA0003row("CHEMTINSNO") = SQLdr("CHEMTINSNO")

                            MA0003row("CHEMTINSNYMD") = If(SQLdr("CHEMTINSNYMD"), "")
                            MA0003row("CHEMTINSYMD") = If(SQLdr("CHEMTINSYMD"), "")

                            MA0003row("OFFCRESERVE1") = SQLdr("OFFCRESERVE1")
                            MA0003row("OFFCRESERVE2") = SQLdr("OFFCRESERVE2")
                            MA0003row("OFFCRESERVE3") = SQLdr("OFFCRESERVE3")
                            MA0003row("OFFCRESERVE4") = SQLdr("OFFCRESERVE4")
                            MA0003row("OFFCRESERVE5") = SQLdr("OFFCRESERVE5")
                            MA0003row("OTHRBMONITOR") = SQLdr("OTHRBMONITOR")
                            MA0003row("OTHRBSONAR") = SQLdr("OTHRBSONAR")
                            MA0003row("OTHRDOCO") = SQLdr("OTHRDOCO")
                            MA0003row("OTHRDRRECORD") = SQLdr("OTHRDRRECORD")
                            MA0003row("OTHRPAINTING") = SQLdr("OTHRPAINTING")
                            MA0003row("OTHRRADIOCON") = SQLdr("OTHRRADIOCON")
                            MA0003row("OTHRRTARGET") = SQLdr("OTHRRTARGET")
                            MA0003row("OTHRTERMINAL") = SQLdr("OTHRTERMINAL")
                            MA0003row("ACCTASST01") = SQLdr("ACCTASST01")
                            MA0003row("ACCTASST02") = SQLdr("ACCTASST02")
                            MA0003row("ACCTASST03") = SQLdr("ACCTASST03")
                            MA0003row("ACCTASST04") = SQLdr("ACCTASST04")
                            MA0003row("ACCTASST05") = SQLdr("ACCTASST05")
                            MA0003row("ACCTASST06") = SQLdr("ACCTASST06")
                            MA0003row("ACCTASST07") = SQLdr("ACCTASST07")
                            MA0003row("ACCTASST08") = SQLdr("ACCTASST08")
                            MA0003row("ACCTASST09") = SQLdr("ACCTASST09")
                            MA0003row("ACCTASST10") = SQLdr("ACCTASST10")
                            MA0003row("ACCTLEASE1") = SQLdr("ACCTLEASE1")
                            MA0003row("ACCTLEASE2") = SQLdr("ACCTLEASE2")
                            MA0003row("ACCTLEASE3") = SQLdr("ACCTLEASE3")
                            MA0003row("ACCTLEASE4") = SQLdr("ACCTLEASE4")
                            MA0003row("ACCTLEASE5") = SQLdr("ACCTLEASE5")
                            MA0003row("ACCTLSUPL1") = SQLdr("ACCTLSUPL1")
                            MA0003row("ACCTLSUPL2") = SQLdr("ACCTLSUPL2")
                            MA0003row("ACCTLSUPL3") = SQLdr("ACCTLSUPL3")
                            MA0003row("ACCTLSUPL4") = SQLdr("ACCTLSUPL4")
                            MA0003row("ACCTLSUPL5") = SQLdr("ACCTLSUPL5")
                            MA0003row("ACCTRCYCLE") = Format(Val(SQLdr("ACCTRCYCLE")), "#,#")

                            MA0003row("NOTES") = SQLdr("NOTES")
                            MA0003row("LICN5LDCAPA") = SQLdr("LICN5LDCAPA")
                            MA0003row("LICNCWEIGHT") = SQLdr("LICNCWEIGHT")
                            MA0003row("LICNFRAMENO") = SQLdr("LICNFRAMENO")
                            MA0003row("LICNLDCAPA") = SQLdr("LICNLDCAPA")
                            MA0003row("LICNMNFACT") = SQLdr("LICNMNFACT")
                            MA0003row("LICNMODEL") = SQLdr("LICNMODEL")
                            MA0003row("LICNMOTOR") = SQLdr("LICNMOTOR")
                            MA0003row("LICNPLTNO1") = SQLdr("LICNPLTNO1")
                            MA0003row("LICNPLTNO2") = SQLdr("LICNPLTNO2")
                            MA0003row("LICNTWEIGHT") = SQLdr("LICNTWEIGHT")
                            MA0003row("LICNWEIGHT") = SQLdr("LICNWEIGHT")

                            MA0003row("LICNYMD") = If(SQLdr("LICNYMD"), "")

                            If Val(SQLdr("TAXATAX")) = 0 Then
                                MA0003row("TAXATAX") = ""
                            Else
                                MA0003row("TAXATAX") = Format(Val(SQLdr("TAXATAX")), "#,#")
                            End If
                            If Val(SQLdr("TAXLINS")) = 0 Then
                                MA0003row("TAXLINS") = ""
                            Else
                                MA0003row("TAXLINS") = Format(Val(SQLdr("TAXLINS")), "#,#")
                            End If

                            MA0003row("TAXLINSYMD") = If(SQLdr("TAXLINSYMD"), "")

                            If Val(SQLdr("TAXVTAX")) = 0 Then
                                MA0003row("TAXVTAX") = ""
                            Else
                                MA0003row("TAXVTAX") = Format(Val(SQLdr("TAXVTAX")), "#,#")
                            End If

                            MA0003row("OTNKTINSNYMD") = If(SQLdr("OTNKTINSNYMD"), "")
                            MA0003row("OTNKTINSYMD") = If(SQLdr("OTNKTINSYMD"), "")
                            MA0003row("HPRSINSNYMD") = If(SQLdr("HPRSINSNYMD"), "")
                            MA0003row("HPRSINSYMD") = If(SQLdr("HPRSINSYMD"), "")
                            MA0003row("HPRSJINSYMD") = If(SQLdr("HPRSJINSYMD"), "")

                            MA0003row("INSKBN") = SQLdr("INSKBN")
                            MA0003row("MANGPROD1") = SQLdr("MANGPROD1")
                            MA0003row("MANGPROD2") = SQLdr("MANGPROD2")
                            MA0003row("FCTRSMAKER") = SQLdr("FCTRSMAKER")
                            MA0003row("FCTRTMAKER") = SQLdr("FCTRTMAKER")
                            MA0003row("OTNKEXHASIZE") = SQLdr("OTNKEXHASIZE")
                            MA0003row("HPRSPMPDR") = SQLdr("HPRSPMPDR")
                            MA0003row("HPRSHOSE") = SQLdr("HPRSHOSE")
                            MA0003row("CONTSHAPE") = SQLdr("CONTSHAPE")
                            MA0003row("CONTPUMP") = SQLdr("CONTPUMP")
                            MA0003row("CONTPMPDR") = SQLdr("CONTPMPDR")
                            MA0003row("OTHRTIRE1") = SQLdr("OTHRTIRE1")
                            MA0003row("OTHRTIRE2") = SQLdr("OTHRTIRE2")
                            MA0003row("OTHRTPMS") = SQLdr("OTHRTPMS")

                            MA0003row("OTNKTMAKER") = SQLdr("OTNKTMAKER")
                            MA0003row("HPRSTMAKER") = SQLdr("HPRSTMAKER")
                            MA0003row("CHEMTMAKER") = SQLdr("CHEMTMAKER")
                            MA0003row("CONTTMAKER") = SQLdr("CONTTMAKER")
                            MA0003row("SHARYOSTATUS") = SQLdr("SHARYOSTATUS")
                            MA0003row("SHARYOINFO1") = SQLdr("SHARYOINFO1")
                            MA0003row("SHARYOINFO2") = SQLdr("SHARYOINFO2")
                            MA0003row("SHARYOINFO3") = SQLdr("SHARYOINFO3")
                            MA0003row("SHARYOINFO4") = SQLdr("SHARYOINFO4")
                            MA0003row("SHARYOINFO5") = SQLdr("SHARYOINFO5")
                            MA0003row("SHARYOINFO6") = SQLdr("SHARYOINFO6")

                            '統一車番＋B開始年月日がブレイク
                            If MA0003row("SHARYOTYPE") = WW_SHARYOTYPE AndAlso
                                MA0003row("TSHABAN") = WW_TSHABAN AndAlso
                                MA0003row("STYMD_B") = WW_STYMD_B Then
                                MA0003row("SELECT") = 0
                            Else
                                MA0003row("SELECT") = 1
                                MA0003row("HIDDEN") = 0   '0:表示
                                '前回キー保存
                                WW_SHARYOTYPE = MA0003row("SHARYOTYPE")
                                WW_TSHABAN = MA0003row("TSHABAN")
                                WW_STYMD_B = MA0003row("STYMD_B")
                            End If

                            '○条件画面で指定に該当するデータを抽出
                            If MA0003row("SELECT") = 1 Then

                                '管理組織
                                Dim WW_SELECT_MORG As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_MORG.Text = "" Then
                                    WW_SELECT_MORG = 1
                                Else
                                    If work.WF_SEL_MORG.Text = MA0003row("MANGMORG") Then
                                        WW_SELECT_MORG = 1
                                    End If
                                End If
                                If WW_SELECT_MORG = 0 Then
                                    MA0003row("SELECT") = 0
                                End If


                                '条件画面で指定された設置部署を抽出
                                Dim WW_SELECT_SORG As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_SORG.Text = "" Then
                                    WW_SELECT_SORG = 1
                                Else
                                    If work.WF_SEL_SORG.Text = MA0003row("MANGSORG") Then
                                        WW_SELECT_SORG = 1
                                    End If
                                End If
                                If WW_SELECT_SORG = 0 Then
                                    MA0003row("SELECT") = 0
                                End If

                                '条件画面で指定された油種を抽出
                                Dim WW_SELECT_OILTYPE As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_OILTYPE1.Text = "" AndAlso
                                    work.WF_SEL_OILTYPE2.Text = "" Then
                                    WW_SELECT_OILTYPE = 1
                                Else
                                    If work.WF_SEL_OILTYPE1.Text = MA0003row("MANGOILTYPE") Then
                                        WW_SELECT_OILTYPE = 1
                                    End If
                                    If work.WF_SEL_OILTYPE2.Text = MA0003row("MANGOILTYPE") Then
                                        WW_SELECT_OILTYPE = 1
                                    End If
                                End If
                                If WW_SELECT_OILTYPE = 0 Then
                                    MA0003row("SELECT") = 0
                                End If

                                '条件画面で指定された荷主を抽出
                                Dim WW_SELECT_OWNER As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_OWNCODE1.Text = "" AndAlso
                                    work.WF_SEL_OWNCODE2.Text = "" Then
                                    WW_SELECT_OWNER = 1
                                Else
                                    If work.WF_SEL_OWNCODE1.Text <= MA0003row("MANGOWNCODE") AndAlso
                                        work.WF_SEL_OWNCODE2.Text >= MA0003row("MANGOWNCODE") Then
                                        WW_SELECT_OWNER = 1
                                    End If
                                End If
                                If WW_SELECT_OWNER = 0 Then
                                    MA0003row("SELECT") = 0
                                End If

                                '条件画面で指定された車両タイプを抽出
                                Dim WW_SELECT_SHAYO As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_SHARYOTYPE1.Text = "" AndAlso
                                    work.WF_SEL_SHARYOTYPE2.Text = "" AndAlso
                                    work.WF_SEL_SHARYOTYPE3.Text = "" AndAlso
                                    work.WF_SEL_SHARYOTYPE4.Text = "" AndAlso
                                    work.WF_SEL_SHARYOTYPE5.Text = "" Then
                                    WW_SELECT_SHAYO = 1
                                Else
                                    If work.WF_SEL_SHARYOTYPE1.Text = MA0003row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE2.Text = MA0003row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE3.Text = MA0003row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE4.Text = MA0003row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE5.Text = MA0003row("SHARYOTYPE") Then
                                        WW_SELECT_SHAYO = 1
                                    End If
                                End If
                                If WW_SELECT_SHAYO = 0 Then
                                    MA0003row("SELECT") = 0
                                End If
                            End If

                            '○抽出対象外の場合、名称取得、レコード追加しない
                            If MA0003row("SELECT") = 1 Then
                                '○名称付与
                                MA0003row("MANGMORGNAME") = ""                                                                                  '管理部署名
                                CODENAME_get("MANGMORG", MA0003row("MANGMORG"), MA0003row("MANGMORGNAME"), WW_DUMMY)
                                MA0003row("MANGSORGNAME") = ""                                                                                  '設置部署名
                                CODENAME_get("MANGSORG", MA0003row("MANGSORG"), MA0003row("MANGSORGNAME"), WW_DUMMY)
                                MA0003row("MANGOILTYPENAME") = ""                                                                               '油種名
                                CODENAME_get("MANGOILTYPE", MA0003row("MANGOILTYPE"), MA0003row("MANGOILTYPENAME"), WW_DUMMY)
                                MA0003row("MANGOWNCODENAME") = ""                                                                               '荷主名
                                CODENAME_get("MANGOWNCODE", MA0003row("MANGOWNCODE"), MA0003row("MANGOWNCODENAME"), WW_DUMMY)
                                MA0003row("MANGOWNCONTNAME") = ""                                                                               '契約区分名
                                CODENAME_get("MANGOWNCONT", MA0003row("MANGOWNCONT"), MA0003row("MANGOWNCONTNAME"), WW_DUMMY)
                                MA0003row("MANGSUPPLNAME") = ""                                                                                 '庸車会社名
                                CODENAME_get("MANGSUPPL", MA0003row("MANGSUPPL"), MA0003row("MANGSUPPLNAME"), WW_DUMMY)
                                MA0003row("MANGUORGNAME") = ""                                                                                  '運用部署名
                                CODENAME_get("MANGUORG", MA0003row("MANGUORG"), MA0003row("MANGUORGNAME"), WW_DUMMY)
                                MA0003row("BASELEASENAME") = ""                                                                                 '車両所有名
                                CODENAME_get("BASELEASE", MA0003row("BASELEASE"), MA0003row("BASELEASENAME"), WW_DUMMY)
                                MA0003row("FCTRAXLENAME") = ""                                                                                  'リフトアクスル名
                                CODENAME_get("FCTRAXLE", MA0003row("FCTRAXLE"), MA0003row("FCTRAXLENAME"), WW_DUMMY)
                                MA0003row("FCTRTMAKERNAME") = ""                                                                                'タンクメーカー名
                                MA0003row("FCTRDPRNAME") = ""                                                                                   'DPR名
                                CODENAME_get("FCTRDPR", MA0003row("FCTRDPR"), MA0003row("FCTRDPRNAME"), WW_DUMMY)
                                MA0003row("FCTRFUELMATENAME") = ""                                                                              '燃料タンク材質名
                                CODENAME_get("FCTRFUELMATE", MA0003row("FCTRFUELMATE"), MA0003row("FCTRFUELMATENAME"), WW_DUMMY)
                                MA0003row("FCTRSHFTNUMNAME") = ""                                                                               '軸数名
                                CODENAME_get("FCTRSHFTNUM", MA0003row("FCTRSHFTNUM"), MA0003row("FCTRSHFTNUMNAME"), WW_DUMMY)
                                MA0003row("FCTRSUSPNAME") = ""                                                                                  'サスペンション種類名
                                CODENAME_get("FCTRSUSP", MA0003row("FCTRSUSP"), MA0003row("FCTRSUSPNAME"), WW_DUMMY)
                                MA0003row("FCTRTMISSIONNAME") = ""                                                                              'ミッション名
                                CODENAME_get("FCTRTMISSION", MA0003row("FCTRTMISSION"), MA0003row("FCTRTMISSIONNAME"), WW_DUMMY)
                                MA0003row("FCTRUREANAME") = ""                                                                                  '尿素名
                                CODENAME_get("FCTRUREA", MA0003row("FCTRUREA"), MA0003row("FCTRUREANAME"), WW_DUMMY)
                                MA0003row("OTNKBPIPENAME") = ""                                                                                 '後配管名
                                CODENAME_get("OTNKBPIPE", MA0003row("OTNKBPIPE"), MA0003row("OTNKBPIPENAME"), WW_DUMMY)
                                MA0003row("OTNKVAPORNAME") = ""                                                                                 'ベーパー名
                                CODENAME_get("OTNKVAPOR", MA0003row("OTNKVAPOR"), MA0003row("OTNKVAPORNAME"), WW_DUMMY)
                                MA0003row("OTNKCVALVENAME") = ""                                                                                '中間ﾊﾞﾙﾌﾞ有無名
                                CODENAME_get("OTNKCVALVE", MA0003row("OTNKCVALVE"), MA0003row("OTNKCVALVENAME"), WW_DUMMY)
                                MA0003row("OTNKDCDNAME") = ""                                                                                   'ＤＣＤ装備名
                                CODENAME_get("OTNKDCD", MA0003row("OTNKDCD"), MA0003row("OTNKDCDNAME"), WW_DUMMY)
                                MA0003row("FCTRSMAKERNAME") = ""                                                                                '車両メーカー
                                CODENAME_get("FCTRSMAKER", MA0003row("FCTRSMAKER"), MA0003row("FCTRSMAKERNAME"), WW_DUMMY)
                                MA0003row("OTNKDETECTORNAME") = ""                                                                              '検水管名
                                CODENAME_get("OTNKDETECTOR", MA0003row("OTNKDETECTOR"), MA0003row("OTNKDETECTORNAME"), WW_DUMMY)
                                MA0003row("OTNKDISGORGENAME") = ""                                                                              '吐出口名
                                CODENAME_get("OTNKDISGORGE", MA0003row("OTNKDISGORGE"), MA0003row("OTNKDISGORGENAME"), WW_DUMMY)
                                MA0003row("OTNKHTECHNAME") = ""                                                                                 'ハイテク種別名
                                CODENAME_get("OTNKHTECH", MA0003row("OTNKHTECH"), MA0003row("OTNKHTECHNAME"), WW_DUMMY)
                                MA0003row("OTNKLVALVENAME") = ""                                                                                '底弁形式名
                                CODENAME_get("OTNKLVALVE", MA0003row("OTNKLVALVE"), MA0003row("OTNKLVALVENAME"), WW_DUMMY)
                                MA0003row("OTNKMATERIALNAME") = ""                                                                              'タンク材質名
                                CODENAME_get("OTNKMATERIAL", MA0003row("OTNKMATERIAL"), MA0003row("OTNKMATERIALNAME"), WW_DUMMY)
                                MA0003row("OTNKPIPENAME") = ""                                                                                  '配管形態名
                                CODENAME_get("OTNKPIPE", MA0003row("OTNKPIPE"), MA0003row("OTNKPIPENAME"), WW_DUMMY)
                                MA0003row("OTNKPIPESIZENAME") = ""                                                                              '配管サイズ名
                                CODENAME_get("OTNKPIPESIZE", MA0003row("OTNKPIPESIZE"), MA0003row("OTNKPIPESIZENAME"), WW_DUMMY)
                                MA0003row("OTNKPUMPNAME") = ""                                                                                  'ポンプ名
                                CODENAME_get("OTNKPUMP", MA0003row("OTNKPUMP"), MA0003row("OTNKPUMPNAME"), WW_DUMMY)
                                MA0003row("HPRSPMPDRNAME") = ""                                                                                 'ポンプ駆動方法
                                CODENAME_get("HPRSPMPDR", MA0003row("HPRSPMPDR"), MA0003row("HPRSPMPDRNAME"), WW_DUMMY)
                                MA0003row("HPRSINSULATENAME") = ""                                                                              '断熱構造名
                                CODENAME_get("HPRSINSULATE", MA0003row("HPRSINSULATE"), MA0003row("HPRSINSULATENAME"), WW_DUMMY)
                                MA0003row("HPRSMATRNAME") = ""                                                                                  'タンク材質名
                                CODENAME_get("HPRSMATR", MA0003row("HPRSMATR"), MA0003row("HPRSMATRNAME"), WW_DUMMY)
                                MA0003row("HPRSPIPENAME") = ""                                                                                  '配管形状（仮）名
                                CODENAME_get("HPRSPIPE", MA0003row("HPRSPIPE"), MA0003row("HPRSPIPENAME"), WW_DUMMY)
                                MA0003row("HPRSPIPENUMNAME") = ""                                                                               '配管口数名
                                CODENAME_get("HPRSPIPENUM", MA0003row("HPRSPIPENUM"), MA0003row("HPRSPIPENUMNAME"), WW_DUMMY)
                                MA0003row("HPRSPUMPNAME") = ""                                                                                  'ポンプ名
                                CODENAME_get("HPRSPUMP", MA0003row("HPRSPUMP"), MA0003row("HPRSPUMPNAME"), WW_DUMMY)
                                MA0003row("HPRSRESSRENAME") = ""                                                                                '加圧器名
                                CODENAME_get("HPRSRESSRE", MA0003row("HPRSRESSRE"), MA0003row("HPRSRESSRENAME"), WW_DUMMY)
                                MA0003row("HPRSSTRUCTNAME") = ""                                                                                'タンク構造名
                                CODENAME_get("HPRSSTRUCT", MA0003row("HPRSSTRUCT"), MA0003row("HPRSSTRUCTNAME"), WW_DUMMY)
                                MA0003row("HPRSVALVENAME") = ""                                                                                 '底弁形式名
                                CODENAME_get("HPRSVALVE", MA0003row("HPRSVALVE"), MA0003row("HPRSVALVENAME"), WW_DUMMY)
                                MA0003row("CHEMDISGORGENAME") = ""                                                                              '吐出口名
                                CODENAME_get("CHEMDISGORGE", MA0003row("CHEMDISGORGE"), MA0003row("CHEMDISGORGENAME"), WW_DUMMY)
                                MA0003row("CHEMHOSENAME") = ""                                                                                  'ホースボックス名
                                CODENAME_get("CHEMHOSE", MA0003row("CHEMHOSE"), MA0003row("CHEMHOSENAME"), WW_DUMMY)
                                MA0003row("CHEMMANOMTRNAME") = ""                                                                               '圧力計名
                                CODENAME_get("CHEMMANOMTR", MA0003row("CHEMMANOMTR"), MA0003row("CHEMMANOMTRNAME"), WW_DUMMY)
                                MA0003row("CHEMMATERIALNAME") = ""                                                                              'タンク材質名
                                CODENAME_get("CHEMMATERIAL", MA0003row("CHEMMATERIAL"), MA0003row("CHEMMATERIALNAME"), WW_DUMMY)
                                MA0003row("CHEMPMPDRNAME") = ""                                                                                 'ポンプ駆動方法名
                                CODENAME_get("CHEMPMPDR", MA0003row("CHEMPMPDR"), MA0003row("CHEMPMPDRNAME"), WW_DUMMY)
                                MA0003row("CHEMPRESDRVNAME") = ""                                                                               '加温装置名
                                CODENAME_get("CHEMPRESDRV", MA0003row("CHEMPRESDRV"), MA0003row("CHEMPRESDRVNAME"), WW_DUMMY)
                                MA0003row("CHEMPRESEQNAME") = ""                                                                                '均圧配管名
                                CODENAME_get("CHEMPRESEQ", MA0003row("CHEMPRESEQ"), MA0003row("CHEMPRESEQNAME"), WW_DUMMY)
                                MA0003row("CHEMPUMPNAME") = ""                                                                                  'ポンプ名
                                CODENAME_get("CHEMPUMP", MA0003row("CHEMPUMP"), MA0003row("CHEMPUMPNAME"), WW_DUMMY)
                                MA0003row("CHEMSTRUCTNAME") = ""                                                                                'タンク構造名
                                CODENAME_get("CHEMSTRUCT", MA0003row("CHEMSTRUCT"), MA0003row("CHEMSTRUCTNAME"), WW_DUMMY)
                                MA0003row("CHEMTHERMNAME") = ""                                                                                 '温度計名
                                CODENAME_get("CHEMTHERM", MA0003row("CHEMTHERM"), MA0003row("CHEMTHERMNAME"), WW_DUMMY)
                                MA0003row("OTHRBMONITORNAME") = ""                                                                              'バックモニター名
                                CODENAME_get("OTHRBMONITOR", MA0003row("OTHRBMONITOR"), MA0003row("OTHRBMONITORNAME"), WW_DUMMY)
                                MA0003row("OTHRBSONARNAME") = ""                                                                                'バックソナー名
                                CODENAME_get("OTHRBSONAR", MA0003row("OTHRBSONAR"), MA0003row("OTHRBSONARNAME"), WW_DUMMY)
                                MA0003row("FCTRTIRENAME") = ""                                                                                  'ＤoCoですCar番号名
                                CODENAME_get("FCTRTIRE", MA0003row("FCTRTIRE"), MA0003row("FCTRTIRENAME"), WW_DUMMY)
                                MA0003row("OTHRDRRECORDNAME") = ""                                                                              'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
                                CODENAME_get("OTHRDRRECORD", MA0003row("OTHRDRRECORD"), MA0003row("OTHRDRRECORDNAME"), WW_DUMMY)
                                MA0003row("OTHRPAINTINGNAME") = ""                                                                              '塗装名
                                CODENAME_get("OTHRPAINTING", MA0003row("OTHRPAINTING"), MA0003row("OTHRPAINTINGNAME"), WW_DUMMY)
                                MA0003row("OTHRRADIOCONNAME") = ""                                                                              '無線（有・無）名
                                CODENAME_get("OTHRRADIOCON", MA0003row("OTHRRADIOCON"), MA0003row("OTHRRADIOCONNAME"), WW_DUMMY)
                                MA0003row("OTHRRTARGETNAME") = ""                                                                               '一括修理非対象車名
                                CODENAME_get("OTHRRTARGET", MA0003row("OTHRRTARGET"), MA0003row("OTHRRTARGETNAME"), WW_DUMMY)
                                MA0003row("OTHRTERMINALNAME") = ""                                                                              '車載端末名
                                CODENAME_get("OTHRTERMINAL", MA0003row("OTHRTERMINAL"), MA0003row("OTHRTERMINALNAME"), WW_DUMMY)
                                MA0003row("MANGPROD1NAME") = ""                                                                                 '品名１
                                CODENAME_get("MANGPROD1", MA0003row("MANGPROD1"), MA0003row("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0003row("MANGOILTYPE"))})
                                MA0003row("MANGPROD2NAME") = ""                                                                                 '品名２
                                CODENAME_get("MANGPROD2", MA0003row("MANGPROD2"), MA0003row("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0003row("MANGOILTYPE")), CStr(MA0003row("MANGPROD1"))})
                                MA0003row("OTNKEXHASIZENAME") = ""                                                                              '吐出口サイズ
                                CODENAME_get("OTNKEXHASIZE", MA0003row("OTNKEXHASIZE"), MA0003row("OTNKEXHASIZENAME"), WW_DUMMY)
                                MA0003row("HPRSHOSENAME") = ""                                                                                  'ホースボックス
                                CODENAME_get("HPRSHOSE", MA0003row("HPRSHOSE"), MA0003row("HPRSHOSENAME"), WW_DUMMY)
                                MA0003row("CONTSHAPENAME") = ""                                                                                 'シャーシ形状
                                CODENAME_get("CONTSHAPE", MA0003row("CONTSHAPE"), MA0003row("CONTSHAPENAME"), WW_DUMMY)
                                MA0003row("CONTPUMPNAME") = ""                                                                                  'ポンプ
                                CODENAME_get("CONTPUMP", MA0003row("CONTPUMP"), MA0003row("CONTPUMPNAME"), WW_DUMMY)
                                MA0003row("CONTPMPDRNAME") = ""                                                                                 'ポンプ駆動方法
                                CODENAME_get("CONTPMPDR", MA0003row("CONTPMPDR"), MA0003row("CONTPMPDRNAME"), WW_DUMMY)
                                MA0003row("OTHRTPMSNAME") = ""                                                                                  'TPMS
                                CODENAME_get("OTHRTPMS", MA0003row("OTHRTPMS"), MA0003row("OTHRTPMSNAME"), WW_DUMMY)
                                MA0003row("OTNKTMAKERNAME") = ""                                                                                '石油タンクメーカー名
                                CODENAME_get("OTNKTMAKER", MA0003row("OTNKTMAKER"), MA0003row("OTNKTMAKERNAME"), WW_DUMMY)
                                MA0003row("HPRSTMAKERNAME") = ""                                                                                '高圧タンクメーカー名
                                CODENAME_get("HPRSTMAKER", MA0003row("HPRSTMAKER"), MA0003row("HPRSTMAKERNAME"), WW_DUMMY)
                                MA0003row("CHEMTMAKERNAME") = ""                                                                                '化成品タンクメーカー名
                                CODENAME_get("CHEMTMAKER", MA0003row("CHEMTMAKER"), MA0003row("CHEMTMAKERNAME"), WW_DUMMY)
                                MA0003row("CONTTMAKERNAME") = ""                                                                                'コンテナタンクメーカー名
                                CODENAME_get("CONTTMAKER", MA0003row("CONTTMAKER"), MA0003row("CONTTMAKERNAME"), WW_DUMMY)
                                MA0003row("SHARYOSTATUSNAME") = ""                                                                              '運行状況名名
                                CODENAME_get("SHARYOSTATUS", MA0003row("SHARYOSTATUS"), MA0003row("SHARYOSTATUSNAME"), WW_DUMMY)
                                MA0003row("INSKBNNAME") = ""                                                                                    '検査区分名
                                CODENAME_get("INSKBN", MA0003row("INSKBN"), MA0003row("INSKBNNAME"), WW_DUMMY)

                                MA0003tbl.Rows.Add(MA0003row)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA002_SHARYOA SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA002_SHARYOA Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = MA0003tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MA0003tbl = CS0026TBLSORT.TABLE
        End If

        'MA3に対するMA2登録チェック
        MA02SYARYOA_Chk()

    End Sub
    ''' <summary>
    ''' 単項目入力チェック
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub INPUT_CHEK(ByRef O_RTN As String)

        '○初期処理
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINEERR_SW As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_SHARYOTYPE2 As String = ""   '車両タイプ分類（前、後）
        Dim WW_TEXT As String = ""

        For Each MA0003INProw As DataRow In MA0003INPtbl.Rows

            WW_LINEERR_SW = ""

            '○単項目チェック
            '・キー項目(会社：CAMPCODE)
            WW_TEXT = MA0003INProw("CAMPCODE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MA0003INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0003INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MA0003INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。 , "
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(統一車番：SHARYOTYPE)…車両タイプ
            WW_TEXT = MA0003INProw("SHARYOTYPE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE", MA0003INProw("SHARYOTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0003INProw("SHARYOTYPE") = ""
                Else
                    CODENAME_get("SHARYOTYPE", MA0003INProw("SHARYOTYPE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番・車両タイプエラー)です。"
                        WW_CheckMES2 = " 車両タイプにエラーがあります。 , "
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    '車両タイプ分類（値２を取得）
                    GetSharyoType2(MA0003INProw("SHARYOTYPE"), WW_SHARYOTYPE2, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番エラー)です。"
                        WW_CheckMES2 = " 車両タイプにエラーがあります。 , "
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(統一車番・車両タイプエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '・キー項目(統一車番：TSHABAN)…連番
            '空白の場合、新規扱い
            If MA0003INProw("TSHABAN") <> "" Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TSHABAN", MA0003INProw("TSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, True)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番・連番エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '・キー項目(有効年月日：STYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", MA0003INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(有効年月日：ENDYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MA0003INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(削除フラグ：DELFLG)
            WW_TEXT = MA0003INProw("DELFLG")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MA0003INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0003INProw("DELFLG") = C_DELETE_FLG.ALIVE
                Else
                    CODENAME_get("DELFLG", MA0003INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                        WW_CheckMES2 = " 削除フラグにエラーがあります。 , "
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '○関連チェック(キー情報) 
            '統一車番存在チェック（統一車番が空白（新規）の場合、存在チェックしない）
            If MA0003INProw("TSHABAN") <> "" Then
                '大小比較チェック
                If MA0003INProw("STYMD") > MA0003INProw("ENDYMD") Then
                    WW_CheckMES1 = "・更新できないレコード(開始日付 ＞ 終了日付)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '権限チェック（更新権限）
            If MA0003INProw("MANGMORG") <> "" OrElse MA0003INProw("MANGSORG") <> "" Then

                '管理部署
                CS0025AUTHORget.USERID = CS0050Session.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MA0003INProw("MANGMORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    '設置部署
                    CS0025AUTHORget.USERID = CS0050Session.USERID
                    CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                    CS0025AUTHORget.CODE = MA0003INProw("MANGSORG")
                    CS0025AUTHORget.STYMD = Date.Now
                    CS0025AUTHORget.ENDYMD = Date.Now
                    CS0025AUTHORget.CS0025AUTHORget()
                    If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                    Else
                        WW_CheckMES1 = "・更新できないレコード(権限無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If


            '○単項目チェック(明細情報) 
            '●タブ別処理(01 管理)
            WW_ItemCheck("BASERDATE", "登録日新規", MA0003INProw("BASERDATE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("BASELEASE", "車両所有", MA0003INProw("BASELEASE"), MA0003INProw, WW_LINEERR_SW, O_RTN)

            '新規（統一車番が空白）の場合
            If MA0003INProw("TSHABAN") = "" Then
                '新規の場合、車両管理マスタ（MA0002_SHARYOA）を登録するため入力チェックを行う
                WW_ItemCheck("MANGOILTYPE", "油種", MA0003INProw("MANGOILTYPE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGMORG", "管理部署", MA0003INProw("MANGMORG"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGOWNCONT", "契約区分", MA0003INProw("MANGOWNCONT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGSUPPL", "庸車会社", MA0003INProw("MANGSUPPL"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGSHAFUKU", "車腹", MA0003INProw("MANGSHAFUKU"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGSORG", "設置部署", MA0003INProw("MANGSORG"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGOWNCODE", "荷主", MA0003INProw("MANGOWNCODE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("SHARYOSTATUS", "運行状況", MA0003INProw("SHARYOSTATUS"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("MANGTTLDIST", "累計走行キロ", MA0003INProw("MANGTTLDIST"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                Try
                    MA0003INProw("MANGTTLDIST") = Format(CInt(MA0003INProw("MANGTTLDIST")), "#0")
                Catch ex As Exception
                    MA0003INProw("MANGTTLDIST") = "0"
                End Try
            End If

            '●タブ別処理(03 車両緒元)
            If WW_SHARYOTYPE2 = "前" Then
                WW_ItemCheck("FCTRSMAKER", "車両メーカー", MA0003INProw("FCTRSMAKER"), MA0003INProw, WW_LINEERR_SW, O_RTN)

                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "FCTRFUELCAPA", MA0003INProw("FCTRFUELCAPA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード（燃料タンク容量Lエラー）です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                WW_ItemCheck("FCTRFUELMATE", "燃料タンク材質", MA0003INProw("FCTRFUELMATE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRTMISSION", "ミッション", MA0003INProw("FCTRTMISSION"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRUREA", "尿素", MA0003INProw("FCTRUREA"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRTIRE", "タイヤ", MA0003INProw("FCTRTIRE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRDPR", "DPR", MA0003INProw("FCTRDPR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRAXLE", "リフトアクスル", MA0003INProw("FCTRAXLE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRSUSP", "サスペンション種類", MA0003INProw("FCTRSUSP"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("FCTRSHFTNUM", "軸数", MA0003INProw("FCTRSHFTNUM"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            Else
                MA0003INProw("FCTRSMAKER") = ""
                MA0003INProw("FCTRFUELCAPA") = ""
                MA0003INProw("FCTRFUELMATE") = ""
                MA0003INProw("FCTRTMISSION") = ""
                MA0003INProw("FCTRUREA") = ""
                MA0003INProw("FCTRTIRE") = ""
                MA0003INProw("FCTRDPR") = ""
                MA0003INProw("FCTRAXLE") = ""
                MA0003INProw("FCTRSUSP") = ""
                MA0003INProw("FCTRSHFTNUM") = ""
            End If

            '●タブ別処理(04 石油タンク)
            If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "01" Then
                '入力値チェック
                WW_ItemCheck("OTNKTMAKER", "タンクメーカー", MA0003INProw("OTNKTMAKER"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKCELLNO", "部屋数", MA0003INProw("OTNKCELLNO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKCELPART", "部屋割", MA0003INProw("OTNKCELPART"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKMATERIAL", "タンク材質", MA0003INProw("OTNKMATERIAL"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKLVALVE", "底弁形式", MA0003INProw("OTNKLVALVE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKPUMP", "ポンプ", MA0003INProw("OTNKPUMP"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKPIPE", "配管形態", MA0003INProw("OTNKPIPE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKBPIPE", "後配管", MA0003INProw("OTNKBPIPE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKHTECH", "ハイテク種別", MA0003INProw("OTNKHTECH"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKDCD", "ＤＣＤ装備", MA0003INProw("OTNKDCD"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKDETECTOR", "検水管", MA0003INProw("OTNKDETECTOR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKPIPESIZE", "配管サイズ", MA0003INProw("OTNKPIPESIZE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKEXHASIZE", "吐出口サイズ", MA0003INProw("OTNKEXHASIZE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKDISGORGE", "吐出口", MA0003INProw("OTNKDISGORGE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKVAPOR", "ベーパー", MA0003INProw("OTNKVAPOR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKCVALVE", "中間ﾊﾞﾙﾌﾞ有無", MA0003INProw("OTNKCVALVE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKTINSNO", "タンク検査番号", MA0003INProw("OTNKTINSNO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKINSYMD", "タンク検査年月日", MA0003INProw("OTNKINSYMD"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("OTNKINSSTAT", "タンク検査行政区分", MA0003INProw("OTNKINSSTAT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            Else
                MA0003INProw("OTNKTMAKER") = ""
                MA0003INProw("OTNKCELLNO") = ""
                MA0003INProw("OTNKCELPART") = ""
                MA0003INProw("OTNKMATERIAL") = ""
                MA0003INProw("OTNKLVALVE") = ""
                MA0003INProw("OTNKPUMP") = ""
                MA0003INProw("OTNKPIPE") = ""
                MA0003INProw("OTNKBPIPE") = ""
                MA0003INProw("OTNKHTECH") = ""
                MA0003INProw("OTNKDCD") = ""
                MA0003INProw("OTNKDETECTOR") = ""
                MA0003INProw("OTNKPIPESIZE") = ""
                MA0003INProw("OTNKEXHASIZE") = ""
                MA0003INProw("OTNKDISGORGE") = ""
                MA0003INProw("OTNKVAPOR") = ""
                MA0003INProw("OTNKCVALVE") = ""
                MA0003INProw("OTNKTINSNO") = ""
                MA0003INProw("OTNKINSYMD") = ""
                MA0003INProw("OTNKINSSTAT") = ""
            End If

            '○タブ別処理(05 高圧タンク)
            If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "02" Then
                '入力値チェック
                WW_ItemCheck("HPRSTMAKER", "タンクメーカー", MA0003INProw("HPRSTMAKER"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSMATR", "タンク材質", MA0003INProw("HPRSMATR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSSTRUCT", "タンク構造", MA0003INProw("HPRSSTRUCT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSVALVE", "底弁形式", MA0003INProw("HPRSVALVE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSPIPE", "配管形状（仮）", MA0003INProw("HPRSPIPE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSPUMP", "ポンプ", MA0003INProw("HPRSPUMP"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSPMPDR", "ポンプ駆動方法", MA0003INProw("HPRSPMPDR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSRESSRE", "加圧器", MA0003INProw("HPRSRESSRE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSPIPENUM", "配管口数", MA0003INProw("HPRSPIPENUM"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSSERNO", "タンク容器番号", MA0003INProw("HPRSSERNO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSINSULATE", "断熱構造", MA0003INProw("HPRSINSULATE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSINSIYMD", "容器検査初回年月日", MA0003INProw("HPRSINSIYMD"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSINSISTAT", "容器検査初回実施場所", MA0003INProw("HPRSINSISTAT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("HPRSHOSE", "ホースボックス", MA0003INProw("HPRSHOSE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            Else
                '初期化
                MA0003INProw("HPRSTMAKER") = ""
                MA0003INProw("HPRSMATR") = ""
                MA0003INProw("HPRSSTRUCT") = ""
                MA0003INProw("HPRSVALVE") = ""
                MA0003INProw("HPRSPIPE") = ""
                MA0003INProw("HPRSPUMP") = ""
                MA0003INProw("HPRSPMPDR") = ""
                MA0003INProw("HPRSRESSRE") = ""
                MA0003INProw("HPRSPIPENUM") = ""
                MA0003INProw("HPRSSERNO") = ""
                MA0003INProw("HPRSINSULATE") = ""
                MA0003INProw("HPRSINSIYMD") = ""
                MA0003INProw("HPRSINSISTAT") = ""
                MA0003INProw("HPRSHOSE") = ""
            End If

            '○タブ別処理(06 化成品タンク)
            If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "03" Then
                '入力値チェック
                WW_ItemCheck("CHEMTMAKER", "タンクメーカー", MA0003INProw("CHEMTMAKER"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMCELLNO", "部屋数", MA0003INProw("CHEMCELLNO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMCELPART", "部屋割", MA0003INProw("CHEMCELPART"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMMATERIAL", "タンク材質", MA0003INProw("CHEMMATERIAL"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMSTRUCT", "タンク構造", MA0003INProw("CHEMSTRUCT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMPUMP", "ポンプ", MA0003INProw("CHEMPUMP"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMPMPDR", "ポンプ駆動方法", MA0003INProw("CHEMPMPDR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMDISGORGE", "吐出口", MA0003INProw("CHEMDISGORGE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMPRESDRV", "加温装置", MA0003INProw("CHEMPRESDRV"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMTHERM", "温度計", MA0003INProw("CHEMTHERM"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMMANOMTR", "圧力計", MA0003INProw("CHEMMANOMTR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMHOSE", "ホースボックス", MA0003INProw("CHEMHOSE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMPRESEQ", "均圧配管", MA0003INProw("CHEMPRESEQ"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMTINSNO", "タンク検査番号", MA0003INProw("CHEMTINSNO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMINSYMD", "タンク検査年月日", MA0003INProw("CHEMINSYMD"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CHEMINSSTAT", "タンク検査行政区分", MA0003INProw("CHEMINSSTAT"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            Else
                '初期化
                MA0003INProw("CHEMTMAKER") = ""
                MA0003INProw("CHEMCELLNO") = ""
                MA0003INProw("CHEMCELPART") = ""
                MA0003INProw("CHEMMATERIAL") = ""
                MA0003INProw("CHEMSTRUCT") = ""
                MA0003INProw("CHEMPUMP") = ""
                MA0003INProw("CHEMPMPDR") = ""
                MA0003INProw("CHEMDISGORGE") = ""
                MA0003INProw("CHEMPRESDRV") = ""
                MA0003INProw("CHEMTHERM") = ""
                MA0003INProw("CHEMMANOMTR") = ""
                MA0003INProw("CHEMHOSE") = ""
                MA0003INProw("CHEMPRESEQ") = ""
                MA0003INProw("CHEMTINSNO") = ""
                MA0003INProw("CHEMINSYMD") = ""
                MA0003INProw("CHEMINSSTAT") = ""
            End If

            '○タブ別処理(07 コンテナ)
            If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "04" Then
                '入力値チェック
                WW_ItemCheck("CONTTMAKER", "タンクメーカー", MA0003INProw("CONTTMAKER"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CONTSHAPE", "シャーシ形状", MA0003INProw("CONTSHAPE"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CONTPUMP", "ポンプ", MA0003INProw("CONTPUMP"), MA0003INProw, WW_LINEERR_SW, O_RTN)
                WW_ItemCheck("CONTPMPDR", "ポンプ駆動方法", MA0003INProw("CONTPMPDR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            Else
                '初期化
                MA0003INProw("CONTTMAKER") = ""
                MA0003INProw("CONTSHAPE") = ""
                MA0003INProw("CONTPUMP") = ""
                MA0003INProw("CONTPMPDR") = ""
            End If

            '○タブ別処理(08 車両その他)
            '入力値チェック
            WW_ItemCheck("OTHRTERMINAL", "車載端末", MA0003INProw("OTHRTERMINAL"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRBMONITOR", "バックモニター", MA0003INProw("OTHRBMONITOR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRRADIOCON", "無線（有・無）", MA0003INProw("OTHRRADIOCON"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRDOCO", "ＤoCoですCar番号", MA0003INProw("OTHRDOCO"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRPAINTING", "塗装", MA0003INProw("OTHRPAINTING"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRDRRECORD", "ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ", MA0003INProw("OTHRDRRECORD"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRBSONAR", "バックソナー", MA0003INProw("OTHRBSONAR"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRRTARGET", "一括修理非対象車", MA0003INProw("OTHRRTARGET"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRTIRE1", "タイヤ情報１", MA0003INProw("OTHRTIRE1"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRTIRE2", "タイヤ情報２", MA0003INProw("OTHRTIRE2"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OTHRTPMS", "TPMS", MA0003INProw("OTHRTPMS"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OFFCRESERVE1", "支店予備項目1", MA0003INProw("OFFCRESERVE1"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OFFCRESERVE2", "支店予備項目2", MA0003INProw("OFFCRESERVE2"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OFFCRESERVE3", "支店予備項目3", MA0003INProw("OFFCRESERVE3"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OFFCRESERVE4", "支店予備項目4", MA0003INProw("OFFCRESERVE4"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("OFFCRESERVE5", "支店予備項目5", MA0003INProw("OFFCRESERVE5"), MA0003INProw, WW_LINEERR_SW, O_RTN)

            '○タブ別処理(09 経理)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ACCTRCYCLE", MA0003INProw("ACCTRCYCLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード（リサイクル料金エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            WW_ItemCheck("ACCTLEASE1", "リース契約NO1", MA0003INProw("ACCTLEASE1"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLEASE2", "リース契約NO2", MA0003INProw("ACCTLEASE2"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLEASE3", "リース契約NO3", MA0003INProw("ACCTLEASE3"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLEASE4", "リース契約NO4", MA0003INProw("ACCTLEASE4"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLEASE5", "リース契約NO5", MA0003INProw("ACCTLEASE5"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLSUPL1", "リース会社NO1", MA0003INProw("ACCTLSUPL1"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLSUPL2", "リース会社NO2", MA0003INProw("ACCTLSUPL2"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLSUPL3", "リース会社NO3", MA0003INProw("ACCTLSUPL3"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLSUPL4", "リース会社NO4", MA0003INProw("ACCTLSUPL4"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTLSUPL5", "リース会社NO5", MA0003INProw("ACCTLSUPL5"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST01", "固定資産NO1", MA0003INProw("ACCTASST01"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST02", "固定資産NO2", MA0003INProw("ACCTASST02"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST03", "固定資産NO3", MA0003INProw("ACCTASST03"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST04", "固定資産NO4", MA0003INProw("ACCTASST04"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST05", "固定資産NO5", MA0003INProw("ACCTASST05"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST06", "固定資産NO6", MA0003INProw("ACCTASST06"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST07", "固定資産NO7", MA0003INProw("ACCTASST07"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST08", "固定資産NO8", MA0003INProw("ACCTASST08"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST09", "固定資産NO9", MA0003INProw("ACCTASST09"), MA0003INProw, WW_LINEERR_SW, O_RTN)
            WW_ItemCheck("ACCTASST10", "固定資産NO10", MA0003INProw("ACCTASST10"), MA0003INProw, WW_LINEERR_SW, O_RTN)

            '○関連チェック(明細情報) 
            If WW_LINEERR_SW = "" Then
                '新規車両の場合のチェック
                If MA0003INProw("TSHABAN") = "" Then
                    If MA0003INProw("MANGMORG") = "" Then
                        WW_CheckMES1 = "・エラーが存在します。（管理部署未入力）"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If

            If WW_LINEERR_SW = "" Then
                If MA0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        '○初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○日付重複チェック
        For Each MA0003INProw As DataRow In MA0003tbl.Rows

            '読み飛ばし
            If (MA0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MA0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MA0003INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MA0003INProw("STYMD") = "" Then
                Continue For
            End If

            If MA0003INProw("TSHABAN") = "" Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            '期間重複チェック
            For Each MA0003row As DataRow In MA0003tbl.Rows

                If MA0003row("SHARYOTYPE") = MA0003INProw("SHARYOTYPE") AndAlso
                    MA0003row("TSHABAN") = MA0003INProw("TSHABAN") AndAlso
                    MA0003row("DELFLG") <> C_DELETE_FLG.DELETE AndAlso
                    MA0003INProw("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                If MA0003row("STYMD") = MA0003INProw("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MA0003INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(MA0003INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MA0003row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MA0003row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next

            If WW_LINEERR_SW = "" Then
                MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 日付連続性チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DATE_RELATION_CHK(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_KEY As String = ""
        Dim WW_BrKEY As String = ""
        Dim WW_TARGET_KEY As List(Of String)
        WW_TARGET_KEY = New List(Of String)

        'チェック準備
        Dim MA0003View As New DataView(MA0003tbl)
        Dim MA0003ViewRow As DataRow = MA0003tbl.NewRow
        MA0003View.Sort = "CAMPCODE, SHARYOTYPE, TSHABAN, STYMD"
        MA0003View.RowFilter = "DELFLG <> '1'"

        'チェック対象KEY抽出(CAMPCODE + SHARYOTYPE + TSHABAN)
        For i As Integer = 0 To MA0003View.Count - 1
            MA0003ViewRow = MA0003View.Item(i).Row

            If InStr(MA0003ViewRow("OPERATION"), C_LIST_OPERATION_CODE.UPDATING) > 0 AndAlso MA0003ViewRow("TSHABAN") <> "" Then

                WW_KEY = MA0003ViewRow("CAMPCODE") & "_" & MA0003ViewRow("SHARYOTYPE") & MA0003ViewRow("TSHABAN")
                WW_TARGET_KEY.Add(WW_KEY)

            End If

        Next

        'チェック対象が無い場合、Exit
        If WW_TARGET_KEY.Count = 0 Then
            Exit Sub
        End If

        '日付連続性チェック
        Dim WW_UMU As String = "OFF"
        Dim WW_STYMD As Date = Date.Now                         '今回レコード・開始年月日
        Dim WW_ENDYMD As Date = Date.Now                        '今回レコード・終了年月日
        Dim WW_BrYMD As Date = Date.Now                         '前回レコード・終了年月日

        For i As Integer = 0 To MA0003View.Count - 1
            MA0003ViewRow = MA0003View.Item(i).Row

            If MA0003ViewRow("STYMD") = Nothing OrElse MA0003ViewRow("ENDYMD") = Nothing Then
                Continue For
            End If

            '○チェック対象レコード内容
            Try
                WW_KEY = MA0003ViewRow("CAMPCODE") & "_" & MA0003ViewRow("SHARYOTYPE") & MA0003ViewRow("TSHABAN")
                Date.TryParse(MA0003ViewRow("STYMD"), WW_STYMD)
                Date.TryParse(MA0003ViewRow("ENDYMD"), WW_ENDYMD)
            Catch ex As Exception
            End Try

            '○KEYブレイク
            If WW_KEY <> WW_BrKEY Then

                'チェック有無判定
                If WW_TARGET_KEY.IndexOf(WW_KEY) >= 0 Then
                    WW_UMU = "ON"
                Else
                    WW_UMU = "OFF"
                End If

                'ブレイク処理（チェックを通す）
                WW_BrKEY = WW_KEY
                WW_BrYMD = WW_STYMD.AddDays(-1)

            End If

            '歯抜けチェック
            If WW_UMU = "ON" AndAlso WW_STYMD <> WW_BrYMD.AddDays(1) Then
                Dim WW_ERR_MES As String = "・更新できないレコードです。(開始、終了年月日が連続していません)"
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & MA0003ViewRow("CAMPCODE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & MA0003ViewRow("SHARYOTYPE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & MA0003ViewRow("TSHABAN") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & MA0003ViewRow("STYMD") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & MA0003ViewRow("ENDYMD") & " , "
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

                For Each MA0003row As DataRow In MA0003tbl.Rows
                    If InStr(MA0003row("OPERATION"), C_LIST_OPERATION_CODE.UPDATING) > 0 Then
                        If MA0003row("CAMPCODE") = MA0003ViewRow("CAMPCODE") AndAlso
                            MA0003row("SHARYOTYPE") = MA0003ViewRow("SHARYOTYPE") AndAlso
                            MA0003row("TSHABAN") = MA0003ViewRow("TSHABAN") Then

                            MA0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED

                        End If
                    End If
                Next

                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR

            End If

            WW_BrYMD = WW_ENDYMD

        Next

    End Sub

    ''' <summary>
    ''' MA003に対するMA002登録チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MA02SYARYOA_Chk()

        Dim WW_SHARYOTYPE As String = ""
        Dim WW_TSHABAN As String = ""
        Dim WW_STYMD_B As String = ""
        Dim WW_ERTN As Boolean = False
        '○MA002チェック用データ取得 

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " SELECT " _
                    & "       isnull(rtrim(MA3.SHARYOTYPE),'')       as SHARYOTYPE              , " _
                    & "       isnull(rtrim(MA3.TSHABAN),'')          as TSHABAN                 , " _
                    & "       isnull(rtrim(MA3.ENDYMD),'')           as MA3_ENDYMD              , " _
                    & "       isnull(rtrim(MA2.ENDYMD),'1950/01/01') as MA2_ENDYMD                " _
                    & " FROM      MA003_SHARYOB MA3                                               " _
                    & " LEFT JOIN MA002_SHARYOA MA2                                               " _
                    & "   ON    MA2.CAMPCODE                          = MA3.CAMPCODE              " _
                    & "   and   MA2.SHARYOTYPE                        = MA3.SHARYOTYPE            " _
                    & "   and   MA2.TSHABAN                           = MA3.TSHABAN               " _
                    & "   and   MA2.STYMD                            <= MA3.ENDYMD                " _
                    & "   and   MA2.ENDYMD                           >= MA3.ENDYMD                " _
                    & "   and   MA2.DELFLG                           <> '1'                       " _
                    & " WHERE   MA3.CAMPCODE                          = @P01                      " _
                    & "   and   MA3.DELFLG                           <> '1'                       " _
                    & " ORDER BY MA3.SHARYOTYPE ASC, MA3.TSHABAN ASC, MA3.ENDYMD ASC, MA2.ENDYMD ASC "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text

                    'テーブル検索結果をテーブル退避
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            '○チェック ＆　メッセージ表示
                            If SQLdr("MA2_ENDYMD") = C_DEFAULT_YMD Then
                                rightview.addErrorReport(ControlChars.NewLine & "・基本情報終了日付の管理情報が未登録です (" & SQLdr("SHARYOTYPE") & SQLdr("TSHABAN") & ")")
                                WW_ERTN = True
                            End If
                        End While
                    End Using
                End Using

                If WW_ERTN Then
                    Master.output(C_MESSAGE_NO.WORNING_RECORD_EXIST, C_MESSAGE_TYPE.INF)
                End If
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA003_SHARYOB SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA003_SHARYOB Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' MA0003tbl更新
    ''' </summary>
    ''' <param name="I_EXCEL"></param>
    ''' <param name="RTN"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPD(ByRef I_EXCEL As String, ByRef RTN As String)

        '画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0003row As DataRow In MA0003tbl.Rows
            Select Case MA0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
            If MA0003row("BASERAGEYY") = "" Then
                MA0003row("BASERAGEYY") = "0"
            End If
            If MA0003row("BASERAGEMM") = "" Then
                MA0003row("BASERAGEMM") = "0"
            End If
            If MA0003row("BASERAGE") = "" Then
                MA0003row("BASERAGE") = "0"
            End If
            If MA0003row("ACCTRCYCLE") = "" Then
                MA0003row("ACCTRCYCLE") = "0"
            End If
            If MA0003row("FCTRFUELCAPA") = "" Then
                MA0003row("FCTRFUELCAPA") = "0"
            End If
        Next

        '○追加変更判定
        Dim MA0003INProw As DataRow = MA0003INPtbl.NewRow
        For i As Integer = 0 To MA0003INPtbl.Rows.Count - 1

            MA0003INProw.ItemArray = MA0003INPtbl.Rows(i).ItemArray

            If MA0003INProw("BASERAGEYY") = "" Then
                MA0003INProw("BASERAGEYY") = "0"
            End If
            If MA0003INProw("BASERAGEMM") = "" Then
                MA0003INProw("BASERAGEMM") = "0"
            End If
            If MA0003INProw("BASERAGE") = "" Then
                MA0003INProw("BASERAGE") = "0"
            End If
            If MA0003INProw("ACCTRCYCLE") = "" Then
                MA0003INProw("ACCTRCYCLE") = "0"
            End If
            If MA0003INProw("FCTRFUELCAPA") = "" Then
                MA0003INProw("FCTRFUELCAPA") = "0"
            End If

            If MA0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                MA0003INProw("OPERATION") = "Insert"
                For Each MA0003row As DataRow In MA0003tbl.Rows
                    'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                    If MA0003row("CAMPCODE") = MA0003INProw("CAMPCODE") AndAlso
                       MA0003row("SHARYOTYPE") = MA0003INProw("SHARYOTYPE") AndAlso
                       MA0003row("TSHABAN") = MA0003INProw("TSHABAN") AndAlso
                       MA0003row("STYMD") = MA0003INProw("STYMD") Then

                        MA0003INProw("OPERATION") = "Update"
                        Exit For

                    End If
                Next
            End If

            MA0003INPtbl.Rows(i).ItemArray = MA0003INProw.ItemArray

        Next

        '○変更有無判定　&　MA0003tblへ入力値反映
        MA0003INProw = MA0003INPtbl.NewRow
        For i As Integer = 0 To MA0003INPtbl.Rows.Count - 1
            MA0003INProw.ItemArray = MA0003INPtbl.Rows(i).ItemArray

            Select Case MA0003INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MA0003INProw)
                Case "Insert"
                    TBL_INSERT_SUB(MA0003INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
            End Select

            MA0003INPtbl.Rows(i).ItemArray = MA0003INProw.ItemArray

        Next

    End Sub
    ''' <summary>
    ''' テーブル内容更新処理
    ''' </summary>
    ''' <param name="MA0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MA0003INProw As DataRow)

        '○判定準備
        '車両タイプ分類（値２を取得）
        Dim WW_SHARYOTYPE2 As String = ""
        GetSharyoType2(MA0003INProw("SHARYOTYPE"), WW_SHARYOTYPE2, WW_DUMMY)

        '○変更有無判定
        For Each MA0003row As DataRow In MA0003tbl.Rows

            If MA0003row("CAMPCODE") = MA0003INProw("CAMPCODE") AndAlso
               MA0003row("SHARYOTYPE") = MA0003INProw("SHARYOTYPE") AndAlso
               MA0003row("TSHABAN") = MA0003INProw("TSHABAN") AndAlso
               MA0003row("STYMD") = MA0003INProw("STYMD") Then
            Else
                Continue For
            End If

            If MA0003row("ENDYMD") = MA0003INProw("ENDYMD") AndAlso
                MA0003row("BASERDATE") = MA0003INProw("BASERDATE") AndAlso
                CInt(MA0003row("BASERAGEYY")) = CInt(MA0003INProw("BASERAGEYY")) AndAlso
                CInt(MA0003row("BASERAGEMM")) = CInt(MA0003INProw("BASERAGEMM")) AndAlso
                CInt(MA0003row("BASERAGE")) = CInt(MA0003INProw("BASERAGE")) AndAlso
                CInt(MA0003row("FCTRFUELCAPA")) = CInt(MA0003INProw("FCTRFUELCAPA")) AndAlso
                Val(MA0003row("ACCTRCYCLE").ToString.Replace(",", "")) = Val(MA0003INProw("ACCTRCYCLE").ToString.Replace(",", "")) AndAlso
                MA0003row("BASELEASE") = MA0003INProw("BASELEASE") AndAlso MA0003row("FCTRFUELMATE") = MA0003INProw("FCTRFUELMATE") AndAlso
                MA0003row("FCTRTMISSION") = MA0003INProw("FCTRTMISSION") AndAlso MA0003row("FCTRUREA") = MA0003INProw("FCTRUREA") AndAlso
                MA0003row("FCTRTIRE") = MA0003INProw("FCTRTIRE") AndAlso MA0003row("FCTRDPR") = MA0003INProw("FCTRDPR") AndAlso
                MA0003row("FCTRAXLE") = MA0003INProw("FCTRAXLE") AndAlso MA0003row("FCTRSUSP") = MA0003INProw("FCTRSUSP") AndAlso
                MA0003row("FCTRSHFTNUM") = MA0003INProw("FCTRSHFTNUM") AndAlso MA0003row("FCTRSMAKER") = MA0003INProw("FCTRSMAKER") AndAlso
                MA0003row("FCTRTMAKER") = MA0003INProw("FCTRTMAKER") AndAlso MA0003row("FCTRRESERVE1") = MA0003INProw("FCTRRESERVE1") AndAlso
                MA0003row("FCTRRESERVE2") = MA0003INProw("FCTRRESERVE2") AndAlso MA0003row("FCTRRESERVE3") = MA0003INProw("FCTRRESERVE3") AndAlso
                MA0003row("FCTRRESERVE4") = MA0003INProw("FCTRRESERVE4") AndAlso MA0003row("FCTRRESERVE5") = MA0003INProw("FCTRRESERVE5") AndAlso
                MA0003row("ACCTLEASE1") = MA0003INProw("ACCTLEASE1") AndAlso MA0003row("ACCTLEASE2") = MA0003INProw("ACCTLEASE2") AndAlso
                MA0003row("ACCTLEASE3") = MA0003INProw("ACCTLEASE3") AndAlso MA0003row("ACCTLEASE4") = MA0003INProw("ACCTLEASE4") AndAlso
                MA0003row("ACCTLEASE5") = MA0003INProw("ACCTLEASE5") AndAlso MA0003row("ACCTLSUPL1") = MA0003INProw("ACCTLSUPL1") AndAlso
                MA0003row("ACCTLSUPL2") = MA0003INProw("ACCTLSUPL2") AndAlso MA0003row("ACCTLSUPL3") = MA0003INProw("ACCTLSUPL3") AndAlso
                MA0003row("ACCTLSUPL4") = MA0003INProw("ACCTLSUPL4") AndAlso MA0003row("ACCTLSUPL5") = MA0003INProw("ACCTLSUPL5") AndAlso
                MA0003row("ACCTASST01") = MA0003INProw("ACCTASST01") AndAlso MA0003row("ACCTASST02") = MA0003INProw("ACCTASST02") AndAlso
                MA0003row("ACCTASST03") = MA0003INProw("ACCTASST03") AndAlso MA0003row("ACCTASST04") = MA0003INProw("ACCTASST04") AndAlso
                MA0003row("ACCTASST05") = MA0003INProw("ACCTASST05") AndAlso MA0003row("ACCTASST06") = MA0003INProw("ACCTASST06") AndAlso
                MA0003row("ACCTASST07") = MA0003INProw("ACCTASST07") AndAlso MA0003row("ACCTASST08") = MA0003INProw("ACCTASST08") AndAlso
                MA0003row("ACCTASST09") = MA0003INProw("ACCTASST09") AndAlso MA0003row("ACCTASST10") = MA0003INProw("ACCTASST10") AndAlso
                MA0003row("OTHRTERMINAL") = MA0003INProw("OTHRTERMINAL") AndAlso MA0003row("OTHRBMONITOR") = MA0003INProw("OTHRBMONITOR") AndAlso
                MA0003row("OTHRRADIOCON") = MA0003INProw("OTHRRADIOCON") AndAlso MA0003row("OTHRDOCO") = MA0003INProw("OTHRDOCO") AndAlso
                MA0003row("OTHRPAINTING") = MA0003INProw("OTHRPAINTING") AndAlso MA0003row("OTHRDRRECORD") = MA0003INProw("OTHRDRRECORD") AndAlso
                MA0003row("OTHRBSONAR") = MA0003INProw("OTHRBSONAR") AndAlso MA0003row("OTHRRTARGET") = MA0003INProw("OTHRRTARGET") AndAlso
                MA0003row("OTHRTIRE1") = MA0003INProw("OTHRTIRE1") AndAlso MA0003row("OTHRTIRE2") = MA0003INProw("OTHRTIRE2") AndAlso
                MA0003row("OTHRTPMS") = MA0003INProw("OTHRTPMS") AndAlso
                MA0003row("OFFCRESERVE1") = MA0003INProw("OFFCRESERVE1") AndAlso MA0003row("OFFCRESERVE2") = MA0003INProw("OFFCRESERVE2") AndAlso
                MA0003row("OFFCRESERVE3") = MA0003INProw("OFFCRESERVE3") AndAlso MA0003row("OFFCRESERVE4") = MA0003INProw("OFFCRESERVE4") AndAlso
                MA0003row("OFFCRESERVE5") = MA0003INProw("OFFCRESERVE5") AndAlso MA0003row("DELFLG") = MA0003INProw("DELFLG") Then

                If WW_SHARYOTYPE2 <> "後" Then
                    MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If

                If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "01" Then
                    If MA0003row("OTNKCELLNO") = MA0003INProw("OTNKCELLNO") AndAlso
                       MA0003row("OTNKCELPART") = MA0003INProw("OTNKCELPART") AndAlso
                       MA0003row("OTNKMATERIAL") = MA0003INProw("OTNKMATERIAL") AndAlso
                       MA0003row("OTNKLVALVE") = MA0003INProw("OTNKLVALVE") AndAlso
                       MA0003row("OTNKPUMP") = MA0003INProw("OTNKPUMP") AndAlso
                       MA0003row("OTNKPIPE") = MA0003INProw("OTNKPIPE") AndAlso
                       MA0003row("OTNKBPIPE") = MA0003INProw("OTNKBPIPE") AndAlso
                       MA0003row("OTNKHTECH") = MA0003INProw("OTNKHTECH") AndAlso
                       MA0003row("OTNKDCD") = MA0003INProw("OTNKDCD") AndAlso
                       MA0003row("OTNKDETECTOR") = MA0003INProw("OTNKDETECTOR") AndAlso
                       MA0003row("OTNKPIPESIZE") = MA0003INProw("OTNKPIPESIZE") AndAlso
                       MA0003row("OTNKEXHASIZE") = MA0003INProw("OTNKEXHASIZE") AndAlso
                       MA0003row("OTNKDISGORGE") = MA0003INProw("OTNKDISGORGE") AndAlso
                       MA0003row("OTNKVAPOR") = MA0003INProw("OTNKVAPOR") AndAlso
                       MA0003row("OTNKCVALVE") = MA0003INProw("OTNKCVALVE") AndAlso
                       MA0003row("OTNKTINSNO") = MA0003INProw("OTNKTINSNO") AndAlso
                       MA0003row("OTNKINSYMD") = MA0003INProw("OTNKINSYMD") AndAlso
                       MA0003row("OTNKINSSTAT") = MA0003INProw("OTNKINSSTAT") AndAlso
                       MA0003row("OTNKTMAKER") = MA0003INProw("OTNKTMAKER") Then
                        MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                End If

                If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "02" Then
                    If MA0003row("HPRSMATR") = MA0003INProw("HPRSMATR") AndAlso
                       MA0003row("HPRSSTRUCT") = MA0003INProw("HPRSSTRUCT") AndAlso
                       MA0003row("HPRSVALVE") = MA0003INProw("HPRSVALVE") AndAlso
                       MA0003row("HPRSPIPE") = MA0003INProw("HPRSPIPE") AndAlso
                       MA0003row("HPRSPUMP") = MA0003INProw("HPRSPUMP") AndAlso
                       MA0003row("HPRSRESSRE") = MA0003INProw("HPRSRESSRE") AndAlso
                       MA0003row("HPRSPIPENUM") = MA0003INProw("HPRSPIPENUM") AndAlso
                       MA0003row("HPRSINSULATE") = MA0003INProw("HPRSINSULATE") AndAlso
                       MA0003row("HPRSSERNO") = MA0003INProw("HPRSSERNO") AndAlso
                       MA0003row("HPRSHOSE") = MA0003INProw("HPRSHOSE") AndAlso
                       MA0003row("HPRSINSIYMD") = MA0003INProw("HPRSINSIYMD") AndAlso
                       MA0003row("HPRSINSISTAT") = MA0003INProw("HPRSINSISTAT") AndAlso
                       MA0003row("HPRSTMAKER") = MA0003INProw("HPRSTMAKER") Then
                        MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                End If

                If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "03" Then
                    If MA0003row("CHEMCELLNO") = MA0003INProw("CHEMCELLNO") AndAlso
                       MA0003row("CHEMCELPART") = MA0003INProw("CHEMCELPART") AndAlso
                       MA0003row("CHEMMATERIAL") = MA0003INProw("CHEMMATERIAL") AndAlso
                       MA0003row("CHEMSTRUCT") = MA0003INProw("CHEMSTRUCT") AndAlso
                       MA0003row("CHEMPUMP") = MA0003INProw("CHEMPUMP") AndAlso
                       MA0003row("CHEMPMPDR") = MA0003INProw("CHEMPMPDR") AndAlso
                       MA0003row("CHEMDISGORGE") = MA0003INProw("CHEMDISGORGE") AndAlso
                       MA0003row("CHEMPRESDRV") = MA0003INProw("CHEMPRESDRV") AndAlso
                       MA0003row("CHEMTHERM") = MA0003INProw("CHEMTHERM") AndAlso
                       MA0003row("CHEMMANOMTR") = MA0003INProw("CHEMMANOMTR") AndAlso
                       MA0003row("CHEMHOSE") = MA0003INProw("CHEMHOSE") AndAlso
                       MA0003row("CHEMPRESEQ") = MA0003INProw("CHEMPRESEQ") AndAlso
                       MA0003row("CHEMTINSNO") = MA0003INProw("CHEMTINSNO") AndAlso
                       MA0003row("CHEMINSYMD") = MA0003INProw("CHEMINSYMD") AndAlso
                       MA0003row("CHEMINSSTAT") = MA0003INProw("CHEMINSSTAT") AndAlso
                       MA0003row("CHEMTMAKER") = MA0003INProw("CHEMTMAKER") Then
                        MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                End If

                If WW_SHARYOTYPE2 = "後" AndAlso MA0003INProw("MANGOILTYPE") = "04" Then
                    If MA0003row("CONTSHAPE") = MA0003INProw("CONTSHAPE") AndAlso
                       MA0003row("CONTPUMP") = MA0003INProw("CONTPUMP") AndAlso
                       MA0003row("CONTPMPDR") = MA0003INProw("CONTPMPDR") AndAlso
                       MA0003row("CONTTMAKER") = MA0003INProw("CONTTMAKER") Then
                        MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    End If
                End If

            End If

            '○テーブル更新
            If MA0003INProw("OPERATION") = "Update" Then

                MA0003INProw("LINECNT") = MA0003row("LINECNT")
                MA0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MA0003INProw("TIMSTP") = MA0003row("TIMSTP")
                MA0003INProw("SELECT") = 1
                MA0003INProw("HIDDEN") = 0
                MA0003INProw("WORK_NO") = 0

                MA0003row.ItemArray = MA0003INProw.ItemArray
                MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                If MA0003row("BASERDATE") <> "" Then

                    Dim WW_DATENOW As Date = Date.Now
                    Dim WW_BASERAGEYY As Integer
                    Dim WW_BASERAGE As Integer
                    Dim WW_BASERAGEMM As Integer
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(MA0003row("BASERDATE"), WW_DATE)
                        WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                        WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                        WW_BASERAGEMM = WW_BASERAGE Mod 12
                        MA0003row("BASERAGEMM") = WW_BASERAGEMM
                        MA0003row("BASERAGEYY") = WW_BASERAGEYY
                        MA0003row("BASERAGE") = WW_BASERAGE
                    Catch ex As Exception
                        MA0003row("BASERAGEMM") = 0
                        MA0003row("BASERAGEYY") = 0
                        MA0003row("BASERAGE") = 0
                    End Try
                End If

                MA0003row("NOTES") = ""
                MA0003row("WORK_NO") = 0
                MA0003row("STYMD_S") = ""
                MA0003row("ENDYMD_S") = ""
                MA0003row("STYMD_A") = ""
                MA0003row("ENDYMD_A") = ""
                MA0003row("STYMD_B") = ""
                MA0003row("ENDYMD_B") = ""
                MA0003row("STYMD_C") = ""
                MA0003row("ENDYMD_C") = ""

            End If

            Exit For

        Next

    End Sub
    ''' <summary>
    ''' テーブル登録処理
    ''' </summary>
    ''' <param name="MA0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef MA0003INProw As DataRow)

        Dim MA0003row As DataRow = MA0003tbl.NewRow

        '項目設定
        MA0003row.ItemArray = MA0003INProw.ItemArray
        MA0003row("LINECNT") = MA0003tbl.Rows.Count + 1
        If MA0003row("TSHABAN") = "" Then
            MA0003row("TSHABAN") = "新" & (MA0003tbl.Rows.Count + 1).ToString("00000")
        End If
        MA0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        If MA0003row("ACCTRCYCLE") = "" Then
            MA0003row("ACCTRCYCLE") = "0"
        End If
        If MA0003row("BASERDATE") = "" Then
            MA0003row("BASERAGEMM") = "0"
            MA0003row("BASERAGEYY") = "0"
            MA0003row("BASERAGE") = "0"
        Else
            Dim WW_DATE As Date
            WW_DATE = MA0003row("BASERDATE")
            MA0003row("BASERDATE") = WW_DATE.ToString("yyyy/MM/dd")

            Dim WW_DATENOW As Date = Date.Now
            Dim WW_BASERAGEYY As Integer
            Dim WW_BASERAGE As Integer
            Dim WW_BASERAGEMM As Integer
            WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
            WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
            WW_BASERAGEMM = WW_BASERAGE Mod 12
            MA0003row("BASERAGEMM") = WW_BASERAGEMM
            MA0003row("BASERAGEYY") = WW_BASERAGEYY
            MA0003row("BASERAGE") = WW_BASERAGE
        End If
        MA0003row("NOTES") = ""
        MA0003row("TIMSTP") = "0"
        MA0003row("SELECT") = 1
        MA0003row("HIDDEN") = 0
        MA0003row("WORK_NO") = 0
        MA0003row("STYMD_S") = ""
        MA0003row("ENDYMD_S") = ""
        MA0003row("STYMD_A") = ""
        MA0003row("ENDYMD_A") = ""
        MA0003row("STYMD_B") = ""
        MA0003row("ENDYMD_B") = ""
        MA0003row("STYMD_C") = ""
        MA0003row("ENDYMD_C") = ""

        MA0003tbl.Rows.Add(MA0003row)

    End Sub
    ''' <summary>
    ''' TAB11番目の表示項目
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MA0003PDFtbl_ColumnsAdd()

        If IsNothing(MA0003PDFtbl) Then
            MA0003PDFtbl = New DataTable()
        End If

        If MA0003PDFtbl.Columns.Count <> 0 Then
            MA0003PDFtbl.Columns.Clear()
        End If

        'MA0003PDFtblテンポラリDB項目作成
        MA0003PDFtbl.Clear()

        MA0003PDFtbl.Columns.Add("FILENAME", GetType(String))
        MA0003PDFtbl.Columns.Add("DELFLG", GetType(String))
        MA0003PDFtbl.Columns.Add("FILEPATH", GetType(String))

    End Sub

    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' 車両タイプのVALUE2を取得する
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub GetSharyoType2(ByVal I_SHARYOTYPE As String, ByRef O_SHARYOTYPE2 As String, ByRef O_RTN As String)

        Dim GS0007FIXVALUElst2 As New GS0007FIXVALUElst
        GS0007FIXVALUElst2.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0007FIXVALUElst2.CLAS = "SHARYOTYPE"
        GS0007FIXVALUElst2.LISTBOX2 = WF_ListBoxSHARYOTYPE2
        GS0007FIXVALUElst2.GS0007FIXVALUElst()
        O_RTN = GS0007FIXVALUElst2.ERR
        If isNormal(GS0007FIXVALUElst2.ERR) Then
            WF_ListBoxSHARYOTYPE2 = GS0007FIXVALUElst2.LISTBOX2
            For Each item As ListItem In WF_ListBoxSHARYOTYPE2.Items
                If item.Value = I_SHARYOTYPE Then
                    O_SHARYOTYPE2 = item.Text
                    Exit For
                End If
            Next
        Else
            Master.output(GS0007FIXVALUElst2.ERR, C_MESSAGE_TYPE.ABORT)
        End If

    End Sub
    ''' <summary>
    ''' PDFの種別一覧を取得する
    ''' </summary>
    ''' <param name="LISTBOX"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub GetPDFList(ByRef LISTBOX As ListBox, ByRef O_RTN As String)
        Dim GS0007FIXVALUElst As New GS0007FIXVALUElst
        GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0007FIXVALUElst.CLAS = "MA0001_PDF"
        GS0007FIXVALUElst.LISTBOX1 = LISTBOX
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        O_RTN = GS0007FIXVALUElst.ERR
        If isNormal(GS0007FIXVALUElst.ERR) Then
            LISTBOX = GS0007FIXVALUElst.LISTBOX1
        Else
            Master.output(GS0007FIXVALUElst.ERR, C_MESSAGE_TYPE.ABORT)
        End If
    End Sub
    ''' <summary>
    ''' 名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_SUB_VALUE"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, ByVal ParamArray I_SUB_VALUE As String())

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE = "" Then
        Else
            Select Case I_FIELD

                Case "MANGMORG"
                    '管理部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, True))
                Case "MANGSORG"
                    '設置部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, False))
                Case "MANGOILTYPE" '油種名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "MANGOWNCODE"
                    '荷主名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTODOParam(work.WF_SEL_CAMPCODE.Text))
                Case "MANGOWNCONT"
                    '契約区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MANGOWNCONT"))
                Case "MANGSUPPL"
                    '庸車会社名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateYOTORIParam(work.WF_SEL_CAMPCODE.Text))
                Case "MANGUORG"
                    '運用部署名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, False))
                Case "BASELEASE"
                    '車両所有名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "BASELEASE"))
                Case "FCTRAXLE"
                    'リフトアクスル名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRAXLE"))
                Case "FCTRTMAKER"
                    'デフ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRTMAKER"))
                Case "FCTRDPR"
                    'DPR名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRDPR"))
                Case "FCTRFUELMATE"
                    '燃料タンク材質名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRFUELMATE"))
                Case "FCTRSHFTNUM"
                    '軸数名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRSHFTNUM"))
                Case "FCTRSUSP"
                    'サスペンション種類名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRSUSP"))
                Case "FCTRTMISSION"
                    'ミッション名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRTMISSION"))
                Case "FCTRUREA"
                    '尿素名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRUREA"))
                Case "OTNKBPIPE"
                    '後配管名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKBPIPE"))
                Case "OTNKVAPOR"
                    'ベーパー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKVAPOR"))
                Case "OTNKCVALVE"
                    '中間ﾊﾞﾙﾌﾞ有無名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKCVALVE"))
                Case "OTNKDCD"
                    'DCD装備名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKDCD"))
                Case "FCTRSMAKER"
                    'DCD登録車名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRSMAKER"))
                Case "OTNKDETECTOR"
                    '検水管名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKDETECTOR"))
                Case "OTNKDISGORGE"
                    '吐出口名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKDISGORGE"))
                Case "OTNKHTECH"
                    'ハイテク種別名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKHTECH"))
                Case "OTNKLVALVE"
                    '底弁形式名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKLVALVE"))
                Case "OTNKMATERIAL"
                    'タンク材質名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKMATERIAL"))
                Case "OTNKPIPE"
                    '配管形態名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKPIPE"))
                Case "OTNKPIPESIZE"
                    '配管サイズ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKPIPESIZE"))
                Case "OTNKPUMP"
                    'ポンプ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKPUMP"))
                Case "HPRSPMPDR"
                    'ポンプ駆動方法
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSPMPDR"))
                Case "HPRSINSULATE"
                    '断熱構造名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSINSULATE"))
                Case "HPRSMATR"
                    'タンク材質名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSMATR"))
                Case "HPRSPIPE"
                    '配管形状（仮）名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSPIPE"))
                Case "HPRSPIPENUM"
                    '配管口数名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSPIPENUM"))
                Case "HPRSPUMP"
                    'ポンプ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSPUMP"))
                Case "HPRSRESSRE"
                    '加圧器名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSRESSRE"))
                Case "HPRSSTRUCT"
                    'タンク構造名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSSTRUCT"))
                Case "HPRSVALVE"
                    '底弁形式名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSVALVE"))
                Case "CHEMDISGORGE"
                    '吐出口名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMDISGORGE"))
                Case "CHEMHOSE"
                    'ホースボックス名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMHOSE"))
                Case "CHEMMANOMTR"
                    '圧力計名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMMANOMTR"))
                Case "CHEMMATERIAL"
                    'タンク材質名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMMATERIAL"))
                Case "CHEMPMPDR"
                    'ポンプ駆動方法名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMPMPDR"))
                Case "CHEMPRESDRV"
                    '加温装置名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMPRESDRV"))
                Case "CHEMPRESEQ"
                    '均圧配管名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMPRESEQ"))
                Case "CHEMPUMP"
                    'ポンプ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMPUMP"))
                Case "CHEMSTRUCT"
                    'タンク構造名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMSTRUCT"))
                Case "CHEMTHERM"
                    '温度計名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMTHERM"))
                Case "OTHRBMONITOR"
                    'バックモニター名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRBMONITOR"))
                Case "OTHRBSONAR"
                    'バックソナー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRBSONAR"))
                Case "FCTRTIRE"
                    'ＤoCoですCar番号名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "FCTRTIRE"))
                Case "OTHRDRRECORD"
                    'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRDRRECORD"))
                Case "OTHRPAINTING"
                    '塗装名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRPAINTING"))
                Case "OTHRRADIOCON"
                    '無線（有・無）名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRRADIOCON"))
                Case "OTHRRTARGET"
                    '一括修理非対象車名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRRTARGET"))
                Case "OTHRTERMINAL"
                    '車載端末名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRTERMINAL"))
                Case "CAMPCODE"
                    '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "SHARYOTYPE", "SHARYOTYPE2", "SHARYOTYPEF", "SHARYOTYPEB", "SHARYOTYPEB2", "SHARYOTYPEB3"
                    '車両タイプ,車両タイプ2,車両タイプ(前),車両タイプ(後),車両タイプ(後2),車両タイプ(後3)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE"))
                Case "LICNPLTNO1"
                    '登録番号(陸運局)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LICNPLTNO1"))
                Case "DELFLG"
                    '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                Case "OTNKEXHASIZE"
                    '吐出口サイズ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKEXHASIZE"))
                Case "HPRSHOSE"
                    'ホースボックス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSHOSE"))
                Case "CONTSHAPE"
                    'シャーシ形状
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONTSHAPE"))
                Case "CONTPUMP"
                    'ポンプ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONTPUMP"))
                Case "CONTPMPDR"
                    'ポンプ駆動方法
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONTPMPDR"))
                Case "OTHRTPMS"
                    'TPMS
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTHRTPMS"))
                Case "MANGPROD1"
                    '品名1
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, I_SUB_VALUE(0)))
                Case "MANGPROD2"
                    '品名2
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoodsParam(work.WF_SEL_CAMPCODE.Text, I_SUB_VALUE(0), I_SUB_VALUE(1)))
                Case "OTNKTMAKER"
                    '石油タンクメーカー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTNKTMAKER"))
                Case "HPRSTMAKER"
                    '高圧タンクメーカー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "HPRSTMAKER"))
                Case "CHEMTMAKER"
                    '化成品タンクメーカー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CHEMTMAKER"))
                Case "CONTTMAKER"
                    'コンテナタンクメーカー名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CONTTMAKER"))
                Case "SHARYOSTATUS"
                    '運行状況名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHARYOSTATUS"))
                Case "INSKBN"
                    '検査区分名
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "INSKBN"))
                Case Else
                    O_TEXT = ""
                    O_RTN = C_MESSAGE_NO.NORMAL
            End Select
        End If


    End Sub

    ''' <summary>
    ''' 項目チェックエラーレポート編集
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_ITEMNM"></param>
    ''' <param name="O_VALUE"></param>
    ''' <param name="MA0003INProw"></param>
    ''' 
    ''' <remarks></remarks>
    Protected Sub WW_ItemCheck(ByVal I_FIELD As String, ByVal I_ITEMNM As String, ByRef O_VALUE As String, ByVal MA0003INProw As DataRow, ByRef WW_LINEERR_SW As String, ByRef O_RTN As String)
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_FIELD As String = MA0003INProw(I_FIELD)

        Master.CheckField(work.WF_SEL_CAMPCODE.Text, I_FIELD, MA0003INProw(I_FIELD), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・エラーが存在します。（" & I_ITEMNM & "）"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
            WW_LINEERR_SW = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        If isNormal(WW_CS0024FCHECKERR) AndAlso WW_FIELD <> "" Then
            CODENAME_get(I_FIELD, MA0003INProw(I_FIELD), WW_DUMMY, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・エラーが存在します。（" & I_ITEMNM & "）"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0003INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

    End Sub
    ''' <summary>
    ''' エラーチェックのログ出力
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <param name="MA0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MA0003INProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & MA0003INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & MA0003INProw("SHARYOTYPE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & MA0003INProw("TSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & MA0003INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & MA0003INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　　　　=" & MA0003INProw("DELFLG") & " "
        rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

    End Sub

End Class

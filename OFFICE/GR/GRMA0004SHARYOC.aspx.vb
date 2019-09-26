Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRMA0004SHARYOC
    Inherits Page

    '検索結果格納ds
    Private MA0004tbl As DataTable                              'Grid格納用テーブル
    Private MA0004INPtbl As DataTable                           'Detail入力用テーブル
    Private MA0004_SHARYOCtbl As DataTable                      '更新用テーブル
    Private MA0004PDFtbl As DataTable                           'Repeater格納用テーブル

    '共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                'プロファイル（GridView）設定
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'UPLOAD_XLSデータ取得
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(APサーバチェックなし)
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
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
                    If Not Master.RecoverTable(MA0004tbl) Then
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
                        Case "WF_PDF_UPLOAD" '○ PDFファイルアップロード入力処理
                            UPLOAD_PDF()
                        Case "WF_DTAB_Click" '○DetailTab切替処理
                            WF_Detail_TABChange()
                            TAB_DisplayCTRL(WF_SHARYOTYPE.Text)
                        Case "WF_DTAB_PDF_Change" '○Detail PFD表示内容切替処理
                            PDF_SELECTchange()
                        Case "WF_DTAB_PDF_Click" '○Detail PFD内容表示処理
                            DTAB_PDFdisplay()
                        Case "WF_STYMD_Change" '○Detail開始年月日変更処理
                            PDF_STYMDchange()
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
            If Not IsNothing(MA0004tbl) Then
                MA0004tbl.Clear()
                MA0004tbl.Dispose()
                MA0004tbl = Nothing
            End If

            If Not IsNothing(MA0004INPtbl) Then
                MA0004INPtbl.Clear()
                MA0004INPtbl.Dispose()
                MA0004INPtbl = Nothing
            End If

            If Not IsNothing(MA0004PDFtbl) Then
                MA0004PDFtbl.Clear()
                MA0004PDFtbl.Dispose()
                MA0004PDFtbl = Nothing
            End If

            If Not IsNothing(MA0004_SHARYOCtbl) Then
                MA0004_SHARYOCtbl.Clear()
                MA0004_SHARYOCtbl.Dispose()
                MA0004_SHARYOCtbl = Nothing
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
        Master.SaveTable(MA0004tbl)

        '○画面編集２
        Using TBLview As DataView = New DataView(MA0004tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRMA0004WRKINC.MAPID
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
        '○Workディレクトリ削除
        PDF_INITdel()
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
        For Each MA0004row As DataRow In MA0004tbl.Rows
            If MA0004row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MA0004row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MA0004tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRMA0004WRKINC.MAPID
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
        Dim WW_RTN As String = ""

        '車両タイプ
        If WF_SELSHARYOTYPE.Text <> "" Then
            CODENAME_get("SHARYOTYPE", Left(WF_SELSHARYOTYPE.Text, 1), WF_SELSHARYOTYPE_TEXT.Text, WW_RTN_SW)
        End If

        '管理部署名
        If WF_SELMORG.Text <> "" Then
            CODENAME_get("MANGMORG", WF_SELMORG.Text, WF_SELMORG_TEXT.Text, WW_RTN_SW)
            If Not isnormal(WW_RTN_SW) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, WF_SELMORG.Text)
                Exit Sub
            End If
        End If

        '○絞り込み操作（GridView明細Hidden設定）
        For Each MA0004row As DataRow In MA0004tbl.Rows

            MA0004row("HIDDEN") = 1

            '車両タイプ・管理組織　絞込判定
            If WF_SELSHARYOTYPE.Text = "" AndAlso WF_SELMORG.Text = "" Then
                MA0004row("HIDDEN") = 0
            End If

            If WF_SELSHARYOTYPE.Text <> "" AndAlso WF_SELMORG.Text = "" Then
                If MA0004row("SHARYOTYPE") & MA0004row("TSHABAN") Like WF_SELSHARYOTYPE.Text & "*" Then
                    MA0004row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHARYOTYPE.Text = "" AndAlso WF_SELMORG.Text <> "" Then
                If MA0004row("MANGMORG") = WF_SELMORG.Text Then
                    MA0004row("HIDDEN") = 0
                End If
            End If

            If WF_SELSHARYOTYPE.Text <> "" AndAlso WF_SELMORG.Text <> "" Then
                If MA0004row("SHARYOTYPE") & MA0004row("TSHABAN") Like WF_SELSHARYOTYPE.Text & "*" AndAlso MA0004row("MANGMORG") = WF_SELMORG.Text Then
                    MA0004row("HIDDEN") = 0
                End If
            End If
        Next

        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)

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

        '更新SQL文･･･マスタへ更新
        Dim WW_DATENOW As DateTime = Date.Now

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        '○日付歯抜けチェック
        If isNormal(WW_ERRCODE) Then
            DATE_RELATION_CHK(WW_ERRCODE)
        End If

        If isNormal(WW_ERRCODE) Then
            Try
                '○ジャーナル用データ
                Master.CreateEmptyTable(MA0004_SHARYOCtbl)
                'メッセージ初期化
                rightview.setErrorReport("")

                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open()       'DataBase接続(Open)

                    Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu " _
                        & "     FROM MA004_SHARYOC " _
                        & "     WHERE    CAMPCODE     = @P01 " _
                        & "       and    SHARYOTYPE   = @P02 " _
                        & "       and    TSHABAN      = @P03 " _
                        & "       and    STYMD        = @P04 ; " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE MA004_SHARYOC " _
                        & "       SET    ENDYMD = @P05 , " _
                        & "              LICNPLTNO1 = @P06 , " _
                        & "              LICNPLTNO2 = @P07 , " _
                        & "              LICNMNFACT = @P08 , " _
                        & "              LICNFRAMENO = @P09 , " _
                        & "              LICNMODEL = @P10 , " _
                        & "              LICNMOTOR = @P11 , " _
                        & "              LICNLDCAPA = @P12 , " _
                        & "              LICN5LDCAPA = @P13 , " _
                        & "              LICNWEIGHT = @P14 , " _
                        & "              LICNTWEIGHT = @P15 , " _
                        & "              LICNCWEIGHT = @P16 , " _
                        & "              LICNYMD = @P17 , " _
                        & "              TAXLINSYMD = @P18 , " _
                        & "              TAXLINS = @P19 , " _
                        & "              TAXVTAX = @P20 , " _
                        & "              TAXATAX = @P21 , " _
                        & "              OTNKTINSYMD = @P22 , " _
                        & "              OTNKTINSNYMD = @P23 , " _
                        & "              HPRSJINSYMD = @P24 , " _
                        & "              HPRSINSYMD = @P25 , " _
                        & "              HPRSINSNYMD = @P26 , " _
                        & "              CHEMTINSYMD = @P27 , " _
                        & "              CHEMTINSNYMD = @P28 , " _
                        & "              INSKBN = @P35 , " _
                        & "              DELFLG = @P29 , " _
                        & "              UPDYMD = @P31 , " _
                        & "              UPDUSER = @P32 , " _
                        & "              UPDTERMID    = @P33 , " _
                        & "              RECEIVEYMD   = @P34  " _
                        & "     WHERE    CAMPCODE       = @P01 " _
                        & "       and    SHARYOTYPE        = @P02 " _
                        & "       and    TSHABAN     = @P03 " _
                        & "       and    STYMD        = @P04 ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO MA004_SHARYOC " _
                        & "             (CAMPCODE , " _
                        & "              SHARYOTYPE , " _
                        & "              TSHABAN , " _
                        & "              STYMD , " _
                        & "              ENDYMD , " _
                        & "              LICNPLTNO1 , " _
                        & "              LICNPLTNO2 , " _
                        & "              LICNMNFACT , " _
                        & "              LICNFRAMENO , " _
                        & "              LICNMODEL , " _
                        & "              LICNMOTOR , " _
                        & "              LICNLDCAPA , " _
                        & "              LICN5LDCAPA , " _
                        & "              LICNWEIGHT , " _
                        & "              LICNTWEIGHT , " _
                        & "              LICNCWEIGHT , " _
                        & "              LICNYMD , " _
                        & "              TAXLINSYMD , " _
                        & "              TAXLINS , " _
                        & "              TAXVTAX , " _
                        & "              TAXATAX , " _
                        & "              OTNKTINSYMD , " _
                        & "              OTNKTINSNYMD , " _
                        & "              HPRSJINSYMD , " _
                        & "              HPRSINSYMD , " _
                        & "              HPRSINSNYMD , " _
                        & "              CHEMTINSYMD , " _
                        & "              CHEMTINSNYMD , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER , " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD , " _
                        & "              INSKBN ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10," _
                        & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20," _
                        & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30," _
                        & "              @P31,@P32,@P33,@P34,@P35);" _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

                    Dim SQLStr2 As String =
                          " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP " _
                        & "     FROM MA004_SHARYOC " _
                        & "     WHERE    CAMPCODE       = @P01 " _
                        & "       and    SHARYOTYPE     = @P02 " _
                        & "       and    TSHABAN        = @P03 " _
                        & "       and    STYMD          = @P04 ; "

                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 19)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Date)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 5)
                        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 15)
                        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20)
                        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)
                        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)
                        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)
                        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.BigInt)
                        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.BigInt)
                        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.BigInt)
                        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.BigInt)
                        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.BigInt)
                        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.DateTime)
                        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.DateTime)
                        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Money)
                        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Money)
                        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Money)
                        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)
                        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Date)
                        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Date)
                        Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Date)
                        Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Date)
                        Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Date)
                        Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Date)
                        Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 1)
                        Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.SmallDateTime)
                        Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)
                        Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)
                        Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 30)
                        Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.DateTime)
                        Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 1)

                        Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", SqlDbType.NVarChar, 1)
                        Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", SqlDbType.NVarChar, 19)
                        Dim PARA4 As SqlParameter = SQLcmd2.Parameters.Add("@P04", SqlDbType.Date)

                        For Each MA0004row As DataRow In MA0004tbl.Rows
                            If MA0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                                MA0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then

                                '○ＤＢ更新

                                '削除は更新しない
                                If MA0004row("DELFLG") = C_DELETE_FLG.DELETE AndAlso
                                    MA0004row("TIMSTP") = "0" Then
                                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                    Continue For
                                End If

                                PARA01.Value = MA0004row("CAMPCODE")
                                PARA02.Value = MA0004row("SHARYOTYPE")
                                PARA03.Value = MA0004row("TSHABAN")
                                PARA04.Value = RTrim(MA0004row("STYMD"))
                                PARA05.Value = RTrim(MA0004row("ENDYMD"))
                                PARA06.Value = MA0004row("LICNPLTNO1")
                                PARA07.Value = MA0004row("LICNPLTNO2")
                                PARA08.Value = MA0004row("LICNMNFACT")
                                PARA09.Value = MA0004row("LICNFRAMENO")
                                PARA10.Value = MA0004row("LICNMODEL")
                                PARA11.Value = MA0004row("LICNMOTOR")
                                PARA12.Value = MA0004row("LICNLDCAPA")
                                PARA13.Value = MA0004row("LICN5LDCAPA")
                                PARA14.Value = MA0004row("LICNWEIGHT")
                                PARA15.Value = MA0004row("LICNTWEIGHT")
                                PARA16.Value = MA0004row("LICNCWEIGHT")
                                If RTrim(MA0004row("LICNYMD")) = "" Then
                                    PARA17.Value = C_DEFAULT_YMD
                                Else
                                    PARA17.Value = MA0004row("LICNYMD")
                                End If
                                If RTrim(MA0004row("TAXLINSYMD")) = "" Then
                                    PARA18.Value = "C_DEFAULT_YMD"
                                Else
                                    PARA18.Value = MA0004row("TAXLINSYMD")
                                End If
                                PARA19.Value = MA0004row("TAXLINS")
                                PARA20.Value = MA0004row("TAXVTAX")
                                PARA21.Value = MA0004row("TAXATAX")
                                If RTrim(MA0004row("OTNKTINSYMD")) = "" Then
                                    PARA22.Value = C_DEFAULT_YMD
                                Else
                                    PARA22.Value = MA0004row("OTNKTINSYMD")
                                End If
                                If RTrim(MA0004row("OTNKTINSNYMD")) = "" Then
                                    PARA23.Value = C_DEFAULT_YMD
                                Else
                                    PARA23.Value = MA0004row("OTNKTINSNYMD")
                                End If
                                If RTrim(MA0004row("HPRSJINSYMD")) = "" Then
                                    PARA24.Value = C_DEFAULT_YMD
                                Else
                                    PARA24.Value = MA0004row("HPRSJINSYMD")
                                End If
                                If RTrim(MA0004row("HPRSINSYMD")) = "" Then
                                    PARA25.Value = C_DEFAULT_YMD
                                Else
                                    PARA25.Value = MA0004row("HPRSINSYMD")
                                End If
                                If RTrim(MA0004row("HPRSINSNYMD")) = "" Then
                                    PARA26.Value = C_DEFAULT_YMD
                                Else
                                    PARA26.Value = MA0004row("HPRSINSNYMD")
                                End If
                                If RTrim(MA0004row("CHEMTINSYMD")) = "" Then
                                    PARA27.Value = C_DEFAULT_YMD
                                Else
                                    PARA27.Value = MA0004row("CHEMTINSYMD")
                                End If
                                If RTrim(MA0004row("CHEMTINSNYMD")) = "" Then
                                    PARA28.Value = C_DEFAULT_YMD
                                Else
                                    PARA28.Value = MA0004row("CHEMTINSNYMD")
                                End If
                                PARA29.Value = MA0004row("DELFLG")
                                PARA30.Value = WW_DATENOW
                                PARA31.Value = WW_DATENOW
                                PARA32.Value = Master.USERID
                                PARA33.Value = Master.USERTERMID
                                PARA34.Value = C_DEFAULT_YMD
                                PARA35.Value = MA0004row("INSKBN")

                                SQLcmd.ExecuteNonQuery()

                                '結果 --> テーブル(MA0004tbl)反映
                                MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                                '○更新ジャーナル追加
                                Dim MA0004_SHARYOCrow As DataRow = MA0004_SHARYOCtbl.NewRow
                                MA0004_SHARYOCrow("CAMPCODE") = MA0004row("CAMPCODE")
                                MA0004_SHARYOCrow("SHARYOTYPE") = MA0004row("SHARYOTYPE")
                                MA0004_SHARYOCrow("TSHABAN") = MA0004row("TSHABAN")
                                MA0004_SHARYOCrow("STYMD") = RTrim(MA0004row("STYMD"))
                                MA0004_SHARYOCrow("ENDYMD") = RTrim(MA0004row("ENDYMD"))
                                MA0004_SHARYOCrow("LICNPLTNO1") = MA0004row("LICNPLTNO1")
                                MA0004_SHARYOCrow("LICNPLTNO2") = MA0004row("LICNPLTNO2")
                                MA0004_SHARYOCrow("LICNMNFACT") = MA0004row("LICNMNFACT")
                                MA0004_SHARYOCrow("LICNFRAMENO") = MA0004row("LICNFRAMENO")
                                MA0004_SHARYOCrow("LICNMODEL") = MA0004row("LICNMODEL")
                                MA0004_SHARYOCrow("LICNMOTOR") = MA0004row("LICNMOTOR")
                                MA0004_SHARYOCrow("LICNLDCAPA") = MA0004row("LICNLDCAPA")
                                MA0004_SHARYOCrow("LICN5LDCAPA") = MA0004row("LICN5LDCAPA")
                                MA0004_SHARYOCrow("LICNWEIGHT") = MA0004row("LICNWEIGHT")
                                MA0004_SHARYOCrow("LICNTWEIGHT") = MA0004row("LICNTWEIGHT")
                                MA0004_SHARYOCrow("LICNCWEIGHT") = MA0004row("LICNCWEIGHT")
                                If RTrim(MA0004row("LICNYMD")) = "" Then
                                    MA0004_SHARYOCrow("LICNYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("LICNYMD") = MA0004row("LICNYMD")
                                End If
                                If RTrim(MA0004row("TAXLINSYMD")) = "" Then
                                    MA0004_SHARYOCrow("TAXLINSYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("TAXLINSYMD") = MA0004row("TAXLINSYMD")
                                End If
                                MA0004_SHARYOCrow("TAXLINS") = MA0004row("TAXLINS")
                                MA0004_SHARYOCrow("TAXVTAX") = MA0004row("TAXVTAX")
                                MA0004_SHARYOCrow("TAXATAX") = MA0004row("TAXATAX")
                                If RTrim(MA0004row("OTNKTINSYMD")) = "" Then
                                    MA0004_SHARYOCrow("OTNKTINSYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("OTNKTINSYMD") = MA0004row("OTNKTINSYMD")
                                End If
                                If RTrim(MA0004row("OTNKTINSNYMD")) = "" Then
                                    MA0004_SHARYOCrow("OTNKTINSNYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("OTNKTINSNYMD") = MA0004row("OTNKTINSNYMD")
                                End If
                                If RTrim(MA0004row("HPRSJINSYMD")) = "" Then
                                    MA0004_SHARYOCrow("HPRSJINSYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("HPRSJINSYMD") = MA0004row("HPRSJINSYMD")
                                End If
                                If RTrim(MA0004row("HPRSINSYMD")) = "" Then
                                    MA0004_SHARYOCrow("HPRSINSYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("HPRSINSYMD") = MA0004row("HPRSINSYMD")
                                End If
                                If RTrim(MA0004row("HPRSINSNYMD")) = "" Then
                                    MA0004_SHARYOCrow("HPRSINSNYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("HPRSINSNYMD") = MA0004row("HPRSINSNYMD")
                                End If
                                If RTrim(MA0004row("CHEMTINSYMD")) = "" Then
                                    MA0004_SHARYOCrow("CHEMTINSYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("CHEMTINSYMD") = MA0004row("CHEMTINSYMD")
                                End If
                                If RTrim(MA0004row("CHEMTINSNYMD")) = "" Then
                                    MA0004_SHARYOCrow("CHEMTINSNYMD") = DBNull.Value
                                Else
                                    MA0004_SHARYOCrow("CHEMTINSNYMD") = MA0004row("CHEMTINSNYMD")
                                End If
                                MA0004_SHARYOCrow("INSKBN") = MA0004row("INSKBN")
                                MA0004_SHARYOCrow("DELFLG") = MA0004row("DELFLG")
                                CS0020JOURNAL.TABLENM = "MA004_SHARYOC"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = MA0004_SHARYOCrow
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

                                '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                                PARA1.Value = MA0004row("CAMPCODE")
                                PARA2.Value = MA0004row("SHARYOTYPE")
                                PARA3.Value = MA0004row("TSHABAN")
                                PARA4.Value = RTrim(MA0004row("STYMD"))

                                Using SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()
                                    If SQLdr2.Read Then
                                        MA0004row("TIMSTP") = SQLdr2("TIMSTP")
                                    End If
                                End Using

                                'PDF更新処理
                                PDF_DBupdate(MA0004row("SHARYOTYPE"), MA0004row("TSHABAN"), MA0004row("STYMD"))
                            End If
                        Next
                    End Using
                End Using
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA004_SHARYOC UPDATE_INSERT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:MA004_SHARYOC UPDATE_INSERT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                Exit Sub
            End Try
        End If

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)

        '○画面編集
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
        CS0030REPORT.MAPID = GRMA0004WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = MA0004tbl                        'データ参照DataTable
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
        CS0030REPORT.MAPID = GRMA0004WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MA0004tbl                        'データ参照DataTable
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

    ' ******************************************************************************
    ' ***  最終頁ボタン処理                                                      ***
    ' ******************************************************************************
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(MA0004tbl)
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

        '○Grid内容(MA0004tbl)よりDetail編集

        WF_Sel_LINECNT.Text = MA0004tbl.Rows(WW_LINECNT)("LINECNT")
        WF_CAMPCODE.Text = MA0004tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_SHARYOTYPE.Text = MA0004tbl.Rows(WW_LINECNT)("SHARYOTYPE")
        WF_TSHABAN.Text = MA0004tbl.Rows(WW_LINECNT)("TSHABAN")
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE.Text, WF_TSHABAN1_TEXT.Text, WW_DUMMY)
        WF_STYMD.Text = MA0004tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = MA0004tbl.Rows(WW_LINECNT)("ENDYMD")
        WF_DELFLG.Text = MA0004tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)


        '○タブ別処理
        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                '左
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0004tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_1"), Label).Text = WW_TEXT
                End If

                '中央
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0004tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_2"), Label).Text = WW_TEXT
                End If

                '右
                WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label)
                If WW_FILED_OBJ.Text <> "" Then
                    '値設定
                    WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MA0004tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox).Text = WW_VALUE
                    '値（名称）設定
                    CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY, {"", ""})
                    CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_TEXT_3"), Label).Text = WW_TEXT
                End If
            Next
        Next

        '品名２名の取り直し
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
        PDF_INITread(WF_CAMPCODE.Text, WF_SHARYOTYPE.Text, WF_TSHABAN.Text, WF_STYMD.Text)

        '○タブ（内容）の表示非表示
        TAB_DisplayCTRL(MA0004tbl.Rows(WW_LINECNT)("SHARYOTYPE"))

        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0004row As DataRow In MA0004tbl.Rows
            Select Case MA0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MA0004tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MA0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MA0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MA0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MA0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MA0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)

        '○メッセージ表示
        Master.output(C_MESSAGE_NO.DETAIL_VIEW_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_STYMD.Focus()

    End Sub
    ''' <summary>
    ''' フィールドフォーマット処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "TAXLINS", "TAXVTAX", "TAXATAX"
                Try
                    WF_ITEM_FORMAT = Format(CInt(I_VALUE), "#,#")
                Catch ex As Exception
                End Try
            Case "LICNLDCAPA", "LICN5LDCAPA", "LICN5LDCAPA", "LICNWEIGHT", "LICNTWEIGHT", "LICNCWEIGHT"
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

        '○入力値テーブル作成
        DetailBoxToMA0004INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPUT_CHEK(WW_ERRCODE)

        '○入力値テーブル反映(MA0004INPtbl⇒MA0004tbl)
        If isNormal(WW_ERRCODE) Then
            TBL_UPD("MAP", WW_ERRCODE)
        End If

        '○PDF更新
        PDF_SAVE_H()

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)

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
    Protected Sub DetailBoxToMA0004INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Master.CreateEmptyTable(MA0004INPtbl)
        Dim MA0004INProw As DataRow = MA0004INPtbl.NewRow
        '初期クリア
        For Each MA0004INPcol As DataColumn In MA0004INPtbl.Columns
            If IsDBNull(MA0004INProw.Item(MA0004INPcol)) OrElse IsNothing(MA0004INProw.Item(MA0004INPcol)) Then
                Select Case MA0004INPcol.ColumnName
                    Case "LINECNT"
                        MA0004INProw.Item(MA0004INPcol) = 0
                    Case "TIMSTP"
                        MA0004INProw.Item(MA0004INPcol) = 0
                    Case "SELECT"
                        MA0004INProw.Item(MA0004INPcol) = 1
                    Case "HIDDEN"
                        MA0004INProw.Item(MA0004INPcol) = 0
                    Case "WORK_NO"
                        MA0004INProw.Item(MA0004INPcol) = 0
                    Case Else
                        If MA0004INPcol.DataType.Name = "String" Then
                            MA0004INProw.Item(MA0004INPcol) = ""
                        Else
                            MA0004INProw.Item(MA0004INPcol) = 0
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

        '○DetailよりMA0004INPtbl編集
        If WF_Sel_LINECNT.Text = "" Then
            MA0004INProw("LINECNT") = 0
        Else
            MA0004INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If

        MA0004INProw("CAMPCODE") = WF_CAMPCODE.Text
        MA0004INProw("SHARYOTYPE") = WF_SHARYOTYPE.Text
        MA0004INProw("TSHABAN") = WF_TSHABAN.Text
        MA0004INProw("STYMD") = WF_STYMD.Text
        MA0004INProw("ENDYMD") = WF_ENDYMD.Text
        MA0004INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_SHARYOTYPE.Text) AndAlso
            String.IsNullOrEmpty(WF_TSHABAN.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMA0004INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○タブ別処理(01 管理)
        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                '左
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0004INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
                End If

                '中央
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0004INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
                End If

                '右
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text <> "" Then
                    CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox).Text
                    CS0010CHARstr.CS0010CHARget()
                    MA0004INProw(CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
                End If
            Next
        Next

        '○名称付与
        MA0004INProw("MANGMORGNAME") = ""
        CODENAME_get("MANGMORG", MA0004INProw("MANGMORG"), MA0004INProw("MANGMORGNAME"), WW_DUMMY)                          '管理部署名
        MA0004INProw("MANGSORGNAME") = ""
        CODENAME_get("MANGSORG", MA0004INProw("MANGSORG"), MA0004INProw("MANGSORGNAME"), WW_DUMMY)                          '設置部署名
        MA0004INProw("MANGOILTYPENAME") = ""
        CODENAME_get("MANGOILTYPE", MA0004INProw("MANGOILTYPE"), MA0004INProw("MANGOILTYPENAME"), WW_DUMMY)                 '油種名
        MA0004INProw("MANGOWNCODENAME") = ""
        CODENAME_get("MANGOWNCODE", MA0004INProw("MANGOWNCODE"), MA0004INProw("MANGOWNCODENAME"), WW_DUMMY)                 '荷主名
        MA0004INProw("MANGOWNCONTNAME") = ""
        CODENAME_get("MANGOWNCONT", MA0004INProw("MANGOWNCONT"), MA0004INProw("MANGOWNCONTNAME"), WW_DUMMY)                 '契約区分名
        MA0004INProw("MANGSUPPLNAME") = ""
        CODENAME_get("MANGSUPPL", MA0004INProw("MANGSUPPL"), MA0004INProw("MANGSUPPLNAME"), WW_DUMMY)                       '庸車会社名
        MA0004INProw("MANGUORGNAME") = ""
        MA0004INProw("BASELEASENAME") = ""
        CODENAME_get("BASELEASE", MA0004INProw("BASELEASE"), MA0004INProw("BASELEASENAME"), WW_DUMMY)                       '車両所有名
        MA0004INProw("FCTRAXLENAME") = ""
        CODENAME_get("FCTRAXLE", MA0004INProw("FCTRAXLE"), MA0004INProw("FCTRAXLENAME"), WW_DUMMY)                          'リフトアクスル名
        MA0004INProw("FCTRTMAKER") = ""
        MA0004INProw("FCTRTMAKERNAME") = ""
        MA0004INProw("FCTRDPRNAME") = ""
        CODENAME_get("FCTRDPR", MA0004INProw("FCTRDPR"), MA0004INProw("FCTRDPRNAME"), WW_DUMMY)                             'DPR名
        MA0004INProw("FCTRFUELMATENAME") = ""
        CODENAME_get("FCTRFUELMATE", MA0004INProw("FCTRFUELMATE"), MA0004INProw("FCTRFUELMATENAME"), WW_DUMMY)              '燃料タンク材質名
        MA0004INProw("FCTRSHFTNUMNAME") = ""
        CODENAME_get("FCTRSHFTNUM", MA0004INProw("FCTRSHFTNUM"), MA0004INProw("FCTRSHFTNUMNAME"), WW_DUMMY)                 '軸数名
        MA0004INProw("FCTRSUSPNAME") = ""
        CODENAME_get("FCTRSUSP", MA0004INProw("FCTRSUSP"), MA0004INProw("FCTRSUSPNAME"), WW_DUMMY)                          'サスペンション種類名
        MA0004INProw("FCTRTMISSIONNAME") = ""
        CODENAME_get("FCTRTMISSION", MA0004INProw("FCTRTMISSION"), MA0004INProw("FCTRTMISSIONNAME"), WW_DUMMY)              'ミッション名
        MA0004INProw("FCTRUREANAME") = ""
        CODENAME_get("FCTRUREA", MA0004INProw("FCTRUREA"), MA0004INProw("FCTRUREANAME"), WW_DUMMY)                          '尿素名
        MA0004INProw("OTNKBPIPENAME") = ""
        CODENAME_get("OTNKBPIPE", MA0004INProw("OTNKBPIPE"), MA0004INProw("OTNKBPIPENAME"), WW_DUMMY)                       '後配管名
        MA0004INProw("OTNKVAPORNAME") = ""
        CODENAME_get("OTNKVAPOR", MA0004INProw("OTNKVAPOR"), MA0004INProw("OTNKVAPORNAME"), WW_DUMMY)                       'ベーパー名
        MA0004INProw("OTNKCVALVENAME") = ""
        CODENAME_get("OTNKCVALVE", MA0004INProw("OTNKCVALVE"), MA0004INProw("OTNKCVALVENAME"), WW_DUMMY)                    '中間ﾊﾞﾙﾌﾞ有無名
        MA0004INProw("OTNKDCDNAME") = ""
        CODENAME_get("OTNKDCD", MA0004INProw("OTNKDCD"), MA0004INProw("OTNKDCDNAME"), WW_DUMMY)                             'ＤＣＤ装備名
        MA0004INProw("FCTRSMAKERNAME") = ""
        CODENAME_get("FCTRSMAKER", MA0004INProw("FCTRSMAKER"), MA0004INProw("FCTRSMAKERNAME"), WW_DUMMY)                    '車両メーカー
        MA0004INProw("OTNKDETECTORNAME") = ""
        CODENAME_get("OTNKDETECTOR", MA0004INProw("OTNKDETECTOR"), MA0004INProw("OTNKDETECTORNAME"), WW_DUMMY)              '検水管名
        MA0004INProw("OTNKDISGORGENAME") = ""
        CODENAME_get("OTNKDISGORGE", MA0004INProw("OTNKDISGORGE"), MA0004INProw("OTNKDISGORGENAME"), WW_DUMMY)              '吐出口名
        MA0004INProw("OTNKHTECHNAME") = ""
        CODENAME_get("OTNKHTECH", MA0004INProw("OTNKHTECH"), MA0004INProw("OTNKHTECHNAME"), WW_DUMMY)                       'ハイテク種別名
        MA0004INProw("OTNKLVALVENAME") = ""
        CODENAME_get("OTNKLVALVE", MA0004INProw("OTNKLVALVE"), MA0004INProw("OTNKLVALVENAME"), WW_DUMMY)                    '底弁形式名
        MA0004INProw("OTNKMATERIALNAME") = ""
        CODENAME_get("OTNKMATERIAL", MA0004INProw("OTNKMATERIAL"), MA0004INProw("OTNKMATERIALNAME"), WW_DUMMY)              'タンク材質名
        MA0004INProw("OTNKPIPENAME") = ""
        CODENAME_get("OTNKPIPE", MA0004INProw("OTNKPIPE"), MA0004INProw("OTNKPIPENAME"), WW_DUMMY)                          '配管形態名
        MA0004INProw("OTNKPIPESIZENAME") = ""
        CODENAME_get("OTNKPIPESIZE", MA0004INProw("OTNKPIPESIZE"), MA0004INProw("OTNKPIPESIZENAME"), WW_DUMMY)              '配管サイズ名
        MA0004INProw("OTNKPUMPNAME") = ""
        CODENAME_get("OTNKPUMP", MA0004INProw("OTNKPUMP"), MA0004INProw("OTNKPUMPNAME"), WW_DUMMY)                          'ポンプ名
        MA0004INProw("HPRSPMPDRNAME") = ""
        CODENAME_get("HPRSPMPDR", MA0004INProw("HPRSPMPDR"), MA0004INProw("HPRSPMPDRNAME"), WW_DUMMY)                       'ポンプ駆動方法
        MA0004INProw("HPRSINSULATENAME") = ""
        CODENAME_get("HPRSINSULATE", MA0004INProw("HPRSINSULATE"), MA0004INProw("HPRSINSULATENAME"), WW_DUMMY)              '断熱構造名
        MA0004INProw("HPRSMATRNAME") = ""
        CODENAME_get("HPRSMATR", MA0004INProw("HPRSMATR"), MA0004INProw("HPRSMATRNAME"), WW_DUMMY)                          'タンク材質名
        MA0004INProw("HPRSPIPENAME") = ""
        CODENAME_get("HPRSPIPE", MA0004INProw("HPRSPIPE"), MA0004INProw("HPRSPIPENAME"), WW_DUMMY)                          '配管形状（仮）名
        MA0004INProw("HPRSPIPENUMNAME") = ""
        CODENAME_get("HPRSPIPENUM", MA0004INProw("HPRSPIPENUM"), MA0004INProw("HPRSPIPENUMNAME"), WW_DUMMY)                 '配管口数名
        MA0004INProw("HPRSPUMPNAME") = ""
        CODENAME_get("HPRSPUMP", MA0004INProw("HPRSPUMP"), MA0004INProw("HPRSPUMPNAME"), WW_DUMMY)                          'ポンプ名
        MA0004INProw("HPRSRESSRENAME") = ""
        CODENAME_get("HPRSRESSRE", MA0004INProw("HPRSRESSRE"), MA0004INProw("HPRSRESSRENAME"), WW_DUMMY)                    '加圧器名
        MA0004INProw("HPRSSTRUCTNAME") = ""
        CODENAME_get("HPRSSTRUCT", MA0004INProw("HPRSSTRUCT"), MA0004INProw("HPRSSTRUCTNAME"), WW_DUMMY)                    'タンク構造名
        MA0004INProw("HPRSVALVENAME") = ""
        CODENAME_get("HPRSVALVE", MA0004INProw("HPRSVALVE"), MA0004INProw("HPRSVALVENAME"), WW_DUMMY)                       '底弁形式名
        MA0004INProw("CHEMDISGORGENAME") = ""
        CODENAME_get("CHEMDISGORGE", MA0004INProw("CHEMDISGORGE"), MA0004INProw("CHEMDISGORGENAME"), WW_DUMMY)              '吐出口名
        MA0004INProw("CHEMHOSENAME") = ""
        CODENAME_get("CHEMHOSE", MA0004INProw("CHEMHOSE"), MA0004INProw("CHEMHOSENAME"), WW_DUMMY)                          'ホースボックス名
        MA0004INProw("CHEMMANOMTRNAME") = ""
        CODENAME_get("CHEMMANOMTR", MA0004INProw("CHEMMANOMTR"), MA0004INProw("CHEMMANOMTRNAME"), WW_DUMMY)                 '圧力計名
        MA0004INProw("CHEMMATERIALNAME") = ""
        CODENAME_get("CHEMMATERIAL", MA0004INProw("CHEMMATERIAL"), MA0004INProw("CHEMMATERIALNAME"), WW_DUMMY)              'タンク材質名
        MA0004INProw("CHEMPMPDRNAME") = ""
        CODENAME_get("CHEMPMPDR", MA0004INProw("CHEMPMPDR"), MA0004INProw("CHEMPMPDRNAME"), WW_DUMMY)                       'ポンプ駆動方法名
        MA0004INProw("CHEMPRESDRVNAME") = ""
        CODENAME_get("CHEMPRESDRV", MA0004INProw("CHEMPRESDRV"), MA0004INProw("CHEMPRESDRVNAME"), WW_DUMMY)                 '加温装置名
        MA0004INProw("CHEMPRESEQNAME") = ""
        CODENAME_get("CHEMPRESEQ", MA0004INProw("CHEMPRESEQ"), MA0004INProw("CHEMPRESEQNAME"), WW_DUMMY)                    '均圧配管名
        MA0004INProw("CHEMPUMPNAME") = ""
        CODENAME_get("CHEMPUMP", MA0004INProw("CHEMPUMP"), MA0004INProw("CHEMPUMPNAME"), WW_DUMMY)                          'ポンプ名
        MA0004INProw("CHEMSTRUCTNAME") = ""
        CODENAME_get("CHEMSTRUCT", MA0004INProw("CHEMSTRUCT"), MA0004INProw("CHEMSTRUCTNAME"), WW_DUMMY)                    'タンク構造名
        MA0004INProw("CHEMTHERMNAME") = ""
        CODENAME_get("CHEMTHERM", MA0004INProw("CHEMTHERM"), MA0004INProw("CHEMTHERMNAME"), WW_DUMMY)                       '温度計名
        MA0004INProw("OTHRBMONITORNAME") = ""
        CODENAME_get("OTHRBMONITOR", MA0004INProw("OTHRBMONITOR"), MA0004INProw("OTHRBMONITORNAME"), WW_DUMMY)              'バックモニター名
        MA0004INProw("OTHRBSONARNAME") = ""
        CODENAME_get("OTHRBSONAR", MA0004INProw("OTHRBSONAR"), MA0004INProw("OTHRBSONARNAME"), WW_DUMMY)                    'バックソナー名
        MA0004INProw("FCTRTIRENAME") = ""
        CODENAME_get("FCTRTIRE", MA0004INProw("FCTRTIRE"), MA0004INProw("FCTRTIRENAME"), WW_DUMMY)                          'ＤoCoですCar番号名
        MA0004INProw("OTHRDRRECORDNAME") = ""
        CODENAME_get("OTHRDRRECORD", MA0004INProw("OTHRDRRECORD"), MA0004INProw("OTHRDRRECORDNAME"), WW_DUMMY)              'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
        MA0004INProw("OTHRPAINTINGNAME") = ""
        CODENAME_get("OTHRPAINTING", MA0004INProw("OTHRPAINTING"), MA0004INProw("OTHRPAINTINGNAME"), WW_DUMMY)              '塗装名
        MA0004INProw("OTHRRADIOCONNAME") = ""
        CODENAME_get("OTHRRADIOCON", MA0004INProw("OTHRRADIOCON"), MA0004INProw("OTHRRADIOCONNAME"), WW_DUMMY)              '無線（有・無）名
        MA0004INProw("OTHRRTARGETNAME") = ""
        CODENAME_get("OTHRRTARGET", MA0004INProw("OTHRRTARGET"), MA0004INProw("OTHRRTARGETNAME"), WW_DUMMY)                 '一括修理非対象車名
        MA0004INProw("OTHRTERMINALNAME") = ""
        CODENAME_get("OTHRTERMINAL", MA0004INProw("OTHRTERMINAL"), MA0004INProw("OTHRTERMINALNAME"), WW_DUMMY)              '車載端末名
        MA0004INProw("MANGPROD1NAME") = ""
        CODENAME_get("MANGPROD1", MA0004INProw("MANGPROD1"), MA0004INProw("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0004INProw("MANGOILTYPE"))})                       '品名１
        MA0004INProw("MANGPROD2NAME") = ""
        CODENAME_get("MANGPROD2", MA0004INProw("MANGPROD1") + MA0004INProw("MANGPROD2"), MA0004INProw("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0004INProw("MANGOILTYPE")), CStr(MA0004INProw("MANGPROD1"))})   '品名２
        MA0004INProw("OTNKEXHASIZENAME") = ""
        CODENAME_get("OTNKEXHASIZE", MA0004INProw("OTNKEXHASIZE"), MA0004INProw("OTNKEXHASIZENAME"), WW_DUMMY)              '吐出口サイズ
        MA0004INProw("HPRSHOSENAME") = ""
        CODENAME_get("HPRSHOSE", MA0004INProw("HPRSHOSE"), MA0004INProw("HPRSHOSENAME"), WW_DUMMY)                          'ホースボックス
        MA0004INProw("CONTSHAPENAME") = ""
        CODENAME_get("CONTSHAPE", MA0004INProw("CONTSHAPE"), MA0004INProw("CONTSHAPENAME"), WW_DUMMY)                       'シャーシ形状
        MA0004INProw("CONTPUMPNAME") = ""
        CODENAME_get("CONTPUMP", MA0004INProw("CONTPUMP"), MA0004INProw("CONTPUMPNAME"), WW_DUMMY)                          'ポンプ
        MA0004INProw("CONTPMPDRNAME") = ""
        CODENAME_get("CONTPMPDR", MA0004INProw("CONTPMPDR"), MA0004INProw("CONTPMPDRNAME"), WW_DUMMY)                       'ポンプ駆動方法
        MA0004INProw("OTHRTPMSNAME") = ""
        CODENAME_get("OTHRTPMS", MA0004INProw("OTHRTPMS"), MA0004INProw("OTHRTPMSNAME"), WW_DUMMY)                          'TPMS
        MA0004INProw("OTNKTMAKERNAME") = ""
        CODENAME_get("OTNKTMAKER", MA0004INProw("OTNKTMAKER"), MA0004INProw("OTNKTMAKERNAME"), WW_DUMMY)                    '石油タンクメーカー
        MA0004INProw("HPRSTMAKERNAME") = ""
        CODENAME_get("HPRSTMAKER", MA0004INProw("HPRSTMAKER"), MA0004INProw("HPRSTMAKERNAME"), WW_DUMMY)                    '高圧タンクメーカー
        MA0004INProw("CHEMTMAKERNAME") = ""
        CODENAME_get("CHEMTMAKER", MA0004INProw("CHEMTMAKER"), MA0004INProw("CHEMTMAKERNAME"), WW_DUMMY)                    '化成品タンクメーカー
        MA0004INProw("CONTTMAKERNAME") = ""
        CODENAME_get("CONTTMAKER", MA0004INProw("CONTTMAKER"), MA0004INProw("CONTTMAKERNAME"), WW_DUMMY)                    'コンテナタンクメーカー
        MA0004INProw("SHARYOSTATUSNAME") = ""
        CODENAME_get("SHARYOSTATUS", MA0004INProw("SHARYOSTATUS"), MA0004INProw("SHARYOSTATUSNAME"), WW_DUMMY)              '運行状況
        MA0004INProw("INSKBNNAME") = ""
        CODENAME_get("INSKBN", MA0004INProw("INSKBN"), MA0004INProw("INSKBNNAME"), WW_DUMMY)                                '検査区分

        MA0004INPtbl.Rows.Add(MA0004INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        For Each MA0004row As DataRow In MA0004tbl.Rows
            Select Case MA0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)

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
        WF_DTAB_CHANGE_NO.Value = "9"
        WF_Detail_TABChange()
        TAB_DisplayCTRL(WF_SHARYOTYPE.Text)

        '○PDF初期画面編集
        'Repeaterバインド準備
        MA0004PDFtbl_ColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = MA0004PDFtbl
        WF_DViewRepPDF.DataBind()

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_FIELD.Value = "WF_STYMD"
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
        WF_SHARYOTYPE.ReadOnly = True
        WF_SHARYOTYPE.Style.Add("background-color", "rgb(213,208,181)")
        WF_TSHABAN.ReadOnly = True
        WF_TSHABAN.Style.Add("background-color", "rgb(213,208,181)")

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

    ' *** 詳細画面-イベント文字取得
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
            Select Case WF_DetailMView.ActiveViewIndex
                Case 9
                    WF_DViewRep10.Visible = True
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

        '管理
        WF_Dtab01.Style.Remove("color")
        WF_Dtab01.Style.Add("color", "black")
        WF_Dtab01.Style.Remove("background-color")
        WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab01.Style.Remove("border")
        WF_Dtab01.Style.Add("border", "1px solid black")
        WF_Dtab01.Style.Remove("font-weight")
        WF_Dtab01.Style.Add("font-weight", "normal")

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
        WF_Dtab03.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab03.Style.Remove("border")
        WF_Dtab03.Style.Add("border", "1px solid black")
        WF_Dtab03.Style.Remove("font-weight")
        WF_Dtab03.Style.Add("font-weight", "normal")

        '石油タンク
        WF_Dtab04.Style.Remove("color")
        WF_Dtab04.Style.Add("color", "black")
        WF_Dtab04.Style.Remove("background-color")
        WF_Dtab04.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab04.Style.Remove("border")
        WF_Dtab04.Style.Add("border", "1px solid black")
        WF_Dtab04.Style.Remove("font-weight")
        WF_Dtab04.Style.Add("font-weight", "normal")

        '高圧タンク
        WF_Dtab05.Style.Remove("color")
        WF_Dtab05.Style.Add("color", "black")
        WF_Dtab05.Style.Remove("background-color")
        WF_Dtab05.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab05.Style.Remove("border")
        WF_Dtab05.Style.Add("border", "1px solid black")
        WF_Dtab05.Style.Remove("font-weight")
        WF_Dtab05.Style.Add("font-weight", "normal")

        '化成品タンク
        WF_Dtab06.Style.Remove("color")
        WF_Dtab06.Style.Add("color", "black")
        WF_Dtab06.Style.Remove("background-color")
        WF_Dtab06.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab06.Style.Remove("border")
        WF_Dtab06.Style.Add("border", "1px solid black")
        WF_Dtab06.Style.Remove("font-weight")
        WF_Dtab06.Style.Add("font-weight", "normal")

        'コンテナ
        WF_Dtab07.Style.Remove("color")
        WF_Dtab07.Style.Add("color", "black")
        WF_Dtab07.Style.Remove("background-color")
        WF_Dtab07.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab07.Style.Remove("border")
        WF_Dtab07.Style.Add("border", "1px solid black")
        WF_Dtab07.Style.Remove("font-weight")
        WF_Dtab07.Style.Add("font-weight", "normal")

        '車両その他
        WF_Dtab08.Style.Remove("color")
        WF_Dtab08.Style.Add("color", "black")
        WF_Dtab08.Style.Remove("background-color")
        WF_Dtab08.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab08.Style.Remove("border")
        WF_Dtab08.Style.Add("border", "1px solid black")
        WF_Dtab08.Style.Remove("font-weight")
        WF_Dtab08.Style.Add("font-weight", "normal")

        '経理
        WF_Dtab09.Style.Remove("color")
        WF_Dtab09.Style.Add("color", "black")
        WF_Dtab09.Style.Remove("background-color")
        WF_Dtab09.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab09.Style.Remove("border")
        WF_Dtab09.Style.Add("border", "1px solid black")
        WF_Dtab09.Style.Remove("font-weight")
        WF_Dtab09.Style.Add("font-weight", "normal")

        '申請
        WF_Dtab10.Style.Remove("color")
        WF_Dtab10.Style.Add("color", "black")
        WF_Dtab10.Style.Remove("background-color")
        WF_Dtab10.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab10.Style.Remove("border")
        WF_Dtab10.Style.Add("border", "1px solid black")
        WF_Dtab10.Style.Remove("font-weight")
        WF_Dtab10.Style.Add("font-weight", "normal")

        '申請書類（PDF）
        WF_Dtab11.Style.Remove("color")
        WF_Dtab11.Style.Add("color", "black")
        WF_Dtab11.Style.Remove("background-color")
        WF_Dtab11.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab11.Style.Remove("border")
        WF_Dtab11.Style.Add("border", "1px solid black")
        WF_Dtab11.Style.Remove("font-weight")
        WF_Dtab11.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '管理
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
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
                WF_Dtab03.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab03.Style.Remove("border")
                WF_Dtab03.Style.Add("border", "1px solid blue")
                WF_Dtab03.Style.Remove("font-weight")
                WF_Dtab03.Style.Add("font-weight", "bold")
            Case 3
                '石油タンク
                WF_Dtab04.Style.Remove("color")
                WF_Dtab04.Style.Add("color", "blue")
                WF_Dtab04.Style.Remove("background-color")
                WF_Dtab04.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab04.Style.Remove("border")
                WF_Dtab04.Style.Add("border", "1px solid blue")
                WF_Dtab04.Style.Remove("font-weight")
                WF_Dtab04.Style.Add("font-weight", "bold")
            Case 4
                '高圧タンク
                WF_Dtab05.Style.Remove("color")
                WF_Dtab05.Style.Add("color", "blue")
                WF_Dtab05.Style.Remove("background-color")
                WF_Dtab05.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab05.Style.Remove("border")
                WF_Dtab05.Style.Add("border", "1px solid blue")
                WF_Dtab05.Style.Remove("font-weight")
                WF_Dtab05.Style.Add("font-weight", "bold")
            Case 5
                '化成品タンク
                WF_Dtab06.Style.Remove("color")
                WF_Dtab06.Style.Add("color", "blue")
                WF_Dtab06.Style.Remove("background-color")
                WF_Dtab06.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab06.Style.Remove("border")
                WF_Dtab06.Style.Add("border", "1px solid blue")
                WF_Dtab06.Style.Remove("font-weight")
                WF_Dtab06.Style.Add("font-weight", "bold")
            Case 6
                'コンテナ
                WF_Dtab07.Style.Remove("color")
                WF_Dtab07.Style.Add("color", "blue")
                WF_Dtab07.Style.Remove("background-color")
                WF_Dtab07.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab07.Style.Remove("border")
                WF_Dtab07.Style.Add("border", "1px solid blue")
                WF_Dtab07.Style.Remove("font-weight")
                WF_Dtab07.Style.Add("font-weight", "bold")
            Case 7
                '車両その他
                WF_Dtab08.Style.Remove("color")
                WF_Dtab08.Style.Add("color", "blue")
                WF_Dtab08.Style.Remove("background-color")
                WF_Dtab08.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab08.Style.Remove("border")
                WF_Dtab08.Style.Add("border", "1px solid blue")
                WF_Dtab08.Style.Remove("font-weight")
                WF_Dtab08.Style.Add("font-weight", "bold")
            Case 8
                '経理
                WF_Dtab09.Style.Remove("color")
                WF_Dtab09.Style.Add("color", "blue")
                WF_Dtab09.Style.Remove("background-color")
                WF_Dtab09.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab09.Style.Remove("border")
                WF_Dtab09.Style.Add("border", "1px solid blue")
                WF_Dtab09.Style.Remove("font-weight")
                WF_Dtab09.Style.Add("font-weight", "bold")
            Case 9
                '申請
                WF_Dtab10.Style.Remove("color")
                WF_Dtab10.Style.Add("color", "blue")
                WF_Dtab10.Style.Remove("background-color")
                WF_Dtab10.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab10.Style.Remove("border")
                WF_Dtab10.Style.Add("border", "1px solid blue")
                WF_Dtab10.Style.Remove("font-weight")
                WF_Dtab10.Style.Add("font-weight", "bold")
            Case 10
                '申請書類（PDF）
                WF_Dtab11.Style.Remove("color")
                WF_Dtab11.Style.Add("color", "blue")
                WF_Dtab11.Style.Remove("background-color")
                WF_Dtab11.Style.Add("background-color", "rgb(220,230,240)")
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

                    'PDF処理(開始日付が変更された場合、PDF情報を再取得)
                    PDF_STYMDchange()
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

            '○ディテール11PDF）変数設定
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

        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" OrElse WF_TSHABAN.Text = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
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

    ' ******************************************************************************
    ' ***  ファイルアップロード入力処理                                          *** 
    ' ******************************************************************************
    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○UPLOAD_XLSデータ取得 ○ 
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMA0004WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSUPLOADrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSUPLOADrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSUPLOADrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSUPLOADrow.Item(XLSTBLcol)) Then
                    CS0023XLSUPLOADrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSUPLOADrow.ItemArray
        Next

        '○エラーレポート準備
        Dim WW_DATE As Date

        Master.CreateEmptyTable(MA0004INPtbl)

        '○Excelデータ毎にチェック＆更新
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒MA0004INProw
            Dim MA0004INProw As DataRow = MA0004INPtbl.NewRow

            '初期クリア
            For Each MA0004INPcol As DataColumn In MA0004INPtbl.Columns
                If IsDBNull(MA0004INProw.Item(MA0004INPcol)) OrElse IsNothing(MA0004INProw.Item(MA0004INPcol)) Then
                    Select Case MA0004INPcol.ColumnName
                        Case "LINECNT"
                            MA0004INProw.Item(MA0004INPcol) = 0
                        Case "TIMSTP"
                            MA0004INProw.Item(MA0004INPcol) = 0
                        Case "SELECT"
                            MA0004INProw.Item(MA0004INPcol) = 1
                        Case "HIDDEN"
                            MA0004INProw.Item(MA0004INPcol) = 0
                        Case "WORK_NO"
                            MA0004INProw.Item(MA0004INPcol) = 0
                        Case Else
                            If MA0004INPcol.DataType.Name = "String" Then
                                MA0004INProw.Item(MA0004INPcol) = ""
                            Else
                                MA0004INProw.Item(MA0004INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("SHARYOTYPE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TSHABAN") >= 0 Then

                For Each MA0004row As DataRow In MA0004tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MA0004row("CAMPCODE") AndAlso
                       XLSTBLrow("SHARYOTYPE") = MA0004row("SHARYOTYPE") AndAlso
                       XLSTBLrow("TSHABAN") = MA0004row("TSHABAN") Then
                        '最新レコード判定
                        If MA0004row("STYMD") = "" Then
                            If WW_STYMD < MA0004row("STYMD_B") Then
                                WW_STYMD = MA0004row("STYMD_B")
                                MA0004INProw.ItemArray = MA0004row.ItemArray
                            End If
                        Else
                            If MA0004row("STYMD") = XLSTBLrow("STYMD") Then
                                WW_STYMD = MA0004row("STYMD")
                                MA0004INProw.ItemArray = MA0004row.ItemArray
                                Exit For
                            End If
                        End If
                    End If
                Next
            End If

            '○項目セット
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MA0004INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPE") >= 0 Then
                MA0004INProw("SHARYOTYPE") = XLSTBLrow("SHARYOTYPE")
            End If

            If WW_COLUMNS.IndexOf("TSHABAN") >= 0 Then
                MA0004INProw("TSHABAN") = XLSTBLrow("TSHABAN")
            End If

            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    WW_DATE = XLSTBLrow("STYMD")
                    MA0004INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    WW_DATE = XLSTBLrow("ENDYMD")
                    MA0004INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MA0004INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("MANGUORG") >= 0 Then
                MA0004INProw("MANGUORG") = XLSTBLrow("MANGUORG")
            End If

            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                MA0004INProw("GSHABAN") = XLSTBLrow("GSHABAN")
            End If

            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MA0004INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 Then
                MA0004INProw("SHARYOTYPEF") = XLSTBLrow("SHARYOTYPEF")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 Then
                MA0004INProw("SHARYOTYPEB") = XLSTBLrow("SHARYOTYPEB")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB2") >= 0 Then
                MA0004INProw("SHARYOTYPEB2") = XLSTBLrow("SHARYOTYPEB2")
            End If

            If WW_COLUMNS.IndexOf("SHARYOTYPEB3") >= 0 Then
                MA0004INProw("SHARYOTYPEB3") = XLSTBLrow("SHARYOTYPEB3")
            End If

            If WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                MA0004INProw("TSHABANF") = XLSTBLrow("TSHABANF")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                MA0004INProw("TSHABANB") = XLSTBLrow("TSHABANB")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB2") >= 0 Then
                MA0004INProw("TSHABANB2") = XLSTBLrow("TSHABANB2")
            End If

            If WW_COLUMNS.IndexOf("TSHABANB3") >= 0 Then
                MA0004INProw("TSHABANB3") = XLSTBLrow("TSHABANB3")
            End If

            If WW_COLUMNS.IndexOf("MANGOILTYPE") >= 0 Then
                MA0004INProw("MANGOILTYPE") = XLSTBLrow("MANGOILTYPE")
            End If

            If WW_COLUMNS.IndexOf("BASERAGEYY") >= 0 Then
                MA0004INProw("BASERAGEYY") = XLSTBLrow("BASERAGEYY")
            End If

            If WW_COLUMNS.IndexOf("BASERAGEMM") >= 0 Then
                MA0004INProw("BASERAGEMM") = XLSTBLrow("BASERAGEMM")
            End If

            If WW_COLUMNS.IndexOf("BASERAGE") >= 0 Then
                MA0004INProw("BASERAGE") = XLSTBLrow("BASERAGE")
            End If

            If WW_COLUMNS.IndexOf("BASERDATE") >= 0 Then
                If IsDate(XLSTBLrow("BASERDATE")) Then
                    WW_DATE = XLSTBLrow("BASERDATE")
                    MA0004INProw("BASERDATE") = WW_DATE.ToString("yyyy/MM/dd")
                    Dim WW_DATENOW As Date = Date.Now
                    Dim WW_BASERAGEYY As Integer
                    Dim WW_BASERAGE As Integer
                    Dim WW_BASERAGEMM As Integer
                    WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                    WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                    WW_BASERAGEMM = WW_BASERAGE Mod 12
                    MA0004INProw("BASERAGEMM") = WW_BASERAGEMM
                    MA0004INProw("BASERAGEYY") = WW_BASERAGEYY
                    MA0004INProw("BASERAGE") = WW_BASERAGE
                End If
            End If

            If WW_COLUMNS.IndexOf("MANGMORG") >= 0 Then
                MA0004INProw("MANGMORG") = XLSTBLrow("MANGMORG")
            End If

            If WW_COLUMNS.IndexOf("MANGOWNCONT") >= 0 Then
                MA0004INProw("MANGOWNCONT") = XLSTBLrow("MANGOWNCONT")
            End If

            If WW_COLUMNS.IndexOf("BASELEASE") >= 0 Then
                MA0004INProw("BASELEASE") = XLSTBLrow("BASELEASE")
            End If

            If WW_COLUMNS.IndexOf("MANGSUPPL") >= 0 Then
                MA0004INProw("MANGSUPPL") = XLSTBLrow("MANGSUPPL")
            End If

            If WW_COLUMNS.IndexOf("MANGSHAFUKU") >= 0 Then
                MA0004INProw("MANGSHAFUKU") = XLSTBLrow("MANGSHAFUKU")
            End If

            If WW_COLUMNS.IndexOf("MANGSORG") >= 0 Then
                MA0004INProw("MANGSORG") = XLSTBLrow("MANGSORG")
            End If

            If WW_COLUMNS.IndexOf("MANGOWNCODE") >= 0 Then
                MA0004INProw("MANGOWNCODE") = XLSTBLrow("MANGOWNCODE")
            End If

            If WW_COLUMNS.IndexOf("MANGTTLDIST") >= 0 Then
                MA0004INProw("MANGTTLDIST") = XLSTBLrow("MANGTTLDIST")
            End If

            If WW_COLUMNS.IndexOf("ACCTRCYCLE") >= 0 Then
                MA0004INProw("ACCTRCYCLE") = XLSTBLrow("ACCTRCYCLE")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST06") >= 0 Then
                MA0004INProw("ACCTASST06") = XLSTBLrow("ACCTASST06")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST07") >= 0 Then
                MA0004INProw("ACCTASST07") = XLSTBLrow("ACCTASST07")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST08") >= 0 Then
                MA0004INProw("ACCTASST08") = XLSTBLrow("ACCTASST08")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST09") >= 0 Then
                MA0004INProw("ACCTASST09") = XLSTBLrow("ACCTASST09")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST10") >= 0 Then
                MA0004INProw("ACCTASST10") = XLSTBLrow("ACCTASST10")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE1") >= 0 Then
                MA0004INProw("ACCTLEASE1") = XLSTBLrow("ACCTLEASE1")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE4") >= 0 Then
                MA0004INProw("ACCTLEASE4") = XLSTBLrow("ACCTLEASE4")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL2") >= 0 Then
                MA0004INProw("ACCTLSUPL2") = XLSTBLrow("ACCTLSUPL2")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL5") >= 0 Then
                MA0004INProw("ACCTLSUPL5") = XLSTBLrow("ACCTLSUPL5")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST01") >= 0 Then
                MA0004INProw("ACCTASST01") = XLSTBLrow("ACCTASST01")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST02") >= 0 Then
                MA0004INProw("ACCTASST02") = XLSTBLrow("ACCTASST02")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST03") >= 0 Then
                MA0004INProw("ACCTASST03") = XLSTBLrow("ACCTASST03")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST04") >= 0 Then
                MA0004INProw("ACCTASST04") = XLSTBLrow("ACCTASST04")
            End If

            If WW_COLUMNS.IndexOf("ACCTASST05") >= 0 Then
                MA0004INProw("ACCTASST05") = XLSTBLrow("ACCTASST05")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE2") >= 0 Then
                MA0004INProw("ACCTLEASE2") = XLSTBLrow("ACCTLEASE2")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE5") >= 0 Then
                MA0004INProw("ACCTLEASE5") = XLSTBLrow("ACCTLEASE5")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL3") >= 0 Then
                MA0004INProw("ACCTLSUPL3") = XLSTBLrow("ACCTLSUPL3")
            End If

            If WW_COLUMNS.IndexOf("ACCTLEASE3") >= 0 Then
                MA0004INProw("ACCTLEASE3") = XLSTBLrow("ACCTLEASE3")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL1") >= 0 Then
                MA0004INProw("ACCTLSUPL1") = XLSTBLrow("ACCTLSUPL1")
            End If

            If WW_COLUMNS.IndexOf("ACCTLSUPL4") >= 0 Then
                MA0004INProw("ACCTLSUPL4") = XLSTBLrow("ACCTLSUPL4")
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSNO") >= 0 Then
                MA0004INProw("CHEMTINSNO") = XLSTBLrow("CHEMTINSNO")
            End If

            If WW_COLUMNS.IndexOf("CHEMCELLNO") >= 0 Then
                MA0004INProw("CHEMCELLNO") = XLSTBLrow("CHEMCELLNO")
            End If

            If WW_COLUMNS.IndexOf("CHEMMATERIAL") >= 0 Then
                MA0004INProw("CHEMMATERIAL") = XLSTBLrow("CHEMMATERIAL")
            End If

            If WW_COLUMNS.IndexOf("CHEMDISGORGE") >= 0 Then
                MA0004INProw("CHEMDISGORGE") = XLSTBLrow("CHEMDISGORGE")
            End If

            If WW_COLUMNS.IndexOf("CHEMPMPDR") >= 0 Then
                MA0004INProw("CHEMPMPDR") = XLSTBLrow("CHEMPMPDR")
            End If

            If WW_COLUMNS.IndexOf("CHEMPUMP") >= 0 Then
                MA0004INProw("CHEMPUMP") = XLSTBLrow("CHEMPUMP")
            End If

            If WW_COLUMNS.IndexOf("CHEMINSSTAT") >= 0 Then
                MA0004INProw("CHEMINSSTAT") = XLSTBLrow("CHEMINSSTAT")
            End If

            If WW_COLUMNS.IndexOf("CHEMCELPART") >= 0 Then
                MA0004INProw("CHEMCELPART") = XLSTBLrow("CHEMCELPART")
            End If

            If WW_COLUMNS.IndexOf("CHEMSTRUCT") >= 0 Then
                MA0004INProw("CHEMSTRUCT") = XLSTBLrow("CHEMSTRUCT")
            End If

            If WW_COLUMNS.IndexOf("CHEMHOSE") >= 0 Then
                MA0004INProw("CHEMHOSE") = XLSTBLrow("CHEMHOSE")
            End If

            If WW_COLUMNS.IndexOf("CHEMPRESDRV") >= 0 Then
                MA0004INProw("CHEMPRESDRV") = XLSTBLrow("CHEMPRESDRV")
            End If

            If WW_COLUMNS.IndexOf("CHEMTHERM") >= 0 Then
                MA0004INProw("CHEMTHERM") = XLSTBLrow("CHEMTHERM")
            End If

            If WW_COLUMNS.IndexOf("CHEMINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMINSYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMINSYMD")
                    MA0004INProw("CHEMINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMMANOMTR") >= 0 Then
                MA0004INProw("CHEMMANOMTR") = XLSTBLrow("CHEMMANOMTR")
            End If

            If WW_COLUMNS.IndexOf("CHEMPRESEQ") >= 0 Then
                MA0004INProw("CHEMPRESEQ") = XLSTBLrow("CHEMPRESEQ")
            End If

            If WW_COLUMNS.IndexOf("FCTRSHFTNUM") >= 0 Then
                MA0004INProw("FCTRSHFTNUM") = XLSTBLrow("FCTRSHFTNUM")
            End If

            If WW_COLUMNS.IndexOf("FCTRFUELCAPA") >= 0 Then
                MA0004INProw("FCTRFUELCAPA") = XLSTBLrow("FCTRFUELCAPA")
            End If

            If WW_COLUMNS.IndexOf("FCTRSUSP") >= 0 Then
                MA0004INProw("FCTRSUSP") = XLSTBLrow("FCTRSUSP")
            End If

            If WW_COLUMNS.IndexOf("FCTRUREA") >= 0 Then
                MA0004INProw("FCTRUREA") = XLSTBLrow("FCTRUREA")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE1") >= 0 Then
                MA0004INProw("FCTRRESERVE1") = XLSTBLrow("FCTRRESERVE1")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE4") >= 0 Then
                MA0004INProw("FCTRRESERVE4") = XLSTBLrow("FCTRRESERVE4")
            End If

            If WW_COLUMNS.IndexOf("FCTRAXLE") >= 0 Then
                MA0004INProw("FCTRAXLE") = XLSTBLrow("FCTRAXLE")
            End If

            If WW_COLUMNS.IndexOf("FCTRFUELMATE") >= 0 Then
                MA0004INProw("FCTRFUELMATE") = XLSTBLrow("FCTRFUELMATE")
            End If

            If WW_COLUMNS.IndexOf("FCTRTIRE") >= 0 Then
                MA0004INProw("FCTRTIRE") = XLSTBLrow("FCTRTIRE")
            End If

            If WW_COLUMNS.IndexOf("FCTRDPR") >= 0 Then
                MA0004INProw("FCTRDPR") = XLSTBLrow("FCTRDPR")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE2") >= 0 Then
                MA0004INProw("FCTRRESERVE2") = XLSTBLrow("FCTRRESERVE2")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE5") >= 0 Then
                MA0004INProw("FCTRRESERVE5") = XLSTBLrow("FCTRRESERVE5")
            End If

            If WW_COLUMNS.IndexOf("FCTRTMISSION") >= 0 Then
                MA0004INProw("FCTRTMISSION") = XLSTBLrow("FCTRTMISSION")
            End If

            If WW_COLUMNS.IndexOf("FCTRRESERVE3") >= 0 Then
                MA0004INProw("FCTRRESERVE3") = XLSTBLrow("FCTRRESERVE3")
            End If

            If WW_COLUMNS.IndexOf("HPRSSERNO") >= 0 Then
                MA0004INProw("HPRSSERNO") = XLSTBLrow("HPRSSERNO")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSISTAT") >= 0 Then
                MA0004INProw("HPRSINSISTAT") = XLSTBLrow("HPRSINSISTAT")
            End If

            If WW_COLUMNS.IndexOf("HPRSSTRUCT") >= 0 Then
                MA0004INProw("HPRSSTRUCT") = XLSTBLrow("HPRSSTRUCT")
            End If

            If WW_COLUMNS.IndexOf("HPRSPIPE") >= 0 Then
                MA0004INProw("HPRSPIPE") = XLSTBLrow("HPRSPIPE")
            End If

            If WW_COLUMNS.IndexOf("HPRSPUMP") >= 0 Then
                MA0004INProw("HPRSPUMP") = XLSTBLrow("HPRSPUMP")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSIYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSIYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSIYMD")
                    MA0004INProw("HPRSINSIYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSMATR") >= 0 Then
                MA0004INProw("HPRSMATR") = XLSTBLrow("HPRSMATR")
            End If

            If WW_COLUMNS.IndexOf("HPRSPIPENUM") >= 0 Then
                MA0004INProw("HPRSPIPENUM") = XLSTBLrow("HPRSPIPENUM")
            End If

            If WW_COLUMNS.IndexOf("HPRSRESSRE") >= 0 Then
                MA0004INProw("HPRSRESSRE") = XLSTBLrow("HPRSRESSRE")
            End If

            If WW_COLUMNS.IndexOf("HPRSINSULATE") >= 0 Then
                MA0004INProw("HPRSINSULATE") = XLSTBLrow("HPRSINSULATE")
            End If

            If WW_COLUMNS.IndexOf("HPRSVALVE") >= 0 Then
                MA0004INProw("HPRSVALVE") = XLSTBLrow("HPRSVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTHRBMONITOR") >= 0 Then
                MA0004INProw("OTHRBMONITOR") = XLSTBLrow("OTHRBMONITOR")
            End If

            If WW_COLUMNS.IndexOf("OTHRDOCO") >= 0 Then
                MA0004INProw("OTHRDOCO") = XLSTBLrow("OTHRDOCO")
            End If

            If WW_COLUMNS.IndexOf("OTHRPAINTING") >= 0 Then
                MA0004INProw("OTHRPAINTING") = XLSTBLrow("OTHRPAINTING")
            End If

            If WW_COLUMNS.IndexOf("OTHRRTARGET") >= 0 Then
                MA0004INProw("OTHRRTARGET") = XLSTBLrow("OTHRRTARGET")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE1") >= 0 Then
                MA0004INProw("OFFCRESERVE1") = XLSTBLrow("OFFCRESERVE1")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE4") >= 0 Then
                MA0004INProw("OFFCRESERVE4") = XLSTBLrow("OFFCRESERVE4")
            End If

            If WW_COLUMNS.IndexOf("OTHRBSONAR") >= 0 Then
                MA0004INProw("OTHRBSONAR") = XLSTBLrow("OTHRBSONAR")
            End If

            If WW_COLUMNS.IndexOf("OTHRDRRECORD") >= 0 Then
                MA0004INProw("OTHRDRRECORD") = XLSTBLrow("OTHRDRRECORD")
            End If

            If WW_COLUMNS.IndexOf("OTHRRADIOCON") >= 0 Then
                MA0004INProw("OTHRRADIOCON") = XLSTBLrow("OTHRRADIOCON")
            End If

            If WW_COLUMNS.IndexOf("OTHRTERMINAL") >= 0 Then
                MA0004INProw("OTHRTERMINAL") = XLSTBLrow("OTHRTERMINAL")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE2") >= 0 Then
                MA0004INProw("OFFCRESERVE2") = XLSTBLrow("OFFCRESERVE2")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE5") >= 0 Then
                MA0004INProw("OFFCRESERVE5") = XLSTBLrow("OFFCRESERVE5")
            End If

            If WW_COLUMNS.IndexOf("OFFCRESERVE3") >= 0 Then
                MA0004INProw("OFFCRESERVE3") = XLSTBLrow("OFFCRESERVE3")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSNO") >= 0 Then
                MA0004INProw("OTNKTINSNO") = XLSTBLrow("OTNKTINSNO")
            End If

            If WW_COLUMNS.IndexOf("OTNKCELLNO") >= 0 Then
                MA0004INProw("OTNKCELLNO") = XLSTBLrow("OTNKCELLNO")
            End If

            If WW_COLUMNS.IndexOf("OTNKMATERIAL") >= 0 Then
                MA0004INProw("OTNKMATERIAL") = XLSTBLrow("OTNKMATERIAL")
            End If

            If WW_COLUMNS.IndexOf("OTNKPIPE") >= 0 Then
                MA0004INProw("OTNKPIPE") = XLSTBLrow("OTNKPIPE")
            End If

            If WW_COLUMNS.IndexOf("OTNKBPIPE") >= 0 Then
                MA0004INProw("OTNKBPIPE") = XLSTBLrow("OTNKBPIPE")
            End If

            If WW_COLUMNS.IndexOf("OTNKDISGORGE") >= 0 Then
                MA0004INProw("OTNKDISGORGE") = XLSTBLrow("OTNKDISGORGE")
            End If

            If WW_COLUMNS.IndexOf("OTNKDCD") >= 0 Then
                MA0004INProw("OTNKDCD") = XLSTBLrow("OTNKDCD")
            End If

            If WW_COLUMNS.IndexOf("OTNKINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKINSYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKINSYMD")
                    MA0004INProw("OTNKINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("OTNKCELPART") >= 0 Then
                MA0004INProw("OTNKCELPART") = XLSTBLrow("OTNKCELPART")
            End If

            If WW_COLUMNS.IndexOf("OTNKPIPESIZE") >= 0 Then
                MA0004INProw("OTNKPIPESIZE") = XLSTBLrow("OTNKPIPESIZE")
            End If

            If WW_COLUMNS.IndexOf("OTNKCVALVE") >= 0 Then
                MA0004INProw("OTNKCVALVE") = XLSTBLrow("OTNKCVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTNKLVALVE") >= 0 Then
                MA0004INProw("OTNKLVALVE") = XLSTBLrow("OTNKLVALVE")
            End If

            If WW_COLUMNS.IndexOf("OTNKINSSTAT") >= 0 Then
                MA0004INProw("OTNKINSSTAT") = XLSTBLrow("OTNKINSSTAT")
            End If

            If WW_COLUMNS.IndexOf("OTNKPUMP") >= 0 Then
                MA0004INProw("OTNKPUMP") = XLSTBLrow("OTNKPUMP")
            End If

            If WW_COLUMNS.IndexOf("OTNKDETECTOR") >= 0 Then
                MA0004INProw("OTNKDETECTOR") = XLSTBLrow("OTNKDETECTOR")
            End If

            If WW_COLUMNS.IndexOf("OTNKVAPOR") >= 0 Then
                MA0004INProw("OTNKVAPOR") = XLSTBLrow("OTNKVAPOR")
            End If

            If WW_COLUMNS.IndexOf("OTNKHTECH") >= 0 Then
                MA0004INProw("OTNKHTECH") = XLSTBLrow("OTNKHTECH")
            End If

            If WW_COLUMNS.IndexOf("TAXATAX") >= 0 Then
                If XLSTBLrow("TAXATAX") <> "" Then
                    MA0004INProw("TAXATAX") = XLSTBLrow("TAXATAX")
                End If
            End If

            If WW_COLUMNS.IndexOf("TAXLINS") >= 0 Then
                If XLSTBLrow("TAXLINS") <> "" Then
                    MA0004INProw("TAXLINS") = XLSTBLrow("TAXLINS")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNYMD") >= 0 Then
                If IsDate(XLSTBLrow("LICNYMD")) Then
                    WW_DATE = XLSTBLrow("LICNYMD")
                    MA0004INProw("LICNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNPLTNO1") >= 0 Then
                MA0004INProw("LICNPLTNO1") = XLSTBLrow("LICNPLTNO1")
            End If

            If WW_COLUMNS.IndexOf("LICNFRAMENO") >= 0 Then
                MA0004INProw("LICNFRAMENO") = XLSTBLrow("LICNFRAMENO")
            End If

            If WW_COLUMNS.IndexOf("LICNMODEL") >= 0 Then
                MA0004INProw("LICNMODEL") = XLSTBLrow("LICNMODEL")
            End If

            If WW_COLUMNS.IndexOf("LICNLDCAPA") >= 0 Then
                MA0004INProw("LICNLDCAPA") = XLSTBLrow("LICNLDCAPA")
            End If

            If WW_COLUMNS.IndexOf("LICN5LDCAPA") >= 0 Then
                MA0004INProw("LICN5LDCAPA") = XLSTBLrow("LICN5LDCAPA")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKTINSYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKTINSYMD")
                    MA0004INProw("OTNKTINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMTINSYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMTINSYMD")
                    MA0004INProw("CHEMTINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSYMD")
                    MA0004INProw("HPRSINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("TAXVTAX") >= 0 Then
                If XLSTBLrow("TAXVTAX") <> "" Then
                    MA0004INProw("TAXVTAX") = XLSTBLrow("TAXVTAX")
                End If
            End If

            If WW_COLUMNS.IndexOf("TAXLINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("TAXLINSYMD")) Then
                    WW_DATE = XLSTBLrow("TAXLINSYMD")
                    MA0004INProw("TAXLINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNPLTNO2") >= 0 Then
                MA0004INProw("LICNPLTNO2") = XLSTBLrow("LICNPLTNO2")
            End If

            If WW_COLUMNS.IndexOf("LICNMNFACT") >= 0 Then
                MA0004INProw("LICNMNFACT") = XLSTBLrow("LICNMNFACT")
            End If

            If WW_COLUMNS.IndexOf("LICNMOTOR") >= 0 Then
                MA0004INProw("LICNMOTOR") = XLSTBLrow("LICNMOTOR")
            End If

            If WW_COLUMNS.IndexOf("LICNTWEIGHT") >= 0 Then
                MA0004INProw("LICNTWEIGHT") = XLSTBLrow("LICNTWEIGHT")
            End If

            If WW_COLUMNS.IndexOf("LICNCWEIGHT") >= 0 Then
                MA0004INProw("LICNCWEIGHT") = XLSTBLrow("LICNCWEIGHT")
            End If

            If WW_COLUMNS.IndexOf("OTNKTINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("OTNKTINSNYMD")) Then
                    WW_DATE = XLSTBLrow("OTNKTINSNYMD")
                    MA0004INProw("OTNKTINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("CHEMTINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("CHEMTINSNYMD")) Then
                    WW_DATE = XLSTBLrow("CHEMTINSNYMD")
                    MA0004INProw("CHEMTINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSINSNYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSINSNYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSINSNYMD")
                    MA0004INProw("HPRSINSNYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("LICNWEIGHT") >= 0 Then
                If XLSTBLrow("LICNWEIGHT") <> "" Then
                    MA0004INProw("LICNWEIGHT") = XLSTBLrow("LICNWEIGHT")
                End If
            End If

            If WW_COLUMNS.IndexOf("HPRSJINSYMD") >= 0 Then
                If IsDate(XLSTBLrow("HPRSJINSYMD")) Then
                    WW_DATE = XLSTBLrow("HPRSJINSYMD")
                    MA0004INProw("HPRSJINSYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("MANGPROD1") >= 0 Then
                MA0004INProw("MANGPROD1") = XLSTBLrow("MANGPROD1")
            End If

            If WW_COLUMNS.IndexOf("MANGPROD2") >= 0 Then
                MA0004INProw("MANGPROD2") = XLSTBLrow("MANGPROD2")
            End If

            If WW_COLUMNS.IndexOf("FCTRSMAKER") >= 0 Then
                MA0004INProw("FCTRSMAKER") = XLSTBLrow("FCTRSMAKER")
            End If

            If WW_COLUMNS.IndexOf("FCTRTMAKER") >= 0 Then
                MA0004INProw("FCTRTMAKER") = XLSTBLrow("FCTRTMAKER")
            End If

            If WW_COLUMNS.IndexOf("OTNKEXHASIZE") >= 0 Then
                MA0004INProw("OTNKEXHASIZE") = XLSTBLrow("OTNKEXHASIZE")
            End If

            If WW_COLUMNS.IndexOf("HPRSPMPDR") >= 0 Then
                MA0004INProw("HPRSPMPDR") = XLSTBLrow("HPRSPMPDR")
            End If

            If WW_COLUMNS.IndexOf("HPRSHOSE") >= 0 Then
                MA0004INProw("HPRSHOSE") = XLSTBLrow("HPRSHOSE")
            End If

            If WW_COLUMNS.IndexOf("CONTSHAPE") >= 0 Then
                MA0004INProw("CONTSHAPE") = XLSTBLrow("CONTSHAPE")
            End If

            If WW_COLUMNS.IndexOf("CONTPUMP") >= 0 Then
                MA0004INProw("CONTPUMP") = XLSTBLrow("CONTPUMP")
            End If

            If WW_COLUMNS.IndexOf("CONTPMPDR") >= 0 Then
                MA0004INProw("CONTPMPDR") = XLSTBLrow("CONTPMPDR")
            End If

            If WW_COLUMNS.IndexOf("OTHRTIRE1") >= 0 Then
                MA0004INProw("OTHRTIRE1") = XLSTBLrow("OTHRTIRE1")
            End If

            If WW_COLUMNS.IndexOf("OTHRTIRE2") >= 0 Then
                MA0004INProw("OTHRTIRE2") = XLSTBLrow("OTHRTIRE2")
            End If

            If WW_COLUMNS.IndexOf("OTHRTPMS") >= 0 Then
                MA0004INProw("OTHRTPMS") = XLSTBLrow("OTHRTPMS")
            End If

            If WW_COLUMNS.IndexOf("OTNKTMAKER") >= 0 Then
                MA0004INProw("OTNKTMAKER") = XLSTBLrow("OTNKTMAKER")
            End If

            If WW_COLUMNS.IndexOf("HPRSTMAKER") >= 0 Then
                MA0004INProw("HPRSTMAKER") = XLSTBLrow("HPRSTMAKER")
            End If

            If WW_COLUMNS.IndexOf("CHEMTMAKER") >= 0 Then
                MA0004INProw("CHEMTMAKER") = XLSTBLrow("CHEMTMAKER")
            End If

            If WW_COLUMNS.IndexOf("CONTTMAKER") >= 0 Then
                MA0004INProw("CONTTMAKER") = XLSTBLrow("CONTTMAKER")
            End If

            If WW_COLUMNS.IndexOf("SHARYOSTATUS") >= 0 Then
                MA0004INProw("SHARYOSTATUS") = XLSTBLrow("SHARYOSTATUS")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO1") >= 0 Then
                MA0004INProw("SHARYOINFO1") = XLSTBLrow("SHARYOINFO1")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO2") >= 0 Then
                MA0004INProw("SHARYOINFO2") = XLSTBLrow("SHARYOINFO2")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO3") >= 0 Then
                MA0004INProw("SHARYOINFO3") = XLSTBLrow("SHARYOINFO3")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO4") >= 0 Then
                MA0004INProw("SHARYOINFO4") = XLSTBLrow("SHARYOINFO4")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO5") >= 0 Then
                MA0004INProw("SHARYOINFO5") = XLSTBLrow("SHARYOINFO5")
            End If

            If WW_COLUMNS.IndexOf("SHARYOINFO6") >= 0 Then
                MA0004INProw("SHARYOINFO6") = XLSTBLrow("SHARYOINFO6")
            End If

            If WW_COLUMNS.IndexOf("INSKBN") >= 0 Then
                MA0004INProw("INSKBN") = XLSTBLrow("INSKBN")
            End If

            '名称付与
            CODENAME_get("BASELEASE", MA0004INProw("BASELEASE"), MA0004INProw("BASELEASENAME"), WW_DUMMY)
            CODENAME_get("CHEMDISGORGE", MA0004INProw("CHEMDISGORGE"), MA0004INProw("CHEMDISGORGENAME"), WW_DUMMY)
            CODENAME_get("CHEMHOSE", MA0004INProw("CHEMHOSE"), MA0004INProw("CHEMHOSENAME"), WW_DUMMY)
            CODENAME_get("CHEMMANOMTR", MA0004INProw("CHEMMANOMTR"), MA0004INProw("CHEMMANOMTRNAME"), WW_DUMMY)
            CODENAME_get("CHEMMATERIAL", MA0004INProw("CHEMMATERIAL"), MA0004INProw("CHEMMATERIALNAME"), WW_DUMMY)
            CODENAME_get("CHEMPMPDR", MA0004INProw("CHEMPMPDR"), MA0004INProw("CHEMPMPDRNAME"), WW_DUMMY)
            CODENAME_get("CHEMPRESDRV", MA0004INProw("CHEMPRESDRV"), MA0004INProw("CHEMPRESDRVNAME"), WW_DUMMY)
            CODENAME_get("CHEMPRESEQ", MA0004INProw("CHEMPRESEQ"), MA0004INProw("CHEMPRESEQNAME"), WW_DUMMY)
            CODENAME_get("CHEMPUMP", MA0004INProw("CHEMPUMP"), MA0004INProw("CHEMPUMPNAME"), WW_DUMMY)
            CODENAME_get("CHEMSTRUCT", MA0004INProw("CHEMSTRUCT"), MA0004INProw("CHEMSTRUCTNAME"), WW_DUMMY)
            CODENAME_get("CHEMTHERM", MA0004INProw("CHEMTHERM"), MA0004INProw("CHEMTHERMNAME"), WW_DUMMY)
            CODENAME_get("FCTRAXLE", MA0004INProw("FCTRAXLE"), MA0004INProw("FCTRAXLENAME"), WW_DUMMY)
            CODENAME_get("FCTRDPR", MA0004INProw("FCTRDPR"), MA0004INProw("FCTRDPRNAME"), WW_DUMMY)
            CODENAME_get("FCTRFUELMATE", MA0004INProw("FCTRFUELMATE"), MA0004INProw("FCTRFUELMATENAME"), WW_DUMMY)
            CODENAME_get("FCTRSHFTNUM", MA0004INProw("FCTRSHFTNUM"), MA0004INProw("FCTRSHFTNUMNAME"), WW_DUMMY)
            CODENAME_get("FCTRSUSP", MA0004INProw("FCTRSUSP"), MA0004INProw("FCTRSUSPNAME"), WW_DUMMY)
            CODENAME_get("FCTRTMISSION", MA0004INProw("FCTRTMISSION"), MA0004INProw("FCTRTMISSIONNAME"), WW_DUMMY)
            CODENAME_get("FCTRUREA", MA0004INProw("FCTRUREA"), MA0004INProw("FCTRUREANAME"), WW_DUMMY)
            CODENAME_get("HPRSINSULATE", MA0004INProw("HPRSINSULATE"), MA0004INProw("HPRSINSULATENAME"), WW_DUMMY)
            CODENAME_get("HPRSMATR", MA0004INProw("HPRSMATR"), MA0004INProw("HPRSMATRNAME"), WW_DUMMY)
            CODENAME_get("HPRSPIPE", MA0004INProw("HPRSPIPE"), MA0004INProw("HPRSPIPENAME"), WW_DUMMY)
            CODENAME_get("HPRSPIPENUM", MA0004INProw("HPRSPIPENUM"), MA0004INProw("HPRSPIPENUMNAME"), WW_DUMMY)
            CODENAME_get("HPRSPUMP", MA0004INProw("HPRSPUMP"), MA0004INProw("HPRSPUMPNAME"), WW_DUMMY)
            CODENAME_get("HPRSRESSRE", MA0004INProw("HPRSRESSRE"), MA0004INProw("HPRSRESSRENAME"), WW_DUMMY)
            CODENAME_get("HPRSSTRUCT", MA0004INProw("HPRSSTRUCT"), MA0004INProw("HPRSSTRUCTNAME"), WW_DUMMY)
            CODENAME_get("HPRSVALVE", MA0004INProw("HPRSVALVE"), MA0004INProw("HPRSVALVENAME"), WW_DUMMY)
            CODENAME_get("MANGMORG", MA0004INProw("MANGMORG"), MA0004INProw("MANGMORGNAME"), WW_DUMMY)
            CODENAME_get("MANGOILTYPE", MA0004INProw("MANGOILTYPE"), MA0004INProw("MANGOILTYPENAME"), WW_DUMMY)
            CODENAME_get("MANGOWNCODE", MA0004INProw("MANGOWNCODE"), MA0004INProw("MANGOWNCODENAME"), WW_DUMMY)
            CODENAME_get("MANGOWNCONT", MA0004INProw("MANGOWNCONT"), MA0004INProw("MANGOWNCONTNAME"), WW_DUMMY)
            CODENAME_get("MANGSORG", MA0004INProw("MANGSORG"), MA0004INProw("MANGSORGNAME"), WW_DUMMY)
            CODENAME_get("MANGSUPPL", MA0004INProw("MANGSUPPL"), MA0004INProw("MANGSUPPLNAME"), WW_DUMMY)
            CODENAME_get("MANGUORG", MA0004INProw("MANGUORG"), MA0004INProw("MANGUORGNAME"), WW_DUMMY)
            CODENAME_get("NOTES", MA0004INProw("NOTES"), MA0004INProw("NOTES"), WW_DUMMY)
            CODENAME_get("OTHRBMONITOR", MA0004INProw("OTHRBMONITOR"), MA0004INProw("OTHRBMONITORNAME"), WW_DUMMY)
            CODENAME_get("OTHRBSONAR", MA0004INProw("OTHRBSONAR"), MA0004INProw("OTHRBSONARNAME"), WW_DUMMY)
            CODENAME_get("FCTRTIRE", MA0004INProw("FCTRTIRE"), MA0004INProw("FCTRTIRENAME"), WW_DUMMY)
            CODENAME_get("OTHRDRRECORD", MA0004INProw("OTHRDRRECORD"), MA0004INProw("OTHRDRRECORDNAME"), WW_DUMMY)
            CODENAME_get("OTHRPAINTING", MA0004INProw("OTHRPAINTING"), MA0004INProw("OTHRPAINTINGNAME"), WW_DUMMY)
            CODENAME_get("OTHRRADIOCON", MA0004INProw("OTHRRADIOCON"), MA0004INProw("OTHRRADIOCONNAME"), WW_DUMMY)
            CODENAME_get("OTHRRTARGET", MA0004INProw("OTHRRTARGET"), MA0004INProw("OTHRRTARGETNAME"), WW_DUMMY)
            CODENAME_get("OTHRTERMINAL", MA0004INProw("OTHRTERMINAL"), MA0004INProw("OTHRTERMINALNAME"), WW_DUMMY)
            CODENAME_get("OTNKBPIPE", MA0004INProw("OTNKBPIPE"), MA0004INProw("OTNKBPIPENAME"), WW_DUMMY)
            CODENAME_get("OTNKCVALVE", MA0004INProw("OTNKCVALVE"), MA0004INProw("OTNKCVALVENAME"), WW_DUMMY)
            CODENAME_get("OTNKDCD", MA0004INProw("OTNKDCD"), MA0004INProw("OTNKDCDNAME"), WW_DUMMY)
            CODENAME_get("OTNKDETECTOR", MA0004INProw("OTNKDETECTOR"), MA0004INProw("OTNKDETECTORNAME"), WW_DUMMY)
            CODENAME_get("OTNKDISGORGE", MA0004INProw("OTNKDISGORGE"), MA0004INProw("OTNKDISGORGENAME"), WW_DUMMY)
            CODENAME_get("OTNKHTECH", MA0004INProw("OTNKHTECH"), MA0004INProw("OTNKHTECHNAME"), WW_DUMMY)
            CODENAME_get("OTNKLVALVE", MA0004INProw("OTNKLVALVE"), MA0004INProw("OTNKLVALVENAME"), WW_DUMMY)
            CODENAME_get("OTNKMATERIAL", MA0004INProw("OTNKMATERIAL"), MA0004INProw("OTNKMATERIALNAME"), WW_DUMMY)
            CODENAME_get("OTNKPIPE", MA0004INProw("OTNKPIPE"), MA0004INProw("OTNKPIPENAME"), WW_DUMMY)
            CODENAME_get("OTNKPIPESIZE", MA0004INProw("OTNKPIPESIZE"), MA0004INProw("OTNKPIPESIZENAME"), WW_DUMMY)
            CODENAME_get("OTNKPUMP", MA0004INProw("OTNKPUMP"), MA0004INProw("OTNKPUMPNAME"), WW_DUMMY)
            CODENAME_get("OTNKVAPOR", MA0004INProw("OTNKVAPOR"), MA0004INProw("OTNKVAPORNAME"), WW_DUMMY)
            CODENAME_get("MANGPROD1", MA0004INProw("MANGPROD1"), MA0004INProw("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0004INProw("MANGOILTYPE"))})
            CODENAME_get("MANGPROD2", MA0004INProw("MANGPROD2"), MA0004INProw("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0004INProw("MANGOILTYPE")), CStr(MA0004INProw("MANGPROD1"))})
            CODENAME_get("FCTRSMAKER", MA0004INProw("FCTRSMAKER"), MA0004INProw("FCTRSMAKERNAME"), WW_DUMMY)
            CODENAME_get("OTNKEXHASIZE", MA0004INProw("OTNKEXHASIZE"), MA0004INProw("OTNKEXHASIZENAME"), WW_DUMMY)
            CODENAME_get("HPRSPMPDR", MA0004INProw("HPRSPMPDR"), MA0004INProw("HPRSPMPDRNAME"), WW_DUMMY)
            CODENAME_get("HPRSHOSE", MA0004INProw("HPRSHOSE"), MA0004INProw("HPRSHOSENAME"), WW_DUMMY)
            CODENAME_get("CONTSHAPE", MA0004INProw("CONTSHAPE"), MA0004INProw("CONTSHAPENAME"), WW_DUMMY)
            CODENAME_get("CONTPUMP", MA0004INProw("CONTPUMP"), MA0004INProw("CONTPUMPNAME"), WW_DUMMY)
            CODENAME_get("CONTPMPDR", MA0004INProw("CONTPMPDR"), MA0004INProw("CONTPMPDRNAME"), WW_DUMMY)
            CODENAME_get("OTHRTPMS", MA0004INProw("OTHRTPMS"), MA0004INProw("OTHRTPMSNAME"), WW_DUMMY)
            CODENAME_get("OTNKTMAKER", MA0004INProw("OTNKTMAKER"), MA0004INProw("OTNKTMAKERNAME"), WW_DUMMY)
            CODENAME_get("HPRSTMAKER", MA0004INProw("HPRSTMAKER"), MA0004INProw("HPRSTMAKERNAME"), WW_DUMMY)
            CODENAME_get("CHEMTMAKER", MA0004INProw("CHEMTMAKER"), MA0004INProw("CHEMTMAKERNAME"), WW_DUMMY)
            CODENAME_get("CONTTMAKER", MA0004INProw("CONTTMAKER"), MA0004INProw("CONTTMAKERNAME"), WW_DUMMY)
            CODENAME_get("SHARYOSTATUS", MA0004INProw("SHARYOSTATUS"), MA0004INProw("SHARYOSTATUSNAME"), WW_DUMMY)
            CODENAME_get("INSKBN", MA0004INProw("INSKBN"), MA0004INProw("INSKBNNAME"), WW_DUMMY)

            MA0004INProw("STYMD_A") = ""
            MA0004INProw("STYMD_B") = ""
            MA0004INProw("STYMD_C") = ""
            MA0004INProw("STYMD_S") = ""
            MA0004INProw("ENDYMD_A") = ""
            MA0004INProw("ENDYMD_B") = ""
            MA0004INProw("ENDYMD_C") = ""
            MA0004INProw("ENDYMD_S") = ""
            MA0004INProw("WORK_NO") = 0

            MA0004INPtbl.Rows.Add(MA0004INProw)
        Next

        '○項目チェック
        INPUT_CHEK(WW_ERRCODE)

        '○入力値テーブル反映(MA0004INPtbl⇒MA0004tbl)
        TBL_UPD("EXCEL", WW_ERRCODE)

        '○PDF保管用ディレクトリ作成
        For Each MA0004INProw As DataRow In MA0004INPtbl.Rows
            If MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                PDF_INITread(MA0004INProw("CAMPCODE"), MA0004INProw("SHARYOTYPE"), MA0004INProw("TSHABAN"), MA0004INProw("STYMD"))
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(MA0004tbl)
        If Not isNormal(WW_RTN_SW) Then
            Exit Sub
        End If

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
        CS0023XLSUPLOAD.TBLDATA.Clear()
        CS0023XLSUPLOAD.TBLDATA = Nothing

    End Sub

    ' ******************************************************************************
    ' ***  詳細画面-PDFファイル操作関連                                          *** 
    ' ******************************************************************************

    ''' <summary>
    '''  PDF Tempディレクトリ削除
    ''' </summary>
    ''' <remarks>PAGE_load時</remarks>
    Protected Sub PDF_INITdel()
        Dim WW_UPdirs As String()
        Dim WW_UPfiles As String()

        '○PDF格納Dir作成
        '   正式登録のPDFフォルダ
        '       c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_01   　　　       　　　　 (01:PDF車検証)
        '       c:\appl\applpdf\MA0004_SHARYOC\統一車番_nn        　　　       　　　　 (nn:PDF書類種類)
        '   一時保存のPDFフォルダ
        '       c:\appll\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_01\Update_H (01:PDF車検証)
        '       c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H       (nn:PDF書類種類)
        '       c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_01\Delete_D  (01:PDF車検証)
        '       c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Delete_D       (nn:PDF書類種類)

        'Temp納ディレクトリ編集
        Dim WW_Dir As String = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\"

        'PDF格納ディレクトリ＞MA0004_SHARYOC\Temp\ユーザIDフォルダ内のファイル削除
        Dim WW_Dir_del As New List(Of String)
        '全統一車番に対し処理

        'Tempの自ユーザ内フォルダを取得
        WW_UPdirs = Directory.GetDirectories(WW_Dir, "*", SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPdirs
            If InStr(tempFile, "\" & CS0050Session.USERID) > 0 AndAlso InStr(tempFile, "\Temp") > 0 Then
                WW_Dir_del.Add(tempFile)
            End If
        Next

        'Listを降順に並べる⇒下位ディレクトリが先頭となる
        WW_Dir_del.Reverse()

        For i As Integer = 0 To WW_Dir_del.Count - 1
            'フォルダー内ファイル削除
            WW_UPfiles = Directory.GetFiles(WW_Dir_del.Item(i), "*", SearchOption.AllDirectories)
            'フォルダー内ファイル削除
            For Each tempFile As String In WW_UPfiles
                'ファイル削除
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                    '読み取り専用などは削除できない
                End Try
            Next

            Try
                'ファイル削除
                Directory.Delete(WW_Dir_del.Item(i))
            Catch ex As Exception
                'ファイルが残っている場合、削除できない
            End Try
        Next

    End Sub

    ''' <summary>
    ''' PDF読み込み,ディレクトリ作成(Header・一覧ダブルクリック時)
    ''' </summary>
    ''' <param name="I_CAMPCODE"></param>
    ''' <param name="I_SHARYOTYPE"></param>
    ''' <param name="I_TSHABAN"></param>
    ''' <param name="I_STYMD"></param>
    ''' <remarks></remarks>
    Protected Sub PDF_INITread(ByVal I_CAMPCODE As String, ByVal I_SHARYOTYPE As String, ByVal I_TSHABAN As String, ByVal I_STYMD As String)
        Dim WW_UPfiles As String()

        '(説明1) PDF保持について
        '　①年度単位に保持(有効期限別には保持しない)

        '(説明2) フォルダ説明
        '　①一覧明細選択～表追加直前のPDF操作内容：Temp\Update_Dフォルダに格納
        '　②表追加によるPDF一時保存内容　　　　　：Temp\Update_Hフォルダに格納   …　Gridへの表追加結果
        '　③正式PDF登録内容　　　　　　　　　　　：正式PDFフォルダ

        '(説明3) イベント別処理内容　　…　処理効率は悪いが、操作がシンプルとなる為、下記処理とした。
        '　①Page_Load時：PDF_INITdel
        '　　　　・Tempフォルダ(Update_D・Update_H)をお掃除
        '　②一覧ダブルクリック時：PDF_INITread
        '　　　　・Update_Hが存在しない場合、Update_Hフォルダ作成＆正式フォルダ内全PDF→Update_Hフォルダへコピー
        '　　　　　注意１…PDF明細選択01～12全てを対象
        '　　　　・Update_Dが存在する場合、Update_Dフォルダ内PDFを全て削除　＆　Update_Dフォルダ削除
        '　　　　　注意１…PDF明細選択01～12全てを対象
        '　　　　・Update_Dフォルダ作成 ＆ Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
        '　　　　　注意１…PDF明細選択01～12全てを対象
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　③Detail操作（PDF表示選択切替）時：PDF_SELECTchange
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　④Detail操作（クリアボタン押下）時：WF_CLEAR_Click
        '　　　　・クリア処理（Update_Dクリア）＆明細クリア表示
        '　⑤Detail操作（表追加ボタン押下）時：PDF_SAVE_H
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・Update_Hフォルダ内容をクリア（PDF明細選択01～12全てを対象)
        '　　　　・Update_Dフォルダ内PDFをUpdate_Hフォルダに全てコピー（PDF明細選択01～12全てを対象)
        '　　　　・Update_Dフォルダ内PDFを全て削除（PDF明細選択01～12全てを対象)
        '　⑥PDFアップロード時：UPLOAD_PDF
        '　　　　・Update_Dフォルダに該当PDFを格納
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　⑦DB更新ボタン押下時：★★★
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_Dをお掃除(Update_Hフォルダは連続入力に備えクリアしない)
        '　⑧Detail操作（有効開始変更)時：PDF_INITread

        '○年度算出(有効開始年月日を基準として算出)
        Dim WW_nendo As String
        Dim WW_date As Date
        Dim WW_Dir As String
        Dim WW_TempDir As String

        Try
            Date.TryParse(I_STYMD, WW_date)
        Catch ex As Exception
            WW_date = C_DEFAULT_YMD
        End Try

        If I_STYMD = "" OrElse WW_date <= C_DEFAULT_YMD Then
            Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "WF_STYMD")
            Exit Sub
        End If

        If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
            WW_nendo = (WW_date.Year - 1).ToString()
        Else
            WW_nendo = (WW_date.Year).ToString()
        End If

        '○事前チェック
        '統一車番の存在確認（一覧に存在する事）
        If I_CAMPCODE = "" OrElse I_SHARYOTYPE = "" OrElse I_TSHABAN = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        Else
            For i As Integer = 0 To MA0004tbl.Rows.Count - 1
                If I_SHARYOTYPE = MA0004tbl.Rows(i)("SHARYOTYPE") OrElse I_TSHABAN = MA0004tbl.Rows(i)("TSHABAN") Then
                    Exit For
                Else
                    If (i - 1) >= MA0004tbl.Rows.Count Then
                        Master.output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "統一車番")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○フォルダ作成　＆　ファイルコピー
        'PDF格納Dir作成
        '   正式登録のPDFフォルダ
        '       c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn   　　　       　　　　 (nn:PDF書類種類)
        '   一時保存のPDFフォルダ
        '       c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn\Temp\ユーザID\Update_H  (nn:PDF書類種類)
        '       c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn\Temp\ユーザID\Delete_D  (nn:PDF書類種類)
        'ファイルコピー
        '　　正式登録のPDFフォルダ　⇒　一時保存のPDFフォルダ


        '○正式格納ディレクトリ作成
        For i As Integer = 1 To 12
            If i = 1 Then
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH
                WW_Dir = WW_Dir & "\MA0004_SHARYOC\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH
                WW_Dir = WW_Dir & "\MA0004_SHARYOC\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00")
            End If

            If Not Directory.Exists(WW_Dir) Then
                Directory.CreateDirectory(WW_Dir)
            End If
        Next

        '○一時保存ディレクトリ作成
        'PDF格納一時保存ディレクトリ１編集    c:\appl\applpdf\MA0004_SHARYOC\Temp
        WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp"

        'ディレクトリ作成
        If Not Directory.Exists(WW_TempDir) Then
            Directory.CreateDirectory(WW_TempDir)
        End If

        'PDF格納一時保存ディレクトリ２編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID
        WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID

        'ディレクトリ作成
        If Not Directory.Exists(WW_TempDir) Then
            Directory.CreateDirectory(WW_TempDir)
        End If

        '○一時保存ディレクトリ内統一車番ディレクトリ作成
        'PDF格納一時保存ディレクトリ３編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn
        For i As Integer = 1 To 12
            If i = 1 Then
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00")
            End If

            'ディレクトリ作成
            If Not Directory.Exists(WW_TempDir) Then
                Directory.CreateDirectory(WW_TempDir)
            End If
        Next

        '○作業用(一覧格納)ディレクトリ作成
        'PDF格納一時保存ディレクトリ４編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
        For i As Integer = 1 To 12
            If i = 1 Then
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00") & "\Update_H"
            End If

            'ディレクトリ作成
            If Not Directory.Exists(WW_TempDir) Then
                Directory.CreateDirectory(WW_TempDir)
            End If
        Next

        '○ファイルコピー処理　…　正式格納ディレクトリ ---> 作業用(一覧格納：Update_H)ディレクトリ
        For i As Integer = 1 To 12
            '(コピー元Dir)
            If i = 1 Then
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\"
                WW_Dir = WW_Dir & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\"
                WW_Dir = WW_Dir & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00")
            End If

            '(コピー先Dir)
            If i = 1 Then
                'PDF格納一時保存ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                'PDF格納一時保存ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00") & "\Update_H"
            End If

            '正式フォルダ内Dirに対するコピー処理
            WW_UPfiles = Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If
                Loop Until InStr(WW_File, "\") <= 0

                '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
                File.Copy(tempFile, WW_TempDir & "\" & WW_File, True)
            Next
        Next

        '○作業用(詳細格納)ディレクトリ作成
        'PDF格納一時保存ディレクトリ５編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
        For i As Integer = 1 To 12
            If i = 1 Then
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_D"
            Else
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00") & "\Update_D"
            End If

            If Directory.Exists(WW_TempDir) Then
                'Update_Dが存在する場合、Update_Dフォルダ内PDFを全て削除（前回処理内容をクリア）
                WW_UPfiles = Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)
                For Each tempFile As String In WW_UPfiles
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            Else
                Directory.CreateDirectory(WW_TempDir)
            End If
        Next

        '○ファイルコピー処理　…　作業用(一覧格納：Update_H)ディレクトリ ---> 作業用(詳細格納：Update_D)ディレクトリ
        'Update_Hディレクトリ ---> Update_Dディレクトリ へファイルコピー処理
        For i As Integer = 1 To 12
            '(コピー元Dir)
            If i = 1 Then
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_Dir = WW_Dir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_Dir = WW_Dir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00") & "\Update_H"
            End If

            '(コピー先Dir)
            If i = 1 Then
                'PDF格納一時保存ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_D"
            Else
                'PDF格納一時保存ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & i.ToString("00") & "\Update_D"
            End If

            'Update_Dフォルダへファイルコピー処理
            WW_UPfiles = Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If
                Loop Until InStr(WW_File, "\") <= 0

                'Update_Hフォルダ内PDF→Update_Dフォルダへ上書コピー　　…　連続処理の場合を想定
                File.Copy(tempFile, WW_TempDir & "\" & WW_File, True)
            Next
        Next


        '○画面編集
        If WF_Rep11_PDFselect.SelectedValue.ToString() = "" Then
            WF_Rep11_PDFselect.SelectedIndex = 0
        End If

        'PDF格納一時保存ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
        If WF_Rep11_PDFselect.SelectedValue.ToString() = 1 Then
            WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WW_nendo & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        Else
            WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_TempDir = WW_TempDir & "\" & I_SHARYOTYPE & I_TSHABAN & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        End If

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        'Update_Dディレクトリ内のファイル一覧でListBox作成
        WW_UPfiles = Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)

        For Each tempFile As String In WW_UPfiles
            If Right(tempFile, 4) = ".pdf" OrElse Right(tempFile, 4) = ".PDF" Then
                Dim WW_tempFile As String = tempFile
                Do
                    'ディレクトリ文字を含む場合、ディレクトリ文字を削除
                    If InStr(WW_tempFile, "\") > 0 Then
                        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                    End If

                    'ディレクトリ文字を含まない（ファイル名称のみ）場合、ListBox作成
                    If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                        'ファイルパス格納　　　★Update_Dディレクトリを示す
                        WW_Files_dir.Add(tempFile)
                        'ファイル名格納
                        WW_Files_name.Add(WW_tempFile)
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If
                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        MA0004PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim MA0004PDFrow As DataRow = MA0004PDFtbl.NewRow
            MA0004PDFrow("FILENAME") = WW_Files_name.Item(i)
            MA0004PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            MA0004PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            MA0004PDFtbl.Rows.Add(MA0004PDFrow)
        Next

        'Repeaterバインド
        WF_DViewRepPDF.DataSource = MA0004PDFtbl
        WF_DViewRepPDF.DataBind()

        CType(WF_ListBoxPDF, ListBox).Items.Clear()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1
            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.ALIVE
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)

            WF_ListBoxPDF.Items.Add(New ListItem(WW_Files_name.Item(i), C_DELETE_FLG.ALIVE))
        Next

        '○イベント設定
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & " )"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF表示内容変更時処理（Detail・PDFタブ内のListBox切替時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_SELECTchange()

        Dim WW_UPfiles As String()

        '○年度算出(有効開始年月日を基準として算出)
        Dim WW_nendo As String
        Dim WW_date As Date
        Dim WW_TempDir As String

        Try
            Date.TryParse(WF_STYMD.Text, WW_date)
        Catch ex As Exception
            WW_date = C_DEFAULT_YMD
        End Try

        If WF_STYMD.Text = "" OrElse WW_date <= C_DEFAULT_YMD Then
            Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "WF_STYMD")
            Exit Sub
        End If

        If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
            WW_nendo = (WW_date.Year - 1).ToString()
        Else
            WW_nendo = (WW_date.Year).ToString()
        End If

        '○事前確認
        '統一車番の存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" OrElse WF_TSHABAN.Text = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To MA0004tbl.Rows.Count - 1
                If WF_SHARYOTYPE.Text = MA0004tbl.Rows(i)("SHARYOTYPE") OrElse WF_TSHABAN.Text = MA0004tbl.Rows(i)("TSHABAN") Then
                    Exit For
                Else
                    If (i - 1) >= MA0004tbl.Rows.Count Then
                        Master.output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "統一車番")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○削除処理
        '○Detail・表示PDFが、削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　※WF_Rep_FILEPATHは、Update_Dフォルダ内該当PDFを示す。

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.DELETE Then

                Try
                    File.Delete(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        '○画面編集
        '○PDF格納一時保存ディレクトリ編集
        If WF_Rep11_PDFselect.SelectedValue.ToString() = 1 Then
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
            WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        Else
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_D
            WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        End If

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        WW_UPfiles = Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)

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
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        MA0004PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim MA0004PDFrow As DataRow = MA0004PDFtbl.NewRow
            MA0004PDFrow("FILENAME") = WW_Files_name.Item(i)
            MA0004PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            MA0004PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            MA0004PDFtbl.Rows.Add(MA0004PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = MA0004PDFtbl
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
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & " )"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF STYMD変更時処理（Detail・有効開始年月日変更時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_STYMDchange()

        '○実行処理
        '○PDF読み込み & ディレクトリ作成
        PDF_INITread(WF_CAMPCODE.Text, WF_SHARYOTYPE.Text, WF_TSHABAN.Text, WF_STYMD.Text)

        '○メッセージ編集処理　…　PDFは再読み込みされました。（途中までの操作は破棄される）
        Master.output(C_MESSAGE_NO.PDF_DATA_REVIEW_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' PDF表追加時処理（Detail・表追加ボタン押下時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_SAVE_H()

        '○初期設定
        '○年度算出(有効開始年月日を基準として算出)
        Dim WW_nendo As String
        Dim WW_date As Date
        Dim WW_Dir As String
        Dim WW_TempDir As String

        Try
            Date.TryParse(WF_STYMD.Text, WW_date)
        Catch ex As Exception
            WW_date = C_DEFAULT_YMD
        End Try

        If WF_STYMD.Text = "" OrElse WW_date <= C_DEFAULT_YMD Then
            Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "WF_STYMD")
            Exit Sub
        End If

        If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
            WW_nendo = (WW_date.Year - 1).ToString()
        Else
            WW_nendo = (WW_date.Year).ToString()
        End If

        '○事前確認
        '統一車番の存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" OrElse WF_TSHABAN.Text = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To MA0004tbl.Rows.Count - 1
                If WF_SHARYOTYPE.Text = MA0004tbl.Rows(i)("SHARYOTYPE") OrElse WF_TSHABAN.Text = MA0004tbl.Rows(i)("TSHABAN") Then
                    Exit For
                Else
                    If (i - 1) >= MA0004tbl.Rows.Count Then
                        Master.output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "統一車番")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○画面・削除入力処理
        '○Detail・表示PDFが、削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　※WF_Rep_FILEPATHは、Update_Dフォルダ内該当PDFを示す。

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            If CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.DELETE Then
                Try
                    File.Delete(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        '○ファイルコピー

        For i As Integer = 1 To 12
            '○Update_Hフォルダクリア処理
            'PDF格納一時保存ディレクトリ編集
            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & i.ToString("00") & "\Update_H"
            End If

            For Each tempFile As String In Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            '○Update_Dフォルダ内容をUpdate_Hフォルダへコピー
            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_D"
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_D
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & i.ToString("00") & "\Update_D"
            End If

            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & i.ToString("00") & "\Update_H"
            End If

            '○Update_Dフォルダ内PDF→Update_Hフォルダへ上書コピー
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
                'Update_Dフォルダ内容取得
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If

                Loop Until InStr(WW_File, "\") <= 0

                'Update_Dフォルダ内PDF→Update_Hフォルダへ上書コピー
                File.Copy(tempFile, WW_TempDir & "\" & WW_File, True)
            Next

            '○Update_Dフォルダクリア
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next
        Next

        '○PDF初期画面編集

        'Repeaterバインド準備
        MA0004PDFtbl_ColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = MA0004PDFtbl
        WF_DViewRepPDF.DataBind()

    End Sub

    ''' <summary>
    '''  PDFファイルアップロード入力処理(PDFドロップ時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_PDF()

        '○初期設定
        '○年度算出(有効開始年月日を基準として算出)
        Dim WW_nendo As String
        Dim WW_date As Date
        Dim WW_Dir As String

        Try
            Date.TryParse(WF_STYMD.Text, WW_date)
        Catch ex As Exception
            WW_date = C_DEFAULT_YMD
        End Try

        If WF_STYMD.Text = "" OrElse WW_date <= C_DEFAULT_YMD Then
            Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "WF_STYMD")
            Exit Sub
        End If

        If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
            WW_nendo = (WW_date.Year - 1).ToString()
        Else
            WW_nendo = (WW_date.Year).ToString()
        End If

        '○事前確認
        '統一車番の存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_SHARYOTYPE.Text = "" OrElse WF_TSHABAN.Text = "" Then
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To MA0004tbl.Rows.Count - 1
                If WF_SHARYOTYPE.Text = MA0004tbl.Rows(i)("SHARYOTYPE") OrElse WF_TSHABAN.Text = MA0004tbl.Rows(i)("TSHABAN") Then
                    Exit For
                Else
                    If (i - 1) >= MA0004tbl.Rows.Count Then
                        Master.output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "統一車番")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○PDF格納一時保存ディレクトリ編集
        If WF_Rep11_PDFselect.SelectedValue.ToString() = 1 Then
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
            WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        Else
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_D
            WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        End If

        '○アップロードフォルダからUpdate_Dフォルダーへファイル移動
        '○アップロードファイル名を取得　＆　移動
        For Each tempFile As String In Directory.GetFiles(CS0050Session.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050Session.USERID, "*.*")

            'ディレクトリ付ファイル名より、ファイル名編集
            Dim WW_File As String = tempFile
            Do
                If InStr(WW_File, "\") > 0 Then
                    WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                End If

            Loop Until InStr(WW_File, "\") <= 0

            '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
            Try
                File.Copy(tempFile, WW_Dir & "\" & WW_File, True)
                File.Delete(tempFile)
            Catch ex As Exception
            End Try

            Exit For
        Next

        '○画面編集
        '○PDF格納一時保存ディレクトリ編集
        If WF_Rep11_PDFselect.SelectedValue.ToString() = 1 Then
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_D
            WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WW_nendo & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        Else
            'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_D
            WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
            WW_Dir = WW_Dir & "\" & WF_SHARYOTYPE.Text & WF_TSHABAN.Text & "_" & WF_Rep11_PDFselect.SelectedValue.ToString() & "\Update_D"
        End If


        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
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
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        MA0004PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim MA0004PDFrow As DataRow = MA0004PDFtbl.NewRow
            MA0004PDFrow("FILENAME") = WW_Files_name.Item(i)
            MA0004PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            MA0004PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            MA0004PDFtbl.Rows.Add(MA0004PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = MA0004PDFtbl
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
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & " )"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

        '○メッセージ編集
        Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' PDF DB更新処理（DB更新ボタン押下時）
    ''' </summary>
    ''' <param name="In_SHARYOTYPE"></param>
    ''' <param name="In_TSHABAN"></param>
    ''' <param name="In_STYMD"></param>
    ''' <remarks></remarks>
    Protected Sub PDF_DBupdate(ByRef In_SHARYOTYPE As String, ByRef In_TSHABAN As String, ByRef In_STYMD As String)

        '○初期設定
        '○年度算出(有効開始年月日を基準として算出)
        Dim WW_nendo As String
        Dim WW_date As Date
        Dim WW_Dir As String
        Dim WW_DirSend As String
        Dim WW_TempDir As String

        Try
            Date.TryParse(In_STYMD, WW_date)
        Catch ex As Exception
        End Try

        If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
            WW_nendo = (WW_date.Year - 1).ToString()
        Else
            WW_nendo = (WW_date.Year).ToString()
        End If

        '　⑦DB更新ボタン押下時：★★★
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_D・Update_Hをお掃除
        '○DB反映処理

        For i As Integer = 1 To 12
            '○PDF格納ディレクトリ編集
            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\" & In_SHARYOTYPE & In_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\統一車番_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\" & In_SHARYOTYPE & In_TSHABAN & "_" & i.ToString("00")
            End If

            '○FTP格納ディレクトリ編集
            If i = 1 Then
                'C:\APPL\APPLFILES\SEND\SENDSTOR\端末名\MA0004_SHARYOC\統一車番_年度_nn
                WW_DirSend = CS0050Session.UPLOAD_PATH & "\SEND\SENDSTOR\" & Master.USERTERMID & "\PDF\MA0004_SHARYOC"
                WW_DirSend = WW_DirSend & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                'C:\APPL\APPLFILES\SEND\SENDSTOR\端末名\MA0004_SHARYOC\統一車番_nn
                WW_DirSend = CS0050Session.UPLOAD_PATH & "\SEND\SENDSTOR\" & Master.USERTERMID & "\PDF\MA0004_SHARYOC"
                WW_DirSend = WW_DirSend & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & i.ToString("00")
            End If

            '○PDF正式格納フォルダクリア処理
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
                'サブフォルダは対象外
                If InStr(tempFile, "\Temp") <= 0 Then
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                End If
            Next

            '○Update_Hフォルダ内容をPDF正式格納フォルダへコピー
            If i = 1 Then
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_年度_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\" & In_SHARYOTYPE & In_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00")
            Else
                'PDF格納ディレクトリ編集    c:\appl\applpdf\MA0004_SHARYOC\統一車番_nn
                WW_Dir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\" & In_SHARYOTYPE & In_TSHABAN & "_" & i.ToString("00")
            End If

            'PDF格納一時保存ディレクトリ編集
            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_H"
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & i.ToString("00") & "\Update_H"
            End If

            Dim WW_tempFiles As String() = Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_tempFiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If

                Loop Until InStr(WW_File, "\") <= 0

                'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                File.Copy(tempFile, WW_Dir & "\" & WW_File, True)
            Next

            '○Update_Dフォルダクリア　※Update_Hフォルダは、連続処理に備えてクリアーしない
            'PDF格納一時保存ディレクトリ編集
            If i = 1 Then
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_年度_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & WW_nendo & "_" & i.ToString("00") & "\Update_D"
            Else
                'c:\appl\applpdf\MA0004_SHARYOC\Temp\ユーザID\統一車番_nn\Update_H
                WW_TempDir = CS0050Session.PDF_PATH & "\MA0004_SHARYOC\Temp\" & CS0050Session.USERID
                WW_TempDir = WW_TempDir & "\" & In_SHARYOTYPE & In_TSHABAN & "_" & i.ToString("00") & "\Update_D"
            End If

            For Each tempFile As String In Directory.GetFiles(WW_TempDir, "*", SearchOption.AllDirectories)
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            'PDF正式格納フォルダ→配信用PDF格納フォルダへ上書コピー
            'ファイルが存在しない場合、空の配信用PDF格納フォルダを作成する
            If Directory.Exists(WW_DirSend) Then
                For Each tempFile As String In Directory.GetFiles(WW_DirSend, "*", SearchOption.AllDirectories)
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            Else
                Directory.CreateDirectory(WW_DirSend)
            End If
            Dim WW_tempDirs As String() = Directory.GetFiles(WW_Dir, "*")
            For Each tempFile As String In WW_tempDirs
                Dim WW_File As String = Path.GetFileName(tempFile)
                File.Copy(tempFile, WW_DirSend & "\" & WW_File, True)
            Next
        Next

    End Sub

    ''' <summary>
    '''  PDF内容表示（Detail・PDFダブルクリック時（内容照会））
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
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' PDFファイルリセット
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_reset()

        Dim WW_Dir As String = ""

        'フォルダ作成処理
        Dim WW_DirIN As String() = Directory.GetDirectories("C:\APPL\APPLPDF\MA0004_SHARYOC", "*", SearchOption.AllDirectories)
        Dim WW_DirOUT As String = ""

        For Each tempFile As String In WW_DirIN

            WW_DirOUT = tempFile
            WW_DirOUT = WW_DirOUT.Replace("C:\APPL\APPLPDF\MA0004_SHARYOC\", "")

            If Right(WW_DirOUT, 2) = "01" Then
                WW_DirOUT = "C:\APPL\APPLPDF\MA0004_SHARYOC_new\" & WW_DirOUT
            Else
                WW_DirOUT = "C:\APPL\APPLPDF\MA0004_SHARYOC_new\" & Mid(WW_DirOUT, 1, 8) & Right(WW_DirOUT, 2)
            End If

            'ディレクトリ作成
            If Not Directory.Exists(WW_DirOUT) Then
                Directory.CreateDirectory(WW_DirOUT)
            End If

        Next

        'ファイルコピー処理
        Dim WW_filesIN As String() = Directory.GetFiles("C:\APPL\APPLPDF\MA0004_SHARYOC", "*", SearchOption.AllDirectories)
        Dim WW_filesOUT As String = ""

        For Each tempFile As String In WW_filesIN

            If InStr(tempFile, "Temp") > 0 Then
                WW_filesOUT = tempFile
            Else
                WW_filesOUT = tempFile
                WW_filesOUT = WW_filesOUT.Replace("C:\APPL\APPLPDF\MA0004_SHARYOC\", "")

                Dim WW_SYARYO As String = Mid(WW_filesOUT, 1, 8)
                Dim WW_NENDO As String = Mid(WW_filesOUT, 10, 4)
                Dim WW_KBN As String = Mid(WW_filesOUT, 15, 2)
                Dim WW_PDF As String = Mid(WW_filesOUT, 18, 100)
                Dim WW_PDFnew As String = Mid(WW_PDF, 1, Len(WW_PDF) - 4)


                If Mid(WW_filesOUT, 12, 2) = "01" Then
                    WW_filesOUT = "C:\APPL\APPLPDF\MA0004_SHARYOC_new\" & WW_filesOUT
                Else
                    WW_filesOUT = "C:\APPL\APPLPDF\MA0004_SHARYOC_new\" & WW_SYARYO & "_" & WW_KBN & "\" & WW_NENDO & "_" & WW_PDF
                End If

                'ファイルコピー
                File.Copy(tempFile, WW_filesOUT, True)
            End If
        Next

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MA0004S Then
            Master.MAPID = GRMA0004WRKINC.MAPID

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
        Dim WW_STYMD_C As String = ""

        '○選択日付編集
        Dim WW_str As String
        Dim WW_STYMD_Yuko As Date      '有効期限(開始)
        Dim WW_ENDYMD_Yuko As Date     '有効期限(終了)
        Dim WW_STYMD_Nendo As Date     '
        Dim WW_ENDYMD_Nendo As Date    '
        Dim WW_int As Integer

        '○有効期限(開始)
        WW_str = work.WF_SEL_YYF.Text & "/4/1"
        Try
            Date.TryParse(WW_str, WW_STYMD_Yuko)
        Catch ex As Exception
            WW_STYMD_Yuko = "2000/4/1"
        End Try

        '○有効期限(終了)
        Try
            Integer.TryParse(work.WF_SEL_YYT.Text, WW_int)
            WW_str = (WW_int + 1).ToString() & "/3/31"

            Date.TryParse(WW_str, WW_ENDYMD_Yuko)
        Catch ex As Exception
            WW_ENDYMD_Yuko = "2099/3/31"
        End Try

        '○対象年度(開始)
        WW_str = work.WF_SEL_YYF.Text & "/4/1"
        Try
            Date.TryParse(WW_str, WW_STYMD_Nendo)
        Catch ex As Exception
            WW_STYMD_Nendo = "2000/4/1"
        End Try

        '○対象年度((終了))
        Try
            Integer.TryParse(work.WF_SEL_YYT.Text, WW_int)
            WW_str = (WW_int + 1).ToString() & "/3/31"

            Date.TryParse(WW_str, WW_ENDYMD_Nendo)
        Catch ex As Exception
            WW_ENDYMD_Nendo = "2099/3/31"
        End Try

        '○画面表示用データ取得

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            '■テーブル検索結果をテーブル退避
            'MA0004テンポラリDB項目作成
            If MA0004tbl Is Nothing Then
                MA0004tbl = New DataTable
            End If

            If MA0004tbl.Columns.Count <> 0 Then
                MA0004tbl.Columns.Clear()
            End If

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
                    & "         cast(isnull(B.ACCTRCYCLE,'') as VarChar)as ACCTRCYCLE  ,   " _
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
                    & "              ELSE FORMAT(C.STYMD,'yyyy/MM/dd')                     " _
                    & "         END                                     as STYMD       ,   " _
                    & "         CASE WHEN A.ENDYMD IS NULL THEN ''                         " _
                    & "              ELSE FORMAT(C.ENDYMD,'yyyy/MM/dd')                    " _
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
                    & "       and   C.STYMD          <= @P07                               " _
                    & "       and   C.ENDYMD         >= @P06                               " _
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
                    PARA4.Value = WW_STYMD_Yuko        '有効期限(開始)
                    PARA5.Value = WW_ENDYMD_Yuko       '有効期限(終了)
                    PARA6.Value = WW_STYMD_Nendo       '対象年度(開始)
                    PARA7.Value = WW_ENDYMD_Nendo      '対象年度(終了)

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MA0004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'MA0004tbl値設定
                        Dim WW_DATA_CNT As Integer = -1
                        While SQLdr.Read

                            '○テーブル初期化
                            Dim MA0004row As DataRow = MA0004tbl.NewRow()
                            Dim WW_DATE As Date

                            '○データ設定

                            '固定項目
                            WW_DATA_CNT = WW_DATA_CNT + 1
                            MA0004row("WORK_NO") = WW_DATA_CNT.ToString()
                            MA0004row("LINECNT") = 0
                            MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            If IsDBNull(SQLdr("TIMSTP")) Then
                                MA0004row("TIMSTP") = "0"
                            Else
                                MA0004row("TIMSTP") = SQLdr("TIMSTP")
                            End If

                            MA0004row("SELECT") = 1   '1:表示
                            MA0004row("HIDDEN") = 0   '0:表示

                            '画面毎の設定項目
                            MA0004row("CAMPCODE") = SQLdr("CAMPCODE")
                            MA0004row("SHARYOTYPE") = SQLdr("SHARYOTYPE")
                            MA0004row("TSHABAN") = SQLdr("TSHABAN")

                            MA0004row("STYMD") = If(SQLdr("STYMD"), "")
                            MA0004row("ENDYMD") = If(SQLdr("ENDYMD"), "")

                            'デバック用フィールド
                            MA0004row("STYMD_S") = If(SQLdr("STYMD_S"), "")
                            MA0004row("ENDYMD_S") = If(SQLdr("ENDYMD_S"), "")

                            MA0004row("STYMD_A") = If(SQLdr("STYMD_A"), "")
                            MA0004row("ENDYMD_A") = If(SQLdr("ENDYMD_A"), "")

                            MA0004row("STYMD_B") = If(SQLdr("STYMD_B"), "")
                            MA0004row("ENDYMD_B") = If(SQLdr("ENDYMD_B"), "")

                            MA0004row("STYMD_C") = If(SQLdr("STYMD_C"), "")
                            MA0004row("ENDYMD_C") = If(SQLdr("ENDYMD_C"), "")


                            MA0004row("DELFLG") = SQLdr("DELFLG")
                            MA0004row("SHARYOTYPEF") = SQLdr("SHARYOTYPEF")
                            MA0004row("TSHABANF") = SQLdr("TSHABANF")
                            MA0004row("SHARYOTYPEB") = SQLdr("SHARYOTYPEB")
                            MA0004row("TSHABANB") = SQLdr("TSHABANB")
                            MA0004row("SHARYOTYPEB2") = SQLdr("SHARYOTYPEB2")
                            MA0004row("TSHABANB2") = SQLdr("TSHABANB2")
                            MA0004row("SHARYOTYPEB3") = SQLdr("SHARYOTYPEB3")
                            MA0004row("TSHABANB3") = SQLdr("TSHABANB3")
                            MA0004row("GSHABAN") = SQLdr("GSHABAN")
                            MA0004row("SEQ") = SQLdr("SEQ")
                            MA0004row("MANGMORG") = SQLdr("MANGMORG")
                            MA0004row("MANGSORG") = SQLdr("MANGSORG")
                            MA0004row("MANGOILTYPE") = SQLdr("MANGOILTYPE")
                            MA0004row("MANGOWNCODE") = SQLdr("MANGOWNCODE")
                            MA0004row("MANGOWNCONT") = SQLdr("MANGOWNCONT")
                            MA0004row("MANGSHAFUKU") = SQLdr("MANGSHAFUKU")
                            MA0004row("MANGSUPPL") = SQLdr("MANGSUPPL")
                            MA0004row("MANGTTLDIST") = SQLdr("MANGTTLDIST")
                            MA0004row("MANGUORG") = SQLdr("MANGUORG")
                            MA0004row("BASELEASE") = SQLdr("BASELEASE")
                            MA0004row("BASERAGE") = SQLdr("BASERAGE")
                            MA0004row("BASERAGEMM") = SQLdr("BASERAGEMM")
                            MA0004row("BASERAGEYY") = SQLdr("BASERAGEYY")
                            MA0004row("BASERDATE") = If(SQLdr("BASERDATE"), "")
                            If IsDBNull(SQLdr("BASERDATE")) OrElse SQLdr("BASERDATE") = "" Then
                                MA0004row("BASERDATE") = ""
                            Else
                                Dim WW_DATENOW As Date = Date.Now
                                Dim WW_BASERAGEYY As Integer
                                Dim WW_BASERAGE As Integer
                                Dim WW_BASERAGEMM As Integer
                                WW_BASERAGE = DateDiff("m", WW_DATE, WW_DATENOW)
                                WW_BASERAGEYY = Math.Truncate(WW_BASERAGE / 12)
                                WW_BASERAGEMM = WW_BASERAGE Mod 12
                                MA0004row("BASERAGEMM") = WW_BASERAGEMM
                                MA0004row("BASERAGEYY") = WW_BASERAGEYY
                                MA0004row("BASERAGE") = WW_BASERAGE
                            End If
                            MA0004row("FCTRAXLE") = SQLdr("FCTRAXLE")
                            MA0004row("FCTRDPR") = SQLdr("FCTRDPR")
                            MA0004row("FCTRFUELCAPA") = SQLdr("FCTRFUELCAPA")
                            MA0004row("FCTRFUELMATE") = SQLdr("FCTRFUELMATE")
                            MA0004row("FCTRRESERVE1") = SQLdr("FCTRRESERVE1")
                            MA0004row("FCTRRESERVE2") = SQLdr("FCTRRESERVE2")
                            MA0004row("FCTRRESERVE3") = SQLdr("FCTRRESERVE3")
                            MA0004row("FCTRRESERVE4") = SQLdr("FCTRRESERVE4")
                            MA0004row("FCTRRESERVE5") = SQLdr("FCTRRESERVE5")
                            MA0004row("FCTRSHFTNUM") = SQLdr("FCTRSHFTNUM")
                            MA0004row("FCTRSUSP") = SQLdr("FCTRSUSP")
                            MA0004row("FCTRTIRE") = SQLdr("FCTRTIRE")
                            MA0004row("FCTRTMISSION") = SQLdr("FCTRTMISSION")
                            MA0004row("FCTRUREA") = SQLdr("FCTRUREA")
                            MA0004row("OTNKBPIPE") = SQLdr("OTNKBPIPE")
                            MA0004row("OTNKCELLNO") = SQLdr("OTNKCELLNO")
                            MA0004row("OTNKVAPOR") = SQLdr("OTNKVAPOR")
                            MA0004row("OTNKCELPART") = SQLdr("OTNKCELPART")
                            MA0004row("OTNKCVALVE") = SQLdr("OTNKCVALVE")
                            MA0004row("OTNKDCD") = SQLdr("OTNKDCD")
                            MA0004row("OTNKDETECTOR") = SQLdr("OTNKDETECTOR")
                            MA0004row("OTNKDISGORGE") = SQLdr("OTNKDISGORGE")
                            MA0004row("OTNKHTECH") = SQLdr("OTNKHTECH")
                            MA0004row("OTNKINSSTAT") = SQLdr("OTNKINSSTAT")
                            MA0004row("OTNKINSYMD") = If(SQLdr("OTNKINSYMD"), "")

                            MA0004row("OTNKLVALVE") = SQLdr("OTNKLVALVE")
                            MA0004row("OTNKMATERIAL") = SQLdr("OTNKMATERIAL")
                            MA0004row("OTNKPIPE") = SQLdr("OTNKPIPE")
                            MA0004row("OTNKPIPESIZE") = SQLdr("OTNKPIPESIZE")
                            MA0004row("OTNKPUMP") = SQLdr("OTNKPUMP")
                            MA0004row("OTNKTINSNO") = SQLdr("OTNKTINSNO")
                            MA0004row("HPRSINSISTAT") = SQLdr("HPRSINSISTAT")

                            MA0004row("HPRSINSIYMD") = If(SQLdr("HPRSINSIYMD"), "")

                            MA0004row("HPRSINSULATE") = SQLdr("HPRSINSULATE")
                            MA0004row("HPRSMATR") = SQLdr("HPRSMATR")
                            MA0004row("HPRSPIPE") = SQLdr("HPRSPIPE")
                            MA0004row("HPRSPIPENUM") = SQLdr("HPRSPIPENUM")
                            MA0004row("HPRSPUMP") = SQLdr("HPRSPUMP")
                            MA0004row("HPRSRESSRE") = SQLdr("HPRSRESSRE")
                            MA0004row("HPRSSERNO") = SQLdr("HPRSSERNO")
                            MA0004row("HPRSSTRUCT") = SQLdr("HPRSSTRUCT")
                            MA0004row("HPRSVALVE") = SQLdr("HPRSVALVE")
                            MA0004row("CHEMCELLNO") = SQLdr("CHEMCELLNO")
                            MA0004row("CHEMCELPART") = SQLdr("CHEMCELPART")
                            MA0004row("CHEMDISGORGE") = SQLdr("CHEMDISGORGE")
                            MA0004row("CHEMHOSE") = SQLdr("CHEMHOSE")
                            MA0004row("CHEMINSSTAT") = SQLdr("CHEMINSSTAT")

                            MA0004row("CHEMINSYMD") = If(SQLdr("CHEMINSYMD"), "")

                            MA0004row("CHEMMANOMTR") = SQLdr("CHEMMANOMTR")
                            MA0004row("CHEMMATERIAL") = SQLdr("CHEMMATERIAL")
                            MA0004row("CHEMPMPDR") = SQLdr("CHEMPMPDR")
                            MA0004row("CHEMPRESDRV") = SQLdr("CHEMPRESDRV")
                            MA0004row("CHEMPRESEQ") = SQLdr("CHEMPRESEQ")
                            MA0004row("CHEMPUMP") = SQLdr("CHEMPUMP")
                            MA0004row("CHEMSTRUCT") = SQLdr("CHEMSTRUCT")
                            MA0004row("CHEMTHERM") = SQLdr("CHEMTHERM")
                            MA0004row("CHEMTINSNO") = SQLdr("CHEMTINSNO")

                            MA0004row("CHEMTINSNYMD") = If(SQLdr("CHEMTINSNYMD"), "")
                            MA0004row("CHEMTINSYMD") = If(SQLdr("CHEMTINSYMD"), "")

                            MA0004row("OFFCRESERVE1") = SQLdr("OFFCRESERVE1")
                            MA0004row("OFFCRESERVE2") = SQLdr("OFFCRESERVE2")
                            MA0004row("OFFCRESERVE3") = SQLdr("OFFCRESERVE3")
                            MA0004row("OFFCRESERVE4") = SQLdr("OFFCRESERVE4")
                            MA0004row("OFFCRESERVE5") = SQLdr("OFFCRESERVE5")
                            MA0004row("OTHRBMONITOR") = SQLdr("OTHRBMONITOR")
                            MA0004row("OTHRBSONAR") = SQLdr("OTHRBSONAR")
                            MA0004row("OTHRDOCO") = SQLdr("OTHRDOCO")
                            MA0004row("OTHRDRRECORD") = SQLdr("OTHRDRRECORD")
                            MA0004row("OTHRPAINTING") = SQLdr("OTHRPAINTING")
                            MA0004row("OTHRRADIOCON") = SQLdr("OTHRRADIOCON")
                            MA0004row("OTHRRTARGET") = SQLdr("OTHRRTARGET")
                            MA0004row("OTHRTERMINAL") = SQLdr("OTHRTERMINAL")
                            MA0004row("ACCTASST01") = SQLdr("ACCTASST01")
                            MA0004row("ACCTASST02") = SQLdr("ACCTASST02")
                            MA0004row("ACCTASST03") = SQLdr("ACCTASST03")
                            MA0004row("ACCTASST04") = SQLdr("ACCTASST04")
                            MA0004row("ACCTASST05") = SQLdr("ACCTASST05")
                            MA0004row("ACCTASST06") = SQLdr("ACCTASST06")
                            MA0004row("ACCTASST07") = SQLdr("ACCTASST07")
                            MA0004row("ACCTASST08") = SQLdr("ACCTASST08")
                            MA0004row("ACCTASST09") = SQLdr("ACCTASST09")
                            MA0004row("ACCTASST10") = SQLdr("ACCTASST10")
                            MA0004row("ACCTLEASE1") = SQLdr("ACCTLEASE1")
                            MA0004row("ACCTLEASE2") = SQLdr("ACCTLEASE2")
                            MA0004row("ACCTLEASE3") = SQLdr("ACCTLEASE3")
                            MA0004row("ACCTLEASE4") = SQLdr("ACCTLEASE4")
                            MA0004row("ACCTLEASE5") = SQLdr("ACCTLEASE5")
                            MA0004row("ACCTLSUPL1") = SQLdr("ACCTLSUPL1")
                            MA0004row("ACCTLSUPL2") = SQLdr("ACCTLSUPL2")
                            MA0004row("ACCTLSUPL3") = SQLdr("ACCTLSUPL3")
                            MA0004row("ACCTLSUPL4") = SQLdr("ACCTLSUPL4")
                            MA0004row("ACCTLSUPL5") = SQLdr("ACCTLSUPL5")
                            MA0004row("ACCTRCYCLE") = Format(Val(SQLdr("ACCTRCYCLE")), "#,#")
                            MA0004row("NOTES") = SQLdr("NOTES")
                            MA0004row("LICN5LDCAPA") = SQLdr("LICN5LDCAPA")
                            MA0004row("LICNCWEIGHT") = SQLdr("LICNCWEIGHT")
                            MA0004row("LICNFRAMENO") = SQLdr("LICNFRAMENO")
                            MA0004row("LICNLDCAPA") = SQLdr("LICNLDCAPA")
                            MA0004row("LICNMNFACT") = SQLdr("LICNMNFACT")
                            MA0004row("LICNMODEL") = SQLdr("LICNMODEL")
                            MA0004row("LICNMOTOR") = SQLdr("LICNMOTOR")
                            MA0004row("LICNPLTNO1") = SQLdr("LICNPLTNO1")
                            MA0004row("LICNPLTNO2") = SQLdr("LICNPLTNO2")
                            MA0004row("LICNTWEIGHT") = SQLdr("LICNTWEIGHT")
                            MA0004row("LICNWEIGHT") = SQLdr("LICNWEIGHT")

                            MA0004row("LICNYMD") = If(SQLdr("LICNYMD"), "")

                            If Val(SQLdr("TAXATAX")) = 0 Then
                                MA0004row("TAXATAX") = "0"
                            Else
                                MA0004row("TAXATAX") = Format(Val(SQLdr("TAXATAX")), "#,#")
                            End If
                            If Val(SQLdr("TAXLINS")) = 0 Then
                                MA0004row("TAXLINS") = "0"
                            Else
                                MA0004row("TAXLINS") = Format(Val(SQLdr("TAXLINS")), "#,#")
                            End If

                            MA0004row("TAXLINSYMD") = If(SQLdr("TAXLINSYMD"), "")

                            If Val(SQLdr("TAXVTAX")) = 0 Then
                                MA0004row("TAXVTAX") = "0"
                            Else
                                MA0004row("TAXVTAX") = Format(Val(SQLdr("TAXVTAX")), "#,#")
                            End If

                            MA0004row("OTNKTINSNYMD") = If(SQLdr("OTNKTINSNYMD"), "")
                            MA0004row("OTNKTINSYMD") = If(SQLdr("OTNKTINSYMD"), "")
                            MA0004row("HPRSINSNYMD") = If(SQLdr("HPRSINSNYMD"), "")
                            MA0004row("HPRSINSYMD") = If(SQLdr("HPRSINSYMD"), "")
                            MA0004row("HPRSJINSYMD") = If(SQLdr("HPRSJINSYMD"), "")

                            MA0004row("INSKBN") = SQLdr("INSKBN")
                            MA0004row("MANGPROD1") = SQLdr("MANGPROD1")
                            MA0004row("MANGPROD2") = SQLdr("MANGPROD2")
                            MA0004row("FCTRSMAKER") = SQLdr("FCTRSMAKER")
                            MA0004row("FCTRTMAKER") = SQLdr("FCTRTMAKER")
                            MA0004row("OTNKEXHASIZE") = SQLdr("OTNKEXHASIZE")
                            MA0004row("HPRSPMPDR") = SQLdr("HPRSPMPDR")
                            MA0004row("HPRSHOSE") = SQLdr("HPRSHOSE")
                            MA0004row("CONTSHAPE") = SQLdr("CONTSHAPE")
                            MA0004row("CONTPUMP") = SQLdr("CONTPUMP")
                            MA0004row("CONTPMPDR") = SQLdr("CONTPMPDR")
                            MA0004row("OTHRTIRE1") = SQLdr("OTHRTIRE1")
                            MA0004row("OTHRTIRE2") = SQLdr("OTHRTIRE2")
                            MA0004row("OTHRTPMS") = SQLdr("OTHRTPMS")
                            MA0004row("OTNKTMAKER") = SQLdr("OTNKTMAKER")
                            MA0004row("HPRSTMAKER") = SQLdr("HPRSTMAKER")
                            MA0004row("CHEMTMAKER") = SQLdr("CHEMTMAKER")
                            MA0004row("CONTTMAKER") = SQLdr("CONTTMAKER")

                            MA0004row("SHARYOSTATUS") = SQLdr("SHARYOSTATUS")
                            MA0004row("SHARYOINFO1") = SQLdr("SHARYOINFO1")
                            MA0004row("SHARYOINFO2") = SQLdr("SHARYOINFO2")
                            MA0004row("SHARYOINFO3") = SQLdr("SHARYOINFO3")
                            MA0004row("SHARYOINFO4") = SQLdr("SHARYOINFO4")
                            MA0004row("SHARYOINFO5") = SQLdr("SHARYOINFO5")
                            MA0004row("SHARYOINFO6") = SQLdr("SHARYOINFO6")

                            '統一車番＋S開始年月日がブレイク
                            If MA0004row("SHARYOTYPE") = WW_SHARYOTYPE AndAlso
                                MA0004row("TSHABAN") = WW_TSHABAN AndAlso
                                MA0004row("STYMD_C") = WW_STYMD_C Then
                                MA0004row("SELECT") = 0
                            Else
                                MA0004row("SELECT") = 1
                                MA0004row("HIDDEN") = 0   '0:表示
                                '前回キー保存
                                WW_SHARYOTYPE = MA0004row("SHARYOTYPE")
                                WW_TSHABAN = MA0004row("TSHABAN")
                                WW_STYMD_C = MA0004row("STYMD_C")
                            End If

                            '○条件画面で指定に該当するデータを抽出
                            If MA0004row("SELECT") = 1 Then

                                '管理組織
                                Dim WW_SELECT_MORG As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_MORG.Text = "" Then
                                    WW_SELECT_MORG = 1
                                Else
                                    If work.WF_SEL_MORG.Text = MA0004row("MANGMORG") Then
                                        WW_SELECT_MORG = 1
                                    End If
                                End If
                                If WW_SELECT_MORG = 0 Then
                                    MA0004row("SELECT") = 0
                                End If

                                '条件画面で指定された設置部署を抽出
                                Dim WW_SELECT_SORG As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_SORG.Text = "" Then
                                    WW_SELECT_SORG = 1
                                Else
                                    If work.WF_SEL_SORG.Text = MA0004row("MANGSORG") Then
                                        WW_SELECT_SORG = 1
                                    End If
                                End If
                                If WW_SELECT_SORG = 0 Then
                                    MA0004row("SELECT") = 0
                                End If

                                '条件画面で指定された油種を抽出
                                Dim WW_SELECT_OILTYPE As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_OILTYPE1.Text = "" AndAlso
                                    work.WF_SEL_OILTYPE2.Text = "" Then
                                    WW_SELECT_OILTYPE = 1
                                Else
                                    If work.WF_SEL_OILTYPE1.Text = MA0004row("MANGOILTYPE") Then
                                        WW_SELECT_OILTYPE = 1
                                    End If
                                    If work.WF_SEL_OILTYPE2.Text = MA0004row("MANGOILTYPE") Then
                                        WW_SELECT_OILTYPE = 1
                                    End If
                                End If
                                If WW_SELECT_OILTYPE = 0 Then
                                    MA0004row("SELECT") = 0
                                End If

                                '条件画面で指定された荷主を抽出
                                Dim WW_SELECT_OWNER As Integer = 0    '0:対象外、1:対象
                                If work.WF_SEL_OWNCODE1.Text = "" AndAlso
                                    work.WF_SEL_OWNCODE2.Text = "" Then
                                    WW_SELECT_OWNER = 1
                                Else
                                    If work.WF_SEL_OWNCODE1.Text <= MA0004row("MANGOWNCODE") AndAlso
                                        work.WF_SEL_OWNCODE2.Text >= MA0004row("MANGOWNCODE") Then
                                        WW_SELECT_OWNER = 1
                                    End If
                                End If
                                If WW_SELECT_OWNER = 0 Then
                                    MA0004row("SELECT") = 0
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
                                    If work.WF_SEL_SHARYOTYPE1.Text = MA0004row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE2.Text = MA0004row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE3.Text = MA0004row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE4.Text = MA0004row("SHARYOTYPE") OrElse
                                        work.WF_SEL_SHARYOTYPE5.Text = MA0004row("SHARYOTYPE") Then
                                        WW_SELECT_SHAYO = 1
                                    End If
                                End If

                                If WW_SELECT_SHAYO = 0 Then
                                    MA0004row("SELECT") = 0
                                End If
                            End If

                            '○抽出対象外の場合、名称取得、レコード追加しない
                            If MA0004row("SELECT") = 1 Then
                                '○名称付与
                                MA0004row("MANGMORGNAME") = ""
                                CODENAME_get("MANGMORG", MA0004row("MANGMORG"), MA0004row("MANGMORGNAME"), WW_DUMMY)                '管理部署名
                                MA0004row("MANGSORGNAME") = ""
                                CODENAME_get("MANGSORG", MA0004row("MANGSORG"), MA0004row("MANGSORGNAME"), WW_DUMMY)                '設置部署名
                                MA0004row("MANGOILTYPENAME") = ""
                                CODENAME_get("MANGOILTYPE", MA0004row("MANGOILTYPE"), MA0004row("MANGOILTYPENAME"), WW_DUMMY)       '油種名
                                MA0004row("MANGOWNCODENAME") = ""
                                CODENAME_get("MANGOWNCODE", MA0004row("MANGOWNCODE"), MA0004row("MANGOWNCODENAME"), WW_DUMMY)       '荷主名
                                MA0004row("MANGOWNCONTNAME") = ""
                                CODENAME_get("MANGOWNCONT", MA0004row("MANGOWNCONT"), MA0004row("MANGOWNCONTNAME"), WW_DUMMY)       '契約区分名
                                MA0004row("MANGSUPPLNAME") = ""
                                CODENAME_get("MANGSUPPL", MA0004row("MANGSUPPL"), MA0004row("MANGSUPPLNAME"), WW_DUMMY)             '庸車会社名
                                MA0004row("MANGUORGNAME") = ""
                                CODENAME_get("MANGUORG", MA0004row("MANGUORG"), MA0004row("MANGUORGNAME"), WW_DUMMY)                '運用部署名
                                MA0004row("BASELEASENAME") = ""
                                CODENAME_get("BASELEASE", MA0004row("BASELEASE"), MA0004row("BASELEASENAME"), WW_DUMMY)             '車両所有名
                                MA0004row("FCTRAXLENAME") = ""
                                CODENAME_get("FCTRAXLE", MA0004row("FCTRAXLE"), MA0004row("FCTRAXLENAME"), WW_DUMMY)                'リフトアクスル名
                                MA0004row("FCTRTMAKERNAME") = ""
                                MA0004row("FCTRDPRNAME") = ""
                                CODENAME_get("FCTRDPR", MA0004row("FCTRDPR"), MA0004row("FCTRDPRNAME"), WW_DUMMY)                   'DPR名
                                MA0004row("FCTRFUELMATENAME") = ""
                                CODENAME_get("FCTRFUELMATE", MA0004row("FCTRFUELMATE"), MA0004row("FCTRFUELMATENAME"), WW_DUMMY)    '燃料タンク材質名
                                MA0004row("FCTRSHFTNUMNAME") = ""
                                CODENAME_get("FCTRSHFTNUM", MA0004row("FCTRSHFTNUM"), MA0004row("FCTRSHFTNUMNAME"), WW_DUMMY)       '軸数名
                                MA0004row("FCTRSUSPNAME") = ""
                                CODENAME_get("FCTRSUSP", MA0004row("FCTRSUSP"), MA0004row("FCTRSUSPNAME"), WW_DUMMY)                'サスペンション種類名
                                MA0004row("FCTRTMISSIONNAME") = ""
                                CODENAME_get("FCTRTMISSION", MA0004row("FCTRTMISSION"), MA0004row("FCTRTMISSIONNAME"), WW_DUMMY)    'ミッション名
                                MA0004row("FCTRUREANAME") = ""
                                CODENAME_get("FCTRUREA", MA0004row("FCTRUREA"), MA0004row("FCTRUREANAME"), WW_DUMMY)                '尿素名
                                MA0004row("OTNKBPIPENAME") = ""
                                CODENAME_get("OTNKBPIPE", MA0004row("OTNKBPIPE"), MA0004row("OTNKBPIPENAME"), WW_DUMMY)             '後配管名
                                MA0004row("OTNKVAPORNAME") = ""
                                CODENAME_get("OTNKVAPOR", MA0004row("OTNKVAPOR"), MA0004row("OTNKVAPORNAME"), WW_DUMMY)             'ベーパー名
                                MA0004row("OTNKCVALVENAME") = ""
                                CODENAME_get("OTNKCVALVE", MA0004row("OTNKCVALVE"), MA0004row("OTNKCVALVENAME"), WW_DUMMY)          '中間ﾊﾞﾙﾌﾞ有無名
                                MA0004row("OTNKDCDNAME") = ""
                                CODENAME_get("OTNKDCD", MA0004row("OTNKDCD"), MA0004row("OTNKDCDNAME"), WW_DUMMY)                   'ＤＣＤ装備名
                                MA0004row("FCTRSMAKERNAME") = ""
                                CODENAME_get("FCTRSMAKER", MA0004row("FCTRSMAKER"), MA0004row("FCTRSMAKERNAME"), WW_DUMMY)          '車両メーカー
                                MA0004row("OTNKDETECTORNAME") = ""
                                CODENAME_get("OTNKDETECTOR", MA0004row("OTNKDETECTOR"), MA0004row("OTNKDETECTORNAME"), WW_DUMMY)    '検水管名
                                MA0004row("OTNKDISGORGENAME") = ""
                                CODENAME_get("OTNKDISGORGE", MA0004row("OTNKDISGORGE"), MA0004row("OTNKDISGORGENAME"), WW_DUMMY)    '吐出口名
                                MA0004row("OTNKHTECHNAME") = ""
                                CODENAME_get("OTNKHTECH", MA0004row("OTNKHTECH"), MA0004row("OTNKHTECHNAME"), WW_DUMMY)             'ハイテク種別名
                                MA0004row("OTNKLVALVENAME") = ""
                                CODENAME_get("OTNKLVALVE", MA0004row("OTNKLVALVE"), MA0004row("OTNKLVALVENAME"), WW_DUMMY)          '底弁形式名
                                MA0004row("OTNKMATERIALNAME") = ""
                                CODENAME_get("OTNKMATERIAL", MA0004row("OTNKMATERIAL"), MA0004row("OTNKMATERIALNAME"), WW_DUMMY)    'タンク材質名
                                MA0004row("OTNKPIPENAME") = ""
                                CODENAME_get("OTNKPIPE", MA0004row("OTNKPIPE"), MA0004row("OTNKPIPENAME"), WW_DUMMY)                '配管形態名
                                MA0004row("OTNKPIPESIZENAME") = ""
                                CODENAME_get("OTNKPIPESIZE", MA0004row("OTNKPIPESIZE"), MA0004row("OTNKPIPESIZENAME"), WW_DUMMY)    '配管サイズ名
                                MA0004row("OTNKPUMPNAME") = ""
                                CODENAME_get("OTNKPUMP", MA0004row("OTNKPUMP"), MA0004row("OTNKPUMPNAME"), WW_DUMMY)                'ポンプ名
                                MA0004row("HPRSPMPDRNAME") = ""
                                CODENAME_get("HPRSPMPDR", MA0004row("HPRSPMPDR"), MA0004row("HPRSPMPDRNAME"), WW_DUMMY)             'ポンプ駆動方法
                                MA0004row("HPRSINSULATENAME") = ""
                                CODENAME_get("HPRSINSULATE", MA0004row("HPRSINSULATE"), MA0004row("HPRSINSULATENAME"), WW_DUMMY)    '断熱構造名
                                MA0004row("HPRSMATRNAME") = ""
                                CODENAME_get("HPRSMATR", MA0004row("HPRSMATR"), MA0004row("HPRSMATRNAME"), WW_DUMMY)                'タンク材質名
                                MA0004row("HPRSPIPENAME") = ""
                                CODENAME_get("HPRSPIPE", MA0004row("HPRSPIPE"), MA0004row("HPRSPIPENAME"), WW_DUMMY)                '配管形状（仮）名
                                MA0004row("HPRSPIPENUMNAME") = ""
                                CODENAME_get("HPRSPIPENUM", MA0004row("HPRSPIPENUM"), MA0004row("HPRSPIPENUMNAME"), WW_DUMMY)       '配管口数名
                                MA0004row("HPRSPUMPNAME") = ""
                                CODENAME_get("HPRSPUMP", MA0004row("HPRSPUMP"), MA0004row("HPRSPUMPNAME"), WW_DUMMY)                'ポンプ名
                                MA0004row("HPRSRESSRENAME") = ""
                                CODENAME_get("HPRSRESSRE", MA0004row("HPRSRESSRE"), MA0004row("HPRSRESSRENAME"), WW_DUMMY)          '加圧器名
                                MA0004row("HPRSSTRUCTNAME") = ""
                                CODENAME_get("HPRSSTRUCT", MA0004row("HPRSSTRUCT"), MA0004row("HPRSSTRUCTNAME"), WW_DUMMY)          'タンク構造名
                                MA0004row("HPRSVALVENAME") = ""
                                CODENAME_get("HPRSVALVE", MA0004row("HPRSVALVE"), MA0004row("HPRSVALVENAME"), WW_DUMMY)             '底弁形式名
                                MA0004row("CHEMDISGORGENAME") = ""
                                CODENAME_get("CHEMDISGORGE", MA0004row("CHEMDISGORGE"), MA0004row("CHEMDISGORGENAME"), WW_DUMMY)    '吐出口名
                                MA0004row("CHEMHOSENAME") = ""
                                CODENAME_get("CHEMHOSE", MA0004row("CHEMHOSE"), MA0004row("CHEMHOSENAME"), WW_DUMMY)                'ホースボックス名
                                MA0004row("CHEMMANOMTRNAME") = ""
                                CODENAME_get("CHEMMANOMTR", MA0004row("CHEMMANOMTR"), MA0004row("CHEMMANOMTRNAME"), WW_DUMMY)       '圧力計名
                                MA0004row("CHEMMATERIALNAME") = ""
                                CODENAME_get("CHEMMATERIAL", MA0004row("CHEMMATERIAL"), MA0004row("CHEMMATERIALNAME"), WW_DUMMY)    'タンク材質名
                                MA0004row("CHEMPMPDRNAME") = ""
                                CODENAME_get("CHEMPMPDR", MA0004row("CHEMPMPDR"), MA0004row("CHEMPMPDRNAME"), WW_DUMMY)             'ポンプ駆動方法名
                                MA0004row("CHEMPRESDRVNAME") = ""
                                CODENAME_get("CHEMPRESDRV", MA0004row("CHEMPRESDRV"), MA0004row("CHEMPRESDRVNAME"), WW_DUMMY)       '加温装置名
                                MA0004row("CHEMPRESEQNAME") = ""
                                CODENAME_get("CHEMPRESEQ", MA0004row("CHEMPRESEQ"), MA0004row("CHEMPRESEQNAME"), WW_DUMMY)          '均圧配管名
                                MA0004row("CHEMPUMPNAME") = ""
                                CODENAME_get("CHEMPUMP", MA0004row("CHEMPUMP"), MA0004row("CHEMPUMPNAME"), WW_DUMMY)                'ポンプ名
                                MA0004row("CHEMSTRUCTNAME") = ""
                                CODENAME_get("CHEMSTRUCT", MA0004row("CHEMSTRUCT"), MA0004row("CHEMSTRUCTNAME"), WW_DUMMY)          'タンク構造名
                                MA0004row("CHEMTHERMNAME") = ""
                                CODENAME_get("CHEMTHERM", MA0004row("CHEMTHERM"), MA0004row("CHEMTHERMNAME"), WW_DUMMY)             '温度計名
                                MA0004row("OTHRBMONITORNAME") = ""
                                CODENAME_get("OTHRBMONITOR", MA0004row("OTHRBMONITOR"), MA0004row("OTHRBMONITORNAME"), WW_DUMMY)    'バックモニター名
                                MA0004row("OTHRBSONARNAME") = ""
                                CODENAME_get("OTHRBSONAR", MA0004row("OTHRBSONAR"), MA0004row("OTHRBSONARNAME"), WW_DUMMY)          'バックソナー名
                                MA0004row("FCTRTIRENAME") = ""
                                CODENAME_get("FCTRTIRE", MA0004row("FCTRTIRE"), MA0004row("FCTRTIRENAME"), WW_DUMMY)                'タイヤメーカー名
                                MA0004row("OTHRDRRECORDNAME") = ""
                                CODENAME_get("OTHRDRRECORD", MA0004row("OTHRDRRECORD"), MA0004row("OTHRDRRECORDNAME"), WW_DUMMY)    'ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ名
                                MA0004row("OTHRPAINTINGNAME") = ""
                                CODENAME_get("OTHRPAINTING", MA0004row("OTHRPAINTING"), MA0004row("OTHRPAINTINGNAME"), WW_DUMMY)    '塗装名
                                MA0004row("OTHRRADIOCONNAME") = ""
                                CODENAME_get("OTHRRADIOCON", MA0004row("OTHRRADIOCON"), MA0004row("OTHRRADIOCONNAME"), WW_DUMMY)    '無線（有・無）名
                                MA0004row("OTHRRTARGETNAME") = ""
                                CODENAME_get("OTHRRTARGET", MA0004row("OTHRRTARGET"), MA0004row("OTHRRTARGETNAME"), WW_DUMMY)       '一括修理非対象車名
                                MA0004row("OTHRTERMINALNAME") = ""
                                CODENAME_get("OTHRTERMINAL", MA0004row("OTHRTERMINAL"), MA0004row("OTHRTERMINALNAME"), WW_DUMMY)    '車載端末名
                                MA0004row("MANGPROD1NAME") = ""
                                CODENAME_get("MANGPROD1", MA0004row("MANGPROD1"), MA0004row("MANGPROD1NAME"), WW_DUMMY, {CStr(MA0004row("MANGOILTYPE"))})             '品名１
                                MA0004row("MANGPROD2NAME") = ""
                                CODENAME_get("MANGPROD2", MA0004row("MANGPROD2"), MA0004row("MANGPROD2NAME"), WW_DUMMY, {CStr(MA0004row("MANGOILTYPE")), CStr(MA0004row("MANGPROD1"))})    '品名２
                                MA0004row("OTNKEXHASIZENAME") = ""
                                CODENAME_get("OTNKEXHASIZE", MA0004row("OTNKEXHASIZE"), MA0004row("OTNKEXHASIZENAME"), WW_DUMMY)    '吐出口サイズ
                                MA0004row("HPRSHOSENAME") = ""
                                CODENAME_get("HPRSHOSE", MA0004row("HPRSHOSE"), MA0004row("HPRSHOSENAME"), WW_DUMMY)                'ホースボックス
                                MA0004row("CONTSHAPENAME") = ""
                                CODENAME_get("CONTSHAPE", MA0004row("CONTSHAPE"), MA0004row("CONTSHAPENAME"), WW_DUMMY)             'シャーシ形状
                                MA0004row("CONTPUMPNAME") = ""
                                CODENAME_get("CONTPUMP", MA0004row("CONTPUMP"), MA0004row("CONTPUMPNAME"), WW_DUMMY)                'ポンプ
                                MA0004row("CONTPMPDRNAME") = ""
                                CODENAME_get("CONTPMPDR", MA0004row("CONTPMPDR"), MA0004row("CONTPMPDRNAME"), WW_DUMMY)             'ポンプ駆動方法
                                MA0004row("OTHRTPMSNAME") = ""
                                CODENAME_get("OTHRTPMS", MA0004row("OTHRTPMS"), MA0004row("OTHRTPMSNAME"), WW_DUMMY)                'TPMS
                                MA0004row("OTNKTMAKERNAME") = ""
                                CODENAME_get("OTNKTMAKER", MA0004row("OTNKTMAKER"), MA0004row("OTNKTMAKERNAME"), WW_DUMMY)          '石油タンクメーカー名
                                MA0004row("HPRSTMAKERNAME") = ""
                                CODENAME_get("HPRSTMAKER", MA0004row("HPRSTMAKER"), MA0004row("HPRSTMAKERNAME"), WW_DUMMY)          '高圧タンクメーカー名
                                MA0004row("CHEMTMAKERNAME") = ""
                                CODENAME_get("CHEMTMAKER", MA0004row("CHEMTMAKER"), MA0004row("CHEMTMAKERNAME"), WW_DUMMY)          '化成品タンクメーカー名
                                MA0004row("CONTTMAKERNAME") = ""
                                CODENAME_get("CONTTMAKER", MA0004row("CONTTMAKER"), MA0004row("CONTTMAKERNAME"), WW_DUMMY)          'コンテナタンクメーカー名
                                MA0004row("SHARYOSTATUSNAME") = ""
                                CODENAME_get("SHARYOSTATUS", MA0004row("SHARYOSTATUS"), MA0004row("SHARYOSTATUSNAME"), WW_DUMMY)
                                MA0004row("INSKBNNAME") = ""
                                CODENAME_get("INSKBN", MA0004row("INSKBN"), MA0004row("INSKBNNAME"), WW_DUMMY)                      '検査区分名

                                MA0004tbl.Rows.Add(MA0004row)
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
        CS0026TBLSORT.TABLE = MA0004tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MA0004tbl = CS0026TBLSORT.TABLE
        End If

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

        Dim WW_STYMD_Yuko As Date      '有効期限(開始)
        Dim WW_ENDYMD_Yuko As Date     '有効期限(終了)
        Dim WW_STYMD_Nendo As Date     '
        Dim WW_ENDYMD_Nendo As Date    '
        Dim WW_TEXT As String
        Dim WW_int As Integer

        '○有効期限(開始)
        WW_TEXT = work.WF_SEL_YYF.Text & "/4/1"
        Try
            Date.TryParse(WW_TEXT, WW_STYMD_Yuko)
        Catch ex As Exception
            WW_STYMD_Yuko = "2000/4/1"
        End Try

        '○有効期限(終了)
        Try
            Integer.TryParse(work.WF_SEL_YYT.Text, WW_int)
            WW_TEXT = (WW_int + 1).ToString() & "/03/31"

            Date.TryParse(WW_TEXT, WW_ENDYMD_Yuko)
        Catch ex As Exception
            WW_ENDYMD_Yuko = "2099/03/31"
        End Try

        '○対象年度(開始)
        WW_TEXT = work.WF_SEL_YYF.Text & "/04/01"
        Try
            Date.TryParse(WW_TEXT, WW_STYMD_Nendo)
        Catch ex As Exception
            WW_STYMD_Nendo = "2000/04/01"
        End Try

        '○対象年度((終了))
        Try
            Integer.TryParse(work.WF_SEL_YYT.Text, WW_int)
            WW_TEXT = (WW_int + 1).ToString() & "/03/31"

            Date.TryParse(WW_TEXT, WW_ENDYMD_Nendo)
        Catch ex As Exception
            WW_ENDYMD_Nendo = "2099/03/31"
        End Try

        For Each MA0004INProw As DataRow In MA0004INPtbl.Rows

            WW_LINEERR_SW = ""

            '○単項目チェック(ヘッダー情報)

            '・キー項目(会社：CAMPCODE)
            WW_TEXT = MA0004INProw("CAMPCODE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MA0004INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0004INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MA0004INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(統一車番：SHARYOTYPE)…車両タイプ
            WW_TEXT = MA0004INProw("SHARYOTYPE")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE", MA0004INProw("SHARYOTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0004INProw("SHARYOTYPE") = ""
                Else
                    CODENAME_get("SHARYOTYPE", MA0004INProw("SHARYOTYPE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番・車両タイプエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(統一車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(統一車番：TSHABAN)…連番
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TSHABAN", MA0004INProw("TSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT, True)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(統一車番・連番エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・キー項目(有効年月日：STYMD)
            Dim WW_STYMD_ERR As String = "OFF"
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", MA0004INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR

                WW_STYMD_ERR = "ON"
            End If

            '・キー項目(有効年月日：ENDYMD)
            Dim WW_ENDYMD_ERR As String = "OFF"
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MA0004INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR

                WW_ENDYMD_ERR = "ON"
            End If

            '・キー項目(削除フラグ：DELFLG)
            WW_TEXT = MA0004INProw("DELFLG")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MA0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MA0004INProw("DELFLG") = ""
                Else
                    CODENAME_get("DELFLG", MA0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                        WW_CheckMES2 = " マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '○関連チェック(キー情報)
            '統一車番存在チェック
            If MA0004tbl.Rows.Count = 0 Then
                WW_CheckMES1 = "・更新できないレコード(該当レコード無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            For i As Integer = 0 To MA0004tbl.Rows.Count - 1
                If MA0004tbl.Rows(i)("SHARYOTYPE") = MA0004INProw("SHARYOTYPE") AndAlso
                   MA0004tbl.Rows(i)("TSHABAN") = MA0004INProw("TSHABAN") Then
                    Exit For
                Else
                    If i >= (MA0004tbl.Rows.Count - 1) Then
                        WW_CheckMES1 = "・更新できないレコード(該当レコード無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Next

            '指定年度範囲チェック
            If MA0004INProw("STYMD") <> "" AndAlso WW_STYMD_ERR = "OFF" Then
                If WW_STYMD_Yuko <= MA0004INProw("STYMD") AndAlso
                   WW_ENDYMD_Yuko >= MA0004INProw("STYMD") Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(開始日付が選択期間外)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            If MA0004INProw("ENDYMD") <> "" AndAlso WW_ENDYMD_ERR = "OFF" Then
                If WW_STYMD_Yuko <= MA0004INProw("ENDYMD") AndAlso
                   WW_ENDYMD_Yuko >= MA0004INProw("ENDYMD") Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(終了日付が選択期間外)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '年度跨りチェック
            If MA0004INProw("ENDYMD") <> "" AndAlso WW_ENDYMD_ERR = "OFF" Then
                If WW_ENDYMD_Nendo < MA0004INProw("ENDYMD") Then
                    WW_CheckMES1 = "・更新できないレコード(終了日付が年度跨り)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '権限チェック（更新権限）
            If MA0004INProw("MANGMORG") <> "" OrElse MA0004INProw("MANGSORG") <> "" Then

                '管理部署
                CS0025AUTHORget.USERID = CS0050Session.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MA0004INProw("MANGMORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    '設置部署
                    CS0025AUTHORget.USERID = CS0050Session.USERID
                    CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                    CS0025AUTHORget.CODE = MA0004INProw("MANGSORG")
                    CS0025AUTHORget.STYMD = Date.Now
                    CS0025AUTHORget.ENDYMD = Date.Now
                    CS0025AUTHORget.CS0025AUTHORget()
                    If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                    Else
                        WW_CheckMES1 = "・更新できないレコード(権限無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                        WW_LINEERR_SW = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If


            '○単項目チェック(明細情報)

            '・明細項目(自動車税金額：TAXATAX)
            '①必須・項目属性チェック
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TAXATAX", MA0004INProw("TAXATAX"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("TAXATAX") = Format(CInt(MA0004INProw("TAXATAX")), "#0")
                Catch ex As Exception
                    MA0004INProw("TAXATAX") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（自動車税金額エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(自賠責料金：TAXLINS)
            '①必須・項目属性チェック
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TAXLINS", MA0004INProw("TAXLINS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("TAXLINS") = Format(CInt(MA0004INProw("TAXLINS")), "#0")
                Catch ex As Exception
                    MA0004INProw("TAXLINS") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（自賠責料金エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(車検有効期限年月日：LICNYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNYMD", MA0004INProw("LICNYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("LICNYMD") = "" Then
                    MA0004INProw("LICNYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（車検有効期限年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(登録番号(陸運局)：LICNPLTNO1)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNPLTNO1", MA0004INProw("LICNPLTNO1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（登録番号(陸運局)エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(車体番号：LICNFRAMENO)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNFRAMENO", MA0004INProw("LICNFRAMENO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（車体番号エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(型式：LICNMODEL)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNMODEL", MA0004INProw("LICNMODEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（型式エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(最大積載量：LICNLDCAPA)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNLDCAPA", MA0004INProw("LICNLDCAPA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("LICNLDCAPA") = Format(CInt(MA0004INProw("LICNLDCAPA")), "#0")
                Catch ex As Exception
                    MA0004INProw("LICNLDCAPA") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（最大積載量エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(第５輪荷重：LICN5LDCAPA)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICN5LDCAPA", MA0004INProw("LICN5LDCAPA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("LICN5LDCAPA") = Format(CInt(MA0004INProw("LICN5LDCAPA")), "#0")
                Catch ex As Exception
                    MA0004INProw("LICN5LDCAPA") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（第５輪荷重エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(石油気密検査年月日：OTNKTINSYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OTNKTINSYMD", MA0004INProw("OTNKTINSYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("OTNKTINSYMD") = "" Then
                    MA0004INProw("OTNKTINSYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（石油気密検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(化成気密検査年月日：CHEMTINSYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CHEMTINSYMD", MA0004INProw("CHEMTINSYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("CHEMTINSYMD") = "" Then
                    MA0004INProw("CHEMTINSYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（化成気密検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(高圧容器再検査年月日：HPRSINSYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "HPRSINSYMD", MA0004INProw("HPRSINSYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("HPRSINSYMD") = "" Then
                    MA0004INProw("HPRSINSYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（高圧容器再検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(重量税金額：TAXVTAX)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TAXVTAX", MA0004INProw("TAXVTAX"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("TAXVTAX") = Format(CInt(MA0004INProw("TAXVTAX")), "#0")
                Catch ex As Exception
                    MA0004INProw("TAXVTAX") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（重量税金額エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(自賠責期限年月日：TAXLINSYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TAXLINSYMD", MA0004INProw("TAXLINSYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("TAXLINSYMD") = "" Then
                    MA0004INProw("TAXLINSYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード（自賠責期限年月日エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(登録番号：LICNPLTNO2)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNPLTNO2", MA0004INProw("LICNPLTNO2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（登録番号エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(車名：LICNMNFACT)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNMNFACT", MA0004INProw("LICNMNFACT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（車名エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(原動機の型式：LICNMOTOR)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNMOTOR", MA0004INProw("LICNMOTOR"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・エラーが存在します。（原動機の型式エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(車両総重量：LICNTWEIGHT)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNTWEIGHT", MA0004INProw("LICNTWEIGHT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("LICNTWEIGHT") = Format(CInt(MA0004INProw("LICNTWEIGHT")), "#0")
                Catch ex As Exception
                    MA0004INProw("LICNTWEIGHT") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（車両総重量エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(連結車両総重量：LICNCWEIGHT)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNCWEIGHT", MA0004INProw("LICNCWEIGHT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("LICNCWEIGHT") = Format(CInt(MA0004INProw("LICNCWEIGHT")), "#0")
                Catch ex As Exception
                    MA0004INProw("LICNCWEIGHT") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（連結車両総重量エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(石油次回気密検査年月日：OTNKTINSNYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OTNKTINSNYMD", MA0004INProw("OTNKTINSNYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("OTNKTINSNYMD") = "" Then
                    MA0004INProw("OTNKTINSNYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（石油次回気密検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(化成次回気密検査年月日：CHEMTINSNYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CHEMTINSNYMD", MA0004INProw("CHEMTINSNYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("CHEMTINSNYMD") = "" Then
                    MA0004INProw("CHEMTINSNYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（化成次回気密検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(高圧次回容器再検査年月日：HPRSINSNYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "HPRSINSNYMD", MA0004INProw("HPRSINSNYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("HPRSINSNYMD") = "" Then
                    MA0004INProw("HPRSINSNYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（高圧次回容器再検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(車両重量：LICNWEIGHT)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "LICNWEIGHT", MA0004INProw("LICNWEIGHT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    MA0004INProw("LICNWEIGHT") = Format(CInt(MA0004INProw("LICNWEIGHT")), "#0")
                Catch ex As Exception
                    MA0004INProw("LICNWEIGHT") = "0"
                End Try
            Else
                WW_CheckMES1 = "・更新できないレコード（車両重量エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(高圧定期自主検査年月日：HPRSJINSYMD)
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "HPRSJINSYMD", MA0004INProw("HPRSJINSYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If MA0004INProw("HPRSJINSYMD") = "" Then
                    MA0004INProw("HPRSJINSYMD") = C_DEFAULT_YMD
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（高圧定期自主検査年月日エラー）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '・明細項目(検査区分：INSKBN)
            If MA0004INProw("INSKBN") = "" Then
                MA0004INProw("INSKBN") = "0"
            End If
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "INSKBN", MA0004INProw("INSKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード（検査区分エラー）です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                WW_LINEERR_SW = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINEERR_SW = "" Then
                If MA0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        For Each MA0004INProw As DataRow In MA0004tbl.Rows

            '読み飛ばし
            If (MA0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MA0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MA0004INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MA0004INProw("STYMD") = "" Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            '期間重複チェック
            For Each MA0004row As DataRow In MA0004tbl.Rows

                '同一KEY以外は読み飛ばし
                If MA0004row("SHARYOTYPE") = MA0004INProw("SHARYOTYPE") AndAlso
                    MA0004row("TSHABAN") = MA0004INProw("TSHABAN") AndAlso
                    MA0004row("DELFLG") <> C_DELETE_FLG.DELETE AndAlso
                    MA0004INProw("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MA0004row("STYMD") = MA0004INProw("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MA0004INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(MA0004INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MA0004row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MA0004row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MA0004INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If
            Next

            If WW_LINEERR_SW = "" Then
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        Dim MA0004View As New DataView(MA0004tbl)
        Dim MA0004ViewRow As DataRow = MA0004tbl.NewRow
        MA0004View.Sort = "CAMPCODE, SHARYOTYPE, TSHABAN, STYMD"
        MA0004View.RowFilter = "DELFLG <> '1'"

        'チェック対象KEY抽出(CAMPCODE + SHARYOTYPE + TSHABAN)
        For i As Integer = 0 To MA0004View.Count - 1
            MA0004ViewRow = MA0004View.Item(i).Row

            If InStr(MA0004ViewRow("OPERATION"), C_LIST_OPERATION_CODE.UPDATING) > 0 AndAlso MA0004ViewRow("TSHABAN") <> "新" Then

                WW_KEY = MA0004ViewRow("CAMPCODE") & "_" & MA0004ViewRow("SHARYOTYPE") & MA0004ViewRow("TSHABAN")
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

        For i As Integer = 0 To MA0004View.Count - 1
            MA0004ViewRow = MA0004View.Item(i).Row

            If MA0004ViewRow("STYMD") = Nothing OrElse MA0004ViewRow("ENDYMD") = Nothing Then
                Continue For
            End If

            '○チェック対象レコード内容
            Try
                WW_KEY = MA0004ViewRow("CAMPCODE") & "_" & MA0004ViewRow("SHARYOTYPE") & MA0004ViewRow("TSHABAN")
                Date.TryParse(MA0004ViewRow("STYMD"), WW_STYMD)
                Date.TryParse(MA0004ViewRow("ENDYMD"), WW_ENDYMD)
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
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & MA0004ViewRow("CAMPCODE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & MA0004ViewRow("SHARYOTYPE") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & MA0004ViewRow("TSHABAN") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & MA0004ViewRow("STYMD") & " , "
                WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & MA0004ViewRow("ENDYMD") & " , "
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

                For Each MA0004row As DataRow In MA0004tbl.Rows
                    If InStr(MA0004row("OPERATION"), C_LIST_OPERATION_CODE.UPDATING) > 0 Then
                        If MA0004row("CAMPCODE") = MA0004ViewRow("CAMPCODE") AndAlso
                            MA0004row("SHARYOTYPE") = MA0004ViewRow("SHARYOTYPE") AndAlso
                            MA0004row("TSHABAN") = MA0004ViewRow("TSHABAN") Then

                            MA0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                        End If
                    End If
                Next

                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR

            End If

            WW_BrYMD = WW_ENDYMD

        Next

    End Sub

    'Protected Sub DATE_RELATION_CHK2()

    '    Dim WW_DATEF As Date
    '    Dim WW_DATET As Date
    '    Dim WW_DATE As Date

    '    '対象キーの抽出（操作が"更新"のもの）
    '    Dim WW_WorkView As New DataView(MA0004tbl)
    '    WW_WorkView.Sort = "CAMPCODE, SHARYOTYPE, TSHABAN, STYMD"
    '    WW_WorkView.RowFilter = "OPERATION in ('更新', '★更新') and DELFLG <> '1'"
    '    '同一キーを集約し、ワークテーブルを作成
    '    Dim WW_TBL As DataTable = WW_WorkView.ToTable("WW_TBL", True, "CAMPCODE", "SHARYOTYPE", "TSHABAN")

    '    'チェック対象データのソート
    '    Dim WW_TBLView As New DataView(MA0004tbl)
    '    WW_TBLView.Sort = "CAMPCODE, SHARYOTYPE, TSHABAN, STYMD"
    '    WW_TBLView.RowFilter = "DELFLG <> '1'"

    '    '日付連続性チェック
    '    For j As Integer = 0 To WW_TBLView.Count - 2
    '        For i As Integer = 0 To WW_TBL.Rows.Count - 1
    '            If WW_TBLView.Item(j).Row.Item("CAMPCODE") = WW_TBL.Rows(i)("CAMPCODE") AndAlso
    '                    WW_TBLView.Item(j).Row.Item("SHARYOTYPE") = WW_TBL.Rows(i)("SHARYOTYPE") AndAlso
    '                    WW_TBLView.Item(j).Row.Item("TSHABAN") = WW_TBL.Rows(i)("TSHABAN") AndAlso
    '                    WW_TBLView.Item(j).Row.Item("TSHABAN") <> "" Then

    '                If WW_TBLView.Item(j).Row.Item("CAMPCODE") = WW_TBLView.Item(j + 1).Row.Item("CAMPCODE") AndAlso
    '                        WW_TBLView.Item(j).Row.Item("SHARYOTYPE") = WW_TBLView.Item(j + 1).Row.Item("SHARYOTYPE") AndAlso
    '                        WW_TBLView.Item(j).Row.Item("TSHABAN") = WW_TBLView.Item(j + 1).Row.Item("TSHABAN") Then

    '                    Date.TryParse(WW_TBLView.Item(j + 1).Row.Item("STYMD"), WW_DATEF)
    '                    Date.TryParse(WW_TBLView.Item(j).Row.Item("ENDYMD"), WW_DATE)
    '                    WW_DATET = DateAdd("d", 1, WW_DATE)

    '                    If WW_DATEF <> WW_DATET Then
    '                        Dim WW_ERR_MES As String = ""
    '                        WW_ERR_MES = "・更新できないレコードです。(開始、終了年月日が連続していません)"
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & WW_TBLView.Item(j).Row.Item("CAMPCODE") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & WW_TBLView.Item(j).Row.Item("SHARYOTYPE") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & WW_TBLView.Item(j).Row.Item("TSHABAN") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & WW_TBLView.Item(j).Row.Item("STYMD") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & WW_TBLView.Item(j).Row.Item("ENDYMD") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & WW_TBLView.Item(j + 1).Row.Item("CAMPCODE") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & WW_TBLView.Item(j + 1).Row.Item("SHARYOTYPE") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & WW_TBLView.Item(j + 1).Row.Item("TSHABAN") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & WW_TBLView.Item(j + 1).Row.Item("STYMD") & " , "
    '                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & WW_TBLView.Item(j + 1).Row.Item("ENDYMD") & " , "
    '                        WF_ERR_REPORT.Text = WF_ERR_REPORT.Text & ControlChars.NewLine & WW_ERR_MES
    '                        If WW_TBLView.Item(j).Row.Item("OPERATION") = "更新" Then
    '                            WW_TBLView.Item(j).Row.Item("OPERATION") = "エラー"
    '                        End If
    '                        If WW_TBLView.Item(j + 1).Row.Item("OPERATION") = "更新" Then
    '                            WW_TBLView.Item(j + 1).Row.Item("OPERATION") = "エラー"
    '                        End If
    '                    End If
    '                End If
    '            End If
    '        Next
    '    Next

    '    For i As Integer = 0 To MA0004tbl.Rows.Count - 1
    '        For j As Integer = 0 To WW_TBLView.Count - 1
    '            If MA0004tbl.Rows(i)("CAMPCODE") = WW_TBLView.Item(j).Row.Item("CAMPCODE") AndAlso
    '                MA0004tbl.Rows(i)("SHARYOTYPE") = WW_TBLView.Item(j).Row.Item("SHARYOTYPE") AndAlso
    '                MA0004tbl.Rows(i)("TSHABAN") = WW_TBLView.Item(j).Row.Item("TSHABAN") AndAlso
    '                MA0004tbl.Rows(i)("STYMD") = WW_TBLView.Item(j).Row.Item("STYMD") AndAlso
    '                MA0004tbl.Rows(i)("ENDYMD") = WW_TBLView.Item(j).Row.Item("ENDYMD") Then
    '                MA0004tbl.Rows(i)("OPERATION") = WW_TBLView.Item(j).Row.Item("OPERATION")
    '            End If
    '        Next
    '    Next

    'End Sub

    ''' <summary>
    ''' MA0004tbl更新
    ''' </summary>
    ''' <param name="I_EXCEL"></param>
    ''' <param name="RTN"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPD(ByRef I_EXCEL As String, ByRef RTN As String)

        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each MA0004row As DataRow In MA0004tbl.Rows
            Select Case MA0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MA0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        Dim MA0004INProw As DataRow = MA0004INPtbl.NewRow
        For i As Integer = 0 To MA0004INPtbl.Rows.Count - 1

            MA0004INProw.ItemArray = MA0004INPtbl.Rows(i).ItemArray

            If MA0004INProw("LICNLDCAPA") = "" Then
                MA0004INProw("LICNLDCAPA") = "0"
            End If
            If MA0004INProw("LICN5LDCAPA") = "" Then
                MA0004INProw("LICN5LDCAPA") = "0"
            End If
            If MA0004INProw("LICNWEIGHT") = "" Then
                MA0004INProw("LICNWEIGHT") = "0"
            End If
            If MA0004INProw("LICNTWEIGHT") = "" Then
                MA0004INProw("LICNTWEIGHT") = "0"
            End If
            If MA0004INProw("LICNCWEIGHT") = "" Then
                MA0004INProw("LICNCWEIGHT") = "0"
            End If
            If MA0004INProw("TAXLINS") = "" Then
                MA0004INProw("TAXLINS") = "0"
            End If
            If MA0004INProw("TAXVTAX") = "" Then
                MA0004INProw("TAXVTAX") = "0"
            End If
            If MA0004INProw("TAXATAX") = "" Then
                MA0004INProw("TAXATAX") = "0"
            End If
            If MA0004INProw("LICNLDCAPA") = "" Then
                MA0004INProw("LICNLDCAPA") = "0"
            End If
            If MA0004INProw("LICN5LDCAPA") = "" Then
                MA0004INProw("LICN5LDCAPA") = "0"
            End If
            If MA0004INProw("LICNWEIGHT") = "" Then
                MA0004INProw("LICNWEIGHT") = "0"
            End If
            If MA0004INProw("LICNTWEIGHT") = "" Then
                MA0004INProw("LICNTWEIGHT") = "0"
            End If
            If MA0004INProw("LICNCWEIGHT") = "" Then
                MA0004INProw("LICNCWEIGHT") = "0"
            End If
            If MA0004INProw("TAXLINS") = "" Then
                MA0004INProw("TAXLINS") = "0"
            End If
            If MA0004INProw("TAXVTAX") = "" Then
                MA0004INProw("TAXVTAX") = "0"
            End If
            If MA0004INProw("TAXATAX") = "" Then
                MA0004INProw("TAXATAX") = "0"
            End If

            If MA0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                MA0004INProw("OPERATION") = "Insert"
                For Each MA0004row As DataRow In MA0004tbl.Rows

                    'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                    If MA0004row("CAMPCODE") = MA0004INProw("CAMPCODE") AndAlso
                        MA0004row("SHARYOTYPE") = MA0004INProw("SHARYOTYPE") AndAlso
                        MA0004row("TSHABAN") = MA0004INProw("TSHABAN") AndAlso
                       (MA0004row("STYMD") = MA0004INProw("STYMD") OrElse
                        MA0004row("STYMD") = "") Then

                        MA0004INProw("OPERATION") = "Update"
                        Exit For

                    End If
                Next
            End If

            MA0004INPtbl.Rows(i).ItemArray = MA0004INProw.ItemArray

        Next

        '○変更有無判定　&　MA0004tblへ入力値反映
        MA0004INProw = MA0004INPtbl.NewRow
        For i As Integer = 0 To MA0004INPtbl.Rows.Count - 1
            MA0004INProw.ItemArray = MA0004INPtbl.Rows(i).ItemArray

            Select Case MA0004INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(I_EXCEL, MA0004INProw)
                Case "Insert"
                    TBL_INSERT_SUB(MA0004INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
            End Select

            MA0004INPtbl.Rows(i).ItemArray = MA0004INProw.ItemArray
        Next

    End Sub
    ''' <summary>
    ''' テーブル内容更新処理
    ''' </summary>
    ''' <param name="I_EXCEL"></param>
    ''' <param name="MA0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByVal I_EXCEL As String, ByRef MA0004INProw As DataRow)

        Dim WW_UPDATE As String = ""
        Dim WW_DATE As Date = Date.Now
        Dim WW_TIMSTP As Integer = 0

        Dim MA0004row As DataRow = MA0004tbl.NewRow
        For i As Integer = 0 To MA0004tbl.Rows.Count - 1
            MA0004row.ItemArray = MA0004tbl.Rows(i).ItemArray

            '不要レコード読み飛ばし
            If MA0004row("CAMPCODE") = MA0004INProw("CAMPCODE") AndAlso
               MA0004row("SHARYOTYPE") = MA0004INProw("SHARYOTYPE") AndAlso
               MA0004row("TSHABAN") = MA0004INProw("TSHABAN") AndAlso
              (MA0004row("STYMD") = MA0004INProw("STYMD") OrElse
               MA0004row("STYMD") = "") Then
                '処理対象
            Else
                Continue For
            End If

            WW_TIMSTP = MA0004row("TIMSTP")

            '■変更有無判定　…　変更結果をMA0004INProw("OPERATION")へ反映

            '○明細変更判定
            If MA0004row("ENDYMD") = MA0004INProw("ENDYMD") AndAlso MA0004row("LICNPLTNO1") = MA0004INProw("LICNPLTNO1") AndAlso
               MA0004row("LICNPLTNO2") = MA0004INProw("LICNPLTNO2") AndAlso MA0004row("LICNMNFACT") = MA0004INProw("LICNMNFACT") AndAlso
               MA0004row("LICNFRAMENO") = MA0004INProw("LICNFRAMENO") AndAlso MA0004row("LICNMODEL") = MA0004INProw("LICNMODEL") AndAlso
               MA0004row("LICNMOTOR") = MA0004INProw("LICNMOTOR") AndAlso
               CInt(MA0004row("LICNLDCAPA")) = CInt(MA0004INProw("LICNLDCAPA")) AndAlso
               CInt(MA0004row("LICN5LDCAPA")) = CInt(MA0004INProw("LICN5LDCAPA")) AndAlso
               CInt(MA0004row("LICNWEIGHT")) = CInt(MA0004INProw("LICNWEIGHT")) AndAlso
               CInt(MA0004row("LICNTWEIGHT")) = CInt(MA0004INProw("LICNTWEIGHT")) AndAlso
               CInt(MA0004row("LICNCWEIGHT")) = CInt(MA0004INProw("LICNCWEIGHT")) AndAlso
               MA0004row("LICNYMD") = MA0004INProw("LICNYMD") AndAlso MA0004row("TAXLINSYMD") = MA0004INProw("TAXLINSYMD") AndAlso
               CInt(MA0004row("TAXVTAX").ToString.Replace(",", "")) = CInt(MA0004INProw("TAXVTAX").ToString.Replace(",", "")) AndAlso
               CInt(MA0004row("TAXATAX").ToString.Replace(",", "")) = CInt(MA0004INProw("TAXATAX").ToString.Replace(",", "")) AndAlso
               MA0004row("OTNKTINSYMD") = MA0004INProw("OTNKTINSYMD") AndAlso MA0004row("OTNKTINSNYMD") = MA0004INProw("OTNKTINSNYMD") AndAlso
               MA0004row("HPRSJINSYMD") = MA0004INProw("HPRSJINSYMD") AndAlso MA0004row("HPRSINSYMD") = MA0004INProw("HPRSINSYMD") AndAlso
               MA0004row("HPRSINSNYMD") = MA0004INProw("HPRSINSNYMD") AndAlso MA0004row("CHEMTINSYMD") = MA0004INProw("CHEMTINSYMD") AndAlso
               MA0004row("CHEMTINSNYMD") = MA0004INProw("CHEMTINSNYMD") AndAlso MA0004row("INSKBN") = MA0004INProw("INSKBN") AndAlso
               MA0004row("DELFLG") = MA0004INProw("DELFLG") Then

                WW_UPDATE = "ON"

            End If

            '■変更有無判定

            '○明細変更無＆PDF変更無（Excelの為、PDF入力無）
            If WW_UPDATE = "ON" AndAlso I_EXCEL = "EXCEL" Then
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If

            '○明細変更無＆PDF変更無（PDF入力無）
            If WW_UPDATE = "ON" AndAlso I_EXCEL <> "EXCEL" AndAlso WF_DViewRepPDF.Items.Count = 0 Then
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If

            '○明細変更無＆PDF変更無（PDF入力有）
            If WW_UPDATE = "ON" AndAlso I_EXCEL <> "EXCEL" AndAlso WF_DViewRepPDF.Items.Count <> 0 Then
                'PDF変更判定(ファイル名変更があれば変更)

                Dim WW_FIND_SW1 As String = "OFF"
                For Each reitem As RepeaterItem In WF_DViewRepPDF.Items

                    Dim WW_FIND_SW2 As String = "OFF"
                    For Each liitem As ListItem In WF_ListBoxPDF.Items
                        If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = liitem.Text AndAlso
                           CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = liitem.Value Then
                            WW_FIND_SW2 = "ON"
                            Exit For
                        End If
                    Next

                    If WW_FIND_SW2 = "OFF" Then
                        '変更有（明細一致しない）
                        WW_FIND_SW1 = "ON"
                        Exit For
                    End If

                Next

                If WW_FIND_SW1 = "OFF" Then
                    '変更無（全明細一致）
                    MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '変更有（全明細一致しない）
                    MA0004INProw("OPERATION") = "Update"
                End If

            End If

            '○明細変更有
            If WW_UPDATE = "" Then
                MA0004INProw("OPERATION") = "Update"
            End If
            '■テーブル更新
            If MA0004INProw("OPERATION") = "Update" Then

                '固定項目
                MA0004INProw("LINECNT") = MA0004row("LINECNT")
                MA0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MA0004INProw("TIMSTP") = MA0004row("TIMSTP")
                MA0004INProw("SELECT") = 1
                MA0004INProw("HIDDEN") = 0
                MA0004INProw("WORK_NO") = 0

                MA0004row = MA0004tbl.NewRow
                MA0004row.ItemArray = MA0004INProw.ItemArray

                If MA0004row("CHEMTINSYMD") = "" Then
                    MA0004row("CHEMTINSNYMD") = C_DEFAULT_YMD
                End If
                If MA0004row("CHEMTINSNYMD") = "" Then
                    MA0004row("CHEMTINSNYMD") = C_DEFAULT_YMD
                End If

                If MA0004row("OTNKTINSYMD") = "" Then
                    MA0004row("OTNKTINSNYMD") = C_DEFAULT_YMD
                End If
                If MA0004row("OTNKTINSNYMD") = "" Then
                    MA0004row("OTNKTINSNYMD") = C_DEFAULT_YMD
                End If

                If MA0004row("HPRSINSYMD") = "" Then
                    MA0004row("HPRSINSNYMD") = C_DEFAULT_YMD
                End If
                If MA0004row("HPRSINSNYMD") = "" Then
                    MA0004row("HPRSINSNYMD") = C_DEFAULT_YMD
                End If

                MA0004row("NOTES") = ""
                MA0004row("WORK_NO") = 0
                MA0004row("STYMD_S") = ""
                MA0004row("ENDYMD_S") = ""
                MA0004row("STYMD_A") = ""
                MA0004row("ENDYMD_A") = ""
                MA0004row("STYMD_B") = ""
                MA0004row("ENDYMD_B") = ""
                MA0004row("STYMD_C") = ""
                MA0004row("ENDYMD_C") = ""

                MA0004tbl.Rows(i).ItemArray = MA0004row.ItemArray
            End If

            Exit For

        Next

    End Sub
    ''' <summary>
    ''' テーブル登録処理
    ''' </summary>
    ''' <param name="MA0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef MA0004INProw As DataRow)

        Dim MA0004row As DataRow = MA0004tbl.NewRow

        '項目設定
        MA0004row.ItemArray = MA0004INProw.ItemArray
        MA0004row("LINECNT") = MA0004tbl.Rows.Count + 1
        MA0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MA0004row("NOTES") = ""
        MA0004row("TIMSTP") = "0"
        MA0004row("SELECT") = 1
        MA0004row("HIDDEN") = 0
        MA0004row("WORK_NO") = 0
        MA0004row("STYMD_S") = ""
        MA0004row("ENDYMD_S") = ""
        MA0004row("STYMD_A") = ""
        MA0004row("ENDYMD_A") = ""
        MA0004row("STYMD_B") = ""
        MA0004row("ENDYMD_B") = ""
        MA0004row("STYMD_C") = ""
        MA0004row("ENDYMD_C") = ""

        MA0004tbl.Rows.Add(MA0004row)

    End Sub

    ''' <summary>
    ''' TAB11番目の表示項目
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MA0004PDFtbl_ColumnsAdd()

        If IsNothing(MA0004PDFtbl) Then
            MA0004PDFtbl = New DataTable()
        End If
        If MA0004PDFtbl.Columns.Count <> 0 Then
            MA0004PDFtbl.Columns.Clear()
        End If

        'MA0004PDFtblテンポラリDB項目作成
        MA0004PDFtbl.Clear()

        MA0004PDFtbl.Columns.Add("FILENAME", GetType(String))
        MA0004PDFtbl.Columns.Add("DELFLG", GetType(String))
        MA0004PDFtbl.Columns.Add("FILEPATH", GetType(String))

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

        If I_VALUE <> "" Then
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
    ''' エラーチェックのログ出力
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <param name="MA0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MA0004INProw As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = ""

        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & MA0004INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 車両タイプ　　=" & MA0004INProw("SHARYOTYPE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 統一車番　　　=" & MA0004INProw("TSHABAN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効開始年月日=" & MA0004INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 有効終了年月日=" & MA0004INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　　　　=" & MA0004INProw("DELFLG") & " "
        rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

    End Sub

End Class

Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 届先マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0006TODOKESAKI
    Inherits Page

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

    Private BASEtbl As DataTable                                'Grid格納用テーブル
    Private INPtbl As DataTable                                 'Detail入力用テーブル
    Private UPDtbl As DataTable                                 '更新用テーブル
    Private PDFtbl As DataTable                                 'PDF Repeater格納用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'テーブルオブジェクト作成
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSTBL As New CS0023XLSUPLOAD                 'UPLOAD_XLSデータ取得
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0033AUTONUM As New CS0033AutoNumber               '自動採番
    Private CS0050Session As New CS0050SESSION                  'セッション管理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

    Private GS0007FIXVALUElst As New GS0007FIXVALUElst          'Leftボックス用固定値リスト取得

    '共通処理結果
    Private WW_ERRCODE As String                                'サブ用リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバ処理の遷移先
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
                    If Not Master.RecoverTable(BASEtbl) Then
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
                        Case "WF_ListboxDBclick"
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick"
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"
                            WF_MEMO_Change()
                        Case "WF_GridDBclick"
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp"
                            WF_GRID_ScroleUp()
                        Case "WF_EXCEL_UPLOAD"
                            If WF_EXCEL_UPLOAD.Value = "XLS_LOADED" Then
                                UPLOAD_EXCEL()
                            ElseIf WF_EXCEL_UPLOAD.Value = "XLS_SAVE" Then
                                UPLOAD_PDF_EXCEL()
                            ElseIf WF_EXCEL_UPLOAD.Value = "PDF_LOADED" Then
                                UPLOAD_PDF_EXCEL()
                            End If
                            WF_EXCEL_UPLOAD.Value = ""
                        Case "WF_MAP"
                            WF_MAP_Click()
                        Case "WF_COORDINATE"
                            WF_COORDINATE_Click()
                        Case "WF_DTAB_Click"
                            WF_Detail_TABChange()
                        Case "WF_DTAB_PDF_Click"
                            DTAB_PDFEXCELdisplay()
                        Case "WF_DTAB_PDF_Change"
                            PDF_EXCEL_SELECTchange()
                        Case Else
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
            If Not IsNothing(BASEtbl) Then
                BASEtbl.Clear()
                BASEtbl.Dispose()
                BASEtbl = Nothing
            End If

            If Not IsNothing(INPtbl) Then
                INPtbl.Clear()
                INPtbl.Dispose()
                INPtbl = Nothing
            End If

            If Not IsNothing(UPDtbl) Then
                UPDtbl.Clear()
                UPDtbl.Dispose()
                UPDtbl = Nothing
            End If

            If Not IsNothing(PDFtbl) Then
                PDFtbl.Clear()
                PDFtbl.Dispose()
                PDFtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = GRMC0006WRKINC.MAPID
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_TORINAME.Focus()
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(BASEtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = Master.MAPID
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
            CS0013PROFview.LEVENT = "ondblclick"
            CS0013PROFview.LFUNC = "ListDbClick"
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○名称付与
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WW_FIXVALUE("MC0006_PDF", WW_DUMMY, WF_Rep2_PDFselect)            'PDF選択ListBox設定   ★LeftBox以外

        '○DTab初期設定
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○Workディレクトリ削除
        PDF_INITdel()

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置（開始）
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For Each BASErow As DataRow In BASEtbl.Rows
            If BASErow("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                BASErow("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(BASEtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成
        CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013PROFview.MAPID = Master.MAPID
        CS0013PROFview.VARI = Master.VIEWID
        CS0013PROFview.SRCDATA = WW_TBLview.ToTable
        CS0013PROFview.TBLOBJ = pnlListArea
        CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013PROFview.LEVENT = "ondblclick"
        CS0013PROFview.LFUNC = "ListDbClick"
        CS0013PROFview.TITLEOPT = True
        CS0013PROFview.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If
        WF_TORINAME.Focus()

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each row As DataRow In BASEtbl.Rows
            '一度全部非表示化する
            row("HIDDEN") = 1

            '取引先名称　届先名称　分類　絞込判定
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text = "" Then
                row("HIDDEN") = 0
            End If

            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING As String = row("TORINAME")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_TORINAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING As String = row("NAMES")        '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_TODOKENAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text <> "" Then
                If WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING2.Contains(WF_TODOKENAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WW_STRING2.Contains(WF_TODOKENAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WW_STRING2.Contains(WF_TODOKENAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
        Next

        '画面先頭を表示
        WF_GridPosition.Text = "1"
        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_TORINAME.Focus()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        DATE_RELATION_CHK()

        Try
            'ジャーナル出力用テーブル準備
            Master.CreateEmptyTable(UPDtbl)

            If WW_ERRCODE = C_MESSAGE_NO.NORMAL Then
                'メッセージ初期化
                rightview.SetErrorReport("")
            End If

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                                        " _
                    & " set @hensuu = 0 ;                                                  " _
                    & " DECLARE hensuu CURSOR FOR                                          " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                    & "     FROM MC006_TODOKESAKI                                          " _
                    & "     WHERE    CAMPCODE      = @P01                                  " _
                    & "       and    TORICODE      = @P02                                  " _
                    & "       and    TODOKECODE    = @P03                                  " _
                    & "       and    STYMD         = @P27 ;                                " _
                    & "                                                                    " _
                    & " OPEN hensuu ;                                                      " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                    & " IF ( @@FETCH_STATUS = 0 )                                          " _
                    & "    UPDATE MC006_TODOKESAKI                                         " _
                    & "       SET    NAMES         = @P04 ,                                " _
                    & "              NAMEL         = @P05 ,                                " _
                    & "              NAMESK        = @P06 ,                                " _
                    & "              NAMELK        = @P07 ,                                " _
                    & "              POSTNUM1      = @P08 ,                                " _
                    & "              POSTNUM2      = @P09 ,                                " _
                    & "              ADDR1         = @P10 ,                                " _
                    & "              ADDR2         = @P11 ,                                " _
                    & "              ADDR3         = @P12 ,                                " _
                    & "              ADDR4         = @P13 ,                                " _
                    & "              TEL           = @P14 ,                                " _
                    & "              FAX           = @P15 ,                                " _
                    & "              MAIL          = @P16 ,                                " _
                    & "              LATITUDE      = @P17 ,                                " _
                    & "              LONGITUDE     = @P18 ,                                " _
                    & "              CITIES        = @P19 ,                                " _
                    & "              MORG          = @P20 ,                                " _
                    & "              NOTES1        = @P21 ,                                " _
                    & "              NOTES2        = @P22 ,                                " _
                    & "              NOTES3        = @P23 ,                                " _
                    & "              NOTES4        = @P24 ,                                " _
                    & "              NOTES5        = @P25 ,                                " _
                    & "              CLASS         = @P26 ,                                " _
                    & "              ENDYMD        = @P28 ,                                " _
                    & "              DELFLG        = @P29 ,                                " _
                    & "              UPDYMD        = @P31 ,                                " _
                    & "              UPDUSER       = @P32 ,                                " _
                    & "              UPDTERMID     = @P33 ,                                " _
                    & "              RECEIVEYMD    = @P34 ,                                " _
                    & "              NOTES6        = @P35 ,                                " _
                    & "              NOTES7        = @P36 ,                                " _
                    & "              NOTES8        = @P37 ,                                " _
                    & "              NOTES9        = @P38 ,                                " _
                    & "              NOTES10       = @P39                                  " _
                    & "     WHERE    CAMPCODE      = @P01                                  " _
                    & "       and    TORICODE      = @P02                                  " _
                    & "       and    TODOKECODE    = @P03                                  " _
                    & "       and    STYMD         = @P27 ;                                " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                    & "    INSERT INTO MC006_TODOKESAKI                                    " _
                    & "             (CAMPCODE ,                                            " _
                    & "              TORICODE ,                                            " _
                    & "              TODOKECODE ,                                          " _
                    & "              NAMES ,                                               " _
                    & "              NAMEL ,                                               " _
                    & "              NAMESK ,                                              " _
                    & "              NAMELK ,                                              " _
                    & "              POSTNUM1 ,                                            " _
                    & "              POSTNUM2 ,                                            " _
                    & "              ADDR1 ,                                               " _
                    & "              ADDR2 ,                                               " _
                    & "              ADDR3 ,                                               " _
                    & "              ADDR4 ,                                               " _
                    & "              TEL ,                                                 " _
                    & "              FAX ,                                                 " _
                    & "              MAIL ,                                                " _
                    & "              LATITUDE ,                                            " _
                    & "              LONGITUDE ,                                           " _
                    & "              CITIES ,                                              " _
                    & "              MORG ,                                                " _
                    & "              NOTES1 ,                                              " _
                    & "              NOTES2 ,                                              " _
                    & "              NOTES3 ,                                              " _
                    & "              NOTES4 ,                                              " _
                    & "              NOTES5 ,                                              " _
                    & "              CLASS ,                                               " _
                    & "              STYMD ,                                               " _
                    & "              ENDYMD ,                                              " _
                    & "              DELFLG ,                                              " _
                    & "              INITYMD ,                                             " _
                    & "              UPDYMD ,                                              " _
                    & "              UPDUSER ,                                             " _
                    & "              UPDTERMID ,                                           " _
                    & "              RECEIVEYMD ,                                          " _
                    & "              NOTES6 ,                                              " _
                    & "              NOTES7 ,                                              " _
                    & "              NOTES8 ,                                              " _
                    & "              NOTES9 ,                                              " _
                    & "              NOTES10 )                                             " _
                    & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,    " _
                    & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,    " _
                    & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,    " _
                    & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39);        " _
                    & " CLOSE hensuu ;                                                     " _
                    & " DEALLOCATE hensuu ;                                                "

                Dim SQLStr2 As String =
                      " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP " _
                    & "     FROM MC006_TODOKESAKI                  " _
                    & "     WHERE    CAMPCODE     = @P01           " _
                    & "       and    TORICODE     = @P02           " _
                    & "       and    TODOKECODE   = @P03           " _
                    & "       and    STYMD        = @P04 ;         "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Char, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Char, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Char, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Char, 20)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Char, 50)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Char, 20)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Char, 50)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Char, 3)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Char, 4)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Char, 20)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Char, 20)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Char, 20)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Char, 30)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Char, 13)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.Char, 13)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Char, 50)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Char, 20)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Char, 20)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Char, 20)
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Char, 20)
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Char, 70)
                    Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Char, 70)
                    Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Char, 70)
                    Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Char, 70)
                    Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Char, 70)
                    Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Char, 1)
                    Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.DateTime)
                    Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)
                    Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Char, 1)
                    Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.DateTime)
                    Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)
                    Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.Char, 20)
                    Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.Char, 30)
                    Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.DateTime)
                    Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.Char, 70)
                    Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Char, 70)
                    Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Char, 70)
                    Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.Char, 70)
                    Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.Char, 70)

                    Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", SqlDbType.Char, 20)
                    Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", SqlDbType.Char, 20)
                    Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", SqlDbType.Char, 20)
                    Dim PARA4 As SqlParameter = SQLcmd2.Parameters.Add("@P04", SqlDbType.Date)

                    'ＤＢ更新
                    '　※エラーは処理されない
                    For Each BASErow As DataRow In BASEtbl.Rows
                        '削除は更新しない（操作欄のみクリア）
                        If BASErow("DELFLG") = C_DELETE_FLG.DELETE AndAlso BASErow("TIMSTP") = "0" Then
                            BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            For Each BASEdel As DataRow In BASEtbl.Rows
                                If BASEdel("CAMPCODE") = BASErow("CAMPCODE") AndAlso
                                    BASEdel("TORICODE") = BASErow("TORICODE") AndAlso
                                    BASEdel("STYMD") = BASErow("STYMD") Then

                                    BASEdel("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                End If
                            Next

                            Continue For
                        End If

                        If BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                            BASErow("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            Dim WW_DATENOW As DateTime = Date.Now

                            '○自動採番     
                            If RTrim(BASErow("TODOKECODE")).StartsWith("新") Then
                                CS0033AUTONUM.CAMPCODE = WF_CAMPCODE.Text
                                CS0033AUTONUM.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.TODOKESAKI
                                CS0033AUTONUM.USERID = Master.USERID
                                CS0033AUTONUM.getAutoNumber()
                                If isNormal(CS0033AUTONUM.ERR) Then
                                    BASErow("TODOKECODE") = CS0033AUTONUM.SEQ
                                Else
                                    Master.Output(CS0033AUTONUM.ERR, C_MESSAGE_TYPE.ABORT, "CS0033AutoNumber")
                                    Exit Sub
                                End If
                            End If

                            '○ＤＢ更新
                            PARA01.Value = BASErow("CAMPCODE")
                            PARA02.Value = BASErow("TORICODE")
                            PARA03.Value = BASErow("TODOKECODE")
                            PARA04.Value = BASErow("NAMES")
                            PARA05.Value = BASErow("NAMEL")
                            PARA06.Value = BASErow("NAMESK")
                            PARA07.Value = BASErow("NAMELK")
                            PARA08.Value = BASErow("POSTNUM1")
                            PARA09.Value = BASErow("POSTNUM2")
                            PARA10.Value = BASErow("ADDR1")
                            PARA11.Value = BASErow("ADDR2")
                            PARA12.Value = BASErow("ADDR3")
                            PARA13.Value = BASErow("ADDR4")
                            PARA14.Value = BASErow("TEL")
                            PARA15.Value = BASErow("FAX")
                            PARA16.Value = BASErow("MAIL")
                            PARA17.Value = BASErow("LATITUDE")
                            PARA18.Value = BASErow("LONGITUDE")
                            PARA19.Value = BASErow("CITIES")
                            PARA20.Value = BASErow("MORG")
                            PARA21.Value = BASErow("NOTES1")
                            PARA22.Value = BASErow("NOTES2")
                            PARA23.Value = BASErow("NOTES3")
                            PARA24.Value = BASErow("NOTES4")
                            PARA25.Value = BASErow("NOTES5")
                            PARA26.Value = BASErow("CLASS")
                            PARA27.Value = RTrim(BASErow("STYMD"))
                            PARA28.Value = RTrim(BASErow("ENDYMD"))
                            PARA29.Value = BASErow("DELFLG")
                            PARA30.Value = WW_DATENOW
                            PARA31.Value = WW_DATENOW
                            PARA32.Value = Master.USERID
                            PARA33.Value = Master.USERTERMID
                            PARA34.Value = C_DEFAULT_YMD
                            PARA35.Value = BASErow("NOTES6")
                            PARA36.Value = BASErow("NOTES7")
                            PARA37.Value = BASErow("NOTES8")
                            PARA38.Value = BASErow("NOTES9")
                            PARA39.Value = BASErow("NOTES10")
                            SQLcmd.CommandTimeout = 300
                            SQLcmd.ExecuteNonQuery()

                            '結果 --> テーブル(BASEtbl)反映
                            BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Dim UPDrow As DataRow = UPDtbl.NewRow

                            UPDrow("CAMPCODE") = BASErow("CAMPCODE")
                            UPDrow("TORICODE") = BASErow("TORICODE")
                            UPDrow("TODOKECODE") = BASErow("TODOKECODE")
                            UPDrow("NAMES") = BASErow("NAMES")
                            UPDrow("NAMEL") = BASErow("NAMEL")
                            UPDrow("NAMESK") = BASErow("NAMESK")
                            UPDrow("NAMELK") = BASErow("NAMELK")
                            UPDrow("POSTNUM1") = BASErow("POSTNUM1")
                            UPDrow("POSTNUM2") = BASErow("POSTNUM2")
                            UPDrow("ADDR1") = BASErow("ADDR1")
                            UPDrow("ADDR2") = BASErow("ADDR2")
                            UPDrow("ADDR3") = BASErow("ADDR3")
                            UPDrow("ADDR4") = BASErow("ADDR4")
                            UPDrow("TEL") = BASErow("TEL")
                            UPDrow("FAX") = BASErow("FAX")
                            UPDrow("MAIL") = BASErow("MAIL")
                            UPDrow("LATITUDE") = BASErow("LATITUDE")
                            UPDrow("LONGITUDE") = BASErow("LONGITUDE")
                            UPDrow("CITIES") = BASErow("CITIES")
                            UPDrow("MORG") = BASErow("MORG")
                            UPDrow("NOTES1") = BASErow("NOTES1")
                            UPDrow("NOTES2") = BASErow("NOTES2")
                            UPDrow("NOTES3") = BASErow("NOTES3")
                            UPDrow("NOTES4") = BASErow("NOTES4")
                            UPDrow("NOTES5") = BASErow("NOTES5")
                            UPDrow("NOTES6") = BASErow("NOTES6")
                            UPDrow("NOTES7") = BASErow("NOTES7")
                            UPDrow("NOTES8") = BASErow("NOTES8")
                            UPDrow("NOTES9") = BASErow("NOTES9")
                            UPDrow("NOTES10") = BASErow("NOTES10")
                            UPDrow("CLASS") = BASErow("CLASS")
                            UPDrow("STYMD") = RTrim(BASErow("STYMD"))
                            UPDrow("ENDYMD") = RTrim(BASErow("ENDYMD"))
                            UPDrow("DELFLG") = BASErow("DELFLG")
                            CS0020JOURNAL.TABLENM = "MC006_TODOKESAKI"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力
                                Exit Sub
                            End If

                            '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                            PARA1.Value = BASErow("CAMPCODE")
                            PARA2.Value = BASErow("TORICODE")
                            PARA3.Value = BASErow("TODOKECODE")
                            PARA4.Value = RTrim(BASErow("STYMD"))
                            SQLcmd2.CommandTimeout = 300

                            Using SQLdr2 As SqlDataReader = SQLcmd2.ExecuteReader()
                                While SQLdr2.Read
                                    BASErow("TIMSTP") = SQLdr2("TIMSTP")

                                    '相手方のタイムスタンプ、操作も更新する
                                    For Each BASEtim As DataRow In BASEtbl.Rows
                                        If BASEtim("CAMPCODE") = BASErow("CAMPCODE") AndAlso
                                        BASEtim("TORICODE") = BASErow("TORICODE") AndAlso
                                        BASEtim("TODOKECODE") = BASErow("TODOKECODE") AndAlso
                                        BASEtim("STYMD") = BASErow("STYMD") Then
                                            BASEtim("TIMSTP") = SQLdr2("TIMSTP")
                                            BASEtim("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                        End If
                                    Next
                                End While
                            End Using

                            'PDF更新処理
                            PDF_DBupdate(BASErow("CAMPCODE"), BASErow("TODOKECODE"))
                        End If
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC006_TODOKESAKI UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            '○メッセージ表示
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)
        Else
            '○メッセージ表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

        'カーソル設定
        WF_TORINAME.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************
    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.CS0030REPORT()

        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)


    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理                                         ***
    ' ******************************************************************************
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
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

        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁遷移ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(BASEtbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○最終頁に移動
        If WW_TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10) + 1
        End If

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧の明細行ダブルクリック時処理(GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_FILED_OBJ As Object

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT = WW_LINECNT - 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○Grid内容(BASEtbl)よりDetail編集

        WF_Sel_LINECNT.Text = BASEtbl.Rows(WW_LINECNT)("LINECNT")

        '有効年月日
        WF_STYMD.Text = BASEtbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = BASEtbl.Rows(WW_LINECNT)("ENDYMD")

        '会社
        WF_CAMPCODE.Text = BASEtbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WW_TEXT, WW_DUMMY)
        WF_CAMPCODE_TEXT.Text = WW_TEXT

        '取引先コード
        WF_TORICODE.Text = BASEtbl.Rows(WW_LINECNT)("TORICODE")
        CODENAME_get("TORICODE", WF_TORICODE.Text, WW_TEXT, WW_DUMMY, work.CreateTORIParam(WF_CAMPCODE.Text))
        WF_TORICODE_TEXT.Text = WW_TEXT

        '届先コード
        WF_TODOKECODE.Text = BASEtbl.Rows(WW_LINECNT)("TODOKECODE")
        CODENAME_get("TODOKECODE", WF_TODOKECODE.Text, WW_TEXT, WW_DUMMY)
        WF_TODOKECODE_TEXT.Text = WW_TEXT

        '削除フラグ
        WF_DELFLG.Text = BASEtbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○タブ別処理(2 書類（PDF）)
        PDF_EXCEL_INITread(WF_CAMPCODE.Text, WF_TORICODE.Text, WF_TODOKECODE.Text)

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each BASErow As DataRow In BASEtbl.Rows
            Select Case BASErow("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case BASEtbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        WF_GridDBclick.Text = ""

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As New Hashtable
                    'フィールドによってパラメーターを変える
                    Select Case WW_FIELD
                        Case "WF_TORICODE"                      '取引先
                            prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)
                        Case "WF_TODOKECODE"                    '届先
                            prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODE.Text)
                        Case "CITIES"                           '市町村コード
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CITIES")
                        Case "WF_CLASS", "CLASS"                '分類
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CLASS")
                        Case "MORG"                             '管理部署
                            prmData = work.CreateMORGParam(work.WF_SEL_CAMPCODE.Text)
                        Case "WF_DELFLG", "WF_Rep_DELFLG"       '削除フラグ
                            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text
                    End Select
                    .ActiveCalendar()
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
    End Sub
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
            rightview.SelectIndex(WF_RightViewChange.Value)
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
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
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
        rightview.SetErrorReport("")

        '○DetailBoxをINPtblへ退避
        DetailBoxToINPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPtbl_CHEK(WW_ERRCODE)

        '○INProwを一覧(BASEtbl)へ反映
        If isNormal(WW_ERRCODE) Then
            BASEtbl_UPD("DISPLAY")
        End If

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○PDF更新 
        If isNormal(WW_ERRCODE) Then
            PDF_SAVE_H()
        End If

        'Detailクリア
        If isNormal(WW_ERRCODE) Then
            WF_CLEAR_Click()
        End If

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_TORINAME.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToINPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Master.CreateEmptyTable(INPtbl)
        Dim INProw As DataRow = INPtbl.NewRow

        For Each INPcol As DataColumn In INPtbl.Columns
            If IsDBNull(INProw.Item(INPcol)) OrElse IsNothing(INProw.Item(INPcol)) Then
                Select Case INPcol.ColumnName
                    Case "LINECNT"
                        INProw.Item(INPcol) = 0
                    Case "TIMSTP"
                        INProw.Item(INPcol) = 0
                    Case "SELECT"
                        INProw.Item(INPcol) = 1
                    Case "HIDDEN"
                        INProw.Item(INPcol) = 0
                    Case Else
                        INProw.Item(INPcol) = ""
                End Select
            End If
        Next

        If WF_Sel_LINECNT.Text = "" Then
            INProw("LINECNT") = 0
        Else
            INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_TORICODE.Text)          '取引先コード
        Master.EraseCharToIgnore(WF_TODOKECODE.Text)        '届先コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        INProw("CAMPCODE") = WF_CAMPCODE.Text
        INProw("TORICODE") = WF_TORICODE.Text
        INProw("TODOKECODE") = WF_TODOKECODE.Text
        INProw("STYMD") = WF_STYMD.Text
        INProw("ENDYMD") = WF_ENDYMD.Text
        INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_TORICODE.Text) AndAlso
            String.IsNullOrEmpty(WF_TODOKECODE.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○名称付与
        '会社名称
        INProw("CAMPNAME") = ""
        CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAME"), WW_DUMMY)
        '取引先名称
        INProw("TORINAME") = ""
        CODENAME_get("TORICODE", INProw("TORICODE"), INProw("TORINAME"), WW_DUMMY)
        '市町村名称
        INProw("CITIESNAME") = ""
        CODENAME_get("CITIES", INProw("CITIES"), INProw("CITIESNAME"), WW_DUMMY)
        '管理部署名称
        INProw("MORGNAME") = ""
        CODENAME_get("MORG", INProw("MORG"), INProw("MORGNAME"), WW_DUMMY)
        '分類名称
        INProw("CLASSNAME") = ""
        CODENAME_get("CLASS", INProw("CLASS"), INProw("CLASSNAME"), WW_DUMMY)

        INPtbl.Rows.Add(INProw)

    End Sub

    ' *** 詳細画面-クリアボタン処理
    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        For Each BASErow As DataRow In BASEtbl.Rows
            Select Case BASErow("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○detailboxヘッダークリア
        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_TODOKECODE.Text = ""
        WF_TODOKECODE_TEXT.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        '○名称付与
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '○Detail初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○PDF初期画面編集
        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_STYMD.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()

        Dim dataTable As DataTable = New DataTable
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        Try
            'カラム情報をリピーター作成用に取得
            Master.CreateEmptyTable(dataTable)
            dataTable.Rows.Add(dataTable.NewRow())

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.TABID = CONST_DETAIL_TABID
            CS0052DetailView.SRCDATA = dataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Exit Sub
            End If

            WF_DetailMView.ActiveViewIndex = 0

            For row As Integer = 0 To CS0052DetailView.ROWMAX - 1
                For col As Integer = 1 To CS0052DetailView.COLMAX
                    'ダブルクリック時コード検索イベント追加
                    If DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), Label).Text <> "" Then
                        repField = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELD_" & col), Label)
                        repValue = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_VALUE_" & col), TextBox)
                        REP_ATTR_get(repField.Text, repAttr)
                        If repAttr <> "" AndAlso Not repValue.ReadOnly Then
                            repValue.Attributes.Remove("ondblclick")
                            repValue.Attributes.Add("ondblclick", repAttr)
                            repName = DirectCast(WF_DViewRep1.Items(row).FindControl("WF_Rep1_FIELDNM_" & col), Label)
                            repName.Attributes.Remove("style")
                            repName.Attributes.Add("style", "text-decoration: underline;")
                        End If
                    End If
                Next col
            Next row

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            dataTable.Dispose()
            dataTable = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-イベント文字取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_ATTR">イベント内容</param>
    ''' <remarks></remarks>
    Protected Sub REP_ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        Select Case I_FIELD
            Case "CITIES"
                '市町村コード
                O_ATTR = "REF_Field_DBclick('CITIES', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"
            Case "MORG"
                '管理部署名
                O_ATTR = "REF_Field_DBclick('MORG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');"
            Case "CLASS"
                '分類名
                O_ATTR = "REF_Field_DBclick('CLASS', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"
            Case Else
                O_ATTR = String.Empty
        End Select

    End Sub

    ''' <summary>
    ''' 詳細画面-タブ切替処理
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
        WF_Dtab01.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab01.Style.Remove("border")
        WF_Dtab01.Style.Add("border", "1px solid black")
        WF_Dtab01.Style.Remove("font-weight")
        WF_Dtab01.Style.Add("font-weight", "normal")

        '申請書類（PDF） 
        WF_Dtab02.Style.Remove("color")
        WF_Dtab02.Style.Add("color", "black")
        WF_Dtab02.Style.Remove("background-color")
        WF_Dtab02.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab02.Style.Remove("border")
        WF_Dtab02.Style.Add("border", "1px solid black")
        WF_Dtab02.Style.Remove("font-weight")
        WF_Dtab02.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '管理
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            Case 1
                '申請書類（PDF） 
                WF_Dtab02.Style.Remove("color")
                WF_Dtab02.Style.Add("color", "blue")
                WF_Dtab02.Style.Remove("background-color")
                WF_Dtab02.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab02.Style.Remove("border")
                WF_Dtab02.Style.Add("border", "1px solid blue")
                WF_Dtab02.Style.Remove("font-weight")
                WF_Dtab02.Style.Add("font-weight", "bold")
        End Select

    End Sub

    ''' <summary>
    ''' 詳細画面-地図表示ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MAP_Click()

        '○エラーレポート準備
        rightview.SetErrorReport("")

        '○DetailBoxをINPtblへ退避
        DetailBoxToINPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        Dim WW_URL As String
        Dim WW_LATITUDEL As String
        Dim WW_LONGITUDE As String

        Dim INProw As DataRow = INPtbl(0)

        If WF_Sel_LINECNT.Text = "" Then
            INProw("LINECNT") = 0
        Else
            INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If

        '○項目チェック
        If String.IsNullOrEmpty(INProw("LATITUDE")) OrElse
           String.IsNullOrEmpty(INProw("LONGITUDE")) Then

            '緯度、経度のどちらかが未入力の場合、「ゲートシティ大崎」を表示
            WW_LATITUDEL = "35.619397"
            WW_LONGITUDE = "139.730808"
        Else
            '入力値を設定
            WW_LATITUDEL = INProw("LATITUDE")
            WW_LONGITUDE = INProw("LONGITUDE")

        End If

        '地図表示
        WW_URL = "http:" & "//maps.google.co.jp/maps?ll=" & WW_LATITUDEL & "," & WW_LONGITUDE & "&spn=0.002,0.002&t=m&q=" & WW_LATITUDEL & "," & WW_LONGITUDE
        ClientScript.RegisterStartupScript(Me.GetType, "OpenNewWindow", "<script language=""javascript"">window.open(' " & WW_URL & "', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>")

    End Sub

    ''' <summary>
    ''' 詳細画面-緯度経度ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_COORDINATE_Click()

        '緯度経度表示
        Dim WW_URL As String
        WW_URL = "http:" & "//user.numazu-ct.ac.jp/~tsato/webmap/sphere/coordinates/advanced.html"
        ClientScript.RegisterStartupScript(Me.GetType, "OpenNewWindow", "<script language=""javascript"">window.open(' " & WW_URL & "', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>")

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
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

                Case "WF_DELFLG"
                    '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectTEXT
                    WF_DELFLG.Focus()

                Case "WF_TORICODE"
                    '取引先
                    WF_TORICODE.Text = WW_SelectValue
                    WF_TORICODE_TEXT.Text = WW_SelectTEXT
                    WF_TORICODE.Focus()

                Case "WF_TODOKECODE"
                    '届先
                    WF_TODOKECODE.Text = WW_SelectValue
                    WF_TODOKECODE_TEXT.Text = WW_SelectTEXT
                    WF_TODOKECODE.Focus()

                Case "WF_CLASS"
                    '分類
                    WF_CLASS.Text = WW_SelectValue
                    WF_CLASS_TEXT.Text = WW_SelectTEXT
                    WF_CLASS.Focus()

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
            '○ディテール01（管理）変数設定
            For Each reitem As RepeaterItem In WF_DViewRep1.Items
                '***********  左サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Focus()
                    Exit For
                End If

                '***********  中央　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Focus()
                    Exit For
                End If

                '***********  右サイド　***********
                If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text = WF_FIELD_REP.Value Then
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_SelectValue
                    CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_SelectTEXT
                    CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Focus()
                    Exit For
                End If
            Next

            '○ディテール02（PDF）変数設定 
            If WF_FIELD_REP.Value = "WF_Rep_DELFLG" Then
                For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
                    If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_FIELD.Value Then
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = WW_SelectValue
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Focus()
                        Exit For
                    End If
                Next
            End If
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

    ' ******************************************************************************
    ' ***  ファイルアップロード入力処理                                          *** 
    ' ******************************************************************************
    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○初期処理
        Dim WW_DATE As Date
        rightview.SetErrorReport("")

        '○UPLOAD_XLSデータ取得
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSTBL.MAPID = Master.MAPID
        CS0023XLSTBL.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSTBL.ERR) Then
            If CS0023XLSTBL.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSTBL.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
            Exit Sub
        End If

        '○CS0023XLSTBL.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSTBL.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSTBL.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSTBL.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSTBL.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○入力テーブル作成
        Master.CreateEmptyTable(INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSTBL.TBLDATA.Rows
            Dim INProw As DataRow = INPtbl.NewRow

            '○初期クリア
            For Each INPcol As DataColumn In INPtbl.Columns
                If IsDBNull(INProw.Item(INPcol)) OrElse IsNothing(INProw.Item(INPcol)) Then
                    Select Case INPcol.ColumnName
                        Case "LINECNT"
                            INProw.Item(INPcol) = 0
                        Case "TIMSTP"
                            INProw.Item(INPcol) = 0
                        Case "SELECT"
                            INProw.Item(INPcol) = 1
                        Case "HIDDEN"
                            INProw.Item(INPcol) = 0
                        Case Else
                            INProw.Item(INPcol) = ""
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TODOKECODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each BASErow As DataRow In BASEtbl.Rows
                    If XLSTBLrow("CAMPCODE") = BASErow("CAMPCODE") AndAlso
                       XLSTBLrow("TORICODE") = BASErow("TORICODE") AndAlso
                       XLSTBLrow("TODOKECODE") = BASErow("TODOKECODE") AndAlso
                       XLSTBLrow("STYMD") = BASErow("STYMD") Then
                        INProw.ItemArray = BASErow.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○項目セット
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                INProw("TODOKECODE") = XLSTBLrow("TODOKECODE")
            End If

            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    WW_DATE = XLSTBLrow("STYMD")
                    INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    WW_DATE = XLSTBLrow("ENDYMD")
                    INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("NAMES") >= 0 Then
                INProw("NAMES") = XLSTBLrow("NAMES")
            End If

            If WW_COLUMNS.IndexOf("NAMEL") >= 0 Then
                INProw("NAMEL") = XLSTBLrow("NAMEL")
            End If

            If WW_COLUMNS.IndexOf("NAMESK") >= 0 Then
                INProw("NAMESK") = XLSTBLrow("NAMESK")
            End If

            If WW_COLUMNS.IndexOf("NAMELK") >= 0 Then
                INProw("NAMELK") = XLSTBLrow("NAMELK")
            End If

            If WW_COLUMNS.IndexOf("POSTNUM1") >= 0 Then
                INProw("POSTNUM1") = XLSTBLrow("POSTNUM1")
            End If

            If WW_COLUMNS.IndexOf("POSTNUM2") >= 0 Then
                INProw("POSTNUM2") = XLSTBLrow("POSTNUM2")
            End If

            If WW_COLUMNS.IndexOf("ADDR1") >= 0 Then
                INProw("ADDR1") = XLSTBLrow("ADDR1")
            End If

            If WW_COLUMNS.IndexOf("ADDR2") >= 0 Then
                INProw("ADDR2") = XLSTBLrow("ADDR2")
            End If

            If WW_COLUMNS.IndexOf("ADDR3") >= 0 Then
                INProw("ADDR3") = XLSTBLrow("ADDR3")
            End If

            If WW_COLUMNS.IndexOf("ADDR4") >= 0 Then
                INProw("ADDR4") = XLSTBLrow("ADDR4")
            End If

            If WW_COLUMNS.IndexOf("TEL") >= 0 Then
                INProw("TEL") = XLSTBLrow("TEL")
            End If

            If WW_COLUMNS.IndexOf("FAX") >= 0 Then
                INProw("FAX") = XLSTBLrow("FAX")
            End If

            If WW_COLUMNS.IndexOf("MAIL") >= 0 Then
                INProw("MAIL") = XLSTBLrow("MAIL")
            End If

            If WW_COLUMNS.IndexOf("LATITUDE") >= 0 Then
                INProw("LATITUDE") = XLSTBLrow("LATITUDE")
            End If

            If WW_COLUMNS.IndexOf("LONGITUDE") >= 0 Then
                INProw("LONGITUDE") = XLSTBLrow("LONGITUDE")
            End If

            If WW_COLUMNS.IndexOf("CITIES") >= 0 Then
                INProw("CITIES") = XLSTBLrow("CITIES")
            End If

            If WW_COLUMNS.IndexOf("MORG") >= 0 Then
                INProw("MORG") = XLSTBLrow("MORG")
            End If

            If WW_COLUMNS.IndexOf("NOTES1") >= 0 Then
                INProw("NOTES1") = XLSTBLrow("NOTES1")
            End If

            If WW_COLUMNS.IndexOf("NOTES2") >= 0 Then
                INProw("NOTES2") = XLSTBLrow("NOTES2")
            End If

            If WW_COLUMNS.IndexOf("NOTES3") >= 0 Then
                INProw("NOTES3") = XLSTBLrow("NOTES3")
            End If

            If WW_COLUMNS.IndexOf("NOTES4") >= 0 Then
                INProw("NOTES4") = XLSTBLrow("NOTES4")
            End If

            If WW_COLUMNS.IndexOf("NOTES5") >= 0 Then
                INProw("NOTES5") = XLSTBLrow("NOTES5")
            End If

            If WW_COLUMNS.IndexOf("NOTES6") >= 0 Then
                INProw("NOTES6") = XLSTBLrow("NOTES6")
            End If

            If WW_COLUMNS.IndexOf("NOTES7") >= 0 Then
                INProw("NOTES7") = XLSTBLrow("NOTES7")
            End If

            If WW_COLUMNS.IndexOf("NOTES8") >= 0 Then
                INProw("NOTES8") = XLSTBLrow("NOTES8")
            End If

            If WW_COLUMNS.IndexOf("NOTES9") >= 0 Then
                INProw("NOTES9") = XLSTBLrow("NOTES9")
            End If

            If WW_COLUMNS.IndexOf("NOTES10") >= 0 Then
                INProw("NOTES10") = XLSTBLrow("NOTES10")
            End If

            If WW_COLUMNS.IndexOf("CLASS") >= 0 Then
                INProw("CLASS") = XLSTBLrow("CLASS")
            End If

            '名称付与
            CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAME"), WW_RTN_SW)
            CODENAME_get("TORICODE", INProw("TORICODE"), INProw("TORINAME"), WW_RTN_SW)
            CODENAME_get("CITIES", INProw("CITIES"), INProw("CITIESNAME"), WW_RTN_SW)
            CODENAME_get("MORG", INProw("MORG"), INProw("MORGNAME"), WW_RTN_SW)
            CODENAME_get("CLASS", INProw("CLASS"), INProw("CLASSNAME"), WW_RTN_SW)

            INPtbl.Rows.Add(INProw)
        Next

        '○項目チェック
        INPtbl_CHEK(WW_ERRCODE)

        '○BASEtbl更新(エラーでも処理する)
        BASEtbl_UPD("EXCEL")

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_TORINAME.Focus()

        '○Close
        CS0023XLSTBL.TBLDATA.Dispose()
        CS0023XLSTBL.TBLDATA.Clear()

    End Sub


    ' ******************************************************************************
    ' ***  PDF関連処理                                                           *** 
    ' ******************************************************************************

    ''' <summary>
    ''' PDF Tempディレクトリ削除(PAGE_load時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_INITdel()

        Dim WW_UPdirs As String()
        Dim WW_UPfiles As String()

        'Temp納ディレクトリ編集
        '○PDF格納Dir作成
        '   一時保存のPDFフォルダ
        '       c:\paal\applpdf\MC0006_TODOKESAKI\Temp\ユーザID\Update_H 
        '       c:\paal\applpdf\MC0006_TODOKESAKI\Temp\ユーザID\Delete_D 

        Dim WW_Dir As String = ""
        WW_Dir = WW_Dir & CS0050Session.PDF_PATH
        WW_Dir = WW_Dir & "\MC0006_TODOKESAKI\Temp\" & CS0050Session.USERID

        Dim WW_Dir_del As New List(Of String)

        'ディレクトリが存在しない場合、作成する
        If Not Directory.Exists(WW_Dir) Then
            Directory.CreateDirectory(WW_Dir)
        End If

        '○PDF格納ディレクトリ＞MC0006_TODOKESAKI\Temp\ユーザIDフォルダ内のファイル取得
        WW_UPdirs = Directory.GetDirectories(WW_Dir, "*", SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPdirs
            'Tempの自ユーザ内フォルダを取得
            WW_Dir_del.Add(tempFile)
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
    ''' PDF初期
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_Repeater_INIT()
        '○初期設定
        Dim WW_Dir As String

        '○画面編集
        '○PDF格納ディレクトリ編集   
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
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
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
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
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub
    ''' <summary>
    ''' PDF読み込み・ディレクトリ作成(Header・一覧ダブルクリック時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_EXCEL_INITread(ByVal I_CAMPCODE As String, ByVal I_TORICODE As String, ByVal I_TODOKECODE As String)

        Dim WW_UPfiles As String()

        '(説明1) フォルダ説明
        '　①一覧明細選択～表追加直前のPDF操作内容：Temp\会社コード_届先コード_nn\Update_Dフォルダに格納
        '　②表追加によるPDF一時保存内容　　　　　：Temp\会社コード_届先コード_nn\Update_Hフォルダに格納
        '　③正式PDF登録内容　　　　　　　　　　　：正式PDFフォルダ

        '(説明2) イベント別処理内容　　…　処理効率は悪いが、操作がシンプルとなる為、下記処理とした。
        '　①Page_Load時：PDF_INITdel
        '　　　　・Tempフォルダ(Update_D・Update_H)をお掃除
        '　②一覧ダブルクリック時：PDF_EXCEL_INITread
        '　　　　・Update_Hが存在しない場合、Update_Hフォルダ作成＆正式フォルダ内全PDF→Update_Hフォルダへコピー
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・Update_Dが存在する場合、Update_Dフォルダ内PDFを全て削除　＆　Update_Dフォルダ削除
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・Update_Dフォルダ作成 ＆ Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　③Detail操作（PDF表示選択切替）時：PDF_EXCEL_SELECTchange
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　④Detail操作（クリアボタン押下）時：WF_CLEAR_Click
        '　　　　・クリア処理（Update_Dクリア）＆明細クリア表示
        '　⑤Detail操作（表追加ボタン押下）時：PDF_SAVE_H
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・Update_Hフォルダ内容をクリア（PDF明細選択01～15全てを対象)
        '　　　　・Update_Dフォルダ内PDFをUpdate_Hフォルダに全てコピー（PDF明細選択01～15全てを対象)
        '　　　　・Update_Dフォルダ内PDFを全て削除（PDF明細選択01～15全てを対象)
        '　⑥PDFアップロード時：UPLOAD_PDF_EXCEL
        '　　　　・Update_Dフォルダに該当PDFを格納
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　⑦DB更新ボタン押下時：★★★
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_Dをお掃除(Update_Hフォルダは連続入力に備えクリアしない)
        '　⑧Detail操作（有効開始変更)時：PDF_EXCEL_INITread

        '○初期設定
        Dim WW_Dir As String

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If I_CAMPCODE = "" OrElse I_TORICODE = "" OrElse I_TODOKECODE = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If I_TORICODE = BASEtbl.Rows(i)("TORICODE") OrElse I_TODOKECODE = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○フォルダ作成　＆　ファイルコピー
        '○PDF格納Dir作成
        '   正式登録のPDFフォルダ
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn   　　　       　　　　 (nn:PDF書類種類)
        '   一時保存のPDFフォルダ
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn\Temp\ユーザID\Update_H  (nn:PDF書類種類)
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn\Temp\ユーザID\Delete_D  (nn:PDF書類種類)

        'c:\appl\applpdf\MC0006_TODOKESAKIフォルダは必ず存在...下位フォルダ処理を行う

        For i As Integer = 1 To 3
            '○PDF格納ディレクトリ編集    c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn
            WW_Dir = ""
            WW_Dir = WW_Dir & CS0050Session.PDF_PATH
            WW_Dir = WW_Dir & "\MC0006_TODOKESAKI"

            '○正式ディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp") Then
                Directory.CreateDirectory(WW_Dir & "\Temp")
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID)
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ＞届先コードディレクトリ作成＞Update_H の処理
            If Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H") Then
                '連続処理の場合、前回処理を残す
            Else
                'ユーザIDディレクトリ＞届先コードディレクトリ作成＞Update_H 作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H")

                '正式フォルダ内ファイル→一時保存ディレクトリ＞届先コードディレクトリ作成＞Update_H へコピー
                WW_UPfiles = Directory.GetFiles(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"), "*", SearchOption.AllDirectories)

                For Each tempFile As String In WW_UPfiles

                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim WW_File As String = tempFile
                    Do
                        If InStr(WW_File, "\") > 0 Then
                            WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                        End If
                    Loop Until InStr(WW_File, "\") <= 0

                    '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
                    File.Copy(tempFile, WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H\" & WW_File, True)
                Next
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成＞届先コードディレクトリ作成＞Update_D 処理
            If Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D") Then
                'Update_Dフォルダ内ファイル削除
                WW_UPfiles = Directory.GetFiles(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D", "*", SearchOption.AllDirectories)
                For Each tempFile As String In WW_UPfiles
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            Else
                'Update_Dが存在しない場合、Update_Dフォルダ作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D")
            End If

            'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
            WW_UPfiles = Directory.GetFiles(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H", "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles

                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If
                Loop Until InStr(WW_File, "\") <= 0

                'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
                File.Copy(tempFile, WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D\" & WW_File, True)
            Next
        Next

        '○画面編集
        '○PDF格納ディレクトリ編集
        If WF_Rep2_PDFselect.SelectedValue.ToString() = "" Then
            WF_Rep2_PDFselect.SelectedIndex = 0
        End If

        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & I_CAMPCODE & "_" & I_TODOKECODE & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル一覧
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
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
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
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
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF表示内容変更時処理（Detail・PDFタブ内のListBox切替時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_EXCEL_SELECTchange()

        '○初期設定
        Dim WW_Dir As String

        rightview.SetErrorReport("")

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_TORICODE.Text = "" OrElse WF_TODOKECODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If WF_TORICODE.Text = BASEtbl.Rows(i)("TORICODE") OrElse WF_TODOKECODE.Text = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
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
        '○PDF格納ディレクトリ編集   
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
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
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
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
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF表追加時処理（Detail・表追加ボタン押下時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_SAVE_H()

        '○初期設定
        Dim WW_Dir As String

        rightview.SetErrorReport("")

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_TORICODE.Text = "" OrElse WF_TODOKECODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If WF_TORICODE.Text = BASEtbl.Rows(i)("TORICODE") OrElse WF_TODOKECODE.Text = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
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
        For i As Integer = 1 To 3

            '○Update_Hフォルダクリア処理
            WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
            WW_Dir = WW_Dir & CS0050Session.USERID & "\"
            WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & i.ToString("00") & "\Update_H"
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            '○Update_Dフォルダ内容をUpdate_Hフォルダへコピー
            WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
            WW_Dir = WW_Dir & CS0050Session.USERID & "\"
            WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & i.ToString("00")
            For Each tempFile As String In Directory.GetFiles(WW_Dir & "\Update_D", "*", SearchOption.AllDirectories)
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If
                Loop Until InStr(WW_File, "\") <= 0

                'Update_Dフォルダ内PDF→Update_Hフォルダへ上書コピー
                File.Copy(tempFile, WW_Dir & "\Update_H\" & WW_File, True)
            Next

            '○Update_Dフォルダクリア
            WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
            WW_Dir = WW_Dir & CS0050Session.USERID & "\"
            WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & i.ToString("00") & "\Update_D"
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next
        Next

        '○PDF初期画面編集

        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

    End Sub

    ''' <summary>
    ''' PDFファイルアップロード入力処理(PDFドロップ時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_PDF_EXCEL()

        '○初期設定
        Dim WW_Dir As String

        rightview.SetErrorReport("")

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_TORICODE.Text = "" OrElse WF_TODOKECODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If WF_TORICODE.Text = BASEtbl.Rows(i)("TORICODE") OrElse WF_TODOKECODE.Text = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○アップロードフォルダからUpdate_Dフォルダーへファイル移動
        '○アップロードファイル名を取得　＆　移動
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

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
        '○PDF格納ディレクトリ編集    c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
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
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
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
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

        '○メッセージ編集
        Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' PDF DB更新処理（DB更新ボタン押下時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_DBupdate(ByRef In_CAMPCODE As String, ByRef In_TODOKECODE As String)

        '○初期設定
        Dim WW_DirSend As String = ""
        Dim WW_DirH As String = ""
        Dim WW_DirD As String = ""
        Dim WW_DirHON As String = ""

        '　⑦DB更新ボタン押下時：★★★
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_D・Update_Hをお掃除
        '○DB反映処理

        For i As Integer = 1 To 3
            '○FTP格納ディレクトリ編集
            WW_DirSend = CS0050Session.UPLOAD_PATH
            WW_DirSend = WW_DirSend & "\SEND\SENDSTOR\"
            WW_DirSend = WW_DirSend & Master.USERTERMID
            WW_DirSend = WW_DirSend & "\PDF\MC0006_TODOKESAKI"
            WW_DirSend = WW_DirSend & "\" & In_CAMPCODE & "_" & In_TODOKECODE & "_" & i.ToString("00")

            WW_DirHON = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\" & In_CAMPCODE & "_" & In_TODOKECODE & "_" & i.ToString("00")

            'Tempフォルダーが存在したら処理する（EXCEL入力の場合、Tempができないため）
            WW_DirH = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
            WW_DirH = WW_DirH & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE & "_" & i.ToString("00") & "\Update_H"
            If Directory.Exists(WW_DirH) Then
                '○PDF正式格納フォルダクリア処理
                For Each tempFile As String In Directory.GetFiles(WW_DirHON, "*", SearchOption.AllDirectories)
                    'サブフォルダは対象外
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next

                '○Update_Hフォルダ内容をPDF正式格納フォルダへコピー
                For Each tempFile As String In Directory.GetFiles(WW_DirH, "*", SearchOption.AllDirectories)
                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim WW_File As String = tempFile
                    Do
                        If InStr(WW_File, "\") > 0 Then
                            WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                        End If
                    Loop Until InStr(WW_File, "\") <= 0

                    'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                    File.Copy(tempFile, WW_DirHON & "\" & WW_File, True)
                Next

                '○Update_Dフォルダクリア　※Update_Hフォルダは、連続処理に備えてクリアーしない
                WW_DirD = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
                WW_DirD = WW_DirD & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE & "_" & i.ToString("00") & "\Update_D"

                For Each tempFile As String In Directory.GetFiles(WW_DirD, "*", SearchOption.AllDirectories)
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next

                'PDF正式格納フォルダ→配信用PDF格納フォルダへ上書コピー（削除してコピー）
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

                Dim WW_tempDirs As String() = Directory.GetFiles(WW_DirHON, "*")
                For Each tempFile As String In WW_tempDirs
                    Dim WW_File As String = Path.GetFileName(tempFile)
                    File.Copy(tempFile, WW_DirSend & "\" & WW_File, True)
                Next
            End If
        Next

    End Sub

    ''' <summary>
    ''' PDF 内容表示（Detail・PDFダブルクリック時（内容照会））
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DTAB_PDFEXCELdisplay()

        Dim WW_Dir As String = CS0050Session.UPLOAD_PATH & "\PRINTWORK\" & CS0050Session.TERMID

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_DTAB_PDF_DISP_FILE.Value Then
                'ディレクトリが存在しない場合、作成する
                If Not Directory.Exists(WW_Dir) Then
                    Directory.CreateDirectory(WW_Dir)
                End If

                'ダウンロードファイル送信準備
                File.Copy(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text,
                            WW_Dir & "\" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text,
                            True)

                'ダウンロード処理へ遷移
                WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/print/" & CS0050Session.TERMID & "/" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' PDF リネーム処理（新規登録のみ）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_RENAME(ByRef In_CAMPCODE As String, ByRef In_TODOKECODE_OLD As String, ByRef In_TODOKECODE_NEW As String)

        '○初期設定
        Dim WW_Dir As String

        '○リネーム処理
        For i As Integer = 1 To 3
            '○新PDF格納ディレクトリ編集（届先コードに採番値が設定されているディレクトリ）
            WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI"

            '○正式ディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp") Then
                Directory.CreateDirectory(WW_Dir & "\Temp")
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID)
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ＞届先コードディレクトリ作成＞Update_H の処理
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00") & "\Update_H") Then
                'ユーザIDディレクトリ＞届先コードディレクトリ作成＞Update_H 作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00") & "\Update_H")
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成＞届先コードディレクトリ作成＞Update_D 処理
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00") & "\Update_D") Then
                'Update_Dが存在しない場合、Update_Dフォルダ作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & In_CAMPCODE & "_" & In_TODOKECODE_NEW & "_" & i.ToString("00") & "\Update_D")
            End If
        Next

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベースを検索し画面表示する一覧を作成する</remarks>
    Protected Sub MAPDATAget()

        '○画面表示用データ取得
        '取引先内容検索
        Try
            '○GridView内容をテーブル退避
            'テンポラリDB項目作成
            If IsNothing(BASEtbl) Then
                BASEtbl = New DataTable
            End If

            If BASEtbl.Columns.Count <> 0 Then
                BASEtbl.Columns.Clear()
            End If

            '○DB項目クリア
            BASEtbl.Clear()

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      " SELECT                                                            " _
                    & "       0                                       as LINECNT ,        " _
                    & "       ''                                      as OPERATION ,      " _
                    & "       TIMSTP = cast(isnull(UPDTIMSTP,0)       as bigint) ,        " _
                    & "       1                                       as 'SELECT' ,       " _
                    & "       0                                       as HIDDEN ,         " _
                    & "       isnull(rtrim(CAMPCODE),'')              as CAMPCODE ,       " _
                    & "       isnull(rtrim(TORICODE),'')              as TORICODE ,       " _
                    & "       isnull(rtrim(TODOKECODE),'')            as TODOKECODE ,     " _
                    & "       isnull(format(STYMD, 'yyyy/MM/dd'),'')  as STYMD ,         " _
                    & "       isnull(format(ENDYMD, 'yyyy/MM/dd'),'') as ENDYMD ,        " _
                    & "       isnull(rtrim(NAMES),'')                 as NAMES ,          " _
                    & "       isnull(rtrim(NAMEL),'')                 as NAMEL ,          " _
                    & "       isnull(rtrim(NAMESK),'')                as NAMESK ,         " _
                    & "       isnull(rtrim(NAMELK),'')                as NAMELK ,         " _
                    & "       isnull(rtrim(POSTNUM1),'')              as POSTNUM1 ,       " _
                    & "       isnull(rtrim(POSTNUM2),'')              as POSTNUM2 ,       " _
                    & "       isnull(rtrim(ADDR1),'')                 as ADDR1 ,          " _
                    & "       isnull(rtrim(ADDR2),'')                 as ADDR2 ,          " _
                    & "       isnull(rtrim(ADDR3),'')                 as ADDR3 ,          " _
                    & "       isnull(rtrim(ADDR4),'')                 as ADDR4 ,          " _
                    & "       isnull(rtrim(TEL),'')                   as TEL ,            " _
                    & "       isnull(rtrim(FAX),'')                   as FAX ,            " _
                    & "       isnull(rtrim(MAIL),'')                  as MAIL ,           " _
                    & "       isnull(rtrim(LATITUDE),'')              as LATITUDE ,       " _
                    & "       isnull(rtrim(LONGITUDE),'')             as LONGITUDE ,      " _
                    & "       isnull(rtrim(CITIES),'')                as CITIES ,         " _
                    & "       isnull(rtrim(MORG),'')                  as MORG ,           " _
                    & "       isnull(rtrim(NOTES1),'')                as NOTES1 ,         " _
                    & "       isnull(rtrim(NOTES2),'')                as NOTES2 ,         " _
                    & "       isnull(rtrim(NOTES3),'')                as NOTES3 ,         " _
                    & "       isnull(rtrim(NOTES4),'')                as NOTES4 ,         " _
                    & "       isnull(rtrim(NOTES5),'')                as NOTES5 ,         " _
                    & "       isnull(rtrim(NOTES6),'')                as NOTES6 ,         " _
                    & "       isnull(rtrim(NOTES7),'')                as NOTES7 ,         " _
                    & "       isnull(rtrim(NOTES8),'')                as NOTES8 ,         " _
                    & "       isnull(rtrim(NOTES9),'')                as NOTES9 ,         " _
                    & "       isnull(rtrim(NOTES10),'')               as NOTES10 ,        " _
                    & "       isnull(rtrim(CLASS),'')                 as CLASS ,          " _
                    & "       rtrim(DELFLG)                           as DELFLG ,        " _
                    & "       ''                                      as INITYMD     ,   " _
                    & "       ''                                      as UPDYMD      ,   " _
                    & "       ''                                      as UPDUSER     ,   " _
                    & "       ''                                      as UPDTERMID   ,   " _
                    & "       ''                                      as RECEIVEYMD  ,   " _
                    & "       ''                                      as UPDTIMSTP ,     " _
                    & "       ''                                      as CAMPNAME ,      " _
                    & "       ''                                      as TORINAME ,      " _
                    & "       ''                                      as CITIESNAME  ,   " _
                    & "       ''                                      as MORGNAME ,      " _
                    & "       ''                                      as CLASSNAME      " _
                    & "  FROM MC006_TODOKESAKI                               " _
                    & " WHERE CAMPCODE   = @P1                               " _
                    & "   and STYMD     <= @P2                               " _
                    & "   and ENDYMD    >= @P3                               " _
                    & "   and DELFLG    <> '1'                               "

                ' 届先コード先頭JXとCOSMOは除外
                SQLStr &= String.Format(" and TODOKECODE NOT LIKE '{0}'", C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & "%")
                SQLStr &= String.Format(" and TODOKECODE NOT LIKE '{0}'", C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & "%")

                ' 条件指定で指定されたものでＳＱＬで可能なものを追加する
                '取引先コード
                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) OrElse Not String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                    If Not String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) AndAlso String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                        SQLStr &= String.Format(" and TORICODE = '{0}' ", work.WF_SEL_TORICODEF.Text)
                    ElseIf String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) AndAlso Not String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                        SQLStr &= String.Format(" and TORICODE = '{0}' ", work.WF_SEL_TORICODET.Text)
                    Else
                        SQLStr &= String.Format(" and TORICODE >= '{0}' ", work.WF_SEL_TORICODEF.Text)
                        SQLStr &= String.Format(" and TORICODE <= '{0}' ", work.WF_SEL_TORICODET.Text)
                    End If
                End If
                '届先コード
                If Not String.IsNullOrEmpty(work.WF_SEL_TODOKECODE.Text) Then
                    SQLStr &= String.Format(" and TODOKECODE = '{0}' ", work.WF_SEL_TODOKECODE.Text)
                End If
                '届先名称（部分一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_TODOKENAME.Text) Then
                    SQLStr &= String.Format(" and NAMES LIKE '%{0}%' ", work.WF_SEL_TODOKENAME.Text)
                End If
                '郵便番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_POSTNUM.Text) Then
                    SQLStr &= String.Format(" and (POSTNUM1 + POSTNUM2) LIKE '{0}%' ", work.WF_SEL_POSTNUM.Text)
                End If
                '住所（部分一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_ADDR.Text) Then
                    SQLStr &= String.Format(" and (ADDR1 + ADDR2 + ADDR3 + ADDR4) LIKE '%{0}%' ", work.WF_SEL_ADDR.Text)
                End If
                '電話番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_TEL.Text) Then
                    SQLStr &= String.Format(" and TEL LIKE '{0}%' ", work.WF_SEL_TEL.Text)
                End If
                'FAX番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_FAX.Text) Then
                    SQLStr &= String.Format(" and FAX LIKE '{0}%' ", work.WF_SEL_FAX.Text)
                End If
                '市町村コード
                If Not String.IsNullOrEmpty(work.WF_SEL_CITIES.Text) Then
                    SQLStr &= String.Format(" and CITIES = '{0}' ", work.WF_SEL_CITIES.Text)
                End If
                '分類
                If Not String.IsNullOrEmpty(work.WF_SEL_CLASS.Text) Then
                    SQLStr &= String.Format(" and CLASS = '{0}' ", work.WF_SEL_CLASS.Text)
                End If

                SQLStr &= " ORDER BY TORICODE, TODOKECODE, STYMD "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.Char, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text

                    SQLcmd.CommandTimeout = 300

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            BASEtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○テーブル検索結果をテーブル格納
                        BASEtbl.Load(SQLdr)
                    End Using

                    For Each BASErow As DataRow In BASEtbl.Rows
                        '○項目名称セット
                        CODENAME_get("CAMPCODE", BASErow("CAMPCODE"), BASErow("CAMPNAME"), WW_DUMMY)        '会社名称
                        CODENAME_get("TORICODE", BASErow("TORICODE"), BASErow("TORINAME"), WW_DUMMY)        '取引先名称
                        CODENAME_get("CITIES", BASErow("CITIES"), BASErow("CITIESNAME"), WW_DUMMY)          '市町村名称
                        CODENAME_get("MORG", BASErow("MORG"), BASErow("MORGNAME"), WW_DUMMY)                '管理部署名称
                        CODENAME_get("CLASS", BASErow("CLASS"), BASErow("CLASSNAME"), WW_DUMMY)             '分類名称
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC006_TODOKESAKI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"
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
        CS0026TBLSORT.TABLE = BASEtbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            BASEtbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' 登録データ入力チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_CHEK(ByRef O_RTNCODE As String)

        '○インターフェイス初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_TEXT As String = ""

        '○単項目チェック(ヘッダー情報)
        Dim dicKeyCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"CAMPCODE", "会社"} _
                                                            , {"TORICODE", "取引先"} _
                                                            , {"TODOKECODE", "届先"} _
                                                            , {"STYMD", "有効年月日"} _
                                                            , {"ENDYMD", "有効年月日"} _
                                                            , {"DELFLG", "削除"}
                                                            }
        '○単項目チェック(明細情報)
        Dim dicCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"MORG", "管理部署"} _
                                                            , {"NAMES", "届先名称（短）"} _
                                                            , {"NAMEL", "届先名称（長）"} _
                                                            , {"NAMESK", "届先カナ名称（短）"} _
                                                            , {"NAMELK", "届先カナ名称（長）"} _
                                                            , {"POSTNUM1", "郵便番号（上）"} _
                                                            , {"POSTNUM2", "郵便番号（下）"} _
                                                            , {"ADDR1", "住所１"} _
                                                            , {"ADDR2", "住所２"} _
                                                            , {"ADDR3", "住所３"} _
                                                            , {"ADDR4", "住所４"} _
                                                            , {"TEL", "電話番号"} _
                                                            , {"FAX", "ＦＡＸ番号"} _
                                                            , {"MAIL", "届先担当メールアドレス"} _
                                                            , {"LATITUDE", "緯度（10進）"} _
                                                            , {"LONGITUDE", "経度（10進）"} _
                                                            , {"CITIES", "市町村コード（JIS)"} _
                                                            , {"NOTES1", "特定要件１"} _
                                                            , {"NOTES2", "特定要件２"} _
                                                            , {"NOTES3", "特定要件３"} _
                                                            , {"NOTES4", "特定要件４"} _
                                                            , {"NOTES5", "特定要件５"} _
                                                            , {"NOTES6", "特定要件６"} _
                                                            , {"NOTES7", "特定要件７"} _
                                                            , {"NOTES8", "特定要件８"} _
                                                            , {"NOTES9", "特定要件９"} _
                                                            , {"NOTES10", "特定要件１０"} _
                                                            , {"CLASS", "分類"}
                                                            }
        '○単項目チェック(マスタ存在)
        Dim dicMasterCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"CAMPCODE", "会社"} _
                                                            , {"TORICODE", "取引先"} _
                                                            , {"MORG", "管理部署"} _
                                                            , {"CITIES", "市町村コード（JIS)"} _
                                                            , {"CLASS", "分類"}
                                                            }

        For Each INProw As DataRow In INPtbl.Rows

            WW_LINEERR_SW = ""

            '○単項目チェック(ヘッダー情報)
            For Each item In dicKeyCheck

                WW_TEXT = INProw(item.Key)
                '届先コード新規時は対象外
                If item.Key = "TODOKECODE" AndAlso WW_TEXT = "" Then Continue For
                Master.CheckField(WF_CAMPCODE.Text, item.Key, INProw(item.Key), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'LeftBox存在チェック
                    If String.IsNullOrEmpty(WW_TEXT) Then
                        INProw(item.Key) = String.Empty
                    Else
                        If dicMasterCheck.ContainsKey(item.Key) Then
                            CODENAME_get(item.Key, INProw(item.Key), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・更新できないレコード(" & item.Value & "エラー)です。"
                                WW_CheckMES2 = " マスタに存在しません。"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                                WW_LINEERR_SW = "ERR"
                                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(" & item.Value & "エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Next

            '関連チェック(キー情報)

            '届先コード存在チェック（届先コードが空白（新規）の場合、存在チェックしない）
            If INProw("TODOKECODE") <> "" AndAlso WW_LINEERR_SW <> "ERR" Then
                If BASEtbl.Rows.Count = 0 Then
                    WW_CheckMES1 = "・更新できないレコード(該当レコード無)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                End If

                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    If BASEtbl.Rows(j)("CAMPCODE") = INProw("CAMPCODE") AndAlso
                       BASEtbl.Rows(j)("TORICODE") = INProw("TORICODE") AndAlso
                       BASEtbl.Rows(j)("TODOKECODE") = INProw("TODOKECODE") Then
                        Exit For
                    Else
                        If j >= (BASEtbl.Rows.Count - 1) Then
                            WW_CheckMES1 = "・更新できないレコード(該当レコード無)です。"
                            WW_CheckMES2 = ""
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                            O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            WW_LINEERR_SW = "ERR"
                            Exit For
                        End If
                    End If
                Next
            End If

            '大小比較チェック
            If INProw("STYMD") > INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始日付 ＞ 終了日付)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If
            '範囲チェック
            If work.WF_SEL_STYMD.Text > INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If
            If work.WF_SEL_ENDYMD.Text < INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(明細情報)
            For Each item In dicCheck

                WW_TEXT = INProw(item.Key)
                Master.CheckField(WF_CAMPCODE.Text, item.Key, INProw(item.Key), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'LeftBox存在チェック
                    If String.IsNullOrEmpty(WW_TEXT) Then
                        INProw(item.Key) = String.Empty
                    Else
                        If dicMasterCheck.ContainsKey(item.Key) Then

                            CODENAME_get(item.Key, INProw(item.Key), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・エラーが存在します。(" & item.Value & ")"
                                WW_CheckMES2 = " マスタに存在しません。"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                                WW_LINEERR_SW = "ERR"
                                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・エラーが存在します。(" & item.Value & ")"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                    WW_LINEERR_SW = "ERR"
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Next

            If WW_LINEERR_SW = "" Then
                If INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        For Each BASErow As DataRow In BASEtbl.Rows

            '読み飛ばし
            If (BASErow("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                BASErow("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                BASErow("DELFLG") = C_DELETE_FLG.DELETE OrElse
                BASErow("STYMD") = "" Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each checkRow As DataRow In BASEtbl.Rows

                '同一KEY以外は読み飛ばし
                If BASErow("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                   BASErow("TORICODE") = checkRow("TORICODE") AndAlso
                   BASErow("TODOKECODE") = checkRow("TODOKECODE") AndAlso
                   checkRow("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If BASErow("STYMD") = checkRow("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(BASErow("STYMD"), WW_DATE_ST)
                    Date.TryParse(BASErow("ENDYMD"), WW_DATE_END)
                    Date.TryParse(checkRow("STYMD"), WW_DATE_ST2)
                    Date.TryParse(checkRow("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, BASErow)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, BASErow)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If
            Next

            If WW_LINEERR_SW = "" Then
                BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

        Next

    End Sub

    ' ***  日付連続性チェック（歯抜けチェック）
    Protected Sub DATE_RELATION_CHK()

        Dim WW_DATEF As Date
        Dim WW_DATET As Date
        Dim WW_DATE As Date

        '対象キーの抽出（操作が"更新"のもの）
        Dim WW_WorkView As New DataView(BASEtbl)
        WW_WorkView.Sort = "CAMPCODE, TORICODE, TODOKECODE"
        WW_WorkView.RowFilter = "OPERATION in ('" & C_LIST_OPERATION_CODE.UPDATING & "', '" & C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING & "')" _
            & " and DELFLG <> '" & C_DELETE_FLG.DELETE & "'"
        '同一キーを集約し、ワークテーブルを作成
        Dim WW_TBL As DataTable = WW_WorkView.ToTable("WW_TBL", True, "CAMPCODE", "TORICODE", "TODOKECODE")

        'チェック対象データのソート
        Dim WW_TBLView As New DataView(BASEtbl)
        WW_TBLView.Sort = "CAMPCODE, TORICODE, TODOKECODE"
        WW_TBLView.RowFilter = "DELFLG <> '" & C_DELETE_FLG.DELETE & "'"

        '日付連続性チェック
        For j As Integer = 0 To WW_TBLView.Count - 2
            For i As Integer = 0 To WW_TBL.Rows.Count - 1
                If WW_TBLView.Item(j).Row.Item("CAMPCODE") = WW_TBL.Rows(i)("CAMPCODE") AndAlso
                   WW_TBLView.Item(j).Row.Item("TORICODE") = WW_TBL.Rows(i)("TORICODE") AndAlso
                   WW_TBLView.Item(j).Row.Item("TODOKECODE") = WW_TBL.Rows(i)("TODOKECODE") AndAlso
                   WW_TBLView.Item(j).Row.Item("TODOKECODE") <> "" Then
                Else
                    Continue For
                End If

                If WW_TBLView.Item(j).Row.Item("CAMPCODE") = WW_TBLView.Item(j + 1).Row.Item("CAMPCODE") AndAlso
                    WW_TBLView.Item(j).Row.Item("TORICODE") = WW_TBLView.Item(j + 1).Row.Item("TORICODE") AndAlso
                    WW_TBLView.Item(j).Row.Item("TODOKECODE") = WW_TBLView.Item(j + 1).Row.Item("TODOKECODE") Then
                Else
                    Continue For
                End If

                Date.TryParse(WW_TBLView.Item(j + 1).Row.Item("STYMD"), WW_DATEF)
                Date.TryParse(WW_TBLView.Item(j).Row.Item("ENDYMD"), WW_DATE)
                WW_DATET = DateAdd("d", 1, WW_DATE)

                If WW_DATEF <> WW_DATET Then
                    'エラーレポート編集
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・更新できないレコードです。(開始、終了年月日が連続していません)"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> CAMPCODE=" & WW_TBLView.Item(j).Row.Item("CAMPCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TORICODE=" & WW_TBLView.Item(j).Row.Item("TORICODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TODOKECODE=" & WW_TBLView.Item(j).Row.Item("TODOKECODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> STYMD=" & WW_TBLView.Item(j).Row.Item("STYMD") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ENDYMD=" & WW_TBLView.Item(j).Row.Item("ENDYMD") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> CAMPCODE=" & WW_TBLView.Item(j + 1).Row.Item("CAMPCODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TORICODE=" & WW_TBLView.Item(j + 1).Row.Item("TORICODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TODOKECODE=" & WW_TBLView.Item(j + 1).Row.Item("TODOKECODE") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> STYMD=" & WW_TBLView.Item(j + 1).Row.Item("STYMD") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ENDYMD=" & WW_TBLView.Item(j + 1).Row.Item("ENDYMD") & " , "
                    rightview.AddErrorReport(WW_ERR_MES)
                    If WW_TBLView.Item(j).Row.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        WW_TBLView.Item(j).Row.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                    If WW_TBLView.Item(j + 1).Row.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                        WW_TBLView.Item(j + 1).Row.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End If
                End If
            Next
        Next

        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            For j As Integer = 0 To WW_TBLView.Count - 1
                If BASEtbl.Rows(i)("CAMPCODE") = WW_TBLView.Item(j).Row.Item("CAMPCODE") AndAlso
                    BASEtbl.Rows(i)("TORICODE") = WW_TBLView.Item(j).Row.Item("TORICODE") AndAlso
                    BASEtbl.Rows(i)("TODOKECODE") = WW_TBLView.Item(j).Row.Item("TODOKECODE") AndAlso
                    BASEtbl.Rows(i)("STYMD") = WW_TBLView.Item(j).Row.Item("STYMD") AndAlso
                    BASEtbl.Rows(i)("ENDYMD") = WW_TBLView.Item(j).Row.Item("ENDYMD") Then

                    BASEtbl.Rows(i)("OPERATION") = WW_TBLView.Item(j).Row.Item("OPERATION")
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub BASEtbl_UPD(ByVal I_EXCEL As String)

        '○画面状態設定
        For Each BASErow As DataRow In BASEtbl.Rows
            Select Case BASErow("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each INProw As DataRow In INPtbl.Rows

            'エラーレコード読み飛ばし
            If INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            INProw("OPERATION") = "Insert"

            For Each BASErow As DataRow In BASEtbl.Rows
                'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                If BASErow("CAMPCODE") = INProw("CAMPCODE") AndAlso
                   BASErow("TORICODE") = INProw("TORICODE") AndAlso
                   BASErow("TODOKECODE") = INProw("TODOKECODE") AndAlso
                  (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then

                    INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        'テーブル反映(変更)
        For Each INProw As DataRow In INPtbl.Rows
            Select Case INProw("OPERATION")
                Case "Update"       '○更新（Update）
                    TBL_Update_SUB(INProw, I_EXCEL)
                Case "Insert"       '○更新（Insert）
                    TBL_Insert_SUB(INProw, I_EXCEL)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Update_SUB(ByRef INProw As DataRow, ByRef I_EXCEL As String)

        Dim WW_UPDATE As String = ""
        Dim WW_TIMSTP As Integer = 0

        For Each BASErow As DataRow In BASEtbl.Rows

            '不要レコード読み飛ばし
            If BASErow("CAMPCODE") = INProw("CAMPCODE") AndAlso
               BASErow("TORICODE") = INProw("TORICODE") AndAlso
               BASErow("TODOKECODE") = INProw("TODOKECODE") AndAlso
              (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then
                '処理対象
            Else
                Continue For
            End If

            WW_TIMSTP = BASErow("TIMSTP")

            '■変更有無判定　…　変更結果をINProw("OPERATION")へ反映
            '変更有無
            If BASErow("ENDYMD") = INProw("ENDYMD") AndAlso
               BASErow("NAMES") = INProw("NAMES") AndAlso
               BASErow("NAMEL") = INProw("NAMEL") AndAlso
               BASErow("NAMESK") = INProw("NAMESK") AndAlso
               BASErow("NAMELK") = INProw("NAMELK") AndAlso
               BASErow("ADDR1") = INProw("ADDR1") AndAlso BASErow("ADDR2") = INProw("ADDR2") AndAlso
               BASErow("ADDR3") = INProw("ADDR3") AndAlso BASErow("ADDR4") = INProw("ADDR4") AndAlso
               BASErow("POSTNUM1") = INProw("POSTNUM1") AndAlso BASErow("POSTNUM2") = INProw("POSTNUM2") AndAlso
               BASErow("TEL") = INProw("TEL") AndAlso
               BASErow("FAX") = INProw("FAX") AndAlso
               BASErow("MAIL") = INProw("MAIL") AndAlso
               BASErow("LATITUDE") = INProw("LATITUDE") AndAlso
               BASErow("LONGITUDE") = INProw("LONGITUDE") AndAlso
               BASErow("CITIES") = INProw("CITIES") AndAlso
               BASErow("MORG") = INProw("MORG") AndAlso
               BASErow("NOTES1") = INProw("NOTES1") AndAlso BASErow("NOTES2") = INProw("NOTES2") AndAlso
               BASErow("NOTES3") = INProw("NOTES3") AndAlso BASErow("NOTES4") = INProw("NOTES4") AndAlso
               BASErow("NOTES5") = INProw("NOTES5") AndAlso BASErow("NOTES6") = INProw("NOTES6") AndAlso
               BASErow("NOTES7") = INProw("NOTES7") AndAlso BASErow("NOTES8") = INProw("NOTES8") AndAlso
               BASErow("NOTES9") = INProw("NOTES9") AndAlso BASErow("NOTES10") = INProw("NOTES10") AndAlso
               BASErow("CLASS") = INProw("CLASS") AndAlso
               BASErow("DELFLG") = INProw("DELFLG") Then

                WW_UPDATE = "ON"
            End If

            '■変更有無判定

            '○明細変更無＆PDF変更無（Excelの為、PDF入力無）
            If WW_UPDATE = "ON" AndAlso I_EXCEL = "EXCEL" Then
                INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            End If

            '○明細変更無＆PDF変更無（PDF入力無）
            If WW_UPDATE = "ON" AndAlso I_EXCEL <> "EXCEL" AndAlso WF_DViewRepPDF.Items.Count = 0 Then
                INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
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
                    INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '変更有（全明細一致しない）
                    INProw("OPERATION") = "Update"
                End If
            End If

            '○明細変更有
            If WW_UPDATE <> "ON" Then
                INProw("OPERATION") = "Update"
            End If

            '○テーブル更新
            If INProw("OPERATION") = "Update" Then
                INProw("LINECNT") = BASErow("LINECNT")
                INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                INProw("TIMSTP") = BASErow("TIMSTP")
                INProw("SELECT") = 1
                INProw("HIDDEN") = 0

                BASErow.ItemArray = INProw.ItemArray

                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Insert_SUB(ByRef INProw As DataRow, ByRef I_EXCEL As String)

        '画面入力テーブル項目設定
        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        Dim BASErow As DataRow = BASEtbl.NewRow
        BASErow.ItemArray = INProw.ItemArray

        'KEY設定
        BASErow("LINECNT") = BASEtbl.Rows.Count + 1
        BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        BASErow("TIMSTP") = "0"
        BASErow("SELECT") = 1
        BASErow("HIDDEN") = 0
        If BASErow("TODOKECODE") = "" Then
            BASErow("TODOKECODE") = "新" & (BASEtbl.Rows.Count + 1).ToString("00")
        End If

        BASEtbl.Rows.Add(BASErow)

        '○新規分のPDFディレクトリ作成
        PDF_EXCEL_INITread(INProw("CAMPCODE"), INProw("TORICODE"), INProw("TODOKECODE"))

    End Sub

    ''' <summary>
    ''' PDFtblカラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDFtbl_ColumnsAdd()

        PDFtbl = New DataTable()
        If PDFtbl.Columns.Count <> 0 Then
            PDFtbl.Columns.Clear()
        End If

        'PDFtblテンポラリDB項目作成
        PDFtbl.Clear()

        PDFtbl.Columns.Add("FILENAME", GetType(String))
        PDFtbl.Columns.Add("DELFLG", GetType(String))
        PDFtbl.Columns.Add("FILEPATH", GetType(String))

    End Sub


    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************
    ''' <summary>
    ''' 書式変更処理
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function REP_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        REP_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    REP_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
            Case Else
        End Select
    End Function

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_CLAS"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="IO_LISTBOX"></param>
    ''' <param name="IO_LISTBOX2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_FIXVALUE(ByVal I_CLAS As String, ByRef O_RTN As String, ByRef IO_LISTBOX As ListBox, Optional ByRef IO_LISTBOX2 As ListBox = Nothing)

        GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0007FIXVALUElst.CLAS = I_CLAS
        GS0007FIXVALUElst.LISTBOX1 = IO_LISTBOX
        If Not IsNothing(IO_LISTBOX2) Then
            GS0007FIXVALUElst.LISTBOX2 = IO_LISTBOX2
        End If

        GS0007FIXVALUElst.GS0007FIXVALUElst()

        If isNormal(GS0007FIXVALUElst.ERR) Then
            IO_LISTBOX = GS0007FIXVALUElst.LISTBOX1
            If Not IsNothing(IO_LISTBOX2) Then
                IO_LISTBOX2 = GS0007FIXVALUElst.LISTBOX2
            End If
            O_RTN = ""
        Else
            Master.Output(GS0007FIXVALUElst.ERR, C_MESSAGE_TYPE.ABORT)
            O_RTN = "ERR"
        End If

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
        O_RTN = ""

        If I_VALUE <> "" Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"     '会社名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                    Case "TORICODE"     '取引先名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))
                    Case "TODOKECODE"   '届先名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODE.Text))
                    Case "MORG"         '管理部署名
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateMORGParam(work.WF_SEL_CAMPCODE.Text))
                    Case "CITIES"       '市町村名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CITIES"))
                    Case "CLASS"        '分類名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CLASS"))
                End Select
            End With
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　=" & INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　　=" & INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 届先　　　=" & INProw("TODOKECODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 名称（短）=" & INProw("NAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 管理組織　=" & INProw("MORG") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日=" & INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日=" & INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　　=" & INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

End Class

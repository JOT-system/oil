Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 固定値マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0001FIXVALUE
    Inherits Page

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

    Private BASEtbl As DataTable                                'Grid格納用テーブル
    Private INPtbl As DataTable                                 'Detail入力用テーブル
    Private UPDtbl As DataTable                                 '更新用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'テーブルオブジェクト作成
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSTBL As New CS0023XLSUPLOAD                 'UPLOAD_XLSデータ取得
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0050Session As New CS0050SESSION                  'セッション管理

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
            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            ElseIf Master.MAPpermitcode > C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "SYSTEM"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Not Master.RecoverTable(BASEtbl) Then Exit Sub

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
                            UPLOAD_EXCEL()
                        Case Else
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
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
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○画面ID設定
        Master.MAPID = GRMC0001WRKINC.MAPID
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_SELBUNRUI.Focus()
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

        '○名称付与
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

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
        WF_SELBUNRUI.Focus()

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each BASErow As DataRow In BASEtbl.Rows
            '一度全部非表示化する
            BASErow("HIDDEN") = 1

            '分類名称　絞込判定
            If WF_SELBUNRUI.Text = "" Then
                BASErow("HIDDEN") = 0
            ElseIf WF_SELBUNRUI.Text <> "" Then
                Dim WW_STRING As String = BASErow("CLASS")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_SELBUNRUI.Text) Then
                    BASErow("HIDDEN") = 0
                End If
            Else
                '両方未設定の場合、押し並べて表示
                BASErow("HIDDEN") = 0
            End If
            '○システムキーフラグが１のデータは非表示にする。
            If WF_MAPpermitcode.Value <> "SYSTEM" Then
                If BASErow("SYSTEMKEYFLG") = "1" Then
                    BASErow("HIDDEN") = 1
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
        WF_SELBUNRUI.Focus()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        'メッセージ初期化
        rightview.SetErrorReport("")

        '○関連チェック
        RelatedCheck(WW_ERRCODE)

        If Not isNormal(WW_ERRCODE) AndAlso WW_ERRCODE <> C_MESSAGE_NO.WORNING_RECORD_EXIST Then
            '○メッセージ表示
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)

            '○画面表示データ保存
            Master.SaveTable(BASEtbl)
            Exit Sub
        End If

        Try
            'ジャーナル出力用テーブル準備
            Master.CreateEmptyTable(UPDtbl)

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                    " _
                    & " set @hensuu = 0 ;                              " _
                    & " DECLARE hensuu CURSOR FOR                      " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu   " _
                    & "     FROM MC001_FIXVALUE                        " _
                    & "     WHERE CAMPCODE     = @P1                   " _
                    & "     AND   CLASS        = @P2                   " _
                    & "     AND   KEYCODE      = @P3                   " _
                    & "     AND   STYMD        = @P4 ;                 " _
                    & " OPEN hensuu ;                                  " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;          " _
                    & " IF ( @@FETCH_STATUS = 0 )                      " _
                    & "    UPDATE MC001_FIXVALUE                       " _
                    & "       SET VALUE1       = @P6 , " _
                    & "           VALUE2       = @P7 , " _
                    & "           VALUE3       = @P8 , " _
                    & "           VALUE4       = @P9 , " _
                    & "           VALUE5       = @P10 , " _
                    & "           NAMES        = @P11 , " _
                    & "           NAMEL        = @P12 , " _
                    & "           SYSTEMKEYFLG = @P13 , " _
                    & "           DELFLG       = @P14 , " _
                    & "           UPDYMD       = @P16 , " _
                    & "           UPDUSER      = @P17 , " _
                    & "           UPDTERMID    = @P18 , " _
                    & "           RECEIVEYMD   = @P19" _
                    & "       WHERE CAMPCODE   = @P1 " _
                    & "       AND   CLASS      = @P2 " _
                    & "       AND   KEYCODE    = @P3 " _
                    & "       AND   STYMD      = @P4 ; " _
                    & " IF ( @@FETCH_STATUS <> 0 ) " _
                    & "    INSERT INTO MC001_FIXVALUE " _
                    & "          (CAMPCODE , " _
                    & "           CLASS , " _
                    & "           KEYCODE , " _
                    & "           STYMD , " _
                    & "           ENDYMD , " _
                    & "           VALUE1 , " _
                    & "           VALUE2 , " _
                    & "           VALUE3 , " _
                    & "           VALUE4 , " _
                    & "           VALUE5 , " _
                    & "           NAMES , " _
                    & "           NAMEL , " _
                    & "           SYSTEMKEYFLG , " _
                    & "           DELFLG , " _
                    & "           INITYMD , " _
                    & "           UPDYMD , " _
                    & "           UPDUSER , " _
                    & "           UPDTERMID , " _
                    & "           RECEIVEYMD" _
                    & "       ) " _
                    & "        VALUES (@P1,@P2,@P3,@P4,@P5,@P6,@P7,@P8,@P9,@P10,@P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19) ; " _
                    & " CLOSE hensuu ; " _
                    & " DEALLOCATE hensuu ; "

                Dim SQLStr2 As String =
                      " SELECT  CAMPCODE , CLASS , KEYCODE , STYMD , ENDYMD , VALUE1 , VALUE2 , VALUE3 , VALUE4 , VALUE5 , " _
                    & "    NAMES , NAMEL , SYSTEMKEYFLG , DELFLG , INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                    & " FROM  MC001_FIXVALUE " _
                    & " WHERE CAMPCODE = @P1 " _
                    & "    and CLASS = @P2 " _
                    & "    and KEYCODE = @P3 " _
                    & "    and STYMD = @P4"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 20)
                    Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 50)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 30)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)

                    Dim PARA2_1 As SqlParameter = SQLcmd2.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2_2 As SqlParameter = SQLcmd2.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
                    Dim PARA2_3 As SqlParameter = SQLcmd2.Parameters.Add("@P3", SqlDbType.NVarChar, 20)
                    Dim PARA2_4 As SqlParameter = SQLcmd2.Parameters.Add("@P4", SqlDbType.Date)

                    For Each BASErow As DataRow In BASEtbl.Rows
                        If BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                            BASErow("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            Dim WW_DATENOW As DateTime = Date.Now

                            'ＤＢ更新
                            PARA1.Value = BASErow("CAMPCODE")
                            PARA2.Value = BASErow("CLASS")
                            PARA3.Value = BASErow("KEYCODE")
                            PARA4.Value = BASErow("STYMD")
                            PARA5.Value = BASErow("ENDYMD")

                            PARA6.Value = BASErow("VALUE1")
                            PARA7.Value = BASErow("VALUE2")
                            PARA8.Value = BASErow("VALUE3")
                            PARA9.Value = BASErow("VALUE4")
                            PARA10.Value = BASErow("VALUE5")
                            PARA11.Value = BASErow("NAMES")
                            PARA12.Value = BASErow("NAMEL")
                            PARA13.Value = BASErow("SYSTEMKEYFLG")

                            PARA14.Value = BASErow("DELFLG")
                            PARA15.Value = WW_DATENOW
                            PARA16.Value = WW_DATENOW
                            PARA17.Value = Master.USERID
                            PARA18.Value = Master.USERTERMID
                            PARA19.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()

                            '結果 --> テーブル(BASEtbl)反映
                            BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '更新ジャーナル追加
                            Try
                                PARA2_1.Value = BASErow("CAMPCODE")
                                PARA2_2.Value = BASErow("CLASS")
                                PARA2_3.Value = BASErow("KEYCODE")
                                PARA2_4.Value = BASErow("STYMD")

                                Dim JOURds As New DataSet()

                                Using SQLadp As New SqlDataAdapter(SQLcmd2)
                                    SQLadp.Fill(JOURds, "JOURtbl")

                                    CS0020JOURNAL.TABLENM = "MC001_FIXVALUE"
                                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                    CS0020JOURNAL.ROW = JOURds.Tables("JOURtbl").Rows(0)
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

                                    BASErow("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")
                                End Using
                            Catch ex As Exception
                                If ex.Message = "Error raised in TIMSTP" Then
                                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If

                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC001_FIXVALUE JOURNAL")
                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE JOURNAL"
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWRITE.TEXT = ex.ToString()
                                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                                Exit Sub
                            End Try
                        End If
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC001_FIXVALUE UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○メッセージ表示
        If WW_ERRCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST Then
            Master.Output(C_MESSAGE_NO.WORNING_RECORD_EXIST, C_MESSAGE_TYPE.WAR)
        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

        'カーソル設定
        WF_SELBUNRUI.Focus()

    End Sub

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        Dim PDFTBL As New DataTable
        CS0026TBLSORT.TABLE = BASEtbl
        CS0026TBLSORT.FILTER = "LINECNT <> '0'"
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.sort(PDFTBL)

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = PDFTBL                           'データ参照DataTable
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

        PDFTBL.Clear()
        PDFTBL.Dispose()
        PDFTBL = Nothing

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

        Dim XLSXTBL As New DataTable
        CS0026TBLSORT.TABLE = BASEtbl
        CS0026TBLSORT.FILTER = "LINECNT <> '0'"
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.sort(XLSXTBL)

        '○帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = XLSXTBL                          'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        XLSXTBL.Clear()
        XLSXTBL.Dispose()
        XLSXTBL = Nothing

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

        Dim WW_LINECNT As Integer = 0
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try

        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            If BASEtbl.Rows(i)("LINECNT") = WW_LINECNT Then
                WW_LINECNT = i
                Exit For
            End If
        Next

        WF_LINECNT.Text = BASEtbl.Rows(WW_LINECNT)("LINECNT")

        WF_CAMPCODE.Text = BASEtbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WF_BUNRUI.Text = BASEtbl.Rows(WW_LINECNT)("CLASS")
        CODENAME_get("CLASS", WF_BUNRUI.Text, WF_BUNRUI_TEXT.Text, WW_DUMMY)

        WF_KEYCODE.Text = BASEtbl.Rows(WW_LINECNT)("KEYCODE")

        WF_STYMD.Text = BASEtbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = BASEtbl.Rows(WW_LINECNT)("ENDYMD")

        WF_VALUE1.Text = BASEtbl.Rows(WW_LINECNT)("VALUE1")
        WF_VALUE2.Text = BASEtbl.Rows(WW_LINECNT)("VALUE2")
        WF_VALUE3.Text = BASEtbl.Rows(WW_LINECNT)("VALUE3")
        WF_VALUE4.Text = BASEtbl.Rows(WW_LINECNT)("VALUE4")
        WF_VALUE5.Text = BASEtbl.Rows(WW_LINECNT)("VALUE5")

        WF_NAMES.Text = BASEtbl.Rows(WW_LINECNT)("NAMES")
        WF_NAMEL.Text = BASEtbl.Rows(WW_LINECNT)("NAMEL")

        WF_SYSTEMFLG.Text = BASEtbl.Rows(WW_LINECNT)("SYSTEMKEYFLG")

        WF_DELFLG.Text = BASEtbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

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
            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As New Hashtable

                    If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE Then
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Else
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                    End If
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
        WF_FIELD.Value = ""
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
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        '○項目チェック
        INPtbl_CHEK(WW_ERRCODE)

        'チェックOKデータ(INPtbl)を一覧(BASEtbl)へ反映
        If isNormal(WW_ERRCODE) Then
            BASEtbl_UPD()
        End If

        '○一覧(BASEtbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As Boolean = False

            Do
                For i As Integer = 0 To BASEtbl.Rows.Count - 1
                    If BASEtbl.Rows(i)("TIMSTP") = "0" AndAlso BASEtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        BASEtbl.Rows(i).Delete()
                        WW_DEL = False
                        Exit For
                    Else
                        If (BASEtbl.Rows.Count - 1) <= i Then WW_DEL = True
                    End If
                Next
            Loop Until WW_DEL
        End If

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

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
        WF_SELBUNRUI.Focus()

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

        If WF_LINECNT.Text = "" Then
            INProw("LINECNT") = 0
        Else
            INProw("LINECNT") = WF_LINECNT.Text
        End If

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_BUNRUI.Text)            '分類
        Master.EraseCharToIgnore(WF_KEYCODE.Text)           'マスタキー
        Master.EraseCharToIgnore(WF_VALUE1.Text)            '値１
        Master.EraseCharToIgnore(WF_VALUE2.Text)            '値２
        Master.EraseCharToIgnore(WF_VALUE3.Text)            '値３
        Master.EraseCharToIgnore(WF_VALUE4.Text)            '値４
        Master.EraseCharToIgnore(WF_VALUE5.Text)            '値５
        Master.EraseCharToIgnore(WF_NAMES.Text)             'マスタキー名称（短）
        Master.EraseCharToIgnore(WF_NAMEL.Text)             'マスタキー名称（長）
        Master.EraseCharToIgnore(WF_SYSTEMFLG.Text)         'システムキーフラグ
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        INProw("CAMPCODE") = WF_CAMPCODE.Text
        INProw("CAMPNAMES") = ""
        INProw("STYMD") = WF_STYMD.Text
        INProw("ENDYMD") = WF_ENDYMD.Text
        INProw("CLASS") = WF_BUNRUI.Text
        INProw("CLASSNAMES") = ""
        INProw("KEYCODE") = WF_KEYCODE.Text
        INProw("VALUE1") = WF_VALUE1.Text
        INProw("VALUE2") = WF_VALUE2.Text
        INProw("VALUE3") = WF_VALUE3.Text
        INProw("VALUE4") = WF_VALUE4.Text
        INProw("VALUE5") = WF_VALUE5.Text
        INProw("NAMES") = WF_NAMES.Text
        INProw("NAMEL") = WF_NAMEL.Text
        INProw("SYSTEMKEYFLG") = If(String.IsNullOrEmpty(WF_SYSTEMFLG.Text), "0", WF_SYSTEMFLG.Text)
        INProw("DELFLG") = If(String.IsNullOrEmpty(WF_DELFLG.Text), C_DELETE_FLG.ALIVE, WF_DELFLG.Text)

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_BUNRUI.Text) AndAlso
            String.IsNullOrEmpty(WF_KEYCODE.Text) AndAlso
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

        '○名称付与
        CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAMES"), WW_DUMMY)
        CODENAME_get("CLASS", INProw("CLASS"), INProw("CLASSNAMES"), WW_DUMMY)

        '○チェック用テーブルに登録する
        INPtbl.Rows.Add(INProw)

    End Sub

    ' *** 詳細画面-クリアボタン処理
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

        '○Detail初期設定
        WF_LINECNT.Text = ""

        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""

        WF_BUNRUI.Text = ""
        WF_BUNRUI_TEXT.Text = ""

        WF_KEYCODE.Text = ""
        WF_KEYCODE_TEXT.Text = ""

        WF_VALUE1.Text = ""
        WF_VALUE2.Text = ""
        WF_VALUE3.Text = ""
        WF_VALUE4.Text = ""
        WF_VALUE5.Text = ""

        WF_NAMES.Text = ""
        WF_NAMEL.Text = ""

        WF_SYSTEMFLG.Text = ""

        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELBUNRUI.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim values As String() = leftview.GetActiveValue

        Select Case WF_FIELD.Value
            Case "WF_SELBUNRUI"
                WF_SELBUNRUI_TEXT.Text = values(1)
                WF_SELBUNRUI.Text = values(0)
                WF_SELBUNRUI.Focus()
            Case "WF_CAMPCODE"
                WF_CAMPCODE_TEXT.Text = values(1)
                WF_CAMPCODE.Text = values(0)
                WF_CAMPCODE.Focus()
            Case "WF_BUNRUI"
                WF_BUNRUI_TEXT.Text = values(1)
                WF_BUNRUI.Text = values(0)
                WF_BUNRUI.Focus()
            Case "WF_DELFLG"
                '削除フラグ
                WF_DELFLG.Text = values(0)
                WF_DELFLG_TEXT.Text = values(1)
                WF_DELFLG.Focus()
            Case "WF_STYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = values(0)
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = values(0)
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_SELBUNRUI"
                WF_SELBUNRUI.Focus()
            Case "WF_CAMPCODE"
                WF_CAMPCODE.Focus()
            Case "WF_BUNRUI"
                WF_BUNRUI.Focus()
            Case "WF_STYMD"
                WF_STYMD.Focus()
            Case "WF_ENDYMD"
                WF_ENDYMD.Focus()
            Case "WF_DELFLG"
                WF_DELFLG.Focus()
        End Select

        '画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
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
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0023XLSTBL.MAPID = Master.MAPID                       '画面ID
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
        For Each XLScol As DataColumn In CS0023XLSTBL.TBLDATA.Columns
            WW_COLUMNS.Add(XLScol.ColumnName.ToString())
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
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CLASS") >= 0 AndAlso
                WW_COLUMNS.IndexOf("KEYCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each BASErow As DataRow In BASEtbl.Rows

                    If (XLSTBLrow("CAMPCODE") = BASErow("CAMPCODE") OrElse BASErow("CAMPCODE") = C_DEFAULT_DATAKEY) AndAlso
                        XLSTBLrow("CLASS") = BASErow("CLASS") AndAlso
                        XLSTBLrow("KEYCODE") = BASErow("KEYCODE") AndAlso
                        XLSTBLrow("STYMD") = BASErow("STYMD") Then
                        INProw.ItemArray = BASErow.ItemArray
                        Exit For
                    End If

                Next
            End If

            '○項目セット
            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        INProw("STYMD") = ""
                    Else
                        INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        INProw("ENDYMD") = ""
                    Else
                        INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                End If
            End If

            '分類
            If WW_COLUMNS.IndexOf("CLASS") >= 0 Then
                INProw("CLASS") = XLSTBLrow("CLASS")
            End If

            'マスタキー
            If WW_COLUMNS.IndexOf("KEYCODE") >= 0 Then
                INProw("KEYCODE") = XLSTBLrow("KEYCODE")
            End If

            '会社CD
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '値１
            If WW_COLUMNS.IndexOf("VALUE1") >= 0 Then
                INProw("VALUE1") = XLSTBLrow("VALUE1")
            End If

            '値２
            If WW_COLUMNS.IndexOf("VALUE2") >= 0 Then
                INProw("VALUE2") = XLSTBLrow("VALUE2")
            End If

            '値３
            If WW_COLUMNS.IndexOf("VALUE3") >= 0 Then
                INProw("VALUE3") = XLSTBLrow("VALUE3")
            End If

            '値４
            If WW_COLUMNS.IndexOf("VALUE4") >= 0 Then
                INProw("VALUE4") = XLSTBLrow("VALUE4")
            End If

            '値５
            If WW_COLUMNS.IndexOf("VALUE5") >= 0 Then
                INProw("VALUE5") = XLSTBLrow("VALUE5")
            End If

            'マスタキー名称（短）
            If WW_COLUMNS.IndexOf("NAMES") >= 0 Then
                INProw("NAMES") = XLSTBLrow("NAMES")
            End If

            'マスタキー名称（長）
            If WW_COLUMNS.IndexOf("NAMEL") >= 0 Then
                INProw("NAMEL") = XLSTBLrow("NAMEL")
            End If

            'システムキーフラグ
            If WW_COLUMNS.IndexOf("SYSTEMKEYFLG") >= 0 Then
                INProw("SYSTEMKEYFLG") = XLSTBLrow("SYSTEMKEYFLG")
            End If

            '削除
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAMES"), WW_DUMMY)
            CODENAME_get("CLASS", INProw("CLASS"), INProw("CLASSNAMES"), WW_DUMMY)

            INPtbl.Rows.Add(INProw)
        Next

        '○項目チェック
        INPtbl_CHEK(WW_ERRCODE)

        '○画面表示データ更新
        BASEtbl_UPD()

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_SELBUNRUI.Focus()

        '○Close
        CS0023XLSTBL.TBLDATA.Dispose()
        CS0023XLSTBL.TBLDATA.Clear()

    End Sub



    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベースを検索し画面表示する一覧を作成する</remarks>
    Private Sub MAPDATAget()

        '○画面表示用データ取得

        Try
            '○GridView内容をテーブル退避
            'テンポラリDB項目作成
            If BASEtbl Is Nothing Then BASEtbl = New DataTable

            If BASEtbl.Columns.Count <> 0 Then BASEtbl.Columns.Clear()

            '○DB項目クリア
            BASEtbl.Clear()

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      " SELECT  0                                      as LINECNT ,       " _
                    & "         ''                                     as OPERATION ,     " _
                    & "         TIMSTP = cast(isnull(UPDTIMSTP,0)      as bigint) ,       " _
                    & "         1                                      as 'SELECT' ,      " _
                    & "         0                                      as HIDDEN ,        " _
                    & "         rtrim(CAMPCODE)                        as CAMPCODE ,      " _
                    & "         rtrim(CLASS)                           as CLASS ,         " _
                    & "         rtrim(KEYCODE)                         as KEYCODE ,       " _
                    & "         format(STYMD, 'yyyy/MM/dd')            as STYMD ,         " _
                    & "         format(ENDYMD, 'yyyy/MM/dd')           as ENDYMD ,        " _
                    & "         rtrim(VALUE1)                          as VALUE1 ,        " _
                    & "         rtrim(VALUE2)                          as VALUE2 ,        " _
                    & "         rtrim(VALUE3)                          as VALUE3 ,        " _
                    & "         rtrim(VALUE4)                          as VALUE4 ,        " _
                    & "         rtrim(VALUE5)                          as VALUE5 ,        " _
                    & "         rtrim(NAMES)                           as NAMES ,         " _
                    & "         rtrim(NAMEL)                           as NAMEL ,         " _
                    & "         rtrim(SYSTEMKEYFLG)                    as SYSTEMKEYFLG,   " _
                    & "         rtrim(DELFLG)                          as DELFLG ,        " _
                    & "         ''                                     as CAMPNAMES ,     " _
                    & "         ''                                     as CLASSNAMES      " _
                    & " FROM  MC001_FIXVALUE                                              " _
                    & " WHERE  CAMPCODE = @P1                                             " _
                    & "   and  STYMD   <= @P2                                             " _
                    & "   and  ENDYMD  >= @P3                                             " _
                    & "   and  DELFLG  <> '1'                                             "

                ' 条件指定で指定されたものでＳＱＬで可能なものを追加する
                '分類コード
                If Not String.IsNullOrEmpty(work.WF_SEL_BUNRUIF.Text) OrElse Not String.IsNullOrEmpty(work.WF_SEL_BUNRUIT.Text) Then
                    If Not String.IsNullOrEmpty(work.WF_SEL_BUNRUIF.Text) AndAlso String.IsNullOrEmpty(work.WF_SEL_BUNRUIT.Text) Then
                        SQLStr &= String.Format(" and CLASS = '{0}' ", work.WF_SEL_BUNRUIF.Text)
                    ElseIf String.IsNullOrEmpty(work.WF_SEL_BUNRUIF.Text) AndAlso Not String.IsNullOrEmpty(work.WF_SEL_BUNRUIT.Text) Then
                        SQLStr &= String.Format(" and CLASS = '{0}' ", work.WF_SEL_BUNRUIT.Text)
                    Else
                        SQLStr &= String.Format(" and CLASS >= '{0}' ", work.WF_SEL_BUNRUIF.Text)
                        SQLStr &= String.Format(" and CLASS <= '{0}' ", work.WF_SEL_BUNRUIT.Text)
                    End If
                End If

                SQLStr &= " ORDER BY CAMPCODE , CLASS , KEYCODE  "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text

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
                        CODENAME_get("CAMPCODE", BASErow("CAMPCODE"), BASErow("CAMPNAMES"), WW_DUMMY)       '会社名称
                        CODENAME_get("CLASS", BASErow("CLASS"), BASErow("CLASSNAMES"), WW_DUMMY)            '分類名称
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC001_FIXVALUE SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○システムキーフラグが１のデータは非表示にする。
        If WF_MAPpermitcode.Value <> "SYSTEM" Then
            For Each BASErow As DataRow In BASEtbl.Rows
                If BASErow("SYSTEMKEYFLG") = "1" Then
                    BASErow("HIDDEN") = 1
                End If
            Next
        End If

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = BASEtbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "HIDDEN = '0'"
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
                                                            , {"CLASS", "分類コード"} _
                                                            , {"STYMD", "有効年月日"} _
                                                            , {"ENDYMD", "有効年月日"} _
                                                            , {"DELFLG", "削除"}
                                                            }
        '○単項目チェック(明細情報)
        Dim dicCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"KEYCODE", "マスタキー"} _
                                                            , {"VALUE1", "値１"} _
                                                            , {"VALUE2", "値２"} _
                                                            , {"VALUE3", "値３"} _
                                                            , {"VALUE4", "値４"} _
                                                            , {"VALUE5", "値５"} _
                                                            , {"NAMES", "名称（短）"} _
                                                            , {"NAMEL", "名称（長）"}
                                                            }
        '○単項目チェック(マスタ存在)
        Dim dicMasterCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"CAMPCODE", "会社"} _
                                                            , {"CLASS", "分類コード"}
                                                            }

        '○事前準備（キー重複レコード削除）
        Dim WW_Cnt1 As Integer = 0
        Dim WW_Cnt2 As Integer = 0
        Do Until WW_Cnt1 > (INPtbl.Rows.Count - 1)

            WW_Cnt2 = WW_Cnt1 + 1
            Do Until WW_Cnt2 > (INPtbl.Rows.Count - 1)

                'KEY重複
                If INPtbl.Rows(WW_Cnt1)("CAMPCODE") = INPtbl.Rows(WW_Cnt2)("CAMPCODE") AndAlso
                   INPtbl.Rows(WW_Cnt1)("CLASS") = INPtbl.Rows(WW_Cnt2)("CLASS") AndAlso
                   INPtbl.Rows(WW_Cnt1)("KEYCODE") = INPtbl.Rows(WW_Cnt2)("KEYCODE") AndAlso
                   INPtbl.Rows(WW_Cnt1)("STYMD") = INPtbl.Rows(WW_Cnt2)("STYMD") AndAlso
                   INPtbl.Rows(WW_Cnt1)("ENDYMD") = INPtbl.Rows(WW_Cnt2)("ENDYMD") Then
                    INPtbl.Rows(WW_Cnt2).Delete()
                Else
                    WW_Cnt2 = WW_Cnt2 + 1
                End If
            Loop
            WW_Cnt1 = WW_Cnt1 + 1
        Loop

        '○チェック実行
        For Each INProw As DataRow In INPtbl.Rows

            WW_LINEERR_SW = ""

            '○単項目チェック(ヘッダー情報)
            For Each item In dicKeyCheck

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

            '○関連チェック(キー情報)
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

            '○単項目チェック(システム専用変更)
            If WF_MAPpermitcode.Value <> "SYSTEM" Then
                'チェック
                For Each checkRow As DataRow In BASEtbl.Rows
                    '同一KEYの場合チェック
                    If INProw("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                       INProw("CLASS") = checkRow("CLASS") AndAlso
                       INProw("KEYCODE") = checkRow("KEYCODE") AndAlso
                       checkRow("DELFLG") = C_DELETE_FLG.ALIVE Then
                        If INProw("SYSTEMKEYFLG") <> checkRow("SYSTEMKEYFLG") Then
                            WW_CheckMES1 = "・更新できないレコード(権限違反)です。"
                            WW_CheckMES2 = ""
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, INProw)
                            O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            WW_LINEERR_SW = "ERR"
                        End If
                    End If
                Next
            End If

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
                   BASErow("CLASS") = checkRow("CLASS") AndAlso
                   BASErow("KEYCODE") = checkRow("KEYCODE") AndAlso
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
            'チェック
            For Each checkRow As DataRow In BASEtbl.Rows

                '同一マスタ以外は読み飛ばし
                If BASErow("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                   BASErow("CLASS") = checkRow("CLASS") Then
                Else
                    Continue For
                End If
                '○警告処理
                If (BASErow("SYSTEMKEYFLG") <> checkRow("SYSTEMKEYFLG")) Then
                    WW_CheckMES1 = "・警告：システム専用区分違いが存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.WORNING_RECORD_EXIST, BASErow)
                    O_RTNCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST
                End If
                If (BASErow("NAMES") <> checkRow("NAMES")) Then
                    WW_CheckMES1 = "・警告：名称違いが存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.WORNING_RECORD_EXIST, BASErow)
                    O_RTNCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST
                End If
                If (BASErow("NAMEL") <> checkRow("NAMEL")) Then
                    WW_CheckMES1 = "・警告：名称違いが存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.WORNING_RECORD_EXIST, BASErow)
                    O_RTNCODE = C_MESSAGE_NO.WORNING_RECORD_EXIST
                End If
            Next

            If WW_LINEERR_SW = "" Then
                BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub BASEtbl_UPD()

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
                   BASErow("CLASS") = INProw("CLASS") AndAlso
                   BASErow("KEYCODE") = INProw("KEYCODE") AndAlso
                  (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then

                    INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next

        Next

        '変更無を操作無とする
        For Each INProw As DataRow In INPtbl.Rows
            'エラーレコード読み飛ばし
            If INProw("OPERATION") <> "Update" Then
                Continue For
            End If

            For Each BASErow As DataRow In BASEtbl.Rows
                'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                If BASErow("CAMPCODE") = INProw("CAMPCODE") AndAlso
                   BASErow("CLASS") = INProw("CLASS") AndAlso
                   BASErow("KEYCODE") = INProw("KEYCODE") AndAlso
                  (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then
                Else
                    Continue For
                End If

                If BASErow("ENDYMD") = INProw("ENDYMD") AndAlso
                   BASErow("VALUE1") = INProw("VALUE1") AndAlso
                   BASErow("VALUE2") = INProw("VALUE2") AndAlso
                   BASErow("VALUE3") = INProw("VALUE3") AndAlso
                   BASErow("VALUE4") = INProw("VALUE4") AndAlso
                   BASErow("VALUE5") = INProw("VALUE5") AndAlso
                   BASErow("NAMES") = INProw("NAMES") AndAlso
                   BASErow("NAMEL") = INProw("NAMEL") AndAlso
                   BASErow("SYSTEMKEYFLG") = INProw("SYSTEMKEYFLG") AndAlso
                   BASErow("DELFLG") = INProw("DELFLG") Then
                    '○変更無
                    INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If

                Exit For

            Next
        Next

        'テーブル反映(変更)
        For Each INProw As DataRow In INPtbl.Rows
            Select Case INProw("OPERATION")
                Case "Update"       '○更新（Update）
                    TBL_Update_SUB(INProw)
                Case "Insert"       '○更新（Insert）
                    TBL_Insert_SUB(INProw)
                Case Else
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Update_SUB(ByRef INProw As DataRow)

        For Each BASErow As DataRow In BASEtbl.Rows
            If BASErow("CAMPCODE") = INProw("CAMPCODE") AndAlso
                BASErow("CLASS") = INProw("CLASS") AndAlso
                BASErow("KEYCODE") = INProw("KEYCODE") AndAlso
               (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then

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
    Protected Sub TBL_Insert_SUB(ByRef INProw As DataRow)

        '画面入力テーブル項目設定
        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        Dim BASERow As DataRow = BASEtbl.NewRow
        BASERow.ItemArray = INProw.ItemArray

        'KEY設定
        BASERow("LINECNT") = BASEtbl.Rows.Count + 1
        BASERow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        BASERow("TIMSTP") = "0"
        BASERow("SELECT") = 1
        BASERow("HIDDEN") = 0

        BASEtbl.Rows.Add(BASERow)

    End Sub



    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "CAMPCODE"     '会社名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "DELFLG"       '削除フラグ名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                Case "CLASS"        '分類
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal INPtblRow As DataRow)

        'エラーレポート編集
        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　=" & INPtblRow("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 分類　　　=" & INPtblRow("CLASS") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> マスタキー=" & INPtblRow("KEYCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日=" & INPtblRow("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日=" & INPtblRow("ENDYMD")
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

End Class

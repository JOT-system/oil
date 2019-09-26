Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 組織マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRM00002ORG
    Inherits Page

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

    Private BASEtbl As DataTable                                'Grid格納用テーブル
    Private INPtbl As DataTable                                 'Detail入力用テーブル
    Private UPDtbl As DataTable                                 '更新用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'テーブルオブジェクト作成
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSTBL As New CS0023XLSUPLOAD                 'UPLOAD_XLSデータ取得
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0050Session As New CS0050SESSION                  'セッション管理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

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
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○画面ID設定
        Master.MAPID = GRM00002WRKINC.MAPID
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.createXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ORGCODE.Focus()
        rightview.resetindex()
        leftview.activeListBox()

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
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '詳細-画面初期設定
        Repeater_INIT()

        '○名称付与
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                  '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                   '(絞り込み後)有効Data数

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

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        If isNormal(WW_ERRCODE) Then
            Try
                'ジャーナル出力用テーブル準備
                Master.CreateEmptyTable(UPDtbl)
                'メッセージ初期化
                rightview.setErrorReport("")

                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open()       'DataBase接続(Open)

                    Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu " _
                        & "     FROM M0002_ORG " _
                        & "     WHERE CAMPCODE     = @P1 " _
                        & "     AND   ORGCODE      = @P2 " _
                        & "     AND   STYMD        = @P3 ; " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE M0002_ORG " _
                        & "       SET ENDYMD       = @P4 , " _
                        & "           NAMES        = @P5 , " _
                        & "           NAMEL        = @P6, " _
                        & "           NAMESK       = @P7, " _
                        & "           NAMELK       = @P8, " _
                        & "           KORGCODEH    = @P9 ,  " _
                        & "           KORGCODES    = @P10 , " _
                        & "           KORGCODEB    = @P11, " _
                        & "           KORGCODEK    = @P12, " _
                        & "           HORGCODEH    = @P13 , " _
                        & "           HORGCODES    = @P14 , " _
                        & "           HORGCODEB    = @P15, " _
                        & "           HORGCODEK    = @P16, " _
                        & "           MORGCODE     = @P17 , " _
                        & "           ORGLEVEL     = @P18 , " _
                        & "           DELFLG       = @P19, " _
                        & "           UPDYMD       = @P21 , " _
                        & "           UPDUSER      = @P22 , " _
                        & "           UPDTERMID    = @P23 , " _
                        & "           RECEIVEYMD   = @P24" _
                        & "     WHERE CAMPCODE     = @P1 " _
                        & "       AND ORGCODE      = @P2 " _
                        & "       AND STYMD        = @P3 ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO M0002_ORG " _
                        & "          (CAMPCODE , " _
                        & "           ORGCODE , " _
                        & "           STYMD , " _
                        & "           ENDYMD , " _
                        & "           NAMES, " _
                        & "           NAMEL, " _
                        & "           NAMESK, " _
                        & "           NAMELK , " _
                        & "           KORGCODEH , " _
                        & "           KORGCODES , " _
                        & "           KORGCODEB , " _
                        & "           KORGCODEK , " _
                        & "           HORGCODEH , " _
                        & "           HORGCODES , " _
                        & "           HORGCODEB , " _
                        & "           HORGCODEK , " _
                        & "           MORGCODE , " _
                        & "           ORGLEVEL , " _
                        & "           DELFLG , " _
                        & "           INITYMD , " _
                        & "           UPDYMD , " _
                        & "           UPDUSER , " _
                        & "           UPDTERMID , " _
                        & "           RECEIVEYMD" _
                        & "       ) " _
                        & "        VALUES (@P1,@P2,@P3,@P4,@P5,@P6,@P7,@P8,@P9,@P10,@P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,@P21,@P22,@P23,@P24) ; " _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

                    Dim SQLStr2 As String =
                          " SELECT  CAMPCODE, ORGCODE , STYMD, ENDYMD, NAMES , NAMEL, NAMESK , NAMELK , " _
                        & "        KORGCODEH , KORGCODES , KORGCODEB , KORGCODEK , " _
                        & "        HORGCODEH , HORGCODES , HORGCODEB , HORGCODEK , " _
                        & "        MORGCODE , ORGLEVEL , DELFLG , " _
                        & "    INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                        & " FROM  M0002_ORG " _
                        & "  WHERE CAMPCODE = @P1 " _
                        & "    and ORGCODE = @P2 " _
                        & "    and STYMD = @P3"

                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd2 As New SqlCommand(SQLStr2, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar)
                        Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar)
                        Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar)
                        Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar)
                        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar)
                        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)
                        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
                        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)
                        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar)
                        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar)
                        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar)
                        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar)
                        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar)
                        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar)
                        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.DateTime)
                        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.DateTime)
                        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar)
                        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar)
                        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.DateTime)

                        Dim PARA2_1 As SqlParameter = SQLcmd2.Parameters.Add("@P1", SqlDbType.NVarChar)
                        Dim PARA2_2 As SqlParameter = SQLcmd2.Parameters.Add("@P2", SqlDbType.NVarChar)
                        Dim PARA2_3 As SqlParameter = SQLcmd2.Parameters.Add("@P3", SqlDbType.Date)

                        Dim WW_DATENOW As DateTime = Date.Now

                        'ＤＢ更新
                        '　※エラーは処理されない
                        For Each BASErow As DataRow In BASEtbl.Rows
                            '削除は更新しない（操作欄のみクリア）
                            If BASErow("DELFLG") = C_DELETE_FLG.DELETE AndAlso BASErow("TIMSTP") = "0" Then
                                BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                For Each DELrow As DataRow In BASEtbl.Rows
                                    If DELrow("CAMPCODE") = BASErow("CAMPCODE") AndAlso
                                        DELrow("ORGCODE") = BASErow("ORGCODE") AndAlso
                                        DELrow("STYMD") = BASErow("STYMD") Then
                                        DELrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                                    End If
                                Next

                                Continue For
                            End If

                            If BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                                BASErow("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then

                                PARA1.Value = BASErow("CAMPCODE")
                                PARA2.Value = BASErow("ORGCODE")
                                PARA3.Value = BASErow("STYMD")
                                PARA4.Value = BASErow("ENDYMD")
                                PARA5.Value = BASErow("NAMES")
                                PARA6.Value = BASErow("NAMEL")
                                PARA7.Value = BASErow("NAMESK")
                                PARA8.Value = BASErow("NAMELK")

                                PARA9.Value = BASErow("KORGCODEH")
                                PARA10.Value = BASErow("KORGCODES")
                                PARA11.Value = BASErow("KORGCODEB")
                                PARA12.Value = BASErow("KORGCODEK")

                                PARA13.Value = BASErow("HORGCODEH")
                                PARA14.Value = BASErow("HORGCODES")
                                PARA15.Value = BASErow("HORGCODEB")
                                PARA16.Value = BASErow("HORGCODEK")

                                PARA17.Value = BASErow("MORGCODE")
                                PARA18.Value = BASErow("ORGLEVEL")
                                PARA19.Value = BASErow("DELFLG")

                                PARA20.Value = WW_DATENOW
                                PARA21.Value = WW_DATENOW
                                PARA22.Value = Master.USERID
                                PARA23.Value = Master.USERTERMID
                                PARA24.Value = C_DEFAULT_YMD

                                SQLcmd.ExecuteNonQuery()

                                '結果 --> テーブル(BASEtbl)反映
                                BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                                '○更新ジャーナル追加
                                Try
                                    PARA2_1.Value = BASErow("CAMPCODE")
                                    PARA2_2.Value = BASErow("ORGCODE")
                                    PARA2_3.Value = BASErow("STYMD")

                                    Dim JOURds As New DataSet()
                                    Dim SQLadp As SqlDataAdapter

                                    SQLadp = New SqlDataAdapter(SQLcmd2)
                                    SQLadp.Fill(JOURds, "JOURtbl")

                                    CS0020JOURNAL.TABLENM = "M0002_ORG"
                                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                    CS0020JOURNAL.ROW = JOURds.Tables("JOURtbl").Rows(0)
                                    CS0020JOURNAL.CS0020JOURNAL()
                                    If Not isNormal(CS0020JOURNAL.ERR) Then
                                        Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                        CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                        CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                        CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                                        Exit Sub
                                    End If

                                    BASErow("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")

                                    SQLadp.Dispose()
                                    SQLadp = Nothing
                                Catch ex As Exception
                                    If ex.Message = "Error raised in TIMSTP" Then
                                        BASErow.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                    End If

                                    Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "M0002_ORG JOURNAL")
                                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                    CS0011LOGWRITE.INFPOSI = "DB:M0002_ORG JOURNAL"
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
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "M0002_ORG UPDATE_INSERT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:M0002_ORG UPDATE_INSERT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                Exit Sub
            End Try
        End If

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_ORGCODE.Focus()

    End Sub

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
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
        WF_ORGCODE.Focus()

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
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        WF_ORGCODE.Focus()

    End Sub

    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub
    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
        WF_ORGCODE.Focus()
    End Sub
    ''' <summary>
    ''' 最終頁遷移ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView = New DataView(BASEtbl)
        WW_TBLview.RowFilter = "HIDDEN = 0"

        '○最終頁に移動
        If WW_TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10) + 1
        End If

        WF_ORGCODE.Focus()

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

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT = WW_LINECNT - 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○Grid内容(BASEtbl)よりDetail編集

        WF_Sel_LINECNT.Text = BASEtbl.Rows(WW_LINECNT)("LINECNT")

        '会社
        WF_CAMPCODE.Text = BASEtbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WW_TEXT, WW_DUMMY)
        WF_CAMPCODE_TEXT.Text = WW_TEXT

        '組織コード
        WF_ORGCODE.Text = BASEtbl.Rows(WW_LINECNT)("ORGCODE")

        '有効年月日
        WF_STYMD.Text = BASEtbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = BASEtbl.Rows(WW_LINECNT)("ENDYMD")

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
        WF_ORGCODE.Focus()

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
                        prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORGLEVEL")
                    Else
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                    End If

                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text
                    End Select
                    .activeCalendar()
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
        WF_FIELD_REP.Value = ""
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
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()

        WF_ORGCODE.Focus()

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()

        WF_ORGCODE.Focus()

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

        '○DetailBoxをINPtblへ退避
        DetailBoxToINPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPtbl_CHEK(WW_ERRCODE)

        'チェックOKデータ(INPtbl)を一覧(BASEtbl)へ反映
        If isNormal(WW_ERRCODE) Then
            BASEtbl_UPD()
        End If

        '○一覧(BASEtbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As String = "ON"

            Do
                For i As Integer = 0 To BASEtbl.Rows.Count - 1
                    If BASEtbl.Rows(i)("TIMSTP") = "0" AndAlso BASEtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        BASEtbl.Rows(i).Delete()
                        WW_DEL = "OFF"
                        Exit For
                    Else
                        If (BASEtbl.Rows.Count - 1) <= i Then
                            WW_DEL = "ON"
                        End If
                    End If
                Next
            Loop Until WW_DEL = "ON"

        End If

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        'Detailクリア
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
        WF_ORGCODE.Focus()

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
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_ORGCODE.Text)           '組織コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        INProw("CAMPCODE") = WF_CAMPCODE.Text
        INProw("ORGCODE") = WF_ORGCODE.Text
        INProw("STYMD") = WF_STYMD.Text
        INProw("ENDYMD") = WF_ENDYMD.Text
        INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_ORGCODE.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then

            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
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
        INProw("CAMPNAMES") = ""
        CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAMES"), WW_DUMMY)             '会社名称
        INProw("ORGLEVELNAMES") = ""
        CODENAME_get("ORGLEVEL", INProw("ORGLEVEL"), INProw("ORGLEVELNAMES"), WW_DUMMY)         '組織レベル

        '○チェック用テーブルに登録する
        INPtbl.Rows.Add(INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
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
        WF_ORGCODE.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        '○名称付与
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '○Detail初期設定
        Repeater_INIT()

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_ORGCODE.Focus()

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
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
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
            Case "ORGLEVEL"
                '組織レベル
                O_ATTR = "REF_Field_DBclick('ORGLEVEL', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_FIX_VALUE & "');"
            Case Else
                O_ATTR = String.Empty
        End Select

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
                Case "WF_STYMD"         '開始年月日
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

                Case "WF_ENDYMD"        '終了年月日
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

                Case "WF_DELFLG"        '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectTEXT
                    WF_DELFLG.Focus()
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
        End If

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
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
        rightview.setErrorReport("")

        '○UPLOAD_XLSデータ取得
        CS0023XLSTBL.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSTBL.MAPID = Master.MAPID
        CS0023XLSTBL.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSTBL.ERR) Then
            If CS0023XLSTBL.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSTBL.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
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
                WW_COLUMNS.IndexOf("ORGCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each BASErow As DataRow In BASEtbl.Rows
                    If XLSTBLrow("CAMPCODE") = BASErow("CAMPCODE") AndAlso
                        XLSTBLrow("ORGCODE") = BASErow("ORGCODE") AndAlso
                        XLSTBLrow("STYMD") = BASErow("STYMD") Then
                        INProw.ItemArray = BASErow.ItemArray
                        Exit For
                    End If
                Next
            End If

            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '組織コード
            If WW_COLUMNS.IndexOf("ORGCODE") >= 0 Then
                INProw("ORGCODE") = XLSTBLrow("ORGCODE")
            End If

            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                If WW_DATE < C_DEFAULT_YMD Then
                    INProw("STYMD") = ""
                Else
                    INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                If WW_DATE < C_DEFAULT_YMD Then
                    INProw("ENDYMD") = ""
                Else
                    INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '組織名称(短)
            If WW_COLUMNS.IndexOf("NAMES") >= 0 Then
                INProw("NAMES") = XLSTBLrow("NAMES")
            End If

            '組織名称(長)
            If WW_COLUMNS.IndexOf("NAMEL") >= 0 Then
                INProw("NAMEL") = XLSTBLrow("NAMEL")
            End If

            '組織カナ名称(短)
            If WW_COLUMNS.IndexOf("NAMESK") >= 0 Then
                INProw("NAMESK") = XLSTBLrow("NAMESK")
            End If

            '組織カナ名称(長)
            If WW_COLUMNS.IndexOf("NAMELK") >= 0 Then
                INProw("NAMELK") = XLSTBLrow("NAMELK")
            End If

            '経理組織コード(本部)
            If WW_COLUMNS.IndexOf("KORGCODEH") >= 0 Then
                INProw("KORGCODEH") = XLSTBLrow("KORGCODEH")
            End If

            '経理組織コード(本支店)
            If WW_COLUMNS.IndexOf("KORGCODES") >= 0 Then
                INProw("KORGCODES") = XLSTBLrow("KORGCODES")
            End If

            '経理組織コード(部)
            If WW_COLUMNS.IndexOf("KORGCODEB") >= 0 Then
                INProw("KORGCODEB") = XLSTBLrow("KORGCODEB")
            End If

            '経理組織コード(課)
            If WW_COLUMNS.IndexOf("KORGCODEK") >= 0 Then
                INProw("KORGCODEK") = XLSTBLrow("KORGCODEK")
            End If

            '人事組織コード(本部)
            If WW_COLUMNS.IndexOf("HORGCODEH") >= 0 Then
                INProw("HORGCODEH") = XLSTBLrow("HORGCODEH")
            End If

            '人事組織コード(本支店)
            If WW_COLUMNS.IndexOf("HORGCODES") >= 0 Then
                INProw("HORGCODES") = XLSTBLrow("HORGCODES")
            End If

            '人事組織コード(部)
            If WW_COLUMNS.IndexOf("HORGCODEB") >= 0 Then
                INProw("HORGCODEB") = XLSTBLrow("HORGCODEB")
            End If

            '人事組織コード(課)
            If WW_COLUMNS.IndexOf("HORGCODEK") >= 0 Then
                INProw("HORGCODEK") = XLSTBLrow("HORGCODEK")
            End If

            '管理組織コード
            If WW_COLUMNS.IndexOf("MORGCODE") >= 0 Then
                INProw("MORGCODE") = XLSTBLrow("MORGCODE")
            End If

            '組織レベル
            If WW_COLUMNS.IndexOf("ORGLEVEL") >= 0 Then
                INProw("ORGLEVEL") = XLSTBLrow("ORGLEVEL")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '名称取得
            CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAMES"), WW_DUMMY)             '会社コード
            CODENAME_get("ORGLEVEL", INProw("ORGLEVEL"), INProw("ORGLEVELNAMES"), WW_DUMMY)         '組織レベル

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
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_ORGCODE.Focus()

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
            If BASEtbl Is Nothing Then
                BASEtbl = New DataTable
            End If

            If BASEtbl.Columns.Count <> 0 Then
                BASEtbl.Columns.Clear()
            End If

            '○DB項目クリア
            BASEtbl.Clear()

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      " SELECT  0                                       as LINECNT ,       " _
                    & "         ''                                      as OPERATION ,     " _
                    & "         TIMSTP = cast(isnull(UPDTIMSTP,0)       as bigint) ,       " _
                    & "         1                                       as 'SELECT' ,      " _
                    & "         0                                       as HIDDEN ,        " _
                    & "         isnull(rtrim(CAMPCODE),'')              as CAMPCODE,       " _
                    & "         isnull(rtrim(ORGCODE),'')               as ORGCODE ,       " _
                    & "         isnull(format(STYMD, 'yyyy/MM/dd'),'')  as STYMD ,         " _
                    & "         isnull(format(ENDYMD, 'yyyy/MM/dd'),'') as ENDYMD ,        " _
                    & "         isnull(rtrim(NAMES),'')                 as NAMES ,         " _
                    & "         isnull(rtrim(NAMEL),'')                 as NAMEL ,         " _
                    & "         isnull(rtrim(NAMESK),'')                as NAMESK ,        " _
                    & "         isnull(rtrim(NAMELK),'')                as NAMELK ,        " _
                    & "         isnull(rtrim(KORGCODEH),'')             as KORGCODEH ,     " _
                    & "         isnull(rtrim(KORGCODES),'')             as KORGCODES ,     " _
                    & "         isnull(rtrim(KORGCODEB),'')             as KORGCODEB ,     " _
                    & "         isnull(rtrim(KORGCODEK),'')             as KORGCODEK ,     " _
                    & "         isnull(rtrim(HORGCODEH),'')             as HORGCODEH ,     " _
                    & "         isnull(rtrim(HORGCODES),'')             as HORGCODES ,     " _
                    & "         isnull(rtrim(HORGCODEB),'')             as HORGCODEB ,     " _
                    & "         isnull(rtrim(HORGCODEK),'')             as HORGCODEK ,     " _
                    & "         isnull(rtrim(MORGCODE),'')              as MORGCODE ,      " _
                    & "         isnull(rtrim(ORGLEVEL),'')              as ORGLEVEL ,      " _
                    & "         isnull(rtrim(DELFLG),'')                as DELFLG  ,       " _
                    & "         ''                                      as CAMPNAMES ,     " _
                    & "         ''                                      as ORGLEVELNAMES   " _
                    & " FROM " _
                    & "     M0002_ORG " _
                    & " WHERE  CAMPCODE = @P1                                " _
                    & "   and  STYMD   <= @P2                                " _
                    & "   and  ENDYMD  >= @P3                                " _
                    & "   and  DELFLG  <> '1'                                " _
                    & " ORDER BY ORGCODE "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.Char, 20)
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
                        CODENAME_get("CAMPCODE", BASErow("CAMPCODE"), BASErow("CAMPNAMES"), WW_DUMMY)           '会社コード
                        CODENAME_get("ORGLEVEL", BASErow("ORGLEVEL"), BASErow("ORGLEVELNAMES"), WW_DUMMY)       '組織レベル
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "M0002_ORG SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                     'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0002_ORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力
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
        rightview.setErrorReport("")

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
                                                            , {"ORGCODE", "組織コード"} _
                                                            , {"STYMD", "有効年月日"} _
                                                            , {"ENDYMD", "有効年月日"} _
                                                            , {"DELFLG", "削除"}
                                                            }
        '○単項目チェック(明細情報)
        Dim dicCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"NAMES", "組織名称（短）"} _
                                                            , {"NAMEL", "組織名称（長）"} _
                                                            , {"NAMESK", "組織カナ名称（短）"} _
                                                            , {"NAMELK", "組織カナ名称（長）"} _
                                                            , {"KORGCODEH", "経理組織コード（本部）"} _
                                                            , {"KORGCODES", "経理組織コード（本支店）"} _
                                                            , {"KORGCODEB", "経理組織コード（部）"} _
                                                            , {"KORGCODEK", "経理組織コード（課）"} _
                                                            , {"HORGCODEH", "人事組織コード（本部）"} _
                                                            , {"HORGCODES", "人事組織コード（本支店）"} _
                                                            , {"HORGCODEB", "人事組織コード（部）"} _
                                                            , {"HORGCODEK", "人事組織コード（課）"} _
                                                            , {"MORGCODE", "管理部署コード"} _
                                                            , {"ORGLEVEL", "組織レベル"}
                                                            }
        '○単項目チェック(マスタ存在)
        Dim dicMasterCheck As Dictionary(Of String, String) = New Dictionary(Of String, String) _
                                                        From {
                                                              {"CAMPCODE", "会社"} _
                                                            , {"ORGLEVEL", "組織レベル"}
                                                            }

        '○事前準備（キー重複レコード削除）
        Dim WW_Cnt1 As Integer = 0
        Dim WW_Cnt2 As Integer = 0
        Do Until WW_Cnt1 > (INPtbl.Rows.Count - 1)

            WW_Cnt2 = WW_Cnt1 + 1
            Do Until WW_Cnt2 > (INPtbl.Rows.Count - 1)

                'KEY重複
                If INPtbl.Rows(WW_Cnt1)("CAMPCODE") = INPtbl.Rows(WW_Cnt2)("CAMPCODE") AndAlso
                    INPtbl.Rows(WW_Cnt1)("ORGCODE") = INPtbl.Rows(WW_Cnt2)("ORGCODE") AndAlso
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
                Master.checkFIeld(WF_CAMPCODE.Text, item.Key, INProw(item.Key), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
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
                Master.checkFIeld(WF_CAMPCODE.Text, item.Key, INProw(item.Key), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'LeftBox存在チェック
                    If String.IsNullOrEmpty(WW_TEXT) Then
                        INProw(item.Key) = String.Empty
                    Else
                        If dicMasterCheck.ContainsKey(item.Key) Then
                            ' 組織レベルで以下の値の時はマスタチェック除外
                            If item.Key = "ORGLEVEL" AndAlso
                                (INProw(item.Key) = "00001" OrElse INProw(item.Key) = "02000") Then
                            Else
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
                   BASErow("ORGCODE") = checkRow("ORGCODE") AndAlso
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
                    BASErow("ORGCODE") = INProw("ORGCODE") AndAlso
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
                    BASErow("ORGCODE") = INProw("ORGCODE") AndAlso
                   (BASErow("STYMD") = INProw("STYMD") OrElse BASErow("STYMD") = "") Then
                Else
                    Continue For
                End If

                If INProw("ENDYMD") = BASErow("ENDYMD") AndAlso
                    INProw("NAMES") = BASErow("NAMES") AndAlso
                    INProw("NAMEL") = BASErow("NAMEL") AndAlso
                    INProw("NAMESK") = BASErow("NAMESK") AndAlso
                    INProw("NAMELK") = BASErow("NAMELK") AndAlso
                    INProw("KORGCODEH") = BASErow("KORGCODEH") AndAlso
                    INProw("KORGCODES") = BASErow("KORGCODES") AndAlso
                    INProw("KORGCODEB") = BASErow("KORGCODEB") AndAlso
                    INProw("KORGCODEK") = BASErow("KORGCODEK") AndAlso
                    INProw("HORGCODEH") = BASErow("HORGCODEH") AndAlso
                    INProw("HORGCODES") = BASErow("HORGCODES") AndAlso
                    INProw("HORGCODEB") = BASErow("HORGCODEB") AndAlso
                    INProw("HORGCODEK") = BASErow("HORGCODEK") AndAlso
                    INProw("MORGCODE") = BASErow("MORGCODE") AndAlso
                    INProw("ORGLEVEL") = BASErow("ORGLEVEL") AndAlso
                    Trim(INProw("DELFLG")) = Trim(BASErow("DELFLG")) Then
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
                BASErow("ORGCODE") = INProw("ORGCODE") AndAlso
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
                Case "ORGLEVEL"     '組織レベル
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORGLEVEL"))
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

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　　　=" & INPtblRow("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 組織　　　　　=" & INPtblRow("ORGCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日　　=" & INPtblRow("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日　　=" & INPtblRow("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 名称（短）　　=" & INPtblRow("NAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 名称（長）　　=" & INPtblRow("NAMEL") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 名称（カナ短）=" & INPtblRow("NAMESK") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 名称（カナ長）=" & INPtblRow("NAMELK") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 組織レベル　　=" & INPtblRow("ORGLEVEL") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　　　　=" & INPtblRow("DELFLG") & "  "
        rightview.addErrorReport(WW_ERR_MES)

    End Sub

End Class

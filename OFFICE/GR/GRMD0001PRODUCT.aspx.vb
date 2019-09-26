Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 品名マスタ（登録）
''' </summary>
''' <remarks></remarks>
Public Class GRMD0001PRODUCT
    Inherits Page

    '検索結果格納
    Private MD0001tbl As DataTable                              'Grid格納用テーブル
    Private MD0001INPtbl As DataTable                           'チェック用テーブル

    '共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(APサーバチェックなし)
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORl As New CS0030REPORT                    '帳票出力
    Private CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

    '共通処理結果
    Private WW_ERRCODE As String = String.Empty                 'リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

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
                    If Not Master.RecoverTable(MD0001tbl) Then
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
                            WF_UPDATE_CLICK()
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
            If Not IsNothing(MD0001tbl) Then
                MD0001tbl.Clear()
                MD0001tbl.Dispose()
                MD0001tbl = Nothing
            End If

            If Not IsNothing(MD0001INPtbl) Then
                MD0001INPtbl.Clear()
                MD0001INPtbl.Dispose()
                MD0001INPtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        WF_FIELD.Value = ""
        WF_SELOILTYPE.Focus()
        rightview.ResetIndex()
        leftview.ActiveListBox()
        MAPrefelence()
        '○ヘルプ無
        Master.dispHelp = False
        '○ドラックアンドドロップON
        Master.eventDrop = True

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(MD0001tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRMD0001WRKINC.MAPID
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
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If
        '詳細-画面初期設定
        Repeater_INIT()
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
        For Each MD0001row As DataRow In MD0001tbl.Rows
            If MD0001row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MD0001row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MD0001tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRMD0001WRKINC.MAPID
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
        WF_SELOILTYPE.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  絞り込みボタン処理                                                    ***
    ' ******************************************************************************
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each MD0001row As DataRow In MD0001tbl.Rows

            Dim WW_HANTEI As Integer = 0

            ' 品目コードによる絞込判定
            If WF_SELOILTYPE.Text = "" Then
                WW_HANTEI = WW_HANTEI + 0
                WF_SELOILTYPE_TEXT.Text = ""
            Else
                Dim wstr As String = MD0001row("PRODUCTCODE")
                If wstr.Substring(2).StartsWith(WF_SELOILTYPE.Text) Then
                    WW_HANTEI = WW_HANTEI + 0
                Else
                    WW_HANTEI = WW_HANTEI + 1
                End If
            End If

            '画面(Grid)のHIDDEN列に結果格納
            If WW_HANTEI = 0 Then
                MD0001row("HIDDEN") = 0     '表示対象
            Else
                MD0001row("HIDDEN") = 1     '非表示対象
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        '○画面表示
        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELOILTYPE.Focus()

    End Sub


    ' ******************************************************************************
    ' ***  DB更新ボタン処理                                                      ***
    ' ******************************************************************************
    ''' <summary>
    ''' DB更新ボタン押下処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ 関連チェック
        RelatedCheck(WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then

            '○メッセージ表示
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ABORT)

            '○画面表示データ保存
            Master.SaveTable(MD0001tbl)
            Exit Sub
        End If
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                                                                    " _
                    & " set @hensuu = 0 ;                                                                              " _
                    & " DECLARE hensuu CURSOR FOR                                                                      " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                                                   " _
                    & "     FROM    MD001_PRODUCT                                                                      " _
                    & "     WHERE CAMPCODE =@P17 and PRODUCTCODE = @P16 and STYMD = @P5 ;                              " _
                    & " OPEN hensuu ;                                                                                  " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                                          " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                                      " _
                    & "    UPDATE   MD001_PRODUCT                                                                      " _
                    & "       SET                                                                                      " _
                    & "         OILTYPE = @P1 , PRODUCT1 = @P2 , PRODUCT2 = @P3 ,                                       " _
                    & " SEQ = @P4 , ENDYMD = @P6 , NAMES = @P7 , NAMEL = @P8, DELFLG = @P9 ,                           " _
                    & "           UPDYMD = @P11 , UPDUSER = @P12 , UPDTERMID = @P13 , RECEIVEYMD = @P14 , STANI = @P15 " _
                    & "     WHERE CAMPCODE =@P17 and PRODUCTCODE = @P16 and STYMD = @P5                                " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                                     " _
                    & "    INSERT INTO MD001_PRODUCT                                                                   " _
                    & "       (OILTYPE , PRODUCT1 , PRODUCT2, SEQ, STYMD , ENDYMD , NAMES, NAMEL, DELFLG,              " _
                    & "        INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , STANI, PRODUCTCODE, CAMPCODE)     " _
                    & "        VALUES (@P1,@P2,@P3,@P4,@P5,@P6,@P7,@P8,@P9,@P10,@P11,@P12,@P13,@P14,@P15,@P16,@P17) ;  " _
                    & " CLOSE hensuu ;                                                                                 " _
                    & " DEALLOCATE hensuu ;                                                                            "

                Dim SQLStr1 As String =
                      " SELECT  CAMPCODE, PRODUCTCODE, OILTYPE, PRODUCT1 , PRODUCT2, SEQ, STANI, STYMD, ENDYMD, NAMES , NAMEL, DELFLG," _
                    & "    INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                    & " FROM  MD001_PRODUCT " _
                    & "  WHERE CAMPCODE = @P1 " _
                    & "    and PRODUCTCODE = @P2 " _
                    & "    and STYMD = @P3 "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmd1 As New SqlCommand(SQLStr1, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Int)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar)
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar)
                    Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar)

                    Dim PARAS1 As SqlParameter = SQLcmd1.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARAS2 As SqlParameter = SQLcmd1.Parameters.Add("@P2", SqlDbType.NVarChar)
                    Dim PARAS3 As SqlParameter = SQLcmd1.Parameters.Add("@P3", SqlDbType.Date)

                    '○ＤＢ更新
                    For Each MD0001row As DataRow In MD0001tbl.Rows
                        If Trim(MD0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                           Trim(MD0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            '※追加レコードは、MD0001tbl.Rows(i)("TIMSTP") = "0"となっているが状態のみで判定

                            PARA1.Value = MD0001row("OILTYPE")
                            PARA2.Value = MD0001row("PRODUCT1")
                            PARA3.Value = MD0001row("PRODUCT2")
                            PARA4.Value = MD0001row("SEQ")
                            PARA5.Value = MD0001row("STYMD")
                            PARA6.Value = MD0001row("ENDYMD")
                            PARA7.Value = MD0001row("PRODUCTNAMES")
                            PARA8.Value = MD0001row("PRODUCTNAMEL")
                            PARA9.Value = MD0001row("DELFLG")
                            PARA10.Value = Date.Now
                            PARA11.Value = Date.Now
                            PARA12.Value = Master.USERID
                            PARA13.Value = Master.USERTERMID
                            PARA14.Value = C_DEFAULT_YMD
                            PARA15.Value = MD0001row("STANI")
                            PARA16.Value = MD0001row("PRODUCTCODE")
                            PARA17.Value = MD0001row("CAMPCODE")

                            SQLcmd.ExecuteNonQuery()

                            MD0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Try
                                PARAS1.Value = MD0001row("CAMPCODE")
                                PARAS2.Value = MD0001row("PRODUCTCODE")
                                PARAS3.Value = MD0001row("STYMD")

                                Dim JOURds As New DataSet()
                                Dim SQLadp As SqlDataAdapter

                                SQLadp = New SqlDataAdapter(SQLcmd1)
                                SQLadp.Fill(JOURds, "JOURtbl")

                                CS0020JOURNAL.TABLENM = "MD001_PRODUCT"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = JOURds.Tables("JOURtbl").Rows(0)
                                CS0020JOURNAL.CS0020JOURNAL()
                                If Not isNormal(CS0020JOURNAL.ERR) Then
                                    Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                    CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                    CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                    CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                                    Exit Sub
                                End If

                                MD0001row("TIMSTP") = JOURds.Tables("JOURtbl").Rows(0)("TIMSTP")

                                SQLadp.Dispose()
                                SQLadp = Nothing
                            Catch ex As Exception
                                If ex.Message = "Error raised in TIMSTP" Then
                                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If
                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD001_PRODUCT JOURNAL")

                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT JOURNAL"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD001_PRODUCT UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        '詳細画面クリア
        Detailbox_Clear()

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELOILTYPE.Focus()

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
        CS0030REPORl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.MAPID = GRMD0001WRKINC.MAPID
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "pdf"
        CS0030REPORl.TBLDATA = MD0001tbl
        CS0030REPORl.CS0030REPORT()

        If Not isNormal(CS0030REPORl.ERR) Then
            If CS0030REPORl.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORl.URL
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

        '○帳票出力
        CS0030REPORl.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0030REPORl.MAPID = GRMD0001WRKINC.MAPID
        CS0030REPORl.PROFID = Master.PROF_REPORT
        CS0030REPORl.REPORTID = rightview.GetReportId()
        CS0030REPORl.FILEtyp = "XLSX"
        CS0030REPORl.TBLDATA = MD0001tbl
        CS0030REPORl.CS0030REPORT()
        If Not isNormal(CS0030REPORl.ERR) Then
            If CS0030REPORl.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ERR)
            Else

                Master.Output(CS0030REPORl.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If
        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORl.URL
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
        WW_TBLview = New DataView(MD0001tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○先頭頁に移動
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

        '○抽出条件(ヘッダーレコードより)定義
        Dim WW_Position As Integer = 0
        Dim WW_FILED_OBJ As Object
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_LINECNT As Integer

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_Position)
            WW_Position = WW_Position - 1
            WW_LINECNT = WW_Position
        Catch ex As Exception
            Exit Sub
        End Try

        '○ダブルクリック明細情報取得設定（GridView --> Detailboxヘッダー情報)
        '選択行
        WF_Sel_LINECNT.Text = MD0001tbl.Rows(WW_Position)("LINECNT")
        WF_CAMPCODE.Text = MD0001tbl.Rows(WW_Position)("CAMPCODE")
        WF_CAMPCODE_TEXT.Text = MD0001tbl.Rows(WW_Position)("CAMPNAMES")
        WF_OILTYPE.Text = MD0001tbl.Rows(WW_Position)("OILTYPE")
        WF_OILTYPE_TEXT.Text = MD0001tbl.Rows(WW_Position)("OILTYPENAMES")
        WF_PRODUCT1.Text = MD0001tbl.Rows(WW_Position)("PRODUCT1")
        WF_PRODUCT1_TEXT.Text = MD0001tbl.Rows(WW_Position)("PRODUCT1NAMES")
        WF_PRODUCT2.Text = MD0001tbl.Rows(WW_Position)("PRODUCT2")
        WF_PRODUCTCODE_TEXT.Text = MD0001tbl.Rows(WW_Position)("PRODUCTCODE")
        '表示順をHiddenに設定
        WF_SEQ.Value = MD0001tbl.Rows(WW_Position)("SEQ").ToString()
        '有効年月日
        WF_STYMD.Text = MD0001tbl.Rows(WW_Position)("STYMD")
        WF_ENDYMD.Text = MD0001tbl.Rows(WW_Position)("ENDYMD")
        '削除フラグ
        WF_DELFLG.Text = MD0001tbl.Rows(WW_Position)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MD0001tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MD0001tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = WF_ITEM_FORMAT(WW_FILED_OBJ.text, MD0001tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MD0001Row As DataRow In MD0001tbl.Rows
            Select Case MD0001Row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MD0001tbl.Rows(WW_Position)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MD0001tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MD0001tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MD0001tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MD0001tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MD0001tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        WF_OILTYPE.Focus()
        WF_GridDBclick.Text = ""

    End Sub

    Protected Function WF_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String

        WF_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    WF_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
            Case Else
        End Select

    End Function


    ' *** 一覧画面-スクロールSUB

    ' *** 一覧画面-非表示列削除（性能対策）

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_CLICK()

        '○エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_ERR10023 As String = C_MESSAGE_NO.NORMAL

        '○DetailBoxをMD0001INPtblへ退避
        Master.CreateEmptyTable(MD0001INPtbl)
        DetailBoxToMD0001INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If
        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○GridView更新
        If isNormal(WW_ERRCODE) Then
            MD0001tbl_UPD()
        End If

        '○一覧(MD0001tbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERRCODE) Then
            Dim WW_DEL As String = "ON"
            Do
                For i As Integer = 0 To MD0001tbl.Rows.Count - 1
                    If MD0001tbl.Rows(i)("TIMSTP") = 0 AndAlso MD0001tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        MD0001tbl.Rows(i).Delete()
                        WW_DEL = "OFF"
                        Exit For
                    Else
                        If (MD0001tbl.Rows.Count - 1) <= i Then
                            WW_DEL = "ON"
                        End If
                    End If
                Next
            Loop Until WW_DEL = "ON"
        End If

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

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
        WF_SELOILTYPE.Focus()

    End Sub

    ''' <summary>
    '''  詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMD0001INPtbl(ByRef O_RTNCODE As String)

        Dim WW_TEXT As String = String.Empty
        Dim WW_RTN As String = String.Empty

        O_RTNCODE = C_MESSAGE_NO.NORMAL

        'MD0001テンポラリDB項目作成
        Master.CreateEmptyTable(MD0001INPtbl)

        '○入力文字置き換え & CS0007CHKテーブルレコード追加

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_OILTYPE.Text)           '油種
        Master.EraseCharToIgnore(WF_PRODUCT1.Text)          '品名１
        Master.EraseCharToIgnore(WF_PRODUCT2.Text)          '品名２
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_OILTYPE.Text) AndAlso
            String.IsNullOrEmpty(WF_PRODUCT1.Text) AndAlso
            String.IsNullOrEmpty(WF_PRODUCT2.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMD0001INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTNCODE = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○画面(Repeaterヘッダー情報)のテーブル退避
        Dim MD0001INProw As DataRow = MD0001INPtbl.NewRow
        '初期クリア
        For Each MD0001INPcol As DataColumn In MD0001INProw.Table.Columns
            If MD0001INPcol.DataType.Name.ToString() = "String" Then
                MD0001INProw(MD0001INPcol.ColumnName) = ""
            End If
        Next

        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text)) Then
            MD0001INProw("LINECNT") = 0
        Else
            MD0001INProw("LINECNT") = CType(WF_Sel_LINECNT.Text, Integer)   'DBの固定フィールド
        End If
        MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA            'DBの固定フィールド
        MD0001INProw("TIMSTP") = 0                                          'DBの固定フィールド
        MD0001INProw("SELECT") = "0"                                        'DBの固定フィールド
        MD0001INProw("HIDDEN") = "0"                                        'DBの固定フィールド

        MD0001INProw("CAMPCODE") = WF_CAMPCODE.Text
        MD0001INProw("OILTYPE") = WF_OILTYPE.Text
        MD0001INProw("PRODUCT1") = WF_PRODUCT1.Text
        MD0001INProw("PRODUCT2") = WF_PRODUCT2.Text
        MD0001INProw("STYMD") = WF_STYMD.Text
        MD0001INProw("ENDYMD") = WF_ENDYMD.Text
        MD0001INProw("DELFLG") = WF_DELFLG.Text
        ' 品名コード（会社＋油種＋品名１＋品名２）
        MD0001INProw("PRODUCTCODE") = String.Format("{0}{1}{2}{3}", WF_CAMPCODE.Text, WF_OILTYPE.Text, WF_PRODUCT1.Text, WF_PRODUCT2.Text)
        ' 表示順を復元
        If (String.IsNullOrEmpty(WF_SEQ.Value) = True) Then
            MD0001INProw("SEQ") = 1
        Else
            MD0001INProw("SEQ") = WF_SEQ.Value
        End If
        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MD0001INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MD0001INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MD0001INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○コード名称を設定する
        ' 油種
        WW_TEXT = ""
        CODENAME_get("CAMPCODE", MD0001INProw("CAMPCODE"), WW_TEXT, WW_DUMMY)
        MD0001INProw("CAMPNAMES") = WW_TEXT
        ' 油種
        WW_TEXT = ""
        CODENAME_get("OILTYPE", MD0001INProw("OILTYPE"), WW_TEXT, WW_DUMMY)
        MD0001INProw("OILTYPENAMES") = WW_TEXT

        ' 品名１
        WW_TEXT = ""
        CODENAME_get("PRODUCT1", MD0001INProw("PRODUCT1"), WW_TEXT, WW_DUMMY, MD0001INProw("OILTYPE"))
        MD0001INProw("PRODUCT1NAMES") = WW_TEXT

        ' 請求単位
        WW_TEXT = ""
        CODENAME_get("STANI", MD0001INProw("STANI"), WW_TEXT, WW_DUMMY)
        MD0001INProw("STANINAMES") = WW_TEXT

        ' チェック用テーブルに登録する
        MD0001INPtbl.Rows.Add(MD0001INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○detailboxクリア
        Detailbox_Clear()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_SELOILTYPE.Focus()

    End Sub
    ''' <summary>
    ''' 詳細画面-クリア処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Detailbox_Clear()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MD0001Row As DataRow In MD0001tbl.Rows
            Select Case MD0001Row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MD0001Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        '画面(Grid)のHIDDEN列により、表示/非表示を行う。

        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        WF_OILTYPE.Text = ""
        WF_OILTYPE_TEXT.Text = ""
        WF_PRODUCT1.Text = ""
        WF_PRODUCT1_TEXT.Text = ""
        WF_PRODUCT2.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""
        WF_SEQ.Value = ""
        WF_PRODUCTCODE_TEXT.Text = ""
        '○Detail初期設定
        Repeater_INIT()

        WF_SELOILTYPE.Focus()

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

        O_ATTR = ""
        Select Case I_FIELD
            Case "STANI"
                ' 請求単位
                O_ATTR = "REF_Field_DBclick('STANI', 'WF_Rep_FIELD' , '999');"

        End Select

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
                    Dim prmData As Hashtable = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_GOODS
                            prmData = work.CreateGoods1Param(work.WF_SEL_CAMPCODE.Text, WF_OILTYPE.Text)

                        Case 999
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI")

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
    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = "0"
        Dim WW_SelectTEXT_LONG As String = "0"
        Dim WW_SelectValue As String = ""

        Dim WW_STAFFNAMES As String = String.Empty
        Dim WW_STAFFNAMEL As String = String.Empty

        '選択内容を取得

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '選択内容を画面項目へセット
        '項目セット　＆　フォーカス
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value

                Case "WF_SELOILTYPE"
                    WF_SELOILTYPE_TEXT.Text = WW_SelectTEXT
                    WF_SELOILTYPE.Text = WW_SelectValue
                    WF_SELOILTYPE.Focus()

                Case "WF_OILTYPE"
                    WF_OILTYPE_TEXT.Text = WW_SelectTEXT
                    WF_OILTYPE.Text = WW_SelectValue
                    WF_OILTYPE.Focus()

                Case "WF_PRODUCT1"
                    WF_PRODUCT1_TEXT.Text = WW_SelectTEXT
                    WF_PRODUCT1.Text = WW_SelectValue
                    WF_PRODUCT1.Focus()

                Case "WF_DELFLG"
                    WF_DELFLG_TEXT.Text = WW_SelectTEXT
                    WF_DELFLG.Text = WW_SelectValue
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
                    CType(reitem.FindControl("WF_Rep1_VALUE_#"), TextBox).Text = WW_SelectValue
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

        '○フォーカスセット
        Select Case WF_FIELD.Value

            Case "WF_SELOILTYPE"
                WF_SELOILTYPE.Focus()

            Case "WF_OILTYPE"
                WF_OILTYPE.Focus()

            Case "WF_PRODUCT1"
                WF_PRODUCT1.Focus()

            Case "WF_DELFLG"
                WF_DELFLG.Focus()

            Case "WF_STYMD"
                WF_STYMD.Focus()
            Case "WF_ENDYMD"
                WF_ENDYMD.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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

    ''' <summary>
    ''' ファイルアップロード入力処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○初期処理
        '○エラーレポート準備
        rightview.SetErrorReport("")

        Master.CreateEmptyTable(MD0001INPtbl)

        '○UPLOAD_XLSデータ取得        
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMD0001WRKINC.MAPID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()

        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ERR, "CS0023XLSTBL")
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

        '○必須列の判定
        If WW_COLUMNS.IndexOf("STYMD") < 0 OrElse
           WW_COLUMNS.IndexOf("ENDYMD") < 0 OrElse
           WW_COLUMNS.IndexOf("CAMPCODE") < 0 OrElse
           WW_COLUMNS.IndexOf("OILTYPE") < 0 OrElse
           WW_COLUMNS.IndexOf("PRODUCT1") < 0 OrElse
           WW_COLUMNS.IndexOf("PRODUCT2") < 0 Then
            ' インポート出来ません(項目： ?01 が存在しません)。
            Master.Output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport TITLE not find")
            Exit Sub
        End If

        '○Excelデータ毎にチェック＆更新
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            '○XLSTBL明細⇒MD0001INProw
            Dim MD0001INProw = MD0001INPtbl.NewRow

            '初期クリア
            For Each MD0001INPcol As DataColumn In MD0001INPtbl.Columns

                If IsDBNull(MD0001INProw.Item(MD0001INPcol)) OrElse IsNothing(MD0001INProw.Item(MD0001INPcol)) Then
                    Select Case MD0001INPcol.ColumnName
                        Case "LINECNT"
                            MD0001INProw.Item(MD0001INPcol) = 0
                        Case "TIMSTP"
                            MD0001INProw.Item(MD0001INPcol) = 0
                        Case "SELECT"
                            MD0001INProw.Item(MD0001INPcol) = 1
                        Case "HIDDEN"
                            MD0001INProw.Item(MD0001INPcol) = 0
                        Case "SEQ"
                            MD0001INProw.Item(MD0001INPcol) = 0
                        Case Else
                            If MD0001INPcol.DataType.Name = "String" Then
                                MD0001INProw.Item(MD0001INPcol) = ""
                            ElseIf MD0001INPcol.DataType.Name = "DateTime" Then
                                MD0001INProw.Item(MD0001INPcol) = C_DEFAULT_YMD
                            Else
                                MD0001INProw.Item(MD0001INPcol) = 0
                            End If
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            Dim WW_STYMD As String = ""

            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("OILTYPE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("PRODUCT1") >= 0 AndAlso
               WW_COLUMNS.IndexOf("PRODUCT2") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each MD0001row As DataRow In MD0001tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MD0001row("CAMPCODE") AndAlso
                       XLSTBLrow("OILTYPE") = MD0001row("OILTYPE") AndAlso
                       XLSTBLrow("PRODUCT1") = MD0001row("PRODUCT1") AndAlso
                       XLSTBLrow("PRODUCT2") = MD0001row("PRODUCT2") AndAlso
                       XLSTBLrow("STYMD") = MD0001row("STYMD") Then
                        MD0001INProw.ItemArray = MD0001row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MD0001INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If
            '油種
            If WW_COLUMNS.IndexOf("OILTYPE") >= 0 Then
                MD0001INProw("OILTYPE") = XLSTBLrow("OILTYPE")
            End If

            '油種名
            If WW_COLUMNS.IndexOf("OILTYPENAMES") >= 0 Then
                MD0001INProw("OILTYPENAMES") = XLSTBLrow("OILTYPENAMES")
            End If

            '品名１
            If WW_COLUMNS.IndexOf("PRODUCT1") >= 0 Then
                MD0001INProw("PRODUCT1") = XLSTBLrow("PRODUCT1")
            End If

            If WW_COLUMNS.IndexOf("PRODUCT1NAMES") >= 0 Then
                MD0001INProw("PRODUCT1NAMES") = XLSTBLrow("PRODUCT1NAMES")
            End If

            '品名２
            If WW_COLUMNS.IndexOf("PRODUCT2") >= 0 Then
                MD0001INProw("PRODUCT2") = XLSTBLrow("PRODUCT2")
            End If

            'SEQ
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MD0001INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '単位
            If WW_COLUMNS.IndexOf("STANI") >= 0 Then
                MD0001INProw("STANI") = XLSTBLrow("STANI")
            End If

            '有効開始日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    MD0001INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '有効終了日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    MD0001INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            '品名
            If WW_COLUMNS.IndexOf("PRODUCTNAMES") >= 0 Then
                MD0001INProw("PRODUCTNAMES") = XLSTBLrow("PRODUCTNAMES")
            End If

            If WW_COLUMNS.IndexOf("PRODUCTNAMEL") >= 0 Then
                MD0001INProw("PRODUCTNAMEL") = XLSTBLrow("PRODUCTNAMEL")
            End If

            '品名コード
            MD0001INProw("PRODUCTCODE") = String.Format("{0}{1}{2}{3}", MD0001INProw("CAMPCODE"), MD0001INProw("OILTYPE"), MD0001INProw("PRODUCT1"), MD0001INProw("PRODUCT2"))

            '単位名
            If WW_COLUMNS.IndexOf("STANINAMES") >= 0 Then
                MD0001INProw("STANINAMES") = XLSTBLrow("STANINAMES")
            End If

            '削除
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MD0001INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            MD0001INPtbl.Rows.Add(MD0001INProw)
        Next

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○画面表示テーブル更新
        If isNormal(WW_ERRCODE) Then
            MD0001tbl_UPD()
        End If

        '○画面表示データ保存
        Master.SaveTable(MD0001tbl)

        'エラー編集
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'detailboxクリア
        Detailbox_Clear()

        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 条件抽出画面情報退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()

        '○選択画面の入力初期値設定
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MD0001S Then

            Master.MAPID = GRMD0001WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        End If

    End Sub
    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベース（MD001_PRODUCT）を検索し画面表示する一覧を作成する</remarks>
    Private Sub MAPDATAget()

        '○画面表示用データ取得

        Try
            'MC0010テンポラリDB項目作成
            If MD0001tbl Is Nothing Then
                MD0001tbl = New DataTable
            End If

            If MD0001tbl.Columns.Count <> 0 Then
                MD0001tbl.Columns.Clear()
            End If

            '○DB項目クリア
            MD0001tbl.Clear()

            '○テーブル検索結果をテーブル退避
            'MD0001テンポラリDB項目作成

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                '　検索説明
                '     条件指定に従い該当データを品名マスタから取得する
                '　注意事項　日付について
                '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
                '　　但し、表追加時の②および③は、TBL入力有効期限。

                Dim SQLStr As String =
                      " SELECT  0                                      as LINECNT       , " _
                    & "         ''                                     as OPERATION     , " _
                    & "         TIMSTP = cast(isnull(MD1.UPDTIMSTP,0) as bigint)        , " _
                    & "         1                                      as 'SELECT'      , " _
                    & "         0                                      as HIDDEN        , " _
                    & "         rtrim(MD1.CAMPCODE)                    as CAMPCODE      , " _
                    & "         rtrim(M01.NAMES)                       as CAMPNAMES     , " _
                    & "         rtrim(MD1.OILTYPE)                     as OILTYPE       , " _
                    & "         rtrim(MC1OIL.VALUE1)                   as OILTYPENAMES  , " _
                    & "         rtrim(MD1.PRODUCT1)                    as PRODUCT1      , " _
                    & "         rtrim(MC1PROD.VALUE1)                  as PRODUCT1NAMES , " _
                    & "         rtrim(MD1.PRODUCT2)                    as PRODUCT2      , " _
                    & "         MD1.SEQ                                as SEQ           , " _
                    & "         MD1.STANI                              as STANI         , " _
                    & "         rtrim(MC1TANI.VALUE1)                  as STANINAMES    , " _
                    & "         format(MD1.STYMD, 'yyyy/MM/dd')        as STYMD         , " _
                    & "         format(MD1.ENDYMD, 'yyyy/MM/dd')       as ENDYMD        , " _
                    & "         rtrim(MD1.NAMES)                       as PRODUCTNAMES  , " _
                    & "         rtrim(MD1.NAMEL)                       as PRODUCTNAMEL  , " _
                    & "         rtrim(MD1.PRODUCTCODE)                 as PRODUCTCODE   , " _
                    & "         rtrim(MD1.DELFLG)                      as DELFLG        , " _
                    & "         ''                                     as INITYMD       , " _
                    & "         ''                                     as UPDYMD        , " _
                    & "         ''                                     as UPDUSER       , " _
                    & "         ''                                     as UPDTERMID     , " _
                    & "         ''                                     as RECEIVEYMD    , " _
                    & "         ''                                     as UPDTIMSTP       " _
                    & " FROM                                                              " _
                    & "           MD001_PRODUCT MD1                                       " _
                    & " LEFT JOIN M0001_CAMP    M01                                    ON " _
                    & "           M01.CAMPCODE    = MD1.CAMPCODE                          " _
                    & "      and  M01.STYMD      <= @P4                                   " _
                    & "      and  M01.ENDYMD     >= @P4                                   " _
                    & "      and  M01.DELFLG     <> '1'                                   " _
                    & " LEFT JOIN MC001_FIXVALUE MC1OIL                                ON " _
                    & "           MC1OIL.CAMPCODE = MD1.CAMPCODE                          " _
                    & "      and  MC1OIL.CLASS    = 'OILTYPE'                             " _
                    & "      and  MC1OIL.KEYCODE  =  MD1.OILTYPE                          " _
                    & "      and  MC1OIL.STYMD   <= @P4                                   " _
                    & "      and  MC1OIL.ENDYMD  >= @P4                                   " _
                    & "      and  MC1OIL.DELFLG  <> '1'                                   " _
                    & " LEFT JOIN MC001_FIXVALUE MC1PROD                               ON " _
                    & "           MC1PROD.CAMPCODE = MD1.CAMPCODE                         " _
                    & "      and  MC1PROD.CLASS   = 'PRODUCT1'                            " _
                    & "      and  MC1PROD.KEYCODE =  MD1.PRODUCT1                         " _
                    & "      and  MC1PROD.STYMD  <= @P4                                   " _
                    & "      and  MC1PROD.ENDYMD >= @P4                                   " _
                    & "      and  MC1PROD.DELFLG <> '1'                                   " _
                    & " LEFT JOIN MC001_FIXVALUE MC1TANI                               ON " _
                    & "           MC1TANI.CAMPCODE = MD1.CAMPCODE                         " _
                    & "      and  MC1TANI.CLASS   = 'STANI'                               " _
                    & "      and  MC1TANI.KEYCODE =  MD1.STANI                            " _
                    & "      and  MC1TANI.STYMD  <= @P4                                   " _
                    & "      and  MC1TANI.ENDYMD >= @P4                                   " _
                    & "      and  MC1TANI.DELFLG <> '1'                                   " _
                    & " WHERE                                                             " _
                    & "           MD1.CAMPCODE    = @P1                                   " _
                    & "      and  MD1.STYMD      <= @P2                                   " _
                    & "      and  MD1.ENDYMD     >= @P3                                   " _
                    & "      and  MD1.DELFLG     <> '1'                                   " _
                    & " ORDER BY                                                          " _
                    & "      CAMPCODE, OILTYPE, PRODUCT1, PRODUCT2, SEQ                   "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = Date.Now

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MD0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'MD0001tbl値設定
                        While SQLdr.Read

                            '2次抽出判定フラグ
                            Dim WW_SELECT_FLAG As Integer = 0    '0:対象外、1:対象

                            Dim MD0001row As DataRow = MD0001tbl.NewRow()

                            '○テンポラリTable追加
                            '固定項目
                            MD0001row("LINECNT") = SQLdr("LINECNT")
                            MD0001row("OPERATION") = SQLdr("OPERATION")
                            MD0001row("TIMSTP") = SQLdr("TIMSTP")
                            MD0001row("HIDDEN") = SQLdr("HIDDEN")

                            '画面毎の設定項目
                            MD0001row("CAMPCODE") = SQLdr("CAMPCODE")
                            MD0001row("CAMPNAMES") = SQLdr("CAMPNAMES")
                            MD0001row("OILTYPE") = SQLdr("OILTYPE")
                            MD0001row("OILTYPENAMES") = SQLdr("OILTYPENAMES")
                            MD0001row("PRODUCT1") = SQLdr("PRODUCT1")
                            MD0001row("PRODUCT1NAMES") = SQLdr("PRODUCT1NAMES")
                            MD0001row("PRODUCT2") = SQLdr("PRODUCT2")
                            MD0001row("SEQ") = SQLdr("SEQ")
                            MD0001row("STANI") = SQLdr("STANI")
                            MD0001row("STANINAMES") = SQLdr("STANINAMES")
                            MD0001row("STYMD") = CDate(SQLdr("STYMD")).ToString("yyyy/MM/dd")
                            MD0001row("ENDYMD") = CDate(SQLdr("ENDYMD")).ToString("yyyy/MM/dd")
                            MD0001row("PRODUCTNAMES") = SQLdr("PRODUCTNAMES")
                            MD0001row("PRODUCTNAMEL") = SQLdr("PRODUCTNAMEL")
                            MD0001row("PRODUCTCODE") = SQLdr("PRODUCTCODE")

                            MD0001row("DELFLG") = SQLdr("DELFLG")

                            WW_SELECT_FLAG = 1

                            If WW_SELECT_FLAG = 1 Then      'SELECT ... 1：対象,0：対象外
                                MD0001row("SELECT") = 1
                            Else
                                MD0001row("SELECT") = 0
                            End If

                            '抽出対象外の場合、レコード追加しない
                            If MD0001row("SELECT") = 1 Then
                                MD0001tbl.Rows.Add(MD0001row)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            'ログ出力
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MD001_PRODUCT SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MD001_PRODUCT Select"
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
        CS0026TBLSORT.TABLE = MD0001tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MD0001tbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_Check(ByRef O_RTNCODE As String)

        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・ユーザ更新権限なしです。"
            WW_CheckMES2 = ""
            O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            WW_LINEERR_SW = "ERR"
            Exit Sub
        End If

        For Each MD0001INProw As DataRow In MD0001INPtbl.Rows

            WW_LINEERR_SW = ""
            '○単項目チェック(会社コード)
            WW_TEXT = MD0001INProw("CAMPCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MD0001INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MD0001INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MD0001INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If
            '○単項目チェック(油種)
            WW_TEXT = MD0001INProw("OILTYPE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILTYPE", MD0001INProw("OILTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MD0001INProw("OILTYPE") = ""
                Else
                    CODENAME_get("OILTYPE", MD0001INProw("OILTYPE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名１)
            WW_TEXT = MD0001INProw("PRODUCT1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCT1", MD0001INProw("PRODUCT1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MD0001INProw("PRODUCT1") = ""
                Else
                    CODENAME_get("PRODUCT1", MD0001INProw("PRODUCT1"), WW_DUMMY, WW_RTN_SW, MD0001INProw("OILTYPE"))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(品名１エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(品名１コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名２)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCT2", MD0001INProw("PRODUCT2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名２コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(SEQ)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MD0001INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(表示順番コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(STANI)
            WW_TEXT = MD0001INProw("STANI")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STANI", MD0001INProw("STANI"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                If WW_TEXT = "" Then
                    MD0001INProw("STANI") = ""
                Else
                    CODENAME_get("STANI", MD0001INProw("STANI"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(請求単位エラー)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請求単位エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(有効開始日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", MD0001INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：開始エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(有効終了日付)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", MD0001INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付：終了エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名名称（短）)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCTNAMES", MD0001INProw("PRODUCTNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名名称（短）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(品名名称（長）)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PRODUCTNAMEL", MD0001INProw("PRODUCTNAMEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(品名名称（長）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○単項目チェック(DELFLG)
            If MD0001INProw("DELFLG") = "" OrElse MD0001INProw("DELFLG") = C_DELETE_FLG.ALIVE OrElse MD0001INProw("DELFLG") = C_DELETE_FLG.DELETE Then
                If MD0001INProw("DELFLG") = "" Then
                    MD0001INProw("DELFLG") = C_DELETE_FLG.ALIVE
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○操作設定
            If WW_LINEERR_SW = "" Then
                If MD0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub
    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○関連チェック
        For Each MD0001INProw As DataRow In MD0001tbl.Rows

            '読み飛ばし
            If (MD0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MD0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MD0001INProw("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MD0001INProw("STYMD") < C_DEFAULT_YMD Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each MD0001Row As DataRow In MD0001tbl.Rows

                '日付以外の項目が等しい
                If MD0001INProw("CAMPCODE") = MD0001Row("CAMPCODE") AndAlso
                   MD0001INProw("OILTYPE") = MD0001Row("OILTYPE") AndAlso
                   MD0001INProw("PRODUCT1") = MD0001Row("PRODUCT1") AndAlso
                   MD0001INProw("PRODUCT2") = MD0001Row("PRODUCT2") AndAlso
                   MD0001Row("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MD0001INProw("STYMD") = MD0001Row("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MD0001INProw("STYMD"), WW_DATE_ST)
                    Date.TryParse(MD0001INProw("ENDYMD"), WW_DATE_END)
                    Date.TryParse(MD0001Row("STYMD"), WW_DATE_ST2)
                    Date.TryParse(MD0001Row("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001Row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MD0001Row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next

            If WW_LINEERR_SW = "" Then
                MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MD0001tbl_UPD()

        '○操作表示クリア
        For Each MD0001row As DataRow In MD0001tbl.Rows
            Select Case MD0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MD0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each MD0001INProw As DataRow In MD0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If MD0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            '初期判定セット
            MD0001INProw("OPERATION") = "Insert"

            For Each MD0001row As DataRow In MD0001tbl.Rows

                If MD0001INProw("CAMPCODE") = MD0001row("CAMPCODE") AndAlso
                   MD0001INProw("OILTYPE") = MD0001row("OILTYPE") AndAlso
                   MD0001INProw("PRODUCT1") = MD0001row("PRODUCT1") AndAlso
                   MD0001INProw("PRODUCT2") = MD0001row("PRODUCT2") AndAlso
                   MD0001INProw("STYMD") = MD0001row("STYMD") Then
                Else
                    Continue For
                End If

                'レコード内容に変更があったか判定
                If MD0001INProw("ENDYMD") = MD0001row("ENDYMD") AndAlso
                   MD0001row("CAMPCODE") = MD0001INProw("CAMPCODE") AndAlso
                   MD0001row("CAMPNAMES") = MD0001INProw("CAMPNAMES") AndAlso
                   MD0001row("OILTYPE") = MD0001INProw("OILTYPE") AndAlso
                   MD0001row("OILTYPENAMES") = MD0001INProw("OILTYPENAMES") AndAlso
                   MD0001row("PRODUCT1") = MD0001INProw("PRODUCT1") AndAlso
                   MD0001row("PRODUCT1NAMES") = MD0001INProw("PRODUCT1NAMES") AndAlso
                   MD0001row("PRODUCT2") = MD0001INProw("PRODUCT2") AndAlso
                   MD0001row("SEQ") = MD0001INProw("SEQ") AndAlso
                   MD0001row("STANI") = MD0001INProw("STANI") AndAlso
                   MD0001row("STANINAMES") = MD0001INProw("STANINAMES") AndAlso
                   MD0001row("STYMD") = MD0001INProw("STYMD") AndAlso
                   MD0001row("ENDYMD") = MD0001INProw("ENDYMD") AndAlso
                   MD0001row("PRODUCTNAMES") = MD0001INProw("PRODUCTNAMES") AndAlso
                   MD0001row("PRODUCTNAMEL") = MD0001INProw("PRODUCTNAMEL") AndAlso
                   MD0001row("PRODUCTCODE") = MD0001INProw("PRODUCTCODE") AndAlso
                   MD0001row("DELFLG") = MD0001INProw("DELFLG") Then

                    MD0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Else
                    '○更新（Update）
                    TBL_Update_SUB(MD0001INProw, MD0001row)
                End If

                Exit For

            Next

            '○MD0001追加処理
            If MD0001INProw("OPERATION") = "Insert" Then
                '○更新（Insert）
                TBL_Insert_SUB(MD0001INProw)
            End If
        Next

    End Sub
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Update_SUB(ByVal INProw As DataRow, ByRef UPDRow As DataRow)

        INProw("LINECNT") = UPDRow("LINECNT")
        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        INProw("TIMSTP") = UPDRow("TIMSTP")
        INProw("SELECT") = 1
        INProw("HIDDEN") = 0

        '○MD0001変更処理
        UPDRow.ItemArray = INProw.ItemArray
        If UPDRow("DELFLG") = "" Then
            UPDRow("DELFLG") = C_DELETE_FLG.ALIVE
        Else
            UPDRow("DELFLG") = UPDRow("DELFLG")
        End If

    End Sub
    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_Insert_SUB(ByRef INProw As DataRow)

        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        '○MD0001追加処理
        Dim MD0001row As DataRow = MD0001tbl.NewRow
        MD0001row.ItemArray = INProw.ItemArray

        MD0001row("LINECNT") = MD0001tbl.Rows.Count + 1
        MD0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MD0001row("TIMSTP") = 0
        MD0001row("SELECT") = 1
        MD0001row("HIDDEN") = 0
        MD0001row("PRODUCTCODE") = String.Format("{0}{1}{2}{3}", MD0001row("CAMPCODE"), MD0001row("OILTYPE"), MD0001row("PRODUCT1"), MD0001row("PRODUCT2"))
        MD0001tbl.Rows.Add(MD0001row)

    End Sub

    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MD0001INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社コード　　=" & MD0001INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 油種　　　　　=" & MD0001INProw("OILTYPE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名１　　　　=" & MD0001INProw("PRODUCT1") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名２　　　　=" & MD0001INProw("PRODUCT2") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日　　=" & MD0001INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日　　=" & MD0001INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名名称（短）=" & MD0001INProw("PRODUCTNAMES") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 品名名称（長）=" & MD0001INProw("PRODUCTNAMEL") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグ　　=" & MD0001INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_OPT_PARAM As String = "")

        '○名称取得

        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE <> "" Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"      '会社
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                    Case "OILTYPE"      '油種
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

                    Case "PRODUCT1" '品名１
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_GOODS, I_VALUE, O_TEXT, O_RTN, work.CreateGoods1Param(work.WF_SEL_CAMPCODE.Text, I_OPT_PARAM))

                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                    Case "STANI" '単位名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "STANI"))

                    Case Else
                        O_TEXT = ""                                                             '該当項目なし

                End Select
            End With
        End If
    End Sub

End Class

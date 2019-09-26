Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 受注集計規則登録画面
''' </summary>
''' <remarks></remarks>
Public Class GRMC0010T3CNTL
    Inherits Page

    '検索結果格納ds
    Private MC0010tbl As DataTable                              'Grid格納用テーブル
    Private MC0010INPtbl As DataTable                           'Detail入力用テーブル
    Private MC010_T3CNTLtbl As DataTable                        '更新用テーブル

    ''共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'ユーザプロファイル（GridView）設定
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
                    If Not Master.RecoverTable(MC0010tbl) Then Exit Sub

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
                        Case "WF_Field_DBClick"
                            WF_Field_DBClick()
                        Case "WF_ButtonSel"
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"
                            WF_ButtonCan_Click()
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
            If Not IsNothing(MC0010tbl) Then
                MC0010tbl.Clear()
                MC0010tbl.Dispose()
                MC0010tbl = Nothing
            End If

            If Not IsNothing(MC0010INPtbl) Then
                MC0010INPtbl.Clear()
                MC0010INPtbl.Dispose()
                MC0010INPtbl = Nothing
            End If

            If Not IsNothing(MC010_T3CNTLtbl) Then
                MC010_T3CNTLtbl.Clear()
                MC010_T3CNTLtbl.Dispose()
                MC010_T3CNTLtbl = Nothing
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
        WF_SELTORI.Focus()
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
        Master.SaveTable(MC0010tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(MC0010tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = GRMC0010WRKINC.MAPID
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
        For Each MC0010row As DataRow In MC0010tbl.Rows
            If MC0010row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                MC0010row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(MC0010tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013PROFview.MAPID = GRMC0010WRKINC.MAPID
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
        WF_SELTORI.Focus()

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each MC0010row As DataRow In MC0010tbl.Rows
            '一度全部非表示化する
            MC0010row("HIDDEN") = 1

            '取引先、受注組織　絞込判定
            If WF_SELTORI.Text <> "" AndAlso WF_SELOORG.Text <> "" Then
                Dim WW_STRING1 As String = MC0010row("TORICODE")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = MC0010row("ORDERORG")    '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_SELTORI.Text) AndAlso WW_STRING2.Contains(WF_SELOORG.Text) Then
                    MC0010row("HIDDEN") = 0
                End If
            ElseIf WF_SELTORI.Text <> "" Then
                Dim WW_STRING As String = MC0010row("TORICODE")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_SELTORI.Text) Then
                    MC0010row("HIDDEN") = 0
                End If
            ElseIf WF_SELOORG.Text <> "" Then
                Dim WW_STRING As String = MC0010row("ORDERORG")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_SELOORG.Text) Then
                    MC0010row("HIDDEN") = 0
                End If
            Else
                '両方未設定の場合、押し並べて表示
                MC0010row("HIDDEN") = 0
            End If
        Next

        '○GridView再表示
        WF_GridPosition.Text = "1"
        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_SELTORI.Focus()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then

            '○メッセージ表示
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ABORT)

            '○画面表示データ保存
            Master.SaveTable(MC0010tbl)
            Exit Sub
        End If
        Try
            'ジャーナル出力用テーブル準備
            Master.CreateEmptyTable(MC010_T3CNTLtbl)
            'メッセージ初期化
            rightview.SetErrorReport("")

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                                                " _
                    & " set @hensuu = 0 ;                                                          " _
                    & " DECLARE hensuu CURSOR FOR                                                  " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                               " _
                    & "     FROM MC010_T3CNTL                                                      " _
                    & "     WHERE    CAMPCODE   = @P01	  and TORICODE   = @P02 and		            " _
                    & "              OILTYPE    = @P03	  and ORDERORG   = @P04 and		            " _
                    & "              STYMD      = @P05	;                                           " _
                    & "                                                                            " _
                    & " OPEN hensuu ;                                                              " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ;                                      " _
                    & " IF ( @@FETCH_STATUS = 0 )                                                  " _
                    & "    UPDATE MC010_T3CNTL                                                     " _
                    & "       SET    ENDYMD     = @P06 ,		                                    " _
                    & "              CNTL01     = @P07		, CNTL02     = @P08 ,		            " _
                    & "              CNTL03     = @P09		, CNTL04     = @P10 ,		            " _
                    & "              CNTL05     = @P11		, CNTL06     = @P12 ,		            " _
                    & "              CNTL07     = @P13		, CNTL08     = @P14 ,		            " _
                    & "              CNTL09     = @P15		, CNTL10     = @P16 ,		            " _
                    & "              CNTL11     = @P17		, CNTL12     = @P18 ,		            " _
                    & "              CNTL13     = @P19		, CNTL14     = @P20 ,		            " _
                    & "              CNTL15     = @P21		, CNTLVALUE  = @P22 ,		            " _
                    & "              URIKBN     = @P23		, DELFLG     = @P24 ,		            " _
                    & "              UPDYMD     = @P26 ,		                                    " _
                    & "              UPDUSER    = @P27		, UPDTERMID  = @P28 ,		            " _
                    & "              RECEIVEYMD = @P29                                             " _
                    & "     WHERE    CAMPCODE   = @P01	  and TORICODE   = @P02 and		            " _
                    & "              OILTYPE    = @P03	  and ORDERORG   = @P04 and		            " _
                    & "              STYMD      = @P05	;                                           " _
                    & " IF ( @@FETCH_STATUS <> 0 )                                                 " _
                    & "    INSERT INTO MC010_T3CNTL                                                " _
                    & "             (CAMPCODE , TORICODE , OILTYPE , ORDERORG , STYMD , ENDYMD ,	" _
                    & "              CNTL01 , CNTL02 , CNTL03 , CNTL04 , CNTL05 ,			        " _
                    & "              CNTL06 , CNTL07 , CNTL08 , CNTL09 , CNTL10 ,			        " _
                    & "              CNTL11 , CNTL12 , CNTL13 , CNTL14 , CNTL15 ,			        " _
                    & "              CNTLVALUE , URIKBN , DELFLG ,					                " _
                    & "              INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD)		    " _
                    & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,		    " _
                    & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,		    " _
                    & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29);			    " _
                    & " CLOSE hensuu ;                                                             " _
                    & " DEALLOCATE hensuu ;                                                        "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.DateTime)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.DateTime)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 1)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 1)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 1)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 1)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 1)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 1)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 1)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 1)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 1)
                    Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 1)
                    Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 1)
                    Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 1)
                    Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)
                    Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 1)
                    Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)
                    Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.DateTime)
                    Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 20)
                    Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 30)
                    Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.DateTime)

                    For Each MC0010Row As DataRow In MC0010tbl.Rows
                        If MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                            MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then

                            '○ＤＢ更新
                            Dim WW_DATENOW As DateTime = Date.Now

                            PARA01.Value = MC0010Row("CAMPCODE")
                            PARA02.Value = MC0010Row("TORICODE")
                            PARA03.Value = MC0010Row("OILTYPE")
                            PARA04.Value = MC0010Row("ORDERORG")
                            PARA05.Value = RTrim(MC0010Row("STYMD"))
                            PARA06.Value = RTrim(MC0010Row("ENDYMD"))
                            PARA07.Value = MC0010Row("CNTL01")
                            PARA08.Value = MC0010Row("CNTL02")
                            PARA09.Value = MC0010Row("CNTL03")
                            PARA10.Value = MC0010Row("CNTL04")
                            PARA11.Value = MC0010Row("CNTL05")
                            PARA12.Value = MC0010Row("CNTL06")
                            PARA13.Value = MC0010Row("CNTL07")
                            PARA14.Value = MC0010Row("CNTL08")
                            PARA15.Value = MC0010Row("CNTL09")
                            PARA16.Value = MC0010Row("CNTL10")
                            PARA17.Value = MC0010Row("CNTL11")
                            PARA18.Value = MC0010Row("CNTL12")
                            PARA19.Value = MC0010Row("CNTL13")
                            PARA20.Value = MC0010Row("CNTL14")
                            PARA21.Value = MC0010Row("CNTL15")
                            PARA22.Value = MC0010Row("CNTLVALUE")
                            PARA23.Value = MC0010Row("URIKBN")
                            PARA24.Value = MC0010Row("DELFLG")
                            PARA25.Value = WW_DATENOW
                            PARA26.Value = WW_DATENOW
                            PARA27.Value = Master.USERID
                            PARA28.Value = Master.USERTERMID
                            PARA29.Value = C_DEFAULT_YMD
                            SQLcmd.ExecuteNonQuery()

                            '結果 --> テーブル(MC0010tbl)反映
                            MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            '○更新ジャーナル追加
                            Dim MC010_T3CNTLrow As DataRow = MC010_T3CNTLtbl.NewRow

                            MC010_T3CNTLrow("CAMPCODE") = MC0010Row("CAMPCODE")
                            MC010_T3CNTLrow("TORICODE") = MC0010Row("TORICODE")
                            MC010_T3CNTLrow("OILTYPE") = MC0010Row("OILTYPE")
                            MC010_T3CNTLrow("ORDERORG") = MC0010Row("ORDERORG")
                            MC010_T3CNTLrow("STYMD") = RTrim(MC0010Row("STYMD"))
                            MC010_T3CNTLrow("ENDYMD") = RTrim(MC0010Row("ENDYMD"))
                            MC010_T3CNTLrow("CNTL01") = MC0010Row("CNTL01")
                            MC010_T3CNTLrow("CNTL02") = MC0010Row("CNTL02")
                            MC010_T3CNTLrow("CNTL03") = MC0010Row("CNTL03")
                            MC010_T3CNTLrow("CNTL04") = MC0010Row("CNTL04")
                            MC010_T3CNTLrow("CNTL05") = MC0010Row("CNTL05")
                            MC010_T3CNTLrow("CNTL06") = MC0010Row("CNTL06")
                            MC010_T3CNTLrow("CNTL07") = MC0010Row("CNTL07")
                            MC010_T3CNTLrow("CNTL08") = MC0010Row("CNTL08")
                            MC010_T3CNTLrow("CNTL09") = MC0010Row("CNTL09")
                            MC010_T3CNTLrow("CNTL10") = MC0010Row("CNTL10")
                            MC010_T3CNTLrow("CNTL11") = MC0010Row("CNTL11")
                            MC010_T3CNTLrow("CNTL12") = MC0010Row("CNTL12")
                            MC010_T3CNTLrow("CNTL13") = MC0010Row("CNTL13")
                            MC010_T3CNTLrow("CNTL14") = MC0010Row("CNTL14")
                            MC010_T3CNTLrow("CNTL15") = MC0010Row("CNTL15")
                            MC010_T3CNTLrow("CNTLVALUE") = MC0010Row("CNTLVALUE")
                            MC010_T3CNTLrow("URIKBN") = MC0010Row("URIKBN")
                            MC010_T3CNTLrow("DELFLG") = MC0010Row("DELFLG")

                            MC010_T3CNTLrow("INITYMD") = WW_DATENOW
                            MC010_T3CNTLrow("UPDYMD") = WW_DATENOW
                            MC010_T3CNTLrow("UPDUSER") = Master.USERID
                            MC010_T3CNTLrow("UPDTERMID") = Master.USERTERMID
                            MC010_T3CNTLrow("RECEIVEYMD") = WW_DATENOW
                            CS0020JOURNAL.TABLENM = "MC010_T3CNTL"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MC010_T3CNTLrow
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
                        End If
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC010_T3CNTL UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"
            CS0011LOGWRITE.INFPOSI = "DB:MC010_T3CNTL UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_SELTORI.Focus()

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
        CS0030REPORT.MAPID = GRMC0010WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = MC0010tbl                        'データ参照DataTable
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
        CS0030REPORT.MAPID = GRMC0010WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MC0010tbl                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
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
        WW_TBLview = New DataView(MC0010tbl)
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

        '○Grid内容(MC0010tbl)よりDetail編集

        WF_Sel_LINECNT.Text = MC0010tbl.Rows(WW_LINECNT)("LINECNT")

        '会社
        WF_CAMPCODE.Text = MC0010tbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WW_TEXT, WW_DUMMY)
        WF_CAMPCODE_TEXT.Text = WW_TEXT

        '受注部署
        WF_OORG.Text = MC0010tbl.Rows(WW_LINECNT)("ORDERORG")
        CODENAME_get("ORDERORG", WF_OORG.Text, WW_TEXT, WW_DUMMY, work.CreateORGParam(WF_CAMPCODE.Text, C_PERMISSION.UPDATE))
        WF_OORG_TEXT.Text = WW_TEXT

        '取引先
        WF_TORICODE.Text = MC0010tbl.Rows(WW_LINECNT)("TORICODE")
        CODENAME_get("TORICODE", WF_TORICODE.Text, WW_TEXT, WW_DUMMY, work.CreateTORIParam(WF_CAMPCODE.Text, WF_OORG.Text))
        WF_TORICODE_TEXT.Text = WW_TEXT

        '油種
        WF_OILTYPE.Text = MC0010tbl.Rows(WW_LINECNT)("OILTYPE")
        CODENAME_get("OILTYPE", WF_OILTYPE.Text, WW_TEXT, WW_DUMMY)
        WF_OILTYPE_TEXT.Text = WW_TEXT

        '有効年月日
        WF_STYMD.Text = MC0010tbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = MC0010tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除フラグ
        WF_DELFLG.Text = MC0010tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, MC0010tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, MC0010tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, MC0010tbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each MC0010Row As DataRow In MC0010tbl.Rows
            Select Case MC0010Row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case MC0010tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MC0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MC0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MC0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MC0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MC0010tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

        WF_GridDBclick.Text = ""

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○フィールドダブルクリック処理
        '○LeftBox処理
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
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            If WF_FIELD.Value = "WF_OORG" Then
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.UPDATE)
                            Else
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                            End If

                        Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                            If WF_FIELD.Value = "WF_TORICODE" Then
                                prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text, WF_OORG.Text)
                            Else
                                prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)
                            End If

                        Case 998
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MC0010_CNTLKBN")
                        Case 999
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MC0010_CNTLVALUE")
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

        '○DetailBoxをMC0010INPtblへ退避
        DetailBoxToMC0010INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○GridView更新
        If isNormal(WW_ERRCODE) Then
            MC0010tbl_UPD()
        End If

        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

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
        WF_SELTORI.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMC0010INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Master.CreateEmptyTable(MC0010INPtbl)
        Dim MC0010INProw As DataRow = MC0010INPtbl.NewRow

        '○DetailよりMC0010INPtbl編集
        For Each MC0010INPcol As DataColumn In MC0010INPtbl.Columns
            If IsDBNull(MC0010INProw.Item(MC0010INPcol)) OrElse IsNothing(MC0010INProw.Item(MC0010INPcol)) Then
                Select Case MC0010INPcol.ColumnName
                    Case "LINECNT"
                        MC0010INProw.Item(MC0010INPcol) = 0
                    Case "TIMSTP"
                        MC0010INProw.Item(MC0010INPcol) = 0
                    Case "SELECT"
                        MC0010INProw.Item(MC0010INPcol) = 1
                    Case "HIDDEN"
                        MC0010INProw.Item(MC0010INPcol) = 0
                    Case "WORK_NO"
                        MC0010INProw.Item(MC0010INPcol) = 0
                    Case Else
                        MC0010INProw.Item(MC0010INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            MC0010INProw("LINECNT") = 0
        Else
            Integer.TryParse(WF_Sel_LINECNT.Text, MC0010INProw("LINECNT"))
        End If

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_OORG.Text)              '受注部署
        Master.EraseCharToIgnore(WF_TORICODE.Text)          '取引先コード
        Master.EraseCharToIgnore(WF_OILTYPE.Text)           '油種
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        MC0010INProw("CAMPCODE") = WF_CAMPCODE.Text
        MC0010INProw("ORDERORG") = WF_OORG.Text
        MC0010INProw("TORICODE") = WF_TORICODE.Text
        MC0010INProw("OILTYPE") = WF_OILTYPE.Text
        MC0010INProw("STYMD") = WF_STYMD.Text
        MC0010INProw("ENDYMD") = WF_ENDYMD.Text
        MC0010INProw("DELFLG") = WF_DELFLG.Text

        MC0010INProw("CAMPNAMES") = ""
        MC0010INProw("TORINAMES") = ""
        MC0010INProw("OILTYPENAMES") = ""
        MC0010INProw("ORDERORGNAMES") = ""
        MC0010INProw("CNTL01NAMES") = ""
        MC0010INProw("CNTL02NAMES") = ""
        MC0010INProw("CNTL03NAMES") = ""
        MC0010INProw("CNTL04NAMES") = ""
        MC0010INProw("CNTL05NAMES") = ""
        MC0010INProw("CNTL06NAMES") = ""
        MC0010INProw("CNTL07NAMES") = ""
        MC0010INProw("CNTL08NAMES") = ""
        MC0010INProw("CNTL09NAMES") = ""
        MC0010INProw("CNTLVALUENAMES") = ""
        MC0010INProw("URIKBNNAMES") = ""

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_OORG.Text) AndAlso
            String.IsNullOrEmpty(WF_TORICODE.Text) AndAlso
            String.IsNullOrEmpty(WF_OILTYPE.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")

            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToMC0010INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0010INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0010INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                MC0010INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○名称付与
        MC0010INProw("CAMPNAMES") = ""
        CODENAME_get("CAMPCODE", MC0010INProw("CAMPCODE"), MC0010INProw("CAMPNAMES"), WW_DUMMY)         '会社名称
        MC0010INProw("TORINAMES") = ""
        CODENAME_get("TORICODE", MC0010INProw("TORICODE"), MC0010INProw("TORINAMES"), WW_DUMMY, work.CreateTORIParam(MC0010INProw("CAMPCODE"), MC0010INProw("ORDERORG")))         '取引先名
        MC0010INProw("OILTYPENAMES") = ""
        CODENAME_get("OILTYPE", MC0010INProw("OILTYPE"), MC0010INProw("OILTYPENAMES"), WW_DUMMY)        '油種
        MC0010INProw("ORDERORGNAMES") = ""
        CODENAME_get("ORDERORG", MC0010INProw("ORDERORG"), MC0010INProw("ORDERORGNAMES"), WW_DUMMY, work.CreateORGParam(MC0010INProw("CAMPCODE"), C_PERMISSION.UPDATE))     '受注組織
        MC0010INProw("CNTL01NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL01"), MC0010INProw("CNTL01NAMES"), WW_DUMMY)          '集計区分01
        MC0010INProw("CNTL02NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL02"), MC0010INProw("CNTL02NAMES"), WW_DUMMY)          '集計区分02
        MC0010INProw("CNTL03NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL03"), MC0010INProw("CNTL03NAMES"), WW_DUMMY)          '集計区分03
        MC0010INProw("CNTL04NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL04"), MC0010INProw("CNTL04NAMES"), WW_DUMMY)          '集計区分04
        MC0010INProw("CNTL05NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL05"), MC0010INProw("CNTL05NAMES"), WW_DUMMY)          '集計区分05
        MC0010INProw("CNTL06NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL06"), MC0010INProw("CNTL06NAMES"), WW_DUMMY)          '集計区分06
        MC0010INProw("CNTL07NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL07"), MC0010INProw("CNTL07NAMES"), WW_DUMMY)          '集計区分07
        MC0010INProw("CNTL08NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL08"), MC0010INProw("CNTL08NAMES"), WW_DUMMY)          '集計区分08
        MC0010INProw("CNTL09NAMES") = ""
        CODENAME_get("CNTLKBN", MC0010INProw("CNTL09"), MC0010INProw("CNTL09NAMES"), WW_DUMMY)          '集計区分09
        MC0010INProw("CNTLVALUENAMES") = ""
        CODENAME_get("CNTLVALUE", MC0010INProw("CNTLVALUE"), MC0010INProw("CNTLVALUENAMES"), WW_DUMMY)  '台数数量集計
        MC0010INProw("URIKBNNAMES") = ""
        CODENAME_get("URIKBN", MC0010INProw("URIKBN"), MC0010INProw("URIKBNNAMES"), WW_DUMMY)           '売上計上区分

        MC0010INPtbl.Rows.Add(MC0010INProw)

    End Sub
    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        For Each MC0010Row As DataRow In MC0010tbl.Rows
            Select Case MC0010Row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0010Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

        '○detailboxヘッダークリア
        WF_Sel_LINECNT.Text = ""

        '会社
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '受注部署
        WF_OORG.Text = ""
        WF_OORG_TEXT.Text = ""

        '取引先
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""

        '油種
        WF_OILTYPE.Text = ""
        WF_OILTYPE_TEXT.Text = ""

        '有効年月日
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""

        '削除フラグ
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        '○Detail初期設定
        Repeater_INIT()

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

        '○HEADERのイベント設定
        WF_OORG.Attributes.Remove("ondblclick")
        WF_OORG.Attributes.Add("ondblclick", "Field_DBclick( 'WF_OORG' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');")
        WF_DELFLG.Attributes.Remove("ondblclick")
        WF_DELFLG.Attributes.Add("ondblclick", "Field_DBclick( 'WF_DELFLG' , '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "');")
        WF_DELFLG.Attributes.Remove("ondblclick")
        WF_DELFLG.Attributes.Add("ondblclick", "Field_DBclick( 'WF_DELFLG' , '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "');")
        WF_TORICODE.Attributes.Remove("ondblclick")
        WF_TORICODE.Attributes.Add("ondblclick", "Field_DBclick( 'WF_TORICODE' , '" & LIST_BOX_CLASSIFICATION.LC_CUSTOMER & "');")
        WF_OORG.Attributes.Remove("ondblclick")
        WF_OORG.Attributes.Add("ondblclick", "Field_DBclick( 'WF_OORG' , '" & LIST_BOX_CLASSIFICATION.LC_ORG & "');")
        WF_OILTYPE.Attributes.Remove("ondblclick")
        WF_OILTYPE.Attributes.Add("ondblclick", "Field_DBclick( 'WF_OILTYPE' , '" & LIST_BOX_CLASSIFICATION.LC_OILTYPE & "');")

        Try
            'カラム情報をリピーター作成用に取得
            Master.CreateEmptyTable(dataTable)
            dataTable.Rows.Add(dataTable.NewRow())

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            'CS0052DetailView.TABID = ""
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
            Case "CNTL01"       '届日計
                O_ATTR = "REF_Field_DBclick('CNTL01', 'WF_Rep_FIELD' , '998');"
            Case "CNTL02"       '出庫日計
                O_ATTR = "REF_Field_DBclick('CNTL02', 'WF_Rep_FIELD' , '998');"
            Case "CNTL03"       '出荷場計
                O_ATTR = "REF_Field_DBclick('CNTL03', 'WF_Rep_FIELD' , '998');"
            Case "CNTL04"       '業車計
                O_ATTR = "REF_Field_DBclick('CNTL04', 'WF_Rep_FIELD' , '998');"
            Case "CNTL05"       '車腹計
                O_ATTR = "REF_Field_DBclick('CNTL05', 'WF_Rep_FIELD' , '998');"
            Case "CNTL06"       '乗務員計
                O_ATTR = "REF_Field_DBclick('CNTL06', 'WF_Rep_FIELD' , '998');"
            Case "CNTL07"       '届先計
                O_ATTR = "REF_Field_DBclick('CNTL07', 'WF_Rep_FIELD' , '998');"
            Case "CNTL08"       '品１計
                O_ATTR = "REF_Field_DBclick('CNTL08', 'WF_Rep_FIELD' , '998');"
            Case "CNTL09"       '品２計
                O_ATTR = "REF_Field_DBclick('CNTL09', 'WF_Rep_FIELD' , '998');"
            Case "CNTLVALUE"    '数／台"
                O_ATTR = "REF_Field_DBclick('CNTLVALUE', 'WF_Rep_FIELD' , '999');"
            Case "URIKBN"       '売上区分"
                O_ATTR = "REF_Field_DBclick('URIKBN', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_URIKBN & "');"
        End Select

    End Sub

    ' *** 詳細画面-タブ切り替え制御 

    ' *** 詳細画面-タブ切替処理


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

                Case "WF_SELTORI"
                    WF_SELTORI_TEXT.Text = WW_SelectTEXT
                    WF_SELTORI.Text = WW_SelectValue
                    WF_SELTORI.Focus()
                Case "WF_SELOORG"
                    WF_SELOORG_TEXT.Text = WW_SelectTEXT
                    WF_SELOORG.Text = WW_SelectValue
                    WF_SELOORG.Focus()

                Case "WF_CAMPCODE"
                    '会社
                    WF_CAMPCODE.Text = WW_SelectValue
                    WF_CAMPCODE_TEXT.Text = WW_SelectTEXT
                    WF_CAMPCODE.Focus()

                Case "WF_OORG"
                    '受注部署
                    WF_OORG.Text = WW_SelectValue
                    WF_OORG_TEXT.Text = WW_SelectTEXT
                    WF_OORG.Focus()

                Case "WF_TORICODE"
                    '取引先
                    WF_TORICODE.Text = WW_SelectValue
                    WF_TORICODE_TEXT.Text = WW_SelectTEXT
                    WF_TORICODE.Focus()

                Case "WF_OILTYPE"
                    '油種
                    WF_OILTYPE.Text = WW_SelectValue
                    WF_OILTYPE_TEXT.Text = WW_SelectTEXT
                    WF_OILTYPE.Focus()

                Case "WF_DELFLG"
                    '削除フラグ
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
        End If

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""
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
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRMC0010WRKINC.MAPID
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

        '○入力テーブル作成
        Master.CreateEmptyTable(MC0010INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MC0010INProw As DataRow = MC0010INPtbl.NewRow

            '○初期クリア
            For Each MC0010INPcol As DataColumn In MC0010INPtbl.Columns
                If IsDBNull(MC0010INProw.Item(MC0010INPcol)) OrElse IsNothing(MC0010INProw.Item(MC0010INPcol)) Then
                    Select Case MC0010INPcol.ColumnName
                        Case "LINECNT"
                            MC0010INProw.Item(MC0010INPcol) = 0
                        Case "TIMSTP"
                            MC0010INProw.Item(MC0010INPcol) = 0
                        Case "SELECT"
                            MC0010INProw.Item(MC0010INPcol) = 1
                        Case "HIDDEN"
                            MC0010INProw.Item(MC0010INPcol) = 0
                        Case "WORK_NO"
                            MC0010INProw.Item(MC0010INPcol) = 0
                        Case Else
                            MC0010INProw.Item(MC0010INPcol) = ""
                    End Select
                End If
            Next

            '○変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("OILTYPE") >= 0 AndAlso
               WW_COLUMNS.IndexOf("ORDERORG") >= 0 AndAlso
               WW_COLUMNS.IndexOf("STYMD") >= 0 Then

                For Each MC0010row As DataRow In MC0010tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MC0010row("CAMPCODE") AndAlso
                       XLSTBLrow("TORICODE") = MC0010row("TORICODE") AndAlso
                       XLSTBLrow("OILTYPE") = MC0010row("OILTYPE") AndAlso
                       XLSTBLrow("ORDERORG") = MC0010row("ORDERORG") AndAlso
                       XLSTBLrow("STYMD") = MC0010row("STYMD") Then
                        MC0010INProw.ItemArray = MC0010row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○XLSTBL明細⇒MC0010INProw
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MC0010INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                MC0010INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            If WW_COLUMNS.IndexOf("OILTYPE") >= 0 Then
                MC0010INProw("OILTYPE") = XLSTBLrow("OILTYPE")
            End If

            If WW_COLUMNS.IndexOf("ORDERORG") >= 0 Then
                MC0010INProw("ORDERORG") = XLSTBLrow("ORDERORG")
            End If

            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                If IsDate(XLSTBLrow("STYMD")) Then
                    WW_DATE = XLSTBLrow("STYMD")
                    MC0010INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                If IsDate(XLSTBLrow("ENDYMD")) Then
                    WW_DATE = XLSTBLrow("ENDYMD")
                    MC0010INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
            End If

            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MC0010INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            If WW_COLUMNS.IndexOf("CNTL01") >= 0 Then
                MC0010INProw("CNTL01") = XLSTBLrow("CNTL01")
            End If

            If WW_COLUMNS.IndexOf("CNTL02") >= 0 Then
                MC0010INProw("CNTL02") = XLSTBLrow("CNTL02")
            End If

            If WW_COLUMNS.IndexOf("CNTL03") >= 0 Then
                MC0010INProw("CNTL03") = XLSTBLrow("CNTL03")
            End If

            If WW_COLUMNS.IndexOf("CNTL04") >= 0 Then
                MC0010INProw("CNTL04") = XLSTBLrow("CNTL04")
            End If

            If WW_COLUMNS.IndexOf("CNTL05") >= 0 Then
                MC0010INProw("CNTL05") = XLSTBLrow("CNTL05")
            End If

            If WW_COLUMNS.IndexOf("CNTL06") >= 0 Then
                MC0010INProw("CNTL06") = XLSTBLrow("CNTL06")
            End If

            If WW_COLUMNS.IndexOf("CNTL07") >= 0 Then
                MC0010INProw("CNTL07") = XLSTBLrow("CNTL07")
            End If

            If WW_COLUMNS.IndexOf("CNTL08") >= 0 Then
                MC0010INProw("CNTL08") = XLSTBLrow("CNTL08")
            End If

            If WW_COLUMNS.IndexOf("CNTL09") >= 0 Then
                MC0010INProw("CNTL09") = XLSTBLrow("CNTL09")
            End If

            If WW_COLUMNS.IndexOf("CNTLVALUE") >= 0 Then
                MC0010INProw("CNTLVALUE") = XLSTBLrow("CNTLVALUE")
            End If

            If WW_COLUMNS.IndexOf("URIKBN") >= 0 Then
                MC0010INProw("URIKBN") = XLSTBLrow("URIKBN")
            End If

            '名称付与
            CODENAME_get("CAMPCODE", MC0010INProw("CAMPCODE"), MC0010INProw("CAMPNAMES"), WW_DUMMY)
            CODENAME_get("TORICODE", MC0010INProw("TORICODE"), MC0010INProw("TORINAMES"), WW_DUMMY, work.CreateTORIParam(MC0010INProw("CAMPCODE"), MC0010INProw("ORDERORG")))
            CODENAME_get("OILTYPE", MC0010INProw("OILTYPE"), MC0010INProw("OILTYPENAMES"), WW_DUMMY)
            CODENAME_get("ORDERORG", MC0010INProw("ORDERORG"), MC0010INProw("ORDERORGNAMES"), WW_DUMMY, work.CreateORGParam(MC0010INProw("CAMPCODE"), C_PERMISSION.UPDATE))
            CODENAME_get("CNTL01", MC0010INProw("CNTL01"), MC0010INProw("CNTL01NAMES"), WW_DUMMY)
            CODENAME_get("CNTL02", MC0010INProw("CNTL02"), MC0010INProw("CNTL02NAMES"), WW_DUMMY)
            CODENAME_get("CNTL03", MC0010INProw("CNTL03"), MC0010INProw("CNTL03NAMES"), WW_DUMMY)
            CODENAME_get("CNTL04", MC0010INProw("CNTL04"), MC0010INProw("CNTL04NAMES"), WW_DUMMY)
            CODENAME_get("CNTL05", MC0010INProw("CNTL05"), MC0010INProw("CNTL05NAMES"), WW_DUMMY)
            CODENAME_get("CNTL06", MC0010INProw("CNTL06"), MC0010INProw("CNTL06NAMES"), WW_DUMMY)
            CODENAME_get("CNTL07", MC0010INProw("CNTL07"), MC0010INProw("CNTL07NAMES"), WW_DUMMY)
            CODENAME_get("CNTL09", MC0010INProw("CNTL09"), MC0010INProw("CNTL09NAMES"), WW_DUMMY)
            CODENAME_get("CNTL08", MC0010INProw("CNTL08"), MC0010INProw("CNTL08NAMES"), WW_DUMMY)
            CODENAME_get("CNTLVALUE", MC0010INProw("CNTLVALUE"), MC0010INProw("CNTLVALUENAMES"), WW_DUMMY)
            CODENAME_get("URIKBN", MC0010INProw("URIKBN"), MC0010INProw("URIKBNNAMES"), WW_DUMMY)

            MC0010INPtbl.Rows.Add(MC0010INProw)
        Next

        '○項目チェック
        INPtbl_Check(WW_ERRCODE)

        '○画面表示データ更新
        MC0010tbl_UPD()

        '○画面表示データ保存
        Master.SaveTable(MC0010tbl)

        'メッセージ表示
        If isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_SELTORI.Focus()

        '○Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    '''  条件抽出画面情報退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0010S Then
            Master.MAPID = GRMC0010WRKINC.MAPID
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
    ''' <remarks>データベース（MC010_T3CNTL）を検索し画面表示する一覧を作成する</remarks>
    Protected Sub MAPDATAget()

        '○画面表示用データ取得

        Try
            '○GridView内容をテーブル退避
            'MC0010テンポラリDB項目作成
            If IsNothing(MC0010tbl) Then
                MC0010tbl = New DataTable
            End If

            If MC0010tbl.Columns.Count <> 0 Then
                MC0010tbl.Columns.Clear()
            End If

            '○DB項目クリア
            MC0010tbl.Clear()

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      "  SELECT  0                                           as LINECNT ,       " _
                    & "         ''                                           as OPERATION ,     " _
                    & "         TIMSTP = cast(isnull(MC10.UPDTIMSTP,0)       as bigint),        " _
                    & "         1                                            as 'SELECT' ,      " _
                    & "         0                                            as HIDDEN ,        " _
                    & "         isnull(rtrim(MC10.CAMPCODE),'')              as CAMPCODE ,      " _
                    & "         isnull(rtrim(MC10.TORICODE),'')              as TORICODE ,      " _
                    & "         isnull(rtrim(MC10.OILTYPE),'')               as OILTYPE ,       " _
                    & "         isnull(rtrim(MC10.ORDERORG),'')              as ORDERORG ,      " _
                    & "         isnull(format(MC10.STYMD, 'yyyy/MM/dd'),'')  as STYMD ,         " _
                    & "         isnull(format(MC10.ENDYMD, 'yyyy/MM/dd'),'') as ENDYMD ,        " _
                    & "         isnull(rtrim(MC10.DELFLG),'0')               as DELFLG ,        " _
                    & "         isnull(rtrim(MC10.CNTL01),'')                as CNTL01 ,        " _
                    & "         isnull(rtrim(MC10.CNTL02),'')                as CNTL02 ,        " _
                    & "         isnull(rtrim(MC10.CNTL03),'')                as CNTL03 ,        " _
                    & "         isnull(rtrim(MC10.CNTL04),'')                as CNTL04 ,        " _
                    & "         isnull(rtrim(MC10.CNTL05),'')                as CNTL05 ,        " _
                    & "         isnull(rtrim(MC10.CNTL06),'')                as CNTL06 ,        " _
                    & "         isnull(rtrim(MC10.CNTL07),'')                as CNTL07 ,        " _
                    & "         isnull(rtrim(MC10.CNTL08),'')                as CNTL08 ,        " _
                    & "         isnull(rtrim(MC10.CNTL09),'')                as CNTL09 ,        " _
                    & "         isnull(rtrim(MC10.CNTL10),'')                as CNTL10 ,        " _
                    & "         isnull(rtrim(MC10.CNTL11),'')                as CNTL11 ,        " _
                    & "         isnull(rtrim(MC10.CNTL12),'')                as CNTL12 ,        " _
                    & "         isnull(rtrim(MC10.CNTL13),'')                as CNTL13 ,        " _
                    & "         isnull(rtrim(MC10.CNTL14),'')                as CNTL14 ,        " _
                    & "         isnull(rtrim(MC10.CNTL15),'')                as CNTL15 ,        " _
                    & "         isnull(rtrim(MC10.CNTLVALUE),'')             as CNTLVALUE ,     " _
                    & "         isnull(rtrim(MC10.URIKBN),'')                as URIKBN ,        " _
                    & "         ''                                           as CAMPNAMES ,     " _
                    & "         ''                                           as TORINAMES ,     " _
                    & "         ''                                           as OILTYPENAMES ,  " _
                    & "         ''                                           as ORDERORGNAMES , " _
                    & "         ''                                           as CNTLVALUENAMES ," _
                    & "         ''                                           as CNTL01NAMES ,   " _
                    & "         ''                                           as CNTL02NAMES ,   " _
                    & "         ''                                           as CNTL03NAMES ,   " _
                    & "         ''                                           as CNTL04NAMES ,   " _
                    & "         ''                                           as CNTL05NAMES ,   " _
                    & "         ''                                           as CNTL06NAMES ,   " _
                    & "         ''                                           as CNTL07NAMES ,   " _
                    & "         ''                                           as CNTL08NAMES ,   " _
                    & "         ''                                           as CNTL09NAMES ,   " _
                    & "         ''                                           as CNTL10NAMES ,   " _
                    & "         ''                                           as CNTL11NAMES ,   " _
                    & "         ''                                           as CNTL12NAMES ,   " _
                    & "         ''                                           as CNTL13NAMES ,   " _
                    & "         ''                                           as CNTL14NAMES ,   " _
                    & "         ''                                           as CNTL15NAMES ,   " _
                    & "         ''                                           as URIKBNNAMES ,   " _
                    & "         ''                                           as INITYMD     ,   " _
                    & "         ''                                           as UPDYMD      ,   " _
                    & "         ''                                           as UPDUSER     ,   " _
                    & "         ''                                           as UPDTERMID   ,   " _
                    & "         ''                                           as RECEIVEYMD  ,   " _
                    & "         ''                                           as UPDTIMSTP       " _
                    & "  FROM MC003_TORIORG MC03                                          " _
                    & "  INNER JOIN MC010_T3CNTL MC10                                     " _
                    & "     ON  MC10.CAMPCODE                           = MC03.CAMPCODE   " _
                    & "     and MC10.TORICODE                           = MC03.TORICODE   " _
                    & "     and MC10.ORDERORG                           = MC03.UORG       " _
                    & "     and MC10.STYMD                             <= @P1             " _
                    & "     and MC10.ENDYMD                            >= @P2             " _
                    & "     and MC10.DELFLG                            <> '1'             " _
                    & "  WHERE  MC03.CAMPCODE                           = @P3             " _
                    & "     and MC03.DELFLG                            <> '1'             "

                ' 条件指定で指定されたものでＳＱＬで可能なものを追加する
                '油種
                If Not String.IsNullOrEmpty(work.WF_SEL_OILTYPE.Text) Then
                    SQLStr &= String.Format(" and MC10.OILTYPE = '{0}' ", work.WF_SEL_OILTYPE.Text)
                End If

                '取引先
                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                    SQLStr &= String.Format(" and MC03.TORICODE = '{0}' ", work.WF_SEL_TORICODE.Text)
                End If

                '受注組織
                If Not String.IsNullOrEmpty(work.WF_SEL_ORDERORG.Text) Then
                    SQLStr &= String.Format(" and MC03.UORG = '{0}' ", work.WF_SEL_ORDERORG.Text)
                End If

                SQLStr &= " ORDER BY ORDERORG, TORICODE, OILTYPE "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.Date)                '有効日(To)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '有効日(From)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '会社

                    PARA1.Value = work.WF_SEL_ENDYMD.Text
                    PARA2.Value = work.WF_SEL_STYMD.Text
                    PARA3.Value = work.WF_SEL_CAMPCODE.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            MC0010tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○テーブル検索結果をテーブル格納
                        MC0010tbl.Load(SQLdr)
                    End Using

                    For Each MC0010row As DataRow In MC0010tbl.Rows
                        '○項目名称セット
                        CODENAME_get("CAMPCODE", MC0010row("CAMPCODE"), MC0010row("CAMPNAMES"), WW_DUMMY)           '会社名称
                        CODENAME_get("TORICODE", MC0010row("TORICODE"), MC0010row("TORINAMES"), WW_DUMMY, work.CreateTORIParam(MC0010row("CAMPCODE"), MC0010row("ORDERORG")))       '取引先名
                        CODENAME_get("OILTYPE", MC0010row("OILTYPE"), MC0010row("OILTYPENAMES"), WW_DUMMY)          '油種
                        CODENAME_get("ORDERORG", MC0010row("ORDERORG"), MC0010row("ORDERORGNAMES"), WW_DUMMY, work.CreateORGParam(MC0010row("CAMPCODE"), C_PERMISSION.UPDATE))      '受注組織
                        CODENAME_get("CNTLKBN", MC0010row("CNTL01"), MC0010row("CNTL01NAMES"), WW_DUMMY)            '集計区分01
                        CODENAME_get("CNTLKBN", MC0010row("CNTL02"), MC0010row("CNTL02NAMES"), WW_DUMMY)            '集計区分02
                        CODENAME_get("CNTLKBN", MC0010row("CNTL03"), MC0010row("CNTL03NAMES"), WW_DUMMY)            '集計区分03
                        CODENAME_get("CNTLKBN", MC0010row("CNTL04"), MC0010row("CNTL04NAMES"), WW_DUMMY)            '集計区分04
                        CODENAME_get("CNTLKBN", MC0010row("CNTL05"), MC0010row("CNTL05NAMES"), WW_DUMMY)            '集計区分05
                        CODENAME_get("CNTLKBN", MC0010row("CNTL06"), MC0010row("CNTL06NAMES"), WW_DUMMY)            '集計区分06
                        CODENAME_get("CNTLKBN", MC0010row("CNTL07"), MC0010row("CNTL07NAMES"), WW_DUMMY)            '集計区分07
                        CODENAME_get("CNTLKBN", MC0010row("CNTL08"), MC0010row("CNTL08NAMES"), WW_DUMMY)            '集計区分08
                        CODENAME_get("CNTLKBN", MC0010row("CNTL09"), MC0010row("CNTL09NAMES"), WW_DUMMY)            '集計区分09
                        CODENAME_get("CNTLVALUE", MC0010row("CNTLVALUE"), MC0010row("CNTLVALUENAMES"), WW_DUMMY)    '台数数量集計
                        CODENAME_get("URIKBN", MC0010row("URIKBN"), MC0010row("URIKBNNAMES"), WW_DUMMY)             '売上計上区分
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC010_T3CNTL SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                     'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC010_T3CNTL Select"
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
        CS0026TBLSORT.TABLE = MC0010tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            MC0010tbl = CS0026TBLSORT.TABLE
        End If

    End Sub
    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub INPtbl_Check(ByRef O_RTNCODE As String)

        '○初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL
        rightview.SetErrorReport("")

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_TEXT As String = ""

        '○チェック実行
        For Each MC0010INProw As DataRow In MC0010INPtbl.Rows

            WW_LINEERR_SW = ""

            '・キー項目(会社：CAMPCODE)
            WW_TEXT = MC0010INProw("CAMPCODE")
            Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", MC0010INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CAMPCODE") = ""
                Else
                    CODENAME_get("CAMPCODE", MC0010INProw("CAMPCODE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・キー項目(取引先：TORICODE)
            Master.CheckField(WF_CAMPCODE.Text, "TORICODE", MC0010INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(受注部署：ORDERORG)
            WW_TEXT = MC0010INProw("ORDERORG")
            Master.CheckField(WF_CAMPCODE.Text, "ORDERORG", MC0010INProw("ORDERORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("ORDERORG") = ""
                Else
                    CODENAME_get("ORDERORG", MC0010INProw("ORDERORG"), WW_TEXT, WW_RTN_SW, work.CreateORGParam(MC0010INProw("CAMPCODE"), C_PERMISSION.UPDATE))
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（受注部署）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（受注部署）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(油種：OILTYPE)
            WW_TEXT = MC0010INProw("OILTYPE")
            Master.CheckField(WF_CAMPCODE.Text, "OILTYPE", MC0010INProw("OILTYPE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("OILTYPE") = ""
                Else
                    CODENAME_get("OILTYPE", MC0010INProw("OILTYPE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（油種）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（油種）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・キー項目(有効年月日：STYMD)
            Master.CheckField(WF_CAMPCODE.Text, "STYMD", MC0010INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・キー項目(有効年月日：ENDYMD)
            Master.CheckField(WF_CAMPCODE.Text, "ENDYMD", MC0010INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・キー項目(削除フラグ：DELFLG)
            Master.CheckField(WF_CAMPCODE.Text, "DELFLG", MC0010INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○関連チェック(キー情報)
            '大小比較チェック
            If MC0010INProw("STYMD") > MC0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始日付 ＞ 終了日付)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If
            '範囲チェック
            If work.WF_SEL_STYMD.Text > MC0010INProw("STYMD") AndAlso
                work.WF_SEL_STYMD.Text > MC0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If
            If work.WF_SEL_ENDYMD.Text < MC0010INProw("STYMD") AndAlso
                work.WF_SEL_ENDYMD.Text < MC0010INProw("ENDYMD") Then
                WW_CheckMES1 = "・更新できないレコード(開始、終了日付が範囲外)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○権限チェック（更新権限）
            If MC0010INProw("ORDERORG") <> "" Then
                '受注部署
                CS0025AUTHORget.USERID = CS0050Session.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MC0010INProw("ORDERORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・エラーが存在します。（権限無）"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                End If
            End If

            '○単項目チェック(明細情報)

            '・明細項目(集計区分01：CNTL01)
            WW_TEXT = MC0010INProw("CNTL01")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL01", MC0010INProw("CNTL01"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL01") = ""
                Else
                    CODENAME_get("CNTL01", MC0010INProw("CNTL01"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分01）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分01）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分02：CNTL02)
            WW_TEXT = MC0010INProw("CNTL02")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL02", MC0010INProw("CNTL02"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL02") = ""
                Else
                    CODENAME_get("CNTL02", MC0010INProw("CNTL02"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分02）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分02）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分03：CNTL03)
            WW_TEXT = MC0010INProw("CNTL03")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL03", MC0010INProw("CNTL03"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL03") = ""
                Else
                    CODENAME_get("CNTL03", MC0010INProw("CNTL03"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分03）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分03）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分04：CNTL04)
            WW_TEXT = MC0010INProw("CNTL04")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL04", MC0010INProw("CNTL04"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL04") = ""
                Else
                    CODENAME_get("CNTL04", MC0010INProw("CNTL04"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分04）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分04）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分05：CNTL05)
            WW_TEXT = MC0010INProw("CNTL05")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL05", MC0010INProw("CNTL05"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL05") = ""
                Else
                    CODENAME_get("CNTL05", MC0010INProw("CNTL05"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分05）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分05）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分06：CNTL06)
            WW_TEXT = MC0010INProw("CNTL06")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL06", MC0010INProw("CNTL06"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL06") = ""
                Else
                    CODENAME_get("CNTL06", MC0010INProw("CNTL06"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分06）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分06）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分07：CNTL07)
            WW_TEXT = MC0010INProw("CNTL07")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL07", MC0010INProw("CNTL07"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL07") = ""
                Else
                    CODENAME_get("CNTL07", MC0010INProw("CNTL07"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分07）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分07）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分08：CNTL08)
            WW_TEXT = MC0010INProw("CNTL08")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL08", MC0010INProw("CNTL08"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL08") = ""
                Else
                    CODENAME_get("CNTL08", MC0010INProw("CNTL08"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分08）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分08）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(集計区分09：CNTL09)
            WW_TEXT = MC0010INProw("CNTL09")
            Master.CheckField(WF_CAMPCODE.Text, "CNTL09", MC0010INProw("CNTL09"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTL09") = ""
                Else
                    CODENAME_get("CNTL09", MC0010INProw("CNTL09"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（集計区分09）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（集計区分09）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(台数数量集計：CNTLVALUE)
            WW_TEXT = MC0010INProw("CNTLVALUE")
            Master.CheckField(WF_CAMPCODE.Text, "CNTLVALUE", MC0010INProw("CNTLVALUE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("CNTLVALUE") = ""
                Else
                    CODENAME_get("CNTLVALUE", MC0010INProw("CNTLVALUE"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（台数数量集計）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（台数数量集計）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '・明細項目(売上計上区分：URIKBN)
            WW_TEXT = MC0010INProw("URIKBN")
            Master.CheckField(WF_CAMPCODE.Text, "URIKBN", MC0010INProw("URIKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'LeftBox存在チェック
                If WW_TEXT = "" Then
                    MC0010INProw("URIKBN") = ""
                Else
                    CODENAME_get("URIKBN", MC0010INProw("URIKBN"), WW_TEXT, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラーが存在します。（売上計上区分）"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINEERR_SW = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・エラーが存在します。（売上計上区分）"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINEERR_SW = "ERR"
            End If

            '○操作設定
            If WW_LINEERR_SW = "" Then
                If MC0010INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MC0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MC0010INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
        For Each MC0010row As DataRow In MC0010tbl.Rows

            '読み飛ばし
            If (MC0010row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                MC0010row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                MC0010row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                MC0010row("STYMD") = "" Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each checkRow As DataRow In MC0010tbl.Rows

                '同一KEY以外は読み飛ばし
                If MC0010row("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                   MC0010row("TORICODE") = checkRow("TORICODE") AndAlso
                   MC0010row("OILTYPE") = checkRow("OILTYPE") AndAlso
                   MC0010row("ORDERORG") = checkRow("ORDERORG") AndAlso
                   checkRow("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If MC0010row("STYMD") = checkRow("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(MC0010row("STYMD"), WW_DATE_ST)
                    Date.TryParse(MC0010row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(checkRow("STYMD"), WW_DATE_ST2)
                    Date.TryParse(checkRow("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, MC0010row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next

            If WW_LINEERR_SW = "" Then
                MC0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                MC0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データ登録・更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MC0010tbl_UPD()

        '○画面状態設定
        For Each MC0010row As DataRow In MC0010tbl.Rows
            Select Case MC0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○追加変更判定
        For Each MC0010INProw As DataRow In MC0010INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0010INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MC0010INProw("OPERATION") = "Insert"

            For Each MC0010row As DataRow In MC0010tbl.Rows
                'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                If MC0010row("CAMPCODE") = MC0010INProw("CAMPCODE") AndAlso
                   MC0010row("TORICODE") = MC0010INProw("TORICODE") AndAlso
                   MC0010row("OILTYPE") = MC0010INProw("OILTYPE") AndAlso
                   MC0010row("ORDERORG") = MC0010INProw("ORDERORG") AndAlso
                  (MC0010row("STYMD") = MC0010INProw("STYMD") OrElse
                   MC0010row("STYMD") = "") Then

                    MC0010INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '変更無を操作無とする
        For Each MC0010INProw As DataRow In MC0010INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0010INProw("OPERATION") <> "Update" Then
                Continue For
            End If

            For Each MC0010row As DataRow In MC0010tbl.Rows
                '同一KEY以外は読み飛ばし
                If MC0010row("CAMPCODE") = MC0010INProw("CAMPCODE") AndAlso
                    MC0010row("TORICODE") = MC0010INProw("TORICODE") AndAlso
                    MC0010row("OILTYPE") = MC0010INProw("OILTYPE") AndAlso
                    MC0010row("ORDERORG") = MC0010INProw("ORDERORG") AndAlso
                   (MC0010row("STYMD") = MC0010INProw("STYMD") OrElse MC0010row("STYMD") = "") Then
                Else
                    Continue For
                End If

                '変更有無
                If MC0010row("ENDYMD") = MC0010INProw("ENDYMD") AndAlso
                   MC0010row("CNTL01") = MC0010INProw("CNTL01") AndAlso
                   MC0010row("CNTL02") = MC0010INProw("CNTL02") AndAlso
                   MC0010row("CNTL03") = MC0010INProw("CNTL03") AndAlso
                   MC0010row("CNTL04") = MC0010INProw("CNTL04") AndAlso
                   MC0010row("CNTL05") = MC0010INProw("CNTL05") AndAlso
                   MC0010row("CNTL06") = MC0010INProw("CNTL06") AndAlso
                   MC0010row("CNTL07") = MC0010INProw("CNTL07") AndAlso
                   MC0010row("CNTL08") = MC0010INProw("CNTL08") AndAlso
                   MC0010row("CNTL09") = MC0010INProw("CNTL09") AndAlso
                   MC0010row("CNTLVALUE") = MC0010INProw("CNTLVALUE") AndAlso
                   MC0010row("URIKBN") = MC0010INProw("URIKBN") AndAlso
                   MC0010row("DELFLG") = MC0010INProw("DELFLG") Then

                    MC0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                End If

                Exit For
            Next
        Next

        'テーブル反映(変更)
        For Each MC0010INProw As DataRow In MC0010INPtbl.Rows
            Select Case MC0010INProw("OPERATION")
                Case "Update"       '○更新（Update）
                    TBL_Update_SUB(MC0010INProw)
                Case "Insert"       '○更新（Insert）
                    TBL_Insert_SUB(MC0010INProw)
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

        For Each MC0010row As DataRow In MC0010tbl.Rows

            If MC0010row("CAMPCODE") = INProw("CAMPCODE") AndAlso
               MC0010row("TORICODE") = INProw("TORICODE") AndAlso
               MC0010row("OILTYPE") = INProw("OILTYPE") AndAlso
               MC0010row("ORDERORG") = INProw("ORDERORG") AndAlso
              (MC0010row("STYMD") = INProw("STYMD") OrElse MC0010row("STYMD") = "") Then

                INProw("LINECNT") = MC0010row("LINECNT")
                INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                INProw("TIMSTP") = MC0010row("TIMSTP")
                INProw("SELECT") = 1
                INProw("HIDDEN") = 0

                MC0010row.ItemArray = INProw.ItemArray

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

        INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        Dim MC0010row As DataRow = MC0010tbl.NewRow
        MC0010row.ItemArray = INProw.ItemArray

        'KEY設定
        MC0010row("LINECNT") = MC0010tbl.Rows.Count + 1
        MC0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        MC0010row("TIMSTP") = "0"
        MC0010row("SELECT") = 1
        MC0010row("HIDDEN") = 0

        MC0010tbl.Rows.Add(MC0010row)

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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal args As Hashtable = Nothing)

        '○名称取得
        O_TEXT = ""
        O_RTN = ""

        If Not String.IsNullOrEmpty(I_VALUE) Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"     '会社名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                    Case "TORICODE"     '取引先名
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, args)
                    Case "ORDERORG"     '受注組織
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, args)
                    Case "OILTYPE"      '油種
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))
                    Case "CNTL01", "CNTL02", "CNTL03", "CNTL04", "CNTL05", "CNTL06", "CNTL07", "CNTL08", "CNTL09"
                        '届日計,出庫日計,出荷場計,業車計,車腹計,乗務員計,届先計,品１計,品２計
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MC0010_CNTLKBN"))
                    Case "CNTLVALUE"    '台数数量集計
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MC0010_CNTLVALUE"))
                    Case "URIKBN"       '売上計上区分
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_URIKBN, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "URIKBN"))
                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
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
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal MC0010INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 会社　　　=" & MC0010INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 取引先　　=" & MC0010INProw("TORICODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 受注組織　=" & MC0010INProw("ORDERORG") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 油種　　　=" & MC0010INProw("OILTYPE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 開始年月日=" & MC0010INProw("STYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 終了年月日=" & MC0010INProw("ENDYMD") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除　　　=" & MC0010INProw("DELFLG") & " "
        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

End Class

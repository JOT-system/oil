Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' メニュー項目メンテナンス画面（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0008PROFMMAP
    Inherits Page

    '検索結果格納ds
    Private CO0008tbl As DataTable                              'Grid格納用テーブル
    Private CO0008INPtbl As DataTable                           'チェック用テーブル
    Private CO0008UPDtbl As DataTable                           '更新用テーブル

    Private WW_RTN As String                                    'サブ用リターンコード
    Private WW_RTN_Detail As String                             'サブ用リターンコード(項目名)
    Private WW_RTN_Action As String                             'サブ用リターンコード(重複:Dub , 新規:Insert , 更新:Update)

    '共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'ユーザプロファイル（GridView）設定
    Private CS0020JOURNAL As New CS0020JOURNAL                  'Journal Out
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'UPLOAD_XLSデータ取得
    Private CS0025AUTHORget As New CS0025AUTHORget              '権限チェック(APサーバチェックなし)
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0050Session As New CS0050SESSION                  'セッション管理

    '共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID
    Private Const CONST_MAX_DETAIL_RECORD As Integer = 21       'メニュー明細数

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
                    If Not Master.RecoverTable(CO0008tbl) Then
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
                        Case "WF_REP_RADIO"
                            WF_REP_RADIO_Click()
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
            If Not IsNothing(CO0008tbl) Then
                CO0008tbl.Clear()
                CO0008tbl.Dispose()
                CO0008tbl = Nothing
            End If

            If Not IsNothing(CO0008INPtbl) Then
                CO0008INPtbl.Clear()
                CO0008INPtbl.Dispose()
                CO0008INPtbl = Nothing
            End If

            If Not IsNothing(CO0008UPDtbl) Then
                CO0008UPDtbl.Clear()
                CO0008UPDtbl.Dispose()
                CO0008UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        WF_SELMAP.Focus()
        WF_FIELD.Value = ""
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
        rightview.resetindex()
        '○画面表示データ取得
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(CO0008tbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(CO0008tbl)
            TBLview.RowFilter = "TITLEKBN = 'H' and LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = GRCO0008WRKINC.MAPID
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

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)=HIDDEN ： 0=表示対象 , 1=非表示対象)
        '　※　絞込（Cells(0)=LINECNT： 0=詳細情報 , 0<>一覧情報)
        For Each CO0008row As DataRow In CO0008tbl.Rows
            If CO0008row("HIDDEN") = 0 AndAlso
                CO0008row("LINECNT") > 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                CO0008row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(CO0008tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "TITLEKBN = 'H' and HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013PROFview.MAPID = GRCO0008WRKINC.MAPID
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

        '○絞り込み操作（GridView明細Hidden設定）
        For Each CO0008row As DataRow In CO0008tbl.Rows
            Dim WW_HANTEI As Integer = 0

            '画面絞込判定
            If WF_SELMAP.Text <> "" AndAlso
                WF_SELMAP.Text <> CO0008row("MAPIDP") Then
                WW_HANTEI = WW_HANTEI + 1

            End If

            '画面(Grid)のHIDDEN列に結果格納
            If WW_HANTEI = 0 Then
                CO0008row("HIDDEN") = 0     '表示対象
            Else
                CO0008row("HIDDEN") = 1     '非表示対象
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(CO0008tbl)

        '画面先頭を表示
        WF_GridPosition.Text = "1"

        '画面(Grid)のHIDDEN列により、表示/非表示を行う。
        WF_GRID_Scrole()

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELMAP.Focus()

    End Sub
    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_ERRCODE As String = C_MESSAGE_NO.NORMAL

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        Try
            'ジャーナル出力用テーブル準備
            Master.CreateEmptyTable(CO0008UPDtbl)
            CO0008UPDtbl.Columns.Add("INITYMD", GetType(DateTime))
            CO0008UPDtbl.Columns.Add("UPDYMD", GetType(DateTime))
            CO0008UPDtbl.Columns.Add("UPDUSER", GetType(String))

            'メッセージ初期化
            rightview.setErrorReport("")

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ;                      " _
                    & " set @hensuu = 0 ;                                " _
                    & " DECLARE hensuu CURSOR FOR                        " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu     " _
                    & "     FROM       S0024_PROFMMAP                    " _
                    & "     WHERE    CAMPCODE       = @P01               " _
                    & "       and    MAPIDP         = @P02               " _
                    & "       and    VARIANTP       = @P03               " _
                    & "       and    TITLEKBN       = @P04               " _
                    & "       and    POSIROW        = @P05               " _
                    & "       and    POSICOL        = @P06               " _
                    & "       and    STYMD          = @P07              ;" _
                    & "                                                  " _
                    & " OPEN hensuu ;                                    " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu             ;" _
                    & " IF ( @@FETCH_STATUS = 0 )                        " _
                    & "   UPDATE       S0024_PROFMMAP                    " _
                    & "   SET        ENDYMD         = @P08             , " _
                    & "              MAPID          = @P09             , " _
                    & "              VARIANT        = @P10             , " _
                    & "              TITLENAMES     = @P11             , " _
                    & "              MAPNAMES       = @P12             , " _
                    & "              MAPNAMEL       = @P13             , " _
                    & "              DELFLG         = @P14             , " _
                    & "              INITYMD        = @P15             , " _
                    & "              UPDYMD         = @P16             , " _
                    & "              UPDUSER        = @P17             , " _
                    & "              UPDTERMID      = @P18             , " _
                    & "              RECEIVEYMD     = @P19               " _
                    & "   WHERE      CAMPCODE       = @P01               " _
                    & "       and    MAPIDP         = @P02               " _
                    & "       and    VARIANTP       = @P03               " _
                    & "       and    TITLEKBN       = @P04               " _
                    & "       and    POSIROW        = @P05               " _
                    & "       and    POSICOL        = @P06               " _
                    & "       and    STYMD          = @P07              ;" _
                    & " IF ( @@FETCH_STATUS <> 0 )                       " _
                    & "    INSERT INTO S0024_PROFMMAP(                   " _
                    & "              CAMPCODE                          , " _
                    & "              MAPIDP                            , " _
                    & "              VARIANTP                          , " _
                    & "              TITLEKBN                          , " _
                    & "              POSIROW                           , " _
                    & "              POSICOL                           , " _
                    & "              STYMD                             , " _
                    & "              ENDYMD                            , " _
                    & "              MAPID                             , " _
                    & "              VARIANT                           , " _
                    & "              TITLENAMES                        , " _
                    & "              MAPNAMES                          , " _
                    & "              MAPNAMEL                          , " _
                    & "              DELFLG                            , " _
                    & "              INITYMD                           , " _
                    & "              UPDYMD                            , " _
                    & "              UPDUSER                           , " _
                    & "              UPDTERMID                         , " _
                    & "              RECEIVEYMD                          " _
                    & "    )  VALUES (                                   " _
                    & "              @P01                              , " _
                    & "              @P02                              , " _
                    & "              @P03                              , " _
                    & "              @P04                              , " _
                    & "              @P05                              , " _
                    & "              @P06                              , " _
                    & "              @P07                              , " _
                    & "              @P08                              , " _
                    & "              @P09                              , " _
                    & "              @P10                              , " _
                    & "              @P11                              , " _
                    & "              @P12                              , " _
                    & "              @P13                              , " _
                    & "              @P14                              , " _
                    & "              @P15                              , " _
                    & "              @P16                              , " _
                    & "              @P17                              , " _
                    & "              @P18                              , " _
                    & "              @P19                                " _
                    & "    )                                            ;" _
                    & " CLOSE hensuu                                    ;" _
                    & " DEALLOCATE hensuu                               ;"

                '○更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                Dim SQLStr2 As String =
                      " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP       " _
                    & " FROM     S0024_PROFMMAP                          " _
                    & " WHERE    CAMPCODE     = @P1                      " _
                    & "   and    MAPIDP       = @P2                      " _
                    & "   and    VARIANTP     = @P3                      " _
                    & "   and    TITLEKBN     = @P4                      " _
                    & "   and    POSIROW      = @P5                      " _
                    & "   and    POSICOL      = @P6                      " _
                    & "   and    STYMD        = @P7                     ;"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdTim As New SqlCommand(SQLStr2, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 50)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 50)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Int)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 50)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 50)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 50)
                    Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)
                    Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)
                    Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)
                    Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)
                    Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 30)
                    Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)

                    Dim PARA1 As SqlParameter = SQLcmdTim.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmdTim.Parameters.Add("@P2", SqlDbType.NVarChar, 50)
                    Dim PARA3 As SqlParameter = SQLcmdTim.Parameters.Add("@P3", SqlDbType.NVarChar, 50)
                    Dim PARA4 As SqlParameter = SQLcmdTim.Parameters.Add("@P4", SqlDbType.NVarChar, 1)
                    Dim PARA5 As SqlParameter = SQLcmdTim.Parameters.Add("@P5", SqlDbType.Int)
                    Dim PARA6 As SqlParameter = SQLcmdTim.Parameters.Add("@P6", SqlDbType.Int)
                    Dim PARA7 As SqlParameter = SQLcmdTim.Parameters.Add("@P7", SqlDbType.Date)

                    'ＤＢ更新
                    For Each CO0008row As DataRow In CO0008tbl.Rows
                        If CO0008row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                            CO0008row("OPERATION") <> C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            Continue For
                        End If

                        '新規追加(タイムスタンプ0)かつ削除の場合は無視
                        If CO0008row("TIMSTP") = "0" AndAlso CO0008row("DELFLG") = C_DELETE_FLG.DELETE Then
                            CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        '新規追加(タイムスタンプ0)かつ画面IDとタイトル名称がブランクの場合(空欄行)は無視
                        If CO0008row("TIMSTP") = "0" AndAlso CO0008row("TITLEKBN") = "I" AndAlso
                            CO0008row("MAPID") = "" AndAlso CO0008row("TITLENAMES") = "" Then
                            CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        Dim WW_DATENOW As DateTime = Date.Now

                        'ＤＢ更新
                        PARA01.Value = CO0008row("CAMPCODE")
                        PARA02.Value = CO0008row("MAPIDP")
                        PARA03.Value = CO0008row("VARIANTP")
                        PARA04.Value = CO0008row("TITLEKBN")
                        PARA05.Value = CO0008row("POSIROW")
                        PARA06.Value = CO0008row("POSICOL")
                        PARA07.Value = CO0008row("STYMD")
                        PARA08.Value = CO0008row("ENDYMD")
                        PARA09.Value = CO0008row("MAPID")
                        PARA10.Value = CO0008row("VARIANT")
                        PARA11.Value = CO0008row("TITLENAMES")
                        PARA12.Value = CO0008row("MAPNAMES")
                        PARA13.Value = CO0008row("MAPNAMEL")
                        PARA14.Value = CO0008row("DELFLG")
                        PARA15.Value = WW_DATENOW
                        PARA16.Value = WW_DATENOW
                        PARA17.Value = Master.USERID
                        PARA18.Value = Master.USERTERMID
                        PARA19.Value = C_DEFAULT_YMD

                        SQLcmd.ExecuteNonQuery()

                        '結果 --> テーブル(CO0008tbl)反映
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        Dim CO0008UPDrow As DataRow = CO0008UPDtbl.NewRow
                        CO0008UPDrow("CAMPCODE") = CO0008row("CAMPCODE")
                        CO0008UPDrow("MAPIDP") = CO0008row("MAPIDP")
                        CO0008UPDrow("VARIANTP") = CO0008row("VARIANTP")
                        CO0008UPDrow("TITLEKBN") = CO0008row("TITLEKBN")
                        CO0008UPDrow("POSICOL") = CO0008row("POSICOL")
                        CO0008UPDrow("POSIROW") = CO0008row("POSIROW")
                        CO0008UPDrow("STYMD") = CO0008row("STYMD")
                        CO0008UPDrow("ENDYMD") = CO0008row("ENDYMD")
                        CO0008UPDrow("MAPID") = CO0008row("MAPID")
                        CO0008UPDrow("VARIANT") = CO0008row("VARIANT")
                        CO0008UPDrow("TITLENAMES") = CO0008row("TITLENAMES")
                        CO0008UPDrow("MAPNAMES") = CO0008row("MAPNAMES")
                        CO0008UPDrow("MAPNAMEL") = CO0008row("MAPNAMEL")
                        CO0008UPDrow("DELFLG") = CO0008row("DELFLG")
                        CO0008UPDrow("INITYMD") = WW_DATENOW
                        CO0008UPDrow("UPDYMD") = WW_DATENOW
                        CO0008UPDrow("UPDUSER") = CS0050Session.USERID

                        '○更新ジャーナル追加
                        CS0020JOURNAL.TABLENM = "S0024_PROFMMAP"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = CO0008UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                            CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWRITE.CS0011LOGWrite()

                            Exit Sub
                        End If

                        PARA1.Value = CO0008row("CAMPCODE")
                        PARA2.Value = CO0008row("MAPIDP")
                        PARA3.Value = CO0008row("VARIANTP")
                        PARA4.Value = CO0008row("TITLEKBN")
                        PARA5.Value = CO0008row("POSIROW")
                        PARA6.Value = CO0008row("POSICOL")
                        PARA7.Value = CO0008row("STYMD")

                        Using SQLdr As SqlDataReader = SQLcmdTim.ExecuteReader()
                            While SQLdr.Read
                                CO0008row("TIMSTP") = SQLdr("TIMSTP")
                            End While
                        End Using
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0024_PROFMMAP UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0024_PROFMMAP UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()
            Master.output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR)

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(CO0008tbl)

        'detailboxクリア
        Detailbox_Clear()

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELMAP.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************
    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        'DataTable.Select()を使いソート(第二引数にソート条件を書く)
        Dim rows As DataRow() = CO0008tbl.Select(Nothing, "CAMPCODE, MAPIDP, VARIANTP, TITLEKBN, POSIROW, POSICOL").Clone()

        'ソート後の DataTable を用意
        Dim dtblSrt As New DataTable()
        'ソート前テーブルの情報をクローン
        dtblSrt = CO0008tbl.Clone()

        'ソートされてる DataRow 配列をソート後の DataTable に追加
        For Each row As DataRow In rows
            dtblSrt.ImportRow(row)
        Next

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRCO0008WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = CO0008tbl                        'データ参照DataTable
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

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理                                         ***
    ' ******************************************************************************
    Protected Sub WF_ButtonCSV_Click()

        'DataTable.Select()を使いソート(第二引数にソート条件を書く)
        Dim rows As DataRow() = CO0008tbl.Select(Nothing, "CAMPCODE, MAPIDP, VARIANTP, TITLEKBN, POSICOL, POSIROW").Clone()

        'ソート後の DataTable を用意
        Dim dtblSrt As New DataTable()
        'ソート前テーブルの情報をクローン
        dtblSrt = CO0008tbl.Clone()

        'ソートされてる DataRow 配列をソート後の DataTable に追加
        For Each row As DataRow In rows
            dtblSrt.ImportRow(row)
        Next

        '○帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRCO0008WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = dtblSrt                          'データ参照DataTable
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

        '○自画面MAPIDより親MAP・URLを取得
        Master.transitionPrevPage()

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
        Dim WW_TBLview As DataView = New DataView(CO0008tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0' and TITLEKBN = 'H'"

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

        '○画面detailboxへ表示
        '画面選択明細(GridView)から画面detailboxへ表示

        '○抽出条件(ヘッダーレコードより)定義]
        Dim WW_CAMPCODE As String = ""
        Dim WW_MAPID As String = ""
        Dim WW_VARIANT As String = ""
        Dim WW_STYMD As String = ""
        Dim WW_ENDYMD As String = ""
        Dim WW_Position As Integer = 0

        'LINECNT
        Dim LINECNT As Long = 0
        Try
            Long.TryParse(WF_GridDBclick.Text, LINECNT)
        Catch ex As Exception
            Exit Sub
        End Try
        '項番退避(WF_GridDBclick)より実アドレス取得
        For i As Integer = 0 To CO0008tbl.Rows.Count - 1
            If CO0008tbl.Rows(i)("LINECNT") = LINECNT Then
                WW_Position = i
                Exit For
            End If
        Next
        '○ダブルクリック明細情報取得設定（GridView --> Detailboxヘッダー情報)
        For j As Integer = 0 To CO0008tbl.Columns.Count - 1
            Dim WW_DataField As String = CO0008tbl.Columns.Item(j).ColumnName '項目名取得用

            Select Case WW_DataField
                '○共通項目"
                Case "LINECNT"
                    WF_Sel_LINECNT.Text = CO0008tbl.Rows(WW_Position)(j)
                    '○画面固有項目"
                Case "CAMPCODE"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_CAMPCODE.Text = ""
                    Else
                        WF_CAMPCODE.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "CAMPNAME"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_CAMPCODE_TEXT.Text = ""
                    Else
                        WF_CAMPCODE_TEXT.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "MAPIDP"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_MAPIDP.Text = ""
                    Else
                        WF_MAPIDP.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "MAPPNAME"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_MAPIDP_TEXT.Text = ""
                    Else
                        WF_MAPIDP_TEXT.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "VARIANTP"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_VARIANTP.Text = ""
                    Else
                        WF_VARIANTP.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "VARIPNAME"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_VARIANTP_TEXT.Text = ""
                    Else
                        WF_VARIANTP_TEXT.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "STYMD"
                    If CO0008tbl.Rows(WW_Position)(j).ToString = "&nbsp;" Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "ENDYMD"
                    If CO0008tbl.Rows(WW_Position)(j).ToString = "&nbsp;" Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = CO0008tbl.Rows(WW_Position)(j)
                    End If
                Case "DELFLG"
                    If CO0008tbl.Rows(WW_Position)(j) = "&nbsp;" Then
                        WF_DELFLG.Text = ""
                        WF_DELFLG_TEXT.Text = ""
                    Else
                        WF_DELFLG.Text = CO0008tbl.Rows(WW_Position)(j)
                        CODENAME_get("DELFLG", CO0008tbl.Rows(WW_Position)(j), WF_DELFLG_TEXT.Text, WW_DUMMY)
                    End If
            End Select
        Next

        '○ダブルクリック明細情報取得設定（GridView --> Detailbox明細情報)

        'CO0008tbl(項目設定)からデータ抽出
        Dim TBLrd As DataRow()
        TBLrd = CO0008tbl.Select("TITLEKBN = 'I' and CAMPCODE = '" & WF_CAMPCODE.Text & "' and MAPIDP = '" & WF_MAPIDP.Text & "' and VARIANTP = '" & WF_VARIANTP.Text & "' and STYMD ='" & WF_STYMD.Text & "' and ENDYMD = '" & WF_ENDYMD.Text & "'", "POSICOL, POSIROW")

        '一時Table(CO0008INPtbl)準備
        Master.CreateEmptyTable(CO0008INPtbl)
        '左右21件作成
        For ccnt As Integer = 1 To 2
            For rcnt As Integer = 1 To CONST_MAX_DETAIL_RECORD

                Dim CO0008INProw As DataRow = CO0008INPtbl.NewRow()
                CO0008INProw("POSIROW") = rcnt
                CO0008INProw("POSICOL") = ccnt
                CO0008INProw("TITLEKBN") = "I"
                CO0008INPtbl.Rows.Add(CO0008INProw)
            Next rcnt
        Next ccnt

        'SelectデータをCO0008INPtblへ張り付け
        For i As Integer = 0 To TBLrd.Count - 1
            For j As Integer = 0 To CO0008INPtbl.Rows.Count - 1
                If (CO0008INPtbl.Rows(j)("POSIROW") = TBLrd(i).Item("POSIROW")) AndAlso
                   (CO0008INPtbl.Rows(j)("POSICOL") = TBLrd(i).Item("POSICOL")) Then
                    CO0008INPtbl.Rows(j)("CAMPCODE") = TBLrd(i).Item("CAMPCODE")
                    CO0008INPtbl.Rows(j)("MAPIDP") = TBLrd(i).Item("MAPIDP")
                    CO0008INPtbl.Rows(j)("VARIANTP") = TBLrd(i).Item("VARIANTP")
                    CO0008INPtbl.Rows(j)("TITLEKBN") = TBLrd(i).Item("TITLEKBN")
                    CO0008INPtbl.Rows(j)("MAPID") = TBLrd(i).Item("MAPID")
                    CO0008INPtbl.Rows(j)("VARIANT") = TBLrd(i).Item("VARIANT")
                    CO0008INPtbl.Rows(j)("STYMD") = TBLrd(i).Item("STYMD")
                    CO0008INPtbl.Rows(j)("ENDYMD") = TBLrd(i).Item("ENDYMD")
                    CO0008INPtbl.Rows(j)("TITLENAMES") = TBLrd(i).Item("TITLENAMES")
                    CO0008INPtbl.Rows(j)("MAPNAMES") = TBLrd(i).Item("MAPNAMES")
                    CO0008INPtbl.Rows(j)("MAPNAMEL") = TBLrd(i).Item("MAPNAMEL")
                    CO0008INPtbl.Rows(j)("DELFLG") = TBLrd(i).Item("DELFLG")
                    CO0008INPtbl.Rows(j)("CAMPNAME") = TBLrd(i).Item("CAMPNAME")
                    CO0008INPtbl.Rows(j)("MAPPNAME") = TBLrd(i).Item("MAPPNAME")
                    CO0008INPtbl.Rows(j)("VARIPNAME") = TBLrd(i).Item("VARIPNAME")
                    CO0008INPtbl.Rows(j)("MAPNAME") = TBLrd(i).Item("MAPNAME")
                    CO0008INPtbl.Rows(j)("VARINAME") = TBLrd(i).Item("VARINAME")
                    Exit For
                End If
            Next
        Next

        Dim TBLrd2 As DataRow()
        TBLrd2 = CO0008INPtbl.Select("TITLEKBN = 'I'", "POSICOL, POSIROW ")

        '明細へデータ張り付け
        WF_Repeater.Visible = True
        WF_Repeater.DataSource = TBLrd2
        WF_Repeater.DataBind()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each CO0008row As DataRow In CO0008tbl.Rows
            Select Case CO0008row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        For Each CO0008row As DataRow In CO0008tbl.Rows
            If CO0008row("LINECNT").ToString = WF_GridDBclick.Text Then
                Select Case CO0008row("OPERATION")
                    Case C_LIST_OPERATION_CODE.NODATA
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.NODISP
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.SELECTED
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    Case C_LIST_OPERATION_CODE.UPDATING
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    Case C_LIST_OPERATION_CODE.ERRORED
                        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    Case Else
                End Select
            End If
        Next

        '○画面表示データ保存
        Master.SaveTable(CO0008tbl)

        '○画面編集
        '画面(Grid)のHIDDEN列により、表示/非表示を行う。
        WF_CAMPCODE.Focus()

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
                    Dim prmData As Hashtable = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case 901
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LRPOSI")
                        Case LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                            Select Case WF_FIELD.Value
                                Case "WF_Rep_VARIANT"
                                    Dim MAPID As String = ""
                                    For Each reitem As RepeaterItem In WF_Repeater.Items
                                        If CType(reitem.FindControl("WF_Rep_POSICOL"), Label).Text = Mid(WF_REP_POSITION.Value, 1, 1) Then
                                            Dim WW_SEQ As Integer = Mid(WF_REP_POSITION.Value, 2, 2)
                                            If CType(reitem.FindControl("WF_Rep_POSIROW"), Label).Text = WW_SEQ.ToString() Then
                                                MAPID = CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    prmData = work.CreateVariantList(CS0050Session.getConnection, Master.PROF_VIEW, MAPID)
                                Case "WF_VARIANTP"
                                    prmData = work.CreateVariantList(CS0050Session.getConnection, Master.PROF_VIEW, WF_MAPIDP.Text)
                                Case Else
                                    prmData = work.CreateVariantList(CS0050Session.getConnection, Master.PROF_VIEW)
                            End Select

                    End Select
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

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○DetailBoxをCO0008INPtblへ退避
        Master.CreateEmptyTable(CO0008INPtbl)
        DetailBoxToCO0008INPtbl(WW_RTN)
        If Not isNormal(WW_RTN) Then
            Exit Sub
        End If

        '○項目チェック
        CO0008INPtbl_CHEK(WW_ERR_SW)

        '○チェックOKデータ(CO0008UPDtbl)を一覧(CO0008tbl)へ反映
        If isNormal(WW_ERR_SW) Then
            CO0008tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(CO0008tbl)

        If isNormal(WW_ERR_SW) Then
            Detailbox_Clear()
            Master.output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        'カーソル設定
        WF_CAMPCODE.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToCO0008INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_DATE As Date

        '○入力文字置き換え(ヘッダー)
        '○画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_MAPIDP.Text)            '親画面ＩＤ
        Master.eraseCharToIgnore(WF_VARIANTP.Text)          '親変数
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日
        Master.eraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If WF_Repeater.Items.Count = 0 Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToCO0008INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○ラジオボタン選択による項目制御
        '○Repeaterの行数分ループ
        Try
            For Each reitem As RepeaterItem In WF_Repeater.Items
                If CType(reitem.FindControl("WF_Rep_SW1"), RadioButton).Checked Then
                    'ボタン名称(WF_Rep_NAMES)
                    CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Text = ""
                    '画面ＩＤ(WF_Rep_MAPID)
                    CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text = ""
                    '変数(WF_Rep_VARIANT)
                    CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text = ""
                Else
                    '見出名称(WF_Rep_TITLE)
                    CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Text = ""
                End If
            Next
        Catch ex As Exception

        End Try

        '○画面情報からインプットテーブル（ヘッダー）作成

        WF_Repeater.Visible = True

        '○ヘッダー行追加
        Dim CO0008INPHrow As DataRow = CO0008INPtbl.NewRow()


        '○共通項目
        CO0008INPHrow("LINECNT") = WF_Sel_LINECNT.Text                  'DBの固定フィールド
        CO0008INPHrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA       'DBの固定フィールド
        CO0008INPHrow("TIMSTP") = 0                                     'DBの固定フィールド
        CO0008INPHrow("SELECT") = 0                                     'DBの固定フィールド
        CO0008INPHrow("HIDDEN") = 0                                     'DBの固定フィールド

        '○画面固有項目
        CO0008INPHrow("CAMPCODE") = WF_CAMPCODE.Text
        CO0008INPHrow("MAPIDP") = WF_MAPIDP.Text
        CO0008INPHrow("VARIANTP") = WF_VARIANTP.Text
        CO0008INPHrow("TITLEKBN") = "H"
        CO0008INPHrow("POSICOL") = 0
        CO0008INPHrow("POSIROW") = 0
        CO0008INPHrow("MAPID") = ""
        CO0008INPHrow("VARIANT") = ""
        Date.TryParse(WF_STYMD.Text, WW_DATE)
        CO0008INPHrow("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
        Date.TryParse(WF_ENDYMD.Text, WW_DATE)
        CO0008INPHrow("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
        CO0008INPHrow("TITLENAMES") = ""
        CO0008INPHrow("MAPNAMES") = ""
        CO0008INPHrow("MAPNAMEL") = ""
        CO0008INPHrow("DELFLG") = WF_DELFLG.Text
        CO0008INPHrow("CAMPNAME") = ""
        CO0008INPHrow("MAPPNAME") = ""
        CO0008INPHrow("VARIPNAME") = ""
        CO0008INPHrow("MAPNAME") = ""
        CO0008INPHrow("VARINAME") = ""

        CODENAME_get("CAMPCODE", CO0008INPHrow("CAMPCODE"), CO0008INPHrow("CAMPNAME"), WW_DUMMY)
        CODENAME_get("MAPID", WF_MAPIDP.Text, CO0008INPHrow("MAPPNAME"), WW_DUMMY)
        CODENAME_get("VARI", WF_MAPIDP.Text & "_" & WF_VARIANTP.Text, CO0008INPHrow("VARIPNAME"), WW_DUMMY)

        CO0008INPtbl.Rows.Add(CO0008INPHrow)
        '○Repeaterの行数分ループ
        Try
            For Each reitem As RepeaterItem In WF_Repeater.Items

                '○入力文字置き換え(ヘッダー)
                '○画面(Repeater明細情報)

                '左右位置(WF_Rep_POSICOL)

                '項番(WF_Rep_POSIROW)

                '見出名称(WF_Rep_TITLENAMES)
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                If CS0010CHARstr.CHARIN <> CS0010CHARstr.CHAROUT Then
                    CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Text = CS0010CHARstr.CHAROUT
                End If

                'ボタン名称(WF_Rep_NAMES)
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                If CS0010CHARstr.CHARIN <> CS0010CHARstr.CHAROUT Then
                    CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Text = CS0010CHARstr.CHAROUT
                End If

                '画面ＩＤ(WF_Rep_MAPID)
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                If CS0010CHARstr.CHARIN <> CS0010CHARstr.CHAROUT Then
                    CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text = CS0010CHARstr.CHAROUT
                End If

                '変数(WF_Rep_VARIANT)
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                If CS0010CHARstr.CHARIN <> CS0010CHARstr.CHAROUT Then
                    CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text = CS0010CHARstr.CHAROUT
                End If

                '画面名称(WF_Rep_MAPID_TEXT)
                '変数名称(WF_Rep_VARIANT_TEXT)

                '○画面情報からインプットテーブル（アイテム）作成

                '○明細行追加
                Dim CO0008INProw As DataRow = CO0008INPtbl.NewRow()

                'DBの固定フィールド
                CO0008INProw("LINECNT") = "0"
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                CO0008INProw("TIMSTP") = 0
                CO0008INProw("SELECT") = 0
                CO0008INProw("HIDDEN") = 1

                '○画面固有項目
                CO0008INProw("CAMPCODE") = WF_CAMPCODE.Text
                CO0008INProw("MAPIDP") = WF_MAPIDP.Text
                CO0008INProw("VARIANTP") = WF_VARIANTP.Text
                CO0008INProw("TITLEKBN") = "I"
                CO0008INProw("POSIROW") = CType(reitem.FindControl("WF_Rep_POSIROW"), Label).Text
                CO0008INProw("POSICOL") = CType(reitem.FindControl("WF_Rep_POSICOL"), Label).Text
                CO0008INProw("MAPID") = CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text
                CO0008INProw("VARIANT") = CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text
                Date.TryParse(WF_STYMD.Text, WW_DATE)
                CO0008INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                Date.TryParse(WF_ENDYMD.Text, WW_DATE)
                CO0008INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                CO0008INProw("TITLENAMES") = CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Text
                CO0008INProw("MAPNAMES") = CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Text
                CO0008INProw("MAPNAMEL") = CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Text
                CO0008INProw("DELFLG") = WF_DELFLG.Text
                CO0008INProw("CAMPNAME") = ""
                CO0008INProw("MAPPNAME") = ""
                CO0008INProw("VARIPNAME") = ""
                CO0008INProw("MAPNAME") = ""
                CO0008INProw("VARINAME") = ""
                '名称取得
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, CO0008INProw("CAMPNAME"), WW_DUMMY)
                CODENAME_get("MAPID", WF_MAPIDP.Text, CO0008INProw("MAPPNAME"), WW_DUMMY)
                CODENAME_get("VARI", WF_MAPIDP.Text & "_" & WF_VARIANTP.Text, CO0008INProw("VARIPNAME"), WW_DUMMY)
                CODENAME_get("MAPID", CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text, CO0008INProw("MAPNAME"), WW_DUMMY)
                CODENAME_get("POSI", CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text & "_" & CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text, CO0008INProw("VARINAME"), WW_DUMMY)

                CO0008INPtbl.Rows.Add(CO0008INProw)
            Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.CAST_FORMAT_ERROR_EX, C_MESSAGE_TYPE.ABORT, "Detail set")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToCO0008INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Detail set"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.CAST_FORMAT_ERROR_EX
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.CAST_FORMAT_ERROR_EX

            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        'detailboxクリア
        Detailbox_Clear()

        'メッセージ表示
        Master.output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub
    ''' <summary>
    ''' 詳細初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Detailbox_Clear()

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each CO0008row As DataRow In CO0008tbl.Rows
            Select Case CO0008row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(CO0008tbl)

        '選択No
        WF_Sel_LINECNT.Text = ""
        '会社コード
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '親画面ＩＤ
        WF_MAPIDP.Text = ""
        WF_MAPIDP_TEXT.Text = ""
        '親変数
        WF_VARIANTP.Text = ""
        WF_VARIANTP_TEXT.Text = ""
        '有効年月日.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        '削除フラグ.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        WF_CAMPCODE.Focus()

        '行削除ができないので非表示にする(行明細0の場合、置き換えが発生しない)
        WF_Repeater.Visible = False

        'Repeaterクリア(空データをバインド)
        Master.CreateEmptyTable(CO0008INPtbl)

        WF_Repeater.DataSource = CO0008INPtbl
        WF_Repeater.DataBind()


    End Sub

    ' *** 詳細画面-初期設定 （空明細作成 & イベント追加）

    ' *** 詳細画面-イベント文字取得

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
        '項目セット　＆　フォーカス
        Select Case WF_FIELD.Value
            Case "WF_SELMAP"            '画面ID
                WF_SELMAP_TEXT.Text = WW_SelectTEXT
                WF_SELMAP.Text = WW_SelectValue
                WF_SELMAP.Focus()
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE_TEXT.Text = WW_SelectTEXT
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE.Focus()
            Case "WF_MAPIDP"            '親画面ID
                WF_MAPIDP_TEXT.Text = WW_SelectTEXT
                WF_MAPIDP.Text = WW_SelectValue
                WF_MAPIDP.Focus()
            Case "WF_VARIANTP"          '親画面変数
                WF_VARIANTP_TEXT.Text = WW_SelectTEXT
                WF_VARIANTP.Text = WW_SelectValue.Split("_")(1)
                WF_VARIANTP.Focus()
            Case "WF_DELFLG"            '削除フラグ
                WF_DELFLG_TEXT.Text = WW_SelectTEXT
                WF_DELFLG.Text = WW_SelectValue
                WF_DELFLG.Focus()
            Case "WF_Rep_MAPID"         '画面ID
                For Each reitem As RepeaterItem In WF_Repeater.Items
                    If CType(reitem.FindControl("WF_Rep_POSICOL"), Label).Text = Mid(WF_REP_POSITION.Value, 1, 1) Then
                        Dim WW_SEQ As Integer = Mid(WF_REP_POSITION.Value, 2, 2)
                        If CType(reitem.FindControl("WF_Rep_POSIROW"), Label).Text = WW_SEQ.ToString() Then
                            CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Text = WW_SelectValue
                            CType(reitem.FindControl("WF_Rep_MAPID_TEXT"), Label).Text = WW_SelectTEXT
                            CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Focus()
                            Exit Select
                        End If
                    End If
                Next
            Case "WF_Rep_VARIANT"       '変数
                For Each reitem As RepeaterItem In WF_Repeater.Items
                    If CType(reitem.FindControl("WF_Rep_POSICOL"), Label).Text = Mid(WF_REP_POSITION.Value, 1, 1) Then
                        Dim WW_SEQ As Integer = Mid(WF_REP_POSITION.Value, 2, 2)
                        If CType(reitem.FindControl("WF_Rep_POSIROW"), Label).Text = WW_SEQ.ToString Then
                            CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Text = WW_SelectValue.Split("_")(1)
                            CType(reitem.FindControl("WF_Rep_VARIANT_TEXT"), Label).Text = WW_SelectTEXT
                            CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Focus()
                            Exit Select
                        End If
                    End If
                Next
            Case "WF_STYMD"             '開始年月日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = WW_DATE
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '終了年月日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = WW_DATE
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_REP_POSITION.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_SELMAP"            '画面ID
                WF_SELMAP.Focus()
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_MAPIDP"            '親画面ID
                WF_MAPIDP.Focus()
            Case "WF_VARIANTP"          '親画面変数
                WF_VARIANTP.Focus()
            Case "WF_STYMD"             '開始年月日
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '終了年月日
                WF_ENDYMD.Focus()
            Case "WF_DELFLG"            '削除フラグ
                WF_DELFLG.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_REP_POSITION.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' *** LeftBoxカレンダ設定

    ' *** LeftBox特殊な設定

    ' ******************************************************************************
    ' ***  ファイルアップロード入力処理                                          *** 
    ' ******************************************************************************

    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○UPLOAD_XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRCO0008WRKINC.MAPID
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

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each CS23XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(CS23XLSTBLcol.ColumnName.ToString())
        Next

        '○ タイトル名称が無いと以降の処理不能(処理ロジックで使用の為)
        If WW_COLUMNS.IndexOf("TITLENAMES") < 0 Then
            Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport TITLENAMES not find")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Inport TITLENAMES not find"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Inport TITLENAMES not find"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '○ 画面IDが無いと以降の処理不能(処理ロジックで使用の為)
        If WW_COLUMNS.IndexOf("MAPID") < 0 Then
            Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport MAPID not find")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Inport MAPID not find"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Inport MAPID not find"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '○ 行位置が無いと以降の処理不能(処理ロジックで使用の為)
        If WW_COLUMNS.IndexOf("POSIROW") < 0 Then
            Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport POSIROW not find")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Inport POSIROW not find"              '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Inport POSIROW not find"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '○ 列位置が無いと以降の処理不能(処理ロジックで使用の為)
        If WW_COLUMNS.IndexOf("POSICOL") < 0 Then
            Master.output(C_MESSAGE_NO.IMPORT_ERROR, C_MESSAGE_TYPE.ERR, "Inport POSICOL not find")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Inport POSICOL not find"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Inport POSICOL not find"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.IMPORT_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        Dim WW_XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each CS23XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            WW_XLSTBLrow.ItemArray = CS23XLSTBLrow.ItemArray

            For Each CS23XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(WW_XLSTBLrow.Item(CS23XLSTBLcol)) OrElse IsNothing(WW_XLSTBLrow.Item(CS23XLSTBLcol)) Then
                    WW_XLSTBLrow.Item(CS23XLSTBLcol) = ""
                End If
            Next

            CS23XLSTBLrow.ItemArray = WW_XLSTBLrow.ItemArray
        Next

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(CO0008INPtbl)

        For Each CS23XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim CO0008INProw As DataRow = CO0008INPtbl.NewRow

            '○ 初期クリア
            For Each CO0008INPcol As DataColumn In CO0008INPtbl.Columns
                If IsDBNull(CO0008INProw.Item(CO0008INPcol)) OrElse IsNothing(CO0008INProw.Item(CO0008INPcol)) Then
                    Select Case CO0008INPcol.ColumnName
                        Case "LINECNT"
                            CO0008INProw.Item(CO0008INPcol) = 0
                        Case "OPERATION"
                            CO0008INProw.Item(CO0008INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            CO0008INProw.Item(CO0008INPcol) = 0
                        Case "SELECT"
                            CO0008INProw.Item(CO0008INPcol) = 1
                        Case "HIDDEN"
                            CO0008INProw.Item(CO0008INPcol) = 0
                        Case "POSIROW", "POSICOL"
                            CO0008INProw.Item(CO0008INPcol) = 0
                        Case Else
                            CO0008INProw.Item(CO0008INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("MAPIDP") >= 0 AndAlso
                WW_COLUMNS.IndexOf("VARIANTP") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TITLEKBN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("POSIROW") >= 0 AndAlso
                WW_COLUMNS.IndexOf("POSICOL") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                For Each CO0008row As DataRow In CO0008tbl.Rows
                    If CS23XLSTBLrow("CAMPCODE") = CO0008row("CAMPCODE") AndAlso
                        CS23XLSTBLrow("MAPIDP") = CO0008row("MAPIDP") AndAlso
                        CS23XLSTBLrow("VARIANTP") = CO0008row("VARIANTP") AndAlso
                        CS23XLSTBLrow("TITLEKBN") = CO0008row("TITLEKBN") AndAlso
                        CS23XLSTBLrow("POSIROW") = CO0008row("POSIROW") AndAlso
                        CS23XLSTBLrow("POSICOL") = CO0008row("POSICOL") AndAlso
                        CS23XLSTBLrow("STYMD") = CO0008row("STYMD") Then
                        CO0008INProw.ItemArray = CO0008row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                CO0008INProw("CAMPCODE") = CS23XLSTBLrow("CAMPCODE")
            End If

            '親画面ID
            If WW_COLUMNS.IndexOf("MAPIDP") >= 0 Then
                CO0008INProw("MAPIDP") = CS23XLSTBLrow("MAPIDP")
            End If

            '親画面変数
            If WW_COLUMNS.IndexOf("VARIANTP") >= 0 Then
                CO0008INProw("VARIANTP") = CS23XLSTBLrow("VARIANTP")
            End If

            'タイトル区分
            If WW_COLUMNS.IndexOf("TITLEKBN") >= 0 Then
                CO0008INProw("TITLEKBN") = CS23XLSTBLrow("TITLEKBN")
            End If

            '行位置
            If WW_COLUMNS.IndexOf("POSIROW") >= 0 Then
                CO0008INProw("POSIROW") = CS23XLSTBLrow("POSIROW")
            End If

            '列位置
            If WW_COLUMNS.IndexOf("POSICOL") >= 0 Then
                CO0008INProw("POSICOL") = CS23XLSTBLrow("POSICOL")
            End If

            '画面ID
            If WW_COLUMNS.IndexOf("MAPID") >= 0 Then
                CO0008INProw("MAPID") = CS23XLSTBLrow("MAPID")
            End If

            '変数
            If WW_COLUMNS.IndexOf("VARIANT") >= 0 Then
                CO0008INProw("VARIANT") = CS23XLSTBLrow("VARIANT")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(CS23XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0008INProw("STYMD") = ""
                    Else
                        CO0008INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0008INProw("STYMD") = ""
                End Try
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(CS23XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0008INProw("ENDYMD") = ""
                    Else
                        CO0008INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                    CO0008INProw("ENDYMD") = ""
                End Try
            End If

            'タイトル名称
            If WW_COLUMNS.IndexOf("TITLENAMES") >= 0 Then
                CO0008INProw("TITLENAMES") = CS23XLSTBLrow("TITLENAMES")
            End If

            '画面名称（短）
            If WW_COLUMNS.IndexOf("MAPNAMES") >= 0 Then
                CO0008INProw("MAPNAMES") = CS23XLSTBLrow("MAPNAMES")
            End If

            '画面名称（長）
            If WW_COLUMNS.IndexOf("MAPNAMEL") >= 0 Then
                CO0008INProw("MAPNAMEL") = CS23XLSTBLrow("MAPNAMEL")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                CO0008INProw("DELFLG") = CS23XLSTBLrow("DELFLG")
            End If

            '○ 名称取得
            CODENAME_get("CAMPCODE", CO0008INProw("CAMPCODE"), CO0008INProw("CAMPNAME"), WW_DUMMY)
            CODENAME_get("MAPID", CO0008INProw("MAPIDP"), CO0008INProw("MAPPNAME"), WW_DUMMY)
            CODENAME_get("VARI", CO0008INProw("MAPIDP") & "_" & CO0008INProw("VARIANTP"), CO0008INProw("VARIPNAME"), WW_DUMMY)

            CO0008INPtbl.Rows.Add(CO0008INProw)
        Next

        '○ 項目チェック
        CO0008INPtbl_CHEK(WW_ERR_SW)

        '○ 入力値のテーブル反映
        CO0008tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(CO0008tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0008S Then

            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            '会社コード表示
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        End If

    End Sub

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub MAPDATAget()

        '○画面表示用データ取得

        'ユーザプロファイル（画面）内容検索
        Try
            '○GridView内容をテーブル退避
            'CO0008テンポラリDB項目作成
            If CO0008tbl Is Nothing Then
                CO0008tbl = New DataTable
            End If

            If CO0008tbl.Columns.Count = 0 Then
            Else
                CO0008tbl.Columns.Clear()
            End If

            '○DB項目クリア
            CO0008tbl.Clear()


            '検索SQL文
            '　検索説明
            '　　Step1：指定された親VARIANT配下のデータを取得
            '　　Step2：メンテナンス可能USERおよびデフォルトUSERのTBL(S0024_PROFMMAP)を取得
            '　　        画面表示は、参照可能および更新ユーザに関連するTBLデータとなるため
            '　　　　　　※データの権限について（参考）　
            '　　　　　　　　権限チャックは、表追加のタイミングで行う。
            '　　　　　　　　（チェック内容）
            '　　　　　　　　①操作USERは、TBL入力データ(USER)の更新権限をもっているか。
            '　　　　　　　　②TBL入力データ(USER)は、TBL入力データ(MAP)の参照および更新権限をもっているか。
            '　　　　　　　　③TBL入力データ(USER)は、TBL入力データ(CAMPCODE)の参照および更新権限をもっているか。
            '　　Step3：関連するグループコードを取得(操作USERに依存)
            '　　Step4：関連する名称を取得(TBL入力データ(USER)に依存)
            '　注意事項　日付について
            '　　権限判断はすべてDateNow。グループコード、名称取得は全てDateNow。表追加時の①はDateNow。
            '　　但し、表追加時の②および③は、TBL入力有効期限。

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " SELECT                                                            " _
                    & "       0                                        as LINECNT       , " _
                    & "       ''                                       as OPERATION     , " _
                    & "       TIMSTP = cast(C.UPDTIMSTP as bigint)                      , " _
                    & "       0                                        as 'SELECT'      , " _
                    & "       0                                        as HIDDEN        , " _
                    & "       rtrim(C.CAMPCODE)                        as CAMPCODE      , " _
                    & "       rtrim(C.MAPIDP)                          as MAPIDP        , " _
                    & "       rtrim(C.VARIANTP)                        as VARIANTP      , " _
                    & "       C.POSIROW                                as POSIROW       , " _
                    & "       rtrim(C.TITLEKBN)                        as TITLEKBN      , " _
                    & "       C.POSICOL                                as POSICOL       , " _
                    & "       rtrim(C.MAPID)                           as MAPID         , " _
                    & "       rtrim(C.VARIANT)                         as VARIANT       , " _
                    & "       format(C.STYMD, 'yyyy/MM/dd')            as STYMD         , " _
                    & "       format(C.ENDYMD, 'yyyy/MM/dd')           as ENDYMD        , " _
                    & "       rtrim(T.NAMES)                           as CAMPNAME      , " _
                    & "       rtrim(C.TITLENAMES)                      as TITLENAMES    , " _
                    & "       rtrim(C.MAPNAMES)                        as MAPNAMES      , " _
                    & "       rtrim(C.MAPNAMEL)                        as MAPNAMEL      , " _
                    & "       rtrim(C.DELFLG)                          as DELFLG        , " _
                    & "       rtrim(E.NAMES)                           as MAPPNAME      , " _
                    & "       rtrim(G.VARIANTNAMES)                    as VARIPNAME     , " _
                    & "       rtrim(F.NAMES)                           as MAPNAME       , " _
                    & "       rtrim(H.VARIANTNAMES)                    as VARINAME        " _
                    & " FROM      S0024_PROFMMAP                         C                " _
                    & " LEFT JOIN M0001_CAMP T                                            " _
                    & "   ON  T.CAMPCODE    = C.CAMPCODE                                  " _
                    & "   and T.STYMD   <= @P4                                            " _
                    & "   and T.ENDYMD  >= @P4                                            " _
                    & "   and T.DELFLG  <> '1'                                            " _
                    & " LEFT JOIN S0009_URL                              E                " _
                    & "   ON  E.MAPID    = C.MAPIDP                                       " _
                    & "   and E.STYMD   <= @P4                                            " _
                    & "   and E.ENDYMD  >= @P4                                            " _
                    & "   and E.DELFLG  <> '1'                                            " _
                    & " LEFT JOIN S0009_URL F                                             " _
                    & "   ON  F.MAPID    = C.MAPID                                        " _
                    & "   and F.STYMD   <= @P4                                            " _
                    & "   and F.ENDYMD  >= @P4                                            " _
                    & "   and F.DELFLG  <> '1'                                            " _
                    & " LEFT JOIN S0023_PROFMVARI                        G                " _
                    & "   ON  G.CAMPCODE = C.CAMPCODE " _
                    & "   and G.MAPID    = C.MAPIDP " _
                    & "   and G.VARIANT  = C.VARIANTP " _
                    & "   and G.TITLEKBN = 'H' " _
                    & "   and G.STYMD   <= @P4 " _
                    & "   and G.ENDYMD  >= @P4 " _
                    & "   and G.DELFLG  <> '1' " _
                    & " LEFT JOIN S0023_PROFMVARI H " _
                    & "   ON  H.CAMPCODE   = C.CAMPCODE " _
                    & "   and H.MAPID    = C.MAPID " _
                    & "   and H.VARIANT  = C.VARIANT " _
                    & "   and H.TITLEKBN = 'H' " _
                    & "   and H.STYMD   <= @P4 " _
                    & "   and H.ENDYMD  >= @P4 " _
                    & "   and H.DELFLG  <> '1' " _
                    & " WHERE  " _
                    & "        C.CAMPCODE = @P1 " _
                    & "   and  C.STYMD   <= @P2 " _
                    & "   and  C.ENDYMD  >= @P3 " _
                    & "   and  C.DELFLG  <> '1' " _
                    & "ORDER BY C.CAMPCODE , C.MAPIDP , C.VARIANTP , C.POSIROW , C.POSICOL  "

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
                            CO0008tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        'CO0008tbl値設定
                        While SQLdr.Read

                            '○抽出条件対象か判断
                            '選択画面情報によりデータ抽出

                            '○親画面(MAPIDP_G)
                            Dim WW_SELECT_MAPIDP_G As Integer = 0    '0:対象外、1:対象

                            '○子画面(MAPID_G)
                            Dim WW_SELECT_MAPID_G As Integer = 0    '0:対象外、1:対象

                            'MAPIDP(From-To)
                            If work.WF_SEL_MAPIDPF.Text = "" AndAlso
                               work.WF_SEL_MAPIDPT.Text = "" Then
                                WW_SELECT_MAPIDP_G = 1
                            Else
                                If SQLdr("MAPIDP") >= work.WF_SEL_MAPIDPF.Text AndAlso
                                   SQLdr("MAPIDP") <= work.WF_SEL_MAPIDPT.Text Then
                                    WW_SELECT_MAPIDP_G = 1
                                End If
                            End If

                            'MAPID(From-To)
                            If work.WF_SEL_MAPIDF.Text = "" AndAlso
                               work.WF_SEL_MAPIDT.Text = "" Then
                                WW_SELECT_MAPID_G = 1
                            Else
                                If SQLdr("MAPID") >= work.WF_SEL_MAPIDF.Text AndAlso
                                   SQLdr("MAPID") <= work.WF_SEL_MAPIDT.Text Then
                                    WW_SELECT_MAPID_G = 1
                                End If
                            End If


                            Dim CO0008row As DataRow = CO0008tbl.NewRow()
                            '○共通項目
                            'LINECNT
                            CO0008row("LINECNT") = SQLdr("LINECNT")
                            'OPERATION
                            CO0008row("OPERATION") = SQLdr("OPERATION")
                            'TIMSTP
                            CO0008row("TIMSTP") = SQLdr("TIMSTP")
                            If WW_SELECT_MAPIDP_G = 1 AndAlso WW_SELECT_MAPID_G = 1 Then
                                CO0008row("SELECT") = 1
                            Else
                                CO0008row("SELECT") = 0
                            End If
                            If SQLdr("TITLEKBN") = "H" Then
                                CO0008row("HIDDEN") = 0
                            Else
                                CO0008row("HIDDEN") = 1
                            End If

                            '○画面固有項目
                            'CAMPCODE
                            CO0008row("CAMPCODE") = SQLdr("CAMPCODE")
                            'MAPIDP
                            CO0008row("MAPIDP") = SQLdr("MAPIDP")
                            'VARIANTP
                            CO0008row("VARIANTP") = SQLdr("VARIANTP")
                            'TITLEKBN
                            CO0008row("TITLEKBN") = SQLdr("TITLEKBN")
                            'POSICOL
                            CO0008row("POSICOL") = SQLdr("POSICOL")
                            'POSIROW
                            CO0008row("POSIROW") = SQLdr("POSIROW")
                            'MAPID
                            CO0008row("MAPID") = SQLdr("MAPID")
                            'VARIANT
                            CO0008row("VARIANT") = SQLdr("VARIANT")
                            'STYMD
                            CO0008row("STYMD") = CDate(SQLdr("STYMD")).ToString("yyyy/MM/dd")
                            'ENDYMD
                            CO0008row("ENDYMD") = CDate(SQLdr("ENDYMD")).ToString("yyyy/MM/dd")
                            'CAMPNAME
                            CO0008row("CAMPNAME") = SQLdr("CAMPNAME")
                            'TITLENAMES
                            CO0008row("TITLENAMES") = SQLdr("TITLENAMES")
                            'MAPNAMES
                            CO0008row("MAPNAMES") = SQLdr("MAPNAMES")
                            'MAPNAMEL
                            CO0008row("MAPNAMEL") = SQLdr("MAPNAMEL")
                            'DELFLG
                            CO0008row("DELFLG") = SQLdr("DELFLG")
                            'MAPPNAME
                            CO0008row("MAPPNAME") = SQLdr("MAPPNAME")
                            'VARIPNAME
                            CO0008row("VARIPNAME") = SQLdr("VARIPNAME")
                            'MAPNAME
                            CO0008row("MAPNAME") = SQLdr("MAPNAME")
                            'VARINAME
                            CO0008row("VARINAME") = SQLdr("VARINAME")

                            '抽出対象外の場合、レコード追加しない
                            If CO0008row("SELECT") = 1 Then
                                CO0008tbl.Rows.Add(CO0008row)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0024_PROFMMAP SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0024_PROFMMAP Select"
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
        CS0026TBLSORT.TABLE = CO0008tbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = "TITLEKBN = 'H' and SELECT = 1"
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            CO0008tbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' 入力チェック
    ''' </summary>
    ''' <param name="O_RTNCODE">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CO0008INPtbl_CHEK(ByRef O_RTNCODE As String)

        '○初期処理
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○CO0008INPのKEY重複 --> 先頭レコードを優先
        Dim WW_Cnt1 As Integer = 0
        Dim WW_Cnt2 As Integer = 0
        Dim WW_Position As Integer = 0

        '○事前準備（キー重複レコード削除）
        'Deleteにより行カウントがずれるのでDoで回す(ドロップデータ対応)
        WW_Cnt1 = 0
        WW_Cnt2 = 0
        Do Until WW_Cnt1 > (CO0008INPtbl.Rows.Count - 1)
            WW_Cnt2 = WW_Cnt1 + 1
            Do Until WW_Cnt2 > (CO0008INPtbl.Rows.Count - 1)

                'KEY重複データを探す
                If If(CO0008INPtbl.Rows(WW_Cnt1)("CAMPCODE"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("CAMPCODE"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("MAPIDP"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("MAPIDP"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("VARIANTP"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("VARIANTP"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("TITLEKBN"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("TITLEKBN"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("POSICOL"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("POSICOL"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("POSIROW"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("POSIROW"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("MAPID"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("MAPID"), "") AndAlso
                   If(CO0008INPtbl.Rows(WW_Cnt1)("VARIANT"), "") = If(CO0008INPtbl.Rows(WW_Cnt2)("VARIANT"), "") AndAlso
                   If(IsDBNull(CO0008INPtbl.Rows(WW_Cnt1)("STYMD")), "", CO0008INPtbl.Rows(WW_Cnt1)("STYMD")) =
                     If(IsDBNull(CO0008INPtbl.Rows(WW_Cnt2)("STYMD")), "", CO0008INPtbl.Rows(WW_Cnt2)("STYMD")) AndAlso
                   If(IsDBNull(CO0008INPtbl.Rows(WW_Cnt1)("ENDYMD")), "", CO0008INPtbl.Rows(WW_Cnt1)("ENDYMD")) =
                     If(IsDBNull(CO0008INPtbl.Rows(WW_Cnt2)("ENDYMD")), "", CO0008INPtbl.Rows(WW_Cnt2)("ENDYMD")) Then
                    CO0008INPtbl.Rows(WW_Cnt2).Delete()
                Else
                    WW_Cnt2 = WW_Cnt2 + 1
                End If
            Loop
            WW_Cnt1 = WW_Cnt1 + 1
        Loop

        '○チェック

        'タイトル区分存在チェック(Hレコード)　…　Hレコードが無ければエラー
        For i As Integer = 0 To CO0008INPtbl.Rows.Count - 1
            If CO0008INPtbl.Rows(i)("TITLEKBN") = "H" Then
                Exit For
            End If
            If i >= (CO0008INPtbl.Rows.Count - 1) AndAlso CO0008INPtbl.Rows(i)("TITLEKBN") <> "H" Then
                WW_CheckMES1 = "・更新できないレコード(Default タイトル区分(H)なし)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INPtbl.Rows(i))
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
                Exit Sub
            End If
        Next

        'タイトル区分存在チェック(Iレコード)　…　Iレコードが無ければエラー
        For i As Integer = 0 To CO0008INPtbl.Rows.Count - 1
            If CO0008INPtbl.Rows(i)("TITLEKBN") = "I" Then
                Exit For
            End If
            If i >= (CO0008INPtbl.Rows.Count - 1) AndAlso
               CO0008INPtbl.Rows(i)("TITLEKBN") <> "I" Then
                WW_CheckMES1 = "・更新できないレコード(Default タイトル区分(I)なし)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INPtbl.Rows(i))
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
                Exit Sub
            End If
        Next

        '○ヘッダー行チェック実行　○
        For Each CO0008INProw As DataRow In CO0008INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0008INProw("TITLEKBN") <> "H" Then
                Continue For
            End If

            '○キー項目(CAMPCODE)
            WW_TEXT = CO0008INProw("CAMPCODE")
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "CAMPCODE", CO0008INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0008INProw("CAMPCODE") = ""
                Else
                    '存在チェック(LeftBox存在しない場合エラー)
                    CODENAME_get("CAMPCODE", CO0008INProw("CAMPCODE"), CO0008INProw("CAMPNAME"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(会社コード未登録)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○キー項目(MAPIDP)
            WW_TEXT = CO0008INProw("MAPIDP")
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "MAPIDP", CO0008INProw("MAPIDP"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0008INProw("MAPIDP") = ""
                Else
                    '存在チェック(LeftBox存在しない場合エラー)
                    CODENAME_get("MAPID", CO0008INProw("MAPIDP"), CO0008INProw("MAPPNAME"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(親画面ID未登録)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(親画面IDエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○キー項目(VARIANTP)
            WW_TEXT = CO0008INProw("VARIANTP")
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "VARIANTP", CO0008INProw("VARIANTP"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    CO0008INProw("VARIANTP") = ""
                Else
                    '存在チェック(LeftBox存在しない場合エラー)
                    CODENAME_get("VARI", CO0008INProw("MAPIDP") & "_" & CO0008INProw("VARIANTP"), CO0008INProw("VARIPNAME"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(親変数未登録)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(親変数エラー)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○キー項目(STYMD)
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "STYMD", CO0008INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○キー項目(ENDYMD)
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "ENDYMD", CO0008INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(有効日付エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○キー項目(DELFLG)
            Master.checkFIeld(CO0008INProw("CAMPCODE"), "DELFLG", CO0008INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", CO0008INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除CD未登録)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            '○権限チェック
            '権限チェック(データ内(USER、画面)の更新権限チェック)
            CS0025AUTHORget.USERID = CS0050Session.USERID
            CS0025AUTHORget.OBJCODE = "MAP"
            CS0025AUTHORget.CODE = CO0008INProw("MAPIDP")
            CS0025AUTHORget.STYMD = Date.Now
            CS0025AUTHORget.ENDYMD = Date.Now
            CS0025AUTHORget.CS0025AUTHORget()
            If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
            Else
                WW_CheckMES1 = "・更新できないレコード(ユーザに親画面操作権限なし)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_LINE_ERR = "ERR"
            End If

            If WW_LINE_ERR = "" Then
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '明細行チェック
        For Each CO0008INProw As DataRow In CO0008INPtbl.Rows

            WW_LINE_ERR = ""

            If CO0008INProw("TITLEKBN") <> "I" Then
                Continue For
            End If


            If String.IsNullOrEmpty(CO0008INProw("MAPID")) AndAlso
               String.IsNullOrEmpty(CO0008INProw("TITLENAMES")) Then
                '空行は処理しない　…　INPtbl作成時、Hレコード+Iレコード(右Max21件)+Iレコード(左Max21件)　　　未使用レコードを含む
                CO0008INProw("MAPID") = ""
                CO0008INProw("VARIANT") = ""
                CO0008INProw("TITLENAMES") = ""
                CO0008INProw("MAPNAMES") = ""
                CO0008INProw("MAPNAMEL") = ""
                CO0008INProw("MAPID") = ""
                CO0008INProw("MAPNAME") = ""
                CO0008INProw("VARINAME") = ""
            Else
                '○一般項目(TITLENAMES、MAPNAMES)
                '①必須チェック
                If CO0008INProw("TITLENAMES") = "" AndAlso CO0008INProw("MAPNAMES") = "" Then
                    WW_CheckMES1 = "・エラー(見出名称・ボタン名称のどちらかに入力が必要)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                If CO0008INProw("TITLENAMES") <> "" AndAlso CO0008INProw("MAPNAMES") <> "" Then
                    WW_CheckMES1 = "・エラー(見出名称・ボタン名称の片方にのみ入力が可能)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                '○キー項目(MAPID)
                '①必須チェック
                If CO0008INProw("TITLENAMES") <> "" AndAlso CO0008INProw("MAPID") <> "" Then
                    WW_CheckMES1 = "・エラー(見出しに画面IDが指定されています)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                If CO0008INProw("MAPNAMES") <> "" AndAlso CO0008INProw("MAPID") = "" Then
                    WW_CheckMES1 = "・エラー(画面ID未入力)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                '存在チェック(LeftBox存在しない場合エラー)
                If CO0008INProw("MAPID") <> "" Then
                    CO0008INProw("MAPNAME") = ""
                    CODENAME_get("MAPID", CO0008INProw("MAPID"), CO0008INProw("MAPNAME"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラー(画面ID不正)が存在します。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If

                '③権限チェック(データ内USERが画面更新権限があるかチェック)
                '　※権限判定時点：データの有効日付
                If CO0008INProw("MAPID") <> "" Then
                    CS0025AUTHORget.USERID = CS0050Session.USERID
                    CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
                    CS0025AUTHORget.CODE = CO0008INProw("MAPID")
                    CS0025AUTHORget.STYMD = Date.Now
                    CS0025AUTHORget.ENDYMD = Date.Now
                    CS0025AUTHORget.CS0025AUTHORget()
                    If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                    Else
                        WW_CheckMES1 = "・更新できないレコード(ユーザに親画面操作権限なし)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If

                '○キー項目(VARIANTP)
                '①必須チェック
                If CO0008INProw("TITLENAMES") <> "" AndAlso CO0008INProw("VARIANT") <> "" Then
                    WW_CheckMES1 = "・エラー(見出しに変数IDが指定されています)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                If CO0008INProw("MAPID") <> "" AndAlso CO0008INProw("VARIANT") = "" Then
                    WW_CheckMES1 = "・エラー(変数ID未入力)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                End If

                '②存在チェック(LeftBox存在しない場合エラー)
                If CO0008INProw("VARIANT") <> "" AndAlso
                    CO0008INProw("VARIANT") <> C_DEFAULT_DATAKEY Then
                    CO0008INProw("VARINAME") = ""
                    CODENAME_get("VARI", CO0008INProw("MAPID") & "_" & CO0008INProw("VARIANT"), CO0008INProw("VARINAME"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・エラー(変数ID不正)が存在します。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008INProw)
                        O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_LINE_ERR = "ERR"
                    End If
                End If
            End If

            If WW_LINE_ERR = "" Then
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' CO0008tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0008tbl_UPD()

        '○ 画面状態設定
        For Each CO0008row As DataRow In CO0008tbl.Rows
            Select Case CO0008row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 事前準備（GridVeiwの該当データを初期化）
        For Each CO0008row As DataRow In CO0008tbl.Rows
            '同一(ENDYMD以外が同一KEY)レコード
            If CO0008INPtbl.Rows(0)("CAMPCODE") = CO0008row("CAMPCODE") AndAlso
               CO0008INPtbl.Rows(0)("MAPIDP") = CO0008row("MAPIDP") AndAlso
               CO0008INPtbl.Rows(0)("VARIANTP") = CO0008row("VARIANTP") AndAlso
               If(IsDBNull(CO0008INPtbl.Rows(0)("STYMD")), Date.MinValue, CO0008INPtbl.Rows(0)("STYMD")) =
               If(IsDBNull(CO0008row("STYMD")), Date.MinValue, CO0008row("STYMD")) Then
                CO0008row("MAPID") = ""
                CO0008row("MAPNAME") = ""
                CO0008row("VARIANT") = ""
                CO0008row("VARINAME") = ""
                CO0008row("TITLENAMES") = ""
                CO0008row("MAPNAMES") = ""
                CO0008row("MAPNAMEL") = ""
                CO0008row("DELFLG") = C_DELETE_FLG.DELETE
            End If
        Next

        '○ 追加変更判定
        Dim WW_UPDAT As Integer = 0
        For Each CO0008INProw As DataRow In CO0008INPtbl.Rows

            'エラーレコード読み飛ばし
            If CO0008INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            CO0008INProw("OPERATION") = "Insert"

            'KEY項目が等しい(ENDYMD以外のKEYが同じ)
            For Each CO0008row As DataRow In CO0008tbl.Rows
                If CO0008row("CAMPCODE") = CO0008INProw("CAMPCODE") AndAlso
                    CO0008row("MAPIDP") = CO0008INProw("MAPIDP") AndAlso
                    CO0008row("VARIANTP") = CO0008INProw("VARIANTP") AndAlso
                    CO0008row("TITLEKBN") = CO0008INProw("TITLEKBN") AndAlso
                    CO0008row("POSIROW") = CO0008INProw("POSIROW") AndAlso
                    CO0008row("POSICOL") = CO0008INProw("POSICOL") AndAlso
                    CO0008row("STYMD") = CO0008INProw("STYMD") Then

                    '変更無は操作無
                    If CO0008row("MAPID") = CO0008INProw("MAPID") AndAlso
                        CO0008row("VARIANT") = CO0008INProw("VARIANT") AndAlso
                        CO0008row("ENDYMD") = CO0008INProw("ENDYMD") AndAlso
                        CO0008row("TITLENAMES") = CO0008INProw("TITLENAMES") AndAlso
                        CO0008row("MAPNAMES") = CO0008INProw("MAPNAMES") AndAlso
                        CO0008row("MAPNAMEL") = CO0008INProw("MAPNAMEL") AndAlso
                        CO0008row("DELFLG") = CO0008INProw("DELFLG") Then
                        CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        WW_UPDAT = WW_UPDAT - 1
                        Exit For
                    End If

                    CO0008INProw("OPERATION") = "Update"
                    WW_UPDAT = WW_UPDAT + 1
                    Exit For
                End If
            Next
        Next

        '○ 変更レコードが存在する場合、ヘッダー区分も更新対象にする
        If WW_UPDAT > 0 Then
            For Each CO0008INProw As DataRow In CO0008INPtbl.Rows
                If CO0008INProw("TITLEKBN") = "H" Then
                    CO0008INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        End If

        '○ 変更有無判定　&　入力値反映
        For Each CO0008INProw As DataRow In CO0008INPtbl.Rows
            Select Case CO0008INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(CO0008INProw)
                Case "Insert"
                    TBL_INSERT_SUB(CO0008INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="CO0008INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef CO0008INProw As DataRow)

        For Each CO0008row As DataRow In CO0008tbl.Rows

            '同一(ENDYMD以外が同一KEY)レコード
            If CO0008row("CAMPCODE") = CO0008INProw("CAMPCODE") AndAlso
                CO0008row("MAPIDP") = CO0008INProw("MAPIDP") AndAlso
                CO0008row("VARIANTP") = CO0008INProw("VARIANTP") AndAlso
                CO0008row("TITLEKBN") = CO0008INProw("TITLEKBN") AndAlso
                CO0008row("POSIROW") = CO0008INProw("POSIROW") AndAlso
                CO0008row("POSICOL") = CO0008INProw("POSICOL") AndAlso
                CO0008row("STYMD") = CO0008INProw("STYMD") Then

                '画面入力テーブル項目設定
                CO0008INProw("LINECNT") = CO0008row("LINECNT")
                CO0008INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                CO0008INProw("TIMSTP") = CO0008row("TIMSTP")
                CO0008INProw("SELECT") = 1
                CO0008INProw("HIDDEN") = 0

                '項目テーブル項目設定
                CO0008row.ItemArray = CO0008INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="CO0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef CO0007INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim CO0008row As DataRow = CO0008tbl.NewRow
        CO0008row.ItemArray = CO0007INProw.ItemArray

        '○ 最大項番数を取得
        Dim TBLview As DataView = New DataView(CO0008tbl)
        TBLview.RowFilter = "TITLEKBN = 'H'"

        If CO0007INProw("TITLEKBN") = "H" Then
            CO0008row("LINECNT") = TBLview.Count + 1
        Else
            CO0008row("LINECNT") = 0
        End If

        CO0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        CO0008row("TIMSTP") = "0"
        CO0008row("SELECT") = 1
        CO0008row("HIDDEN") = 0

        CO0008tbl.Rows.Add(CO0008row)

        TBLview.Dispose()
        TBLview = Nothing

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
        For Each CO0008Row As DataRow In CO0008tbl.Rows
            '読み飛ばし
            If (CO0008Row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0008Row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0008Row("DELFLG") = C_DELETE_FLG.DELETE Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each checkRow As DataRow In CO0008tbl.Rows

                '同一KEY以外は読み飛ばし
                If CO0008Row("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                   CO0008Row("MAPIDP") = checkRow("MAPIDP") AndAlso
                   CO0008Row("VARIANTP") = checkRow("VARIANTP") AndAlso
                   CO0008Row("POSICOL") = checkRow("POSICOL") AndAlso
                   CO0008Row("POSIROW") = checkRow("POSIROW") AndAlso
                   CO0008Row("TITLEKBN") = checkRow("TITLEKBN") Then
                Else
                    Continue For
                End If

                If checkRow("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If
                '期間変更対象は読み飛ばし
                If CO0008Row("STYMD") = checkRow("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0008Row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0008Row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(checkRow("STYMD"), WW_DATE_ST2)
                    Date.TryParse(checkRow("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008Row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008Row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next
            If WW_LINEERR_SW = "" Then
                CO0008Row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0008Row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next
        '○親子関連チェック
        Dim CO0008Hrows As DataRow() = CO0008tbl.Select("TITLEKBN = 'H' and OPERATION='" & C_LIST_OPERATION_CODE.UPDATING & "'")
        For i As Integer = 0 To CO0008Hrows.Count - 1
            WW_LINEERR_SW = ""
            Dim CO0008Hrow As DataRow = CO0008Hrows(i)
            '○子供のレコード群を取得
            Dim CO0008CRows As DataRow() = CO0008tbl.Select("TITLEKBN = 'I' and CAMPCODE='" & CO0008Hrow("CAMPCODE") &
                                                            "' and MAPIDP='" & CO0008Hrow("MAPIDP") &
                                                            "' and VARIANTP='" & CO0008Hrow("VARIANTP") &
                                                            "' and DELFLG<>'" & C_DELETE_FLG.DELETE & "'")
            For j As Integer = 0 To CO0008CRows.Count - 1
                Dim CO0008Drow As DataRow = CO0008CRows(j)

                Try
                    Date.TryParse(CO0008Hrow("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0008Hrow("ENDYMD"), WW_DATE_END)
                    Date.TryParse(CO0008Drow("STYMD"), WW_DATE_ST2)
                    Date.TryParse(CO0008Drow("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try
                '開始日チェック
                If Not (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間対象外)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008Hrow)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If Not (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間対象外)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0008Hrow)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If
            Next j
            If WW_LINEERR_SW = "" Then

            Else
                CO0008Hrow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next i

    End Sub
    ''' <summary>
    ''' WF_Repeater バインド時 編集
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub RptInfo_ItemDataBound_L(ByVal sender As Object, ByVal e As RepeaterItemEventArgs) Handles WF_Repeater.ItemDataBound

        '○WF_Repeaterバインド時 編集
        '○ヘッダー編集
        If e.Item.ItemType = ListItemType.Header Then
        End If

        '○アイテム編集
        If e.Item.ItemType = ListItemType.Item OrElse
            e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim WW_Position As String = ""
            '左右位置
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "POSICOL" Then
                    CType(e.Item.FindControl("WF_Rep_POSICOL"), Label).Text = e.Item.DataItem(i).ToString()
                    WW_Position = e.Item.DataItem(i).ToString()
                    Exit For
                End If
            Next
            '項番
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "POSIROW" Then
                    CType(e.Item.FindControl("WF_Rep_POSIROW"), Label).Text = e.Item.DataItem(i).ToString()
                    WW_Position = WW_Position & Right("0" & e.Item.DataItem(i).ToString(), 2)
                    Exit For
                End If
            Next
            '定義内容
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "MAPID" Then
                    CType(e.Item.FindControl("WF_Rep_SW1"), RadioButton).Attributes.Add("Onclick", "Rep_ButtonChange('WF_Rep_SW1','" & WW_Position & "');")
                    CType(e.Item.FindControl("WF_Rep_SW2"), RadioButton).Attributes.Add("Onclick", "Rep_ButtonChange('WF_Rep_SW2','" & WW_Position & "');")
                    If e.Item.DataItem(i).ToString() = "" Then
                        CType(e.Item.FindControl("WF_Rep_SW1"), RadioButton).Checked = True
                        CType(e.Item.FindControl("WF_Rep_TITLE"), TextBox).Visible = True
                        CType(e.Item.FindControl("WF_Rep_NAMES"), TextBox).Visible = False
                        CType(e.Item.FindControl("WF_Rep_MAPID"), TextBox).Visible = False
                        CType(e.Item.FindControl("WF_Rep_MAPID_TEXT"), Label).Visible = False
                        CType(e.Item.FindControl("WF_Rep_VARIANT"), TextBox).Visible = False
                        CType(e.Item.FindControl("WF_Rep_VARIANT_TEXT"), Label).Visible = False
                    Else
                        CType(e.Item.FindControl("WF_Rep_SW2"), RadioButton).Checked = True
                        CType(e.Item.FindControl("WF_Rep_TITLE"), TextBox).Visible = False
                        CType(e.Item.FindControl("WF_Rep_NAMES"), TextBox).Visible = True
                        CType(e.Item.FindControl("WF_Rep_MAPID"), TextBox).Visible = True
                        CType(e.Item.FindControl("WF_Rep_MAPID_TEXT"), Label).Visible = True
                        CType(e.Item.FindControl("WF_Rep_VARIANT"), TextBox).Visible = True
                        CType(e.Item.FindControl("WF_Rep_VARIANT_TEXT"), Label).Visible = True
                    End If
                    Exit For
                End If
            Next

            '見出名称
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "TITLENAMES" Then
                    CType(e.Item.FindControl("WF_Rep_TITLE"), TextBox).Text = e.Item.DataItem(i).ToString()
                    Exit For
                End If
            Next
            'ボタン名称
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "MAPNAMES" Then
                    CType(e.Item.FindControl("WF_Rep_NAMES"), TextBox).Text = e.Item.DataItem(i).ToString()
                    Exit For
                End If
            Next
            '画面ＩＤ
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "MAPID" Then
                    CType(e.Item.FindControl("WF_Rep_MAPID"), TextBox).Text = e.Item.DataItem(i).ToString()
                    CType(e.Item.FindControl("WF_Rep_MAPID"), TextBox).Attributes.Add("ondblclick", "Repeater_Lines('" & WW_Position & "');Field_DBclick('WF_Rep_MAPID', " & LIST_BOX_CLASSIFICATION.LC_URL & ");")
                    Exit For
                End If
            Next
            '画面名称
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "MAPNAME" Then
                    CType(e.Item.FindControl("WF_Rep_MAPID_TEXT"), Label).Text = e.Item.DataItem(i).ToString()
                    Exit For
                End If
            Next
            '変数
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "VARIANT" Then
                    CType(e.Item.FindControl("WF_Rep_VARIANT"), TextBox).Text = e.Item.DataItem(i).ToString()
                    CType(e.Item.FindControl("WF_Rep_VARIANT"), TextBox).Attributes.Add("ondblclick", "Repeater_Lines('" & WW_Position & "');Field_DBclick('WF_Rep_VARIANT', " & LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST & ");")
                    Exit For
                End If
            Next
            '変数名称
            For i As Integer = 0 To e.Item.DataItem.table.columns.count - 1
                If e.Item.DataItem.table.columns(i).ToString() = "VARINAME" Then
                    CType(e.Item.FindControl("WF_Rep_VARIANT_TEXT"), Label).Text = e.Item.DataItem(i).ToString()
                    Exit For
                End If
            Next
        End If

        '○フッター編集
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ''' <summary>
    ''' Repeater ラジオボタン 処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_REP_RADIO_Click()

        '○Repeater ラジオボタンによる入力保護/解除
        Try
            For Each reitem As RepeaterItem In WF_Repeater.Items

                '○Repeater明細の操作行を判定する
                If Left(WF_REP_POSITION.Value, 1) = CType(reitem.FindControl("WF_Rep_POSICOL"), Label).Text Then
                    If Right(WF_REP_POSITION.Value, 2) = Right("0" & CType(reitem.FindControl("WF_Rep_POSIROW"), Label).Text, 2) Then
                        If WF_REP_SW.Value = "WF_Rep_SW1" Then
                            CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Visible = True
                            CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Visible = False
                            CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Visible = False
                            CType(reitem.FindControl("WF_Rep_MAPID_TEXT"), Label).Visible = False
                            CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Visible = False
                            CType(reitem.FindControl("WF_Rep_VARIANT_TEXT"), Label).Visible = False
                        Else
                            CType(reitem.FindControl("WF_Rep_TITLE"), TextBox).Visible = False
                            CType(reitem.FindControl("WF_Rep_NAMES"), TextBox).Visible = True
                            CType(reitem.FindControl("WF_Rep_MAPID"), TextBox).Visible = True
                            CType(reitem.FindControl("WF_Rep_MAPID_TEXT"), Label).Visible = True
                            CType(reitem.FindControl("WF_Rep_VARIANT"), TextBox).Visible = True
                            CType(reitem.FindControl("WF_Rep_VARIANT_TEXT"), Label).Visible = True
                        End If
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.CAST_FORMAT_ERROR_EX, C_MESSAGE_TYPE.ABORT, "Detail set")
            CS0011LOGWRITE.INFSUBCLASS = "WF_REP_RADIO_Click"           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Radio action"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.CAST_FORMAT_ERROR_EX
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************


    ' ***  LeftBoxより名称取得＆チェック
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得

        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        Select Case I_FIELD

            Case "CAMPCODE"
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)

            Case "MAPID"
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_URL, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))                      '画面ID

            Case "VARI"
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.CreateVariantList(CS0050Session.getConnection, Master.PROF_VIEW))                     '変数

            Case "POSI"
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LRPOSI"))                  '位置

            Case "DELFLG"
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text))

            Case Else
                O_TEXT = ""
                O_RTN = "OK"

        End Select

    End Sub

    ''' <summary>
    ''' エラーレポート出力
    ''' </summary>
    ''' <param name="I_MESSAGE1"></param>
    ''' <param name="I_MESSAGE2"></param>
    ''' <param name="I_ERRCD"></param>
    ''' <param name="CO0008INProw"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal CO0008INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> CAMPCODE=" & CO0008INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> MAPIDP=" & CO0008INProw("MAPIDP") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> VARIANTP=" & CO0008INProw("VARIANTP") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TITLEKBN=" & CO0008INProw("TITLEKBN") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> POSICOL=" & CO0008INProw("POSICOL") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> POSIROW=" & CO0008INProw("POSIROW") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> STYMD=" & CO0008INProw("STYMD") & " "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ENDYMD=" & CO0008INProw("ENDYMD") & " "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> DELFLG=" & CO0008INProw("DELFLG") & " "
        rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

    End Sub

End Class

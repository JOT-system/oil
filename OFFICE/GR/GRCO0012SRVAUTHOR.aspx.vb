Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' サーバ権限マスタ（実行）
''' </summary>
''' <remarks>
''' </remarks>
Public Class GRCO0012SRVAUTHOR
    Inherits Page

    '共通宣言
    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013ProfView As New CS0013ProfView                'プロファイル（GridView）設定
    Private CS0020JOURNAL As New CS0020JOURNAL                  '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD              'XLSアップロード
    Private CS0026TBLSORT As New CS0026TBLSORT                  'GridView用テーブルソート文字列取得
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力
    Private CS0050Session As New CS0050SESSION                  'セッション情報操作処理

    '検索結果格納ds
    Private CO0012tbl As DataTable                              'Grid格納用テーブル
    Private CO0012INPtbl As DataTable                           'チェック用テーブル
    Private CO0012UPDtbl As DataTable                           'デフォルト用テーブル
    Private S0012_SRVAUTHORtbl As DataTable                     '更新用テーブル

    Private WW_ERRCODE As String = String.Empty                 'リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    Dim WW_ERRLIST_ALL As List(Of String)                       'インポート全体のエラー
    Dim WW_ERRLIST As List(Of String)                           'インポート中の１セット分のエラー

    Private Const CONST_DSPROWCOUNT As Integer = 30             '１画面表示対象
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
                '■■■ 各ボタン押下処理 ■■■
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Not Master.RecoverTable(CO0012tbl) Then Exit Sub

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
                        Case "WF_EXCEL_UPLOAD"
                            UPLOAD_EXCEL()
                        Case Else
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○初期化処理
                Initialize()
            End If
            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

        Finally
            '○Close
            If Not IsNothing(CO0012tbl) Then
                CO0012tbl.Clear()
                CO0012tbl.Dispose()
                CO0012tbl = Nothing
            End If
            If Not IsNothing(CO0012INPtbl) Then
                CO0012INPtbl.Clear()
                CO0012INPtbl.Dispose()
                CO0012INPtbl = Nothing
            End If
            If Not IsNothing(CO0012UPDtbl) Then
                CO0012UPDtbl.Clear()
                CO0012UPDtbl.Dispose()
                CO0012UPDtbl = Nothing
            End If
            If Not IsNothing(S0012_SRVAUTHORtbl) Then
                S0012_SRVAUTHORtbl.Clear()
                S0012_SRVAUTHORtbl.Dispose()
                S0012_SRVAUTHORtbl = Nothing
            End If
        End Try

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○初期値設定
        rightview.resetindex()
        leftview.activeListBox()
        '○ 条件抽出画面情報退避
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
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(CO0012tbl) Then Exit Sub

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(CO0012tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRCO0012WRKINC.MAPID
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
        For Each CO0012row As DataRow In CO0012tbl.Rows
            If CO0012row("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                CO0012row("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(CO0012tbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRCO0012WRKINC.MAPID
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
        WF_TERMID.Focus()

    End Sub
    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each CO0012row As DataRow In CO0012tbl.Rows
            '一度全部非表示化する
            CO0012row("HIDDEN") = 1

            '端末ID、オブジェクト絞込判定
            If WF_SELTERM.Text <> "" AndAlso WF_SELOBJECT.Text <> "" Then
                Dim WW_STRING1 As String = CO0012row("TERMID")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = CO0012row("OBJECT")    '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_SELTERM.Text) AndAlso WW_STRING2.Contains(WF_SELOBJECT.Text) Then
                    CO0012row("HIDDEN") = 0
                End If
            ElseIf WF_SELTERM.Text <> "" Then
                Dim WW_STRING As String = CO0012row("TERMID")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_SELTERM.Text) Then
                    CO0012row("HIDDEN") = 0
                End If
            ElseIf WF_SELOBJECT.Text <> "" Then
                Dim WW_STRING As String = CO0012row("OBJECT")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_SELOBJECT.Text) Then
                    CO0012row("HIDDEN") = 0
                End If
            Else
                '両方未設定の場合、押し並べて表示
                CO0012row("HIDDEN") = 0
            End If
        Next

        '○GridView再表示
        WF_GridPosition.Text = "1"
        '○画面表示データ保存
        Master.SaveTable(CO0012tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELTERM.Focus()

    End Sub
    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        Try
            'メッセージ初期化
            rightview.setErrorReport("")

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '○DB更新前チェック
                '  ※同一Key全てのレコードが更新されていない事をチェックする
                rightview.setErrorReport("")

                '同一(ENDYMD以外)Keyレコードを抽出
                Dim SQLStr0 As String =
                      " SELECT rtrim(TERMID)             as TERMID    , " _
                    & "        rtrim(OBJECT)             as OBJECT    , " _
                    & "        rtrim(CAMPCODE)           as CAMPCODE  , " _
                    & "        rtrim(ROLE)               as ROLE      , " _
                    & "        rtrim(ROLENAMES)          as CODENAMES , " _
                    & "        rtrim(ROLENAMEL)          as CODENAMEL , " _
                    & "        STYMD                     as STYMD     , " _
                    & "        ENDYMD                    as ENDYMD    , " _
                    & "        CAST(UPDTIMSTP as bigint) as TIMSTP      " _
                    & " FROM S0012_SRVAUTHOR                            " _
                    & " WHERE    TERMID       = @P01                    " _
                    & "   and    OBJECT       = @P02                    " _
                    & "   and    CAMPCODE     = @P03                    " _
                    & "   and    ROLE         = @P04                    " _
                    & "   and    STYMD        = @P05                  ; "

                Using SQLcmd As New SqlCommand(SQLStr0, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 50)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 50)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.Date)

                    For Each CO0012row As DataRow In CO0012tbl.Rows
                        If CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso CO0012row("TIMSTP") <> "0" Then
                            '※追加レコードは、CO0012row("TIMSTP") = "0"となっている

                            PARA01.Value = CO0012row("TERMID")
                            PARA02.Value = CO0012row("OBJECT")
                            PARA03.Value = CO0012row("CAMPCODE")
                            PARA04.Value = CO0012row("ROLE")
                            PARA05.Value = RTrim(CO0012row("STYMD"))

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                                While SQLdr.Read
                                    If RTrim(CO0012row("TIMSTP")) <> SQLdr("TIMSTP") Then
                                        For Each CO0012tim As DataRow In CO0012tbl.Rows
                                            If CO0012tim("TERMID") = CO0012row("TERMID") AndAlso
                                                CO0012tim("OBJECT") = CO0012row("OBJECT") AndAlso
                                                CO0012tim("CAMPCODE") = CO0012row("CAMPCODE") AndAlso
                                                CO0012tim("ROLE") = CO0012row("ROLE") AndAlso
                                                CO0012tim("STYMD") = CO0012row("STYMD") Then

                                                CO0012tim("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                            End If
                                        Next

                                        'エラーレポート編集
                                        Dim WW_ERR_MES As String = ""
                                        rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)

                                        WW_ERR_MES = "・更新出来ないレコードが発生しました(既に他端末で更新済み)。"
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TERMID=" & CO0012row("TERMID") & " , "
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> CAMPCODE=" & CO0012row("CAMPCODE") & " , "
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> OBJECT=" & CO0012row("OBJECT") & " , "
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ROLE=" & CO0012row("ROLE") & " , "
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> STYMD=" & CO0012row("STYMD") & " , "
                                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ENDYMD=" & CO0012row("ENDYMD") & " "
                                        rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                                    End If
                                End While
                            End Using
                        End If
                    Next
                End Using

                'ＤＢ更新
                Dim SQLStr As String =
                      " DECLARE @hensuu as bigint ; " _
                    & " set @hensuu = 0 ; " _
                    & " DECLARE hensuu CURSOR FOR " _
                    & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu " _
                    & "     FROM S0012_SRVAUTHOR " _
                    & "     WHERE TERMID = @P1 and CAMPCODE = @P2 " _
                    & "           and OBJECT = @P3 and ROLE = @P4 and STYMD = @P5 ; " _
                    & " OPEN hensuu ; " _
                    & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                    & " IF ( @@FETCH_STATUS = 0 ) " _
                    & "    UPDATE S0012_SRVAUTHOR " _
                    & "       SET ENDYMD = @P6 , ROLENAMES = @P7 , ROLENAMEL = @P8 , DELFLG = @P9 , " _
                    & "           UPDYMD = @P10 , UPDUSER = @P11 , UPDTERMID = @P12 , RECEIVEYMD = @P13" _
                    & "     WHERE TERMID = @P1 and CAMPCODE = @P2 " _
                    & "           and OBJECT = @P3 and ROLE = @P4 and STYMD = @P5 ; " _
                    & " IF ( @@FETCH_STATUS <> 0 ) " _
                    & "    INSERT INTO S0012_SRVAUTHOR " _
                    & "       (TERMID , CAMPCODE , OBJECT , ROLE, SEQ, STYMD , ENDYMD , ROLENAMES , ROLENAMEL , " _
                    & "        DELFLG , INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD) " _
                    & "        VALUES (@P1,@P2,@P3,@P4,1,@P5,@P6,@P7,@P8,@P9,@P10,@P10,@P11,@P12,@P13) ; " _
                    & " CLOSE hensuu ; " _
                    & " DEALLOCATE hensuu ; "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.Date)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar)
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar)
                    Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar)
                    Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
                    Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.DateTime)

                    For Each CO0012row As DataRow In CO0012tbl.Rows
                        If CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING OrElse
                           CO0012row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                            '※追加レコードは、CO0012row("TIMSTP") = "0"となっているが状態のみで判定

                            'INSERT時ブランクが出力される為、MIDにて桁数設定する。
                            PARA1.Value = CO0012row("TERMID")
                            PARA2.Value = CO0012row("CAMPCODE")
                            PARA3.Value = CO0012row("OBJECT")
                            PARA4.Value = CO0012row("ROLE")
                            PARA5.Value = CO0012row("STYMD")
                            PARA6.Value = CO0012row("ENDYMD")
                            PARA7.Value = CO0012row("CODENAMES")
                            PARA8.Value = CO0012row("CODENAMEL")
                            PARA9.Value = CO0012row("DELFLG")
                            PARA10.Value = Date.Now
                            PARA11.Value = Master.USERID
                            PARA12.Value = Master.USERTERMID
                            PARA13.Value = C_DEFAULT_YMD

                            SQLcmd.ExecuteNonQuery()

                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                            Try
                                S0012_SRVAUTHORtbl_ColumnsAdd()
                                Dim CO0012_JNLRow As DataRow = S0012_SRVAUTHORtbl.NewRow
                                CO0012_JNLRow("TERMID") = CO0012row("TERMID")
                                CO0012_JNLRow("CAMPCODE") = CO0012row("CAMPCODE")
                                CO0012_JNLRow("OBJECT") = CO0012row("OBJECT")
                                CO0012_JNLRow("ROLE") = CO0012row("ROLE")
                                CO0012_JNLRow("STYMD") = CO0012row("STYMD")
                                CO0012_JNLRow("ENDYMD") = CO0012row("ENDYMD")
                                CO0012_JNLRow("CODENAMES") = CO0012row("CODENAMES")
                                CO0012_JNLRow("CODENAMEL") = CO0012row("CODENAMEL")
                                CO0012_JNLRow("DELFLG") = CO0012row("DELFLG")
                                CO0012_JNLRow("INITYMD") = Date.Now
                                CO0012_JNLRow("UPDUSER") = Master.USERID

                                '更新ジャーナル追加
                                CS0020JOURNAL.TABLENM = "S0012_SRVAUTHOR"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = CO0012_JNLRow
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
                            Catch ex As Exception
                                If ex.Message = "Error raised in TIMSTP" Then
                                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                                End If

                                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR JOURNAL")
                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:S0012_SRVAUTHOR JOURNAL"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR  UPDATE_INSERT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0012_SRVAUTHOR UPDATE_INSERT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            Exit Sub
        End Try

        '○画面表示データ保存
        Master.SaveTable(CO0012tbl)
        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_SELTERM.Focus()

    End Sub
    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRCO0012WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = CO0012tbl                        'データ参照DataTable
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
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRCO0012WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = CO0012tbl                        'データ参照DataTable
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
    ''' <summary>
    ''' ファイルアップロード入力処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UPLOAD_EXCEL()

        rightview.setErrorReport("")

        '○ 初期処理
        WW_ERRLIST = New List(Of String)
        WW_ERRLIST_ALL = New List(Of String)

        '○UPLOAD_XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0023XLSUPLOAD.MAPID = GRCO0012WRKINC.MAPID
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

        '■(CS0023XLSTBL.TBLDATA --> CO0012INPtbl)
        '○ 列情報追加
        CO0012INPtbl_ColumnsAdd()
        CO0012UPDtbl_ColumnsAdd()

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows

            'デフォルトレコード削除
            Do Until CO0012UPDtbl.Rows.Count = 0
                CO0012UPDtbl.Rows(0).Delete()
            Loop

            'チェック入力レコード削除
            Do Until CO0012INPtbl.Rows.Count = 0
                CO0012INPtbl.Rows(0).Delete()
            Loop
            '■■■ 項目チェック準備 ■■■

            If Not IsDBNull(XLSTBLrow("TERMID")) Then
                '○ 入力テーブル事前編集　＆　CO0012INPtbl作成(Excel→CO0012INPtbl)

                '○ 初期処理
                WW_ERRLIST = New List(Of String)

                Dim WW_STYMD_H As String = ""
                Dim WW_ENDYMD_H As String = ""
                Dim WW_TERMID_H As String = ""
                Dim WW_CAMPCODE_H As String = ""
                Dim WW_OBJECT_H As String = ""
                Dim WW_ROLE_H As String = ""
                Dim WW_CODENAMES_H As String = ""
                Dim WW_CODENAMEL_H As String = ""

                '○ 固定項目
                Dim CO0012INProw As DataRow = CO0012INPtbl.NewRow
                CO0012INProw("LINECNT") = "0"
                CO0012INProw("OPERATION") = ""
                CO0012INProw("TIMSTP") = ""
                CO0012INProw("SELECT") = 0        '数値
                CO0012INProw("HIDDEN") = 0        '数値（0=表示対象 , 1=非表示対象）

                '○ 各項目
                '有効開始日
                If IsDBNull(XLSTBLrow("STYMD")) OrElse
                   IsNothing(XLSTBLrow("STYMD")) Then
                    CO0012INProw("STYMD") = ""
                Else
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("STYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0012INProw("STYMD") = ""
                    Else
                        CO0012INProw("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                    WW_STYMD_H = XLSTBLrow("STYMD")
                End If

                '有効終了日
                If IsDBNull(XLSTBLrow("ENDYMD")) OrElse
                   IsNothing(XLSTBLrow("ENDYMD")) Then
                    CO0012INProw("ENDYMD") = ""
                Else
                    Dim WW_DATE As Date
                    Date.TryParse(XLSTBLrow("ENDYMD"), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        CO0012INProw("ENDYMD") = ""
                    Else
                        CO0012INProw("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                    WW_ENDYMD_H = XLSTBLrow("ENDYMD")
                End If

                'ユーザＩＤ
                If IsDBNull(XLSTBLrow("TERMID")) OrElse
                   IsNothing(XLSTBLrow("TERMID")) Then
                    CO0012INProw("TERMID") = ""
                Else
                    CO0012INProw("TERMID") = XLSTBLrow("TERMID")
                    WW_TERMID_H = XLSTBLrow("TERMID")
                End If

                'マスタキー
                If IsDBNull(XLSTBLrow("OBJECT")) OrElse
                   IsNothing(XLSTBLrow("OBJECT")) Then
                    CO0012INProw("OBJECT") = ""
                Else
                    CO0012INProw("OBJECT") = XLSTBLrow("OBJECT")
                    WW_OBJECT_H = XLSTBLrow("OBJECT")
                End If

                '会社CD
                If IsDBNull(XLSTBLrow("CAMPCODE")) OrElse
                   IsNothing(XLSTBLrow("CAMPCODE")) Then
                    CO0012INProw("CAMPCODE") = ""
                Else
                    CO0012INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
                    WW_CAMPCODE_H = XLSTBLrow("CAMPCODE")
                End If

                'ロール
                If IsDBNull(XLSTBLrow("ROLE")) OrElse
                   IsNothing(XLSTBLrow("ROLE")) Then
                    CO0012INProw("ROLE") = ""
                Else
                    CO0012INProw("ROLE") = XLSTBLrow("ROLE")
                End If

                '権限
                If IsDBNull(XLSTBLrow("PERMITCODE")) OrElse
                   IsNothing(XLSTBLrow("PERMITCODE")) Then
                    CO0012INProw("PERMITCODE") = ""
                Else
                    CO0012INProw("PERMITCODE") = XLSTBLrow("PERMITCODE")
                End If

                'ロール名（短）
                If IsDBNull(XLSTBLrow("CODENAMES")) OrElse
                   IsNothing(XLSTBLrow("CODENAMES")) Then
                    CO0012INProw("CODENAMES") = ""
                Else
                    CO0012INProw("CODENAMES") = XLSTBLrow("CODENAMES")
                End If

                'ロール名（長）
                If IsDBNull(XLSTBLrow("CODENAMEL")) OrElse
                   IsNothing(XLSTBLrow("CODENAMEL")) Then
                    CO0012INProw("CODENAMEL") = ""
                Else
                    CO0012INProw("CODENAMEL") = XLSTBLrow("CODENAMEL")
                End If

                '削除
                If IsDBNull(XLSTBLrow("DELFLG")) OrElse
                   IsNothing(XLSTBLrow("DELFLG")) Then
                    CO0012INProw("DELFLG") = "0"
                Else
                    CO0012INProw("DELFLG") = XLSTBLrow("DELFLG")
                End If

                CO0012INPtbl.Rows.Add(CO0012INProw)

                '○ 入力テーブル作成(画面Grid→CO0012INPtbl)

                DefaultToCO0012UPDtbl(WW_ERRCODE)
                If Not isNormal(WW_ERRCODE) Then Exit Sub
                '■■■ 項目チェック ■■■
                '
                'CO0012INPtbl内容　チェック
                Dim WW_RTN_Detail As String = String.Empty
                Dim WW_RTN_Action As String = String.Empty
                '　※チェックOKデータをCO0012UPDtblへ格納する
                CO0012INPtbl_CHEK(WW_ERRCODE, WW_RTN_Detail, WW_RTN_Action)

                '■■■ GridView更新 ■■■
                'KEYエラー判定。
                Dim WW_ERR10023 As String = ""
                For Each WW_ERR As String In WW_ERRLIST
                    If WW_ERR = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
                        WW_ERR10023 = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    End If
                Next

                '○チェックOKデータ(CO0012UPDtbl)を一覧(CO0012tbl)へ反映
                'KEYエラー以外は明細処理を行う。
                If WW_ERR10023 <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
                    Dim WW_ERR100XX As String = C_MESSAGE_NO.NORMAL
                    For Each WW_ERR As String In WW_ERRLIST
                        If Not isNormal(WW_ERR) Then
                            WW_ERR100XX = WW_ERR
                            Exit For
                        End If
                    Next

                    CO0012tbl_UPD(WW_ERR100XX, WW_RTN_Detail, WW_RTN_Action)
                End If
            End If
        Next

        '■■■ 画面（GridView）表示データ保存 ■■■
        '○画面表示データ保存
        Master.SaveTable(CO0012tbl)

        'エラー編集
        For Each WW_ERR As String In WW_ERRLIST_ALL
            If WW_ERR = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                Exit For
            End If
        Next

        If WW_ERRLIST_ALL.Count = 0 Then
            Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.NOR)
        End If

        'detailboxクリア
        detailbox_Clear()

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
        WW_TBLview = New DataView(CO0012tbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

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
                        Case LIST_BOX_CLASSIFICATION.LC_COMPANY
                            prmData = work.CreateCompParam()
                        Case LIST_BOX_CLASSIFICATION.LC_TERM
                            prmData = work.CreateTERMIDParam()
                        Case 901
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SRVOBJECT")
                        Case 911
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SRVOBJECT")
                        Case LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                            prmData = work.CreateRoleParam(WF_CAMPCODE.Text, "SRVOBJECT", WF_OBJECT.Text)
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

    ' ******************************************************************************
    ' ***  leftBOX選択ボタン押下時処理(ListBox値 ---> detailbox)                 ***    2015/5/4完了
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim value As String() = leftview.getActiveValue

        '選択内容を画面項目へセット
        '項目セット　＆　フォーカス
        Select Case WF_FIELD.Value
            Case "WF_SELOBJECT"
                WF_SELOBJECT_TEXT.Text = value(1)
                WF_SELOBJECT.Text = value(0)
                WF_SELOBJECT.Focus()
            Case "WF_SELTERM"
                WF_SELTERM.Text = value(0)
                WF_SELTERM.Focus()

            Case "WF_TERMID"
                WF_TERMID.Text = value(0)
                WF_TERMID.Focus()
            Case "WF_CAMPCODE"
                WF_CAMPCODE_TEXT.Text = value(1)
                WF_CAMPCODE.Text = value(0)
                WF_CAMPCODE.Focus()
            Case "WF_OBJECT"
                WF_OBJECT_TEXT.Text = value(1)
                WF_OBJECT.Text = value(0)
                WF_OBJECT.Focus()
            Case "WF_ROLE"
                WF_ROLE_TEXT.Text = value(0).Substring(0, 1)
                WF_ROLE.Text = value(1).Split(" ")(1)
                WF_ROLE.Focus()
            Case "WF_DELFLG"
                WF_DELFLG_TEXT.Text = value(1)
                WF_DELFLG.Text = value(0)
                WF_DELFLG.Focus()
            Case "WF_STYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(value(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = value(0)
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()
            Case "WF_ENDYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(value(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = value(0)
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_SELOBJECT"
                WF_SELOBJECT.Focus()
            Case "WF_SELTERM"
                WF_SELTERM.Focus()

            Case "WF_TERMID"
                WF_TERMID.Focus()
            Case "WF_CAMPCODE"
                WF_CAMPCODE.Focus()
            Case "WF_OBJECT"
                WF_OBJECT.Focus()
            Case "WF_DELFLG"
                WF_DELFLG.Focus()
            Case "WF_STYMD"
                WF_STYMD.Focus()
            Case "WF_ENDYMD"
                WF_ENDYMD.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_LeftboxOpen.Value = ""


    End Sub

    ' ******************************************************************************
    ' ***  一覧表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベース（MC010_T3CNTL）を検索し画面表示する一覧を作成する</remarks>
    Private Sub MAPDATAget()
        Dim SortStr As String = String.Empty

        '■■■ 画面表示用データ取得 ■■■

        'ユーザプロファイル（変数）内容検索(自ユーザ権限＆抽出条件なしで検索)
        Try
            '■テーブル検索結果をテーブル退避
            'CO0012テンポラリDB項目作成
            CO0012tbl_ColumnsAdd()

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                'ソート文字列取得
                CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
                CS0026TBLSORT.PROFID = Master.PROF_VIEW
                CS0026TBLSORT.MAPID = Master.MAPID
                CS0026TBLSORT.VARI = Master.VIEWID
                CS0026TBLSORT.TAB = ""
                CS0026TBLSORT.getSorting()
                SortStr = CS0026TBLSORT.SORTING

                '検索SQL文
                '　検索説明
                '　　Step1：操作USERが、メンテナンス可能なUSERを取得
                '　　　　　　※権限ではUSER、MAPで行う必要があるが、絞り込み効率を勘案し、最初にUSERで処理を限定
                '　　Step2：メンテナンス可能USERおよびデフォルトUSERのTBL(S0012_SRVAUTHOR)を取得
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
                      " SELECT * FROM ( " _
                    & " SELECT 0 as LINECNT , " _
                    & "       '' as OPERATION , " _
                    & "       TIMSTP = cast(C.UPDTIMSTP as bigint) , " _
                    & "       rtrim(C.TERMID)  as TERMID, " _
                    & "       rtrim(C.CAMPCODE) as CAMPCODE, " _
                    & "       rtrim(C.OBJECT) as OBJECT , " _
                    & "       '' as OBJECTNAMES , " _
                    & "       rtrim(C.ROLE) as ROLE , " _
                    & "       rtrim(E.PERMITCODE) as PERMITCODE , " _
                    & "       rtrim(C.SEQ) as SEQ , " _
                    & "       rtrim(C.ROLENAMES) as CODENAMES , " _
                    & "       rtrim(C.ROLENAMEL) as CODENAMEL , " _
                    & "       C.STYMD , " _
                    & "       C.ENDYMD , " _
                    & "       rtrim(C.DELFLG) as DELFLG , " _
                    & "       rtrim(F.NAMES) as CAMPNAMES  " _
                    & " FROM " _
                    & "     ( SELECT " _
                    & "         S0006_ROLE.CAMPCODE,S0006_ROLE.OBJECT,S0006_ROLE.ROLE,MAX(S0006_ROLE.PERMITCODE) as PERMITCODE " _
                    & "       FROM S0006_ROLE " _
                    & "            ,MC001_FIXVALUE " _
                    & "       WHERE      MC001_FIXVALUE.CAMPCODE = S0006_ROLE.CAMPCODE " _
                    & "             and  MC001_FIXVALUE.CLASS = @P1 " _
                    & "             and  MC001_FIXVALUE.KEYCODE = S0006_ROLE.OBJECT " _
                    & "             and  S0006_ROLE.STYMD   <= @P4 " _
                    & "             and  S0006_ROLE.ENDYMD  >= @P4 " _
                    & "             and  S0006_ROLE.DELFLG  <> '1' " _
                    & "             and  MC001_FIXVALUE.STYMD   <= @P4 " _
                    & "             and  MC001_FIXVALUE.ENDYMD  >= @P4 " _
                    & "             and  MC001_FIXVALUE.DELFLG  <> '1' " _
                    & "        GROUP BY  S0006_ROLE.CAMPCODE,S0006_ROLE.OBJECT,S0006_ROLE.ROLE " _
                    & "     ) E " _
                    & "     ,S0012_SRVAUTHOR C " _
                    & " LEFT JOIN M0001_CAMP F " _
                    & "   ON   F.CAMPCODE = C.CAMPCODE " _
                    & "   and  F.STYMD   <= @P4 " _
                    & "   and  F.ENDYMD  >= @P4 " _
                    & "   and  F.DELFLG  <> '1' " _
                    & " WHERE  C.STYMD   <= @P2 " _
                    & "   and  C.ENDYMD  >= @P3 " _
                    & "   and  C.DELFLG  <> '1' " _
                    & "   and  E.OBJECT   = C.OBJECT " _
                    & "   and  E.ROLE     = C.ROLE " _
                    & "   and  E.CAMPCODE = C.CAMPCODE " _
                    & " ) AAA "

                ' ソート文字列指定がある場合は再度ORDER BYを生成する
                If SortStr <> String.Empty Then
                    SQLStr = String.Format("{0} ORDER BY {1}", SQLStr, SortStr)
                Else
                    SQLStr = String.Format("{0} ORDER BY TERMID , CAMPCODE , OBJECT, ROLE, SEQ", SQLStr)
                End If

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)

                    PARA1.Value = "SRVOBJECT"
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = Date.Now

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'CO0012tbl値設定
                        While SQLdr.Read
                            'USERID_G
                            Dim WW_SELECT_TERM_G As Integer = 0    '0:対象外、1:対象

                            'TERMID(From-To)
                            If String.IsNullOrEmpty(work.WF_SEL_TERMIDF.Text) AndAlso
                               String.IsNullOrEmpty(work.WF_SEL_TERMIDT.Text) Then
                                WW_SELECT_TERM_G = 1
                            Else
                                If SQLdr("TERMID") >= work.WF_SEL_TERMIDF.Text AndAlso
                                   SQLdr("TERMID") <= work.WF_SEL_TERMIDT.Text Then
                                    WW_SELECT_TERM_G = 1
                                End If
                            End If

                            Dim CO0012row As DataRow = CO0012tbl.NewRow()

                            '○テンポラリTable追加
                            '固定項目
                            CO0012row("LINECNT") = SQLdr("LINECNT")
                            CO0012row("OPERATION") = SQLdr("OPERATION")
                            CO0012row("TIMSTP") = SQLdr("TIMSTP")

                            If WW_SELECT_TERM_G = 1 Then      'SELECT ... 1：対象,0：対象外
                                CO0012row("SELECT") = 1
                            Else
                                CO0012row("SELECT") = 0
                            End If

                            CO0012row("HIDDEN") = 0

                            '画面毎の設定項目
                            CO0012row("TERMID") = SQLdr("TERMID")
                            CO0012row("CAMPCODE") = SQLdr("CAMPCODE")
                            CO0012row("CAMPNAMES") = SQLdr("CAMPNAMES")
                            CO0012row("OBJECT") = SQLdr("OBJECT")

                            ' オブジェクトのListから設定する
                            CO0012row("OBJECTNAMES") = String.Empty
                            CODENAME_get("OBJECT", CO0012row("OBJECT"), CO0012row("OBJECTNAMES"), WW_DUMMY, CO0012row("CAMPCODE"))

                            CO0012row("ROLE") = SQLdr("ROLE")
                            CO0012row("PERMITCODE") = SQLdr("PERMITCODE")
                            CO0012row("SEQ") = SQLdr("SEQ")
                            CO0012row("CODENAMES") = SQLdr("CODENAMES")
                            CO0012row("CODENAMEL") = SQLdr("CODENAMEL")
                            CO0012row("STYMD") = CDate(SQLdr("STYMD")).ToString("yyyy/MM/dd")
                            CO0012row("ENDYMD") = CDate(SQLdr("ENDYMD")).ToString("yyyy/MM/dd")
                            CO0012row("DELFLG") = SQLdr("DELFLG")

                            '抽出対象外の場合、レコード追加しない
                            If CO0012row("SELECT") = 1 Then
                                CO0012tbl.Rows.Add(CO0012row)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0012_SRVAUTHOR Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ' ソート文字列が指定されている場合はソートしてからLINECNTを振り直す
        If Not String.IsNullOrEmpty(SortStr) Then
            ' 取得データのビュー生成
            Dim WW_TBLview As DataView = New DataView(CO0012tbl)
            ' ダミーDataTable生成
            Dim dt2 As DataTable = CO0012tbl.Clone()
            ' ソートする
            WW_TBLview.Sort = SortStr
            ' ソート結果を取得データに戻す
            For Each drv As DataRowView In WW_TBLview
                dt2.ImportRow(drv.Row)
            Next
            ' 結果で置き換える
            CO0012tbl = dt2
        End If

        '項番(LineCnt)設定
        Dim WW_LINECNT As Integer = 0
        For Each CO0012row As DataRow In CO0012tbl.Rows
            'SELECT=1:番号振り直し、以外：削除
            WW_LINECNT = WW_LINECNT + 1
            CO0012row("LINECNT") = WW_LINECNT
        Next

    End Sub

    ''' <summary>
    ''' 一覧の明細行ダブルクリック時処理
    ''' </summary>
    ''' <remarks>(GridView ---> detailbox)</remarks>
    Protected Sub WF_Grid_DBclick()

        '■■■　画面detailboxへ表示　■■■
        '画面選択明細(GridView)から画面detailboxへ表示

        '○ 抽出条件(ヘッダーレコードより)定義
        Dim WW_TERMID As String = ""
        Dim WW_CAMPCODE As String = ""
        Dim WW_OBJECT As String = ""
        Dim WW_ROLE As String = ""
        Dim WW_CODENAMES As String = ""
        Dim WW_CODENAMEL As String = ""
        Dim WW_STYMD As String = ""
        Dim WW_ENDYMD As String = ""
        Dim WW_Position As Integer = 0

        '項番退避(WF_change)より実アドレス取得
        For i As Integer = 0 To CO0012tbl.Rows.Count - 1
            If CO0012tbl.Rows(i)("LINECNT") = WF_GridDBclick.Text Then
                WW_Position = i
                Exit For
            End If
        Next

        '○ ダブルクリック明細情報取得設定（GridView --> Detailboxヘッダー情報)
        For Each CO0012col As DataColumn In CO0012tbl.Columns
            Dim WW_DataField As String = CO0012col.ColumnName '項目名取得用

            Select Case WW_DataField
                Case "LINECNT"
                    WF_Sel_LINECNT.Text = CO0012tbl.Rows(WW_Position)(CO0012col)

                Case "TERMID"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_TERMID.Text = ""
                        WW_TERMID = ""
                    Else
                        WF_TERMID.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_TERMID = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "CAMPCODE"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_CAMPCODE.Text = ""
                        WW_CAMPCODE = ""
                    Else
                        WF_CAMPCODE.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_CAMPCODE = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "CAMPNAMES"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_CAMPCODE_TEXT.Text = ""
                    Else
                        WF_CAMPCODE_TEXT.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "OBJECT"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_OBJECT.Text = ""
                        WW_OBJECT = ""
                    Else
                        WF_OBJECT.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_OBJECT = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "OBJECTNAMES"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_OBJECT_TEXT.Text = ""
                    Else
                        WF_OBJECT_TEXT.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "ROLE"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_ROLE.Text = ""
                        WW_ROLE = ""
                    Else
                        WF_ROLE.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_ROLE = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "PERMITCODE"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_ROLE_TEXT.Text = ""
                    Else
                        WF_ROLE_TEXT.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "CODENAMES"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_CODENAMES.Text = ""
                    Else
                        WF_CODENAMES.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "CODENAMEL"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_CODENAMEL.Text = ""
                    Else
                        WF_CODENAMEL.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "STYMD"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_STYMD.Text = ""
                        WW_STYMD = ""
                    Else
                        WF_STYMD.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_STYMD = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "ENDYMD"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_ENDYMD.Text = ""
                        WW_ENDYMD = ""
                    Else
                        WF_ENDYMD.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        WW_ENDYMD = CO0012tbl.Rows(WW_Position)(CO0012col)
                    End If

                Case "DELFLG"
                    If CO0012tbl.Rows(WW_Position)(CO0012col) = "&nbsp;" Then
                        WF_DELFLG.Text = ""
                    Else
                        WF_DELFLG.Text = CO0012tbl.Rows(WW_Position)(CO0012col)
                        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY, WF_CAMPCODE.Text)
                    End If
            End Select
        Next
        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each CO0012row As DataRow In CO0012tbl.Rows
            Select Case CO0012row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case CO0012tbl.Rows(WW_Position)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                CO0012tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                CO0012tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                CO0012tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                CO0012tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                CO0012tbl.Rows(WW_Position)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(CO0012tbl)

        WF_GridDBclick.Text = ""

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

        '○DetailBoxをCO0012INPtblへ退避
        DetailBoxToCO0012INPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        '○デフォルトをCO0012UPDtblへ退避
        CO0012UPDtbl_ColumnsAdd()
        DefaultToCO0012UPDtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then Exit Sub

        '■■■ 項目チェック ■■■

        '○ 初期処理
        WW_ERRLIST = New List(Of String)
        WW_ERRLIST_ALL = New List(Of String)
        Dim WW_RTN_Detail As String = String.Empty
        Dim WW_RTN_Action As String = String.Empty

        'CO0012INPtbl内容　チェック
        '　※チェックOKデータをCO0012UPDtblへ格納する
        CO0012INPtbl_CHEK(WW_ERRCODE, WW_RTN_Detail, WW_RTN_Action)
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(WW_ERRCODE, C_MESSAGE_TYPE.ERR, WW_RTN_Detail)
        End If

        '■■■ GridView更新 ■■■
        'KEYエラー判定
        Dim WW_ERR10023 As String = C_MESSAGE_NO.NORMAL
        For Each WW_ERR As String In WW_ERRLIST
            If WW_ERR = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
                WW_ERR10023 = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Exit For
            End If
        Next

        '○チェックOKデータ(CO0012UPDtbl)を一覧(CO0012tbl)へ反映
        'KEYエラー以外は明細処理を行う。
        If WW_ERR10023 <> C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR Then
            Dim WW_ERR100XX As String = C_MESSAGE_NO.NORMAL
            For Each WW_ERR As String In WW_ERRLIST
                If Not isNormal(WW_ERR) Then
                    WW_ERR100XX = WW_ERR
                    Exit For
                End If
            Next

            CO0012tbl_UPD(WW_ERR100XX, WW_RTN_Detail, WW_RTN_Action)
        End If

        '○一覧(CO0012tbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        If isNormal(WW_ERR10023) Then
            Dim WW_ISDEL As Boolean = True

            Do
                For i As Integer = 0 To CO0012tbl.Rows.Count - 1
                    If CO0012tbl.Rows(i)("TIMSTP") = "0" AndAlso CO0012tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        CO0012tbl.Rows(i).Delete()
                        WW_ISDEL = False
                        Exit For
                    Else
                        If (CO0012tbl.Rows.Count - 1) <= i Then WW_ISDEL = True
                    End If
                Next
            Loop Until WW_ISDEL = True

        End If

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0012tbl)

        If Not isNormal(WW_ERR10023) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        Else
            If WW_ERRLIST.Count = 0 OrElse isNormal(WW_ERRLIST(0)) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
                'detailboxクリア
                detailbox_Clear()
            Else
                Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ERR)
                'detailboxクリア
                detailbox_Clear()
            End If
        End If

        'カーソル設定
        WF_TERMID.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        'detailboxクリア
        detailbox_Clear()
        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        'カーソル設定
        WF_TERMID.Focus()

    End Sub
    ''' <summary>
    ''' 明細実初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub detailbox_Clear()

        '■画面WF_GRID状態設定
        '状態をクリア設定
        For Each CO0012row As DataRow In CO0012tbl.Rows
            Select Case CO0012row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(CO0012tbl)

        WF_Sel_LINECNT.Text = ""
        WF_TERMID.Text = ""
        WF_CAMPCODE.Text = ""
        WF_CAMPCODE_TEXT.Text = ""
        WF_OBJECT.Text = ""
        WF_OBJECT_TEXT.Text = ""
        WF_ROLE.Text = ""
        WF_ROLE_TEXT.Text = ""
        WF_CODENAMES.Text = ""
        WF_CODENAMEL.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        WF_TERMID.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' CO0012tbl項目設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0012tbl_ColumnsAdd()

        If IsNothing(CO0012tbl) Then CO0012tbl = New DataTable
        If CO0012tbl.Columns.Count <> 0 Then CO0012tbl.Columns.Clear()

        'CO0012テンポラリDB項目作成
        CO0012tbl.Clear()
        CO0012tbl.Columns.Add("LINECNT", GetType(Integer))          'DBの固定フィールド
        CO0012tbl.Columns.Add("OPERATION", GetType(String))         'DBの固定フィールド
        CO0012tbl.Columns.Add("TIMSTP", GetType(String))            'DBの固定フィールド
        CO0012tbl.Columns.Add("SELECT", GetType(Integer))           'DBの固定フィールド
        CO0012tbl.Columns.Add("HIDDEN", GetType(Integer))           'DBの固定フィールド

        CO0012tbl.Columns.Add("TERMID", GetType(String))
        CO0012tbl.Columns.Add("CAMPCODE", GetType(String))
        CO0012tbl.Columns.Add("CAMPNAMES", GetType(String))
        CO0012tbl.Columns.Add("OBJECT", GetType(String))
        CO0012tbl.Columns.Add("OBJECTNAMES", GetType(String))
        CO0012tbl.Columns.Add("ROLE", GetType(String))
        CO0012tbl.Columns.Add("PERMITCODE", GetType(String))
        CO0012tbl.Columns.Add("CODENAMES", GetType(String))
        CO0012tbl.Columns.Add("CODENAMEL", GetType(String))
        CO0012tbl.Columns.Add("SEQ", GetType(Integer))
        CO0012tbl.Columns.Add("STYMD", GetType(String))
        CO0012tbl.Columns.Add("ENDYMD", GetType(String))
        CO0012tbl.Columns.Add("DELFLG", GetType(String))
    End Sub

    ''' <summary>
    ''' CO0012INPtbl項目設定
    ''' </summary>
    Protected Sub CO0012INPtbl_ColumnsAdd()

        If IsNothing(CO0012INPtbl) Then CO0012INPtbl = New DataTable
        If CO0012INPtbl.Columns.Count <> 0 Then CO0012INPtbl.Columns.Clear()

        'CO0012テンポラリDB項目作成
        CO0012INPtbl.Clear()
        CO0012INPtbl.Columns.Add("LINECNT", GetType(Integer))           'DBの固定フィールド
        CO0012INPtbl.Columns.Add("OPERATION", GetType(String))          'DBの固定フィールド
        CO0012INPtbl.Columns.Add("TIMSTP", GetType(String))             'DBの固定フィールド
        CO0012INPtbl.Columns.Add("SELECT", GetType(Integer))            'DBの固定フィールド
        CO0012INPtbl.Columns.Add("HIDDEN", GetType(Integer))            'DBの固定フィールド

        CO0012INPtbl.Columns.Add("TERMID", GetType(String))
        CO0012INPtbl.Columns.Add("CAMPCODE", GetType(String))
        CO0012INPtbl.Columns.Add("CAMPNAMES", GetType(String))
        CO0012INPtbl.Columns.Add("OBJECT", GetType(String))
        CO0012INPtbl.Columns.Add("OBJECTNAMES", GetType(String))
        CO0012INPtbl.Columns.Add("ROLE", GetType(String))
        CO0012INPtbl.Columns.Add("PERMITCODE", GetType(String))
        CO0012INPtbl.Columns.Add("SEQ", GetType(Integer))
        CO0012INPtbl.Columns.Add("STYMD", GetType(String))
        CO0012INPtbl.Columns.Add("ENDYMD", GetType(String))
        CO0012INPtbl.Columns.Add("CODENAMES", GetType(String))
        CO0012INPtbl.Columns.Add("CODENAMEL", GetType(String))
        CO0012INPtbl.Columns.Add("DELFLG", GetType(String))

    End Sub

    ''' <summary>
    ''' CO0012UPDtbl項目設定
    ''' </summary>
    Protected Sub CO0012UPDtbl_ColumnsAdd()
        If IsNothing(CO0012UPDtbl) Then CO0012UPDtbl = New DataTable
        If CO0012UPDtbl.Columns.Count <> 0 Then CO0012UPDtbl.Columns.Clear()

        'CO0012テンポラリDB項目作成
        CO0012UPDtbl.Clear()
        CO0012UPDtbl.Columns.Add("LINECNT", GetType(Integer))           'DBの固定フィールド
        CO0012UPDtbl.Columns.Add("OPERATION", GetType(String))          'DBの固定フィールド
        CO0012UPDtbl.Columns.Add("TIMSTP", GetType(String))             'DBの固定フィールド
        CO0012UPDtbl.Columns.Add("SELECT", GetType(Integer))            'DBの固定フィールド
        CO0012UPDtbl.Columns.Add("HIDDEN", GetType(Integer))            'DBの固定フィールド

        CO0012UPDtbl.Columns.Add("TERMID", GetType(String))
        CO0012UPDtbl.Columns.Add("OBJECT", GetType(String))
        CO0012UPDtbl.Columns.Add("OBJECTNAMES", GetType(String))
        CO0012UPDtbl.Columns.Add("CAMPCODE", GetType(String))
        CO0012UPDtbl.Columns.Add("CAMPNAMES", GetType(String))
        CO0012UPDtbl.Columns.Add("ROLE", GetType(String))
        CO0012UPDtbl.Columns.Add("PERMITCODE", GetType(String))
        CO0012UPDtbl.Columns.Add("SEQ", GetType(Integer))
        CO0012UPDtbl.Columns.Add("STYMD", GetType(String))
        CO0012UPDtbl.Columns.Add("ENDYMD", GetType(String))
        CO0012UPDtbl.Columns.Add("CODENAMES", GetType(String))
        CO0012UPDtbl.Columns.Add("CODENAMEL", GetType(String))
        CO0012UPDtbl.Columns.Add("DELFLG", GetType(String))
    End Sub

    ''' <summary>
    ''' S0012_SRVAUTHORtbl項目設定
    ''' </summary>
    Protected Sub S0012_SRVAUTHORtbl_ColumnsAdd()

        If IsNothing(S0012_SRVAUTHORtbl) Then S0012_SRVAUTHORtbl = New DataTable
        If S0012_SRVAUTHORtbl.Columns.Count <> 0 Then S0012_SRVAUTHORtbl.Columns.Clear()

        S0012_SRVAUTHORtbl.Columns.Add("TERMID", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("CAMPCODE", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("OBJECT", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("ROLE", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("PERMITCODE", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("SEQ", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("STYMD", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("ENDYMD", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("CODENAMES", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("CODENAMEL", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("DELFLG", GetType(String))

        S0012_SRVAUTHORtbl.Columns.Add("INITYMD", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("UPDYMD", GetType(String))
        S0012_SRVAUTHORtbl.Columns.Add("UPDUSER", GetType(String))
    End Sub

    ''' <summary>
    ''' DetailBox退避(CO0012INPtbl)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToCO0012INPtbl(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        'CO0012テンポラリDB項目作成
        CO0012INPtbl_ColumnsAdd()

        '■■■ 入力文字置き換え & CS0007CHKテーブルレコード追加　■■■

        '○画面(Repeaterヘッダー情報)の使用禁止文字排除

        'WF_TERMID.Text
        Master.eraseCharToIgnore(WF_TERMID.Text)
        'WF_CAMPCODE.Text
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)

        'WF_OBJECT.Text
        Master.eraseCharToIgnore(WF_OBJECT.Text)
        'WF_ROLE.Text
        Master.eraseCharToIgnore(WF_ROLE.Text)

        'WF_CODENAMES.Text
        Master.eraseCharToIgnore(WF_CODENAMES.Text)

        'WF_CODENAMEL.Text
        Master.eraseCharToIgnore(WF_CODENAMEL.Text)

        'WF_STYMD.Text
        Master.eraseCharToIgnore(WF_STYMD.Text)

        'WF_ENDYMD.Text
        Master.eraseCharToIgnore(WF_ENDYMD.Text)

        'WF_DELFLG.Text
        Master.eraseCharToIgnore(WF_DELFLG.Text)

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_TERMID.Text) AndAlso
            String.IsNullOrEmpty(WF_CAMPCODE.Text) AndAlso
            String.IsNullOrEmpty(WF_OBJECT.Text) AndAlso
            String.IsNullOrEmpty(WF_ROLE.Text) AndAlso
            String.IsNullOrEmpty(WF_CODENAMES.Text) AndAlso
            String.IsNullOrEmpty(WF_CODENAMEL.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToCO0012INPtbl"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○画面(Repeaterヘッダー情報)のテーブル退避

        Dim CO0012INProw As DataRow = CO0012INPtbl.NewRow
        If (String.IsNullOrEmpty(WF_Sel_LINECNT.Text)) Then
            CO0012INProw("LINECNT") = 0
        Else
            CO0012INProw("LINECNT") = CType(WF_Sel_LINECNT.Text, Integer) 'DBの固定フィールド
        End If
        CO0012INProw("OPERATION") = ""                                 'DBの固定フィールド
        CO0012INProw("TIMSTP") = ""                                    'DBの固定フィールド
        CO0012INProw("SELECT") = "0"                                   'DBの固定フィールド
        CO0012INProw("HIDDEN") = "0"                                   'DBの固定フィールド

        CO0012INProw("TERMID") = WF_TERMID.Text

        CO0012INProw("OBJECT") = WF_OBJECT.Text

        CO0012INProw("OBJECTNAMES") = WF_OBJECT_TEXT.Text

        CO0012INProw("CAMPCODE") = WF_CAMPCODE.Text

        CO0012INProw("CAMPNAMES") = WF_CAMPCODE_TEXT.Text

        CO0012INProw("ROLE") = WF_ROLE.Text
        CO0012INProw("PERMITCODE") = WF_ROLE_TEXT.Text
        CO0012INProw("CODENAMES") = WF_CODENAMES.Text
        CO0012INProw("CODENAMEL") = WF_CODENAMEL.Text
        CO0012INProw("STYMD") = WF_STYMD.Text
        CO0012INProw("ENDYMD") = WF_ENDYMD.Text
        CO0012INProw("DELFLG") = WF_DELFLG.Text

        CO0012INPtbl.Rows.Add(CO0012INProw)

    End Sub

    ''' <summary>
    ''' Default退避(CO0012UPDtbl)
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DefaultToCO0012UPDtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CO0012UPDrow As DataRow = CO0012UPDtbl.NewRow
        CO0012UPDrow("LINECNT") = 0                         'DBの固定フィールド
        CO0012UPDrow("OPERATION") = ""                      'DBの固定フィールド
        CO0012UPDrow("TIMSTP") = ""                         'DBの固定フィールド
        CO0012UPDrow("SELECT") = "0"                        'DBの固定フィールド
        CO0012UPDrow("HIDDEN") = "0"                        'DBの固定フィールド

        CO0012UPDrow("TERMID") = ""
        CO0012UPDrow("OBJECT") = ""
        CO0012UPDrow("OBJECTNAMES") = ""
        CO0012UPDrow("CAMPCODE") = ""
        CO0012UPDrow("CAMPNAMES") = ""
        CO0012UPDrow("ROLE") = ""
        CO0012UPDrow("STYMD") = ""
        CO0012UPDrow("ENDYMD") = ""
        CO0012UPDrow("CODENAMES") = ""
        CO0012UPDrow("CODENAMEL") = ""
        CO0012UPDrow("DELFLG") = "0"
        CO0012UPDrow("SEQ") = 0

        CO0012UPDtbl.Rows.Add(CO0012UPDrow)

    End Sub

    ''' <summary>
    ''' CO0012INPtblチェック 
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <param name="O_RTN_Detail"></param>
    ''' <param name="O_RTN_Action"></param>
    ''' <remarks></remarks>
    Protected Sub CO0012INPtbl_CHEK(ByRef O_RTN As String, ByRef O_RTN_Detail As String, ByRef O_RTN_Action As String)

        '○インターフェイス初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL
        O_RTN_Detail = ""
        O_RTN_Action = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_TEXT As String = ""
        '■■■ CO0012INPのKEY重複 --> 先頭レコードを優先 ■■■
        Dim WW_Cnt1 As Integer = 0
        Dim WW_Cnt2 As Integer = 0
        Dim WW_Position As Integer = 0

        '○事前準備（キー重複レコード削除）
        'Deleteにより行カウントがずれるのでDoで回す(ドロップデータ件数はガードできていない)
        Do Until WW_Cnt1 > (CO0012INPtbl.Rows.Count - 1)

            WW_Cnt2 = WW_Cnt1 + 1
            Do Until WW_Cnt2 > (CO0012INPtbl.Rows.Count - 1)

                'KEY重複
                If CO0012INPtbl.Rows(WW_Cnt1)("TERMID") = CO0012INPtbl.Rows(WW_Cnt2)("TERMID") AndAlso
                   CO0012INPtbl.Rows(WW_Cnt1)("CAMPCODE") = CO0012INPtbl.Rows(WW_Cnt2)("CAMPCODE") AndAlso
                   CO0012INPtbl.Rows(WW_Cnt1)("OBJECT") = CO0012INPtbl.Rows(WW_Cnt2)("OBJECT") AndAlso
                   CO0012INPtbl.Rows(WW_Cnt1)("ROLE") = CO0012INPtbl.Rows(WW_Cnt2)("ROLE") AndAlso
                   CO0012INPtbl.Rows(WW_Cnt1)("STYMD") = CO0012INPtbl.Rows(WW_Cnt2)("STYMD") AndAlso
                   CO0012INPtbl.Rows(WW_Cnt1)("ENDYMD") = CO0012INPtbl.Rows(WW_Cnt2)("ENDYMD") Then
                    CO0012INPtbl.Rows(WW_Cnt2).Delete()
                Else
                    WW_Cnt2 = WW_Cnt2 + 1
                End If
            Loop
            WW_Cnt1 = WW_Cnt1 + 1
        Loop

        '■■■ チェック ＆　更新用テーブル作成■■■
        '○前提確認
        'CO0012UPDポジション　＆　行レコード取得

        'レコードなしチェック　　　☆☆☆☆　  2015/4/30追加
        If CO0012UPDtbl.Rows.Count = 0 Then
            If WW_ERRLIST_ALL.Count = 0 Then
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                O_RTN_Detail = "TERMID"
            End If

            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・更新できないレコード(端末ID未存在)です。"
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TERMID=" & CO0012INPtbl.Rows(0)("TERMID") & " , "
            WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
            Exit Sub
        End If

        'タイトル区分存在チェック(Iレコード)　…　Iレコードが無ければエラー
        'ヘッダーのみも存在するのでチェックしない

        '■■■　チェック実行　-->　OK時 CO007UPDtbl作成　■■■　…　パラメータ数を担保する必要あり(Defaultを参照)
        For Each CO0012INProw As DataRow In CO0012INPtbl.Rows
            '○単項目チェック　…　キー項目でエラー時、処理しない

            '○キー項目(TERMID)
            '①必須チェック
            WW_TEXT = CO0012INProw("TERMID")
            Master.checkFIeld(CO0012INProw("CAMPCODE"), "TERMID", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック(LeftBox存在しない場合エラー)
                'LeftBox存在チェック
                If Not String.IsNullOrEmpty(CO0012INProw("TERMID")) Then
                    CODENAME_get("TERMID", CO0012INProw("TERMID"), WW_TEXT, WW_RTN_SW, CO0012INProw("CAMPCODE"))
                    If Not isNormal(WW_RTN_SW) Then
                        If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "TERMID"
                        WW_CheckMES1 = "・更新できないレコード(端末エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If
            Else
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "TERMID"
                WW_CheckMES1 = "・更新できないレコード(端末エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○キー項目(OBJECT)
            '①必須チェック
            WW_TEXT = CO0012INProw("OBJECT")
            Master.checkFIeld(CO0012INProw("CAMPCODE"), "OBJECT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック(LeftBox存在しない場合エラー)
                'LeftBox存在チェック
                If Not String.IsNullOrEmpty(CO0012INProw("OBJECT")) Then
                    CO0012INProw("OBJECTNAMES") = String.Empty
                    CODENAME_get("OBJECT", CO0012INProw("OBJECT"), WW_TEXT, WW_RTN_SW, CO0012INProw("CAMPCODE"))
                    If isNormal(WW_RTN_SW) Then
                        CO0012INProw("OBJECTNAMES") = WW_TEXT
                    Else
                        If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "OBJECT"
                        WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If
            Else
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "OBJECT"
                WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○キー項目(ROLE)
            '①必須チェック
            WW_TEXT = CO0012INProw("ROLE")
            Master.checkFIeld(CO0012INProw("CAMPCODE"), "ROLE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック(LeftBox存在しない場合エラー)
                'LeftBox存在チェック 
                'ROLEは名称取得しない
                If Not String.IsNullOrEmpty(CO0012INProw("ROLE")) Then
                    CODENAME_get("ROLE", CO0012INProw("ROLE"), WW_TEXT, WW_RTN_SW, CO0012INProw("CAMPCODE"))
                    If Not isNormal(WW_RTN_SW) Then
                        If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "ROLE"
                        WW_CheckMES1 = "・更新できないレコード(ロールエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                        WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                    End If
                End If
            Else
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "ROLE"
                WW_CheckMES1 = "・更新できないレコード(ロールエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            ' ○必須チェック 会社コード
            If CO0012INProw("CAMPCODE") <> C_DEFAULT_DATAKEY Then
                WW_TEXT = CO0012INProw("CAMPCODE")
                Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'LeftBox存在チェック
                    If WW_TEXT = "" Then
                        CO0012INProw("CAMPCODE") = ""
                    Else
                        CO0012INProw("CAMPNAMES") = String.Empty
                        CODENAME_get("CAMPCODE", CO0012INProw("CAMPCODE"), WW_TEXT, WW_RTN_SW, CO0012INProw("CAMPCODE"))
                        If isNormal(WW_RTN_SW) Then
                            CO0012INProw("CAMPNAMES") = WW_TEXT
                        Else
                            WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    If WW_ERRLIST_ALL.Count = 0 Then
                        O_RTN = WW_CS0024FCHECKREPORT
                        O_RTN_Detail = "CAMPCODE"
                    End If
                    WW_CheckMES1 = "・更新できないレコード(会社エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '○キー項目(オブジェクト)
            '①必須チェック
            WW_TEXT = CO0012INProw("OBJECT")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OBJECT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "VARIANT"
                WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○キー項目(STYMD)
            '①必須チェック
            WW_TEXT = CO0012INProw("STYMD")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "STYMD"
                WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○キー項目(ENDYMD)
            '①必須チェック
            WW_TEXT = CO0012INProw("ENDYMD")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "ENDYMD"
                WW_CheckMES1 = "・更新できないレコード(オブジェクトエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○キー項目(DELFLG)
            '①必須チェック
            If String.IsNullOrEmpty(CO0012INProw("DELFLG")) OrElse CO0012INProw("DELFLG") = C_DELETE_FLG.ALIVE OrElse CO0012INProw("DELFLG") = C_DELETE_FLG.DELETE Then
                If String.IsNullOrEmpty(CO0012INProw("DELFLG")) Then CO0012INProw("DELFLG") = C_DELETE_FLG.ALIVE
            Else
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "DELFLG"
                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(削除CD不正)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○一般項目(その他)　…　カラム長を判定させる
            'ロール名（短）
            WW_TEXT = CO0012INProw("CODENAMES")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CODENAMES", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "CODENAMES"
                WW_CheckMES1 = "・更新できないレコード(ロール名（短）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            'ロール名（長）
            WW_TEXT = CO0012INProw("CODENAMEL")
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CODENAMEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "CODENAMEL"
                WW_CheckMES1 = "・更新できないレコード(ロール名（長）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If

            '○権限チェック

            '権限チェック(操作者がデータ内USERの更新権限があるかチェック
            '　※権限判定時点：現在
            Dim CS0025AUTHORget As New CS0025AUTHORget
            CS0025AUTHORget.USERID = Master.USERID
            CS0025AUTHORget.OBJCODE = "USER"
            CS0025AUTHORget.CODE = Master.USERID
            CS0025AUTHORget.STYMD = Date.Now
            CS0025AUTHORget.ENDYMD = Date.Now
            CS0025AUTHORget.CS0025AUTHORget()
            If Not (isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE >= C_PERMISSION.REFERLANCE) Then
                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "USERID"

                'エラーレポート編集
                WW_CheckMES1 = "・更新できないレコード(ユーザ操作権限なし)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                O_RTN = WW_CS0024FCHECKERR
                WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                WW_ERRLIST.Add(WW_CS0024FCHECKERR)
            End If


            '権限チェック(データ内USERが会社更新権限があるかチェック)
            '　※権限判定時点：データの有効日付
            If CO0012INProw("CAMPCODE") <> C_DEFAULT_DATAKEY Then

                CS0025AUTHORget.USERID = Master.USERID
                CS0025AUTHORget.OBJCODE = "CAMP"
                CS0025AUTHORget.CODE = CO0012INProw("CAMPCODE")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If Not (isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE >= C_PERMISSION.REFERLANCE) Then
                    If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "MAPID"

                    'エラーレポート編集
                    WW_CheckMES1 = "・更新できないレコード(ユーザに画面操作権限なし)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                    O_RTN = WW_CS0024FCHECKERR
                    WW_ERRLIST_ALL.Add(WW_CS0024FCHECKERR)
                    WW_ERRLIST.Add(WW_CS0024FCHECKERR)
                End If
            End If

            ' ○ロールマスタとの整合性判定 --- ロールマスタに同じ会社、オブジェクト、ロールが存在するか
            Try
                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open()       'DataBase接続(Open)

                    Dim SQLStr As String =
                          " SELECT COUNT(*) AS CNT " _
                        & " FROM S0006_ROLE " _
                        & " WHERE    1=1 " _
                        & "    and    CAMPCODE    = @P01 " _
                        & "    and    OBJECT      = @P02 " _
                        & "    and    ROLE        = @P03 " _
                        & "    and    DELFLG     <> '1' "

                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)

                        PARA01.Value = CO0012INProw("CAMPCODE")
                        PARA02.Value = CO0012INProw("OBJECT")
                        PARA03.Value = CO0012INProw("ROLE")

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                            If SQLdr.Read Then
                                Dim cnt As Integer = CType(SQLdr("CNT"), Integer)
                                If cnt = 0 Then
                                    'エラーレポート編集
                                    WW_CheckMES1 = "・更新できないレコード(ロールマスタとの不整合)です。"
                                    WW_CheckMES2 = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012INProw)
                                    O_RTN = WW_CS0024FCHECKERR
                                    WW_ERRLIST_ALL.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                    WW_ERRLIST.Add(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR)
                                End If
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0006_ROLE SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0006_ROLE SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                Exit Sub
            End Try

            '○キー項目チェック結果をCO0012UPDtblへ反映
            '　全レコードへKey情報をセット
            For Each CO0012UPDrow As DataRow In CO0012UPDtbl.Rows
                'キー項目(USERID)
                CO0012UPDrow("TERMID") = CO0012INProw("TERMID")
                'キー項目(CAMPCODE)（任意）
                If IsNumeric(CO0012INProw("CAMPCODE")) Then
                    CO0012UPDrow("CAMPCODE") = Right("00" & CO0012INProw("CAMPCODE"), 2)
                Else
                    CO0012UPDrow("CAMPCODE") = CO0012INProw("CAMPCODE")
                End If
                CO0012UPDrow("CAMPNAMES") = CO0012INProw("CAMPNAMES")
                'キー項目(オブジェクト)
                CO0012UPDrow("OBJECT") = CO0012INProw("OBJECT")
                CO0012UPDrow("OBJECTNAMES") = CO0012INProw("OBJECTNAMES")
                'キー項目(ROLE)
                CO0012UPDrow("ROLE") = CO0012INProw("ROLE")
                CO0012UPDrow("PERMITCODE") = CO0012INProw("PERMITCODE")
                'キー項目(SEQ)
                CO0012UPDrow("SEQ") = 1
                'キー項目(STYMD)
                CO0012UPDrow("STYMD") = CO0012INProw("STYMD")
                'キー項目(ENDYMD)
                CO0012UPDrow("ENDYMD") = CO0012INProw("ENDYMD")

                '一般項目 ロール名（短）　…　デフォルト値
                CO0012UPDrow("CODENAMES") = CO0012INProw("CODENAMES")
                '一般項目 ロール名（長）　…　デフォルト値
                CO0012UPDrow("CODENAMEL") = CO0012INProw("CODENAMEL")

                '一般項目(DELFLG)　…　デフォルト値
                CO0012UPDrow("DELFLG") = CO0012INProw("DELFLG")
            Next
        Next

        '■■■ 関連チェック ■■■
        '○主キー情報重複チェック(GridView表示内容とのチェック)
        For Each CO0012UPDrow As DataRow In CO0012UPDtbl.Rows
            For Each CO0012row As DataRow In CO0012tbl.Rows
                If CO0012row("DELFLG") <> C_DELETE_FLG.DELETE Then
                    '日付以外の項目が等しい
                    If CO0012row("TERMID") = CO0012UPDrow("TERMID") AndAlso
                       CO0012row("CAMPCODE") = CO0012UPDrow("CAMPCODE") AndAlso
                       CO0012row("OBJECT") = CO0012UPDrow("OBJECT") AndAlso
                       CO0012row("ROLE") = CO0012UPDrow("ROLE") Then
                        'ENDYMDは変更扱い
                        '同一レコードなら処理しない
                        If CO0012row("STYMD") = CO0012UPDrow("STYMD") Then Exit For

                        Dim WW_DATE_ST As Date
                        Dim WW_DATE_END As Date
                        Dim WW_DATE As Date

                        'CO0012tbl(STYMD)
                        Try
                            Date.TryParse(CO0012UPDrow("STYMD"), WW_DATE_ST)
                            Date.TryParse(CO0012UPDrow("ENDYMD"), WW_DATE_END)
                            Date.TryParse(CO0012row("STYMD"), WW_DATE)

                            If WW_DATE_ST <= WW_DATE AndAlso WW_DATE <= WW_DATE_END Then
                                'KEY重複
                                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "STYMD-ENDYMD"

                                'エラーレポート編集
                                WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR, CO0012UPDrow)
                                O_RTN = C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR
                                WW_ERRLIST_ALL.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                WW_ERRLIST.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                Exit For
                            End If
                        Catch ex As Exception
                        End Try

                        'CO0012tbl(ENDYMD)
                        Try
                            Date.TryParse(CO0012UPDrow("STYMD"), WW_DATE_ST)
                            Date.TryParse(CO0012UPDrow("ENDYMD"), WW_DATE_END)
                            Date.TryParse(CO0012row("ENDYMD"), WW_DATE)

                            If WW_DATE_ST <= WW_DATE AndAlso WW_DATE <= WW_DATE_END Then
                                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "STYMD-ENDYMD"

                                'エラーレポート編集
                                WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR, CO0012UPDrow)
                                O_RTN = C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR
                                WW_ERRLIST_ALL.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                WW_ERRLIST.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                Exit For
                            End If
                        Catch ex As Exception
                        End Try

                        'CO0012UPDtbl(STYMD)
                        Try
                            Date.TryParse(CO0012UPDrow("STYMD"), WW_DATE_ST)
                            Date.TryParse(CO0012UPDrow("ENDYMD"), WW_DATE_END)
                            Date.TryParse(CO0012row("STYMD"), WW_DATE)

                            If WW_DATE_ST <= WW_DATE AndAlso WW_DATE <= WW_DATE_END Then
                                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "STYMD-ENDYMD"
                                'エラーレポート編集
                                WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR, CO0012UPDrow)
                                O_RTN = C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR
                                WW_ERRLIST_ALL.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                WW_ERRLIST.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                Exit For
                            End If
                        Catch ex As Exception
                        End Try

                        'CO0012UPDtbl(ENDYMD)
                        Try
                            Date.TryParse(CO0012UPDrow("STYMD"), WW_DATE_ST)
                            Date.TryParse(CO0012UPDrow("ENDYMD"), WW_DATE_END)
                            Date.TryParse(CO0012row("ENDYMD"), WW_DATE)

                            If WW_DATE_ST <= WW_DATE AndAlso WW_DATE <= WW_DATE_END Then
                                If WW_ERRLIST_ALL.Count = 0 Then O_RTN_Detail = "STYMD-ENDYMD"
                                'エラーレポート編集
                                WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR, CO0012UPDrow)
                                O_RTN = C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR
                                WW_ERRLIST_ALL.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                WW_ERRLIST.Add(C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR)
                                Exit For
                            End If
                        Catch ex As Exception
                        End Try
                    End If
                End If
            Next
        Next

        '○変更有無
        O_RTN_Action = "Insert"
        For Each CO0012UPDrow As DataRow In CO0012UPDtbl.Rows
            For Each CO0012row As DataRow In CO0012tbl.Rows
                'KEY項目が等しい(ENDYMD以外のKEYが同じ)
                If CO0012row("TERMID") = CO0012UPDrow("TERMID") AndAlso
                   CO0012row("CAMPCODE") = CO0012UPDrow("CAMPCODE") AndAlso
                   CO0012row("OBJECT") = CO0012UPDrow("OBJECT") AndAlso
                   CO0012row("ROLE") = CO0012UPDrow("ROLE") AndAlso
                   CO0012row("STYMD") = CO0012UPDrow("STYMD") Then
                    O_RTN_Action = "Update"
                    Exit For
                End If
            Next
        Next

        '変更なし判定
        If O_RTN_Action = "Update" AndAlso
            (isNormal(O_RTN) OrElse O_RTN = C_MESSAGE_NO.BOX_ERROR_EXIST OrElse O_RTN = C_MESSAGE_NO.UPDATE_DATA_RELATION_ERROR) Then

            Dim WW_UMU As String = "無"
            For i As Integer = 0 To CO0012UPDtbl.Rows.Count - 1
                For Each CO0012row As DataRow In CO0012tbl.Rows

                    '同一KEYレコード
                    If CO0012UPDtbl.Rows(i)("TERMID") = CO0012row("TERMID") AndAlso
                       CO0012UPDtbl.Rows(i)("CAMPCODE") = CO0012row("CAMPCODE") AndAlso
                       CO0012UPDtbl.Rows(i)("OBJECT") = CO0012row("OBJECT") AndAlso
                       CO0012UPDtbl.Rows(i)("ROLE") = CO0012row("ROLE") AndAlso
                       CO0012UPDtbl.Rows(i)("STYMD") = CO0012row("STYMD") Then
                        ' キーが一致した
                        If CO0012UPDtbl.Rows(i)("ENDYMD") = CO0012row("ENDYMD") AndAlso
                            CO0012UPDtbl.Rows(i)("CODENAMES") = CO0012row("CODENAMES") AndAlso
                            CO0012UPDtbl.Rows(i)("CODENAMEL") = CO0012row("CODENAMEL") AndAlso
                            Trim(CO0012UPDtbl.Rows(i)("DELFLG")) = CO0012row("DELFLG") Then
                            ' 内容が一致した
                        Else
                            ' 一致しないデータがあった
                            WW_UMU = "有"
                        End If

                        Exit For
                    End If
                Next

                '変更せずに更新ボタン押下
                If WW_UMU = "無" AndAlso i >= (CO0012UPDtbl.Rows.Count - 1) Then O_RTN_Action = ""
            Next
        End If

    End Sub


    ''' <summary>
    ''' CO0012tbl更新 
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <param name="O_RTN_Detail"></param>
    ''' <param name="IO_RTN_Action"></param>
    ''' <remarks></remarks>
    Protected Sub CO0012tbl_UPD(ByRef O_RTN As String, ByRef O_RTN_Detail As String, ByRef IO_RTN_Action As String)

        '■■■ 更新処理　■■■
        Select Case IO_RTN_Action

            Case "Insert"
                '項番カウント
                Dim WW_H_CNT As Integer = CO0012tbl.Rows.Count

                '操作表示クリア
                For Each CO0012row As DataRow In CO0012tbl.Rows
                    Select Case CO0012row("OPERATION")
                        Case C_LIST_OPERATION_CODE.NODISP
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End Select
                Next

                '追加処理
                For Each CO0012UPDrow As DataRow In CO0012UPDtbl.Rows
                    WW_H_CNT = WW_H_CNT + 1

                    Dim CO0012row As DataRow = CO0012tbl.NewRow

                    CO0012UPDrow("SELECT") = WW_H_CNT

                    CO0012row("LINECNT") = WW_H_CNT                                 'DBの固定フィールド
                    If isNormal(O_RTN) Then
                        CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING     'DBの固定フィールド
                    Else
                        CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED      'DBの固定フィールド
                    End If
                    CO0012row("TIMSTP") = 0                                         'DBの固定フィールド
                    CO0012row("SELECT") = CO0012UPDrow("SELECT")                    'DBの固定フィールド
                    CO0012row("HIDDEN") = CO0012UPDrow("HIDDEN")                    'DBの固定フィールド

                    CO0012row("TERMID") = CO0012UPDrow("TERMID")

                    CO0012row("OBJECT") = CO0012UPDrow("OBJECT")
                    CO0012row("OBJECTNAMES") = CO0012UPDrow("OBJECTNAMES")

                    CO0012row("CAMPCODE") = CO0012UPDrow("CAMPCODE")
                    CO0012row("CAMPNAMES") = CO0012UPDrow("CAMPNAMES")
                    CO0012row("ROLE") = CO0012UPDrow("ROLE")
                    CO0012row("PERMITCODE") = CO0012UPDrow("PERMITCODE")
                    CO0012row("STYMD") = CO0012UPDrow("STYMD")
                    CO0012row("ENDYMD") = CO0012UPDrow("ENDYMD")
                    CO0012row("CODENAMES") = CO0012UPDrow("CODENAMES")
                    CO0012row("CODENAMEL") = CO0012UPDrow("CODENAMEL")
                    If CO0012UPDrow("DELFLG") = "" Then
                        CO0012row("DELFLG") = C_DELETE_FLG.ALIVE
                    Else
                        CO0012row("DELFLG") = CO0012UPDrow("DELFLG")
                    End If

                    CO0012row("SEQ") = CO0012UPDrow("SEQ")
                    CO0012tbl.Rows.Add(CO0012row)
                Next

            Case "Update"

                '操作表示クリア
                For Each CO0012row As DataRow In CO0012tbl.Rows
                    Select Case CO0012row("OPERATION")
                        Case C_LIST_OPERATION_CODE.NODISP
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End Select
                Next

                '更新処理
                For Each CO0012UPDrow As DataRow In CO0012UPDtbl.Rows
                    For Each CO0012row As DataRow In CO0012tbl.Rows
                        '同一(ENDYMD以外が同一KEY)レコード
                        If CO0012UPDrow("TERMID") = CO0012row("TERMID") AndAlso
                           CO0012UPDrow("OBJECT") = CO0012row("OBJECT") AndAlso
                           CO0012UPDrow("CAMPCODE") = CO0012row("CAMPCODE") AndAlso
                           CO0012UPDrow("ROLE") = CO0012row("ROLE") AndAlso
                           CO0012UPDrow("STYMD") = CO0012row("STYMD") Then

                            If isNormal(O_RTN) Then
                                CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING         'DBの固定フィールド
                            Else
                                CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED          'DBの固定フィールド
                            End If
                            'CO0012row("TIMSTP") = CO0012row("TIMSTP")                          'DBの固定フィールド
                            CO0012row("SELECT") = CO0012UPDrow("SELECT")                        'DBの固定フィールド
                            CO0012row("HIDDEN") = CO0012UPDrow("HIDDEN")                        'DBの固定フィールド
                            CO0012row("OBJECTNAMES") = CO0012UPDrow("OBJECTNAMES")
                            CO0012row("CAMPNAMES") = CO0012UPDrow("CAMPNAMES")
                            CO0012row("CODENAMES") = CO0012UPDrow("CODENAMES")
                            CO0012row("CODENAMEL") = CO0012UPDrow("CODENAMEL")
                            ' 権限必要
                            CO0012row("PERMITCODE") = CO0012UPDrow("PERMITCODE")
                            If String.IsNullOrEmpty(CO0012UPDrow("DELFLG")) Then
                                CO0012row("DELFLG") = C_DELETE_FLG.ALIVE
                            Else
                                CO0012row("DELFLG") = CO0012UPDrow("DELFLG")
                            End If
                            CO0012row("ENDYMD") = CO0012UPDrow("ENDYMD")
                        End If
                    Next
                Next
            Case Else
                '操作表示クリア
                For Each CO0012row As DataRow In CO0012tbl.Rows
                    Select Case CO0012row("OPERATION")
                        Case C_LIST_OPERATION_CODE.NODISP
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                            CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    End Select
                Next
        End Select

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
        For Each CO0012row As DataRow In CO0012tbl.Rows
            '読み飛ばし
            If (CO0012row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                CO0012row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                CO0012row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                CO0012row("STYMD") = "" Then
                Continue For
            End If

            WW_LINEERR_SW = ""

            'チェック
            For Each checkRow As DataRow In CO0012tbl.Rows

                '同一KEY以外は読み飛ばし

                If CO0012row("TERMID") = checkRow("TERMID") AndAlso
                   CO0012row("OBJECT") = checkRow("OBJECT") AndAlso
                   CO0012row("CAMPCODE") = checkRow("CAMPCODE") AndAlso
                   CO0012row("ROLE") = checkRow("ROLE") AndAlso
                   CO0012row("STYMD") = checkRow("STYMD") AndAlso
                   checkRow("DELFLG") <> C_DELETE_FLG.DELETE Then
                Else
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If CO0012row("STYMD") = checkRow("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(CO0012row("STYMD"), WW_DATE_ST)
                    Date.TryParse(CO0012row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(checkRow("STYMD"), WW_DATE_ST2)
                    Date.TryParse(checkRow("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                End Try

                '開始日チェック
                If (WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

                '終了日チェック
                If (WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2) Then
                    WW_CheckMES1 = "・エラー(期間重複)が存在します。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, CO0012row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINEERR_SW = "ERR"
                    Exit For
                End If

            Next

            If WW_LINEERR_SW = "" Then
                CO0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                CO0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub
    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0012S Then          '条件画面からの画面遷移
            If IsNothing(Master.MAPID) Then Master.MAPID = GRCO0012WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_CAMPCODE"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, ByVal I_CAMPCODE As String)

        '○名称取得
        O_TEXT = ""
        O_RTN = ""

        If Not String.IsNullOrEmpty(I_VALUE) Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"     '会社名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCompParam())
                    Case "TERMID"       '端末名
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_TERM, I_VALUE, O_TEXT, O_RTN, work.CreateTERMIDParam())
                    Case "SRVOBJECT"    'サーバのオブジェクト
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_CAMPCODE, "SRVOBJECT"))
                    Case "OBJECT"       'サーバのオブジェクト
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_CAMPCODE, "SRVOBJECT"))
                    Case "ROLE"         'サーバのロール
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.CreateRoleParam(I_CAMPCODE, "SRVOBJECT", WF_OBJECT.Text, True))
                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(I_CAMPCODE, "DELFLG"))
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
    ''' <param name="CO0012INProw"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByRef I_MESSAGE1 As String, ByRef I_MESSAGE2 As String, ByVal I_ERRCD As String, ByVal CO0012INProw As DataRow)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = I_MESSAGE1
        If I_MESSAGE2 <> "" Then
            WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> " & I_MESSAGE2 & " , "
        End If
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> TERMID=" & CO0012INProw("TERMID") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> CAMPCODE=" & CO0012INProw("CAMPCODE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> OBJECT=" & CO0012INProw("OBJECT") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ROLE=" & CO0012INProw("ROLE") & " , "
        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> SEQ=" & CO0012INProw("SEQ") & " "
        rightview.addErrorReport(WW_ERR_MES)

    End Sub

End Class

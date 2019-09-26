Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 届先部署マスタ入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0007TODKORG
    Inherits Page

    '○ 検索結果格納Table
    Private MC0007tbl As DataTable                          '一覧格納用テーブル
    Private MC0007INPtbl As DataTable                       'チェック用テーブル
    Private MC0007UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

    ''' <summary>
    ''' サーバー処理の遷移先
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
                    If Master.RecoverTable(MC0007tbl) Then
                        '○ 画面の情報反映
                        WF_TableChange()
                    Else
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
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
            If Not IsNothing(MC0007tbl) Then
                MC0007tbl.Clear()
                MC0007tbl.Dispose()
                MC0007tbl = Nothing
            End If

            If Not IsNothing(MC0007INPtbl) Then
                MC0007INPtbl.Clear()
                MC0007INPtbl.Dispose()
                MC0007INPtbl = Nothing
            End If

            If Not IsNothing(MC0007UPDtbl) Then
                MC0007UPDtbl.Clear()
                MC0007UPDtbl.Dispose()
                MC0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMC0007WRKINC.MAPID

        WF_LeftMViewChange.Value = ""
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()
        rightview.ResetIndex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0007S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MC0007tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MC0007tbl)

        TBLview.RowFilter = "LINECNT >= 1"

        '○ 一覧表示データ編集(性能対策)
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(MC0007tbl) Then
            MC0007tbl = New DataTable
        End If

        If MC0007tbl.Columns.Count <> 0 Then
            MC0007tbl.Columns.Clear()
        End If

        MC0007tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("              0                                         as LINECNT            ")
        SQLStr.AppendLine("            , ''                                        as OPERATION          ")
        SQLStr.AppendLine("            , CAST(ISNULL(MC07.UPDTIMSTP, 0) AS bigint) as TIMSTP             ")
        SQLStr.AppendLine("            , 1                                         as 'SELECT'           ")
        SQLStr.AppendLine("            , 0                                         as HIDDEN             ")
        SQLStr.AppendLine("            , isnull(rtrim(S006.CAMPCODE),'')           as CAMPCODE           ")
        SQLStr.AppendLine("            , ''                                        as CAMPNAMES          ")
        'SQLStr.AppendLine("            , isnull(rtrim(S006.CODE),'')               as ORGCODE            ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.UORG),'')               as UORG               ")
        SQLStr.AppendLine("            , ''                                        as UORGNAMES          ")
        SQLStr.AppendLine("            , isnull(rtrim(MC06.TORICODE),'')           as TORICODE           ")
        SQLStr.AppendLine("            , ''                                        as TORINAMES          ")
        SQLStr.AppendLine("            , isnull(rtrim(MC06.TODOKECODE),'')         as TODOKECODE         ")
        SQLStr.AppendLine("            , isnull(rtrim(MC06.NAMES),'')              as TODOKENAMES        ")
        SQLStr.AppendLine("            , isnull(rtrim(MC06.NAMESK),'')             as TODOKEKANA         ")
        SQLStr.AppendLine("            , rtrim(MC07.ARRIVTIME)                     as ARRIVTIME          ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.DISTANCE),'')           as DISTANCE           ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.SEQ),'')                as SEQ                ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.YTODOKECODE),'')        as YTODOKECODE        ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.JSRTODOKECODE),'')      as JSRTODOKECODE      ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.SHUKABASHO),'')         as SHUKABASHO         ")
        SQLStr.AppendLine("            , ''                                        as SHUKABASHONAMES    ")
        SQLStr.AppendLine("            , isnull(rtrim(MC07.DELFLG),'1')            as DELFLG             ")
        SQLStr.AppendLine("            , CASE WHEN MC07.UORG IS NULL THEN '02'                           ")
        SQLStr.AppendLine("                                          ELSE '01' END as ORGUSE             ")
        SQLStr.AppendLine(" FROM       MC006_TODOKESAKI MC06                                             ")
        SQLStr.AppendLine(" INNER JOIN S0006_ROLE S006                                                   ")
        SQLStr.AppendLine("         ON S006.CAMPCODE    = @CAMPCODE                                      ")
        SQLStr.AppendLine("        and S006.OBJECT      = @OBJECT                                        ")
        SQLStr.AppendLine("        and S006.ROLE        = @ROLE                                          ")
        SQLStr.AppendLine("        and S006.CODE        = @UORG                                          ")
        SQLStr.AppendLine("        and S006.STYMD      <= @STYMD                                         ")
        SQLStr.AppendLine("        and S006.ENDYMD     >= @STYMD                                         ")
        SQLStr.AppendLine("        and S006.DELFLG     <> @DELFLG                                        ")
        SQLStr.AppendLine("  LEFT JOIN MC007_TODKORG MC07                                                ")
        SQLStr.AppendLine("         ON MC07.CAMPCODE    = S006.CAMPCODE                                  ")
        SQLStr.AppendLine("        and MC07.TORICODE    = MC06.TORICODE                                  ")
        SQLStr.AppendLine("        and MC07.TODOKECODE  = MC06.TODOKECODE                                ")
        SQLStr.AppendLine("        and MC07.UORG        = S006.CODE                                      ")
        SQLStr.AppendLine("        and MC07.DELFLG     <> @DELFLG                                        ")
        SQLStr.AppendLine("  WHERE     MC06.CAMPCODE    = @CAMPCODE                                      ")
        SQLStr.AppendLine("        and MC06.STYMD      <= @STYMD                                         ")
        SQLStr.AppendLine("        and MC06.ENDYMD     >= @ENDYMD                                        ")
        SQLStr.AppendLine("        and MC06.DELFLG     <> @DELFLG                                        ")
        SQLStr.AppendLine("  ORDER BY S006.CAMPCODE ASC , ORGUSE ASC , MC07.UORG , MC07.SEQ ASC , MC06.NAMESK ")

        Try
            Using SQLcmd As New SqlCommand(SQLStr.ToString(), SQLcon)
                Dim ParmCampCode As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar)       '会社コード
                Dim ParmUorg As SqlParameter = SQLcmd.Parameters.Add("@UORG", SqlDbType.NVarChar)               '運用部署
                Dim ParmObject As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", SqlDbType.NVarChar)           'オブジェクト
                Dim ParmRole As SqlParameter = SQLcmd.Parameters.Add("@ROLE", SqlDbType.NVarChar)               'ロール
                Dim ParmStYmd As SqlParameter = SQLcmd.Parameters.Add("@STYMD", SqlDbType.Date)                 '現在日付
                Dim ParmEndYmd As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", SqlDbType.Date)               '現在日付-1月初日
                Dim ParmDelflg As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)           '削除フラグ

                ParmCampCode.Value = work.WF_SEL_CAMPCODE.Text
                ParmUorg.Value = work.WF_SEL_UORG.Text
                ParmObject.Value = C_ROLE_VARIANT.USER_ORG
                ParmRole.Value = Master.ROLE_ORG
                ParmStYmd.Value = Date.Now
                ParmEndYmd.Value = Convert.ToDateTime(Date.Now.AddMonths(-1).ToString("yyyy/MM") & "/01")
                ParmDelflg.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MC0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MC0007tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each MC0007row As DataRow In MC0007tbl.Rows

                    '光英JX届先コードは対象外(件数が多くて表示できない)
                    If Left(MC0007row("TODOKECODE"), 2) = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX Then
                        MC0007row("HIDDEN") = 1
                    End If

                    '届先コードCOSMOも対象外
                    If Left(MC0007row("TODOKECODE"), 5) = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO Then
                        MC0007row("HIDDEN") = 1
                    End If

                    '届組織登録レコードまたは指定取引先のみ表示
                    If MC0007row("UORG") = "" Then
                        If MC0007row("TORICODE") = work.WF_SEL_TORICODE.Text Then
                        Else
                            MC0007row("HIDDEN") = 1
                        End If
                    End If

                    '○追加レコード編集
                    If MC0007row("UORG") = "" Then
                        MC0007row("DELFLG") = 1
                        MC0007row("UORG") = work.WF_SEL_UORG.Text
                    End If

                    If MC0007row("HIDDEN") = 1 Then
                        Continue For
                    End If

                    i += 1
                    MC0007row("LINECNT") = i        'LINECNT

                    If IsDBNull(MC0007row("ARRIVTIME")) Then
                        MC0007row("ARRIVTIME") = "0:00:00"
                    Else
                        Try
                            Dim WW_TIME As DateTime
                            Date.TryParse(MC0007row("ARRIVTIME"), WW_TIME)
                            MC0007row("ARRIVTIME") = WW_TIME.ToString("H:mm:ss")

                        Catch ex As Exception
                            MC0007row("ARRIVTIME") = "0:00:00"
                        End Try
                    End If

                    '名称取得
                    CODENAME_get("CAMPCODE", MC0007row("CAMPCODE"), MC0007row("CAMPNAMES"), WW_DUMMY)                   '会社コード
                    CODENAME_get("UORG", MC0007row("UORG"), MC0007row("UORGNAMES"), WW_DUMMY)                           '運用部署
                    CODENAME_get("TORICODE", MC0007row("TORICODE"), MC0007row("TORINAMES"), WW_DUMMY)                   '取引先会社コード
                    CODENAME_get("SHUKABASHO", MC0007row("SHUKABASHO"), MC0007row("SHUKABASHONAMES"), WW_DUMMY)         '出荷場所
                Next

                Dim query = From dr In MC0007tbl Where dr.Item("HIDDEN") = "0"
                If query.Count > 0 Then
                    MC0007tbl = query.CopyToDataTable
                Else
                    MC0007tbl.Clear()
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC007_TODKORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC007_TODKORG Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each MC0007row As DataRow In MC0007tbl.Rows
            If MC0007row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MC0007row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定
        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(MC0007tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = C_DEFAULT_DATAKEY
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをtblへ退避
        DetailBoxToMC0007tbl()

        '○ 項目チェック
        TableCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '届先部署マスタ更新
                UpdateCustomerORGMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MC0007tbl)

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMC0007tbl()

        For i As Integer = 0 To MC0007tbl.Rows.Count - 1

            '使用有無
            MC0007tbl.Rows(i)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & (i + 1)))
            Select Case MC0007tbl.Rows(i)("ORGUSE")
                Case "01"       '使用
                    If MC0007tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.ALIVE Then
                        MC0007tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE
                        MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                Case "02"       '未使用
                    If MC0007tbl.Rows(i)("DELFLG") <> C_DELETE_FLG.DELETE Then
                        MC0007tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                        MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
            End Select

            '所要時間
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("ARRIVTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & (i + 1))) Then
                MC0007tbl.Rows(i)("ARRIVTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("ARRIVTIME"))

            '配送距離（配車用）
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("DISTANCE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (i + 1))) Then
                MC0007tbl.Rows(i)("DISTANCE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("DISTANCE"))

            '矢崎車端用届先コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("YTODOKECODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & (i + 1))) Then
                MC0007tbl.Rows(i)("YTODOKECODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("YTODOKECODE"))

            'JSR届先コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("JSRTODOKECODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & (i + 1))) Then
                MC0007tbl.Rows(i)("JSRTODOKECODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("JSRTODOKECODE"))

            '出荷場所
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("SHUKABASHO") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & (i + 1))) Then
                MC0007tbl.Rows(i)("SHUKABASHO") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("SHUKABASHO"))

            'SEQ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) AndAlso
                MC0007tbl.Rows(i)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) Then
                MC0007tbl.Rows(i)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1)))
                MC0007tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.EraseCharToIgnore(MC0007tbl.Rows(i)("SEQ"))
        Next

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub TableCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each MC0007row As DataRow In MC0007tbl.Rows

            '変更していない明細は飛ばす
            If MC0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                Continue For
            End If

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MC0007row("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MC0007row("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MC0007row("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MC0007row("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MC0007row("UORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署更新権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '所要時間
            WW_TEXT = MC0007row("ARRIVTIME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRIVTIME", MC0007row("ARRIVTIME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MC0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MC0007row("ARRIVTIME") = ""
                Else
                    Try
                        Dim WW_TIME As DateTime
                        Date.TryParse(MC0007row("ARRIVTIME"), WW_TIME)
                        MC0007row("ARRIVTIME") = WW_TIME.ToString("H:mm:ss")

                    Catch ex As Exception
                        MC0007row("ARRIVTIME") = "0:00:00"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(所要時間エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '配送距離（配車用）
            WW_TEXT = MC0007row("DISTANCE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DISTANCE", MC0007row("DISTANCE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MC0007row("DISTANCE") = ""
                Else
                    Try
                        MC0007row("DISTANCE") = Format(CInt(MC0007row("DISTANCE")), "#0")
                    Catch ex As Exception
                        MC0007row("DISTANCE") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(配送距離（配車用）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用届先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YTODOKECODE", MC0007row("YTODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用届先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JSR届先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JSRTODOKECODE", MC0007row("JSRTODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR届先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '出荷場所
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", MC0007row("SHUKABASHO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("SHUKABASHO", MC0007row("SHUKABASHO"), WW_DUMMY, WW_RTN_SW, MC0007row("TORICODE"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MC0007row("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MC0007row("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MC0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MC0007row("SEQ") = ""
                Else
                    Try
                        MC0007row("SEQ") = Format(CInt(MC0007row("SEQ")), "#0")
                    Catch ex As Exception
                        MC0007row("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR <> "" Then
                MC0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 届先部署マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateCustomerORGMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As New StringBuilder
        SQLStr.AppendLine(" DECLARE @hensuu as bigint ;                         ")
        SQLStr.AppendLine(" set @hensuu = 0 ;                                   ")
        SQLStr.AppendLine(" DECLARE hensuu CURSOR FOR                           ")
        SQLStr.AppendLine("   SELECT CAST(UPDTIMSTP as bigint) as hensuu        ")
        SQLStr.AppendLine("     FROM MC007_TODKORG                              ")
        SQLStr.AppendLine("     WHERE    CAMPCODE      = @CAMPCODE              ")
        SQLStr.AppendLine("       and    TORICODE      = @TORICODE              ")
        SQLStr.AppendLine("       and    TODOKECODE    = @TODOKECODE            ")
        SQLStr.AppendLine("       and    UORG          = @UORG ;                ")
        SQLStr.AppendLine("                                                     ")
        SQLStr.AppendLine(" OPEN hensuu ;                                       ")
        SQLStr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;               ")
        SQLStr.AppendLine(" IF ( @@FETCH_STATUS = 0 )                           ")
        SQLStr.AppendLine("    UPDATE MC007_TODKORG                             ")
        SQLStr.AppendLine("       SET    ARRIVTIME     = @ARRIVTIME ,           ")
        SQLStr.AppendLine("              DISTANCE      = @DISTANCE ,            ")
        SQLStr.AppendLine("              SEQ           = @SEQ ,                 ")
        SQLStr.AppendLine("              YTODOKECODE   = @YTODOKECODE ,         ")
        SQLStr.AppendLine("              JSRTODOKECODE = @JSRTODOKECODE ,       ")
        SQLStr.AppendLine("              SHUKABASHO    = @SHUKABASHO ,          ")
        SQLStr.AppendLine("              DELFLG        = @DELFLG ,              ")
        SQLStr.AppendLine("              UPDYMD        = @UPDYMD ,              ")
        SQLStr.AppendLine("              UPDUSER       = @UPDUSER ,             ")
        SQLStr.AppendLine("              UPDTERMID     = @UPDTERMID ,           ")
        SQLStr.AppendLine("              RECEIVEYMD    = @RECEIVEYMD            ")
        SQLStr.AppendLine("     WHERE    CAMPCODE      = @CAMPCODE              ")
        SQLStr.AppendLine("       and    TORICODE      = @TORICODE              ")
        SQLStr.AppendLine("       and    TODOKECODE    = @TODOKECODE            ")
        SQLStr.AppendLine("       and    UORG          = @UORG ;                ")
        SQLStr.AppendLine(" IF ( @@FETCH_STATUS <> 0 )                          ")
        SQLStr.AppendLine("    INSERT INTO MC007_TODKORG                        ")
        SQLStr.AppendLine("            ( CAMPCODE ,                             ")
        SQLStr.AppendLine("              TORICODE ,                             ")
        SQLStr.AppendLine("              TODOKECODE ,                           ")
        SQLStr.AppendLine("              UORG ,                                 ")
        SQLStr.AppendLine("              ARRIVTIME ,                            ")
        SQLStr.AppendLine("              DISTANCE ,                             ")
        SQLStr.AppendLine("              SEQ ,                                  ")
        SQLStr.AppendLine("              YTODOKECODE ,                          ")
        SQLStr.AppendLine("              JSRTODOKECODE ,                        ")
        SQLStr.AppendLine("              SHUKABASHO ,                           ")
        SQLStr.AppendLine("              DELFLG ,                               ")
        SQLStr.AppendLine("              INITYMD ,                              ")
        SQLStr.AppendLine("              UPDYMD ,                               ")
        SQLStr.AppendLine("              UPDUSER ,                              ")
        SQLStr.AppendLine("              UPDTERMID ,                            ")
        SQLStr.AppendLine("              RECEIVEYMD )                           ")
        SQLStr.AppendLine("      VALUES (@CAMPCODE,                             ")
        SQLStr.AppendLine("              @TORICODE,                             ")
        SQLStr.AppendLine("              @TODOKECODE,                           ")
        SQLStr.AppendLine("              @UORG,                                 ")
        SQLStr.AppendLine("              @ARRIVTIME,                            ")
        SQLStr.AppendLine("              @DISTANCE,                             ")
        SQLStr.AppendLine("              @SEQ,                                  ")
        SQLStr.AppendLine("              @YTODOKECODE,                          ")
        SQLStr.AppendLine("              @JSRTODOKECODE,                        ")
        SQLStr.AppendLine("              @SHUKABASHO,                           ")
        SQLStr.AppendLine("              @DELFLG,                               ")
        SQLStr.AppendLine("              @INITYMD,                              ")
        SQLStr.AppendLine("              @UPDYMD,                               ")
        SQLStr.AppendLine("              @UPDUSER,                              ")
        SQLStr.AppendLine("              @UPDTERMID,                            ")
        SQLStr.AppendLine("              @RECEIVEYMD);                          ")
        SQLStr.AppendLine(" CLOSE hensuu ;                                      ")
        SQLStr.AppendLine(" DEALLOCATE hensuu ;                                 ")

        '○ 更新ジャーナル出力
        Dim SQLJnl As New StringBuilder
        SQLJnl.AppendLine(" SELECT                                                                                ")
        SQLJnl.AppendLine("      rtrim(CAMPCODE) as CAMPCODE                                                      ")
        SQLJnl.AppendLine("    , rtrim(TORICODE) as TORICODE                                                      ")
        SQLJnl.AppendLine("    , rtrim(TODOKECODE) as TODOKECODE                                                  ")
        SQLJnl.AppendLine("    , rtrim(UORG) as UORG                                                              ")
        SQLJnl.AppendLine("    , convert(nvarchar,format(convert(datetime,ARRIVTIME),'H:mm:ss')) as ARRIVTIME     ")
        SQLJnl.AppendLine("    , rtrim(DISTANCE) as DISTANCE                                                      ")
        SQLJnl.AppendLine("    , SEQ as SEQ                                                                       ")
        SQLJnl.AppendLine("    , rtrim(YTODOKECODE) as YTODOKECODE                                                ")
        SQLJnl.AppendLine("    , rtrim(JSRTODOKECODE) as JSRTODOKECODE                                            ")
        SQLJnl.AppendLine("    , rtrim(SHUKABASHO) as SHUKABASHO                                                  ")
        SQLJnl.AppendLine("    , DELFLG                                                                           ")
        SQLJnl.AppendLine("    , INITYMD                                                                          ")
        SQLJnl.AppendLine("    , UPDYMD                                                                           ")
        SQLJnl.AppendLine("    , UPDUSER                                                                          ")
        SQLJnl.AppendLine("    , UPDTERMID                                                                        ")
        SQLJnl.AppendLine("    , RECEIVEYMD                                                                       ")
        SQLJnl.AppendLine("    , CAST(UPDTIMSTP AS bigint) TIMSTP                                                 ")
        SQLJnl.AppendLine(" FROM                                                                                  ")
        SQLJnl.AppendLine("    MC007_TODKORG                                                                      ")
        SQLJnl.AppendLine(" WHERE                                                                                 ")
        SQLJnl.AppendLine("        CAMPCODE     = @CAMPCODE                                                       ")
        SQLJnl.AppendLine("    AND TORICODE     = @TORICODE                                                       ")
        SQLJnl.AppendLine("    AND TODOKECODE   = @TODOKECODE                                                     ")
        SQLJnl.AppendLine("    AND UORG         = @UORG                                                           ")

        Try
            Using SQLcmd As New SqlCommand(SQLStr.ToString(), SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl.ToString(), SQLcon)
                Dim ParmCampCode As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar)                   '会社コード
                Dim ParmToriCode As SqlParameter = SQLcmd.Parameters.Add("@TORICODE", SqlDbType.NVarChar)                   '取引先コード
                Dim ParmTodokeCode As SqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", SqlDbType.NVarChar)               '届先コード
                Dim ParmUorg As SqlParameter = SQLcmd.Parameters.Add("@UORG", SqlDbType.NVarChar)                           '運用部署
                Dim ParmArrivTime As SqlParameter = SQLcmd.Parameters.Add("@ARRIVTIME", SqlDbType.Time)                     '所要時間
                Dim ParmDistance As SqlParameter = SQLcmd.Parameters.Add("@DISTANCE", SqlDbType.NVarChar)                   '配送距離（配車用）
                Dim ParmSeq As SqlParameter = SQLcmd.Parameters.Add("@SEQ", SqlDbType.Int)                                  '表示順番
                Dim ParmYtodokeCode As SqlParameter = SQLcmd.Parameters.Add("@YTODOKECODE", SqlDbType.NVarChar)             '矢崎車端用届先コード
                Dim ParmJsrTodokeCode As SqlParameter = SQLcmd.Parameters.Add("@JSRTODOKECODE", SqlDbType.NVarChar)         'JSR届先コード
                Dim ParmShukabasho As SqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", SqlDbType.NVarChar)               '出荷場所
                Dim ParmDelFlg As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)                       '削除フラグ
                Dim ParmInitYmd As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)                     '登録年月日
                Dim ParmUpdYmd As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)                       '更新年月日
                Dim ParmUpdUser As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)                     '更新ユーザーID
                Dim ParmUpdTermId As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)                 '更新端末
                Dim ParmReceiveYmd As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)               '集信日時

                Dim JParmCampCode As SqlParameter = SQLcmdJnl.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar)               '会社コード
                Dim JParmToriCode As SqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", SqlDbType.NVarChar)               '取引先コード
                Dim JParmTodokeCode As SqlParameter = SQLcmdJnl.Parameters.Add("@TODOKECODE", SqlDbType.NVarChar)           '届先コード
                Dim JParmUorg As SqlParameter = SQLcmdJnl.Parameters.Add("@UORG", SqlDbType.NVarChar)                       '運用部署

                For Each MC0007row As DataRow In MC0007tbl.Rows
                    If Trim(MC0007row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING Then

                        '新規分で削除のレコードは作成しない
                        If MC0007row("TIMSTP") = 0 AndAlso MC0007row("DELFLG") = C_DELETE_FLG.DELETE Then
                            MC0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Continue For
                        End If

                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        ParmCampCode.Value = MC0007row("CAMPCODE")
                        ParmToriCode.Value = MC0007row("TORICODE")
                        ParmTodokeCode.Value = MC0007row("TODOKECODE")
                        ParmUorg.Value = MC0007row("UORG")
                        ParmArrivTime.Value = MC0007row("ARRIVTIME")
                        ParmDistance.Value = MC0007row("DISTANCE")
                        ParmSeq.Value = MC0007row("SEQ")
                        ParmYtodokeCode.Value = MC0007row("YTODOKECODE")
                        ParmJsrTodokeCode.Value = MC0007row("JSRTODOKECODE")
                        ParmShukabasho.Value = MC0007row("SHUKABASHO")
                        ParmDelFlg.Value = MC0007row("DELFLG")
                        ParmInitYmd.Value = WW_DATENOW
                        ParmUpdYmd.Value = WW_DATENOW
                        ParmUpdUser.Value = Master.USERID
                        ParmUpdTermId.Value = Master.USERTERMID
                        ParmReceiveYmd.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MC0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JParmCampCode.Value = MC0007row("CAMPCODE")
                        JParmToriCode.Value = MC0007row("TORICODE")
                        JParmTodokeCode.Value = MC0007row("TODOKECODE")
                        JParmUorg.Value = MC0007row("UORG")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MC0007UPDtbl) Then
                                MC0007UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MC0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MC0007UPDtbl.Clear()
                            MC0007UPDtbl.Load(SQLdr)
                        End Using

                        For Each MC0007UPDrow As DataRow In MC0007UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MC007_TODKORG"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MC0007UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC007_TODKORG UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC007_TODKORG UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MC0007tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
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

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(MC0007INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MC0007INProw As DataRow = MC0007INPtbl.NewRow

            '○ 初期クリア
            For Each MC0007INPcol As DataColumn In MC0007INPtbl.Columns
                If IsDBNull(MC0007INProw.Item(MC0007INPcol)) OrElse IsNothing(MC0007INProw.Item(MC0007INPcol)) Then
                    Select Case MC0007INPcol.ColumnName
                        Case "LINECNT"
                            MC0007INProw.Item(MC0007INPcol) = 0
                        Case "OPERATION"
                            MC0007INProw.Item(MC0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MC0007INProw.Item(MC0007INPcol) = 0
                        Case "SELECT"
                            MC0007INProw.Item(MC0007INPcol) = 1
                        Case "HIDDEN"
                            MC0007INProw.Item(MC0007INPcol) = 0
                        Case Else
                            MC0007INProw.Item(MC0007INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("UORG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                For Each MC0007row As DataRow In MC0007tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MC0007row("CAMPCODE") AndAlso
                        XLSTBLrow("UORG") = MC0007row("UORG") AndAlso
                        XLSTBLrow("TORICODE") = MC0007row("TORICODE") AndAlso
                        XLSTBLrow("TODOKECODE") = MC0007row("TODOKECODE") Then
                        MC0007INProw.ItemArray = MC0007row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MC0007INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '運用部署
            If WW_COLUMNS.IndexOf("UORG") >= 0 Then
                MC0007INProw("UORG") = XLSTBLrow("UORG")
            End If

            '取引先コード
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                MC0007INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            '届先コード
            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                MC0007INProw("TODOKECODE") = XLSTBLrow("TODOKECODE")
            End If

            '所要時間
            If WW_COLUMNS.IndexOf("ARRIVTIME") >= 0 Then
                MC0007INProw("ARRIVTIME") = XLSTBLrow("ARRIVTIME")
            End If

            '配送距離（配車用）
            If WW_COLUMNS.IndexOf("DISTANCE") >= 0 Then
                MC0007INProw("DISTANCE") = XLSTBLrow("DISTANCE")
            End If

            '矢崎車端用届先コード
            If WW_COLUMNS.IndexOf("YTODOKECODE") >= 0 Then
                MC0007INProw("YTODOKECODE") = XLSTBLrow("YTODOKECODE")
            End If

            'JSR届先コード
            If WW_COLUMNS.IndexOf("JSRTODOKECODE") >= 0 Then
                MC0007INProw("JSRTODOKECODE") = XLSTBLrow("JSRTODOKECODE")
            End If

            '出荷場所
            If WW_COLUMNS.IndexOf("SHUKABASHO") >= 0 Then
                MC0007INProw("SHUKABASHO") = XLSTBLrow("SHUKABASHO")
            End If

            '表示順番
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MC0007INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MC0007INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            MC0007INPtbl.Rows.Add(MC0007INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MC0007tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MC0007tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        For Each MC0007INProw As DataRow In MC0007INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MC0007INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MC0007INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MC0007INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "UORG", MC0007INProw("UORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MC0007INProw("UORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MC0007INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "検索条件の運用部署と一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", MC0007INProw("TORICODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TORICODE", MC0007INProw("TORICODE"), MC0007INProw("TORINAMES"), WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取引先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '届先
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", MC0007INProw("TODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("TODOKECODE", MC0007INProw("TODOKECODE"), MC0007INProw("TODOKENAMES"), WW_RTN_SW, MC0007INProw("TORICODE"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(届先エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '所要時間
            WW_TEXT = MC0007INProw("ARRIVTIME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRIVTIME", MC0007INProw("ARRIVTIME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" AndAlso MC0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    MC0007INProw("ARRIVTIME") = ""
                Else
                    Try
                        Dim WW_TIME As DateTime
                        Date.TryParse(MC0007INProw("ARRIVTIME"), WW_TIME)
                        MC0007INProw("ARRIVTIME") = WW_TIME.ToString("H:mm:ss")

                    Catch ex As Exception
                        MC0007INProw("ARRIVTIME") = "0:00:00"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(所要時間エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '配送距離（配車用）
            WW_TEXT = MC0007INProw("DISTANCE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DISTANCE", MC0007INProw("DISTANCE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MC0007INProw("DISTANCE") = ""
                Else
                    Try
                        MC0007INProw("DISTANCE") = Format(CInt(MC0007INProw("DISTANCE")), "#0")
                    Catch ex As Exception
                        MC0007INProw("DISTANCE") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(配送距離（配車用）エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '矢崎車端用届先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "YTODOKECODE", MC0007INProw("YTODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(矢崎車端用届先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JSR届先コード
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JSRTODOKECODE", MC0007INProw("JSRTODOKECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JSR届先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '出荷場所
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHUKABASHO", MC0007INProw("SHUKABASHO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("SHUKABASHO", MC0007INProw("SHUKABASHO"), MC0007INProw("SHUKABASHONAMES"), WW_RTN_SW, MC0007INProw("TORICODE"))
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷場所エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'SEQ
            WW_TEXT = MC0007INProw("SEQ")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEQ", MC0007INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT = "" Then
                    MC0007INProw("SEQ") = ""
                Else
                    Try
                        MC0007INProw("SEQ") = Format(CInt(MC0007INProw("SEQ")), "#0")
                    Catch ex As Exception
                        MC0007INProw("SEQ") = "0"
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(表示順番)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '削除
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", MC0007INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MC0007INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MC0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MC0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MC0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If

        Next

    End Sub

    ''' <summary>
    ''' MC0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MC0007tbl_UPD()

        '○ 追加変更判定
        For Each MC0007INProw As DataRow In MC0007INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MC0007INProw("OPERATION") = "Insert"

            'KEY項目が等しい
            For Each MC0007row As DataRow In MC0007tbl.Rows
                If MC0007row("CAMPCODE") = MC0007INProw("CAMPCODE") AndAlso
                    MC0007row("UORG") = MC0007INProw("UORG") AndAlso
                    MC0007row("TORICODE") = MC0007INProw("TORICODE") AndAlso
                    MC0007row("TODOKECODE") = MC0007INProw("TODOKECODE") Then

                    '変更無は操作無
                    If MC0007row("ARRIVTIME") = MC0007INProw("ARRIVTIME") AndAlso
                        MC0007row("DISTANCE") = MC0007INProw("DISTANCE") AndAlso
                        MC0007row("YTODOKECODE") = MC0007INProw("YTODOKECODE") AndAlso
                        MC0007row("JSRTODOKECODE") = MC0007INProw("JSRTODOKECODE") AndAlso
                        MC0007row("SHUKABASHO") = MC0007INProw("SHUKABASHO") AndAlso
                        MC0007row("SEQ") = MC0007INProw("SEQ") AndAlso
                        MC0007row("DELFLG") = MC0007INProw("DELFLG") Then
                        MC0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MC0007INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MC0007INProw As DataRow In MC0007INPtbl.Rows
            Select Case MC0007INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MC0007INProw)
                Case "Insert"
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MC0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MC0007INProw As DataRow)

        For Each MC0007row As DataRow In MC0007tbl.Rows

            '同一レコード
            If MC0007INProw("CAMPCODE") = MC0007row("CAMPCODE") AndAlso
                MC0007INProw("UORG") = MC0007row("UORG") AndAlso
                MC0007INProw("TORICODE") = MC0007row("TORICODE") AndAlso
                MC0007INProw("TODOKECODE") = MC0007row("TODOKECODE") Then

                '画面入力テーブル項目設定
                MC0007INProw("LINECNT") = MC0007row("LINECNT")
                MC0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MC0007INProw("TIMSTP") = MC0007row("TIMSTP")
                MC0007INProw("SELECT") = 1
                MC0007INProw("HIDDEN") = 0

                '使用有無判定
                If MC0007INProw("DELFLG") = C_DELETE_FLG.ALIVE Then
                    MC0007INProw("ORGUSE") = "01"       '使用
                Else
                    MC0007INProw("ORGUSE") = "02"       '未使用
                End If

                '項目テーブル項目設定
                MC0007row.ItemArray = MC0007INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 一覧変更情報取込処理
    ''' </summary>
    Protected Sub WF_TableChange()

        For Each MC0007row As DataRow In MC0007tbl.Rows
            WF_SelectedIndex.Value = CStr(MC0007row("LINECNT"))
            WF_ListChange(False)
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MC0007tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        WF_ListChange(True)

    End Sub
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="isSaving">更新保存可否</param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(ByVal isSaving As Boolean)

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        If WW_LINECNT < 0 Then Exit Sub
        '○ 変更チェック
        '使用有無
        If MC0007tbl.Rows(WW_LINECNT)("ORGUSE") <> Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("ORGUSE") = Convert.ToString(Request.Form("ctl00$contents1$rblORGUSEORGUSE" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '所要時間
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("ARRIVTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("ARRIVTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ARRIVTIME" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '配送距離（配車用）
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("DISTANCE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("DISTANCE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '矢崎車端用届先コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("YTODOKECODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("YTODOKECODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YTODOKECODE" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'JSR届先コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("JSRTODOKECODE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("JSRTODOKECODE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRTODOKECODE" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '出荷場所
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("SHUKABASHO") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("SHUKABASHO") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKABASHO" & WF_SelectedIndex.Value))
            CODENAME_get("SHUKABASHO", MC0007tbl(WW_LINECNT)("SHUKABASHO"), MC0007tbl(WW_LINECNT)("SHUKABASHONAMES"), WW_DUMMY, MC0007tbl(WW_LINECNT)("TORICODE"))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'SEQ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) AndAlso
            MC0007tbl.Rows(WW_LINECNT)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) Then
            MC0007tbl.Rows(WW_LINECNT)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value))
            MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '○ 画面表示データ保存
        If isSaving Then Master.SaveTable(MC0007tbl)

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim WW_LINECNT As Integer = 0

            With leftview
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                Select Case WF_FIELD.Value
                    Case "SHUKABASHO"        '出荷場所

                        Dim toriCode As String = ""

                        '取引先コード
                        toriCode = MC0007tbl.Rows(WF_SelectLine.Value - 1)("TORICODE")

                        prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, toriCode)
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_DISTINATION
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            End With
        End If
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_LINECNT As Integer = 0
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectLine.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "SHUKABASHO"        '出荷場所
                If MC0007tbl.Rows(WW_LINECNT)("SHUKABASHO") <> WW_SelectValue Then
                    MC0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MC0007tbl.Rows(WW_LINECNT)("SHUKABASHO") = WW_SelectValue
                MC0007tbl.Rows(WW_LINECNT)("SHUKABASHONAMES") = WW_SelectText
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MC0007tbl)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "SHUKABASHO"        '出荷場所
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

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
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MC0007row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MC0007row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MC0007row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社       =" & MC0007row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用部署   =" & MC0007row("UORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先     =" & MC0007row("TORICODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先名称 =" & MC0007row("TORINAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 届先       =" & MC0007row("TODOKECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 届先名称   =" & MC0007row("TODOKENAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順番   =" & MC0007row("SEQ")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional toriCode As String = "")

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORICODE"         '取引先
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TODOKECODE"       '届先
                    prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, toriCode)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHUKABASHO"       '出荷場所
                    prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, toriCode)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

Imports System.Drawing
Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 事務員勤務入力（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRT00009JIMKINTAI
    Inherits Page

    '○ 検索結果格納Table
    Private T00009tbl As DataTable                          '一覧格納用テーブル
    Private T00009INPtbl As DataTable                       'チェック用テーブル
    Private T00009TSVtbl As DataTable                       '一時保存用テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0006TERMchk As New CS0006TERMchk              'コンピュータ名存在チェック
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0021PROFXLS As New CS0021PROFXLS              'プロファイル(帳票)取得
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0026TBLSORT As New CS0026TBLSORT              '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0033AutoNumber As New CS0033AutoNumber        '自動採番
    Private CS0038ACCODEget As New CS0038ACCODEget          '勘定科目取得
    Private CS0044L1INSERT As New CS0044L1INSERT            '統計DB出力
    Private CS0048Apploval As New CS0048Apploval            '承認管理
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    Private T0007COM As New GRT0007COM                      '勤怠共通
    Private T0007UPDATE As New GRT0007UPDATE                '勤怠DB更新

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_RTN_SW2 As String = ""
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
                    If Not Master.RecoverTable(T00009tbl, WF_XMLsaveF.Value) OrElse
                        Not Master.RecoverTable(T00009INPtbl, WF_XMLsaveF_INP.Value) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonCALC"            '残業再計算ボタン押下
                            WF_ButtonCALC_Click()
                        Case "WF_ButtonDOWN"            '前頁ボタン押下
                            WF_ButtonDOWN_Click()
                        Case "WF_ButtonUP"              '次頁ボタン押下
                            WF_ButtonUP_Click()
                        Case "WF_ButtonSAVE"            '一時保存ボタン押下
                            WF_ButtonSAVE_Click()
                        Case "WF_ButtonExtract"         '絞り込みボタン押下
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonPrint_Click("XLSX")
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click("pdf")
                        Case "WF_ButtonUPDATE2"         '更新ボタン押下
                            WF_ButtonUPDATE2_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_DtabChange"            '調整画面切り替え
                            WF_DtabChange()
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
                        Case "HELP"                     'ヘルプ表示
                            WF_HELP_Click()
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
            If Not IsNothing(T00009tbl) Then
                T00009tbl.Clear()
                T00009tbl.Dispose()
                T00009tbl = Nothing
            End If

            If Not IsNothing(T00009INPtbl) Then
                T00009INPtbl.Clear()
                T00009INPtbl.Dispose()
                T00009INPtbl = Nothing
            End If

            If Not IsNothing(T00009TSVtbl) Then
                T00009TSVtbl.Clear()
                T00009TSVtbl.Dispose()
                T00009TSVtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRT00009WRKINC.MAPID

        WF_SELSTAFFCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()
        rightview.resetindex()

        Dim WW_CheckMES As String = ""
        Dim WW_MSGNO As String = C_MESSAGE_NO.NORMAL

        '○ 画面の値設定
        WW_MAPValueSet(WW_CheckMES, WW_MSGNO)
        If Not isNormal(WW_MSGNO) Then
            Master.output(WW_MSGNO, C_MESSAGE_TYPE.ABORT)
            WW_CheckERR(WW_CheckMES, "")
        End If

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

        '○ 明細合計行設定
        DisplayTotal()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <param name="O_MSG"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet(ByRef O_MSG As String, ByRef O_RTN As String)

        O_MSG = ""
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_ERR_MSG As String = ""

        'Grid情報保存先のファイル名
        WF_XMLsaveF.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & _
            Master.USERID & "-" & Master.MAPID & "-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        WF_XMLsaveF_INP.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & _
            Master.USERID & "-" & Master.MAPID & "INP-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00009S Then          '検索画面からの遷移
            WF_BEFORE_MAPID.Value = GRT00009WRKINC.MAPIDS

            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                If work.WF_SEL_LIMITFLG.Text = "0" Then
                    If work.WF_SEL_PERMITCODE.Text = C_PERMISSION.UPDATE Then
                        '更新権限あり
                        WF_MAPpermitcode.Value = "TRUE"
                    Else
                        '更新権限なし
                        WF_MAPpermitcode.Value = "FALSE"
                        O_RTN = C_MESSAGE_NO.UPDATE_AUTHORIZATION_ERROR
                        WW_ERR_MSG = "・選択した配属部署は、更新権限がありません。"
                        O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
                    End If
                Else
                    '対象年月の締後は更新できない
                    WF_MAPpermitcode.Value = "FALSE"
                    O_RTN = C_MESSAGE_NO.OVER_CLOSING_DATE_ERROR
                    WW_ERR_MSG = "・勤怠締後は更新できません。"
                    O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
                End If
            Else
                '更新権限なし
                WF_MAPpermitcode.Value = "FALSE"
                O_RTN = C_MESSAGE_NO.UPDATE_AUTHORIZATION_ERROR
                WW_ERR_MSG = "・営業勤怠登録の更新権限がありません。"
                O_MSG = O_MSG & ControlChars.NewLine & WW_ERR_MSG
            End If

            '画面初期従業員設定
            Dim prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK, work.WF_SEL_CAMPCODE.Text,
                            work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
            leftview.setListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmData)
            For i As Integer = 0 To leftview.WF_LeftListBox.Items.Count - 1
                WF_STAFFCODE.Text = leftview.WF_LeftListBox.Items(i).Value
                Exit For
            Next
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00010 Then       '承認画面からの遷移
            WF_BEFORE_MAPID.Value = work.WF_T09_MAPID.Text
            Master.MAPvariant = work.WF_T09_MAPVARIANT.Text
            WF_MAPpermitcode.Value = "FALSE"

            '承認画面の状態を別枠(上書きしないよう)に保持しておく
            work.WF_T10_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text           '会社コード
            work.WF_T10_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text           '申請年月
            work.WF_T10_HORG.Text = work.WF_SEL_HORG.Text                   '配属部署

            '承認画面から情報を取得
            work.WF_SEL_CAMPCODE.Text = work.WF_T09_CAMPCODE.Text           '会社コード
            work.WF_SEL_TAISHOYM.Text = work.WF_T09_TAISHOYM.Text           '対象年月
            work.WF_SEL_HORG.Text = work.WF_T09_HORG.Text                   '配属部署
            work.WF_SEL_STAFFKBN.Text = work.WF_T09_STAFFKBN.Text           '社員区分
            work.WF_SEL_STAFFCODE.Text = work.WF_T09_STAFFCODE.Text         '従業員(コード)
            WF_STAFFCODE.Text = work.WF_T09_STAFFCODE.Text                  '従業員
            work.WF_SEL_STAFFNAMES.Text = work.WF_T09_STAFFNAME.Text        '従業員(名称)
            work.KintaiALLCheck(work.WF_SEL_CAMPCODE.Text, Master.USERID)
        End If

        '○ 勤怠個人
        If Master.MAPvariant Like GRT00009WRKINC.VAR_ALL Then
            WF_ONLY.Value = "FALSE"
        Else
            WF_ONLY.Value = "TRUE"
        End If

        'その他作業部署取得
        Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

        '○ 部署が新潟東港の場合、残業再計算ボタンを表示する
        If Not IsNothing(specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)) Then
            WF_ButtonCALC.Visible = True
        Else
            WF_ButtonCALC.Visible = False
        End If

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 再開ボタン押下時
        If work.WF_SEL_RESTARTFLG.Text = "TRUE" Then
            If Not Master.RecoverTable(T00009tbl, work.WF_SEL_XMLsaveTMP.Text) Then
                Exit Sub
            End If
        Else
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)

        Master.CreateEmptyTable(T00009INPtbl, WF_XMLsaveF.Value)

        '○ 初期画面の事務員分のデータを格納
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009INPtbl)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00009INPtbl)
        TBLview.RowFilter = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '0'"

        '○ 一部画面表示編集(00:00をブランクに変更)
        ZeroToBlank(TBLview)

        Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        If Not IsNothing(specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)) Then
            '新潟東港を選択している場合、強制的に画面を変更する
            CS0013ProfView.PROFID = C_DEFAULT_DATAKEY & "_" & (specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)).ToString
        Else
            CS0013ProfView.PROFID = Master.PROF_VIEW
        End If
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.NOCOLUMNWIDTHOPT = -1
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.WITHTAGNAMES = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

        '○ 曜日表示色変更
        WeekColorChange()

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        '対象年月
        If IsNothing(T00009tbl) Then
            T00009tbl = New DataTable
        End If

        If T00009tbl.Columns.Count <> 0 Then
            T00009tbl.Columns.Clear()
        End If

        T00009tbl.Clear()

        Dim T00009BEFtbl As DataTable = New DataTable       '前月分
        Dim T00009AFTtbl As DataTable = New DataTable       '翌月分

        Dim SQLcmd As New SqlCommand()

        '開始と終了の日付を準備
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Try
            Date.TryParse(work.WF_SEL_TAISHOYM.Text & "/01", WW_DATE_ST)
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        Catch ex As Exception
            WW_DATE_ST = Convert.ToDateTime(Date.Now.ToString("yyyy/MM") & "/01")
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        End Try

        Try
            '○ テンポラリーテーブルを作成する
            Dim SQLstr As String =
                  " CREATE TABLE #MBTemp" _
                & " (" _
                & "    CAMPCODE nvarchar(20)" _
                & "    , STAFFCODE nvarchar(20)" _
                & "    , HORG nvarchar(20)" _
                & " )"

            SQLcmd = New SqlCommand(SQLstr, SQLcon)
            SQLcmd.ExecuteNonQuery()

            '○ テンポラリーテーブル用のデータを取得する
            SQLstr =
                 " SELECT" _
                & "    ISNULL(RTRIM(MB01.CAMPCODE), '')    AS CAMPCODE" _
                & "    , ISNULL(RTRIM(MB01.STAFFCODE), '') AS STAFFCODE" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')      AS HORG" _
                & " FROM" _
                & "    MB001_STAFF MB01" _
                & "    INNER JOIN S0012_SRVAUTHOR S012" _
                & "        ON  S012.TERMID    = @P1" _
                & "        AND S012.CAMPCODE  = @P2" _
                & "        AND S012.OBJECT    = @P3" _
                & "        AND S012.STYMD    <= @P9" _
                & "        AND S012.ENDYMD   >= @P9" _
                & "        AND S012.DELFLG   <> @P10" _
                & "    INNER JOIN S0006_ROLE S006" _
                & "        ON  S006.CAMPCODE  = S012.CAMPCODE" _
                & "        AND S006.OBJECT    = @P3" _
                & "        AND S006.ROLE      = S012.ROLE" _
                & "        AND S006.STYMD    <= @P9" _
                & "        AND S006.ENDYMD   >= @P9" _
                & "        AND S006.DELFLG   <> @P10" _
                & "    INNER JOIN (" _
                & "            SELECT" _
                & "                ISNULL(RTRIM(CODE), '') AS CODE" _
                & "            FROM" _
                & "                M0006_STRUCT" _
                & "            WHERE" _
                & "                CAMPCODE     = @P2" _
                & "                AND OBJECT   = @P4" _
                & "                AND STRUCT   = @P5" _
                & "                AND GRCODE01 = @P6" _
                & "                AND STYMD   <= @P9" _
                & "                AND ENDYMD  >= @P9" _
                & "                AND DELFLG  <> @P10) M006" _
                & "        ON  M006.CODE      = S006.CODE" _
                & "        AND M006.CODE      = MB01.HORG" _
                & " WHERE" _
                & "    MB01.CAMPCODE      = @P2" _
                & "    AND MB01.STAFFKBN NOT LIKE '03%'" _
                & "    AND MB01.STYMD    <= @P7" _
                & "    AND MB01.ENDYMD   >= @P8" _
                & "    AND MB01.DELFLG   <> @P10"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '従業員(コード)
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFCODE.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFCODE = '{0}'", work.WF_SEL_STAFFCODE.Text)
            End If
            '職務区分
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFKBN.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFKBN  = '{0}'", work.WF_SEL_STAFFKBN.Text)
            End If
            '従業員(名称)
            If Not String.IsNullOrEmpty(work.WF_SEL_STAFFNAMES.Text) Then
                SQLstr &= String.Format("    AND MB01.STAFFNAMES LIKE '%{0}%'", work.WF_SEL_STAFFNAMES.Text)
            End If

            SQLstr &=
                  " GROUP BY" _
                & "    MB01.CAMPCODE" _
                & "    , MB01.STAFFCODE" _
                & "    , MB01.HORG" _
                & " ORDER BY" _
                & "    MB01.CAMPCODE" _
                & "    , MB01.STAFFCODE" _
                & "    , MB01.HORG"

            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 30)           '端末ID
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)           '会社コード
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)           'オブジェクト
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)           'オブジェクト
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 50)           '構造コード
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           'グループコード1
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                   '開始年月日
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                   '終了年月日
            Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.Date)                   '現在日付
            Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 1)          '削除フラグ

            PARA1.Value = CS0050SESSION.APSV_ID
            PARA2.Value = work.WF_SEL_CAMPCODE.Text
            PARA3.Value = C_ROLE_VARIANT.SERV_ORG
            PARA4.Value = C_ROLE_VARIANT.USER_ORG
            PARA5.Value = "勤怠管理組織"
            PARA6.Value = work.WF_SEL_HORG.Text
            PARA7.Value = WW_DATE_END
            PARA8.Value = WW_DATE_ST
            PARA9.Value = Date.Now
            PARA10.Value = C_DELETE_FLG.DELETE

            Dim WW_TABLE As DataTable = New DataTable

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                WW_TABLE.Load(SQLdr)
            End Using

            'テンポラリーテーブルに出力
            Using SQLbc As New SqlBulkCopy(SQLcon)
                SQLbc.DestinationTableName = "#MBTemp"
                SQLbc.WriteToServer(WW_TABLE)
            End Using

            If Not IsNothing(WW_TABLE) Then
                WW_TABLE.Clear()
                WW_TABLE.Dispose()
                WW_TABLE = Nothing
            End If

            '○ 画面表示のメインSQL
            SQLstr =
                  " SELECT" _
                & "    0                                                      AS LINECNT" _
                & "    , ''                                                   AS OPERATION" _
                & "    , CAST(ISNULL(T007.UPDTIMSTP, 0) AS bigint)            AS TIMSTP" _
                & "    , 1                                                    AS 'SELECT'" _
                & "    , 0                                                    AS HIDDEN" _
                & "    , '0'                                                  AS EXTRACTCNT" _
                & "    , ISNULL(RTRIM(T091.STATUS), '')                       AS STATUS" _
                & "    , (CASE WHEN ISNULL(RTRIM(T091.STEP), '') = ''" _
                & "            THEN ''" _
                & "            WHEN ISNULL(RTRIM(T091.STEP), '') > '01'" _
                & "            AND  ISNULL(RTRIM(T092.STEP), '') = '01'" _
                & "            THEN ISNULL(RTRIM(MC01.VALUE2), '')" _
                & "            ELSE ISNULL(RTRIM(MC01.VALUE1), '') END)       AS STATUSTEXT" _
                & "    , ISNULL(RTRIM(TEMP.CAMPCODE), '')                     AS CAMPCODE" _
                & "    , ''                                                   AS CAMPNAMES" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'yyyy/MM'), '')       AS TAISHOYM" _
                & "    , ISNULL(RTRIM(TEMP.STAFFCODE), '')                    AS STAFFCODE" _
                & "    , ''                                                   AS STAFFNAMES" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'yyyy/MM/dd'), '')    AS WORKDATE" _
                & "    , ISNULL(FORMAT(MB05.WORKINGYMD, 'dd'), '')            AS WORKDAY" _
                & "    , ISNULL(RTRIM(MB05.WORKINGWEEK), '')                  AS WORKINGWEEK" _
                & "    , ''                                                   AS WORKINGWEEKNAMES" _
                & "    , ISNULL(RTRIM(T007.HDKBN), 'H')                       AS HDKBN" _
                & "    , ISNULL(RTRIM(MB05.RECODEKBN), '0')                   AS RECODEKBN" _
                & "    , ''                                                   AS RECODEKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.SEQ), '0')                         AS SEQ" _
                & "    , ISNULL(RTRIM(T007.ENTRYDATE), '')                    AS ENTRYDATE" _
                & "    , ISNULL(RTRIM(T007.NIPPOLINKCODE), '')                AS NIPPOLINKCODE" _
                & "    , ISNULL(RTRIM(MB01.MORG), '')                         AS MORG" _
                & "    , ''                                                   AS MORGNAMES" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')                         AS HORG" _
                & "    , ''                                                   AS HORGNAMES" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')                         AS SORG" _
                & "    , ''                                                   AS SORGNAMES" _
                & "    , ISNULL(RTRIM(MB01.STAFFKBN), '')                     AS STAFFKBN" _
                & "    , ''                                                   AS STAFFKBNNAMES" _
                & "    , ''                                                   AS STAFFKBNTAISHOGAI" _
                & "    , ISNULL(RTRIM(MB05.WORKINGKBN), '')                   AS HOLIDAYKBN" _
                & "    , ''                                                   AS HOLIDAYKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.PAYKBN), '00')                     AS PAYKBN" _
                & "    , ''                                                   AS PAYKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKKBN), '0')                 AS SHUKCHOKKBN" _
                & "    , ''                                                   AS SHUKCHOKKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.WORKKBN), '')                      AS WORKKBN" _
                & "    , ''                                                   AS WORKKBNNAMES" _
                & "    , ISNULL(FORMAT(T007.STDATE, 'yyyy/MM/dd'), '')        AS STDATE" _
                & "    , ISNULL(CONVERT(char(5), T007.STTIME), '00:00')       AS STTIME" _
                & "    , ISNULL(FORMAT(T007.ENDDATE, 'yyyy/MM/dd'), '')       AS ENDDATE" _
                & "    , ISNULL(CONVERT(char(5), T007.ENDTIME), '00:00')      AS ENDTIME" _
                & "    , ISNULL(RTRIM(T007.WORKTIME), '0')                    AS WORKTIME" _
                & "    , ISNULL(RTRIM(T007.MOVETIME), '0')                    AS MOVETIME" _
                & "    , ISNULL(RTRIM(T007.ACTTIME), '0')                     AS ACTTIME" _
                & "    , ISNULL(CONVERT(char(5), T007.BINDSTDATE), '00:00')   AS BINDSTDATE" _
                & "    , ISNULL(RTRIM(T007.BINDTIME), '0')                    AS BINDTIMEMIN" _
                & "    , ISNULL(CONVERT(char(5), MB04.WORKINGH), '00:00')     AS BINDTIME" _
                & "    , ISNULL(RTRIM(T007.NIPPOBREAKTIME), '0')              AS NIPPOBREAKTIME" _
                & "    , ISNULL(RTRIM(T007.BREAKTIME), '0')                   AS BREAKTIME" _
                & "    , ISNULL(RTRIM(T007.BREAKTIMECHO), '0')                AS BREAKTIMECHO" _
                & "    , '0'                                                  AS BREAKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.NIGHTTIME), '0')                   AS NIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.NIGHTTIMECHO), '0')                AS NIGHTTIMECHO" _
                & "    , '0'                                                  AS NIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.ORVERTIME), '0')                   AS ORVERTIME" _
                & "    , ISNULL(RTRIM(T007.ORVERTIMECHO), '0')                AS ORVERTIMECHO" _
                & "    , ISNULL(RTRIM(T007.ORVERTIMEADD), '0')                AS ORVERTIMEADD" _
                & "    , '0'                                                  AS ORVERTIMETTL" _
                & "    , ISNULL(RTRIM(T007.WNIGHTTIME), '0')                  AS WNIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.WNIGHTTIMECHO), '0')               AS WNIGHTTIMECHO" _
                & "    , ISNULL(RTRIM(T007.WNIGHTTIMEADD), '0')               AS WNIGHTTIMEADD" _
                & "    , '0'                                                  AS WNIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.SWORKTIME), '0')                   AS SWORKTIME" _
                & "    , ISNULL(RTRIM(T007.SWORKTIMECHO), '0')                AS SWORKTIMECHO" _
                & "    , ISNULL(RTRIM(T007.SWORKTIMEADD), '0')                AS SWORKTIMEADD" _
                & "    , '0'                                                  AS SWORKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.SNIGHTTIME), '0')                  AS SNIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.SNIGHTTIMECHO), '0')               AS SNIGHTTIMECHO" _
                & "    , ISNULL(RTRIM(T007.SNIGHTTIMEADD), '0')               AS SNIGHTTIMEADD" _
                & "    , '0'                                                  AS SNIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.HWORKTIME), '0')                   AS HWORKTIME" _
                & "    , ISNULL(RTRIM(T007.HWORKTIMECHO), '0')                AS HWORKTIMECHO" _
                & "    , '0'                                                  AS HWORKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.HNIGHTTIME), '0')                  AS HNIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.HNIGHTTIMECHO), '0')               AS HNIGHTTIMECHO" _
                & "    , '0'                                                  AS HNIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.WORKNISSU), '0')                   AS WORKNISSU" _
                & "    , ISNULL(RTRIM(T007.WORKNISSUCHO), '0')                AS WORKNISSUCHO" _
                & "    , '0'                                                  AS WORKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHOUKETUNISSU), '0')               AS SHOUKETUNISSU" _
                & "    , ISNULL(RTRIM(T007.SHOUKETUNISSUCHO), '0')            AS SHOUKETUNISSUCHO" _
                & "    , '0'                                                  AS SHOUKETUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.KUMIKETUNISSU), '0')               AS KUMIKETUNISSU" _
                & "    , ISNULL(RTRIM(T007.KUMIKETUNISSUCHO), '0')            AS KUMIKETUNISSUCHO" _
                & "    , '0'                                                  AS KUMIKETUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.ETCKETUNISSU), '0')                AS ETCKETUNISSU" _
                & "    , ISNULL(RTRIM(T007.ETCKETUNISSUCHO), '0')             AS ETCKETUNISSUCHO" _
                & "    , '0'                                                  AS ETCKETUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.NENKYUNISSU), '0')                 AS NENKYUNISSU" _
                & "    , ISNULL(RTRIM(T007.NENKYUNISSUCHO), '0')              AS NENKYUNISSUCHO" _
                & "    , '0'                                                  AS NENKYUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.TOKUKYUNISSU), '0')                AS TOKUKYUNISSU" _
                & "    , ISNULL(RTRIM(T007.TOKUKYUNISSUCHO), '0')             AS TOKUKYUNISSUCHO" _
                & "    , '0'                                                  AS TOKUKYUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.CHIKOKSOTAINISSU), '0')            AS CHIKOKSOTAINISSU" _
                & "    , ISNULL(RTRIM(T007.CHIKOKSOTAINISSUCHO), '0')         AS CHIKOKSOTAINISSUCHO" _
                & "    , '0'                                                  AS CHIKOKSOTAINISSUTTL" _
                & "    , ISNULL(RTRIM(T007.STOCKNISSU), '0')                  AS STOCKNISSU" _
                & "    , ISNULL(RTRIM(T007.STOCKNISSUCHO), '0')               AS STOCKNISSUCHO" _
                & "    , '0'                                                  AS STOCKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.KYOTEIWEEKNISSU), '0')             AS KYOTEIWEEKNISSU" _
                & "    , ISNULL(RTRIM(T007.KYOTEIWEEKNISSUCHO), '0')          AS KYOTEIWEEKNISSUCHO" _
                & "    , '0'                                                  AS KYOTEIWEEKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.WEEKNISSU), '0')                   AS WEEKNISSU" _
                & "    , ISNULL(RTRIM(T007.WEEKNISSUCHO), '0')                AS WEEKNISSUCHO" _
                & "    , '0'                                                  AS WEEKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.DAIKYUNISSU), '0')                 AS DAIKYUNISSU" _
                & "    , ISNULL(RTRIM(T007.DAIKYUNISSUCHO), '0')              AS DAIKYUNISSUCHO" _
                & "    , '0'                                                  AS DAIKYUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.NENSHINISSU), '0')                 AS NENSHINISSU" _
                & "    , ISNULL(RTRIM(T007.NENSHINISSUCHO), '0')              AS NENSHINISSUCHO" _
                & "    , '0'                                                  AS NENSHINISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNNISSU), '0')              AS SHUKCHOKNNISSU" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNNISSUCHO), '0')           AS SHUKCHOKNNISSUCHO" _
                & "    , '0'                                                  AS SHUKCHOKNNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNISSU), '0')               AS SHUKCHOKNISSU" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNISSUCHO), '0')            AS SHUKCHOKNISSUCHO" _
                & "    , '0'                                                  AS SHUKCHOKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNHLDNISSU), '0')           AS SHUKCHOKNHLDNISSU" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKNHLDNISSUCHO), '0')        AS SHUKCHOKNHLDNISSUCHO" _
                & "    , '0'                                                  AS SHUKCHOKNHLDNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKHLDNISSU), '0')            AS SHUKCHOKHLDNISSU" _
                & "    , ISNULL(RTRIM(T007.SHUKCHOKHLDNISSUCHO), '0')         AS SHUKCHOKHLDNISSUCHO" _
                & "    , '0'                                                  AS SHUKCHOKHLDNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.TOKSAAKAISU), '0')                 AS TOKSAAKAISU" _
                & "    , ISNULL(RTRIM(T007.TOKSAAKAISUCHO), '0')              AS TOKSAAKAISUCHO" _
                & "    , '0'                                                  AS TOKSAAKAISUTTL" _
                & "    , ISNULL(RTRIM(T007.TOKSABKAISU), '0')                 AS TOKSABKAISU" _
                & "    , ISNULL(RTRIM(T007.TOKSABKAISUCHO), '0')              AS TOKSABKAISUCHO" _
                & "    , '0'                                                  AS TOKSABKAISUTTL" _
                & "    , ISNULL(RTRIM(T007.TOKSACKAISU), '0')                 AS TOKSACKAISU" _
                & "    , ISNULL(RTRIM(T007.TOKSACKAISUCHO), '0')              AS TOKSACKAISUCHO" _
                & "    , '0'                                                  AS TOKSACKAISUTTL" _
                & "    , ISNULL(RTRIM(FLOOR(T007.TENKOKAISU)), '0')           AS TENKOKAISU" _
                & "    , ISNULL(RTRIM(FLOOR(T007.TENKOKAISUCHO)), '0')        AS TENKOKAISUCHO" _
                & "    , '0'                                                  AS TENKOKAISUTTL" _
                & "    , ISNULL(RTRIM(T007.HOANTIME), '0')                    AS HOANTIME" _
                & "    , ISNULL(RTRIM(T007.HOANTIMECHO), '0')                 AS HOANTIMECHO" _
                & "    , '0'                                                  AS HOANTIMETTL" _
                & "    , ISNULL(RTRIM(T007.KOATUTIME), '0')                   AS KOATUTIME" _
                & "    , ISNULL(RTRIM(T007.KOATUTIMECHO), '0')                AS KOATUTIMECHO" _
                & "    , '0'                                                  AS KOATUTIMETTL" _
                & "    , ISNULL(RTRIM(T007.TOKUSA1TIME), '0')                 AS TOKUSA1TIME" _
                & "    , ISNULL(RTRIM(T007.TOKUSA1TIMECHO), '0')              AS TOKUSA1TIMECHO" _
                & "    , '0'                                                  AS TOKUSA1TIMETTL" _
                & "    , ISNULL(RTRIM(T007.HAYADETIME), '0')                  AS HAYADETIME" _
                & "    , ISNULL(RTRIM(T007.HAYADETIMECHO), '0')               AS HAYADETIMECHO" _
                & "    , '0'                                                  AS HAYADETIMETTL" _
                & "    , ISNULL(RTRIM(T007.PONPNISSU), '0')                   AS PONPNISSU" _
                & "    , ISNULL(RTRIM(T007.PONPNISSUCHO), '0')                AS PONPNISSUCHO" _
                & "    , '0'                                                  AS PONPNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.BULKNISSU), '0')                   AS BULKNISSU" _
                & "    , ISNULL(RTRIM(T007.BULKNISSUCHO), '0')                AS BULKNISSUCHO" _
                & "    , '0'                                                  AS BULKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.TRAILERNISSU), '0')                AS TRAILERNISSU" _
                & "    , ISNULL(RTRIM(T007.TRAILERNISSUCHO), '0')             AS TRAILERNISSUCHO" _
                & "    , '0'                                                  AS TRAILERNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.BKINMUKAISU), '0')                 AS BKINMUKAISU" _
                & "    , ISNULL(RTRIM(T007.BKINMUKAISUCHO), '0')              AS BKINMUKAISUCHO" _
                & "    , '0'                                                  AS BKINMUKAISUTTL" _
                & "    , ISNULL(RTRIM(T007.SHARYOKBN), '')                    AS SHARYOKBN" _
                & "    , ''                                                   AS SHARYOKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.OILPAYKBN), '')                    AS OILPAYKBN" _
                & "    , ''                                                   AS OILPAYKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.UNLOADCNT), '0')                   AS UNLOADCNT" _
                & "    , ISNULL(RTRIM(T007.UNLOADCNTCHO), '0')                AS UNLOADCNTCHO" _
                & "    , '0'                                                  AS UNLOADCNTTTL" _
                & "    , '0'                                                  AS UNLOADCNTTTL0101" _
                & "    , '0'                                                  AS UNLOADCNTTTL0102" _
                & "    , '0'                                                  AS UNLOADCNTTTL0103" _
                & "    , '0'                                                  AS UNLOADCNTTTL0104" _
                & "    , '0'                                                  AS UNLOADCNTTTL0105" _
                & "    , '0'                                                  AS UNLOADCNTTTL0106" _
                & "    , '0'                                                  AS UNLOADCNTTTL0107" _
                & "    , '0'                                                  AS UNLOADCNTTTL0108" _
                & "    , '0'                                                  AS UNLOADCNTTTL0109" _
                & "    , '0'                                                  AS UNLOADCNTTTL0110" _
                & "    , '0'                                                  AS UNLOADCNTTTL0201" _
                & "    , '0'                                                  AS UNLOADCNTTTL0202" _
                & "    , '0'                                                  AS UNLOADCNTTTL0203" _
                & "    , '0'                                                  AS UNLOADCNTTTL0204" _
                & "    , '0'                                                  AS UNLOADCNTTTL0205" _
                & "    , '0'                                                  AS UNLOADCNTTTL0206" _
                & "    , '0'                                                  AS UNLOADCNTTTL0207" _
                & "    , '0'                                                  AS UNLOADCNTTTL0208" _
                & "    , '0'                                                  AS UNLOADCNTTTL0209" _
                & "    , '0'                                                  AS UNLOADCNTTTL0210" _
                & "    , ISNULL(RTRIM(FLOOR(T007.HAIDISTANCE)), '0')          AS HAIDISTANCE" _
                & "    , ISNULL(RTRIM(FLOOR(T007.HAIDISTANCECHO)), '0')       AS HAIDISTANCECHO" _
                & "    , '0'                                                  AS HAIDISTANCETTL" _
                & "    , '0'                                                  AS HAIDISTANCETTL0101" _
                & "    , '0'                                                  AS HAIDISTANCETTL0102" _
                & "    , '0'                                                  AS HAIDISTANCETTL0103" _
                & "    , '0'                                                  AS HAIDISTANCETTL0104" _
                & "    , '0'                                                  AS HAIDISTANCETTL0105" _
                & "    , '0'                                                  AS HAIDISTANCETTL0106" _
                & "    , '0'                                                  AS HAIDISTANCETTL0107" _
                & "    , '0'                                                  AS HAIDISTANCETTL0108" _
                & "    , '0'                                                  AS HAIDISTANCETTL0109" _
                & "    , '0'                                                  AS HAIDISTANCETTL0110" _
                & "    , '0'                                                  AS HAIDISTANCETTL0201" _
                & "    , '0'                                                  AS HAIDISTANCETTL0202" _
                & "    , '0'                                                  AS HAIDISTANCETTL0203" _
                & "    , '0'                                                  AS HAIDISTANCETTL0204" _
                & "    , '0'                                                  AS HAIDISTANCETTL0205" _
                & "    , '0'                                                  AS HAIDISTANCETTL0206" _
                & "    , '0'                                                  AS HAIDISTANCETTL0207" _
                & "    , '0'                                                  AS HAIDISTANCETTL0208" _
                & "    , '0'                                                  AS HAIDISTANCETTL0209" _
                & "    , '0'                                                  AS HAIDISTANCETTL0210" _
                & "    , ISNULL(RTRIM(FLOOR(T007.KAIDISTANCE)), '0')          AS KAIDISTANCE" _
                & "    , ISNULL(RTRIM(FLOOR(T007.KAIDISTANCECHO)), '0')       AS KAIDISTANCECHO" _
                & "    , '0'                                                  AS KAIDISTANCETTL" _
                & "    , ISNULL(RTRIM(T005.L1KAISO), '')                      AS L1KAISO" _
                & "    , ISNULL(CONVERT(char(5), T007.YENDTIME), '00:00')     AS YENDTIME" _
                & "    , ISNULL(RTRIM(T007.APPLYID), '')                      AS APPLYID" _
                & "    , ISNULL(RTRIM(T007.RIYU), '')                         AS RIYU" _
                & "    , ''                                                   AS RIYUNAMES" _
                & "    , ISNULL(RTRIM(T007.RIYUETC), '')                      AS RIYUETC" _
                & "    , ISNULL(RTRIM(T007.HAISOTIME), '0')                   AS HAISOTIME" _
                & "    , ISNULL(RTRIM(T007.NENMATUNISSU), '0')                AS NENMATUNISSU" _
                & "    , ISNULL(RTRIM(T007.NENMATUNISSUCHO), '0')             AS NENMATUNISSUCHO" _
                & "    , '0'                                                  AS NENMATUNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.SHACHUHAKKBN), '')                 AS SHACHUHAKKBN" _
                & "    , ''                                                   AS SHACHUHAKKBNNAMES" _
                & "    , ISNULL(RTRIM(T007.SHACHUHAKNISSU), '0')              AS SHACHUHAKNISSU" _
                & "    , ISNULL(RTRIM(T007.SHACHUHAKNISSUCHO), '0')           AS SHACHUHAKNISSUCHO" _
                & "    , '0'                                                  AS SHACHUHAKNISSUTTL" _
                & "    , ISNULL(RTRIM(FLOOR(T007.MODELDISTANCE)), '0')        AS MODELDISTANCE" _
                & "    , ISNULL(RTRIM(FLOOR(T007.MODELDISTANCECHO)), '0')     AS MODELDISTANCECHO" _
                & "    , '0'                                                  AS MODELDISTANCETTL" _
                & "    , ISNULL(RTRIM(T007.JIKYUSHATIME), '0')                AS JIKYUSHATIME" _
                & "    , ISNULL(RTRIM(T007.JIKYUSHATIMECHO), '0')             AS JIKYUSHATIMECHO" _
                & "    , '0'                                                  AS JIKYUSHATIMETTL" _
                & "    , ISNULL(RTRIM(T007.HDAIWORKTIME), '0')                AS HDAIWORKTIME" _
                & "    , ISNULL(RTRIM(T007.HDAIWORKTIMECHO), '0')             AS HDAIWORKTIMECHO" _
                & "    , '0'                                                  AS HDAIWORKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.HDAINIGHTTIME), '0')               AS HDAINIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.HDAINIGHTTIMECHO), '0')            AS HDAINIGHTTIMECHO" _
                & "    , '0'                                                  AS HDAINIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.SDAIWORKTIME), '0')                AS SDAIWORKTIME" _
                & "    , ISNULL(RTRIM(T007.SDAIWORKTIMECHO), '0')             AS SDAIWORKTIMECHO" _
                & "    , '0'                                                  AS SDAIWORKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.SDAINIGHTTIME), '0')               AS SDAINIGHTTIME" _
                & "    , ISNULL(RTRIM(T007.SDAINIGHTTIMECHO), '0')            AS SDAINIGHTTIMECHO" _
                & "    , '0'                                                  AS SDAINIGHTTIMETTL" _
                & "    , ISNULL(RTRIM(T007.WWORKTIME), '0')                   AS WWORKTIME" _
                & "    , ISNULL(RTRIM(T007.WWORKTIMECHO), '0')                AS WWORKTIMECHO" _
                & "    , '0'                                                  AS WWORKTIMETTL" _
                & "    , ISNULL(RTRIM(T007.JYOMUTIME), '0')                   AS JYOMUTIME" _
                & "    , ISNULL(RTRIM(T007.JYOMUTIMECHO), '0')                AS JYOMUTIMECHO" _
                & "    , '0'                                                  AS JYOMUTIMETTL" _
                & "    , ISNULL(RTRIM(T007.HWORKNISSU), '0')                  AS HWORKNISSU" _
                & "    , ISNULL(RTRIM(T007.HWORKNISSUCHO), '0')               AS HWORKNISSUCHO" _
                & "    , '0'                                                  AS HWORKNISSUTTL" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT), '0')                   AS KAITENCNT" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO), '0')                AS KAITENCNTCHO" _
                & "    , '0'                                                  AS KAITENCNTTTL" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT1_1), '0')                AS KAITENCNT1_1" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO1_1), '0')             AS KAITENCNTCHO1_1" _
                & "    , '0'                                                  AS KAITENCNTTTL1_1" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT1_2), '0')                AS KAITENCNT1_2" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO1_2), '0')             AS KAITENCNTCHO1_2" _
                & "    , '0'                                                  AS KAITENCNTTTL1_2" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT1_3), '0')                AS KAITENCNT1_3" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO1_3), '0')             AS KAITENCNTCHO1_3" _
                & "    , '0'                                                  AS KAITENCNTTTL1_3" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT1_4), '0')                AS KAITENCNT1_4" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO1_4), '0')             AS KAITENCNTCHO1_4" _
                & "    , '0'                                                  AS KAITENCNTTTL1_4" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT2_1), '0')                AS KAITENCNT2_1" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO2_1), '0')             AS KAITENCNTCHO2_1" _
                & "    , '0'                                                  AS KAITENCNTTTL2_1" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT2_2), '0')                AS KAITENCNT2_2" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO2_2), '0')             AS KAITENCNTCHO2_2" _
                & "    , '0'                                                  AS KAITENCNTTTL2_2" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT2_3), '0')                AS KAITENCNT2_3" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO2_3), '0')             AS KAITENCNTCHO2_3" _
                & "    , '0'                                                  AS KAITENCNTTTL2_3" _
                & "    , ISNULL(RTRIM(T007.KAITENCNT2_4), '0')                AS KAITENCNT2_4" _
                & "    , ISNULL(RTRIM(T007.KAITENCNTCHO2_4), '0')             AS KAITENCNTCHO2_4" _
                & "    , '0'                                                  AS KAITENCNTTTL2_4" _
                & "    , ISNULL(RTRIM(T007.SENJYOCNT), '0')                   AS SENJYOCNT" _
                & "    , ISNULL(RTRIM(T007.SENJYOCNTCHO), '0')                AS SENJYOCNTCHO" _
                & "    , '0'                                                  AS SENJYOCNTTTL" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT1), '0')               AS UNLOADADDCNT1" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT1CHO), '0')            AS UNLOADADDCNT1CHO" _
                & "    , '0'                                                  AS UNLOADADDCNT1TTL" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT2), '0')               AS UNLOADADDCNT2" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT2CHO), '0')            AS UNLOADADDCNT2CHO" _
                & "    , '0'                                                  AS UNLOADADDCNT2TTL" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT3), '0')               AS UNLOADADDCNT3" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT3CHO), '0')            AS UNLOADADDCNT3CHO" _
                & "    , '0'                                                  AS UNLOADADDCNT3TTL" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT4), '0')               AS UNLOADADDCNT4" _
                & "    , ISNULL(RTRIM(T007.UNLOADADDCNT4CHO), '0')            AS UNLOADADDCNT4CHO" _
                & "    , '0'                                                  AS UNLOADADDCNT4TTL" _
                & "    , ISNULL(RTRIM(T007.LOADINGCNT1), '0')                 AS LOADINGCNT1" _
                & "    , ISNULL(RTRIM(T007.LOADINGCNT1CHO), '0')              AS LOADINGCNT1CHO" _
                & "    , '0'                                                  AS LOADINGCNT1TTL" _
                & "    , ISNULL(RTRIM(T007.LOADINGCNT2), '0')                 AS LOADINGCNT2" _
                & "    , ISNULL(RTRIM(T007.LOADINGCNT2CHO), '0')              AS LOADINGCNT2CHO" _
                & "    , '0'                                                  AS LOADINGCNT2TTL" _
                & "    , ISNULL(RTRIM(T007.SHORTDISTANCE1), '0')              AS SHORTDISTANCE1" _
                & "    , ISNULL(RTRIM(T007.SHORTDISTANCE1CHO), '0')           AS SHORTDISTANCE1CHO" _
                & "    , '0'                                                  AS SHORTDISTANCE1TTL" _
                & "    , ISNULL(RTRIM(T007.SHORTDISTANCE2), '0')              AS SHORTDISTANCE2" _
                & "    , ISNULL(RTRIM(T007.SHORTDISTANCE2CHO), '0')           AS SHORTDISTANCE2CHO" _
                & "    , '0'                                                  AS SHORTDISTANCE2TTL" _
                & "    , ISNULL(RTRIM(T007.DELFLG), '0')                      AS DELFLG" _
                & "    , ''                                                   AS DELFLGNAMES" _
                & "    , 'K'                                                  AS DATAKBN" _
                & "    , ''                                                   AS SHIPORG" _
                & "    , ''                                                   AS SHIPORGNAMES" _
                & "    , ''                                                   AS NIPPONO" _
                & "    , ''                                                   AS GSHABAN" _
                & "    , '0'                                                  AS RUIDISTANCE" _
                & "    , '0'                                                  AS JIDISTANCE" _
                & "    , '0'                                                  AS KUDISTANCE" _
                & "    , '0'                                                  AS T10SAVECNT" _
                & "    , ''                                                   AS T10SHARYOKBN1" _
                & "    , ''                                                   AS T10OILPAYKBN1" _
                & "    , ''                                                   AS T10SHUKABASHO1" _
                & "    , ''                                                   AS T10TODOKECODE1" _
                & "    , '0'                                                  AS T10MODELDISTANCE1" _
                & "    , ''                                                   AS T10MODIFYKBN1" _
                & "    , ''                                                   AS T10SHARYOKBN2" _
                & "    , ''                                                   AS T10OILPAYKBN2" _
                & "    , ''                                                   AS T10SHUKABASHO2 " _
                & "    , ''                                                   AS T10TODOKECODE2" _
                & "    , '0'                                                  AS T10MODELDISTANCE2" _
                & "    , ''                                                   AS T10MODIFYKBN2" _
                & "    , ''                                                   AS T10SHARYOKBN3" _
                & "    , ''                                                   AS T10OILPAYKBN3" _
                & "    , ''                                                   AS T10SHUKABASHO3" _
                & "    , ''                                                   AS T10TODOKECODE3" _
                & "    , '0'                                                  AS T10MODELDISTANCE3" _
                & "    , ''                                                   AS T10MODIFYKBN3" _
                & "    , ''                                                   AS T10SHARYOKBN4" _
                & "    , ''                                                   AS T10OILPAYKBN4" _
                & "    , ''                                                   AS T10SHUKABASHO4" _
                & "    , ''                                                   AS T10TODOKECODE4" _
                & "    , '0'                                                  AS T10MODELDISTANCE4" _
                & "    , ''                                                   AS T10MODIFYKBN4" _
                & "    , ''                                                   AS T10SHARYOKBN5" _
                & "    , ''                                                   AS T10OILPAYKBN5" _
                & "    , ''                                                   AS T10SHUKABASHO5" _
                & "    , ''                                                   AS T10TODOKECODE5" _
                & "    , '0'                                                  AS T10MODELDISTANCE5" _
                & "    , ''                                                   AS T10MODIFYKBN5" _
                & "    , ''                                                   AS T10SHARYOKBN6" _
                & "    , ''                                                   AS T10OILPAYKBN6" _
                & "    , ''                                                   AS T10SHUKABASHO6" _
                & "    , ''                                                   AS T10TODOKECODE6" _
                & "    , '0'                                                  AS T10MODELDISTANCE6" _
                & "    , ''                                                   AS T10MODIFYKBN6" _
                & "    , '0'                                                  AS ENTRYFLG" _
                & "    , '0'                                                  AS DRAWALFLG" _
                & "    , (CASE WHEN ISNULL(RTRIM(T007.RECODEKBN), '') <> ''" _
                & "            THEN '1'" _
                & "            ELSE '0' END)                                  AS DBUMUFLG" _
                & " FROM" _
                & "    #MBTemp TEMP" _
                & "    INNER JOIN (" _
                & "        SELECT" _
                & "            ISNULL(RTRIM(CAMPCODE), '')                    AS CAMPCODE" _
                & "            , ISNULL(FORMAT(WORKINGYMD, 'yyyy/MM/dd'), '') AS WORKINGYMD" _
                & "            , ISNULL(RTRIM(WORKINGWEEK), '')               AS WORKINGWEEK" _
                & "            , ISNULL(RTRIM(WORKINGKBN), '')                AS WORKINGKBN" _
                & "            , '0'                                          AS RECODEKBN" _
                & "        FROM" _
                & "            MB005_CALENDAR" _
                & "        WHERE" _
                & "            CAMPCODE          = @P1" _
                & "            AND WORKINGYMD   >= @P3" _
                & "            AND WORKINGYMD   <= @P4" _
                & "            AND DELFLG       <> @P6" _
                & "        UNION ALL" _
                & "        SELECT" _
                & "            @P1                                            AS CAMPCODE" _
                & "            , EOMONTH(@P3, 0)                              AS WORKINGYMD" _
                & "            , ''                                           AS WORKINGWEEK" _
                & "            , ''                                           AS WORKINGKBN" _
                & "            , '2'                                          AS RECODEKBN" _
                & "        ) MB05" _
                & "        ON  MB05.CAMPCODE     = TEMP.CAMPCODE" _
                & "    LEFT JOIN MB001_STAFF MB01" _
                & "        ON  MB01.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND MB01.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND MB01.STYMD       <= MB05.WORKINGYMD" _
                & "        AND MB01.ENDYMD      >= MB05.WORKINGYMD" _
                & "        AND MB01.DELFLG      <> @P6" _
                & "    LEFT JOIN MB004_WORKINGH MB04" _
                & "        ON  MB04.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND MB04.HORG         = MB01.HORG" _
                & "        AND MB04.STAFFKBN     = MB01.STAFFKBN" _
                & "        AND MB04.STYMD       <= MB05.WORKINGYMD" _
                & "        AND MB04.ENDYMD      >= MB05.WORKINGYMD" _
                & "        AND MB04.DELFLG      <> @P6" _
                & "    LEFT JOIN T0005_NIPPO T005" _
                & "        ON  T005.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND T005.YMD          = MB05.WORKINGYMD" _
                & "        AND T005.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND T005.SEQ          = 1" _
                & "        AND T005.ENTRYDATE    = (" _
                & "            SELECT" _
                & "                MAX(ENTRYDATE)" _
                & "            FROM" _
                & "                T0005_NIPPO" _
                & "            WHERE" _
                & "                CAMPCODE      = TEMP.CAMPCODE" _
                & "                AND YMD       = MB05.WORKINGYMD" _
                & "                AND STAFFCODE = TEMP.STAFFCODE" _
                & "                AND SEQ       = 1" _
                & "                AND DELFLG   <> @P6)" _
                & "        AND T005.DELFLG      <> @P6" _
                & "    LEFT JOIN T0007_KINTAI T007" _
                & "        ON  T007.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND T007.WORKDATE     = MB05.WORKINGYMD" _
                & "        AND T007.STAFFCODE    = TEMP.STAFFCODE" _
                & "        AND T007.RECODEKBN    = MB05.RECODEKBN" _
                & "        AND T007.DELFLG      <> @P6" _
                & "    LEFT JOIN T0009_APPROVALHIST T091" _
                & "        ON  T091.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND T091.APPLYID      = T007.APPLYID" _
                & "        AND T091.STEP         = (" _
                & "            SELECT" _
                & "                MAX(STEP)" _
                & "            FROM" _
                & "                T0009_APPROVALHIST" _
                & "            WHERE" _
                & "                CAMPCODE      = TEMP.CAMPCODE" _
                & "                AND APPLYID   = T007.APPLYID" _
                & "                AND DELFLG   <> @P6)" _
                & "        AND T091.DELFLG      <> @P6" _
                & "    LEFT JOIN T0009_APPROVALHIST T092" _
                & "        ON  T092.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND T092.APPLYID      = T007.APPLYID" _
                & "        AND T092.STATUS       = '10'" _
                & "        AND T092.STEP         = (" _
                & "            SELECT" _
                & "                MAX(STEP)" _
                & "            FROM" _
                & "                T0009_APPROVALHIST" _
                & "            WHERE" _
                & "                CAMPCODE      = TEMP.CAMPCODE" _
                & "                AND APPLYID   = T007.APPLYID" _
                & "                AND STATUS    = '10'" _
                & "                AND DELFLG   <> @P6)" _
                & "        AND T092.DELFLG      <> @P6" _
                & "    LEFT JOIN MC001_FIXVALUE MC01" _
                & "        ON  MC01.CAMPCODE     = TEMP.CAMPCODE" _
                & "        AND MC01.CLASS        = @P2" _
                & "        AND MC01.KEYCODE      = T091.STATUS" _
                & "        AND MC01.STYMD       <= @P5" _
                & "        AND MC01.ENDYMD      >= @P5" _
                & "        AND MC01.DELFLG      <> @P6" _
                & " WHERE" _
                & "    TEMP.CAMPCODE = @P1" _
                & " ORDER BY" _
                & "    TEMP.CAMPCODE" _
                & "    , MB01.HORG" _
                & "    , TEMP.STAFFCODE" _
                & "    , MB05.WORKINGYMD" _
                & "    , T005.STDATE" _
                & "    , T005.STTIME" _
                & "    , T005.ENDDATE" _
                & "    , T005.ENDTIME" _
                & "    , T007.HDKBN DESC"

            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            PARA1 = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
            PARA2 = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '分類(承認ステータス)
            PARA3 = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '対象年月初
            PARA4 = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '対象年月末
            PARA5 = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '現在日付
            PARA6 = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

            PARA1.Value = work.WF_SEL_CAMPCODE.Text
            PARA2.Value = "APPROVAL"
            PARA3.Value = WW_DATE_ST
            PARA4.Value = WW_DATE_END
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00009tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00009tbl.Load(SQLdr)
            End Using


            '○ 前月分(2日前まで)を取得
            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            PARA1 = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
            PARA2 = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '分類(承認ステータス)
            PARA3 = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '対象年月初
            PARA4 = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '対象年月末
            PARA5 = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '現在日付
            PARA6 = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

            PARA1.Value = work.WF_SEL_CAMPCODE.Text
            PARA2.Value = "APPROVAL"
            PARA3.Value = WW_DATE_ST.AddDays(-3)
            PARA4.Value = WW_DATE_ST.AddDays(-1)
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00009BEFtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00009BEFtbl.Load(SQLdr)
            End Using

            '○ 日別のヘッダのみ抽出
            CS0026TBLSORT.TABLE = T00009BEFtbl
            CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '0'"
            CS0026TBLSORT.sort(T00009BEFtbl)

            '○ 翌月分(1日分)を取得
            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            PARA1 = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
            PARA2 = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '分類(承認ステータス)
            PARA3 = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '対象年月初
            PARA4 = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '対象年月末
            PARA5 = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '現在日付
            PARA6 = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

            PARA1.Value = work.WF_SEL_CAMPCODE.Text
            PARA2.Value = "APPROVAL"
            PARA3.Value = WW_DATE_ST.AddMonths(1)
            PARA4.Value = WW_DATE_ST.AddMonths(1)
            PARA5.Value = Date.Now
            PARA6.Value = C_DELETE_FLG.DELETE

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    T00009AFTtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                T00009AFTtbl.Load(SQLdr)
            End Using

            '○ 日別のヘッダのみ抽出
            CS0026TBLSORT.TABLE = T00009AFTtbl
            CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '0'"
            CS0026TBLSORT.sort(T00009AFTtbl)

            '○前月と翌月分をマージ
            T00009tbl.Merge(T00009BEFtbl)
            T00009tbl.Merge(T00009AFTtbl)

            If Not IsNothing(T00009BEFtbl) Then
                T00009BEFtbl.Clear()
                T00009BEFtbl.Dispose()
                T00009BEFtbl = Nothing
            End If

            If Not IsNothing(T00009AFTtbl) Then
                T00009AFTtbl.Clear()
                T00009AFTtbl.Dispose()
                T00009AFTtbl = Nothing
            End If

            '○ 日別のヘッダのみ抽出
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.SORTING = "CAMPCODE, HORG, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME, HDKBN DESC"
            CS0026TBLSORT.FILTER = ""
            CS0026TBLSORT.sort(T00009tbl)

            Dim WW_LINECNT As Integer = 0
            Dim WW_SAVEKEY As String = ""
            For Each T00009row As DataRow In T00009tbl.Rows
                Dim WW_KEY As String = T00009row("CAMPCODE") & "," & T00009row("HORG") & "," & T00009row("STAFFCODE")
                If WW_SAVEKEY <> WW_KEY Then
                    WW_LINECNT = 0
                    WW_SAVEKEY = WW_KEY
                End If

                '固定項目
                If T00009row("TAISHOYM") = work.WF_SEL_TAISHOYM.Text Then
                    If T00009row("HDKBN") = "H" Then
                        If T00009row("RECODEKBN") = "0" Then
                            WW_LINECNT = WW_LINECNT + 1
                            T00009row("LINECNT") = WW_LINECNT
                        End If
                        T00009row("SELECT") = 1
                        T00009row("HIDDEN") = 0
                    Else
                        T00009row("SELECT") = 1
                        T00009row("HIDDEN") = 1
                    End If
                Else
                    T00009row("SELECT") = 0
                    T00009row("HIDDEN") = 1
                End If

                '設定項目
                '開始日
                If String.IsNullOrEmpty(T00009row("STDATE")) Then
                    T00009row("STDATE") = T00009row("WORKDATE")
                End If

                '終了日
                If String.IsNullOrEmpty(T00009row("ENDDATE")) Then
                    T00009row("ENDDATE") = T00009row("WORKDATE")
                End If

                '拘束時間(分)
                If T00009row("BINDTIMEMIN") = "0" Then
                    If IsDate(T00009row("BINDTIME")) Then
                        If T00009row("HOLIDAYKBN") <> "0" Then
                            T00009row("BINDTIME") = "00:00"
                        End If
                    Else
                        T00009row("BINDTIME") = "12:00"
                    End If
                Else
                    If T00009row("HOLIDAYKBN") = "0" Then
                        T00009row("BINDTIME") = T0007COM.formatHHMM(T00009row("BINDTIMEMIN"))
                    Else
                        T00009row("BINDTIME") = "00:00"
                    End If
                End If

                Dim WW_TOTAL As Integer = 0

                '合計算出、分 → 時:分に変換(formatHHMM)
                T00009row("WORKTIME") = T0007COM.formatHHMM(T00009row("WORKTIME"))                      '作業時間
                T00009row("MOVETIME") = T0007COM.formatHHMM(T00009row("MOVETIME"))                      '移動時間
                T00009row("ACTTIME") = T0007COM.formatHHMM(T00009row("ACTTIME"))                        '稼働時間

                WW_TOTAL = CInt(T00009row("NIPPOBREAKTIME")) + CInt(T00009row("BREAKTIME")) + CInt(T00009row("BREAKTIMECHO"))
                T00009row("NIPPOBREAKTIME") = T0007COM.formatHHMM(T00009row("NIPPOBREAKTIME"))          '休憩時間
                T00009row("BREAKTIME") = T0007COM.formatHHMM(T00009row("BREAKTIME"))                    '休憩時間(分)
                T00009row("BREAKTIMECHO") = T0007COM.formatHHMM(T00009row("BREAKTIMECHO"))              '休憩調整時間(分)
                T00009row("BREAKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '休憩時間合計

                WW_TOTAL = CInt(T00009row("NIGHTTIME")) + CInt(T00009row("NIGHTTIMECHO"))
                T00009row("NIGHTTIME") = T0007COM.formatHHMM(T00009row("NIGHTTIME"))                    '所定深夜時間(分)
                T00009row("NIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("NIGHTTIMECHO"))              '所定深夜調整時間(分)
                T00009row("NIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '所定深夜時間合計

                WW_TOTAL = CInt(T00009row("ORVERTIME")) + CInt(T00009row("ORVERTIMECHO"))
                T00009row("ORVERTIME") = T0007COM.formatHHMM(T00009row("ORVERTIME"))                    '平日残業時間(分)
                T00009row("ORVERTIMECHO") = T0007COM.formatHHMM(T00009row("ORVERTIMECHO"))              '平日残業調整時間(分)
                T00009row("ORVERTIMEADD") = T0007COM.formatHHMM(T00009row("ORVERTIMEADD"))              '平日残業時間(調整加算)(分)
                T00009row("ORVERTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '平日残業時間合計

                WW_TOTAL = CInt(T00009row("WNIGHTTIME")) + CInt(T00009row("WNIGHTTIMECHO"))
                T00009row("WNIGHTTIME") = T0007COM.formatHHMM(T00009row("WNIGHTTIME"))                  '平日深夜時間(分)
                T00009row("WNIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("WNIGHTTIMECHO"))            '平日深夜調整時間(分)
                T00009row("WNIGHTTIMEADD") = T0007COM.formatHHMM(T00009row("WNIGHTTIMEADD"))            '平日深夜時間(調整加算)(分)
                T00009row("WNIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                              '平日深夜時間合計

                WW_TOTAL = CInt(T00009row("SWORKTIME")) + CInt(T00009row("SWORKTIMECHO"))
                T00009row("SWORKTIME") = T0007COM.formatHHMM(T00009row("SWORKTIME"))                    '日曜出勤時間(分)
                T00009row("SWORKTIMECHO") = T0007COM.formatHHMM(T00009row("SWORKTIMECHO"))              '日曜出勤調整時間(分)
                T00009row("SWORKTIMEADD") = T0007COM.formatHHMM(T00009row("SWORKTIMEADD"))              '日曜出勤時間(調整加算)(分)
                T00009row("SWORKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '日曜出勤時間合計

                WW_TOTAL = CInt(T00009row("SNIGHTTIME")) + CInt(T00009row("SNIGHTTIMECHO"))
                T00009row("SNIGHTTIME") = T0007COM.formatHHMM(T00009row("SNIGHTTIME"))                  '日曜深夜時間(分)
                T00009row("SNIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("SNIGHTTIMECHO"))            '日曜深夜調整時間(分)
                T00009row("SNIGHTTIMEADD") = T0007COM.formatHHMM(T00009row("SNIGHTTIMEADD"))            '日曜深夜時間(調整加算)(分)
                T00009row("SNIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                              '日曜深夜時間合計

                WW_TOTAL = CInt(T00009row("HWORKTIME")) + CInt(T00009row("HWORKTIMECHO"))
                T00009row("HWORKTIME") = T0007COM.formatHHMM(T00009row("HWORKTIME"))                    '休日出勤時間(分)
                T00009row("HWORKTIMECHO") = T0007COM.formatHHMM(T00009row("HWORKTIMECHO"))              '休日出勤調整時間(分)
                T00009row("HWORKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '休日出勤時間合計

                WW_TOTAL = CInt(T00009row("HNIGHTTIME")) + CInt(T00009row("HNIGHTTIMECHO"))
                T00009row("HNIGHTTIME") = T0007COM.formatHHMM(T00009row("HNIGHTTIME"))                  '休日深夜時間(分)
                T00009row("HNIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("HNIGHTTIMECHO"))            '休日深夜調整時間(分)
                T00009row("HNIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                              '休日深夜時間合計

                WW_TOTAL = CInt(T00009row("WORKNISSU")) + CInt(T00009row("WORKNISSUCHO"))
                T00009row("WORKNISSUTTL") = WW_TOTAL.ToString()                                         '所労合計

                WW_TOTAL = CInt(T00009row("SHOUKETUNISSU")) + CInt(T00009row("SHOUKETUNISSUCHO"))
                T00009row("SHOUKETUNISSUTTL") = WW_TOTAL.ToString()                                     '傷欠合計

                WW_TOTAL = CInt(T00009row("KUMIKETUNISSU")) + CInt(T00009row("KUMIKETUNISSUCHO"))
                T00009row("KUMIKETUNISSUTTL") = WW_TOTAL.ToString()                                     '組欠合計

                WW_TOTAL = CInt(T00009row("ETCKETUNISSU")) + CInt(T00009row("ETCKETUNISSUCHO"))
                T00009row("ETCKETUNISSUTTL") = WW_TOTAL.ToString()                                      '他欠合計

                WW_TOTAL = CInt(T00009row("NENKYUNISSU")) + CInt(T00009row("NENKYUNISSUCHO"))
                T00009row("NENKYUNISSUTTL") = WW_TOTAL.ToString()                                       '年休合計

                WW_TOTAL = CInt(T00009row("TOKUKYUNISSU")) + CInt(T00009row("TOKUKYUNISSUCHO"))
                T00009row("TOKUKYUNISSUTTL") = WW_TOTAL.ToString()                                      '特休合計

                WW_TOTAL = CInt(T00009row("CHIKOKSOTAINISSU")) + CInt(T00009row("CHIKOKSOTAINISSUCHO"))
                T00009row("CHIKOKSOTAINISSUTTL") = WW_TOTAL.ToString()                                  '遅早合計

                WW_TOTAL = CInt(T00009row("STOCKNISSU")) + CInt(T00009row("STOCKNISSUCHO"))
                T00009row("STOCKNISSUTTL") = WW_TOTAL.ToString()                                        'ストック休暇合計

                WW_TOTAL = CInt(T00009row("KYOTEIWEEKNISSU")) + CInt(T00009row("KYOTEIWEEKNISSUCHO"))
                T00009row("KYOTEIWEEKNISSUTTL") = WW_TOTAL.ToString()                                    '協定週休合計

                WW_TOTAL = CInt(T00009row("WEEKNISSU")) + CInt(T00009row("WEEKNISSUCHO"))
                T00009row("WEEKNISSUTTL") = WW_TOTAL.ToString()                                         '週休合計

                WW_TOTAL = CInt(T00009row("DAIKYUNISSU")) + CInt(T00009row("DAIKYUNISSUCHO"))
                T00009row("DAIKYUNISSUTTL") = WW_TOTAL.ToString()                                       '代休合計

                WW_TOTAL = CInt(T00009row("NENSHINISSU")) + CInt(T00009row("NENSHINISSUCHO"))
                T00009row("NENSHINISSUTTL") = WW_TOTAL.ToString()                                       '年始出勤合計

                WW_TOTAL = CInt(T00009row("SHUKCHOKNNISSU")) + CInt(T00009row("SHUKCHOKNNISSUCHO"))
                T00009row("SHUKCHOKNNISSUTTL") = WW_TOTAL.ToString()                                    '宿日直年始合計

                WW_TOTAL = CInt(T00009row("SHUKCHOKNISSU")) + CInt(T00009row("SHUKCHOKNISSUCHO"))
                T00009row("SHUKCHOKNISSUTTL") = WW_TOTAL.ToString()                                     '宿日直通常合計

                WW_TOTAL = CInt(T00009row("SHUKCHOKNHLDNISSU")) + CInt(T00009row("SHUKCHOKNHLDNISSUCHO"))
                T00009row("SHUKCHOKNHLDNISSUTTL") = WW_TOTAL.ToString()                                 '宿日直年始(翌日休み)合計

                WW_TOTAL = CInt(T00009row("SHUKCHOKHLDNISSU")) + CInt(T00009row("SHUKCHOKHLDNISSUCHO"))
                T00009row("SHUKCHOKHLDNISSUTTL") = WW_TOTAL.ToString()                                  '宿日直通常(翌日休み)合計

                WW_TOTAL = CInt(T00009row("TOKSAAKAISU")) + CInt(T00009row("TOKSAAKAISUCHO"))
                T00009row("TOKSAAKAISUTTL") = WW_TOTAL.ToString()                                       '特作A合計

                WW_TOTAL = CInt(T00009row("TOKSABKAISU")) + CInt(T00009row("TOKSABKAISUCHO"))
                T00009row("TOKSABKAISUTTL") = WW_TOTAL.ToString()                                       '特作B合計

                WW_TOTAL = CInt(T00009row("TOKSACKAISU")) + CInt(T00009row("TOKSACKAISUCHO"))
                T00009row("TOKSACKAISUTTL") = WW_TOTAL.ToString()                                       '特作C合計

                WW_TOTAL = CInt(T00009row("TENKOKAISU")) + CInt(T00009row("TENKOKAISUCHO"))
                T00009row("TENKOKAISUTTL") = WW_TOTAL.ToString()                                        '点呼手当合計

                WW_TOTAL = CInt(T00009row("HOANTIME")) + CInt(T00009row("HOANTIMECHO"))
                T00009row("HOANTIME") = T0007COM.formatHHMM(T00009row("HOANTIME"))                      '保安検査(分)
                T00009row("HOANTIMECHO") = T0007COM.formatHHMM(T00009row("HOANTIMECHO"))                '保安検査調整(分)
                T00009row("HOANTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                                '保安検査合計

                WW_TOTAL = CInt(T00009row("KOATUTIME")) + CInt(T00009row("KOATUTIMECHO"))
                T00009row("KOATUTIME") = T0007COM.formatHHMM(T00009row("KOATUTIME"))                    '高圧作業時間(分)
                T00009row("KOATUTIMECHO") = T0007COM.formatHHMM(T00009row("KOATUTIMECHO"))              '高圧作業時間調整(分)
                T00009row("KOATUTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '高圧作業時間合計

                WW_TOTAL = CInt(T00009row("TOKUSA1TIME")) + CInt(T00009row("TOKUSA1TIMECHO"))
                T00009row("TOKUSA1TIME") = T0007COM.formatHHMM(T00009row("TOKUSA1TIME"))                '特作Ⅰ(分)
                T00009row("TOKUSA1TIMECHO") = T0007COM.formatHHMM(T00009row("TOKUSA1TIMECHO"))          '特作Ⅰ調整(分)
                T00009row("TOKUSA1TIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                             '特作Ⅰ合計

                WW_TOTAL = CInt(T00009row("HAYADETIME")) + CInt(T00009row("HAYADETIMECHO"))
                T00009row("HAYADETIME") = T0007COM.formatHHMM(T00009row("HAYADETIME"))                  '早出補填(分)
                T00009row("HAYADETIMECHO") = T0007COM.formatHHMM(T00009row("HAYADETIMECHO"))            '早出補填調整(分)
                T00009row("HAYADETIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                              '早出補填合計

                WW_TOTAL = CInt(T00009row("PONPNISSU")) + CInt(T00009row("PONPNISSUCHO"))
                T00009row("PONPNISSUTTL") = WW_TOTAL.ToString()                                         'ポンプ合計

                WW_TOTAL = CInt(T00009row("BULKNISSU")) + CInt(T00009row("BULKNISSUCHO"))
                T00009row("BULKNISSUTTL") = WW_TOTAL.ToString()                                         'バルク合計

                WW_TOTAL = CInt(T00009row("TRAILERNISSU")) + CInt(T00009row("TRAILERNISSUCHO"))
                T00009row("TRAILERNISSUTTL") = WW_TOTAL.ToString()                                      'トレーラ合計

                WW_TOTAL = CInt(T00009row("BKINMUKAISU")) + CInt(T00009row("BKINMUKAISUCHO"))
                T00009row("BKINMUKAISUTTL") = WW_TOTAL.ToString()                                       'B勤務合計

                WW_TOTAL = CInt(T00009row("UNLOADCNT")) + CInt(T00009row("UNLOADCNTCHO"))
                T00009row("UNLOADCNTTTL") = WW_TOTAL.ToString()                                         '荷卸回数合計

                WW_TOTAL = CInt(T00009row("HAIDISTANCE")) + CInt(T00009row("HAIDISTANCECHO"))
                T00009row("HAIDISTANCETTL") = WW_TOTAL.ToString()                                       '配送距離合計

                WW_TOTAL = CInt(T00009row("KAIDISTANCE")) + CInt(T00009row("KAIDISTANCECHO"))
                T00009row("KAIDISTANCETTL") = WW_TOTAL.ToString()                                       '回送作業距離合計

                T00009row("HAISOTIME") = T0007COM.formatHHMM(T00009row("HAISOTIME"))                    '配送時間

                WW_TOTAL = CInt(T00009row("NENMATUNISSU")) + CInt(T00009row("NENMATUNISSUCHO"))
                T00009row("NENMATUNISSUTTL") = WW_TOTAL.ToString()                                      '年末出勤日数合計

                WW_TOTAL = CInt(T00009row("SHACHUHAKNISSU")) + CInt(T00009row("SHACHUHAKNISSUCHO"))
                T00009row("SHACHUHAKNISSUTTL") = WW_TOTAL.ToString()                                    '車中泊日数合計

                WW_TOTAL = CInt(T00009row("MODELDISTANCE")) + CInt(T00009row("MODELDISTANCECHO"))
                T00009row("MODELDISTANCETTL") = WW_TOTAL.ToString()                                     'モデル距離合計

                WW_TOTAL = CInt(T00009row("JIKYUSHATIME")) + CInt(T00009row("JIKYUSHATIMECHO"))
                T00009row("JIKYUSHATIME") = T0007COM.formatHHMM(T00009row("JIKYUSHATIME"))              '時給者時間
                T00009row("JIKYUSHATIMECHO") = T0007COM.formatHHMM(T00009row("JIKYUSHATIMECHO"))        '時給者時間調整
                T00009row("JIKYUSHATIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                            '時給者時間合計

                WW_TOTAL = CInt(T00009row("HDAIWORKTIME")) + CInt(T00009row("HDAIWORKTIMECHO"))
                T00009row("HDAIWORKTIME") = T0007COM.formatHHMM(T00009row("HDAIWORKTIME"))              '代休出勤
                T00009row("HDAIWORKTIMECHO") = T0007COM.formatHHMM(T00009row("HDAIWORKTIMECHO"))        '代休出勤調整
                T00009row("HDAIWORKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                            '代休出勤合計

                WW_TOTAL = CInt(T00009row("HDAINIGHTTIME")) + CInt(T00009row("HDAINIGHTTIMECHO"))
                T00009row("HDAINIGHTTIME") = T0007COM.formatHHMM(T00009row("HDAINIGHTTIME"))            '代休深夜
                T00009row("HDAINIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("HDAINIGHTTIMECHO"))      '代休深夜調整
                T00009row("HDAINIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                           '代休深夜合計

                WW_TOTAL = CInt(T00009row("SDAIWORKTIME")) + CInt(T00009row("SDAIWORKTIMECHO"))
                T00009row("SDAIWORKTIME") = T0007COM.formatHHMM(T00009row("SDAIWORKTIME"))              '日曜代休出勤
                T00009row("SDAIWORKTIMECHO") = T0007COM.formatHHMM(T00009row("SDAIWORKTIMECHO"))        '日曜代休出勤調整
                T00009row("SDAIWORKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                            '日曜代休出勤合計

                WW_TOTAL = CInt(T00009row("SDAINIGHTTIME")) + CInt(T00009row("SDAINIGHTTIMECHO"))
                T00009row("SDAINIGHTTIME") = T0007COM.formatHHMM(T00009row("SDAINIGHTTIME"))            '日曜代休深夜
                T00009row("SDAINIGHTTIMECHO") = T0007COM.formatHHMM(T00009row("SDAINIGHTTIMECHO"))      '日曜代休深夜調整
                T00009row("SDAINIGHTTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                           '日曜代休深夜合計

                WW_TOTAL = CInt(T00009row("WWORKTIME")) + CInt(T00009row("WWORKTIMECHO"))
                T00009row("WWORKTIME") = T0007COM.formatHHMM(T00009row("WWORKTIME"))                    '所定内時間
                T00009row("WWORKTIMECHO") = T0007COM.formatHHMM(T00009row("WWORKTIMECHO"))              '所定内時間調整
                T00009row("WWORKTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '所定内時間合計

                WW_TOTAL = CInt(T00009row("JYOMUTIME")) + CInt(T00009row("JYOMUTIMECHO"))
                T00009row("JYOMUTIME") = T0007COM.formatHHMM(T00009row("JYOMUTIME"))                    '乗務時間
                T00009row("JYOMUTIMECHO") = T0007COM.formatHHMM(T00009row("JYOMUTIMECHO"))              '乗務時間調整
                T00009row("JYOMUTIMETTL") = T0007COM.formatHHMM(WW_TOTAL)                               '乗務時間合計

                WW_TOTAL = CInt(T00009row("HWORKNISSU")) + CInt(T00009row("HWORKNISSUCHO"))
                T00009row("HWORKNISSUTTL") = WW_TOTAL.ToString()                                        '休日出勤日数合計

                WW_TOTAL = CInt(T00009row("KAITENCNT")) + CInt(T00009row("KAITENCNTCHO"))
                T00009row("KAITENCNTTTL") = WW_TOTAL.ToString()                                         '回転数合計

                WW_TOTAL = CInt(T00009row("KAITENCNT1_1")) + CInt(T00009row("KAITENCNTCHO1_1"))
                T00009row("KAITENCNTTTL1_1") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT1_2")) + CInt(T00009row("KAITENCNTCHO1_2"))
                T00009row("KAITENCNTTTL1_2") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT1_3")) + CInt(T00009row("KAITENCNTCHO1_3"))
                T00009row("KAITENCNTTTL1_3") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT1_4")) + CInt(T00009row("KAITENCNTCHO1_4"))
                T00009row("KAITENCNTTTL1_4") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT2_1")) + CInt(T00009row("KAITENCNTCHO2_1"))
                T00009row("KAITENCNTTTL2_1") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT2_2")) + CInt(T00009row("KAITENCNTCHO2_2"))
                T00009row("KAITENCNTTTL2_2") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT2_3")) + CInt(T00009row("KAITENCNTCHO2_3"))
                T00009row("KAITENCNTTTL2_3") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("KAITENCNT2_4")) + CInt(T00009row("KAITENCNTCHO2_4"))
                T00009row("KAITENCNTTTL2_4") = WW_TOTAL.ToString()

                WW_TOTAL = CInt(T00009row("SENJYOCNT")) + CInt(T00009row("SENJYOCNTCHO"))
                T00009row("SENJYOCNTTTL") = WW_TOTAL.ToString()                                         '洗浄回数合計

                WW_TOTAL = CInt(T00009row("UNLOADADDCNT1")) + CInt(T00009row("UNLOADADDCNT1CHO"))
                T00009row("UNLOADADDCNT1TTL") = WW_TOTAL.ToString()                                     '危険物荷卸回数1合計

                WW_TOTAL = CInt(T00009row("UNLOADADDCNT2")) + CInt(T00009row("UNLOADADDCNT2CHO"))
                T00009row("UNLOADADDCNT2TTL") = WW_TOTAL.ToString()                                     '危険物荷卸回数2合計

                WW_TOTAL = CInt(T00009row("UNLOADADDCNT3")) + CInt(T00009row("UNLOADADDCNT3CHO"))
                T00009row("UNLOADADDCNT3TTL") = WW_TOTAL.ToString()                                     '危険物荷卸回数3合計

                WW_TOTAL = CInt(T00009row("UNLOADADDCNT4")) + CInt(T00009row("UNLOADADDCNT4CHO"))
                T00009row("UNLOADADDCNT4TTL") = WW_TOTAL.ToString()                                     '危険物荷卸回数4合計

                WW_TOTAL = CInt(T00009row("LOADINGCNT1")) + CInt(T00009row("LOADINGCNT1CHO"))
                T00009row("LOADINGCNT1TTL") = WW_TOTAL.ToString()                                       '危険品積込回数1合計

                WW_TOTAL = CInt(T00009row("LOADINGCNT2")) + CInt(T00009row("LOADINGCNT2CHO"))
                T00009row("LOADINGCNT2TTL") = WW_TOTAL.ToString()                                       '危険品積込回数2合計

                WW_TOTAL = CInt(T00009row("SHORTDISTANCE1")) + CInt(T00009row("SHORTDISTANCE1CHO"))
                T00009row("SHORTDISTANCE1TTL") = WW_TOTAL.ToString()                                    '危険物荷積回数1合計

                WW_TOTAL = CInt(T00009row("SHORTDISTANCE2")) + CInt(T00009row("SHORTDISTANCE2CHO"))
                T00009row("SHORTDISTANCE2TTL") = WW_TOTAL.ToString()                                    '危険物荷積回数2合計

                '状態
                If T00009row("STATUS") = "" AndAlso T00009row("RIYU") <> "" Then
                    T00009row("STATUS") = "01"
                    CODENAME_get("STATUS", T00009row("STATUS"), T00009row("STATUSTEXT"), WW_DUMMY)
                End If

                '名称取得
                CODENAME_get("CAMPCODE", T00009row("CAMPCODE"), T00009row("CAMPNAMES"), WW_DUMMY)                       '会社コード
                CODENAME_get("STAFFCODE", T00009row("STAFFCODE"), T00009row("STAFFNAMES"), WW_DUMMY)                    '従業員コード
                CODENAME_get("WORKINGWEEK", T00009row("WORKINGWEEK"), T00009row("WORKINGWEEKNAMES"), WW_DUMMY)          '営業日曜日
                CODENAME_get("RECODEKBN", T00009row("RECODEKBN"), T00009row("RECODEKBNNAMES"), WW_DUMMY)                'レコード区分
                CODENAME_get("ORG", T00009row("MORG"), T00009row("MORGNAMES"), WW_DUMMY)                                '管理部署
                CODENAME_get("ORG", T00009row("HORG"), T00009row("HORGNAMES"), WW_DUMMY)                                '配属部署
                CODENAME_get("ORG", T00009row("SORG"), T00009row("SORGNAMES"), WW_DUMMY)                                '作業部署
                CODENAME_get("STAFFKBN", T00009row("STAFFKBN"), T00009row("STAFFKBNNAMES"), WW_DUMMY)                   '社員区分
                CODENAME_get("STAFFKBN3", T00009row("STAFFKBN"), T00009row("STAFFKBNTAISHOGAI"), WW_DUMMY)              '残業申請対象外
                CODENAME_get("HOLIDAYKBN", T00009row("HOLIDAYKBN"), T00009row("HOLIDAYKBNNAMES"), WW_DUMMY)             '休日区分
                CODENAME_get("PAYKBN", T00009row("PAYKBN"), T00009row("PAYKBNNAMES"), WW_DUMMY)                         '勤怠区分
                CODENAME_get("SHUKCHOKKBN", T00009row("SHUKCHOKKBN"), T00009row("SHUKCHOKKBNNAMES"), WW_DUMMY)          '宿日直区分
                CODENAME_get("WORKKBN", T00009row("WORKKBN"), T00009row("WORKKBNNAMES"), WW_DUMMY)                      '作業区分
                CODENAME_get("SHARYOKBN", T00009row("SHARYOKBN"), T00009row("SHARYOKBNNAMES"), WW_DUMMY)                '単車・トレーラ区分
                CODENAME_get("OILPAYKBN", T00009row("OILPAYKBN"), T00009row("OILPAYKBNNAMES"), WW_DUMMY)                '油種給与区分
                CODENAME_get("RIYU", T00009row("RIYU"), T00009row("RIYUNAMES"), WW_DUMMY)                               '理由
                CODENAME_get("DELFLG", T00009row("DELFLG"), T00009row("DELFLGNAMES"), WW_DUMMY)                         '削除フラグ
            Next
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:T0007_KINTAI Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        Finally
            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

        '○ 合計行レコードの作成
        CreateTotalDetail()

        '○ 所定労働日数初期設定(全員分)
        WORKNISSUEdit()

        '○ 月合計レコードの集計
        T0007COM.T0007_TotalRecodeEdit(T00009tbl)

        '○ 調整レコードの再作成
        T0007COM.T0007_ChoseiRecodeCreate(T00009tbl)

    End Sub

    ''' <summary>
    ''' 合計明細行作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CreateTotalDetail()

        Dim WW_HEADtbl As DataTable = T00009tbl.Clone()
        Dim WW_TOTALtbl As DataTable = T00009tbl.Clone()

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '2' and SELECT = 1"
        CS0026TBLSORT.sort(WW_HEADtbl)

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "SELECT, STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        For Each WW_HEADrow As DataRow In WW_HEADtbl.Rows
            CS0026TBLSORT.FILTER = "WORKDATE = '" & WW_HEADrow("WORKDATE") & "'" _
                & " and STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "'" _
                & " and RECODEKBN = '" & WW_HEADrow("RECODEKBN") & "'" _
                & " and HDKBN = 'D' and SELECT = 1"
            CS0026TBLSORT.sort(WW_TOTALtbl)

            If WW_TOTALtbl.Rows.Count = 0 Then
                For i As Integer = 1 To 2
                    For j As Integer = 1 To 10
                        Dim T00009row As DataRow = T00009tbl.NewRow
                        T0007COM.INProw_Init(work.WF_SEL_CAMPCODE.Text, T00009row)
                        'その他の項目は、現在のレコードをコピーする
                        T00009row("HIDDEN") = 1
                        T00009row("CAMPCODE") = WW_HEADrow("CAMPCODE")
                        T00009row("TAISHOYM") = WW_HEADrow("TAISHOYM")
                        T00009row("STAFFCODE") = WW_HEADrow("STAFFCODE")
                        T00009row("WORKDATE") = WW_HEADrow("WORKDATE")
                        T00009row("WORKDAY") = WW_HEADrow("WORKDAY")
                        T00009row("HDKBN") = "D"
                        T00009row("RECODEKBN") = "2"
                        T00009row("SEQ") = (i * 10 + j).ToString()
                        T00009row("MORG") = WW_HEADrow("MORG")
                        T00009row("HORG") = WW_HEADrow("HORG")
                        T00009row("STAFFKBN") = WW_HEADrow("STAFFKBN")
                        T00009row("STDATE") = WW_HEADrow("STDATE")
                        T00009row("ENDDATE") = WW_HEADrow("ENDDATE")
                        T00009row("SHARYOKBN") = i.ToString()
                        T00009row("OILPAYKBN") = j.ToString("00")
                        T00009row("DATAKBN") = "K"
                        T00009tbl.Rows.Add(T00009row)
                    Next
                Next
            End If
        Next

    End Sub

    ''' <summary>
    ''' 所定労働日数初期設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WORKNISSUEdit()

        Dim WW_WORKNISSU As Integer = 0
        Dim WW_WORKNISSU2 As Integer = 0

        Try
            '所定労働日数
            WORKNISSUget(WW_WORKNISSU, WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

            For Each T00009row As DataRow In T00009tbl.Rows
                If T00009row("SELECT") = 1 AndAlso
                    T00009row("HDKBN") = "H" AndAlso
                    T00009row("RECODEKBN") = "2" AndAlso
                    T00009row("WORKNISSUTTL") = 0 Then
                    '所定労働日数
                    WORKNISSUget2(T00009row, WW_WORKNISSU2, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    If WW_WORKNISSU2 > 0 Then
                        T00009row("WORKNISSU") = WW_WORKNISSU2
                        T00009row("WORKNISSUTTL") = WW_WORKNISSU2
                    End If

                    If WW_WORKNISSU2 = 0 Then
                        T00009row("WORKNISSU") = WW_WORKNISSU
                        T00009row("WORKNISSUTTL") = WW_WORKNISSU
                    End If
                End If
            Next
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "T0007_WORKNISSUEdit"          'SUBクラス名
            CS0011LOGWrite.INFPOSI = ""
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 所定労働日数取得(カレンダーマスタより)
    ''' </summary>
    ''' <param name="O_WORKNISSU"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WORKNISSUget(ByRef O_WORKNISSU As Integer, ByRef O_RTN As String)

        O_WORKNISSU = 0
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Try
            Date.TryParse(work.WF_SEL_TAISHOYM.Text & "/01", WW_DATE_ST)
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        Catch ex As Exception
            WW_DATE_ST = Convert.ToDateTime(Date.Now.ToString("yyyy/MM") & "/01")
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        End Try

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    COUNT(*) AS WORKNISSU" _
                & " FROM" _
                & "    MB005_CALENDAR" _
                & " WHERE" _
                & "    CAMPCODE        = @P1" _
                & "    AND WORKINGYMD >= @P2" _
                & "    AND WORKINGYMD <= @P3" _
                & "    AND WORKINGKBN  = '0'" _
                & "    AND DELFLG     <> @P4"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '営業年月日(From)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '営業年月日(To)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = WW_DATE_ST
                PARA3.Value = WW_DATE_END
                PARA4.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    O_WORKNISSU = SQLdr("WORKNISSU")
                End If
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR

                CS0011LOGWrite.INFSUBCLASS = "MB005_CALENDAR"               'SUBクラス名
                CS0011LOGWrite.INFPOSI = "MB005_CALENDAR SELECT"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' 所定労働日数取得2(所定労働時間マスタより)
    ''' </summary>
    ''' <param name="T00009row"></param>
    ''' <param name="O_WORKNISSU"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WORKNISSUget2(ByVal T00009row As DataRow, ByRef O_WORKNISSU As Integer, ByRef O_RTN As String)

        O_WORKNISSU = 0
        O_RTN = C_MESSAGE_NO.NORMAL

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    ISNULL(WORKINGN, 0) AS WORKINGN" _
                & " FROM" _
                & "    MB004_WORKINGH" _
                & " WHERE" _
                & "    CAMPCODE     = @P1" _
                & "    AND HORG     = @P2" _
                & "    AND STAFFKBN = @P3" _
                & "    AND STYMD   <= @P4" _
                & "    AND ENDYMD  >= @P5" _
                & "    AND DELFLG  <> @P6"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '配属部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 5)         '職務区分
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '営業年月日(From)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '営業年月日(To)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = T00009row("CAMPCODE")
                PARA2.Value = T00009row("HORG")
                PARA3.Value = T00009row("STAFFKBN")
                PARA4.Value = T00009row("WORKDATE")
                PARA5.Value = T00009row("WORKDATE")
                PARA6.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    O_WORKNISSU = SQLdr("WORKINGN")
                End If
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR

                CS0011LOGWrite.INFSUBCLASS = "MB004_WORKINGH"               'SUBクラス名
                CS0011LOGWrite.INFPOSI = "MB004_WORKINGH SELECT"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        '○ ヘッダ編集
        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            If T00009INProw("HIDDEN") <> 0 OrElse
                T00009INProw("HDKBN") <> "H" OrElse
                T00009INProw("RECODEKBN") <> "2" Then
                Continue For
            End If

            WF_TAISHOYM.Text = CDate(T00009INProw("TAISHOYM") & "/01").ToString("yyyy/MM")
            WF_STAFFCODE.Text = T00009INProw("STAFFCODE")
            WF_HORG.Text = T00009INProw("HORG")

            '名称取得
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)
            CODENAME_get("ORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)
            Exit For
        Next

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(T00009INPtbl)
        TBLview.RowFilter = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '0'"

        ZeroToBlank(TBLview)

        Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        If Not IsNothing(specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)) Then
            '新潟東港を選択している場合、強制的に画面を変更する
            CS0013ProfView.PROFID = C_DEFAULT_DATAKEY & "_" & (specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)).ToString
        Else
            CS0013ProfView.PROFID = Master.PROF_VIEW
        End If
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.NOCOLUMNWIDTHOPT = -1
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.WITHTAGNAMES = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        TBLview.Dispose()
        TBLview = Nothing

        '○ 曜日表示色変更
        WeekColorChange()

        '○ 明細合計行設定
        DisplayTotal()

        '○ 月間調整画面設定
        DisplayAdjust()

    End Sub

    ''' <summary>
    ''' 00:00をブランクに変換
    ''' </summary>
    ''' <param name="I_VIEW"></param>
    ''' <remarks></remarks>
    Protected Sub ZeroToBlank(ByRef I_VIEW As DataView)

        For Each row As DataRow In I_VIEW.Table.Rows
            '画面に表示しない分は編集しない
            If row("HIDDEN") <> 0 OrElse
                row("HDKBN") <> "H" OrElse
                row("RECODEKBN") <> "0" Then
                Continue For
            End If

            For Each col As DataColumn In I_VIEW.Table.Columns
                '下記項目は00:00をブランクに変更
                If col.ColumnName = "STTIME" OrElse
                    col.ColumnName = "ACTTIME" OrElse
                    col.ColumnName = "BINDSTDATE" OrElse
                    col.ColumnName = "BREAKTIME" OrElse
                    col.ColumnName = "NIGHTTIME" OrElse
                    col.ColumnName = "ORVERTIME" OrElse
                    col.ColumnName = "WNIGHTTIME" OrElse
                    col.ColumnName = "SWORKTIME" OrElse
                    col.ColumnName = "SNIGHTTIME" OrElse
                    col.ColumnName = "HWORKTIME" OrElse
                    col.ColumnName = "HNIGHTTIME" OrElse
                    col.ColumnName = "HAYADETIME" OrElse
                    col.ColumnName = "ORVERTIMEADD" OrElse
                    col.ColumnName = "WNIGHTTIMEADD" OrElse
                    col.ColumnName = "SWORKTIMEADD" OrElse
                    col.ColumnName = "SNIGHTTIMEADD" OrElse
                    col.ColumnName = "YENDTIME" OrElse
                    col.ColumnName = "HDAIWORKTIME" OrElse
                    col.ColumnName = "HDAINIGHTTIME" OrElse
                    col.ColumnName = "SDAIWORKTIME" OrElse
                    col.ColumnName = "SDAINIGHTTIME" Then
                    Dim WW_TIME As String() = row(col).Split(":")

                    If WW_TIME.Count > 1 AndAlso
                        row(col) = "00:00" Then
                        row(col) = ""
                    End If
                End If

                '終了時刻は開始日=終了日の時にブランクに変更
                If col.ColumnName = "ENDTIME" AndAlso
                    row("STDATE") = row("ENDDATE") Then
                    Dim WW_TIME As String() = row(col).Split(":")

                    If WW_TIME.Count > 1 AndAlso
                        row(col) = "00:00" Then
                        row(col) = ""
                    End If
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' 曜日の表示色変更
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WeekColorChange()

        Dim WW_T00009tbl As DataTable = New DataTable
        CS0026TBLSORT.TABLE = T00009INPtbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '0'"
        CS0026TBLSORT.sort(WW_T00009tbl)

        Dim tblDataL As Control = pnlListArea.FindControl(pnlListArea.ID & "_DL").Controls(0)

        For i As Integer = 0 To WW_T00009tbl.Rows.Count - 1
            Dim rows As Control = tblDataL.Controls(i)
            Dim WeekCell As TableCell = Nothing

            For Each cell As TableCell In rows.Controls
                'LabelセルにはIDを持っていないためテキストで探す
                If cell.Text = "月" OrElse
                    cell.Text = "火" OrElse
                    cell.Text = "水" OrElse
                    cell.Text = "木" OrElse
                    cell.Text = "金" OrElse
                    cell.Text = "土" OrElse
                    cell.Text = "日" Then
                    WeekCell = cell
                    Exit For
                End If
            Next

            If IsNothing(WeekCell) Then
                Continue For
            End If

            If WW_T00009tbl.Rows(i)("HOLIDAYKBN") = "0" Then
                '平日は黒
                WeekCell.ForeColor = Color.Black
            Else
                '平日以外は赤
                WeekCell.ForeColor = Color.Red
            End If
        Next

        If Not IsNothing(WW_T00009tbl) Then
            WW_T00009tbl.Clear()
            WW_T00009tbl.Dispose()
            WW_T00009tbl = Nothing
        End If

    End Sub

    ''' <summary>
    ''' 明細合計行表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayTotal()

        If T00009INPtbl.Rows.Count = 0 Then
            Exit Sub
        End If

        '○ 画面に表示する項目を取得
        Dim Proftbl As DataTable = New DataTable
        Dim T00009FTRtbl As DataTable = New DataTable

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()

            Dim SQLStr As String =
                  " SELECT" _
                & "    RTRIM(FIELD)        AS FIELD" _
                & "    , RTRIM(FIELDNAMES) AS FIELDNAMES" _
                & "    , RTRIM(ALIGN)      AS ALIGN" _
                & "    , RTRIM(EFFECT)     AS EFFECT" _
                & "    , POSICOL           AS POSICOL" _
                & "    , WIDTH             AS WIDTH" _
                & "    , RTRIM(OBJECTTYPE) AS OBJECTTYPE" _
                & "    , RTRIM(FIXCOL)     AS FIXCOL" _
                & " FROM" _
                & "    S0025_PROFMVIEW" _
                & " WHERE" _
                & "    CAMPCODE     = @P1" _
                & "    AND PROFID   = @P2" _
                & "    AND MAPID    = @P3" _
                & "    AND VARIANT  = @P4" _
                & "    AND HDKBN    = 'H'" _
                & "    AND TITLEKBN = 'I'" _
                & "    AND STYMD   <= @P5" _
                & "    AND ENDYMD  >= @P5" _
                & "    AND DELFLG  <> @P6" _
                & " ORDER BY" _
                & "    EFFECT DESC" _
                & "    , POSICOL" _
                & "    , FIELD"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'プロファイルID
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)        '画面ID
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)        '変数
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '現在日付
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA3.Value = Master.MAPID
                PARA4.Value = Master.VIEWID
                PARA5.Value = Date.Now
                PARA6.Value = C_DELETE_FLG.DELETE

                Dim WW_PROFID As String = ""
                Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

                If Not IsNothing(specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)) Then
                    '新潟東港を選択している場合、強制的に画面を変更する
                    WW_PROFID = C_DEFAULT_DATAKEY & "_" & (specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)).ToString
                Else
                    WW_PROFID = Master.PROF_VIEW
                End If

                For Each ProfID As String In {WW_PROFID, C_DEFAULT_DATAKEY}
                    PARA2.Value = ProfID

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If Not SQLdr.HasRows Then
                            Continue For
                        End If

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            Proftbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        Proftbl.Load(SQLdr)
                    End Using
                    Exit For
                Next
            Finally
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

        '○ 合計明細行作成
        Dim TotalHCell = New TableHeaderCell With {.ViewStateMode = ViewStateMode.Disabled}
        Dim LengthFix As Integer = 0
        Dim LengthFixLAll As Integer = 0
        Dim LengthFixRAll As Integer = 0

        '○ テーブルに属性追加
        pnlListTotalArea.Attributes.Add("data-generated", "1")
        pnlListTotalArea.Attributes.Add("data-scrolltype", "1")
        pnlListTotalArea.Attributes.Add("data-usersort", "0")

        '○ フッター作成(左)
        Dim TotalPanelL = New Panel With {.ViewStateMode = ViewStateMode.Disabled}
        TotalPanelL.ID = pnlListTotalArea.ID & "_FL"
        Dim TotalTableL = New Table() With {.ViewStateMode = ViewStateMode.Disabled}
        Dim TotalHeaderL = New TableHeaderRow With {.ViewStateMode = ViewStateMode.Disabled}

        '○ フッター作成(右)
        Dim TotalPanelR = New Panel With {.ViewStateMode = ViewStateMode.Disabled}
        TotalPanelR.ID = pnlListTotalArea.ID & "_FR"
        Dim TotalTableR = New Table() With {.ViewStateMode = ViewStateMode.Disabled}
        Dim TotalHeaderR = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}

        For i As Integer = 0 To 1
            TotalHeaderL = New TableHeaderRow With {.ViewStateMode = ViewStateMode.Disabled}
            TotalHeaderR = New TableRow With {.ViewStateMode = ViewStateMode.Disabled}

            '合計行に操作(OPERATION)を設定
            If i = 0 Then
                CS0026TBLSORT.TABLE = T00009INPtbl
                CS0026TBLSORT.SORTING = ""
                CS0026TBLSORT.FILTER = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '1'"
                CS0026TBLSORT.sort(T00009FTRtbl)

                TotalHCell = New TableHeaderCell With {.ViewStateMode = ViewStateMode.Disabled}
                TotalHCell.Text = C_LIST_OPERATION_CODE.NODATA
                LengthFix = 48
                TotalHCell.Style.Add("width", LengthFix.ToString() & "px")
                TotalHCell.Style.Add("text-align", "center")
                TotalHeaderL.Cells.Add(TotalHCell)
                LengthFixLAll = LengthFixLAll + LengthFix + If(LengthFix = 0, 0, 2)
            Else
                CS0026TBLSORT.TABLE = T00009INPtbl
                CS0026TBLSORT.SORTING = ""
                CS0026TBLSORT.FILTER = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '2'"
                CS0026TBLSORT.sort(T00009FTRtbl)

                TotalHCell = New TableHeaderCell With {.ViewStateMode = ViewStateMode.Disabled}
                TotalHCell.Attributes.Add("cellfiedlname", "OPERATION")
                TotalHCell.Text = T00009FTRtbl.Rows(0)("OPERATION")
                LengthFix = 48
                TotalHCell.Style.Add("width", LengthFix.ToString() & "px")
                TotalHCell.Style.Add("text-align", "center")
                TotalHeaderL.Cells.Add(TotalHCell)
            End If

            For j As Integer = 0 To Proftbl.Rows.Count - 1
                Dim TotalCell = New TableCell With {.ViewStateMode = ViewStateMode.Disabled}
                If Proftbl.Rows(j)("EFFECT") = "N" Then
                    Continue For
                End If

                Dim FieldName As String = Convert.ToString(Proftbl.Rows(j)("FIELD"))

                If Convert.ToString(Proftbl.Rows(j)("OBJECTTYPE")) = "2" Then         'TextBox
                    LengthFix = (CInt(Proftbl.Rows(j)("WIDTH")) * 16) + 16
                Else
                    LengthFix = (CInt(Proftbl.Rows(j)("WIDTH")) * 16)
                End If

                If i = 0 Then
                    If Proftbl.Rows(j)("FIXCOL") = "1" Then
                        LengthFixLAll = LengthFixLAll + LengthFix + If(LengthFix = 0, 0, 2)
                    Else
                        LengthFixRAll = LengthFixRAll + LengthFix + If(LengthFix = 0, 0, 2)
                    End If
                End If

                If i = 0 Then
                    '月調整(1行目固定)
                    Select Case FieldName
                        Case "WORKINGWEEKNAMES"         'タイトル
                            TotalCell.Attributes.Add("cellfiedlname", "CHOSEI")
                            TotalCell.Text = "月調整"

                        Case "BREAKTIME"                '休憩時間(分)
                            TotalCell.Text = ""

                        Case Else                       'その他調整(あるならば)
                            FieldName = FieldName & "CHO"

                            If T00009FTRtbl.Columns.Contains(FieldName) Then
                                TotalCell.Text = T00009FTRtbl.Rows(0)(FieldName)
                            Else
                                TotalCell.Text = ""
                            End If
                    End Select

                Else
                    '合計(2行目固定)
                    Select Case FieldName
                        Case "WORKINGWEEKNAMES"         'タイトル
                            TotalCell.Attributes.Add("cellfiedlname", "TOTAL")
                            TotalCell.Text = "合  計"

                        Case "ACTTIME"                  '稼働時間
                            TotalCell.Text = T00009FTRtbl.Rows(0)("ACTTIME")

                        Case "BINDTIME"                 '拘束時間(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("BINDTIME")

                        Case "BREAKTIME"                '休憩時間(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("BREAKTIME")

                        Case "ORVERTIMEADD"             '平日残業時間(調整加算)(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("ORVERTIMEADD")

                        Case "WNIGHTTIMEADD"            '平日深夜時間(調整加算)(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("WNIGHTTIMEADD")

                        Case "SWORKTIMEADD"             '日曜出勤時間(調整加算)(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("SWORKTIMEADD")

                        Case "SNIGHTTIMEADD"            '日曜深夜時間(調整加算)(分)
                            TotalCell.Text = T00009FTRtbl.Rows(0)("SNIGHTTIMEADD")

                        Case Else                       'その他合計(あるならば)
                            FieldName = FieldName & "TTL"

                            If T00009FTRtbl.Columns.Contains(FieldName) Then
                                TotalCell.Text = T00009FTRtbl.Rows(0)(FieldName)
                            Else
                                TotalCell.Text = ""
                            End If
                    End Select
                End If

                TotalCell.Style.Add("text-align", Proftbl.Rows(j)("ALIGN"))

                If (CInt(Proftbl.Rows(j)("WIDTH")) * 16) = 0 Then
                    TotalCell.Style.Add("display", "none")
                Else
                    TotalCell.Style.Add("width", LengthFix.ToString() & "px")
                End If

                If Proftbl.Rows(j)("FIXCOL") = "1" Then
                    TotalHeaderL.Cells.Add(TotalCell)
                Else
                    TotalHeaderR.Cells.Add(TotalCell)
                End If
            Next

            TotalHeaderL.Attributes.Add("ondblclick", "DtabChange();")
            TotalHeaderR.Attributes.Add("ondblclick", "DtabChange();")

            TotalTableL.Rows.Add(TotalHeaderL)
            TotalTableR.Rows.Add(TotalHeaderR)
        Next

        TotalTableL.Style.Add("width", LengthFixLAll.ToString() & "px")
        TotalPanelL.Style.Add("width", LengthFixLAll.ToString() & "px")
        TotalPanelL.Controls.Add(TotalTableL)

        TotalTableR.Style.Add("width", LengthFixRAll.ToString() & "px")
        TotalPanelR.Style.Add("left", LengthFixLAll.ToString() & "px")
        TotalPanelR.Controls.Add(TotalTableR)

        pnlListTotalArea.Controls.Add(TotalPanelL)
        pnlListTotalArea.Controls.Add(TotalPanelR)

        '明細ヘッド編集
        For Each T00009FTRrow As DataRow In T00009FTRtbl.Rows
            WF_TAISHOYM.Text = T00009FTRrow("TAISHOYM")         '対象年月
            WF_STAFFCODE.Text = T00009FTRrow("STAFFCODE")       '従業員
            WF_HORG.Text = T00009FTRrow("HORG")                 '配属部署

            '名称取得
            CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)          '従業員
            CODENAME_get("ORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)                          '配属部署
        Next

        '○ テーブルクローズ
        If Not IsNothing(Proftbl) Then
            Proftbl.Clear()
            Proftbl.Dispose()
            Proftbl = Nothing
        End If

        If Not IsNothing(T00009FTRtbl) Then
            T00009FTRtbl.Clear()
            T00009FTRtbl.Dispose()
            T00009FTRtbl = Nothing
        End If

    End Sub

    ''' <summary>
    ''' 月間調整画面表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayAdjust()

        Dim T00009TTLtbl As DataTable = New DataTable

        CS0026TBLSORT.TABLE = T00009INPtbl
        CS0026TBLSORT.SORTING = ""
        CS0026TBLSORT.FILTER = "HIDDEN = 0 and HDKBN = 'H' and RECODEKBN = '2'"
        CS0026TBLSORT.sort(T00009TTLtbl)

        Dim WW_TIME As Integer = 0
        For Each T00009TTLrow As DataRow In T00009TTLtbl.Rows
            WF_NIGHTTIMETTL.Text = T00009TTLrow("NIGHTTIMETTL")                         '所定深夜時間合計(分)

            WW_TIME = T0007COM.HHMMtoMinutes(T00009TTLrow("ORVERTIMETTL")) + T0007COM.HHMMtoMinutes(T00009TTLrow("ORVERTIMEADD"))
            WF_ORVERTIMETTL.Text = T0007COM.formatHHMM(WW_TIME)                         '平日残業時間合計(分)

            WW_TIME = T0007COM.HHMMtoMinutes(T00009TTLrow("WNIGHTTIMETTL")) + T0007COM.HHMMtoMinutes(T00009TTLrow("WNIGHTTIMEADD"))
            WF_WNIGHTTIMETTL.Text = T0007COM.formatHHMM(WW_TIME)                        '平日深夜時間合計(分)

            WW_TIME = T0007COM.HHMMtoMinutes(T00009TTLrow("SWORKTIMETTL")) + T0007COM.HHMMtoMinutes(T00009TTLrow("SWORKTIMEADD"))
            WF_SWORKTIMETTL.Text = T0007COM.formatHHMM(WW_TIME)                         '日曜出勤時間合計(分)

            WW_TIME = T0007COM.HHMMtoMinutes(T00009TTLrow("SNIGHTTIMETTL")) + T0007COM.HHMMtoMinutes(T00009TTLrow("SNIGHTTIMEADD"))
            WF_SNIGHTTIMETTL.Text = T0007COM.formatHHMM(WW_TIME)                        '日曜深夜時間合計(分)

            WF_HWORKTIMETTL.Text = T00009TTLrow("HWORKTIMETTL")                         '休日出勤時間合計(分)
            WF_HNIGHTTIMETTL.Text = T00009TTLrow("HNIGHTTIMETTL")                       '休日深夜時間合計(分)
            WF_WORKNISSUTTL.Text = T00009TTLrow("WORKNISSUTTL")                         '所労合計
            WF_SHOUKETUNISSUTTL.Text = T00009TTLrow("SHOUKETUNISSUTTL")                 '傷欠合計
            WF_KUMIKETUNISSUTTL.Text = T00009TTLrow("KUMIKETUNISSUTTL")                 '組欠合計
            WF_ETCKETUNISSUTTL.Text = T00009TTLrow("ETCKETUNISSUTTL")                   '他欠合計
            WF_NENKYUNISSUTTL.Text = T00009TTLrow("NENKYUNISSUTTL")                     '年休合計
            WF_TOKUKYUNISSUTTL.Text = T00009TTLrow("TOKUKYUNISSUTTL")                   '特休合計
            WF_CHIKOKSOTAINISSUTTL.Text = T00009TTLrow("CHIKOKSOTAINISSUTTL")           '遅早合計
            WF_STOCKNISSUTTL.Text = T00009TTLrow("STOCKNISSUTTL")                       'ストック休暇合計
            WF_KYOTEIWEEKNISSUTTL.Text = T00009TTLrow("KYOTEIWEEKNISSUTTL")             '協定週休合計
            WF_WEEKNISSUTTL.Text = T00009TTLrow("WEEKNISSUTTL")                         '週休合計
            WF_DAIKYUNISSUTTL.Text = T00009TTLrow("DAIKYUNISSUTTL")                     '代休合計
            WF_NENSHINISSUTTL.Text = T00009TTLrow("NENSHINISSUTTL")                     '年始出勤合計
            WF_SHUKCHOKNNISSUTTL.Text = T00009TTLrow("SHUKCHOKNNISSUTTL")               '宿日直年始合計
            WF_SHUKCHOKNISSUTTL.Text = T00009TTLrow("SHUKCHOKNISSUTTL")                 '宿日直通常合計
            WF_SHUKCHOKNHLDNISSUTTL.Text = T00009TTLrow("SHUKCHOKNHLDNISSUTTL")         '宿日直年始合計(翌日休み)
            WF_SHUKCHOKHLDNISSUTTL.Text = T00009TTLrow("SHUKCHOKHLDNISSUTTL")           '宿日直通常合計(翌日休み)
            WF_TOKUSA1TIMETTL.Text = T00009TTLrow("TOKUSA1TIMETTL")                     '特作Ⅰ合計(分)
            WF_HAYADETIMETTL.Text = T00009TTLrow("HAYADETIMETTL")                       '早出補填合計(分)
            WF_NENMATUNISSUTTL.Text = T00009TTLrow("NENMATUNISSUTTL")                   '年末出勤日数合計
            WF_JIKYUSHATIMETTL.Text = T00009TTLrow("JIKYUSHATIMETTL")                   '時給者時間合計
            WF_HDAIWORKTIMETTL.Text = T00009TTLrow("HDAIWORKTIMETTL")                   '代休出勤合計
            WF_HDAINIGHTTIMETTL.Text = T00009TTLrow("HDAINIGHTTIMETTL")                 '代休深夜合計
            WF_SDAIWORKTIMETTL.Text = T00009TTLrow("SDAIWORKTIMETTL")                   '日曜代休出勤合計
            WF_SDAINIGHTTIMETTL.Text = T00009TTLrow("SDAINIGHTTIMETTL")                 '日曜代休深夜合計
            WF_WWORKTIMETTL.Text = T00009TTLrow("WWORKTIMETTL")                         '所定内時間合計
            WF_HWORKNISSUTTL.Text = T00009TTLrow("HWORKNISSUTTL")                       '休日出勤日数合計
        Next

    End Sub


    ''' <summary>
    ''' 残業再計算ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCALC_Click()

        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_T00009SELtbl As DataTable = New DataTable
        Dim WW_T00009CPYtbl As DataTable = New DataTable
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 対象年月の初日と末日を取得
        Try
            Date.TryParse(WF_TAISHOYM.Text & "/01", WW_DATE_ST)
            WW_DATE_END = WW_DATE_ST.AddMonths(1).AddDays(-1)
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 残業計算
        Dim WW_STAFFKBN As String = ""

        '○ 対象のデータ(現画面に表示している事務員)抽出
        CS0026TBLSORT.TABLE = T00009INPtbl
        CS0026TBLSORT.SORTING = "WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        CS0026TBLSORT.FILTER = "SELECT = 1 and RECODEKBN = '0'" _
            & " and WORKDATE >= #" & WW_DATE_ST & "# and WORKDATE <= #" & WW_DATE_END & "#"
        CS0026TBLSORT.sort(WW_T00009SELtbl)

        '○ 労働時間取得
        For Each T00009SELrow As DataRow In WW_T00009SELtbl.Rows
            If T00009SELrow("HDKBN") = "H" AndAlso T00009SELrow("BINDTIME") = "12:00" Then
                Dim WW_WORKINGH As String = ""
                WORKINGHget(T00009SELrow, WW_WORKINGH, WW_ERR_SW)
                If Not isNormal(WW_ERR_SW) Then
                    Exit Sub
                End If

                T00009SELrow("BINDTIME") = WW_WORKINGH
                T00009SELrow("BINDTIMEMIN") = T0007COM.HHMMtoMinutes(WW_WORKINGH)
            End If

            WW_STAFFKBN = T00009SELrow("STAFFKBN")
        Next

        '○ 更新前データをコピー
        WW_T00009CPYtbl = WW_T00009SELtbl.Copy

        '○ 各会社毎で残業計算を行う
        'エネックス
        If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_ENEX Then
            T0007COM.T0007_KintaiCalc(WW_T00009SELtbl, T00009INPtbl)
        End If

        '近石
        If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_KNK Then
            T0007COM.T0007_KintaiCalc_KNK(WW_T00009SELtbl, T00009INPtbl)
        End If

        'ニュージェイズ
        If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_NJS Then
            T0007COM.T0007_KintaiCalc_NJS(WW_T00009SELtbl, T00009INPtbl)
        End If

        'JKトランス
        If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_JKT Then
            T0007COM.T0007_KintaiCalc_JKT(WW_T00009SELtbl, T00009INPtbl)
        End If

        '○ 時間外計算対象外を判定し、対象外の場合は深夜のみ設定する
        CODENAME_get("STAFFKBN2", WW_STAFFKBN, WW_DUMMY, WW_ERR_SW)
        If isNormal(WW_ERR_SW) Then
            For Each T00009SELrow As DataRow In WW_T00009SELtbl.Rows
                T00009SELrow("ORVERTIME") = "00:00"         '平日残業時間
                T00009SELrow("SWORKTIME") = "00:00"         '日曜出勤時間
                T00009SELrow("HWORKTIME") = "00:00"         '休日出勤時間
                T00009SELrow("HAYADETIME") = "00:00"        '早出補填時間
                T00009SELrow("HDAIWORKTIME") = "00:00"      '代休出勤時間
                T00009SELrow("SDAIWORKTIME") = "00:00"      '日曜代休出勤時間
            Next
        End If

        CS0026TBLSORT.TABLE = WW_T00009SELtbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(WW_T00009SELtbl)

        CS0026TBLSORT.TABLE = WW_T00009CPYtbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(WW_T00009CPYtbl)

        Dim WW_IDX As Integer = 0
        Dim WW_KEYINP As String = ""
        Dim WW_KEYTBL As String = ""

        For Each T00009SELrow As DataRow In WW_T00009SELtbl.Rows
            WW_KEYINP = T00009SELrow("STAFFCODE") & T00009SELrow("WORKDATE") & T00009SELrow("RECODEKBN")
            T00009SELrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA

            If T00009SELrow("HDKBN") = "H" Then
                For i As Integer = WW_IDX To WW_T00009CPYtbl.Rows.Count - 1
                    Dim T00009CPYrow As DataRow = WW_T00009CPYtbl.Rows(i)
                    WW_KEYTBL = T00009CPYrow("STAFFCODE") & T00009CPYrow("WORKDATE") & T00009CPYrow("RECODEKBN")
                    If WW_KEYTBL < WW_KEYINP Then
                        Continue For
                    End If

                    'キー項目が一致する場合
                    If WW_KEYTBL = WW_KEYINP Then
                        If T00009SELrow("ORVERTIME") = T00009CPYrow("ORVERTIME") AndAlso
                            T00009SELrow("WNIGHTTIME") = T00009CPYrow("WNIGHTTIME") AndAlso
                            T00009SELrow("SWORKTIME") = T00009CPYrow("SWORKTIME") AndAlso
                            T00009SELrow("SNIGHTTIME") = T00009CPYrow("SNIGHTTIME") AndAlso
                            T00009SELrow("HWORKTIME") = T00009CPYrow("HWORKTIME") AndAlso
                            T00009SELrow("HNIGHTTIME") = T00009CPYrow("HNIGHTTIME") AndAlso
                            T00009SELrow("NIGHTTIME") = T00009CPYrow("NIGHTTIME") Then
                            '残業再計算前が更新ならそのまま生かす
                            T00009SELrow("OPERATION") = T00009CPYrow("OPERATION")
                        Else
                            '残業計算が変わった場合更新
                            T00009SELrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        End If
                    End If

                    If WW_KEYTBL > WW_KEYINP Then
                        WW_IDX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '○ 入力データをT00009INPtblに反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009INPtbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "WORKDATE < #" & WW_DATE_ST & "# or WORKDATE > #" & WW_DATE_END & "# or RECODEKBN = '2'"
        CS0026TBLSORT.sort(T00009INPtbl)
        T00009INPtbl.Merge(WW_T00009SELtbl)

        '○ 合計レコード編集
        T0007COM.T0007_TotalRecodeCreate(T00009INPtbl)

        '○ 月調整レコード作成
        T0007COM.T0007_ChoseiRecodeCreate(T00009INPtbl)

        '○ 全体データにT00009INPtblを反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009tbl)
        T00009tbl.Merge(T00009INPtbl)

        '○ 再ソート
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00009tbl)

        '○ 項番再採番
        Dim WW_LINECNT As Integer = 0
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("TAISHOYM") = WF_TAISHOYM.Text AndAlso
                T00009row("HDKBN") = "H" AndAlso
                T00009row("DELFLG") = C_DELETE_FLG.ALIVE Then
                T00009row("SELECT") = 1
                T00009row("HIDDEN") = 0
                WW_LINECNT = WW_LINECNT + 1
                T00009row("LINECNT") = WW_LINECNT
            End If
        Next

        If Not IsNothing(WW_T00009SELtbl) Then
            WW_T00009SELtbl.Clear()
            WW_T00009SELtbl.Dispose()
            WW_T00009SELtbl = Nothing
        End If

        If Not IsNothing(WW_T00009CPYtbl) Then
            WW_T00009CPYtbl.Clear()
            WW_T00009CPYtbl.Dispose()
            WW_T00009CPYtbl = Nothing
        End If

        '○ テーブル保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        '○ 絞込ボタン処理
        WF_ButtonExtract_Click()

        '○ 重複チェック
        T0007COM.T0007_DuplCheck(T00009tbl, WW_CheckMES2, WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            WW_CheckMES1 = "内部処理エラー"
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

            CS0011LOGWrite.INFSUBCLASS = "T0007_DuplCheck"          'SUBクラス名
            CS0011LOGWrite.INFPOSI = "T0007_DuplCheck"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = WW_CheckMES1 & WW_CheckMES2
            CS0011LOGWrite.MESSAGENO = WW_ERR_SW
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ABORT)
        End If

    End Sub

    ''' <summary>
    ''' 所定労働時間取得
    ''' </summary>
    ''' <param name="T00009row"></param>
    ''' <param name="O_WORKINGH"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WORKINGHget(ByVal T00009row As DataRow, ByRef O_WORKINGH As String, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    ISNULL(WORKINGH, '00:00:00') AS WORKINGH" _
                & " FROM" _
                & "    MB004_WORKINGH" _
                & " WHERE" _
                & "    CAMPCODE     = @P1" _
                & "    AND HORG     = @P2" _
                & "    AND STAFFKBN = @P3" _
                & "    AND STYMD   <= @P4" _
                & "    AND ENDYMD  >= @P4" _
                & "    AND DELFLG  <> @P5"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '配属部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 5)         '職務区分
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '対象年月日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = T00009row("CAMPCODE")
                PARA2.Value = T00009row("HORG")
                PARA3.Value = T00009row("STAFFKBN")
                PARA4.Value = T00009row("WORKDATE")
                PARA5.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                O_WORKINGH = "12:00"
                While SQLdr.Read
                    If IsDate(SQLdr("WORKINGH")) Then
                        O_WORKINGH = CDate(SQLdr("WORKINGH")).ToString("hh:mm")
                    End If
                End While
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR

                CS0011LOGWrite.INFSUBCLASS = "MB004_WORKINGH"               'SUBクラス名
                CS0011LOGWrite.INFPOSI = "MB004_WORKINGH SELECT"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

    End Sub


    ''' <summary>
    ''' 前頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDOWN_Click()

        '○ 絞込条件が入力されている場合処理しない
        WF_SELSTAFFCODE_TEXT.Text = ""
        If WF_SELSTAFFCODE.Text <> "" Then
            Master.eraseCharToIgnore(WF_SELSTAFFCODE.Text)
            CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_DUMMY)
            Exit Sub
        End If

        '○ 全体データにT00009INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009tbl)
        T00009tbl.Merge(T00009INPtbl)

        '○ 前の事務員を取得(既に最初の場合変更無し)
        Dim prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK, work.WF_SEL_CAMPCODE.Text,
                            work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        leftview.setListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmData)
        Dim WW_STAFF As String = WF_STAFFCODE.Text
        For i As Integer = 0 To leftview.WF_LeftListBox.Items.Count - 1
            If leftview.WF_LeftListBox.Items(i).Value = WF_STAFFCODE.Text Then
                Exit For
            End If

            WW_STAFF = leftview.WF_LeftListBox.Items(i).Value
        Next

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WW_STAFF & "'"
        CS0026TBLSORT.sort(T00009INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' 次頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUP_Click()

        '○ 絞込条件が入力されている場合処理しない
        WF_SELSTAFFCODE_TEXT.Text = ""
        If WF_SELSTAFFCODE.Text <> "" Then
            Master.eraseCharToIgnore(WF_SELSTAFFCODE.Text)
            CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_DUMMY)
            Exit Sub
        End If

        '○ 全体データにT00009INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009tbl)
        T00009tbl.Merge(T00009INPtbl)

        '○ 次の事務員を取得(既に最後の場合変更無し)
        Dim prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK, work.WF_SEL_CAMPCODE.Text,
                        work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
        leftview.setListBox(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WW_DUMMY, prmData)
        Dim WW_STAFF As String = WF_STAFFCODE.Text
        For i As Integer = leftview.WF_LeftListBox.Items.Count - 1 To 0 Step -1
            If leftview.WF_LeftListBox.Items(i).Value = WF_STAFFCODE.Text Then
                Exit For
            End If

            WW_STAFF = leftview.WF_LeftListBox.Items(i).Value
        Next

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WW_STAFF & "'"
        CS0026TBLSORT.sort(T00009INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' 一時保存ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSAVE_Click()

        '○ 一時保存ファイルに出力
        If Not Master.SaveTable(T00009tbl, work.WF_SEL_XMLsaveTMP.Text) Then
            Exit Sub
        End If

        '○ 従業員名称はブランクに
        work.WF_SEL_STAFFNAMES.Text = ""

        '○ メッセージ表示
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○ 使用禁止文字排除
        Master.eraseCharToIgnore(WF_SELSTAFFCODE.Text)

        '○ 名称取得
        CODENAME_get("STAFFCODE", WF_SELSTAFFCODE.Text, WF_SELSTAFFCODE_TEXT.Text, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Master.output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ERR, "絞込従業員 : " & WF_SELSTAFFCODE.Text)
            Exit Sub
        End If

        '○ 全体データにT00009INPtbl(個人)を反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009tbl)
        T00009tbl.Merge(T00009INPtbl)

        '○ 画面表示変更
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        If WF_SELSTAFFCODE.Text = "" Then
            '絞込従業員が空欄の場合、今の画面のまま
            CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        Else
            '絞込従業員を表示
            CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_SELSTAFFCODE.Text & "'"
        End If
        CS0026TBLSORT.sort(T00009INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 現在エラーレコードが1件でもある場合、更新処理を行わない
        Dim WW_ERR As Boolean = False
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED Then
                WW_CheckMES1 = "エラーデータが存在します。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                WW_ERR = True
            End If
        Next

        If WW_ERR Then
            Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ 関連チェック
        RelatedCheck(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE"
            CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
            CS0026TBLSORT.sort(T00009INPtbl)

            '○ テーブル保存
            Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
            Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

            '○ メッセージ表示
            Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ 全データをソート
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00009tbl)

        '○ 初期データ作成(DB未登録データを更新対象にする)
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("TAISHOYM") = WF_TAISHOYM.Text AndAlso
                T00009row("HDKBN") = "H" AndAlso
                T00009row("DBUMUFLG") = "0" Then
                T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
        Next

        '○ 更新を明細行に反映する。また、明細が更新の場合、月合計も更新する
        Dim WW_INDEX As Integer = 0
        Dim WW_KEYHEAD As String = ""
        Dim WW_KEYDTL As String = ""
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("TAISHOYM") = WF_TAISHOYM.Text AndAlso
                T00009row("HDKBN") = "H" AndAlso
                T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                WW_KEYHEAD = T00009row("STAFFCODE") & T00009row("WORKDATE") & T00009row("RECODEKBN")

                For i As Integer = WW_INDEX To T00009tbl.Rows.Count - 1
                    If T00009tbl.Rows(i)("HDKBN") = "D" Then
                        WW_KEYDTL = T00009tbl.Rows(i)("STAFFCODE") & T00009tbl.Rows(i)("WORKDATE") & T00009tbl.Rows(i)("RECODEKBN")
                        If WW_KEYDTL < WW_KEYHEAD Then
                            Continue For
                        End If

                        '日別レコードの明細に更新を設定
                        If WW_KEYDTL = WW_KEYHEAD Then
                            T00009tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        End If

                        If WW_KEYDTL > WW_KEYHEAD Then
                            WW_INDEX = i
                            Exit For
                        End If
                    End If
                Next
            End If
        Next

        WW_INDEX = 0
        WW_KEYHEAD = ""
        WW_KEYDTL = ""
        Dim WW_OLDSTAFF As String = ""
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("TAISHOYM") = WF_TAISHOYM.Text AndAlso
                T00009row("HDKBN") = "H" AndAlso
                T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso
                T00009row("RECODEKBN") = "0" Then
                If WW_OLDSTAFF = T00009row("STAFFCODE") Then
                    WW_OLDSTAFF = T00009row("STAFFCODE")
                    Continue For
                End If

                WW_KEYHEAD = T00009row("STAFFCODE") & "2"
                For i As Integer = WW_INDEX To T00009tbl.Rows.Count - 1
                    WW_KEYDTL = T00009tbl.Rows(i)("STAFFCODE") & T00009tbl.Rows(i)("RECODEKBN")
                    If WW_KEYDTL < WW_KEYHEAD Then
                        Continue For
                    End If

                    If WW_KEYDTL = WW_KEYHEAD Then
                        T00009tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If

                    If WW_KEYDTL > WW_KEYHEAD Then
                        WW_INDEX = i
                        Exit For
                    End If
                Next

                WW_OLDSTAFF = T00009row("STAFFCODE")
            End If
        Next

        '○ 重複チェック
        T0007COM.T0007_DuplCheck(T00009tbl, WW_CheckMES2, WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            WW_CheckMES1 = "内部処理エラー"
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

            CS0011LOGWrite.INFSUBCLASS = "T0007_DuplCheck"          'SUBクラス名
            CS0011LOGWrite.INFPOSI = "T0007_DuplCheck"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = WW_CheckMES1 & WW_CheckMES2
            CS0011LOGWrite.MESSAGENO = WW_ERR_SW
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ABORT)
        End If

        '○ 残業申請の場合、申請ID付番
        Dim WW_NOW As DateTime = Date.Now
        For Each T00009row As DataRow In T00009tbl.Rows
            If T00009row("HDKBN") = "H" AndAlso
                T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso
                T00009row("RECODEKBN") = "0" AndAlso
                T00009row("ENTRYFLG") = "1" Then
                T00009row("APPLYID") = T00009row("STAFFCODE") & CDate(T00009row("WORKDATE")).ToString("MMdd") & WW_NOW.ToString("yyMMddHHmmss")
            End If
        Next

        '○ 勤怠DB更新用のテーブル作成
        Dim WW_UPDATEtbl As DataTable = New DataTable
        Dim WW_KINTAItbl As DataTable = New DataTable

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, TAISHOYM, STAFFCODE, WORKDATE, HDKBN DESC"
        CS0026TBLSORT.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "'" _
            & " and SELECT = 1 and RECODEKBN <> '1'"
        CS0026TBLSORT.sort(WW_UPDATEtbl)

        T0007UPDATE.T0007UPDtbl_ColumnsAdd(WW_KINTAItbl)
        InsertTableEdit(WW_UPDATEtbl, WW_KINTAItbl, WW_NOW)

        '○ 勤怠DB更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            '勤怠DB作成
            UpdateJimKintai(SQLcon, WW_KINTAItbl, WW_NOW)
        End Using

        '○ 申請履歴テーブル登録
        Dim WW_UNAPPROVER As Boolean = True
        For Each UPDATErow As DataRow In WW_UPDATEtbl.Rows
            '申請登録
            If UPDATErow("ENTRYFLG") = "1" Then
                CS0048Apploval.I_CAMPCODE = UPDATErow("CAMPCODE")
                CS0048Apploval.I_APPLYID = UPDATErow("APPLYID")
                CS0048Apploval.I_MAPID = Master.MAPID
                CS0048Apploval.I_EVENTCODE = "残業申請"
                CS0048Apploval.I_SUBCODE = UPDATErow("HORG")
                CS0048Apploval.I_STAFFCODE = UPDATErow("STAFFCODE")
                CS0048Apploval.I_VALUE_C1 = CDate(UPDATErow("WORKDATE")).ToString("yyyy/MM/dd") & "（" & UPDATErow("WORKINGWEEKNAMES") & "）"
                CS0048Apploval.I_VALUE_C2 = UPDATErow("YENDTIME")
                If UPDATErow("RIYUETC") = "" Then
                    CS0048Apploval.I_VALUE_C3 = UPDATErow("RIYUNAMES")
                Else
                    CS0048Apploval.I_VALUE_C3 = Mid(UPDATErow("RIYUNAMES") & "（" & UPDATErow("RIYUETC") & "）", 1, 200)
                End If
                CS0048Apploval.I_VALUE_C4 = UPDATErow("ENDTIME")
                Dim WW_NUM As Integer =
                    T0007COM.HHMMtoMinutes(UPDATErow("ORVERTIME")) +
                    T0007COM.HHMMtoMinutes(UPDATErow("WNIGHTTIME")) +
                    T0007COM.HHMMtoMinutes(UPDATErow("SWORKTIME")) +
                    T0007COM.HHMMtoMinutes(UPDATErow("SNIGHTTIME")) +
                    T0007COM.HHMMtoMinutes(UPDATErow("HWORKTIME")) +
                    T0007COM.HHMMtoMinutes(UPDATErow("HNIGHTTIME"))
                CS0048Apploval.I_VALUE_C5 = T0007COM.formatHHMM(WW_NUM)
                CS0048Apploval.I_UPDUSER = Master.USERID
                CS0048Apploval.I_UPDTERMID = Master.USERTERMID
                CS0048Apploval.CS0048setApply()
                If CS0048Apploval.O_ERR = "99999" Then
                    WW_UNAPPROVER = False
                ElseIf Not isNormal(CS0048Apploval.O_ERR) Then
                    Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0009_APPROVALHIST")
                    Exit Sub
                End If
            End If

            '取下げ
            If UPDATErow("DRAWALFLG") = "1" Then
                CS0048Apploval.I_CAMPCODE = UPDATErow("CAMPCODE")
                CS0048Apploval.I_APPLYID = UPDATErow("APPLYID")
                CS0048Apploval.I_UPDUSER = Master.USERID
                CS0048Apploval.I_UPDTERMID = Master.USERTERMID
                CS0048Apploval.CS0048delApply()
                If Not isNormal(CS0048Apploval.O_ERR) Then
                    Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0009_APPROVALHIST")
                    Exit Sub
                End If
            End If
        Next

        '○ 更新用テーブル初期化
        If Not IsNothing(WW_UPDATEtbl) Then
            WW_UPDATEtbl.Clear()
            WW_UPDATEtbl.Dispose()
            WW_UPDATEtbl = Nothing
        End If

        If Not IsNothing(WW_KINTAItbl) Then
            WW_KINTAItbl.Clear()
            WW_KINTAItbl.Dispose()
            WW_KINTAItbl = Nothing
        End If


        '○ 統計DB更新用のテーブル作成
        Dim WW_SELECtbl As DataTable = New DataTable
        Dim WW_TOKEItbl As DataTable = New DataTable

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "SELECT, WORKDATE, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TBLSORT.FILTER = "SELECT = 1 and RECODEKBN <> '1'"
        CS0026TBLSORT.sort(WW_SELECtbl)

        CS0044L1INSERT.CS0044L1ColmnsAdd(WW_TOKEItbl)

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLtrn As SqlTransaction = Nothing

            Try
                '事務員、勤務
                TOKEItblJimEdit(WW_SELECtbl, WW_TOKEItbl, WW_NOW)

                '月合計(ジャーナル)
                TOKEItblMonthlyTotalEdit(WW_SELECtbl, WW_TOKEItbl, WW_NOW)

                '削除データ
                For Each TOKEIrow As DataRow In WW_TOKEItbl.Rows
                    Dim SQLStr As String =
                          " UPDATE L0001_TOKEI" _
                        & " SET" _
                        & "    DELFLG       = @P7" _
                        & "    , UPDYMD     = @P8" _
                        & "    , UPDUSER    = @P9" _
                        & "    , UPDTERMID  = @P10" _
                        & "    , RECEIVEYMD = @P11" _
                        & " WHERE" _
                        & "    CAMPCODE         = @P1" _
                        & "    AND DENTYPE      = @P2" _
                        & "    AND NACSHUKODATE = @P3" _
                        & "    AND PAYOILKBN    = @P4" _
                        & "    AND PAYSHARYOKBN = @P5" _
                        & "    AND KEYSTAFFCODE = @P6" _
                        & "    AND DELFLG      <> @P7"

                    If TOKEIrow("ACACHANTEI") = "AMD" OrElse TOKEIrow("ACACHANTEI") = "AMC" Then
                        SQLStr &= "    AND ACACHANTEI  IN ('AMD', 'AMC')"
                    End If

                    SQLcmd = New SqlCommand(SQLStr, SQLcon, SQLtrn)

                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '伝票タイプ
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                    '出庫日
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)            '勤怠用油種区分
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             '勤怠用車両区分
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)            '従業員コード(KEY)
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)             '削除フラグ
                    Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)                '更新年月日
                    Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '更新ユーザID
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 30)          '更新端末
                    Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '集信日時

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = "T07"
                    PARA3.Value = TOKEIrow("NACSHUKODATE")
                    PARA4.Value = TOKEIrow("PAYOILKBN")
                    PARA5.Value = TOKEIrow("PAYSHARYOKBN")
                    PARA6.Value = TOKEIrow("KEYSTAFFCODE")
                    PARA7.Value = C_DELETE_FLG.DELETE
                    PARA8.Value = WW_NOW
                    PARA9.Value = Master.USERID
                    PARA10.Value = Master.USERTERMID
                    PARA11.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    '統計DB追加
                    T0007COM.L1Insert(TOKEIrow, SQLcon)
                Next

                If Not IsNothing(SQLtrn) Then
                    SQLtrn.Commit()
                End If
            Catch ex As Exception
                If Not IsNothing(SQLtrn) Then
                    SQLtrn.Rollback()
                End If

                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI")

                CS0011LOGWrite.INFSUBCLASS = "WF_ButtonUPDATE_Click"
                CS0011LOGWrite.INFPOSI = "DB:INSERT L0001_TOKEI"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
                Exit Sub
            Finally
                If Not IsNothing(SQLtrn) Then
                    SQLtrn.Dispose()
                    SQLtrn = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

        '○ 更新用テーブル初期化
        If Not IsNothing(WW_SELECtbl) Then
            WW_SELECtbl.Clear()
            WW_SELECtbl.Dispose()
            WW_SELECtbl = Nothing
        End If

        If Not IsNothing(WW_TOKEItbl) Then
            WW_TOKEItbl.Clear()
            WW_TOKEItbl.Dispose()
            WW_TOKEItbl = Nothing
        End If

        '○ メインのテーブルも初期化
        If Not IsNothing(T00009tbl) Then
            T00009tbl.Clear()
            T00009tbl.Dispose()
            T00009tbl = Nothing
        End If

        If Not IsNothing(T00009INPtbl) Then
            T00009INPtbl.Clear()
            T00009INPtbl.Dispose()
            T00009INPtbl = Nothing
        End If

        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)

        '○ 現在表示している事務員分のデータを格納
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009INPtbl)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        If WW_UNAPPROVER Then
            Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            WW_CheckMES1 = "・承認者（マスター）が未設定のため残業申請できません。"
            WW_CheckMES2 = "総務部へ連絡してください"
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            Master.output(C_MESSAGE_NO.WORNING_RECORD_EXIST, C_MESSAGE_TYPE.WAR)
        End If

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTN As String)

        '○ 初期値設定
        O_RTN = C_MESSAGE_NO.NORMAL
        rightview.setErrorReport("")

        Dim WW_LINE_ERR As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 項目チェック
        For Each T00009row As DataRow In T00009tbl.Rows
            '対象年月が違うデータはチェックしない
            If T00009row("TAISHOYM") <> WF_TAISHOYM.Text OrElse
                T00009row("RECODEKBN") <> "0" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            If T0007COM.CheckHOLIDAY("0", T00009row("PAYKBN")) Then
                Dim WW_ADD_MSG As String = ""

                '宿日直区分
                If T00009row("SHUKCHOKKBN") <> "0" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 宿直区分 =" & T00009row("SHUKCHOKKBN")
                    WW_LINE_ERR = "ERR"
                End If

                '開始時刻
                If T00009row("STTIME") <> "00:00" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 出社時刻 =" & T00009row("STTIME")
                    WW_LINE_ERR = "ERR"
                End If

                '拘束開始時刻
                If T00009row("BINDSTDATE") <> "00:00" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 拘束開始 =" & T00009row("BINDSTDATE")
                    WW_LINE_ERR = "ERR"
                End If

                '終了時刻
                If T00009row("ENDTIME") <> "00:00" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 退社時刻 =" & T00009row("ENDTIME")
                    WW_LINE_ERR = "ERR"
                End If

                '休憩時間(分)
                If T00009row("BREAKTIME") <> "00:00" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 休憩     =" & T00009row("BREAKTIME")
                    WW_LINE_ERR = "ERR"
                End If

                '退社予定時刻
                If T00009row("YENDTIME") <> "00:00" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 予定退社時刻 =" & T00009row("YENDTIME")
                    WW_LINE_ERR = "ERR"
                End If

                '理由
                If T00009row("RIYU") <> "" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 残業理由 =" & T00009row("RIYU")
                    WW_LINE_ERR = "ERR"
                End If

                '理由(その他)
                If T00009row("RIYUETC") <> "" Then
                    WW_ADD_MSG = WW_ADD_MSG & ControlChars.NewLine & "  --> 残業理由（補足） =" & T00009row("RIYUETC")
                    WW_LINE_ERR = "ERR"
                End If

                'エラーレポート出力
                If WW_LINE_ERR = "ERR" Then
                    WW_CheckMES1 = "・更新できないレコードです。"
                    WW_CheckMES2 = "休みが指定されているため、下記項目をクリアしてください。"
                    WW_CheckMES2 = WW_CheckMES2 & WW_ADD_MSG
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                End If
            Else
                '開始日時、終了日時
                If IsDate(T00009row("STDATE")) AndAlso IsDate(T00009row("STTIME")) AndAlso
                    IsDate(T00009row("ENDDATE")) AndAlso IsDate(T00009row("ENDTIME")) Then
                    If Not (T00009row("STDATE") = T00009row("ENDDATE") AndAlso T00009row("ENDTIME") = "00:00") Then
                        Dim WW_DATE_ST As Date = CDate(T00009row("STDATE") & " " & T00009row("STTIME"))
                        Dim WW_DATE_END As Date = CDate(T00009row("ENDDATE") & " " & T00009row("ENDTIME"))

                        '日付大小チェック
                        If WW_DATE_ST > WW_DATE_END Then
                            WW_CheckMES1 = "・更新できないレコード(開始時刻 > 終了時刻)です。"
                            WW_CheckMES2 = WW_DATE_ST.ToString("yyyy/MM/dd HH:mm") & ">" & WW_DATE_END.ToString("yyyy/MM/dd HH:mm")
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                            WW_LINE_ERR = "ERR"
                        End If

                        '休憩時間算出
                        Dim WW_BREAKTIME As Integer = 0
                        Dim WW_MIN As Integer = DateDiff("n", WW_DATE_ST, WW_DATE_END)

                        If WW_MIN <= 360 Then
                            '拘束時間が6時間以下の場合、休憩=0分で良い
                            WW_BREAKTIME = 0
                        ElseIf WW_MIN <= 480 Then
                            '拘束時間が6時間超え、8時間以下の場合、休憩=45分で良い
                            WW_BREAKTIME = 45
                        Else
                            '拘束時間が8時間超えの場合、休憩=60分で良い
                            WW_BREAKTIME = 60
                        End If

                        If IsDate(T00009row("BREAKTIME")) AndAlso
                            WW_BREAKTIME > T0007COM.HHMMtoMinutes(T00009row("BREAKTIME")) Then
                            WW_CheckMES1 = "・更新できないレコード(休憩時間不足)です。"
                            WW_CheckMES2 = T00009row("BREAKTIME")
                            WW_CheckMES2 = WW_CheckMES2 & ControlChars.NewLine & "  --> 拘束時間 6時間以下： 0分 , " _
                                                        & ControlChars.NewLine & "  -->          8時間以下：45分 , " _
                                                        & ControlChars.NewLine & "  -->          8時間超  ：60分   "
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                            WW_LINE_ERR = "ERR"
                        End If
                    End If
                End If

                Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

                '拘束開始時刻
                If Not IsNothing(specialOrg.Items.FindByValue(T00009row("HORG"))) AndAlso
                    IsDate(T00009row("BINDSTDATE")) AndAlso
                    CDate(T00009row("BINDSTDATE")).ToString("HHmm") < "0500" AndAlso
                    CDate(T00009row("BINDSTDATE")).ToString("HHmm") <> "0000" Then
                    WW_CheckMES1 = "・更新できないレコード(拘束開始は５時以降)です。"
                    WW_CheckMES2 = T00009row("BINDSTDATE") & " < 05:00"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                    WW_LINE_ERR = "ERR"
                End If

                '従業員区分が時間外対象外ならチェックをしない(残業申請をしないため)
                CODENAME_get("STAFFKBN3", T00009row("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                CODENAME_get("STAFFKBN4", T00009row("STAFFKBN"), WW_DUMMY, WW_RTN_SW2)
                If Not (WW_RTN_SW = C_MESSAGE_NO.NORMAL OrElse WW_RTN_SW2 = C_MESSAGE_NO.NORMAL) Then
                    If T00009row("HOLIDAYKBN") = "0" Then
                        '時間外計算対象外を判定し、対象外の場合は残業申請を非活性とする
                        If T0007COM.HHMMtoMinutes(T00009row("BINDTIME")) < (T0007COM.HHMMtoMinutes(T00009row("WORKTIME")) - T0007COM.HHMMtoMinutes(T00009row("BREAKTIME"))) Then
                            '理由
                            If T00009row("RIYU") = "" Then
                                WW_CheckMES1 = "・更新できないレコード(残業理由エラー)です。"
                                WW_CheckMES2 = "残業の場合、必須入力"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                WW_LINE_ERR = "ERR"
                            End If

                            '理由（その他）
                            If T00009row("RIYUNAMES") = "その他" AndAlso
                                T00009row("RIYUETC") = "" Then
                                WW_CheckMES1 = "・更新できないレコード(残業理由補足エラー)です。"
                                WW_CheckMES2 = "残業理由が「その他」の場合、必須入力"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                WW_LINE_ERR = "ERR"
                            End If

                            '退社予定時刻
                            If T00009row("YENDTIME") = "00:00" Then
                                WW_CheckMES1 = "・更新できないレコード(予定退社時刻エラー)です。"
                                WW_CheckMES2 = "残業の場合、必須入力"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                WW_LINE_ERR = "ERR"
                            End If
                        Else
                            '退社時刻の入力、未入力判定
                            If T00009row("ENDTIME") = "00:00" Then
                                '退社時刻未入力の場合
                                If Not (T00009row("RIYU") = "" AndAlso T00009row("YENDTIME") = "00:00") Then
                                    '理由
                                    If T00009row("RIYU") = "" Then
                                        WW_CheckMES1 = "・更新できないレコード(残業理由エラー)です。"
                                        WW_CheckMES2 = "退社予定時刻入力時は、必須入力"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                        WW_LINE_ERR = "ERR"
                                    End If

                                    '理由（その他）
                                    If T00009row("RIYUNAMES") = "その他" AndAlso
                                        T00009row("RIYUETC") = "" Then
                                        WW_CheckMES1 = "・更新できないレコード(残業理由補足エラー)です。"
                                        WW_CheckMES2 = "残業理由が「その他」の場合、必須入力"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                        WW_LINE_ERR = "ERR"
                                    End If

                                    '退社予定時刻
                                    If T00009row("YENDTIME") = "00:00" Then
                                        WW_CheckMES1 = "・更新できないレコード(予定退社時刻エラー)です。"
                                        WW_CheckMES2 = "退社予定時刻入力時は、必須入力"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                        WW_LINE_ERR = "ERR"
                                    End If
                                End If
                            Else
                                '退社時刻が入力された場合
                                '理由
                                If T00009row("RIYU") <> "" Then
                                    WW_CheckMES1 = "・更新できないレコード(残業理由エラー)です。"
                                    WW_CheckMES2 = "残業が無いため、入力不要"
                                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                    WW_LINE_ERR = "ERR"
                                End If

                                '理由（その他）
                                If T00009row("RIYUETC") <> "" Then
                                    WW_CheckMES1 = "・更新できないレコード(残業理由補足エラー)です。"
                                    WW_CheckMES2 = "残業が無いため、入力不要"
                                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                    WW_LINE_ERR = "ERR"
                                End If

                                '退社予定時刻
                                If T00009row("YENDTIME") <> "00:00" Then
                                    WW_CheckMES1 = "・更新できないレコード(予定退社時刻エラー)です。"
                                    WW_CheckMES2 = "残業が無いため、入力不要"
                                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                                    WW_LINE_ERR = "ERR"
                                End If
                            End If
                        End If
                    Else
                        '理由
                        If T00009row("RIYU") <> "" Then
                            WW_CheckMES1 = "・更新できないレコード(残業理由エラー)です。"
                            WW_CheckMES2 = "法定休日／法定外休日は、入力不要"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                            WW_LINE_ERR = "ERR"
                        End If

                        '理由（その他）
                        If T00009row("RIYUETC") <> "" Then
                            WW_CheckMES1 = "・更新できないレコード(残業理由補足エラー)です。"
                            WW_CheckMES2 = "法定休日／法定外休日は、入力不要"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                            WW_LINE_ERR = "ERR"
                        End If

                        '退社予定時刻
                        If T00009row("YENDTIME") <> "00:00" Then
                            WW_CheckMES1 = "・更新できないレコード(予定退社時刻エラー)です。"
                            WW_CheckMES2 = "法定休日／法定外休日は、入力不要"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009row)
                            WW_LINE_ERR = "ERR"
                        End If
                    End If
                End If

                '理由が未入力の場合、申請フラグOFF
                If T00009row("ENTRYFLG") = "1" AndAlso
                    ((T00009row("RIYU") = "") OrElse
                     (T00009row("STDATE") = T00009row("ENDDATE") AndAlso
                      T00009row("ENDTIME") = "00:00")) Then
                    T00009row("ENTRYFLG") = "0"
                End If

                '承認済を取り下げる(未申請とする)
                If T00009row("DRAWALFLG") = "1" AndAlso
                    T00009row("STATUS") = "10" Then
                    T00009row("APPLYID") = ""
                End If

                If T00009row("RIYU") = "" Then
                    T00009row("APPLYID") = ""
                End If
            End If

            If WW_LINE_ERR = "ERR" Then
                T00009row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新テーブル編集
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <param name="O_TABLE"></param>
    ''' <param name="I_NOW"></param>
    ''' <remarks></remarks>
    Protected Sub InsertTableEdit(ByVal I_TABLE As DataTable, ByRef O_TABLE As DataTable, ByVal I_NOW As DateTime)

        For Each I_ROW As DataRow In I_TABLE.Rows
            Dim O_ROW As DataRow = O_TABLE.NewRow

            O_ROW("CAMPCODE") = I_ROW("CAMPCODE")                                               '会社コード
            O_ROW("TAISHOYM") = I_ROW("TAISHOYM")                                               '対象年月
            O_ROW("STAFFCODE") = I_ROW("STAFFCODE")                                             '従業員コード
            O_ROW("WORKDATE") = I_ROW("WORKDATE")                                               '勤務年月日
            O_ROW("HDKBN") = I_ROW("HDKBN")                                                     'ヘッダ・明細区分
            O_ROW("RECODEKBN") = I_ROW("RECODEKBN")                                             'レコード区分
            O_ROW("SEQ") = I_ROW("SEQ")                                                         '明細行番号
            O_ROW("ENTRYDATE") = I_NOW.ToString("yyyyMMddHHmmssfff")                            'エントリー日時
            O_ROW("NIPPOLINKCODE") = I_ROW("NIPPOLINKCODE")                                     '日報連結コード
            O_ROW("MORG") = I_ROW("MORG")                                                       '管理部署
            O_ROW("HORG") = I_ROW("HORG")                                                       '配属部署
            O_ROW("SORG") = I_ROW("SORG")                                                       '作業部署
            O_ROW("STAFFKBN") = I_ROW("STAFFKBN")                                               '社員区分
            O_ROW("HOLIDAYKBN") = I_ROW("HOLIDAYKBN")                                           '休日区分
            O_ROW("PAYKBN") = I_ROW("PAYKBN")                                                   '勤怠区分
            O_ROW("SHUKCHOKKBN") = I_ROW("SHUKCHOKKBN")                                         '宿日直区分
            O_ROW("WORKKBN") = I_ROW("WORKKBN")                                                 '作業区分

            '開始日
            If IsDate(I_ROW("STDATE")) Then
                O_ROW("STDATE") = I_ROW("STDATE")
            Else
                O_ROW("STDATE") = I_ROW("WORKDATE")
            End If

            '開始時刻
            If IsDate(I_ROW("STTIME")) Then
                O_ROW("STTIME") = I_ROW("STTIME")
            Else
                O_ROW("STTIME") = "00:00"
            End If

            '終了日
            If IsDate(I_ROW("ENDDATE")) Then
                O_ROW("ENDDATE") = I_ROW("ENDDATE")
            Else
                O_ROW("ENDDATE") = I_ROW("WORKDATE")
            End If

            '終了時刻
            If IsDate(I_ROW("ENDTIME")) Then
                O_ROW("ENDTIME") = I_ROW("ENDTIME")
            Else
                O_ROW("ENDTIME") = "00:00"
            End If

            O_ROW("WORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("WORKTIME"))                       '作業時間
            O_ROW("MOVETIME") = T0007COM.HHMMtoMinutes(I_ROW("MOVETIME"))                       '移動時間
            O_ROW("ACTTIME") = T0007COM.HHMMtoMinutes(I_ROW("ACTTIME"))                         '稼働時間

            '拘束開始時刻
            If IsDate(I_ROW("BINDSTDATE")) Then
                O_ROW("BINDSTDATE") = I_ROW("BINDSTDATE")
            Else
                O_ROW("BINDSTDATE") = "00:00"
            End If

            O_ROW("BINDTIME") = T0007COM.HHMMtoMinutes(I_ROW("BINDTIME"))                       '拘束時間(分)
            O_ROW("NIPPOBREAKTIME") = T0007COM.HHMMtoMinutes(I_ROW("NIPPOBREAKTIME"))           '休憩時間
            O_ROW("BREAKTIME") = T0007COM.HHMMtoMinutes(I_ROW("BREAKTIME"))                     '休憩時間(分)
            O_ROW("BREAKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("BREAKTIMECHO"))               '休憩調整時間(分)
            O_ROW("NIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("NIGHTTIME"))                     '所定深夜時間(分)
            O_ROW("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("NIGHTTIMECHO"))               '所定深夜調整時間(分)
            O_ROW("ORVERTIME") = T0007COM.HHMMtoMinutes(I_ROW("ORVERTIME"))                     '平日残業時間(分)
            O_ROW("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("ORVERTIMECHO"))               '平日残業調整時間(分)
            O_ROW("ORVERTIMEADD") = T0007COM.HHMMtoMinutes(I_ROW("ORVERTIMEADD"))               '平日残業時間(調整加算)(分)
            O_ROW("WNIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("WNIGHTTIME"))                   '平日深夜時間(分)
            O_ROW("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("WNIGHTTIMECHO"))             '平日深夜調整時間(分)
            O_ROW("WNIGHTTIMEADD") = T0007COM.HHMMtoMinutes(I_ROW("WNIGHTTIMEADD"))             '平日深夜時間(調整加算)(分)
            O_ROW("SWORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("SWORKTIME"))                     '日曜出勤時間(分)
            O_ROW("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("SWORKTIMECHO"))               '日曜出勤調整時間(分)
            O_ROW("SWORKTIMEADD") = T0007COM.HHMMtoMinutes(I_ROW("SWORKTIMEADD"))               '日曜出勤時間(調整加算)(分)
            O_ROW("SNIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("SNIGHTTIME"))                   '日曜深夜時間(分)
            O_ROW("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("SNIGHTTIMECHO"))             '日曜深夜調整時間(分)
            O_ROW("SNIGHTTIMEADD") = T0007COM.HHMMtoMinutes(I_ROW("SNIGHTTIMEADD"))             '日曜深夜時間(調整加算)(分)
            O_ROW("HWORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("HWORKTIME"))                     '休日出勤時間(分)
            O_ROW("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HWORKTIMECHO"))               '休日出勤調整時間(分)
            O_ROW("HNIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("HNIGHTTIME"))                   '休日深夜時間(分)
            O_ROW("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HNIGHTTIMECHO"))             '休日深夜調整時間(分)

            O_ROW("WORKNISSU") = I_ROW("WORKNISSU")                                             '所労
            O_ROW("WORKNISSUCHO") = I_ROW("WORKNISSUCHO")                                       '所労調整
            O_ROW("SHOUKETUNISSU") = I_ROW("SHOUKETUNISSU")                                     '傷欠
            O_ROW("SHOUKETUNISSUCHO") = I_ROW("SHOUKETUNISSUCHO")                               '傷欠調整
            O_ROW("KUMIKETUNISSU") = I_ROW("KUMIKETUNISSU")                                     '組欠
            O_ROW("KUMIKETUNISSUCHO") = I_ROW("KUMIKETUNISSUCHO")                               '組欠調整
            O_ROW("ETCKETUNISSU") = I_ROW("ETCKETUNISSU")                                       '他欠
            O_ROW("ETCKETUNISSUCHO") = I_ROW("ETCKETUNISSUCHO")                                 '他欠調整
            O_ROW("NENKYUNISSU") = I_ROW("NENKYUNISSU")                                         '年休
            O_ROW("NENKYUNISSUCHO") = I_ROW("NENKYUNISSUCHO")                                   '年休調整
            O_ROW("TOKUKYUNISSU") = I_ROW("TOKUKYUNISSU")                                       '特休
            O_ROW("TOKUKYUNISSUCHO") = I_ROW("TOKUKYUNISSUCHO")                                 '特休調整
            O_ROW("CHIKOKSOTAINISSU") = I_ROW("CHIKOKSOTAINISSU")                               '遅早
            O_ROW("CHIKOKSOTAINISSUCHO") = I_ROW("CHIKOKSOTAINISSUCHO")                         '遅早調整
            O_ROW("STOCKNISSU") = I_ROW("STOCKNISSU")                                           'ストック休暇
            O_ROW("STOCKNISSUCHO") = I_ROW("STOCKNISSUCHO")                                     'ストック休暇調整
            O_ROW("KYOTEIWEEKNISSU") = I_ROW("KYOTEIWEEKNISSU")                                 '協定週休
            O_ROW("KYOTEIWEEKNISSUCHO") = I_ROW("KYOTEIWEEKNISSUCHO")                           '協定週休調整
            O_ROW("WEEKNISSU") = I_ROW("WEEKNISSU")                                             '週休
            O_ROW("WEEKNISSUCHO") = I_ROW("WEEKNISSUCHO")                                       '週休調整
            O_ROW("DAIKYUNISSU") = I_ROW("DAIKYUNISSU")                                         '代休
            O_ROW("DAIKYUNISSUCHO") = I_ROW("DAIKYUNISSUCHO")                                   '代休調整
            O_ROW("NENSHINISSU") = I_ROW("NENSHINISSU")                                         '年始出勤
            O_ROW("NENSHINISSUCHO") = I_ROW("NENSHINISSUCHO")                                   '年始出勤調整
            O_ROW("SHUKCHOKNNISSU") = I_ROW("SHUKCHOKNNISSU")                                   '宿日直年始
            O_ROW("SHUKCHOKNNISSUCHO") = I_ROW("SHUKCHOKNNISSUCHO")                             '宿日直年始調整
            O_ROW("SHUKCHOKNISSU") = I_ROW("SHUKCHOKNISSU")                                     '宿日直通常
            O_ROW("SHUKCHOKNISSUCHO") = I_ROW("SHUKCHOKNISSUCHO")                               '宿日直通常調整
            O_ROW("SHUKCHOKNHLDNISSU") = I_ROW("SHUKCHOKNHLDNISSU")                             '宿日直年始(翌日休み)
            O_ROW("SHUKCHOKNHLDNISSUCHO") = I_ROW("SHUKCHOKNHLDNISSUCHO")                       '宿日直年始調整(翌日休み)
            O_ROW("SHUKCHOKHLDNISSU") = I_ROW("SHUKCHOKHLDNISSU")                               '宿日直通常(翌日休み)
            O_ROW("SHUKCHOKHLDNISSUCHO") = I_ROW("SHUKCHOKHLDNISSUCHO")                         '宿日直通常調整(翌日休み)
            O_ROW("TOKSAAKAISU") = I_ROW("TOKSAAKAISU")                                         '特作A
            O_ROW("TOKSAAKAISUCHO") = I_ROW("TOKSAAKAISUCHO")                                   '特作A調整
            O_ROW("TOKSABKAISU") = I_ROW("TOKSABKAISU")                                         '特作B
            O_ROW("TOKSABKAISUCHO") = I_ROW("TOKSABKAISUCHO")                                   '特作B調整
            O_ROW("TOKSACKAISU") = I_ROW("TOKSACKAISU")                                         '特作C
            O_ROW("TOKSACKAISUCHO") = I_ROW("TOKSACKAISUCHO")                                   '特作C調整
            O_ROW("TENKOKAISU") = I_ROW("TENKOKAISU")                                           '点呼手当
            O_ROW("TENKOKAISUCHO") = I_ROW("TENKOKAISUCHO")                                     '点呼手当調整

            O_ROW("HOANTIME") = T0007COM.HHMMtoMinutes(I_ROW("HOANTIME"))                       '保安検査(分)
            O_ROW("HOANTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HOANTIMECHO"))                 '保安検査調整(分)
            O_ROW("KOATUTIME") = T0007COM.HHMMtoMinutes(I_ROW("KOATUTIME"))                     '高圧作業時間(分)
            O_ROW("KOATUTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("KOATUTIMECHO"))               '高圧作業時間調整(分)
            O_ROW("TOKUSA1TIME") = T0007COM.HHMMtoMinutes(I_ROW("TOKUSA1TIME"))                 '特作Ⅰ(分)
            O_ROW("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("TOKUSA1TIMECHO"))           '特作Ⅰ調整(分)
            O_ROW("HAYADETIME") = T0007COM.HHMMtoMinutes(I_ROW("HAYADETIME"))                   '早出補填(分)
            O_ROW("HAYADETIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HAYADETIMECHO"))             '早出補填調整(分)

            O_ROW("PONPNISSU") = I_ROW("PONPNISSU")                                             'ポンプ
            O_ROW("PONPNISSUCHO") = I_ROW("PONPNISSUCHO")                                       'ポンプ調整
            O_ROW("BULKNISSU") = I_ROW("BULKNISSU")                                             'バルク
            O_ROW("BULKNISSUCHO") = I_ROW("BULKNISSUCHO")                                       'バルク調整
            O_ROW("TRAILERNISSU") = I_ROW("TRAILERNISSU")                                       'トレーラ
            O_ROW("TRAILERNISSUCHO") = I_ROW("TRAILERNISSUCHO")                                 'トレーラ調整
            O_ROW("BKINMUKAISU") = I_ROW("BKINMUKAISU")                                         'B勤務
            O_ROW("BKINMUKAISUCHO") = I_ROW("BKINMUKAISUCHO")                                   'B勤務調整
            O_ROW("SHARYOKBN") = I_ROW("SHARYOKBN")                                             '単車・トレーラ区分
            O_ROW("OILPAYKBN") = I_ROW("OILPAYKBN")                                             '油種給与区分
            O_ROW("UNLOADCNT") = I_ROW("UNLOADCNT")                                             '荷卸回数
            O_ROW("UNLOADCNTCHO") = I_ROW("UNLOADCNTCHO")                                       '荷卸回数調整
            O_ROW("HAIDISTANCE") = I_ROW("HAIDISTANCE")                                         '配送距離
            O_ROW("HAIDISTANCECHO") = I_ROW("HAIDISTANCECHO")                                   '配送調整距離
            O_ROW("KAIDISTANCE") = I_ROW("KAIDISTANCE")                                         '回送作業距離
            O_ROW("KAIDISTANCECHO") = I_ROW("KAIDISTANCECHO")                                   '回送作業調整距離
            O_ROW("YENDTIME") = I_ROW("YENDTIME")                                               '退社予定時刻
            O_ROW("APPLYID") = I_ROW("APPLYID")                                                 '申請ID
            O_ROW("RIYU") = I_ROW("RIYU")                                                       '理由
            O_ROW("RIYUETC") = I_ROW("RIYUETC")                                                 '理由(その他)

            'NJS専用
            O_ROW("HAISOTIME") = T0007COM.HHMMtoMinutes(I_ROW("HAISOTIME"))                     '配送時間
            O_ROW("NENMATUNISSU") = I_ROW("NENMATUNISSU")                                       '年末出勤日数
            O_ROW("NENMATUNISSUCHO") = I_ROW("NENMATUNISSUCHO")                                 '年末出勤日数調整
            O_ROW("SHACHUHAKKBN") = I_ROW("SHACHUHAKKBN")                                       '車中泊区分
            O_ROW("SHACHUHAKNISSU") = I_ROW("SHACHUHAKNISSU")                                   '車中泊日数
            O_ROW("SHACHUHAKNISSUCHO") = I_ROW("SHACHUHAKNISSUCHO")                             '車中泊日数調整
            O_ROW("MODELDISTANCE") = I_ROW("MODELDISTANCE")                                     'モデル距離
            O_ROW("MODELDISTANCECHO") = I_ROW("MODELDISTANCECHO")                               'モデル距離調整
            O_ROW("JIKYUSHATIME") = T0007COM.HHMMtoMinutes(I_ROW("JIKYUSHATIME"))               '時給者時間
            O_ROW("JIKYUSHATIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("JIKYUSHATIMECHO"))         '時給者時間調整

            '近石専用
            O_ROW("HDAIWORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("HDAIWORKTIME"))               '代休出勤
            O_ROW("HDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HDAIWORKTIMECHO"))         '代休出勤
            O_ROW("HDAINIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("HDAINIGHTTIME"))             '代休深夜
            O_ROW("HDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("HDAINIGHTTIMECHO"))       '代休深夜調整
            O_ROW("SDAIWORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("SDAIWORKTIME"))               '日曜代休出勤
            O_ROW("SDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("SDAIWORKTIMECHO"))         '日曜代休出勤調整
            O_ROW("SDAINIGHTTIME") = T0007COM.HHMMtoMinutes(I_ROW("SDAINIGHTTIME"))             '日曜代休深夜
            O_ROW("SDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("SDAINIGHTTIMECHO"))       '日曜代休深夜調整
            O_ROW("WWORKTIME") = T0007COM.HHMMtoMinutes(I_ROW("WWORKTIME"))                     '所定内時間
            O_ROW("WWORKTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("WWORKTIMECHO"))               '所定内時間調整
            O_ROW("JYOMUTIME") = T0007COM.HHMMtoMinutes(I_ROW("JYOMUTIME"))                     '乗務時間
            O_ROW("JYOMUTIMECHO") = T0007COM.HHMMtoMinutes(I_ROW("JYOMUTIMECHO"))               '乗務時間調整
            O_ROW("HWORKNISSU") = I_ROW("HWORKNISSU")                                           '休日出勤日数
            O_ROW("HWORKNISSUCHO") = I_ROW("HWORKNISSUCHO")                                     '休日出勤日数調整
            O_ROW("KAITENCNT") = I_ROW("KAITENCNT")                                             '回転数
            O_ROW("KAITENCNTCHO") = I_ROW("KAITENCNTCHO")                                       '回転数調整
            O_ROW("KAITENCNT1_1") = I_ROW("KAITENCNT1_1")                                       '回転数1_1
            O_ROW("KAITENCNTCHO1_1") = I_ROW("KAITENCNTCHO1_1")                                 '回転数調整1_1
            O_ROW("KAITENCNT1_2") = I_ROW("KAITENCNT1_2")                                       '回転数1_2
            O_ROW("KAITENCNTCHO1_2") = I_ROW("KAITENCNTCHO1_2")                                 '回転数調整1_2
            O_ROW("KAITENCNT1_3") = I_ROW("KAITENCNT1_3")                                       '回転数1_3
            O_ROW("KAITENCNTCHO1_3") = I_ROW("KAITENCNTCHO1_3")                                 '回転数調整1_3
            O_ROW("KAITENCNT1_4") = I_ROW("KAITENCNT1_4")                                       '回転数1_4
            O_ROW("KAITENCNTCHO1_4") = I_ROW("KAITENCNTCHO1_4")                                 '回転数調整1_4
            O_ROW("KAITENCNT2_1") = I_ROW("KAITENCNT2_1")                                       '回転数2_1
            O_ROW("KAITENCNTCHO2_1") = I_ROW("KAITENCNTCHO2_1")                                 '回転数調整2_1
            O_ROW("KAITENCNT2_2") = I_ROW("KAITENCNT2_2")                                       '回転数2_2
            O_ROW("KAITENCNTCHO2_2") = I_ROW("KAITENCNTCHO2_2")                                 '回転数調整2_2
            O_ROW("KAITENCNT2_3") = I_ROW("KAITENCNT2_3")                                       '回転数2_3
            O_ROW("KAITENCNTCHO2_3") = I_ROW("KAITENCNTCHO2_3")                                 '回転数調整2_3
            O_ROW("KAITENCNT2_4") = I_ROW("KAITENCNT2_4")                                       '回転数2_4
            O_ROW("KAITENCNTCHO2_4") = I_ROW("KAITENCNTCHO2_4")                                 '回転数調整2_4

            'JKT専用
            O_ROW("SENJYOCNT") = I_ROW("SENJYOCNT")                                             '洗浄回数
            O_ROW("SENJYOCNTCHO") = I_ROW("SENJYOCNTCHO")                                       '洗浄回数調整
            O_ROW("UNLOADADDCNT1") = I_ROW("UNLOADADDCNT1")                                     '危険物荷卸回数1
            O_ROW("UNLOADADDCNT1CHO") = I_ROW("UNLOADADDCNT1CHO")                               '危険物荷卸回数1調整
            O_ROW("UNLOADADDCNT2") = I_ROW("UNLOADADDCNT2")                                     '危険物荷卸回数2
            O_ROW("UNLOADADDCNT2CHO") = I_ROW("UNLOADADDCNT2CHO")                               '危険物荷卸回数2調整
            O_ROW("UNLOADADDCNT3") = I_ROW("UNLOADADDCNT3")                                     '危険物荷卸回数3
            O_ROW("UNLOADADDCNT3CHO") = I_ROW("UNLOADADDCNT3CHO")                               '危険物荷卸回数3調整
            O_ROW("UNLOADADDCNT4") = I_ROW("UNLOADADDCNT4")                                     '危険物荷卸回数4
            O_ROW("UNLOADADDCNT4CHO") = I_ROW("UNLOADADDCNT4CHO")                               '危険物荷卸回数4調整
            O_ROW("LOADINGCNT1") = I_ROW("LOADINGCNT1")                                         '危険品積込回数1
            O_ROW("LOADINGCNT1CHO") = I_ROW("LOADINGCNT1CHO")                                   '危険品積込回数1調整
            O_ROW("LOADINGCNT2") = I_ROW("LOADINGCNT2")                                         '危険品積込回数2
            O_ROW("LOADINGCNT2CHO") = I_ROW("LOADINGCNT2CHO")                                   '危険品積込回数2調整
            O_ROW("SHORTDISTANCE1") = I_ROW("SHORTDISTANCE1")                                   '危険物荷積回数1
            O_ROW("SHORTDISTANCE1CHO") = I_ROW("SHORTDISTANCE1CHO")                             '危険物荷積回数1調整
            O_ROW("SHORTDISTANCE2") = I_ROW("SHORTDISTANCE2")                                   '危険物荷積回数2
            O_ROW("SHORTDISTANCE2CHO") = I_ROW("SHORTDISTANCE2CHO")                             '危険物荷積回数2調整

            O_ROW("DELFLG") = I_ROW("DELFLG")                                                   '削除フラグ
            O_ROW("INITYMD") = I_NOW                                                            '登録年月日
            O_ROW("UPDYMD") = I_NOW                                                             '更新年月日
            O_ROW("UPDUSER") = Master.USERID                                                    '更新ユーザID
            O_ROW("UPDTERMID") = Master.USERTERMID                                              '更新端末
            O_ROW("RECEIVEYMD") = C_DEFAULT_YMD                                                 '集信日時

            O_TABLE.Rows.Add(O_ROW)
        Next

    End Sub

    ''' <summary>
    ''' 勤怠DB作成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_TABLE"></param>
    ''' <param name="I_NOW"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateJimKintai(ByVal SQLcon As SqlConnection, ByVal I_TABLE As DataTable, ByVal I_NOW As DateTime)

        '○ DB更新
        Dim SQLStr As String =
              " INSERT INTO T0007_KINTAI" _
            & "    (CAMPCODE" _
            & "    , TAISHOYM" _
            & "    , STAFFCODE" _
            & "    , WORKDATE" _
            & "    , HDKBN" _
            & "    , RECODEKBN" _
            & "    , SEQ" _
            & "    , ENTRYDATE" _
            & "    , NIPPOLINKCODE" _
            & "    , MORG" _
            & "    , HORG" _
            & "    , SORG" _
            & "    , STAFFKBN" _
            & "    , HOLIDAYKBN" _
            & "    , PAYKBN" _
            & "    , SHUKCHOKKBN" _
            & "    , WORKKBN" _
            & "    , STDATE" _
            & "    , STTIME" _
            & "    , ENDDATE" _
            & "    , ENDTIME" _
            & "    , WORKTIME" _
            & "    , MOVETIME" _
            & "    , ACTTIME" _
            & "    , BINDSTDATE" _
            & "    , BINDTIME" _
            & "    , NIPPOBREAKTIME" _
            & "    , BREAKTIME" _
            & "    , BREAKTIMECHO" _
            & "    , NIGHTTIME" _
            & "    , NIGHTTIMECHO" _
            & "    , ORVERTIME" _
            & "    , ORVERTIMECHO" _
            & "    , WNIGHTTIME" _
            & "    , WNIGHTTIMECHO" _
            & "    , SWORKTIME" _
            & "    , SWORKTIMECHO" _
            & "    , SNIGHTTIME" _
            & "    , SNIGHTTIMECHO" _
            & "    , HWORKTIME" _
            & "    , HWORKTIMECHO" _
            & "    , HNIGHTTIME" _
            & "    , HNIGHTTIMECHO" _
            & "    , WORKNISSU" _
            & "    , WORKNISSUCHO" _
            & "    , SHOUKETUNISSU" _
            & "    , SHOUKETUNISSUCHO" _
            & "    , KUMIKETUNISSU" _
            & "    , KUMIKETUNISSUCHO" _
            & "    , ETCKETUNISSU" _
            & "    , ETCKETUNISSUCHO" _
            & "    , NENKYUNISSU" _
            & "    , NENKYUNISSUCHO" _
            & "    , TOKUKYUNISSU" _
            & "    , TOKUKYUNISSUCHO" _
            & "    , CHIKOKSOTAINISSU" _
            & "    , CHIKOKSOTAINISSUCHO" _
            & "    , STOCKNISSU" _
            & "    , STOCKNISSUCHO" _
            & "    , KYOTEIWEEKNISSU" _
            & "    , KYOTEIWEEKNISSUCHO" _
            & "    , WEEKNISSU" _
            & "    , WEEKNISSUCHO" _
            & "    , DAIKYUNISSU" _
            & "    , DAIKYUNISSUCHO" _
            & "    , NENSHINISSU" _
            & "    , NENSHINISSUCHO" _
            & "    , SHUKCHOKNNISSU" _
            & "    , SHUKCHOKNNISSUCHO" _
            & "    , SHUKCHOKNISSU" _
            & "    , SHUKCHOKNISSUCHO" _
            & "    , SHUKCHOKNHLDNISSU" _
            & "    , SHUKCHOKNHLDNISSUCHO" _
            & "    , SHUKCHOKHLDNISSU" _
            & "    , SHUKCHOKHLDNISSUCHO" _
            & "    , TOKSAAKAISU" _
            & "    , TOKSAAKAISUCHO" _
            & "    , TOKSABKAISU" _
            & "    , TOKSABKAISUCHO" _
            & "    , TOKSACKAISU" _
            & "    , TOKSACKAISUCHO" _
            & "    , TENKOKAISU" _
            & "    , TENKOKAISUCHO" _
            & "    , HOANTIME" _
            & "    , HOANTIMECHO" _
            & "    , KOATUTIME" _
            & "    , KOATUTIMECHO" _
            & "    , TOKUSA1TIME" _
            & "    , TOKUSA1TIMECHO" _
            & "    , HAYADETIME" _
            & "    , HAYADETIMECHO" _
            & "    , PONPNISSU" _
            & "    , PONPNISSUCHO" _
            & "    , BULKNISSU" _
            & "    , BULKNISSUCHO" _
            & "    , TRAILERNISSU" _
            & "    , TRAILERNISSUCHO" _
            & "    , BKINMUKAISU" _
            & "    , BKINMUKAISUCHO" _
            & "    , SHARYOKBN" _
            & "    , OILPAYKBN" _
            & "    , UNLOADCNT" _
            & "    , UNLOADCNTCHO" _
            & "    , HAIDISTANCE" _
            & "    , HAIDISTANCECHO" _
            & "    , KAIDISTANCE" _
            & "    , KAIDISTANCECHO" _
            & "    , ORVERTIMEADD" _
            & "    , WNIGHTTIMEADD" _
            & "    , SWORKTIMEADD" _
            & "    , SNIGHTTIMEADD" _
            & "    , YENDTIME" _
            & "    , APPLYID" _
            & "    , RIYU" _
            & "    , RIYUETC" _
            & "    , HAISOTIME" _
            & "    , NENMATUNISSU" _
            & "    , NENMATUNISSUCHO" _
            & "    , SHACHUHAKKBN" _
            & "    , SHACHUHAKNISSU" _
            & "    , SHACHUHAKNISSUCHO" _
            & "    , MODELDISTANCE" _
            & "    , MODELDISTANCECHO" _
            & "    , JIKYUSHATIME" _
            & "    , JIKYUSHATIMECHO" _
            & "    , HDAIWORKTIME" _
            & "    , HDAIWORKTIMECHO" _
            & "    , HDAINIGHTTIME" _
            & "    , HDAINIGHTTIMECHO" _
            & "    , SDAIWORKTIME" _
            & "    , SDAIWORKTIMECHO" _
            & "    , SDAINIGHTTIME" _
            & "    , SDAINIGHTTIMECHO" _
            & "    , WWORKTIME" _
            & "    , WWORKTIMECHO" _
            & "    , JYOMUTIME" _
            & "    , JYOMUTIMECHO" _
            & "    , HWORKNISSU" _
            & "    , HWORKNISSUCHO" _
            & "    , KAITENCNT" _
            & "    , KAITENCNTCHO" _
            & "    , KAITENCNT1_1" _
            & "    , KAITENCNTCHO1_1" _
            & "    , KAITENCNT1_2" _
            & "    , KAITENCNTCHO1_2" _
            & "    , KAITENCNT1_3" _
            & "    , KAITENCNTCHO1_3" _
            & "    , KAITENCNT1_4" _
            & "    , KAITENCNTCHO1_4" _
            & "    , KAITENCNT2_1" _
            & "    , KAITENCNTCHO2_1" _
            & "    , KAITENCNT2_2" _
            & "    , KAITENCNTCHO2_2" _
            & "    , KAITENCNT2_3" _
            & "    , KAITENCNTCHO2_3" _
            & "    , KAITENCNT2_4" _
            & "    , KAITENCNTCHO2_4" _
            & "    , SENJYOCNT" _
            & "    , SENJYOCNTCHO" _
            & "    , UNLOADADDCNT1" _
            & "    , UNLOADADDCNT1CHO" _
            & "    , UNLOADADDCNT2" _
            & "    , UNLOADADDCNT2CHO" _
            & "    , UNLOADADDCNT3" _
            & "    , UNLOADADDCNT3CHO" _
            & "    , UNLOADADDCNT4" _
            & "    , UNLOADADDCNT4CHO" _
            & "    , LOADINGCNT1" _
            & "    , LOADINGCNT1CHO" _
            & "    , LOADINGCNT2" _
            & "    , LOADINGCNT2CHO" _
            & "    , SHORTDISTANCE1" _
            & "    , SHORTDISTANCE1CHO" _
            & "    , SHORTDISTANCE2" _
            & "    , SHORTDISTANCE2CHO" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD)" _
            & " VALUES" _
            & "    (@P1       , @P2      , @P3      , @P4      , @P5" _
            & "    , @P6      , @P7      , @P8      , @P9      , @P10" _
            & "    , @P11     , @P12     , @P13     , @P14     , @P15" _
            & "    , @P16     , @P17     , @P18     , @P19     , @P20" _
            & "    , @P21     , @P22     , @P23     , @P24     , @P25" _
            & "    , @P26     , @P27     , @P28     , @P29     , @P30" _
            & "    , @P31     , @P32     , @P33     , @P34     , @P35" _
            & "    , @P36     , @P37     , @P38     , @P39     , @P40" _
            & "    , @P41     , @P42     , @P43     , @P44     , @P45" _
            & "    , @P46     , @P47     , @P48     , @P49     , @P50" _
            & "    , @P51     , @P52     , @P53     , @P54     , @P55" _
            & "    , @P56     , @P57     , @P58     , @P59     , @P60" _
            & "    , @P61     , @P62     , @P63     , @P64     , @P65" _
            & "    , @P66     , @P67     , @P68     , @P69     , @P70" _
            & "    , @P71     , @P72     , @P73     , @P74     , @P75" _
            & "    , @P76     , @P77     , @P78     , @P79     , @P80" _
            & "    , @P81     , @P82     , @P83     , @P84     , @P85" _
            & "    , @P86     , @P87     , @P88     , @P89     , @P90" _
            & "    , @P91     , @P92     , @P93     , @P94     , @P95" _
            & "    , @P96     , @P97     , @P98     , @P99     , @P100" _
            & "    , @P101    , @P102    , @P103    , @P104    , @P105" _
            & "    , @P106    , @P107    , @P108    , @P109    , @P110" _
            & "    , @P111    , @P112    , @P113    , @P114    , @P115" _
            & "    , @P116    , @P117    , @P118    , @P119    , @P120" _
            & "    , @P121    , @P122    , @P123    , @P124    , @P125" _
            & "    , @P126    , @P127    , @P128    , @P129    , @P130" _
            & "    , @P131    , @P132    , @P133    , @P134    , @P135" _
            & "    , @P136    , @P137    , @P138    , @P139    , @P140" _
            & "    , @P141    , @P162    , @P163    , @P164    , @P165" _
            & "    , @P166    , @P167    , @P168    , @P169    , @P170" _
            & "    , @P171    , @P172    , @P173    , @P174    , @P175" _
            & "    , @P176    , @P177    , @P142    , @P143    , @P144" _
            & "    , @P145    , @P146    , @P147    , @P148    , @P149" _
            & "    , @P150    , @P151    , @P178    , @P179    , @P180" _
            & "    , @P181    , @P152    , @P153    , @P154    , @P155" _
            & "    , @P156    , @P157    , @P158    , @P159    , @P160" _
            & "    , @P161) ;"

        Dim SQLcmd As New SqlCommand()
        Dim SQLtrn As SqlTransaction = Nothing

        Try
            For Each WW_ROW As DataRow In I_TABLE.Rows
                If WW_ROW("HDKBN") = "H" Then
                    '○ 勤怠DB削除処理
                    T0007UPDATE.T0007_Delete(SQLcon, SQLtrn, WW_ROW, I_NOW, WW_ERR_SW, Master.USERID, Master.USERTERMID)
                    If Not isNormal(WW_ERR_SW) Then
                        If Not IsNothing(SQLtrn) Then
                            SQLtrn.Rollback()
                        End If

                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI")
                        Exit Sub
                    End If
                End If

                '○ 勤怠DB追加
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 7)             '対象年月
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)            '従業員コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                    '勤務年月日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             'ヘッダ・明細区分
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)             'レコード区分
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Int)                     '明細行番号
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 25)            'エントリー日時
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 200)           '日報連結コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '管理部署
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '配属部署
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '作業部署
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 5)           '社員区分
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)           '休日区分
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          '勤怠区分
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)          '宿日直区分
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 2)           '作業区分
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Date)                  '開始日
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Time)                  '開始時刻
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Date)                  '終了日
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Time)                  '終了時刻
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)                   '作業時間
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Int)                   '移動時間
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Int)                   '稼働時間
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Time)                  '拘束開始時刻
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Int)                   '拘束時間(分)
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Int)                   '休憩時間
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Int)                   '休憩時間(分)
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Int)                   '休憩調整時間(分)
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.Int)                   '所定深夜時間(分)
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.Int)                   '所定深夜調整時間(分)
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.Int)                   '平日残業時間(分)
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.Int)                   '平日残業調整時間(分)
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.Int)                   '平日深夜時間(分)
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.Int)                   '平日深夜調整時間(分)
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Int)                   '日曜出勤時間(分)
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Int)                   '日曜出勤調整時間(分)
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.Int)                   '日曜深夜時間(分)
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.Int)                   '日曜深夜調整時間(分)
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.Int)                   '休日出勤時間(分)
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.Int)                   '休日出勤調整時間(分)
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.Int)                   '休日深夜時間(分)
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Int)                   '休日深夜調整時間(分)
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Int)                   '所労
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)                   '所労調整
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)                   '傷欠
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Int)                   '傷欠調整
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Int)                   '組欠
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Int)                   '組欠調整
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.Int)                   '他欠
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.Int)                   '他欠調整
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.Int)                   '年休
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.Int)                   '年休調整
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.Int)                   '特休
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.Int)                   '特休調整
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.Int)                   '遅早
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.Int)                   '遅早調整
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.Int)                   'ストック休暇
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.Int)                   'ストック休暇調整
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.Int)                   '協定週休
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.Int)                   '協定週休調整
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.Int)                   '週休
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.Int)                   '週休調整
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.Int)                   '代休
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", SqlDbType.Int)                   '代休調整
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", SqlDbType.Int)                   '年始出勤
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", SqlDbType.Int)                   '年始出勤調整
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", SqlDbType.Int)                   '宿日直年始
                Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", SqlDbType.Int)                   '宿日直年始調整
                Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", SqlDbType.Int)                   '宿日直通常
                Dim PARA71 As SqlParameter = SQLcmd.Parameters.Add("@P71", SqlDbType.Int)                   '宿日直通常調整
                Dim PARA72 As SqlParameter = SQLcmd.Parameters.Add("@P72", SqlDbType.Int)                   '宿日直年始(翌日休み)
                Dim PARA73 As SqlParameter = SQLcmd.Parameters.Add("@P73", SqlDbType.Int)                   '宿日直年始調整(翌日休み)
                Dim PARA74 As SqlParameter = SQLcmd.Parameters.Add("@P74", SqlDbType.Int)                   '宿日直通常(翌日休み)
                Dim PARA75 As SqlParameter = SQLcmd.Parameters.Add("@P75", SqlDbType.Int)                   '宿日直通常調整(翌日休み)
                Dim PARA76 As SqlParameter = SQLcmd.Parameters.Add("@P76", SqlDbType.Int)                   '特作A
                Dim PARA77 As SqlParameter = SQLcmd.Parameters.Add("@P77", SqlDbType.Int)                   '特作A調整
                Dim PARA78 As SqlParameter = SQLcmd.Parameters.Add("@P78", SqlDbType.Int)                   '特作B
                Dim PARA79 As SqlParameter = SQLcmd.Parameters.Add("@P79", SqlDbType.Int)                   '特作B調整
                Dim PARA80 As SqlParameter = SQLcmd.Parameters.Add("@P80", SqlDbType.Int)                   '特作C
                Dim PARA81 As SqlParameter = SQLcmd.Parameters.Add("@P81", SqlDbType.Int)                   '特作C調整
                Dim PARA82 As SqlParameter = SQLcmd.Parameters.Add("@P82", SqlDbType.Decimal)               '点呼手当
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.Decimal)               '点呼手当調整
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.Int)                   '保安検査(分)
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.Int)                   '保安検査調整(分)
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.Int)                   '高圧作業時間(分)
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.Int)                   '高圧作業時間調整(分)
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.Int)                   '特作Ⅰ(分)
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.Int)                   '特作Ⅰ調整(分)
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.Int)                   '早出補填(分)
                Dim PARA91 As SqlParameter = SQLcmd.Parameters.Add("@P91", SqlDbType.Int)                   '早出補填調整(分)
                Dim PARA92 As SqlParameter = SQLcmd.Parameters.Add("@P92", SqlDbType.Int)                   'ポンプ
                Dim PARA93 As SqlParameter = SQLcmd.Parameters.Add("@P93", SqlDbType.Int)                   'ポンプ調整
                Dim PARA94 As SqlParameter = SQLcmd.Parameters.Add("@P94", SqlDbType.Int)                   'バルク
                Dim PARA95 As SqlParameter = SQLcmd.Parameters.Add("@P95", SqlDbType.Int)                   'バルク調整
                Dim PARA96 As SqlParameter = SQLcmd.Parameters.Add("@P96", SqlDbType.Int)                   'トレーラ
                Dim PARA97 As SqlParameter = SQLcmd.Parameters.Add("@P97", SqlDbType.Int)                   'トレーラ調整
                Dim PARA98 As SqlParameter = SQLcmd.Parameters.Add("@P98", SqlDbType.Int)                   'B勤務
                Dim PARA99 As SqlParameter = SQLcmd.Parameters.Add("@P99", SqlDbType.Int)                   'B勤務調整
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.NVarChar, 1)         '単車・トレーラ区分
                Dim PARA101 As SqlParameter = SQLcmd.Parameters.Add("@P101", SqlDbType.NVarChar, 20)        '油種給与区分
                Dim PARA102 As SqlParameter = SQLcmd.Parameters.Add("@P102", SqlDbType.Int)                 '荷卸回数
                Dim PARA103 As SqlParameter = SQLcmd.Parameters.Add("@P103", SqlDbType.Int)                 '荷卸回数調整
                Dim PARA104 As SqlParameter = SQLcmd.Parameters.Add("@P104", SqlDbType.Decimal)             '配送距離
                Dim PARA105 As SqlParameter = SQLcmd.Parameters.Add("@P105", SqlDbType.Decimal)             '配送調整距離
                Dim PARA106 As SqlParameter = SQLcmd.Parameters.Add("@P106", SqlDbType.Decimal)             '回送作業距離
                Dim PARA107 As SqlParameter = SQLcmd.Parameters.Add("@P107", SqlDbType.Decimal)             '回送作業調整距離
                Dim PARA108 As SqlParameter = SQLcmd.Parameters.Add("@P108", SqlDbType.Int)                 '平日残業時間(調整加算)(分)
                Dim PARA109 As SqlParameter = SQLcmd.Parameters.Add("@P109", SqlDbType.Int)                 '平日深夜時間(調整加算)(分)
                Dim PARA110 As SqlParameter = SQLcmd.Parameters.Add("@P110", SqlDbType.Int)                 '日曜出勤時間(調整加算)(分)
                Dim PARA111 As SqlParameter = SQLcmd.Parameters.Add("@P111", SqlDbType.Int)                 '日曜深夜時間(調整加算)(分)
                Dim PARA112 As SqlParameter = SQLcmd.Parameters.Add("@P112", SqlDbType.Time)                '退社予定時刻
                Dim PARA113 As SqlParameter = SQLcmd.Parameters.Add("@P113", SqlDbType.NVarChar, 30)        '申請ID
                Dim PARA114 As SqlParameter = SQLcmd.Parameters.Add("@P114", SqlDbType.NVarChar, 2)         '理由
                Dim PARA115 As SqlParameter = SQLcmd.Parameters.Add("@P115", SqlDbType.NVarChar, 200)       '理由(その他)
                Dim PARA116 As SqlParameter = SQLcmd.Parameters.Add("@P116", SqlDbType.Int)                 '配送時間
                Dim PARA117 As SqlParameter = SQLcmd.Parameters.Add("@P117", SqlDbType.Int)                 '年末出勤日数
                Dim PARA118 As SqlParameter = SQLcmd.Parameters.Add("@P118", SqlDbType.Int)                 '年末出勤日数調整
                Dim PARA119 As SqlParameter = SQLcmd.Parameters.Add("@P119", SqlDbType.NVarChar, 1)         '車中泊区分
                Dim PARA120 As SqlParameter = SQLcmd.Parameters.Add("@P120", SqlDbType.Int)                 '車中泊日数
                Dim PARA121 As SqlParameter = SQLcmd.Parameters.Add("@P121", SqlDbType.Int)                 '車中泊日数調整
                Dim PARA122 As SqlParameter = SQLcmd.Parameters.Add("@P122", SqlDbType.Decimal)             'モデル距離
                Dim PARA123 As SqlParameter = SQLcmd.Parameters.Add("@P123", SqlDbType.Decimal)             'モデル距離調整
                Dim PARA124 As SqlParameter = SQLcmd.Parameters.Add("@P124", SqlDbType.Int)                 '時給者時間
                Dim PARA125 As SqlParameter = SQLcmd.Parameters.Add("@P125", SqlDbType.Int)                 '時給者時間調整
                Dim PARA126 As SqlParameter = SQLcmd.Parameters.Add("@P126", SqlDbType.Int)                 '代休出勤
                Dim PARA127 As SqlParameter = SQLcmd.Parameters.Add("@P127", SqlDbType.Int)                 '代休出勤調整
                Dim PARA128 As SqlParameter = SQLcmd.Parameters.Add("@P128", SqlDbType.Int)                 '代休深夜
                Dim PARA129 As SqlParameter = SQLcmd.Parameters.Add("@P129", SqlDbType.Int)                 '代休深夜調整
                Dim PARA130 As SqlParameter = SQLcmd.Parameters.Add("@P130", SqlDbType.Int)                 '日曜代休出勤
                Dim PARA131 As SqlParameter = SQLcmd.Parameters.Add("@P131", SqlDbType.Int)                 '日曜代休出勤調整
                Dim PARA132 As SqlParameter = SQLcmd.Parameters.Add("@P132", SqlDbType.Int)                 '日曜代休深夜
                Dim PARA133 As SqlParameter = SQLcmd.Parameters.Add("@P133", SqlDbType.Int)                 '日曜代休深夜調整
                Dim PARA134 As SqlParameter = SQLcmd.Parameters.Add("@P134", SqlDbType.Int)                 '所定内時間
                Dim PARA135 As SqlParameter = SQLcmd.Parameters.Add("@P135", SqlDbType.Int)                 '所定内時間調整
                Dim PARA136 As SqlParameter = SQLcmd.Parameters.Add("@P136", SqlDbType.Int)                 '乗務時間
                Dim PARA137 As SqlParameter = SQLcmd.Parameters.Add("@P137", SqlDbType.Int)                 '乗務時間調整
                Dim PARA138 As SqlParameter = SQLcmd.Parameters.Add("@P138", SqlDbType.Int)                 '休日出勤日数
                Dim PARA139 As SqlParameter = SQLcmd.Parameters.Add("@P139", SqlDbType.Int)                 '休日出勤日数調整
                Dim PARA140 As SqlParameter = SQLcmd.Parameters.Add("@P140", SqlDbType.Int)                 '回転数
                Dim PARA141 As SqlParameter = SQLcmd.Parameters.Add("@P141", SqlDbType.Int)                 '回転数調整
                Dim PARA162 As SqlParameter = SQLcmd.Parameters.Add("@P162", SqlDbType.Int)                 '回転数1_1
                Dim PARA163 As SqlParameter = SQLcmd.Parameters.Add("@P163", SqlDbType.Int)                 '回転数調整1_1
                Dim PARA164 As SqlParameter = SQLcmd.Parameters.Add("@P164", SqlDbType.Int)                 '回転数1_2
                Dim PARA165 As SqlParameter = SQLcmd.Parameters.Add("@P165", SqlDbType.Int)                 '回転数調整1_2
                Dim PARA166 As SqlParameter = SQLcmd.Parameters.Add("@P166", SqlDbType.Int)                 '回転数1_3
                Dim PARA167 As SqlParameter = SQLcmd.Parameters.Add("@P167", SqlDbType.Int)                 '回転数調整1_3
                Dim PARA168 As SqlParameter = SQLcmd.Parameters.Add("@P168", SqlDbType.Int)                 '回転数1_4
                Dim PARA169 As SqlParameter = SQLcmd.Parameters.Add("@P169", SqlDbType.Int)                 '回転数調整1_4
                Dim PARA170 As SqlParameter = SQLcmd.Parameters.Add("@P170", SqlDbType.Int)                 '回転数2_1
                Dim PARA171 As SqlParameter = SQLcmd.Parameters.Add("@P171", SqlDbType.Int)                 '回転数調整2_1
                Dim PARA172 As SqlParameter = SQLcmd.Parameters.Add("@P172", SqlDbType.Int)                 '回転数2_2
                Dim PARA173 As SqlParameter = SQLcmd.Parameters.Add("@P173", SqlDbType.Int)                 '回転数調整2_2
                Dim PARA174 As SqlParameter = SQLcmd.Parameters.Add("@P174", SqlDbType.Int)                 '回転数2_3
                Dim PARA175 As SqlParameter = SQLcmd.Parameters.Add("@P175", SqlDbType.Int)                 '回転数調整2_3
                Dim PARA176 As SqlParameter = SQLcmd.Parameters.Add("@P176", SqlDbType.Int)                 '回転数2_4
                Dim PARA177 As SqlParameter = SQLcmd.Parameters.Add("@P177", SqlDbType.Int)                 '回転数調整2_4
                Dim PARA142 As SqlParameter = SQLcmd.Parameters.Add("@P142", SqlDbType.Int)                 '洗浄回数
                Dim PARA143 As SqlParameter = SQLcmd.Parameters.Add("@P143", SqlDbType.Int)                 '洗浄回数調整
                Dim PARA144 As SqlParameter = SQLcmd.Parameters.Add("@P144", SqlDbType.Int)                 '危険物荷卸回数1
                Dim PARA145 As SqlParameter = SQLcmd.Parameters.Add("@P145", SqlDbType.Int)                 '危険物荷卸回数1調整
                Dim PARA146 As SqlParameter = SQLcmd.Parameters.Add("@P146", SqlDbType.Int)                 '危険物荷卸回数2
                Dim PARA147 As SqlParameter = SQLcmd.Parameters.Add("@P147", SqlDbType.Int)                 '危険物荷卸回数2調整
                Dim PARA148 As SqlParameter = SQLcmd.Parameters.Add("@P148", SqlDbType.Int)                 '危険物荷卸回数3
                Dim PARA149 As SqlParameter = SQLcmd.Parameters.Add("@P149", SqlDbType.Int)                 '危険物荷卸回数3調整
                Dim PARA150 As SqlParameter = SQLcmd.Parameters.Add("@P150", SqlDbType.Int)                 '危険物荷卸回数4
                Dim PARA151 As SqlParameter = SQLcmd.Parameters.Add("@P151", SqlDbType.Int)                 '危険物荷卸回数4調整
                Dim PARA178 As SqlParameter = SQLcmd.Parameters.Add("@P178", SqlDbType.Int)                 '危険品積込回数1
                Dim PARA179 As SqlParameter = SQLcmd.Parameters.Add("@P179", SqlDbType.Int)                 '危険品積込回数1調整
                Dim PARA180 As SqlParameter = SQLcmd.Parameters.Add("@P180", SqlDbType.Int)                 '危険品積込回数2
                Dim PARA181 As SqlParameter = SQLcmd.Parameters.Add("@P181", SqlDbType.Int)                 '危険品積込回数2調整
                Dim PARA152 As SqlParameter = SQLcmd.Parameters.Add("@P152", SqlDbType.Int)                 '危険物荷積回数1
                Dim PARA153 As SqlParameter = SQLcmd.Parameters.Add("@P153", SqlDbType.Int)                 '危険物荷積回数1調整
                Dim PARA154 As SqlParameter = SQLcmd.Parameters.Add("@P154", SqlDbType.Int)                 '危険物荷積回数2
                Dim PARA155 As SqlParameter = SQLcmd.Parameters.Add("@P155", SqlDbType.Int)                 '危険物荷積回数2調整
                Dim PARA156 As SqlParameter = SQLcmd.Parameters.Add("@P156", SqlDbType.NVarChar, 1)         '削除フラグ
                Dim PARA157 As SqlParameter = SQLcmd.Parameters.Add("@P157", SqlDbType.DateTime)            '登録年月日
                Dim PARA158 As SqlParameter = SQLcmd.Parameters.Add("@P158", SqlDbType.DateTime)            '更新年月日
                Dim PARA159 As SqlParameter = SQLcmd.Parameters.Add("@P159", SqlDbType.NVarChar, 20)        '更新ユーザID
                Dim PARA160 As SqlParameter = SQLcmd.Parameters.Add("@P160", SqlDbType.NVarChar, 30)        '更新端末
                Dim PARA161 As SqlParameter = SQLcmd.Parameters.Add("@P161", SqlDbType.DateTime)            '集信日時

                PARA1.Value = WW_ROW("CAMPCODE")
                PARA2.Value = WW_ROW("TAISHOYM")
                PARA3.Value = WW_ROW("STAFFCODE")
                PARA4.Value = WW_ROW("WORKDATE")
                PARA5.Value = WW_ROW("HDKBN")
                PARA6.Value = WW_ROW("RECODEKBN")
                PARA7.Value = WW_ROW("SEQ")
                PARA8.Value = WW_ROW("ENTRYDATE")
                PARA9.Value = WW_ROW("NIPPOLINKCODE")
                PARA10.Value = WW_ROW("MORG")
                PARA11.Value = WW_ROW("HORG")
                PARA12.Value = WW_ROW("SORG")
                PARA13.Value = WW_ROW("STAFFKBN")
                PARA14.Value = WW_ROW("HOLIDAYKBN")
                PARA15.Value = WW_ROW("PAYKBN")
                PARA16.Value = WW_ROW("SHUKCHOKKBN")
                PARA17.Value = WW_ROW("WORKKBN")
                PARA18.Value = WW_ROW("STDATE")
                PARA19.Value = WW_ROW("STTIME")
                PARA20.Value = WW_ROW("ENDDATE")
                PARA21.Value = WW_ROW("ENDTIME")
                PARA22.Value = WW_ROW("WORKTIME")
                PARA23.Value = WW_ROW("MOVETIME")
                PARA24.Value = WW_ROW("ACTTIME")
                PARA25.Value = WW_ROW("BINDSTDATE")
                PARA26.Value = WW_ROW("BINDTIME")
                PARA27.Value = WW_ROW("NIPPOBREAKTIME")
                PARA28.Value = WW_ROW("BREAKTIME")
                PARA29.Value = WW_ROW("BREAKTIMECHO")
                PARA30.Value = WW_ROW("NIGHTTIME")
                PARA31.Value = WW_ROW("NIGHTTIMECHO")
                PARA32.Value = WW_ROW("ORVERTIME")
                PARA33.Value = WW_ROW("ORVERTIMECHO")
                PARA34.Value = WW_ROW("WNIGHTTIME")
                PARA35.Value = WW_ROW("WNIGHTTIMECHO")
                PARA36.Value = WW_ROW("SWORKTIME")
                PARA37.Value = WW_ROW("SWORKTIMECHO")
                PARA38.Value = WW_ROW("SNIGHTTIME")
                PARA39.Value = WW_ROW("SNIGHTTIMECHO")
                PARA40.Value = WW_ROW("HWORKTIME")
                PARA41.Value = WW_ROW("HWORKTIMECHO")
                PARA42.Value = WW_ROW("HNIGHTTIME")
                PARA43.Value = WW_ROW("HNIGHTTIMECHO")
                PARA44.Value = WW_ROW("WORKNISSU")
                PARA45.Value = WW_ROW("WORKNISSUCHO")
                PARA46.Value = WW_ROW("SHOUKETUNISSU")
                PARA47.Value = WW_ROW("SHOUKETUNISSUCHO")
                PARA48.Value = WW_ROW("KUMIKETUNISSU")
                PARA49.Value = WW_ROW("KUMIKETUNISSUCHO")
                PARA50.Value = WW_ROW("ETCKETUNISSU")
                PARA51.Value = WW_ROW("ETCKETUNISSUCHO")
                PARA52.Value = WW_ROW("NENKYUNISSU")
                PARA53.Value = WW_ROW("NENKYUNISSUCHO")
                PARA54.Value = WW_ROW("TOKUKYUNISSU")
                PARA55.Value = WW_ROW("TOKUKYUNISSUCHO")
                PARA56.Value = WW_ROW("CHIKOKSOTAINISSU")
                PARA57.Value = WW_ROW("CHIKOKSOTAINISSUCHO")
                PARA58.Value = WW_ROW("STOCKNISSU")
                PARA59.Value = WW_ROW("STOCKNISSUCHO")
                PARA60.Value = WW_ROW("KYOTEIWEEKNISSU")
                PARA61.Value = WW_ROW("KYOTEIWEEKNISSUCHO")
                PARA62.Value = WW_ROW("WEEKNISSU")
                PARA63.Value = WW_ROW("WEEKNISSUCHO")
                PARA64.Value = WW_ROW("DAIKYUNISSU")
                PARA65.Value = WW_ROW("DAIKYUNISSUCHO")
                PARA66.Value = WW_ROW("NENSHINISSU")
                PARA67.Value = WW_ROW("NENSHINISSUCHO")
                PARA68.Value = WW_ROW("SHUKCHOKNNISSU")
                PARA69.Value = WW_ROW("SHUKCHOKNNISSUCHO")
                PARA70.Value = WW_ROW("SHUKCHOKNISSU")
                PARA71.Value = WW_ROW("SHUKCHOKNISSUCHO")
                PARA72.Value = WW_ROW("SHUKCHOKNHLDNISSU")
                PARA73.Value = WW_ROW("SHUKCHOKNHLDNISSUCHO")
                PARA74.Value = WW_ROW("SHUKCHOKHLDNISSU")
                PARA75.Value = WW_ROW("SHUKCHOKHLDNISSUCHO")
                PARA76.Value = WW_ROW("TOKSAAKAISU")
                PARA77.Value = WW_ROW("TOKSAAKAISUCHO")
                PARA78.Value = WW_ROW("TOKSABKAISU")
                PARA79.Value = WW_ROW("TOKSABKAISUCHO")
                PARA80.Value = WW_ROW("TOKSACKAISU")
                PARA81.Value = WW_ROW("TOKSACKAISUCHO")
                PARA82.Value = WW_ROW("TENKOKAISU")
                PARA83.Value = WW_ROW("TENKOKAISUCHO")
                PARA84.Value = WW_ROW("HOANTIME")
                PARA85.Value = WW_ROW("HOANTIMECHO")
                PARA86.Value = WW_ROW("KOATUTIME")
                PARA87.Value = WW_ROW("KOATUTIMECHO")
                PARA88.Value = WW_ROW("TOKUSA1TIME")
                PARA89.Value = WW_ROW("TOKUSA1TIMECHO")
                PARA90.Value = WW_ROW("HAYADETIME")
                PARA91.Value = WW_ROW("HAYADETIMECHO")
                PARA92.Value = WW_ROW("PONPNISSU")
                PARA93.Value = WW_ROW("PONPNISSUCHO")
                PARA94.Value = WW_ROW("BULKNISSU")
                PARA95.Value = WW_ROW("BULKNISSUCHO")
                PARA96.Value = WW_ROW("TRAILERNISSU")
                PARA97.Value = WW_ROW("TRAILERNISSUCHO")
                PARA98.Value = WW_ROW("BKINMUKAISU")
                PARA99.Value = WW_ROW("BKINMUKAISUCHO")
                PARA100.Value = WW_ROW("SHARYOKBN")
                PARA101.Value = WW_ROW("OILPAYKBN")
                PARA102.Value = WW_ROW("UNLOADCNT")
                PARA103.Value = WW_ROW("UNLOADCNTCHO")
                PARA104.Value = WW_ROW("HAIDISTANCE")
                PARA105.Value = WW_ROW("HAIDISTANCECHO")
                PARA106.Value = WW_ROW("KAIDISTANCE")
                PARA107.Value = WW_ROW("KAIDISTANCECHO")
                PARA108.Value = WW_ROW("ORVERTIMEADD")
                PARA109.Value = WW_ROW("WNIGHTTIMEADD")
                PARA110.Value = WW_ROW("SWORKTIMEADD")
                PARA111.Value = WW_ROW("SNIGHTTIMEADD")
                PARA112.Value = WW_ROW("YENDTIME")
                PARA113.Value = WW_ROW("APPLYID")
                PARA114.Value = WW_ROW("RIYU")
                PARA115.Value = WW_ROW("RIYUETC")
                PARA116.Value = WW_ROW("HAISOTIME")
                PARA117.Value = WW_ROW("NENMATUNISSU")
                PARA118.Value = WW_ROW("NENMATUNISSUCHO")
                PARA119.Value = WW_ROW("SHACHUHAKKBN")
                PARA120.Value = WW_ROW("SHACHUHAKNISSU")
                PARA121.Value = WW_ROW("SHACHUHAKNISSUCHO")
                PARA122.Value = WW_ROW("MODELDISTANCE")
                PARA123.Value = WW_ROW("MODELDISTANCECHO")
                PARA124.Value = WW_ROW("JIKYUSHATIME")
                PARA125.Value = WW_ROW("JIKYUSHATIMECHO")
                PARA126.Value = WW_ROW("HDAIWORKTIME")
                PARA127.Value = WW_ROW("HDAIWORKTIMECHO")
                PARA128.Value = WW_ROW("HDAINIGHTTIME")
                PARA129.Value = WW_ROW("HDAINIGHTTIMECHO")
                PARA130.Value = WW_ROW("SDAIWORKTIME")
                PARA131.Value = WW_ROW("SDAIWORKTIMECHO")
                PARA132.Value = WW_ROW("SDAINIGHTTIME")
                PARA133.Value = WW_ROW("SDAINIGHTTIMECHO")
                PARA134.Value = WW_ROW("WWORKTIME")
                PARA135.Value = WW_ROW("WWORKTIMECHO")
                PARA136.Value = WW_ROW("JYOMUTIME")
                PARA137.Value = WW_ROW("JYOMUTIMECHO")
                PARA138.Value = WW_ROW("HWORKNISSU")
                PARA139.Value = WW_ROW("HWORKNISSUCHO")
                PARA140.Value = WW_ROW("KAITENCNT")
                PARA141.Value = WW_ROW("KAITENCNTCHO")
                PARA162.Value = WW_ROW("KAITENCNT1_1")
                PARA163.Value = WW_ROW("KAITENCNTCHO1_1")
                PARA164.Value = WW_ROW("KAITENCNT1_2")
                PARA165.Value = WW_ROW("KAITENCNTCHO1_2")
                PARA166.Value = WW_ROW("KAITENCNT1_3")
                PARA167.Value = WW_ROW("KAITENCNTCHO1_3")
                PARA168.Value = WW_ROW("KAITENCNT1_4")
                PARA169.Value = WW_ROW("KAITENCNTCHO1_4")
                PARA170.Value = WW_ROW("KAITENCNT2_1")
                PARA171.Value = WW_ROW("KAITENCNTCHO2_1")
                PARA172.Value = WW_ROW("KAITENCNT2_2")
                PARA173.Value = WW_ROW("KAITENCNTCHO2_2")
                PARA174.Value = WW_ROW("KAITENCNT2_3")
                PARA175.Value = WW_ROW("KAITENCNTCHO2_3")
                PARA176.Value = WW_ROW("KAITENCNT2_4")
                PARA177.Value = WW_ROW("KAITENCNTCHO2_4")
                PARA142.Value = WW_ROW("SENJYOCNT")
                PARA143.Value = WW_ROW("SENJYOCNTCHO")
                PARA144.Value = WW_ROW("UNLOADADDCNT1")
                PARA145.Value = WW_ROW("UNLOADADDCNT1CHO")
                PARA146.Value = WW_ROW("UNLOADADDCNT2")
                PARA147.Value = WW_ROW("UNLOADADDCNT2CHO")
                PARA148.Value = WW_ROW("UNLOADADDCNT3")
                PARA149.Value = WW_ROW("UNLOADADDCNT3CHO")
                PARA150.Value = WW_ROW("UNLOADADDCNT4")
                PARA151.Value = WW_ROW("UNLOADADDCNT4CHO")
                PARA178.Value = WW_ROW("LOADINGCNT1")
                PARA179.Value = WW_ROW("LOADINGCNT1CHO")
                PARA180.Value = WW_ROW("LOADINGCNT2")
                PARA181.Value = WW_ROW("LOADINGCNT2CHO")
                PARA152.Value = WW_ROW("SHORTDISTANCE1")
                PARA153.Value = WW_ROW("SHORTDISTANCE1CHO")
                PARA154.Value = WW_ROW("SHORTDISTANCE2")
                PARA155.Value = WW_ROW("SHORTDISTANCE2CHO")
                PARA156.Value = WW_ROW("DELFLG")
                PARA157.Value = WW_ROW("INITYMD")
                PARA158.Value = WW_ROW("UPDYMD")
                PARA159.Value = WW_ROW("UPDUSER")
                PARA160.Value = WW_ROW("UPDTERMID")
                PARA161.Value = WW_ROW("RECEIVEYMD")

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            Next

            If Not IsNothing(SQLtrn) Then
                SQLtrn.Commit()
            End If
        Catch ex As Exception
            If Not IsNothing(SQLtrn) Then
                SQLtrn.Rollback()
            End If

            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0007_KINTAI INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:T0007_KINTAI INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        Finally
            If Not IsNothing(SQLtrn) Then
                SQLtrn.Dispose()
                SQLtrn = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' 事務員用テーブル編集
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <param name="O_TABLE"></param>
    ''' <remarks></remarks>
    Protected Sub TOKEItblJimEdit(ByVal I_TABLE As DataTable, ByRef O_TABLE As DataTable, ByVal I_NOW As DateTime)

        Dim WW_T00009tbl As DataTable = New DataTable
        Dim WW_ACHANTEItbl As DataTable = New DataTable

        CS0026TBLSORT.TABLE = I_TABLE
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        CS0026TBLSORT.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "'" _
            & " and STAFFKBN not like '03*'" _
            & " and RECODEKBN = '0' and HDKBN = 'H'"
        CS0026TBLSORT.sort(WW_T00009tbl)

        For Each T00009row As DataRow In WW_T00009tbl.Rows
            Try
                Dim O_ROW As DataRow = O_TABLE.NewRow

                '伝票番号採番
                Dim WW_SEQ As String = "000000"
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.CAMPCODE = T00009row("CAMPCODE")
                CS0033AutoNumber.MORG = work.WF_SEL_HORG.Text
                CS0033AutoNumber.USERID = Master.USERID
                CS0033AutoNumber.getAutoNumber()
                If isNormal(CS0033AutoNumber.ERR) Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    Master.output(CS0033AutoNumber.ERR, C_MESSAGE_TYPE.ABORT, "CS0033AutoNumber:DENNO")
                    Exit Sub
                End If

                'テーブル編集
                O_ROW("CAMPCODE") = T00009row("CAMPCODE")                                               '会社コード
                O_ROW("MOTOCHO") = "LO"                                                                 '元帳(非会計予定を設定)
                O_ROW("VERSION") = "000"                                                                'バージョン
                O_ROW("DENTYPE") = "T07"                                                                '伝票タイプ
                O_ROW("TENKI") = "0"                                                                    '統計転記
                O_ROW("KEIJOYMD") = T00009row("WORKDATE")                                               '計上日付(勤務年月日を設定)
                O_ROW("DENYMD") = T00009row("WORKDATE")                                                 '伝票日付(勤務年月日設定)

                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T00009row("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                O_ROW("DENNO") = T00009row("HORG") & WW_DENNO & WW_SEQ

                '関連伝票№ + 明細№
                O_ROW("KANRENDENNO") = T00009row("HORG") & " " _
                    & T00009row("STAFFCODE") & " " _
                    & T00009row("WORKDATE") & " "

                O_ROW("ACTORICODE") = ""                                                                '取引先コード
                O_ROW("ACOILTYPE") = ""                                                                 '油種
                O_ROW("ACSHARYOTYPE") = ""                                                              '統一車番(上)
                O_ROW("ACTSHABAN") = ""                                                                 '統一車番(下)
                O_ROW("ACSTAFFCODE") = ""                                                               '従業員コード
                O_ROW("ACBANKAC") = ""                                                                  '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050SESSION.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If isNormal(CS0006TERMchk.ERR) Then
                    O_ROW("ACKEIJOMORG") = CS0006TERMchk.MORG                                           '計上管理部署コード(部・支店)
                Else
                    O_ROW("ACKEIJOMORG") = T00009row("MORG")                                            '計上管理部署コード(管理部署)
                End If

                O_ROW("ACTAXKBN") = 0                                                                   '税区分
                O_ROW("ACAMT") = 0                                                                      '金額
                O_ROW("NACSHUKODATE") = T00009row("WORKDATE")                                           '勤務日
                O_ROW("NACSHUKADATE") = C_DEFAULT_YMD                                                   '出荷日
                O_ROW("NACTODOKEDATE") = C_DEFAULT_YMD                                                  '届日
                O_ROW("NACTORICODE") = ""                                                               '荷主コード
                O_ROW("NACURIKBN") = ""                                                                 '売上計上基準
                O_ROW("NACTODOKECODE") = ""                                                             '届先コード
                O_ROW("NACSTORICODE") = ""                                                              '販売店コード
                O_ROW("NACSHUKABASHO") = ""                                                             '出荷場所

                O_ROW("NACTORITYPE01") = ""                                                             '取引先・取引タイプ01
                O_ROW("NACTORITYPE02") = ""                                                             '取引先・取引タイプ02
                O_ROW("NACTORITYPE03") = ""                                                             '取引先・取引タイプ03
                O_ROW("NACTORITYPE04") = ""                                                             '取引先・取引タイプ04
                O_ROW("NACTORITYPE05") = ""                                                             '取引先・取引タイプ05

                O_ROW("NACOILTYPE") = ""                                                                '油種
                O_ROW("NACPRODUCT1") = ""                                                               '品名1
                O_ROW("NACPRODUCT2") = ""                                                               '品名2
                O_ROW("NACGSHABAN") = ""                                                                '業務車番
                O_ROW("NACSUPPLIERKBN") = ""                                                            '社有・庸車区分
                O_ROW("NACSUPPLIER") = ""                                                               '庸車会社
                O_ROW("NACSHARYOOILTYPE") = ""                                                          '車両登録油種

                O_ROW("NACSHARYOTYPE1") = ""                                                            '統一車番(上)1
                O_ROW("NACTSHABAN1") = ""                                                               '統一車番(下)1
                O_ROW("NACMANGMORG1") = ""                                                              '車両管理部署1
                O_ROW("NACMANGSORG1") = ""                                                              '車両設置部署1
                O_ROW("NACMANGUORG1") = ""                                                              '車両運用部署1
                O_ROW("NACBASELEASE1") = ""                                                             '車両所有1

                O_ROW("NACSHARYOTYPE2") = ""                                                            '統一車番(上)2
                O_ROW("NACTSHABAN2") = ""                                                               '統一車番(下)2
                O_ROW("NACMANGMORG2") = ""                                                              '車両管理部署2
                O_ROW("NACMANGSORG2") = ""                                                              '車両設置部署2
                O_ROW("NACMANGUORG2") = ""                                                              '車両運用部署2
                O_ROW("NACBASELEASE2") = ""                                                             '車両所有2

                O_ROW("NACSHARYOTYPE3") = ""                                                            '統一車番(上)3
                O_ROW("NACTSHABAN3") = ""                                                               '統一車番(下)3
                O_ROW("NACMANGMORG3") = ""                                                              '車両管理部署3
                O_ROW("NACMANGSORG3") = ""                                                              '車両設置部署3
                O_ROW("NACMANGUORG3") = ""                                                              '車両運用部署3
                O_ROW("NACBASELEASE3") = ""                                                             '車両所有3

                O_ROW("NACCREWKBN") = ""                                                                '正副区分
                O_ROW("NACSTAFFCODE") = ""                                                              '従業員コード(正)
                O_ROW("NACSTAFFKBN") = ""                                                               '社員区分(正)
                O_ROW("NACMORG") = ""                                                                   '管理部署(正)
                O_ROW("NACHORG") = ""                                                                   '配属部署(正)
                O_ROW("NACSORG") = ""                                                                   '作業部署(正)

                O_ROW("NACSTAFFCODE2") = ""                                                             '従業員コード(副)
                O_ROW("NACSTAFFKBN2") = ""                                                              '社員区分(副)
                O_ROW("NACMORG2") = ""                                                                  '管理部署(副)
                O_ROW("NACHORG2") = ""                                                                  '配属部署(副)
                O_ROW("NACSORG2") = ""                                                                  '作業部署(副)

                O_ROW("NACORDERNO") = ""                                                                '受注番号
                O_ROW("NACDETAILNO") = ""                                                               '明細№
                O_ROW("NACTRIPNO") = ""                                                                 'トリップ
                O_ROW("NACTRIPNO") = ""                                                                 'ドロップ
                O_ROW("NACSEQ") = ""                                                                    'SEQ

                O_ROW("NACORDERORG") = ""                                                               '受注部署
                O_ROW("NACSHIPORG") = ""                                                                '配送部署
                O_ROW("NACSURYO") = 0                                                                   '受注・数量
                O_ROW("NACTANI") = ""                                                                   '受注・単位
                O_ROW("NACJSURYO") = 0                                                                  '実績・配送数量
                O_ROW("NACSTANI") = ""                                                                  '実績・配送単位
                O_ROW("NACHAIDISTANCE") = 0                                                             '実績・配送距離
                O_ROW("NACKAIDISTANCE") = 0                                                             '実績・回送作業距離
                O_ROW("NACCHODISTANCE") = 0                                                             '実績・勤怠調整距離
                O_ROW("NACTTLDISTANCE") = 0                                                             '実績・配送距離合計Σ
                O_ROW("NACHAISTDATE") = C_DEFAULT_YMD                                                   '実績・配送作業開始日時
                O_ROW("NACHAIENDDATE") = C_DEFAULT_YMD                                                  '実績・配送作業終了日時
                O_ROW("NACHAIWORKTIME") = 0                                                             '実績・配送作業時間(分)
                O_ROW("NACGESSTDATE") = C_DEFAULT_YMD                                                   '実績・下車作業開始日時
                O_ROW("NACGESENDDATE") = C_DEFAULT_YMD                                                  '実績・下車作業終了日時
                O_ROW("NACGESWORKTIME") = 0                                                             '実績・下車作業時間(分)
                O_ROW("NACCHOWORKTIME") = 0                                                             '実績・勤怠調整時間(分)
                O_ROW("NACTTLWORKTIME") = 0                                                             '実績・配送合計時間Σ(分)
                O_ROW("NACOUTWORKTIME") = 0                                                             '実績・就業外時間
                O_ROW("NACBREAKSTDATE") = C_DEFAULT_YMD                                                 '実績・休憩開始日時
                O_ROW("NACBREAKENDDATE") = C_DEFAULT_YMD                                                '実績・休憩終了日時
                O_ROW("NACBREAKTIME") = 0                                                               '実績・休憩時間(分)
                O_ROW("NACCHOBREAKTIME") = 0                                                            '実績・休憩調整時間(分)
                O_ROW("NACTTLBREAKTIME") = 0                                                            '実績・休憩合計時間Σ(分)
                O_ROW("NACCASH") = 0                                                                    '実績・現金
                O_ROW("NACETC") = 0                                                                     '実績・ETC
                O_ROW("NACTICKET") = 0                                                                  '実績・回数券
                O_ROW("NACKYUYU") = 0                                                                   '実績・軽油
                O_ROW("NACUNLOADCNT") = 0                                                               '実績・荷卸回数
                O_ROW("NACCHOUNLOADCNT") = 0                                                            '実績・荷卸回数調整
                O_ROW("NACTTLUNLOADCNT") = 0                                                            '実績・荷卸回数合計Σ
                O_ROW("NACKAIJI") = 0                                                                   '実績・回次
                O_ROW("NACJITIME") = 0                                                                  '実績・実車時間(分)
                O_ROW("NACJICHOSTIME") = 0                                                              '実績・実車時間調整(分)
                O_ROW("NACJITTLETIME") = 0                                                              '実績・実車時間合計Σ(分)
                O_ROW("NACKUTIME") = 0                                                                  '実績・空車時間(分)
                O_ROW("NACKUCHOTIME") = 0                                                               '実績・空車時間調整(分)
                O_ROW("NACKUTTLTIME") = 0                                                               '実績・空車時間合計Σ(分)
                O_ROW("NACJIDISTANCE") = 0                                                              '実績・実車距離
                O_ROW("NACJICHODISTANCE") = 0                                                           '実績・実車距離調整
                O_ROW("NACJITTLDISTANCE") = 0                                                           '実績・実車距離合計Σ
                O_ROW("NACKUDISTANCE") = 0                                                              '実績・空車距離
                O_ROW("NACKUCHODISTANCE") = 0                                                           '実績・空車距離調整
                O_ROW("NACKUTTLDISTANCE") = 0                                                           '実績・空車距離合計Σ
                O_ROW("NACTARIFFFARE") = 0                                                              '実績・運賃タリフ額
                O_ROW("NACFIXEDFARE") = 0                                                               '実績・運賃固定額
                O_ROW("NACINCHOFARE") = 0                                                               '実績・運賃手入力調整額
                O_ROW("NACTTLFARE") = 0                                                                 '実績・運賃合計額Σ
                O_ROW("NACOFFICESORG") = T00009row("SORG")                                              '実績・作業部署
                O_ROW("NACOFFICETIME") = T0007COM.HHMMtoMinutes(T00009row("WORKTIME"))                  '実績・事務時間
                O_ROW("NACOFFICEBREAKTIME") = T0007COM.HHMMtoMinutes(T00009row("BREAKTIMETTL"))         '実績・事務休憩時間
                O_ROW("PAYSHUSHADATE") = T00009row("STDATE") & " " & T00009row("STTIME")                '出社日時
                O_ROW("PAYTAISHADATE") = T00009row("ENDDATE") & " " & T00009row("ENDTIME")              '退社日時
                O_ROW("PAYSTAFFCODE") = T00009row("STAFFCODE")                                          '従業員コード
                O_ROW("PAYSTAFFKBN") = T00009row("STAFFKBN")                                            '社員区分
                O_ROW("PAYMORG") = T00009row("MORG")                                                    '従業員管理部署
                O_ROW("PAYHORG") = T00009row("HORG")                                                    '従業員配属部署
                O_ROW("PAYHOLIDAYKBN") = T00009row("HOLIDAYKBN")                                        '休日区分
                O_ROW("PAYKBN") = T00009row("PAYKBN")                                                   '勤怠区分
                O_ROW("PAYSHUKCHOKKBN") = T00009row("SHUKCHOKKBN")                                      '宿日直区分
                O_ROW("PAYJYOMUKBN") = "3"                                                              '乗務区分
                O_ROW("PAYOILKBN") = ""                                                                 '勤怠用油種区分
                O_ROW("PAYSHARYOKBN") = ""                                                              '勤怠用車両区分

                '所労
                If T00009row("HOLIDAYKBN") = "0" Then
                    O_ROW("PAYWORKNISSU") = 1
                Else
                    O_ROW("PAYWORKNISSU") = 0
                End If

                O_ROW("PAYSHOUKETUNISSU") = T00009row("SHOUKETUNISSUTTL")                               '傷欠
                O_ROW("PAYKUMIKETUNISSU") = T00009row("KUMIKETUNISSUTTL")                               '組欠
                O_ROW("PAYETCKETUNISSU") = T00009row("ETCKETUNISSUTTL")                                 '他欠
                O_ROW("PAYNENKYUNISSU") = T00009row("NENKYUNISSUTTL")                                   '年休
                O_ROW("PAYTOKUKYUNISSU") = T00009row("TOKUKYUNISSUTTL")                                 '特休
                O_ROW("PAYCHIKOKSOTAINISSU") = T00009row("CHIKOKSOTAINISSUTTL")                         '遅早
                O_ROW("PAYSTOCKNISSU") = T00009row("STOCKNISSUTTL")                                     'ストック休暇
                O_ROW("PAYKYOTEIWEEKNISSU") = T00009row("KYOTEIWEEKNISSUTTL")                           '協定週休
                O_ROW("PAYWEEKNISSU") = T00009row("WEEKNISSUTTL")                                       '週休
                O_ROW("PAYDAIKYUNISSU") = T00009row("DAIKYUNISSUTTL")                                   '代休
                O_ROW("PAYWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("BINDTIME"))                    '所定労働時間(分)
                O_ROW("PAYWWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("WWORKTIMETTL"))               '所定内時間（分）
                O_ROW("PAYNIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("NIGHTTIMETTL"))               '所定深夜時間(分)
                O_ROW("PAYORVERTIME") = T0007COM.HHMMtoMinutes(T00009row("ORVERTIMETTL"))               '平日残業時間(分)
                O_ROW("PAYWNIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("WNIGHTTIMETTL"))             '平日深夜時間(分)
                O_ROW("PAYWSWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("SWORKTIMETTL"))              '日曜出勤時間(分)
                O_ROW("PAYSNIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("SNIGHTTIMETTL"))             '日曜深夜時間(分)
                O_ROW("PAYSDAIWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("SDAIWORKTIMETTL"))         '日曜出勤時間（分）
                O_ROW("PAYSDAINIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("SDAINIGHTTIMETTL"))       '日曜深夜時間（分）
                O_ROW("PAYHWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("HWORKTIMETTL"))               '休日出勤時間(分)
                O_ROW("PAYHNIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("HNIGHTTIMETTL"))             '休日深夜時間(分)
                O_ROW("PAYHDAIWORKTIME") = T0007COM.HHMMtoMinutes(T00009row("HDAIWORKTIMETTL"))         '休日代休出勤時間（分）
                O_ROW("PAYHDAINIGHTTIME") = T0007COM.HHMMtoMinutes(T00009row("HDAINIGHTTIMETTL"))       '休日代休深夜時間（分）
                O_ROW("PAYBREAKTIME") = T0007COM.HHMMtoMinutes(T00009row("BREAKTIMETTL"))               '休憩時間(分)

                O_ROW("PAYNENSHINISSU") = T00009row("NENSHINISSUTTL")                                   '年始出勤
                O_ROW("PAYNENMATUNISSU") = T00009row("NENMATUNISSUTTL")                                 '年末出勤
                O_ROW("PAYSHUKCHOKNNISSU") = T00009row("SHUKCHOKNNISSUTTL")                             '宿日直年始
                O_ROW("PAYSHUKCHOKNISSU") = T00009row("SHUKCHOKNISSUTTL")                               '宿日直通常
                O_ROW("PAYSHUKCHOKNHLDNISSU") = T00009row("SHUKCHOKNHLDNISSUTTL")                       '宿日直年始(翌休み)
                O_ROW("PAYSHUKCHOKHLDNISSU") = T00009row("SHUKCHOKHLDNISSUTTL")                         '宿日直通常(翌休み)
                O_ROW("PAYTOKSAAKAISU") = T00009row("TOKSAAKAISUTTL")                                   '特作A
                O_ROW("PAYTOKSABKAISU") = T00009row("TOKSABKAISUTTL")                                   '特作B
                O_ROW("PAYTOKSACKAISU") = T00009row("TOKSACKAISUTTL")                                   '特作C
                O_ROW("PAYTENKOKAISU") = T00009row("TENKOKAISUTTL")                                     '点呼回数
                O_ROW("PAYHOANTIME") = T0007COM.HHMMtoMinutes(T00009row("HOANTIMETTL"))                 '保安検査入力(分)
                O_ROW("PAYKOATUTIME") = T0007COM.HHMMtoMinutes(T00009row("KOATUTIMETTL"))               '高圧作業入力(分)
                O_ROW("PAYTOKUSA1TIME") = T0007COM.HHMMtoMinutes(T00009row("TOKUSA1TIMETTL"))           '特作Ⅰ(分)
                O_ROW("PAYPONPNISSU") = 0                                                               'ポンプ
                O_ROW("PAYBULKNISSU") = 0                                                               'バルク
                O_ROW("PAYTRAILERNISSU") = 0                                                            'トレーラ
                O_ROW("PAYBKINMUKAISU") = 0                                                             'B勤務
                O_ROW("PAYYENDTIME") = T00009row("YENDTIME")                                            '予定退社時刻
                O_ROW("PAYAPPLYID") = T00009row("APPLYID")                                              '申請ID
                O_ROW("PAYRIYU") = T00009row("RIYU")                                                    '理由
                O_ROW("PAYRIYUETC") = T00009row("RIYUETC")                                              '理由その他
                O_ROW("PAYHAYADETIME") = T0007COM.HHMMtoMinutes(T00009row("HAYADETIMETTL"))             '早出補填時間
                O_ROW("PAYHAISOTIME") = 0                                                               '配送時間
                O_ROW("PAYSHACHUHAKNISSU") = 0                                                          '車中泊日数
                O_ROW("PAYMODELDISTANCE") = 0                                                           'モデル距離
                O_ROW("PAYJIKYUSHATIME") = T0007COM.HHMMtoMinutes(T00009row("JIKYUSHATIMETTL"))         '時給者時間
                O_ROW("PAYJYOMUTIME") = 0                                                               '乗務時間
                O_ROW("PAYHWORKNISSU") = T00009row("HWORKNISSUTTL")                                     '休日出勤日数
                O_ROW("PAYKAITENCNT") = 0                                                               '回転数
                O_ROW("PAYSENJYOCNT") = 0                                                               '洗浄回数
                O_ROW("PAYUNLOADADDCNT1") = 0                                                           '危険物荷卸回数1
                O_ROW("PAYUNLOADADDCNT2") = 0                                                           '危険物荷卸回数2
                O_ROW("PAYUNLOADADDCNT3") = 0                                                           '危険物荷卸回数3
                O_ROW("PAYUNLOADADDCNT4") = 0                                                           '危険物荷卸回数4
                O_ROW("PAYSHORTDISTANCE1") = 0                                                          '短距離手当1
                O_ROW("PAYSHORTDISTANCE2") = 0                                                          '短距離手当2
                O_ROW("APPKIJUN") = ""                                                                  '配賦基準
                O_ROW("APPKEY") = ""                                                                    '配賦統計キー

                O_ROW("WORKKBN") = T00009row("WORKKBN")                                                 '作業区分
                O_ROW("KEYSTAFFCODE") = T00009row("STAFFCODE")                                          '従業員コードキー
                O_ROW("KEYGSHABAN") = ""                                                                '業務車番キー
                O_ROW("KEYTRIPNO") = ""                                                                 'トリップキー
                O_ROW("KEYDROPNO") = ""                                                                 'ドロップキー

                O_ROW("DELFLG") = C_DELETE_FLG.ALIVE                                                    '削除フラグ
                O_ROW("INITYMD") = I_NOW                                                                '登録年月日
                O_ROW("UPDYMD") = I_NOW                                                                 '更新年月日
                O_ROW("UPDUSER") = Master.USERID                                                        '更新ユーザID
                O_ROW("UPDTERMID") = Master.USERTERMID                                                  '更新端末
                O_ROW("RECEIVEYMD") = C_DEFAULT_YMD                                                     '集信日時

                '勘定科目判定テーブル検索(共通設定項目)
                CS0038ACCODEget.TBL = WW_ACHANTEItbl                                                    '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = O_ROW("CAMPCODE")                                            '会社コード
                CS0038ACCODEget.STYMD = O_ROW("KEIJOYMD")                                               '開始日
                CS0038ACCODEget.ENDYMD = O_ROW("KEIJOYMD")                                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                                          '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                                         '伝票タイプ

                CS0038ACCODEget.TORICODE = O_ROW("NACTORICODE")                                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = O_ROW("NACTORITYPE01")                                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = O_ROW("NACTORITYPE02")                                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = O_ROW("NACTORITYPE03")                                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = O_ROW("NACTORITYPE04")                                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = O_ROW("NACTORITYPE05")                                     '取引タイプ05
                CS0038ACCODEget.URIKBN = O_ROW("NACURIKBN")                                             '売上計上基準
                CS0038ACCODEget.STORICODE = O_ROW("NACSTORICODE")                                       '販売店コード
                CS0038ACCODEget.OILTYPE = O_ROW("NACOILTYPE")                                           '油種
                CS0038ACCODEget.PRODUCT1 = O_ROW("NACPRODUCT1")                                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = O_ROW("NACSUPPLIERKBN")                                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = O_ROW("NACMANGSORG1")                                        '車両設置部署
                CS0038ACCODEget.MANGUORG = O_ROW("NACMANGUORG1")                                        '車両運用部署
                CS0038ACCODEget.BASELEASE = O_ROW("NACBASELEASE1")                                      '車両所有
                CS0038ACCODEget.STAFFKBN = O_ROW("NACSTAFFKBN")                                         '社員区分
                CS0038ACCODEget.HORG = O_ROW("NACHORG")                                                 '配属部署
                CS0038ACCODEget.SORG = O_ROW("NACSORG")                                                 '作業部署

                '勘定科目判定テーブル検索(借方)
                CS0038ACCODEget.ACHANTEI = "JMD"                                                        '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索(貸方)
                CS0038ACCODEget.ACHANTEI = "JMC"                                                        '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                '削除データ
                'If T00009row("DELFLG") = C_DELETE_FLG.DELETE Then
                '    '借方
                '    O_ROW("ACCODE") = WW_ACCODE_C                                                       '勘定科目コード
                '    O_ROW("SUBACCODE") = WW_SUBACCODE_C                                                 '補助科目コード
                '    O_ROW("INQKBN") = "1"                                                               '照会区分
                '    O_ROW("ACDCKBN") = "D"                                                              '貸借区分
                '    O_ROW("ACACHANTEI") = "JMD"                                                         '勘定科目判定コード
                '    O_ROW("DTLNO") = "01"                                                               '明細番号
                '    O_ROW("ACKEIJOORG") = T00009row("HORG")                                             '計上部署コード（作業部署）

                '    Dim WW_ROW As DataRow = O_TABLE.NewRow
                '    WW_ROW.ItemArray = O_ROW.ItemArray
                '    O_TABLE.Rows.Add(WW_ROW)

                '    '貸方
                '    O_ROW("ACCODE") = WW_ACCODE_D                                                       '勘定科目コード
                '    O_ROW("SUBACCODE") = WW_SUBACCODE_D                                                 '補助科目コード
                '    O_ROW("INQKBN") = "0"                                                               '照会区分
                '    O_ROW("ACDCKBN") = "C"                                                              '貸借区分
                '    O_ROW("ACACHANTEI") = "JMC"                                                         '勘定科目判定コード
                '    O_ROW("DTLNO") = "02"                                                               '明細番号
                '    O_ROW("ACKEIJOORG") = T00009row("HORG")                                             '計上部署コード（配属部署）

                '    WW_ROW = O_TABLE.NewRow
                '    WW_ROW.ItemArray = O_ROW.ItemArray
                '    O_TABLE.Rows.Add(WW_ROW)
                'End If

                '追加データ
                If T00009row("DELFLG") = C_DELETE_FLG.ALIVE Then
                    '借方
                    If WW_INQKBN_D = "1" Then
                        O_ROW("ACCODE") = WW_ACCODE_D                                                   '勘定科目コード
                        O_ROW("SUBACCODE") = WW_SUBACCODE_D                                             '補助科目コード
                        O_ROW("INQKBN") = WW_INQKBN_D                                                   '照会区分
                        O_ROW("ACDCKBN") = "D"                                                          '貸借区分
                        O_ROW("ACACHANTEI") = "JMD"                                                     '勘定科目判定コード
                        O_ROW("DTLNO") = "01"                                                           '明細番号
                        O_ROW("ACKEIJOORG") = T00009row("HORG")                                         '計上部署コード（作業部署）

                        Dim WW_ROW As DataRow = O_TABLE.NewRow
                        WW_ROW.ItemArray = O_ROW.ItemArray
                        O_TABLE.Rows.Add(WW_ROW)
                    End If

                    '貸方
                    If WW_INQKBN_C = "1" Then
                        O_ROW("ACCODE") = WW_ACCODE_C                                                   '勘定科目コード
                        O_ROW("SUBACCODE") = WW_SUBACCODE_C                                             '補助科目コード
                        O_ROW("INQKBN") = WW_INQKBN_C                                                   '照会区分
                        O_ROW("ACDCKBN") = "C"                                                          '貸借区分
                        O_ROW("ACACHANTEI") = "JMC"                                                     '勘定科目判定コード
                        O_ROW("DTLNO") = "02"                                                           '明細番号
                        O_ROW("ACKEIJOORG") = T00009row("HORG")                                         '計上部署コード（配属部署）

                        Dim WW_ROW As DataRow = O_TABLE.NewRow
                        WW_ROW.ItemArray = O_ROW.ItemArray
                        O_TABLE.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                Dim WW_CSV As String = DataRowToCSV(T00009row)

                CS0011LOGWrite.INFSUBCLASS = "tblDailyTtlEdit"
                CS0011LOGWrite.INFPOSI = "tblDailyTtlEdit"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA = (" & WW_CSV & ")"
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
                Throw
            End Try
        Next

    End Sub

    ''' <summary>
    ''' 月合計、ジャーナル用テーブル編集
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <param name="O_TABLE"></param>
    ''' <param name="I_NOW"></param>
    ''' <remarks></remarks>
    Protected Sub TOKEItblMonthlyTotalEdit(ByVal I_TABLE As DataTable, ByRef O_TABLE As DataTable, ByVal I_NOW As DateTime)

        Dim WW_T00009tbl As DataTable = New DataTable
        Dim WW_ACHANTEItbl As DataTable = New DataTable
        Dim WW_HEADrow As DataRow = Nothing
        Dim WW_DTLrow As DataRow = Nothing

        CS0026TBLSORT.TABLE = I_TABLE
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        CS0026TBLSORT.FILTER = "OPERATION = '" & C_LIST_OPERATION_CODE.UPDATING & "'" _
            & " and RECODEKBN = '2'"
        CS0026TBLSORT.sort(WW_T00009tbl)

        For i As Integer = 0 To WW_T00009tbl.Rows.Count - 1
            Try
                'ヘッダーレコードをキープ
                If WW_T00009tbl.Rows(i)("HDKBN") = "H" Then
                    WW_HEADrow = WW_T00009tbl.Rows(i)
                    Continue For
                End If

                '明細レコードをキープ
                If WW_T00009tbl.Rows(i)("HDKBN") = "D" Then
                    WW_DTLrow = WW_T00009tbl.Rows(i)

                    If IsNothing(WW_HEADrow) Then
                        Continue For
                    End If
                End If

                Dim O_ROW As DataRow = O_TABLE.NewRow

                '伝票番号採番
                Dim WW_SEQ As String = "000000"
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.CAMPCODE = WW_HEADrow("CAMPCODE")
                CS0033AutoNumber.MORG = work.WF_SEL_HORG.Text
                CS0033AutoNumber.USERID = Master.USERID
                CS0033AutoNumber.getAutoNumber()
                If isNormal(CS0033AutoNumber.ERR) Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    Master.output(CS0033AutoNumber.ERR, C_MESSAGE_TYPE.ABORT, "CS0033AutoNumber:DENNO")
                    Exit Sub
                End If

                'テーブル編集
                O_ROW("CAMPCODE") = WW_HEADrow("CAMPCODE")                                              '会社コード
                O_ROW("MOTOCHO") = "LO"                                                                 '元帳(非会計予定を設定)
                O_ROW("VERSION") = "000"                                                                'バージョン
                O_ROW("DENTYPE") = "T07"                                                                '伝票タイプ
                O_ROW("TENKI") = "0"                                                                    '統計転記
                O_ROW("KEIJOYMD") = WW_HEADrow("WORKDATE")                                              '計上日付(勤務年月日を設定)
                O_ROW("DENYMD") = WW_HEADrow("WORKDATE")                                                '伝票日付(勤務年月日設定)

                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(WW_HEADrow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                O_ROW("DENNO") = WW_HEADrow("HORG") & WW_DENNO & WW_SEQ

                '関連伝票№ + 明細№
                O_ROW("KANRENDENNO") = WW_HEADrow("HORG") & " " _
                    & WW_HEADrow("STAFFCODE") & " " _
                    & WW_HEADrow("WORKDATE") & " "

                O_ROW("ACTORICODE") = ""                                                                '取引先コード
                O_ROW("ACOILTYPE") = ""                                                                 '油種
                O_ROW("ACSHARYOTYPE") = ""                                                              '統一車番(上)
                O_ROW("ACTSHABAN") = ""                                                                 '統一車番(下)
                O_ROW("ACSTAFFCODE") = ""                                                               '従業員コード
                O_ROW("ACBANKAC") = ""                                                                  '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050SESSION.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If isNormal(CS0006TERMchk.ERR) Then
                    O_ROW("ACKEIJOMORG") = CS0006TERMchk.MORG                                           '計上管理部署コード(部・支店)
                Else
                    O_ROW("ACKEIJOMORG") = WW_HEADrow("MORG")                                           '計上管理部署コード(管理部署)
                End If

                O_ROW("ACTAXKBN") = 0                                                                   '税区分
                O_ROW("ACAMT") = 0                                                                      '金額
                O_ROW("NACSHUKODATE") = WW_HEADrow("WORKDATE")                                          '勤務日
                O_ROW("NACSHUKADATE") = C_DEFAULT_YMD                                                   '出荷日
                O_ROW("NACTODOKEDATE") = C_DEFAULT_YMD                                                  '届日
                O_ROW("NACTORICODE") = ""                                                               '荷主コード
                O_ROW("NACURIKBN") = ""                                                                 '売上計上基準
                O_ROW("NACTODOKECODE") = ""                                                             '届先コード
                O_ROW("NACSTORICODE") = ""                                                              '販売店コード
                O_ROW("NACSHUKABASHO") = ""                                                             '出荷場所

                O_ROW("NACTORITYPE01") = ""                                                             '取引先・取引タイプ01
                O_ROW("NACTORITYPE02") = ""                                                             '取引先・取引タイプ02
                O_ROW("NACTORITYPE03") = ""                                                             '取引先・取引タイプ03
                O_ROW("NACTORITYPE04") = ""                                                             '取引先・取引タイプ04
                O_ROW("NACTORITYPE05") = ""                                                             '取引先・取引タイプ05

                O_ROW("NACOILTYPE") = ""                                                                '油種
                O_ROW("NACPRODUCT1") = ""                                                               '品名1
                O_ROW("NACPRODUCT2") = ""                                                               '品名2
                O_ROW("NACGSHABAN") = ""                                                                '業務車番
                O_ROW("NACSUPPLIERKBN") = ""                                                            '社有・庸車区分
                O_ROW("NACSUPPLIER") = ""                                                               '庸車会社
                O_ROW("NACSHARYOOILTYPE") = ""                                                          '車両登録油種

                O_ROW("NACSHARYOTYPE1") = ""                                                            '統一車番(上)1
                O_ROW("NACTSHABAN1") = ""                                                               '統一車番(下)1
                O_ROW("NACMANGMORG1") = ""                                                              '車両管理部署1
                O_ROW("NACMANGSORG1") = ""                                                              '車両設置部署1
                O_ROW("NACMANGUORG1") = ""                                                              '車両運用部署1
                O_ROW("NACBASELEASE1") = ""                                                             '車両所有1

                O_ROW("NACSHARYOTYPE2") = ""                                                            '統一車番(上)2
                O_ROW("NACTSHABAN2") = ""                                                               '統一車番(下)2
                O_ROW("NACMANGMORG2") = ""                                                              '車両管理部署2
                O_ROW("NACMANGSORG2") = ""                                                              '車両設置部署2
                O_ROW("NACMANGUORG2") = ""                                                              '車両運用部署2
                O_ROW("NACBASELEASE2") = ""                                                             '車両所有2

                O_ROW("NACSHARYOTYPE3") = ""                                                            '統一車番(上)3
                O_ROW("NACTSHABAN3") = ""                                                               '統一車番(下)3
                O_ROW("NACMANGMORG3") = ""                                                              '車両管理部署3
                O_ROW("NACMANGSORG3") = ""                                                              '車両設置部署3
                O_ROW("NACMANGUORG3") = ""                                                              '車両運用部署3
                O_ROW("NACBASELEASE3") = ""                                                             '車両所有3

                O_ROW("NACCREWKBN") = ""                                                                '正副区分
                O_ROW("NACSTAFFCODE") = ""                                                              '従業員コード(正)
                O_ROW("NACSTAFFKBN") = ""                                                               '社員区分(正)
                O_ROW("NACMORG") = ""                                                                   '管理部署(正)
                O_ROW("NACHORG") = ""                                                                   '配属部署(正)
                O_ROW("NACSORG") = ""                                                                   '作業部署(正)

                O_ROW("NACSTAFFCODE2") = ""                                                             '従業員コード(副)
                O_ROW("NACSTAFFKBN2") = ""                                                              '社員区分(副)
                O_ROW("NACMORG2") = ""                                                                  '管理部署(副)
                O_ROW("NACHORG2") = ""                                                                  '配属部署(副)
                O_ROW("NACSORG2") = ""                                                                  '作業部署(副)

                O_ROW("NACORDERNO") = ""                                                                '受注番号
                O_ROW("NACDETAILNO") = ""                                                               '明細№
                O_ROW("NACTRIPNO") = ""                                                                 'トリップ
                O_ROW("NACTRIPNO") = ""                                                                 'ドロップ
                O_ROW("NACSEQ") = ""                                                                    'SEQ

                O_ROW("NACORDERORG") = ""                                                               '受注部署
                O_ROW("NACSHIPORG") = ""                                                                '配送部署
                O_ROW("NACSURYO") = 0                                                                   '受注・数量
                O_ROW("NACTANI") = ""                                                                   '受注・単位
                O_ROW("NACJSURYO") = 0                                                                  '実績・配送数量
                O_ROW("NACSTANI") = ""                                                                  '実績・配送単位
                O_ROW("NACHAIDISTANCE") = 0                                                             '実績・配送距離
                O_ROW("NACKAIDISTANCE") = 0                                                             '実績・回送作業距離
                O_ROW("NACCHODISTANCE") = WW_DTLrow("HAIDISTANCECHO")                                   '実績・勤怠調整距離
                O_ROW("NACTTLDISTANCE") = WW_DTLrow("HAIDISTANCECHO")                                   '実績・配送距離合計Σ
                O_ROW("NACHAISTDATE") = C_DEFAULT_YMD                                                   '実績・配送作業開始日時
                O_ROW("NACHAIENDDATE") = C_DEFAULT_YMD                                                  '実績・配送作業終了日時
                O_ROW("NACHAIWORKTIME") = 0                                                             '実績・配送作業時間(分)
                O_ROW("NACGESSTDATE") = C_DEFAULT_YMD                                                   '実績・下車作業開始日時
                O_ROW("NACGESENDDATE") = C_DEFAULT_YMD                                                  '実績・下車作業終了日時
                O_ROW("NACGESWORKTIME") = 0                                                             '実績・下車作業時間(分)

                Dim WW_NIGHTTIME As Integer = 0                                                         '所定深夜時間(分)
                Dim WW_ORVERTIME As Integer = 0                                                         '平日残業時間(分)
                Dim WW_WNIGHTTIME As Integer = 0                                                        '平日深夜時間(分)
                Dim WW_WSWORKTIME As Integer = 0                                                        '日曜出勤時間(分)
                Dim WW_SNIGHTTIME As Integer = 0                                                        '日曜深夜時間(分)
                Dim WW_HWORKTIME As Integer = 0                                                         '休日出勤時間(分)
                Dim WW_HNIGHTTIME As Integer = 0                                                        '休日深夜時間(分)

                WW_NIGHTTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO"))                       '所定深夜時間(分)
                WW_ORVERTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO"))                       '平日残業時間(分)
                WW_WNIGHTTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO"))                     '平日深夜時間(分)
                WW_WSWORKTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO"))                      '日曜出勤時間(分)
                WW_SNIGHTTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO"))                     '日曜深夜時間(分)
                WW_HWORKTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO"))                       '休日出勤時間(分)
                WW_HNIGHTTIME = T0007COM.HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO"))                     '休日深夜時間(分)

                '実績・勤怠調整時間(分)
                If IsDBNull(WW_DTLrow("SHARYOKBN")) Then
                    WW_DTLrow("SHARYOKBN") = ""
                End If
                If IsDBNull(WW_DTLrow("OILPAYKBN")) Then
                    WW_DTLrow("OILPAYKBN") = ""
                End If
                If WW_DTLrow("SHARYOKBN") = "1" AndAlso WW_DTLrow("OILPAYKBN") = "01" Then
                    O_ROW("NACCHOWORKTIME") = WW_NIGHTTIME + WW_ORVERTIME + WW_WNIGHTTIME +
                        WW_WSWORKTIME + WW_SNIGHTTIME + WW_HWORKTIME + WW_HNIGHTTIME                    '実績・勤怠調整時間(分)
                    O_ROW("NACTTLWORKTIME") = O_ROW("NACCHOWORKTIME")                                   '実績・配送合計時間Σ(分)
                Else
                    O_ROW("NACCHOWORKTIME") = 0                                                         '実績・勤怠調整時間(分)
                    O_ROW("NACTTLWORKTIME") = 0                                                         '実績・配送合計時間Σ(分)
                End If

                O_ROW("NACOUTWORKTIME") = 0                                                             '実績・就業外時間
                O_ROW("NACBREAKSTDATE") = C_DEFAULT_YMD                                                 '実績・休憩開始日時
                O_ROW("NACBREAKENDDATE") = C_DEFAULT_YMD                                                '実績・休憩終了日時
                O_ROW("NACBREAKTIME") = 0                                                               '実績・休憩時間(分)
                O_ROW("NACCHOBREAKTIME") = 0                                                            '実績・休憩調整時間(分)
                O_ROW("NACTTLBREAKTIME") = 0                                                            '実績・休憩合計時間Σ(分)
                O_ROW("NACCASH") = 0                                                                    '実績・現金
                O_ROW("NACETC") = 0                                                                     '実績・ETC
                O_ROW("NACTICKET") = 0                                                                  '実績・回数券
                O_ROW("NACKYUYU") = 0                                                                   '実績・軽油
                O_ROW("NACUNLOADCNT") = 0                                                               '実績・荷卸回数
                O_ROW("NACCHOUNLOADCNT") = WW_DTLrow("UNLOADCNTCHO")                                    '実績・荷卸回数調整
                O_ROW("NACTTLUNLOADCNT") = WW_DTLrow("UNLOADCNTCHO")                                    '実績・荷卸回数合計Σ
                O_ROW("NACKAIJI") = 0                                                                   '実績・回次
                O_ROW("NACJITIME") = 0                                                                  '実績・実車時間(分)
                O_ROW("NACJICHOSTIME") = 0                                                              '実績・実車時間調整(分)
                O_ROW("NACJITTLETIME") = 0                                                              '実績・実車時間合計Σ(分)
                O_ROW("NACKUTIME") = 0                                                                  '実績・空車時間(分)
                O_ROW("NACKUCHOTIME") = 0                                                               '実績・空車時間調整(分)
                O_ROW("NACKUTTLTIME") = 0                                                               '実績・空車時間合計Σ(分)
                O_ROW("NACJIDISTANCE") = 0                                                              '実績・実車距離
                O_ROW("NACJICHODISTANCE") = 0                                                           '実績・実車距離調整
                O_ROW("NACJITTLDISTANCE") = 0                                                           '実績・実車距離合計Σ
                O_ROW("NACKUDISTANCE") = 0                                                              '実績・空車距離
                O_ROW("NACKUCHODISTANCE") = 0                                                           '実績・空車距離調整
                O_ROW("NACKUTTLDISTANCE") = 0                                                           '実績・空車距離合計Σ
                O_ROW("NACTARIFFFARE") = 0                                                              '実績・運賃タリフ額
                O_ROW("NACFIXEDFARE") = 0                                                               '実績・運賃固定額
                O_ROW("NACINCHOFARE") = 0                                                               '実績・運賃手入力調整額
                O_ROW("NACTTLFARE") = 0                                                                 '実績・運賃合計額Σ
                O_ROW("NACOFFICESORG") = WW_HEADrow("SORG")                                             '実績・作業部署

                '実績・事務時間
                If WW_DTLrow("SHARYOKBN") = "1" AndAlso WW_DTLrow("OILPAYKBN") = "01" Then
                    If WW_HEADrow("STAFFKBN") Like "03*" Then
                        O_ROW("NACOFFICETIME") = 0
                    Else
                        O_ROW("NACOFFICETIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("WORKTIME"))
                    End If
                Else
                    O_ROW("NACOFFICETIME") = 0
                End If

                O_ROW("NACOFFICEBREAKTIME") = 0                                                         '実績・事務休憩時間
                O_ROW("PAYSHUSHADATE") = C_DEFAULT_YMD                                                  '出社日時
                O_ROW("PAYTAISHADATE") = C_DEFAULT_YMD                                                  '退社日時
                O_ROW("PAYSTAFFCODE") = WW_HEADrow("STAFFCODE")                                         '従業員コード
                O_ROW("PAYSTAFFKBN") = WW_HEADrow("STAFFKBN")                                           '社員区分
                O_ROW("PAYMORG") = WW_HEADrow("MORG")                                                   '従業員管理部署
                O_ROW("PAYHORG") = WW_HEADrow("HORG")                                                   '従業員配属部署
                O_ROW("PAYHOLIDAYKBN") = WW_HEADrow("HOLIDAYKBN")                                       '休日区分
                O_ROW("PAYKBN") = WW_HEADrow("PAYKBN")                                                  '勤怠区分
                O_ROW("PAYSHUKCHOKKBN") = WW_HEADrow("SHUKCHOKKBN")                                     '宿日直区分
                O_ROW("PAYJYOMUKBN") = "3"                                                              '乗務区分

                O_ROW("PAYOILKBN") = WW_DTLrow("OILPAYKBN")                                             '勤怠用油種区分
                O_ROW("PAYSHARYOKBN") = WW_DTLrow("SHARYOKBN")                                          '勤怠用車両区分

                If WW_DTLrow("SHARYOKBN") = "1" AndAlso WW_DTLrow("OILPAYKBN") = "01" Then
                    O_ROW("PAYWORKNISSU") = WW_HEADrow("WORKNISSUCHO")                                  '所労
                    O_ROW("PAYSHOUKETUNISSU") = WW_HEADrow("SHOUKETUNISSUCHO")                          '傷欠
                    O_ROW("PAYKUMIKETUNISSU") = WW_HEADrow("KUMIKETUNISSUCHO")                          '組欠
                    O_ROW("PAYETCKETUNISSU") = WW_HEADrow("ETCKETUNISSUCHO")                            '他欠
                    O_ROW("PAYNENKYUNISSU") = WW_HEADrow("NENKYUNISSUCHO")                              '年休
                    O_ROW("PAYTOKUKYUNISSU") = WW_HEADrow("TOKUKYUNISSUCHO")                            '特休
                    O_ROW("PAYCHIKOKSOTAINISSU") = WW_HEADrow("CHIKOKSOTAINISSUCHO")                    '遅早
                    O_ROW("PAYSTOCKNISSU") = WW_HEADrow("STOCKNISSUCHO")                                'ストック休暇
                    O_ROW("PAYKYOTEIWEEKNISSU") = WW_HEADrow("KYOTEIWEEKNISSUCHO")                      '協定週休
                    O_ROW("PAYWEEKNISSU") = WW_HEADrow("WEEKNISSUCHO")                                  '週休
                    O_ROW("PAYDAIKYUNISSU") = WW_HEADrow("DAIKYUNISSUCHO")                              '代休

                    O_ROW("PAYWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("BINDTIME"))               '所定労働時間(分)
                    O_ROW("PAYWWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("WWORKTIMETTL"))          '所定内時間（分）
                    O_ROW("PAYNIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO"))          '所定深夜時間(分)
                    O_ROW("PAYORVERTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO"))          '平日残業時間(分)
                    O_ROW("PAYWNIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO"))        '平日深夜時間(分)
                    O_ROW("PAYWSWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO"))         '日曜出勤時間(分)
                    O_ROW("PAYSNIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO"))        '日曜深夜時間(分)
                    O_ROW("PAYSDAIWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("SDAIWORKTIMETTL"))    '日曜出勤時間（分）
                    O_ROW("PAYSDAINIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("SDAINIGHTTIMETTL"))  '日曜深夜時間（分）
                    O_ROW("PAYHWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO"))          '休日出勤時間(分)
                    O_ROW("PAYHNIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO"))        '休日深夜時間(分)
                    O_ROW("PAYHDAIWORKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HDAIWORKTIMETTL"))    '休日代休出勤時間（分）
                    O_ROW("PAYHDAINIGHTTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HDAINIGHTTIMETTL"))  '休日代休深夜時間（分）
                    O_ROW("PAYBREAKTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("BREAKTIMECHO"))          '休憩時間(分)

                    O_ROW("PAYNENSHINISSU") = WW_HEADrow("NENSHINISSUCHO")                              '年始出勤
                    O_ROW("PAYNENMATUNISSU") = WW_HEADrow("NENMATUNISSUTTL")                            '年末出勤
                    O_ROW("PAYSHUKCHOKNNISSU") = WW_HEADrow("SHUKCHOKNNISSUCHO")                        '宿日直年始
                    O_ROW("PAYSHUKCHOKNISSU") = WW_HEADrow("SHUKCHOKNISSUCHO")                          '宿日直通常
                    O_ROW("PAYSHUKCHOKNHLDNISSU") = WW_HEADrow("SHUKCHOKNHLDNISSUCHO")                  '宿日直年始(翌休み)
                    O_ROW("PAYSHUKCHOKHLDNISSU") = WW_HEADrow("SHUKCHOKHLDNISSUCHO")                    '宿日直通常(翌休み)
                    O_ROW("PAYTOKSAAKAISU") = WW_HEADrow("TOKSAAKAISUCHO")                              '特作A
                    O_ROW("PAYTOKSABKAISU") = WW_HEADrow("TOKSABKAISUCHO")                              '特作B
                    O_ROW("PAYTOKSACKAISU") = WW_HEADrow("TOKSACKAISUCHO")                              '特作C
                    O_ROW("PAYTENKOKAISU") = WW_HEADrow("TENKOKAISUCHO")                                '点呼回数

                    O_ROW("PAYHOANTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HOANTIMECHO"))            '保安検査入力(分)
                    O_ROW("PAYKOATUTIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("KOATUTIMECHO"))          '高圧作業入力(分)
                    O_ROW("PAYTOKUSA1TIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("TOKUSA1TIMECHO"))      '特作I(分)

                    O_ROW("PAYPONPNISSU") = WW_HEADrow("PONPNISSUCHO")                                  'ポンプ
                    O_ROW("PAYBULKNISSU") = WW_HEADrow("BULKNISSUCHO")                                  'バルク
                    O_ROW("PAYTRAILERNISSU") = WW_HEADrow("TRAILERNISSUCHO")                            'トレーラ
                    O_ROW("PAYBKINMUKAISU") = WW_HEADrow("BKINMUKAISUCHO")                              'B勤務

                    O_ROW("PAYHAYADETIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("HAYADETIMETTL"))        '早出補填時間
                    O_ROW("PAYHAISOTIME") = 0                                                           '配送時間
                    O_ROW("PAYSHACHUHAKNISSU") = 0                                                      '車中泊日数
                    O_ROW("PAYMODELDISTANCE") = 0                                                       'モデル距離
                    O_ROW("PAYJIKYUSHATIME") = T0007COM.HHMMtoMinutes(WW_HEADrow("JIKYUSHATIMETTL"))    '時給者時間
                    O_ROW("PAYJYOMUTIME") = 0                                                           '乗務時間
                    O_ROW("PAYHWORKNISSU") = WW_HEADrow("HWORKNISSUTTL")                                '休日出勤日数
                    O_ROW("PAYKAITENCNT") = 0                                                           '回転数
                    O_ROW("PAYSENJYOCNT") = 0                                                           '洗浄回数
                    O_ROW("PAYUNLOADADDCNT1") = 0                                                       '危険物荷卸回数1
                    O_ROW("PAYUNLOADADDCNT2") = 0                                                       '危険物荷卸回数2
                    O_ROW("PAYUNLOADADDCNT3") = 0                                                       '危険物荷卸回数3
                    O_ROW("PAYUNLOADADDCNT4") = 0                                                       '危険物荷卸回数4
                    O_ROW("PAYSHORTDISTANCE1") = 0                                                      '短距離手当1
                    O_ROW("PAYSHORTDISTANCE2") = 0                                                      '短距離手当2
                Else
                    O_ROW("PAYWORKNISSU") = 0                                                           '所労
                    O_ROW("PAYSHOUKETUNISSU") = 0                                                       '傷欠
                    O_ROW("PAYKUMIKETUNISSU") = 0                                                       '組欠
                    O_ROW("PAYETCKETUNISSU") = 0                                                        '他欠
                    O_ROW("PAYNENKYUNISSU") = 0                                                         '年休
                    O_ROW("PAYTOKUKYUNISSU") = 0                                                        '特休
                    O_ROW("PAYCHIKOKSOTAINISSU") = 0                                                    '遅早
                    O_ROW("PAYSTOCKNISSU") = 0                                                          'ストック休暇
                    O_ROW("PAYKYOTEIWEEKNISSU") = 0                                                     '協定週休
                    O_ROW("PAYWEEKNISSU") = 0                                                           '週休
                    O_ROW("PAYDAIKYUNISSU") = 0                                                         '代休

                    O_ROW("PAYWORKTIME") = 0                                                            '所定労働時間(分)
                    O_ROW("PAYWWORKTIME") = 0                                                           '所定内時間（分）
                    O_ROW("PAYNIGHTTIME") = 0                                                           '所定深夜時間(分)
                    O_ROW("PAYORVERTIME") = 0                                                           '平日残業時間(分)
                    O_ROW("PAYWNIGHTTIME") = 0                                                          '平日深夜時間(分)
                    O_ROW("PAYWSWORKTIME") = 0                                                          '日曜出勤時間(分)
                    O_ROW("PAYSNIGHTTIME") = 0                                                          '日曜深夜時間(分)
                    O_ROW("PAYSDAIWORKTIME") = 0                                                        '日曜代休出勤時間（分）
                    O_ROW("PAYSDAINIGHTTIME") = 0                                                       '日曜代休深夜時間（分）
                    O_ROW("PAYHWORKTIME") = 0                                                           '休日出勤時間(分)
                    O_ROW("PAYHNIGHTTIME") = 0                                                          '休日深夜時間(分)
                    O_ROW("PAYHDAIWORKTIME") = 0                                                        '休日代休出勤時間（分）
                    O_ROW("PAYHDAINIGHTTIME") = 0                                                       '休日代休深夜時間（分）
                    O_ROW("PAYBREAKTIME") = 0                                                           '休憩時間(分)

                    O_ROW("PAYNENSHINISSU") = 0                                                         '年始出勤
                    O_ROW("PAYNENMATUNISSU") = 0                                                        '年末出勤
                    O_ROW("PAYSHUKCHOKNNISSU") = 0                                                      '宿日直年始
                    O_ROW("PAYSHUKCHOKNISSU") = 0                                                       '宿日直通常
                    O_ROW("PAYSHUKCHOKNHLDNISSU") = 0                                                   '宿日直年始(翌休み)
                    O_ROW("PAYSHUKCHOKHLDNISSU") = 0                                                    '宿日直通常(翌休み)
                    O_ROW("PAYTOKSAAKAISU") = 0                                                         '特作A
                    O_ROW("PAYTOKSABKAISU") = 0                                                         '特作B
                    O_ROW("PAYTOKSACKAISU") = 0                                                         '特作C
                    O_ROW("PAYTENKOKAISU") = 0                                                          '点呼回数

                    O_ROW("PAYHOANTIME") = 0                                                            '保安検査入力(分)
                    O_ROW("PAYKOATUTIME") = 0                                                           '高圧作業入力(分)
                    O_ROW("PAYTOKUSA1TIME") = 0                                                         '特作I(分)

                    O_ROW("PAYPONPNISSU") = 0                                                           'ポンプ
                    O_ROW("PAYBULKNISSU") = 0                                                           'バルク
                    O_ROW("PAYTRAILERNISSU") = 0                                                        'トレーラ
                    O_ROW("PAYBKINMUKAISU") = 0                                                         'B勤務
                    O_ROW("PAYHAYADETIME") = 0                                                          '早出補填時間
                    O_ROW("PAYHAISOTIME") = 0                                                           '配送時間
                    O_ROW("PAYSHACHUHAKNISSU") = 0                                                      '車中泊日数
                    O_ROW("PAYMODELDISTANCE") = 0                                                       'モデル距離
                    O_ROW("PAYJIKYUSHATIME") = 0                                                        '時給者時間
                    O_ROW("PAYJYOMUTIME") = 0                                                           '乗務時間
                    O_ROW("PAYHWORKNISSU") = 0                                                          '休日出勤日数
                    O_ROW("PAYKAITENCNT") = 0                                                           '回転数
                    O_ROW("PAYSENJYOCNT") = 0                                                           '洗浄回数
                    O_ROW("PAYUNLOADADDCNT1") = 0                                                       '危険物荷卸回数1
                    O_ROW("PAYUNLOADADDCNT2") = 0                                                       '危険物荷卸回数2
                    O_ROW("PAYUNLOADADDCNT3") = 0                                                       '危険物荷卸回数3
                    O_ROW("PAYUNLOADADDCNT4") = 0                                                       '危険物荷卸回数4
                    O_ROW("PAYSHORTDISTANCE1") = 0                                                      '短距離手当1
                    O_ROW("PAYSHORTDISTANCE2") = 0                                                      '短距離手当2
                End If

                O_ROW("PAYYENDTIME") = ""                                                               '予定退社時刻
                O_ROW("PAYAPPLYID") = ""                                                                '申請ID
                O_ROW("PAYRIYU") = ""                                                                   '理由
                O_ROW("PAYRIYUETC") = ""                                                                '理由その他
                O_ROW("APPKIJUN") = ""                                                                  '配賦基準
                O_ROW("APPKEY") = ""                                                                    '配賦統計キー

                O_ROW("WORKKBN") = WW_HEADrow("WORKKBN")                                                '作業区分
                O_ROW("KEYSTAFFCODE") = WW_HEADrow("STAFFCODE")                                         '従業員コードキー
                O_ROW("KEYGSHABAN") = ""                                                                '業務車番キー
                O_ROW("KEYTRIPNO") = ""                                                                 'トリップキー
                O_ROW("KEYDROPNO") = ""                                                                 'ドロップキー
                
                O_ROW("DELFLG") = C_DELETE_FLG.ALIVE                                                    '削除フラグ
                O_ROW("INITYMD") = I_NOW                                                                '登録年月日
                O_ROW("UPDYMD") = I_NOW                                                                 '更新年月日
                O_ROW("UPDUSER") = Master.USERID                                                        '更新ユーザID
                O_ROW("UPDTERMID") = Master.USERTERMID                                                  '更新端末
                O_ROW("RECEIVEYMD") = C_DEFAULT_YMD                                                     '集信日時

                '勘定科目判定テーブル検索(共通設定項目)
                CS0038ACCODEget.TBL = WW_ACHANTEItbl                                                    '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = O_ROW("CAMPCODE")                                            '会社コード
                CS0038ACCODEget.STYMD = O_ROW("KEIJOYMD")                                               '開始日
                CS0038ACCODEget.ENDYMD = O_ROW("KEIJOYMD")                                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                                          '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                                         '伝票タイプ

                CS0038ACCODEget.TORICODE = O_ROW("NACTORICODE")                                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = O_ROW("NACTORITYPE01")                                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = O_ROW("NACTORITYPE02")                                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = O_ROW("NACTORITYPE03")                                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = O_ROW("NACTORITYPE04")                                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = O_ROW("NACTORITYPE05")                                     '取引タイプ05
                CS0038ACCODEget.URIKBN = O_ROW("NACURIKBN")                                             '売上計上基準
                CS0038ACCODEget.STORICODE = O_ROW("NACSTORICODE")                                       '販売店コード
                CS0038ACCODEget.OILTYPE = O_ROW("NACOILTYPE")                                           '油種
                CS0038ACCODEget.PRODUCT1 = O_ROW("NACPRODUCT1")                                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = O_ROW("NACSUPPLIERKBN")                                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = O_ROW("NACMANGSORG1")                                        '車両設置部署
                CS0038ACCODEget.MANGUORG = O_ROW("NACMANGUORG1")                                        '車両運用部署
                CS0038ACCODEget.BASELEASE = O_ROW("NACBASELEASE1")                                      '車両所有
                CS0038ACCODEget.STAFFKBN = O_ROW("NACSTAFFKBN")                                         '社員区分
                CS0038ACCODEget.HORG = O_ROW("NACHORG")                                                 '配属部署
                CS0038ACCODEget.SORG = O_ROW("NACSORG")                                                 '作業部署

                '勘定科目判定テーブル検索(借方)
                CS0038ACCODEget.ACHANTEI = "AMD"                                                        '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索(貸方)
                CS0038ACCODEget.ACHANTEI = "AMC"                                                        '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                '削除データ
                'If WW_HEADrow("DELFLG") = "1" Then
                '    '借方
                '    O_ROW("ACCODE") = WW_ACCODE_C                                                       '勘定科目コード
                '    O_ROW("SUBACCODE") = WW_SUBACCODE_C                                                 '補助科目コード
                '    O_ROW("INQKBN") = "1"                                                               '照会区分
                '    O_ROW("ACDCKBN") = "D"                                                              '貸借区分
                '    O_ROW("ACACHANTEI") = "AMD"                                                         '勘定科目判定コード
                '    O_ROW("DTLNO") = "01"                                                               '明細番号
                '    O_ROW("ACKEIJOORG") = WW_HEADrow("HORG")                                            '計上部署コード(作業部署)

                '    Dim WW_ROW As DataRow = O_TABLE.NewRow
                '    WW_ROW.ItemArray = O_ROW.ItemArray
                '    O_TABLE.Rows.Add(WW_ROW)

                '    '貸方
                '    O_ROW("ACCODE") = WW_ACCODE_D                                                       '勘定科目コード
                '    O_ROW("SUBACCODE") = WW_SUBACCODE_D                                                 '補助科目コード
                '    O_ROW("INQKBN") = "0"                                                               '照会区分
                '    O_ROW("ACDCKBN") = "C"                                                              '貸借区分
                '    O_ROW("ACACHANTEI") = "AMC"                                                         '勘定科目判定コード
                '    O_ROW("DTLNO") = "02"                                                               '明細番号
                '    O_ROW("ACKEIJOORG") = WW_HEADrow("HORG")                                            '計上部署コード(配属部署)

                '    WW_ROW = O_TABLE.NewRow
                '    WW_ROW.ItemArray = O_ROW.ItemArray
                '    O_TABLE.Rows.Add(WW_ROW)
                'End If

                '追加データ
                If WW_HEADrow("DELFLG") = "0" Then
                    '借方
                    If WW_INQKBN_D = "1" Then
                        O_ROW("ACCODE") = WW_ACCODE_D                                                   '勘定科目コード
                        O_ROW("SUBACCODE") = WW_SUBACCODE_D                                             '補助科目コード
                        O_ROW("INQKBN") = WW_INQKBN_D                                                   '照会区分
                        O_ROW("ACDCKBN") = "D"                                                          '貸借区分
                        O_ROW("ACACHANTEI") = "AMD"                                                     '勘定科目判定コード
                        O_ROW("DTLNO") = "01"                                                           '明細番号
                        O_ROW("ACKEIJOORG") = WW_HEADrow("HORG")                                        '計上部署コード（作業部署）

                        Dim WW_ROW As DataRow = O_TABLE.NewRow
                        WW_ROW.ItemArray = O_ROW.ItemArray
                        O_TABLE.Rows.Add(WW_ROW)
                    End If

                    '貸方
                    If WW_INQKBN_C = "1" Then
                        O_ROW("ACCODE") = WW_ACCODE_C                                                   '勘定科目コード
                        O_ROW("SUBACCODE") = WW_SUBACCODE_C                                             '補助科目コード
                        O_ROW("INQKBN") = WW_INQKBN_C                                                   '照会区分
                        O_ROW("ACDCKBN") = "C"                                                          '貸借区分
                        O_ROW("ACACHANTEI") = "AMC"                                                     '勘定科目判定コード
                        O_ROW("DTLNO") = "02"                                                           '明細番号
                        O_ROW("ACKEIJOORG") = WW_HEADrow("HORG")                                        '計上部署コード（配属部署）

                        Dim WW_ROW As DataRow = O_TABLE.NewRow
                        WW_ROW.ItemArray = O_ROW.ItemArray
                        O_TABLE.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                Dim WW_CSV As String = DataRowToCSV(WW_T00009tbl.Rows(i))

                CS0011LOGWrite.INFSUBCLASS = "tbMonthlylTtlEdit"
                CS0011LOGWrite.INFPOSI = "tbMonthlylTtlEdit"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA = (" & WW_CSV & ")"
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
                Throw
            End Try
        Next

    End Sub


    ''' <summary>
    ''' ダウンロード、一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <param name="I_FILETYPE"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click(ByVal I_FILETYPE As String)

        '○ Excel用項目初期化
        For Each T00009row As DataRow In T00009tbl.Rows
            '車両区分(2種類)
            For WW_SYARYOKBN As Integer = 1 To 2
                '給与油種区分(10種類)
                For WW_OILPAYKBN As Integer = 1 To 10
                    Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_SYARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SYARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    T00009row(WW_UNLOADCNTTTL) = ""         '車両区分
                    T00009row(WW_HAIDISTANCETTL) = ""       '給与油種区分
                Next
            Next
        Next

        '○ 日別明細 or 月合計要求判定　…　Excel定義に月合計項目が有効ならば、月合計判定("ON")
        Dim WW_HANTEI As String = ""
        ExcelHantei(rightview.getReportId(), WW_HANTEI)

        '○ データ抽出(日別・月合計共通処理)
        Dim WW_OUTtbl As DataTable = New DataTable
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "CAMPCODE, HORG, STAFFCODE, WORKDATE"

        If WW_HANTEI = "月合計" Then
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '2' and SELECT = 1"
        Else
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and SELECT = 1"
        End If

        CS0026TBLSORT.sort(WW_OUTtbl)

        '○ 月合計編集 …　単トレ・油種情報付与
        If WW_HANTEI = "月合計" Then
            Dim WW_DTLtbl As DataTable = New DataTable
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.SORTING = "STAFFCODE, HDKBN, RECODEKBN, SELECT"
            CS0026TBLSORT.FILTER = "HDKBN = 'D' and RECODEKBN = '2' and SELECT = 1"
            CS0026TBLSORT.sort(WW_DTLtbl)

            Dim TBLview As DataView = New DataView(WW_DTLtbl)
            TBLview.Sort = "STAFFCODE"
            For i As Integer = 0 To WW_OUTtbl.Rows.Count - 1
                TBLview.RowFilter = "STAFFCODE = '" & WW_OUTtbl.Rows(i)("STAFFCODE") & "'"
                For j As Integer = 0 To TBLview.Count - 1
                    '車両区分 (SHARYOKBN 1:単車、2:トレーラ)
                    '給与油種区分 (OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素)
                    Dim WW_SYARYOKBN As String = Val(TBLview.Item(j)("SHARYOKBN")).ToString("00")
                    Dim WW_OILPAYKBN As String = TBLview.Item(j)("OILPAYKBN")

                    If (WW_SYARYOKBN = "01" OrElse WW_SYARYOKBN = "02") AndAlso
                        WW_OILPAYKBN >= "01" AndAlso WW_OILPAYKBN <= "10" Then
                        Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_SYARYOKBN & WW_OILPAYKBN
                        Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SYARYOKBN & WW_OILPAYKBN

                        WW_OUTtbl.Rows(i)(WW_UNLOADCNTTTL) = TBLview.Item(j)("UNLOADCNTTTL")
                        WW_OUTtbl.Rows(i)(WW_HAIDISTANCETTL) = TBLview.Item(j)("HAIDISTANCETTL")
                    End If
                Next
            Next

            TBLview.Dispose()
            TBLview = Nothing

            If Not IsNothing(WW_DTLtbl) Then
                WW_DTLtbl.Clear()
                WW_DTLtbl.Dispose()
                WW_DTLtbl = Nothing
            End If
        End If

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = I_FILETYPE                       '出力ファイル形式
        CS0030REPORT.TBLDATA = WW_OUTtbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        If I_FILETYPE = "XLSX" Then
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        Else
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
        End If

        If Not IsNothing(WW_OUTtbl) Then
            WW_OUTtbl.Clear()
            WW_OUTtbl.Dispose()
            WW_OUTtbl = Nothing
        End If

    End Sub


    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE2_Click()

        '○ 画面の使用禁止文字排除
        Master.eraseCharToIgnore(WF_NIGHTTIMETTL.Text)              '所定深夜
        Master.eraseCharToIgnore(WF_ORVERTIMETTL.Text)              '平日残業
        Master.eraseCharToIgnore(WF_WNIGHTTIMETTL.Text)             '平日深夜
        Master.eraseCharToIgnore(WF_SWORKTIMETTL.Text)              '日曜出勤
        Master.eraseCharToIgnore(WF_SNIGHTTIMETTL.Text)             '日曜深夜
        Master.eraseCharToIgnore(WF_HWORKTIMETTL.Text)              '休日出勤
        Master.eraseCharToIgnore(WF_HNIGHTTIMETTL.Text)             '休日深夜
        Master.eraseCharToIgnore(WF_WORKNISSUTTL.Text)              '所労
        Master.eraseCharToIgnore(WF_SHOUKETUNISSUTTL.Text)          '傷欠
        Master.eraseCharToIgnore(WF_KUMIKETUNISSUTTL.Text)          '組欠
        Master.eraseCharToIgnore(WF_ETCKETUNISSUTTL.Text)           '他欠
        Master.eraseCharToIgnore(WF_NENKYUNISSUTTL.Text)            '年休
        Master.eraseCharToIgnore(WF_TOKUKYUNISSUTTL.Text)           '特休
        Master.eraseCharToIgnore(WF_CHIKOKSOTAINISSUTTL.Text)       '遅早
        Master.eraseCharToIgnore(WF_STOCKNISSUTTL.Text)             'ストック休暇
        Master.eraseCharToIgnore(WF_KYOTEIWEEKNISSUTTL.Text)        '協約週休
        Master.eraseCharToIgnore(WF_WEEKNISSUTTL.Text)              '週休
        Master.eraseCharToIgnore(WF_DAIKYUNISSUTTL.Text)            '代休
        Master.eraseCharToIgnore(WF_NENSHINISSUTTL.Text)            '年始出勤日
        Master.eraseCharToIgnore(WF_SHUKCHOKNNISSUTTL.Text)         '宿日直年始
        Master.eraseCharToIgnore(WF_SHUKCHOKNISSUTTL.Text)          '宿日直通常
        Master.eraseCharToIgnore(WF_SHUKCHOKNHLDNISSUTTL.Text)      '宿日直年始（翌日休み）
        Master.eraseCharToIgnore(WF_SHUKCHOKHLDNISSUTTL.Text)       '宿日直通常（翌日休み）
        Master.eraseCharToIgnore(WF_TOKUSA1TIMETTL.Text)            '特作Ⅰ
        Master.eraseCharToIgnore(WF_HAYADETIMETTL.Text)             '早出補填
        Master.eraseCharToIgnore(WF_NENMATUNISSUTTL.Text)           '年末出勤日
        Master.eraseCharToIgnore(WF_JIKYUSHATIMETTL.Text)           '時給者時間
        Master.eraseCharToIgnore(WF_HDAIWORKTIMETTL.Text)           '代休出勤
        Master.eraseCharToIgnore(WF_HDAINIGHTTIMETTL.Text)          '代休深夜
        Master.eraseCharToIgnore(WF_SDAIWORKTIMETTL.Text)           '日曜代休出勤
        Master.eraseCharToIgnore(WF_SDAINIGHTTIMETTL.Text)          '日曜代休深夜
        Master.eraseCharToIgnore(WF_WWORKTIMETTL.Text)              '所定内時間
        Master.eraseCharToIgnore(WF_HWORKNISSUTTL.Text)             '休日出勤日数

        '○ 月合計入力の変更取込
        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            If T00009INProw("HDKBN") = "H" AndAlso T00009INProw("RECODEKBN") = "2" Then
                T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                T00009INProw("TIMSTP") = 0
                T00009INProw("WORKNISSUCHO") = Val(WF_WORKNISSUTTL.Text) - Val(T00009INProw("WORKNISSU"))
                T00009INProw("WORKNISSUTTL") = Val(T00009INProw("WORKNISSU")) + Val(T00009INProw("WORKNISSUCHO"))
                T00009INProw("SHOUKETUNISSUCHO") = Val(WF_SHOUKETUNISSUTTL.Text) - Val(T00009INProw("SHOUKETUNISSU"))
                T00009INProw("SHOUKETUNISSUTTL") = Val(T00009INProw("SHOUKETUNISSU")) + Val(T00009INProw("SHOUKETUNISSUCHO"))
                T00009INProw("KUMIKETUNISSUCHO") = Val(WF_KUMIKETUNISSUTTL.Text) - Val(T00009INProw("KUMIKETUNISSU"))
                T00009INProw("KUMIKETUNISSUTTL") = Val(T00009INProw("KUMIKETUNISSU")) + Val(T00009INProw("KUMIKETUNISSUCHO"))
                T00009INProw("ETCKETUNISSUCHO") = Val(WF_ETCKETUNISSUTTL.Text) - Val(T00009INProw("ETCKETUNISSU"))
                T00009INProw("ETCKETUNISSUTTL") = Val(T00009INProw("ETCKETUNISSU")) + Val(T00009INProw("ETCKETUNISSUCHO"))
                T00009INProw("NENKYUNISSUCHO") = Val(WF_NENKYUNISSUTTL.Text) - Val(T00009INProw("NENKYUNISSU"))
                T00009INProw("NENKYUNISSUTTL") = Val(T00009INProw("NENKYUNISSU")) + Val(T00009INProw("NENKYUNISSUCHO"))
                T00009INProw("TOKUKYUNISSUCHO") = Val(WF_TOKUKYUNISSUTTL.Text) - Val(T00009INProw("TOKUKYUNISSU"))
                T00009INProw("TOKUKYUNISSUTTL") = Val(T00009INProw("TOKUKYUNISSU")) + Val(T00009INProw("TOKUKYUNISSUCHO"))
                T00009INProw("CHIKOKSOTAINISSUCHO") = Val(WF_CHIKOKSOTAINISSUTTL.Text) - Val(T00009INProw("CHIKOKSOTAINISSU"))
                T00009INProw("CHIKOKSOTAINISSUTTL") = Val(T00009INProw("CHIKOKSOTAINISSU")) + Val(T00009INProw("CHIKOKSOTAINISSUCHO"))
                T00009INProw("STOCKNISSUCHO") = Val(WF_STOCKNISSUTTL.Text) - Val(T00009INProw("STOCKNISSU"))
                T00009INProw("STOCKNISSUTTL") = Val(T00009INProw("STOCKNISSU")) + Val(T00009INProw("STOCKNISSUCHO"))
                T00009INProw("KYOTEIWEEKNISSUCHO") = Val(WF_KYOTEIWEEKNISSUTTL.Text) - Val(T00009INProw("KYOTEIWEEKNISSU"))
                T00009INProw("KYOTEIWEEKNISSUTTL") = Val(T00009INProw("KYOTEIWEEKNISSU")) + Val(T00009INProw("KYOTEIWEEKNISSUCHO"))
                T00009INProw("WEEKNISSUCHO") = Val(WF_WEEKNISSUTTL.Text) - Val(T00009INProw("WEEKNISSU"))
                T00009INProw("WEEKNISSUTTL") = Val(T00009INProw("WEEKNISSU")) + Val(T00009INProw("WEEKNISSUCHO"))
                T00009INProw("DAIKYUNISSUCHO") = Val(WF_DAIKYUNISSUTTL.Text) - Val(T00009INProw("DAIKYUNISSU"))
                T00009INProw("DAIKYUNISSUTTL") = Val(T00009INProw("DAIKYUNISSU")) + Val(T00009INProw("DAIKYUNISSUCHO"))
                T00009INProw("NENSHINISSUCHO") = Val(WF_NENSHINISSUTTL.Text) - Val(T00009INProw("NENSHINISSU"))
                T00009INProw("NENSHINISSUTTL") = Val(T00009INProw("NENSHINISSU")) + Val(T00009INProw("NENSHINISSUCHO"))
                T00009INProw("SHUKCHOKNNISSUCHO") = Val(WF_SHUKCHOKNNISSUTTL.Text) - Val(T00009INProw("SHUKCHOKNNISSU"))
                T00009INProw("SHUKCHOKNNISSUTTL") = Val(T00009INProw("SHUKCHOKNNISSU")) + Val(T00009INProw("SHUKCHOKNNISSUCHO"))
                T00009INProw("SHUKCHOKNISSUCHO") = Val(WF_SHUKCHOKNISSUTTL.Text) - Val(T00009INProw("SHUKCHOKNISSU"))
                T00009INProw("SHUKCHOKNISSUTTL") = Val(T00009INProw("SHUKCHOKNISSU")) + Val(T00009INProw("SHUKCHOKNISSUCHO"))
                T00009INProw("SHUKCHOKNHLDNISSUCHO") = Val(WF_SHUKCHOKNHLDNISSUTTL.Text) - Val(T00009INProw("SHUKCHOKNHLDNISSU"))
                T00009INProw("SHUKCHOKNHLDNISSUTTL") = Val(T00009INProw("SHUKCHOKNHLDNISSU")) + Val(T00009INProw("SHUKCHOKNHLDNISSUCHO"))
                T00009INProw("SHUKCHOKHLDNISSUCHO") = Val(WF_SHUKCHOKHLDNISSUTTL.Text) - Val(T00009INProw("SHUKCHOKHLDNISSU"))
                T00009INProw("SHUKCHOKHLDNISSUTTL") = Val(T00009INProw("SHUKCHOKHLDNISSU")) + Val(T00009INProw("SHUKCHOKHLDNISSUCHO"))
                T00009INProw("NENMATUNISSUCHO") = Val(WF_NENMATUNISSUTTL.Text) - Val(T00009INProw("NENMATUNISSU"))
                T00009INProw("NENMATUNISSUTTL") = Val(T00009INProw("NENMATUNISSU")) + Val(T00009INProw("NENMATUNISSUCHO"))
                T00009INProw("HWORKNISSUCHO") = Val(WF_HWORKNISSUTTL.Text) - Val(T00009INProw("HWORKNISSU"))
                T00009INProw("HWORKNISSUTTL") = Val(T00009INProw("HWORKNISSU")) + Val(T00009INProw("HWORKNISSUCHO"))

                T00009INProw("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_NIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T00009INProw("NIGHTTIME"))
                T00009INProw("NIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("NIGHTTIME")) + Val(T00009INProw("NIGHTTIMECHO"))
                T00009INProw("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(WF_ORVERTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("ORVERTIME")) + T0007COM.HHMMtoMinutes(T00009INProw("ORVERTIMEADD")))
                T00009INProw("ORVERTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("ORVERTIME")) + Val(T00009INProw("ORVERTIMECHO"))
                T00009INProw("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_WNIGHTTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("WNIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009INProw("WNIGHTTIMEADD")))
                T00009INProw("WNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("WNIGHTTIME")) + Val(T00009INProw("WNIGHTTIMECHO"))
                T00009INProw("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_SWORKTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("SWORKTIME")) + T0007COM.HHMMtoMinutes(T00009INProw("SWORKTIMEADD")))
                T00009INProw("SWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("SWORKTIME")) + Val(T00009INProw("SWORKTIMECHO"))
                T00009INProw("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_SNIGHTTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("SNIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009INProw("SNIGHTTIMEADD")))
                T00009INProw("SNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("SNIGHTTIME")) + Val(T00009INProw("SNIGHTTIMECHO"))
                T00009INProw("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_HWORKTIMETTL.Text) - T0007COM.HHMMtoMinutes(T00009INProw("HWORKTIME"))
                T00009INProw("HWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("HWORKTIME")) + Val(T00009INProw("HWORKTIMECHO"))
                T00009INProw("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_HNIGHTTIMETTL.Text) - T0007COM.HHMMtoMinutes(T00009INProw("HNIGHTTIME"))
                T00009INProw("HNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("HNIGHTTIME")) + Val(T00009INProw("HNIGHTTIMECHO"))
                T00009INProw("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(WF_TOKUSA1TIMETTL.Text) - T0007COM.HHMMtoMinutes(T00009INProw("TOKUSA1TIME"))
                T00009INProw("TOKUSA1TIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("TOKUSA1TIME")) + Val(T00009INProw("TOKUSA1TIMECHO"))
                T00009INProw("HAYADETIMECHO") = T0007COM.HHMMtoMinutes(WF_HAYADETIMETTL.Text) - T0007COM.HHMMtoMinutes(T00009INProw("HAYADETIME"))
                T00009INProw("HAYADETIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("HAYADETIME")) + Val(T00009INProw("HAYADETIMECHO"))
                T00009INProw("JIKYUSHATIMECHO") = T0007COM.HHMMtoMinutes(WF_JIKYUSHATIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("JIKYUSHATIME")))
                T00009INProw("JIKYUSHATIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("JIKYUSHATIME")) + Val(T00009INProw("JIKYUSHATIMECHO"))
                T00009INProw("HDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_HDAIWORKTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("HDAIWORKTIME")))
                T00009INProw("HDAIWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("HDAIWORKTIME")) + Val(T00009INProw("HDAIWORKTIMECHO"))
                T00009INProw("HDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_HDAINIGHTTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("HDAINIGHTTIME")))
                T00009INProw("HDAINIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("HDAINIGHTTIME")) + Val(T00009INProw("HDAINIGHTTIMECHO"))
                T00009INProw("SDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_SDAIWORKTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("SDAIWORKTIME")))
                T00009INProw("SDAIWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("SDAIWORKTIME")) + Val(T00009INProw("SDAIWORKTIMECHO"))
                T00009INProw("SDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(WF_SDAINIGHTTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("SDAINIGHTTIME")))
                T00009INProw("SDAINIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("SDAINIGHTTIME")) + Val(T00009INProw("SDAINIGHTTIMECHO"))
                T00009INProw("WWORKTIMECHO") = T0007COM.HHMMtoMinutes(WF_WWORKTIMETTL.Text) - (T0007COM.HHMMtoMinutes(T00009INProw("WWORKTIME")))
                T00009INProw("WWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009INProw("WWORKTIME")) + Val(T00009INProw("WWORKTIMECHO"))

                T00009INProw("NIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("NIGHTTIMECHO"))
                T00009INProw("NIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("NIGHTTIMETTL"))
                T00009INProw("ORVERTIMECHO") = T0007COM.formatHHMM(T00009INProw("ORVERTIMECHO"))
                T00009INProw("ORVERTIMEADD") = T00009INProw("ORVERTIMEADD")
                T00009INProw("ORVERTIMETTL") = T0007COM.formatHHMM(T00009INProw("ORVERTIMETTL"))
                T00009INProw("WNIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("WNIGHTTIMECHO"))
                T00009INProw("WNIGHTTIMEADD") = T00009INProw("WNIGHTTIMEADD")
                T00009INProw("WNIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("WNIGHTTIMETTL"))
                T00009INProw("SWORKTIMECHO") = T0007COM.formatHHMM(T00009INProw("SWORKTIMECHO"))
                T00009INProw("SWORKTIMEADD") = T00009INProw("SWORKTIMEADD")
                T00009INProw("SWORKTIMETTL") = T0007COM.formatHHMM(T00009INProw("SWORKTIMETTL"))
                T00009INProw("SNIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("SNIGHTTIMECHO"))
                T00009INProw("SNIGHTTIMEADD") = T00009INProw("SNIGHTTIMEADD")
                T00009INProw("SNIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("SNIGHTTIMETTL"))
                T00009INProw("HWORKTIMECHO") = T0007COM.formatHHMM(T00009INProw("HWORKTIMECHO"))
                T00009INProw("HWORKTIMETTL") = T0007COM.formatHHMM(T00009INProw("HWORKTIMETTL"))
                T00009INProw("HNIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("HNIGHTTIMECHO"))
                T00009INProw("HNIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("HNIGHTTIMETTL"))
                T00009INProw("TOKUSA1TIMECHO") = T0007COM.formatHHMM(T00009INProw("TOKUSA1TIMECHO"))
                T00009INProw("TOKUSA1TIMETTL") = T0007COM.formatHHMM(T00009INProw("TOKUSA1TIMETTL"))
                T00009INProw("HAYADETIMECHO") = T0007COM.formatHHMM(T00009INProw("HAYADETIMECHO"))
                T00009INProw("HAYADETIMETTL") = T0007COM.formatHHMM(T00009INProw("HAYADETIMETTL"))
                T00009INProw("JIKYUSHATIMECHO") = T0007COM.formatHHMM(T00009INProw("JIKYUSHATIMECHO"))
                T00009INProw("JIKYUSHATIMETTL") = T0007COM.formatHHMM(T00009INProw("JIKYUSHATIMETTL"))
                T00009INProw("HDAIWORKTIMECHO") = T0007COM.formatHHMM(T00009INProw("HDAIWORKTIMECHO"))
                T00009INProw("HDAIWORKTIMETTL") = T0007COM.formatHHMM(T00009INProw("HDAIWORKTIMETTL"))
                T00009INProw("HDAINIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("HDAINIGHTTIMECHO"))
                T00009INProw("HDAINIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("HDAINIGHTTIMETTL"))
                T00009INProw("SDAIWORKTIMECHO") = T0007COM.formatHHMM(T00009INProw("SDAIWORKTIMECHO"))
                T00009INProw("SDAIWORKTIMETTL") = T0007COM.formatHHMM(T00009INProw("SDAIWORKTIMETTL"))
                T00009INProw("SDAINIGHTTIMECHO") = T0007COM.formatHHMM(T00009INProw("SDAINIGHTTIMECHO"))
                T00009INProw("SDAINIGHTTIMETTL") = T0007COM.formatHHMM(T00009INProw("SDAINIGHTTIMETTL"))
                T00009INProw("WWORKTIMECHO") = T0007COM.formatHHMM(T00009INProw("WWORKTIMECHO"))
                T00009INProw("WWORKTIMETTL") = T0007COM.formatHHMM(T00009INProw("WWORKTIMETTL"))

                '名称取得
                CODENAME_get("CAMPCODE", T00009INProw("CAMPCODE"), T00009INProw("CAMPNAMES"), WW_DUMMY)                     '会社コード
                CODENAME_get("STAFFKBN", T00009INProw("STAFFKBN"), T00009INProw("STAFFKBNNAMES"), WW_DUMMY)                 '社員区分
                CODENAME_get("STAFFKBN3", T00009INProw("STAFFKBN"), T00009INProw("STAFFKBNTAISHOGAI"), WW_DUMMY)            '残業申請対象外
                CODENAME_get("ORG", T00009INProw("MORG"), T00009INProw("MORGNAMES"), WW_DUMMY)                              '管理部署
                CODENAME_get("ORG", T00009INProw("HORG"), T00009INProw("HORGNAMES"), WW_DUMMY)                              '配属部署
                CODENAME_get("ORG", T00009INProw("SORG"), T00009INProw("SORGNAMES"), WW_DUMMY)                              '作業部署
                CODENAME_get("HOLIDAYKBN", T00009INProw("HOLIDAYKBN"), T00009INProw("HOLIDAYKBNNAMES"), WW_DUMMY)           '休日区分
                CODENAME_get("PAYKBN", T00009INProw("PAYKBN"), T00009INProw("PAYKBNNAMES"), WW_DUMMY)                       '勤怠区分
                CODENAME_get("SHUKCHOKKBN", T00009INProw("SHUKCHOKKBN"), T00009INProw("SHUKCHOKKBNNAMES"), WW_DUMMY)        '宿日直区分
                CODENAME_get("RIYU", T00009INProw("RIYU"), T00009INProw("RIYUNAMES"), WW_DUMMY)                             '理由
                Exit For
            End If
        Next

        '○ 調整レコードの再作成
        T0007COM.T0007_ChoseiRecodeCreate(T00009INPtbl)

        '○ 全体データにT00009INPtblを反映(削除してマージ)
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = "STAFFCODE <> '" & WF_STAFFCODE.Text & "'"
        CS0026TBLSORT.sort(T00009tbl)
        T00009tbl.Merge(T00009INPtbl)

        '○ テーブル保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        '○ 画面切替
        WF_DISP.Value = "List"
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        If WF_DISP.Value = "Adjust" Then
            '画面切替
            WF_DISP.Value = "List"
        Else
            If WF_BEFORE_MAPID.Value = GRT00009WRKINC.MAPIDS Then
                '前画面に戻る
                Master.transitionPrevPage()
            ElseIf WF_BEFORE_MAPID.Value = GRT00010WRKINC.MAPID Then
                '承認画面から遷移してきた場合
                Master.MAPID = GRT00010WRKINC.MAPIDS
                work.WF_SEL_CAMPCODE.Text = work.WF_T10_CAMPCODE.Text       '会社コード
                work.WF_SEL_TAISHOYM.Text = work.WF_T10_TAISHOYM.Text       '申請年月
                work.WF_SEL_HORG.Text = work.WF_T10_HORG.Text               '配属部署
                Master.transitionPage()
            End If
        End If

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ 勤怠ALLのみ処理可能
        If Not Master.MAPvariant Like GRT00009WRKINC.VAR_ALL Then
            Master.output(C_MESSAGE_NO.EXCEL_UPLOAD_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ 日別明細 or 月合計要求判定　…　Excel定義に月合計項目が有効ならば、月合計判定("ON")
        Dim WW_HANTEI As String = ""
        ExcelHantei(CS0023XLSUPLOAD.REPORTID, WW_HANTEI)
        If WW_HANTEI = "ERR" Then
            Master.output(C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○ インポートファイルの列情報有り無し判定
        Master.CreateEmptyTable(T00009INPtbl, WF_XMLsaveF.Value)
        ExcelInpMake(CS0023XLSUPLOAD.TBLDATA, WW_HANTEI)

        '○ INPデータチェック
        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            INPTableCheck(T00009INProw, WW_ERR_SW)

            If isNormal(WW_ERR_SW) Then
                T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                T00009INProw("SELECT") = 1
            Else
                T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                T00009INProw("SELECT") = 0
            End If

            '残業申請対象外を判定し、対象外の場合は残業申請を削除する
            CODENAME_get("STAFFKBN3", T00009INProw("STAFFKBN"), T00009INProw("STAFFKBNTAISHOGAI"), WW_RTN_SW)       '残業申請対象外
            CODENAME_get("STAFFKBN4", T00009INProw("STAFFKBN"), T00009INProw("STAFFKBNTAISHOGAI"), WW_RTN_SW2)      '残業申請対象外
            If isNormal(WW_RTN_SW) OrElse isNormal(WW_RTN_SW2) Then
                T00009INProw("YENDTIME") = "00:00"
                T00009INProw("RIYU") = ""
                T00009INProw("RIYUNAMES") = ""
                T00009INProw("RIYUETC") = ""
            End If
        Next

        '○ 重大エラーの場合、インポートデータから削除
        For i As Integer = T00009INPtbl.Rows.Count - 1 To 0 Step -1
            If T00009INPtbl.Rows(i)("SELECT") = 0 Then
                T00009INPtbl.Rows(i).Delete()
            End If
        Next

        '○ 画面表示の従業員のみ抽出
        Dim WW_COLs As String() = {"STAFFCODE"}
        Dim WW_KEYtbl As DataTable = New DataTable
        Dim TBLview As DataView = New DataView(T00009tbl)
        WW_KEYtbl = TBLview.ToTable(True, WW_COLs)

        Dim WW_FIND As Boolean = False
        For i As Integer = T00009INPtbl.Rows.Count - 1 To 0 Step -1
            WW_FIND = False
            For Each WW_KEYrow As DataRow In WW_KEYtbl.Rows
                If WW_KEYrow("STAFFCODE") = T00009INPtbl.Rows(i)("STAFFCODE") Then
                    WW_FIND = True
                    Exit For
                End If
            Next

            If Not WW_FIND Then
                Dim WW_CheckMES1 As String = "・更新できないレコード(従業員エラー)です。"
                Dim WW_CheckMES2 As String = "画面選択されていない従業員です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INPtbl.Rows(i))

                T00009INPtbl.Rows(i).Delete()
            End If
        Next

        TBLview.Dispose()
        TBLview = Nothing

        If Not IsNothing(WW_KEYtbl) Then
            WW_KEYtbl.Clear()
            WW_KEYtbl.Dispose()
            WW_KEYtbl = Nothing
        End If

        Dim WW_T00009tbl As DataTable = T00009INPtbl.Clone()
        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            If T00009INProw("RECODEKBN") = "2" AndAlso T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                '月初日をセット
                Dim WW_DATE As Date = CDate(T00009INProw("TAISHOYM") & "/01")
                '月末日を算出
                T00009INProw("WORKDATE") = WW_DATE.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")

                Dim WW_UPD_FLG As Boolean = False
                Dim WW_UNLOADCNT As Integer = 0
                Dim WW_UNLOADCNTCHO As Integer = 0
                Dim WW_HAIDISTANCE As Double = 0
                Dim WW_HAIDISTANCECHO As Double = 0

                For Each T00009row As DataRow In T00009tbl.Rows
                    If T00009row("WORKDATE") = T00009INProw("WORKDATE") AndAlso
                        T00009row("STAFFCODE") = T00009INProw("STAFFCODE") AndAlso
                        T00009row("RECODEKBN") = T00009INProw("RECODEKBN") AndAlso
                        T00009row("SELECT") = 1 Then

                        If T00009row("HDKBN") = "H" Then
                            Dim WW_T00009row As DataRow = WW_T00009tbl.NewRow
                            WW_T00009row.ItemArray = T00009row.ItemArray

                            WW_T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_T00009row("TIMSTP") = 0
                            WW_T00009row("WORKNISSUCHO") = Val(T00009INProw("WORKNISSUTTL")) - T00009row("WORKNISSU")
                            WW_T00009row("WORKNISSUTTL") = Val(T00009row("WORKNISSU")) + Val(T00009row("WORKNISSUCHO"))
                            WW_T00009row("SHOUKETUNISSUCHO") = Val(T00009INProw("SHOUKETUNISSUTTL")) - T00009row("SHOUKETUNISSU")
                            WW_T00009row("SHOUKETUNISSUTTL") = Val(T00009row("SHOUKETUNISSU")) + Val(T00009row("SHOUKETUNISSUCHO"))
                            WW_T00009row("KUMIKETUNISSUCHO") = Val(T00009INProw("KUMIKETUNISSUTTL")) - T00009row("KUMIKETUNISSU")
                            WW_T00009row("KUMIKETUNISSUTTL") = Val(T00009row("KUMIKETUNISSU")) + Val(T00009row("KUMIKETUNISSUCHO"))
                            WW_T00009row("ETCKETUNISSUCHO") = Val(T00009INProw("ETCKETUNISSUTTL")) - T00009row("ETCKETUNISSU")
                            WW_T00009row("ETCKETUNISSUTTL") = Val(T00009row("ETCKETUNISSU")) + Val(T00009row("ETCKETUNISSUCHO"))
                            WW_T00009row("NENKYUNISSUCHO") = Val(T00009INProw("NENKYUNISSUTTL")) - T00009row("NENKYUNISSU")
                            WW_T00009row("NENKYUNISSUTTL") = Val(T00009row("NENKYUNISSU")) + Val(T00009row("NENKYUNISSUCHO"))
                            WW_T00009row("TOKUKYUNISSUCHO") = Val(T00009INProw("TOKUKYUNISSUTTL")) - T00009row("TOKUKYUNISSU")
                            WW_T00009row("TOKUKYUNISSUTTL") = Val(T00009row("TOKUKYUNISSU")) + Val(T00009row("TOKUKYUNISSUCHO"))
                            WW_T00009row("CHIKOKSOTAINISSUCHO") = Val(T00009INProw("CHIKOKSOTAINISSUTTL")) - T00009row("CHIKOKSOTAINISSU")
                            WW_T00009row("CHIKOKSOTAINISSUTTL") = Val(T00009row("CHIKOKSOTAINISSU")) + Val(T00009row("CHIKOKSOTAINISSUCHO"))
                            WW_T00009row("STOCKNISSUCHO") = Val(T00009INProw("STOCKNISSUTTL")) - T00009row("STOCKNISSU")
                            WW_T00009row("STOCKNISSUTTL") = Val(T00009row("STOCKNISSU")) + Val(T00009row("STOCKNISSUCHO"))
                            WW_T00009row("KYOTEIWEEKNISSUCHO") = Val(T00009INProw("KYOTEIWEEKNISSUTTL")) - T00009row("KYOTEIWEEKNISSU")
                            WW_T00009row("KYOTEIWEEKNISSUTTL") = Val(T00009row("KYOTEIWEEKNISSU")) + Val(T00009row("KYOTEIWEEKNISSUCHO"))
                            WW_T00009row("WEEKNISSUCHO") = Val(T00009INProw("WEEKNISSUTTL")) - T00009row("WEEKNISSU")
                            WW_T00009row("WEEKNISSUTTL") = Val(T00009row("WEEKNISSU")) + Val(T00009row("WEEKNISSUCHO"))
                            WW_T00009row("DAIKYUNISSUCHO") = Val(T00009INProw("DAIKYUNISSUTTL")) - T00009row("DAIKYUNISSU")
                            WW_T00009row("DAIKYUNISSUTTL") = Val(T00009row("DAIKYUNISSU")) + Val(T00009row("DAIKYUNISSUCHO"))
                            WW_T00009row("NENSHINISSUCHO") = Val(T00009INProw("NENSHINISSUTTL")) - T00009row("NENSHINISSU")
                            WW_T00009row("NENSHINISSUTTL") = Val(T00009row("NENSHINISSU")) + Val(T00009row("NENSHINISSUCHO"))
                            WW_T00009row("SHUKCHOKNNISSUCHO") = Val(T00009INProw("SHUKCHOKNNISSUTTL")) - T00009row("SHUKCHOKNNISSU")
                            WW_T00009row("SHUKCHOKNNISSUTTL") = Val(T00009row("SHUKCHOKNNISSU")) + Val(T00009row("SHUKCHOKNNISSUCHO"))
                            WW_T00009row("SHUKCHOKNISSUCHO") = Val(T00009INProw("SHUKCHOKNISSUTTL")) - T00009row("SHUKCHOKNISSU")
                            WW_T00009row("SHUKCHOKNISSUTTL") = Val(T00009row("SHUKCHOKNISSU")) + Val(T00009row("SHUKCHOKNISSUCHO"))
                            WW_T00009row("SHUKCHOKNHLDNISSUCHO") = Val(T00009INProw("SHUKCHOKNHLDNISSUTTL")) - T00009row("SHUKCHOKNHLDNISSU")
                            WW_T00009row("SHUKCHOKNHLDNISSUTTL") = Val(T00009row("SHUKCHOKNHLDNISSU")) + Val(T00009row("SHUKCHOKNHLDNISSUCHO"))
                            WW_T00009row("SHUKCHOKHLDNISSUCHO") = Val(T00009INProw("SHUKCHOKHLDNISSUTTL")) - T00009row("SHUKCHOKHLDNISSU")
                            WW_T00009row("SHUKCHOKHLDNISSUTTL") = Val(T00009row("SHUKCHOKHLDNISSU")) + Val(T00009row("SHUKCHOKHLDNISSUCHO"))
                            WW_T00009row("TOKSAAKAISUCHO") = Val(T00009INProw("TOKSAAKAISUTTL")) - T00009row("TOKSAAKAISU")
                            WW_T00009row("TOKSAAKAISUTTL") = Val(T00009row("TOKSAAKAISU")) + Val(T00009row("TOKSAAKAISUCHO"))
                            WW_T00009row("TOKSABKAISUCHO") = Val(T00009INProw("TOKSABKAISUTTL")) - T00009row("TOKSABKAISU")
                            WW_T00009row("TOKSABKAISUTTL") = Val(T00009row("TOKSABKAISU")) + Val(T00009row("TOKSABKAISUCHO"))
                            WW_T00009row("TOKSACKAISUCHO") = Val(T00009INProw("TOKSACKAISUTTL")) - T00009row("TOKSACKAISU")
                            WW_T00009row("TOKSACKAISUTTL") = Val(T00009row("TOKSACKAISU")) + Val(T00009row("TOKSACKAISUCHO"))
                            WW_T00009row("TENKOKAISUCHO") = Val(T00009INProw("TENKOKAISUTTL")) - T00009row("TENKOKAISU")
                            WW_T00009row("TENKOKAISUTTL") = Val(T00009row("TENKOKAISU")) + Val(T00009row("TENKOKAISUCHO"))
                            WW_T00009row("NENMATUNISSUCHO") = Val(T00009INProw("NENMATUNISSUTTL")) - Val(T00009row("NENMATUNISSU"))
                            WW_T00009row("NENMATUNISSUTTL") = Val(T00009row("NENMATUNISSU")) + Val(T00009row("NENMATUNISSUCHO"))
                            WW_T00009row("HWORKNISSUCHO") = Val(T00009INProw("HWORKNISSUTTL")) - Val(T00009row("HWORKNISSU"))
                            WW_T00009row("HWORKNISSUTTL") = Val(T00009row("HWORKNISSU")) + Val(T00009row("HWORKNISSUCHO"))

                            WW_T00009row("NIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("NIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("NIGHTTIME"))
                            WW_T00009row("NIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("NIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("NIGHTTIMECHO"))
                            WW_T00009row("ORVERTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("ORVERTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("ORVERTIME"))
                            WW_T00009row("ORVERTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("ORVERTIME")) + T0007COM.HHMMtoMinutes(T00009row("ORVERTIMECHO"))
                            WW_T00009row("WNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("WNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("WNIGHTTIME"))
                            WW_T00009row("WNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("WNIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("WNIGHTTIMECHO"))
                            WW_T00009row("SWORKTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("SWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("SWORKTIME"))
                            WW_T00009row("SWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("SWORKTIME")) + T0007COM.HHMMtoMinutes(T00009row("SWORKTIMECHO"))
                            WW_T00009row("SNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("SNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("SNIGHTTIME"))
                            WW_T00009row("SNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("SNIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("SNIGHTTIMECHO"))
                            WW_T00009row("HWORKTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HWORKTIME"))
                            WW_T00009row("HWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HWORKTIME")) + T0007COM.HHMMtoMinutes(T00009row("HWORKTIMECHO"))
                            WW_T00009row("HNIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HNIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HNIGHTTIME"))
                            WW_T00009row("HNIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HNIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("HNIGHTTIMECHO"))
                            WW_T00009row("HOANTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HOANTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HOANTIME"))
                            WW_T00009row("HOANTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HOANTIME")) + T0007COM.HHMMtoMinutes(T00009row("HOANTIMECHO"))
                            WW_T00009row("KOATUTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("KOATUTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("KOATUTIME"))
                            WW_T00009row("KOATUTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("KOATUTIME")) + T0007COM.HHMMtoMinutes(T00009row("KOATUTIMECHO"))
                            WW_T00009row("TOKUSA1TIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("TOKUSA1TIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("TOKUSA1TIME"))
                            WW_T00009row("TOKUSA1TIMETTL") = T0007COM.HHMMtoMinutes(T00009row("TOKUSA1TIME")) + T0007COM.HHMMtoMinutes(T00009row("TOKUSA1TIMECHO"))
                            WW_T00009row("HAYADETIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HAYADETIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HAYADETIME"))
                            WW_T00009row("HAYADETIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HAYADETIME")) + T0007COM.HHMMtoMinutes(T00009row("HAYADETIMECHO"))
                            WW_T00009row("JIKYUSHATIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("JIKYUSHATIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("JIKYUSHATIME"))
                            WW_T00009row("JIKYUSHATIMETTL") = T0007COM.HHMMtoMinutes(T00009row("JIKYUSHATIME")) + T0007COM.HHMMtoMinutes(T00009row("JIKYUSHATIMECHO"))
                            WW_T00009row("HDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HDAIWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HDAIWORKTIME"))
                            WW_T00009row("HDAIWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HDAIWORKTIME")) + T0007COM.HHMMtoMinutes(T00009row("HDAIWORKTIMECHO"))
                            WW_T00009row("HDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("HDAINIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("HDAINIGHTTIME"))
                            WW_T00009row("HDAINIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("HDAINIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("HDAINIGHTTIMECHO"))
                            WW_T00009row("SDAIWORKTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("SDAIWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("SDAIWORKTIME"))
                            WW_T00009row("SDAIWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("SDAIWORKTIME")) + T0007COM.HHMMtoMinutes(T00009row("SDAIWORKTIMECHO"))
                            WW_T00009row("SDAINIGHTTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("SDAINIGHTTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("SDAINIGHTTIME"))
                            WW_T00009row("SDAINIGHTTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("SDAINIGHTTIME")) + T0007COM.HHMMtoMinutes(T00009row("SDAINIGHTTIMECHO"))
                            WW_T00009row("WWORKTIMECHO") = T0007COM.HHMMtoMinutes(T00009INProw("WWORKTIMETTL")) - T0007COM.HHMMtoMinutes(T00009row("WWORKTIME"))
                            WW_T00009row("WWORKTIMETTL") = T0007COM.HHMMtoMinutes(T00009row("WWORKTIME")) + T0007COM.HHMMtoMinutes(T00009row("SDAINIGHTTIMECHO"))

                            WW_T00009row("PONPNISSUCHO") = Val(T00009INProw("PONPNISSUTTL")) - Val(T00009row("PONPNISSU"))
                            WW_T00009row("PONPNISSUTTL") = Val(T00009row("PONPNISSU")) + Val(T00009row("PONPNISSUCHO"))
                            WW_T00009row("BULKNISSUCHO") = Val(T00009INProw("BULKNISSUTTL")) - T00009row("BULKNISSU")
                            WW_T00009row("BULKNISSUTTL") = Val(T00009row("BULKNISSU")) + Val(T00009row("BULKNISSUCHO"))
                            WW_T00009row("TRAILERNISSUCHO") = Val(T00009INProw("TRAILERNISSUTTL")) - T00009row("TRAILERNISSU")
                            WW_T00009row("TRAILERNISSUTTL") = Val(T00009row("TRAILERNISSU")) + Val(T00009row("TRAILERNISSUCHO"))
                            WW_T00009row("BKINMUKAISUCHO") = Val(T00009INProw("BKINMUKAISUTTL")) - T00009row("BKINMUKAISU")
                            WW_T00009row("BKINMUKAISUTTL") = Val(T00009row("BKINMUKAISU")) + Val(T00009row("BKINMUKAISUCHO"))

                            WW_T00009row("NIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("NIGHTTIMECHO"))
                            WW_T00009row("NIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("NIGHTTIMETTL"))
                            WW_T00009row("ORVERTIMECHO") = T0007COM.formatHHMM(WW_T00009row("ORVERTIMECHO"))
                            WW_T00009row("ORVERTIMETTL") = T0007COM.formatHHMM(WW_T00009row("ORVERTIMETTL"))
                            WW_T00009row("WNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("WNIGHTTIMECHO"))
                            WW_T00009row("WNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("WNIGHTTIMETTL"))
                            WW_T00009row("SWORKTIMECHO") = T0007COM.formatHHMM(WW_T00009row("SWORKTIMECHO"))
                            WW_T00009row("SWORKTIMETTL") = T0007COM.formatHHMM(WW_T00009row("SWORKTIMETTL"))
                            WW_T00009row("SNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("SNIGHTTIMECHO"))
                            WW_T00009row("SNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("SNIGHTTIMETTL"))
                            WW_T00009row("HWORKTIMECHO") = T0007COM.formatHHMM(WW_T00009row("HWORKTIMECHO"))
                            WW_T00009row("HWORKTIMETTL") = T0007COM.formatHHMM(WW_T00009row("HWORKTIMETTL"))
                            WW_T00009row("HNIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("HNIGHTTIMECHO"))
                            WW_T00009row("HNIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("HNIGHTTIMETTL"))
                            WW_T00009row("HOANTIMECHO") = T0007COM.formatHHMM(WW_T00009row("HOANTIMECHO"))
                            WW_T00009row("HOANTIMETTL") = T0007COM.formatHHMM(WW_T00009row("HOANTIMETTL"))
                            WW_T00009row("KOATUTIMECHO") = T0007COM.formatHHMM(WW_T00009row("KOATUTIMECHO"))
                            WW_T00009row("KOATUTIMETTL") = T0007COM.formatHHMM(WW_T00009row("KOATUTIMETTL"))
                            WW_T00009row("TOKUSA1TIMECHO") = T0007COM.formatHHMM(WW_T00009row("TOKUSA1TIMECHO"))
                            WW_T00009row("TOKUSA1TIMETTL") = T0007COM.formatHHMM(WW_T00009row("TOKUSA1TIMETTL"))
                            WW_T00009row("HAYADETIMECHO") = T0007COM.formatHHMM(WW_T00009row("HAYADETIMECHO"))
                            WW_T00009row("HAYADETIMETTL") = T0007COM.formatHHMM(WW_T00009row("HAYADETIMETTL"))
                            WW_T00009row("JIKYUSHATIMECHO") = T0007COM.formatHHMM(WW_T00009row("JIKYUSHATIMECHO"))
                            WW_T00009row("JIKYUSHATIMETTL") = T0007COM.formatHHMM(WW_T00009row("JIKYUSHATIMETTL"))
                            WW_T00009row("HDAIWORKTIMECHO") = T0007COM.formatHHMM(WW_T00009row("HDAIWORKTIMECHO"))
                            WW_T00009row("HDAIWORKTIMETTL") = T0007COM.formatHHMM(WW_T00009row("HDAIWORKTIMETTL"))
                            WW_T00009row("HDAINIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("HDAINIGHTTIMECHO"))
                            WW_T00009row("HDAINIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("HDAINIGHTTIMETTL"))
                            WW_T00009row("SDAIWORKTIMECHO") = T0007COM.formatHHMM(WW_T00009row("SDAIWORKTIMECHO"))
                            WW_T00009row("SDAIWORKTIMETTL") = T0007COM.formatHHMM(WW_T00009row("SDAIWORKTIMETTL"))
                            WW_T00009row("SDAINIGHTTIMECHO") = T0007COM.formatHHMM(WW_T00009row("SDAINIGHTTIMECHO"))
                            WW_T00009row("SDAINIGHTTIMETTL") = T0007COM.formatHHMM(WW_T00009row("SDAINIGHTTIMETTL"))
                            WW_T00009row("WWORKTIMECHO") = T0007COM.formatHHMM(WW_T00009row("WWORKTIMECHO"))
                            WW_T00009row("WWORKTIMETTL") = T0007COM.formatHHMM(WW_T00009row("WWORKTIMETTL"))

                            '名称取得
                            CODENAME_get("CAMPCODE", WW_T00009row("CAMPCODE"), WW_T00009row("CAMPNAMES"), WW_DUMMY)
                            CODENAME_get("STAFFKBN", WW_T00009row("STAFFKBN"), WW_T00009row("STAFFKBNNAMES"), WW_DUMMY)
                            CODENAME_get("ORG", WW_T00009row("MORG"), WW_T00009row("MORGNAMES"), WW_DUMMY)
                            CODENAME_get("HORG", WW_T00009row("HORG"), WW_T00009row("HORGNAMES"), WW_DUMMY)
                            CODENAME_get("HOLIDAYKBN", WW_T00009row("HOLIDAYKBN"), WW_T00009row("HOLIDAYKBNNAMES"), WW_DUMMY)
                            CODENAME_get("PAYKBN", WW_T00009row("PAYKBN"), WW_T00009row("PAYKBNNAMES"), WW_DUMMY)
                            CODENAME_get("SHUKCHOKKBN", WW_T00009row("SHUKCHOKKBN"), WW_T00009row("SHUKCHOKKBNNAMES"), WW_DUMMY)
                            CODENAME_get("RIYU", WW_T00009row("RIYU"), WW_T00009row("RIYUNAMES"), WW_DUMMY)

                            WW_T00009tbl.Rows.Add(WW_T00009row)
                        End If

                        If T00009row("HDKBN") = "D" Then
                            WW_UPD_FLG = True
                            Dim WW_T00009row As DataRow = WW_T00009tbl.NewRow
                            WW_T00009row.ItemArray = T00009row.ItemArray

                            WW_T00009row("TIMSTP") = 0
                            
                            '車両区分 (SHARYOKBN 1:単車、2:トレーラ)
                            '給与油種区分 (OILPAYKBN 01:一般、02:潤滑油、03:ＬＰＧ、04:ＬＮＧ、05:コンテナ、06:酸素、07:窒素、08:ﾒﾀｰﾉｰﾙ、09:ﾗﾃｯｸｽ、10:水素)
                            'UNLOADCNTTTL0101～UNLOADCNTTTL0110変数名を動的に作成
                            'UNLOADCNTTTL0201～UNLOADCNTTTL0210変数名を動的に作成
                            'HAIDISTANCETTL0101～HAIDISTANCETTL0110変数名を動的に作成
                            'HAIDISTANCETTL0201～HAIDISTANCETTL0210変数名を動的に作成
                            Dim WW_SHARYOKBN As String = Val(T00009row("SHARYOKBN")).ToString("00")
                            Dim WW_OILPAYKBN As String = T00009row("OILPAYKBN")
                            If (WW_SHARYOKBN = "01" OrElse WW_SHARYOKBN = "02") AndAlso
                                WW_OILPAYKBN >= "01" AndAlso WW_OILPAYKBN <= "10" Then
                                Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & CInt(T00009row("SHARYOKBN")).ToString("00") & T00009row("OILPAYKBN")
                                Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & CInt(T00009row("SHARYOKBN")).ToString("00") & T00009row("OILPAYKBN")
                                If T00009INProw(WW_UNLOADCNTTTL) <> T00009row("UNLOADCNTTTL") OrElse
                                   T00009INProw(WW_HAIDISTANCETTL) <> T00009row("HAIDISTANCETTL") Then
                                    WW_T00009row("UNLOADCNTCHO") = Val(T00009INProw(WW_UNLOADCNTTTL)) - T00009row("UNLOADCNT")
                                    WW_T00009row("HAIDISTANCECHO") = Val(T00009INProw(WW_HAIDISTANCETTL)) - T00009row("HAIDISTANCE")
                                End If
                            End If

                            WW_T00009row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            WW_T00009row("UNLOADCNTTTL") = Val(WW_T00009row("UNLOADCNT")) + Val(WW_T00009row("UNLOADCNTCHO"))
                            WW_T00009row("HAIDISTANCETTL") = Val(WW_T00009row("HAIDISTANCE")) + Val(WW_T00009row("HAIDISTANCECHO"))
                            WW_T00009tbl.Rows.Add(WW_T00009row)

                            WW_UNLOADCNT += WW_T00009row("UNLOADCNT")
                            WW_UNLOADCNTCHO += WW_T00009row("UNLOADCNTCHO")
                            WW_HAIDISTANCE += WW_T00009row("HAIDISTANCE")
                            WW_HAIDISTANCECHO += WW_T00009row("HAIDISTANCECHO")
                        End If
                    End If
                Next

                If WW_UPD_FLG Then
                    For Each WW_T00009row As DataRow In WW_T00009tbl.Rows
                        If WW_T00009row("WORKDATE") = T00009INProw("WORKDATE") AndAlso
                            WW_T00009row("STAFFCODE") = T00009INProw("STAFFCODE") AndAlso
                            WW_T00009row("RECODEKBN") = "2" AndAlso
                            WW_T00009row("HDKBN") = "H" Then
                            WW_T00009row("UNLOADCNT") = WW_UNLOADCNT
                            WW_T00009row("UNLOADCNTCHO") = WW_UNLOADCNTCHO
                            WW_T00009row("UNLOADCNTTTL") = WW_UNLOADCNT + WW_UNLOADCNTCHO
                            WW_T00009row("HAIDISTANCE") = WW_HAIDISTANCE
                            WW_T00009row("HAIDISTANCECHO") = WW_HAIDISTANCECHO
                            WW_T00009row("HAIDISTANCETTL") = WW_HAIDISTANCE + WW_HAIDISTANCECHO
                        End If
                    Next
                End If
            End If
        Next

        '○ 日別は、日報をマージし残業計算を行う
        If WW_HANTEI = "日別" Then
            T00009INPtbl.Merge(WW_T00009tbl)

            Dim WW_T00009NXTtbl As DataTable = T00009tbl.Clone()
            Dim WW_DATE_NEXT As Date = CDate(work.WF_SEL_TAISHOYM.Text & "/01").AddMonths(1)

            '正常データのみ抽出し、残業計算を行う
            '翌月1日のデータを抽出し残業計算を行う
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
            CS0026TBLSORT.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and WORKDATE = #" & WW_DATE_NEXT.ToString("yyyy/MM/dd") & "#"
            CS0026TBLSORT.sort(WW_T00009NXTtbl)
            WW_T00009NXTtbl.Merge(T00009INPtbl)

            '各会社毎に計算を行う
            'エネックス
            If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_ENEX Then
                T0007COM.T0007_KintaiCalc(T00009INPtbl, WW_T00009NXTtbl)
            End If

            '近石
            If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_KNK Then
                T0007COM.T0007_KintaiCalc_KNK(T00009INPtbl, WW_T00009NXTtbl)
            End If

            'ニュージェイズ
            If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_NJS Then
                T0007COM.T0007_KintaiCalc_NJS(T00009INPtbl, WW_T00009NXTtbl)
            End If
            
            'JKトランス
            If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_JKT Then
                T0007COM.T0007_KintaiCalc_JKT(T00009INPtbl, WW_T00009NXTtbl)
            End If
            
            If Not IsNothing(WW_T00009NXTtbl) Then
                WW_T00009NXTtbl.Clear()
                WW_T00009NXTtbl.Dispose()
                WW_T00009NXTtbl = Nothing
            End If

            For Each T00009INProw As DataRow In T00009INPtbl.Rows
                T00009INProw("TIMSTP") = 0
                If T00009INProw("HDKBN") = "D" Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If

                If T00009INProw("HDKBN") = "H" Then
                    '時間外計算対象外を判定し、対象外の場合は深夜のみ設定する
                    CODENAME_get("STAFFKBN2", T00009INProw("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T00009INProw("ORVERTIME") = "00:00"         '平日残業時間
                        T00009INProw("SWORKTIME") = "00:00"         '日曜出勤時間
                        T00009INProw("HWORKTIME") = "00:00"         '休日出勤時間
                        T00009INProw("HAYADETIME") = "00:00"        '早出補填時間
                        T00009INProw("HDAIWORKTIME") = "00:00"      '代休出勤時間
                        T00009INProw("SDAIWORKTIME") = "00:00"      '日曜代休出勤時間
                    End If

                    '入力補助(退社時間を対処予定時刻にコピー)
                    '平日で残業あり(所定労働時間＜拘束時間－休憩)。但し、従業員区分が時間外対象外ならチェックしない(残業申請しないため)
                    CODENAME_get("STAFFKBN3", T00009INProw("STAFFKBN"), T00009INProw("STAFFKBNTAISHOGAI"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) AndAlso T00009INProw("HOLIDAYKBN") = "0" Then
                        '時間外計算対象外を判定し、退社予定時刻が未入力ならば退社時刻をコピー
                        If T0007COM.HHMMtoMinutes(T00009INProw("BINDTIME")) < T0007COM.HHMMtoMinutes(T00009INProw("WORKTIME")) - T0007COM.HHMMtoMinutes(T00009INProw("BREAKTIME")) Then
                            If T00009INProw("YENDTIME") = "00:00" OrElse T00009INProw("YENDTIME") = "" Then
                                T00009INProw("YENDTIME") = T00009INProw("ENDTIME")
                            End If
                        End If
                    End If

                    '時間外計算残業申請対象外を判定し、対象外の場合は"00:00"を設定する
                    CODENAME_get("STAFFKBN4", T00009INProw("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        T00009INProw("ORVERTIME") = "00:00"         '平日残業時間
                        T00009INProw("SWORKTIME") = "00:00"         '日曜出勤時間
                        T00009INProw("HWORKTIME") = "00:00"         '休日出勤時間
                        T00009INProw("HAYADETIME") = "00:00"        '早出補填時間
                        T00009INProw("HDAIWORKTIME") = "00:00"      '代休出勤時間
                        T00009INProw("SDAIWORKTIME") = "00:00"      '日曜代休出勤時間
                        T00009INProw("NIGHTTIME") = "00:00"         '所定深夜時間
                        T00009INProw("WNIGHTTIME") = "00:00"        '平日深夜時間
                        T00009INProw("SNIGHTTIME") = "00:00"        '日曜深夜時間
                        T00009INProw("HNIGHTTIME") = "00:00"        '休日深夜時間
                        T00009INProw("HDAINIGHTTIME") = "00:00"     '代休深夜時間
                        T00009INProw("SDAINIGHTTIME") = "00:00"     '日曜代休深夜時間
                    End If

                End If

                '申請チェックボックス
                If T00009INProw("RIYU") = "" AndAlso T00009INProw("RIYUETC") = "" Then
                    T00009INProw("ENTRYFLG") = "0"
                Else
                    T00009INProw("ENTRYFLG") = "1"
                End If
            Next
        End If

        '○ テーブルソート
        CS0026TBLSORT.TABLE = T00009INPtbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00009INPtbl)

        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00009tbl)

        Dim WW_INDEX As Integer = 0
        Dim WW_KEY_INP As String = ""
        Dim WW_KEY_TBL As String = ""

        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            WW_KEY_INP = T00009INProw("STAFFCODE") & T00009INProw("WORKDATE") & T00009INProw("RECODEKBN")

            If T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso T00009INProw("HDKBN") = "H" Then
                For i As Integer = WW_INDEX To T00009tbl.Rows.Count - 1
                    Dim T00009row As DataRow = T00009tbl.Rows(i)
                    WW_KEY_TBL = T00009row("STAFFCODE") & T00009row("WORKDATE") & T00009row("RECODEKBN")

                    If WW_KEY_TBL < WW_KEY_INP Then
                        Continue For
                    End If

                    If WW_KEY_TBL = WW_KEY_INP Then
                        If T00009row("STATUS") = "02" Then
                            '申請中の場合、入力不可（入力データを捨てる）
                            T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            T00009INProw("SELECT") = 0
                            T00009INProw("HIDDEN") = 1
                            T00009INProw("DELFLG") = C_DELETE_FLG.DELETE
                        Else
                            '申請中以外（取下げ、承認、否認）の場合、入力可（入力データを有効にする）
                            T00009row("OPERATION") = T00009INProw("OPERATION")
                            T00009row("SELECT") = 0
                            T00009row("HIDDEN") = 1
                            T00009row("DELFLG") = C_DELETE_FLG.DELETE
                        End If
                    End If

                    If WW_KEY_TBL > WW_KEY_INP Then
                        WW_INDEX = i
                        Exit For
                    End If
                Next
            End If
        Next

        '○ 当画面で生成したデータ(タイムスタンプ = 0)に対する変更は、変更前を削除する
        For i As Integer = T00009tbl.Rows.Count - 1 To 0 Step -1
            If T00009tbl.Rows(i)("TIMSTP") = 0 AndAlso
                T00009tbl.Rows(i)("SELECT") = 0 Then
                T00009tbl.Rows(i).Delete()
            End If
        Next

        '○ 残業申請中を、入力(EXCELデータから除く)上記処理でSELECT=0
        For i As Integer = T00009INPtbl.Rows.Count - 1 To 0 Step -1
            If T00009INPtbl.Rows(i)("SELECT") = 0  Then
                T00009INPtbl.Rows(i).Delete()
            End If
        Next

        '○ 合計明細編集
        If WW_HANTEI = "月合計" Then
            T00009INPtbl = WW_T00009tbl.Copy()
        End If

        T00009tbl.Merge(T00009INPtbl)

        '○ 合計レコード編集
        If WW_HANTEI = "日別" Then
            T0007COM.T0007_TotalRecodeCreate(T00009tbl)
        Else
            T0007COM.T0007_TotalRecodeEdit(T00009tbl)
        End If

        '○ T0007COM.T0007_TotalRecodeEdit処理を行うと合計行の"更新"が消えてしまうため、再設定
        If WW_HANTEI = "月合計" Then
            CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN"
            CS0026TBLSORT.FILTER = ""

            CS0026TBLSORT.TABLE = T00009INPtbl
            CS0026TBLSORT.sort(T00009INPtbl)
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.sort(T00009tbl)

            WW_INDEX = 0
            WW_KEY_INP = ""
            WW_KEY_TBL = ""
            For Each T00009INProw As DataRow In T00009INPtbl.Rows
                WW_KEY_INP = T00009INProw("STAFFCODE") & T00009INProw("WORKDATE") & T00009INProw("RECODEKBN")

                If T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso T00009INProw("HDKBN") = "H" Then
                    For i As Integer = WW_INDEX To T00009tbl.Rows.Count - 1
                        Dim T00009row As DataRow = T00009tbl.Rows(i)
                        WW_KEY_TBL = T00009row("STAFFCODE") & T00009row("WORKDATE") & T00009row("RECODEKBN")

                        If WW_KEY_TBL < WW_KEY_INP Then
                            Continue For
                        End If

                        If WW_KEY_TBL = WW_KEY_INP Then
                            T00009row("OPERATION") = T00009INProw("OPERATION")
                        End If

                        If WW_KEY_TBL > WW_KEY_INP Then
                            WW_INDEX = i
                            Exit For
                        End If
                    Next
                End If
            Next
        End If

        '○ 月調整レコード作成
        T0007COM.T0007_ChoseiRecodeCreate(T00009tbl)

        '○ テーブルソート
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.sort(T00009tbl)

        Dim WW_LINECNT As Integer = 0
        Dim WW_SAVEKEY As String = ""
        For Each T00009row As DataRow In T00009tbl.Rows
            Dim WW_KEY As String = T00009row("CAMPCODE") & "," & T00009row("HORG") & "," & T00009row("STAFFCODE")
            If WW_SAVEKEY <> WW_KEY Then
                WW_LINECNT = 0
                WW_SAVEKEY = WW_KEY
            End If

            If T00009row("TAISHOYM") = work.WF_SEL_TAISHOYM.Text AndAlso T00009row("SELECT") = 1 Then
                If T00009row("HDKBN") = "H" AndAlso T00009row("DELFLG") = C_DELETE_FLG.ALIVE Then
                    If T00009row("RECODEKBN") = "0" Then
                        WW_LINECNT += 1
                        T00009row("LINECNT") = WW_LINECNT
                    End If
                    T00009row("SELECT") = 1
                    T00009row("HIDDEN") = 0
                Else
                    T00009row("SELECT") = 1
                    T00009row("HIDDEN") = 1
                    T00009row("LINECNT") = 0
                End If
            End If
        Next

        '○ 画面表示用データ
        CS0026TBLSORT.TABLE = T00009tbl
        CS0026TBLSORT.SORTING = "STAFFCODE, WORKDATE"
        CS0026TBLSORT.FILTER = "STAFFCODE = '" & WF_STAFFCODE.Text & "' and SELECT = 1 and DELFLG = '" & C_DELETE_FLG.ALIVE & "'"
        CS0026TBLSORT.sort(T00009INPtbl)

        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' インポートデータを取得
    ''' </summary>
    ''' <param name="I_TABLE"></param>
    ''' <param name="I_HANTEI"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelInpMake(ByVal I_TABLE As DataTable, ByVal I_HANTEI As String)

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each TBLcol As DataColumn In I_TABLE.Columns
            WW_COLUMNS.Add(TBLcol.ColumnName.ToString())
        Next

        Dim WW_ROW As DataRow = I_TABLE.NewRow
        For Each TBLrow As DataRow In I_TABLE.Rows
            WW_ROW.ItemArray = TBLrow.ItemArray

            For Each TBLcol As DataColumn In I_TABLE.Columns
                If IsDBNull(WW_ROW.Item(TBLcol)) OrElse IsNothing(WW_ROW.Item(TBLcol)) Then
                    WW_ROW.Item(TBLcol) = ""
                End If
            Next

            TBLrow.ItemArray = WW_ROW.ItemArray
        Next

        For Each TBLrow As DataRow In I_TABLE.Rows
            Dim T00009INProw As DataRow = T00009INPtbl.NewRow

            '初期クリア
            For Each T00009INPcol As DataColumn In T00009INPtbl.Columns
                If IsDBNull(T00009INProw.Item(T00009INPcol)) OrElse IsNothing(T00009INProw.Item(T00009INPcol)) Then
                    Select Case T00009INPcol.ColumnName
                        Case "LINECNT"
                            T00009INProw.Item(T00009INPcol) = 0
                        Case "OPERATION"
                            T00009INProw.Item(T00009INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            T00009INProw.Item(T00009INPcol) = 0
                        Case "SELECT"
                            T00009INProw.Item(T00009INPcol) = 1
                        Case "HIDDEN"
                            T00009INProw.Item(T00009INPcol) = 0
                        Case Else
                            T00009INProw.Item(T00009INPcol) = ""
                    End Select
                End If
            Next
            T0007COM.INProw_Init(work.WF_SEL_CAMPCODE.Text, T00009INProw)

            '共通項目
            T00009INProw("LINECNT") = 0
            T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            T00009INProw("TIMSTP") = 0
            T00009INProw("SELECT") = 1
            T00009INProw("HIDDEN") = 0

            T00009INProw("SEQ") = 0
            T00009INProw("EXTRACTCNT") = "0"
            T00009INProw("HDKBN") = "H"
            T00009INProw("DATAKBN") = "K"

            If I_HANTEI = "月合計" Then
                T00009INProw("RECODEKBN") = "2"
            Else
                T00009INProw("RECODEKBN") = "0"
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                T00009INProw("CAMPCODE") = TBLrow("CAMPCODE")
            Else
                T00009INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
            End If

            '対象年月
            If WW_COLUMNS.IndexOf("TAISHOYM") >= 0 Then
                T00009INProw("TAISHOYM") = TBLrow("TAISHOYM")
            Else
                T00009INProw("TAISHOYM") = ""
            End If

            '従業員コード
            If WW_COLUMNS.IndexOf("STAFFCODE") >= 0 Then
                T00009INProw("STAFFCODE") = TBLrow("STAFFCODE")
            Else
                T00009INProw("STAFFCODE") = ""
            End If

            '勤務年月日
            If WW_COLUMNS.IndexOf("WORKDATE") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(TBLrow("WORKDATE"), WW_DATE)
                    T00009INProw("WORKDATE") = WW_DATE.ToString("yyyy/MM/dd")
                    T00009INProw("WORKDAY") = WW_DATE.ToString("dd")
                Catch ex As Exception
                    T00009INProw("WORKDATE") = ""
                End Try
            Else
                T00009INProw("WORKDATE") = ""
            End If

            'レコード区分
            If WW_COLUMNS.IndexOf("RECODEKBN") >= 0 Then
                T00009INProw("RECODEKBN") = TBLrow("RECODEKBN")
            Else
                T00009INProw("RECODEKBN") = "0"
            End If

            '管理部署
            If WW_COLUMNS.IndexOf("MORG") >= 0 Then
                T00009INProw("MORG") = TBLrow("MORG")
            Else
                T00009INProw("MORG") = ""
            End If

            '配属部署
            If WW_COLUMNS.IndexOf("HORG") >= 0 Then
                T00009INProw("HORG") = TBLrow("HORG")
            Else
                T00009INProw("HORG") = ""
            End If

            '社員区分
            If WW_COLUMNS.IndexOf("STAFFKBN") >= 0 Then
                T00009INProw("STAFFKBN") = TBLrow("STAFFKBN")
            Else
                T00009INProw("STAFFKBN") = ""
            End If

            '曜日
            If WW_COLUMNS.IndexOf("WORKINGWEEK") >= 0 Then
                T00009INProw("WORKINGWEEK") = TBLrow("WORKINGWEEK")
            Else
                T00009INProw("WORKINGWEEK") = ""
            End If

            '休日区分
            If WW_COLUMNS.IndexOf("HOLIDAYKBN") >= 0 Then
                T00009INProw("HOLIDAYKBN") = TBLrow("HOLIDAYKBN")
            Else
                T00009INProw("HOLIDAYKBN") = ""
            End If

            '勤怠区分
            If WW_COLUMNS.IndexOf("PAYKBN") >= 0 Then
                T00009INProw("PAYKBN") = TBLrow("PAYKBN")
            Else
                T00009INProw("PAYKBN") = "00"
            End If

            '宿日直区分
            If WW_COLUMNS.IndexOf("SHUKCHOKKBN") >= 0 Then
                T00009INProw("SHUKCHOKKBN") = TBLrow("SHUKCHOKKBN")
            Else
                T00009INProw("SHUKCHOKKBN") = "0"
            End If

            '開始日
            If WW_COLUMNS.IndexOf("STDATE") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(TBLrow("STDATE"), WW_DATE)
                    T00009INProw("STDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                    T00009INProw("STDATE") = ""
                End Try
            Else
                T00009INProw("STDATE") = T00009INProw("WORKDATE")
            End If

            '開始時刻
            If WW_COLUMNS.IndexOf("STTIME") >= 0 Then
                Dim WW_TIME As Date
                Try
                    Date.TryParse(TBLrow("STTIME"), WW_TIME)
                    T00009INProw("STTIME") = WW_TIME.ToString("HH:mm")
                Catch ex As Exception
                    T00009INProw("STTIME") = ""
                End Try
            Else
                T00009INProw("STTIME") = "00:00"
            End If

            '終了日
            If WW_COLUMNS.IndexOf("ENDDATE") >= 0 Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(TBLrow("ENDDATE"), WW_DATE)
                    T00009INProw("ENDDATE") = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                    T00009INProw("ENDDATE") = ""
                End Try
            Else
                T00009INProw("ENDDATE") = T00009INProw("WORKDATE")
            End If

            '終了時刻
            If WW_COLUMNS.IndexOf("ENDTIME") >= 0 Then
                Dim WW_TIME As Date
                Try
                    Date.TryParse(TBLrow("ENDTIME"), WW_TIME)
                    T00009INProw("ENDTIME") = WW_TIME.ToString("HH:mm")
                Catch ex As Exception
                    T00009INProw("ENDTIME") = ""
                End Try
            Else
                T00009INProw("ENDTIME") = "00:00"
            End If

            '稼働時間
            If WW_COLUMNS.IndexOf("ACTTIME") >= 0 Then
                T00009INProw("ACTTIME") = TBLrow("ACTTIME")
            Else
                T00009INProw("ACTTIME") = "00:00"
            End If

            '拘束開始時刻
            If WW_COLUMNS.IndexOf("BINDSTDATE") >= 0 Then
                T00009INProw("BINDSTDATE") = TBLrow("BINDSTDATE")
            Else
                T00009INProw("BINDSTDATE") = "00:00"
            End If

            '拘束時間(分)
            If WW_COLUMNS.IndexOf("BINDTIME") >= 0 Then
                T00009INProw("BINDTIME") = TBLrow("BINDTIME")
            Else
                T00009INProw("BINDTIME") = "00:00"
            End If

            '休憩時間(分)
            If WW_COLUMNS.IndexOf("BREAKTIME") >= 0 Then
                T00009INProw("BREAKTIME") = TBLrow("BREAKTIME")
            Else
                T00009INProw("BREAKTIME") = "00:00"
            End If

            '休憩時間(分)合計
            If WW_COLUMNS.IndexOf("BREAKTIMETTL") >= 0 Then
                T00009INProw("BREAKTIMETTL") = TBLrow("BREAKTIMETTL")
            Else
                T00009INProw("BREAKTIMETTL") = "00:00"
            End If

            '所定深夜時間(分)
            If WW_COLUMNS.IndexOf("NIGHTTIME") >= 0 Then
                T00009INProw("NIGHTTIME") = TBLrow("NIGHTTIME")
            Else
                T00009INProw("NIGHTTIME") = "00:00"
            End If

            '所定深夜時間(分)合計
            If WW_COLUMNS.IndexOf("NIGHTTIMETTL") >= 0 Then
                T00009INProw("NIGHTTIMETTL") = TBLrow("NIGHTTIMETTL")
            Else
                T00009INProw("NIGHTTIMETTL") = "00:00"
            End If

            '平日残業時間(分)
            If WW_COLUMNS.IndexOf("ORVERTIME") >= 0 Then
                T00009INProw("ORVERTIME") = TBLrow("ORVERTIME")
            Else
                T00009INProw("ORVERTIME") = "00:00"
            End If

            '平日残業時間(分)合計
            If WW_COLUMNS.IndexOf("ORVERTIMETTL") >= 0 Then
                T00009INProw("ORVERTIMETTL") = TBLrow("ORVERTIMETTL")
            Else
                T00009INProw("ORVERTIMETTL") = "00:00"
            End If

            '平日深夜時間(分)
            If WW_COLUMNS.IndexOf("WNIGHTTIME") >= 0 Then
                T00009INProw("WNIGHTTIME") = TBLrow("WNIGHTTIME")
            Else
                T00009INProw("WNIGHTTIME") = "00:00"
            End If

            '平日深夜時間(分)合計
            If WW_COLUMNS.IndexOf("WNIGHTTIMETTL") >= 0 Then
                T00009INProw("WNIGHTTIMETTL") = TBLrow("WNIGHTTIMETTL")
            Else
                T00009INProw("WNIGHTTIMETTL") = "00:00"
            End If

            '日曜出勤時間(分)
            If WW_COLUMNS.IndexOf("SWORKTIME") >= 0 Then
                T00009INProw("SWORKTIME") = TBLrow("SWORKTIME")
            Else
                T00009INProw("SWORKTIME") = "00:00"
            End If

            '日曜出勤時間(分)合計
            If WW_COLUMNS.IndexOf("SWORKTIMETTL") >= 0 Then
                T00009INProw("SWORKTIMETTL") = TBLrow("SWORKTIMETTL")
            Else
                T00009INProw("SWORKTIMETTL") = "00:00"
            End If

            '日曜深夜時間(分)
            If WW_COLUMNS.IndexOf("SNIGHTTIME") >= 0 Then
                T00009INProw("SNIGHTTIME") = TBLrow("SNIGHTTIME")
            Else
                T00009INProw("SNIGHTTIME") = "00:00"
            End If

            '日曜深夜時間(分)合計
            If WW_COLUMNS.IndexOf("SNIGHTTIMETTL") >= 0 Then
                T00009INProw("SNIGHTTIMETTL") = TBLrow("SNIGHTTIMETTL")
            Else
                T00009INProw("SNIGHTTIMETTL") = "00:00"
            End If

            '休日出勤時間(分)
            If WW_COLUMNS.IndexOf("HWORKTIME") >= 0 Then
                T00009INProw("HWORKTIME") = TBLrow("HWORKTIME")
            Else
                T00009INProw("HWORKTIME") = "00:00"
            End If

            '休日出勤時間(分)合計
            If WW_COLUMNS.IndexOf("HWORKTIMETTL") >= 0 Then
                T00009INProw("HWORKTIMETTL") = TBLrow("HWORKTIMETTL")
            Else
                T00009INProw("HWORKTIMETTL") = "00:00"
            End If

            '休日深夜時間(分)
            If WW_COLUMNS.IndexOf("HNIGHTTIME") >= 0 Then
                T00009INProw("HNIGHTTIME") = TBLrow("HNIGHTTIME")
            Else
                T00009INProw("HNIGHTTIME") = "00:00"
            End If

            '休日深夜時間(分)合計
            If WW_COLUMNS.IndexOf("HNIGHTTIMETTL") >= 0 Then
                T00009INProw("HNIGHTTIMETTL") = TBLrow("HNIGHTTIMETTL")
            Else
                T00009INProw("HNIGHTTIMETTL") = "0"
            End If

            '所労合計
            If WW_COLUMNS.IndexOf("WORKNISSUTTL") >= 0 Then
                T00009INProw("WORKNISSUTTL") = TBLrow("WORKNISSUTTL")
            Else
                T00009INProw("WORKNISSUTTL") = "0"
            End If

            '傷欠合計
            If WW_COLUMNS.IndexOf("SHOUKETUNISSUTTL") >= 0 Then
                T00009INProw("SHOUKETUNISSUTTL") = TBLrow("SHOUKETUNISSUTTL")
            Else
                T00009INProw("SHOUKETUNISSUTTL") = "0"
            End If

            '組欠合計
            If WW_COLUMNS.IndexOf("KUMIKETUNISSUTTL") >= 0 Then
                T00009INProw("KUMIKETUNISSUTTL") = TBLrow("KUMIKETUNISSUTTL")
            Else
                T00009INProw("KUMIKETUNISSUTTL") = "0"
            End If

            '他欠合計
            If WW_COLUMNS.IndexOf("ETCKETUNISSUTTL") >= 0 Then
                T00009INProw("ETCKETUNISSUTTL") = TBLrow("ETCKETUNISSUTTL")
            Else
                T00009INProw("ETCKETUNISSUTTL") = "0"
            End If

            '年休合計
            If WW_COLUMNS.IndexOf("NENKYUNISSUTTL") >= 0 Then
                T00009INProw("NENKYUNISSUTTL") = TBLrow("NENKYUNISSUTTL")
            Else
                T00009INProw("NENKYUNISSUTTL") = "0"
            End If

            '特休合計
            If WW_COLUMNS.IndexOf("TOKUKYUNISSUTTL") >= 0 Then
                T00009INProw("TOKUKYUNISSUTTL") = TBLrow("TOKUKYUNISSUTTL")
            Else
                T00009INProw("TOKUKYUNISSUTTL") = "0"
            End If

            '遅早合計
            If WW_COLUMNS.IndexOf("CHIKOKSOTAINISSUTTL") >= 0 Then
                T00009INProw("CHIKOKSOTAINISSUTTL") = TBLrow("CHIKOKSOTAINISSUTTL")
            Else
                T00009INProw("CHIKOKSOTAINISSUTTL") = "0"
            End If

            'ストック休暇合計
            If WW_COLUMNS.IndexOf("STOCKNISSUTTL") >= 0 Then
                T00009INProw("STOCKNISSUTTL") = TBLrow("STOCKNISSUTTL")
            Else
                T00009INProw("STOCKNISSUTTL") = "0"
            End If

            '協定週休合計
            If WW_COLUMNS.IndexOf("KYOTEIWEEKNISSUTTL") >= 0 Then
                T00009INProw("KYOTEIWEEKNISSUTTL") = TBLrow("KYOTEIWEEKNISSUTTL")
            Else
                T00009INProw("KYOTEIWEEKNISSUTTL") = "0"
            End If

            '週休合計
            If WW_COLUMNS.IndexOf("WEEKNISSUTTL") >= 0 Then
                T00009INProw("WEEKNISSUTTL") = TBLrow("WEEKNISSUTTL")
            Else
                T00009INProw("WEEKNISSUTTL") = "0"
            End If

            '代休合計
            If WW_COLUMNS.IndexOf("DAIKYUNISSUTTL") >= 0 Then
                T00009INProw("DAIKYUNISSUTTL") = TBLrow("DAIKYUNISSUTTL")
            Else
                T00009INProw("DAIKYUNISSUTTL") = "0"
            End If

            '年始出勤合計
            If WW_COLUMNS.IndexOf("NENSHINISSUTTL") >= 0 Then
                T00009INProw("NENSHINISSUTTL") = TBLrow("NENSHINISSUTTL")
            Else
                T00009INProw("NENSHINISSUTTL") = "0"
            End If

            '宿日直年始合計
            If WW_COLUMNS.IndexOf("SHUKCHOKNNISSUTTL") >= 0 Then
                T00009INProw("SHUKCHOKNNISSUTTL") = TBLrow("SHUKCHOKNNISSUTTL")
            Else
                T00009INProw("SHUKCHOKNNISSUTTL") = "0"
            End If

            '宿日直通常合計
            If WW_COLUMNS.IndexOf("SHUKCHOKNISSUTTL") >= 0 Then
                T00009INProw("SHUKCHOKNISSUTTL") = TBLrow("SHUKCHOKNISSUTTL")
            Else
                T00009INProw("SHUKCHOKNISSUTTL") = "0"
            End If

            '宿日直年始(翌日休み)合計
            If WW_COLUMNS.IndexOf("SHUKCHOKNHLDNISSUTTL") >= 0 Then
                T00009INProw("SHUKCHOKNHLDNISSUTTL") = TBLrow("SHUKCHOKNHLDNISSUTTL")
            Else
                T00009INProw("SHUKCHOKNHLDNISSUTTL") = "0"
            End If

            '宿日直通常(翌日休み)合計
            If WW_COLUMNS.IndexOf("SHUKCHOKHLDNISSUTTL") >= 0 Then
                T00009INProw("SHUKCHOKHLDNISSUTTL") = TBLrow("SHUKCHOKHLDNISSUTTL")
            Else
                T00009INProw("SHUKCHOKHLDNISSUTTL") = "0"
            End If

            '特作A
            If WW_COLUMNS.IndexOf("TOKSAAKAISU") >= 0 Then
                T00009INProw("TOKSAAKAISU") = TBLrow("TOKSAAKAISU")
            Else
                T00009INProw("TOKSAAKAISU") = "0"
            End If

            '特作A合計
            If WW_COLUMNS.IndexOf("TOKSAAKAISUTTL") >= 0 Then
                T00009INProw("TOKSAAKAISUTTL") = TBLrow("TOKSAAKAISUTTL")
            Else
                T00009INProw("TOKSAAKAISUTTL") = "0"
            End If

            '特作B
            If WW_COLUMNS.IndexOf("TOKSABKAISU") >= 0 Then
                T00009INProw("TOKSABKAISU") = TBLrow("TOKSABKAISU")
            Else
                T00009INProw("TOKSABKAISU") = "0"
            End If

            '特作B合計
            If WW_COLUMNS.IndexOf("TOKSABKAISUTTL") >= 0 Then
                T00009INProw("TOKSABKAISUTTL") = TBLrow("TOKSABKAISUTTL")
            Else
                T00009INProw("TOKSABKAISUTTL") = "0"
            End If

            '特作C
            If WW_COLUMNS.IndexOf("TOKSACKAISU") >= 0 Then
                T00009INProw("TOKSACKAISU") = TBLrow("TOKSACKAISU")
            Else
                T00009INProw("TOKSACKAISU") = "0"
            End If

            '特作C合計
            If WW_COLUMNS.IndexOf("TOKSACKAISUTTL") >= 0 Then
                T00009INProw("TOKSACKAISUTTL") = TBLrow("TOKSACKAISUTTL")
            Else
                T00009INProw("TOKSACKAISUTTL") = "0"
            End If

            '点呼手当
            If WW_COLUMNS.IndexOf("TENKOKAISU") >= 0 Then
                T00009INProw("TENKOKAISU") = TBLrow("TENKOKAISU")
            Else
                T00009INProw("TENKOKAISU") = "0"
            End If

            '点呼手当合計
            If WW_COLUMNS.IndexOf("TENKOKAISUTTL") >= 0 Then
                T00009INProw("TENKOKAISUTTL") = TBLrow("TENKOKAISUTTL")
            Else
                T00009INProw("TENKOKAISUTTL") = "0"
            End If

            '保安検査(分)
            If WW_COLUMNS.IndexOf("HOANTIME") >= 0 Then
                T00009INProw("HOANTIME") = TBLrow("HOANTIME")
            Else
                T00009INProw("HOANTIME") = "00:00"
            End If

            '保安検査(分)合計
            If WW_COLUMNS.IndexOf("HOANTIMETTL") >= 0 Then
                T00009INProw("HOANTIMETTL") = TBLrow("HOANTIMETTL")
            Else
                T00009INProw("HOANTIMETTL") = "00:00"
            End If

            '高圧作業時間(分)
            If WW_COLUMNS.IndexOf("KOATUTIME") >= 0 Then
                T00009INProw("KOATUTIME") = TBLrow("KOATUTIME")
            Else
                T00009INProw("KOATUTIME") = "00:00"
            End If

            '高圧作業時間(分)合計
            If WW_COLUMNS.IndexOf("KOATUTIMETTL") >= 0 Then
                T00009INProw("KOATUTIMETTL") = TBLrow("KOATUTIMETTL")
            Else
                T00009INProw("KOATUTIMETTL") = "00:00"
            End If

            '特作Ⅰ(分)
            If WW_COLUMNS.IndexOf("TOKUSA1TIME") >= 0 Then
                T00009INProw("TOKUSA1TIME") = TBLrow("TOKUSA1TIME")
            Else
                T00009INProw("TOKUSA1TIME") = "00:00"
            End If

            '特作Ⅰ(分)合計
            If WW_COLUMNS.IndexOf("TOKUSA1TIMETTL") >= 0 Then
                T00009INProw("TOKUSA1TIMETTL") = TBLrow("TOKUSA1TIMETTL")
            Else
                T00009INProw("TOKUSA1TIMETTL") = "00:00"
            End If

            '早出補填(分)
            If WW_COLUMNS.IndexOf("HAYADETIME") >= 0 Then
                T00009INProw("HAYADETIME") = TBLrow("HAYADETIME")
            Else
                T00009INProw("HAYADETIME") = "00:00"
            End If

            '早出補填(分)合計
            If WW_COLUMNS.IndexOf("HAYADETIMETTL") >= 0 Then
                T00009INProw("HAYADETIMETTL") = TBLrow("HAYADETIMETTL")
            Else
                T00009INProw("HAYADETIMETTL") = "00:00"
            End If

            'ポンプ合計
            If WW_COLUMNS.IndexOf("PONPNISSUTTL") >= 0 Then
                T00009INProw("PONPNISSUTTL") = TBLrow("PONPNISSUTTL")
            Else
                T00009INProw("PONPNISSUTTL") = "0"
            End If

            'バルク合計
            If WW_COLUMNS.IndexOf("BULKNISSUTTL") >= 0 Then
                T00009INProw("BULKNISSUTTL") = TBLrow("BULKNISSUTTL")
            Else
                T00009INProw("BULKNISSUTTL") = "0"
            End If

            'トレーラ合計
            If WW_COLUMNS.IndexOf("TRAILERNISSUTTL") >= 0 Then
                T00009INProw("TRAILERNISSUTTL") = TBLrow("TRAILERNISSUTTL")
            Else
                T00009INProw("TRAILERNISSUTTL") = "0"
            End If

            'B勤務合計
            If WW_COLUMNS.IndexOf("BKINMUKAISUTTL") >= 0 Then
                T00009INProw("BKINMUKAISUTTL") = TBLrow("BKINMUKAISUTTL")
            Else
                T00009INProw("BKINMUKAISUTTL") = "0"
            End If

            '荷卸回数
            If WW_COLUMNS.IndexOf("UNLOADCNT") >= 0 Then
                T00009INProw("UNLOADCNT") = TBLrow("UNLOADCNT")
            Else
                T00009INProw("UNLOADCNT") = "0"
            End If

            '配送距離
            If WW_COLUMNS.IndexOf("HAIDISTANCE") >= 0 Then
                T00009INProw("HAIDISTANCE") = TBLrow("HAIDISTANCE")
            Else
                T00009INProw("HAIDISTANCE") = "0"
            End If

            '荷卸回数合計、配送距離合計
            For WW_SHARYOKBN As Integer = 1 To 2
                For WW_OILPAYKBN As Integer = 1 To 10
                    Dim WW_UNLOADCNT As String = "UNLOADCNTTTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SHARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")

                    '荷卸回数合計
                    If WW_COLUMNS.IndexOf(WW_UNLOADCNT) >= 0 Then
                        T00009INProw(WW_UNLOADCNT) = TBLrow(WW_UNLOADCNT)
                    Else
                        T00009INProw(WW_UNLOADCNT) = "0"
                    End If

                    '配送距離合計
                    If WW_COLUMNS.IndexOf(WW_HAIDISTANCETTL) >= 0 Then
                        T00009INProw(WW_HAIDISTANCETTL) = TBLrow(WW_HAIDISTANCETTL)
                    Else
                        T00009INProw(WW_HAIDISTANCETTL) = "0"
                    End If
                Next
            Next

            '回送作業距離
            If WW_COLUMNS.IndexOf("KAIDISTANCE") >= 0 Then
                T00009INProw("KAIDISTANCE") = TBLrow("KAIDISTANCE")
            Else
                T00009INProw("KAIDISTANCE") = "0"
            End If

            '退社予定時刻
            If WW_COLUMNS.IndexOf("YENDTIME") >= 0 Then
                Dim WW_TIME As Date
                Try
                    Date.TryParse(TBLrow("YENDTIME"), WW_TIME)
                    T00009INProw("YENDTIME") = WW_TIME.ToString("HH:mm")
                Catch ex As Exception
                    T00009INProw("YENDTIME") = ""
                End Try
            Else
                T00009INProw("YENDTIME") = "00:00"
            End If

            '理由
            If WW_COLUMNS.IndexOf("RIYU") >= 0 Then
                T00009INProw("RIYU") = TBLrow("RIYU")
            Else
                T00009INProw("RIYU") = ""
            End If

            '理由(その他)
            If WW_COLUMNS.IndexOf("RIYUETC") >= 0 Then
                T00009INProw("RIYUETC") = TBLrow("RIYUETC")
            Else
                T00009INProw("RIYUETC") = ""
            End If

            '年末出勤日数合計
            If WW_COLUMNS.IndexOf("NENMATUNISSUTTL") >= 0 Then
                T00009INProw("NENMATUNISSUTTL") = TBLrow("NENMATUNISSUTTL")
            Else
                T00009INProw("NENMATUNISSUTTL") = "0"
            End If

            '時給者時間合計
            If WW_COLUMNS.IndexOf("JIKYUSHATIMETTL") >= 0 Then
                T00009INProw("JIKYUSHATIMETTL") = TBLrow("JIKYUSHATIMETTL")
            Else
                T00009INProw("JIKYUSHATIMETTL") = "00:00"
            End If

            '代休出勤合計
            If WW_COLUMNS.IndexOf("HDAIWORKTIMETTL") >= 0 Then
                T00009INProw("HDAIWORKTIMETTL") = TBLrow("HDAIWORKTIMETTL")
            Else
                T00009INProw("HDAIWORKTIMETTL") = "00:00"
            End If

            '代休深夜合計
            If WW_COLUMNS.IndexOf("HDAINIGHTTIMETTL") >= 0 Then
                T00009INProw("HDAINIGHTTIMETTL") = TBLrow("HDAINIGHTTIMETTL")
            Else
                T00009INProw("HDAINIGHTTIMETTL") = "00:00"
            End If

            '日曜代休出勤合計
            If WW_COLUMNS.IndexOf("SDAIWORKTIMETTL") >= 0 Then
                T00009INProw("SDAIWORKTIMETTL") = TBLrow("SDAIWORKTIMETTL")
            Else
                T00009INProw("SDAIWORKTIMETTL") = "00:00"
            End If

            '日曜代休深夜合計
            If WW_COLUMNS.IndexOf("SDAINIGHTTIMETTL") >= 0 Then
                T00009INProw("SDAINIGHTTIMETTL") = TBLrow("SDAINIGHTTIMETTL")
            Else
                T00009INProw("SDAINIGHTTIMETTL") = "00:00"
            End If

            '所定内時間合計
            If WW_COLUMNS.IndexOf("WWORKTIMETTL") >= 0 Then
                T00009INProw("WWORKTIMETTL") = TBLrow("WWORKTIMETTL")
            Else
                T00009INProw("WWORKTIMETTL") = "00:00"
            End If

            '休日出勤日数合計
            If WW_COLUMNS.IndexOf("HWORKNISSUTTL") >= 0 Then
                T00009INProw("HWORKNISSUTTL") = TBLrow("HWORKNISSUTTL")
            Else
                T00009INProw("HWORKNISSUTTL") = "0"
            End If

            T00009INPtbl.Rows.Add(T00009INProw)
        Next

    End Sub


    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="I_CHANGED"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(Optional ByVal I_CHANGED As String = "")

        Dim WW_WORKDATE As Date
        rightview.setErrorReport("")
        Dim timef As New GRT00009TIMEFORMAT

        '○ 変更箇所の日付を取得
        Try
            Date.TryParse(Convert.ToString(Request.Form("txt" & pnlListArea.ID & "WORKDATE" & WF_SelectedIndex.Value)), WW_WORKDATE)
        Catch ex As Exception
            Exit Sub
        End Try

        Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)

        '○ 画面項目チェック
        For Each T00009INProw As DataRow In T00009INPtbl.Rows
            If T00009INProw("HDKBN") <> "H" OrElse
                T00009INProw("RECODEKBN") <> "0" OrElse
                T00009INProw("SELECT") <> 1 OrElse
                T00009INProw("STAFFCODE") <> WF_STAFFCODE.Text OrElse
                T00009INProw("WORKDATE") <> WW_WORKDATE.ToString("yyyy/MM/dd") Then
                Continue For
            End If

            '変更内容取得(入力禁止文字除外)
            '勤怠区分
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "PAYKBN" & WF_SelectedIndex.Value)) AndAlso I_CHANGED <> "PAYKBN" Then
                If T00009INProw("PAYKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "PAYKBN" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("PAYKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "PAYKBN" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("PAYKBN"))
            CODENAME_get("PAYKBN", T00009INProw("PAYKBN"), T00009INProw("PAYKBNNAMES"), WW_DUMMY)

            '宿日直区分
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHUKCHOKKBN" & WF_SelectedIndex.Value)) AndAlso I_CHANGED <> "SHUKCHOKKBN" Then
                If T00009INProw("SHUKCHOKKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKCHOKKBN" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("SHUKCHOKKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHUKCHOKKBN" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("SHUKCHOKKBN"))
            CODENAME_get("SHUKCHOKKBN", T00009INProw("SHUKCHOKKBN"), T00009INProw("SHUKCHOKKBNNAMES"), WW_DUMMY)

            '出社時刻
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "STTIME" & WF_SelectedIndex.Value)) Then
                If T00009INProw("STTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STTIME" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("STTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "STTIME" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("STTIME"))
            T00009INProw("STTIME") = timef.formatHHMM(T00009INProw("STTIME"))

            '退社日
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ENDDATE" & WF_SelectedIndex.Value)) Then
                If T00009INProw("ENDDATE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ENDDATE" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("ENDDATE") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ENDDATE" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("ENDDATE"))

            '退社時刻
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ENDTIME" & WF_SelectedIndex.Value)) Then
                If T00009INProw("ENDTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ENDTIME" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("ENDTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ENDTIME" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("ENDTIME"))
            T00009INProw("ENDTIME") = timef.formatHHMM(T00009INProw("ENDTIME"))

            '拘束開始
            Dim WW_BINDSTDATE As String = T00009INProw("BINDSTDATE")
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "BINDSTDATE" & WF_SelectedIndex.Value)) Then
                If T00009INProw("BINDSTDATE") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BINDSTDATE" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                WW_BINDSTDATE = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BINDSTDATE" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(WW_BINDSTDATE)

            If T00009INProw("STTIME") <> "00:00" AndAlso
                (WW_BINDSTDATE = "00:00" OrElse WW_BINDSTDATE = "") Then
                '出社時刻が入力されていて、拘束開始時刻が未入力の場合、出社時刻=拘束開始時刻(初期値)とする
                '但し5時前の場合は5時を設定
                If IsDate(T00009INProw("STTIME")) Then

                    If Not IsNothing(specialOrg.Items.FindByValue(T00009INProw("HORG"))) Then
                        '新潟東港の場合
                        If CDate(T00009INProw("STTIME")).ToString("HHmm") < "0500" Then
                            T00009INProw("BINDSTDATE") = "05:00"
                        Else
                            T00009INProw("BINDSTDATE") = T00009INProw("STTIME")
                        End If
                    Else
                        '新潟東港以外の場合
                        T00009INProw("BINDSTDATE") = T00009INProw("STTIME")
                    End If
                Else
                    T00009INProw("BINDSTDATE") = WW_BINDSTDATE
                End If
            Else
                '出社時刻が入力されて、拘束開始時刻が入力済なら拘束開始時刻をそのままにする
                T00009INProw("BINDSTDATE") = WW_BINDSTDATE
            End If

            '所定拘束
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "BINDTIME" & WF_SelectedIndex.Value)) Then
                If T00009INProw("BINDTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BINDTIME" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("BINDTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BINDTIME" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("BINDTIME"))

            '休憩時間
            Dim WW_BREAKTIME As String = T00009INProw("BREAKTIME")
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "BREAKTIME" & WF_SelectedIndex.Value)) Then
                If T00009INProw("BREAKTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BREAKTIME" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                WW_BREAKTIME = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "BREAKTIME" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(WW_BREAKTIME)

            If T00009INProw("STTIME") <> "00:00" AndAlso T00009INProw("ENDTIME") <> "00:00" Then
                '休憩の初期表示
                If IsDate(T00009INProw("STDATE")) AndAlso IsDate(T00009INProw("STTIME")) AndAlso
                    IsDate(T00009INProw("ENDDATE")) AndAlso IsDate(T00009INProw("ENDTIME")) Then
                    Dim WW_DATE_ST As Date = CDate(CDate(T00009INProw("STDATE")).ToString("yyyy/MM/dd") & " " & T00009INProw("STTIME"))
                    Dim WW_DATE_END As Date = CDate(CDate(T00009INProw("ENDDATE")).ToString("yyyy/MM/dd") & " " & T00009INProw("ENDTIME"))
                    Dim WW_TIME As Integer = 0

                    If DateDiff("n", WW_DATE_ST, WW_DATE_END) <= 360 Then
                        WW_TIME = 0
                    ElseIf DateDiff("n", WW_DATE_ST, WW_DATE_END) <= 480 Then
                        WW_TIME = 45
                    Else
                        WW_TIME = 60
                    End If

                    If WW_BREAKTIME <> T0007COM.formatHHMM(WW_TIME) Then
                        If WW_BREAKTIME = "00:00" OrElse WW_BREAKTIME = "" Then
                            '計算値
                            T00009INProw("BREAKTIME") = T0007COM.formatHHMM(WW_TIME)
                        Else
                            '入力値と計算値が違ったら入力値のまま
                            T00009INProw("BREAKTIME") = WW_BREAKTIME
                        End If
                    Else
                        '入力値と計算値が違ったら入力値のまま
                        T00009INProw("BREAKTIME") = WW_BREAKTIME
                    End If
                Else
                    '計算できない場合、入力値
                    T00009INProw("BREAKTIME") = WW_BREAKTIME
                End If
            Else
                '計算できない場合、入力値
                T00009INProw("BREAKTIME") = WW_BREAKTIME
            End If

            '普通休日(追加調整)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ORVERTIMEADD" & WF_SelectedIndex.Value)) Then
                If T00009INProw("ORVERTIMEADD") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ORVERTIMEADD" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("ORVERTIMEADD") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ORVERTIMEADD" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("ORVERTIMEADD"))

            '普通深夜(追加調整)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "WNIGHTTIMEADD" & WF_SelectedIndex.Value)) Then
                If T00009INProw("WNIGHTTIMEADD") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "WNIGHTTIMEADD" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("WNIGHTTIMEADD") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "WNIGHTTIMEADD" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("WNIGHTTIMEADD"))

            '日曜(追加調整)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SWORKTIMEADD" & WF_SelectedIndex.Value)) Then
                If T00009INProw("SWORKTIMEADD") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SWORKTIMEADD" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("SWORKTIMEADD") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SWORKTIMEADD" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("SWORKTIMEADD"))

            '日曜深夜(追加調整)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SNIGHTTIMEADD" & WF_SelectedIndex.Value)) Then
                If T00009INProw("SNIGHTTIMEADD") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SNIGHTTIMEADD" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("SNIGHTTIMEADD") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SNIGHTTIMEADD" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("SNIGHTTIMEADD"))

            '退社予定時刻
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YENDTIME" & WF_SelectedIndex.Value)) Then
                If T00009INProw("YENDTIME") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YENDTIME" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("YENDTIME") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YENDTIME" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("YENDTIME"))

            '残業理由
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "RIYU" & WF_SelectedIndex.Value)) AndAlso I_CHANGED <> "RIYU" Then
                If T00009INProw("RIYU") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "RIYU" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("RIYU") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "RIYU" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("RIYU"))
            CODENAME_get("RIYU", T00009INProw("RIYU"), T00009INProw("RIYUNAMES"), WW_DUMMY)

            '理由補足
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "RIYUETC" & WF_SelectedIndex.Value)) Then
                If T00009INProw("RIYUETC") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "RIYUETC" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                T00009INProw("RIYUETC") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "RIYUETC" & WF_SelectedIndex.Value))
            End If
            Master.eraseCharToIgnore(T00009INProw("RIYUETC"))

            '申請
            If Not IsNothing(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "ENTRYFLG" & WF_SelectedIndex.Value)) Then
                If T00009INProw("ENTRYFLG") <> Convert.ToString(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "ENTRYFLG" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                Dim WW_FLG As String = Convert.ToString(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "ENTRYFLG" & WF_SelectedIndex.Value))
                If WW_FLG = "on" OrElse WW_FLG = "1" Then
                    T00009INProw("ENTRYFLG") = "1"
                End If
            Else
                If T00009INProw("ENTRYFLG") = "1" Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    T00009INProw("ENTRYFLG") = "0"
                End If
            End If

            '取下げ
            If Not IsNothing(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "DRAWALFLG" & WF_SelectedIndex.Value)) Then
                If T00009INProw("DRAWALFLG") <> Convert.ToString(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "DRAWALFLG" & WF_SelectedIndex.Value)) Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                Dim WW_FLG As String = Convert.ToString(Request.Form("ctl00$contents1$chk" & pnlListArea.ID & "DRAWALFLG" & WF_SelectedIndex.Value))
                If WW_FLG = "on" OrElse WW_FLG = "1" Then
                    T00009INProw("DRAWALFLG") = "1"
                End If
            Else
                If T00009INProw("DRAWALFLG") = "1" Then
                    T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    T00009INProw("DRAWALFLG") = "0"
                End If
            End If

            If T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                T00009INProw("TIMSTP") = 0
            End If

            '項目チェック
            INPTableCheck(T00009INProw, WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Master.output(WW_ERR_SW, C_MESSAGE_TYPE.ABORT)
            Else
                '残業計算
                Dim WW_T00009tbl As DataTable = T00009INPtbl.Clone()
                Dim WW_T00009row As DataRow = WW_T00009tbl.NewRow
                WW_T00009row.ItemArray = T00009INProw.ItemArray
                WW_T00009tbl.Rows.Add(WW_T00009row)

                '各会社毎に計算を行う
                'エネックス
                If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_ENEX Then
                    T0007COM.T0007_KintaiCalc(WW_T00009tbl, T00009tbl)
                End If

                '近石
                If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_KNK Then
                    T0007COM.T0007_KintaiCalc_KNK(WW_T00009tbl, T00009tbl)
                End If

                'ニュージェイズ
                If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_NJS Then
                    T0007COM.T0007_KintaiCalc_NJS(WW_T00009tbl, T00009tbl)
                End If

                'JKトランス
                If work.WF_SEL_CAMPCODE.Text = GRT00009WRKINC.CAMP_JKT Then
                    T0007COM.T0007_KintaiCalc_JKT(WW_T00009tbl, T00009tbl)
                End If

                '時間外計算対象外を判定し、対象外の場合は深夜のみ設定する
                CODENAME_get("STAFFKBN2", WW_T00009row("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    For Each WW_ROW As DataRow In WW_T00009tbl.Rows
                        WW_ROW("ORVERTIME") = "00:00"         '平日残業時間
                        WW_ROW("SWORKTIME") = "00:00"         '日曜出勤時間
                        WW_ROW("HWORKTIME") = "00:00"         '休日出勤時間
                        WW_ROW("HAYADETIME") = "00:00"        '早出補填時間
                        WW_ROW("HDAIWORKTIME") = "00:00"      '代休出勤時間
                        WW_ROW("SDAIWORKTIME") = "00:00"      '日曜代休出勤時間
                    Next
                End If

                '入力補助(退社時間を対処予定時刻にコピー)
                '平日で残業あり(所定労働時間<拘束時間－休憩)。但し、従業員区分が時間外対象外ならチェックしない(残業申請しないため)
                CODENAME_get("STAFFKBN3", WW_T00009row("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    For Each WW_ROW As DataRow In WW_T00009tbl.Rows
                        If WW_ROW("HOLIDAYKBN") = "0" Then
                            '時間外計算対象外を判定し、退社予定時刻が未入力ならば退社時刻をコピー
                            If T0007COM.HHMMtoMinutes(WW_ROW("BINDTIME")) < T0007COM.HHMMtoMinutes(WW_ROW("WORKTIME")) - T0007COM.HHMMtoMinutes(WW_ROW("BREAKTIME")) Then
                                If WW_ROW("YENDTIME") = "00:00" Then
                                    WW_ROW("YENDTIME") = WW_ROW("ENDTIME")
                                End If
                            End If
                        End If
                    Next
                End If

                '時間外計算残業申請対象外を判定し、対象外の場合は"00:00"を設定する
                CODENAME_get("STAFFKBN4", WW_T00009row("STAFFKBN"), WW_DUMMY, WW_RTN_SW)
                If isNormal(WW_RTN_SW) Then
                    For Each WW_ROW As DataRow In WW_T00009tbl.Rows
                        WW_ROW("ORVERTIME") = "00:00"         '平日残業時間
                        WW_ROW("SWORKTIME") = "00:00"         '日曜出勤時間
                        WW_ROW("HWORKTIME") = "00:00"         '休日出勤時間
                        WW_ROW("HAYADETIME") = "00:00"        '早出補填時間
                        WW_ROW("HDAIWORKTIME") = "00:00"      '代休出勤時間
                        WW_ROW("SDAIWORKTIME") = "00:00"      '日曜代休出勤時間
                        WW_ROW("NIGHTTIME") = "00:00"         '所定深夜時間
                        WW_ROW("WNIGHTTIME") = "00:00"        '平日深夜時間
                        WW_ROW("SNIGHTTIME") = "00:00"        '日曜深夜時間
                        WW_ROW("HNIGHTTIME") = "00:00"        '休日深夜時間
                        WW_ROW("HDAINIGHTTIME") = "00:00"     '代休深夜時間
                        WW_ROW("SDAINIGHTTIME") = "00:00"     '日曜代休深夜時間
                    Next
                End If

                '入力データをINPtblに反映(削除してマージ)
                CS0026TBLSORT.TABLE = T00009INPtbl
                CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
                CS0026TBLSORT.FILTER = "STAFFCODE = '" & WW_T00009row("STAFFCODE") & "'" _
                    & " and (WORKDATE <> #" & WW_T00009row("WORKDATE") & "# or RECODEKBN = '2')"
                CS0026TBLSORT.sort(T00009INPtbl)
                T00009INPtbl.Merge(WW_T00009tbl)

                '合計レコード編集
                T0007COM.T0007_TotalRecodeCreate(T00009INPtbl)

                '月調整レコード作成
                T0007COM.T0007_ChoseiRecodeCreate(T00009INPtbl)
            End If

            '全体データにINPtblに反映(削除してマージ)
            CS0026TBLSORT.TABLE = T00009tbl
            CS0026TBLSORT.SORTING = "LINECNT, STAFFCODE, WORKDATE, RECODEKBN"
            CS0026TBLSORT.FILTER = "STAFFCODE <> '" & T00009INProw("STAFFCODE") & "'"
            CS0026TBLSORT.sort(T00009tbl)
            T00009tbl.Merge(T00009INPtbl)
            Exit For
        Next

        '○ 画面表示データ保存
        Master.SaveTable(T00009tbl, WF_XMLsaveF.Value)
        Master.SaveTable(T00009INPtbl, WF_XMLsaveF_INP.Value)

    End Sub


    ''' <summary>
    ''' 調整画面切り替え時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_DtabChange()

        '○ 勤怠ALLのみ処理可能
        If Not Master.MAPvariant Like GRT00009WRKINC.VAR_ALL Then
            Exit Sub
        End If

        '○ 月間調整画面設定
        DisplayAdjust()

        '○ 会社や部署に合わせて、画面表示を一部変更する
        Select Case work.WF_SEL_CAMPCODE.Text
            Case GRT00009WRKINC.CAMP_ENEX       'ENEX
                WF_NENMATUNISSUTTL_L.Visible = False            '年末出勤日非表示
                WF_NENMATUNISSUTTL.Visible = False
                WF_HWORKNISSUTTL_L.Visible = False              '休日出勤日非表示
                WF_HWORKNISSUTTL.Visible = False

                WF_SHUKCHOKNNISSUTTL_L.Text = "宿直年末年始（翌日勤務）"
                WF_SHUKCHOKNISSUTTL_L.Text = "宿直（翌日勤務）"
                WF_SHUKCHOKNHLDNISSUTTL_L.Text = "宿直年末年始（翌日休み）"
                WF_SHUKCHOKHLDNISSUTTL_L.Text = "宿直（翌日休み）"

                '新潟東港限定
                Dim specialOrg As ListBox = T0007COM.getList(work.WF_SEL_CAMPCODE.Text, GRT00007WRKINC.CONST_SPEC)
                If Not IsNothing(specialOrg.Items.FindByValue(work.WF_SEL_HORG.Text)) Then
                    WF_HAYADETIMETTL.Enabled = False
                    WF_SHUKCHOKNNISSUTTL_L.Text = "宿直年末年始（割増無し）"
                    WF_SHUKCHOKNISSUTTL_L.Text = "宿直（割増無し）"
                    WF_SHUKCHOKNHLDNISSUTTL_L.Text = "宿直年末年始（割増有り）"
                    WF_SHUKCHOKHLDNISSUTTL_L.Text = "宿直（割増有り）"
                End If

                WF_HDAI_L.Visible = False                       '代休出勤非表示
                WF_HDAIWORKTIMETTL.Visible = False
                WF_HDAINIGHTTIMETTL.Visible = False
                WF_SDAI_L.Visible = False                       '日曜代休非表示
                WF_SDAIWORKTIMETTL.Visible = False
                WF_SDAINIGHTTIMETTL.Visible = False
                WF_TOKUSA1TIMETTL_L.Visible = False             '特作Ⅰ非表示
                WF_TOKUSA1TIMETTL.Visible = False
                WF_WWORKTIMETTL_L.Visible = False               '所定内時間非表示
                WF_WWORKTIMETTL.Visible = False
                WF_JIKYUSHATIMETTL_L.Visible = False            '時給者時間非表示
                WF_JIKYUSHATIMETTL_L2.Visible = False
                WF_JIKYUSHATIMETTL.Visible = False

            Case GRT00009WRKINC.CAMP_KNK        '近石
                WF_NENMATUNISSUTTL_L.Visible = False            '年末出勤日非表示
                WF_NENMATUNISSUTTL.Visible = False
                WF_SHUKCHOKNNISSUTTL_L.Text = "宿日直年始"
                WF_SHUKCHOKNISSUTTL_L.Text = "宿日直通常"
                WF_SHUKCHOKNHLDNISSUTTL_L.Visible = False       '宿日直年始(翌日休み)非表示
                WF_SHUKCHOKNHLDNISSUTTL.Visible = False
                WF_SHUKCHOKHLDNISSUTTL_L.Visible = False        '宿日直通常(翌日休み)非表示
                WF_SHUKCHOKHLDNISSUTTL.Visible = False
                WF_HAYADETIMETTL_L.Visible = False              '早出補填非表示
                WF_HAYADETIMETTL.Visible = False
                WF_JIKYUSHATIMETTL_L.Visible = False            '時給者時間非表示
                WF_JIKYUSHATIMETTL_L2.Visible = False
                WF_JIKYUSHATIMETTL.Visible = False
                WF_TOKUSA1TIMETTL_L.Text = "特作Ⅰ"             '特作Ⅰ名称変更

            Case GRT00009WRKINC.CAMP_NJS        'ニュージェイズ
                WF_HWORKNISSUTTL_L.Visible = False              '休日出勤日非表示
                WF_HWORKNISSUTTL.Visible = False
                WF_SHUKCHOKNNISSUTTL_L.Visible = False          '宿日直年始非表示
                WF_SHUKCHOKNNISSUTTL.Visible = False
                WF_SHUKCHOKNISSUTTL_L.Visible = False           '宿日直通常非表示
                WF_SHUKCHOKNISSUTTL.Visible = False
                WF_SHUKCHOKNHLDNISSUTTL_L.Visible = False       '宿日直年始(翌日休み)非表示
                WF_SHUKCHOKNHLDNISSUTTL.Visible = False
                WF_SHUKCHOKHLDNISSUTTL_L.Visible = False        '宿日直通常(翌日休み)非表示
                WF_SHUKCHOKHLDNISSUTTL.Visible = False
                WF_HAYADETIMETTL_L.Visible = False              '早出補填非表示
                WF_HAYADETIMETTL.Visible = False
                WF_HDAI_L.Visible = False                       '代休出勤非表示
                WF_HDAIWORKTIMETTL.Visible = False
                WF_HDAINIGHTTIMETTL.Visible = False
                WF_SDAI_L.Visible = False                       '日曜代休非表示
                WF_SDAIWORKTIMETTL.Visible = False
                WF_SDAINIGHTTIMETTL.Visible = False
                WF_TOKUSA1TIMETTL_L.Text = "特作"               '特作Ⅰ名称変更
                WF_WWORKTIMETTL_L.Visible = False               '所定内時間非表示
                WF_WWORKTIMETTL.Visible = False
                WF_JIKYUSHATIMETTL_L.Text = "時給者作業"        '時給者時間名称変更
                WF_JIKYUSHATIMETTL_L2.Visible = False

            Case GRT00009WRKINC.CAMP_JKT        'JKトランス
                WF_WEEKNISSUTTL_L.Visible = False               '週休非表示
                WF_WEEKNISSUTTL.Visible = False
                WF_KUMIKETUNISSUTTL_L.Text = "休 業 日 数"
                WF_NENMATUNISSUTTL_L.Visible = False            '年末出勤日非表示
                WF_NENMATUNISSUTTL.Visible = False
                WF_HWORKNISSUTTL_L.Visible = False              '休日出勤日非表示
                WF_HWORKNISSUTTL.Visible = False
                WF_SHUKCHOKNNISSUTTL_L.Text = "宿日直年始"
                WF_SHUKCHOKNISSUTTL_L.Text = "宿日直通常"
                WF_SHUKCHOKNHLDNISSUTTL_L.Visible = False       '宿日直年始(翌日休み)非表示
                WF_SHUKCHOKNHLDNISSUTTL.Visible = False
                WF_SHUKCHOKHLDNISSUTTL_L.Visible = False        '宿日直通常(翌日休み)非表示
                WF_SHUKCHOKHLDNISSUTTL.Visible = False
                WF_HAYADETIMETTL_L.Visible = False              '早出補填非表示
                WF_HAYADETIMETTL.Visible = False
                WF_HDAI_L.Visible = False                       '代休出勤非表示
                WF_HDAIWORKTIMETTL.Visible = False
                WF_HDAINIGHTTIMETTL.Visible = False
                WF_SDAI_L.Visible = False                       '日曜代休非表示
                WF_SDAIWORKTIMETTL.Visible = False
                WF_SDAINIGHTTIMETTL.Visible = False
                WF_TOKUSA1TIMETTL_L.Visible = False             '特作Ⅰ非表示
                WF_TOKUSA1TIMETTL.Visible = False
                WF_WWORKTIMETTL_L.Visible = False               '所定内時間非表示
                WF_WWORKTIMETTL.Visible = False
                WF_JIKYUSHATIMETTL_L.Text = "時間給者"          '時給者時間名称変更
        End Select

        '○ 画面切替
        WF_DISP.Value = "Adjust"
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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

            With leftview
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                
                Select Case WF_FIELD.Value
                    Case "WF_SELSTAFFCODE"          '従業員コード
                        prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK, work.WF_SEL_CAMPCODE.Text,
                                    work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                    Case "PAYKBN"                   '勤怠区分
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "PAYKBN"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                    Case "SHUKCHOKKBN"              '宿日直区分
                        prmData = work.CreateShukchokKBNParam()
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                    Case "RIYU"                     '残業理由
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_RIYU"
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                End Select

                .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .activeListBox()
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

        Dim WW_WORKDATE As Date
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 変更箇所の日付を取得
        Try
            Date.TryParse(Convert.ToString(Request.Form("txt" & pnlListArea.ID & "WORKDATE" & WF_SelectedIndex.Value)), WW_WORKDATE)
        Catch ex As Exception
            WW_WORKDATE = CDate(C_DEFAULT_YMD)
        End Try

        '○ 選択内容を画面項目へセット
        If WF_FIELD.Value = "WF_SELSTAFFCODE" Then
            '従業員コード(絞込条件)
            WF_SELSTAFFCODE.Text = WW_SelectValue
            WF_SELSTAFFCODE_TEXT.Text = WW_SelectText
            WF_SELSTAFFCODE.Focus()
        Else
            For Each T00009INProw As DataRow In T00009INPtbl.Rows
                If T00009INProw("HDKBN") <> "H" OrElse
                    T00009INProw("RECODEKBN") <> "0" OrElse
                    T00009INProw("SELECT") <> 1 OrElse
                    T00009INProw("STAFFCODE") <> WF_STAFFCODE.Text OrElse
                    T00009INProw("WORKDATE") <> WW_WORKDATE.ToString("yyyy/MM/dd") Then
                    Continue For
                End If

                Select Case WF_FIELD.Value
                    Case "PAYKBN"               '勤怠区分
                        If T00009INProw("PAYKBN") <> WW_SelectValue Then
                            T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        End If
                        T00009INProw("PAYKBN") = WW_SelectValue
                        T00009INProw("PAYKBNNAMES") = WW_SelectText

                    Case "SHUKCHOKKBN"          '宿日直区分
                        If T00009INProw("SHUKCHOKKBN") <> WW_SelectValue Then
                            T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        End If
                        T00009INProw("SHUKCHOKKBN") = WW_SelectValue
                        T00009INProw("SHUKCHOKKBNNAMES") = WW_SelectText

                    Case "RIYU"                 '残業理由
                        If T00009INProw("RIYU") <> WW_SelectValue Then
                            T00009INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                        End If
                        T00009INProw("RIYU") = WW_SelectValue
                        T00009INProw("RIYUNAMES") = WW_SelectText
                End Select

                If WF_FIELD.Value <> "WF_SELSTAFFCODE" Then
                    WF_ListChange(WF_FIELD.Value)
                End If
            Next
        End If

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
            Case "WF_SELSTAFFCODE"          '従業員コード
                WF_SELSTAFFCODE.Focus()
            Case "PAYKBN"                   '勤怠区分
            Case "SHUKCHOKKBN"              '宿日直区分
            Case "RIYU"                     '残業理由
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

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.showHelp()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="T00009INProw"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef T00009INProw As DataRow, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0036FCHECKERR As String = ""
        Dim WW_CS0036FCHECKREPORT As String = ""
        Dim WW_WORKINGH As String = ""
        Dim WW_TIME As String() = {}
        Dim WW_S0013tbl As DataTable = New DataTable

        '○ 単項目チェック
        'レコード区分
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RECODEKBN", T00009INProw("RECODEKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("RECODEKBN", T00009INProw("RECODEKBN"), T00009INProw("RECODEKBNNAMES"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(レコード区分エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("RECODEKBN") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(レコード区分エラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '会社コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", T00009INProw("CAMPCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", T00009INProw("CAMPCODE"), T00009INProw("CAMPNAMES"), WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("CAMPCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '従業員コード
        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", T00009INProw("STAFFCODE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
        If isNormal(WW_CS0036FCHECKERR) Then
            '存在チェック
            CODENAME_get("STAFFCODE", T00009INProw("STAFFCODE"), T00009INProw("STAFFNAMES"), WW_RTN_SW)
            If isNormal(WW_RTN_SW) Then
                '従業員マスタから各情報取得
                MB001_Select(T00009INProw, WW_WORKINGH)
            Else
                WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
                WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("STAFFCODE") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(従業員コードエラー)です。"
            WW_CheckMES2 = WW_CS0036FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '日別情報
        If T00009INProw("RECODEKBN") = "0" Then
            T00009INProw("TAISHOYM") = WF_TAISHOYM.Text

            '勤務年月日
            If String.IsNullOrEmpty(T00009INProw("WORKDATE")) Then
                WW_CheckMES1 = "・更新できないレコード(勤務年月日無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKDATE", T00009INProw("WORKDATE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    'カレンダーマスタから各情報取得
                    MB005_Select(T00009INProw)

                    '対象年月チェック
                    If IsDate(T00009INProw("WORKDATE")) AndAlso
                        CDate(T00009INProw("WORKDATE")).ToString("yyyy/MM") <> WF_TAISHOYM.Text Then
                        WW_CheckMES1 = "・更新できないレコード(勤務年月日不正)です。"
                        WW_CheckMES2 = T00009INProw("WORKDATE")
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤務年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '休日区分
            If String.IsNullOrEmpty(T00009INProw("HOLIDAYKBN")) Then
                WW_CheckMES1 = "・更新できないレコード(休日区分無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HOLIDAYKBN", T00009INProw("HOLIDAYKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("HOLIDAYKBN", T00009INProw("HOLIDAYKBN"), T00009INProw("HOLIDAYKBNNAMES"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(休日区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("HOLIDAYKBN") & ")"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(休日区分エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '勤怠区分
            If String.IsNullOrEmpty(T00009INProw("PAYKBN")) Then
                WW_CheckMES1 = "・更新できないレコード(勤怠区分無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PAYKBN", T00009INProw("PAYKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("PAYKBN", T00009INProw("PAYKBN"), T00009INProw("PAYKBNNAMES"), WW_RTN_SW)
                    If isNormal(WW_RTN_SW) Then
                        '近石のみ勤怠区分特殊チェック
                        If T00009INProw("CAMPCODE") = GRT00009WRKINC.CAMP_KNK Then
                            Select Case T00009INProw("PAYKBN")
                                Case "10"       '代休出勤
                                    If T00009INProw("HOLIDAYKBN") = "0" Then
                                        WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                                        WW_CheckMES2 = "休日以外に代休出勤は出来ません。"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                    End If

                                Case "11"       '代休取得
                                    If T00009INProw("HOLIDAYKBN") <> "0" Then
                                        WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                                        WW_CheckMES2 = "休日に代休取得は出来ません。"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                    End If

                                Case "12"       '振替出勤
                                    If T00009INProw("HOLIDAYKBN") = "0" Then
                                        WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                                        WW_CheckMES2 = "休日以外に振替出勤は出来ません。"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                    End If

                                Case "13"       '振替取得
                                    If T00009INProw("HOLIDAYKBN") <> "0" Then
                                        WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                                        WW_CheckMES2 = "休日に振替取得は出来ません。"
                                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                    End If
                            End Select
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("PAYKBN") & ")"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤怠区分エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '宿日直区分
            If String.IsNullOrEmpty(T00009INProw("SHUKCHOKKBN")) Then
                WW_CheckMES1 = "・更新できないレコード(宿日直区分無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKCHOKKBN", T00009INProw("SHUKCHOKKBN"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("SHUKCHOKKBN", T00009INProw("SHUKCHOKKBN"), T00009INProw("SHUKCHOKKBNNAMES"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(宿日直区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("SHUKCHOKKBN") & ")"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(宿日直区分エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '開始日
            If String.IsNullOrEmpty(T00009INProw("STDATE")) Then
                T00009INProw("STDATE") = T00009INProw("WORKDATE")
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STDATE", T00009INProw("STDATE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If Not isNormal(WW_CS0036FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(出社日エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '開始時刻
            If String.IsNullOrEmpty(T00009INProw("STTIME")) Then
                T00009INProw("STTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STTIME", T00009INProw("STTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("STTIME") = CDate(T00009INProw("STTIME")).ToString("HH:mm")

                    '5分単位で入力
                    If T00009INProw("CAMPCODE") = GRT00009WRKINC.CAMP_ENEX OrElse
                        T00009INProw("CAMPCODE") = GRT00009WRKINC.CAMP_KNK Then
                        If Right(T00009INProw("STTIME"), 1) <> "0" AndAlso Right(T00009INProw("STTIME"), 1) <> "5" Then
                            WW_CheckMES1 = "・更新できないレコード(出社時刻エラー)です。"
                            WW_CheckMES2 = "５分単位で入力してください。(" & T00009INProw("STTIME") & ")"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(出社時刻エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '終了日
            If String.IsNullOrEmpty(T00009INProw("ENDDATE")) Then
                T00009INProw("ENDDATE") = T00009INProw("WORKDATE")
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDDATE", T00009INProw("ENDDATE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If Not isNormal(WW_CS0036FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(退社日エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '終了時刻
            If String.IsNullOrEmpty(T00009INProw("ENDTIME")) Then
                T00009INProw("ENDTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ENDTIME", T00009INProw("ENDTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("ENDTIME") = CDate(T00009INProw("ENDTIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(退社時刻エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '稼働時間
            If isNormal(O_RTN) Then
                Dim WW_DATE_ST As String = T00009INProw("STDATE") & " " & T00009INProw("STTIME")
                Dim WW_DATE_END As String = T00009INProw("ENDDATE") & " " & T00009INProw("ENDTIME")

                If IsDate(WW_DATE_ST) AndAlso IsDate(WW_DATE_END) Then
                    If DateDiff("n", WW_DATE_ST, WW_DATE_END) < 0 Then
                        T00009INProw("ACTTIME") = "00:00"
                        T00009INProw("WORKTIME") = "00:00"
                    Else
                        T00009INProw("ACTTIME") = T0007COM.formatHHMM(DateDiff("n", WW_DATE_ST, WW_DATE_END))
                        T00009INProw("WORKTIME") = T0007COM.formatHHMM(DateDiff("n", WW_DATE_ST, WW_DATE_END))
                    End If
                Else
                    T00009INProw("ACTTIME") = ""
                    T00009INProw("WORKTIME") = "00:00"
                End If
            End If

            '拘束開始時刻
            If String.IsNullOrEmpty(T00009INProw("BINDSTDATE")) Then
                If String.IsNullOrEmpty(T00009INProw("STTIME")) Then
                    T00009INProw("BINDSTDATE") = "00:00"
                Else
                    T00009INProw("BINDSTDATE") = T00009INProw("STTIME")
                End If
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "BINDSTDATE", T00009INProw("BINDSTDATE"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("BINDSTDATE") = CDate(T00009INProw("BINDSTDATE")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(拘束開始時刻エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '拘束時間
            If String.IsNullOrEmpty(T00009INProw("BINDTIME")) Then
                If T00009INProw("HOLIDAYKBN") = "0" Then
                    T00009INProw("BINDTIME") = WW_WORKINGH
                Else
                    T00009INProw("BINDTIME") = "00:00"
                End If
            End If

            Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "BINDTIME", T00009INProw("BINDTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
            If isNormal(WW_CS0036FCHECKERR) Then
                T00009INProw("BINDTIME") = CDate(T00009INProw("BINDTIME")).ToString("HH:mm")
            Else
                WW_CheckMES1 = "・更新できないレコード(拘束時間エラー)です。"
                WW_CheckMES2 = WW_CS0036FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '休憩時間
            If String.IsNullOrEmpty(T00009INProw("BREAKTIME")) Then
                T00009INProw("BREAKTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "BREAKTIME", T00009INProw("BREAKTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("BREAKTIME") = CDate(T00009INProw("BREAKTIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(休憩時間エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作A
            If String.IsNullOrEmpty(T00009INProw("TOKSAAKAISU")) Then
                T00009INProw("TOKSAAKAISU") = "0"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSAAKAISU", T00009INProw("TOKSAAKAISU"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSAAKAISU") = Val(T00009INProw("TOKSAAKAISU"))
                    T00009INProw("TOKSAAKAISUTTL") = Val(T00009INProw("TOKSAAKAISU"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ａエラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作B
            If String.IsNullOrEmpty(T00009INProw("TOKSABKAISU")) Then
                T00009INProw("TOKSABKAISU") = "0"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSABKAISU", T00009INProw("TOKSABKAISU"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSABKAISU") = Val(T00009INProw("TOKSABKAISU"))
                    T00009INProw("TOKSABKAISUTTL") = Val(T00009INProw("TOKSABKAISU"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ｂエラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作C
            If String.IsNullOrEmpty(T00009INProw("TOKSACKAISU")) Then
                T00009INProw("TOKSACKAISU") = "0"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSACKAISU", T00009INProw("TOKSACKAISU"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSACKAISU") = Val(T00009INProw("TOKSACKAISU"))
                    T00009INProw("TOKSACKAISUTTL") = Val(T00009INProw("TOKSACKAISU"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ｃエラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '保安検査
            If String.IsNullOrEmpty(T00009INProw("HOANTIME")) Then
                T00009INProw("HOANTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HOANTIME", T00009INProw("HOANTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("HOANTIME") = CDate(T00009INProw("HOANTIME")).ToString("HH:mm")
                    T00009INProw("HOANTIMETTL") = CDate(T00009INProw("HOANTIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '高圧作業時間
            If String.IsNullOrEmpty(T00009INProw("KOATUTIME")) Then
                T00009INProw("KOATUTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOATUTIME", T00009INProw("KOATUTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("KOATUTIME") = CDate(T00009INProw("KOATUTIME")).ToString("HH:mm")
                    T00009INProw("KOATUTIMETTL") = CDate(T00009INProw("KOATUTIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(高圧作業時間エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作Ⅰ
            If String.IsNullOrEmpty(T00009INProw("TOKUSA1TIME")) Then
                T00009INProw("TOKUSA1TIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKUSA1TIME", T00009INProw("TOKUSA1TIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKUSA1TIME") = CDate(T00009INProw("TOKUSA1TIME")).ToString("HH:mm")
                    T00009INProw("TOKUSA1TIMETTL") = CDate(T00009INProw("TOKUSA1TIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '平日残業時間(調整加算)
            If String.IsNullOrEmpty(T00009INProw("ORVERTIMEADD")) Then
                T00009INProw("ORVERTIMEADD") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ORVERTIMEADD", T00009INProw("ORVERTIMEADD"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("ORVERTIMEADD") = CDate(T00009INProw("ORVERTIMEADD")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(調整加算(普通・休日))エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '平日深夜時間(調整加算)
            If String.IsNullOrEmpty(T00009INProw("WNIGHTTIMEADD")) Then
                T00009INProw("WNIGHTTIMEADD") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WNIGHTTIMEADD", T00009INProw("WNIGHTTIMEADD"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("WNIGHTTIMEADD") = CDate(T00009INProw("WNIGHTTIMEADD")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(調整加算(普通・深夜))エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜出勤時間(調整加算)
            If String.IsNullOrEmpty(T00009INProw("SWORKTIMEADD")) Then
                T00009INProw("SWORKTIMEADD") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SWORKTIMEADD", T00009INProw("SWORKTIMEADD"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SWORKTIMEADD") = CDate(T00009INProw("SWORKTIMEADD")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(調整加算(日曜・普通))エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜深夜時間(調整加算)
            If String.IsNullOrEmpty(T00009INProw("SNIGHTTIMEADD")) Then
                T00009INProw("SNIGHTTIMEADD") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SNIGHTTIMEADD", T00009INProw("SNIGHTTIMEADD"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SNIGHTTIMEADD") = CDate(T00009INProw("SNIGHTTIMEADD")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(調整加算(日曜・深夜))エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '退社予定時刻
            If String.IsNullOrEmpty(T00009INProw("YENDTIME")) Then
                T00009INProw("YENDTIME") = "00:00"
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "YENDTIME", T00009INProw("YENDTIME"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("YENDTIME") = CDate(T00009INProw("YENDTIME")).ToString("HH:mm")
                Else
                    WW_CheckMES1 = "・更新できないレコード(退社予定時刻エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '理由
            If String.IsNullOrEmpty(T00009INProw("RIYU")) Then
                T00009INProw("RIYU") = ""
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RIYU", T00009INProw("RIYU"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("RIYU", T00009INProw("RIYU"), T00009INProw("RIYUNAMES"), WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(残業理由コードエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。(" & T00009INProw("RIYU") & ")"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(残業理由コードエラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '理由(その他)
            If String.IsNullOrEmpty(T00009INProw("RIYUETC")) Then
                T00009INProw("RIYUETC") = ""
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "RIYUETC", T00009INProw("RIYUETC"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If Not isNormal(WW_CS0036FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(残業理由補足エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If

        '月合計情報
        If T00009INProw("RECODEKBN") = "2" Then
            '対象年月
            If String.IsNullOrEmpty(T00009INProw("TAISHOYM")) Then
                WW_CheckMES1 = "・更新できないレコード(対象年月無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                If IsDate(T00009INProw("TAISHOYM") & "/01") Then
                    T00009INProw("TAISHOYM") = CDate(T00009INProw("TAISHOYM") & "/01").ToString("yyyy/MM")
                    If T00009INProw("TAISHOYM") <> WF_TAISHOYM.Text Then
                        WW_CheckMES1 = "・更新できないレコード(対象年月無不正)です。"
                        WW_CheckMES2 = T00009INProw("TAISHOYM")
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(対象年月無不正)です。"
                    WW_CheckMES2 = T00009INProw("TAISHOYM")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '所定深夜時間合計
            If String.IsNullOrEmpty(T00009INProw("NIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(所定深夜時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("NIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(所定深夜時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("NIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(所定深夜時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(所定深夜時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(所定深夜時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("NIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '平日残業時間合計
            If String.IsNullOrEmpty(T00009INProw("ORVERTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(平日残業時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("ORVERTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ORVERTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ORVERTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(平日残業時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("ORVERTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(平日残業時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(平日残業時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(平日残業時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("ORVERTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '平日深夜時間合計
            If String.IsNullOrEmpty(T00009INProw("WNIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(平日深夜時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("WNIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(平日深夜時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("WNIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(平日深夜時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(平日深夜時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(平日深夜時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("WNIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜出勤時間合計
            If String.IsNullOrEmpty(T00009INProw("SWORKTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(日曜出勤時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("SWORKTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(日曜出勤時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("SWORKTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(日曜出勤時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(日曜出勤時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(日曜出勤時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("SWORKTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜深夜時間合計
            If String.IsNullOrEmpty(T00009INProw("SNIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(日曜深夜時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("SNIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(日曜深夜時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("SNIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(日曜深夜時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(日曜深夜時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(日曜深夜時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("SNIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '休日出勤時間合計
            If String.IsNullOrEmpty(T00009INProw("HWORKTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(休日出勤時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HWORKTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(休日出勤時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("HWORKTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(休日出勤時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(休日出勤時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(休日出勤時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("HWORKTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '休日深夜時間合計
            If String.IsNullOrEmpty(T00009INProw("HNIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(休日深夜時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HNIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HNIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(休日深夜時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("HNIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(休日深夜時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(休日深夜時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(休日深夜時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("HNIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '所労合計
            If String.IsNullOrEmpty(T00009INProw("WORKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(所労日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WORKNISSUTTL", T00009INProw("WORKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("WORKNISSUTTL") = Val(T00009INProw("WORKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(所労日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '傷欠合計
            If String.IsNullOrEmpty(T00009INProw("SHOUKETUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(傷欠日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHOUKETUNISSUTTL", T00009INProw("SHOUKETUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SHOUKETUNISSUTTL") = Val(T00009INProw("SHOUKETUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(傷欠日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '組欠合計
            If String.IsNullOrEmpty(T00009INProw("KUMIKETUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(組欠日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KUMIKETUNISSUTTL", T00009INProw("KUMIKETUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("KUMIKETUNISSUTTL") = Val(T00009INProw("KUMIKETUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(組欠日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '他欠合計
            If String.IsNullOrEmpty(T00009INProw("ETCKETUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(他欠日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "ETCKETUNISSUTTL", T00009INProw("ETCKETUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("ETCKETUNISSUTTL") = Val(T00009INProw("ETCKETUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(他欠日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '年休合計
            If String.IsNullOrEmpty(T00009INProw("NENKYUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(年休日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NENKYUNISSUTTL", T00009INProw("NENKYUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("NENKYUNISSUTTL") = Val(T00009INProw("NENKYUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(年休日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特休合計
            If String.IsNullOrEmpty(T00009INProw("TOKUKYUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(特休日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKUKYUNISSUTTL", T00009INProw("TOKUKYUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKUKYUNISSUTTL") = Val(T00009INProw("TOKUKYUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特休日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '遅早合計
            If String.IsNullOrEmpty(T00009INProw("CHIKOKSOTAINISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(遅早日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "CHIKOKSOTAINISSUTTL", T00009INProw("CHIKOKSOTAINISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("CHIKOKSOTAINISSUTTL") = Val(T00009INProw("CHIKOKSOTAINISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(遅早日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'ストック休暇合計
            If String.IsNullOrEmpty(T00009INProw("STOCKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(ストック休暇日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "STOCKNISSUTTL", T00009INProw("STOCKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("STOCKNISSUTTL") = Val(T00009INProw("STOCKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(ストック休暇日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '協定週休合計
            If String.IsNullOrEmpty(T00009INProw("KYOTEIWEEKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(協定週休日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KYOTEIWEEKNISSUTTL", T00009INProw("KYOTEIWEEKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("KYOTEIWEEKNISSUTTL") = Val(T00009INProw("KYOTEIWEEKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(協定週休日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '週休合計
            If String.IsNullOrEmpty(T00009INProw("WEEKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(週休日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WEEKNISSUTTL", T00009INProw("WEEKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("WEEKNISSUTTL") = Val(T00009INProw("WEEKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(週休日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '代休合計
            If String.IsNullOrEmpty(T00009INProw("DAIKYUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(代休日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "DAIKYUNISSUTTL", T00009INProw("DAIKYUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("DAIKYUNISSUTTL") = Val(T00009INProw("DAIKYUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(代休日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '年始出勤合計
            If String.IsNullOrEmpty(T00009INProw("NENSHINISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(年始出勤日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NENSHINISSUTTL", T00009INProw("NENSHINISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("NENSHINISSUTTL") = Val(T00009INProw("NENSHINISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(年始出勤日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '宿日直年始合計
            If String.IsNullOrEmpty(T00009INProw("SHUKCHOKNNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(宿日直年始日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKCHOKNNISSUTTL", T00009INProw("SHUKCHOKNNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SHUKCHOKNNISSUTTL") = Val(T00009INProw("SHUKCHOKNNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(宿日直年始日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '宿日直通常合計
            If String.IsNullOrEmpty(T00009INProw("SHUKCHOKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(宿日直通常日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKCHOKNISSUTTL", T00009INProw("SHUKCHOKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SHUKCHOKNISSUTTL") = Val(T00009INProw("SHUKCHOKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(宿日直通常日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '宿日直年始(翌日休み)合計
            If String.IsNullOrEmpty(T00009INProw("SHUKCHOKNHLDNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(宿日直年始日数(翌休み)無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKCHOKNHLDNISSUTTL", T00009INProw("SHUKCHOKNHLDNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SHUKCHOKNHLDNISSUTTL") = Val(T00009INProw("SHUKCHOKNHLDNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(宿日直年始日数(翌休み)エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '宿日直通常(翌日休み)合計
            If String.IsNullOrEmpty(T00009INProw("SHUKCHOKHLDNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(宿日直通常日数(翌休み)無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SHUKCHOKHLDNISSUTTL", T00009INProw("SHUKCHOKHLDNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("SHUKCHOKHLDNISSUTTL") = Val(T00009INProw("SHUKCHOKHLDNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(宿日直通常日数(翌休み)エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作A合計
            If String.IsNullOrEmpty(T00009INProw("TOKSAAKAISUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(特作Ａ日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSAAKAISUTTL", T00009INProw("TOKSAAKAISUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSAAKAISUTTL") = Val(T00009INProw("TOKSAAKAISUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ａ日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作B合計
            If String.IsNullOrEmpty(T00009INProw("TOKSABKAISUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(特作Ｂ日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSABKAISUTTL", T00009INProw("TOKSABKAISUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSABKAISUTTL") = Val(T00009INProw("TOKSABKAISUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ｂ日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作C合計
            If String.IsNullOrEmpty(T00009INProw("TOKSACKAISUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(特作Ｃ日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKSACKAISUTTL", T00009INProw("TOKSACKAISUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TOKSACKAISUTTL") = Val(T00009INProw("TOKSACKAISUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ｃ日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '保安検査合計
            If String.IsNullOrEmpty(T00009INProw("HOANTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(保安検査無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HOANTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HOANTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HOANTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                                WW_CheckMES2 = T00009INProw("HOANTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(保安検査エラー)です。"
                    WW_CheckMES2 = T00009INProw("HOANTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '高圧作業時間合計
            If String.IsNullOrEmpty(T00009INProw("KOATUTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(高圧作業時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("KOATUTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOATUTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "KOATUTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(高圧作業時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("KOATUTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(高圧作業時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(高圧作業時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(高圧作業時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("KOATUTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '特作Ⅰ合計
            If String.IsNullOrEmpty(T00009INProw("TOKUSA1TIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(特作Ⅰ無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("TOKUSA1TIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKUSA1TIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TOKUSA1TIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                                WW_CheckMES2 = T00009INProw("TOKUSA1TIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(特作Ⅰエラー)です。"
                    WW_CheckMES2 = T00009INProw("TOKUSA1TIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '早出補填時間合計
            If String.IsNullOrEmpty(T00009INProw("HAYADETIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(早出補填時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HAYADETIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HAYADETIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HAYADETIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(早出補填時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("HAYADETIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(早出補填時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(早出補填時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(早出補填時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("HAYADETIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'ポンプ合計
            If String.IsNullOrEmpty(T00009INProw("PONPNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(ポンプ日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "PONPNISSUTTL", T00009INProw("PONPNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("PONPNISSUTTL") = Val(T00009INProw("PONPNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(ポンプ日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'バルク合計
            If String.IsNullOrEmpty(T00009INProw("BULKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(バルク日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "BULKNISSUTTL", T00009INProw("BULKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("BULKNISSUTTL") = Val(T00009INProw("BULKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(バルク日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'トレーラ合計
            If String.IsNullOrEmpty(T00009INProw("TRAILERNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(トレーラ日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "TRAILERNISSUTTL", T00009INProw("TRAILERNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("TRAILERNISSUTTL") = Val(T00009INProw("TRAILERNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(トレーラ日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'B勤務合計
            If String.IsNullOrEmpty(T00009INProw("BKINMUKAISUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(Ｂ勤務日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "BKINMUKAISUTTL", T00009INProw("BKINMUKAISUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("BKINMUKAISUTTL") = Val(T00009INProw("BKINMUKAISUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(Ｂ勤務日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            For WW_SYARYOKBN As Integer = 1 To 2
                For WW_OILPAYKBN As Integer = 1 To 10
                    Dim WW_SYARYO As String() = {"単車", "トレーラ"}
                    Dim WW_UNLOADCNTTTL As String = "UNLOADCNTTTL" & WW_SYARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")
                    Dim WW_HAIDISTANCETTL As String = "HAIDISTANCETTL" & WW_SYARYOKBN.ToString("00") & WW_OILPAYKBN.ToString("00")

                    '単車/トレーラー・荷卸回数01～10合計
                    If String.IsNullOrEmpty(T00009INProw(WW_UNLOADCNTTTL)) Then
                        WW_CheckMES1 = "・更新できないレコード(" & WW_SYARYO(WW_SYARYOKBN - 1) & "・荷卸回数" & WW_OILPAYKBN.ToString("00") & "無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, WW_UNLOADCNTTTL, T00009INProw(WW_UNLOADCNTTTL), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            T00009INProw(WW_UNLOADCNTTTL) = Val(T00009INProw(WW_UNLOADCNTTTL))
                        Else
                            WW_CheckMES1 = "・更新できないレコード(" & WW_SYARYO(WW_SYARYOKBN - 1) & "・荷卸回数" & WW_OILPAYKBN.ToString("00") & "エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If

                    '単車/トレーラー・配送距離01～10合計
                    If String.IsNullOrEmpty(T00009INProw(WW_HAIDISTANCETTL)) Then
                        WW_CheckMES1 = "・更新できないレコード(" & WW_SYARYO(WW_SYARYOKBN - 1) & "・配送距離" & WW_OILPAYKBN.ToString("00") & "無)です。"
                        WW_CheckMES2 = ""
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, WW_HAIDISTANCETTL, T00009INProw(WW_HAIDISTANCETTL), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            T00009INProw(WW_HAIDISTANCETTL) = Val(T00009INProw(WW_HAIDISTANCETTL))
                        Else
                            WW_CheckMES1 = "・更新できないレコード(" & WW_SYARYO(WW_SYARYOKBN - 1) & "・配送距離" & WW_OILPAYKBN.ToString("00") & "エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Next
            Next

            '年末出勤日数合計
            If String.IsNullOrEmpty(T00009INProw("NENMATUNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(年末出勤日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "NENMATUNISSUTTL", T00009INProw("NENMATUNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("NENMATUNISSUTTL") = Val(T00009INProw("NENMATUNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(年末出勤日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '時給者時間合計
            If String.IsNullOrEmpty(T00009INProw("JIKYUSHATIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(時給者時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("JIKYUSHATIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JIKYUSHATIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "JIKYUSHATIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(時給者時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("JIKYUSHATIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(時給者時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(時給者時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(時給者時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("JIKYUSHATIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '代休出勤合計
            If String.IsNullOrEmpty(T00009INProw("HDAIWORKTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(代休出勤無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HDAIWORKTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HDAIWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HDAIWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(代休出勤エラー)です。"
                                WW_CheckMES2 = T00009INProw("HDAIWORKTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(代休出勤エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(代休出勤エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(代休出勤エラー)です。"
                    WW_CheckMES2 = T00009INProw("HDAIWORKTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '代休深夜合計
            If String.IsNullOrEmpty(T00009INProw("HDAINIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(代休深夜無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("HDAINIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HDAINIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HDAINIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(代休深夜エラー)です。"
                                WW_CheckMES2 = T00009INProw("HDAINIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(代休深夜エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(代休深夜エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(代休深夜エラー)です。"
                    WW_CheckMES2 = T00009INProw("HDAINIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜代休出勤合計
            If String.IsNullOrEmpty(T00009INProw("SDAIWORKTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(日曜代休出勤無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("SDAIWORKTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SDAIWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SDAIWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(日曜代休出勤エラー)です。"
                                WW_CheckMES2 = T00009INProw("SDAIWORKTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(日曜代休出勤エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(日曜代休出勤エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(日曜代休出勤エラー)です。"
                    WW_CheckMES2 = T00009INProw("SDAIWORKTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '日曜代休深夜合計
            If String.IsNullOrEmpty(T00009INProw("SDAINIGHTTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(日曜代休深夜無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("SDAINIGHTTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SDAINIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "SDAINIGHTTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(日曜代休深夜エラー)です。"
                                WW_CheckMES2 = T00009INProw("SDAINIGHTTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(日曜代休深夜エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(日曜代休深夜エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(日曜代休深夜エラー)です。"
                    WW_CheckMES2 = T00009INProw("SDAINIGHTTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '所定内時間合計
            If String.IsNullOrEmpty(T00009INProw("WWORKTIMETTL")) Then
                WW_CheckMES1 = "・更新できないレコード(所定内時間無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_TIME = T00009INProw("WWORKTIMETTL").ToString().Split(":")
                If WW_TIME.Length = 2 Then
                    WW_TEXT = WW_TIME(0)
                    Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                    If isNormal(WW_CS0036FCHECKERR) Then
                        WW_TEXT = WW_TIME(1)
                        Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "WWORKTIMETTL", WW_TEXT, WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                        If isNormal(WW_CS0036FCHECKERR) Then
                            If Val(WW_TIME(1)) >= 60 Then
                                WW_CheckMES1 = "・更新できないレコード(所定内時間エラー)です。"
                                WW_CheckMES2 = T00009INProw("WWORKTIMETTL")
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・更新できないレコード(所定内時間エラー)です。"
                            WW_CheckMES2 = WW_CS0036FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(所定内時間エラー)です。"
                        WW_CheckMES2 = WW_CS0036FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(所定内時間エラー)です。"
                    WW_CheckMES2 = T00009INProw("WWORKTIMETTL")
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '休日出勤日数合計
            If String.IsNullOrEmpty(T00009INProw("HWORKNISSUTTL")) Then
                WW_CheckMES1 = "・更新できないレコード(休日出勤日数無)です。"
                WW_CheckMES2 = ""
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckFieldForTable(work.WF_SEL_CAMPCODE.Text, "HWORKNISSUTTL", T00009INProw("HWORKNISSUTTL"), WW_CS0036FCHECKERR, WW_CS0036FCHECKREPORT, WW_S0013tbl)
                If isNormal(WW_CS0036FCHECKERR) Then
                    T00009INProw("HWORKNISSUTTL") = Val(T00009INProw("HWORKNISSUTTL"))
                Else
                    WW_CheckMES1 = "・更新できないレコード(休日出勤日数エラー)です。"
                    WW_CheckMES2 = WW_CS0036FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, T00009INProw)
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="T00009row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal T00009row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(T00009row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社     =" & T00009row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員   =" & T00009row("STAFFCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 従業員名 =" & T00009row("STAFFNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 配属部署 =" & T00009row("HORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 部署名   =" & T00009row("HORGNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 日付     =" & T00009row("WORKDATE")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' DataRowをカンマ区切り文字列に変換
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <returns>カンマ区切り文字列</returns>
    ''' <remarks></remarks>
    Protected Function DataRowToCSV(ByVal I_ROW As DataRow) As String

        Dim O_CSV = ""

        If IsNothing(I_ROW) Then
            DataRowToCSV = O_CSV
        End If

        For i As Integer = 0 To I_ROW.ItemArray.Count - 1
            If i = 0 Then
                O_CSV = I_ROW.ItemArray(i).ToString()
            Else
                O_CSV = O_CSV & ControlChars.Tab & I_ROW.ItemArray(i).ToString()
            End If
        Next

        DataRowToCSV = O_CSV

    End Function


    ''' <summary>
    ''' Excel 日別明細 or 月合計要求判定
    ''' </summary>
    ''' <param name="I_REPORTID"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub ExcelHantei(ByVal I_REPORTID As String, ByRef O_RTN As String)

        O_RTN = ""
        Dim WW_MONTH As Boolean = False         '月合計判定
        Dim WW_DAY As Boolean = False           '日別判定

        CS0021PROFXLS.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0021PROFXLS.PROFID = Master.PROF_REPORT
        CS0021PROFXLS.MAPID = Master.MAPID
        CS0021PROFXLS.REPORTID = I_REPORTID
        CS0021PROFXLS.CS0021PROFXLS()
        If Not isNormal(CS0021PROFXLS.ERR) Then
            Master.output(CS0021PROFXLS.ERR, C_MESSAGE_TYPE.ERR, "CS0021UPROFXLS")
            Exit Sub
        End If

        For i As Integer = 0 To CS0021PROFXLS.FIELD.Count - 1
            If CS0021PROFXLS.EFFECT(i) = "Y" AndAlso CS0021PROFXLS.POSIX(i) > 0 AndAlso CS0021PROFXLS.POSIY(i) > 0 Then
                If CS0021PROFXLS.FIELD(i) = "TOKUSA1TIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "HOANTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "KOATUTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKSAAKAISUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKSABKAISUTTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "TOKSACKAISUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "ORVERTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "WNIGHTTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "HWORKTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "HNIGHTTIMETTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "SWORKTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "SNIGHTTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "NIGHTTIMETTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "WORKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "NENKYUNISSUTTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "KYOTEIWEEKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "SHOUKETUNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKUKYUNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "WEEKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "KUMIKETUNISSUTTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "CHIKOKSOTAINISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "DAIKYUNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "ETCKETUNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "STOCKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "NENSHINISSUTTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "SHUKCHOKNNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "SHUKCHOKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "PONPNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "BULKNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "TRAILERNISSUTTL" OrElse
                    CS0021PROFXLS.FIELD(i) = "BKINMUKAISUTTL" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0101" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0102" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0103" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0104" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0105" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0106" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0107" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0108" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0109" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0110" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0201" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0202" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0203" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0204" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0205" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0206" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0207" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0208" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0209" OrElse
                    CS0021PROFXLS.FIELD(i) = "UNLOADCNTTTL0210" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0101" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0102" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0103" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0104" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0105" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0106" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0107" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0108" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0109" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0110" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0201" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0202" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0203" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0204" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0205" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0206" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0207" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0208" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0209" OrElse
                    CS0021PROFXLS.FIELD(i) = "HAIDISTANCETTL0210" Then
                    WW_MONTH = True
                End If

                If CS0021PROFXLS.FIELD(i) = "WORKDATE" OrElse
                    CS0021PROFXLS.FIELD(i) = "HOLIDAYKBN" OrElse
                    CS0021PROFXLS.FIELD(i) = "PAYKBN" OrElse
                    CS0021PROFXLS.FIELD(i) = "SHUKCHOKKBN" OrElse
                    CS0021PROFXLS.FIELD(i) = "STDATE" OrElse
                    CS0021PROFXLS.FIELD(i) = "STTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "ENDDATE" OrElse
                    CS0021PROFXLS.FIELD(i) = "ENDTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "BINDTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "BINDSTDATE" OrElse
                    CS0021PROFXLS.FIELD(i) = "BREAKTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKUSA1TIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "HOANTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "KOATUTIME" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKSAAKAISU" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKSABKAISU" OrElse
                    CS0021PROFXLS.FIELD(i) = "TOKSACKAISU" Then
                    WW_DAY = True
                End If
            End If
        Next

        If Not WW_MONTH AndAlso Not WW_DAY Then
            'Excel定義で月合計項目と日別項目が同時に有効となっている
            O_RTN = "ERR"
        Else
            If WW_MONTH Then
                O_RTN = "月合計"
            Else
                O_RTN = "日別"
            End If
        End If

    End Sub


    ''' <summary>
    ''' 従業員情報取得
    ''' </summary>
    ''' <param name="IO_ROW"></param>
    ''' <param name="O_WORKINGH"></param>
    ''' <remarks></remarks>
    Protected Sub MB001_Select(ByRef IO_ROW As DataRow, ByRef O_WORKINGH As String)

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    ISNULL(RTRIM(MB01.STAFFCODE), '')                  AS STAFFCODE" _
                & "    , ISNULL(RTRIM(MB01.MORG), '')                     AS MORG" _
                & "    , ISNULL(RTRIM(MB01.HORG), '')                     AS HORG" _
                & "    , ISNULL(RTRIM(MB01.STAFFKBN), '')                 AS STAFFKBN" _
                & "    , ISNULL(CONVERT(char(5), MB04.WORKINGH), '00:00') AS WORKINGH" _
                & " FROM" _
                & "    MB001_STAFF MB01" _
                & "    LEFT JOIN MB004_WORKINGH MB04" _
                & "    ON  MB04.CAMPCODE = MB01.CAMPCODE" _
                & "    AND MB04.HORG     = MB01.HORG" _
                & "    AND MB04.STAFFKBN = MB01.STAFFKBN" _
                & "    AND MB04.STYMD   <= @P3" _
                & "    AND MB04.ENDYMD  >= @P3" _
                & "    AND MB04.DELFLG  <> @P4" _
                & " WHERE" _
                & "    MB01.CAMPCODE      = @P1" _
                & "    AND MB01.STAFFCODE = @P2" _
                & "    AND MB01.STYMD    <= @P3" _
                & "    AND MB01.ENDYMD   >= @P3" _
                & "    AND MB01.DELFLG   <> @P4"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '従業員コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '対象年月初日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = IO_ROW("CAMPCODE")
                PARA2.Value = IO_ROW("STAFFCODE")
                PARA3.Value = work.WF_SEL_TAISHOYM.Text & "/01"
                PARA4.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    IO_ROW("MORG") = SQLdr("MORG")                  '管理部署
                    IO_ROW("HORG") = SQLdr("HORG")                  '配属部署
                    IO_ROW("SORG") = SQLdr("HORG")                  '作業部署
                    IO_ROW("STAFFKBN") = SQLdr("STAFFKBN")          '社員区分
                    O_WORKINGH = SQLdr("WORKINGH")                  '所定労働時間
                End If

                '名称取得
                CODENAME_get("ORG", IO_ROW("MORG"), IO_ROW("MORGNAMES"), WW_DUMMY)                          '管理部署
                CODENAME_get("ORG", IO_ROW("HORG"), IO_ROW("HORGNAMES"), WW_DUMMY)                          '配属部署
                CODENAME_get("ORG", IO_ROW("SORG"), IO_ROW("SORGNAMES"), WW_DUMMY)                          '作業部署
                CODENAME_get("STAFFKBN", IO_ROW("STAFFKBN"), IO_ROW("STAFFKBNNAMES"), WW_DUMMY)             '社員区分
                CODENAME_get("STAFFKBN3", IO_ROW("STAFFKBN"), IO_ROW("STAFFKBNTAISHOGAI"), WW_RTN_SW)       '残業申請対象外
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MB001_STAFF Select"
                CS0011LOGWrite.INFPOSI = "MB001_STAFF Select"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' カレンダー情報取得
    ''' </summary>
    ''' <param name="IO_ROW"></param>
    ''' <remarks></remarks>
    Protected Sub MB005_Select(ByRef IO_ROW)

        Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
            SQLcon.Open()       'DataBase接続

            Dim SQLcmd As New SqlCommand()
            Dim SQLdr As SqlDataReader = Nothing

            Dim SQLStr As String =
                  " SELECT" _
                & "    ISNULL(RTRIM(WORKINGWEEK), '')  AS WORKINGWEEK" _
                & "    , ISNULL(RTRIM(WORKINGKBN), '') AS WORKIGNKBN" _
                & " FROM" _
                & "    MB005_CALENDAR" _
                & " WHERE" _
                & "    CAMPCODE       = @P1" _
                & "    AND WORKINGYMD = @P2" _
                & "    AND DELFLG    <> @P3"

            Try
                SQLcmd = New SqlCommand(SQLStr, SQLcon)

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '対象年月初日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = IO_ROW("CAMPCODE")
                PARA2.Value = IO_ROW("WORKDATE")
                PARA3.Value = C_DELETE_FLG.DELETE

                SQLdr = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    IO_ROW("WORKINGWEEK") = SQLdr("WORKINGWEEK")        '営業日曜日
                    IO_ROW("HOLIDAYKBN") = SQLdr("WORKIGNKBN")          '営業日区分
                End If

                '名称取得
                CODENAME_get("WORKINGWEEK", IO_ROW("WORKINGWEEK"), IO_ROW("WORKINGWEEKNAMES"), WW_DUMMY)        '営業日曜日
                CODENAME_get("HOLIDAYKBN", IO_ROW("HOLIDAYKBN"), IO_ROW("HOLIDAYKBNNAMES"), WW_DUMMY)           '休日区分
            Catch ex As Exception
                CS0011LOGWrite.INFSUBCLASS = "MB005_CALENDAR Select"
                CS0011LOGWrite.INFPOSI = "MB005_CALENDAR Select"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()
            Finally
                If Not IsNothing(SQLdr) Then
                    SQLdr.Close()
                    SQLdr = Nothing
                End If

                SQLcmd.Dispose()
                SQLcmd = Nothing
            End Try
        End Using

    End Sub


    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

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
                Case "CAMPCODE"             '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"            '従業員コード
                    prmData = work.CreateStaffCodeParam(GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_CLERK, work.WF_SEL_CAMPCODE.Text,
                                work.WF_SEL_TAISHOYM.Text, work.WF_SEL_HORG.Text, work.WF_SEL_STAFFKBN.Text, work.WF_SEL_STAFFCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKINGWEEK"          '営業日曜日
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKINGWEEK"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATUS"               '状態
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "APPROVAL"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RECODEKBN"            'レコード区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "RECODEKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"                  '部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"                 '配属部署
                    prmData = work.CreateHORGParam(work.WF_SEL_CAMPCODE.Text, Master.USERID, Master.ROLE_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"             '社員区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN2"            '時間外計算対象外（深夜のみ）
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_STAFFKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN3"            '残業申請対象外
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_STAFFKBN2"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN4"            '時間外計算、残業申請対象外
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_STAFFKBN3"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HOLIDAYKBN"           '休日区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "HOLIDAYKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PAYKBN"               '勤怠区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "PAYKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHUKCHOKKBN"          '宿日直区分
                    prmData = work.CreateShukchokKBNParam()
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "WORKKBN"              '作業区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "WORKKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHARYOKBN"            '単車・トレーラ区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILPAYKBN"            '油種給与区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILPAYKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RIYU"                 '理由
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "T0009_RIYU"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"               '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

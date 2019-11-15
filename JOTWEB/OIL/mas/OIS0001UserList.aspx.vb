''************************************************************
' ユーザIDマスタメンテ一覧画面
' 作成日 2019/11/14
' 更新日 2019/11/14
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' ユーザIDマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIS0001UserList
    Inherits Page

    '○ 検索結果格納Table
    Private OIS0001tbl As DataTable                                  '一覧格納用テーブル
    Private OIS0001INPtbl As DataTable                               'チェック用テーブル
    Private OIS0001UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

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
                    Master.RecoverTable(OIS0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
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
            If Not IsNothing(OIS0001tbl) Then
                OIS0001tbl.Clear()
                OIS0001tbl.Dispose()
                OIS0001tbl = Nothing
            End If

            If Not IsNothing(OIS0001INPtbl) Then
                OIS0001INPtbl.Clear()
                OIS0001INPtbl.Dispose()
                OIS0001INPtbl = Nothing
            End If

            If Not IsNothing(OIS0001UPDtbl) Then
                OIS0001UPDtbl.Clear()
                OIS0001UPDtbl.Dispose()
                OIS0001UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIS0001WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIS0001S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIS0001C Then
            Master.RecoverTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)
        End If

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_SEL_ORGNAME.Text, WW_DUMMY)                     '運用部署

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIS0001C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIS0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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

        If IsNothing(OIS0001tbl) Then
            OIS0001tbl = New DataTable
        End If

        If OIS0001tbl.Columns.Count <> 0 Then
            OIS0001tbl.Columns.Clear()
        End If

        OIS0001tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザマスタ、ユーザIDマスタから取得する
        Dim SQLStr As String =
            " SELECT " _
            & "    0                                                   AS LINECNT " _
            & "    , ''                                                AS OPERATION " _
            & "    , CAST(OIS0004.UPDTIMSTP AS BIGINT)                    AS TIMSTP " _
            & "    , 1                                                 AS 'SELECT' " _
            & "    , 0                                                 AS HIDDEN " _
            & "    , ISNULL(RTRIM(OIS0004.DELFLG), '')                    AS DELFLG " _
            & "    , ISNULL(RTRIM(OIS0004.USERID), '')                    AS USERID " _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMES), '')                AS STAFFNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMEL), '')                AS STAFFNAMEL0 " _
            & "    , ISNULL(RTRIM(OIS0004.MAPID), '')                     AS MAPID " _
            & "    , ISNULL(RTRIM(OIS0005.PASSWORD), '')                  AS PASSWORD " _
            & "    , ISNULL(RTRIM(OIS0005.MISSCNT), '')                   AS MISSCNT " _
            & "    , ISNULL(FORMAT(OIS0005.PASSENDYMD, 'yyyy/MM/dd'), '') AS PASSENDYMD " _
            & "    , ISNULL(FORMAT(OIS0004.STYMD, 'yyyy/MM/dd'), '')      AS STYMD " _
            & "    , ISNULL(FORMAT(OIS0004.ENDYMD, 'yyyy/MM/dd'), '')     AS ENDYMD " _
            & "    , ISNULL(RTRIM(OIS0004.CAMPCODE), '')                  AS CAMPCODE " _
            & "    , ''                                                AS CAMPNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.ORG), '')                       AS ORG " _
            & "    , ''                                                AS ORGNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.EMAIL), '')                     AS EMAIL " _
            & "    , ISNULL(RTRIM(OIS0004.MENUROLE), '')                  AS MENUROLE " _
            & "    , ISNULL(RTRIM(OIS0004.MAPROLE), '')                   AS MAPROLE " _
            & "    , ISNULL(RTRIM(OIS0004.VIEWPROFID), '')                AS VIEWPROFID " _
            & "    , ISNULL(RTRIM(OIS0004.RPRTPROFID), '')                AS RPRTPROFID " _
            & "    , ISNULL(RTRIM(OIS0004.VARIANT), '')             AS INIMAPVALROLE " _
            & "    , ISNULL(RTRIM(OIS0004.APPROVALID), '')                AS APPROVALID " _
            & " FROM " _
            & "    COM.OIS0004_USER OIS0004 " _
            & "    INNER JOIN COM.OIS0005_USERPASS OIS0005 " _
            & "        ON  OIS0005.USERID   = OIS0004.USERID" _
            & "        AND OIS0005.DELFLG  <> @P6" _
            & " WHERE" _
            & "    OIS0004.CAMPCODE    = @P1" _
            & "    AND OIS0004.STYMD  <= @P4" _
            & "    AND OIS0004.ENDYMD >= @P5" _
            & "    AND OIS0004.DELFLG <> @P6"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLStr &= String.Format("    AND OIS0004.ORG     = '{0}'", work.WF_SEL_ORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIS0004.ORG" _
            & "    , OIS0004.USERID"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)                '有効年月日(From)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA4.Value = work.WF_SEL_ENDYMD.Text
                PARA5.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIS0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIS0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIS0001row As DataRow In OIS0001tbl.Rows
                    i += 1
                    OIS0001row("LINECNT") = i        'LINECNT
                    '名称取得
                    CODENAME_get("CAMPCODE", OIS0001row("CAMPCODE"), OIS0001row("CAMPNAMES"), WW_DUMMY)                               '会社コード
                    CODENAME_get("ORG", OIS0001row("ORG"), OIS0001row("ORGNAMES"), WW_DUMMY)                                          '組織コード
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001L Select"
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
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            If OIS0001row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIS0001row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIS0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        WF_Sel_LINECNT.Text = ""
        work.WF_SEL_LINECNT.Text = ""

        'ユーザID
        WF_USERID.Text = ""
        work.WF_SEL_USERID.Text = ""

        '社員名（短）
        WF_STAFFNAMES.Text = ""
        work.WF_SEL_STAFFNAMES.Text = ""

        '社員名（長）
        WF_STAFFNAMEL.Text = ""
        work.WF_SEL_STAFFNAMEL.Text = ""

        '画面ＩＤ
        WF_MAPID.Text = ""
        work.WF_SEL_MAPID.Text = ""

        'パスワード
        WF_PASSWORD.Text = ""
        work.WF_SEL_PASSWORD.Text = ""

        '誤り回数
        WF_MISSCNT.Text = ""
        work.WF_SEL_MISSCNT.Text = ""

        'パスワード有効期限
        WF_PASSENDYMD.Text = ""
        work.WF_SEL_PASSENDYMD.Text = ""

        '開始年月日
        WF_STYMD.Text = ""
        work.WF_SEL_STYMD.Text = ""

        '終了年月日
        WF_ENDYMD.Text = ""
        work.WF_SEL_ENDYMD.Text = ""

        '会社コード
        WF_CAMPCODE.Text = ""
        work.WF_SEL_CAMPCODE.Text = ""

        '組織コード
        WF_ORG.Text = ""
        work.WF_SEL_ORG.Text = ""

        'メールアドレス
        WF_EMAIL.Text = ""
        work.WF_SEL_EMAIL.Text = ""

        'メニュー表示制御ロール
        WF_MENUROLE.Text = ""
        work.WF_SEL_MENUROLE.Text = ""

        '画面参照更新制御ロール
        WF_MAPROLE.Text = ""
        work.WF_SEL_MAPROLE.Text = ""

        '画面表示項目制御ロール
        WF_VIEWPROFID.Text = ""
        work.WF_SEL_VIEWPROFID.Text = ""

        'エクセル出力制御ロール
        WF_RPRTPROFID.Text = ""
        work.WF_SEL_RPRTPROFID.Text = ""

        '画面初期値ロール
        WF_VARIANT.Text = ""
        work.WF_SEL_VARIANT.Text = ""

        '承認権限ロール
        WF_APPROVALID.Text = ""
        work.WF_SEL_APPROVALID.Text = ""

        '削除
        WF_DELFLG.Text = "0"
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        WF_GridDBclick.Text = ""

        work.WF_SEL_DELFLG.Text = "0"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '○関連チェック
        RelatedCheck(WW_ERRCODE)

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'マスタ更新
                UpdateMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

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
        Dim WW_LINE_ERR As String = ""
        Dim WW_CheckMES As String = ""
        Dim WW_DATE_ST As Date
        Dim WW_DATE_END As Date
        Dim WW_DATE_ST2 As Date
        Dim WW_DATE_END2 As Date

        '○ 日付重複チェック
        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '読み飛ばし
            If (OIS0001row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                OIS0001row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                OIS0001row("DELFLG") = C_DELETE_FLG.DELETE OrElse
                OIS0001row("STYMD") = "" Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each OIS0001chk As DataRow In OIS0001tbl.Rows

                '同一KEY以外は読み飛ばし
                If OIS0001row("CAMPCODE") <> OIS0001chk("CAMPCODE") OrElse
                    OIS0001row("USERID") <> OIS0001chk("USERID") OrElse
                    OIS0001chk("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                '期間変更対象は読み飛ばし
                If OIS0001row("STYMD") = OIS0001chk("STYMD") Then
                    Continue For
                End If

                Try
                    Date.TryParse(OIS0001row("STYMD"), WW_DATE_ST)
                    Date.TryParse(OIS0001row("ENDYMD"), WW_DATE_END)
                    Date.TryParse(OIS0001chk("STYMD"), WW_DATE_ST2)
                    Date.TryParse(OIS0001chk("ENDYMD"), WW_DATE_END2)
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                    Exit Sub
                End Try

                '開始日チェック
                If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", OIS0001row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '終了日チェック
                If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                    WW_CheckMES = "・エラー(期間重複)が存在します。"
                    WW_CheckERR(WW_CheckMES, "", OIS0001row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If

                '日付連続性チェック
                If WW_DATE_END.AddDays(1) <> WW_DATE_ST2 Then
                    WW_CheckMES = "・エラー(開始、終了年月日が連続していません)。"
                    WW_CheckERR(WW_CheckMES, "", OIS0001row)
                    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    WW_LINE_ERR = "ERR"
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' ユーザIDマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        COM.OIS0004_USER" _
            & "    INNER JOIN COM.OIS0005_USERPASS OIS0005 " _
            & "        ON  OIS0005.USERID   = OIS0004.USERID" _
            & "        AND OIS0005.DELFLG  <> @P6" _
            & "    WHERE" _
            & "        USERID       = @P1" _
            & "        AND STYMD    = @P2" _
            & "        AND CAMPCODE = @P4 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE COM.OIS0004_USER" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , USERID = @P01" _
            & "        , STAFFNAMES = @P02" _
            & "        , STAFFNAMEL = @P03" _
            & "        , MAPID = @P04" _
            & "        , PASSWORD = @P05" _
            & "        , MISSCNT = @P06" _
            & "        , PASSENDYMD = @P07" _
            & "        , STYMD = @P08" _
            & "        , ENDYMD = @P09" _
            & "        , CAMPCODE = @P10" _
            & "        , ORG = @P11" _
            & "        , EMAIL = @P12" _
            & "        , MENUROLE = @P13" _
            & "        , MAPROLE = @P14" _
            & "        , VIEWPROFID = @P15" _
            & "        , RPRTPROFID = @P16" _
            & "        , INIMAPVALROLE = @P17" _
            & "        , APPROVALID = @P18" _
            & "        , INITYMD = @P19" _
            & "        , INITUSER = @P20" _
            & "        , INITTERMID = @P21" _
            & "        , UPDYMD = @P22" _
            & "        , UPDUSER = @P23" _
            & "        , UPDTERMID = @P24" _
            & "        , RECEIVEYMD = @P25" _
            & "    WHERE" _
            & "        USERID       = @P01 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO COM.OIS0004_USER" _
            & "        (DELFLG" _
            & "        , USERID" _
            & "        , STAFFNAMES" _
            & "        , STAFFNAMEL" _
            & "        , MAPID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , INIMAPVALROLE" _
            & "        , APPROVALID" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P00" _
            & "        , @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P13" _
            & "        , @P14" _
            & "        , @P15" _
            & "        , @P16" _
            & "        , @P17" _
            & "        , @P18" _
            & "        , @P19" _
            & "        , @P20" _
            & "        , @P21" _
            & "        , @P22" _
            & "        , @P23" _
            & "        , @P24" _
            & "        , @P25) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "        , USERID" _
            & "        , STAFFNAMES" _
            & "        , STAFFNAMEL" _
            & "        , MAPID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , INIMAPVALROLE" _
            & "        , APPROVALID" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    COM.OIS0004_USER" _
            & "    INNER JOIN COM.OIS0005_USERPASS OIS0005 " _
            & "        ON  OIS0005.USERID   = OIS0004.USERID" _
            & "        AND OIS0005.DELFLG  <> @P6" _
            & " WHERE" _
            & "        USERID = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)            '開始年月日
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Date)            '終了年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 2)            '会社コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 6)            '組織コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 128)            'メールアドレス
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)            'メニュー表示制御ロール
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '画面参照更新制御ロール
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '画面表示項目制御ロール
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)            'エクセル出力制御ロール
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '画面初期値ロール
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '承認権限ロール
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.DateTime)            '登録年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.NVarChar, 20)            '登録端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.DateTime)            '更新年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.NVarChar, 20)            '更新端末
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.DateTime)            '集信日時

                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
                Dim JPARA08 As SqlParameter = SQLcmdJnl.Parameters.Add("@P08", SqlDbType.Date)            '開始年月日
                Dim JPARA09 As SqlParameter = SQLcmdJnl.Parameters.Add("@P09", SqlDbType.Date)            '終了年月日
                Dim JPARA10 As SqlParameter = SQLcmdJnl.Parameters.Add("@P10", SqlDbType.NVarChar, 2)            '会社コード
                Dim JPARA11 As SqlParameter = SQLcmdJnl.Parameters.Add("@P11", SqlDbType.NVarChar, 6)            '組織コード
                Dim JPARA12 As SqlParameter = SQLcmdJnl.Parameters.Add("@P12", SqlDbType.NVarChar, 128)            'メールアドレス
                Dim JPARA13 As SqlParameter = SQLcmdJnl.Parameters.Add("@P13", SqlDbType.NVarChar, 20)            'メニュー表示制御ロール
                Dim JPARA14 As SqlParameter = SQLcmdJnl.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '画面参照更新制御ロール
                Dim JPARA15 As SqlParameter = SQLcmdJnl.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '画面表示項目制御ロール
                Dim JPARA16 As SqlParameter = SQLcmdJnl.Parameters.Add("@P16", SqlDbType.NVarChar, 20)            'エクセル出力制御ロール
                Dim JPARA17 As SqlParameter = SQLcmdJnl.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '画面初期値ロール
                Dim JPARA18 As SqlParameter = SQLcmdJnl.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '承認権限ロール
                Dim JPARA19 As SqlParameter = SQLcmdJnl.Parameters.Add("@P58", SqlDbType.DateTime)            '登録年月日
                Dim JPARA20 As SqlParameter = SQLcmdJnl.Parameters.Add("@P59", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim JPARA21 As SqlParameter = SQLcmdJnl.Parameters.Add("@P60", SqlDbType.NVarChar, 20)            '登録端末
                Dim JPARA22 As SqlParameter = SQLcmdJnl.Parameters.Add("@P61", SqlDbType.DateTime)            '更新年月日
                Dim JPARA23 As SqlParameter = SQLcmdJnl.Parameters.Add("@P62", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim JPARA24 As SqlParameter = SQLcmdJnl.Parameters.Add("@P63", SqlDbType.NVarChar, 20)            '更新端末
                Dim JPARA25 As SqlParameter = SQLcmdJnl.Parameters.Add("@P64", SqlDbType.DateTime)            '集信日時

                For Each OIS0001row As DataRow In OIS0001tbl.Rows
                    If Trim(OIS0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIS0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIS0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIS0001row("DELFLG")
                        PARA01.Value = OIS0001row("USERID")
                        PARA02.Value = OIS0001row("STAFFNAMES")
                        PARA03.Value = OIS0001row("STAFFNAMEL")
                        PARA04.Value = OIS0001row("MAPID")
                        PARA05.Value = OIS0001row("PASSWORD")
                        If OIS0001row("MISSCNT") <> "" Then
                            PARA06.Value = OIS0001row("MISSCNT")
                        Else
                            PARA06.Value = "0"
                        End If
                        If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                            PARA07.Value = RTrim(OIS0001row("PASSENDYMD"))
                        Else
                            PARA07.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIS0001row("STYMD")) <> "" Then
                            PARA08.Value = RTrim(OIS0001row("STYMD"))
                        Else
                            PARA08.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIS0001row("ENDYMD")) <> "" Then
                            PARA09.Value = RTrim(OIS0001row("ENDYMD"))
                        Else
                            PARA09.Value = C_DEFAULT_YMD
                        End If
                        PARA10.Value = OIS0001row("CAMPCODE")
                        PARA11.Value = OIS0001row("ORG")
                        PARA12.Value = OIS0001row("EMAIL")
                        PARA13.Value = OIS0001row("MENUROLE")
                        PARA14.Value = OIS0001row("MAPROLE")
                        PARA15.Value = OIS0001row("VIEWPROFID")
                        PARA16.Value = OIS0001row("RPRTPROFID")
                        PARA17.Value = OIS0001row("INIMAPVALROLE")
                        PARA18.Value = OIS0001row("APPROVALID")
                        PARA19.Value = WW_DATENOW
                        PARA20.Value = Master.USERID
                        PARA21.Value = Master.USERTERMID
                        PARA22.Value = WW_DATENOW
                        PARA23.Value = Master.USERID
                        PARA24.Value = Master.USERTERMID
                        PARA25.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIS0001row("DELFLG")
                        JPARA01.Value = OIS0001row("USERID")
                        JPARA02.Value = OIS0001row("STAFFNAMES")
                        JPARA03.Value = OIS0001row("STAFFNAMEL")
                        JPARA04.Value = OIS0001row("MAPID")
                        JPARA05.Value = OIS0001row("PASSWORD")
                        If OIS0001row("MISSCNT") <> "" Then
                            JPARA06.Value = OIS0001row("MISSCNT")
                        Else
                            JPARA06.Value = "0"
                        End If
                        If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                            JPARA07.Value = RTrim(OIS0001row("PASSENDYMD"))
                        Else
                            JPARA07.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIS0001row("STYMD")) <> "" Then
                            JPARA08.Value = RTrim(OIS0001row("STYMD"))
                        Else
                            JPARA08.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIS0001row("ENDYMD")) <> "" Then
                            JPARA09.Value = RTrim(OIS0001row("ENDYMD"))
                        Else
                            JPARA09.Value = C_DEFAULT_YMD
                        End If
                        JPARA10.Value = OIS0001row("CAMPCODE")
                        JPARA11.Value = OIS0001row("ORG")
                        JPARA12.Value = OIS0001row("EMAIL")
                        JPARA13.Value = OIS0001row("MENUROLE")
                        JPARA14.Value = OIS0001row("MAPROLE")
                        JPARA15.Value = OIS0001row("VIEWPROFID")
                        JPARA16.Value = OIS0001row("RPRTPROFID")
                        JPARA17.Value = OIS0001row("INIMAPVALROLE")
                        JPARA18.Value = OIS0001row("APPROVALID")
                        JPARA19.Value = WW_DATENOW
                        JPARA20.Value = Master.USERID
                        JPARA21.Value = Master.USERTERMID
                        JPARA22.Value = WW_DATENOW
                        JPARA23.Value = Master.USERID
                        JPARA24.Value = Master.USERTERMID
                        JPARA25.Value = C_DEFAULT_YMD

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                                If IsNothing(OIS0001UPDtbl) Then
                                    OIS0001UPDtbl = New DataTable

                                    For index As Integer = 0 To SQLdr.FieldCount - 1
                                        OIS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                    Next
                                End If

                                OIS0001UPDtbl.Clear()
                                OIS0001UPDtbl.Load(SQLdr)
                            End Using

                            For Each OIS0001UPDrow As DataRow In OIS0001UPDtbl.Rows
                                CS0020JOURNAL.TABLENM = "OIS0001L"
                                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                                CS0020JOURNAL.ROW = OIS0001UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIS0001tbl                        'データ参照  Table
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
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIS0001tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub


    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(OIS0001tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '選択行
        WF_Sel_LINECNT.Text = OIS0001tbl.Rows(WW_LINECNT)("LINECNT")
        work.WF_SEL_LINECNT.Text = OIS0001tbl.Rows(WW_LINECNT)("LINECNT")

        'ユーザID
        WF_USERID.Text = OIS0001tbl.Rows(WW_LINECNT)("USERID")
        work.WF_SEL_USERID.Text = OIS0001tbl.Rows(WW_LINECNT)("USERID")

        '社員名（短）
        WF_STAFFNAMES.Text = OIS0001tbl.Rows(WW_LINECNT)("STAFFNAMES")
        work.WF_SEL_STAFFNAMES.Text = OIS0001tbl.Rows(WW_LINECNT)("STAFFNAMES")

        '社員名（長）
        WF_STAFFNAMEL.Text = OIS0001tbl.Rows(WW_LINECNT)("STAFFNAMEL")
        work.WF_SEL_STAFFNAMEL.Text = OIS0001tbl.Rows(WW_LINECNT)("STAFFNAMEL")

        '画面ＩＤ
        WF_MAPID.Text = OIS0001tbl.Rows(WW_LINECNT)("MAPID")
        work.WF_SEL_MAPID.Text = OIS0001tbl.Rows(WW_LINECNT)("MAPID")

        'パスワード
        WF_PASSWORD.Text = OIS0001tbl.Rows(WW_LINECNT)("PASSWORD")
        work.WF_SEL_PASSWORD.Text = OIS0001tbl.Rows(WW_LINECNT)("PASSWORD")

        '誤り回数
        WF_MISSCNT.Text = OIS0001tbl.Rows(WW_LINECNT)("MISSCNT")
        work.WF_SEL_MISSCNT.Text = OIS0001tbl.Rows(WW_LINECNT)("MISSCNT")

        'パスワード有効期限
        WF_PASSENDYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("PASSENDYMD")
        work.WF_SEL_PASSENDYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("PASSENDYMD")

        '開始年月日
        WF_STYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("STYMD")
        work.WF_SEL_STYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("STYMD")

        '終了年月日
        WF_ENDYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("ENDYMD")
        work.WF_SEL_ENDYMD.Text = OIS0001tbl.Rows(WW_LINECNT)("ENDYMD")

        '会社コード
        WF_CAMPCODE.Text = OIS0001tbl.Rows(WW_LINECNT)("CAMPCODE")
        work.WF_SEL_CAMPCODE.Text = OIS0001tbl.Rows(WW_LINECNT)("CAMPCODE")

        '組織コード
        WF_ORG.Text = OIS0001tbl.Rows(WW_LINECNT)("ORG")
        work.WF_SEL_ORG.Text = OIS0001tbl.Rows(WW_LINECNT)("ORG")

        'メールアドレス
        WF_EMAIL.Text = OIS0001tbl.Rows(WW_LINECNT)("EMAIL")
        work.WF_SEL_EMAIL.Text = OIS0001tbl.Rows(WW_LINECNT)("EMAIL")

        'メニュー表示制御ロール
        WF_MENUROLE.Text = OIS0001tbl.Rows(WW_LINECNT)("MENUROLE")
        work.WF_SEL_MENUROLE.Text = OIS0001tbl.Rows(WW_LINECNT)("MENUROLE")

        '画面参照更新制御ロール
        WF_MAPROLE.Text = OIS0001tbl.Rows(WW_LINECNT)("MAPROLE")
        work.WF_SEL_MAPROLE.Text = OIS0001tbl.Rows(WW_LINECNT)("MAPROLE")

        '画面表示項目制御ロール
        WF_VIEWPROFID.Text = OIS0001tbl.Rows(WW_LINECNT)("VIEWPROFID")
        work.WF_SEL_VIEWPROFID.Text = OIS0001tbl.Rows(WW_LINECNT)("VIEWPROFID")

        'エクセル出力制御ロール
        WF_RPRTPROFID.Text = OIS0001tbl.Rows(WW_LINECNT)("RPRTPROFID")
        work.WF_SEL_RPRTPROFID.Text = OIS0001tbl.Rows(WW_LINECNT)("RPRTPROFID")

        '画面初期値ロール
        WF_VARIANT.Text = OIS0001tbl.Rows(WW_LINECNT)("VARIANT")
        work.WF_SEL_VARIANT.Text = OIS0001tbl.Rows(WW_LINECNT)("VARIANT")

        '承認権限ロール
        WF_APPROVALID.Text = OIS0001tbl.Rows(WW_LINECNT)("APPROVALID")
        work.WF_SEL_APPROVALID.Text = OIS0001tbl.Rows(WW_LINECNT)("APPROVALID")

        '削除フラグ
        WF_DELFLG.Text = OIS0001tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)
        work.WF_SEL_DELFLG.Text = OIS0001tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIS0001tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIS0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIS0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIS0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIS0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIS0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        '登録画面ページへ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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
        Master.CreateEmptyTable(OIS0001INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIS0001INProw As DataRow = OIS0001INPtbl.NewRow

            '○ 初期クリア
            For Each OIS0001INPcol As DataColumn In OIS0001INPtbl.Columns
                If IsDBNull(OIS0001INProw.Item(OIS0001INPcol)) OrElse IsNothing(OIS0001INProw.Item(OIS0001INPcol)) Then
                    Select Case OIS0001INPcol.ColumnName
                        Case "LINECNT"
                            OIS0001INProw.Item(OIS0001INPcol) = 0
                        Case "OPERATION"
                            OIS0001INProw.Item(OIS0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIS0001INProw.Item(OIS0001INPcol) = 0
                        Case "SELECT"
                            OIS0001INProw.Item(OIS0001INPcol) = 1
                        Case "HIDDEN"
                            OIS0001INProw.Item(OIS0001INPcol) = 0
                        Case Else
                            OIS0001INProw.Item(OIS0001INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("USERID") >= 0 Then

                For Each OIS0001row As DataRow In OIS0001tbl.Rows
                    If XLSTBLrow("USERID") = OIS0001row("USERID") AndAlso
                        XLSTBLrow("STAFFNAMES") = OIS0001row("STAFFNAMES") AndAlso
                        XLSTBLrow("STAFFNAMEL") = OIS0001row("STAFFNAMEL") AndAlso
                        XLSTBLrow("MAPID") = OIS0001row("MAPID") AndAlso
                        XLSTBLrow("PASSWORD") = OIS0001row("PASSWORD") AndAlso
                        XLSTBLrow("MISSCNT") = OIS0001row("MISSCNT") AndAlso
                        XLSTBLrow("PASSENDYMD") = OIS0001row("PASSENDYMD") AndAlso
                        XLSTBLrow("STYMD") = OIS0001row("STYMD") AndAlso
                        XLSTBLrow("ENDYMD") = OIS0001row("ENDYMD") AndAlso
                        XLSTBLrow("CAMPCODE") = OIS0001row("CAMPCODE") AndAlso
                        XLSTBLrow("ORG") = OIS0001row("ORG") AndAlso
                        XLSTBLrow("EMAIL") = OIS0001row("EMAIL") AndAlso
                        XLSTBLrow("MENUROLE") = OIS0001row("MENUROLE") AndAlso
                        XLSTBLrow("MAPROLE") = OIS0001row("MAPROLE") AndAlso
                        XLSTBLrow("VIEWPROFID") = OIS0001row("VIEWPROFID") AndAlso
                        XLSTBLrow("RPRTPROFID") = OIS0001row("RPRTPROFID") AndAlso
                        XLSTBLrow("INIMAPVALROLE") = OIS0001row("INIMAPVALROLE") AndAlso
                        XLSTBLrow("APPROVALID") = OIS0001row("APPROVALID") Then
                        OIS0001INProw.ItemArray = OIS0001row.ItemArray Then
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            'JOT車番
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                OIS0001INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")
            End If

            '型式
            If WW_COLUMNS.IndexOf("MODEL") >= 0 Then
                OIS0001INProw("MODEL") = XLSTBLrow("MODEL")
            End If

            '原籍所有者C
            If WW_COLUMNS.IndexOf("ORIGINOWNERCODE") >= 0 Then
                OIS0001INProw("ORIGINOWNERCODE") = XLSTBLrow("ORIGINOWNERCODE")
            End If

            '名義所有者C
            If WW_COLUMNS.IndexOf("OWNERCODE") >= 0 Then
                OIS0001INProw("OWNERCODE") = XLSTBLrow("OWNERCODE")
            End If

            'リース先C
            If WW_COLUMNS.IndexOf("LEASECODE") >= 0 Then
                OIS0001INProw("LEASECODE") = XLSTBLrow("LEASECODE")
            End If

            'リース区分C
            If WW_COLUMNS.IndexOf("LEASECLASS") >= 0 Then
                OIS0001INProw("LEASECLASS") = XLSTBLrow("LEASECLASS")
            End If

            '自動延長
            If WW_COLUMNS.IndexOf("AUTOEXTENTION") >= 0 Then
                OIS0001INProw("AUTOEXTENTION") = XLSTBLrow("AUTOEXTENTION")
            End If

            'リース開始年月日
            If WW_COLUMNS.IndexOf("LEASESTYMD") >= 0 Then
                OIS0001INProw("LEASESTYMD") = XLSTBLrow("LEASESTYMD")
            End If

            'リース満了年月日
            If WW_COLUMNS.IndexOf("LEASEENDYMD") >= 0 Then
                OIS0001INProw("LEASEENDYMD") = XLSTBLrow("LEASEENDYMD")
            End If

            '第三者使用者C
            If WW_COLUMNS.IndexOf("USERCODE") >= 0 Then
                OIS0001INProw("USERCODE") = XLSTBLrow("USERCODE")
            End If

            '原常備駅C
            If WW_COLUMNS.IndexOf("CURRENTSTATIONCODE") >= 0 Then
                OIS0001INProw("CURRENTSTATIONCODE") = XLSTBLrow("CURRENTSTATIONCODE")
            End If

            '臨時常備駅C
            If WW_COLUMNS.IndexOf("EXTRADINARYSTATIONCODE") >= 0 Then
                OIS0001INProw("EXTRADINARYSTATIONCODE") = XLSTBLrow("EXTRADINARYSTATIONCODE")
            End If

            '第三者使用期限
            If WW_COLUMNS.IndexOf("USERLIMIT") >= 0 Then
                OIS0001INProw("USERLIMIT") = XLSTBLrow("USERLIMIT")
            End If

            '臨時常備駅期限
            If WW_COLUMNS.IndexOf("LIMITTEXTRADIARYSTATION") >= 0 Then
                OIS0001INProw("LIMITTEXTRADIARYSTATION") = XLSTBLrow("LIMITTEXTRADIARYSTATION")
            End If

            '原専用種別C
            If WW_COLUMNS.IndexOf("DEDICATETYPECODE") >= 0 Then
                OIS0001INProw("DEDICATETYPECODE") = XLSTBLrow("DEDICATETYPECODE")
            End If

            '臨時専用種別C
            If WW_COLUMNS.IndexOf("EXTRADINARYTYPECODE") >= 0 Then
                OIS0001INProw("EXTRADINARYTYPECODE") = XLSTBLrow("EXTRADINARYTYPECODE")
            End If

            '臨時専用期限
            If WW_COLUMNS.IndexOf("EXTRADINARYLIMIT") >= 0 Then
                OIS0001INProw("EXTRADINARYLIMIT") = XLSTBLrow("EXTRADINARYLIMIT")
            End If

            '運用基地C
            If WW_COLUMNS.IndexOf("OPERATIONBASECODE") >= 0 Then
                OIS0001INProw("OPERATIONBASECODE") = XLSTBLrow("OPERATIONBASECODE")
            End If

            '塗色C
            If WW_COLUMNS.IndexOf("COLORCODE") >= 0 Then
                OIS0001INProw("COLORCODE") = XLSTBLrow("COLORCODE")
            End If

            'エネオス
            If WW_COLUMNS.IndexOf("ENEOS") >= 0 Then
                OIS0001INProw("ENEOS") = XLSTBLrow("ENEOS")
            End If

            'エコレール
            If WW_COLUMNS.IndexOf("ECO") >= 0 Then
                OIS0001INProw("ECO") = XLSTBLrow("ECO")
            End If

            '取得年月日
            If WW_COLUMNS.IndexOf("ALLINSPECTIONDATE") >= 0 Then
                OIS0001INProw("ALLINSPECTIONDATE") = XLSTBLrow("ALLINSPECTIONDATE")
            End If

            '車籍編入年月日
            If WW_COLUMNS.IndexOf("TRANSFERDATE") >= 0 Then
                OIS0001INProw("TRANSFERDATE") = XLSTBLrow("TRANSFERDATE")
            End If

            '取得先C
            If WW_COLUMNS.IndexOf("OBTAINEDCODE") >= 0 Then
                OIS0001INProw("OBTAINEDCODE") = XLSTBLrow("OBTAINEDCODE")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIS0001INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIS0001INProw("DELFLG") = "0"
            End If

            OIS0001INPtbl.Rows.Add(OIS0001INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIS0001tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

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


    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIS0001INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIS0001tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        '○ 詳細画面初期化
        If isNormal(WW_ERR_SW) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            End If
        End If

        '○画面切替設定
        WF_BOXChange.Value = "headerbox"

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIS0001INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIS0001INPtbl)
        Dim OIS0001INProw As DataRow = OIS0001INPtbl.NewRow

        '○ 初期クリア
        For Each OIS0001INPcol As DataColumn In OIS0001INPtbl.Columns
            If IsDBNull(OIS0001INProw.Item(OIS0001INPcol)) OrElse IsNothing(OIS0001INProw.Item(OIS0001INPcol)) Then
                Select Case OIS0001INPcol.ColumnName
                    Case "LINECNT"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "OPERATION"
                        OIS0001INProw.Item(OIS0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "SELECT"
                        OIS0001INProw.Item(OIS0001INPcol) = 1
                    Case "HIDDEN"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case Else
                        OIS0001INProw.Item(OIS0001INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIS0001INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIS0001INProw("LINECNT"))
            Catch ex As Exception
                OIS0001INProw("LINECNT") = 0
            End Try
        End If

        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIS0001INProw("UPDTIMSTP") = 0
        OIS0001INProw("SELECT") = 1
        OIS0001INProw("HIDDEN") = 0

        OIS0001INProw("USERID") = WF_USERID.Text              'ユーザID

        OIS0001INProw("STAFFNAMES") = WF_STAFFNAMES.Text              '社員名（短）

        OIS0001INProw("STAFFNAMEL") = WF_STAFFNAMEL.Text              '社員名（長）

        OIS0001INProw("MAPID") = WF_MAPID.Text              '画面ＩＤ

        OIS0001INProw("PASSWORD") = WF_PASSWORD.Text              'パスワード

        OIS0001INProw("MISSCNT") = WF_MISSCNT.Text              '誤り回数

        OIS0001INProw("PASSENDYMD") = WF_PASSENDYMD.Text              'パスワード有効期限

        OIS0001INProw("STYMD") = WF_STYMD.Text              '開始年月日

        OIS0001INProw("ENDYMD") = WF_ENDYMD.Text              '終了年月日

        OIS0001INProw("CAMPCODE") = WF_CAMPCODE.Text              '会社コード

        OIS0001INProw("ORG") = WF_ORG.Text              '組織コード

        OIS0001INProw("EMAIL") = WF_EMAIL.Text              'メールアドレス

        OIS0001INProw("MENUROLE") = WF_MENUROLE.Text              'メニュー表示制御ロール

        OIS0001INProw("MAPROLE") = WF_MAPROLE.Text              '画面参照更新制御ロール

        OIS0001INProw("VIEWPROFID") = WF_VIEWPROFID.Text              '画面表示項目制御ロール

        OIS0001INProw("RPRTPROFID") = WF_RPRTPROFID.Text              'エクセル出力制御ロール

        OIS0001INProw("VARIANT") = WF_VARIANT.Text              '画面初期値ロール

        OIS0001INProw("APPROVALID") = WF_APPROVALID.Text              '承認権限ロール

        '○ チェック用テーブルに登録する
        OIS0001INPtbl.Rows.Add(OIS0001INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○画面切替設定
        WF_BOXChange.Value = "headerbox"

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_USERID.Text = ""            'ユーザID
        WF_STAFFNAMES.Text = ""            '社員名（短）
        WF_STAFFNAMEL.Text = ""            '社員名（長）
        WF_MAPID.Text = ""            '画面ＩＤ
        WF_PASSWORD.Text = ""            'パスワード
        WF_MISSCNT.Text = ""            '誤り回数
        WF_PASSENDYMD.Text = ""            'パスワード有効期限
        WF_STYMD.Text = ""            '開始年月日
        WF_ENDYMD.Text = ""            '終了年月日
        WF_CAMPCODE.Text = ""            '会社コード
        WF_ORG.Text = ""            '組織コード
        WF_EMAIL.Text = ""            'メールアドレス
        WF_MENUROLE.Text = ""            'メニュー表示制御ロール
        WF_MAPROLE.Text = ""            '画面参照更新制御ロール
        WF_VIEWPROFID.Text = ""            '画面表示項目制御ロール
        WF_RPRTPROFID.Text = ""            'エクセル出力制御ロール
        WF_VARIANT.Text = ""            '画面初期値ロール
        WF_APPROVALID.Text = ""            '承認権限ロール
        WF_DELFLG.Text = ""                 '削除フラグ
        WF_DELFLG_TEXT.Text = ""            '削除フラグ名称

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                '会社コード
                Dim prmData As New Hashtable

                'フィールドによってパラメーターを変える
                Select Case WW_FIELD
                    Case "WF_DELFLG"
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
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

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除フラグ
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

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
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIS0001INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIS0001INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JOT車番(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIS0001INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "JOT車番入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", OIS0001INProw("ORIGINOWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原籍所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", OIS0001INProw("OWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "名義所有者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECODE", OIS0001INProw("LEASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース区分C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASS", OIS0001INProw("LEASECLASS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース区分C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '自動延長(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION", OIS0001INProw("AUTOEXTENTION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "自動延長入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース開始年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASESTYMD", OIS0001INProw("LEASESTYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース開始年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース満了年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASEENDYMD", OIS0001INProw("LEASEENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "リース満了年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERCODE", OIS0001INProw("USERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用者C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", OIS0001INProw("CURRENTSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONCODE", OIS0001INProw("EXTRADINARYSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERLIMIT", OIS0001INProw("USERLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "第三者使用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LIMITTEXTRADIARYSTATION", OIS0001INProw("LIMITTEXTRADIARYSTATION"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時常備駅期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPECODE", OIS0001INProw("DEDICATETYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "原専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用種別C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPECODE", OIS0001INProw("EXTRADINARYTYPECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用種別C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用期限(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYLIMIT", OIS0001INProw("EXTRADINARYLIMIT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "臨時専用期限入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", OIS0001INProw("OPERATIONBASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "運用基地C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '塗色C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORCODE", OIS0001INProw("COLORCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "塗色C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エネオス(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENEOS", OIS0001INProw("ENEOS"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エネオス入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エコレール(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ECO", OIS0001INProw("ECO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "エコレール入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTIONDATE", OIS0001INProw("ALLINSPECTIONDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車籍編入年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", OIS0001INProw("TRANSFERDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "車籍編入年月日入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得先C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDCODE", OIS0001INProw("OBTAINEDCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "取得先C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIS0001row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIS0001row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIS0001row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIS0001row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIS0001row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIS0001row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIS0001row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分C =" & OIS0001row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長 =" & OIS0001row("AUTOEXTENTION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース開始年月日 =" & OIS0001row("LEASESTYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース満了年月日 =" & OIS0001row("LEASEENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者C =" & OIS0001row("USERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅C =" & OIS0001row("CURRENTSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅C =" & OIS0001row("EXTRADINARYSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用期限 =" & OIS0001row("USERLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅期限 =" & OIS0001row("LIMITTEXTRADIARYSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別C =" & OIS0001row("DEDICATETYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別C =" & OIS0001row("EXTRADINARYTYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用期限 =" & OIS0001row("EXTRADINARYLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIS0001row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色C =" & OIS0001row("COLORCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エネオス =" & OIS0001row("ENEOS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エコレール =" & OIS0001row("ECO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得年月日 =" & OIS0001row("ALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍編入年月日 =" & OIS0001row("TRANSFERDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先C =" & OIS0001row("OBTAINEDCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIS0001row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式カナ =" & OIS0001row("MODELKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIS0001row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重単位 =" & OIS0001row("LOADUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積 =" & OIS0001row("VOLUME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積単位 =" & OIS0001row("VOLUMEUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIS0001row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIS0001row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIS0001row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分 =" & OIS0001row("LEASECLASSNEMAE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者 =" & OIS0001row("USERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅 =" & OIS0001row("CURRENTSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅 =" & OIS0001row("EXTRADINARYSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別 =" & OIS0001row("DEDICATETYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別 =" & OIS0001row("EXTRADINARYTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIS0001row("OPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色 =" & OIS0001row("COLORNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備1 =" & OIS0001row("RESERVE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備2 =" & OIS0001row("RESERVE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日 =" & OIS0001row("SPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日(JR)  =" & OIS0001row("JRALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 現在経年 =" & OIS0001row("PROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検時経年 =" & OIS0001row("NEXTPROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日(JR） =" & OIS0001row("JRINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日 =" & OIS0001row("INSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日(JR) =" & OIS0001row("JRSPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車番 =" & OIS0001row("JRTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 旧JOT車番 =" & OIS0001row("OLDTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT車番 =" & OIS0001row("OTTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG車番 =" & OIS0001row("JXTGTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモ車番 =" & OIS0001row("COSMOTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 富士石油車番 =" & OIS0001row("FUJITANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シ車番 =" & OIS0001row("SHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備 =" & OIS0001row("RESERVE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIS0001row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

    ''' <summary>
    ''' OIS0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIS0001tbl_UPD()

        '○ 画面状態設定
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIS0001INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIS0001row As DataRow In OIS0001tbl.Rows
                If OIS0001row("TANKNUMBER") = OIS0001INProw("TANKNUMBER") AndAlso
                    OIS0001row("MODEL") = OIS0001INProw("MODEL") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIS0001row("DELFLG") = OIS0001INProw("DELFLG") AndAlso
                        OIS0001row("ORIGINOWNERCODE") = OIS0001INProw("ORIGINOWNERCODE") AndAlso
                        OIS0001row("OWNERCODE") = OIS0001INProw("OWNERCODE") AndAlso
                        OIS0001row("LEASECODE") = OIS0001INProw("LEASECODE") AndAlso
                        OIS0001row("LEASECLASS") = OIS0001INProw("LEASECLASS") AndAlso
                        OIS0001row("AUTOEXTENTION") = OIS0001INProw("AUTOEXTENTION") AndAlso
                        OIS0001row("LEASESTYMD") = OIS0001INProw("LEASESTYMD") AndAlso
                        OIS0001row("LEASEENDYMD") = OIS0001INProw("LEASEENDYMD") AndAlso
                        OIS0001row("USERCODE") = OIS0001INProw("USERCODE") AndAlso
                        OIS0001row("CURRENTSTATIONCODE") = OIS0001INProw("CURRENTSTATIONCODE") AndAlso
                        OIS0001row("EXTRADINARYSTATIONCODE") = OIS0001INProw("EXTRADINARYSTATIONCODE") AndAlso
                        OIS0001row("USERLIMIT") = OIS0001INProw("USERLIMIT") AndAlso
                        OIS0001row("LIMITTEXTRADIARYSTATION") = OIS0001INProw("LIMITTEXTRADIARYSTATION") AndAlso
                        OIS0001row("DEDICATETYPECODE") = OIS0001INProw("DEDICATETYPECODE") AndAlso
                        OIS0001row("EXTRADINARYTYPECODE") = OIS0001INProw("EXTRADINARYTYPECODE") AndAlso
                        OIS0001row("EXTRADINARYLIMIT") = OIS0001INProw("EXTRADINARYLIMIT") AndAlso
                        OIS0001row("OPERATIONBASECODE") = OIS0001INProw("OPERATIONBASECODE") AndAlso
                        OIS0001row("COLORCODE") = OIS0001INProw("COLORCODE") AndAlso
                        OIS0001row("ENEOS") = OIS0001INProw("ENEOS") AndAlso
                        OIS0001row("ECO") = OIS0001INProw("ECO") AndAlso
                        OIS0001row("ALLINSPECTIONDATE") = OIS0001INProw("ALLINSPECTIONDATE") AndAlso
                        OIS0001row("TRANSFERDATE") = OIS0001INProw("TRANSFERDATE") AndAlso
                        OIS0001row("OBTAINEDCODE") = OIS0001INProw("OBTAINEDCODE") AndAlso
                        OIS0001row("MODEL") = OIS0001INProw("MODEL") AndAlso
                        OIS0001row("MODELKANA") = OIS0001INProw("MODELKANA") AndAlso
                        OIS0001row("LOAD") = OIS0001INProw("LOAD") AndAlso
                        OIS0001row("LOADUNIT") = OIS0001INProw("LOADUNIT") AndAlso
                        OIS0001row("VOLUME") = OIS0001INProw("VOLUME") AndAlso
                        OIS0001row("VOLUMEUNIT") = OIS0001INProw("VOLUMEUNIT") AndAlso
                        OIS0001row("ORIGINOWNERNAME") = OIS0001INProw("ORIGINOWNERNAME") AndAlso
                        OIS0001row("OWNERNAME") = OIS0001INProw("OWNERNAME") AndAlso
                        OIS0001row("LEASENAME") = OIS0001INProw("LEASENAME") AndAlso
                        OIS0001row("LEASECLASSNEMAE") = OIS0001INProw("LEASECLASSNEMAE") AndAlso
                        OIS0001row("USERNAME") = OIS0001INProw("USERNAME") AndAlso
                        OIS0001row("CURRENTSTATIONNAME") = OIS0001INProw("CURRENTSTATIONNAME") AndAlso
                        OIS0001row("EXTRADINARYSTATIONNAME") = OIS0001INProw("EXTRADINARYSTATIONNAME") AndAlso
                        OIS0001row("DEDICATETYPENAME") = OIS0001INProw("DEDICATETYPENAME") AndAlso
                        OIS0001row("EXTRADINARYTYPENAME") = OIS0001INProw("EXTRADINARYTYPENAME") AndAlso
                        OIS0001row("OPERATIONBASENAME") = OIS0001INProw("OPERATIONBASENAME") AndAlso
                        OIS0001row("COLORNAME") = OIS0001INProw("COLORNAME") AndAlso
                        OIS0001row("RESERVE1") = OIS0001INProw("RESERVE1") AndAlso
                        OIS0001row("RESERVE2") = OIS0001INProw("RESERVE2") AndAlso
                        OIS0001row("SPECIFIEDDATE") = OIS0001INProw("SPECIFIEDDATE") AndAlso
                        OIS0001row("JRALLINSPECTIONDATE") = OIS0001INProw("JRALLINSPECTIONDATE") AndAlso
                        OIS0001row("PROGRESSYEAR") = OIS0001INProw("PROGRESSYEAR") AndAlso
                        OIS0001row("NEXTPROGRESSYEAR") = OIS0001INProw("NEXTPROGRESSYEAR") AndAlso
                        OIS0001row("JRINSPECTIONDATE") = OIS0001INProw("JRINSPECTIONDATE") AndAlso
                        OIS0001row("INSPECTIONDATE") = OIS0001INProw("INSPECTIONDATE") AndAlso
                        OIS0001row("JRSPECIFIEDDATE") = OIS0001INProw("JRSPECIFIEDDATE") AndAlso
                        OIS0001row("JRTANKNUMBER") = OIS0001INProw("JRTANKNUMBER") AndAlso
                        OIS0001row("OLDTANKNUMBER") = OIS0001INProw("OLDTANKNUMBER") AndAlso
                        OIS0001row("OTTANKNUMBER") = OIS0001INProw("OTTANKNUMBER") AndAlso
                        OIS0001row("JXTGTANKNUMBER") = OIS0001INProw("JXTGTANKNUMBER") AndAlso
                        OIS0001row("COSMOTANKNUMBER") = OIS0001INProw("COSMOTANKNUMBER") AndAlso
                        OIS0001row("FUJITANKNUMBER") = OIS0001INProw("FUJITANKNUMBER") AndAlso
                        OIS0001row("SHELLTANKNUMBER") = OIS0001INProw("SHELLTANKNUMBER") AndAlso
                        OIS0001row("RESERVE3") = OIS0001INProw("RESERVE3") AndAlso
                        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIS0001INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows
            Select Case OIS0001INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIS0001INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIS0001INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIS0001INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIS0001INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("TANKNUMBER") = OIS0001row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIS0001INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIS0001row As DataRow = OIS0001tbl.NewRow
        OIS0001row.ItemArray = OIS0001INProw.ItemArray

        OIS0001row("LINECNT") = OIS0001tbl.Rows.Count + 1
        If OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIS0001row("UPDTIMSTP") = "0"
        OIS0001row("SELECT") = 1
        OIS0001row("HIDDEN") = 0

        OIS0001tbl.Rows.Add(OIS0001row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("TANKNUMBER") = OIS0001row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="I_SUBCODE"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal I_SUBCODE As String = "1")

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFixValueParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

''************************************************************
' 組織マスタメンテ登録画面
' 作成日 2020/5/26
' 更新日 2020/10/26
' 作成者 JOT杉山
' 更新車 JOT杉山
'
' 修正履歴:新規作成
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 組織マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0002OrgList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0002tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0002INPtbl As DataTable                              'チェック用テーブル
    Private OIM0002UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0002tbl)

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
            If Not IsNothing(OIM0002tbl) Then
                OIM0002tbl.Clear()
                OIM0002tbl.Dispose()
                OIM0002tbl = Nothing
            End If

            If Not IsNothing(OIM0002INPtbl) Then
                OIM0002INPtbl.Clear()
                OIM0002INPtbl.Dispose()
                OIM0002INPtbl = Nothing
            End If

            If Not IsNothing(OIM0002UPDtbl) Then
                OIM0002UPDtbl.Clear()
                OIM0002UPDtbl.Dispose()
                OIM0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0002WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0002S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '######### おためし ##########################
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0002C Then
            Master.RecoverTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)
        End If
        '20200615杉山テスト的に修正　2に変更
        '○ 名称設定処理
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE2.Text, work.WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("ORGCODE", work.WF_SEL_ORGCODE2.Text, work.WF_SEL_ORGNAME.Text, WW_DUMMY)                '組織コード

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '######### おためし ##########################
        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0002C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0002tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0002tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
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

        If IsNothing(OIM0002tbl) Then
            OIM0002tbl = New DataTable
        End If

        If OIM0002tbl.Columns.Count <> 0 Then
            OIM0002tbl.Columns.Clear()
        End If

        OIM0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを組織マスタから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                 AS LINECNT" _
            & " , ''                                                AS OPERATION" _
            & " , CAST(OIM0002.UPDTIMSTP AS bigint)                 AS TIMSTP" _
            & " , 1                                                 AS 'SELECT'" _
            & " , 0                                                 AS HIDDEN" _
            & " , ISNULL(RTRIM(OIM0002.CAMPCODE), '')               AS CAMPCODE" _
            & " , ''                                                AS CAMPNAME" _
            & " , ISNULL(RTRIM(OIM0002.ORGCODE), '')                AS ORGCODE" _
            & " , ISNULL(FORMAT(OIM0002.STYMD, 'yyyy/MM/dd'), '')   AS STYMD " _
            & " , ISNULL(FORMAT(OIM0002.ENDYMD, 'yyyy/MM/dd'), '')  AS ENDYMD " _
            & " , ISNULL(RTRIM(OIM0002.NAME), '')                   AS NAME" _
            & " , ISNULL(RTRIM(OIM0002.NAMES), '')                  AS NAMES" _
            & " , ISNULL(RTRIM(OIM0002.NAMEKANA), '')               AS NAMEKANA" _
            & " , ISNULL(RTRIM(OIM0002.NAMEKANAS), '')              AS NAMEKANAS" _
            & " , ISNULL(RTRIM(OIM0002.DELFLG), '')                 AS DELFLG" _
            & " FROM OIL.OIM0002_ORG OIM0002 " _
            & " WHERE OIM0002.DELFLG = @P3"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '会社コード
        If Not String.IsNullOrEmpty(work.WF_SEL_CAMPCODE2.Text) Then
            SQLStr &= String.Format("    AND OIM0002.CAMPCODE Like '{0}'", work.WF_SEL_CAMPCODE2.Text)
        End If
        '組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORGCODE2.Text) Then
            'SQLStr &= String.Format("    AND OIM0002.ORGCODE = '{0}'", work.WF_SEL_ORGCODE.Text)
            SQLStr &= String.Format("    AND OIM0002.ORGCODE like '{0}'", work.WF_SEL_ORGCODE2.Text)
        End If
        '検索条件
        'If Not String.IsNullOrEmpty(work.WF_SEL_SELECT.Text) Then
        '    SQLStr &= String.Format("    AND OIM0002.DELFLG like '{0}'", work.WF_SEL_SELECT.Text)
        'End If

        SQLStr &=
              " ORDER BY" _
            & "    OIM0002.CAMPCODE" _
            & "    , OIM0002.ORGCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                'Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)        '会社コード
                'Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 3)        '組織コード
                'Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)        '削除フラグ
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)

                'PARA1.Value = work.WF_SEL_CAMPCODE.Text + "%"
                'PARA1.Value = work.WF_SEL_CAMPCODE.Text
                'PARA2.Value = work.WF_SEL_ORGCODE.Text
                'PARA3.Value = C_DELETE_FLG.DELETE
                PARA1.Value = work.WF_SEL_SELECT.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0002row As DataRow In OIM0002tbl.Rows
                    i += 1
                    OIM0002row("LINECNT") = i        'LINECNT

                    '発着駅フラグ
                    'CODENAME_get("DEPARRSTATIONFLG", OIM0002row("DEPARRSTATIONFLG"), OIM0002row("DEPARRSTATIONNAME"), WW_DUMMY)
                    '会社コード
                    CODENAME_get("CAMPCODE", OIM0002row("CAMPCODE"), OIM0002row("CAMPNAME"), WW_DUMMY)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0002L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0002L Select"
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
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            If OIM0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0002tbl)

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
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
        'WF_Sel_LINECNT.Text = ""
        work.WF_SEL_LINECNT.Text = ""

        '会社コード
        'TxtCampCode.Text = ""
        work.WF_SEL_CAMPCODE2.Text = ""

        '組織コード
        'TxtOrgCode.Text = ""
        work.WF_SEL_ORGCODE2.Text = ""

        '組織名
        'TxtCampName.Text = ""
        work.WF_SEL_ORGNAME.Text = ""

        '組織名（短）
        'TxtCampNameKana.Text = ""
        work.WF_SEL_ORGNAMES.Text = ""

        '組織名カナ
        'TxtTypeName.Text = ""
        work.WF_SEL_ORGNAMEKANA.Text = ""

        '組織名カナ（短）
        'TxtTypeNameKana.Text = ""
        work.WF_SEL_ORGNAMEKANAS.Text = ""

        '開始年月日
        work.WF_SEL_STYMD.Text = ""

        '終了年月日
        work.WF_SEL_ENDYMD.Text = ""

        '削除フラグ
        work.WF_SEL_SELECT.Text = "0"

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0002tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

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

        '○同一レコードチェック
        '※開始終了期間を持っていないため現状意味無し
        'For Each OIM0002row As DataRow In OIM0002tbl.Rows
        '    '読み飛ばし
        '    If OIM0002row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING OrElse
        '        OIM0002row("DELFLG") = C_DELETE_FLG.DELETE Then
        '        Continue For
        '    End If

        '    WW_LINEERR_SW = ""

        '    '期間重複チェック
        '    For Each checkRow As DataRow In OIM0002tbl.Rows
        '        '同一KEY以外は読み飛ばし
        '        If checkRow("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
        '            checkRow("ORGCODE") = OIM0002row("ORGCODE") AndAlso
        '            checkRow("MODELPATTERN") = OIM0002row("MODELPATTERN") AndAlso
        '            checkRow("TORICODES") = OIM0002row("TORICODES") AndAlso
        '            checkRow("SHUKABASHO") = OIM0002row("SHUKABASHO") AndAlso
        '            checkRow("TORICODET") = OIM0002row("TORICODET") AndAlso
        '            checkRow("TODOKECODE") = OIM0002row("TODOKECODE") Then
        '        Else
        '            Continue For
        '        End If
        '    Next

        '    If WW_LINEERR_SW = "" Then
        '        If OIM0002row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        '            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        End If
        '    Else
        '        OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End If
        'Next

    End Sub


    ''' <summary>
    ''' 会社マスタ登録更新
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
            & "        OIL.OIM0002_ORG" _
            & "    WHERE" _
            & "        CAMPCODE           = @P1" _
            & "        AND ORGCODE        = @P2 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0002_ORG" _
            & "    SET" _
            & "          STYMD            = @P3  , ENDYMD          = @P4" _
            & "        , NAME             = @P5  , NAMES           = @P6" _
            & "        , NAMEKANA         = @P7  , NAMEKANAS       = @P8" _
            & "        , DELFLG           = @P9" _
            & "        , UPDYMD           = @P13 , UPDUSER         = @P14 , UPDTERMID = @P15" _
            & "        , RECEIVEYMD       = @P16" _
            & "    WHERE" _
            & "        CAMPCODE           = @P1" _
            & "        AND ORGCODE        = @P2 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0002_ORG" _
            & "        ( CAMPCODE   , ORGCODE " _
            & "        , STYMD      , ENDYMD       , NAME          , NAMES" _
            & "        , NAMEKANA   , NAMEKANAS    ,  DELFLG" _
            & "        , INITYMD    , INITUSER     , INITTERMID" _
            & "        , UPDYMD     , UPDUSER      , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P1  , @P2" _
            & "        , @P3  , @P4 , @P5  , @P6" _
            & "        , @P7  , @P8 , @P9" _
            & "        , @P10 , @P11 ,@P12" _
            & "        , @P13 , @P14, @P15" _
            & "        , @P16) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , ORGCODE" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , NAME" _
            & "    , NAMES" _
            & "    , NAMEKANA" _
            & "    , NAMEKANAS" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    OIL.OIM0002_ORG" _
            & " WHERE" _
            & "        CAMPCODE      = @P1" _
            & "        AND ORGCODE       = @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 2)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 6)            '組織コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.DateTime)               '開始年月日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.DateTime)               '終了年月日
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 200)          '組織名称
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 100)          '組織名称（短）
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 100)          '組織名称カナ
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 100)          '組織名称カナ（短）
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)             '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)         '登録ユーザーID
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)         '登録端末
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.DateTime)             '更新年月日
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)         '更新ユーザーID
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)         '更新端末
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)             '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 2)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 6)        '組織コード

                For Each OIM0002row As DataRow In OIM0002tbl.Rows
                    If Trim(OIM0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0002row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        '                        Trim(OIM0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = OIM0002row("CAMPCODE")
                        PARA2.Value = OIM0002row("ORGCODE")
                        PARA3.Value = OIM0002row("STYMD")
                        PARA4.Value = OIM0002row("ENDYMD")
                        PARA5.Value = OIM0002row("NAME")
                        PARA6.Value = OIM0002row("NAMES")
                        PARA7.Value = OIM0002row("NAMEKANA")
                        PARA8.Value = OIM0002row("NAMEKANAS")
                        PARA9.Value = OIM0002row("DELFLG")
                        PARA10.Value = WW_DATENOW
                        PARA11.Value = Master.USERID
                        PARA12.Value = Master.USERTERMID
                        PARA13.Value = WW_DATENOW
                        PARA14.Value = Master.USERID
                        PARA15.Value = Master.USERTERMID
                        PARA16.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = OIM0002row("CAMPCODE")
                        JPARA2.Value = OIM0002row("ORGCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0002UPDtbl) Then
                                OIM0002UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0002UPDtbl.Clear()
                            OIM0002UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0002UPDrow As DataRow In OIM0002UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0002L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0002L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0002L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0002tbl                       'データ参照  Table
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
        CS0030REPORT.TBLDATA = OIM0002tbl                       'データ参照Table
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
    ''' 終了ボタン押下時処理
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
        Dim TBLview As New DataView(OIM0002tbl)
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
        'WF_Sel_LINECNT.Text = OIM0002tbl.Rows(WW_LINECNT)("LINECNT")
        work.WF_SEL_LINECNT.Text = OIM0002tbl.Rows(WW_LINECNT)("LINECNT")

        '会社コード
        'TxtCampCode.Text = OIM0002tbl.Rows(WW_LINECNT)("CAMPCODE")
        work.WF_SEL_CAMPCODE2.Text = OIM0002tbl.Rows(WW_LINECNT)("CAMPCODE")

        '組織コード
        'TxtOrgCode.Text = OIM0002tbl.Rows(WW_LINECNT)("ORGCODE")
        work.WF_SEL_ORGCODE2.Text = OIM0002tbl.Rows(WW_LINECNT)("ORGCODE")

        '組織名
        'TxtCampName.Text = OIM0002tbl.Rows(WW_LINECNT)("CAMPNAME")
        work.WF_SEL_ORGNAME.Text = OIM0002tbl.Rows(WW_LINECNT)("NAME")

        '組織名（短）
        'TxtCampNameKana.Text = OIM0002tbl.Rows(WW_LINECNT)("CAMPNAMEKANA")
        work.WF_SEL_ORGNAMES.Text = OIM0002tbl.Rows(WW_LINECNT)("NAMES")

        '組織名カナ
        'TxtTypeName.Text = OIM0002tbl.Rows(WW_LINECNT)("TYPENAME")
        work.WF_SEL_ORGNAMEKANA.Text = OIM0002tbl.Rows(WW_LINECNT)("NAMEKANA")

        '組織名カナ（短）
        'TxtTypeNameKana.Text = OIM0002tbl.Rows(WW_LINECNT)("TYPENAMEKANA")
        work.WF_SEL_ORGNAMEKANAS.Text = OIM0002tbl.Rows(WW_LINECNT)("NAMEKANAS")

        '開始年月日
        work.WF_SEL_STYMD.Text = OIM0002tbl.Rows(WW_LINECNT)("STYMD")

        '終了年月日
        work.WF_SEL_ENDYMD.Text = OIM0002tbl.Rows(WW_LINECNT)("ENDYMD")

        '削除フラグ
        work.WF_SEL_SELECT.Text = OIM0002tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            Select Case OIM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIM0002tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0002tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0002INProw As DataRow = OIM0002INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0002INPcol As DataColumn In OIM0002INPtbl.Columns
                If IsDBNull(OIM0002INProw.Item(OIM0002INPcol)) OrElse IsNothing(OIM0002INProw.Item(OIM0002INPcol)) Then
                    Select Case OIM0002INPcol.ColumnName
                        Case "LINECNT"
                            OIM0002INProw.Item(OIM0002INPcol) = 0
                        Case "OPERATION"
                            OIM0002INProw.Item(OIM0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            OIM0002INProw.Item(OIM0002INPcol) = 0
                        Case "SELECT"
                            OIM0002INProw.Item(OIM0002INPcol) = 1
                        Case "HIDDEN"
                            OIM0002INProw.Item(OIM0002INPcol) = 0
                        Case Else
                            OIM0002INProw.Item(OIM0002INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ORGCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CAMPNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("CAMPNAMEKANA") >= 0 Then
                For Each OIM0002row As DataRow In OIM0002tbl.Rows
                    If XLSTBLrow("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
                        XLSTBLrow("ORGCODE") = OIM0002row("ORGCODE") AndAlso
                        XLSTBLrow("CAMPNAME") = OIM0002row("CAMPNAME") AndAlso
                        XLSTBLrow("CAMPNAMEKANA") = OIM0002row("CAMPNAMEKANA") Then
                        OIM0002INProw.ItemArray = OIM0002row.ItemArray
                        Exit For
                    End If
                Next
            End If
            '○ 項目セット
            '会社コード
            OIM0002INProw.Item("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

        '組織コード
        OIM0002INProw.Item("ORGCODE") = work.WF_SEL_ORGCODE.Text

        '会社名称
        If WW_COLUMNS.IndexOf("CAMPNAME") >= 0 Then
            OIM0002INProw("CAMPNAME") = XLSTBLrow("CAMPNAME")
        End If

        '会社名称カナ
        If WW_COLUMNS.IndexOf("CAMPNAMEKANA") >= 0 Then
            OIM0002INProw("CAMPNAMEKANA") = XLSTBLrow("CAMPNAMEKANA")
        End If

        OIM0002INPtbl.Rows.Add(OIM0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

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

    '''' <summary>
    '''' 詳細画面-表更新ボタン押下時処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub WF_UPDATE_Click()

    '    '○ エラーレポート準備
    '    rightview.SetErrorReport("")

    '    '○ DetailBoxをINPtblへ退避
    '    DetailBoxToOIM0002INPtbl(WW_ERR_SW)
    '    If Not isNormal(WW_ERR_SW) Then
    '        Exit Sub
    '    End If

    '    '○ 項目チェック
    '    INPTableCheck(WW_ERR_SW)

    '    '○ 入力値のテーブル反映
    '    If isNormal(WW_ERR_SW) Then
    '        OIM0002tbl_UPD()
    '    End If

    '    '○ 画面表示データ保存
    '    Master.SaveTable(OIM0002tbl)

    '    '○ 詳細画面初期化
    '    If isNormal(WW_ERR_SW) Then
    '        DetailBoxClear()
    '    End If

    '    '○ メッセージ表示
    '    If WW_ERR_SW = "" Then
    '        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
    '    Else
    '        If isNormal(WW_ERR_SW) Then
    '            Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    '        Else
    '            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
    '        End If


    '    End If

    '    '○画面切替設定
    '    WF_BOXChange.Value = "headerbox"

    'End Sub

    '''' <summary>
    '''' 詳細画面-テーブル退避
    '''' </summary>
    '''' <param name="O_RTN"></param>
    '''' <remarks></remarks>
    'Protected Sub DetailBoxToOIM0002INPtbl(ByRef O_RTN As String)

    '    O_RTN = C_MESSAGE_NO.NORMAL

    '    '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
    '    Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除

    '    '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
    '    If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
    '        String.IsNullOrEmpty(WF_DELFLG.Text) Then
    '        Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

    '        CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
    '        CS0011LOGWrite.INFPOSI = "non Detail"
    '        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
    '        CS0011LOGWrite.TEXT = "non Detail"
    '        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
    '        CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

    '        O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
    '        Exit Sub
    '    End If

    '    Master.CreateEmptyTable(OIM0002INPtbl)
    '    Dim OIM0002INProw As DataRow = OIM0002INPtbl.NewRow

    '    '○ 初期クリア
    '    For Each OIM0002INPcol As DataColumn In OIM0002INPtbl.Columns
    '        If IsDBNull(OIM0002INProw.Item(OIM0002INPcol)) OrElse IsNothing(OIM0002INProw.Item(OIM0002INPcol)) Then
    '            Select Case OIM0002INPcol.ColumnName
    '                Case "LINECNT"
    '                    OIM0002INProw.Item(OIM0002INPcol) = 0
    '                Case "OPERATION"
    '                    OIM0002INProw.Item(OIM0002INPcol) = C_LIST_OPERATION_CODE.NODATA
    '                Case "TIMSTP"
    '                    OIM0002INProw.Item(OIM0002INPcol) = 0
    '                Case "SELECT"
    '                    OIM0002INProw.Item(OIM0002INPcol) = 1
    '                Case "HIDDEN"
    '                    OIM0002INProw.Item(OIM0002INPcol) = 0
    '                Case Else
    '                    OIM0002INProw.Item(OIM0002INPcol) = ""
    '            End Select
    '        End If
    '    Next

    '    'LINECNT
    '    If WF_Sel_LINECNT.Text = "" Then
    '        OIM0002INProw("LINECNT") = 0
    '    Else
    '        Try
    '            Integer.TryParse(WF_Sel_LINECNT.Text, OIM0002INProw("LINECNT"))
    '        Catch ex As Exception
    '            OIM0002INProw("LINECNT") = 0
    '        End Try
    '    End If

    '    OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
    '    OIM0002INProw("TIMSTP") = 0
    '    OIM0002INProw("SELECT") = 1
    '    OIM0002INProw("HIDDEN") = 0

    '    'OIM0002INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text        '会社コード
    '    'OIM0002INProw("ORGCODE") = work.WF_SEL_ORGCODE.Text                '組織コード

    '    OIM0002INProw("DELFLG") = WF_DELFLG.Text                     '削除

    '    OIM0002INProw("CAMPCODE") = TxtCampCode.Text           '会社コード
    '    OIM0002INProw("ORGCODE") = TxtOrgCode.Text                     '組織コード
    '    OIM0002INProw("CAMPNAME") = TxtCampName.Text            '会社名称
    '    OIM0002INProw("CAMPNAMEKANA") = TxtCampNameKana.Text   '会社名称カナ
    '    OIM0002INProw("TypeName") = TxtTypeName.Text                 '会社種別名称
    '    OIM0002INProw("TYPENAMEKANA") = TxtTypeNameKana.Text         '会社種別名称カナ

    '    '○ チェック用テーブルに登録する
    '    OIM0002INPtbl.Rows.Add(OIM0002INProw)

    'End Sub


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
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            Select Case OIM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0002tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT
        'TxtCampCode.Text = ""            '会社コード
        'TxtOrgCode.Text = ""                 '組織コード
        'TxtCampName.Text = ""            '会社名称
        'TxtCampNameKana.Text = ""        '会社名称カナ
        'TxtTypeName.Text = ""               '会社種別名称
        'TxtTypeNameKana.Text = ""           '会社種別名称カナ
        'WF_DELFLG.Text = ""                 '削除
        'WF_DELFLG_TEXT.Text = ""            '削除名称

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

    '''' <summary>
    '''' LeftBox選択時処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub WF_ButtonSel_Click()

    '    Dim WW_SelectValue As String = ""
    '    Dim WW_SelectText As String = ""

    '    '○ 選択内容を取得
    '    If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
    '        WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
    '        WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
    '        WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
    '    End If

    '    '○ 選択内容を画面項目へセット
    '    If WF_FIELD_REP.Value = "" Then
    '        Select Case WF_FIELD.Value
    '                '会社コード
    '            Case "CAMPCODE"
    '                TxtCampCode.Text = WW_SelectValue
    '                LblCampCodeText.Text = WW_SelectText
    '                TxtCampCode.Focus()

    '                '組織コード
    '            Case "ORGCODE"
    '                TxtOrgCode.Text = WW_SelectValue
    '                LblOrgCodeText.Text = WW_SelectText
    '                TxtOrgCode.Focus()

    '                '会社名称
    '            Case "CAMPNAME"
    '                TxtCampName.Text = WW_SelectValue
    '                LblCampNameText.Text = WW_SelectText
    '                TxtCampName.Focus()

    '                '会社名称カナ
    '            Case "CAMPNAMEKANA"
    '                TxtCampNameKana.Text = WW_SelectValue
    '                LblCampNameKanaText.Text = WW_SelectText
    '                TxtCampNameKana.Focus()

    '               '削除
    '            Case "WF_DELFLG"
    '                WF_DELFLG.Text = WW_SelectValue
    '                WF_DELFLG_TEXT.Text = WW_SelectText
    '                WF_DELFLG.Focus()

    '        End Select
    '    Else
    '    End If

    '    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
    '    WF_FIELD.Value = ""
    '    WF_FIELD_REP.Value = ""
    '    WF_LeftboxOpen.Value = ""
    '    WF_RightboxOpen.Value = ""

    'End Sub

    '''' <summary>
    '''' LeftBoxキャンセルボタン押下時処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub WF_ButtonCan_Click()

    '    '○ フォーカスセット
    '    If WF_FIELD_REP.Value = "" Then
    '        Select Case WF_FIELD.Value
    '                '会社コード
    '            Case "CAMPCODE"
    '                TxtCampCode.Focus()

    '                '組織コード
    '            Case "ORGCODE"
    '                TxtOrgCode.Focus()

    '                '会社名称
    '            Case "CAMPNAME"
    '                TxtCampName.Focus()

    '                '会社名称カナ
    '            Case "CAMPNAMEKANA"
    '                TxtCampNameKana.Focus()

    '            '削除
    '            Case "WF_DELFLG"
    '                WF_DELFLG.Focus()

    '        End Select
    '    Else
    '    End If

    '    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
    '    WF_FIELD.Value = ""
    '    WF_FIELD_REP.Value = ""
    '    WF_LeftboxOpen.Value = ""
    '    WF_RightboxOpen.Value = ""

    'End Sub


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
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CAMPCODE", OIM0002INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("CAMPCODE", OIM0002INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORGCODE", OIM0002INProw("ORGCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("ORGCODE", OIM0002INProw("ORGCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(組織コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(組織コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0002INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード       =" & OIM0002row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織コード     =" & OIM0002row("ORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社名称         =" & OIM0002row("CAMPNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社名称カナ     =" & OIM0002row("CAMPNAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0002row("DELFLG")
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
    ''' OIM0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0002tbl_UPD()

        '○ 画面状態設定
        For Each OIM0002row As DataRow In OIM0002tbl.Rows
            Select Case OIM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0002INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0002row As DataRow In OIM0002tbl.Rows
                If OIM0002row("CAMPCODE") = OIM0002INProw("CAMPCODE") AndAlso
                    OIM0002row("ORGCODE") = OIM0002INProw("ORGCODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0002row("DELFLG") = OIM0002INProw("DELFLG") AndAlso
                        OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0002INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0002INProw As DataRow In OIM0002INPtbl.Rows
            Select Case OIM0002INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0002INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0002INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0002INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0002INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0002INProw As DataRow)

        For Each OIM0002row As DataRow In OIM0002tbl.Rows

            '同一レコードか判定
            If OIM0002INProw("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
                OIM0002INProw("ORGCODE") = OIM0002row("ORGCODE") Then
                '画面入力テーブル項目設定
                OIM0002INProw("LINECNT") = OIM0002row("LINECNT")
                OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0002INProw("TIMSTP") = OIM0002row("TIMSTP")
                OIM0002INProw("SELECT") = 1
                OIM0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0002row.ItemArray = OIM0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0002INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0002row As DataRow = OIM0002tbl.NewRow
        OIM0002row.ItemArray = OIM0002INProw.ItemArray

        OIM0002row("LINECNT") = OIM0002tbl.Rows.Count + 1
        If OIM0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0002row("TIMSTP") = "0"
        OIM0002row("SELECT") = 1
        OIM0002row("HIDDEN") = 0

        OIM0002tbl.Rows.Add(OIM0002row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0002INProw As DataRow)

        For Each OIM0002row As DataRow In OIM0002tbl.Rows

            '同一レコードか判定
            If OIM0002INProw("CAMPCODE") = OIM0002row("CAMPCODE") AndAlso
               OIM0002INProw("ORGCODE") = OIM0002row("ORGCODE") Then
                '画面入力テーブル項目設定
                OIM0002INProw("LINECNT") = OIM0002row("LINECNT")
                OIM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0002INProw("TIMSTP") = OIM0002row("TIMSTP")
                OIM0002INProw("SELECT") = 1
                OIM0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0002row.ItemArray = OIM0002INProw.ItemArray
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
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

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
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    Dim AUTHORITYALL_FLG As String = "0"
                    If work.WF_SEL_CAMPCODE.Text = "" Then '会社コードが空の場合
                        AUTHORITYALL_FLG = "1"
                    Else '会社コードに入力済みの場合
                        AUTHORITYALL_FLG = "2"
                    End If
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text, AUTHORITYALL_FLG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORGCODE"             '組織コード
                    Dim AUTHORITYALL_FLG As String = "0"
                    If work.WF_SEL_CAMPCODE2.Text = "" Then '会社コードが空の場合
                        AUTHORITYALL_FLG = "1"
                    Else '会社コードに入力済みの場合
                        AUTHORITYALL_FLG = "2"
                    End If
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE2.Text, AUTHORITYALL_FLG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

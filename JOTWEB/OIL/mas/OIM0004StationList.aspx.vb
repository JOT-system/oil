﻿''************************************************************
' 貨物駅マスタメンテ登録画面
' 作成日 2019/10/29
' 更新日 2021/04/09
' 作成者 JOT森川
' 更新者 JOT伊草
'
' 修正履歴:2019/10/19 新規作成
'         :2021/04/09 1)項目「発着駅フラグ」が表示されていなかった為
'         :             横スクロールバーを出すように修正
'         :             また、上記項目のバリデーション処理を追加
'         :           2)登録・更新画面にて更新メッセージが設定された場合
'         :             画面下部に更新メッセージを表示するように修正
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 貨物駅マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0004StationList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0004tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0004INPtbl As DataTable                              'チェック用テーブル
    Private OIM0004UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0004tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
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
            If Not IsNothing(OIM0004tbl) Then
                OIM0004tbl.Clear()
                OIM0004tbl.Dispose()
                OIM0004tbl = Nothing
            End If

            If Not IsNothing(OIM0004INPtbl) Then
                OIM0004INPtbl.Clear()
                OIM0004INPtbl.Dispose()
                OIM0004INPtbl = Nothing
            End If

            If Not IsNothing(OIM0004UPDtbl) Then
                OIM0004UPDtbl.Clear()
                OIM0004UPDtbl.Dispose()
                OIM0004UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0004WRKINC.MAPIDL
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

        '〇 更新画面からの遷移の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0004S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0004C Then
            Master.RecoverTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)
        End If

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_SELUORG_TEXT.Text, WW_DUMMY)                     '運用部署

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0004C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0004tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0004tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
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

        If IsNothing(OIM0004tbl) Then
            OIM0004tbl = New DataTable
        End If

        If OIM0004tbl.Columns.Count <> 0 Then
            OIM0004tbl.Columns.Clear()
        End If

        OIM0004tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを貨物車マスタから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                           AS LINECNT" _
            & " , ''                                          AS OPERATION" _
            & " , CAST(OIM0004.UPDTIMSTP AS bigint)           AS TIMSTP" _
            & " , 1                                           AS 'SELECT'" _
            & " , 0                                           AS HIDDEN" _
            & " , ISNULL(RTRIM(OIM0004.STATIONCODE), '')      AS STATIONCODE" _
            & " , ISNULL(RTRIM(OIM0004.BRANCH), '')           AS BRANCH" _
            & " , ISNULL(RTRIM(OIM0004.STATONNAME), '')       AS STATONNAME" _
            & " , ISNULL(RTRIM(OIM0004.STATIONNAMEKANA), '')  AS STATIONNAMEKANA" _
            & " , ISNULL(RTRIM(OIM0004.TYPENAME), '')         AS TYPENAME" _
            & " , ISNULL(RTRIM(OIM0004.TYPENAMEKANA), '')     AS TYPENAMEKANA" _
            & " , ISNULL(RTRIM(OIM0004.DEPARRSTATIONFLG), '') AS DEPARRSTATIONFLG" _
            & " , ''                                          AS DEPARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIM0004.DELFLG), '')           AS DELFLG" _
            & " FROM OIL.OIM0004_STATION OIM0004 " _
            & " WHERE OIM0004.DELFLG <> @P3"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '貨物駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_STATIONCODE.Text) Then
            SQLStr &= String.Format("    AND OIM0004.STATIONCODE like '{0}%'", work.WF_SEL_STATIONCODE.Text)
        End If
        '貨物コード枝番
        If Not String.IsNullOrEmpty(work.WF_SEL_BRANCH.Text) Then
            SQLStr &= String.Format("    AND OIM0004.BRANCH like '%{0}%'", work.WF_SEL_BRANCH.Text)
        End If
        '発着駅フラグ
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPARRSTATIONFLG.Text) Then
            SQLStr &= String.Format("    AND OIM0004.DEPARRSTATIONFLG like '{0}'", work.WF_SEL_DEPARRSTATIONFLG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIM0004.STATIONCODE" _
            & "    , OIM0004.BRANCH"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)        '削除フラグ

                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0004tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0004tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0004row As DataRow In OIM0004tbl.Rows
                    i += 1
                    OIM0004row("LINECNT") = i        'LINECNT

                    '発着駅フラグ
                    CODENAME_get("DEPARRSTATIONFLG", OIM0004row("DEPARRSTATIONFLG"), OIM0004row("DEPARRSTATIONNAME"), WW_DUMMY)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0004L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0004L Select"
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
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            If OIM0004row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0004row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0004tbl)

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
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
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
        work.WF_SEL_LINECNT.Text = ""

        '貨物車コード
        work.WF_SEL_STATIONCODE2.Text = ""

        '貨物コード枝番
        work.WF_SEL_BRANCH2.Text = ""

        '貨物駅名称
        work.WF_SEL_STATONNAME.Text = ""

        '貨物駅名称カナ
        work.WF_SEL_STATIONNAMEKANA.Text = ""

        '貨物駅種別名称
        work.WF_SEL_TYPENAME.Text = ""

        '貨物駅種別名称
        work.WF_SEL_TYPENAMEKANA.Text = ""

        '発着駅フラグ
        work.WF_SEL_DEPARRSTATIONFLG2.Text = ""

        '削除
        work.WF_SEL_DELFLG.Text = "0"

        '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

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

        ''○関連チェック
        'RelatedCheck(WW_ERRCODE)

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'マスタ更新
                UpdateMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

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
        'For Each OIM0004row As DataRow In OIM0004tbl.Rows
        '    '読み飛ばし
        '    If OIM0004row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING OrElse
        '        OIM0004row("DELFLG") = C_DELETE_FLG.DELETE Then
        '        Continue For
        '    End If

        '    WW_LINEERR_SW = ""

        '    '期間重複チェック
        '    For Each checkRow As DataRow In OIM0004tbl.Rows
        '        '同一KEY以外は読み飛ばし
        '        If checkRow("CAMPCODE") = OIM0004row("CAMPCODE") AndAlso
        '            checkRow("UORG") = OIM0004row("UORG") AndAlso
        '            checkRow("MODELPATTERN") = OIM0004row("MODELPATTERN") AndAlso
        '            checkRow("TORICODES") = OIM0004row("TORICODES") AndAlso
        '            checkRow("SHUKABASHO") = OIM0004row("SHUKABASHO") AndAlso
        '            checkRow("TORICODET") = OIM0004row("TORICODET") AndAlso
        '            checkRow("TODOKECODE") = OIM0004row("TODOKECODE") Then
        '        Else
        '            Continue For
        '        End If
        '    Next

        '    If WW_LINEERR_SW = "" Then
        '        If OIM0004row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        '            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        End If
        '    Else
        '        OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End If
        'Next

    End Sub

    ''' <summary>
    ''' 貨物駅マスタ登録更新
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
            & "        OIL.OIM0004_STATION" _
            & "    WHERE" _
            & "        STATIONCODE      = @P1" _
            & "        AND BRANCH       = @P2 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0004_STATION" _
            & "    SET" _
            & "        STATONNAME         = @P3  , STATIONNAMEKANA = @P4" _
            & "        , TYPENAME         = @P5  , TYPENAMEKANA    = @P6" _
            & "        , DEPARRSTATIONFLG = @P15 , DELFLG          = @P7" _
            & "        , UPDYMD           = @P11 , UPDUSER         = @P12 , UPDTERMID = @P13" _
            & "        , RECEIVEYMD       = @P14" _
            & "    WHERE" _
            & "        STATIONCODE       = @P1" _
            & "        AND BRANCH       = @P2 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0004_STATION" _
            & "        ( STATIONCODE, BRANCH       , STATONNAME       , STATIONNAMEKANA" _
            & "        , TYPENAME   , TYPENAMEKANA , DEPARRSTATIONFLG , DELFLG" _
            & "        , INITYMD    , INITUSER     , INITTERMID" _
            & "        , UPDYMD     , UPDUSER      , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P1  , @P2 , @P3  , @P4" _
            & "        , @P5  , @P6 , @P15 , @P7" _
            & "        , @P8  , @P9 , @P10" _
            & "        , @P11 , @P12, @P13" _
            & "        , @P14) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    STATIONCODE" _
            & "    , BRANCH" _
            & "    , STATONNAME" _
            & "    , STATIONNAMEKANA" _
            & "    , TYPENAME" _
            & "    , TYPENAMEKANA" _
            & "    , DEPARRSTATIONFLG" _
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
            & "    OIL.OIM0004_STATION" _
            & " WHERE" _
            & "        STATIONCODE      = @P1" _
            & "        AND BRANCH       = @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '貨物駅コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 3)            '貨物コード枝番
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 200)          '貨物駅名称
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 100)          '貨物駅名称カナ
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 40)           '貨物駅種別名称
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           '貨物駅種別名称カナ
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)          '発着駅フラグ
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)               '登録年月日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)           '登録ユーザーID
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)         '登録端末
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)             '更新年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)         '更新ユーザーID
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)         '更新端末
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)             '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 4)        '貨物駅コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 3)        '貨物コード枝番

                For Each OIM0004row As DataRow In OIM0004tbl.Rows
                    If Trim(OIM0004row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0004row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0004row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then

                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = OIM0004row("STATIONCODE")
                        PARA2.Value = OIM0004row("BRANCH")
                        PARA3.Value = OIM0004row("STATONNAME")
                        PARA4.Value = OIM0004row("STATIONNAMEKANA")
                        PARA5.Value = OIM0004row("TYPENAME")
                        PARA6.Value = OIM0004row("TYPENAMEKANA")
                        PARA15.Value = OIM0004row("DEPARRSTATIONFLG")
                        PARA7.Value = OIM0004row("DELFLG")
                        PARA8.Value = WW_DATENOW
                        PARA9.Value = Master.USERID
                        PARA10.Value = Master.USERTERMID
                        PARA11.Value = WW_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = OIM0004row("STATIONCODE")
                        JPARA2.Value = OIM0004row("BRANCH")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0004UPDtbl) Then
                                OIM0004UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0004UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0004UPDtbl.Clear()
                            OIM0004UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0004UPDrow As DataRow In OIM0004UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0004L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0004UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0004L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0004L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0004tbl                       'データ参照  Table
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
        CS0030REPORT.TBLDATA = OIM0004tbl                       'データ参照Table
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
        Dim TBLview As New DataView(OIM0004tbl)
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
        work.WF_SEL_LINECNT.Text = OIM0004tbl.Rows(WW_LINECNT)("LINECNT")

        '貨物車コード
        work.WF_SEL_STATIONCODE2.Text = OIM0004tbl.Rows(WW_LINECNT)("STATIONCODE")

        '貨物コード枝番
        work.WF_SEL_BRANCH2.Text = OIM0004tbl.Rows(WW_LINECNT)("BRANCH")

        '貨物駅名称
        work.WF_SEL_STATONNAME.Text = OIM0004tbl.Rows(WW_LINECNT)("STATONNAME")

        '貨物駅名称カナ
        work.WF_SEL_STATIONNAMEKANA.Text = OIM0004tbl.Rows(WW_LINECNT)("STATIONNAMEKANA")

        '貨物駅種別名称
        work.WF_SEL_TYPENAME.Text = OIM0004tbl.Rows(WW_LINECNT)("TYPENAME")

        '貨物駅種別名称カナ
        work.WF_SEL_TYPENAMEKANA.Text = OIM0004tbl.Rows(WW_LINECNT)("TYPENAMEKANA")

        '発着駅フラグ
        work.WF_SEL_DEPARRSTATIONFLG2.Text = OIM0004tbl.Rows(WW_LINECNT)("DEPARRSTATIONFLG")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0004tbl.Rows(WW_LINECNT)("DELFLG")

        '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

        '○ 状態をクリア
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIM0004tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0004tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0004INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0004INProw As DataRow = OIM0004INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0004INPcol As DataColumn In OIM0004INPtbl.Columns
                If IsDBNull(OIM0004INProw.Item(OIM0004INPcol)) OrElse IsNothing(OIM0004INProw.Item(OIM0004INPcol)) Then
                    Select Case OIM0004INPcol.ColumnName
                        Case "LINECNT"
                            OIM0004INProw.Item(OIM0004INPcol) = 0
                        Case "OPERATION"
                            OIM0004INProw.Item(OIM0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            OIM0004INProw.Item(OIM0004INPcol) = 0
                        Case "SELECT"
                            OIM0004INProw.Item(OIM0004INPcol) = 1
                        Case "HIDDEN"
                            OIM0004INProw.Item(OIM0004INPcol) = 0
                        Case Else
                            OIM0004INProw.Item(OIM0004INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("STATIONCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("BRANCH") >= 0 Then
                For Each OIM0004row As DataRow In OIM0004tbl.Rows
                    If XLSTBLrow("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
                        XLSTBLrow("BRANCH") = OIM0004row("BRANCH") Then
                        OIM0004INProw.ItemArray = OIM0004row.ItemArray
                        '更新種別は初期化する
                        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '貨物駅コード
            If WW_COLUMNS.IndexOf("STATIONCODE") >= 0 Then
                OIM0004INProw("STATIONCODE") = XLSTBLrow("STATIONCODE")
            End If

            '貨物コード枝番
            If WW_COLUMNS.IndexOf("BRANCH") >= 0 Then
                OIM0004INProw("BRANCH") = XLSTBLrow("BRANCH")
            End If

            '貨物駅名称
            If WW_COLUMNS.IndexOf("STATONNAME") >= 0 Then
                OIM0004INProw("STATONNAME") = XLSTBLrow("STATONNAME")
            End If

            '貨物駅名称カナ
            If WW_COLUMNS.IndexOf("STATIONNAMEKANA") >= 0 Then
                OIM0004INProw("STATIONNAMEKANA") = XLSTBLrow("STATIONNAMEKANA")
            End If

            '貨物駅種別名称
            If WW_COLUMNS.IndexOf("TYPENAME") >= 0 Then
                OIM0004INProw("TYPENAME") = XLSTBLrow("TYPENAME")
            End If

            '貨物駅種別名称カナ
            If WW_COLUMNS.IndexOf("TYPENAMEKANA") >= 0 Then
                OIM0004INProw("TYPENAMEKANA") = XLSTBLrow("TYPENAMEKANA")
            End If

            '発着駅フラグ
            If WW_COLUMNS.IndexOf("DEPARRSTATIONFLG") >= 0 Then
                OIM0004INProw("DEPARRSTATIONFLG") = XLSTBLrow("DEPARRSTATIONFLG")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0004INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '○ 名称取得
            '発着駅フラグ名
            CODENAME_get("DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), OIM0004INProw("DEPARRSTATIONNAME"), WW_DUMMY)           '取引先名称(届先)

            OIM0004INPtbl.Rows.Add(OIM0004INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0004tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

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
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl)

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

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
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物駅コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STATIONCODE", OIM0004INProw("STATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "貨物駅コード入力エラー。数値を入力してください。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物コード枝番(バリデーションチェック) ※値が設定されている場合のみ
            If Not String.IsNullOrEmpty(OIM0004INProw("BRANCH")) Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BRANCH", OIM0004INProw("BRANCH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "貨物コード枝番入力エラー。数値を入力してください。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '発着駅フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値が設定されている場合のみ、値存在チェック
                If Not String.IsNullOrEmpty(OIM0004INProw("DEPARRSTATIONFLG")) Then
                    CODENAME_get("DEPARRSTATIONFLG", OIM0004INProw("DEPARRSTATIONFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(発着駅フラグエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発着駅フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0004row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0004row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0004row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅コード       =" & OIM0004row("STATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物コード枝番     =" & OIM0004row("BRANCH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称         =" & OIM0004row("STATONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称カナ     =" & OIM0004row("STATIONNAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称     =" & OIM0004row("TYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称カナ =" & OIM0004row("TYPENAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発着駅フラグ       =" & OIM0004row("DEPARRSTATIONFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0004row("DELFLG")
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
    ''' OIM0004tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0004tbl_UPD()

        '○ 画面状態設定
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0004INProw.Item("OPERATION") = CONST_INSERT

            '入力レコードと現行レコードを比較する
            For Each OIM0004row As DataRow In OIM0004tbl.Rows
                'KEY項目が等しい時
                If OIM0004row("STATIONCODE") = OIM0004INProw("STATIONCODE") AndAlso
                    OIM0004row("BRANCH") = OIM0004INProw("BRANCH") Then
                    'KEY項目以外の項目の差異をチェック
                    If OIM0004row("STATONNAME") = OIM0004INProw("STATONNAME") AndAlso
                        OIM0004row("STATIONNAMEKANA") = OIM0004INProw("STATIONNAMEKANA") AndAlso
                        OIM0004row("TYPENAME") = OIM0004INProw("TYPENAME") AndAlso
                        OIM0004row("TYPENAMEKANA") = OIM0004INProw("TYPENAMEKANA") AndAlso
                        OIM0004row("DEPARRSTATIONFLG") = OIM0004INProw("DEPARRSTATIONFLG") AndAlso
                        OIM0004row("DELFLG") = OIM0004INProw("DELFLG") Then
                        '変更がないときは「操作」の項目は空白にする
                        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        '変更がある時は「操作」の項目を「更新」に設定する
                        OIM0004INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows
            Select Case OIM0004INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0004INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0004INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0004INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0004INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
                OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0004INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0004row As DataRow = OIM0004tbl.NewRow
        OIM0004row.ItemArray = OIM0004INProw.ItemArray

        OIM0004row("LINECNT") = OIM0004tbl.Rows.Count + 1
        If OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0004row("TIMSTP") = "0"
        OIM0004row("SELECT") = 1
        OIM0004row("HIDDEN") = 0

        OIM0004tbl.Rows.Add(OIM0004row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
               OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
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
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DEPARRSTATIONFLG" '発着駅フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG"))
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

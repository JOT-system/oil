Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' モデル距離マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0012MODEL
    Inherits Page

    '○ 検索結果格納Table
    Private MC0012tbl As DataTable                                  '一覧格納用テーブル
    Private MC0012INPtbl As DataTable                               'チェック用テーブル
    Private MC0012UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    Private Const CONST_PATTERN1 As String = "1"                    'モデル距離パターン　届先のみ
    Private Const CONST_PATTERN2 As String = "2"                    'モデル距離パターン　届先、出荷場所
    Private Const CONST_PATTERN3 As String = "3"                    'モデル距離パターン　出荷場所

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
                    Master.RecoverTable(MC0012tbl)

                    Select Case WF_ButtonClick.Value
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
            If Not IsNothing(MC0012tbl) Then
                MC0012tbl.Clear()
                MC0012tbl.Dispose()
                MC0012tbl = Nothing
            End If

            If Not IsNothing(MC0012INPtbl) Then
                MC0012INPtbl.Clear()
                MC0012INPtbl.Dispose()
                MC0012INPtbl = Nothing
            End If

            If Not IsNothing(MC0012UPDtbl) Then
                MC0012UPDtbl.Clear()
                MC0012UPDtbl.Dispose()
                MC0012UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = GRMC0012WRKINC.MAPID
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MC0012S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
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

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MC0012tbl)

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

        If IsNothing(MC0012tbl) Then
            MC0012tbl = New DataTable
        End If

        If MC0012tbl.Columns.Count <> 0 Then
            MC0012tbl.Columns.Clear()
        End If

        MC0012tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを出荷地・届先別モデル距離マスタから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                     AS LINECNT" _
            & " , ''                                    AS OPERATION" _
            & " , CAST(MC012.UPDTIMSTP AS bigint)       AS TIMSTP" _
            & " , 1                                     AS 'SELECT'" _
            & " , 0                                     AS HIDDEN" _
            & " , ISNULL(RTRIM(MC012.CAMPCODE), '')     AS CAMPCODE" _
            & " , ISNULL(RTRIM(MC012.UORG), '')         AS UORG" _
            & " , ISNULL(RTRIM(MC012.MODELPATTERN), '') AS MODELPATTERN" _
            & " , ISNULL(RTRIM(MC012.TORICODES), '')    AS TORICODES" _
            & " , ''                                    AS TORINAMES" _
            & " , ISNULL(RTRIM(MC012.SHUKABASHO), '')   AS SHUKABASHO" _
            & " , ''                                    AS SHUKABASHONAMES" _
            & " , ISNULL(RTRIM(MC012.TORICODET), '')    AS TORICODET" _
            & " , ''                                    AS TORINAMET" _
            & " , ISNULL(RTRIM(MC012.TODOKECODE), '')   AS TODOKECODE" _
            & " , ''                                    AS TODOKENAME" _
            & " , ISNULL(RTRIM(MC012.MODEL), '')        AS MODEL" _
            & " , ISNULL(RTRIM(MC012.DELFLG), '')       AS DELFLG" _
            & " FROM MC012_MODEL MC012 " _
            & " INNER JOIN M0001_CAMP M001 " _
            & "    ON  M001.CAMPCODE = MC012.CAMPCODE" _
            & "   AND M001.STYMD    <= @P2" _
            & "   AND M001.ENDYMD   >= @P3" _
            & "   AND M001.DELFLG   <> @P4" _
            & " WHERE MC012.CAMPCODE   = @P1" _
            & "   AND MC012.DELFLG    <> @P4"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '運用部署
        If Not String.IsNullOrEmpty(work.WF_SEL_UORG.Text) Then
            SQLStr &= String.Format("    AND MC012.UORG = '{0}'", work.WF_SEL_UORG.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    MC012.UORG" _
            & "    , MC012.MODELPATTERN" _
            & "    , MC012.TORICODES" _
            & "    , MC012.SHUKABASHO" _
            & "    , MC012.TORICODET" _
            & "    , MC012.TODOKECODE" _
            & "    , MC012.MODEL"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '有効年月日(To)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '有効年月日(From)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Date.Now
                PARA3.Value = Date.Now
                PARA4.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MC0012tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MC0012tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each MC0012row As DataRow In MC0012tbl.Rows
                    i += 1
                    MC0012row("LINECNT") = i        'LINECNT

                    '取引先名称(出荷先)
                    CODENAME_get("TORICODES", MC0012row("TORICODES"), MC0012row("TORINAMES"), WW_DUMMY)
                    work.WF_SEL_TORICODES.Text = MC0012row("TORICODES")

                    '出荷場所名称
                    CODENAME_get("SHUKABASHO", MC0012row("SHUKABASHO"), MC0012row("SHUKABASHONAMES"), WW_DUMMY)

                    '取引先名称(届先)
                    CODENAME_get("TORICODET", MC0012row("TORICODET"), MC0012row("TORINAMET"), WW_DUMMY)
                    work.WF_SEL_TORICODET.Text = MC0012row("TORICODET")

                    '届先名称
                    CODENAME_get("TODOKECODE", MC0012row("TODOKECODE"), MC0012row("TODOKENAME"), WW_DUMMY)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC012_MODEL SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC012_MODEL Select"
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
        For Each MC0012row As DataRow In MC0012tbl.Rows
            If MC0012row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MC0012row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(MC0012tbl)

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

                'モデル距離マスタ更新
                UpdateModelMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

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
        'For Each MC0012row As DataRow In MC0012tbl.Rows
        '    '読み飛ばし
        '    If MC0012row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING OrElse
        '        MC0012row("DELFLG") = C_DELETE_FLG.DELETE Then
        '        Continue For
        '    End If

        '    WW_LINEERR_SW = ""

        '    '期間重複チェック
        '    For Each checkRow As DataRow In MC0012tbl.Rows
        '        '同一KEY以外は読み飛ばし
        '        If checkRow("CAMPCODE") = MC0012row("CAMPCODE") AndAlso
        '            checkRow("UORG") = MC0012row("UORG") AndAlso
        '            checkRow("MODELPATTERN") = MC0012row("MODELPATTERN") AndAlso
        '            checkRow("TORICODES") = MC0012row("TORICODES") AndAlso
        '            checkRow("SHUKABASHO") = MC0012row("SHUKABASHO") AndAlso
        '            checkRow("TORICODET") = MC0012row("TORICODET") AndAlso
        '            checkRow("TODOKECODE") = MC0012row("TODOKECODE") Then
        '        Else
        '            Continue For
        '        End If
        '    Next

        '    If WW_LINEERR_SW = "" Then
        '        If MC0012row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        '            MC0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        End If
        '    Else
        '        MC0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End If
        'Next

    End Sub


    ''' <summary>
    ''' 出荷地・届先別モデル距離マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateModelMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        MC012_MODEL" _
            & "    WHERE" _
            & "        CAMPCODE         = @P1" _
            & "        AND UORG         = @P2" _
            & "        AND MODELPATTERN = @P3" _
            & "        AND TORICODES    = @P4" _
            & "        AND SHUKABASHO   = @P5" _
            & "        AND TORICODET    = @P6" _
            & "        AND TODOKECODE   = @P7 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MC012_MODEL" _
            & "    SET" _
            & "        MODELPATTERN = @P3     , TORICODES = @P4" _
            & "        , SHUKABASHO = @P5     , TORICODET = @P6" _
            & "        , TODOKECODE = @P7     , MODEL     = @P8" _
            & "        , DELFLG     = @P9     , UPDYMD    = @P11" _
            & "        , UPDUSER    = @P12    , UPDTERMID = @P13" _
            & "        , RECEIVEYMD = @P14" _
            & "    WHERE" _
            & "        CAMPCODE         = @P1" _
            & "        AND UORG         = @P2" _
            & "        AND MODELPATTERN = @P3" _
            & "        AND TORICODES    = @P4" _
            & "        AND SHUKABASHO   = @P5" _
            & "        AND TORICODET    = @P6" _
            & "        AND TODOKECODE   = @P7 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MC012_MODEL" _
            & "        (CAMPCODE    , UORG    , MODELPATTERN         , TORICODES        , SHUKABASHO" _
            & "        , TORICODET       , TODOKECODE     , MODEL    , DELFLG    , INITYMD" _
            & "        , UPDYMD     , UPDUSER       , UPDTERMID        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2     , @P3     , @P4     , @P5" _
            & "        , @P6     , @P7     , @P8     , @P9     , @P10" _
            & "        , @P11    , @P12    , @P13    , @P14    ) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , UORG" _
            & "    , MODELPATTERN" _
            & "    , TORICODES" _
            & "    , SHUKABASHO" _
            & "    , TORICODET" _
            & "    , TODOKECODE" _
            & "    , MODEL" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    MC012_MODEL" _
            & " WHERE" _
            & "        CAMPCODE         = @P1" _
            & "        AND UORG         = @P2" _
            & "        AND MODELPATTERN = @P3" _
            & "        AND TORICODES    = @P4" _
            & "        AND SHUKABASHO   = @P5" _
            & "        AND TORICODET    = @P6" _
            & "        AND TODOKECODE   = @P7 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '運用部署コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)             'モデル距離パターン
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)            '取引先コード（出荷場所）
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20)            '出荷場所コード
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)            '取引先コード（届先）
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)            '届先コード
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Int)                     'モデル距離
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)              '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)              '更新年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '運用部署コード
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 1)         'モデル距離パターン
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        '取引先コード（出荷場所）
                Dim JPARA5 As SqlParameter = SQLcmdJnl.Parameters.Add("@P5", SqlDbType.NVarChar, 20)        '出荷場所コード
                Dim JPARA6 As SqlParameter = SQLcmdJnl.Parameters.Add("@P6", SqlDbType.NVarChar, 20)        '取引先コード（届先）
                Dim JPARA7 As SqlParameter = SQLcmdJnl.Parameters.Add("@P7", SqlDbType.NVarChar, 20)        '届先コード

                For Each MC0012row As DataRow In MC0012tbl.Rows
                    If Trim(MC0012row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(MC0012row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        '                        Trim(MC0012row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA1.Value = MC0012row("CAMPCODE")
                        PARA2.Value = MC0012row("UORG")
                        PARA3.Value = MC0012row("MODELPATTERN")
                        PARA4.Value = MC0012row("TORICODES")
                        PARA5.Value = MC0012row("SHUKABASHO")
                        PARA6.Value = MC0012row("TORICODET")
                        PARA7.Value = MC0012row("TODOKECODE")
                        PARA8.Value = MC0012row("MODEL")
                        PARA9.Value = MC0012row("DELFLG")
                        PARA10.Value = WW_DATENOW
                        PARA11.Value = WW_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MC0012row("CAMPCODE")
                        JPARA2.Value = MC0012row("UORG")
                        JPARA3.Value = MC0012row("MODELPATTERN")
                        JPARA4.Value = MC0012row("TORICODES")
                        JPARA5.Value = MC0012row("SHUKABASHO")
                        JPARA6.Value = MC0012row("TORICODET")
                        JPARA7.Value = MC0012row("TODOKECODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MC0012UPDtbl) Then
                                MC0012UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MC0012UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MC0012UPDtbl.Clear()
                            MC0012UPDtbl.Load(SQLdr)
                        End Using

                        For Each MC0012UPDrow As DataRow In MC0012UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MC012_MODEL"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MC0012UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC012_MODEL UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MC012_MODEL UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = MC0012tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = MC0012tbl                        'データ参照Table
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
        Dim TBLview As New DataView(MC0012tbl)
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
        WF_Sel_LINECNT.Text = MC0012tbl.Rows(WW_LINECNT)("LINECNT")

        'モデル距離
        WF_MODEL.Text = MC0012tbl.Rows(WW_LINECNT)("MODEL")

        'モデルパターン
        WF_MODELPT.Text = MC0012tbl.Rows(WW_LINECNT)("MODELPATTERN")
        CODENAME_get("MODELPATTERN", WF_MODELPT.Text, WF_MODELPT_TEXT.Text, WW_DUMMY)

        '取引先名称(出荷先)
        WF_TORICODES.Text = MC0012tbl.Rows(WW_LINECNT)("TORICODES")
        CODENAME_get("TORICODES", WF_TORICODES.Text, WF_TORICODES_TEXT.Text, WW_DUMMY)
        '   work.WF_SEL_TORICODES.Text = WF_TORICODES.Text


        '出荷場所名称
        WF_SHUKABASHO.Text = MC0012tbl.Rows(WW_LINECNT)("SHUKABASHO")
        CODENAME_get("SHUKABASHO", WF_SHUKABASHO.Text, WF_SHUKABASHO_TEXT.Text, WW_DUMMY)


        '取引先名称(届先)
        WF_TORICODET.Text = MC0012tbl.Rows(WW_LINECNT)("TORICODET")
        CODENAME_get("TORICODET", WF_TORICODET.Text, WF_TORICODET_TEXT.Text, WW_DUMMY)
        'work.WF_SEL_TORICODET.Text = WF_TORICODET.Text



        '届先名称
        WF_TODOKECODE.Text = MC0012tbl.Rows(WW_LINECNT)("TODOKECODE")
        CODENAME_get("TODOKECODE", WF_TODOKECODE.Text, WF_TODOKECODE_TEXT.Text, WW_DUMMY)

        '削除
        WF_DELFLG.Text = MC0012tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○ 状態をクリア
        For Each MC0012row As DataRow In MC0012tbl.Rows
            Select Case MC0012row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case MC0012tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                MC0012tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                MC0012tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                MC0012tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                MC0012tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                MC0012tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

        WF_GridDBclick.Text = ""

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
        Master.CreateEmptyTable(MC0012INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim MC0012INProw As DataRow = MC0012INPtbl.NewRow

            '○ 初期クリア
            For Each MC0012INPcol As DataColumn In MC0012INPtbl.Columns
                If IsDBNull(MC0012INProw.Item(MC0012INPcol)) OrElse IsNothing(MC0012INProw.Item(MC0012INPcol)) Then
                    Select Case MC0012INPcol.ColumnName
                        Case "LINECNT"
                            MC0012INProw.Item(MC0012INPcol) = 0
                        Case "OPERATION"
                            MC0012INProw.Item(MC0012INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MC0012INProw.Item(MC0012INPcol) = 0
                        Case "SELECT"
                            MC0012INProw.Item(MC0012INPcol) = 1
                        Case "HIDDEN"
                            MC0012INProw.Item(MC0012INPcol) = 0
                        Case Else
                            MC0012INProw.Item(MC0012INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("MODELPATTERN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORICODES") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SHUKABASHO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORICODET") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                For Each MC0012row As DataRow In MC0012tbl.Rows
                    If XLSTBLrow("MODELPATTERN") = MC0012row("MODELPATTERN") AndAlso
                        XLSTBLrow("TORICODES") = MC0012row("TORICODES") AndAlso
                        XLSTBLrow("SHUKABASHO") = MC0012row("SHUKABASHO") AndAlso
                        XLSTBLrow("TORICODET") = MC0012row("TORICODET") AndAlso
                        XLSTBLrow("TODOKECODE") = MC0012row("TODOKECODE") Then
                        MC0012INProw.ItemArray = MC0012row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            MC0012INProw.Item("CAMPCODE") = work.WF_SEL_CAMPCODE.Text

            '運用部署
            MC0012INProw.Item("UORG") = work.WF_SEL_UORG.Text

            'モデル距離パターン
            If WW_COLUMNS.IndexOf("MODELPATTERN") >= 0 Then
                MC0012INProw("MODELPATTERN") = XLSTBLrow("MODELPATTERN")
            End If

            '取引先（出荷場所）
            If WW_COLUMNS.IndexOf("TORICODES") >= 0 Then
                MC0012INProw("TORICODES") = XLSTBLrow("TORICODES")
            End If

            '出荷場所
            If WW_COLUMNS.IndexOf("SHUKABASHO") >= 0 Then
                MC0012INProw("SHUKABASHO") = XLSTBLrow("SHUKABASHO")
            End If

            '取引先（届先）
            If WW_COLUMNS.IndexOf("TORICODET") >= 0 Then
                MC0012INProw("TORICODET") = XLSTBLrow("TORICODET")
            End If

            '届先
            If WW_COLUMNS.IndexOf("TODOKECODE") >= 0 Then
                MC0012INProw("TODOKECODE") = XLSTBLrow("TODOKECODE")
            End If

            'モデル距離
            If WW_COLUMNS.IndexOf("MODEL") >= 0 Then
                MC0012INProw("MODEL") = XLSTBLrow("MODEL")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MC0012INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '○ 名称取得
            CODENAME_get("TORICODES", MC0012INProw("TORICODES"), MC0012INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
            CODENAME_get("SHUKABASHO", MC0012INProw("SHUKABASHO"), MC0012INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

            CODENAME_get("TORICODET", MC0012INProw("TORICODET"), MC0012INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
            CODENAME_get("TODOKECODE", MC0012INProw("TODOKECODE"), MC0012INProw("TODOKENAME"), WW_DUMMY)        '届先名称

            MC0012INPtbl.Rows.Add(MC0012INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MC0012tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

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
        DetailBoxToMC0012INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            MC0012tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

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
    Protected Sub DetailBoxToMC0012INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除

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

        Master.CreateEmptyTable(MC0012INPtbl)
        Dim MC0012INProw As DataRow = MC0012INPtbl.NewRow

        '○ 初期クリア
        For Each MC0012INPcol As DataColumn In MC0012INPtbl.Columns
            If IsDBNull(MC0012INProw.Item(MC0012INPcol)) OrElse IsNothing(MC0012INProw.Item(MC0012INPcol)) Then
                Select Case MC0012INPcol.ColumnName
                    Case "LINECNT"
                        MC0012INProw.Item(MC0012INPcol) = 0
                    Case "OPERATION"
                        MC0012INProw.Item(MC0012INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        MC0012INProw.Item(MC0012INPcol) = 0
                    Case "SELECT"
                        MC0012INProw.Item(MC0012INPcol) = 1
                    Case "HIDDEN"
                        MC0012INProw.Item(MC0012INPcol) = 0
                    Case Else
                        MC0012INProw.Item(MC0012INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            MC0012INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, MC0012INProw("LINECNT"))
            Catch ex As Exception
                MC0012INProw("LINECNT") = 0
            End Try
        End If

        MC0012INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        MC0012INProw("TIMSTP") = 0
        MC0012INProw("SELECT") = 1
        MC0012INProw("HIDDEN") = 0

        MC0012INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text        '会社コード
        MC0012INProw("UORG") = work.WF_SEL_UORG.Text                '運用部署

        MC0012INProw("DELFLG") = WF_DELFLG.Text                     '削除

        MC0012INProw("MODELPATTERN") = WF_MODELPT.Text              'モデル距離パターン

        MC0012INProw("TORICODES") = WF_TORICODES.Text               '取引先（出荷場所）

        MC0012INProw("SHUKABASHO") = WF_SHUKABASHO.Text             '出荷場所

        MC0012INProw("TORICODET") = WF_TORICODET.Text               '取引先（届先）

        MC0012INProw("TODOKECODE") = WF_TODOKECODE.Text             '届先

        MC0012INProw("MODEL") = WF_MODEL.Text                       'モデル距離

        '○ 名称取得
        CODENAME_get("TORICODES", MC0012INProw("TORICODES"), MC0012INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
        CODENAME_get("SHUKABASHO", MC0012INProw("SHUKABASHO"), MC0012INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

        CODENAME_get("TORICODET", MC0012INProw("TORICODET"), MC0012INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
        CODENAME_get("TODOKECODE", MC0012INProw("TODOKECODE"), MC0012INProw("TODOKENAME"), WW_DUMMY)        '届先名称

        '○ チェック用テーブルに登録する
        MC0012INPtbl.Rows.Add(MC0012INProw)

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
        For Each MC0012row As DataRow In MC0012tbl.Rows
            Select Case MC0012row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MC0012tbl)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        WF_MODELPT.Text = ""                'モデル距離パターン
        WF_MODELPT_TEXT.Text = ""           'モデル距離パターン名
        WF_TORICODES.Text = ""              '取引先（出荷場所）コード
        WF_TORICODES_TEXT.Text = ""         '取引先（出荷場所）名称
        WF_SHUKABASHO.Text = ""             '出荷場所コード
        WF_SHUKABASHO_TEXT.Text = ""        '出荷場所名
        WF_TORICODET.Text = ""              '取引先（届先）コード
        WF_TORICODET_TEXT.Text = ""         '取引先（届先）名称
        WF_TODOKECODE.Text = ""             '届先コード
        WF_TODOKECODE_TEXT.Text = ""        '届先名称
        WF_DELFLG.Text = ""                 '削除
        WF_DELFLG_TEXT.Text = ""            '削除名称

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
                    Case "WF_TORICODES"                             '取引先(出荷場所)
                        prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)

                    Case "WF_SHUKABASHO"                            '出荷場所
                        prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODES.Text)

                    Case "WF_TORICODET"                             '取引先(届先)
                        prmData = work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text)

                    Case "WF_TODOKECODE"                            '届先
                        prmData = work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODET.Text)

                    Case "WF_MODELPT"                               'モデル距離パターン
                        prmData = work.CreateMODELPTParam(work.WF_SEL_CAMPCODE.Text, WF_MODELPT.Text)

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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                    'モデル距離パターン
                Case "WF_MODELPT"
                    WF_MODELPT.Text = WW_SelectValue
                    WF_MODELPT_TEXT.Text = WW_SelectText
                    WF_MODELPT.Focus()

                    '取引先（出荷場所）
                Case "WF_TORICODES"
                    WF_TORICODES.Text = WW_SelectValue
                    WF_TORICODES_TEXT.Text = WW_SelectText
                    WF_TORICODES.Focus()

                    '出荷場所
                Case "WF_SHUKABASHO"
                    WF_SHUKABASHO.Text = WW_SelectValue
                    WF_SHUKABASHO_TEXT.Text = WW_SelectText
                    WF_SHUKABASHO.Focus()

                    '取引先（届先）
                Case "WF_TORICODET"
                    WF_TORICODET.Text = WW_SelectValue
                    WF_TORICODET_TEXT.Text = WW_SelectText
                    WF_TORICODET.Focus()

                    '届先
                Case "WF_TODOKECODE"
                    WF_TODOKECODE.Text = WW_SelectValue
                    WF_TODOKECODE_TEXT.Text = WW_SelectText
                    WF_TODOKECODE.Focus()
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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()

                    'モデル距離パターン
                Case "WF_MODELPT"
                    WF_MODELPT.Focus()

                    '取引先（出荷場所）
                Case "WF_TORICODES"
                    WF_TORICODES.Focus()

                    '出荷場所
                Case "WF_SHUKABASHO"
                    WF_SHUKABASHO.Focus()

                    '取引先（届先）
                Case "WF_TORICODET"
                    WF_TORICODET.Focus()

                    '届先
                Case "WF_TODOKECODE"
                    WF_TODOKECODE.Focus()

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
        For Each MC0012INProw As DataRow In MC0012INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", MC0012INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", MC0012INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'モデル距離(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODEL", MC0012INProw("MODEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "モデル距離入力エラー。数値を入力してください。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '削除フラグ　有効なら関連チェックする。　※削除なら関連チェックせず、削除フラグを立てる
            If MC0012INProw("DELFLG") = C_DELETE_FLG.ALIVE Then

                'モデル距離パターン(バリデーションチェック)
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODELPATTERN", MC0012INProw("MODELPATTERN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    'モデル距離パターン存在チェック
                    CODENAME_get("MODELPATTERN", MC0012INProw("MODELPATTERN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "モデル距離パターンエラー。'1'：届先のみ  '2':出荷場所、届先指定   '3':出荷場所のみ　のいずれかを入力してください。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "モデル距離パターンエラー。'1'：届先のみ  '2':出荷場所、届先指定   '3':出荷場所のみ　のいずれかを入力してください。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '関連チェック

                Select Case MC0012INProw("MODELPATTERN")
                    Case CONST_PATTERN1 '届先のみ
                        '取引先（届先）、届先、モデル距離が入力されている事
                        '取引先（届先）、届先がマスタに登録されていること

                        If MC0012INProw("TORICODES") = "" AndAlso MC0012INProw("SHUKABASHO") = "" AndAlso
                            MC0012INProw("TORICODET") <> "" AndAlso MC0012INProw("TODOKECODE") <> "" Then

                            '取引先(届先)コード存在チェック
                            CODENAME_get("TORICODET", MC0012INProw("TORICODET"), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・取引先(届先)コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If

                            '届先コード存在チェック
                            work.WF_SEL_TORICODET.Text = MC0012INProw("TORICODET")
                            CODENAME_get("TODOKECODE", MC0012INProw("TODOKECODE"), WW_DUMMY, WW_RTN_SW)  '届先名称
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・届先コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・モデル距離パターン組合せエラー。取引先（届先）コードと届先コードのみ入力してください。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                            WW_LINE_ERR = "PATTEN ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If

                    Case CONST_PATTERN2 '出荷場所、届先
                        '取引先（出荷場所）、出荷場所、取引先（届先）、届先、モデル距離が入力されている事
                        '取引先（出荷場所）、出荷場所、取引先（届先）、届先がマスタに登録されていること

                        If MC0012INProw("TORICODES") <> "" AndAlso MC0012INProw("SHUKABASHO") <> "" AndAlso
                            MC0012INProw("TORICODET") <> "" AndAlso MC0012INProw("TODOKECODE") <> "" Then

                            '取引先(出荷先)コード存在チェック
                            CODENAME_get("TORICODES", MC0012INProw("TORICODES"), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・取引先(出荷先)コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If

                            '出荷場所コード存在チェック
                            work.WF_SEL_TORICODES.Text = MC0012INProw("TORICODES")
                            CODENAME_get("SHUKABASHO", MC0012INProw("SHUKABASHO"), WW_DUMMY, WW_RTN_SW)  '出荷場所名
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・出荷場所コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If

                            '取引先(届先)コード存在チェック
                            CODENAME_get("TORICODET", MC0012INProw("TORICODET"), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・取引先(届先)コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If

                            '届先コード存在チェック
                            work.WF_SEL_TORICODET.Text = MC0012INProw("TORICODET")
                            CODENAME_get("TODOKECODE", MC0012INProw("TODOKECODE"), WW_DUMMY, WW_RTN_SW)  '届先名称
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・届先コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "モデル距離パターン組合せエラー。取引先（出荷場所）コード、出荷場所コード、取引先（届先）コード、届先コードを入力してください。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                            WW_LINE_ERR = "PATTEN ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If

                    Case CONST_PATTERN3 '出荷場所のみ
                        '取引先（出荷場所）、出荷場所、モデル距離が入力されている事
                        '取引先（出荷場所）、出荷場所がマスタに登録されていること
                        If MC0012INProw("TORICODES") <> "" AndAlso MC0012INProw("SHUKABASHO") <> "" AndAlso
                            MC0012INProw("TORICODET") = "" AndAlso MC0012INProw("TODOKECODE") = "" Then

                            '取引先(出荷先)コード存在チェック
                            CODENAME_get("TORICODES", MC0012INProw("TORICODES"), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・取引先(出荷先)コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If

                            '出荷場所コード存在チェック
                            work.WF_SEL_TORICODES.Text = MC0012INProw("TORICODES")
                            CODENAME_get("SHUKABASHO", MC0012INProw("SHUKABASHO"), WW_DUMMY, WW_RTN_SW)  '出荷場所名
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・出荷場所コードエラー。マスタに存在しないコードです。"
                                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                                WW_LINE_ERR = "PATTEN ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        Else
                            WW_CheckMES1 = "・モデル距離パターン組合せエラー。取引先（出荷先）コードと出荷場所コードのみ入力してください。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MC0012INProw)
                            WW_LINE_ERR = "PATTEN ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                End Select
            End If

            If WW_LINE_ERR = "" Then
                If MC0012INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MC0012INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    MC0012INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    MC0012INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MC0012row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MC0012row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MC0012row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> モデル距離パターン =" & MC0012row("MODELPATTERN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先（出荷場所） =" & MC0012row("TORICODES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出荷場所           =" & MC0012row("SHUKABASHO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先（届先）     =" & MC0012row("TORICODET") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 届先               =" & MC0012row("TODOKECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> モデル距離         =" & MC0012row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & MC0012row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' MC0012tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MC0012tbl_UPD()

        '○ 画面状態設定
        For Each MC0012row As DataRow In MC0012tbl.Rows
            Select Case MC0012row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    MC0012row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each MC0012INProw As DataRow In MC0012INPtbl.Rows

            'エラーレコード読み飛ばし
            If MC0012INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MC0012INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each MC0012row As DataRow In MC0012tbl.Rows
                If MC0012row("CAMPCODE") = MC0012INProw("CAMPCODE") AndAlso
                    MC0012row("UORG") = MC0012INProw("UORG") AndAlso
                    MC0012row("TORICODES") = MC0012INProw("TORICODES") AndAlso
                    MC0012row("SHUKABASHO") = MC0012INProw("SHUKABASHO") AndAlso
                    MC0012row("TORICODET") = MC0012INProw("TORICODET") AndAlso
                    MC0012row("TODOKECODE") = MC0012INProw("TODOKECODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If MC0012row("DELFLG") = MC0012INProw("DELFLG") AndAlso
                       MC0012row("MODELPATTERN") = MC0012INProw("MODELPATTERN") AndAlso
                       MC0012row("MODEL") = MC0012INProw("MODEL") Then
                        MC0012INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        MC0012INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MC0012INProw As DataRow In MC0012INPtbl.Rows
            Select Case MC0012INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(MC0012INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(MC0012INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(MC0012INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(MC0012INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MC0012INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MC0012INProw As DataRow)

        For Each MC0012row As DataRow In MC0012tbl.Rows

            '同一レコードか判定
            If MC0012INProw("CAMPCODE") = MC0012row("CAMPCODE") AndAlso
                MC0012INProw("UORG") = MC0012row("UORG") AndAlso
                MC0012INProw("TORICODES") = MC0012row("TORICODES") AndAlso
                MC0012INProw("SHUKABASHO") = MC0012row("SHUKABASHO") AndAlso
                MC0012INProw("TORICODET") = MC0012row("TORICODET") AndAlso
                MC0012INProw("TODOKECODE") = MC0012row("TODOKECODE") Then
                '画面入力テーブル項目設定
                MC0012INProw("LINECNT") = MC0012row("LINECNT")
                MC0012INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MC0012INProw("TIMSTP") = MC0012row("TIMSTP")
                MC0012INProw("SELECT") = 1
                MC0012INProw("HIDDEN") = 0

                '項目テーブル項目設定
                MC0012row.ItemArray = MC0012INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="MC0012INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef MC0012INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim MC0012row As DataRow = MC0012tbl.NewRow
        MC0012row.ItemArray = MC0012INProw.ItemArray

        MC0012row("LINECNT") = MC0012tbl.Rows.Count + 1
        If MC0012INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            MC0012row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            MC0012row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        MC0012row("TIMSTP") = "0"
        MC0012row("SELECT") = 1
        MC0012row("HIDDEN") = 0

        MC0012tbl.Rows.Add(MC0012row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="MC0012INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef MC0012INProw As DataRow)

        For Each MC0012row As DataRow In MC0012tbl.Rows

            '同一レコードか判定
            If MC0012INProw("CAMPCODE") = MC0012row("CAMPCODE") AndAlso
               MC0012INProw("UORG") = MC0012row("UORG") AndAlso
               MC0012INProw("TORICODES") = MC0012row("TORICODES") AndAlso
               MC0012INProw("SHUKABASHO") = MC0012row("SHUKABASHO") AndAlso
               MC0012INProw("TORICODET") = MC0012row("TORICODET") AndAlso
               MC0012INProw("TODOKECODE") = MC0012row("TODOKECODE") Then
                '画面入力テーブル項目設定
                MC0012INProw("LINECNT") = MC0012row("LINECNT")
                MC0012INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                MC0012INProw("TIMSTP") = MC0012row("TIMSTP")
                MC0012INProw("SELECT") = 1
                MC0012INProw("HIDDEN") = 0

                '項目テーブル項目設定
                MC0012row.ItemArray = MC0012INProw.ItemArray
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

                Case "TORICODES"     '取引先名称(出荷先)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                Case "SHUKABASHO"   '出荷場所名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_TORICODES.Text))
                Case "TORICODET"     '取引先名称（届先）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))

                Case "TODOKECODE"   '届先名称
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, work.WF_SEL_TORICODET.Text))
                Case "MODELPATTERN" 'モデル距離パターン
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MODELPT, I_VALUE, O_TEXT, O_RTN, work.CreateMODELPTParam(work.WF_SEL_CAMPCODE.Text, WF_MODELPT.Text))

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

''************************************************************
' 油槽所諸元マスタメンテナンス一覧
' 作成日 2020/11/18
' 更新日 2021/04/15
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/11/18 新規作成
'         :2021/04/15 1)項目「荷受人」「荷主」「油種」をコード→名称で表示するように変更
'         :           2)登録・更新画面にて更新メッセージが設定された場合
'         :             画面下部に更新メッセージを表示するように修正
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 油槽所諸元マスタメンテナンス一覧（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0015SyogenList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0015tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0015INPtbl As DataTable                               'チェック用テーブル
    Private OIM0015UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0015tbl)

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
            If Not IsNothing(OIM0015tbl) Then
                OIM0015tbl.Clear()
                OIM0015tbl.Dispose()
                OIM0015tbl = Nothing
            End If

            If Not IsNothing(OIM0015INPtbl) Then
                OIM0015INPtbl.Clear()
                OIM0015INPtbl.Dispose()
                OIM0015INPtbl = Nothing
            End If

            If Not IsNothing(OIM0015UPDtbl) Then
                OIM0015UPDtbl.Clear()
                OIM0015UPDtbl.Dispose()
                OIM0015UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0015WRKINC.MAPIDL
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
        rightview.COMPCODE = Master.USERCAMP
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0015S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0015C Then
            Master.RecoverTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0015C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0015tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0015tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = Master.USERCAMP
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

        If IsNothing(OIM0015tbl) Then
            OIM0015tbl = New DataTable
        End If

        If OIM0015tbl.Columns.Count <> 0 Then
            OIM0015tbl.Columns.Clear()
        End If

        OIM0015tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷受人マスタから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                         AS LINECNT " _
            & " , ''                                                        AS OPERATION " _
            & " , CAST(OIM0015.UPDTIMSTP AS bigint)                         AS UPDTIMSTP " _
            & " , 1                                                         AS 'SELECT' " _
            & " , 0                                                         AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0015.DELFLG), '')                         AS DELFLG " _
            & " , ISNULL(RTRIM(OIM0015.CONSIGNEECODE), '')                  AS CONSIGNEECODE " _
            & " , ''                                                        AS CONSIGNEENAME " _
            & " , ISNULL(RTRIM(OIM0015.SHIPPERSCODE), '')                   AS SHIPPERSCODE " _
            & " , ''                                                        AS SHIPPERSNAME " _
            & " , ISNULL(RTRIM(OIM0015.FROMMD), '')                         AS FROMMD " _
            & " , ISNULL(RTRIM(OIM0015.TOMD), '')                           AS TOMD " _
            & " , ISNULL(RTRIM(OIM0015.OILCODE), '')                        AS OILCODE " _
            & " , ''                                                        AS OILNAME " _
            & " , ISNULL(RTRIM(OIM0015.TANKCAP), '')                        AS TANKCAP " _
            & " , ISNULL(RTRIM(OIM0015.TARGETCAPRATE), '')                  AS TARGETCAPRATE " _
            & " , ISNULL(RTRIM(OIM0015.DS), '')                             AS DS " _
            & " FROM OIL.OIM0015_SYOGEN OIM0015 " _
            & " WHERE OIM0015.DELFLG <> @P1 "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '荷受人コード
        If Not String.IsNullOrEmpty(work.WF_SEL_CONSIGNEECODE.Text) Then
            SQLStr &= "    AND OIM0015.CONSIGNEECODE      = @P2"
        End If

        '荷主コード
        If Not String.IsNullOrEmpty(work.WF_SEL_SHIPPERSCODE.Text) Then
            SQLStr &= "    AND OIM0015.SHIPPERSCODE      = @P3"
        End If

        '油種コード
        If Not String.IsNullOrEmpty(work.WF_SEL_OILCODE.Text) Then
            SQLStr &= "    AND OIM0015.OILCODE        = @P4"
        End If

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('0000' + CAST(OIM0015.CONSIGNEECODE AS NVARCHAR), 4)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 1)        '削除フラグ
                PARA1.Value = C_DELETE_FLG.DELETE

                If Not String.IsNullOrEmpty(work.WF_SEL_CONSIGNEECODE.Text) Then
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 10)    '荷受人コード
                    PARA2.Value = work.WF_SEL_CONSIGNEECODE.Text
                End If

                If Not String.IsNullOrEmpty(work.WF_SEL_SHIPPERSCODE.Text) Then
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 10)    '荷主コード
                    PARA3.Value = work.WF_SEL_SHIPPERSCODE.Text
                End If

                If Not String.IsNullOrEmpty(work.WF_SEL_OILCODE.Text) Then
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)     '油種コード
                    PARA4.Value = work.WF_SEL_OILCODE.Text
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0015tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0015tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0015row As DataRow In OIM0015tbl.Rows
                    i += 1
                    OIM0015row("LINECNT") = i        'LINECNT
                    '〇名称設定
                    '荷受人
                    CODENAME_get("CONSIGNEECODE", OIM0015row("CONSIGNEECODE"), OIM0015row("CONSIGNEENAME"), WW_DUMMY)
                    '荷主
                    CODENAME_get("SHIPPERSCODE", OIM0015row("SHIPPERSCODE"), OIM0015row("SHIPPERSNAME"), WW_DUMMY)
                    '油種
                    CODENAME_get("OILCODE", OIM0015row("OILCODE"), OIM0015row("OILNAME"), WW_DUMMY)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0015L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0015L Select"
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
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            If OIM0015row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0015row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0015tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = Master.USERCAMP
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

        '荷受人コード(登録(新規追加用))
        work.WF_SEL_CONSIGNEECODE2.Text = ""

        '荷主コード(登録(新規追加用))
        work.WF_SEL_SHIPPERSCODE2.Text = ""

        '開始月日
        work.WF_SEL_FROMMD.Text = ""

        '終了月日
        work.WF_SEL_TOMD.Text = ""

        '油種コード(登録(新規追加用))
        work.WF_SEL_OILCODE2.Text = ""

        'タンク容量
        work.WF_SEL_TANKCAP.Text = "0.0"

        '目標在庫率
        work.WF_SEL_TARGETCAPRATE.Text = "0.000"

        'Ｄ／Ｓ
        work.WF_SEL_DS.Text = "0.0"

        '削除
        work.WF_SEL_DELFLG.Text = "0"

        ' 詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0015tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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

    End Sub

    ''' <summary>
    ''' 油槽所諸元マスタ登録更新
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
            & "        OIL.OIM0015_SYOGEN" _
            & "    WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P05 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0015_SYOGEN" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , FROMMD = @P03" _
            & "        , TOMD = @P04" _
            & "        , TANKCAP = @P06" _
            & "        , TARGETCAPRATE = @P07" _
            & "        , DS = @P08" _
            & "    WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P05 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0015_SYOGEN" _
            & "        (DELFLG" _
            & "        , CONSIGNEECODE" _
            & "        , SHIPPERSCODE" _
            & "        , FROMMD" _
            & "        , TOMD" _
            & "        , OILCODE" _
            & "        , TANKCAP" _
            & "        , TARGETCAPRATE" _
            & "        , DS" _
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
            & "        , @P15) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , CONSIGNEECODE" _
            & "    , SHIPPERSCODE" _
            & "    , FROMMD" _
            & "    , TOMD" _
            & "    , OILCODE" _
            & "    , TANKCAP" _
            & "    , TARGETCAPRATE" _
            & "    , DS" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0015_SYOGEN" _
            & " WHERE" _
            & "        CONSIGNEECODE  = @P01 " _
            & "        AND " _
            & "        SHIPPERSCODE   = @P02 " _
            & "        AND " _
            & "        OILCODE        = @P03 ;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 10)          '荷受人コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)          '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 10)          '開始月日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 10)          '終了月日
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 10)          '油種コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10)          'タンク容量
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 10)          '目標在庫率
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 10)          'Ｄ／Ｓ

                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.DateTime)              '登録年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.DateTime)              '更新年月日
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime)              '集信日時


                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)       '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4)       '荷受人コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 4)       '荷主コード
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 4)       '油種コード

                For Each OIM0015row As DataRow In OIM0015tbl.Rows
                    If Trim(OIM0015row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0015row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0015row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0015row("DELFLG")
                        PARA01.Value = OIM0015row("CONSIGNEECODE")
                        PARA02.Value = OIM0015row("SHIPPERSCODE")
                        PARA03.Value = OIM0015row("FROMMD")
                        PARA04.Value = OIM0015row("TOMD")
                        PARA05.Value = OIM0015row("OILCODE")
                        PARA06.Value = OIM0015row("TANKCAP")
                        PARA07.Value = OIM0015row("TARGETCAPRATE")
                        PARA08.Value = OIM0015row("DS")
                        PARA09.Value = WW_DATENOW
                        PARA10.Value = Master.USERID
                        PARA11.Value = Master.USERTERMID
                        PARA12.Value = WW_DATENOW
                        PARA13.Value = Master.USERID
                        PARA14.Value = Master.USERTERMID
                        PARA15.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIM0015row("DELFLG")
                        JPARA01.Value = OIM0015row("CONSIGNEECODE")
                        JPARA02.Value = OIM0015row("SHIPPERSCODE")
                        JPARA03.Value = OIM0015row("OILCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0015UPDtbl) Then
                                OIM0015UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0015UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0015UPDtbl.Clear()
                            OIM0015UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0015UPDrow As DataRow In OIM0015UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0015L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0015UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0015L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0015L UPDATE_INSERT"
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
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0015tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0015tbl                        'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        Dim TBLview As New DataView(OIM0015tbl)
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
        work.WF_SEL_LINECNT.Text = OIM0015tbl.Rows(WW_LINECNT)("LINECNT")

        '荷受人コード
        work.WF_SEL_CONSIGNEECODE2.Text = OIM0015tbl.Rows(WW_LINECNT)("CONSIGNEECODE")

        '荷主コード
        work.WF_SEL_SHIPPERSCODE2.Text = OIM0015tbl.Rows(WW_LINECNT)("SHIPPERSCODE")

        '開始月日
        work.WF_SEL_FROMMD.Text = OIM0015tbl.Rows(WW_LINECNT)("FROMMD")

        '終了月日
        work.WF_SEL_TOMD.Text = OIM0015tbl.Rows(WW_LINECNT)("TOMD")

        '油種コード
        work.WF_SEL_OILCODE2.Text = OIM0015tbl.Rows(WW_LINECNT)("OILCODE")

        'タンク容量
        work.WF_SEL_TANKCAP.Text = OIM0015tbl.Rows(WW_LINECNT)("TANKCAP")

        '目標在庫率
        work.WF_SEL_TARGETCAPRATE.Text = OIM0015tbl.Rows(WW_LINECNT)("TARGETCAPRATE")

        'Ｄ／Ｓ
        work.WF_SEL_DS.Text = OIM0015tbl.Rows(WW_LINECNT)("DS")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0015tbl.Rows(WW_LINECNT)("DELFLG")

        ' 詳細画面更新メッセージ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""

        '○ 状態をクリア
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            Select Case OIM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0015tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0015tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0015tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0015tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0015tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0015tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0015tbl, work.WF_SEL_INPTBL.Text)

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
        CS0023XLSUPLOAD.CAMPCODE = Master.USERCAMP                  '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        Master.CreateEmptyTable(OIM0015INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0015INProw As DataRow = OIM0015INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0015INPcol As DataColumn In OIM0015INPtbl.Columns
                If IsDBNull(OIM0015INProw.Item(OIM0015INPcol)) OrElse IsNothing(OIM0015INProw.Item(OIM0015INPcol)) Then
                    Select Case OIM0015INPcol.ColumnName
                        Case "LINECNT"
                            OIM0015INProw.Item(OIM0015INPcol) = 0
                        Case "OPERATION"
                            OIM0015INProw.Item(OIM0015INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0015INProw.Item(OIM0015INPcol) = 0
                        Case "SELECT"
                            OIM0015INProw.Item(OIM0015INPcol) = 1
                        Case "HIDDEN"
                            OIM0015INProw.Item(OIM0015INPcol) = 0
                        Case Else
                            OIM0015INProw.Item(OIM0015INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CONSIGNEECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SHIPPERSCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OILCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                For Each OIM0015row As DataRow In OIM0015tbl.Rows
                    'キー項目が一致する場合
                    If XLSTBLrow("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                        XLSTBLrow("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                        XLSTBLrow("FROMMD") = OIM0015row("FROMMD") AndAlso
                        XLSTBLrow("TOMD") = OIM0015row("TOMD") AndAlso
                        XLSTBLrow("OILCODE") = OIM0015row("OILCODE") AndAlso
                        XLSTBLrow("TANKCAP") = OIM0015row("TANKCAP") AndAlso
                        XLSTBLrow("TARGETCAPRATE") = OIM0015row("TARGETCAPRATE") AndAlso
                        XLSTBLrow("DS") = OIM0015row("DS") AndAlso
                        XLSTBLrow("DELFLG") = OIM0015row("DELFLG") Then
                        '変更元情報を入力レコードにコピーする
                        OIM0015INProw.ItemArray = OIM0015row.ItemArray
                        '更新種別は初期化する
                        OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '荷受人コード
            If WW_COLUMNS.IndexOf("CONSIGNEECODE") >= 0 Then
                OIM0015INProw("CONSIGNEECODE") = XLSTBLrow("CONSIGNEECODE")
                '荷受人
                CODENAME_get("CONSIGNEECODE", OIM0015INProw("CONSIGNEECODE"), OIM0015INProw("CONSIGNEENAME"), WW_DUMMY)
            End If

            '荷主コード
            If WW_COLUMNS.IndexOf("SHIPPERSCODE") >= 0 Then
                OIM0015INProw("SHIPPERSCODE") = XLSTBLrow("SHIPPERSCODE")
                '荷主
                CODENAME_get("SHIPPERSCODE", OIM0015INProw("SHIPPERSCODE"), OIM0015INProw("SHIPPERSNAME"), WW_DUMMY)
            End If

            '開始月日
            If WW_COLUMNS.IndexOf("FROMMD") >= 0 Then
                OIM0015INProw("FROMMD") = XLSTBLrow("FROMMD")
            End If

            '終了月日
            If WW_COLUMNS.IndexOf("TOMD") >= 0 Then
                OIM0015INProw("TOMD") = XLSTBLrow("TOMD")
            End If

            '油種コード
            If WW_COLUMNS.IndexOf("OILCODE") >= 0 Then
                OIM0015INProw("OILCODE") = XLSTBLrow("OILCODE")
                '油種
                CODENAME_get("OILCODE", OIM0015INProw("OILCODE"), OIM0015INProw("OILNAME"), WW_DUMMY)
            End If

            'タンク容量
            If WW_COLUMNS.IndexOf("TANKCAP") >= 0 Then
                OIM0015INProw("TANKCAP") = XLSTBLrow("TANKCAP")
            Else
                OIM0015INProw("TARGETCAPRATE") = "0.0"
            End If

            '目標在庫率
            If WW_COLUMNS.IndexOf("TARGETCAPRATE") >= 0 Then
                OIM0015INProw("TARGETCAPRATE") = XLSTBLrow("TARGETCAPRATE")
            Else
                OIM0015INProw("TARGETCAPRATE") = "0.000"
            End If

            'Ｄ／Ｓ
            If WW_COLUMNS.IndexOf("DS") >= 0 Then
                OIM0015INProw("DS") = XLSTBLrow("DS")
            Else
                OIM0015INProw("DS") = "0,0"
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0015INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIM0015INProw("DELFLG") = "0"
            End If

            OIM0015INPtbl.Rows.Add(OIM0015INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0015tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            Select Case OIM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0015tbl)

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
        Dim dateErrFlag As String = ""

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
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            WW_TEXT = OIM0015INProw("DELFLG")
            Master.CheckField(Master.USERCAMP, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0015INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷受人コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("CONSIGNEECODE")
            Master.CheckField(Master.USERCAMP, "CONSIGNEECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("CONSIGNEECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(荷受人コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷受人コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("SHIPPERSCODE")
            Master.CheckField(Master.USERCAMP, "SHIPPERSCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("SHIPPERSCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始月日(バリデーションチェック)
            WW_TEXT = OIM0015INProw("FROMMD")
            Master.CheckField(Master.USERCAMP, "FROMMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '月日チェック
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '月日チェック
                    WW_CheckMD(WW_TEXT, "開始月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(開始月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了月日(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TOMD")
            Master.CheckField(Master.USERCAMP, "TOMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '月日チェック
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '月日チェック
                    WW_CheckMD(WW_TEXT, "終了月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(終了月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック)
            WW_TEXT = OIM0015INProw("OILCODE")
            Master.CheckField(Master.USERCAMP, "OILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク容量(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TANKCAP")
            Master.CheckField(Master.USERCAMP, "TANKCAP", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク容量入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '目標在庫率(バリデーションチェック)
            WW_TEXT = OIM0015INProw("TARGETCAPRATE")
            Master.CheckField(Master.USERCAMP, "TARGETCAPRATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(目標在庫率入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'Ｄ／Ｓ(バリデーションチェック)
            WW_TEXT = OIM0015INProw("DS")
            Master.CheckField(Master.USERCAMP, "DS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(Ｄ／Ｓ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0015INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0015INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0015INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 月日チェック
    ''' </summary>
    ''' <param name="I_MD"></param>
    ''' <param name="I_MDNAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckMD(ByVal I_MD As String, ByVal I_MDNAME As String, ByVal I_VALUE As String, ByRef mdErrFlag As String)

        mdErrFlag = "1"
        Try
            '月取得
            Dim getMonth As String = I_MD.Remove(I_MD.IndexOf("/"))
            '日取得
            Dim getDay As String = I_MD.Remove(0, I_MD.IndexOf("/") + 1)

            '月と日の範囲チェック
            If getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_MDNAME, needsPopUp:=True)
            Else
                'エラーなし
                mdErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_MDNAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0015row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0015row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0015row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷受人コード =" & OIM0015row("CONSIGNEECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷受人 =" & OIM0015row("CONSIGNEENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード =" & OIM0015row("SHIPPERSCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主 =" & OIM0015row("SHIPPERSNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始月日 =" & OIM0015row("FROMMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了月日 =" & OIM0015row("TOMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード =" & OIM0015row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種 =" & OIM0015row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク容量 =" & OIM0015row("TANKCAP") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 目標在庫率 =" & OIM0015row("TARGETCAPRATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> Ｄ／Ｓ =" & OIM0015row("DS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0015row("DELFLG")
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
    ''' OIM0015tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0015tbl_UPD()

        '○ 画面状態設定
        For Each OIM0015row As DataRow In OIM0015tbl.Rows
            Select Case OIM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0015INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0015row As DataRow In OIM0015tbl.Rows
                ' KEY項目が等しい時
                If OIM0015row("CONSIGNEECODE") = OIM0015INProw("CONSIGNEECODE") AndAlso
                    OIM0015row("SHIPPERSCODE") = OIM0015INProw("SHIPPERSCODE") AndAlso
                    OIM0015row("OILCODE") = OIM0015INProw("OILCODE") Then
                    ' KEY項目以外の項目の差異チェック
                    If OIM0015row("FROMMD") = OIM0015INProw("FROMMD") AndAlso
                        OIM0015row("TOMD") = OIM0015INProw("TOMD") AndAlso
                        OIM0015row("TANKCAP") = OIM0015INProw("TANKCAP") AndAlso
                        OIM0015row("TARGETCAPRATE") = OIM0015INProw("TARGETCAPRATE") AndAlso
                        OIM0015row("DS") = OIM0015INProw("DS") AndAlso
                        OIM0015row("DELFLG") = OIM0015INProw("DELFLG") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0015INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0015INProw As DataRow In OIM0015INPtbl.Rows
            Select Case OIM0015INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0015INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0015INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0015INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0015INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0015INProw As DataRow)

        For Each OIM0015row As DataRow In OIM0015tbl.Rows

            '同一レコードか判定
            If OIM0015INProw("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                OIM0015INProw("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                OIM0015INProw("OILCODE") = OIM0015row("OILCODE") Then
                '画面入力テーブル項目設定
                OIM0015INProw("LINECNT") = OIM0015row("LINECNT")
                OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0015INProw("UPDTIMSTP") = OIM0015row("UPDTIMSTP")
                OIM0015INProw("SELECT") = 1
                OIM0015INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0015row.ItemArray = OIM0015INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0015INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0015row As DataRow = OIM0015tbl.NewRow
        OIM0015row.ItemArray = OIM0015INProw.ItemArray

        OIM0015row("LINECNT") = OIM0015tbl.Rows.Count + 1
        If OIM0015INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0015row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0015row("UPDTIMSTP") = "0"
        OIM0015row("SELECT") = 1
        OIM0015row("HIDDEN") = 0

        OIM0015tbl.Rows.Add(OIM0015row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0015INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0015INProw As DataRow)

        For Each OIM0015row As DataRow In OIM0015tbl.Rows

            '同一レコードか判定
            If OIM0015INProw("CONSIGNEECODE") = OIM0015row("CONSIGNEECODE") AndAlso
                OIM0015INProw("SHIPPERSCODE") = OIM0015row("SHIPPERSCODE") AndAlso
                OIM0015INProw("OILCODE") = OIM0015row("OILCODE") Then
                '画面入力テーブル項目設定
                OIM0015INProw("LINECNT") = OIM0015row("LINECNT")
                OIM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0015INProw("UPDTIMSTP") = OIM0015row("UPDTIMSTP")
                OIM0015INProw("SELECT") = 1
                OIM0015INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0015row.ItemArray = OIM0015INProw.ItemArray
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
                Case "CONSIGNEECODE"
                    '荷受人コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERSCODE"
                    '荷主コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILCODE"
                    '油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    '削除
                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0019AccountList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0019tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0019INPtbl As DataTable                              'チェック用テーブル
    Private OIM0019UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(OIM0019tbl)

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
            If Not IsNothing(OIM0019tbl) Then
                OIM0019tbl.Clear()
                OIM0019tbl.Dispose()
                OIM0019tbl = Nothing
            End If

            If Not IsNothing(OIM0019INPtbl) Then
                OIM0019INPtbl.Clear()
                OIM0019INPtbl.Dispose()
                OIM0019INPtbl = Nothing
            End If

            If Not IsNothing(OIM0019UPDtbl) Then
                OIM0019UPDtbl.Clear()
                OIM0019UPDtbl.Dispose()
                OIM0019UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0019WRKINC.MAPIDL
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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0019S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0019C Then
            Master.RecoverTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0019C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0019tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0019tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = Master.USERCAMP
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

        If IsNothing(OIM0019tbl) Then
            OIM0019tbl = New DataTable
        End If

        If OIM0019tbl.Columns.Count <> 0 Then
            OIM0019tbl.Columns.Clear()
        End If

        OIM0019tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷受人マスタから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                         AS LINECNT " _
            & " , ''                                                        AS OPERATION " _
            & " , CAST(OIM0019.UPDTIMSTP AS bigint)                         AS UPDTIMSTP " _
            & " , 1                                                         AS 'SELECT' " _
            & " , 0                                                         AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0019.DELFLG), '')                         AS DELFLG " _
            & " , ISNULL(RTRIM(FORMAT(OIM0019.FROMYMD, 'yyyy/MM/dd')), '')  AS FROMYMD " _
            & " , ISNULL(RTRIM(FORMAT(OIM0019.ENDYMD, 'yyyy/MM/dd')), '')   AS ENDYMD " _
            & " , ISNULL(RTRIM(OIM0019.ACCOUNTCODE), '')                    AS ACCOUNTCODE " _
            & " , ISNULL(RTRIM(OIM0019.ACCOUNTNAME), '')                    AS ACCOUNTNAME " _
            & " , ISNULL(RTRIM(OIM0019.SEGMENTCODE), '')                    AS SEGMENTCODE " _
            & " , ISNULL(RTRIM(OIM0019.SEGMENTNAME), '')                    AS SEGMENTNAME " _
            & " , ISNULL(RTRIM(OIM0019.SEGMENTBRANCHCODE), '')              AS SEGMENTBRANCHCODE " _
            & " , ISNULL(RTRIM(OIM0019.SEGMENTBRANCHNAME), '')              AS SEGMENTBRANCHNAME " _
            & " , ISNULL(RTRIM(OIM0019.ACCOUNTTYPE), '')                    AS ACCOUNTTYPE " _
            & " , ISNULL(RTRIM(OIM0019.ACCOUNTTYPENAME), '')                AS ACCOUNTTYPENAME " _
            & " FROM OIL.OIM0019_ACCOUNT OIM0019 " _
            & " WHERE OIM0019.DELFLG <> @P1 "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '適用開始年月日
        If Not String.IsNullOrEmpty(work.WF_SEL_FROMYMD.Text) Then
            SQLStr &= "    AND OIM0019.FROMYMD           <= @P2"
        End If

        '適用終了年月日
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            SQLStr &= "    AND OIM0019.ENDYMD            >= @P3"
        End If

        '科目コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ACCOUNTCODE.Text) Then
            SQLStr &= "    AND OIM0019.ACCOUNTCODE       = @P4"
        End If

        'セグメント
        If Not String.IsNullOrEmpty(work.WF_SEL_SEGMENTCODE.Text) Then
            SQLStr &= "    AND OIM0019.SEGMENTCODE       = @P5"
        End If

        'セグメント枝番
        If Not String.IsNullOrEmpty(work.WF_SEL_SEGMENTBRANCHCODE.Text) Then
            SQLStr &= "    AND OIM0019.SEGMENTBRANCHCODE = @P6"
        End If

        '科目区分
        If Not String.IsNullOrEmpty(work.WF_SEL_ACCOUNTTYPE.Text) Then
            SQLStr &= "    AND OIM0019.ACCOUNTTYPE       = @P7"
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIM0019.FROMYMD" _
            & "    , OIM0019.ENDYMD" _
            & "    , RIGHT('00000000' + CAST(OIM0019.ACCOUNTCODE AS NVARCHAR), 8)" _
            & "    , RIGHT('00000' + CAST(OIM0019.SEGMENTCODE AS NVARCHAR), 5)" _
            & "    , RIGHT('00' + CAST(OIM0019.SEGMENTBRANCHCODE AS NVARCHAR), 2)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                '削除フラグ
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 1)
                PARA1.Value = C_DELETE_FLG.DELETE

                '適用開始年月日
                If Not String.IsNullOrEmpty(work.WF_SEL_FROMYMD.Text) Then
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    PARA2.Value = Date.Parse(work.WF_SEL_FROMYMD.Text)
                End If

                '適用終了年月日
                If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    PARA3.Value = Date.Parse(work.WF_SEL_ENDYMD.Text)
                End If

                '科目コード
                If Not String.IsNullOrEmpty(work.WF_SEL_ACCOUNTCODE.Text) Then
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 8)
                    PARA4.Value = work.WF_SEL_ACCOUNTCODE.Text
                End If

                'セグメント
                If Not String.IsNullOrEmpty(work.WF_SEL_SEGMENTCODE.Text) Then
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 5)
                    PARA5.Value = work.WF_SEL_SEGMENTCODE.Text
                End If

                'セグメント枝番
                If Not String.IsNullOrEmpty(work.WF_SEL_SEGMENTBRANCHCODE.Text) Then
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 2)
                    PARA6.Value = work.WF_SEL_SEGMENTBRANCHCODE.Text
                End If

                '科目区分
                If Not String.IsNullOrEmpty(work.WF_SEL_ACCOUNTTYPE.Text) Then
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 2)
                    PARA7.Value = work.WF_SEL_ACCOUNTTYPE.Text
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0019tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0019tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0019row As DataRow In OIM0019tbl.Rows
                    i += 1
                    OIM0019row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0019L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0019L Select"
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
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            If OIM0019row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0019row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0019tbl)

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
        work.WF_SEL_LINECNT.Text = ""

        '適用開始年月日
        work.WF_SEL_FROMYMD2.Text = ""

        '適用終了年月日
        work.WF_SEL_ENDYMD2.Text = ""

        '科目コード
        work.WF_SEL_ACCOUNTCODE2.Text = ""

        '科目名
        work.WF_SEL_ACCOUNTNAME.Text = ""

        'セグメント
        work.WF_SEL_SEGMENTCODE2.Text = ""

        'セグメント名
        work.WF_SEL_SEGMENTNAME.Text = ""

        'セグメント枝番
        work.WF_SEL_SEGMENTBRANCHCODE2.Text = ""

        'セグメント枝番名
        work.WF_SEL_SEGMENTBRANCHNAME.Text = ""

        '科目区分
        work.WF_SEL_ACCOUNTTYPE2.Text = ""

        '科目区分名
        work.WF_SEL_ACCOUNTTYPENAME.Text = ""

        '削除
        work.WF_SEL_DELFLG.Text = "0"

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0019tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

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
    ''' 勘定科目マスタ登録更新
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
            & "        OIL.OIM0019_ACCOUNT" _
            & "    WHERE" _
            & "        FROMYMD             = @P01 " _
            & "    AND ENDYMD              = @P02 " _
            & "    AND ACCOUNTCODE         = @P03 " _
            & "    AND SEGMENTCODE         = @P05 " _
            & "    AND SEGMENTBRANCHCODE   = @P07 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0019_ACCOUNT" _
            & "    SET" _
            & "        DELFLG              = @P00" _
            & "        , ACCOUNTNAME       = @P04 " _
            & "        , SEGMENTNAME       = @P06 " _
            & "        , SEGMENTBRANCHNAME = @P08 " _
            & "        , ACCOUNTTYPE       = @P09 " _
            & "        , ACCOUNTTYPENAME   = @P10 " _
            & "        , UPDYMD            = @P14" _
            & "        , UPDUSER           = @P15" _
            & "        , UPDTERMID         = @P16" _
            & "        , RECEIVEYMD        = @P17" _
            & "    WHERE" _
            & "        FROMYMD             = @P01 " _
            & "    AND ENDYMD              = @P02 " _
            & "    AND ACCOUNTCODE         = @P03 " _
            & "    AND SEGMENTCODE         = @P05 " _
            & "    AND SEGMENTBRANCHCODE   = @P07 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0019_ACCOUNT" _
            & "        (DELFLG" _
            & "        , FROMYMD" _
            & "        , ENDYMD" _
            & "        , ACCOUNTCODE" _
            & "        , ACCOUNTNAME" _
            & "        , SEGMENTCODE" _
            & "        , SEGMENTNAME" _
            & "        , SEGMENTBRANCHCODE" _
            & "        , SEGMENTBRANCHNAME" _
            & "        , ACCOUNTTYPE" _
            & "        , ACCOUNTTYPENAME" _
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
            & "        , @P17) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , FROMYMD" _
            & "    , ENDYMD" _
            & "    , ACCOUNTCODE" _
            & "    , ACCOUNTNAME" _
            & "    , SEGMENTCODE" _
            & "    , SEGMENTNAME" _
            & "    , SEGMENTBRANCHCODE" _
            & "    , SEGMENTBRANCHNAME" _
            & "    , ACCOUNTTYPE" _
            & "    , ACCOUNTTYPENAME" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0019_ACCOUNT" _
            & " WHERE" _
            & "     FROMYMD           = @P01 " _
            & " AND ENDYMD            = @P02 " _
            & " AND ACCOUNTCODE       = @P03 " _
            & " AND SEGMENTCODE       = @P05 " _
            & " AND SEGMENTBRANCHCODE = @P07 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)          '適用開始年月日
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)          '適用終了年月日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   '科目コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 40)  '科目名
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 5)   'セグメント
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 40)  'セグメント名
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 2)   'セグメント枝番
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 40)  'セグメント枝番名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 2)   '科目区分
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40)  '科目区分名

                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)      '登録年月日
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)  '登録ユーザーＩＤ
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)  '登録端末
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)      '更新年月日
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)  '更新ユーザーＩＤ
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)  '更新端末
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.DateTime)      '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.Date)         '適用開始年月日
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.Date)         '適用終了年月日
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 8)  '科目コード
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 5)  'セグメント
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.NVarChar, 2)  'セグメント枝番

                For Each OIM0019row As DataRow In OIM0019tbl.Rows
                    If Trim(OIM0019row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0019row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0019row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0019row("DELFLG")

                        PARA01.Value = OIM0019row("FROMYMD")
                        PARA02.Value = OIM0019row("ENDYMD")
                        PARA03.Value = OIM0019row("ACCOUNTCODE")
                        PARA04.Value = OIM0019row("ACCOUNTNAME")
                        PARA05.Value = OIM0019row("SEGMENTCODE")
                        PARA06.Value = OIM0019row("SEGMENTNAME")
                        PARA07.Value = OIM0019row("SEGMENTBRANCHCODE")
                        PARA08.Value = OIM0019row("SEGMENTBRANCHNAME")
                        PARA09.Value = OIM0019row("ACCOUNTTYPE")
                        PARA10.Value = OIM0019row("ACCOUNTTYPENAME")

                        PARA11.Value = WW_DATENOW
                        PARA12.Value = Master.USERID
                        PARA13.Value = Master.USERTERMID
                        PARA14.Value = WW_DATENOW
                        PARA15.Value = Master.USERID
                        PARA16.Value = Master.USERTERMID
                        PARA17.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0019row("FROMYMD")
                        JPARA02.Value = OIM0019row("ENDYMD")
                        JPARA03.Value = OIM0019row("ACCOUNTCODE")
                        JPARA05.Value = OIM0019row("SEGMENTCODE")
                        JPARA07.Value = OIM0019row("SEGMENTBRANCHCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0019UPDtbl) Then
                                OIM0019UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0019UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0019UPDtbl.Clear()
                            OIM0019UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0019UPDrow As DataRow In OIM0019UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0019L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0019UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0019L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0019L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0019tbl                        'データ参照  Table
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
        CS0030REPORT.TBLDATA = OIM0019tbl                        'データ参照Table
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
        Dim TBLview As New DataView(OIM0019tbl)
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
        work.WF_SEL_LINECNT.Text = OIM0019tbl.Rows(WW_LINECNT)("LINECNT")

        '適用開始年月日
        work.WF_SEL_FROMYMD2.Text = OIM0019tbl.Rows(WW_LINECNT)("FROMYMD")

        '適用終了年月日
        work.WF_SEL_ENDYMD2.Text = OIM0019tbl.Rows(WW_LINECNT)("ENDYMD")

        '科目コード
        work.WF_SEL_ACCOUNTCODE2.Text = OIM0019tbl.Rows(WW_LINECNT)("ACCOUNTCODE")

        '科目名
        work.WF_SEL_ACCOUNTNAME.Text = OIM0019tbl.Rows(WW_LINECNT)("ACCOUNTNAME")

        'セグメント
        work.WF_SEL_SEGMENTCODE2.Text = OIM0019tbl.Rows(WW_LINECNT)("SEGMENTCODE")

        'セグメント名
        work.WF_SEL_SEGMENTNAME.Text = OIM0019tbl.Rows(WW_LINECNT)("SEGMENTNAME")

        'セグメント枝番
        work.WF_SEL_SEGMENTBRANCHCODE2.Text = OIM0019tbl.Rows(WW_LINECNT)("SEGMENTBRANCHCODE")

        'セグメント枝番名
        work.WF_SEL_SEGMENTBRANCHNAME.Text = OIM0019tbl.Rows(WW_LINECNT)("SEGMENTBRANCHNAME")

        '科目区分
        work.WF_SEL_ACCOUNTTYPE2.Text = OIM0019tbl.Rows(WW_LINECNT)("ACCOUNTTYPE")

        '科目区分名
        work.WF_SEL_ACCOUNTTYPENAME.Text = OIM0019tbl.Rows(WW_LINECNT)("ACCOUNTTYPENAME")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0019tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            Select Case OIM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0019tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0019tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0019tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0019tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0019tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0019tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0019tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0019INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0019INProw As DataRow = OIM0019INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0019INPcol As DataColumn In OIM0019INPtbl.Columns
                If IsDBNull(OIM0019INProw.Item(OIM0019INPcol)) OrElse IsNothing(OIM0019INProw.Item(OIM0019INPcol)) Then
                    Select Case OIM0019INPcol.ColumnName
                        Case "LINECNT"
                            OIM0019INProw.Item(OIM0019INPcol) = 0
                        Case "OPERATION"
                            OIM0019INProw.Item(OIM0019INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0019INProw.Item(OIM0019INPcol) = 0
                        Case "SELECT"
                            OIM0019INProw.Item(OIM0019INPcol) = 1
                        Case "HIDDEN"
                            OIM0019INProw.Item(OIM0019INPcol) = 0
                        Case Else
                            OIM0019INProw.Item(OIM0019INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("FROMYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ENDYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ACCOUNTCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SEGMENTCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SEGMENTBRANCHCODE") >= 0 Then
                For Each OIM0019row As DataRow In OIM0019tbl.Rows
                    If XLSTBLrow("FROMYMD") = OIM0019row("FROMYMD") AndAlso
                        XLSTBLrow("ENDYMD") = OIM0019row("ENDYMD") AndAlso
                        XLSTBLrow("ACCOUNTCODE") = OIM0019row("ACCOUNTCODE") AndAlso
                        XLSTBLrow("SEGMENTCODE") = OIM0019row("SEGMENTCODE") AndAlso
                        XLSTBLrow("SEGMENTBRANCHCODE") = OIM0019row("SEGMENTBRANCHCODE") Then
                        OIM0019INProw.ItemArray = OIM0019row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '適用開始年月日
            If WW_COLUMNS.IndexOf("FROMYMD") >= 0 Then
                OIM0019INProw("FROMYMD ") = XLSTBLrow("FROMYMD")
            End If

            '適用終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                OIM0019INProw("ENDYMD ") = XLSTBLrow("ENDYMD")
            End If

            '科目コード
            If WW_COLUMNS.IndexOf("ACCOUNTCODE") >= 0 Then
                OIM0019INProw("ACCOUNTCODE ") = XLSTBLrow("ACCOUNTCODE")
            End If

            '科目名
            If WW_COLUMNS.IndexOf("ACCOUNTNAME") >= 0 Then
                OIM0019INProw("ACCOUNTNAME ") = XLSTBLrow("ACCOUNTNAME")
            End If

            'セグメント
            If WW_COLUMNS.IndexOf("SEGMENTCODE") >= 0 Then
                OIM0019INProw("SEGMENTCODE ") = XLSTBLrow("SEGMENTCODE")
            End If

            'セグメント名
            If WW_COLUMNS.IndexOf("SEGMENTNAME") >= 0 Then
                OIM0019INProw("SEGMENTNAME ") = XLSTBLrow("SEGMENTNAME")
            End If

            'セグメント枝番
            If WW_COLUMNS.IndexOf("SEGMENTBRANCHCODE") >= 0 Then
                OIM0019INProw("SEGMENTBRANCHCODE ") = XLSTBLrow("SEGMENTBRANCHCODE")
            End If

            'セグメント枝番名
            If WW_COLUMNS.IndexOf("SEGMENTBRANCHNAME") >= 0 Then
                OIM0019INProw("SEGMENTBRANCHNAME ") = XLSTBLrow("SEGMENTBRANCHNAME")
            End If

            '科目区分
            If WW_COLUMNS.IndexOf("ACCOUNTTYPE") >= 0 Then
                OIM0019INProw("ACCOUNTTYPE ") = XLSTBLrow("ACCOUNTTYPE")
            End If

            '科目区分名
            If WW_COLUMNS.IndexOf("ACCOUNTTYPENAME") >= 0 Then
                OIM0019INProw("ACCOUNTTYPENAME ") = XLSTBLrow("ACCOUNTTYPENAME")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0019INProw("DELFLG ") = XLSTBLrow("DELFLG")
            Else
                OIM0019INProw("DELFLG") = "0"
            End If

            OIM0019INPtbl.Rows.Add(OIM0019INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0019tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

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
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            Select Case OIM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0019tbl)

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
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            WW_TEXT = OIM0019INProw("DELFLG")
            Master.CheckField(Master.USERCAMP, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0019INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '適用開始年月日(バリデーションチェック）
            WW_TEXT = OIM0019INProw("FROMYMD")
            Master.CheckField(Master.USERCAMP, "FROMYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "適用開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0019INProw("FROMYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '適用終了年月日(バリデーションチェック）
            WW_TEXT = OIM0019INProw("ENDYMD")
            Master.CheckField(Master.USERCAMP, "ENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "適用終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0019INProw("ENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(適用終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目コード（バリデーションチェック）
            WW_TEXT = OIM0019INProw("ACCOUNTCODE")
            Master.CheckField(work.WF_SEL_ACCOUNTCODE.Text, "ACCOUNTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目名（バリデーションチェック）
            WW_TEXT = OIM0019INProw("ACCOUNTNAME")
            Master.CheckField(work.WF_SEL_ACCOUNTNAME.Text, "ACCOUNTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント（バリデーションチェック）
            WW_TEXT = OIM0019INProw("SEGMENTCODE")
            Master.CheckField(work.WF_SEL_SEGMENTCODE.Text, "SEGMENTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメントエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント名（バリデーションチェック）
            WW_TEXT = OIM0019INProw("SEGMENTNAME")
            Master.CheckField(work.WF_SEL_SEGMENTNAME.Text, "SEGMENTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント枝番（バリデーションチェック）
            WW_TEXT = OIM0019INProw("SEGMENTBRANCHCODE")
            Master.CheckField(work.WF_SEL_SEGMENTBRANCHCODE.Text, "SEGMENTBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント枝番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント枝番名（バリデーションチェック）
            WW_TEXT = OIM0019INProw("SEGMENTBRANCHNAME")
            Master.CheckField(work.WF_SEL_SEGMENTBRANCHNAME.Text, "SEGMENTBRANCHNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント枝番名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目区分（バリデーションチェック）
            WW_TEXT = OIM0019INProw("ACCOUNTTYPE")
            Master.CheckField(work.WF_SEL_ACCOUNTTYPE.Text, "ACCOUNTTYPE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '科目区分名（バリデーションチェック）
            WW_TEXT = OIM0019INProw("ACCOUNTTYPENAME")
            Master.CheckField(work.WF_SEL_ACCOUNTTYPENAME.Text, "ACCOUNTTYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(科目区分名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0019INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0019INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0019INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            ' 年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            ' 月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            ' 月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            ' 日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            ' 閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                ' 月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                ' エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0019row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0019row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0019row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用開始年月日 =" & OIM0019row("FROMYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 適用終了年月日 =" & OIM0019row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目コード =" & OIM0019row("ACCOUNTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目名 =" & OIM0019row("ACCOUNTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント =" & OIM0019row("SEGMENTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント名 =" & OIM0019row("SEGMENTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番 =" & OIM0019row("SEGMENTBRANCHCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番名 =" & OIM0019row("SEGMENTBRANCHNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目区分 =" & OIM0019row("ACCOUNTTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 科目区分名 =" & OIM0019row("ACCOUNTTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0019row("DELFLG")
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
    ''' OIM0019tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0019tbl_UPD()

        '○ 画面状態設定
        For Each OIM0019row As DataRow In OIM0019tbl.Rows
            Select Case OIM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0019INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0019row As DataRow In OIM0019tbl.Rows
                ' KEY項目が等しい時
                If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                    OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                    OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                    OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                    OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then

                    ' KEY項目以外の項目の差異をチェック
                    If OIM0019row("ACCOUNTNAME") = OIM0019INProw("ACCOUNTNAME") AndAlso
                        OIM0019row("SEGMENTNAME") = OIM0019INProw("SEGMENTNAME") AndAlso
                        OIM0019row("SEGMENTBRANCHNAME") = OIM0019INProw("SEGMENTBRANCHNAME") AndAlso
                        OIM0019row("ACCOUNTTYPE") = OIM0019INProw("ACCOUNTTYPE") AndAlso
                        OIM0019row("ACCOUNTTYPENAME") = OIM0019INProw("ACCOUNTTYPENAME") AndAlso
                        OIM0019row("DELFLG") = OIM0019INProw("DELFLG") Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        '変更がある時は「操作」の項目を「更新」に設定する
                        OIM0019INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0019INProw As DataRow In OIM0019INPtbl.Rows
            Select Case OIM0019INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0019INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0019INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0019INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0019INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0019INProw As DataRow)

        For Each OIM0019row As DataRow In OIM0019tbl.Rows

            '同一レコードか判定
            If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then
                '画面入力テーブル項目設定
                OIM0019INProw("LINECNT") = OIM0019row("LINECNT")
                OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0019INProw("UPDTIMSTP") = OIM0019row("UPDTIMSTP")
                OIM0019INProw("SELECT") = 1
                OIM0019INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0019row.ItemArray = OIM0019INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0019INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0019row As DataRow = OIM0019tbl.NewRow
        OIM0019row.ItemArray = OIM0019INProw.ItemArray

        OIM0019row("LINECNT") = OIM0019tbl.Rows.Count + 1
        If OIM0019INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0019row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0019row("UPDTIMSTP") = "0"
        OIM0019row("SELECT") = 1
        OIM0019row("HIDDEN") = 0

        OIM0019tbl.Rows.Add(OIM0019row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0019INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0019INProw As DataRow)

        For Each OIM0019row As DataRow In OIM0019tbl.Rows

            '同一レコードか判定
            If OIM0019row("FROMYMD") = OIM0019INProw("FROMYMD") AndAlso
                OIM0019row("ENDYMD") = OIM0019INProw("ENDYMD") AndAlso
                OIM0019row("ACCOUNTCODE") = OIM0019INProw("ACCOUNTCODE") AndAlso
                OIM0019row("SEGMENTCODE") = OIM0019INProw("SEGMENTCODE") AndAlso
                OIM0019row("SEGMENTBRANCHCODE") = OIM0019INProw("SEGMENTBRANCHCODE") Then

                '画面入力テーブル項目設定
                OIM0019INProw("LINECNT") = OIM0019row("LINECNT")
                OIM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0019INProw("UPDTIMSTP") = OIM0019row("UPDTIMSTP")
                OIM0019INProw("SELECT") = 1
                OIM0019INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0019row.ItemArray = OIM0019INProw.ItemArray
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
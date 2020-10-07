Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0011ToriList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0011tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0011INPtbl As DataTable                               'チェック用テーブル
    Private OIM0011UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIM0011tbl)

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
            If Not IsNothing(OIM0011tbl) Then
                OIM0011tbl.Clear()
                OIM0011tbl.Dispose()
                OIM0011tbl = Nothing
            End If

            If Not IsNothing(OIM0011INPtbl) Then
                OIM0011INPtbl.Clear()
                OIM0011INPtbl.Dispose()
                OIM0011INPtbl = Nothing
            End If

            If Not IsNothing(OIM0011UPDtbl) Then
                OIM0011UPDtbl.Clear()
                OIM0011UPDtbl.Dispose()
                OIM0011UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0011WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0011S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0011C Then
            Master.RecoverTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0011C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0011tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0011tbl)

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

        If IsNothing(OIM0011tbl) Then
            OIM0011tbl = New DataTable
        End If

        If OIM0011tbl.Columns.Count <> 0 Then
            OIM0011tbl.Columns.Clear()
        End If

        OIM0011tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを取引先マスタから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                            AS LINECNT " _
            & " , ''                                                           AS OPERATION " _
            & " , CAST(OIM0011.UPDTIMSTP AS bigint)                            AS UPDTIMSTP " _
            & " , 1                                                            AS 'SELECT' " _
            & " , 0                                                            AS HIDDEN " _
            & " , ISNULL(RTRIM(OIM0011.DELFLG), '')                            AS DELFLG " _
            & " , ISNULL(RTRIM(OIM0011.TORICODE), '')                          AS TORICODE " _
            & " , ISNULL(FORMAT(OIM0011.STYMD, 'yyyy/MM/dd'), '')              AS STYMD " _
            & " , ISNULL(FORMAT(OIM0011.ENDYMD, 'yyyy/MM/dd'), '')             AS ENDYMD " _
            & " , ISNULL(RTRIM(OIM0011.TORINAME), '')                          AS TORINAME " _
            & " , ISNULL(RTRIM(OIM0011.TORINAMES), '')                         AS TORINAMES " _
            & " , ISNULL(RTRIM(OIM0011.TORINAMEKANA), '')                      AS TORINAMEKANA " _
            & " , ISNULL(RTRIM(OIM0011.POSTNUM1), '')                          AS POSTNUM1 " _
            & " , ISNULL(RTRIM(OIM0011.POSTNUM2), '')                          AS POSTNUM2 " _
            & " , ISNULL(RTRIM(OIM0011.ADDR1), '')                             AS ADDR1 " _
            & " , ISNULL(RTRIM(OIM0011.ADDR2), '')                             AS ADDR2 " _
            & " , ISNULL(RTRIM(OIM0011.ADDR3), '')                             AS ADDR3 " _
            & " , ISNULL(RTRIM(OIM0011.ADDR4), '')                             AS ADDR4 " _
            & " , ISNULL(RTRIM(OIM0011.TEL), '')                               AS TEL " _
            & " , ISNULL(RTRIM(OIM0011.FAX), '')                               AS FAX " _
            & " , ISNULL(RTRIM(OIM0011.MAIL), '')                              AS MAIL " _
            & " , ISNULL(RTRIM(OIM0011.OILUSEFLG), '')                         AS OILUSEFLG " _
            & " FROM OIL.OIM0011_TORI OIM0011 " _
            & " WHERE OIM0011.DELFLG <> @P1 "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '取引先コード
        If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
            'SQLStr &= String.Format("    AND OIM0011.TORICODE = '{0}'", work.WF_SEL_TORICODE.Text)
            SQLStr &= "    AND OIM0011.TORICODE   = @P2"
        End If

        '有効年月日（開始）
        If Not String.IsNullOrEmpty(work.WF_SEL_STYMD.Text) Then
            SQLStr &= "    AND OIM0011.STYMD      <= @P3"
        End If

        '有効年月日（終了）
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            SQLStr &= "    AND OIM0011.ENDYMD     >= @P4"
        End If

        SQLStr &=
              " ORDER BY" _
            & "    RIGHT('0000' + CAST(OIM0011.TORICODE AS NVARCHAR), 4)"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 1)        '削除フラグ
                PARA1.Value = C_DELETE_FLG.DELETE

                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 10)        '取引先コード
                    PARA2.Value = work.WF_SEL_TORICODE.Text
                End If

                If Not String.IsNullOrEmpty(work.WF_SEL_STYMD.Text) Then
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)        '有効年月日(開始）
                    PARA3.Value = work.WF_SEL_STYMD.Text
                End If

                If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)        '有効年月日(終了）
                    PARA4.Value = work.WF_SEL_ENDYMD.Text
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0011tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0011tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0011row As DataRow In OIM0011tbl.Rows
                    i += 1
                    OIM0011row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0011L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0011L Select"
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
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            If OIM0011row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0011row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0011tbl)

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
        work.WF_SEL_LINECNT.Text = ""

        '取引先コード(登録(新規追加用))
        work.WF_SEL_TORICODE2.Text = ""

        '開始年月日(登録(新規追加用))
        work.WF_SEL_STYMD2.Text = ""

        '終了年月日(登録(新規追加用))
        work.WF_SEL_ENDYMD2.Text = ""

        '取引先名称
        work.WF_SEL_TORINAME.Text = ""

        '取引先略称
        work.WF_SEL_TORINAMES.Text = ""

        '取引先カナ名称
        work.WF_SEL_TORINAMEKANA.Text = ""

        '郵便番号（上）
        work.WF_SEL_POSTNUM1.Text = ""

        '郵便番号（下）
        work.WF_SEL_POSTNUM2.Text = ""

        '住所１
        work.WF_SEL_ADDR1.Text = ""

        '住所２
        work.WF_SEL_ADDR2.Text = ""

        '住所３
        work.WF_SEL_ADDR3.Text = ""

        '住所４
        work.WF_SEL_ADDR4.Text = ""

        '電話番号
        work.WF_SEL_TEL.Text = ""

        'ＦＡＸ番号
        work.WF_SEL_FAX.Text = ""

        'メールアドレス
        work.WF_SEL_MAIL.Text = ""

        '石油利用フラグ
        work.WF_SEL_OILUSEFLG.Text = ""

        '削除
        work.WF_SEL_DELFLG.Text = "0"

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0011tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

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
    ''' 取引先マスタ登録更新
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
            & "        OIL.OIM0011_TORI" _
            & "    WHERE" _
            & "        TORICODE       = @P01 " _
            & "        AND STYMD      = @P02 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0011_TORI" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , ENDYMD = @P03" _
            & "        , TORINAME = @P04" _
            & "        , TORINAMES = @P05" _
            & "        , TORINAMEKANA = @P06" _
            & "        , POSTNUM1 = @P07" _
            & "        , POSTNUM2 = @P08" _
            & "        , ADDR1 = @P09" _
            & "        , ADDR2 = @P10" _
            & "        , ADDR3 = @P11" _
            & "        , ADDR4 = @P12" _
            & "        , TEL = @P13" _
            & "        , FAX = @P14" _
            & "        , MAIL = @P15" _
            & "        , OILUSEFLG = @P16" _
            & "    WHERE" _
            & "        TORICODE       = @P01" _
            & "        AND STYMD      = @P02 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0011_TORI" _
            & "        (DELFLG" _
            & "        , TORICODE" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , TORINAME" _
            & "        , TORINAMES" _
            & "        , TORINAMEKANA" _
            & "        , POSTNUM1" _
            & "        , POSTNUM2" _
            & "        , ADDR1" _
            & "        , ADDR2" _
            & "        , ADDR3" _
            & "        , ADDR4" _
            & "        , TEL" _
            & "        , FAX" _
            & "        , MAIL" _
            & "        , OILUSEFLG" _
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
            & "        , @P23) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "    , TORICODE" _
            & "    , STYMD" _
            & "    , ENDYMD" _
            & "    , TORINAME" _
            & "    , TORINAMES" _
            & "    , TORINAMEKANA" _
            & "    , POSTNUM1" _
            & "    , POSTNUM2" _
            & "    , ADDR1" _
            & "    , ADDR2" _
            & "    , ADDR3" _
            & "    , ADDR4" _
            & "    , TEL" _
            & "    , FAX" _
            & "    , MAIL" _
            & "    , OILUSEFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIM0011_TORI" _
            & " WHERE" _
            & "        TORICODE = @P01" _
            & "        AND STYMD    = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 10)          '取引先コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)                  '開始年月日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)                  '終了年月日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 100)         '取引先名称
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 50)          '取引先略称
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 100)         '取引先カナ名称
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 3)           '郵便番号（上）
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 4)           '郵便番号（下）
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 120)         '住所１
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 120)         '住所２
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 120)         '住所３
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 120)         '住所４
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 15)          '電話番号
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 15)          'ＦＡＸ番号
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 128)         'メールアドレス
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 1)           '石油利用フラグ
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.DateTime)              '登録年月日
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.DateTime)              '更新年月日
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.DateTime)              '集信日時


                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)       '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 10)      '取引先コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.Date)              '開始年月日

                For Each OIM0011row As DataRow In OIM0011tbl.Rows
                    If Trim(OIM0011row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0011row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0011row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0011row("DELFLG")
                        PARA01.Value = OIM0011row("TORICODE")
                        If RTrim(OIM0011row("STYMD")) <> "" Then
                            PARA02.Value = RTrim(OIM0011row("STYMD"))
                        Else
                            PARA02.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIM0011row("ENDYMD")) <> "" Then
                            PARA03.Value = RTrim(OIM0011row("ENDYMD"))
                        Else
                            PARA03.Value = C_DEFAULT_YMD
                        End If
                        PARA04.Value = OIM0011row("TORINAME")
                        PARA05.Value = OIM0011row("TORINAMES")
                        PARA06.Value = OIM0011row("TORINAMEKANA")
                        PARA07.Value = OIM0011row("POSTNUM1")
                        PARA08.Value = OIM0011row("POSTNUM2")
                        PARA09.Value = OIM0011row("ADDR1")
                        PARA10.Value = OIM0011row("ADDR2")
                        PARA11.Value = OIM0011row("ADDR3")
                        PARA12.Value = OIM0011row("ADDR4")
                        PARA13.Value = OIM0011row("TEL")
                        PARA14.Value = OIM0011row("FAX")
                        PARA15.Value = OIM0011row("MAIL")
                        PARA16.Value = OIM0011row("OILUSEFLG")
                        PARA17.Value = WW_DATENOW
                        PARA18.Value = Master.USERID
                        PARA19.Value = Master.USERTERMID
                        PARA20.Value = WW_DATENOW
                        PARA21.Value = Master.USERID
                        PARA22.Value = Master.USERTERMID
                        PARA23.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIM0011row("DELFLG")
                        JPARA01.Value = OIM0011row("TORICODE")
                        If RTrim(OIM0011row("STYMD")) <> "" Then
                            JPARA02.Value = RTrim(OIM0011row("STYMD"))
                        Else
                            JPARA02.Value = C_DEFAULT_YMD
                        End If

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0011UPDtbl) Then
                                OIM0011UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0011UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0011UPDtbl.Clear()
                            OIM0011UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0011UPDrow As DataRow In OIM0011UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0011L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0011UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0011L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0011L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0011tbl                        'データ参照  Table
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
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0011tbl                        'データ参照Table
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
        Dim TBLview As New DataView(OIM0011tbl)
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
        work.WF_SEL_LINECNT.Text = OIM0011tbl.Rows(WW_LINECNT)("LINECNT")

        '取引先コード
        work.WF_SEL_TORICODE2.Text = OIM0011tbl.Rows(WW_LINECNT)("TORICODE")

        '開始年月日
        work.WF_SEL_STYMD2.Text = OIM0011tbl.Rows(WW_LINECNT)("STYMD")

        '終了年月日
        work.WF_SEL_ENDYMD2.Text = OIM0011tbl.Rows(WW_LINECNT)("ENDYMD")

        '取引先名称
        work.WF_SEL_TORINAME.Text = OIM0011tbl.Rows(WW_LINECNT)("TORINAME")

        '取引先略称
        work.WF_SEL_TORINAMES.Text = OIM0011tbl.Rows(WW_LINECNT)("TORINAMES")

        '取引先カナ名称
        work.WF_SEL_TORINAMEKANA.Text = OIM0011tbl.Rows(WW_LINECNT)("TORINAMEKANA")

        '郵便番号（上）
        work.WF_SEL_POSTNUM1.Text = OIM0011tbl.Rows(WW_LINECNT)("POSTNUM1")

        '郵便番号（下）
        work.WF_SEL_POSTNUM2.Text = OIM0011tbl.Rows(WW_LINECNT)("POSTNUM2")

        '住所１
        work.WF_SEL_ADDR1.Text = OIM0011tbl.Rows(WW_LINECNT)("ADDR1")

        '住所２
        work.WF_SEL_ADDR2.Text = OIM0011tbl.Rows(WW_LINECNT)("ADDR2")

        '住所３
        work.WF_SEL_ADDR3.Text = OIM0011tbl.Rows(WW_LINECNT)("ADDR3")

        '住所４
        work.WF_SEL_ADDR4.Text = OIM0011tbl.Rows(WW_LINECNT)("ADDR4")

        '電話番号
        work.WF_SEL_TEL.Text = OIM0011tbl.Rows(WW_LINECNT)("TEL")

        'ＦＡＸ番号
        work.WF_SEL_FAX.Text = OIM0011tbl.Rows(WW_LINECNT)("FAX")

        'メールアドレス
        work.WF_SEL_MAIL.Text = OIM0011tbl.Rows(WW_LINECNT)("MAIL")

        '石油利用フラグ
        work.WF_SEL_OILUSEFLG.Text = OIM0011tbl.Rows(WW_LINECNT)("OILUSEFLG")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0011tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            Select Case OIM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0011tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0011tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0011tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0011INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0011INProw As DataRow = OIM0011INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0011INPcol As DataColumn In OIM0011INPtbl.Columns
                If IsDBNull(OIM0011INProw.Item(OIM0011INPcol)) OrElse IsNothing(OIM0011INProw.Item(OIM0011INPcol)) Then
                    Select Case OIM0011INPcol.ColumnName
                        Case "LINECNT"
                            OIM0011INProw.Item(OIM0011INPcol) = 0
                        Case "OPERATION"
                            OIM0011INProw.Item(OIM0011INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0011INProw.Item(OIM0011INPcol) = 0
                        Case "SELECT"
                            OIM0011INProw.Item(OIM0011INPcol) = 1
                        Case "HIDDEN"
                            OIM0011INProw.Item(OIM0011INPcol) = 0
                        Case Else
                            OIM0011INProw.Item(OIM0011INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("STYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ENDYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORINAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORINAMES") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TORINAMEKANA") >= 0 AndAlso
                WW_COLUMNS.IndexOf("POSTNUM1") >= 0 AndAlso
                WW_COLUMNS.IndexOf("POSTNUM2") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ADDR1") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ADDR2") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ADDR3") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ADDR4") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TEL") >= 0 AndAlso
                WW_COLUMNS.IndexOf("FAX") >= 0 AndAlso
                WW_COLUMNS.IndexOf("MAIL") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OILUSEFLG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                For Each OIM0011row As DataRow In OIM0011tbl.Rows
                    If XLSTBLrow("TORICODE") = OIM0011row("TORICODE") AndAlso
                        XLSTBLrow("STYMD") = OIM0011row("STYMD") AndAlso
                        XLSTBLrow("ENDYMD") = OIM0011row("ENDYMD") AndAlso
                        XLSTBLrow("TORINAME") = OIM0011row("TORINAME") AndAlso
                        XLSTBLrow("TORINAMES") = OIM0011row("TORINAMES") AndAlso
                        XLSTBLrow("TORINAMEKANA") = OIM0011row("TORINAMEKANA") AndAlso
                        XLSTBLrow("POSTNUM1") = OIM0011row("POSTNUM1") AndAlso
                        XLSTBLrow("POSTNUM2") = OIM0011row("POSTNUM2") AndAlso
                        XLSTBLrow("ADDR1") = OIM0011row("ADDR1") AndAlso
                        XLSTBLrow("ADDR2") = OIM0011row("ADDR2") AndAlso
                        XLSTBLrow("ADDR3") = OIM0011row("ADDR3") AndAlso
                        XLSTBLrow("ADDR4") = OIM0011row("ADDR4") AndAlso
                        XLSTBLrow("TEL") = OIM0011row("TEL") AndAlso
                        XLSTBLrow("FAX") = OIM0011row("FAX") AndAlso
                        XLSTBLrow("MAIL") = OIM0011row("MAIL") AndAlso
                        XLSTBLrow("OILUSEFLG") = OIM0011row("OILUSEFLG") AndAlso
                        XLSTBLrow("DELFLG") = OIM0011row("DELFLG") Then
                        OIM0011INProw.ItemArray = OIM0011row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '取引先コード
            If WW_COLUMNS.IndexOf("TORICODE") >= 0 Then
                OIM0011INProw("TORICODE") = XLSTBLrow("TORICODE")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                OIM0011INProw("STYMD") = XLSTBLrow("STYMD")
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                OIM0011INProw("ENDYMD") = XLSTBLrow("ENDYMD")
            End If

            '取引先名称
            If WW_COLUMNS.IndexOf("TORINAME") >= 0 Then
                OIM0011INProw("TORINAME") = XLSTBLrow("TORINAME")
            End If

            '取引先略称
            If WW_COLUMNS.IndexOf("TORINAMES") >= 0 Then
                OIM0011INProw("TORINAMES") = XLSTBLrow("TORINAMES")
            End If

            '取引先カナ名称
            If WW_COLUMNS.IndexOf("TORINAMEKANA") >= 0 Then
                OIM0011INProw("TORINAMEKANA") = XLSTBLrow("TORINAMEKANA")
            End If

            '郵便番号（上）
            If WW_COLUMNS.IndexOf("POSTNUM1") >= 0 Then
                OIM0011INProw("POSTNUM1") = XLSTBLrow("POSTNUM1")
            End If

            '郵便番号（下）
            If WW_COLUMNS.IndexOf("POSTNUM2") >= 0 Then
                OIM0011INProw("POSTNUM2") = XLSTBLrow("POSTNUM2")
            End If

            '住所１
            If WW_COLUMNS.IndexOf("ADDR1") >= 0 Then
                OIM0011INProw("ADDR1") = XLSTBLrow("ADDR1")
            End If

            '住所２
            If WW_COLUMNS.IndexOf("ADDR2") >= 0 Then
                OIM0011INProw("ADDR2") = XLSTBLrow("ADDR2")
            End If

            '住所３
            If WW_COLUMNS.IndexOf("ADDR3") >= 0 Then
                OIM0011INProw("ADDR3") = XLSTBLrow("ADDR3")
            End If

            '住所４
            If WW_COLUMNS.IndexOf("ADDR4") >= 0 Then
                OIM0011INProw("ADDR4") = XLSTBLrow("ADDR4")
            End If

            '電話番号
            If WW_COLUMNS.IndexOf("TEL") >= 0 Then
                OIM0011INProw("TEL") = XLSTBLrow("TEL")
            End If

            'ＦＡＸ番号
            If WW_COLUMNS.IndexOf("FAX") >= 0 Then
                OIM0011INProw("FAX") = XLSTBLrow("FAX")
            End If

            'メールアドレス
            If WW_COLUMNS.IndexOf("MAIL") >= 0 Then
                OIM0011INProw("MAIL") = XLSTBLrow("MAIL")
            End If

            '石油利用フラグ
            If WW_COLUMNS.IndexOf("OILUSEFLG") >= 0 Then
                OIM0011INProw("OILUSEFLG") = XLSTBLrow("OILUSEFLG")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0011INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIM0011INProw("DELFLG") = "0"
            End If

            OIM0011INPtbl.Rows.Add(OIM0011INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0011tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

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
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            Select Case OIM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0011tbl)

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
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            WW_TEXT = OIM0011INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0011INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先コード(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORICODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORICODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日(バリデーションチェック）
            WW_TEXT = OIM0011INProw("STYMD")
            Master.CheckField(Master.USERCAMP, "STYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0011INProw("STYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日(バリデーションチェック）
            WW_TEXT = OIM0011INProw("ENDYMD")
            Master.CheckField(Master.USERCAMP, "ENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(WW_TEXT, "終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKERR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0011INProw("ENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先名(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先略称(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAMES")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAMES", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先略称入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先名カナ(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TORINAMEKANA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TORINAMEKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取引先名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '郵便番号（上）(バリデーションチェック)
            WW_TEXT = OIM0011INProw("POSTNUM1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "POSTNUM1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(郵便番号（上）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '郵便番号（下）(バリデーションチェック)
            WW_TEXT = OIM0011INProw("POSTNUM2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "POSTNUM2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(郵便番号（下）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所１(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所１入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所２(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所２入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所３(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所３入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '住所４(バリデーションチェック)
            WW_TEXT = OIM0011INProw("ADDR4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ADDR4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(住所４入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '電話番号(バリデーションチェック)
            WW_TEXT = OIM0011INProw("TEL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(電話番号入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ＦＡＸ番号(バリデーションチェック)
            WW_TEXT = OIM0011INProw("FAX")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FAX", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(ＦＡＸ番号入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'メールアドレス(バリデーションチェック)
            WW_TEXT = OIM0011INProw("MAIL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAIL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(メールアドレス入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '石油利用フラグ(バリデーションチェック)
            WW_TEXT = OIM0011INProw("OILUSEFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILUSEFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OILUSEFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(石油利用フラグ入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(石油利用フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0011INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0011INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0011INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
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
    ''' <param name="OIM0011row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0011row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0011row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先コード =" & OIM0011row("TORICODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & OIM0011row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & OIM0011row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先名称 =" & OIM0011row("TORINAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先略称 =" & OIM0011row("TORINAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取引先カナ名称 =" & OIM0011row("TORINAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（上） =" & OIM0011row("POSTNUM1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 郵便番号（下） =" & OIM0011row("POSTNUM2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所１ =" & OIM0011row("ADDR1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所２ =" & OIM0011row("ADDR2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所３ =" & OIM0011row("ADDR3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 住所４ =" & OIM0011row("ADDR4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 電話番号 =" & OIM0011row("TEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ＦＡＸ番号 =" & OIM0011row("FAX") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> メールアドレス =" & OIM0011row("MAIL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 石油利用フラグ =" & OIM0011row("OILUSEFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0011row("DELFLG")
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
    ''' OIM0011tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0011tbl_UPD()

        '○ 画面状態設定
        For Each OIM0011row As DataRow In OIM0011tbl.Rows
            Select Case OIM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0011INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0011row As DataRow In OIM0011tbl.Rows
                If OIM0011row("TORICODE") = OIM0011INProw("TORICODE") AndAlso
                    OIM0011row("STYMD") = OIM0011row("STYMD") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0011row("ENDYMD") = OIM0011row("ENDYMD") AndAlso
                        OIM0011row("TORINAME") = OIM0011row("TORINAME") AndAlso
                        OIM0011row("TORINAMES") = OIM0011row("TORINAMES") AndAlso
                        OIM0011row("TORINAMEKANA") = OIM0011row("TORINAMEKANA") AndAlso
                        OIM0011row("POSTNUM1") = OIM0011row("POSTNUM1") AndAlso
                        OIM0011row("POSTNUM2") = OIM0011row("POSTNUM2") AndAlso
                        OIM0011row("ADDR1") = OIM0011row("ADDR1") AndAlso
                        OIM0011row("ADDR2") = OIM0011row("ADDR2") AndAlso
                        OIM0011row("ADDR3") = OIM0011row("ADDR3") AndAlso
                        OIM0011row("ADDR4") = OIM0011row("ADDR4") AndAlso
                        OIM0011row("TEL") = OIM0011row("TEL") AndAlso
                        OIM0011row("FAX") = OIM0011row("FAX") AndAlso
                        OIM0011row("MAIL") = OIM0011row("MAIL") AndAlso
                        OIM0011row("OILUSEFLG") = OIM0011row("OILUSEFLG") AndAlso
                        OIM0011row("DELFLG") = OIM0011INProw("DELFLG") AndAlso
                        OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0011INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0011INProw As DataRow In OIM0011INPtbl.Rows
            Select Case OIM0011INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0011INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0011INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0011INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0011INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0011INProw As DataRow)

        For Each OIM0011row As DataRow In OIM0011tbl.Rows

            '同一レコードか判定
            If OIM0011INProw("TORICODE") = OIM0011row("TORICODE") AndAlso
                OIM0011INProw("STYMD") = OIM0011row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0011INProw("LINECNT") = OIM0011row("LINECNT")
                OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0011INProw("UPDTIMSTP") = OIM0011row("UPDTIMSTP")
                OIM0011INProw("SELECT") = 1
                OIM0011INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0011row.ItemArray = OIM0011INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0011INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0011row As DataRow = OIM0011tbl.NewRow
        OIM0011row.ItemArray = OIM0011INProw.ItemArray

        OIM0011row("LINECNT") = OIM0011tbl.Rows.Count + 1
        If OIM0011INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0011row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0011row("UPDTIMSTP") = "0"
        OIM0011row("SELECT") = 1
        OIM0011row("HIDDEN") = 0

        OIM0011tbl.Rows.Add(OIM0011row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0011INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0011INProw As DataRow)

        For Each OIM0011row As DataRow In OIM0011tbl.Rows

            '同一レコードか判定
            If OIM0011INProw("TORICODE") = OIM0011row("TORICODE") AndAlso
                OIM0011INProw("STYMD") = OIM0011row("STYMD") Then
                '画面入力テーブル項目設定
                OIM0011INProw("LINECNT") = OIM0011row("LINECNT")
                OIM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0011INProw("UPDTIMSTP") = OIM0011row("UPDTIMSTP")
                OIM0011INProw("SELECT") = 1
                OIM0011INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0011row.ItemArray = OIM0011INProw.ItemArray
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
                Case "TORICODE"        '取引先コード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILUSEFLG"        '石油利用フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OILUSEFLG"))
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
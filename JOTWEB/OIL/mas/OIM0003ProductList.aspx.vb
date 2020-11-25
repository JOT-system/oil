Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 品種マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0003ProductList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0003INPtbl As DataTable                              'チェック用テーブル
    Private OIM0003UPDtbl As DataTable                              '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数

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
                    Master.RecoverTable(OIM0003tbl)

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
            If Not IsNothing(OIM0003tbl) Then
                OIM0003tbl.Clear()
                OIM0003tbl.Dispose()
                OIM0003tbl = Nothing
            End If

            If Not IsNothing(OIM0003INPtbl) Then
                OIM0003INPtbl.Clear()
                OIM0003INPtbl.Dispose()
                OIM0003INPtbl = Nothing
            End If

            If Not IsNothing(OIM0003UPDtbl) Then
                OIM0003UPDtbl.Clear()
                OIM0003UPDtbl.Dispose()
                OIM0003UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0003WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0003S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0003C Then
            Master.RecoverTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0003C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

        '〇 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0003tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0003tbl)

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

        If IsNothing(OIM0003tbl) Then
            OIM0003tbl = New DataTable
        End If

        If OIM0003tbl.Columns.Count <> 0 Then
            OIM0003tbl.Columns.Clear()
        End If

        OIM0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを品種マスタから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                         AS LINECNT" _
            & " , ''                                                        AS OPERATION" _
            & " , CAST(OIM0003.UPDTIMSTP AS bigint)                         AS TIMSTP" _
            & " , 1                                                         AS 'SELECT'" _
            & " , 0                                                         AS HIDDEN" _
            & " , ISNULL(RTRIM(OIM0003.OFFICECODE), '')                     AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIM0003.SHIPPERCODE), '')                    AS SHIPPERCODE" _
            & " , ISNULL(RTRIM(OIM0003.PLANTCODE), '')                      AS PLANTCODE" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILCODE), '')                     AS BIGOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILNAME), '')                     AS BIGOILNAME" _
            & " , ISNULL(RTRIM(OIM0003.BIGOILKANA), '')                     AS BIGOILKANA" _
            & " , ISNULL(RTRIM(OIM0003.MIDDLEOILCODE), '')                  AS MIDDLEOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.MIDDLEOILNAME), '')                  AS MIDDLEOILNAME" _
            & " , ISNULL(RTRIM(OIM0003.MIDDLEOILKANA), '')                  AS MIDDLEOILKANA" _
            & " , ISNULL(RTRIM(OIM0003.OILCODE), '')                        AS OILCODE" _
            & " , ISNULL(RTRIM(OIM0003.OILNAME), '')                        AS OILNAME" _
            & " , ISNULL(RTRIM(OIM0003.OILKANA), '')                        AS OILKANA" _
            & " , ISNULL(RTRIM(OIM0003.SEGMENTOILCODE), '')                 AS SEGMENTOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.SEGMENTOILNAME), '')                 AS SEGMENTOILNAME" _
            & " , ISNULL(RTRIM(OIM0003.OTOILCODE), '')                      AS OTOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.OTOILNAME), '')                      AS OTOILNAME" _
            & " , ISNULL(RTRIM(OIM0003.SHIPPEROILCODE), '')                 AS SHIPPEROILCODE" _
            & " , ISNULL(RTRIM(OIM0003.SHIPPEROILNAME), '')                 AS SHIPPEROILNAME" _
            & " , ISNULL(RTRIM(OIM0003.CHECKOILCODE), '')                   AS CHECKOILCODE" _
            & " , ISNULL(RTRIM(OIM0003.CHECKOILNAME), '')                   AS CHECKOILNAME" _
            & " , ISNULL(RTRIM(OIM0003.STOCKFLG), '')                       AS STOCKFLG" _
            & " , ISNULL(FORMAT(OIM0003.ORDERFROMDATE, 'yyyy/MM/dd'), '')   AS ORDERFROMDATE" _
            & " , ISNULL(FORMAT(OIM0003.ORDERTODATE, 'yyyy/MM/dd'), '')     AS ORDERTODATE" _
            & " , ISNULL(RTRIM(OIM0003.DELFLG), '')                         AS DELFLG" _
            & " FROM OIL.OIM0003_PRODUCT OIM0003 "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim isAnyWhere As Boolean = False

        '削除フラグ
        If Not String.IsNullOrEmpty(work.WF_SEL_DELFLG.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.DELFLG = @P1"
            isAnyWhere = True
        End If
        '営業所コード
        If Not String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.OFFICECODE = @P2"
            isAnyWhere = True
        End If
        '荷主コード
        If Not String.IsNullOrEmpty(work.WF_SEL_SHIPPERCODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.SHIPPERCODE = @P3"
            isAnyWhere = True
        End If
        '基地コード
        If Not String.IsNullOrEmpty(work.WF_SEL_PLANTCODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.PLANTCODE = @P4"
            isAnyWhere = True
        End If
        '油種大分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_BIGOILCODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.BIGOILCODE = @P5"
            isAnyWhere = True
        End If
        '油種中分類コード
        If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLEOILCODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.MIDDLEOILCODE = @P6"
            isAnyWhere = True
        End If
        '油種コード
        If Not String.IsNullOrEmpty(work.WF_SEL_OILCODE.Text) Then
            If isAnyWhere Then
                SQLStr &= "    AND "
            Else
                SQLStr &= " WHERE "
            End If
            SQLStr &= "OIM0003.OILCODE = @P7"
            isAnyWhere = True
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIM0003.OFFICECODE" _
            & "    , OIM0003.SHIPPERCODE" _
            & "    , OIM0003.PLANTCODE" _
            & "    , OIM0003.BIGOILCODE" _
            & "    , OIM0003.MIDDLEOILCODE" _
            & "    , OIM0003.OILCODE"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                '削除フラグ
                If Not String.IsNullOrEmpty(work.WF_SEL_DELFLG.Text) Then
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 1)
                    PARA1.Value = work.WF_SEL_DELFLG.Text
                End If
                '営業所コード
                If Not String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 6)
                    PARA2.Value = work.WF_SEL_OFFICECODE.Text
                End If
                '荷主コード
                If Not String.IsNullOrEmpty(work.WF_SEL_SHIPPERCODE.Text) Then
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 10)
                    PARA3.Value = work.WF_SEL_SHIPPERCODE.Text
                End If
                '基地コード
                If Not String.IsNullOrEmpty(work.WF_SEL_PLANTCODE.Text) Then
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)
                    PARA4.Value = work.WF_SEL_PLANTCODE.Text
                End If
                '油種大分類コード
                If Not String.IsNullOrEmpty(work.WF_SEL_BIGOILCODE.Text) Then
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)
                    PARA5.Value = work.WF_SEL_BIGOILCODE.Text
                End If
                '油種中分類コード
                If Not String.IsNullOrEmpty(work.WF_SEL_MIDDLEOILCODE.Text) Then
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)
                    PARA6.Value = work.WF_SEL_MIDDLEOILCODE.Text
                End If
                '油種コード
                If Not String.IsNullOrEmpty(work.WF_SEL_OILCODE.Text) Then
                    Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 4)
                    PARA7.Value = work.WF_SEL_OILCODE.Text
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0003row As DataRow In OIM0003tbl.Rows
                    i += 1
                    OIM0003row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003L Select"
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
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            If OIM0003row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0003row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0003tbl)

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
        work.WF_SEL_LINECNT.Text = ""

        '営業所コード
        work.WF_SEL_OFFICECODE2.Text = ""

        '荷主コード
        work.WF_SEL_SHIPPERCODE2.Text = ""

        '基地コード
        work.WF_SEL_PLANTCODE2.Text = ""

        '油種大分類コード
        work.WF_SEL_BIGOILCODE2.Text = ""

        '油種大分類名
        work.WF_SEL_BIGOILNAME.Text = ""

        '油種大分類名カナ
        work.WF_SEL_BIGOILKANA.Text = ""

        '油種中分類コード
        work.WF_SEL_MIDDLEOILCODE2.Text = ""

        '油種中分類名
        work.WF_SEL_MIDDLEOILNAME.Text = ""

        '油種中分類名カナ
        work.WF_SEL_MIDDLEOILKANA.Text = ""

        '油種コード
        work.WF_SEL_OILCODE2.Text = ""

        '油種名
        work.WF_SEL_OILNAME.Text = ""

        '油種名カナ
        work.WF_SEL_OILKANA.Text = ""

        '油種細分コード
        work.WF_SEL_SEGMENTOILCODE.Text = ""

        '油種名（細分）
        work.WF_SEL_SEGMENTOILNAME.Text = ""

        'OT油種コード
        work.WF_SEL_OTOILCODE.Text = ""

        'OT油種名
        work.WF_SEL_OTOILNAME.Text = ""

        '荷主油種コード
        work.WF_SEL_SHIPPEROILCODE.Text = ""

        '荷主油種名
        work.WF_SEL_SHIPPEROILNAME.Text = ""

        '積込チェック用油種コード
        work.WF_SEL_CHECKOILCODE.Text = ""

        '積込チェック用油種名
        work.WF_SEL_CHECKOILNAME.Text = ""

        '在庫管理対象フラグ
        work.WF_SEL_STOCKFLG.Text = ""

        '受注登録可能期間FROM
        work.WF_SEL_ORDERFROMDATE.Text = ""

        '受注登録可能期間TO
        work.WF_SEL_ORDERTODATE.Text = ""

        '削除フラグ
        work.WF_SEL_DELFLG2.Text = "0"

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0003tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

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

    End Sub


    ''' <summary>
    ''' 品種マスタ登録更新
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
            & "        OIL.OIM0003_PRODUCT" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P10" _
            & "        AND SEGMENTOILCODE = @P13;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0003_PRODUCT" _
            & "    SET" _
            & "        OFFICECODE     = @P01" _
            & "        , SHIPPERCODE    = @P02" _
            & "        , PLANTCODE      = @P03" _
            & "        , BIGOILCODE     = @P04" _
            & "        , BIGOILNAME     = @P05" _
            & "        , BIGOILKANA     = @P06" _
            & "        , MIDDLEOILCODE  = @P07" _
            & "        , MIDDLEOILNAME  = @P08" _
            & "        , MIDDLEOILKANA  = @P09" _
            & "        , OILCODE        = @P10" _
            & "        , OILNAME        = @P11" _
            & "        , OILKANA        = @P12" _
            & "        , SEGMENTOILCODE = @P13" _
            & "        , SEGMENTOILNAME = @P14" _
            & "        , OTOILCODE      = @P15" _
            & "        , OTOILNAME      = @P16" _
            & "        , SHIPPEROILCODE = @P17" _
            & "        , SHIPPEROILNAME = @P18" _
            & "        , CHECKOILCODE   = @P19" _
            & "        , CHECKOILNAME   = @P20" _
            & "        , STOCKFLG       = @P21" _
            & "        , ORDERFROMDATE  = @P22" _
            & "        , ORDERTODATE    = @P23" _
            & "        , DELFLG         = @P24" _
            & "        , UPDYMD         = @P28" _
            & "        , UPDUSER        = @P29" _
            & "        , UPDTERMID      = @P30" _
            & "        , RECEIVEYMD     = @P31" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P10" _
            & "        AND SEGMENTOILCODE = @P13;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0003_PRODUCT" _
            & "        ( OFFICECODE" _
            & "        , SHIPPERCODE" _
            & "        , PLANTCODE" _
            & "        , BIGOILCODE" _
            & "        , BIGOILNAME" _
            & "        , BIGOILKANA" _
            & "        , MIDDLEOILCODE" _
            & "        , MIDDLEOILNAME" _
            & "        , MIDDLEOILKANA" _
            & "        , OILCODE" _
            & "        , OILNAME" _
            & "        , OILKANA" _
            & "        , SEGMENTOILCODE" _
            & "        , SEGMENTOILNAME" _
            & "        , OTOILCODE" _
            & "        , OTOILNAME" _
            & "        , SHIPPEROILCODE" _
            & "        , SHIPPEROILNAME" _
            & "        , CHECKOILCODE" _
            & "        , CHECKOILNAME" _
            & "        , STOCKFLG" _
            & "        , ORDERFROMDATE" _
            & "        , ORDERTODATE" _
            & "        , DELFLG" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , RECEIVEYMD )" _
            & "    VALUES" _
            & "        ( @P01" _
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
            & "        , @P25" _
            & "        , @P26" _
            & "        , @P27" _
            & "        , @P31 );" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    OFFICECODE" _
            & "    , SHIPPERCODE" _
            & "    , PLANTCODE" _
            & "    , BIGOILCODE" _
            & "    , BIGOILNAME" _
            & "    , BIGOILKANA" _
            & "    , MIDDLEOILCODE" _
            & "    , MIDDLEOILNAME" _
            & "    , MIDDLEOILKANA" _
            & "    , OILCODE" _
            & "    , OILNAME" _
            & "    , OILKANA" _
            & "    , SEGMENTOILCODE" _
            & "    , SEGMENTOILNAME" _
            & "    , OTOILCODE" _
            & "    , OTOILNAME" _
            & "    , SHIPPEROILCODE" _
            & "    , SHIPPEROILNAME" _
            & "    , CHECKOILCODE" _
            & "    , CHECKOILNAME" _
            & "    , STOCKFLG" _
            & "    , ORDERFROMDATE" _
            & "    , ORDERTODATE" _
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
            & "    OIL.OIM0003_PRODUCT" _
            & " WHERE" _
            & "    OFFICECODE         = @P01" _
            & "    AND SHIPPERCODE    = @P02" _
            & "    AND PLANTCODE      = @P03" _
            & "    AND OILCODE        = @P04" _
            & "    AND SEGMENTOILCODE = @P05;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)           '営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)          '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)           '基地コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)           '油種大分類コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 10)          '油種大分類名
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10)          '油種大分類名カナ
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)           '油種中分類コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20)          '油種中分類名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)          '油種中分類名カナ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4)           '油種コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40)          '油種名
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40)          '油種名カナ
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)           '油種細分コード
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40)          '油種名（細分）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4)           'OT油種コード
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)          'OT油種名
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)          '荷主油種コード
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 40)          '荷主油種名
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4)           '積込チェック用油種コード
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40)          '積込チェック用油種名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 1)           '在庫管理対象フラグ
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)                  '受注登録可能期間FROM
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Date)                  '受注登録可能期間TO
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)              '登録年月日
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 20)          '登録ユーザーID
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)              '更新年月日
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)              '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 6)        '営業所コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 10)       '荷主コード
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 4)        '基地コード
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 4)        '油種コード
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 1)        '油種細分コード

                For Each OIM0003row As DataRow In OIM0003tbl.Rows
                    If Trim(OIM0003row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0003row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0003row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA01.Value = OIM0003row("OFFICECODE")
                        PARA02.Value = OIM0003row("SHIPPERCODE")
                        PARA03.Value = OIM0003row("PLANTCODE")
                        PARA04.Value = OIM0003row("BIGOILCODE")
                        PARA05.Value = OIM0003row("BIGOILNAME")
                        PARA06.Value = OIM0003row("BIGOILKANA")
                        PARA07.Value = OIM0003row("MIDDLEOILCODE")
                        PARA08.Value = OIM0003row("MIDDLEOILNAME")
                        PARA09.Value = OIM0003row("MIDDLEOILKANA")
                        PARA10.Value = OIM0003row("OILCODE")
                        PARA11.Value = OIM0003row("OILNAME")
                        PARA12.Value = OIM0003row("OILKANA")
                        PARA13.Value = OIM0003row("SEGMENTOILCODE")
                        PARA14.Value = OIM0003row("SEGMENTOILNAME")
                        PARA15.Value = OIM0003row("OTOILCODE")
                        PARA16.Value = OIM0003row("OTOILNAME")
                        PARA17.Value = OIM0003row("SHIPPEROILCODE")
                        PARA18.Value = OIM0003row("SHIPPEROILNAME")
                        PARA19.Value = OIM0003row("CHECKOILCODE")
                        PARA20.Value = OIM0003row("CHECKOILNAME")
                        PARA21.Value = OIM0003row("STOCKFLG")
                        PARA22.Value = OIM0003row("ORDERFROMDATE")
                        PARA23.Value = OIM0003row("ORDERTODATE")
                        PARA24.Value = OIM0003row("DELFLG")
                        PARA25.Value = WW_DATENOW
                        PARA26.Value = Master.USERID
                        PARA27.Value = Master.USERTERMID
                        PARA28.Value = WW_DATENOW
                        PARA29.Value = Master.USERID
                        PARA30.Value = Master.USERTERMID
                        PARA31.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0003row("OFFICECODE")
                        JPARA02.Value = OIM0003row("SHIPPERCODE")
                        JPARA03.Value = OIM0003row("PLANTCODE")
                        JPARA04.Value = OIM0003row("OILCODE")
                        JPARA05.Value = OIM0003row("SEGMENTOILCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0003UPDtbl) Then
                                OIM0003UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0003UPDtbl.Clear()
                            OIM0003UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0003UPDrow As DataRow In OIM0003UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0003L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0003UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0003tbl                       'データ参照  Table
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
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0003tbl                       'データ参照Table
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
        Dim TBLview As New DataView(OIM0003tbl)
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
        work.WF_SEL_OFFICECODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("OFFICECODE")

        '営業所コード
        work.WF_SEL_SHIPPERCODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("SHIPPERCODE")

        '基地コード
        work.WF_SEL_PLANTCODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("PLANTCODE")

        '油種大分類コード
        work.WF_SEL_BIGOILCODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("BIGOILCODE")

        '油種大分類名
        work.WF_SEL_BIGOILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("BIGOILNAME")

        '油種大分類名カナ
        work.WF_SEL_BIGOILKANA.Text = OIM0003tbl.Rows(WW_LINECNT)("BIGOILKANA")

        '油種中分類コード
        work.WF_SEL_MIDDLEOILCODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("MIDDLEOILCODE")

        '油種中分類名
        work.WF_SEL_MIDDLEOILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("MIDDLEOILNAME")

        '油種中分類名カナ
        work.WF_SEL_MIDDLEOILKANA.Text = OIM0003tbl.Rows(WW_LINECNT)("MIDDLEOILKANA")

        '油種コード
        work.WF_SEL_OILCODE2.Text = OIM0003tbl.Rows(WW_LINECNT)("OILCODE")

        '油種名
        work.WF_SEL_OILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("OILNAME")

        '油種名カナ
        work.WF_SEL_OILKANA.Text = OIM0003tbl.Rows(WW_LINECNT)("OILKANA")

        '油種細分コード
        work.WF_SEL_SEGMENTOILCODE.Text = OIM0003tbl.Rows(WW_LINECNT)("SEGMENTOILCODE")

        '油種名（細分）
        work.WF_SEL_SEGMENTOILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("SEGMENTOILNAME")

        'OT油種コード
        work.WF_SEL_OTOILCODE.Text = OIM0003tbl.Rows(WW_LINECNT)("OTOILCODE")

        'OT油種名
        work.WF_SEL_OTOILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("OTOILNAME")

        '荷主油種コード
        work.WF_SEL_SHIPPEROILCODE.Text = OIM0003tbl.Rows(WW_LINECNT)("SHIPPEROILCODE")

        '荷主油種名
        work.WF_SEL_SHIPPEROILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("SHIPPEROILNAME")

        '積込チェック用油種コード
        work.WF_SEL_CHECKOILCODE.Text = OIM0003tbl.Rows(WW_LINECNT)("CHECKOILCODE")

        '積込チェック用油種名
        work.WF_SEL_CHECKOILNAME.Text = OIM0003tbl.Rows(WW_LINECNT)("CHECKOILNAME")

        '在庫管理対象フラグ
        work.WF_SEL_STOCKFLG.Text = OIM0003tbl.Rows(WW_LINECNT)("STOCKFLG")

        '受注登録可能期間FROM
        work.WF_SEL_ORDERFROMDATE.Text = OIM0003tbl.Rows(WW_LINECNT)("ORDERFROMDATE")

        '受注登録可能期間TO
        work.WF_SEL_ORDERTODATE.Text = OIM0003tbl.Rows(WW_LINECNT)("ORDERTODATE")

        '削除フラグ
        work.WF_SEL_DELFLG2.Text = OIM0003tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIM0003tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0003INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0003INProw As DataRow = OIM0003INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0003INPcol As DataColumn In OIM0003INPtbl.Columns
                If IsDBNull(OIM0003INProw.Item(OIM0003INPcol)) OrElse IsNothing(OIM0003INProw.Item(OIM0003INPcol)) Then
                    Select Case OIM0003INPcol.ColumnName
                        Case "LINECNT"
                            OIM0003INProw.Item(OIM0003INPcol) = 0
                        Case "OPERATION"
                            OIM0003INProw.Item(OIM0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            OIM0003INProw.Item(OIM0003INPcol) = 0
                        Case "SELECT"
                            OIM0003INProw.Item(OIM0003INPcol) = 1
                        Case "HIDDEN"
                            OIM0003INProw.Item(OIM0003INPcol) = 0
                        Case Else
                            OIM0003INProw.Item(OIM0003INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SHIPPERCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PLANTCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OILCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("SEGMENTOILCODE") >= 0 Then
                For Each OIM0003row As DataRow In OIM0003tbl.Rows
                    If XLSTBLrow("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                        XLSTBLrow("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                        XLSTBLrow("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                        XLSTBLrow("BIGOILCODE") = OIM0003row("BIGOILCODE") AndAlso
                        XLSTBLrow("BIGOILNAME") = OIM0003row("BIGOILNAME") AndAlso
                        XLSTBLrow("BIGOILKANA") = OIM0003row("BIGOILKANA") AndAlso
                        XLSTBLrow("MIDDLEOILCODE") = OIM0003row("MIDDLEOILCODE") AndAlso
                        XLSTBLrow("MIDDLEOILNAME") = OIM0003row("MIDDLEOILNAME") AndAlso
                        XLSTBLrow("MIDDLEOILKANA") = OIM0003row("MIDDLEOILKANA") AndAlso
                        XLSTBLrow("OILCODE") = OIM0003row("OILCODE") AndAlso
                        XLSTBLrow("OILNAME") = OIM0003row("OILNAME") AndAlso
                        XLSTBLrow("OILKANA") = OIM0003row("OILKANA") AndAlso
                        XLSTBLrow("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") AndAlso
                        XLSTBLrow("SEGMENTOILNAME") = OIM0003row("SEGMENTOILNAME") AndAlso
                        XLSTBLrow("OTOILCODE") = OIM0003row("OTOILCODE") AndAlso
                        XLSTBLrow("OTOILNAME") = OIM0003row("OTOILNAME") AndAlso
                        XLSTBLrow("SHIPPEROILCODE") = OIM0003row("SHIPPEROILCODE") AndAlso
                        XLSTBLrow("SHIPPEROILNAME") = OIM0003row("SHIPPEROILNAME") AndAlso
                        XLSTBLrow("CHECKOILCODE") = OIM0003row("CHECKOILCODE") AndAlso
                        XLSTBLrow("CHECKOILNAME") = OIM0003row("CHECKOILNAME") AndAlso
                        XLSTBLrow("STOCKFLG") = OIM0003row("STOCKFLG") AndAlso
                        XLSTBLrow("ORDERFROMDATE") = OIM0003row("ORDERFROMDATE") AndAlso
                        XLSTBLrow("ORDERTODATE") = OIM0003row("ORDERTODATE") Then

                        OIM0003INProw.ItemArray = OIM0003row.ItemArray
                        Exit For
                    End If
                Next
            End If
            '○ 項目セット

            '営業所コード
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
                OIM0003INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
            End If

            '荷主コード
            If WW_COLUMNS.IndexOf("SHIPPERCODE") >= 0 Then
                OIM0003INProw("SHIPPERCODE") = XLSTBLrow("SHIPPERCODE")
            End If

            '基地コード
            If WW_COLUMNS.IndexOf("PLANTCODE") >= 0 Then
                OIM0003INProw("PLANTCODE") = XLSTBLrow("PLANTCODE")
            End If

            '油種大分類コード
            If WW_COLUMNS.IndexOf("BIGOILCODE") >= 0 Then
                OIM0003INProw("BIGOILCODE") = XLSTBLrow("BIGOILCODE")
            End If

            '油種大分類名
            If WW_COLUMNS.IndexOf("BIGOILNAME") >= 0 Then
                OIM0003INProw("BIGOILNAME") = XLSTBLrow("BIGOILNAME")
            End If

            '油種大分類名カナ
            If WW_COLUMNS.IndexOf("BIGOILKANA") >= 0 Then
                OIM0003INProw("BIGOILKANA") = XLSTBLrow("BIGOILKANA")
            End If

            '油種中分類コード
            If WW_COLUMNS.IndexOf("MIDDLEOILCODE") >= 0 Then
                OIM0003INProw("MIDDLEOILCODE") = XLSTBLrow("MIDDLEOILCODE")
            End If

            '油種中分類名
            If WW_COLUMNS.IndexOf("MIDDLEOILNAME") >= 0 Then
                OIM0003INProw("MIDDLEOILNAME") = XLSTBLrow("MIDDLEOILNAME")
            End If

            '油種中分類名カナ
            If WW_COLUMNS.IndexOf("MIDDLEOILKANA") >= 0 Then
                OIM0003INProw("MIDDLEOILKANA") = XLSTBLrow("MIDDLEOILKANA")
            End If

            '油種コード
            If WW_COLUMNS.IndexOf("OILCODE") >= 0 Then
                OIM0003INProw("OILCODE") = XLSTBLrow("OILCODE")
            End If

            '油種名
            If WW_COLUMNS.IndexOf("OILNAME") >= 0 Then
                OIM0003INProw("OILNAME") = XLSTBLrow("OILNAME")
            End If

            '油種名カナ
            If WW_COLUMNS.IndexOf("OILKANA") >= 0 Then
                OIM0003INProw("OILKANA") = XLSTBLrow("OILKANA")
            End If

            '油種細分コード
            If WW_COLUMNS.IndexOf("SEGMENTOILCODE") >= 0 Then
                OIM0003INProw("SEGMENTOILCODE") = XLSTBLrow("SEGMENTOILCODE")
            End If

            '油種名（細分）
            If WW_COLUMNS.IndexOf("SEGMENTOILNAME") >= 0 Then
                OIM0003INProw("SEGMENTOILNAME") = XLSTBLrow("SEGMENTOILNAME")
            End If

            'OT油種コード
            If WW_COLUMNS.IndexOf("OTOILCODE") >= 0 Then
                OIM0003INProw("OTOILCODE") = XLSTBLrow("OTOILCODE")
            End If

            'OT油種名
            If WW_COLUMNS.IndexOf("OTOILNAME") >= 0 Then
                OIM0003INProw("OTOILNAME") = XLSTBLrow("OTOILNAME")
            End If

            '荷主油種コード
            If WW_COLUMNS.IndexOf("SHIPPEROILCODE") >= 0 Then
                OIM0003INProw("SHIPPEROILCODE") = XLSTBLrow("SHIPPEROILCODE")
            End If

            '荷主油種名
            If WW_COLUMNS.IndexOf("SHIPPEROILNAME") >= 0 Then
                OIM0003INProw("SHIPPEROILNAME") = XLSTBLrow("SHIPPEROILNAME")
            End If

            '積込チェック用油種コード
            If WW_COLUMNS.IndexOf("CHECKOILCODE") >= 0 Then
                OIM0003INProw("CHECKOILCODE") = XLSTBLrow("CHECKOILCODE")
            End If

            '積込チェック用油種名
            If WW_COLUMNS.IndexOf("CHECKOILNAME") >= 0 Then
                OIM0003INProw("CHECKOILNAME") = XLSTBLrow("CHECKOILNAME")
            End If

            '在庫管理対象フラグ
            If WW_COLUMNS.IndexOf("STOCKFLG") >= 0 Then
                OIM0003INProw("STOCKFLG") = XLSTBLrow("STOCKFLG")
            End If

            '受注登録可能期間FROM
            If WW_COLUMNS.IndexOf("ORDERFROMDATE") >= 0 Then
                OIM0003INProw("ORDERFROMDATE") = XLSTBLrow("ORDERFROMDATE")
            End If

            '受注登録可能期間TO
            If WW_COLUMNS.IndexOf("ORDERTODATE") >= 0 Then
                OIM0003INProw("ORDERTODATE") = XLSTBLrow("ORDERTODATE")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0003INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIM0003INProw("DELFLG") = "0"
            End If

            OIM0003INPtbl.Rows.Add(OIM0003INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0003tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

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
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl)

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
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            WW_TEXT = OIM0003INProw("DELFLG")
            Master.CheckField(Master.USERCAMP, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("DELFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '営業所コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OFFICECODE")
            Master.CheckField(Master.USERCAMP, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("OFFICECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SHIPPERCODE")
            Master.CheckField(Master.USERCAMP, "SHIPPERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("SHIPPERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("PLANTCODE")
            Master.CheckField(Master.USERCAMP, "PLANTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("PLANTCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("BIGOILCODE")
            Master.CheckField(Master.USERCAMP, "BIGOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("BIGOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("BIGOILNAME")
            Master.CheckField(Master.USERCAMP, "BIGOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名カナ(バリデーションチェック）
            WW_TEXT = OIM0003INProw("BIGOILKANA")
            Master.CheckField(Master.USERCAMP, "BIGOILKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("MIDDLEOILCODE")
            Master.CheckField(Master.USERCAMP, "MIDDLEOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("MIDDLEOILNAME")
            Master.CheckField(Master.USERCAMP, "MIDDLEOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名カナ(バリデーションチェック）
            WW_TEXT = OIM0003INProw("MIDDLEOILKANA")
            Master.CheckField(Master.USERCAMP, "MIDDLEOILKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OILCODE")
            Master.CheckField(Master.USERCAMP, "OILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OILNAME")
            Master.CheckField(Master.USERCAMP, "OILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名カナ(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OILKANA")
            Master.CheckField(Master.USERCAMP, "OILKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種細分コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SEGMENTOILCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名（細分）(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SEGMENTOILNAME")
            Master.CheckField(Master.USERCAMP, "SEGMENTOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名（細分）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OTOILCODE")
            Master.CheckField(Master.USERCAMP, "OTOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("OTOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("OTOILNAME")
            Master.CheckField(Master.USERCAMP, "OTOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SHIPPEROILCODE")
            Master.CheckField(Master.USERCAMP, "SHIPPEROILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SHIPPEROILNAME")
            Master.CheckField(Master.USERCAMP, "SHIPPEROILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種コード(バリデーションチェック）
            WW_TEXT = OIM0003INProw("CHECKOILCODE")
            Master.CheckField(Master.USERCAMP, "CHECKOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("CHECKOILNAME")
            Master.CheckField(Master.USERCAMP, "CHECKOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '在庫管理対象フラグ(バリデーションチェック）
            WW_TEXT = OIM0003INProw("STOCKFLG")
            Master.CheckField(Master.USERCAMP, "STOCKFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("STOCKFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間FROM(バリデーションチェック）
            WW_TEXT = OIM0003INProw("ORDERFROMDATE")
            Master.CheckField(Master.USERCAMP, "ORDERFROMDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "受注登録可能期間FROM", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間TO(バリデーションチェック）
            WW_TEXT = OIM0003INProw("ORDERTODATE")
            Master.CheckField(Master.USERCAMP, "ORDERTODATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "受注登録可能期間TO", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        O_RTN = "ERR"
                        Exit Sub
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 営業所コード             =" & OIM0003row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード               =" & OIM0003row("SHIPPERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード               =" & OIM0003row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード         =" & OIM0003row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名             =" & OIM0003row("BIGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名カナ         =" & OIM0003row("BIGOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード         =" & OIM0003row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名             =" & OIM0003row("MIDDLEOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名カナ         =" & OIM0003row("MIDDLEOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード               =" & OIM0003row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名                   =" & OIM0003row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名カナ               =" & OIM0003row("OILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード           =" & OIM0003row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名（細分）           =" & OIM0003row("SEGMENTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種コード             =" & OIM0003row("OTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種名                 =" & OIM0003row("OTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種コード           =" & OIM0003row("SHIPPEROILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種名               =" & OIM0003row("SHIPPEROILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種コード =" & OIM0003row("CHECKOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種名     =" & OIM0003row("CHECKOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 在庫管理対象フラグ       =" & OIM0003row("STOCKFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間FROM     =" & OIM0003row("ORDERFROMDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間TO       =" & OIM0003row("ORDERTODATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ               =" & OIM0003row("DELFLG")
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
    ''' OIM0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0003tbl_UPD()

        '○ 画面状態設定
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0003INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0003row As DataRow In OIM0003tbl.Rows
                If OIM0003row("OFFICECODE") = OIM0003INProw("OFFICECODE") AndAlso
                    OIM0003row("SHIPPERCODE") = OIM0003INProw("SHIPPERCODE") AndAlso
                    OIM0003row("PLANTCODE") = OIM0003INProw("PLANTCODE") AndAlso
                    OIM0003row("OILCODE") = OIM0003INProw("OILCODE") AndAlso
                    OIM0003row("SEGMENTOILCODE") = OIM0003INProw("SEGMENTOILCODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0003row("DELFLG") = OIM0003INProw("DELFLG") AndAlso
                        OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0003INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows
            Select Case OIM0003INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0003INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0003INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0003INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0003INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0003INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0003row As DataRow = OIM0003tbl.NewRow
        OIM0003row.ItemArray = OIM0003INProw.ItemArray

        OIM0003row("LINECNT") = OIM0003tbl.Rows.Count + 1
        If OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0003row("TIMSTP") = "0"
        OIM0003row("SELECT") = 1
        OIM0003row("HIDDEN") = 0

        OIM0003tbl.Rows.Add(OIM0003row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
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
                Case "OFFICECODE"
                    '営業所コード
                    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERCODE"
                    '荷主コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    '基地コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"
                    '油種大分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"
                    '油種中分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTOILCODE"
                    'OT油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STOCKFLG"
                    '在庫管理対象フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    '削除フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

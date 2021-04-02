Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 列車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0007TrainList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0007tbl As DataTable                                 ' 一覧格納用テーブル
    Private OIM0007INPtbl As DataTable                              ' チェック用テーブル
    Private OIM0007UPDtbl As DataTable                              ' 更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                ' 1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 ' マウススクロール時稼働行数

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 ' データ追加
    Private Const CONST_UPDATE As String = "Update"                 ' データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         ' 関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    ' ログ出力
    Private CS0013ProfView As New CS0013ProfView                    ' Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      ' 更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  ' XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  ' 権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        ' 帳票出力
    Private CS0050SESSION As New CS0050SESSION                      ' セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    ' サブ用リターンコード

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
                    Master.RecoverTable(OIM0007tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"          ' 追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          ' DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             ' ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           ' 一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             ' 戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           ' 先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            ' 最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_GridDBclick"           ' GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          ' マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        ' マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          ' ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"       ' (右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            ' (右ボックス)メモ欄更新
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
            If Not IsNothing(OIM0007tbl) Then
                OIM0007tbl.Clear()
                OIM0007tbl.Dispose()
                OIM0007tbl = Nothing
            End If

            If Not IsNothing(OIM0007INPtbl) Then
                OIM0007INPtbl.Clear()
                OIM0007INPtbl.Dispose()
                OIM0007INPtbl = Nothing
            End If

            If Not IsNothing(OIM0007UPDtbl) Then
                OIM0007UPDtbl.Clear()
                OIM0007UPDtbl.Dispose()
                OIM0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0007WRKINC.MAPIDL
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

        ' 右Boxへの値設定
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0007S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0007C Then
            Master.RecoverTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        ' 登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0007C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

        '○ 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0007tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0007tbl)

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

        If IsNothing(OIM0007tbl) Then
            OIM0007tbl = New DataTable
        End If

        If OIM0007tbl.Columns.Count <> 0 Then
            OIM0007tbl.Columns.Clear()
        End If

        OIM0007tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを列車マスタから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT ")
        SQLStrBldr.AppendLine("       0                                               AS LINECNT ")          ' 行番号
        SQLStrBldr.AppendLine("     , ''                                              AS OPERATION ")        ' 編集
        SQLStrBldr.AppendLine("     , CAST(OIM0007.UPDTIMSTP AS bigint)               AS UPDTIMSTP ")        ' タイムスタンプ
        SQLStrBldr.AppendLine("     , 1                                               AS 'SELECT' ")         ' 選択
        SQLStrBldr.AppendLine("     , 0                                               AS HIDDEN ")           ' 非表示
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OFFICECODE), '')           AS OFFICECODE ")       ' 管轄受注営業所
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINNO), '')              AS TRAINNO ")          ' 本線列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINNAME), '')            AS TRAINNAME ")        ' 本線列車番号名
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TSUMI), '')                AS TSUMI ")            ' 積置フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OTTRAINNO), '')            AS OTTRAINNO ")        ' OT列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.OTFLG), '')                AS OTFLG ")            ' OT発送日報送信フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DEPSTATION), '')           AS DEPSTATION ")       ' 発駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ARRSTATION), '')           AS ARRSTATION ")       ' 着駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO1), '')           AS JRTRAINNO1 ")       ' JR発列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK1), '')             AS MAXTANK1 ")         ' JR発列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO2), '')           AS JRTRAINNO2 ")       ' JR中継列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK2), '')             AS MAXTANK2 ")         ' JR中継列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.JRTRAINNO3), '')           AS JRTRAINNO3 ")       ' JR最終列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MAXTANK3), '')             AS MAXTANK3 ")         ' JR最終列車牽引車数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.TRAINCLASS), '')           AS TRAINCLASS ")       ' 列車区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.SPEEDCLASS), '')           AS SPEEDCLASS ")       ' 高速列車区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.SHIPORDERCLASS), '')       AS SHIPORDERCLASS ")   ' 発送順区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DEPDAYS), '')              AS DEPDAYS ")          ' 発日日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.MARGEDAYS), '')            AS MARGEDAYS ")        ' 特継日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ARRDAYS), '')              AS ARRDAYS ")          ' 積車着日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ACCDAYS), '')              AS ACCDAYS ")          ' 受入日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.EMPARRDAYS), '')           AS EMPARRDAYS ")       ' 空車着日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.USEDAYS), '')              AS USEDAYS ")          ' 当日利用日数
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.FEEKBN), '')               AS FEEKBN ")           ' 料金マスタ区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.RUN), '')                  AS RUN ")              ' 稼働フラグ
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.ZAIKOSORT), '')            AS ZAIKOSORT ")        ' 在庫管理表表示ソート区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.BIKOU), '')                AS BIKOU ")            ' 備考
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0007.DELFLG), '')               AS DELFLG ")           ' 削除フラグ
        SQLStrBldr.AppendLine(" FROM ")
        SQLStrBldr.AppendLine("     [oil].OIM0007_TRAIN OIM0007 ")

        '○ 条件指定
        Dim andFlg As Boolean = False

        ' 管轄受注営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
            SQLStrBldr.AppendLine(" WHERE ")
            SQLStrBldr.AppendLine("     OIM0007.OFFICECODE = @P1 ")
            andFlg = True
        Else
            SQLStrBldr.AppendLine(" WHERE ")
            SQLStrBldr.AppendLine("     OIM0007.OFFICECODE IN (SELECT OFFICECODE FROM OIL.VIW0003_OFFICECHANGE WHERE ORGCODE = @P1) ")
            andFlg = True
        End If

        ' 本線列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNO.Text) Then
            If andFlg Then
                SQLStrBldr.AppendLine("     AND ")
            Else
                SQLStrBldr.AppendLine(" WHERE ")
            End If
            SQLStrBldr.AppendLine("     OIM0007.TRAINNO = @P2 ")
            andFlg = True
        End If

        ' 積置フラグ
        If Not String.IsNullOrEmpty(work.WF_SEL_TSUMI.Text) Then
            If andFlg Then
                SQLStrBldr.AppendLine("     AND ")
            Else
                SQLStrBldr.AppendLine(" WHERE ")
            End If
            SQLStrBldr.AppendLine("     OIM0007.TSUMI = @P3 ")
            andFlg = True
        End If

        '削除フラグ
        If andFlg Then
            SQLStrBldr.AppendLine("     AND ")
        Else
            SQLStrBldr.AppendLine(" WHERE ")
        End If
        SQLStrBldr.AppendLine("     OIM0007.DELFLG = @P0 ")


        '○ ソート
        SQLStrBldr.AppendLine(" ORDER BY ")
        SQLStrBldr.AppendLine("     OIM0007.OFFICECODE ")
        SQLStrBldr.AppendLine("     , OIM0007.TRAINNO ")
        SQLStrBldr.AppendLine("     , OIM0007.TSUMI ")
        SQLStrBldr.AppendLine("     , OIM0007.OTTRAINNO ")
        SQLStrBldr.AppendLine("     , OIM0007.DEPSTATION ")
        SQLStrBldr.AppendLine("     , OIM0007.ARRSTATION ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)     ' 営業所
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 4)     ' 本線列車番号
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)     ' 積置フラグ
                Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", SqlDbType.NVarChar, 1)     ' 削除フラグ

                '営業所
                If String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
                    PARA1.Value = Master.USER_ORG
                Else
                    PARA1.Value = work.WF_SEL_OFFICECODE.Text
                End If

                '本線車番
                PARA2.Value = work.WF_SEL_TRAINNO.Text

                '積置フラグ
                PARA3.Value = work.WF_SEL_TSUMI.Text

                '削除フラグ
                PARA0.Value = C_DELETE_FLG.ALIVE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0007tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0007tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0007row As DataRow In OIM0007tbl.Rows
                    i += 1
                    OIM0007row("LINECNT") = i        ' LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0007L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0007L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          ' 表示位置(開始)
        Dim WW_DataCNT As Integer = 0           ' (絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            If OIM0007row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0007row("SELECT") = WW_DataCNT
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

        ' 表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        ' 表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(OIM0007tbl)

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

        ' 選択行
        work.WF_SEL_LINECNT.Text = ""

        ' 管轄受注営業所
        work.WF_SEL_OFFICECODE2.Text = ""

        ' 本線列車番号
        work.WF_SEL_TRAINNO2.Text = ""

        ' 本線列車番号名
        work.WF_SEL_TRAINNAME.Text = ""

        ' 積置フラグ
        work.WF_SEL_TSUMI2.Text = ""

        ' OT列車番号
        work.WF_SEL_OTTRAINNO.Text = ""

        ' OT発送日報送信フラグ
        work.WF_SEL_OTFLG.Text = ""

        ' 発駅コード
        work.WF_SEL_DEPSTATION.Text = ""

        ' 着駅コード
        work.WF_SEL_ARRSTATION.Text = ""

        ' JR発列車番号
        work.WF_SEL_JRTRAINNO1.Text = ""

        ' JR発列車牽引車数
        work.WF_SEL_MAXTANK1.Text = ""

        ' JR中継列車番号
        work.WF_SEL_JRTRAINNO2.Text = ""

        ' JR中継列車牽引車数
        work.WF_SEL_MAXTANK2.Text = ""

        ' JR最終列車番号
        work.WF_SEL_JRTRAINNO3.Text = ""

        ' JR最終列車牽引車数
        work.WF_SEL_MAXTANK3.Text = ""

        ' 列車区分
        work.WF_SEL_TRAINCLASS.Text = ""

        ' 高速列車区分
        work.WF_SEL_SPEEDCLASS.Text = ""

        ' 発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = ""

        ' 発日日数
        work.WF_SEL_DEPDAYS.Text = ""

        ' 特継日数
        work.WF_SEL_MARGEDAYS.Text = ""

        ' 積車着日数
        work.WF_SEL_ARRDAYS.Text = ""

        ' 受入日数
        work.WF_SEL_ACCDAYS.Text = ""

        ' 空車着日数
        work.WF_SEL_EMPARRDAYS.Text = ""

        ' 当日利用日数
        work.WF_SEL_USEDAYS.Text = ""

        ' 料金マスタ区分
        work.WF_SEL_FEEKBN.Text = ""

        ' 稼働フラグ
        work.WF_SEL_RUN.Text = ""

        ' 在庫管理表表示ソート区分
        work.WF_SEL_ZAIKOSORT.Text = ""

        ' 備考
        work.WF_SEL_BIKOU.Text = ""

        ' 削除フラグ
        work.WF_SEL_DELFLG.Text = ""

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)

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

        '○ 関連チェック
        RelatedCheck(WW_ERRCODE)

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                'マスタ更新
                UpdateMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

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

        '○ 初期値設定
        O_RTNCODE = C_MESSAGE_NO.NORMAL

        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

    End Sub


    ''' <summary>
    ''' 列車マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" DECLARE @hensuu AS bigint; ")
        SQLStrBldr.AppendLine("    SET @hensuu = 0; ")
        SQLStrBldr.AppendLine(" DECLARE hensuu CURSOR FOR ")
        SQLStrBldr.AppendLine("    SELECT ")
        SQLStrBldr.AppendLine("        CAST(UPDTIMSTP AS bigint) AS hensuu ")
        SQLStrBldr.AppendLine("    FROM ")
        SQLStrBldr.AppendLine("        OIL.OIM0007_TRAIN ")
        SQLStrBldr.AppendLine("    WHERE ")
        SQLStrBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        TRAINNO = @P01 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        TSUMI = @P03 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        OTTRAINNO = @P04 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        DEPSTATION = @P06 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        ARRSTATION = @P07; ")
        SQLStrBldr.AppendLine(" OPEN hensuu; ")
        SQLStrBldr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu; ")
        SQLStrBldr.AppendLine(" IF (@@FETCH_STATUS = 0) ")
        SQLStrBldr.AppendLine("     UPDATE OIL.OIM0007_TRAIN ")
        SQLStrBldr.AppendLine("     SET ")
        SQLStrBldr.AppendLine("         TRAINNAME = @P02 ")
        SQLStrBldr.AppendLine("         , OTFLG = @P05 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO1 = @P08 ")
        SQLStrBldr.AppendLine("         , MAXTANK1 = @P09 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO2 = @P10 ")
        SQLStrBldr.AppendLine("         , MAXTANK2 = @P11 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO3 = @P12 ")
        SQLStrBldr.AppendLine("         , MAXTANK3 = @P13 ")
        SQLStrBldr.AppendLine("         , TRAINCLASS = @P14 ")
        SQLStrBldr.AppendLine("         , SPEEDCLASS = @P15 ")
        SQLStrBldr.AppendLine("         , SHIPORDERCLASS = @P16 ")
        SQLStrBldr.AppendLine("         , DEPDAYS = @P17 ")
        SQLStrBldr.AppendLine("         , MARGEDAYS = @P18 ")
        SQLStrBldr.AppendLine("         , ARRDAYS = @P19 ")
        SQLStrBldr.AppendLine("         , ACCDAYS = @P20 ")
        SQLStrBldr.AppendLine("         , EMPARRDAYS = @P21 ")
        SQLStrBldr.AppendLine("         , USEDAYS = @P22 ")
        SQLStrBldr.AppendLine("         , FEEKBN = @P23 ")
        SQLStrBldr.AppendLine("         , RUN = @P24 ")
        SQLStrBldr.AppendLine("         , ZAIKOSORT = @P25 ")
        SQLStrBldr.AppendLine("         , BIKOU = @P26 ")
        SQLStrBldr.AppendLine("         , DELFLG = @P27 ")
        SQLStrBldr.AppendLine("         , UPDYMD = @P31 ")
        SQLStrBldr.AppendLine("         , UPDUSER = @P32 ")
        SQLStrBldr.AppendLine("         , UPDTERMID = @P33 ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD = @P34 ")
        SQLStrBldr.AppendLine("     WHERE ")
        SQLStrBldr.AppendLine("         OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         TRAINNO = @P01 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         TSUMI = @P03 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         OTTRAINNO = @P04 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         DEPSTATION = @P06 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         ARRSTATION = @P07; ")
        SQLStrBldr.AppendLine("  IF (@@FETCH_STATUS <> 0) ")
        SQLStrBldr.AppendLine("     INSERT INTO OIL.OIM0007_TRAIN( ")
        SQLStrBldr.AppendLine("         OFFICECODE ")
        SQLStrBldr.AppendLine("         , TRAINNO ")
        SQLStrBldr.AppendLine("         , TRAINNAME ")
        SQLStrBldr.AppendLine("         , TSUMI ")
        SQLStrBldr.AppendLine("         , OTTRAINNO ")
        SQLStrBldr.AppendLine("         , OTFLG ")
        SQLStrBldr.AppendLine("         , DEPSTATION ")
        SQLStrBldr.AppendLine("         , ARRSTATION ")
        SQLStrBldr.AppendLine("         , JRTRAINNO1 ")
        SQLStrBldr.AppendLine("         , MAXTANK1 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO2 ")
        SQLStrBldr.AppendLine("         , MAXTANK2 ")
        SQLStrBldr.AppendLine("         , JRTRAINNO3 ")
        SQLStrBldr.AppendLine("         , MAXTANK3 ")
        SQLStrBldr.AppendLine("         , TRAINCLASS ")
        SQLStrBldr.AppendLine("         , SPEEDCLASS ")
        SQLStrBldr.AppendLine("         , SHIPORDERCLASS ")
        SQLStrBldr.AppendLine("         , DEPDAYS ")
        SQLStrBldr.AppendLine("         , MARGEDAYS ")
        SQLStrBldr.AppendLine("         , ARRDAYS ")
        SQLStrBldr.AppendLine("         , ACCDAYS ")
        SQLStrBldr.AppendLine("         , EMPARRDAYS ")
        SQLStrBldr.AppendLine("         , USEDAYS ")
        SQLStrBldr.AppendLine("         , FEEKBN ")
        SQLStrBldr.AppendLine("         , RUN ")
        SQLStrBldr.AppendLine("         , ZAIKOSORT ")
        SQLStrBldr.AppendLine("         , BIKOU ")
        SQLStrBldr.AppendLine("         , DELFLG ")
        SQLStrBldr.AppendLine("         , INITYMD ")
        SQLStrBldr.AppendLine("         , INITUSER ")
        SQLStrBldr.AppendLine("         , INITTERMID ")
        SQLStrBldr.AppendLine("         , UPDYMD ")
        SQLStrBldr.AppendLine("         , UPDUSER ")
        SQLStrBldr.AppendLine("         , UPDTERMID ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD) ")
        SQLStrBldr.AppendLine("     VALUES ")
        SQLStrBldr.AppendLine("         (@P00 ")
        SQLStrBldr.AppendLine("         , @P01 ")
        SQLStrBldr.AppendLine("         , @P02 ")
        SQLStrBldr.AppendLine("         , @P03 ")
        SQLStrBldr.AppendLine("         , @P04 ")
        SQLStrBldr.AppendLine("         , @P05 ")
        SQLStrBldr.AppendLine("         , @P06 ")
        SQLStrBldr.AppendLine("         , @P07 ")
        SQLStrBldr.AppendLine("         , @P08 ")
        SQLStrBldr.AppendLine("         , @P09 ")
        SQLStrBldr.AppendLine("         , @P10 ")
        SQLStrBldr.AppendLine("         , @P11 ")
        SQLStrBldr.AppendLine("         , @P12 ")
        SQLStrBldr.AppendLine("         , @P13 ")
        SQLStrBldr.AppendLine("         , @P14 ")
        SQLStrBldr.AppendLine("         , @P15 ")
        SQLStrBldr.AppendLine("         , @P16 ")
        SQLStrBldr.AppendLine("         , @P17 ")
        SQLStrBldr.AppendLine("         , @P18 ")
        SQLStrBldr.AppendLine("         , @P19 ")
        SQLStrBldr.AppendLine("         , @P20 ")
        SQLStrBldr.AppendLine("         , @P21 ")
        SQLStrBldr.AppendLine("         , @P22 ")
        SQLStrBldr.AppendLine("         , @P23 ")
        SQLStrBldr.AppendLine("         , @P24 ")
        SQLStrBldr.AppendLine("         , @P25 ")
        SQLStrBldr.AppendLine("         , @P26 ")
        SQLStrBldr.AppendLine("         , @P27 ")
        SQLStrBldr.AppendLine("         , @P28 ")
        SQLStrBldr.AppendLine("         , @P29 ")
        SQLStrBldr.AppendLine("         , @P30 ")
        SQLStrBldr.AppendLine("         , @P31 ")
        SQLStrBldr.AppendLine("         , @P32 ")
        SQLStrBldr.AppendLine("         , @P33 ")
        SQLStrBldr.AppendLine("         , @P34); ")
        SQLStrBldr.AppendLine("  CLOSE hensuu; ")
        SQLStrBldr.AppendLine("  DEALLOCATE hensuu; ")

        '○ 更新ジャーナル出力
        Dim SQLJnlBldr As New StringBuilder
        SQLJnlBldr.AppendLine(" SELECT ")
        SQLJnlBldr.AppendLine("    OFFICECODE ")
        SQLJnlBldr.AppendLine("    , TRAINNO ")
        SQLJnlBldr.AppendLine("    , TRAINNAME ")
        SQLJnlBldr.AppendLine("    , TSUMI ")
        SQLJnlBldr.AppendLine("    , OTTRAINNO ")
        SQLJnlBldr.AppendLine("    , OTFLG ")
        SQLJnlBldr.AppendLine("    , DEPSTATION ")
        SQLJnlBldr.AppendLine("    , ARRSTATION ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO1 ")
        SQLJnlBldr.AppendLine("    , MAXTANK1 ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO2 ")
        SQLJnlBldr.AppendLine("    , MAXTANK2 ")
        SQLJnlBldr.AppendLine("    , JRTRAINNO3 ")
        SQLJnlBldr.AppendLine("    , MAXTANK3 ")
        SQLJnlBldr.AppendLine("    , TRAINCLASS ")
        SQLJnlBldr.AppendLine("    , SPEEDCLASS ")
        SQLJnlBldr.AppendLine("    , SHIPORDERCLASS ")
        SQLJnlBldr.AppendLine("    , DEPDAYS ")
        SQLJnlBldr.AppendLine("    , MARGEDAYS ")
        SQLJnlBldr.AppendLine("    , ARRDAYS ")
        SQLJnlBldr.AppendLine("    , ACCDAYS ")
        SQLJnlBldr.AppendLine("    , EMPARRDAYS ")
        SQLJnlBldr.AppendLine("    , USEDAYS ")
        SQLJnlBldr.AppendLine("    , FEEKBN ")
        SQLJnlBldr.AppendLine("    , RUN ")
        SQLJnlBldr.AppendLine("    , ZAIKOSORT ")
        SQLJnlBldr.AppendLine("    , BIKOU ")
        SQLJnlBldr.AppendLine("    , DELFLG ")
        SQLJnlBldr.AppendLine("    , INITYMD ")
        SQLJnlBldr.AppendLine("    , INITUSER ")
        SQLJnlBldr.AppendLine("    , INITTERMID ")
        SQLJnlBldr.AppendLine("    , UPDYMD ")
        SQLJnlBldr.AppendLine("    , UPDUSER ")
        SQLJnlBldr.AppendLine("    , UPDTERMID ")
        SQLJnlBldr.AppendLine("    , RECEIVEYMD ")
        SQLJnlBldr.AppendLine("    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP ")
        SQLJnlBldr.AppendLine(" FROM ")
        SQLJnlBldr.AppendLine("    OIL.OIM0007_TRAIN ")
        SQLJnlBldr.AppendLine(" WHERE ")
        SQLJnlBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        TRAINNO = @P01 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        TSUMI = @P03 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        OTTRAINNO = @P04 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        DEPSTATION = @P06 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        ARRSTATION = @P07 ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon), SQLcmdJnl As New SqlCommand(SQLJnlBldr.ToString(), SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 6)           ' 管轄受注営業所
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 4)           ' 本線列車番号
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)          ' 本線列車番号名
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)           ' 積置フラグ
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)           ' OT列車番号
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)           ' OT発送日報送信フラグ
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 7)           ' 発駅コード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)           ' 着駅コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 4)           ' JR発列車番号
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Int)                   ' JR発列車牽引車数
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4)           ' JR中継列車番号
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)                   ' JR中継列車牽引車数
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 4)           ' JR最終列車番号
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)                   ' JR最終列車牽引車数
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 1)           ' 列車区分
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)           ' 高速列車区分
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 1)           ' 発送順区分
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.Int)                   ' 発日日数
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.Int)                   ' 特継日数
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.Int)                   ' 積車着日数
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.Int)                   ' 受入日数
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Int)                   ' 空車着日数
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)                   ' 当日利用日数
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)           ' 料金マスタ区分
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 1)           ' 稼働フラグ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Int)                   ' 在庫管理表表示ソート区分
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 200)         ' 備考
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 1)           ' 削除フラグ
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)              ' 登録年月日
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20)          ' 登録ユーザーＩＤ
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          ' 登録端末
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)              ' 更新年月日
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)          ' 更新ユーザーＩＤ
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)          ' 更新端末
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.DateTime)              ' 集信日時


                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 6)       ' 管轄受注営業所
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4)       ' 本線列車番号
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 1)       ' 積置フラグ
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 4)       ' OT列車番号
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.NVarChar, 7)       ' 発駅コード
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.NVarChar, 7)       ' 着駅コード
                For Each OIM0007row As DataRow In OIM0007tbl.Rows
                    If Trim(OIM0007row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0007row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0007row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0007row("OFFICECODE")
                        PARA01.Value = OIM0007row("TRAINNO")
                        PARA02.Value = OIM0007row("TRAINNAME")
                        PARA03.Value = OIM0007row("TSUMI")
                        PARA04.Value = OIM0007row("OTTRAINNO")
                        PARA05.Value = OIM0007row("OTFLG")
                        PARA06.Value = OIM0007row("DEPSTATION")
                        PARA07.Value = OIM0007row("ARRSTATION")
                        PARA08.Value = OIM0007row("JRTRAINNO1")
                        PARA09.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK1")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK1"))
                        PARA10.Value = OIM0007row("JRTRAINNO2")
                        PARA11.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK2")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK2"))
                        PARA12.Value = OIM0007row("JRTRAINNO3")
                        PARA13.Value = If(String.IsNullOrEmpty(OIM0007row("MAXTANK3")), SqlTypes.SqlInt32.Null, OIM0007row("MAXTANK3"))
                        PARA14.Value = OIM0007row("TRAINCLASS")
                        PARA15.Value = OIM0007row("SPEEDCLASS")
                        PARA16.Value = OIM0007row("SHIPORDERCLASS")
                        PARA17.Value = If(String.IsNullOrEmpty(OIM0007row("DEPDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("DEPDAYS"))
                        PARA18.Value = If(String.IsNullOrEmpty(OIM0007row("MARGEDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("MARGEDAYS"))
                        PARA19.Value = If(String.IsNullOrEmpty(OIM0007row("ARRDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("ARRDAYS"))
                        PARA20.Value = If(String.IsNullOrEmpty(OIM0007row("ACCDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("ACCDAYS"))
                        PARA21.Value = If(String.IsNullOrEmpty(OIM0007row("EMPARRDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("EMPARRDAYS"))
                        PARA22.Value = If(String.IsNullOrEmpty(OIM0007row("USEDAYS")), SqlTypes.SqlInt32.Null, OIM0007row("USEDAYS"))
                        PARA23.Value = OIM0007row("FEEKBN")
                        PARA24.Value = OIM0007row("RUN")
                        PARA25.Value = If(String.IsNullOrEmpty(OIM0007row("ZAIKOSORT")), SqlTypes.SqlInt32.Null, OIM0007row("ZAIKOSORT"))
                        PARA26.Value = OIM0007row("BIKOU")
                        PARA27.Value = OIM0007row("DELFLG")
                        PARA28.Value = WW_DATENOW
                        PARA29.Value = Master.USERID
                        PARA30.Value = Master.USERTERMID
                        PARA31.Value = WW_DATENOW
                        PARA32.Value = Master.USERID
                        PARA33.Value = Master.USERTERMID
                        PARA34.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        ' 更新ジャーナル出力
                        JPARA00.Value = OIM0007row("OFFICECODE")
                        JPARA01.Value = OIM0007row("TRAINNO")
                        JPARA03.Value = OIM0007row("TSUMI")
                        JPARA04.Value = OIM0007row("OTTRAINNO")
                        JPARA06.Value = OIM0007row("DEPSTATION")
                        JPARA07.Value = OIM0007row("ARRSTATION")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0007UPDtbl) Then
                                OIM0007UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0007UPDtbl.Clear()
                            OIM0007UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0007UPDrow As DataRow In OIM0007UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0007L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0007UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     ' SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         ' ログ出力
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0007L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0007L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
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
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       ' 会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                ' プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       ' 画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         ' 帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           ' 出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0007tbl                        ' データ参照  Table
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
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       ' 会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                ' プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       ' 画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         ' 帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            ' 出力ファイル形式
        CS0030REPORT.TBLDATA = OIM0007tbl                        ' データ参照Table
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
        Dim TBLview As New DataView(OIM0007tbl)
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

        ' 選択行
        work.WF_SEL_LINECNT.Text = OIM0007tbl.Rows(WW_LINECNT)("LINECNT")

        ' 管轄受注営業所
        work.WF_SEL_OFFICECODE2.Text = OIM0007tbl.Rows(WW_LINECNT)("OFFICECODE")

        ' 本線列車番号
        work.WF_SEL_TRAINNO2.Text = OIM0007tbl.Rows(WW_LINECNT)("TRAINNO")

        ' 本線列車番号名
        work.WF_SEL_TRAINNAME.Text = OIM0007tbl.Rows(WW_LINECNT)("TRAINNAME")

        ' 積置フラグ
        work.WF_SEL_TSUMI2.Text = OIM0007tbl.Rows(WW_LINECNT)("TSUMI")

        ' OT列車番号
        work.WF_SEL_OTTRAINNO.Text = OIM0007tbl.Rows(WW_LINECNT)("OTTRAINNO")

        ' OT発送日報送信フラグ
        work.WF_SEL_OTFLG.Text = OIM0007tbl.Rows(WW_LINECNT)("OTFLG")

        ' 発駅コード
        work.WF_SEL_DEPSTATION.Text = OIM0007tbl.Rows(WW_LINECNT)("DEPSTATION")

        ' 着駅コード
        work.WF_SEL_ARRSTATION.Text = OIM0007tbl.Rows(WW_LINECNT)("ARRSTATION")

        ' JR発列車番号
        work.WF_SEL_JRTRAINNO1.Text = OIM0007tbl.Rows(WW_LINECNT)("JRTRAINNO1")

        ' JR発列車牽引車数
        work.WF_SEL_MAXTANK1.Text = OIM0007tbl.Rows(WW_LINECNT)("MAXTANK1")

        ' JR中継列車番号
        work.WF_SEL_JRTRAINNO2.Text = OIM0007tbl.Rows(WW_LINECNT)("JRTRAINNO2")

        ' JR中継列車牽引車数
        work.WF_SEL_MAXTANK2.Text = OIM0007tbl.Rows(WW_LINECNT)("MAXTANK2")

        ' JR最終列車番号
        work.WF_SEL_JRTRAINNO3.Text = OIM0007tbl.Rows(WW_LINECNT)("JRTRAINNO3")

        ' JR最終列車牽引車数
        work.WF_SEL_MAXTANK3.Text = OIM0007tbl.Rows(WW_LINECNT)("MAXTANK3")

        ' 列車区分
        work.WF_SEL_TRAINCLASS.Text = OIM0007tbl.Rows(WW_LINECNT)("TRAINCLASS")

        ' 高速列車区分
        work.WF_SEL_SPEEDCLASS.Text = OIM0007tbl.Rows(WW_LINECNT)("SPEEDCLASS")

        ' 発送順区分
        work.WF_SEL_SHIPORDERCLASS.Text = OIM0007tbl.Rows(WW_LINECNT)("SHIPORDERCLASS")

        ' 発日日数
        work.WF_SEL_DEPDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("DEPDAYS")

        ' 特継日数
        work.WF_SEL_MARGEDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("MARGEDAYS")

        ' 積車着日数
        work.WF_SEL_ARRDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("ARRDAYS")

        ' 受入日数
        work.WF_SEL_ACCDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("ACCDAYS")

        ' 空車着日数
        work.WF_SEL_EMPARRDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("EMPARRDAYS")

        ' 当日利用日数
        work.WF_SEL_USEDAYS.Text = OIM0007tbl.Rows(WW_LINECNT)("USEDAYS")

        ' 料金マスタ区分
        work.WF_SEL_FEEKBN.Text = OIM0007tbl.Rows(WW_LINECNT)("FEEKBN")

        ' 稼働フラグ
        work.WF_SEL_RUN.Text = OIM0007tbl.Rows(WW_LINECNT)("RUN")

        ' 在庫管理表表示ソート区分
        work.WF_SEL_ZAIKOSORT.Text = OIM0007tbl.Rows(WW_LINECNT)("ZAIKOSORT")

        ' 備考
        work.WF_SEL_BIKOU.Text = OIM0007tbl.Rows(WW_LINECNT)("BIKOU")

        ' 削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0007tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            Select Case OIM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0007tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0007tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

        WF_GridDBclick.Text = ""

        ' 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        ' 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0007tbl, work.WF_SEL_INPTBL.Text)

        ' 登録画面ページへ遷移
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
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        ' 会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        ' 画面ID
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
        Master.CreateEmptyTable(OIM0007INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0007INProw As DataRow = OIM0007INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0007INPcol As DataColumn In OIM0007INPtbl.Columns
                If IsDBNull(OIM0007INProw.Item(OIM0007INPcol)) OrElse IsNothing(OIM0007INProw.Item(OIM0007INPcol)) Then
                    Select Case OIM0007INPcol.ColumnName
                        Case "LINECNT"
                            OIM0007INProw.Item(OIM0007INPcol) = 0
                        Case "OPERATION"
                            OIM0007INProw.Item(OIM0007INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0007INProw.Item(OIM0007INPcol) = 0
                        Case "SELECT"
                            OIM0007INProw.Item(OIM0007INPcol) = 1
                        Case "HIDDEN"
                            OIM0007INProw.Item(OIM0007INPcol) = 0
                        Case Else
                            OIM0007INProw.Item(OIM0007INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TRAINNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TSUMI") >= 0 AndAlso
                WW_COLUMNS.IndexOf("OTTRAINNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DEPSTATION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ARRSTATION") >= 0 Then
                For Each OIM0007row As DataRow In OIM0007tbl.Rows
                    ' キー項目が一致する場合、変更元情報を入力レコードにコピーする
                    If XLSTBLrow("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                        XLSTBLrow("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                        XLSTBLrow("TSUMI") = OIM0007row("TSUMI") AndAlso
                        XLSTBLrow("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                        XLSTBLrow("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                        XLSTBLrow("ARRSTATION") = OIM0007row("ARRSTATION") Then
                        OIM0007INProw.ItemArray = OIM0007row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            ' 管轄受注営業所
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
                OIM0007INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
            End If

            ' 本線列車番号
            If WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
                OIM0007INProw("TRAINNO") = XLSTBLrow("TRAINNO")
            End If

            ' 本線列車番号名
            If WW_COLUMNS.IndexOf("TRAINNAME") >= 0 Then
                OIM0007INProw("TRAINNAME") = XLSTBLrow("TRAINNAME")
            End If

            ' 積置フラグ
            If WW_COLUMNS.IndexOf("TSUMI") >= 0 Then
                OIM0007INProw("TSUMI") = XLSTBLrow("TSUMI")
            End If

            ' OT列車番号
            If WW_COLUMNS.IndexOf("OTTRAINNO") >= 0 Then
                OIM0007INProw("OTTRAINNO") = XLSTBLrow("OTTRAINNO")
            End If

            ' OT発送日報送信フラグ
            If WW_COLUMNS.IndexOf("OTFLG") >= 0 Then
                OIM0007INProw("OTFLG") = XLSTBLrow("OTFLG")
            End If

            ' 発駅コード
            If WW_COLUMNS.IndexOf("DEPSTATION") >= 0 Then
                OIM0007INProw("DEPSTATION") = XLSTBLrow("DEPSTATION")
            End If

            ' 着駅コード
            If WW_COLUMNS.IndexOf("ARRSTATION") >= 0 Then
                OIM0007INProw("ARRSTATION") = XLSTBLrow("ARRSTATION")
            End If

            ' JR発列車番号
            If WW_COLUMNS.IndexOf("JRTRAINNO1") >= 0 Then
                OIM0007INProw("JRTRAINNO1") = XLSTBLrow("JRTRAINNO1")
            End If

            ' JR発列車牽引車数
            If WW_COLUMNS.IndexOf("MAXTANK1") >= 0 Then
                OIM0007INProw("MAXTANK1") = XLSTBLrow("MAXTANK1")
            End If

            ' JR中継列車番号
            If WW_COLUMNS.IndexOf("JRTRAINNO2") >= 0 Then
                OIM0007INProw("JRTRAINNO2") = XLSTBLrow("JRTRAINNO2")
            End If

            ' JR中継列車牽引車数
            If WW_COLUMNS.IndexOf("MAXTANK2") >= 0 Then
                OIM0007INProw("MAXTANK2") = XLSTBLrow("MAXTANK2")
            End If

            ' JR最終列車番号
            If WW_COLUMNS.IndexOf("JRTRAINNO3") >= 0 Then
                OIM0007INProw("JRTRAINNO3") = XLSTBLrow("JRTRAINNO3")
            End If

            ' JR最終列車牽引車数
            If WW_COLUMNS.IndexOf("MAXTANK3") >= 0 Then
                OIM0007INProw("MAXTANK3") = XLSTBLrow("MAXTANK3")
            End If

            ' 列車区分
            If WW_COLUMNS.IndexOf("TRAINCLASS") >= 0 Then
                OIM0007INProw("TRAINCLASS") = XLSTBLrow("TRAINCLASS")
            End If

            ' 高速列車区分
            If WW_COLUMNS.IndexOf("SPEEDCLASS") >= 0 Then
                OIM0007INProw("SPEEDCLASS") = XLSTBLrow("SPEEDCLASS")
            End If

            ' 発送順区分
            If WW_COLUMNS.IndexOf("SHIPORDERCLASS") >= 0 Then
                OIM0007INProw("SHIPORDERCLASS") = XLSTBLrow("SHIPORDERCLASS")
            End If

            ' 発日日数
            If WW_COLUMNS.IndexOf("DEPDAYS") >= 0 Then
                OIM0007INProw("DEPDAYS") = XLSTBLrow("DEPDAYS")
            End If

            ' 特継日数
            If WW_COLUMNS.IndexOf("MARGEDAYS") >= 0 Then
                OIM0007INProw("MARGEDAYS") = XLSTBLrow("MARGEDAYS")
            End If

            ' 積車着日数
            If WW_COLUMNS.IndexOf("ARRDAYS") >= 0 Then
                OIM0007INProw("ARRDAYS") = XLSTBLrow("ARRDAYS")
            End If

            ' 受入日数
            If WW_COLUMNS.IndexOf("ACCDAYS") >= 0 Then
                OIM0007INProw("ACCDAYS") = XLSTBLrow("ACCDAYS")
            End If

            ' 空車着日数
            If WW_COLUMNS.IndexOf("EMPARRDAYS") >= 0 Then
                OIM0007INProw("EMPARRDAYS") = XLSTBLrow("EMPARRDAYS")
            End If

            ' 当日利用日数
            If WW_COLUMNS.IndexOf("USEDAYS") >= 0 Then
                OIM0007INProw("USEDAYS") = XLSTBLrow("USEDAYS")
            End If

            ' 料金マスタ区分
            If WW_COLUMNS.IndexOf("FEEKBN") >= 0 Then
                OIM0007INProw("FEEKBN") = XLSTBLrow("FEEKBN")
            End If

            ' 稼働フラグ
            If WW_COLUMNS.IndexOf("RUN") >= 0 Then
                OIM0007INProw("RUN") = XLSTBLrow("RUN")
            End If

            ' 在庫管理表表示ソート区分
            If WW_COLUMNS.IndexOf("ZAIKOSORT") >= 0 Then
                OIM0007INProw("ZAIKOSORT") = XLSTBLrow("ZAIKOSORT")
            End If

            ' 備考
            If WW_COLUMNS.IndexOf("BIKOU") >= 0 Then
                OIM0007INProw("BIKOU") = XLSTBLrow("BIKOU")
            End If

            ' 削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0007INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            OIM0007INPtbl.Rows.Add(OIM0007INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0007tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

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
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            Select Case OIM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0007tbl)

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
        ' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        ' 　※権限判定時点：現在
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
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows

            WW_LINE_ERR = ""

            ' 管轄受注営業所（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OFFICECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("OFFICECODE", OIM0007INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 本線列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINNO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(本線列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 本線列車番号名（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(本線列車番号名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 積置フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TSUMI")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TSUMI", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("TSUMI", OIM0007INProw("TSUMI"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(積置フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(積置フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OTTRAINNO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTTRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT発送日報送信フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("OTFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("OTFLG", OIM0007INProw("OTFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(OT発送日報送信フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(OT発送日報送信フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DEPSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0007INProw("DEPSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着駅コード（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ARRSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0007INProw("ARRSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR発列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR発列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR発列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR発列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR中継列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR中継列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR中継列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR中継列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR最終列車番号（バリデーションチェック）
            WW_TEXT = OIM0007INProw("JRTRAINNO3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTRAINNO3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR最終列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR最終列車牽引車数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MAXTANK3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXTANK3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR最終列車牽引車数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 列車区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("TRAINCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("TRAINCLASS", OIM0007INProw("TRAINCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(列車区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(列車区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 高速列車区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("SPEEDCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SPEEDCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("SPEEDCLASS", OIM0007INProw("SPEEDCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(高速列車区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(高速列車区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発送順区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("SHIPORDERCLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHIPORDERCLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("SHIPORDERCLASS", OIM0007INProw("SHIPORDERCLASS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(発送順区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発送順区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発日日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DEPDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(発日日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特継日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("MARGEDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARGEDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(特継日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 積車着日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ARRDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積車着日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 受入日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ACCDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(受入日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 空車着日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("EMPARRDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EMPARRDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(空車着日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 当日利用日数（バリデーションチェック）
            WW_TEXT = OIM0007INProw("USEDAYS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USEDAYS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("USEDAYS", OIM0007INProw("USEDAYS"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(当日利用日数エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(当日利用日数エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 料金マスタ区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("FEEKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FEEKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(料金マスタ区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 稼働フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("RUN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RUN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("RUN", OIM0007INProw("RUN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(稼働フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(稼働フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 在庫管理表表示ソート区分（バリデーションチェック）
            WW_TEXT = OIM0007INProw("ZAIKOSORT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ZAIKOSORT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(在庫管理表表示ソート区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 備考（バリデーションチェック）
            WW_TEXT = OIM0007INProw("BIKOU")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIKOU", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(備考エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 削除フラグ（バリデーションチェック）
            WW_TEXT = OIM0007INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("DELFLG", OIM0007INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0007INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            If WW_LINE_ERR = "" Then
                If OIM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    OIM0007INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    OIM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0007row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0007row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0007row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 管轄受注営業所 =" & OIM0007row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車番号 =" & OIM0007row("TRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車番号名 =" & OIM0007row("TRAINNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積置フラグ =" & OIM0007row("TSUMI") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT列車番号 =" & OIM0007row("OTTRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT発送日報送信フラグ =" & OIM0007row("OTFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅コード =" & OIM0007row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅コード =" & OIM0007row("ARRSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR発列車番号 =" & OIM0007row("JRTRAINNO1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR発列車牽引車数 =" & OIM0007row("MAXTANK1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR中継列車番号 =" & OIM0007row("JRTRAINNO2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR中継列車牽引車数 =" & OIM0007row("MAXTANK2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR最終列車番号 =" & OIM0007row("JRTRAINNO3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR最終列車牽引車数 =" & OIM0007row("MAXTANK3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 列車区分 =" & OIM0007row("TRAINCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 高速列車区分 =" & OIM0007row("SPEEDCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発送順区分 =" & OIM0007row("SHIPORDERCLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発日日数 =" & OIM0007row("DEPDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 特継日数 =" & OIM0007row("MARGEDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積車着日数 =" & OIM0007row("ARRDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受入日数 =" & OIM0007row("ACCDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着日数 =" & OIM0007row("EMPARRDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 当日利用日数 =" & OIM0007row("USEDAYS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 料金マスタ区分 =" & OIM0007row("FEEKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 稼働フラグ =" & OIM0007row("RUN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 在庫管理表表示ソート区分 =" & OIM0007row("ZAIKOSORT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 備考 =" & OIM0007row("BIKOU") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0007row("DELFLG")
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
    ''' OIM0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0007tbl_UPD()

        '○ 画面状態設定
        For Each OIM0007row As DataRow In OIM0007tbl.Rows
            Select Case OIM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows

            ' エラーレコード読み飛ばし
            If OIM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0007INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0007row As DataRow In OIM0007tbl.Rows
                ' KEY項目が等しい時
                If OIM0007row("OFFICECODE") = OIM0007INProw("OFFICECODE") AndAlso
                    OIM0007row("TRAINNO") = OIM0007INProw("TRAINNO") AndAlso
                    OIM0007row("TSUMI") = OIM0007INProw("TSUMI") AndAlso
                    OIM0007row("OTTRAINNO") = OIM0007INProw("OTTRAINNO") AndAlso
                    OIM0007row("DEPSTATION") = OIM0007INProw("DEPSTATION") AndAlso
                    OIM0007row("ARRSTATION") = OIM0007INProw("ARRSTATION") Then
                    '  KEY項目以外の項目に差異があるかチェック
                    If OIM0007row("TRAINNAME") = OIM0007INProw("TRAINNAME") AndAlso
                        OIM0007row("OTFLG") = OIM0007INProw("OTFLG") AndAlso
                        OIM0007row("JRTRAINNO1") = OIM0007INProw("JRTRAINNO1") AndAlso
                        OIM0007row("MAXTANK1") = OIM0007INProw("MAXTANK1") AndAlso
                        OIM0007row("JRTRAINNO2") = OIM0007INProw("JRTRAINNO2") AndAlso
                        OIM0007row("MAXTANK2") = OIM0007INProw("MAXTANK2") AndAlso
                        OIM0007row("JRTRAINNO3") = OIM0007INProw("JRTRAINNO3") AndAlso
                        OIM0007row("MAXTANK3") = OIM0007INProw("MAXTANK3") AndAlso
                        OIM0007row("TRAINCLASS") = OIM0007INProw("TRAINCLASS") AndAlso
                        OIM0007row("SPEEDCLASS") = OIM0007INProw("SPEEDCLASS") AndAlso
                        OIM0007row("SHIPORDERCLASS") = OIM0007INProw("SHIPORDERCLASS") AndAlso
                        OIM0007row("DEPDAYS") = OIM0007INProw("DEPDAYS") AndAlso
                        OIM0007row("MARGEDAYS") = OIM0007INProw("MARGEDAYS") AndAlso
                        OIM0007row("ARRDAYS") = OIM0007INProw("ARRDAYS") AndAlso
                        OIM0007row("ACCDAYS") = OIM0007INProw("ACCDAYS") AndAlso
                        OIM0007row("EMPARRDAYS") = OIM0007INProw("EMPARRDAYS") AndAlso
                        OIM0007row("USEDAYS") = OIM0007INProw("USEDAYS") AndAlso
                        OIM0007row("FEEKBN") = OIM0007INProw("FEEKBN") AndAlso
                        OIM0007row("RUN") = OIM0007INProw("RUN") AndAlso
                        OIM0007row("ZAIKOSORT") = OIM0007INProw("ZAIKOSORT") AndAlso
                        OIM0007row("BIKOU") = OIM0007INProw("BIKOU") AndAlso
                        OIM0007row("DELFLG") = OIM0007INProw("DELFLG") Then
                        '変更がないときは「操作」の項目は空白にする
                        OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        '変更がある時は「操作」の項目を「更新」に設定する
                        OIM0007INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0007INProw As DataRow In OIM0007INPtbl.Rows
            Select Case OIM0007INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0007INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0007INProw)
                Case CONST_PATTERNERR
                    ' 関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0007INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0007INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0007INProw As DataRow)

        For Each OIM0007row As DataRow In OIM0007tbl.Rows

            ' 同一レコードか判定
            If OIM0007INProw("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                OIM0007INProw("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                OIM0007INProw("TSUMI") = OIM0007row("TSUMI") AndAlso
                OIM0007INProw("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                OIM0007INProw("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                OIM0007INProw("ARRSTATION") = OIM0007row("ARRSTATION") Then
                ' 画面入力テーブル項目設定
                OIM0007INProw("LINECNT") = OIM0007row("LINECNT")
                OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0007INProw("UPDTIMSTP") = OIM0007row("UPDTIMSTP")
                OIM0007INProw("SELECT") = 1
                OIM0007INProw("HIDDEN") = 0

                ' 項目テーブル項目設定
                OIM0007row.ItemArray = OIM0007INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0007INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0007row As DataRow = OIM0007tbl.NewRow
        OIM0007row.ItemArray = OIM0007INProw.ItemArray

        OIM0007row("LINECNT") = OIM0007tbl.Rows.Count + 1
        If OIM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0007row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0007row("UPDTIMSTP") = "0"
        OIM0007row("SELECT") = 1
        OIM0007row("HIDDEN") = 0

        OIM0007tbl.Rows.Add(OIM0007row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0007INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0007INProw As DataRow)

        For Each OIM0007row As DataRow In OIM0007tbl.Rows

            ' 同一レコードか判定
            If OIM0007INProw("OFFICECODE") = OIM0007row("OFFICECODE") AndAlso
                OIM0007INProw("TRAINNO") = OIM0007row("TRAINNO") AndAlso
                OIM0007INProw("TSUMI") = OIM0007row("TSUMI") AndAlso
                OIM0007INProw("OTTRAINNO") = OIM0007row("OTTRAINNO") AndAlso
                OIM0007INProw("DEPSTATION") = OIM0007row("DEPSTATION") AndAlso
                OIM0007INProw("ARRSTATION") = OIM0007row("ARRSTATION") Then
                ' 画面入力テーブル項目設定
                OIM0007INProw("LINECNT") = OIM0007row("LINECNT")
                OIM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0007INProw("UPDTIMSTP") = OIM0007row("UPDTIMSTP")
                OIM0007INProw("SELECT") = 1
                OIM0007INProw("HIDDEN") = 0

                ' 項目テーブル項目設定
                OIM0007row.ItemArray = OIM0007INProw.ItemArray
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
                    ' 管轄受注営業所
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TRAINNO"
                    ' 本線列車番号
                    If String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
                        '管轄受注営業所コード未設定の場合、所属組織コードで検索する
                        prmData = work.CreateTrainNoParam(Master.USER_ORG, I_VALUE)
                    Else
                        prmData = work.CreateTrainNoParam(work.WF_SEL_OFFICECODE.Text, I_VALUE)
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TSUMI"
                    ' 積置フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TSUMI")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATION"
                    ' 駅
                    prmData = work.CreateFIXParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTFLG"
                    ' OT発送日報送信フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OTFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TRAINCLASS"
                    ' 列車区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TRAINCLASS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINCLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SPEEDCLASS"
                    ' 高速列車区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SPEEDCLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPORDERCLASS"
                    ' 発送順区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPORDERCLASS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USEDAYS"
                    ' 当日利用日数
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "USEDAYS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RUN"
                    ' 稼働フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "RUN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    ' 削除
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

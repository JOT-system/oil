﻿Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 列車マスタ（臨海）登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0016RTrainList
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0016tbl As DataTable                                 ' 一覧格納用テーブル
    Private OIM0016INPtbl As DataTable                              ' チェック用テーブル
    Private OIM0016UPDtbl As DataTable                              ' 更新用テーブル

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
                    Master.RecoverTable(OIM0016tbl)

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
            If Not IsNothing(OIM0016tbl) Then
                OIM0016tbl.Clear()
                OIM0016tbl.Dispose()
                OIM0016tbl = Nothing
            End If

            If Not IsNothing(OIM0016INPtbl) Then
                OIM0016INPtbl.Clear()
                OIM0016INPtbl.Dispose()
                OIM0016INPtbl = Nothing
            End If

            If Not IsNothing(OIM0016UPDtbl) Then
                OIM0016UPDtbl.Clear()
                OIM0016UPDtbl.Dispose()
                OIM0016UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0016WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0016S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0016C Then
            Master.RecoverTable(OIM0016tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        ' 登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIM0016C Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

        '○ 一覧の件数を取得
        Me.WF_ListCNT.Text = "件数：" + OIM0016tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIM0016tbl)

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

        If IsNothing(OIM0016tbl) Then
            OIM0016tbl = New DataTable
        End If

        If OIM0016tbl.Columns.Count <> 0 Then
            OIM0016tbl.Columns.Clear()
        End If

        OIM0016tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを列車マスタ（臨海）から取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT ")
        SQLStrBldr.AppendLine("       0                                               AS LINECNT ")          ' 行番号
        SQLStrBldr.AppendLine("     , ''                                              AS OPERATION ")        ' 編集
        SQLStrBldr.AppendLine("     , CAST(OIM0016.UPDTIMSTP AS bigint)               AS UPDTIMSTP ")        ' タイムスタンプ
        SQLStrBldr.AppendLine("     , 1                                               AS 'SELECT' ")         ' 選択
        SQLStrBldr.AppendLine("     , 0                                               AS HIDDEN ")           ' 非表示
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.OFFICECODE), '')           AS OFFICECODE ")       ' 管轄受注営業所
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.IOKBN), '')                AS IOKBN ")            ' 入線出線区分
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.TRAINNO), '')              AS TRAINNO" )          ' 入線出線列車番号
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.TRAINNAME), '')            AS TRAINNAME" )        ' 入線出線列車名
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.DEPSTATION), '')           AS DEPSTATION" )       ' 発駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.ARRSTATION), '')           AS ARRSTATION" )       ' 着駅コード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.PLANTCODE), '')            AS PLANTCODE" )        ' プラントコード
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.LINECNT), '')              AS LINE" )             ' 回線
        SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIM0016.DELFLG), '')               AS DELFLG ")           ' 削除フラグ
        SQLStrBldr.AppendLine(" FROM ")
        SQLStrBldr.AppendLine("     [oil].OIM0016_RTRAIN OIM0016 ")

        '○ 条件指定
        Dim andFlg As Boolean = False

        ' 管轄受注営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
            SQLStrBldr.AppendLine(" WHERE ")
            SQLStrBldr.AppendLine("     OIM0016.OFFICECODE = @P1 ")
            andFlg = True
        End If

        ' 入線出線区分
        If Not String.IsNullOrEmpty(work.WF_SEL_IOKBN.Text) Then
            If andFlg Then
                SQLStrBldr.AppendLine("     AND ")
            Else
                SQLStrBldr.AppendLine(" WHERE ")
            End If
            SQLStrBldr.AppendLine("     OIM0016.IOKBN = @P2 ")
            andFlg = True
        End If

        ' 回線
        If Not String.IsNullOrEmpty(work.WF_SEL_LINE.Text) Then
            If andFlg Then
                SQLStrBldr.AppendLine("     AND ")
            Else
                SQLStrBldr.AppendLine(" WHERE ")
            End If
            SQLStrBldr.AppendLine("     OIM0016.LINECNT = @P3 ")
            andFlg = True
        End If

        '○ ソート
        SQLStrBldr.AppendLine(" ORDER BY ")
        SQLStrBldr.AppendLine("     OIM0016.OFFICECODE ")
        SQLStrBldr.AppendLine("     , OIM0016.IOKBN ")
        SQLStrBldr.AppendLine("     , OIM0016.LINECNT ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)     ' 管轄受注営業所
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)     ' 入線出線区分
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Int, 4)          ' 回線

                PARA1.Value = work.WF_SEL_OFFICECODE.Text
                PARA2.Value = work.WF_SEL_IOKBN.Text
                If String.IsNullOrEmpty(work.WF_SEL_LINE.Text) Then
                    PARA3.Value = 0
                Else
                    PARA3.Value = Int32.Parse(work.WF_SEL_LINE.Text)
                End If


                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0016tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0016tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIM0016row As DataRow In OIM0016tbl.Rows
                    i += 1
                    OIM0016row("LINECNT") = i        ' LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0016L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0016L Select"
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
        For Each OIM0016row As DataRow In OIM0016tbl.Rows
            If OIM0016row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIM0016row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIM0016tbl)

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

        ' 入線出線区分
        work.WF_SEL_IOKBN2.Text = ""

        ' 入線出線列車番号
        work.WF_SEL_TRAINNO.Text = ""

        ' 入線出線列車名
        work.WF_SEL_TRAINNAME.Text = ""

        ' 発駅コード
        work.WF_SEL_DEPSTATION.Text = ""

        ' 着駅コード
        work.WF_SEL_ARRSTATION.Text = ""

        ' プラントコード
        work.WF_SEL_PLANTCODE.Text = ""

        ' 回線
        work.WF_SEL_LINE2.Text = ""

        ' 削除フラグ
        work.WF_SEL_DELFLG.Text = ""

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIM0016tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

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
    ''' 列車マスタ（臨海）登録更新
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
        SQLStrBldr.AppendLine("        OIL.OIM0016_RTRAIN ")
        SQLStrBldr.AppendLine("    WHERE ")
        SQLStrBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        IOKBN = @P01 ")
        SQLStrBldr.AppendLine("        AND ")
        SQLStrBldr.AppendLine("        LINECNT = @P07 ")
        SQLStrBldr.AppendLine(" OPEN hensuu; ")
        SQLStrBldr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu; ")
        SQLStrBldr.AppendLine(" IF (@@FETCH_STATUS = 0) ")
        SQLStrBldr.AppendLine("     UPDATE OIL.OIM0016_RTRAIN ")
        SQLStrBldr.AppendLine("     SET ")
        SQLStrBldr.AppendLine("         TRAINNO = @P02 ")
        SQLStrBldr.AppendLine("         , TRAINNAME = @P03 ")
        SQLStrBldr.AppendLine("         , DEPSTATION = @P04 ")
        SQLStrBldr.AppendLine("         , ARRSTATION = @P05 ")
        SQLStrBldr.AppendLine("         , PLANTCODE = @P06 ")
        SQLStrBldr.AppendLine("         , DELFLG = @P08 ")
        'SQLStrBldr.AppendLine("         , INITYMD = @P09 ")
        'SQLStrBldr.AppendLine("         , INITUSER = @P10 ")
        'SQLStrBldr.AppendLine("         , INITTERMID = @P11 ")
        SQLStrBldr.AppendLine("         , UPDYMD = @P12 ")
        SQLStrBldr.AppendLine("         , UPDUSER = @P13 ")
        SQLStrBldr.AppendLine("         , UPDTERMID = @P14 ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD = @P15 ")
        'SQLStrBldr.AppendLine("         , UPDTIMSTP = @P16 ")
        SQLStrBldr.AppendLine("     WHERE ")
        SQLStrBldr.AppendLine("         OFFICECODE = @P00 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         IOKBN = @P01 ")
        SQLStrBldr.AppendLine("         AND ")
        SQLStrBldr.AppendLine("         LINECNT = @P07; ")
        SQLStrBldr.AppendLine("  IF (@@FETCH_STATUS <> 0) ")
        SQLStrBldr.AppendLine("     INSERT INTO OIL.OIM0016_RTRAIN ")
        SQLStrBldr.AppendLine("         (OFFICECODE ")
        SQLStrBldr.AppendLine("         , IOKBN ")
        SQLStrBldr.AppendLine("         , TRAINNO ")
        SQLStrBldr.AppendLine("         , TRAINNAME ")
        SQLStrBldr.AppendLine("         , DEPSTATION ")
        SQLStrBldr.AppendLine("         , ARRSTATION ")
        SQLStrBldr.AppendLine("         , PLANTCODE ")
        SQLStrBldr.AppendLine("         , LINECNT ")
        SQLStrBldr.AppendLine("         , DELFLG ")
        SQLStrBldr.AppendLine("         , INITYMD ")
        SQLStrBldr.AppendLine("         , INITUSER ")
        SQLStrBldr.AppendLine("         , INITTERMID ")
        SQLStrBldr.AppendLine("         , UPDYMD ")
        SQLStrBldr.AppendLine("         , UPDUSER ")
        SQLStrBldr.AppendLine("         , UPDTERMID ")
        SQLStrBldr.AppendLine("         , RECEIVEYMD) ")
        'SQLStrBldr.AppendLine("         , UPDTIMSTP) ")
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
        SQLStrBldr.AppendLine("         , @P15); ")
        'SQLStrBldr.AppendLine("         , @P16); ")
        SQLStrBldr.AppendLine("  CLOSE hensuu; ")
        SQLStrBldr.AppendLine("  DEALLOCATE hensuu; ")

        '○ 更新ジャーナル出力
        Dim SQLJnlBldr As New StringBuilder
        SQLJnlBldr.AppendLine(" SELECT ")
        SQLJnlBldr.AppendLine("    OFFICECODE ")
        SQLJnlBldr.AppendLine("    , IOKBN ")
        SQLJnlBldr.AppendLine("    , TRAINNO ")
        SQLJnlBldr.AppendLine("    , TRAINNAME ")
        SQLJnlBldr.AppendLine("    , DEPSTATION ")
        SQLJnlBldr.AppendLine("    , ARRSTATION ")
        SQLJnlBldr.AppendLine("    , PLANTCODE ")
        SQLJnlBldr.AppendLine("    , LINECNT ")
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
        SQLJnlBldr.AppendLine("    OIL.OIM0016_RTRAIN ")
        SQLJnlBldr.AppendLine(" WHERE ")
        SQLJnlBldr.AppendLine("        OFFICECODE = @P00 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        IOKBN = @P01 ")
        SQLJnlBldr.AppendLine("        AND ")
        SQLJnlBldr.AppendLine("        LINECNT = @P07 ")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon), SQLcmdJnl As New SqlCommand(SQLJnlBldr.ToString(), SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 12)          ' 管轄受注営業所
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 1)           ' 入線出線区分
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 8)           ' 入線出線列車番号
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 40)          ' 入線出線列車名
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 14)          ' 発駅コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 14)          ' 着駅コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 14)          ' プラントコード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Int, 4)                ' 回線
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 2)           ' 削除フラグ
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.DateTime, 8)           ' 登録年月日
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40)          ' 登録ユーザーＩＤ
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40)          ' 登録端末
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.DateTime, 8)           ' 更新年月日
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)          ' 更新ユーザーＩＤ
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40)          ' 更新端末
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.DateTime, 8)           ' 集信日時


                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 12)         ' 管轄受注営業所
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 1)          ' 入線出線区分
                'Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 8)          ' 入線出線列車番号
                'Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 40)         ' 入線出線列車名
                'Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 14)         ' 発駅コード
                'Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 14)         ' 着駅コード
                'Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.NVarChar, 4)           ' プラントコード
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.Int, 4)               ' 回線
                'Dim JPARA08 As SqlParameter = SQLcmdJnl.Parameters.Add("@P08", SqlDbType.NVarChar, 2)          ' 削除フラグ
                'Dim JPARA09 As SqlParameter = SQLcmdJnl.Parameters.Add("@P09", SqlDbType.DateTime, 8)          ' 登録年月日
                'Dim JPARA10 As SqlParameter = SQLcmdJnl.Parameters.Add("@P10", SqlDbType.NVarChar, 40)         ' 登録ユーザーＩＤ
                'Dim JPARA11 As SqlParameter = SQLcmdJnl.Parameters.Add("@P11", SqlDbType.NVarChar, 40)         ' 登録端末
                'Dim JPARA12 As SqlParameter = SQLcmdJnl.Parameters.Add("@P12", SqlDbType.DateTime, 8)          ' 更新年月日
                'Dim JPARA13 As SqlParameter = SQLcmdJnl.Parameters.Add("@P13", SqlDbType.NVarChar, 40)         ' 更新ユーザーＩＤ
                'Dim JPARA14 As SqlParameter = SQLcmdJnl.Parameters.Add("@P14", SqlDbType.NVarChar, 40)         ' 更新端末
                'Dim JPARA15 As SqlParameter = SQLcmdJnl.Parameters.Add("@P15", SqlDbType.DateTime, 8)          ' 集信日時

                For Each OIM0016row As DataRow In OIM0016tbl.Rows
                    If Trim(OIM0016row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0016row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0016row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIM0016row("OFFICECODE")
                        PARA01.Value = OIM0016row("IOKBN")
                        PARA02.Value = OIM0016row("TRAINNO")
                        PARA03.Value = OIM0016row("TRAINNAME")
                        PARA04.Value = OIM0016row("DEPSTATION")
                        PARA05.Value = OIM0016row("ARRSTATION")
                        PARA06.Value = OIM0016row("PLANTCODE")
                        PARA07.Value = Int32.Parse(OIM0016row("LINE"))
                        PARA08.Value = OIM0016row("DELFLG")
                        PARA09.Value = WW_DATENOW
                        PARA10.Value = Master.USERID
                        PARA11.Value = Master.USERTERMID
                        PARA12.Value = WW_DATENOW
                        PARA13.Value = Master.USERID
                        PARA14.Value = Master.USERTERMID
                        PARA15.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        ' 更新ジャーナル出力
                        JPARA00.Value = OIM0016row("OFFICECODE")
                        JPARA01.Value = OIM0016row("IOKBN")
                        'JPARA02.Value = OIM0016row("TRAINNO")
                        'JPARA03.Value = OIM0016row("TRAINNAME")
                        'JPARA04.Value = OIM0016row("DEPSTATION")
                        'JPARA05.Value = OIM0016row("ARRSTATION")
                        'JPARA06.Value = OIM0016row("PLANTCODE")
                        JPARA07.Value = Int32.Parse(OIM0016row("LINE"))
                        'JPARA08.Value = OIM0016row("DELFLG")
                        'JPARA09.Value = OIM0016row("INITYMD")
                        'JPARA10.Value = OIM0016row("INITUSER")
                        'JPARA11.Value = OIM0016row("INITTERMID")
                        'JPARA12.Value = OIM0016row("UPDYMD")
                        'JPARA13.Value = OIM0016row("UPDUSER")
                        'JPARA14.Value = OIM0016row("UPDTERMID")
                        'JPARA15.Value = OIM0016row("RECEIVEYMD")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0016UPDtbl) Then
                                OIM0016UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0016UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0016UPDtbl.Clear()
                            OIM0016UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0016UPDrow As DataRow In OIM0016UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0016L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0016UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0016L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0016L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIM0016tbl                        ' データ参照  Table
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
        CS0030REPORT.TBLDATA = OIM0016tbl                        ' データ参照Table
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
        Dim TBLview As New DataView(OIM0016tbl)
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
        work.WF_SEL_LINECNT.Text = OIM0016tbl.Rows(WW_LINECNT)("LINECNT")

        ' 管轄受注営業所
        work.WF_SEL_OFFICECODE2.Text = OIM0016tbl.Rows(WW_LINECNT)("OFFICECODE")

        ' 入線出線区分
        work.WF_SEL_IOKBN2.Text = OIM0016tbl.Rows(WW_LINECNT)("IOKBN")

        ' 入線出線列車番号
        work.WF_SEL_TRAINNO.Text = OIM0016tbl.Rows(WW_LINECNT)("TRAINNO")

        ' 入線出線列車名
        work.WF_SEL_TRAINNAME.Text = OIM0016tbl.Rows(WW_LINECNT)("TRAINNAME")

        ' 発駅コード
        work.WF_SEL_DEPSTATION.Text = OIM0016tbl.Rows(WW_LINECNT)("DEPSTATION")

        ' 着駅コード
        work.WF_SEL_ARRSTATION.Text = OIM0016tbl.Rows(WW_LINECNT)("ARRSTATION")

        ' プラントコード
        work.WF_SEL_PLANTCODE.Text = OIM0016tbl.Rows(WW_LINECNT)("PLANTCODE")

        ' 回線
        work.WF_SEL_LINE2.Text = OIM0016tbl.Rows(WW_LINECNT)("LINE")

        ' 削除フラグ
        work.WF_SEL_DELFLG.Text = OIM0016tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIM0016row As DataRow In OIM0016tbl.Rows
            Select Case OIM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIM0016tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIM0016tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIM0016tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIM0016tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIM0016tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIM0016tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

        WF_GridDBclick.Text = ""

        ' 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        ' 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIM0016tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIM0016INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIM0016INProw As DataRow = OIM0016INPtbl.NewRow

            '○ 初期クリア
            For Each OIM0016INPcol As DataColumn In OIM0016INPtbl.Columns
                If IsDBNull(OIM0016INProw.Item(OIM0016INPcol)) OrElse IsNothing(OIM0016INProw.Item(OIM0016INPcol)) Then
                    Select Case OIM0016INPcol.ColumnName
                        Case "LINECNT"
                            OIM0016INProw.Item(OIM0016INPcol) = 0
                        Case "OPERATION"
                            OIM0016INProw.Item(OIM0016INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIM0016INProw.Item(OIM0016INPcol) = 0
                        Case "SELECT"
                            OIM0016INProw.Item(OIM0016INPcol) = 1
                        Case "HIDDEN"
                            OIM0016INProw.Item(OIM0016INPcol) = 0
                        Case Else
                            OIM0016INProw.Item(OIM0016INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("IOKBN") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TRAINNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TRAINNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DEPSTATION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("ARRSTATION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PLANTCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                For Each OIM0016row As DataRow In OIM0016tbl.Rows
                    If XLSTBLrow("OFFICECODE") = OIM0016row("OFFICECODE") AndAlso
                        XLSTBLrow("IOKBN") = OIM0016row("IOKBN") AndAlso
                        XLSTBLrow("TRAINNO") = OIM0016row("TRAINNO") AndAlso
                        XLSTBLrow("TRAINNAME") = OIM0016row("TRAINNAME") AndAlso
                        XLSTBLrow("DEPSTATION") = OIM0016row("DEPSTATION") AndAlso
                        XLSTBLrow("ARRSTATION") = OIM0016row("ARRSTATION") AndAlso
                        XLSTBLrow("PLANTCODE") = OIM0016row("PLANTCODE") AndAlso
                        XLSTBLrow("LINE") = OIM0016row("LINE") AndAlso
                        XLSTBLrow("DELFLG") = OIM0016row("DELFLG") Then
                        OIM0016INProw.ItemArray = OIM0016row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            ' 管轄受注営業所
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
                OIM0016INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
            End If

            ' 入線出線区分
            If WW_COLUMNS.IndexOf("IOKBN") >= 0 Then
                OIM0016INProw("IOKBN") = XLSTBLrow("TRAINNO")
            End If

            ' 入線出線列車番号
            If WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
                OIM0016INProw("TRAINNO") = XLSTBLrow("TRAINNO")
            End If

            ' 入線出線列車名
            If WW_COLUMNS.IndexOf("TRAINNAME") >= 0 Then
                OIM0016INProw("TRAINNAME") = XLSTBLrow("TRAINNAME")
            End If

            ' 発駅コード
            If WW_COLUMNS.IndexOf("DEPSTATION") >= 0 Then
                OIM0016INProw("DEPSTATION") = XLSTBLrow("DEPSTATION")
            End If

            ' 着駅コード
            If WW_COLUMNS.IndexOf("ARRSTATION") >= 0 Then
                OIM0016INProw("ARRSTATION") = XLSTBLrow("ARRSTATION")
            End If

            ' プラントコード
            If WW_COLUMNS.IndexOf("PLANTCODE") >= 0 Then
                OIM0016INProw("PLANTCODE") = XLSTBLrow("PLANTCODE")
            End If

            ' 回線
            If WW_COLUMNS.IndexOf("LINE") >= 0 Then
                OIM0016INProw("LINE") = XLSTBLrow("LINE")
            End If

            ' 削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIM0016INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            OIM0016INPtbl.Rows.Add(OIM0016INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIM0016tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

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
        For Each OIM0016row As DataRow In OIM0016tbl.Rows
            Select Case OIM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0016tbl)

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
        For Each OIM0016INProw As DataRow In OIM0016INPtbl.Rows

            WW_LINE_ERR = ""

            ' 管轄受注営業所（バリデーションチェック）
            WW_TEXT = OIM0016INProw("OFFICECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("OFFICECODE", OIM0016INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 入線出線区分（バリデーションチェック）
            WW_TEXT = OIM0016INProw("IOKBN")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IOKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("IOKBN", OIM0016INProw("IOKBN"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(入線出線区分エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(入線出線区分エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 入線出線列車番号（バリデーションチェック）
            WW_TEXT = OIM0016INProw("TRAINNO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(入線出線列車番号エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 入線出線列車名（バリデーションチェック）
            WW_TEXT = OIM0016INProw("TRAINNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(入線出線列車名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード（バリデーションチェック）
            WW_TEXT = OIM0016INProw("DEPSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0016INProw("DEPSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着駅コード（バリデーションチェック）
            WW_TEXT = OIM0016INProw("ARRSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("STATION", OIM0016INProw("ARRSTATION"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' プラントコード（バリデーションチェック）
            WW_TEXT = OIM0016INProw("PLANTCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("PLANTCODE", OIM0016INProw("PLANTCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(プラントコードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(プラントコードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 回線（バリデーションチェック）
            WW_TEXT = OIM0016INProw("LINE")
            Master.CheckField(work.WF_SEL_LINE.Text, "LINE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(回線エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 削除フラグ（バリデーションチェック）
            WW_TEXT = OIM0016INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ' 値存在チェックT
                CODENAME_get("DELFLG", OIM0016INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0016INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            If WW_LINE_ERR = "" Then
                If OIM0016INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    OIM0016INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    OIM0016INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0016row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0016row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0016row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 管轄受注営業所 =" & OIM0016row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線区分 =" & OIM0016row("IOKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線列車番号 =" & OIM0016row("TRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線列車名 =" & OIM0016row("TRAINNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅コード =" & OIM0016row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅コード =" & OIM0016row("ARRSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プラントコード =" & OIM0016row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 回線 =" & OIM0016row("LINE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0016row("DELFLG")
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
    ''' OIM0016tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0016tbl_UPD()

        '○ 画面状態設定
        For Each OIM0016row As DataRow In OIM0016tbl.Rows
            Select Case OIM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0016INProw As DataRow In OIM0016INPtbl.Rows

            ' エラーレコード読み飛ばし
            If OIM0016INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0016INProw.Item("OPERATION") = CONST_INSERT

            ' KEY項目が等しい時
            For Each OIM0016row As DataRow In OIM0016tbl.Rows
                If OIM0016row("OFFICECODE") = OIM0016INProw("OFFICECODE") AndAlso
                    OIM0016row("IOKBN") = OIM0016INProw("IOKBN") AndAlso
                    OIM0016row("PLANTCODE") = OIM0016INProw("PLANTCODE") Then
                    ' KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0016row("TRAINNO") = OIM0016INProw("TRAINNO") AndAlso
                        OIM0016row("TRAINNAME") = OIM0016INProw("TRAINNAME") AndAlso
                        OIM0016row("DEPSTATION") = OIM0016INProw("DEPSTATION") AndAlso
                        OIM0016row("ARRSTATION") = OIM0016INProw("ARRSTATION") AndAlso
                        OIM0016row("LINE") = OIM0016INProw("LINE") AndAlso
                        OIM0016row("DELFLG") = OIM0016INProw("DELFLG") AndAlso
                        OIM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        ' KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0016INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0016INProw As DataRow In OIM0016INPtbl.Rows
            Select Case OIM0016INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0016INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0016INProw)
                Case CONST_PATTERNERR
                    ' 関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0016INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0016INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0016INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0016INProw As DataRow)

        For Each OIM0016row As DataRow In OIM0016tbl.Rows

            ' 同一レコードか判定
            If OIM0016INProw("OFFICECODE") = OIM0016row("OFFICECODE") AndAlso
                OIM0016INProw("IOKBN") = OIM0016row("IOKBN") AndAlso
                OIM0016INProw("PLANTCODE") = OIM0016row("PLANTCODE") Then
                ' 画面入力テーブル項目設定
                OIM0016INProw("LINECNT") = OIM0016row("LINECNT")
                OIM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0016INProw("UPDTIMSTP") = OIM0016row("UPDTIMSTP")
                OIM0016INProw("SELECT") = 1
                OIM0016INProw("HIDDEN") = 0

                ' 項目テーブル項目設定
                OIM0016row.ItemArray = OIM0016INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0016INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0016INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0016row As DataRow = OIM0016tbl.NewRow
        OIM0016row.ItemArray = OIM0016INProw.ItemArray

        OIM0016row("LINECNT") = OIM0016tbl.Rows.Count + 1
        If OIM0016INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0016row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIM0016row("UPDTIMSTP") = "0"
        OIM0016row("SELECT") = 1
        OIM0016row("HIDDEN") = 0

        OIM0016tbl.Rows.Add(OIM0016row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0016INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0016INProw As DataRow)

        For Each OIM0016row As DataRow In OIM0016tbl.Rows

            ' 同一レコードか判定
            If OIM0016INProw("OFFICECODE") = OIM0016row("OFFICECODE") AndAlso
                OIM0016INProw("IOKBN") = OIM0016row("IOKBN") AndAlso
                OIM0016INProw("PLANTCODE") = OIM0016row("PLANTCODE") Then
                ' 画面入力テーブル項目設定
                OIM0016INProw("LINECNT") = OIM0016row("LINECNT")
                OIM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0016INProw("UPDTIMSTP") = OIM0016row("UPDTIMSTP")
                OIM0016INProw("SELECT") = 1
                OIM0016INProw("HIDDEN") = 0

                ' 項目テーブル項目設定
                OIM0016row.ItemArray = OIM0016INProw.ItemArray
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
                Case "IOKBN"
                    ' 入線出線区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "IOKBN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
'                Case "TRAINNO"
'                    ' 入線出線列車番号
'                    prmData = work.CreateTrainNoParam(work.WF_SEL_OFFICECODE.Text, I_VALUE)
'                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATION"
                    ' 駅
                    prmData = work.CreateFIXParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    ' プラントコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "PLANTCODE")
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

    Private Sub OIM0016RTrainList_Init(sender As Object, e As EventArgs) Handles Me.Init

    End Sub
End Class

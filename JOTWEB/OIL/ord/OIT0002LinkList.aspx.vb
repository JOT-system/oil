﻿''************************************************************
' 貨車連結順序表一覧画面
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
''' 貨車連結順序表テーブル登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIT0002LinkList
    Inherits Page

    '○ 検索結果格納Table
    Private OIT0002tbl As DataTable                                  '一覧格納用テーブル
    Private OIT0002INPtbl As DataTable                               'チェック用テーブル
    Private OIT0002UPDtbl As DataTable                               '更新用テーブル

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
                    Master.RecoverTable(OIT0002tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonALLSELECT"　　　 '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonALLCANCEL"　　　 '選択解除ボタン押下
                            WF_ButtonALLCANCEL_Click()
                        Case "WF_ButtonINSERT"          '行削除ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        'Case "WF_ButtonPrint"           '一覧印刷ボタン押下
                        '    WF_ButtonPrint_Click()
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
                        'Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                        '    WF_FILEUPLOAD()
                        'Case "WF_UPDATE"                '表更新ボタン押下
                        '    WF_UPDATE_Click()
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
            If Not IsNothing(OIT0002tbl) Then
                OIT0002tbl.Clear()
                OIT0002tbl.Dispose()
                OIT0002tbl = Nothing
            End If

            If Not IsNothing(OIT0002INPtbl) Then
                OIT0002INPtbl.Clear()
                OIT0002INPtbl.Dispose()
                OIT0002INPtbl = Nothing
            End If

            If Not IsNothing(OIT0002UPDtbl) Then
                OIT0002UPDtbl.Clear()
                OIT0002UPDtbl.Dispose()
                OIT0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0002WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002D Then
            Master.RecoverTable(OIT0002tbl, work.WF_SEL_INPTBL.Text)
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '空車発駅
        WF_DEPSTATION.Text = work.WF_SEL_DEPSTATION2.Text

        '本線列車
        WF_TRAINNO.Text = work.WF_SEL_TRAINNO2.Text

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0002D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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

        If IsNothing(OIT0002tbl) Then
            OIT0002tbl = New DataTable
        End If

        If OIT0002tbl.Columns.Count <> 0 Then
            OIT0002tbl.Columns.Clear()
        End If

        OIT0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザマスタ、貨車連結順序表テーブルから取得する
        Dim SQLStr As String =
                  " Select " _
                & "    0                                                   As LINECNT " _
                & "    , ''                                                AS OPERATION " _
                & "    , CAST(OIT0004.UPDTIMSTP AS BIGINT)                    AS UPDTIMSTP " _
                & "    , 1                                                 AS 'SELECT' " _
                & "    , 0                                                 AS HIDDEN " _
                & "    , ISNULL(RTRIM(OIT0004.DELFLG), '')                    AS DELFLG " _
                & "    , ISNULL(FORMAT(OIT0004.INITYMD, 'yyyy/MM/dd'), '')      AS INITYMD " _
                & "    , ISNULL(RTRIM(OIT0004.LINKNO), '')                    AS LINKNO " _
                & "    , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                    AS LINKDETAILNO " _
                & "    , ISNULL(RTRIM(OIT0004.STATUS), '')                    AS STATUS " _
                & "    , ISNULL(RTRIM(OIT0004.PREORDERNO), '')                    AS PREORDERNO " _
                & "    , ISNULL(RTRIM(OIT0004.OFFICECODE), '')                    AS OFFICECODE " _
                & "    , ISNULL(RTRIM(OIT0004.TRAINNO), '')                    AS TRAINNO " _
                & "    , ISNULL(RTRIM(OIT0004.DEPSTATION), '')                    AS DEPSTATION " _
                & "    , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')                    AS DEPSTATIONNAME " _
                & "    , ISNULL(RTRIM(OIT0004.RETSTATION), '')                    AS RETSTATION " _
                & "    , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')                    AS RETSTATIONNAME " _
                & "    , ISNULL(FORMAT(OIT0004.EMPARRDATE, 'yyyy/MM/dd'), '')      AS EMPARRDATE " _
                & "    , ISNULL(FORMAT(OIT0004.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')      AS ACTUALEMPARRDATE " _
                & "    , ISNULL(RTRIM(OIT0004.LINETRAINNO), '')                    AS LINETRAINNO " _
                & "    , ISNULL(RTRIM(OIT0004.LINEORDER), '')                    AS LINEORDER " _
                & "    , ISNULL(RTRIM(OIT0004.TANKNUMBER), '')                    AS TANKNUMBER " _
                & "    , ISNULL(RTRIM(OIT0004.PREOILCODE), '')                    AS PREOILCODE " _
                & " FROM " _
                & "    OIL.OIT0004_LINK OIT0004 "

        If work.WF_SEL_TRAINNO.Text <> "" Then
            If work.WF_SEL_SELECT.Text = "1" Then
                SQLStr &=
                  " WHERE" _
                & "    OIT0004.DEPSTATION        = @P1" _
                & "    AND OIT0004.INITYMD      >= @P2" _
                & "    AND OIT0004.INITYMD      <= @P3" _
                & "    AND OIT0004.TRAINNO       = @P4" _
                & "    And OIT0004.STATUS        = @P5" _
                & "    AND OIT0004.DELFLG       <> @P6"
            Else
                SQLStr &=
                  " WHERE" _
                & "    OIT0004.DEPSTATION        = @P1" _
                & "    AND OIT0004.INITYMD      >= @P2" _
                & "    AND OIT0004.INITYMD      <= @P3" _
                & "    AND OIT0004.TRAINNO       = @P4" _
                & "    AND OIT0004.DELFLG       <> @P6"
            End If
        Else
            If work.WF_SEL_SELECT.Text = "1" Then
                SQLStr &=
                  " WHERE" _
                & "    OIT0004.DEPSTATION        = @P1" _
                & "    AND OIT0004.INITYMD      >= @P2" _
                & "    AND OIT0004.INITYMD      <= @P3" _
                & "    And OIT0004.STATUS        = @P5" _
                & "    AND OIT0004.DELFLG       <> @P6"
            Else
                SQLStr &=
                  " WHERE" _
                & "    OIT0004.DEPSTATION        = @P1" _
                & "    AND OIT0004.INITYMD      >= @P2" _
                & "    AND OIT0004.INITYMD      <= @P3" _
                & "    AND OIT0004.DELFLG       <> @P6"
            End If
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0004.LINKNO" _
            & "    , OIT0004.LINKDETAILNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 7)         '空車発駅コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '有効年月日(To)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '有効年月日(From)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)         '本線列車
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         'ステータス
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_DEPSTATION.Text
                PARA2.Value = work.WF_SEL_STYMD.Text
                PARA3.Value = work.WF_SEL_ENDYMD.Text
                PARA4.Value = work.WF_SEL_TRAINNO.Text
                PARA5.Value = work.WF_SEL_SELECT.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    i += 1
                    OIT0002row("LINECNT") = i        'LINECNT
                    ''名称取得
                    'CODENAME_get("CAMPCODE", OIT0002row("CAMPCODE"), OIT0002row("CAMPNAMES"), WW_DUMMY)                               '会社コード
                    'CODENAME_get("ORG", OIT0002row("ORG"), OIT0002row("ORGNAMES"), WW_DUMMY)                                          '組織コード
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L Select"
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
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            If OIT0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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
    ''' 全選択ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 選択解除ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLCANCEL_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        WF_Sel_LINECNT.Text = ""
        work.WF_SEL_LINECNT.Text = ""

        ''開始年月日
        'WF_STYMD.Text = ""
        'work.WF_SEL_STYMD2.Text = ""

        ''終了年月日
        'WF_ENDYMD.Text = ""
        'work.WF_SEL_ENDYMD2.Text = ""

        ''会社コード
        'WF_CAMPCODE.Text = ""
        'work.WF_SEL_CAMPCODE2.Text = ""

        '削除
        WF_DELFLG.Text = "0"
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        WF_GridDBclick.Text = ""

        work.WF_SEL_DELFLG.Text = "0"

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl, work.WF_SEL_INPTBL.Text)

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
        Master.SaveTable(OIT0002tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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

        '○ 日付重複チェック
        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '読み飛ばし
            If (OIT0002row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
                OIT0002row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
                OIT0002row("DELFLG") = C_DELETE_FLG.DELETE Then
                Continue For
            End If

            WW_LINE_ERR = ""

            'チェック
            For Each OIT0002Dhk As DataRow In OIT0002tbl.Rows

                ''同一KEY以外は読み飛ばし
                'If OIT0002row("CAMPCODE") <> OIT0002Dhk("CAMPCODE") OrElse
                '    OIT0002row("USERID") <> OIT0002Dhk("USERID") OrElse
                '    OIT0002Dhk("DELFLG") = C_DELETE_FLG.DELETE Then
                '    Continue For
                'End If

                ''期間変更対象は読み飛ばし
                'If OIT0002row("STYMD") = OIT0002Dhk("STYMD") Then
                '    Continue For
                'End If

                'Try
                '    Date.TryParse(OIT0002row("STYMD"), WW_DATE_ST)
                '    Date.TryParse(OIT0002row("ENDYMD"), WW_DATE_END)
                '    Date.TryParse(OIT0002Dhk("STYMD"), WW_DATE_ST2)
                '    Date.TryParse(OIT0002Dhk("ENDYMD"), WW_DATE_END2)
                'Catch ex As Exception
                '    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
                '    Exit Sub
                'End Try

                ''開始日チェック
                'If WW_DATE_ST >= WW_DATE_ST2 AndAlso WW_DATE_ST <= WW_DATE_END2 Then
                '    WW_CheckMES = "・エラー(期間重複)が存在します。"
                '    WW_CheckERR(WW_CheckMES, "", OIT0002row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINE_ERR = "ERR"
                '    Exit For
                'End If

                ''終了日チェック
                'If WW_DATE_END >= WW_DATE_ST2 AndAlso WW_DATE_END <= WW_DATE_END2 Then
                '    WW_CheckMES = "・エラー(期間重複)が存在します。"
                '    WW_CheckERR(WW_CheckMES, "", OIT0002row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINE_ERR = "ERR"
                '    Exit For
                'End If

                ''日付連続性チェック
                'If WW_DATE_END.AddDays(1) <> WW_DATE_ST2 Then
                '    WW_CheckMES = "・エラー(開始、終了年月日が連続していません)。"
                '    WW_CheckERR(WW_CheckMES, "", OIT0002row)
                '    O_RTNCODE = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                '    WW_LINE_ERR = "ERR"
                '    Exit For
                'End If
            Next

            If WW_LINE_ERR = "" Then
                OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            Else
                OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 貨車連結順序表テーブル登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新(ユーザマスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0004_LINK" _
            & "    WHERE" _
            & "        USERID       = @P01" _
            & "        AND STYMD    = @P08" _
            & "        AND CAMPCODE = @P10 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_LINK" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , STAFFNAMES = @P02" _
            & "        , STAFFNAMEL = @P03" _
            & "        , MAPID = @P04" _
            & "        , ENDYMD = @P09" _
            & "        , ORG = @P11" _
            & "        , EMAIL = @P12" _
            & "        , MENUROLE = @P13" _
            & "        , MAPROLE = @P14" _
            & "        , VIEWPROFID = @P15" _
            & "        , RPRTPROFID = @P16" _
            & "        , VARIANT = @P17" _
            & "        , APPROVALID = @P18" _
            & "        , INITYMD = @P19" _
            & "        , INITUSER = @P20" _
            & "        , INITTERMID = @P21" _
            & "        , UPDYMD = @P22" _
            & "        , UPDUSER = @P23" _
            & "        , UPDTERMID = @P24" _
            & "        , RECEIVEYMD = @P25" _
            & "    WHERE" _
            & "        USERID       = @P01" _
            & "        AND STYMD    = @P08" _
            & "        AND CAMPCODE = @P10 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_LINK" _
            & "        (DELFLG" _
            & "        , USERID" _
            & "        , STAFFNAMES" _
            & "        , STAFFNAMEL" _
            & "        , MAPID" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , VARIANT" _
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
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , VARIANT" _
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
            & "    OIL.OIT0004_LINK" _
            & " WHERE" _
            & "        USERID       = @P01" _
            & "        AND STYMD    = @P08" _
            & "        AND CAMPCODE = @P10"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                'Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                'Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                'Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
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
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)            '登録年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 20)            '登録端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)            '更新年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '更新端末
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)            '集信日時

                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                'Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                'Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                'Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
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
                Dim JPARA19 As SqlParameter = SQLcmdJnl.Parameters.Add("@P19", SqlDbType.DateTime)            '登録年月日
                Dim JPARA20 As SqlParameter = SQLcmdJnl.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim JPARA21 As SqlParameter = SQLcmdJnl.Parameters.Add("@P21", SqlDbType.NVarChar, 20)            '登録端末
                Dim JPARA22 As SqlParameter = SQLcmdJnl.Parameters.Add("@P22", SqlDbType.DateTime)            '更新年月日
                Dim JPARA23 As SqlParameter = SQLcmdJnl.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim JPARA24 As SqlParameter = SQLcmdJnl.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '更新端末
                Dim JPARA25 As SqlParameter = SQLcmdJnl.Parameters.Add("@P25", SqlDbType.DateTime)            '集信日時

                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIT0002row("DELFLG")
                        PARA01.Value = OIT0002row("USERID")
                        PARA02.Value = OIT0002row("STAFFNAMES")
                        PARA03.Value = OIT0002row("STAFFNAMEL")
                        PARA04.Value = OIT0002row("MAPID")
                        'PARA05.Value = OIT0002row("PASSWORD")
                        'If OIT0002row("MISSCNT") <> "" Then
                        '    PARA06.Value = OIT0002row("MISSCNT")
                        'Else
                        '    PARA06.Value = "0"
                        'End If
                        'If RTrim(OIT0002row("PASSENDYMD")) <> "" Then
                        '    PARA07.Value = RTrim(OIT0002row("PASSENDYMD"))
                        'Else
                        '    PARA07.Value = C_DEFAULT_YMD
                        'End If
                        If RTrim(OIT0002row("STYMD")) <> "" Then
                            PARA08.Value = RTrim(OIT0002row("STYMD"))
                        Else
                            PARA08.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIT0002row("ENDYMD")) <> "" Then
                            PARA09.Value = RTrim(OIT0002row("ENDYMD"))
                        Else
                            PARA09.Value = C_DEFAULT_YMD
                        End If
                        PARA10.Value = OIT0002row("CAMPCODE")
                        PARA11.Value = OIT0002row("ORG")
                        PARA12.Value = OIT0002row("EMAIL")
                        PARA13.Value = OIT0002row("MENUROLE")
                        PARA14.Value = OIT0002row("MAPROLE")
                        PARA15.Value = OIT0002row("VIEWPROFID")
                        PARA16.Value = OIT0002row("RPRTPROFID")
                        PARA17.Value = OIT0002row("VARIANT")
                        PARA18.Value = OIT0002row("APPROVALID")
                        PARA19.Value = WW_DATENOW
                        PARA20.Value = Master.USERID
                        PARA21.Value = Master.USERTERMID
                        PARA22.Value = WW_DATENOW
                        PARA23.Value = Master.USERID
                        PARA24.Value = Master.USERTERMID
                        PARA25.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        'OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIT0002row("DELFLG")
                        JPARA01.Value = OIT0002row("USERID")
                        JPARA02.Value = OIT0002row("STAFFNAMES")
                        JPARA03.Value = OIT0002row("STAFFNAMEL")
                        JPARA04.Value = OIT0002row("MAPID")
                        'JPARA05.Value = OIT0002row("PASSWORD")
                        'If OIT0002row("MISSCNT") <> "" Then
                        '    JPARA06.Value = OIT0002row("MISSCNT")
                        'Else
                        '    JPARA06.Value = "0"
                        'End If
                        'If RTrim(OIT0002row("PASSENDYMD")) <> "" Then
                        '    JPARA07.Value = RTrim(OIT0002row("PASSENDYMD"))
                        'Else
                        '    JPARA07.Value = C_DEFAULT_YMD
                        'End If
                        If RTrim(OIT0002row("STYMD")) <> "" Then
                            JPARA08.Value = RTrim(OIT0002row("STYMD"))
                        Else
                            JPARA08.Value = C_DEFAULT_YMD
                        End If
                        If RTrim(OIT0002row("ENDYMD")) <> "" Then
                            JPARA09.Value = RTrim(OIT0002row("ENDYMD"))
                        Else
                            JPARA09.Value = C_DEFAULT_YMD
                        End If
                        JPARA10.Value = OIT0002row("CAMPCODE")
                        JPARA11.Value = OIT0002row("ORG")
                        JPARA12.Value = OIT0002row("EMAIL")
                        JPARA13.Value = OIT0002row("MENUROLE")
                        JPARA14.Value = OIT0002row("MAPROLE")
                        JPARA15.Value = OIT0002row("VIEWPROFID")
                        JPARA16.Value = OIT0002row("RPRTPROFID")
                        JPARA17.Value = OIT0002row("VARIANT")
                        JPARA18.Value = OIT0002row("APPROVALID")
                        JPARA19.Value = WW_DATENOW
                        JPARA20.Value = Master.USERID
                        JPARA21.Value = Master.USERTERMID
                        JPARA22.Value = WW_DATENOW
                        JPARA23.Value = Master.USERID
                        JPARA24.Value = Master.USERTERMID
                        JPARA25.Value = C_DEFAULT_YMD

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIT0002UPDtbl) Then
                                OIT0002UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIT0002UPDtbl.Clear()
                            OIT0002UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIT0002L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ ＤＢ更新(ユーザパスワードマスタ)
        SQLStr =
            "OPEN SYMMETRIC KEY loginpasskey  DECRYPTION BY CERTIFICATE certjotoil;" _
            & " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIS0005_LINKPASS" _
            & "    WHERE" _
            & "        USERID       = @P01 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIS0005_LINKPASS" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , PASSWORD = EncryptByKey(Key_GUID('loginpasskey')  , @P05)" _
            & "        , MISSCNT = @P06" _
            & "        , PASSENDYMD = @P07" _
            & "        , INITYMD = @P19" _
            & "        , INITUSER = @P20" _
            & "        , INITTERMID = @P21" _
            & "        , UPDYMD = @P22" _
            & "        , UPDUSER = @P23" _
            & "        , UPDTERMID = @P24" _
            & "        , RECEIVEYMD = @P25" _
            & "    WHERE" _
            & "        USERID       = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIS0005_LINKPASS" _
            & "        (DELFLG" _
            & "        , USERID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
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
            & "        , EncryptByKey(Key_GUID('loginpasskey')  , @P05)" _
            & "        , @P06" _
            & "        , @P07" _
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
        SQLJnl =
              " Select" _
            & "    DELFLG" _
            & "        , USERID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIS0005_LINKPASS" _
            & " WHERE" _
            & "        USERID       = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                'Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                'Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                'Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
                'Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)            '開始年月日
                'Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Date)            '終了年月日
                'Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 2)            '会社コード
                'Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 6)            '組織コード
                'Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 128)            'メールアドレス
                'Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)            'メニュー表示制御ロール
                'Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '画面参照更新制御ロール
                'Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '画面表示項目制御ロール
                'Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 20)            'エクセル出力制御ロール
                'Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '画面初期値ロール
                'Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '承認権限ロール
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)            '登録年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 20)            '登録端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)            '更新年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '更新端末
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)            '集信日時

                Dim JPARA00 As SqlParameter = SQLcmdJnl.Parameters.Add("@P00", SqlDbType.NVarChar, 1)            '削除フラグ
                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 20)            'ユーザID
                'Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 20)            '社員名（短）
                'Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 50)            '社員名（長）
                'Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 20)            '画面ＩＤ
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 200)            'パスワード
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.Int)            '誤り回数
                Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P07", SqlDbType.Date)            'パスワード有効期限
                'Dim JPARA08 As SqlParameter = SQLcmdJnl.Parameters.Add("@P08", SqlDbType.Date)            '開始年月日
                'Dim JPARA09 As SqlParameter = SQLcmdJnl.Parameters.Add("@P09", SqlDbType.Date)            '終了年月日
                'Dim JPARA10 As SqlParameter = SQLcmdJnl.Parameters.Add("@P10", SqlDbType.NVarChar, 2)            '会社コード
                'Dim JPARA11 As SqlParameter = SQLcmdJnl.Parameters.Add("@P11", SqlDbType.NVarChar, 6)            '組織コード
                'Dim JPARA12 As SqlParameter = SQLcmdJnl.Parameters.Add("@P12", SqlDbType.NVarChar, 128)            'メールアドレス
                'Dim JPARA13 As SqlParameter = SQLcmdJnl.Parameters.Add("@P13", SqlDbType.NVarChar, 20)            'メニュー表示制御ロール
                'Dim JPARA14 As SqlParameter = SQLcmdJnl.Parameters.Add("@P14", SqlDbType.NVarChar, 20)            '画面参照更新制御ロール
                'Dim JPARA15 As SqlParameter = SQLcmdJnl.Parameters.Add("@P15", SqlDbType.NVarChar, 20)            '画面表示項目制御ロール
                'Dim JPARA16 As SqlParameter = SQLcmdJnl.Parameters.Add("@P16", SqlDbType.NVarChar, 20)            'エクセル出力制御ロール
                'Dim JPARA17 As SqlParameter = SQLcmdJnl.Parameters.Add("@P17", SqlDbType.NVarChar, 20)            '画面初期値ロール
                'Dim JPARA18 As SqlParameter = SQLcmdJnl.Parameters.Add("@P18", SqlDbType.NVarChar, 20)            '承認権限ロール
                Dim JPARA19 As SqlParameter = SQLcmdJnl.Parameters.Add("@P19", SqlDbType.DateTime)            '登録年月日
                Dim JPARA20 As SqlParameter = SQLcmdJnl.Parameters.Add("@P20", SqlDbType.NVarChar, 20)            '登録ユーザーＩＤ
                Dim JPARA21 As SqlParameter = SQLcmdJnl.Parameters.Add("@P21", SqlDbType.NVarChar, 20)            '登録端末
                Dim JPARA22 As SqlParameter = SQLcmdJnl.Parameters.Add("@P22", SqlDbType.DateTime)            '更新年月日
                Dim JPARA23 As SqlParameter = SQLcmdJnl.Parameters.Add("@P23", SqlDbType.NVarChar, 20)            '更新ユーザーＩＤ
                Dim JPARA24 As SqlParameter = SQLcmdJnl.Parameters.Add("@P24", SqlDbType.NVarChar, 20)            '更新端末
                Dim JPARA25 As SqlParameter = SQLcmdJnl.Parameters.Add("@P25", SqlDbType.DateTime)            '集信日時

                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA00.Value = OIT0002row("DELFLG")
                        PARA01.Value = OIT0002row("USERID")
                        'PARA02.Value = OIT0002row("STAFFNAMES")
                        'PARA03.Value = OIT0002row("STAFFNAMEL")
                        'PARA04.Value = OIT0002row("MAPID")
                        PARA05.Value = OIT0002row("PASSWORD")
                        If OIT0002row("MISSCNT") <> "" Then
                            PARA06.Value = OIT0002row("MISSCNT")
                        Else
                            PARA06.Value = "0"
                        End If
                        If RTrim(OIT0002row("PASSENDYMD")) <> "" Then
                            PARA07.Value = RTrim(OIT0002row("PASSENDYMD"))
                        Else
                            PARA07.Value = C_DEFAULT_YMD
                        End If
                        'If RTrim(OIT0002row("STYMD")) <> "" Then
                        '    PARA08.Value = RTrim(OIT0002row("STYMD"))
                        'Else
                        '    PARA08.Value = C_DEFAULT_YMD
                        'End If
                        'If RTrim(OIT0002row("ENDYMD")) <> "" Then
                        '    PARA09.Value = RTrim(OIT0002row("ENDYMD"))
                        'Else
                        '    PARA09.Value = C_DEFAULT_YMD
                        'End If
                        'PARA10.Value = OIT0002row("CAMPCODE")
                        'PARA11.Value = OIT0002row("ORG")
                        'PARA12.Value = OIT0002row("EMAIL")
                        'PARA13.Value = OIT0002row("MENUROLE")
                        'PARA14.Value = OIT0002row("MAPROLE")
                        'PARA15.Value = OIT0002row("VIEWPROFID")
                        'PARA16.Value = OIT0002row("RPRTPROFID")
                        'PARA17.Value = OIT0002row("VARIANT")
                        'PARA18.Value = OIT0002row("APPROVALID")
                        PARA19.Value = WW_DATENOW
                        PARA20.Value = Master.USERID
                        PARA21.Value = Master.USERTERMID
                        PARA22.Value = WW_DATENOW
                        PARA23.Value = Master.USERID
                        PARA24.Value = Master.USERTERMID
                        PARA25.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA00.Value = OIT0002row("DELFLG")
                        JPARA01.Value = OIT0002row("USERID")
                        'JPARA02.Value = OIT0002row("STAFFNAMES")
                        'JPARA03.Value = OIT0002row("STAFFNAMEL")
                        'JPARA04.Value = OIT0002row("MAPID")
                        JPARA05.Value = OIT0002row("PASSWORD")
                        If OIT0002row("MISSCNT") <> "" Then
                            JPARA06.Value = OIT0002row("MISSCNT")
                        Else
                            JPARA06.Value = "0"
                        End If
                        If RTrim(OIT0002row("PASSENDYMD")) <> "" Then
                            JPARA07.Value = RTrim(OIT0002row("PASSENDYMD"))
                        Else
                            JPARA07.Value = C_DEFAULT_YMD
                        End If
                        'If RTrim(OIT0002row("STYMD")) <> "" Then
                        '    JPARA08.Value = RTrim(OIT0002row("STYMD"))
                        'Else
                        '    JPARA08.Value = C_DEFAULT_YMD
                        'End If
                        'If RTrim(OIT0002row("ENDYMD")) <> "" Then
                        '    JPARA09.Value = RTrim(OIT0002row("ENDYMD"))
                        'Else
                        '    JPARA09.Value = C_DEFAULT_YMD
                        'End If
                        'JPARA10.Value = OIT0002row("CAMPCODE")
                        'JPARA11.Value = OIT0002row("ORG")
                        'JPARA12.Value = OIT0002row("EMAIL")
                        'JPARA13.Value = OIT0002row("MENUROLE")
                        'JPARA14.Value = OIT0002row("MAPROLE")
                        'JPARA15.Value = OIT0002row("VIEWPROFID")
                        'JPARA16.Value = OIT0002row("RPRTPROFID")
                        'JPARA17.Value = OIT0002row("VARIANT")
                        'JPARA18.Value = OIT0002row("APPROVALID")
                        JPARA19.Value = WW_DATENOW
                        JPARA20.Value = Master.USERID
                        JPARA21.Value = Master.USERTERMID
                        JPARA22.Value = WW_DATENOW
                        JPARA23.Value = Master.USERID
                        JPARA24.Value = Master.USERTERMID
                        JPARA25.Value = C_DEFAULT_YMD

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIT0002UPDtbl) Then
                                OIT0002UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIT0002UPDtbl.Clear()
                            OIT0002UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIT0002L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L UPDATE_INSERT"
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
        CS0030REPORT.TBLDATA = OIT0002tbl                        'データ参照  Table
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

    '''' <summary>
    '''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    '''' </summary>
    '''' <remarks></remarks>
    ''Protected Sub WF_ButtonPrint_Click()

    '    '○ 帳票出力
    '    CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
    '    CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
    '    CS0030REPORT.MAPID = Master.MAPID                       '画面ID
    '    CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
    '    CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
    '    CS0030REPORT.TBLDATA = OIT0002tbl                        'データ参照Table
    '    CS0030REPORT.CS0030REPORT()
    '    If Not isNormal(CS0030REPORT.ERR) Then
    '        If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
    '            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
    '        Else
    '            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
    '        End If
    '        Exit Sub
    '    End If

    '    '○ 別画面でPDFを表示
    '    WF_PrintURL.Value = CS0030REPORT.URL
    '    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    'End Sub


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
        Dim TBLview As New DataView(OIT0002tbl)
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
        WF_Sel_LINECNT.Text = OIT0002tbl.Rows(WW_LINECNT)("LINECNT")
        work.WF_SEL_LINECNT.Text = OIT0002tbl.Rows(WW_LINECNT)("LINECNT")

        ''開始年月日
        'WF_STYMD.Text = OIT0002tbl.Rows(WW_LINECNT)("STYMD")
        'work.WF_SEL_STYMD2.Text = OIT0002tbl.Rows(WW_LINECNT)("STYMD")

        ''終了年月日
        'WF_ENDYMD.Text = OIT0002tbl.Rows(WW_LINECNT)("ENDYMD")
        'work.WF_SEL_ENDYMD2.Text = OIT0002tbl.Rows(WW_LINECNT)("ENDYMD")

        ''会社コード
        'WF_CAMPCODE.Text = OIT0002tbl.Rows(WW_LINECNT)("CAMPCODE")
        'work.WF_SEL_CAMPCODE2.Text = OIT0002tbl.Rows(WW_LINECNT)("CAMPCODE")

        '削除フラグ
        WF_DELFLG.Text = OIT0002tbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)
        work.WF_SEL_DELFLG.Text = OIT0002tbl.Rows(WW_LINECNT)("DELFLG")

        '○ 状態をクリア
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case OIT0002tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIT0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIT0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIT0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIT0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIT0002tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0002tbl, work.WF_SEL_INPTBL.Text)

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
        Master.CreateEmptyTable(OIT0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIT0002INProw As DataRow = OIT0002INPtbl.NewRow

            '○ 初期クリア
            For Each OIT0002INPcol As DataColumn In OIT0002INPtbl.Columns
                If IsDBNull(OIT0002INProw.Item(OIT0002INPcol)) OrElse IsNothing(OIT0002INProw.Item(OIT0002INPcol)) Then
                    Select Case OIT0002INPcol.ColumnName
                        Case "LINECNT"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "OPERATION"
                            OIT0002INProw.Item(OIT0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "UPDTIMSTP"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "SELECT"
                            OIT0002INProw.Item(OIT0002INPcol) = 1
                        Case "HIDDEN"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case Else
                            OIT0002INProw.Item(OIT0002INPcol) = ""
                    End Select
                End If
            Next

            ''○ 変更元情報をデフォルト設定
            'If WW_COLUMNS.IndexOf("USERID") >= 0 Then

            '    For Each OIT0002row As DataRow In OIT0002tbl.Rows
            '        If XLSTBLrow("USERID") = OIT0002row("USERID") AndAlso
            '            XLSTBLrow("STAFFNAMES") = OIT0002row("STAFFNAMES") AndAlso
            '            XLSTBLrow("STAFFNAMEL") = OIT0002row("STAFFNAMEL") AndAlso
            '            XLSTBLrow("STYMD") = OIT0002row("STYMD") AndAlso
            '            XLSTBLrow("ENDYMD") = OIT0002row("ENDYMD") AndAlso
            '            XLSTBLrow("CAMPCODE") = OIT0002row("CAMPCODE") AndAlso
            '            XLSTBLrow("ORG") = OIT0002row("ORG") AndAlso
            '            XLSTBLrow("EMAIL") = OIT0002row("EMAIL") AndAlso
            '            XLSTBLrow("MENUROLE") = OIT0002row("MENUROLE") AndAlso
            '            XLSTBLrow("MAPROLE") = OIT0002row("MAPROLE") AndAlso
            '            XLSTBLrow("VIEWPROFID") = OIT0002row("VIEWPROFID") AndAlso
            '            XLSTBLrow("RPRTPROFID") = OIT0002row("RPRTPROFID") AndAlso
            '            XLSTBLrow("VARIANT") = OIT0002row("VARIANT") AndAlso
            '            XLSTBLrow("APPROVALID") = OIT0002row("APPROVALID") Then
            '            OIT0002INProw.ItemArray = OIT0002row.ItemArray
            '            Exit For
            '        End If
            '    Next
            'End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                OIT0002INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '開始年月日
            If WW_COLUMNS.IndexOf("STYMD") >= 0 Then
                OIT0002INProw("STYMD") = XLSTBLrow("STYMD")
            End If

            '終了年月日
            If WW_COLUMNS.IndexOf("ENDYMD") >= 0 Then
                OIT0002INProw("ENDYMD") = XLSTBLrow("ENDYMD")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIT0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIT0002INProw("DELFLG") = "0"
            End If

            OIT0002INPtbl.Rows.Add(OIT0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIT0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
        DetailBoxToOIT0002INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIT0002tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
    Protected Sub DetailBoxToOIT0002INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIT0002INPtbl)
        Dim OIT0002INProw As DataRow = OIT0002INPtbl.NewRow

        '○ 初期クリア
        For Each OIT0002INPcol As DataColumn In OIT0002INPtbl.Columns
            If IsDBNull(OIT0002INProw.Item(OIT0002INPcol)) OrElse IsNothing(OIT0002INProw.Item(OIT0002INPcol)) Then
                Select Case OIT0002INPcol.ColumnName
                    Case "LINECNT"
                        OIT0002INProw.Item(OIT0002INPcol) = 0
                    Case "OPERATION"
                        OIT0002INProw.Item(OIT0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIT0002INProw.Item(OIT0002INPcol) = 0
                    Case "SELECT"
                        OIT0002INProw.Item(OIT0002INPcol) = 1
                    Case "HIDDEN"
                        OIT0002INProw.Item(OIT0002INPcol) = 0
                    Case Else
                        OIT0002INProw.Item(OIT0002INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIT0002INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIT0002INProw("LINECNT"))
            Catch ex As Exception
                OIT0002INProw("LINECNT") = 0
            End Try
        End If

        OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIT0002INProw("UPDTIMSTP") = 0
        OIT0002INProw("SELECT") = 1
        OIT0002INProw("HIDDEN") = 0

        'OIT0002INProw("STYMD") = WF_STYMD.Text              '開始年月日

        'OIT0002INProw("ENDYMD") = WF_ENDYMD.Text              '終了年月日

        'OIT0002INProw("CAMPCODE") = WF_CAMPCODE.Text              '会社コード

        '○ チェック用テーブルに登録する
        OIT0002INPtbl.Rows.Add(OIT0002INProw)

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
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        'WF_CAMPCODE.Text = ""            '会社コード
        'WF_STYMD.Text = ""            '開始年月日
        'WF_ENDYMD.Text = ""            '終了年月日
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

            With leftview
                Select Case WF_LeftMViewChange.Value
                    'Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                    '    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    '    Select Case WF_FIELD.Value
                    '        Case "WF_STYMD"         '有効年月日(From)
                    '            .WF_Calendar.Text = WF_STYMD.Text
                    '        Case "WF_ENDYMD"        '有効年月日(To)
                    '            .WF_Calendar.Text = WF_ENDYMD.Text
                    '    End Select
                    '    .ActiveCalendar()

                    'Case Else
                    '    '以外
                    '    Dim prmData As New Hashtable
                    '    prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                    'フィールドによってパラメータを変える
                    'Select Case WF_FIELD.Value
                    '    Case "WF_ORG"       '組織コード
                    '        prmData = work.CreateORGParam(WF_CAMPCODE.Text)
                    'End Select

                    '.SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    '    .ActiveListBox()
                End Select
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
                Case "WF_DELFLG"            '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                    'Case "WF_STYMD"             '有効年月日(From)
                    '    Dim WW_DATE As Date
                    '    Try
                    '        Date.TryParse(WW_SelectValue, WW_DATE)
                    '        WF_STYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                    '    Catch ex As Exception
                    '    End Try
                    '    WF_STYMD.Focus()

                    'Case "WF_ENDYMD"            '有効年月日(To)
                    '    Dim WW_DATE As Date
                    '    Try
                    '        Date.TryParse(WW_SelectValue, WW_DATE)
                    '        WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                    '    Catch ex As Exception
                    '    End Try
                    '    WF_ENDYMD.Focus()

                    'Case "WF_ORG"               '組織コード
                    '    WF_ORG.Text = WW_SelectValue
                    '    WF_ORG_TEXT.Text = WW_SelectText
                    '    WF_ORG.Focus()

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
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIT0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIT0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザID(バリデーションチェック)
            'Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERID", OIT0002INProw("USERID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "ユーザID入力エラー。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            'If WW_LINE_ERR = "" Then
            '    If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
            '        OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            '    End If
            'Else
            '    If WW_LINE_ERR = CONST_PATTERNERR Then
            '        '関連チェックエラーをセット
            '        OIT0002INProw.Item("OPERATION") = CONST_PATTERNERR
            '    Else
            '        '単項目チェックエラーをセット
            '        OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            '    End If
            'End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIT0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIT0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIT0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & OIT0002row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & OIT0002row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード =" & OIT0002row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIT0002row("DELFLG")
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
    ''' OIT0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIT0002tbl_UPD()

        '○ 画面状態設定
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIT0002INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIT0002row As DataRow In OIT0002tbl.Rows
                If OIT0002row("STYMD") = OIT0002INProw("STYMD") AndAlso
                    OIT0002row("ENDYMD") = OIT0002INProw("ENDYMD") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIT0002row("DELFLG") = OIT0002INProw("DELFLG") AndAlso
                        OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIT0002INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows
            Select Case OIT0002INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIT0002INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIT0002INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIT0002INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIT0002INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("USERID") = OIT0002row("USERID") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIT0002INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIT0002row As DataRow = OIT0002tbl.NewRow
        OIT0002row.ItemArray = OIT0002INProw.ItemArray

        OIT0002row("LINECNT") = OIT0002tbl.Rows.Count + 1
        If OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIT0002row("UPDTIMSTP") = "0"
        OIT0002row("SELECT") = 1
        OIT0002row("HIDDEN") = 0

        OIT0002tbl.Rows.Add(OIT0002row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("USERID") = OIT0002row("USERID") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
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

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
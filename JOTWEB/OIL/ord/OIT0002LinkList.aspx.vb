﻿''************************************************************
' 貨車連結順序表一覧画面
' 作成日  :2020/07/27
' 更新日  :2020/07/27
' 作成者  :森川
' 更新車  :森川
'
' 修正履歴:新規作成
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
    Private OIT0002WKtbl As DataTable                                '作業用テーブル
    Private OIT0002GETtbl As DataTable                               '取得用テーブル
    Private OIT0002EXLUPtbl As DataTable                             'EXCELアップロード用
    Private OIT0002EXLDELtbl As DataTable                            'EXCELアップロード(削除)用
    Private OIT0002EXLINStbl As DataTable                            'EXCELアップロード(追加(貨車連結表TBL))用
    Private OIT0002Fixvaltbl As DataTable                            '作業用テーブル(固定値マスタ取得用)
    Private OIT0002His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0002His2tbl As DataTable                             '履歴格納用テーブル

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
    'Private CS0013ProfView As New CS0013ProfView_TEST                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 貨車連結順序表アップロード用
    Private WW_ARTICLENAME() As String = {"検", "○"}               '品名

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
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"　　　 '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonINSERT"          '新規登録ボタン押下
                            WF_ButtonINSERT_Click()
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
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView_TEST.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
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
        '     条件指定に従い該当データを貨車連結順序表テーブルから取得する
        Dim SQLStr As String =
                " SELECT " _
            & "    0                                                             AS LINECNT " _
            & "    , ''                                                          AS OPERATION " _
            & "    , 1                                                           AS 'SELECT' " _
            & "    , 0                                                           AS HIDDEN " _
            & "    , ISNULL(RTRIM(OIT0011.RLINKNO), '')                          AS RLINKNO " _
            & "    , ISNULL(RTRIM(OIT0011.LINKNO), '')                           AS LINKNO "

        SQLStr &=
              "    , ''                                                          AS INFO " _
            & "    , ''                                                          AS ORDERINFONAME " _
            & "    , ISNULL(RTRIM(OIT0004.TRAINNO), '')                          AS TRAINNO " _
            & "    , ISNULL(RTRIM(OIT0004.TRAINNAME), '')                        AS TRAINNAME " _
            & "    , ISNULL(RTRIM(OIT0004.OFFICECODE), '')                       AS OFFICECODE " _
            & "    , ''                                                          AS OFFICENAME " _
            & "    , ISNULL(FORMAT(OIT0004.EMPARRDATE, 'yyyy/MM/dd'), NULL)      AS EMPARRDATE "

        SQLStr &=
              "    , ISNULL(RTRIM(OIT0004.DEPSTATION), '')                       AS DEPSTATION " _
            & "    , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')                   AS DEPSTATIONNAME " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATION), '')                       AS RETSTATION " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')                   AS RETSTATIONNAME "

        SQLStr &=
              "	   , COUNT(1)                                                    AS TOTALTANK "

        '油種(ハイオク)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS HTANK ", BaseDllConst.CONST_HTank)
        '油種(レギュラー)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS RTANK ", BaseDllConst.CONST_RTank)
        '油種(灯油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS TTANK ", BaseDllConst.CONST_TTank)
        '油種(未添加灯油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS MTTANK ", BaseDllConst.CONST_MTTank)
        '油種(軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS KTANK ", BaseDllConst.CONST_KTank1)
        '油種(３号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K3TANK ", BaseDllConst.CONST_K3Tank1)
        '油種(５号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K5TANK ", BaseDllConst.CONST_K5Tank)
        '油種(１０号軽油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS K10TANK ", BaseDllConst.CONST_K10Tank)
        '油種(ＬＳＡ)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS LTANK ", BaseDllConst.CONST_LTank1)
        '油種(Ａ重油)
        SQLStr &= String.Format("	   , SUM(CASE WHEN OIT0004.PREOILCODE ='{0}' Then 1 Else 0 End) AS ATANK ", BaseDllConst.CONST_ATank)

        SQLStr &=
              " FROM oil.OIT0011_RLINK OIT0011 " _
            & " INNER JOIN oil.OIT0004_LINK OIT0004 ON " _
            & "     OIT0004.LINKNO       = OIT0011.LINKNO " _
            & " AND OIT0004.LINKDETAILNO = OIT0011.RLINKDETAILNO " _
            & " AND OIT0004.STATUS       = '1' "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '返送列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_SEARCH_BTRAINNO.Text) Then
            SQLStr &= String.Format(" AND OIT0004.TRAINNO      = '{0}'", work.WF_SEL_SEARCH_BTRAINNO.Text)
        End If

        SQLStr &=
              " AND OIT0004.EMPARRDATE  >= @P01 " _
            & " AND OIT0004.DELFLG      <> @P02 " _
            & " WHERE ISNULL(OIT0011.TRUCKSYMBOL,'') <> '' " _
            & " AND ISNULL(OIT0011.LINKNO,'') <> '' "

        SQLStr &=
              " GROUP BY " _
            & "      OIT0011.RLINKNO " _
            & "	    ,OIT0011.LINKNO " _
            & "	    ,OIT0004.TRAINNO " _
            & "	    ,OIT0004.TRAINNAME " _
            & "	    ,OIT0004.OFFICECODE " _
            & "	    ,OIT0004.EMPARRDATE " _
            & "	    ,OIT0004.DEPSTATION " _
            & "	    ,OIT0004.DEPSTATIONNAME " _
            & "	    ,OIT0004.RETSTATION " _
            & "	    ,OIT0004.RETSTATIONNAME "

        SQLStr &=
              " ORDER BY " _
            & "      OIT0004.TRAINNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)                '空車着日
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA01.Value = work.WF_SEL_SEARCH_EMPARRDATE.Text
                PARA02.Value = C_DELETE_FLG.DELETE

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

                    '◯名称取得
                    '受注営業所
                    CODENAME_get("SALESOFFICE", OIT0002row("OFFICECODE"), OIT0002row("OFFICENAME"), WW_DUMMY)                               '会社コード
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
        'CS0013ProfView.SCROLLTYPE = CS0013ProfView_TEST.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
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
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0002tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0002tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0002tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
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
    ''' 選択解除ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

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
    ''' 行削除ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '■■■ OIT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･貨車連結順序表を一括論理削除
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0004_LINK        " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = @P02        " _
                    & "  WHERE LINKNO      = @P01       " _
                    & "    AND DELFLG     <> @P02       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)         '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            intTblCnt = OIT0002tbl.Rows.Count
            For Each OIT0002UPDrow In OIT0002tbl.Rows
                If OIT0002UPDrow("OPERATION") = "on" Then
                    If OIT0002UPDrow("LINECNT") < 9000 Then
                        SelectChk = True
                    End If

                    j += 1
                    OIT0002UPDrow("LINECNT") = j        'LINECNT
                    'OIT0002UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0002UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0002UPDrow("LINKNO")
                    PARA02.Value = C_DELETE_FLG.DELETE
                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0002UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○メッセージ表示
        '一覧件数が０件の時の行削除の場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_DELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            '一覧件数が１件以上で未選択による行削除の場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_DELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

        Else
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If

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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If

    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        ''○初期値設定
        'O_RTNCODE = C_MESSAGE_NO.NORMAL

        'Dim WW_LINEERR_SW As String = ""
        'Dim WW_DUMMY As String = ""
        'Dim WW_CheckMES1 As String = ""
        'Dim WW_CheckMES2 As String = ""
        'Dim WW_LINE_ERR As String = ""
        'Dim WW_CheckMES As String = ""

        ''○ 日付重複チェック
        'For Each OIT0002row As DataRow In OIT0002tbl.Rows

        '    '読み飛ばし
        '    If (OIT0002row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING AndAlso
        '        OIT0002row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED) OrElse
        '        OIT0002row("DELFLG") = C_DELETE_FLG.DELETE Then
        '        Continue For
        '    End If

        '    WW_LINE_ERR = ""

        '    'チェック
        '    For Each OIT0002Dhk As DataRow In OIT0002tbl.Rows

        '        '同一KEY以外は読み飛ばし
        '        If OIT0002row("CAMPCODE") <> OIT0002Dhk("CAMPCODE") OrElse
        '            OIT0002row("RETSTATION") <> OIT0002Dhk("RETSTATION") OrElse
        '            OIT0002row("TRAINNO") <> OIT0002Dhk("TRAINNO") OrElse
        '            OIT0002Dhk("DELFLG") = C_DELETE_FLG.DELETE Then
        '            Continue For
        '        End If
        '    Next

        '    If WW_LINE_ERR = "" Then
        '        OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '    Else
        '        OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End If
        'Next

    End Sub

    ''' <summary>
    ''' 貨車連結順序表テーブル登録更新
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
            & "        OIL.OIT0004_LINK" _
            & "    WHERE" _
            & "        LINKNO          = @P01 " _
            & "   AND  LINKDETAILNO    = @P02 " _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_LINK" _
            & "    SET" _
            & "          AVAILABLEYMD  = @P03    , STATUS           = @P04" _
            & "        , INFO          = @P05    , PREORDERNO       = @P06" _
            & "        , TRAINNO       = @P07    , OFFICECODE       = @P08" _
            & "        , DEPSTATION    = @P09    , DEPSTATIONNAME   = @P10" _
            & "        , RETSTATION    = @P11    , RETSTATIONNAME   = @P12" _
            & "        , EMPARRDATE    = @P13    , ACTUALEMPARRDATE = @P14" _
            & "        , LINETRAINNO   = @P15    , LINEORDER        = @P16" _
            & "        , TANKNUMBER    = @P17    , PREOILCODE       = @P18" _
            & "        , UPDYMD        = @P87    , UPDUSER          = @P88" _
            & "        , UPDTERMID     = @P89    , RECEIVEYMD       = @P90" _
            & "    WHERE" _
            & "        LINKNO            = @P01 " _
            & "        AND  LINKDETAILNO = @P02 " _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_LINK" _
            & "        ( LINKNO       , LINKDETAILNO    , AVAILABLEYMD   , STATUS            , INFO           " _
            & "        , PREORDERNO   , TRAINNO         , OFFICECODE     , DEPSTATION        , DEPSTATIONNAME " _
            & "        , RETSTATION   , RETSTATIONNAME  , EMPARRDATE     , ACTUALEMPARRDATE  , LINETRAINNO    " _
            & "        , LINEORDER    , TANKNUMBER      , PREOILCODE " _
            & "        , DELFLG       , INITYMD         , INITUSER       , INITTERMID " _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID      , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18" _
            & "        , @P83, @P84, @P85, @P86" _
            & "        , @P87, @P88, @P89, @P90) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "      LINKNO" _
            & "    , LINKDETAILNO" _
            & "    , STATUS" _
            & "    , INFO" _
            & "    , PREORDERNO" _
            & "    , TRAINNO" _
            & "    , OFFICECODE" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , EMPARRDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , LINETRAINNO" _
            & "    , LINEORDER" _
            & "    , TANKNUMBER" _
            & "    , PREOILCODE" _
            & "    , AVAILABLEYMD" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0004_LINK" _
            & " WHERE" _
            & "        LINKNO       = @P01" _
            & "   AND  LINKDETAILNO = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '利用可能日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '情報
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 11) '前回オーダー№
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)  '本線列車
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 6)  '登録営業所コード
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 7)  '空車発駅（着駅）コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40) '空車発駅（着駅）名
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 7)  '空車着駅（発駅）コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40) '空車着駅（発駅）名
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Date)         '空車着日（予定）
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Date)         '空車着日（実績）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4)  '入線列車番号
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 2)  '入線順
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 8)  'タンク車№
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4)  '前回油種
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.DateTime)     '登録年月日
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.DateTime)     '更新年月日
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№

                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA01.Value = OIT0002row("LINKNO")
                        PARA02.Value = OIT0002row("LINKDETAILNO")
                        PARA03.Value = OIT0002row("AVAILABLEYMD")
                        PARA04.Value = OIT0002row("STATUS")
                        PARA05.Value = OIT0002row("INFO")
                        PARA06.Value = OIT0002row("PREORDERNO")
                        PARA07.Value = OIT0002row("TRAINNO")
                        PARA08.Value = OIT0002row("OFFICECODE")
                        PARA09.Value = OIT0002row("DEPSTATION")
                        PARA10.Value = OIT0002row("DEPSTATIONNAME")
                        PARA11.Value = OIT0002row("RETSTATION")
                        PARA12.Value = OIT0002row("RETSTATIONNAME")
                        PARA13.Value = OIT0002row("EMPARRDATE")
                        PARA14.Value = OIT0002row("ACTUALEMPARRDATE")
                        PARA15.Value = OIT0002row("LINETRAINNO")
                        PARA16.Value = OIT0002row("LINEORDER")
                        PARA17.Value = OIT0002row("TANKNUMBER")
                        PARA18.Value = OIT0002row("PREOILCODE")
                        PARA83.Value = OIT0002row("DELFLG")
                        PARA84.Value = WW_DATENOW
                        PARA85.Value = Master.USERID
                        PARA86.Value = Master.USERTERMID
                        PARA87.Value = WW_DATENOW
                        PARA88.Value = Master.USERID
                        PARA89.Value = Master.USERTERMID
                        PARA90.Value = C_DEFAULT_YMD
                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIT0002row("LINKNO")
                        JPARA02.Value = OIT0002row("LINKDETAILNO")

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
    '            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
    ''' 新規登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""

        '貨車連結順序表(臨海)№
        work.WF_SEL_RLINKNO.Text = ""

        '貨車連結順序表№
        work.WF_SEL_LINKNO.Text = ""

        '情報
        work.WF_SEL_INFO.Text = ""
        '情報名
        work.WF_SEL_INFONOW.Text = ""

        '返送列車
        work.WF_SEL_BTRAINNO.Text = ""
        work.WF_SEL_BTRAINNAME.Text = ""

        '登録営業所コード
        work.WF_SEL_OFFICECODE.Text = ""
        '登録営業所名
        work.WF_SEL_OFFICENAME.Text = ""

        '空車発駅（着駅）コード
        work.WF_SEL_DEPSTATION.Text = ""
        '空車発駅（着駅）名
        work.WF_SEL_DEPSTATIONNAME.Text = ""

        '空車着駅（発駅）コード
        work.WF_SEL_RETSTATION.Text = ""
        '空車着駅（発駅）名
        work.WF_SEL_RETSTATIONNAME.Text = ""

        '空車着日（予定）
        work.WF_SEL_EMPARRDATE.Text = ""
        '空車着日（実績）
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""

        'タンク車合計
        work.WF_SEL_TANKCARTOTAL.Text = "0"
        'ハイオク(タンク車数)
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = "0"
        'レギュラー(タンク車数)
        work.WF_SEL_REGULAR_TANKCAR.Text = "0"
        '灯油(タンク車数)
        work.WF_SEL_KEROSENE_TANKCAR.Text = "0"
        '未添加灯油(タンク車数)
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '軽油(タンク車数)
        work.WF_SEL_DIESEL_TANKCAR.Text = "0"
        '3号軽油(タンク車数)
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = "0"
        '5号軽油(タンク車数)
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = "0"
        '10号軽油(タンク車数)
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = "0"
        'LSA(タンク車数)
        work.WF_SEL_LSA_TANKCAR.Text = "0"
        'A重油(タンク車数)
        work.WF_SEL_AHEAVY_TANKCAR.Text = "0"

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

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
        work.WF_SEL_LINECNT.Text = OIT0002tbl.Rows(WW_LINECNT)("LINECNT")

        '貨車連結順序表(臨海)№
        work.WF_SEL_RLINKNO.Text = OIT0002tbl.Rows(WW_LINECNT)("RLINKNO")

        '貨車連結順序表№
        work.WF_SEL_LINKNO.Text = OIT0002tbl.Rows(WW_LINECNT)("LINKNO")

        '情報
        work.WF_SEL_INFO.Text = OIT0002tbl.Rows(WW_LINECNT)("INFO")
        '情報名
        work.WF_SEL_INFONOW.Text = OIT0002tbl.Rows(WW_LINECNT)("ORDERINFONAME")

        '返送列車
        work.WF_SEL_BTRAINNO.Text = OIT0002tbl.Rows(WW_LINECNT)("TRAINNO")
        work.WF_SEL_BTRAINNAME.Text = OIT0002tbl.Rows(WW_LINECNT)("TRAINNAME")

        '登録営業所コード
        work.WF_SEL_OFFICECODE.Text = OIT0002tbl.Rows(WW_LINECNT)("OFFICECODE")
        '登録営業所名
        work.WF_SEL_OFFICENAME.Text = OIT0002tbl.Rows(WW_LINECNT)("OFFICENAME")

        '空車発駅（着駅）コード
        work.WF_SEL_DEPSTATION.Text = OIT0002tbl.Rows(WW_LINECNT)("DEPSTATION")
        '空車発駅（着駅）名
        work.WF_SEL_DEPSTATIONNAME.Text = OIT0002tbl.Rows(WW_LINECNT)("DEPSTATIONNAME")

        '空車着駅（発駅）コード
        work.WF_SEL_RETSTATION.Text = OIT0002tbl.Rows(WW_LINECNT)("RETSTATION")
        '空車着駅（発駅）名
        work.WF_SEL_RETSTATIONNAME.Text = OIT0002tbl.Rows(WW_LINECNT)("RETSTATIONNAME")

        '空車着日（予定）
        work.WF_SEL_EMPARRDATE.Text = OIT0002tbl.Rows(WW_LINECNT)("EMPARRDATE")
        '空車着日（実績）
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""

        'タンク車合計
        work.WF_SEL_TANKCARTOTAL.Text = OIT0002tbl.Rows(WW_LINECNT)("TOTALTANK")
        'ハイオク(タンク車数)
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("HTANK")
        'レギュラー(タンク車数)
        work.WF_SEL_REGULAR_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("RTANK")
        '灯油(タンク車数)
        work.WF_SEL_KEROSENE_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("TTANK")
        '未添加灯油(タンク車数)
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("MTTANK")
        '軽油(タンク車数)
        work.WF_SEL_DIESEL_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("KTANK")
        '3号軽油(タンク車数)
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("K3TANK")
        '5号軽油(タンク車数)
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("K5TANK")
        '10号軽油(タンク車数)
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("K10TANK")
        'LSA(タンク車数)
        work.WF_SEL_LSA_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("LTANK")
        'A重油(タンク車数)
        work.WF_SEL_AHEAVY_TANKCAR.Text = OIT0002tbl.Rows(WW_LINECNT)("ATANK")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "2"

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

        '★ファイル判別フラグ
        Dim useFlg As String = ""

        Try
            '○ UPLOAD XLSデータ取得
            CS0023XLSUPLOAD.CS0023XLSUPLOAD_RLINK(OIT0002EXLUPtbl, useFlg)

            '◯列車分解報告(運用指示書あり)、またはポラリス投入用の場合
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            If useFlg = "2" OrElse useFlg = "4" Then
                '配列を初期化
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                '★(UPLOAD XLS)列車番号が在線の場合
                WW_FixvalueMasterSearch(BaseDllConst.CONST_OFFICECODE_011201, "CTRAINNUMBER_FIND", OIT0002EXLUPtbl.Rows(0)("TRAINNO").ToString().Replace("番", "") + "番", WW_GetValue)
                For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Rows
                    If WW_GetValue(0) = "" Then Exit For
                    OIT0002EXLUProw("CONVENTIONAL") = OIT0002EXLUProw("TRAINNO").ToString().Replace("番", "")
                    OIT0002EXLUProw("TRAINNO") = WW_GetValue(0)
                Next
            End If

            '◯発駅コード、着駅コード取得
            For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Rows
                If OIT0002EXLUProw("DEPSTATIONNAME") = "" Then Continue For

                '配列を初期化
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                '発駅名から発駅コードを取得
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "STATIONPATTERN_N", OIT0002EXLUProw("DEPSTATIONNAME"), WW_GetValue)
                OIT0002EXLUProw("DEPSTATIONCODE") = WW_GetValue(0)

                '配列を初期化
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                '着駅名から着駅コードを取得
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "STATIONPATTERN_N", OIT0002EXLUProw("ARRSTATIONNAME"), WW_GetValue)
                OIT0002EXLUProw("ARRSTATIONCODE") = WW_GetValue(0)
            Next

        Catch ex As Exception
            Exit Sub
        End Try

        '◯貨車連結(臨海)TBL削除処理(再アップロード対応)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_DELETE_RLINK(SQLcon)
        End Using

        '◯貨車連結(臨海)TBL追加処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_INSERT_RLINK(SQLcon, useFlg)
        End Using

        '◯運用指示書あり(受注情報が設定)
        If useFlg = "0" OrElse useFlg = "2" OrElse useFlg = "4" Then
            '★受注No取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_GetOrderNo(SQLcon)
            End Using

            '受注明細DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateORDERDETAIL(SQLcon)
            End Using

            '受注DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateORDER(SQLcon)
            End Using
        End If

        '★(一覧)で設定しているタンク車がOT所有か判断
        '割り当てたタンク車のチェック
        Dim WW_GetValueTankSts() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        For Each OIT0002EXLINSrow As DataRow In OIT0002EXLINStbl.Rows

            '配列を初期化
            WW_GetValueTankSts = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

            '★(一覧)タンク車NoがOT本社、または在日米軍のリース車かチェック
            WW_FixvalueMasterSearch("ZZ", "TANKNO_OTCHECK", OIT0002EXLINSrow("TANKNUMBER"), WW_GetValueTankSts)

            'タンク車がOT本社、または在日米軍のリース車の場合
            If WW_GetValueTankSts(0) <> "" Then
                '(タンク車所在TBL)の内容を更新
                '引数１：タンク車状態　⇒　変更あり("3"(到着))
                '引数２：積車区分　　　⇒　変更あり("E"(空車))
                '引数３：タンク車状況　⇒　変更あり("1"(残車))
                '引数４：使用受注№　　⇒　初期化あり(TRUE)
                WW_UpdateTankShozai("3", "E", OIT0002EXLINSrow("TANKNUMBER"), OIT0002EXLINSrow("RETSTATION"), I_SITUATION:="1", I_USEORDERNO:=True)
            End If
        Next

        '◯アップロードデータチェック
        WW_CheckUpload(WW_ERRCODE)
        If WW_ERRCODE = "WAR" Then
            Master.Output(C_MESSAGE_NO.OIL_UPLOAD_WAR_MESSAGE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
        End If

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 貨車連結(臨海)TBL削除処理(再アップロード対応)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    Protected Sub WW_DELETE_RLINK(ByVal SQLcon As SqlConnection)

        '再アップロード時の削除データ取得用
        If IsNothing(OIT0002EXLDELtbl) Then
            OIT0002EXLDELtbl = New DataTable
        End If

        If OIT0002EXLDELtbl.Columns.Count <> 0 Then
            OIT0002EXLDELtbl.Columns.Clear()
        End If

        OIT0002EXLDELtbl.Clear()

        '○ ＤＢ削除
        Dim SQLDelRLinkTblStr As String =
          " DELETE FROM OIL.OIT0011_RLINK WHERE RLINKNO = @P01 AND DELFLG = '0'; "

        Dim SQLDelLinkTblStr As String =
          " DELETE FROM OIL.OIT0004_LINK WHERE LINKNO = @P01 AND DELFLG = '0'; " _

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを貨車連結順序表テーブルから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "      ISNULL(RTRIM(OIT0011.RLINKNO), '')  AS RLINKNO " _
            & "    , ISNULL(RTRIM(OIT0011.LINKNO), '')   AS LINKNO " _
            & "    , ISNULL(RTRIM(OIT0011.ORDERNO), '')  AS ORDERNO " _
            & "    , ISNULL(RTRIM(OIT0011.DETAILNO), '') AS DETAILNO " _
            & "    , ISNULL(RTRIM(OIT0011.TRUCKNO), '')  AS TRUCKNO " _
            & " FROM oil.OIT0011_RLINK OIT0011 " _
            & " WHERE " _
            & "     OIT0011.LINKNO          <> '' " _
            & " AND OIT0011.REGISTRATIONDATE = @P01 " _
            & " AND OIT0011.TRAINNO          = @P02 " _
            & " AND OIT0011.DELFLG          <> @P03 "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon),
                  SQLDel1cmd As New SqlCommand(SQLDelRLinkTblStr, SQLcon),
                  SQLDel2cmd As New SqlCommand(SQLDelLinkTblStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)                '登録年月日
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)         '列車
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA01.Value = OIT0002EXLUPtbl.Rows(0)("REGISTRATIONDATE")
                PARA02.Value = OIT0002EXLUPtbl.Rows(0)("TRAINNO")
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002EXLDELtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002EXLDELtbl.Load(SQLdr)
                End Using

                '★削除対象データが存在した場合
                If OIT0002EXLDELtbl.Rows.Count <> 0 Then
                    '★削除実行(貨車連結表(臨海)テーブル)
                    Dim PARADELRL01 As SqlParameter = SQLDel1cmd.Parameters.Add("@P01", SqlDbType.NVarChar) '貨車連結(臨海)順序表№
                    PARADELRL01.Value = OIT0002EXLDELtbl.Rows(0)("RLINKNO")
                    SQLDel1cmd.ExecuteNonQuery()
                    SQLDel1cmd.Dispose()

                    '★削除実行(貨車連結表テーブル)
                    Dim PARADELL01 As SqlParameter = SQLDel2cmd.Parameters.Add("@P01", SqlDbType.NVarChar)  '貨車連結順序表№
                    For Each OIT0002Exlrow As DataRow In OIT0002EXLDELtbl.Rows
                        PARADELL01.Value = OIT0002Exlrow("LINKNO")
                        SQLDel2cmd.ExecuteNonQuery()
                    Next
                    SQLDel2cmd.Dispose()

                    '★受注No、受注明細Noの引継ぎ処理
                    OIT0002EXLUPtbl.Columns.Add("ORDERNO", Type.GetType("System.String"))
                    OIT0002EXLUPtbl.Columns.Add("DETAILNO", Type.GetType("System.String"))
                    For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows
                        For Each OIT0002Exlrow As DataRow In OIT0002EXLDELtbl.Rows
                            If OIT0002ExlUProw("TRUCKNO") = OIT0002Exlrow("TRUCKNO") _
                            AndAlso OIT0002Exlrow("ORDERNO") <> "" Then
                                OIT0002ExlUProw("ORDERNO") = OIT0002Exlrow("ORDERNO")
                                OIT0002ExlUProw("DETAILNO") = OIT0002Exlrow("DETAILNO")
                            End If
                        Next
                    Next

                Else
                    '★受注No、受注明細Noの引継ぎ処理
                    OIT0002EXLUPtbl.Columns.Add("ORDERNO", Type.GetType("System.String")).DefaultValue = ""
                    OIT0002EXLUPtbl.Columns.Add("DETAILNO", Type.GetType("System.String")).DefaultValue = ""
                    For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows
                        OIT0002ExlUProw("ORDERNO") = ""
                        OIT0002ExlUProw("DETAILNO") = ""
                    Next
                End If

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_RLINK_DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_RLINK_DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨車連結(臨海)TBL追加処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    Protected Sub WW_INSERT_RLINK(ByVal SQLcon As SqlConnection, ByVal useFlg As String)

        Try
            '貨車連結順序表No取得用SQL
            Dim SQLLinkKeyNo As String =
                  "SELECT VIW0001.VALUE1 FROM OIL.VIW0001_FIXVALUE VIW0001 WHERE VIW0001.CLASS = 'NEWLINKNOGET'"

            '貨車連結(臨海)順序表No取得用SQL
            Dim SQLRLinkKeyNo As String =
                  "SELECT VIW0001.VALUE1 FROM OIL.VIW0001_FIXVALUE VIW0001 WHERE VIW0001.CLASS = 'NEWRLINKNOGET'"

            '更新SQL文･･･貨車連結表(臨海)TBLの各フラグを更新
            Dim SQLRLinkStr As String =
                  " INSERT INTO OIL.OIT0011_RLINK " _
                & " ( RLINKNO       , RLINKDETAILNO  , FILENAME        , AGOBEHINDFLG    , REGISTRATIONDATE" _
                & " , TRAINNO       , CONVENTIONAL   , CONVENTIONALTIME, SERIALNUMBER    , TRUCKSYMBOL   , TRUCKNO" _
                & " , DEPSTATIONNAME, ARRSTATIONNAME , ARTICLENAME     , INSPECTIONDATE  , CONVERSIONAMOUNT" _
                & " , ARTICLE       , ARTICLETRAINNO , ARTICLEOILNAME" _
                & " , OILNAME       , LINE           , POSITION        , INLINETRAIN     , LOADARRSTATION" _
                & " , LOADINGTRAINNO, LOADINGLODDATE , LOADINGDEPDATE  , CURRENTCARTOTAL" _
                & " , EXTEND        , CONVERSIONTOTAL, LINKNO          , ORDERNO         , DETAILNO" _
                & " , DELFLG        , INITYMD        , INITUSER        , INITTERMID" _
                & " , UPDYMD        , UPDUSER        , UPDTERMID       , RECEIVEYMD)"

            SQLRLinkStr &=
                  " VALUES" _
                & " ( @RLINKNO       , @RLINKDETAILNO  , @FILENAME        , @AGOBEHINDFLG    , @REGISTRATIONDATE" _
                & " , @TRAINNO       , @CONVENTIONAL   , @CONVENTIONALTIME, @SERIALNUMBER    , @TRUCKSYMBOL   , @TRUCKNO" _
                & " , @DEPSTATIONNAME, @ARRSTATIONNAME , @ARTICLENAME     , @INSPECTIONDATE  , @CONVERSIONAMOUNT" _
                & " , @ARTICLE       , @ARTICLETRAINNO , @ARTICLEOILNAME" _
                & " , @OILNAME       , @LINE           , @POSITION        , @INLINETRAIN     , @LOADARRSTATION" _
                & " , @LOADINGTRAINNO, @LOADINGLODDATE , @LOADINGDEPDATE  , @CURRENTCARTOTAL" _
                & " , @EXTEND        , @CONVERSIONTOTAL, @LINKNO          , @ORDERNO         , @DETAILNO" _
                & " , @DELFLG        , @INITYMD        , @INITUSER        , @INITTERMID" _
                & " , @UPDYMD        , @UPDUSER        , @UPDTERMID       , @RECEIVEYMD);"

            'Dim SQLTempTblStr As String =
            '  " SELECT LINKNO FROM OIL.OIT0011_RLINK WHERE REGISTRATIONDATE = @P01 AND TRAINNO = @P02;"

            Using SQLKeyNocmd As New SqlCommand(SQLRLinkKeyNo, SQLcon),
                  SQLRLinkcmd As New SqlCommand(SQLRLinkStr, SQLcon),
                  SQLKeyNo2cmd As New SqlCommand(SQLLinkKeyNo, SQLcon)

                '貨車連結(臨海)順序表No取得
                Dim sRLinkNo As String
                Using SQLdr As SqlDataReader = SQLKeyNocmd.ExecuteReader()
                    SQLdr.Read()
                    sRLinkNo = SQLdr(0)
                End Using
                'CLOSE
                SQLKeyNocmd.Dispose()

                '### 20200710 START((全体)No102対応) ######################################
                '貨車連結順序表No取得
                Dim sLinkNo As String = ""
                Using SQLdr As SqlDataReader = SQLKeyNo2cmd.ExecuteReader()
                    SQLdr.Read()
                    sLinkNo = SQLdr(0)
                End Using
                'CLOSE
                SQLKeyNo2cmd.Dispose()
                '### 20200710 END  ((全体)No102対応) ######################################

                Dim WW_DATENOW As DateTime = Date.Now
                Dim RLINKNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@RLINKNO", SqlDbType.NVarChar)                     '貨車連結(臨海)順序表№
                Dim RLINKDETAILNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@RLINKDETAILNO", SqlDbType.NVarChar)         '貨車連結(臨海)順序表明細№
                Dim FILENAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@FILENAME", SqlDbType.NVarChar)                   'ファイル名
                Dim AGOBEHINDFLG As SqlParameter = SQLRLinkcmd.Parameters.Add("@AGOBEHINDFLG", SqlDbType.NVarChar)           '前後フラグ
                Dim REGISTRATIONDATE As SqlParameter = SQLRLinkcmd.Parameters.Add("@REGISTRATIONDATE", SqlDbType.Date)       '登録年月日
                Dim TRAINNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)                     '列車
                Dim CONVENTIONAL As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVENTIONAL", SqlDbType.NVarChar)           '在来線
                Dim CONVENTIONALTIME As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVENTIONALTIME", SqlDbType.NVarChar)   '在来線時間
                Dim SERIALNUMBER As SqlParameter = SQLRLinkcmd.Parameters.Add("@SERIALNUMBER", SqlDbType.Int)                '通番
                Dim TRUCKSYMBOL As SqlParameter = SQLRLinkcmd.Parameters.Add("@TRUCKSYMBOL", SqlDbType.NVarChar)             '貨車(記号及び符号)
                Dim TRUCKNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@TRUCKNO", SqlDbType.NVarChar)                     '貨車(番号)
                Dim DEPSTATIONNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@DEPSTATIONNAME", SqlDbType.NVarChar)       '発駅
                Dim ARRSTATIONNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARRSTATIONNAME", SqlDbType.NVarChar)       '着駅
                Dim ARTICLENAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLENAME", SqlDbType.NVarChar)             '品名
                Dim INSPECTIONDATE As SqlParameter = SQLRLinkcmd.Parameters.Add("@INSPECTIONDATE", SqlDbType.NVarChar)       '交検年月日
                Dim CONVERSIONAMOUNT As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVERSIONAMOUNT", SqlDbType.Decimal)    '換算数量
                Dim ARTICLE As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLE", SqlDbType.NVarChar)                     '記事
                Dim ARTICLETRAINNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLETRAINNO", SqlDbType.NVarChar)       '記事(列車)
                Dim ARTICLEOILNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLEOILNAME", SqlDbType.NVarChar)       '記事(油種)
                ' ### 運送指示書(項目) START #########################################################################
                Dim OILNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@OILNAME", SqlDbType.NVarChar)                     '油種(運用指示)
                Dim LINE As SqlParameter = SQLRLinkcmd.Parameters.Add("@LINE", SqlDbType.NVarChar)                           '回転(運用指示)
                Dim POSITION As SqlParameter = SQLRLinkcmd.Parameters.Add("@POSITION", SqlDbType.NVarChar)                   '位置(運用指示)
                Dim INLINETRAIN As SqlParameter = SQLRLinkcmd.Parameters.Add("@INLINETRAIN", SqlDbType.NVarChar)             '入線列車(運用指示)
                Dim LOADARRSTATION As SqlParameter = SQLRLinkcmd.Parameters.Add("@LOADARRSTATION", SqlDbType.NVarChar)       '着駅(運用指示)
                Dim LOADINGTRAINNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@LOADINGTRAINNO", SqlDbType.NVarChar)       '本線列車(運用指示)
                Dim LOADINGLODDATE As SqlParameter = SQLRLinkcmd.Parameters.Add("@LOADINGLODDATE", SqlDbType.NVarChar)       '積込日(運用指示)
                Dim LOADINGDEPDATE As SqlParameter = SQLRLinkcmd.Parameters.Add("@LOADINGDEPDATE", SqlDbType.NVarChar)       '発日(運用指示)
                ' ### 運送指示書(項目) END   #########################################################################
                Dim CURRENTCARTOTAL As SqlParameter = SQLRLinkcmd.Parameters.Add("@CURRENTCARTOTAL", SqlDbType.Decimal)      '現車合計
                Dim EXTEND As SqlParameter = SQLRLinkcmd.Parameters.Add("@EXTEND", SqlDbType.Decimal)                        '延長
                Dim CONVERSIONTOTAL As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVERSIONTOTAL", SqlDbType.Decimal)      '換算合計
                Dim LINKNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@LINKNO", SqlDbType.NVarChar)                       '貨車連結順序表№
                Dim ORDERNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)                     '受注№
                Dim DETAILNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@DETAILNO", SqlDbType.NVarChar)                   '受注明細№
                Dim DELFLG As SqlParameter = SQLRLinkcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)                       '削除フラグ
                Dim INITYMD As SqlParameter = SQLRLinkcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)                     '登録年月日
                Dim INITUSER As SqlParameter = SQLRLinkcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar)                   '登録ユーザーＩＤ
                Dim INITTERMID As SqlParameter = SQLRLinkcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar)               '登録端末
                Dim UPDYMD As SqlParameter = SQLRLinkcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)                       '更新年月日
                Dim UPDUSER As SqlParameter = SQLRLinkcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)                     '更新ユーザーＩＤ
                Dim UPDTERMID As SqlParameter = SQLRLinkcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)                 '更新端末
                Dim RECEIVEYMD As SqlParameter = SQLRLinkcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)               '集信日時

                '発駅・着駅名(保存用)
                Dim strDepstationName As String = ""
                Dim strArrstationName As String = ""
                '### 内部テーブルに出線順の値を設定するための準備 ########################
                Dim dcOutOrder As DataColumn = New DataColumn
                Dim iTblTotal As Integer = OIT0002EXLUPtbl.Select("TRUCKSYMBOL<>''").Count
                dcOutOrder.ColumnName = "OUTORDER"
                dcOutOrder.DefaultValue = String.Empty
                dcOutOrder.DataType = Type.GetType("System.String")
                OIT0002EXLUPtbl.Columns.Add(dcOutOrder)
                '#########################################################################
                'For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows
                For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Select(Nothing, "ARRSTATIONNAME, DEPSTATIONNAME")
                    'Select Case (Nothing, "ARRSTATIONNAME, DEPSTATIONNAME, SERIALNUMBER")

                    '貨車連結(臨海)順序表№
                    RLINKNO.Value = sRLinkNo
                    OIT0002EXLUProw("RLINKNO") = sRLinkNo
                    '貨車連結(臨海)順序表明細№
                    RLINKDETAILNO.Value = OIT0002EXLUProw("RLINKDETAILNO")
                    'ファイル名
                    FILENAME.Value = OIT0002EXLUProw("FILENAME")
                    '前後フラグ
                    AGOBEHINDFLG.Value = OIT0002EXLUProw("AGOBEHINDFLG")
                    '登録年月日
                    REGISTRATIONDATE.Value = OIT0002EXLUProw("REGISTRATIONDATE")
                    'REGISTRATIONDATE.Value = DBNull.Value
                    '列車
                    TRAINNO.Value = OIT0002EXLUProw("TRAINNO")
                    '在来線
                    CONVENTIONAL.Value = OIT0002EXLUProw("CONVENTIONAL")
                    '在来線時間
                    CONVENTIONALTIME.Value = OIT0002EXLUProw("CONVENTIONALTIME")
                    '通番
                    If OIT0002EXLUProw("SERIALNUMBER") = "" Then
                        SERIALNUMBER.Value = DBNull.Value
                    Else
                        SERIALNUMBER.Value = Integer.Parse(OIT0002EXLUProw("SERIALNUMBER"))
                    End If
                    '貨車(記号及び符号)
                    TRUCKSYMBOL.Value = OIT0002EXLUProw("TRUCKSYMBOL")
                    '貨車(番号)
                    TRUCKNO.Value = OIT0002EXLUProw("TRUCKNO")
                    '発駅
                    DEPSTATIONNAME.Value = OIT0002EXLUProw("DEPSTATIONNAME")
                    '着駅
                    ARRSTATIONNAME.Value = OIT0002EXLUProw("ARRSTATIONNAME")
                    '品名
                    ARTICLENAME.Value = OIT0002EXLUProw("ARTICLENAME")
                    '交検年月日
                    If OIT0002EXLUProw("INSPECTIONDATE") = "" Then
                        INSPECTIONDATE.Value = DBNull.Value
                    Else
                        INSPECTIONDATE.Value = OIT0002EXLUProw("INSPECTIONDATE")
                    End If
                    '換算数量
                    If OIT0002EXLUProw("CONVERSIONAMOUNT") = "" Then
                        CONVERSIONAMOUNT.Value = DBNull.Value
                    Else
                        CONVERSIONAMOUNT.Value = Decimal.Parse(OIT0002EXLUProw("CONVERSIONAMOUNT"))
                    End If
                    '記事
                    ARTICLE.Value = OIT0002EXLUProw("ARTICLE")
                    '列車(記事)
                    Try
                        ARTICLETRAINNO.Value = OIT0002EXLUProw("ARTICLE").ToString().Substring(0, 4)
                    Catch ex As Exception
                        ARTICLETRAINNO.Value = ""
                    End Try
                    '油種名(記事)
                    Try
                        ARTICLEOILNAME.Value = OIT0002EXLUProw("ARTICLE").ToString().Substring(4)
                    Catch ex As Exception
                        ARTICLEOILNAME.Value = ""
                        If OIT0002EXLUProw("ARTICLE").ToString().Length <= 2 Then
                            ARTICLEOILNAME.Value = OIT0002EXLUProw("ARTICLE").ToString()
                        End If
                    End Try

                    ' ### 運送指示書(項目) START ####################################
                    '◯運用指示書あり(受注情報が設定)
                    If useFlg = "0" OrElse useFlg = "2" OrElse useFlg = "4" Then
                        '油種(運用指示)
                        OILNAME.Value = OIT0002EXLUProw("OILNAME")
                        '回転(運用指示)
                        LINE.Value = OIT0002EXLUProw("LINE")
                        '位置(運用指示)
                        POSITION.Value = OIT0002EXLUProw("POSITION")
                        '入線列車(運用指示)
                        INLINETRAIN.Value = OIT0002EXLUProw("INLINETRAIN")
                        '着駅(運用指示)
                        LOADARRSTATION.Value = OIT0002EXLUProw("LOADARRSTATION")
                        '本線列車(運用指示)
                        LOADINGTRAINNO.Value = OIT0002EXLUProw("LOADINGTRAINNO")
                        '積込日(運用指示)
                        If OIT0002EXLUProw("LOADINGLODDATE").ToString() = "" Then
                            LOADINGLODDATE.Value = DBNull.Value
                        Else
                            LOADINGLODDATE.Value = OIT0002EXLUProw("LOADINGLODDATE")
                        End If
                        '発日(運用指示)
                        If OIT0002EXLUProw("LOADINGDEPDATE").ToString() = "" Then
                            LOADINGDEPDATE.Value = DBNull.Value
                        Else
                            LOADINGDEPDATE.Value = OIT0002EXLUProw("LOADINGDEPDATE")
                        End If
                    ElseIf useFlg = "1" Then
                        '油種(運用指示)
                        OILNAME.Value = ""
                        '回転(運用指示)
                        LINE.Value = ""
                        '位置(運用指示)
                        POSITION.Value = ""
                        '入線列車(運用指示)
                        INLINETRAIN.Value = ""
                        '着駅(運用指示)
                        LOADARRSTATION.Value = ""
                        '本線列車(運用指示)
                        LOADINGTRAINNO.Value = ""
                        '積込日(運用指示)
                        LOADINGLODDATE.Value = DBNull.Value
                        '発日(運用指示)
                        LOADINGDEPDATE.Value = DBNull.Value
                    End If
                    ' ### 運送指示書(項目) END   ####################################

                    '現車合計
                    If OIT0002EXLUProw("CURRENTCARTOTAL") = "" Then
                        CURRENTCARTOTAL.Value = DBNull.Value
                    Else
                        CURRENTCARTOTAL.Value = Decimal.Parse(OIT0002EXLUProw("CURRENTCARTOTAL"))
                    End If
                    '延長
                    If OIT0002EXLUProw("EXTEND") = "" Then
                        EXTEND.Value = DBNull.Value
                    Else
                        EXTEND.Value = Decimal.Parse(OIT0002EXLUProw("EXTEND"))
                    End If
                    '換算合計
                    If OIT0002EXLUProw("CONVERSIONTOTAL") = "" Then
                        CONVERSIONTOTAL.Value = DBNull.Value
                    Else
                        CONVERSIONTOTAL.Value = Decimal.Parse(OIT0002EXLUProw("CONVERSIONTOTAL"))
                    End If

                    '### 20200710 START((全体)No102対応) ######################################
                    '貨車連結順序表№
                    If strArrstationName <> "" _
                        AndAlso (strArrstationName <> ARRSTATIONNAME.Value _
                                 OrElse (strArrstationName = ARRSTATIONNAME.Value AndAlso strDepstationName <> DEPSTATIONNAME.Value)) _
                        AndAlso TRUCKSYMBOL.Value <> "" Then
                        Dim sLinkNoBak1 As String = sLinkNo
                        Dim iLinkNoBak1 As Integer
                        sLinkNo = sLinkNoBak1.Substring(0, 9)
                        iLinkNoBak1 = Integer.Parse(sLinkNoBak1.Substring(9, 2)) + 1
                        sLinkNo &= iLinkNoBak1.ToString("00")
                    End If

                    Dim cvTruckSymbol As String = StrConv(TRUCKSYMBOL.Value, Microsoft.VisualBasic.VbStrConv.Wide, &H411)
                    '★貨車(記号及び符号)が未設定
                    If cvTruckSymbol = "" Then
                        '貨車連結順序表№は未設定
                        LINKNO.Value = ""
                    ElseIf cvTruckSymbol.Substring(0, 1) = "コ" _
                        OrElse cvTruckSymbol.Substring(0, 1) = "チ" Then
                        '貨車連結順序表№は未設定
                        LINKNO.Value = ""
                    Else
                        LINKNO.Value = sLinkNo
                    End If

                    '★受注№
                    ORDERNO.Value = OIT0002EXLUProw("ORDERNO")

                    '★受注明細№
                    DETAILNO.Value = OIT0002EXLUProw("DETAILNO")

                    '★着駅名を保存
                    strDepstationName = DEPSTATIONNAME.Value
                    strArrstationName = ARRSTATIONNAME.Value
                    '### 20200710 END  ((全体)No102対応) ######################################

                    '削除フラグ
                    DELFLG.Value = C_DELETE_FLG.ALIVE
                    '登録年月日
                    INITYMD.Value = Date.Now
                    '登録ユーザーＩＤ
                    INITUSER.Value = Master.USERID
                    '登録端末
                    INITTERMID.Value = Master.USERTERMID
                    '更新年月日
                    UPDYMD.Value = Date.Now
                    '更新ユーザーＩＤ
                    UPDUSER.Value = Master.USERID
                    '更新端末
                    UPDTERMID.Value = Master.USERTERMID
                    '集信日時
                    RECEIVEYMD.Value = C_DEFAULT_YMD

                    '### ★内部テーブルにて『出線順』を設定 ###################################
                    If OIT0002EXLUProw("TRUCKSYMBOL") <> "" Then
                        OIT0002EXLUProw("OUTORDER") = iTblTotal
                        iTblTotal -= 1
                    End If
                    '##########################################################################

                    SQLRLinkcmd.CommandTimeout = 300
                    SQLRLinkcmd.ExecuteNonQuery()
                Next
                'CLOSE
                SQLRLinkcmd.Dispose()

                '貨車連結TBL追加処理
                WW_INSERT_LINK(SQLcon, WW_ERRCODE, useFlg, I_RLinkNo:=sRLinkNo)
                If WW_ERRCODE = "ERR" Then
                    Exit Sub
                End If

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_RLINK_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_RLINK_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 貨車連結TBL追加処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    Protected Sub WW_INSERT_LINK(ByVal SQLcon As SqlConnection,
                                 ByRef O_RTN As String,
                                 ByVal I_UseFlg As String,
                                 Optional ByVal I_RLinkNo As String = Nothing)

        If IsNothing(OIT0002EXLINStbl) Then
            OIT0002EXLINStbl = New DataTable
        End If

        If OIT0002EXLINStbl.Columns.Count <> 0 Then
            OIT0002EXLINStbl.Columns.Clear()
        End If

        OIT0002EXLINStbl.Clear()

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_DATENOW As DateTime = Date.Now

        Try
            '更新SQL文･･･貨車連結表TBLの各フラグを更新
            Dim SQLLinkStr As String =
                  " INSERT INTO OIL.OIT0004_LINK " _
                & " ( LINKNO         , LINKDETAILNO      , AVAILABLEYMD, STATUS        , INFO" _
                & " , PREORDERNO     , TRAINNO           , TRAINNAME   , OFFICECODE" _
                & " , DEPSTATION     , DEPSTATIONNAME    , RETSTATION  , RETSTATIONNAME" _
                & " , EMPARRDATE     , ACTUALEMPARRDATE  , LINETRAINNO , LINEORDER" _
                & " , TANKNUMBER     , PREOILCODE        , PREOILNAME" _
                & " , PREORDERINGTYPE, PREORDERINGOILNAME" _
                & " , DELFLG         , INITYMD           , INITUSER    , INITTERMID" _
                & " , UPDYMD         , UPDUSER           , UPDTERMID   , RECEIVEYMD)"

            Dim SQLStr As String =
                  " SELECT DISTINCT" _
                & "   OIT0011.LINKNO                                   AS LINKNO" _
                & " , OIT0011.RLINKDETAILNO                            AS LINKDETAILNO" _
                & " , OIT0011.REGISTRATIONDATE                         AS AVAILABLEYMD" _
                & " , '1'                                              AS STATUS" _
                & " , ''                                               AS INFO" _
                & " , ''                                               AS PREORDERNO" _
                & " , OIT0011.TRAINNO                                  AS TRAINNO" _
                & " , CASE" _
                & "   WHEN OIT0011.CONVENTIONAL = '' THEN OIT0011.TRAINNO" _
                & "   ELSE OIT0011.CONVENTIONAL" _
                & "   END                                              AS TRAINNAME" _
                & " , VIW0002.OFFICECODE                               AS OFFICECODE" _
                & " , VIW0002.DEPSTATION                               AS DEPSTATION" _
                & " , VIW0002.DEPSTATIONNAME                           AS DEPSTATIONNAME" _
                & " , VIW0002.ARRSTATION                               AS RETSTATION" _
                & " , VIW0002.ARRSTATIONNAME                           AS RETSTATIONNAME" _
                & " , ISNULL(CONVERT(NVARCHAR, (CONVERT(DATETIME, OIT0011.REGISTRATIONDATE)" _
                & "   + (CONVERT(INT, VIW0001.VALUE7) - CONVERT(INT, VIW0001.VALUE6))), 111), OIT0011.REGISTRATIONDATE) AS EMPARRDATE" _
                & " , NULL                                             AS ACTUALEMPARRDATE" _
                & " , ''                                               AS LINETRAINNO" _
                & " , OIT0011.SERIALNUMBER                             AS LINEORDER" _
                & " , OIT0011.TRUCKNO                                  AS TANKNUMBER" _
                & " , ISNULL(TMP0005.OILCODE, OIT0005.LASTOILCODE)     AS PREOILCODE" _
                & " , ISNULL(TMP0005.OILNAME, OIT0005.LASTOILNAME)     AS PREOILNAME" _
                & " , ISNULL(TMP0005.SEGMENTOILCODE, OIT0005.PREORDERINGTYPE)    AS PREORDERINGTYPE" _
                & " , ISNULL(TMP0005.SEGMENTOILNAME, OIT0005.PREORDERINGOILNAME) AS PREORDERINGOILNAME" _
                & String.Format(" , '{0}'                              AS DELFLG", C_DELETE_FLG.ALIVE) _
                & String.Format(" , '{0}'                              AS INITYMD", WW_DATENOW) _
                & String.Format(" , '{0}'                              AS INITUSER", Master.USERID) _
                & String.Format(" , '{0}'                              AS INITTERMID", Master.USERTERMID) _
                & String.Format(" , '{0}'                              AS UPDYMD", WW_DATENOW) _
                & String.Format(" , '{0}'                              AS UPDUSER", Master.USERID) _
                & String.Format(" , '{0}'                              AS UPDTERMID", Master.USERTERMID) _
                & String.Format(" , '{0}'                              AS RECEIVEYMD", C_DEFAULT_YMD)

            '### 20200923 START(受注用の油種コード(JOT)を取得) ########################
            Dim SQLGetOil As String =
                  SQLStr _
                & " , ISNULL(VIW0001_OILCONVERT.VALUE1, '')            AS OILCODE" _
                & " , ISNULL(VIW0001_OILCONVERT.KEYCODE, '')           AS OILNAME" _
                & " , ISNULL(VIW0001_OILCONVERT.VALUE2, '')            AS ORDERINGTYPE" _
                & " , ISNULL(VIW0001_OILCONVERT.VALUE3, '')            AS ORDERINGOILNAME"
            'Dim SQLGetOil As String =
            '      SQLStr _
            '    & " , ISNULL(TMP0005_OILCONVERT.OILCODE, '')           AS OILCODE" _
            '    & " , ISNULL(TMP0005_OILCONVERT.OILNAME, '')           AS OILNAME" _
            '    & " , ISNULL(TMP0005_OILCONVERT.SEGMENTOILCODE, '')    AS ORDERINGTYPE" _
            '    & " , ISNULL(TMP0005_OILCONVERT.SEGMENTOILNAME, '')    AS ORDERINGOILNAME" _
            '### 20200923 END  (受注用の油種コード(JOT)を取得) ########################

            'SQLStr &=
            Dim SQLCmn As String =
                  " FROM OIL.OIT0011_RLINK OIT0011" _
                & " LEFT JOIN OIL.VIW0002_LINKCONVERTMASTER VIW0002 ON" _
                & "  VIW0002.DEPSTATIONNAME = OIT0011.DEPSTATIONNAME" _
                & "  AND VIW0002.ARRSTATIONNAME = OIT0011.ARRSTATIONNAME" _
                & " LEFT JOIN OIL.OIM0007_TRAIN OIM0007 ON" _
                & "  OIM0007.OTTRAINNO = OIT0011.TRAINNO" _
                & "  AND OIM0007.DEPSTATION = VIW0002.DEPSTATION" _
                & "  AND OIM0007.ARRSTATION = VIW0002.ARRSTATION"
            SQLCmn &= String.Format("  AND OIM0007.DELFLG <> '{0}'", C_DELETE_FLG.DELETE)

            '### 20200706 START((内部)No184対応) ######################################
            SQLCmn &=
                  " LEFT JOIN OIL.TMP0005OILMASTER TMP0005 ON" _
                & "  TMP0005.OFFICECODE = VIW0002.OFFICECODE" _
                & "  AND TMP0005.OILNo = '1'" _
                & "  AND TMP0005.RINKAIOILCODE <> ''" _
                & "  AND TMP0005.RINKAIOILKANA = OIT0011.ARTICLEOILNAME"
            SQLCmn &= String.Format("  AND TMP0005.OILCODE IN ('{0}', '{1}', '{2}', '{3}', '{4}')",
                                        BaseDllConst.CONST_HTank,
                                        BaseDllConst.CONST_RTank,
                                        BaseDllConst.CONST_TTank,
                                        BaseDllConst.CONST_KTank1,
                                        BaseDllConst.CONST_ATank)

            '### 20200923 START(受注用の油種コード(JOT)を取得) ########################
            SQLCmn &=
                  " LEFT JOIN OIL.VIW0001_FIXVALUE VIW0001_OILCONVERT ON" _
                & "  VIW0001_OILCONVERT.CLASS = 'PRODUCTPATTERN_N'" _
                & "  AND VIW0001_OILCONVERT.CAMPCODE = VIW0002.OFFICECODE" _
                & "  AND VIW0001_OILCONVERT.VALUE3 = OIT0011.OILNAME"
            ''### 20200910 START(受注用の油種コードを取得) #############################
            'SQLCmn &=
            '      " LEFT JOIN OIL.TMP0005OILMASTER TMP0005_OILCONVERT ON" _
            '    & "  TMP0005_OILCONVERT.OFFICECODE = VIW0002.OFFICECODE" _
            '    & "  AND TMP0005_OILCONVERT.OILNo = '1'" _
            '    & "  AND TMP0005_OILCONVERT.RINKAIOILCODE <> ''" _
            '    & "  AND TMP0005_OILCONVERT.RINKAIOILKANA = OIT0011.OILNAME"
            'SQLCmn &= String.Format("  AND TMP0005_OILCONVERT.OILCODE IN ('{0}', '{1}', '{2}', '{3}', '{4}')",
            '                            BaseDllConst.CONST_HTank,
            '                            BaseDllConst.CONST_RTank,
            '                            BaseDllConst.CONST_TTank,
            '                            BaseDllConst.CONST_KTank1,
            '                            BaseDllConst.CONST_ATank)
            ''### 20200910 END  (受注用の油種コードを取得) #############################
            '### 20200923 END  (受注用の油種コード(JOT)を取得) ########################

            SQLCmn &=
                  " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON" _
                & "  OIT0005.TANKNUMBER = OIT0011.TRUCKNO"
            SQLCmn &= String.Format("  AND OIT0005.DELFLG <> '{0}'", C_DELETE_FLG.DELETE)
            '### 20200706 END  ((内部)No184対応) ######################################

            '### 20200710 START 列車マスタ(返送)から次回利用可能日を取得 ##############
            SQLCmn &=
                  " LEFT JOIN OIL.VIW0001_FIXVALUE VIW0001 ON" _
                & "  VIW0001.CLASS = 'BTRAINNUMBER_FIND'" _
                & "  AND VIW0001.CAMPCODE = VIW0002.OFFICECODE" _
                & "  AND VIW0001.KEYCODE = OIT0011.TRAINNO + VIW0002.DEPSTATION"
            '### 20200710 END   列車マスタ(返送)から次回利用可能日を取得 ##############

            SQLCmn &= String.Format(" WHERE OIT0011.DELFLG <> '{0}'", C_DELETE_FLG.DELETE) _
                & "  AND OIT0011.TRUCKSYMBOL <> ''" _
                & "  AND OIT0011.LINKNO <> ''" _
                & "  AND VIW0002.OFFICECODE IS NOT NULL"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '貨車連結(臨海)順序表№
            If Not String.IsNullOrEmpty(I_RLinkNo) Then
                SQLCmn &= String.Format("    AND OIT0011.RLINKNO = '{0}'", I_RLinkNo)
            End If

            '### 20200717 START((全体)No114対応) ######################################
            '★ 貨車連結順序表アップロード時において、品目が交検以外を対象とする。
            SQLCmn &= String.Format("    AND OIT0011.ARTICLENAME <> '{0}'", WW_ARTICLENAME)
            '### 20200717 START((全体)No114対応) ######################################

            'SQL結合(INSERT文とSELECT分)
            SQLStr &= SQLCmn
            SQLLinkStr &= SQLStr

            'SQL結合(SELECT分(Fetch用))
            SQLGetOil &= SQLCmn

            Using SQLLinkcmd As New SqlCommand(SQLLinkStr, SQLcon),
                  SQLcmd As New SqlCommand(SQLGetOil, SQLcon)
                'SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLLinkcmd.CommandTimeout = 300
                SQLLinkcmd.ExecuteNonQuery()

                'CLOSE
                SQLLinkcmd.Dispose()

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002EXLINStbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002EXLINStbl.Load(SQLdr)
                End Using

                'マスタ検索用配列
                Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                Dim strOfficeName As String = ""

                '★営業所コード、本線列車名、発着駅、油種の設定
                OIT0002EXLUPtbl.Columns.Add("OFFICECODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("OFFICENAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("LOADINGTRAINNAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("DEPSTATION", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("RETSTATION", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("ORDEROILCODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("ORDEROILNAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("ORDERINGTYPE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("ORDERINGOILNAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("LOADINGARRDATE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("LOADINGACCDATE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("LOADINGEMPARRDATE", Type.GetType("System.String"))
                '◯運用指示書あり(受注情報が設定)
                If I_UseFlg = "0" OrElse I_UseFlg = "2" OrElse I_UseFlg = "4" Then
                    For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows
                        For Each OIT0002ExlINSrow As DataRow In OIT0002EXLINStbl.Rows
                            If OIT0002ExlUProw("TRUCKNO") = OIT0002ExlINSrow("TANKNUMBER") Then

                                '受注営業所
                                OIT0002ExlUProw("OFFICECODE") = OIT0002ExlINSrow("OFFICECODE")
                                '◯名称取得
                                CODENAME_get("SALESOFFICE", OIT0002ExlUProw("OFFICECODE"), strOfficeName, WW_DUMMY)
                                OIT0002ExlUProw("OFFICENAME") = strOfficeName

                                '空車着駅
                                OIT0002ExlUProw("RETSTATION") = OIT0002ExlINSrow("RETSTATION")
                                '油種
                                OIT0002ExlUProw("ORDEROILCODE") = OIT0002ExlINSrow("OILCODE")
                                OIT0002ExlUProw("ORDEROILNAME") = OIT0002ExlINSrow("OILNAME")
                                OIT0002ExlUProw("ORDERINGTYPE") = OIT0002ExlINSrow("ORDERINGTYPE")
                                OIT0002ExlUProw("ORDERINGOILNAME") = OIT0002ExlINSrow("ORDERINGOILNAME")

                                '本線列車が未登録の場合はSKIP
                                If OIT0002ExlUProw("LOADINGTRAINNO") = "" Then Continue For

                                '本線列車名
                                OIT0002ExlUProw("LOADINGTRAINNAME") = OIT0002ExlUProw("LOADINGTRAINNO") + "-" + OIT0002ExlUProw("LOADARRSTATION")

                                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "STATIONPATTERN_N", OIT0002ExlUProw("LOADARRSTATION"), WW_GetValue)
                                OIT0002ExlUProw("DEPSTATION") = WW_GetValue(0)
                                'OIT0002ExlUProw("DEPSTATION") = OIT0002ExlINSrow("DEPSTATION")

                                '積車着日(予定), 受入日(予定), 空車着日(予定)
                                If OIT0002ExlUProw("LOADINGDEPDATE").ToString() = "" Then Continue For
                                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                                'WW_FixvalueMasterSearch(OIT0002ExlUProw("OFFICECODE"), "TRAINNUMBER_FIND", OIT0002ExlUProw("LOADINGTRAINNAME"), WW_GetValue)
                                'WW_FixvalueMasterSearch(OIT0002ExlUProw("OFFICECODE"), "TRAINNUMBER_FIND", OIT0002ExlUProw("LOADINGTRAINNO") + "-" + OIT0002ExlUProw("LOADARRSTATION"), WW_GetValue)
                                WW_FixvalueMasterSearch(OIT0002ExlUProw("OFFICECODE"), "TRAINNUMBER_FIND", OIT0002ExlUProw("LOADINGTRAINNO") + OIT0002ExlUProw("DEPSTATION"), WW_GetValue)

                                Try
                                    '〇 (予定)の日付を設定
                                    If Integer.Parse(WW_GetValue(6)) = 0 Then
                                        OIT0002ExlUProw("LOADINGARRDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                                        OIT0002ExlUProw("LOADINGACCDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                                        OIT0002ExlUProw("LOADINGEMPARRDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays(Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                                    ElseIf Integer.Parse(WW_GetValue(6)) > 0 Then
                                        OIT0002ExlUProw("LOADINGARRDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(8))).ToString("yyyy/MM/dd")
                                        OIT0002ExlUProw("LOADINGACCDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(9))).ToString("yyyy/MM/dd")
                                        OIT0002ExlUProw("LOADINGACCDATE") = Date.Parse(OIT0002ExlUProw("LOADINGDEPDATE")).AddDays((-1 * Integer.Parse(WW_GetValue(6))) + Integer.Parse(WW_GetValue(10))).ToString("yyyy/MM/dd")
                                    End If
                                Catch ex As Exception
                                    OIT0002ExlUProw("LOADINGARRDATE") = DBNull.Value
                                    OIT0002ExlUProw("LOADINGACCDATE") = DBNull.Value
                                    OIT0002ExlUProw("LOADINGEMPARRDATE") = DBNull.Value
                                End Try
                            End If
                        Next
                    Next
                End If

                '★列車番号(臨海)の設定
                OIT0002EXLUPtbl.Columns.Add("INLINETRAINNAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("OUTLINETRAIN", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("OUTLINETRAINNAME", Type.GetType("System.String"))
                '◯運用指示書あり(受注情報が設定)
                If I_UseFlg = "0" OrElse I_UseFlg = "2" OrElse I_UseFlg = "4" Then
                    For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows

                        '★入線列車番号が未設定の場合はSKIP
                        If OIT0002ExlUProw("INLINETRAIN") = "" Then
                            Continue For
                        Else
                            '★入線列車名の設定
                            OIT0002ExlUProw("INLINETRAINNAME") = OIT0002ExlUProw("INLINETRAIN") + "レ"

                            '★甲子営業所の場合は入線列車名を追加設定
                            If OIT0002ExlUProw("OFFICECODE").ToString() = BaseDllConst.CONST_OFFICECODE_011202 _
                                AndAlso OIT0002ExlUProw("LINE") <> "" Then
                                If OIT0002ExlUProw("LINE") = "11" Then
                                    OIT0002ExlUProw("INLINETRAINNAME") &= "1"
                                ElseIf OIT0002ExlUProw("LINE") = "12" Then
                                    OIT0002ExlUProw("INLINETRAINNAME") &= "2"
                                End If
                            End If
                        End If

                        '〇営業所配下情報を取得・設定
                        WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                        WW_FixvalueMasterSearch(OIT0002ExlUProw("OFFICECODE").ToString(), "RINKAITRAIN_FIND_I", OIT0002ExlUProw("INLINETRAINNAME"), WW_GetValue)

                        '出線列車番号
                        OIT0002ExlUProw("OUTLINETRAIN") = WW_GetValue(6)
                        '出線列車名
                        OIT0002ExlUProw("OUTLINETRAINNAME") = WW_GetValue(7)

                    Next
                End If

                '★荷主、基地、荷受人、受注パターンの設定
                OIT0002EXLUPtbl.Columns.Add("SHIPPERSCODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("SHIPPERSNAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("BASECODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("BASENAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("CONSIGNEECODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("CONSIGNEENAME", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("PATTERNCODE", Type.GetType("System.String"))
                OIT0002EXLUPtbl.Columns.Add("PATTERNNAME", Type.GetType("System.String"))
                '◯運用指示書あり(受注情報が設定)
                If I_UseFlg = "0" OrElse I_UseFlg = "2" OrElse I_UseFlg = "4" Then
                    For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows
                        If OIT0002ExlUProw("LOADINGTRAINNO") <> "" Then

                            '〇営業所配下情報を取得・設定
                            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                            WW_FixvalueMasterSearch(OIT0002ExlUProw("OFFICECODE").ToString(), "PATTERNMASTER", OIT0002ExlUProw("DEPSTATION"), WW_GetValue)

                            OIT0002ExlUProw("SHIPPERSCODE") = WW_GetValue(0)
                            OIT0002ExlUProw("SHIPPERSNAME") = WW_GetValue(1)
                            OIT0002ExlUProw("BASECODE") = WW_GetValue(2)
                            OIT0002ExlUProw("BASENAME") = WW_GetValue(3)
                            OIT0002ExlUProw("CONSIGNEECODE") = WW_GetValue(4)
                            OIT0002ExlUProw("CONSIGNEENAME") = WW_GetValue(5)
                            OIT0002ExlUProw("PATTERNCODE") = WW_GetValue(6)
                            OIT0002ExlUProw("PATTERNNAME") = WW_GetValue(7)

                        End If
                    Next
                End If
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_LINK_INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_LINK_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = "ERR"
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' 受注TBLから受注Noを取得(受注TBLに未存在の場合は新規で受注Noを設定)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_GetOrderNo(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002GETtbl) Then
            OIT0002GETtbl = New DataTable
        End If

        If OIT0002GETtbl.Columns.Count <> 0 Then
            OIT0002GETtbl.Columns.Clear()
        End If

        OIT0002GETtbl.Clear()

        Dim SQLStr As String =
              " SELECT" _
            & "   OIT0002.ORDERNO                                           AS ORDERNO" _
            & " , MAX(OIT0003.DETAILNO)                                     AS DETAILNO" _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' Then 1 Else 0 End)    AS TOTALTANK"

        '油種(ハイオク)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS HTANK ", BaseDllConst.CONST_HTank)
        '油種(レギュラー)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS RTANK ", BaseDllConst.CONST_RTank)
        '油種(灯油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS TTANK ", BaseDllConst.CONST_TTank)
        '油種(未添加灯油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS MTTANK ", BaseDllConst.CONST_MTTank)
        '油種(軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS KTANK ", BaseDllConst.CONST_KTank1)
        '油種(３号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K3TANK ", BaseDllConst.CONST_K3Tank1)
        '油種(５号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K5TANK ", BaseDllConst.CONST_K5Tank)
        '油種(１０号軽油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K10TANK ", BaseDllConst.CONST_K10Tank)
        '油種(ＬＳＡ)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS LTANK ", BaseDllConst.CONST_LTank1)
        '油種(Ａ重油)
        SQLStr &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS ATANK ", BaseDllConst.CONST_ATank)

        SQLStr &=
              " FROM OIL.OIT0002_ORDER OIT0002" _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003 ON" _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO" _
            & " AND OIT0003.DELFLG <> @DELFLG" _
            & " WHERE " _
            & "     OIT0002.OFFICECODE = @OFFICECODE" _
            & " AND OIT0002.TRAINNAME  = @TRAINNAME" _
            & " AND OIT0002.LODDATE    = @LODDATE" _
            & " AND OIT0002.DEPDATE    = @DEPDATE" _
            & " AND OIT0002.DELFLG    <> @DELFLG"

        SQLStr &=
              " GROUP BY OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim P_TRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar, 40)   '本線列車名
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)               '積込日(予定)
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.Date)               '発日(予定)
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ

                P_DELFLG.Value = C_DELETE_FLG.DELETE

                '受注№取得
                Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                WW_FixvalueMasterSearch("ZZ", "NEWORDERNOGET", "", WW_GetValue)
                Dim sOrderNo As String = WW_GetValue(0)

                '退避用
                Dim sOrderContent() As String = {"", "", "", "", "", ""}
                Dim iNum As Integer

                For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Select(Nothing, "LOADINGTRAINNAME, LOADINGLODDATE, LOADINGDEPDATE, ORDERNO, DETAILNO")

                    '★すでに受注Noが設定されているデータはSKIP
                    If OIT0002EXLUProw("ORDERNO").ToString() <> "" Then Continue For
                    If OIT0002EXLUProw("LOADINGTRAINNAME").ToString() = "" Then Continue For

                    '同じオーダーの場合
                    If sOrderContent(2) = OIT0002EXLUProw("OFFICECODE").ToString() _
                       AndAlso sOrderContent(3) = OIT0002EXLUProw("LOADINGTRAINNAME").ToString() _
                       AndAlso sOrderContent(4) = OIT0002EXLUProw("LOADINGLODDATE").ToString() _
                       AndAlso sOrderContent(5) = OIT0002EXLUProw("LOADINGDEPDATE").ToString() Then

                        OIT0002EXLUProw("ORDERNO") = sOrderContent(0)
                        iNum = Integer.Parse(sOrderContent(1)) + 1
                        OIT0002EXLUProw("DETAILNO") = iNum.ToString("000")

                    Else
                        P_OFFICECODE.Value = OIT0002EXLUProw("OFFICECODE").ToString()
                        P_TRAINNAME.Value = OIT0002EXLUProw("LOADINGTRAINNAME").ToString()
                        P_LODDATE.Value = OIT0002EXLUProw("LOADINGLODDATE").ToString()
                        P_DEPDATE.Value = OIT0002EXLUProw("LOADINGDEPDATE").ToString()
                        P_DELFLG.Value = C_DELETE_FLG.DELETE

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            If OIT0002GETtbl.Columns.Count = 0 Then
                                '○ フィールド名とフィールドの型を取得
                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIT0002GETtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIT0002GETtbl.Clear()

                            '○ テーブル検索結果をテーブル格納
                            OIT0002GETtbl.Load(SQLdr)

                        End Using

                        '★受注TBLに存在しない場合
                        If OIT0002GETtbl.Rows.Count = 0 Then
                            OIT0002EXLUProw("ORDERNO") = sOrderNo
                            OIT0002EXLUProw("DETAILNO") = "001"

                            '次回用に受注Noをカウント
                            iNum = Integer.Parse(sOrderNo.Substring(9, 2)) + 1
                            sOrderNo = sOrderNo.Substring(0, 9) + iNum.ToString("00")
                        Else
                            '存在する場合は、設定されている受注Noを設定
                            OIT0002EXLUProw("ORDERNO") = OIT0002GETtbl.Rows(0)("ORDERNO")
                            iNum = Integer.Parse(OIT0002GETtbl.Rows(0)("DETAILNO")) + 1
                            OIT0002EXLUProw("DETAILNO") = iNum.ToString("000")

                        End If

                        'sOrderContent(0) = OIT0002row("ORDERNO")
                        'sOrderContent(1) = OIT0002row("DETAILNO")
                        'sOrderContent(2) = OIT0002row("OFFICECODE")
                        'sOrderContent(3) = OIT0002row("LOADINGTRAINNAME")
                        'sOrderContent(4) = OIT0002row("LOADINGLODDATE")
                        'sOrderContent(5) = OIT0002row("LOADINGDEPDATE")

                    End If
                    sOrderContent(0) = OIT0002EXLUProw("ORDERNO")
                    sOrderContent(1) = OIT0002EXLUProw("DETAILNO")
                    sOrderContent(2) = OIT0002EXLUProw("OFFICECODE")
                    sOrderContent(3) = OIT0002EXLUProw("LOADINGTRAINNAME")
                    sOrderContent(4) = OIT0002EXLUProw("LOADINGLODDATE")
                    sOrderContent(5) = OIT0002EXLUProw("LOADINGDEPDATE")
                Next

                '設定した受注№、受注明細№を【貨車連結表(臨海)TBL】に反映
                For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Rows
                    If OIT0002EXLUProw("ORDERNO").ToString() <> "" Then
                        WW_UpdateRLinkOrderNo(SQLcon, OIT0002EXLUProw)
                    End If
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L GET_ORDERNO", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L GET_ORDERNO"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注明細TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateORDERDETAIL(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0003_DETAIL" _
            & "    WHERE" _
            & "        ORDERNO  = @ORDERNO" _
            & "   AND  DETAILNO = @DETAILNO" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0003_DETAIL" _
            & "    SET" _
            & "        LINEORDER               = @LINEORDER            , TANKNO                  = @TANKNO" _
            & "        , STACKINGFLG           = @STACKINGFLG          , OTTRANSPORTFLG          = @OTTRANSPORTFLG" _
            & "        , SHIPPERSCODE          = @SHIPPERSCODE         , SHIPPERSNAME            = @SHIPPERSNAME" _
            & "        , OILCODE               = @OILCODE              , OILNAME                 = @OILNAME" _
            & "        , ORDERINGTYPE          = @ORDERINGTYPE         , ORDERINGOILNAME         = @ORDERINGOILNAME" _
            & "        , LINE                  = @LINE                 , FILLINGPOINT            = @FILLINGPOINT" _
            & "        , LOADINGIRILINETRAINNO = @LOADINGIRILINETRAINNO, LOADINGIRILINETRAINNAME = @LOADINGIRILINETRAINNAME" _
            & "        , LOADINGIRILINEORDER   = @LOADINGIRILINEORDER" _
            & "        , LOADINGOUTLETTRAINNO  = @LOADINGOUTLETTRAINNO , LOADINGOUTLETTRAINNAME  = @LOADINGOUTLETTRAINNAME" _
            & "        , LOADINGOUTLETORDER    = @LOADINGOUTLETORDER" _
            & "        , DELFLG                = @DELFLG" _
            & "        , UPDYMD                = @UPDYMD               , UPDUSER                 = @UPDUSER" _
            & "        , UPDTERMID             = @UPDTERMID            , RECEIVEYMD              = @RECEIVEYMD" _
            & "    WHERE" _
            & "        ORDERNO          = @ORDERNO" _
            & "        AND DETAILNO     = @DETAILNO" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO              , DETAILNO               , LINEORDER          , TANKNO" _
            & "        , STACKINGFLG          , FIRSTRETURNFLG         , AFTERRETURNFLG     , OTTRANSPORTFLG" _
            & "        , ORDERINFO            , SHIPPERSCODE           , SHIPPERSNAME" _
            & "        , OILCODE              , OILNAME                , ORDERINGTYPE       , ORDERINGOILNAME" _
            & "        , CARSNUMBER           , CARSAMOUNT             , LINE               , FILLINGPOINT" _
            & "        , LOADINGIRILINETRAINNO, LOADINGIRILINETRAINNAME, LOADINGIRILINEORDER" _
            & "        , LOADINGOUTLETTRAINNO , LOADINGOUTLETTRAINNAME , LOADINGOUTLETORDER" _
            & "        , RESERVEDNO           , OTSENDCOUNT            , DLRESERVEDCOUNT    , DLTAKUSOUCOUNT" _
            & "        , SALSE                , SALSETAX               , TOTALSALSE" _
            & "        , PAYMENT              , PAYMENTTAX             , TOTALPAYMENT" _
            & "        , DELFLG               , INITYMD                , INITUSER           , INITTERMID" _
            & "        , UPDYMD               , UPDUSER                , UPDTERMID          , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @ORDERNO              , @DETAILNO               , @LINEORDER          , @TANKNO" _
            & "        , @STACKINGFLG          , @FIRSTRETURNFLG         , @AFTERRETURNFLG     , @OTTRANSPORTFLG" _
            & "        , @ORDERINFO            , @SHIPPERSCODE           , @SHIPPERSNAME" _
            & "        , @OILCODE              , @OILNAME                , @ORDERINGTYPE       , @ORDERINGOILNAME" _
            & "        , @CARSNUMBER           , @CARSAMOUNT             , @LINE               , @FILLINGPOINT" _
            & "        , @LOADINGIRILINETRAINNO, @LOADINGIRILINETRAINNAME, @LOADINGIRILINEORDER" _
            & "        , @LOADINGOUTLETTRAINNO , @LOADINGOUTLETTRAINNAME , @LOADINGOUTLETORDER" _
            & "        , @RESERVEDNO           , @OTSENDCOUNT            , @DLRESERVEDCOUNT    , @DLTAKUSOUCOUNT" _
            & "        , @SALSE                , @SALSETAX               , @TOTALSALSE" _
            & "        , @PAYMENT              , @PAYMENTTAX             , @TOTALPAYMENT" _
            & "        , @DELFLG               , @INITYMD                , @INITUSER           , @INITTERMID" _
            & "        , @UPDYMD               , @UPDUSER                , @UPDTERMID          , @RECEIVEYMD) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , DETAILNO" _
            & "    , LINEORDER" _
            & "    , TANKNO" _
            & "    , STACKINGFLG" _
            & "    , FIRSTRETURNFLG" _
            & "    , AFTERRETURNFLG" _
            & "    , OTTRANSPORTFLG" _
            & "    , ORDERINFO" _
            & "    , SHIPPERSCODE" _
            & "    , SHIPPERSNAME" _
            & "    , OILCODE" _
            & "    , OILNAME" _
            & "    , ORDERINGTYPE" _
            & "    , ORDERINGOILNAME" _
            & "    , CARSNUMBER" _
            & "    , CARSAMOUNT" _
            & "    , LINE" _
            & "    , FILLINGPOINT" _
            & "    , LOADINGIRILINETRAINNO" _
            & "    , LOADINGIRILINETRAINNAME" _
            & "    , LOADINGIRILINEORDER" _
            & "    , LOADINGOUTLETTRAINNO" _
            & "    , LOADINGOUTLETTRAINNAME" _
            & "    , LOADINGOUTLETORDER" _
            & "    , RESERVEDNO" _
            & "    , OTSENDCOUNT" _
            & "    , DLRESERVEDCOUNT" _
            & "    , DLTAKUSOUCOUNT" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0003_DETAIL" _
            & " WHERE" _
            & "        ORDERNO  = @ORDERNO" _
            & "   AND  DETAILNO = @DETAILNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11)           '受注№
                Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", SqlDbType.NVarChar, 3)          '受注明細№
                Dim P_LINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LINEORDER", SqlDbType.NVarChar, 2)        '貨物駅入線順
                Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar, 8)              'タンク車№
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar)       '積置可否フラグ
                Dim P_FIRSTRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@FIRSTRETURNFLG", SqlDbType.NVarChar) '先返し可否フラグ
                Dim P_AFTERRETURNFLG As SqlParameter = SQLcmd.Parameters.Add("@AFTERRETURNFLG", SqlDbType.NVarChar) '後返し可否フラグ
                Dim P_OTTRANSPORTFLG As SqlParameter = SQLcmd.Parameters.Add("@OTTRANSPORTFLG", SqlDbType.NVarChar) 'OT輸送可否フラグ
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar, 2)        '受注情報
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar, 10) '荷主名
                Dim P_OILCODE As SqlParameter = SQLcmd.Parameters.Add("@OILCODE", SqlDbType.NVarChar, 4)            '油種コード
                Dim P_OILNAME As SqlParameter = SQLcmd.Parameters.Add("@OILNAME", SqlDbType.NVarChar, 40)           '油種名
                Dim P_ORDERINGTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGTYPE", SqlDbType.NVarChar, 2)  '油種区分(受発注用)
                Dim P_ORDERINGOILNAME As SqlParameter = SQLcmd.Parameters.Add("@ORDERINGOILNAME", SqlDbType.NVarChar, 40)  '油種名(受発注用)
                Dim P_CARSNUMBER As SqlParameter = SQLcmd.Parameters.Add("@CARSNUMBER", SqlDbType.Int)              '車数
                Dim P_CARSAMOUNT As SqlParameter = SQLcmd.Parameters.Add("@CARSAMOUNT", SqlDbType.Int)              '数量
                Dim P_LINE As SqlParameter = SQLcmd.Parameters.Add("@LINE", SqlDbType.NVarChar, 2)                  '回線
                Dim P_FILLINGPOINT As SqlParameter = SQLcmd.Parameters.Add("@FILLINGPOINT", SqlDbType.NVarChar, 2)  '充填ポイント
                Dim P_LOADINGIRILINETRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNO", SqlDbType.NVarChar, 4)      '積込入線列車番号
                Dim P_LOADINGIRILINETRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINETRAINNAME", SqlDbType.NVarChar, 40) '積込入線列車番号名
                Dim P_LOADINGIRILINEORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGIRILINEORDER", SqlDbType.NVarChar, 2)          '積込入線順
                Dim P_LOADINGOUTLETTRAINNO As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNO", SqlDbType.NVarChar, 4)        '積込出線列車番号
                Dim P_LOADINGOUTLETTRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar, 40)   '積込出線列車番号名
                Dim P_LOADINGOUTLETORDER As SqlParameter = SQLcmd.Parameters.Add("@LOADINGOUTLETORDER", SqlDbType.NVarChar, 2)            '積込出線順
                Dim P_RESERVEDNO As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDNO", SqlDbType.NVarChar, 11)     '予約番号
                Dim P_OTSENDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@OTSENDCOUNT", SqlDbType.Int)            'OT発送日報送信回数
                Dim P_DLRESERVEDCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLRESERVEDCOUNT", SqlDbType.Int)    '出荷予約ダウンロード回数
                Dim P_DLTAKUSOUCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DLTAKUSOUCOUNT", SqlDbType.Int)      '託送状ダウンロード回数
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.Int)                        '売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.Int)                  '売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.Int)              '売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.Int)                    '支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.Int)              '支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.Int)          '支払合計金額
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)              '削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)               '登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar, 20)         '登録ユーザーID
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar, 20)     '登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)                 '更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)           '更新ユーザーID
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 20)       '更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)         '集信日時

                Dim JP_ORDERNO As SqlParameter = SQLcmdJnl.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 4)   '受注№
                Dim JP_DETAILNO As SqlParameter = SQLcmdJnl.Parameters.Add("@DETAILNO", SqlDbType.NVarChar, 3) '受注明細№

                Dim WW_DATENOW As DateTime = Date.Now
                For Each OIT0002row As DataRow In OIT0002EXLUPtbl.Select(Nothing, "ORDERNO, DETAILNO")

                    '★受注№が未設定の場合は次レコード
                    If OIT0002row("ORDERNO").ToString() = "" Then Continue For
                    If OIT0002row("LOADINGTRAINNO").ToString() = "" Then Continue For

                    P_ORDERNO.Value = OIT0002row("ORDERNO")                 '受注№
                    P_DETAILNO.Value = OIT0002row("DETAILNO")               '受注明細№

                    P_LINEORDER.Value = ""               '貨物駅入線順
                    'P_LINEORDER.Value = OIT0002row("LINECNT")               '貨物駅入線順
                    P_TANKNO.Value = OIT0002row("TRUCKNO")                  'タンク車№
                    P_STACKINGFLG.Value = "2"                               '積置可否フラグ
                    P_FIRSTRETURNFLG.Value = "2"                            '先返し可否フラグ
                    P_AFTERRETURNFLG.Value = "2"                            '後返し可否フラグ
                    P_OTTRANSPORTFLG.Value = "2"                            'OT輸送可否フラグ

                    P_ORDERINFO.Value = ""                                  '受注情報
                    P_SHIPPERSCODE.Value = OIT0002row("SHIPPERSCODE")       '荷主コード
                    P_SHIPPERSNAME.Value = OIT0002row("SHIPPERSNAME")       '荷主名

                    P_OILCODE.Value = OIT0002row("ORDEROILCODE")            '油種コード
                    P_OILNAME.Value = OIT0002row("ORDEROILNAME")            '油種名
                    P_ORDERINGTYPE.Value = OIT0002row("ORDERINGTYPE")       '油種区分(受発注用)
                    P_ORDERINGOILNAME.Value = OIT0002row("ORDERINGOILNAME") '油種名(受発注用)
                    P_CARSNUMBER.Value = 1                                  '車数
                    P_CARSAMOUNT.Value = 0                                  '数量

                    P_FILLINGPOINT.Value = OIT0002row("POSITION")           '充填ポイント
                    P_LINE.Value = OIT0002row("LINE")                       '回線
                    P_LOADINGIRILINETRAINNO.Value = OIT0002row("INLINETRAIN")       '積込入線列車番号
                    P_LOADINGIRILINETRAINNAME.Value = OIT0002row("INLINETRAINNAME") '積込入線列車番号名
                    P_LOADINGIRILINEORDER.Value = ""                                '積込入線順
                    P_LOADINGOUTLETTRAINNO.Value = OIT0002row("OUTLINETRAIN")       '積込出線列車番号
                    P_LOADINGOUTLETTRAINNAME.Value = OIT0002row("OUTLINETRAINNAME") '積込出線列車番号名
                    P_LOADINGOUTLETORDER.Value = ""                                 '積込出線順

                    ''貨物駅入線順を積込入線順に設定
                    'P_LOADINGIRILINEORDER.Value = OIT0002row("LINEORDER")
                    ''積込出線順に(明細数 - 積込入線順 + 1)設定
                    'P_LOADINGOUTLETORDER.Value = (OIT0002tbl.Rows.Count - Integer.Parse(OIT0002row("LINEORDER"))) + 1

                    P_RESERVEDNO.Value = ""                                 '予約番号
                    P_OTSENDCOUNT.Value = "0"                               'OT発送日報送信回数
                    P_DLRESERVEDCOUNT.Value = "0"                           '出荷予約ダウンロード回数
                    P_DLTAKUSOUCOUNT.Value = "0"                            '託送状ダウンロード回数

                    P_SALSE.Value = "0"                                     '売上金額
                    P_SALSETAX.Value = "0"                                  '売上消費税額
                    P_TOTALSALSE.Value = "0"                                '売上合計金額
                    P_PAYMENT.Value = "0"                                   '支払金額
                    P_PAYMENTTAX.Value = "0"                                '支払消費税額
                    P_TOTALPAYMENT.Value = "0"                              '支払合計金額
                    P_DELFLG.Value = "0"                                    '削除フラグ
                    P_INITYMD.Value = WW_DATENOW                            '登録年月日
                    P_INITUSER.Value = Master.USERID                        '登録ユーザーID
                    P_INITTERMID.Value = Master.USERTERMID                  '登録端末
                    P_UPDYMD.Value = WW_DATENOW                             '更新年月日
                    P_UPDUSER.Value = Master.USERID                         '更新ユーザーID
                    P_UPDTERMID.Value = Master.USERTERMID                   '更新端末
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    'OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    JP_ORDERNO.Value = OIT0002row("ORDERNO")                 '受注№
                    JP_DETAILNO.Value = OIT0002row("DETAILNO")               '受注明細№

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
                        CS0020JOURNAL.TABLENM = "OIT0002D"
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
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L UPDATE_INSERT_ORDERDETAIL", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateORDER(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002UPDtbl) Then
            OIT0002UPDtbl = New DataTable
        End If

        If OIT0002UPDtbl.Columns.Count <> 0 Then
            OIT0002UPDtbl.Columns.Clear()
        End If

        OIT0002UPDtbl.Clear()

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0002_ORDER" _
            & "    WHERE" _
            & "        ORDERNO          = @ORDERNO" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0002_ORDER" _
            & "    SET" _
            & "        OFFICECODE        = @OFFICECODE   , OFFICENAME     = @OFFICENAME" _
            & "        , TRAINNO         = @TRAINNO      , TRAINNAME      = @TRAINNAME" _
            & "        , ORDERTYPE       = @ORDERTYPE" _
            & "        , SHIPPERSCODE    = @SHIPPERSCODE , SHIPPERSNAME   = @SHIPPERSNAME" _
            & "        , BASECODE        = @BASECODE     , BASENAME       = @BASENAME" _
            & "        , CONSIGNEECODE   = @CONSIGNEECODE, CONSIGNEENAME  = @CONSIGNEENAME" _
            & "        , DEPSTATION      = @DEPSTATION   , DEPSTATIONNAME = @DEPSTATIONNAME" _
            & "        , ARRSTATION      = @ARRSTATION   , ARRSTATIONNAME = @ARRSTATIONNAME" _
            & "        , ORDERINFO       = @ORDERINFO    , STACKINGFLG    = @STACKINGFLG" _
            & "        , LODDATE         = @LODDATE      , DEPDATE        = @DEPDATE" _
            & "        , ARRDATE         = @ARRDATE      , ACCDATE        = @ACCDATE" _
            & "        , EMPARRDATE      = @EMPARRDATE" _
            & "        , UPDYMD          = @UPDYMD       , UPDUSER        = @UPDUSER" _
            & "        , UPDTERMID       = @UPDTERMID    , RECEIVEYMD     = @RECEIVEYMD" _
            & "    WHERE" _
            & "        ORDERNO          = @ORDERNO" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0002_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , TRAINNAME       , ORDERYMD            , OFFICECODE , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME    , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION      , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , ORDERSTATUS  , ORDERINFO " _
            & "        , EMPTYTURNFLG , STACKINGFLG     , USEPROPRIETYFLG , CONTACTFLG          , RESULTFLG  , DELIVERYFLG   , DELIVERYCOUNT" _
            & "        , LODDATE      , DEPDATE         , ARRDATE         , ACCDATE             , EMPARRDATE " _
            & "        , RTANK        , HTANK           , TTANK           , MTTANK " _
            & "        , KTANK        , K3TANK          , K5TANK          , K10TANK" _
            & "        , LTANK        , ATANK           , OTHER1OTANK     , OTHER2OTANK         , OTHER3OTANK" _
            & "        , OTHER4OTANK  , OTHER5OTANK     , OTHER6OTANK     , OTHER7OTANK         , OTHER8OTANK" _
            & "        , OTHER9OTANK  , OTHER10OTANK    , TOTALTANK" _
            & "        , RTANKCH      , HTANKCH         , TTANKCH         , MTTANKCH            , KTANKCH" _
            & "        , K3TANKCH     , K5TANKCH        , K10TANKCH       , LTANKCH             , ATANKCH" _
            & "        , OTHER1OTANKCH, OTHER2OTANKCH   , OTHER3OTANKCH   , OTHER4OTANKCH       , OTHER5OTANKCH" _
            & "        , OTHER6OTANKCH, OTHER7OTANKCH   , OTHER8OTANKCH   , OTHER9OTANKCH       , OTHER10OTANKCH" _
            & "        , TOTALTANKCH  , KEIJYOYMD       , SALSE           , SALSETAX            , TOTALSALSE" _
            & "        , PAYMENT      , PAYMENTTAX      , TOTALPAYMENT" _
            & "        , RECEIVECOUNT , OTSENDSTATUS    , RESERVEDSTATUS  , TAKUSOUSTATUS" _
            & "        , DELFLG       , INITYMD         , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID       , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @ORDERNO      , @TRAINNO      , @TRAINNAME      , @ORDERYMD      , @OFFICECODE, @OFFICENAME" _
            & "        , @ORDERTYPE    , @SHIPPERSCODE , @SHIPPERSNAME   , @BASECODE      , @BASENAME" _
            & "        , @CONSIGNEECODE, @CONSIGNEENAME, @DEPSTATION     , @DEPSTATIONNAME, @ARRSTATION, @ARRSTATIONNAME" _
            & "        , @ORDERSTATUS  , @ORDERINFO" _
            & "        , @EMPTYTURNFLG , @STACKINGFLG  , @USEPROPRIETYFLG, @CONTACTFLG    , @RESULTFLG , @DELIVERYFLG   , @DELIVERYCOUNT" _
            & "        , @LODDATE      , @DEPDATE      , @ARRDATE        , @ACCDATE       , @EMPARRDATE" _
            & "        , @RTANK        , @HTANK        , @TTANK          , @MTTANK" _
            & "        , @KTANK        , @K3TANK       , @K5TANK         , @K10TANK" _
            & "        , @LTANK        , @ATANK        , @OTHER1OTANK    , @OTHER2OTANK   , @OTHER3OTANK" _
            & "        , @OTHER4OTANK  , @OTHER5OTANK  , @OTHER6OTANK    , @OTHER7OTANK   , @OTHER8OTANK" _
            & "        , @OTHER9OTANK  , @OTHER10OTANK , @TOTALTANK" _
            & "        , @RTANKCH      , @HTANKCH      , @TTANKCH        , @MTTANKCH      , @KTANKCH" _
            & "        , @K3TANKCH     , @K5TANKCH     , @K10TANKCH      , @LTANKCH       , @ATANKCH" _
            & "        , @OTHER1OTANKCH, @OTHER2OTANKCH, @OTHER3OTANKCH  , @OTHER4OTANKCH , @OTHER5OTANKCH" _
            & "        , @OTHER6OTANKCH, @OTHER7OTANKCH, @OTHER8OTANKCH  , @OTHER9OTANKCH , @OTHER10OTANKCH" _
            & "        , @TOTALTANKCH  , @KEIJYOYMD    , @SALSE          , @SALSETAX      , @TOTALSALSE" _
            & "        , @PAYMENT      , @PAYMENTTAX   , @TOTALPAYMENT" _
            & "        , @RECEIVECOUNT , @OTSENDSTATUS , @RESERVEDSTATUS , @TAKUSOUSTATUS" _
            & "        , @DELFLG       , @INITYMD      , @INITUSER       , @INITTERMID" _
            & "        , @UPDYMD       , @UPDUSER      , @UPDTERMID      , @RECEIVEYMD) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , TRAINNO" _
            & "    , TRAINNAME" _
            & "    , ORDERYMD" _
            & "    , OFFICECODE" _
            & "    , OFFICENAME" _
            & "    , ORDERTYPE" _
            & "    , SHIPPERSCODE" _
            & "    , SHIPPERSNAME" _
            & "    , BASECODE" _
            & "    , BASENAME" _
            & "    , CONSIGNEECODE" _
            & "    , CONSIGNEENAME" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , ARRSTATION" _
            & "    , ARRSTATIONNAME" _
            & "    , ORDERSTATUS" _
            & "    , ORDERINFO" _
            & "    , EMPTYTURNFLG" _
            & "    , STACKINGFLG" _
            & "    , USEPROPRIETYFLG" _
            & "    , CONTACTFLG" _
            & "    , RESULTFLG" _
            & "    , DELIVERYFLG" _
            & "    , DELIVERYCOUNT" _
            & "    , LODDATE" _
            & "    , DEPDATE" _
            & "    , ARRDATE" _
            & "    , ACCDATE" _
            & "    , EMPARRDATE" _
            & "    , RTANK" _
            & "    , HTANK" _
            & "    , TTANK" _
            & "    , MTTANK" _
            & "    , KTANK" _
            & "    , K3TANK" _
            & "    , K5TANK" _
            & "    , K10TANK" _
            & "    , LTANK" _
            & "    , ATANK" _
            & "    , OTHER1OTANK" _
            & "    , OTHER2OTANK" _
            & "    , OTHER3OTANK" _
            & "    , OTHER4OTANK" _
            & "    , OTHER5OTANK" _
            & "    , OTHER6OTANK" _
            & "    , OTHER7OTANK" _
            & "    , OTHER8OTANK" _
            & "    , OTHER9OTANK" _
            & "    , OTHER10OTANK" _
            & "    , TOTALTANK" _
            & "    , RTANKCH" _
            & "    , HTANKCH" _
            & "    , TTANKCH" _
            & "    , MTTANKCH" _
            & "    , KTANKCH" _
            & "    , K3TANKCH" _
            & "    , K5TANKCH" _
            & "    , K10TANKCH" _
            & "    , LTANKCH" _
            & "    , ATANKCH" _
            & "    , OTHER1OTANKCH" _
            & "    , OTHER2OTANKCH" _
            & "    , OTHER3OTANKCH" _
            & "    , OTHER4OTANKCH" _
            & "    , OTHER5OTANKCH" _
            & "    , OTHER6OTANKCH" _
            & "    , OTHER7OTANKCH" _
            & "    , OTHER8OTANKCH" _
            & "    , OTHER9OTANKCH" _
            & "    , OTHER10OTANKCH" _
            & "    , TOTALTANKCH" _
            & "    , KEIJYOYMD" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , RECEIVECOUNT" _
            & "    , OTSENDSTATUS" _
            & "    , RESERVEDSTATUS" _
            & "    , TAKUSOUSTATUS" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0002_ORDER" _
            & " WHERE" _
            & "        ORDERNO = @ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11) '受注№
                Dim P_TRAINNO As SqlParameter = SQLcmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar, 4)  '本線列車
                Dim P_TRAINNAME As SqlParameter = SQLcmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar, 20) '本線列車名
                Dim P_ORDERYMD As SqlParameter = SQLcmd.Parameters.Add("@ORDERYMD", SqlDbType.Date)         '受注登録日
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim P_OFFICENAME As SqlParameter = SQLcmd.Parameters.Add("@OFFICENAME", SqlDbType.NVarChar, 20) '受注営業所名
                Dim P_ORDERTYPE As SqlParameter = SQLcmd.Parameters.Add("@ORDERTYPE", SqlDbType.NVarChar, 7)  '受注パターン
                Dim P_SHIPPERSCODE As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSCODE", SqlDbType.NVarChar, 10) '荷主コード
                Dim P_SHIPPERSNAME As SqlParameter = SQLcmd.Parameters.Add("@SHIPPERSNAME", SqlDbType.NVarChar, 40) '荷主名
                Dim P_BASECODE As SqlParameter = SQLcmd.Parameters.Add("@BASECODE", SqlDbType.NVarChar, 9)  '基地コード
                Dim P_BASENAME As SqlParameter = SQLcmd.Parameters.Add("@BASENAME", SqlDbType.NVarChar, 40) '基地名
                Dim P_CONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEECODE", SqlDbType.NVarChar, 10) '荷受人コード
                Dim P_CONSIGNEENAME As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEENAME", SqlDbType.NVarChar, 40) '荷受人名
                Dim P_DEPSTATION As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", SqlDbType.NVarChar, 7)  '発駅コード
                Dim P_DEPSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@DEPSTATIONNAME", SqlDbType.NVarChar, 40) '発駅名
                Dim P_ARRSTATION As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", SqlDbType.NVarChar, 7)  '着駅コード
                Dim P_ARRSTATIONNAME As SqlParameter = SQLcmd.Parameters.Add("@ARRSTATIONNAME", SqlDbType.NVarChar, 40) '着駅名
                Dim P_ORDERSTATUS As SqlParameter = SQLcmd.Parameters.Add("@ORDERSTATUS", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim P_ORDERINFO As SqlParameter = SQLcmd.Parameters.Add("@ORDERINFO", SqlDbType.NVarChar, 2)  '受注情報
                Dim P_EMPTYTURNFLG As SqlParameter = SQLcmd.Parameters.Add("@EMPTYTURNFLG", SqlDbType.NVarChar, 1)  '空回日報可否フラグ
                Dim P_STACKINGFLG As SqlParameter = SQLcmd.Parameters.Add("@STACKINGFLG", SqlDbType.NVarChar, 1)  '積置可否フラグ
                Dim P_USEPROPRIETYFLG As SqlParameter = SQLcmd.Parameters.Add("@USEPROPRIETYFLG", SqlDbType.NVarChar, 1)  '利用可否フラグ
                Dim P_CONTACTFLG As SqlParameter = SQLcmd.Parameters.Add("@CONTACTFLG", SqlDbType.NVarChar, 1)  '手配連絡フラグ
                Dim P_RESULTFLG As SqlParameter = SQLcmd.Parameters.Add("@RESULTFLG", SqlDbType.NVarChar, 1)    '結果受理フラグ
                Dim P_DELIVERYFLG As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYFLG", SqlDbType.NVarChar, 1) '託送指示フラグ
                Dim P_DELIVERYCOUNT As SqlParameter = SQLcmd.Parameters.Add("@DELIVERYCOUNT", SqlDbType.Int)     '託送指示送信回数
                Dim P_LODDATE As SqlParameter = SQLcmd.Parameters.Add("@LODDATE", SqlDbType.Date)               '積込日（予定）
                Dim P_DEPDATE As SqlParameter = SQLcmd.Parameters.Add("@DEPDATE", SqlDbType.Date)               '発日（予定）
                Dim P_ARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ARRDATE", SqlDbType.Date)               '積車着日（予定）
                Dim P_ACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACCDATE", SqlDbType.Date)               '受入日（予定）
                Dim P_EMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@EMPARRDATE", SqlDbType.Date)         '空車着日（予定）
                Dim P_RTANK As SqlParameter = SQLcmd.Parameters.Add("@RTANK", SqlDbType.Int)                    '車数（レギュラー）
                Dim P_HTANK As SqlParameter = SQLcmd.Parameters.Add("@HTANK", SqlDbType.Int)                    '車数（ハイオク）
                Dim P_TTANK As SqlParameter = SQLcmd.Parameters.Add("@TTANK", SqlDbType.Int)                    '車数（灯油）
                Dim P_MTTANK As SqlParameter = SQLcmd.Parameters.Add("@MTTANK", SqlDbType.Int)                  '車数（未添加灯油）
                Dim P_KTANK As SqlParameter = SQLcmd.Parameters.Add("@KTANK", SqlDbType.Int)                    '車数（軽油）
                Dim P_K3TANK As SqlParameter = SQLcmd.Parameters.Add("@K3TANK", SqlDbType.Int)                  '車数（３号軽油）
                Dim P_K5TANK As SqlParameter = SQLcmd.Parameters.Add("@K5TANK", SqlDbType.Int)                  '車数（５号軽油）
                Dim P_K10TANK As SqlParameter = SQLcmd.Parameters.Add("@K10TANK", SqlDbType.Int)                '車数（１０号軽油）
                Dim P_LTANK As SqlParameter = SQLcmd.Parameters.Add("@LTANK", SqlDbType.Int)                    '車数（LSA）
                Dim P_ATANK As SqlParameter = SQLcmd.Parameters.Add("@ATANK", SqlDbType.Int)                    '車数（A重油）
                Dim P_OTHER1OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANK", SqlDbType.Int)        '車数（その他１）
                Dim P_OTHER2OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANK", SqlDbType.Int)        '車数（その他２）
                Dim P_OTHER3OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANK", SqlDbType.Int)        '車数（その他３）
                Dim P_OTHER4OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANK", SqlDbType.Int)        '車数（その他４）
                Dim P_OTHER5OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANK", SqlDbType.Int)        '車数（その他５）
                Dim P_OTHER6OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANK", SqlDbType.Int)        '車数（その他６）
                Dim P_OTHER7OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANK", SqlDbType.Int)        '車数（その他７）
                Dim P_OTHER8OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANK", SqlDbType.Int)        '車数（その他８）
                Dim P_OTHER9OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANK", SqlDbType.Int)        '車数（その他９）
                Dim P_OTHER10OTANK As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANK", SqlDbType.Int)      '車数（その他１０）
                Dim P_TOTALTANK As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANK", SqlDbType.Int)            '合計車数
                Dim P_RTANKCH As SqlParameter = SQLcmd.Parameters.Add("@RTANKCH", SqlDbType.Int)                '変更後_車数（レギュラー）
                Dim P_HTANKCH As SqlParameter = SQLcmd.Parameters.Add("@HTANKCH", SqlDbType.Int)                '変更後_車数（ハイオク）
                Dim P_TTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TTANKCH", SqlDbType.Int)                '変更後_車数（灯油）
                Dim P_MTTANKCH As SqlParameter = SQLcmd.Parameters.Add("@MTTANKCH", SqlDbType.Int)              '変更後_車数（未添加灯油）
                Dim P_KTANKCH As SqlParameter = SQLcmd.Parameters.Add("@KTANKCH", SqlDbType.Int)                '変更後_車数（軽油）
                Dim P_K3TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K3TANKCH", SqlDbType.Int)              '変更後_車数（３号軽油）
                Dim P_K5TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K5TANKCH", SqlDbType.Int)              '変更後_車数（５号軽油）
                Dim P_K10TANKCH As SqlParameter = SQLcmd.Parameters.Add("@K10TANKCH", SqlDbType.Int)            '変更後_車数（１０号軽油）
                Dim P_LTANKCH As SqlParameter = SQLcmd.Parameters.Add("@LTANKCH", SqlDbType.Int)                '変更後_車数（LSA）
                Dim P_ATANKCH As SqlParameter = SQLcmd.Parameters.Add("@ATANKCH", SqlDbType.Int)                '変更後_車数（A重油）
                Dim P_OTHER1OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER1OTANKCH", SqlDbType.Int)    '変更後_車数（その他１）
                Dim P_OTHER2OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER2OTANKCH", SqlDbType.Int)    '変更後_車数（その他２）
                Dim P_OTHER3OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER3OTANKCH", SqlDbType.Int)    '変更後_車数（その他３）
                Dim P_OTHER4OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER4OTANKCH", SqlDbType.Int)    '変更後_車数（その他４）
                Dim P_OTHER5OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER5OTANKCH", SqlDbType.Int)    '変更後_車数（その他５）
                Dim P_OTHER6OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER6OTANKCH", SqlDbType.Int)    '変更後_車数（その他６）
                Dim P_OTHER7OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER7OTANKCH", SqlDbType.Int)    '変更後_車数（その他７）
                Dim P_OTHER8OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER8OTANKCH", SqlDbType.Int)    '変更後_車数（その他８）
                Dim P_OTHER9OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER9OTANKCH", SqlDbType.Int)    '変更後_車数（その他９）
                Dim P_OTHER10OTANKCH As SqlParameter = SQLcmd.Parameters.Add("@OTHER10OTANKCH", SqlDbType.Int)  '変更後_車数（その他１０）
                Dim P_TOTALTANKCH As SqlParameter = SQLcmd.Parameters.Add("@TOTALTANKCH", SqlDbType.Int)        '変更後_合計車数
                Dim P_KEIJYOYMD As SqlParameter = SQLcmd.Parameters.Add("@KEIJYOYMD", SqlDbType.Date)           '計上日
                Dim P_SALSE As SqlParameter = SQLcmd.Parameters.Add("@SALSE", SqlDbType.Int)                    '売上金額
                Dim P_SALSETAX As SqlParameter = SQLcmd.Parameters.Add("@SALSETAX", SqlDbType.Int)              '売上消費税額
                Dim P_TOTALSALSE As SqlParameter = SQLcmd.Parameters.Add("@TOTALSALSE", SqlDbType.Int)          '売上合計金額
                Dim P_PAYMENT As SqlParameter = SQLcmd.Parameters.Add("@PAYMENT", SqlDbType.Int)                '支払金額
                Dim P_PAYMENTTAX As SqlParameter = SQLcmd.Parameters.Add("@PAYMENTTAX", SqlDbType.Int)          '支払消費税額
                Dim P_TOTALPAYMENT As SqlParameter = SQLcmd.Parameters.Add("@TOTALPAYMENT", SqlDbType.Int)      '支払合計金額
                Dim P_RECEIVECOUNT As SqlParameter = SQLcmd.Parameters.Add("@RECEIVECOUNT", SqlDbType.Int)             'OT空回日報受信回数
                Dim P_OTSENDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@OTSENDSTATUS", SqlDbType.NVarChar, 1)     'OT発送日報送信状況
                Dim P_RESERVEDSTATUS As SqlParameter = SQLcmd.Parameters.Add("@RESERVEDSTATUS", SqlDbType.NVarChar, 1) '出荷予約ダウンロード状況
                Dim P_TAKUSOUSTATUS As SqlParameter = SQLcmd.Parameters.Add("@TAKUSOUSTATUS", SqlDbType.NVarChar, 1)   '託送状ダウンロード状況
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)          '削除フラグ
                Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)           '登録年月日
                Dim P_INITUSER As SqlParameter = SQLcmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar, 20)     '登録ユーザーID
                Dim P_INITTERMID As SqlParameter = SQLcmd.Parameters.Add("@INITTERMID", SqlDbType.NVarChar, 20) '登録端末
                Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)             '更新年月日
                Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)       '更新ユーザーID
                Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 20)   '更新端末
                Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)     '集信日時

                Dim JP_ORDERNO As SqlParameter = SQLcmdJnl.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 11)   '受注№

                Dim WW_DATENOW As DateTime = Date.Now
                Dim iresult As Integer
                Dim strOilCnt() As String
                Dim strOrderNo As String = ""

                'For Each OIT0002row As DataRow In OIT0002EXLUPtbl.Rows
                For Each OIT0002row As DataRow In OIT0002EXLUPtbl.Select(Nothing, "ORDERNO")

                    '★受注№が未設定の場合は次レコード
                    If OIT0002row("ORDERNO").ToString() = "" Then Continue For
                    If OIT0002row("LOADINGTRAINNO").ToString() = "" Then Continue For

                    'DB更新
                    P_ORDERNO.Value = OIT0002row("ORDERNO")                       '受注№
                    P_TRAINNO.Value = OIT0002row("LOADINGTRAINNO")                '本線列車
                    P_TRAINNAME.Value = OIT0002row("LOADINGTRAINNAME")            '本線列車名
                    P_ORDERYMD.Value = WW_DATENOW                                 '受注登録日
                    P_OFFICECODE.Value = OIT0002row("OFFICECODE")                 '受注営業所コード
                    P_OFFICENAME.Value = OIT0002row("OFFICENAME")                 '受注営業所名
                    P_ORDERTYPE.Value = OIT0002row("PATTERNCODE")                 '受注パターン
                    P_SHIPPERSCODE.Value = OIT0002row("SHIPPERSCODE")             '荷主コード
                    P_SHIPPERSNAME.Value = OIT0002row("SHIPPERSNAME")             '荷主名
                    P_BASECODE.Value = OIT0002row("BASECODE")                     '基地コード
                    P_BASENAME.Value = OIT0002row("BASENAME")                     '基地名
                    P_CONSIGNEECODE.Value = OIT0002row("CONSIGNEECODE")           '荷受人コード
                    P_CONSIGNEENAME.Value = OIT0002row("CONSIGNEENAME")           '荷受人名
                    P_DEPSTATION.Value = OIT0002row("RETSTATION")                 '発駅コード
                    P_DEPSTATIONNAME.Value = OIT0002row("ARRSTATIONNAME")         '発駅名
                    P_ARRSTATION.Value = OIT0002row("DEPSTATION")                 '着駅コード
                    P_ARRSTATIONNAME.Value = OIT0002row("LOADARRSTATION")         '着駅名

                    P_ORDERSTATUS.Value = BaseDllConst.CONST_ORDERSTATUS_100      '受注進行ステータス
                    P_ORDERINFO.Value = ""                                        '受注情報
                    P_EMPTYTURNFLG.Value = "3"                                    '空回日報可否フラグ(3:作成(貨車連結表から作成))

                    '〇 積込日 < 発日 の場合 
                    Try
                        iresult = Date.Parse(OIT0002row("LOADINGLODDATE")).CompareTo(Date.Parse(OIT0002row("LOADINGDEPDATE")))
                        '例) iresult = dt1.Date.CompareTo(dt2.Date)
                        '    iresultの意味
                        '     0 : dt1とdt2は同じ日
                        '    -1 : dt1はdt2より前の日
                        '     1 : dt1はdt2より後の日
                        If iresult = -1 Then
                            P_STACKINGFLG.Value = "1"                         '積置可否フラグ(1:積置あり)
                        Else
                            P_STACKINGFLG.Value = "2"                         '積置可否フラグ(2:積置なし)
                        End If
                    Catch ex As Exception
                        P_STACKINGFLG.Value = "2"                         '積置可否フラグ(2:積置なし)
                    End Try

                    P_USEPROPRIETYFLG.Value = "1"                         '利用可否フラグ(1:利用可能)
                    P_CONTACTFLG.Value = "0"                              '手配連絡フラグ(0:未連絡)
                    P_RESULTFLG.Value = "0"                               '結果受理フラグ(0:未受理)
                    P_DELIVERYFLG.Value = "0"                             '託送指示フラグ(0:未手配, 1:手配)
                    P_DELIVERYCOUNT.Value = "0"                           '託送指示送信回数
                    '積込日（予定）
                    If OIT0002row("LOADINGLODDATE") <> "" Then
                        P_LODDATE.Value = OIT0002row("LOADINGLODDATE")
                    Else
                        P_LODDATE.Value = DBNull.Value
                    End If
                    '発日（予定）
                    If OIT0002row("LOADINGDEPDATE") <> "" Then
                        P_DEPDATE.Value = OIT0002row("LOADINGDEPDATE")
                    Else
                        P_DEPDATE.Value = DBNull.Value
                    End If
                    '積車着日（予定）
                    If OIT0002row("LOADINGARRDATE").ToString() <> "" Then
                        P_ARRDATE.Value = OIT0002row("LOADINGARRDATE")
                    Else
                        P_ARRDATE.Value = DBNull.Value
                    End If
                    '受入日（予定）
                    If OIT0002row("LOADINGACCDATE").ToString() <> "" Then
                        P_ACCDATE.Value = OIT0002row("LOADINGACCDATE")
                    Else
                        P_ACCDATE.Value = DBNull.Value
                    End If
                    '空車着日（予定）
                    If OIT0002row("LOADINGEMPARRDATE").ToString() <> "" Then
                        P_EMPARRDATE.Value = OIT0002row("LOADINGEMPARRDATE")
                    Else
                        P_EMPARRDATE.Value = DBNull.Value
                    End If

                    '★受注登録用油種数カウント用
                    strOilCnt = {"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0"}
                    '油種別タンク車数、積込数量データ取得
                    WW_OILTANKCntGet(SQLcon, P_ORDERNO.Value, strOilCnt)

                    P_HTANK.Value = strOilCnt(1)                        '車数（ハイオク）
                    P_RTANK.Value = strOilCnt(2)                        '車数（レギュラー）
                    P_TTANK.Value = strOilCnt(3)                        '車数（灯油）
                    P_MTTANK.Value = strOilCnt(4)                       '車数（未添加灯油）
                    P_KTANK.Value = strOilCnt(5)                        '車数（軽油）
                    P_K3TANK.Value = strOilCnt(6)                       '車数（３号軽油）
                    P_K5TANK.Value = strOilCnt(7)                       '車数（５号軽油）
                    P_K10TANK.Value = strOilCnt(8)                      '車数（１０号軽油）
                    P_LTANK.Value = strOilCnt(9)                        '車数（LSA）
                    P_ATANK.Value = strOilCnt(10)                       '車数（A重油）

                    P_OTHER1OTANK.Value = 0                             '車数（その他１）
                    P_OTHER2OTANK.Value = 0                             '車数（その他２）
                    P_OTHER3OTANK.Value = 0                             '車数（その他３）
                    P_OTHER4OTANK.Value = 0                             '車数（その他４）
                    P_OTHER5OTANK.Value = 0                             '車数（その他５）
                    P_OTHER6OTANK.Value = 0                             '車数（その他６）
                    P_OTHER7OTANK.Value = 0                             '車数（その他７）
                    P_OTHER8OTANK.Value = 0                             '車数（その他８）
                    P_OTHER9OTANK.Value = 0                             '車数（その他９）
                    P_OTHER10OTANK.Value = 0                            '車数（その他１０）
                    P_TOTALTANK.Value = strOilCnt(0)                    '合計車数

                    P_HTANKCH.Value = strOilCnt(1)                      '変更後_車数（ハイオク）
                    P_RTANKCH.Value = strOilCnt(2)                      '変更後_車数（レギュラー）
                    P_TTANKCH.Value = strOilCnt(3)                      '変更後_車数（灯油）
                    P_MTTANKCH.Value = strOilCnt(4)                     '変更後_車数（未添加灯油）
                    P_KTANKCH.Value = strOilCnt(5)                      '変更後_車数（軽油）
                    P_K3TANKCH.Value = strOilCnt(6)                     '変更後_車数（３号軽油）
                    P_K5TANKCH.Value = strOilCnt(7)                     '変更後_車数（５号軽油）
                    P_K10TANKCH.Value = strOilCnt(8)                    '変更後_車数（１０号軽油）
                    P_LTANKCH.Value = strOilCnt(9)                      '変更後_車数（LSA）
                    P_ATANKCH.Value = strOilCnt(10)                     '変更後_車数（A重油）
                    P_OTHER1OTANKCH.Value = 0                           '変更後_車数（その他１）
                    P_OTHER2OTANKCH.Value = 0                           '変更後_車数（その他２）
                    P_OTHER3OTANKCH.Value = 0                           '変更後_車数（その他３）
                    P_OTHER4OTANKCH.Value = 0                           '変更後_車数（その他４）
                    P_OTHER5OTANKCH.Value = 0                           '変更後_車数（その他５）
                    P_OTHER6OTANKCH.Value = 0                           '変更後_車数（その他６）
                    P_OTHER7OTANKCH.Value = 0                           '変更後_車数（その他７）
                    P_OTHER8OTANKCH.Value = 0                           '変更後_車数（その他８）
                    P_OTHER9OTANKCH.Value = 0                           '変更後_車数（その他９）
                    P_OTHER10OTANKCH.Value = 0                          '変更後_車数（その他１０）
                    P_TOTALTANKCH.Value = strOilCnt(0)                  '変更後_合計車数

                    P_KEIJYOYMD.Value = DBNull.Value                    '計上日
                    P_SALSE.Value = 0                                   '売上金額
                    P_SALSETAX.Value = 0                                '売上消費税額
                    P_TOTALSALSE.Value = 0                              '売上合計金額
                    P_PAYMENT.Value = 0                                 '支払金額
                    P_PAYMENTTAX.Value = 0                              '支払消費税額
                    P_TOTALPAYMENT.Value = 0                            '支払合計金額

                    P_RECEIVECOUNT.Value = 0                            'OT空回日報受信回数
                    P_OTSENDSTATUS.Value = "0"                          'OT発送日報送信状況
                    P_RESERVEDSTATUS.Value = "0"                        '出荷予約ダウンロード状況
                    P_TAKUSOUSTATUS.Value = "0"                         '託送状ダウンロード状況

                    P_DELFLG.Value = "0"                                '削除フラグ
                    P_INITYMD.Value = WW_DATENOW                        '登録年月日
                    P_INITUSER.Value = Master.USERID                    '登録ユーザーID
                    P_INITTERMID.Value = Master.USERTERMID              '登録端末
                    P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                    P_UPDUSER.Value = Master.USERID                     '更新ユーザーID
                    P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                    P_RECEIVEYMD.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    'OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JP_ORDERNO.Value = OIT0002row("ORDERNO")

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
                        CS0020JOURNAL.TABLENM = "OIT0002D"
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

                    If strOrderNo <> OIT0002row("ORDERNO") Then
                        '受注履歴テーブル追加処理
                        WW_InsertOrderHistory(SQLcon, OIT0002row("ORDERNO"))
                    End If
                    strOrderNo = OIT0002row("ORDERNO")
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L UPDATE_INSERT_ORDER", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L UPDATE_INSERT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    Private Sub WW_InsertOrderHistory(ByVal SQLcon As SqlConnection,
                                      ByVal I_ORDERNO As String)
        Dim WW_GetHistoryNo() As String = {""}
        WW_FixvalueMasterSearch("", "NEWHISTORYNOGET", "", WW_GetHistoryNo)

        '◯受注履歴テーブル格納用
        If IsNothing(OIT0002His1tbl) Then
            OIT0002His1tbl = New DataTable
        End If

        If OIT0002His1tbl.Columns.Count <> 0 Then
            OIT0002His1tbl.Columns.Clear()
        End If
        OIT0002His1tbl.Clear()

        '◯受注明細履歴テーブル格納用
        If IsNothing(OIT0002His2tbl) Then
            OIT0002His2tbl = New DataTable
        End If

        If OIT0002His2tbl.Columns.Count <> 0 Then
            OIT0002His2tbl.Columns.Clear()
        End If
        OIT0002His2tbl.Clear()

        '○ 受注TBL検索SQL
        Dim SQLOrderStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0002.*" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & String.Format(" WHERE OIT0002.ORDERNO = '{0}'", I_ORDERNO)

        '○ 受注明細TBL検索SQL
        Dim SQLOrderDetailStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0003.*" _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & String.Format(" WHERE OIT0003.ORDERNO = '{0}'", I_ORDERNO)

        Try
            Using SQLcmd As New SqlCommand(SQLOrderStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002His1tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002His1tbl.Load(SQLdr)
                End Using
            End Using

            Using SQLcmd As New SqlCommand(SQLOrderDetailStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002His2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002His2tbl.Load(SQLdr)
                End Using
            End Using

            Using tran = SQLcon.BeginTransaction
                '■受注履歴テーブル
                EntryHistory.InsertOrderHistory(SQLcon, tran, OIT0002His1tbl.Rows(0))

                '■受注明細履歴テーブル
                For Each OIT0001His2rowtbl In OIT0002His2tbl.Rows
                    EntryHistory.InsertOrderDetailHistory(SQLcon, tran, OIT0001His2rowtbl)
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L ORDERHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L ORDERHISTORY"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 油種別タンク車数、積込数量データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OILTANKCntGet(ByVal SQLcon As SqlConnection, ByVal OrderNo As String, ByRef OilCnt() As String)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT DISTINCT " _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , ''                                                 AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0003.ORDERNO), '')                 AS ORDERNO" _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P10 THEN 1 ELSE 0 END) " _
            & "    OVER(Partition BY OIT0003.ORDERNO)                AS HTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P11 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS RTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P12 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P13 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS MTTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P14 OR OIT0003.OILCODE = @P15 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS KTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P16 OR OIT0003.OILCODE = @P17 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K3TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P18 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K5TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P19 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K10TANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P20 OR OIT0003.OILCODE = @P21 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS LTANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P22 THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS ATANK " _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' THEN 1 ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TOTAL " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P10 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS HTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P11 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS RTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P12 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P13 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS MTTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P14 OR OIT0003.OILCODE = @P15 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS KTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P16 OR OIT0003.OILCODE = @P17 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K3TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P18 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K5TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P19 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS K10TANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P20 OR OIT0003.OILCODE = @P21 THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS LTANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE = @P22 THEN ISNULL(OIT0003.CARSAMOUNT,0)ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS ATANKCNT " _
            & " , SUM(CASE WHEN OIT0003.OILCODE <> '' THEN ISNULL(OIT0003.CARSAMOUNT,0) ELSE 0 END) " _
            & "    OVER (PARTITION BY OIT0003.ORDERNO)               AS TOTALCNT " _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & "  LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "  OIT0003.TANKNO = OIM0005.TANKNUMBER " _
            & " WHERE OIT0003.ORDERNO = @P01" _
            & "   AND OIT0003.DELFLG <> @P02"

        'SQLStr &=
        '      " ORDER BY" _
        '    & "    OIT0003.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                PARA01.Value = OrderNo
                PARA02.Value = C_DELETE_FLG.DELETE

                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4) '油種(ハイオク)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 4) '油種(レギュラー)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 4) '油種(灯油)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 4) '油種(未添加灯油)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4) '油種(５号軽油)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4) '油種(１０号軽油)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 4) '油種(Ａ重油)
                PARA10.Value = BaseDllConst.CONST_HTank
                PARA11.Value = BaseDllConst.CONST_RTank
                PARA12.Value = BaseDllConst.CONST_TTank
                PARA13.Value = BaseDllConst.CONST_MTTank
                PARA14.Value = BaseDllConst.CONST_KTank1
                PARA15.Value = BaseDllConst.CONST_KTank2
                PARA16.Value = BaseDllConst.CONST_K3Tank1
                PARA17.Value = BaseDllConst.CONST_K3Tank2
                PARA18.Value = BaseDllConst.CONST_K5Tank
                PARA19.Value = BaseDllConst.CONST_K10Tank
                PARA20.Value = BaseDllConst.CONST_LTank1
                PARA21.Value = BaseDllConst.CONST_LTank2
                PARA22.Value = BaseDllConst.CONST_ATank

                '■　初期化
                '〇 油種別タンク車数(車)
                OilCnt(0) = "0"             'タンク車数合計
                OilCnt(1) = "0"             '油種(ハイオク)
                OilCnt(2) = "0"             '油種(レギュラー)
                OilCnt(3) = "0"             '油種(灯油)
                OilCnt(4) = "0"             '油種(未添加灯油)
                OilCnt(5) = "0"             '油種(軽油)
                OilCnt(6) = "0"             '油種(３号軽油)
                OilCnt(7) = "0"             '油種(５号軽油)
                OilCnt(8) = "0"             '油種(１０号軽油)
                OilCnt(9) = "0"             '油種(ＬＳＡ)
                OilCnt(10) = "0"            '油種(Ａ重油)
                ''〇 積込数量(kl)
                'Me.TxtHTank_c2.Text = "0"
                'Me.TxtRTank_c2.Text = "0"
                'Me.TxtTTank_c2.Text = "0"
                'Me.TxtMTTank_c2.Text = "0"
                'Me.TxtKTank_c2.Text = "0"
                'Me.TxtK3Tank_c2.Text = "0"
                'Me.TxtK5Tank_c2.Text = "0"
                'Me.TxtK10Tank_c2.Text = "0"
                'Me.TxtLTank_c2.Text = "0"
                'Me.TxtATank_c2.Text = "0"
                'Me.TxtTotalCnt_c2.Text = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    i += 1
                    OIT0002WKrow("LINECNT") = i        'LINECNT

                    '[ヘッダー]
                    '〇 油種別タンク車数(車)
                    OilCnt(0) = OIT0002WKrow("TOTAL")             'タンク車数合計
                    OilCnt(1) = OIT0002WKrow("HTANK")             '油種(ハイオク)
                    OilCnt(2) = OIT0002WKrow("RTANK")             '油種(レギュラー)
                    OilCnt(3) = OIT0002WKrow("TTANK")             '油種(灯油)
                    OilCnt(4) = OIT0002WKrow("MTTANK")            '油種(未添加灯油)
                    OilCnt(5) = OIT0002WKrow("KTANK")             '油種(軽油)
                    OilCnt(6) = OIT0002WKrow("K3TANK")            '油種(３号軽油)
                    OilCnt(7) = OIT0002WKrow("K5TANK")            '油種(５号軽油)
                    OilCnt(8) = OIT0002WKrow("K10TANK")           '油種(１０号軽油)
                    OilCnt(9) = OIT0002WKrow("LTANK")             '油種(ＬＳＡ)
                    OilCnt(10) = OIT0002WKrow("ATANK")            '油種(Ａ重油)

                    ''〇 積込数量(kl)
                    'Me.TxtHTank_c2.Text = OIT0002WKrow("HTANKCNT")
                    'Me.TxtRTank_c2.Text = OIT0002WKrow("RTANKCNT")
                    'Me.TxtTTank_c2.Text = OIT0002WKrow("TTANKCNT")
                    'Me.TxtMTTank_c2.Text = OIT0002WKrow("MTTANKCNT")
                    'Me.TxtKTank_c2.Text = OIT0002WKrow("KTANKCNT")
                    'Me.TxtK3Tank_c2.Text = OIT0002WKrow("K3TANKCNT")
                    'Me.TxtK5Tank_c2.Text = OIT0002WKrow("K5TANKCNT")
                    'Me.TxtK10Tank_c2.Text = OIT0002WKrow("K10TANKCNT")
                    'Me.TxtLTank_c2.Text = OIT0002WKrow("LTANKCNT")
                    'Me.TxtATank_c2.Text = OIT0002WKrow("ATANKCNT")
                    'Me.TxtTotalCnt_c2.Text = OIT0002WKrow("TOTALCNT")

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    '''' <summary>
    '''' ファイルアップロード時処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub WF_FILEUPLOAD()

    '    '○ エラーレポート準備
    '    rightview.SetErrorReport("")

    '    '○ UPLOAD XLSデータ取得
    '    CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
    '    CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
    '    CS0023XLSUPLOAD.CS0023XLSUPLOAD()
    '    If isNormal(CS0023XLSUPLOAD.ERR) Then
    '        If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
    '            Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
    '            Exit Sub
    '        End If
    '    Else
    '        Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
    '        Exit Sub
    '    End If

    '    '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
    '    Dim WW_COLUMNS As New List(Of String)
    '    For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
    '        WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
    '    Next

    '    Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
    '    For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
    '        CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

    '        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
    '            If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
    '                CS0023XLSTBLrow.Item(XLSTBLcol) = ""
    '            End If
    '        Next

    '        XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
    '    Next

    '    '○ XLSUPLOAD明細⇒INPtbl
    '    Master.CreateEmptyTable(OIT0002INPtbl)

    '    For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
    '        Dim OIT0002INProw As DataRow = OIT0002INPtbl.NewRow

    '        '○ 初期クリア
    '        For Each OIT0002INPcol As DataColumn In OIT0002INPtbl.Columns
    '            If IsDBNull(OIT0002INProw.Item(OIT0002INPcol)) OrElse IsNothing(OIT0002INProw.Item(OIT0002INPcol)) Then
    '                Select Case OIT0002INPcol.ColumnName
    '                    Case "LINECNT"
    '                        OIT0002INProw.Item(OIT0002INPcol) = 0
    '                    Case "OPERATION"
    '                        OIT0002INProw.Item(OIT0002INPcol) = C_LIST_OPERATION_CODE.NODATA
    '                    Case "UPDTIMSTP"
    '                        OIT0002INProw.Item(OIT0002INPcol) = 0
    '                    Case "SELECT"
    '                        OIT0002INProw.Item(OIT0002INPcol) = 1
    '                    Case "HIDDEN"
    '                        OIT0002INProw.Item(OIT0002INPcol) = 0
    '                    Case Else
    '                        OIT0002INProw.Item(OIT0002INPcol) = ""
    '                End Select
    '            End If
    '        Next

    '        ''○ 変更元情報をデフォルト設定
    '        If WW_COLUMNS.IndexOf("RETSTATION") >= 0 AndAlso
    '            WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
    '            For Each OIT0002row As DataRow In OIT0002tbl.Rows
    '                If XLSTBLrow("LINKNO") = OIT0002row("LINKNO") AndAlso
    '                    XLSTBLrow("LINKDETAILNO") = OIT0002row("LINKDETAILNO") AndAlso
    '                    XLSTBLrow("STATUS") = OIT0002row("STATUS") AndAlso
    '                    XLSTBLrow("INFO") = OIT0002row("INFO") AndAlso
    '                    XLSTBLrow("PREORDERNO") = OIT0002row("PREORDERNO") AndAlso
    '                    XLSTBLrow("OFFICECODE") = OIT0002row("OFFICECODE") AndAlso
    '                    XLSTBLrow("DEPSTATION") = OIT0002row("DEPSTATION") AndAlso
    '                    XLSTBLrow("DEPSTATIONNAME") = OIT0002row("DEPSTATIONNAME") AndAlso
    '                    XLSTBLrow("RETSTATION") = OIT0002row("RETSTATION") AndAlso
    '                    XLSTBLrow("RETSTATIONNAME") = OIT0002row("RETSTATIONNAME") AndAlso
    '                    XLSTBLrow("EMPARRDATE") = OIT0002row("EMPARRDATE") AndAlso
    '                    XLSTBLrow("ACTUALEMPARRDATE") = OIT0002row("ACTUALEMPARRDATE") AndAlso
    '                    XLSTBLrow("LINETRAINNO") = OIT0002row("LINETRAINNO") AndAlso
    '                    XLSTBLrow("LINEORDER") = OIT0002row("LINEORDER") AndAlso
    '                    XLSTBLrow("TANKNUMBER") = OIT0002row("TANKNUMBER") AndAlso
    '                    XLSTBLrow("PREOILCODE") = OIT0002row("PREOILCODE") Then
    '                    OIT0002INProw.ItemArray = OIT0002row.ItemArray
    '                    Exit For
    '                End If
    '            Next
    '        End If

    '        '○ 項目セット
    '        '貨車連結順序表№
    '        If WW_COLUMNS.IndexOf("LINKNO") >= 0 Then
    '            OIT0002INProw("LINKNO") = XLSTBLrow("LINKNO")
    '        End If

    '        '貨車連結順序表明細№
    '        If WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
    '            OIT0002INProw("LINKDETAILNO") = XLSTBLrow("LINKDETAILNO")
    '        End If

    '        'ステータス
    '        If WW_COLUMNS.IndexOf("STATUS") >= 0 Then
    '            OIT0002INProw("STATUS") = XLSTBLrow("STATUS")
    '        End If

    '        '情報
    '        If WW_COLUMNS.IndexOf("INFO") >= 0 Then
    '            OIT0002INProw("INFO") = XLSTBLrow("INFO")
    '        End If

    '        '前回オーダー№
    '        If WW_COLUMNS.IndexOf("PREORDERNO") >= 0 Then
    '            OIT0002INProw("PREORDERNO") = XLSTBLrow("PREORDERNO")
    '        End If

    '        '本線列車
    '        If WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
    '            OIT0002INProw("TRAINNO") = XLSTBLrow("TRAINNO")
    '        End If

    '        '登録営業所コード
    '        If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
    '            OIT0002INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
    '        End If

    '        '空車発駅（着駅）コード
    '        If WW_COLUMNS.IndexOf("DEPSTATION") >= 0 Then
    '            OIT0002INProw("DEPSTATION") = XLSTBLrow("DEPSTATION")
    '        End If

    '        '空車発駅（着駅）名
    '        If WW_COLUMNS.IndexOf("DEPSTATIONNAME") >= 0 Then
    '            OIT0002INProw("DEPSTATIONNAME") = XLSTBLrow("DEPSTATIONNAME")
    '        End If

    '        '空車着駅（発駅）コード
    '        If WW_COLUMNS.IndexOf("RETSTATION") >= 0 Then
    '            OIT0002INProw("RETSTATION") = XLSTBLrow("RETSTATION")
    '        End If

    '        '空車着駅（発駅）名
    '        If WW_COLUMNS.IndexOf("RETSTATIONNAME") >= 0 Then
    '            OIT0002INProw("RETSTATIONNAME") = XLSTBLrow("RETSTATIONNAME")
    '        End If

    '        '空車着日（予定）
    '        If WW_COLUMNS.IndexOf("EMPARRDATE") >= 0 Then
    '            OIT0002INProw("EMPARRDATE") = XLSTBLrow("EMPARRDATE")
    '        End If

    '        '空車着日（実績）
    '        If WW_COLUMNS.IndexOf("ACTUALEMPARRDATE") >= 0 Then
    '            OIT0002INProw("ACTUALEMPARRDATE") = XLSTBLrow("ACTUALEMPARRDATE")
    '        End If

    '        '入線列車番号
    '        If WW_COLUMNS.IndexOf("LINETRAINNO") >= 0 Then
    '            OIT0002INProw("LINETRAINNO") = XLSTBLrow("LINETRAINNO")
    '        End If

    '        '入線順
    '        If WW_COLUMNS.IndexOf("LINEORDER") >= 0 Then
    '            OIT0002INProw("LINEORDER") = XLSTBLrow("LINEORDER")
    '        End If

    '        'タンク車№
    '        If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
    '            OIT0002INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")
    '        End If

    '        '前回油種
    '        If WW_COLUMNS.IndexOf("PREOILCODE") >= 0 Then
    '            OIT0002INProw("PREOILCODE") = XLSTBLrow("PREOILCODE")
    '        End If

    '        '削除フラグ
    '        If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
    '            OIT0002INProw("DELFLG") = XLSTBLrow("DELFLG")
    '        Else
    '            OIT0002INProw("DELFLG") = "0"
    '        End If

    '        OIT0002INPtbl.Rows.Add(OIT0002INProw)
    '    Next

    '    '○ 項目チェック
    '    INPTableCheck(WW_ERR_SW)

    '    '○ 入力値のテーブル反映
    '    OIT0002tbl_UPD()

    '    '○ 画面表示データ保存
    '    Master.SaveTable(OIT0002tbl)

    '    '○ メッセージ表示
    '    If isNormal(WW_ERR_SW) Then
    '        Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    '    Else
    '        Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
    '    End If

    '    '○ Close
    '    CS0023XLSUPLOAD.TBLDATA.Dispose()
    '    CS0023XLSUPLOAD.TBLDATA.Clear()

    'End Sub

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

    ''' <summary>
    ''' アップロードデータチェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_CheckUpload(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_Kensa As String = "検"
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_ErrMES() As String = {"本線列車未登録のため受注登録できません",
                                     "油種は対象外のため受注登録できません",
                                     "検査中のため受注登録できません"}

        For Each OIT0002ExlUProw As DataRow In OIT0002EXLUPtbl.Rows

            '◯本線列車未設定チェック(運用指示書に登録ありで、本線列車が未登録の場合)
            '★本線列車が未登録の場合、かつ
            '　　　　　(運用指示)油種が設定されている
            '　または、(運用指示)回転が設定されている
            '　または、(運用指示)位置が設定されている
            '　または、(運用指示)入線列車が設定されている
            '　または、(運用指示)積込後の着駅が設定されている
            '　または、(ポラリス必須)積込日が設定されている
            '　または、(ポラリス必須)発日が設定されている
            If OIT0002ExlUProw("LOADINGTRAINNO") = "" AndAlso
                (OIT0002ExlUProw("OILNAME") <> "" _
                 OrElse OIT0002ExlUProw("LINE") <> "" _
                 OrElse OIT0002ExlUProw("POSITION") <> "" _
                 OrElse OIT0002ExlUProw("INLINETRAIN") <> "" _
                 OrElse OIT0002ExlUProw("LOADARRSTATION") <> "" _
                 OrElse OIT0002ExlUProw("LOADINGLODDATE") <> "" _
                 OrElse OIT0002ExlUProw("LOADINGDEPDATE") <> "") Then

                If OIT0002ExlUProw("ARTICLENAME") = WW_Kensa Then
                    WW_CheckUploadERR(WW_CheckMES1, WW_CheckMES2, OIT0002ExlUProw, WW_ErrMES(2))
                    Continue For
                Else
                    WW_CheckUploadERR(WW_CheckMES1, WW_CheckMES2, OIT0002ExlUProw, WW_ErrMES(0))
                End If
                O_RTN = "WAR"

            End If

            '◯油種が営業所未対象チェック
            '★(運用指示)油種と(受注登録用)油種が不一致
            If OIT0002ExlUProw("OILNAME").ToString() <> OIT0002ExlUProw("ORDERINGOILNAME").ToString() Then

                If OIT0002ExlUProw("ARTICLENAME") = WW_Kensa Then
                    WW_CheckUploadERR(WW_CheckMES1, WW_CheckMES2, OIT0002ExlUProw, WW_ErrMES(2))
                    Continue For
                Else
                    WW_CheckUploadERR(WW_CheckMES1, WW_CheckMES2, OIT0002ExlUProw, WW_ErrMES(1))
                End If
                O_RTN = "WAR"

            End If
        Next

    End Sub

    ''' <summary>
    ''' (貨車連結表(臨海)TBL)受注No更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateRLinkOrderNo(ByVal SQLcon As SqlConnection, ByVal OIT0002row As DataRow)

        Try
            'DataBase接続文字
            'Dim SQLcon = CS0050SESSION.getConnection
            'SQLcon.Open() 'DataBase接続(Open)

            Dim SQLStr As String = ""
            '更新SQL文･･･貨車連結表(臨海)TBLの受注Noを更新
            SQLStr =
                " UPDATE OIL.OIT0011_RLINK " _
                & "    SET ORDERNO        = @P04, " _
                & "        DETAILNO       = @P05, " _
                & "        UPDYMD         = @P11, " _
                & "        UPDUSER        = @P12, " _
                & "        UPDTERMID      = @P13, " _
                & "        RECEIVEYMD     = @P14  " _
                & "  WHERE RLINKNO        = @P01  " _
                & "    AND RLINKDETAILNO  = @P02  " _
                & "    AND DELFLG        <> @P03; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = OIT0002row("RLINKNO")
            PARA02.Value = OIT0002row("RLINKDETAILNO")
            PARA03.Value = C_DELETE_FLG.DELETE
            PARA04.Value = OIT0002row("ORDERNO")
            PARA05.Value = OIT0002row("DETAILNO")

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L_RLINK_ORDERNO UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L_RLINK_ORDERNO UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (タンク車所在TBL)の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_TANKNO As String = Nothing,
                                      Optional ByVal I_LOCATION As String = Nothing,
                                      Optional ByVal I_SITUATION As String = Nothing,
                                      Optional ByVal I_USEORDERNO As Boolean = False)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･タンク車所在TBL更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0005_SHOZAI " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            '所在地コード
            If Not String.IsNullOrEmpty(I_LOCATION) Then
                SQLStr &= String.Format("        LOCATIONCODE = '{0}', ", I_LOCATION)
            Else
                SQLStr &= "        LOCATIONCODE = @P03, "
            End If
            'タンク車状態コード
            If Not String.IsNullOrEmpty(I_STATUS) Then
                SQLStr &= String.Format("        TANKSTATUS   = '{0}', ", I_STATUS)
            End If
            '積車区分
            If Not String.IsNullOrEmpty(I_KBN) Then
                SQLStr &= String.Format("        LOADINGKBN   = '{0}', ", I_KBN)
            End If
            'タンク車状況コード
            If Not String.IsNullOrEmpty(I_SITUATION) Then
                SQLStr &= String.Format("        TANKSITUATION = '{0}', ", I_SITUATION)
            End If
            '使用受注№
            If I_USEORDERNO = True Then
                SQLStr &= String.Format("        USEORDERNO = '{0}', ", "")
            End If

            SQLStr &=
                      "        LASTOILCODE    = @P04, " _
                    & "        LASTOILNAME    = @P05, " _
                    & "        PREORDERINGTYPE    = @P06, " _
                    & "        PREORDERINGOILNAME = @P07, " _
                    & "        EMPARRDATE         = @P08, " _
                    & "        ACTUALEMPARRDATE   = @P09, " _
                    & "        UPDYMD         = @P11, " _
                    & "        UPDUSER        = @P12, " _
                    & "        UPDTERMID      = @P13, " _
                    & "        RECEIVEYMD     = @P14  " _
                    & "  WHERE TANKNUMBER     = @P01  " _
                    & "    AND TANKSITUATION <> '3' " _
                    & "    AND DELFLG        <> @P02 "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  'タンク車№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)  '所在地コード
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)  '前回油種
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)  '前回油種名
            Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)  '前回油種区分(受発注用)
            Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)  '前回油種名(受発注用)
            Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)      '空車着日（予定）
            Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)      '空車着日（実績）

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA02.Value = C_DELETE_FLG.DELETE
            PARA08.Value = DBNull.Value
            PARA09.Value = DBNull.Value

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            If Not String.IsNullOrEmpty(I_TANKNO) Then
                '(一覧)で設定しているタンク車をKEYに更新
                For Each OIT0002EXLINSrow As DataRow In OIT0002EXLINStbl.Rows
                    If I_TANKNO = OIT0002EXLINSrow("TANKNUMBER") Then
                        PARA01.Value = OIT0002EXLINSrow("TANKNUMBER")           'タンク車No
                        PARA03.Value = OIT0002EXLINSrow("RETSTATION")           '空車着駅コード
                        PARA04.Value = OIT0002EXLINSrow("PREOILCODE")           '前回油種
                        PARA05.Value = OIT0002EXLINSrow("PREOILNAME")           '前回油種名
                        PARA06.Value = OIT0002EXLINSrow("PREORDERINGTYPE")      '前回油種区分(受発注用)
                        PARA07.Value = OIT0002EXLINSrow("PREORDERINGOILNAME")   '前回油種名(受発注用)
                        SQLcmd.ExecuteNonQuery()
                        Exit For
                    End If
                Next
            Else
                '(一覧)で設定しているタンク車をKEYに更新
                For Each OIT0002EXLINSrow As DataRow In OIT0002EXLINStbl.Rows

                    PARA01.Value = OIT0002EXLINSrow("TANKNUMBER")           'タンク車No
                    PARA03.Value = OIT0002EXLINSrow("RETSTATION")           '空車着駅コード
                    PARA04.Value = OIT0002EXLINSrow("PREOILCODE")           '前回油種
                    PARA05.Value = OIT0002EXLINSrow("PREOILNAME")           '前回油種名
                    PARA06.Value = OIT0002EXLINSrow("PREORDERINGTYPE")      '前回油種区分(受発注用)
                    PARA07.Value = OIT0002EXLINSrow("PREORDERINGOILNAME")   '前回油種名(受発注用)
                    SQLcmd.ExecuteNonQuery()
                Next
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨車連結順序表№ =" & OIT0002row("LINKNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨車連結順序表明細№ =" & OIT0002row("LINKDETAILNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 利用可能日 =" & OIT0002row("AVAILABLEYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> ステータス =" & OIT0002row("STATUS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報 =" & OIT0002row("INFO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回オーダー№ =" & OIT0002row("PREORDERNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車 =" & OIT0002row("TRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録営業所コード =" & OIT0002row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車発駅コード =" & OIT0002row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車発駅名 =" & OIT0002row("DEPSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着駅コード =" & OIT0002row("RETSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着駅名 =" & OIT0002row("RETSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着日（予定） =" & OIT0002row("EMPARRDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着日（実績） =" & OIT0002row("ACTUALEMPARRDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線列車番号 =" & OIT0002row("LINETRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線順 =" & OIT0002row("LINEORDER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車№ =" & OIT0002row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回油種 =" & OIT0002row("PREOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIT0002row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(アップロード用)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIT0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckUploadERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String,
                                    Optional ByVal OIT0002row As DataRow = Nothing,
                                    Optional ByVal ERRReason As String = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIT0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 該当行　　　:" & OIT0002row("SERIALNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車　　:" & OIT0002row("LOADINGTRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込日　　　:" & OIT0002row("LOADINGLODDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発日　　　　:" & OIT0002row("LOADINGDEPDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種　　　　:" & OIT0002row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 回転　　　　:" & OIT0002row("LINE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 位置　　　　:" & OIT0002row("POSITION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線列車　　:" & OIT0002row("INLINETRAIN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込後の着駅:" & OIT0002row("LOADARRSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エラー理由　:" & ERRReason
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
                If OIT0002row("LINKNO") = OIT0002INProw("LINKNO") AndAlso
                         OIT0002row("LINKDETAILNO") = OIT0002INProw("LINKDETAILNO") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIT0002row("DELFLG") = OIT0002INProw("DELFLG") AndAlso
                        OIT0002row("AVAILABLEYMD") = OIT0002INProw("AVAILABLEYMD") AndAlso
                        OIT0002row("STATUS") = OIT0002INProw("STATUS") AndAlso
                        OIT0002row("INFO") = OIT0002INProw("INFO") AndAlso
                        OIT0002row("PREORDERNO") = OIT0002INProw("PREORDERNO") AndAlso
                        OIT0002row("TRAINNO") = OIT0002INProw("TRAINNO") AndAlso
                        OIT0002row("OFFICECODE") = OIT0002INProw("OFFICECODE") AndAlso
                        OIT0002row("DEPSTATION") = OIT0002INProw("DEPSTATION") AndAlso
                        OIT0002row("DEPSTATIONNAME") = OIT0002INProw("DEPSTATIONNAME") AndAlso
                        OIT0002row("RETSTATION") = OIT0002INProw("RETSTATION") AndAlso
                        OIT0002row("RETSTATIONNAME") = OIT0002INProw("RETSTATIONNAME") AndAlso
                        OIT0002row("EMPARRDATE") = OIT0002INProw("EMPARRDATE") AndAlso
                        OIT0002row("ACTUALEMPARRDATE") = OIT0002INProw("ACTUALEMPARRDATE") AndAlso
                        OIT0002row("LINETRAINNO") = OIT0002INProw("LINETRAINNO") AndAlso
                        OIT0002row("LINEORDER") = OIT0002INProw("LINEORDER") AndAlso
                        OIT0002row("TANKNUMBER") = OIT0002INProw("TANKNUMBER") AndAlso
                        OIT0002row("PREOILCODE") = OIT0002INProw("PREOILCODE") AndAlso
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
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
                OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
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
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
                OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
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

                Case "SALESOFFICE"      '登録営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "USEPROPRIETY"     '利用可否フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "USEPROPRIETY"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub WW_FixvalueMasterSearch(ByVal I_CODE As String,
                                          ByVal I_CLASS As String,
                                          ByVal I_KEYCODE As String,
                                          ByRef O_VALUE() As String,
                                          Optional ByVal I_PARA01 As String = Nothing)

        If IsNothing(OIT0002Fixvaltbl) Then
            OIT0002Fixvaltbl = New DataTable
        End If

        If OIT0002Fixvaltbl.Columns.Count <> 0 Then
            OIT0002Fixvaltbl.Columns.Clear()
        End If

        OIT0002Fixvaltbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
                & " , ISNULL(RTRIM(VIW0001.VALUE16), '')     AS VALUE16" _
                & " , ISNULL(RTRIM(VIW0001.VALUE17), '')     AS VALUE17" _
                & " , ISNULL(RTRIM(VIW0001.VALUE18), '')     AS VALUE18" _
                & " , ISNULL(RTRIM(VIW0001.VALUE19), '')     AS VALUE19" _
                & " , ISNULL(RTRIM(VIW0001.VALUE20), '')     AS VALUE20" _
                & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
                & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
                & " WHERE VIW0001.CLASS = @P01" _
                & " AND VIW0001.DELFLG <> @P03"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '会社コード
            If Not String.IsNullOrEmpty(I_CODE) Then
                SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
            End If
            'マスターキー
            If Not String.IsNullOrEmpty(I_KEYCODE) Then
                SQLStr &= String.Format("    AND VIW0001.KEYCODE = '{0}'", I_KEYCODE)
            End If

            SQLStr &=
                  " ORDER BY" _
                & "    VIW0001.KEYCODE"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                'Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

                PARA01.Value = I_CLASS
                'PARA02.Value = I_KEYCODE
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then

                    If IsNothing(I_PARA01) Then
                        'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                        For Each OIT0002WKrow As DataRow In OIT0002Fixvaltbl.Rows '(全抽出結果回るので要検討
                            'O_VALUE(i) = OIT0003WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                            For i = 1 To O_VALUE.Length
                                O_VALUE(i - 1) = OIT0002WKrow("VALUE" & i.ToString())
                            Next
                            'i += 1 '2020/3/23 三宅 Delete
                        Next

                    ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                        Dim i As Integer = 0
                        For Each OIT0002WKrow As DataRow In OIT0002Fixvaltbl.Rows
                            O_VALUE(i) = Convert.ToString(OIT0002WKrow("KEYCODE"))
                            i += 1
                        Next
                    End If

                Else
                    For Each OIT0002WKrow As DataRow In OIT0002Fixvaltbl.Rows

                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0002WKrow("VALUE" & i.ToString())
                        Next
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002L MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002L MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

End Class

''************************************************************
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
    Private OIT0002EXLUPtbl As DataTable                             'EXCELアップロード用
    Private OIT0002EXLDELtbl As DataTable                            'EXCELアップロード(削除)用
    Private OIT0002Fixvaltbl As DataTable                            '作業用テーブル(固定値マスタ取得用)

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
            & " WHERE ISNULL(OIT0011.TRUCKSYMBOL,'') <> '' "

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

            '更新SQL文･･･受注・受注明細を一括論理削除
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
                    OIT0002UPDrow("DELFLG") = C_DELETE_FLG.DELETE
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

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CS0023XLSUPLOAD_RLINK(OIT0002EXLUPtbl)

        '◯貨車連結(臨海)TBL削除処理(再アップロード対応)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_DELETE_RLINK(SQLcon)
        End Using

        '◯貨車連結(臨海)TBL追加処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_INSERT_RLINK(SQLcon)
        End Using

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
    Protected Sub WW_INSERT_RLINK(ByVal SQLcon As SqlConnection)

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
                & " ( RLINKNO       , RLINKDETAILNO  , FILENAME      , AGOBEHINDFLG    , REGISTRATIONDATE" _
                & " , TRAINNO       , SERIALNUMBER   , TRUCKSYMBOL   , TRUCKNO" _
                & " , DEPSTATIONNAME, ARRSTATIONNAME , ARTICLENAME   , CONVERSIONAMOUNT" _
                & " , ARTICLE       , ARTICLETRAINNO , ARTICLEOILNAME, CURRENTCARTOTAL" _
                & " , EXTEND        , CONVERSIONTOTAL, LINKNO" _
                & " , DELFLG        , INITYMD        , INITUSER      , INITTERMID" _
                & " , UPDYMD        , UPDUSER        , UPDTERMID     , RECEIVEYMD)"

            SQLRLinkStr &=
                  " VALUES" _
                & " ( @RLINKNO       , @RLINKDETAILNO  , @FILENAME      , @AGOBEHINDFLG    , @REGISTRATIONDATE" _
                & " , @TRAINNO       , @SERIALNUMBER   , @TRUCKSYMBOL   , @TRUCKNO" _
                & " , @DEPSTATIONNAME, @ARRSTATIONNAME , @ARTICLENAME   , @CONVERSIONAMOUNT" _
                & " , @ARTICLE       , @ARTICLETRAINNO , @ARTICLEOILNAME, @CURRENTCARTOTAL" _
                & " , @EXTEND        , @CONVERSIONTOTAL, @LINKNO" _
                & " , @DELFLG        , @INITYMD        , @INITUSER      , @INITTERMID" _
                & " , @UPDYMD        , @UPDUSER        , @UPDTERMID     , @RECEIVEYMD);"

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
                Dim SERIALNUMBER As SqlParameter = SQLRLinkcmd.Parameters.Add("@SERIALNUMBER", SqlDbType.Int)                '通番
                Dim TRUCKSYMBOL As SqlParameter = SQLRLinkcmd.Parameters.Add("@TRUCKSYMBOL", SqlDbType.NVarChar)             '貨車(記号及び符号)
                Dim TRUCKNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@TRUCKNO", SqlDbType.NVarChar)                     '貨車(番号)
                Dim DEPSTATIONNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@DEPSTATIONNAME", SqlDbType.NVarChar)       '発駅
                Dim ARRSTATIONNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARRSTATIONNAME", SqlDbType.NVarChar)       '着駅
                Dim ARTICLENAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLENAME", SqlDbType.NVarChar)             '品名
                Dim CONVERSIONAMOUNT As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVERSIONAMOUNT", SqlDbType.Decimal)    '換算数量
                Dim ARTICLE As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLE", SqlDbType.NVarChar)                     '記事
                Dim ARTICLETRAINNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLETRAINNO", SqlDbType.NVarChar)                     '記事
                Dim ARTICLEOILNAME As SqlParameter = SQLRLinkcmd.Parameters.Add("@ARTICLEOILNAME", SqlDbType.NVarChar)                     '記事
                Dim CURRENTCARTOTAL As SqlParameter = SQLRLinkcmd.Parameters.Add("@CURRENTCARTOTAL", SqlDbType.Decimal)      '現車合計
                Dim EXTEND As SqlParameter = SQLRLinkcmd.Parameters.Add("@EXTEND", SqlDbType.Decimal)                        '延長
                Dim CONVERSIONTOTAL As SqlParameter = SQLRLinkcmd.Parameters.Add("@CONVERSIONTOTAL", SqlDbType.Decimal)      '換算合計
                Dim LINKNO As SqlParameter = SQLRLinkcmd.Parameters.Add("@LINKNO", SqlDbType.NVarChar)                       '貨車連結順序表№
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
                For Each OIT0002EXLUProw As DataRow In OIT0002EXLUPtbl.Rows
                    'Select Case (Nothing, "ARRSTATIONNAME, DEPSTATIONNAME, SERIALNUMBER")

                    '貨車連結(臨海)順序表№
                    RLINKNO.Value = sRLinkNo
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
                    '現車合計
                    If OIT0002EXLUProw("CURRENTCARTOTAL") = "" Then
                        CURRENTCARTOTAL.Value = DBNull.Value
                    Else
                        CURRENTCARTOTAL.Value = Decimal.Parse(OIT0002EXLUProw("CURRENTCARTOTAL"))
                    End If
                    '延長
                    EXTEND.Value = OIT0002EXLUProw("EXTEND")
                    '換算合計
                    If OIT0002EXLUProw("CONVERSIONTOTAL") = "" Then
                        CONVERSIONTOTAL.Value = DBNull.Value
                    Else
                        CONVERSIONTOTAL.Value = Decimal.Parse(OIT0002EXLUProw("CONVERSIONTOTAL"))
                    End If

                    '### 20200710 START((全体)No102対応) ######################################
                    '貨車連結順序表№
                    If strArrstationName <> "" _
                        AndAlso strArrstationName <> ARRSTATIONNAME.Value _
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

                    SQLRLinkcmd.CommandTimeout = 300
                    SQLRLinkcmd.ExecuteNonQuery()
                Next
                'CLOSE
                SQLRLinkcmd.Dispose()

                '貨車連結TBL追加処理
                WW_INSERT_LINK(SQLcon, WW_ERRCODE, I_RLinkNo:=sRLinkNo)
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
                                 Optional ByVal I_RLinkNo As String = Nothing)
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

            SQLLinkStr &=
                  " SELECT DISTINCT" _
                & "   OIT0011.LINKNO                                   AS LINKNO" _
                & " , OIT0011.RLINKDETAILNO                            AS LINKDETAILNO" _
                & " , OIT0011.REGISTRATIONDATE                         AS AVAILABLEYMD" _
                & " , '1'                                              AS STATUS" _
                & " , ''                                               AS INFO" _
                & " , ''                                               AS PREORDERNO" _
                & " , OIT0011.TRAINNO                                  AS TRAINNO" _
                & " , ISNULL(OIM0007.TRAINNAME, '')                    AS TRAINNAME" _
                & " , VIW0002.OFFICECODE                               AS OFFICECODE" _
                & " , VIW0002.DEPSTATION                               AS DEPSTATION" _
                & " , VIW0002.DEPSTATIONNAME                           AS DEPSTATIONNAME" _
                & " , VIW0002.ARRSTATION                               AS RETSTATION" _
                & " , VIW0002.ARRSTATIONNAME                           AS RETSTATIONNAME" _
                & " , CONVERT(NVARCHAR, (CONVERT(DATETIME, OIT0011.REGISTRATIONDATE)" _
                & "   + (CONVERT(INT, VIW0001.VALUE7) - CONVERT(INT, VIW0001.VALUE6))), 111) AS EMPARRDATE" _
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

            SQLLinkStr &=
                  " FROM OIL.OIT0011_RLINK OIT0011" _
                & " LEFT JOIN OIL.VIW0002_LINKCONVERTMASTER VIW0002 ON" _
                & "  VIW0002.DEPSTATIONNAME = OIT0011.DEPSTATIONNAME" _
                & "  AND VIW0002.ARRSTATIONNAME = OIT0011.ARRSTATIONNAME" _
                & " LEFT JOIN OIL.OIM0007_TRAIN OIM0007 ON" _
                & "  OIM0007.OTTRAINNO = OIT0011.TRAINNO" _
                & "  AND OIM0007.DEPSTATION = VIW0002.DEPSTATION" _
                & "  AND OIM0007.ARRSTATION = VIW0002.ARRSTATION"
            SQLLinkStr &= String.Format("  AND OIM0007.DELFLG <> '{0}'", C_DELETE_FLG.DELETE)

            '### 20200706 START((内部)No184対応) ######################################
            SQLLinkStr &=
                  " LEFT JOIN OIL.TMP0005OILMASTER TMP0005 ON" _
                & "  TMP0005.OFFICECODE = VIW0002.OFFICECODE" _
                & "  AND TMP0005.OILNo = '1'" _
                & "  AND TMP0005.RINKAIOILCODE <> ''" _
                & "  AND TMP0005.RINKAIOILKANA = OIT0011.ARTICLEOILNAME"
            SQLLinkStr &= String.Format("  AND TMP0005.OILCODE IN ('{0}', '{1}', '{2}', '{3}', '{4}')",
                                        BaseDllConst.CONST_HTank,
                                        BaseDllConst.CONST_RTank,
                                        BaseDllConst.CONST_TTank,
                                        BaseDllConst.CONST_KTank1,
                                        BaseDllConst.CONST_ATank)

            SQLLinkStr &=
                  " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON" _
                & "  OIT0005.TANKNUMBER = OIT0011.TRUCKNO"
            SQLLinkStr &= String.Format("  AND OIT0005.DELFLG <> '{0}'", C_DELETE_FLG.DELETE)
            '### 20200706 END  ((内部)No184対応) ######################################

            '### 20200710 START 列車マスタ(返送)から次回利用可能日を取得 ##############
            SQLLinkStr &=
                  " LEFT JOIN OIL.VIW0001_FIXVALUE VIW0001 ON" _
                & "  VIW0001.CLASS = 'BTRAINNUMBER_FIND'" _
                & "  AND VIW0001.CAMPCODE = VIW0002.OFFICECODE" _
                & "  AND VIW0001.KEYCODE = OIT0011.TRAINNO + VIW0002.DEPSTATION"
            '### 20200710 END   列車マスタ(返送)から次回利用可能日を取得 ##############

            SQLLinkStr &= String.Format(" WHERE OIT0011.DELFLG <> '{0}'", C_DELETE_FLG.DELETE) _
                & "  AND OIT0011.TRUCKSYMBOL <> ''" _
                & "  AND VIW0002.OFFICECODE IS NOT NULL"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '貨車連結(臨海)順序表№
            If Not String.IsNullOrEmpty(I_RLinkNo) Then
                SQLLinkStr &= String.Format("    AND OIT0011.RLINKNO = '{0}'", I_RLinkNo)
            End If

            '### 20200717 START((全体)No114対応) ######################################
            '★ 貨車連結順序表アップロード時において、品目が交検以外を対象とする。
            SQLLinkStr &= String.Format("    AND OIT0011.ARTICLENAME <> '{0}'", WW_ARTICLENAME)
            '### 20200717 START((全体)No114対応) ######################################

            Using SQLLinkcmd As New SqlCommand(SQLLinkStr, SQLcon)
                SQLLinkcmd.CommandTimeout = 300
                SQLLinkcmd.ExecuteNonQuery()

                'CLOSE
                SQLLinkcmd.Dispose()
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

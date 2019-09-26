Imports System.Data.SqlClient

Public Class GRTA0005LMKINTAI
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013ProfView As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
    ''' <summary>
    ''' テーブルソート
    ''' </summary>    
    Private CS0026TblSort As New CS0026TBLSORT                      'テーブルソート
    ''' <summary>
    ''' 帳票出力(入力：TBL)
    ''' </summary>
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力(入力：TBL)
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション管理
    ''' <summary>
    ''' 勤怠関連共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通
    ''' <summary>
    ''' 事務勤怠共通
    ''' </summary>
    Private T0008COM As New GRT0008COM                              '事務勤怠共通

    '検索結果格納ds
    Private TA0005tbl As DataTable                                  'Grid格納用テーブル
    Private TA0005SUMtbl As DataTable                               'Grid格納用テーブル
    Private TA0005VIEWtbl As DataTable                              'Grid格納用テーブル
    Private SELECTORtbl As DataTable                                'TREE選択作成作業テーブル

    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERRCODE As String = String.Empty             'リターンコード
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String                                     '
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String                                      '
    ''' <summary>
    ''' 一覧最大表示件数（一画面）
    ''' </summary>
    Private Const CONST_DSPROWCOUNT As Integer = 45         '１画面表示対象
    ''' <summary>
    ''' 一覧のマウススクロール時の増分（件数）
    ''' </summary>
    Private Const CONST_SCROLLROWCOUNT As Integer = 20      'マウススクロール時の増分
    ''' <summary>
    ''' 詳細部タブID
    ''' </summary>
    Private Const CONST_DETAIL_TABID As String = "DTL1"     '詳細部タブID

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then

            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonINQ"                 '■ 照会ボタン押下時処理
                        WF_ButtonINQ_Click()
                    Case "WF_ButtonXLS"                 '■ ダウンロードボタンクリック時処理
                        WF_ButtonXLS_Click()
                    Case "WF_ButtonFIRST"               '■ 最始行ボタンクリック時処理
                        WF_ButtonFIRST_Click()
                    Case "WF_ButtonLAST"                '■ 最終行ボタンクリック時処理
                        WF_ButtonLAST_Click()
                    Case "WF_ButtonEND"                 '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_SELECTOR_CHG"              '■ セレクタ変更ラジオボタンクリック処理
                        WF_Selector_Change_Click()
                    Case "WF_SELECTOR_SW_Click"         '■ セレクタ変更ラジオボタンクリック処理
                        SELECTOR_Click()
                    Case "WF_CHECKBOX_CHG"              '■ チェックボックス変更時処理
                End Select
            End If
            '○ 一覧再表示処理
            DisplayGrid()
        Else
            '〇初期化処理
            Initialize()

        End If

        '○Close
        If Not IsNothing(TA0005tbl) Then
            TA0005tbl.Dispose()
            TA0005tbl = Nothing
        End If
        If Not IsNothing(TA0005VIEWtbl) Then
            TA0005VIEWtbl.Dispose()
            TA0005VIEWtbl = Nothing
        End If
        If Not IsNothing(TA0005SUMtbl) Then
            TA0005SUMtbl.Dispose()
            TA0005SUMtbl = Nothing
        End If
        If Not IsNothing(SELECTORtbl) Then
            SELECTORtbl.Dispose()
            SELECTORtbl = Nothing
        End If

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定

        rightview.resetindex()
        leftview.activeListBox()
        '〇 条件抽出画面情報退避
        MAPrefelence()
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        GetMapData()

        '○画面表示データ保存
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0005tbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0005tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.PROFID = Master.PROF_VIEW
            CS0013ProfView.MAPID = GRTA0005WRKINC.MAPID
            CS0013ProfView.VARI = Master.VIEWID
            CS0013ProfView.SRCDATA = TBLview.ToTable
            CS0013ProfView.TBLOBJ = pnlListArea
            CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
            CS0013ProfView.TITLEOPT = True
            CS0013ProfView.HIDEOPERATIONOPT = True
            CS0013ProfView.CS0013ProfView()
        End Using
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If
        '〇セレクタ初期表示処理
        WF_SelectorMView.ActiveViewIndex = 0

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        If IsNothing(TA0005VIEWtbl) Then
            If Not Master.RecoverTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        End If

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0005VIEWtbl.Rows.Count - 1
            If TA0005VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0005VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
            End If
        Next

        '○表示Linecnt取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLROWCOUNT) <= WW_DataCNT Then
                WW_GridPosition = WW_GridPosition + CONST_SCROLLROWCOUNT
            End If
        End If

        '表示開始_位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLROWCOUNT) > 0 Then
                WW_GridPosition = WW_GridPosition - CONST_SCROLLROWCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○画面（GridView）表示
        Dim WW_TBLview As DataView = New DataView(TA0005VIEWtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = GRTA0005WRKINC.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = WW_TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If

    End Sub
    ''' <summary>
    ''' 照会ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINQ_Click()

        'チェックボックス選択チェック
        If WF_CBOX_SW1.Checked = False AndAlso
            WF_CBOX_SW2.Checked = False AndAlso
            WF_CBOX_SW3.Checked = False Then
            Master.output(C_MESSAGE_NO.SELECT_AGGREGATE_CONDITION, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '■ データリカバリ
        '○ T00005ALLデータリカバリ
        If Not Master.RecoverTable(TA0005tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        '○T00005VIEWtbl取得
        GetViewTA0005Tbl()

        '○ ２次サマリー
        SumTA0005Work2()
        Dim wCNT As Integer = 0
        For Each TA0005row As DataRow In TA0005VIEWtbl.Rows
            wCNT = wCNT + 1
            TA0005row("LINECNT") = wCNT
        Next

        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        WF_SaveX.Value = 0
        WF_SaveY.Value = 0

    End Sub
    ''' <summary>
    ''' セレクタ変更ラジオボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_Selector_Change_Click()
        WF_SelectorMView.ActiveViewIndex = WF_SELECTOR_Chg.Value
        WF_SELECTOR_Chg.Value = String.Empty
    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPDF_Click()

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ 帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0005WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0005VIEWtbl                      'データ参照DataTable
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.CS0030REPORT()

        If isNormal(CS0030REPORT.ERR) Then
        Else
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0005WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0005VIEWtbl                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "f_ExcelPrint", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 画面遷移実行
        Master.transitionPrevPage()
    End Sub

    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>                                                  
    Protected Sub WF_ButtonFIRST_Click()

        '■ データリカバリ
        '○ T00005ALLデータリカバリ
        If Not Master.RecoverTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '■ GridView表示
        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0005VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(TA0005VIEWtbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○最終頁に移動
        If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
        End If

    End Sub
    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' T00005VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0005Tbl()

        '〇 T00007ALLよりデータ抽出
        Dim WW_Sort As String = ""
        Dim WW_Filter As String = ""

        Dim WW_View As DataView
        WW_View = New DataView(TA0005tbl)

        WW_Sort = "LINECNT"
        If Not String.IsNullOrEmpty(WF_SELECTOR_PosiORG.Value) AndAlso WF_SELECTOR_PosiORG.Value <> GRTA0004WRKINC.ALL_SELECTOR.CODE Then
            WW_Sort = WW_Sort & ",PAYHORG"
            WW_Filter = WW_Filter & "PAYHORG = '" & WF_SELECTOR_PosiORG.Value & "'"
        End If

        If Not String.IsNullOrEmpty(WF_SELECTOR_PosiSTAFF.Value) AndAlso WF_SELECTOR_PosiSTAFF.Value <> GRTA0004WRKINC.ALL_SELECTOR.CODE Then

                WW_Sort = WW_Sort & ",PAYSTAFFCODE"
                If WW_Filter <> "" Then
                    WW_Filter = WW_Filter & " and "
                End If
                WW_Filter = WW_Filter & "PAYSTAFFCODE = '" & WF_SELECTOR_PosiSTAFF.Value & "'"
            End If

            WW_View.Sort = WW_Sort
        WW_View.RowFilter = WW_Filter

        TA0005VIEWtbl = WW_View.ToTable

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0005VIEWrow As DataRow In TA0005VIEWtbl.Rows
            TA0005VIEWrow("LINECNT") = 0
        Next

        For Each TA0005VIEWrow As DataRow In TA0005VIEWtbl.Rows

            If TA0005VIEWrow("LINECNT") = 0 Then
                TA0005VIEWrow("SELECT") = "1"
                TA0005VIEWrow("HIDDEN") = "0"      '表示
                WW_LINECNT += 1
                TA0005VIEWrow("LINECNT") = WW_LINECNT
            End If

        Next

        '〇 Close
        WW_View.Dispose()
        WW_View = Nothing

    End Sub

    ''' <summary>
    '''  表示元データ(TA0005WKtbl)取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetMapData()

        '■ 表示元データ(条件によるサマリーデータ)取得
        'カラム設定
        AddColumnToTA0005tbl(TA0005tbl)

        '端末クラス取得（本社サーバーを識別したい）
        Dim WW_TermClass = GetTermClass(CS0050Session.APSV_ID)

        GetTA0005Work(WW_TermClass)

        If TA0005tbl.Rows.Count > 65000 Then
            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ERR)
            TA0005tbl.Clear()
            Exit Sub
        End If

        GetTA0005Work2(WW_TermClass)

        If TA0005tbl.Rows.Count > 65000 Then
            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ERR)
            TA0005tbl.Clear()
            Exit Sub
        End If

        '■ セレクター作成
        InitialSelector()

        '■ ソート
        CS0026TblSort.TABLE = TA0005tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "PAYHORG,PAYSTAFFCODE,NACSHUKODATE"
        TA0005tbl = CS0026TblSort.sort()

        Dim wCNT As Integer = 0
        For Each TA0005row As DataRow In TA0005tbl.Rows
            wCNT = wCNT + 1
            TA0005row("LINECNT") = wCNT
        Next

    End Sub

    ''' <summary>
    ''' 抽出条件の部署一覧を作成する
    ''' </summary>
    ''' <returns>部署一覧</returns>
    ''' <remarks></remarks>
    Private Function GetORGList(ByVal I_TERM_CLASS As String) As List(Of String)
        Using SQLcon As SqlConnection = CS0050Session.getConnection

            '抽出条件(サーバー部署)List作成
            Dim W_ORGlst As New List(Of String)
            Try

                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT          S06.CAMPCODE , S06.CODE      ")
                SQLStr.AppendLine(" FROM            S0006_ROLE S06               ")
                SQLStr.AppendLine(" WHERE           S06.CAMPCODE      =  @P02    ")
                SQLStr.AppendLine("             and S06.OBJECT        = 'ORG'    ")
                SQLStr.AppendLine("             and S06.ROLE          =  @P01    ")
                SQLStr.AppendLine("             and S06.CODE     like  @P04 +'%' ")
                SQLStr.AppendLine("             and S06.PERMITCODE    = '2'      ")
                SQLStr.AppendLine("             and S06.STYMD         <= @P03    ")
                SQLStr.AppendLine("             and S06.ENDYMD        >= @P03    ")
                SQLStr.AppendLine("             and S06.DELFLG        <> '1'     ")
                SQLStr.AppendLine(" GROUP BY        S06.CAMPCODE , S06.CODE      ")

                Using SQLcmdQRG = New SqlCommand(SQLStr.ToString, SQLcon)

                    Dim parm4 As String = ""

                    '本社サーバーの場合、自部署のみ抽出する。但し、総務の場合はすべて抽出
                    If I_TERM_CLASS = C_TERMCLASS.HEAD Then
                        Dim WW_UserOrg = Master.USER_ORG
                        If T0008COM.IsGeneralAffair(work.WF_SEL_CAMPCODE.Text, WW_UserOrg, WW_RTN_SW) Then
                            parm4 = ""
                        Else
                            parm4 = WW_UserOrg
                        End If
                    Else
                        parm4 = ""
                    End If

                    With SQLcmdQRG.Parameters
                        .Add("@P01", SqlDbType.NVarChar, 20).Value = Master.ROLE_ORG
                        .Add("@P02", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@P03", SqlDbType.Date).Value = Date.Now
                        .Add("@P04", SqlDbType.NVarChar, 20).Value = parm4
                    End With

                    SQLcmdQRG.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmdQRG.ExecuteReader()

                        While SQLdr.Read
                            W_ORGlst.Add(SQLdr("CODE"))
                        End While
                    End Using
                End Using

                Return W_ORGlst
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0006_ROLE SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0006_ROLE Select"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Return Nothing
                Exit Function
            End Try
        End Using

    End Function
    ''' <summary>
    '''  表示元データ(条件によるサマリー前データ)取得
    ''' </summary>
    ''' <param name="I_TERM_CLASS">端末種別</param>
    ''' <remarks></remarks>
    Private Sub GetTA0005Work(ByVal I_TERM_CLASS As String)

        '○初期クリア
        'TA0005tbl値設定
        Dim wINT As Integer
        Dim wDATE As Date
        Dim wDATETime As DateTime

        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As List(Of String) = GetORGList(I_TERM_CLASS)

        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As New StringBuilder(20000)
            SQLStr.AppendLine("  SELECT ")
            SQLStr.AppendLine("    isnull(rtrim(L01.CAMPCODE), '')                                as CAMPCODE ")
            SQLStr.AppendLine("  , isnull(rtrim(M01.NAMES), '')                                   as CAMPNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KEIJOYMD), '" & C_DEFAULT_YMD & "')           as KEIJOYMD ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.DENYMD), '" & C_DEFAULT_YMD & "')             as DENYMD ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.DENNO), '')                                   as DENNO ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KANRENDENNO), '')                             as KANRENDENNO ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.DTLNO), '')                                   as DTLNO ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.ACACHANTEI), '')                              as ACACHANTEI ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_09.VALUE1), '') ")
            SQLStr.AppendLine("      from  MC001_FIXVALUE MC1_09  ")
            SQLStr.AppendLine("      where MC1_09.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_09.CLASS = 'ACHANTEI'  ")
            SQLStr.AppendLine("        and MC1_09.KEYCODE = L01.ACACHANTEI  ")
            SQLStr.AppendLine("        and MC1_09.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_09.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_09.DELFLG <> '1'  ")
            SQLStr.AppendLine("    )                                                              as ACACHANTEINAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHUKODATE), '" & C_DEFAULT_YMD & "')       as NACSHUKODATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIDISTANCE), '0')                         as NACHAIDISTANCE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACKAIDISTANCE), '0')                         as NACKAIDISTANCE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACCHODISTANCE), '0')                         as NACCHODISTANCE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLDISTANCE), '0')                         as NACTTLDISTANCE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAISTDATE), '" & C_DEFAULT_YMD & "')       as NACHAISTDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIENDDATE), '" & C_DEFAULT_YMD & "')      as NACHAIENDDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIWORKTIME), '0')                         as NACHAIWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESSTDATE), '" & C_DEFAULT_YMD & "')       as NACGESSTDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESENDDATE), '" & C_DEFAULT_YMD & "')      as NACGESENDDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESWORKTIME), '0')                         as NACGESWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACCHOWORKTIME), '0')                         as NACCHOWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLWORKTIME), '0')                         as NACTTLWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACOUTWORKTIME), '0')                         as NACOUTWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKSTDATE), '" & C_DEFAULT_YMD & "')     as NACBREAKSTDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKENDDATE), '" & C_DEFAULT_YMD & "')    as NACBREAKENDDATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKTIME), '0')                           as NACBREAKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACCHOBREAKTIME), '0')                        as NACCHOBREAKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLBREAKTIME), '0')                        as NACTTLBREAKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACUNLOADCNT), '0')                           as NACUNLOADCNT ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACCHOUNLOADCNT), '0')                        as NACCHOUNLOADCNT ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLUNLOADCNT), '0')                        as NACTTLUNLOADCNT ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACOFFICESORG), '')                           as NACOFFICESORG ")
            SQLStr.AppendLine("  , isnull(( ")
            SQLStr.AppendLine("      select isnull(rtrim(M02_22.NAMES), '') ")
            SQLStr.AppendLine("      from M0002_ORG M02_22  ")
            SQLStr.AppendLine("      where M02_22.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and M02_22.ORGCODE = L01.NACOFFICESORG  ")
            SQLStr.AppendLine("        and M02_22.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_22.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_22.DELFLG <> '1' ")
            SQLStr.AppendLine("    ),'')                                                          as NACOFFICESORGNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACOFFICETIME), '0')                          as NACOFFICETIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.NACOFFICEBREAKTIME), '0')                     as NACOFFICEBREAKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUSHADATE), '" & C_DEFAULT_YMD & "')      as PAYSHUSHADATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTAISHADATE), '" & C_DEFAULT_YMD & "')      as PAYTAISHADATE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSTAFFKBN), '')                             as PAYSTAFFKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_29.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_29  ")
            SQLStr.AppendLine("      where MC1_29.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_29.CLASS = 'STAFFKBN'  ")
            SQLStr.AppendLine("        and MC1_29.KEYCODE = L01.PAYSTAFFKBN  ")
            SQLStr.AppendLine("        and MC1_29.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_29.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_29.DELFLG <> '1' )                                 as PAYSTAFFKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSTAFFCODE), '')                            as PAYSTAFFCODE ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MB1_4.STAFFNAMES), '') ")
            SQLStr.AppendLine("      from MB001_STAFF MB1_4  ")
            SQLStr.AppendLine("      where MB1_4.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MB1_4.STAFFCODE = L01.PAYSTAFFCODE  ")
            SQLStr.AppendLine("        and MB1_4.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MB1_4.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MB1_4.DELFLG <> '1' ")
            SQLStr.AppendLine("    )                                                              as PAYSTAFFCODENAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYMORG), '')                                 as PAYMORG ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(M02_20.NAMES), '') ")
            SQLStr.AppendLine("      from  M0002_ORG M02_20  ")
            SQLStr.AppendLine("      where M02_20.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and M02_20.ORGCODE = L01.PAYMORG  ")
            SQLStr.AppendLine("        and M02_20.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_20.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_20.DELFLG <> '1' )                                 as PAYMORGNAME  ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHORG), '')                                 as PAYHORG ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(M02_21.NAMES), '') ")
            SQLStr.AppendLine("      from M0002_ORG M02_21  ")
            SQLStr.AppendLine("      where M02_21.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and M02_21.ORGCODE = L01.PAYHORG  ")
            SQLStr.AppendLine("        and M02_21.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_21.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and M02_21.DELFLG <> '1'  ")
            SQLStr.AppendLine("    )                                                              as PAYHORGNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHOLIDAYKBN), '')                           as PAYHOLIDAYKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_40.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_40  ")
            SQLStr.AppendLine("      where MC1_40.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_40.CLASS = 'HOLIDAYKBN'  ")
            SQLStr.AppendLine("        and MC1_40.KEYCODE = L01.PAYHOLIDAYKBN  ")
            SQLStr.AppendLine("        and MC1_40.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_40.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_40.DELFLG <> '1'  ")
            SQLStr.AppendLine("    )                                                              as PAYHOLIDAYKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYKBN), '')                                  as PAYKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_31.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_31  ")
            SQLStr.AppendLine("      where MC1_31.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_31.CLASS = 'PAYKBN'  ")
            SQLStr.AppendLine("        and MC1_31.KEYCODE = L01.PAYKBN  ")
            SQLStr.AppendLine("        and MC1_31.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_31.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_31.DELFLG <> '1' ")
            SQLStr.AppendLine("    )                                                              as PAYKBNNAME     ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUKCHOKKBN), '')                          as PAYSHUKCHOKKBN ")
            SQLStr.AppendLine("  , isnull(( ")
            SQLStr.AppendLine("      select distinct isnull(rtrim(MC1_32.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_32  ")
            SQLStr.AppendLine("      where (MC1_32.CLASS = 'SHUKCHOKKBN'  ")
            SQLStr.AppendLine("          or MC1_32.CLASS = 'T0009_SHUKCHOKKBN')  ")
            SQLStr.AppendLine("        and MC1_32.CAMPCODE = L01.CAMPCODE        ")
            SQLStr.AppendLine("        and MC1_32.KEYCODE = L01.PAYSHUKCHOKKBN  ")
            SQLStr.AppendLine("        and MC1_32.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_32.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_32.DELFLG <> '1'  ")
            SQLStr.AppendLine("    ),'')                                                          as PAYSHUKCHOKKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYJYOMUKBN), '')                             as PAYJYOMUKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_33.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_33  ")
            SQLStr.AppendLine("      where MC1_33.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_33.CLASS = 'JYOMUKBN'  ")
            SQLStr.AppendLine("        and MC1_33.KEYCODE = L01.PAYJYOMUKBN  ")
            SQLStr.AppendLine("        and MC1_33.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_33.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_33.DELFLG <> '1' ")
            SQLStr.AppendLine("     )                                                             as PAYJYOMUKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYOILKBN), '')                               as PAYOILKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_35.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_35  ")
            SQLStr.AppendLine("      where MC1_35.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_35.CLASS = 'OILPAYKBN'  ")
            SQLStr.AppendLine("        and MC1_35.KEYCODE = L01.PAYOILKBN  ")
            SQLStr.AppendLine("        and MC1_35.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_35.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_35.DELFLG <> '1' ")
            SQLStr.AppendLine("    )                                                             as PAYOILKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHARYOKBN), '')                           as PAYSHARYOKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_36.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_36  ")
            SQLStr.AppendLine("      where MC1_36.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_36.CLASS = 'SHARYOKBN'  ")
            SQLStr.AppendLine("        and MC1_36.KEYCODE = L01.PAYSHARYOKBN  ")
            SQLStr.AppendLine("        and MC1_36.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_36.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_36.DELFLG <> '1' ")
            SQLStr.AppendLine("    )                                                             as PAYSHARYOKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYWORKNISSU), '0')                          as PAYWORKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHOUKETUNISSU), '0')                      as PAYSHOUKETUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYKUMIKETUNISSU), '0')                      as PAYKUMIKETUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYETCKETUNISSU), '0')                       as PAYETCKETUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYNENKYUNISSU), '0')                        as PAYNENKYUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTOKUKYUNISSU), '0')                       as PAYTOKUKYUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYCHIKOKSOTAINISSU), '0')                   as PAYCHIKOKSOTAINISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSTOCKNISSU), '0')                         as PAYSTOCKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYKYOTEIWEEKNISSU), '0')                    as PAYKYOTEIWEEKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYWEEKNISSU), '0')                          as PAYWEEKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYDAIKYUNISSU), '0')                        as PAYDAIKYUNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYWORKTIME), '0')                           as PAYWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYNIGHTTIME), '0')                          as PAYNIGHTTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYORVERTIME), '0')                          as PAYORVERTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYWNIGHTTIME), '0')                         as PAYWNIGHTTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYWSWORKTIME), '0')                         as PAYWSWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSNIGHTTIME), '0')                         as PAYSNIGHTTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHWORKTIME), '0')                          as PAYHWORKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHNIGHTTIME), '0')                         as PAYHNIGHTTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYBREAKTIME), '0')                          as PAYBREAKTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYNENSHINISSU), '0')                        as PAYNENSHINISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUKCHOKNNISSU), '0')                     as PAYSHUKCHOKNNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUKCHOKNISSU), '0')                      as PAYSHUKCHOKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUKCHOKNHLDNISSU), '0')                  as PAYSHUKCHOKNHLDNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUKCHOKHLDNISSU), '0')                   as PAYSHUKCHOKHLDNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTOKSAAKAISU), '0')                        as PAYTOKSAAKAISU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTOKSABKAISU), '0')                        as PAYTOKSABKAISU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTOKSACKAISU), '0')                        as PAYTOKSACKAISU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHOANTIME), '0')                           as PAYHOANTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYKOATUTIME), '0')                          as PAYKOATUTIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTOKUSA1TIME), '0')                        as PAYTOKUSA1TIME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYPONPNISSU), '0')                          as PAYPONPNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYBULKNISSU), '0')                          as PAYBULKNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTRAILERNISSU), '0')                       as PAYTRAILERNISSU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYBKINMUKAISU), '0')                        as PAYBKINMUKAISU ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYAPPLYID), '')                             as PAYAPPLYID  ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYRIYU), '')                                as PAYRIYU  ")
            SQLStr.AppendLine("  , isnull(( ")
            SQLStr.AppendLine("        select isnull(rtrim(MC1_38.VALUE1), '')  ")
            SQLStr.AppendLine("        from MC001_FIXVALUE MC1_38  ")
            SQLStr.AppendLine("        where MC1_38.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("          and MC1_38.CLASS = 'T0009_RIYU'  ")
            SQLStr.AppendLine("          and MC1_38.KEYCODE = L01.PAYRIYU  ")
            SQLStr.AppendLine("          and MC1_38.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_38.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_38.DELFLG <> '1' ")
            SQLStr.AppendLine("    ),'')                                                         as PAYRIYUNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.PAYRIYUETC), '')                             as PAYRIYUETC  ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.WORKKBN), '')                                as WORKKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_34.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_34  ")
            SQLStr.AppendLine("      where MC1_34.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_34.CLASS = 'WORKKBN'  ")
            SQLStr.AppendLine("        and MC1_34.KEYCODE = L01.WORKKBN  ")
            SQLStr.AppendLine("        and MC1_34.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_34.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_34.DELFLG <> '1' ")
            SQLStr.AppendLine("    )                                                             as WORKKBNNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KEYSTAFFCODE), '')                           as KEYSTAFFCODE ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KEYGSHABAN), '')                             as KEYGSHABAN ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KEYTRIPNO), '')                              as KEYTRIPNO ")
            SQLStr.AppendLine("  , isnull(rtrim(L01.KEYDROPNO), '')                              as KEYDROPNO  ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      case when rtrim(L01.ACACHANTEI) in ('AMC','AMD') then '1' ")
            SQLStr.AppendLine("           else '0' end)                                          as RECODEKBN ")
            SQLStr.AppendLine("  , ( ")
            SQLStr.AppendLine("      select isnull(rtrim(MC1_37.VALUE1), '') ")
            SQLStr.AppendLine("      from MC001_FIXVALUE MC1_37  ")
            SQLStr.AppendLine("      where MC1_37.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("        and MC1_37.CLASS = 'RECODEKBN'  ")
            SQLStr.AppendLine("        and MC1_37.KEYCODE = ( ")
            SQLStr.AppendLine("              case when rtrim(L01.ACACHANTEI) in ('AMC','AMD') then '1' ")
            SQLStr.AppendLine("                   else '0' end)  ")
            SQLStr.AppendLine("        and MC1_37.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_37.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("        and MC1_37.DELFLG <> '1' ")
            SQLStr.AppendLine("     )                                                            as RECODEKBNNAME ")
            SQLStr.AppendLine(" FROM       L0001_TOKEI                        L01 ")
            SQLStr.AppendLine(" INNER JOIN M0001_CAMP                         M01              ON ")
            SQLStr.AppendLine("          M01.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("      and M01.STYMD <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("      and M01.ENDYMD >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("      and M01.DELFLG <> '1'  ")
            SQLStr.AppendLine(" INNER JOIN MB001_STAFF                        MB1              ON ")
            SQLStr.AppendLine("          MB1.CAMPCODE = L01.CAMPCODE  ")
            SQLStr.AppendLine("      and MB1.HORG     = @P09  ")
            SQLStr.AppendLine("      and MB1.STYMD   <= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("      and MB1.ENDYMD  >= L01.NACSHUKODATE  ")
            SQLStr.AppendLine("      and MB1.DELFLG  <> '1'  ")
            SQLStr.AppendLine(" WHERE  ")
            SQLStr.AppendLine("        L01.CAMPCODE = @P02  ")
            SQLStr.AppendLine("    and L01.ACKEIJOORG = @P09  ")
            SQLStr.AppendLine("    and L01.INQKBN = '1'  ")
            SQLStr.AppendLine("    and L01.NACSHUKODATE <= @P05  ")
            SQLStr.AppendLine("    and L01.NACSHUKODATE >= @P06  ")
            SQLStr.AppendLine("    and L01.KEIJOYMD <= @P07  ")
            SQLStr.AppendLine("    and L01.KEIJOYMD >= @P08  ")
            SQLStr.AppendLine("    and L01.ACACHANTEI IN ('HSC','HSD','KSC','KSD','RSC','RSD','ELC','ELD','HRC','HRD','ERC','ERD','JMC','JMD','AMC','AMD') ")
            SQLStr.AppendLine("    and L01.PAYSTAFFCODE = MB1.STAFFCODE  ")
            SQLStr.AppendLine("    and L01.DELFLG <> '1'  ")
            SQLStr.AppendLine(" ORDER BY ")
            SQLStr.AppendLine("        L01.PAYHORG, L01.PAYSTAFFCODE, L01.NACSHUKODATE, L01.ACACHANTEI DESC")

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
                '抽出条件(サーバー部署)List毎にデータ抽出
                For Each WI_ORG As String In W_ORGlst
                    '部署変換
                    Dim WW_ORG As String = ""
                    ConvORGCode(WI_ORG, WW_ORG, WW_ERRCODE)
                    If Not isNormal(WW_ERRCODE) Then
                        Exit Sub
                    End If

                    '勤怠締テーブル取得
                    Dim WW_LIMITFLG As String = "0"
                    Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                    T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                       WW_ORG,
                                       work.WF_SEL_STYM.Text,
                                       WW_LIMITFLG,
                                       WW_ERR_RTN)
                    If Not isNormal(WW_ERR_RTN) Then
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                        Exit Sub
                    End If

                    '締まっていたらサマリーテーブルから取得するためスキップする
                    If WW_LIMITFLG = "1" Then Continue For

                    Try

                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = ""
                        PARA04.Value = Date.Now
                        PARA05.Value = C_MAX_YMD
                        PARA06.Value = C_DEFAULT_YMD
                        PARA07.Value = C_MAX_YMD
                        PARA08.Value = C_DEFAULT_YMD
                        PARA09.Value = WI_ORG

                        '月末
                        Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")
                        PARA05.Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA06.Value = work.WF_SEL_STYM.Text & "/" & "01"
                        PARA07.Value = PARA05.Value
                        PARA08.Value = PARA06.Value

                        SQLcmd.CommandTimeout = 300
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            'ブレークKey
                            Dim WW_NACSHUKODATE As String = ""
                            Dim WW_PAYHORG As String = ""
                            Dim WW_PAYSTAFFCODE As String = ""
                            Dim WW_ACACHANTEI As String = ""
                            '判定Key
                            Dim wNACSHUKODATE As String = ""
                            Dim wPAYHORG As String = ""
                            Dim wPAYSTAFFCODE As String = ""
                            Dim wACACHANTEI As String = ""
                            'レコード集計
                            Dim wSUM_NACHAIDISTANCE_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離
                            Dim wSUM_NACKAIDISTANCE_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・下車作業距離
                            Dim wSUM_NACCHODISTANCE_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・勤怠調整距離
                            Dim wSUM_NACTTLDISTANCE_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離合計Σ
                            Dim wSUM_NACHAIDISTANCE_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離
                            Dim wSUM_NACKAIDISTANCE_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・下車作業距離
                            Dim wSUM_NACCHODISTANCE_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・勤怠調整距離
                            Dim wSUM_NACTTLDISTANCE_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離合計Σ
                            Dim wSUM_NACTTLDISTANCE_G As Integer = 0                                              '実績・配送距離合計Σ

                            Dim wSUM_NACHAIWORKTIME As Integer = 0                                                '実績・配送作業時間
                            Dim wSUM_NACGESWORKTIME As Integer = 0                                                '実績・下車作業時間
                            Dim wSUM_NACCHOWORKTIME As Integer = 0                                                '実績・勤怠調整時間
                            Dim wSUM_NACTTLWORKTIME As Integer = 0                                                '実績・配送合計時間Σ

                            Dim wSUM_NACOUTWORKTIME As Integer = 0                                                '実績・就業外時間

                            Dim wSUM_NACBREAKTIME As Integer = 0                                                  '実績・休憩時間
                            Dim wSUM_NACCHOBREAKTIME As Integer = 0                                               '実績・休憩調整時間
                            Dim wSUM_NACTTLBREAKTIME As Integer = 0                                               '実績・休憩合計時間Σ

                            Dim wSUM_NACUNLOADCNT_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}                 '実績・荷卸回数
                            Dim wSUM_NACCHOUNLOADCNT_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数調整
                            Dim wSUM_NACTTLUNLOADCNT_1() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数合計Σ
                            Dim wSUM_NACUNLOADCNT_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}                 '実績・荷卸回数
                            Dim wSUM_NACCHOUNLOADCNT_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数調整
                            Dim wSUM_NACTTLUNLOADCNT_2() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数合計Σ
                            Dim wSUM_NACTTLUNLOADCNT_G As Integer = 0                                             '実績・荷卸回数合計Σ
                            Dim wSUM_NACOFFICETIME As Integer = 0                                                 '実績・事務時間
                            Dim wSUM_NACOFFICEBREAKTIME As Integer = 0                                            '実績・事務休憩時間
                            Dim wSUM_PAYWORKNISSU As Integer = 0                                                  '所労
                            Dim wSUM_PAYSHOUKETUNISSU As Integer = 0                                              '傷欠
                            Dim wSUM_PAYKUMIKETUNISSU As Integer = 0                                              '組欠
                            Dim wSUM_PAYETCKETUNISSU As Integer = 0                                               '他欠
                            Dim wSUM_PAYNENKYUNISSU As Integer = 0                                                '年休
                            Dim wSUM_PAYTOKUKYUNISSU As Integer = 0                                               '特休
                            Dim wSUM_PAYCHIKOKSOTAINISSU As Integer = 0                                           '遅早
                            Dim wSUM_PAYSTOCKNISSU As Integer = 0                                                 'ストック休暇
                            Dim wSUM_PAYKYOTEIWEEKNISSU As Integer = 0                                            '協定週休
                            Dim wSUM_PAYWEEKNISSU As Integer = 0                                                  '週休
                            Dim wSUM_PAYDAIKYUNISSU As Integer = 0                                                '代休
                            Dim wSUM_PAYWORKTIME As Integer = 0                                                   '所定労働時間
                            Dim wSUM_PAYNIGHTTIME As Integer = 0                                                  '所定深夜時間
                            Dim wSUM_PAYORVERTIME As Integer = 0                                                  '平日残業時間
                            Dim wSUM_PAYWNIGHTTIME As Integer = 0                                                 '平日深夜時間
                            Dim wSUM_PAYWSWORKTIME As Integer = 0                                                 '日曜出勤時間
                            Dim wSUM_PAYSNIGHTTIME As Integer = 0                                                 '日曜深夜時間
                            Dim wSUM_PAYHWORKTIME As Integer = 0                                                  '休日出勤時間
                            Dim wSUM_PAYHNIGHTTIME As Integer = 0                                                 '休日深夜時間
                            Dim wSUM_PAYBREAKTIME As Integer = 0                                                  '休憩時間
                            Dim wSUM_PAYNENSHINISSU As Integer = 0                                                '年始出勤
                            Dim wSUM_PAYSHUKCHOKNNISSU As Integer = 0                                             '宿日直年始
                            Dim wSUM_PAYSHUKCHOKNISSU As Integer = 0                                              '宿日直通常
                            Dim wSUM_PAYSHUKCHOKNHLDNISSU As Integer = 0                                          '宿日直年始（翌休み）
                            Dim wSUM_PAYSHUKCHOKHLDNISSU As Integer = 0                                           '宿日直通常（翌休み）
                            Dim wSUM_PAYTOKSAAKAISU As Integer = 0                                                '特作A
                            Dim wSUM_PAYTOKSABKAISU As Integer = 0                                                '特作B
                            Dim wSUM_PAYTOKSACKAISU As Integer = 0                                                '特作C
                            Dim wSUM_PAYHOANTIME As Integer = 0                                                   '保安検査入力
                            Dim wSUM_PAYKOATUTIME As Integer = 0                                                  '高圧作業入力
                            Dim wSUM_PAYTOKUSA1TIME As Integer = 0                                                '特作Ⅰ
                            Dim wSUM_PAYPONPNISSU As Integer = 0                                                  'ポンプ
                            Dim wSUM_PAYBULKNISSU As Integer = 0                                                  'バルク
                            Dim wSUM_PAYTRAILERNISSU As Integer = 0                                               'トレーラ
                            Dim wSUM_PAYBKINMUKAISU As Integer = 0                                                'B勤務
                            Dim wSUM_PAYSHUSHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_PAYTAISHADATE As String = C_DEFAULT_YMD
                            Dim wSUM_NACOFFICESORG As String = ""
                            Dim wSUM_NACOFFICESORGNAME As String = ""
                            Dim wSUM_PAYKBN As String = ""
                            Dim wSUM_PAYKBNNAME As String = ""
                            Dim wSUM_PAYSHUKCHOKKBN As String = ""
                            Dim wSUM_PAYSHUKCHOKKBNNAME As String = ""
                            Dim wSUM_PAYAPPLYID As String = ""
                            Dim wSUM_PAYRIYU As String = ""
                            Dim wSUM_PAYRIYUNAME As String = ""
                            Dim wSUM_PAYRIYUETC As String = ""

                            Dim wSEQ As Integer = 0
                            Dim TA0005row As DataRow = Nothing
                            While SQLdr.Read
                                '月調整を含めない場合、AMD,AMCは除外
                                If work.WF_SEL_FUNC.Text = "0" Then
                                    If SQLdr("ACACHANTEI") = "AMD" OrElse SQLdr("ACACHANTEI") = "AMC" Then
                                        Continue While
                                    End If
                                End If

                                '〇判定Key作成
                                If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = SQLdr("NACSHUKODATE")
                                    wNACSHUKODATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACSHUKODATE = C_DEFAULT_YMD
                                End If
                                wPAYHORG = SQLdr("PAYHORG")                                                       '配属部署
                                wPAYSTAFFCODE = SQLdr("PAYSTAFFCODE")                                             '従業員
                                If SQLdr("ACACHANTEI") = "AMD" OrElse SQLdr("ACACHANTEI") = "AMC" Then
                                    wACACHANTEI = SQLdr("ACACHANTEI")                                '仕訳決定（月調整）
                                Else
                                    wACACHANTEI = ""                                                 '仕訳決定
                                End If

                                '〇Keyブレーク時のレコード設定
                                If WW_NACSHUKODATE = wNACSHUKODATE AndAlso
                                   WW_PAYHORG = wPAYHORG AndAlso
                                   WW_PAYSTAFFCODE = wPAYSTAFFCODE AndAlso
                                   WW_ACACHANTEI = wACACHANTEI Then
                                Else
                                    '〇１件目
                                    If WW_NACSHUKODATE = "" AndAlso
                                       WW_PAYHORG = "" AndAlso
                                       WW_PAYSTAFFCODE = "" AndAlso
                                       WW_ACACHANTEI = "" Then

                                    Else
                                        '〇レコード出力
                                        '合計値セット
                                        TA0005row("TAISHYM") = work.WF_SEL_STYM.Text
                                        TA0005row("NACHAIDISTANCE_1_1") = wSUM_NACHAIDISTANCE_1(0)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_1") = wSUM_NACKAIDISTANCE_1(0)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_1") = wSUM_NACCHODISTANCE_1(0)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_1") = wSUM_NACTTLDISTANCE_1(0)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_2") = wSUM_NACHAIDISTANCE_1(1)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_2") = wSUM_NACKAIDISTANCE_1(1)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_2") = wSUM_NACCHODISTANCE_1(1)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_2") = wSUM_NACTTLDISTANCE_1(1)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_3") = wSUM_NACHAIDISTANCE_1(2)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_3") = wSUM_NACKAIDISTANCE_1(2)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_3") = wSUM_NACCHODISTANCE_1(2)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_3") = wSUM_NACTTLDISTANCE_1(2)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_4") = wSUM_NACHAIDISTANCE_1(3)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_4") = wSUM_NACKAIDISTANCE_1(3)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_4") = wSUM_NACCHODISTANCE_1(3)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_4") = wSUM_NACTTLDISTANCE_1(3)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_5") = wSUM_NACHAIDISTANCE_1(4)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_5") = wSUM_NACKAIDISTANCE_1(4)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_5") = wSUM_NACCHODISTANCE_1(4)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_5") = wSUM_NACTTLDISTANCE_1(4)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_6") = wSUM_NACHAIDISTANCE_1(5)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_6") = wSUM_NACKAIDISTANCE_1(5)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_6") = wSUM_NACCHODISTANCE_1(5)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_6") = wSUM_NACTTLDISTANCE_1(5)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_7") = wSUM_NACHAIDISTANCE_1(6)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_7") = wSUM_NACKAIDISTANCE_1(6)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_7") = wSUM_NACCHODISTANCE_1(6)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_7") = wSUM_NACTTLDISTANCE_1(6)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_8") = wSUM_NACHAIDISTANCE_1(7)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_8") = wSUM_NACKAIDISTANCE_1(7)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_8") = wSUM_NACCHODISTANCE_1(7)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_8") = wSUM_NACTTLDISTANCE_1(7)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_9") = wSUM_NACHAIDISTANCE_1(8)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_9") = wSUM_NACKAIDISTANCE_1(8)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_9") = wSUM_NACCHODISTANCE_1(8)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_9") = wSUM_NACTTLDISTANCE_1(8)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_1_10") = wSUM_NACHAIDISTANCE_1(9)          '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_1_10") = wSUM_NACKAIDISTANCE_1(9)          '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_1_10") = wSUM_NACCHODISTANCE_1(9)          '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_1_10") = wSUM_NACTTLDISTANCE_1(9)          '実績・配送距離合計Σ

                                        TA0005row("NACHAIDISTANCE_2_1") = wSUM_NACHAIDISTANCE_2(0)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_1") = wSUM_NACKAIDISTANCE_2(0)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_1") = wSUM_NACCHODISTANCE_2(0)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_1") = wSUM_NACTTLDISTANCE_2(0)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_2") = wSUM_NACHAIDISTANCE_2(1)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_2") = wSUM_NACKAIDISTANCE_2(1)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_2") = wSUM_NACCHODISTANCE_2(1)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_2") = wSUM_NACTTLDISTANCE_2(1)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_3") = wSUM_NACHAIDISTANCE_2(2)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_3") = wSUM_NACKAIDISTANCE_2(2)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_3") = wSUM_NACCHODISTANCE_2(2)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_3") = wSUM_NACTTLDISTANCE_2(2)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_4") = wSUM_NACHAIDISTANCE_2(3)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_4") = wSUM_NACKAIDISTANCE_2(3)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_4") = wSUM_NACCHODISTANCE_2(3)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_4") = wSUM_NACTTLDISTANCE_2(3)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_5") = wSUM_NACHAIDISTANCE_2(4)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_5") = wSUM_NACKAIDISTANCE_2(4)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_5") = wSUM_NACCHODISTANCE_2(4)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_5") = wSUM_NACTTLDISTANCE_2(4)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_6") = wSUM_NACHAIDISTANCE_2(5)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_6") = wSUM_NACKAIDISTANCE_2(5)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_6") = wSUM_NACCHODISTANCE_2(5)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_6") = wSUM_NACTTLDISTANCE_2(5)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_7") = wSUM_NACHAIDISTANCE_2(6)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_7") = wSUM_NACKAIDISTANCE_2(6)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_7") = wSUM_NACCHODISTANCE_2(6)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_7") = wSUM_NACTTLDISTANCE_2(6)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_8") = wSUM_NACHAIDISTANCE_2(7)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_8") = wSUM_NACKAIDISTANCE_2(7)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_8") = wSUM_NACCHODISTANCE_2(7)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_8") = wSUM_NACTTLDISTANCE_2(7)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_9") = wSUM_NACHAIDISTANCE_2(8)           '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_9") = wSUM_NACKAIDISTANCE_2(8)           '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_9") = wSUM_NACCHODISTANCE_2(8)           '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_9") = wSUM_NACTTLDISTANCE_2(8)           '実績・配送距離合計Σ
                                        TA0005row("NACHAIDISTANCE_2_10") = wSUM_NACHAIDISTANCE_2(9)          '実績・配送距離
                                        TA0005row("NACKAIDISTANCE_2_10") = wSUM_NACKAIDISTANCE_2(9)          '実績・下車作業距離
                                        TA0005row("NACCHODISTANCE_2_10") = wSUM_NACCHODISTANCE_2(9)          '実績・勤怠調整距離
                                        TA0005row("NACTTLDISTANCE_2_10") = wSUM_NACTTLDISTANCE_2(9)          '実績・配送距離合計Σ
                                        For i As Integer = 0 To 9
                                            wSUM_NACTTLDISTANCE_G += wSUM_NACTTLDISTANCE_1(i) + wSUM_NACTTLDISTANCE_2(i)
                                        Next
                                        TA0005row("NACTTLDISTANCE_G") = wSUM_NACTTLDISTANCE_G                '実績・配送距離合計Σ
                                        TA0005row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                        TA0005row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                        TA0005row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                        TA0005row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                        TA0005row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                        TA0005row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                        TA0005row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                        TA0005row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                        TA0005row("NACUNLOADCNT_1_1") = wSUM_NACUNLOADCNT_1(0)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_1") = wSUM_NACCHOUNLOADCNT_1(0)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_2") = wSUM_NACUNLOADCNT_1(1)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_2") = wSUM_NACCHOUNLOADCNT_1(1)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_3") = wSUM_NACUNLOADCNT_1(2)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_3") = wSUM_NACCHOUNLOADCNT_1(2)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_4") = wSUM_NACUNLOADCNT_1(3)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_4") = wSUM_NACCHOUNLOADCNT_1(3)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_5") = wSUM_NACUNLOADCNT_1(4)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_5") = wSUM_NACCHOUNLOADCNT_1(4)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_6") = wSUM_NACUNLOADCNT_1(5)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_6") = wSUM_NACCHOUNLOADCNT_1(5)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_7") = wSUM_NACUNLOADCNT_1(6)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_7") = wSUM_NACCHOUNLOADCNT_1(6)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_8") = wSUM_NACUNLOADCNT_1(7)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_8") = wSUM_NACCHOUNLOADCNT_1(7)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_9") = wSUM_NACUNLOADCNT_1(8)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_9") = wSUM_NACCHOUNLOADCNT_1(8)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_1_10") = wSUM_NACUNLOADCNT_1(9)                             '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_1_10") = wSUM_NACCHOUNLOADCNT_1(9)                       '実績・荷卸回数調整

                                        TA0005row("NACUNLOADCNT_2_1") = wSUM_NACUNLOADCNT_2(0)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_1") = wSUM_NACCHOUNLOADCNT_2(0)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_2") = wSUM_NACUNLOADCNT_2(1)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_2") = wSUM_NACCHOUNLOADCNT_2(1)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_3") = wSUM_NACUNLOADCNT_2(2)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_3") = wSUM_NACCHOUNLOADCNT_2(2)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_4") = wSUM_NACUNLOADCNT_2(3)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_4") = wSUM_NACCHOUNLOADCNT_2(3)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_5") = wSUM_NACUNLOADCNT_2(4)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_5") = wSUM_NACCHOUNLOADCNT_2(4)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_6") = wSUM_NACUNLOADCNT_2(5)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_6") = wSUM_NACCHOUNLOADCNT_2(5)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_7") = wSUM_NACUNLOADCNT_2(6)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_7") = wSUM_NACCHOUNLOADCNT_2(6)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_8") = wSUM_NACUNLOADCNT_2(7)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_8") = wSUM_NACCHOUNLOADCNT_2(7)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_9") = wSUM_NACUNLOADCNT_2(8)                              '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_9") = wSUM_NACCHOUNLOADCNT_2(8)                        '実績・荷卸回数調整
                                        TA0005row("NACUNLOADCNT_2_10") = wSUM_NACUNLOADCNT_2(9)                             '実績・荷卸回数
                                        TA0005row("NACCHOUNLOADCNT_2_10") = wSUM_NACCHOUNLOADCNT_2(9)                       '実績・荷卸回数調整
                                        For i As Integer = 0 To 9
                                            wSUM_NACTTLUNLOADCNT_G += wSUM_NACUNLOADCNT_1(i) + wSUM_NACCHOUNLOADCNT_1(i) + wSUM_NACUNLOADCNT_2(i) + wSUM_NACCHOUNLOADCNT_2(i)
                                        Next
                                        TA0005row("NACTTLUNLOADCNT_G") = wSUM_NACTTLUNLOADCNT_G                             '実績・荷卸回数合計Σ
                                        TA0005row("NACOFFICETIME") = wSUM_NACOFFICETIME                                     '実績・従業時間
                                        TA0005row("NACOFFICEBREAKTIME") = wSUM_NACOFFICEBREAKTIME                           '実績・従業休憩時間

                                        TA0005row("NACOFFICESORG") = wSUM_NACOFFICESORG                                     '実績・作業部署
                                        TA0005row("NACOFFICESORGNAME") = wSUM_NACOFFICESORGNAME                             '実績・作業部署名称
                                        TA0005row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                        TA0005row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                        TA0005row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                        TA0005row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                        TA0005row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                        TA0005row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時
                                        Try
                                            Dim wMin As Integer = DateDiff("n", wSUM_PAYSHUSHADATE, wSUM_PAYTAISHADATE)
                                            TA0005row("WORKTIME") = MinutesToHHMM(wMin)                                        '拘束時間
                                            TA0005row("WORKTIMEMIN") = wMin                                                 '拘束時間（分)
                                            If wMin >= 960 Then
                                                TA0005row("WORKTIMEMIN16UP") = 1                                            '拘束時間１６時間超（回数)
                                            Else
                                                TA0005row("WORKTIMEMIN16UP") = 0                                            '拘束時間１６時間超（回数)
                                            End If
                                        Catch ex As Exception
                                            TA0005row("WORKTIME") = "00:00"
                                            TA0005row("WORKTIMEMIN") = 0
                                            TA0005row("WORKTIMEMIN16UP") = 0                                                '拘束時間１６時間超（回数)
                                        End Try
                                        If wSUM_PAYWORKNISSU = 0 AndAlso TA0005row("PAYHOLIDAYKBN") = "0" Then
                                            TA0005row("PAYWORKNISSU") = 1                                                   '所労
                                        Else
                                            TA0005row("PAYWORKNISSU") = wSUM_PAYWORKNISSU                                   '所労
                                        End If
                                        TA0005row("PAYSHOUKETUNISSU") = wSUM_PAYSHOUKETUNISSU                               '傷欠
                                        TA0005row("PAYKUMIKETUNISSU") = wSUM_PAYKUMIKETUNISSU                               '組欠
                                        TA0005row("PAYETCKETUNISSU") = wSUM_PAYETCKETUNISSU                                 '他欠
                                        TA0005row("PAYNENKYUNISSU") = wSUM_PAYNENKYUNISSU                                   '年休
                                        TA0005row("PAYTOKUKYUNISSU") = wSUM_PAYTOKUKYUNISSU                                 '特休
                                        TA0005row("PAYCHIKOKSOTAINISSU") = wSUM_PAYCHIKOKSOTAINISSU                         '遅早
                                        TA0005row("PAYSTOCKNISSU") = wSUM_PAYSTOCKNISSU                                     'ストック休暇
                                        TA0005row("PAYKYOTEIWEEKNISSU") = wSUM_PAYKYOTEIWEEKNISSU                           '協定週休
                                        TA0005row("PAYWEEKNISSU") = wSUM_PAYWEEKNISSU                                       '週休
                                        TA0005row("PAYDAIKYUNISSU") = wSUM_PAYDAIKYUNISSU                                   '代休
                                        TA0005row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                        TA0005row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                        TA0005row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                        TA0005row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                        TA0005row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                        TA0005row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                        TA0005row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                        TA0005row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                        TA0005row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                        TA0005row("PAYNENSHINISSU") = wSUM_PAYNENSHINISSU                                   '年始出勤
                                        TA0005row("PAYSHUKCHOKNNISSU") = wSUM_PAYSHUKCHOKNNISSU                             '宿日直年始
                                        TA0005row("PAYSHUKCHOKNISSU") = wSUM_PAYSHUKCHOKNISSU                               '宿日直通常
                                        TA0005row("PAYSHUKCHOKNHLDNISSU") = wSUM_PAYSHUKCHOKNHLDNISSU                       '宿日直年始（翌休み）
                                        TA0005row("PAYSHUKCHOKHLDNISSU") = wSUM_PAYSHUKCHOKHLDNISSU                         '宿日直通常（翌休み）
                                        TA0005row("PAYTOKSAAKAISU") = wSUM_PAYTOKSAAKAISU                                   '特作A
                                        TA0005row("PAYTOKSABKAISU") = wSUM_PAYTOKSABKAISU                                   '特作B
                                        TA0005row("PAYTOKSACKAISU") = wSUM_PAYTOKSACKAISU                                   '特作C
                                        TA0005row("PAYHOANTIME") = wSUM_PAYHOANTIME                                         '保安検査入力
                                        TA0005row("PAYKOATUTIME") = wSUM_PAYKOATUTIME                                       '高圧作業入力
                                        TA0005row("PAYTOKUSA1TIME") = wSUM_PAYTOKUSA1TIME                                   '特作Ⅰ
                                        TA0005row("PAYPONPNISSU") = wSUM_PAYPONPNISSU                                       'ポンプ
                                        TA0005row("PAYBULKNISSU") = wSUM_PAYBULKNISSU                                       'バルク
                                        TA0005row("PAYTRAILERNISSU") = wSUM_PAYTRAILERNISSU                                 'トレーラ
                                        TA0005row("PAYBKINMUKAISU") = wSUM_PAYBKINMUKAISU                                   'B勤務
                                        TA0005row("PAYAPPLYID") = wSUM_PAYAPPLYID                                           '申請ID
                                        TA0005row("PAYRIYU") = wSUM_PAYRIYU                                                 '理由
                                        TA0005row("PAYRIYUNAME") = wSUM_PAYRIYUNAME                                         '理由
                                        TA0005row("PAYRIYUETC") = wSUM_PAYRIYUETC                                           '理由その他

                                        TA0005tbl.Rows.Add(TA0005row)
                                    End If

                                    '〇新レコード準備(固定項目設定)
                                    TA0005row = TA0005tbl.NewRow

                                    wSEQ = 0

                                    'ブレイクキー設定
                                    WW_NACSHUKODATE = wNACSHUKODATE
                                    WW_PAYHORG = wPAYHORG
                                    WW_PAYSTAFFCODE = wPAYSTAFFCODE
                                    WW_ACACHANTEI = wACACHANTEI

                                    '合計項目クリア
                                    wSUM_NACHAIDISTANCE_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離
                                    wSUM_NACKAIDISTANCE_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・下車作業距離
                                    wSUM_NACCHODISTANCE_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・勤怠調整距離
                                    wSUM_NACTTLDISTANCE_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離合計Σ
                                    wSUM_NACHAIDISTANCE_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離
                                    wSUM_NACKAIDISTANCE_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・下車作業距離
                                    wSUM_NACCHODISTANCE_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・勤怠調整距離
                                    wSUM_NACTTLDISTANCE_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}               '実績・配送距離合計Σ
                                    wSUM_NACUNLOADCNT_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}                 '実績・荷卸回数
                                    wSUM_NACCHOUNLOADCNT_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数調整
                                    wSUM_NACTTLUNLOADCNT_1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数合計Σ
                                    wSUM_NACUNLOADCNT_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}                 '実績・荷卸回数
                                    wSUM_NACCHOUNLOADCNT_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数調整
                                    wSUM_NACTTLUNLOADCNT_2 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}              '実績・荷卸回数合計Σ
                                    wSUM_NACTTLDISTANCE_G = 0                                                      '実績・配送距離合計Σ
                                    wSUM_NACTTLUNLOADCNT_G = 0                                                     '実績・荷卸回数合計Σ

                                    wSUM_NACHAIWORKTIME = 0                                                         '実績・配送作業時間
                                    wSUM_NACGESWORKTIME = 0                                                         '実績・下車作業時間
                                    wSUM_NACCHOWORKTIME = 0                                                         '実績・勤怠調整時間
                                    wSUM_NACTTLWORKTIME = 0                                                         '実績・配送合計時間Σ
                                    wSUM_NACOUTWORKTIME = 0                                                         '実績・就業外時間
                                    wSUM_NACBREAKTIME = 0                                                           '実績・休憩時間
                                    wSUM_NACCHOBREAKTIME = 0                                                        '実績・休憩調整時間
                                    wSUM_NACTTLBREAKTIME = 0                                                        '実績・休憩合計時間Σ
                                    wSUM_NACOFFICETIME = 0                                                          '実績・従業時間
                                    wSUM_NACOFFICEBREAKTIME = 0                                                     '実績・従業休憩時間
                                    wSUM_PAYWORKNISSU = 0                                                           '所労
                                    wSUM_PAYSHOUKETUNISSU = 0                                                       '傷欠
                                    wSUM_PAYKUMIKETUNISSU = 0                                                       '組欠
                                    wSUM_PAYETCKETUNISSU = 0                                                        '他欠
                                    wSUM_PAYNENKYUNISSU = 0                                                         '年休
                                    wSUM_PAYTOKUKYUNISSU = 0                                                        '特休
                                    wSUM_PAYCHIKOKSOTAINISSU = 0                                                    '遅早
                                    wSUM_PAYSTOCKNISSU = 0                                                          'ストック休暇
                                    wSUM_PAYKYOTEIWEEKNISSU = 0                                                     '協定週休
                                    wSUM_PAYWEEKNISSU = 0                                                           '週休
                                    wSUM_PAYDAIKYUNISSU = 0                                                         '代休
                                    wSUM_PAYWORKTIME = 0                                                            '所定労働時間
                                    wSUM_PAYNIGHTTIME = 0                                                           '所定深夜時間
                                    wSUM_PAYORVERTIME = 0                                                           '平日残業時間
                                    wSUM_PAYWNIGHTTIME = 0                                                          '平日深夜時間
                                    wSUM_PAYWSWORKTIME = 0                                                          '日曜出勤時間
                                    wSUM_PAYSNIGHTTIME = 0                                                          '日曜深夜時間
                                    wSUM_PAYHWORKTIME = 0                                                           '休日出勤時間
                                    wSUM_PAYHNIGHTTIME = 0                                                          '休日深夜時間
                                    wSUM_PAYBREAKTIME = 0                                                           '休憩時間
                                    wSUM_PAYNENSHINISSU = 0                                                         '年始出勤
                                    wSUM_PAYSHUKCHOKNNISSU = 0                                                      '宿日直年始
                                    wSUM_PAYSHUKCHOKNISSU = 0                                                       '宿日直通常
                                    wSUM_PAYSHUKCHOKNHLDNISSU = 0                                                   '宿日直年始（翌休み）
                                    wSUM_PAYSHUKCHOKHLDNISSU = 0                                                    '宿日直通常（翌休み）
                                    wSUM_PAYTOKSAAKAISU = 0                                                         '特作A
                                    wSUM_PAYTOKSABKAISU = 0                                                         '特作B
                                    wSUM_PAYTOKSACKAISU = 0                                                         '特作C
                                    wSUM_PAYHOANTIME = 0                                                            '保安検査入力
                                    wSUM_PAYKOATUTIME = 0                                                           '高圧作業入力
                                    wSUM_PAYTOKUSA1TIME = 0                                                         '特作Ⅰ
                                    wSUM_PAYPONPNISSU = 0                                                           'ポンプ
                                    wSUM_PAYBULKNISSU = 0                                                           'バルク
                                    wSUM_PAYTRAILERNISSU = 0                                                        'トレーラ
                                    wSUM_PAYBKINMUKAISU = 0                                                           'B勤務
                                    wSUM_NACOFFICESORG = ""
                                    wSUM_NACOFFICESORGNAME = ""
                                    wSUM_PAYKBN = ""
                                    wSUM_PAYKBNNAME = ""
                                    wSUM_PAYSHUKCHOKKBN = ""
                                    wSUM_PAYSHUKCHOKKBNNAME = ""
                                    wSUM_PAYAPPLYID = ""
                                    wSUM_PAYRIYU = ""
                                    wSUM_PAYRIYUNAME = ""
                                    wSUM_PAYRIYUETC = ""

                                    '固定項目
                                    TA0005row("LINECNT") = 0                                                        'DBの固定フィールド(2017/11/9)
                                    TA0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA                           'DBの固定フィールド(2017/11/9)
                                    TA0005row("TIMSTP") = 0                                                         'DBの固定フィールド(2017/11/9)
                                    TA0005row("SELECT") = "0"                                                       'DBの固定フィールド(2017/11/9)
                                    TA0005row("HIDDEN") = 0                                                         'DBの固定フィールド(2017/11/9)

                                    '画面固有項目
                                    TA0005row("CAMPCODE") = SQLdr("CAMPCODE")                                       '会社
                                    TA0005row("CAMPNAME") = SQLdr("CAMPNAME")                                       '会社名称
                                    If IsDate(SQLdr("KEIJOYMD")) AndAlso SQLdr("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                        wDATE = SQLdr("KEIJOYMD")
                                        TA0005row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0005row("KEIJOYMD") = C_DEFAULT_YMD
                                    End If
                                    If IsDate(SQLdr("DENYMD")) AndAlso SQLdr("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                        wDATE = SQLdr("DENYMD")
                                        TA0005row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0005row("DENYMD") = C_DEFAULT_YMD
                                    End If
                                    TA0005row("DENNO") = SQLdr("DENNO")                                             '伝票番号
                                    TA0005row("KANRENDENNO") = SQLdr("KANRENDENNO")                                 '関連伝票No＋明細No
                                    TA0005row("DTLNO") = SQLdr("DTLNO")                                             '明細番号
                                    TA0005row("ACACHANTEI") = SQLdr("ACACHANTEI")                                   '仕訳決定
                                    TA0005row("ACACHANTEINAME") = SQLdr("ACACHANTEINAME")                           '仕訳決定名称
                                    If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                        wDATE = SQLdr("NACSHUKODATE")
                                        TA0005row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0005row("NACSHUKODATE") = C_DEFAULT_YMD
                                    End If

                                    TA0005row("WORKKBN") = SQLdr("WORKKBN")                                         'SYS作業区分
                                    TA0005row("WORKKBNNAME") = SQLdr("WORKKBNNAME")                                 'SYS作業区分名称
                                    TA0005row("KEYSTAFFCODE") = SQLdr("KEYSTAFFCODE")                               'SYS従業員
                                    TA0005row("KEYGSHABAN") = SQLdr("KEYGSHABAN")                                   'SYS業務車番
                                    TA0005row("KEYTRIPNO") = SQLdr("KEYTRIPNO")                                     'SYSトリップ
                                    TA0005row("KEYDROPNO") = SQLdr("KEYDROPNO")                                     'SYSドロップ
                                    TA0005row("RECODEKBN") = SQLdr("RECODEKBN")                                     'SYSレコード区分
                                    TA0005row("RECODEKBNNAME") = SQLdr("RECODEKBNNAME")                             'SYSレコード区分名称

                                    TA0005row("NACHAIDISTANCE_1_1") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_1") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_1") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_1") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_2") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_2") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_2") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_2") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_3") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_3") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_3") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_3") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_4") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_4") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_4") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_4") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_5") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_5") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_5") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_5") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_6") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_6") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_6") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_6") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_7") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_7") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_7") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_7") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_8") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_8") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_8") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_8") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_9") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_9") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_9") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_9") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_1_10") = ""                                           '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_1_10") = ""                                           '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_1_10") = ""                                           '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_1_10") = ""                                           '実績・配送距離合計Σ

                                    TA0005row("NACHAIDISTANCE_2_1") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_1") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_1") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_1") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_2") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_2") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_2") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_2") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_3") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_3") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_3") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_3") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_4") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_4") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_4") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_4") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_5") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_5") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_5") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_5") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_6") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_6") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_6") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_6") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_7") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_7") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_7") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_7") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_8") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_8") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_8") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_8") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_9") = ""                                            '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_9") = ""                                            '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_9") = ""                                            '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_9") = ""                                            '実績・配送距離合計Σ
                                    TA0005row("NACHAIDISTANCE_2_10") = ""                                           '実績・配送距離
                                    TA0005row("NACKAIDISTANCE_2_10") = ""                                           '実績・下車作業距離
                                    TA0005row("NACCHODISTANCE_2_10") = ""                                           '実績・勤怠調整距離
                                    TA0005row("NACTTLDISTANCE_2_10") = ""                                           '実績・配送距離合計Σ
                                    TA0005row("NACTTLDISTANCE_G") = ""                                              '実績・配送距離合計Σ

                                    TA0005row("NACHAIWORKTIME") = ""                                                '実績・配送作業時間
                                    TA0005row("NACGESWORKTIME") = ""                                                '実績・下車作業時間
                                    TA0005row("NACCHOWORKTIME") = ""                                                '実績・勤怠調整時間
                                    TA0005row("NACTTLWORKTIME") = ""                                                '実績・配送合計時間Σ
                                    TA0005row("NACOUTWORKTIME") = ""                                                '実績・就業外時間
                                    TA0005row("NACBREAKTIME") = ""                                                  '実績・休憩時間
                                    TA0005row("NACCHOBREAKTIME") = ""                                               '実績・休憩調整時間
                                    TA0005row("NACTTLBREAKTIME") = ""                                               '実績・休憩合計時間Σ
                                    TA0005row("NACUNLOADCNT_1_1") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_1") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_2") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_2") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_3") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_3") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_4") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_4") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_5") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_5") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_6") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_6") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_7") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_7") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_8") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_8") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_9") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_9") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_1_10") = ""                                             '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_1_10") = ""                                          '実績・荷卸回数調整

                                    TA0005row("NACUNLOADCNT_2_1") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_1") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_2") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_2") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_3") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_3") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_4") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_4") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_5") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_5") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_6") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_6") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_7") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_7") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_8") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_8") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_9") = ""                                              '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_9") = ""                                           '実績・荷卸回数調整
                                    TA0005row("NACUNLOADCNT_2_10") = ""                                             '実績・荷卸回数
                                    TA0005row("NACCHOUNLOADCNT_2_10") = ""                                          '実績・荷卸回数調整
                                    TA0005row("NACTTLUNLOADCNT_G") = ""                                             '実績・荷卸回数合計Σ
                                    TA0005row("NACOFFICETIME") = ""                                                 '実績・従業時間
                                    TA0005row("NACOFFICEBREAKTIME") = ""                                            '実績・従業休憩時間
                                    TA0005row("PAYWORKNISSU") = ""                                                  '所労
                                    TA0005row("PAYSHOUKETUNISSU") = ""                                              '傷欠
                                    TA0005row("PAYKUMIKETUNISSU") = ""                                              '組欠
                                    TA0005row("PAYETCKETUNISSU") = ""                                               '他欠
                                    TA0005row("PAYNENKYUNISSU") = ""                                                '年休
                                    TA0005row("PAYTOKUKYUNISSU") = ""                                               '特休
                                    TA0005row("PAYCHIKOKSOTAINISSU") = ""                                           '遅早
                                    TA0005row("PAYSTOCKNISSU") = ""                                                 'ストック休暇
                                    TA0005row("PAYKYOTEIWEEKNISSU") = ""                                            '協定週休
                                    TA0005row("PAYWEEKNISSU") = ""                                                  '週休
                                    TA0005row("PAYDAIKYUNISSU") = ""                                                '代休
                                    TA0005row("PAYWORKTIME") = ""                                                   '所定労働時間
                                    TA0005row("PAYNIGHTTIME") = ""                                                  '所定深夜時間
                                    TA0005row("PAYORVERTIME") = ""                                                  '平日残業時間
                                    TA0005row("PAYWNIGHTTIME") = ""                                                 '平日深夜時間
                                    TA0005row("PAYWSWORKTIME") = ""                                                 '日曜出勤時間
                                    TA0005row("PAYSNIGHTTIME") = ""                                                 '日曜深夜時間
                                    TA0005row("PAYHWORKTIME") = ""                                                  '休日出勤時間
                                    TA0005row("PAYHNIGHTTIME") = ""                                                 '休日深夜時間
                                    TA0005row("PAYBREAKTIME") = ""                                                  '休憩時間
                                    TA0005row("PAYNENSHINISSU") = ""                                                '年始出勤
                                    TA0005row("PAYSHUKCHOKNNISSU") = ""                                             '宿日直年始
                                    TA0005row("PAYSHUKCHOKNISSU") = ""                                              '宿日直通常
                                    TA0005row("PAYSHUKCHOKNHLDNISSU") = ""                                          '宿日直年始（翌休み）
                                    TA0005row("PAYSHUKCHOKHLDNISSU") = ""                                           '宿日直通常（翌休み）
                                    TA0005row("PAYTOKSAAKAISU") = ""                                                '特作A
                                    TA0005row("PAYTOKSABKAISU") = ""                                                '特作B
                                    TA0005row("PAYTOKSACKAISU") = ""                                                '特作C
                                    TA0005row("PAYHOANTIME") = ""                                                   '保安検査入力
                                    TA0005row("PAYKOATUTIME") = ""                                                  '高圧作業入力
                                    TA0005row("PAYTOKUSA1TIME") = ""                                                '特作Ⅰ
                                    TA0005row("PAYPONPNISSU") = ""                                                  'ポンプ
                                    TA0005row("PAYBULKNISSU") = ""                                                  'バルク
                                    TA0005row("PAYTRAILERNISSU") = ""                                               'トレーラ
                                    TA0005row("PAYBKINMUKAISU") = ""                                                'B勤務
                                    TA0005row("PAYAPPLYID") = ""                                                    '申請ID
                                    TA0005row("PAYRIYU") = ""                                                       '理由
                                    TA0005row("PAYRIYUNAME") = ""                                                   '理由
                                    TA0005row("PAYRIYUETC") = ""                                                    '理由その他

                                    TA0005row("PAYSHARYOKBN_1") = ""                                               '勤怠用車両区分
                                    TA0005row("PAYSHARYOKBNNAME_1") = ""                                           '勤怠用油車両分名称
                                    TA0005row("PAYOILKBN_1_1") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_1") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_2") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_2") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_3") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_3") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_4") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_4") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_5") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_5") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_6") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_6") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_7") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_7") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_8") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_8") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_9") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_9") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_1_10") = ""                                               '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_1_10") = ""                                           '勤怠用油種区分名称

                                    TA0005row("PAYSHARYOKBN_2") = ""                                               '勤怠用車両区分
                                    TA0005row("PAYSHARYOKBNNAME_2") = ""                                           '勤怠用油車両分名称
                                    TA0005row("PAYOILKBN_2_1") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_1") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_2") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_2") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_3") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_3") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_4") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_4") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_5") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_5") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_6") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_6") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_7") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_7") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_8") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_8") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_9") = ""                                                '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_9") = ""                                            '勤怠用油種区分名称
                                    TA0005row("PAYOILKBN_2_10") = ""                                               '勤怠用油種区分
                                    TA0005row("PAYOILKBNNAME_2_10") = ""                                           '勤怠用油種区分名称

                                End If

                                TA0005row("NACHAISTDATE") = SQLdr("NACHAISTDATE")                               '実績・配送作業開始日時
                                TA0005row("NACHAIENDDATE") = SQLdr("NACHAIENDDATE")                             '実績・配送作業終了日時

                                TA0005row("NACGESSTDATE") = SQLdr("NACGESSTDATE")                               '実績・下車作業開始日時
                                TA0005row("NACGESENDDATE") = SQLdr("NACGESENDDATE")                             '実績・下車作業終了日時

                                TA0005row("NACBREAKSTDATE") = SQLdr("NACBREAKSTDATE")                           '実績・休憩開始日時
                                TA0005row("NACBREAKENDDATE") = SQLdr("NACBREAKENDDATE")                         '実績・休憩終了日時

                                TA0005row("NACOFFICESORG") = SQLdr("NACOFFICESORG")                             '実績・従業作業部署
                                TA0005row("NACOFFICESORGNAME") = SQLdr("NACOFFICESORGNAME")                     '実績・従業作業部署名称

                                TA0005row("PAYSHUSHADATE") = SQLdr("PAYSHUSHADATE")
                                TA0005row("PAYTAISHADATE") = SQLdr("PAYTAISHADATE")
                                TA0005row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                TA0005row("PAYSTAFFKBNNAME") = SQLdr("PAYSTAFFKBNNAME")                         '社員区分名称
                                TA0005row("PAYSTAFFCODE") = SQLdr("PAYSTAFFCODE")                               '従業員
                                TA0005row("PAYSTAFFCODENAME") = SQLdr("PAYSTAFFCODENAME")                       '従業員名称
                                TA0005row("PAYMORG") = SQLdr("PAYMORG")                                         '従業員管理部署
                                TA0005row("PAYMORGNAME") = SQLdr("PAYMORGNAME")                                 '従業員管理部署名称
                                TA0005row("PAYHORG") = SQLdr("PAYHORG")                                         '従業員配属部署
                                TA0005row("PAYHORGNAME") = SQLdr("PAYHORGNAME")                                 '従業員配属部署名称
                                TA0005row("PAYHOLIDAYKBN") = SQLdr("PAYHOLIDAYKBN")                             '休日区分
                                TA0005row("PAYHOLIDAYKBNNAME") = SQLdr("PAYHOLIDAYKBNNAME")                     '休日区分名称
                                TA0005row("PAYKBN") = SQLdr("PAYKBN")                                           '勤怠区分
                                TA0005row("PAYKBNNAME") = SQLdr("PAYKBNNAME")                                   '勤怠区分名称
                                TA0005row("PAYSHUKCHOKKBN") = SQLdr("PAYSHUKCHOKKBN")                           '宿日直区分
                                TA0005row("PAYSHUKCHOKKBNNAME") = SQLdr("PAYSHUKCHOKKBNNAME")                   '宿日直区分名称
                                TA0005row("PAYJYOMUKBN") = SQLdr("PAYJYOMUKBN")                                 '乗務区分
                                TA0005row("PAYJYOMUKBNNAME") = SQLdr("PAYJYOMUKBNNAME")                         '乗務区分名称

                                '実績・配送作業開始日時
                                If IsDate(SQLdr("NACHAISTDATE")) AndAlso SQLdr("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACHAISTDATE")
                                    TA0005row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACHAISTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・配送作業終了日時
                                If IsDate(SQLdr("NACHAIENDDATE")) AndAlso SQLdr("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACHAIENDDATE")
                                    TA0005row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACHAIENDDATE") = C_DEFAULT_YMD
                                End If

                                '実績・下車作業開始日時
                                If IsDate(SQLdr("NACGESSTDATE")) AndAlso SQLdr("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACGESSTDATE")
                                    TA0005row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACGESSTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・下車作業終了日時
                                If IsDate(SQLdr("NACGESENDDATE")) AndAlso SQLdr("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACGESENDDATE")
                                    TA0005row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACGESENDDATE") = C_DEFAULT_YMD
                                End If

                                '休憩開始日時
                                If IsDate(SQLdr("NACBREAKSTDATE")) AndAlso SQLdr("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACBREAKSTDATE")
                                    TA0005row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                End If

                                '休憩終了日時
                                If IsDate(SQLdr("NACBREAKENDDATE")) AndAlso SQLdr("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = SQLdr("NACBREAKENDDATE")
                                    TA0005row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0005row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                End If

                                If SQLdr("ACACHANTEI") = "AMD" OrElse SQLdr("ACACHANTEI") = "HSD" OrElse
                                   SQLdr("ACACHANTEI") = "AMC" OrElse SQLdr("ACACHANTEI") = "HSC" Then
                                    '単車
                                    If SQLdr("PAYSHARYOKBN") = "1" Then
                                        TA0005row("PAYSHARYOKBN_1") = SQLdr("PAYSHARYOKBN")                      '勤怠用車両区分
                                        TA0005row("PAYSHARYOKBNNAME_1") = SQLdr("PAYSHARYOKBNNAME")              '勤怠用油車両分名称

                                        Select Case SQLdr("PAYOILKBN")
                                            Case "01"
                                                TA0005row("PAYOILKBN_1_1") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_1") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(0) = wSUM_NACHAIDISTANCE_1(0) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(0) = wSUM_NACKAIDISTANCE_1(0) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(0) = wSUM_NACHAIDISTANCE_1(0) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(0) = wSUM_NACTTLDISTANCE_1(0) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(0) = wSUM_NACUNLOADCNT_1(0) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(0) = wSUM_NACUNLOADCNT_1(0) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "02"
                                                TA0005row("PAYOILKBN_1_2") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_2") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(1) = wSUM_NACHAIDISTANCE_1(1) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(1) = wSUM_NACKAIDISTANCE_1(1) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(1) = wSUM_NACHAIDISTANCE_1(1) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(1) = wSUM_NACTTLDISTANCE_1(1) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(1) = wSUM_NACUNLOADCNT_1(1) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(1) = wSUM_NACUNLOADCNT_1(1) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "03"
                                                TA0005row("PAYOILKBN_1_3") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_3") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(2) = wSUM_NACHAIDISTANCE_1(2) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(2) = wSUM_NACKAIDISTANCE_1(2) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(2) = wSUM_NACHAIDISTANCE_1(2) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(2) = wSUM_NACTTLDISTANCE_1(2) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(2) = wSUM_NACUNLOADCNT_1(2) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(2) = wSUM_NACUNLOADCNT_1(2) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "04"
                                                TA0005row("PAYOILKBN_1_4") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_4") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(3) = wSUM_NACHAIDISTANCE_1(3) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(3) = wSUM_NACKAIDISTANCE_1(3) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(3) = wSUM_NACHAIDISTANCE_1(3) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(3) = wSUM_NACTTLDISTANCE_1(3) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(3) = wSUM_NACUNLOADCNT_1(3) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(3) = wSUM_NACUNLOADCNT_1(3) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "05"
                                                TA0005row("PAYOILKBN_1_5") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_5") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(4) = wSUM_NACHAIDISTANCE_1(4) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(4) = wSUM_NACKAIDISTANCE_1(4) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(4) = wSUM_NACHAIDISTANCE_1(4) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(4) = wSUM_NACTTLDISTANCE_1(4) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(4) = wSUM_NACUNLOADCNT_1(4) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(4) = wSUM_NACUNLOADCNT_1(4) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "06"
                                                TA0005row("PAYOILKBN_1_6") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_6") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(5) = wSUM_NACHAIDISTANCE_1(5) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(5) = wSUM_NACKAIDISTANCE_1(5) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(5) = wSUM_NACHAIDISTANCE_1(5) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(5) = wSUM_NACTTLDISTANCE_1(5) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(5) = wSUM_NACUNLOADCNT_1(5) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(5) = wSUM_NACUNLOADCNT_1(5) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "07"
                                                TA0005row("PAYOILKBN_1_7") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_7") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(6) = wSUM_NACHAIDISTANCE_1(6) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(6) = wSUM_NACKAIDISTANCE_1(6) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(6) = wSUM_NACHAIDISTANCE_1(6) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(6) = wSUM_NACTTLDISTANCE_1(6) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(6) = wSUM_NACUNLOADCNT_1(6) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(6) = wSUM_NACUNLOADCNT_1(6) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "08"
                                                TA0005row("PAYOILKBN_1_8") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_8") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(7) = wSUM_NACHAIDISTANCE_1(7) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(7) = wSUM_NACKAIDISTANCE_1(7) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(7) = wSUM_NACHAIDISTANCE_1(7) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(7) = wSUM_NACTTLDISTANCE_1(7) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(7) = wSUM_NACUNLOADCNT_1(7) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(7) = wSUM_NACUNLOADCNT_1(7) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "09"
                                                TA0005row("PAYOILKBN_1_9") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_9") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(8) = wSUM_NACHAIDISTANCE_1(8) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(8) = wSUM_NACKAIDISTANCE_1(8) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(8) = wSUM_NACHAIDISTANCE_1(8) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(8) = wSUM_NACTTLDISTANCE_1(8) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(8) = wSUM_NACUNLOADCNT_1(8) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(8) = wSUM_NACUNLOADCNT_1(8) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "10"
                                                TA0005row("PAYOILKBN_1_10") = SQLdr("PAYOILKBN")                  '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_1_10") = SQLdr("PAYOILKBNNAME")           '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_1(9) = wSUM_NACHAIDISTANCE_1(9) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_1(9) = wSUM_NACKAIDISTANCE_1(9) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_1(9) = wSUM_NACHAIDISTANCE_1(9) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_1(9) = wSUM_NACTTLDISTANCE_1(9) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_1(9) = wSUM_NACUNLOADCNT_1(9) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_1(9) = wSUM_NACUNLOADCNT_1(9) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                        End Select

                                    End If

                                    'トレーラ
                                    If SQLdr("PAYSHARYOKBN") = "2" Then
                                        TA0005row("PAYSHARYOKBN_2") = SQLdr("PAYSHARYOKBN")                      '勤怠用車両区分
                                        TA0005row("PAYSHARYOKBNNAME_2") = SQLdr("PAYSHARYOKBNNAME")              '勤怠用油車両分名称

                                        Select Case SQLdr("PAYOILKBN")
                                            Case "01"
                                                TA0005row("PAYOILKBN_2_1") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_1") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(0) = wSUM_NACHAIDISTANCE_2(0) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(0) = wSUM_NACKAIDISTANCE_2(0) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(0) = wSUM_NACHAIDISTANCE_2(0) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(0) = wSUM_NACTTLDISTANCE_2(0) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(0) = wSUM_NACUNLOADCNT_2(0) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(0) = wSUM_NACUNLOADCNT_2(0) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "02"
                                                TA0005row("PAYOILKBN_2_2") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_2") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(1) = wSUM_NACHAIDISTANCE_2(1) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(1) = wSUM_NACKAIDISTANCE_2(1) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(1) = wSUM_NACHAIDISTANCE_2(1) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(1) = wSUM_NACTTLDISTANCE_2(1) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(1) = wSUM_NACUNLOADCNT_2(1) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(1) = wSUM_NACUNLOADCNT_2(1) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "03"
                                                TA0005row("PAYOILKBN_2_3") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_3") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(2) = wSUM_NACHAIDISTANCE_2(2) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(2) = wSUM_NACKAIDISTANCE_2(2) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(2) = wSUM_NACHAIDISTANCE_2(2) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(2) = wSUM_NACTTLDISTANCE_2(2) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(2) = wSUM_NACUNLOADCNT_2(2) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(2) = wSUM_NACUNLOADCNT_2(2) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "04"
                                                TA0005row("PAYOILKBN_2_4") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_4") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(3) = wSUM_NACHAIDISTANCE_2(3) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(3) = wSUM_NACKAIDISTANCE_2(3) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(3) = wSUM_NACHAIDISTANCE_2(3) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(3) = wSUM_NACTTLDISTANCE_2(3) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(3) = wSUM_NACUNLOADCNT_2(3) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(3) = wSUM_NACUNLOADCNT_2(3) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "05"
                                                TA0005row("PAYOILKBN_2_5") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_5") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(4) = wSUM_NACHAIDISTANCE_2(4) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(4) = wSUM_NACKAIDISTANCE_2(4) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(4) = wSUM_NACHAIDISTANCE_2(4) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(4) = wSUM_NACTTLDISTANCE_2(4) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(4) = wSUM_NACUNLOADCNT_2(4) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(4) = wSUM_NACUNLOADCNT_2(4) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "06"
                                                TA0005row("PAYOILKBN_2_6") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_6") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(5) = wSUM_NACHAIDISTANCE_2(5) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(5) = wSUM_NACKAIDISTANCE_2(5) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(5) = wSUM_NACHAIDISTANCE_2(5) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(5) = wSUM_NACTTLDISTANCE_2(5) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(5) = wSUM_NACUNLOADCNT_2(5) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(5) = wSUM_NACUNLOADCNT_2(5) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "07"
                                                TA0005row("PAYOILKBN_2_7") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_7") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(6) = wSUM_NACHAIDISTANCE_2(6) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(6) = wSUM_NACKAIDISTANCE_2(6) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(6) = wSUM_NACHAIDISTANCE_2(6) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(6) = wSUM_NACTTLDISTANCE_2(6) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(6) = wSUM_NACUNLOADCNT_2(6) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(6) = wSUM_NACUNLOADCNT_2(6) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "08"
                                                TA0005row("PAYOILKBN_2_8") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_8") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(7) = wSUM_NACHAIDISTANCE_2(7) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(7) = wSUM_NACKAIDISTANCE_2(7) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(7) = wSUM_NACHAIDISTANCE_2(7) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(7) = wSUM_NACTTLDISTANCE_2(7) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(7) = wSUM_NACUNLOADCNT_2(7) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(7) = wSUM_NACUNLOADCNT_2(7) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "09"
                                                TA0005row("PAYOILKBN_2_9") = SQLdr("PAYOILKBN")                   '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_9") = SQLdr("PAYOILKBNNAME")            '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(8) = wSUM_NACHAIDISTANCE_2(8) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(8) = wSUM_NACKAIDISTANCE_2(8) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(8) = wSUM_NACHAIDISTANCE_2(8) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(8) = wSUM_NACTTLDISTANCE_2(8) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(8) = wSUM_NACUNLOADCNT_2(8) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(8) = wSUM_NACUNLOADCNT_2(8) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                            Case "10"
                                                TA0005row("PAYOILKBN_2_10") = SQLdr("PAYOILKBN")                  '勤怠用油種区分
                                                TA0005row("PAYOILKBNNAME_2_10") = SQLdr("PAYOILKBNNAME")           '勤怠用油種区分名称
                                                wSUM_NACHAIDISTANCE_2(9) = wSUM_NACHAIDISTANCE_2(9) + Val(SQLdr("NACHAIDISTANCE")) '実績・配送距離
                                                wSUM_NACKAIDISTANCE_2(9) = wSUM_NACKAIDISTANCE_2(9) + Val(SQLdr("NACKAIDISTANCE")) '実績・下車作業距離
                                                wSUM_NACHAIDISTANCE_2(9) = wSUM_NACHAIDISTANCE_2(9) + Val(SQLdr("NACCHODISTANCE")) '実績・勤怠調整距離
                                                wSUM_NACTTLDISTANCE_2(9) = wSUM_NACTTLDISTANCE_2(9) + Val(SQLdr("NACTTLDISTANCE")) '実績・配送距離合計Σ
                                                wSUM_NACUNLOADCNT_2(9) = wSUM_NACUNLOADCNT_2(9) + Val(SQLdr("NACUNLOADCNT"))       '実績・荷卸回数
                                                wSUM_NACUNLOADCNT_2(9) = wSUM_NACUNLOADCNT_2(9) + Val(SQLdr("NACCHOUNLOADCNT"))  '実績・荷卸回数調整
                                        End Select

                                    End If
                                End If

                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "JMD" OrElse
                                   SQLdr("ACACHANTEI") = "AMD" OrElse SQLdr("ACACHANTEI") = "ERC" OrElse
                                   SQLdr("ACACHANTEI") = "JMC" OrElse SQLdr("ACACHANTEI") = "AMC" Then
                                    wSUM_NACOFFICESORG = SQLdr("NACOFFICESORG")                         '実績・従業作業部署
                                    wSUM_NACOFFICESORGNAME = SQLdr("NACOFFICESORGNAME")                 '実績・従業作業部署名称
                                End If

                                If SQLdr("ACACHANTEI") = "ERD" OrElse SQLdr("ACACHANTEI") = "JMD" OrElse
                                   SQLdr("ACACHANTEI") = "ERC" OrElse SQLdr("ACACHANTEI") = "JMC" Then
                                    '出社日時
                                    If IsDate(SQLdr("PAYSHUSHADATE")) AndAlso SQLdr("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYSHUSHADATE")
                                        wSUM_PAYSHUSHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYSHUSHADATE = C_DEFAULT_YMD
                                    End If

                                    '退社日時
                                    If IsDate(SQLdr("PAYTAISHADATE")) AndAlso SQLdr("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = SQLdr("PAYTAISHADATE")
                                        wSUM_PAYTAISHADATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_PAYTAISHADATE = C_DEFAULT_YMD
                                    End If

                                    wSUM_PAYKBN = SQLdr("PAYKBN")                                       '勤怠区分
                                    wSUM_PAYKBNNAME = SQLdr("PAYKBNNAME")                               '勤怠区分名称
                                    wSUM_PAYSHUKCHOKKBN = SQLdr("PAYSHUKCHOKKBN")                       '宿日直区分
                                    wSUM_PAYSHUKCHOKKBNNAME = SQLdr("PAYSHUKCHOKKBNNAME")               '宿日直区分名称
                                End If

                                If SQLdr("ACACHANTEI") = "AMD" OrElse SQLdr("ACACHANTEI") = "AMC" Then
                                    wSUM_PAYSHUSHADATE = ""
                                    wSUM_PAYTAISHADATE = ""
                                    wSUM_PAYKBN = ""
                                    wSUM_PAYKBNNAME = ""
                                    wSUM_PAYSHUKCHOKKBN = ""
                                    wSUM_PAYSHUKCHOKKBNNAME = ""
                                End If

                                If SQLdr("ACACHANTEI") = "JMD" OrElse SQLdr("ACACHANTEI") = "JMC" Then
                                    wSUM_PAYAPPLYID = SQLdr("PAYAPPLYID")                               '申請ID
                                    wSUM_PAYRIYU = SQLdr("PAYRIYU")                                     '理由
                                    wSUM_PAYRIYUNAME = SQLdr("PAYRIYUNAME")                             '理由
                                    wSUM_PAYRIYUETC = SQLdr("PAYRIYUETC")                               '理由その他
                                End If

                                wINT = Val(SQLdr("NACHAIWORKTIME"))
                                wSUM_NACHAIWORKTIME = wSUM_NACHAIWORKTIME + wINT                                      '実績・配送作業時間

                                wINT = Val(SQLdr("NACGESWORKTIME"))
                                wSUM_NACGESWORKTIME = wSUM_NACGESWORKTIME + wINT                                      '実績・下車作業時間

                                wINT = Val(SQLdr("NACCHOWORKTIME"))
                                wSUM_NACCHOWORKTIME = wSUM_NACCHOWORKTIME + wINT                                      '実績・勤怠調整時間

                                wINT = Val(SQLdr("NACTTLWORKTIME"))
                                wSUM_NACTTLWORKTIME = wSUM_NACTTLWORKTIME + wINT                                      '実績・配送合計時間Σ

                                wINT = Val(SQLdr("NACOUTWORKTIME"))
                                wSUM_NACOUTWORKTIME = wSUM_NACOUTWORKTIME + wINT                                      '実績・就業外時間

                                wINT = Val(SQLdr("NACBREAKTIME"))
                                wSUM_NACBREAKTIME = wSUM_NACBREAKTIME + wINT                                          '実績・休憩時間

                                wINT = Val(SQLdr("NACCHOBREAKTIME"))
                                wSUM_NACCHOBREAKTIME = wSUM_NACCHOBREAKTIME + wINT                                    '実績・休憩調整時間

                                wINT = Val(SQLdr("NACTTLBREAKTIME"))
                                wSUM_NACTTLBREAKTIME = wSUM_NACTTLBREAKTIME + wINT                                    '実績・休憩合計時間Σ

                                wINT = Val(SQLdr("NACOFFICETIME"))
                                wSUM_NACOFFICETIME = wSUM_NACOFFICETIME + wINT                                        '実績・従業時間

                                wINT = Val(SQLdr("NACOFFICEBREAKTIME"))
                                wSUM_NACOFFICEBREAKTIME = wSUM_NACOFFICEBREAKTIME + wINT                              '実績・従業休憩時間

                                wINT = Val(SQLdr("PAYWORKNISSU"))
                                wSUM_PAYWORKNISSU = wSUM_PAYWORKNISSU + wINT                                         '所労

                                wINT = Val(SQLdr("PAYSHOUKETUNISSU"))
                                wSUM_PAYSHOUKETUNISSU = wSUM_PAYSHOUKETUNISSU + wINT                                 '傷欠

                                wINT = Val(SQLdr("PAYKUMIKETUNISSU"))
                                wSUM_PAYKUMIKETUNISSU = wSUM_PAYKUMIKETUNISSU + wINT                                 '組欠

                                wINT = Val(SQLdr("PAYETCKETUNISSU"))
                                wSUM_PAYETCKETUNISSU = wSUM_PAYETCKETUNISSU + wINT                                   '他欠

                                wINT = Val(SQLdr("PAYNENKYUNISSU"))
                                wSUM_PAYNENKYUNISSU = wSUM_PAYNENKYUNISSU + wINT                                     '年休

                                wINT = Val(SQLdr("PAYTOKUKYUNISSU"))
                                wSUM_PAYTOKUKYUNISSU = wSUM_PAYTOKUKYUNISSU + wINT                                   '特休

                                wINT = Val(SQLdr("PAYCHIKOKSOTAINISSU"))
                                wSUM_PAYCHIKOKSOTAINISSU = wSUM_PAYCHIKOKSOTAINISSU + wINT                           '遅早

                                wINT = Val(SQLdr("PAYSTOCKNISSU"))
                                wSUM_PAYSTOCKNISSU = wSUM_PAYSTOCKNISSU + wINT                                       'ストック休暇

                                wINT = Val(SQLdr("PAYKYOTEIWEEKNISSU"))
                                wSUM_PAYKYOTEIWEEKNISSU = wSUM_PAYKYOTEIWEEKNISSU + wINT                             '協定週休

                                wINT = Val(SQLdr("PAYWEEKNISSU"))
                                wSUM_PAYWEEKNISSU = wSUM_PAYWEEKNISSU + wINT                                         '週休

                                wINT = Val(SQLdr("PAYDAIKYUNISSU"))
                                wSUM_PAYDAIKYUNISSU = wSUM_PAYDAIKYUNISSU + wINT                                     '代休

                                wINT = Val(SQLdr("PAYWORKTIME"))
                                wSUM_PAYWORKTIME = wSUM_PAYWORKTIME + wINT                                           '所定労働時間

                                wINT = Val(SQLdr("PAYNIGHTTIME"))
                                wSUM_PAYNIGHTTIME = wSUM_PAYNIGHTTIME + wINT                                         '所定深夜時間

                                wINT = Val(SQLdr("PAYORVERTIME"))
                                wSUM_PAYORVERTIME = wSUM_PAYORVERTIME + wINT                                         '平日残業時間

                                wINT = Val(SQLdr("PAYWNIGHTTIME"))
                                wSUM_PAYWNIGHTTIME = wSUM_PAYWNIGHTTIME + wINT                                       '平日深夜時間

                                wINT = Val(SQLdr("PAYWSWORKTIME"))
                                wSUM_PAYWSWORKTIME = wSUM_PAYWSWORKTIME + wINT                                       '日曜出勤時間

                                wINT = Val(SQLdr("PAYSNIGHTTIME"))
                                wSUM_PAYSNIGHTTIME = wSUM_PAYSNIGHTTIME + wINT                                       '日曜深夜時間

                                wINT = Val(SQLdr("PAYHWORKTIME"))
                                wSUM_PAYHWORKTIME = wSUM_PAYHWORKTIME + wINT                                         '休日出勤時間

                                wINT = Val(SQLdr("PAYHNIGHTTIME"))
                                wSUM_PAYHNIGHTTIME = wSUM_PAYHNIGHTTIME + wINT                                       '休日深夜時間

                                wINT = Val(SQLdr("PAYBREAKTIME"))
                                wSUM_PAYBREAKTIME = wSUM_PAYBREAKTIME + wINT                                         '休憩時間

                                wINT = Val(SQLdr("PAYNENSHINISSU"))
                                wSUM_PAYNENSHINISSU = wSUM_PAYNENSHINISSU + wINT                                     '年始出勤

                                wINT = Val(SQLdr("PAYSHUKCHOKNNISSU"))
                                wSUM_PAYSHUKCHOKNNISSU = wSUM_PAYSHUKCHOKNNISSU + wINT                               '宿日直年始

                                wINT = Val(SQLdr("PAYSHUKCHOKNISSU"))
                                wSUM_PAYSHUKCHOKNISSU = wSUM_PAYSHUKCHOKNISSU + wINT                                 '宿日直通常

                                wINT = Val(SQLdr("PAYSHUKCHOKNHLDNISSU"))
                                wSUM_PAYSHUKCHOKNHLDNISSU = wSUM_PAYSHUKCHOKNHLDNISSU + wINT                         '宿日直年始（翌休み）

                                wINT = Val(SQLdr("PAYSHUKCHOKHLDNISSU"))
                                wSUM_PAYSHUKCHOKHLDNISSU = wSUM_PAYSHUKCHOKHLDNISSU + wINT                           '宿日直通常（翌休み）

                                wINT = Val(SQLdr("PAYTOKSAAKAISU"))
                                wSUM_PAYTOKSAAKAISU = wSUM_PAYTOKSAAKAISU + wINT                                     '特作A

                                wINT = Val(SQLdr("PAYTOKSABKAISU"))
                                wSUM_PAYTOKSABKAISU = wSUM_PAYTOKSABKAISU + wINT                                     '特作B

                                wINT = Val(SQLdr("PAYTOKSACKAISU"))
                                wSUM_PAYTOKSACKAISU = wSUM_PAYTOKSACKAISU + wINT                                     '特作C

                                wINT = Val(SQLdr("PAYHOANTIME"))
                                wSUM_PAYHOANTIME = wSUM_PAYHOANTIME + wINT                                           '保安検査入力

                                wINT = Val(SQLdr("PAYKOATUTIME"))
                                wSUM_PAYKOATUTIME = wSUM_PAYKOATUTIME + wINT                                         '高圧作業入力

                                wINT = Val(SQLdr("PAYTOKUSA1TIME"))
                                wSUM_PAYTOKUSA1TIME = wSUM_PAYTOKUSA1TIME + wINT                                     '特作Ⅰ

                                wINT = Val(SQLdr("PAYPONPNISSU"))
                                wSUM_PAYPONPNISSU = wSUM_PAYPONPNISSU + wINT                                         'ポンプ

                                wINT = Val(SQLdr("PAYBULKNISSU"))
                                wSUM_PAYBULKNISSU = wSUM_PAYBULKNISSU + wINT                                         'バルク

                                wINT = Val(SQLdr("PAYTRAILERNISSU"))
                                wSUM_PAYTRAILERNISSU = wSUM_PAYTRAILERNISSU + wINT                                   'トレーラ

                                wINT = Val(SQLdr("PAYBKINMUKAISU"))
                                wSUM_PAYBKINMUKAISU = wSUM_PAYBKINMUKAISU + wINT                                     'B勤務

                            End While


                            '〇最終レコード出力

                            If Not (WW_NACSHUKODATE = "" AndAlso
                           WW_PAYHORG = "" AndAlso
                           WW_PAYSTAFFCODE = "" AndAlso
                           WW_ACACHANTEI = "") Then
                                '合計値セット
                                TA0005row("TAISHYM") = work.WF_SEL_STYM.Text
                                TA0005row("NACHAIDISTANCE_1_1") = wSUM_NACHAIDISTANCE_1(0)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_1") = wSUM_NACKAIDISTANCE_1(0)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_1") = wSUM_NACCHODISTANCE_1(0)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_1") = wSUM_NACTTLDISTANCE_1(0)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_2") = wSUM_NACHAIDISTANCE_1(1)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_2") = wSUM_NACKAIDISTANCE_1(1)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_2") = wSUM_NACCHODISTANCE_1(1)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_2") = wSUM_NACTTLDISTANCE_1(1)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_3") = wSUM_NACHAIDISTANCE_1(2)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_3") = wSUM_NACKAIDISTANCE_1(2)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_3") = wSUM_NACCHODISTANCE_1(2)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_3") = wSUM_NACTTLDISTANCE_1(2)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_4") = wSUM_NACHAIDISTANCE_1(3)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_4") = wSUM_NACKAIDISTANCE_1(3)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_4") = wSUM_NACCHODISTANCE_1(3)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_4") = wSUM_NACTTLDISTANCE_1(3)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_5") = wSUM_NACHAIDISTANCE_1(4)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_5") = wSUM_NACKAIDISTANCE_1(4)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_5") = wSUM_NACCHODISTANCE_1(4)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_5") = wSUM_NACTTLDISTANCE_1(4)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_6") = wSUM_NACHAIDISTANCE_1(5)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_6") = wSUM_NACKAIDISTANCE_1(5)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_6") = wSUM_NACCHODISTANCE_1(5)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_6") = wSUM_NACTTLDISTANCE_1(5)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_7") = wSUM_NACHAIDISTANCE_1(6)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_7") = wSUM_NACKAIDISTANCE_1(6)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_7") = wSUM_NACCHODISTANCE_1(6)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_7") = wSUM_NACTTLDISTANCE_1(6)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_8") = wSUM_NACHAIDISTANCE_1(7)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_8") = wSUM_NACKAIDISTANCE_1(7)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_8") = wSUM_NACCHODISTANCE_1(7)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_8") = wSUM_NACTTLDISTANCE_1(7)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_9") = wSUM_NACHAIDISTANCE_1(8)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_9") = wSUM_NACKAIDISTANCE_1(8)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_9") = wSUM_NACCHODISTANCE_1(8)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_9") = wSUM_NACTTLDISTANCE_1(8)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_1_10") = wSUM_NACHAIDISTANCE_1(9)          '実績・配送距離
                                TA0005row("NACKAIDISTANCE_1_10") = wSUM_NACKAIDISTANCE_1(9)          '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_1_10") = wSUM_NACCHODISTANCE_1(9)          '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_1_10") = wSUM_NACTTLDISTANCE_1(9)          '実績・配送距離合計Σ

                                TA0005row("NACHAIDISTANCE_2_1") = wSUM_NACHAIDISTANCE_2(0)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_1") = wSUM_NACKAIDISTANCE_2(0)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_1") = wSUM_NACCHODISTANCE_2(0)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_1") = wSUM_NACTTLDISTANCE_2(0)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_2") = wSUM_NACHAIDISTANCE_2(1)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_2") = wSUM_NACKAIDISTANCE_2(1)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_2") = wSUM_NACCHODISTANCE_2(1)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_2") = wSUM_NACTTLDISTANCE_2(1)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_3") = wSUM_NACHAIDISTANCE_2(2)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_3") = wSUM_NACKAIDISTANCE_2(2)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_3") = wSUM_NACCHODISTANCE_2(2)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_3") = wSUM_NACTTLDISTANCE_2(2)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_4") = wSUM_NACHAIDISTANCE_2(3)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_4") = wSUM_NACKAIDISTANCE_2(3)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_4") = wSUM_NACCHODISTANCE_2(3)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_4") = wSUM_NACTTLDISTANCE_2(3)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_5") = wSUM_NACHAIDISTANCE_2(4)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_5") = wSUM_NACKAIDISTANCE_2(4)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_5") = wSUM_NACCHODISTANCE_2(4)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_5") = wSUM_NACTTLDISTANCE_2(4)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_6") = wSUM_NACHAIDISTANCE_2(5)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_6") = wSUM_NACKAIDISTANCE_2(5)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_6") = wSUM_NACCHODISTANCE_2(5)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_6") = wSUM_NACTTLDISTANCE_2(5)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_7") = wSUM_NACHAIDISTANCE_2(6)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_7") = wSUM_NACKAIDISTANCE_2(6)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_7") = wSUM_NACCHODISTANCE_2(6)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_7") = wSUM_NACTTLDISTANCE_2(6)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_8") = wSUM_NACHAIDISTANCE_2(7)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_8") = wSUM_NACKAIDISTANCE_2(7)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_8") = wSUM_NACCHODISTANCE_2(7)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_8") = wSUM_NACTTLDISTANCE_2(7)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_9") = wSUM_NACHAIDISTANCE_2(8)           '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_9") = wSUM_NACKAIDISTANCE_2(8)           '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_9") = wSUM_NACCHODISTANCE_2(8)           '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_9") = wSUM_NACTTLDISTANCE_2(8)           '実績・配送距離合計Σ
                                TA0005row("NACHAIDISTANCE_2_10") = wSUM_NACHAIDISTANCE_2(9)          '実績・配送距離
                                TA0005row("NACKAIDISTANCE_2_10") = wSUM_NACKAIDISTANCE_2(9)          '実績・下車作業距離
                                TA0005row("NACCHODISTANCE_2_10") = wSUM_NACCHODISTANCE_2(9)          '実績・勤怠調整距離
                                TA0005row("NACTTLDISTANCE_2_10") = wSUM_NACTTLDISTANCE_2(9)          '実績・配送距離合計Σ
                                TA0005row("NACTTLDISTANCE_2_10") = wSUM_NACTTLDISTANCE_2(9)          '実績・配送距離合計Σ
                                For i As Integer = 0 To 9
                                    wSUM_NACTTLDISTANCE_G += wSUM_NACTTLDISTANCE_1(i) + wSUM_NACTTLDISTANCE_2(i)
                                Next
                                TA0005row("NACTTLDISTANCE_G") = wSUM_NACTTLDISTANCE_G                '実績・配送距離合計Σ

                                TA0005row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                TA0005row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                TA0005row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                TA0005row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                TA0005row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                TA0005row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                TA0005row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                TA0005row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                TA0005row("NACUNLOADCNT_1_1") = wSUM_NACUNLOADCNT_1(0)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_1") = wSUM_NACCHOUNLOADCNT_1(0)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_2") = wSUM_NACUNLOADCNT_1(1)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_2") = wSUM_NACCHOUNLOADCNT_1(1)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_3") = wSUM_NACUNLOADCNT_1(2)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_3") = wSUM_NACCHOUNLOADCNT_1(2)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_4") = wSUM_NACUNLOADCNT_1(3)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_4") = wSUM_NACCHOUNLOADCNT_1(3)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_5") = wSUM_NACUNLOADCNT_1(4)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_5") = wSUM_NACCHOUNLOADCNT_1(4)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_6") = wSUM_NACUNLOADCNT_1(5)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_6") = wSUM_NACCHOUNLOADCNT_1(5)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_7") = wSUM_NACUNLOADCNT_1(6)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_7") = wSUM_NACCHOUNLOADCNT_1(6)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_8") = wSUM_NACUNLOADCNT_1(7)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_8") = wSUM_NACCHOUNLOADCNT_1(7)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_9") = wSUM_NACUNLOADCNT_1(8)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_9") = wSUM_NACCHOUNLOADCNT_1(8)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_1_10") = wSUM_NACUNLOADCNT_1(9)                             '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_1_10") = wSUM_NACCHOUNLOADCNT_1(9)                       '実績・荷卸回数調整

                                TA0005row("NACUNLOADCNT_2_1") = wSUM_NACUNLOADCNT_2(0)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_1") = wSUM_NACCHOUNLOADCNT_2(0)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_2") = wSUM_NACUNLOADCNT_2(1)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_2") = wSUM_NACCHOUNLOADCNT_2(1)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_3") = wSUM_NACUNLOADCNT_2(2)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_3") = wSUM_NACCHOUNLOADCNT_2(2)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_4") = wSUM_NACUNLOADCNT_2(3)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_4") = wSUM_NACCHOUNLOADCNT_2(3)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_5") = wSUM_NACUNLOADCNT_2(4)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_5") = wSUM_NACCHOUNLOADCNT_2(4)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_6") = wSUM_NACUNLOADCNT_2(5)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_6") = wSUM_NACCHOUNLOADCNT_2(5)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_7") = wSUM_NACUNLOADCNT_2(6)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_7") = wSUM_NACCHOUNLOADCNT_2(6)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_8") = wSUM_NACUNLOADCNT_2(7)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_8") = wSUM_NACCHOUNLOADCNT_2(7)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_9") = wSUM_NACUNLOADCNT_2(8)                              '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_9") = wSUM_NACCHOUNLOADCNT_2(8)                        '実績・荷卸回数調整
                                TA0005row("NACUNLOADCNT_2_10") = wSUM_NACUNLOADCNT_2(9)                             '実績・荷卸回数
                                TA0005row("NACCHOUNLOADCNT_2_10") = wSUM_NACCHOUNLOADCNT_2(9)                       '実績・荷卸回数調整
                                For i As Integer = 0 To 9
                                    wSUM_NACTTLUNLOADCNT_G += wSUM_NACUNLOADCNT_1(i) + wSUM_NACCHOUNLOADCNT_1(i) + wSUM_NACUNLOADCNT_2(i) + wSUM_NACCHOUNLOADCNT_2(i)
                                Next
                                TA0005row("NACTTLUNLOADCNT_G") = wSUM_NACTTLUNLOADCNT_G                             '実績・荷卸回数合計Σ
                                TA0005row("NACOFFICETIME") = wSUM_NACOFFICETIME                                     '実績・従業時間
                                TA0005row("NACOFFICEBREAKTIME") = wSUM_NACOFFICEBREAKTIME                           '実績・従業休憩時間

                                TA0005row("NACOFFICESORG") = wSUM_NACOFFICESORG                                     '実績・作業部署
                                TA0005row("NACOFFICESORGNAME") = wSUM_NACOFFICESORGNAME                             '実績・作業部署名称
                                TA0005row("PAYKBN") = wSUM_PAYKBN                                                   '勤怠区分
                                TA0005row("PAYKBNNAME") = wSUM_PAYKBNNAME                                           '勤怠区分名称
                                TA0005row("PAYSHUKCHOKKBN") = wSUM_PAYSHUKCHOKKBN                                   '宿日直区分
                                TA0005row("PAYSHUKCHOKKBNNAME") = wSUM_PAYSHUKCHOKKBNNAME                           '宿日直区分名称

                                TA0005row("PAYSHUSHADATE") = wSUM_PAYSHUSHADATE                                     '出社日時
                                TA0005row("PAYTAISHADATE") = wSUM_PAYTAISHADATE                                     '退社日時
                                Try
                                    Dim wMin As Integer = DateDiff("n", wSUM_PAYSHUSHADATE, wSUM_PAYTAISHADATE)
                                    TA0005row("WORKTIME") = MinutesToHHMM(wMin)                                        '拘束時間
                                    TA0005row("WORKTIMEMIN") = wMin                                                 '拘束時間（分）
                                    If wMin >= 960 Then
                                        TA0005row("WORKTIMEMIN16UP") = 1                                            '拘束時間１６時間超（回数)
                                    Else
                                        TA0005row("WORKTIMEMIN16UP") = 0                                            '拘束時間１６時間超（回数)
                                    End If
                                Catch ex As Exception
                                    TA0005row("WORKTIME") = "00:00"
                                    TA0005row("WORKTIMEMIN") = 0
                                    TA0005row("WORKTIMEMIN16UP") = 0                                                '拘束時間１６時間超（回数)
                                End Try
                                TA0005row("PAYWORKNISSU") = wSUM_PAYWORKNISSU                                       '所労
                                TA0005row("PAYSHOUKETUNISSU") = wSUM_PAYSHOUKETUNISSU                               '傷欠
                                TA0005row("PAYKUMIKETUNISSU") = wSUM_PAYKUMIKETUNISSU                               '組欠
                                TA0005row("PAYETCKETUNISSU") = wSUM_PAYETCKETUNISSU                                 '他欠
                                TA0005row("PAYNENKYUNISSU") = wSUM_PAYNENKYUNISSU                                   '年休
                                TA0005row("PAYTOKUKYUNISSU") = wSUM_PAYTOKUKYUNISSU                                 '特休
                                TA0005row("PAYCHIKOKSOTAINISSU") = wSUM_PAYCHIKOKSOTAINISSU                         '遅早
                                TA0005row("PAYSTOCKNISSU") = wSUM_PAYSTOCKNISSU                                     'ストック休暇
                                TA0005row("PAYKYOTEIWEEKNISSU") = wSUM_PAYKYOTEIWEEKNISSU                           '協定週休
                                TA0005row("PAYWEEKNISSU") = wSUM_PAYWEEKNISSU                                       '週休
                                TA0005row("PAYDAIKYUNISSU") = wSUM_PAYDAIKYUNISSU                                   '代休
                                TA0005row("PAYWORKTIME") = wSUM_PAYWORKTIME                                         '所定労働時間
                                TA0005row("PAYNIGHTTIME") = wSUM_PAYNIGHTTIME                                       '所定深夜時間
                                TA0005row("PAYORVERTIME") = wSUM_PAYORVERTIME                                       '平日残業時間
                                TA0005row("PAYWNIGHTTIME") = wSUM_PAYWNIGHTTIME                                     '平日深夜時間
                                TA0005row("PAYWSWORKTIME") = wSUM_PAYWSWORKTIME                                     '日曜出勤時間
                                TA0005row("PAYSNIGHTTIME") = wSUM_PAYSNIGHTTIME                                     '日曜深夜時間
                                TA0005row("PAYHWORKTIME") = wSUM_PAYHWORKTIME                                       '休日出勤時間
                                TA0005row("PAYHNIGHTTIME") = wSUM_PAYHNIGHTTIME                                     '休日深夜時間
                                TA0005row("PAYBREAKTIME") = wSUM_PAYBREAKTIME                                       '休憩時間
                                TA0005row("PAYNENSHINISSU") = wSUM_PAYNENSHINISSU                                   '年始出勤
                                TA0005row("PAYSHUKCHOKNNISSU") = wSUM_PAYSHUKCHOKNNISSU                             '宿日直年始
                                TA0005row("PAYSHUKCHOKNISSU") = wSUM_PAYSHUKCHOKNISSU                               '宿日直通常
                                TA0005row("PAYSHUKCHOKNHLDNISSU") = wSUM_PAYSHUKCHOKNHLDNISSU                       '宿日直年始（翌休み）
                                TA0005row("PAYSHUKCHOKHLDNISSU") = wSUM_PAYSHUKCHOKHLDNISSU                         '宿日直通常（翌休み）
                                TA0005row("PAYTOKSAAKAISU") = wSUM_PAYTOKSAAKAISU                                   '特作A
                                TA0005row("PAYTOKSABKAISU") = wSUM_PAYTOKSABKAISU                                   '特作B
                                TA0005row("PAYTOKSACKAISU") = wSUM_PAYTOKSACKAISU                                   '特作C
                                TA0005row("PAYHOANTIME") = wSUM_PAYHOANTIME                                         '保安検査入力
                                TA0005row("PAYKOATUTIME") = wSUM_PAYKOATUTIME                                       '高圧作業入力
                                TA0005row("PAYTOKUSA1TIME") = wSUM_PAYTOKUSA1TIME                                   '特作Ⅰ
                                TA0005row("PAYPONPNISSU") = wSUM_PAYPONPNISSU                                       'ポンプ
                                TA0005row("PAYBULKNISSU") = wSUM_PAYBULKNISSU                                       'バルク
                                TA0005row("PAYTRAILERNISSU") = wSUM_PAYTRAILERNISSU                                 'トレーラ
                                TA0005row("PAYBKINMUKAISU") = wSUM_PAYBKINMUKAISU                                   'B勤務
                                TA0005row("PAYAPPLYID") = wSUM_PAYAPPLYID                                           '申請ID
                                TA0005row("PAYRIYU") = wSUM_PAYRIYU                                                 '理由
                                TA0005row("PAYRIYUNAME") = wSUM_PAYRIYUNAME                                         '理由
                                TA0005row("PAYRIYUETC") = wSUM_PAYRIYUETC                                           '理由その他

                                TA0005tbl.Rows.Add(TA0005row)
                            End If
                        End Using

                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0001_TOKEI SELECT")
                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:L0001_TOKEI Select"           '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' サマリー後データ取得
    ''' </summary>
    ''' <param name="I_TERM_CLASS"></param>
    ''' <remarks></remarks>
    Private Sub GetTA0005Work2(ByVal I_TERM_CLASS As String)

        '○初期クリア
        'TA0005tbl値設定
        Dim wINT As Integer
        Dim wDATE As Date
        Dim wDATETime As DateTime
        Dim WW_TA0005WKtbl As DataTable = New DataTable
        Dim WW_TA0005WK2tbl As DataTable = New DataTable

        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As List(Of String) = GetORGList(I_TERM_CLASS)

        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As New StringBuilder(20000)
            SQLStr.AppendLine(" SELECT ")
            SQLStr.AppendLine("    isnull(rtrim(L04.CAMPCODE),'') as CAMPCODE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.CAMPNAME),'') as CAMPNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.KEIJOYMD),'') as KEIJOYMD   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.DENYMD),'') as DENYMD   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.DENNO),'') as DENNO   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.KANRENDENNO),'') as KANRENDENNO   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.DTLNO),'') as DTLNO   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.ACACHANTEI),'') as ACACHANTEI   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.ACACHANTEINAME),'') as ACACHANTEINAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACSHUKODATE),'') as NACSHUKODATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_1),'') as NACHAIDISTANCE_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_1),'') as NACKAIDISTANCE_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_1),'') as NACCHODISTANCE_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_1),'') as NACTTLDISTANCE_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_2),'') as NACHAIDISTANCE_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_2),'') as NACKAIDISTANCE_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_2),'') as NACCHODISTANCE_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_2),'') as NACTTLDISTANCE_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_3),'') as NACHAIDISTANCE_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_3),'') as NACKAIDISTANCE_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_3),'') as NACCHODISTANCE_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_3),'') as NACTTLDISTANCE_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_4),'') as NACHAIDISTANCE_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_4),'') as NACKAIDISTANCE_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_4),'') as NACCHODISTANCE_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_4),'') as NACTTLDISTANCE_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_5),'') as NACHAIDISTANCE_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_5),'') as NACKAIDISTANCE_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_5),'') as NACCHODISTANCE_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_5),'') as NACTTLDISTANCE_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_6),'') as NACHAIDISTANCE_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_6),'') as NACKAIDISTANCE_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_6),'') as NACCHODISTANCE_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_6),'') as NACTTLDISTANCE_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_7),'') as NACHAIDISTANCE_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_7),'') as NACKAIDISTANCE_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_7),'') as NACCHODISTANCE_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_7),'') as NACTTLDISTANCE_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_8),'') as NACHAIDISTANCE_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_8),'') as NACKAIDISTANCE_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_8),'') as NACCHODISTANCE_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_8),'') as NACTTLDISTANCE_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_9),'') as NACHAIDISTANCE_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_9),'') as NACKAIDISTANCE_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_9),'') as NACCHODISTANCE_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_9),'') as NACTTLDISTANCE_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_1_10),'') as NACHAIDISTANCE_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_1_10),'') as NACKAIDISTANCE_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_1_10),'') as NACCHODISTANCE_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_1_10),'') as NACTTLDISTANCE_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_1),'') as NACHAIDISTANCE_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_1),'') as NACKAIDISTANCE_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_1),'') as NACCHODISTANCE_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_1),'') as NACTTLDISTANCE_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_2),'') as NACHAIDISTANCE_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_2),'') as NACKAIDISTANCE_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_2),'') as NACCHODISTANCE_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_2),'') as NACTTLDISTANCE_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_3),'') as NACHAIDISTANCE_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_3),'') as NACKAIDISTANCE_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_3),'') as NACCHODISTANCE_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_3),'') as NACTTLDISTANCE_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_4),'') as NACHAIDISTANCE_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_4),'') as NACKAIDISTANCE_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_4),'') as NACCHODISTANCE_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_4),'') as NACTTLDISTANCE_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_5),'') as NACHAIDISTANCE_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_5),'') as NACKAIDISTANCE_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_5),'') as NACCHODISTANCE_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_5),'') as NACTTLDISTANCE_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_6),'') as NACHAIDISTANCE_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_6),'') as NACKAIDISTANCE_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_6),'') as NACCHODISTANCE_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_6),'') as NACTTLDISTANCE_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_7),'') as NACHAIDISTANCE_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_7),'') as NACKAIDISTANCE_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_7),'') as NACCHODISTANCE_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_7),'') as NACTTLDISTANCE_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_8),'') as NACHAIDISTANCE_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_8),'') as NACKAIDISTANCE_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_8),'') as NACCHODISTANCE_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_8),'') as NACTTLDISTANCE_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_9),'') as NACHAIDISTANCE_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_9),'') as NACKAIDISTANCE_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_9),'') as NACCHODISTANCE_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_9),'') as NACTTLDISTANCE_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIDISTANCE_2_10),'') as NACHAIDISTANCE_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACKAIDISTANCE_2_10),'') as NACKAIDISTANCE_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHODISTANCE_2_10),'') as NACCHODISTANCE_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_2_10),'') as NACTTLDISTANCE_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLDISTANCE_G),'') as NACTTLDISTANCE_G   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAISTDATE),'') as NACHAISTDATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIENDDATE),'') as NACHAIENDDATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACHAIWORKTIME),'') as NACHAIWORKTIME   ")
            SQLStr.AppendLine("  , '" & C_DEFAULT_YMD & "' as NACGESSTDATE   ")
            SQLStr.AppendLine("  , '" & C_DEFAULT_YMD & "' as NACGESENDDATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACGESWORKTIME),'') as NACGESWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOWORKTIME),'') as NACCHOWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLWORKTIME),'') as NACTTLWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACOUTWORKTIME),'') as NACOUTWORKTIME   ")
            SQLStr.AppendLine("  , '" & C_DEFAULT_YMD & "' as NACBREAKSTDATE   ")
            SQLStr.AppendLine("  , '" & C_DEFAULT_YMD & "' as NACBREAKENDDATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACBREAKTIME),'') as NACBREAKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOBREAKTIME),'') as NACCHOBREAKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLBREAKTIME),'') as NACTTLBREAKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_1),'') as NACUNLOADCNT_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_1),'') as NACCHOUNLOADCNT_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_2),'') as NACUNLOADCNT_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_2),'') as NACCHOUNLOADCNT_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_3),'') as NACUNLOADCNT_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_3),'') as NACCHOUNLOADCNT_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_4),'') as NACUNLOADCNT_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_4),'') as NACCHOUNLOADCNT_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_5),'') as NACUNLOADCNT_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_5),'') as NACCHOUNLOADCNT_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_6),'') as NACUNLOADCNT_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_6),'') as NACCHOUNLOADCNT_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_7),'') as NACUNLOADCNT_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_7),'') as NACCHOUNLOADCNT_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_8),'') as NACUNLOADCNT_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_8),'') as NACCHOUNLOADCNT_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_9),'') as NACUNLOADCNT_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_9),'') as NACCHOUNLOADCNT_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_1_10),'') as NACUNLOADCNT_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_1_10),'') as NACCHOUNLOADCNT_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_1),'') as NACUNLOADCNT_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_1),'') as NACCHOUNLOADCNT_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_2),'') as NACUNLOADCNT_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_2),'') as NACCHOUNLOADCNT_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_3),'') as NACUNLOADCNT_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_3),'') as NACCHOUNLOADCNT_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_4),'') as NACUNLOADCNT_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_4),'') as NACCHOUNLOADCNT_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_5),'') as NACUNLOADCNT_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_5),'') as NACCHOUNLOADCNT_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_6),'') as NACUNLOADCNT_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_6),'') as NACCHOUNLOADCNT_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_7),'') as NACUNLOADCNT_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_7),'') as NACCHOUNLOADCNT_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_8),'') as NACUNLOADCNT_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_8),'') as NACCHOUNLOADCNT_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_9),'') as NACUNLOADCNT_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_9),'') as NACCHOUNLOADCNT_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACUNLOADCNT_2_10),'') as NACUNLOADCNT_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACCHOUNLOADCNT_2_10),'') as NACCHOUNLOADCNT_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACTTLUNLOADCNT_G),'') as NACTTLUNLOADCNT_G   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACOFFICESORG),'') as NACOFFICESORG   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACOFFICESORGNAME),'') as NACOFFICESORGNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACOFFICETIME),'') as NACOFFICETIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.NACOFFICEBREAKTIME),'') as NACOFFICEBREAKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUSHADATE),'') as PAYSHUSHADATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTAISHADATE),'') as PAYTAISHADATE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSTAFFKBN),'') as PAYSTAFFKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSTAFFKBNNAME),'') as PAYSTAFFKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSTAFFCODE),'') as PAYSTAFFCODE   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSTAFFCODENAME),'') as PAYSTAFFCODENAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYMORG),'') as PAYMORG   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYMORGNAME),'') as PAYMORGNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHORG),'') as PAYHORG   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHORGNAME),'') as PAYHORGNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHOLIDAYKBN),'') as PAYHOLIDAYKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHOLIDAYKBNNAME),'') as PAYHOLIDAYKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYKBN),'') as PAYKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYKBNNAME),'') as PAYKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKKBN),'') as PAYSHUKCHOKKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKKBNNAME),'') as PAYSHUKCHOKKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYJYOMUKBN),'') as PAYJYOMUKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYJYOMUKBNNAME),'') as PAYJYOMUKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_1),'') as PAYOILKBN_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_1),'') as PAYOILKBNNAME_1_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_2),'') as PAYOILKBN_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_2),'') as PAYOILKBNNAME_1_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_3),'') as PAYOILKBN_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_3),'') as PAYOILKBNNAME_1_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_4),'') as PAYOILKBN_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_4),'') as PAYOILKBNNAME_1_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_5),'') as PAYOILKBN_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_5),'') as PAYOILKBNNAME_1_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_6),'') as PAYOILKBN_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_6),'') as PAYOILKBNNAME_1_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_7),'') as PAYOILKBN_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_7),'') as PAYOILKBNNAME_1_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_8),'') as PAYOILKBN_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_8),'') as PAYOILKBNNAME_1_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_9),'') as PAYOILKBN_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_9),'') as PAYOILKBNNAME_1_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_1_10),'') as PAYOILKBN_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_1_10),'') as PAYOILKBNNAME_1_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_1),'') as PAYOILKBN_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_1),'') as PAYOILKBNNAME_2_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_2),'') as PAYOILKBN_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_2),'') as PAYOILKBNNAME_2_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_3),'') as PAYOILKBN_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_3),'') as PAYOILKBNNAME_2_3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_4),'') as PAYOILKBN_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_4),'') as PAYOILKBNNAME_2_4   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_5),'') as PAYOILKBN_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_5),'') as PAYOILKBNNAME_2_5   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_6),'') as PAYOILKBN_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_6),'') as PAYOILKBNNAME_2_6   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_7),'') as PAYOILKBN_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_7),'') as PAYOILKBNNAME_2_7   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_8),'') as PAYOILKBN_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_8),'') as PAYOILKBNNAME_2_8   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_9),'') as PAYOILKBN_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_9),'') as PAYOILKBNNAME_2_9   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBN_2_10),'') as PAYOILKBN_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYOILKBNNAME_2_10),'') as PAYOILKBNNAME_2_10   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHARYOKBN_1),'') as PAYSHARYOKBN_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHARYOKBNNAME_1),'') as PAYSHARYOKBNNAME_1   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHARYOKBN_2),'') as PAYSHARYOKBN_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHARYOKBNNAME_2),'') as PAYSHARYOKBNNAME_2   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYWORKNISSU),'') as PAYWORKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHOUKETUNISSU),'') as PAYSHOUKETUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYKUMIKETUNISSU),'') as PAYKUMIKETUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYETCKETUNISSU),'') as PAYETCKETUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYNENKYUNISSU),'') as PAYNENKYUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTOKUKYUNISSU),'') as PAYTOKUKYUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYCHIKOKSOTAINISSU),'') as PAYCHIKOKSOTAINISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSTOCKNISSU),'') as PAYSTOCKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYKYOTEIWEEKNISSU),'') as PAYKYOTEIWEEKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYWEEKNISSU),'') as PAYWEEKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYDAIKYUNISSU),'') as PAYDAIKYUNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYWORKTIME),'') as PAYWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYNIGHTTIME),'') as PAYNIGHTTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYORVERTIME),'') as PAYORVERTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYWNIGHTTIME),'') as PAYWNIGHTTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYWSWORKTIME),'') as PAYWSWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSNIGHTTIME),'') as PAYSNIGHTTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHWORKTIME),'') as PAYHWORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHNIGHTTIME),'') as PAYHNIGHTTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYBREAKTIME),'') as PAYBREAKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYNENSHINISSU),'') as PAYNENSHINISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKNNISSU),'') as PAYSHUKCHOKNNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKNISSU),'') as PAYSHUKCHOKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKNHLDNISSU),'') as PAYSHUKCHOKNHLDNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYSHUKCHOKHLDNISSU),'') as PAYSHUKCHOKHLDNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTOKSAAKAISU),'') as PAYTOKSAAKAISU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTOKSABKAISU),'') as PAYTOKSABKAISU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTOKSACKAISU),'') as PAYTOKSACKAISU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYHOANTIME),'') as PAYHOANTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYKOATUTIME),'') as PAYKOATUTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTOKUSA1TIME),'') as PAYTOKUSA1TIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYPONPNISSU),'') as PAYPONPNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYBULKNISSU),'') as PAYBULKNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYTRAILERNISSU),'') as PAYTRAILERNISSU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYBKINMUKAISU),'') as PAYBKINMUKAISU   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYAPPLYID),'') as PAYAPPLYID   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYRIYU),'') as PAYRIYU   ")
            SQLStr.AppendLine("  , isnull(( ")
            SQLStr.AppendLine("        select isnull(rtrim(MC1_38.VALUE1), '') ")
            SQLStr.AppendLine("        from MC001_FIXVALUE MC1_38  ")
            SQLStr.AppendLine("        where MC1_38.CAMPCODE = @P02  ")
            SQLStr.AppendLine("          and MC1_38.CLASS = 'T0009_RIYU'  ")
            SQLStr.AppendLine("          and MC1_38.KEYCODE = L04.PAYRIYU  ")
            SQLStr.AppendLine("          and MC1_38.STYMD <= L04.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_38.ENDYMD >= L04.NACSHUKODATE  ")
            SQLStr.AppendLine("          and MC1_38.DELFLG <> '1' ")
            SQLStr.AppendLine("    ),'') as PAYRIYUNAME ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.PAYRIYUETC),'') as PAYRIYUETC   ")
            SQLStr.AppendLine("  , '' as WORKKBN   ")
            SQLStr.AppendLine("  , '' as WORKKBNNAME   ")
            SQLStr.AppendLine("  , '' as KEYSTAFFCODE   ")
            SQLStr.AppendLine("  , '' as KEYGSHABAN   ")
            SQLStr.AppendLine("  , '' as KEYTRIPNO   ")
            SQLStr.AppendLine("  , '' as KEYDROPNO   ")
            SQLStr.AppendLine("  , '' as KEYTSHABAN1   ")
            SQLStr.AppendLine("  , '' as KEYTSHABAN2   ")
            SQLStr.AppendLine("  , '' as KEYTSHABAN3   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.RECODEKBN),'') as RECODEKBN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.RECODEKBNNAME),'') as RECODEKBNNAME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.WORKTIME),'') as WORKTIME   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.WORKTIMEMIN),'') as WORKTIMEMIN   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.WORKTIMEMIN16UP),'') as WORKTIMEMIN16UP   ")
            SQLStr.AppendLine("  , isnull(rtrim(L04.TAISHYM),'') as TAISHYM   ")
            SQLStr.AppendLine("  FROM L0004_SUMMARYK L04 ")
            SQLStr.AppendLine("  WHERE  ")
            SQLStr.AppendLine("         L04.CAMPCODE        = @P02  ")
            SQLStr.AppendLine("     and L04.NACSHUKODATE   <= @P05  ")
            SQLStr.AppendLine("     and L04.NACSHUKODATE   >= @P06  ")
            SQLStr.AppendLine("     and L04.KEIJOYMD       <= @P07  ")
            SQLStr.AppendLine("     and L04.KEIJOYMD       >= @P08  ")
            SQLStr.AppendLine("     and L04.PAYHORG         = @P09  ")
            SQLStr.AppendLine("     and L04.DELFLG         <> '1'  ")
            SQLStr.AppendLine("  ORDER BY ")
            SQLStr.AppendLine("         L04.PAYHORG, L04.PAYSTAFFCODE, L04.NACSHUKODATE, L04.ACACHANTEI DESC")

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 20)
                '抽出条件(サーバー部署)List毎にデータ抽出
                For Each WI_ORG As String In W_ORGlst

                    '部署変換
                    Dim WW_ORG As String = ""
                    ConvORGCode(WI_ORG, WW_ORG, WW_ERRCODE)
                    If isNormal(WW_ERRCODE) Then
                        Exit Sub
                    End If

                    '勤怠締テーブル取得
                    Dim WW_LIMITFLG As String = "0"
                    Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                    T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                       WW_ORG,
                                       work.WF_SEL_STYM.Text,
                                       WW_LIMITFLG,
                                       WW_ERR_RTN)
                    If Not isNormal(WW_ERR_RTN) Then
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                        Exit Sub
                    End If

                    '締まっていたらサマリーテーブルから取得するためスキップする
                    If WW_LIMITFLG = "0" Then
                        Continue For
                    End If

                    Try

                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = ""
                        PARA04.Value = Date.Now
                        PARA05.Value = C_MAX_YMD
                        PARA06.Value = C_DEFAULT_YMD
                        PARA07.Value = C_MAX_YMD
                        PARA08.Value = C_DEFAULT_YMD

                        '月末
                        Dim dt As Date = CDate(work.WF_SEL_STYM.Text & "/01")
                        PARA05.Value = dt.AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                        PARA06.Value = work.WF_SEL_STYM.Text & "/" & "01"
                        PARA07.Value = PARA05.Value
                        PARA08.Value = PARA06.Value
                        PARA09.Value = WI_ORG

                        SQLcmd.CommandTimeout = 300
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            '部署別に処理するためワークテーブルに読み込み（編集後にマージ）
                            WW_TA0005WKtbl = TA0005tbl.Clone
                            WW_TA0005WKtbl.Load(SQLdr)

                        End Using

                        For i As Integer = 0 To WW_TA0005WKtbl.Rows.Count - 1
                            Dim TA0005row As DataRow = TA0005tbl.NewRow
                            TA0005row.ItemArray = WW_TA0005WKtbl.Rows(i).ItemArray

                            If work.WF_SEL_FUNC.Text = "0" Then
                                '月調整を含めない
                                If TA0005row("ACACHANTEI") = "AMD" OrElse TA0005row("ACACHANTEI") = "AMC" Then
                                    Continue For
                                End If
                            End If

                            '〇判定Key作成
                            If IsDate(TA0005row("NACSHUKODATE")) AndAlso TA0005row("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                wDATE = TA0005row("NACSHUKODATE")
                                TA0005row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                            Else
                                TA0005row("NACSHUKODATE") = C_DEFAULT_YMD
                            End If
                            '固定項目
                            TA0005row("LINECNT") = 0                                                        'DBの固定フィールド(2017/11/9)
                            TA0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA                           'DBの固定フィールド(2017/11/9)
                            TA0005row("TIMSTP") = 0                                                         'DBの固定フィールド(2017/11/9)
                            TA0005row("SELECT") = "0"                                                       'DBの固定フィールド(2017/11/9)
                            TA0005row("HIDDEN") = 0                                                         'DBの固定フィールド(2017/11/9)

                            If IsDate(TA0005row("KEIJOYMD")) AndAlso TA0005row("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                wDATE = TA0005row("KEIJOYMD")
                                TA0005row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                            Else
                                TA0005row("KEIJOYMD") = C_DEFAULT_YMD
                            End If
                            If IsDate(TA0005row("DENYMD")) AndAlso TA0005row("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                wDATE = TA0005row("DENYMD")
                                TA0005row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                            Else
                                TA0005row("DENYMD") = C_DEFAULT_YMD
                            End If
                            If IsDate(TA0005row("NACSHUKODATE")) AndAlso TA0005row("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                wDATE = TA0005row("NACSHUKODATE")
                                TA0005row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                            Else
                                TA0005row("NACSHUKODATE") = C_DEFAULT_YMD
                            End If

                            '実績・配送作業開始日時
                            If IsDate(TA0005row("NACHAISTDATE")) AndAlso TA0005row("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACHAISTDATE")
                                TA0005row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACHAISTDATE") = C_DEFAULT_YMD
                            End If

                            '実績・配送作業終了日時
                            If IsDate(TA0005row("NACHAIENDDATE")) AndAlso TA0005row("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACHAIENDDATE")
                                TA0005row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACHAIENDDATE") = C_DEFAULT_YMD
                            End If

                            '実績・下車作業開始日時
                            If IsDate(TA0005row("NACGESSTDATE")) AndAlso TA0005row("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACGESSTDATE")
                                TA0005row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACGESSTDATE") = C_DEFAULT_YMD
                            End If

                            '実績・下車作業終了日時
                            If IsDate(TA0005row("NACGESENDDATE")) AndAlso TA0005row("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACGESENDDATE")
                                TA0005row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACGESENDDATE") = C_DEFAULT_YMD
                            End If

                            '休憩開始日時
                            If IsDate(TA0005row("NACBREAKSTDATE")) AndAlso TA0005row("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACBREAKSTDATE")
                                TA0005row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACBREAKSTDATE") = C_DEFAULT_YMD
                            End If

                            '休憩終了日時
                            If IsDate(TA0005row("NACBREAKENDDATE")) AndAlso TA0005row("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("NACBREAKENDDATE")
                                TA0005row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("NACBREAKENDDATE") = C_DEFAULT_YMD
                            End If

                            '出社日時
                            If IsDate(TA0005row("PAYSHUSHADATE")) AndAlso TA0005row("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("PAYSHUSHADATE")
                                TA0005row("PAYSHUSHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("PAYSHUSHADATE") = C_DEFAULT_YMD
                            End If

                            '退社日時
                            If IsDate(TA0005row("PAYTAISHADATE")) AndAlso TA0005row("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                wDATETime = TA0005row("PAYTAISHADATE")
                                TA0005row("PAYTAISHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                            Else
                                TA0005row("PAYTAISHADATE") = C_DEFAULT_YMD
                            End If

                            wINT = Val(TA0005row("NACHAIWORKTIME"))
                            TA0005row("NACHAIWORKTIME") = wINT                                      '実績・配送作業時間

                            wINT = Val(TA0005row("NACGESWORKTIME"))
                            TA0005row("NACGESWORKTIME") = wINT                                      '実績・下車作業時間

                            wINT = Val(TA0005row("NACCHOWORKTIME"))
                            TA0005row("NACCHOWORKTIME") = wINT                                      '実績・勤怠調整時間

                            wINT = Val(TA0005row("NACTTLWORKTIME"))
                            TA0005row("NACTTLWORKTIME") = wINT                                      '実績・配送合計時間Σ

                            wINT = Val(TA0005row("NACOUTWORKTIME"))
                            TA0005row("NACOUTWORKTIME") = wINT                                      '実績・就業外時間

                            wINT = Val(TA0005row("NACBREAKTIME"))
                            TA0005row("NACBREAKTIME") = wINT                                        '実績・休憩時間

                            wINT = Val(TA0005row("NACCHOBREAKTIME"))
                            TA0005row("NACCHOBREAKTIME") = wINT                                     '実績・休憩調整時間

                            wINT = Val(TA0005row("NACTTLBREAKTIME"))
                            TA0005row("NACTTLBREAKTIME") = wINT                                     '実績・休憩合計時間Σ

                            wINT = Val(TA0005row("NACOFFICETIME"))
                            TA0005row("NACOFFICETIME") = wINT                                       '実績・従業時間

                            wINT = Val(TA0005row("NACOFFICEBREAKTIME"))
                            TA0005row("NACOFFICEBREAKTIME") = wINT                                  '実績・従業休憩時間

                            wINT = Val(TA0005row("PAYWORKNISSU"))
                            TA0005row("PAYWORKNISSU") = wINT                                        '所労

                            wINT = Val(TA0005row("PAYSHOUKETUNISSU"))
                            TA0005row("PAYSHOUKETUNISSU") = wINT                                    '傷欠

                            wINT = Val(TA0005row("PAYKUMIKETUNISSU"))
                            TA0005row("PAYKUMIKETUNISSU") = wINT                                    '組欠

                            wINT = Val(TA0005row("PAYETCKETUNISSU"))
                            TA0005row("PAYETCKETUNISSU") = wINT                                     '他欠

                            wINT = Val(TA0005row("PAYNENKYUNISSU"))
                            TA0005row("PAYNENKYUNISSU") = wINT                                      '年休

                            wINT = Val(TA0005row("PAYTOKUKYUNISSU"))
                            TA0005row("PAYTOKUKYUNISSU") = wINT                                     '特休

                            wINT = Val(TA0005row("PAYCHIKOKSOTAINISSU"))
                            TA0005row("PAYCHIKOKSOTAINISSU") = wINT                                 '遅早

                            wINT = Val(TA0005row("PAYSTOCKNISSU"))
                            TA0005row("PAYSTOCKNISSU") = wINT                                       'ストック休暇

                            wINT = Val(TA0005row("PAYKYOTEIWEEKNISSU"))
                            TA0005row("PAYKYOTEIWEEKNISSU") = wINT                                  '協定週休

                            wINT = Val(TA0005row("PAYWEEKNISSU"))
                            TA0005row("PAYWEEKNISSU") = wINT                                        '週休

                            wINT = Val(TA0005row("PAYDAIKYUNISSU"))
                            TA0005row("PAYDAIKYUNISSU") = wINT                                      '代休

                            wINT = Val(TA0005row("PAYWORKTIME"))
                            TA0005row("PAYWORKTIME") = wINT                                         '所定労働時間

                            wINT = Val(TA0005row("PAYNIGHTTIME"))
                            TA0005row("PAYNIGHTTIME") = wINT                                        '所定深夜時間

                            wINT = Val(TA0005row("PAYORVERTIME"))
                            TA0005row("PAYORVERTIME") = wINT                                        '平日残業時間

                            wINT = Val(TA0005row("PAYWNIGHTTIME"))
                            TA0005row("PAYWNIGHTTIME") = wINT                                       '平日深夜時間

                            wINT = Val(TA0005row("PAYWSWORKTIME"))
                            TA0005row("PAYWSWORKTIME") = wINT                                       '日曜出勤時間

                            wINT = Val(TA0005row("PAYSNIGHTTIME"))
                            TA0005row("PAYSNIGHTTIME") = wINT                                       '日曜深夜時間

                            wINT = Val(TA0005row("PAYHWORKTIME"))
                            TA0005row("PAYHWORKTIME") = wINT                                        '休日出勤時間

                            wINT = Val(TA0005row("PAYHNIGHTTIME"))
                            TA0005row("PAYHNIGHTTIME") = wINT                                       '休日深夜時間

                            wINT = Val(TA0005row("PAYBREAKTIME"))
                            TA0005row("PAYBREAKTIME") = wINT                                        '休憩時間

                            wINT = Val(TA0005row("PAYNENSHINISSU"))
                            TA0005row("PAYNENSHINISSU") = wINT                                      '年始出勤

                            wINT = Val(TA0005row("PAYSHUKCHOKNNISSU"))
                            TA0005row("PAYSHUKCHOKNNISSU") = wINT                                   '宿日直年始

                            wINT = Val(TA0005row("PAYSHUKCHOKNISSU"))
                            TA0005row("PAYSHUKCHOKNISSU") = wINT                                    '宿日直通常

                            wINT = Val(TA0005row("PAYSHUKCHOKNHLDNISSU"))
                            TA0005row("PAYSHUKCHOKNHLDNISSU") = wINT                                '宿日直年始（翌休み）

                            wINT = Val(TA0005row("PAYSHUKCHOKHLDNISSU"))
                            TA0005row("PAYSHUKCHOKHLDNISSU") = wINT                                 '宿日直通常（翌休み）

                            wINT = Val(TA0005row("PAYTOKSAAKAISU"))
                            TA0005row("PAYTOKSAAKAISU") = wINT                                      '特作A

                            wINT = Val(TA0005row("PAYTOKSABKAISU"))
                            TA0005row("PAYTOKSABKAISU") = wINT                                      '特作B

                            wINT = Val(TA0005row("PAYTOKSACKAISU"))
                            TA0005row("PAYTOKSACKAISU") = wINT                                      '特作C

                            wINT = Val(TA0005row("PAYHOANTIME"))
                            TA0005row("PAYHOANTIME") = wINT                                         '保安検査入力

                            wINT = Val(TA0005row("PAYKOATUTIME"))
                            TA0005row("PAYKOATUTIME") = wINT                                        '高圧作業入力

                            wINT = Val(TA0005row("PAYTOKUSA1TIME"))
                            TA0005row("PAYTOKUSA1TIME") = wINT                                      '特作Ⅰ

                            wINT = Val(TA0005row("PAYPONPNISSU"))
                            TA0005row("PAYPONPNISSU") = wINT                                        'ポンプ

                            wINT = Val(TA0005row("PAYBULKNISSU"))
                            TA0005row("PAYBULKNISSU") = wINT                                        'バルク

                            wINT = Val(TA0005row("PAYTRAILERNISSU"))
                            TA0005row("PAYTRAILERNISSU") = wINT                                     'トレーラ

                            wINT = Val(TA0005row("PAYBKINMUKAISU"))
                            TA0005row("PAYBKINMUKAISU") = wINT                                      'B勤務

                            'マージ
                            TA0005tbl.Rows.Add(TA0005row)
                        Next

                        WW_TA0005WKtbl.Dispose()
                        WW_TA0005WKtbl = Nothing


                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0004_SUMMARYK SELECT")
                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:L0004_SUMMARYK Select"           '
                        CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWRITE.TEXT = ex.ToString()
                        CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        Exit Sub
                    End Try
                Next
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' 条件サマリー処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SumTA0005Work2()

        Dim wINT As Integer
        Dim WW_NACSURYOG As Double = 0
        Dim WW_NACJSURYOG As Double = 0

        Dim TA0005SUMtbl As DataTable = TA0005tbl.Clone
        Dim TA0005SUMrow As DataRow = Nothing
        Dim TA0005SVrow As DataRow = Nothing

        '***********************************************************************************************
        '一時サマリ（出荷部署、出庫日、出荷日、届日、荷主、業務車番、乗務員、トリップ、ドロップ別）
        '***********************************************************************************************
        'ソートキー設定
        Dim WW_SORT As String = String.Empty
        '部署別
        If WF_CBOX_SW1.Checked Then WW_SORT = If(String.IsNullOrEmpty(WW_SORT), WW_SORT, WW_SORT & ",") & "PAYHORG"
        '作業日別
        If WF_CBOX_SW2.Checked Then WW_SORT = If(String.IsNullOrEmpty(WW_SORT), WW_SORT, WW_SORT & ",") & "NACSHUKODATE"
        '従業員別
        If WF_CBOX_SW2.Checked Then WW_SORT = If(String.IsNullOrEmpty(WW_SORT), WW_SORT, WW_SORT & ",") & "PAYSTAFFCODE"
        '〇デフォルトソートの設定
        WW_SORT = If(String.IsNullOrEmpty(WW_SORT), WW_SORT, WW_SORT & ",") & "RECODEKBN"

        'ソート
        CS0026TblSort.TABLE = TA0005VIEWtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = WW_SORT
        TA0005VIEWtbl = CS0026TblSort.sort()

        Dim WW_KEY As String = ""
        Dim WW_KEY_OLD As String = ""
        Dim WW_FIRST As String = "OFF"

        TA0005SUMtbl.Clear()
        TA0005SUMrow = Nothing
        TA0005SVrow = Nothing

        For Each TA0005row As DataRow In TA0005VIEWtbl.Rows

            WW_KEY = ""

            '部署別
            If WF_CBOX_SW1.Checked = True Then WW_KEY = WW_KEY & TA0005row("PAYHORG") & "_"

            '作業日別
            If WF_CBOX_SW2.Checked = True Then WW_KEY = WW_KEY & TA0005row("NACSHUKODATE") & "_"

            '従業員別
            If WF_CBOX_SW3.Checked = True Then WW_KEY = WW_KEY & TA0005row("PAYSTAFFCODE") & "_"

            'レコード区分別
            WW_KEY = WW_KEY & TA0005row("RECODEKBN") & "_"

            If WW_FIRST = "OFF" Then
                '初回のみブレイクキーを設定
                WW_KEY_OLD = ""

                '部署別
                If WF_CBOX_SW1.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0005row("PAYHORG") & "_"
                End If

                '作業日別
                If WF_CBOX_SW2.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0005row("NACSHUKODATE") & "_"
                End If

                '従業員別
                If WF_CBOX_SW3.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0005row("PAYSTAFFCODE") & "_"
                End If

                WW_KEY_OLD = WW_KEY_OLD & TA0005row("RECODEKBN") & "_"

                TA0005SVrow = TA0005SUMtbl.NewRow
                TA0005SVrow.ItemArray = TA0005row.ItemArray
                'サマリー項目初期化
                InitalSummaryItem(TA0005SVrow)
                WW_FIRST = "ON"
            End If

            'ブレイクキーが変わったらサマリー結果を出力
            If WW_KEY_OLD = WW_KEY Then
            Else
                TA0005SUMrow = TA0005SUMtbl.NewRow
                TA0005SUMrow.ItemArray = TA0005SVrow.ItemArray
                TA0005SUMtbl.Rows.Add(TA0005SUMrow)

                TA0005SVrow = TA0005SUMtbl.NewRow
                TA0005SVrow.ItemArray = TA0005row.ItemArray
                'サマリー項目初期化
                InitalSummaryItem(TA0005SVrow)
            End If

            '部署別
            If WF_CBOX_SW1.Checked = False Then
                TA0005SVrow("PAYHORG") = ""                    '配属部署 
                TA0005SVrow("PAYHORGNAME") = ""                '配属部署名称
            End If

            '作業日別
            If WF_CBOX_SW2.Checked = False Then
                TA0005SVrow("NACSHUKODATE") = ""               '出庫日・作業日
            End If

            '従業員別
            If WF_CBOX_SW3.Checked = False Then
                TA0005SVrow("PAYSTAFFCODE") = ""               '出荷日
            End If

            '********************************
            ' 以降、編集（サマリー）処理
            '********************************

            '---------------
            '労務費（配送作業）
            '---------------
            wINT = Val(TA0005row("NACHAIDISTANCE_1_1"))
            TA0005SVrow("NACHAIDISTANCE_1_1") = Val(TA0005SVrow("NACHAIDISTANCE_1_1")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_2"))
            TA0005SVrow("NACHAIDISTANCE_1_2") = Val(TA0005SVrow("NACHAIDISTANCE_1_2")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_3"))
            TA0005SVrow("NACHAIDISTANCE_1_3") = Val(TA0005SVrow("NACHAIDISTANCE_1_3")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_4"))
            TA0005SVrow("NACHAIDISTANCE_1_4") = Val(TA0005SVrow("NACHAIDISTANCE_1_4")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_5"))
            TA0005SVrow("NACHAIDISTANCE_1_5") = Val(TA0005SVrow("NACHAIDISTANCE_1_5")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_6"))
            TA0005SVrow("NACHAIDISTANCE_1_6") = Val(TA0005SVrow("NACHAIDISTANCE_1_6")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_7"))
            TA0005SVrow("NACHAIDISTANCE_1_7") = Val(TA0005SVrow("NACHAIDISTANCE_1_7")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_8"))
            TA0005SVrow("NACHAIDISTANCE_1_8") = Val(TA0005SVrow("NACHAIDISTANCE_1_8")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_9"))
            TA0005SVrow("NACHAIDISTANCE_1_9") = Val(TA0005SVrow("NACHAIDISTANCE_1_9")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_1_10"))
            TA0005SVrow("NACHAIDISTANCE_1_10") = Val(TA0005SVrow("NACHAIDISTANCE_1_10")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_1"))
            TA0005SVrow("NACHAIDISTANCE_2_1") = Val(TA0005SVrow("NACHAIDISTANCE_2_1")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_2"))
            TA0005SVrow("NACHAIDISTANCE_2_2") = Val(TA0005SVrow("NACHAIDISTANCE_2_2")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_3"))
            TA0005SVrow("NACHAIDISTANCE_2_3") = Val(TA0005SVrow("NACHAIDISTANCE_2_3")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_4"))
            TA0005SVrow("NACHAIDISTANCE_2_4") = Val(TA0005SVrow("NACHAIDISTANCE_2_4")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_5"))
            TA0005SVrow("NACHAIDISTANCE_2_5") = Val(TA0005SVrow("NACHAIDISTANCE_2_5")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_6"))
            TA0005SVrow("NACHAIDISTANCE_2_6") = Val(TA0005SVrow("NACHAIDISTANCE_2_6")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_7"))
            TA0005SVrow("NACHAIDISTANCE_2_7") = Val(TA0005SVrow("NACHAIDISTANCE_2_7")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_8"))
            TA0005SVrow("NACHAIDISTANCE_2_8") = Val(TA0005SVrow("NACHAIDISTANCE_2_8")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_9"))
            TA0005SVrow("NACHAIDISTANCE_2_9") = Val(TA0005SVrow("NACHAIDISTANCE_2_9")) + wINT               '実績・配送距離

            wINT = Val(TA0005row("NACHAIDISTANCE_2_10"))
            TA0005SVrow("NACHAIDISTANCE_2_10") = Val(TA0005SVrow("NACHAIDISTANCE_2_10")) + wINT               '実績・配送距離

            '---------------
            '労務費（回送）
            '---------------
            wINT = Val(TA0005row("NACKAIDISTANCE_1_1"))
            TA0005SVrow("NACKAIDISTANCE_1_1") = Val(TA0005SVrow("NACKAIDISTANCE_1_1")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_2"))
            TA0005SVrow("NACKAIDISTANCE_1_2") = Val(TA0005SVrow("NACKAIDISTANCE_1_2")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_3"))
            TA0005SVrow("NACKAIDISTANCE_1_3") = Val(TA0005SVrow("NACKAIDISTANCE_1_3")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_4"))
            TA0005SVrow("NACKAIDISTANCE_1_4") = Val(TA0005SVrow("NACKAIDISTANCE_1_4")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_5"))
            TA0005SVrow("NACKAIDISTANCE_1_5") = Val(TA0005SVrow("NACKAIDISTANCE_1_5")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_6"))
            TA0005SVrow("NACKAIDISTANCE_1_6") = Val(TA0005SVrow("NACKAIDISTANCE_1_6")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_7"))
            TA0005SVrow("NACKAIDISTANCE_1_7") = Val(TA0005SVrow("NACKAIDISTANCE_1_7")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_8"))
            TA0005SVrow("NACKAIDISTANCE_1_8") = Val(TA0005SVrow("NACKAIDISTANCE_1_8")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_9"))
            TA0005SVrow("NACKAIDISTANCE_1_9") = Val(TA0005SVrow("NACKAIDISTANCE_1_9")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_1_10"))
            TA0005SVrow("NACKAIDISTANCE_1_10") = Val(TA0005SVrow("NACKAIDISTANCE_1_10")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_1"))
            TA0005SVrow("NACKAIDISTANCE_2_1") = Val(TA0005SVrow("NACKAIDISTANCE_2_1")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_2"))
            TA0005SVrow("NACKAIDISTANCE_2_2") = Val(TA0005SVrow("NACKAIDISTANCE_2_2")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_3"))
            TA0005SVrow("NACKAIDISTANCE_2_3") = Val(TA0005SVrow("NACKAIDISTANCE_2_3")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_4"))
            TA0005SVrow("NACKAIDISTANCE_2_4") = Val(TA0005SVrow("NACKAIDISTANCE_2_4")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_5"))
            TA0005SVrow("NACKAIDISTANCE_2_5") = Val(TA0005SVrow("NACKAIDISTANCE_2_5")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_6"))
            TA0005SVrow("NACKAIDISTANCE_2_6") = Val(TA0005SVrow("NACKAIDISTANCE_2_6")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_7"))
            TA0005SVrow("NACKAIDISTANCE_2_7") = Val(TA0005SVrow("NACKAIDISTANCE_2_7")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_8"))
            TA0005SVrow("NACKAIDISTANCE_2_8") = Val(TA0005SVrow("NACKAIDISTANCE_2_8")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_9"))
            TA0005SVrow("NACKAIDISTANCE_2_9") = Val(TA0005SVrow("NACKAIDISTANCE_2_9")) + wINT               '実績・下車作業距離

            wINT = Val(TA0005row("NACKAIDISTANCE_2_10"))
            TA0005SVrow("NACKAIDISTANCE_2_10") = Val(TA0005SVrow("NACKAIDISTANCE_2_10")) + wINT               '実績・下車作業距離

            '労務費（配送距離合計）
            wINT = Val(TA0005row("NACTTLDISTANCE_1_1"))
            TA0005SVrow("NACTTLDISTANCE_1_1") = Val(TA0005SVrow("NACTTLDISTANCE_1_1")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_2"))
            TA0005SVrow("NACTTLDISTANCE_1_2") = Val(TA0005SVrow("NACTTLDISTANCE_1_2")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_3"))
            TA0005SVrow("NACTTLDISTANCE_1_3") = Val(TA0005SVrow("NACTTLDISTANCE_1_3")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_4"))
            TA0005SVrow("NACTTLDISTANCE_1_4") = Val(TA0005SVrow("NACTTLDISTANCE_1_4")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_5"))
            TA0005SVrow("NACTTLDISTANCE_1_5") = Val(TA0005SVrow("NACTTLDISTANCE_1_5")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_6"))
            TA0005SVrow("NACTTLDISTANCE_1_6") = Val(TA0005SVrow("NACTTLDISTANCE_1_6")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_7"))
            TA0005SVrow("NACTTLDISTANCE_1_7") = Val(TA0005SVrow("NACTTLDISTANCE_1_7")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_8"))
            TA0005SVrow("NACTTLDISTANCE_1_8") = Val(TA0005SVrow("NACTTLDISTANCE_1_8")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_9"))
            TA0005SVrow("NACTTLDISTANCE_1_9") = Val(TA0005SVrow("NACTTLDISTANCE_1_9")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_1_10"))
            TA0005SVrow("NACTTLDISTANCE_1_10") = Val(TA0005SVrow("NACTTLDISTANCE_1_10")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_1"))
            TA0005SVrow("NACTTLDISTANCE_2_1") = Val(TA0005SVrow("NACTTLDISTANCE_2_1")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_2"))
            TA0005SVrow("NACTTLDISTANCE_2_2") = Val(TA0005SVrow("NACTTLDISTANCE_2_2")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_3"))
            TA0005SVrow("NACTTLDISTANCE_2_3") = Val(TA0005SVrow("NACTTLDISTANCE_2_3")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_4"))
            TA0005SVrow("NACTTLDISTANCE_2_4") = Val(TA0005SVrow("NACTTLDISTANCE_2_4")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_5"))
            TA0005SVrow("NACTTLDISTANCE_2_5") = Val(TA0005SVrow("NACTTLDISTANCE_2_5")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_6"))
            TA0005SVrow("NACTTLDISTANCE_2_6") = Val(TA0005SVrow("NACTTLDISTANCE_2_6")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_7"))
            TA0005SVrow("NACTTLDISTANCE_2_7") = Val(TA0005SVrow("NACTTLDISTANCE_2_7")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_8"))
            TA0005SVrow("NACTTLDISTANCE_2_8") = Val(TA0005SVrow("NACTTLDISTANCE_2_8")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_9"))
            TA0005SVrow("NACTTLDISTANCE_2_9") = Val(TA0005SVrow("NACTTLDISTANCE_2_9")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLDISTANCE_2_10"))
            TA0005SVrow("NACTTLDISTANCE_2_10") = Val(TA0005SVrow("NACTTLDISTANCE_2_10")) + wINT                   '実績・配送距離合計Σ

            wINT = Val(TA0005row("NACTTLUNLOADCNT_G"))
            TA0005SVrow("NACTTLUNLOADCNT_G") = Val(TA0005SVrow("NACTTLUNLOADCNT_G")) + wINT                   '実績・合計Σ

            '労務費（配送作業）
            TA0005SVrow("NACHAISTDATE") = ""                                                        '実績・配送作業開始日時
            TA0005SVrow("NACHAIENDDATE") = ""                                                       '実績・配送作業終了日時
            wINT = Val(TA0005row("NACHAIWORKTIME"))
            TA0005SVrow("NACHAIWORKTIME") = Val(TA0005SVrow("NACHAIWORKTIME")) + wINT               '実績・配送作業時間

            '労務費（回送）
            TA0005SVrow("NACGESSTDATE") = ""                                                        '実績・下車作業開始日時
            TA0005SVrow("NACGESENDDATE") = ""                                                       '実績・下車作業終了日時
            wINT = Val(TA0005row("NACGESWORKTIME"))
            TA0005SVrow("NACGESWORKTIME") = Val(TA0005SVrow("NACGESWORKTIME")) + wINT               '実績・下車作業時間

            wINT = Val(TA0005row("NACCHOWORKTIME"))
            TA0005SVrow("NACCHOWORKTIME") = Val(TA0005SVrow("NACCHOWORKTIME")) + wINT                   '実績・勤怠調整時間

            wINT = Val(TA0005row("NACTTLWORKTIME"))
            TA0005SVrow("NACTTLWORKTIME") = Val(TA0005SVrow("NACTTLWORKTIME")) + wINT                   '実績・配送合計時間Σ


            '労務費（配送作業）& 労務費（回送）
            wINT = Val(TA0005row("NACOUTWORKTIME"))
            TA0005SVrow("NACOUTWORKTIME") = Val(TA0005SVrow("NACOUTWORKTIME")) + wINT               '実績・就業外時間

            '労務費（休憩）
            TA0005SVrow("NACBREAKSTDATE") = ""                                                     '実績・休憩開始日時
            TA0005SVrow("NACBREAKENDDATE") = ""                                                    '実績・休憩終了日時
            wINT = Val(TA0005row("NACBREAKTIME"))
            TA0005SVrow("NACBREAKTIME") = Val(TA0005SVrow("NACBREAKTIME")) + wINT                  '実績・休憩時間
            wINT = Val(TA0005row("NACCHOBREAKTIME"))
            TA0005SVrow("NACCHOBREAKTIME") = Val(TA0005SVrow("NACCHOBREAKTIME")) + wINT            '実績・休憩調整時間
            wINT = Val(TA0005row("NACTTLBREAKTIME"))
            TA0005SVrow("NACTTLBREAKTIME") = Val(TA0005SVrow("NACTTLBREAKTIME")) + wINT            '実績・休憩合計時間Σ

            '労務費（配送作業）
            wINT = Val(TA0005row("NACUNLOADCNT_1_1"))
            TA0005SVrow("NACUNLOADCNT_1_1") = Val(TA0005SVrow("NACUNLOADCNT_1_1")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_2"))
            TA0005SVrow("NACUNLOADCNT_1_2") = Val(TA0005SVrow("NACUNLOADCNT_1_2")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_3"))
            TA0005SVrow("NACUNLOADCNT_1_3") = Val(TA0005SVrow("NACUNLOADCNT_1_3")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_4"))
            TA0005SVrow("NACUNLOADCNT_1_4") = Val(TA0005SVrow("NACUNLOADCNT_1_4")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_5"))
            TA0005SVrow("NACUNLOADCNT_1_5") = Val(TA0005SVrow("NACUNLOADCNT_1_5")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_6"))
            TA0005SVrow("NACUNLOADCNT_1_6") = Val(TA0005SVrow("NACUNLOADCNT_1_6")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_7"))
            TA0005SVrow("NACUNLOADCNT_1_7") = Val(TA0005SVrow("NACUNLOADCNT_1_7")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_8"))
            TA0005SVrow("NACUNLOADCNT_1_8") = Val(TA0005SVrow("NACUNLOADCNT_1_8")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_9"))
            TA0005SVrow("NACUNLOADCNT_1_9") = Val(TA0005SVrow("NACUNLOADCNT_1_9")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_1_10"))
            TA0005SVrow("NACUNLOADCNT_1_10") = Val(TA0005SVrow("NACUNLOADCNT_1_10")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_1"))
            TA0005SVrow("NACUNLOADCNT_2_1") = Val(TA0005SVrow("NACUNLOADCNT_2_1")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_2"))
            TA0005SVrow("NACUNLOADCNT_2_2") = Val(TA0005SVrow("NACUNLOADCNT_2_2")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_3"))
            TA0005SVrow("NACUNLOADCNT_2_3") = Val(TA0005SVrow("NACUNLOADCNT_2_3")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_4"))
            TA0005SVrow("NACUNLOADCNT_2_4") = Val(TA0005SVrow("NACUNLOADCNT_2_4")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_5"))
            TA0005SVrow("NACUNLOADCNT_2_5") = Val(TA0005SVrow("NACUNLOADCNT_2_5")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_6"))
            TA0005SVrow("NACUNLOADCNT_2_6") = Val(TA0005SVrow("NACUNLOADCNT_2_6")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_7"))
            TA0005SVrow("NACUNLOADCNT_2_7") = Val(TA0005SVrow("NACUNLOADCNT_2_7")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_8"))
            TA0005SVrow("NACUNLOADCNT_2_8") = Val(TA0005SVrow("NACUNLOADCNT_2_8")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_9"))
            TA0005SVrow("NACUNLOADCNT_2_9") = Val(TA0005SVrow("NACUNLOADCNT_2_9")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACUNLOADCNT_2_10"))
            TA0005SVrow("NACUNLOADCNT_2_10") = Val(TA0005SVrow("NACUNLOADCNT_2_10")) + wINT                  '実績・荷卸回数

            wINT = Val(TA0005row("NACTTLDISTANCE_G"))
            TA0005SVrow("NACTTLDISTANCE_G") = Val(TA0005SVrow("NACTTLDISTANCE_G")) + wINT                  '実績・荷卸回数

            TA0005SVrow("NACOFFICESORG") = TA0005row("NACOFFICESORG")                             '実績・従業作業部署
            TA0005SVrow("NACOFFICESORGNAME") = TA0005row("NACOFFICESORGNAME")                     '実績・従業作業部署名称
            wINT = Val(TA0005row("NACOFFICETIME"))
            TA0005SVrow("NACOFFICETIME") = Val(TA0005SVrow("NACOFFICETIME")) + wINT                 '実績・従業時間
            wINT = Val(TA0005row("NACOFFICEBREAKTIME"))
            TA0005SVrow("NACOFFICEBREAKTIME") = Val(TA0005SVrow("NACOFFICEBREAKTIME")) + wINT       '実績・従業休憩時間
            If WF_CBOX_SW3.Checked Then
                TA0005SVrow("PAYSHUSHADATE") = TA0005row("PAYSHUSHADATE")                          '出社日時
                TA0005SVrow("PAYTAISHADATE") = TA0005row("PAYTAISHADATE")                          '退社日時
                TA0005SVrow("PAYSTAFFKBN") = TA0005row("PAYSTAFFKBN")                              '社員区分
                TA0005SVrow("PAYSTAFFKBNNAME") = TA0005row("PAYSTAFFKBNNAME")                      '社員区分名称
                TA0005SVrow("PAYSTAFFCODE") = TA0005row("PAYSTAFFCODE")                            '従業員
                TA0005SVrow("PAYSTAFFCODENAME") = TA0005row("PAYSTAFFCODENAME")                    '従業員名称
                TA0005SVrow("PAYMORG") = TA0005row("PAYMORG")                                      '従業員管理部署
                TA0005SVrow("PAYMORGNAME") = TA0005row("PAYMORGNAME")                              '従業員管理部署名称
                TA0005SVrow("PAYHORG") = TA0005row("PAYHORG")                                      '従業員配属部署
                TA0005SVrow("PAYHORGNAME") = TA0005row("PAYHORGNAME")                              '従業員配属部署名称
                TA0005SVrow("NACOFFICESORG") = TA0005row("NACOFFICESORG")                          '従業員配属部署
                TA0005SVrow("NACOFFICESORGNAME") = TA0005row("NACOFFICESORGNAME")                  '従業員配属部署名称
            Else
                TA0005SVrow("PAYSHUSHADATE") = ""                                                    '出社日時
                TA0005SVrow("PAYTAISHADATE") = ""                                                    '退社日時
                TA0005SVrow("PAYSTAFFKBN") = ""                                                      '社員区分
                TA0005SVrow("PAYSTAFFKBNNAME") = ""                                                  '社員区分名称
                TA0005SVrow("PAYSTAFFCODE") = ""                                                     '従業員
                TA0005SVrow("PAYSTAFFCODENAME") = ""                                                 '従業員名称
            End If

            If WF_CBOX_SW2.Checked = True Then
                If WF_CBOX_SW3.Checked = True Then
                    TA0005SVrow("PAYHOLIDAYKBN") = TA0005row("PAYHOLIDAYKBN")                            '休日区分
                    TA0005SVrow("PAYHOLIDAYKBNNAME") = TA0005row("PAYHOLIDAYKBNNAME")                    '休日区分名称
                    TA0005SVrow("PAYKBN") = TA0005row("PAYKBN")                                          '勤怠区分
                    TA0005SVrow("PAYKBNNAME") = TA0005row("PAYKBNNAME")                                  '勤怠区分名称
                    TA0005SVrow("PAYSHUKCHOKKBN") = TA0005row("PAYSHUKCHOKKBN")                          '宿日直区分
                    TA0005SVrow("PAYSHUKCHOKKBNNAME") = TA0005row("PAYSHUKCHOKKBNNAME")                  '宿日直区分名称
                    TA0005SVrow("PAYJYOMUKBN") = TA0005row("PAYJYOMUKBN")                                '乗務区分
                    TA0005SVrow("PAYJYOMUKBNNAME") = TA0005row("PAYJYOMUKBNNAME")                        '乗務区分名称
                Else
                    TA0005SVrow("PAYHOLIDAYKBN") = TA0005row("PAYHOLIDAYKBN")                            '休日区分
                    TA0005SVrow("PAYHOLIDAYKBNNAME") = TA0005row("PAYHOLIDAYKBNNAME")                    '休日区分名称
                    TA0005SVrow("PAYKBN") = ""                                                             '勤怠区分
                    TA0005SVrow("PAYKBNNAME") = ""                                                         '勤怠区分名称
                    TA0005SVrow("PAYSHUKCHOKKBN") = ""                                                     '宿日直区分
                    TA0005SVrow("PAYSHUKCHOKKBNNAME") = ""                                                 '宿日直区分名称
                    TA0005SVrow("PAYJYOMUKBN") = ""                                                        '乗務区分
                    TA0005SVrow("PAYJYOMUKBNNAME") = ""                                                    '乗務区分名称
                End If
            Else
                TA0005SVrow("PAYSHUSHADATE") = ""                                                      '出社日時
                TA0005SVrow("PAYTAISHADATE") = ""                                                      '退社日時
                TA0005SVrow("PAYHOLIDAYKBN") = ""                                                      '休日区分
                TA0005SVrow("PAYHOLIDAYKBNNAME") = ""                                                  '休日区分名称
                TA0005SVrow("PAYKBN") = ""                                                             '勤怠区分
                TA0005SVrow("PAYKBNNAME") = ""                                                         '勤怠区分名称
                TA0005SVrow("PAYSHUKCHOKKBN") = ""                                                     '宿日直区分
                TA0005SVrow("PAYSHUKCHOKKBNNAME") = ""                                                 '宿日直区分名称
                TA0005SVrow("PAYJYOMUKBN") = ""                                                        '乗務区分
                TA0005SVrow("PAYJYOMUKBNNAME") = ""                                                    '乗務区分名称
            End If

            If WF_CBOX_SW1.Checked = True Then
                TA0005SVrow("NACOFFICESORG") = TA0005row("NACOFFICESORG")                          '従業員配属部署
                TA0005SVrow("NACOFFICESORGNAME") = TA0005row("NACOFFICESORGNAME")                  '従業員配属部署名称
            Else
                TA0005SVrow("NACOFFICESORG") = ""                                                    '従業員配属部署
                TA0005SVrow("NACOFFICESORGNAME") = ""                                                '従業員配属部署名称
            End If

            wINT = Val(TA0005row("WORKTIMEMIN"))
            TA0005SVrow("WORKTIMEMIN") = Val(TA0005SVrow("WORKTIMEMIN")) + wINT                      '拘束時間（分）
            TA0005SVrow("WORKTIME") = MinutesToHHMM(TA0005SVrow("WORKTIMEMIN"))                         '拘束時間

            wINT = Val(TA0005row("WORKTIMEMIN16UP"))
            TA0005SVrow("WORKTIMEMIN16UP") = Val(TA0005SVrow("WORKTIMEMIN16UP")) + wINT              '拘束時間１６時間超（回数）

            If WF_CBOX_SW2.Checked = False OrElse WF_CBOX_SW2.Checked = False Then
                TA0005SVrow("PAYWORKNISSU") = 0
            Else
                wINT = Val(TA0005row("PAYWORKNISSU"))
                If wINT > 0 Then
                    TA0005SVrow("PAYWORKNISSU") = wINT                                                 '所労
                End If
            End If

            wINT = Val(TA0005row("PAYSHOUKETUNISSU"))
            TA0005SVrow("PAYSHOUKETUNISSU") = Val(TA0005SVrow("PAYSHOUKETUNISSU")) + wINT          '傷欠
            wINT = Val(TA0005row("PAYKUMIKETUNISSU"))
            TA0005SVrow("PAYKUMIKETUNISSU") = Val(TA0005SVrow("PAYKUMIKETUNISSU")) + wINT          '組欠
            wINT = Val(TA0005row("PAYETCKETUNISSU"))
            TA0005SVrow("PAYETCKETUNISSU") = Val(TA0005SVrow("PAYETCKETUNISSU")) + wINT            '他欠
            wINT = Val(TA0005row("PAYNENKYUNISSU"))
            TA0005SVrow("PAYNENKYUNISSU") = Val(TA0005SVrow("PAYNENKYUNISSU")) + wINT              '年休
            wINT = Val(TA0005row("PAYTOKUKYUNISSU"))
            TA0005SVrow("PAYTOKUKYUNISSU") = Val(TA0005SVrow("PAYTOKUKYUNISSU")) + wINT            '特休
            wINT = Val(TA0005row("PAYCHIKOKSOTAINISSU"))
            TA0005SVrow("PAYCHIKOKSOTAINISSU") = Val(TA0005SVrow("PAYCHIKOKSOTAINISSU")) + wINT    '遅早
            wINT = Val(TA0005row("PAYSTOCKNISSU"))
            TA0005SVrow("PAYSTOCKNISSU") = Val(TA0005SVrow("PAYSTOCKNISSU")) + wINT                'ストック休暇
            wINT = Val(TA0005row("PAYKYOTEIWEEKNISSU"))
            TA0005SVrow("PAYKYOTEIWEEKNISSU") = Val(TA0005SVrow("PAYKYOTEIWEEKNISSU")) + wINT      '協定週休
            wINT = Val(TA0005row("PAYWEEKNISSU"))
            TA0005SVrow("PAYWEEKNISSU") = Val(TA0005SVrow("PAYWEEKNISSU")) + wINT                  '週休
            wINT = Val(TA0005row("PAYDAIKYUNISSU"))
            TA0005SVrow("PAYDAIKYUNISSU") = Val(TA0005SVrow("PAYDAIKYUNISSU")) + wINT              '代休
            wINT = Val(TA0005row("PAYWORKTIME"))
            TA0005SVrow("PAYWORKTIME") = Val(TA0005SVrow("PAYWORKTIME")) + wINT                    '所定労働時間
            wINT = Val(TA0005row("PAYNIGHTTIME"))
            TA0005SVrow("PAYNIGHTTIME") = Val(TA0005SVrow("PAYNIGHTTIME")) + wINT                  '所定深夜時間
            wINT = Val(TA0005row("PAYORVERTIME"))
            TA0005SVrow("PAYORVERTIME") = Val(TA0005SVrow("PAYORVERTIME")) + wINT                  '平日残業時間
            wINT = Val(TA0005row("PAYWNIGHTTIME"))
            TA0005SVrow("PAYWNIGHTTIME") = Val(TA0005SVrow("PAYWNIGHTTIME")) + wINT                '平日深夜時間
            wINT = Val(TA0005row("PAYWSWORKTIME"))
            TA0005SVrow("PAYWSWORKTIME") = Val(TA0005SVrow("PAYWSWORKTIME")) + wINT                '日曜出勤時間
            wINT = Val(TA0005row("PAYSNIGHTTIME"))
            TA0005SVrow("PAYSNIGHTTIME") = Val(TA0005SVrow("PAYSNIGHTTIME")) + wINT                '日曜深夜時間
            wINT = Val(TA0005row("PAYHWORKTIME"))
            TA0005SVrow("PAYHWORKTIME") = Val(TA0005SVrow("PAYHWORKTIME")) + wINT                  '休日出勤時間
            wINT = Val(TA0005row("PAYHNIGHTTIME"))
            TA0005SVrow("PAYHNIGHTTIME") = Val(TA0005SVrow("PAYHNIGHTTIME")) + wINT                '休日深夜時間
            wINT = Val(TA0005row("PAYBREAKTIME"))
            TA0005SVrow("PAYBREAKTIME") = Val(TA0005SVrow("PAYBREAKTIME")) + wINT                  '休憩時間
            wINT = Val(TA0005row("PAYNENSHINISSU"))
            TA0005SVrow("PAYNENSHINISSU") = Val(TA0005SVrow("PAYNENSHINISSU")) + wINT              '年始出勤
            wINT = Val(TA0005row("PAYSHUKCHOKNNISSU"))
            TA0005SVrow("PAYSHUKCHOKNNISSU") = Val(TA0005SVrow("PAYSHUKCHOKNNISSU")) + wINT        '宿日直年始
            wINT = Val(TA0005row("PAYSHUKCHOKNISSU"))
            TA0005SVrow("PAYSHUKCHOKNISSU") = Val(TA0005SVrow("PAYSHUKCHOKNISSU")) + wINT          '宿日直通常
            wINT = Val(TA0005row("PAYSHUKCHOKNHLDNISSU"))
            TA0005SVrow("PAYSHUKCHOKNHLDNISSU") = Val(TA0005SVrow("PAYSHUKCHOKNHLDNISSU")) + wINT  '宿日直年始（翌休み）
            wINT = Val(TA0005row("PAYSHUKCHOKHLDNISSU"))
            TA0005SVrow("PAYSHUKCHOKHLDNISSU") = Val(TA0005SVrow("PAYSHUKCHOKHLDNISSU")) + wINT    '宿日直通常（翌休み）
            wINT = Val(TA0005row("PAYTOKSAAKAISU"))
            TA0005SVrow("PAYTOKSAAKAISU") = Val(TA0005SVrow("PAYTOKSAAKAISU")) + wINT              '特作A
            wINT = Val(TA0005row("PAYTOKSABKAISU"))
            TA0005SVrow("PAYTOKSABKAISU") = Val(TA0005SVrow("PAYTOKSABKAISU")) + wINT              '特作B
            wINT = Val(TA0005row("PAYTOKSACKAISU"))
            TA0005SVrow("PAYTOKSACKAISU") = Val(TA0005SVrow("PAYTOKSACKAISU")) + wINT              '特作C
            wINT = Val(TA0005row("PAYHOANTIME"))
            TA0005SVrow("PAYHOANTIME") = Val(TA0005SVrow("PAYHOANTIME")) + wINT                    '保安検査入力
            wINT = Val(TA0005row("PAYKOATUTIME"))
            TA0005SVrow("PAYKOATUTIME") = Val(TA0005SVrow("PAYKOATUTIME")) + wINT                  '高圧作業入力
            wINT = Val(TA0005row("PAYTOKUSA1TIME"))
            TA0005SVrow("PAYTOKUSA1TIME") = Val(TA0005SVrow("PAYTOKUSA1TIME")) + wINT              '特作Ⅰ
            wINT = Val(TA0005row("PAYPONPNISSU"))
            TA0005SVrow("PAYPONPNISSU") = Val(TA0005SVrow("PAYPONPNISSU")) + wINT                  'ポンプ
            wINT = Val(TA0005row("PAYBULKNISSU"))
            TA0005SVrow("PAYBULKNISSU") = Val(TA0005SVrow("PAYBULKNISSU")) + wINT                  'バルク
            wINT = Val(TA0005row("PAYTRAILERNISSU"))
            TA0005SVrow("PAYTRAILERNISSU") = Val(TA0005SVrow("PAYTRAILERNISSU")) + wINT            'トレーラ
            wINT = Val(TA0005row("PAYBKINMUKAISU"))
            TA0005SVrow("PAYBKINMUKAISU") = Val(TA0005SVrow("PAYBKINMUKAISU")) + wINT              'B勤務

            TA0005SVrow("RECODEKBN") = TA0005row("RECODEKBN")                                    'レコード区分

            TA0005SVrow("DENYMD") = ""
            TA0005SVrow("DENNO") = ""
            TA0005SVrow("KANRENDENNO") = ""
            TA0005SVrow("DTLNO") = ""
            TA0005SVrow("ACACHANTEI") = ""
            TA0005SVrow("ACACHANTEINAME") = ""
            TA0005SVrow("WORKKBN") = ""
            TA0005SVrow("WORKKBNNAME") = ""


            WW_KEY_OLD = ""

            '部署別
            If WF_CBOX_SW1.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0005row("PAYHORG") & "_"
            End If

            '作業日別
            If WF_CBOX_SW2.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0005row("NACSHUKODATE") & "_"
            End If

            '従業員別
            If WF_CBOX_SW3.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0005row("PAYSTAFFCODE") & "_"
            End If

            WW_KEY_OLD = WW_KEY_OLD & TA0005row("RECODEKBN") & "_"

        Next
        '最終レコードの出力
        If TA0005VIEWtbl.Rows.Count > 0 Then
            TA0005SUMrow = TA0005SUMtbl.NewRow
            TA0005SUMrow.ItemArray = TA0005SVrow.ItemArray
            TA0005SUMtbl.Rows.Add(TA0005SUMrow)
        End If
        'ソート
        CS0026TblSort.TABLE = TA0005SUMtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "PAYHORG,PAYSTAFFCODE,NACSHUKODATE,RECODEKBN"
        TA0005SUMtbl = CS0026TblSort.sort()
        'サマリー結果で入れ替え
        TA0005VIEWtbl = TA0005SUMtbl.Copy

    End Sub

    ''' <summary>
    ''' サマーリー項目初期化
    ''' </summary>
    ''' <param name="IO_ROW">初期化するテーブル</param>
    ''' <remarks></remarks>
    Protected Sub InitalSummaryItem(ByRef IO_ROW As DataRow)

        IO_ROW("NACHAIDISTANCE_1_1") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_1") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_1") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_2") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_2") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_2") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_3") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_3") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_3") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_4") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_4") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_4") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_5") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_5") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_5") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_6") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_6") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_6") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_7") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_7") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_7") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_8") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_8") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_8") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_9") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_9") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_9") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_1_10") = 0               '実績・配送距離
        IO_ROW("NACKAIDISTANCE_1_10") = 0               '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_1_10") = 0               '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_1") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_1") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_1") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_2") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_2") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_2") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_3") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_3") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_3") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_4") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_4") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_4") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_5") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_5") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_5") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_6") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_6") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_6") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_7") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_7") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_7") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_8") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_8") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_8") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_9") = 0                '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_9") = 0                '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_9") = 0                '実績・配送距離合計Σ
        IO_ROW("NACHAIDISTANCE_2_10") = 0               '実績・配送距離
        IO_ROW("NACKAIDISTANCE_2_10") = 0               '実績・下車作業距離
        IO_ROW("NACTTLDISTANCE_2_10") = 0               '実績・配送距離合計Σ
        IO_ROW("NACTTLDISTANCE_G") = 0                  '実績・配送距離合計Σ（合計）

        IO_ROW("NACHAIWORKTIME") = 0                    '実績・配送作業時間
        IO_ROW("NACGESWORKTIME") = 0                    '実績・下車作業時間
        IO_ROW("NACCHOWORKTIME") = 0                    '実績・勤怠調整時間
        IO_ROW("NACTTLWORKTIME") = 0                    '実績・配送合計時間Σ
        IO_ROW("NACOUTWORKTIME") = 0                    '実績・就業外時間
        IO_ROW("NACBREAKTIME") = 0                      '実績・休憩時間
        IO_ROW("NACTTLBREAKTIME") = 0                   '実績・休憩合計時間Σ

        IO_ROW("NACUNLOADCNT_1_1") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_2") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_3") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_4") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_5") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_6") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_7") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_8") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_9") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_1_10") = 0                 '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_1") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_2") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_3") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_4") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_5") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_6") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_7") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_8") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_9") = 0                  '実績・荷卸回数
        IO_ROW("NACUNLOADCNT_2_10") = 0                 '実績・荷卸回数
        IO_ROW("NACTTLUNLOADCNT_G") = 0                 '実績・荷卸回数合計Σ（合計）

        IO_ROW("NACHAIWORKTIME") = 0                    '実績・配送作業時間
        IO_ROW("NACCHOWORKTIME") = 0                    '実績・勤怠調整時間
        IO_ROW("NACTTLWORKTIME") = 0                    '実績・配送合計時間Σ

        IO_ROW("NACOUTWORKTIME") = 0                    '実績・就業外時間
        IO_ROW("NACBREAKTIME") = 0                      '実績・休憩時間
        IO_ROW("NACCHOBREAKTIME") = 0                   '実績・休憩調整時間
        IO_ROW("NACTTLBREAKTIME") = 0                   '実績・休憩合計時間Σ
        IO_ROW("NACOFFICETIME") = 0                     '実績・事務時間
        IO_ROW("NACOFFICEBREAKTIME") = 0                '実績・事務休憩時間

        IO_ROW("PAYWORKNISSU") = 0                      '所労
        IO_ROW("PAYSHOUKETUNISSU") = 0                  '傷欠
        IO_ROW("PAYKUMIKETUNISSU") = 0                  '組欠
        IO_ROW("PAYETCKETUNISSU") = 0                   '他欠
        IO_ROW("PAYNENKYUNISSU") = 0                    '年休
        IO_ROW("PAYTOKUKYUNISSU") = 0                   '特休
        IO_ROW("PAYCHIKOKSOTAINISSU") = 0               '遅早
        IO_ROW("PAYSTOCKNISSU") = 0                     'ストック休暇
        IO_ROW("PAYKYOTEIWEEKNISSU") = 0                '協定週休
        IO_ROW("PAYWEEKNISSU") = 0                      '週休
        IO_ROW("PAYDAIKYUNISSU") = 0                    '代休
        IO_ROW("PAYWORKTIME") = 0                       '所定労働時間
        IO_ROW("PAYNIGHTTIME") = 0                      '所定深夜時間
        IO_ROW("PAYORVERTIME") = 0                      '平日残業時間
        IO_ROW("PAYWNIGHTTIME") = 0                     '平日深夜時間
        IO_ROW("PAYWSWORKTIME") = 0                     '日曜出勤時間
        IO_ROW("PAYSNIGHTTIME") = 0                     '日曜深夜時間
        IO_ROW("PAYHWORKTIME") = 0                      '休日出勤時間
        IO_ROW("PAYHNIGHTTIME") = 0                     '休日深夜時間
        IO_ROW("PAYBREAKTIME") = 0                      '休憩時間
        IO_ROW("PAYNENSHINISSU") = 0                    '年始出勤
        IO_ROW("PAYSHUKCHOKNNISSU") = 0                 '宿日直年始
        IO_ROW("PAYSHUKCHOKNISSU") = 0                  '宿日直通常
        IO_ROW("PAYSHUKCHOKNHLDNISSU") = 0              '宿日直年始（翌休み）
        IO_ROW("PAYSHUKCHOKHLDNISSU") = 0               '宿日直通常（翌休み）
        IO_ROW("PAYTOKSAAKAISU") = 0                    '特作A
        IO_ROW("PAYTOKSABKAISU") = 0                    '特作B
        IO_ROW("PAYTOKSACKAISU") = 0                    '特作C
        IO_ROW("PAYHOANTIME") = 0                       '保安検査入力
        IO_ROW("PAYKOATUTIME") = 0                      '高圧作業入力
        IO_ROW("PAYTOKUSA1TIME") = 0                    '特作Ⅰ
        IO_ROW("PAYPONPNISSU") = 0                      'ポンプ
        IO_ROW("PAYBULKNISSU") = 0                      'バルク
        IO_ROW("PAYTRAILERNISSU") = 0                   'トレーラ
        IO_ROW("PAYBKINMUKAISU") = 0                    'B勤務
        IO_ROW("WORKTIMEMIN") = 0                       '拘束時間（分）
        IO_ROW("WORKTIMEMIN16UP") = 0                   '拘束時間１６時間超（回数）

    End Sub

    ''' <summary>
    ''' セレクター設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub InitialSelector()

        Dim WW_POS As String = ""
        Dim WW_TBLview As DataView
        Dim WW_GRPtbl As DataTable

        'テンポラリDB項目作成
        If IsNothing(SELECTORtbl) Then SELECTORtbl = New DataTable
        SELECTORtbl.Clear()
        SELECTORtbl.Columns.Add("CODE", GetType(String))                        'CODE               コード
        SELECTORtbl.Columns.Add("NAME", GetType(String))                        'NAME               名称

        '---------------------------------------------------
        '組織セレクター作成
        '---------------------------------------------------
        Dim WW_Cols As String() = {"PAYHORG", "PAYHORGNAME"}
        WW_TBLview = New DataView(TA0005tbl)
        WW_TBLview.Sort = "PAYHORG"
        '出荷部署、出荷部署名でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = GRTA0005WRKINC.ALL_SELECTOR.CODE
        SELECTORrow("NAME") = GRTA0005WRKINC.ALL_SELECTOR.NAME
        SELECTORtbl.Rows.Add(SELECTORrow)
        For Each TA0005row As DataRow In WW_GRPtbl.Rows

            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0005row("PAYHORG")
            SELECTORrow("NAME") = TA0005row("PAYHORGNAME") & "(" & TA0005row("PAYHORG") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next
        'ソート
        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "CODE, NAME"
        SELECTORtbl = CS0026TblSort.sort()
        '●セレクター設定処理
        WF_ORGselector.DataSource = SELECTORtbl
        WF_ORGselector.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WW_POS = ""
            WF_SELECTOR_PosiORG.Value = ""
        Else
            WW_POS = SELECTORtbl.Rows(0)("CODE")
            WF_SELECTOR_PosiORG.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        SetRepeater("0", WF_ORGselector, "WF_SELorg_VALUE", "WF_SELorg_TEXT", WW_POS)

        '---------------------------------------------------
        '乗務員セレクター作成
        '---------------------------------------------------
        SELECTORtbl.Clear()
        WW_GRPtbl.Clear()
        WW_Cols = {}


        WW_Cols = {"PAYSTAFFCODE", "PAYSTAFFCODENAME"}
        WW_TBLview = New DataView(TA0005tbl)
        WW_TBLview.Sort = "PAYSTAFFCODE"

        '乗務員、乗務員名称でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        SELECTORrow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = GRTA0005WRKINC.ALL_SELECTOR.CODE
        SELECTORrow("NAME") = GRTA0005WRKINC.ALL_SELECTOR.NAME
        SELECTORtbl.Rows.Add(SELECTORrow)
        For Each TA0005row As DataRow In WW_GRPtbl.Rows

            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0005row("PAYSTAFFCODE")
            SELECTORrow("NAME") = TA0005row("PAYSTAFFCODENAME") & "(" & TA0005row("PAYSTAFFCODE") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next
        'ソート
        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "CODE, NAME"
        SELECTORtbl = CS0026TblSort.sort()

        '●セレクター設定処理
        WF_STAFFselector.DataSource = SELECTORtbl
        WF_STAFFselector.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WW_POS = ""
            WF_SELECTOR_PosiSTAFF.Value = ""
        Else
            WW_POS = SELECTORtbl.Rows(0)("CODE")
            WF_SELECTOR_PosiSTAFF.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        SetRepeater("1", WF_STAFFselector, "WF_SELstaff_VALUE", "WF_SELstaff_TEXT", WW_POS)

    End Sub
    ''' <summary>
    ''' リピータ設定処理
    ''' </summary>
    ''' <param name="I_KBN">区分値</param>
    ''' <param name="IO_SELECTOR_OBJ">セレクター</param>
    ''' <param name="I_VALUE_OBJ">コード</param>
    ''' <param name="I_TEXT_OBJ">名称</param>
    ''' <param name="I_POS">位置</param>
    ''' <remarks></remarks>
    Protected Sub SetRepeater(ByVal I_KBN As String, ByRef IO_SELECTOR_OBJ As Object, ByVal I_VALUE_OBJ As String, ByVal I_TEXT_OBJ As String, ByVal I_POS As String)

        For i As Integer = 0 To IO_SELECTOR_OBJ.Items.Count - 1
            '値　
            CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), Label).Text = SELECTORtbl.Rows(i)("CODE")
            'テキスト
            CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Text = "　" & SELECTORtbl.Rows(i)("NAME")

            '背景色
            If CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), Label).Text = I_POS Then
                CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If

            'イベント追加
            CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Attributes.Remove("onclick")
            CType(IO_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Attributes.Add("onclick", "SELECTOR_Click('" & I_KBN & "','" & SELECTORtbl.Rows(i)("CODE") & "');")
        Next

    End Sub

    ''' <summary>
    ''' セレクタークリック(選択変更)処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SELECTOR_Click()

        Dim WW_RADIO As Integer = WF_SelectorMView.ActiveViewIndex
        '■ セレクター表示切替
        '組織
        If WW_RADIO = 0 Then
            For i As Integer = 0 To WF_ORGselector.Items.Count - 1
                '背景色
                If CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_VALUE"), Label).Text = WF_SELECTOR_PosiORG.Value Then
                    CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;"
                Else
                    CType(WF_ORGselector.Items(i).FindControl("WF_SELorg_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
                End If
            Next

        End If

        '乗務員
        If WW_RADIO = 1 Then
            For i As Integer = 0 To WF_STAFFselector.Items.Count - 1
                '背景色
                If CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_VALUE"), Label).Text = WF_SELECTOR_PosiSTAFF.Value Then
                    CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;"
                Else
                    CType(WF_STAFFselector.Items(i).FindControl("WF_SELstaff_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
                End If
            Next

        End If

    End Sub

    ''' <summary>
    ''' TA0005tbl項目設定
    ''' </summary>
    ''' <param name="IO_TBL">TA0005tbl</param>
    ''' <remarks></remarks>
    Protected Sub AddColumnToTA0005tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        '○DB項目クリア
        If IO_TBL.Columns.Count = 0 Then
        Else
            IO_TBL.Columns.Clear()
        End If

        '○共通項目
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))                   'DBの固定フィールド
        IO_TBL.Columns.Add("OPERATION", GetType(String))                  'DBの固定フィールド
        IO_TBL.Columns.Add("TIMSTP", GetType(String))                     'DBの固定フィールド
        IO_TBL.Columns.Add("SELECT", GetType(Integer))                    'DBの固定フィールド
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))                    'DBの固定フィールド

        '○画面固有項目
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))                   '会社
        IO_TBL.Columns.Add("CAMPNAME", GetType(String))                   '会社名称
        IO_TBL.Columns.Add("KEIJOYMD", GetType(String))                   '計上日付
        IO_TBL.Columns.Add("DENYMD", GetType(String))                     '伝票日付
        IO_TBL.Columns.Add("DENNO", GetType(String))                      '伝票番号
        IO_TBL.Columns.Add("KANRENDENNO", GetType(String))                '関連伝票No＋明細No
        IO_TBL.Columns.Add("DTLNO", GetType(String))                      '明細番号
        IO_TBL.Columns.Add("ACACHANTEI", GetType(String))                 '仕訳決定
        IO_TBL.Columns.Add("ACACHANTEINAME", GetType(String))             '仕訳決定名称

        IO_TBL.Columns.Add("NACSHUKODATE", GetType(String))               '出庫日・作業日

        IO_TBL.Columns.Add("NACHAIDISTANCE_1_1", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_1", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_1", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_1", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_2", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_2", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_2", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_2", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_3", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_3", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_3", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_3", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_4", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_4", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_4", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_4", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_5", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_5", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_5", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_5", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_6", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_6", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_6", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_6", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_7", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_7", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_7", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_7", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_8", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_8", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_8", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_8", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_9", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_9", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_9", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_9", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_1_10", GetType(String))            '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_1_10", GetType(String))            '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_1_10", GetType(String))            '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_1_10", GetType(String))            '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_1", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_1", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_1", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_1", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_2", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_2", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_2", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_2", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_3", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_3", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_3", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_3", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_4", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_4", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_4", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_4", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_5", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_5", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_5", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_5", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_6", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_6", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_6", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_6", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_7", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_7", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_7", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_7", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_8", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_8", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_8", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_8", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_9", GetType(String))             '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_9", GetType(String))             '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_9", GetType(String))             '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_9", GetType(String))             '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACHAIDISTANCE_2_10", GetType(String))            '実績・配送距離
        IO_TBL.Columns.Add("NACKAIDISTANCE_2_10", GetType(String))            '実績・下車作業距離
        IO_TBL.Columns.Add("NACCHODISTANCE_2_10", GetType(String))            '実績・勤怠調整距離
        IO_TBL.Columns.Add("NACTTLDISTANCE_2_10", GetType(String))            '実績・配送距離合計Σ
        IO_TBL.Columns.Add("NACTTLDISTANCE_G", GetType(String))               '実績・配送距離合計Σ

        IO_TBL.Columns.Add("NACHAISTDATE", GetType(String))               '実績・配送作業開始日時
        IO_TBL.Columns.Add("NACHAIENDDATE", GetType(String))              '実績・配送作業終了日時
        IO_TBL.Columns.Add("NACHAIWORKTIME", GetType(String))             '実績・配送作業時間
        IO_TBL.Columns.Add("NACGESSTDATE", GetType(String))               '実績・下車作業開始日時
        IO_TBL.Columns.Add("NACGESENDDATE", GetType(String))              '実績・下車作業終了日時
        IO_TBL.Columns.Add("NACGESWORKTIME", GetType(String))             '実績・下車作業時間
        IO_TBL.Columns.Add("NACCHOWORKTIME", GetType(String))             '実績・勤怠調整時間
        IO_TBL.Columns.Add("NACTTLWORKTIME", GetType(String))             '実績・配送合計時間Σ

        IO_TBL.Columns.Add("NACOUTWORKTIME", GetType(String))             '実績・就業外時間

        IO_TBL.Columns.Add("NACBREAKSTDATE", GetType(String))             '実績・休憩開始日時
        IO_TBL.Columns.Add("NACBREAKENDDATE", GetType(String))            '実績・休憩終了日時
        IO_TBL.Columns.Add("NACBREAKTIME", GetType(String))               '実績・休憩時間
        IO_TBL.Columns.Add("NACCHOBREAKTIME", GetType(String))            '実績・休憩調整時間
        IO_TBL.Columns.Add("NACTTLBREAKTIME", GetType(String))            '実績・休憩合計時間Σ

        IO_TBL.Columns.Add("NACUNLOADCNT_1_1", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_1", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_2", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_2", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_3", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_3", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_4", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_4", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_5", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_5", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_6", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_6", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_7", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_7", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_8", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_8", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_9", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_9", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_1_10", GetType(String))              '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_1_10", GetType(String))           '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_1", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_1", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_2", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_2", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_3", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_3", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_4", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_4", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_5", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_5", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_6", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_6", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_7", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_7", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_8", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_8", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_9", GetType(String))               '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_9", GetType(String))            '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACUNLOADCNT_2_10", GetType(String))              '実績・荷卸回数
        IO_TBL.Columns.Add("NACCHOUNLOADCNT_2_10", GetType(String))           '実績・荷卸回数調整
        IO_TBL.Columns.Add("NACTTLUNLOADCNT_G", GetType(String))              '実績・荷卸回数合計Σ（合計）

        IO_TBL.Columns.Add("NACOFFICESORG", GetType(String))              '実績・従業作業部署
        IO_TBL.Columns.Add("NACOFFICESORGNAME", GetType(String))          '実績・従業作業部署名称
        IO_TBL.Columns.Add("NACOFFICETIME", GetType(String))              '実績・従業時間
        IO_TBL.Columns.Add("NACOFFICEBREAKTIME", GetType(String))         '実績・従業休憩時間
        IO_TBL.Columns.Add("PAYSHUSHADATE", GetType(String))              '出社日時
        IO_TBL.Columns.Add("PAYTAISHADATE", GetType(String))              '退社日時
        IO_TBL.Columns.Add("PAYSTAFFKBN", GetType(String))                '社員区分
        IO_TBL.Columns.Add("PAYSTAFFKBNNAME", GetType(String))            '社員区分名称
        IO_TBL.Columns.Add("PAYSTAFFCODE", GetType(String))               '従業員
        IO_TBL.Columns.Add("PAYSTAFFCODENAME", GetType(String))           '従業員名称
        IO_TBL.Columns.Add("PAYMORG", GetType(String))                    '従業員管理部署
        IO_TBL.Columns.Add("PAYMORGNAME", GetType(String))                '従業員管理部署名称
        IO_TBL.Columns.Add("PAYHORG", GetType(String))                    '従業員配属部署
        IO_TBL.Columns.Add("PAYHORGNAME", GetType(String))                '従業員配属部署名称
        IO_TBL.Columns.Add("PAYHOLIDAYKBN", GetType(String))              '休日区分
        IO_TBL.Columns.Add("PAYHOLIDAYKBNNAME", GetType(String))          '休日区分名称
        IO_TBL.Columns.Add("PAYKBN", GetType(String))                     '勤怠区分
        IO_TBL.Columns.Add("PAYKBNNAME", GetType(String))                 '勤怠区分名称
        IO_TBL.Columns.Add("PAYSHUKCHOKKBN", GetType(String))             '宿日直区分
        IO_TBL.Columns.Add("PAYSHUKCHOKKBNNAME", GetType(String))         '宿日直区分名称
        IO_TBL.Columns.Add("PAYJYOMUKBN", GetType(String))                '乗務区分
        IO_TBL.Columns.Add("PAYJYOMUKBNNAME", GetType(String))            '乗務区分名称

        IO_TBL.Columns.Add("PAYOILKBN_1_1", GetType(String))                '勤怠用油種区分1
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_1", GetType(String))            '勤怠用油種区分名称1
        IO_TBL.Columns.Add("PAYOILKBN_1_2", GetType(String))                '勤怠用油種区分2
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_2", GetType(String))            '勤怠用油種区分名称2
        IO_TBL.Columns.Add("PAYOILKBN_1_3", GetType(String))                '勤怠用油種区分3
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_3", GetType(String))            '勤怠用油種区分名称3
        IO_TBL.Columns.Add("PAYOILKBN_1_4", GetType(String))                '勤怠用油種区分4
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_4", GetType(String))            '勤怠用油種区分名称4
        IO_TBL.Columns.Add("PAYOILKBN_1_5", GetType(String))                '勤怠用油種区分5
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_5", GetType(String))            '勤怠用油種区分名称5
        IO_TBL.Columns.Add("PAYOILKBN_1_6", GetType(String))                '勤怠用油種区分6
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_6", GetType(String))            '勤怠用油種区分名称6
        IO_TBL.Columns.Add("PAYOILKBN_1_7", GetType(String))                '勤怠用油種区分7
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_7", GetType(String))            '勤怠用油種区分名称7
        IO_TBL.Columns.Add("PAYOILKBN_1_8", GetType(String))                '勤怠用油種区分8
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_8", GetType(String))            '勤怠用油種区分名称8
        IO_TBL.Columns.Add("PAYOILKBN_1_9", GetType(String))                '勤怠用油種区分9
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_9", GetType(String))            '勤怠用油種区分名称9
        IO_TBL.Columns.Add("PAYOILKBN_1_10", GetType(String))               '勤怠用油種区分10
        IO_TBL.Columns.Add("PAYOILKBNNAME_1_10", GetType(String))           '勤怠用油種区分名称10

        IO_TBL.Columns.Add("PAYOILKBN_2_1", GetType(String))                '勤怠用油種区分1
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_1", GetType(String))            '勤怠用油種区分名称1
        IO_TBL.Columns.Add("PAYOILKBN_2_2", GetType(String))                '勤怠用油種区分2
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_2", GetType(String))            '勤怠用油種区分名称2
        IO_TBL.Columns.Add("PAYOILKBN_2_3", GetType(String))                '勤怠用油種区分3
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_3", GetType(String))            '勤怠用油種区分名称3
        IO_TBL.Columns.Add("PAYOILKBN_2_4", GetType(String))                '勤怠用油種区分4
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_4", GetType(String))            '勤怠用油種区分名称4
        IO_TBL.Columns.Add("PAYOILKBN_2_5", GetType(String))                '勤怠用油種区分5
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_5", GetType(String))            '勤怠用油種区分名称5
        IO_TBL.Columns.Add("PAYOILKBN_2_6", GetType(String))                '勤怠用油種区分6
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_6", GetType(String))            '勤怠用油種区分名称6
        IO_TBL.Columns.Add("PAYOILKBN_2_7", GetType(String))                '勤怠用油種区分7
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_7", GetType(String))            '勤怠用油種区分名称7
        IO_TBL.Columns.Add("PAYOILKBN_2_8", GetType(String))                '勤怠用油種区分8
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_8", GetType(String))            '勤怠用油種区分名称8
        IO_TBL.Columns.Add("PAYOILKBN_2_9", GetType(String))                '勤怠用油種区分9
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_9", GetType(String))            '勤怠用油種区分名称9
        IO_TBL.Columns.Add("PAYOILKBN_2_10", GetType(String))               '勤怠用油種区分10
        IO_TBL.Columns.Add("PAYOILKBNNAME_2_10", GetType(String))           '勤怠用油種区分名称10

        IO_TBL.Columns.Add("PAYSHARYOKBN_1", GetType(String))               '勤怠用車両区分1
        IO_TBL.Columns.Add("PAYSHARYOKBNNAME_1", GetType(String))           '勤怠用車両区分名称1
        IO_TBL.Columns.Add("PAYSHARYOKBN_2", GetType(String))               '勤怠用車両区分2
        IO_TBL.Columns.Add("PAYSHARYOKBNNAME_2", GetType(String))           '勤怠用車両区分名称2

        IO_TBL.Columns.Add("PAYWORKNISSU", GetType(String))               '所労
        IO_TBL.Columns.Add("PAYSHOUKETUNISSU", GetType(String))           '傷欠
        IO_TBL.Columns.Add("PAYKUMIKETUNISSU", GetType(String))           '組欠
        IO_TBL.Columns.Add("PAYETCKETUNISSU", GetType(String))            '他欠
        IO_TBL.Columns.Add("PAYNENKYUNISSU", GetType(String))             '年休
        IO_TBL.Columns.Add("PAYTOKUKYUNISSU", GetType(String))            '特休
        IO_TBL.Columns.Add("PAYCHIKOKSOTAINISSU", GetType(String))        '遅早
        IO_TBL.Columns.Add("PAYSTOCKNISSU", GetType(String))              'ストック休暇
        IO_TBL.Columns.Add("PAYKYOTEIWEEKNISSU", GetType(String))         '協定週休
        IO_TBL.Columns.Add("PAYWEEKNISSU", GetType(String))               '週休
        IO_TBL.Columns.Add("PAYDAIKYUNISSU", GetType(String))             '代休
        IO_TBL.Columns.Add("PAYWORKTIME", GetType(String))                '所定労働時間
        IO_TBL.Columns.Add("PAYNIGHTTIME", GetType(String))               '所定深夜時間
        IO_TBL.Columns.Add("PAYORVERTIME", GetType(String))               '平日残業時間
        IO_TBL.Columns.Add("PAYWNIGHTTIME", GetType(String))              '平日深夜時間
        IO_TBL.Columns.Add("PAYWSWORKTIME", GetType(String))              '日曜出勤時間
        IO_TBL.Columns.Add("PAYSNIGHTTIME", GetType(String))              '日曜深夜時間
        IO_TBL.Columns.Add("PAYHWORKTIME", GetType(String))               '休日出勤時間
        IO_TBL.Columns.Add("PAYHNIGHTTIME", GetType(String))              '休日深夜時間
        IO_TBL.Columns.Add("PAYBREAKTIME", GetType(String))               '休憩時間
        IO_TBL.Columns.Add("PAYNENSHINISSU", GetType(String))             '年始出勤
        IO_TBL.Columns.Add("PAYSHUKCHOKNNISSU", GetType(String))          '宿日直年始
        IO_TBL.Columns.Add("PAYSHUKCHOKNISSU", GetType(String))           '宿日直通常
        IO_TBL.Columns.Add("PAYSHUKCHOKNHLDNISSU", GetType(String))       '宿日直年始（翌休み）
        IO_TBL.Columns.Add("PAYSHUKCHOKHLDNISSU", GetType(String))        '宿日直通常（翌休み）
        IO_TBL.Columns.Add("PAYTOKSAAKAISU", GetType(String))             '特作A
        IO_TBL.Columns.Add("PAYTOKSABKAISU", GetType(String))             '特作B
        IO_TBL.Columns.Add("PAYTOKSACKAISU", GetType(String))             '特作C
        IO_TBL.Columns.Add("PAYHOANTIME", GetType(String))                '保安検査入力
        IO_TBL.Columns.Add("PAYKOATUTIME", GetType(String))               '高圧作業入力
        IO_TBL.Columns.Add("PAYTOKUSA1TIME", GetType(String))             '特作Ⅰ
        IO_TBL.Columns.Add("PAYPONPNISSU", GetType(String))               'ポンプ
        IO_TBL.Columns.Add("PAYBULKNISSU", GetType(String))               'バルク
        IO_TBL.Columns.Add("PAYTRAILERNISSU", GetType(String))            'トレーラ
        IO_TBL.Columns.Add("PAYBKINMUKAISU", GetType(String))             'B勤務
        IO_TBL.Columns.Add("PAYAPPLYID", GetType(String))                 '申請ID
        IO_TBL.Columns.Add("PAYRIYU", GetType(String))                    '理由
        IO_TBL.Columns.Add("PAYRIYUNAME", GetType(String))               '理由名称
        IO_TBL.Columns.Add("PAYRIYUETC", GetType(String))                 '理由その他

        IO_TBL.Columns.Add("WORKKBN", GetType(String))                    'SYS作業区分
        IO_TBL.Columns.Add("WORKKBNNAME", GetType(String))                'SYS作業区分名称
        IO_TBL.Columns.Add("KEYSTAFFCODE", GetType(String))               'SYS従業員
        IO_TBL.Columns.Add("KEYGSHABAN", GetType(String))                 'SYS業務車番
        IO_TBL.Columns.Add("KEYTRIPNO", GetType(String))                  'SYSトリップ
        IO_TBL.Columns.Add("KEYDROPNO", GetType(String))                  'SYSドロップ
        IO_TBL.Columns.Add("KEYTSHABAN1", GetType(String))                'SYS統一車番1
        IO_TBL.Columns.Add("KEYTSHABAN2", GetType(String))                'SYS統一車番2
        IO_TBL.Columns.Add("KEYTSHABAN3", GetType(String))                'SYS統一車番3
        IO_TBL.Columns.Add("RECODEKBN", GetType(String))                  'SYSレコード区分（日、月調整）
        IO_TBL.Columns.Add("RECODEKBNNAME", GetType(String))              'SYSレコード区分名称（日、月調整）
        IO_TBL.Columns.Add("WORKTIME", GetType(String))                   'SYS拘束時間
        IO_TBL.Columns.Add("WORKTIMEMIN", GetType(String))                'SYS拘束時間（分）
        IO_TBL.Columns.Add("WORKTIMEMIN16UP", GetType(String))            'SYS拘束時間１６時間超（回数）
        IO_TBL.Columns.Add("TAISHYM", GetType(String))                    'SYS対象年月
    End Sub

    ''' <summary>
    ''' 時刻変換（分　⇒　時分）
    ''' </summary>
    ''' <param name="I_PARAM">変換前時間（分）</param>
    ''' <returns>変換後時間（時：分）</returns>
    Function MinutesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        Dim WW_ABS As Integer = System.Math.Abs(I_PARAM)

        WW_HHMM = Int(WW_ABS / 60) * 100 + WW_ABS Mod 60
        If I_PARAM < 0 Then
            WW_HHMM = WW_HHMM * -1
        End If
        Return Format(WW_HHMM, "0#:##")
    End Function

    ''' <summary>
    ''' 部署コード変換
    ''' </summary>
    ''' <param name="I_ORG">変換前部署コード</param>
    ''' <param name="O_ORG">変換後部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub ConvORGCode(ByVal I_ORG As String, ByRef O_ORG As String, ByRef O_RTN As String)

        O_ORG = I_ORG
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT CODE                              ")
                SQLStr.AppendLine(" FROM   M0006_STRUCT    M06               ")
                SQLStr.AppendLine(" WHERE  M06.CAMPCODE     = @P01           ")
                SQLStr.AppendLine("   AND  M06.OBJECT       = 'ORG'          ")
                SQLStr.AppendLine("   AND  M06.STRUCT       = '勤怠管理組織' ")
                SQLStr.AppendLine("   AND  M06.GRCODE01     = @P02           ")
                SQLStr.AppendLine("   AND  M06.STYMD       <= @P04           ")
                SQLStr.AppendLine("   AND  M06.ENDYMD      >= @P03           ")
                SQLStr.AppendLine("   AND  M06.DELFLG      <> '1'            ")

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P01", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@P02", SqlDbType.NVarChar, 20).Value = I_ORG
                        .Add("@P03", SqlDbType.Date).Value = Date.Now
                        .Add("@P04", SqlDbType.Date).Value = Date.Now
                    End With

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            O_ORG = SQLdr("CODE")
                        End While

                    End Using
                End Using
            End Using


        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC001_FIXVALUE SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC001_FIXVALUE Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    '''  端末種別取得（全社サーバーか否か判定）
    ''' </summary>
    ''' <param name="I_TERMID"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetTermClass(ByVal I_TERMID As String) As String
        Dim WW_TermClass As String = ""

        '○ ユーザ
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As New StringBuilder(1000)
                SQLStr.AppendLine(" SELECT TERMCLASS                          ")
                SQLStr.AppendLine(" FROM S0001_TERM                           ")
                SQLStr.AppendLine(" WHERE TERMID        =  '" & I_TERMID & "' ")
                SQLStr.AppendLine(" AND   STYMD        <= getdate()           ")
                SQLStr.AppendLine(" AND   ENDYMD       >= getdate()           ")
                SQLStr.AppendLine(" AND   DELFLG       <> '1'                 ")

                Using SQLcmd As New SqlCommand(SQLStr.ToString, SQLcon)
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            WW_TermClass = SQLdr("TERMCLASS")
                        End While
                    End Using

                End Using

                End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0001_TERM SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return WW_TermClass
        End Try
        Return WW_TermClass

    End Function

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()
        '■■■ 選択画面の入力初期値設定 ■■■

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0005S Then                                                    '条件画面からの画面遷移

            If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0005WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
            '○Grid情報保存先のファイル名
            work.WF_SEL_XMLsaveF.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0004-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            work.WF_SEL_XMLsaveF2.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0004INQ-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        End If
    End Sub


End Class




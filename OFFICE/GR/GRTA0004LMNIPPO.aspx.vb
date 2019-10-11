Imports System.Data.SqlClient

Public Class GRTA0004LMNIPPO
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' LogOutput DirString Get
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    ''' <summary>
    ''' ユーザプロファイル（GridView）設定
    ''' </summary>
    Private CS0013PROFview As New CS0013ProfView                    'ユーザプロファイル（GridView）設定
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
    Private CS0050SESSION As New CS0050SESSION                      'セッション管理
    ''' <summary>
    ''' 勤怠関連共通
    ''' </summary>
    Private T0007COM As New GRT0007COM                              '勤怠共通

    '検索結果格納ds
    Private TA0004tbl As DataTable                                  'Grid格納用テーブル
    Private TA0004SUMtbl As DataTable                               'Grid格納用テーブル
    Private TA0004VIEWtbl As DataTable                              'Grid格納用テーブル
    Private SELECTORtbl As DataTable                                'TREE選択作成作業テーブル

    '共通処理結果
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
    Private Const CONST_DSPROWCOUNT As Integer = 40         '１画面表示対象
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

        '■ Close
        If Not IsNothing(TA0004tbl) Then
            TA0004tbl.Dispose()
            TA0004tbl = Nothing
        End If
        If Not IsNothing(TA0004VIEWtbl) Then
            TA0004VIEWtbl.Dispose()
            TA0004VIEWtbl = Nothing
        End If
        If Not IsNothing(TA0004SUMtbl) Then
            TA0004SUMtbl.Dispose()
            TA0004SUMtbl = Nothing
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
        If Not Master.SaveTable(TA0004tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub
        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0004tbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(TA0004tbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & (CONST_DSPROWCOUNT)
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = GRTA0004WRKINC.MAPID
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.HIDEOPERATIONOPT = True
            CS0013PROFview.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
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

        If IsNothing(TA0004VIEWtbl) Then
            If Not Master.RecoverTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
        End If

        Dim WW_GridPosition As Integer                 '表示位置（開始）
        Dim WW_DataCNT As Integer = 0                  '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To TA0004VIEWtbl.Rows.Count - 1
            If TA0004VIEWtbl.Rows(i)(4) = "0" Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                TA0004VIEWtbl.Rows(i)("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(TA0004VIEWtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString
        '一覧作成

        CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013PROFview.MAPID = GRTA0004WRKINC.MAPID
        CS0013PROFview.VARI = Master.VIEWID
        CS0013PROFview.SRCDATA = WW_TBLview.ToTable
        CS0013PROFview.TBLOBJ = pnlListArea
        CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Horizontal
        CS0013PROFview.TITLEOPT = True
        CS0013PROFview.HIDEOPERATIONOPT = True
        CS0013PROFview.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

    End Sub
    ''' <summary>
    ''' 照会ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINQ_Click()

        'チェックボックス選択チェック
        If WF_CBOX_SW1.Checked = False AndAlso
            WF_CBOX_SW2.Checked = False AndAlso
            WF_CBOX_SW3.Checked = False AndAlso
            WF_CBOX_SW4.Checked = False AndAlso
            WF_CBOX_SW5.Checked = False AndAlso
            WF_CBOX_SW6.Checked = False AndAlso
            WF_CBOX_SW7.Checked = False AndAlso
            WF_CBOX_SW8.Checked = False AndAlso
            WF_CBOX_SW9.Checked = False Then
            Master.output(C_MESSAGE_NO.SELECT_AGGREGATE_CONDITION, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0004tbl, work.WF_SEL_XMLsaveF.Text) Then Exit Sub

        '○TA0004VIEWtbl取得
        GetViewTA0004Tbl()

        '○ ２次サマリー
        SumTA0004WK2()

        Dim wCNT As Integer = 0
        For Each TA0004row As DataRow In TA0004VIEWtbl.Rows
            wCNT = wCNT + 1
            TA0004row("LINECNT") = wCNT
        Next

        '■■■ 画面（GridView）表示データ保存 ■■■
        If Not Master.SaveTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

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
    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0004WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0004VIEWtbl                      'データ参照DataTable
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
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonXLS_Click()

        '■ データリカバリ
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = GRTA0004WRKINC.MAPID               '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = TA0004VIEWtbl                        'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If isNormal(CS0030REPORT.ERR) Then
        Else
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
        '○ T00004ALLデータリカバリ
        If Not Master.RecoverTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub
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
        If Not Master.RecoverTable(TA0004VIEWtbl, work.WF_SEL_XMLsaveF2.Text) Then Exit Sub

        '○ソート
        Using WW_TBLview As DataView = New DataView(TA0004VIEWtbl)
            WW_TBLview.RowFilter = "HIDDEN= '0'"

            '○最終頁に移動
            If WW_TBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT)
            Else
                WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod CONST_SCROLLROWCOUNT) + 1
            End If
        End Using
    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' T00007VIEW-GridView用テーブル作成
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetViewTA0004Tbl()

        '〇 T00007ALLよりデータ抽出
        Dim WW_Sort As String = ""
        Dim WW_Filter As String = ""

        Using WW_View As DataView = New DataView(TA0004tbl)

            WW_Sort = "LINECNT"
            If Not String.IsNullOrEmpty(WF_SELECTOR_PosiORG.Value) AndAlso WF_SELECTOR_PosiORG.Value <> GRTA0004WRKINC.ALL_SELECTOR.CODE Then
                WW_Sort = WW_Sort & ",NACSHIPORG"
                WW_Filter = WW_Filter & "NACSHIPORG = '" & WF_SELECTOR_PosiORG.Value & "'"
            End If

            If Not String.IsNullOrEmpty(WF_SELECTOR_PosiSTAFF.Value) AndAlso WF_SELECTOR_PosiSTAFF.Value <> GRTA0004WRKINC.ALL_SELECTOR.CODE Then
                WW_Sort = WW_Sort & ",NACSTAFFCODE"
                If WW_Filter <> "" Then
                    WW_Filter = WW_Filter & " and "
                End If
                WW_Filter = WW_Filter & "NACSTAFFCODE = '" & WF_SELECTOR_PosiSTAFF.Value & "'"
            End If

            If Not String.IsNullOrEmpty(WF_SELECTOR_PosiGSHABAN.Value) AndAlso WF_SELECTOR_PosiGSHABAN.Value <> GRTA0004WRKINC.ALL_SELECTOR.CODE Then
                WW_Sort = WW_Sort & ",KEYGSHABAN"
                If WW_Filter <> "" Then
                    WW_Filter = WW_Filter & " and "
                End If
                WW_Filter = WW_Filter & "KEYGSHABAN = '" & WF_SELECTOR_PosiGSHABAN.Value & "'"
            End If

            WW_View.Sort = WW_Sort
            WW_View.RowFilter = WW_Filter

            TA0004VIEWtbl = WW_View.ToTable
        End Using

        '○LineCNT付番・枝番再付番
        Dim WW_LINECNT As Integer = 0
        Dim WW_SEQ As Integer = 0

        For Each TA0004VIEWrow As DataRow In TA0004VIEWtbl.Rows
            TA0004VIEWrow("LINECNT") = 0
        Next

        For Each TA0004VIEWrow As DataRow In TA0004VIEWtbl.Rows

            If TA0004VIEWrow("LINECNT") = 0 Then
                TA0004VIEWrow("SELECT") = 1
                TA0004VIEWrow("HIDDEN") = 0      '表示
                WW_LINECNT += 1
                TA0004VIEWrow("LINECNT") = WW_LINECNT
            End If

        Next

    End Sub

    ''' <summary>
    '''  表示元データ(TA0004WKtbl)取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetMapData()


        '■ 表示元データ(条件によるサマリーデータ)取得
        'カラム設定
        AddColumnToTA0004tbl(TA0004tbl)

        '勤怠締処理前の支店・営業所は明細テーブル（L0001_TOKEI）より取得
        GetTA0004tbl()

        If TA0004tbl.Rows.Count > 65000 Then
            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ERR)
            TA0004tbl.Clear()
            Exit Sub
        End If

        '勤怠締処理後の支店・営業所はサマリーテーブル（L0003_SUMMARYN）より取得）
        GetTA0004tbl2()

        If TA0004tbl.Rows.Count > 65000 Then
            'データ取得件数が65,000件を超えたため表示できません。選択条件を変更して下さい。
            Master.output(C_MESSAGE_NO.DISPLAY_RECORD_OVER, C_MESSAGE_TYPE.ERR)
            TA0004tbl.Clear()
            Exit Sub
        End If

        '■ セレクター作成
        InitialSelector()

        '■ ソート
        CS0026TblSort.TABLE = TA0004tbl
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "NACSHIPORG,NACSHUKODATE,NACSHUKADATE,NACTODOKEDATE,NACKEIJODATE,NACTORICODE,KEYGSHABAN,NACSTAFFCODE,KEYTRIPNO,KEYDROPNO,NACSEQ"
        CS0026TblSort.sort()

        Dim wCNT As Integer = 0
        For Each TA0004row As DataRow In TA0004tbl.Rows
            wCNT = wCNT + 1
            TA0004row("LINECNT") = wCNT
        Next

    End Sub
    ''' <summary>
    ''' 抽出条件の部署一覧を作成する
    ''' </summary>
    ''' <returns>部署一覧</returns>
    ''' <remarks></remarks>
    Private Function getORGList() As List(Of String)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection

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
                SQLStr.AppendLine("             and S06.PERMITCODE in ('1', '2') ")
                SQLStr.AppendLine("             and S06.STYMD         <= @P03    ")
                SQLStr.AppendLine("             and S06.ENDYMD        >= @P03    ")
                SQLStr.AppendLine("             and S06.DELFLG        <> '1'     ")
                SQLStr.AppendLine(" GROUP BY        S06.CAMPCODE , S06.CODE      ")

                Using SQLcmdQRG = New SqlCommand(SQLStr.ToString, SQLcon)
                    With SQLcmdQRG.Parameters
                        .Add("@P01", SqlDbType.NVarChar, 20).Value = Master.ROLE_ORG
                        .Add("@P02", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                        .Add("@P03", SqlDbType.Date).Value = Date.Now
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
    ''' 表示元データ(条件によるサマリー前データ)取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetTA0004tbl()

        '○初期クリア
        'TA0004WKtbl値設定
        Dim wINT As Integer
        Dim wDBL As Double
        Dim wDATE As Date
        Dim wDATETime As DateTime
        '抽出条件(サーバー部署)List毎にデータ抽出
        Dim WW_MMCNT As Integer = DateDiff("m", work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text)
        Dim WW_STYMD As String = work.WF_SEL_STYMD.Text
        Dim WW_ENDYMD As String = work.WF_SEL_ENDYMD.Text
        Dim dt As Date = CDate(work.WF_SEL_STYMD.Text)
        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As List(Of String) = getORGList()
        If IsNothing(W_ORGlst) Then Exit Sub

        '検索SQL文
        Dim SQLStr As New StringBuilder(70000)
        SQLStr.AppendLine(" SELECT                                                                                      ")
        SQLStr.AppendLine("    isnull(rtrim(L01.CAMPCODE), '')                            as CAMPCODE                   ")
        SQLStr.AppendLine("  , isnull(rtrim(M01.NAMES), '')                               as CAMPNAME                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KEIJOYMD), '" & C_DEFAULT_YMD & "')       as KEIJOYMD                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.DENYMD), '" & C_DEFAULT_YMD & "')         as DENYMD                     ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.DENNO), '')                               as DENNO                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KANRENDENNO), '')                         as KANRENDENNO                ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.DTLNO), '')                               as DTLNO                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.INQKBN), '')                              as INQKBN                     ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_07.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_07                              ")
        SQLStr.AppendLine("      where   MC1_07.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_07.CLASS      = 'INQKBN'                                                   ")
        SQLStr.AppendLine("        and   MC1_07.KEYCODE    = L01.INQKBN                                                 ")
        SQLStr.AppendLine("        and   MC1_07.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_07.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_07.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as INQKBNNAME                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.ACACHANTEI), '')                          as ACACHANTEI                 ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_09.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_09                              ")
        SQLStr.AppendLine("      where   MC1_09.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_09.CLASS      = 'ACHANTEI'                                                 ")
        SQLStr.AppendLine("        and   MC1_09.KEYCODE    = L01.ACACHANTEI                                             ")
        SQLStr.AppendLine("        and   MC1_09.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_09.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_09.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("     )                                                         as ACACHANTEINAME             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHUKODATE), '" & C_DEFAULT_YMD & "')   as NACSHUKODATE               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHUKADATE), '" & C_DEFAULT_YMD & "')   as NACSHUKADATE               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTODOKEDATE), '" & C_DEFAULT_YMD & "')  as NACTODOKEDATE              ")
        SQLStr.AppendLine("  , '" & C_DEFAULT_YMD & "'                                    as NACKEIJODATE               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORICODE), '')                         as NACTORICODE                ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC2_02.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from        MC002_TORIHIKISAKI                     MC2_02                              ")
        SQLStr.AppendLine("      where   MC2_02.CAMPCODE    = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC2_02.TORICODE    = L01.NACTORICODE                                           ")
        SQLStr.AppendLine("        and   MC2_02.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC2_02.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC2_02.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACTORICODENAME            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACURIKBN), '') as NACURIKBN                                            ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_12.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_12                              ")
        SQLStr.AppendLine("      where   MC1_12.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_12.CLASS        = 'URIKBN'                                                 ")
        SQLStr.AppendLine("        and   MC1_12.KEYCODE      = L01.NACURIKBN                                            ")
        SQLStr.AppendLine("        and   MC1_12.STYMD       <= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC1_12.ENDYMD      >= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC1_12.DELFLG      <> '1'                                                      ")
        SQLStr.AppendLine("    )                                                          as NACURIKBNNAME              ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTODOKECODE), '') as NACTODOKECODE                                    ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select distinct(isnull(rtrim(MC6_01.NAMES), ''))                                       ")
        SQLStr.AppendLine("      from        MC006_TODOKESAKI                       MC6_01                              ")
        SQLStr.AppendLine("      where   MC6_01.CAMPCODE     = L01.CAMPCODE                                             ")
        SQLStr.AppendLine("        and   MC6_01.TODOKECODE   = L01.NACTODOKECODE                                        ")
        SQLStr.AppendLine("        and   MC6_01.STYMD       <= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC6_01.ENDYMD      >= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC6_01.DELFLG      <> '1'                                                      ")
        SQLStr.AppendLine("    )                                                          as NACTODOKECODENAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTORICODE), '')                        as NACSTORICODE               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC2_03.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from        MC002_TORIHIKISAKI                     MC2_03                              ")
        SQLStr.AppendLine("      where   MC2_03.CAMPCODE     = L01.CAMPCODE                                             ")
        SQLStr.AppendLine("        and   MC2_03.TORICODE     = L01.NACSTORICODE                                         ")
        SQLStr.AppendLine("        and   MC2_03.STYMD       <= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC2_03.ENDYMD      >= L01.NACSHUKODATE                                         ")
        SQLStr.AppendLine("        and   MC2_03.DELFLG      <> '1'                                                      ")
        SQLStr.AppendLine("     )                                                         as NACSTORICODENAME           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHUKABASHO), '')                       as NACSHUKABASHO              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select distinct(isnull(rtrim(MC6_02.NAMES), ''))                                       ")
        SQLStr.AppendLine("      from        MC006_TODOKESAKI                       MC6_02                              ")
        SQLStr.AppendLine("      where   MC6_02.CAMPCODE    = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC6_02.TODOKECODE  = L01.NACSHUKABASHO                                         ")
        SQLStr.AppendLine("        and   MC6_02.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC6_02.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC6_02.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACSHUKABASHONAME          ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_13.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_13                              ")
        SQLStr.AppendLine("      where   MC1_13.CAMPCODE    = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC1_13.CLASS       = 'TORITYPE01'                                              ")
        SQLStr.AppendLine("        and   MC1_13.KEYCODE     = L01.NACTORITYPE01                                         ")
        SQLStr.AppendLine("        and   MC1_13.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_13.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_13.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACTORITYPE01NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORITYPE01), '')                       as NACTORITYPE01              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_14.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_14                              ")
        SQLStr.AppendLine("      where   MC1_14.CAMPCODE    = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC1_14.CLASS       = 'TORITYPE02'                                              ")
        SQLStr.AppendLine("        and   MC1_14.KEYCODE     = L01.NACTORITYPE02                                         ")
        SQLStr.AppendLine("        and   MC1_14.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_14.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_14.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACTORITYPE02NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORITYPE02), '')                       as NACTORITYPE02              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_15.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_15                              ")
        SQLStr.AppendLine("      where   MC1_15.CAMPCODE    =  L01.CAMPCODE                                             ")
        SQLStr.AppendLine("        and   MC1_15.CLASS       = 'TORITYPE02'                                              ")
        SQLStr.AppendLine("        and   MC1_15.KEYCODE     = L01.NACTORITYPE03                                         ")
        SQLStr.AppendLine("        and   MC1_15.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_15.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_15.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACTORITYPE03NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORITYPE03), '')                       as NACTORITYPE03              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_16.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from        MC001_FIXVALUE                         MC1_16                              ")
        SQLStr.AppendLine("      where   MC1_16.CAMPCODE    = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC1_16.CLASS       = 'TORITYPE02'                                              ")
        SQLStr.AppendLine("        and   MC1_16.KEYCODE     = L01.NACTORITYPE04                                         ")
        SQLStr.AppendLine("        and   MC1_16.STYMD      <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_16.ENDYMD     >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("        and   MC1_16.DELFLG     <> '1'                                                       ")
        SQLStr.AppendLine("    )                                                          as NACTORITYPE04NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORITYPE04), '')                       as NACTORITYPE04              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_17.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from      MC001_FIXVALUE                           MC1_17                              ")
        SQLStr.AppendLine("      where   MC1_17.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_17.CLASS      = 'TORITYPE02'                                               ")
        SQLStr.AppendLine("        and   MC1_17.KEYCODE    = L01.NACTORITYPE05                                          ")
        SQLStr.AppendLine("        and   MC1_17.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_17.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_17.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACTORITYPE05NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTORITYPE05), '')                       as NACTORITYPE05              ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACOILTYPE), '')                          as NACOILTYPE_1               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_01.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                          MC1_01                              ")
        SQLStr.AppendLine("      where   MC1_01.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_01.CLASS      = 'OILTYPE'                                                  ")
        SQLStr.AppendLine("        and   MC1_01.KEYCODE    = L01.NACOILTYPE                                             ")
        SQLStr.AppendLine("        and   MC1_01.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_01.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_01.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("     )                                                         as NACOILTYPENAME_1           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACPRODUCT1), '')                         as NACPRODUCT1_1              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_02.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                          MC1_02                              ")
        SQLStr.AppendLine("      where   MC1_02.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_02.CLASS      = 'PRODUCT1'                                                 ")
        SQLStr.AppendLine("        and   MC1_02.KEYCODE    = L01.NACPRODUCT1                                            ")
        SQLStr.AppendLine("        and   MC1_02.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_02.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_02.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACPRODUCT1NAME_1          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACPRODUCT2), '')                         as NACPRODUCT2_1              ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACPRODUCTCODE), '')                      as NACPRODUCTCODE_1           ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MD1.NAMES), '')                                                    ")
        SQLStr.AppendLine("      from       MD001_PRODUCT                           MD1                                 ")
        SQLStr.AppendLine("      where   MD1.CAMPCODE      = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MD1.PRODUCTCODE   = L01.NACPRODUCTCODE                                         ")
        SQLStr.AppendLine("        and   MD1.STYMD        <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MD1.ENDYMD       >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MD1.DELFLG       <> '1' )                        as NACPRODUCT2NAME_1          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACGSHABAN), '')                          as NACGSHABAN                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSUPPLIERKBN), '')                      as NACSUPPLIERKBN             ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_18.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                          MC1_18                              ")
        SQLStr.AppendLine("      where   MC1_18.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_18.CLASS      = 'SUPPLIERKBN'                                              ")
        SQLStr.AppendLine("        and   MC1_18.KEYCODE    = L01.NACSUPPLIERKBN                                         ")
        SQLStr.AppendLine("        and   MC1_18.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_18.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_18.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACSUPPLIERKBNNAME         ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSUPPLIER), '')                         as NACSUPPLIER                ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC2_04.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       MC002_TORIHIKISAKI                      MC2_04                              ")
        SQLStr.AppendLine("      where   MC2_04.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC2_04.TORICODE   = L01.NACSUPPLIER                                            ")
        SQLStr.AppendLine("        and   MC2_04.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC2_04.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC2_04.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("     )                                                         as NACSUPPLIERNAME            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHARYOOILTYPE), '')                    as NACSHARYOOILTYPE           ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_19.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                          MC1_19                              ")
        SQLStr.AppendLine("      where   MC1_19.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_19.CLASS      = 'OILTYPE'                                                  ")
        SQLStr.AppendLine("        and   MC1_19.KEYCODE    = L01.NACSHARYOOILTYPE                                       ")
        SQLStr.AppendLine("        and   MC1_19.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_19.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_19.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACSHARYOOILTYPENAME       ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHARYOTYPE1), '')                      as NACSHARYOTYPE1             ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_20.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                          MC1_20                              ")
        SQLStr.AppendLine("      where   MC1_20.CAMPCODE   =  L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC1_20.CLASS      = 'SHARYOTYPE'                                               ")
        SQLStr.AppendLine("        and   MC1_20.KEYCODE    = L01.NACSHARYOTYPE1                                         ")
        SQLStr.AppendLine("        and   MC1_20.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_20.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_20.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACSHARYOTYPE1NAME         ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTSHABAN1), '')                         as NACTSHABAN1                ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGMORG1), '')                        as NACMANGMORG1               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_03.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                               M02_03                              ")
        SQLStr.AppendLine("      where   M02_03.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_03.ORGCODE    = L01.NACMANGMORG1                                           ")
        SQLStr.AppendLine("        and   M02_03.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_03.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_03.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACMANGMORG1NAME           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGSORG1), '')                        as NACMANGSORG1               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_04.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                                M02_04                             ")
        SQLStr.AppendLine("      where   M02_04.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_04.ORGCODE    = L01.NACMANGSORG1                                           ")
        SQLStr.AppendLine("        and   M02_04.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_04.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_04.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACMANGSORG1NAME           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGUORG1), '')                        as NACMANGUORG1               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_05.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                                M02_05                             ")
        SQLStr.AppendLine("      where   M02_05.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_05.ORGCODE    = L01.NACMANGUORG1                                           ")
        SQLStr.AppendLine("        and   M02_05.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_05.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_05.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACMANGUORG1NAME           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBASELEASE1), '')                       as NACBASELEASE1              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_23.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                           MC1_23                             ")
        SQLStr.AppendLine("      where   MC1_23.CAMPCODE   =  L01.CAMPCODE                                              ")
        SQLStr.AppendLine("        and   MC1_23.CLASS      = 'BASELEASE'                                                ")
        SQLStr.AppendLine("        and   MC1_23.KEYCODE    = L01.NACBASELEASE1                                          ")
        SQLStr.AppendLine("        and   MC1_23.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_23.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_23.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACBASELEASE1NAME          ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select distinct(isnull(rtrim(MA4_01.LICNPLTNO1), '')                                   ")
        SQLStr.AppendLine("                    + isnull(rtrim(MA4_01.LICNPLTNO2), ''))                                  ")
        SQLStr.AppendLine("      from       MA004_SHARYOC                            MA4_01                             ")
        SQLStr.AppendLine("      where   MA4_01.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MA4_01.SHARYOTYPE = L01.NACSHARYOTYPE1                                         ")
        SQLStr.AppendLine("        and   MA4_01.TSHABAN    = L01.NACTSHABAN1                                            ")
        SQLStr.AppendLine("        and   MA4_01.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MA4_01.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MA4_01.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                          as NACLICNPLTNOF1             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHARYOTYPE2), '')                      as NACSHARYOTYPE2             ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_21.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                           MC1_21                             ")
        SQLStr.AppendLine("      where   MC1_21.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   MC1_21.CLASS      = 'SHARYOTYPE'                                               ")
        SQLStr.AppendLine("        and   MC1_21.KEYCODE    = L01.NACSHARYOTYPE2                                         ")
        SQLStr.AppendLine("        and   MC1_21.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_21.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   MC1_21.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                           as NACSHARYOTYPE2NAME        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTSHABAN2), '')                          as NACTSHABAN2               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGMORG2), '')                         as NACMANGMORG2              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_06.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                                M02_06                             ")
        SQLStr.AppendLine("      where   M02_06.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_06.ORGCODE    = L01.NACMANGMORG2                                           ")
        SQLStr.AppendLine("        and   M02_06.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_06.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_06.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                           as NACMANGMORG2NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGSORG2), '')                         as NACMANGSORG2              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_07.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                                M02_07                             ")
        SQLStr.AppendLine("      where   M02_07.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_07.ORGCODE    = L01.NACMANGSORG2                                           ")
        SQLStr.AppendLine("        and   M02_07.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_07.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_07.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                           as NACMANGSORG2NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGUORG2), '')                         as NACMANGUORG2              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_08.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from       M0002_ORG                                M02_08                             ")
        SQLStr.AppendLine("      where   M02_08.CAMPCODE   = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and   M02_08.ORGCODE    = L01.NACMANGUORG2                                           ")
        SQLStr.AppendLine("        and   M02_08.STYMD     <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_08.ENDYMD    >= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and   M02_08.DELFLG    <> '1'                                                        ")
        SQLStr.AppendLine("    )                                                           as NACMANGUORG2NAME          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBASELEASE2), '')                        as NACBASELEASE2             ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_24.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from       MC001_FIXVALUE                            MC1_24                            ")
        SQLStr.AppendLine("      where MC1_24.CAMPCODE     = L01.CAMPCODE                                               ")
        SQLStr.AppendLine("        and MC1_24.CLASS        = 'BASELEASE'                                                ")
        SQLStr.AppendLine("        and MC1_24.KEYCODE      = L01.NACBASELEASE2                                          ")
        SQLStr.AppendLine("        and MC1_24.STYMD       <= L01.NACSHUKODATE                                           ")
        SQLStr.AppendLine("        and MC1_24.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MC1_24.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    )                                                           as NACBASELEASE2NAME         ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select distinct(isnull(rtrim(MA4_02.LICNPLTNO1), '')                                   ")
        SQLStr.AppendLine("                    + isnull(rtrim(MA4_02.LICNPLTNO2), ''))                                  ")
        SQLStr.AppendLine("      from       MA004_SHARYOC                             MA4_02                            ")
        SQLStr.AppendLine("      where MA4_02.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MA4_02.SHARYOTYPE = L01.NACSHARYOTYPE2                                           ")
        SQLStr.AppendLine("        and MA4_02.TSHABAN = L01.NACTSHABAN2                                                 ")
        SQLStr.AppendLine("        and MA4_02.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MA4_02.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MA4_02.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACLICNPLTNOF2                                                                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSHARYOTYPE3), '')                        as NACSHARYOTYPE3           ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_22.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from MC001_FIXVALUE MC1_22                                                             ")
        SQLStr.AppendLine("      where MC1_22.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MC1_22.CLASS = 'SHARYOTYPE'                                                      ")
        SQLStr.AppendLine("        and MC1_22.KEYCODE = L01.NACSHARYOTYPE3                                              ")
        SQLStr.AppendLine("        and MC1_22.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_22.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MC1_22.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSHARYOTYPE3NAME                                                                  ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTSHABAN3), '') as NACTSHABAN3                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGMORG3), '') as NACMANGMORG3                                      ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_09.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_09                                                                  ")
        SQLStr.AppendLine("      where M02_09.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_09.ORGCODE = L01.NACMANGMORG3                                                ")
        SQLStr.AppendLine("        and M02_09.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_09.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_09.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("     ) as NACMANGMORG3NAME                                                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGSORG3), '') as NACMANGSORG3                                      ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_10.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_10                                                                  ")
        SQLStr.AppendLine("      where M02_10.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_10.ORGCODE = L01.NACMANGSORG3                                                ")
        SQLStr.AppendLine("        and M02_10.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_10.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_10.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACMANGSORG3NAME                                                                    ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMANGUORG3), '') as NACMANGUORG3                                      ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_11.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_11                                                                  ")
        SQLStr.AppendLine("      where M02_11.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_11.ORGCODE = L01.NACMANGUORG3                                                ")
        SQLStr.AppendLine("        and M02_11.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_11.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_11.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACMANGUORG3NAME                                                                    ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBASELEASE3), '') as NACBASELEASE3                                    ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_25.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from MC001_FIXVALUE MC1_25                                                             ")
        SQLStr.AppendLine("      where MC1_25.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MC1_25.CLASS = 'BASELEASE'                                                       ")
        SQLStr.AppendLine("        and MC1_25.KEYCODE = L01.NACBASELEASE3                                               ")
        SQLStr.AppendLine("        and MC1_25.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_25.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MC1_25.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACBASELEASE3NAME                                                                   ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select distinct(isnull(rtrim(MA4_03.LICNPLTNO1), '')                                   ")
        SQLStr.AppendLine("                    + isnull(rtrim(MA4_03.LICNPLTNO2), ''))                                  ")
        SQLStr.AppendLine("      from  MA004_SHARYOC MA4_03                                                             ")
        SQLStr.AppendLine("      where MA4_03.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MA4_03.SHARYOTYPE = L01.NACSHARYOTYPE3                                           ")
        SQLStr.AppendLine("        and MA4_03.TSHABAN = L01.NACTSHABAN3                                                 ")
        SQLStr.AppendLine("        and MA4_03.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MA4_03.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MA4_03.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACLICNPLTNOF3                                                                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACCREWKBN), '') as NACCREWKBN                                          ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_26.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from MC001_FIXVALUE MC1_26                                                             ")
        SQLStr.AppendLine("      where MC1_26.CAMPCODE =  L01.CAMPCODE                                                  ")
        SQLStr.AppendLine("        and MC1_26.CLASS = 'CREWKBN'                                                         ")
        SQLStr.AppendLine("        and MC1_26.KEYCODE = L01.NACCREWKBN                                                  ")
        SQLStr.AppendLine("        and MC1_26.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_26.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MC1_26.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACCREWKBNNAME                                                                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTAFFCODE), '') as NACSTAFFCODE                                      ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MB1_02.STAFFNAMES), '')                                            ")
        SQLStr.AppendLine("      from MB001_STAFF MB1_02                                                                ")
        SQLStr.AppendLine("      where MB1_02.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MB1_02.STAFFCODE = L01.NACSTAFFCODE                                              ")
        SQLStr.AppendLine("        and MB1_02.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MB1_02.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MB1_02.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSTAFFCODENAME                                                                    ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTAFFKBN), '') as NACSTAFFKBN                                        ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_27.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from MC001_FIXVALUE MC1_27                                                             ")
        SQLStr.AppendLine("      where MC1_27.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MC1_27.CLASS = 'STAFFKBN'                                                        ")
        SQLStr.AppendLine("        and MC1_27.KEYCODE = L01.NACSTAFFKBN                                                 ")
        SQLStr.AppendLine("        and MC1_27.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_27.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MC1_27.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSTAFFKBNNAME                                                                     ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMORG), '') as NACMORG                                                ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_12.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_12                                                                  ")
        SQLStr.AppendLine("      where M02_12.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_12.ORGCODE = L01.NACMORG                                                     ")
        SQLStr.AppendLine("        and M02_12.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_12.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_12.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACMORGNAME                                                                         ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHORG), '') as NACHORG                                                ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_13.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_13                                                                  ")
        SQLStr.AppendLine("      where M02_13.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_13.ORGCODE = L01.NACHORG                                                     ")
        SQLStr.AppendLine("        and M02_13.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_13.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_13.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACHORGNAME                                                                         ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSORG), '') as NACSORG                                                ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_14.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_14                                                                  ")
        SQLStr.AppendLine("      where M02_14.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_14.ORGCODE = L01.NACSORG                                                     ")
        SQLStr.AppendLine("        and M02_14.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_14.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_14.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSORGNAME                                                                         ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTAFFCODE2), '') as NACSTAFFCODE2                                    ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MB1_03.STAFFNAMES), '')                                            ")
        SQLStr.AppendLine("      from MB001_STAFF MB1_03                                                                ")
        SQLStr.AppendLine("      where MB1_03.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MB1_03.STAFFCODE = L01.NACSTAFFCODE2                                             ")
        SQLStr.AppendLine("        and MB1_03.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MB1_03.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and MB1_03.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSTAFFCODE2NAME                                                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTAFFKBN2), '') as NACSTAFFKBN2                                      ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_28.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from MC001_FIXVALUE MC1_28                                                             ")
        SQLStr.AppendLine("      where MC1_28.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MC1_28.CLASS = 'STAFFKBN'                                                        ")
        SQLStr.AppendLine("        and MC1_28.KEYCODE = L01.NACSTAFFKBN2                                                ")
        SQLStr.AppendLine("        and MC1_28.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_28.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("       and MC1_28.DELFLG <> '1'                                                              ")
        SQLStr.AppendLine("    ) as NACSTAFFKBN2NAME                                                                    ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACMORG2), '') as NACMORG2                                              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_15.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_15                                                                  ")
        SQLStr.AppendLine("      where M02_15.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_15.ORGCODE = L01.NACMORG2                                                    ")
        SQLStr.AppendLine("        and M02_15.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_15.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_15.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACMORG2NAME                                                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHORG2), '') as NACHORG2                                              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_16.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_16                                                                  ")
        SQLStr.AppendLine("      where M02_16.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_16.ORGCODE = L01.NACHORG2                                                    ")
        SQLStr.AppendLine("        and M02_16.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_16.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_16.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACHORG2NAME                                                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSORG2), '') as NACSORG2                                              ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_17.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_17                                                                  ")
        SQLStr.AppendLine("      where M02_17.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_17.ORGCODE = L01.NACSORG2                                                    ")
        SQLStr.AppendLine("        and M02_17.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_17.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_17.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSORG2NAME                                                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACORDERNO), '') as NACORDERNO                                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACDETAILNO), '') as NACDETAILNO                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTRIPNO), '') as NACTRIPNO                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACDROPNO), '') as NACDROPNO                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSEQ), '') as NACSEQ                                                  ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACORDERORG), '') as NACORDERORG                                        ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_18.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_18                                                                  ")
        SQLStr.AppendLine("      where M02_18.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_18.ORGCODE = L01.NACORDERORG                                                 ")
        SQLStr.AppendLine("        and M02_18.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_18.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_18.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACORDERORGNAME                                                                     ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      case when rtrim(L01.NACSHIPORG) <> '' then isnull(rtrim(L01.NACSHIPORG),'')            ")
        SQLStr.AppendLine("           else isnull(rtrim(L01.ACKEIJOORG),'') end                                         ")
        SQLStr.AppendLine("     ) as NACSHIPORG                                                                         ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_19.NAMES), '')                                                 ")
        SQLStr.AppendLine("      from M0002_ORG M02_19                                                                  ")
        SQLStr.AppendLine("      where M02_19.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_19.ORGCODE = (                                                               ")
        SQLStr.AppendLine("                  case when rtrim(L01.NACSHIPORG) <> '' then isnull(rtrim(L01.NACSHIPORG),'')")
        SQLStr.AppendLine("                  else isnull(rtrim(L01.ACKEIJOORG),'') end)                                 ")
        SQLStr.AppendLine("        and M02_19.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_19.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_19.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACSHIPORGNAME                                                                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSURYO), '0') as NACSURYO1                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTANI), '') as NACTANI1                                               ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_03.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from  MC001_FIXVALUE MC1_03                                                            ")
        SQLStr.AppendLine("      where   MC1_03.CAMPCODE = L01.CAMPCODE                                                 ")
        SQLStr.AppendLine("        and   MC1_03.CLASS = 'STANI'                                                         ")
        SQLStr.AppendLine("        and   MC1_03.KEYCODE = L01.NACTANI                                                   ")
        SQLStr.AppendLine("        and   MC1_03.STYMD <= L01.NACSHUKODATE                                               ")
        SQLStr.AppendLine("        and   MC1_03.ENDYMD >= L01.NACSHUKODATE                                              ")
        SQLStr.AppendLine("        and   MC1_03.DELFLG <> '1'                                                           ")
        SQLStr.AppendLine("    ) as NACTANINAME1                                                                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACJSURYO), '0') as NACJSURYO1                                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACSTANI), '') as NACSTANI1                                             ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(MC1_04.VALUE1), '')                                                ")
        SQLStr.AppendLine("      from  MC001_FIXVALUE MC1_04                                                            ")
        SQLStr.AppendLine("      where MC1_04.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and MC1_04.CLASS = 'STANI'                                                           ")
        SQLStr.AppendLine("        and MC1_04.KEYCODE = L01.NACSTANI                                                    ")
        SQLStr.AppendLine("        and MC1_04.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and MC1_04.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("       and MC1_04.DELFLG <> '1'                                                              ")
        SQLStr.AppendLine("    ) as NACSTANINAME1                                                                       ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIDISTANCE), '0') as NACHAIDISTANCE                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKAIDISTANCE), '0') as NACKAIDISTANCE                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLDISTANCE), '0') as NACTTLDISTANCE                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAISTDATE), '1950/01/01') as NACHAISTDATE                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIENDDATE), '1950/01/01') as NACHAIENDDATE                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACHAIWORKTIME), '0') as NACHAIWORKTIME                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESSTDATE), '1950/01/01') as NACGESSTDATE                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESENDDATE), '1950/01/01') as NACGESENDDATE                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACGESWORKTIME), '0') as NACGESWORKTIME                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLWORKTIME), '0') as NACTTLWORKTIME                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACOUTWORKTIME), '0') as NACOUTWORKTIME                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKSTDATE), '1950/01/01') as NACBREAKSTDATE                        ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKENDDATE), '1950/01/01') as NACBREAKENDDATE                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACBREAKTIME), '0') as NACBREAKTIME                                     ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLBREAKTIME), '0') as NACTTLBREAKTIME                               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACCASH), '0') as NACCASH                                               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACETC), '0') as NACETC                                                 ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTICKET), '0') as NACTICKET                                           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKYUYU), '0') as NACKYUYU                                             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACUNLOADCNT), '0') as NACUNLOADCNT                                     ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACTTLUNLOADCNT), '0') as NACTTLUNLOADCNT                               ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKAIJI), '0') as NACKAIJI                                             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACJITIME), '0') as NACJITIME                                           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACJITTLETIME), '0') as NACJITTLETIME                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKUTIME), '0') as NACKUTIME                                           ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKUTTLTIME), '0') as NACKUTTLTIME                                     ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACJIDISTANCE), '0') as NACJIDISTANCE                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACJITTLDISTANCE), '0') as NACJITTLDISTANCE                             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKUDISTANCE), '0') as NACKUDISTANCE                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACKUTTLDISTANCE), '0') as NACKUTTLDISTANCE                             ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.NACOFFICESORG), '') as NACOFFICESORG                                    ")
        SQLStr.AppendLine("  , (                                                                                        ")
        SQLStr.AppendLine("      select isnull(rtrim(M02_22.NAMES), '') from M0002_ORG M02_22                           ")
        SQLStr.AppendLine("      where M02_22.CAMPCODE = L01.CAMPCODE                                                   ")
        SQLStr.AppendLine("        and M02_22.ORGCODE = L01.NACOFFICESORG                                               ")
        SQLStr.AppendLine("        and M02_22.STYMD <= L01.NACSHUKODATE                                                 ")
        SQLStr.AppendLine("        and M02_22.ENDYMD >= L01.NACSHUKODATE                                                ")
        SQLStr.AppendLine("        and M02_22.DELFLG <> '1'                                                             ")
        SQLStr.AppendLine("    ) as NACOFFICESORGNAME                                                                   ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSHUSHADATE), '1950/01/01') as PAYSHUSHADATE                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYTAISHADATE), '1950/01/01') as PAYTAISHADATE                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSTAFFKBN), '') as PAYSTAFFKBN                                        ")
        SQLStr.AppendLine("  ,(select isnull(rtrim(MC1_29.VALUE1), '') from MC001_FIXVALUE MC1_29                       ")
        SQLStr.AppendLine("    where MC1_29.CAMPCODE = L01.CAMPCODE                                                     ")
        SQLStr.AppendLine("    and MC1_29.CLASS = 'STAFFKBN'                                                            ")
        SQLStr.AppendLine("    and MC1_29.KEYCODE = L01.PAYSTAFFKBN                                                     ")
        SQLStr.AppendLine("    and MC1_29.STYMD <= L01.NACSHUKODATE                                                     ")
        SQLStr.AppendLine("    and MC1_29.ENDYMD >= L01.NACSHUKODATE                                                    ")
        SQLStr.AppendLine("    and MC1_29.DELFLG <> '1' ) as PAYSTAFFKBNNAME                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYSTAFFCODE), '') as PAYSTAFFCODE                                      ")
        SQLStr.AppendLine("  ,(select isnull(rtrim(MB1_4.STAFFNAMES), '') from MB001_STAFF MB1_4                        ")
        SQLStr.AppendLine("    where MB1_4.CAMPCODE = L01.CAMPCODE                                                      ")
        SQLStr.AppendLine("    and MB1_4.STAFFCODE = L01.PAYSTAFFCODE                                                   ")
        SQLStr.AppendLine("    and MB1_4.STYMD <= L01.NACSHUKODATE                                                      ")
        SQLStr.AppendLine("    and MB1_4.ENDYMD >= L01.NACSHUKODATE                                                     ")
        SQLStr.AppendLine("    and MB1_4.DELFLG <> '1' ) as PAYSTAFFCODENAME                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYMORG), '') as PAYMORG                                                ")
        SQLStr.AppendLine("  ,(select isnull(rtrim(M02_20.NAMES), '') from  M0002_ORG M02_20                            ")
        SQLStr.AppendLine("    where M02_20.CAMPCODE = L01.CAMPCODE                                                     ")
        SQLStr.AppendLine("    and M02_20.ORGCODE = L01.PAYMORG                                                         ")
        SQLStr.AppendLine("    and M02_20.STYMD <= L01.NACSHUKODATE                                                     ")
        SQLStr.AppendLine("    and M02_20.ENDYMD >= L01.NACSHUKODATE                                                    ")
        SQLStr.AppendLine("    and M02_20.DELFLG <> '1' ) as PAYMORGNAME                                                ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.PAYHORG), '') as PAYHORG                                                ")
        SQLStr.AppendLine("  ,(select isnull(rtrim(M02_21.NAMES), '') from M0002_ORG M02_21                             ")
        SQLStr.AppendLine("    where M02_21.CAMPCODE = L01.CAMPCODE                                                     ")
        SQLStr.AppendLine("    and M02_21.ORGCODE = L01.PAYHORG                                                         ")
        SQLStr.AppendLine("    and M02_21.STYMD <= L01.NACSHUKODATE                                                     ")
        SQLStr.AppendLine("    and M02_21.ENDYMD >= L01.NACSHUKODATE                                                    ")
        SQLStr.AppendLine("    and M02_21.DELFLG <> '1' ) as PAYHORGNAME                                                ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.WORKKBN), '') as WORKKBN                                                ")
        SQLStr.AppendLine("  ,(select isnull(rtrim(MC1_34.VALUE1), '') from MC001_FIXVALUE MC1_34                       ")
        SQLStr.AppendLine("    where MC1_34.CAMPCODE =  L01.CAMPCODE                                                    ")
        SQLStr.AppendLine("    and MC1_34.CLASS = 'WORKKBN'                                                             ")
        SQLStr.AppendLine("    and MC1_34.KEYCODE = L01.WORKKBN                                                         ")
        SQLStr.AppendLine("    and MC1_34.STYMD <= L01.NACSHUKODATE                                                     ")
        SQLStr.AppendLine("    and MC1_34.ENDYMD >= L01.NACSHUKODATE                                                    ")
        SQLStr.AppendLine("    and MC1_34.DELFLG <> '1' ) as WORKKBNNAME                                                ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KEYSTAFFCODE), '') as KEYSTAFFCODE                                      ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KEYGSHABAN), '') as KEYGSHABAN                                          ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KEYTRIPNO), '') as KEYTRIPNO                                            ")
        SQLStr.AppendLine("  , isnull(rtrim(L01.KEYDROPNO), '') as KEYDROPNO                                            ")
        SQLStr.AppendLine(" FROM       L0001_TOKEI           L01                                                        ")
        SQLStr.AppendLine(" INNER JOIN M0001_CAMP            M01                   ON                                   ")
        SQLStr.AppendLine("             M01.CAMPCODE        = L01.CAMPCODE                                              ")
        SQLStr.AppendLine("   and       M01.STYMD          <= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("   and       M01.ENDYMD         >= L01.NACSHUKODATE                                          ")
        SQLStr.AppendLine("   and       M01.DELFLG         <> '1'                                                       ")
        SQLStr.AppendLine(" WHERE                                                                                       ")
        SQLStr.AppendLine("        L01.CAMPCODE = @P02                                                                  ")
        SQLStr.AppendLine("   and  L01.INQKBN = '1'                                                                     ")
        SQLStr.AppendLine("   and  L01.ACKEIJOORG = @P14                                                                ")
        SQLStr.AppendLine("   and  L01.NACSHUKODATE <= @P05                                                             ")
        SQLStr.AppendLine("   and  L01.NACSHUKODATE >= @P06                                                             ")
        SQLStr.AppendLine("   and  L01.NACSHUKADATE <= @P07                                                             ")
        SQLStr.AppendLine("   and  L01.NACSHUKADATE >= @P08                                                             ")
        SQLStr.AppendLine("   and  L01.NACTODOKEDATE <= @P09                                                            ")
        SQLStr.AppendLine("   and  L01.NACTODOKEDATE >= @P10                                                            ")
        SQLStr.AppendLine("   and  L01.KEIJOYMD <= @P11                                                                 ")
        SQLStr.AppendLine("   and  L01.KEIJOYMD >= @P12                                                                 ")
        SQLStr.AppendLine("   and  ( L01.NACCREWKBN = '1' OR L01.NACCREWKBN = @P13 )                                    ")
        SQLStr.AppendLine("   and  L01.DELFLG <> '1'                                                                    ")
        SQLStr.AppendLine("   and  ( L01.ACACHANTEI = 'HJC' Or L01.ACACHANTEI = 'HJD' Or L01.ACACHANTEI = 'HLC' Or L01.ACACHANTEI = 'HLD' Or    ")
        SQLStr.AppendLine("          L01.ACACHANTEI = 'HSC' Or L01.ACACHANTEI = 'HSD' Or L01.ACACHANTEI = 'KJC' Or L01.ACACHANTEI = 'KJD' Or    ")
        SQLStr.AppendLine("          L01.ACACHANTEI = 'KLC' Or L01.ACACHANTEI = 'KLD' Or L01.ACACHANTEI = 'KSC' Or L01.ACACHANTEI = 'KSD' Or    ")
        SQLStr.AppendLine("          L01.ACACHANTEI = 'RSC' Or L01.ACACHANTEI = 'RSD' Or L01.ACACHANTEI = 'KEC' Or L01.ACACHANTEI = 'KED' Or    ")
        SQLStr.AppendLine("          L01.ACACHANTEI = 'TUC' Or L01.ACACHANTEI = 'TUD' Or L01.ACACHANTEI = 'URC' Or L01.ACACHANTEI = 'URD' )     ")
        SQLStr.AppendLine(" ORDER BY                                                                                                            ")
        SQLStr.AppendLine("   L01.NACSHUKODATE, L01.NACSHUKADATE, L01.NACTODOKEDATE, L01.NACTORICODE, L01.NACSHIPORG, L01.KEYGSHABAN, L01.NACCREWKBN, L01.KEYSTAFFCODE, L01.KEYTRIPNO, L01.KEYDROPNO, L01.ACACHANTEI DESC, L01.NACSEQ ")

        '***********************************************************************************************
        '売上項目を展開（労務費、車両、休憩レコードに反映）
        '　　※労務費レコードには売上項目が設定されていない為、売上項目を反映
        '　　※車両レコードには売上項目が設定されていない為、売上項目を反映
        '　　※休憩レコードには詳細項目が設定されていない為、売上項目を反映
        '***********************************************************************************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            Using SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.Date)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.Date)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)

                Dim TA0004row As DataRow = Nothing
                For Each WI_ORG As String In W_ORGlst

                    '部署変換（締めテーブル確認のため）
                    Dim WW_ORG As String = String.Empty
                    ConvORGCode(WI_ORG, WW_ORG, WW_ERRCODE)
                    If Not isNormal(WW_ERRCODE) Then Exit Sub

                    '抽出範囲決定（締まっていない範囲を求める）
                    For i As Integer = 0 To WW_MMCNT
                        Dim WW_DATE As String = dt.AddMonths(i).ToString("yyyy/MM")

                        '勤怠締テーブル取得
                        Dim WW_LIMITFLG As String = "0"
                        Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                           WW_ORG,
                                           WW_DATE,
                                           WW_LIMITFLG,
                                           WW_ERR_RTN)
                        If Not isNormal(WW_ERR_RTN) Then
                            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                            Exit Sub
                        End If

                        '締まっていたらサマリーテーブルから取得するためスキップする
                        If WW_LIMITFLG = "1" Then
                            WW_STYMD = C_DEFAULT_YMD
                            WW_ENDYMD = C_DEFAULT_YMD
                            Continue For
                        ElseIf WW_LIMITFLG = "0" Then
                            If WW_DATE = CDate(work.WF_SEL_STYMD.Text).ToString("yyyy/MM") Then
                                WW_STYMD = work.WF_SEL_STYMD.Text
                            Else
                                WW_STYMD = WW_DATE & "/01"
                            End If
                            WW_ENDYMD = work.WF_SEL_ENDYMD.Text
                            Exit For
                        End If
                    Next

                    Try

                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = ""
                        PARA04.Value = Date.Now
                        PARA05.Value = C_MAX_YMD
                        PARA06.Value = C_DEFAULT_YMD
                        PARA07.Value = C_MAX_YMD
                        PARA08.Value = C_DEFAULT_YMD
                        PARA09.Value = C_MAX_YMD
                        PARA10.Value = C_DEFAULT_YMD
                        PARA11.Value = C_MAX_YMD
                        PARA12.Value = C_DEFAULT_YMD
                        Select Case work.WF_SEL_FUNC.Text
                            Case "0"    '含めない
                                PARA13.Value = "1"
                            Case Else   '含める
                                PARA13.Value = "2"
                        End Select

                        Select Case work.WF_SEL_FIELDSEL.Text
                            Case "1"    '出庫日
                                PARA05.Value = WW_ENDYMD
                                PARA06.Value = WW_STYMD
                            Case "2"    '出荷日
                                PARA07.Value = WW_ENDYMD
                                PARA08.Value = WW_STYMD
                            Case "3"    '届日
                                PARA09.Value = WW_ENDYMD
                                PARA10.Value = WW_STYMD
                            Case "4"    '計上日
                                PARA11.Value = WW_ENDYMD
                                PARA12.Value = WW_STYMD
                        End Select
                        PARA14.Value = WI_ORG

                        SQLcmd.CommandTimeout = 300

                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            'ブレークKey
                            Dim WW_NACSHUKODATE As String = ""
                            Dim WW_NACSHUKADATE As String = ""
                            Dim WW_NACTODOKEDATE As String = ""
                            Dim WW_NACTORICODE As String = ""
                            Dim WW_NACSHIPORG As String = ""
                            Dim WW_KEYGSHABAN As String = ""
                            Dim WW_KEYSTAFFCODE As String = ""
                            Dim WW_KEYTRIPNO As String = ""
                            Dim WW_KEYDROPNO As String = ""
                            Dim WW_NACSEQ As String = ""
                            Dim WW_ACACHANTEI As String = ""
                            '判定Key
                            Dim wNACSHUKODATE As String = ""
                            Dim wNACSHUKADATE As String = ""
                            Dim wNACTODOKEDATE As String = ""
                            Dim wNACTORICODE As String = ""
                            Dim wNACSHIPORG As String = ""
                            Dim wKEYGSHABAN As String = ""
                            Dim wKEYSTAFFCODE As String = ""
                            Dim wKEYTRIPNO As String = ""
                            Dim wKEYDROPNO As String = ""
                            Dim wNACSEQ As String = ""
                            Dim wACACHANTEI As String = ""
                            'レコード集計
                            Dim wSUM_NACSURYO As Double = 0
                            Dim wSUM_NACSURYOG As Double = 0

                            Dim wSUM_NACJSURYO As Double = 0
                            Dim wSUM_NACJSURYOG As Double = 0

                            Dim wSUM_NACHAIDISTANCE As Double = 0                                                 '実績・配送距離
                            Dim wSUM_NACKAIDISTANCE As Double = 0                                                 '実績・下車作業距離
                            Dim wSUM_NACCHODISTANCE As Double = 0                                                 '実績・勤怠調整距離
                            Dim wSUM_NACTTLDISTANCE As Double = 0                                                 '実績・配送距離合計Σ
                            Dim wSUM_NACHAIWORKTIME As Integer = 0                                                '実績・配送作業時間
                            Dim wSUM_NACGESWORKTIME As Integer = 0                                                '実績・下車作業時間
                            Dim wSUM_NACCHOWORKTIME As Integer = 0                                                '実績・勤怠調整時間
                            Dim wSUM_NACTTLWORKTIME As Integer = 0                                                '実績・配送合計時間Σ
                            Dim wSUM_NACOUTWORKTIME As Integer = 0                                                '実績・就業外時間
                            Dim wSUM_NACBREAKTIME As Integer = 0                                                  '実績・休憩時間
                            Dim wSUM_NACCHOBREAKTIME As Integer = 0                                               '実績・休憩調整時間
                            Dim wSUM_NACTTLBREAKTIME As Integer = 0                                               '実績・休憩合計時間Σ
                            Dim wSUM_NACCASH As Integer = 0                                                       '実績・現金
                            Dim wSUM_NACETC As Integer = 0                                                        '実績・ETC
                            Dim wSUM_NACTICKET As Integer = 0                                                     '実績・回数券
                            Dim wSUM_NACKYUYU As Double = 0                                                       '実績・軽油
                            Dim wSUM_NACUNLOADCNT As Integer = 0                                                  '実績・荷卸回数
                            Dim wSUM_NACCHOUNLOADCNT As Integer = 0                                               '実績・荷卸回数調整
                            Dim wSUM_NACTTLUNLOADCNT As Integer = 0                                               '実績・荷卸回数合計Σ
                            Dim wSUM_NACKAIJI As Integer = 0                                                      '実績・回次
                            Dim wSUM_NACJITIME As Integer = 0                                                     '実績・実車時間
                            Dim wSUM_NACJICHOSTIME As Integer = 0                                                 '実績・実車時間調整
                            Dim wSUM_NACJITTLETIME As Integer = 0                                                 '実績・実車時間合計Σ
                            Dim wSUM_NACKUTIME As Integer = 0                                                     '実績・空車時間
                            Dim wSUM_NACKUCHOTIME As Integer = 0                                                  '実績・空車時間調整
                            Dim wSUM_NACKUTTLTIME As Integer = 0                                                  '実績・空車時間合計Σ
                            Dim wSUM_NACJKTTLTIME As Integer = 0                                                  '実績・実車/空車時間合計Σ
                            Dim wSUM_NACJIDISTANCE As Double = 0                                                  '実績・実車距離
                            Dim wSUM_NACJICHODISTANCE As Double = 0                                               '実績・実車距離調整
                            Dim wSUM_NACJITTLDISTANCE As Double = 0                                               '実績・実車距離合計Σ
                            Dim wSUM_NACKUDISTANCE As Double = 0                                                  '実績・空車距離
                            Dim wSUM_NACKUCHODISTANCE As Double = 0                                               '実績・空車距離調整
                            Dim wSUM_NACKUTTLDISTANCE As Double = 0                                               '実績・空車距離合計Σ
                            Dim wSUM_NACJKTTLDISTANCE As Double = 0                                               '実績・実車/空車距離合計Σ

                            Dim wSUM_NACTARIFFFARE As Integer = 0                                                 '実績・運賃タリフ額
                            Dim wSUM_NACFIXEDFARE As Integer = 0                                                  '実績・運賃固定額
                            Dim wSUM_NACINCHOFARE As Integer = 0                                                  '実績・運賃手入力調整額
                            Dim wSUM_NACTTLFARE As Integer = 0                                                    '実績・運賃合計額Σ
                            Dim wSUM_NACOFFICETIME As Integer = 0                                                 '実績・従業時間
                            Dim wSUM_NACOFFICEBREAKTIME As Integer = 0                                            '実績・従業休憩時間
                            Dim wSUM_NACHAISTDATE As String = C_DEFAULT_YMD                                       '実績・配送作業開始日時
                            Dim wSUM_NACHAIENDDATE As String = C_DEFAULT_YMD                                      '実績・配送作業終了日時

                            Dim wSEQ As Integer = 0

                            While SQLdr.Read

                                '〇不要レコード判定
                                If Not {"HJD", "HLD", "HSD", "KJD", "KLD", "KSD", "RSD", "KED", "TUD", "URC",
                                        "HJC", "HLC", "HSC", "KJC", "KLC", "KSC", "RSC", "KEC", "TUC", "URD"}.Contains(SQLdr("ACACHANTEI")) Then
                                    Continue While
                                End If

                                If work.WF_SEL_FUNCBRK.Text = "0" Then
                                    '休憩を含めない
                                    If SQLdr("ACACHANTEI") = "RSC" OrElse SQLdr("ACACHANTEI") = "RSD" Then
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

                                If IsDate(SQLdr("NACSHUKADATE")) AndAlso SQLdr("NACSHUKADATE") <> C_DEFAULT_YMD Then   '出荷日
                                    wDATE = SQLdr("NACSHUKADATE")
                                    wNACSHUKADATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACSHUKADATE = C_DEFAULT_YMD
                                End If

                                If IsDate(SQLdr("NACTODOKEDATE")) AndAlso SQLdr("NACTODOKEDATE") <> C_DEFAULT_YMD Then '届日
                                    wDATE = SQLdr("NACTODOKEDATE")
                                    wNACTODOKEDATE = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    wNACTODOKEDATE = C_DEFAULT_YMD
                                End If

                                wNACTORICODE = SQLdr("NACTORICODE")                                               '荷主
                                wNACSHIPORG = SQLdr("NACSHIPORG")                                                 '配送部署
                                wKEYGSHABAN = SQLdr("KEYGSHABAN")                                                 'SYS業務車番
                                wKEYSTAFFCODE = SQLdr("KEYSTAFFCODE")                                             'SYS従業員
                                wKEYTRIPNO = SQLdr("KEYTRIPNO")                                                   'SYSトリップ
                                wKEYDROPNO = SQLdr("KEYDROPNO")                                                   'SYSドロップ
                                wNACSEQ = SQLdr("NACSEQ")                                                         'SEQ
                                wACACHANTEI = SQLdr("ACACHANTEI")                                                 '仕訳決定

                                '〇Keyブレーク時のレコード設定
                                If WW_NACSHUKODATE = wNACSHUKODATE AndAlso
                                   WW_NACSHUKADATE = wNACSHUKADATE AndAlso
                                   WW_NACTODOKEDATE = wNACTODOKEDATE AndAlso
                                   WW_NACTORICODE = wNACTORICODE AndAlso
                                   WW_NACSHIPORG = wNACSHIPORG AndAlso
                                   WW_KEYGSHABAN = wKEYGSHABAN AndAlso
                                   WW_KEYSTAFFCODE = wKEYSTAFFCODE AndAlso
                                   WW_KEYTRIPNO = wKEYTRIPNO AndAlso
                                   WW_KEYDROPNO = wKEYDROPNO Then
                                Else
                                    '〇１件目はなにもしない
                                    If WW_NACSHUKODATE = "" AndAlso
                                       WW_NACSHUKADATE = "" AndAlso
                                       WW_NACTODOKEDATE = "" AndAlso
                                       WW_NACTORICODE = "" AndAlso
                                       WW_NACSHIPORG = "" AndAlso
                                       WW_KEYGSHABAN = "" AndAlso
                                       WW_KEYSTAFFCODE = "" AndAlso
                                       WW_KEYTRIPNO = "" AndAlso
                                       WW_KEYDROPNO = "" Then

                                    Else
                                        '〇レコード出力
                                        '合計値セット
                                        TA0004row("NACHAIDISTANCE") = wSUM_NACHAIDISTANCE.ToString("#0.00")                 '実績・配送距離
                                        TA0004row("NACKAIDISTANCE") = wSUM_NACKAIDISTANCE.ToString("#0.00")                 '実績・下車作業距離
                                        TA0004row("NACCHODISTANCE") = wSUM_NACCHODISTANCE.ToString("#0.00")                 '実績・勤怠調整距離
                                        TA0004row("NACTTLDISTANCE") = wSUM_NACTTLDISTANCE.ToString("#0.00")                 '実績・配送距離合計Σ
                                        TA0004row("NACHAISTDATE") = wSUM_NACHAISTDATE                                       '実績・配送作業開始日時
                                        TA0004row("NACHAIENDDATE") = wSUM_NACHAIENDDATE                                     '実績・配送作業終了日時
                                        TA0004row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                        TA0004row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                        TA0004row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                        TA0004row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                        TA0004row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                        TA0004row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                        TA0004row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                        TA0004row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                        TA0004row("NACCASH") = wSUM_NACCASH                                                 '実績・現金
                                        TA0004row("NACETC") = wSUM_NACETC                                                   '実績・ETC
                                        TA0004row("NACTICKET") = wSUM_NACTICKET                                             '実績・回数券
                                        TA0004row("NACKYUYU") = wSUM_NACKYUYU                                               '実績・軽油
                                        TA0004row("NACUNLOADCNT") = wSUM_NACUNLOADCNT                                       '実績・荷卸回数
                                        TA0004row("NACCHOUNLOADCNT") = wSUM_NACCHOUNLOADCNT                                 '実績・荷卸回数調整
                                        TA0004row("NACTTLUNLOADCNT") = wSUM_NACTTLUNLOADCNT                                 '実績・荷卸回数合計Σ
                                        TA0004row("NACJITIME") = wSUM_NACJITIME                                             '実績・実車時間
                                        TA0004row("NACKAIJI") = wSUM_NACKAIJI                                               '実績・回次
                                        TA0004row("NACJICHOSTIME") = wSUM_NACJICHOSTIME                                     '実績・実車時間調整
                                        TA0004row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                        TA0004row("NACKUTIME") = wSUM_NACKUTIME                                             '実績・空車時間
                                        TA0004row("NACKUCHOTIME") = wSUM_NACKUCHOTIME                                       '実績・空車時間調整
                                        TA0004row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                        TA0004row("NACJKTTLTIME") = wSUM_NACJKTTLTIME                                       '実績・実車/空車時間合計Σ
                                        TA0004row("NACJIDISTANCE") = wSUM_NACJIDISTANCE                                     '実績・実車距離
                                        TA0004row("NACJICHODISTANCE") = wSUM_NACJICHODISTANCE                               '実績・実車距離調整
                                        TA0004row("NACJITTLDISTANCE") = wSUM_NACJITTLDISTANCE                               '実績・実車距離合計Σ
                                        TA0004row("NACKUDISTANCE") = wSUM_NACKUDISTANCE                                     '実績・空車距離
                                        TA0004row("NACKUCHODISTANCE") = wSUM_NACKUCHODISTANCE                               '実績・空車距離調整
                                        TA0004row("NACKUTTLDISTANCE") = wSUM_NACKUTTLDISTANCE                               '実績・空車距離合計Σ
                                        TA0004row("NACJKTTLDISTANCE") = wSUM_NACJKTTLDISTANCE                               '実績・実車/空車距離合計Σ
                                        TA0004row("NACTARIFFFARE") = wSUM_NACTARIFFFARE                                     '実績・運賃タリフ額
                                        TA0004row("NACFIXEDFARE") = wSUM_NACFIXEDFARE                                       '実績・運賃固定額
                                        TA0004row("NACINCHOFARE") = wSUM_NACINCHOFARE                                       '実績・運賃手入力調整額
                                        TA0004row("NACTTLFARE") = wSUM_NACTTLFARE                                           '実績・運賃合計額Σ
                                        TA0004row("NACOFFICETIME") = wSUM_NACOFFICETIME                                     '実績・従業時間
                                        TA0004row("NACOFFICEBREAKTIME") = wSUM_NACOFFICEBREAKTIME                           '実績・従業休憩時間
                                        TA0004row("NACSURYOG") = wSUM_NACSURYOG.ToString("#0.000")                          '受注・数量合計
                                        TA0004row("NACJSURYOG") = wSUM_NACJSURYOG.ToString("#0.000")                        '実績・配送数量合計
                                        If TA0004row("ACACHANTEI") = "KED" OrElse
                                           TA0004row("ACACHANTEI") = "KEC" Then
                                            TA0004row("NACKYUYU") = wSUM_NACKYUYU
                                        End If

                                        TA0004tbl.Rows.Add(TA0004row)

                                    End If
                                    '〇新レコード準備(固定項目設定)
                                    TA0004row = TA0004tbl.NewRow
                                    wSEQ = 0

                                    'ブレイクキー設定
                                    WW_NACSHUKODATE = wNACSHUKODATE
                                    WW_NACSHUKADATE = wNACSHUKADATE
                                    WW_NACTODOKEDATE = wNACTODOKEDATE
                                    WW_NACTORICODE = wNACTORICODE
                                    WW_NACSHIPORG = wNACSHIPORG
                                    WW_KEYGSHABAN = wKEYGSHABAN
                                    WW_KEYSTAFFCODE = wKEYSTAFFCODE
                                    WW_KEYTRIPNO = wKEYTRIPNO
                                    WW_KEYDROPNO = wKEYDROPNO
                                    WW_ACACHANTEI = wACACHANTEI

                                    '合計項目クリア
                                    'wSUM_ACAMT =0                                                                  '金額
                                    wSUM_NACHAIDISTANCE = 0                                                         '実績・配送距離
                                    wSUM_NACKAIDISTANCE = 0                                                         '実績・下車作業距離
                                    wSUM_NACCHODISTANCE = 0                                                         '実績・勤怠調整距離
                                    wSUM_NACTTLDISTANCE = 0                                                         '実績・配送距離合計Σ
                                    wSUM_NACHAIWORKTIME = 0                                                         '実績・配送作業時間
                                    wSUM_NACGESWORKTIME = 0                                                         '実績・下車作業時間
                                    wSUM_NACCHOWORKTIME = 0                                                         '実績・勤怠調整時間
                                    wSUM_NACTTLWORKTIME = 0                                                         '実績・配送合計時間Σ
                                    wSUM_NACOUTWORKTIME = 0                                                         '実績・就業外時間
                                    wSUM_NACBREAKTIME = 0                                                           '実績・休憩時間
                                    wSUM_NACCHOBREAKTIME = 0                                                        '実績・休憩調整時間
                                    wSUM_NACTTLBREAKTIME = 0                                                        '実績・休憩合計時間Σ
                                    wSUM_NACCASH = 0                                                                '実績・現金
                                    wSUM_NACETC = 0                                                                 '実績・ETC
                                    wSUM_NACTICKET = 0                                                              '実績・回数券
                                    wSUM_NACKYUYU = 0                                                               '実績・軽油
                                    wSUM_NACUNLOADCNT = 0                                                           '実績・荷卸回数
                                    wSUM_NACCHOUNLOADCNT = 0                                                        '実績・荷卸回数調整
                                    wSUM_NACTTLUNLOADCNT = 0                                                        '実績・荷卸回数合計Σ
                                    wSUM_NACKAIJI = 0                                                               '実績・回次
                                    wSUM_NACJITIME = 0                                                              '実績・実車時間
                                    wSUM_NACJICHOSTIME = 0                                                          '実績・実車時間調整
                                    wSUM_NACJITTLETIME = 0                                                          '実績・実車時間合計Σ
                                    wSUM_NACKUTIME = 0                                                              '実績・空車時間
                                    wSUM_NACKUCHOTIME = 0                                                           '実績・空車時間調整
                                    wSUM_NACKUTTLTIME = 0                                                           '実績・空車時間合計Σ
                                    wSUM_NACJKTTLTIME = 0                                                           '実績・実車/空車時間合計Σ
                                    wSUM_NACJIDISTANCE = 0                                                          '実績・実車距離
                                    wSUM_NACJICHODISTANCE = 0                                                       '実績・実車距離調整
                                    wSUM_NACJITTLDISTANCE = 0                                                       '実績・実車距離合計Σ
                                    wSUM_NACKUDISTANCE = 0                                                          '実績・空車距離
                                    wSUM_NACKUCHODISTANCE = 0                                                       '実績・空車距離調整
                                    wSUM_NACKUTTLDISTANCE = 0                                                       '実績・空車距離合計Σ
                                    wSUM_NACJKTTLDISTANCE = 0                                                       '実績・実車/空車距離合計Σ

                                    wSUM_NACTARIFFFARE = 0                                                          '実績・運賃タリフ額
                                    wSUM_NACFIXEDFARE = 0                                                           '実績・運賃固定額
                                    wSUM_NACINCHOFARE = 0                                                           '実績・運賃手入力調整額
                                    wSUM_NACTTLFARE = 0                                                             '実績・運賃合計額Σ
                                    wSUM_NACOFFICETIME = 0                                                          '実績・従業時間
                                    wSUM_NACOFFICEBREAKTIME = 0                                                     '実績・従業休憩時間
                                    wSUM_NACSURYOG = 0
                                    wSUM_NACJSURYOG = 0

                                    wSUM_NACHAISTDATE = C_DEFAULT_YMD
                                    wSUM_NACHAIENDDATE = C_DEFAULT_YMD

                                    '固定項目
                                    TA0004row("LINECNT") = "0"                                                      'DBの固定フィールド(2017/11/5)
                                    TA0004row("OPERATION") = ""                                                     'DBの固定フィールド(2017/11/5)
                                    TA0004row("TIMSTP") = "0"                                                       'DBの固定フィールド(2017/11/5)
                                    TA0004row("SELECT") = "0"                                                       'DBの固定フィールド
                                    TA0004row("HIDDEN") = "0"                                                       'DBの固定フィールド(2017/11/5)

                                    '画面固有項目
                                    TA0004row("CAMPCODE") = SQLdr("CAMPCODE")                                       '会社
                                    TA0004row("CAMPNAME") = SQLdr("CAMPNAME")                                       '会社名称
                                    TA0004row("MOTOCHO") = ""                                                       '元帳(2017/11/5)
                                    TA0004row("MOTOCHONAME") = ""                                                   '元帳名称(2017/11/5)
                                    TA0004row("VERSION") = ""                                                       'バージョン(2017/11/5)
                                    TA0004row("DENTYPE") = ""                                                       '伝票タイプ(2017/11/5)
                                    TA0004row("TENKI") = ""                                                         '統計転記(2017/11/5)
                                    TA0004row("TENKINAME") = ""                                                     '統計転記名称(2017/11/5)
                                    If IsDate(SQLdr("KEIJOYMD")) AndAlso SQLdr("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                        wDATE = SQLdr("KEIJOYMD")
                                        TA0004row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0004row("KEIJOYMD") = C_DEFAULT_YMD
                                    End If

                                    If IsDate(SQLdr("DENYMD")) AndAlso SQLdr("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                        wDATE = SQLdr("DENYMD")
                                        TA0004row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0004row("DENYMD") = C_DEFAULT_YMD
                                    End If

                                    TA0004row("DENNO") = SQLdr("DENNO")                                             '伝票番号
                                    TA0004row("KANRENDENNO") = SQLdr("KANRENDENNO")                                 '関連伝票No＋明細No
                                    TA0004row("DTLNO") = SQLdr("DTLNO")                                             '明細番号
                                    TA0004row("INQKBN") = SQLdr("INQKBN")                                           '照会区分
                                    TA0004row("INQKBNNAME") = SQLdr("INQKBNNAME")                                   '照会区分名称
                                    TA0004row("ACACHANTEI") = SQLdr("ACACHANTEI")                                   '仕訳決定
                                    TA0004row("ACACHANTEINAME") = SQLdr("ACACHANTEINAME")                           '仕訳決定名称

                                    If IsDate(SQLdr("NACSHUKODATE")) AndAlso SQLdr("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                        wDATE = SQLdr("NACSHUKODATE")
                                        TA0004row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0004row("NACSHUKODATE") = C_DEFAULT_YMD
                                    End If

                                    If IsDate(SQLdr("NACSHUKADATE")) AndAlso SQLdr("NACSHUKADATE") <> C_DEFAULT_YMD Then   '出荷日
                                        wDATE = SQLdr("NACSHUKADATE")
                                        TA0004row("NACSHUKADATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0004row("NACSHUKADATE") = C_DEFAULT_YMD
                                    End If

                                    If IsDate(SQLdr("NACTODOKEDATE")) AndAlso SQLdr("NACTODOKEDATE") <> C_DEFAULT_YMD Then '届日
                                        wDATE = SQLdr("NACTODOKEDATE")
                                        TA0004row("NACTODOKEDATE") = wDATE.ToString("yyyy/MM/dd")
                                    Else
                                        TA0004row("NACTODOKEDATE") = C_DEFAULT_YMD
                                    End If
                                    '
                                    TA0004row("NACTORICODE") = SQLdr("NACTORICODE")                                 '荷主
                                    TA0004row("NACTORICODENAME") = SQLdr("NACTORICODENAME")                         '荷主名称
                                    TA0004row("NACURIKBN") = SQLdr("NACURIKBN")                                     '売上計上基準
                                    TA0004row("NACURIKBNNAME") = SQLdr("NACURIKBNNAME")                             '売上計上基準名称
                                    TA0004row("NACTODOKECODE") = SQLdr("NACTODOKECODE")                             '届先
                                    TA0004row("NACTODOKECODENAME") = SQLdr("NACTODOKECODENAME")                     '届先名称
                                    TA0004row("NACSTORICODE") = SQLdr("NACSTORICODE")                               '販売店
                                    TA0004row("NACSTORICODENAME") = SQLdr("NACSTORICODENAME")                       '販売店名称
                                    TA0004row("NACSHUKABASHO") = SQLdr("NACSHUKABASHO")                             '出荷場所
                                    TA0004row("NACSHUKABASHONAME") = SQLdr("NACSHUKABASHONAME")                     '出荷場所名称
                                    TA0004row("NACTORITYPE01") = SQLdr("NACTORITYPE01")                             '取引タイプ01
                                    TA0004row("NACTORITYPE01NAME") = SQLdr("NACTORITYPE01NAME")                     '取引タイプ01名称
                                    TA0004row("NACTORITYPE02") = SQLdr("NACTORITYPE02")                             '取引タイプ02
                                    TA0004row("NACTORITYPE02NAME") = SQLdr("NACTORITYPE02NAME")                     '取引タイプ02名称
                                    TA0004row("NACTORITYPE03") = SQLdr("NACTORITYPE03")                             '取引タイプ03
                                    TA0004row("NACTORITYPE03NAME") = SQLdr("NACTORITYPE03NAME")                     '取引タイプ03名称
                                    TA0004row("NACTORITYPE04") = SQLdr("NACTORITYPE04")                             '取引タイプ04
                                    TA0004row("NACTORITYPE04NAME") = SQLdr("NACTORITYPE04NAME")                     '取引タイプ04名称
                                    TA0004row("NACTORITYPE05") = SQLdr("NACTORITYPE05")                             '取引タイプ05
                                    TA0004row("NACTORITYPE05NAME") = SQLdr("NACTORITYPE05NAME")                     '取引タイプ05名称

                                    TA0004row("NACGSHABAN") = SQLdr("NACGSHABAN")                                   '業務車番
                                    TA0004row("NACSUPPLIERKBN") = SQLdr("NACSUPPLIERKBN")                           '社有・庸車区分
                                    TA0004row("NACSUPPLIERKBNNAME") = SQLdr("NACSUPPLIERKBNNAME")                   '社有・庸車区分名称
                                    TA0004row("NACSUPPLIER") = SQLdr("NACSUPPLIER")                                 '庸車会社
                                    TA0004row("NACSUPPLIERNAME") = SQLdr("NACSUPPLIERNAME")                         '庸車会社名称
                                    TA0004row("NACSHARYOOILTYPE") = SQLdr("NACSHARYOOILTYPE")                       '車両登録油種
                                    TA0004row("NACSHARYOOILTYPENAME") = SQLdr("NACSHARYOOILTYPENAME")               '車両登録油種名称

                                    TA0004row("NACSHARYOTYPE1") = SQLdr("NACSHARYOTYPE1")                           '車両タイプ1
                                    TA0004row("NACSHARYOTYPE1NAME") = SQLdr("NACSHARYOTYPE1NAME")                   '車両タイプ1名称
                                    TA0004row("NACTSHABAN1") = SQLdr("NACTSHABAN1")                                 '統一車番1
                                    TA0004row("NACMANGMORG1") = SQLdr("NACMANGMORG1")                               '車両管理部署1
                                    TA0004row("NACMANGMORG1NAME") = SQLdr("NACMANGMORG1NAME")                       '車両管理部署1名称
                                    TA0004row("NACMANGSORG1") = SQLdr("NACMANGSORG1")                               '車両設置部署1
                                    TA0004row("NACMANGSORG1NAME") = SQLdr("NACMANGSORG1NAME")                       '車両設置部署1名称
                                    TA0004row("NACMANGUORG1") = SQLdr("NACMANGUORG1")                               '車両運用部署1
                                    TA0004row("NACMANGUORG1NAME") = SQLdr("NACMANGUORG1NAME")                       '車両運用部署1名称
                                    TA0004row("NACBASELEASE1") = SQLdr("NACBASELEASE1")                             '車両所有1
                                    TA0004row("NACBASELEASE1NAME") = SQLdr("NACBASELEASE1NAME")                     '車両所有1名称
                                    TA0004row("NACLICNPLTNOF1") = SQLdr("NACLICNPLTNOF1")                           '登録番号1
                                    TA0004row("NACSHARYOTYPE2") = SQLdr("NACSHARYOTYPE2")                           '車両タイプ2
                                    TA0004row("NACSHARYOTYPE2NAME") = SQLdr("NACSHARYOTYPE2NAME")                   '車両タイプ2名称
                                    TA0004row("NACTSHABAN2") = SQLdr("NACTSHABAN2")                                 '統一車番2
                                    TA0004row("NACMANGMORG2") = SQLdr("NACMANGMORG2")                               '車両管理部署2
                                    TA0004row("NACMANGMORG2NAME") = SQLdr("NACMANGMORG2NAME")                       '車両管理部署2名称
                                    TA0004row("NACMANGSORG2") = SQLdr("NACMANGSORG2")                               '車両設置部署2
                                    TA0004row("NACMANGSORG2NAME") = SQLdr("NACMANGSORG2NAME")                       '車両設置部署2名称
                                    TA0004row("NACMANGUORG2") = SQLdr("NACMANGUORG2")                               '車両運用部署2
                                    TA0004row("NACMANGUORG2NAME") = SQLdr("NACMANGUORG2NAME")                       '車両運用部署2名称
                                    TA0004row("NACBASELEASE2") = SQLdr("NACBASELEASE2")                             '車両所有2
                                    TA0004row("NACBASELEASE2NAME") = SQLdr("NACBASELEASE2NAME")                     '車両所有2名称
                                    TA0004row("NACLICNPLTNOF2") = SQLdr("NACLICNPLTNOF2")                           '登録番号2
                                    TA0004row("NACSHARYOTYPE3") = SQLdr("NACSHARYOTYPE3")                           '車両タイプ3
                                    TA0004row("NACSHARYOTYPE3NAME") = SQLdr("NACSHARYOTYPE3NAME")                   '車両タイプ3名称
                                    TA0004row("NACTSHABAN3") = SQLdr("NACTSHABAN3")                                 '統一車番3
                                    TA0004row("NACMANGMORG3") = SQLdr("NACMANGMORG3")                               '車両管理部署3
                                    TA0004row("NACMANGMORG3NAME") = SQLdr("NACMANGMORG3NAME")                       '車両管理部署3名称
                                    TA0004row("NACMANGSORG3") = SQLdr("NACMANGSORG3")                               '車両設置部署3
                                    TA0004row("NACMANGSORG3NAME") = SQLdr("NACMANGSORG3NAME")                       '車両設置部署3名称
                                    TA0004row("NACMANGUORG3") = SQLdr("NACMANGUORG3")                               '車両運用部署3
                                    TA0004row("NACMANGUORG3NAME") = SQLdr("NACMANGUORG3NAME")                       '車両運用部署3名称
                                    TA0004row("NACBASELEASE3") = SQLdr("NACBASELEASE3")                             '車両所有3
                                    TA0004row("NACBASELEASE3NAME") = SQLdr("NACBASELEASE3NAME")                     '車両所有3名称
                                    TA0004row("NACLICNPLTNOF3") = SQLdr("NACLICNPLTNOF3")                           '登録番号3

                                    TA0004row("NACCREWKBN") = SQLdr("NACCREWKBN")                                   '正副区分
                                    TA0004row("NACCREWKBNNAME") = SQLdr("NACCREWKBNNAME")                           '正副区分名称
                                    TA0004row("NACSTAFFCODE") = SQLdr("NACSTAFFCODE")                               '乗務員・従業員コード
                                    TA0004row("NACSTAFFCODENAME") = SQLdr("NACSTAFFCODENAME")                       '乗務員・従業員コード名称
                                    TA0004row("NACSTAFFKBN") = SQLdr("NACSTAFFKBN")                                 '乗務員・社員区分
                                    TA0004row("NACSTAFFKBNNAME") = SQLdr("NACSTAFFKBNNAME")                         '乗務員・社員区分名称
                                    TA0004row("NACMORG") = SQLdr("NACMORG")                                         '乗務員・管理部署
                                    TA0004row("NACMORGNAME") = SQLdr("NACMORGNAME")                                 '乗務員・管理部署名称
                                    TA0004row("NACHORG") = SQLdr("NACHORG")                                         '乗務員・配属部署
                                    TA0004row("NACHORGNAME") = SQLdr("NACHORGNAME")                                 '乗務員・配属部署名称
                                    TA0004row("NACSORG") = SQLdr("NACSORG")                                         '乗務員・作業部署
                                    TA0004row("NACSORGNAME") = SQLdr("NACSORGNAME")                                 '乗務員・作業部署名称
                                    TA0004row("NACSTAFFCODE2") = SQLdr("NACSTAFFCODE2")                             '副乗務員・従業員コード
                                    TA0004row("NACSTAFFCODE2NAME") = SQLdr("NACSTAFFCODE2NAME")                     '副乗務員・従業員コード名称
                                    TA0004row("NACSTAFFKBN2") = SQLdr("NACSTAFFKBN2")                               '副乗務員・社員区分
                                    TA0004row("NACSTAFFKBN2NAME") = SQLdr("NACSTAFFKBN2NAME")                       '副乗務員・社員区分名称
                                    TA0004row("NACMORG2") = SQLdr("NACMORG2")                                       '副乗務員・管理部署
                                    TA0004row("NACMORG2NAME") = SQLdr("NACMORG2NAME")                               '副乗務員・管理部署名称
                                    TA0004row("NACHORG2") = SQLdr("NACHORG2")                                       '副乗務員・配属部署
                                    TA0004row("NACHORG2NAME") = SQLdr("NACHORG2NAME")                               '副乗務員・配属部署名称
                                    TA0004row("NACSORG2") = SQLdr("NACSORG2")                                       '副乗務員・作業部署
                                    TA0004row("NACSORG2NAME") = SQLdr("NACSORG2NAME")                               '副乗務員・作業部署名称

                                    TA0004row("NACORDERNO") = SQLdr("NACORDERNO")                                   '受注番号
                                    TA0004row("NACDETAILNO") = SQLdr("NACDETAILNO")                                 '明細№
                                    TA0004row("NACTRIPNO") = SQLdr("NACTRIPNO")                                     'トリップ
                                    TA0004row("NACDROPNO") = SQLdr("NACDROPNO")                                     'ドロップ
                                    TA0004row("NACSEQ") = SQLdr("NACSEQ")                                           'SEQ
                                    TA0004row("NACORDERORG") = SQLdr("NACORDERORG")                                 '受注部署
                                    TA0004row("NACORDERORGNAME") = SQLdr("NACORDERORGNAME")                         '受注部署名称
                                    TA0004row("NACSHIPORG") = SQLdr("NACSHIPORG")                                   '配送部署
                                    TA0004row("NACSHIPORGNAME") = SQLdr("NACSHIPORGNAME")                           '配送部署名称

                                    TA0004row("NACHAISTDATE") = SQLdr("NACHAISTDATE")                               '実績・配送作業開始日時
                                    TA0004row("NACHAIENDDATE") = SQLdr("NACHAIENDDATE")                             '実績・配送作業終了日時

                                    TA0004row("NACGESSTDATE") = SQLdr("NACGESSTDATE")                               '実績・下車作業開始日時
                                    TA0004row("NACGESENDDATE") = SQLdr("NACGESENDDATE")                             '実績・下車作業終了日時

                                    TA0004row("NACBREAKSTDATE") = SQLdr("NACBREAKSTDATE")                           '実績・休憩開始日時
                                    TA0004row("NACBREAKENDDATE") = SQLdr("NACBREAKENDDATE")                         '実績・休憩終了日時

                                    TA0004row("NACOFFICESORG") = SQLdr("NACOFFICESORG")                             '実績・従業作業部署
                                    TA0004row("NACOFFICESORGNAME") = SQLdr("NACOFFICESORGNAME")                     '実績・従業作業部署名称

                                    TA0004row("PAYSHUSHADATE") = SQLdr("PAYSHUSHADATE")                             '出社日時
                                    TA0004row("PAYTAISHADATE") = SQLdr("PAYTAISHADATE")                             '退社日時
                                    TA0004row("PAYSTAFFKBN") = SQLdr("PAYSTAFFKBN")                                 '社員区分
                                    TA0004row("PAYSTAFFKBNNAME") = SQLdr("PAYSTAFFKBNNAME")                         '社員区分名称
                                    TA0004row("PAYSTAFFCODE") = SQLdr("PAYSTAFFCODE")                               '従業員
                                    TA0004row("PAYSTAFFCODENAME") = SQLdr("PAYSTAFFCODENAME")                       '従業員名称
                                    TA0004row("PAYMORG") = SQLdr("PAYMORG")                                         '従業員管理部署
                                    TA0004row("PAYMORGNAME") = SQLdr("PAYMORGNAME")                                 '従業員管理部署名称
                                    TA0004row("PAYHORG") = SQLdr("PAYHORG")                                         '従業員配属部署
                                    TA0004row("PAYHORGNAME") = SQLdr("PAYHORGNAME")                                 '従業員配属部署名称

                                    TA0004row("WORKKBN") = SQLdr("WORKKBN")                                         'SYS作業区分
                                    TA0004row("WORKKBNNAME") = SQLdr("WORKKBNNAME")                                 'SYS作業区分名称
                                    TA0004row("KEYSTAFFCODE") = SQLdr("KEYSTAFFCODE")                               'SYS従業員
                                    TA0004row("KEYGSHABAN") = SQLdr("KEYGSHABAN")                                   'SYS業務車番
                                    TA0004row("KEYTRIPNO") = SQLdr("KEYTRIPNO")                                     'SYSトリップ
                                    TA0004row("KEYDROPNO") = SQLdr("KEYDROPNO")                                     'SYSドロップ

                                    '売上計上日付(★追加)
                                    If TA0004row("NACURIKBN") = "1" Then
                                        '出荷日ベース
                                        TA0004row("NACKEIJODATE") = TA0004row("NACSHUKADATE")
                                    Else
                                        '届日ベース
                                        TA0004row("NACKEIJODATE") = TA0004row("NACTODOKEDATE")
                                    End If

                                    '実績・配送作業開始日時
                                    If IsDate(TA0004row("NACHAISTDATE")) AndAlso TA0004row("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACHAISTDATE")
                                        TA0004row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACHAISTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・配送作業終了日時
                                    If IsDate(TA0004row("NACHAIENDDATE")) AndAlso TA0004row("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACHAIENDDATE")
                                        TA0004row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACHAIENDDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・下車作業開始日時
                                    If IsDate(TA0004row("NACGESSTDATE")) AndAlso TA0004row("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACGESSTDATE")
                                        TA0004row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACGESSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '実績・下車作業終了日時
                                    If IsDate(TA0004row("NACGESENDDATE")) AndAlso TA0004row("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACGESENDDATE")
                                        TA0004row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACGESENDDATE") = C_DEFAULT_YMD
                                    End If

                                    '休憩開始日時
                                    If IsDate(TA0004row("NACBREAKSTDATE")) AndAlso TA0004row("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACBREAKSTDATE")
                                        TA0004row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                    End If

                                    '休憩終了日時
                                    If IsDate(TA0004row("NACBREAKENDDATE")) AndAlso TA0004row("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACBREAKENDDATE")
                                        TA0004row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                    End If

                                    '出社日時
                                    If IsDate(TA0004row("PAYSHUSHADATE")) AndAlso TA0004row("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("PAYSHUSHADATE")
                                        TA0004row("PAYSHUSHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("PAYSHUSHADATE") = C_DEFAULT_YMD
                                    End If

                                    '退社日時
                                    If IsDate(TA0004row("PAYTAISHADATE")) AndAlso TA0004row("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("PAYTAISHADATE")
                                        TA0004row("PAYTAISHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("PAYTAISHADATE") = C_DEFAULT_YMD
                                    End If

                                    '統一車番
                                    TA0004row("KEYTSHABAN1") = TA0004row("NACSHARYOTYPE1") & TA0004row("NACTSHABAN1")     'SYS統一車番1
                                    TA0004row("KEYTSHABAN2") = TA0004row("NACSHARYOTYPE2") & TA0004row("NACTSHABAN2")     'SYS統一車番2
                                    TA0004row("KEYTSHABAN3") = TA0004row("NACSHARYOTYPE3") & TA0004row("NACTSHABAN3")     'SYS統一車番3

                                    '
                                    TA0004row("NACHAIDISTANCE") = ""                                                '実績・配送距離
                                    TA0004row("NACKAIDISTANCE") = ""                                                '実績・下車作業距離
                                    TA0004row("NACCHODISTANCE") = ""                                                '実績・勤怠調整距離
                                    TA0004row("NACTTLDISTANCE") = ""                                                '実績・配送距離合計Σ
                                    TA0004row("NACHAIWORKTIME") = ""                                                '実績・配送作業時間
                                    TA0004row("NACGESWORKTIME") = ""                                                '実績・下車作業時間
                                    TA0004row("NACCHOWORKTIME") = ""                                                '実績・勤怠調整時間
                                    TA0004row("NACTTLWORKTIME") = ""                                                '実績・配送合計時間Σ
                                    TA0004row("NACOUTWORKTIME") = ""                                                '実績・就業外時間
                                    TA0004row("NACBREAKTIME") = ""                                                  '実績・休憩時間
                                    TA0004row("NACCHOBREAKTIME") = ""                                               '実績・休憩調整時間
                                    TA0004row("NACTTLBREAKTIME") = ""                                               '実績・休憩合計時間Σ
                                    TA0004row("NACCASH") = ""                                                       '実績・現金
                                    TA0004row("NACETC") = ""                                                        '実績・ETC
                                    TA0004row("NACTICKET") = ""                                                     '実績・回数券
                                    TA0004row("NACKYUYU") = ""                                                      '実績・軽油
                                    TA0004row("NACUNLOADCNT") = ""                                                  '実績・荷卸回数
                                    TA0004row("NACCHOUNLOADCNT") = ""                                               '実績・荷卸回数調整
                                    TA0004row("NACTTLUNLOADCNT") = ""                                               '実績・荷卸回数合計Σ
                                    TA0004row("NACKAIJI") = "0"                                                     '実績・回次
                                    TA0004row("NACJITIME") = ""                                                     '実績・実車時間
                                    TA0004row("NACJICHOSTIME") = ""                                                 '実績・実車時間調整
                                    TA0004row("NACJITTLETIME") = ""                                                 '実績・実車時間合計Σ
                                    TA0004row("NACKUTIME") = ""                                                     '実績・空車時間
                                    TA0004row("NACKUCHOTIME") = ""                                                  '実績・空車時間調整
                                    TA0004row("NACKUTTLTIME") = ""                                                  '実績・空車時間合計Σ
                                    TA0004row("NACJKTTLTIME") = ""                                                  '実績・実車/空車時間合計Σ
                                    TA0004row("NACJIDISTANCE") = ""                                                 '実績・実車距離
                                    TA0004row("NACJICHODISTANCE") = ""                                              '実績・実車距離調整
                                    TA0004row("NACJITTLDISTANCE") = ""                                              '実績・実車距離合計Σ
                                    TA0004row("NACKUDISTANCE") = ""                                                 '実績・空車距離
                                    TA0004row("NACKUCHODISTANCE") = ""                                              '実績・空車距離調整
                                    TA0004row("NACKUTTLDISTANCE") = ""                                              '実績・空車距離合計Σ
                                    TA0004row("NACJKTTLDISTANCE") = ""                                              '実績・実車/空車距離合計Σ
                                    TA0004row("NACTARIFFFARE") = ""                                                 '実績・運賃タリフ額
                                    TA0004row("NACFIXEDFARE") = ""                                                  '実績・運賃固定額
                                    TA0004row("NACINCHOFARE") = ""                                                  '実績・運賃手入力調整額
                                    TA0004row("NACTTLFARE") = ""                                                    '実績・運賃合計額Σ
                                    TA0004row("NACOFFICETIME") = ""                                                 '実績・従業時間
                                    TA0004row("NACOFFICEBREAKTIME") = ""                                            '実績・従業休憩時間

                                    'SEQ別明細クリア
                                    TA0004row("NACOILTYPE_1") = ""                                                  '油種_1
                                    TA0004row("NACOILTYPENAME_1") = ""                                              '油種名称_1
                                    TA0004row("NACOILTYPE_2") = ""                                                  '油種_2
                                    TA0004row("NACOILTYPENAME_2") = ""                                              '油種名称_2
                                    TA0004row("NACOILTYPE_3") = ""                                                  '油種_3
                                    TA0004row("NACOILTYPENAME_3") = ""                                              '油種名称_3
                                    TA0004row("NACOILTYPE_4") = ""                                                  '油種_4
                                    TA0004row("NACOILTYPENAME_4") = ""                                              '油種名称_4
                                    TA0004row("NACOILTYPE_5") = ""                                                  '油種_5
                                    TA0004row("NACOILTYPENAME_5") = ""                                              '油種名称_5
                                    TA0004row("NACOILTYPE_6") = ""                                                  '油種_6
                                    TA0004row("NACOILTYPENAME_6") = ""                                              '油種名称_6
                                    TA0004row("NACOILTYPE_7") = ""                                                  '油種_7
                                    TA0004row("NACOILTYPENAME_7") = ""                                              '油種名称_7
                                    TA0004row("NACOILTYPE_8") = ""                                                  '油種_8
                                    TA0004row("NACOILTYPENAME_8") = ""                                              '油種名称_8
                                    TA0004row("NACPRODUCT1_1") = ""                                                 '品名１_1
                                    TA0004row("NACPRODUCT1NAME_1") = ""                                             '品名１名称_1
                                    TA0004row("NACPRODUCT1_2") = ""                                                 '品名１_2
                                    TA0004row("NACPRODUCT1NAME_2") = ""                                             '品名１名称_2
                                    TA0004row("NACPRODUCT1_3") = ""                                                 '品名１_3
                                    TA0004row("NACPRODUCT1NAME_3") = ""                                             '品名１名称_3
                                    TA0004row("NACPRODUCT1_4") = ""                                                 '品名１_4
                                    TA0004row("NACPRODUCT1NAME_4") = ""                                             '品名１名称_4
                                    TA0004row("NACPRODUCT1_5") = ""                                                 '品名１_5
                                    TA0004row("NACPRODUCT1NAME_5") = ""                                             '品名１名称_5
                                    TA0004row("NACPRODUCT1_6") = ""                                                 '品名１_6
                                    TA0004row("NACPRODUCT1NAME_6") = ""                                             '品名１名称_6
                                    TA0004row("NACPRODUCT1_7") = ""                                                 '品名１_7
                                    TA0004row("NACPRODUCT1NAME_7") = ""                                             '品名１名称_7
                                    TA0004row("NACPRODUCT1_8") = ""                                                 '品名１_8
                                    TA0004row("NACPRODUCT1NAME_8") = ""                                             '品名１名称_8
                                    TA0004row("NACPRODUCT2_1") = ""                                                 '品名２_1
                                    TA0004row("NACPRODUCT2NAME_1") = ""                                             '品名２名称_1
                                    TA0004row("NACPRODUCT2_2") = ""                                                 '品名２_2
                                    TA0004row("NACPRODUCT2NAME_2") = ""                                             '品名２名称_2
                                    TA0004row("NACPRODUCT2_3") = ""                                                 '品名２_3
                                    TA0004row("NACPRODUCT2NAME_3") = ""                                             '品名２名称_3
                                    TA0004row("NACPRODUCT2_4") = ""                                                 '品名２_4
                                    TA0004row("NACPRODUCT2NAME_4") = ""                                             '品名２名称_4
                                    TA0004row("NACPRODUCT2_5") = ""                                                 '品名２_5
                                    TA0004row("NACPRODUCT2NAME_5") = ""                                             '品名２名称_5
                                    TA0004row("NACPRODUCT2_6") = ""                                                 '品名２_6
                                    TA0004row("NACPRODUCT2NAME_6") = ""                                             '品名２名称_6
                                    TA0004row("NACPRODUCT2_7") = ""                                                 '品名２_7
                                    TA0004row("NACPRODUCT2NAME_7") = ""                                             '品名２名称_7
                                    TA0004row("NACPRODUCT2_8") = ""                                                 '品名２_8
                                    TA0004row("NACPRODUCT2NAME_8") = ""                                             '品名２名称_8

                                    TA0004row("NACSURYO1") = "0"                                                    '受注・数量1
                                    TA0004row("NACTANI1") = ""                                                      '受注・単位1
                                    TA0004row("NACTANINAME1") = ""                                                  '受注・単位1名称
                                    TA0004row("NACSURYO2") = "0"                                                    '受注・数量2
                                    TA0004row("NACTANI2") = ""                                                      '受注・単位2
                                    TA0004row("NACTANINAME2") = ""                                                  '受注・単位2名称
                                    TA0004row("NACSURYO3") = "0"                                                    '受注・数量3
                                    TA0004row("NACTANI3") = ""                                                      '受注・単位3
                                    TA0004row("NACTANINAME3") = ""                                                  '受注・単位3名称
                                    TA0004row("NACSURYO4") = "0"                                                    '受注・数量4
                                    TA0004row("NACTANI4") = ""                                                      '受注・単位4
                                    TA0004row("NACTANINAME4") = ""                                                  '受注・単位4名称
                                    TA0004row("NACSURYO5") = "0"                                                    '受注・数量5
                                    TA0004row("NACTANI5") = ""                                                      '受注・単位5
                                    TA0004row("NACTANINAME5") = ""                                                  '受注・単位5名称
                                    TA0004row("NACSURYO6") = "0"                                                    '受注・数量6
                                    TA0004row("NACTANI6") = ""                                                      '受注・単位6
                                    TA0004row("NACTANINAME6") = ""                                                  '受注・単位6名称
                                    TA0004row("NACSURYO7") = "0"                                                    '受注・数量7
                                    TA0004row("NACTANI7") = ""                                                      '受注・単位7
                                    TA0004row("NACTANINAME7") = ""                                                  '受注・単位7名称
                                    TA0004row("NACSURYO8") = "0"                                                    '受注・数量8
                                    TA0004row("NACTANI8") = ""                                                      '受注・単位8
                                    TA0004row("NACTANINAME8") = ""                                                  '受注・単位8名称
                                    TA0004row("NACJSURYO1") = "0"                                                   '実績・配送数量1
                                    TA0004row("NACSTANI1") = ""                                                     '実績・配送単位1
                                    TA0004row("NACSTANINAME1") = ""                                                 '実績・配送単位1名称
                                    TA0004row("NACJSURYO2") = "0"                                                   '実績・配送数量2
                                    TA0004row("NACSTANI2") = ""                                                     '実績・配送単位2
                                    TA0004row("NACSTANINAME2") = ""                                                 '実績・配送単位2名称
                                    TA0004row("NACJSURYO3") = "0"                                                   '実績・配送数量3
                                    TA0004row("NACSTANI3") = ""                                                     '実績・配送単位3
                                    TA0004row("NACSTANINAME3") = ""                                                 '実績・配送単位3名称
                                    TA0004row("NACJSURYO4") = "0"                                                   '実績・配送数量4
                                    TA0004row("NACSTANI4") = ""                                                     '実績・配送単位4
                                    TA0004row("NACSTANINAME4") = ""                                                 '実績・配送単位4名称
                                    TA0004row("NACJSURYO5") = "0"                                                   '実績・配送数量5
                                    TA0004row("NACSTANI5") = ""                                                     '実績・配送単位5
                                    TA0004row("NACSTANINAME5") = ""                                                 '実績・配送単位5名称
                                    TA0004row("NACJSURYO6") = "0"                                                   '実績・配送数量6
                                    TA0004row("NACSTANI6") = ""                                                     '実績・配送単位6
                                    TA0004row("NACSTANINAME6") = ""                                                 '実績・配送単位6名称
                                    TA0004row("NACJSURYO7") = "0"                                                   '実績・配送数量7
                                    TA0004row("NACSTANI7") = ""                                                     '実績・配送単位7
                                    TA0004row("NACSTANINAME7") = ""                                                 '実績・配送単位7名称
                                    TA0004row("NACJSURYO8") = "0"                                                   '実績・配送数量8
                                    TA0004row("NACSTANI8") = ""                                                     '実績・配送単位8
                                    TA0004row("NACSTANINAME8") = ""                                                 '実績・配送単位8名称

                                    TA0004row("NACSURYOG") = "0"                                                    '受注・数量合計
                                    TA0004row("NACJSURYOG") = "0"                                                   '実績・配送数量合計

                                End If


                                If SQLdr("ACACHANTEI") = "URD" OrElse
                                   SQLdr("ACACHANTEI") = "URC" Then

                                    wSEQ = wSEQ + 1

                                    Dim wDET_NACSURYO As Double = 0
                                    Dim wDET_NACJSURYO As Double = 0

                                    wDET_NACSURYO = Val(SQLdr("NACSURYO1"))                                         '受注・数量1

                                    wDET_NACJSURYO = Val(SQLdr("NACJSURYO1"))                                       '実績・配送数量1

                                    wSUM_NACSURYOG = wSUM_NACSURYOG + wDET_NACSURYO
                                    wSUM_NACJSURYOG = wSUM_NACJSURYOG + wDET_NACJSURYO

                                    Select Case wSEQ
                                        Case 1
                                            TA0004row("NACOILTYPE_1") = SQLdr("NACOILTYPE_1")                       '油種_1
                                            TA0004row("NACOILTYPENAME_1") = SQLdr("NACOILTYPENAME_1")               '油種名称_1
                                            TA0004row("NACPRODUCT1_1") = SQLdr("NACPRODUCT1_1")                     '品名１_1
                                            TA0004row("NACPRODUCT1NAME_1") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_1
                                            TA0004row("NACPRODUCT2_1") = SQLdr("NACPRODUCT2_1")                     '品名２_1
                                            TA0004row("NACPRODUCT2NAME_1") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_1
                                            TA0004row("NACTANI1") = SQLdr("NACTANI1")                               '受注・単位1
                                            TA0004row("NACTANINAME1") = SQLdr("NACTANINAME1")                       '受注・単位1名称
                                            TA0004row("NACSTANI1") = SQLdr("NACSTANI1")                             '実績・配送単位1
                                            TA0004row("NACSTANINAME1") = SQLdr("NACSTANINAME1")                     '実績・配送単位1名称
                                            TA0004row("NACSURYO1") = wDET_NACSURYO.ToString("#0.000")               '受注・数量1
                                            TA0004row("NACJSURYO1") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量1
                                        Case 2
                                            TA0004row("NACOILTYPE_2") = SQLdr("NACOILTYPE_1")                       '油種_2
                                            TA0004row("NACOILTYPENAME_2") = SQLdr("NACOILTYPENAME_1")               '油種名称_2
                                            TA0004row("NACPRODUCT1_2") = SQLdr("NACPRODUCT1_1")                     '品名１_2
                                            TA0004row("NACPRODUCT1NAME_2") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_2
                                            TA0004row("NACPRODUCT2_2") = SQLdr("NACPRODUCT2_1")                     '品名２_2
                                            TA0004row("NACPRODUCT2NAME_2") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_2
                                            TA0004row("NACTANI2") = SQLdr("NACTANI1")                               '受注・単位2
                                            TA0004row("NACTANINAME2") = SQLdr("NACTANINAME1")                       '受注・単位2名称
                                            TA0004row("NACSTANI2") = SQLdr("NACSTANI1")                             '実績・配送単位2
                                            TA0004row("NACSTANINAME2") = SQLdr("NACSTANINAME1")                     '実績・配送単位2名称
                                            TA0004row("NACSURYO2") = wDET_NACSURYO.ToString("#0.000")               '受注・数量2
                                            TA0004row("NACJSURYO2") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量2
                                        Case 3
                                            TA0004row("NACOILTYPE_3") = SQLdr("NACOILTYPE_1")                       '油種_3
                                            TA0004row("NACOILTYPENAME_3") = SQLdr("NACOILTYPENAME_1")               '油種名称_3
                                            TA0004row("NACPRODUCT1_3") = SQLdr("NACPRODUCT1_1")                     '品名１_3
                                            TA0004row("NACPRODUCT1NAME_3") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_3
                                            TA0004row("NACPRODUCT2_3") = SQLdr("NACPRODUCT2_1")                     '品名２_3
                                            TA0004row("NACPRODUCT2NAME_3") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_3
                                            TA0004row("NACTANI3") = SQLdr("NACTANI1")                               '受注・単位3
                                            TA0004row("NACTANINAME3") = SQLdr("NACTANINAME1")                       '受注・単位3名称
                                            TA0004row("NACSTANI3") = SQLdr("NACSTANI1")                             '実績・配送単位3
                                            TA0004row("NACSTANINAME3") = SQLdr("NACSTANINAME1")                     '実績・配送単位3名称
                                            TA0004row("NACSURYO3") = wDET_NACSURYO.ToString("#0.000")               '受注・数量3
                                            TA0004row("NACJSURYO3") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量3
                                        Case 4
                                            TA0004row("NACOILTYPE_4") = SQLdr("NACOILTYPE_1")                       '油種_4
                                            TA0004row("NACOILTYPENAME_4") = SQLdr("NACOILTYPENAME_1")               '油種名称_4
                                            TA0004row("NACPRODUCT1_4") = SQLdr("NACPRODUCT1_1")                     '品名１_4
                                            TA0004row("NACPRODUCT1NAME_4") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_4
                                            TA0004row("NACPRODUCT2_4") = SQLdr("NACPRODUCT2_1")                     '品名２_4
                                            TA0004row("NACPRODUCT2NAME_4") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_4
                                            TA0004row("NACTANI4") = SQLdr("NACTANI1")                               '受注・単位4
                                            TA0004row("NACTANINAME4") = SQLdr("NACTANINAME1")                       '受注・単位4名称
                                            TA0004row("NACSTANI4") = SQLdr("NACSTANI1")                             '実績・配送単位4
                                            TA0004row("NACSTANINAME4") = SQLdr("NACSTANINAME1")                     '実績・配送単位4名称
                                            TA0004row("NACSURYO4") = wDET_NACSURYO.ToString("#0.000")               '受注・数量4
                                            TA0004row("NACJSURYO4") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量4
                                        Case 5
                                            TA0004row("NACOILTYPE_5") = SQLdr("NACOILTYPE_1")                       '油種_5
                                            TA0004row("NACOILTYPENAME_5") = SQLdr("NACOILTYPENAME_1")               '油種名称_5
                                            TA0004row("NACPRODUCT1_5") = SQLdr("NACPRODUCT1_1")                     '品名１_5
                                            TA0004row("NACPRODUCT1NAME_5") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_5
                                            TA0004row("NACPRODUCT2_5") = SQLdr("NACPRODUCT2_1")                     '品名２_5
                                            TA0004row("NACPRODUCT2NAME_5") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_5
                                            TA0004row("NACTANI5") = SQLdr("NACTANI1")                               '受注・単位5
                                            TA0004row("NACTANINAME5") = SQLdr("NACTANINAME1")                       '受注・単位5名称
                                            TA0004row("NACSTANI5") = SQLdr("NACSTANI1")                             '実績・配送単位5
                                            TA0004row("NACSTANINAME5") = SQLdr("NACSTANINAME1")                     '実績・配送単位5名称
                                            TA0004row("NACSURYO5") = wDET_NACSURYO.ToString("#0.000")               '受注・数量5
                                            TA0004row("NACJSURYO5") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量5
                                        Case 6
                                            TA0004row("NACOILTYPE_6") = SQLdr("NACOILTYPE_1")                       '油種_6
                                            TA0004row("NACOILTYPENAME_6") = SQLdr("NACOILTYPENAME_1")               '油種名称_6
                                            TA0004row("NACPRODUCT1_6") = SQLdr("NACPRODUCT1_1")                     '品名１_6
                                            TA0004row("NACPRODUCT1NAME_6") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_6
                                            TA0004row("NACPRODUCT2_6") = SQLdr("NACPRODUCT2_1")                     '品名２_6
                                            TA0004row("NACPRODUCT2NAME_6") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_6
                                            TA0004row("NACTANI6") = SQLdr("NACTANI1")                               '受注・単位6
                                            TA0004row("NACTANINAME6") = SQLdr("NACTANINAME1")                       '受注・単位6名称
                                            TA0004row("NACSTANI6") = SQLdr("NACSTANI1")                             '実績・配送単位6
                                            TA0004row("NACSTANINAME6") = SQLdr("NACSTANINAME1")                     '実績・配送単位6名称
                                            TA0004row("NACSURYO6") = wDET_NACSURYO.ToString("#0.000")               '受注・数量6
                                            TA0004row("NACJSURYO6") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量6
                                        Case 7
                                            TA0004row("NACOILTYPE_7") = SQLdr("NACOILTYPE_1")                       '油種_7
                                            TA0004row("NACOILTYPENAME_7") = SQLdr("NACOILTYPENAME_1")               '油種名称_7
                                            TA0004row("NACPRODUCT1_7") = SQLdr("NACPRODUCT1_1")                     '品名１_7
                                            TA0004row("NACPRODUCT1NAME_7") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_7
                                            TA0004row("NACPRODUCT2_7") = SQLdr("NACPRODUCT2_1")                     '品名２_7
                                            TA0004row("NACPRODUCT2NAME_7") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_7
                                            TA0004row("NACTANI7") = SQLdr("NACTANI1")                               '受注・単位7
                                            TA0004row("NACTANINAME7") = SQLdr("NACTANINAME1")                       '受注・単位7名称
                                            TA0004row("NACSTANI7") = SQLdr("NACSTANI1")                             '実績・配送単位7
                                            TA0004row("NACSTANINAME7") = SQLdr("NACSTANINAME1")                     '実績・配送単位7名称
                                            TA0004row("NACSURYO7") = wDET_NACSURYO.ToString("#0.000")               '受注・数量7
                                            TA0004row("NACJSURYO7") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量7
                                        Case 8
                                            TA0004row("NACOILTYPE_8") = SQLdr("NACOILTYPE_1")                       '油種_8
                                            TA0004row("NACOILTYPENAME_8") = SQLdr("NACOILTYPENAME_1")               '油種名称_8
                                            TA0004row("NACPRODUCT1_8") = SQLdr("NACPRODUCT1_1")                     '品名１_8
                                            TA0004row("NACPRODUCT1NAME_8") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_8
                                            TA0004row("NACPRODUCT2_8") = SQLdr("NACPRODUCT2_1")                     '品名２_8
                                            TA0004row("NACPRODUCT2NAME_8") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_8
                                            TA0004row("NACTANI8") = SQLdr("NACTANI1")                               '受注・単位8
                                            TA0004row("NACTANINAME8") = SQLdr("NACTANINAME1")                       '受注・単位8名称
                                            TA0004row("NACSTANI8") = SQLdr("NACSTANI1")                             '実績・配送単位8
                                            TA0004row("NACSTANINAME8") = SQLdr("NACSTANINAME1")                     '実績・配送単位8名称
                                            TA0004row("NACSURYO8") = wDET_NACSURYO.ToString("#0.000")               '受注・数量8
                                            TA0004row("NACJSURYO8") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量8
                                        Case Else
                                            TA0004row("NACOILTYPE_8") = SQLdr("NACOILTYPE_1")                       '油種_8
                                            TA0004row("NACOILTYPENAME_8") = SQLdr("NACOILTYPENAME_1")               '油種名称_8
                                            TA0004row("NACPRODUCT1_8") = SQLdr("NACPRODUCT1_1")                     '品名１_8
                                            TA0004row("NACPRODUCT1NAME_8") = SQLdr("NACPRODUCT1NAME_1")             '品名１名称_8
                                            TA0004row("NACPRODUCT2_8") = SQLdr("NACPRODUCT2_1")                     '品名２_8
                                            TA0004row("NACPRODUCT2NAME_8") = SQLdr("NACPRODUCT2NAME_1")             '品名２名称_8
                                            TA0004row("NACTANI8") = SQLdr("NACTANI1")                               '受注・単位8
                                            TA0004row("NACTANINAME8") = SQLdr("NACTANINAME1")                       '受注・単位8名称
                                            TA0004row("NACSTANI8") = SQLdr("NACSTANI1")                             '実績・配送単位8
                                            TA0004row("NACSTANINAME8") = SQLdr("NACSTANINAME1")                     '実績・配送単位8名称
                                            TA0004row("NACSURYO8") = wDET_NACSURYO.ToString("#0.000")               '受注・数量8
                                            TA0004row("NACJSURYO8") = wDET_NACJSURYO.ToString("#0.000")             '実績・配送数量8
                                    End Select

                                End If

                                If SQLdr("ACACHANTEI") = "HSD" OrElse
                                   SQLdr("ACACHANTEI") = "HSC" Then
                                    If IsDate(SQLdr("NACHAISTDATE")) Then
                                        wDATETime = SQLdr("NACHAISTDATE")
                                        wSUM_NACHAISTDATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_NACHAISTDATE = C_DEFAULT_YMD
                                    End If

                                    If IsDate(SQLdr("NACHAIENDDATE")) Then
                                        wDATETime = SQLdr("NACHAIENDDATE")
                                        wSUM_NACHAIENDDATE = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        wSUM_NACHAIENDDATE = C_DEFAULT_YMD
                                    End If
                                End If

                                '回送の場合
                                If {"KJD", "KLD", "KSD", "KJC", "KLC", "KSC"}.Contains(SQLdr("ACACHANTEI")) Then
                                    TA0004row("NACTRIPNO") = ""                                                           'トリップ
                                    TA0004row("NACDROPNO") = ""                                                           'ドロップ
                                    TA0004row("NACSEQ") = ""                                                              'SEQ
                                    TA0004row("KEYTRIPNO") = ""                                                           'SYSトリップ
                                    TA0004row("KEYDROPNO") = ""                                                           'SYSドロップ
                                End If

                                '〇集計

                                wDBL = Val(SQLdr("NACHAIDISTANCE"))
                                wSUM_NACHAIDISTANCE = wSUM_NACHAIDISTANCE + wDBL                                      '実績・配送距離

                                wDBL = Val(SQLdr("NACKAIDISTANCE"))
                                wSUM_NACKAIDISTANCE = wSUM_NACKAIDISTANCE + wDBL                                      '実績・下車作業距離

                                wDBL = 0
                                wSUM_NACCHODISTANCE = wSUM_NACCHODISTANCE + wDBL                                      '実績・勤怠調整距離(2017/11/5)

                                wDBL = Val(SQLdr("NACTTLDISTANCE"))
                                wSUM_NACTTLDISTANCE = wSUM_NACTTLDISTANCE + wDBL                                      '実績・配送距離合計Σ

                                wINT = Val(SQLdr("NACHAIWORKTIME"))
                                wSUM_NACHAIWORKTIME = wSUM_NACHAIWORKTIME + wINT                                      '実績・配送作業時間

                                wINT = Val(SQLdr("NACGESWORKTIME"))
                                wSUM_NACGESWORKTIME = wSUM_NACGESWORKTIME + wINT                                      '実績・下車作業時間

                                wINT = 0
                                wSUM_NACCHOWORKTIME = wSUM_NACCHOWORKTIME + wINT                                      '実績・勤怠調整時間(2017/11/5)

                                wINT = Val(SQLdr("NACTTLWORKTIME"))
                                wSUM_NACTTLWORKTIME = wSUM_NACTTLWORKTIME + wINT                                      '実績・配送合計時間Σ

                                wINT = Val(SQLdr("NACOUTWORKTIME"))
                                wSUM_NACOUTWORKTIME = wSUM_NACOUTWORKTIME + wINT                                      '実績・就業外時間

                                wINT = Val(SQLdr("NACBREAKTIME"))
                                wSUM_NACBREAKTIME = wSUM_NACBREAKTIME + wINT                                          '実績・休憩時間

                                wINT = 0
                                wSUM_NACCHOBREAKTIME = wSUM_NACCHOBREAKTIME + wINT                                    '実績・休憩調整時間(2017/11/5)

                                wINT = Val(SQLdr("NACTTLBREAKTIME"))
                                wSUM_NACTTLBREAKTIME = wSUM_NACTTLBREAKTIME + wINT                                    '実績・休憩合計時間Σ

                                wINT = Val(SQLdr("NACCASH"))
                                wSUM_NACCASH = wSUM_NACCASH + wINT                                                    '実績・現金

                                wINT = Val(SQLdr("NACETC"))
                                wSUM_NACETC = wSUM_NACETC + wINT                                                      '実績・ETC

                                wINT = Val(SQLdr("NACTICKET"))
                                wSUM_NACTICKET = wSUM_NACTICKET + wINT                                                '実績・回数券

                                wINT = Val(SQLdr("NACKYUYU"))
                                wSUM_NACKYUYU = wSUM_NACKYUYU + wINT                                                  '実績・軽油

                                wINT = Val(SQLdr("NACUNLOADCNT"))
                                wSUM_NACUNLOADCNT = wSUM_NACUNLOADCNT + wINT                                          '実績・荷卸回数

                                wINT = 0
                                wSUM_NACCHOUNLOADCNT = wSUM_NACCHOUNLOADCNT + wINT                                    '実績・荷卸回数調整(2017/11/5)

                                wINT = Val(SQLdr("NACTTLUNLOADCNT"))
                                wSUM_NACTTLUNLOADCNT = wSUM_NACTTLUNLOADCNT + wINT                                    '実績・荷卸回数合計Σ

                                wINT = Val(SQLdr("NACKAIJI"))
                                wSUM_NACKAIJI = wSUM_NACKAIJI + wINT                                                 '実績・回次

                                wINT = Val(SQLdr("NACJITIME"))
                                wSUM_NACJITIME = wSUM_NACJITIME + wINT                                                '実績・実車時間

                                wINT = 0
                                wSUM_NACJICHOSTIME = wSUM_NACJICHOSTIME + wINT                                        '実績・実車時間調整(2017/11/5)

                                wINT = Val(SQLdr("NACJITTLETIME"))
                                wSUM_NACJITTLETIME = wSUM_NACJITTLETIME + wINT                                        '実績・実車時間合計Σ
                                wSUM_NACJKTTLTIME = wSUM_NACJKTTLTIME + wINT                                          '実績・実車/空車時間合計Σ

                                wINT = Val(SQLdr("NACKUTIME"))
                                wSUM_NACKUTIME = wSUM_NACKUTIME + wINT                                                '実績・空車時間

                                wINT = 0
                                wSUM_NACKUCHOTIME = wSUM_NACKUCHOTIME + wINT                                          '実績・空車時間調整(2017/11/5)

                                wINT = Val(SQLdr("NACKUTTLTIME"))
                                wSUM_NACKUTTLTIME = wSUM_NACKUTTLTIME + wINT                                          '実績・空車時間合計Σ
                                wSUM_NACJKTTLTIME = wSUM_NACJKTTLTIME + wINT                                          '実績・実車/空車時間合計Σ

                                wDBL = Val(SQLdr("NACJIDISTANCE"))
                                wSUM_NACJIDISTANCE = wSUM_NACJIDISTANCE + wDBL                                        '実績・実車距離

                                wDBL = 0
                                wSUM_NACJICHODISTANCE = wSUM_NACJICHODISTANCE + wDBL                                  '実績・実車距離調整(2017/11/5)

                                wDBL = Val(SQLdr("NACJITTLDISTANCE"))
                                wSUM_NACJITTLDISTANCE = wSUM_NACJITTLDISTANCE + wDBL                                  '実績・実車距離合計Σ
                                wSUM_NACJKTTLDISTANCE = wSUM_NACJKTTLDISTANCE + wDBL                                  '実績・実車/空車距離合計Σ

                                wDBL = Val(SQLdr("NACKUDISTANCE"))
                                wSUM_NACKUDISTANCE = wSUM_NACKUDISTANCE + wDBL                                        '実績・空車距離

                                wDBL = 0
                                wSUM_NACKUCHODISTANCE = wSUM_NACKUCHODISTANCE + wDBL                                  '実績・空車距離調整(2017/11/5)

                                wDBL = Val(SQLdr("NACKUTTLDISTANCE"))
                                wSUM_NACKUTTLDISTANCE = wSUM_NACKUTTLDISTANCE + wDBL                                  '実績・空車距離合計Σ
                                wSUM_NACJKTTLDISTANCE = wSUM_NACJKTTLDISTANCE + wDBL                                  '実績・実車/空車距離合計Σ

                                wINT = 0
                                wSUM_NACTARIFFFARE = wSUM_NACTARIFFFARE + wINT                                        '実績・運賃タリフ額(2017/11/5)

                                wINT = 0
                                wSUM_NACFIXEDFARE = wSUM_NACFIXEDFARE + wINT                                          '実績・運賃固定額(2017/11/5)

                                wINT = 0
                                wSUM_NACINCHOFARE = wSUM_NACINCHOFARE + wINT                                          '実績・運賃手入力調整額(2017/11/5)

                                wINT = 0
                                wSUM_NACTTLFARE = wSUM_NACTTLFARE + wINT                                              '実績・運賃合計額Σ(2017/11/5)

                                wINT = 0
                                wSUM_NACOFFICETIME = wSUM_NACOFFICETIME + wINT                                        '実績・従業時間(2017/11/5)

                                wINT = 0
                                wSUM_NACOFFICEBREAKTIME = wSUM_NACOFFICEBREAKTIME + wINT                              '実績・従業休憩時間(2017/11/5)

                            End While

                            '〇最終レコード出力

                            If Not (WW_NACSHUKODATE = "" AndAlso
                               WW_NACSHUKADATE = "" AndAlso
                               WW_NACTODOKEDATE = "" AndAlso
                               WW_NACTORICODE = "" AndAlso
                               WW_NACSHIPORG = "" AndAlso
                               WW_KEYGSHABAN = "" AndAlso
                               WW_KEYSTAFFCODE = "" AndAlso
                               WW_KEYTRIPNO = "" AndAlso
                               WW_KEYDROPNO = "" AndAlso
                               WW_ACACHANTEI = "") Then
                                '合計値セット
                                TA0004row("NACHAIDISTANCE") = wSUM_NACHAIDISTANCE.ToString("#0.00")                 '実績・配送距離
                                TA0004row("NACKAIDISTANCE") = wSUM_NACKAIDISTANCE.ToString("#0.00")                 '実績・下車作業距離
                                TA0004row("NACCHODISTANCE") = wSUM_NACCHODISTANCE.ToString("#0.00")                 '実績・勤怠調整距離
                                TA0004row("NACTTLDISTANCE") = wSUM_NACTTLDISTANCE.ToString("#0.00")                 '実績・配送距離合計Σ
                                TA0004row("NACHAISTDATE") = wSUM_NACHAISTDATE                                       '実績・配送作業開始日時
                                TA0004row("NACHAIENDDATE") = wSUM_NACHAIENDDATE                                     '実績・配送作業終了日時
                                TA0004row("NACHAIWORKTIME") = wSUM_NACHAIWORKTIME                                   '実績・配送作業時間
                                TA0004row("NACGESWORKTIME") = wSUM_NACGESWORKTIME                                   '実績・下車作業時間
                                TA0004row("NACCHOWORKTIME") = wSUM_NACCHOWORKTIME                                   '実績・勤怠調整時間
                                TA0004row("NACTTLWORKTIME") = wSUM_NACTTLWORKTIME                                   '実績・配送合計時間Σ
                                TA0004row("NACOUTWORKTIME") = wSUM_NACOUTWORKTIME                                   '実績・就業外時間
                                TA0004row("NACBREAKTIME") = wSUM_NACBREAKTIME                                       '実績・休憩時間
                                TA0004row("NACCHOBREAKTIME") = wSUM_NACCHOBREAKTIME                                 '実績・休憩調整時間
                                TA0004row("NACTTLBREAKTIME") = wSUM_NACTTLBREAKTIME                                 '実績・休憩合計時間Σ
                                TA0004row("NACCASH") = wSUM_NACCASH                                                 '実績・現金
                                TA0004row("NACETC") = wSUM_NACETC                                                   '実績・ETC
                                TA0004row("NACTICKET") = wSUM_NACTICKET                                             '実績・回数券
                                TA0004row("NACKYUYU") = wSUM_NACKYUYU.ToString("#0.00")                             '実績・軽油
                                TA0004row("NACUNLOADCNT") = wSUM_NACUNLOADCNT                                       '実績・荷卸回数
                                TA0004row("NACCHOUNLOADCNT") = wSUM_NACCHOUNLOADCNT                                 '実績・荷卸回数調整
                                TA0004row("NACTTLUNLOADCNT") = wSUM_NACTTLUNLOADCNT                                 '実績・荷卸回数合計Σ
                                TA0004row("NACKAIJI") = wSUM_NACKAIJI                                               '実績・回次
                                TA0004row("NACJITIME") = wSUM_NACJITIME                                             '実績・実車時間
                                TA0004row("NACJICHOSTIME") = wSUM_NACJICHOSTIME                                     '実績・実車時間調整
                                TA0004row("NACJITTLETIME") = wSUM_NACJITTLETIME                                     '実績・実車時間合計Σ
                                TA0004row("NACKUTIME") = wSUM_NACKUTIME                                             '実績・空車時間
                                TA0004row("NACKUCHOTIME") = wSUM_NACKUCHOTIME                                       '実績・空車時間調整
                                TA0004row("NACKUTTLTIME") = wSUM_NACKUTTLTIME                                       '実績・空車時間合計Σ
                                TA0004row("NACJKTTLTIME") = wSUM_NACJKTTLTIME                                       '実績・実車/空車時間合計Σ
                                TA0004row("NACJIDISTANCE") = wSUM_NACJIDISTANCE.ToString("#0.00")                   '実績・実車距離
                                TA0004row("NACJICHODISTANCE") = wSUM_NACJICHODISTANCE.ToString("#0.00")             '実績・実車距離調整
                                TA0004row("NACJITTLDISTANCE") = wSUM_NACJITTLDISTANCE.ToString("#0.00")             '実績・実車距離合計Σ
                                TA0004row("NACKUDISTANCE") = wSUM_NACKUDISTANCE.ToString("#0.00")                   '実績・空車距離
                                TA0004row("NACKUCHODISTANCE") = wSUM_NACKUCHODISTANCE.ToString("#0.00")             '実績・空車距離調整
                                TA0004row("NACKUTTLDISTANCE") = wSUM_NACKUTTLDISTANCE.ToString("#0.00")             '実績・空車距離合計Σ
                                TA0004row("NACJKTTLDISTANCE") = wSUM_NACJKTTLDISTANCE.ToString("#0.00")             '実績・実車/空車距離合計Σ
                                TA0004row("NACTARIFFFARE") = wSUM_NACTARIFFFARE                                     '実績・運賃タリフ額
                                TA0004row("NACFIXEDFARE") = wSUM_NACFIXEDFARE                                       '実績・運賃固定額
                                TA0004row("NACINCHOFARE") = wSUM_NACINCHOFARE                                       '実績・運賃手入力調整額
                                TA0004row("NACTTLFARE") = wSUM_NACTTLFARE                                           '実績・運賃合計額Σ
                                TA0004row("NACOFFICETIME") = wSUM_NACOFFICETIME                                     '実績・従業時間
                                TA0004row("NACOFFICEBREAKTIME") = wSUM_NACOFFICEBREAKTIME                           '実績・従業休憩時間
                                TA0004row("NACSURYOG") = wSUM_NACSURYOG.ToString("#0.000")                          '受注・数量合計
                                TA0004row("NACJSURYOG") = wSUM_NACJSURYOG.ToString("#0.000")                        '実績・配送数量合計

                                TA0004tbl.Rows.Add(TA0004row)

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
    ''' <remarks></remarks>
    Private Sub GetTA0004tbl2()

        '抽出条件(サーバー部署)List作成
        Dim W_ORGlst As List(Of String) = getORGList()

        '抽出条件(サーバー部署)List毎にデータ抽出
        Dim WW_MMCNT As Integer = DateDiff("m", work.WF_SEL_STYMD.Text, work.WF_SEL_ENDYMD.Text)
        Dim WW_STYMD As String = work.WF_SEL_STYMD.Text
        Dim WW_ENDYMD As String = work.WF_SEL_ENDYMD.Text
        Dim dt As Date = CDate(work.WF_SEL_STYMD.Text)


        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As New StringBuilder(30000)
            SQLStr.AppendLine(" SELECT                                                                  ")
            SQLStr.AppendLine("  isnull(rtrim(L03.CAMPCODE),'') as CAMPCODE                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.CAMPNAME),'') as CAMPNAME                             ")
            SQLStr.AppendLine(" ,'' as MOTOCHO                                                          ")
            SQLStr.AppendLine(" ,'' as MOTOCHONAME                                                      ")
            SQLStr.AppendLine(" ,'' as VERSION                                                          ")
            SQLStr.AppendLine(" ,'' as DENTYPE                                                          ")
            SQLStr.AppendLine(" ,'' as TENKI                                                            ")
            SQLStr.AppendLine(" ,'' as TENKINAME                                                        ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEIJOYMD),'') as KEIJOYMD                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.DENYMD),'') as DENYMD                                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.DENNO),'') as DENNO                                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KANRENDENNO),'') as KANRENDENNO                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.DTLNO),'') as DTLNO                                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.INQKBN),'') as INQKBN                                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.INQKBNNAME),'') as INQKBNNAME                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.ACACHANTEI),'') as ACACHANTEI                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.ACACHANTEINAME),'') as ACACHANTEINAME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHUKODATE),'') as NACSHUKODATE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHUKADATE),'') as NACSHUKADATE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTODOKEDATE),'') as NACTODOKEDATE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKEIJODATE),'') as NACKEIJODATE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORICODE),'') as NACTORICODE                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORICODENAME),'') as NACTORICODENAME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACURIKBN),'') as NACURIKBN                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACURIKBNNAME),'') as NACURIKBNNAME                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTODOKECODE),'') as NACTODOKECODE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTODOKECODENAME),'') as NACTODOKECODENAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTORICODE),'') as NACSTORICODE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTORICODENAME),'') as NACSTORICODENAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHUKABASHO),'') as NACSHUKABASHO                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHUKABASHONAME),'') as NACSHUKABASHONAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE01),'') as NACTORITYPE01                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE01NAME),'') as NACTORITYPE01NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE02),'') as NACTORITYPE02                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE02NAME),'') as NACTORITYPE02NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE03),'') as NACTORITYPE03                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE03NAME),'') as NACTORITYPE03NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE04),'') as NACTORITYPE04                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE04NAME),'') as NACTORITYPE04NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE05),'') as NACTORITYPE05                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTORITYPE05NAME),'') as NACTORITYPE05NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_1),'') as NACOILTYPE_1                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_1),'') as NACOILTYPENAME_1             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_2),'') as NACOILTYPE_2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_2),'') as NACOILTYPENAME_2             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_3),'') as NACOILTYPE_3                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_3),'') as NACOILTYPENAME_3             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_4),'') as NACOILTYPE_4                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_4),'') as NACOILTYPENAME_4             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_5),'') as NACOILTYPE_5                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_5),'') as NACOILTYPENAME_5             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_6),'') as NACOILTYPE_6                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_6),'') as NACOILTYPENAME_6             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_7),'') as NACOILTYPE_7                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_7),'') as NACOILTYPENAME_7             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPE_8),'') as NACOILTYPE_8                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOILTYPENAME_8),'') as NACOILTYPENAME_8             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_1),'') as NACPRODUCT1_1                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_1),'') as NACPRODUCT1NAME_1           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_2),'') as NACPRODUCT1_2                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_2),'') as NACPRODUCT1NAME_2           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_3),'') as NACPRODUCT1_3                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_3),'') as NACPRODUCT1NAME_3           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_4),'') as NACPRODUCT1_4                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_4),'') as NACPRODUCT1NAME_4           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_5),'') as NACPRODUCT1_5                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_5),'') as NACPRODUCT1NAME_5           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_6),'') as NACPRODUCT1_6                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_6),'') as NACPRODUCT1NAME_6           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_7),'') as NACPRODUCT1_7                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_7),'') as NACPRODUCT1NAME_7           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1_8),'') as NACPRODUCT1_8                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT1NAME_8),'') as NACPRODUCT1NAME_8           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_1),'') as NACPRODUCT2_1                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_1),'') as NACPRODUCT2NAME_1           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_2),'') as NACPRODUCT2_2                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_2),'') as NACPRODUCT2NAME_2           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_3),'') as NACPRODUCT2_3                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_3),'') as NACPRODUCT2NAME_3           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_4),'') as NACPRODUCT2_4                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_4),'') as NACPRODUCT2NAME_4           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_5),'') as NACPRODUCT2_5                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_5),'') as NACPRODUCT2NAME_5           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_6),'') as NACPRODUCT2_6                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_6),'') as NACPRODUCT2NAME_6           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_7),'') as NACPRODUCT2_7                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_7),'') as NACPRODUCT2NAME_7           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2_8),'') as NACPRODUCT2_8                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACPRODUCT2NAME_8),'') as NACPRODUCT2NAME_8           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACGSHABAN),'') as NACGSHABAN                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSUPPLIERKBN),'') as NACSUPPLIERKBN                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSUPPLIERKBNNAME),'') as NACSUPPLIERKBNNAME         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSUPPLIER),'') as NACSUPPLIER                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSUPPLIERNAME),'') as NACSUPPLIERNAME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOOILTYPE),'') as NACSHARYOOILTYPE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOOILTYPENAME),'') as NACSHARYOOILTYPENAME     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE1),'') as NACSHARYOTYPE1                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE1NAME),'') as NACSHARYOTYPE1NAME         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTSHABAN1),'') as NACTSHABAN1                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG1),'') as NACMANGMORG1                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG1NAME),'') as NACMANGMORG1NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG1),'') as NACMANGSORG1                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG1NAME),'') as NACMANGSORG1NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG1),'') as NACMANGUORG1                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG1NAME),'') as NACMANGUORG1NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE1),'') as NACBASELEASE1                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE1NAME),'') as NACBASELEASE1NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACLICNPLTNOF1),'') as NACLICNPLTNOF1                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE2),'') as NACSHARYOTYPE2                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE2NAME),'') as NACSHARYOTYPE2NAME         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTSHABAN2),'') as NACTSHABAN2                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG2),'') as NACMANGMORG2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG2NAME),'') as NACMANGMORG2NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG2),'') as NACMANGSORG2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG2NAME),'') as NACMANGSORG2NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG2),'') as NACMANGUORG2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG2NAME),'') as NACMANGUORG2NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE2),'') as NACBASELEASE2                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE2NAME),'') as NACBASELEASE2NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACLICNPLTNOF2),'') as NACLICNPLTNOF2                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE3),'') as NACSHARYOTYPE3                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHARYOTYPE3NAME),'') as NACSHARYOTYPE3NAME         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTSHABAN3),'') as NACTSHABAN3                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG3),'') as NACMANGMORG3                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGMORG3NAME),'') as NACMANGMORG3NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG3),'') as NACMANGSORG3                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGSORG3NAME),'') as NACMANGSORG3NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG3),'') as NACMANGUORG3                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMANGUORG3NAME),'') as NACMANGUORG3NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE3),'') as NACBASELEASE3                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBASELEASE3NAME),'') as NACBASELEASE3NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACLICNPLTNOF3),'') as NACLICNPLTNOF3                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCREWKBN),'') as NACCREWKBN                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCREWKBNNAME),'') as NACCREWKBNNAME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFCODE),'') as NACSTAFFCODE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFCODENAME),'') as NACSTAFFCODENAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFKBN),'') as NACSTAFFKBN                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFKBNNAME),'') as NACSTAFFKBNNAME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMORG),'') as NACMORG                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMORGNAME),'') as NACMORGNAME                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHORG),'') as NACHORG                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHORGNAME),'') as NACHORGNAME                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSORG),'') as NACSORG                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSORGNAME),'') as NACSORGNAME                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFCODE2),'') as NACSTAFFCODE2                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFCODE2NAME),'') as NACSTAFFCODE2NAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFKBN2),'') as NACSTAFFKBN2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTAFFKBN2NAME),'') as NACSTAFFKBN2NAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMORG2),'') as NACMORG2                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACMORG2NAME),'') as NACMORG2NAME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHORG2),'') as NACHORG2                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHORG2NAME),'') as NACHORG2NAME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSORG2),'') as NACSORG2                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSORG2NAME),'') as NACSORG2NAME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACORDERNO),'') as NACORDERNO                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACDETAILNO),'') as NACDETAILNO                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTRIPNO),'') as NACTRIPNO                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACDROPNO),'') as NACDROPNO                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSEQ),'') as NACSEQ                                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACORDERORG),'') as NACORDERORG                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACORDERORGNAME),'') as NACORDERORGNAME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHIPORG),'') as NACSHIPORG                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSHIPORGNAME),'') as NACSHIPORGNAME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO1),'') as NACSURYO1                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI1),'') as NACTANI1                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME1),'') as NACTANINAME1                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO2),'') as NACSURYO2                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI2),'') as NACTANI2                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME2),'') as NACTANINAME2                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO3),'') as NACSURYO3                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI3),'') as NACTANI3                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME3),'') as NACTANINAME3                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO4),'') as NACSURYO4                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI4),'') as NACTANI4                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME4),'') as NACTANINAME4                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO5),'') as NACSURYO5                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI5),'') as NACTANI5                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME5),'') as NACTANINAME5                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO6),'') as NACSURYO6                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI6),'') as NACTANI6                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME6),'') as NACTANINAME6                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO7),'') as NACSURYO7                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI7),'') as NACTANI7                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME7),'') as NACTANINAME7                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYO8),'') as NACSURYO8                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANI8),'') as NACTANI8                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTANINAME8),'') as NACTANINAME8                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSURYOG),'') as NACSURYOG                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO1),'') as NACJSURYO1                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI1),'') as NACSTANI1                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME1),'') as NACSTANINAME1                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO2),'') as NACJSURYO2                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI2),'') as NACSTANI2                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME2),'') as NACSTANINAME2                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO3),'') as NACJSURYO3                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI3),'') as NACSTANI3                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME3),'') as NACSTANINAME3                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO4),'') as NACJSURYO4                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI4),'') as NACSTANI4                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME4),'') as NACSTANINAME4                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO5),'') as NACJSURYO5                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI5),'') as NACSTANI5                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME5),'') as NACSTANINAME5                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO6),'') as NACJSURYO6                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI6),'') as NACSTANI6                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME6),'') as NACSTANINAME6                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO7),'') as NACJSURYO7                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI7),'') as NACSTANI7                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME7),'') as NACSTANINAME7                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYO8),'') as NACJSURYO8                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANI8),'') as NACSTANI8                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACSTANINAME8),'') as NACSTANINAME8                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJSURYOG),'') as NACJSURYOG                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHAIDISTANCE),'') as NACHAIDISTANCE                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKAIDISTANCE),'') as NACKAIDISTANCE                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCHODISTANCE),'') as NACCHODISTANCE                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTTLDISTANCE),'') as NACTTLDISTANCE                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHAISTDATE),'') as NACHAISTDATE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHAIENDDATE),'') as NACHAIENDDATE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACHAIWORKTIME),'') as NACHAIWORKTIME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACGESSTDATE),'') as NACGESSTDATE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACGESENDDATE),'') as NACGESENDDATE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACGESWORKTIME),'') as NACGESWORKTIME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCHOWORKTIME),'') as NACCHOWORKTIME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTTLWORKTIME),'') as NACTTLWORKTIME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOUTWORKTIME),'') as NACOUTWORKTIME                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBREAKSTDATE),'') as NACBREAKSTDATE                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBREAKENDDATE),'') as NACBREAKENDDATE               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACBREAKTIME),'') as NACBREAKTIME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCHOBREAKTIME),'') as NACCHOBREAKTIME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTTLBREAKTIME),'') as NACTTLBREAKTIME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCASH),'') as NACCASH                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACETC),'') as NACETC                                 ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTICKET),'') as NACTICKET                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKYUYU),'') as NACKYUYU                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACUNLOADCNT),'') as NACUNLOADCNT                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACCHOUNLOADCNT),'') as NACCHOUNLOADCNT               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTTLUNLOADCNT),'') as NACTTLUNLOADCNT               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKAIJI),'') as NACKAIJI                             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJITIME),'') as NACJITIME                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJICHOSTIME),'') as NACJICHOSTIME                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJITTLETIME),'') as NACJITTLETIME                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUTIME),'') as NACKUTIME                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUCHOTIME),'') as NACKUCHOTIME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUTTLTIME),'') as NACKUTTLTIME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJKTTLTIME),'') as NACJKTTLTIME                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJIDISTANCE),'') as NACJIDISTANCE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJICHODISTANCE),'') as NACJICHODISTANCE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJITTLDISTANCE),'') as NACJITTLDISTANCE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUDISTANCE),'') as NACKUDISTANCE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUCHODISTANCE),'') as NACKUCHODISTANCE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACKUTTLDISTANCE),'') as NACKUTTLDISTANCE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACJKTTLDISTANCE),'') as NACJKTTLDISTANCE             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTARIFFFARE),'') as NACTARIFFFARE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACFIXEDFARE),'') as NACFIXEDFARE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACINCHOFARE),'') as NACINCHOFARE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACTTLFARE),'') as NACTTLFARE                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOFFICESORG),'') as NACOFFICESORG                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOFFICESORGNAME),'') as NACOFFICESORGNAME           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOFFICETIME),'') as NACOFFICETIME                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.NACOFFICEBREAKTIME),'') as NACOFFICEBREAKTIME         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYSHUSHADATE),'') as PAYSHUSHADATE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYTAISHADATE),'') as PAYTAISHADATE                   ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYSTAFFKBN),'') as PAYSTAFFKBN                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYSTAFFKBNNAME),'') as PAYSTAFFKBNNAME               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYSTAFFCODE),'') as PAYSTAFFCODE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYSTAFFCODENAME),'') as PAYSTAFFCODENAME             ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYMORG),'') as PAYMORG                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYMORGNAME),'') as PAYMORGNAME                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYHORG),'') as PAYHORG                               ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.PAYHORGNAME),'') as PAYHORGNAME                       ")
            SQLStr.AppendLine(" ,'' as WORKKBN                                                          ")
            SQLStr.AppendLine(" ,'' as WORKKBNNAME                                                      ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYSTAFFCODE),'') as KEYSTAFFCODE                     ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYGSHABAN),'') as KEYGSHABAN                         ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYTRIPNO),'') as KEYTRIPNO                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYDROPNO),'') as KEYDROPNO                           ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYTSHABAN1),'') as KEYTSHABAN1                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYTSHABAN2),'') as KEYTSHABAN2                       ")
            SQLStr.AppendLine(" ,isnull(rtrim(L03.KEYTSHABAN3),'') as KEYTSHABAN3                       ")
            SQLStr.AppendLine(" FROM L0003_SUMMARYN L03                                                 ")
            SQLStr.AppendLine(" WHERE                                                                   ")
            SQLStr.AppendLine("        L03.CAMPCODE = @P02                                              ")
            SQLStr.AppendLine("    and L03.INQKBN = '1'                                                 ")
            SQLStr.AppendLine("    and L03.NACSHUKODATE <= @P05                                         ")
            SQLStr.AppendLine("    and L03.NACSHUKODATE >= @P06                                         ")
            SQLStr.AppendLine("    and L03.NACSHUKADATE <= @P07                                         ")
            SQLStr.AppendLine("    and L03.NACSHUKADATE >= @P08                                         ")
            SQLStr.AppendLine("    and L03.NACTODOKEDATE <= @P09                                        ")
            SQLStr.AppendLine("    and L03.NACTODOKEDATE >= @P10                                        ")
            SQLStr.AppendLine("    and L03.KEIJOYMD <= @P11                                             ")
            SQLStr.AppendLine("    and L03.KEIJOYMD >= @P12                                             ")
            SQLStr.AppendLine("    and L03.NACSORG   = @P14                                             ")
            SQLStr.AppendLine("    and (L03.NACCREWKBN = '1' or L03.NACCREWKBN = @P13)                  ")
            SQLStr.AppendLine("    and L03.DELFLG <> '1'                                                ")
            SQLStr.AppendLine("ORDER BY                                                                 ")
            SQLStr.AppendLine(" L03.NACSHUKODATE, L03.NACSHUKADATE, L03.NACTODOKEDATE, L03.NACTORICODE, L03.NACSHIPORG, L03.KEYGSHABAN, L03.NACCREWKBN, L03.KEYSTAFFCODE, L03.KEYTRIPNO, L03.KEYDROPNO, L03.ACACHANTEI DESC, L03.NACSEQ ")

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr.ToString, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Date)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.Date)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.Date)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 20)

                For Each WI_ORG As String In W_ORGlst

                    '部署変換
                    Dim WW_ORG As String = ""
                    ConvORGCode(WI_ORG, WW_ORG, WW_ERRCODE)
                    If Not isNormal(WW_ERRCODE) Then
                        Exit Sub
                    End If

                    '抽出範囲決定（締まっている範囲を求める）
                    For i As Integer = 0 To WW_MMCNT
                        Dim WW_DATE As String = dt.AddMonths(i).ToString("yyyy/MM")

                        '勤怠締テーブル取得
                        Dim WW_LIMITFLG As String = "0"
                        Dim WW_ERR_RTN As String = C_MESSAGE_NO.NORMAL
                        T0007COM.T00008get(work.WF_SEL_CAMPCODE.Text,
                                           WW_ORG,
                                           WW_DATE,
                                           WW_LIMITFLG,
                                           WW_ERR_RTN)
                        If Not isNormal(WW_ERR_RTN) Then
                            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
                            Exit Sub
                        End If

                        If WW_LIMITFLG = "0" Then
                            WW_STYMD = C_DEFAULT_YMD
                            WW_ENDYMD = C_DEFAULT_YMD
                            Continue For
                        End If

                        '締まっていたらサマリーテーブルから取得する
                        If WW_LIMITFLG = "1" Then
                            WW_STYMD = work.WF_SEL_STYMD.Text
                            If WW_DATE = CDate(work.WF_SEL_ENDYMD.Text).ToString("yyyy/MM") Then
                                WW_ENDYMD = work.WF_SEL_ENDYMD.Text
                            Else
                                WW_ENDYMD = CDate(WW_DATE & "/01").AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                            End If
                        End If
                    Next

                    Try


                        PARA01.Value = Master.USERID
                        PARA02.Value = work.WF_SEL_CAMPCODE.Text
                        PARA03.Value = ""
                        PARA04.Value = Date.Now
                        PARA05.Value = C_MAX_YMD
                        PARA06.Value = C_DEFAULT_YMD
                        PARA07.Value = C_MAX_YMD
                        PARA08.Value = C_DEFAULT_YMD
                        PARA09.Value = C_MAX_YMD
                        PARA10.Value = C_DEFAULT_YMD
                        PARA11.Value = C_MAX_YMD
                        PARA12.Value = C_DEFAULT_YMD
                        Select Case work.WF_SEL_FUNC.Text
                            Case "0"    '含めない
                                PARA13.Value = "1"
                            Case Else   '含める
                                PARA13.Value = "2"
                        End Select
                        Select Case work.WF_SEL_FIELDSEL.Text
                            Case "1"    '出庫日
                                PARA05.Value = WW_ENDYMD
                                PARA06.Value = WW_STYMD
                            Case "2"    '出荷日
                                PARA07.Value = WW_ENDYMD
                                PARA08.Value = WW_STYMD
                            Case "3"    '届日
                                PARA09.Value = WW_ENDYMD
                                PARA10.Value = WW_STYMD
                            Case "4"    '計上日
                                PARA11.Value = WW_ENDYMD
                                PARA12.Value = WW_STYMD
                        End Select
                        PARA14.Value = WI_ORG

                        SQLcmd.CommandTimeout = 300
                        Using WW_TA0004WKtbl As DataTable = TA0004tbl.Clone

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                                WW_TA0004WKtbl.Load(SQLdr)

                            End Using

                            Dim wDATE As Date
                            For i As Integer = 0 To WW_TA0004WKtbl.Rows.Count - 1
                                Dim TA0004row As DataRow = TA0004tbl.NewRow
                                TA0004row.ItemArray = WW_TA0004WKtbl.Rows(i).ItemArray


                                '固定項目
                                TA0004row("LINECNT") = "0"                                                      'DBの固定フィールド(2017/11/5)
                                TA0004row("OPERATION") = ""                                                     'DBの固定フィールド(2017/11/5)
                                TA0004row("TIMSTP") = "0"                                                       'DBの固定フィールド(2017/11/5)
                                TA0004row("SELECT") = "0"                                                       'DBの固定フィールド
                                TA0004row("HIDDEN") = "0"                                                       'DBの固定フィールド(2017/11/5)

                                If IsDate(TA0004row("NACSHUKODATE")) AndAlso TA0004row("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = TA0004row("NACSHUKODATE")
                                    TA0004row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACSHUKODATE") = C_DEFAULT_YMD
                                End If

                                If IsDate(TA0004row("NACSHUKADATE")) AndAlso TA0004row("NACSHUKADATE") <> C_DEFAULT_YMD Then   '出荷日
                                    wDATE = TA0004row("NACSHUKADATE")
                                    TA0004row("NACSHUKADATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACSHUKADATE") = C_DEFAULT_YMD
                                End If

                                If IsDate(TA0004row("NACTODOKEDATE")) AndAlso TA0004row("NACTODOKEDATE") <> C_DEFAULT_YMD Then '届日
                                    wDATE = TA0004row("NACTODOKEDATE")
                                    TA0004row("NACTODOKEDATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACTODOKEDATE") = C_DEFAULT_YMD
                                End If


                                If IsDate(TA0004row("KEIJOYMD")) AndAlso TA0004row("KEIJOYMD") <> C_DEFAULT_YMD Then           '計上日付
                                    wDATE = TA0004row("KEIJOYMD")
                                    TA0004row("KEIJOYMD") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("KEIJOYMD") = C_DEFAULT_YMD
                                End If

                                If IsDate(TA0004row("DENYMD")) AndAlso TA0004row("DENYMD") <> C_DEFAULT_YMD Then               '伝票日付
                                    wDATE = TA0004row("DENYMD")
                                    TA0004row("DENYMD") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("DENYMD") = C_DEFAULT_YMD
                                End If


                                If IsDate(TA0004row("NACSHUKODATE")) AndAlso TA0004row("NACSHUKODATE") <> C_DEFAULT_YMD Then   '出庫日・作業日
                                    wDATE = TA0004row("NACSHUKODATE")
                                    TA0004row("NACSHUKODATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACSHUKODATE") = C_DEFAULT_YMD
                                End If

                                If IsDate(TA0004row("NACSHUKADATE")) AndAlso TA0004row("NACSHUKADATE") <> C_DEFAULT_YMD Then   '出荷日
                                    wDATE = TA0004row("NACSHUKADATE")
                                    TA0004row("NACSHUKADATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACSHUKADATE") = C_DEFAULT_YMD
                                End If

                                If IsDate(TA0004row("NACTODOKEDATE")) AndAlso TA0004row("NACTODOKEDATE") <> C_DEFAULT_YMD Then '届日
                                    wDATE = TA0004row("NACTODOKEDATE")
                                    TA0004row("NACTODOKEDATE") = wDATE.ToString("yyyy/MM/dd")
                                Else
                                    TA0004row("NACTODOKEDATE") = C_DEFAULT_YMD
                                End If
                                '

                                Dim wDATETime As DateTime
                                '実績・配送作業開始日時
                                If IsDate(TA0004row("NACHAISTDATE")) AndAlso TA0004row("NACHAISTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("NACHAISTDATE")
                                    TA0004row("NACHAISTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("NACHAISTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・配送作業終了日時
                                If IsDate(TA0004row("NACHAIENDDATE")) AndAlso TA0004row("NACHAIENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("NACHAIENDDATE")
                                    TA0004row("NACHAIENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("NACHAIENDDATE") = C_DEFAULT_YMD
                                End If

                                '実績・下車作業開始日時
                                If IsDate(TA0004row("NACGESSTDATE")) AndAlso TA0004row("NACGESSTDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("NACGESSTDATE")
                                    TA0004row("NACGESSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("NACGESSTDATE") = C_DEFAULT_YMD
                                End If

                                '実績・下車作業終了日時
                                If IsDate(TA0004row("NACGESENDDATE")) AndAlso TA0004row("NACGESENDDATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("NACGESENDDATE")
                                    TA0004row("NACGESENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("NACGESENDDATE") = C_DEFAULT_YMD
                                End If

                                If work.WF_SEL_FUNCBRK.Text = "0" Then
                                    '休憩を含めない
                                    TA0004row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                Else
                                    '休憩開始日時
                                    If IsDate(TA0004row("NACBREAKSTDATE")) AndAlso TA0004row("NACBREAKSTDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACBREAKSTDATE")
                                        TA0004row("NACBREAKSTDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACBREAKSTDATE") = C_DEFAULT_YMD
                                    End If
                                End If

                                If work.WF_SEL_FUNCBRK.Text = "0" Then
                                    '休憩を含めない
                                    TA0004row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                Else
                                    '休憩終了日時
                                    If IsDate(TA0004row("NACBREAKENDDATE")) AndAlso TA0004row("NACBREAKENDDATE") <> C_DEFAULT_YMD Then
                                        wDATETime = TA0004row("NACBREAKENDDATE")
                                        TA0004row("NACBREAKENDDATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                    Else
                                        TA0004row("NACBREAKENDDATE") = C_DEFAULT_YMD
                                    End If
                                End If

                                If work.WF_SEL_FUNCBRK.Text = "0" Then
                                    '休憩を含めない
                                    TA0004row("NACBREAKTIME") = 0
                                    TA0004row("NACCHOBREAKTIME") = 0
                                    TA0004row("NACTTLBREAKTIME") = 0
                                End If

                                '出社日時
                                If IsDate(TA0004row("PAYSHUSHADATE")) AndAlso TA0004row("PAYSHUSHADATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("PAYSHUSHADATE")
                                    TA0004row("PAYSHUSHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("PAYSHUSHADATE") = C_DEFAULT_YMD
                                End If

                                '退社日時
                                If IsDate(TA0004row("PAYTAISHADATE")) AndAlso TA0004row("PAYTAISHADATE") <> C_DEFAULT_YMD Then
                                    wDATETime = TA0004row("PAYTAISHADATE")
                                    TA0004row("PAYTAISHADATE") = wDATETime.ToString("yyyy/MM/dd HH:mm")
                                Else
                                    TA0004row("PAYTAISHADATE") = C_DEFAULT_YMD
                                End If

                                TA0004tbl.Rows.Add(TA0004row)

                            Next
                        End Using


                    Catch ex As Exception
                        Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "L0003_SUMMARYN SELECT")
                        CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWRITE.INFPOSI = "DB:L0003_SUMMARYN Select"           '
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
    Public Sub SumTA0004WK2()

        Dim wINT As Integer
        Dim wDBL As Double
        Dim WW_KEY As String = ""
        Dim WW_KEY_OLD As String = ""
        Dim WW_NACSURYOG As Double = 0
        Dim WW_NACJSURYOG As Double = 0
        Dim WW_FIRST As String = "OFF"

        Dim TA0004SUMtbl As DataTable = TA0004tbl.Clone
        Dim TA0004SUMrow As DataRow = Nothing
        Dim TA0004SVrow As DataRow = Nothing

        '***********************************************************************************************
        '一時サマリ（出荷部署、出庫日、出荷日、届日、荷主、業務車番、乗務員、トリップ、ドロップ別）
        '***********************************************************************************************
        'ソートキー設定
        Dim WW_SORT As String = ""
        '部署別
        If WF_CBOX_SW1.Checked = True Then
            WW_SORT = WW_SORT & "NACSHIPORG"
        End If

        '出庫日別
        If WF_CBOX_SW2.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACSHUKODATE"
        End If

        '出荷日別
        If WF_CBOX_SW3.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACSHUKADATE"
        End If

        '届日別
        If WF_CBOX_SW4.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACTODOKEDATE"
        End If

        '荷主別
        If WF_CBOX_SW5.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACTORICODE"
        End If

        '業務車番別
        If WF_CBOX_SW6.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "KEYGSHABAN"
        End If

        '乗務員別
        If WF_CBOX_SW7.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACSTAFFCODE"
        End If

        'トリップ別
        If WF_CBOX_SW8.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACSHUKABASHO,KEYTRIPNO"
        End If
        'ドロップ別
        If WF_CBOX_SW9.Checked = True Then
            If WW_SORT <> "" Then
                WW_SORT = WW_SORT & ","
            End If
            WW_SORT = WW_SORT & "NACTODOKECODE,KEYDROPNO,NACSEQ"
        End If

        'ソート
        CS0026TblSort.TABLE = TA0004VIEWtbl
        CS0026TblSort.SORTING = WW_SORT
        CS0026TblSort.FILTER = ""
        TA0004VIEWtbl = CS0026TblSort.sort()
        WW_KEY = ""
        WW_KEY_OLD = ""
        WW_FIRST = "OFF"
        TA0004SUMtbl.Clear()
        TA0004SUMrow = Nothing
        TA0004SVrow = Nothing
        TA0004SUMtbl.Dispose()

        For Each TA0004row As DataRow In TA0004VIEWtbl.Rows

            WW_KEY = ""

            '出荷部署別
            If WF_CBOX_SW1.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACSHIPORG") & "_"
            End If

            '出庫日別
            If WF_CBOX_SW2.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACSHUKODATE") & "_"
            End If

            '出荷日別
            If WF_CBOX_SW3.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACSHUKADATE") & "_"
            End If

            '届日別
            If WF_CBOX_SW4.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACTODOKEDATE") & "_"
            End If

            '荷主別
            If WF_CBOX_SW5.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACTORICODE") & "_"
            End If

            '業務車番別
            If WF_CBOX_SW6.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("KEYGSHABAN") & "_"
            End If

            '乗務員別
            If WF_CBOX_SW7.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACSTAFFCODE") & "_"
            End If

            'トリップ別
            If WF_CBOX_SW8.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACSHUKABASHO") & "_" & TA0004row("KEYTRIPNO") & "_"
            End If
            'ドロップ別
            If WF_CBOX_SW9.Checked = True Then
                WW_KEY = WW_KEY & TA0004row("NACTODOKECODE") & "_" & TA0004row("KEYDROPNO") & "_"
            End If


            If WW_FIRST = "OFF" Then
                '初回のみブレイクキーを設定
                WW_KEY_OLD = ""

                '部署別
                If WF_CBOX_SW1.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHIPORG") & "_"
                End If

                '出庫日別
                If WF_CBOX_SW2.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKODATE") & "_"
                End If

                '出荷日別
                If WF_CBOX_SW3.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKADATE") & "_"
                End If

                '届日別
                If WF_CBOX_SW4.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTODOKEDATE") & "_"
                End If

                '荷主別
                If WF_CBOX_SW5.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTORICODE") & "_"
                End If

                '業務車番別
                If WF_CBOX_SW6.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("KEYGSHABAN") & "_"
                End If

                '乗務員別
                If WF_CBOX_SW7.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSTAFFCODE") & "_"
                End If

                'トリップ別
                If WF_CBOX_SW8.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKABASHO") & "_" & TA0004row("KEYTRIPNO") & "_"
                End If
                'ドロップ別
                If WF_CBOX_SW9.Checked = True Then
                    WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTODOKECODE") & "_" & TA0004row("KEYDROPNO") & "_"
                End If

                TA0004SVrow = TA0004SUMtbl.NewRow
                TA0004SVrow.ItemArray = TA0004row.ItemArray
                'サマリー項目初期化
                ItinalSummaryItem(TA0004SVrow)
                WW_FIRST = "ON"
            End If

            'ブレイクキーが変わったらサマリー結果を出力
            If WW_KEY_OLD = WW_KEY Then
            Else
                TA0004SUMrow = TA0004SUMtbl.NewRow
                TA0004SUMrow.ItemArray = TA0004SVrow.ItemArray
                TA0004SUMtbl.Rows.Add(TA0004SUMrow)

                TA0004SVrow = TA0004SUMtbl.NewRow
                TA0004SVrow.ItemArray = TA0004row.ItemArray
                'サマリー項目初期化
                ItinalSummaryItem(TA0004SVrow)
            End If

            '部署別
            If WF_CBOX_SW1.Checked = False Then
                TA0004SVrow("NACSHIPORG") = ""                 '配送部署 
                TA0004SVrow("NACSHIPORGNAME") = ""             '配送部署名称
                TA0004SVrow("NACORDERORG") = ""                '受注部署
                TA0004SVrow("NACORDERORGNAME") = ""            '受注部署名称
            End If

            '出庫日別
            If WF_CBOX_SW2.Checked = False Then
                TA0004SVrow("NACSHUKODATE") = ""               '出庫日・作業日
            End If

            '出荷日別
            If WF_CBOX_SW3.Checked = False Then
                TA0004SVrow("NACSHUKADATE") = ""               '出荷日
            End If

            '届日別
            If WF_CBOX_SW4.Checked = False Then
                TA0004SVrow("NACTODOKEDATE") = ""              '届日
            End If

            '荷主別
            If WF_CBOX_SW5.Checked = False Then
                TA0004SVrow("NACTORICODE") = ""                '荷主
                TA0004SVrow("NACTORICODENAME") = ""            '荷主名称 
            End If

            '業務車番別
            If WF_CBOX_SW6.Checked = False Then
                TA0004SVrow("NACGSHABAN") = ""                 '業務車番
                TA0004SVrow("NACSUPPLIERKBN") = ""             '社有・庸車区分
                TA0004SVrow("NACSUPPLIERKBNNAME") = ""         '社有・庸車区分名称
                TA0004SVrow("NACSUPPLIER") = ""                '庸車会社
                TA0004SVrow("NACSUPPLIERNAME") = ""            '庸車会社名称
                TA0004SVrow("NACSHARYOOILTYPE") = ""           '車両登録油種
                TA0004SVrow("NACSHARYOOILTYPENAME") = ""       '車両登録油種名称

                TA0004SVrow("NACSHARYOTYPE1") = ""             '車両タイプ1
                TA0004SVrow("NACSHARYOTYPE1NAME") = ""         '車両タイプ1名称
                TA0004SVrow("NACTSHABAN1") = ""                '統一車番1
                TA0004SVrow("NACMANGMORG1") = ""               '車両管理部署1
                TA0004SVrow("NACMANGMORG1NAME") = ""           '車両管理部署1名称
                TA0004SVrow("NACMANGSORG1") = ""               '車両設置部署1
                TA0004SVrow("NACMANGSORG1NAME") = ""           '車両設置部署1名称
                TA0004SVrow("NACMANGUORG1") = ""               '車両運用部署1
                TA0004SVrow("NACMANGUORG1NAME") = ""           '車両運用部署1名称
                TA0004SVrow("NACBASELEASE1") = ""              '車両所有1
                TA0004SVrow("NACBASELEASE1NAME") = ""          '車両所有1名称
                TA0004SVrow("NACLICNPLTNOF1") = ""             '登録番号1
                TA0004SVrow("NACSHARYOTYPE2") = ""             '車両タイプ2
                TA0004SVrow("NACSHARYOTYPE2NAME") = ""         '車両タイプ2名称
                TA0004SVrow("NACTSHABAN2") = ""                '統一車番2
                TA0004SVrow("NACMANGMORG2") = ""               '車両管理部署2
                TA0004SVrow("NACMANGMORG2NAME") = ""           '車両管理部署2名称
                TA0004SVrow("NACMANGSORG2") = ""               '車両設置部署2
                TA0004SVrow("NACMANGSORG2NAME") = ""           '車両設置部署2名称
                TA0004SVrow("NACMANGUORG2") = ""               '車両運用部署2
                TA0004SVrow("NACMANGUORG2NAME") = ""           '車両運用部署2名称
                TA0004SVrow("NACBASELEASE2") = ""              '車両所有2
                TA0004SVrow("NACBASELEASE2NAME") = ""          '車両所有2名称
                TA0004SVrow("NACLICNPLTNOF2") = ""             '登録番号2
                TA0004SVrow("NACSHARYOTYPE3") = ""             '車両タイプ3
                TA0004SVrow("NACSHARYOTYPE3NAME") = ""         '車両タイプ3名称
                TA0004SVrow("NACTSHABAN3") = ""                '統一車番3
                TA0004SVrow("NACMANGMORG3") = ""               '車両管理部署3
                TA0004SVrow("NACMANGMORG3NAME") = ""           '車両管理部署3名称
                TA0004SVrow("NACMANGSORG3") = ""               '車両設置部署3
                TA0004SVrow("NACMANGSORG3NAME") = ""           '車両設置部署3名称
                TA0004SVrow("NACMANGUORG3") = ""               '車両運用部署3
                TA0004SVrow("NACMANGUORG3NAME") = ""           '車両運用部署3名称
                TA0004SVrow("NACBASELEASE3") = ""              '車両所有3
                TA0004SVrow("NACBASELEASE3NAME") = ""          '車両所有3名称
                TA0004SVrow("NACLICNPLTNOF3") = ""             '登録番号3

                TA0004SVrow("KEYGSHABAN") = ""                 'SYS業務車番
                TA0004SVrow("KEYTSHABAN1") = ""                'SYS統一車番1
                TA0004SVrow("KEYTSHABAN2") = ""                'SYS統一車番2
                TA0004SVrow("KEYTSHABAN3") = ""                'SYS統一車番3
            End If

            '乗務員別
            If WF_CBOX_SW7.Checked = False Then
                TA0004SVrow("NACCREWKBN") = ""                 '正副区分
                TA0004SVrow("NACCREWKBNNAME") = ""             '正副区分名称
                TA0004SVrow("NACSTAFFCODE") = ""               '乗務員・従業員コード
                TA0004SVrow("NACSTAFFCODENAME") = ""           '乗務員・従業員コード名称
                TA0004SVrow("NACSTAFFKBN") = ""                '乗務員・社員区分
                TA0004SVrow("NACSTAFFKBNNAME") = ""            '乗務員・社員区分名称
                TA0004SVrow("NACMORG") = ""                    '乗務員・管理部署
                TA0004SVrow("NACMORGNAME") = ""                '乗務員・管理部署名称
                TA0004SVrow("NACHORG") = ""                    '乗務員・配属部署
                TA0004SVrow("NACHORGNAME") = ""                '乗務員・配属部署名称
                TA0004SVrow("NACSORG") = ""                    '乗務員・作業部署
                TA0004SVrow("NACSORGNAME") = ""                '乗務員・作業部署名称
                TA0004SVrow("NACSTAFFCODE2") = ""              '副乗務員・従業員コード
                TA0004SVrow("NACSTAFFCODE2NAME") = ""          '副乗務員・従業員コード名称
                TA0004SVrow("NACSTAFFKBN2") = ""               '副乗務員・社員区分
                TA0004SVrow("NACSTAFFKBN2NAME") = ""           '副乗務員・社員区分名称
                TA0004SVrow("NACMORG2") = ""                   '副乗務員・管理部署
                TA0004SVrow("NACMORG2NAME") = ""               '副乗務員・管理部署名称
                TA0004SVrow("NACHORG2") = ""                   '副乗務員・配属部署
                TA0004SVrow("NACHORG2NAME") = ""               '副乗務員・配属部署名称
                TA0004SVrow("NACSORG2") = ""                   '副乗務員・作業部署
                TA0004SVrow("NACSORG2NAME") = ""               '副乗務員・作業部署名称

                TA0004SVrow("PAYSTAFFKBN") = ""                '社員区分
                TA0004SVrow("PAYSTAFFKBNNAME") = ""            '社員区分名称
                TA0004SVrow("PAYSTAFFCODE") = ""               '従業員
                TA0004SVrow("PAYSTAFFCODENAME") = ""           '従業員名称
                TA0004SVrow("PAYMORG") = ""                    '従業員管理部署
                TA0004SVrow("PAYMORGNAME") = ""                '従業員管理部署名称
                TA0004SVrow("PAYHORG") = ""                    '従業員配属部署
                TA0004SVrow("PAYHORGNAME") = ""                '従業員配属部署名称

                TA0004SVrow("KEYSTAFFCODE") = ""               'SYS従業員
            End If

            'トリップ別
            If WF_CBOX_SW8.Checked = False Then
                'TA0004SVrow("NACKAIJI") = ""                   '回次
                TA0004SVrow("NACTRIPNO") = ""                  'トリップ
                TA0004SVrow("KEYTRIPNO") = ""                  'SYSトリップ

                TA0004SVrow("NACORDERNO") = ""                 '受注番号
                TA0004SVrow("NACDETAILNO") = ""                '明細№

                TA0004SVrow("NACSHUKABASHO") = ""              '出荷場所
                TA0004SVrow("NACSHUKABASHONAME") = ""          '出荷場所名称
            End If

            'ドロップ別
            If WF_CBOX_SW9.Checked = False Then
                'TA0004SVrow("NACKAIJI") = ""                   '回次
                TA0004SVrow("NACDROPNO") = ""                  'ドロップ
                TA0004SVrow("KEYDROPNO") = ""                  'SYSドロップ

                TA0004SVrow("NACSEQ") = ""                     'SEQ

                TA0004SVrow("NACPRODUCT2_1") = ""              '品名２_1
                TA0004SVrow("NACPRODUCT2NAME_1") = ""          '品名２名称_1
                TA0004SVrow("NACPRODUCT2_2") = ""              '品名２_2
                TA0004SVrow("NACPRODUCT2NAME_2") = ""          '品名２名称_2
                TA0004SVrow("NACPRODUCT2_3") = ""              '品名２_3
                TA0004SVrow("NACPRODUCT2NAME_3") = ""          '品名２名称_3
                TA0004SVrow("NACPRODUCT2_4") = ""              '品名２_4
                TA0004SVrow("NACPRODUCT2NAME_4") = ""          '品名２名称_4
                TA0004SVrow("NACPRODUCT2_5") = ""              '品名２_5
                TA0004SVrow("NACPRODUCT2NAME_5") = ""          '品名２名称_5
                TA0004SVrow("NACPRODUCT2_6") = ""              '品名２_6
                TA0004SVrow("NACPRODUCT2NAME_6") = ""          '品名２名称_6
                TA0004SVrow("NACPRODUCT2_7") = ""              '品名２_7
                TA0004SVrow("NACPRODUCT2NAME_7") = ""          '品名２名称_7
                TA0004SVrow("NACPRODUCT2_8") = ""              '品名２_8
                TA0004SVrow("NACPRODUCT2NAME_8") = ""          '品名２名称_8

                TA0004SVrow("NACTODOKECODE") = ""              '届先
                TA0004SVrow("NACTODOKECODENAME") = ""          '届先名称
            End If


            '********************************
            ' 以降、編集（サマリー）処理
            '********************************
            '売上
            wDBL = Val(TA0004row("NACSURYO1"))
            TA0004SVrow("NACSURYO1") = (Val(TA0004SVrow("NACSURYO1")) + wDBL).ToString("#0.000")    '受注・数量1
            TA0004SVrow("NACTANI1") = TA0004row("NACTANI1")                                       '受注・単位1
            TA0004SVrow("NACTANINAME1") = TA0004row("NACTANINAME1")                               '受注・単位1名称
            wDBL = Val(TA0004row("NACSURYO2"))
            TA0004SVrow("NACSURYO2") = (Val(TA0004SVrow("NACSURYO2")) + wDBL).ToString("#0.000")    '受注・数量2
            TA0004SVrow("NACTANI2") = TA0004row("NACTANI2")                                       '受注・単位2
            TA0004SVrow("NACTANINAME2") = TA0004row("NACTANINAME2")                               '受注・単位2名称
            wDBL = Val(TA0004row("NACSURYO3"))
            TA0004SVrow("NACSURYO3") = (Val(TA0004SVrow("NACSURYO3")) + wDBL).ToString("#0.000")    '受注・数量3
            TA0004SVrow("NACTANI3") = TA0004row("NACTANI3")                                       '受注・単位3
            TA0004SVrow("NACTANINAME3") = TA0004row("NACTANINAME3")                               '受注・単位3名称
            wDBL = Val(TA0004row("NACSURYO4"))
            TA0004SVrow("NACSURYO4") = (Val(TA0004SVrow("NACSURYO4")) + wDBL).ToString("#0.000")    '受注・数量4
            TA0004SVrow("NACTANI4") = TA0004row("NACTANI4")                                       '受注・単位4
            TA0004SVrow("NACTANINAME4") = TA0004row("NACTANINAME4")                               '受注・単位4名称
            wDBL = Val(TA0004row("NACSURYO5"))
            TA0004SVrow("NACSURYO5") = (Val(TA0004SVrow("NACSURYO5")) + wDBL).ToString("#0.000")    '受注・数量5
            TA0004SVrow("NACTANI5") = TA0004row("NACTANI5")                                       '受注・単位5
            TA0004SVrow("NACTANINAME5") = TA0004row("NACTANINAME5")                               '受注・単位5名称
            wDBL = Val(TA0004row("NACSURYO6"))
            TA0004SVrow("NACSURYO6") = (Val(TA0004SVrow("NACSURYO6")) + wDBL).ToString("#0.000")    '受注・数量6
            TA0004SVrow("NACTANI6") = TA0004row("NACTANI6")                                       '受注・単位6
            TA0004SVrow("NACTANINAME6") = TA0004row("NACTANINAME6")                               '受注・単位6名称
            wDBL = Val(TA0004row("NACSURYO7"))
            TA0004SVrow("NACSURYO7") = (Val(TA0004SVrow("NACSURYO7")) + wDBL).ToString("#0.000")    '受注・数量7
            TA0004SVrow("NACTANI7") = TA0004row("NACTANI7")                                       '受注・単位7
            TA0004SVrow("NACTANINAME7") = TA0004row("NACTANINAME7")                               '受注・単位7名称
            wDBL = Val(TA0004row("NACSURYO8"))
            TA0004SVrow("NACSURYO8") = (Val(TA0004SVrow("NACSURYO8")) + wDBL).ToString("#0.000")    '受注・数量8
            TA0004SVrow("NACTANI8") = TA0004row("NACTANI8")                                       '受注・単位8
            TA0004SVrow("NACTANINAME8") = TA0004row("NACTANINAME8")                               '受注・単位8名称
            wDBL = Val(TA0004row("NACSURYOG"))
            TA0004SVrow("NACSURYOG") = (Val(TA0004SVrow("NACSURYOG")) + wDBL).ToString("#0.000")    '受注・数量合計Σ

            wDBL = Val(TA0004row("NACJSURYO1"))
            TA0004SVrow("NACJSURYO1") = (Val(TA0004SVrow("NACJSURYO1")) + wDBL).ToString("#0.000")  '実績・配送数量1
            TA0004SVrow("NACSTANI1") = TA0004row("NACSTANI1")                                     '実績・配送単位1
            TA0004SVrow("NACSTANINAME1") = TA0004row("NACSTANINAME1")                             '実績・配送単位1名称
            wDBL = Val(TA0004row("NACJSURYO2"))
            TA0004SVrow("NACJSURYO2") = (Val(TA0004SVrow("NACJSURYO2")) + wDBL).ToString("#0.000")  '実績・配送数量2
            TA0004SVrow("NACSTANI2") = TA0004row("NACSTANI2")                                     '実績・配送単位2
            TA0004SVrow("NACSTANINAME2") = TA0004row("NACSTANINAME2")                             '実績・配送単位2名称
            wDBL = Val(TA0004row("NACJSURYO3"))
            TA0004SVrow("NACJSURYO3") = (Val(TA0004SVrow("NACJSURYO3")) + wDBL).ToString("#0.000")  '実績・配送数量3
            TA0004SVrow("NACSTANI3") = TA0004row("NACSTANI3")                                     '実績・配送単位3
            TA0004SVrow("NACSTANINAME3") = TA0004row("NACSTANINAME3")                             '実績・配送単位3名称
            wDBL = Val(TA0004row("NACJSURYO4"))
            TA0004SVrow("NACJSURYO4") = (Val(TA0004SVrow("NACJSURYO4")) + wDBL).ToString("#0.000")  '実績・配送数量4
            TA0004SVrow("NACSTANI4") = TA0004row("NACSTANI4")                                     '実績・配送単位4
            TA0004SVrow("NACSTANINAME4") = TA0004row("NACSTANINAME4")                             '実績・配送単位4名称
            wDBL = Val(TA0004row("NACJSURYO5"))
            TA0004SVrow("NACJSURYO5") = (Val(TA0004SVrow("NACJSURYO5")) + wDBL).ToString("#0.000")  '実績・配送数量5
            TA0004SVrow("NACSTANI5") = TA0004row("NACSTANI5")                                     '実績・配送単位5
            TA0004SVrow("NACSTANINAME5") = TA0004row("NACSTANINAME5")                             '実績・配送単位5名称
            wDBL = Val(TA0004row("NACJSURYO6"))
            TA0004SVrow("NACJSURYO6") = (Val(TA0004SVrow("NACJSURYO6")) + wDBL).ToString("#0.000")  '実績・配送数量6
            TA0004SVrow("NACSTANI6") = TA0004row("NACSTANI6")                                     '実績・配送単位6
            TA0004SVrow("NACSTANINAME6") = TA0004row("NACSTANINAME6")                             '実績・配送単位6名称
            wDBL = Val(TA0004row("NACJSURYO7"))
            TA0004SVrow("NACJSURYO7") = (Val(TA0004SVrow("NACJSURYO7")) + wDBL).ToString("#0.000")  '実績・配送数量7
            TA0004SVrow("NACSTANI7") = TA0004row("NACSTANI7")                                     '実績・配送単位7
            TA0004SVrow("NACSTANINAME7") = TA0004row("NACSTANINAME7")                             '実績・配送単位7名称
            wDBL = Val(TA0004row("NACJSURYO8"))
            TA0004SVrow("NACJSURYO8") = (Val(TA0004SVrow("NACJSURYO8")) + wDBL).ToString("#0.000")  '実績・配送数量8
            TA0004SVrow("NACSTANI8") = TA0004row("NACSTANI8")                                     '実績・配送単位8
            TA0004SVrow("NACSTANINAME8") = TA0004row("NACSTANINAME8")                             '実績・配送単位8名称

            wDBL = Val(TA0004row("NACJSURYOG"))
            TA0004SVrow("NACJSURYOG") = (Val(TA0004SVrow("NACJSURYOG")) + wDBL).ToString("#0.000") '実績・配送数量合計Σ


            '労務費（配送作業）
            wDBL = Val(TA0004row("NACHAIDISTANCE"))
            TA0004SVrow("NACHAIDISTANCE") = (Val(TA0004SVrow("NACHAIDISTANCE")) + wDBL).ToString("#0.00")     '実績・配送距離

            '労務費（回送）
            wDBL = Val(TA0004row("NACKAIDISTANCE"))
            TA0004SVrow("NACKAIDISTANCE") = (Val(TA0004SVrow("NACKAIDISTANCE")) + wDBL).ToString("#0.00")    '実績・下車作業距離

            wDBL = Val(TA0004row("NACCHODISTANCE"))
            TA0004SVrow("NACCHODISTANCE") = (Val(TA0004SVrow("NACCHODISTANCE")) + wDBL).ToString("#0.00")    '実績・勤怠調整距離

            wDBL = Val(TA0004row("NACTTLDISTANCE"))
            TA0004SVrow("NACTTLDISTANCE") = (Val(TA0004SVrow("NACTTLDISTANCE")) + wDBL).ToString("#0.00")    '実績・配送距離合計Σ

            '労務費（配送作業）
            TA0004SVrow("NACHAISTDATE") = TA0004row("NACHAISTDATE")                                        '実績・配送作業開始日時
            TA0004SVrow("NACHAIENDDATE") = TA0004row("NACHAIENDDATE")                                      '実績・配送作業終了日時
            wINT = Val(TA0004row("NACHAIWORKTIME"))
            TA0004SVrow("NACHAIWORKTIME") = Val(TA0004SVrow("NACHAIWORKTIME")) + wINT                       '実績・配送作業時間

            '労務費（回送）
            TA0004SVrow("NACGESSTDATE") = ""                                                                '実績・下車作業開始日時
            TA0004SVrow("NACGESENDDATE") = ""                                                               '実績・下車作業終了日時
            wINT = Val(TA0004row("NACGESWORKTIME"))
            TA0004SVrow("NACGESWORKTIME") = Val(TA0004SVrow("NACGESWORKTIME")) + wINT                       '実績・下車作業時間


            wINT = Val(TA0004row("NACCHOWORKTIME"))
            TA0004SVrow("NACCHOWORKTIME") = Val(TA0004SVrow("NACCHOWORKTIME")) + wINT                       '実績・勤怠調整時間

            wINT = Val(TA0004row("NACTTLWORKTIME"))
            TA0004SVrow("NACTTLWORKTIME") = Val(TA0004SVrow("NACTTLWORKTIME")) + wINT                       '実績・配送合計時間Σ


            '労務費（配送作業）& 労務費（回送）
            wINT = Val(TA0004row("NACOUTWORKTIME"))
            TA0004SVrow("NACOUTWORKTIME") = Val(TA0004SVrow("NACOUTWORKTIME")) + wINT                       '実績・就業外時間


            '労務費（休憩）
            TA0004SVrow("NACBREAKSTDATE") = ""                                                              '実績・休憩開始日時
            TA0004SVrow("NACBREAKENDDATE") = ""                                                             '実績・休憩終了日時
            wINT = Val(TA0004row("NACBREAKTIME"))
            TA0004SVrow("NACBREAKTIME") = Val(TA0004SVrow("NACBREAKTIME")) + wINT                           '実績・休憩時間
            wINT = Val(TA0004row("NACCHOBREAKTIME"))
            TA0004SVrow("NACCHOBREAKTIME") = Val(TA0004SVrow("NACCHOBREAKTIME")) + wINT                     '実績・休憩調整時間
            wINT = Val(TA0004row("NACTTLBREAKTIME"))
            TA0004SVrow("NACTTLBREAKTIME") = Val(TA0004SVrow("NACTTLBREAKTIME")) + wINT                     '実績・休憩合計時間Σ

            wINT = Val(TA0004row("NACCASH"))
            TA0004SVrow("NACCASH") = Val(TA0004SVrow("NACCASH")) + wINT                                     '実績・現金

            wINT = Val(TA0004row("NACETC"))
            TA0004SVrow("NACETC") = Val(TA0004SVrow("NACETC")) + wINT                                       '実績・ETC

            wINT = Val(TA0004row("NACTICKET"))
            TA0004SVrow("NACTICKET") = Val(TA0004SVrow("NACTICKET")) + wINT                                 '実績・回数券

            wDBL = Val(TA0004row("NACKYUYU"))
            TA0004SVrow("NACKYUYU") = (Val(TA0004SVrow("NACKYUYU")) + wDBL).ToString("#0.00")               '実績・軽油

            '労務費（配送作業）
            wINT = Val(TA0004row("NACUNLOADCNT"))
            TA0004SVrow("NACUNLOADCNT") = Val(TA0004SVrow("NACUNLOADCNT")) + wINT                           '実績・荷卸回数
            wINT = Val(TA0004row("NACCHOUNLOADCNT"))
            TA0004SVrow("NACCHOUNLOADCNT") = Val(TA0004SVrow("NACCHOUNLOADCNT")) + wINT                     '実績・荷卸回数調整
            wINT = Val(TA0004row("NACTTLUNLOADCNT"))
            TA0004SVrow("NACTTLUNLOADCNT") = Val(TA0004SVrow("NACTTLUNLOADCNT")) + wINT                     '実績・荷卸回数合計Σ

            '車両稼動（自社＆リース）
            wINT = Val(TA0004row("NACKAIJI"))
            TA0004SVrow("NACKAIJI") = Val(TA0004SVrow("NACKAIJI")) + wINT                                   '実績・回次

            wINT = Val(TA0004row("NACJITIME"))
            TA0004SVrow("NACJITIME") = Val(TA0004SVrow("NACJITIME")) + wINT                                 '実績・実車時間
            wINT = Val(TA0004row("NACJICHOSTIME"))
            TA0004SVrow("NACJICHOSTIME") = Val(TA0004SVrow("NACJICHOSTIME")) + wINT                         '実績・実車時間調整
            wINT = Val(TA0004row("NACJITTLETIME"))
            TA0004SVrow("NACJITTLETIME") = Val(TA0004SVrow("NACJITTLETIME")) + wINT                         '実績・実車時間合計Σ
            TA0004SVrow("NACJKTTLTIME") = Val(TA0004SVrow("NACJKTTLTIME")) + wINT                           '実績・実績／空車時間合計Σ


            '車両稼動（自社＆リース）、回送（自社＆リース）
            wINT = Val(TA0004row("NACKUTIME"))
            TA0004SVrow("NACKUTIME") = Val(TA0004SVrow("NACKUTIME")) + wINT                                 '実績・空車時間
            wINT = Val(TA0004row("NACKUCHOTIME"))
            TA0004SVrow("NACKUCHOTIME") = Val(TA0004SVrow("NACKUCHOTIME")) + wINT                           '実績・空車時間調整
            wINT = Val(TA0004row("NACKUTTLTIME"))
            TA0004SVrow("NACKUTTLTIME") = Val(TA0004SVrow("NACKUTTLTIME")) + wINT                           '実績・空車時間合計Σ
            TA0004SVrow("NACJKTTLTIME") = Val(TA0004SVrow("NACJKTTLTIME")) + wINT                           '実績・実績／空車時間合計Σ

            '車両稼動（自社＆リース）
            wDBL = Val(TA0004row("NACJIDISTANCE"))
            TA0004SVrow("NACJIDISTANCE") = (Val(TA0004SVrow("NACJIDISTANCE")) + wDBL).ToString("#0.00")                '実績・実車距離
            wDBL = Val(TA0004row("NACJICHODISTANCE"))
            TA0004SVrow("NACJICHODISTANCE") = (Val(TA0004SVrow("NACJICHODISTANCE")) + wDBL).ToString("#0.00")          '実績・実車距離調整
            wDBL = Val(TA0004row("NACJITTLDISTANCE"))
            TA0004SVrow("NACJITTLDISTANCE") = (Val(TA0004SVrow("NACJITTLDISTANCE")) + wDBL).ToString("#0.00")          '実績・実車距離合計Σ
            TA0004SVrow("NACJKTTLDISTANCE") = (Val(TA0004SVrow("NACJKTTLDISTANCE")) + wDBL).ToString("#0.00")          '実績・実車／空車距離合計Σ

            '車両稼動（自社＆リース）、回送（自社＆リース）
            wDBL = Val(TA0004row("NACKUDISTANCE"))
            TA0004SVrow("NACKUDISTANCE") = (Val(TA0004SVrow("NACKUDISTANCE")) + wDBL).ToString("#0.00")                '実績・空車距離
            wDBL = Val(TA0004row("NACKUCHODISTANCE"))
            TA0004SVrow("NACKUCHODISTANCE") = (Val(TA0004SVrow("NACKUCHODISTANCE")) + wDBL).ToString("#0.00")          '実績・空車距離調整
            wDBL = Val(TA0004row("NACKUTTLDISTANCE"))
            TA0004SVrow("NACKUTTLDISTANCE") = (Val(TA0004SVrow("NACKUTTLDISTANCE")) + wDBL).ToString("#0.00")          '実績・空車距離合計Σ
            TA0004SVrow("NACJKTTLDISTANCE") = (Val(TA0004SVrow("NACJKTTLDISTANCE")) + wDBL).ToString("#0.00")          '実績・実車／空車距離合計Σ

            If WF_CBOX_SW5.Checked = True Then
                TA0004SVrow("PAYSTAFFKBN") = TA0004row("PAYSTAFFKBN")                              '社員区分
                TA0004SVrow("PAYSTAFFKBNNAME") = TA0004row("PAYSTAFFKBNNAME")                      '社員区分名称
                TA0004SVrow("PAYSTAFFCODE") = TA0004row("PAYSTAFFCODE")                            '従業員
                TA0004SVrow("PAYSTAFFCODENAME") = TA0004row("PAYSTAFFCODENAME")                    '従業員名称
                TA0004SVrow("PAYMORG") = TA0004row("PAYMORG")                                      '従業員管理部署
                TA0004SVrow("PAYMORGNAME") = TA0004row("PAYMORGNAME")                              '従業員管理部署名称
                TA0004SVrow("PAYHORG") = TA0004row("PAYHORG")                                      '従業員配属部署
                TA0004SVrow("PAYHORGNAME") = TA0004row("PAYHORGNAME")                              '従業員配属部署名称
            Else
                TA0004SVrow("PAYSTAFFKBN") = ""                                                      '社員区分
                TA0004SVrow("PAYSTAFFKBNNAME") = ""                                                  '社員区分名称
                TA0004SVrow("PAYSTAFFCODE") = ""                                                     '従業員
                TA0004SVrow("PAYSTAFFCODENAME") = ""                                                 '従業員名称
                TA0004SVrow("PAYMORG") = ""                                                          '従業員管理部署
                TA0004SVrow("PAYMORGNAME") = ""                                                      '従業員管理部署名称
                TA0004SVrow("PAYHORG") = ""                                                          '従業員配属部署
                TA0004SVrow("PAYHORGNAME") = ""                                                      '従業員配属部署名称
            End If
            '            

            TA0004SVrow("DENYMD") = ""
            TA0004SVrow("DENNO") = ""
            TA0004SVrow("KANRENDENNO") = ""
            TA0004SVrow("DTLNO") = ""
            TA0004SVrow("ACACHANTEI") = ""
            TA0004SVrow("ACACHANTEINAME") = ""
            TA0004SVrow("WORKKBN") = ""
            TA0004SVrow("WORKKBNNAME") = ""


            WW_KEY_OLD = ""

            '部署別
            If WF_CBOX_SW1.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHIPORG") & "_"
            End If

            '出庫日別
            If WF_CBOX_SW2.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKODATE") & "_"
            End If

            '出荷日別
            If WF_CBOX_SW3.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKADATE") & "_"
            End If

            '届日別
            If WF_CBOX_SW4.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTODOKEDATE") & "_"
            End If

            '荷主別
            If WF_CBOX_SW5.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTORICODE") & "_"
            End If

            '業務車番別
            If WF_CBOX_SW6.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("KEYGSHABAN") & "_"
            End If

            '乗務員別
            If WF_CBOX_SW7.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSTAFFCODE") & "_"
            End If

            'トリップ別
            If WF_CBOX_SW8.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACSHUKABASHO") & "_" & TA0004row("KEYTRIPNO") & "_"
            End If
            'ドロップ別
            If WF_CBOX_SW9.Checked = True Then
                WW_KEY_OLD = WW_KEY_OLD & TA0004row("NACTODOKECODE") & "_" & TA0004row("KEYDROPNO") & "_"
            End If

        Next
        '最終レコードの出力
        If TA0004VIEWtbl.Rows.Count > 0 Then
            TA0004SUMrow = TA0004SUMtbl.NewRow
            TA0004SUMrow.ItemArray = TA0004SVrow.ItemArray
            TA0004SUMtbl.Rows.Add(TA0004SUMrow)
        End If

        CS0026TblSort.TABLE = TA0004SUMtbl
        CS0026TblSort.SORTING = "NACSHIPORG,NACSHUKODATE,NACSHUKADATE,NACTODOKEDATE,NACKEIJODATE,NACTORICODE,KEYGSHABAN,NACSTAFFCODE,KEYTRIPNO,KEYDROPNO,NACSEQ"
        CS0026TblSort.FILTER = ""
        TA0004SUMtbl = CS0026TblSort.sort()

        'サマリー結果で入れ替え
        TA0004VIEWtbl = TA0004SUMtbl.Copy

    End Sub

    ''' <summary>
    ''' サマリー項目の初期化
    ''' </summary>
    ''' <param name="IO_ROW">初期化対象のテーブル</param>
    ''' <remarks></remarks>
    Protected Sub ItinalSummaryItem(ByRef IO_ROW As DataRow)

        IO_ROW("NACSURYO1") = 0                         '受注・数量1
        IO_ROW("NACSURYO2") = 0                         '受注・数量2
        IO_ROW("NACSURYO3") = 0                         '受注・数量3
        IO_ROW("NACSURYO4") = 0                         '受注・数量4
        IO_ROW("NACSURYO5") = 0                         '受注・数量5
        IO_ROW("NACSURYO6") = 0                         '受注・数量6
        IO_ROW("NACSURYO7") = 0                         '受注・数量7
        IO_ROW("NACSURYO8") = 0                         '受注・数量8
        IO_ROW("NACSURYOG") = 0                         '受注・数量合計Σ
        IO_ROW("NACJSURYO1") = 0                        '実績・配送数量1
        IO_ROW("NACJSURYO2") = 0                        '実績・配送数量2
        IO_ROW("NACJSURYO3") = 0                        '実績・配送数量3
        IO_ROW("NACJSURYO4") = 0                        '実績・配送数量4
        IO_ROW("NACJSURYO5") = 0                        '実績・配送数量5
        IO_ROW("NACJSURYO6") = 0                        '実績・配送数量6
        IO_ROW("NACJSURYO7") = 0                        '実績・配送数量7
        IO_ROW("NACJSURYO8") = 0                        '実績・配送数量8
        IO_ROW("NACJSURYOG") = 0                        '実績・配送数量合計Σ
        IO_ROW("NACHAIDISTANCE") = 0                    '実績・配送距離
        IO_ROW("NACKAIDISTANCE") = 0                    '実績・下車作業距離
        IO_ROW("NACCHODISTANCE") = 0                    '実績・勤怠調整距離
        IO_ROW("NACTTLDISTANCE") = 0                    '実績・配送距離合計Σ
        IO_ROW("NACHAIWORKTIME") = 0                    '実績・配送作業時間
        IO_ROW("NACGESWORKTIME") = 0                    '実績・下車作業時間
        IO_ROW("NACCHOWORKTIME") = 0                    '実績・勤怠調整時間
        IO_ROW("NACTTLWORKTIME") = 0                    '実績・配送合計時間Σ
        IO_ROW("NACCASH") = 0                           '実績・現金
        IO_ROW("NACETC") = 0                            '実績・ETC
        IO_ROW("NACTICKET") = 0                         '実績・回数券
        IO_ROW("NACKYUYU") = 0                          '実績・軽油
        IO_ROW("NACOUTWORKTIME") = 0                    '実績・就業外時間
        IO_ROW("NACBREAKTIME") = 0                      '実績・休憩時間
        IO_ROW("NACTTLBREAKTIME") = 0                   '実績・休憩合計時間Σ
        IO_ROW("NACUNLOADCNT") = 0                      '実績・荷卸回数
        IO_ROW("NACCHOUNLOADCNT") = 0                   '実績・荷卸回数調整
        IO_ROW("NACTTLUNLOADCNT") = 0                   '実績・荷卸回数合計Σ
        IO_ROW("NACKAIJI") = 0                          '実績・回次
        IO_ROW("NACJITIME") = 0                         '実績・実車時間
        IO_ROW("NACJICHOSTIME") = 0                     '実績・実車時間調整
        IO_ROW("NACJITTLETIME") = 0                     '実績・実車時間合計Σ
        IO_ROW("NACKUTIME") = 0                         '実績・空車時間
        IO_ROW("NACKUCHOTIME") = 0                      '実績・空車時間調整
        IO_ROW("NACKUTTLTIME") = 0                      '実績・空車時間合計Σ
        IO_ROW("NACJKTTLTIME") = 0                      '実績・実車／空車時間合計Σ
        IO_ROW("NACJIDISTANCE") = 0                     '実績・実車距離
        IO_ROW("NACJICHODISTANCE") = 0                  '実績・実車距離調整
        IO_ROW("NACJITTLDISTANCE") = 0                  '実績・実車距離合計Σ
        IO_ROW("NACKUDISTANCE") = 0                     '実績・空車距離
        IO_ROW("NACKUCHODISTANCE") = 0                  '実績・空車距離調整
        IO_ROW("NACKUTTLDISTANCE") = 0                  '実績・空車距離合計Σ
        IO_ROW("NACJKTTLDISTANCE") = 0                  '実績・実車／空車距離合計Σ

    End Sub

    ''' <summary>
    ''' セレクターの設定初期化処理
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
        Dim WW_Cols As String() = {"NACSHIPORG", "NACSHIPORGNAME"}
        WW_TBLview = New DataView(TA0004tbl)
        WW_TBLview.Sort = "NACSHIPORG"
        '出荷部署、出荷部署名でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        Dim SELECTORrow As DataRow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = GRTA0004WRKINC.ALL_SELECTOR.CODE
        SELECTORrow("NAME") = GRTA0004WRKINC.ALL_SELECTOR.NAME
        SELECTORtbl.Rows.Add(SELECTORrow)

        For Each TA0004row As DataRow In WW_GRPtbl.Rows
            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0004row("NACSHIPORG")
            SELECTORrow("NAME") = TA0004row("NACSHIPORGNAME") & "(" & TA0004row("NACSHIPORG") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.SORTING = "CODE, NAME"
        CS0026TblSort.FILTER = ""
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

        SELECTORtbl.Clear()
        WW_GRPtbl.Clear()
        WW_Cols = {}

        '---------------------------------------------------
        '乗務員セレクター作成
        '---------------------------------------------------
        WW_Cols = {"NACSTAFFCODE", "NACSTAFFCODENAME"}
        WW_TBLview = New DataView(TA0004tbl)
        WW_TBLview.Sort = "NACSTAFFCODE"

        '乗務員、乗務員名称でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        SELECTORrow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = GRTA0004WRKINC.ALL_SELECTOR.CODE
        SELECTORrow("NAME") = GRTA0004WRKINC.ALL_SELECTOR.NAME
        SELECTORtbl.Rows.Add(SELECTORrow)

        For Each TA0004row As DataRow In WW_GRPtbl.Rows

            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0004row("NACSTAFFCODE")
            SELECTORrow("NAME") = TA0004row("NACSTAFFCODENAME") & "(" & TA0004row("NACSTAFFCODE") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.SORTING = "CODE, NAME"
        CS0026TblSort.FILTER = ""
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

        SELECTORtbl.Clear()
        WW_GRPtbl.Clear()
        WW_Cols = {}

        '---------------------------------------------------
        '車両セレクター作成
        '---------------------------------------------------
        WW_Cols = {"NACGSHABAN", "NACLICNPLTNOF1"}
        WW_TBLview = New DataView(TA0004tbl)
        WW_TBLview.Sort = "NACGSHABAN"

        '車両、登録№でグループ化しキーテーブル作成
        WW_GRPtbl = WW_TBLview.ToTable(True, WW_Cols)

        SELECTORrow = SELECTORtbl.NewRow
        SELECTORrow("CODE") = GRTA0004WRKINC.ALL_SELECTOR.CODE
        SELECTORrow("NAME") = GRTA0004WRKINC.ALL_SELECTOR.NAME
        SELECTORtbl.Rows.Add(SELECTORrow)
        For Each TA0004row As DataRow In WW_GRPtbl.Rows
            SELECTORrow = SELECTORtbl.NewRow
            SELECTORrow("CODE") = TA0004row("NACGSHABAN")
            SELECTORrow("NAME") = TA0004row("NACGSHABAN") & "(" & TA0004row("NACLICNPLTNOF1") & ")"
            SELECTORtbl.Rows.Add(SELECTORrow)
        Next

        CS0026TblSort.TABLE = SELECTORtbl
        CS0026TblSort.SORTING = "CODE, NAME"
        CS0026TblSort.FILTER = ""
        SELECTORtbl = CS0026TblSort.sort()

        '●セレクター設定処理
        WF_GSHABANselector.DataSource = SELECTORtbl
        WF_GSHABANselector.DataBind()

        If SELECTORtbl.Rows.Count <= 0 Then
            WW_POS = ""
            WF_SELECTOR_PosiGSHABAN.Value = ""
        Else
            WW_POS = SELECTORtbl.Rows(0)("CODE")
            WF_SELECTOR_PosiGSHABAN.Value = SELECTORtbl.Rows(0)("CODE")
        End If

        SetRepeater("2", WF_GSHABANselector, "WF_SELgshaban_VALUE", "WF_SELgshaban_TEXT", WW_POS)

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

        WW_GRPtbl.Dispose()
        WW_GRPtbl = Nothing

    End Sub
    ''' <summary>
    ''' セレクターの詳細情報設定処理
    ''' </summary>
    ''' <param name="I_KBN">区分値</param>
    ''' <param name="I_SELECTOR_OBJ">セレクター</param>
    ''' <param name="I_VALUE_OBJ">コード</param>
    ''' <param name="I_TEXT_OBJ">文字列</param>
    ''' <param name="I_POS">位置</param>
    ''' <remarks></remarks>
    Protected Sub SetRepeater(ByVal I_KBN As String, ByRef I_SELECTOR_OBJ As Repeater, ByRef I_VALUE_OBJ As String, ByRef I_TEXT_OBJ As String, ByVal I_POS As String)

        For i As Integer = 0 To I_SELECTOR_OBJ.Items.Count - 1
            '値　
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), Label).Text = SELECTORtbl.Rows(i)("CODE")
            'テキスト
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Text = "　" & SELECTORtbl.Rows(i)("NAME")

            '背景色
            If CType(I_SELECTOR_OBJ.Items(i).FindControl(I_VALUE_OBJ), Label).Text = I_POS Then
                CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black; font-size: 1.3rem;"
            Else
                CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black; font-size: 1.3rem;"
            End If

            'イベント追加
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Attributes.Remove("onclick")
            CType(I_SELECTOR_OBJ.Items(i).FindControl(I_TEXT_OBJ), Label).Attributes.Add("onclick", "SELECTOR_Click('" & I_KBN & "','" & SELECTORtbl.Rows(i)("CODE") & "');")
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

        '車両
        If WW_RADIO = 2 Then
            For i As Integer = 0 To WF_GSHABANselector.Items.Count - 1
                '背景色
                If CType(WF_GSHABANselector.Items(i).FindControl("WF_SELgshaban_VALUE"), Label).Text = WF_SELECTOR_PosiGSHABAN.Value Then
                    CType(WF_GSHABANselector.Items(i).FindControl("WF_SELgshaban_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:darksalmon;border: solid 1.0px black;"
                Else
                    CType(WF_GSHABANselector.Items(i).FindControl("WF_SELgshaban_TEXT"), Label).Style.Value = "height:1.5em;width:11.7em;background-color:rgb(220,230,240);border: solid 1.0px black;"
                End If
            Next

        End If

    End Sub

    ''' <summary>
    ''' TA0004tbl項目設定
    ''' </summary>
    ''' <param name="O_TBL">列追加対象テーブル</param>
    ''' <remarks></remarks>
    Protected Sub AddColumnToTA0004tbl(ByRef O_TBL As DataTable)
        If IsNothing(O_TBL) Then O_TBL = New DataTable

        '○DB項目クリア
        If O_TBL.Columns.Count = 0 Then
        Else
            O_TBL.Columns.Clear()
        End If

        '○共通項目
        O_TBL.Clear()
        O_TBL.Columns.Add("LINECNT", GetType(Integer))                   'DBの固定フィールド
        O_TBL.Columns.Add("OPERATION", GetType(String))                  'DBの固定フィールド
        O_TBL.Columns.Add("TIMSTP", GetType(String))                     'DBの固定フィールド
        O_TBL.Columns.Add("SELECT", GetType(Integer))                    'DBの固定フィールド
        O_TBL.Columns.Add("HIDDEN", GetType(Integer))                    'DBの固定フィールド

        '○画面固有項目
        O_TBL.Columns.Add("CAMPCODE", GetType(String))                   '会社
        O_TBL.Columns.Add("CAMPNAME", GetType(String))                   '会社名称
        O_TBL.Columns.Add("MOTOCHO", GetType(String))                    '元帳
        O_TBL.Columns.Add("MOTOCHONAME", GetType(String))                '元帳名称
        O_TBL.Columns.Add("VERSION", GetType(String))                    'バージョン
        O_TBL.Columns.Add("DENTYPE", GetType(String))                    '伝票タイプ
        O_TBL.Columns.Add("TENKI", GetType(String))                      '統計転記
        O_TBL.Columns.Add("TENKINAME", GetType(String))                  '統計転記名称
        O_TBL.Columns.Add("KEIJOYMD", GetType(String))                   '計上日付
        O_TBL.Columns.Add("DENYMD", GetType(String))                     '伝票日付
        O_TBL.Columns.Add("DENNO", GetType(String))                      '伝票番号
        O_TBL.Columns.Add("KANRENDENNO", GetType(String))                '関連伝票No＋明細No
        O_TBL.Columns.Add("DTLNO", GetType(String))                      '明細番号
        O_TBL.Columns.Add("INQKBN", GetType(String))                     '照会区分
        O_TBL.Columns.Add("INQKBNNAME", GetType(String))                 '照会区分名称
        O_TBL.Columns.Add("ACACHANTEI", GetType(String))                 '仕訳決定
        O_TBL.Columns.Add("ACACHANTEINAME", GetType(String))             '仕訳決定名称


        O_TBL.Columns.Add("NACSHUKODATE", GetType(String))               '出庫日・作業日
        O_TBL.Columns.Add("NACSHUKADATE", GetType(String))               '出荷日
        O_TBL.Columns.Add("NACTODOKEDATE", GetType(String))              '届日
        O_TBL.Columns.Add("NACKEIJODATE", GetType(String))               '計上日付(★追加)

        O_TBL.Columns.Add("NACTORICODE", GetType(String))                '荷主
        O_TBL.Columns.Add("NACTORICODENAME", GetType(String))            '荷主名称
        O_TBL.Columns.Add("NACURIKBN", GetType(String))                  '売上計上基準
        O_TBL.Columns.Add("NACURIKBNNAME", GetType(String))              '売上計上基準名称
        O_TBL.Columns.Add("NACTODOKECODE", GetType(String))              '届先
        O_TBL.Columns.Add("NACTODOKECODENAME", GetType(String))          '届先名称
        O_TBL.Columns.Add("NACSTORICODE", GetType(String))               '販売店
        O_TBL.Columns.Add("NACSTORICODENAME", GetType(String))           '販売店名称
        O_TBL.Columns.Add("NACSHUKABASHO", GetType(String))              '出荷場所
        O_TBL.Columns.Add("NACSHUKABASHONAME", GetType(String))          '出荷場所名称
        O_TBL.Columns.Add("NACTORITYPE01", GetType(String))              '取引タイプ01
        O_TBL.Columns.Add("NACTORITYPE01NAME", GetType(String))          '取引タイプ01名称
        O_TBL.Columns.Add("NACTORITYPE02", GetType(String))              '取引タイプ02
        O_TBL.Columns.Add("NACTORITYPE02NAME", GetType(String))          '取引タイプ02名称
        O_TBL.Columns.Add("NACTORITYPE03", GetType(String))              '取引タイプ03
        O_TBL.Columns.Add("NACTORITYPE03NAME", GetType(String))          '取引タイプ03名称
        O_TBL.Columns.Add("NACTORITYPE04", GetType(String))              '取引タイプ04
        O_TBL.Columns.Add("NACTORITYPE04NAME", GetType(String))          '取引タイプ04名称
        O_TBL.Columns.Add("NACTORITYPE05", GetType(String))              '取引タイプ05
        O_TBL.Columns.Add("NACTORITYPE05NAME", GetType(String))          '取引タイプ05名称

        O_TBL.Columns.Add("NACOILTYPE_1", GetType(String))               '油種_1
        O_TBL.Columns.Add("NACOILTYPENAME_1", GetType(String))           '油種名称_1
        O_TBL.Columns.Add("NACOILTYPE_2", GetType(String))               '油種_2
        O_TBL.Columns.Add("NACOILTYPENAME_2", GetType(String))           '油種名称_2
        O_TBL.Columns.Add("NACOILTYPE_3", GetType(String))               '油種_3
        O_TBL.Columns.Add("NACOILTYPENAME_3", GetType(String))           '油種名称_3
        O_TBL.Columns.Add("NACOILTYPE_4", GetType(String))               '油種_4
        O_TBL.Columns.Add("NACOILTYPENAME_4", GetType(String))           '油種名称_4
        O_TBL.Columns.Add("NACOILTYPE_5", GetType(String))               '油種_5
        O_TBL.Columns.Add("NACOILTYPENAME_5", GetType(String))           '油種名称_5
        O_TBL.Columns.Add("NACOILTYPE_6", GetType(String))               '油種_6
        O_TBL.Columns.Add("NACOILTYPENAME_6", GetType(String))           '油種名称_6
        O_TBL.Columns.Add("NACOILTYPE_7", GetType(String))               '油種_7
        O_TBL.Columns.Add("NACOILTYPENAME_7", GetType(String))           '油種名称_7
        O_TBL.Columns.Add("NACOILTYPE_8", GetType(String))               '油種_8
        O_TBL.Columns.Add("NACOILTYPENAME_8", GetType(String))           '油種名称_8
        O_TBL.Columns.Add("NACPRODUCT1_1", GetType(String))              '品名１_1
        O_TBL.Columns.Add("NACPRODUCT1NAME_1", GetType(String))          '品名１名称_1
        O_TBL.Columns.Add("NACPRODUCT1_2", GetType(String))              '品名１_2
        O_TBL.Columns.Add("NACPRODUCT1NAME_2", GetType(String))          '品名１名称_2
        O_TBL.Columns.Add("NACPRODUCT1_3", GetType(String))              '品名１_3
        O_TBL.Columns.Add("NACPRODUCT1NAME_3", GetType(String))          '品名１名称_3
        O_TBL.Columns.Add("NACPRODUCT1_4", GetType(String))              '品名１_4
        O_TBL.Columns.Add("NACPRODUCT1NAME_4", GetType(String))          '品名１名称_4
        O_TBL.Columns.Add("NACPRODUCT1_5", GetType(String))              '品名１_5
        O_TBL.Columns.Add("NACPRODUCT1NAME_5", GetType(String))          '品名１名称_5
        O_TBL.Columns.Add("NACPRODUCT1_6", GetType(String))              '品名１_6
        O_TBL.Columns.Add("NACPRODUCT1NAME_6", GetType(String))          '品名１名称_6
        O_TBL.Columns.Add("NACPRODUCT1_7", GetType(String))              '品名１_7
        O_TBL.Columns.Add("NACPRODUCT1NAME_7", GetType(String))          '品名１名称_7
        O_TBL.Columns.Add("NACPRODUCT1_8", GetType(String))              '品名１_8
        O_TBL.Columns.Add("NACPRODUCT1NAME_8", GetType(String))          '品名１名称_8
        O_TBL.Columns.Add("NACPRODUCT2_1", GetType(String))              '品名２_1
        O_TBL.Columns.Add("NACPRODUCT2NAME_1", GetType(String))          '品名２名称_1
        O_TBL.Columns.Add("NACPRODUCT2_2", GetType(String))              '品名２_2
        O_TBL.Columns.Add("NACPRODUCT2NAME_2", GetType(String))          '品名２名称_2
        O_TBL.Columns.Add("NACPRODUCT2_3", GetType(String))              '品名２_3
        O_TBL.Columns.Add("NACPRODUCT2NAME_3", GetType(String))          '品名２名称_3
        O_TBL.Columns.Add("NACPRODUCT2_4", GetType(String))              '品名２_4
        O_TBL.Columns.Add("NACPRODUCT2NAME_4", GetType(String))          '品名２名称_4
        O_TBL.Columns.Add("NACPRODUCT2_5", GetType(String))              '品名２_5
        O_TBL.Columns.Add("NACPRODUCT2NAME_5", GetType(String))          '品名２名称_5
        O_TBL.Columns.Add("NACPRODUCT2_6", GetType(String))              '品名２_6
        O_TBL.Columns.Add("NACPRODUCT2NAME_6", GetType(String))          '品名２名称_6
        O_TBL.Columns.Add("NACPRODUCT2_7", GetType(String))              '品名２_7
        O_TBL.Columns.Add("NACPRODUCT2NAME_7", GetType(String))          '品名２名称_7
        O_TBL.Columns.Add("NACPRODUCT2_8", GetType(String))              '品名２_8
        O_TBL.Columns.Add("NACPRODUCT2NAME_8", GetType(String))          '品名２名称_8

        O_TBL.Columns.Add("NACGSHABAN", GetType(String))                 '業務車番
        O_TBL.Columns.Add("NACSUPPLIERKBN", GetType(String))             '社有・庸車区分
        O_TBL.Columns.Add("NACSUPPLIERKBNNAME", GetType(String))         '社有・庸車区分名称
        O_TBL.Columns.Add("NACSUPPLIER", GetType(String))                '庸車会社
        O_TBL.Columns.Add("NACSUPPLIERNAME", GetType(String))            '庸車会社名称
        O_TBL.Columns.Add("NACSHARYOOILTYPE", GetType(String))           '車両登録油種
        O_TBL.Columns.Add("NACSHARYOOILTYPENAME", GetType(String))       '車両登録油種名称

        O_TBL.Columns.Add("NACSHARYOTYPE1", GetType(String))             '車両タイプ1
        O_TBL.Columns.Add("NACSHARYOTYPE1NAME", GetType(String))         '車両タイプ1名称
        O_TBL.Columns.Add("NACTSHABAN1", GetType(String))                '統一車番1
        O_TBL.Columns.Add("NACMANGMORG1", GetType(String))               '車両管理部署1
        O_TBL.Columns.Add("NACMANGMORG1NAME", GetType(String))           '車両管理部署1名称
        O_TBL.Columns.Add("NACMANGSORG1", GetType(String))               '車両設置部署1
        O_TBL.Columns.Add("NACMANGSORG1NAME", GetType(String))           '車両設置部署1名称
        O_TBL.Columns.Add("NACMANGUORG1", GetType(String))               '車両運用部署1
        O_TBL.Columns.Add("NACMANGUORG1NAME", GetType(String))           '車両運用部署1名称
        O_TBL.Columns.Add("NACBASELEASE1", GetType(String))              '車両所有1
        O_TBL.Columns.Add("NACBASELEASE1NAME", GetType(String))          '車両所有1名称
        O_TBL.Columns.Add("NACLICNPLTNOF1", GetType(String))             '登録番号1
        O_TBL.Columns.Add("NACSHARYOTYPE2", GetType(String))             '車両タイプ2
        O_TBL.Columns.Add("NACSHARYOTYPE2NAME", GetType(String))         '車両タイプ2名称
        O_TBL.Columns.Add("NACTSHABAN2", GetType(String))                '統一車番2
        O_TBL.Columns.Add("NACMANGMORG2", GetType(String))               '車両管理部署2
        O_TBL.Columns.Add("NACMANGMORG2NAME", GetType(String))           '車両管理部署2名称
        O_TBL.Columns.Add("NACMANGSORG2", GetType(String))               '車両設置部署2
        O_TBL.Columns.Add("NACMANGSORG2NAME", GetType(String))           '車両設置部署2名称
        O_TBL.Columns.Add("NACMANGUORG2", GetType(String))               '車両運用部署2
        O_TBL.Columns.Add("NACMANGUORG2NAME", GetType(String))           '車両運用部署2名称
        O_TBL.Columns.Add("NACBASELEASE2", GetType(String))              '車両所有2
        O_TBL.Columns.Add("NACBASELEASE2NAME", GetType(String))          '車両所有2名称
        O_TBL.Columns.Add("NACLICNPLTNOF2", GetType(String))             '登録番号2
        O_TBL.Columns.Add("NACSHARYOTYPE3", GetType(String))             '車両タイプ3
        O_TBL.Columns.Add("NACSHARYOTYPE3NAME", GetType(String))         '車両タイプ3名称
        O_TBL.Columns.Add("NACTSHABAN3", GetType(String))                '統一車番3
        O_TBL.Columns.Add("NACMANGMORG3", GetType(String))               '車両管理部署3
        O_TBL.Columns.Add("NACMANGMORG3NAME", GetType(String))           '車両管理部署3名称
        O_TBL.Columns.Add("NACMANGSORG3", GetType(String))               '車両設置部署3
        O_TBL.Columns.Add("NACMANGSORG3NAME", GetType(String))           '車両設置部署3名称
        O_TBL.Columns.Add("NACMANGUORG3", GetType(String))               '車両運用部署3
        O_TBL.Columns.Add("NACMANGUORG3NAME", GetType(String))           '車両運用部署3名称
        O_TBL.Columns.Add("NACBASELEASE3", GetType(String))              '車両所有3
        O_TBL.Columns.Add("NACBASELEASE3NAME", GetType(String))          '車両所有3名称
        O_TBL.Columns.Add("NACLICNPLTNOF3", GetType(String))             '登録番号3

        O_TBL.Columns.Add("NACCREWKBN", GetType(String))                 '正副区分
        O_TBL.Columns.Add("NACCREWKBNNAME", GetType(String))             '正副区分名称
        O_TBL.Columns.Add("NACSTAFFCODE", GetType(String))               '乗務員・従業員コード
        O_TBL.Columns.Add("NACSTAFFCODENAME", GetType(String))           '乗務員・従業員コード名称
        O_TBL.Columns.Add("NACSTAFFKBN", GetType(String))                '乗務員・社員区分
        O_TBL.Columns.Add("NACSTAFFKBNNAME", GetType(String))            '乗務員・社員区分名称
        O_TBL.Columns.Add("NACMORG", GetType(String))                    '乗務員・管理部署
        O_TBL.Columns.Add("NACMORGNAME", GetType(String))                '乗務員・管理部署名称
        O_TBL.Columns.Add("NACHORG", GetType(String))                    '乗務員・配属部署
        O_TBL.Columns.Add("NACHORGNAME", GetType(String))                '乗務員・配属部署名称
        O_TBL.Columns.Add("NACSORG", GetType(String))                    '乗務員・作業部署
        O_TBL.Columns.Add("NACSORGNAME", GetType(String))                '乗務員・作業部署名称
        O_TBL.Columns.Add("NACSTAFFCODE2", GetType(String))              '副乗務員・従業員コード
        O_TBL.Columns.Add("NACSTAFFCODE2NAME", GetType(String))          '副乗務員・従業員コード名称
        O_TBL.Columns.Add("NACSTAFFKBN2", GetType(String))               '副乗務員・社員区分
        O_TBL.Columns.Add("NACSTAFFKBN2NAME", GetType(String))           '副乗務員・社員区分名称
        O_TBL.Columns.Add("NACMORG2", GetType(String))                   '副乗務員・管理部署
        O_TBL.Columns.Add("NACMORG2NAME", GetType(String))               '副乗務員・管理部署名称
        O_TBL.Columns.Add("NACHORG2", GetType(String))                   '副乗務員・配属部署
        O_TBL.Columns.Add("NACHORG2NAME", GetType(String))               '副乗務員・配属部署名称
        O_TBL.Columns.Add("NACSORG2", GetType(String))                   '副乗務員・作業部署
        O_TBL.Columns.Add("NACSORG2NAME", GetType(String))               '副乗務員・作業部署名称

        O_TBL.Columns.Add("NACORDERNO", GetType(String))                 '受注番号
        O_TBL.Columns.Add("NACDETAILNO", GetType(String))                '明細№
        O_TBL.Columns.Add("NACTRIPNO", GetType(String))                  'トリップ
        O_TBL.Columns.Add("NACDROPNO", GetType(String))                  'ドロップ
        O_TBL.Columns.Add("NACSEQ", GetType(String))                     'SEQ
        O_TBL.Columns.Add("NACORDERORG", GetType(String))                '受注部署
        O_TBL.Columns.Add("NACORDERORGNAME", GetType(String))            '受注部署名称
        O_TBL.Columns.Add("NACSHIPORG", GetType(String))                 '配送部署
        O_TBL.Columns.Add("NACSHIPORGNAME", GetType(String))             '配送部署名称

        O_TBL.Columns.Add("NACSURYO1", GetType(String))                  '受注・数量1
        O_TBL.Columns.Add("NACTANI1", GetType(String))                   '受注・単位1
        O_TBL.Columns.Add("NACTANINAME1", GetType(String))               '受注・単位1名称
        O_TBL.Columns.Add("NACSURYO2", GetType(String))                  '受注・数量2
        O_TBL.Columns.Add("NACTANI2", GetType(String))                   '受注・単位2
        O_TBL.Columns.Add("NACTANINAME2", GetType(String))               '受注・単位2名称
        O_TBL.Columns.Add("NACSURYO3", GetType(String))                  '受注・数量3
        O_TBL.Columns.Add("NACTANI3", GetType(String))                   '受注・単位3
        O_TBL.Columns.Add("NACTANINAME3", GetType(String))               '受注・単位3名称
        O_TBL.Columns.Add("NACSURYO4", GetType(String))                  '受注・数量4
        O_TBL.Columns.Add("NACTANI4", GetType(String))                   '受注・単位4
        O_TBL.Columns.Add("NACTANINAME4", GetType(String))               '受注・単位4名称
        O_TBL.Columns.Add("NACSURYO5", GetType(String))                  '受注・数量5
        O_TBL.Columns.Add("NACTANI5", GetType(String))                   '受注・単位5
        O_TBL.Columns.Add("NACTANINAME5", GetType(String))               '受注・単位5名称
        O_TBL.Columns.Add("NACSURYO6", GetType(String))                  '受注・数量6
        O_TBL.Columns.Add("NACTANI6", GetType(String))                   '受注・単位6
        O_TBL.Columns.Add("NACTANINAME6", GetType(String))               '受注・単位6名称
        O_TBL.Columns.Add("NACSURYO7", GetType(String))                  '受注・数量7
        O_TBL.Columns.Add("NACTANI7", GetType(String))                   '受注・単位7
        O_TBL.Columns.Add("NACTANINAME7", GetType(String))               '受注・単位7名称
        O_TBL.Columns.Add("NACSURYO8", GetType(String))                  '受注・数量8
        O_TBL.Columns.Add("NACTANI8", GetType(String))                   '受注・単位8
        O_TBL.Columns.Add("NACTANINAME8", GetType(String))               '受注・単位8名称
        O_TBL.Columns.Add("NACSURYOG", GetType(String))                  '受注・数量合計(★追加)

        O_TBL.Columns.Add("NACJSURYO1", GetType(String))                 '実績・配送数量1
        O_TBL.Columns.Add("NACSTANI1", GetType(String))                  '実績・配送単位1
        O_TBL.Columns.Add("NACSTANINAME1", GetType(String))              '実績・配送単位1名称
        O_TBL.Columns.Add("NACJSURYO2", GetType(String))                 '実績・配送数量2
        O_TBL.Columns.Add("NACSTANI2", GetType(String))                  '実績・配送単位2
        O_TBL.Columns.Add("NACSTANINAME2", GetType(String))              '実績・配送単位2名称
        O_TBL.Columns.Add("NACJSURYO3", GetType(String))                 '実績・配送数量3
        O_TBL.Columns.Add("NACSTANI3", GetType(String))                  '実績・配送単位3
        O_TBL.Columns.Add("NACSTANINAME3", GetType(String))              '実績・配送単位3名称
        O_TBL.Columns.Add("NACJSURYO4", GetType(String))                 '実績・配送数量4
        O_TBL.Columns.Add("NACSTANI4", GetType(String))                  '実績・配送単位4
        O_TBL.Columns.Add("NACSTANINAME4", GetType(String))              '実績・配送単位4名称
        O_TBL.Columns.Add("NACJSURYO5", GetType(String))                 '実績・配送数量5
        O_TBL.Columns.Add("NACSTANI5", GetType(String))                  '実績・配送単位5
        O_TBL.Columns.Add("NACSTANINAME5", GetType(String))              '実績・配送単位5名称
        O_TBL.Columns.Add("NACJSURYO6", GetType(String))                 '実績・配送数量6
        O_TBL.Columns.Add("NACSTANI6", GetType(String))                  '実績・配送単位6
        O_TBL.Columns.Add("NACSTANINAME6", GetType(String))              '実績・配送単位6名称
        O_TBL.Columns.Add("NACJSURYO7", GetType(String))                 '実績・配送数量7
        O_TBL.Columns.Add("NACSTANI7", GetType(String))                  '実績・配送単位7
        O_TBL.Columns.Add("NACSTANINAME7", GetType(String))              '実績・配送単位7名称
        O_TBL.Columns.Add("NACJSURYO8", GetType(String))                 '実績・配送数量8
        O_TBL.Columns.Add("NACSTANI8", GetType(String))                  '実績・配送単位8
        O_TBL.Columns.Add("NACSTANINAME8", GetType(String))              '実績・配送単位8名称
        O_TBL.Columns.Add("NACJSURYOG", GetType(String))                 '実績・配送数量合計(★追加)

        O_TBL.Columns.Add("NACHAIDISTANCE", GetType(String))             '実績・配送距離
        O_TBL.Columns.Add("NACKAIDISTANCE", GetType(String))             '実績・下車作業距離
        O_TBL.Columns.Add("NACCHODISTANCE", GetType(String))             '実績・勤怠調整距離
        O_TBL.Columns.Add("NACTTLDISTANCE", GetType(String))             '実績・配送距離合計Σ

        O_TBL.Columns.Add("NACHAISTDATE", GetType(String))               '実績・配送作業開始日時
        O_TBL.Columns.Add("NACHAIENDDATE", GetType(String))              '実績・配送作業終了日時
        O_TBL.Columns.Add("NACHAIWORKTIME", GetType(String))             '実績・配送作業時間
        O_TBL.Columns.Add("NACGESSTDATE", GetType(String))               '実績・下車作業開始日時
        O_TBL.Columns.Add("NACGESENDDATE", GetType(String))              '実績・下車作業終了日時
        O_TBL.Columns.Add("NACGESWORKTIME", GetType(String))             '実績・下車作業時間
        O_TBL.Columns.Add("NACCHOWORKTIME", GetType(String))             '実績・勤怠調整時間
        O_TBL.Columns.Add("NACTTLWORKTIME", GetType(String))             '実績・配送合計時間Σ

        O_TBL.Columns.Add("NACOUTWORKTIME", GetType(String))             '実績・就業外時間

        O_TBL.Columns.Add("NACBREAKSTDATE", GetType(String))             '実績・休憩開始日時
        O_TBL.Columns.Add("NACBREAKENDDATE", GetType(String))            '実績・休憩終了日時
        O_TBL.Columns.Add("NACBREAKTIME", GetType(String))               '実績・休憩時間
        O_TBL.Columns.Add("NACCHOBREAKTIME", GetType(String))            '実績・休憩調整時間
        O_TBL.Columns.Add("NACTTLBREAKTIME", GetType(String))            '実績・休憩合計時間Σ

        O_TBL.Columns.Add("NACCASH", GetType(String))                    '実績・現金
        O_TBL.Columns.Add("NACETC", GetType(String))                     '実績・ETC
        O_TBL.Columns.Add("NACTICKET", GetType(String))                  '実績・回数券
        O_TBL.Columns.Add("NACKYUYU", GetType(String))                   '実績・軽油
        O_TBL.Columns.Add("NACUNLOADCNT", GetType(String))               '実績・荷卸回数
        O_TBL.Columns.Add("NACCHOUNLOADCNT", GetType(String))            '実績・荷卸回数調整
        O_TBL.Columns.Add("NACTTLUNLOADCNT", GetType(String))            '実績・荷卸回数合計Σ
        O_TBL.Columns.Add("NACKAIJI", GetType(String))                   '実績・回次
        O_TBL.Columns.Add("NACJITIME", GetType(String))                  '実績・実車時間
        O_TBL.Columns.Add("NACJICHOSTIME", GetType(String))              '実績・実車時間調整
        O_TBL.Columns.Add("NACJITTLETIME", GetType(String))              '実績・実車時間合計Σ
        O_TBL.Columns.Add("NACKUTIME", GetType(String))                  '実績・空車時間
        O_TBL.Columns.Add("NACKUCHOTIME", GetType(String))               '実績・空車時間調整
        O_TBL.Columns.Add("NACKUTTLTIME", GetType(String))               '実績・空車時間合計Σ
        O_TBL.Columns.Add("NACJKTTLTIME", GetType(String))               '実績・実車／空車時間合計Σ
        O_TBL.Columns.Add("NACJIDISTANCE", GetType(String))              '実績・実車距離
        O_TBL.Columns.Add("NACJICHODISTANCE", GetType(String))           '実績・実車距離調整
        O_TBL.Columns.Add("NACJITTLDISTANCE", GetType(String))           '実績・実車距離合計Σ
        O_TBL.Columns.Add("NACKUDISTANCE", GetType(String))              '実績・空車距離
        O_TBL.Columns.Add("NACKUCHODISTANCE", GetType(String))           '実績・空車距離調整
        O_TBL.Columns.Add("NACKUTTLDISTANCE", GetType(String))           '実績・空車距離合計Σ
        O_TBL.Columns.Add("NACJKTTLDISTANCE", GetType(String))           '実績・実車／空車距離合計Σ
        O_TBL.Columns.Add("NACTARIFFFARE", GetType(String))              '実績・運賃タリフ額
        O_TBL.Columns.Add("NACFIXEDFARE", GetType(String))               '実績・運賃固定額
        O_TBL.Columns.Add("NACINCHOFARE", GetType(String))               '実績・運賃手入力調整額
        O_TBL.Columns.Add("NACTTLFARE", GetType(String))                 '実績・運賃合計額Σ

        O_TBL.Columns.Add("NACOFFICESORG", GetType(String))              '実績・従業作業部署
        O_TBL.Columns.Add("NACOFFICESORGNAME", GetType(String))          '実績・従業作業部署名称
        O_TBL.Columns.Add("NACOFFICETIME", GetType(String))              '実績・従業時間
        O_TBL.Columns.Add("NACOFFICEBREAKTIME", GetType(String))         '実績・従業休憩時間
        O_TBL.Columns.Add("PAYSHUSHADATE", GetType(String))              '出社日時
        O_TBL.Columns.Add("PAYTAISHADATE", GetType(String))              '退社日時
        O_TBL.Columns.Add("PAYSTAFFKBN", GetType(String))                '社員区分
        O_TBL.Columns.Add("PAYSTAFFKBNNAME", GetType(String))            '社員区分名称
        O_TBL.Columns.Add("PAYSTAFFCODE", GetType(String))               '従業員
        O_TBL.Columns.Add("PAYSTAFFCODENAME", GetType(String))           '従業員名称
        O_TBL.Columns.Add("PAYMORG", GetType(String))                    '従業員管理部署
        O_TBL.Columns.Add("PAYMORGNAME", GetType(String))                '従業員管理部署名称
        O_TBL.Columns.Add("PAYHORG", GetType(String))                    '従業員配属部署
        O_TBL.Columns.Add("PAYHORGNAME", GetType(String))                '従業員配属部署名称


        O_TBL.Columns.Add("WORKKBN", GetType(String))                    'SYS作業区分
        O_TBL.Columns.Add("WORKKBNNAME", GetType(String))                'SYS作業区分名称
        O_TBL.Columns.Add("KEYSTAFFCODE", GetType(String))               'SYS従業員
        O_TBL.Columns.Add("KEYGSHABAN", GetType(String))                 'SYS業務車番
        O_TBL.Columns.Add("KEYTRIPNO", GetType(String))                  'SYSトリップ
        O_TBL.Columns.Add("KEYDROPNO", GetType(String))                  'SYSドロップ
        O_TBL.Columns.Add("KEYTSHABAN1", GetType(String))                'SYS統一車番1
        O_TBL.Columns.Add("KEYTSHABAN2", GetType(String))                'SYS統一車番2
        O_TBL.Columns.Add("KEYTSHABAN3", GetType(String))                'SYS統一車番3
    End Sub

    ''' <summary>
    ''' 部署コードの変換処理
    ''' </summary>
    ''' <param name="I_ORG">変換前部署コード</param>
    ''' <param name="O_ORG">変換後部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub ConvORGCode(ByVal I_ORG As String, ByRef O_ORG As String, ByRef O_RTN As String)

        O_ORG = I_ORG
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
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

                Using SQLcmd As New SqlCommand(SQLStr.ToString, SQLcon)
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
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MapRefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0004S Then                                                    '条件画面からの画面遷移
            Master.MAPID = GRTA0004WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
            '○Grid情報保存先のファイル名
            work.WF_SEL_XMLsaveF.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0004-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"
            work.WF_SEL_XMLsaveF2.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-TA0004INQ-" & Master.MAPvariant & "-" & Date.Now.ToString("HHmmss") & ".txt"

        End If

    End Sub

End Class




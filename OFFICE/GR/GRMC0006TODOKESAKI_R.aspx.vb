Imports System.Data.SqlClient
Imports System.IO
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 届先マスタ照会（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0006TODOKESAKI_R
    Inherits Page

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID

    Private BASEtbl As DataTable                                'Grid格納用テーブル
    Private INPtbl As DataTable                                 'Detail入力用テーブル
    Private UPDtbl As DataTable                                 '更新用テーブル
    Private PDFtbl As DataTable                                 'PDF Repeater格納用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0010CHARstr As New CS0010CHARget                  '例外文字排除 String Get
    Private CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
    Private CS0013PROFview As New CS0013ProfView                'テーブルオブジェクト作成
    Private CS0026TBLSORT As New CS0026TBLSORT                  '表示画面情報ソート
    Private CS0030REPORT As New CS0030REPORT                    '帳票出力(入力：TBL)
    Private CS0050Session As New CS0050SESSION                  'セッション管理
    Private CS0052DetailView As New CS0052DetailView            'Repeterオブジェクト作成

    Private GS0007FIXVALUElst As New GS0007FIXVALUElst          'Leftボックス用固定値リスト取得

    '共通処理結果
    Private WW_ERRCODE As String                                'サブ用リターンコード
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバ処理の遷移先
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
                    If Not Master.RecoverTable(BASEtbl) Then
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonCSV"
                            WF_ButtonCSV_Click()
                        Case "WF_ButtonPrint"
                            WF_Print_Click()
                        Case "WF_ButtonFIRST"
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"
                            WF_ButtonLAST_Click()
                        Case "WF_CLEAR"
                            WF_CLEAR_Click()
                        Case "WF_ButtonEND"
                            WF_ButtonEND_Click()
                        Case "WF_ButtonSel"
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"
                            WF_ButtonCan_Click()
                        Case "WF_Field_DBClick"
                            WF_Field_DBClick()
                        Case "WF_ListboxDBclick"
                            WF_Listbox_DBClick()
                        Case "WF_RadioButonClick"
                            WF_RadioButon_Click()
                        Case "WF_MEMOChange"
                            WF_MEMO_Change()
                        Case "WF_GridDBclick"
                            WF_Grid_DBclick()
                        Case "WF_MouseWheelDown"
                            WF_GRID_ScroleDown()
                        Case "WF_MouseWheelUp"
                            WF_GRID_ScroleUp()
                        Case "WF_MAP"
                            WF_MAP_Click()
                        Case "WF_DTAB_Click"
                            WF_Detail_TABChange()
                        Case "WF_DTAB_PDF_Click"
                            DTAB_PDFEXCELdisplay()
                        Case "WF_DTAB_PDF_Change"
                            PDF_EXCEL_SELECTchange()
                        Case Else
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
            If Not IsNothing(BASEtbl) Then
                BASEtbl.Clear()
                BASEtbl.Dispose()
                BASEtbl = Nothing
            End If

            If Not IsNothing(INPtbl) Then
                INPtbl.Clear()
                INPtbl.Dispose()
                INPtbl = Nothing
            End If

            If Not IsNothing(UPDtbl) Then
                UPDtbl.Clear()
                UPDtbl.Dispose()
                UPDtbl = Nothing
            End If

            If Not IsNothing(PDFtbl) Then
                PDFtbl.Clear()
                PDFtbl.Dispose()
                PDFtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = GRMC0006WRKINC.MAPID_R
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = False
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_TORINAME.Focus()
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        MAPDATAget()

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '一覧表示データ編集（性能対策）
        Using TBLview As DataView = New DataView(BASEtbl)
            TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DSPROWCOUNT
            CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013PROFview.PROFID = Master.PROF_VIEW
            CS0013PROFview.MAPID = Master.MAPID
            CS0013PROFview.VARI = Master.VIEWID
            CS0013PROFview.SRCDATA = TBLview.ToTable
            CS0013PROFview.TBLOBJ = pnlListArea
            CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
            CS0013PROFview.LEVENT = "ondblclick"
            CS0013PROFview.LFUNC = "ListDbClick"
            CS0013PROFview.TITLEOPT = True
            CS0013PROFview.CS0013ProfView()
        End Using
        If Not isNormal(CS0013PROFview.ERR) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○名称付与
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        WW_FIXVALUE("MC0006_PDF", WW_DUMMY, WF_Rep2_PDFselect)            'PDF選択ListBox設定   ★LeftBox以外

        '○DTab初期設定
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○Workディレクトリ削除
        PDF_INITdel()

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置（開始）
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For Each BASErow As DataRow In BASEtbl.Rows
            If BASErow("HIDDEN") = 0 Then
                WW_DataCNT = WW_DataCNT + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                BASErow("SELECT") = WW_DataCNT
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
        Dim WW_TBLview As DataView = New DataView(BASEtbl)

        'ソート
        WW_TBLview.Sort = "LINECNT"
        WW_TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DSPROWCOUNT).ToString()
        '一覧作成
        CS0013PROFview.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013PROFview.PROFID = Master.PROF_VIEW
        CS0013PROFview.MAPID = Master.MAPID
        CS0013PROFview.VARI = Master.VIEWID
        CS0013PROFview.SRCDATA = WW_TBLview.ToTable
        CS0013PROFview.TBLOBJ = pnlListArea
        CS0013PROFview.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.None
        CS0013PROFview.LEVENT = "ondblclick"
        CS0013PROFview.LFUNC = "ListDbClick"
        CS0013PROFview.TITLEOPT = True
        CS0013PROFview.CS0013ProfView()

        '○クリア
        If WW_TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = WW_TBLview.Item(0)("SELECT")
        End If
        WF_TORINAME.Focus()

    End Sub

    ''' <summary>
    ''' 一覧絞り込みボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonExtract_Click()

        '○絞り込み操作（GridView明細Hidden設定）
        For Each row As DataRow In BASEtbl.Rows
            '一度全部非表示化する
            row("HIDDEN") = 1

            '取引先名称　届先名称　分類　絞込判定
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text = "" Then
                row("HIDDEN") = 0
            End If

            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING As String = row("TORINAME")     '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_TORINAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING As String = row("NAMES")        '検索用文字列（部分一致）
                If WW_STRING.Contains(WF_TODOKENAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text <> "" Then
                If WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text = "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING2.Contains(WF_TODOKENAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text = "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text = "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WW_STRING2.Contains(WF_TODOKENAME.Text) Then
                    row("HIDDEN") = 0
                End If
            End If
            If WF_TORINAME.Text <> "" AndAlso WF_TODOKENAME.Text <> "" AndAlso WF_CLASS.Text <> "" Then
                Dim WW_STRING1 As String = row("TORINAME")    '検索用文字列（部分一致）
                Dim WW_STRING2 As String = row("NAMES")       '検索用文字列（部分一致）
                If WW_STRING1.Contains(WF_TORINAME.Text) AndAlso WW_STRING2.Contains(WF_TODOKENAME.Text) AndAlso WF_CLASS.Text = row("CLASS") Then
                    row("HIDDEN") = 0
                End If
            End If
        Next

        '画面先頭を表示
        WF_GridPosition.Text = "1"
        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_FILTER_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○カーソル設定
        WF_TORINAME.Focus()

    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン処理                                 ***
    ' ******************************************************************************
    ''' <summary>
    ''' 一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Print_Click()

        '○帳票出力
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.CS0030REPORT()

        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            End If
            Exit Sub
        End If

        '○別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)


    End Sub

    ' ******************************************************************************
    ' ***  ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン処理                                         ***
    ' ******************************************************************************
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCSV_Click()

        '○帳票出力dll Interface
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = BASEtbl                          'データ参照DataTable
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORTtbl")
            Exit Sub
        End If

        '○別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 先頭頁移動ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○先頭頁に移動
        WF_GridPosition.Text = "1"
    End Sub
    ''' <summary>
    ''' 最終頁遷移ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ソート
        Dim WW_TBLview As DataView
        WW_TBLview = New DataView(BASEtbl)
        WW_TBLview.RowFilter = "HIDDEN= '0'"

        '○最終頁に移動
        If WW_TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = WW_TBLview.Count - (WW_TBLview.Count Mod 10) + 1
        End If

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧の明細行ダブルクリック時処理(GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBclick()

        Dim WW_LINECNT As Integer
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_RTN As String = ""
        Dim WW_FILED_OBJ As Object

        '○LINECNT
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT = WW_LINECNT - 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○Grid内容(BASEtbl)よりDetail編集

        WF_Sel_LINECNT.Text = BASEtbl.Rows(WW_LINECNT)("LINECNT")

        '有効年月日
        WF_STYMD.Text = BASEtbl.Rows(WW_LINECNT)("STYMD")
        WF_ENDYMD.Text = BASEtbl.Rows(WW_LINECNT)("ENDYMD")

        '会社
        WF_CAMPCODE.Text = BASEtbl.Rows(WW_LINECNT)("CAMPCODE")
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WW_TEXT, WW_DUMMY)
        WF_CAMPCODE_TEXT.Text = WW_TEXT

        '取引先コード
        WF_TORICODE.Text = BASEtbl.Rows(WW_LINECNT)("TORICODE")
        CODENAME_get("TORICODE", WF_TORICODE.Text, WW_TEXT, WW_DUMMY, work.CreateTORIParam(WF_CAMPCODE.Text))
        WF_TORICODE_TEXT.Text = WW_TEXT

        '届先コード
        WF_TODOKECODE.Text = BASEtbl.Rows(WW_LINECNT)("TODOKECODE")
        CODENAME_get("TODOKECODE", WF_TODOKECODE.Text, WW_TEXT, WW_DUMMY)
        WF_TODOKECODE_TEXT.Text = WW_TEXT

        '削除フラグ
        WF_DELFLG.Text = BASEtbl.Rows(WW_LINECNT)("DELFLG")
        CODENAME_get("DELFLG", WF_DELFLG.Text, WW_TEXT, WW_DUMMY)
        WF_DELFLG_TEXT.Text = WW_TEXT

        '○Grid設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_1"), Label).Text = WW_TEXT
            End If

            '中央
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = WW_TEXT
            End If

            '右
            WW_FILED_OBJ = CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label)
            If WW_FILED_OBJ.Text <> "" Then
                '値設定
                WW_VALUE = REP_ITEM_FORMAT(WW_FILED_OBJ.text, BASEtbl.Rows(WW_LINECNT)(WW_FILED_OBJ.Text))
                CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text = WW_VALUE
                '値（名称）設定
                CODENAME_get(WW_FILED_OBJ.Text, WW_VALUE, WW_TEXT, WW_DUMMY)
                CType(reitem.FindControl("WF_Rep1_VALUE_TEXT_3"), Label).Text = WW_TEXT
            End If
        Next

        '○タブ別処理(2 書類（PDF）)
        PDF_EXCEL_INITread(WF_CAMPCODE.Text, WF_TORICODE.Text, WF_TODOKECODE.Text)

        '○画面WF_GRID状態設定
        '状態をクリア設定
        For Each BASErow As DataRow In BASEtbl.Rows
            Select Case BASErow("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case BASEtbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                BASEtbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
            Case Else
        End Select

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        WF_GridDBclick.Text = ""

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○LeftBox処理（フィールドダブルクリック時）
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As New Hashtable
                    'フィールドによってパラメーターを変える
                    Select Case WW_FIELD
                        Case "WF_CLASS"             '分類
                            prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CLASS")
                        Case "WF_Rep_DELFLG"        '削除フラグ
                            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                    End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                End If
            End With
        End If
    End Sub

    ''' <summary>
    ''' 左リストボックスダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Listbox_DBClick()
        WF_ButtonSel_Click()
    End Sub
    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '○RightBox処理（ラジオボタン選択）
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
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '○RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleDown()

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_ScroleUp()

    End Sub
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_GRID_Scrole()

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面をテーブルデータに退避する
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToINPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Master.CreateEmptyTable(INPtbl)
        Dim INProw As DataRow = INPtbl.NewRow

        For Each INPcol As DataColumn In INPtbl.Columns
            If IsDBNull(INProw.Item(INPcol)) OrElse IsNothing(INProw.Item(INPcol)) Then
                Select Case INPcol.ColumnName
                    Case "LINECNT"
                        INProw.Item(INPcol) = 0
                    Case "TIMSTP"
                        INProw.Item(INPcol) = 0
                    Case "SELECT"
                        INProw.Item(INPcol) = 1
                    Case "HIDDEN"
                        INProw.Item(INPcol) = 0
                    Case Else
                        INProw.Item(INPcol) = ""
                End Select
            End If
        Next

        If WF_Sel_LINECNT.Text = "" Then
            INProw("LINECNT") = 0
        Else
            INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_TORICODE.Text)          '取引先コード
        Master.EraseCharToIgnore(WF_TODOKECODE.Text)        '届先コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '終了年月日
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        INProw("CAMPCODE") = WF_CAMPCODE.Text
        INProw("TORICODE") = WF_TORICODE.Text
        INProw("TODOKECODE") = WF_TODOKECODE.Text
        INProw("STYMD") = WF_STYMD.Text
        INProw("ENDYMD") = WF_ENDYMD.Text
        INProw("DELFLG") = WF_DELFLG.Text

        'GridViewから未選択状態で表更新ボタンを押下時の例外を回避する 
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_TORICODE.Text) AndAlso
            String.IsNullOrEmpty(WF_TODOKECODE.Text) AndAlso
            String.IsNullOrEmpty(WF_STYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_ENDYMD.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "non Detail")
            CS0011LOGWRITE.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "non Detail"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "non Detail"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR

            Exit Sub
        End If

        '○Detail設定処理
        For Each reitem As RepeaterItem In WF_DViewRep1.Items
            '左
            If CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_1"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_1"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '中央
            If CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_2"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_2"), Label).Text) = CS0010CHARstr.CHAROUT
            End If

            '右
            If CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                CS0010CHARstr.CHARIN = CType(reitem.FindControl("WF_Rep1_VALUE_3"), TextBox).Text
                CS0010CHARstr.CS0010CHARget()
                INProw(CType(reitem.FindControl("WF_Rep1_FIELD_3"), Label).Text) = CS0010CHARstr.CHAROUT
            End If
        Next

        '○名称付与
        '会社名称
        INProw("CAMPNAME") = ""
        CODENAME_get("CAMPCODE", INProw("CAMPCODE"), INProw("CAMPNAME"), WW_DUMMY)
        '取引先名称
        INProw("TORINAME") = ""
        CODENAME_get("TORICODE", INProw("TORICODE"), INProw("TORINAME"), WW_DUMMY)
        '市町村名称
        INProw("CITIESNAME") = ""
        CODENAME_get("CITIES", INProw("CITIES"), INProw("CITIESNAME"), WW_DUMMY)
        '管理部署名称
        INProw("MORGNAME") = ""
        CODENAME_get("MORG", INProw("MORG"), INProw("MORGNAME"), WW_DUMMY)
        '運用部署名称
        INProw("UORGNAME") = ""
        CODENAME_get("UORG", INProw("UORG"), INProw("UORGNAME"), WW_DUMMY)
        '分類名称
        INProw("CLASSNAME") = ""
        CODENAME_get("CLASS", INProw("CLASS"), INProw("CLASSNAME"), WW_DUMMY)

        INPtbl.Rows.Add(INProw)

    End Sub

    ' *** 詳細画面-クリアボタン処理
    ''' <summary>
    ''' 詳細画面-クリアボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        For Each BASErow As DataRow In BASEtbl.Rows
            Select Case BASErow("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    BASErow("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○画面表示データ保存
        Master.SaveTable(BASEtbl)

        '○detailboxヘッダークリア
        WF_Sel_LINECNT.Text = ""
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        WF_TORICODE.Text = ""
        WF_TORICODE_TEXT.Text = ""
        WF_TODOKECODE.Text = ""
        WF_TODOKECODE_TEXT.Text = ""
        WF_STYMD.Text = ""
        WF_ENDYMD.Text = ""
        WF_DELFLG.Text = ""
        WF_DELFLG_TEXT.Text = ""

        '○名称付与
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        '○Detail初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"
        WF_Detail_TABChange()

        '○PDF初期画面編集
        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        'メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        'カーソル設定
        WF_STYMD.Focus()

    End Sub

    ''' <summary>
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()

        Dim dataTable As DataTable = New DataTable
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        Try
            WF_TORICODE.ReadOnly = True
            WF_TORICODE.Style.Add("background-color", "rgb(213,208,181)")
            WF_TODOKECODE.ReadOnly = True
            WF_TODOKECODE.Style.Add("background-color", "rgb(213,208,181)")
            WF_STYMD.ReadOnly = True
            WF_STYMD.Style.Add("background-color", "rgb(213,208,181)")
            WF_ENDYMD.ReadOnly = True
            WF_ENDYMD.Style.Add("background-color", "rgb(213,208,181)")
            WF_DELFLG.ReadOnly = True
            WF_DELFLG.Style.Add("background-color", "rgb(213,208,181)")

            'カラム情報をリピーター作成用に取得
            Master.CreateEmptyTable(dataTable)
            dataTable.Rows.Add(dataTable.NewRow())

            'リピーター作成
            CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0052DetailView.PROFID = Master.PROF_VIEW
            CS0052DetailView.MAPID = Master.MAPID
            CS0052DetailView.VARI = Master.VIEWID
            CS0052DetailView.TABID = CONST_DETAIL_TABID
            CS0052DetailView.SRCDATA = dataTable
            CS0052DetailView.REPEATER = WF_DViewRep1
            CS0052DetailView.COLPREFIX = "WF_Rep1_"
            CS0052DetailView.MaketDetailView()
            If Not isNormal(CS0052DetailView.ERR) Then
                Exit Sub
            End If

            WF_DetailMView.ActiveViewIndex = 0

            WF_DViewRep1.Visible = True

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT)
        Finally
            dataTable.Dispose()
            dataTable = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-タブ切替処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        '初期値（書式）変更

        '管理
        WF_Dtab01.Style.Remove("color")
        WF_Dtab01.Style.Add("color", "black")
        WF_Dtab01.Style.Remove("background-color")
        WF_Dtab01.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab01.Style.Remove("border")
        WF_Dtab01.Style.Add("border", "1px solid black")
        WF_Dtab01.Style.Remove("font-weight")
        WF_Dtab01.Style.Add("font-weight", "normal")

        '申請書類（PDF） 
        WF_Dtab02.Style.Remove("color")
        WF_Dtab02.Style.Add("color", "black")
        WF_Dtab02.Style.Remove("background-color")
        WF_Dtab02.Style.Add("background-color", "rgb(255,255,253)")
        WF_Dtab02.Style.Remove("border")
        WF_Dtab02.Style.Add("border", "1px solid black")
        WF_Dtab02.Style.Remove("font-weight")
        WF_Dtab02.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '管理
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            Case 1
                '申請書類（PDF） 
                WF_Dtab02.Style.Remove("color")
                WF_Dtab02.Style.Add("color", "blue")
                WF_Dtab02.Style.Remove("background-color")
                WF_Dtab02.Style.Add("background-color", "rgb(220,230,240)")
                WF_Dtab02.Style.Remove("border")
                WF_Dtab02.Style.Add("border", "1px solid blue")
                WF_Dtab02.Style.Remove("font-weight")
                WF_Dtab02.Style.Add("font-weight", "bold")
        End Select

    End Sub

    ''' <summary>
    ''' 詳細画面-地図表示ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MAP_Click()

        '○エラーレポート準備
        rightview.SetErrorReport("")

        '○DetailBoxをINPtblへ退避
        DetailBoxToINPtbl(WW_ERRCODE)
        If Not isNormal(WW_ERRCODE) Then
            Exit Sub
        End If

        Dim WW_URL As String
        Dim WW_LATITUDEL As String
        Dim WW_LONGITUDE As String

        Dim INProw As DataRow = INPtbl(0)

        If WF_Sel_LINECNT.Text = "" Then
            INProw("LINECNT") = 0
        Else
            INProw("LINECNT") = WF_Sel_LINECNT.Text
        End If
        '○項目チェック
        If String.IsNullOrEmpty(INProw("LATITUDE")) OrElse
           String.IsNullOrEmpty(INProw("LONGITUDE")) Then

            '緯度、経度のどちらかが未入力の場合、「ゲートシティ大崎」を表示
            WW_LATITUDEL = "35.619397"
            WW_LONGITUDE = "139.730808"
        Else
            '入力値を設定
            WW_LATITUDEL = INProw("LATITUDE")
            WW_LONGITUDE = INProw("LONGITUDE")

        End If

        '地図表示
        WW_URL = "http:" & "//maps.google.co.jp/maps?ll=" & WW_LATITUDEL & "," & WW_LONGITUDE & "&spn=0.002,0.002&t=m&q=" & WW_LATITUDEL & "," & WW_LONGITUDE
        ClientScript.RegisterStartupScript(Me.GetType, "OpenNewWindow", "<script language=""javascript"">window.open(' " & WW_URL & "', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>")

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBOX選択ボタン処理(ListBox値 ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_CLASS"
                    '分類
                    WF_CLASS.Text = WW_SelectValue
                    WF_CLASS_TEXT.Text = WW_SelectTEXT
                    WF_CLASS.Focus()
            End Select
        Else
            '○ディテール02（PDF）変数設定 
            If WF_FIELD_REP.Value = "WF_Rep_DELFLG" Then
                For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
                    If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_FIELD.Value Then
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = WW_SelectValue
                        CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Focus()
                        Exit For
                    End If
                Next
            End If
        End If

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ' ******************************************************************************
    ' ***  PDF関連処理                                                           *** 
    ' ******************************************************************************

    ''' <summary>
    ''' PDF Tempディレクトリ削除(PAGE_load時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_INITdel()

        Dim WW_UPdirs As String()
        Dim WW_UPfiles As String()

        'Temp納ディレクトリ編集
        '○PDF格納Dir作成
        '   一時保存のPDFフォルダ
        '       c:\paal\applpdf\MC0006_TODOKESAKI\Temp\ユーザID\Update_H 
        '       c:\paal\applpdf\MC0006_TODOKESAKI\Temp\ユーザID\Delete_D 

        Dim WW_Dir As String = ""
        WW_Dir = WW_Dir & CS0050Session.PDF_PATH
        WW_Dir = WW_Dir & "\MC0006_TODOKESAKI\Temp\" & CS0050Session.USERID

        Dim WW_Dir_del As New List(Of String)

        'ディレクトリが存在しない場合、作成する
        If Not Directory.Exists(WW_Dir) Then
            Directory.CreateDirectory(WW_Dir)
        End If

        '○PDF格納ディレクトリ＞MC0006_TODOKESAKI\Temp\ユーザIDフォルダ内のファイル取得
        WW_UPdirs = Directory.GetDirectories(WW_Dir, "*", SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPdirs
            'Tempの自ユーザ内フォルダを取得
            WW_Dir_del.Add(tempFile)
        Next

        'Listを降順に並べる⇒下位ディレクトリが先頭となる
        WW_Dir_del.Reverse()

        For i As Integer = 0 To WW_Dir_del.Count - 1
            'フォルダー内ファイル削除
            WW_UPfiles = Directory.GetFiles(WW_Dir_del.Item(i), "*", SearchOption.AllDirectories)
            'フォルダー内ファイル削除
            For Each tempFile As String In WW_UPfiles
                'ファイル削除
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                    '読み取り専用などは削除できない
                End Try
            Next

            Try
                'ファイル削除
                Directory.Delete(WW_Dir_del.Item(i))
            Catch ex As Exception
                'ファイルが残っている場合、削除できない
            End Try
        Next

    End Sub

    ''' <summary>
    ''' PDF初期
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_Repeater_INIT()
        '○初期設定
        Dim WW_Dir As String

        '○画面編集
        '○PDF格納ディレクトリ編集   
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
                Dim WW_tempFile As String = tempFile
                Do
                    If InStr(WW_tempFile, "\") > 0 Then
                        'ファイル名編集
                        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                    End If

                    If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                        'ファイルパス格納
                        WW_Files_dir.Add(tempFile)
                        'ファイル名格納
                        WW_Files_name.Add(WW_tempFile)
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        CType(WF_ListBoxPDF, ListBox).Items.Clear()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1
            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.ALIVE
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)

            WF_ListBoxPDF.Items.Add(New ListItem(WW_Files_name.Item(i), C_DELETE_FLG.ALIVE))
        Next

        '○イベント設定
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub
    ''' <summary>
    ''' PDF読み込み・ディレクトリ作成(Header・一覧ダブルクリック時)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_EXCEL_INITread(ByVal I_CAMPCODE As String, ByVal I_TORICODE As String, ByVal I_TODOKECODE As String)

        Dim WW_UPfiles As String()

        '(説明1) フォルダ説明
        '　①一覧明細選択～表追加直前のPDF操作内容：Temp\会社コード_届先コード_nn\Update_Dフォルダに格納
        '　②表追加によるPDF一時保存内容　　　　　：Temp\会社コード_届先コード_nn\Update_Hフォルダに格納
        '　③正式PDF登録内容　　　　　　　　　　　：正式PDFフォルダ

        '(説明2) イベント別処理内容　　…　処理効率は悪いが、操作がシンプルとなる為、下記処理とした。
        '　①Page_Load時：PDF_INITdel
        '　　　　・Tempフォルダ(Update_D・Update_H)をお掃除
        '　②一覧ダブルクリック時：PDF_EXCEL_INITread
        '　　　　・Update_Hが存在しない場合、Update_Hフォルダ作成＆正式フォルダ内全PDF→Update_Hフォルダへコピー
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・Update_Dが存在する場合、Update_Dフォルダ内PDFを全て削除　＆　Update_Dフォルダ削除
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・Update_Dフォルダ作成 ＆ Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
        '　　　　　注意１…PDF明細選択01～15全てを対象
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　③Detail操作（PDF表示選択切替）時：PDF_EXCEL_SELECTchange
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　④Detail操作（クリアボタン押下）時：WF_CLEAR_Click
        '　　　　・クリア処理（Update_Dクリア）＆明細クリア表示
        '　⑤Detail操作（表追加ボタン押下）時：PDF_SAVE_H
        '　　　　・表示PDFに対し削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　　　　・Update_Hフォルダ内容をクリア（PDF明細選択01～15全てを対象)
        '　　　　・Update_Dフォルダ内PDFをUpdate_Hフォルダに全てコピー（PDF明細選択01～15全てを対象)
        '　　　　・Update_Dフォルダ内PDFを全て削除（PDF明細選択01～15全てを対象)
        '　⑥PDFアップロード時：UPLOAD_PDF_EXCEL
        '　　　　・Update_Dフォルダに該当PDFを格納
        '　　　　・（PDF明細選択に従い）Update_Dフォルダ内容を表示
        '　⑦DB更新ボタン押下時：★★★
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_Dをお掃除(Update_Hフォルダは連続入力に備えクリアしない)
        '　⑧Detail操作（有効開始変更)時：PDF_EXCEL_INITread

        rightview.SetErrorReport("")

        '○初期設定
        Dim WW_Dir As String

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If I_CAMPCODE = "" OrElse I_TORICODE = "" OrElse I_TODOKECODE = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If I_TORICODE = BASEtbl.Rows(i)("TORICODE") OrElse I_TODOKECODE = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○フォルダ作成　＆　ファイルコピー
        '○PDF格納Dir作成
        '   正式登録のPDFフォルダ
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn   　　　       　　　　 (nn:PDF書類種類)
        '   一時保存のPDFフォルダ
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn\Temp\ユーザID\Update_H  (nn:PDF書類種類)
        '       c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn\Temp\ユーザID\Delete_D  (nn:PDF書類種類)


        'c:\appl\applpdf\MC0006_TODOKESAKIフォルダは必ず存在...下位フォルダ処理を行う

        For i As Integer = 1 To 3
            '○PDF格納ディレクトリ編集    c:\appl\applpdf\MC0006_TODOKESAKI\会社コード_届先コード_nn
            WW_Dir = ""
            WW_Dir = WW_Dir & CS0050Session.PDF_PATH
            WW_Dir = WW_Dir & "\MC0006_TODOKESAKI"

            '○正式ディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp") Then
                Directory.CreateDirectory(WW_Dir & "\Temp")
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID)
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ＞届先コードディレクトリ作成
            If Not Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00")) Then
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"))
            End If

            '○一時保存ディレクトリ＞届先コードディレクトリ作成＞Update_H の処理
            If Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H") Then
                '連続処理の場合、前回処理を残す
            Else
                'ユーザIDディレクトリ＞届先コードディレクトリ作成＞Update_H 作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H")

                '正式フォルダ内ファイル→一時保存ディレクトリ＞届先コードディレクトリ作成＞Update_H へコピー
                WW_UPfiles = Directory.GetFiles(WW_Dir & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00"), "*", SearchOption.AllDirectories)

                For Each tempFile As String In WW_UPfiles
                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim WW_File As String = tempFile
                    Do
                        If InStr(WW_File, "\") > 0 Then
                            WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                        End If
                    Loop Until InStr(WW_File, "\") <= 0

                    '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
                    File.Copy(tempFile, WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H\" & WW_File, True)
                Next
            End If

            '○一時保存ディレクトリ＞ユーザIDディレクトリ作成＞届先コードディレクトリ作成＞Update_D 処理
            If Directory.Exists(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D") Then
                'Update_Dフォルダ内ファイル削除
                WW_UPfiles = Directory.GetFiles(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D", "*", SearchOption.AllDirectories)
                For Each tempFile As String In WW_UPfiles
                    Try
                        File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            Else
                'Update_Dが存在しない場合、Update_Dフォルダ作成
                Directory.CreateDirectory(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D")
            End If

            'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
            WW_UPfiles = Directory.GetFiles(WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_H", "*", SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = tempFile
                Do
                    If InStr(WW_File, "\") > 0 Then
                        WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 100)
                    End If
                Loop Until InStr(WW_File, "\") <= 0

                'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
                File.Copy(tempFile, WW_Dir & "\Temp\" & CS0050Session.USERID & "\" & I_CAMPCODE & "_" & I_TODOKECODE & "_" & i.ToString("00") & "\Update_D\" & WW_File, True)
            Next
        Next

        '○画面編集
        '○PDF格納ディレクトリ編集
        If WF_Rep2_PDFselect.SelectedValue.ToString() = "" Then
            WF_Rep2_PDFselect.SelectedIndex = 0
        End If

        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & I_CAMPCODE & "_" & I_TODOKECODE & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル一覧
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
                Dim WW_tempFile As String = tempFile
                Do
                    If InStr(WW_tempFile, "\") > 0 Then
                        'ファイル名編集
                        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                    End If

                    If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                        'ファイルパス格納
                        WW_Files_dir.Add(tempFile)
                        'ファイル名格納
                        WW_Files_name.Add(WW_tempFile)
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        CType(WF_ListBoxPDF, ListBox).Items.Clear()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1
            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.ALIVE
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)

            WF_ListBoxPDF.Items.Add(New ListItem(WW_Files_name.Item(i), C_DELETE_FLG.ALIVE))
        Next

        '○イベント設定
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF表示内容変更時処理（Detail・PDFタブ内のListBox切替時）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDF_EXCEL_SELECTchange()

        '○初期設定
        Dim WW_Dir As String

        rightview.SetErrorReport("")

        '○事前確認
        '届先コードの存在確認（一覧に存在する事）
        If WF_CAMPCODE.Text = "" OrElse WF_TORICODE.Text = "" OrElse WF_TODOKECODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If WF_TORICODE.Text = BASEtbl.Rows(i)("TORICODE") OrElse WF_TODOKECODE.Text = BASEtbl.Rows(i)("TODOKECODE") Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        Master.Output(C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR, C_MESSAGE_TYPE.ABORT, "届先コード")
                        Exit Sub
                    End If
                End If
            Next
        End If

        '○削除処理
        '○Detail・表示PDFが、削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　※WF_Rep_FILEPATHは、Update_Dフォルダ内該当PDFを示す。

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.DELETE Then

                Try
                    File.Delete(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        '○画面編集
        '○PDF格納ディレクトリ編集   
        WW_Dir = CS0050Session.PDF_PATH & "\MC0006_TODOKESAKI\Temp\"
        WW_Dir = WW_Dir & CS0050Session.USERID & "\"
        WW_Dir = WW_Dir & WF_CAMPCODE.Text & "_" & WF_TODOKECODE.Text & "_" & WF_Rep2_PDFselect.SelectedValue.ToString() & "\Update_D"

        '○表追加前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In Directory.GetFiles(WW_Dir, "*", SearchOption.AllDirectories)
            If Right(tempFile, 4).ToUpper() = ".PDF" OrElse
               Right(tempFile, 4).ToUpper() = ".XLS" OrElse
               Right(tempFile, 5).ToUpper() = ".XLSX" Then
                Dim WW_tempFile As String = tempFile
                Do
                    If InStr(WW_tempFile, "\") > 0 Then
                        'ファイル名編集
                        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                    End If

                    If InStr(WW_tempFile, "\") = 0 AndAlso WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                        'ファイルパス格納
                        WW_Files_dir.Add(tempFile)
                        'ファイル名格納
                        WW_Files_name.Add(WW_tempFile)
                        '削除フラグ格納
                        WW_Files_del.Add("0")
                        Exit Do
                    End If

                Loop Until InStr(WW_tempFile, "\") = 0
            End If
        Next

        'Repeaterバインド準備
        PDFtbl_ColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            Dim PDFrow As DataRow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = C_DELETE_FLG.ALIVE
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1
            'ファイル記号名称
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = C_DELETE_FLG.ALIVE
            'FILEPATH
            CType(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)
        Next

        '○イベント設定
        Dim WW_ATTR As String = ""
        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "REF_Field_DBclick('WF_Rep_DELFLG' "
            WW_ATTR = WW_ATTR & ", '" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text & "'"
            WW_ATTR = WW_ATTR & ", " & LIST_BOX_CLASSIFICATION.LC_DELFLG & ")"
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            CType(reitem.FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub

    ''' <summary>
    ''' PDF 内容表示（Detail・PDFダブルクリック時（内容照会））
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DTAB_PDFEXCELdisplay()

        Dim WW_Dir As String = CS0050Session.UPLOAD_PATH & "\PRINTWORK\" & CS0050Session.TERMID

        For Each reitem As RepeaterItem In WF_DViewRepPDF.Items
            'ダブルクリック時コード検索イベント追加
            If CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text = WF_DTAB_PDF_DISP_FILE.Value Then
                'ディレクトリが存在しない場合、作成する
                If Not Directory.Exists(WW_Dir) Then
                    Directory.CreateDirectory(WW_Dir)
                End If

                'ダウンロードファイル送信準備
                File.Copy(CType(reitem.FindControl("WF_Rep_FILEPATH"), Label).Text,
                            WW_Dir & "\" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text,
                            True)

                'ダウンロード処理へ遷移
                WF_PrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/print/" & CS0050Session.TERMID & "/" & CType(reitem.FindControl("WF_Rep_FILENAME"), Label).Text
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

                Exit For
            End If
        Next

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面データ取得
    ''' </summary>
    ''' <remarks>データベースを検索し画面表示する一覧を作成する</remarks>
    Protected Sub MAPDATAget()

        '○画面表示用データ取得
        '取引先内容検索
        Try
            '○GridView内容をテーブル退避
            'テンポラリDB項目作成
            If IsNothing(BASEtbl) Then
                BASEtbl = New DataTable
            End If

            If BASEtbl.Columns.Count <> 0 Then
                BASEtbl.Columns.Clear()
            End If

            '○DB項目クリア
            BASEtbl.Clear()

            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      " SELECT                                                              " _
                    & "       0                                         as LINECNT ,        " _
                    & "       ''                                        as OPERATION ,      " _
                    & "       TIMSTP = cast(isnull(D.UPDTIMSTP,0)       as bigint) ,        " _
                    & "       1                                         as 'SELECT' ,       " _
                    & "       0                                         as HIDDEN ,         " _
                    & "       isnull(rtrim(D.CAMPCODE),'')              as CAMPCODE ,       " _
                    & "       isnull(rtrim(D.TORICODE),'')              as TORICODE ,       " _
                    & "       isnull(rtrim(D.TODOKECODE),'')            as TODOKECODE ,     " _
                    & "       isnull(format(D.STYMD, 'yyyy/MM/dd'),'')  as STYMD ,          " _
                    & "       isnull(format(D.ENDYMD, 'yyyy/MM/dd'),'') as ENDYMD ,         " _
                    & "       isnull(rtrim(D.NAMES),'')                 as NAMES ,          " _
                    & "       isnull(rtrim(D.NAMEL),'')                 as NAMEL ,          " _
                    & "       isnull(rtrim(D.NAMESK),'')                as NAMESK ,         " _
                    & "       isnull(rtrim(D.NAMELK),'')                as NAMELK ,         " _
                    & "       isnull(rtrim(D.POSTNUM1),'')              as POSTNUM1 ,       " _
                    & "       isnull(rtrim(D.POSTNUM2),'')              as POSTNUM2 ,       " _
                    & "       isnull(rtrim(D.ADDR1),'')                 as ADDR1 ,          " _
                    & "       isnull(rtrim(D.ADDR2),'')                 as ADDR2 ,          " _
                    & "       isnull(rtrim(D.ADDR3),'')                 as ADDR3 ,          " _
                    & "       isnull(rtrim(D.ADDR4),'')                 as ADDR4 ,          " _
                    & "       isnull(rtrim(D.TEL),'')                   as TEL ,            " _
                    & "       isnull(rtrim(D.FAX),'')                   as FAX ,            " _
                    & "       isnull(rtrim(D.MAIL),'')                  as MAIL ,           " _
                    & "       isnull(rtrim(D.LATITUDE),'')              as LATITUDE ,       " _
                    & "       isnull(rtrim(D.LONGITUDE),'')             as LONGITUDE ,      " _
                    & "       isnull(rtrim(D.CITIES),'')                as CITIES ,         " _
                    & "       isnull(rtrim(D.MORG),'')                  as MORG ,           " _
                    & "       isnull(rtrim(D.NOTES1),'')                as NOTES1 ,         " _
                    & "       isnull(rtrim(D.NOTES2),'')                as NOTES2 ,         " _
                    & "       isnull(rtrim(D.NOTES3),'')                as NOTES3 ,         " _
                    & "       isnull(rtrim(D.NOTES4),'')                as NOTES4 ,         " _
                    & "       isnull(rtrim(D.NOTES5),'')                as NOTES5 ,         " _
                    & "       isnull(rtrim(D.NOTES6),'')                as NOTES6 ,         " _
                    & "       isnull(rtrim(D.NOTES7),'')                as NOTES7 ,         " _
                    & "       isnull(rtrim(D.NOTES8),'')                as NOTES8 ,         " _
                    & "       isnull(rtrim(D.NOTES9),'')                as NOTES9 ,         " _
                    & "       isnull(rtrim(D.NOTES10),'')               as NOTES10 ,        " _
                    & "       isnull(rtrim(D.CLASS),'')                 as CLASS ,          " _
                    & "       rtrim(D.DELFLG)                           as DELFLG ,         " _
                    & "       ''                                        as INITYMD     ,    " _
                    & "       ''                                        as UPDYMD      ,    " _
                    & "       ''                                        as UPDUSER     ,    " _
                    & "       ''                                        as UPDTERMID   ,    " _
                    & "       ''                                        as RECEIVEYMD  ,    " _
                    & "       ''                                        as UPDTIMSTP ,      " _
                    & "       ''                                        as CAMPNAME ,       " _
                    & "       ''                                        as TORINAME ,       " _
                    & "       ''                                        as CITIESNAME  ,    " _
                    & "       ''                                        as MORGNAME ,       " _
                    & "       ''                                        as UORGNAME ,       " _
                    & "       ''                                        as CLASSNAME,       " _
                    & "       isnull(rtrim(C.UORG),'')                  as UORG ,           " _
                    & "       isnull(rtrim(C.ARRIVTIME) ,'')            as ARRIVTIME ,      " _
                    & "       isnull(rtrim(C.DISTANCE),'')              as DISTANCE ,       " _
                    & "       isnull(rtrim(C.SEQ),'')                   as SEQ ,            " _
                    & "       isnull(rtrim(C.YTODOKECODE),'')           as YTODOKECODE      " _
                    & "  FROM MC006_TODOKESAKI D                             " _
                    & " INNER JOIN S0006_ROLE B                              " _
                    & "         ON B.CAMPCODE    = @P1                       " _
                    & "        and B.OBJECT      = @P5                       " _
                    & "        and B.ROLE        = @P6                       " _
                    & "        and B.STYMD      <= @P2                       " _
                    & "        and B.ENDYMD     >= @P3                       " _
                    & "        and B.DELFLG     <> @P4                       " _
                    & " INNER JOIN MC007_TODKORG C                           " _
                    & "         ON C.CAMPCODE    = D.CAMPCODE                " _
                    & "        and C.TORICODE    = D.TORICODE                " _
                    & "        and C.TODOKECODE  = D.TODOKECODE              " _
                    & "        and C.DELFLG     <> @P4                       " _
                    & "        and C.CAMPCODE    = B.CAMPCODE                " _
                    & "        and C.UORG        = B.CODE                    " _
                    & " WHERE D.CAMPCODE   = @P1                             " _
                    & "   and D.STYMD     <= @P2                             " _
                    & "   and D.ENDYMD    >= @P3                             " _
                    & "   and D.DELFLG    <> @P4                             "

                ' 条件指定で指定されたものでＳＱＬで可能なものを追加する
                '取引先コード
                If Not String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) OrElse Not String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                    If Not String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) AndAlso String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                        SQLStr &= String.Format(" and D.TORICODE = '{0}' ", work.WF_SEL_TORICODEF.Text)
                    ElseIf String.IsNullOrEmpty(work.WF_SEL_TORICODEF.Text) AndAlso Not String.IsNullOrEmpty(work.WF_SEL_TORICODET.Text) Then
                        SQLStr &= String.Format(" and D.TORICODE = '{0}' ", work.WF_SEL_TORICODET.Text)
                    Else
                        SQLStr &= String.Format(" and D.TORICODE >= '{0}' ", work.WF_SEL_TORICODEF.Text)
                        SQLStr &= String.Format(" and D.TORICODE <= '{0}' ", work.WF_SEL_TORICODET.Text)
                    End If
                End If
                '届先コード
                If Not String.IsNullOrEmpty(work.WF_SEL_TODOKECODE.Text) Then
                    SQLStr &= String.Format(" and NAMES = '{0}' ", work.WF_SEL_TODOKECODE.Text)
                End If
                '届先名称（部分一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_TODOKENAME.Text) Then
                    SQLStr &= String.Format(" and NAMES LIKE '%{0}%' ", work.WF_SEL_TODOKENAME.Text)
                End If
                '郵便番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_POSTNUM.Text) Then
                    SQLStr &= String.Format(" and (POSTNUM1 + POSTNUM2) LIKE '{0}%' ", work.WF_SEL_POSTNUM.Text)
                End If
                '住所（部分一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_ADDR.Text) Then
                    SQLStr &= String.Format(" and (ADDR1 + ADDR2 + ADDR3 + ADDR4) LIKE '%{0}%' ", work.WF_SEL_ADDR.Text)
                End If
                '電話番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_TEL.Text) Then
                    SQLStr &= String.Format(" and TEL LIKE '{0}%' ", work.WF_SEL_TEL.Text)
                End If
                'FAX番号（前方一致）
                If Not String.IsNullOrEmpty(work.WF_SEL_FAX.Text) Then
                    SQLStr &= String.Format(" and FAX LIKE '{0}%' ", work.WF_SEL_FAX.Text)
                End If
                '市町村コード
                If Not String.IsNullOrEmpty(work.WF_SEL_CITIES.Text) Then
                    SQLStr &= String.Format(" and CITIES = '{0}' ", work.WF_SEL_CITIES.Text)
                End If
                '分類
                If Not String.IsNullOrEmpty(work.WF_SEL_CLASS.Text) Then
                    SQLStr &= String.Format(" and CLASS = '{0}' ", work.WF_SEL_CLASS.Text)
                End If

                SQLStr &= " ORDER BY D.TORICODE, D.TODOKECODE, D.STYMD "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar)
                    Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar)

                    PARA1.Value = work.WF_SEL_CAMPCODE.Text
                    PARA2.Value = work.WF_SEL_ENDYMD.Text
                    PARA3.Value = work.WF_SEL_STYMD.Text
                    PARA4.Value = C_DELETE_FLG.DELETE
                    PARA5.Value = "ORG"
                    PARA6.Value = Master.ROLE_ORG

                    SQLcmd.CommandTimeout = 300

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        'フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            BASEtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                        '○テーブル検索結果をテーブル格納
                        BASEtbl.Load(SQLdr)
                    End Using

                    For Each BASErow As DataRow In BASEtbl.Rows
                        '○項目名称セット
                        CODENAME_get("CAMPCODE", BASErow("CAMPCODE"), BASErow("CAMPNAME"), WW_DUMMY)        '会社名称
                        CODENAME_get("TORICODE", BASErow("TORICODE"), BASErow("TORINAME"), WW_DUMMY)        '取引先名称
                        CODENAME_get("CITIES", BASErow("CITIES"), BASErow("CITIESNAME"), WW_DUMMY)          '市町村名称
                        CODENAME_get("MORG", BASErow("MORG"), BASErow("MORGNAME"), WW_DUMMY)                '管理部署名称
                        CODENAME_get("CLASS", BASErow("CLASS"), BASErow("CLASSNAME"), WW_DUMMY)             '分類名称
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MC006_TODOKESAKI SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC006_TODOKESAKI Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データソート
        CS0026TBLSORT.COMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0026TBLSORT.PROFID = Master.PROF_VIEW
        CS0026TBLSORT.MAPID = Master.MAPID
        CS0026TBLSORT.VARI = Master.VIEWID
        CS0026TBLSORT.TABLE = BASEtbl
        CS0026TBLSORT.TAB = ""
        CS0026TBLSORT.FILTER = ""
        CS0026TBLSORT.SortandNumbring()
        If isNormal(CS0026TBLSORT.ERR) Then
            BASEtbl = CS0026TBLSORT.TABLE
        End If

    End Sub

    ''' <summary>
    ''' PDFtblカラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub PDFtbl_ColumnsAdd()
        PDFtbl = New DataTable()
        If PDFtbl.Columns.Count <> 0 Then
            PDFtbl.Columns.Clear()
        End If

        'PDFtblテンポラリDB項目作成
        PDFtbl.Clear()

        PDFtbl.Columns.Add("FILENAME", GetType(String))
        PDFtbl.Columns.Add("DELFLG", GetType(String))
        PDFtbl.Columns.Add("FILEPATH", GetType(String))

    End Sub


    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************
    ''' <summary>
    ''' 書式変更処理
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function REP_ITEM_FORMAT(ByVal I_FIELD As String, ByRef I_VALUE As String) As String
        REP_ITEM_FORMAT = I_VALUE
        Select Case I_FIELD
            Case "SEQ"
                Try
                    REP_ITEM_FORMAT = Format(CInt(I_VALUE), "0")
                Catch ex As Exception
                End Try
            Case Else
        End Select
    End Function

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_CLAS"></param>
    ''' <param name="O_RTN"></param>
    ''' <param name="IO_LISTBOX"></param>
    ''' <param name="IO_LISTBOX2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_FIXVALUE(ByVal I_CLAS As String, ByRef O_RTN As String, ByRef IO_LISTBOX As ListBox, Optional ByRef IO_LISTBOX2 As ListBox = Nothing)

        GS0007FIXVALUElst.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        GS0007FIXVALUElst.CLAS = I_CLAS
        GS0007FIXVALUElst.LISTBOX1 = IO_LISTBOX
        If Not IsNothing(IO_LISTBOX2) Then
            GS0007FIXVALUElst.LISTBOX2 = IO_LISTBOX2
        End If

        GS0007FIXVALUElst.GS0007FIXVALUElst()

        If isNormal(GS0007FIXVALUElst.ERR) Then
            IO_LISTBOX = GS0007FIXVALUElst.LISTBOX1
            If Not IsNothing(IO_LISTBOX2) Then
                IO_LISTBOX2 = GS0007FIXVALUElst.LISTBOX2
            End If
            O_RTN = ""
        Else
            Master.Output(GS0007FIXVALUElst.ERR, C_MESSAGE_TYPE.ABORT)
            O_RTN = "ERR"
        End If

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal args As Hashtable = Nothing)

        '○名称取得
        O_TEXT = ""
        O_RTN = ""

        If I_VALUE <> "" Then
            With leftview
                Select Case I_FIELD
                    Case "CAMPCODE"     '会社名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                    Case "DELFLG"       '削除フラグ名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
                    Case "TORICODE"     '取引先名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTORIParam(work.WF_SEL_CAMPCODE.Text))
                    Case "TODOKECODE"   '届先名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, I_VALUE, O_TEXT, O_RTN, work.CreateTODOKEParam(work.WF_SEL_CAMPCODE.Text, WF_TORICODE.Text))
                    Case "MORG"         '管理部署名
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateMORGParam(work.WF_SEL_CAMPCODE.Text))
                    Case "UORG"         '運用部署名
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text))
                    Case "CITIES"       '市町村名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CITIES"))
                    Case "CLASS"        '分類名称
                        .CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "CLASS"))
                End Select
            End With
        End If

    End Sub

End Class

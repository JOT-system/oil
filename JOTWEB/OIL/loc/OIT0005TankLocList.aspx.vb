Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0005TankLocList
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIT0005tbl As DataTable                                 '一覧格納用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力

    Private WW_UPBUTTONFLG As String = "0"                          '更新用ボタンフラグ(1:明細更新)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                'Dim dispDataObj As DemoDispDataClass
                'dispDataObj = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
                '○ 画面表示データ復元
                Master.RecoverTable(OIT0005tbl)

                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '◯ フラグ初期化
                    Me.WW_UPBUTTONFLG = "0"
                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECTWHOLESALE",
                             "WF_CheckBoxSELECTINSPECTION",
                             "WF_CheckBoxSELECTDETENTION"    'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click(WF_ButtonClick.Value)
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "chklGroupFilter"
                            chklGroupFilter_Change()
                        Case "WF_ButtonEND"                 '戻るボタン押下
                            WF_ButtonEND_Click()
                    End Select
                End If
                DisplayGrid()
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

            WF_BOXChange.Value = "detailbox"

        Finally
            If OIT0005tbl IsNot Nothing Then
                OIT0005tbl.Clear()
                OIT0005tbl.Dispose()
                OIT0005tbl = Nothing
            End If
        End Try

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0005WRKINC.MAPIDL
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
        Dim rtn As String = ""
        rightview.Initialize(rtn)

        '○ 画面の値設定
        WW_MAPValueSet()

        '★その他状況(受注(未卸中・交検中・留置中))の場合
        If work.WF_COND_DETAILTYPE.Text = "9" Then
            '○(一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()
        End If

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        '**********************************************
        '状況名称をヘッダー左下に設定
        '**********************************************
        Master.SetTitleLeftBottomText(work.WF_COND_DETAILTYPENAME.Text)

        '****************************************
        '生成したデータを画面に貼り付け
        '*************************
        GridViewInitialize()
        '**********************************************
        '絞り込みタブを設定
        '**********************************************
        Me.chklGroupFilter.DataSource = OIT0005WRKINC.DispDataClass.GetDetailInsideNames(work.WF_COND_DETAILTYPE.Text)
        Me.chklGroupFilter.DataValueField = "Key"
        Me.chklGroupFilter.DataTextField = "Value"
        Me.chklGroupFilter.DataBind()
        Dim rowCnt As Integer = 0
        Dim fieldName As String = ""
        For Each chkGrp In Me.chklGroupFilter.Items.Cast(Of ListItem)
            chkGrp.Selected = True
            fieldName = String.Format("ISCOUNT{0}GROUP", chkGrp.Value)
            rowCnt = (From dr As DataRow In Me.OIT0005tbl Where dr(fieldName).Equals("1")).Count
            chkGrp.Text = chkGrp.Text & "(" & rowCnt.ToString("#,##0両") & ")"
        Next chkGrp

    End Sub
    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        'リアルタイム性が重要な為、マスタからの一括更新はしない。単票で完結保存させる想定
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

        '〇 一覧の件数を取得
        'Me.WF_ListCNT.Text = "件数：" + OIT0005tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0005tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        '★その他状況(受注(未卸中・交検中・留置中))の場合は表示内容を変更
        If work.WF_COND_DETAILTYPE.Text = "9" Then
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text + "9"
        Else
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
        End If
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.None).ToString
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.OPERATIONCOLUMNWIDTHOPT = -1
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
    Protected Function MAPDataGet(ByVal SQLcon As SqlConnection) As Boolean
        If OIT0005tbl Is Nothing Then
            OIT0005tbl = New DataTable
        End If

        If OIT0005tbl.Columns.Count <> 0 Then
            OIT0005tbl.Columns.Clear()
        End If

        OIT0005tbl.Clear()
        Dim viewName As String = work.GetTankViewName(work.WF_COND_DETAILTYPE.Text)
        Dim salesOfficeInstat As String = GRC0001TILESELECTORWRKINC.GetSelectedSqlInStatement(work.WF_SEL_SALESOFFICE_TILES.Text)
        Dim sotrOrderValue As String = work.GetTankViewOrderByString(work.WF_COND_DETAILTYPE.Text)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0})  AS LINECNT", sotrOrderValue).AppendLine()
        sqlStat.AppendLine("      ,'' AS OPERATION")
        'sqlStat.AppendLine("     ,0  AS TIMSTP)
        sqlStat.AppendLine("      ,1  AS 'SELECT'")
        sqlStat.AppendLine("      ,0  AS HIDDEN")
        sqlStat.AppendLine("      ,VTS.* ") 'ビューのフィールド追加しても動作可能なようにしている(削った場合は要稼働確認)
        sqlStat.AppendFormat("  FROM {0} VTS", viewName).AppendLine()
        sqlStat.AppendFormat(" WHERE VTS.OFFICECODE in ({0})", salesOfficeInstat).AppendLine()
        If salesOfficeInstat.Contains("'110001'") Then
            sqlStat.AppendFormat("    OR VTS.BRANCHCODE = '110001'", salesOfficeInstat).AppendLine()
            'sqlStat.AppendFormat("    OR VTS.OFFICECODE = ''", salesOfficeInstat).AppendLine()
        End If
        sqlStat.AppendFormat(" ORDER BY {0}", sotrOrderValue).AppendLine()

        Try
            Using sqlCmd As New SqlCommand(sqlStat.ToString, SQLcon)

                Using SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0005tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next index

                    '○ テーブル検索結果をテーブル格納
                    OIT0005tbl.Load(SQLdr)
                End Using
            End Using
            Return True
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L SELECT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:VIW0008 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return False
        End Try

    End Function
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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
        CS0030REPORT.TBLDATA = OIT0005tbl                       'データ参照  Table
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' フィルタタイル変更時イベント
    ''' </summary>
    Protected Sub chklGroupFilter_Change()
        WF_GridPosition.Text = "1"
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
        Dim TBLview As New DataView(OIT0005tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = (TBLview.Count - (TBLview.Count Mod 10)).ToString
        Else
            WF_GridPosition.Text = (TBLview.Count - (TBLview.Count Mod 10) + 1).ToString
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click(ByVal chkFieldName As String)

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0005tbl)

        Select Case chkFieldName
            Case "WF_CheckBoxSELECTWHOLESALE"
                'チェックボックス判定
                For i As Integer = 0 To OIT0005tbl.Rows.Count - 1
                    If Convert.ToString(OIT0005tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                        If Convert.ToString(OIT0005tbl.Rows(i)("WHOLESALEFLG")) = "on" Then
                            OIT0005tbl.Rows(i)("WHOLESALEFLG") = ""
                        Else
                            OIT0005tbl.Rows(i)("WHOLESALEFLG") = "on"
                        End If
                        OIT0005tbl.Rows(i)("WHOLESALECHGFLG") = "1"
                    End If
                Next

            Case "WF_CheckBoxSELECTINSPECTION"
                'チェックボックス判定
                For i As Integer = 0 To OIT0005tbl.Rows.Count - 1
                    If Convert.ToString(OIT0005tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                        If Convert.ToString(OIT0005tbl.Rows(i)("INSPECTIONFLG")) = "on" Then
                            OIT0005tbl.Rows(i)("INSPECTIONFLG") = ""
                        Else
                            OIT0005tbl.Rows(i)("INSPECTIONFLG") = "on"
                        End If
                        OIT0005tbl.Rows(i)("INSPECTIONCHGFLG") = "1"
                    End If
                Next

            Case "WF_CheckBoxSELECTDETENTION"
                'チェックボックス判定
                For i As Integer = 0 To OIT0005tbl.Rows.Count - 1
                    If Convert.ToString(OIT0005tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                        If Convert.ToString(OIT0005tbl.Rows(i)("DETENTIONFLG")) = "on" Then
                            OIT0005tbl.Rows(i)("DETENTIONFLG") = ""
                        Else
                            OIT0005tbl.Rows(i)("DETENTIONFLG") = "on"
                        End If
                        OIT0005tbl.Rows(i)("DETENTIONCHGFLG") = "1"
                    End If
                Next
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '明細更新ボタン押下時
        Me.WW_UPBUTTONFLG = "1"

    End Sub

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
            WW_LINECNT = Integer.Parse(WF_GridDBclick.Text)
        Catch ex As Exception
            Exit Sub
        End Try
        Dim tankNo As String = (From dr As DataRow In Me.OIT0005tbl Where CInt(dr("LINECNT")) = WW_LINECNT Select Convert.ToString(dr("TANKNUMBER"))).FirstOrDefault
        work.WF_LISTSEL_INPTBL.Text = Master.XMLsaveF
        work.WF_LISTSEL_TANKNUMBER.Text = tankNo
        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        WF_GridDBclick.Text = ""

        '登録画面ページへ遷移
        Master.TransitionPage()

    End Sub
    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数
        Dim qFilterQue = (From chklItm In Me.chklGroupFilter.Items.Cast(Of ListItem) Where chklItm.Selected Select chklItm.Value)
        Dim filterKeyValues As List(Of String)
        If qFilterQue.Any Then
            filterKeyValues = qFilterQue.ToList
        Else
            filterKeyValues = New List(Of String)
        End If
        '○ 表示対象行カウント(絞り込み対象)
        Dim fieldName As String = "ISCOUNT{0}GROUP"
        For Each OIT0005row As DataRow In OIT0005tbl.Rows
            OIT0005row("HIDDEN") = "1"
            For Each filterKeyValue In filterKeyValues
                If Convert.ToString(OIT0005row(String.Format(fieldName, filterKeyValue))).Equals("1") Then
                    OIT0005row("HIDDEN") = "0"
                End If
            Next

            If Convert.ToString(OIT0005row("HIDDEN")) = "0" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0005row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0005tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        '★その他状況(受注(未卸中・交検中・留置中))の場合は表示内容を変更
        If work.WF_COND_DETAILTYPE.Text = "9" Then
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text + "9"
        Else
            CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
            CS0013ProfView.LEVENT = "ondblclick"
            CS0013ProfView.LFUNC = "ListDbClick"
        End If
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.None).ToString
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.OPERATIONCOLUMNWIDTHOPT = -1
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = Convert.ToString(TBLview.Item(0)("SELECT"))
        End If

        TBLview.Dispose()
        TBLview = Nothing

        '★その他状況(受注(未卸中・交検中・留置中))の場合
        If work.WF_COND_DETAILTYPE.Text = "9" Then
            '○(一覧)テキストボックスの制御(読取専用)
            WW_ListTextBoxReadControl()
        End If

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea.FindControl(pnlListArea.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim chkObjWH As CheckBox = Nothing
        Dim chkObjIN As CheckBox = Nothing
        Dim chkObjDE As CheckBox = Nothing
        'LINECNTを除いたチェックボックスID
        Dim chkObjIdWOWHcnt As String = "chk" & pnlListArea.ID & "WHOLESALEFLG"
        Dim chkObjIdWOINcnt As String = "chk" & pnlListArea.ID & "INSPECTIONFLG"
        Dim chkObjIdWODEcnt As String = "chk" & pnlListArea.ID & "DETENTIONFLG"
        'LINECNTを含むチェックボックスID
        Dim chkObjWHId As String = ""
        Dim chkObjINId As String = ""
        Dim chkObjDEId As String = ""
        Dim chkObjType As String = ""
        '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        '　データテーブルの行Index
        Dim rowIdx As Integer = 0


        For Each rowitem As TableRow In tblObj.Rows
            '★未卸・交検・留置(チェックボックス)の制御
            If OIT0005tbl.Rows.Count <> 0 Then
                loopdr = OIT0005tbl.Rows(rowIdx)
                chkObjWHId = chkObjIdWOWHcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjWH = Nothing
                chkObjINId = chkObjIdWOINcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjIN = Nothing
                chkObjDEId = chkObjIdWODEcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjDE = Nothing

                For Each cellObj As TableCell In rowitem.Controls
                    chkObjWH = DirectCast(cellObj.FindControl(chkObjWHId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjWH IsNot Nothing AndAlso loopdr("WHOLESALEFLG").ToString() = "" AndAlso loopdr("WHOLESALECHGFLG").ToString() = "0" Then
                        '未卸可否フラグ(チェックボックス)を非活性
                        chkObjWH.Enabled = False
                        Exit For
                    End If
                Next
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjIN = DirectCast(cellObj.FindControl(chkObjINId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjIN IsNot Nothing AndAlso loopdr("INSPECTIONFLG").ToString() = "" AndAlso loopdr("INSPECTIONCHGFLG").ToString() = "0" Then
                        '交検可否フラグ(チェックボックス)を非活性
                        chkObjIN.Enabled = False
                        Exit For
                    End If
                Next
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjDE = DirectCast(cellObj.FindControl(chkObjDEId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjDE IsNot Nothing AndAlso loopdr("DETENTIONFLG").ToString() = "" AndAlso loopdr("DETENTIONCHGFLG").ToString() = "0" Then
                        '留置可否フラグ(チェックボックス)を非活性
                        chkObjDE.Enabled = False
                        Exit For
                    End If
                Next

                For Each cellObj As TableCell In rowitem.Controls
                    '(一覧)受入日
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDER_ACTUALACCDATE") Then
                        If loopdr("WHOLESALEFLG").ToString() = "" AndAlso loopdr("WHOLESALECHGFLG").ToString() = "0" Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    End If

                    '(一覧)空車着日
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDER_ACTUALEMPARRDATE") Then
                        If loopdr("WHOLESALEFLG").ToString() = "" AndAlso loopdr("WHOLESALECHGFLG").ToString() = "0" _
                            AndAlso loopdr("INSPECTIONFLG").ToString() = "" AndAlso loopdr("INSPECTIONCHGFLG").ToString() = "0" _
                            AndAlso loopdr("DETENTIONFLG").ToString() = "" AndAlso loopdr("DETENTIONCHGFLG").ToString() = "0" Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    End If
                Next
            End If
            rowIdx += 1
        Next

    End Sub
End Class
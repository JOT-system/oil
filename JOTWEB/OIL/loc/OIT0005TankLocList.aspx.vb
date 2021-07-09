Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
Imports JOTWEB.GRC0001TILESELECTORWRKINC

Public Class OIT0005TankLocList
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIT0005tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0005Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0005KaisouCnttbl As DataTable                        '作業用テーブル(回送進行ステータスチェック用)

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力

    Private WW_DETAILTYPE09 As String = "9"
    Private WW_DETAILTYPE10 As String = "10"
    Private WW_DETAILTYPE09_FILTER() As String = {"未卸中", "交検中", "留置中"}
    Private WW_DETAILTYPE10_FILTER() As String = {"修理", "ＭＣ", "交検", "全検", "留置"}

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード
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
                        Case "WF_Field_DbClick"              'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"                  '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                  '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"              '左ボックスダブルクリック
                            WF_ButtonSel_Click()
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
                        Case "WF_ButtonUpdateList"      '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ListChange"                  'リスト変更
                            WF_ListChange()
                        Case "WF_ComDeleteIconClick"          'リスト削除
                            WF_ListDelete()
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
        'Master.MAPID = OIT0005WRKINC.MAPIDL
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}
            work.WF_MAIN_OFFICECODE.Text = ""
            If Master.MAPID = OIT0005WRKINC.MAPIDL + "ORDMAIN" Then
                '★受注着駅到着後状況
                work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE09
                work.WF_COND_DETAILTYPENAME.Text = "その他状況"
                WW_FixvalueMasterSearch(Master.USER_ORG, "SALESOFFICE", "", WW_GetValue)
            ElseIf Master.MAPID = OIT0005WRKINC.MAPIDL + "OOSMAIN" Then
                '★回送後状況
                work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE10
                work.WF_COND_DETAILTYPENAME.Text = "その他状況"
                WW_FixvalueMasterSearch(Master.USER_ORG, "SALESOFFICE_KAISOU", "", WW_GetValue)
            Else
                Exit Sub
            End If

            For i = 0 To WW_GetValue.Length - 1
                If WW_GetValue(i) = "" Then Continue For

                If i = 0 Then
                    work.WF_MAIN_OFFICECODE.Text &= "'" + WW_GetValue(i) + "'"
                Else
                    work.WF_MAIN_OFFICECODE.Text &= ",'" + WW_GetValue(i) + "'"
                End If
            Next

            work.WF_MAIN_VIEWTABLE.Text = work.GetTankViewName(work.WF_COND_DETAILTYPE.Text)
            work.WF_MAIN_VIEWSORT.Text = work.GetTankViewOrderByString(work.WF_COND_DETAILTYPE.Text)

            '○ 画面レイアウト設定
            If Master.VIEWID = "" Then
                'Dim rightview As New GRIS0003SRightBox
                'rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
                'Master.VIEWID = rightview.GetViewId(work.WF_SEL_CAMPCODE.Text + "9")
                Master.VIEWID = "jotsys"
            End If

        End If
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

        '○(一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

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
        ''**********************************************
        ''絞り込みタブを設定
        ''**********************************************
        'Me.chklGroupFilter.DataSource = OIT0005WRKINC.DispDataClass.GetDetailInsideNames(work.WF_COND_DETAILTYPE.Text)
        'Me.chklGroupFilter.DataValueField = "Key"
        'Me.chklGroupFilter.DataTextField = "Value"
        'Me.chklGroupFilter.DataBind()
        'Dim rowCnt As Integer = 0
        'Dim fieldName As String = ""
        'For Each chkGrp In Me.chklGroupFilter.Items.Cast(Of ListItem)
        '    chkGrp.Selected = True
        '    fieldName = String.Format("ISCOUNT{0}GROUP", chkGrp.Value)
        '    rowCnt = (From dr As DataRow In Me.OIT0005tbl Where dr(fieldName).Equals("1")).Count
        '    chkGrp.Text = chkGrp.Text & "(" & rowCnt.ToString("#,##0両") & ")"
        'Next chkGrp

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

        '○受注着駅到着後状況の場合
        If work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE09 Then
            '○未卸以外の場合
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION <> '" + BaseDllConst.CONST_TANKSITUATION_20 + "'")
                '★返送日が入力されている場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                '★交検日の入力を促すようにするため空白にする。
                OIT0005row("JRINSPECTIONDATE") = ""
            Next

            '○回送後状況の場合
        ElseIf work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE10 Then
            '○交検の場合
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_13 + "'" + " OR " + "TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_03 + "'")
                '★返送日が入力されている場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                '★交検日の入力を促すようにするため空白にする。
                OIT0005row("JRINSPECTIONDATE") = ""
            Next
            '○全検の場合
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_14 + "'" + " OR " + "TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_04 + "'")
                '★返送日が入力されている場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                '★全検日の入力を促すようにするため空白にする。
                OIT0005row("JRALLINSPECTIONDATE") = ""
            Next

            '### 20210513 START 指摘票対応(No479)全体 #################################################
            '○名称取得(中間点検場所、自主点検場所)
            Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            For Each OIT0005row As DataRow In OIT0005tbl.Select("INTERINSPECTSTATION <> '' OR SELFINSPECTSTATION <> ''")
                '中間点検場所
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "STATIONPATTERN", Convert.ToString(OIT0005row("INTERINSPECTSTATION")), WW_GetValue)
                OIT0005row("INTERINSPECTSTATION") = WW_GetValue(0)
                '自主点検場所
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "STATIONPATTERN", Convert.ToString(OIT0005row("SELFINSPECTSTATION")), WW_GetValue)
                OIT0005row("SELFINSPECTSTATION") = WW_GetValue(0)

            Next

            '○名称取得(中間点検実施者、自主点検実施者)
            WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            For Each OIT0005row As DataRow In OIT0005tbl.Select("INTERINSPECTORGCODE <> '' OR SELFINSPECTORGCODE <> ''")
                '中間点検場所
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE", Convert.ToString(OIT0005row("INTERINSPECTORGCODE")), WW_GetValue)
                OIT0005row("INTERINSPECTORGCODE") = WW_GetValue(0)
                '自主点検場所
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                WW_FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE", Convert.ToString(OIT0005row("SELFINSPECTORGCODE")), WW_GetValue)
                OIT0005row("SELFINSPECTORGCODE") = WW_GetValue(0)

            Next
            '### 20210513 END   指摘票対応(No479)全体 #################################################

        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

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
            Select Case work.WF_COND_DETAILTYPE.Text
                '○受注着駅到着後状況の場合
                Case "9"
                    '○タンク車状況
                    Select Case chkGrp.Text
                        '★留置中は未チェック
                        Case WW_DETAILTYPE09_FILTER(2)
                            chkGrp.Selected = False
                        Case Else
                            chkGrp.Selected = True
                    End Select
                Case Else
                    chkGrp.Selected = True
            End Select

            fieldName = String.Format("ISCOUNT{0}GROUP", chkGrp.Value)
            rowCnt = (From dr As DataRow In Me.OIT0005tbl Where dr(fieldName).Equals("1")).Count
            chkGrp.Text = chkGrp.Text & "(" & rowCnt.ToString("#,##0両") & ")"
        Next chkGrp

        '○ 表示対象行カウント(絞り込み対象)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数
        Dim qFilterQue = (From chklItm In Me.chklGroupFilter.Items.Cast(Of ListItem) Where chklItm.Selected Select chklItm.Value)
        Dim filterKeyValues As List(Of String)
        If qFilterQue.Any Then
            filterKeyValues = qFilterQue.ToList
        Else
            filterKeyValues = New List(Of String)
        End If
        Dim fieldName1 As String = "ISCOUNT{0}GROUP"
        For Each OIT0005row As DataRow In OIT0005tbl.Rows
            OIT0005row("HIDDEN") = "1"
            For Each filterKeyValue In filterKeyValues
                If Convert.ToString(OIT0005row(String.Format(fieldName1, filterKeyValue))).Equals("1") Then
                    OIT0005row("HIDDEN") = "0"
                End If
            Next

            If Convert.ToString(OIT0005row("HIDDEN")) = "0" Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0005row("SELECT") = WW_DataCNT
            End If
        Next

        '〇 一覧の件数を取得
        'Me.WF_ListCNT.Text = "件数：" + OIT0005tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0005tbl)

        TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT
        'TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        '★その他状況(受注(未卸中・交検中・留置中))の場合は表示内容を変更
        '★　　　　　 回送(修理・ＭＣ・交検・全検・留置・移動)
        If work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE09 _
           OrElse work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE10 Then
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
        Dim sqlStat As New StringBuilder
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005C OrElse
           Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005D Then
            Dim viewName As String = work.GetTankViewName(work.WF_COND_DETAILTYPE.Text)
            Dim salesOfficeInstat As String = GRC0001TILESELECTORWRKINC.GetSelectedSqlInStatement(work.WF_SEL_SALESOFFICE_TILES.Text)
            Dim sotrOrderValue As String = work.GetTankViewOrderByString(work.WF_COND_DETAILTYPE.Text)
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
        Else
            sqlStat.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0})  AS LINECNT", work.WF_MAIN_VIEWSORT.Text).AppendLine()
            'sqlStat.AppendFormat("SELECT ROW_NUMBER() OVER(ORDER BY {0})  AS LINECNT", "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)").AppendLine()
            sqlStat.AppendLine("      ,'' AS OPERATION")
            'sqlStat.AppendLine("     ,0  AS TIMSTP)
            sqlStat.AppendLine("      ,1  AS 'SELECT'")
            sqlStat.AppendLine("      ,0  AS HIDDEN")
            sqlStat.AppendLine("      ,VTS.* ") 'ビューのフィールド追加しても動作可能なようにしている(削った場合は要稼働確認)
            sqlStat.AppendFormat("  FROM {0} VTS", work.WF_MAIN_VIEWTABLE.Text).AppendLine()
            sqlStat.AppendFormat(" WHERE VTS.OFFICECODE in ({0})", work.WF_MAIN_OFFICECODE.Text).AppendLine()
            sqlStat.AppendFormat(" ORDER BY {0}", work.WF_MAIN_VIEWSORT.Text).AppendLine()
            'sqlStat.AppendFormat(" ORDER BY {0}", "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)").AppendLine()
        End If

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
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '明細更新ボタン押下時
        Me.WW_UPBUTTONFLG = "1"

        Select Case work.WF_COND_DETAILTYPE.Text
            '○受注着駅到着後状況
            Case "9"
                WW_UpdateOrderAfterSituation(WW_ERRCODE)
                If WW_ERRCODE = "ERR" Then
                    Exit Sub
                End If

            '○回送後状況
            Case "10"
                WW_UpdateKaisouAfterSituation(WW_ERRCODE)
                If WW_ERRCODE = "ERR" Then
                    Exit Sub
                End If
        End Select

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub

#Region "リスト変更時処理"
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()
        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0005tbl.AsEnumerable.
                    FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '〇 一覧の件数取得
        Dim intListCnt As Integer = OIT0005tbl.Rows.Count

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "JRINSPECTIONDATE"         '(一覧)交検日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_ListValue, WW_DATE)
                    If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                        updHeader.Item(WF_FIELD.Value) = ""
                    Else
                        updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                    'タンク車マスタの交検日を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                        I_ITEM:="JRINSPECTIONDATE",
                                        I_VALUE:=Convert.ToString(updHeader.Item(WF_FIELD.Value)))
                Catch ex As Exception
                End Try
            Case "JRALLINSPECTIONDATE"      '(一覧)全検日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_ListValue, WW_DATE)
                    If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                        updHeader.Item(WF_FIELD.Value) = ""
                    Else
                        updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                    'タンク車マスタの全検日を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                        I_ITEM:="JRALLINSPECTIONDATE",
                                        I_VALUE:=Convert.ToString(updHeader.Item(WF_FIELD.Value)))
                Catch ex As Exception
                End Try
            Case "ORDER_ACTUALEMPARRDATE"   '(一覧)返送日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_ListValue, WW_DATE)
                    If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                        updHeader.Item(WF_FIELD.Value) = ""
                    Else
                        updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try

            '### 20210513 START 指摘票対応(No479)全体 #################################################
            Case "MYWEIGHT"                 '(一覧)自重
                '★全角⇒半角変換
                WW_ListValue = StrConv(WW_ListValue, VbStrConv.Narrow)
                Try
                    '○数値チェック(文字の場合はDecimal変換できないためExceptionにてSKIP)
                    Dim dcWeight As Decimal = 0
                    dcWeight = Decimal.Parse(WW_ListValue)

                    '○数値チェック(整数のみ、整数と少数などの組み合わせ)
                    Dim iDecLen As Integer = WW_ListValue.IndexOf(".")
                    Dim sInt As String = ""
                    Dim sDec As String = ""
                    If iDecLen < 0 Then
                        sInt = WW_ListValue
                        sDec = "0"
                        WW_ListValue = WW_ListValue + "." + sDec
                    Else
                        sInt = WW_ListValue.Substring(0, iDecLen)
                        sDec = WW_ListValue.Substring(iDecLen + 1)
                    End If

                    '○数値チェック(整数の桁数(整数2桁, 小数第1位))対象ではない場合はSKIP
                    If sInt.Length > 2 Then
                        Dim Msg As String = "自重の整数は2桁までとしてください。"
                        Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
                        Exit Select
                    ElseIf sDec.Length > 1 Then
                        Dim Msg As String = "自重の少数は少数第1までとしてください。"
                        Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, I_PARA01:=Msg, needsPopUp:=True)
                        Exit Select
                    End If

                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの自重を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="MYWEIGHT",
                                                I_VALUE:=WW_ListValue)
                Catch ex As Exception
                End Try
                '### 20210513 END   指摘票対応(No479)全体 #################################################
            Case "INSPECTMEMBERNAME"        '(一覧)点検実施者(社員名)
                '文字の長さを設定
                Dim iNameLen As Integer = WW_ListValue.Length
                Try
                    If iNameLen > 20 Then
                        WW_ListValue = WW_ListValue.Substring(0, 20)
                    End If
                Catch ex As Exception
                    WW_ListValue = ""
                End Try

                updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Master.SaveTable(OIT0005tbl)
                'タンク車マスタの点検実施者(社員名)を更新
                WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="INSPECTMEMBERNAME",
                                                I_VALUE:=WW_ListValue)
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

    End Sub
#End Region

#Region "リスト削除時処理"
    ''' <summary>
    ''' リスト削除時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListDelete()
        '紐付けているリストのID
        Dim ListId As String = Master.DELETE_FIELDINFO.ListId
        'フィールド名
        Dim FieldName As String = Master.DELETE_FIELDINFO.FieldName
        '行番号
        Dim LineCnt As String = Master.DELETE_FIELDINFO.LineCnt

        Select Case work.WF_COND_DETAILTYPE.Text
                '受注着駅到着後状況
            Case "9"
                WW_ListDelete_TYPE09(FieldName, LineCnt)

                '回送後状況
            Case "10"
                WW_ListDelete_TYPE10(FieldName, LineCnt)

        End Select

    End Sub
    ''' <summary>
    ''' リスト削除時処理(受注着駅到着後状況)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TYPE09(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)

    End Sub
    ''' <summary>
    ''' リスト削除時処理(回送後状況)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_ListDelete_TYPE10(ByVal I_FIELDNAME As String, ByVal I_LINECNT As String)
        '○ 対象ヘッダー取得
        Dim updHeader = OIT0005tbl.AsEnumerable.
                    FirstOrDefault(Function(x) Convert.ToString(x.Item("LINECNT")) = I_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        Select Case I_FIELDNAME
            Case "ORDER_ACTUALEMPARRDATE"
                updHeader.Item("ORDER_ACTUALEMPARRDATE") = ""
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)
    End Sub
#End Region

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

        Select Case work.WF_COND_DETAILTYPE.Text
            Case "9"
                WW_CheckBoxSELECT09_Click(chkFieldName)
            Case "10"
            Case Else
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0005tbl)

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理(受注着駅到着後状況)
    ''' </summary>
    Protected Sub WW_CheckBoxSELECT09_Click(ByVal chkFieldName As String)
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
    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                WF_LeftMViewChange.Value = Integer.Parse(WF_LeftMViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If CInt(WF_LeftMViewChange.Value) = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(一覧)受入日, (一覧)空車着日, (一覧)次回交検日
                        '(一覧)中間点検年月, (一覧)自主点検年月
                        Case "ORDER_ACTUALACCDATE", "ORDER_ACTUALEMPARRDATE", "JRINSPECTIONDATE",
                             "INTERINSPECTDATE", "SELFINSPECTDATE"

                            '○ LINECNT取得
                            Dim WW_LINECNT As Integer = 0
                            If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                            '○ 対象ヘッダー取得
                            Dim updHeader = OIT0005tbl.AsEnumerable.
                                FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
                            If IsNothing(updHeader) Then Exit Sub

                            .WF_Calendar.Text = Convert.ToString(updHeader.Item(WF_FIELD.Value))
                    End Select
                    .ActiveCalendar()

                Else
                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                    '### 20210513 START 指摘票対応(No479)全体 #################################################
                    '(一覧)中間点検場所, (一覧)中間点検者, (一覧)自主点検場所, (一覧)自主点検者, (一覧)点検実施者(社員名)
                    If WF_FIELD.Value = "INTERINSPECTSTATION" _
                        OrElse WF_FIELD.Value = "INTERINSPECTORGCODE" _
                        OrElse WF_FIELD.Value = "SELFINSPECTSTATION" _
                        OrElse WF_FIELD.Value = "SELFINSPECTORGCODE" _
                        OrElse WF_FIELD.Value = "INSPECTMEMBERNAME" Then

                        '○ LINECNT取得
                        Dim WW_LINECNT As Integer = 0
                        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                        '○ 対象ヘッダー取得
                        Dim updHeader = OIT0005tbl.AsEnumerable.
                        FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
                        If IsNothing(updHeader) Then Exit Sub

                        '★(一覧)中間点検場所, (一覧)自主点検場所の場合
                        If WF_FIELD.Value = "INTERINSPECTSTATION" _
                            OrElse WF_FIELD.Value = "SELFINSPECTSTATION" Then

                        End If
                        '★(一覧)中間点検者, (一覧)自主点検者
                        If WF_FIELD.Value = "INTERINSPECTORGCODE" _
                            OrElse WF_FIELD.Value = "SELFINSPECTORGCODE" Then
                            '★抽出条件追加
                            Dim condition As String = ""
                            condition &= " AND KEYCODE IN ("
                            condition &= "   '" + BaseDllConst.CONST_OFFICECODE_010401 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_010402 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_011201 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_011202 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_011203 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_011401 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_011402 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_012301 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_012401 + "'"
                            condition &= " , '" + BaseDllConst.CONST_OFFICECODE_012402 + "'"
                            condition &= " , '" + BaseDllConst.CONST_JOT_011403 + "'"
                            condition &= " )"
                            prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = condition
                        End If

                        '★(一覧)点検実施者(社員名)
                        If WF_FIELD.Value = "INSPECTMEMBERNAME" Then
                            '★抽出条件追加
                            Dim condition As String = ""
                            condition &= " AND KEYCODE IN ("
                            condition &= " '" + Master.USER_ORG + "'"
                            condition &= " )"
                            prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = condition
                        End If
                    End If
                        '### 20210513 END   指摘票対応(No479)全体 #################################################
                        .SetListBox(CType(WF_LeftMViewChange.Value, LIST_BOX_CLASSIFICATION), WW_DUMMY, prmData)
                    .ActiveListBox()
                End If
            End With

        End If
    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ 選択内容を取得
        If leftview.ActiveViewIdx = 2 Then
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            Dim selectedTblKey As String = "VALUE1"
            If selectedLeftTableVal.ContainsKey(selectedTblKey) = False Then
                selectedTblKey = "VALUE8"
            End If
            WW_SelectText = selectedLeftTableVal(selectedTblKey) '他のフィールド名でも取ること可能一旦VALUE1で
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex.ToString
            WW_SelectValue = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Text

        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            '(一覧)受入日, (一覧)空車着日
            '(一覧)次回交検日, (一覧)次回全検日
            '(一覧)中間点検年月, (一覧)中間点検場所, (一覧)中間点検者
            '(一覧)自主点検年月, (一覧)自主点検場所, (一覧)自主点検者
            '(一覧)点検実施者(社員名)
            Case "ORDER_ACTUALACCDATE", "ORDER_ACTUALEMPARRDATE",
                 "JRINSPECTIONDATE", "JRALLINSPECTIONDATE",
                 "INTERINSPECTDATE", "INTERINSPECTSTATION", "INTERINSPECTORGCODE",
                 "SELFINSPECTDATE", "SELFINSPECTSTATION", "SELFINSPECTORGCODE",
                 "INSPECTMEMBERNAME"

                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0005tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0005tbl.AsEnumerable.
                            FirstOrDefault(Function(x) CInt(x.Item("LINECNT")) = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧項目へ設定
                '(一覧)受入日, (一覧)空車着日
                If WF_FIELD.Value = "ORDER_ACTUALACCDATE" _
                    OrElse WF_FIELD.Value = "ORDER_ACTUALEMPARRDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < Date.Parse(BaseDllConst.C_DEFAULT_YMD) Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try

                    '(一覧)次回交検日
                ElseIf WF_FIELD.Value = "JRINSPECTIONDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            ''■ 選択した日付が未設定,
                            ''   選択した日付が現状の交検日より過去の場合
                            'If leftview.WF_Calendar.Text = "" _
                            '    OrElse Convert.ToString(updHeader.Item(WF_FIELD.Value)) = "" Then
                            '    '### 20201001 START 交検日が過去でも設定できるようにするため廃止 ################################################
                            '    'OrElse Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(updHeader.Item(WF_FIELD.Value))) = -1 Then
                            '    '### 20201001 END   交検日が過去でも設定できるようにするため廃止 ################################################
                            '    Master.Output(C_MESSAGE_NO.OIL_TANKNO_KOUKENBI_PAST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                            '    '■ 選択した日付が現状の交検日と同日の場合
                            'ElseIf Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(Convert.ToString(updHeader.Item(WF_FIELD.Value)))) = 0 Then
                            '    updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text

                            'Else
                            '(一覧)交検日に指定した日付を設定
                            updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                            Master.SaveTable(OIT0005tbl)
                            'タンク車マスタの交検日を更新
                            WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="JRINSPECTIONDATE",
                                                I_VALUE:=Convert.ToString(updHeader.Item(WF_FIELD.Value)))
                            'End If
                        End If
                    Catch ex As Exception

                    End Try

                    '(一覧)次回全検日
                ElseIf WF_FIELD.Value = "JRALLINSPECTIONDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            ''■ 選択した日付が未設定,
                            ''   選択した日付が現状の全検日より過去の場合
                            'If leftview.WF_Calendar.Text = "" _
                            '    OrElse Convert.ToString(updHeader.Item(WF_FIELD.Value)) = "" Then
                            '    '### 20201001 START 全検日が過去でも設定できるようにするため廃止 ################################################
                            '    'OrElse Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(updHeader.Item(WF_FIELD.Value))) = -1 Then
                            '    '### 20201001 END   全検日が過去でも設定できるようにするため廃止 ################################################
                            '    Master.Output(C_MESSAGE_NO.OIL_TANKNO_KOUKENBI_PAST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

                            '    '■ 選択した日付が現状の全検日と同日の場合
                            'ElseIf Date.Compare(Date.Parse(leftview.WF_Calendar.Text), Date.Parse(Convert.ToString(updHeader.Item(WF_FIELD.Value)))) = 0 Then
                            '    updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text

                            'Else
                            '(一覧)全検日に指定した日付を設定
                            updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                            Master.SaveTable(OIT0005tbl)
                            'タンク車マスタの全検日を更新
                            WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="JRALLINSPECTIONDATE",
                                                I_VALUE:=Convert.ToString(updHeader.Item(WF_FIELD.Value)))
                            'End If
                        End If
                    Catch ex As Exception

                    End Try

                    '### 20210513 START 指摘票対応(No479)全体 #################################################
                    '(一覧)中間点検年月
                ElseIf WF_FIELD.Value = "INTERINSPECTDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            '(一覧)中間点検年月に指定した日付を設定
                            updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM")
                            Master.SaveTable(OIT0005tbl)
                            'タンク車マスタの中間点検年月を更新
                            WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="INTERINSPECTYM",
                                                I_VALUE:=leftview.WF_Calendar.Text)
                        End If
                    Catch ex As Exception
                    End Try

                    '(一覧)中間点検場所
                ElseIf WF_FIELD.Value = "INTERINSPECTSTATION" Then
                    '(一覧)中間点検場所に指定した値を設定
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの中間点検場所を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="INTERINSPECTSTATION",
                                                I_VALUE:=WW_SETVALUE)

                    '(一覧)中間点検実施者
                ElseIf WF_FIELD.Value = "INTERINSPECTORGCODE" Then
                    '(一覧)中間点検実施者に指定した値を設定
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの中間点検実施者を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="INTERINSPECTORGCODE",
                                                I_VALUE:=WW_SETVALUE)

                    '(一覧)自主点検年月
                ElseIf WF_FIELD.Value = "SELFINSPECTDATE" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < Date.Parse(C_DEFAULT_YMD) Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            '(一覧)自主点検年月に指定した日付を設定
                            updHeader.Item(WF_FIELD.Value) = WW_DATE.ToString("yyyy/MM")
                            Master.SaveTable(OIT0005tbl)
                            'タンク車マスタの自主点検年月を更新
                            WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="SELFINSPECTYM",
                                                I_VALUE:=leftview.WF_Calendar.Text)
                        End If
                    Catch ex As Exception
                    End Try

                    '(一覧)自主点検場所
                ElseIf WF_FIELD.Value = "SELFINSPECTSTATION" Then
                    '(一覧)自主点検場所に指定した値を設定
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの自主点検場所を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="SELFINSPECTSTATION",
                                                I_VALUE:=WW_SETVALUE)

                    '(一覧)自主点検実施者
                ElseIf WF_FIELD.Value = "SELFINSPECTORGCODE" Then
                    '(一覧)自主点検実施者に指定した値を設定
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの自主点検実施者を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="SELFINSPECTORGCODE",
                                                I_VALUE:=WW_SETVALUE)
                    '### 20210513 END   指摘票対応(No479)全体 #################################################

                    '(一覧)点検実施者(社員名)
                ElseIf WF_FIELD.Value = "INSPECTMEMBERNAME" Then
                    '(一覧)点検実施者(社員名)に指定した値を設定
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT
                    Master.SaveTable(OIT0005tbl)
                    'タンク車マスタの点検実施者(社員名)を更新
                    WW_UpdateTankMaster(Convert.ToString(updHeader.Item("TANKNUMBER")),
                                                I_ITEM:="INSPECTMEMBERNAME",
                                                I_VALUE:=WW_SETTEXT)
                End If

                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0005tbl) Then Exit Sub

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        ''○ フォーカスセット
        'Select Case WF_FIELD.Value
        '    Case "WF_CAMPCODE"          '会社コード
        '        WF_CAMPCODE.Focus()
        '    Case "WF_UORG"              '運用部署
        '        WF_UORG.Focus()
        'End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
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

        '明細更新ボタン押下時
        If Me.WW_UPBUTTONFLG = "1" Then
            '○受注着駅到着後状況の場合
            If work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE09 Then
                '○未卸以外の場合
                For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION <> '" + BaseDllConst.CONST_TANKSITUATION_20 + "'")
                    '★返送日が入力されている場合はSKIP
                    If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                    '★交検日の入力を促すようにするため空白にする。
                    OIT0005row("JRINSPECTIONDATE") = ""
                Next

                '○回送後状況の場合
            ElseIf work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE10 Then
                '○交検の場合
                For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_13 + "'")
                    '★返送日が入力されている場合はSKIP
                    If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                    '★交検日の入力を促すようにするため空白にする。
                    OIT0005row("JRINSPECTIONDATE") = ""
                Next
                '○全検の場合
                For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION = '" + BaseDllConst.CONST_TANKSITUATION_14 + "'")
                    '★返送日が入力されている場合はSKIP
                    If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" Then Continue For
                    '★全検日の入力を促すようにするため空白にする。
                    OIT0005row("JRALLINSPECTIONDATE") = ""
                Next
            End If
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
        '★　　　　　 回送(修理・ＭＣ・交検・全検・留置・移動)
        If work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE09 _
           OrElse work.WF_COND_DETAILTYPE.Text = Me.WW_DETAILTYPE10 Then
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

        '○(一覧)テキストボックスの制御(読取専用)
        WW_ListTextBoxReadControl()

    End Sub

#Region "受注着駅到着後状況"
    ''' <summary>
    ''' 受注着駅到着後状況
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_UpdateOrderAfterSituation(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        '○チェック処理(交検・留置)
        For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION<>'" + BaseDllConst.CONST_TANKSITUATION_20 + "'")
            '★返送日が設定されているが、次回交検日が未入力の場合エラー
            If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" _
                AndAlso Convert.ToString(OIT0005row("JRINSPECTIONDATE")) = "" Then
                OIT0005row("ORDERINFONAME") = "未交検"
                O_RTN = "ERR"
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR,
                              "次回交検日が未入力です。",
                              needsPopUp:=True)
            Exit Sub
        End If

        Dim iresult As Integer
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '★【未卸】対象データ更新
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_20 + "'")
                '####################################
                '○受入日に日付が未設定の場合はSKIP
                '####################################
                If Convert.ToString(OIT0005row("ORDER_ACTUALACCDATE")) = "" Then Continue For

                '★①受注明細TBL更新(未卸を解除)
                WW_UpdateOrderDetail(SQLcon,
                                 I_OIT0005row:=OIT0005row,
                                 I_ITEM:="WHOLESALEFLG",
                                 I_VALUE:="2",
                                 I_WHOLESALE:=True)
                '★②受注明細TBL更新(未卸⇒留置に変更)
                WW_UpdateOrderDetail(SQLcon,
                                         I_OIT0005row:=OIT0005row,
                                         I_ITEM:="DETENTIONFLG",
                                         I_VALUE:="1")

                '★タンク車所在TBL更新
                WW_UpdateTankShozai(SQLcon,
                                    OIT0005row,
                                    I_LOCATION:=Convert.ToString(OIT0005row("DEPSTATION")),
                                    I_KBN:="E",
                                    I_STATUS:=BaseDllConst.CONST_TANKSTATUS_03,
                                    I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_22)

                '####################################
                '○空車着日に日付が未設定の場合はSKIP
                '####################################
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then Continue For

                '○ 日付妥当性チェック
                '例) iresult = dt1.Date.CompareTo(dt2.Date)
                '    iresultの意味
                '     0 : dt1とdt2は同じ日
                '    -1 : dt1はdt2より前の日
                '     1 : dt1はdt2より後の日
                '空車着日 と　現在日付を比較
                iresult = Date.Parse(Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE"))).CompareTo(DateTime.Today)

                '空車着日が同日より過去日で設定された場合
                If iresult <> 1 Then
                    '★受注明細TBL更新
                    WW_UpdateOrderDetail(SQLcon,
                                     I_OIT0005row:=OIT0005row,
                                     I_ITEM:="DETENTIONFLG",
                                     I_VALUE:="2")

                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row,
                                        I_LOCATION:=Convert.ToString(OIT0005row("DEPSTATION")),
                                        I_STATUS:=BaseDllConst.CONST_TANKSTATUS_02,
                                        I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01,
                                        I_USEORDERNO:=True)
                End If
            Next

            '★【交検】対象データ更新
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_21 + "'")
                '○空車着日に日付が未設定の場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then Continue For

                '○ 日付妥当性チェック
                '例) iresult = dt1.Date.CompareTo(dt2.Date)
                '    iresultの意味
                '     0 : dt1とdt2は同じ日
                '    -1 : dt1はdt2より前の日
                '     1 : dt1はdt2より後の日
                '空車着日 と　現在日付を比較
                iresult = Date.Parse(Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE"))).CompareTo(DateTime.Today)

                '空車着日が未来日で設定された場合
                If iresult = 1 Then
                    '★受注明細TBL更新(※未来日の場合は、交検のままとする)
                    WW_UpdateOrderDetail(SQLcon,
                                     I_OIT0005row:=OIT0005row,
                                     I_ITEM:="INSPECTIONFLG",
                                     I_VALUE:="1")

                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row)
                Else
                    '★受注明細TBL更新
                    WW_UpdateOrderDetail(SQLcon,
                                     I_OIT0005row:=OIT0005row,
                                     I_ITEM:="INSPECTIONFLG",
                                     I_VALUE:="2")

                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row,
                                        I_LOCATION:=Convert.ToString(OIT0005row("DEPSTATION")),
                                        I_STATUS:=BaseDllConst.CONST_TANKSTATUS_02,
                                        I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01,
                                        I_USEORDERNO:=True)
                End If
            Next

            '★【留置】対象データ更新
            For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_22 + "'")
                '○空車着日に日付が未設定の場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then Continue For

                '○ 日付妥当性チェック
                '例) iresult = dt1.Date.CompareTo(dt2.Date)
                '    iresultの意味
                '     0 : dt1とdt2は同じ日
                '    -1 : dt1はdt2より前の日
                '     1 : dt1はdt2より後の日
                '空車着日 と　現在日付を比較
                iresult = Date.Parse(Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE"))).CompareTo(DateTime.Today)

                '空車着日が未来日で設定された場合
                If iresult = 1 Then
                    '★受注明細TBL更新(※未来日の場合は、交検のままとする)
                    WW_UpdateOrderDetail(SQLcon,
                                     I_OIT0005row:=OIT0005row,
                                     I_ITEM:="DETENTIONFLG",
                                     I_VALUE:="1")

                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row)

                Else
                    '★受注明細TBL更新
                    WW_UpdateOrderDetail(SQLcon,
                                     I_OIT0005row:=OIT0005row,
                                     I_ITEM:="DETENTIONFLG",
                                     I_VALUE:="2")

                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row,
                                        I_LOCATION:=Convert.ToString(OIT0005row("DEPSTATION")),
                                        I_STATUS:=BaseDllConst.CONST_TANKSTATUS_02,
                                        I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01,
                                        I_USEORDERNO:=True)
                End If
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 受注明細TBL更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_UpdateOrderDetail(ByVal SQLcon As SqlConnection,
                                    Optional I_OIT0005row As DataRow = Nothing,
                                    Optional I_ITEM As String = Nothing,
                                    Optional I_VALUE As String = Nothing,
                                    Optional I_WHOLESALE As Boolean = False)
        Try
            '更新SQL文･･･受注明細TBLのステータスを更新
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0003_DETAIL " _
                    & "    SET " _
                    & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

            '○未卸が有効
            If I_WHOLESALE = True Then
                '★受入日を更新対象とする
                SQLStr &= "   ACTUALACCDATE = @ACTUALACCDATE, "
            End If

            SQLStr &=
                      "   ACTUALEMPARRDATE = @ACTUALEMPARRDATE, " _
                    & "        UPDYMD      = @UPDYMD, " _
                    & "        UPDUSER     = @UPDUSER, " _
                    & "        UPDTERMID   = @UPDTERMID, " _
                    & "        RECEIVEYMD  = @RECEIVEYMD  " _
                    & "  WHERE ORDERNO     = @ORDERNO  " _
                    & "    AND TANKNO      = @TANKNO  "
            '& "    AND DELFLG     <> @DELFLG; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
            Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", System.Data.SqlDbType.NVarChar)
            Dim P_ACTUALACCDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALACCDATE", System.Data.SqlDbType.Date)
            Dim P_ACTUALEMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALEMPARRDATE", System.Data.SqlDbType.Date)
            'Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            'P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            If IsNothing(I_OIT0005row) Then
                For Each OIT0005row As DataRow In OIT0005tbl.Rows
                    P_ORDERNO.Value = OIT0005row("ORDERNO")
                    P_TANKNO.Value = OIT0005row("TANKNUMBER")
                    P_ACTUALACCDATE.Value = OIT0005row("ORDER_ACTUALACCDATE")
                    If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then
                        P_ACTUALEMPARRDATE.Value = DBNull.Value
                    Else
                        P_ACTUALEMPARRDATE.Value = OIT0005row("ORDER_ACTUALEMPARRDATE")
                    End If
                    SQLcmd.ExecuteNonQuery()
                Next
            Else
                P_ORDERNO.Value = I_OIT0005row("ORDERNO")
                P_TANKNO.Value = I_OIT0005row("TANKNUMBER")
                P_ACTUALACCDATE.Value = I_OIT0005row("ORDER_ACTUALACCDATE")
                If Convert.ToString(I_OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then
                    P_ACTUALEMPARRDATE.Value = DBNull.Value
                Else
                    P_ACTUALEMPARRDATE.Value = I_OIT0005row("ORDER_ACTUALEMPARRDATE")
                End If
                SQLcmd.ExecuteNonQuery()
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L_ORDERDETAIL UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L_ORDERDETAIL UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        ''○メッセージ表示
        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    End Sub
#End Region

    ''' <summary>
    ''' (タンク車マスタTBL)の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankMaster(ByVal I_TANKNO As String,
                                      Optional I_ITEM As String = Nothing,
                                      Optional I_VALUE As String = Nothing)
        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･タンク車マスタTBL更新
            Dim SQLStr As String =
                      " UPDATE OIL.OIM0005_TANK " _
                    & " SET " _
                    & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

            SQLStr &=
                      "        UPDYMD         = @P11, " _
                    & "        UPDUSER        = @P12, " _
                    & "        UPDTERMID      = @P13, " _
                    & "        RECEIVEYMD     = @P14  " _
                    & "  WHERE TANKNUMBER     = @P01  " _
                    & "    AND DELFLG        <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  'タンク車№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA01.Value = I_TANKNO
            PARA02.Value = C_DELETE_FLG.DELETE

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L_TANKMASTER UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L_TANKMASTER UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' (タンク車所在TBL)の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal SQLcon As SqlConnection,
                                      ByVal I_OIT0005row As DataRow,
                                      Optional I_LOCATION As String = Nothing,
                                      Optional I_STATUS As String = Nothing,
                                      Optional I_KBN As String = Nothing,
                                      Optional I_SITUATION As String = Nothing,
                                      Optional I_USEORDERNO As Boolean = False)

        '更新SQL文･･･タンク車所在TBL更新
        Dim SQLStr As String =
                    " UPDATE OIL.OIT0005_SHOZAI " _
                    & "    SET "

        '○ 更新内容が指定されていれば追加する
        '所在地コード
        If Not String.IsNullOrEmpty(I_LOCATION) Then
            SQLStr &= String.Format("        LOCATIONCODE = '{0}', ", I_LOCATION)
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

        '空車着日(実績)
        SQLStr &= "      ACTUALEMPARRDATE = @ACTUALEMPARRDATE, "

        SQLStr &=
              "        UPDYMD         = @UPDYMD, " _
            & "        UPDUSER        = @UPDUSER, " _
            & "        UPDTERMID      = @UPDTERMID, " _
            & "        RECEIVEYMD     = @RECEIVEYMD  " _
            & "  WHERE TANKNUMBER     = @TANKNUMBER  " _
            & "    AND DELFLG        <> @DELFLG "

        Try
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim P_TANKNUMBER As SqlParameter = SQLcmd.Parameters.Add("@TANKNUMBER", System.Data.SqlDbType.NVarChar) 'タンク車№
            Dim P_ACTUALEMPARRDATE As SqlParameter = SQLcmd.Parameters.Add("@ACTUALEMPARRDATE", System.Data.SqlDbType.Date) '空車着日(実績)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)         '削除フラグ

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_TANKNUMBER.Value = I_OIT0005row("TANKNUMBER")
            If Convert.ToString(I_OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then
                P_ACTUALEMPARRDATE.Value = DBNull.Value
            Else
                P_ACTUALEMPARRDATE.Value = I_OIT0005row("ORDER_ACTUALEMPARRDATE")
            End If
            P_DELFLG.Value = C_DELETE_FLG.DELETE

            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

#Region "回送後状況"
    ''' <summary>
    ''' 回送後状況
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_UpdateKaisouAfterSituation(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        '○チェック処理(交検)
        For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_13 + "'")
            '★返送日が設定されているが、次回交検日が未入力の場合エラー
            If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" _
                AndAlso Convert.ToString(OIT0005row("JRINSPECTIONDATE")) = "" Then
                OIT0005row("ORDERINFONAME") = "未交検"
                O_RTN = "ERR"
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR,
                              "次回交検日が未入力です。",
                              needsPopUp:=True)
            Exit Sub
        End If
        '○チェック処理(全検)
        For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_14 + "'")
            '★返送日が設定されているが、次回全検日が未入力の場合エラー
            If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" _
                AndAlso Convert.ToString(OIT0005row("JRALLINSPECTIONDATE")) = "" Then
                OIT0005row("ORDERINFONAME") = "未全検"
                O_RTN = "ERR"
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR,
                              "次回全検日が未入力です。",
                              needsPopUp:=True)
            Exit Sub
        End If
        '　全検実施に伴い交検も変更があるため、交検の入力チェックもあわせて実施する
        For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKSITUATION='" + BaseDllConst.CONST_TANKSITUATION_14 + "'")
            '★返送日が設定されているが、次回交検日が未入力の場合エラー
            If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) <> "" _
                AndAlso Convert.ToString(OIT0005row("JRINSPECTIONDATE")) = "" Then
                OIT0005row("ORDERINFONAME") = "未交検"
                O_RTN = "ERR"
            End If
        Next
        If O_RTN = "ERR" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR,
                              "次回交検日が未入力です。",
                              needsPopUp:=True)
            Exit Sub
        End If

        Dim iresult As Integer
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '★【修理】【ＭＣ】【交検】【全検】【留置】対象データ更新
            For Each OIT0005row As DataRow In OIT0005tbl.Rows
                '○返送日に日付が未設定の場合はSKIP
                If Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")) = "" Then Continue For

                '○ 日付妥当性チェック
                '例) iresult = dt1.Date.CompareTo(dt2.Date)
                '    iresultの意味
                '     0 : dt1とdt2は同じ日
                '    -1 : dt1はdt2より前の日
                '     1 : dt1はdt2より後の日
                '返送日 と　現在日付を比較
                iresult = Date.Parse(Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE"))).CompareTo(DateTime.Today)

                '★回送明細TBL更新
                WW_UpdateKaisouDetail(SQLcon, I_OIT0005row:=OIT0005row,
                                      I_ITEM:="ACTUALEMPARRDATE",
                                      I_VALUE:=Convert.ToString(OIT0005row("ORDER_ACTUALEMPARRDATE")))

                '返送日が未来日で設定された場合
                If iresult = 1 Then
                    '★タンク車所在TBL更新
                    WW_UpdateTankShozai(SQLcon,
                                        OIT0005row)
                Else
                    '○回送登録画面より登録した場合
                    If Convert.ToString(OIT0005row("USEORDERNO")) <> "" Then
                        '★タンク車所在TBL更新
                        WW_UpdateTankShozai(SQLcon,
                                        OIT0005row,
                                        I_LOCATION:=Convert.ToString(OIT0005row("DEPSTATION")),
                                        I_STATUS:=BaseDllConst.CONST_TANKSTATUS_02,
                                        I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01,
                                        I_USEORDERNO:=True)
                    Else
                        '★タンク車所在TBL更新
                        WW_UpdateTankShozai(SQLcon,
                                        OIT0005row,
                                        I_STATUS:=BaseDllConst.CONST_TANKSTATUS_02,
                                        I_SITUATION:=BaseDllConst.CONST_TANKSITUATION_01)
                    End If
                End If
            Next

            '○回送TBLの回送進行ステータス更新チェック
            For Each OIT0005row As DataRow In OIT0005tbl.Select("ISNULL(KAISOUNO,'')<>'' AND ORDER_ACTUALEMPARRDATE<>''", "KAISOUNO")
                '★回送進行ステータスチェック用
                If WW_CheckKaisouCnt(SQLcon, Convert.ToString(OIT0005row("KAISOUNO"))) = True Then
                    '★回送TBL更新
                    WW_UpdateKaisou(SQLcon, I_OIT0005row:=OIT0005row,
                                I_ITEM:="KAISOUSTATUS",
                                I_VALUE:=BaseDllConst.CONST_KAISOUSTATUS_500)
                End If
            Next
            'WW_CheckKaisouCnt(SQLcon)
            'For Each OIT0005row As DataRow In OIT0005tbl.Select("ISNULL(KAISOUNO,'')<>''")
            '    '○対象のオーダー回送の返送日がすべて登録済みか確認
            '    If OIT0005KaisouCnttbl.Select("KAISOUNO='" + Convert.ToString(OIT0005row("KAISOUNO")) + "'").Count <> 0 Then Continue For

            '    '★回送TBL更新
            '    WW_UpdateKaisou(SQLcon, I_OIT0005row:=OIT0005row,
            '                    I_ITEM:="KAISOUSTATUS",
            '                    I_VALUE:=BaseDllConst.CONST_KAISOUSTATUS_500)

            'Next

        End Using
    End Sub
    ''' <summary>
    ''' 回送明細TBL更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_UpdateKaisouDetail(ByVal SQLcon As SqlConnection,
                                     Optional I_OIT0005row As DataRow = Nothing,
                                     Optional I_ITEM As String = Nothing,
                                     Optional I_VALUE As String = Nothing)
        Try
            '更新SQL文･･･回送明細TBLのステータスを更新
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0007_KAISOUDETAIL " _
                    & "    SET " _
                    & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

            SQLStr &=
                      "        UPDYMD      = @UPDYMD, " _
                    & "        UPDUSER     = @UPDUSER, " _
                    & "        UPDTERMID   = @UPDTERMID, " _
                    & "        RECEIVEYMD  = @RECEIVEYMD  " _
                    & "  WHERE KAISOUNO    = @KAISOUNO  " _
                    & "    AND TANKNO      = @TANKNO  "
            '& "    AND DELFLG     <> @DELFLG; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            Dim P_KAISOUNO As SqlParameter = SQLcmd.Parameters.Add("@KAISOUNO", System.Data.SqlDbType.NVarChar)
            Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", System.Data.SqlDbType.NVarChar)
            'Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            'P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            If IsNothing(I_OIT0005row) Then
                For Each OIT0005row As DataRow In OIT0005tbl.Rows
                    P_KAISOUNO.Value = OIT0005row("KAISOUNO")
                    P_TANKNO.Value = OIT0005row("TANKNUMBER")
                    SQLcmd.ExecuteNonQuery()
                Next
            Else
                P_KAISOUNO.Value = I_OIT0005row("KAISOUNO")
                P_TANKNO.Value = I_OIT0005row("TANKNUMBER")
                SQLcmd.ExecuteNonQuery()
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L_KAISOUDETAIL UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L_KAISOUDETAIL UPDATE"
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
    ''' 回送TBL更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_UpdateKaisou(ByVal SQLcon As SqlConnection,
                                     Optional I_OIT0005row As DataRow = Nothing,
                                     Optional I_ITEM As String = Nothing,
                                     Optional I_VALUE As String = Nothing)

        Try
            '更新SQL文･･･回送TBLのステータスを更新
            Dim SQLStr As String =
                      " UPDATE OIL.OIT0006_KAISOU " _
                    & "    SET " _
                    & String.Format("        {0}  = '{1}', ", I_ITEM, I_VALUE)

            SQLStr &=
                      "        UPDYMD      = @UPDYMD, " _
                    & "        UPDUSER     = @UPDUSER, " _
                    & "        UPDTERMID   = @UPDTERMID, " _
                    & "        RECEIVEYMD  = @RECEIVEYMD  " _
                    & "  WHERE KAISOUNO    = @KAISOUNO  " _
                    & String.Format("    AND KAISOUSTATUS < '{0}'", BaseDllConst.CONST_KAISOUSTATUS_500)
            '& "    AND DELFLG     <> @DELFLG; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300
            Dim P_KAISOUNO As SqlParameter = SQLcmd.Parameters.Add("@KAISOUNO", System.Data.SqlDbType.NVarChar)
            'Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            'P_DELFLG.Value = C_DELETE_FLG.DELETE
            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = Master.USERID
            P_UPDTERMID.Value = Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            If IsNothing(I_OIT0005row) Then
                For Each OIT0005row As DataRow In OIT0005tbl.Rows
                    P_KAISOUNO.Value = OIT0005row("KAISOUNO")
                    SQLcmd.ExecuteNonQuery()
                Next
            Else
                P_KAISOUNO.Value = I_OIT0005row("KAISOUNO")
                SQLcmd.ExecuteNonQuery()
            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L_KAISOU UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L_KAISOU UPDATE"
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
    ''' 回送進行ステータスチェック用
    ''' </summary>
    ''' <remarks></remarks>
    Public Function WW_CheckKaisouCnt(ByVal SQLcon As SqlConnection, ByVal I_KAISOUNO As String) As Boolean
        Dim blnFlg As Boolean = False
        If IsNothing(OIT0005KaisouCnttbl) Then
            OIT0005KaisouCnttbl = New DataTable
        End If

        If OIT0005KaisouCnttbl.Columns.Count <> 0 Then
            OIT0005KaisouCnttbl.Columns.Clear()
        End If

        OIT0005KaisouCnttbl.Clear()

        Dim SQLStr As String = ""
        SQLStr =
          " SELECT " _
        & "   OIT0007_ALL.KAISOUNO " _
        & " , OIT0007_ALL.ALLCNT " _
        & " , OIT0007_CHK.CHKCNT " _
        & " , CASE WHEN OIT0007_ALL.ALLCNT = OIT0007_CHK.CHKCNT THEN '0' ELSE '1' END AS CHK_FLG " _
        & " FROM " _
        & " ( " _
        & "      SELECT " _
        & "        OIT0007.KAISOUNO" _
        & "      , count(1) AS ALLCNT" _
        & "      FROM oil.OIT0007_KAISOUDETAIL OIT0007 " _
        & String.Format("      WHERE OIT0007.KAISOUNO = '{0}' ", I_KAISOUNO) _
        & String.Format("        AND OIT0007.DELFLG  <> '{0}' ", C_DELETE_FLG.DELETE) _
        & "      GROUP BY OIT0007.KAISOUNO " _
        & " ) OIT0007_ALL " _
        & " INNER JOIN " _
        & " ( " _
        & "      SELECT " _
        & "        OIT0007.KAISOUNO" _
        & "      , SUM(CASE WHEN OIT0007.ACTUALEMPARRDATE <= FORMAT(GETDATE(),'yyyy/MM/dd') Then 1 Else 0 End) AS CHKCNT" _
        & "      FROM oil.OIT0007_KAISOUDETAIL OIT0007 " _
        & String.Format("      WHERE OIT0007.KAISOUNO = '{0}' ", I_KAISOUNO) _
        & String.Format("        AND OIT0007.DELFLG  <> '{0}' ", C_DELETE_FLG.DELETE) _
        & "      GROUP BY OIT0007.KAISOUNO " _
        & " ) OIT0007_CHK ON " _
        & " OIT0007_ALL.KAISOUNO = OIT0007_CHK.KAISOUNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                'Dim DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)  '削除フラグ
                'DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0005KaisouCnttbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0005KaisouCnttbl.Load(SQLdr)
                End Using

                '★対象の回送登録すべてが返送日が設定(未来日は除く)されている場合
                If Convert.ToString(OIT0005KaisouCnttbl.Rows(0)("CHK_FLG")) = "0" Then blnFlg = True

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005L CHECKKAISOUCNT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005L CHECKKAISOUCNT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return blnFlg
            'Exit Sub
        End Try
        Return blnFlg
    End Function

#End Region

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ListTextBoxReadControl()

        Select Case work.WF_COND_DETAILTYPE.Text
            '★その他状況(受注(未卸中・交検中・留置中))
            Case "9"
                WW_OrderListTextBoxReadControl()
            '★その他状況(回送(修理・ＭＣ・交検・全検・留置・移動))
            Case "10"
                WW_KaisouListTextBoxReadControl()
        End Select

    End Sub

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' 　　　(受注着駅到着後状況)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_OrderListTextBoxReadControl()
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
                loopdr = OIT0005tbl.Select("HIDDEN<>'1'")(rowIdx)
                'loopdr = OIT0005tbl.Rows(rowIdx)
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
                    '(一覧)交検の設定
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JRINSPECTIONDATE") Then
                        If Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_21 _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                    End If

                    '(一覧)受入日
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDER_ACTUALACCDATE") Then
                        If loopdr("WHOLESALEFLG").ToString() = "" AndAlso loopdr("WHOLESALECHGFLG").ToString() = "0" Then
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        End If
                    End If

                    '(一覧)返送日
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

    ''' <summary>
    ''' (一覧)テキストボックスの制御(読取専用)
    ''' 　　　(回送後状況)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_KaisouListTextBoxReadControl()
        '〇 (一覧)テキストボックスの制御(読取専用)
        Dim divObj = DirectCast(pnlListArea.FindControl(pnlListArea.ID & "_DR"), Panel)
        Dim tblObj = DirectCast(divObj.Controls(0), Table)
        Dim chkObjType As String = ""

        '★修理チェックボックス用
        Dim chkObjREP As CheckBox = Nothing
        Dim chkObjIdWOREPcnt As String = "chk" & pnlListArea.ID & "REPAIRFLG"
        Dim chkObjREPId As String = ""
        '★ＭＣチェックボックス用
        Dim chkObjMC As CheckBox = Nothing
        Dim chkObjIdWOMCcnt As String = "chk" & pnlListArea.ID & "MCFLG"
        Dim chkObjMCId As String = ""
        '★交検チェックボックス用
        Dim chkObjINS As CheckBox = Nothing
        Dim chkObjIdWOINScnt As String = "chk" & pnlListArea.ID & "INSPECTIONFLG"
        Dim chkObjINSId As String = ""
        '★全検チェックボックス用
        Dim chkObjAINS As CheckBox = Nothing
        Dim chkObjIdWOAINScnt As String = "chk" & pnlListArea.ID & "ALLINSPECTIONFLG"
        Dim chkObjAINSId As String = ""
        '★留置チェックボックス用
        Dim chkObjIND As CheckBox = Nothing
        Dim chkObjIdWOINDcnt As String = "chk" & pnlListArea.ID & "INDWELLINGFLG"
        Dim chkObjINDId As String = ""
        '★移動チェックボックス用
        Dim chkObjMV As CheckBox = Nothing
        Dim chkObjIdWOMVcnt As String = "chk" & pnlListArea.ID & "MOVEFLG"
        Dim chkObjMVId As String = ""

        '　ループ内の対象データROW(これでXXX項目の値をとれるかと）
        Dim loopdr As DataRow = Nothing
        '　データテーブルの行Index
        Dim rowIdx As Integer = 0

        For Each rowitem As TableRow In tblObj.Rows
            '○修理・ＭＣ・交検・全検・留置・移動(チェックボックス)の制御
            If OIT0005tbl.Rows.Count <> 0 Then
                For Each OIT0005row As DataRow In OIT0005tbl.Select("TANKNUMBER='" + rowitem.Cells.Item(1).Text + "'")
                    loopdr = OIT0005row
                    Exit For
                Next
                If loopdr Is Nothing Then loopdr = OIT0005tbl.Rows(rowIdx)
                '★修理
                chkObjREPId = chkObjIdWOREPcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjREP = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjREP = DirectCast(cellObj.FindControl(chkObjREPId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjREP IsNot Nothing _
                        AndAlso (loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_11 _
                                 AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_05) Then
                        '修理フラグ(チェックボックス)を非活性
                        chkObjREP.Enabled = False
                        Exit For
                    End If
                Next
                '★ＭＣ
                chkObjMCId = chkObjIdWOMCcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjMC = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjMC = DirectCast(cellObj.FindControl(chkObjMCId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjMC IsNot Nothing _
                        AndAlso (loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_12 _
                                 AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_07) Then
                        'ＭＣフラグ(チェックボックス)を非活性
                        chkObjMC.Enabled = False
                        Exit For
                    End If
                Next
                '★交検
                chkObjINSId = chkObjIdWOINScnt & Convert.ToString(loopdr("LINECNT"))
                chkObjINS = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjINS = DirectCast(cellObj.FindControl(chkObjINSId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjINS IsNot Nothing _
                        AndAlso (loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_13 _
                                 AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_03) Then
                        '交検フラグ(チェックボックス)を非活性
                        chkObjINS.Enabled = False
                        Exit For
                    End If
                Next
                '★全検
                chkObjAINSId = chkObjIdWOAINScnt & Convert.ToString(loopdr("LINECNT"))
                chkObjAINS = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjAINS = DirectCast(cellObj.FindControl(chkObjAINSId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjAINS IsNot Nothing _
                        AndAlso (loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_14 _
                                 AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_04) Then
                        '全検フラグ(チェックボックス)を非活性
                        chkObjAINS.Enabled = False
                        Exit For
                    End If
                Next
                '★留置
                chkObjINDId = chkObjIdWOINDcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjIND = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjIND = DirectCast(cellObj.FindControl(chkObjINDId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjIND IsNot Nothing _
                        AndAlso (loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_15 _
                                 AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_06) Then
                        '留置フラグ(チェックボックス)を非活性
                        chkObjIND.Enabled = False
                        Exit For
                    End If
                Next
                '★移動
                chkObjMVId = chkObjIdWOMVcnt & Convert.ToString(loopdr("LINECNT"))
                chkObjMV = Nothing
                For Each cellObj As TableCell In rowitem.Controls
                    chkObjMV = DirectCast(cellObj.FindControl(chkObjMVId), CheckBox)
                    'コントロールが見つかったら脱出
                    If chkObjMV IsNot Nothing _
                        AndAlso loopdr("TANKSITUATION").ToString() <> BaseDllConst.CONST_TANKSITUATION_08 Then
                        '移動フラグ(チェックボックス)を非活性
                        chkObjMV.Enabled = False
                        Exit For
                    End If
                Next

                '○次回交検日・次回全検日・返送日(テキストボックス)の制御
                For Each cellObj As TableCell In rowitem.Controls
                    If cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JRINSPECTIONDATE") Then
                        '★交検の設定
                        If (Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_13 _
                            OrElse Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_03) _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            cellObj.Text = cellObj.Text.Replace(">", " class='iconOnly'>")
                            'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                            '★全検の設定(次回交検日の入力解除)
                        ElseIf (Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_14 _
                            OrElse Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_04) _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            cellObj.Text = cellObj.Text.Replace(">", " class='iconOnly'>")
                            'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "JRALLINSPECTIONDATE") Then
                        '★全検の設定
                        If (Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_14 _
                            OrElse Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_04) _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            cellObj.Text = cellObj.Text.Replace(">", " class='iconOnly'>")
                            'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "ORDER_ACTUALEMPARRDATE") Then
                        '★返送日の設定
                        'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly showDeleteIcon'>")
                        cellObj.Text = cellObj.Text.Replace(">", " class='iconOnly'>")
                        'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")

                        '### 20210512 START 指摘票対応(No479)全体 #################################################
                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "MYWEIGHT") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "INSPECTMEMBERNAME") Then
                        '★自重, 点検実施者(社員名)
                        '○タンク車状況(全検)の場合のみ入力可能とする。
                        If (Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_14 _
                            OrElse Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_04) _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            'cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If

                    ElseIf cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "INTERINSPECTYM") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "INTERINSPECTDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "INTERINSPECTSTATION") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "INTERINSPECTORGCODE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SELFINSPECTYM") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SELFINSPECTDATE") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SELFINSPECTSTATION") _
                        OrElse cellObj.Text.Contains("input id=""txt" & pnlListArea.ID & "SELFINSPECTORGCODE") Then
                        '★中間点検年月の設定, 中間点検場所の設定, 中間点検実施者
                        '　自主点検年月      , 自主点検場所      , 自主点検実施者

                        '○タンク車状況(全検)の場合のみ入力可能とする。
                        If (Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_14 _
                            OrElse Convert.ToString(loopdr("TANKSITUATION")) = BaseDllConst.CONST_TANKSITUATION_04) _
                            AndAlso Convert.ToString(loopdr("ORDER_ACTUALEMPARRDATE")) <> "" Then
                            'cellObj.Text = cellObj.Text.Replace(">", " class='iconOnly'>")
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly' class='iconOnly'>")
                        Else
                            cellObj.Text = cellObj.Text.Replace(">", " readonly='readonly'>")
                        End If
                        '### 20210512 END   指摘票対応(No479)全体 #################################################

                    End If
                Next

            End If
            rowIdx += 1
        Next

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
                                          ByRef O_VALUE() As String)

        If IsNothing(OIT0005Fixvaltbl) Then
            OIT0005Fixvaltbl = New DataTable
        End If

        If OIT0005Fixvaltbl.Columns.Count <> 0 Then
            OIT0005Fixvaltbl.Columns.Clear()
        End If

        OIT0005Fixvaltbl.Clear()

        Try
            'DBより取得
            OIT0005Fixvaltbl = WW_FixvalueMasterDataGet(I_CODE, I_CLASS, I_KEYCODE)

            If I_KEYCODE.Equals("") Then
                Dim i As Integer = 0
                For Each OIT0003WKrow As DataRow In OIT0005Fixvaltbl.Rows
                    Try
                        O_VALUE(i) = Convert.ToString(OIT0003WKrow("KEYCODE"))
                        i += 1
                    Catch ex As Exception
                        Exit For
                    End Try
                Next
            Else
                For Each OIT0003WKrow As DataRow In OIT0005Fixvaltbl.Rows
                    For i = 1 To O_VALUE.Length
                        O_VALUE(i - 1) = Convert.ToString(OIT0003WKrow("VALUE" & i.ToString()))
                    Next
                Next
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' マスタ検索処理（同じパラメータならDB抽出せずに保持内容を返却）
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <returns></returns>
    Private Function WW_FixvalueMasterDataGet(I_CODE As String, I_CLASS As String, I_KEYCODE As String) As DataTable
        Static keyValues As Dictionary(Of String, String)
        Static retDt As DataTable
        Dim retFilterdDt As DataTable
        'キー情報を比較または初期状態または異なるキーの場合は再抽出
        If keyValues Is Nothing OrElse
           (Not (keyValues("I_CODE") = I_CODE _
                 AndAlso keyValues("I_CLASS") = I_CLASS)) Then
            keyValues = New Dictionary(Of String, String) _
                      From {{"I_CODE", I_CODE}, {"I_CLASS", I_CLASS}}
            retDt = New DataTable
        Else
            retFilterdDt = retDt
            '抽出キー情報が一致しているので保持内容を返却
            If I_KEYCODE <> "" Then
                Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
                If qKeyFilterd.Any Then
                    retFilterdDt = qKeyFilterd.CopyToDataTable
                Else
                    retFilterdDt = retDt.Clone
                End If
            End If

            Return retFilterdDt
        End If
        'キーが変更された場合の抽出処理
        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        SqlConnection.ClearPool(SQLcon)

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
                    retDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                retDt.Load(SQLdr)
            End Using
            'CLOSE
            SQLcmd.Dispose()
        End Using

        retFilterdDt = retDt
        '抽出キー情報が一致しているので保持内容を返却
        If I_KEYCODE <> "" Then
            Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
            If qKeyFilterd.Any Then
                retFilterdDt = qKeyFilterd.CopyToDataTable
            Else
                retFilterdDt = retDt.Clone
            End If
        End If

        Return retFilterdDt
    End Function
End Class
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 費用管理詳細
''' </summary>
''' <remarks></remarks>
Public Class OIT0008CostDetail
    Inherits Page

    '○ 検索結果格納Table
    Private OIT0008tbl As DataTable                                 ' 一覧格納用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                ' 1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 ' マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    ' ログ出力
    Private CS0013ProfView As New CS0013ProfView                    ' Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      ' 更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  ' 権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        ' 帳票出力
    Private CS0050SESSION As New CS0050SESSION                      ' セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    ' サブ用リターンコード
    Private WW_CARSNUMBER_SUM As Long = 0
    Private WW_QUANTITY_SUM As Double = 0.0
    Private WW_AMOUNT_SUM As Long = 0
    Private WW_TAX_SUM As Long = 0
    Private WW_TOTAL_SUM As Long = 0

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
                    'Master.RecoverTable(OIT0008tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonCSV"             ' ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonPrint"           ' 一覧印刷ボタン押下
                            WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             ' 戻るボタン押下
                            WF_ButtonEND_Click()
                        'Case "WF_ButtonFIRST"           ' 先頭頁ボタン押下
                        '    WF_ButtonFIRST_Click()
                        'Case "WF_ButtonLAST"            ' 最終頁ボタン押下
                        '    WF_ButtonLAST_Click()
                        'Case "WF_MouseWheelUp"          ' マウスホイール(Up)
                        '    WF_Grid_Scroll()
                        'Case "WF_MouseWheelDown"        ' マウスホイール(Down)
                        '    WF_Grid_Scroll()
                        Case "WF_RadioButonClick"       ' (右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            ' (右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select

                    '○ 一覧再表示処理
                    'DisplayGrid()
                    GridViewInitialize()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            WF_MAPpermitcode.Value = "FALSE"

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0008tbl) Then
                OIT0008tbl.Clear()
                OIT0008tbl.Dispose()
                OIT0008tbl = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0008WRKINC.MAPIDD
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = False
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

        Dim seachCode As String = ""
        Dim seachedName As String = ""

        '#
        TxtLine.Text = work.WF_SEL_LINE.Text.Trim()

        '勘定科目コード
        TxtAccountCode.Text = work.WF_SEL_ACCOUNTCODE.Text
        '勘定科目名
        TxtAccountName.Text = work.WF_SEL_ACCOUNTNAME.Text
        'セグメント
        TxtSegmentCode.Text = work.WF_SEL_SEGMENTCODE.Text
        'セグメント名
        TxtSegmentName.Text = work.WF_SEL_SEGMENTNAME.Text
        'セグメント枝番
        TxtSegmentBranchCode.Text = work.WF_SEL_SEGMENTBRANCHCODE.Text
        'セグメント枝番名
        TxtSegmentBranchName.Text = work.WF_SEL_SEGMENTBRANCHNAME.Text
        '荷主コード
        TxtShippersCode.Text = work.WF_SEL_SHIPPERSCODE.Text
        '荷主名
        TxtShippersName.Text = work.WF_SEL_SHIPPERSNAME.Text
        '請求先コード
        TxtInvoiceCode.Text = work.WF_SEL_INVOICECODE.Text
        '請求先名
        TxtInvoiceName.Text = work.WF_SEL_INVOICENAME.Text
        '請求先部門
        TxtInvoiceDeptName.Text = work.WF_SEL_INVOICEDEPTNAME.Text
        '支払先コード
        TxtPayeeCode.Text = work.WF_SEL_PAYEECODE.Text
        '支払先名
        TxtPayeeName.Text = work.WF_SEL_PAYEENAME.Text
        '支払先部門
        TxtPayeeDeptName.Text = work.WF_SEL_PAYEEDEPTNAME.Text

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        ' 登録画面からの遷移の場合はテーブルから取得しない
        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        WF_CONSIGNEELIST.DataSource = OIT0008tbl
        WF_CONSIGNEELIST.DataBind()

    End Sub

    ''' <summary>
    ''' フッター行の値算出
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub WF_CONSIGNEELIST_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles WF_CONSIGNEELIST.RowDataBound
        Select Case e.Row.RowType
            Case DataControlRowType.DataRow
                Dim row = DirectCast(e.Row.DataItem, DataRowView)

                If Not row("OILCODE") = "9999" Then Exit Sub

                If Not row("CARSNUMBER") Is DBNull.Value Then
                    WW_CARSNUMBER_SUM += row("CARSNUMBER")
                End If
                If Not row("QUANTITY") Is DBNull.Value Then
                    WW_QUANTITY_SUM += row("QUANTITY")
                End If
                If Not row("AMOUNT") Is DBNull.Value Then
                    WW_AMOUNT_SUM += row("AMOUNT")
                End If
                If Not row("TAX") Is DBNull.Value Then
                    WW_TAX_SUM += row("TAX")
                End If
                If Not row("TOTAL") Is DBNull.Value Then
                    WW_TOTAL_SUM += row("TOTAL")
                End If
        End Select
    End Sub

    ''' <summary>
    ''' 明細リストの体裁を整える
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub WF_CONSIGNEELIST_DataBound(sender As Object, e As EventArgs) Handles WF_CONSIGNEELIST.DataBound

        'GridView本体を取得
        Dim grid As GridView = CType(sender, GridView)

        Dim lastCONSIGNEENAME As String = ""
        For Each gvrow As GridViewRow In CType(grid.Controls(0), Table).Rows
            If gvrow.RowType = DataControlRowType.Footer Then
                'フッター
                gvrow.Cells(2).Text = String.Format("{0:#,##0.000}", WW_QUANTITY_SUM)
                gvrow.Cells(3).Text = String.Format("{0:#,##0}", WW_CARSNUMBER_SUM)
                gvrow.Cells(4).Text = String.Format("{0:#,##0}", WW_AMOUNT_SUM)
                gvrow.Cells(5).Text = String.Format("{0:#,##0}", WW_TAX_SUM)
                gvrow.Cells(6).Text = String.Format("{0:#,##0}", WW_TOTAL_SUM)
            ElseIf gvrow.RowType = DataControlRowType.DataRow Then
                'データ行
                If Not String.IsNullOrEmpty(lastCONSIGNEENAME) Then
                    If lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text Then
                        '前回出現した荷受人名と現在行の荷受人名が一致する場合
                        '荷受人名をクリアする
                        DirectCast(gvrow.Cells(0).Controls(3), Label).Text = ""
                    Else
                        lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text
                    End If
                Else
                    lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text
                End If

                If DirectCast(gvrow.Cells(1).Controls(1), HiddenField).Value = "9999" Then
                    gvrow.Cells(0).CssClass = "CONSIGNEENAME centerText AllBorder"
                    gvrow.Cells(0).ColumnSpan = "2"
                    gvrow.Cells(1).Visible = False
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0008tbl) Then
            OIT0008tbl = New DataTable
        End If

        If OIT0008tbl.Columns.Count <> 0 Then
            OIT0008tbl.Columns.Clear()
        End If

        OIT0008tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを列車マスタから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     UNIQ.CONSIGNEECODE")    '荷受人コード
        SQLStrBldr.AppendLine("     , UNIQ.CONSIGNEENAME")  '荷受人名
        SQLStrBldr.AppendLine("     , UNIQ.OILCODE")        '油種コード
        SQLStrBldr.AppendLine("     , UNIQ.OILNAME")        '油種名
        SQLStrBldr.AppendLine("     , UNIQ.CARSNUMBER")     '車数
        SQLStrBldr.AppendLine("     , UNIQ.QUANTITY")       '数量
        SQLStrBldr.AppendLine("     , UNIQ.AMOUNT")         '金額
        SQLStrBldr.AppendLine("     , UNIQ.TAX")            '税額
        SQLStrBldr.AppendLine("     , UNIQ.TOTAL")          '総額
        SQLStrBldr.AppendLine(" FROM (")
        '荷受人・油種単位での集計値
        SQLStrBldr.AppendLine("     SELECT")
        SQLStrBldr.AppendLine("         OIT0013.CONSIGNEECODE")
        SQLStrBldr.AppendLine("         , OIT0013.CONSIGNEENAME")
        SQLStrBldr.AppendLine("         , OIT0013.OILCODE")
        SQLStrBldr.AppendLine("         , OIT0013.OILNAME")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.CARSNUMBER) AS CARSNUMBER")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.CARSAMOUNT) AS QUANTITY")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.APPLYCHARGE) AS AMOUNT")
        SQLStrBldr.AppendLine("         , SUM(FLOOR(OIT0013.APPLYCHARGE * 0.10)) AS TAX")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.APPLYCHARGE) + SUM(FLOOR(OIT0013.APPLYCHARGE * 0.10)) AS TOTAL")
        SQLStrBldr.AppendLine("     FROM")
        SQLStrBldr.AppendLine("         [oil].OIT0013_ORDERDETAILBILLING OIT0013")
        SQLStrBldr.AppendLine("         INNER JOIN (")
        SQLStrBldr.AppendLine("             SELECT")
        SQLStrBldr.AppendLine("                 *")
        SQLStrBldr.AppendLine("             FROM")
        SQLStrBldr.AppendLine("                 oil.TMP0008_COST")
        SQLStrBldr.AppendLine("             WHERE")
        SQLStrBldr.AppendLine("                 LINE = @P04")
        SQLStrBldr.AppendLine("             AND OFFICECODE = @P01")
        SQLStrBldr.AppendLine("             AND KEIJYOYM = @P02")
        SQLStrBldr.AppendLine("         ) SEL_TMP")
        SQLStrBldr.AppendLine("         ON  SEL_TMP.ACCOUNTCODE = OIT0013.ACCOUNTCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.ACCOUNTNAME = OIT0013.ACCOUNTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTCODE = OIT0013.SEGMENTCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTNAME = OIT0013.SEGMENTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTBRANCHCODE = OIT0013.BREAKDOWNCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTBRANCHNAME = OIT0013.BREAKDOWN")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SHIPPERSCODE = OIT0013.SHIPPERSCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICECODE = OIT0013.INVOICECODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICENAME = OIT0013.INVOICENAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICEDEPTNAME = OIT0013.INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEECODE = OIT0013.PAYEECODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEENAME = OIT0013.PAYEENAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEEDEPTNAME = OIT0013.PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.OFFICECODE = OIT0013.OFFICECODE")
        SQLStrBldr.AppendLine("         AND OIT0013.KEIJYOYMD BETWEEN @P02 AND @P03")
        SQLStrBldr.AppendLine("         AND OIT0013.DELFLG <> @P00")
        SQLStrBldr.AppendLine("     GROUP BY")
        SQLStrBldr.AppendLine("         OIT0013.CONSIGNEECODE")
        SQLStrBldr.AppendLine("         , OIT0013.CONSIGNEENAME")
        SQLStrBldr.AppendLine("         , OIT0013.OILCODE")
        SQLStrBldr.AppendLine("         , OIT0013.OILNAME")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine("     UNION ALL")
        SQLStrBldr.AppendLine(" ")
        '荷受人単位での集計値
        SQLStrBldr.AppendLine("     SELECT")
        SQLStrBldr.AppendLine("         CONSIGNEECODE")
        SQLStrBldr.AppendLine("         , CONSIGNEENAME + '計' AS CONSIGNEENAME")
        SQLStrBldr.AppendLine("         , '9999' AS OILCODE")
        SQLStrBldr.AppendLine("         , '' AS OILNAME")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.CARSNUMBER) AS CARSNUMBER")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.CARSAMOUNT) AS QUANTITY")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.APPLYCHARGE) AS AMOUNT")
        SQLStrBldr.AppendLine("         , SUM(FLOOR(OIT0013.APPLYCHARGE * 0.10)) AS TAX")
        SQLStrBldr.AppendLine("         , SUM(OIT0013.APPLYCHARGE) + SUM(FLOOR(OIT0013.APPLYCHARGE * 0.10)) AS TOTAL")
        SQLStrBldr.AppendLine("     FROM")
        SQLStrBldr.AppendLine("         [oil].OIT0013_ORDERDETAILBILLING OIT0013")
        SQLStrBldr.AppendLine("         INNER JOIN (")
        SQLStrBldr.AppendLine("             SELECT")
        SQLStrBldr.AppendLine("                 *")
        SQLStrBldr.AppendLine("             FROM")
        SQLStrBldr.AppendLine("                 oil.TMP0008_COST")
        SQLStrBldr.AppendLine("             WHERE")
        SQLStrBldr.AppendLine("                 LINE = @P04")
        SQLStrBldr.AppendLine("             AND OFFICECODE = @P01")
        SQLStrBldr.AppendLine("             AND KEIJYOYM = @P02")
        SQLStrBldr.AppendLine("         ) SEL_TMP")
        SQLStrBldr.AppendLine("         ON  SEL_TMP.ACCOUNTCODE = OIT0013.ACCOUNTCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.ACCOUNTNAME = OIT0013.ACCOUNTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTCODE = OIT0013.SEGMENTCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTNAME = OIT0013.SEGMENTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTBRANCHCODE = OIT0013.BREAKDOWNCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SEGMENTBRANCHNAME = OIT0013.BREAKDOWN")
        SQLStrBldr.AppendLine("         AND SEL_TMP.SHIPPERSCODE = OIT0013.SHIPPERSCODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICECODE = OIT0013.INVOICECODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICENAME = OIT0013.INVOICENAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.INVOICEDEPTNAME = OIT0013.INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEECODE = OIT0013.PAYEECODE")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEENAME = OIT0013.PAYEENAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.PAYEEDEPTNAME = OIT0013.PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("         AND SEL_TMP.OFFICECODE = OIT0013.OFFICECODE")
        SQLStrBldr.AppendLine("         AND OIT0013.KEIJYOYMD BETWEEN @P02 AND @P03")
        SQLStrBldr.AppendLine("         AND OIT0013.DELFLG <> @P00")
        SQLStrBldr.AppendLine("     GROUP BY")
        SQLStrBldr.AppendLine("         OIT0013.CONSIGNEECODE")
        SQLStrBldr.AppendLine("         , OIT0013.CONSIGNEENAME")
        SQLStrBldr.AppendLine(" ) UNIQ")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     UNIQ.CONSIGNEECODE")
        SQLStrBldr.AppendLine("     , UNIQ.OILCODE")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)          ' 計上年月(月初日)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)          ' 計上年月(月末日)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.Int)           ' 勘定科目コード
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)   ' 削除フラグ

                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                Dim WK_STYMD = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                Dim WK_ENDYMD = New Date(WK_STYMD.Year, WK_STYMD.Month, DateTime.DaysInMonth(WK_STYMD.Year, WK_STYMD.Month))
                PARA02.Value = WK_STYMD
                PARA03.Value = WK_ENDYMD
                PARA04.Value = Int32.Parse(work.WF_SEL_LINE.Text)
                PARA00.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0008tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0008tbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

    End Sub

    '''' <summary>
    '''' 一覧再表示処理
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub DisplayGrid()

    'End Sub

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
        CS0030REPORT.TBLDATA = OIT0008tbl                        ' データ参照  Table
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
        CS0030REPORT.TBLDATA = OIT0008tbl                        ' データ参照Table
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

#Region "未使用"
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
        Dim TBLview As New DataView(OIT0008tbl)
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
#End Region

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

#Region "未使用"
    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub
#End Region

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
                Case "TORIMASTER"
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TORIMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ACCOUNTPATTERN"
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTPATTERN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORI_DEPT"
                    ' 請求先コード/支払先コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TORI_DEPT")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

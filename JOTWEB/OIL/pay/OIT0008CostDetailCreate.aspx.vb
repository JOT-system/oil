Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 費用管理明細入力
''' </summary>
''' <remarks></remarks>
Public Class OIT0008CostDetailCreate
    Inherits Page

    '○ 検索結果格納Table
    Private TMP0009tbl As DataTable                                 ' 一覧格納用テーブル
    Private TMP0009INPtbl As DataTable                              ' 更新時入力テーブル
    'Private OIM0003tbl As DataTable                                 ' 油種リスト格納テーブル
    Private OIM0012tbl As DataTable                                 ' 荷受人リスト格納テーブル
    Private postOfficeTbl As DataTable                              ' 計上営業所格納テーブル

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
    Private WW_CARSAMOUNT_SUM As Double = 0.0
    Private WW_AMOUNT_SUM As Long = 0
    Private WW_LOADAMOUNT_SUM As Long = 0
    Private WW_TAX_SUM As Long = 0
    Private WW_TOTAL_SUM As Long = 0

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        ''油種テーブル取得
        'GetOilTable()

        '荷受人テーブル取得
        GetConsigneeTable()

        '計上営業所テーブル取得
        GetPostOfficeTable()

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データをデータテーブルに変換する
                    SetGridViewToTable()

                    Select Case WF_ButtonClick.Value
                        Case "WF_LeftBoxSelectClick"                'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ListboxDBclick", "WF_ButtonSel"    '（左ボックス）項目選択
                            WF_ButtonSel_Click()
                        Case "WF_Field_DBClick"                     'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonCan"                         '（左ボックス）キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_RadioButonClick"                   ' (右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"                        ' (右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ButtonADDROW"                      '「行追加」ボタン押下
                            WF_Grid_AddRow()
                        Case "WF_ButtonUPDATE"                      '「保存する」ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"                         ' 戻るボタン押下
                            WF_ButtonEND_Click()
                    End Select

                    '○ 一覧再表示処理
                    GridViewReload()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            WF_MAPpermitcode.Value = "FALSE"

        Finally
            '○ 格納Table Close
            If Not IsNothing(TMP0009tbl) Then
                TMP0009tbl.Clear()
                TMP0009tbl.Dispose()
                TMP0009tbl = Nothing
            End If

            '○ 格納Table Close
            If Not IsNothing(TMP0009INPtbl) Then
                TMP0009INPtbl.Clear()
                TMP0009INPtbl.Dispose()
                TMP0009INPtbl = Nothing
            End If

            ''○ 格納Table Close
            'If Not IsNothing(OIM0003tbl) Then
            '    OIM0003tbl.Clear()
            '    OIM0003tbl.Dispose()
            '    OIM0003tbl = Nothing
            'End If

            '○ 格納Table Close
            If Not IsNothing(OIM0012tbl) Then
                OIM0012tbl.Clear()
                OIM0012tbl.Dispose()
                OIM0012tbl = Nothing
            End If

            '○ 格納Table Close
            If Not IsNothing(postOfficeTbl) Then
                postOfficeTbl.Clear()
                postOfficeTbl.Dispose()
                postOfficeTbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0008WRKINC.MAPIDC

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
        '摘要
        TxtTekiyou.Text = work.WF_SEL_TEKIYOU.Text

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(初期化)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '営業所
        Dim officeName As String = ""
        CODENAME_get("OFFICECODE", work.WF_SEL_LAST_OFFICECODE.Text, officeName, WW_RTN_SW)
        WF_OFFICENAME.Text = officeName

        '計上年月
        WF_KEIJYOYM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text

        WF_COSTDETAILTBL.DataSource = TMP0009tbl
        WF_COSTDETAILTBL.DataBind()

    End Sub

    ''' <summary>
    ''' GridViewデータ設定(リロード)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewReload()

        '営業所
        Dim officeName As String = ""
        CODENAME_get("OFFICECODE", work.WF_SEL_LAST_OFFICECODE.Text, officeName, WW_RTN_SW)
        WF_OFFICENAME.Text = officeName

        '計上年月
        WF_KEIJYOYM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text

        WF_COSTDETAILTBL.DataSource = TMP0009tbl
        WF_COSTDETAILTBL.DataBind()

    End Sub

    ''' <summary>
    ''' 行追加
    ''' </summary>
    Protected Sub WF_Grid_AddRow()

        '明細Noの最大値を取得
        Dim maxDetailNo As Integer = 0
        If TMP0009tbl.Select("DETAILNO = MAX(DETAILNO)").Count > 0 Then
            maxDetailNo = TMP0009tbl.Select("DETAILNO = MAX(DETAILNO)")(0).Item("DETAILNO")
        Else
            maxDetailNo = 0
        End If
        '税率を取得
        Dim maxConsumptionTax As Decimal = GetConsumptionTax()

        '空行を追加
        Dim addRow As DataRow = TMP0009tbl.NewRow
        addRow("DETAILNO") = maxDetailNo + 1
        addRow("CONSIGNEECODE") = ""
        addRow("CONSIGNEENAME") = ""
        'addRow("OILCODE") = ""
        'addRow("OILNAME") = ""
        'addRow("ORDERINGTYPE") = ""
        'addRow("ORDERINGOILNAME") = ""
        'addRow("CARSAMOUNT") = 0.0
        'addRow("CARSNUMBER") = 0
        'addRow("LOADAMOUNT") = 0
        'addRow("UNITPRICE") = 0.0
        addRow("AMOUNT") = 0
        addRow("TAX") = 0
        addRow("CONSUMPTIONTAX") = maxConsumptionTax
        addRow("TOTAL") = 0
        addRow("TEKIYOU") = ""

        TMP0009tbl.Rows.Add(addRow)

    End Sub

    ''' <summary>
    ''' フッター行の値算出
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub WF_COSTDETAILTBL_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles WF_COSTDETAILTBL.RowDataBound
        'Select Case e.Row.RowType
        '    Case DataControlRowType.DataRow
        '        Dim row = DirectCast(e.Row.DataItem, DataRowView)

        '        If Not row("OILCODE") = "9999" Then Exit Sub

        '        If Not row("CARSNUMBER") Is DBNull.Value Then
        '            WW_CARSNUMBER_SUM += row("CARSNUMBER")
        '        End If
        '        If Not row("QUANTITY") Is DBNull.Value Then
        '            WW_CARSAMOUNT_SUM += row("CARSAMOUNT")
        '        End If
        '        If Not row("LOADAMOUNT") Is DBNull.Value Then
        '            WW_LOADAMOUNT_SUM += row("LOADAMOUNT")
        '        End If
        '        If Not row("AMOUNT") Is DBNull.Value Then
        '            WW_AMOUNT_SUM += row("AMOUNT")
        '        End If
        '        If Not row("TAX") Is DBNull.Value Then
        '            WW_TAX_SUM += row("TAX")
        '        End If
        '        If Not row("TOTAL") Is DBNull.Value Then
        '            WW_TOTAL_SUM += row("TOTAL")
        '        End If
        'End Select
    End Sub

    ''' <summary>
    ''' 明細リストの体裁を整える
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub WF_COSTDETAILTBL_DataBound(sender As Object, e As EventArgs) Handles WF_COSTDETAILTBL.DataBound

        'GridView本体を取得
        Dim grid As GridView = CType(sender, GridView)

        For Each gvrow As GridViewRow In CType(grid.Controls(0), Table).Rows
            If gvrow.RowType = DataControlRowType.DataRow Then
                '計上営業所ドロップダウンリスト設定
                Dim ddl As DropDownList = Nothing
                Dim postOfficeCode As String = ""

                ddl = gvrow.Cells(2).FindControl("WF_COSTDETAILTBL_POSTOFFICENAMELIST")
                postOfficeCode = DirectCast(gvrow.Cells(2).FindControl("WF_COSTDETAILTBL_POSTOFFICECODE"), HiddenField).Value

                If ddl IsNot Nothing Then
                    Dim rowCnt As Integer = 0
                    For Each row As DataRow In postOfficeTbl.Rows
                        Dim ddlItm As New ListItem(row("OFFICENAME"), row("OFFICECODE"))

                        If String.IsNullOrEmpty(postOfficeCode) Then
                            If rowCnt = 0 Then
                                Dim hidden As HiddenField = Nothing
                                hidden = DirectCast(gvrow.Cells(2).FindControl("WF_COSTDETAILTBL_POSTOFFICECODE"), HiddenField)
                                If hidden IsNot Nothing Then
                                    hidden.Value = row("OFFICECODE")
                                End If
                                hidden = DirectCast(gvrow.Cells(2).FindControl("WF_COSTDETAILTBL_POSTOFFICENAME"), HiddenField)
                                If hidden IsNot Nothing Then
                                    hidden.Value = row("OFFICENAME")
                                End If
                            End If
                        Else
                            If row("OFFICECODE").Equals(postOfficeCode) Then
                                ddlItm.Selected = True
                            Else
                                ddlItm.Selected = False
                            End If
                        End If

                        ddl.Items.Add(ddlItm)
                        rowCnt += 1
                    Next
                    ddl.Attributes.Add("onchange", "selectChangeDdl(this);")
                End If

                '荷受人ドロップダウンリスト設定
                Dim consigneddCode As String = ""

                ddl = gvrow.Cells(3).FindControl("WF_COSTDETAILTBL_CONSIGNEENAMELIST")
                consigneddCode = DirectCast(gvrow.Cells(3).FindControl("WF_COSTDETAILTBL_CONSIGNEECODE"), HiddenField).Value

                If ddl IsNot Nothing Then
                    For Each row As DataRow In OIM0012tbl.Rows
                        Dim ddlItm As New ListItem(row("CONSIGNEENAME"), row("CONSIGNEECODE"))
                        If row("CONSIGNEECODE").Equals(consigneddCode) Then
                            ddlItm.Selected = True
                        Else
                            ddlItm.Selected = False
                        End If
                        ddl.Items.Add(ddlItm)
                    Next
                    ddl.Attributes.Add("onchange", "selectChangeDdl(this);")
                End If

                ''油種ドロップダウンリスト設定
                'ddl = Nothing
                'Dim oilCode As String = ""
                'Dim oilName As String = ""
                'Dim segmentOilCode As String = ""
                'Dim keyCode As String = ""

                'ddl = gvrow.Cells(4).FindControl("WF_COSTDETAILTBL_ORDERINGOILNAMELIST")

                'oilCode = DirectCast(gvrow.Cells(4).FindControl("WF_COSTDETAILTBL_OILCODE"), HiddenField).Value
                'oilName = DirectCast(gvrow.Cells(4).FindControl("WF_COSTDETAILTBL_OILNAME"), HiddenField).Value
                'segmentOilCode = DirectCast(gvrow.Cells(4).FindControl("WF_COSTDETAILTBL_ORDERINGTYPE"), HiddenField).Value

                'If Not String.IsNullOrEmpty(oilCode) Then
                '    keyCode = oilCode + "/" + oilName + "/" + segmentOilCode
                'End If

                'If ddl IsNot Nothing Then
                '    For Each row As DataRow In OIM0003tbl.Rows
                '        Dim ddlItm As New ListItem(row("SEGMENTOILNAME"), row("KEYCODE"))
                '        If row("KEYCODE").Equals(keyCode) Then
                '            ddlItm.Selected = True
                '        Else
                '            ddlItm.Selected = False
                '        End If
                '        ddl.Items.Add(ddlItm)
                '    Next
                '    ddl.Attributes.Add("onchange", "selectChangeDdl(this);")
                'End If

                ''数量入力
                'Dim textBox As TextBox = Nothing
                'textBox = gvrow.Cells(5).FindControl("WF_COSTDETAILTBL_CARSAMOUNT")
                'If textBox IsNot Nothing Then
                '    textBox.Attributes.Add("onblur", "numberOnBlur(this, 3);")
                'End If

                ''車数入力
                'textBox = gvrow.Cells(6).FindControl("WF_COSTDETAILTBL_CARSNUMBER")
                'If textBox IsNot Nothing Then
                '    textBox.Attributes.Add("onblur", "numberOnBlur(this, 0);")
                'End If

                ''屯数入力
                'textBox = gvrow.Cells(7).FindControl("WF_COSTDETAILTBL_LOADAMOUNT")
                'If textBox IsNot Nothing Then
                '    textBox.Attributes.Add("onblur", "numberOnBlur(this, 0);")
                'End If

                ''単価入力
                'textBox = gvrow.Cells(8).FindControl("WF_COSTDETAILTBL_UNITPRICE")
                'If textBox IsNot Nothing Then
                '    textBox.Attributes.Add("onblur", "numberOnBlur(this, 2);")
                'End If

                '金額入力
                Dim textBox As TextBox = gvrow.Cells(4).FindControl("WF_COSTDETAILTBL_AMOUNT")
                If textBox IsNot Nothing Then
                    textBox.Attributes.Add("onblur", "amountOnBlur(this);")
                End If

            End If
        Next

        'Dim lastCONSIGNEENAME As String = ""
        'For Each gvrow As GridViewRow In CType(grid.Controls(0), Table).Rows
        '    If gvrow.RowType = DataControlRowType.Footer Then
        '        'フッター
        '        gvrow.Cells(0).Text = "計"
        '        gvrow.Cells(0).CssClass = "CONSIGNEENAME centerText AllBorder"
        '        gvrow.Cells(0).ColumnSpan = "2"
        '        gvrow.Cells(1).Visible = False
        '        gvrow.Cells(2).Text = String.Format("{0:#,##0.000}", WW_QUANTITY_SUM)
        '        gvrow.Cells(3).Text = String.Format("{0:#,##0}", WW_CARSNUMBER_SUM)
        '        gvrow.Cells(4).Text = String.Format("{0:#,##0}", WW_LOADAMOUNT_SUM)
        '        gvrow.Cells(5).Text = ""
        '        gvrow.Cells(6).Text = String.Format("{0:#,##0}", WW_AMOUNT_SUM)
        '        gvrow.Cells(7).Text = String.Format("{0:#,##0}", WW_TAX_SUM)
        '        gvrow.Cells(8).Text = String.Format("{0:#,##0}", WW_TOTAL_SUM)
        '    ElseIf gvrow.RowType = DataControlRowType.DataRow Then
        '        'データ行
        '        If Not String.IsNullOrEmpty(lastCONSIGNEENAME) Then
        '            If lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text Then
        '                '前回出現した荷受人名と現在行の荷受人名が一致する場合
        '                '荷受人名をクリアする
        '                DirectCast(gvrow.Cells(0).Controls(3), Label).Text = ""
        '            Else
        '                lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text
        '            End If
        '        Else
        '            lastCONSIGNEENAME = DirectCast(gvrow.Cells(0).Controls(3), Label).Text
        '        End If

        '        If DirectCast(gvrow.Cells(1).Controls(1), HiddenField).Value = "9999" Then
        '            gvrow.Cells(0).CssClass = "CONSIGNEENAME centerText AllBorder"
        '            gvrow.Cells(0).ColumnSpan = "2"
        '            gvrow.Cells(1).Visible = False
        '        End If
        '    End If
        'Next

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(TMP0009tbl) Then
            TMP0009tbl = New DataTable
        End If

        If TMP0009tbl.Columns.Count <> 0 Then
            TMP0009tbl.Columns.Clear()
        End If

        TMP0009tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを費用管理明細テーブルから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     DETAILNO")
        SQLStrBldr.AppendLine("     , POSTOFFICECODE")
        SQLStrBldr.AppendLine("     , POSTOFFICENAME")
        SQLStrBldr.AppendLine("     , CONSIGNEECODE")
        SQLStrBldr.AppendLine("     , CONSIGNEENAME")
        'SQLStrBldr.AppendLine("     , OILCODE")
        'SQLStrBldr.AppendLine("     , OILNAME")
        'SQLStrBldr.AppendLine("     , ORDERINGTYPE")
        'SQLStrBldr.AppendLine("     , ORDERINGOILNAME")
        'SQLStrBldr.AppendLine("     , CARSAMOUNT")
        'SQLStrBldr.AppendLine("     , CARSNUMBER")
        'SQLStrBldr.AppendLine("     , LOADAMOUNT")
        'SQLStrBldr.AppendLine("     , CAST(UNITPRICE AS NUMERIC(6, 2)) AS UNITPRICE")
        SQLStrBldr.AppendLine("     , CAST(AMOUNT AS NUMERIC(10, 0)) AS AMOUNT")
        SQLStrBldr.AppendLine("     , CAST(TAX AS NUMERIC(10, 0)) AS TAX")
        SQLStrBldr.AppendLine("     , (")
        SQLStrBldr.AppendLine("         SELECT")
        SQLStrBldr.AppendLine("             CAST(MAX(KEYCODE) AS NUMERIC(5, 2))")
        SQLStrBldr.AppendLine("         FROM")
        SQLStrBldr.AppendLine("             oil.VIW0001_FIXVALUE")
        SQLStrBldr.AppendLine("         WHERE")
        SQLStrBldr.AppendLine("             [CLASS] = 'CONSUMPTIONTAX'")
        SQLStrBldr.AppendLine("         AND    CAMPCODE = 'ZZ'")
        SQLStrBldr.AppendLine("         AND    @P02 BETWEEN STYMD AND ENDYMD")
        SQLStrBldr.AppendLine("     ) AS CONSUMPTIONTAX")
        SQLStrBldr.AppendLine("     , AMOUNT + TAX AS TOTAL")
        SQLStrBldr.AppendLine("     , TEKIYOU")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.TMP0009_COSTDETAIL")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     OFFICECODE = @P01")
        SQLStrBldr.AppendLine(" AND KEIJYOYM = @P02")
        SQLStrBldr.AppendLine(" AND LINE = @P03")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     (")
        SQLStrBldr.AppendLine("         SELECT")
        SQLStrBldr.AppendLine("             ISNULL(MAX(DETAILNO), 0) + 1")
        SQLStrBldr.AppendLine("         FROM")
        SQLStrBldr.AppendLine("             oil.TMP0009_COSTDETAIL")
        SQLStrBldr.AppendLine("         WHERE")
        SQLStrBldr.AppendLine("             OFFICECODE = @P01")
        SQLStrBldr.AppendLine("         AND KEIJYOYM = @P02")
        SQLStrBldr.AppendLine("         AND LINE = @P03")
        SQLStrBldr.AppendLine("     ) AS DETAILNO")
        SQLStrBldr.AppendLine("     , '' AS POSTOFFICECODE")
        SQLStrBldr.AppendLine("     , '' AS POSTOFFICENAME")
        SQLStrBldr.AppendLine("     , '' AS CONSIGNEECODE")
        SQLStrBldr.AppendLine("     , '' AS CONSIGNEENAME")
        'SQLStrBldr.AppendLine("     , '' AS OILCODE")
        'SQLStrBldr.AppendLine("     , '' AS OILNAME")
        'SQLStrBldr.AppendLine("     , '' AS ORDERINGTYPE")
        'SQLStrBldr.AppendLine("     , '' AS ORDERINGOILNAME")
        'SQLStrBldr.AppendLine("     , 0.0 AS CARSAMOUNT")
        'SQLStrBldr.AppendLine("     , 0 AS CARSNUMBER")
        'SQLStrBldr.AppendLine("     , 0 AS LOADAMOUNT")
        'SQLStrBldr.AppendLine("     , 0.0 AS UNITPRICE")
        SQLStrBldr.AppendLine("     , 0 AS AMOUNT")
        SQLStrBldr.AppendLine("     , 0 AS TAX")
        SQLStrBldr.AppendLine("     , (")
        SQLStrBldr.AppendLine("         SELECT")
        SQLStrBldr.AppendLine("             CAST(MAX(KEYCODE) AS NUMERIC(5, 2))")
        SQLStrBldr.AppendLine("         FROM")
        SQLStrBldr.AppendLine("             oil.VIW0001_FIXVALUE")
        SQLStrBldr.AppendLine("         WHERE")
        SQLStrBldr.AppendLine("             [CLASS] = 'CONSUMPTIONTAX'")
        SQLStrBldr.AppendLine("         AND CAMPCODE = 'ZZ'")
        SQLStrBldr.AppendLine("         AND @P02 BETWEEN STYMD AND ENDYMD")
        SQLStrBldr.AppendLine("     ) AS CONSUMPTIONTAX")
        SQLStrBldr.AppendLine("     , 0 AS TOTAL")
        SQLStrBldr.AppendLine("     , '' AS TEKIYOU")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     DETAILNO")

        Try
            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)          ' 計上年月(月初日)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Int)           ' 行番号

                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                PARA02.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                PARA03.Value = Int32.Parse(work.WF_SEL_LINE.Text)

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        TMP0009tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    TMP0009tbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008C Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        Dim WW_RESULT As String = ""

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '〇 データ入力テーブルの準備
        listTableToInpTable()

        '○項目チェック
        INPTableCheck(WW_ERR_SW)

        '○メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '費用管理テーブル更新
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            '費用管理明細及び費用管理テーブルから同一営業所、計上年月、行番号のレコードをいったん削除する
            InitCostDetailTable(SQLcon)

            '削除エラーの場合は処理を中断
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

            '費用管理明細テーブルへデータを追加する
            InsertCostDetailTable(SQLcon)

            '費用管理テーブルへデータを追加する
            InsertCostTable(SQLcon)
        End Using

        '正常終了の場合は前画面へ遷移する
        If isNormal(WW_ERR_SW) Then
            Master.TransitionPrevPage()
        End If
    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview

                Dim prmData As New Hashtable

                Select Case WW_FIELD

                    Case TxtShippersCode.ID '荷主
                        '荷主マスタ検索
                        prmData = work.CreateFIXParam(Master.USER_ORG, "SHIPPERSMASTER")

                    Case TxtAccountCode.ID  '勘定科目
                        '勘定科目パターンマスタ検索
                        prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTPATTERN")

                    Case TxtInvoiceCode.ID, TxtPayeeCode.ID '請求先/支払先
                        '取引マスタ検索
                        prmData = work.CreateFIXParam(Master.USERCAMP, "TORI_DEPT")

                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()

            End With

        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        Dim WK_CODE As String = ""
        Dim WK_NAME As String = ""
        WW_RTN_SW = C_MESSAGE_NO.NORMAL
        Dim WK_RELOAD_FLG As Boolean = False

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtInvoiceCode.ID  '請求先
                WK_CODE = TxtInvoiceCode.Text
                '請求先コードが空の場合、請求先名、請求先部門もクリアする
                If String.IsNullOrEmpty(WK_CODE) Then
                    '請求先名
                    TxtInvoiceName.Text = ""
                    '請求先部門
                    TxtInvoiceDeptName.Text = ""
                Else
                    '取引先部門名称を取得
                    CODENAME_get("TORI_DEPT", WK_CODE, WK_NAME, WW_RTN_SW)
                    '取得できた場合、取引先名、部門名をそれぞれ請求先名、請求先部門に設定
                    If Not String.IsNullOrEmpty(WK_NAME) Then
                        Dim WK_TORI_DEPT_NAMES = WK_NAME.Split(" ")
                        '請求先名
                        If WK_TORI_DEPT_NAMES.Length > 0 Then
                            TxtInvoiceName.Text = WK_TORI_DEPT_NAMES(0)
                        Else
                            TxtInvoiceName.Text = WK_NAME
                        End If
                        '請求先部門
                        If WK_TORI_DEPT_NAMES.Length > 1 Then
                            TxtInvoiceDeptName.Text = WK_TORI_DEPT_NAMES(1)
                        End If
                    End If
                End If
                'フォーカスセット
                TxtInvoiceCode.Focus()

            Case TxtPayeeCode.ID    '支払先
                WK_CODE = TxtPayeeCode.Text
                '支払先コードが空の場合、支払先名、支払先部門もクリアする
                If String.IsNullOrEmpty(WK_CODE) Then
                    '支払先名
                    TxtPayeeName.Text = ""
                    '支払先部門
                    TxtPayeeDeptName.Text = ""
                Else
                    '取引先部門名称を取得
                    CODENAME_get("TORI_DEPT", WK_CODE, WK_NAME, WW_RTN_SW)
                    '取得できた場合、取引先名、部門名をそれぞれ支払先名、支払先部門に設定
                    If Not String.IsNullOrEmpty(WK_NAME) Then
                        Dim WK_TORI_DEPT_NAMES = WK_NAME.Split(" ")
                        '支払先名
                        If WK_TORI_DEPT_NAMES.Length > 0 Then
                            TxtPayeeName.Text = WK_TORI_DEPT_NAMES(0)
                        Else
                            TxtPayeeName.Text = WK_NAME
                        End If
                        '支払先部門
                        If WK_TORI_DEPT_NAMES.Length > 1 Then
                            TxtPayeeDeptName.Text = WK_TORI_DEPT_NAMES(1)
                        End If
                    End If
                End If
                'フォーカスセット
                TxtPayeeCode.Focus()

            Case Else

                Dim rowIdx As Integer = 0
                Dim WK_TextBox As TextBox = Nothing
                Dim WK_Label As Label = Nothing
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WK_RELOAD_FLG As Boolean = False

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value

            Case TxtShippersCode.ID  '荷主コード
                '荷主コード
                TxtShippersCode.Text = WW_SelectValue
                '荷主名
                TxtShippersName.Text = WW_SelectText
                'フォーカスセット
                TxtShippersCode.Focus()

            Case TxtAccountCode.ID  '勘定科目
                Dim patternCodes = WW_SelectValue.Split(" ")
                Dim patternNames = ConvertAccountPatternName(WW_SelectText)

                '勘定科目コード
                TxtAccountCode.Text = patternCodes(0)

                '勘定科目名
                If patternNames.Length > 0 Then
                    TxtAccountName.Text = patternNames(0)
                End If

                'セグメント
                If patternCodes.Length > 1 Then
                    TxtSegmentCode.Text = patternCodes(1)
                End If

                'セグメント名
                If patternNames.Length > 1 Then
                    TxtSegmentName.Text = patternNames(1)
                End If

                'セグメント枝番
                If patternCodes.Length > 2 Then
                    TxtSegmentBranchCode.Text = patternCodes(2)
                End If

                'セグメント枝番名
                If patternNames.Length > 2 Then
                    TxtSegmentBranchName.Text = patternNames(2)
                End If

                'フォーカスセット
                TxtAccountCode.Focus()

            Case TxtInvoiceCode.ID    '請求先コード
                Dim WK_TORI_DEPAT_TEXT = WW_SelectText.Split(" ")

                '請求先コード
                TxtInvoiceCode.Text = WW_SelectValue

                '請求先名
                If WK_TORI_DEPAT_TEXT.Length > 0 Then
                    TxtInvoiceName.Text = WK_TORI_DEPAT_TEXT(0)
                Else
                    TxtInvoiceName.Text = WW_SelectText
                End If

                '請求先部門
                If WK_TORI_DEPAT_TEXT.Length > 1 Then
                    TxtInvoiceDeptName.Text = WK_TORI_DEPAT_TEXT(1)
                End If

                'フォーカスセット
                TxtInvoiceCode.Focus()

            Case TxtPayeeCode.ID    '支払先コード
                Dim WK_TORI_DEPAT_TEXT = WW_SelectText.Split(" ")

                '支払先コード
                TxtPayeeCode.Text = WW_SelectValue

                '支払先名
                If WK_TORI_DEPAT_TEXT.Length > 0 Then
                    TxtPayeeName.Text = WK_TORI_DEPAT_TEXT(0)
                Else
                    TxtPayeeName.Text = WW_SelectText
                End If

                '支払先部門
                If WK_TORI_DEPAT_TEXT.Length > 1 Then
                    TxtPayeeDeptName.Text = WK_TORI_DEPAT_TEXT(1)
                End If

                'フォーカスセット
                TxtPayeeCode.Focus()

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

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case TxtShippersCode.ID '荷主
                TxtShippersCode.Focus()

            Case TxtAccountCode.ID  '勘定科目
                TxtAccountCode.Focus()

            Case TxtInvoiceCode.ID  '請求先
                TxtInvoiceCode.Focus()

            Case TxtInvoiceCode.ID  '支払先
                TxtInvoiceCode.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

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
    ''' 費用管理明細テーブルの初期化・費用管理テーブルから指定の営業所・計上年月・行番号のレコードを削除する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InitCostDetailTable(ByVal SQLcon As SqlConnection)

        Dim DelBldr As New StringBuilder
        DelBldr.AppendLine(" DELETE FROM [oil].TMP0009_COSTDETAIL")
        DelBldr.AppendLine(" WHERE")
        DelBldr.AppendLine("     OFFICECODE = @P01")
        DelBldr.AppendLine(" AND KEIJYOYM = @P02")
        DelBldr.AppendLine(" AND LINE = @P03")

        Dim DelCostBldr As New StringBuilder
        DelCostBldr.AppendLine(" DELETE FROM [oil].TMP0008_COST")
        DelCostBldr.AppendLine(" WHERE")
        DelCostBldr.AppendLine("     OFFICECODE = @P01")
        DelCostBldr.AppendLine(" AND KEIJYOYM = @P02")
        DelCostBldr.AppendLine(" AND LINE = @P03")

        Try
            Using DelCmd As New SqlCommand(DelBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = DelCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
                Dim PARA02 As SqlParameter = DelCmd.Parameters.Add("@P02", SqlDbType.Date)          ' 計上年月(月初日)
                Dim PARA03 As SqlParameter = DelCmd.Parameters.Add("@P03", SqlDbType.Int)           ' 行番号

                '費用管理明細WKの初期化
                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                PARA02.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                PARA03.Value = Int32.Parse(work.WF_SEL_LINE.Text)

                DelCmd.CommandTimeout = 300
                DelCmd.ExecuteNonQuery()

            End Using

            Using DelCmd As New SqlCommand(DelCostBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = DelCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
                Dim PARA02 As SqlParameter = DelCmd.Parameters.Add("@P02", SqlDbType.Date)          ' 計上年月(月初日)
                Dim PARA03 As SqlParameter = DelCmd.Parameters.Add("@P03", SqlDbType.Int)           ' 行番号

                '費用管理WKから指定の営業所、計上年月、行番号のレコードを削除
                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                PARA02.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                PARA03.Value = Int32.Parse(work.WF_SEL_LINE.Text)

                DelCmd.CommandTimeout = 300
                DelCmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008C DELETE OTMP0008_COST AND TMP0009_COSTDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008C DELETE OTMP0008_COST AND TMP0009_COSTDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR

            Exit Sub
        End Try

        WW_ERR_SW = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' 費用管理明細テーブルへのデータ追加
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertCostDetailTable(ByVal SQLcon As SqlConnection)

        '費用管理明細テーブルへのデータ追加
        Dim InsBldr As New StringBuilder
        InsBldr.AppendLine(" INSERT INTO [oil].TMP0009_COSTDETAIL(")
        InsBldr.AppendLine("     OFFICECODE")
        InsBldr.AppendLine("     , KEIJYOYM")
        InsBldr.AppendLine("     , LINE")
        InsBldr.AppendLine("     , DETAILNO")
        InsBldr.AppendLine("     , ACCOUNTCODE")
        InsBldr.AppendLine("     , ACCOUNTNAME")
        InsBldr.AppendLine("     , SEGMENTCODE")
        InsBldr.AppendLine("     , SEGMENTNAME")
        InsBldr.AppendLine("     , BREAKDOWNCODE")
        InsBldr.AppendLine("     , BREAKDOWN")
        InsBldr.AppendLine("     , SHIPPERSCODE")
        InsBldr.AppendLine("     , SHIPPERSNAME")
        InsBldr.AppendLine("     , INVOICECODE")
        InsBldr.AppendLine("     , INVOICENAME")
        InsBldr.AppendLine("     , INVOICEDEPTNAME")
        InsBldr.AppendLine("     , PAYEECODE")
        InsBldr.AppendLine("     , PAYEENAME")
        InsBldr.AppendLine("     , PAYEEDEPTNAME")
        InsBldr.AppendLine("     , CONSIGNEECODE")
        InsBldr.AppendLine("     , CONSIGNEENAME")
        InsBldr.AppendLine("     , OILCODE")
        InsBldr.AppendLine("     , OILNAME")
        InsBldr.AppendLine("     , ORDERINGTYPE")
        InsBldr.AppendLine("     , ORDERINGOILNAME")
        InsBldr.AppendLine("     , CARSAMOUNT")
        InsBldr.AppendLine("     , CARSNUMBER")
        InsBldr.AppendLine("     , LOADAMOUNT")
        InsBldr.AppendLine("     , UNITPRICE")
        InsBldr.AppendLine("     , AMOUNT")
        InsBldr.AppendLine("     , TAX")
        InsBldr.AppendLine("     , CONSUMPTIONTAX")
        InsBldr.AppendLine("     , OFFICENAME")
        InsBldr.AppendLine("     , POSTOFFICECODE")
        InsBldr.AppendLine("     , POSTOFFICENAME")
        InsBldr.AppendLine("     , TEKIYOU")
        InsBldr.AppendLine(" )")
        InsBldr.AppendLine(" VALUES(")
        InsBldr.AppendLine("     @P01")
        InsBldr.AppendLine("     , @P02")
        InsBldr.AppendLine("     , @P03")
        InsBldr.AppendLine("     , @P04")
        InsBldr.AppendLine("     , @P05")
        InsBldr.AppendLine("     , @P06")
        InsBldr.AppendLine("     , @P07")
        InsBldr.AppendLine("     , @P08")
        InsBldr.AppendLine("     , @P09")
        InsBldr.AppendLine("     , @P10")
        InsBldr.AppendLine("     , @P11")
        InsBldr.AppendLine("     , @P12")
        InsBldr.AppendLine("     , @P13")
        InsBldr.AppendLine("     , @P14")
        InsBldr.AppendLine("     , @P15")
        InsBldr.AppendLine("     , @P16")
        InsBldr.AppendLine("     , @P17")
        InsBldr.AppendLine("     , @P18")
        InsBldr.AppendLine("     , @P19")
        InsBldr.AppendLine("     , @P20")
        InsBldr.AppendLine("     , @P21")
        InsBldr.AppendLine("     , @P22")
        InsBldr.AppendLine("     , @P23")
        InsBldr.AppendLine("     , @P24")
        InsBldr.AppendLine("     , @P25")
        InsBldr.AppendLine("     , @P26")
        InsBldr.AppendLine("     , @P27")
        InsBldr.AppendLine("     , @P28")
        InsBldr.AppendLine("     , @P29")
        InsBldr.AppendLine("     , @P30")
        InsBldr.AppendLine("     , @P31")
        InsBldr.AppendLine("     , @P32")
        InsBldr.AppendLine("     , @P33")
        InsBldr.AppendLine("     , @P34")
        InsBldr.AppendLine("     , @P35")
        InsBldr.AppendLine(" )")

        Try
            Using InsCmd As New SqlCommand(InsBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = InsCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   '営業所コード
                Dim PARA02 As SqlParameter = InsCmd.Parameters.Add("@P02", SqlDbType.Date)          '計上年月
                Dim PARA03 As SqlParameter = InsCmd.Parameters.Add("@P03", SqlDbType.Int)           '行番号

                Dim PARA04 As SqlParameter = InsCmd.Parameters.Add("@P04", SqlDbType.Int)           '明細番号
                Dim PARA05 As SqlParameter = InsCmd.Parameters.Add("@P05", SqlDbType.NVarChar, 8)   '勘定科目コード
                Dim PARA06 As SqlParameter = InsCmd.Parameters.Add("@P06", SqlDbType.NVarChar, 40)  '勘定科目名
                Dim PARA07 As SqlParameter = InsCmd.Parameters.Add("@P07", SqlDbType.NVarChar, 5)   'セグメント
                Dim PARA08 As SqlParameter = InsCmd.Parameters.Add("@P08", SqlDbType.NVarChar, 40)  'セグメント名
                Dim PARA09 As SqlParameter = InsCmd.Parameters.Add("@P09", SqlDbType.NVarChar, 2)   'セグメント枝番
                Dim PARA10 As SqlParameter = InsCmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40)  'セグメント枝番名
                Dim PARA11 As SqlParameter = InsCmd.Parameters.Add("@P11", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA12 As SqlParameter = InsCmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40)  '荷主名
                Dim PARA13 As SqlParameter = InsCmd.Parameters.Add("@P13", SqlDbType.NVarChar, 10)  '請求先コード
                Dim PARA14 As SqlParameter = InsCmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40)  '請求先名
                Dim PARA15 As SqlParameter = InsCmd.Parameters.Add("@P15", SqlDbType.NVarChar, 40)  '請求先部門名
                Dim PARA16 As SqlParameter = InsCmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)  '支払先コード
                Dim PARA17 As SqlParameter = InsCmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '支払先名
                Dim PARA18 As SqlParameter = InsCmd.Parameters.Add("@P18", SqlDbType.NVarChar, 40)  '支払先部門名
                Dim PARA19 As SqlParameter = InsCmd.Parameters.Add("@P19", SqlDbType.NVarChar, 10)  '荷受人コード
                Dim PARA20 As SqlParameter = InsCmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40)  '荷受人名
                Dim PARA21 As SqlParameter = InsCmd.Parameters.Add("@P21", SqlDbType.NVarChar, 4)   '油種コード
                Dim PARA22 As SqlParameter = InsCmd.Parameters.Add("@P22", SqlDbType.NVarChar, 40)  '油種名
                Dim PARA23 As SqlParameter = InsCmd.Parameters.Add("@P23", SqlDbType.NVarChar, 2)   '油種区分(受発注用)
                Dim PARA24 As SqlParameter = InsCmd.Parameters.Add("@P24", SqlDbType.NVarChar, 40)  '油種名(受発注用)
                Dim PARA25 As SqlParameter = InsCmd.Parameters.Add("@P25", SqlDbType.Decimal)       '数量
                Dim PARA26 As SqlParameter = InsCmd.Parameters.Add("@P26", SqlDbType.Int)           '車数
                Dim PARA27 As SqlParameter = InsCmd.Parameters.Add("@P27", SqlDbType.Decimal)       '屯数
                Dim PARA28 As SqlParameter = InsCmd.Parameters.Add("@P28", SqlDbType.Decimal)       '単価
                Dim PARA29 As SqlParameter = InsCmd.Parameters.Add("@P29", SqlDbType.Money)         '金額
                Dim PARA30 As SqlParameter = InsCmd.Parameters.Add("@P30", SqlDbType.Money)         '税額
                Dim PARA31 As SqlParameter = InsCmd.Parameters.Add("@P31", SqlDbType.Decimal)       '税率
                Dim PARA32 As SqlParameter = InsCmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)  '営業所名
                Dim PARA33 As SqlParameter = InsCmd.Parameters.Add("@P33", SqlDbType.NVarChar, 6)   '計上営業所コード
                Dim PARA34 As SqlParameter = InsCmd.Parameters.Add("@P34", SqlDbType.NVarChar, 20)  '計上営業所名
                Dim PARA35 As SqlParameter = InsCmd.Parameters.Add("@P35", SqlDbType.NVarChar, 200) '摘要

                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                PARA02.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                PARA03.Value = Int32.Parse(work.WF_SEL_LINE.Text)

                PARA05.Value = TxtAccountCode.Text
                PARA06.Value = TxtAccountName.Text
                PARA07.Value = TxtSegmentCode.Text
                PARA08.Value = TxtSegmentName.Text
                PARA09.Value = TxtSegmentBranchCode.Text
                PARA10.Value = TxtSegmentBranchName.Text
                PARA11.Value = TxtShippersCode.Text
                PARA12.Value = TxtShippersName.Text
                PARA13.Value = TxtInvoiceCode.Text
                PARA14.Value = TxtInvoiceName.Text
                PARA15.Value = TxtInvoiceDeptName.Text
                PARA16.Value = TxtPayeeCode.Text
                PARA17.Value = TxtPayeeName.Text
                PARA18.Value = TxtPayeeDeptName.Text

                Dim WK_OFFICENAME As String = ""
                CODENAME_get("OFFICECODE", work.WF_SEL_LAST_OFFICECODE.Text, WK_OFFICENAME, WW_RTN_SW)
                PARA32.Value = WK_OFFICENAME

                For Each TMP0009INProw As DataRow In TMP0009INPtbl.Rows
                    PARA04.Value = TMP0009INProw("DETAILNO")
                    PARA19.Value = TMP0009INProw("CONSIGNEECODE")
                    PARA20.Value = TMP0009INProw("CONSIGNEENAME")
                    PARA21.Value = DBNull.Value
                    PARA22.Value = DBNull.Value
                    PARA23.Value = DBNull.Value
                    PARA24.Value = DBNull.Value
                    PARA25.Value = 0.0
                    PARA26.Value = 0
                    PARA27.Value = 0.0
                    PARA28.Value = 0.0
                    PARA29.Value = TMP0009INProw("AMOUNT")
                    PARA30.Value = TMP0009INProw("TAX")
                    PARA31.Value = TMP0009INProw("CONSUMPTIONTAX")
                    PARA33.Value = TMP0009INProw("POSTOFFICECODE")
                    PARA34.Value = TMP0009INProw("POSTOFFICENAME")
                    PARA35.Value = TMP0009INProw("TEKIYOU")

                    InsCmd.CommandTimeout = 300
                    InsCmd.ExecuteNonQuery()
                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008C TMP0009_COSTDETAIL INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008C TMP0009_COSTDETAIL INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR

            Exit Sub
        End Try

        WW_ERR_SW = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' 費用管理テーブルへのデータ追加
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertCostTable(ByVal SQLcon As SqlConnection)

        '費用管理テーブルへのデータ追加
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" INSERT INTO [oil].TMP0008_COST( ")
        SQLStrBldr.AppendLine("     OFFICECODE ")
        SQLStrBldr.AppendLine("     , KEIJYOYM ")
        SQLStrBldr.AppendLine("     , LINE ")
        SQLStrBldr.AppendLine("     , CHECKFLG ")
        SQLStrBldr.AppendLine("     , CALCACCOUNT ")
        SQLStrBldr.AppendLine("     , ACCOUNTCODE ")
        SQLStrBldr.AppendLine("     , ACCOUNTNAME ")
        SQLStrBldr.AppendLine("     , SEGMENTCODE ")
        SQLStrBldr.AppendLine("     , SEGMENTNAME ")
        SQLStrBldr.AppendLine("     , SEGMENTBRANCHCODE ")
        SQLStrBldr.AppendLine("     , SEGMENTBRANCHNAME ")
        SQLStrBldr.AppendLine("     , SHIPPERSCODE ")
        SQLStrBldr.AppendLine("     , SHIPPERSNAME ")
        SQLStrBldr.AppendLine("     , QUANTITY ")
        SQLStrBldr.AppendLine("     , UNITPRICE ")
        SQLStrBldr.AppendLine("     , AMOUNT ")
        SQLStrBldr.AppendLine("     , TAX ")
        SQLStrBldr.AppendLine("     , INVOICECODE ")
        SQLStrBldr.AppendLine("     , INVOICENAME ")
        SQLStrBldr.AppendLine("     , INVOICEDEPTNAME ")
        SQLStrBldr.AppendLine("     , PAYEECODE ")
        SQLStrBldr.AppendLine("     , PAYEENAME ")
        SQLStrBldr.AppendLine("     , PAYEEDEPTNAME ")
        SQLStrBldr.AppendLine("     , TEKIYOU ")
        SQLStrBldr.AppendLine(" ) ")
        SQLStrBldr.AppendLine(" SELECT ")
        SQLStrBldr.AppendLine("     @P01 AS OFFICECODE ")
        SQLStrBldr.AppendLine("     , @P02 AS KEIJYOYM ")
        SQLStrBldr.AppendLine("     , @P03 AS LINE ")
        SQLStrBldr.AppendLine("     , 0 AS CHECKFLG ")
        SQLStrBldr.AppendLine("     , '2' AS CALCACCOUNT ")
        SQLStrBldr.AppendLine("     , @P04 AS ACCOUNTCODE ")
        SQLStrBldr.AppendLine("     , @P05 AS ACCOUNTNAME ")
        SQLStrBldr.AppendLine("     , @P06 AS SEGMENTCODE ")
        SQLStrBldr.AppendLine("     , @P07 AS SEGMENTNAME ")
        SQLStrBldr.AppendLine("     , @P08 AS SEGMENTBRANCHCODE ")
        SQLStrBldr.AppendLine("     , @P09 AS SEGMENTBRANCHNAME ")
        SQLStrBldr.AppendLine("     , @P10 AS SHIPPERSCODE ")
        SQLStrBldr.AppendLine("     , @P11 AS SHIPPERSNAME ")
        SQLStrBldr.AppendLine("     , SUM(CARSAMOUNT) AS QUANTITY ")
        SQLStrBldr.AppendLine("     , MAX(UNITPRICE) AS UNITPRICE ")
        SQLStrBldr.AppendLine("     , SUM(AMOUNT) AS AMOUNT ")
        SQLStrBldr.AppendLine("     , SUM(TAX) AS TAX ")
        SQLStrBldr.AppendLine("     , @P12 AS INVOICECODE ")
        SQLStrBldr.AppendLine("     , @P13 AS INVOICENAME ")
        SQLStrBldr.AppendLine("     , @P14 AS INVOICEDEPTNAME ")
        SQLStrBldr.AppendLine("     , @P15 AS PAYEECODE ")
        SQLStrBldr.AppendLine("     , @P16 AS PAYEENAME ")
        SQLStrBldr.AppendLine("     , @P17 AS PAYEEDEPTNAME ")
        SQLStrBldr.AppendLine("     , @P18 AS TEKIYO ")
        SQLStrBldr.AppendLine(" FROM ")
        SQLStrBldr.AppendLine("     oil.TMP0009_COSTDETAIL ")
        SQLStrBldr.AppendLine(" WHERE ")
        SQLStrBldr.AppendLine("     OFFICECODE = @P01 ")
        SQLStrBldr.AppendLine(" AND KEIJYOYM = @P02 ")
        SQLStrBldr.AppendLine(" AND LINE = @P03 ")
        SQLStrBldr.AppendLine(" GROUP BY ")
        SQLStrBldr.AppendLine("     OFFICECODE ")
        SQLStrBldr.AppendLine("     , KEIJYOYM ")
        SQLStrBldr.AppendLine("     , LINE ")

        Try
            Using InsCmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                Dim PARA01 As SqlParameter = InsCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   '営業所コード
                Dim PARA02 As SqlParameter = InsCmd.Parameters.Add("@P02", SqlDbType.Date)          '計上年月
                Dim PARA03 As SqlParameter = InsCmd.Parameters.Add("@P03", SqlDbType.Int)           '行番号

                Dim PARA04 As SqlParameter = InsCmd.Parameters.Add("@P04", SqlDbType.NVarChar, 8)   '勘定科目コード
                Dim PARA05 As SqlParameter = InsCmd.Parameters.Add("@P05", SqlDbType.NVarChar, 40)  '勘定科目名
                Dim PARA06 As SqlParameter = InsCmd.Parameters.Add("@P06", SqlDbType.NVarChar, 5)   'セグメント
                Dim PARA07 As SqlParameter = InsCmd.Parameters.Add("@P07", SqlDbType.NVarChar, 40)  'セグメント名
                Dim PARA08 As SqlParameter = InsCmd.Parameters.Add("@P08", SqlDbType.NVarChar, 2)   'セグメント枝番
                Dim PARA09 As SqlParameter = InsCmd.Parameters.Add("@P09", SqlDbType.NVarChar, 40)  'セグメント枝番名
                Dim PARA10 As SqlParameter = InsCmd.Parameters.Add("@P10", SqlDbType.NVarChar, 10)  '荷主コード
                Dim PARA11 As SqlParameter = InsCmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40)  '荷主名
                Dim PARA12 As SqlParameter = InsCmd.Parameters.Add("@P12", SqlDbType.NVarChar, 10)  '請求先コード
                Dim PARA13 As SqlParameter = InsCmd.Parameters.Add("@P13", SqlDbType.NVarChar, 40)  '請求先名
                Dim PARA14 As SqlParameter = InsCmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40)  '請求先部門名
                Dim PARA15 As SqlParameter = InsCmd.Parameters.Add("@P15", SqlDbType.NVarChar, 10)  '支払先コード
                Dim PARA16 As SqlParameter = InsCmd.Parameters.Add("@P16", SqlDbType.NVarChar, 40)  '支払先名
                Dim PARA17 As SqlParameter = InsCmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)  '支払先部門名
                Dim PARA18 As SqlParameter = InsCmd.Parameters.Add("@P18", SqlDbType.NVarChar, 200) '摘要

                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text
                PARA02.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")
                PARA03.Value = Int32.Parse(work.WF_SEL_LINE.Text)

                PARA04.Value = TxtAccountCode.Text
                PARA05.Value = TxtAccountName.Text
                PARA06.Value = TxtSegmentCode.Text
                PARA07.Value = TxtSegmentName.Text
                PARA08.Value = TxtSegmentBranchCode.Text
                PARA09.Value = TxtSegmentBranchName.Text
                PARA10.Value = TxtShippersCode.Text
                PARA11.Value = TxtShippersName.Text
                PARA12.Value = TxtInvoiceCode.Text
                PARA13.Value = TxtInvoiceName.Text
                PARA14.Value = TxtInvoiceDeptName.Text
                PARA15.Value = TxtPayeeCode.Text
                PARA16.Value = TxtPayeeName.Text
                PARA17.Value = TxtPayeeDeptName.Text
                PARA18.Value = TxtTekiyou.Text

                InsCmd.CommandTimeout = 300
                InsCmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008C TMP0008_COST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008C TMP0008_COST INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR

            Exit Sub
        End Try

        WW_ERR_SW = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' 一覧テーブルから入力テーブルへの移送
    ''' </summary>
    Protected Sub listTableToInpTable()
        'クローンを作成
        TMP0009INPtbl = TMP0009tbl.Clone
        '行を全て削除
        TMP0009INPtbl.Rows.Clear()

        '行コピー
        For Each TMP0009row In TMP0009tbl.Rows
            '明細入力欄が全て未設定の場合はスキップ
            If String.IsNullOrEmpty(TMP0009row("CONSIGNEECODE")) AndAlso
                String.IsNullOrEmpty(TMP0009row("CONSIGNEENAME")) AndAlso
                TMP0009row("AMOUNT") = 0 AndAlso
                TMP0009row("TAX") = 0 AndAlso
                String.IsNullOrEmpty(TMP0009row("TEKIYOU")) Then
                'String.IsNullOrEmpty(TMP0009row("OILCODE")) AndAlso
                'String.IsNullOrEmpty(TMP0009row("OILNAME")) AndAlso
                'String.IsNullOrEmpty(TMP0009row("ORDERINGTYPE")) AndAlso
                'String.IsNullOrEmpty(TMP0009row("ORDERINGOILNAME")) AndAlso
                'TMP0009row("CARSAMOUNT") = 0 AndAlso
                'TMP0009row("CARSNUMBER") = 0 AndAlso
                'TMP0009row("LOADAMOUNT") = 0 AndAlso
                'TMP0009row("UNITPRICE") = 0 AndAlso
                Continue For
            End If

            Dim TMP0009INProw As DataRow = TMP0009INPtbl.NewRow
            '項目コピー
            TMP0009INProw.ItemArray = TMP0009row.ItemArray
            '行追加
            TMP0009INPtbl.Rows.Add(TMP0009INProw)
        Next

    End Sub

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

        '勘定科目コード
        WW_TEXT = TxtAccountCode.Text
        Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(勘定科目コード)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        'セグメント
        WW_TEXT = TxtSegmentCode.Text
        Master.CheckField(Master.USERCAMP, "SEGMENTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(セグメント)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        'セグメント枝番
        WW_TEXT = TxtSegmentBranchCode.Text
        Master.CheckField(Master.USERCAMP, "SEGMENTBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(セグメント枝番)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '存在チェック(勘定科目マスタ)
        If String.IsNullOrEmpty(WW_LINE_ERR) AndAlso
                Not String.IsNullOrEmpty(TxtAccountCode.Text) AndAlso
                Not String.IsNullOrEmpty(TxtSegmentCode.Text) AndAlso
                Not String.IsNullOrEmpty(TxtSegmentBranchCode.Text) Then

            Dim WW_CODE As String = TxtAccountCode.Text & " " & TxtSegmentCode.Text & " " & TxtSegmentBranchCode.Text
            CODENAME_get("ACCOUNTPATTERN", WW_CODE, WW_DUMMY, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(勘定科目コード/セグメント/セグメント枝番エラー)です。"
                WW_CheckMES2 = "勘定科目マスタに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        '荷主コード
        WW_TEXT = TxtShippersCode.Text
        Master.CheckField(Master.USERCAMP, "SHIPPERSCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '値存在チェック
            CODENAME_get("SHIPPERSCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(荷主コード)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '請求先コード
        WW_TEXT = TxtInvoiceCode.Text
        Master.CheckField(Master.USERCAMP, "INVOICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                '値存在チェック
                CODENAME_get("TORI_DEPT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(請求先コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(請求先コード)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '請求先名
        WW_TEXT = TxtInvoiceName.Text
        Master.CheckField(Master.USERCAMP, "INVOICENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(請求先名)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '請求先部門名
        WW_TEXT = TxtInvoiceDeptName.Text
        Master.CheckField(Master.USERCAMP, "INVOICEDEPTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(請求先部門名)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '支払先コード
        WW_TEXT = TxtPayeeCode.Text
        Master.CheckField(Master.USERCAMP, "PAYEECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                '値存在チェック
                CODENAME_get("TORI_DEPT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(支払先コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・更新できないレコード(支払先コード)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '支払先名
        WW_TEXT = TxtPayeeName.Text
        Master.CheckField(Master.USERCAMP, "PAYEENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(支払先名)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '支払先部門名
        WW_TEXT = TxtPayeeDeptName.Text
        Master.CheckField(Master.USERCAMP, "PAYEEDEPTNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(支払先部門名)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '摘要
        WW_TEXT = TxtTekiyou.Text
        Master.CheckField(Master.USERCAMP, "TEKIYOU", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            WW_CheckMES1 = "・更新できないレコード(摘要)です。"
            WW_CheckMES2 = WW_CS0024FCHECKREPORT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '費用管理明細テーブル
        Dim rowCnt As Integer = 0
        For Each TMP0009INProw As DataRow In TMP0009INPtbl.Rows

            WW_LINE_ERR = ""

            '計上営業所コード（バリデーションチェック）
            WW_TEXT = TMP0009INProw("POSTOFFICECODE")
            Master.CheckField(Master.USERCAMP, "POSTOFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("OFFICECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(計上営業所コード)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(計上営業所コード)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ''数量（バリデーションチェック）
            'WW_TEXT = TMP0009INProw("CARSAMOUNT")
            'Master.CheckField(Master.USERCAMP, "CARSAMOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(数量エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''車数（バリデーションチェック）
            'WW_TEXT = TMP0009INProw("CARSNUMBER")
            'Master.CheckField(Master.USERCAMP, "CARSNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(車数エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''屯数（バリデーションチェック）
            'WW_TEXT = TMP0009INProw("LOADAMOUNT")
            'Master.CheckField(Master.USERCAMP, "LOADAMOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(屯数エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ''単価（バリデーションチェック）
            'WW_TEXT = TMP0009INProw("UNITPRICE")
            'Master.CheckField(Master.USERCAMP, "UNITPRICE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            'If Not isNormal(WW_CS0024FCHECKERR) Then
            '    WW_CheckMES1 = "・更新できないレコード(単価エラー)です。"
            '    WW_CheckMES2 = WW_CS0024FCHECKREPORT
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
            '    WW_LINE_ERR = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            '金額（バリデーションチェック）
            WW_TEXT = TMP0009INProw("AMOUNT")
            Master.CheckField(Master.USERCAMP, "AMOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(金額エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '税額（バリデーションチェック）
            WW_TEXT = TMP0009INProw("TAX")
            Master.CheckField(Master.USERCAMP, "TAX", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(税額エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '摘要
            WW_TEXT = TMP0009INProw("TEKIYOU")
            Master.CheckField(Master.USERCAMP, "TEKIYOU", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(摘要)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '重複チェック
            rowCnt += 1
            For i As Integer = rowCnt To TMP0009tbl.Rows.Count - 1
                Dim row As DataRow = TMP0009tbl.Rows(i)

                '計上営業所と荷受人と油種が一致するレコードが存在する場合、重複エラーとする
                If row("POSTOFFICENAME") = TMP0009INProw("POSTOFFICENAME") AndAlso
                    row("CONSIGNEENAME") = TMP0009INProw("CONSIGNEENAME") AndAlso
                    row("TEKIYOU") = TMP0009INProw("TEKIYOU") Then
                    'row("ORDERINGOILNAME") = TMP0009INProw("ORDERINGOILNAME") AndAlso

                    WW_CheckMES1 = "・更新できないレコード(計上営業所・荷受人・摘要重複エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, TMP0009INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="TMP0009row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal TMP0009row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        WW_ERR_MES &= ControlChars.NewLine & "  --> # =" & TxtLine.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 勘定科目コード =" & TxtAccountCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 勘定科目名 =" & TxtAccountName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント =" & TxtSegmentCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント名 =" & TxtSegmentName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番 =" & TxtSegmentBranchCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番名 =" & TxtSegmentBranchName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード =" & TxtShippersCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主名 =" & TxtShippersName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先コード =" & TxtInvoiceCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先名 =" & TxtInvoiceName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先部門 =" & TxtInvoiceDeptName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先コード =" & TxtPayeeCode.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先名 =" & TxtPayeeName.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先部門 =" & TxtPayeeDeptName.Text


        If Not IsNothing(TMP0009row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 明細No =" & TMP0009row("DETAILNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 計上営業所 =" & TMP0009row("POSTOFFICENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷受人 =" & TMP0009row("CONSIGNEENAME") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 油種 =" & TMP0009row("ORDERINGOILNAME") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 数量 =" & TMP0009row("CARSAMOUNT") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 車数 =" & TMP0009row("CARSNUMBER") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 屯数 =" & TMP0009row("LOADAMOUNT") & " , "
            'WW_ERR_MES &= ControlChars.NewLine & "  --> 単価 =" & TMP0009row("UNITPRICE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 金額 =" & TMP0009row("AMOUNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 税額 =" & TMP0009row("TAX") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 総額 =" & TMP0009row("TOTAL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 摘要 =" & TMP0009row("TEKIYOU")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

#Region "未使用(コメントアウト)"
    '''' <summary>
    '''' 油種リストデータ設定
    '''' </summary>
    '''' <remarks></remarks>
    'Protected Sub GetOilTable()

    '    '油種テーブル初期化
    '    OIM0003tbl = New DataTable
    '    OIM0003tbl.Columns.Clear()
    '    OIM0003tbl.Clear()

    '    '〇検索SQL説明
    '    '　検索説明
    '    '     条件指定に従い該当データを油種マスタから取得する
    '    Dim SQLStrBldr As New StringBuilder
    '    SQLStrBldr.AppendLine(" SELECT")
    '    SQLStrBldr.AppendLine("     OILCODE + '/' + OILNAME + '/' + SEGMENTOILCODE AS KEYCODE")
    '    SQLStrBldr.AppendLine("     , SEGMENTOILNAME")
    '    SQLStrBldr.AppendLine(" FROM")
    '    SQLStrBldr.AppendLine("     oil.OIM0003_PRODUCT")
    '    SQLStrBldr.AppendLine(" WHERE")
    '    SQLStrBldr.AppendLine("     DELFLG = '0'")
    '    SQLStrBldr.AppendLine(" AND OFFICECODE = @P01")
    '    SQLStrBldr.AppendLine(" GROUP BY")
    '    SQLStrBldr.AppendLine("     OILCODE")
    '    SQLStrBldr.AppendLine("     , OILNAME")
    '    SQLStrBldr.AppendLine("     , SEGMENTOILCODE")
    '    SQLStrBldr.AppendLine("     , SEGMENTOILNAME")
    '    SQLStrBldr.AppendLine(" ")
    '    SQLStrBldr.AppendLine(" UNION ALL")
    '    SQLStrBldr.AppendLine(" ")
    '    SQLStrBldr.AppendLine(" SELECT")
    '    SQLStrBldr.AppendLine("     '' AS KEYCODE")
    '    SQLStrBldr.AppendLine("     , '' AS SEGMENTOILNAME")
    '    SQLStrBldr.AppendLine(" ")
    '    SQLStrBldr.AppendLine(" ORDER BY")
    '    SQLStrBldr.AppendLine("     KEYCODE")

    '    '○ 油種テーブルデータ取得
    '    Try
    '        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
    '            SQLcon.Open()       ' DataBase接続

    '            Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
    '                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
    '                PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text

    '                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
    '                    '○ フィールド名とフィールドの型を取得
    '                    For index As Integer = 0 To SQLdr.FieldCount - 1
    '                        OIM0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
    '                    Next

    '                    '○ テーブル検索結果をテーブル格納
    '                    OIM0003tbl.Load(SQLdr)
    '                End Using

    '            End Using
    '        End Using
    '    Catch ex As Exception
    '        Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008C SELECT OIM0003_PRODUCT")

    '        CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
    '        CS0011LOGWrite.INFPOSI = "DB:OIT0008C SELECT OIM0003_PRODUCT"
    '        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
    '        CS0011LOGWrite.TEXT = ex.ToString()
    '        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
    '        CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
    '    End Try

    'End Sub
#End Region

    ''' <summary>
    ''' 荷受人リストデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetConsigneeTable()

        '荷受人テーブル初期化
        OIM0012tbl = New DataTable
        OIM0012tbl.Columns.Clear()
        OIM0012tbl.Clear()

        '〇検索SQL説明
        '　検索説明
        '     条件指定に従い該当データを荷受人マスタから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     CONSIGNEECODE")
        SQLStrBldr.AppendLine("     , CONSIGNEENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0012_NIUKE")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     DELFLG = '0'")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     '' AS CONSIGNEECODE")
        SQLStrBldr.AppendLine("     , '' AS CONSIGNEENAME")
        SQLStrBldr.AppendLine(" ")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     CONSIGNEECODE")

        '○ 荷受人テーブルデータ取得
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0012tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        OIM0012tbl.Load(SQLdr)
                    End Using

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008C SELECT OIM0012_NIUKE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008C SELECT OIM0003_NIUKE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 計上営業所リストデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetPostOfficeTable()

        '計上営業所テーブル初期化
        postOfficeTbl = New DataTable
        postOfficeTbl.Columns.Clear()
        postOfficeTbl.Clear()

        '〇検索SQL説明
        '　検索説明
        '     条件指定に従い該当データを営業所関連付けマスタから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     OFFICECODE")
        SQLStrBldr.AppendLine("     , OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.VIW0003_OFFICECHANGE")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE = @P01")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     OFFICECODE")

        '○ 計上営業所テーブルデータ取得
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   ' 営業所コード
                    PARA01.Value = work.WF_SEL_LAST_OFFICECODE.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            postOfficeTbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        postOfficeTbl.Load(SQLdr)
                    End Using

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008C SELECT VIW0003_OFFICECHANGE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008C SELECT VIW0003_OFFICECHANGE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 税率取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GetConsumptionTax() As Decimal

        Dim consumptionTax As Decimal = 0.0

        'テーブル初期化
        Dim dt As DataTable = New DataTable
        dt.Columns.Clear()
        dt.Clear()

        '〇検索SQL説明
        '　検索説明
        '     条件指定に従い該当データを油種マスタから取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     CAST(MAX(KEYCODE) AS NUMERIC(5, 2)) AS CONSUMPTIONTAX")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.VIW0001_FIXVALUE")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     [CLASS] = 'CONSUMPTIONTAX'")
        SQLStrBldr.AppendLine(" AND CAMPCODE = 'ZZ'")
        SQLStrBldr.AppendLine(" AND @P01 BETWEEN STYMD AND ENDYMD")

        '○ 油種テーブルデータ取得
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       ' DataBase接続

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.Date)          ' 計上年月(月初日)
                    PARA01.Value = Date.Parse(work.WF_SEL_LAST_KEIJYO_YM.Text + "/01")

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008C SELECT VIW0001_FIXVALUE(CONSUMPTIONTAX)")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008C  SELECT VIW0001_FIXVALUE(CONSUMPTIONTAX)"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        End Try

        If dt.Rows.Count > 0 Then
            consumptionTax = dt.Rows(0)("CONSUMPTIONTAX")
        End If

        Return consumptionTax
    End Function

    ''' <summary>
    ''' GridViewからDataTableへの変換
    ''' </summary>
    Protected Sub SetGridViewToTable()

        '入力テーブル作成
        TMP0009tbl = New DataTable
        TMP0009tbl.Columns.Add("DETAILNO", Type.GetType("System.Int32"))
        TMP0009tbl.Columns.Add("POSTOFFICECODE", Type.GetType("System.String"))
        TMP0009tbl.Columns.Add("POSTOFFICENAME", Type.GetType("System.String"))
        TMP0009tbl.Columns.Add("CONSIGNEECODE", Type.GetType("System.String"))
        TMP0009tbl.Columns.Add("CONSIGNEENAME", Type.GetType("System.String"))
        'TMP0009tbl.Columns.Add("OILCODE", Type.GetType("System.String"))
        'TMP0009tbl.Columns.Add("OILNAME", Type.GetType("System.String"))
        'TMP0009tbl.Columns.Add("ORDERINGTYPE", Type.GetType("System.String"))
        'TMP0009tbl.Columns.Add("ORDERINGOILNAME", Type.GetType("System.String"))
        'TMP0009tbl.Columns.Add("CARSAMOUNT", Type.GetType("System.Decimal"))
        'TMP0009tbl.Columns.Add("CARSNUMBER", Type.GetType("System.Decimal"))
        'TMP0009tbl.Columns.Add("LOADAMOUNT", Type.GetType("System.Decimal"))
        'TMP0009tbl.Columns.Add("UNITPRICE", Type.GetType("System.Decimal"))
        TMP0009tbl.Columns.Add("AMOUNT", Type.GetType("System.Decimal"))
        TMP0009tbl.Columns.Add("TAX", Type.GetType("System.Decimal"))
        TMP0009tbl.Columns.Add("CONSUMPTIONTAX", Type.GetType("System.Decimal"))
        TMP0009tbl.Columns.Add("TOTAL", Type.GetType("System.Decimal"))
        TMP0009tbl.Columns.Add("TEKIYOU", Type.GetType("System.String"))

        'GridViewの行を検索
        For Each gRow As GridViewRow In WF_COSTDETAILTBL.Rows

            Dim addRow = TMP0009tbl.NewRow

            'データ行でなければ処理を行わない
            If Not gRow.RowType = DataControlRowType.DataRow Then
                Continue For
            End If

            '削除フラグONならば処理を行わない
            If DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CHECKFLG"), CheckBox).Checked = True Then
                Continue For
            End If

            '計上営業所コード(POSTOFFICECODE)
            addRow("POSTOFFICECODE") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_POSTOFFICECODE"), HiddenField).Value

            '計上営業所名(POSTOFFICENAME)
            addRow("POSTOFFICENAME") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_POSTOFFICENAME"), HiddenField).Value

            '荷受人コード(CONSIGNEECODE)
            addRow("CONSIGNEECODE") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CONSIGNEECODE"), HiddenField).Value

            '荷受人名(CONSIGNEENAME)
            addRow("CONSIGNEENAME") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CONSIGNEENAME"), HiddenField).Value

            ''油種コード(OILCODE)
            'addRow("OILCODE") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_OILCODE"), HiddenField).Value

            ''油種名(OILNAME)
            'addRow("OILNAME") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_OILNAME"), HiddenField).Value

            ''油種細分コード(受注用)(ORDERINGTYPE)
            'addRow("ORDERINGTYPE") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_ORDERINGTYPE"), HiddenField).Value

            ''油種名(受注用(ORDERINGOILNAME)
            'addRow("ORDERINGOILNAME") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_ORDERINGOILNAME"), HiddenField).Value

            ''数量(CARSAMOUNT)
            'Dim quantity As Decimal = 0
            'Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CARSAMOUNT"), TextBox).Text, quantity)
            'addRow("CARSAMOUNT") = quantity

            ''車数(CARSNUMBER)
            'Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CARSNUMBER"), TextBox).Text, quantity)
            'addRow("CARSNUMBER") = quantity

            ''屯数(LOADAMOUNT)
            'Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_LOADAMOUNT"), TextBox).Text, quantity)
            'addRow("LOADAMOUNT") = quantity

            ''単価(UNITPRICE)
            'Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_UNITPRICE"), TextBox).Text, quantity)
            'addRow("UNITPRICE") = quantity

            '金額(AMOUNT)
            Dim amount As Decimal = 0
            Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_AMOUNT"), TextBox).Text, amount)
            addRow("AMOUNT") = amount

            '税金(TAX)
            Dim consumptionTax As Decimal = 0
            Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTDETAILTBL_CONSUMPTIONTAX"), HiddenField).Value, consumptionTax)
            addRow("TAX") = Math.Round(amount * consumptionTax)
            addRow("CONSUMPTIONTAX") = consumptionTax

            '合計(TOTAL)
            addRow("TOTAL") = addRow("AMOUNT") + addRow("TAX")

            '摘要(TEKIYOU)
            addRow("TEKIYOU") = DirectCast(gRow.FindControl("WF_COSTDETAILTBL_TEKIYOU"), TextBox).Text

            'テーブルに行追加
            TMP0009tbl.Rows.Add(addRow)
        Next

        '明細番号ふり直し
        For i As Integer = 0 To TMP0009tbl.Rows.Count - 1
            TMP0009tbl.Rows(i)("DETAILNO") = i + 1
        Next

    End Sub

    ''' <summary>
    ''' 勘定科目パターン名称を勘定科目名/セグメント名/セグメント枝番名に分割する
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    Private Function ConvertAccountPatternName(ByVal source As String) As String()

        Dim retStrs As String() = Nothing

        '元文字列の最初の「(」を半角空白に変換
        Dim repSource = Replace(source, "(", " ", 1, 1)
        '元文字列の最後の「)」を除去
        repSource = repSource.Substring(0, repSource.LastIndexOf(")"))
        '変換した文字列を半角空白で分割
        retStrs = repSource.Split(" ")

        Return retStrs

    End Function

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
                    '営業所コード
                    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CONSIGNEECODE"
                    '荷受人コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERSCODE"
                    '荷主コード
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ACCOUNTPATTERN"
                    '勘定科目パターン
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTPATTERN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TORI_DEPT"
                    '請求先コード/支払先コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TORI_DEPT")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

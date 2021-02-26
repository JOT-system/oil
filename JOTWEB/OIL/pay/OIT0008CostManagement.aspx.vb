﻿Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 費用管理画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0008CostManagement
    Inherits Page

    '○ 検索結果格納Table
    Private OIT0008tbl As DataTable                                 ' 一覧格納用テーブル
    Private OIT0008INPtbl As DataTable                              ' チェック用テーブル
    Private OIT0008SubTotaltbl As DataTable                         ' 小計テーブル

    Private OIM0002tbl As DataTable

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    ' ログ出力
    Private CS0013ProfView As New CS0013ProfView                    ' Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      ' 更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  ' XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  ' 権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        ' 帳票出力
    Private CS0050SESSION As New CS0050SESSION                      ' セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    ' サブ用リターンコード
    Private WW_FOCUS_CONTROL As String = ""
    Private WW_FOCUS_ROW As Integer = 0
    Private WW_OFFICECODE As String = ""
    Private WW_KEIJYO_YM As String = ""
    Private WW_EDITABLEFLG As Boolean = True

    '〇　請求/支払合計計算用
    Private WK_INV_AMOUNT_ALL As Integer = 0
    Private WK_INV_TAX_ALL As Integer = 0
    Private WK_PAY_AMOUNT_ALL As Integer = 0
    Private WK_PAY_TAX_ALL As Integer = 0

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

                    '項目の内容変更、又は「戻る」以外のポストバックでは、GridViewの表示内容をワークテーブルへ反映する
                    If Not WF_ButtonClick.Value = "WF_ButtonEND" OrElse
                       Not WF_ButtonClick.Value = "WF_ListboxDBclick" OrElse
                       Not WF_ButtonClick.Value = "WF_LeftBoxSelectClick" Then
                        SetGridViewToTempTable()
                    End If

                    Dim DisplayGridViewFlg As Boolean = True
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonEND"             '「戻る」ボタンクリック
                            WF_ButtonEND_Click()
                            DisplayGridViewFlg = False
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                            DisplayGridViewFlg = False
                        Case "WF_ListboxDBclick", "WF_ButtonSel" '（左ボックス）項目選択
                            WF_ButtonSel_Click()
                            DisplayGridViewFlg = False
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonCan"             '（左ボックス）キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_RadioButonClick"       '（右ボックス）ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '（右ボックス）メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_Button_OfficeCode"     '「営業所」「表示する」ボタンクリック
                            WF_OfficeButton_Click()
                            DisplayGridViewFlg = False
                        Case "WF_ButtonDELETEROW"       '「行削除」ボタンクリック
                            WF_Grid_DeleteRow()
                        Case "WF_ButtonADDROW"          '「行追加」ボタンクリック
                            WF_Grid_AddRow()
                        Case "WF_ButtonUPDATE"          '「保存する」ボタンクリック
                            WF_ButtonUPDATE_Click()
                            'Case "WF_ButtonCSV"             ' ダウンロードボタン押下
                            '    WF_ButtonDownload_Click()
                            'Case "WF_ButtonPrint"           ' 一覧印刷ボタン押下
                            '    WF_ButtonPrint_Click()
                        Case Else
                            If WF_ButtonClick.Value.Contains("WF_ButtonShowDetail") Then
                                WF_ButtonShowDetail()   '「詳細を見る」ボタンクリック
                            End If
                    End Select

                    '○ 一覧再表示処理
                    If DisplayGridViewFlg Then
                        WF_Grid_RELOAD(False)
                    End If
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
            If Not IsNothing(OIT0008tbl) Then
                OIT0008tbl.Clear()
                OIT0008tbl.Dispose()
                OIT0008tbl = Nothing
            End If

            If Not IsNothing(OIT0008INPtbl) Then
                OIT0008INPtbl.Clear()
                OIT0008INPtbl.Dispose()
                OIT0008INPtbl = Nothing
            End If

            If Not IsNothing(OIM0002tbl) Then
                OIM0002tbl.Clear()
                OIM0002tbl.Dispose()
                OIM0002tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 「詳細を見る」ボタンクリック処理
    ''' </summary>
    Protected Sub WF_ButtonShowDetail()
        Dim rowIdx As Integer = 0
        'ボタンの行番号を取得する
        Integer.TryParse(WF_ButtonClick.Value.Substring(WF_ButtonClick.Value.Length - 3), rowIdx)

        '明細画面の検索条件を設定
        '#
        work.WF_SEL_LINE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_LINE"), Label).Text
        '勘定科目コード
        work.WF_SEL_ACCOUNTCODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTCODE"), TextBox).Text
        '勘定科目名
        work.WF_SEL_ACCOUNTNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTNAME"), HiddenField).Value
        'セグメント
        work.WF_SEL_SEGMENTCODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTCODE"), Label).Text
        'セグメント名
        work.WF_SEL_SEGMENTNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTNAME"), HiddenField).Value
        'セグメント枝番
        work.WF_SEL_SEGMENTBRANCHCODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE"), HiddenField).Value
        'セグメント枝番名
        work.WF_SEL_SEGMENTBRANCHNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTBRANCHNAME"), Label).Text
        '荷主コード
        work.WF_SEL_SHIPPERSCODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SHIPPERSCODE"), HiddenField).Value
        '荷主名
        work.WF_SEL_SHIPPERSNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SHIPPERSNAME"), Label).Text
        '請求先コード
        work.WF_SEL_INVOICECODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox).Text
        '請求先名
        work.WF_SEL_INVOICENAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME"), Label).Text
        '請求先部門
        work.WF_SEL_INVOICEDEPTNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME"), Label).Text
        '支払先コード
        work.WF_SEL_PAYEECODE.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox).Text
        '支払先名
        work.WF_SEL_PAYEENAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME"), Label).Text
        '支払先部門
        work.WF_SEL_PAYEEDEPTNAME.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME"), Label).Text
        '摘要
        work.WF_SEL_TEKIYOU.Text = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_TEKIYOU"), TextBox).Text

        '明細画面に遷移
        Master.CheckParmissionCode(Master.USERCAMP)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' 営業所ボタンクリック処理
    ''' </summary>
    Private Sub WF_OfficeButton_Click()
        '選択中の営業所から営業所コードを取得
        Dim contents1 = Me.Controls(0).FindControl("contents1")
        Dim selectedHiddenId = DirectCast(contents1.FindControl("WF_OFFICEHDN_ID"), HiddenField).Value
        Dim selectedHiddenControl = DirectCast(contents1.FindControl(selectedHiddenId), HiddenField)
        WW_OFFICECODE = selectedHiddenControl.Value

        '前回リロード時の営業所コードと異なる場合
        If WW_OFFICECODE = work.WF_SEL_LAST_OFFICECODE.Text Then
            'リロード
            WF_Grid_RELOAD(False)
        Else
            '初期化リロード
            WF_Grid_RELOAD(True)
        End If

    End Sub

    ''' <summary>
    ''' 画面リロード処理
    ''' </summary>
    ''' <param name="InitFlg">初期化フラグ</param>
    Protected Sub WF_Grid_RELOAD(Optional ByRef InitFlg As Boolean = False)

        '選択中の営業所から営業所コードを取得
        Dim contents1 = Me.Controls(0).FindControl("contents1")
        Dim selectedHiddenId = DirectCast(contents1.FindControl("WF_OFFICEHDN_ID"), HiddenField).Value
        Dim selectedHiddenControl = DirectCast(contents1.FindControl(selectedHiddenId), HiddenField)
        WW_OFFICECODE = selectedHiddenControl.Value

        '今回表示する営業コードを保持
        work.WF_SEL_LAST_OFFICECODE.Text = WW_OFFICECODE

        '計上年月を取得
        WW_KEIJYO_YM = WF_KEIJYO_YM.Text

        '今回表示する計上年月を保持
        work.WF_SEL_LAST_KEIJYO_YM.Text = WW_KEIJYO_YM

        '初期表示計上年月よりも表示計上年月が小さい場合、編集不可とする
        If CDate(work.WF_SEL_INIT_KEIJYO_YM.Text).CompareTo(CDate(work.WF_SEL_LAST_KEIJYO_YM.Text)) > 0 Then
            WW_EDITABLEFLG = False
        Else
            WW_EDITABLEFLG = True
        End If

        'ボタン
        WF_ALLSELECT.Enabled = WW_EDITABLEFLG
        WF_ALLRELEACE.Enabled = WW_EDITABLEFLG
        WF_ADDROW.Enabled = WW_EDITABLEFLG
        WF_DELETEROW.Enabled = WW_EDITABLEFLG
        WF_UPDATE.Enabled = WW_EDITABLEFLG

        'データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection

            'データベース接続
            SQLcon.Open()

            '初期化フラグが立っている場合は初期化
            If InitFlg Then
                '費用管理ワークテーブル群の初期化
                InitTempTable(SQLcon)

                '費用管理ワークテーブル群の初期データ設定
                SetTempTable(SQLcon)

                '入力可能の計上年月の場合、入力行を1行追加
                If WW_EDITABLEFLG Then
                    WF_Grid_AddRow()
                End If
            End If

            'GridView設定
            GridViewSetup(SQLcon)
        End Using

        '営業所ボタンのスタイル変更
        Dim endIndex = selectedHiddenId.Split("_")
        SetOfficeStyle(endIndex(2))

    End Sub

    ''' <summary>
    ''' 営業所ボタンのスタイル設定
    ''' </summary>
    ''' <param name="endIndex">選択中ボタンの番号</param>
    Protected Sub SetOfficeStyle(ByRef endIndex As String)

        'いったん初期化
        WF_OFFICEBTN_1.CssClass = "btn-office"
        WF_OFFICEBTN_2.CssClass = "btn-office"
        WF_OFFICEBTN_3.CssClass = "btn-office"
        WF_OFFICEBTN_4.CssClass = "btn-office"
        WF_OFFICEBTN_5.CssClass = "btn-office"
        WF_OFFICEBTN_6.CssClass = "btn-office"
        WF_OFFICEBTN_7.CssClass = "btn-office"
        WF_OFFICEBTN_8.CssClass = "btn-office"
        WF_OFFICEBTN_9.CssClass = "btn-office"
        WF_OFFICEBTN_10.CssClass = "btn-office"
        WF_OFFICEBTN_11.CssClass = "btn-office last"

        '選択されているボタンのコントロールを得る
        Dim btnControl = DirectCast(Me.Controls(0).FindControl("contents1").FindControl("WF_OFFICEBTN_" + endIndex), Button)
        btnControl.CssClass += " selected"

    End Sub

    ''' <summary>
    ''' 行削除
    ''' </summary>
    Protected Sub WF_Grid_DeleteRow()

        '選択行を削除
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" DELETE FROM [oil].TMP0008_COST")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     OFFICECODE = @P01")
        SQLStrBldr.AppendLine(" AND KEIJYOYM = @P02")
        SQLStrBldr.AppendLine(" AND CHECKFLG = 1")

        '行番号の振り直し
        Dim MergeSQLStrBldr As New StringBuilder
        MergeSQLStrBldr.AppendLine(" MERGE [oil].TMP0008_COST AS OLD_T")
        MergeSQLStrBldr.AppendLine(" USING (")
        MergeSQLStrBldr.AppendLine("     SELECT")
        MergeSQLStrBldr.AppendLine("         OFFICECODE")
        MergeSQLStrBldr.AppendLine("         , KEIJYOYM")
        MergeSQLStrBldr.AppendLine("         , LINE")
        MergeSQLStrBldr.AppendLine("         , ROW_NUMBER() OVER(ORDER BY LINE) AS NEW_LINE")
        MergeSQLStrBldr.AppendLine("     FROM")
        MergeSQLStrBldr.AppendLine("         [oil].TMP0008_COST")
        MergeSQLStrBldr.AppendLine("     WHERE")
        MergeSQLStrBldr.AppendLine("         OFFICECODE = @P01")
        MergeSQLStrBldr.AppendLine("     AND KEIJYOYM = @P02")
        MergeSQLStrBldr.AppendLine(" ) AS NEW_T")
        MergeSQLStrBldr.AppendLine("     ON  OLD_T.OFFICECODE = NEW_T.OFFICECODE")
        MergeSQLStrBldr.AppendLine("     AND OLD_T.KEIJYOYM = NEW_T.KEIJYOYM")
        MergeSQLStrBldr.AppendLine("     AND OLD_T.LINE = NEW_T.LINE")
        MergeSQLStrBldr.AppendLine(" WHEN MATCHED THEN UPDATE SET OLD_T.LINE = NEW_T.NEW_LINE;")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()

                Using DelRowCmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon), MergeCmd As New SqlCommand(MergeSQLStrBldr.ToString(), SQLcon)
                    Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                    Dim PARA1 As SqlParameter = DelRowCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                    Dim PARA2 As SqlParameter = DelRowCmd.Parameters.Add("@P02", SqlDbType.Date)
                    Dim MPARA1 As SqlParameter = MergeCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                    Dim MPARA2 As SqlParameter = MergeCmd.Parameters.Add("@P02", SqlDbType.Date)

                    '行削除
                    PARA1.Value = WW_OFFICECODE
                    PARA2.Value = WK_DATE
                    DelRowCmd.CommandTimeout = 300
                    DelRowCmd.ExecuteNonQuery()

                    '行番号振り直し
                    MPARA1.Value = WW_OFFICECODE
                    MPARA2.Value = WK_DATE
                    MergeCmd.CommandTimeout = 300
                    MergeCmd.ExecuteNonQuery()

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M TMP0008_COST DELETE_ROW")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M TMP0008_COST DELETE_ROW"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 行追加
    ''' </summary>
    Protected Sub WF_Grid_AddRow()

        '空行を追加
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" INSERT INTO [oil].TMP0008_COST")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     @P01 AS OFFICECODE")
        SQLStrBldr.AppendLine("     , @P02 AS KEIJYOYM")
        SQLStrBldr.AppendLine("     , ISNULL((SELECT MAX(LINE) FROM [oil].TMP0008_COST WHERE OFFICECODE = @P01 AND KEIJYOYM = @P02), 0) + 1 AS LINE")
        SQLStrBldr.AppendLine("     , 0 AS CHEKFLG")
        SQLStrBldr.AppendLine("     , '2' AS CALCACCOUNT")
        SQLStrBldr.AppendLine("     , '' AS ACCOUNTCODE")
        SQLStrBldr.AppendLine("     , '' AS ACCOUNTNAME")
        SQLStrBldr.AppendLine("     , '' AS SEGMENTCODE")
        SQLStrBldr.AppendLine("     , '' AS SEGMENTNAME")
        SQLStrBldr.AppendLine("     , '' AS SEGMENTBRANCHCODE")
        SQLStrBldr.AppendLine("     , '' AS SEGMENTBRANCHNAME")
        SQLStrBldr.AppendLine("     , '' AS SHIPPERSCODE")
        SQLStrBldr.AppendLine("     , '' AS SHIPPERSNAME")
        SQLStrBldr.AppendLine("     , 0.0 AS QUANTITY")
        SQLStrBldr.AppendLine("     , 0.0 AS UNITPRICE")
        SQLStrBldr.AppendLine("     , 0 AS AMOUNT")
        SQLStrBldr.AppendLine("     , 0 AS TAX")
        SQLStrBldr.AppendLine("     , '' AS INVOICECODE")
        SQLStrBldr.AppendLine("     , '' AS INVOICENAME")
        SQLStrBldr.AppendLine("     , '' AS INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("     , '' AS PAYEECODE")
        SQLStrBldr.AppendLine("     , '' AS PAYEENAME")
        SQLStrBldr.AppendLine("     , '' AS PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("     , '' AS TEKIYOU")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()

                Using InsBlankCmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    Dim PARA1 As SqlParameter = InsBlankCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                    Dim PARA2 As SqlParameter = InsBlankCmd.Parameters.Add("@P02", SqlDbType.Date)

                    '費用管理ワークテーブルへ空行を追加
                    PARA1.Value = WW_OFFICECODE
                    Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                    PARA2.Value = WK_DATE


                    InsBlankCmd.CommandTimeout = 300
                    InsBlankCmd.ExecuteNonQuery()

                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M TMP0008_COST INSERT_BLANK_ROW")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M TMP0008_COST INSERT_BLANK_ROW"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 費用管理ワークテーブルの更新(GridViewから入力テーブルへの変換)
    ''' </summary>
    Protected Sub SetGridViewToTempTable()

        '前回表示時の営業所コードを設定
        WW_OFFICECODE = work.WF_SEL_LAST_OFFICECODE.Text

        '前回表示時の計上年月を設定
        WW_KEIJYO_YM = work.WF_SEL_LAST_KEIJYO_YM.Text

        '入力テーブル作成
        OIT0008INPtbl = New DataTable
        OIT0008INPtbl.Columns.Add("LINE", Type.GetType("System.Int32"))
        OIT0008INPtbl.Columns.Add("CHECKFLG", Type.GetType("System.Int32"))
        OIT0008INPtbl.Columns.Add("CALCACCOUNT", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("ACCOUNTCODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("ACCOUNTNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SEGMENTCODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SEGMENTNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SEGMENTBRANCHCODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SEGMENTBRANCHNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SHIPPERSCODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("SHIPPERSNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("QUANTITY", Type.GetType("System.Decimal"))
        OIT0008INPtbl.Columns.Add("AMOUNT", Type.GetType("System.Decimal"))
        OIT0008INPtbl.Columns.Add("TAX", Type.GetType("System.Decimal"))
        OIT0008INPtbl.Columns.Add("INVOICECODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("INVOICENAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("INVOICEDEPTNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("PAYEECODE", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("PAYEENAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("PAYEEDEPTNAME", Type.GetType("System.String"))
        OIT0008INPtbl.Columns.Add("TEKIYOU", Type.GetType("System.String"))

        'GridViewの行を検索
        For Each gRow As GridViewRow In WF_COSTLISTTBL.Rows

            Dim addRow = OIT0008INPtbl.NewRow

            'データ行でなければ処理を行わない
            If Not gRow.RowType = DataControlRowType.DataRow Then
                Continue For
            End If

            '確認ボタンが使用可ならば自動計算科目なので、処理をスキップする
            If DirectCast(gRow.FindControl("WF_COSTLISTTBL_CALCACCOUNT"), Button).Enabled = True Then
                addRow("CALCACCOUNT") = "1"
            Else
                addRow("CALCACCOUNT") = "2"
            End If

            '#(LINE)
            addRow("LINE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_LINE"), Label).Text

            '選択(CHECKFLG)
            If DirectCast(gRow.FindControl("WF_COSTLISTTBL_CHECKFLG"), CheckBox).Checked Then
                addRow("CHECKFLG") = 1
            Else
                addRow("CHECKFLG") = 0
            End If

            '勘定科目コード(ACCOUNTCODE)
            addRow("ACCOUNTCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_ACCOUNTCODE"), TextBox).Text

            '勘定科目名(ACCOUNTNAME)
            addRow("ACCOUNTNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_ACCOUNTNAME"), HiddenField).Value

            'セグメント(SEGMENTCODE)
            addRow("SEGMENTCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTCODE"), Label).Text

            'セグメント名(SEGMENTNAME)
            addRow("SEGMENTNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTNAME"), HiddenField).Value

            'セグメント枝番(SEGMENTBRANCHCODE)
            addRow("SEGMENTBRANCHCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE"), HiddenField).Value

            'セグメント枝番名(SEGMENTBRANCHNAME)
            addRow("SEGMENTBRANCHNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTBRANCHNAME"), Label).Text

            '荷主コード(SHIPPERSCODE)
            addRow("SHIPPERSCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SHIPPERSCODE"), HiddenField).Value

            '荷主名(SHIPPERSNAME)
            addRow("SHIPPERSNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SHIPPERSNAME"), Label).Text

            '数量(QUANTITY)
            Dim quantity As Decimal = 0
            Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTLISTTBL_QUANTITY"), HiddenField).Value, quantity)
            addRow("QUANTITY") = quantity

            '金額(AMOUNT)
            Dim amount As Decimal = 0
            Decimal.TryParse(DirectCast(gRow.FindControl("WF_COSTLISTTBL_AMOUNT"), TextBox).Text, amount)
            addRow("AMOUNT") = amount

            '税金(TAX)
            Dim tax As Decimal = 0
            addRow("TAX") = Math.Floor(amount * 0.1)

            '請求先コード(INVOICECODE)
            addRow("INVOICECODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox).Text

            '請求先名(INVOICENAME)
            addRow("INVOICENAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICENAME"), Label).Text

            '請求先部門(INVOICEDEPTNAME)
            addRow("INVOICEDEPTNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME"), Label).Text

            '支払先コード(PAYEECODE)
            addRow("PAYEECODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox).Text

            '支払先名(PAYEENAME)
            addRow("PAYEENAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEENAME"), Label).Text

            '支払先部門(PAYEEDEPTNAME)
            addRow("PAYEEDEPTNAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME"), Label).Text

            '摘要(TEKIYOU)
            addRow("TEKIYOU") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_TEKIYOU"), TextBox).Text

            'テーブルに行追加
            OIT0008INPtbl.Rows.Add(addRow)
        Next

        '更新対象がなければ処理終了
        If OIT0008INPtbl.Rows.Count = 0 Then Exit Sub

        '画面表示データをワークテーブルへ反映
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            UpdateTempTable(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' 費用管理ワークテーブルの更新(DB処理)
    ''' </summary>
    ''' <param name="SQLcon"></param>
    Protected Sub UpdateTempTable(ByVal SQLcon As SqlConnection)

        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" MERGE [oil].TMP0008_COST AS T0008")
        SQLStrBldr.AppendLine(" USING (")
        SQLStrBldr.AppendLine("     SELECT")
        SQLStrBldr.AppendLine("         @P01 AS OFFICECODE")
        SQLStrBldr.AppendLine("         , @P02 AS KEIJYOYM")
        SQLStrBldr.AppendLine("         , @P03 AS LINE")
        SQLStrBldr.AppendLine("         , @P04 AS CHECKFLG")
        SQLStrBldr.AppendLine("         , @P05 AS CALCACCOUNT")
        SQLStrBldr.AppendLine("         , @P06 AS ACCOUNTCODE")
        SQLStrBldr.AppendLine("         , @P22 AS ACCOUNTNAME")
        SQLStrBldr.AppendLine("         , @P07 AS SEGMENTCODE")
        SQLStrBldr.AppendLine("         , @P23 AS SEGMENTNAME")
        SQLStrBldr.AppendLine("         , @P08 AS SEGMENTBRANCHCODE")
        SQLStrBldr.AppendLine("         , @P24 AS SEGMENTBRANCHNAME")
        SQLStrBldr.AppendLine("         , @P09 AS SHIPPERSCODE")
        SQLStrBldr.AppendLine("         , @P10 AS SHIPPERSNAME")
        SQLStrBldr.AppendLine("         , @P11 AS QUANTITY")
        SQLStrBldr.AppendLine("         , @P12 AS UNITPRICE")
        SQLStrBldr.AppendLine("         , @P13 AS AMOUNT")
        SQLStrBldr.AppendLine("         , @P14 AS TAX")
        SQLStrBldr.AppendLine("         , @P15 AS INVOICECODE")
        SQLStrBldr.AppendLine("         , @P16 AS INVOICENAME")
        SQLStrBldr.AppendLine("         , @P17 AS INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("         , @P18 AS PAYEECODE")
        SQLStrBldr.AppendLine("         , @P19 AS PAYEENAME")
        SQLStrBldr.AppendLine("         , @P20 AS PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("         , @P21 AS TEKIYOU")
        SQLStrBldr.AppendLine(" ) AS GVROW")
        SQLStrBldr.AppendLine("     ON  T0008.OFFICECODE = GVROW.OFFICECODE")
        SQLStrBldr.AppendLine("     AND T0008.KEIJYOYM = GVROW.KEIJYOYM")
        SQLStrBldr.AppendLine("     AND T0008.LINE = GVROW.LINE")
        SQLStrBldr.AppendLine("     AND GVROW.CALCACCOUNT = '2'")
        SQLStrBldr.AppendLine(" WHEN MATCHED")
        SQLStrBldr.AppendLine("         THEN UPDATE")
        SQLStrBldr.AppendLine("             SET")
        SQLStrBldr.AppendLine("                 T0008.CHECKFLG = GVROW.CHECKFLG")
        SQLStrBldr.AppendLine("                 , T0008.CALCACCOUNT = GVROW.CALCACCOUNT")
        SQLStrBldr.AppendLine("                 , T0008.ACCOUNTCODE = GVROW.ACCOUNTCODE")
        SQLStrBldr.AppendLine("                 , T0008.ACCOUNTNAME = GVROW.ACCOUNTNAME")
        SQLStrBldr.AppendLine("                 , T0008.SEGMENTCODE = GVROW.SEGMENTCODE")
        SQLStrBldr.AppendLine("                 , T0008.SEGMENTNAME = GVROW.SEGMENTNAME")
        SQLStrBldr.AppendLine("                 , T0008.SEGMENTBRANCHCODE = GVROW.SEGMENTBRANCHCODE")
        SQLStrBldr.AppendLine("                 , T0008.SEGMENTBRANCHNAME = GVROW.SEGMENTBRANCHNAME")
        SQLStrBldr.AppendLine("                 , T0008.SHIPPERSCODE = GVROW.SHIPPERSCODE")
        SQLStrBldr.AppendLine("                 , T0008.SHIPPERSNAME = GVROW.SHIPPERSNAME")
        SQLStrBldr.AppendLine("                 , T0008.QUANTITY = GVROW.QUANTITY")
        SQLStrBldr.AppendLine("                 , T0008.UNITPRICE = GVROW.UNITPRICE")
        SQLStrBldr.AppendLine("                 , T0008.AMOUNT = GVROW.AMOUNT")
        SQLStrBldr.AppendLine("                 , T0008.TAX = GVROW.TAX")
        SQLStrBldr.AppendLine("                 , T0008.INVOICECODE = GVROW.INVOICECODE")
        SQLStrBldr.AppendLine("                 , T0008.INVOICENAME = GVROW.INVOICENAME")
        SQLStrBldr.AppendLine("                 , T0008.INVOICEDEPTNAME = GVROW.INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("                 , T0008.PAYEECODE = GVROW.PAYEECODE")
        SQLStrBldr.AppendLine("                 , T0008.PAYEENAME = GVROW.PAYEENAME")
        SQLStrBldr.AppendLine("                 , T0008.PAYEEDEPTNAME = GVROW.PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("                 , T0008.TEKIYOU = GVROW.TEKIYOU")
        SQLStrBldr.AppendLine(" WHEN NOT MATCHED BY TARGET")
        SQLStrBldr.AppendLine("         THEN INSERT (")
        SQLStrBldr.AppendLine("                  OFFICECODE")
        SQLStrBldr.AppendLine("                  , KEIJYOYM")
        SQLStrBldr.AppendLine("                  , LINE")
        SQLStrBldr.AppendLine("                  , CHECKFLG")
        SQLStrBldr.AppendLine("                  , CALCACCOUNT")
        SQLStrBldr.AppendLine("                  , ACCOUNTCODE")
        SQLStrBldr.AppendLine("                  , ACCOUNTNAME")
        SQLStrBldr.AppendLine("                  , SEGMENTCODE")
        SQLStrBldr.AppendLine("                  , SEGMENTNAME")
        SQLStrBldr.AppendLine("                  , SEGMENTBRANCHCODE")
        SQLStrBldr.AppendLine("                  , SEGMENTBRANCHNAME")
        SQLStrBldr.AppendLine("                  , SHIPPERSCODE")
        SQLStrBldr.AppendLine("                  , SHIPPERSNAME")
        SQLStrBldr.AppendLine("                  , QUANTITY")
        SQLStrBldr.AppendLine("                  , UNITPRICE")
        SQLStrBldr.AppendLine("                  , AMOUNT")
        SQLStrBldr.AppendLine("                  , TAX")
        SQLStrBldr.AppendLine("                  , INVOICECODE")
        SQLStrBldr.AppendLine("                  , INVOICENAME")
        SQLStrBldr.AppendLine("                  , INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("                  , PAYEECODE")
        SQLStrBldr.AppendLine("                  , PAYEENAME")
        SQLStrBldr.AppendLine("                  , PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("                  , TEKIYOU")
        SQLStrBldr.AppendLine("              ) VALUES (")
        SQLStrBldr.AppendLine("                  GVROW.OFFICECODE")
        SQLStrBldr.AppendLine("                  , GVROW.KEIJYOYM")
        SQLStrBldr.AppendLine("                  , GVROW.LINE")
        SQLStrBldr.AppendLine("                  , GVROW.CHECKFLG")
        SQLStrBldr.AppendLine("                  , GVROW.CALCACCOUNT")
        SQLStrBldr.AppendLine("                  , GVROW.ACCOUNTCODE")
        SQLStrBldr.AppendLine("                  , GVROW.ACCOUNTNAME")
        SQLStrBldr.AppendLine("                  , GVROW.SEGMENTCODE")
        SQLStrBldr.AppendLine("                  , GVROW.SEGMENTNAME")
        SQLStrBldr.AppendLine("                  , GVROW.SEGMENTBRANCHCODE")
        SQLStrBldr.AppendLine("                  , GVROW.SEGMENTBRANCHNAME")
        SQLStrBldr.AppendLine("                  , GVROW.SHIPPERSCODE")
        SQLStrBldr.AppendLine("                  , GVROW.SHIPPERSNAME")
        SQLStrBldr.AppendLine("                  , GVROW.QUANTITY")
        SQLStrBldr.AppendLine("                  , GVROW.UNITPRICE")
        SQLStrBldr.AppendLine("                  , GVROW.AMOUNT")
        SQLStrBldr.AppendLine("                  , GVROW.TAX")
        SQLStrBldr.AppendLine("                  , GVROW.INVOICECODE")
        SQLStrBldr.AppendLine("                  , GVROW.INVOICENAME")
        SQLStrBldr.AppendLine("                  , GVROW.INVOICEDEPTNAME")
        SQLStrBldr.AppendLine("                  , GVROW.PAYEECODE")
        SQLStrBldr.AppendLine("                  , GVROW.PAYEENAME")
        SQLStrBldr.AppendLine("                  , GVROW.PAYEEDEPTNAME")
        SQLStrBldr.AppendLine("                  , GVROW.TEKIYOU")
        SQLStrBldr.AppendLine("              );")

        Dim UpdateBldr As New StringBuilder
        UpdateBldr.AppendLine(" UPDATE [oil].TMP0008_COST")
        UpdateBldr.AppendLine(" SET")
        UpdateBldr.AppendLine("     TEKIYOU = @P04")
        UpdateBldr.AppendLine(" WHERE")
        UpdateBldr.AppendLine("     OFFICECODE = @P01")
        UpdateBldr.AppendLine(" AND KEIJYOYM = @P02")
        UpdateBldr.AppendLine(" AND LINE = @P03")

        Try
            Using MergeCmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon), UpdateCmd As New SqlCommand(UpdateBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = MergeCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = MergeCmd.Parameters.Add("@P02", SqlDbType.Date)
                Dim PARA3 As SqlParameter = MergeCmd.Parameters.Add("@P03", SqlDbType.Int)
                Dim PARA4 As SqlParameter = MergeCmd.Parameters.Add("@P04", SqlDbType.Int)
                Dim PARA5 As SqlParameter = MergeCmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)
                Dim PARA6 As SqlParameter = MergeCmd.Parameters.Add("@P06", SqlDbType.NVarChar, 8)
                Dim PARA7 As SqlParameter = MergeCmd.Parameters.Add("@P07", SqlDbType.NVarChar, 5)
                Dim PARA8 As SqlParameter = MergeCmd.Parameters.Add("@P08", SqlDbType.NVarChar, 2)
                Dim PARA9 As SqlParameter = MergeCmd.Parameters.Add("@P09", SqlDbType.NVarChar, 10)
                Dim PARA10 As SqlParameter = MergeCmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40)
                Dim PARA11 As SqlParameter = MergeCmd.Parameters.Add("@P11", SqlDbType.Float)
                Dim PARA12 As SqlParameter = MergeCmd.Parameters.Add("@P12", SqlDbType.Money)
                Dim PARA13 As SqlParameter = MergeCmd.Parameters.Add("@P13", SqlDbType.Money)
                Dim PARA14 As SqlParameter = MergeCmd.Parameters.Add("@P14", SqlDbType.Money)
                Dim PARA15 As SqlParameter = MergeCmd.Parameters.Add("@P15", SqlDbType.NVarChar, 10)
                Dim PARA16 As SqlParameter = MergeCmd.Parameters.Add("@P16", SqlDbType.NVarChar, 40)
                Dim PARA17 As SqlParameter = MergeCmd.Parameters.Add("@P17", SqlDbType.NVarChar, 40)
                Dim PARA18 As SqlParameter = MergeCmd.Parameters.Add("@P18", SqlDbType.NVarChar, 10)
                Dim PARA19 As SqlParameter = MergeCmd.Parameters.Add("@P19", SqlDbType.NVarChar, 40)
                Dim PARA20 As SqlParameter = MergeCmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40)
                Dim PARA21 As SqlParameter = MergeCmd.Parameters.Add("@P21", SqlDbType.NVarChar, 200)
                Dim PARA22 As SqlParameter = MergeCmd.Parameters.Add("@P22", SqlDbType.NVarChar, 40)
                Dim PARA23 As SqlParameter = MergeCmd.Parameters.Add("@P23", SqlDbType.NVarChar, 40)
                Dim PARA24 As SqlParameter = MergeCmd.Parameters.Add("@P24", SqlDbType.NVarChar, 40)

                Dim UPARA1 As SqlParameter = UpdateCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim UPARA2 As SqlParameter = UpdateCmd.Parameters.Add("@P02", SqlDbType.Date)
                Dim UPARA3 As SqlParameter = UpdateCmd.Parameters.Add("@P03", SqlDbType.Int)
                Dim UPARA4 As SqlParameter = UpdateCmd.Parameters.Add("@P04", SqlDbType.NVarChar, 200)

                '費用管理ワークテーブルのデータ更新
                PARA1.Value = WW_OFFICECODE
                UPARA1.Value = WW_OFFICECODE
                Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                PARA2.Value = WK_DATE
                UPARA2.Value = WK_DATE

                '入力テーブルに格納された行数分、更新処理を行う
                For Each row As DataRow In OIT0008INPtbl.Rows

                    '自動計算科目以外はスキップ
                    If row("CALCACCOUNT") = "1" Then
                        UPARA3.Value = row("LINE")
                        UPARA4.Value = row("TEKIYOU")

                        UpdateCmd.CommandTimeout = 300
                        UpdateCmd.ExecuteNonQuery()
                    Else
                        PARA3.Value = row("LINE")
                        PARA4.Value = row("CHECKFLG")
                        PARA5.Value = row("CALCACCOUNT")
                        PARA6.Value = row("ACCOUNTCODE")
                        PARA22.Value = row("ACCOUNTNAME")
                        PARA7.Value = row("SEGMENTCODE")
                        PARA23.Value = row("SEGMENTNAME")
                        PARA8.Value = row("SEGMENTBRANCHCODE")
                        PARA24.Value = row("SEGMENTBRANCHNAME")
                        PARA9.Value = row("SHIPPERSCODE")
                        PARA10.Value = row("SHIPPERSNAME")
                        PARA11.Value = row("QUANTITY")
                        PARA12.Value = 0.0  '単価(0固定)
                        PARA13.Value = row("AMOUNT")
                        PARA15.Value = row("INVOICECODE")
                        PARA14.Value = Math.Floor(row("AMOUNT") * 0.1) '税額(10%切り捨て)
                        PARA16.Value = row("INVOICENAME")
                        PARA17.Value = row("INVOICEDEPTNAME")
                        PARA18.Value = row("PAYEECODE")
                        PARA19.Value = row("PAYEENAME")
                        PARA20.Value = row("PAYEEDEPTNAME")
                        PARA21.Value = row("TEKIYOU")

                        MergeCmd.CommandTimeout = 300
                        MergeCmd.ExecuteNonQuery()
                    End If

                Next

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M TMP0008_COST MERGE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M TMP0008_COST MERGE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0008WRKINC.MAPIDM
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
        rightview.MAPID = OIT0008WRKINC.MAPIDM
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueInitSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueInitSet()

        '対象全営業所取得
        GetAllOffice()

        '営業所ボタンの設定
        WF_OFFICEBTN_1.Text = OIM0002tbl.Rows(0)("OFFICENAME")
        WF_OFFICEHDN_1.Value = OIM0002tbl.Rows(0)("OFFICECODE")
        WF_OFFICEBTN_1.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_1')"

        WF_OFFICEBTN_2.Text = OIM0002tbl.Rows(1)("OFFICENAME")
        WF_OFFICEHDN_2.Value = OIM0002tbl.Rows(1)("OFFICECODE")
        WF_OFFICEBTN_2.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_2')"

        WF_OFFICEBTN_3.Text = OIM0002tbl.Rows(2)("OFFICENAME")
        WF_OFFICEHDN_3.Value = OIM0002tbl.Rows(2)("OFFICECODE")
        WF_OFFICEBTN_3.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_3')"

        WF_OFFICEBTN_4.Text = OIM0002tbl.Rows(3)("OFFICENAME")
        WF_OFFICEHDN_4.Value = OIM0002tbl.Rows(3)("OFFICECODE")
        WF_OFFICEBTN_4.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_4')"

        WF_OFFICEBTN_5.Text = OIM0002tbl.Rows(4)("OFFICENAME")
        WF_OFFICEHDN_5.Value = OIM0002tbl.Rows(4)("OFFICECODE")
        WF_OFFICEBTN_5.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_5')"

        WF_OFFICEBTN_6.Text = OIM0002tbl.Rows(5)("OFFICENAME")
        WF_OFFICEHDN_6.Value = OIM0002tbl.Rows(5)("OFFICECODE")
        WF_OFFICEBTN_6.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_6')"

        WF_OFFICEBTN_7.Text = OIM0002tbl.Rows(6)("OFFICENAME")
        WF_OFFICEHDN_7.Value = OIM0002tbl.Rows(6)("OFFICECODE")
        WF_OFFICEBTN_7.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_7')"

        WF_OFFICEBTN_8.Text = OIM0002tbl.Rows(7)("OFFICENAME")
        WF_OFFICEHDN_8.Value = OIM0002tbl.Rows(7)("OFFICECODE")
        WF_OFFICEBTN_8.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_8')"

        WF_OFFICEBTN_9.Text = OIM0002tbl.Rows(8)("OFFICENAME")
        WF_OFFICEHDN_9.Value = OIM0002tbl.Rows(8)("OFFICECODE")
        WF_OFFICEBTN_9.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_9')"

        WF_OFFICEBTN_10.Text = OIM0002tbl.Rows(9)("OFFICENAME")
        WF_OFFICEHDN_10.Value = OIM0002tbl.Rows(9)("OFFICECODE")
        WF_OFFICEBTN_10.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_10')"

        WF_OFFICEBTN_11.Text = OIM0002tbl.Rows(10)("OFFICENAME")
        WF_OFFICEHDN_11.Value = OIM0002tbl.Rows(10)("OFFICECODE")
        WF_OFFICEBTN_11.OnClientClick = "OfficeButtonClick('WF_OFFICEHDN_11')"

        '所属営業所によるボタンの制御
        SetOfficeAuth()

        'メニュー画面からの遷移の場合
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then
            '計上月の初期化
            InitKEIJYO_YM()

            '画面表示データの取得(初期化)
            WF_Grid_RELOAD(True)
        Else
            Select Case work.WF_SEL_LAST_OFFICECODE.Text
                Case WF_OFFICEHDN_1.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_1.ID
                Case WF_OFFICEHDN_2.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_2.ID
                Case WF_OFFICEHDN_3.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_3.ID
                Case WF_OFFICEHDN_4.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_4.ID
                Case WF_OFFICEHDN_5.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_5.ID
                Case WF_OFFICEHDN_6.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_6.ID
                Case WF_OFFICEHDN_7.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_7.ID
                Case WF_OFFICEHDN_8.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_8.ID
                Case WF_OFFICEHDN_9.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_9.ID
                Case WF_OFFICEHDN_10.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_10.ID
                Case WF_OFFICEHDN_11.Value
                    WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_11.ID
            End Select

            '計上年月を最終表示計上年月に設定
            WF_KEIJYO_YM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text

            '画面表示データの取得(初期化なし)
            WF_Grid_RELOAD(False)
        End If

        '詳細画面検索条件を初期化
        work.WF_SEL_LINE.Text = ""
        work.WF_SEL_ACCOUNTCODE.Text = ""
        work.WF_SEL_ACCOUNTNAME.Text = ""
        work.WF_SEL_SEGMENTCODE.Text = ""
        work.WF_SEL_SEGMENTNAME.Text = ""
        work.WF_SEL_SEGMENTBRANCHCODE.Text = ""
        work.WF_SEL_SEGMENTBRANCHNAME.Text = ""
        work.WF_SEL_SHIPPERSCODE.Text = ""
        work.WF_SEL_SHIPPERSNAME.Text = ""
        work.WF_SEL_INVOICECODE.Text = ""
        work.WF_SEL_INVOICENAME.Text = ""
        work.WF_SEL_INVOICEDEPTNAME.Text = ""
        work.WF_SEL_PAYEECODE.Text = ""
        work.WF_SEL_PAYEENAME.Text = ""
        work.WF_SEL_PAYEEDEPTNAME.Text = ""
        work.WF_SEL_TEKIYOU.Text = ""

    End Sub

    ''' <summary>
    ''' 計上年月初期化
    ''' </summary>
    Protected Sub InitKEIJYO_YM()

        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     FORMAT(KEIJYOYM, 'yyyy/MM') AS KEIJYOYM")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     [oil].OIT0019_KEIJYOYM")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    'SQL実行
                    Dim WK_TBL As DataTable = New DataTable()
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            WK_TBL.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        WK_TBL.Load(SQLdr)
                    End Using

                    '計上年月を設定
                    WF_KEIJYO_YM.Text = WK_TBL.Rows(0)("KEIJYOYM")
                    '初期計上年月を保持
                    work.WF_SEL_INIT_KEIJYO_YM.Text = WF_KEIJYO_YM.Text
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008 SELECT OIT0019_KEIJYOYM")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008 SELECT OIT0019_KEIJYOYM"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 営業所ボタン設定(対象全営業所コード取得)
    ''' </summary>
    Protected Sub GetAllOffice()

        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     OFFICECODE")
        SQLStrBldr.AppendLine("     , OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.VIW0015_COSTMANAGE_OFFICE")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE = 'ALL'")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     SORTORDER")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    'SQL実行
                    OIM0002tbl = New DataTable()
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        OIM0002tbl.Load(SQLdr)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008 GetOffice")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008 GetOffice"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 営業所ボタン設定(各営業所ボタンの押下可・不可設定)
    ''' </summary>
    Protected Sub SetOfficeAuth()

        'ユーザーの所属組織で選択可能な営業所を取得する
        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     OFFICECODE")
        SQLStrBldr.AppendLine("     , OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.VIW0015_COSTMANAGE_OFFICE")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE = @P01")
        SQLStrBldr.AppendLine(" ORDER BY")
        SQLStrBldr.AppendLine("     SORTORDER")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                    PARA1.Value = Master.USER_ORG

                    'SQL実行
                    OIM0002tbl = New DataTable()
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        OIM0002tbl.Load(SQLdr)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008 SetOfficeAuth")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0008 SetOfficeAuth"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
            Exit Sub
        End Try

        '営業所ボタンをすべて利用不可に初期化する
        WF_OFFICEBTN_1.Enabled = False
        WF_OFFICEBTN_2.Enabled = False
        WF_OFFICEBTN_3.Enabled = False
        WF_OFFICEBTN_4.Enabled = False
        WF_OFFICEBTN_5.Enabled = False
        WF_OFFICEBTN_6.Enabled = False
        WF_OFFICEBTN_7.Enabled = False
        WF_OFFICEBTN_8.Enabled = False
        WF_OFFICEBTN_9.Enabled = False
        WF_OFFICEBTN_10.Enabled = False
        WF_OFFICEBTN_11.Enabled = False

        '選択可能な営業所に対応するボタンのみ利用可とする
        Dim InitSelectHdnId As String = ""
        For Each row As DataRow In OIM0002tbl.Rows
            If WF_OFFICEHDN_1.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_1.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_1.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_2.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_2.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_2.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_3.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_3.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_3.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_4.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_4.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_4.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_5.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_5.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_5.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_6.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_6.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_6.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_7.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_7.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_7.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_8.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_8.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_8.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_9.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_9.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_9.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_10.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_10.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_10.ID
                End If
                Continue For
            End If
            If WF_OFFICEHDN_11.Value = row("OFFICECODE") Then
                WF_OFFICEBTN_11.Enabled = True
                If String.IsNullOrEmpty(InitSelectHdnId) Then
                    InitSelectHdnId = WF_OFFICEHDN_11.ID
                End If
                Continue For
            End If
        Next

        '最初に利用可となった営業所を初期選択とする
        WF_OFFICEHDN_ID.Value = InitSelectHdnId

    End Sub

    ''' <summary>
    ''' 費用管理ワークテーブルデータ取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続設定</param>
    ''' <returns></returns>
    Protected Function GetTempTable(ByVal SQLcon As SqlConnection) As DataTable

        Dim retDt As DataTable = Nothing
        Dim SelSQLBldr As New StringBuilder()
        SelSQLBldr.AppendLine(" SELECT")
        SelSQLBldr.AppendLine("     OFFICECODE")
        SelSQLBldr.AppendLine("     , KEIJYOYM")
        SelSQLBldr.AppendLine("     , LINE")
        SelSQLBldr.AppendLine("     , CHECKFLG")
        SelSQLBldr.AppendLine("     , CALCACCOUNT")
        SelSQLBldr.AppendLine("     , ACCOUNTCODE")
        SelSQLBldr.AppendLine("     , ACCOUNTNAME")
        SelSQLBldr.AppendLine("     , SEGMENTCODE")
        SelSQLBldr.AppendLine("     , SEGMENTNAME")
        SelSQLBldr.AppendLine("     , SEGMENTBRANCHCODE")
        SelSQLBldr.AppendLine("     , SEGMENTBRANCHNAME")
        SelSQLBldr.AppendLine("     , SHIPPERSCODE")
        SelSQLBldr.AppendLine("     , SHIPPERSNAME")
        SelSQLBldr.AppendLine("     , QUANTITY")
        SelSQLBldr.AppendLine("     , AMOUNT")
        SelSQLBldr.AppendLine("     , TAX")
        SelSQLBldr.AppendLine("     , INVOICECODE")
        SelSQLBldr.AppendLine("     , INVOICENAME")
        SelSQLBldr.AppendLine("     , INVOICEDEPTNAME")
        SelSQLBldr.AppendLine("     , PAYEECODE")
        SelSQLBldr.AppendLine("     , PAYEENAME")
        SelSQLBldr.AppendLine("     , PAYEEDEPTNAME")
        SelSQLBldr.AppendLine("     , TEKIYOU")
        SelSQLBldr.AppendLine(" FROM")
        SelSQLBldr.AppendLine("     [oil].TMP0008_COST")
        SelSQLBldr.AppendLine(" WHERE")
        SelSQLBldr.AppendLine("     OFFICECODE = @P01")
        SelSQLBldr.AppendLine(" AND KEIJYOYM = @P02")
        SelSQLBldr.AppendLine(" ORDER BY")
        SelSQLBldr.AppendLine("     LINE")

        Try
            '費用管理明細ワークテーブルからローカルテーブルへインポート
            Using SelCmd As New SqlCommand(SelSQLBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = SelCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = SelCmd.Parameters.Add("@P02", SqlDbType.DateTime)

                PARA1.Value = WW_OFFICECODE
                Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                PARA2.Value = WK_DATE

                Using SQLdr As SqlDataReader = SelCmd.ExecuteReader()
                    retDt = New DataTable

                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        retDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    retDt.Clear()
                    retDt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M TMP0008_COST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M TMP0008_COST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
        End Try

        Return retDt

    End Function

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
    ''' 小計テーブル生成
    ''' </summary>
    Protected Sub CreateSubTotalTable()

        '小計行テーブル生成
        OIT0008SubTotaltbl = New DataTable()
        '小計の集計キー：勘定科目コード/セグメント/セグメント枝番/請求先コード/支払先コード
        OIT0008SubTotaltbl.Columns.Add("SHIPPERSCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SHIPPERSNAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("ACCOUNTCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("ACCOUNTNAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTNAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTBRANCHCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTBRANCHNAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("INVOICECODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("INVOICENAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("INVOICEDEPTNAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEECODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEENAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEEDEPTNAME", Type.GetType("System.String"))
        '集計対象：金額/税額
        OIT0008SubTotaltbl.Columns.Add("AMOUNT", Type.GetType("System.Int32"))
        OIT0008SubTotaltbl.Columns.Add("TAX", Type.GetType("System.Int32"))

        '小計行テーブルデータ生成
        For Each row As DataRow In OIT0008tbl.Rows

            Dim dataFound As Boolean = False

            '勘定科目コード/セグメント/セグメント枝番のいずれか
            '又は請求先/ 支払先コードの両方が未設定の場合は無視
            If String.IsNullOrEmpty(row("ACCOUNTCODE")) OrElse
                String.IsNullOrEmpty(row("SEGMENTCODE")) OrElse
                String.IsNullOrEmpty(row("SEGMENTBRANCHCODE")) OrElse
                (String.IsNullOrEmpty(row("INVOICECODE")) AndAlso String.IsNullOrEmpty(row("PAYEECODE"))) Then
                Continue For
            End If

            For Each strow As DataRow In OIT0008SubTotaltbl.Rows
                '勘定科目コード/セグメント/請求先コード/支払先コードが一致する行が存在する場合
                '金額、税額をそれぞれ加算
                If row("SHIPPERSCODE") = strow("SHIPPERSCODE") AndAlso
                    row("ACCOUNTCODE") = strow("ACCOUNTCODE") AndAlso
                    row("SEGMENTCODE") = strow("SEGMENTCODE") AndAlso
                    row("SEGMENTBRANCHCODE") = strow("SEGMENTBRANCHCODE") AndAlso
                    row("INVOICECODE") = strow("INVOICECODE") AndAlso
                    row("PAYEECODE") = strow("PAYEECODE") Then

                    If Not row("AMOUNT") Is DBNull.Value Then
                        strow("AMOUNT") += row("AMOUNT")
                    End If
                    If Not row("TAX") Is DBNull.Value Then
                        strow("TAX") += row("TAX")
                    End If
                    dataFound = True
                    Exit For
                End If
            Next
            '一致行が存在しない場合はレコードを追加
            If Not dataFound Then

                Dim strow As DataRow = OIT0008SubTotaltbl.NewRow()

                strow("SHIPPERSCODE") = row("SHIPPERSCODE")
                strow("SHIPPERSNAME") = row("SHIPPERSNAME")
                strow("ACCOUNTCODE") = row("ACCOUNTCODE")
                strow("ACCOUNTNAME") = row("ACCOUNTNAME")
                strow("SEGMENTCODE") = row("SEGMENTCODE")
                strow("SEGMENTNAME") = row("SEGMENTNAME")
                strow("SEGMENTBRANCHCODE") = row("SEGMENTBRANCHCODE")
                strow("SEGMENTBRANCHNAME") = row("SEGMENTBRANCHNAME")

                '勘定科目/セグメント/セグメント枝番名称取得
                Dim WK_CODE As String = strow("ACCOUNTCODE") & " " & strow("SEGMENTCODE") & " " & strow("SEGMENTBRANCHCODE")
                Dim WK_NAME As String = ""
                CODENAME_get("ACCOUNTPATTERN", WK_CODE, WK_NAME, WW_RTN_SW)
                If Not String.IsNullOrEmpty(WK_NAME) Then
                    Dim names = ConvertAccountPatternName(WK_NAME)
                    If names.Length > 0 Then strow("ACCOUNTNAME") = names(0)
                    If names.Length > 1 Then strow("SEGMENTNAME") = names(1)
                    If names.Length > 2 Then strow("SEGMENTBRANCHNAME") = names(2)
                End If

                strow("INVOICECODE") = row("INVOICECODE")
                strow("INVOICENAME") = row("INVOICENAME")
                strow("INVOICEDEPTNAME") = row("INVOICEDEPTNAME")
                strow("PAYEECODE") = row("PAYEECODE")
                strow("PAYEENAME") = row("PAYEENAME")
                strow("PAYEEDEPTNAME") = row("PAYEEDEPTNAME")

                strow("AMOUNT") = row("AMOUNT")
                If strow("AMOUNT") Is DBNull.Value Then strow("AMOUNT") = 0
                strow("TAX") = row("TAX")
                If strow("TAX") Is DBNull.Value Then strow("TAX") = 0

                OIT0008SubTotaltbl.Rows.Add(strow)
            End If
        Next

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewSetup(ByVal SQLcon As SqlConnection)

        '費用管理ワークテーブルからのデータ取得
        OIT0008tbl = GetTempTable(SQLcon)

        '小計テーブルの作成
        CreateSubTotalTable()

        'GridViewへのデータバインド
        WF_COSTLISTTBL.DataSource = OIT0008tbl
        WF_COSTLISTTBL.DataBind()

    End Sub

    Protected Function GetCheckVal(ByRef val As Integer) As Boolean

        If val = 1 Then
            Return True
        End If

        Return False
    End Function

    Protected Function GetCalcAccountVal(ByRef val As String) As Boolean

        If val = "1" Then
            Return True
        End If

        Return False
    End Function

    Protected Function GetCalcAccountValAndEditable(ByRef val As String) As Boolean

        If val = "1" OrElse Not WW_EDITABLEFLG Then
            Return True
        End If

        Return False
    End Function

    Protected Function GetEnabled(ByRef val As String) As Boolean

        If val = "2" AndAlso WW_EDITABLEFLG Then
            Return True
        End If

        Return False
    End Function

    Protected Function GetEditable() As Boolean

        If Not WW_EDITABLEFLG Then
            Return True
        End If

        Return False
    End Function

    Protected Function GetAccountCodeStyle(ByRef val As String) As String

        Dim cssStyle As String = "WF_TEXTBOX_CSS boxIcon"

        If val = "2" AndAlso WW_EDITABLEFLG Then
            cssStyle += " iconOnly"
        End If

        Return cssStyle
    End Function

    ''' <summary>
    ''' 費用管理ワークテーブルへのデータ設定
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub SetTempTable(ByVal SQLcon As SqlConnection)

        '費用管理ワークテーブルへの格納①
        '受注費用明細テーブルの集計レコード
        '(集計キー:営業所コード、計上年月、勘定科目コード、セグメント、セグメント枝番、荷主コード、請求先コード、支払先コード)
        'と、同一キーで費用管理テーブルにレコードがあれば摘要を取得して格納する
        Dim InsBldr As New StringBuilder
        InsBldr.AppendLine(" INSERT INTO [oil].TMP0008_COST")
        InsBldr.AppendLine(" SELECT")
        InsBldr.AppendLine("     TMP.OFFICECODE")
        InsBldr.AppendLine("     , TMP.KEIJYOYM")
        InsBldr.AppendLine("     , ROW_NUMBER() OVER(")
        InsBldr.AppendLine("           ORDER BY")
        InsBldr.AppendLine("               TMP.OFFICECODE")
        InsBldr.AppendLine("               , TMP.SHIPPERSCODE")
        InsBldr.AppendLine("               , TMP.ACCOUNTCODE")
        InsBldr.AppendLine("               , TMP.SEGMENTCODE")
        InsBldr.AppendLine("               , TMP.SEGMENTBRANCHCODE")
        InsBldr.AppendLine("               , TMP.INVOICECODE")
        InsBldr.AppendLine("               , TMP.PAYEECODE")
        InsBldr.AppendLine("       ) AS LINE")
        InsBldr.AppendLine("     , 0 AS CHECKFLG")
        InsBldr.AppendLine("     , TMP.CALCACCOUNT")
        InsBldr.AppendLine("     , TMP.ACCOUNTCODE")
        InsBldr.AppendLine("     , TMP.ACCOUNTNAME")
        InsBldr.AppendLine("     , TMP.SEGMENTCODE")
        InsBldr.AppendLine("     , TMP.SEGMENTNAME")
        InsBldr.AppendLine("     , TMP.SEGMENTBRANCHCODE")
        InsBldr.AppendLine("     , TMP.SEGMENTBRANCHNAME")
        InsBldr.AppendLine("     , TMP.SHIPPERSCODE")
        InsBldr.AppendLine("     , TMP.SHIPPERSNAME")
        InsBldr.AppendLine("     , TMP.QUANTITY")
        InsBldr.AppendLine("     , 0.0 AS UNITPRICE")
        InsBldr.AppendLine("     , TMP.AMOUNT")
        InsBldr.AppendLine("     , TMP.TAX")
        InsBldr.AppendLine("     , TMP.INVOICECODE")
        InsBldr.AppendLine("     , TMP.INVOICENAME")
        InsBldr.AppendLine("     , TMP.INVOICEDEPTNAME")
        InsBldr.AppendLine("     , TMP.PAYEECODE")
        InsBldr.AppendLine("     , TMP.PAYEENAME")
        InsBldr.AppendLine("     , TMP.PAYEEDEPTNAME")
        InsBldr.AppendLine("     , ISNULL(RTRIM(OIT0018.TEKIYOU), '') AS TEKIYOU")
        InsBldr.AppendLine(" FROM (")
        InsBldr.AppendLine("     SELECT")
        InsBldr.AppendLine("         OFFICECODE")
        InsBldr.AppendLine("         , @P02 AS KEIJYOYM")
        InsBldr.AppendLine("         , '1' AS CALCACCOUNT")
        InsBldr.AppendLine("         , ACCOUNTCODE")
        InsBldr.AppendLine("         , ACCOUNTNAME")
        InsBldr.AppendLine("         , SEGMENTCODE")
        InsBldr.AppendLine("         , SEGMENTNAME")
        InsBldr.AppendLine("         , BREAKDOWNCODE AS SEGMENTBRANCHCODE")
        InsBldr.AppendLine("         , BREAKDOWN AS SEGMENTBRANCHNAME")
        InsBldr.AppendLine("         , SHIPPERSCODE")
        InsBldr.AppendLine("         , SHIPPERSNAME")
        InsBldr.AppendLine("         , SUM(CARSAMOUNT) AS QUANTITY")
        InsBldr.AppendLine("         , SUM(APPLYCHARGE) AS AMOUNT")
        InsBldr.AppendLine("         , SUM(FLOOR(APPLYCHARGE * 0.10)) AS TAX")
        InsBldr.AppendLine("         , INVOICECODE")
        InsBldr.AppendLine("         , INVOICENAME")
        InsBldr.AppendLine("         , INVOICEDEPTNAME")
        InsBldr.AppendLine("         , PAYEECODE")
        InsBldr.AppendLine("         , PAYEENAME")
        InsBldr.AppendLine("         , PAYEEDEPTNAME")
        InsBldr.AppendLine("     FROM")
        InsBldr.AppendLine("         [oil].OIT0013_ORDERDETAILBILLING")
        InsBldr.AppendLine("     WHERE")
        InsBldr.AppendLine("         DELFLG <> '1'")
        InsBldr.AppendLine("     AND OFFICECODE = @P01")
        InsBldr.AppendLine("     AND KEIJYOYMD BETWEEN @P02 AND @P03")
        InsBldr.AppendLine("     GROUP BY")
        InsBldr.AppendLine("         OFFICECODE")
        InsBldr.AppendLine("         , ACCOUNTCODE")
        InsBldr.AppendLine("         , ACCOUNTNAME")
        InsBldr.AppendLine("         , SEGMENTCODE")
        InsBldr.AppendLine("         , SEGMENTNAME")
        InsBldr.AppendLine("         , BREAKDOWNCODE")
        InsBldr.AppendLine("         , BREAKDOWN")
        InsBldr.AppendLine("         , SHIPPERSCODE")
        InsBldr.AppendLine("         , SHIPPERSNAME")
        InsBldr.AppendLine("         , INVOICECODE")
        InsBldr.AppendLine("         , INVOICENAME")
        InsBldr.AppendLine("         , INVOICEDEPTNAME")
        InsBldr.AppendLine("         , PAYEECODE")
        InsBldr.AppendLine("         , PAYEENAME")
        InsBldr.AppendLine("         , PAYEEDEPTNAME")
        InsBldr.AppendLine(" ) TMP")
        InsBldr.AppendLine(" LEFT OUTER JOIN [oil].OIT0018_COST OIT0018")
        InsBldr.AppendLine("     ON  TMP.OFFICECODE = OIT0018.OFFICECODE")
        InsBldr.AppendLine("     AND TMP.KEIJYOYM = OIT0018.KEIJYOYM")
        InsBldr.AppendLine("     AND TMP.CALCACCOUNT = OIT0018.CALCACCOUNT")
        InsBldr.AppendLine("     AND TMP.ACCOUNTCODE = OIT0018.ACCOUNTCODE")
        InsBldr.AppendLine("     AND TMP.ACCOUNTNAME = OIT0018.ACCOUNTNAME")
        InsBldr.AppendLine("     AND TMP.SEGMENTCODE = OIT0018.SEGMENTCODE")
        InsBldr.AppendLine("     AND TMP.SEGMENTNAME = OIT0018.SEGMENTNAME")
        InsBldr.AppendLine("     AND TMP.SEGMENTBRANCHCODE = OIT0018.SEGMENTBRANCHCODE")
        InsBldr.AppendLine("     AND TMP.SEGMENTBRANCHNAME = OIT0018.SEGMENTBRANCHNAME")
        InsBldr.AppendLine("     AND TMP.SHIPPERSCODE = OIT0018.SHIPPERSCODE")
        InsBldr.AppendLine("     AND TMP.INVOICECODE = OIT0018.INVOICECODE")
        InsBldr.AppendLine("     AND TMP.INVOICENAME = OIT0018.INVOICENAME")
        InsBldr.AppendLine("     AND TMP.INVOICEDEPTNAME = OIT0018.INVOICEDEPTNAME")
        InsBldr.AppendLine("     AND TMP.PAYEECODE = OIT0018.PAYEECODE")
        InsBldr.AppendLine("     AND TMP.PAYEENAME = OIT0018.PAYEENAME")
        InsBldr.AppendLine("     AND TMP.PAYEEDEPTNAME = OIT0018.PAYEEDEPTNAME")
        InsBldr.AppendLine(" ORDER BY")
        InsBldr.AppendLine("     LINE")

        '費用管理ワークテーブルへの格納②
        '費用管理テーブルに、営業コード、計上年月で計算科目＝2:手動入力のレコードがあれば抽出し
        'SEQ番号順に、①で追加したレコード群の後のLINE番号を振り直して格納する
        Dim InsBldr2 As StringBuilder = New StringBuilder
        InsBldr2.AppendLine(" INSERT INTO [oil].TMP0008_COST")
        InsBldr2.AppendLine(" SELECT")
        InsBldr2.AppendLine("     OIT0018.OFFICECODE")
        InsBldr2.AppendLine("     , OIT0018.KEIJYOYM")
        InsBldr2.AppendLine("     , ISNULL((SELECT MAX(LINE) FROM [oil].TMP0008_COST WHERE OFFICECODE = @P01 AND KEIJYOYM = @P02), 0) + ROW_NUMBER() OVER(ORDER BY OIT0018.SEQ) AS LINE")
        InsBldr2.AppendLine("     , 0 AS CHECKFLG")
        InsBldr2.AppendLine("     , OIT0018.CALCACCOUNT")
        InsBldr2.AppendLine("     , OIT0018.ACCOUNTCODE")
        InsBldr2.AppendLine("     , OIT0018.ACCOUNTNAME")
        InsBldr2.AppendLine("     , OIT0018.SEGMENTCODE")
        InsBldr2.AppendLine("     , OIT0018.SEGMENTNAME")
        InsBldr2.AppendLine("     , OIT0018.SEGMENTBRANCHCODE")
        InsBldr2.AppendLine("     , OIT0018.SEGMENTBRANCHNAME")
        InsBldr2.AppendLine("     , OIT0018.SHIPPERSCODE")
        InsBldr2.AppendLine("     , OIT0018.SHIPPERSNAME")
        InsBldr2.AppendLine("     , OIT0018.QUANTITY")
        InsBldr2.AppendLine("     , OIT0018.UNITPRICE")
        InsBldr2.AppendLine("     , OIT0018.AMOUNT")
        InsBldr2.AppendLine("     , OIT0018.TAX")
        InsBldr2.AppendLine("     , OIT0018.INVOICECODE")
        InsBldr2.AppendLine("     , OIT0018.INVOICENAME")
        InsBldr2.AppendLine("     , OIT0018.INVOICEDEPTNAME")
        InsBldr2.AppendLine("     , OIT0018.PAYEECODE")
        InsBldr2.AppendLine("     , OIT0018.PAYEENAME")
        InsBldr2.AppendLine("     , OIT0018.PAYEEDEPTNAME")
        InsBldr2.AppendLine("     , OIT0018.TEKIYOU")
        InsBldr2.AppendLine(" FROM")
        InsBldr2.AppendLine("     [oil].OIT0018_COST AS OIT0018")
        InsBldr2.AppendLine(" WHERE")
        InsBldr2.AppendLine("     OIT0018.CALCACCOUNT = '2'")
        InsBldr2.AppendLine(" AND OIT0018.OFFICECODE = @P01")
        InsBldr2.AppendLine(" AND OIT0018.KEIJYOYM = @P02")
        InsBldr2.AppendLine(" ORDER BY")
        InsBldr2.AppendLine("     OIT0018.SEQ")

        Try
            Using InsCmd As New SqlCommand(InsBldr.ToString(), SQLcon), InsCmd2 As New SqlCommand(InsBldr2.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = InsCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = InsCmd.Parameters.Add("@P02", SqlDbType.DateTime)
                Dim PARA3 As SqlParameter = InsCmd.Parameters.Add("@P03", SqlDbType.DateTime)

                Dim PARA2_1 As SqlParameter = InsCmd2.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim PARA2_2 As SqlParameter = InsCmd2.Parameters.Add("@P02", SqlDbType.DateTime)

                '費用管理ワークテーブルのデータ生成
                PARA1.Value = WW_OFFICECODE
                Dim WK_STYMD = DateTime.Parse(WW_KEIJYO_YM + "/01")
                Dim WK_ENDYMD = New DateTime(WK_STYMD.Year, WK_STYMD.Month, DateTime.DaysInMonth(WK_STYMD.Year, WK_STYMD.Month))
                PARA2.Value = WK_STYMD
                PARA3.Value = WK_ENDYMD
                InsCmd.CommandTimeout = 300
                InsCmd.ExecuteNonQuery()

                PARA2_1.Value = WW_OFFICECODE
                PARA2_2.Value = WK_STYMD
                InsCmd2.CommandTimeout = 300
                InsCmd2.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M TMP0008_COST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M TMP0008_COST INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' ワークテーブルの初期化
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InitTempTable(ByVal SQLcon As SqlConnection)

        Dim DelBldr As New StringBuilder
        DelBldr.AppendLine(" DELETE FROM [oil].TMP0008_COST")
        DelBldr.AppendLine(" WHERE")
        DelBldr.AppendLine("     OFFICECODE = @P1")
        DelBldr.AppendLine(" AND KEIJYOYM = @P2")

        Try
            Using DelCmd As New SqlCommand(DelBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = DelCmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = DelCmd.Parameters.Add("@P2", SqlDbType.DateTime)

                '費用管理ワークテーブルの初期化
                PARA1.Value = WW_OFFICECODE
                Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                PARA2.Value = WK_DATE
                DelCmd.CommandTimeout = 300
                DelCmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M InitTempTable")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M InitTempTable"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

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

            '費用管理テーブルから同一営業所、計上年月のレコードをいったん削除する
            InitCostTable(SQLcon)

            '削除エラーの場合は処理を中断
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

            'ワークテーブルから費用管理テーブルへデータを移送する
            SetCostTable(SQLcon)
        End Using

        '正常終了の場合はメッセージを表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        End If
    End Sub

    ''' <summary>
    ''' 費用管理テーブルの初期化
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InitCostTable(ByVal SQLcon As SqlConnection)

        Dim DelBldr As New StringBuilder
        DelBldr.AppendLine(" DELETE FROM [oil].OIT0018_COST")
        DelBldr.AppendLine(" WHERE")
        DelBldr.AppendLine("     OFFICECODE = @P1")
        DelBldr.AppendLine(" AND KEIJYOYM = @P2")

        Try
            Using DelCmd As New SqlCommand(DelBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = DelCmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = DelCmd.Parameters.Add("@P2", SqlDbType.DateTime)

                '費用管理テーブルの初期化
                PARA1.Value = WW_OFFICECODE
                Dim WK_DATE = DateTime.Parse(WW_KEIJYO_YM + "/01")
                PARA2.Value = WK_DATE
                DelCmd.CommandTimeout = 300
                DelCmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M DELETE OIT0018_COST")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M DELETE OIT0018_COST"
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
    ''' 費用管理テーブルへのデータ保存
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub SetCostTable(ByVal SQLcon As SqlConnection)

        'ワークテーブルから費用管理明細テーブルへの移送
        Dim InsBldr As New StringBuilder
        InsBldr.AppendLine(" INSERT INTO [oil].OIT0018_COST(")
        InsBldr.AppendLine("     OFFICECODE")
        InsBldr.AppendLine("     , KEIJYOYM")
        InsBldr.AppendLine("     , SEQ")
        InsBldr.AppendLine("     , CALCACCOUNT")
        InsBldr.AppendLine("     , ACCOUNTCODE")
        InsBldr.AppendLine("     , ACCOUNTNAME")
        InsBldr.AppendLine("     , SEGMENTCODE")
        InsBldr.AppendLine("     , SEGMENTNAME")
        InsBldr.AppendLine("     , SEGMENTBRANCHCODE")
        InsBldr.AppendLine("     , SEGMENTBRANCHNAME")
        InsBldr.AppendLine("     , SHIPPERSCODE")
        InsBldr.AppendLine("     , SHIPPERSNAME")
        InsBldr.AppendLine("     , QUANTITY")
        InsBldr.AppendLine("     , UNITPRICE")
        InsBldr.AppendLine("     , AMOUNT")
        InsBldr.AppendLine("     , TAX")
        InsBldr.AppendLine("     , INVOICECODE")
        InsBldr.AppendLine("     , INVOICENAME")
        InsBldr.AppendLine("     , INVOICEDEPTNAME")
        InsBldr.AppendLine("     , PAYEECODE")
        InsBldr.AppendLine("     , PAYEENAME")
        InsBldr.AppendLine("     , PAYEEDEPTNAME")
        InsBldr.AppendLine("     , TEKIYOU")
        InsBldr.AppendLine("     , INITYMD")
        InsBldr.AppendLine("     , INITUSER")
        InsBldr.AppendLine("     , INITTERMID")
        InsBldr.AppendLine("     , UPDYMD")
        InsBldr.AppendLine("     , UPDUSER")
        InsBldr.AppendLine("     , UPDTERMID")
        InsBldr.AppendLine("     , RECEIVEYMD")
        InsBldr.AppendLine(" )")
        InsBldr.AppendLine(" SELECT")
        InsBldr.AppendLine("     TMP0008.OFFICECODE")
        InsBldr.AppendLine("     , TMP0008.KEIJYOYM")
        InsBldr.AppendLine("     , ROW_NUMBER() OVER(ORDER BY LINE) AS SEQ")
        InsBldr.AppendLine("     , TMP0008.CALCACCOUNT")
        InsBldr.AppendLine("     , TMP0008.ACCOUNTCODE")
        InsBldr.AppendLine("     , TMP0008.ACCOUNTNAME")
        InsBldr.AppendLine("     , TMP0008.SEGMENTCODE")
        InsBldr.AppendLine("     , TMP0008.SEGMENTNAME")
        InsBldr.AppendLine("     , TMP0008.SEGMENTBRANCHCODE")
        InsBldr.AppendLine("     , TMP0008.SEGMENTBRANCHNAME")
        InsBldr.AppendLine("     , TMP0008.SHIPPERSCODE")
        InsBldr.AppendLine("     , TMP0008.SHIPPERSNAME")
        InsBldr.AppendLine("     , TMP0008.QUANTITY")
        InsBldr.AppendLine("     , TMP0008.UNITPRICE")
        InsBldr.AppendLine("     , TMP0008.AMOUNT")
        InsBldr.AppendLine("     , TMP0008.TAX")
        InsBldr.AppendLine("     , TMP0008.INVOICECODE")
        InsBldr.AppendLine("     , TMP0008.INVOICENAME")
        InsBldr.AppendLine("     , TMP0008.INVOICEDEPTNAME")
        InsBldr.AppendLine("     , TMP0008.PAYEECODE")
        InsBldr.AppendLine("     , TMP0008.PAYEENAME")
        InsBldr.AppendLine("     , TMP0008.PAYEEDEPTNAME")
        InsBldr.AppendLine("     , TMP0008.TEKIYOU")
        InsBldr.AppendLine("     , @P03 AS INITYMD")
        InsBldr.AppendLine("     , @P04 AS INITUSER")
        InsBldr.AppendLine("     , @P05 AS INITTERMID")
        InsBldr.AppendLine("     , @P03 AS UPDYMD")
        InsBldr.AppendLine("     , @P04 AS UPDTUSER")
        InsBldr.AppendLine("     , @P05 AS UPDTERMID")
        InsBldr.AppendLine("     , @P06 AS RECEIVEYMD")
        InsBldr.AppendLine(" FROM")
        InsBldr.AppendLine("     [oil].TMP0008_COST TMP0008")
        InsBldr.AppendLine(" WHERE")
        InsBldr.AppendLine("     TMP0008.OFFICECODE = @P01")
        InsBldr.AppendLine(" AND TMP0008.KEIJYOYM = @P02")
        InsBldr.AppendLine(" AND (TMP0008.ACCOUNTCODE       IS NOT NULL AND TMP0008.ACCOUNTCODE <> '')")
        InsBldr.AppendLine(" AND (TMP0008.SEGMENTCODE       IS NOT NULL AND TMP0008.SEGMENTCODE <> '')")
        InsBldr.AppendLine(" AND (TMP0008.SEGMENTBRANCHCODE IS NOT NULL AND TMP0008.SEGMENTBRANCHCODE <> '')")
        InsBldr.AppendLine(" AND TMP0008.INVOICECODE        IS NOT NULL")
        InsBldr.AppendLine(" AND TMP0008.PAYEECODE          IS NOT NULL")
        InsBldr.AppendLine(" AND (TMP0008.INVOICECODE <> '' OR TMP0008.PAYEECODE <> '')")
        InsBldr.AppendLine(" AND (TMP0008.SHIPPERSCODE      IS NOT NULL AND")
        InsBldr.AppendLine("         (TMP0008.CALCACCOUNT = '1' AND TMP0008.SHIPPERSCODE <> '') OR")
        InsBldr.AppendLine("         (TMP0008.CALCACCOUNT = '2' AND TMP0008.SHIPPERSCODE =  '')")
        InsBldr.AppendLine(" )")
        InsBldr.AppendLine(" ORDER BY")
        InsBldr.AppendLine("     TMP0008.LINE")

        Try
            Using InsCmd As New SqlCommand(InsBldr.ToString(), SQLcon)
                Dim PARA1 As SqlParameter = InsCmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)
                Dim PARA2 As SqlParameter = InsCmd.Parameters.Add("@P02", SqlDbType.Date)
                Dim PARA3 As SqlParameter = InsCmd.Parameters.Add("@P03", SqlDbType.DateTime)
                Dim PARA4 As SqlParameter = InsCmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)
                Dim PARA5 As SqlParameter = InsCmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20)
                Dim PARA6 As SqlParameter = InsCmd.Parameters.Add("@P06", SqlDbType.DateTime)

                PARA1.Value = WW_OFFICECODE
                Dim WK_DATE = Date.Parse(WW_KEIJYO_YM + "/01")
                PARA2.Value = WK_DATE
                PARA3.Value = DateTime.Now
                PARA4.Value = Master.USERID
                PARA5.Value = Master.USERTERMID
                PARA6.Value = C_DEFAULT_YMD

                InsCmd.CommandTimeout = 300
                InsCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0008M OIT0018_COST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0008M OIT0018_COST INSERT"
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
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        ''○ 帳票出力
        'CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       ' 会社コード
        'CS0030REPORT.PROFID = Master.PROF_REPORT                ' プロファイルID
        'CS0030REPORT.MAPID = Master.MAPID                       ' 画面ID
        'CS0030REPORT.REPORTID = rightview.GetReportId()         ' 帳票ID
        'CS0030REPORT.FILEtyp = "XLSX"                           ' 出力ファイル形式
        'CS0030REPORT.TBLDATA = OIT0008tbl                        ' データ参照  Table
        'CS0030REPORT.CS0030REPORT()

        'If Not isNormal(CS0030REPORT.ERR) Then
        '    If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '    Else
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
        '    End If
        '    Exit Sub
        'End If

        ''○ 別画面でExcelを表示
        'WF_PrintURL.Value = CS0030REPORT.URL
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPrint_Click()

        ''○ 帳票出力
        'CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       ' 会社コード
        'CS0030REPORT.PROFID = Master.PROF_REPORT                ' プロファイルID
        'CS0030REPORT.MAPID = Master.MAPID                       ' 画面ID
        'CS0030REPORT.REPORTID = rightview.GetReportId()         ' 帳票ID
        'CS0030REPORT.FILEtyp = "pdf"                            ' 出力ファイル形式
        'CS0030REPORT.TBLDATA = OIT0008tbl                        ' データ参照Table
        'CS0030REPORT.CS0030REPORT()

        'If Not isNormal(CS0030REPORT.ERR) Then
        '    If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        '    Else
        '        Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
        '    End If
        '    Exit Sub
        'End If

        ''○ 別画面でPDFを表示
        'WF_PrintURL.Value = CS0030REPORT.URL
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    Protected Sub WF_COSTLISTTBL_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles WF_COSTLISTTBL.RowDataBound
        Select Case e.Row.RowType
            Case DataControlRowType.DataRow
                Dim row = DirectCast(e.Row.DataItem, DataRowView)

                If row("ACCOUNTCODE") Is DBNull.Value OrElse String.IsNullOrEmpty(row("ACCOUNTCODE")) Then
                    Exit Sub
                End If

                If row("ACCOUNTCODE").ToString().Substring(0, 1) = "4" Then
                    If Not row("AMOUNT") Is DBNull.Value Then
                        WK_INV_AMOUNT_ALL += row("AMOUNT")
                    End If
                    If Not row("TAX") Is DBNull.Value Then
                        WK_INV_TAX_ALL += row("TAX")
                    End If
                ElseIf row("ACCOUNTCODE").ToString().Substring(0, 1) = "5" Then
                    If Not row("AMOUNT") Is DBNull.Value Then
                        WK_PAY_AMOUNT_ALL += row("AMOUNT")
                    End If
                    If Not row("TAX") Is DBNull.Value Then
                        WK_PAY_TAX_ALL += row("TAX")
                    End If
                End If
        End Select
    End Sub

    Protected Sub WF_COSTLISTTBL_DataBound(sender As Object, e As EventArgs) Handles WF_COSTLISTTBL.DataBound

        'GridView本体を取得
        Dim grid As GridView = CType(sender, GridView)

        '現在のフッター行のクローンを生成
        Dim subTotalRowCnt = OIT0008SubTotaltbl.Rows.Count
        Dim i As Integer
        Dim j As Integer
        For i = 0 To subTotalRowCnt - 1
            Dim footer As GridViewRow = grid.FooterRow
            Dim numCells = footer.Cells.Count

            Dim newRow As New GridViewRow(footer.RowIndex + 1, -1, footer.RowType, footer.RowState)

            ''have to add in the right number of cells
            ''this also copies any styles over from the original footer
            For j = 0 To numCells - 1
                Dim emptyCell As New TableCell
                newRow.Cells.Add(emptyCell)
            Next

            CType(grid.Controls(0), Table).Rows.Add(newRow)
        Next

        i = 0
        For Each gvrow As GridViewRow In CType(grid.Controls(0), Table).Rows

            If gvrow.RowType = DataControlRowType.DataRow Then
                Dim button As Button = DirectCast(gvrow.FindControl("WF_COSTLISTTBL_CALCACCOUNT"), Button)
                If button.Enabled Then
                    button.OnClientClick = "ButtonClick('WF_ButtonShowDetail" & String.Format("{0:000}", gvrow.RowIndex + 1) & "');"
                End If
                Continue For
            ElseIf Not gvrow.RowType = DataControlRowType.Footer Then
                Continue For
            End If
            If i < subTotalRowCnt Then  '小計行
                '「小計」のスタイル設定
                gvrow.Cells(0).CssClass = "footerCells text"
                gvrow.Cells(0).ColumnSpan = 3
                gvrow.Cells(0).Text = "小計"

                gvrow.Cells(1).CssClass = "footerCells noicon"
                gvrow.Cells(1).Text = OIT0008SubTotaltbl.Rows(i)("SHIPPERSNAME")

                gvrow.Cells(2).CssClass = "footerCells withicon"
                gvrow.Cells(2).Text = OIT0008SubTotaltbl.Rows(i)("ACCOUNTCODE")

                gvrow.Cells(3).CssClass = "footerCells noicon"
                gvrow.Cells(3).Text = OIT0008SubTotaltbl.Rows(i)("SEGMENTCODE")

                gvrow.Cells(4).CssClass = "footerCells noicon"
                gvrow.Cells(4).Text = OIT0008SubTotaltbl.Rows(i)("SEGMENTBRANCHNAME")

                gvrow.Cells(5).CssClass = "footerCells money"
                gvrow.Cells(5).Text = String.Format("{0:#,##0}", OIT0008SubTotaltbl.Rows(i)("AMOUNT"))

                gvrow.Cells(6).CssClass = "footerCells money"
                gvrow.Cells(6).Text = String.Format("{0:#,##0}", OIT0008SubTotaltbl.Rows(i)("TAX"))

                gvrow.Cells(7).CssClass = "footerCells withicon"
                gvrow.Cells(7).Text = OIT0008SubTotaltbl.Rows(i)("invoicecode")

                gvrow.Cells(8).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(8).Text = "<span class='inv_pay'>" + OIT0008SubTotaltbl.Rows(i)("INVOICENAME") + "</span>"

                gvrow.Cells(9).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(9).Text = "<span class='inv_pay'>" + OIT0008SubTotaltbl.Rows(i)("INVOICEDEPTNAME") + "</span>"

                gvrow.Cells(10).CssClass = "footerCells withicon"
                gvrow.Cells(10).Text = OIT0008SubTotaltbl.Rows(i)("PAYEECODE")

                gvrow.Cells(11).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(11).Text = "<span class='inv_pay'>" + OIT0008SubTotaltbl.Rows(i)("PAYEENAME") + "</span>"

                gvrow.Cells(12).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(12).Text = "<span class='inv_pay'>" + OIT0008SubTotaltbl.Rows(i)("PAYEEDEPTNAME") + "</span>"

                For j = 13 To gvrow.Cells.Count - 1
                    gvrow.Cells(j).Visible = False
                Next

                i += 1
            Else                        '請求合計
                '「請求合計」のスタイル設定
                gvrow.Cells(0).CssClass = "footerCells text"
                gvrow.Cells(0).ColumnSpan = 7
                gvrow.Cells(0).Text = "請求合計"

                gvrow.Cells(1).CssClass = "footerCells money"
                gvrow.Cells(1).Text = String.Format("{0:#,##0}", WK_INV_AMOUNT_ALL)

                gvrow.Cells(2).CssClass = "footerCells money"
                gvrow.Cells(2).Text = String.Format("{0:#,##0}", WK_INV_TAX_ALL)

                gvrow.Cells(3).CssClass = "footerCells text"
                gvrow.Cells(3).Text = "支払合計"

                gvrow.Cells(4).CssClass = "footerCells money"
                gvrow.Cells(4).Text = String.Format("{0:#,##0}", WK_PAY_AMOUNT_ALL)

                gvrow.Cells(5).CssClass = "footerCells money"
                gvrow.Cells(5).Text = String.Format("{0:#,##0}", WK_PAY_TAX_ALL)

                For j = 6 To gvrow.Cells.Count - 1
                    gvrow.Cells(j).Visible = False
                Next

            End If
        Next

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

            With leftview

                Select Case WF_LeftMViewChange.Value

                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        .WF_Calendar.Text = WF_KEIJYO_YM.Text + "/01"
                        .ActiveCalendar()
                    Case Else
                        Dim prmData As New Hashtable
                        '勘定科目コード/セグメント/セグメント枝番
                        If WF_FIELD.Value.Contains("WF_COSTLISTTBL_ACCOUNTCODE") Then
                            prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTPATTERN")
                        End If
                        '請求/支払先
                        If WF_FIELD.Value.Contains("WF_COSTLISTTBL_INVOICECODE") OrElse
                            WF_FIELD.Value.Contains("WF_COSTLISTTBL_PAYEECODE") Then
                            '取引マスタ検索
                            prmData = work.CreateFIXParam(Master.USERCAMP, "TORI_DEPT")
                        End If

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()

                End Select

            End With

        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        WW_RTN_SW = C_MESSAGE_NO.NORMAL
        Dim WK_RELOAD_FLG As Boolean = False

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '計上年月
            Case WF_KEIJYO_YM.ID
                '入力チェック
                Try
                    Dim WW_DATE As Date = Date.Parse(WF_KEIJYO_YM.Text + "/01")
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_KEIJYO_YM.Text = "1950/01"
                    Else
                        WF_KEIJYO_YM.Text = WW_DATE.ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                    '型変換エラー
                    WW_RTN_SW = C_MESSAGE_NO.CAST_FORMAT_ERROR
                    '入力値変換エラーの場合は前回表示年月に戻す
                    WF_KEIJYO_YM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text
                End Try
                '前回までの年月と異なる場合は、初期化リロード
                If Not WF_KEIJYO_YM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text Then
                    WK_RELOAD_FLG = True
                End If
                'フォーカスセット
                WF_KEIJYO_YM.Focus()

            Case Else

                Dim rowIdx As Integer = 0
                Dim WK_TextBox As TextBox = Nothing
                Dim WK_Label As Label = Nothing
                Dim WK_CODE As String = ""
                Dim WK_NAME As String = ""
                '請求先コード
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_INVOICECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                    '請求先コードを取得する
                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE") IsNot Nothing Then
                        WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox)
                        WK_CODE = WK_TextBox.Text
                    End If
                    '請求先コードが空の場合、請求先名、請求先部門もクリアする
                    If String.IsNullOrEmpty(WK_CODE) Then
                        '請求先名
                        If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME") IsNot Nothing Then
                            WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME"), Label)
                            WK_Label.Text = ""
                        End If
                        '請求先部門
                        If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME") IsNot Nothing Then
                            WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME"), Label)
                            WK_Label.Text = ""
                        End If
                    Else
                        '取引先部門名称を取得
                        CODENAME_get("TORI_DEPT", WK_CODE, WK_NAME, WW_RTN_SW)
                        '取得できた場合、取引先名、部門名をそれぞれ請求先名、請求先部門に設定
                        If Not String.IsNullOrEmpty(WK_NAME) Then
                            Dim WK_TORI_DEPT_NAMES = WK_NAME.Split(" ")
                            '請求先名
                            If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME") IsNot Nothing Then
                                WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME"), Label)
                                If WK_TORI_DEPT_NAMES.Length > 0 Then
                                    WK_Label.Text = WK_TORI_DEPT_NAMES(0)
                                Else
                                    WK_Label.Text = WK_NAME
                                End If
                            End If
                            '請求先部門
                            If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME") IsNot Nothing Then
                                WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME"), Label)
                                If WK_TORI_DEPT_NAMES.Length > 1 Then
                                    WK_Label.Text = WK_TORI_DEPT_NAMES(1)
                                End If
                            End If
                        End If
                    End If

                End If
                '支払先コード
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_PAYEECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                    '支払先コードを取得する
                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE") IsNot Nothing Then
                        WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox)
                        WK_CODE = WK_TextBox.Text
                    End If
                    '支払先コードが空の場合、支払先名、支払先部門もクリアする
                    If String.IsNullOrEmpty(WK_CODE) Then
                        '支払先名
                        If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME") IsNot Nothing Then
                            WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME"), Label)
                            WK_Label.Text = ""
                        End If
                        '支払先部門
                        If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME") IsNot Nothing Then
                            WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME"), Label)
                            WK_Label.Text = ""
                        End If
                    Else
                        '取引先部門名称を取得
                        CODENAME_get("TORI_DEPT", WK_CODE, WK_NAME, WW_RTN_SW)
                        '取得できた場合、取引先名、部門名をそれぞれ支払先名、支払先部門に設定
                        If Not String.IsNullOrEmpty(WK_NAME) Then
                            Dim WK_TORI_DEPT_NAMES = WK_NAME.Split(" ")
                            '支払先名
                            If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME") IsNot Nothing Then
                                WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME"), Label)
                                If WK_TORI_DEPT_NAMES.Length > 0 Then
                                    WK_Label.Text = WK_TORI_DEPT_NAMES(0)
                                Else
                                    WK_Label.Text = WK_NAME
                                End If
                            End If
                            '支払先部門
                            If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME") IsNot Nothing Then
                                WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME"), Label)
                                If WK_TORI_DEPT_NAMES.Length > 1 Then
                                    WK_Label.Text = WK_TORI_DEPT_NAMES(1)
                                End If
                            End If
                        End If
                    End If

                End If

                'ワークテーブルへのデータ反映
                SetGridViewToTempTable()
        End Select

        'GridViewリロード
        WF_Grid_RELOAD(WK_RELOAD_FLG)

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

            Case WF_KEIJYO_YM.ID
                '計上年月
                Dim WW_DATE As Date
                Try
                    WW_DATE = Date.Parse(leftview.WF_Calendar.Text)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_KEIJYO_YM.Text = "1950/01"
                    Else
                        WF_KEIJYO_YM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                    '型変換エラー
                    WW_RTN_SW = C_MESSAGE_NO.CAST_FORMAT_ERROR
                    '入力値変換エラーの場合は前回表示年月に戻す
                    WF_KEIJYO_YM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text
                End Try
                '前回までの年月と異なる場合は、初期化リロード
                If Not WF_KEIJYO_YM.Text = work.WF_SEL_LAST_KEIJYO_YM.Text Then
                    WK_RELOAD_FLG = True
                End If
                'フォーカスセット
                WF_KEIJYO_YM.Focus()

            Case Else
                Dim rowIdx As Integer
                Dim WK_TextBox As TextBox = Nothing
                Dim WK_Label As Label = Nothing
                Dim WK_Hidden As HiddenField = Nothing

                '勘定科目コード/セグメント/セグメント枝番
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_ACCOUNTCODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)

                    Dim patternCodes = WW_SelectValue.Split(" ")
                    Dim patternNames = ConvertAccountPatternName(WW_SelectText)

                    '勘定科目コード
                    WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTCODE"), TextBox)
                    If patternCodes.Length > 0 Then
                        WK_TextBox.Text = patternCodes(0)
                    End If

                    '勘定科目名
                    WK_Hidden = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTNAME"), HiddenField)
                    If patternNames.Length > 0 Then
                        WK_Hidden.Value = patternNames(0)
                    End If

                    'セグメント
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTCODE"), Label)
                    If patternCodes.Length > 1 Then
                        WK_Label.Text = patternCodes(1)
                    End If

                    'セグメント名
                    WK_Hidden = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTNAME"), HiddenField)
                    If patternNames.Length > 1 Then
                        WK_Hidden.Value = patternNames(1)
                    End If

                    'セグメント枝番
                    WK_Hidden = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE"), HiddenField)
                    If patternCodes.Length > 2 Then
                        WK_Hidden.Value = patternCodes(2)
                    End If

                    'セグメント枝番名
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTBRANCHNAME"), Label)
                    If patternNames.Length > 2 Then
                        WK_Label.Text = patternNames(2)
                    End If
                End If

                '請求先
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_INVOICECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                    Dim WK_TORI_DEPAT_TEXT = WW_SelectText.Split(" ")

                    '請求先コード
                    WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox)
                    WK_TextBox.Text = WW_SelectValue

                    '請求先名
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICENAME"), Label)
                    If WK_TORI_DEPAT_TEXT.Length > 0 Then
                        WK_Label.Text = WK_TORI_DEPAT_TEXT(0)
                    Else
                        WK_Label.Text = WW_SelectText
                    End If

                    '請求先部門
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICEDEPTNAME"), Label)
                    If WK_TORI_DEPAT_TEXT.Length > 1 Then
                        WK_Label.Text = WK_TORI_DEPAT_TEXT(1)
                    End If
                End If

                '支払先
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_PAYEECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                    Dim WK_TORI_DEPAT_TEXT = WW_SelectText.Split(" ")

                    '支払先コード
                    WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox)
                    WK_TextBox.Text = WW_SelectValue

                    '支払先名
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEENAME"), Label)
                    If WK_TORI_DEPAT_TEXT.Length > 0 Then
                        WK_Label.Text = WK_TORI_DEPAT_TEXT(0)
                    Else
                        WK_Label.Text = WW_SelectText
                    End If

                    '支払先部門
                    WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEEDEPTNAME"), Label)
                    If WK_TORI_DEPAT_TEXT.Length > 1 Then
                        WK_Label.Text = WK_TORI_DEPAT_TEXT(1)
                    End If
                End If

                'ワークテーブルへのデータ反映
                SetGridViewToTempTable()

        End Select

        'GridViewリロード
        WF_Grid_RELOAD(WK_RELOAD_FLG)

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
            Case WF_KEIJYO_YM.ID
                WF_KEIJYO_YM.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIT0008row As DataRow In OIT0008tbl.Rows
            Select Case OIT0008row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0008tbl)

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
        For Each OIT0008INProw As DataRow In OIT0008INPtbl.Rows

            WW_LINE_ERR = ""

            '摘要（バリデーションチェック）
            WW_TEXT = OIT0008INProw("TEKIYOU")
            Master.CheckField(Master.USERCAMP, "TEKIYOU", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(摘要エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '計算科目=1:自動計算は摘要以外スキップ
            If OIT0008INProw("CALCACCOUNT") = "1" Then
                Continue For
            End If

            '勘定科目コード
            WW_TEXT = OIT0008INProw("ACCOUNTCODE")
            Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(勘定科目コード)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント
            WW_TEXT = OIT0008INProw("SEGMENTCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'セグメント枝番
            WW_TEXT = OIT0008INProw("SEGMENTBRANCHCODE")
            Master.CheckField(Master.USERCAMP, "SEGMENTBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(セグメント)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '存在チェック(勘定科目マスタ)
            If String.IsNullOrEmpty(WW_LINE_ERR) AndAlso
                Not String.IsNullOrEmpty(OIT0008INProw("ACCOUNTCODE")) AndAlso
                Not String.IsNullOrEmpty(OIT0008INProw("SEGMENTCODE")) AndAlso
                Not String.IsNullOrEmpty(OIT0008INProw("SEGMENTBRANCHCODE")) Then

                Dim WW_CODE As String = OIT0008INProw("ACCOUNTCODE") & " " &
                                        OIT0008INProw("SEGMENTCODE") & " " &
                                        OIT0008INProw("SEGMENTBRANCHCODE")
                CODENAME_get("ACCOUNTPATTERN", WW_CODE, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(勘定科目コード/セグメント/セグメント枝番エラー)です。"
                    WW_CheckMES2 = "勘定科目マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '金額（バリデーションチェック）
            WW_TEXT = OIT0008INProw("AMOUNT")
            Master.CheckField(Master.USERCAMP, "AMOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(金額エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請求先コード（バリデーションチェック）
            WW_TEXT = OIT0008INProw("INVOICECODE")
            Master.CheckField(Master.USERCAMP, "INVOICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("TORI_DEPT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(請求先コード)です。"
                    WW_CheckMES2 = "該当する取引先コードが取引マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請求先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '支払先コード（バリデーションチェック）
            WW_TEXT = OIT0008INProw("PAYEECODE")
            Master.CheckField(Master.USERCAMP, "PAYEECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                CODENAME_get("TORI_DEPT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(支払先コード)です。"
                    WW_CheckMES2 = "該当する取引先コードが取引マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(支払先コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請求先コード/支払先コード共にNull又は空の場合、エラーとする
            If String.IsNullOrEmpty(OIT0008INProw("INVOICECODE")) AndAlso
                String.IsNullOrEmpty(OIT0008INProw("PAYEECODE")) Then
                WW_CheckMES1 = "・更新できないレコード(請求先コード/支払先コード)です。"
                WW_CheckMES2 = "請求先、支払先のどちらかを入力してください。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

        Next

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            ' 年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            ' 月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            ' 月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            ' 日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            ' 閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                ' 月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                ' エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIT0008row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIT0008row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIT0008row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> # =" & OIT0008row("LINE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 勘定科目コード =" & OIT0008row("ACCOUNTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント =" & OIT0008row("SEGMENTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> セグメント枝番 =" & OIT0008row("SEGMENTBRANCHNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 金額 =" & OIT0008row("AMOUNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 税額 =" & OIT0008row("TAX") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先コード =" & OIT0008row("INVOICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先名 =" & OIT0008row("INVOICENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請求先部門 =" & OIT0008row("INVOICEDEPTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先コード =" & OIT0008row("PAYEECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先名 =" & OIT0008row("PAYEENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 支払先部門 =" & OIT0008row("PAYEEDEPTNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 摘要 =" & OIT0008row("TEKIYOU")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

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

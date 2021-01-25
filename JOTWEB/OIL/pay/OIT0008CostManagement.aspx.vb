Imports System.Data.SqlClient
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

    Private Const CONST_DISPROWCOUNT As Integer = 45                ' 1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 ' マウススクロール時稼働行数

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 ' データ追加
    Private Const CONST_UPDATE As String = "Update"                 ' データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         ' 関連チェックエラー

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

    '〇　請求/支払合計計算用
    Private WK_INV_AMMOUNT_ALL As Integer = 0
    Private WK_INV_TAX_ALL As Integer = 0
    Private WK_PAY_AMMOUNT_ALL As Integer = 0
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
                    ''○ 画面表示データ復元
                    'Master.RecoverTable(OIT0008tbl)

                    Select Case WF_ButtonClick.Value
                        'Case "WF_ButtonCSV"             ' ダウンロードボタン押下
                        '    WF_ButtonDownload_Click()
                        'Case "WF_ButtonPrint"           ' 一覧印刷ボタン押下
                        '    WF_ButtonPrint_Click()
                        Case "WF_ButtonEND"             '「戻る」ボタンクリック
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        'Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                        '    WF_FIELD_Change()
                        Case "WF_ButtonSel"             '（左ボックス）選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '（左ボックス）キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '（左ボックス）ダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '（右ボックス）ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '（右ボックス）メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "WF_ButtonRELOAD"          '「表示する」ボタンクリック
                            WF_Grid_RELOAD()
                        Case "WF_ButtonDELETEROW"       '「行削除」ボタンクリック
                            WF_Grid_DeleteRow()
                        Case "WF_ButtonADDROW"          '「行追加」ボタンクリック
                            WF_Grid_AddRow()
                        Case "WF_ButtonUPDATE"          '「保存する」ボタンクリック
                            WF_ButtonUPDATE_Click()
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

    Protected Sub WF_Grid_RELOAD()
        ''【暫定】初期化処理
        'Initialize()

        '選択中の営業所から営業所コードを取得
        Dim selectedHiddenId = DirectCast(Me.Controls(0).FindControl("contents1").FindControl("WF_OFFICEHDN_ID"), HiddenField).Value
        Dim selectedHiddenControl = DirectCast(Me.Controls(0).FindControl("contents1").FindControl(selectedHiddenId), HiddenField)
        Dim WK_OFFICECODE = selectedHiddenControl.Value

        '計上月を取得
        Dim WK_KEIJOYM = DateTime.Parse(WF_KEIJOYM.Text + "/01")

        'データ取得

        'GridViewの初期化
        GridViewInitialize()

        '営業所ボタンのスタイル変更
        Dim endIndex = selectedHiddenId.Split("_")
        SetOfficeStyle(endIndex(2))

    End Sub

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
        WF_OFFICEBTN_10.CssClass = "btn-office last"

        '選択されているボタンのコントロールを得る
        Dim btnControl = DirectCast(Me.Controls(0).FindControl("contents1").FindControl("WF_OFFICEBTN_" + endIndex), Button)
        btnControl.CssClass += " selected"

    End Sub

    Protected Sub WF_Grid_DeleteRow()
        'グリッドビューから入力データテーブルに格納
        ConvertGridViewToTable()

        '入力データテーブルをコピー
        OIT0008tbl = OIT0008INPtbl.Clone()
        '選択行を除いて行をコピーし、#をカウントし直す
        Dim lineCnt As Integer = 1
        For Each INProw As DataRow In OIT0008INPtbl.Rows
            If INProw("CHECK") = 0 Then

                Dim aRow As DataRow = OIT0008tbl.NewRow()
                aRow.ItemArray = INProw.ItemArray

                aRow("LINECNT") = lineCnt
                OIT0008tbl.Rows.Add(aRow)

                lineCnt = lineCnt + 1
            End If
        Next

        '小計テーブル生成
        CreateSubTotalTable()

        'データバインド
        WF_COSTLISTTBL.DataSource = OIT0008tbl
        WF_COSTLISTTBL.DataBind()
    End Sub

    Protected Sub WF_Grid_AddRow()
        'グリッドビューから入力データテーブルに格納
        ConvertGridViewToTable()

        '入力データテーブルをコピー
        OIT0008tbl = OIT0008INPtbl.Clone()
        '行数を取得
        Dim lineCnt As Integer = 0
        Dim aRow As DataRow = Nothing
        For Each INProw As DataRow In OIT0008INPtbl.Rows
            lineCnt = INProw("LINECNT")
            aRow = OIT0008tbl.NewRow()
            aRow.ItemArray = INProw.ItemArray
            OIT0008tbl.Rows.Add(aRow)
        Next
        '行を追加
        aRow = OIT0008tbl.NewRow()
        aRow("LINECNT") = lineCnt + 1
        aRow("CHECK") = 0
        aRow("DETAIL") = 0
        OIT0008tbl.Rows.Add(aRow)

        '小計テーブル生成
        CreateSubTotalTable()

        'データバインド
        WF_COSTLISTTBL.DataSource = OIT0008tbl
        WF_COSTLISTTBL.DataBind()
    End Sub

    Protected Sub ConvertGridViewToTable()
        'テーブル作成
        OIT0008INPtbl = InitTableCreate()

        'GridViewの行を検索
        For Each gRow As GridViewRow In WF_COSTLISTTBL.Rows
            Dim tRow As DataRow = OIT0008INPtbl.NewRow()
            '#(LINECNT)
            If gRow.FindControl("WF_COSTLISTTBL_LINECNT") IsNot Nothing Then
                tRow("LINECNT") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_LINECNT"), Label).Text
            End If
            '選択(CHECK)
            If gRow.FindControl("WF_COSTLISTTBL_CHECK") IsNot Nothing Then
                Dim tmpCheckBox = DirectCast(gRow.FindControl("WF_COSTLISTTBL_CHECK"), CheckBox)
                If tmpCheckBox IsNot Nothing AndAlso tmpCheckBox.Checked Then
                    tRow("CHECK") = 1
                Else
                    tRow("CHECK") = 0
                End If
            End If
            '確認(DETAIL)
            If gRow.FindControl("WF_COSTLISTTBL_DETAIL") IsNot Nothing Then
                Dim tmpButton = DirectCast(gRow.FindControl("WF_COSTLISTTBL_DETAIL"), Button)
                If tmpButton IsNot Nothing AndAlso tmpButton.Enabled Then
                    tRow("DETAIL") = 1
                Else
                    tRow("DETAIL") = 0
                End If
            End If
            '勘定科目コード(ACCOUNTCODE)
            If gRow.FindControl("WF_COSTLISTTBL_ACCOUNTCODE") IsNot Nothing Then
                tRow("ACCOUNTCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_ACCOUNTCODE"), TextBox).Text
            End If
            'セグメント(SEGMENTCODE)
            If gRow.FindControl("WF_COSTLISTTBL_SEGMENTCODE") IsNot Nothing Then
                tRow("SEGMENTCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTCODE"), Label).Text
            End If
            'セグメント枝番(SEGMENTCODE)
            If gRow.FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE") IsNot Nothing Then
                tRow("SEGMENTBRANCHCODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE"), HiddenField).Value
            End If
            '金額(AMMOUNT)
            If gRow.FindControl("WF_COSTLISTTBL_AMMOUNT") IsNot Nothing Then
                Dim ammount As Integer = 0
                Integer.TryParse(DirectCast(gRow.FindControl("WF_COSTLISTTBL_AMMOUNT"), TextBox).Text, ammount)
                tRow("AMMOUNT") = ammount
            End If
            '税額(TAX)
            If gRow.FindControl("WF_COSTLISTTBL_TAX") IsNot Nothing Then
                Dim tax As Integer = 0
                Integer.TryParse(DirectCast(gRow.FindControl("WF_COSTLISTTBL_TAX"), Label).Text, tax)
                tRow("TAX") = tax
            End If
            '請求先コード(INVOICECODE)
            If gRow.FindControl("WF_COSTLISTTBL_INVOICECODE") IsNot Nothing Then
                tRow("INVOICECODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox).Text
            End If
            '請求先名(INVOICENAME)
            If gRow.FindControl("WF_COSTLISTTBL_INVOICENAME") IsNot Nothing Then
                tRow("INVOICENAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICENAME"), Label).Text
            End If
            '請求先部門(INVOICEDEPT)
            If gRow.FindControl("WF_COSTLISTTBL_INVOICEDEPT") IsNot Nothing Then
                tRow("INVOICEDEPT") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_INVOICEDEPT"), Label).Text
            End If
            '支払先コード(PAYEECODE)
            If gRow.FindControl("WF_COSTLISTTBL_PAYEECODE") IsNot Nothing Then
                tRow("PAYEECODE") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox).Text
            End If
            '支払先名(PAYEENAME)
            If gRow.FindControl("WF_COSTLISTTBL_PAYEENAME") IsNot Nothing Then
                tRow("PAYEENAME") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEENAME"), Label).Text
            End If
            '支払先部門(PAYEEDEPT)
            If gRow.FindControl("WF_COSTLISTTBL_PAYEEDEPT") IsNot Nothing Then
                tRow("PAYEEDEPT") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_PAYEEDEPT"), Label).Text
            End If
            '摘要(ABSTRACT)
            If gRow.FindControl("WF_COSTLISTTBL_ABSTRACT") IsNot Nothing Then
                tRow("ABSTRACT") = DirectCast(gRow.FindControl("WF_COSTLISTTBL_ABSTRACT"), TextBox).Text
            End If
            'テーブルに行追加
            OIT0008INPtbl.Rows.Add(tRow)
        Next
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

        ' 右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueInitSet()

        ''○ GridView初期設定
        'GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueInitSet()

        '営業所ボタンの設定
        GetOffice()
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

        '計上月の初期化(当月の月初)
        WF_KEIJOYM.Text = DateTime.Now.ToString("yyyy/MM")

        '所属営業所によるボタンの制御
        SetOfficeAuth()

        '画面表示データの取得
        WF_Grid_RELOAD()

    End Sub

    Protected Sub GetOffice()

        Dim SQLStrBldr As New StringBuilder
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     ORGCODE AS OFFICECODE")
        SQLStrBldr.AppendLine("     , NAMES AS OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0002_ORG")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE IN ('010007')")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     ORGCODE AS OFFICECODE")
        SQLStrBldr.AppendLine("     , NAMES AS OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0002_ORG")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE IN ('010401','010402')")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     ORGCODE AS OFFICECODE")
        SQLStrBldr.AppendLine("     , NAMES AS OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0002_ORG")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE IN ('011401')")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     ORGCODE AS OFFICECODE")
        SQLStrBldr.AppendLine("     , NAMES AS OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0002_ORG")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE IN ('011201','011202','011203','011402')")
        SQLStrBldr.AppendLine(" UNION ALL")
        SQLStrBldr.AppendLine(" SELECT")
        SQLStrBldr.AppendLine("     ORGCODE AS OFFICECODE")
        SQLStrBldr.AppendLine("     , NAMES AS OFFICENAME")
        SQLStrBldr.AppendLine(" FROM")
        SQLStrBldr.AppendLine("     oil.OIM0002_ORG")
        SQLStrBldr.AppendLine(" WHERE")
        SQLStrBldr.AppendLine("     ORGCODE IN ('012301','012401','012402')")

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

    Protected Sub SetOfficeAuth()

        '暫定(全営業所選択可)
        WF_OFFICEBTN_1.Enabled = True
        WF_OFFICEBTN_2.Enabled = True
        WF_OFFICEBTN_3.Enabled = True
        WF_OFFICEBTN_4.Enabled = True
        WF_OFFICEBTN_5.Enabled = True
        WF_OFFICEBTN_6.Enabled = True
        WF_OFFICEBTN_7.Enabled = True
        WF_OFFICEBTN_8.Enabled = True
        WF_OFFICEBTN_9.Enabled = True
        WF_OFFICEBTN_10.Enabled = True
        '暫定(石油部を初期選択)
        WF_OFFICEHDN_ID.Value = WF_OFFICEHDN_1.ID

    End Sub

    Protected Function InitTableCreate() As DataTable

        Dim retTbl As DataTable = New DataTable()

        retTbl.Columns.Add("LINECNT", Type.GetType("System.Int32"))             '#
        retTbl.Columns.Add("CHECK", Type.GetType("System.Int32"))               '選択
        retTbl.Columns.Add("DETAIL", Type.GetType("System.Int32"))              '確認
        retTbl.Columns.Add("ACCOUNTCODE", Type.GetType("System.String"))        '勘定科目コード(検索？)
        retTbl.Columns.Add("SEGMENTCODE", Type.GetType("System.String"))        'セグメント(表示のみ？)
        retTbl.Columns.Add("SEGMENTBRANCHCODE", Type.GetType("System.String"))  'セグメント枝番(非表示)
        retTbl.Columns.Add("AMMOUNT", Type.GetType("System.Int32"))             '金額(入力有)
        retTbl.Columns.Add("TAX", Type.GetType("System.Int32"))                 '税額(入力 or 表示のみ(自動計算)？)
        retTbl.Columns.Add("INVOICECODE", Type.GetType("System.String"))        '請求先コード(表示のみ？)
        retTbl.Columns.Add("INVOICENAME", Type.GetType("System.String"))        '請求先名(表示のみ？)
        retTbl.Columns.Add("INVOICEDEPT", Type.GetType("System.String"))        '請求先部門(表示のみ？)
        retTbl.Columns.Add("PAYEECODE", Type.GetType("System.String"))          '支払先コード(表示のみ？)
        retTbl.Columns.Add("PAYEENAME", Type.GetType("System.String"))          '支払先名(表示のみ？)
        retTbl.Columns.Add("PAYEEDEPT", Type.GetType("System.String"))          '支払先部門(表示のみ?)
        retTbl.Columns.Add("ABSTRACT", Type.GetType("System.String"))           '摘要(入力有)

        Return retTbl

    End Function

    Protected Sub InitTableDataSet()

        OIT0008tbl = InitTableCreate()

        Dim OIT0008row As DataRow = OIT0008tbl.NewRow()
        OIT0008row("LINECNT") = 1
        OIT0008row("CHECK") = 0
        OIT0008row("DETAIL") = 1
        OIT0008row("ACCOUNTCODE") = "4199999999"
        OIT0008row("SEGMENTCODE") = "10101"
        OIT0008row("SEGMENTBRANCHCODE") = "1"
        OIT0008row("AMMOUNT") = 99999999
        OIT0008row("TAX") = 9999999
        OIT0008row("INVOICECODE") = "ABCDEFGHIJ"
        OIT0008row("INVOICENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("INVOICEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEECODE") = "ABCDEFGHIJ"
        OIT0008row("PAYEENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("ABSTRACT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008tbl.Rows.Add(OIT0008row)

        OIT0008row = OIT0008tbl.NewRow()
        OIT0008row("LINECNT") = 2
        OIT0008row("CHECK") = 0
        OIT0008row("DETAIL") = 0
        OIT0008row("ACCOUNTCODE") = "5199999999"
        OIT0008row("SEGMENTCODE") = "10101"
        OIT0008row("SEGMENTBRANCHCODE") = "1"
        OIT0008row("AMMOUNT") = 9999999
        OIT0008row("TAX") = 9999999
        OIT0008row("INVOICECODE") = "ABCDEFGHIJ"
        OIT0008row("INVOICENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("INVOICEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEECODE") = "ABCDEFGHIJ"
        OIT0008row("PAYEENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("ABSTRACT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008tbl.Rows.Add(OIT0008row)

        OIT0008row = OIT0008tbl.NewRow()
        OIT0008row("LINECNT") = 3
        OIT0008row("CHECK") = 0
        OIT0008row("DETAIL") = 1
        OIT0008row("ACCOUNTCODE") = "5199999999"
        OIT0008row("SEGMENTCODE") = "10101"
        OIT0008row("SEGMENTBRANCHCODE") = "1"
        OIT0008row("AMMOUNT") = 1
        OIT0008row("TAX") = 1
        OIT0008row("INVOICECODE") = "ABCDEFGHIJ"
        OIT0008row("INVOICENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("INVOICEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEECODE") = "ABCDEFGHIJ"
        OIT0008row("PAYEENAME") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("PAYEEDEPT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008row("ABSTRACT") = "あいうえおかきくけこさしすせそたちつてと"
        OIT0008tbl.Rows.Add(OIT0008row)

        '小計テーブル生成
        CreateSubTotalTable()

    End Sub

    Protected Sub CreateSubTotalTable()

        '小計行テーブル生成
        OIT0008SubTotaltbl = New DataTable()
        '小計の集計キー：勘定科目コード/セグメント/セグメント枝番/請求先コード/支払先コード
        OIT0008SubTotaltbl.Columns.Add("ACCOUNTCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("SEGMENTBRANCHCODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("INVOICECODE", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEECODE", Type.GetType("System.String"))
        '表示項目
        OIT0008SubTotaltbl.Columns.Add("INVOICENAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("INVOICEDEPT", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEENAME", Type.GetType("System.String"))
        OIT0008SubTotaltbl.Columns.Add("PAYEEDEPT", Type.GetType("System.String"))
        '集計対象：金額/税額
        OIT0008SubTotaltbl.Columns.Add("AMMOUNT", Type.GetType("System.Int32"))
        OIT0008SubTotaltbl.Columns.Add("TAX", Type.GetType("System.Int32"))

        '小計行テーブルデータ生成
        For Each row As DataRow In OIT0008tbl.Rows

            Dim dataFound As Boolean = False

            '勘定科目コードまたはセグメントが未設定の場合は無視
            If row("ACCOUNTCODE") Is DBNull.Value OrElse
                row("SEGMENTCODE") Is DBNull.Value OrElse
                row("SEGMENTBRANCHCODE") Is DBNull.Value OrElse
                row("INVOICECODE") Is DBNull.Value OrElse
                row("PAYEECODE") Is DBNull.Value Then
                Continue For
            End If

            For Each strow As DataRow In OIT0008SubTotaltbl.Rows
                '勘定科目コード、セグメントが一致する行が存在する場合、金額、税額をそれぞれ加算
                If row("ACCOUNTCODE") = strow("ACCOUNTCODE") AndAlso
                    row("SEGMENTCODE") = strow("SEGMENTCODE") AndAlso
                    row("SEGMENTBRANCHCODE") = strow("SEGMENTBRANCHCODE") AndAlso
                    row("INVOICECODE") = strow("INVOICECODE") AndAlso
                    row("PAYEECODE") = strow("PAYEECODE") Then
                    If Not row("AMMOUNT") Is DBNull.Value Then
                        strow("AMMOUNT") += row("AMMOUNT")
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

                strow("ACCOUNTCODE") = row("ACCOUNTCODE")
                strow("SEGMENTCODE") = row("SEGMENTCODE")
                strow("SEGMENTBRANCHCODE") = row("SEGMENTBRANCHCODE")
                strow("INVOICECODE") = row("INVOICECODE")
                strow("INVOICENAME") = row("INVOICENAME")
                strow("INVOICEDEPT") = row("INVOICEDEPT")
                strow("PAYEECODE") = row("PAYEECODE")
                strow("PAYEENAME") = row("PAYEENAME")
                strow("PAYEEDEPT") = row("PAYEEDEPT")

                strow("AMMOUNT") = row("AMMOUNT")
                If strow("AMMOUNT") Is DBNull.Value Then strow("AMMOUNT") = 0
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
    Protected Sub GridViewInitialize()

        InitTableDataSet()
        WF_COSTLISTTBL.DataSource = OIT0008tbl
        WF_COSTLISTTBL.DataBind()

    End Sub

    Protected Function GetCheckVal(ByRef val As Integer) As Boolean

        If val = 1 Then
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        'If IsNothing(OIT0008tbl) Then
        '    OIT0008tbl = New DataTable
        'End If

        'If OIT0008tbl.Columns.Count <> 0 Then
        '    OIT0008tbl.Columns.Clear()
        'End If

        'OIT0008tbl.Clear()

        ''○ 検索SQL
        ''　検索説明
        ''     条件指定に従い該当データを列車マスタ（臨海）から取得する
        'Dim SQLStrBldr As New StringBuilder
        'SQLStrBldr.AppendLine(" SELECT ")
        'SQLStrBldr.AppendLine("       0                                               AS LINECNT ")          ' 行番号
        'SQLStrBldr.AppendLine("     , ''                                              AS OPERATION ")        ' 編集
        'SQLStrBldr.AppendLine("     , CAST(OIT0008.UPDTIMSTP AS bigint)               AS UPDTIMSTP ")        ' タイムスタンプ
        'SQLStrBldr.AppendLine("     , 1                                               AS 'SELECT' ")         ' 選択
        'SQLStrBldr.AppendLine("     , 0                                               AS HIDDEN ")           ' 非表示
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.OFFICECODE), '')           AS OFFICECODE ")       ' 管轄受注営業所
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.IOKBN), '')                AS IOKBN ")            ' 入線出線区分
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.TRAINNO), '')              AS TRAINNO" )          ' 入線出線列車番号
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.TRAINNAME), '')            AS TRAINNAME" )        ' 入線出線列車名
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.DEPSTATION), '')           AS DEPSTATION" )       ' 発駅コード
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.ARRSTATION), '')           AS ARRSTATION" )       ' 着駅コード
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.PLANTCODE), '')            AS PLANTCODE" )        ' プラントコード
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.LINECNT), '')              AS LINE" )             ' 回線
        'SQLStrBldr.AppendLine("     , ISNULL(RTRIM(OIT0008.DELFLG), '')               AS DELFLG ")           ' 削除フラグ
        'SQLStrBldr.AppendLine(" FROM ")
        'SQLStrBldr.AppendLine("     [oil].OIT0008_RTRAIN OIT0008 ")

        ''○ 条件指定
        'Dim andFlg As Boolean = False

        '' 管轄受注営業所
        'If Not String.IsNullOrEmpty(work.WF_SEL_OFFICECODE.Text) Then
        '    SQLStrBldr.AppendLine(" WHERE ")
        '    SQLStrBldr.AppendLine("     OIT0008.OFFICECODE = @P1 ")
        '    andFlg = True
        'End If

        '' 入線出線区分
        'If Not String.IsNullOrEmpty(work.WF_SEL_IOKBN.Text) Then
        '    If andFlg Then
        '        SQLStrBldr.AppendLine("     AND ")
        '    Else
        '        SQLStrBldr.AppendLine(" WHERE ")
        '    End If
        '    SQLStrBldr.AppendLine("     OIT0008.IOKBN = @P2 ")
        '    andFlg = True
        'End If

        '' 回線
        'If Not String.IsNullOrEmpty(work.WF_SEL_LINE.Text) Then
        '    If andFlg Then
        '        SQLStrBldr.AppendLine("     AND ")
        '    Else
        '        SQLStrBldr.AppendLine(" WHERE ")
        '    End If
        '    SQLStrBldr.AppendLine("     OIT0008.LINECNT = @P3 ")
        '    andFlg = True
        'End If

        ''○ ソート
        'SQLStrBldr.AppendLine(" ORDER BY ")
        'SQLStrBldr.AppendLine("     OIT0008.OFFICECODE ")
        'SQLStrBldr.AppendLine("     , OIT0008.IOKBN ")
        'SQLStrBldr.AppendLine("     , OIT0008.LINECNT ")

        'Try
        '    Using SQLcmd As New SqlCommand(SQLStrBldr.ToString(), SQLcon)
        '        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6)     ' 管轄受注営業所
        '        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)     ' 入線出線区分
        '        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Int, 4)          ' 回線

        '        PARA1.Value = work.WF_SEL_OFFICECODE.Text
        '        PARA2.Value = work.WF_SEL_IOKBN.Text
        '        If String.IsNullOrEmpty(work.WF_SEL_LINE.Text) Then
        '            PARA3.Value = 0
        '        Else
        '            PARA3.Value = Int32.Parse(work.WF_SEL_LINE.Text)
        '        End If


        '        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
        '            '○ フィールド名とフィールドの型を取得
        '            For index As Integer = 0 To SQLdr.FieldCount - 1
        '                OIT0008tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
        '            Next

        '            '○ テーブル検索結果をテーブル格納
        '            OIT0008tbl.Load(SQLdr)
        '        End Using

        '        Dim i As Integer = 0
        '        For Each OIT0008row As DataRow In OIT0008tbl.Rows
        '            i += 1
        '            OIT0008row("LINECNT") = i        ' LINECNT
        '        Next
        '    End Using
        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0008L SELECT")

        '    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         ' SUBクラス名
        '    CS0011LOGWrite.INFPOSI = "DB:OIT0008L Select"
        '    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '    CS0011LOGWrite.TEXT = ex.ToString()
        '    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        '    Exit Sub
        'End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        'グリッドビューから入力データテーブルに格納
        ConvertGridViewToTable()

        '入力データテーブルをコピー
        OIT0008tbl = OIT0008INPtbl.Clone()
        '行数を取得
        Dim lineCnt As Integer = 0
        Dim aRow As DataRow = Nothing
        For Each INProw As DataRow In OIT0008INPtbl.Rows
            lineCnt = INProw("LINECNT")
            aRow = OIT0008tbl.NewRow()
            aRow.ItemArray = INProw.ItemArray
            OIT0008tbl.Rows.Add(aRow)
        Next

        '小計テーブル生成
        CreateSubTotalTable()

        'データバインド
        WF_COSTLISTTBL.DataSource = OIT0008tbl
        WF_COSTLISTTBL.DataBind()

        'フォーカスコントロール
        Dim WK_TextBox As TextBox = Nothing
        Select Case WW_FOCUS_CONTROL
            Case WF_KEIJOYM.ID
                WF_KEIJOYM.Focus()
            Case Else
                If WW_FOCUS_CONTROL.Contains("WF_COSTLISTTBL") Then
                    WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(WW_FOCUS_ROW).FindControl(WW_FOCUS_CONTROL), TextBox)
                    WK_TextBox.Focus()
                End If
        End Select

    End Sub

    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        ''○ エラーレポート準備
        'rightview.SetErrorReport("")

        'Dim WW_RESULT As String = ""

        ''○ 関連チェック
        'RelatedCheck(WW_ERRCODE)

        ''○ 同一レコードチェック
        'If isNormal(WW_ERRCODE) Then
        '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '        SQLcon.Open()       ' DataBase接続

        '        'マスタ更新
        '        UpdateMaster(SQLcon)
        '    End Using
        'End If

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0008tbl)

        ''○ GridView初期設定
        ''○ 画面表示データ再取得
        'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()       ' DataBase接続

        '    MAPDataGet(SQLcon)
        'End Using

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0008tbl)

        ''○ 詳細画面クリア
        'If isNormal(WW_ERRCODE) Then
        '    DetailBoxClear()
        'End If

        ''○ メッセージ表示
        'If Not isNormal(WW_ERRCODE) Then
        '    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        'End If

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
                    If Not row("AMMOUNT") Is DBNull.Value Then
                        WK_INV_AMMOUNT_ALL += row("AMMOUNT")
                    End If
                    If Not row("TAX") Is DBNull.Value Then
                        WK_INV_TAX_ALL += row("TAX")
                    End If
                ElseIf row("ACCOUNTCODE").ToString().Substring(0, 1) = "5" Then
                    If Not row("AMMOUNT") Is DBNull.Value Then
                        WK_PAY_AMMOUNT_ALL += row("AMMOUNT")
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
            If Not gvrow.RowType = DataControlRowType.Footer Then
                Continue For
            End If
            If i < subTotalRowCnt Then  '小計行
                '「小計」のスタイル設定
                gvrow.Cells(0).CssClass = "footerCells text"
                gvrow.Cells(0).ColumnSpan = 3
                gvrow.Cells(0).Text = "小計"

                gvrow.Cells(1).CssClass = "footerCells withicon"
                gvrow.Cells(1).Text = OIT0008SubTotaltbl.Rows(i)("ACCOUNTCODE")

                gvrow.Cells(2).CssClass = "footerCells noicon"
                gvrow.Cells(2).Text = OIT0008SubTotaltbl.Rows(i)("SEGMENTCODE")

                gvrow.Cells(3).CssClass = "footerCells money"
                gvrow.Cells(3).Text = OIT0008SubTotaltbl.Rows(i)("AMMOUNT")

                gvrow.Cells(4).CssClass = "footerCells money"
                gvrow.Cells(4).Text = OIT0008SubTotaltbl.Rows(i)("TAX")

                Dim cellindex As Integer = 4

                gvrow.Cells(cellindex + 1).CssClass = "footerCells withicon"
                gvrow.Cells(cellindex + 1).Text = OIT0008SubTotaltbl.Rows(i)("invoicecode")

                gvrow.Cells(cellindex + 2).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(cellindex + 2).Text = "<span class='inv_pay'>" +
                    OIT0008SubTotaltbl.Rows(i)("INVOICENAME") + "</span>"

                gvrow.Cells(cellindex + 3).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(cellindex + 3).Text = "<span class='inv_pay'>" +
                    OIT0008SubTotaltbl.Rows(i)("INVOICEDEPT") + "</span>"

                gvrow.Cells(cellindex + 4).CssClass = "footerCells withicon"
                gvrow.Cells(cellindex + 4).Text = OIT0008SubTotaltbl.Rows(i)("PAYEECODE")

                gvrow.Cells(cellindex + 5).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(cellindex + 5).Text = "<span class='inv_pay'>" +
                    OIT0008SubTotaltbl.Rows(i)("PAYEENAME") + "</span>"

                gvrow.Cells(cellindex + 6).CssClass = "footerCells noicon inv_pay"
                gvrow.Cells(cellindex + 6).Text = "<span class='inv_pay'>" +
                    OIT0008SubTotaltbl.Rows(i)("PAYEEDEPT") + "</span>"

                For j = cellindex + 7 To gvrow.Cells.Count - 1
                    gvrow.Cells(j).Visible = False
                Next

                i += 1
            Else                        '請求合計
                '「請求合計」のスタイル設定
                gvrow.Cells(0).CssClass = "footerCells text"
                gvrow.Cells(0).ColumnSpan = 5
                gvrow.Cells(0).Text = "請求合計"

                gvrow.Cells(1).CssClass = "footerCells money"
                gvrow.Cells(1).Text = WK_INV_AMMOUNT_ALL

                gvrow.Cells(2).CssClass = "footerCells money"
                gvrow.Cells(2).Text = WK_INV_TAX_ALL

                gvrow.Cells(3).CssClass = "footerCells text"
                gvrow.Cells(3).Text = "支払合計"

                gvrow.Cells(4).CssClass = "footerCells money"
                gvrow.Cells(4).Text = WK_PAY_AMMOUNT_ALL

                gvrow.Cells(5).CssClass = "footerCells money"
                gvrow.Cells(5).Text = WK_PAY_TAX_ALL

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
                        .WF_Calendar.Text = WF_KEIJOYM.Text + "/01"
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
                            prmData = work.CreateFIXParam(Master.USERCAMP, "TORIMASTER")
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

        ''○ 変更した項目の名称をセット
        'Select Case WF_FIELD.Value
        '    Case WF_OFFICECODE.ID
        '        '関連項目
        '        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
        '        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
        '        '管轄営業所
        '        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)

        '    Case WF_OILCODE.ID
        '        '関連項目
        '        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
        '        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
        '        '油種コード
        '        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)

        '    Case WF_SEGMENTOILCODE.ID
        '        '関連項目
        '        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
        '        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
        '        '油種細分コード
        '        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)

        'End Select

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

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        ''○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value

            Case WF_KEIJOYM.ID
                '計上年月
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_KEIJOYM.Text = "1950/01"
                    Else
                        WF_KEIJOYM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WW_FOCUS_CONTROL = WF_KEIJOYM.ID

            Case Else
                Dim rowIdx As Integer
                Dim WK_TextBox As TextBox = Nothing
                Dim WK_Label As Label = Nothing
                Dim WK_Hidden As HiddenField = Nothing
                '勘定科目コード/セグメント/セグメント枝番
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_ACCOUNTCODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)

                    WW_FOCUS_CONTROL = "WF_COSTLISTTBL_ACCOUNTCODE"
                    WW_FOCUS_ROW = rowIdx - 1

                    Dim accountCodes = WW_SelectValue.Split(" ")
                    '勘定科目コード
                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTCODE") IsNot Nothing Then
                        WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_ACCOUNTCODE"), TextBox)
                        WK_TextBox.Text = accountCodes(0)
                    End If
                    'セグメント/セグメント枝番
                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTCODE") IsNot Nothing Then
                        WK_Label = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTCODE"), Label)
                        WK_Label.Text = accountCodes(1)
                        WK_Hidden = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_SEGMENTBRANCHCODE"), HiddenField)
                        WK_Hidden.Value = accountCodes(2)
                    End If
                End If
                '請求先コード
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_INVOICECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)

                    WW_FOCUS_CONTROL = "WF_COSTLISTTBL_INVOICECODE"
                    WW_FOCUS_ROW = rowIdx - 1

                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE") IsNot Nothing Then
                        WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_INVOICECODE"), TextBox)
                        WK_TextBox.Text = WW_SelectValue
                    End If
                    '請求先名/請求先部門取得
                End If
                '支払先コード
                If WF_FIELD.Value.Contains("WF_COSTLISTTBL_PAYEECODE") Then
                    Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)

                    WW_FOCUS_CONTROL = "WF_COSTLISTTBL_PAYEECODE"
                    WW_FOCUS_ROW = rowIdx - 1

                    If WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE") IsNot Nothing Then
                        WK_TextBox = DirectCast(WF_COSTLISTTBL.Rows(rowIdx - 1).FindControl("WF_COSTLISTTBL_PAYEECODE"), TextBox)
                        WK_TextBox.Text = WW_SelectValue
                    End If
                    '支払先名/支払先部門取得
                End If

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
            'Case WF_OFFICECODE.ID
            '    '管轄営業所
            '    WF_OFFICECODE.Focus()

            'Case WF_OILCODE.ID
            '    '油種コード
            '    WF_OILCODE.Focus()

            'Case WF_SEGMENTOILCODE.ID
            '    '油種細分コード
            '    WF_SEGMENTOILCODE.Focus()
            Case WF_KEIJOYM.ID
                WF_KEIJOYM.Focus()
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
        'O_RTN = C_MESSAGE_NO.NORMAL

        'Dim WW_LINE_ERR As String = ""
        'Dim WW_TEXT As String = ""
        'Dim WW_CheckMES1 As String = ""
        'Dim WW_CheckMES2 As String = ""
        'Dim WW_CS0024FCHECKERR As String = ""
        'Dim WW_CS0024FCHECKREPORT As String = ""
        'Dim dateErrFlag As String = ""

        ''○ 画面操作権限チェック
        '' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '' 　※権限判定時点：現在
        'CS0025AUTHORget.USERID = CS0050SESSION.USERID
        'CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        'CS0025AUTHORget.CODE = Master.MAPID
        'CS0025AUTHORget.STYMD = Date.Now
        'CS0025AUTHORget.ENDYMD = Date.Now
        'CS0025AUTHORget.CS0025AUTHORget()
        'If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        'Else
        '    WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
        '    WW_CheckMES2 = ""
        '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
        '    WW_LINE_ERR = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    Exit Sub
        'End If

        ''○ 単項目チェック
        'For Each OIT0008INProw As DataRow In OIT0008INPtbl.Rows

        '    WW_LINE_ERR = ""

        '    ' 管轄受注営業所（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("OFFICECODE")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("OFFICECODE", OIT0008INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(管轄受注営業所エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 入線出線区分（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("IOKBN")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IOKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("IOKBN", OIT0008INProw("IOKBN"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(入線出線区分エラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(入線出線区分エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 入線出線列車番号（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("TRAINNO")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If Not isNormal(WW_CS0024FCHECKERR) Then
        '        WW_CheckMES1 = "・更新できないレコード(入線出線列車番号エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 入線出線列車名（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("TRAINNAME")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If Not isNormal(WW_CS0024FCHECKERR) Then
        '        WW_CheckMES1 = "・更新できないレコード(入線出線列車名エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 発駅コード（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("DEPSTATION")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("STATION", OIT0008INProw("DEPSTATION"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(発駅コードエラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 着駅コード（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("ARRSTATION")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("STATION", OIT0008INProw("ARRSTATION"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(着駅コードエラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' プラントコード（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("PLANTCODE")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PLANTCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("PLANTCODE", OIT0008INProw("PLANTCODE"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(プラントコードエラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(プラントコードエラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 回線（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("LINE")
        '    Master.CheckField(work.WF_SEL_LINE.Text, "LINE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If Not isNormal(WW_CS0024FCHECKERR) Then
        '        WW_CheckMES1 = "・更新できないレコード(回線エラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If

        '    ' 削除フラグ（バリデーションチェック）
        '    WW_TEXT = OIT0008INProw("DELFLG")
        '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If isNormal(WW_CS0024FCHECKERR) Then
        '        ' 値存在チェックT
        '        CODENAME_get("DELFLG", OIT0008INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
        '            WW_CheckMES2 = "マスタに存在しません。"
        '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '            WW_LINE_ERR = "ERR"
        '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '        End If
        '    Else
        '        WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
        '        WW_CheckMES2 = WW_CS0024FCHECKREPORT
        '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIT0008INProw)
        '        WW_LINE_ERR = "ERR"
        '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End If


        '    If WW_LINE_ERR = "" Then
        '        If OIT0008INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        '            OIT0008INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        End If
        '    Else
        '        If WW_LINE_ERR = CONST_PATTERNERR Then
        '            ' 関連チェックエラーをセット
        '            OIT0008INProw.Item("OPERATION") = CONST_PATTERNERR
        '        Else
        '            ' 単項目チェックエラーをセット
        '            OIT0008INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '        End If
        '    End If
        'Next

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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 管轄受注営業所 =" & OIT0008row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線区分 =" & OIT0008row("IOKBN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線列車番号 =" & OIT0008row("TRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線出線列車名 =" & OIT0008row("TRAINNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 発駅コード =" & OIT0008row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 着駅コード =" & OIT0008row("ARRSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> プラントコード =" & OIT0008row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 回線 =" & OIT0008row("LINE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIT0008row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        'work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
        '    Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

    ''' <summary>
    ''' OIT0008tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIT0008tbl_UPD()

        ''○ 画面状態設定
        'For Each OIT0008row As DataRow In OIT0008tbl.Rows
        '    Select Case OIT0008row("OPERATION")
        '        Case C_LIST_OPERATION_CODE.NODATA
        '            OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '        Case C_LIST_OPERATION_CODE.NODISP
        '            OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '        Case C_LIST_OPERATION_CODE.SELECTED
        '            OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
        '            OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        '            OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '    End Select
        'Next

        ''○ 追加変更判定
        'For Each OIT0008INProw As DataRow In OIT0008INPtbl.Rows

        '    ' エラーレコード読み飛ばし
        '    If OIT0008INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
        '        Continue For
        '    End If

        '    OIT0008INProw.Item("OPERATION") = CONST_INSERT

        '    ' KEY項目が等しい時
        '    For Each OIT0008row As DataRow In OIT0008tbl.Rows
        '        If OIT0008row("OFFICECODE") = OIT0008INProw("OFFICECODE") AndAlso
        '            OIT0008row("IOKBN") = OIT0008INProw("IOKBN") AndAlso
        '            OIT0008row("PLANTCODE") = OIT0008INProw("PLANTCODE") Then
        '            ' KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
        '            If OIT0008row("TRAINNO") = OIT0008INProw("TRAINNO") AndAlso
        '                OIT0008row("TRAINNAME") = OIT0008INProw("TRAINNAME") AndAlso
        '                OIT0008row("DEPSTATION") = OIT0008INProw("DEPSTATION") AndAlso
        '                OIT0008row("ARRSTATION") = OIT0008INProw("ARRSTATION") AndAlso
        '                OIT0008row("LINE") = OIT0008INProw("LINE") AndAlso
        '                OIT0008row("DELFLG") = OIT0008INProw("DELFLG") AndAlso
        '                OIT0008INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
        '            Else
        '                ' KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
        '                OIT0008INProw("OPERATION") = CONST_UPDATE
        '                Exit For
        '            End If

        '            Exit For

        '        End If
        '    Next
        'Next

        ''○ 変更有無判定　&　入力値反映
        'For Each OIT0008INProw As DataRow In OIT0008INPtbl.Rows
        '    Select Case OIT0008INProw("OPERATION")
        '        Case CONST_UPDATE
        '            TBL_UPDATE_SUB(OIT0008INProw)
        '        Case CONST_INSERT
        '            TBL_INSERT_SUB(OIT0008INProw)
        '        Case CONST_PATTERNERR
        '            ' 関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
        '            TBL_INSERT_SUB(OIT0008INProw)
        '        Case C_LIST_OPERATION_CODE.ERRORED
        '            TBL_ERR_SUB(OIT0008INProw)
        '    End Select
        'Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIT0008INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIT0008INProw As DataRow)

        'For Each OIT0008row As DataRow In OIT0008tbl.Rows

        '    ' 同一レコードか判定
        '    If OIT0008INProw("OFFICECODE") = OIT0008row("OFFICECODE") AndAlso
        '        OIT0008INProw("IOKBN") = OIT0008row("IOKBN") AndAlso
        '        OIT0008INProw("PLANTCODE") = OIT0008row("PLANTCODE") Then
        '        ' 画面入力テーブル項目設定
        '        OIT0008INProw("LINECNT") = OIT0008row("LINECNT")
        '        OIT0008INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '        OIT0008INProw("UPDTIMSTP") = OIT0008row("UPDTIMSTP")
        '        OIT0008INProw("SELECT") = 1
        '        OIT0008INProw("HIDDEN") = 0

        '        ' 項目テーブル項目設定
        '        OIT0008row.ItemArray = OIT0008INProw.ItemArray
        '        Exit For
        '    End If
        'Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0008INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIT0008INProw As DataRow)

        ''○ 項目テーブル項目設定
        'Dim OIT0008row As DataRow = OIT0008tbl.NewRow
        'OIT0008row.ItemArray = OIT0008INProw.ItemArray

        'OIT0008row("LINECNT") = OIT0008tbl.Rows.Count + 1
        'If OIT0008INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
        '    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        'Else
        '    OIT0008row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        'End If

        'OIT0008row("UPDTIMSTP") = "0"
        'OIT0008row("SELECT") = 1
        'OIT0008row("HIDDEN") = 0

        'OIT0008tbl.Rows.Add(OIT0008row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0008INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIT0008INProw As DataRow)

        'For Each OIT0008row As DataRow In OIT0008tbl.Rows

        '    ' 同一レコードか判定
        '    If OIT0008INProw("OFFICECODE") = OIT0008row("OFFICECODE") AndAlso
        '        OIT0008INProw("IOKBN") = OIT0008row("IOKBN") AndAlso
        '        OIT0008INProw("PLANTCODE") = OIT0008row("PLANTCODE") Then
        '        ' 画面入力テーブル項目設定
        '        OIT0008INProw("LINECNT") = OIT0008row("LINECNT")
        '        OIT0008INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '        OIT0008INProw("UPDTIMSTP") = OIT0008row("UPDTIMSTP")
        '        OIT0008INProw("SELECT") = 1
        '        OIT0008INProw("HIDDEN") = 0

        '        ' 項目テーブル項目設定
        '        OIT0008row.ItemArray = OIT0008INProw.ItemArray
        '        Exit For
        '    End If
        'Next

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
                Case "OFFICECODE"
                    ' 管轄受注営業所
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "IOKBN"
                    ' 入線出線区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "IOKBN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
'                Case "TRAINNO"
'                    ' 入線出線列車番号
'                    prmData = work.CreateTrainNoParam(work.WF_SEL_OFFICECODE.Text, I_VALUE)
'                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATION"
                    ' 駅
                    prmData = work.CreateFIXParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    ' プラントコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "PLANTCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    ' 削除
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

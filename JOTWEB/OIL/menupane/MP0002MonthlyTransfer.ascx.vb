Option Strict On
Imports System.Data.SqlClient
Imports System.Web.UI.DataVisualization.Charting
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 月間輸送数量ユーザーコントロールクラス
''' </summary>
Public Class MP0002MonthlyTransfer
    Inherits MP0000Base
    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    ''' <summary>
    ''' コントロールロード処理 
    ''' １．呼び出し元のフォームのロード(現時点でメニュー画面）
    ''' ２．MP0000Baseのロード処理（同階層にあるMP0000Base）
    ''' ３．当処理の順に呼び出される
    ''' 強制終了を１，２でしない限りは呼び出されるがWinアプリではないので強制終了は無い
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        '自身が非表示の場合は何もしない
        If Me.Visible = False Then
            Return
        End If
        '初回ロードかポストバックか判定
        If IsPostBack = False Then
            '初回ロード
            Initialize()
            Me.hdnCurrentListPattern.Value = Me.ddlListPattern.SelectedValue
        Else
            Try
                'ポストバック
                If Me.hdnRefreshCall.Value = "1" Then
                    '最新化処理
                    With Me.ddlListPattern
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    With Me.ddlMonthTransOffice
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    SetDisplayValues()

                End If
                'ダウンロードボタン押下時処理
                If Me.hdnDownloadCall.Value = "1" Then
                    With Me.ddlListPattern
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    With Me.ddlMonthTransOffice
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    '画面展開するリストデータ(次処理にて同じデータを引き渡し帳票を生成する
                    Dim retVal As List(Of DataTable) = SetDisplayValues()
                    '帳票生成
                    Dim tempFileName As String = String.Format("{0}{1}.xlsx", Me.ID, Me.ddlListPattern.SelectedValue)
                    Using clsPrint As New M00001MP0002CustomReport(
                        Me.Page.Title, tempFileName, retVal,
                        Me.ddlListPattern.SelectedValue, Me.ddlListPattern.SelectedItem.Text,
                        Me.ddlMonthTransOffice.SelectedItem.Text
                        )
                        clsPrint.CreateExcelFileStream(Me.Page)
                    End Using
                End If
                '処理フラグを落とす
                Me.hdnRefreshCall.Value = ""
            Catch ex As Threading.ThreadAbortException
                Dim doNothing = Nothing
            Catch ex As Exception
                Throw
            End Try

        End If 'End IsPostBack = False
    End Sub
    ''' <summary>
    ''' 初期処理
    ''' </summary>
    Protected Sub Initialize()
        Me.lblPaneTitle.Text = String.Format("{0:M月d日}時点　月間輸送数量", Now)
        'MP0000Baseの月間輸送量表、表示パターン一覧を取得
        Dim retDdl As DropDownList = Me.GetMonthlyTransListPattern
        If retDdl.Items.Count > 0 Then
            Me.ddlListPattern.Items.AddRange(retDdl.Items.Cast(Of ListItem).ToArray)
            Dim savedSelectedVal As String = ""
            savedSelectedVal = Me.LoadCookie(ddlListPattern.ClientID)
            If savedSelectedVal = "" Then
                Me.ddlListPattern.SelectedIndex = retDdl.SelectedIndex
            Else
                SetDdlDefaultValue(Me.ddlListPattern, savedSelectedVal)
            End If

        End If

        'MP0000Baseの共通処理の営業所抽出を呼出し営業所ドロップダウン生成
        retDdl = Me.GetOfficeList()
        If retDdl.Items.Count > 0 Then
            Me.ddlMonthTransOffice.Items.AddRange(retDdl.Items.Cast(Of ListItem).ToArray)
            Dim savedSelectedVal As String = ""
            savedSelectedVal = Me.LoadCookie(ddlMonthTransOffice.ClientID)
            If savedSelectedVal = "" Then
                Me.ddlMonthTransOffice.SelectedIndex = retDdl.SelectedIndex
            Else
                SetDdlDefaultValue(Me.ddlMonthTransOffice, savedSelectedVal)
            End If
        End If

        SetDisplayValues()

    End Sub
    ''' <summary>
    ''' 一覧表データ取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListData(sqlCon As SqlConnection) As DataTable
        Dim sqlStat As New StringBuilder
        Dim retTbl As DataTable = Nothing
        Dim variableFields As New List(Of String) '表パターンに応じ動的に動くフィールド名
        '表示切り口に応じ抽出文言フィールドを指定
        Select Case Me.ddlListPattern.SelectedValue
            Case "VIEW001" '営業所別
                variableFields.AddRange({"OILCODE", "OILNAME"})
            Case "VIEW002" '支店別
                variableFields.AddRange({"ORGCODE", "ORGNAME", "BIGOILCODE", "BIGOILNAME", "OILCODE", "OILNAME", "TRAINCLASS", "TRAINCLASSNAME", "SHIPPERCODE", "SHIPPERNAME"})
            Case "VIEW003" '荷主別　請負輸送OT輸送合算
                variableFields.AddRange({"BIGOILCODE", "BIGOILNAME", "OILCODE", "OILNAME", "TRAINCLASS", "TRAINCLASSNAME", "SHIPPERCODE", "SHIPPERNAME"})
            Case "VIEW004" '荷受人別
                variableFields.AddRange({"BIGOILCODE", "BIGOILNAME", "OILCODE", "OILNAME", "TRAINCLASS", "TRAINCLASSNAME", "CONSIGNEECODE", "CONSIGNEENAME"})
            Case "VIEW005" '油種別（中分類）
                variableFields.AddRange({"BIGOILCODE", "BIGOILNAME", "MIDDLEOILCODE", "MIDDLEOILNAME", "OILCODE", "OILNAME", "TRAINCLASS", "TRAINCLASSNAME"})
            Case "VIEW006" '荷主別
                variableFields.AddRange({"BIGOILCODE", "BIGOILNAME", "OILCODE", "OILNAME", "TRAINCLASS", "TRAINCLASSNAME", "SHIPPERCODE", "SHIPPERNAME"})
        End Select

        'SQL生成(動的な名称類のフィールドは後付け）
        sqlStat.AppendLine("SELECT  ISNULL(SUM(VL.MAERUIKEIVOLUME),0)  AS MAERUIKEIVOLUME") '
        sqlStat.AppendLine("      , ISNULL(SUM(VL.RUIKEIVOLUME),0)     AS RUIKEIVOLUME")    '
        sqlStat.AppendLine("      , ISNULL(SUM(VL.VOLUME),0)           AS VOLUME")          '当日輸送（実績）
        sqlStat.AppendLine("      , ISNULL(SUM(VL.VOLUMECHANGE),0)     AS VOLUMECHANGE")    '対予算数量増減
        sqlStat.AppendLine("      , ISNULL(SUM(VL.LYVOLUMECHANGE),0)   AS LYVOLUMECHANGE")  '対前年数量増減
        'どのみち率は純粋にサマリーすると違うので数量増減を元に再計算する
        sqlStat.AppendLine("      , ISNULL(SUM(VL.VOLUMERATIO),0)      AS VOLUMERATIO")     '対予算数量比率
        sqlStat.AppendLine("      , ISNULL(SUM(VL.LYVOLUMERATIO),0)    AS LYVOLUMERATIO")   '対前年数量比率
        '動的な名称類フィールド
        For Each varField As String In variableFields
            sqlStat.AppendFormat(" , VL.{0}", varField).AppendLine()
        Next
        'プログラム内で行結合判定用のフィールド（この段階では確定しない）
        sqlStat.AppendLine("      , ''    AS ROWSPANFIELD1")
        sqlStat.AppendLine("      , ''    AS ROWSPANFIELD2")
        sqlStat.AppendLine("  FROM OIL.ANA1000_VOLUME VL with(nolock)")

        sqlStat.AppendLine(" WHERE VL.TARGETYM = @TARGETYM")
        sqlStat.AppendLine("   AND VL.DELFLG   = @DELFLG")
        If Me.ddlListPattern.SelectedValue = "VIEW001" Then
            sqlStat.AppendLine("   AND VL.OFFICECODE   = @OFFICECODE")
        End If
        sqlStat.AppendLine(" GROUP BY ")
        For Each varField As String In variableFields
            If varField = variableFields(0) Then
                sqlStat.AppendFormat("          VL.{0}", varField).AppendLine()
            Else
                sqlStat.AppendFormat("         ,VL.{0}", varField).AppendLine()
            End If
        Next

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                Dim targetYM As String = Now.ToString("yyyy/MM")
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.ddlMonthTransOffice.SelectedValue
                .Add("@TARGETYM", SqlDbType.NVarChar).Value = targetYM
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE

            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If IsNothing(retTbl) Then
                    retTbl = New DataTable

                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        retTbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                End If

                retTbl.Clear()
                retTbl.Load(sqlDr)
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retTbl
    End Function
    ''' <summary>
    ''' グラフ及び一覧表のデータを設定
    ''' </summary>
    Private Function SetDisplayValues() As List(Of DataTable)
        '部署ドロップダウンの表示非表示
        If Me.ddlListPattern.SelectedValue = "VIEW001" Then
            Me.divMonthlyTransOffice.Visible = True
        Else
            Me.divMonthlyTransOffice.Visible = False
        End If
        'マルチビューの切替(未定義の場合VIWEのID="UNDEFINE"に切り替え処理終了
        Dim selView As View = DirectCast(Me.mvwMonthlyTransfer.FindControl(Me.ddlListPattern.SelectedValue), View)
        If selView Is Nothing Then
            selView = DirectCast(Me.mvwMonthlyTransfer.FindControl("UNDEFINE"), View)
            Me.mvwMonthlyTransfer.SetActiveView(selView)
            Return Nothing
        End If
        Me.mvwMonthlyTransfer.SetActiveView(selView)

        'データ取得（データ取得が当ペイン共通ロジックで収まらなくなった場合、検討を）
        Dim dt As DataTable
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            dt = GetListData(sqlCon)
        End Using

        '画面にデータ展開(長くなるので各画面展開用関数に投げる)
        ClearAllDataBinds() '一旦全てのDataBindをクリア
        Dim retVal As New List(Of DataTable)
        Select Case Me.ddlListPattern.SelectedValue
            Case "VIEW001"
                retVal = SetView001(dt)
            Case "VIEW002"
                retVal = SetView002(dt)
            Case "VIEW003"
                retVal = SetView003(dt)
            Case "VIEW004"
                retVal = SetView004(dt)
            Case "VIEW005"
                retVal = SetView005(dt)
            Case "VIEW006"
                retVal = SetView006(dt)
        End Select
        Return retVal
    End Function
    ''' <summary>
    ''' ペイン内部のバインドデータを全てクリア(VIEWSTATEの容量軽減の為)
    ''' </summary>
    Private Sub ClearAllDataBinds()
        Me.repMonthTrans.DataSource = Nothing
        Me.repMonthTrans.DataBind()
        Me.chtMonthTrans.DataSource = Nothing
        Me.chtMonthTrans.DataBind()

        Me.repMonthTrans002.DataSource = Nothing
        Me.repMonthTrans002.DataBind()

        Me.repMonthTrans003.DataSource = Nothing
        Me.repMonthTrans003.DataBind()

        Me.repMonthTrans004.DataSource = Nothing
        Me.repMonthTrans004.DataBind()

        Me.repMonthTrans005.DataSource = Nothing
        Me.repMonthTrans005.DataBind()

        Me.repMonthTrans006.DataSource = Nothing
        Me.repMonthTrans006.DataBind()

        Me.pnlNoData.Visible = False
        Me.btnDownload.Visible = True
    End Sub
    ''' <summary>
    ''' 営業所別ビューコンテンツ展開
    ''' </summary>
    ''' <param name="dt"></param>
    Private Function SetView001(dt As DataTable) As List(Of DataTable)
        Dim targetTbl As DataTable = Nothing
        Dim qTarget = (From dr As DataRow In dt Order By Convert.ToString(dr("OILCODE")) Ascending)
        If qTarget.Any Then
            targetTbl = qTarget.CopyToDataTable
        End If
        Me.repMonthTrans.DataSource = targetTbl
        Me.repMonthTrans.DataBind()
        With Me.chtMonthTrans
            Dim revData = targetTbl
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                revData = (From dr As DataRow In dt Order By Convert.ToString(dr("OILCODE")) Descending).CopyToDataTable
            End If

            .DataSource = revData
            .DataBind()

        End With
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Me.chtMonthTrans.Visible = False
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        Else
            Me.chtMonthTrans.Visible = True
        End If
        Return New List(Of DataTable) From {targetTbl}
    End Function
    ''' <summary>
    ''' 支店別画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Function SetView002(dt As DataTable) As List(Of DataTable)
        Dim dtSum As DataTable = dt.Clone
        Dim dtShiro As DataTable = dt.Clone
        Dim dtKuro As DataTable = dt.Clone
        Dim appendDr As DataRow = Nothing
        '*********************************
        '粒データを白・黒別に集計する
        '*********************************
        For Each bigOilCode As String In {"B", "W"}
            Dim appendDt As DataTable = Nothing
            '油種分類に応じ挿入先テーブルを判定
            If bigOilCode = "B" Then
                appendDt = dtKuro
            Else
                appendDt = dtShiro
            End If

            For Each trainClass As String In {"J", "O"}
                '対象の黒油・白油、JOT・OT輸送を絞り込む
                Dim qTarget = (From dr As DataRow In dt
                               Where dr("BIGOILCODE").Equals(bigOilCode) _
                                           AndAlso dr("TRAINCLASS").Equals(trainClass)
                               Order By Convert.ToString(dr("ORGCODE"))
                               )
                If qTarget.Any = False Then
                    Continue For
                End If
                Dim targetDt As DataTable = qTarget.CopyToDataTable
                '対象内の支店コードをグループ化
                Dim orgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("ORGCODE")) Into Group Order By x Select x
                Dim IsTransBreak As Boolean = True
                For Each orgCode As String In orgCodeList
                    Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("ORGCODE").Equals(orgCode)).CopyToDataTable
                    appendDr = CreateDispRow(orgTbl, appendDt)
                    '付帯文言を読み取りテーブルから付与
                    Dim fstRow As DataRow = orgTbl.Rows(0)
                    appendDr("ORGCODE") = fstRow("ORGCODE")
                    appendDr("ORGNAME") = fstRow("ORGNAME")
                    appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                    appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                    appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                    appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                    If IsTransBreak = True Then
                        IsTransBreak = False
                        appendDr("ROWSPANFIELD2") = orgCodeList.Count + 1
                    End If
                    appendDt.Rows.Add(appendDr)
                Next orgCode

                If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                    Continue For
                End If
                '合計行の設定
                appendDr = CreateDispRow(targetDt, appendDt)
                appendDr("ORGNAME") = "計"
                appendDr("BIGOILCODE") = bigOilCode
                appendDr("TRAINCLASS") = trainClass
                appendDt.Rows.Add(appendDr)
            Next trainClass
            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            appendDt.Rows(0)("ROWSPANFIELD1") = appendDt.Rows.Count
        Next bigOilCode
        '*********************************
        '白黒の合算
        '*********************************
        Dim margeTable As DataTable = dtShiro.AsEnumerable().Union(dtKuro.AsEnumerable).CopyToDataTable
        For Each trainClass As String In {"J", "O"}
            '対象の黒油・白油、JOT・OT輸送を絞り込む
            Dim qTarget = (From dr As DataRow In margeTable
                           Where dr("TRAINCLASS").Equals(trainClass) _
                                       AndAlso Convert.ToString(dr("ORGCODE")) <> ""
                           Order By Convert.ToString(dr("ORGCODE"))
                           )
            If qTarget.Any = False Then
                Continue For
            End If
            Dim targetDt As DataTable = qTarget.CopyToDataTable
            Dim allOrgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("ORGCODE")) Into Group Order By x Select x
            Dim IsAllTransBreak As Boolean = True
            For Each orgCode As String In allOrgCodeList
                Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("ORGCODE").Equals(orgCode)).CopyToDataTable
                appendDr = CreateDispRow(orgTbl, dtSum)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = orgTbl.Rows(0)
                appendDr("ORGCODE") = fstRow("ORGCODE")
                appendDr("ORGNAME") = fstRow("ORGNAME")
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                If IsAllTransBreak = True Then
                    IsAllTransBreak = False
                    appendDr("ROWSPANFIELD2") = allOrgCodeList.Count + 1
                End If
                dtSum.Rows.Add(appendDr)
            Next orgCode
            If dtSum Is Nothing OrElse dtSum.Rows.Count = 0 Then
                Continue For
            End If
            '合計行の設定
            appendDr = CreateDispRow(targetDt, dtSum)
            appendDr("ORGNAME") = "計"
            appendDr("BIGOILCODE") = "計"
            appendDr("TRAINCLASS") = trainClass
            dtSum.Rows.Add(appendDr)
        Next trainClass
        If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
            dtSum.Rows(0)("ROWSPANFIELD1") = dtSum.Rows.Count
        End If

        '*********************************
        '画面へのデータバインド
        '*********************************
        Dim dispData As New List(Of DataTable)
        '一方（白or黒）しか無い場合白黒合算のテーブルを出す意味が無いので制御
        If dtSum.Rows.Count = 0 Then
            dispData = Nothing
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.Add(dtShiro)
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.Add(dtKuro)
        Else
            dispData.AddRange({dtSum, dtShiro, dtKuro})
        End If
        Me.repMonthTrans002.DataSource = dispData
        Me.repMonthTrans002.DataBind()
        Return dispData
    End Function
    ''' <summary>
    ''' 荷主別　請負輸送OT輸送合算画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Function SetView003(dt As DataTable) As List(Of DataTable)
        Dim dtSum As DataTable = dt.Clone
        Dim dtShiro As DataTable = dt.Clone
        Dim dtKuro As DataTable = dt.Clone
        Dim appendDr As DataRow = Nothing
        '*********************************
        '粒データを白・黒別に集計する
        '*********************************
        For Each bigOilCode As String In {"B", "W"}
            Dim appendDt As DataTable = Nothing
            '油種分類に応じ挿入先テーブルを判定
            If bigOilCode = "B" Then
                appendDt = dtKuro
            Else
                appendDt = dtShiro
            End If


            '対象の黒油・白油、JOT・OT輸送を絞り込む
            Dim qTarget = (From dr As DataRow In dt
                           Where dr("BIGOILCODE").Equals(bigOilCode) _
                             AndAlso {"J", "O"}.Contains(Convert.ToString(dr("TRAINCLASS")))
                           Order By Convert.ToString(dr("SHIPPERCODE"))
                               )
            If qTarget.Any = False Then
                Continue For
            End If
            Dim targetDt As DataTable = qTarget.CopyToDataTable
            '対象内の支店コードをグループ化
            Dim shpCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("SHIPPERCODE")) Into Group Order By x Select x
            For Each shpCode As String In shpCodeList
                Dim shpTbl As DataTable = (From dr As DataRow In targetDt Where dr("SHIPPERCODE").Equals(shpCode)).CopyToDataTable
                appendDr = CreateDispRow(shpTbl, appendDt)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = shpTbl.Rows(0)
                appendDr("SHIPPERCODE") = fstRow("SHIPPERCODE")
                appendDr("SHIPPERNAME") = StrConv(Convert.ToString(fstRow("SHIPPERNAME")), VbStrConv.Wide)
                appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                appendDr("TRAINCLASS") = ""
                appendDr("TRAINCLASSNAME") = "請負+ＯＴ"
                appendDt.Rows.Add(appendDr)
            Next shpCode

            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            '合計行の設定
            appendDr = CreateDispRow(targetDt, appendDt)
            appendDr("SHIPPERNAME") = "計"
            appendDr("BIGOILCODE") = bigOilCode
            appendDt.Rows.Add(appendDr)

            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            appendDt.Rows(0)("ROWSPANFIELD1") = appendDt.Rows.Count
        Next bigOilCode
        '*********************************
        '白黒の合算
        '*********************************
        Dim margeTable As DataTable = dtShiro.AsEnumerable().Union(dtKuro.AsEnumerable).CopyToDataTable

        '対象の黒油・白油、JOT・OT輸送を絞り込む
        Dim qAllTarget = (From dr As DataRow In margeTable
                          Where Convert.ToString(dr("SHIPPERCODE")) <> ""
                          Order By Convert.ToString(dr("SHIPPERCODE"))
                           )
        If qAllTarget.Any = True Then

            Dim targetDt As DataTable = qAllTarget.CopyToDataTable
            Dim allOrgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("SHIPPERCODE")) Into Group Order By x Select x
            Dim IsAllTransBreak As Boolean = True
            For Each orgCode As String In allOrgCodeList
                Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("SHIPPERCODE").Equals(orgCode)).CopyToDataTable
                appendDr = CreateDispRow(orgTbl, dtSum)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = orgTbl.Rows(0)
                appendDr("SHIPPERCODE") = fstRow("SHIPPERCODE")
                appendDr("SHIPPERNAME") = fstRow("SHIPPERNAME")
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASSNAME") = "請負+ＯＴ"
                dtSum.Rows.Add(appendDr)
            Next orgCode
            If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
                '合計行の設定
                appendDr = CreateDispRow(targetDt, dtSum)
                appendDr("SHIPPERNAME") = "計"
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASSNAME") = "請負+ＯＴ"
                dtSum.Rows.Add(appendDr)
            End If
        End If

        If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
            dtSum.Rows(0)("ROWSPANFIELD1") = dtSum.Rows.Count
        End If

        '*********************************
        '画面へのデータバインド
        '*********************************
        Dim dispData As New List(Of DataTable)
        '一方（白or黒）しか無い場合白黒合算のテーブルを出す意味が無いので制御
        If dtSum.Rows.Count = 0 Then
            dispData = Nothing
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.Add(dtShiro)
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.Add(dtKuro)
        Else
            dispData.AddRange({dtSum, dtShiro, dtKuro})
        End If
        Me.repMonthTrans003.DataSource = dispData
        Me.repMonthTrans003.DataBind()
        Return dispData
    End Function
    ''' <summary>
    ''' 荷受人別画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Function SetView004(dt As DataTable) As List(Of DataTable)
        Dim dtSum As DataTable = dt.Clone
        Dim dtShiro As DataTable = dt.Clone
        Dim dtKuro As DataTable = dt.Clone
        Dim appendDr As DataRow = Nothing
        '*********************************
        '粒データを白・黒別に集計する
        '*********************************
        For Each bigOilCode As String In {"B", "W"}
            Dim appendDt As DataTable = Nothing
            '油種分類に応じ挿入先テーブルを判定
            If bigOilCode = "B" Then
                appendDt = dtKuro
            Else
                appendDt = dtShiro
            End If


            '対象の黒油・白油、OT輸送を絞り込む 当切り口はOTのみ
            Dim qTarget = (From dr As DataRow In dt
                           Where dr("BIGOILCODE").Equals(bigOilCode) _
                             AndAlso {"O"}.Contains(Convert.ToString(dr("TRAINCLASS")))
                           Order By Convert.ToString(dr("CONSIGNEECODE"))
                               )
            If qTarget.Any = False Then
                Continue For
            End If
            Dim targetDt As DataTable = qTarget.CopyToDataTable
            '対象内の支店コードをグループ化
            Dim shpCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("CONSIGNEECODE")) Into Group Order By x Select x
            For Each shpCode As String In shpCodeList
                Dim shpTbl As DataTable = (From dr As DataRow In targetDt Where dr("CONSIGNEECODE").Equals(shpCode)).CopyToDataTable
                appendDr = CreateDispRow(shpTbl, appendDt)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = shpTbl.Rows(0)
                appendDr("CONSIGNEECODE") = fstRow("CONSIGNEECODE")
                appendDr("CONSIGNEENAME") = StrConv(Convert.ToString(fstRow("CONSIGNEENAME")), VbStrConv.Wide).Replace("ＯＴ", "")
                appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                appendDt.Rows.Add(appendDr)
            Next shpCode

            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            '合計行の設定
            appendDr = CreateDispRow(targetDt, appendDt)
            appendDr("CONSIGNEENAME") = "計"
            appendDr("BIGOILCODE") = bigOilCode
            appendDt.Rows.Add(appendDr)

            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            appendDt.Rows(0)("ROWSPANFIELD1") = appendDt.Rows.Count
        Next bigOilCode
        '*********************************
        '白黒の合算
        '*********************************
        Dim qmargeTable = dtShiro.AsEnumerable().Union(dtKuro.AsEnumerable)
        Dim margeTable As DataTable = dt.Clone
        If qmargeTable.Any Then
            margeTable = qmargeTable.CopyToDataTable
        End If
        '対象の黒油・白油、JOT・OT輸送を絞り込む
        Dim qAllTarget = (From dr As DataRow In margeTable
                          Where Convert.ToString(dr("CONSIGNEECODE")) <> ""
                          Order By Convert.ToString(dr("CONSIGNEECODE"))
                           )
        If qAllTarget.Any = True Then

            Dim targetDt As DataTable = qAllTarget.CopyToDataTable
            Dim allOrgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("CONSIGNEECODE")) Into Group Order By x Select x
            Dim IsAllTransBreak As Boolean = True
            For Each orgCode As String In allOrgCodeList
                Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("CONSIGNEECODE").Equals(orgCode)).CopyToDataTable
                appendDr = CreateDispRow(orgTbl, dtSum)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = orgTbl.Rows(0)
                appendDr("CONSIGNEECODE") = fstRow("CONSIGNEECODE")
                appendDr("CONSIGNEENAME") = fstRow("CONSIGNEENAME")
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                dtSum.Rows.Add(appendDr)
            Next orgCode
            If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
                '合計行の設定
                appendDr = CreateDispRow(targetDt, dtSum)
                appendDr("CONSIGNEENAME") = "計"
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASSNAME") = "ＯＴ"
                dtSum.Rows.Add(appendDr)
            End If
        End If

        If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
            dtSum.Rows(0)("ROWSPANFIELD1") = dtSum.Rows.Count
        End If

        '*********************************
        '画面へのデータバインド
        '*********************************
        Dim dispData As New List(Of DataTable)
        '一方（白or黒）しか無い場合白黒合算のテーブルを出す意味が無いので制御
        If dtSum.Rows.Count = 0 Then
            dispData = Nothing
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.Add(dtShiro)
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.Add(dtKuro)
        Else
            dispData.AddRange({dtSum, dtShiro, dtKuro})
        End If
        Me.repMonthTrans004.DataSource = dispData
        Me.repMonthTrans004.DataBind()
        Return dispData
    End Function
    ''' <summary>
    ''' 油種別（中分類）画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Function SetView005(dt As DataTable) As List(Of DataTable)
        Dim dtDisp As DataTable = dt.Clone
        Dim appendDr As DataRow = Nothing
        '*********************************
        '粒データを白・黒別に集計する
        '*********************************
        Dim rowCnt As Integer = 0
        For Each trainClass As String In {"J", "O"}
            rowCnt = dtDisp.Rows.Count
            '油種分類に応じ挿入先テーブルを判定
            For Each bigOilCode As String In {"W", "B"}
                Dim hasBigOilData As Boolean
                hasBigOilData = False
                '対象の黒油・白油、JOT・OT輸送を絞り込む
                Dim qTarget = (From dr As DataRow In dt
                               Where dr("BIGOILCODE").Equals(bigOilCode) _
                             AndAlso dr("TRAINCLASS").Equals(trainClass)
                               Order By Convert.ToString(dr("OILCODE"))
                               )
                If qTarget.Any = False Then
                    Continue For
                End If
                Dim targetDt As DataTable = qTarget.CopyToDataTable
                '対象内の支店コードをグループ化
                Dim oilCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("OILCODE")) Into Group Order By x Select x
                Dim IsTransBreak As Boolean = True
                Dim bigOilName As String = ""
                For Each oilCode As String In oilCodeList
                    Dim oilTbl As DataTable = (From dr As DataRow In targetDt Where dr("OILCODE").Equals(oilCode)).CopyToDataTable
                    appendDr = CreateDispRow(oilTbl, dtDisp)
                    '付帯文言を読み取りテーブルから付与
                    Dim fstRow As DataRow = oilTbl.Rows(0)
                    appendDr("OILCODE") = fstRow("OILCODE")
                    appendDr("OILNAME") = StrConv(Convert.ToString(fstRow("OILNAME")), VbStrConv.Wide)
                    appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                    appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                    appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                    appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                    bigOilName = Convert.ToString(fstRow("BIGOILNAME"))
                    If IsTransBreak = True Then
                        IsTransBreak = False
                        appendDr("ROWSPANFIELD2") = oilCodeList.Count + 1
                    End If
                    hasBigOilData = True
                    dtDisp.Rows.Add(appendDr)
                Next oilCode

                If hasBigOilData = False Then
                    Continue For
                End If
                '油種大分類合計行の設定
                appendDr = CreateDispRow(targetDt, dtDisp)
                appendDr("OILNAME") = bigOilName & "計"
                appendDr("BIGOILCODE") = bigOilCode
                appendDr("TRAINCLASS") = trainClass
                dtDisp.Rows.Add(appendDr)
            Next bigOilCode
            If dtDisp Is Nothing OrElse dtDisp.Rows.Count = 0 Then
                Continue For
            End If
            '輸送合計行の設定
            Dim qsumTrainClass = (From dr In dt Where dr("TRAINCLASS").Equals(trainClass))
            If qsumTrainClass.Any = False Then
                Continue For
            End If
            Dim sumTrainClassDt As DataTable = qsumTrainClass.CopyToDataTable
            appendDr = CreateDispRow(sumTrainClassDt, dtDisp)
            appendDr("OILNAME") = "合計"
            appendDr("BIGOILCODE") = ""
            appendDr("TRAINCLASS") = trainClass
            dtDisp.Rows.Add(appendDr)

            dtDisp.Rows(rowCnt)("ROWSPANFIELD1") = dtDisp.Rows.Count - rowCnt
        Next trainClass

        '*********************************
        '画面へのデータバインド
        '*********************************
        '一方（白or黒）しか無い場合白黒合算のテーブルを出す意味が無いので制御
        If dtDisp Is Nothing OrElse dtDisp.Rows.Count = 0 Then
            'dispData = Nothing
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        End If
        Me.repMonthTrans005.DataSource = dtDisp
        Me.repMonthTrans005.DataBind()
        Return New List(Of DataTable) From {dtDisp}
    End Function
    ''' <summary>
    ''' 荷主別画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Function SetView006(dt As DataTable) As List(Of DataTable)
        Dim dtAllSum As DataTable = dt.Clone
        Dim dtSum As DataTable = dt.Clone
        Dim dtShiro As DataTable = dt.Clone
        Dim dtKuro As DataTable = dt.Clone
        Dim appendDr As DataRow = Nothing
        '*********************************
        '粒データを白・黒別に集計する
        '*********************************
        For Each bigOilCode As String In {"B", "W"}
            Dim appendDt As DataTable = Nothing
            '油種分類に応じ挿入先テーブルを判定
            If bigOilCode = "B" Then
                appendDt = dtKuro
            Else
                appendDt = dtShiro
            End If

            For Each trainClass As String In {"J", "O"}
                '対象の黒油・白油、JOT・OT輸送を絞り込む
                Dim qTarget = (From dr As DataRow In dt
                               Where dr("BIGOILCODE").Equals(bigOilCode) _
                                           AndAlso dr("TRAINCLASS").Equals(trainClass)
                               Order By Convert.ToString(dr("SHIPPERCODE"))
                               )
                If qTarget.Any = False Then
                    Continue For
                End If
                Dim targetDt As DataTable = qTarget.CopyToDataTable
                '対象内の支店コードをグループ化
                Dim orgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("SHIPPERCODE")) Into Group Order By x Select x
                Dim IsTransBreak As Boolean = True
                For Each orgCode As String In orgCodeList
                    Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("SHIPPERCODE").Equals(orgCode)).CopyToDataTable
                    appendDr = CreateDispRow(orgTbl, appendDt)
                    '付帯文言を読み取りテーブルから付与
                    Dim fstRow As DataRow = orgTbl.Rows(0)
                    appendDr("SHIPPERCODE") = fstRow("SHIPPERCODE")
                    appendDr("SHIPPERNAME") = StrConv(Convert.ToString(fstRow("SHIPPERNAME")), VbStrConv.Wide)
                    appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                    appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                    appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                    appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                    If IsTransBreak = True Then
                        IsTransBreak = False
                        appendDr("ROWSPANFIELD2") = orgCodeList.Count + 1
                    End If
                    appendDt.Rows.Add(appendDr)
                Next orgCode

                If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                    Continue For
                End If
                '合計行の設定
                appendDr = CreateDispRow(targetDt, appendDt)
                appendDr("SHIPPERNAME") = "計"
                appendDr("BIGOILCODE") = bigOilCode
                appendDr("TRAINCLASS") = trainClass
                appendDt.Rows.Add(appendDr)
            Next trainClass
            If appendDt Is Nothing OrElse appendDt.Rows.Count = 0 Then
                Continue For
            End If
            appendDt.Rows(0)("ROWSPANFIELD1") = appendDt.Rows.Count
        Next bigOilCode
        '*********************************
        '白黒の合算
        '*********************************
        Dim margeTable As DataTable = dtShiro.AsEnumerable().Union(dtKuro.AsEnumerable).CopyToDataTable
        For Each trainClass As String In {"J", "O"}
            '対象の黒油・白油、JOT・OT輸送を絞り込む
            Dim qTarget = (From dr As DataRow In margeTable
                           Where dr("TRAINCLASS").Equals(trainClass) _
                                       AndAlso Convert.ToString(dr("SHIPPERCODE")) <> ""
                           Order By Convert.ToString(dr("SHIPPERCODE"))
                           )
            If qTarget.Any = False Then
                Continue For
            End If
            Dim targetDt As DataTable = qTarget.CopyToDataTable
            Dim allOrgCodeList = From dr As DataRow In targetDt Group By x = Convert.ToString(dr("SHIPPERCODE")) Into Group Order By x Select x
            Dim IsAllTransBreak As Boolean = True
            For Each orgCode As String In allOrgCodeList
                Dim orgTbl As DataTable = (From dr As DataRow In targetDt Where dr("SHIPPERCODE").Equals(orgCode)).CopyToDataTable
                appendDr = CreateDispRow(orgTbl, dtSum)
                '付帯文言を読み取りテーブルから付与
                Dim fstRow As DataRow = orgTbl.Rows(0)
                appendDr("SHIPPERCODE") = fstRow("SHIPPERCODE")
                appendDr("SHIPPERNAME") = StrConv(Convert.ToString(fstRow("SHIPPERNAME")), VbStrConv.Wide)
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASS") = fstRow("TRAINCLASS")
                appendDr("TRAINCLASSNAME") = StrConv(Convert.ToString(fstRow("TRAINCLASSNAME")), VbStrConv.Wide).Replace("輸送", "")
                If IsAllTransBreak = True Then
                    IsAllTransBreak = False
                    appendDr("ROWSPANFIELD2") = allOrgCodeList.Count + 1
                End If
                dtSum.Rows.Add(appendDr)
            Next orgCode
            If dtSum Is Nothing OrElse dtSum.Rows.Count = 0 Then
                Continue For
            End If
            '合計行の設定
            appendDr = CreateDispRow(targetDt, dtSum)
            appendDr("SHIPPERNAME") = "計"
            appendDr("BIGOILCODE") = "計"
            appendDr("TRAINCLASS") = trainClass
            dtSum.Rows.Add(appendDr)
        Next trainClass
        If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
            dtSum.Rows(0)("ROWSPANFIELD1") = dtSum.Rows.Count
        End If
        '*********************************
        '総計テーブル生成
        '*********************************
        For Each trainClass As String In {"J", "O"}
            '対象の黒油・白油、JOT・OT輸送を絞り込む
            Dim qTarget = (From dr As DataRow In margeTable
                           Where dr("TRAINCLASS").Equals(trainClass) _
                                       AndAlso Convert.ToString(dr("SHIPPERCODE")) <> ""
                           Order By Convert.ToString(dr("SHIPPERCODE"))
                           )
            If qTarget.Any = False Then
                Continue For
            End If
            Dim targetDt As DataTable = qTarget.CopyToDataTable
            '総計行の設定
            appendDr = CreateDispRow(targetDt, dtAllSum)
            appendDr("SHIPPERNAME") = "全社"
            appendDr("BIGOILCODE") = targetDt.Rows(0)("BIGOILCODE")
            appendDr("BIGOILNAME") = "計"
            appendDr("TRAINCLASS") = targetDt.Rows(0)("TRAINCLASS")
            appendDr("TRAINCLASSNAME") = targetDt.Rows(0)("TRAINCLASSNAME")
            appendDr("ROWSPANFIELD2") = "1"
            dtAllSum.Rows.Add(appendDr)
        Next trainClass
        If Not (dtAllSum Is Nothing OrElse dtAllSum.Rows.Count = 0) Then
            '合計行の設定
            appendDr = CreateDispRow(dtAllSum, dtAllSum)
            appendDr("SHIPPERNAME") = "全社"
            appendDr("BIGOILNAME") = "計"
            appendDr("TRAINCLASSNAME") = "総計"
            appendDr("ROWSPANFIELD2") = "1"
            dtAllSum.Rows.Add(appendDr)
            dtAllSum.Rows(0)("ROWSPANFIELD1") = dtAllSum.Rows.Count
        End If
        '*********************************
        '画面へのデータバインド
        '*********************************
        Dim dispData As New List(Of DataTable)
        '一方（白or黒）しか無い場合白黒合算のテーブルを出す意味が無いので制御
        If dtSum.Rows.Count = 0 Then
            dispData = Nothing
            Me.pnlNoData.Visible = True
            Me.btnDownload.Visible = False
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.AddRange({dtAllSum, dtShiro})
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.AddRange({dtAllSum, dtKuro})
        Else
            dispData.AddRange({dtAllSum, dtSum, dtShiro, dtKuro})
        End If
        Me.repMonthTrans006.DataSource = dispData
        Me.repMonthTrans006.DataBind()
        Return dispData
    End Function
    ''' <summary>
    ''' 率再計算
    ''' </summary>
    ''' <param name="vol">実績値</param>
    ''' <param name="ud">増減値</param>
    ''' <returns>0.xx表記の率（%ではない)</returns>
    Private Function CalcRatio(vol As Object, ud As Object) As Decimal
        '数値変換出来ない場合は0でリターンし終了
        If Not (IsNumeric(vol) AndAlso IsNumeric(ud)) Then
            Return 0
        End If
        Dim decVol As Decimal = CDec(vol)
        Dim decUd As Decimal = CDec(ud)

        '対実績値を算出
        Dim devVsVol As Decimal = decVol - decUd
        Dim retRetio As Decimal = 0
        '0除算になる場合は0でリターンし終了
        If devVsVol = 0 Then
            Return 0
        End If
        retRetio = (decVol / devVsVol) - 1
        Return retRetio
    End Function
    ''' <summary>
    ''' 各種数値を計算しDataRowを出力
    ''' </summary>
    ''' <param name="base"></param>
    ''' <param name="appendTargetTable"></param>
    ''' <returns></returns>
    Private Function CreateDispRow(base As DataTable, appendTargetTable As DataTable) As DataRow
        Dim maeRuikeiVolume As Decimal = (From dr As DataRow In base Select CDec(dr("MAERUIKEIVOLUME"))).Sum
        Dim ruikeiVolume As Decimal = (From dr As DataRow In base Select CDec(dr("RUIKEIVOLUME"))).Sum
        Dim volume As Decimal = (From dr As DataRow In base Select CDec(dr("VOLUME"))).Sum
        Dim volumeChange As Decimal = (From dr As DataRow In base Select CDec(dr("VOLUMECHANGE"))).Sum
        Dim lyVolumeChange As Decimal = (From dr As DataRow In base Select CDec(dr("LYVOLUMECHANGE"))).Sum

        Dim newRow As DataRow = appendTargetTable.NewRow
        newRow("MAERUIKEIVOLUME") = maeRuikeiVolume
        newRow("RUIKEIVOLUME") = ruikeiVolume
        newRow("VOLUME") = volume
        newRow("VOLUMECHANGE") = volumeChange
        newRow("LYVOLUMECHANGE") = lyVolumeChange
        '各種率の計算
        newRow("VOLUMERATIO") = CalcRatio(ruikeiVolume, volumeChange)
        newRow("LYVOLUMERATIO") = CalcRatio(ruikeiVolume, lyVolumeChange)

        Return newRow
    End Function

End Class
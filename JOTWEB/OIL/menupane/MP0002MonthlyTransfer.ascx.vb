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
            '処理フラグを落とす
            Me.hdnRefreshCall.Value = ""
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
                variableFields.AddRange({"MIDDLEOILCODE", "MIDDLEOILNAME", "TRAINCLASS", "TRAINCLASSNAME"})
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
    Private Sub SetDisplayValues()
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
            Return
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
        Select Case Me.ddlListPattern.SelectedValue
            Case "VIEW001"
                SetView001(dt)
            Case "VIEW002"
                SetView002(dt)
            Case "VIEW003"
                SetView003(dt)
        End Select
    End Sub
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
        Me.pnlNoData.Visible = False
    End Sub
    ''' <summary>
    ''' 営業所別ビューコンテンツ展開
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub SetView001(dt As DataTable)
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
        Else
            Me.chtMonthTrans.Visible = True
        End If
    End Sub
    ''' <summary>
    ''' 支店別画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Sub SetView002(dt As DataTable)
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
                    appendDr("TRAINCLASSNAME") = fstRow("TRAINCLASSNAME")
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
                appendDr("TRAINCLASSNAME") = fstRow("TRAINCLASSNAME")
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
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.Add(dtShiro)
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.Add(dtKuro)
        Else
            dispData.AddRange({dtSum, dtShiro, dtKuro})
        End If
        Me.repMonthTrans002.DataSource = dispData
        Me.repMonthTrans002.DataBind()
    End Sub
    ''' <summary>
    ''' 荷主別　請負輸送OT輸送合算画面展開
    ''' </summary>
    ''' <param name="dt">全て込みのデータ</param>
    Private Sub SetView003(dt As DataTable)
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
                appendDr("SHIPPERNAME") = fstRow("SHIPPERNAME")
                appendDr("BIGOILCODE") = fstRow("BIGOILCODE")
                appendDr("BIGOILNAME") = fstRow("BIGOILNAME")
                appendDr("TRAINCLASS") = ""
                appendDr("TRAINCLASSNAME") = "請負+OT"
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
                appendDr("TRAINCLASSNAME") = "請負+OT"
                dtSum.Rows.Add(appendDr)
            Next orgCode
            If Not (dtSum Is Nothing OrElse dtSum.Rows.Count = 0) Then
                '合計行の設定
                appendDr = CreateDispRow(targetDt, dtSum)
                appendDr("SHIPPERNAME") = "計"
                appendDr("BIGOILNAME") = "計"
                appendDr("TRAINCLASSNAME") = "請負+OT"
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
        ElseIf dtShiro.Rows.Count > 0 AndAlso dtKuro.Rows.Count = 0 Then
            dispData.Add(dtShiro)
        ElseIf dtShiro.Rows.Count = 0 AndAlso dtKuro.Rows.Count > 0 Then
            dispData.Add(dtKuro)
        Else
            dispData.AddRange({dtSum, dtShiro, dtKuro})
        End If
        'Me.repMonthTrans002.DataSource = dispData
        'Me.repMonthTrans002.DataBind()
    End Sub
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
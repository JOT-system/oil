Option Strict On
Imports System.Data.SqlClient
Imports System.Web.UI.DataVisualization.Charting
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 営業所別　列車牽引実績
''' </summary>
Public Class MP0009ActualTraction
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
            Try
                '初回ロード
                Initialize()
                Me.hdnCurrentOfficeCode.Value = Me.ddlActualTractionOffice.SelectedValue

            Catch ex As Exception
                pnlSysError.Visible = True
                Me.ddlActualTractionOffice.Enabled = False
                Me.ddlActualTractionArrStation.Enabled = False
                CS0011LOGWRITE.INFSUBCLASS = "MP0009ActualTraction"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "INIT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
            End Try
        Else
            Try
                'ポストバック
                If Me.hdnRefreshCall.Value = "1" Then
                    pnlSysError.Visible = False
                    '最新化処理
                    With Me.ddlActualTractionOffice
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    '着駅再取得
                    If Me.hdnCurrentOfficeCode.Value <> Me.ddlActualTractionOffice.SelectedValue Then
                        If Me.ddlActualTractionOffice.Items.Count > 0 Then
                            Me.ddlActualTractionArrStation.Items.Clear()
                            Dim arrStDdl As DropDownList = Me.GetArrTrainNoList(Me.ddlActualTractionOffice.SelectedValue)
                            Me.ddlActualTractionArrStation.Items.AddRange(arrStDdl.Items.Cast(Of ListItem).ToArray)
                            Dim cuurentSt As String = ""
                            Dim savedSelectedVal As String = ""
                            SetDdlDefaultValue(Me.ddlActualTractionArrStation, savedSelectedVal)
                        End If
                        Me.hdnCurrentOfficeCode.Value = Me.ddlActualTractionOffice.SelectedValue
                    End If
                    With Me.ddlActualTractionArrStation
                        Me.SaveCookie(.ClientID, .SelectedValue)
                    End With
                    SetDisplayValues()

                End If
                'ダウンロードボタン押下時処理
                If Me.hdnDownloadCall.Value = "1" Then
                    pnlSysError.Visible = False
                    'ダウンロードデータ取得
                    Dim dt As DataTable
                    Using sqlCon As SqlConnection = CS0050Session.getConnection
                        sqlCon.Open()
                        SqlConnection.ClearPool(sqlCon)
                        dt = GetDownloadListData(sqlCon)
                    End Using
                    '帳票生成
                    Dim tempFileName As String = String.Format("{0}_ACTUALTRACTION_{1}.xlsx", Me.ID, Me.ddlActualTractionOffice.SelectedValue)
                    Using clsPrint As New M00001MP0009ActualTraction(
                        Me.Page.Title, tempFileName, dt,
                        Me.ddlActualTractionOffice.SelectedValue, Me.ddlActualTractionOffice.SelectedItem.Text,
                        Me.ddlActualTractionArrStation.SelectedValue, Me.ddlActualTractionArrStation.SelectedItem.Text,
                        Me.ddlActualTractionYearMonth.SelectedItem.Text
                        )
                        clsPrint.CreateExcelFileStream(Me.Page)
                    End Using
                    SetDisplayValues()
                End If
                '処理フラグを落とす
                Me.hdnRefreshCall.Value = ""
            Catch ex As Exception
                pnlSysError.Visible = True
                Me.ddlActualTractionOffice.Enabled = False
                Me.ddlActualTractionArrStation.Enabled = False
                CS0011LOGWRITE.INFSUBCLASS = "MP0009ActualTraction"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "POSTBACK"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                '処理フラグを落とす
                Me.hdnRefreshCall.Value = ""
            End Try

        End If 'End IsPostBack = False
    End Sub
    ''' <summary>
    ''' 初期処理
    ''' </summary>
    Protected Sub Initialize()
        Me.lblPaneTitle.Text = "営業所別　列車牽引実績"
        Me.lblPaneDownloadTitle.Text = "月間列車別牽引実績のダウンロード"
        'MP0000Baseの共通処理の営業所抽出を呼出し営業所ドロップダウン生成
        Dim retDdl As DropDownList = Me.GetOfficeList()
        If retDdl.Items.Count > 0 Then
            Me.ddlActualTractionOffice.Items.AddRange(retDdl.Items.Cast(Of ListItem).ToArray)
            Dim savedSelectedVal As String = ""
            savedSelectedVal = Me.LoadCookie(ddlActualTractionOffice.ClientID)
            If savedSelectedVal = "" Then
                Me.ddlActualTractionOffice.SelectedIndex = retDdl.SelectedIndex
            Else
                SetDdlDefaultValue(Me.ddlActualTractionOffice, savedSelectedVal)
            End If

        End If
        '着駅ドロップダウンの生成
        If Me.ddlActualTractionOffice.Items.Count > 0 Then
            Dim arrStDdl As DropDownList = Me.GetArrTrainNoList(Me.ddlActualTractionOffice.SelectedValue)
            Me.ddlActualTractionArrStation.Items.AddRange(arrStDdl.Items.Cast(Of ListItem).ToArray)
            Dim cuurentSt As String = ""
            Dim savedSelectedVal As String = ""
            savedSelectedVal = Me.LoadCookie(Me.ddlActualTractionArrStation.ClientID)
            SetDdlDefaultValue(Me.ddlActualTractionArrStation, savedSelectedVal)
        End If
        '年月ドロップダウンの生成
        Dim ymDdl As New DropDownList
        Dim dt As Date = Now
        For pMonth As Integer = 0 To -12 Step -1
            ymDdl.Items.Add(dt.AddMonths(pMonth).ToString("yyyy/MM"))
        Next
        If ymDdl.Items.Count > 0 Then
            Me.ddlActualTractionYearMonth.Items.AddRange(ymDdl.Items.Cast(Of ListItem).ToArray)
            Dim savedSelectedVal As String = ""
            savedSelectedVal = Me.LoadCookie(ddlActualTractionYearMonth.ClientID)
            If savedSelectedVal = "" Then
                Me.ddlActualTractionYearMonth.SelectedIndex = ymDdl.SelectedIndex
            Else
                SetDdlDefaultValue(Me.ddlActualTractionYearMonth, savedSelectedVal)
            End If
        End If

        'グラフ情報の設定
        SetDisplayValues()

    End Sub
    ''' <summary>
    ''' 一覧表データ取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListData(sqlCon As SqlConnection) As DataTable
        Dim sqlTrainData As New StringBuilder
        sqlTrainData.AppendLine("SELECT TR.TRAINNO")
        sqlTrainData.AppendLine("  FROM OIL.OIM0007_TRAIN TR")
        sqlTrainData.AppendLine(" WHERE TR.DELFLG = @DELFLG")
        If Not Me.ddlActualTractionOffice.SelectedValue.Equals("ALL") Then
            sqlTrainData.AppendLine("   AND TR.OFFICECODE = @OFFICECODE")
        End If
        sqlTrainData.AppendLine("   AND TR.ARRSTATION = @ARRSTATION")
        sqlTrainData.AppendLine(" GROUP BY TR.TRAINNO")
        sqlTrainData.AppendLine(" ORDER BY CONVERT(int,TR.TRAINNO)")

        Dim sqlStat As New StringBuilder
        Dim retTbl As DataTable = Nothing

        '受注テーブルより列車番号を横軸としたクロス集計を行いグラフ用データを取得
        'またデータが無くても日付の範囲で出せるようカレンダーテーブルを結合する
        sqlStat.AppendLine("SELECT pvr.TARGETDATE  ")
        sqlStat.AppendLine("      ,{0}  ")
        sqlStat.AppendLine("  FROM (SELECT format(CAL.WORKINGYMD,'yyyy/MM/dd') AS TARGETDATE")
        sqlStat.AppendLine("              ,ODR.TRAINNO AS TRAINNO")
        sqlStat.AppendLine("              ,DTL.CARSNUMBER")
        sqlStat.AppendLine("          FROM COM.OIS0021_CALENDAR CAL with(nolock)")
        sqlStat.AppendLine("     LEFT JOIN OIL.OIT0002_ORDER ODR  with(nolock)")
        sqlStat.AppendLine("            ON CAL.WORKINGYMD  =  ODR.LODDATE")
        sqlStat.AppendLine("           AND CAL.DELFLG      =  @DELFLG")
        sqlStat.AppendLine("           AND ODR.DELFLG      =  @DELFLG")
        sqlStat.AppendLine("           AND ODR.ORDERSTATUS <> @ORDERSTATUS")
        sqlStat.AppendLine("           AND ODR.LODDATE IS NOT NULL")
        sqlStat.AppendLine("           AND ODR.OFFICECODE  = @OFFICECODE")
        sqlStat.AppendLine("     LEFT JOIN OIL.OIT0003_DETAIL DTL  with(nolock)")
        sqlStat.AppendLine("            ON ODR.ORDERNO = DTL.ORDERNO")
        sqlStat.AppendLine("           AND DTL.DELFLG     =  @DELFLG")
        sqlStat.AppendLine("         WHERE CAL.WORKINGYMD BETWEEN Getdate() -3 AND Getdate()")
        sqlStat.AppendLine("       ) pvbase")
        sqlStat.AppendLine(" PIVOT(SUM(CARSNUMBER) FOR [TRAINNO] IN ({1})) pvr")

        Using sqlCmd As New SqlCommand(sqlTrainData.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.ddlActualTractionOffice.SelectedValue
                .Add("@ARRSTATION", SqlDbType.NVarChar).Value = Me.ddlActualTractionArrStation.SelectedValue
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@ORDERSTATUS", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With
            Dim fieldList As String = ""
            Dim firldListWithIsNull As String = ""
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                '列車リストが取得出来ない場合はグラフデータなしのまま終了
                If sqlDr.HasRows = False Then
                    Return retTbl
                End If

                While sqlDr.Read
                    If fieldList <> "" Then
                        fieldList = fieldList & ","
                        firldListWithIsNull = firldListWithIsNull & ","
                    End If
                    fieldList = fieldList & "[" & Convert.ToString(sqlDr("TRAINNO")) & "]"
                    firldListWithIsNull = firldListWithIsNull & String.Format("IsNull([{0}],0) AS [{0}]", Convert.ToString(sqlDr("TRAINNO")))
                End While

            End Using 'sqlDr
            sqlCmd.CommandText = String.Format(sqlStat.ToString, firldListWithIsNull, fieldList)
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If IsNothing(retTbl) Then
                    retTbl = New DataTable

                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        retTbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                End If

                retTbl.Clear()
                retTbl.Load(sqlDr)
            End Using

        End Using 'sqlCmd
        Return retTbl
    End Function
    ''' <summary>
    ''' グラフ及び一覧表のデータを設定
    ''' </summary>
    Private Function SetDisplayValues() As DataTable
        Dim dt As DataTable
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            dt = GetListData(sqlCon)
        End Using

        With Me.chtActualTraction
            'Dim revData = dt
            'If dt IsNot Nothing OrElse dt.Rows.Count > 0 Then
            '    revData = (From dr As DataRow In dt Order By Convert.ToString(dr("TARGETDATE")) Descending).CopyToDataTable
            'End If
            .Series.Clear()
            'データ列が動的な為VB上で生成
            For Each revCol As DataColumn In dt.Columns
                If revCol.ColumnName = "TARGETDATE" Then
                    Continue For
                End If
                With .Series.Add("T" & revCol.ColumnName)
                    .ChartArea = "carActualTraction"
                    .ChartType = SeriesChartType.Column
                    '.Color = "#2F5197" '色は一旦自動でおまかせ。列車番号毎に必要なら別途定義
                    .XValueMember = "TARGETDATE"
                    .YValueMembers = revCol.ColumnName
                    '凡例名
                    .LegendText = revCol.ColumnName
                    .Legend = "legHan"
                    '棒グラフに値を表示
                    .IsValueShownAsLabel = True
                End With

            Next
            For Each dr As DataRow In dt.Rows
                dr("TARGETDATE") = CDate(dr("TARGETDATE")).ToString("M月d日")
            Next
            .DataSource = dt
            .DataBind()

        End With
        Return dt
    End Function

    ''' <summary>
    ''' 一覧表データ取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDownloadListData(sqlCon As SqlConnection) As DataTable

        Dim sqlStat As New StringBuilder
        Dim retTbl As DataTable = Nothing

        With sqlStat
            .AppendLine(" SELECT ")
            .AppendLine("     ODR.OFFICECODE                         AS OFFICECODE ")
            .AppendLine("   , DTL.SHIPPERSCODE                       AS SHIPPERCODE ")
            .AppendLine("   , ODR.ARRSTATION                         AS ARRSTATIONCODE ")
            .AppendLine("   , ODR.TRAINNO                            AS TRAINNO ")
            .AppendLine("   , FORMAT(ODR.LODDATE, 'yyyy/MM/dd')      AS LODDATE ")
            .AppendLine("   , FORMAT(ODR.DEPDATE, 'yyyy/MM/dd')      AS DEPDATE ")
            .AppendLine("   , DTL.OTTRANSPORTFLG                     AS OTTRANSPORTFLG ")
            .AppendLine("   , CASE DTL.OTTRANSPORTFLG ")
            .AppendLine("     WHEN 1 THEN ISNULL(SUM(DTL.CARSNUMBER), 0) ")
            .AppendLine("     WHEN 2 THEN ISNULL(SUM(DTL.CARSNUMBER), 0) ")
            .AppendLine("     ELSE 0 ")
            .AppendLine("     END                                    AS CARSNUMBER ")
            .AppendLine("   , ISNULL(MAX(CONVERT(INT, DTL.LINE)), 0) AS LINE ")
            .AppendLine(" FROM ")
            .AppendLine("   OIL.OIT0002_ORDER ODR WITH (NOLOCK) ")
            .AppendLine("   LEFT JOIN OIL.OIT0003_DETAIL DTL WITH (NOLOCK) ")
            .AppendLine("     ON ODR.ORDERNO = DTL.ORDERNO ")
            .AppendLine(" WHERE ")
            .AppendLine("   ODR.DELFLG = @DELFLG ")
            .AppendLine("   AND DTL.DELFLG = @DELFLG ")
            .AppendLine("   AND ( ")
            .AppendLine("     ODR.LODDATE BETWEEN @BEGINDATE AND @ENDDATE ")
            .AppendLine("     OR ODR.DEPDATE BETWEEN @BEGINDATE AND @ENDDATE ")
            .AppendLine("   ) ")
            .AppendLine("   AND ODR.OFFICECODE = @OFFICECODE ")
            .AppendLine("   AND ODR.ORDERSTATUS < @ORDERSTATUS ")
            .AppendLine(" GROUP BY ")
            .AppendLine("   ODR.OFFICECODE ")
            .AppendLine("   , DTL.SHIPPERSCODE ")
            .AppendLine("   , ODR.ARRSTATION ")
            .AppendLine("   , ODR.TRAINNO ")
            .AppendLine("   , ODR.LODDATE ")
            .AppendLine("   , ODR.DEPDATE ")
            .AppendLine("   , DTL.OTTRANSPORTFLG ")
            .AppendLine(" ORDER BY ")
            .AppendLine("   ODR.OFFICECODE ")
            .AppendLine("   , DTL.SHIPPERSCODE ")
            .AppendLine("   , ODR.ARRSTATION ")
            .AppendLine("   , ODR.TRAINNO ")
            .AppendLine("   , ODR.LODDATE ")
            .AppendLine("   , ODR.DEPDATE ")
            .AppendLine("   , DTL.OTTRANSPORTFLG ")
        End With

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            Dim strDt As String = String.Format("{0}/01", Me.ddlActualTractionYearMonth.SelectedValue)
            Dim dt As Date = Nothing
            If Not Date.TryParse(strDt, dt) Then
                dt = Now
            End If

            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.ddlActualTractionOffice.SelectedValue
                .Add("@BEGINDATE", SqlDbType.NVarChar).Value = dt.AddDays(-1).ToString("yyyy/MM/dd")
                .Add("@ENDDATE", SqlDbType.NVarChar).Value = dt.AddMonths(1).ToString("yyyy/MM/dd")
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@ORDERSTATUS", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
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
            End Using

        End Using 'sqlCmd
        Return retTbl
    End Function

End Class
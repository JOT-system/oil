﻿Option Strict On
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
            '初回ロード
            Initialize()
            Me.hdnCurrentOfficeCode.Value = Me.ddlActualTractionOffice.SelectedValue
        Else
            'ポストバック
            If Me.hdnRefreshCall.Value = "1" Then
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
            '処理フラグを落とす
            Me.hdnRefreshCall.Value = ""
        End If 'End IsPostBack = False
    End Sub
    ''' <summary>
    ''' 初期処理
    ''' </summary>
    Protected Sub Initialize()
        Me.lblPaneTitle.Text = "営業所別　列車牽引実績"
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
        sqlStat.AppendLine("            ON CAL.WORKINGYMD =  ODR.LODDATE")
        sqlStat.AppendLine("           AND CAL.DELFLG     =  @DELFLG")
        sqlStat.AppendLine("           AND ODR.DELFLG     =  @DELFLG")
        sqlStat.AppendLine("           AND ODR.LODDATE IS NOT NULL")
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
    Private Sub SetDisplayValues()
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
    End Sub
End Class
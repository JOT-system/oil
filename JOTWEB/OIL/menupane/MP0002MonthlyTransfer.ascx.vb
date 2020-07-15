Option Strict On
Imports System.Data.SqlClient
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
        Else
            'ポストバック
            If Me.hdnRefreshCall.Value = "1" Then
                '最新化処理
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
        'MP0000Baseの共通処理の営業所抽出を呼出し営業所ドロップダウン生成
        Dim retDdl As DropDownList = Me.GetOfficeList()
        If retDdl.Items.Count > 0 Then
            Me.ddlMonthTransOffice.Items.AddRange(retDdl.Items.Cast(Of ListItem).ToArray)
            Me.ddlMonthTransOffice.SelectedIndex = retDdl.SelectedIndex
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
        'FIXVALUEのPRODUCTPATTERNを油種の主体とする
        '受注データが無くとも全対象油種表示をさせる
        sqlStat.AppendLine("SELECT  FV.KEYCODE AS OILCODE ")
        sqlStat.AppendLine("      , FV.VALUE1  AS OILNAME ")
        sqlStat.AppendLine("      , ISNULL(SUMVAL.YESTERDAYVAL,0) AS YESTERDAYVAL")
        sqlStat.AppendLine("      , ISNULL(SUMVAL.YESTERDAYVAL,0) AS TODAYVAL")
        sqlStat.AppendLine("      , ISNULL(SUMVAL.TODAYVAL,0) - ISNULL(SUMVAL.YESTERDAYVAL,0)  AS TODAYTRANS")
        sqlStat.AppendLine("  FROM OIL.VIW0001_FIXVALUE FV")
        sqlStat.AppendLine("  LEFT JOIN  (SELECT ODD.OILCODE")
        sqlStat.AppendLine("                    ,ISNULL(SUM(CASE WHEN ORD.ACTUALDEPDATE = @TO_DATE THEN 0 ELSE ODD.CARSAMOUNT END),0) AS YESTERDAYVAL")
        sqlStat.AppendLine("                    ,ISNULL(SUM(ODD.CARSAMOUNT),0)                                                        AS TODAYVAL")
        sqlStat.AppendLine("                FROM      OIL.OIT0002_ORDER  ORD")
        sqlStat.AppendLine("               INNER JOIN OIL.OIT0003_DETAIL ODD")
        sqlStat.AppendLine("                       ON ORD.ORDERNO    = ODD.ORDERNO")
        sqlStat.AppendLine("                      AND ORD.OFFICECODE = @OFFICECODE")
        sqlStat.AppendLine("                      AND ORD.ACTUALDEPDATE BETWEEN  @FROM_DATE AND @TO_DATE")
        sqlStat.AppendLine("                      AND ORD.DELFLG     = @DELFLG")
        sqlStat.AppendLine("                      AND ODD.DELFLG     = @DELFLG")
        sqlStat.AppendLine("               GROUP BY ODD.OILCODE")
        sqlStat.AppendLine("             ) SUMVAL")
        sqlStat.AppendLine("         ON FV.KEYCODE = SUMVAL.OILCODE")
        sqlStat.AppendLine(" WHERE FV.CAMPCODE = @OFFICECODE")
        sqlStat.AppendLine("   AND FV.CLASS    = @CLASS")
        sqlStat.AppendLine("   AND FV.DELFLG   = @DELFLG")
        sqlStat.AppendLine(" ORDER BY FV.KEYCODE")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                '月初から現在の日付の範囲
                Dim fromDtm As Date = New Date(Now.Year, Now.Month, 1)
                Dim toDtm As Date = Now

                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.ddlMonthTransOffice.SelectedValue
                .Add("@CLASS", SqlDbType.NVarChar).Value = "PRODUCTPATTERN"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@FROM_DATE", SqlDbType.Date).Value = fromDtm
                .Add("@TO_DATE", SqlDbType.Date).Value = toDtm

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
        Dim dt As DataTable
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            dt = GetListData(sqlCon)
        End Using
        Me.repMonthTrans.DataSource = dt
        Me.repMonthTrans.DataBind()
    End Sub
End Class
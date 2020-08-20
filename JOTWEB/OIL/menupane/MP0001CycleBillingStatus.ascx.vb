Option Strict On
Imports System.Data.SqlClient
Imports System.Web.UI.DataVisualization.Charting
''' <summary>
''' 月締状況ユーザーコントロールクラス
''' </summary>
Public Class MP0001CycleBillingStatus
    Inherits MP0000Base
    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報
    '5当月5営業日までは前月を表示
    Private Const WorkDayToPrevMonth As Integer = 5
    Private Const MasterOfficeCode As String = "010007"
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
        SetDisplayValues()
    End Sub
    ''' <summary>
    ''' カレンダーテーブルより現在何営業日か取得し表示すべき月を判定する
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    Private Function GetTargetDay(sqlCon As SqlConnection) As Date
        Dim sqlStat As New StringBuilder
        Dim retDate As Date = New Date(Now.Year, Now.Month, 1)
        sqlStat.AppendLine("SELECT COUNT(*) AS WORKDAYCNT")
        sqlStat.AppendLine("  FROM COM.OIS0021_CALENDAR with(nolock)")
        sqlStat.AppendLine(" WHERE DELFLG = '0'")
        sqlStat.AppendLine("   AND DATEPART(YEAR,WORKINGYMD)  = DATEPART(YEAR,getdate())")
        sqlStat.AppendLine("   AND DATEPART(MONTH,WORKINGYMD) = DATEPART(MONTH,getdate())")
        sqlStat.AppendLine("   AND WORKINGYMD < getdate()")
        sqlStat.AppendLine("   AND WORKINGKBN = '0'")
        sqlStat.AppendLine("   AND WORKINGWEEK BETWEEN 1 AND 5")
        Dim sqlresult As Integer
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            Dim result = sqlCmd.ExecuteScalar()
            sqlresult = CInt(result)
        End Using

        If sqlresult <= WorkDayToPrevMonth Then
            retDate = retDate.AddMonths(-1)
        End If

        Return retDate
    End Function
    ''' <summary>
    ''' 対象リストの取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    Private Function GetBaseList(sqlCon As SqlConnection, ByRef bottomItem As ClosingItem) As Dictionary(Of String, ClosingItem)
        Dim retVal As New Dictionary(Of String, ClosingItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ORGCODE")
        sqlStat.AppendLine("      ,ORGNAME")
        sqlStat.AppendLine("      ,OFFICECODE")
        sqlStat.AppendLine("      ,OFFICENAME")
        sqlStat.AppendLine("      ,SORTORDER")
        sqlStat.AppendLine("  FROM OIL.VIW0010_BELONG_TO_OFFICE with(nolock)")
        sqlStat.AppendLine(" WHERE ORGCODE = @ORGCODE")
        sqlStat.AppendLine(" ORDER BY SORTORDER")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@ORGCODE", SqlDbType.NVarChar).Value = MasterOfficeCode
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = False Then
                    Return retVal
                End If

                While sqlDr.Read
                    If bottomItem Is Nothing Then
                        bottomItem = New ClosingItem()
                        bottomItem.Code = Convert.ToString(sqlDr("ORGCODE"))
                        bottomItem.Name = Convert.ToString(sqlDr("ORGNAME"))
                    End If
                    Dim code As String = Convert.ToString(sqlDr("OFFICECODE"))
                    Dim name As String = Convert.ToString(sqlDr("OFFICENAME")).Replace("営業所", "")
                    Dim sortOrder As String = Convert.ToString(sqlDr("SORTORDER"))
                    Dim keyCode As String = Left(sortOrder, 3)
                    If sortOrder.EndsWith("000") Then
                        Dim item As New ClosingItem
                        item.Code = code
                        item.Name = name
                        retVal.Add(keyCode, item)
                    ElseIf retVal.ContainsKey(keyCode) Then
                        Dim item As New ClosingItem
                        item.Code = code
                        item.Name = name
                        retVal(keyCode).ChildItem.Add(item)
                    End If
                End While
            End Using
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 一覧表データ取得
    ''' </summary>
    ''' <returns></returns>
    Private Function EditClosingStatus(sqlCon As SqlConnection, targetDay As Date, baseList As Dictionary(Of String, ClosingItem)) As Dictionary(Of String, ClosingItem)
        'ここで〆状態を取得しフラグを設定する予定
        Return baseList
    End Function
    ''' <summary>
    ''' グラフ及び一覧表のデータを設定
    ''' </summary>
    Private Sub SetDisplayValues()
        Dim targetList As Dictionary(Of String, ClosingItem)
        Dim bottomItem As ClosingItem = Nothing
        Dim targetDay As Date
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            targetDay = GetTargetDay(sqlCon)
            targetList = GetBaseList(sqlCon, bottomItem)
            targetList = EditClosingStatus(sqlCon, targetDay, targetList)
        End Using
        With Me.repBranch
            .DataSource = targetList
            .DataBind()
        End With
        If bottomItem IsNot Nothing Then
            Me.lblBottomItem.Text = String.Format("<span class='bottomitem' data-isclosed=""{0}"">{1}</span>", If(bottomItem.IsClosed, "True", ""), bottomItem.Name)
        End If
        Me.hdnTargetMonth.Value = targetDay.ToString("yyyy/MM/dd")
        Me.lblPaneTitle.Text = String.Format("{0:M月}締状況", targetDay)
    End Sub
    ''' <summary>
    ''' 支店・営業所〆情報格納クラス
    ''' </summary>
    Public Class ClosingItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ChildItem = New List(Of ClosingItem)
            Me.IsClosed = False
        End Sub

        ''' <summary>
        ''' 名称
        ''' </summary>
        ''' <returns></returns>
        Public Property [Name] As String
        ''' <summary>
        ''' オフィスコード
        ''' </summary>
        ''' <returns></returns>
        Public Property Code As String
        ''' <summary>
        ''' 〆済フラグ(True:〆,False:未)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsClosed As Boolean
        ''' <summary>
        ''' 支店にぶら下がる営業所〆情報を格納
        ''' </summary>
        ''' <returns></returns>
        Public Property ChildItem As List(Of ClosingItem)
    End Class
End Class
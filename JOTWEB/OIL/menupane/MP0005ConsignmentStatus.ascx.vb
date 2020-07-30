Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 託送指示送信状況コントロールクラス
''' </summary>
Public Class MP0005ConsignmentStatus
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
        Me.lblPaneTitle.Text = String.Format("{0:M月d日} 託送指示送信状況", Now)
        'MP0000Baseの共通処理の営業所抽出を呼出し営業所ドロップダウン生成
        Dim retDdl As DropDownList = Me.GetOfficeList()
        If retDdl.Items.Count > 0 Then
            Me.ddlConsignmentOffice.Items.AddRange(retDdl.Items.Cast(Of ListItem).ToArray)
            Me.ddlConsignmentOffice.SelectedIndex = retDdl.SelectedIndex
        End If
        SetDisplayValues()
    End Sub
    ''' <summary>
    ''' 画面表示する列車一覧を取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    Private Function GetBaseTrainList(sqlCon As SqlConnection) As List(Of LinkImportItem)
        Dim retVal As New List(Of LinkImportItem)
        Dim sqlStat As New StringBuilder
        'TODO 着駅で同一が複数、ソートまちまちなので要調整
        '     一旦列車番号
        sqlStat.AppendLine("SELECT TR.TRAINNO ")
        sqlStat.AppendLine("  FROM OIL.OIM0007_TRAIN TR")
        sqlStat.AppendLine(" WHERE TR.OFFICECODE  = @OFFICECODE")
        'sqlStat.AppendLine("   AND TR.OTFLG       = @OTFLG")
        'sqlStat.AppendLine("   AND TR.TRAINCLASS  IN(@TRAINCLASS_O,@TRAINCLASS_T)")
        sqlStat.AppendLine("   AND TR.DELFLG      = @DELFLG")
        sqlStat.AppendLine(" GROUP BY TR.TRAINNO")
        sqlStat.AppendLine(" ORDER BY CONVERT(INT,TRAINNO)")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.ddlConsignmentOffice.SelectedValue
                '.Add("@OTFLG", SqlDbType.NVarChar).Value = "1"
                '.Add("@TRAINCLASS_O", SqlDbType.NVarChar).Value = "O"
                '.Add("@TRAINCLASS_T", SqlDbType.NVarChar).Value = "T"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = False Then
                    Return retVal
                End If

                While sqlDr.Read
                    Dim rtrItm As New LinkImportItem
                    rtrItm.TrainNo = Convert.ToString(sqlDr("TRAINNO"))
                    rtrItm.Status = "-" '初期は未取込
                    retVal.Add(rtrItm)
                End While
            End Using

        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' ステータス状況を取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="targetList"></param>
    ''' <returns></returns>
    ''' <remarks>7/29時点でまだ未定</remarks>
    Private Function EditStatusFlag(sqlCon As SqlConnection, targetList As List(Of LinkImportItem)) As List(Of LinkImportItem)
        Return targetList
    End Function
    ''' <summary>
    ''' グラフ及び一覧表のデータを設定
    ''' </summary>
    Private Sub SetDisplayValues()
        Dim dispList As List(Of LinkImportItem)
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            dispList = GetBaseTrainList(sqlCon)
            dispList = EditStatusFlag(sqlCon, dispList)
        End Using

        With Me.repConsignmentItems
            .DataSource = dispList
            .DataBind()
        End With

    End Sub
    ''' <summary>
    ''' 貨車連結順序表取込状況用アイテムクラス
    ''' </summary>
    Public Class LinkImportItem
        ''' <summary>
        ''' 列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainNo As String = ""
        ''' <summary>
        ''' 状況フラグ "-":ハイフン,"C":円(Circle),"X":×(Xross),"T":三角(Triangle)
        ''' </summary>
        ''' <returns>"-":ハイフン,"C":円(Circle),"X":×(Xross),"T":三角(Triangle)</returns>
        Public Property Status As String = "-"
    End Class
End Class
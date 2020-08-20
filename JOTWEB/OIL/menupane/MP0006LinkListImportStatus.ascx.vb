Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' 貨車連結順序表取込状況コントロールクラス
''' </summary>
Public Class MP0006LinkListImportStatus
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
        Me.lblPaneTitle.Text = String.Format("{0:M月d日} 貨車連結順序表取込状況", Now)
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
        sqlStat.AppendLine("SELECT REPLACE(RTR.TRAINNAME,'レ','') AS TRAINNO ")
        sqlStat.AppendLine("     , MIN(RTR.OFFICECODE) AS OFFICECODE")
        sqlStat.AppendLine("     , MIN(RTR.LINECNT)    AS LINECNT")
        sqlStat.AppendLine("  FROM OIL.OIM0016_RTRAIN RTR with(nolock)")
        sqlStat.AppendLine(" WHERE RTR.IOKBN  = @IOKBN")
        sqlStat.AppendLine("   AND RTR.DELFLG = @DELFLG")
        sqlStat.AppendLine(" GROUP BY RTR.TRAINNAME")
        sqlStat.AppendLine(" ORDER BY OFFICECODE, LINECNT, TRAINNO")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@IOKBN", SqlDbType.NVarChar).Value = "I"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = False Then
                    Return retVal
                End If

                While sqlDr.Read
                    Dim rtrItm As New LinkImportItem
                    rtrItm.TrainNo = Convert.ToString(sqlDr("TRAINNO"))
                    rtrItm.Imported = False '初期は未取込
                    retVal.Add(rtrItm)
                End While
            End Using

        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 受信状況を取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="targetList"></param>
    ''' <returns></returns>
    ''' <remarks>7/29時点でまだ未定</remarks>
    Private Function EditImportedFlag(sqlCon As SqlConnection, targetList As List(Of LinkImportItem)) As List(Of LinkImportItem)
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
            dispList = EditImportedFlag(sqlCon, dispList)
        End Using

        With Me.repLinkListImportItems
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
        ''' 取込済フラグ
        ''' </summary>
        ''' <returns>True:取込済、False:未取込</returns>
        Public Property Imported As Boolean = False
    End Class
End Class
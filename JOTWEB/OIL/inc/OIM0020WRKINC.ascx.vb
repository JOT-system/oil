Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0020WRKINC
    Inherits System.Web.UI.UserControl
    Public Const MAPIDS As String = "OIM0020S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0020L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0020C"       'MAPID(更新)
    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub
    ''' <summary>
    ''' 対象フラグの初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Function GetNewDisplayFlags() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)
        retVal.Add(New DisplayFlag("社外", "OUTFLG", 0))
        retVal.Add(New DisplayFlag("石油部", "INFLG1", 1))
        retVal.Add(New DisplayFlag("東北支店", "INFLG2", 2))
        retVal.Add(New DisplayFlag("関東支店", "INFLG3", 3))
        retVal.Add(New DisplayFlag("中部支店", "INFLG4", 4))
        retVal.Add(New DisplayFlag("仙台新港営業所", "INFLG5", 5))
        retVal.Add(New DisplayFlag("五井営業所", "INFLG6", 6))
        retVal.Add(New DisplayFlag("甲子営業所", "INFLG7", 7))
        retVal.Add(New DisplayFlag("袖ヶ浦営業所", "INFLG8", 8))
        retVal.Add(New DisplayFlag("根岸営業所", "INFLG9", 9))
        retVal.Add(New DisplayFlag("四日市営業所", "INFLG10", 10))
        retVal.Add(New DisplayFlag("三重塩浜営業所", "INFLG11", 11))
        Return retVal
    End Function

    ''' <summary>
    ''' 掲載フラグ関連クラス
    ''' </summary>
    <Serializable>
    Public Class DisplayFlag
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="dispName">画面表示名</param>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispOrder">並び順</param>
        Public Sub New(dispName As String, fieldName As String, dispOrder As Integer)
            Me.DispName = dispName
            Me.FieldName = fieldName
            Me.DispOrder = dispOrder
        End Sub
        ''' <summary>
        ''' 表示名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispName As String
        ''' <summary>
        ''' 対象フィールド
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 表示順
        ''' </summary>
        ''' <returns></returns>
        Public Property DispOrder As Integer
        ''' <summary>
        ''' 表示グループ（仮）
        ''' </summary>
        ''' <returns></returns>
        Public Property Group As String = ""
        ''' <summary>
        ''' 選択フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Checked As Boolean = False
    End Class
End Class
Option Strict On
''' <summary>
''' メニューペイン基底クラス
''' </summary>
''' <remarks>メニューペインで共通で利用・設定する動作は当基底クラスに記述
''' 表示・非表示や並び順の設定はこちらで行っているので他で意識する必要なし
''' ※「このペインでこの場合」出す出さないが発生したら要検討</remarks>
Public Class MP0000Base
    Inherits System.Web.UI.UserControl
    Public Property TargetCustomPaneInfo As CS0050SESSION.UserMenuCostomItem
    ''' <summary>
    ''' ロード時処理(根底クラス)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub MP0000Base_Load(sender As Object, e As EventArgs) Handles Me.Load
        'セッション変数よりカスタムペイン情報一覧を取得
        Dim CS0050Session As New CS0050SESSION
        Dim customPaneList = CS0050Session.UserMenuCostomList
        Dim styleBase As String = "order: {0};"
        If customPaneList Is Nothing Then
            Me.Visible = False
            Return
        End If
        '継承先のIDと一致するカスタムペイン情報を特定
        Me.TargetCustomPaneInfo = (From plItm In customPaneList Where plItm.OutputId = Me.ID).FirstOrDefault
        If Me.TargetCustomPaneInfo Is Nothing Then
            Me.Visible = False
            Return
        End If
        'ロード時処理
        If Page.IsPostBack = False Then
            '初回ロード時(カスタムペイン情報が非表示ならVisibleを切って終了)
            If Me.TargetCustomPaneInfo.OnOff = False Then
                Me.Visible = False
            End If
            Dim mainPane As Panel = DirectCast(Me.FindControl("contentPane"), Panel)
            Dim orderObj As HiddenField = DirectCast(Me.FindControl("hdnPaneOrder"), HiddenField)
            orderObj.Value = Me.TargetCustomPaneInfo.SortNo.ToString
            mainPane.Attributes.Add("style", String.Format(styleBase, orderObj.Value))
        Else
            'ポストバック時(非表示ならなにもしない)
            If Me.Visible = False Then
                Return
            End If
            Dim mainPane As Panel = DirectCast(Me.FindControl("contentPane"), Panel)
            Dim orderObj As HiddenField = DirectCast(Me.FindControl("hdnPaneOrder"), HiddenField)
            mainPane.Attributes.Add("style", String.Format(styleBase, orderObj.Value))
        End If
    End Sub
End Class

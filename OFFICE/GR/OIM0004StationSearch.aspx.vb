Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL
Public Class OIM0004StationSearch
    Inherits System.Web.UI.Page

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then

        Else
            '★★★ 初期画面表示 ★★★
            Initialize()
        End If
    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '貨物駅コードテキストにフォーカスする。
        TxtGoodsStationCode.Focus()
        '貨物駅コードテキストを空にする。
        TxtGoodsStationCode.Text = ""
        '貨物コード枝番テキストを空にする。
        TxtGoodsStationCodeBranch.Text = ""

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

    End Sub

    Private Sub BtnSearch_Click(sender As Object, e As EventArgs) Handles BtnSearch.Click

    End Sub

    Private Sub BtnEnd_Click(sender As Object, e As EventArgs) Handles BtnEnd.Click

    End Sub
End Class
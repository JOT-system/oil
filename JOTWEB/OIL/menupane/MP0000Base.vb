Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' メニューペイン基底クラス
''' </summary>
''' <remarks>メニューペインで共通で利用・設定する動作は当基底クラスに記述
''' 表示・非表示や並び順の設定はこちらで行っているので他で意識する必要なし
''' ※「このペインでこの場合」出す出さないが発生したら要検討</remarks>
Public Class MP0000Base
    Inherits System.Web.UI.UserControl
    Public Property TargetCustomPaneInfo As CS0050SESSION.UserMenuCostomItem
    Private Const CONST_COOKIE_MENUPANE_PREFIX = "MP0000"

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
    ''' <summary>
    ''' オフィス選択用のコンボボックス取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>複数ペインで利用するため共通化</remarks>
    Public Function GetOfficeList() As DropDownList
        Dim letList As New DropDownList
        Using obj As GRIS0005LeftBox = DirectCast(LoadControl("~/inc/GRIS0005LeftBox.ascx"), GRIS0005LeftBox)
            Dim prmData As New Hashtable
            Dim patent = DirectCast(Me.Page.Master, OILMasterPage)
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = patent.USER_ORG
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_SALESOFFICE) = ""
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
            Dim WW_DUMMY As String = ""
            obj.SetListBox(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, WW_DUMMY, prmData)
            For Each listitm As ListItem In obj.WF_LeftListBox.Items
                Dim ddlItm As New ListItem(listitm.Text, listitm.Value)
                ddlItm.Selected = False
                letList.Items.Add(ddlItm)
            Next
            Dim foundItem As ListItem = letList.Items.FindByValue(patent.USER_ORG)
            If foundItem IsNot Nothing Then
                foundItem.Selected = True
            End If
        End Using
        Return letList
    End Function
    ''' <summary>
    ''' 営業所を元に列車に紐づく着駅コードを取得
    ''' </summary>
    ''' <param name="officeCode">営業所コード</param>
    ''' <returns></returns>
    Public Function GetArrTrainNoList(officeCode As String) As DropDownList
        Dim retList As New DropDownList
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TR.ARRSTATION AS STATIONCODE")
        sqlStat.AppendLine("      ,ISNULL(ST.STATONNAME,'') AS STATONNAME")
        sqlStat.AppendLine("  FROM      OIL.OIM0007_TRAIN TR  with(nolock)")
        sqlStat.AppendLine("  LEFT JOIN OIL.OIM0004_STATION ST  with(nolock)")
        sqlStat.AppendLine("    ON TR.ARRSTATION = ST.STATIONCODE + ST.BRANCH")
        sqlStat.AppendLine("   AND ST.DELFLG     = @DELFLG")
        sqlStat.AppendLine(" WHERE TR.OFFICECODE = @OFFICECODE")
        sqlStat.AppendLine("   AND TR.DELFLG     = @DELFLG")
        sqlStat.AppendLine(" GROUP BY TR.ARRSTATION,ST.STATONNAME")
        sqlStat.AppendLine(" ORDER BY STATIONCODE")
        Using sqlCon As New SqlConnection(CS0050Session.DBCon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = officeCode
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = False Then
                    Return retList
                End If
                While sqlDr.Read
                    Dim listItm As New ListItem(Convert.ToString(sqlDr("STATONNAME")), Convert.ToString(sqlDr("STATIONCODE")))
                    retList.Items.Add(listItm)
                End While
            End Using
        End Using
        Return retList
    End Function
    ''' <summary>
    ''' 月間輸送量ペイン用の一覧表の表示パターンを取得
    ''' </summary>
    ''' <returns></returns>
    Public Function GetMonthlyTransListPattern() As DropDownList
        Dim letList As New DropDownList
        Using obj As GRIS0005LeftBox = DirectCast(LoadControl("~/inc/GRIS0005LeftBox.ascx"), GRIS0005LeftBox)
            Dim prmData As New Hashtable
            Dim patent = DirectCast(Me.Page.Master, OILMasterPage)
            'prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = patent.USER_ORG
            'prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_SALESOFFICE) = ""
            'prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL

            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = "01"
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "MENUMONTHTRPAT"
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ADDITINALSORTORDER) = "VALUE5"

            Dim WW_DUMMY As String = ""
            obj.SetListBox(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WW_DUMMY, prmData)
            For Each listitm As ListItem In obj.WF_LeftListBox.Items
                Dim ddlItm As New ListItem(listitm.Text, listitm.Value)
                ddlItm.Selected = False
                letList.Items.Add(ddlItm)
            Next

        End Using
        Return letList
    End Function
    ''' <summary>
    ''' cookieにDDLの選択値を保持する
    ''' </summary>
    ''' <param name="key">クライアント描画のコントロールID</param>
    ''' <param name="value">設定値</param>
    Public Sub SaveCookie(ByVal key As String, ByVal value As String)

        'Dim prefixedKey As String = Me.Page.Title & key
        'Dim cookie As HttpCookie = Nothing
        'If Me.Page.Response.Cookies.AllKeys.Contains(prefixedKey) Then
        '    Me.Page.Response.Cookies.Remove(prefixedKey)
        'End If

        'cookie = New HttpCookie(prefixedKey)
        'cookie.Expires = DateTime.Now.AddYears(1) '一旦10年保持
        'cookie.Value = value

        'Me.Page.Response.Cookies.Add(cookie)
        SaveCookie(key, value, Me.Page)
    End Sub
    Public Shared Sub SaveCookie(ByVal key As String, ByVal value As String, pageObj As Page)
        Dim prefixedKey As String = pageObj.Title & key
        Dim cookie As HttpCookie = Nothing
        If pageObj.Response.Cookies.AllKeys.Contains(prefixedKey) Then
            pageObj.Response.Cookies.Remove(prefixedKey)
        End If

        cookie = New HttpCookie(prefixedKey)
        cookie.Expires = DateTime.Now.AddYears(1) '一旦10年保持
        cookie.Value = value

        pageObj.Response.Cookies.Add(cookie)
    End Sub

    ''' <summary>
    ''' cookieよりDDLの選択値を取得
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    Public Function LoadCookie(ByVal key As String) As String
        'Dim prefixedKey As String = Me.Page.Title & key
        'Dim retVal As String = ""
        'If Me.Page.Request.Cookies.AllKeys.Contains(prefixedKey) = False Then
        '    Return retVal
        'End If
        'Dim cookie As HttpCookie
        'cookie = Me.Page.Request.Cookies(prefixedKey)
        'retVal = cookie.Value
        'If Me.Page.Response.Cookies.AllKeys.Contains(prefixedKey) Then
        '    Me.Page.Response.Cookies.Remove(prefixedKey)
        'End If
        'cookie.Expires = DateTime.Now.AddYears(1)
        'Me.Page.Response.Cookies.Add(cookie)
        'Return retVal
        Return LoadCookie(key, Me.Page)
    End Function
    Public Shared Function LoadCookie(ByVal key As String, pageObj As Page) As String
        Dim prefixedKey As String = pageObj.Title & key
        Dim retVal As String = ""
        If pageObj.Request.Cookies.AllKeys.Contains(prefixedKey) = False Then
            Return retVal
        End If
        Dim cookie As HttpCookie
        cookie = pageObj.Request.Cookies(prefixedKey)
        retVal = cookie.Value
        If pageObj.Response.Cookies.AllKeys.Contains(prefixedKey) Then
            pageObj.Response.Cookies.Remove(prefixedKey)
        End If
        cookie.Expires = DateTime.Now.AddYears(1)
        pageObj.Response.Cookies.Add(cookie)
        Return retVal
    End Function


    ''' <summary>
    ''' ドロップダウンの初期値を設定する
    ''' </summary>
    ''' <param name="ddlObj"></param>
    ''' <param name="value"></param>
    Public Sub SetDdlDefaultValue(ByRef ddlObj As DropDownList, ByVal value As String)
        If ddlObj Is Nothing OrElse
           ddlObj.Items.Count = 0 Then
            Return
        End If

        If ddlObj.Items.FindByValue(value) IsNot Nothing Then
            ddlObj.SelectedValue = value
        Else
            ddlObj.SelectedIndex = 0
        End If

    End Sub
    ''' <summary>
    ''' 引数文字列をbase64エンコード文字に変換
    ''' </summary>
    ''' <param name="targetStr"></param>
    ''' <returns></returns>
    Public Shared Function GetBase64Str(targetStr As String) As String
        Dim encoding = System.Text.Encoding.UTF8
        Return Convert.ToBase64String(encoding.GetBytes(targetStr))
    End Function

End Class

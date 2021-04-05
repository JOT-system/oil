Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' メニュー画面クラス
''' </summary>
Public Class M00001MENU
    Inherits System.Web.UI.Page
    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報

    Public Property SelectedGuidanceNo As String = ""
    ''' <summary>
    '''  パスワードの変更依頼（期限切れまで何日前からか）
    ''' </summary>
    Private Const C_PASSWORD_CHANGE_LIMIT_COUNT As Integer = 31
    Private Const C_VSNAME_LEFTNAVIDATA As String = "VS_MENU_LEFT_NAVI"

    ''' <summary>
    ''' ページロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '左ナビの開閉状態をcookieに記憶（Initializeで復元します）
            Me.LeftNavCollectToSaveCookie()
            If "01".Equals(Master.USERCAMP) Then
                'ドロップダウンリスト選択値の保存
                Me.ddlReportNameList.SelectedIndex = Integer.Parse(Me.ddlReportNameList_LaIdx.Value)
                Me.ddlTrOfficeNameList.SelectedIndex = Integer.Parse(Me.ddlTrOfficeNameList_LaIdx.Value)
            End If

            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                If WF_ButtonClick.Value.StartsWith("WF_ButtonShowGuidance") Then
                    WF_ButtonShowGuidance_Click()
                    Return
                End If
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonLeftNavi"
                        BtnLeftNavi_Click()
                    '以下、左ボックス処理用
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        FIELD_Change()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        ButtonSel_Click()
                    Case "WF_DownLoadReport"            '帳票ダウンロード
                        BtnDownLoadReport()
                End Select
            End If
            'ドロップダウンリスト選択変更
            If Not String.IsNullOrEmpty(WF_SelectChangeDdl.Value) Then
                Select Case WF_SelectChangeDdl.Value
                    Case Me.ddlReportNameList.ID
                        ChangeReportNameList()
                End Select
            End If
        Else
            '★★★ 初期画面表示 ★★★
            Initialize()
            WF_ButtonClick.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        Master.MAPID = GRM00001WRKINC.MAPID
        Dim menuButtonList As List(Of MenuItem) = Nothing
        Using sqlCon As SqlConnection = CS0050Session.getConnection
            sqlCon.Open()
            'メニューボタン情報の取得
            Try
                menuButtonList = GetMenuItemList(sqlCon)
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0008_UPROFMAP SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                Return
            End Try
            '取得したデータを画面に展開
            ViewState(C_VSNAME_LEFTNAVIDATA) = menuButtonList
            Me.repLeftNav.DataSource = menuButtonList
            Me.repLeftNav.DataBind()
            'ガイダンスマスタの表示
            'ガイダンスデータ取得
            Try
                Dim guidanceDt As DataTable = GetGuidanceData(sqlCon)
                Me.repGuidance.DataSource = guidanceDt
                Me.repGuidance.DataBind()
                If guidanceDt.Rows.Count = 0 Then
                    guidanceArea.Visible = False
                End If
            Catch ex As Exception
            End Try
            '帳票ダウンロードエリアの初期化
            Try
                InitReportDLArea()
            Catch ex As Exception
            End Try
        End Using
    End Sub
    ''' <summary>
    ''' 左ナビゲーションボタン押下時処理
    ''' </summary>
    Protected Sub BtnLeftNavi_Click()
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap
        Dim leftNaviList = DirectCast(ViewState(C_VSNAME_LEFTNAVIDATA), List(Of MenuItem))
        'ありえないがメニュー表示リストが存在しない場合はそのまま終了
        If leftNaviList Is Nothing OrElse
           IsNumeric(Me.hdnPosiCol.Value) = False OrElse
           IsNumeric(Me.hdnRowLine.Value) = False Then
            Return
        End If
        Dim posiRow As Integer = CInt(Me.hdnRowLine.Value)
        Dim posiCol As Integer = CInt(Me.hdnPosiCol.Value)
        Dim rowLine As Integer = CInt(Me.hdnRowLine.Value)
        Me.hdnPosiCol.Value = ""
        Me.hdnRowLine.Value = ""
        Dim menuItm As MenuItem = Nothing
        Dim qMenuItm = From itm In leftNaviList Where itm.PosiCol = posiCol
        If rowLine = 1 Then
            menuItm = qMenuItm.FirstOrDefault
        Else
            If qMenuItm.Any Then
                menuItm = (From itm In qMenuItm(0).ChildMenuItem Where itm.RowLine = rowLine).FirstOrDefault
            End If
        End If
        'ありえないが選択したメニューアイテムが存在しない場合はそのまま終了
        If menuItm Is Nothing Then
            Return
        End If
        '★★★ ボタン押下時、画面遷移（左） ★★★

        '○画面遷移権限チェック（左）
        CS0007CheckAuthority.MAPID = menuItm.MapId
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = menuItm.MapId
                CS0050Session.VIEW_MAP_VARIANT = menuItm.Variant
                CS0050Session.MAP_ETC = ""

                Master.MAPvariant = menuItm.Variant
                Master.MAPID = menuItm.MapId
                Master.MAPpermitcode = CS0007CheckAuthority.MAPPERMITCODE
                Master.POSICOL = posiCol.ToString()
                Master.POSIROW = posiRow.ToString()
                Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
                Master.ShowMessage()

            Else
                Master.Output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "画面:" & menuItm.MapId)
                Master.ShowMessage()

                Exit Sub
            End If
        Else
            Master.Output(CS0007CheckAuthority.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & menuItm.MapId)
            Master.ShowMessage()

            Exit Sub
        End If
        'セッション変数クリア
        Dim eraseSessionNames As New List(Of String) From {"Selected_STYMD", "Selected_ENDYMD",
            "Selected_USERIDFrom", "Selected_USERIDTo", "Selected_USERIDG1", "Selected_USERIDG2", "Selected_USERIDG3", "Selected_USERIDG4", "Selected_USERIDG5",
            "Selected_MAPIDPFrom", "Selected_MAPIDPTo", "Selected_MAPIDPG1", "Selected_MAPIDPG2", "Selected_MAPIDPG3", "Selected_MAPIDPG4", "Selected_MAPIDPG5",
            "Selected_MAPIDFrom", "Selected_MAPIDTo", "Selected_MAPIDG1", "Selected_MAPIDG2", "Selected_MAPIDG3", "Selected_MAPIDG4", "Selected_MAPIDG5"}

        For Each eraseSessionName In eraseSessionNames
            HttpContext.Current.Session(eraseSessionName) = ""
        Next

        'ボタン押下時、画面遷移
        Server.Transfer(menuItm.Url)
    End Sub
    ''' <summary>
    ''' ガイダンスリンク押下時
    ''' </summary>
    Private Sub WF_ButtonShowGuidance_Click()
        Dim guidanceNo As String = WF_ButtonClick.Value.Replace("WF_ButtonShowGuidance", "")
        Me.SelectedGuidanceNo = guidanceNo
        'ボタン押下時、画面遷移
        Server.Transfer(Me.WF_HdnGuidanceUrl.Value)
    End Sub
    ''' <summary>
    ''' メニューボタン情報を取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetMenuItemList(sqlCon As SqlConnection) As List(Of MenuItem)
        Dim retItm As New List(Of MenuItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT A.POSICOL")
        sqlStat.AppendLine("      ,A.POSIROW AS ROWLINE")
        sqlStat.AppendLine("      ,rtrim(isnull(A.MAPID,''))      as MAPID")
        sqlStat.AppendLine("      ,rtrim(isnull(A.VARIANT,''))    as VARIANT")
        sqlStat.AppendLine("      ,rtrim(isnull(A.TITLENAMES,'')) as TITLE")
        sqlStat.AppendLine("      ,rtrim(isnull(A.MAPNAMES,''))   as NAMES")
        sqlStat.AppendLine("      ,rtrim(isnull(A.MAPNAMEL,''))   as NAMEL")
        sqlStat.AppendLine("      ,rtrim(isnull(B.URL,''))        as URL")
        sqlStat.AppendLine("  FROM      COM.OIS0008_PROFMMAP           A")
        sqlStat.AppendLine("  LEFT JOIN COM.OIS0007_URL                B")
        sqlStat.AppendLine("    ON B.MAPID    = A.MAPID")
        sqlStat.AppendLine("   AND B.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND B.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND B.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" WHERE A.CAMPCODE = @CAMPCODE")
        sqlStat.AppendLine("   AND A.MAPIDP   = @MAPIDP")
        sqlStat.AppendLine("   AND A.VARIANTP = @VARIANTP")
        sqlStat.AppendLine("   AND A.TITLEKBN = 'I'")
        sqlStat.AppendLine("   AND A.STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND A.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND A.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY A.POSICOL,A.POSIROW")
        Using dt As New DataTable
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                With sqlCmd.Parameters
                    .Add("@CAMPCODE", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                    .Add("@MAPIDP", SqlDbType.NVarChar, 50).Value = Master.MAPID
                    .Add("@VARIANTP", SqlDbType.NVarChar, 50).Value = Master.ROLE_MENU
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        dt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    dt.Load(sqlDr)
                    sqlDr.Close()
                End Using 'sqlDr
            End Using 'sqlCmd
            '取得結果を元にメニューアイテムクラスに格納
            '上位リストのみを取得()
            Dim topLevelList = From dr As DataRow In dt Where dr("ROWLINE").Equals(1)
            Dim childItems As List(Of DataRow) = Nothing
            '上位回送のリストループROWLINEが"1"のみ
            For Each topLevelItm In topLevelList
                Dim posiCol As Integer = CInt(topLevelItm("POSICOL"))
                childItems = (From dr As DataRow In dt Where dr("POSICOL").Equals(posiCol) AndAlso Not dr("ROWLINE").Equals(1)).ToList

                Dim retTopLevelItm = New MenuItem
                retTopLevelItm.PosiCol = CInt(topLevelItm("POSICOL"))
                retTopLevelItm.RowLine = CInt(topLevelItm("ROWLINE"))
                retTopLevelItm.MapId = Convert.ToString(topLevelItm("MAPID"))
                retTopLevelItm.Variant = Convert.ToString(topLevelItm("VARIANT"))
                retTopLevelItm.Title = Convert.ToString(topLevelItm("TITLE"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMES"))
                retTopLevelItm.Names = Convert.ToString(topLevelItm("NAMEL"))
                retTopLevelItm.Url = Convert.ToString(topLevelItm("URL"))

                If childItems.Count = 0 Then
                    '子供を完全に持たない
                    '一応意味はないがコケると困るので
                    If retTopLevelItm.Url = "" Then
                        retTopLevelItm.Url = "~/OIL/ex/page_404.html"
                    End If

                ElseIf childItems.Count = 1 Then
                    With childItems(0)
                        If retTopLevelItm.MapId = "" Then
                            retTopLevelItm.MapId = Convert.ToString(.Item("MAPID"))
                        End If
                        If retTopLevelItm.Variant = "" Then
                            retTopLevelItm.Variant = Convert.ToString(.Item("VARIANT"))
                        End If
                        If retTopLevelItm.Title = "" Then
                            retTopLevelItm.Title = Convert.ToString(.Item("TITLE"))
                        End If
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = Convert.ToString(.Item("URL"))
                        End If
                        If retTopLevelItm.Url = "" Then
                            retTopLevelItm.Url = "~/OIL/ex/page_404.html"
                        End If
                    End With
                Else
                    '名前が無ければ子供の先頭の名称を付与
                    With childItems(0)
                        If retTopLevelItm.Names = "" Then
                            retTopLevelItm.Names = Convert.ToString(.Item("NAMES"))
                        End If
                        If retTopLevelItm.Namel = "" Then
                            retTopLevelItm.Namel = Convert.ToString(.Item("NAMEL"))
                        End If
                    End With
                    For Each childItem In childItems
                        Dim retChildItm = New MenuItem
                        retChildItm.PosiCol = CInt(childItem("POSICOL"))
                        retChildItm.RowLine = CInt(childItem("ROWLINE"))
                        retChildItm.MapId = Convert.ToString(childItem("MAPID"))
                        retChildItm.Variant = Convert.ToString(childItem("VARIANT"))
                        retChildItm.Title = Convert.ToString(childItem("TITLE"))
                        retChildItm.Names = Convert.ToString(childItem("NAMES"))
                        retChildItm.Namel = Convert.ToString(childItem("NAMEL"))
                        retChildItm.Url = Convert.ToString(childItem("URL"))
                        If retChildItm.Url = "" Then
                            retChildItm.Url = "~/OIL/ex/page_404.html"
                        End If
                        retTopLevelItm.ChildMenuItem.Add(retChildItm)
                    Next childItem

                End If
                childItems = Nothing
                If retTopLevelItm.Names = "" Then
                    retTopLevelItm.Names = "　"
                End If

                Dim keyName As String = MP0000Base.GetBase64Str(retTopLevelItm.Names)
                Dim val As String = MP0000Base.LoadCookie(keyName, Me)
                Dim isOpen As Boolean = False
                If val <> "" Then
                    isOpen = Convert.ToBoolean(val)
                End If
                retTopLevelItm.OpenChild = isOpen
                retItm.Add(retTopLevelItm)
            Next topLevelItm

        End Using 'dt
        Return retItm

    End Function
    ''' <summary>
    ''' 表示用のガイダンスデータ取得
    ''' </summary>
    ''' <param name="sqlCon">SQLConnection</param>
    ''' <returns>ガイダンスデータ</returns>
    Private Function GetGuidanceData(sqlCon As SqlConnection) As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            .Add("GUIDANCENO", GetType(String))
            .Add("ENTRYDATE", GetType(String))
            .Add("TYPE", GetType(String))
            .Add("TITLE", GetType(String))
            .Add("NAIYOU", GetType(String))
            .Add("FILE1", GetType(String))
        End With
        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT GD.GUIDANCENO")
            sqlStat.AppendLine("      ,format(GD.INITYMD,'yyyy/M/d') AS ENTRYDATE")
            sqlStat.AppendLine("      ,GD.TYPE                       AS TYPE")
            sqlStat.AppendLine("      ,GD.TITLE                      AS TITLE")
            sqlStat.AppendLine("      ,GD.NAIYOU                     AS NAIYOU")
            sqlStat.AppendLine("      ,GD.FILE1                      AS FILE1")
            sqlStat.AppendLine("  FROM oil.OIM0020_GUIDANCE GD")
            sqlStat.AppendLine(" WHERE GETDATE() BETWEEN GD.FROMYMD AND GD.ENDYMD")
            sqlStat.AppendLine("   AND DELFLG = @DELFLG_NO")
            sqlStat.AppendLine("   AND OUTFLG <> '1'")
            Dim userOrg = Master.USER_ORG
            If Not {"jot_oil_1", "jot_sys_1"}.Contains(CS0050Session.VIEW_MENU_MODE) Then
                Dim targetDispFlags = OIM0020WRKINC.GetNewDisplayFlags
                Dim showDispFlag = (From flg In targetDispFlags Where flg.OfficeCode = userOrg Select flg.FieldName).FirstOrDefault
                If showDispFlag <> "" Then
                    sqlStat.AppendFormat("   AND {0} = '1'", showDispFlag).AppendLine()
                Else
                    sqlStat.AppendLine("   AND 1 = 2")
                End If
            End If
            sqlStat.AppendLine(" ORDER BY (CASE WHEN GD.TYPE = 'E' THEN '1'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'W' THEN '2'")
            sqlStat.AppendLine("                WHEN GD.TYPE = 'I' THEN '3'")
            sqlStat.AppendLine("                ELSE '9'")
            sqlStat.AppendLine("            END)")
            sqlStat.AppendLine("          ,GD.INITYMD DESC")
            '他のフラグや最大取得件数（条件がある場合）はあとで
            Using sqlGuidCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlGuidCmd.Parameters.Add("@DELFLG_NO", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlGuidDr As SqlDataReader = sqlGuidCmd.ExecuteReader()
                    Dim dr As DataRow
                    While sqlGuidDr.Read
                        dr = retDt.NewRow
                        dr("GUIDANCENO") = sqlGuidDr("GUIDANCENO")
                        dr("ENTRYDATE") = sqlGuidDr("ENTRYDATE")
                        dr("TYPE") = sqlGuidDr("TYPE")
                        dr("TITLE") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("TITLE")))
                        dr("NAIYOU") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("NAIYOU"))).Replace(ControlChars.CrLf, "<br />").Replace(ControlChars.Cr, "<br />").Replace(ControlChars.Lf, "<br />")
                        dr("FILE1") = Convert.ToString(sqlGuidDr("FILE1"))

                        retDt.Rows.Add(dr)
                    End While
                End Using

            End Using
            sqlStat = New StringBuilder
            sqlStat.AppendLine("SELECT URL.URL")
            sqlStat.AppendLine("  FROM COM.OIS0007_URL URL")
            sqlStat.AppendLine(" WHERE URL.MAPID = @MAPID")
            sqlStat.AppendLine("   AND GETDATE() BETWEEN URL.STYMD AND URL.ENDYMD")
            sqlStat.AppendLine("   AND URL.DELFLG = @DELFLG")

            Using sqlGuidUrlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlGuidUrlCmd.Parameters
                    .Add("@MAPID", SqlDbType.NVarChar).Value = OIM0020WRKINC.MAPIDC
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Dim urlVal = sqlGuidUrlCmd.ExecuteScalar
                Me.WF_HdnGuidanceUrl.Value = Convert.ToString(urlVal)
            End Using
        Catch ex As Exception
            Return retDt
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' 左ナビの開閉状態をcookieに保存
    ''' </summary>
    Private Sub LeftNavCollectToSaveCookie()
        '左ナビの表示アイテムが無い場合は終了
        If Me.repLeftNav Is Nothing OrElse Me.repLeftNav.Items.Count = 0 Then
            Return
        End If
        For Each repItm As RepeaterItem In Me.repLeftNav.Items
            Dim chkObj As CheckBox = DirectCast(repItm.FindControl("chkTopItem"), CheckBox)
            If chkObj Is Nothing Then
                Continue For
            End If

            Dim keyName As String = MP0000Base.GetBase64Str(chkObj.Text)
            Dim val As String = Convert.ToString(chkObj.Checked)
            MP0000Base.SaveCookie(keyName, val, Me)
        Next repItm
    End Sub

    ''' <summary>
    ''' 帳票DLエリア初期化
    ''' </summary>
    Private Sub InitReportDLArea()

        '所属会社がJOT以外の場合は帳票出力エリアを表示しない
        If Not "01".Equals(Master.USERCAMP) Then
            Me.reportDLAreaPane.Visible = False
            Exit Sub
        End If

        '帳票リスト初期化
        Me.ddlReportNameList.Items.AddRange(Me.GetReportNameList.Cast(Of ListItem).ToArray())
        Me.ddlReportNameList.Attributes.Add("onchange", "selectChangeDdl('" + Me.ddlReportNameList.ClientID + "');")
        Me.ddlReportNameList.SelectedIndex = 0
        Me.ddlReportNameList_LaIdx.Value = "0"

        '帳票条件ペインの初期化
        ChangeReportNameList(True)

    End Sub

    ''' <summary>
    ''' 帳票名変更時処理
    ''' </summary>
    ''' <param name="initFlag">初期化時のみ設定</param>
    Private Sub ChangeReportNameList(Optional initFlag As Boolean = False)

        '帳票条件ペインの表示初期化
        Me.transportResultCondPane.Visible = False

        '帳票条件ぺインの表示変更
        If CONST_REPORTNAME_TRANSPORT_RESULT.Equals(Me.ddlReportNameList.SelectedItem.Text) Then
            '輸送実績表条件パネル表示
            Me.transportResultCondPane.Visible = True
        End If

        '帳票条件ぺインの表示変更
        If CONST_REPORTNAME_TANK_TRANSPORT_RESULT.Equals(Me.ddlReportNameList.SelectedItem.Text) OrElse
            CONST_REPORTNAME_TANK_TRANSPORT_RESULT_ARR.Equals(Me.ddlReportNameList.SelectedItem.Text) Then
            '輸送実績表条件パネル表示
            Me.TankTransportResultCondPane.Visible = True
        End If

        '初期化フラグON
        If initFlag Then
            '------------'
            ' 初期化処理 '
            '------------'
            '〇輸送実績表
            '期間開始日(当月月初)
            Me.txtTrStYmd.Text = New Date(Now.Year, Now.Month, 1).ToString("yyyy/MM/dd")
            '期間終了日(当日)
            Me.txtTrEdYmd.Text = Now.ToString("yyyy/MM/dd")
            '営業所選択リスト
            Me.ddlTrOfficeNameList.Attributes.Add("onchange", "selectChangeDdl('" + Me.ddlTrOfficeNameList.ClientID + "');")
            Me.InitTrOfficeNameList()

            '〇タンク車運賃実績表
            '期間開始日(当月月初)
            Me.txtTtrStYmd.Text = New Date(Now.Year, Now.Month, 1).ToString("yyyy/MM/dd")
            '期間終了日(当日)
            Me.txtTtrEdYmd.Text = Now.ToString("yyyy/MM/dd")
            '営業所選択リスト
            Me.ddlTtrOfficeNameList.Attributes.Add("onchange", "selectChangeDdl('" + Me.ddlTtrOfficeNameList.ClientID + "');")
            Me.InitTtrOfficeNameList()
            '種別
            Me.ddlTtrTypeList.Attributes.Add("onchange", "selectChangeDdl('" + Me.ddlTtrTypeList.ClientID + "');")
            Me.InitTtrTypeList()
        End If
    End Sub

    ''' <summary>
    ''' 帳票名リスト取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetReportNameList() As ArrayList
        Dim list As ArrayList = New ArrayList()
        Dim codeIndex As Integer = 1

        list.Add(New ListItem(CONST_REPORTNAME_TRANSPORT_RESULT, String.Format("{0:999}", codeIndex)))
        codeIndex += 1
        list.Add(New ListItem(CONST_REPORTNAME_TANK_TRANSPORT_RESULT, String.Format("{0:999}", codeIndex)))
        codeIndex += 1
        list.Add(New ListItem(CONST_REPORTNAME_TANK_TRANSPORT_RESULT_ARR, String.Format("{0:999}", codeIndex)))
        'codeIndex += 1

        Return list
    End Function

    ''' <summary>
    ''' (輸送実績表)営業所リスト初期化
    ''' </summary>
    Private Sub InitTrOfficeNameList()

        Using obj As GRIS0005LeftBox = DirectCast(LoadControl("~/inc/GRIS0005LeftBox.ascx"), GRIS0005LeftBox)
            Dim prmData As New Hashtable
            Dim ddlList As New DropDownList
            Dim selectedIdx As Integer = 0
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = Me.Master.USER_ORG
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_SALESOFFICE) = ""
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
            Dim wkDUMMY As String = ""
            obj.SetListBox(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, wkDUMMY, prmData)

            For Each listitm As ListItem In obj.WF_LeftListBox.Items
                Dim ddlItm As New ListItem(listitm.Text, listitm.Value)
                If listitm.Value.Equals(Me.Master.USER_ORG) Then
                    ddlItm.Selected = True
                Else
                    ddlItm.Selected = False
                End If
                ddlList.Items.Add(ddlItm)
            Next
            Me.ddlTrOfficeNameList.Items.AddRange(ddlList.Items.Cast(Of ListItem).ToArray)
            Me.ddlTrOfficeNameList_LaIdx.Value = Me.ddlTrOfficeNameList.SelectedIndex.ToString
        End Using

    End Sub

    ''' <summary>
    ''' (タンク車運賃実績表)営業所リスト初期化
    ''' </summary>
    Private Sub InitTtrOfficeNameList()

        Using obj As GRIS0005LeftBox = DirectCast(LoadControl("~/inc/GRIS0005LeftBox.ascx"), GRIS0005LeftBox)
            Dim prmData As New Hashtable
            Dim ddlList As New DropDownList
            Dim selectedIdx As Integer = 0
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = Me.Master.USER_ORG
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_SALESOFFICE) = ""
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
            Dim wkDUMMY As String = ""
            obj.SetListBox(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, wkDUMMY, prmData)

            For Each listitm As ListItem In obj.WF_LeftListBox.Items
                Dim ddlItm As New ListItem(listitm.Text, listitm.Value)
                If listitm.Value.Equals(Me.Master.USER_ORG) Then
                    ddlItm.Selected = True
                Else
                    ddlItm.Selected = False
                End If
                ddlList.Items.Add(ddlItm)
            Next
            Me.ddlTtrOfficeNameList.Items.AddRange(ddlList.Items.Cast(Of ListItem).ToArray)
            Me.ddlTtrOfficeNameList_LaIdx.Value = Me.ddlTrOfficeNameList.SelectedIndex.ToString
        End Using

    End Sub

    ''' <summary>
    ''' (タンク車運賃実績表)種別リスト初期化
    ''' </summary>
    Private Sub InitTtrTypeList()

        Dim ddlList As New DropDownList
        Dim selectedIdx As Integer = 0

        Dim ddlItm As New ListItem("往路所定運賃", "1")
        ddlItm.Selected = True
        ddlList.Items.Add(ddlItm)
        ddlItm = New ListItem("往路割引後運賃", "2")
        ddlItm.Selected = False
        ddlList.Items.Add(ddlItm)

        Me.ddlTtrTypeList.Items.AddRange(ddlList.Items.Cast(Of ListItem).ToArray)
        Me.ddlTtrTypeList_LaIdx.Value = Me.ddlTrOfficeNameList.SelectedIndex.ToString

    End Sub

    ''' <summary>
    ''' 帳票ダウンロード
    ''' </summary>
    Private Sub BtnDownLoadReport()
        Try
            '輸送実績表
            If CONST_REPORTNAME_TRANSPORT_RESULT.Equals(ddlReportNameList.SelectedItem.Text) Then
                Using clsPrint As New OIT0008CustomReport(
                        CONST_MAPID_COST_MANAGEMENT,        '費用管理
                        CONST_TEMPNAME_TRANSPORT_RESULT,    '輸送実績表
                        Me.GetTransportResultData)
                    '帳票出力＆ファイルパス取得
                    WF_PrintURL.Value = clsPrint.CreateExcelPrintData_TansportResult(
                        CDate(txtTrStYmd.Text), CDate(txtTrEdYmd.Text))
                    '帳票ダウンロード
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
            End If

            'タンク車輸送実績表
            If CONST_REPORTNAME_TANK_TRANSPORT_RESULT.Equals(ddlReportNameList.SelectedItem.Text) Then
                Dim tempName As String = CONST_TEMPNAME_TANK_TRANSPORT_RESULT
                '営業所が仙台新港営業所の場合は、仙台テンプレートを使用
                If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                    tempName = CONST_TEMPNAME_TANK_TRANSPORT_RESULT_010402
                End If

                Using clsPrint As New OIT0008CustomReport(
                        CONST_MAPID_COST_MANAGEMENT,    '費用管理
                        tempName,                       'タンク車輸送実績表
                        Me.GetTankTransportResultData)
                    '帳票出力＆ファイルパス取得
                    If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                        'タンク車運賃実績表-列車別-仙台
                        WF_PrintURL.Value = clsPrint.CreateExcelPrintData_TankTansportResult_010402(
                            CDate(txtTtrStYmd.Text), CDate(txtTtrEdYmd.Text),
                            Integer.Parse(ddlTtrTypeList.SelectedValue))
                    Else
                        'タンク車運賃実績表-列車別-仙台以外
                        WF_PrintURL.Value = clsPrint.CreateExcelPrintData_TankTansportResult(
                            CDate(txtTtrStYmd.Text), CDate(txtTtrEdYmd.Text),
                            Integer.Parse(ddlTtrTypeList.SelectedValue))
                    End If

                    '帳票ダウンロード
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
            End If

            'タンク車輸送実績表（着駅別）
            If CONST_REPORTNAME_TANK_TRANSPORT_RESULT_ARR.Equals(ddlReportNameList.SelectedItem.Text) Then
                Dim tempName As String = CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR
                '営業所が仙台新港営業所の場合は、仙台テンプレートを使用
                If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                    tempName = CONST_TEMPNAME_TANK_TRANSPORT_RESULT_ARR_010402
                End If

                Using clsPrint As New OIT0008CustomReport(
                        CONST_MAPID_COST_MANAGEMENT,    '費用管理
                        tempName,                       'タンク車輸送実績表（着駅別）
                        Me.GetTankTransportResultData)
                    '帳票出力＆ファイルパス取得
                    If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                        WF_PrintURL.Value = clsPrint.CreateExcelPrintData_TankTansportResult_Arr_010402(
                            CDate(txtTtrStYmd.Text), CDate(txtTtrEdYmd.Text),
                            Integer.Parse(ddlTtrTypeList.SelectedValue))
                    Else
                        'タンク車運賃実績表-着駅別-仙台以外
                        WF_PrintURL.Value = clsPrint.CreateExcelPrintData_TankTansportResult_Arr(
                            CDate(txtTtrStYmd.Text), CDate(txtTtrEdYmd.Text),
                            Integer.Parse(ddlTtrTypeList.SelectedValue))
                    End If
                    '帳票ダウンロード
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
            End If

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "M00001MENU"   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "BtnDownLoadReport"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()

            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "帳票出力に失敗しました。")
        End Try

    End Sub

    ''' <summary>
    ''' 輸送実績表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetTransportResultData() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New SqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure
                SQLcmd.CommandText = "[oil].[GET_TRANSPORT_RESULT]"
                SQLcmd.Parameters.Clear()
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", SqlDbType.Date)             ' 累計開始日
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@EDYMD", SqlDbType.Date)             ' 累計終了日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.VarChar, 6)  ' 営業所コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 1000)  ' メッセージ
                Dim RV As SqlParameter = SQLcmd.Parameters.Add("ReturnValue", SqlDbType.Int)            ' 戻り値

                PARA1.Value = CDate(txtTrStYmd.Text)
                PARA2.Value = CDate(txtTrEdYmd.Text)
                PARA3.Value = ddlTrOfficeNameList.SelectedValue

                PARA4.Direction = ParameterDirection.Output
                RV.Direction = ParameterDirection.ReturnValue

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ''' <summary>
    ''' タンク車輸送実績表データ取得
    ''' </summary>
    ''' <returns>DataTable</returns>
    Private Function GetTankTransportResultData() As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open()

            Using SQLcmd As New SqlCommand
                SQLcmd.Connection = SQLcon
                SQLcmd.CommandType = CommandType.StoredProcedure

                'プロシージャ名
                If CONST_REPORTNAME_TANK_TRANSPORT_RESULT.Equals(ddlReportNameList.SelectedItem.Text) Then
                    '帳票名が「タンク車運賃実績表」の場合、列車別データ取得プロシージャを呼び出す
                    If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                        '営業所が「仙台新港営業所」の場合、仙台用データ取得プロシージャを呼び出す
                        SQLcmd.CommandText = "[oil].[GET_TANK_TRANSPORT_RESULT_010402]"
                    Else
                        SQLcmd.CommandText = "[oil].[GET_TANK_TRANSPORT_RESULT]"
                    End If
                Else
                    '帳票名が「タンク車運賃実績表（着駅別）」の場合、着駅別データ取得プロシージャを呼び出す
                    If CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                        '営業所が「仙台新港営業所」の場合、仙台用データ取得プロシージャを呼び出す
                        SQLcmd.CommandText = "[oil].[GET_TANK_TRANSPORT_RESULT_ARR_010402]"
                    Else
                        SQLcmd.CommandText = "[oil].[GET_TANK_TRANSPORT_RESULT_ARR]"
                    End If
                End If

                SQLcmd.Parameters.Clear()
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", SqlDbType.Date)             ' 累計開始日
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@EDYMD", SqlDbType.Date)             ' 累計終了日
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@TYPE", SqlDbType.Int)               ' 種別
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@MESSAGE", SqlDbType.VarChar, 1000)  ' メッセージ
                Dim RV As SqlParameter = SQLcmd.Parameters.Add("ReturnValue", SqlDbType.Int)            ' 戻り値

                PARA1.Value = CDate(txtTtrStYmd.Text)
                PARA2.Value = CDate(txtTtrEdYmd.Text)
                PARA4.Value = Integer.Parse(ddlTtrTypeList.SelectedValue)
                PARA5.Direction = ParameterDirection.Output
                RV.Direction = ParameterDirection.ReturnValue

                '営業所が「仙台新港営業所」以外の場合、営業所コードをパラメータに付与
                If Not CONST_OFFICECODE_010402.Equals(ddlTtrOfficeNameList.SelectedValue) Then
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", SqlDbType.VarChar, 6)  ' 営業所コード
                    PARA3.Value = ddlTtrOfficeNameList.SelectedValue
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    dt.Load(SQLdr)
                End Using

            End Using

        End Using

        Return dt
    End Function

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    Protected Sub FIELD_DBClick()
        Dim mValue As Integer

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, mValue)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case mValue
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case txtTrStYmd.ID     '(輸送実績表)期間FROM
                                .WF_Calendar.Text = txtTrStYmd.Text
                            Case txtTrEdYmd.ID     '(輸送実績表)期間FROM
                                .WF_Calendar.Text = txtTrEdYmd.Text
                            Case txtTtrStYmd.ID    '(タンク車運賃実績表)期間FROM
                                .WF_Calendar.Text = txtTtrStYmd.Text
                            Case txtTtrEdYmd.ID    '(タンク車運賃実績表)期間FROM
                                .WF_Calendar.Text = txtTtrEdYmd.Text
                        End Select
                        .ActiveCalendar()
                End Select
            End With

        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub FIELD_Change()

    End Sub

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ButtonSel_Click()

        'Dim wkSelectValue As String = ""
        'Dim wkSelectText As String = ""
        'Dim wkSelectedIndex As Integer = 0
        Dim wkDate As Date

        '○ 選択内容を取得
        'If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
        '    wkSelectedIndex = leftview.WF_LeftListBox.SelectedIndex
        '    WF_SelectedIndex.Value = wkSelectedIndex.ToString
        '    wkSelectValue = leftview.WF_LeftListBox.Items(wkSelectedIndex).Value
        '    wkSelectText = leftview.WF_LeftListBox.Items(wkSelectedIndex).Text
        'End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case txtTrStYmd.ID  '(輸送実績表)期間FROM
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        txtTrStYmd.Text = ""
                    Else
                        txtTrStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                txtTrStYmd.Focus()

            Case txtTrEdYmd.ID  '(輸送実績表)期間TO
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        txtTrEdYmd.Text = ""
                    Else
                        txtTrEdYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                txtTrEdYmd.Focus()

            Case txtTtrStYmd.ID '(タンク車運賃実績表)期間FROM
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        txtTtrStYmd.Text = ""
                    Else
                        txtTtrStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                txtTtrStYmd.Focus()

            Case txtTtrEdYmd.ID '(タンク車運賃実績表)期間TO
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        txtTtrEdYmd.Text = ""
                    Else
                        txtTtrEdYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                txtTtrEdYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "txtTrStYmd"
                txtTrStYmd.Focus()
            Case "txtTrEdYmd"
                txtTrEdYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 画面表示用遷移ボタンアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class MenuItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ChildMenuItem = New List(Of MenuItem)
            Me.OpenChild = False
        End Sub
        ''' <summary>
        ''' 列表示(PROFMAP:POSICOL)
        ''' </summary>
        ''' <returns></returns>
        Public Property PosiCol As Integer
        ''' <summary>
        ''' 行位置(PROFMAP:POSIROW) ⇒ 親クラスリストとして利用する場合は"1"のみ、子で再帰利用している箇所は"1"以外
        ''' </summary>
        ''' <returns></returns>
        Public Property RowLine As Integer
        ''' <summary>
        ''' 画面ＩＤ(PROFMAP:MAPID)
        ''' </summary>
        ''' <returns></returns>
        Public Property MapId As String
        ''' <summary>
        ''' 変数(PROFMAP:VARIANT)
        ''' </summary>
        ''' <returns></returns>
        Public Property [Variant] As String
        ''' <summary>
        ''' タイトル名称(PROFMAP:TITLENAMES)⇒左ナビのCSSクラス名として設定
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title As String
        ''' <summary>
        ''' 画面名称（短）(PROFMAP:MAPNAMES) ⇒ ボタン名称に設定
        ''' </summary>
        ''' <returns></returns>
        Public Property Names As String
        ''' <summary>
        ''' 画面名称（長）(PROFMAP:MAPNAMEL) ⇒ 現状当プロパティに投入のみ未使用
        ''' </summary>
        ''' <returns></returns>
        Public Property Namel As String
        ''' <summary>
        ''' URL（URLマスタ：URL）チルダ付き（アプリルート相対）の遷移URL
        ''' </summary>
        ''' <returns></returns>
        Public Property Url As String
        ''' <summary>
        ''' POSICOLが同一でROWLINが1以外の子データを格納
        ''' </summary>
        ''' <returns></returns>
        Public Property ChildMenuItem As List(Of MenuItem)
        ''' <summary>
        ''' 子要素の表示状態
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>現状未使用：ポストバック発生時に閉じてしまったら利用検討</remarks>
        Public Property OpenChild As Boolean = False

        ''' <summary>
        ''' 子要素を持っているか（デザイン判定用：▼表示判定）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>ある程度「孫・ひ孫」対応できる構造だが現状「子」のみ</remarks>
        Public ReadOnly Property HasChild As Boolean
            Get
                If ChildMenuItem Is Nothing OrElse ChildMenuItem.Count = 0 Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property
        ''' <summary>
        ''' メニュー２遷移可否（デザイン判定用：▶表示判定）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsMenu2Link As Boolean
            Get
                '遷移先URLがM00002MENU.aspxで終わればメニュー２と判定
                If Me.Title = "Master" Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
        ''' <summary>
        ''' 次ページ遷移情報を持つか(True：次画面遷移あり、False：次画面遷移無し)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasNextPageInfo As Boolean
            Get
                'MAPIDを持つか持たないかで判定
                If Me.MapId.Trim.Equals("") Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

    End Class

#Region "ViewStateを圧縮 個人用ペインでかなり大きくなると予想"
    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
#End Region
End Class
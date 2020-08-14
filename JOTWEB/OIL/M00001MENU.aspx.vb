Option Strict On
Imports System.Data.SqlClient
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
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                If WF_ButtonClick.Value.StartsWith("WF_ButtonShowGuidance") Then
                    WF_ButtonShowGuidance_Click()
                    Return
                End If
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonLeftNavi"
                        BtnLeftNavi_Click()
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
                If Me.Url.EndsWith("M00002MENU.aspx") Then
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
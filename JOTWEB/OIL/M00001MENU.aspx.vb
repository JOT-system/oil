Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' メインメニュー画面クラス
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
    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If IsPostBack Then
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                If WF_ButtonClick.Value.StartsWith("WF_ButtonShowGuidance") Then
                    WF_ButtonShowGuidance_Click()
                End If
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
        '★★★ メニュー貼り付け ★★★

        '○メニュー貼り付け（左）
        Dim WW_Select_CNT As String = String.Empty

        '検索SQL文 最大２１行で取得できたものを当て込むように修正する
        Dim SQLStr As String =
                          "WITH ROWIDX(ROWLINE)  AS (          " _
                        & " SELECT                             " _
                        & "      1               AS ROWLINE    " _
                        & " UNION ALL                          " _
                        & " SELECT                             " _
                        & "      ROWLINE + 1     AS ROWLINE    " _
                        & " FROM  ROWIDX                       " _
                        & " WHERE ROWLINE <= 6                " _
                        & ")                                   " _
                        & " SELECT                             " _
                        & "      rtrim(R.ROWLINE)               as SEQ     , " _
                        & "      rtrim(isnull(A.MAPID,''))      as MAPID   , " _
                        & "      rtrim(isnull(A.VARIANT,''))    as VARIANT , " _
                        & "      rtrim(isnull(A.TITLENAMES,'')) as TITLE   , " _
                        & "      rtrim(isnull(A.MAPNAMES,''))   as NAMES   , " _
                        & "      rtrim(isnull(A.MAPNAMEL,''))   as NAMEL   , " _
                        & "      rtrim(isnull(B.URL,''))        as URL       " _
                        & " FROM      ROWIDX                      R          " _
                        & " LEFT JOIN COM.OIS0008_PROFMMAP              A       ON " _
                        & "       A.CAMPCODE = @P1                           " _
                        & "   and A.MAPIDP   = @P2                           " _
                        & "   and A.VARIANTP = @P3                           " _
                        & "   and A.TITLEKBN = 'I'                           " _
                        & "   and A.POSICOL  = @P4                           " _
                        & "   and A.STYMD   <= @P5                           " _
                        & "   and A.ENDYMD  >= @P6                           " _
                        & "   and A.DELFLG  <> @P7                           " _
                        & "   and A.POSIROW  = R.ROWLINE                     " _
                        & " LEFT JOIN COM.OIS0007_URL                   B       ON " _
                        & "       B.MAPID    = A.MAPID                       " _
                        & "   and B.STYMD   <= @P5                           " _
                        & "   and B.ENDYMD  >= @P6                           " _
                        & "   and B.DELFLG  <> @P7                           " _
                        & " ORDER BY R.ROWLINE                               "
        '　１回目（ユーザＩＤ）での貼り付け
        Using SQLcon As SqlConnection = CS0050Session.getConnection,
              SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Try
                'DataBase接続文字
                SQLcon.Open() 'DataBase接続(Open)
                '固定パラメータ
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = work.WF_SEL_CAMPCODE.Text
                    .Add("@P2", SqlDbType.NVarChar, 50).Value = Master.MAPID
                    .Add("@P3", SqlDbType.NVarChar, 50).Value = Master.ROLE_MENU
                    .Add("@P5", SqlDbType.Date).Value = Date.Now
                    .Add("@P6", SqlDbType.Date).Value = Date.Now
                    .Add("@P7", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                '動的パラメータ
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)

                Dim dicMenuBoxes As New Dictionary(Of String, Repeater) From
                    {{"1", Repeater_Menu_L}, {"2", Repeater_Menu_L2},
                     {"3", Repeater_Menu_R}, {"4", Repeater_Menu_R2},
                     {"5", Repeater_Menu_L3}, {"6", Repeater_Menu_L4},
                     {"7", Repeater_Menu_R3}, {"8", Repeater_Menu_R4}}

                For Each menuBox In dicMenuBoxes
                    WW_Select_CNT = ""
                    PARA4.Value = menuBox.Key
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.HasRows = True Then
                            menuBox.Value.DataSource = SQLdr
                            menuBox.Value.DataBind()
                            WW_Select_CNT = "OK"
                        Else
                            WW_Select_CNT = "NG"
                        End If
                    End Using
                Next menuBox

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0008_UPROFMAP SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "S0008_UPROFMAP SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()
                Exit Sub
            End Try
            'ガイダンスデータ取得
            Try
                Dim guidanceDt As DataTable = GetGuidanceData(SQLcon)
                Me.repGuidance.DataSource = guidanceDt
                Me.repGuidance.DataBind()
            Catch ex As Exception
            End Try

            '■■■ パスワード有効期限の警告表示 ■■■
            '○パスワード有効期限の警告表示
            Dim WW_ENDYMD As Date = Date.Now

            Try

                'S0014_USER検索SQL文
                Dim SQL_Str As String =
                     "SELECT PASSENDYMD " _
                   & " FROM  COM.OIS0005_USERPASS " _
                   & " Where USERID = @P1 " _
                   & "   and DELFLG <> @P2 "
                Using USERcmd As New SqlCommand(SQL_Str, SQLcon)
                    With USERcmd.Parameters
                        .Add("@P1", SqlDbType.NVarChar, 20).Value = CS0050Session.USERID
                        .Add("@P2", SqlDbType.NVarChar, 1).Value = "1"
                    End With
                    Dim SQLdr As SqlDataReader = USERcmd.ExecuteReader()

                    While SQLdr.Read
                        WW_ENDYMD = CDate(SQLdr("PASSENDYMD"))
                        Exit While
                    End While
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0005_USERPASS SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "OIS0005_USERPASS SELECT"                '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try


            If DateDiff("d", Date.Now, WW_ENDYMD) < C_PASSWORD_CHANGE_LIMIT_COUNT Then
                Master.Output(C_MESSAGE_NO.PASSWORD_INVALID_AT_SOON, C_MESSAGE_TYPE.INF)
            End If

        End Using

    End Sub
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
            .Add("TITTLE", GetType(String))
            .Add("NAIYOU", GetType(String))
            .Add("FAILE1", GetType(String))
        End With
        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT GD.GUIDANCENO")
            sqlStat.AppendLine("      ,format(GD.INITYMD,'yyyy/M/d') AS ENTRYDATE")
            sqlStat.AppendLine("      ,GD.TYPE                       AS TYPE")
            sqlStat.AppendLine("      ,GD.TITTLE                     AS TITTLE")
            sqlStat.AppendLine("      ,GD.NAIYOU                     AS NAIYOU")
            sqlStat.AppendLine("      ,GD.FAILE1                     AS FAILE1")
            sqlStat.AppendLine("  FROM oil.OIM0020_GUIDANCE GD")
            sqlStat.AppendLine(" WHERE GETDATE() BETWEEN GD.FROMYMD AND GD.ENDYMD")
            sqlStat.AppendLine("   AND DELFLG = @DELFLG_NO")
            sqlStat.AppendLine("   AND OUTFLG <> '1'")
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
                        dr("TITTLE") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("TITTLE")))
                        dr("NAIYOU") = HttpUtility.HtmlEncode(Convert.ToString(sqlGuidDr("NAIYOU")))
                        dr("FAILE1") = Convert.ToString(sqlGuidDr("FAILE1"))

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
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' Repeater_Menu_x バインドイベント(Handlesに含めたオブジェクトが対象)
    ''' </summary>
    ''' <param name="sender">イベント発生オブジェクト</param>
    ''' <param name="e"></param>
    Protected Sub RptInfo_ItemDataBound_L(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) _
        Handles Repeater_Menu_L.ItemDataBound, Repeater_Menu_L2.ItemDataBound, Repeater_Menu_R.ItemDataBound,
                Repeater_Menu_R2.ItemDataBound, Repeater_Menu_L3.ItemDataBound, Repeater_Menu_L4.ItemDataBound,
                Repeater_Menu_R3.ItemDataBound, Repeater_Menu_R4.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        Dim dicSuffixList As New Dictionary(Of String, String) From {{"Repeater_Menu_L", "L"}, {"Repeater_Menu_L2", "L2"},
                                                                     {"Repeater_Menu_R", "R"}, {"Repeater_Menu_R2", "R2"},
                                                                     {"Repeater_Menu_L3", "L3"}, {"Repeater_Menu_L4", "L4"},
                                                                     {"Repeater_Menu_R3", "R3"}, {"Repeater_Menu_R4", "R4"}}
        Dim callRep As Repeater = DirectCast(sender, Repeater)
        Dim repFieldSuffix As String = dicSuffixList(callRep.ID)

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            Dim repItem As Common.DbDataRecord = DirectCast(e.Item.DataItem, System.Data.Common.DbDataRecord)
            Dim menuLabel As Label = DirectCast(e.Item.FindControl(String.Format("WF_MenuLabe_{0}", repFieldSuffix)), Label)
            Dim menuVari As Label = DirectCast(e.Item.FindControl(String.Format("WF_MenuVARI_{0}", repFieldSuffix)), Label)
            Dim menuUrl As Label = DirectCast(e.Item.FindControl(String.Format("WF_MenuURL_{0}", repFieldSuffix)), Label)
            Dim menuMap As Label = DirectCast(e.Item.FindControl(String.Format("WF_MenuMAP_{0}", repFieldSuffix)), Label)
            Dim menuButton As Button = DirectCast(e.Item.FindControl(String.Format("WF_MenuButton_{0}", repFieldSuffix)), Button)

            menuLabel.Text = Convert.ToString(repItem("TITLE"))
            menuVari.Text = Convert.ToString(repItem("VARIANT"))
            If Convert.ToString(repItem("URL")) = "" Then
                menuUrl.Text = String.Empty
            Else
                menuUrl.Text = Convert.ToString(repItem("URL"))
            End If
            menuMap.Text = Convert.ToString(repItem("MAPID"))
            menuButton.Text = "  " & Convert.ToString(repItem("NAMES"))

            If Convert.ToString(repItem("TITLE")) = "" Then
                If Convert.ToString(repItem("NAMES")) = "" Then
                    menuLabel.Text = "　　"
                    menuLabel.Visible = False
                    menuVari.Visible = False
                    menuButton.Visible = False
                    menuUrl.Visible = False
                    menuMap.Visible = False
                Else
                    menuLabel.Visible = False
                    menuVari.Visible = False
                    menuButton.Visible = True
                    menuUrl.Visible = False
                    menuMap.Visible = False
                End If
            Else
                menuLabel.Visible = True
                menuVari.Visible = False
                menuButton.Visible = False
                menuUrl.Visible = False
                menuMap.Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub
    ''' <summary>
    ''' Repeater_Menu_X ボタン押下時処理
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="e"></param>
    Protected Sub Repeater_Menu_ItemCommand_L(source As Object, e As RepeaterCommandEventArgs) _
        Handles Repeater_Menu_L.ItemCommand, Repeater_Menu_L2.ItemCommand, Repeater_Menu_R.ItemCommand,
                Repeater_Menu_R2.ItemCommand, Repeater_Menu_L3.ItemCommand, Repeater_Menu_L4.ItemCommand,
                Repeater_Menu_R3.ItemCommand, Repeater_Menu_R4.ItemCommand

        Dim dicSuffixList As New Dictionary(Of String, String) From {{"Repeater_Menu_L", "L"}, {"Repeater_Menu_L2", "L2"},
                                                                     {"Repeater_Menu_R", "R"}, {"Repeater_Menu_R2", "R2"},
                                                                     {"Repeater_Menu_L3", "L3"}, {"Repeater_Menu_L4", "L4"},
                                                                     {"Repeater_Menu_R3", "R3"}, {"Repeater_Menu_R4", "R4"}}
        Dim callRep As Repeater = DirectCast(source, Repeater)
        Dim repFieldSuffix As String = dicSuffixList(callRep.ID)

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex
        Dim repItem As RepeaterItem = callRep.Items(WW_COUNT)
        Dim WW_URL As Label = DirectCast(repItem.FindControl(String.Format("WF_MenuURL_{0}", repFieldSuffix)), Label)
        Dim WW_VARI As Label = DirectCast(repItem.FindControl(String.Format("WF_MenuVARI_{0}", repFieldSuffix)), Label)
        Dim WW_MAPID As Label = DirectCast(repItem.FindControl(String.Format("WF_MenuMAP_{0}", repFieldSuffix)), Label)

        '○画面遷移権限チェック（左）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""

                Master.MAPvariant = WW_VARI.Text
                Master.MAPID = WW_MAPID.Text
                Master.MAPpermitcode = CS0007CheckAuthority.MAPPERMITCODE
                Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
                Master.ShowMessage()

            Else
                Master.Output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
                Master.ShowMessage()

                Exit Sub
            End If
        Else
            Master.Output(CS0007CheckAuthority.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & WW_MAPID.Text)
            Master.ShowMessage()

            Exit Sub
        End If
        'セッション変数クリア
        HttpContext.Current.Session("Selected_STYMD") = ""
        HttpContext.Current.Session("Selected_ENDYMD") = ""

        HttpContext.Current.Session("Selected_USERIDFrom") = ""
        HttpContext.Current.Session("Selected_USERIDTo") = ""
        HttpContext.Current.Session("Selected_USERIDG1") = ""
        HttpContext.Current.Session("Selected_USERIDG2") = ""
        HttpContext.Current.Session("Selected_USERIDG3") = ""
        HttpContext.Current.Session("Selected_USERIDG4") = ""
        HttpContext.Current.Session("Selected_USERIDG5") = ""

        HttpContext.Current.Session("Selected_MAPIDPFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDPTo") = ""
        HttpContext.Current.Session("Selected_MAPIDPG1") = ""
        HttpContext.Current.Session("Selected_MAPIDPG2") = ""
        HttpContext.Current.Session("Selected_MAPIDPG3") = ""
        HttpContext.Current.Session("Selected_MAPIDPG4") = ""
        HttpContext.Current.Session("Selected_MAPIDPG5") = ""

        HttpContext.Current.Session("Selected_MAPIDFrom") = ""
        HttpContext.Current.Session("Selected_MAPIDTo") = ""
        HttpContext.Current.Session("Selected_MAPIDG1") = ""
        HttpContext.Current.Session("Selected_MAPIDG2") = ""
        HttpContext.Current.Session("Selected_MAPIDG3") = ""
        HttpContext.Current.Session("Selected_MAPIDG4") = ""
        HttpContext.Current.Session("Selected_MAPIDG5") = ""
        'ボタン押下時、画面遷移
        Server.Transfer(WW_URL.Text)

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
End Class
Imports System.Data.SqlClient

Public Class M00002MENU
    Inherits System.Web.UI.Page

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報


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
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonBackToMenu"
                        WF_ButtonBackToMenu_Click()
                End Select
            End If
        Else
            '★★★ 初期画面表示 ★★★
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' メニューへ戻るボタン押下時処理
    ''' </summary>
    Public Sub WF_ButtonBackToMenu_Click()
        '本当はURLマスタですが一旦固定
        Server.Transfer("~/OIL/M00001MENU.aspx")
        CS0050Session.VIEW_MAPID = "M00001"
        'Me.MAPID = "M00001"
    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        Master.MAPID = GRM00002WRKINC.MAPID
        '★★★ メニュー貼り付け ★★★

        '○メニュー貼り付け（左）
        Dim WW_Select_CNT As String = String.Empty

        '　１回目（ユーザＩＤ）での貼り付け
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            Try
                'DataBase接続文字
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文 最大２１行で取得できたものを当て込むように修正する
                Dim SQLStr As String =
                          "WITH ROWIDX(ROWLINE)  AS (          " _
                        & " SELECT                             " _
                        & "      1               AS ROWLINE    " _
                        & " UNION ALL                          " _
                        & " SELECT                             " _
                        & "      ROWLINE + 1     AS ROWLINE    " _
                        & " FROM  ROWIDX                       " _
                        & " WHERE ROWLINE <= 30                " _
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
                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 1)

                '********* メニューエリア　在庫管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "1"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrL As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrL.HasRows = True Then
                    Repeater_Menu_L.DataSource = SQLdrL
                    Repeater_Menu_L.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrL.Close()
                SQLdrL = Nothing



                '********* メニューエリア　回送管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "2"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrR As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrR.HasRows = True Then
                    Repeater_Menu_R.DataSource = SQLdrR
                    Repeater_Menu_R.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrR.Close() 'Reader(Close)
                SQLdrR = Nothing

                'SQLコマンド　クローズ
                SQLcmd.Dispose()
                SQLcmd = Nothing

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
                Dim USERcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = USERcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = USERcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = CS0050Session.USERID
                PARA2.Value = "1"
                Dim SQLdr As SqlDataReader = USERcmd.ExecuteReader()

                While SQLdr.Read
                    WW_ENDYMD = SQLdr("PASSENDYMD")
                    Exit While
                End While

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                USERcmd.Dispose()
                USERcmd = Nothing

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
    ''' Repeater_Menu_x バインドイベント(Handlesに含めたオブジェクトが対象)
    ''' </summary>
    ''' <param name="sender">イベント発生オブジェクト</param>
    ''' <param name="e"></param>
    Protected Sub RptInfo_ItemDataBound_L(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) _
        Handles Repeater_Menu_L.ItemDataBound, Repeater_Menu_R.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        Dim dicSuffixList As New Dictionary(Of String, String) From {{"Repeater_Menu_L", "L"}, {"Repeater_Menu_R", "R"}}
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
        Handles Repeater_Menu_L.ItemCommand, Repeater_Menu_R.ItemCommand

        Dim dicSuffixList As New Dictionary(Of String, String) From {{"Repeater_Menu_L", "L"}, {"Repeater_Menu_R", "R"}}
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



End Class
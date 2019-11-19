Imports System.Data.SqlClient

Public Class M00001MENU
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
        Master.MAPID = GRM00001WRKINC.MAPID
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
                        & " WHERE ROWLINE <= 11                " _
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


                '********* メニューエリア　受注管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "2"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrL2 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrL2.HasRows = True Then
                    Repeater_Menu_L2.DataSource = SQLdrL2
                    Repeater_Menu_L2.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrL2.Close() 'Reader(Close)
                SQLdrL2 = Nothing


                '********* メニューエリア　回送管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "3"
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


                '********* メニューエリア　マスタ管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "4"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrR2 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrR2.HasRows = True Then
                    Repeater_Menu_R2.DataSource = SQLdrR2
                    Repeater_Menu_R2.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrR2.Close() 'Reader(Close)
                SQLdrR2 = Nothing

                '********* メニューエリア　請求支払管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "5"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrL3 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrL3.HasRows = True Then
                    Repeater_Menu_L3.DataSource = SQLdrL3
                    Repeater_Menu_L3.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrL3.Close()
                SQLdrL3 = Nothing

                '********* メニューエリア　請求支払管理
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "6"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrL4 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrL4.HasRows = True Then
                    Repeater_Menu_L4.DataSource = SQLdrL4
                    Repeater_Menu_L4.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrL4.Close()
                SQLdrL4 = Nothing

                '********* メニューエリア　実績・統計
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "7"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrR3 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrR3.HasRows = True Then
                    Repeater_Menu_R3.DataSource = SQLdrR3
                    Repeater_Menu_R3.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrR3.Close()
                SQLdrR3 = Nothing

                '********* メニューエリア　データ連携
                WW_Select_CNT = ""

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = Master.MAPID
                PARA3.Value = Master.ROLE_MENU
                PARA4.Value = "8"
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = C_DELETE_FLG.DELETE

                Dim SQLdrR4 As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdrR4.HasRows = True Then
                    Repeater_Menu_R4.DataSource = SQLdrR4
                    Repeater_Menu_R4.DataBind()
                    WW_Select_CNT = "OK"
                Else
                    WW_Select_CNT = "NG"
                End If

                'Close
                SQLdrR4.Close()
                SQLdrR4 = Nothing


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
    ' ******************************************************************************
    ' ***  Repeater_Menu_L バインド　在庫管理　                                  ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_L(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Text = String.Empty
            Else
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_L"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_L"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_L"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_L"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_L"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_L"), Label).Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L2 バインド 受注管理　                                  ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_L2(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L2.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_L2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_L2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_L2"), Label).Text = String.Empty
            Else
                CType(e.Item.FindControl("WF_MenuURL_L2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_L2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_L2"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_L2"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_L2"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_L2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L2"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_L2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L2"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_L2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_L2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L2"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_L2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L2"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_L2"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_L2"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_L2"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_L2"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_L2"), Label).Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R バインド  回送管理　　                                 ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_R(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_R.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集　 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Text = ""
            Else
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_R"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_R"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_R"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_R"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_R"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_R"), Label).Visible = False
            End If
        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R2 バインド　マスタ管理                                 ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_R2(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_R2.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集　 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_R2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_R2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_R2"), Label).Text = ""
            Else
                CType(e.Item.FindControl("WF_MenuURL_R2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_R2"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_R2"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_R2"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_R2"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_R2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R2"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_R2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R2"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_R2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_R2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R2"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_R2"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R2"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_R2"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_R2"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_R2"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_R2"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_R2"), Label).Visible = False
            End If
        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L3 バインド 支払請求管理　                                  ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_L3(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L3.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_L3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_L3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_L3"), Label).Text = String.Empty
            Else
                CType(e.Item.FindControl("WF_MenuURL_L3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_L3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_L3"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_L3"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_L3"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_L3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L3"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_L3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L3"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_L3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_L3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L3"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_L3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L3"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_L3"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_L3"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_L3"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_L3"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_L3"), Label).Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L4 バインド 支払請求管理　                                  ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_L4(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L4.ItemDataBound

        '★★★ Repeater_Menu_Lバインド時 編集（左） ★★★
        '○ヘッダー編集 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_L4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_L4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_L4"), Label).Text = String.Empty
            Else
                CType(e.Item.FindControl("WF_MenuURL_L4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_L4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_L4"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_L4"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_L4"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_L4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L4"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_L4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L4"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_L4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_L4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_L4"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_L4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_L4"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_L4"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_L4"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_L4"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_L4"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_L4"), Label).Visible = False
            End If

        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R3 バインド　実績・統計                                ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_R3(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_R3.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集　 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_R3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_R3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_R3"), Label).Text = ""
            Else
                CType(e.Item.FindControl("WF_MenuURL_R3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_R3"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_R3"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_R3"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_R3"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_R3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R3"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_R3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R3"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_R3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_R3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R3"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_R3"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R3"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_R3"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_R3"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_R3"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_R3"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_R3"), Label).Visible = False
            End If
        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R4 バインド　実績・統計                                ***
    ' ******************************************************************************
    Protected Sub RptInfo_ItemDataBound_R4(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_R4.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集　 処理なし
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            CType(e.Item.FindControl("WF_MenuLabe_R4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "TITLE")
            CType(e.Item.FindControl("WF_MenuVARI_R4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "VARIANT")
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                CType(e.Item.FindControl("WF_MenuURL_R4"), Label).Text = ""
            Else
                CType(e.Item.FindControl("WF_MenuURL_R4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "URL")
            End If
            CType(e.Item.FindControl("WF_MenuMAP_R4"), Label).Text = DataBinder.Eval(e.Item.DataItem, "MAPID")
            CType(e.Item.FindControl("WF_MenuButton_R4"), Button).Text = "  " & DataBinder.Eval(e.Item.DataItem, "NAMES")

            If DataBinder.Eval(e.Item.DataItem, "TITLE") = "" Then
                If DataBinder.Eval(e.Item.DataItem, "NAMES") = "" Then
                    CType(e.Item.FindControl("WF_MenuLabe_R4"), Label).Text = "　　"
                    CType(e.Item.FindControl("WF_MenuLabe_R4"), Label).Visible = True
                    CType(e.Item.FindControl("WF_MenuVARI_R4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R4"), Button).Visible = False
                    CType(e.Item.FindControl("WF_MenuURL_R4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R4"), Label).Visible = False
                Else
                    CType(e.Item.FindControl("WF_MenuLabe_R4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuVARI_R4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuButton_R4"), Button).Visible = True
                    CType(e.Item.FindControl("WF_MenuURL_R4"), Label).Visible = False
                    CType(e.Item.FindControl("WF_MenuMAP_R4"), Label).Visible = False
                End If
            Else
                CType(e.Item.FindControl("WF_MenuLabe_R4"), Label).Visible = True
                CType(e.Item.FindControl("WF_MenuVARI_R4"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuButton_R4"), Button).Visible = False
                CType(e.Item.FindControl("WF_MenuURL_R4"), Label).Visible = False
                CType(e.Item.FindControl("WF_MenuMAP_R4"), Label).Visible = False
            End If
        End If

        '○フッター編集　 処理なし
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L 在庫管理メニューボタン押下処理                        ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_L(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuURL_L")
        Dim WW_VARI As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuVARI_L")
        Dim WW_MAPID As Label = Repeater_Menu_L.Items(WW_COUNT).FindControl("WF_MenuMAP_L")

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

    ' ******************************************************************************
    ' ***  Repeater_Menu_L2 受注管理メニューボタン押下処理                       ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_L2(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L2.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_L2.Items(WW_COUNT).FindControl("WF_MenuURL_L2")
        Dim WW_VARI As Label = Repeater_Menu_L2.Items(WW_COUNT).FindControl("WF_MenuVARI_L2")
        Dim WW_MAPID As Label = Repeater_Menu_L2.Items(WW_COUNT).FindControl("WF_MenuMAP_L2")

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

    ' ******************************************************************************
    ' ***  Repeater_Menu_R 回送管理メニューボタン押下処理                        ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_R(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_R.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（右） ★★★
        'ボタン押下時、画面遷移
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuURL_R")
        Dim WW_VARI As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuVARI_R")
        Dim WW_MAPID As Label = Repeater_Menu_R.Items(WW_COUNT).FindControl("WF_MenuMAP_R")

        '○画面遷移権限チェック（右）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        '20191101-追加-START
        CS0007CheckAuthority.ROLECODE_MENU = Master.ROLE_MENU
        CS0007CheckAuthority.ROLECODE_VIEWPROF = Master.ROLE_VIEWPROF
        CS0007CheckAuthority.ROLECODE_RPRTPROF = Master.ROLE_RPRTPROF
        '20191101-追加-END
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""
                '20191101-追加-START
                CS0050Session.VIEW_MENU_MODE = CS0007CheckAuthority.ROLECODE_MENU
                CS0050Session.VIEW_MAP_MODE = CS0007CheckAuthority.ROLECODE_MAP
                CS0050Session.VIEW_VIEWPROF_MODE = CS0007CheckAuthority.ROLECODE_VIEWPROF
                CS0050Session.VIEW_RPRTPROF_MODE = CS0007CheckAuthority.ROLECODE_RPRTPROF
                '20191101-追加-END

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

        Server.Transfer(WW_URL.Text)

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R2 マスタ管理メニューボタン押下処理                     ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_R2(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_R2.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（右） ★★★
        'ボタン押下時、画面遷移
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_R2.Items(WW_COUNT).FindControl("WF_MenuURL_R2")
        Dim WW_VARI As Label = Repeater_Menu_R2.Items(WW_COUNT).FindControl("WF_MenuVARI_R2")
        Dim WW_MAPID As Label = Repeater_Menu_R2.Items(WW_COUNT).FindControl("WF_MenuMAP_R2")

        '○画面遷移権限チェック（右）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        '20191101-追加-START
        CS0007CheckAuthority.ROLECODE_MENU = Master.ROLE_MENU
        CS0007CheckAuthority.ROLECODE_VIEWPROF = Master.ROLE_VIEWPROF
        CS0007CheckAuthority.ROLECODE_RPRTPROF = Master.ROLE_RPRTPROF
        '20191101-追加-END
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""
                '20191101-追加-START
                CS0050Session.VIEW_MENU_MODE = CS0007CheckAuthority.ROLECODE_MENU
                CS0050Session.VIEW_MAP_MODE = CS0007CheckAuthority.ROLECODE_MAP
                CS0050Session.VIEW_VIEWPROF_MODE = CS0007CheckAuthority.ROLECODE_VIEWPROF
                CS0050Session.VIEW_RPRTPROF_MODE = CS0007CheckAuthority.ROLECODE_RPRTPROF
                '20191101-追加-END

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

        Server.Transfer(WW_URL.Text)

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_L3 請求支払管理メニューボタン押下処理                       ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_L3(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L3.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_L3.Items(WW_COUNT).FindControl("WF_MenuURL_L3")
        Dim WW_VARI As Label = Repeater_Menu_L3.Items(WW_COUNT).FindControl("WF_MenuVARI_L3")
        Dim WW_MAPID As Label = Repeater_Menu_L3.Items(WW_COUNT).FindControl("WF_MenuMAP_L3")

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

    ' ******************************************************************************
    ' ***  Repeater_Menu_L4 タンク車所在管理メニューボタン押下処理                        ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_L4(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L4.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_L4.Items(WW_COUNT).FindControl("WF_MenuURL_L4")
        Dim WW_VARI As Label = Repeater_Menu_L4.Items(WW_COUNT).FindControl("WF_MenuVARI_L4")
        Dim WW_MAPID As Label = Repeater_Menu_L4.Items(WW_COUNT).FindControl("WF_MenuMAP_L4")

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

    ' ******************************************************************************
    ' ***  Repeater_Menu_R3 実績・統計メニューボタン押下処理                     ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_R3(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_R3.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（右） ★★★
        'ボタン押下時、画面遷移
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_R3.Items(WW_COUNT).FindControl("WF_MenuURL_R3")
        Dim WW_VARI As Label = Repeater_Menu_R3.Items(WW_COUNT).FindControl("WF_MenuVARI_R3")
        Dim WW_MAPID As Label = Repeater_Menu_R3.Items(WW_COUNT).FindControl("WF_MenuMAP_R3")

        '○画面遷移権限チェック（右）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        '20191101-追加-START
        CS0007CheckAuthority.ROLECODE_MENU = Master.ROLE_MENU
        CS0007CheckAuthority.ROLECODE_VIEWPROF = Master.ROLE_VIEWPROF
        CS0007CheckAuthority.ROLECODE_RPRTPROF = Master.ROLE_RPRTPROF
        '20191101-追加-END
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""
                '20191101-追加-START
                CS0050Session.VIEW_MENU_MODE = CS0007CheckAuthority.ROLECODE_MENU
                CS0050Session.VIEW_MAP_MODE = CS0007CheckAuthority.ROLECODE_MAP
                CS0050Session.VIEW_VIEWPROF_MODE = CS0007CheckAuthority.ROLECODE_VIEWPROF
                CS0050Session.VIEW_RPRTPROF_MODE = CS0007CheckAuthority.ROLECODE_RPRTPROF
                '20191101-追加-END

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

        Server.Transfer(WW_URL.Text)

    End Sub

    ' ******************************************************************************
    ' ***  Repeater_Menu_R4 データ連携   メニューボタン押下処理                     ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand_R4(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_R4.ItemCommand

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim CS0007CheckAuthority As New CS0007CheckAuthority          'AUTHORmap

        '★★★ ボタン押下時、画面遷移（右） ★★★
        'ボタン押下時、画面遷移
        Dim WW_COUNT As Integer = e.Item.ItemIndex.ToString()
        Dim WW_URL As Label = Repeater_Menu_R4.Items(WW_COUNT).FindControl("WF_MenuURL_R4")
        Dim WW_VARI As Label = Repeater_Menu_R4.Items(WW_COUNT).FindControl("WF_MenuVARI_R4")
        Dim WW_MAPID As Label = Repeater_Menu_R4.Items(WW_COUNT).FindControl("WF_MenuMAP_R4")

        '○画面遷移権限チェック（右）
        CS0007CheckAuthority.MAPID = WW_MAPID.Text
        CS0007CheckAuthority.ROLECODE_MAP = Master.ROLE_MAP
        '20191101-追加-START
        CS0007CheckAuthority.ROLECODE_MENU = Master.ROLE_MENU
        CS0007CheckAuthority.ROLECODE_VIEWPROF = Master.ROLE_VIEWPROF
        CS0007CheckAuthority.ROLECODE_RPRTPROF = Master.ROLE_RPRTPROF
        '20191101-追加-END
        CS0007CheckAuthority.check()
        If isNormal(CS0007CheckAuthority.ERR) Then
            If CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.REFERLANCE OrElse
               CS0007CheckAuthority.MAPPERMITCODE = C_PERMISSION.UPDATE Then
                CS0050Session.VIEW_PERMIT = CS0007CheckAuthority.MAPPERMITCODE
                CS0050Session.VIEW_MAPID = WW_MAPID.Text
                CS0050Session.VIEW_MAP_VARIANT = WW_VARI.Text
                CS0050Session.MAP_ETC = ""
                '20191101-追加-START
                CS0050Session.VIEW_MENU_MODE = CS0007CheckAuthority.ROLECODE_MENU
                CS0050Session.VIEW_MAP_MODE = CS0007CheckAuthority.ROLECODE_MAP
                CS0050Session.VIEW_VIEWPROF_MODE = CS0007CheckAuthority.ROLECODE_VIEWPROF
                CS0050Session.VIEW_RPRTPROF_MODE = CS0007CheckAuthority.ROLECODE_RPRTPROF
                '20191101-追加-END

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

        Server.Transfer(WW_URL.Text)

    End Sub

End Class
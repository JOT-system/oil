Option Strict On
Imports System.Data.SqlClient
Imports System.Net

Public Class M00000LOGON
    Inherits System.Web.UI.Page

    'セッション情報
    Private CS0050Session As New CS0050SESSION

    '画面ID
    Private Const MAPID As String = "M00000"

    'パスワード誤り回数を超えた時のメッセージ
    Private Const CONST_MSG_10056 As String = "10056"

    'パスワード入力間違いの時のメッセージ 
    Private Const CONST_MSG_10057 As String = "10057"

    'ＩＤ、パスワード入力間違いの時のメッセージ 
    Private Const CONST_MSG_10058 As String = "10058"

    Private Const C_MAX_MISS_PASSWORD_COUNT As Integer = 6      'パスワード入力失敗の最大回数
    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■　初期処理　■■■
        '共通セッション情報
        '   Class         : クラス(プロジェクト直下のクラス)
        '   Userid        : ユーザID
        '   APSRVname     : APサーバー名称
        '   Term          : 操作端末(端末操作情報として利用)

        '   DBcon         : DB接続文字列 
        '   LOGdir        : ログ出力ディレクトリ 
        '   PDFdir        : PDF用ワークのディレクトリ
        '   FILEdir       : FILE格納ディレクトリ
        '   JNLdir        : 更新ジャーナル格納ディレクトリ

        '   MAPmapid      : 画面間IF(MAPID)

        If IsPostBack Then
            PassWord.Attributes.Add("value", PassWord.Text)

            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonOK"
                        WF_ButtonOK_Click(sender, e)
                End Select
            End If
        Else
            '〇初期化処理
            Initialize()
        End If

        Master.LOGINCOMP = WF_TERMCAMP.Text
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '■■■　セッション変数設定　■■■
        Dim CS001INIFILE As New CS0001INIFILEget            'INIファイル読み込み
        Dim CS0006TERMchk As New CS0006TERMchk              'ローカルコンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Master.dispHelp = False

        Master.MAPID = MAPID
        '○ 固定項目設定
        If String.IsNullOrEmpty(CS0050Session.USERID) Then
            CS0050Session.USERID = "INIT"
            CS0050Session.APSV_ID = "INIT"
            CS0050Session.APSV_COMPANY = "INIT"
            CS0050Session.APSV_ORG = "INIT"
            CS0050Session.SELECTED_COMPANY = "INIT"
            CS0050Session.DRIVERS = ""
        End If
        CS001INIFILE.CS0001INIFILEget()
        If Not isNormal(CS001INIFILE.ERR) Then
            Master.Output(CS001INIFILE.ERR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        '○ APサーバー情報からAPサーバー設置会社(APSRVCamp)、APサーバー設置部署(APSRVOrg)取得
        CS0006TERMchk.TERMID = CS0050Session.APSV_ID
        CS0006TERMchk.CS0006TERMchk()
        If isNormal(CS0006TERMchk.ERR) Then
            CS0050Session.APSV_COMPANY = CS0006TERMchk.TERMCAMP
            CS0050Session.APSV_ORG = CS0006TERMchk.TERMORG
            CS0050Session.APSV_M_ORG = CS0006TERMchk.MORG
        Else
            Master.Output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0006TERMchk")
            Exit Sub
        End If



        '■■■　オンラインサービス判定　■■■

        '○オンラインサービス停止なら画面遷移しない 
        '接続サーバ（INIファイルのサーバ）、対象会社がオンラインか確認

        CS0008ONLINEstat.COMPCODE = WF_TERMCAMP.Text
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            If CS0008ONLINEstat.ONLINESW = 0 Then
                Master.Output(C_MESSAGE_NO.CLOSED_SERVICE, C_MESSAGE_TYPE.ERR)
                WF_Guidance.Text = String.Empty
                Exit Sub
            Else
                WF_Guidance.Text = WF_Guidance.Text & CS0008ONLINEstat.TEXT.Replace(vbCrLf, "<br />")
            End If
        Else
            Master.Output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If


        '■■■ 初期画面表示 ■■■

        '○パソコン名存在チェック
        ' ホスト名を取得する
        Dim WW_ipAddress As String
        Dim WW_hostName As String

        Try
            WW_ipAddress = Convert.ToString(Request.ServerVariables("REMOTE_HOST"))
            WW_hostName = System.Net.Dns.GetHostEntry(WW_ipAddress).HostName()
            If InStr(WW_hostName, ".") = 0 Then
                CS0006TERMchk.TERMID = WW_hostName.ToString
            Else
                CS0006TERMchk.TERMID = Mid(WW_hostName.ToString, 1, InStr(WW_hostName.ToString, ".") - 1)
            End If


        Catch ex As Exception
            'サーバー名
            CS0006TERMchk.TERMID = Environment.MachineName
        End Try

        CS0006TERMchk.TERMID = CS0050Session.APSV_ID

        CS0006TERMchk.CS0006TERMchk()
        If isNormal(CS0006TERMchk.ERR) Then
            CS0050Session.TERMID = CS0006TERMchk.TERMID
            CS0050Session.TERM_COMPANY = CS0006TERMchk.TERMCAMP
            CS0050Session.TERM_ORG = CS0006TERMchk.TERMORG
            CS0050Session.TERM_M_ORG = CS0006TERMchk.MORG
        Else
            Master.Output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0006TERMchk")
            Exit Sub
        End If


        '■■■　初期メッセージ表示　■■■
        'Master.Output(C_MESSAGE_NO.INPUT_ID_PASSWD, C_MESSAGE_TYPE.INF)

        'C:\APPL\APPLFILES\XML_TMPディレクトリの不要データを掃除
        Dim WW_File As String

        For Each tempFile As String In System.IO.Directory.GetFiles(
            CS0050Session.UPLOAD_PATH & "\XML_TMP", "*", System.IO.SearchOption.AllDirectories)
            ' ファイルパスからファイル名を取得
            WW_File = tempFile
            Do
                WW_File = Mid(WW_File, InStr(WW_File, "\") + 1, 200)
            Loop Until InStr(WW_File, "\") = 0

            '本日作成以外のファイルは削除
            If Mid(WW_File, 1, 8) <> Date.Now.ToString("yyyyMMdd") Then System.IO.File.Delete(tempFile)
        Next
        UserID.Focus()

    End Sub
    ''' <summary>
    '''　OKボタン押下時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOK_Click(sender As Object, e As EventArgs)

        '■■■　初期処理　■■■

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'メッセージ出力 out
        Dim CS0006TERMchk As New CS0006TERMchk              'ローカルコンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態

        '○オンラインサービス判定
        '画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            'オンラインサービス停止時、ログオン画面へ遷移
            If CS0008ONLINEstat.ONLINESW = 0 Then Exit Sub

        Else
            Master.Output(CS0008ONLINEstat.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If

        '■■■　メイン処理　■■■
        '〇ID、パスワードのいずれかが未入力なら抜ける
        If String.IsNullOrEmpty(UserID.Text) OrElse String.IsNullOrEmpty(PassWord.Text) Then Exit Sub

        '○ 入力文字内の禁止文字排除
        '   画面UserID内の使用禁止文字排除
        Master.EraseCharToIgnore(UserID.Text)
        Master.EraseCharToIgnore(PassWord.Text)

        '○ 画面UserIDのDB(OIS0004_USER)存在チェック
        Dim WW_USERID As String = String.Empty
        Dim WW_PASSWORD As String = String.Empty
        Dim WW_USERCAMP As String = String.Empty
        Dim WW_ORG As String = String.Empty
        Dim WW_STYMD As Date = Date.Now
        Dim WW_ENDYMD As Date = Date.Now
        Dim WW_MISSCNT As Integer = 0
        Dim WW_UPDYMD As Date
        Dim WW_UPDTIMSTP As Byte()
        '20191101-追加-START
        Dim WW_MENUROLE As String = String.Empty
        Dim WW_MAPROLE As String = String.Empty
        Dim WW_VIEWPROFID As String = String.Empty
        Dim WW_RPRTPROFID As String = String.Empty
        Dim WW_APPROVALID As String = String.Empty
        '20191101-追加-END
        Dim WW_MAPID As String = String.Empty
        Dim WW_VARIANT As String = String.Empty
        Dim WW_PASSENDYMD As String = String.Empty
        Dim WW_err As String = String.Empty
        Dim WW_RTN As String = String.Empty
        Dim WW_LOGONYMD As String = Date.Now.ToString("yyyy/MM/dd")
        Dim WW_URL As String = String.Empty
        Dim WW_MENUURL As String = String.Empty
        Dim WW_chk As String = String.Empty

        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            ' パスワード　証明書オープン
            Try
                Dim SQLOpen_Str As String = "OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotoil"
                Using SQLOpencmd As New SqlCommand(SQLOpen_Str, SQLcon)
                    SQLOpencmd.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0004_USERPASS OPEN")
                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "OIS0004_USERPASS OPEN"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            Try
                'OIS0004_USER検索SQL文
                Dim SQL_Str As String =
                     "SELECT " _
                   & " rtrim(A.USERID)   as USERID    , " _
                   & " rtrim(A.CAMPCODE) as CAMPCODE  , " _
                   & " rtrim(A.ORG)      as ORG       , " _
                   & " A.STYMD                        , " _
                   & " A.ENDYMD                       , " _
                   & " rtrim(CONVERT(nvarchar, DecryptByKey(B.PASSWORD))) as PASSWORD  , " _
                   & " B.MISSCNT                      , " _
                   & " A.INITYMD                      , " _
                   & " A.UPDYMD                       , " _
                   & " A.UPDTIMSTP                    , " _
                   & " rtrim(A.MENUROLE) as MENUROLE  , " _
                   & " rtrim(A.MAPROLE) as MAPROLE    , " _
                   & " rtrim(A.VIEWPROFID) as VIEWPROFID    , " _
                   & " rtrim(A.RPRTPROFID) as RPRTPROFID    , " _
                   & " rtrim(A.MAPID)    as MAPID     , " _
                   & " rtrim(A.VARIANT)  as VARIANT   , " _
                   & " rtrim(A.APPROVALID) as APPROVALID    , " _
                   & " B.PASSENDYMD      as PASSENDYMD  " _
                   & " FROM       COM.OIS0004_USER       A    " _
                   & " INNER JOIN COM.OIS0005_USERPASS   B ON " _
                   & "       B.USERID      = A.USERID   " _
                   & "   and B.DELFLG     <> @P4        " _
                   & " Where A.USERID      = @P1        " _
                   & "   and A.STYMD      <= @P2        " _
                   & "   and A.ENDYMD     >= @P3        " _
                   & "   and B.PASSENDYMD >= @P3        " _
                   & "   and A.DELFLG     <> @P4        "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = UserID.Text
                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    WW_err = C_MESSAGE_NO.UNMATCH_ID_PASSWD_ERROR
                    If SQLdr.Read Then
                        WW_USERID = Convert.ToString(SQLdr("USERID"))
                        WW_PASSWORD = Convert.ToString(SQLdr("PASSWORD"))
                        WW_USERCAMP = Convert.ToString(SQLdr("CAMPCODE"))
                        WW_ORG = Convert.ToString(SQLdr("ORG"))
                        WW_STYMD = CDate(SQLdr("STYMD"))
                        WW_ENDYMD = CDate(SQLdr("ENDYMD"))
                        WW_MISSCNT = CInt(SQLdr("MISSCNT"))
                        If SQLdr("UPDYMD") Is DBNull.Value Then
                            WW_UPDYMD = System.DateTime.UtcNow
                        Else
                            WW_UPDYMD = CDate(SQLdr("UPDYMD"))
                        End If
                        WW_UPDTIMSTP = CType(SQLdr("UPDTIMSTP"), Byte())
                        '20191101-追加-START
                        WW_MENUROLE = Convert.ToString(SQLdr("MENUROLE"))
                        WW_MAPROLE = Convert.ToString(SQLdr("MAPROLE"))
                        WW_VIEWPROFID = Convert.ToString(SQLdr("VIEWPROFID"))
                        WW_RPRTPROFID = Convert.ToString(SQLdr("RPRTPROFID"))
                        WW_APPROVALID = Convert.ToString(SQLdr("APPROVALID"))
                        '20191101-追加-END
                        WW_MAPID = Convert.ToString(SQLdr("MAPID"))
                        WW_VARIANT = Convert.ToString(SQLdr("VARIANT"))
                        WW_PASSENDYMD = Convert.ToString(SQLdr("PASSENDYMD"))
                        WW_err = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0004_USER SELECT")

                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "OIS0004_USER SELECT"                           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            'ユーザID誤り
            'If Not isNormal(WW_err) OrElse
            '    UserID.Text = C_DEFAULT_DATAKEY OrElse
            '    UserID.Text = "INIT" Then

            If Not isNormal(WW_err) Then
                Master.Output(CONST_MSG_10058, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                Exit Sub
            End If

            '○ パスワードチェック
            'ユーザあり　かつ　(パスワード誤り　または　パスワード6回以上誤り)
            If (PassWord.Text <> WW_PASSWORD) Then

                Master.Output(CONST_MSG_10057, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                WW_chk = "err"

            ElseIf (WW_MISSCNT >= C_MAX_MISS_PASSWORD_COUNT) Then

                Master.Output(CONST_MSG_10056, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                UserID.Focus()
                WW_chk = "err"

            End If

            If WW_chk = "err" Then
                'パスワードエラー回数のカウントUP
                Try
                    'S0014_USER更新SQL文
                    Dim SQL_Str As String =
                         "Update COM.OIS0005_USERPASS " _
                       & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                       & "Where  USERID  = @P3 "
                    Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Int)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.DateTime)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                        If WW_MISSCNT = 999 Then
                            PARA1.Value = WW_MISSCNT
                        Else
                            PARA1.Value = WW_MISSCNT + 1
                        End If
                        PARA2.Value = Date.Now
                        PARA3.Value = UserID.Text
                        SQLcmd.ExecuteNonQuery()

                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0005_USERPASS UPDATE")
                    CS0011LOGWRITE.INFSUBCLASS = "Main"
                    CS0011LOGWRITE.INFPOSI = "OIS0005_USERPASS Update"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                    CS0011LOGWRITE.CS0011LOGWrite()
                End Try
                UserID.Focus()
                Exit Sub
            End If

            '○ パスワードチェックＯＫ時処理
            'セッション情報（ユーザＩＤ）設定
            CS0050Session.USERID = UserID.Text

            'ミスカウントクリア
            Try
                'S0014_USER更新SQL文
                Dim SQL_Str As String =
                     "Update COM.OIS0005_USERPASS " _
                   & "Set    MISSCNT = @P1 , UPDYMD = @P2 , UPDUSER = @P3 " _
                   & "Where  USERID  = @P3 "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Int)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.DateTime)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                    PARA1.Value = 0
                    PARA2.Value = Date.Now
                    PARA3.Value = UserID.Text
                    SQLcmd.ExecuteNonQuery()

                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0005_USERPASS UPDATE")

                CS0011LOGWRITE.INFSUBCLASS = "Main"
                CS0011LOGWRITE.INFPOSI = "OIS0005_USERPASS Update"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()
                Exit Sub
            End Try

            '■■■　終了処理　■■■

            '○ パスワードチェックＯＫ時、指定画面へ遷移
            'ユーザマスタより、MAPIDおよびVARIANTを取得

            Try
                If WW_PASSENDYMD <= Date.Now.AddDays(7).ToString("yyyy/MM/dd") Then
                    'パスワード登録画面（1週間前）の場合
                    GetURL(WW_PASSENDYMD, "CO0014", WW_URL)
                    GetURL(WW_PASSENDYMD, WW_MAPID, WW_MENUURL)
                Else
                    GetURL(WW_PASSENDYMD, WW_MAPID, WW_URL)
                End If

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0007_URL SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "OIS0007_URL SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try

            '★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
            'デバッグ時は、
            ' ①ログオン日付更新処理をコメントアウトする（リコンパイル）
            ' ②OIS0006_LOGONYMDテーブルの該当SRV（TERMID）のログオン日付をテスト対象日に手修正
            '
            '本番は、
            ' ①下記コメントを外し、ログオン日付更新処理を有効にする（リコンパイル）
            '★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            Try
                'OIS0006_LOGONYMD検索SQL文
                Dim SQL_Str As String =
                     "SELECT isnull(LOGONYMD, '') as LOGONYMD " _
                   & " FROM  COM.OIS0006_LOGONYMD " _
                   & " Where TERMID   = @P1 "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
                    PARA1.Value = CS0050Session.APSV_ID

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        Try
                            Dim WW_DATE As Date
                            Date.TryParse(Convert.ToString(SQLdr("LOGONYMD")), WW_DATE)
                            WW_LOGONYMD = WW_DATE.ToString("yyyy/MM/dd")
                        Catch ex As Exception
                            WW_LOGONYMD = Date.Now.ToString("yyyy/MM/dd")
                        End Try
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0006_LOGONYMD SELECT")
                CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "OIS0006_LOGONYMD SELECT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End Try
        End Using

        CS0050Session.VIEW_MAPID = WW_MAPID
        '20191101-追加-START
        CS0050Session.VIEW_MENU_MODE = WW_MENUROLE
        CS0050Session.VIEW_MAP_MODE = WW_MAPID
        CS0050Session.VIEW_VIEWPROF_MODE = WW_VIEWPROFID
        CS0050Session.VIEW_RPRTPROF_MODE = WW_RPRTPROFID
        CS0050Session.VIEW_APPROVALID = WW_APPROVALID
        '20191101-追加-END
        CS0050Session.VIEW_MAP_VARIANT = WW_VARIANT
        CS0050Session.MAP_ETC = ""
        CS0050Session.VIEW_PERMIT = ""

        Master.MAPID = WW_MAPID
        Master.USERCAMP = WW_USERCAMP
        '20191101-追加-START
        Master.ROLE_MENU = WW_MENUROLE
        Master.ROLE_MAP = WW_MAPID
        Master.ROLE_VIEWPROF = WW_VIEWPROFID
        Master.ROLE_RPRTPROF = WW_RPRTPROFID
        Master.ROLE_APPROVALID = WW_APPROVALID
        '20191101-追加-END
        Master.MAPvariant = WW_VARIANT
        Master.MAPpermitcode = ""
        CS0050Session.LOGONDATE = WW_LOGONYMD

        '画面遷移実行
        If CS0050Session.USERID <> "INIT" Then
            Server.Transfer(WW_URL)
        End If

    End Sub


    ''' <summary>
    ''' 遷移先URLの取得
    ''' </summary>
    ''' <param name="I_PASSENDYMD"></param>
    ''' <param name="I_MAPID"></param>
    ''' <param name="O_URL"></param>
    ''' <remarks></remarks>
    Protected Sub GetURL(ByVal I_PASSENDYMD As String, ByVal I_MAPID As String, ByRef O_URL As String)

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get

        Dim WW_URL As String = ""
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'OIS0007_URL検索SQL文
                Dim SQL_Str As String =
                     "SELECT rtrim(URL) as URL " _
                   & " FROM  COM.OIS0007_URL " _
                   & " Where MAPID    = @P1 " _
                   & "   and STYMD   <= @P2 " _
                   & "   and ENDYMD  >= @P3 " _
                   & "   and DELFLG  <> @P4 "
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 50)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                    PARA1.Value = I_MAPID

                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        O_URL = Convert.ToString(SQLdr("URL"))
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0007_URL SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "GetURL"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "OIS0007_URL SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub
End Class




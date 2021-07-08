''************************************************************
' パスワード変更画面
' 作成日 2021/06/24
' 更新日 
' 作成者 JOT伊草
' 更新者 
'
' 修正履歴:2021/06/24 新規作成
''************************************************************
Imports System.Data.SqlClient

''' <summary>
''' パスワード変更(実行)
''' </summary>
''' <remarks></remarks>
Public Class OIS0002PasswordChange
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0020JOURNAL As New CS0020JOURNAL              'SQLジャーナル

    ''' <summary>
    ''' 共通処理結果
    ''' </summary>
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                          '変更ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_RIGHT_VIEW_DBClick"                '右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                        'メモ欄更新
                        WF_RIGHTBOX_Change()
                    Case "HELP"                                 'ヘルプ表示
                        WF_HELP_Click()
                    Case "WF_ButtonEND", "btnChangeCompleteOk"  '戻るボタン押下 or 更新完了ダイアログでOKクリック
                        WF_ButtonEND_Click()
                End Select
            End If
        Else
            '○ 初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = "OIS0002PC"

        WF_USERID.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_RightboxOpen.Value = ""

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '画面間の情報クリア
        work.Initialize()

        '○ RightBox情報設定
        rightview.MAPIDS = ""
        rightview.MAPID = "OIS0002PC"
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_USERID.Text)            'ユーザーID
        Master.EraseCharToIgnore(WF_CURRENTPASSWORD.Text)   '現在パスワード
        Master.EraseCharToIgnore(WF_NEWPASSWORD.Text)       '新パスワード
        Master.EraseCharToIgnore(WF_NEWPASSWORDCONF.Text)   '新パスワード(確認)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        Else
            'パスワード更新処理
            UpdatePassword()

            If WW_ERR_SW = "ERR" Then
                Exit Sub
            Else
                'パスワード有効期限警告ダイアログを出力
                Master.Output("10064", C_MESSAGE_TYPE.INF, needsPopUp:=True,
                              messageBoxTitle:="パスワード更新完了", IsConfirm:=True,
                              YesButtonId:="btnChangeCompleteOk")
            End If
        End If

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""
        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_LINE_ERR As String = ""

        '○ 単項目チェック
        'ユーザーID
        WW_TEXT = WF_USERID.Text
        Master.CheckField(Master.USERCAMP, "USERID", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If Not CheckUser(WW_TEXT) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "ユーザーID : " & WW_TEXT, needsPopUp:=True)
                WF_USERID.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "ユーザーID", needsPopUp:=True)
            WF_USERID.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '現在のパスワード
        WW_TEXT = WF_CURRENTPASSWORD.Text
        Master.CheckField(Master.USERCAMP, "CURRENTPASSWORD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If Not CheckCurrentPassword(WF_USERID.Text, WF_CURRENTPASSWORD.Text) Then
                Master.Output("10061", C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WF_CURRENTPASSWORD.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "現在のパスワード", needsPopUp:=True)
            WF_CURRENTPASSWORD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '新しいパスワード
        WW_TEXT = WF_NEWPASSWORD.Text
        Master.CheckField(Master.USERCAMP, "NEWPASSWORD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "新しいパスワード", needsPopUp:=True)
            WF_CURRENTPASSWORD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '新しいパスワード（確認用）
        WW_TEXT = WF_NEWPASSWORDCONF.Text
        Master.CheckField(Master.USERCAMP, "NEWPASSWORDCONF", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "新しいパスワード（確認用）", needsPopUp:=True)
            WF_CURRENTPASSWORD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '現在パスワードと新パスワードの一致チェック
        If WF_CURRENTPASSWORD.Text.Equals(WF_NEWPASSWORD.Text) Then
            Master.Output("10062", C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WF_CURRENTPASSWORD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '新パスワード、確認用の一致チェック
        If Not WF_NEWPASSWORD.Text.Equals(WF_NEWPASSWORDCONF.Text) Then
            Master.Output("10063", C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WF_CURRENTPASSWORD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' ユーザーパスワード更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdatePassword()

        '○ パスワード証明書 オープン
        Dim OpenStr As String = "OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotoil"

        '○ ＤＢ更新
        Dim UpdBldr As StringBuilder = New StringBuilder()
        UpdBldr.AppendLine(" UPDATE JOTDB.com.OIS0005_USERPASS")
        UpdBldr.AppendLine(" SET")
        UpdBldr.AppendLine("     PASSWORD = EncryptByKey(Key_GUID('loginpasskey'), @P02),")
        UpdBldr.AppendLine("     PASSENDYMD = @P03,")
        UpdBldr.AppendLine("     UPDYMD = @P04,")
        UpdBldr.AppendLine("     UPDUSER = @P05,")
        UpdBldr.AppendLine("     UPDTERMID = @P06")
        UpdBldr.AppendLine(" WHERE")
        UpdBldr.AppendLine("     USERID = @P01")

        '○ 更新ジャーナル出力
        Dim JnlBldr As StringBuilder = New StringBuilder()
        JnlBldr.AppendLine(" SELECT")
        JnlBldr.AppendLine("     USERID")                                   'ユーザＩＤ
        JnlBldr.AppendLine("     , PASSWORD")                               'パスワード
        JnlBldr.AppendLine("     , MISSCNT")                                '誤り回数
        JnlBldr.AppendLine("     , PASSENDYMD")                             'パスワード有効期限
        JnlBldr.AppendLine("     , DELFLG")                                 '削除フラグ
        JnlBldr.AppendLine("     , INITYMD")                                '登録年月日
        JnlBldr.AppendLine("     , INITUSER")                               '登録ユーザーＩＤ
        JnlBldr.AppendLine("     , INITTERMID")                             '登録端末
        JnlBldr.AppendLine("     , UPDYMD")                                 '更新年月日
        JnlBldr.AppendLine("     , UPDUSER")                                '更新ユーザＩＤ
        JnlBldr.AppendLine("     , UPDTERMID")                              '更新端末
        JnlBldr.AppendLine("     , RECEIVEYMD")                             '集信日時
        JnlBldr.AppendLine("     , CAST(UPDTIMSTP AS bigint) AS TIMSTP")    'タイムスタンプ
        JnlBldr.AppendLine(" FROM")
        JnlBldr.AppendLine("     [com].OIS0005_USERPASS")
        JnlBldr.AppendLine(" WHERE")
        JnlBldr.AppendLine("     USERID = @P01")

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Using SQLOpen As New SqlCommand(OpenStr, SQLcon),
                        SQLcmd As New SqlCommand(UpdBldr.ToString, SQLcon),
                        SQLcmdJnl As New SqlCommand(JnlBldr.ToString, SQLcon)

                    'パスワード証明書オープン
                    SQLOpen.ExecuteNonQuery()

                    'DB更新
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)      'ユーザーID
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)      '新パスワード
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.DateTime)          'パスワード有効期限
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.DateTime)          '更新年月日
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20)      '更新ユーザーID
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 20)      '更新端末
                    PARA01.Value = WF_USERID.Text
                    PARA02.Value = WF_NEWPASSWORD.Text
                    PARA03.Value = Date.Now.AddDays(90) '本日から90日間有効
                    PARA04.Value = Date.Now
                    PARA05.Value = Master.USERID
                    PARA06.Value = Master.USERTERMID
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    '更新ジャーナル出力
                    Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 20)  'ユーザーID
                    Dim jnlTbl As DataTable = New DataTable
                    JPARA01.Value = WF_USERID.Text
                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            jnlTbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        jnlTbl.Clear()
                        jnlTbl.Load(SQLdr)
                    End Using

                    For Each OIM0002UPDrow As DataRow In jnlTbl.Rows
                        CS0020JOURNAL.TABLENM = "OIS0002PC"
                        CS0020JOURNAL.ACTION = "UPDATE"
                        CS0020JOURNAL.ROW = OIM0002UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0002PC UPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0002PC UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            WW_ERR_SW = "ERR"
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(Master.USERCAMP, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.ShowHelp()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' ユーザー存在チェック
    ''' </summary>
    ''' <param name="userid"></param>
    ''' <returns></returns>
    Private Function CheckUser(ByVal userid As String) As Boolean

        Dim ret As Boolean = False

        '○ 画面表示データ取得
        Try

            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Dim StrBldr = New StringBuilder()
                StrBldr.AppendLine(" SELECT")
                StrBldr.AppendLine("     STAFFNAMES")
                StrBldr.AppendLine(" FROM")
                StrBldr.AppendLine("     com.OIS0004_USER")
                StrBldr.AppendLine(" WHERE")
                StrBldr.AppendLine("     USERID =  @P01")
                StrBldr.AppendLine(" AND STYMD <=  @P02")
                StrBldr.AppendLine(" AND ENDYMD >= @P02")
                StrBldr.AppendLine(" AND DELFLG <> @P00")


                Using SQLcmd As SqlCommand = New SqlCommand(StrBldr.ToString, SQLcon)
                    Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)

                    PARA00.Value = BaseDllConst.C_DELETE_FLG.DELETE
                    PARA01.Value = userid
                    PARA02.Value = Date.Now

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            ret = True
                        End If
                    End Using

                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0002PC SELECT USER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0002PC SELECT USER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

        End Try

        Return ret
    End Function

    ''' <summary>
    ''' 現在パスワードチェック
    ''' </summary>
    ''' <param name="userid">ユーザーID</param>
    ''' <param name="password">現在パスワード</param>
    ''' <returns></returns>
    Private Function CheckCurrentPassword(ByVal userid As String, ByVal password As String) As Boolean

        Dim ret As Boolean = False

        '○ 画面表示データ取得
        Try

            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'パスワード　証明書オープン
                Try
                    Dim SQLOpen_Str As String = "OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotoil"
                    Using SQLOpencmd As New SqlCommand(SQLOpen_Str, SQLcon)
                        SQLOpencmd.ExecuteNonQuery()
                    End Using

                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0004_USERPASS OPEN")

                    CS0011LOGWrite.INFSUBCLASS = "Main"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "OIS0004_USERPASS OPEN"                           '
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    Return ret

                End Try

                Dim StrBldr = New StringBuilder()
                StrBldr.AppendLine(" SELECT")
                StrBldr.AppendLine("     USERID")
                StrBldr.AppendLine(" FROM")
                StrBldr.AppendLine("     com.OIS0005_USERPASS")
                StrBldr.AppendLine(" WHERE")
                StrBldr.AppendLine("     USERID = @P01")
                StrBldr.AppendLine(" AND CONVERT(NVARCHAR, DecryptByKey(PASSWORD)) = @P02")
                StrBldr.AppendLine(" AND PASSENDYMD >= @P03")
                StrBldr.AppendLine(" AND DELFLG <> @P00")


                Using SQLcmd As SqlCommand = New SqlCommand(StrBldr.ToString, SQLcon)
                    Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)

                    PARA00.Value = BaseDllConst.C_DELETE_FLG.DELETE
                    PARA01.Value = userid
                    PARA02.Value = password
                    PARA03.Value = Date.Now

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            ret = True
                        End If
                    End Using

                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0002PC SELECT USERPASS")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0002PC SELECT USERPASS"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

        End Try

        Return ret
    End Function

End Class

Imports System.Data.SqlClient
Imports BASEDLL

''' <summary>
''' パスワード変更
''' </summary>
''' <remarks></remarks>
Public Class GRCO0014USERPASS
    Inherits Page

    '○ 更新結果格納Table
    Private CO0014UPDtbl As DataTable                   '更新用テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite        'ログ出力
    Private CS0020JOURNAL As New CS0020JOURNAL          '更新ジャーナル出力
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    '○ 共通処理結果
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

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                    End Select
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(CO0014UPDtbl) Then
                CO0014UPDtbl.Clear()
                CO0014UPDtbl.Dispose()
                CO0014UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRCO0014WRKINC.MAPID

        WF_PASSWORD.Focus()

        '画面間の情報クリア
        work.Initialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        Dim SQLStr As String =
              " SELECT" _
            & "    RTRIM(USERID)     AS USERID" _
            & "    , RTRIM(PASSWORD) AS PASSWORD" _
            & "    , MISSCNT" _
            & "    , PASSENDYMD" _
            & " FROM" _
            & "    S0014_USERPASS" _
            & " WHERE" _
            & "    USERID      = @P1" _
            & "    AND DELFLG <> @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)

                PARA1.Value = CS0050SESSION.USERID
                PARA2.Value = C_DELETE_FLG.DELETE

                Dim WW_DATE As Date
                Dim WW_DATE_NOW As Date = Date.Now

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Date.TryParse(SQLdr("PASSENDYMD"), WW_DATE)
                        Master.output(C_MESSAGE_NO.PASSWORD_VALID_LIMIT, C_MESSAGE_TYPE.NOR, WW_DATE.ToString("yyyy年MM月dd日"))
                        WF_INFO.Text = "有効期限まで残り " & DateDiff("d", WW_DATE_NOW, WW_DATE).ToString() & " 日です。"
                        Exit While
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0014_USERPASS SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0014_USERPASS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub


    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ チェック処理
        WW_Check(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'ユーザーパスワードマスタ更新
                UpdateUserPass(SQLcon)
                MAPDataGet(SQLcon)
            End Using
        End If

        WF_PASSWORD.Text = ""
        WF_PASSWORD_R.Text = ""
        WF_PASSWORD.Focus()

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_PASSWORD As Date
        Dim WW_PASSWORD_R As Date

        '○ 単項目チェック
        '新しいパスワード
        If WF_PASSWORD.Text = "" Then
            Master.output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            Exit Sub
        End If

        Try
            Date.TryParse(WF_PASSWORD.Text, WW_PASSWORD)
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.CAST_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "新しいパスワード : " & WF_PASSWORD.Text)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"
            CS0011LOGWrite.INFPOSI = "新しいパスワード"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.CAST_FORMAT_ERROR
            CS0011LOGWrite.CS0011LOGWrite()

            O_RTN = C_MESSAGE_NO.CAST_FORMAT_ERROR
            Exit Sub
        End Try

        '(再入力)新しいパスワード
        If WF_PASSWORD_R.Text = "" Then
            Master.output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            Exit Sub
        End If

        Try
            Date.TryParse(WF_PASSWORD_R.Text, WW_PASSWORD_R)
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.CAST_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "(再入力)新しいパスワード : " & WF_PASSWORD_R.Text)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"
            CS0011LOGWrite.INFPOSI = "(再入力)新しいパスワード"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.CAST_FORMAT_ERROR
            CS0011LOGWrite.CS0011LOGWrite()

            O_RTN = C_MESSAGE_NO.CAST_FORMAT_ERROR
            Exit Sub
        End Try

        '項目一致チェック
        If WF_PASSWORD.Text <> WF_PASSWORD_R.Text Then
            Master.output(C_MESSAGE_NO.REINPUT_DATA_UNMATCH_ERROR, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.REINPUT_DATA_UNMATCH_ERROR
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' ユーザーパスワードマスタ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateUserPass(ByVal SQLcon As SqlConnection)

        Dim WW_DATENOW As DateTime = Date.Now

        '○ ＤＢ更新
        Dim SQLStr As String =
              " UPDATE S0014_USERPASS" _
            & " SET" _
            & "    PASSWORD     = @P2" _
            & "    , MISSCNT    = @P3" _
            & "    , PASSENDYMD = @P4" _
            & "    , DELFLG     = @P5" _
            & "    , UPDYMD     = @P6" _
            & "    , UPDUSER    = @P7" _
            & "    , UPDTERMID  = @P8" _
            & "    , RECEIVEYMD = @P9" _
            & " WHERE" _
            & "    USERID = @P1"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    USERID" _
            & "    , PASSWORD" _
            & "    , MISSCNT" _
            & "    , PASSENDYMD" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) TIMSTP" _
            & " FROM" _
            & "    S0014_USERPASS" _
            & " WHERE" _
            & "    USERID = @P1"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            'ユーザＩＤ
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 30)            'パスワード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Int)                     '誤り回数
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                    'パスワード有効期限
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             '削除フラグ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.DateTime)                '更新年月日
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)            '更新ユーザＩＤ
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 30)            '更新端末
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.DateTime)                '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        'ユーザＩＤ

                'ＤＢ更新
                PARA1.Value = CS0050SESSION.USERID
                PARA2.Value = WF_PASSWORD.Text
                PARA3.Value = 0
                PARA4.Value = WW_DATENOW.AddMonths(3)
                PARA5.Value = C_DELETE_FLG.ALIVE
                PARA6.Value = WW_DATENOW
                PARA7.Value = Master.USERID
                PARA8.Value = Master.USERTERMID
                PARA9.Value = C_DEFAULT_YMD

                SQLcmd.ExecuteNonQuery()

                '更新ジャーナル出力
                JPARA1.Value = CS0050SESSION.USERID

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(CO0014UPDtbl) Then
                        CO0014UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            CO0014UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    CO0014UPDtbl.Clear()
                    CO0014UPDtbl.Load(SQLdr)
                End Using

                For Each CO0014UPDrow As DataRow In CO0014UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "S0014_USERPASS"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = CO0014UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

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
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0014_USERPASS UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0014_USERPASS UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.transitionPrevPage()

    End Sub

End Class

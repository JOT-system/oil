Imports System.Data.SqlClient
Imports BASEDLL

Public Class GRCO0015ONLINESTAT
    Inherits Page

    '検索結果格納ds
    Private CO0015tbl As DataTable                      'Grid格納用テーブル

    '*共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite        'LogOutput DirString Get
    Private CS0020JOURNAL As New CS0020JOURNAL          'Journal Out
    Private CS0050Session As New CS0050SESSION          'セッション管理クラス

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">ボタン</param>
    ''' <param name="e">押下時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonEND"
                            WF_ButtonEND_Click()
                    End Select
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(CO0015tbl) Then
                CO0015tbl.Clear()
                CO0015tbl.Dispose()
                CO0015tbl = Nothing
            End If
        End Try

        WF_Guidance.Focus()

    End Sub

    ''' <summary>
    ''' 初期処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Initialize()
        Master.dispHelp = False
        '■■■　ガイダンス情報取得　■■■
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection()
                SQLcon.Open()       'DataBase接続(Open)

                'S0029_ONLINESTAT検索SQL文
                Dim SQLStr As String =
                      " SELECT rtrim(TEXT)         as TEXT " _
                    & " FROM      S0029_ONLINESTAT         " _
                    & " Where TERMID      = @P1            " _
                    & "   and CAMPCODE    = @P2            " _
                    & "   and DELFLG     <> '1'            "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 30)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)

                    PARA1.Value = CS0050Session.APSV_ID
                    PARA2.Value = work.WF_SEL_CAMPCODE.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            WF_Guidance.Text = SQLdr("TEXT")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0029_ONLINESTAT SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "S0029_ONLINESTAT SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '■　DB更新（チェックOK時）
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection()
                SQLcon.Open()       'DataBase接続(Open)

                Dim SQLStr As String =
                      " UPDATE S0029_ONLINESTAT " _
                    & "       SET    TEXT              = @P02 , " _
                    & "              UPDYMD            = @P03 , " _
                    & "              UPDUSER           = @P04 , " _
                    & "              UPDTERMID         = @P05   " _
                    & "     WHERE    TERMID            = @P01   " _
                    & "       AND    CAMPCODE          = @P06   "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 30)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 500)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.DateTime)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 30)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 20)

                    Dim WW_NOW As DateTime = Date.Now

                    PARA01.Value = CS0050Session.APSV_ID
                    PARA02.Value = WF_Guidance.Text
                    PARA03.Value = WW_NOW
                    PARA04.Value = Master.USERID
                    PARA05.Value = Master.USERTERMID
                    PARA06.Value = work.WF_SEL_CAMPCODE.Text

                    SQLcmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0029_ONLINESTAT UPDATE")
            CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "S0029_ONLINESTAT UPDATE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '■更新ジャーナル作成
        '  準備
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection()
                SQLcon.Open()       'DataBase接続(Open)

                'S0029_ONLINESTAT検索SQL文
                Dim SQLStr As String =
                      " SELECT * " _
                    & " FROM  S0029_ONLINESTAT " _
                    & " Where TERMID    = @P1  " _
                    & "   and CAMPCODE  = @P2  " _
                    & "   and DELFLG   <> '1'  "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 30)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)

                    PARA1.Value = CS0050Session.APSV_ID
                    PARA2.Value = work.WF_SEL_CAMPCODE.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            'CO0015テンポラリDB項目作成
                            CO0015tbl = New DataTable
                            CO0015tbl.Columns.Add("TERMID", GetType(String))
                            CO0015tbl.Columns.Add("CAMPCODE", GetType(String))
                            CO0015tbl.Columns.Add("ONLINESW", GetType(Integer))
                            CO0015tbl.Columns.Add("TEXT", GetType(String))
                            CO0015tbl.Columns.Add("DELFLG", GetType(String))
                            CO0015tbl.Columns.Add("INITYMD", GetType(Date))
                            CO0015tbl.Columns.Add("UPDYMD", GetType(Date))
                            CO0015tbl.Columns.Add("UPDUSER", GetType(String))
                            CO0015tbl.Columns.Add("UPDTERMID", GetType(String))
                            CO0015tbl.Columns.Add("RECEIVEYMD", GetType(DateTime))

                            CS0020JOURNAL.ROW = CO0015tbl.NewRow

                            CS0020JOURNAL.TABLENM = "S0015_ONLINESTAT"
                            CS0020JOURNAL.ACTION = "UPDATE"
                            CS0020JOURNAL.ROW("TERMID") = SQLdr("TERMID")
                            CS0020JOURNAL.ROW("CAMPCODE") = SQLdr("CAMPCODE")
                            CS0020JOURNAL.ROW("ONLINESW") = SQLdr("ONLINESW")
                            CS0020JOURNAL.ROW("TEXT") = SQLdr("TEXT")
                            CS0020JOURNAL.ROW("DELFLG") = SQLdr("DELFLG")
                            CS0020JOURNAL.ROW("INITYMD") = SQLdr("INITYMD")
                            CS0020JOURNAL.ROW("UPDYMD") = SQLdr("UPDYMD")
                            CS0020JOURNAL.ROW("UPDUSER") = SQLdr("UPDUSER")
                            CS0020JOURNAL.ROW("UPDTERMID") = SQLdr("UPDTERMID")
                            CS0020JOURNAL.ROW("RECEIVEYMD") = SQLdr("RECEIVEYMD")
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")
                                CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWRITE.TEXT = "CS0020JOURNAL Call err!"
                                CS0011LOGWRITE.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                                Exit Sub
                            End If
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)
            CS0011LOGWRITE.INFSUBCLASS = "Main"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "S0029_ONLINESTAT SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '■完了メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        '表示項目
        WF_Guidance.Focus()

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        Master.transitionPrevPage()
    End Sub

End Class

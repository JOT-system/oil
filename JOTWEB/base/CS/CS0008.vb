Imports System.Data.SqlClient

''' <summary>
''' オンライン状態取得
''' </summary>
''' <remarks></remarks>
Public Class CS0008ONLINEstat : Implements IDisposable

    ''' <summary>
    ''' 確認する端末
    ''' </summary>
    ''' <value>端末ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' 確認する会社コード
    ''' </summary>
    ''' <value>端末ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property COMPCODE() As String
    ''' <summary>
    ''' オンライン状態
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ONLINESW() As Integer

    ''' <summary>
    ''' :業務連絡テキスト
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TEXT() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0008ONLINEstat"

    ''' <summary>
    ''' 端末に対するオンライン状態を確認する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0008ONLINEstat()
        Dim sm As CS0050SESSION = New CS0050SESSION()
        'PARM EXTRA01: 端末ID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If
        '●オンライン状態取得
        '○ 画面UserIDのDB(S0015_ONLINESTAT)検索
        Try
            'DataBase接続文字
            'Dim SQLcon As New SqlConnection(HttpContext.Current.Session("DBcon"))
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'S0015_ONLINESTAT検索SQL文
                Dim SQL_Str As String =
                     "SELECT " _
                   & "  isnull(ONLINESW,0) as ONLINESW  " _
                   & ", rtrim(TEXT)        as TEXT      " _
                   & " FROM                             " _
                   & "  com.OIS0020_ONLINESTAT                " _
                   & " Where TERMID  = @P1              " _
                   & "   and DELFLG <> @P2              "

                If (String.IsNullOrEmpty(Me.COMPCODE) = False) Then SQL_Str &= String.Format(" and CAMPCODE = '{0}' ", Me.COMPCODE)
                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = TERMID
                    PARA2.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    ONLINESW = 0
                    ERR = C_MESSAGE_NO.DB_ERROR
                    Dim swcnt As Integer = 0
                    While SQLdr.Read
                        ONLINESW += SQLdr("ONLINESW")
                        TEXT &= SQLdr("TEXT")
                        swcnt += 1
                        ERR = C_MESSAGE_NO.NORMAL
                    End While
                    ONLINESW = ONLINESW / swcnt
                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0015_ONLINESTAT Select"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Public Sub Dispose(ByVal isDispose As Boolean)
        If isDispose Then

        End If
    End Sub
End Class

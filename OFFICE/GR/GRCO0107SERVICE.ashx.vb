Imports System.Data.SqlClient
Imports System.ServiceProcess
Imports BASEDLL

Public Class GRCO0107SERVICE
    Implements IHttpHandler, IRequiresSessionState

    ''' <summary>
    ''' ジョブ管理バッチのサービス名
    ''' </summary>
    ''' <remarks></remarks>
    Dim WW_ServiceNameCB0011 As String = "CB0011JobControl"

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim CS0001INIFILE As New CS0001INIFILEget
        Dim CS0050Session = New CS0050SESSION
        '■要求取得
        CS0001INIFILE.CS0001INIFILEget()
        If Not isNormal(CS0001INIFILE.ERR) Then Exit Sub
        '要求KeyWordを取得
        Dim WW_Reader As New System.IO.StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8)
        Dim WW_ReqSTR As String = HttpUtility.UrlDecode(WW_Reader.ReadToEnd(), Encoding.UTF8)
        Dim WW_REQSPLIT As String() = WW_ReqSTR.Split(",")
        WW_ReqSTR = WW_REQSPLIT(0)
        Dim WW_TERMID = If(WW_REQSPLIT.Length > 1, WW_REQSPLIT(1), CS0050Session.APSV_ID)
        Dim WW_COMPCD = If(WW_REQSPLIT.Length > 2, WW_REQSPLIT(2), String.Empty)
        Dim WW_USERID = If(WW_REQSPLIT.Length > 3, WW_REQSPLIT(3), CS0050Session.USERID)

        '閉じる
        WW_Reader.Close()

        '■サービス関連コマンド処理
        Dim objWinServ As New ServiceController
        Dim wStatus As String = ""

        objWinServ.ServiceName = WW_ServiceNameCB0011
        objWinServ.MachineName = "localhost"

        '〇サービス状態確認
        If WW_ReqSTR = RemoteServiceCmd.C_GET_SERVICE_STATUS OrElse WW_ReqSTR = RemoteServiceCmd.C_START_SERVICE OrElse WW_ReqSTR = RemoteServiceCmd.C_STOP_SERVICE Then
            Try
                Select Case objWinServ.Status
                    Case ServiceControllerStatus.Running    '実行中
                        wStatus = "Running"
                    Case ServiceControllerStatus.Stopped    '停止
                        wStatus = "Stopped"
                    Case Else
                        wStatus = "Intermidiate"
                End Select

                Try
                    If wStatus = "Running" Then
                        'DataBase接続文字
                        Using SQLcon As SqlConnection = CS0050Session.getConnection
                            SQLcon.Open() 'DataBase接続(Open)

                            Dim SQL_Str As String = ""
                            '指定された端末IDより振分先を取得
                            SQL_Str = _
                                    " SELECT isnull(JOBSTAT,0) as JOBSTAT " & _
                                    " FROM S0019_JOBCNTL       " & _
                                    " WHERE TERMID       =  '" & Trim(WW_TERMID) & "' " & _
                                    " AND   DELFLG       <> '1' "
                            Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                                Dim WW_JOBSTAT As String = ""
                                While SQLdr.Read
                                    If SQLdr("JOBSTAT") = "1" Then wStatus = "RunningJOB"
                                End While

                                If SQLdr.HasRows = False Then
                                    wStatus = "Data Not Found : TERMID=" & Trim(WW_TERMID)
                                    context.Response.StatusCode = 304
                                    context.Response.ContentType = "text/plain"
                                    context.Response.Write(wStatus)
                                End If

                                'Close
                                SQLdr.Close() 'Reader(Close)
                                SQLdr = Nothing

                            End Using

                        End Using

                    End If

                Catch ex As Exception
                    wStatus = ex.ToString()
                    context.Response.StatusCode = 304               'エラーリターン(textStatus:errorとなる)
                    context.Response.ContentType = "text/plain"
                    context.Response.Write(wStatus)
                    Exit Sub
                End Try

                context.Response.StatusCode = 200               '正常リターン

            Catch ex As Exception
                wStatus = ex.ToString()
                context.Response.StatusCode = 301               'エラーリターン(textStatus:errorとなる)
                context.Response.ContentType = "text/plain"
                context.Response.Write(wStatus)
                Exit Sub
            End Try
        End If

        '〇サービス開始
        If WW_ReqSTR = RemoteServiceCmd.C_START_SERVICE AndAlso (wStatus = "Stopped" OrElse wStatus = "Intermidiate") Then
            Try
                objWinServ.Start()
                objWinServ.WaitForStatus(ServiceControllerStatus.Running, System.TimeSpan.FromSeconds(20))
                wStatus = "ServiceRunning"
                context.Response.StatusCode = 200               '正常リターン
            Catch e As Exception
                wStatus = e.ToString()
                'wStatus = "ServiceNotRunning"
                context.Response.StatusCode = 302               'エラーリターン(textStatus:errorとなる)
                context.Response.ContentType = "text/plain"
                context.Response.Write(wStatus)
                Exit Sub
            End Try
        End If

        '〇サービス停止
        If WW_ReqSTR = RemoteServiceCmd.C_STOP_SERVICE AndAlso wStatus = "Running" Then
            Try
                objWinServ.Stop()
                objWinServ.WaitForStatus(ServiceControllerStatus.Stopped, System.TimeSpan.FromSeconds(20))
                wStatus = "ServiceStopped"
                context.Response.StatusCode = 200               '正常リターン
            Catch e As Exception
                wStatus = e.ToString()
                context.Response.StatusCode = 303               'エラーリターン(textStatus:errorとなる)
                context.Response.ContentType = "text/plain"
                context.Response.Write(wStatus)
                Exit Sub
            End Try
        End If

        '■オンラインサービスコマンド処理


        '○ オンライン状態テーブル取得
        If WW_ReqSTR = RemoteServiceCmd.C_GET_ONLINE_STATUS Then
            Try
                wStatus = ""

                'DataBase接続文字
                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    '指定された端末IDより振分先を取得
                    Dim SQL_Str As String = _
                            " SELECT isnull(ONLINESW,0) as ONLINESW " & _
                            " FROM S0029_ONLINESTAT       " & _
                            " WHERE   TERMID        = '" & Trim(WW_TERMID) & "' " & _
                            "   AND   DELFLG       <> '1' "

                    If (String.IsNullOrEmpty(WW_COMPCD) = False) Then SQL_Str &= String.Format(" and CAMPCODE = '{0}' ", WW_COMPCD)

                    Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                        Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        Dim cnt As Short = 0
                        While SQLdr.Read
                            cnt += 1
                            wStatus = SQLdr("ONLINESW") + Val(wStatus)
                        End While

                        wStatus = wStatus / cnt
                        context.Response.StatusCode = 200               '正常リターン

                        'Close
                        SQLdr.Close() 'Reader(Close)
                        SQLdr = Nothing
                    End Using
                End Using

            Catch ex As Exception
                wStatus = ex.ToString()
                'wStatus = ""
                context.Response.StatusCode = 304               'エラーリターン(textStatus:errorとなる)
                context.Response.ContentType = "text/plain"
                context.Response.Write(wStatus)
                Exit Sub
            End Try

        End If

        '○オンライン状態テーブル更新
        If WW_ReqSTR = RemoteServiceCmd.C_START_ONLINE_STATUS OrElse WW_ReqSTR = RemoteServiceCmd.C_STOP_ONLINE_STATUS Then
            Dim wSW As String = "0"
            If WW_ReqSTR = RemoteServiceCmd.C_START_ONLINE_STATUS Then wSW = "1"

            Try
                'DataBase接続文字
                Using SQLcon As SqlConnection = CS0050Session.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    '指定された端末IDより振分先を取得
                    Dim SQL_Str As String = _
                                " UPDATE S0029_ONLINESTAT     " _
                                & " SET   ONLINESW     =  '" & wSW & "' " _
                                & "      ,UPDYMD       =  '" & Date.Now & "' " _
                                & "      ,UPDUSER      =  '" & WW_USERID & "' " _
                                & "      ,UPDTERMID    =  '" & WW_TERMID & "' " _
                                & "      ,RECEIVEYMD   =  '" & C_DEFAULT_YMD & "' " _
                                & " WHERE TERMID       =  '" & Trim(WW_TERMID) & "' "
                    If (String.IsNullOrEmpty(WW_COMPCD) = False) Then SQL_Str &= String.Format(" and CAMPCODE = '{0}' ", WW_COMPCD)

                    Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                        SQLcmd.ExecuteNonQuery()

                        wStatus = wSW
                        context.Response.StatusCode = 200               '正常リターン
                    End Using
                End Using

            Catch ex As Exception
                wStatus = ex.ToString()
                'wStatus = ""
                context.Response.StatusCode = 305               'エラーリターン(textStatus:errorとなる)
                context.Response.ContentType = "text/plain"
                context.Response.Write(wStatus)
                Exit Sub
            End Try
        End If

        'Clear
        objWinServ = Nothing

        '結果送信
        context.Response.ContentType = "text/plain"
        context.Response.Write(wStatus)

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
''' <summary>
''' 遠隔サービス確認コマンド一覧
''' </summary>
''' <remarks></remarks>
Public Class RemoteServiceCmd
    Public Const C_GET_ONLINE_STATUS As String = "ONLINESTATget"
    Public Const C_STOP_ONLINE_STATUS As String = "ONLINESTATset0"
    Public Const C_START_ONLINE_STATUS As String = "ONLINESTATset1"
    Public Const C_GET_SERVICE_STATUS As String = "SERVICESTATget"
    Public Const C_STOP_SERVICE As String = "SERVICEstop"
    Public Const C_START_SERVICE As String = "SERVICEstop"

End Class

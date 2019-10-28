Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Data.SqlClient
Imports System.Net
Imports System.Text.RegularExpressions

''' <summary>
'''  FTPクライアントクラス
''' </summary>
''' <remarks></remarks>
Public Class CS0053FtpClient

#Region "FtpFileInfo"

    ''' <summary>
    ''' FTPファイル情報クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class FtpFileInfo

        ''' <summary>
        ''' ファイル名（フルパス）
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property FullName As String
            Get
                If Path.EndsWith("/") Then
                    Return (Path + Filename)
                Else
                    Return (Path + "/" + Filename)
                End If
            End Get
        End Property
        ''' <summary>
        ''' ファイル名
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Filename As String
            Get
                Return _filename
            End Get
        End Property
        ''' <summary>
        ''' ファイルパス
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Path As String
            Get
                Return _path
            End Get
        End Property
        ''' <summary>
        ''' ファイルタイプ
        ''' </summary>
        ''' <remarks>File or Directory</remarks>
        Public ReadOnly Property FileType As DirectoryEntryTypes
            Get
                Return _fileType
            End Get
        End Property
        ''' <summary>
        ''' ファイルサイズ
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Size As Long
            Get
                Return _size
            End Get
        End Property
        ''' <summary>
        ''' ファイル更新日時
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property FileDateTime As DateTime
            Get
                Return _fileDateTime
            End Get
        End Property
        ''' <summary>
        ''' パーミッション
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Permission As String
            Get
                Return _permission
            End Get
        End Property
        ''' <summary>
        ''' ファイル拡張子
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Extension As String
            Get
                Dim i As Integer = Me.Filename.LastIndexOf(".")
                If ((i >= 0) AndAlso (i < (Me.Filename.Length - 1))) Then
                    Return Me.Filename.Substring((i + 1))
                Else
                    Return String.Empty
                End If

            End Get
        End Property
        ''' <summary>
        ''' ファイル名（拡張子除く）
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property NameOnly As String
            Get
                Dim i As Integer = Me.Filename.LastIndexOf(".")
                If (i > 0) Then
                    Return Me.Filename.Substring(0, i)
                Else
                    Return Me.Filename
                End If

            End Get
        End Property

        Private _filename As String
        Private _path As String
        Private _fileType As DirectoryEntryTypes
        Private _size As Long
        Private _fileDateTime As DateTime
        Private _permission As String

        Public Enum DirectoryEntryTypes
            File
            Directory
        End Enum

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="line">LIST取得行データ</param>
        ''' <param name="path">取得ディレクトリ</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal line As String, ByVal path As String)

            ' ls取得行の分解
            Dim m As Match = Me.GetMatchingRegex(line)
            If IsNothing(m) Then
                Throw New ApplicationException(("Unable to parse line: " + line))
            Else
                Me._filename = m.Groups("name").Value
                Me._path = path
                Int64.TryParse(m.Groups("size").Value, Me._size)
                Me._permission = m.Groups("permission").Value
                Dim _dir As String = m.Groups("dir").Value
                If ((_dir <> "") AndAlso (_dir <> "-")) Then
                    Me._fileType = DirectoryEntryTypes.Directory
                Else
                    Me._fileType = DirectoryEntryTypes.File
                End If

                Try
                    Me._fileDateTime = DateTime.Parse(m.Groups("timestamp").Value)
                Catch ex As Exception
                    Me._fileDateTime = DateTime.MinValue
                End Try

            End If

        End Sub

        ''' <summary>
        ''' LIST取得行データ解析分解
        ''' </summary>
        ''' <param name="line">LIST取得行データ</param>
        ''' <returns >分解結果</returns>
        ''' <remarks>ls結果を各項目に分解(dir,name,size,permission,timestamp)</remarks>
        Private Function GetMatchingRegex(ByVal line As String) As Match
            Dim rx As Regex
            Dim m As Match
            Dim i As Integer = 0
            Do While (i <= (_ParseFormats.Length - 1))
                rx = New Regex(_ParseFormats(i))
                m = rx.Match(line)
                If m.Success Then
                    Return m
                End If
                i = (i + 1)
            Loop

            Return Nothing

        End Function

        Private Shared _ParseFormats() As String = New String() { _
            "(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)", _
            "(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{4})\s+(?<name>.+)", _
            "(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\d+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)", _
            "(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})\s+\d+\s+\w+\s+\w+\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+\s+\d{1,2}:\d{2})\s+(?<name>.+)", _
            "(?<dir>[\-d])(?<permission>([\-r][\-w][\-xs]){3})(\s+)(?<size>(\d+))(\s+)(?<ctbit>(\w+\s\w+))(\s+)(?<size2>(\d+))\s+(?<timestamp>\w+\s+\d+\s+\d{2}:\d{2})\s+(?<name>.+)", _
            "(?<timestamp>\d{2}\-\d{2}\-\d{2}\s+\d{2}:\d{2}[Aa|Pp][mM])\s+(?<dir>\<\w+\>){0,1}(?<size>\d+){0,1}\s+(?<name>.+)" _
            }
    End Class
#End Region

#Region "FtpDirectory"

    ''' <summary>
    ''' FTPディレクトリクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class FtpDirectory
        Inherits List(Of FtpFileInfo)

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="path"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal dir As String, ByVal path As String)
            For Each line As String In dir.Replace(vbLf, "").Split(System.Convert.ToChar(vbCr))
                If (line <> "") Then
                    Me.Add(New FtpFileInfo(line, path))
                End If
            Next
        End Sub

        ''' <summary>
        ''' ファイル一覧取得
        ''' </summary>
        ''' <param name="ext">拡張子</param>
        ''' <returns>FTPdirectory listing</returns>
        Public Function GetFiles(Optional ByVal ext As String = "") As FtpDirectory
            Return Me.GetFileOrDir(FtpFileInfo.DirectoryEntryTypes.File, ext)
        End Function

        ''' <summary>
        ''' ディレクトリ一覧取得
        ''' </summary>
        ''' <returns>FTPDirectory list</returns>
        ''' <remarks></remarks>
        Public Function GetDirectories() As FtpDirectory
            Return Me.GetFileOrDir(FtpFileInfo.DirectoryEntryTypes.Directory, "")
        End Function

        ''' <summary>
        ''' ディレクトリ・ファイル一覧取得
        ''' </summary>
        ''' <returns>FTPDirectory list</returns>
        ''' <remarks></remarks>
        Private Function GetFileOrDir(ByVal type As FtpFileInfo.DirectoryEntryTypes, ByVal ext As String) As FtpDirectory
            Dim result As FtpDirectory = New FtpDirectory
            For Each fi As FtpFileInfo In Me
                If (fi.FileType = type) Then
                    If (ext = "") Then
                        result.Add(fi)
                    ElseIf (ext = fi.Extension) Then
                        result.Add(fi)
                    End If
                End If
            Next
            Return result
        End Function
        ''' <summary>
        ''' ファイル存在確認
        ''' </summary>
        ''' <returns>存在有無</returns>
        ''' <remarks></remarks>
        Public Function FileExists(ByVal filename As String) As Boolean
            For Each ftpfile As FtpFileInfo In Me
                If (ftpfile.Filename = filename) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private Const slash As Char = Microsoft.VisualBasic.ChrW(47)

        ''' <summary>
        ''' 上位ディレクトリ取得
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetParentDirectory(ByVal dir As String) As String
            Dim tmp As String = dir.TrimEnd(slash)
            Dim i As Integer = tmp.LastIndexOf(slash)
            If (i > 0) Then
                Return tmp.Substring(0, (i - 1))
            Else
                Throw New ApplicationException("No parent for root")
            End If

        End Function
    End Class
#End Region


    ''' <summary>
    ''' FTPサーバ情報
    ''' </summary>
    Private Class FTP_SERVER
        Public SERVERID As String
        Public SERVERNAME As String
        Public IPADDR As String
        Public IPADDR_SECONDARY As String
        Public PORT As Integer
        Public FTP_USER As String
        Public FTP_PASSWORD As String
        Public PASV_MODE As String
    End Class

    ''' <summary>
    ''' FTPサーバ管理
    ''' </summary>
    Private _ftpServ As FTP_SERVER
    ''' <summary>
    ''' FTPユーザ
    ''' </summary>
    ''' <returns>FTP_USER</returns>
    Public ReadOnly Property FTP_USER As String
        Get
            Return _ftpServ.FTP_USER
        End Get
    End Property
    ''' <summary>
    ''' FTPパスワード
    ''' </summary>
    ''' <returns>FTP_PASS</returns>
    Public ReadOnly Property FTP_PASS As String
        Get
            Return _ftpServ.FTP_PASSWORD
        End Get
    End Property

    ''' <summary>
    ''' IPADDR
    ''' </summary>
    ''' <returns>IPADDR</returns>
    Public ReadOnly Property IPADDR As String
        Get
            Return _ftpServ.IPADDR
        End Get
    End Property

    Private _serverId As String
    ''' <summary>
    ''' FTPサーバID
    ''' </summary>
    ''' <returns>SERVERID</returns>
    Public ReadOnly Property SERVERID As String
        Get
            Return _serverId
        End Get
    End Property


    ''' <summary>
    ''' ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR As String
    ''' <summary>
    ''' DB接続文字列
    ''' </summary>
    ''' <value></value>
    Public Property DBCon As String


    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="I_serverId" >FTPサーバ</param>
    ''' <remarks></remarks> 
    Public Sub New(ByVal I_serverId As String, Optional ByRef I_DBCon As String = "")
        'プロパティ初期化
        Initialize()

        _serverId = I_serverId
        If Not String.IsNullOrEmpty(I_DBCon) Then
            Me.DBCon = I_DBCon
        End If

        ' FTPサーバ情報取得
        _ftpServ = GetFtpServInfo()
        If (IsNothing(_ftpServ)) Then
            Me.ERR = C_MESSAGE_NO.DB_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "S0027_FTPSERVER SERVERID NotFound")
        End If

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        Me.ERR = C_MESSAGE_NO.NORMAL
        Me._ftpServ = Nothing
        Me.DBCon = String.Empty

    End Sub

    ''' <summary>
    ''' FTPサーバ情報取得
    ''' </summary>
    ''' <returns>FTP_SERVER Object</returns>
    ''' <remarks> OK:00000</remarks> 
    Private Function GetFtpServInfo() As FTP_SERVER
        'セッション管理
        Dim sm As New CS0050SESSION

        Dim ftpserv As FTP_SERVER = Nothing

        Try
            'DataBase接続文字
            Using SQLcon = If(String.IsNullOrEmpty(DBCon), sm.getConnection, New SqlConnection(DBCon))
                SQLcon.Open() 'DataBase接続(Open)

                Dim sb As New StringBuilder
                sb.Append("SELECT ")
                sb.Append("    s.SERVERID ")
                sb.Append("  , isnull(s.SERVERNAME, '') as SERVERNAME ")
                sb.Append("  , isnull(s.IPADDR, '') as IPADDR ")
                sb.Append("  , isnull(s.IPADDR_SECONDARY, '') as IPADDR_SECONDARY ")
                sb.Append("  , isnull(s.PORT, 0) as PORT ")
                sb.Append("  , isnull(s.FTP_USER, '') as FTP_USER ")
                sb.Append("  , isnull(s.FTP_PASSWORD, '') as FTP_PASSWORD ")
                sb.Append("  , isnull(s.PASV_MODE, '') as PASV_MODE ")
                sb.Append("FROM ")
                sb.Append("  S0027_FTPSERVER s ")
                sb.Append("WHERE ")
                sb.Append("  s.SERVERID = @serverId ")
                sb.Append("  AND s.STYMD <= @stymd ")
                sb.Append("  AND s.ENDYMD >= @endymd ")
                sb.Append("  AND s.DELFLG <> @delflg ")

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim p_serverId As SqlParameter = SQLcmd.Parameters.Add("@serverId", System.Data.SqlDbType.NVarChar)
                    Dim p_stymd As SqlParameter = SQLcmd.Parameters.Add("@stymd", System.Data.SqlDbType.Date)
                    Dim p_endymd As SqlParameter = SQLcmd.Parameters.Add("@endymd", System.Data.SqlDbType.Date)
                    Dim p_delFlg As SqlParameter = SQLcmd.Parameters.Add("@delflg", System.Data.SqlDbType.NVarChar)
                    p_serverId.Value = _serverId
                    p_stymd.Value = Date.Now
                    p_endymd.Value = Date.Now
                    p_delFlg.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.HasRows Then
                            SQLdr.Read()
                            ftpserv = New FTP_SERVER With {
                                .SERVERID = SQLdr("SERVERID"),
                                .SERVERNAME = SQLdr("SERVERNAME"),
                                .IPADDR = SQLdr("IPADDR"),
                                .IPADDR_SECONDARY = SQLdr("IPADDR_SECONDARY"),
                                .PORT = SQLdr("PORT"),
                                .FTP_USER = SQLdr("FTP_USER"),
                                .FTP_PASSWORD = SQLdr("FTP_PASSWORD"),
                                .PASV_MODE = SQLdr("PASV_MODE")
                            }
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

        End Try

        Return ftpserv

    End Function

    ''' <summary>
    ''' FTP接続解除
    ''' </summary>
    ''' <remarks></remarks> 
    Public Function Close() As Boolean

        Try

            'FTPリクエスト[PWD]
            Dim WW_FTPreq = Me.GetRequest()
            WW_FTPreq.Method = WebRequestMethods.Ftp.PrintWorkingDirectory

            ' コマンド実行後接続解除
            WW_FTPreq.KeepAlive = False

            'FTPリクエスト実行
            Using WW_FTPres As System.Net.FtpWebResponse = CType(WW_FTPreq.GetResponse(), System.Net.FtpWebResponse)
                If Not WW_FTPres.ExitMessage.Contains(FtpStatusCode.ClosingControl) Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Disconnect Error")

                    Return False
                End If
            End Using

            Return True

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Disconnect Exception")

            Return False

        Finally
        End Try

    End Function

    ''' <summary>
    ''' FTPファイル一覧
    ''' </summary>
    ''' <param name="directory" >対象Dirパス（Server）</param>
    ''' <returns>List object</returns>
    ''' <remarks></remarks> 
    Public Function ListDirectory(ByVal directory As String) As List(Of String)

        'FTPリクエスト[NLST]
        Dim ftp As System.Net.FtpWebRequest = Me.GetRequest(directory)
        ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectory

        'FTPリクエスト実行
        Dim str As String = Me.GetStringResponse(ftp)
        ' カレントディレクト設定
        _lastDirectory = directory
        ' ディレクトリ一覧をFtpDirectoryオブジェクトで返却
        str = str.Replace("" & vbCrLf, "" & vbCr).TrimEnd(vbCr)
        Dim result As List(Of String) = New List(Of String)
        result.AddRange(str.Split(vbCr))
        Return result
    End Function

    ''' <summary>
    ''' FTPファイル一覧
    ''' </summary>
    ''' <param name="directory" >対象Dirパス（Server）</param>
    ''' <returns>FtpDirectory object</returns>
    ''' <remarks></remarks> 
    Public Function ListDirectoryDetail(ByVal directory As String) As FtpDirectory

        'FTPリクエスト[LIST]
        Dim ftp As System.Net.FtpWebRequest = Me.GetRequest(directory)
        ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails

        'FTPリクエスト実行
        Dim str As String = Me.GetStringResponse(ftp)
        ' カレントディレクト設定
        _lastDirectory = directory
        ' ディレクトリ一覧をFtpDirectoryオブジェクトで返却
        str = str.Replace("" & vbCrLf, "" & vbCr).TrimEnd(vbCr)
        Return New FtpDirectory(str, _lastDirectory)

    End Function

    ''' <summary>
    ''' FTPファイルダウンロード
    ''' </summary>
    ''' <param name="sourceFilename" >[From] 対象ファイル（Server）</param>
    ''' <param name="targetFI" >[To] Downloadファイル（Local）</param>
    ''' <param name="PermitOverwrite" >上書きモード　true:上書き(初期値), false:エラー</param>
    ''' <param name="typeBinary" >ファイル転送モード　true:binary, false:ascii(初期値)</param>
    ''' <remarks></remarks> 
    Public Overloads Function Download(ByVal sourceFilename As String, ByVal targetFI As FileInfo, Optional ByVal PermitOverwrite As Boolean = True, Optional ByVal typeBinary As Boolean = False) As Boolean

        ' Local同名ファイル存在・上書き権限チェック
        If (targetFI.Exists AndAlso Not PermitOverwrite) Then
            Throw New ApplicationException("Target file already exists")
        End If

        ' Download対象ファイル名チェック
        Dim target As String
        If (sourceFilename.Trim = String.Empty) Then
            Throw New ApplicationException("File not specified")
        ElseIf sourceFilename.Contains("/") Then
            target = Me.AdjustDir(sourceFilename)
        Else
            target = (CurrentDirectory + sourceFilename)
        End If

        Try
            'FTPリクエスト[RETR]
            Dim ftp = Me.GetRequest(target)
            ftp.Method = WebRequestMethods.Ftp.DownloadFile

            'ファイル転送モード設定（Default:Ascii）
            ftp.UseBinary = typeBinary

            'FTPリクエスト実行
            Using response As FtpWebResponse = CType(ftp.GetResponse, FtpWebResponse)
                'FTPファイルStream取得
                Using responseStream As System.IO.Stream = response.GetResponseStream()
                    'ローカルファイルを開く
                    Using fs As FileStream = targetFI.OpenWrite
                        'ローカルファイルStreamに書き込む
                        Dim buffer(1023) As Byte
                        While True
                            Dim readSize As Integer = responseStream.Read(buffer, 0, buffer.Length)
                            If readSize = 0 Then
                                Exit While
                            End If
                            fs.Write(buffer, 0, readSize)
                        End While
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Download Exception")

            Return False

        Finally
        End Try

    End Function
    Public Overloads Function Download(ByVal sourceFilename As String, ByVal localFilename As String, Optional ByVal PermitOverwrite As Boolean = True, Optional ByVal typeBinary As Boolean = False) As Boolean
        Dim fi As FileInfo = New FileInfo(localFilename)
        Return Me.Download(sourceFilename, fi, PermitOverwrite, typeBinary)
    End Function

    Public Overloads Function Download(ByVal file As FtpFileInfo, ByVal localFilename As String, Optional ByVal PermitOverwrite As Boolean = True, Optional ByVal typeBinary As Boolean = False) As Boolean
        Return Me.Download(file.FullName, localFilename, PermitOverwrite, typeBinary)
    End Function

    Public Overloads Function Download(ByVal file As FtpFileInfo, ByVal localFI As FileInfo, Optional ByVal PermitOverwrite As Boolean = True, Optional ByVal typeBinary As Boolean = False) As Boolean
        Return Me.Download(file.FullName, localFI, PermitOverwrite, typeBinary)
    End Function


    ''' <summary>
    ''' FTPファイルアップロード
    ''' </summary>
    ''' <param name="fi" >[From] Localファイル（Local）</param>
    ''' <param name="targetFilename" >[To] UPLOADファイル（Server）</param>
    ''' <param name="typeBinary" >ファイル転送モード　true:binary, false:ascii(初期値)</param>
    ''' <remarks></remarks> 
    Public Overloads Function Upload(ByVal fi As FileInfo, ByVal targetFilename As String, Optional ByVal typeBinary As Boolean = False) As Boolean

        Dim target As String
        If (targetFilename.Trim = "") Then
            target = (Me.CurrentDirectory + fi.Name)
        ElseIf targetFilename.Contains("/") Then
            target = Me.AdjustDir(targetFilename)
        Else
            target = (CurrentDirectory + targetFilename)
        End If
        Try
            'FTPリクエスト[STOR]
            Dim ftp = Me.GetRequest(target)
            ftp.Method = WebRequestMethods.Ftp.UploadFile

            'ファイル転送モード設定（Default:Ascii）
            ftp.UseBinary = typeBinary

            'アップロードStream取得
            Using WW_FTPrstrm As System.IO.Stream = ftp.GetRequestStream()
                'アップロードファイルを開く
                Using WW_FStream As FileStream = fi.OpenRead
                    'アップロードStreamに書き込む
                    Dim buffer(1023) As Byte
                    While True
                        Dim readSize As Integer = WW_FStream.Read(buffer, 0, buffer.Length)
                        If readSize = 0 Then
                            Exit While
                        End If
                        WW_FTPrstrm.Write(buffer, 0, readSize)
                    End While
                End Using
                WW_FTPrstrm.Close()
            End Using

            'FTPリクエスト実行
            Using WW_FTPres As System.Net.FtpWebResponse = CType(ftp.GetResponse(), System.Net.FtpWebResponse)
                If WW_FTPres.StatusCode <> FtpStatusCode.ClosingData Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Upload Error")

                    Return False
                End If
            End Using

            Return True

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Upload Exception")

            Return False

        Finally
        End Try

    End Function
    Public Overloads Function Upload(ByVal localFilename As String, ByVal targetFilename As String, Optional ByVal typeBinary As Boolean = False) As Boolean

        If Not File.Exists(localFilename) Then
            Throw New ApplicationException(("File (" + localFilename + ") not found"))
        End If

        Dim fi As FileInfo = New FileInfo(localFilename)
        Return Me.Upload(fi, targetFilename)
    End Function
    ''' <summary>
    ''' FTPファイル削除
    ''' </summary>
    ''' <param name="targetPath" >対象ファイルパス（Server）</param>
    ''' <remarks></remarks> 
    Public Function Delete(ByVal targetPath As String) As Boolean

        Try
            'FTPリクエスト[DELE]
            Dim ftp = Me.GetRequest(targetPath)
            ftp.Method = WebRequestMethods.Ftp.DeleteFile

            'FTPリクエスト実行
            Using WW_FTPres As System.Net.FtpWebResponse = CType(ftp.GetResponse(), System.Net.FtpWebResponse)
                If WW_FTPres.StatusCode <> FtpStatusCode.FileActionOK Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Delete Error")

                    Return False
                End If
            End Using

            Return True

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Delete Exception")

            Return False

        Finally
        End Try

    End Function

    ''' <summary>
    ''' FTPファイルリネーム
    ''' </summary>
    ''' <param name="sourceFilename" >元ファイル名（Server）</param>
    ''' <param name="newName" >新ファイル名（Server）</param>
    ''' <remarks></remarks> 
    Public Function Rename(ByVal sourceFilename As String, ByVal newName As String) As Boolean
        Try
            Dim source As String = Me.GetFullPath(sourceFilename)
            If Not Me.FileExists(source) Then
                Throw New FileNotFoundException("File (" & source & ") not found")
            End If

            Dim target As String = Me.GetFullPath(newName)
            If (target = source) Then
                Throw New ApplicationException("Source and target are the same")
            ElseIf Me.FileExists(target) Then
                Throw New ApplicationException("Target file (" & target & ") already exists")
            End If

            'FTPリクエスト[RENAME]
            Dim ftp = Me.GetRequest(sourceFilename)
            ftp.Method = WebRequestMethods.Ftp.Rename
            ftp.RenameTo = target

            'FTPリクエスト実行
            Using response As System.Net.FtpWebResponse = CType(ftp.GetResponse(), System.Net.FtpWebResponse)
                If response.StatusCode <> FtpStatusCode.FileActionOK Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Rename Error")

                    Return False
                End If
            End Using

            Return True
        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP Rename Exception")

            Return False

        Finally
        End Try
    End Function

    ''' <summary>
    ''' ファイルサイズ取得
    ''' </summary>
    ''' <param name="filename">対象ファイル名</param>
    ''' <returns></returns>
    ''' <remarks>Throws an exception if file does not exist</remarks>
    Public Function GetFileSize(ByVal filename As String) As Long
        Dim path As String
        If filename.Contains("/") Then
            path = Me.AdjustDir(filename)
        Else
            path = (Me.CurrentDirectory + filename)
        End If

        'FTPリクエスト[SIZE]
        Dim ftp = Me.GetRequest(filename)
        ftp.Method = WebRequestMethods.Ftp.GetFileSize

        'FTPリクエスト実行
        Dim response As FtpWebResponse = CType(ftp.GetResponse, FtpWebResponse)
        Dim size As Long = response.ContentLength
        response.Close()
        Return size


    End Function

    ''' <summary>
    ''' ファイル存在チェック
    ''' </summary>
    ''' <param name="filename">対象ファイル名</param>
    ''' <returns></returns>
    Public Function FileExists(ByVal filename As String) As Boolean
        Try
            Dim size As Long = Me.GetFileSize(filename)
            Return True
        Catch ex As WebException
            If ex.Message.Contains("550") Then
                Return False
            Else
                Throw
            End If
        End Try

    End Function

    ''' <summary>
    ''' FTP接続チェック
    ''' </summary>
    ''' <remarks></remarks> 
    Public Function ServExists() As Boolean

        Try
            'FTPリクエスト[PWD]
            Dim WW_FTPreq = Me.GetRequest()
            WW_FTPreq.Method = WebRequestMethods.Ftp.PrintWorkingDirectory

            'FTPリクエスト実行
            Using WW_FTPres As System.Net.FtpWebResponse = CType(WW_FTPreq.GetResponse(), System.Net.FtpWebResponse)
                If WW_FTPres.StatusCode <> FtpStatusCode.PathnameCreated Then
                    Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
                    PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP SERVER Check Error")

                    Return False
                End If
            End Using

            Return True

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "FTP SERVER Check Exception")

            Return False

        Finally
        End Try

    End Function

    ''' <summary>
    ''' FTPリクエストインスタンス取得
    ''' </summary>
    ''' <param name="path" >Path</param>
    ''' <returns>FtpWebRequest</returns>
    ''' <remarks></remarks> 
    Private Function GetRequest(Optional ByVal path As String = "") As System.Net.FtpWebRequest

        ' FTPサーバ情報未取得時は取得処理
        If IsNothing(_ftpServ) Then
            _ftpServ = GetFtpServInfo()
            If (IsNothing(_ftpServ)) Then
                Me.ERR = C_MESSAGE_NO.DB_ERROR
                PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "S0027_FTPSERVER SERVERID NotFound")
            End If
        End If

        ' URI作成
        Dim URI As String = String.Format("ftp://{0}", _ftpServ.IPADDR)
        If _ftpServ.PORT > 0 Then
            ' ポート設定時はポート指定
            URI &= String.Format(":{0}", _ftpServ.PORT)
        End If
        URI &= AdjustDir(path)

        ' FTPリクエスト作成
        Dim req As System.Net.FtpWebRequest = CType(FtpWebRequest.Create(URI), System.Net.FtpWebRequest)
        req.Credentials = New NetworkCredential(_ftpServ.FTP_USER, _ftpServ.FTP_PASSWORD)

        ' PASSIVモード
        If _ftpServ.PASV_MODE = "Y" Then
            req.UsePassive = True
        Else
            req.UsePassive = False
        End If
        ' 要求完了後接続継続
        req.KeepAlive = True

        Return req

    End Function

    ''' <summary>
    ''' FTPレスポンス文字列取得
    ''' </summary>
    ''' <param name="ftp">FtpRequest</param>
    ''' <returns>レスポンス文字列</returns>
    ''' <remarks></remarks>
    Private Function GetStringResponse(ByVal ftp As FtpWebRequest) As String
        Dim result As String = String.Empty
        Dim response As FtpWebResponse = CType(ftp.GetResponse, FtpWebResponse)
        Using datastream As Stream = response.GetResponseStream
            Using sr As StreamReader = New StreamReader(datastream)
                result = sr.ReadToEnd
            End Using
        End Using
        response.Close()
        Return result
    End Function
    Private Function GetFullPath(ByVal file As String) As String
        If file.Contains("/") Then
            Return Me.AdjustDir(file)
        Else
            Return (Me.CurrentDirectory + file)
        End If

    End Function
    Private Function AdjustDir(ByVal path As String) As String
        Return If(path.StartsWith("/"), path, "/" & path)
    End Function

    Private _lastDirectory As String = ""
    Private _currentDirectory As String = "/"
    Public Property CurrentDirectory As String
        Get
            Return _currentDirectory & If(Me._currentDirectory.EndsWith("/"), "", "/")
        End Get
        Set(value As String)
            If Not value.StartsWith("/") Then
                Throw New ApplicationException("Directory should start with /")
            End If
            Me._currentDirectory = value
        End Set
    End Property

    ''' <summary>
    ''' ログ出力
    ''' </summary>
    ''' <remarks></remarks> 
    Private Sub PutLog(ByVal messageNo As String,
                       ByVal niwea As String,
                       Optional ByVal messageText As String = "",
                       <System.Runtime.CompilerServices.CallerMemberName> Optional callerMemberName As String = Nothing)

        Dim logWrite As New CS0011LOGWrite With {
            .INFSUBCLASS = Me.GetType.Name,
            .INFPOSI = callerMemberName,
            .NIWEA = niwea,
            .TEXT = messageText,
            .MESSAGENO = messageNo
        }
        logWrite.CS0011LOGWrite()
    End Sub

End Class
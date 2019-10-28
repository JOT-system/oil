Option Explicit On

Imports System.Data.SqlClient
Imports System.Text

''' <summary>
'''  FTPファイル情報クラス
''' </summary>
''' <remarks></remarks>
Public Class CS0054FtpFiles

    ''' <summary>
    ''' FTP_TYPE（DB設定値）
    ''' </summary>
    Public Class FTP_TYPE
        Public Const [GET] As String = "GET"
        Public Const PUT As String = "PUT"
        Public Const MGET As String = "MGET"
        Public Const MPUT As String = "MPUT"
    End Class
    ''' <summary>
    ''' FTP_MODE（DB設定値）
    ''' </summary>
    Public Class FTP_MODE
        Public Const ASCII As String = "A"
        Public Const BINARY As String = "B"
    End Class
    ''' <summary>
    ''' FILE_MODE（DB設定値）
    ''' </summary>
    Public Class FILE_MODE
        Public Const OVERRIDE As String = "W"
    End Class

    ''' <summary>
    ''' FTPファイル情報(S0028_FTPFILES)
    ''' </summary>
    Public Class FTP_FILE
        Public TargetID As String                   'FTP対象ID
        Public Seq As Integer                       'SEQ
        Public FtpType As String                    '送受信タイプ PUT / GET
        Public ServerID As String                   'FTPサーバID
        Public FileName As String                   'FTPファイル名
        Public FileExt As String                    'FTPファイル拡張子
        Public FileDateFormat As String             'FTPファイル名日付書式
        Public FileDir As String                    'FTPディレクトリ
        Public LocalFileName As String              'ローカルファイル名
        Public LocalFileDir As String               'ローカルディレクトリ
        Public LocalFileExt As String               'ローカルファイル拡張子
        Public LocalFileDateFormat As String        'ローカルファイル名日付書式
        Public FtpMode As String                    '転送モード A(ASCII) or B(Binary)
        Public FileMode As String                   'ファイル書込モード ファイル存在時挙動　W(上書き)
    End Class

    ''' <summary>
    ''' FTPファイル管理
    ''' </summary>
    Private _ftpFiles As List(Of FTP_FILE)
    Public ReadOnly Property FTPFILES() As List(Of FTP_FILE)
        Get
            Return _ftpFiles
        End Get
    End Property
    Private _targetId As String
    ''' <summary>
    ''' FTPターゲットID
    ''' </summary>
    ''' <returns>TARGETID</returns>
    Public ReadOnly Property TARGET() As String
        Get
            Return _targetId
        End Get
    End Property

    ''' <summary>
    ''' ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR() As String
    ''' <summary>
    ''' DB接続文字列
    ''' </summary>
    ''' <value></value>
    Public Property DBCon As String


    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="I_targetId" >FTPターゲットID</param>
    ''' <remarks></remarks> 
    Public Sub New(ByVal I_targetId As String, Optional ByRef I_DBCon As String = "")
        'プロパティ初期化
        Initialize()

        _targetId = I_targetId
        If Not String.IsNullOrEmpty(I_DBCon) Then
            Me.DBCon = I_DBCon
        End If

        ' FTPファイル情報取得
        _ftpFiles = GetFtpFileInfo()
        If (IsNothing(_ftpFiles)) Then
            Me.ERR = C_MESSAGE_NO.DB_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, "S0028_FTPFILES TARGETID NotFound")
        End If

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        Me.ERR = C_MESSAGE_NO.NORMAL
        Me._ftpFiles = Nothing
        Me.DBCon = String.Empty

    End Sub

    ''' <summary>
    ''' FTPファイル情報取得
    ''' </summary>
    ''' <remarks></remarks> 
    Private Function GetFtpFileInfo() As List(Of FTP_FILE)
        'セッション管理
        Dim sm As New CS0050SESSION
        Dim ftpfm = Nothing

        Try
            'DataBase接続文字
            Using SQLcon = If(String.IsNullOrEmpty(DBCon), sm.getConnection, New SqlConnection(DBCon))
                SQLcon.Open() 'DataBase接続(Open)

                Dim sb As New StringBuilder
                sb.Append("Select ")
                sb.Append("  f.SEQ ")
                sb.Append("  , isnull(f.FTP_TYPE, '') as FTP_TYPE ")
                sb.Append("  , isnull(f.SERVERID, '') as SERVERID ")
                sb.Append("  , isnull(f.FILENAME, '') as FILENAME ")
                sb.Append("  , isnull(f.FILEEXT, '') as FILEEXT ")
                sb.Append("  , isnull(f.FILEDATEFORMAT, '') as FILEDATEFORMAT ")
                sb.Append("  , isnull(f.FILEDIR, '') as FILEDIR ")
                sb.Append("  , isnull(f.LOCAL_FILENAME, '') as LOCAL_FILENAME ")
                sb.Append("  , isnull(f.LOCAL_FILEEXT, '') as LOCAL_FILEEXT ")
                sb.Append("  , isnull(f.LOCAL_FILEDATEFORMAT, '') as LOCAL_FILEDATEFORMAT ")
                sb.Append("  , isnull(f.LOCAL_FILEDIR, '') as LOCAL_FILEDIR ")
                sb.Append("  , isnull(f.FTP_MODE, '') as FTP_MODE ")
                sb.Append("  , isnull(f.FILE_MODE, '') as FILE_MODE ")
                sb.Append("FROM ")
                sb.Append("  S0028_FTPFILES f ")
                sb.Append("WHERE ")
                sb.Append("  f.TARGETID = @targetId ")
                sb.Append("  AND f.STYMD <= @stymd ")
                sb.Append("  AND f.ENDYMD >= @endymd ")
                sb.Append("  AND f.DELFLG <> @delflg ")
                sb.Append("ORDER BY ")
                sb.Append("  SEQ ")

                Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                    Dim p_targetId As SqlParameter = SQLcmd.Parameters.Add("@targetId", System.Data.SqlDbType.NVarChar)
                    Dim p_stymd As SqlParameter = SQLcmd.Parameters.Add("@stymd", System.Data.SqlDbType.Date)
                    Dim p_endymd As SqlParameter = SQLcmd.Parameters.Add("@endymd", System.Data.SqlDbType.Date)
                    Dim p_delFlg As SqlParameter = SQLcmd.Parameters.Add("@delflg", System.Data.SqlDbType.NVarChar)
                    p_targetId.Value = _targetId
                    p_stymd.Value = Date.Now
                    p_endymd.Value = Date.Now
                    p_delFlg.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.HasRows Then
                            ftpfm = New List(Of FTP_FILE)
                            While SQLdr.Read
                                Dim fileInfo As New FTP_FILE With {
                                    .TargetID = _targetId,
                                    .Seq = SQLdr("SEQ"),
                                    .ServerID = SQLdr("SERVERID"),
                                    .FtpType = SQLdr("FTP_TYPE"),
                                    .FileName = SQLdr("FILENAME"),
                                    .FileDateFormat = SQLdr("FILEDATEFORMAT"),
                                    .FileExt = SQLdr("FILEEXT"),
                                    .FileDir = SQLdr("FILEDIR"),
                                    .LocalFileName = SQLdr("LOCAL_FILENAME"),
                                    .LocalFileExt = SQLdr("LOCAL_FILEEXT"),
                                    .LocalFileDateFormat = SQLdr("LOCAL_FILEDATEFORMAT"),
                                    .LocalFileDir = SQLdr("LOCAL_FILEDIR"),
                                    .FtpMode = SQLdr("FTP_MODE"),
                                    .FileMode = SQLdr("FILE_MODE")
                                }
                                ftpfm.Add(fileInfo)
                            End While
                        End If
                    End Using
                End Using
            End Using

        Catch ex As Exception
            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

        End Try

        Return ftpfm

    End Function

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
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports BASEDLL

''' <summary>
'''  FTPコントロールクラス
''' </summary>
''' <remarks></remarks>
Public Class FtpControl

    ''' <summary>
    ''' 送受信結果リスト
    ''' </summary>
    Public Class FTP_RESULT
        ''' <summary>
        ''' FTP対象ID
        ''' </summary>
        Public TargetID As String
        ''' <summary>
        ''' ローカルファイル
        ''' </summary>
        Public LocalFile As FileInfo
        ''' <summary>
        ''' サーバーファイル
        ''' </summary>
        Public ServerFile As String = ""
        ''' <summary>
        ''' サーバーファイル（FullPath）
        ''' </summary>
        ReadOnly Property ServerFullName As String
            Get
                Return ServerFile.ToString
            End Get
        End Property
        ''' <summary>
        ''' サーバーファイル（ファイル名）
        ''' </summary>
        ReadOnly Property ServerFileName As String
            Get
                Return ServerFile.Substring(ServerFile.LastIndexOf("/") + 1)
            End Get
        End Property
        ''' <summary>
        ''' サーバーファイル（拡張子）
        ''' </summary>

        ReadOnly Property ServerFileExtension As String
            Get
                Return ServerFileName.Substring(ServerFileName.LastIndexOf("."))
            End Get
        End Property
        ''' <summary>
        ''' サーバーファイル（拡張子なしファイル名）
        ''' </summary>

        ReadOnly Property ServerFileNameOnly As String
            Get
                Return ServerFileName.Replace(ServerFileExtension, "")
            End Get
        End Property

        ''' <summary>
        ''' 送受信結果ステータス
        ''' </summary>
        Public Status As Integer

        ''' <summary>
        ''' 送受信結果ステータス 送受信正常
        ''' </summary>
        Public Const OK As Integer = 0
        ''' <summary>
        ''' 送受信結果ステータス ファイル未存在
        ''' </summary>
        Public Const NOTFOUD As Integer = 1
        ''' <summary>
        ''' 送受信結果ステータス 送受信エラー
        ''' </summary>
        Public Const ERR As Integer = -1

    End Class

    ''' <summary>
    ''' [IN]対象ID
    ''' </summary>
    ''' <returns>[IN]TARGETID</returns>
    Public Property TargetID As String
    ''' <summary>
    ''' [IN]対象部署コード
    ''' </summary>
    ''' <returns>[IN]ORGCODE</returns>
    Public Property OrgCode As String
    ''' <summary>
    ''' [IN]参照処理のみ
    ''' </summary>
    ''' <returns>[IN]RenameFlag</returns>
    Public Property IsReadOnly As Boolean

    ''' <summary>
    ''' [OUT]ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR As String

    ''' <summary>
    ''' [OUT]RESULTプロパティ
    ''' </summary>
    ''' <returns>[OUT]RESULT</returns>
    Public Property Result As New List(Of FTP_RESULT)

    ''' <summary>
    ''' FTPクライアント管理
    ''' </summary>
    Private _dicFtpClient As New Dictionary(Of String, CS0053FtpClient)


    ''' <summary>
    ''' 日時
    ''' </summary>
    Private _tm As DateTime

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub New()

        'プロパティ初期化
        Initialize()

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        TargetID = String.Empty
        OrgCode = String.Empty
        IsReadOnly = False
        ERR = C_MESSAGE_NO.NORMAL

        Close()

    End Sub

    ''' <summary>
    ''' クローズ処理
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Close()

        Result.Clear()

        For Each client In _dicFtpClient.Values
            client.Close()
            client = Nothing
        Next
        _dicFtpClient.Clear()

    End Sub

    ''' <summary>
    ''' FTPリクエスト実行
    ''' </summary>
    ''' <param name="I_TargetId" >FTP対象ID</param>
    ''' <param name="I_OrgCode" >部署コード</param>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Public Function Request(Optional ByVal I_TargetId As String = "",
                            Optional ByVal I_OrgCode As String = "") As Boolean

        Me.ERR = C_MESSAGE_NO.NORMAL

        ' FTP結果リスト
        Me.Result.Clear()

        Me._tm = DateTime.Now

        ' パラメータ省略時はプロパティから
        Dim wkTargetId As String
        If String.IsNullOrEmpty(I_TargetId) Then
            wkTargetId = Me.TargetID
        Else
            wkTargetId = I_TargetId
        End If

        Dim wkOrgCode As String
        If String.IsNullOrEmpty(I_OrgCode) Then
            wkOrgCode = Me.OrgCode
        Else
            wkOrgCode = I_OrgCode
        End If

        Dim sm = New CS0050SESSION
        Dim ftpFiles As New CS0054FtpFiles(wkTargetId, sm.DBCon)

        Try
            '必須設定チェック
            If String.IsNullOrEmpty(wkTargetId) Then
                Throw New Exception("パラメータエラー TARGETID")
            End If

            For Each serverID In ftpFiles.FTPFILES.Select(Function(x) x.ServerID)
                ' FTPクライアント作成
                If Not _dicFtpClient.ContainsKey(serverID) Then
                    _dicFtpClient.Add(serverID, New CS0053FtpClient(serverID, sm.DBCon))
                End If
            Next

            For Each ftpfile In ftpFiles.FTPFILES

                Select Case ftpfile.FtpType
                    Case CS0054FtpFiles.FTP_TYPE.GET
                        'GET実行
                        FtpGet(_dicFtpClient(ftpfile.ServerID), ftpfile, wkOrgCode)
                    Case CS0054FtpFiles.FTP_TYPE.PUT
                        'PUT実行
                        FtpPut(_dicFtpClient(ftpfile.ServerID), ftpfile, wkOrgCode)
                    Case CS0054FtpFiles.FTP_TYPE.MGET
                        'MGET実行
                        FtpMGet(_dicFtpClient(ftpfile.ServerID), ftpfile, wkOrgCode)
                    Case CS0054FtpFiles.FTP_TYPE.MPUT
                        'MPUT実行
                        FtpMPut(_dicFtpClient(ftpfile.ServerID), ftpfile, wkOrgCode)
                End Select
            Next

            Return True

        Catch ex As Exception

            Me.ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            PutLog(Me.ERR, C_MESSAGE_TYPE.ABORT, ex.Message)

            Return False

        Finally

        End Try

    End Function

    ''' <summary>
    ''' FTPリクエスト GET実行
    ''' </summary>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Private Function FtpGet(ByRef ftpClient As CS0053FtpClient,
                            ByVal ftpFile As CS0054FtpFiles.FTP_FILE,
                            Optional ByVal orgCode As String = "") As Boolean

        ' サーバ側のファイル格納ディレクトリ・ファイル名設定
        Dim sbServerDir = New StringBuilder
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbServerDir.AppendFormat("/{0}", orgCode)
        End If
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbServerDir.AppendFormat("/{0}", ftpFile.FileDir)
        End If

        ' ローカル側のファイル格納ディレクトリ・ファイル名設定
        Dim sbLocalDir = New StringBuilder
        If Not String.IsNullOrEmpty(ftpFile.LocalFileDir) Then
            sbLocalDir.AppendFormat("{0}", ftpFile.LocalFileDir)
        End If
        Dim localDir = New DirectoryInfo(sbLocalDir.ToString)
        If Not localDir.Exists() Then
            localDir.Create()
        End If
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbLocalDir.AppendFormat("\{0}", orgCode)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If
        'サーバディレクトリ名を追加
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbLocalDir.AppendFormat("\{0}", ftpFile.FileDir)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If

        'サーバファイル名
        Dim sbFileName = New StringBuilder
        sbFileName.Append(ftpFile.FileName)
        If Not String.IsNullOrEmpty(ftpFile.FileExt) Then
            sbFileName.AppendFormat(".{0}", ftpFile.FileExt)
        End If

        Dim sbServerFullName = New StringBuilder()
        sbServerFullName.AppendFormat("{0}/{1}", sbServerDir, sbFileName)

        '//// その他設定 /////////////////

        ' 転送データモード
        Dim typeBinary As Boolean
        If ftpFile.FtpMode = CS0054FtpFiles.FTP_MODE.BINARY Then
            typeBinary = True
        Else
            typeBinary = False
        End If
        ' ファイル書込みモード
        Dim fileMode As Boolean
        If ftpFile.FileMode = CS0054FtpFiles.FILE_MODE.OVERRIDE Then
            fileMode = True
        Else
            fileMode = False
        End If

        Dim ftpResult = New FTP_RESULT With {
            .TargetID = ftpFile.TargetID,
            .ServerFile = sbServerFullName.ToString
        }

        ' ファイル一覧取得
        Dim ftpDir As CS0053FtpClient.FtpDirectory = ftpClient.ListDirectoryDetail(sbServerDir.ToString)
        If Not isNormal(ftpClient.ERR) Then
            ERR = C_MESSAGE_NO.FTP_FILE_GET_ERROR
            Return False
        End If
        ' ファイル一覧から対象ファイル抽出
        ' 条件：①ファイル（ディレクトリ除外）
        '     ：②ファイル名＝S0028.FILENAME＋S0028.FILENAMEの拡張子
        Dim file = ftpDir.FirstOrDefault(Function(x) x.FileType = CS0053FtpClient.FtpFileInfo.DirectoryEntryTypes.File AndAlso
                                                     x.Filename = sbFileName.ToString)
        If Not IsNothing(file) Then
            'ローカルファイル名
            Dim sbLocalFileName = New StringBuilder
            If Not String.IsNullOrEmpty(ftpFile.LocalFileName) Then
                sbLocalFileName.AppendFormat("{0}", ftpFile.LocalFileName)
            Else
                sbLocalFileName.AppendFormat("{0}", file.NameOnly)
            End If

            If Not String.IsNullOrEmpty(ftpFile.LocalFileExt) Then
                sbLocalFileName.AppendFormat(".{0}", ftpFile.LocalFileExt)
            Else
                sbLocalFileName.AppendFormat(".{0}", file.Extension)
            End If

            Dim sbLocalFile = New StringBuilder(sbLocalDir.ToString)
            sbLocalFile.AppendFormat("\{0}", sbLocalFileName)
            ftpResult.LocalFile = New FileInfo(sbLocalFile.ToString)

            Dim rtn = ftpClient.Download(ftpResult.ServerFile, ftpResult.LocalFile, fileMode, typeBinary)
            If rtn = True Then
                If Not IsReadOnly Then
                    ' ダウンロードファイルをリネーム
                    ftpClient.Rename(file.FullName, file.FullName & ".used")
                End If
                ftpResult.Status = FTP_RESULT.OK
            Else
                ftpResult.Status = FTP_RESULT.ERR
            End If

            Me.Result.Add(ftpResult)
        End If

        Return True

    End Function

    ''' <summary>
    ''' FTPリクエスト PUT実行
    ''' </summary>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Private Function FtpPut(ByRef ftpClient As CS0053FtpClient,
                            ByVal ftpFile As CS0054FtpFiles.FTP_FILE,
                            Optional ByVal orgCode As String = "") As Boolean

        '//// サーバ側設定 /////////////////

        ' サーバ側のファイル格納ディレクトリ・ファイル名設定
        Dim sbServerDir = New StringBuilder
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbServerDir.AppendFormat("/{0}", orgCode)
        End If
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbServerDir.AppendFormat("/{0}", ftpFile.FileDir)
        End If

        'サーバファイル名
        Dim sbFileName = New StringBuilder
        sbFileName.Append(ftpFile.FileName)
        If Not String.IsNullOrEmpty(ftpFile.FileDateFormat) Then
            'FTP送信時、日時を追加
            sbFileName.AppendFormat("_{0}", _tm.ToString(ftpFile.FileDateFormat))
        End If
        If Not String.IsNullOrEmpty(ftpFile.FileExt) Then
            sbFileName.AppendFormat(".{0}", ftpFile.FileExt)
        End If

        Dim sbServerFullName = New StringBuilder()
        sbServerFullName.AppendFormat("{0}/{1}", sbServerDir, sbFileName)

        '//// ローカル側設定 /////////////////

        ' ローカル側のファイル格納ディレクトリ・ファイル名設定
        Dim sbLocalDir = New StringBuilder
        If Not String.IsNullOrEmpty(ftpFile.LocalFileDir) Then
            sbLocalDir.AppendFormat("{0}", ftpFile.LocalFileDir)
        End If
        Dim localDir = New DirectoryInfo(sbLocalDir.ToString)
        If Not localDir.Exists() Then
            Throw New Exception("local directory missing  " & sbLocalDir.ToString)
        End If
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbLocalDir.AppendFormat("\{0}", orgCode)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If

        'ローカルファイル名
        Dim sbLocalFileName = New StringBuilder
        sbLocalFileName.Append(ftpFile.LocalFileName)
        If Not String.IsNullOrEmpty(ftpFile.LocalFileExt) Then
            sbLocalFileName.AppendFormat(".{0}", ftpFile.LocalFileExt)
        End If

        Dim sbLocalFile = New StringBuilder(sbLocalDir.ToString)
        sbLocalFile.AppendFormat("\{0}", sbLocalFileName)


        '//// その他設定 /////////////////

        ' 転送データモード
        Dim typeBinary As Boolean
        If ftpFile.FtpMode = CS0054FtpFiles.FTP_MODE.BINARY Then
            typeBinary = True
        Else
            typeBinary = False
        End If

        Dim ftpResult = New FTP_RESULT With {
            .TargetID = ftpFile.TargetID,
            .ServerFile = sbServerFullName.ToString,
            .LocalFile = New FileInfo(sbLocalFile.ToString)
        }
        Dim rtn = ftpClient.Upload(sbLocalFile.ToString, sbServerFullName.ToString, typeBinary)
        If rtn = True Then
            ftpResult.Status = FTP_RESULT.OK
        Else
            ftpResult.Status = FTP_RESULT.ERR
        End If

        Me.Result.Add(ftpResult)

        Return True

    End Function

    ''' <summary>
    ''' FTPリクエスト MGET実行
    ''' </summary>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Private Function FtpMGet(ByRef ftpClient As CS0053FtpClient,
                            ByVal ftpFile As CS0054FtpFiles.FTP_FILE,
                            Optional ByVal orgCode As String = "") As Boolean

        ' サーバ側のファイル格納ディレクトリ・ファイル名設定
        Dim sbServerDir = New StringBuilder
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbServerDir.AppendFormat("/{0}", orgCode)
        End If
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbServerDir.AppendFormat("/{0}", ftpFile.FileDir)
        End If

        ' ローカル側のファイル格納ディレクトリ・ファイル名設定
        Dim sbLocalDir = New StringBuilder
        If Not String.IsNullOrEmpty(ftpFile.LocalFileDir) Then
            sbLocalDir.AppendFormat("{0}", ftpFile.LocalFileDir)
        End If
        Dim localDir = New DirectoryInfo(sbLocalDir.ToString)
        If Not localDir.Exists() Then
            localDir.Create()
        End If
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbLocalDir.AppendFormat("\{0}", orgCode)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If
        'サーバディレクトリ名を追加
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbLocalDir.AppendFormat("\{0}", ftpFile.FileDir)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If

        '//// その他設定 /////////////////

        ' 転送データモード
        Dim typeBinary As Boolean
        If ftpFile.FtpMode = CS0054FtpFiles.FTP_MODE.BINARY Then
            typeBinary = True
        Else
            typeBinary = False
        End If
        ' ファイル書込みモード
        Dim fileMode As Boolean
        If ftpFile.FileMode = CS0054FtpFiles.FILE_MODE.OVERRIDE Then
            fileMode = True
        Else
            fileMode = False
        End If

        ' ファイル一覧取得
        Dim ftpDir As CS0053FtpClient.FtpDirectory = ftpClient.ListDirectoryDetail(sbServerDir.ToString)
        If Not isNormal(ftpClient.ERR) Then
            ERR = C_MESSAGE_NO.FTP_FILE_GET_ERROR
            Return False
        End If

        ' ファイル一覧から対象ファイル抽出
        ' 条件：①ファイル（ディレクトリ除外）
        '     ：②ファイル名＝S0028.FILENAMEに一致
        '     ：③ファイル拡張子＝S0028.FILENAMEの拡張子
        Dim wkFiles = ftpDir.
                            Where(Function(x) x.FileType = CS0053FtpClient.FtpFileInfo.DirectoryEntryTypes.File AndAlso
                                                Regex.IsMatch(x.NameOnly, ftpFile.FileName) AndAlso
                                                x.Extension = ftpFile.FileExt)
        For Each file In wkFiles
            'ローカルファイル名
            Dim sbLocalFile As StringBuilder = New StringBuilder(sbLocalDir.ToString)
            sbLocalFile.AppendFormat("\{0}.{1}", file.NameOnly, file.Extension)

            Dim ftpResult = New FTP_RESULT With {
                .TargetID = ftpFile.TargetID,
                .ServerFile = file.FullName,
                .LocalFile = New FileInfo(sbLocalFile.ToString)
            }
            Me.Result.Add(ftpResult)

            Dim rtn = ftpClient.Download(ftpResult.ServerFile, ftpResult.LocalFile, fileMode, typeBinary)
            If rtn = True Then
                If Not IsReadOnly Then
                    ' ダウンロードファイルをリネーム
                    ftpClient.Rename(file.FullName, file.FullName & ".used")
                End If
                ftpResult.Status = FTP_RESULT.OK
            Else
                ftpResult.Status = FTP_RESULT.ERR
            End If

        Next

        Return True

    End Function

    ''' <summary>
    ''' FTPリクエスト MGET実行
    ''' </summary>
    ''' <returns>TRUE|FALSE</returns>
    ''' <remarks></remarks> 
    Private Function FtpMPut(ByRef ftpClient As CS0053FtpClient,
                            ByVal ftpFile As CS0054FtpFiles.FTP_FILE,
                            Optional ByVal orgCode As String = "") As Boolean

        ' サーバ側のファイル格納ディレクトリ名設定
        Dim sbServerDir = New StringBuilder
        '部署コード指定時はパス先頭追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbServerDir.AppendFormat("/{0}", orgCode)
        End If
        If Not String.IsNullOrEmpty(ftpFile.FileDir) Then
            sbServerDir.AppendFormat("/{0}", ftpFile.FileDir)
        End If

        ' ローカル側のファイル格納ディレクトリ名設定
        Dim sbLocalDir = New StringBuilder
        If Not String.IsNullOrEmpty(ftpFile.LocalFileDir) Then
            sbLocalDir.AppendFormat("{0}", ftpFile.LocalFileDir)
        End If
        Dim localDir = New DirectoryInfo(sbLocalDir.ToString)
        If Not localDir.Exists() Then
            localDir.Create()
        End If
        '部署コード指定時はパス追加
        If Not String.IsNullOrEmpty(orgCode) Then
            sbLocalDir.AppendFormat("\{0}", orgCode)
            localDir = New DirectoryInfo(sbLocalDir.ToString)
            If Not localDir.Exists Then
                localDir.Create()
            End If
        End If
        '//// その他設定 /////////////////

        ' 転送データモード
        Dim typeBinary As Boolean
        If ftpFile.FtpMode = CS0054FtpFiles.FTP_MODE.BINARY Then
            typeBinary = True
        Else
            typeBinary = False
        End If
        ' ファイル書込みモード
        Dim fileMode As Boolean
        If ftpFile.FileMode = CS0054FtpFiles.FILE_MODE.OVERRIDE Then
            fileMode = True
        Else
            fileMode = False
        End If

        ' ファイル一覧取得
        ' ファイル一覧から対象ファイル抽出
        ' 条件：①ファイル名＝S0028.LOCALFILENAMEに一致
        '     ：②ファイル拡張子＝S0028.LOCALFILENAMEの拡張子
        Dim wkFiles = localDir.GetFiles(ftpFile.LocalFileName & "." & ftpFile.LocalFileExt)
        For Each file In wkFiles
            'サーバファイル名
            Dim sbFileName = New StringBuilder
            sbFileName.Append(file.Name.Replace(file.Extension, ""))
            If Not String.IsNullOrEmpty(ftpFile.FileDateFormat) Then
                'FTP送信時、日時を追加
                sbFileName.AppendFormat("_{0}", _tm.ToString(ftpFile.FileDateFormat))
            End If
            If Not String.IsNullOrEmpty(ftpFile.FileExt) Then
                sbFileName.AppendFormat(".{0}", ftpFile.FileExt)
            Else
                sbFileName.AppendFormat("{0}", file.Extension)
            End If

            Dim sbServerFullName = New StringBuilder()
            sbServerFullName.AppendFormat("{0}/{1}", sbServerDir, sbFileName)

            Dim ftpResult = New FTP_RESULT With {
                .TargetID = ftpFile.TargetID,
                .ServerFile = sbServerFullName.ToString,
                .LocalFile = file
            }
            Me.Result.Add(ftpResult)
            Dim rtn = ftpClient.Upload(file, sbServerFullName.ToString, typeBinary)
            If rtn = True Then
                ftpResult.Status = FTP_RESULT.OK
            Else
                ftpResult.Status = FTP_RESULT.ERR
            End If

        Next

        Return True

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

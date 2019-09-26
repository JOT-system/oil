Public Class GRCO0106WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPID As String = "CO0106"                          'MAPID(実行)
    '各種起動ファイルの所在
    Public Const C_PGM_FILE_PATH As String = "\APPLBIN\BATCH\CB00013LIBSEND\CB00013LIBSEND.exe"
    Public Const C_VERSION_FILE_PATH As String = "\APPLBIN\SYSLIB\APPLBAT\CMD\Version.txt"
    Public Const C_BAT_SEND_FILE_PATH As String = "\APPLBIN\SYSLIB\APPLBAT\CMD\libsend.bat"
    Public Const C_WORK_CMD_PATH As String = "\APPLBIN\SYSLIB\APPLBAT\CMD"
    Public Const C_SERVICE_NAME_CB0011 As String = "CB0011JobControl"
    '状態メッセージ一覧
    Public Class C_STATUS
        Public Const OK As String = "OK"
        Public Const NG As String = "NG"
        Public Const STOPPING As String = "停止中"
        Public Const RUNNING As String = "稼働中"
    End Class
    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub initialize()

    End Sub

End Class
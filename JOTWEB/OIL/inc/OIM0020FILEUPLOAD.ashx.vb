Option Strict On
Imports System.IO
Imports System.Web
Imports System.Web.Services

Public Class OIM0020FILEUPLOAD
    Implements IHttpHandler, IRequiresSessionState

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        '共通関数宣言(BASEDLL)
        Dim CS0006TERMchk As New CS0006TERMchk              'コンピュータ名存在チェック
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'オンライン状態取得
        Dim CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

        '★★★ セッション情報（ユーザ）未設定時の処理　★★★ 
        If CS0050SESSION.USERID = "" Then
            'エラーリターン(textStatus:errorとなる)
            context.Response.StatusCode = 300
            Return
        End If

        '★★★ オンラインサービス判定  ★★★
        '○画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) Then
            If CS0008ONLINEstat.ONLINESW = 0 Then
                'エラーリターン(textStatus:errorとなる)
                context.Response.StatusCode = 300
                Return
            End If
        Else
            'エラーリターン(textStatus:errorとなる)
            context.Response.StatusCode = 300
            Return
        End If

        '■アップロードFILE格納ディレクトリ取得
        Try
            '　アップロードFILE格納フォルダ作成
            Dim WW_Dir As String = ""
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP"
            '　格納フォルダ存在確認＆作成(...\UPLOAD_TMP)
            If Not Directory.Exists(WW_Dir) Then
                Directory.CreateDirectory(WW_Dir)
            End If

            '　アップロードFILE格納フォルダ存在確認＆作成(...\UPLOAD_TMP\ユーザーID)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID
            If Not Directory.Exists(WW_Dir) Then
                Directory.CreateDirectory(WW_Dir)
            End If

            '　アップロードFILE格納フォルダ内不要ファイル削除(すべて削除)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID
            For Each tempFile As String In Directory.GetFiles(WW_Dir, "*.*")
                ' ファイルパスからファイル名を取得
                File.Delete(tempFile)
            Next
        Catch ex As Exception
            'エラーリターン(textStatus:errorとなる)
            context.Response.StatusCode = 300
            Exit Sub
        End Try

        '■アップロードFILE格納
        Try
            For i As Integer = 0 To context.Request.Files.Count - 1
                'ファイル名称切り出し
                Dim WW_FileName As String = context.Request.Files(i).FileName
                WW_FileName = Mid(WW_FileName, InStrRev(WW_FileName, "\") + 1, Len(WW_FileName))

                Dim WW_PostedFile As HttpPostedFile = context.Request.Files(i)
                WW_PostedFile.SaveAs(CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID & "\" & WW_FileName)
            Next
        Catch ex As Exception
            'エラーリターン(textStatus:errorとなる)
            context.Response.StatusCode = 300
        End Try
    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
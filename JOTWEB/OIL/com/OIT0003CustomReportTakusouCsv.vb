Option Strict On
''' <summary>
''' 託送指示書(CSV)作成クラス
''' </summary>
Public Class OIT0003CustomReportTakusouCsv : Implements IDisposable
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private uploadRootPath As String = ""
    Private uploadTmpFileName As String = ""
    Private uploadTmpFilePath As String = ""
    Private uploadFilePath As String = ""
    Private urlRoot As String = ""
    Private csvData As DataTable
    Private csvSW As IO.StreamWriter
    Private xlProcId As Integer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="csvDataClass"></param>
    Public Sub New(csvDataClass As DataTable)

        Dim CS0050SESSION As New CS0050SESSION
        Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")

        Me.uploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
        Me.csvData = csvDataClass
        Me.uploadTmpFileName = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".csv"
        Me.uploadTmpFilePath = IO.Path.Combine(Me.uploadRootPath, Me.uploadTmpFileName)

        'ディレクトリが存在しない場合は生成
        If IO.Directory.Exists(Me.uploadRootPath) = False Then
            IO.Directory.CreateDirectory(Me.uploadRootPath)
        End If
        '前日プリフィックスのアップロードファイルが残っていた場合は削除
        Dim targetFiles = IO.Directory.GetFiles(Me.uploadRootPath, "*.*")
        Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
        For Each targetFile In targetFiles
            Dim fileNameFol As String = IO.Path.GetFileName(targetFile)
            '今日の日付が先頭のファイル名の場合は残す
            If fileNameFol.StartsWith(keepFilePrefix) Then
                Continue For
            End If
            Try
                IO.File.Delete(targetFile)
            Catch ex As Exception
                '削除時のエラーは無視
            End Try
        Next targetFile
        'URLのルートを表示
        Me.urlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        '書き込むファイルを開く
        Me.csvSW = New System.IO.StreamWriter(Me.uploadTmpFilePath, False, enc)
    End Sub

    ''' <summary>
    ''' 出力ファイル作成
    ''' </summary>
    ''' <param name="writeHeader"></param>
    ''' <param name="blnFrame"></param>
    ''' <param name="delm"></param>
    ''' <returns></returns>
    Public Function CreatePrintData(Optional ByVal writeHeader As Boolean = True,
                                    Optional ByVal blnFrame As Boolean = False,
                                    Optional ByVal delm As String = ",") As String

        Try
            'ヘッダ情報取得
            Dim fieldNames As List(Of String) = csvData.Columns.Cast(Of DataColumn).Select(Function(c) c.ColumnName).ToList()

            'ヘッダ書き込み
            If writeHeader Then
                If fieldNames IsNot Nothing AndAlso fieldNames.Any() Then
                    'ヘッダ退避
                    Dim writeDataList As List(Of String) = fieldNames
                    If blnFrame = True Then
                        'ダブルクォーテーションで括る
                        writeDataList.ForEach(Sub(x) x = String.Format("""{0}""", x))
                    End If
                    '書き込み
                    Me.csvSW.Write(String.Join(delm, writeDataList))
                    '改行
                    Me.csvSW.Write(vbCrLf)
                End If
            End If

            'レコードを書き込む
            Dim row As DataRow
            For Each row In Me.csvData.Rows
                Dim fieldDataList As List(Of String) = row.ItemArray().Select(Function(field) field.ToString()).ToList()
                If fieldDataList IsNot Nothing AndAlso fieldDataList.Any() Then
                    'データ退避
                    Dim writeDataList As List(Of String) = fieldDataList
                    If blnFrame = True Then
                        'ダブルクォーテーションで括る
                        writeDataList.ForEach(Sub(x) x = String.Format("""{0}""", x))
                    End If
                    '書き込み
                    Me.csvSW.Write(String.Join(delm, writeDataList))
                    '改行
                    Me.csvSW.Write(vbCrLf)
                End If
            Next

            '閉じる
            Me.csvSW.Close()

            '★指定フォルダが設定されている場合
            If Me.uploadFilePath <> "" Then
                '作成したファイルを指定フォルダに配置する。
                System.IO.File.Copy(Me.uploadTmpFilePath, Me.uploadFilePath)
            End If

            Return urlRoot & Me.uploadTmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: マネージド状態を破棄します (マネージド オブジェクト)。
            End If

            ' TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If
        disposedValue = True
    End Sub

    ' TODO: 上の Dispose(disposing As Boolean) にアンマネージド リソースを解放するコードが含まれる場合にのみ Finalize() をオーバーライドします。
    'Protected Overrides Sub Finalize()
    '    ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
        Dispose(True)
        ' TODO: 上の Finalize() がオーバーライドされている場合は、次の行のコメントを解除してください。
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

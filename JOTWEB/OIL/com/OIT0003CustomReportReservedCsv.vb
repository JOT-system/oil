﻿Option Strict On
''' <summary>
''' 出荷予約CSV出力クラス（テキストベース）
''' </summary>
Public Class OIT0003CustomReportReservedCsv : Implements System.IDisposable
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private UploadRootPath As String = ""
    Private UploadTmpFileName As String = ""
    Private UploadTmpFilePath As String = ""
    Private UploadFilePath As String = ""
    Private UrlRoot As String = ""
    Private CsvData As DataTable
    Private CsvSW As IO.StreamWriter
    Private xlProcId As Integer
    Public Property FileNameWithoutExtention As String = ""
    Public Property FileExtention As String = ""
    Public Property OutputDef As OIT0003OTLinkageList.FileLinkagePatternItem
    Public Sub New(csvDataClass As DataTable, outputDef As OIT0003OTLinkageList.FileLinkagePatternItem, Optional fileName As String = "", Optional fileExtention As String = "csv")
        Dim CS0050SESSION As New CS0050SESSION
        'CSVファイルに書き込むときに使うEncoding
        Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")

        Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
        Me.CsvData = csvDataClass
        Me.UploadTmpFileName = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".csv"
        If fileName <> "" Then
            Me.UploadTmpFileName = fileName & "." & fileExtention
        End If
        Me.UploadTmpFilePath = IO.Path.Combine(Me.UploadRootPath, Me.UploadTmpFileName)
        'If Not String.IsNullOrEmpty(I_FolderPath) Then
        '    Me.UploadFilePath = IO.Path.Combine(I_FolderPath, Me.UploadTmpFileName)
        'End If

        'ディレクトリが存在しない場合は生成
        If IO.Directory.Exists(Me.UploadRootPath) = False Then
            IO.Directory.CreateDirectory(Me.UploadRootPath)
        End If
        '前日プリフィックスのアップロードファイルが残っていた場合は削除
        Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
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
        Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        '書き込むファイルを開く
        'Dim sr As New System.IO.StreamWriter(Me.UploadTmpFilePath, False, enc)
        Me.CsvSW = New System.IO.StreamWriter(Me.UploadTmpFilePath, False, enc)
        Me.OutputDef = outputDef
    End Sub

    ''' <summary>
    ''' DataTableの内容をCSVファイルに保存する
    ''' </summary>
    ''' <param name="writeHeader">ヘッダを書き込む時はtrue。</param>
    ''' <param name="blnFrame">"(ダブルクオーテーション)で囲む時はtrue。</param>
    ''' <param name="delm">デリミタ（未指定時はカンマ）</param>
    Public Function ConvertDataTableToCsv(writeHeader As Boolean,
                                          Optional ByVal blnFrame As Boolean = False,
                                          Optional ByVal delm As String = ",") As String
        Dim colCount As Integer = Me.OutputDef.OutputFiledList.Count
        Dim lastColIndex As Integer = colCount - 1
        Dim i As Integer

        Try
            'ヘッダを書き込む
            If writeHeader Then
                For i = 0 To colCount - 1
                    'ヘッダの取得
                    Dim field As String = Me.OutputDef.OutputFiledList.Keys(i)
                    '"で囲む
                    If blnFrame = True Then
                        field = EncloseDoubleQuotesIfNeed(field)
                    End If
                    'フィールドを書き込む
                    Me.CsvSW.Write(field)
                    'カンマを書き込む
                    If lastColIndex > i Then
                        Me.CsvSW.Write(delm)
                    End If
                Next
                '改行する
                Me.CsvSW.Write(vbCrLf)
            End If
            If OutputDef.OutputReservedCustomOutputFiledHeader <> "" Then
                Me.CsvSW.Write(OutputDef.OutputReservedCustomOutputFiledHeader)
                Me.CsvSW.Write(vbCrLf)
            End If
            'レコードを書き込む
            Dim row As DataRow
            For Each row In Me.CsvData.Rows
                For i = 0 To colCount - 1
                    Dim fieldName As String = Me.OutputDef.OutputFiledList.Keys(i)
                    If Me.CsvData.Columns.Contains(fieldName) = False Then
                        Return ""
                    End If
                    'フィールドの取得
                    Dim field As String = Convert.ToString(row(fieldName))
                    If Me.OutputDef.OutputReservedConstantField Then
                        Dim fieldLength As Integer = Me.OutputDef.OutputFiledList.Values(i)
                        If Me.OutputDef.OutputFiledIsFront Then
                            '前埋め
                            field = Right(Space(fieldLength) & field, fieldLength)
                        Else
                            '後埋め
                            field = Left(field & Space(fieldLength), fieldLength)
                        End If
                    End If

                    '"で囲む
                    If blnFrame = True Then
                        field = EncloseDoubleQuotesIfNeed(field)
                    End If
                    'フィールドを書き込む
                    Me.CsvSW.Write(field)
                    'カンマを書き込む
                    If lastColIndex > i Then
                        Me.CsvSW.Write(delm)
                    End If
                Next
                '改行する
                Me.CsvSW.Write(vbCrLf)
            Next
            '閉じる
            Me.CsvSW.Close()

            '★指定フォルダが設定されている場合
            If Me.UploadFilePath <> "" Then
                '作成したファイルを指定フォルダに配置する。
                System.IO.File.Copy(Me.UploadTmpFilePath, Me.UploadFilePath)
            End If

            ''ストリーム生成
            'Using fs As New IO.FileStream(Me.UploadTmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            '    Dim binaryLength = Convert.ToInt32(fs.Length)
            '    ReDim retByte(binaryLength)
            '    fs.Read(retByte, 0, binaryLength)
            '    fs.Flush()
            'End Using
            Return UrlRoot & Me.UploadTmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally

        End Try

    End Function
    ''' <summary>
    ''' シーケンスファイル作成メソッド
    ''' </summary>
    ''' <param name="blnFrame"></param>
    ''' <param name="delm"></param>
    ''' <returns></returns>
    Public Function CreateSequence(Optional ByVal blnFrame As Boolean = False,
                                   Optional ByVal delm As String = "") As String
        Dim colCount As Integer = Me.OutputDef.OutputFiledList.Count
        Dim lastColIndex As Integer = colCount - 1
        Dim i As Integer

        Try
            'ヘッダを書き込む
            If OutputDef.OutputReservedCustomOutputFiledHeader <> "" Then
                Me.CsvSW.Write(OutputDef.OutputReservedCustomOutputFiledHeader)
                Me.CsvSW.Write(vbCrLf)
            End If
            'レコードを書き込む
            Dim row As DataRow
            For Each row In Me.CsvData.Rows
                For i = 0 To colCount - 1

                    Dim fieldName As String = Me.OutputDef.OutputFiledList.Keys(i)
                    Dim fieldLength As Integer = Me.OutputDef.OutputFiledList(fieldName)
                    If Me.CsvData.Columns.Contains(fieldName) = False Then
                        Return ""
                    End If
                    'フィールドの取得
                    Dim field As String = Convert.ToString(row(fieldName))
                    field = PaddingRightSpace(field, fieldLength)
                    '"で囲む
                    If blnFrame = True Then
                        field = EncloseDoubleQuotesIfNeed(field)
                    End If
                    'フィールドを書き込む
                    Me.CsvSW.Write(field)
                    'カンマを書き込む
                    If lastColIndex > i Then
                        Me.CsvSW.Write(delm)
                    End If
                Next
                '改行する
                'Me.CsvSW.Write(vbCrLf)
            Next
            '閉じる
            Me.CsvSW.Close()

            '★指定フォルダが設定されている場合
            If Me.UploadFilePath <> "" Then
                '作成したファイルを指定フォルダに配置する。
                System.IO.File.Copy(Me.UploadTmpFilePath, Me.UploadFilePath)
            End If

            ''ストリーム生成
            'Using fs As New IO.FileStream(Me.UploadTmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            '    Dim binaryLength = Convert.ToInt32(fs.Length)
            '    ReDim retByte(binaryLength)
            '    fs.Read(retByte, 0, binaryLength)
            '    fs.Flush()
            'End Using
            Return UrlRoot & Me.UploadTmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally

        End Try
    End Function
    ''' <summary>
    ''' シーケンスヘッダーファイル生成
    ''' </summary>
    ''' <returns></returns>
    Public Function CreateSequenceRequest(Optional requestFileName As String = "COSDE.SEQ") As String
        Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
        Dim filePath As String = IO.Path.Combine(Me.UploadRootPath, requestFileName)
        Using sw As New IO.StreamWriter(filePath, False, enc)
            'データ種別
            sw.Write(PaddingRightSpace("90", 2))
            '処理区分
            sw.Write(PaddingRightSpace("0", 1))
            '年月日
            sw.Write(PaddingRightSpace(Strings.StrDup(8, "0"), 8))
            '支店コード
            Dim branch As String = "06"
            If Me.OutputDef.OfficeCode = "012401" Then
                branch = "08"
            End If
            sw.Write(PaddingRightSpace(branch, 2))
            '予備
            sw.Write(PaddingRightSpace(Strings.StrDup(238, "0"), 238))
        End Using
        Return UrlRoot & requestFileName

    End Function

    ''' <summary>
    ''' 必要ならば、文字列をダブルクォートで囲む
    ''' </summary>
    Public Shared Function EncloseDoubleQuotesIfNeed(field As String) As String
        If NeedEncloseDoubleQuotes(field) Then
            Return EncloseDoubleQuotes(field)
        End If
        Return field
    End Function

    ''' <summary>
    ''' 文字列をダブルクォートで囲む
    ''' </summary>
    Public Shared Function EncloseDoubleQuotes(field As String) As String
        If field.IndexOf(""""c) > -1 Then
            '"を""とする
            field = field.Replace("""", """""")
        End If
        Return """" & field & """"
    End Function

    ''' <summary>
    ''' 文字列をダブルクォートで囲む必要があるか調べる
    ''' </summary>
    Public Shared Function NeedEncloseDoubleQuotes(field As String) As Boolean
        Return field.IndexOf(""""c) > -1 OrElse
            field.IndexOf(","c) > -1 OrElse
            field.IndexOf(ControlChars.Cr) > -1 OrElse
            field.IndexOf(ControlChars.Lf) > -1 OrElse
            field.StartsWith(" ") OrElse
            field.StartsWith(vbTab) OrElse
            field.EndsWith(" ") OrElse
            field.EndsWith(vbTab)
    End Function
    ''' <summary>
    ''' 全角混在、文字数オーバーした場合はカットする関数
    ''' </summary>
    ''' <param name="targetString">対象文字列</param>
    ''' <param name="length">設定バイト数</param>
    ''' <returns></returns>
    Private Function PaddingRightSpace(targetString As String, length As Integer) As String
        Dim enc As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
        '最大Length分の追加スペースを一旦追加
        Dim additionalSpace As String = Space(length)
        Dim editString As String = targetString & additionalSpace
        '入力文字列を切り取りバイト配列に格納
        Dim bArray() As Byte = DirectCast(Array.CreateInstance(GetType(Byte), length), Byte())
        Dim st1 As String = enc.GetString(bArray)
        Array.Copy(enc.GetBytes(editString), 0, bArray, 0, length)
        '切り取った結果をバイト配列に格納
        Dim resString As String = enc.GetString(bArray)
        '最後が全角の1バイト分の場合はLengthが1少ない為、１スペース分足す
        Dim resLength = enc.GetByteCount(resString)
        If length = resLength - 1 Then
            Return resString.Substring(0, resString.Length - 1) & " "
        Else
            Return resString
        End If
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
        If Me.CsvSW IsNot Nothing Then
            Try
                Me.CsvSW.Close()
                Me.CsvSW.Dispose()
                Me.CsvSW = Nothing
            Catch ex As Exception
                '強制クローズの為何もしない
            End Try
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


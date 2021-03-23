Option Strict On
Imports System.IO
Imports ClosedXML.Excel
''' <summary>
''' 託送指示書(Excel)作成クラス
''' </summary>
''' <remarks>CLOSEDXML版（PDFは未対応）</remarks>
Public Class OIT0003CustomReportTakusouExcel : Implements IDisposable
    Public Enum OutputFileType
        Excel = 0
        Pdf = 1
    End Enum
    ''' <summary>
    ''' 出力ファイルタイププロパティ(初期値PDF)
    ''' </summary>
    ''' <returns></returns>
    Public Property FileType As OutputFileType = OutputFileType.Pdf
    ''' <summary>
    ''' Excelのテンプレート用FS
    ''' </summary>
    Private FS As FileStream = Nothing
    Private ExcelBook As XLWorkbook
    Private ExcelWorkSheets As IXLWorksheets
    Private ExcelWorkSheet As IXLWorksheet
    Private ExcelTempSheet As IXLWorksheet
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="officeCode"></param>
    ''' <param name="printDataClass"></param>
    Public Sub New(officeCode As String, printDataClass As DataTable)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Dim templateParentFolderName As String = "OIT0003Takusou"
            Dim tempXlsFileName As String = "" '[営業所コード].xlsxとする
            tempXlsFileName = String.Format("{0}.xlsx", officeCode)

            Me.PrintData = printDataClass
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      templateParentFolderName, tempXlsFileName)
            Dim tempSealImageFileName As String = String.Format("{0}.png", officeCode)

            If IO.File.Exists(Me.ExcelTemplatePath) = False Then
                Throw New Exception(String.Format("テンプレートファイルが存在しません。{0}", Me.ExcelTemplatePath))
            End If

            Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
            'ディレクトリが存在しない場合は生成
            If IO.Directory.Exists(Me.UploadRootPath) = False Then
                IO.Directory.CreateDirectory(Me.UploadRootPath)
            End If
            '前日プリフィックスのアップロードファイルが残っていた場合は削除
            Dim targetFiles = IO.Directory.GetFiles(Me.UploadRootPath, "*.*")
            Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
            For Each targetFile In targetFiles
                Dim fileName As String = IO.Path.GetFileName(targetFile)
                '今日の日付が先頭のファイル名の場合は残す
                If fileName.StartsWith(keepFilePrefix) Then
                    Continue For
                End If
                Try
                    IO.File.Delete(targetFile)
                Catch ex As Exception
                    '削除時のエラーは無視
                End Try
            Next targetFile
            'URLのルートを表示
            Me.UrlRoot = String.Format("{0}://{1}/PRINT/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID)

            'Excelアプリケーションオブジェクトの生成(普通にファイルパスでXLWorkbookをnew出来るが読み取り専用で開けないので
            '                                        読み取り専用のFileStreamをかます)
            Me.FS = New FileStream(Me.ExcelTemplatePath, mode:=FileMode.Open, access:=FileAccess.Read, share:=FileShare.Read)
            Me.ExcelBook = New XLWorkbook(Me.FS)


            Me.ExcelWorkSheets = Me.ExcelBook.Worksheets

            Me.ExcelWorkSheet = Me.ExcelWorkSheets.Worksheet("運送状")
            Me.ExcelTempSheet = Me.ExcelWorkSheets.Worksheet("tempWork")

        Catch ex As Exception
            If Me.FS IsNot Nothing Then
                Me.FS.Close()
                Me.FS.Dispose()
            End If
            Throw
        End Try
    End Sub
    ''' <summary>
    ''' 帳票作成処理
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData() As String
        Dim outputExtention As String = "pdf"
        If Me.FileType = OutputFileType.Excel Then
            outputExtention = "xlsx"
        Else
            Throw New Exception("PDFには対応していません")
        End If

        Dim tmpFileName As String = String.Format("{0:yyyyMMddHHmmss}{1}.{2}", DateTime.Now, DateTime.Now.Millisecond.ToString, outputExtention)
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '◯ヘッダーの設定
            EditHeaderArea()
            '◯明細の設定
            EditDetailArea()
            ExcelTempSheet.Delete() '雛形シート削除

            '保存処理実行
            Me.ExcelBook.SaveAs(file:=tmpFilePath, options:=New SaveOptions() With {.EvaluateFormulasBeforeSaving = True})
            'テンプレートファイルクローズ
            Me.FS.Close()
            Me.FS = Nothing
            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Function
    ''' <summary>
    ''' 託送指示ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Dim firstDr As DataRow = Me.PrintData.Rows(0)
        Dim hkDate As String = Convert.ToString(firstDr("HKDATE"))
        Dim depStation As String = Convert.ToString(firstDr("DEPSTATIONNAME"))
        Dim rngHeaderArea As IXLRange
        Try
            '発駅名
            rngHeaderArea = Me.ExcelWorkSheet.Range("C3")
            rngHeaderArea.Value = depStation
            '発行日
            rngHeaderArea = Me.ExcelWorkSheet.Range("M2")
            rngHeaderArea.Value = hkDate
        Catch ex As Exception
            Throw
        End Try

    End Sub
    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea()
        Dim breakFields As New List(Of String)
        'ブレイク対象のフィールド名
        breakFields.AddRange({"AGREEMENTCODE", "JROILTYPE"})
        'ブレイク対象の値保持用
        Dim breakKey As New Hashtable
        For Each fieldName In breakFields
            breakKey(fieldName) = "%INIT%"
        Next fieldName

        Dim rngTmpVal As IXLRange = Nothing
        Try
            Dim startRow As Integer = 8
            Dim rowCounter As Integer = 0
            Dim tankCnt As Integer = 0
            Dim isBreak As Boolean = False
            Dim summaryCol1 As String = "G"
            Dim summaryCol1Format As String = "{0:#,##0}両"
            Dim summaryCol2 As String = "I"
            Dim currentRow As Integer = startRow + rowCounter
            Dim valuesArray(8) As Object
            For Each dr As DataRow In Me.PrintData.Rows

                If rowCounter > 0 Then
                    For Each fieldName In breakFields
                        If Convert.ToString(breakKey(fieldName)) <> Convert.ToString(dr(fieldName)) Then
                            isBreak = True
                            breakKey(fieldName) = Convert.ToString(dr(fieldName))
                        End If
                    Next fieldName


                    If isBreak Then
                        'ブレイクし合計行の追加
                        rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol1 & currentRow)
                        rngTmpVal.Value = String.Format(summaryCol1Format, tankCnt)

                        rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol2 & currentRow)
                        rngTmpVal.Value = "運送状計"

                        'フラグ・カウントの初期化
                        isBreak = False
                        tankCnt = 0
                        rowCounter = rowCounter + 1
                        currentRow = startRow + rowCounter
                    End If
                Else rowCounter = 0
                    For Each fieldName In breakFields
                        breakKey(fieldName) = Convert.ToString(dr(fieldName))
                    Next
                End If
                '一括貼り付け用の配列に値を設定
                valuesArray(0) = dr("FIXEDNO") '固定No
                valuesArray(1) = dr("AGREEMENTCODE") '協定コード
                valuesArray(2) = Convert.ToString(dr("EXTRADISCOUNTCODE")) '割引コード
                valuesArray(3) = dr("TAKUSOUOILCODE") '品目コード
                valuesArray(4) = dr("TRTYPE") '車種コード
                valuesArray(5) = dr("TRAINNO") '貨車番号
                valuesArray(6) = dr("TANKNUMBER") '列車番号
                valuesArray(7) = dr("ARRSTATIONNAME") '着駅名
                valuesArray(8) = dr("TAKUSOUNAME")  '荷受人名

                '指定した範囲に値を一括貼り付け
                rngTmpVal = Me.ExcelWorkSheet.Range(String.Format("B{0}:J{0}", currentRow))
                rngTmpVal.FirstCell.Value = New List(Of Object) From {valuesArray}
                '行カウント、タンク数のインクリメント
                rowCounter = rowCounter + 1
                currentRow = startRow + rowCounter
                tankCnt = tankCnt + 1
            Next dr
            '最終行は合計が付かない為こちらで設定
            rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol1 & currentRow)
            rngTmpVal.Value = String.Format(summaryCol1Format, tankCnt)
            rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol2 & currentRow)
            rngTmpVal.Value = "運送状計"
        Catch ex As Exception
            Throw
        End Try

    End Sub

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
        If Me.FS IsNot Nothing Then
            Me.FS.Close()
            Me.FS.Dispose()
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

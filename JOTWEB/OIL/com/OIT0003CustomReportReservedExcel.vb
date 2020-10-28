Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 出荷予約Excel出力クラス（Excelベース）
''' </summary>
''' <remarks>現状袖ヶ浦のみの想定
''' ※適切に設定すれば他でも使えるように構築</remarks>
Public Class OIT0003CustomReportReservedExcel : Implements System.IDisposable
    ''' <summary>
    ''' エクセルアプリケーションオブジェクト
    ''' </summary>
    Private ExcelAppObj As Excel.Application
    ''' <summary>
    ''' エクセルブックコレクション
    ''' </summary>
    Private ExcelBooksObj As Excel.Workbooks
    ''' <summary>
    ''' エクセルブックオブジェクト
    ''' </summary>
    Private ExcelBookObj As Excel.Workbook
    ''' <summary>
    ''' エクセルシートコレクション
    ''' </summary>
    Private ExcelWorkSheets As Excel.Sheets
    ''' <summary>
    ''' エクセルシートオブジェクト
    ''' </summary>
    Private ExcelWorkSheet As Excel.Worksheet

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private xlProcId As Integer
    Public Property FileNameWithoutExtention As String = ""
    Public Property FileExtention As String = ""
    Public Property OutputDef As OIT0003OTLinkageList.FileLinkagePatternItem

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="printDataClass"></param>
    ''' <param name="outputDef">出力設定クラス</param>
    Public Sub New(printDataClass As DataTable, outputDef As OIT0003OTLinkageList.FileLinkagePatternItem, Optional fileName As String = "", Optional fileExtention As String = "")
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Dim templateParentFolderName As String = "OIT0003Reserved"
            Dim tempXlsFileName As String = "" '[営業所コード].xlsxとする
            tempXlsFileName = String.Format("{0}.xlsx", outputDef.OfficeCode)

            Me.PrintData = printDataClass
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      templateParentFolderName, tempXlsFileName)

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
                Dim dirfileName As String = IO.Path.GetFileName(targetFile)
                '今日の日付が先頭のファイル名の場合は残す
                If dirfileName.StartsWith(keepFilePrefix) Then
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

            'Excelアプリケーションオブジェクトの生成
            Me.ExcelAppObj = New Excel.Application
            ExcelAppObj.DisplayAlerts = False
            Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
            GetWindowThreadProcessId(xlHwnd, Me.xlProcId)
            'Excelワークブックオブジェクトの生成
            Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
            Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
            '先頭のシートを設定
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets(1), Excel.Worksheet)
            Me.OutputDef = outputDef
            Me.FileNameWithoutExtention = fileName
            Me.FileExtention = fileExtention
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub
    ''' <summary>
    ''' 帳票作成処理
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData() As String

        Dim defaultExtention As String = "xlsx"
        If OutputDef.ReservedOutputType = OIT0003OTLinkageList.FileLinkagePatternItem.ReserveOutputFileType.Excel2003 Then
            defaultExtention = "xls"
        ElseIf OutputDef.ReservedOutputType = OIT0003OTLinkageList.FileLinkagePatternItem.ReserveOutputFileType.Pdf Then
            defaultExtention = "pdf"
        End If

        Dim tmpFileName As String = String.Format("{0:yyyyMMddHHmmss}{1}.{2}", DateTime.Now, DateTime.Now.Millisecond.ToString, defaultExtention)
        If Me.FileNameWithoutExtention <> "" AndAlso Me.FileExtention <> "" Then
            tmpFileName = String.Format("{0}.{1}", Me.FileNameWithoutExtention, Me.FileExtention)
        ElseIf Me.FileNameWithoutExtention <> "" AndAlso Me.FileExtention = "" Then
            tmpFileName = String.Format("{0}.{1}", Me.FileNameWithoutExtention, defaultExtention)
        End If
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '◯ヘッダーの設定
            EditHeaderArea()

            '◯明細の設定
            EditDetailArea()

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                If OutputDef.ReservedOutputType = OIT0003OTLinkageList.FileLinkagePatternItem.ReserveOutputFileType.Excel2007 Then
                    'Excel2007以降形式(4文字拡張子出力時)
                    Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
                ElseIf OutputDef.ReservedOutputType = OIT0003OTLinkageList.FileLinkagePatternItem.ReserveOutputFileType.Excel2003 Then
                    'Excel2003形式(3文字拡張子出力時)
                    Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlExcel8)
                Else
                    'PDF出力時
                    Me.ExcelWorkSheet.ExportAsFixedFormat(Type:=0,
                                                         Filename:=tmpFilePath,
                                                         Quality:=0,
                                                         IncludeDocProperties:=True,
                                                         IgnorePrintAreas:=False,
                                                         OpenAfterPublish:=False)


                    ''PDF印刷でのファイル出力
                    'Me.ExcelWorkSheet.PrintOut(ActivePrinter:="Microsoft Print to PDF",
                    '                           PrintToFile:=True, PrToFileName:=tmpFilePath)

                End If

            End SyncLock
            Me.ExcelBookObj.Close(False)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try
    End Function
    ''' <summary>
    ''' 託送指示ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea()
        If OutputDef.OfficeCode <> "011203" Then
            '一旦袖ヶ浦以外は何もしない
            Return
        End If
        Dim firstDr As DataRow = Me.PrintData.Rows(0)
        Dim lodDate As String = Convert.ToString(firstDr("LODDATE"))
        Dim rngHeaderArea As Excel.Range = Nothing
        Try
            '積込日
            rngHeaderArea = Me.ExcelWorkSheet.Range("A1")
            rngHeaderArea.Value = CDate(lodDate)
            ExcelMemoryRelease(rngHeaderArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub
    ''' <summary>
    ''' 明細部分の編集
    ''' </summary>
    Private Sub EditDetailArea()
        Dim rngTmpStart As Excel.Range = Nothing
        Dim rngTmpVal As Excel.Range = Nothing
        Try

            '貼り付け領域を配列に設定
            Dim rowCnt As Integer = Me.PrintData.Rows.Count
            Dim colCnt As Integer = Me.OutputDef.OutputFiledList.Count
            Dim objOutputValues(rowCnt - 1, colCnt - 1) As Object
            Dim rowCounter = 0
            Dim colCounter = 0
            'データテーブルのループ
            For Each dr As DataRow In Me.PrintData.Rows
                '出力対象カラムのループ
                colCounter = 0
                For Each coldef In Me.OutputDef.OutputFiledList
                    If coldef.Value >= 0 Then
                        objOutputValues(rowCounter, colCounter) = Convert.ToString(dr(coldef.Key))
                    ElseIf coldef.Value = -1 Then
                        Dim valStr As String = Convert.ToString(dr(coldef.Key))
                        If IsDate(valStr) Then
                            objOutputValues(rowCounter, colCounter) = CDate(valStr)
                        End If
                    ElseIf coldef.Value = -2 Then
                        Dim valStr As String = Convert.ToString(dr(coldef.Key))
                        If IsNumeric(valStr) Then
                            objOutputValues(rowCounter, colCounter) = CDec(valStr)
                        End If
                    End If

                    colCounter = colCounter + 1
                Next coldef
                rowCounter = rowCounter + 1
            Next dr
            rngTmpStart = DirectCast(Me.ExcelWorkSheet.Range(Me.OutputDef.OutputReservedExcelDataStartAddress), Excel.Range)
            rngTmpVal = rngTmpStart.Resize(rowCounter, colCounter)
            rngTmpVal.Value = objOutputValues
            ExcelMemoryRelease(rngTmpVal)
            ExcelMemoryRelease(rngTmpStart)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTmpVal)
            ExcelMemoryRelease(rngTmpStart)
        End Try


    End Sub

    ''' <summary>
    ''' Excelオブジェクトの解放
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="objCom"></param>
    Private Sub ExcelMemoryRelease(Of T As Class)(ByRef objCom As T)

        'ランタイム実行対象がComObjectのアンマネージコードの場合、メモリ開放
        If objCom Is Nothing Then
            Return
        Else
            Try
                If Marshal.IsComObject(objCom) Then
                    Dim count As Integer = Marshal.FinalReleaseComObject(objCom)
                End If
            Finally
                objCom = Nothing
            End Try
        End If

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
        'Excel Sheetオブジェクトの解放
        ExcelMemoryRelease(ExcelWorkSheet)
        'Excel Sheetコレクションの解放
        ExcelMemoryRelease(ExcelWorkSheets)
        'Excel Bookオブジェクトを閉じる
        If ExcelBookObj IsNot Nothing Then
            Try
                'ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
                ExcelBookObj.Close(False)
            Catch ex As Exception
            End Try
        End If

        ExcelMemoryRelease(ExcelBookObj)
        'Excel Bookコレクションの解放
        ExcelMemoryRelease(ExcelBooksObj)
        'Excel Appの終了
        If ExcelAppObj IsNot Nothing Then
            Try
                ExcelAppObj.Quit()
            Catch ex As Exception
            End Try
        End If
        ExcelProcEnd()
        disposedValue = True
    End Sub
    ''' <summary>
    ''' Excelプロセスの終了
    ''' </summary>
    Private Sub ExcelProcEnd()
        ExcelMemoryRelease(ExcelAppObj)
        Try
            '念のため当処理で起動したプロセスが残っていたらKill
            Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
            System.Threading.Thread.Sleep(200) 'Waitかけないとプロセスが終了しきらない為
            If Not xproc.HasExited Then
                xproc.Kill()
            End If
        Catch ex As Exception
        End Try
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

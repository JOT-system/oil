﻿Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 託送指示書(Excel/(orPDF))作成クラス
''' </summary>
''' <remarks>20200918時点ガワのみ</remarks>
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
    ''' 一時作業シート
    ''' </summary>
    Private ExcelTempSheet As Excel.Worksheet

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private SealImageFilePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private xlProcId As Integer

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="officeCode"></param>
    ''' <param name="printDataClass"></param>
    Public Sub New(officeCode As String, printDataClass As DataTable)
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
        Me.SealImageFilePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      templateParentFolderName, tempSealImageFileName)
        If IO.File.Exists(Me.ExcelTemplatePath) = False Then
            Throw New Exception(String.Format("テンプレートファイルが存在しません。{0}", Me.ExcelTemplatePath))
        End If

        If IO.File.Exists(Me.SealImageFilePath) = False Then
            Me.SealImageFilePath = ""
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
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("運送状"), Excel.Worksheet)
        Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
    End Sub
    ''' <summary>
    ''' 帳票作成処理
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData() As String
        Dim outputExtention As String = "pdf"
        If Me.FileType = OutputFileType.Excel Then
            outputExtention = "xlsx"
        End If
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = String.Format("{0:yyyyMMddHHmmss}{1}.{2}", DateTime.Now, DateTime.Now.Millisecond.ToString, outputExtention)
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea()
            '○ヘッダーに印画像の挿入
            EditHeaderSealArea()
            '◯明細の設定
            EditDetailArea()
            '***** TODO処理 ここまで *****
            ExcelTempSheet.Delete() '雛形シート削除

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                If Me.FileType = OutputFileType.Excel Then
                    'Excel出力時
                    Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)
                Else
                    'PDF出力時
                    '透過がワークしないためプランB
                    'Me.ExcelWorkSheet.ExportAsFixedFormat(Type:=0,
                    '                                     Filename:=tmpFilePath,
                    '                                     Quality:=0,
                    '                                     IncludeDocProperties:=True,
                    '                                     IgnorePrintAreas:=False,
                    '                                     OpenAfterPublish:=False)


                    'PDF印刷でのファイル出力
                    Me.ExcelWorkSheet.PrintOut(ActivePrinter:="Microsoft Print to PDF",
                                               PrintToFile:=True, PrToFileName:=tmpFilePath)

                End If

            End SyncLock
            Me.ExcelBookObj.Close(False)

            ''ストリーム生成
            'Using fs As New IO.FileStream(tmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            '    Dim binaryLength = Convert.ToInt32(fs.Length)
            '    ReDim retByte(binaryLength)
            '    fs.Read(retByte, 0, binaryLength)
            '    fs.Flush()
            'End Using
            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
            ExcelMemoryRelease(rngWrite)
        End Try
    End Function
    ''' <summary>
    ''' 託送指示ヘッダー部の設定
    ''' </summary>
    Private Sub EditHeaderArea()
        Dim firstDr As DataRow = Me.PrintData.Rows(0)
        Dim hkDate As String = Convert.ToString(firstDr("HKDATE"))
        Dim depStation As String = Convert.ToString(firstDr("DEPSTATIONNAME"))
        Dim rngHeaderArea As Excel.Range = Nothing
        Try
            '発駅名
            rngHeaderArea = Me.ExcelWorkSheet.Range("C3")
            rngHeaderArea.Value = depStation
            ExcelMemoryRelease(rngHeaderArea)
            '発行日
            rngHeaderArea = Me.ExcelWorkSheet.Range("M2")
            rngHeaderArea.Value = hkDate
            ExcelMemoryRelease(rngHeaderArea)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try

    End Sub
    ''' <summary>
    ''' 印鑑画像貼り付け処理
    ''' </summary>
    Private Sub EditHeaderSealArea()
        '画像ファイルが存在しない場合以下のプロパティは空白となる為スキップ
        If Me.SealImageFilePath = "" Then
            Return
        End If
        Dim excelShps As Excel.Shapes = Nothing
        Dim addedShape As Excel.Shape = Nothing
        Dim pasteTopLeftCell As Excel.Range = Nothing
        Try
            pasteTopLeftCell = Me.ExcelWorkSheet.Range("D1")
            Dim top As Single = CSng(pasteTopLeftCell.Top)
            Dim left As Single = CSng(pasteTopLeftCell.Left)
            ExcelMemoryRelease(pasteTopLeftCell)

            excelShps = Me.ExcelWorkSheet.Shapes
            Dim width As Single = 91
            Dim height As Single = width
            addedShape = excelShps.AddPicture(Me.SealImageFilePath, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue, left, top, width, height)

            'addedShape.ScaleHeight(1, Microsoft.Office.Core.MsoTriState.msoTrue)
            'addedShape.ScaleWidth(1, Microsoft.Office.Core.MsoTriState.msoTrue)

            ExcelMemoryRelease(addedShape)
            ExcelMemoryRelease(excelShps)

        Catch ex As Exception
            Throw New Exception("画像設定に失敗" & ex.ToString)
        Finally
            ExcelMemoryRelease(pasteTopLeftCell)
            ExcelMemoryRelease(addedShape)
            ExcelMemoryRelease(excelShps)
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

        Dim rngTmpVal As Excel.Range = Nothing
        Try
            Dim startRow As Integer = 8
            Dim rowCounter As Integer = 0
            Dim tankCnt As Integer = 0
            Dim isBreak As Boolean = False
            Dim summaryCol1 As String = "G"
            Dim summaryCol1Format As String = "{0:#,##0}両"
            Dim summaryCol2 As String = "I"
            Dim currentRow As Integer = startRow + rowCounter
            Dim valuesArray(0, 8) As Object
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
                        ExcelMemoryRelease(rngTmpVal)
                        rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol2 & currentRow)
                        rngTmpVal.Value = "運送状計"
                        ExcelMemoryRelease(rngTmpVal)
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
                valuesArray(0, 0) = dr("FIXEDNO") '固定No
                valuesArray(0, 1) = dr("AGREEMENTCODE") '協定コード
                valuesArray(0, 2) = Convert.ToString(dr("EXTRADISCOUNTCODE")) '割引コード
                valuesArray(0, 3) = dr("TAKUSOUOILCODE") '品目コード
                valuesArray(0, 4) = dr("TRTYPE") '車種コード
                valuesArray(0, 5) = dr("TRAINNO") '貨車番号
                valuesArray(0, 6) = dr("TANKNUMBER") '列車番号
                valuesArray(0, 7) = dr("ARRSTATIONNAME") '着駅名
                valuesArray(0, 8) = dr("TAKUSOUNAME")  '荷受人名
                rngTmpVal = Me.ExcelWorkSheet.Range(String.Format("B{0}:J{0}", currentRow))
                rngTmpVal.Value = valuesArray
                ExcelMemoryRelease(rngTmpVal)
                rowCounter = rowCounter + 1
                currentRow = startRow + rowCounter
                tankCnt = tankCnt + 1
            Next dr
            'ブレイクし合計行の追加
            rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol1 & currentRow)
            rngTmpVal.Value = String.Format(summaryCol1Format, tankCnt)
            ExcelMemoryRelease(rngTmpVal)
            rngTmpVal = Me.ExcelWorkSheet.Range(summaryCol2 & currentRow)
            rngTmpVal.Value = "運送状計"
            ExcelMemoryRelease(rngTmpVal)
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngTmpVal)
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
        ExcelMemoryRelease(ExcelTempSheet)
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

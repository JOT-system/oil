Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' OT発送日報帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0003CustomReportHassouExcel : Implements IDisposable
    'Public Enum OutputFileType
    '    Excel = 0
    '    Pdf = 1
    'End Enum
    'Public Property FileType As OutputFileType = OutputFileType.Pdf
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
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable
    Private xlProcId As Integer
    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                          "PRINTFORMAT",
                                                          C_DEFAULT_DATAKEY,
                                                          mapId, excelFileName)
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
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

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
            If excelFileName = "OIT0003OTL_HassouCheck.xlsx" Then
                Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets("OT発送日報"), Excel.Worksheet)
                Me.ExcelTempSheet = DirectCast(Me.ExcelWorkSheets("tempWork"), Excel.Worksheet)
            End If
        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロード(OT発送日報)URLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal tyohyoType As String, ByVal officeCode As String, Optional ByVal lodDate As String = Nothing) As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".pdf"
        'Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Select Case tyohyoType
                '固定帳票(OT発送日報)作成処理
                Case "OTHASSOU"
                    '***** TODO処理 ここから *****
                    '◯ヘッダーの設定
                    EditOTHassouHeaderArea(lodDate, officeCode)
                    '◯明細の設定
                    EditOTHassouDetailArea(officeCode)
                    '***** TODO処理 ここまで *****
                    ExcelTempSheet.Delete() '雛形シート削除
                    ExcelMemoryRelease(ExcelTempSheet)
            End Select

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                ''Excel出力時
                'Me.ExcelBookObj.SaveAs(tmpFilePath, Excel.XlFileFormat.xlOpenXMLWorkbook)

                'PDF出力時
                '透過がワークしないためプランB
                Me.ExcelWorkSheet.ExportAsFixedFormat(Type:=0,
                                                     Filename:=tmpFilePath,
                                                     Quality:=0,
                                                     IncludeDocProperties:=True,
                                                     IgnorePrintAreas:=False,
                                                     OpenAfterPublish:=False)

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
    ''' 帳票のヘッダー設定(OT発送日報)
    ''' </summary>
    Private Sub EditOTHassouHeaderArea(ByVal lodDate As String, ByVal officeCode As String)
        Dim rngHeaderArea As Excel.Range = Nothing
        Dim value As String = Now.AddDays(0).ToString("yyyy年MM月dd日（ddd）", New Globalization.CultureInfo("ja-JP"))

        Try
            For Each PrintDatarow As DataRow In PrintData.Rows
                '◯ 営業所名
                rngHeaderArea = Me.ExcelWorkSheet.Range("B1")
                rngHeaderArea.Value = PrintDatarow("OFFICENAME")
                ExcelMemoryRelease(rngHeaderArea)
                '◯ 作成日(当日)
                rngHeaderArea = Me.ExcelWorkSheet.Range("L1")
                rngHeaderArea.Value = value
                ExcelMemoryRelease(rngHeaderArea)
                'Dim value As Date = Now.AddDays(0)
                'rngHeaderArea = Me.ExcelWorkSheet.Range("L1")
                'rngHeaderArea.Value = value.ToString("MM月dd日分", New Globalization.CultureInfo("ja-JP"))

                Exit For
            Next
        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngHeaderArea)
        End Try
    End Sub
    ''' <summary>
    ''' 帳票の明細設定(OT発送日報)
    ''' </summary>
    Private Sub EditOTHassouDetailArea(ByVal officeCode As String)
        Dim rngDetailArea As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Dim rngSummary As Excel.Range = Nothing
        Dim strTrainNoSave As String = ""
        Dim strTrainNameSave As String = ""
        'Dim strTotalTankSave As String = ""
        'Dim strOTTransportSave As String = ""
        Dim blnNewLine As Boolean = False

        Try
            Dim iLine() As Integer = {5, 35, 65, 95, 125, 155, 185, 215, 245, 275, 305}
            Dim j As Integer = 0
            Dim i As Integer = iLine(j)
            'Dim i As Integer = 5
            Dim iNo As Integer = 1
            Dim iTotalCnt As Integer = 0
            Dim iTotalAmount As Decimal = 0
            For Each PrintDatarow As DataRow In PrintData.Rows

                '### 合計表示の処理 START #####################################################################
                '○前回の列車名と今回の列車名が不一致
                'If strTrainNameSave <> PrintDatarow("TRAINNAME").ToString() Then
                If strTrainNoSave <> "" AndAlso strTrainNoSave <> PrintDatarow("TRAINNO").ToString() Then
                    blnNewLine = True
                End If

                '★合計表示対象の場合
                If blnNewLine = True Then
                    '★tmpシートより合計行をコピーして値を設定
                    rngSummary = Me.ExcelTempSheet.Range("B1:L1")
                    rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "L" + i.ToString())
                    'rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                    rngSummary.Copy(rngTmp)
                    ExcelMemoryRelease(rngSummary)
                    ExcelMemoryRelease(rngTmp)
                    '◯ 合計車数
                    rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                    rngDetailArea.Value = Convert.ToString(iTotalCnt) + "両"
                    'rngDetailArea.Value = strTotalTankSave + "両"
                    ExcelMemoryRelease(rngDetailArea)
                    '◯ 合計数量
                    rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                    rngDetailArea.Value = Convert.ToString(iTotalAmount) + "(kl)"
                    ExcelMemoryRelease(rngDetailArea)

                    j += 1
                    i = iLine(j)
                    'i += 1
                    iNo = 1
                    iTotalCnt = 0
                    iTotalAmount = 0
                    blnNewLine = False
                End If
                '### 合計表示の処理 END   #####################################################################

                '◯ No
                rngDetailArea = Me.ExcelWorkSheet.Range("B" + i.ToString())
                rngDetailArea.Value = iNo
                ExcelMemoryRelease(rngDetailArea)
                '◯ OT営業所
                rngDetailArea = Me.ExcelWorkSheet.Range("C" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OTDAILYCONSIGNEEN")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 荷主
                rngDetailArea = Me.ExcelWorkSheet.Range("D" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPPERSNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 発送年月日
                rngDetailArea = Me.ExcelWorkSheet.Range("E" + i.ToString())
                rngDetailArea.Value = PrintDatarow("LODDATE")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 列車№
                rngDetailArea = Me.ExcelWorkSheet.Range("F" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TRAINNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 着駅
                rngDetailArea = Me.ExcelWorkSheet.Range("G" + i.ToString())
                rngDetailArea.Value = PrintDatarow("ARRSTATIONNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 油種
                rngDetailArea = Me.ExcelWorkSheet.Range("H" + i.ToString())
                rngDetailArea.Value = PrintDatarow("OTOILNAME")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 車号
                rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
                rngDetailArea.Value = PrintDatarow("TANKNO")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 数量
                rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
                rngDetailArea.Value = PrintDatarow("CARSAMOUNT")
                ExcelMemoryRelease(rngDetailArea)
                '◯ 連結順位
                rngDetailArea = Me.ExcelWorkSheet.Range("K" + i.ToString())
                rngDetailArea.Value = PrintDatarow("SHIPORDER")
                ExcelMemoryRelease(rngDetailArea)
                '○備考
                rngDetailArea = Me.ExcelWorkSheet.Range("L" + i.ToString())
                rngDetailArea.Value = PrintDatarow("REMARK")
                ExcelMemoryRelease(rngDetailArea)

                '★ 列車名・合計車数を退避
                strTrainNoSave = PrintDatarow("TRAINNO").ToString()
                'strTrainNameSave = PrintDatarow("TRAINNAME").ToString()
                'strTotalTankSave = PrintDatarow("TOTALTANK").ToString()
                'strOTTransportSave = PrintDatarow("OTTRANSPORTFLG").ToString()

                i += 1
                iNo += 1
                iTotalCnt += 1
                iTotalAmount += Decimal.Parse(PrintDatarow("CARSAMOUNT").ToString())
            Next

            '### 合計表示の処理 START #####################################################################
            '★tmpシートより合計行をコピーして値を設定
            rngSummary = Me.ExcelTempSheet.Range("B1:L1")
            rngTmp = Me.ExcelWorkSheet.Range("B" + i.ToString(), "L" + i.ToString())
            'rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
            rngSummary.Copy(rngTmp)
            ExcelMemoryRelease(rngSummary)
            ExcelMemoryRelease(rngTmp)
            '◯ 合計車数
            rngDetailArea = Me.ExcelWorkSheet.Range("I" + i.ToString())
            rngDetailArea.Value = Convert.ToString(iTotalCnt) + "両"
            'rngDetailArea.Value = strTotalTankSave + "両"
            ExcelMemoryRelease(rngDetailArea)
            '◯ 合計数量
            rngDetailArea = Me.ExcelWorkSheet.Range("J" + i.ToString())
            rngDetailArea.Value = Convert.ToString(iTotalAmount) + "(kl)"
            ExcelMemoryRelease(rngDetailArea)
            '### 合計表示の処理 END   #####################################################################

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngDetailArea)
        End Try

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

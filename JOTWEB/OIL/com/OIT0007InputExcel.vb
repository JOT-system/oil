Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 出荷予約実績Excel取込クラス
''' </summary>
Public Class OIT0007InputExcel : Implements IDisposable
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
    ''' アップロードファイルパス
    ''' </summary>
    Private FilePath As String = ""
    Private InputSettings As OIT0007FileInputList.FileLinkagePatternItem
    Private xlProcId As Integer

    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="inputSettings">取込設定クラス</param>
    ''' <param name="filePath">取込ファイルパス</param>
    ''' 
    Public Sub New(inputSettings As OIT0007FileInputList.FileLinkagePatternItem, filePath As String)
        Try
            Me.FilePath = filePath
            Me.InputSettings = inputSettings
            'ありえないがアップロードファイルの存在チェック
            If IO.File.Exists(Me.FilePath) = False Then
                Throw New Exception(String.Format("アップロードファイルが存在しません。{0}", Me.FilePath))
            End If
            'Excelアプリケーションオブジェクトの生成
            Me.ExcelAppObj = New Excel.Application
            ExcelAppObj.DisplayAlerts = False
            Dim xlHwnd As IntPtr = CType(Me.ExcelAppObj.Hwnd, IntPtr)
            GetWindowThreadProcessId(xlHwnd, Me.xlProcId)

            'Excelワークブックオブジェクトの生成
            Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
            Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.FilePath,
                                            UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                            [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
            Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
            Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets(1), Excel.Worksheet)

        Catch ex As Exception
            If Me.xlProcId <> 0 Then
                ExcelProcEnd()
            End If
            Throw
        End Try
    End Sub
    ''' <summary>
    ''' CSVファイル読み込みメソッド
    ''' </summary>
    ''' <returns></returns>
    Public Function ReadExcel() As List(Of OIT0007FileInputList.InputDataItem)
        If Me.InputSettings.OfficeCode = "011203" Then
            '袖ヶ浦ファイル読み取り
            Return Read011203Sode()
        Else
            Return Nothing
        End If
    End Function
    ''' <summary>
    ''' 甲子アップロードファイル読み取り
    ''' </summary>
    ''' <returns></returns>
    Private Function Read011203Sode() As List(Of OIT0007FileInputList.InputDataItem)
        Dim rowCounter As Integer = 2
        Dim reservedNo As String = ""
        Dim lineNo As Integer = 1
        Dim retVal As New List(Of OIT0007FileInputList.InputDataItem)
        reservedNo = GetRangeVal("C" & rowCounter.ToString)

        Do Until reservedNo = ""
            Dim newItm As New OIT0007FileInputList.InputDataItem
            newItm.InpRowNum = lineNo
            newItm.InpReservedNo = reservedNo
            If IsNumeric(reservedNo) AndAlso reservedNo.Length = 10 Then
                newItm.LodDate = CInt(Left(newItm.InpReservedNo, 8)).ToString("0000/00/00")
                newItm.ReservedNo = CInt(Right(newItm.InpReservedNo, 2)).ToString("000")
                If IsDate(newItm.LodDate) = False Then
                    newItm.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.NoOrderInfo
                End If
            Else
                newItm.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.NoOrderInfo
            End If
            newItm.InpTnkNo = GetRangeVal("O" & rowCounter.ToString)

            newItm.InpOilTypeName = GetRangeVal("K" & rowCounter.ToString)
            newItm.InpCarsAmount = GetRangeVal("M" & rowCounter.ToString)
            If IsNumeric(newItm.InpCarsAmount) = False OrElse
                   CDec(newItm.InpCarsAmount) >= 100 Then
                newItm.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.AmountFormatError
            Else
                newItm.InpCarsAmount = CDec(newItm.InpCarsAmount).ToString("00.000")
            End If
            rowCounter = rowCounter + 1
            reservedNo = GetRangeVal("C" & rowCounter.ToString)
            retVal.Add(newItm)
            lineNo = lineNo + 1
        Loop
        Return retVal
    End Function
    ''' <summary>
    ''' Excel値取得解放実行
    ''' </summary>
    ''' <param name="rngString"></param>
    ''' <returns></returns>
    Private Function GetRangeVal(rngString As String) As String
        Dim rngVal As Excel.Range = Nothing
        Dim retVal As String
        rngVal = Me.ExcelWorkSheet.Range(rngString)
        retVal = Convert.ToString(rngVal.Value)
        ExcelMemoryRelease(rngVal)
        Return retVal
    End Function
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

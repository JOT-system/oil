Option Strict On
Imports Microsoft.Office.Interop
Imports System.Runtime.InteropServices
''' <summary>
''' 在庫管理表個別帳票作成クラス
''' </summary>
''' <remarks>当クラスはUsingで使用する事
''' （ファイナライザで正しくExcelオブジェクトを破棄）</remarks>
Public Class OIT0004CustomReport : Implements IDisposable
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
    Private PrintData As OIT0004OilStockCreate.DispDataClass
    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As OIT0004OilStockCreate.DispDataClass)
        Dim CS0050SESSION As New CS0050SESSION
        Me.PrintData = printDataClass
        Me.ExcelTemplatePath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                      "PRINTFORMAT",
                                                      C_DEFAULT_DATAKEY,
                                                      mapId, excelFileName)
        Me.UploadRootPath = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
        Me.UrlRoot = String.Format("{0}://{1}/PRINT/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID)

        'Excelアプリケーションオブジェクトの生成
        Me.ExcelAppObj = New Excel.Application
        'Excelワークブックオブジェクトの生成
        Me.ExcelBooksObj = Me.ExcelAppObj.Workbooks
        Me.ExcelBookObj = Me.ExcelBooksObj.Open(Me.ExcelTemplatePath,
                                                UpdateLinks:=Excel.XlUpdateLinks.xlUpdateLinksNever,
                                                [ReadOnly]:=Excel.XlFileAccess.xlReadOnly)
        Me.ExcelWorkSheets = Me.ExcelBookObj.Sheets
        Me.ExcelWorkSheet = DirectCast(Me.ExcelWorkSheets(1), Excel.Worksheet)
    End Sub
    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData() As String
        Dim rngWrite As Excel.Range = Nothing
        Dim tmpFileName As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Try
            '***** TODO処理 ここから *****
            'rngWrite = Me.ExcelWorkSheet.Range("A1")
            'rngWrite.Value = "test"
            'EditSuggestArea()
            '***** TODO処理 ここまで *****
            '保存処理実行
            Me.ExcelBookObj.SaveAs(tmpFilePath)
            Me.ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
            'ストリーム生成
            Using fs As New IO.FileStream(tmpFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                Dim binaryLength = Convert.ToInt32(fs.Length)
                ReDim retByte(binaryLength)
                fs.Read(retByte, 0, binaryLength)
                fs.Flush()
            End Using
            Return UrlRoot & tmpFileName
        Catch ex As Exception
            Throw '呼出し元にThrow
        Finally
            ExcelMemoryRelease(rngWrite)
        End Try
    End Function
    ''' <summary>
    ''' 提案エリアの生成
    ''' </summary>
    Private Sub EditSuggestArea()
        Dim rngWork As Excel.Range = Nothing
        Dim rngTmp As Excel.Range = Nothing
        Try
            '提案欄非表示の場合はテンプレートより対象行削除し終了
            If Me.PrintData.ShowSuggestList = False Then
                rngWork = Me.ExcelWorkSheet.Range("3:9")
                rngWork.Delete(Excel.XlDeleteShiftDirection.xlShiftUp)
                Return
            End If
            '提案欄油種行の拡張（罫線維持）
            Dim oilTypeRowCnt = Me.PrintData.SuggestOilNameList.Count
            rngTmp = Me.ExcelWorkSheet.Range(String.Format("5:{0}", oilTypeRowCnt + 3))
            rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
            ExcelMemoryRelease(rngTmp)
            '提案欄の値を展開
            If Me.PrintData.HasMoveInsideItem Then
                '合計行の拡張
                Dim miStartRow = oilTypeRowCnt + 4
                rngWork = Me.ExcelWorkSheet.Range("{0}:{0}", miStartRow)
                rngTmp = Me.ExcelWorkSheet.Range("{0}:{0}", miStartRow + 1)
                rngWork.Copy(rngTmp)
                ExcelMemoryRelease(rngTmp)
                'データ行の拡張
                oilTypeRowCnt = Me.PrintData.MiDispData.SuggestOilNameList.Count
                rngTmp = Me.ExcelWorkSheet.Range(String.Format("{0}:{1}", miStartRow, miStartRow + oilTypeRowCnt - 1))
                rngTmp.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove)
                ExcelMemoryRelease(rngTmp)
            End If

        Catch ex As Exception
            Throw
        Finally
            ExcelMemoryRelease(rngWork)
            ExcelMemoryRelease(rngTmp)
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
            'Excel Sheetオブジェクトの解放
            ExcelMemoryRelease(ExcelWorkSheet)
            'Excel Sheetコレクションの解放
            ExcelMemoryRelease(ExcelWorkSheets)
            'Excel Bookオブジェクトを閉じる
            If ExcelBookObj IsNot Nothing Then
                Try
                    ExcelBookObj.Close(Excel.XlSaveAction.xlDoNotSaveChanges)
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
            ExcelMemoryRelease(ExcelAppObj)
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

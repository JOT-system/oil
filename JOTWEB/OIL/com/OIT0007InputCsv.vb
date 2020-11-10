Option Strict On
Public Class OIT0007InputCsv : Implements System.IDisposable
    ''' <summary>
    ''' CSVファイルストリーム
    ''' </summary>
    Private Fs As IO.FileStream = Nothing
    ''' <summary>
    ''' アップロードファイルパス
    ''' </summary>
    Private FilePath As String = ""
    Private InputSettings As OIT0007FileInputList.FileLinkagePatternItem

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
            '読み取り専用でファイルストリーム生成
            Me.Fs = New IO.FileStream(Me.FilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)

        Catch ex As Exception
            If Fs IsNot Nothing Then
                Fs.Close()
                Fs.Dispose()
            End If
            Throw
        End Try
    End Sub
    ''' <summary>
    ''' CSVファイル読み込みメソッド
    ''' </summary>
    ''' <returns></returns>
    Public Function ReadCsv() As List(Of OIT0007FileInputList.InputDataItem)
        If Me.InputSettings.OfficeCode = "011202" Then
            '甲子ファイル読み取り
            Return Read011202Kino()
        ElseIf Me.InputSettings.OfficeCode = "011402" Then
            '根岸ファイル読み取り
            Return Read011402Negi()
        Else
            Return Nothing
        End If
    End Function
    ''' <summary>
    ''' 甲子アップロードファイル読み取り
    ''' </summary>
    ''' <returns></returns>
    Private Function Read011202Kino() As List(Of OIT0007FileInputList.InputDataItem)
        Dim enc = System.Text.Encoding.GetEncoding("Shift-JIS")
        Dim lineNo As Integer = 1
        Dim retVal As New List(Of OIT0007FileInputList.InputDataItem)
        Using sr As New IO.StreamReader(Me.Fs, enc)
            While Not sr.EndOfStream
                Dim lineStr As String = sr.ReadLine()
                'ありえないが空行の場合はスキップ
                If lineStr.Trim = "" Then
                    Continue While
                End If
                'カンマで区切り配列に分割
                Dim colItems = lineStr.Split(","c)
                If Not colItems.Count = 11 Then
                    '11カラムじゃないと対象ファイル以外と判定
                    Continue While
                End If
                Dim itmData As New OIT0007FileInputList.InputDataItem
                itmData.InpRowNum = lineNo
                '実績積込日（更新対象）
                If IsNumeric(colItems(0)) AndAlso colItems(0).Length = 8 Then
                    itmData.UpdActualLodDate = CInt(colItems(0)).ToString("0000/00/00") 'スラッシュ無し年月日をスラッシュ付きに変換
                End If
                itmData.InpReservedNo = colItems(1)
                itmData.InpTnkNo = colItems(2)
                itmData.InpOilTypeName = colItems(3)
                itmData.InpCarsAmount = colItems(8)
                If IsNumeric(itmData.InpCarsAmount) = False OrElse
                   CDec(itmData.InpCarsAmount) >= 100 Then
                    '整数100以上、または数字以外が格納されている場合は対象外として判定
                    itmData.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.AmountFormatError
                End If
                '取り込んだ予約番号を積込予定日と予約番号３桁に分離
                If IsNumeric(itmData.InpReservedNo) AndAlso itmData.InpReservedNo.Length = 11 Then
                    itmData.LodDate = CInt(Left(itmData.InpReservedNo, 8)).ToString("0000/00/00")
                    itmData.ReservedNo = Right(itmData.InpReservedNo, 3)
                End If
                retVal.Add(itmData)
                lineNo = lineNo + 1
            End While
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 根岸アップロードファイル読み取り
    ''' </summary>  
    ''' <returns></returns>
    Private Function Read011402Negi() As List(Of OIT0007FileInputList.InputDataItem)
        Dim enc = System.Text.Encoding.GetEncoding("Shift-JIS")
        Dim lineNo As Integer = 1
        Dim retVal As New List(Of OIT0007FileInputList.InputDataItem)
        Using sr As New IO.StreamReader(Me.Fs, enc)
            If Not sr.EndOfStream Then
                '根岸一行目はヘッダーなので読み飛ばす
                sr.ReadLine()
            End If
            While Not sr.EndOfStream
                Dim lineStr As String = sr.ReadLine()
                'ありえないが空行の場合はスキップ
                If lineStr.Trim = "" Then
                    Continue While
                End If
                'カンマで区切り配列に分割
                Dim colItems = lineStr.Split(","c)
                If Not colItems.Count = 11 Then
                    'カラム数が11ではない場合対象外と判定
                    Continue While
                End If
                Dim itmData As New OIT0007FileInputList.InputDataItem
                itmData.InpRowNum = lineNo
                '実績積込日（更新対象）
                If IsNumeric(colItems(0)) AndAlso colItems(0).Length = 8 Then
                    itmData.UpdActualLodDate = CInt(colItems(0)).ToString("0000/00/00") 'スラッシュ無し年月日をスラッシュ付きに変換
                End If
                itmData.InpReservedNo = colItems(1)
                itmData.InpTnkNo = colItems(5)
                itmData.InpOilTypeName = colItems(6)
                itmData.InpCarsAmount = colItems(10)
                If IsNumeric(itmData.InpCarsAmount) AndAlso itmData.InpCarsAmount.Length = 5 Then
                    '小数点なしの為左２桁＆小数点＆右３桁で文字連結
                    itmData.InpCarsAmount = Left(itmData.InpCarsAmount, itmData.InpCarsAmount.Length - 3) & "." & Right(itmData.InpCarsAmount, 3)
                Else
                    'それ以外は書式エラーとする
                    itmData.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.AmountFormatError
                End If
                '取り込んだ予約番号を積込予定日と予約番号３桁に分離
                If IsNumeric(itmData.InpReservedNo) AndAlso itmData.InpReservedNo.Length = 3 Then
                    itmData.LodDate = itmData.UpdActualLodDate
                    itmData.ReservedNo = CInt(itmData.InpReservedNo).ToString("000")
                End If
                retVal.Add(itmData)
                lineNo = lineNo + 1
            End While
        End Using
        Return retVal
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
        If Fs IsNot Nothing Then
            Fs.Close()
            Fs.Dispose()
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

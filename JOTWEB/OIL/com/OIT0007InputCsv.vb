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
        ElseIf {"011201", "012401"}.Contains(Me.InputSettings.OfficeCode) Then
            '五井、四日市SEQファイルと判定
            Return ReadSeqFile()
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
                If IsNumeric(Trim(colItems(0))) AndAlso Trim(colItems(0)).Length = 8 Then
                    itmData.UpdActualLodDate = CInt(Trim(colItems(0))).ToString("0000/00/00") 'スラッシュ無し年月日をスラッシュ付きに変換
                End If
                itmData.InpReservedNo = Trim(colItems(1))
                itmData.InpTnkNo = Trim(colItems(2))
                itmData.InpOilTypeName = Trim(colItems(3))
                itmData.InpCarsAmount = Trim(colItems(8))
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
            '20201204 根岸はヘッダー付くはずだが田中さん連携ファイルにヘッダーがないのでこの処理コメント
            'If Not sr.EndOfStream Then
            '    '根岸一行目はヘッダーなので読み飛ばす
            '    sr.ReadLine()
            'End If
            Dim isFirstRow As Boolean = True
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
                '先頭行がカラム名ヘッダーが判定
                '20201204 根岸はヘッダー付くはずだが田中さん連携ファイルにヘッダーがないのでこの処理コメント0
                '本当のファイルにはヘッダーが付く可能性が否定出来ないので念のためヘッダー判定追加
                If isFirstRow Then
                    isFirstRow = False
                    If Not (IsNumeric(Trim(colItems(0))) AndAlso Trim(colItems(0)).Length = 8) Then
                        Continue While
                    End If
                End If
                Dim itmData As New OIT0007FileInputList.InputDataItem
                itmData.InpRowNum = lineNo
                '実績積込日（更新対象）
                If IsNumeric(Trim(colItems(0))) AndAlso Trim(colItems(0)).Length = 8 Then
                    itmData.UpdActualLodDate = CInt(Trim(colItems(0))).ToString("0000/00/00") 'スラッシュ無し年月日をスラッシュ付きに変換
                End If
                itmData.InpReservedNo = Trim(colItems(1))
                itmData.InpTnkNo = Trim(colItems(5))
                itmData.InpOilTypeName = Trim(colItems(6))
                itmData.InpCarsAmount = Trim(colItems(10))
                If IsNumeric(itmData.InpCarsAmount) AndAlso Not (CDec(itmData.InpCarsAmount) > 100000) Then
                    '小数点なしの為左２桁＆小数点＆右３桁で文字連結
                    'Dim intVal As String = "000"
                    'If IsNumeric(Left(itmData.InpCarsAmount, itmData.InpCarsAmount.Length - 3)) Then
                    '    intVal = CDec(Left(itmData.InpCarsAmount, itmData.InpCarsAmount.Length - 3)).ToString("00")
                    'End If
                    'itmData.InpCarsAmount = intVal & "." & Right(itmData.InpCarsAmount, 3)
                    Dim val = CDec(itmData.InpCarsAmount) / 1000
                    itmData.InpCarsAmount = val.ToString("#0.000")
                Else
                    'それ以外は書式エラーとする
                    itmData.CheckReadonCode = OIT0007FileInputList.InputDataItem.CheckReasonCodes.AmountFormatError
                End If
                '取り込んだ予約番号を積込予定日と予約番号３桁に分離
                If IsNumeric(itmData.InpReservedNo) AndAlso itmData.InpReservedNo.Length <= 3 Then
                    itmData.LodDate = itmData.UpdActualLodDate
                    itmData.ReservedNo = CInt(itmData.InpReservedNo).ToString("000")
                End If
                retVal.Add(itmData)
                lineNo = lineNo + 1
            End While
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' シーケンスファイルより入力データアイテムリストクラスに変換
    ''' </summary>
    ''' <returns></returns>
    Private Function ReadSeqFile() As List(Of OIT0007FileInputList.InputDataItem)
        Dim dt As DataTable = ReadSeqToDataTable()
        Dim lineNo As Integer = 1
        Dim retVal As New List(Of OIT0007FileInputList.InputDataItem)
        For Each dr As DataRow In dt.Rows
            Dim itmData As New OIT0007FileInputList.InputDataItem
            itmData.InpRowNum = lineNo
            '実績積込日（更新対象）
            Dim lodDateStr As String = Convert.ToString(dr("LODDATE_WITHOUT_SLASH"))
            If IsNumeric(lodDateStr) AndAlso lodDateStr.Length = 8 Then
                itmData.UpdActualLodDate = CInt(lodDateStr).ToString("0000/00/00") 'スラッシュ無し年月日をスラッシュ付きに変換
            End If
            itmData.InpReservedNo = Convert.ToString(dr("OUTPUTRESERVENO"))
            itmData.InpTnkNo = Convert.ToString(dr("SEQ_TANKNO"))
            itmData.InpOilTypeName = Convert.ToString(dr("SHIPPEROILCODE"))
            itmData.InpCarsAmount = Convert.ToString(dr("SEQ_ACCTUAL_AMOUNT"))
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
        Next dr
        Return retVal
    End Function
    ''' <summary>
    ''' 設定に合せシーケンスファイルを読み取り
    ''' </summary>
    ''' <returns></returns>
    Private Function ReadSeqToDataTable() As DataTable
        Dim retDt As DataTable = Nothing
        Dim enc = System.Text.Encoding.GetEncoding("Shift-JIS")
        Dim lineNo As Integer = 1
        Dim retVal As New List(Of OIT0007FileInputList.InputDataItem)
        Dim lineStr As String = ""
        'SEQファイルは改行なし１行の為、行ループで読み込まない
        Using sr As New IO.StreamReader(Me.Fs, enc)
            If sr.EndOfStream Then
                Return retDt
            End If
            '1行読み取り
            lineStr = sr.ReadLine()
        End Using
        '1行のバイト数が各フィールドのLength合計の倍数でない場合は対象外ファイルと判定
        Dim recLength = (From itm In Me.InputSettings.InputFiledList Select itm.Value).Sum
        Dim lineLength = enc.GetByteCount(lineStr)
        If lineLength Mod recLength <> 0 Then
            Return retDt
        End If
        '少なくともLengthにのっとったSEQの為処理開始
        retDt = New DataTable
        '********************************************
        '*テーブルカラム生成
        '********************************************
        For Each fldItem In Me.InputSettings.InputFiledList
            Dim colItem As New DataColumn(fldItem.Key, GetType(String))
            retDt.Columns.Add(colItem)
        Next
        Dim curLength As Integer = recLength
        Dim lineBytedata As Byte() = enc.GetBytes(lineStr)
        Do
            Dim startPosition = curLength - recLength
            '一行分切り出し
            Dim rowString = enc.GetString(lineBytedata, startPosition, recLength)
            Dim rowByteData As Byte() = enc.GetBytes(rowString)
            Dim dr As DataRow = retDt.NewRow
            '読み込みフィールドリストループ
            Dim startFieldPos As Integer = 0
            For Each fldItem In Me.InputSettings.InputFiledList
                Dim fieldString As String = enc.GetString(rowByteData, startFieldPos, fldItem.Value)
                dr(fldItem.Key) = fieldString.Trim
                startFieldPos = startFieldPos + fldItem.Value
            Next
            retDt.Rows.Add(dr)
            curLength = curLength + recLength
        Loop Until curLength > lineLength

        Return retDt
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

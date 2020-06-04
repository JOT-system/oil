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

    Private xlProcId As Integer
    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As OIT0004OilStockCreate.DispDataClass
    ''' <summary>
    ''' WindowハンドルよりProcessIDを取得
    ''' </summary>
    ''' <param name="hwnd"></param>
    ''' <param name="lpdwProcessId"></param>
    ''' <returns></returns>
    ''' <remarks>ExcelのWindowハンドルを探しプロセスIDを取得
    ''' 当処理で使用したExcelのプロセスIDが残っていた場合KILLする為使用</remarks>
    Private Declare Auto Function GetWindowThreadProcessId Lib "user32.dll" (ByVal hwnd As IntPtr,
              ByRef lpdwProcessId As Integer) As Integer
    Private Class OilTypeColorSettings
        Private ColorSettings As Dictionary(Of String, OilTypeColorSetting)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.ColorSettings = New Dictionary(Of String, OilTypeColorSetting)
            'ハイオク
            Me.ColorSettings.Add("1001", New OilTypeColorSetting(RGB(255, 255, 0), RGB(20, 23, 26)))
            'レギュラー
            Me.ColorSettings.Add("1101", New OilTypeColorSetting(RGB(255, 192, 0), RGB(255, 255, 255)))
            '灯油
            Me.ColorSettings.Add("1301", New OilTypeColorSetting(RGB(255, 255, 255), RGB(20, 23, 26)))
            '未添加灯油
            Me.ColorSettings.Add("1302", New OilTypeColorSetting(RGB(221, 245, 253), RGB(20, 23, 26)))
            '軽油
            Me.ColorSettings.Add("1401", New OilTypeColorSetting(RGB(0, 176, 80), RGB(255, 255, 255)))
            '3号軽油
            Me.ColorSettings.Add("1404", New OilTypeColorSetting(RGB(146, 208, 80), RGB(255, 255, 255)))
            'A重油
            Me.ColorSettings.Add("2101", New OilTypeColorSetting(RGB(0, 112, 192), RGB(255, 255, 255)))
            'LSA
            Me.ColorSettings.Add("2201", New OilTypeColorSetting(RGB(0, 176, 240), RGB(255, 255, 255)))
        End Sub
        ''' <summary>
        ''' 油種別の色情報取得
        ''' </summary>
        ''' <param name="oilTypeCode"></param>
        ''' <returns></returns>
        Public Function GetColor(oilTypeCode As String) As OilTypeColorSetting
            If Me.ColorSettings.ContainsKey(oilTypeCode) Then
                Return Me.ColorSettings(oilTypeCode)
            Else
                Return New OilTypeColorSetting(RGB(51, 152, 109), RGB(255, 255, 255))
            End If
        End Function

    End Class
    ''' <summary>
    ''' 色設定クラス
    ''' </summary>
    Private Class OilTypeColorSetting
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(backGroundColor As Integer, fontColor As Integer)
            Me.BackGroundColor = backGroundColor
            Me.FontColor = fontColor
        End Sub

        ''' <summary>
        ''' 背景色
        ''' </summary>
        ''' <returns></returns>
        Public Property BackGroundColor As Integer
        ''' <summary>
        ''' 文字色
        ''' </summary>
        ''' <returns></returns>
        Public Property FontColor As Integer
    End Class
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
            Try
                '念のため当処理で起動したプロセスが残っていたらKill
                Dim xproc As Process = Process.GetProcessById(Me.xlProcId)
                If Not xproc.HasExited Then
                    xproc.Kill()
                End If
            Catch ex As Exception
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

Option Strict On
''' <summary>
''' GS系根底クラス
''' </summary>
''' <remarks></remarks>
Public MustInherit Class GS0000 : Implements IDisposable

    ''' <summary>
    ''' タイトル区分
    ''' </summary>
    Protected Friend Class C_TITLEKBN
        ''' <summary>
        ''' ヘッダー
        ''' </summary>
        Public Const HEADER As String = "H"
        ''' <summary>
        ''' タイトル
        ''' </summary>
        Public Const TITLE As String = "T"
        ''' <summary>
        ''' 明細
        ''' </summary>
        Public Const DETAIL As String = "I"
        ''' <summary>
        ''' 繰り返しデータのキー項目
        ''' </summary>
        Public Const REPEAT_KEY As String = "I_DataKey"
        ''' <summary>
        ''' 繰り返しデータ
        ''' </summary>
        Public Const REPEAT_DATA As String = "I_Data"
    End Class
    ''' <summary>
    ''' HD区分
    ''' </summary>
    Protected Friend Class C_HDKBN
        ''' <summary>
        ''' ヘッダー
        ''' </summary>
        Public Const HEADER As String = "H"
        ''' <summary>
        ''' 明細
        ''' </summary>
        Public Const DETAIL As String = "I"
    End Class
    ''' <summary>
    ''' エラーメッセージ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ERR As String = C_MESSAGE_NO.NORMAL

    ''' <summary>
    ''' パラメータチェック処理
    ''' </summary>
    ''' <param name="subclass"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function checkParam(ByVal subclass As String, ByVal value As Object) As String

        If IsNothing(value) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = subclass
            CS0011LOGWRITE.INFPOSI = value.ToString
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            checkParam = C_MESSAGE_NO.DLL_IF_ERROR
        Else
            checkParam = C_MESSAGE_NO.NORMAL
        End If
    End Function
    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        'GC.SuppressFinalize(Me)
    End Sub
End Class


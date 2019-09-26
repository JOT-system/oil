Imports System.Data.SqlClient

''' <summary>
''' T00008関連共通クラス
''' </summary>
''' <remarks></remarks>
Public Class GRT0008COM : Implements IDisposable
    Private CS0050Session As New CS0050SESSION
    Private CS0011LOGWrite As New CS0011LOGWrite

    Private AffairList As New Hashtable
    ''' <summary>
    ''' 総務部確認処理
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">変換元部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Function IsGeneralAffair(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_RTN As String) As Boolean

        Const CLASS_CODE As String = "AFFIAIRSLIST"
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            If Not AffairList.ContainsKey(I_COMPCODE) Then
                Using GS0032 As New GS0032FIXVALUElst
                    GS0032.CAMPCODE = I_COMPCODE
                    GS0032.CLAS = CLASS_CODE
                    GS0032.STDATE = Date.Now
                    GS0032.ENDDATE = Date.Now
                    GS0032.GS0032FIXVALUElst()
                    If Not isNormal(GS0032.ERR) Then
                        O_RTN = GS0032.ERR
                        Return False
                    End If
                    AffairList.Add(I_COMPCODE, GS0032.VALUE1)
                End Using
            End If
            '存在する場合TRUE、しない場合FALSEを帰す
            Using Lst As ListBox = AffairList(I_COMPCODE)
                Return (Not IsNothing(Lst.Items.FindByValue(I_ORGCODE)))
            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "GRT0008COM"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:GENERAL_AFFAIRS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return False
        End Try

    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出する

    ' IDisposable
    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: マネージ状態を破棄します (マネージ オブジェクト)。
            End If

            ' TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下の Finalize() をオーバーライドします。
            ' TODO: 大きなフィールドを null に設定します。
        End If
        Me.disposedValue = True
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' このコードを変更しないでください。クリーンアップ コードを上の Dispose(disposing As Boolean) に記述します。
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

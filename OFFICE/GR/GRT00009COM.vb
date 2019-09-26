Imports System.Data.SqlClient

''' <summary>
''' T0009関連共通クラス
''' </summary>
''' <remarks></remarks>
Public Class GRT00009COM : Implements IDisposable
    Private CS0050Session As New CS0050SESSION
    Private CS0011LOGWrite As New CS0011LOGWrite

    ''' <summary>
    ''' 構造体マスタ検索用
    ''' </summary>
    Protected Const C_ATTENDANCE_ORG_STRUCT As String = "勤怠管理組織"

    ''' <summary>
    ''' 端末種別の取得
    ''' </summary>
    ''' <param name="I_TERMID">端末ID</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTermClass(ByVal I_TERMID As String) As String

        Dim WW_TermClass As String = ""

        '○ ユーザ
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String = _
                        " SELECT TERMCLASS                          " & _
                        " FROM S0001_TERM                           " & _
                        " WHERE TERMID        =  '" & I_TERMID & "' " & _
                        " AND   STYMD        <= getdate()           " & _
                        " AND   ENDYMD       >= getdate()           " & _
                        " AND   DELFLG       <> '1'                 "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        WW_TermClass = SQLdr("TERMCLASS")
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using

            End Using

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "GetTermClass"                 'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0001_TERM Select"             '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return WW_TermClass
        End Try
        Return WW_TermClass

    End Function

    ''' <summary>
    ''' 従業員番号取得処理
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_USERID" >ユーザID</param>
    ''' <param name="O_ORG">取得した従業員の所属部署</param>
    ''' <param name="O_STAFFCODE">取得した従業員のコード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Sub USERtoSTAFF(ByVal I_COMPCODE As String, ByVal I_USERID As String, ByRef O_ORG As String, ByRef O_STAFFCODE As String, ByRef O_RTN As String)

        O_ORG = String.Empty
        O_STAFFCODE = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            '○　従業員ListBox設定                
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                Dim SQLStr As String = ""

                SQLcon.Open() 'DataBase接続(Open)
                '検索SQL文
                SQLStr = _
                     "SELECT isnull(rtrim(B.CODE),'')      as ORG       " _
                   & "      ,isnull(rtrim(A.STAFFCODE),'') as STAFFCODE " _
                   & " FROM       S0004_USER   A                        " _
                   & " INNER JOIN M0006_STRUCT B                        " _
                   & "    ON   B.CAMPCODE     = @COMPCODE               " _
                   & "   and   B.OBJECT       = @OBJECT                 " _
                   & "   and   B.STRUCT       = @STRUCT                 " _
                   & "   and   B.GRCODE01     = A.ORG                   " _
                   & "   and   B.STYMD       <= @P2                     " _
                   & "   and   B.ENDYMD      >= @P2                     " _
                   & "   and   B.DELFLG      <> '1'                     " _
                   & " WHERE   A.USERID       = @USERID                 " _
                   & "   and   A.STYMD       <= @ENDYMD                 " _
                   & "   and   A.ENDYMD      >= @STYMD                  " _
                   & "   and   A.DELFLG      <> '1'                     "

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
                    Dim P_USERID As SqlParameter = SQLcmd.Parameters.Add("@USERID", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STRUCT As SqlParameter = SQLcmd.Parameters.Add("@STRUCT", System.Data.SqlDbType.NVarChar, 20)

                    P_USERID.Value = I_USERID
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_COMPCODE.Value = I_COMPCODE
                    P_ENDYMD.Value = Date.Now
                    P_OBJECT.Value = C_ROLE_VARIANT.USER_ORG
                    P_STRUCT.Value = C_ATTENDANCE_ORG_STRUCT
                    SQLcmd.CommandTimeout = 300

                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        O_ORG = SQLdr("ORG")
                        O_STAFFCODE = SQLdr("STAFFCODE")
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0004_USER Select"             '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 部署コード変換
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">変換元部署コード</param>
    ''' <param name="O_ORGCODE">変換後部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub ConvORGCODE(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_ORGCODE As String, ByRef O_RTN As String)

        O_ORGCODE = I_ORGCODE
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String = _
                      " select CODE                                " _
                    & " from   M0006_STRUCT    M06                 " _
                    & " where  M06.CAMPCODE     = @COMPCODE        " _
                    & "   and  M06.OBJECT       = @OBJECT          " _
                    & "   and  M06.STRUCT       = @STRUCT          " _
                    & "   and  M06.GRCODE01     = @ORGCODE         " _
                    & "   and  M06.STYMD       <= @ENDYMD          " _
                    & "   and  M06.ENDYMD      >= @STYMD           " _
                    & "   and  M06.DELFLG      <> '1'              "
                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)

                    Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STRUCT As SqlParameter = SQLcmd.Parameters.Add("@STRUCT", System.Data.SqlDbType.NVarChar, 20)
                    P_COMPCODE.Value = I_COMPCODE
                    P_ORGCODE.Value = I_ORGCODE
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_OBJECT.Value = C_ROLE_VARIANT.USER_ORG
                    P_STRUCT.Value = C_ATTENDANCE_ORG_STRUCT
                    SQLcmd.CommandTimeout = 300
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        O_ORGCODE = SQLdr("CODE")
                    End While

                    SQLdr.Dispose()
                    SQLdr = Nothing
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "GRT0009COM"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:M0006_STRUCT Select"           '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

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

''' <summary>
''' 時間調整用クラス
''' </summary>
''' <remarks></remarks>
Public Class GRT00009TIMEFORMAT : Implements IDisposable
    ''' <summary>
    ''' 丸め処理の区分
    ''' </summary>
    Public Enum EM_ROUND_TYPE As Byte
        ''' <summary>
        ''' 切下
        ''' </summary>
        FLOOR
        ''' <summary>
        ''' 切上
        ''' </summary>
        CEILING
        ''' <summary>
        ''' 五捨六入
        ''' </summary>
        ROUND
        ''' <summary>
        ''' 四捨五入
        ''' </summary>
        ROUND_AWAY
    End Enum
    ''' <summary>
    ''' 時間丸め処理
    ''' </summary>
    ''' <param name="I_PARAM">丸め対象の時間（HH:MM)</param>
    ''' <param name="I_SPLIT_MINUITE">丸め単位時刻（MM)</param>
    ''' <param name="I_ROUND_TYPE">丸め方法</param>
    ''' <returns>丸めた結果時間（HH:MM)</returns>
    ''' <remarks></remarks>
    Public Function RoundMinute(ByVal I_PARAM As String, ByVal I_SPLIT_MINUITE As Integer, ByVal I_ROUND_TYPE As EM_ROUND_TYPE) As String
        Dim WW_MINUITE As Integer = HHMMtoMinutes(I_PARAM)
        If WW_MINUITE Mod I_SPLIT_MINUITE = 0 Then Return I_PARAM

        Select Case I_ROUND_TYPE
            Case EM_ROUND_TYPE.FLOOR
                WW_MINUITE = Math.Floor(WW_MINUITE / I_SPLIT_MINUITE) * I_SPLIT_MINUITE
            Case EM_ROUND_TYPE.CEILING
                WW_MINUITE = Math.Ceiling(WW_MINUITE / I_SPLIT_MINUITE) * I_SPLIT_MINUITE
            Case EM_ROUND_TYPE.ROUND
                WW_MINUITE = Math.Round(WW_MINUITE / I_SPLIT_MINUITE) * I_SPLIT_MINUITE
            Case EM_ROUND_TYPE.ROUND_AWAY
                WW_MINUITE = Math.Round(WW_MINUITE / I_SPLIT_MINUITE, MidpointRounding.AwayFromZero) * I_SPLIT_MINUITE
        End Select
        Return MinutestoHHMM(WW_MINUITE)
    End Function

    ''' <summary>
    ''' 時間変換（分→時:分）
    ''' </summary>
    ''' <param name="I_PARAM">変換対象時刻（MINUITE）</param>
    ''' <returns>変換後時刻（HH:MM)</returns>
    ''' <remarks></remarks>
    Function MinutesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        WW_HHMM = Int(I_PARAM / 60) * 100 + I_PARAM Mod 60
        Return Format(WW_HHMM, "0#:##")
    End Function

    ''' <summary>
    ''' 変換（0 or 00:00をスペースへ
    ''' </summary>
    ''' <param name="I_PARAM"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function ZeroToSpace(ByVal I_PARAM As String) As String
        Dim WW_TIME As String() = I_PARAM.Split(":")
        If WW_TIME.Count > 1 Then
            If I_PARAM = "00:00" Then Return ""
        Else
            If Val(I_PARAM) = 0 Then Return ""
        End If
        Return I_PARAM
    End Function

    ''' <summary>
    ''' 変換（時：分→分）
    ''' </summary>
    ''' <param name="I_PARAM">変換元</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function HHMMToMinutes(ByVal I_PARAM As String) As Integer
        If String.IsNullOrEmpty(I_PARAM) Then
            Return 0
        Else
            Dim WW_HOUR As String
            Dim WW_MINUITE As String
            If I_PARAM.Contains(":") Then
                WW_HOUR = I_PARAM.Split(":")(0)
                WW_MINUITE = I_PARAM.Split(":")(1)
            Else
                WW_HOUR = I_PARAM.PadLeft(4, "0").Substring(0, 2)
                WW_MINUITE = I_PARAM.PadLeft(4, "0").Substring(2, 2)
            End If
            Return Val(WW_HOUR) * 60 + Val(WW_MINUITE)
        End If

    End Function
    ''' <summary>
    ''' 変換（時：分→時：分）
    ''' </summary>
    ''' <param name="I_PARAM">変換元</param>
    ''' <returns></returns>
    ''' <remarks>非推奨　下位互換のために存在</remarks>
    Function FormatHHMM(ByVal I_PARAM As String) As String
        Return MinutesToHHMM(HHMMToMinutes(I_PARAM))

    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 重複する呼び出しを検出するには

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
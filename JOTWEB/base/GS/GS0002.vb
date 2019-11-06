Imports System.Data.SqlClient

''' <summary>
''' グループコードサマリー
''' </summary>
''' <remarks></remarks>
Public Class GS0002GRCODEsum
    Inherits GS0000
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRLIST() As List(Of String)
    ''' <summary>
    ''' グループ名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRNMLIST() As List(Of String)
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCAMPLIST() As List(Of String)
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRUSERLIST() As List(Of String)
    ''' <summary>
    ''' オブジェクト
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GROBJLIST() As List(Of String)
    ''' <summary>
    ''' 構造名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRSTRLIST() As List(Of String)
    ''' <summary>
    ''' 有効期限（開始年月日）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRSYMDLIST() As List(Of Date)
    ''' <summary>
    ''' 有効期限（終了）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GREYMDLIST() As List(Of Date)
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE01() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE02() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE03() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE04() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE05() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE06() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE07() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE08() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE09() As String
    ''' <summary>
    ''' グループコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GRCODE10() As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' オブジェクトコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OBJCODE() As String
    ''' <summary>
    ''' 構造名
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STRUCT() As String
    ''' <summary>
    ''' 有効期限（開始年月日）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date
    ''' <summary>
    ''' 有効期限（終了）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date

    ''' <summary>
    ''' グループコードまとめ処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0002GRCODEsum()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー))

        '●グループコードサマリー
        Dim WW_GRCODE_CNT As Integer = 0
        If GRCODE01 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE01)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE01 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE01)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE02 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE02)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE02 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE02)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE03 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE03)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE03 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE03)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE04 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE04)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE04 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE04)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE05 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE05)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE05 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE05)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE06 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE06)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE06 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE06)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE07 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE07)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE07 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE07)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE08 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE08)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE08 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE08)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE09 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE09)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE09 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE09)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If

        WW_GRCODE_CNT = 0
        If GRCODE10 <> "" Then
            Do
                If GRLIST.Count = 0 Then
                    GRLIST.Add(GRCODE10)
                    GRNMLIST.Add("")
                    GRUSERLIST.Add(USERID)
                    GRCAMPLIST.Add(CAMPCODE)
                    GROBJLIST.Add(OBJCODE)
                    GRSTRLIST.Add(STRUCT)
                    GRSYMDLIST.Add(STYMD)
                    GREYMDLIST.Add(ENDYMD)

                    Exit Do
                Else
                    If GRLIST(WW_GRCODE_CNT) = GRCODE10 Then
                        Exit Do
                    Else
                        If WW_GRCODE_CNT >= (GRLIST.Count - 1) Then
                            GRLIST.Add(GRCODE10)
                            GRNMLIST.Add("")
                            GRUSERLIST.Add(USERID)
                            GRCAMPLIST.Add(CAMPCODE)
                            GROBJLIST.Add(OBJCODE)
                            GRSTRLIST.Add(STRUCT)
                            GRSYMDLIST.Add(STYMD)
                            GREYMDLIST.Add(ENDYMD)

                            Exit Do
                        End If
                    End If
                End If
                WW_GRCODE_CNT = WW_GRCODE_CNT + 1
            Loop Until WW_GRCODE_CNT >= GRLIST.Count - 1
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●グループ名称取得
        '○ DB(M0007_GROUP)検索
        For i As Integer = 0 To GRLIST.Count - 1
            If GRNMLIST(i) = "" Then
                Try
                    'DataBase接続文字
                    Dim SQLcon = sm.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    'CAMPCODE検索SQL文
                    Dim SQL_Str As String
                    If GRCAMPLIST(i) = "" Then
                        SQL_Str = _
                                "SELECT rtrim(NODENAMES) as NODENAMES " _
                            & " FROM  M0007_GROUP " _
                            & " Where USERID   = @P1 " _
                            & "   and OBJECT   = @P3 " _
                            & "   and STRUCT   = @P4 " _
                            & "   and GRCODE   = @P5 " _
                            & "   and STYMD <= @P6 " _
                            & "   and ENDYMD >= @P7 " _
                            & "   and DELFLG <> @P8 "
                    Else
                        SQL_Str = _
                                "SELECT rtrim(NODENAMES) as NODENAMES " _
                            & " FROM  M0007_GROUP " _
                            & " Where USERID   = @P1 " _
                            & "   and CAMPCODE = @P2 " _
                            & "   and OBJECT   = @P3 " _
                            & "   and STRUCT   = @P4 " _
                            & "   and GRCODE   = @P5 " _
                            & "   and STYMD <= @P6 " _
                            & "   and ENDYMD >= @P7 " _
                            & "   and DELFLG <> @P8 "
                    End If
                    Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    If GRCAMPLIST(i) = "" Then
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Char, 20)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                        Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                        Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Char, 1)
                        PARA1.Value = GRUSERLIST(i)
                        PARA3.Value = GROBJLIST(i)
                        PARA4.Value = GRSTRLIST(i)
                        PARA5.Value = GRLIST(i)
                        PARA6.Value = GRSYMDLIST(i)
                        PARA7.Value = GREYMDLIST(i)
                        PARA8.Value = "1"
                    Else
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Char, 20)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 20)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 20)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Char, 20)
                        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                        Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.Date)
                        Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.Char, 1)
                        PARA1.Value = GRUSERLIST(i)
                        PARA2.Value = GRCAMPLIST(i)
                        PARA3.Value = GROBJLIST(i)
                        PARA4.Value = GRSTRLIST(i)
                        PARA5.Value = GRLIST(i)
                        PARA6.Value = GRSYMDLIST(i)
                        PARA7.Value = GREYMDLIST(i)
                        PARA8.Value = "1"
                    End If
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        GRNMLIST(i) = SQLdr("NODENAMES")
                        Exit While
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                    SQLcmd.Dispose()
                    SQLcmd = Nothing

                    SQLcon.Close() 'DataBase接続(Close)
                    SQLcon.Dispose()
                    SQLcon = Nothing

                Catch ex As Exception
                    Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                    CS0011LOGWRITE.INFSUBCLASS = "GS0002GRCODEsum"              'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:M0007_GROUP Select"            '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                    ERR = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
            End If

        Next

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class

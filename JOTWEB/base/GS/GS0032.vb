Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' LeftBox 固定値リスト取得
''' </summary>
''' <remarks></remarks>
Public Class GS0032FIXVALUElst
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' クラスコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STDATE As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDDATE As Date
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE1() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE2() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE3() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE4() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE5() As ListBox
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX1() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX2() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX3() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX4() As Object
    ''' <summary>
    ''' 固定値一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX5() As Object

    Protected METHOD_NAME As String = "GS0032FIXVALUElst"
    ''' <summary>
    ''' Leftbox固定値一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0032FIXVALUElst()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'PARAM03: STDATE
        If checkParam(METHOD_NAME, STDATE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'PARAM04: ENDDATE
        If checkParam(METHOD_NAME, ENDDATE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        '●初期処理
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        VALUE1 = New ListBox
        VALUE2 = New ListBox
        VALUE3 = New ListBox
        VALUE4 = New ListBox
        VALUE5 = New ListBox
        Try
            If IsNothing(LISTBOX1) Then
                LISTBOX1 = New ListBox
            Else
                DirectCast(LISTBOX1, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX2) Then
                LISTBOX2 = New ListBox
            Else
                DirectCast(LISTBOX2, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX3) Then
                LISTBOX3 = New ListBox
            Else
                DirectCast(LISTBOX3, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX4) Then
                LISTBOX4 = New ListBox
            Else
                DirectCast(LISTBOX4, ListBox).Items.Clear()
            End If

            If IsNothing(LISTBOX5) Then
                LISTBOX5 = New ListBox
            Else
                DirectCast(LISTBOX5, ListBox).Items.Clear()
            End If

        Catch ex As Exception
        End Try

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●固定値リスト取得(指定値)
        '○ DB(OIS0015_FIXVALUE)検索
        Try
            'S0011_UPROFXLS検索SQL文
            Dim SQL_Str As String =
                    "SELECT rtrim(KEYCODE) as KEYCODE , rtrim(VALUE1) as VALUE1 , rtrim(VALUE2) as VALUE2 , rtrim(VALUE3) as VALUE3 , rtrim(VALUE4) as VALUE4 , rtrim(VALUE5) as VALUE5 " _
                & " FROM  COM.OIS0015_FIXVALUE " _
                & " Where CAMPCODE  = @P1 " _
                & "   and CLASS     = @P2 " _
                & "   and STYMD    <= @P3 " _
                & "   and ENDYMD   >= @P4 " _
                & "   and DELFLG   <> @P5 " _
                & " ORDER BY KEYCODE "

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P2", SqlDbType.NVarChar, 20).Value = CLAS
                    .Add("@P3", SqlDbType.Date).Value = ENDDATE
                    .Add("@P4", SqlDbType.Date).Value = STDATE
                    .Add("@P5", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    Dim keyCode As String = ""
                    Dim val(5) As String
                    Dim listboxObjType As New List(Of ListBox) From {Nothing, DirectCast(LISTBOX1, ListBox), DirectCast(LISTBOX2, ListBox),
                                                                     DirectCast(LISTBOX3, ListBox), DirectCast(LISTBOX4, ListBox), DirectCast(LISTBOX5, ListBox)}
                    While SQLdr.Read
                        keyCode = Convert.ToString(SQLdr("KEYCODE"))
                        If keyCode <> "" Then
                            For i = 1 To 5
                                val(i) = Convert.ToString(SQLdr("VALUE" & i.ToString))
                            Next
                            VALUE1.Items.Add(New ListItem(val(1), keyCode))
                            VALUE2.Items.Add(New ListItem(val(2), keyCode))
                            VALUE3.Items.Add(New ListItem(val(3), keyCode))
                            VALUE4.Items.Add(New ListItem(val(4), keyCode))
                            VALUE5.Items.Add(New ListItem(val(5), keyCode))

                            listboxObjType(1).Items.Add(New ListItem(val(1), keyCode))
                            listboxObjType(2).Items.Add(New ListItem(val(2), keyCode))
                            listboxObjType(3).Items.Add(New ListItem(val(3), keyCode))
                            listboxObjType(4).Items.Add(New ListItem(val(4), keyCode))
                            listboxObjType(5).Items.Add(New ListItem(val(5), keyCode))
                        End If
                    End While
                    ERR = C_MESSAGE_NO.NORMAL
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0015_FIXVALUE Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●固定値リスト取得(デフォルト値)
        '○ DB(OIS0015_FIXVALUE)検索
        If VALUE1.Items.Count = 0 Then
            Try
                'S0011_UPROFXLS検索SQL文
                Dim SQL_Str As String =
                        "SELECT rtrim(KEYCODE) as KEYCODE , rtrim(VALUE1) as VALUE1 , rtrim(VALUE2) as VALUE2 , rtrim(VALUE3) as VALUE3 , rtrim(VALUE4) as VALUE4 , rtrim(VALUE5) as VALUE5 " _
                    & " FROM  COM.OIS0015_FIXVALUE " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and CLASS     = @P2 " _
                    & "   and STYMD    <= @P3 " _
                    & "   and ENDYMD   >= @P4 " _
                    & "   and DELFLG   <> @P5 " _
                    & " ORDER BY KEYCODE "

                'DataBase接続文字
                Using SQLcon = sm.getConnection,
                      SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    With SQLcmd.Parameters
                        .Add("@P1", SqlDbType.NVarChar, 20).Value = C_DEFAULT_DATAKEY
                        .Add("@P2", SqlDbType.NVarChar, 20).Value = CLAS
                        .Add("@P3", SqlDbType.Date).Value = ENDDATE
                        .Add("@P4", SqlDbType.Date).Value = STDATE
                        .Add("@P5", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        Dim keyCode As String = ""
                        Dim val(5) As String
                        Dim listboxObjType As New List(Of ListBox) From {Nothing, DirectCast(LISTBOX1, ListBox), DirectCast(LISTBOX2, ListBox),
                                                                     DirectCast(LISTBOX3, ListBox), DirectCast(LISTBOX4, ListBox), DirectCast(LISTBOX5, ListBox)}
                        While SQLdr.Read
                            keyCode = Convert.ToString(SQLdr("KEYCODE"))
                            If keyCode <> "" Then
                                For i = 1 To 5
                                    val(i) = Convert.ToString(SQLdr("VALUE" & i.ToString))
                                Next

                                VALUE1.Items.Add(New ListItem(val(1), keyCode))
                                VALUE2.Items.Add(New ListItem(val(2), keyCode))
                                VALUE3.Items.Add(New ListItem(val(3), keyCode))
                                VALUE4.Items.Add(New ListItem(val(4), keyCode))
                                VALUE5.Items.Add(New ListItem(val(5), keyCode))

                                listboxObjType(1).Items.Add(New ListItem(val(1), keyCode))
                                listboxObjType(2).Items.Add(New ListItem(val(2), keyCode))
                                listboxObjType(3).Items.Add(New ListItem(val(3), keyCode))
                                listboxObjType(4).Items.Add(New ListItem(val(4), keyCode))
                                listboxObjType(5).Items.Add(New ListItem(val(5), keyCode))
                            End If
                        End While

                        ERR = C_MESSAGE_NO.NORMAL
                        'Close
                        SQLdr.Close() 'Reader(Close)
                    End Using
                    SQLcon.Close() 'DataBase接続(Close)
                End Using
            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:OIS0015_FIXVALUE Select"         '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If

    End Sub

End Class


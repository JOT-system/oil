Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' メッセージ取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0009MESSAGEout

    ''' <summary>
    ''' 取得するメッセージのNO
    ''' </summary>
    ''' <value>MESSAGENO</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MESSAGENO() As String

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAEIW() As String


    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?01)</remarks>
    Public Property PARA01() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?02)</remarks>
    Public Property PARA02() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?03)</remarks>
    Public Property PARA03() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?04)</remarks>
    Public Property PARA04() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?05)</remarks>
    Public Property PARA05() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?06)</remarks>
    Public Property PARA06() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?07)</remarks>
    Public Property PARA07() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?08)</remarks>
    Public Property PARA08() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?09)</remarks>
    Public Property PARA09() As String

    ''' <summary>
    ''' 埋込文字
    ''' </summary>
    ''' <value>埋込文字</value>
    ''' <returns></returns>
    ''' <remarks>Message埋込文字(?10)</remarks>
    Public Property PARA10() As String

    ''' <summary>
    ''' 表示するメッセージボックス
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MESSAGEBOX() As Label

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0009MESSAGEout"

    ''' <summary>
    ''' メッセージを取得し、ボックスに表示する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0009MESSAGEout()

        '●In PARAMチェック
        'PARAM01: MESSAGENO
        If IsNothing(MESSAGENO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MESSAGENO"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02:N(正常)/A(異常)/E(エラー)/W(警告)/I(インフォメーション)
        If IsNothing(NAEIW) Then
            NAEIW = ""
        End If
        Select Case NAEIW.ToUpper
            Case C_MESSAGE_TYPE.NOR, C_MESSAGE_TYPE.ABORT, C_MESSAGE_TYPE.ERR, C_MESSAGE_TYPE.WAR, C_MESSAGE_TYPE.INF
                Exit Select
            Case Else
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "NAEIW"                          '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
        End Select

        'PARAM03:Message埋込文字(?01)
        If IsNothing(PARA01) Then
            PARA01 = ""
        End If

        'PARAM04:Message埋込文字(?02)
        If IsNothing(PARA02) Then
            PARA02 = ""
        End If

        'PARAM05:Message埋込文字(?03)
        If IsNothing(PARA03) Then
            PARA03 = ""
        End If

        'PARAM06:Message埋込文字(?04)
        If IsNothing(PARA04) Then
            PARA04 = ""
        End If

        'PARAM07:Message埋込文字(?05)
        If IsNothing(PARA05) Then
            PARA05 = ""
        End If

        'PARAM08:Message埋込文字(?06)
        If IsNothing(PARA06) Then
            PARA06 = ""
        End If

        'PARAM09:Message埋込文字(?07)
        If IsNothing(PARA07) Then
            PARA07 = ""
        End If

        'PARAM10:Message埋込文字(?08)
        If IsNothing(PARA08) Then
            PARA08 = ""
        End If

        'PARAM11:Message埋込文字(?09)
        If IsNothing(PARA09) Then
            PARA09 = ""
        End If

        'PARAM12:Message埋込文字(?10)
        If IsNothing(PARA10) Then
            PARA10 = ""
        End If
        'セッション管理
        Dim sm As New CS0050SESSION
        '●メッセージ情報取得
        'メッセージ
        Dim W_Message As String

        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'Message検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(TEXT) as TEXT " _
               & " FROM  COM.OIS0003_MESSAGE " _
               & " Where ID= @P1 "
            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10).Value = MESSAGENO
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        W_Message = "  " & Convert.ToString(SQLdr("TEXT"))
                        W_Message = W_Message.Replace("?01", PARA01)
                        W_Message = W_Message.Replace("?02", PARA02)
                        W_Message = W_Message.Replace("?03", PARA03)
                        W_Message = W_Message.Replace("?04", PARA04)
                        W_Message = W_Message.Replace("?05", PARA05)
                        W_Message = W_Message.Replace("?06", PARA06)
                        W_Message = W_Message.Replace("?07", PARA07)
                        W_Message = W_Message.Replace("?08", PARA08)
                        W_Message = W_Message.Replace("?09", PARA09)
                        W_Message = W_Message.Replace("?10", PARA10)

                        MESSAGEBOX.Text = W_Message

                        Select Case NAEIW.ToUpper
                            Case C_MESSAGE_TYPE.NOR
                                MESSAGEBOX.ForeColor = Drawing.Color.Black 'black
                                MESSAGEBOX.Font.Bold = False
                            Case C_MESSAGE_TYPE.INF
                                MESSAGEBOX.ForeColor = Drawing.Color.DarkBlue 'darkblue
                                MESSAGEBOX.Font.Bold = True
                            Case C_MESSAGE_TYPE.WAR
                                MESSAGEBOX.ForeColor = Drawing.Color.DarkBlue 'darkblue
                                MESSAGEBOX.Font.Bold = True
                            Case C_MESSAGE_TYPE.ERR, C_MESSAGE_TYPE.ABORT
                                MESSAGEBOX.ForeColor = Drawing.Color.Red 'red
                                MESSAGEBOX.Font.Bold = True
                        End Select

                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using

            ERR = C_MESSAGE_NO.NORMAL
        Catch ex As Exception

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0003_MESSAGE Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            MESSAGEBOX.TEXT = "システム管理者へ連絡して下さい(DB OIS0003_MESSAGE Select ERR)"
            MESSAGEBOX.ForeColor = Drawing.Color.Red 'red
            MESSAGEBOX.BackColor = Drawing.Color.DarkSalmon 'darksalmon
            MESSAGEBOX.Font.Bold = True
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure



Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' ログ出力
''' </summary>
''' <remarks></remarks>
Public Structure CS0011LOGWrite

    ''' <summary>
    ''' SubCLASS(問題発生場所)
    ''' </summary>
    ''' <value>SUBCLASS</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property INFSUBCLASS As String

    ''' <summary>
    ''' Position(問題発生場所)
    ''' </summary>
    ''' <value>Position</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property INFPOSI As String

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    ''' <value>メッセージタイプ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NIWEA As String

    ''' <summary>
    ''' MessageTEXT
    ''' </summary>
    ''' <value>メッセージ文字列</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TEXT As String

    ''' <summary>
    ''' MESSAGENO
    ''' </summary>
    ''' <value>MESSAGENO</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MESSAGENO As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(パラメータERR),ERR:00003(DB err),ERR:00004(File io err)</remarks>
    Public Property ERR As String

    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0011LOGWrite"

    ''' <summary>
    ''' ログにメッセージを出力する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0011LOGWrite()

        ERR = C_MESSAGE_NO.NORMAL

        '●In PARAMチェック
        'PARAM01: SUBCLASS(問題発生場所)
        If IsNothing(INFSUBCLASS) Then
            INFSUBCLASS = ""
        End If
        If INFSUBCLASS = "" Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: POSITION(問題発生場所),任意入力情報
        If IsNothing(INFPOSI) Then
            INFPOSI = ""
        End If

        'PARAM03:N(正常)/A(異常)/E(エラー)/W(警告)/I(インフォメーション)
        If IsNothing(NIWEA) Then
            NIWEA = ""
        Else
            Select Case NIWEA.ToUpper
                Case C_MESSAGE_TYPE.ABORT, C_MESSAGE_TYPE.ERR, C_MESSAGE_TYPE.WAR, C_MESSAGE_TYPE.INF, C_MESSAGE_TYPE.NOR
                    Exit Select
                Case Else
                    ERR = C_MESSAGE_NO.DLL_IF_ERROR
                    Exit Sub
            End Select
        End If

        'PARAM04: MessageTEXT
        If IsNothing(TEXT) Then
            TEXT = ""
        End If
        If TEXT = "" Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM05: MESSAG6NO
        '●エラーログ出力判定
        'ERRLog出力判定SW
        Dim W_OUTPUTSW As String = ""
        Dim sm As New CS0050SESSION

        Try
            Dim SQLstr_LOGCNTL As String = "SELECT A , E , W , I , N " _
                                          & " FROM  COM.OIS0002_LOGCNTL " _
                                          & " Where stymd  <= @P1 " _
                                          & "   and endymd >= @P2 " _
                                          & "   and DELFLG <> @P3 "
            'DataBase接続
            '*共通関数
            'OIS0002_LOGCNTL検索SQL文
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQLstr_LOGCNTL, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                SqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.Date).Value = Date.Now
                    .Add("@P2", SqlDbType.Date).Value = Date.Now
                    .Add("@P3", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Select Case NIWEA.ToUpper
                            Case C_MESSAGE_TYPE.ABORT  '異常(DataBase以外のERRLog出力)
                                W_OUTPUTSW = Convert.ToString(SQLdr(C_MESSAGE_TYPE.ABORT))
                            Case C_MESSAGE_TYPE.ERR  'エラー(ファイル出力等)
                                W_OUTPUTSW = Convert.ToString(SQLdr(C_MESSAGE_TYPE.ERR))
                            Case C_MESSAGE_TYPE.WAR   '警告()
                                W_OUTPUTSW = Convert.ToString(SQLdr(C_MESSAGE_TYPE.WAR))
                            Case C_MESSAGE_TYPE.INF  'インフォメーション(トランザクション処理の開始・終了)
                                W_OUTPUTSW = Convert.ToString(SQLdr(C_MESSAGE_TYPE.INF))
                            Case C_MESSAGE_TYPE.NOR  '正常終了(DataBase更新)
                                W_OUTPUTSW = Convert.ToString(SQLdr(C_MESSAGE_TYPE.NOR))
                        End Select
                    End While
                    SQLdr.Close()
                End Using

            End Using 'SQLcon, SQLcmd

        Catch ex As Exception
            'エラーログのエラーは処理できない
            W_OUTPUTSW = "1"
            '  ERR = C_MESSAGE_NO.DB_ERROR  'DB ERR
            ERR = "99999"  'DB ERR
        End Try

        '●エラーログ出力
        If W_OUTPUTSW = "1" Then
            Try
                'ＥＲＲＬｏｇディレクトリ＆ファイル名作成
                Dim W_LOGDIR As String
                '                W_LOGDIR = sm.LOG_PATH & "\ONLINE\"

                W_LOGDIR = "\OIL\" & "\ONLINE\"
                W_LOGDIR = W_LOGDIR & sm.TERM_COMPANY & "-"
                W_LOGDIR = W_LOGDIR & sm.TERM_ORG & "-"
                W_LOGDIR = W_LOGDIR & DateTime.Now.ToString("yyyyMMddHHmmss")
                W_LOGDIR = W_LOGDIR & DateTime.Now.Millisecond & "-"
                W_LOGDIR = W_LOGDIR & NIWEA & MESSAGENO & ".txt"
                Using ERRLog As New System.IO.StreamWriter(W_LOGDIR, True, System.Text.Encoding.UTF8)
                    'ＥＲＲＬｏｇ出力
                    Dim W_ERRTEXT As String
                    W_ERRTEXT = "DATETIME = " & DateTime.Now.ToString & " , "
                    W_ERRTEXT = W_ERRTEXT & "Term = " & sm.TERMID & " , "
                    W_ERRTEXT = W_ERRTEXT & "Camp = " & sm.TERM_COMPANY & " , "
                    W_ERRTEXT = W_ERRTEXT & "Userid = " & sm.USERID & " , "
                    W_ERRTEXT = W_ERRTEXT & "SubClass = " & INFSUBCLASS & " , "
                    W_ERRTEXT = W_ERRTEXT & "Position = " & INFPOSI & " , "
                    W_ERRTEXT = W_ERRTEXT & "MESSAGENO = " & MESSAGENO & " , "
                    W_ERRTEXT = W_ERRTEXT & "TEXT = " & TEXT
                    'スタックトレース追加
                    W_ERRTEXT = W_ERRTEXT & Environment.NewLine & Environment.NewLine & Environment.StackTrace
                    ERRLog.Write(W_ERRTEXT)
                End Using
                '全体
            Catch ex As System.SystemException
                ERR = C_MESSAGE_NO.FILE_IO_ERROR 'IO ERR
                Exit Sub

            End Try

        End If

    End Sub
End Structure

Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' 画面メモ情報取得
''' </summary>
''' <remarks></remarks>
Public Class GS0003MEMOget
    Inherits GS0000

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID() As String
    ''' <summary>
    ''' メモ情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MEMO() As String

    Public Const METHOD_NAME As String = "GS0003MEMOget"
    Public Sub GS0003MEMOget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: MAPID
        If checkParam(METHOD_NAME, MAPID) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        ' PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If

        '●画面メモ情報取得
        '○ DB(OIS0000_MEMO)検索
        Try
            'OIS0000_MEMO検索SQL文
            Dim SQL_Str As String =
                    "SELECT rtrim(MEMO) as MEMO " _
                & " FROM  COM.OIS0000_MEMO " _
                & " Where USERID   = @P1 " _
                & "   and MAPID    = @P2 " _
                & "   and DELFLG  <> @P3 "

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)

                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = USERID
                    .Add("@P2", SqlDbType.NVarChar, 50).Value = MAPID
                    .Add("@P3", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    ERR = C_MESSAGE_NO.DLL_IF_ERROR
                    '存在したら一行取得
                    If SQLdr.Read Then
                        MEMO = Convert.ToString(SQLdr("MEMO"))
                        ERR = C_MESSAGE_NO.NORMAL
                    End If
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using 'SQLdr
            End Using 'SQLcon, SQLcmd

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "GS0003MEMOget"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0000_MEMO Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


        '○ メモ情報なしの場合、空データ追加
        If ERR = C_MESSAGE_NO.DLL_IF_ERROR Then
            Try
                'OIS0000_MEMO追加SQL文
                Dim SQL_Str As String =
                        "INSERT " _
                    & " INTO  COM.OIS0000_MEMO " _
                    & "     ( USERID  , " _
                    & "       MAPID   , " _
                    & "       MEMO    , " _
                    & "       DELFLG  , " _
                    & "       INITYMD , " _
                    & "       UPDYMD  , " _
                    & "       UPDUSER ) " _
                    & " VALUES " _
                    & "     ( @P1 , " _
                    & "       @P2 , " _
                    & "       @P3 , " _
                    & "       @P4 , " _
                    & "       @P5 , " _
                    & "       @P6 , " _
                    & "       @P7 ) "

                'DataBase接続文字
                Using SQLcon = sm.getConnection,
                      SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)

                    With SQLcmd.Parameters
                        .Add("@P1", SqlDbType.NVarChar, 20).Value = USERID
                        .Add("@P2", SqlDbType.NVarChar, 50).Value = MAPID
                        .Add("@P3", SqlDbType.NVarChar, 500).Value = ""
                        .Add("@P4", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.ALIVE
                        .Add("@P5", SqlDbType.Date).Value = Date.Now
                        .Add("@P6", SqlDbType.Date).Value = Date.Now
                        .Add("@P7", SqlDbType.NVarChar, 20).Value = USERID
                    End With

                    SQLcmd.ExecuteNonQuery()

                    SQLcon.Close() 'DataBase接続(Close)

                    MEMO = ""
                    ERR = C_MESSAGE_NO.NORMAL
                End Using
            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:OIS0000_MEMO Insert"
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

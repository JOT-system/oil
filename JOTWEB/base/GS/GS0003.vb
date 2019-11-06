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
        If checkParam(METHOD_NAME, MAPID) Then
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
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'OIS0000_MEMO検索SQL文
            Dim SQL_Str As String =
                    "SELECT rtrim(MEMO) as MEMO " _
                & " FROM  COM.OIS0000_MEMO " _
                & " Where USERID   = @P1 " _
                & "   and MAPID    = @P2 " _
                & "   and DELFLG  <> @P3 "
            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = USERID
            PARA2.Value = MAPID
            PARA3.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            '存在したら一行取得
            If SQLdr.Read Then
                MEMO = SQLdr("MEMO")
                ERR = C_MESSAGE_NO.NORMAL
            End If

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
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

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
                Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 500)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = USERID
                PARA2.Value = MAPID
                PARA3.Value = ""
                PARA4.Value = C_DELETE_FLG.ALIVE
                PARA5.Value = Date.Now
                PARA6.Value = Date.Now
                PARA7.Value = USERID
                SQLcmd.ExecuteNonQuery()
                SQLcmd.Dispose()

                SQLcon.Close() 'DataBase接続(Close)
                SQLcon.Dispose()
                SQLcon = Nothing

                MEMO = ""
                ERR = C_MESSAGE_NO.NORMAL

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

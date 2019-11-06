Imports System.Data.SqlClient

''' <summary>
''' 画面メモ情報更新
''' </summary>
''' <remarks></remarks>
Public Class GS0004MEMOset
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
    ''' 端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String
    ''' <summary>
    ''' メモ情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MEMO() As String

    ''' <summary>
    ''' 実行名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "GS0004MEMOset"
    ''' <summary>
    ''' メモ欄更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0004MEMOset()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: MAPID
        If checkParam(METHOD_NAME, MAPID) Then
            Exit Sub
        End If
        'PARAM02: MEMO
        If checkParam(METHOD_NAME, _MEMO) Then
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01: USERID
        If IsNothing(USERID) Then
            USERID = sm.USERID
        End If
        'PARAM EXTRA02: TERMID
        If IsNothing(TERMID) Then
            TERMID = sm.APSV_ID
        End If


        '●画面メモ情報更新
        '○ DB(T0002_MEMO)更新
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'T0002_MEMO更新SQL文
            Dim SQLStr As String =
                    " DECLARE @hensuu as bigint ;                                                                    " _
                         & " set @hensuu = 0 ;                                                                       " _
                         & " DECLARE hensuu CURSOR FOR                                                               " _
                         & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                                            " _
                         & "     FROM    T0002_MEMO                                                                  " _
                         & "     WHERE USERID =@P2                                                                   " _
                         & "       and MAPID = @P3 ;                                                                 " _
                         & " OPEN hensuu ;                                                                                  " _
                         & " FETCH NEXT FROM hensuu INTO @hensuu ;                                                          " _
                         & " IF ( @@FETCH_STATUS = 0 )                                                                      " _
                         & "    UPDATE   T0002_MEMO                                                                         " _
                         & "       SET                                                                                      " _
                         & "         MEMO       = @P1 ,                                                                     " _
                         & "         UPDYMD     = @P4 ,                                                                     " _
                         & "         UPDUSER    = @P5 ,                                                                     " _
                         & "         UPDTERMID  = @P6 ,                                                                     " _
                         & "         RECEIVEYMD = @P7                                                                       " _
                         & "     WHERE                                                                                      " _
                         & "            USERID     = @P2                                                                    " _
                         & "       And  MAPID      = @P3                                                                    " _
                         & " IF ( @@FETCH_STATUS <> 0 )                                                                     " _
                         & "    INSERT INTO T0002_MEMO                                                                      " _
                         & "       (USERID , MAPID , MEMO, DELFLG  ,                                                        " _
                         & "        INITYMD , UPDYMD , UPDUSER , UPDTERMID , RECEIVEYMD)                                    " _
                         & "        VALUES (@P2,@P3,@P1,@P8,                                                                " _
                         & "        @P4,@P4,@P5,@P6,@P7) ;                                                                  " _
                         & " CLOSE hensuu ;                                                                                 " _
                         & " DEALLOCATE hensuu ; "
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 500)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.DateTime)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.NVarChar, 30)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", System.Data.SqlDbType.DateTime)
            Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", System.Data.SqlDbType.NVarChar, 1)
            PARA1.Value = MEMO
            PARA2.Value = USERID
            PARA3.Value = MAPID
            PARA4.Value = Date.Now
            PARA5.Value = USERID
            PARA6.Value = TERMID
            PARA7.Value = C_DEFAULT_YMD
            PARA8.Value = C_DELETE_FLG.ALIVE
            SQLcmd.ExecuteNonQuery()
            SQLcmd.Dispose()

            ERR = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0002_MEMO Update"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class

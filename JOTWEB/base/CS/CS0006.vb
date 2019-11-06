Imports System.Data.SqlClient


''' <summary>
''' コンピュータ名存在チェック
''' </summary>
''' <remarks>指定された端末名がDBに登録されているか確認する</remarks>
Public Structure CS0006TERMchk

    ''' <summary>
    ''' コンピュータ名
    ''' </summary>
    ''' <value>確認するコンピュータ名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID() As String

    ''' <summary>
    ''' 端末設置会社
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns>端末設置場所の会社コード</returns>
    ''' <remarks></remarks>
    Public Property TERMCAMP() As String

    ''' <summary>
    ''' 端末設置部署
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>端末設置場所の部署コード</returns>
    ''' <remarks></remarks>
    Public Property TERMORG() As String

    ''' <summary>
    ''' 管理部署
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns>端末管理の部署コード</returns>
    ''' <remarks></remarks>
    Public Property MORG() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(Customize),ERR:00003(DBerr),ERR:00005(TERM err)</remarks>
    Public Property ERR() As String


    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0006TERMchk"

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0006TERMchk()
        '●In PARAMチェック
        'PARAM01:コンピュータ名
        If IsNothing(TERMID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TERMID"                            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT             'メッセージタイプ
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                            'ログ出力
            Exit Sub
        End If
        'セッション管理
        Dim sm As New CS0050SESSION
        '****************
        '*** 共通宣言 ***
        '****************
        'DataBase接続文字
        Using SQLcon = sm.getConnection
            '●コンピュータ名の有効チェック
            Try


                SQLcon.Open() 'DataBase接続(Open)

                Dim WW_CNT As Integer = 0
                TERMCAMP = ""
                TERMORG = ""

                'Message検索SQL文
                Dim SQLStr As String =
                     "SELECT rtrim(TERMID) as TERMID , rtrim(TERMCAMP) as TERMCAMP , rtrim(TERMORG) as TERMORG , rtrim(MORG) as MORG" _
                   & " FROM  com.OIS0001_TERM " _
                   & " Where TERMID = @P1 " _
                   & "   and STYMD <= @P2 " _
                   & "   and ENDYMD >= @P3 " _
                   & "   and DELFLG <> @P4 "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                    PARA1.Value = TERMID
                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        TERMCAMP = SQLdr("TERMCAMP")
                        TERMORG = SQLdr("TERMORG")
                        MORG = SQLdr("MORG")
                        WW_CNT = 1
                    End If

                    If WW_CNT = 0 Then
                        ERR = C_MESSAGE_NO.SYSTEM_CANNOT_WAKEUP
                    Else
                        ERR = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing
                End Using
            Catch ex As Exception

                Dim CS0011LOGWrite As New CS0011LOGWrite                    'LogOutput DirString Get
                CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR

                Exit Sub

            End Try

        End Using

    End Sub

End Structure

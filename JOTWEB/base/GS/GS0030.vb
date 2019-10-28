Imports System.Data.SqlClient


''' <summary>
''' 売上計上区分デフォルト取得
''' </summary>
''' <remarks>受注配車用</remarks>
Public Class GS0030URIKBNget
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' 取引先コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 取引先売上計上区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URIKBN() As String

    Protected METHOD_NAME As String = "GS0030URIKBNget"
    ''' <summary>
    ''' 売上計上区分デフォルト取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0030URIKBNget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        URIKBN = ""
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01:ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = sm.APSV_ORG
        End If
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Try
            '●売上計上区分デフォルト取得取得（APSRVOrg）
            '○ セッション変数（APSRVOrg）に紐付く荷主データ取得
            '検索SQL文
            Dim SQLStr As String = _
                    "   SELECT rtrim(A.URIKBN) 	as URIKBN 		    " _
                & "   FROM   MC003_TORIORG      as A 			    " _
                & "   Where        A.CAMPCODE    = @P1 				" _
                & "            and A.UORG        = @P2          	" _
                & "            and A.TORICODE    = @P3 				" _
                & "            and A.DELFLG     <> '1' 				"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = CAMPCODE
            PARA2.Value = ORGCODE
            PARA3.Value = TORICODE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
            '○出力編集
            If SQLdr.Read Then
                URIKBN = SQLdr("URIKBN")
            End If

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            ERR = C_MESSAGE_NO.NORMAL
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC003_TORIORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class

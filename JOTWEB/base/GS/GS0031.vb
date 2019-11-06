Imports System.Data.SqlClient

''' <summary>
''' 届先より荷主取得
''' </summary>
''' <remarks>受注配車用</remarks>
Public Class GS0031TODtoTORget
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
    ''' 届先コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKECODE() As String
    ''' <summary>
    ''' 取引先コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String

    Protected METHOD_NAME As String = "GS0031TODtoTORget"
    ''' <summary>
    ''' 届先から取引先を取得する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0031TODtoTORget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        TORICODE = ""
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'PARAM EXTRA01:ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = sm.APSV_ORG
        End If
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '●届先より荷主取得（APSRVOrg）
        Try
            If TODOKECODE <> "" Then
                Dim SQLStr As String = _
                            " SELECT rtrim(A.TORICODE)      as TORICODE " _
                        & " FROM MC007_TODKORG            as A        " _
                        & " Where        A.CAMPCODE        = @P1      " _
                        & "          and A.UORG            = @P2      " _
                        & "          and A.TODOKECODE      = @P3      " _
                        & "          and A.DELFLG         <> '1'      "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = CAMPCODE
                PARA2.Value = ORGCODE
                PARA3.Value = TODOKECODE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○出力編集
                If SQLdr.Read Then
                    TORICODE = SQLdr("TORICODE")
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing


                SQLcon.Close() 'DataBase接続(Close)
                SQLcon.Dispose()
                SQLcon = Nothing
            End If

            ERR = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            ERR = C_MESSAGE_NO.DB_ERROR
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC007_TODKORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try


    End Sub
End Class

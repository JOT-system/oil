Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' 画面RightBOX用帳票ID取得
''' </summary>
''' <remarks></remarks>
Public Class GS0005ReportList
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property COMPCODE() As String
    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String
    ''' <summary>
    ''' プロフィールID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PROFID() As String
    ''' <summary>
    ''' 対象年月
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TARGETDATE() As String
    ''' <summary>
    ''' RightBOX用帳票ID(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REPORTOBJ() As Object
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "getList"
    ''' <summary>
    ''' 画面RightBOX用帳票ID取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getList()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: COMPCODE
        If checkParam(METHOD_NAME, COMPCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM02: MAPID
        If checkParam(METHOD_NAME, MAPID) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM03: PROFID
        If checkParam(METHOD_NAME, PROFID) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        '■対象日付
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●初期処理
        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR                                                 '該当するマスタは存在しません
        Dim WW_ListBOX As New ListBox

        '●RightBOX用帳票List取得
        '○ DB(OIS0014_PROFMXLS)検索　…　入力パラメータによる検索
        Try
            'S0011_UPROFXLS検索SQL文
            Dim SQL_Str As String =
                  " SELECT " _
                & "   rtrim(REPORTID) as REPORTID ," _
                & "   rtrim(FIELDNAMES) as FIELDNAME ," _
                & "   CASE WHEN rtrim(EXCELFILE) IS NULL THEN 'なし' " _
                & "        WHEN rtrim(EXCELFILE) ='' THEN 'なし' " _
                & "        ELSE rtrim(EXCELFILE) " _
                & "   END  as EXCELFILE " _
                & " FROM  COM.OIS0014_PROFMXLS " _
                & " WHERE " _
                & "       CAMPCODE    = @P1 " _
                & "   and PROFID     IN (@P2, '" & C_DEFAULT_DATAKEY & "') " _
                & "   and MAPID       = @P3 " _
                & "   and TITLEKBN    = @P4 " _
                & "   and STYMD      <= @P5 " _
                & "   and ENDYMD     >= @P6 " _
                & "   and DELFLG     <> @P7 " _
                & " GROUP BY REPORTID , FIELDNAMES , EXCELFILE " _
                & " ORDER BY REPORTID "

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                SqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = COMPCODE
                    .Add("@P2", SqlDbType.NVarChar, 20).Value = PROFID
                    .Add("@P3", SqlDbType.NVarChar, 50).Value = MAPID
                    .Add("@P4", SqlDbType.NVarChar, 1).Value = C_TITLEKBN.HEADER
                    .Add("@P5", SqlDbType.Date).Value = TARGETDATE
                    .Add("@P6", SqlDbType.Date).Value = TARGETDATE
                    .Add("@P7", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        WW_ListBOX.Items.Add(New ListItem(String.Format("{0}(書式:{1})", SQLdr("FIELDNAME"), SQLdr("EXCELFILE")), Convert.ToString(SQLdr("REPORTID"))))
                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using

                ERR = C_MESSAGE_NO.NORMAL
                REPORTOBJ = WW_ListBOX

                SQLcon.Close() 'DataBase接続(Close)
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Class

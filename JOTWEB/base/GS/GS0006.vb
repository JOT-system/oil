Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' 画面RightBOX用ビューID取得
''' </summary>
''' <remarks></remarks>
Public Class GS0006ViewList
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
    ''' ビューID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW() As ListBox
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected METHOD_NAME As String = "getList"
    ''' <summary>
    ''' 画面RightBOX用ビューID取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub getList()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●In PARAMチェック
        'PARAM01: CAMPCODE
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
        '●初期処理
        ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR                                                   '該当するマスタは存在しません
        VIEW = New ListBox
        'セッション制御宣言
        Dim sm As New CS0050SESSION

        '●画面RightBOX用ビューList取得
        '○ DB(S0025_PROFVIEW)検索　…　入力パラメータによる検索
        Try
            'S0011_UPROFXLS検索SQL文
            Dim SQL_Str As String =
                  " SELECT " _
                & "    rtrim(VARIANT)        as VARIANT            , " _
                & "    rtrim(FIELDNAMES)     as FIELDNAMES           " _
                & " FROM      COM.OIS0012_PROFMVIEW " _
                & " WHERE                         " _
                & "           CAMPCODE   = @P1 " _
                & "      and  PROFID    IN (@P2 ,'" & C_DEFAULT_DATAKEY & "')" _
                & "      and  MAPID      = @P3 " _
                & "      and  TITLEKBN   = @P4 " _
                & "      and  HDKBN      = @P5 " _
                & "      and  STYMD     <= @P6 " _
                & "      and  ENDYMD    >= @P7 " _
                & "      and  DELFLG    <> " & C_DELETE_FLG.DELETE _
                & " GROUP BY VARIANT , FIELDNAMES " _
                & " ORDER BY VARIANT "

            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = COMPCODE
                    .Add("@P2", SqlDbType.NVarChar, 20).Value = PROFID
                    .Add("@P3", SqlDbType.NVarChar, 50).Value = MAPID
                    .Add("@P4", SqlDbType.NVarChar, 1).Value = C_TITLEKBN.HEADER
                    .Add("@P5", SqlDbType.NVarChar, 1).Value = C_HDKBN.HEADER
                    .Add("@P6", SqlDbType.Date).Value = Date.Now
                    .Add("@P7", SqlDbType.Date).Value = Date.Now
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        VIEW.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("FIELDNAMES"), SQLdr("VARIANT")), Convert.ToString(SQLdr("VARIANT"))))
                        ERR = C_MESSAGE_NO.NORMAL
                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using


        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "GS0006VIEWIDget"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class
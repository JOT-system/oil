Imports System.Data.SqlClient

''' <summary>
''' 取引先部署取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0041TORIORGget
    ''' <summary>
    ''' 取引先部署保管テーブル
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns>テーブルデータ</returns>
    ''' <remarks></remarks>
    Public Property TBL() As DataTable
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 取引先コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 運用部署コード
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property UORG() As String
    ''' <summary>
    ''' 取引先タイプ01
    ''' </summary>
    ''' <value>取引先タイプ</value>
    ''' <returns>取引先タイプ</returns>
    ''' <remarks></remarks>
    Public Property TORITYPE01() As String
    ''' <summary>
    ''' 取引先タイプ02
    ''' </summary>
    ''' <value>取引先タイプ</value>
    ''' <returns>取引先タイプ</returns>
    ''' <remarks></remarks>
    Public Property TORITYPE02() As String
    ''' <summary>
    ''' 取引先タイプ03
    ''' </summary>
    ''' <value>取引先タイプ</value>
    ''' <returns>取引先タイプ</returns>
    ''' <remarks></remarks>
    Public Property TORITYPE03() As String
    ''' <summary>
    ''' 取引先タイプ04
    ''' </summary>
    ''' <value>取引先タイプ</value>
    ''' <returns>取引先タイプ</returns>
    ''' <remarks></remarks>
    Public Property TORITYPE04() As String
    ''' <summary>
    ''' 取引先タイプ05
    ''' </summary>
    ''' <value>取引先タイプ</value>
    ''' <returns>取引先タイプ</returns>
    ''' <remarks></remarks>
    Public Property TORITYPE05() As String
    ''' <summary>
    ''' 請求先コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns>取引先コード</returns>
    ''' <remarks></remarks>
    Public Property STORICODE() As String
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0041TORIORGget"
    ''' <summary>
    ''' 取引先部署一覧の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0041TORIORGget()
        '●In PARAMチェック
        'PARAM00: TBL
        If IsNothing(TBL) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TBL"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: TORICODE
        If IsNothing(TORICODE) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TORICODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM03: UORG
        If IsNothing(UORG) Then
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "UORG"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●取引先部署マスタ取得
        Try
            If TBL.Columns.Count = 0 Then
                'DataBase接続文字
                Dim SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT " _
                   & "       isnull(rtrim(CAMPCODE),'') as CAMPCODE " _
                   & "     , isnull(rtrim(TORICODE),'') as TORICODE " _
                   & "     , isnull(rtrim(UORG),'') as UORG " _
                   & "     , isnull(rtrim(TORITYPE01),'') as TORITYPE01 " _
                   & "     , isnull(rtrim(TORITYPE02),'') as TORITYPE02 " _
                   & "     , isnull(rtrim(TORITYPE03),'') as TORITYPE03 " _
                   & "     , isnull(rtrim(TORITYPE04),'') as TORITYPE04 " _
                   & "     , isnull(rtrim(TORITYPE05),'') as TORITYPE05 " _
                   & "     , isnull(rtrim(STORICODE),'') as STORICODE " _
                   & " FROM  OIL.MC003_TORIORG " _
                   & " Where DELFLG     <> '1' "

                Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                'SELECT結果をテンポラリに保存
                TBL.Load(SQLdr)

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

                SQLcmd.Dispose()
                SQLcmd = Nothing

                SQLcon.Close() 'DataBase接続(Close)
                SQLcon.Dispose()
                SQLcon = Nothing

            End If

            TORITYPE01 = ""
            TORITYPE02 = ""
            TORITYPE03 = ""
            TORITYPE04 = ""
            TORITYPE05 = ""
            STORICODE = ""

            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            For Each TBLrow As DataRow In TBL.Rows
                If TBLrow("CAMPCODE") = CAMPCODE And
                   TBLrow("TORICODE") = TORICODE And
                   TBLrow("UORG") = UORG Then
                    TORITYPE01 = TBLrow("TORITYPE01")
                    TORITYPE02 = TBLrow("TORITYPE02")
                    TORITYPE03 = TBLrow("TORITYPE03")
                    TORITYPE04 = TBLrow("TORITYPE04")
                    TORITYPE05 = TBLrow("TORITYPE05")
                    STORICODE = TBLrow("STORICODE")

                    ERR = C_MESSAGE_NO.NORMAL
                    Exit For
                End If
            Next

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC003_TORIORG Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Structure

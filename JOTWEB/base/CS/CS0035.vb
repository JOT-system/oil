Imports System.Data.SqlClient

''' <summary>
''' 配送受注ＤＢ情報取得（受注番号）
''' </summary>
''' <remarks></remarks>
Public Structure CS0035HORDERget
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 出荷部署
    ''' </summary>
    ''' <value>出荷部署</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHIPORG() As String
    ''' <summary>
    ''' 基準日
    ''' </summary>
    ''' <value>基準日</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KIJUNDATE() As Date
    ''' <summary>
    ''' 業務車番
    ''' </summary>
    ''' <value>業務車番</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GSHABAN() As String
    ''' <summary>
    ''' 取引先コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 出荷場所
    ''' </summary>
    ''' <value>出荷場所</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKABASHO() As String
    ''' <summary>
    ''' 届先コード
    ''' </summary>
    ''' <value>届先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TODOKECODE() As String
    ''' <summary>
    ''' 受注番号
    ''' </summary>
    ''' <value>受注番号</value>
    ''' <returns>受注番号</returns>
    ''' <remarks></remarks>
    Public Property ORDERNO() As String
    ''' <summary>
    ''' 受注明細番号
    ''' </summary>
    ''' <value>受注明細番号</value>
    ''' <returns>受注明細番号</returns>
    ''' <remarks></remarks>
    Public Property DETAILNO() As String
    ''' <summary>
    ''' トリップ番号
    ''' </summary>
    ''' <value></value>
    ''' <returns>トリップ番号</returns>
    ''' <remarks></remarks>
    Public Property TRIPNO() As String
    ''' <summary>
    ''' ドロップ番号
    ''' </summary>
    ''' <value></value>
    ''' <returns>ドロップ番号</returns>
    ''' <remarks></remarks>
    Public Property DROPNO() As String
    ''' <summary>
    ''' 枝番
    ''' </summary>
    ''' <value></value>
    ''' <returns>枝番</returns>
    ''' <remarks></remarks>
    Public Property SEQ() As String

    ''' <summary>
    ''' 件数
    ''' </summary>
    ''' <value></value>
    ''' <returns>件数</returns>
    ''' <remarks></remarks>
    Public Property CNT() As String
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
    Public Const METHOD_NAME As String = "CS0035HORDERget"

    ''' <summary>
    ''' 配送受注情報の取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0035HORDERget()

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: SHIPORG
        If IsNothing(SHIPORG) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SHIPORG"                '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM03: KIJUNDATE
        If IsNothing(KIJUNDATE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "KIJUNDATE"              '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        Else
            If KIJUNDATE < C_DEFAULT_YMD Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "KIJUNDATE"                    '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
                Exit Sub
            Else
            End If
        End If

        'PARAM04: GSHABAN
        If IsNothing(GSHABAN) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "GSHABAN"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM05: TORICODE
        If IsNothing(TORICODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TORICODE"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM06: SHUKABASHO
        If IsNothing(SHUKABASHO) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SHUKABASHO"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM07: TODOKECODE
        If IsNothing(TODOKECODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TODOKECODE"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            'T0004_HORDER検索SQL文
            Dim SQLStr As String = ""
            SQLStr = " SELECT ORDERNO, DETAILNO, TRIPNO, DROPNO, SEQ " _
                         & "     FROM T0004_HORDER " _
                         & "     WHERE    CAMPCODE        = @P01 " _
                         & "       and    SHIPORG         = @P02 " _
                         & "       and    KIJUNDATE       = @P03 " _
                         & "       and    GSHABAN         = @P04 " _
                         & "       and    TORICODE        = @P05 " _
                         & "       and    SHUKABASHO      = @P06 " _
                         & "       and    TODOKECODE      = @P07 " _
                         & "       and    DELFLG         <> '1' ; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)

            PARA1.Value = CAMPCODE
            PARA2.Value = SHIPORG
            PARA3.Value = KIJUNDATE
            PARA4.Value = GSHABAN
            PARA5.Value = TORICODE
            PARA6.Value = SHUKABASHO
            PARA7.Value = TODOKECODE

            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            ORDERNO = ""
            DETAILNO = ""
            TRIPNO = ""
            DROPNO = ""
            SEQ = ""
            CNT = 0
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            '最終の配送受注情報を取得
            While SQLdr.Read
                CNT = CNT + 1
                ORDERNO = RTrim(SQLdr("ORDERNO"))
                DETAILNO = RTrim(SQLdr("DETAILNO"))
                TRIPNO = RTrim(SQLdr("TRIPNO"))
                DROPNO = RTrim(SQLdr("DROPNO"))
                SEQ = RTrim(SQLdr("SEQ"))
                ERR = C_MESSAGE_NO.NORMAL
            End While

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

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
End Structure

Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' 会社情報取得
''' </summary>
''' <remarks></remarks>
Public Class GS0001CAMPget
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As Date
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As Date
    ''' <summary>
    ''' 会社名称（短）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMES() As String
    ''' <summary>
    ''' 会社名称(長)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMEL() As String
    ''' <summary>
    ''' 会社カナ名称（短）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMESK() As String
    ''' <summary>
    ''' 会社カナ名称（長）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMELK() As String
    ''' <summary>
    ''' 郵便番号（上）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSTNUM1() As String
    ''' <summary>
    ''' 郵便番号（下）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property POSTNUM2() As String
    ''' <summary>
    ''' 住所1
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR1() As String
    ''' <summary>
    ''' 住所2
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR2() As String
    ''' <summary>
    ''' 住所3
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR3() As String
    ''' <summary>
    ''' 住所4
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ADDR4() As String
    ''' <summary>
    ''' 電話番号
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TEL() As String
    ''' <summary>
    ''' FAX番号
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FAX() As String
    ''' <summary>
    ''' メール
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAIL() As String

    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "GS0001CAMPget"
    ''' <summary>
    ''' 会社情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0001CAMPget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'PARAM02: STYMD

        If checkParam(METHOD_NAME, STYMD) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        ElseIf STYMD < CDate(C_DEFAULT_YMD) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0001CAMPget"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "STYMD"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If

        'PARAM03: ENDYMD
        If checkParam(METHOD_NAME, ENDYMD) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        ElseIf ENDYMD < CDate(C_DEFAULT_YMD) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GS0001CAMPget"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_ENDYMD"                    '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                         '
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●会社情報取得
        '○ DB(OIM0001_CAMP)検索
        Try
            'OIM0001_CAMP検索SQL文
            Dim SQL_Str As String =
                    "SELECT rtrim(NAMES) as NAMES , rtrim(NAMEL) as NAMEL , rtrim(NAMESK) as NAMESK , rtrim(NAMELK) as NAMELK , rtrim(POSTNUM1) as POSTNUM1 , rtrim(POSTNUM2) as POSTNUM2 , rtrim(ADDR1) as ADDR1 , rtrim(ADDR2) as ADDR2 , rtrim(ADDR3) as ADDR3 , rtrim(ADDR4) as ADDR4 , rtrim(TEL) as TEL , rtrim(FAX) as FAX , rtrim(MAIL) as MAIL " _
                & " FROM  OIL.OIM0001_CAMP " _
                & " Where CAMPCODE = @P1 " _
                & "   and STYMD   <= @P2 " _
                & "   and ENDYMD  >= @P3 " _
                & "   and DELFLG  <> @P4 "
            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P2", SqlDbType.Date).Value = ENDYMD
                    .Add("@P3", SqlDbType.Date).Value = STYMD
                    .Add("@P4", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    NAMES = ""
                    NAMEL = ""
                    NAMESK = ""
                    NAMELK = ""
                    POSTNUM1 = ""
                    POSTNUM2 = ""
                    ADDR1 = ""
                    ADDR2 = ""
                    ADDR3 = ""
                    ADDR4 = ""
                    TEL = ""
                    FAX = ""
                    MAIL = ""
                    ERR = C_MESSAGE_NO.DLL_IF_ERROR

                    If SQLdr.Read Then
                        NAMES = Convert.ToString(SQLdr("NAMES"))
                        NAMEL = Convert.ToString(SQLdr("NAMEL"))
                        NAMESK = Convert.ToString(SQLdr("NAMESK"))
                        NAMELK = Convert.ToString(SQLdr("NAMELK"))
                        POSTNUM1 = Convert.ToString(SQLdr("POSTNUM1"))
                        POSTNUM2 = Convert.ToString(SQLdr("POSTNUM2"))
                        ADDR1 = Convert.ToString(SQLdr("ADDR1"))
                        ADDR2 = Convert.ToString(SQLdr("ADDR2"))
                        ADDR3 = Convert.ToString(SQLdr("ADDR3"))
                        ADDR4 = Convert.ToString(SQLdr("ADDR4"))
                        TEL = Convert.ToString(SQLdr("TEL"))
                        FAX = Convert.ToString(SQLdr("FAX"))
                        MAIL = Convert.ToString(SQLdr("MAIL"))
                        ERR = C_MESSAGE_NO.NORMAL
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
                SQLcon.Close() 'DataBase接続(Close)
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "GS0001CAMPget"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIM0001_CAMP Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class


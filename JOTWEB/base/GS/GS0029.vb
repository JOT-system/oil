Imports System.Data.SqlClient

''' <summary>
''' 荷主受注集計制御マスタ取得
''' </summary>
''' <remarks></remarks>
Public Class GS0029T3CNTLget
    Inherits GS0000
    ''' <summary>
    ''' 荷主コード
    ''' </summary>
    ''' <value>取引先コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TORICODE() As String
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 受注部署
    ''' </summary>
    ''' <value>部署コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORDERORG() As String
    ''' <summary>
    ''' 基準日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property KIJUNDATE() As Date
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>届日</remarks>
    Public Property CNTL01() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>出庫日</remarks>
    Public Property CNTL02() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>出荷場所</remarks>
    Public Property CNTL03() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>業務車番</remarks>
    Public Property CNTL04() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>車腹(積載量)</remarks>
    Public Property CNTL05() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>乗務員コード</remarks>
    Public Property CNTL06() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>届先コード</remarks>
    Public Property CNTL07() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名1</remarks>
    Public Property CNTL08() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL09() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL10() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL11() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL12() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL13() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL14() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>品名2</remarks>
    Public Property CNTL15() As String
    ''' <summary>
    ''' 集計区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>(数量/台数</remarks>
    Public Property CNTLVALUE() As String
    ''' <summary>
    ''' 売上計上区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property URIKBN() As String

    Protected METHOD_NAME As String = "GS0029T3CNTLget"
    ''' <summary>
    ''' 荷主受注集計制御マスタ取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0029T3CNTLget()
        '<< エラー説明 >>
        'ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        'PARAM01: KIJUNDATE
        If IsNothing(KIJUNDATE) Then
            KIJUNDATE = Date.Now
        End If

        'PARAM02: CAMPCODE
        'TODO:未設定時はエラーにする必要がこの後ある
        If CAMPCODE = Nothing Then
            CAMPCODE = "02"
        End If
        '●初期処理
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●荷主受注集計制御マスタ取得

        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        '検索SQL文
        Try
            '検索SQL文
            Dim SQLStr As String =
                    "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,        " _
                & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,        " _
                & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,         " _
                & "       isnull(rtrim(A.CNTL01),'')            as CNTL01 ,          " _
                & "       isnull(rtrim(A.CNTL02),'')            as CNTL02 ,          " _
                & "       isnull(rtrim(A.CNTL03),'')            as CNTL03 ,          " _
                & "       isnull(rtrim(A.CNTL04),'')            as CNTL04 ,          " _
                & "       isnull(rtrim(A.CNTL05),'')            as CNTL05 ,          " _
                & "       isnull(rtrim(A.CNTL06),'')            as CNTL06 ,          " _
                & "       isnull(rtrim(A.CNTL07),'')            as CNTL07 ,          " _
                & "       isnull(rtrim(A.CNTL08),'')            as CNTL08 ,          " _
                & "       isnull(rtrim(A.CNTL09),'')            as CNTL09 ,          " _
                & "       isnull(rtrim(A.CNTL10),'')            as CNTL10 ,          " _
                & "       isnull(rtrim(A.CNTL11),'')            as CNTL11 ,          " _
                & "       isnull(rtrim(A.CNTL12),'')            as CNTL12 ,          " _
                & "       isnull(rtrim(A.CNTL13),'')            as CNTL13 ,          " _
                & "       isnull(rtrim(A.CNTL14),'')            as CNTL14 ,          " _
                & "       isnull(rtrim(A.CNTL15),'')            as CNTL15 ,          " _
                & "       isnull(rtrim(A.CNTLVALUE),'')         as CNTLVALUE,        " _
                & "       isnull(rtrim(A.URIKBN),'')            as URIKBN            " _
                & " FROM OIL.MC010_T3CNTL AS A								" _
                & " WHERE A.CAMPCODE         = @P01                      " _
                & "   and A.TORICODE     	= @P02      				" _
                & "   and A.OILTYPE          = @P03           		    " _
                & "   and A.ORDERORG         = @P04           		    " _
                & "   and A.STYMD           <= @P05      				" _
                & "   and A.ENDYMD          >= @P05      				" _
                & "   and A.DELFLG          <> '1'                       "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出荷日

            '○検索条件指定
            PARA01.Value = CAMPCODE                                               '会社
            PARA02.Value = TORICODE                                               '荷主
            PARA03.Value = OILTYPE                                                '油種
            PARA04.Value = ORDERORG                                               '受注部署
            PARA05.Value = KIJUNDATE                                              '基準日

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル格納
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            If SQLdr.Read Then
                CNTL01 = SQLdr("CNTL01")                                '集計区分(届日)
                CNTL02 = SQLdr("CNTL02")                                '集計区分(出庫日)
                CNTL03 = SQLdr("CNTL03")                                '集計区分(出荷場所)
                CNTL04 = SQLdr("CNTL04")                                '集計区分(業務車番)
                CNTL05 = SQLdr("CNTL05")                                '集計区分(車腹(積載量))
                CNTL06 = SQLdr("CNTL06")                                '集計区分(乗務員コード)
                CNTL07 = SQLdr("CNTL07")                                '集計区分(届先コード)
                CNTL08 = SQLdr("CNTL08")                                '集計区分(品名１)
                CNTL09 = SQLdr("CNTL09")                                '集計区分(品名２)
                CNTL10 = SQLdr("CNTL10")                                '集計区分(予備)
                CNTL11 = SQLdr("CNTL11")                                '集計区分(予備)
                CNTL12 = SQLdr("CNTL12")                                '集計区分(予備)
                CNTL13 = SQLdr("CNTL13")                                '集計区分(予備)
                CNTL14 = SQLdr("CNTL14")                                '集計区分(予備)
                CNTL15 = SQLdr("CNTL15")                                '集計区分(予備)
                CNTLVALUE = SQLdr("CNTLVALUE")                          '集計区分(数量/台数)
                URIKBN = SQLdr("URIKBN")                                '売上計上基準

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
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_PRODUCT Select"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

End Class

Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Web
Imports System.Data.SqlClient
Imports System.Net

''' <summary>
''' 自動採番
''' </summary>
''' <remarks></remarks>
Public Class CS0033AutoNumber
    ''' <summary>
    ''' 自動採番タイプ
    ''' </summary>
    Public Class C_SEQTYPE
        ''' <summary>
        ''' 届先
        ''' </summary>
        Public Const TODOKESAKI As String = "TODOKESAKI"

        ''' <summary>
        ''' 統一車番（前）車輛タイプA
        ''' </summary>
        Public Const SHARYO_A As String = "A"

        ''' <summary>
        ''' 統一車番（前）車輛タイプB
        ''' </summary>
        Public Const SHARYO_B As String = "B"

        ''' <summary>
        ''' 統一車番（前）車輛タイプC
        ''' </summary>
        Public Const SHARYO_C As String = "C"

        ''' <summary>
        ''' 統一車番（前）車輛タイプD
        ''' </summary>
        Public Const SHARYO_D As String = "D"

        ''' <summary>
        ''' 統一車番（前）車輛タイプ 庸車E(A)
        ''' </summary>
        Public Const SHARYO_YO_E As String = "E"

        ''' <summary>
        ''' 統一車番（前）車輛タイプ 庸車F(B)
        ''' </summary>
        Public Const SHARYO_YO_F As String = "F"

        ''' <summary>
        ''' 統一車番（前）車輛タイプ 庸車G(C)
        ''' </summary>
        Public Const SHARYO_YO_G As String = "G"

        ''' <summary>
        ''' 統一車番（前）車輛タイプ 庸車H(D)
        ''' </summary>
        Public Const SHARYO_YO_H As String = "H"
        ''' <summary>
        ''' 受注番号
        ''' </summary>
        Public Const ORDERNO As String = "ORDER"

        ''' <summary>
        ''' 伝票番号
        ''' </summary>
        Public Const DENNO As String = "DENNO"

    End Class

    Public Class C_POST_KEYWORD
        ''' <summary>
        ''' POST用キーワード　自動採番タイプ
        ''' </summary>
        Public Const SEQTYPE As String = "SEQTYPE"
        ''' <summary>
        ''' POST用キーワード　会社コード
        ''' </summary>
        Public Const CAMPCODE As String = "CAMPCODE"
        ''' <summary>
        ''' POST用キーワード　管理部署コード
        ''' </summary>
        Public Const MORG As String = "MORG"
        ''' <summary>
        ''' POST用キーワード　ユーザID
        ''' </summary>
        Public Const USERID As String = "USERID"
    End Class

    ''' <summary>
    ''' POST用レスポンスステータスコード
    ''' </summary>
    Public Enum C_POST_STATUSCODE As Integer
        ''' <summary>
        ''' KEYWORD設定エラー
        ''' </summary>
        ILLEGAL_KEYWORD_ERROR = 300
        ''' <summary>
        ''' 採番データ未存在
        ''' </summary>
        SEQ_NOT_FOUND_ERROR
        ''' <summary>
        ''' 採番更新エラー
        ''' </summary>
        SEQ_UPDATE_ERROR
        ''' <summary>
        ''' 採番処理例外エラー
        ''' </summary>
        SEQ_UPDATE_EX_ERROR
        ''' <summary>
        ''' 採番最大値超過エラー
        ''' </summary>
        SEQ_OVER_ERROR
    End Enum

    ''' <summary>
    ''' [IN]採番対象パラメタ
    ''' </summary>
    ''' <value>パラメータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SEQTYPE As String
    ''' <summary>
    ''' [IN]採番対象パラメタ（車輛タイプ）
    ''' </summary>
    ''' <value>車輛タイプ</value>
    ''' <remarks></remarks>
    Public WriteOnly Property SHARYOTYPE As String
        Set(value As String)
            Select Case value
                Case C_SEQTYPE.SHARYO_A, C_SEQTYPE.SHARYO_B, C_SEQTYPE.SHARYO_C, C_SEQTYPE.SHARYO_D
                    SEQTYPE = value
                Case C_SEQTYPE.SHARYO_YO_E, C_SEQTYPE.SHARYO_YO_F, C_SEQTYPE.SHARYO_YO_G, C_SEQTYPE.SHARYO_YO_H
                    SEQTYPE = value
                Case Else
                    SEQTYPE = String.Empty
                    ERR = C_MESSAGE_NO.DLL_IF_ERROR
            End Select
        End Set
    End Property

    ''' <summary>
    ''' [IN]会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE As String
    ''' <summary>
    ''' [IN]管理部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>届先・統一車番時は未使用</remarks>
    Public Property MORG As String
    ''' <summary>
    ''' [IN]更新ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
    ''' <summary>
    ''' [OUT]採番結果
    ''' </summary>
    ''' <value></value>
    ''' <returns>採番</returns>
    ''' <remarks></remarks>
    Public Property SEQ As String
    ''' <summary>
    ''' [OUT]ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR As String
    ''' <summary>
    ''' [OUT]ERR詳細コードプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR_DETAIL As Integer

    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private sm As New CS0050SESSION

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub New()

        'プロパティ初期化
        Initialize()
    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        SEQTYPE = String.Empty
        SHARYOTYPE = String.Empty
        CAMPCODE = String.Empty
        MORG = String.Empty
        SEQ = String.Empty
        USERID = String.Empty
        ERR = C_MESSAGE_NO.NORMAL
        ERR_DETAIL = 0
    End Sub

    ''' <summary>
    '''自動採番処理
    ''' </summary>
    ''' <remarks>
    '''  [IN]  SEQTYPE or SHARYOTYPE
    '''  [IN]  CAMPCODE
    '''  [IN]  MORG(SEQTYPE="DENNO" or "ORDER")　
    '''  [IN]  SEQ
    '''  [IN]  USERID
    '''  [OUT] ERR
    '''  [OUT] ERR_DETAIL
    ''' </remarks>
    Public Sub getAutoNumber()
        Dim CS0011LOGWRITE As New CS0011LOGWrite

        Dim WW_SVPROC As Boolean = False                            ' 全社サーバ転送

        '○初期化
        SEQ = String.Empty
        ERR = C_MESSAGE_NO.NORMAL

        '●In PARAMチェック
        'PARAM01: SEQTYPE
        Select Case SEQTYPE
            Case C_SEQTYPE.TODOKESAKI
                WW_SVPROC = True
            Case C_SEQTYPE.SHARYO_A, C_SEQTYPE.SHARYO_B, C_SEQTYPE.SHARYO_C, C_SEQTYPE.SHARYO_D
                WW_SVPROC = True
            Case C_SEQTYPE.SHARYO_YO_E, C_SEQTYPE.SHARYO_YO_F, C_SEQTYPE.SHARYO_YO_G, C_SEQTYPE.SHARYO_YO_H
                WW_SVPROC = True
            Case C_SEQTYPE.ORDERNO
                WW_SVPROC = True
            Case C_SEQTYPE.DENNO
                WW_SVPROC = True
            Case Else
                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "SEQTYPE"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
        End Select
        'PARAM02: CAMPCODE
        If String.IsNullOrEmpty(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'PARAM03: MORG
        If SEQTYPE = C_SEQTYPE.ORDERNO OrElse SEQTYPE = C_SEQTYPE.DENNO Then
            ' 受注番号、伝票番号は部署必須
            If String.IsNullOrEmpty(MORG) Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "SEQTYPE"                          '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If
        Else
            MORG = String.Empty
        End If
        'PARAM04: USERID
        If String.IsNullOrEmpty(USERID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "USERID"                          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        If WW_SVPROC = True Then
            ' 全社サーバにて採番処理

            Dim WW_IPADDR As String = String.Empty
            ' 全社サーバ情報取得
            getServerInfo(WW_IPADDR)
            If isNormal(ERR) Then
                ' 全社サーバ採番処理リクエスト
                ' ※全社サーバ側で自動採番処理を実行する
                reqServerProc(WW_IPADDR)
            End If
        Else
            ' 自動採番処理
            getAutoNumberProc()
        End If

    End Sub

    ''' <summary>
    '''サーバ情報取得処理
    ''' </summary>
    ''' <param name="O_IPADDR" >IPアドレス</param>
    ''' <remarks></remarks>
    Private Sub getServerInfo(ByRef O_IPADDR As String)
        Dim CS0011LOGWRITE As New CS0011LOGWrite
        Dim WW_IPADDR As String = String.Empty

        Try
            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                          " SELECT rtrim(IPADDR)     as IPADDR      " _
                        & "   FROM com.OIS0001_TERM                       " _
                        & "  Where TERMCLASS    = @P1               " _
                        & "    and STYMD       <= @P2               " _
                        & "    and ENDYMD      >= @P2               " _
                        & "    and DELFLG      <> @P3               "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    PARA1.Value = C_TERMCLASS.CLOUD
                    PARA2.Value = Date.Now
                    PARA3.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        If SQLdr.HasRows = True Then
                            '全社サーバIPを取得
                            While SQLdr.Read
                                WW_IPADDR = SQLdr("IPADDR")
                            End While
                        Else
                            'データが抽出出来ない場合
                            ERR = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"             '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWRITE.TEXT = "データが存在しません。"
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                        End If

                    End Using
                End Using
            End Using

            O_IPADDR = WW_IPADDR

        Catch ex As Exception
            ERR = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力
        End Try

    End Sub
    ''' <summary>
    '''サーバ採番処理リクエスト
    ''' </summary>
    ''' <param name="I_IPADDR" >IPアドレス</param>
    ''' <remarks></remarks>
    Private Sub reqServerProc(ByVal I_IPADDR As String)
        Dim CS0011LOGWRITE As New CS0011LOGWrite

        Try
            '○Web要求定義
            Dim WW_req As WebRequest = WebRequest.Create(HttpContext.Current.Request.Url.Scheme & "://" & I_IPADDR & C_URL.NUMBER_ASSIGNMENT)
            ' 権限設定
            WW_req.Credentials = CredentialCache.DefaultCredentials
            '○ポスト・データの作成
            Dim WW_POSTitems As String = String.Empty
            Dim WW_POSTitem As New Dictionary(Of String, String)

            '　ポスト・データ編集(KEYは複数設定可能)＆設定（区切りは"|"）
            WW_POSTitem(C_POST_KEYWORD.SEQTYPE) = HttpUtility.UrlEncode(SEQTYPE, Encoding.UTF8)
            WW_POSTitem(C_POST_KEYWORD.CAMPCODE) = HttpUtility.UrlEncode(CAMPCODE, Encoding.UTF8)
            WW_POSTitem(C_POST_KEYWORD.MORG) = HttpUtility.UrlEncode(MORG, Encoding.UTF8)
            WW_POSTitem(C_POST_KEYWORD.USERID) = HttpUtility.UrlEncode(USERID, Encoding.UTF8)

            For Each k As KeyValuePair(Of String, String) In WW_POSTitem
                WW_POSTitems &= k.Key & "=" & k.Value & "|"
            Next

            Dim WW_data As Byte() = Encoding.UTF8.GetBytes(WW_POSTitems)
            WW_req.Method = "POST"
            WW_req.ContentType = "application/x-www-form-urlencoded"
            WW_req.ContentLength = WW_data.Length

            '○ポスト・データ書込み
            Using reqStream As Stream = WW_req.GetRequestStream()
                reqStream.Write(WW_data, 0, WW_data.Length)
            End Using

            '○ポスト実行
            Using response As HttpWebResponse = CType(WW_req.GetResponse(), HttpWebResponse)
                '○自動採番結果取得
                Using reader As New StreamReader(response.GetResponseStream())
                    SEQ = reader.ReadToEnd()
                End Using
            End Using

        Catch ex As System.Net.WebException
            Select Case CType(ex.Response, HttpWebResponse).StatusCode
                Case C_POST_STATUSCODE.ILLEGAL_KEYWORD_ERROR
                    ERR = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "CS0001DBcon"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(INI_File Not Find)"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case C_POST_STATUSCODE.SEQ_NOT_FOUND_ERROR
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:M0008_SEQNO Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = "データが存在しません。"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case C_POST_STATUSCODE.SEQ_UPDATE_ERROR
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name                'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:M0008_SEQNO Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                    CS0011LOGWRITE.TEXT = "更新に失敗しました。再度実行してください。"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case C_POST_STATUSCODE.SEQ_UPDATE_EX_ERROR
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:M0008_SEQNO Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(DB M0008_SEQNO Select ERR)"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                Case Else
                    ERR = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:M0008_SEQNO Select"      '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
            End Select
        End Try
    End Sub

    ''' <summary>
    ''' 採番処理
    ''' </summary>
    ''' <remarks>HTTPリクエスト経由からの呼び出し用</remarks>
    Public Sub getAutoNumberProc()
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        Dim WW_SEQ As Long = 0
        Dim WW_MAXSEQ As Long = 0
        Dim WW_MAXSEQKBN As String = String.Empty
        Dim WW_TIMSTP As Long = 0
        Dim WW_UPDCNT As Integer = 0
        Dim PARA(8) As SqlParameter

        ERR = C_MESSAGE_NO.NORMAL
        ERR_DETAIL = 0
        Dim KEY_INFO As String = "(SEQTYPE[" & SEQTYPE & "],CAMPCODE[" & CAMPCODE & "],MORG[" & MORG & "],USERID[" & USERID & "])"
        Try
            If String.IsNullOrEmpty(sm.DBCon) Then
                ' HTTPリクエスト経由時はセッション情報無いので自ら設定
                Dim CS001INI As New CS0001INIFILEget                 'INI File Data Get
                CS001INI.CS0001INIFILEget()
            End If

            'DataBase接続文字
            Using SQLcon = sm.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                           "SELECT SEQ, MAXSEQ, MAXSEQKBN , CAST(UPDTIMSTP as bigint) as TIMSTP" _
                         & " FROM oil.M0008_SEQNO WITH (UPDLOCK) " _
                         & " WHERE SEQTYPE    = @P01 " _
                         & " AND   CAMPCODE   = @P02 " _
                         & " AND   MORG       = @P03 " _
                         & " AND   DELFLG    <> @P04 "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    PARA(0) = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                    PARA(1) = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                    PARA(2) = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                    PARA(3) = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
                    PARA(0).Value = SEQTYPE
                    PARA(1).Value = CAMPCODE
                    PARA(2).Value = MORG
                    PARA(3).Value = C_DELETE_FLG.DELETE
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.HasRows = True Then
                            While SQLdr.Read
                                WW_SEQ = SQLdr("SEQ")
                                WW_MAXSEQ = SQLdr("MAXSEQ")
                                WW_MAXSEQKBN = SQLdr("MAXSEQKBN")
                                WW_TIMSTP = SQLdr("TIMSTP")
                            End While
                        Else
                            ERR_DETAIL = C_POST_STATUSCODE.SEQ_NOT_FOUND_ERROR
                            Throw New Exception("採番マスタ（M0008_SEQNO）が存在しません" & KEY_INFO)
                        End If
                    End Using
                End Using

                ' 番号カウントアップ
                WW_SEQ += 1
                If WW_SEQ > WW_MAXSEQ Then
                    If WW_MAXSEQKBN = "1" Then
                        ERR_DETAIL = C_POST_STATUSCODE.SEQ_OVER_ERROR
                        Throw New Exception("採番マスタ（M0008_SEQNO）が最大番号を超えています" & KEY_INFO)
                    End If
                    WW_SEQ = 1
                End If

                '番号更新
                SQLStr =
                            "UPDATE M0008_SEQNO                     " _
                          & "SET SEQ        = @P05                  " _
                          & "  , UPDYMD     = @P06                  " _
                          & "  , UPDUSER    = @P07                  " _
                          & "  , UPDTERMID  = @P08                  " _
                          & "  , RECEIVEYMD = @P09                  " _
                          & "WHERE                                  " _
                          & "    SEQTYPE  = @P01                    " _
                          & "AND CAMPCODE = @P02                    " _
                          & "AND MORG  = @P03                       " _
                          & "AND CAST(UPDTIMSTP as bigint)  = @P04; "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    PARA(0) = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    PARA(1) = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    PARA(2) = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 15)
                    PARA(3) = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.BigInt)
                    PARA(4) = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 7)
                    PARA(5) = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.DateTime)
                    PARA(6) = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
                    PARA(7) = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 30)
                    PARA(8) = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.DateTime)
                    PARA(0).Value = SEQTYPE
                    PARA(1).Value = CAMPCODE
                    PARA(2).Value = MORG
                    PARA(3).Value = WW_TIMSTP
                    PARA(4).Value = WW_SEQ
                    PARA(5).Value = Date.Now
                    PARA(6).Value = USERID
                    PARA(7).Value = sm.APSV_ID
                    PARA(8).Value = C_DEFAULT_YMD

                    WW_UPDCNT = SQLcmd.ExecuteNonQuery()
                    If WW_UPDCNT = 0 Then
                        ERR_DETAIL = C_POST_STATUSCODE.SEQ_UPDATE_EX_ERROR
                        Throw New Exception("他の端末と競合し、採番できませんでした")
                    End If

                    '採番番号書式設定（最大値の桁数でZeroPadding）
                    SEQ = WW_SEQ.ToString("D" & WW_MAXSEQ.ToString.Length)

                    If SEQTYPE = C_SEQTYPE.SHARYO_A OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_B OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_C OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_D OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_YO_E OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_YO_F OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_YO_G OrElse
                       SEQTYPE = C_SEQTYPE.SHARYO_YO_H Then
                        ' 車番の場合は先頭に会社コード付与
                        ' ex) 車輛タイプ=A，会社コード=02，採番結果=12345
                        '     ⇒ 0212345 ('02' & '12345)'
                        SEQ = CAMPCODE & SEQ
                    End If
                End Using

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0008_SEQNO Select_Update"   '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            ' 詳細コード未定時は例外エラーコードを設定
            ERR_DETAIL = IIf(ERR_DETAIL = 0, C_POST_STATUSCODE.SEQ_UPDATE_EX_ERROR, ERR_DETAIL)
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class

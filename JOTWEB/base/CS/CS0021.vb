Imports System.Data.SqlClient

''' <summary>
''' プロファイル(帳票)取得
''' </summary>
''' <remarks></remarks>
Public Structure CS0021PROFXLS

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value>プロファイルID</value>
    ''' <returns>プロファイルID</returns>
    ''' <remarks></remarks>
    Public Property PROFID() As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    ''' <value>帳票ID</value>
    ''' <returns>帳票ID</returns>
    ''' <remarks></remarks>
    Public Property REPORTID() As String

    ''' <summary>
    ''' タイトル・明細区分
    ''' </summary>
    ''' <value></value>
    ''' <returns>種別区分</returns>
    ''' <remarks>タイトル(H or T)、明細(I or I_Data or I_DataKey)</remarks>
    Public Property TITLEKBN() As List(Of String)

    ''' <summary>
    ''' 表示項目
    ''' </summary>
    ''' <value></value>
    ''' <returns>項目</returns>
    ''' <remarks></remarks>
    Public Property FIELD() As List(Of String)

    ''' <summary>
    ''' 表示項目名
    ''' </summary>
    ''' <value></value>
    ''' <returns>項目名</returns>
    ''' <remarks></remarks>
    Public Property FIELDNAME() As List(Of String)

    ''' <summary>
    ''' 項目構造体
    ''' </summary>
    ''' <value></value>
    ''' <returns>構造</returns>
    ''' <remarks></remarks>
    Public Property STRUCT() As List(Of String)

    ''' <summary>
    ''' 列位置
    ''' </summary>
    ''' <value></value>
    ''' <returns>列位置</returns>
    ''' <remarks></remarks>
    Public Property POSIX() As List(Of Integer)

    ''' <summary>
    ''' 行位置
    ''' </summary>
    ''' <value></value>
    ''' <returns>行位置</returns>
    ''' <remarks></remarks>
    Public Property POSIY() As List(Of Integer)

    ''' <summary>
    ''' 表示幅
    ''' </summary>
    ''' <value></value>
    ''' <returns>表示幅</returns>
    ''' <remarks></remarks>
    Public Property WIDTH() As List(Of Integer)

    ''' <summary>
    ''' ソート順
    ''' </summary>
    ''' <value></value>
    ''' <returns>ソート順</returns>
    ''' <remarks></remarks>
    Public Property SORT() As List(Of Integer)

    ''' <summary>
    ''' 区分値タイトルの最大列数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大列数</returns>
    ''' <remarks></remarks>
    Public Property POSI_T_X_MAX() As Integer

    ''' <summary>
    ''' 区分値タイトルの最大行数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大行数</returns>
    ''' <remarks></remarks>
    Public Property POSI_T_Y_MAX() As Integer

    ''' <summary>
    '''  区分値明細の最大列数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大列数</returns>
    ''' <remarks></remarks>
    Public Property POSI_I_X_MAX() As Integer

    ''' <summary>
    ''' 区分値明細の最大行数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大行数</returns>
    ''' <remarks></remarks>
    Public Property POSI_I_Y_MAX() As Integer

    ''' <summary>
    ''' 繰返しアイテムの最大列数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大列数</returns>
    ''' <remarks></remarks>
    Public Property POSI_R_X_MAX() As Integer

    ''' <summary>
    ''' 繰返しアイテムの最大行数
    ''' </summary>
    ''' <value></value>
    ''' <returns>最大行数</returns>
    ''' <remarks></remarks>
    Public Property POSI_R_Y_MAX() As Integer

    ''' <summary>
    ''' 有効区分
    ''' </summary>
    ''' <value></value>
    ''' <returns>有効区分</returns>
    ''' <remarks>Y:有効　N：無効</remarks>
    Public Property EFFECT() As List(Of String)

    ''' <summary>
    ''' 書式用Excelファイル名
    ''' </summary>
    ''' <value>ファイル名</value>
    ''' <returns>ファイル名</returns>
    ''' <remarks></remarks>
    Public Property EXCELFILE() As String

    ''' <summary>
    ''' 明細開始位置
    ''' </summary>
    ''' <value>開始位置</value>
    ''' <returns>開始位置</returns>
    ''' <remarks></remarks>
    Public Property POSISTART() As Integer

    ''' <summary>
    ''' ソート文字列
    ''' </summary>
    ''' <value></value>
    ''' <returns>ソート文字列</returns>
    ''' <remarks>未使用項目</remarks>
    Public Property SORTstr() As String

    ''' <summary>
    ''' ヘッダー記載
    ''' </summary>
    ''' <value></value>
    ''' <returns>ヘッダー記載</returns>
    ''' <remarks>未使用項目</remarks>
    Public Property HEADWRITE() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>エラーコード</returns>
    ''' <remarks></remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 対象日付
    ''' </summary>
    ''' <value>対象日付</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TARGETDATE() As String

    Public Const METHOD_NAME = "CS0021PROFXLS"

    ''' <summary>
    ''' プロファイル取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0021PROFXLS()

        ERR = C_MESSAGE_NO.DLL_IF_ERROR

        '●In PARAM
        'CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "CAMPCODE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT        'パラメーターエラー
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
        End If

        'MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "MAPID"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT        'パラメーターエラー
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'REPORTID
        If IsNothing(REPORTID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "REPORTID"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT        'パラメーターエラー
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'TARGETDATE
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        '●プロファイル(帳票)取得
        Dim CS0050SESSION As New CS0050SESSION
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open()       'DataBase接続(Open)
        Try
            GetProfM(SQLcon)
        Finally
            SQLcon.Close()      'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

    End Sub

    ''' <summary>
    ''' プロファイル(帳票)取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub GetProfM(ByVal SQLcon As SqlConnection)

        Dim SQLstr As String =
              " SELECT                                " _
            & "      RTRIM(TITLEKBN)   AS TITLEKBN" _
            & "    , RTRIM(PROFID)     AS PROFID" _
            & "    , RTRIM(REPORTID)   AS REPORTID" _
            & "    , RTRIM(EXCELFILE)  AS EXCELFILE" _
            & "    , RTRIM(FIELD)      AS FIELD" _
            & "    , RTRIM(FIELDNAMES) AS FIELDNAMES" _
            & "    , POSISTART         AS POSISTART " _
            & "    , POSIROW           AS POSIROW  " _
            & "    , POSICOL           AS POSICOL " _
            & "    , WIDTH             AS WIDTH " _
            & "    , RTRIM(EFFECT)     AS EFFECT" _
            & "    , RTRIM(STRUCTCODE) AS STRUCT" _
            & "    , SORTORDER         AS SORTORDER" _
            & " FROM" _
            & "    com.S0026_PROFMXLS" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND PROFID   = @P2" _
            & "    AND MAPID    = @P3" _
            & "    AND REPORTID = @P4" _
            & "    AND STYMD   <= @P5" _
            & "    AND ENDYMD  >= @P5" _
            & "    AND DELFLG  <> @P6" _
            & " ORDER BY" _
            & "    SORTORDER"

        Dim SQLcmd As New SqlCommand()

        TITLEKBN = New List(Of String)
        FIELD = New List(Of String)
        FIELDNAME = New List(Of String)
        STRUCT = New List(Of String)
        POSIX = New List(Of Integer)
        POSIY = New List(Of Integer)
        WIDTH = New List(Of Integer)
        EFFECT = New List(Of String)
        SORT = New List(Of Integer)

        EXCELFILE = ""
        POSISTART = 0
        POSI_T_X_MAX = 0
        POSI_T_Y_MAX = 0
        POSI_I_X_MAX = 0
        POSI_I_Y_MAX = 0
        POSI_R_X_MAX = 0
        POSI_R_Y_MAX = 0
        SORTstr = ""
        HEADWRITE = ""

        Try
            SQLcmd = New SqlCommand(SQLstr, SQLcon)

            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 50)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)

            PARA1.Value = CAMPCODE
            PARA3.Value = MAPID
            PARA4.Value = REPORTID
            PARA5.Value = TARGETDATE
            PARA6.Value = C_DELETE_FLG.DELETE

            Dim WW_READ As Boolean = False
            For Each key As String In {PROFID, C_DEFAULT_DATAKEY}
                PARA2.Value = key

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Select Case SQLdr("TITLEKBN")
                            Case "H"                'ヘッダー領域
                                If Not IsDBNull(SQLdr("EXCELFILE")) Then
                                    EXCELFILE = SQLdr("EXCELFILE")
                                End If
                                POSISTART = SQLdr("POSISTART")
                                PROFID = SQLdr("PROFID")
                                REPORTID = SQLdr("REPORTID")
                                HEADWRITE = SQLdr("EFFECT")

                            Case "T"                'タイトル領域
                                WW_READ = True
                                TITLEKBN.Add(SQLdr("TITLEKBN"))
                                FIELD.Add(SQLdr("FIELD"))
                                FIELDNAME.Add(SQLdr("FIELDNAMES"))
                                If IsDBNull(SQLdr("STRUCT")) Then
                                    STRUCT.Add(Space(20))
                                Else
                                    STRUCT.Add(SQLdr("STRUCT"))
                                End If
                                POSIX.Add(SQLdr("POSICOL"))
                                POSIY.Add(SQLdr("POSIROW"))
                                WIDTH.Add(SQLdr("WIDTH"))
                                EFFECT.Add(SQLdr("EFFECT"))
                                SORT.Add(SQLdr("SORTORDER"))

                                If SQLdr("POSICOL") > POSI_T_X_MAX Then
                                    POSI_T_X_MAX = SQLdr("POSICOL")
                                End If

                                If SQLdr("POSIROW") > POSI_T_Y_MAX Then
                                    POSI_T_Y_MAX = SQLdr("POSIROW")
                                End If

                            Case "I"                '明細領域
                                WW_READ = True
                                TITLEKBN.Add(SQLdr("TITLEKBN"))
                                FIELD.Add(SQLdr("FIELD"))
                                FIELDNAME.Add(SQLdr("FIELDNAMES"))
                                If IsDBNull(SQLdr("STRUCT")) Then
                                    STRUCT.Add(Space(20))
                                Else
                                    STRUCT.Add(SQLdr("STRUCT"))
                                End If
                                POSIX.Add(SQLdr("POSICOL"))
                                POSIY.Add(SQLdr("POSIROW"))
                                WIDTH.Add(SQLdr("WIDTH"))
                                EFFECT.Add(SQLdr("EFFECT"))
                                SORT.Add(SQLdr("SORTORDER"))

                                If SQLdr("POSICOL") > POSI_I_X_MAX Then
                                    POSI_I_X_MAX = SQLdr("POSICOL")
                                End If

                                If SQLdr("POSIROW") > POSI_I_Y_MAX Then
                                    POSI_I_Y_MAX = SQLdr("POSIROW")
                                End If

                            Case "I_Data"           '繰返アイテムデータ
                                WW_READ = True
                                TITLEKBN.Add(SQLdr("TITLEKBN"))
                                FIELD.Add(SQLdr("FIELD"))
                                FIELDNAME.Add(SQLdr("FIELDNAMES"))
                                If IsDBNull(SQLdr("STRUCT")) Then
                                    STRUCT.Add(Space(20))
                                Else
                                    STRUCT.Add(SQLdr("STRUCT"))
                                End If
                                POSIX.Add(SQLdr("POSICOL"))
                                POSIY.Add(SQLdr("POSIROW"))
                                WIDTH.Add(SQLdr("WIDTH"))
                                EFFECT.Add(SQLdr("EFFECT"))
                                SORT.Add(SQLdr("SORTORDER"))

                                If SQLdr("POSICOL") > POSI_R_X_MAX Then
                                    POSI_R_X_MAX = SQLdr("POSICOL")
                                End If

                                If SQLdr("POSIROW") > POSI_R_Y_MAX Then
                                    POSI_R_Y_MAX = SQLdr("POSIROW")
                                End If

                            Case "I_DataKey"        '繰返アイテムキー
                                WW_READ = True
                                TITLEKBN.Add(SQLdr("TITLEKBN"))
                                FIELD.Add(SQLdr("FIELD"))
                                FIELDNAME.Add(SQLdr("FIELDNAMES"))
                                If IsDBNull(SQLdr("STRUCT")) Then
                                    STRUCT.Add(Space(20))
                                Else
                                    STRUCT.Add(SQLdr("STRUCT"))
                                End If
                                POSIX.Add(SQLdr("POSICOL"))
                                POSIY.Add(SQLdr("POSIROW"))
                                WIDTH.Add(SQLdr("WIDTH"))
                                EFFECT.Add(SQLdr("EFFECT"))
                                SORT.Add(SQLdr("SORTORDER"))
                        End Select

                        'ソート文字列取得
                        If Not (SQLdr("TITLEKBN") = "H" Or SQLdr("TITLEKBN") = "T") Then
                            If Not IsDBNull(SQLdr("SORTORDER")) Then
                                If Not SQLdr("SORTORDER") = 0 Then
                                    If SORTstr = "" Then
                                        SORTstr &= SQLdr("FIELD")
                                    Else
                                        SORTstr &= " , " & SQLdr("FIELD")
                                    End If
                                End If
                            End If
                        End If
                    End While
                End Using

                If WW_READ Then
                    ERR = C_MESSAGE_NO.NORMAL
                    Exit For
                End If
            Next
        Catch ex As Exception
            ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:S0026_PROFMXLS Select"         '問題発生個所
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                 '異常
            CS0011LOGWrite.TEXT = ex.ToString()                         '例外エラー
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR            'DBエラー
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        Finally
            SQLcmd.Dispose()
            SQLcmd = Nothing
        End Try

    End Sub

End Structure

Option Strict On
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
        Using SQLcon = CS0050SESSION.getConnection
            SQLcon.Open()
            GetProfM(SQLcon)
            SQLcon.Close()
        End Using

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
            & "    COM.OIS0014_PROFMXLS" _
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
            Using SQLcmd As New SqlCommand(SQLstr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P3", SqlDbType.NVarChar, 50).Value = MAPID
                    .Add("@P4", SqlDbType.NVarChar, 50).Value = REPORTID
                    .Add("@P5", SqlDbType.Date).Value = TARGETDATE
                    .Add("@P6", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                '動的パラメータ
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)

                Dim WW_READ As Boolean = False
                Dim titleKbnVal As String = ""
                Dim fieldVal As String = ""
                Dim excelFileVal As String = ""
                Dim effectVal As String = ""
                Dim fieldnamesVal As String = ""
                Dim structVal As String = ""
                Dim posicolVal As Integer = 0
                Dim posirowVal As Integer = 0
                Dim widthVal As Integer = 0
                Dim sortVal As Integer = 0
                For Each key As String In {PROFID, C_DEFAULT_DATAKEY}
                    PARA2.Value = key

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            titleKbnVal = Convert.ToString(SQLdr("TITLEKBN"))
                            fieldVal = Convert.ToString(SQLdr("FIELD"))
                            effectVal = Convert.ToString(SQLdr("EFFECT"))
                            fieldnamesVal = Convert.ToString(SQLdr("FIELDNAMES"))
                            If IsDBNull(SQLdr("STRUCT")) Then
                                structVal = Space(20)
                            Else
                                structVal = Convert.ToString(SQLdr("STRUCT"))
                            End If
                            posicolVal = CInt(SQLdr("POSICOL"))
                            posirowVal = CInt(SQLdr("POSIROW"))
                            widthVal = CInt(SQLdr("WIDTH"))
                            sortVal = 0
                            If Not IsDBNull(SQLdr("SORTORDER")) Then
                                sortVal = CInt(SQLdr("SORTORDER"))
                            End If
                            Select Case titleKbnVal
                                Case "H"                'ヘッダー領域
                                    If Not IsDBNull(SQLdr("EXCELFILE")) Then
                                        EXCELFILE = Convert.ToString(SQLdr("EXCELFILE"))
                                    End If
                                    POSISTART = CInt(SQLdr("POSISTART"))
                                    PROFID = Convert.ToString(SQLdr("PROFID"))
                                    REPORTID = Convert.ToString(SQLdr("REPORTID"))
                                    HEADWRITE = effectVal

                                Case "T"                'タイトル領域
                                    WW_READ = True
                                    TITLEKBN.Add(titleKbnVal)
                                    FIELD.Add(fieldVal)
                                    FIELDNAME.Add(fieldnamesVal)
                                    STRUCT.Add(structVal)

                                    POSIX.Add(posicolVal)
                                    POSIY.Add(posirowVal)
                                    WIDTH.Add(widthVal)
                                    EFFECT.Add(effectVal)
                                    SORT.Add(sortVal)

                                    If posicolVal > POSI_T_X_MAX Then
                                        POSI_T_X_MAX = posicolVal
                                    End If

                                    If posirowVal > POSI_T_Y_MAX Then
                                        POSI_T_Y_MAX = posirowVal
                                    End If

                                Case "I"                '明細領域
                                    WW_READ = True
                                    TITLEKBN.Add(titleKbnVal)
                                    FIELD.Add(fieldVal)
                                    FIELDNAME.Add(fieldnamesVal)
                                    STRUCT.Add(structVal)

                                    POSIX.Add(posicolVal)
                                    POSIY.Add(posirowVal)
                                    WIDTH.Add(widthVal)
                                    EFFECT.Add(effectVal)
                                    SORT.Add(sortVal)

                                    If posicolVal > POSI_I_X_MAX Then
                                        POSI_I_X_MAX = posicolVal
                                    End If

                                    If posirowVal > POSI_I_Y_MAX Then
                                        POSI_I_Y_MAX = posirowVal
                                    End If

                                Case "I_Data"           '繰返アイテムデータ
                                    WW_READ = True
                                    TITLEKBN.Add(titleKbnVal)
                                    FIELD.Add(fieldVal)
                                    FIELDNAME.Add(fieldnamesVal)
                                    STRUCT.Add(structVal)

                                    POSIX.Add(posicolVal)
                                    POSIY.Add(posirowVal)
                                    WIDTH.Add(widthVal)
                                    EFFECT.Add(effectVal)
                                    SORT.Add(sortVal)

                                    If posicolVal > POSI_R_X_MAX Then
                                        POSI_R_X_MAX = posicolVal
                                    End If

                                    If posirowVal > POSI_R_Y_MAX Then
                                        POSI_R_Y_MAX = posirowVal
                                    End If

                                Case "I_DataKey"        '繰返アイテムキー
                                    WW_READ = True
                                    TITLEKBN.Add(titleKbnVal)
                                    FIELD.Add(fieldVal)
                                    FIELDNAME.Add(fieldnamesVal)
                                    STRUCT.Add(structVal)

                                    POSIX.Add(posicolVal)
                                    POSIY.Add(posirowVal)
                                    WIDTH.Add(widthVal)
                                    EFFECT.Add(effectVal)
                                    SORT.Add(sortVal)
                            End Select

                            'ソート文字列取得
                            If Not (titleKbnVal = "H" OrElse titleKbnVal = "T") Then
                                If Not sortVal = 0 Then
                                    If SORTstr = "" Then
                                        SORTstr &= fieldVal
                                    Else
                                        SORTstr &= " , " & fieldVal
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
            End Using

        Catch ex As Exception
            ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0014_PROFMXLS Select"         '問題発生個所
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                 '異常
            CS0011LOGWrite.TEXT = ex.ToString()                         '例外エラー
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR            'DBエラー
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

End Structure

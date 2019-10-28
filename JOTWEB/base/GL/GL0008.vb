Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 業務車番情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0008WorkLorryList
    Inherits GL0000


    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 権限フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PERMISSION() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' 油種
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OILTYPE() As String
    ''' <summary>
    ''' 出庫日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SHUKODATE() As Date
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"
    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = String.Empty
        End If
        'PARAM EXTRA02: SHUKODATE
        If SHUKODATE < C_DEFAULT_YMD Then
            SHUKODATE = Date.Now
        End If
        'PARAM EXTRA04: OILTYPE
        If IsNothing(OILTYPE) Then
            OILTYPE = String.Empty
        End If
        Try
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try

        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            getWorkLorryAllList(SQLcon)

            SQLcon.Close() 'DataBase接続(Close)
        End Using
    End Sub

    ''' <summary>
    ''' 業務車番全取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Sub getWorkLorryAllList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用車両取得
        '○ User権限によりDB(MA006_SHABANORG)検索
        Try
            Dim sb = New System.Text.StringBuilder()
            '検索SQL文
            sb.Append("SELECT ")
            sb.Append("  rtrim(A.GSHABAN) as GSHABAN ")
            sb.Append("  , CASE isnull(rtrim(A.TSHABANFNAMES), '') ")
            sb.Append("    WHEN '' THEN isnull(rtrim(C.LICNPLTNO1) + rtrim(C.LICNPLTNO2), '') ")
            sb.Append("    ELSE rtrim(A.TSHABANFNAMES) ")
            sb.Append("    END as LICNPLTNOF ")
            sb.Append("  , CASE isnull(rtrim(A.TSHABANBNAMES), '') ")
            sb.Append("    WHEN '' THEN isnull(rtrim(D.LICNPLTNO1) + rtrim(D.LICNPLTNO2), '') ")
            sb.Append("    ELSE rtrim(A.TSHABANBNAMES) ")
            sb.Append("    END as LICNPLTNOB ")
            sb.Append("FROM ")
            sb.Append("  MA006_SHABANORG as A ")
            If Not String.IsNullOrEmpty(OILTYPE) Then
                sb.Append("  INNER JOIN MA002_SHARYOA as B ")
                sb.Append("    ON B.CAMPCODE = A.CAMPCODE ")
                sb.Append("    and B.SHARYOTYPE = A.SHARYOTYPEB ")
                sb.Append("    and B.TSHABAN = B.TSHABANB ")
                sb.Append("    and B.MANGOILTYPE = @P4 ")
                sb.Append("    and B.STYMD <= @P1 ")
                sb.Append("    and B.ENDYMD >= @P1 ")
                sb.Append("    and B.DELFLG <> '1' ")
            End If
            sb.Append("  LEFT JOIN MA004_SHARYOC as C ")
            sb.Append("    ON C.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and C.SHARYOTYPE = A.SHARYOTYPEF ")
            sb.Append("    and C.TSHABAN = A.TSHABANF ")
            sb.Append("    and C.STYMD <= @P1 ")
            sb.Append("    and C.ENDYMD >= @P1 ")
            sb.Append("    and C.DELFLG <> '1' ")
            sb.Append("  LEFT JOIN MA004_SHARYOC as D ")
            sb.Append("    ON D.CAMPCODE = A.CAMPCODE ")
            sb.Append("    and D.SHARYOTYPE = A.SHARYOTYPEB ")
            sb.Append("    and D.TSHABAN = A.TSHABANB ")
            sb.Append("    and D.STYMD <= @P1 ")
            sb.Append("    and D.ENDYMD >= @P1 ")
            sb.Append("    and D.DELFLG <> '1' ")
            sb.Append("WHERE ")
            sb.Append("  A.CAMPCODE = @P2 ")
            If Not String.IsNullOrEmpty(ORGCODE) Then
                sb.Append("  and A.MANGUORG = @P3 ")
            End If
            sb.Append("  and A.DELFLG <> '1' ")

            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE
                    sb.Append("   ORDER BY A.GSHABAN, A.TSHABANFNAMES, A.TSHABANBNAMES, A.SEQ  ")
                Case C_DEFAULT_SORT.NAMES
                    sb.Append("   ORDER BY A.TSHABANFNAMES, A.TSHABANBNAMES, A.GSHABAN, A.SEQ  ")
                Case C_DEFAULT_SORT.SEQ, String.Empty
                    sb.Append("   ORDER BY A.SEQ, A.GSHABAN, A.TSHABANFNAMES, A.TSHABANBNAMES  ")
                Case Else
            End Select

            Using SQLcmd As New SqlCommand(sb.ToString, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

                PARA1.Value = SHUKODATE
                PARA2.Value = CAMPCODE
                PARA3.Value = ORGCODE
                PARA4.Value = OILTYPE

                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                '○出力編集
                While SQLdr.Read
                    Select Case VIEW_FORMAT
                        Case C_VIEW_FORMAT_PATTERN.NAMES
                            If Not IsDBNull(SQLdr("LICNPLTNOF")) Then
                                LIST.Items.Add(New ListItem(SQLdr("LICNPLTNOF"), SQLdr("GSHABAN")))
                            ElseIf Not IsDBNull(SQLdr("LICNPLTNOB")) Then
                                LIST.Items.Add(New ListItem(SQLdr("LICNPLTNOB"), SQLdr("GSHABAN")))
                            Else
                                LIST.Items.Add(New ListItem("", SQLdr("GSHABAN")))
                            End If
                        Case C_VIEW_FORMAT_PATTERN.CODE
                            LIST.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                        Case C_VIEW_FORMAT_PATTERN.BOTH
                            If Not IsDBNull(SQLdr("LICNPLTNOF")) Then
                                LIST.Items.Add(New ListItem(SQLdr("LICNPLTNOF") & "(" & SQLdr("GSHABAN") & ")", SQLdr("GSHABAN")))
                            ElseIf Not IsDBNull(SQLdr("LICNPLTNOB")) Then
                                LIST.Items.Add(New ListItem(SQLdr("LICNPLTNOB") & "(" & SQLdr("GSHABAN") & ")", SQLdr("GSHABAN")))
                            Else
                                LIST.Items.Add(New ListItem("(" & SQLdr("GSHABAN") & ")", SQLdr("GSHABAN")))
                            End If
                    End Select
                End While
                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing

            End Using

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_SHABANORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

End Class


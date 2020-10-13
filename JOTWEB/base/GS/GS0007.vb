Option Strict On
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 固定値リスト取得
''' </summary>
''' <remarks></remarks>
Public Class GS0007FIXVALUElst
    Inherits GS0000
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' クラスコード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE1() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE2() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE3() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE4() As ListBox
    ''' <summary>
    ''' 結果(ListBOX)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VALUE5() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX1() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX2() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX3() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX4() As ListBox
    ''' <summary>
    ''' 固定値1LISTBOX
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX5() As ListBox
    ''' <summary>
    ''' SQL検索条件に含めるテキスト(このまま加える為、SQLインジェクションに注意)
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_CONDITION As String = ""
    ''' <summary>
    ''' SQLのORDER BYの後にしてい未指定時はKEYCODEとなる
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_SORT_ORDER As String = ""
    ''' <summary>
    ''' SQL検索条件(開始～終了)の条件
    ''' </summary>
    ''' <returns></returns>
    Public Property ADDITIONAL_FROM_TO As String = ""
    Protected METHOD_NAME As String = "GS0007FIXVALUElst"

    Public Sub GS0007FIXVALUElst()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        VALUE1 = New ListBox
        VALUE2 = New ListBox
        VALUE3 = New ListBox
        VALUE4 = New ListBox
        VALUE5 = New ListBox

        Try
            If IsNothing(LISTBOX1) Then
                LISTBOX1 = New ListBox
            Else
                LISTBOX1.Items.Clear()
            End If

            If IsNothing(LISTBOX2) Then
                LISTBOX2 = New ListBox
            Else
                LISTBOX2.Items.Clear()
            End If

            If IsNothing(LISTBOX3) Then
                LISTBOX3 = New ListBox
            Else
                LISTBOX3.Items.Clear()
            End If

            If IsNothing(LISTBOX4) Then
                LISTBOX4 = New ListBox
            Else
                LISTBOX4.Items.Clear()
            End If

            If IsNothing(LISTBOX5) Then
                LISTBOX5 = New ListBox
            Else
                LISTBOX5.Items.Clear()
            End If

        Catch ex As Exception
        End Try

        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '初期値設定
        ERR = C_MESSAGE_NO.DLL_IF_ERROR

        '●固定値リスト取得(指定値)
        '○ DB(OIS0015_FIXVALUE)検索

        Dim SQLStr As String = String.Empty
        Try

            'OIS0015_FIXVALUE検索SQL文
            If String.IsNullOrEmpty(CLAS) Then
                SQLStr =
                      " SELECT DISTINCT                  " _
                    & "      rtrim(CLASS)  as KEYCODE , " _
                    & "      rtrim(NAMES)  as VALUE1  , " _
                    & "      rtrim(NAMES)  as VALUE2  , " _
                    & "      rtrim(NAMES)  as VALUE3  , " _
                    & "      rtrim(NAMES)  as VALUE4  , " _
                    & "      rtrim(NAMES)  as VALUE5    " _
                    & " FROM  OIL.VIW0001_FIXVALUE             " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and STYMD    <= @P3 " _
                    & "   and ENDYMD   >= @P4 " _
                    & "   and DELFLG   <> @P5 "
                If ADDITIONAL_CONDITION <> "" Then
                    SQLStr = SQLStr & " " & ADDITIONAL_CONDITION & " "
                End If
                If Me.ADDITIONAL_SORT_ORDER <> "" Then
                    SQLStr = SQLStr & " ORDER BY " & Me.ADDITIONAL_SORT_ORDER & " "
                Else
                    SQLStr = SQLStr & " ORDER BY KEYCODE "
                End If

            Else
                SQLStr =
                      " SELECT                           " _
                    & "      rtrim(KEYCODE) as KEYCODE , " _
                    & "      rtrim(VALUE1)  as VALUE1  , " _
                    & "      rtrim(VALUE2)  as VALUE2  , " _
                    & "      rtrim(VALUE3)  as VALUE3  , " _
                    & "      rtrim(VALUE4)  as VALUE4  , " _
                    & "      rtrim(VALUE5)  as VALUE5    " _
                    & " FROM  OIL.VIW0001_FIXVALUE             " _
                    & " Where CAMPCODE  = @P1 " _
                    & "   and CLASS     = @P2 " _
                    & "   and STYMD    <= @P3 " _
                    & "   and ENDYMD   >= @P4 " _
                    & "   and DELFLG   <> @P5 "
                If ADDITIONAL_CONDITION <> "" Then
                    SQLStr = SQLStr & " " & ADDITIONAL_CONDITION & " "
                End If
                If Me.ADDITIONAL_SORT_ORDER <> "" Then
                    SQLStr = SQLStr & " ORDER BY " & Me.ADDITIONAL_SORT_ORDER & " "
                Else
                    SQLStr = SQLStr & " ORDER BY KEYCODE "
                End If
            End If

            'If String.IsNullOrEmpty(CLAS) Then
            '    SQLStr =
            '          " SELECT DISTINCT                  " _
            '        & "      rtrim(CLASS)  as KEYCODE , " _
            '        & "      rtrim(NAMES)  as VALUE1  , " _
            '        & "      rtrim(NAMES)  as VALUE2  , " _
            '        & "      rtrim(NAMES)  as VALUE3  , " _
            '        & "      rtrim(NAMES)  as VALUE4  , " _
            '        & "      rtrim(NAMES)  as VALUE5    " _
            '        & " FROM  COM.OIS0015_FIXVALUE             " _
            '        & " Where CAMPCODE  = @P1 " _
            '        & "   and STYMD    <= @P3 " _
            '        & "   and ENDYMD   >= @P4 " _
            '        & "   and DELFLG   <> @P5 " _
            '        & " ORDER BY KEYCODE "
            'Else
            '    SQLStr =
            '          " SELECT                           " _
            '        & "      rtrim(KEYCODE) as KEYCODE , " _
            '        & "      rtrim(VALUE1)  as VALUE1  , " _
            '        & "      rtrim(VALUE2)  as VALUE2  , " _
            '        & "      rtrim(VALUE3)  as VALUE3  , " _
            '        & "      rtrim(VALUE4)  as VALUE4  , " _
            '        & "      rtrim(VALUE5)  as VALUE5    " _
            '        & " FROM  COM.OIS0015_FIXVALUE             " _
            '        & " Where CAMPCODE  = @P1 " _
            '        & "   and CLASS     = @P2 " _
            '        & "   and STYMD    <= @P3 " _
            '        & "   and ENDYMD   >= @P4 " _
            '        & "   and DELFLG   <> @P5 " _
            '        & " ORDER BY KEYCODE "
            'End If
            'DataBase接続文字
            Using SQLcon = sm.getConnection,
                  SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                SqlConnection.ClearPool(SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.NVarChar, 20).Value = CAMPCODE
                    .Add("@P2", SqlDbType.NVarChar, 20).Value = CLAS
                    .Add("@P3", SqlDbType.Date).Value = Date.Now
                    .Add("@P4", SqlDbType.Date).Value = Date.Now
                    .Add("@P5", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    Dim val(5) As String
                    Dim keyCode As String = ""
                    While SQLdr.Read
                        keyCode = Convert.ToString(SQLdr("KEYCODE"))
                        If keyCode <> "" Then
                            For i As Integer = 1 To 5
                                val(i) = Convert.ToString(SQLdr(String.Format("VALUE{0}", i)))
                            Next
                            VALUE1.Items.Add(New ListItem(val(1), keyCode))
                            VALUE2.Items.Add(New ListItem(val(2), keyCode))
                            VALUE3.Items.Add(New ListItem(val(3), keyCode))
                            VALUE4.Items.Add(New ListItem(val(4), keyCode))
                            VALUE5.Items.Add(New ListItem(val(5), keyCode))

                            LISTBOX1.Items.Add(New ListItem(val(1), keyCode))
                            LISTBOX2.Items.Add(New ListItem(val(2), keyCode))
                            LISTBOX3.Items.Add(New ListItem(val(3), keyCode))
                            LISTBOX4.Items.Add(New ListItem(val(4), keyCode))
                            LISTBOX5.Items.Add(New ListItem(val(5), keyCode))
                        End If
                    End While
                End Using 'SQLdr
                ERR = C_MESSAGE_NO.NORMAL
            End Using 'SQLcon,SQLcmd
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '●固定値リスト取得(デフォルト値)
        '○ DB(OIS0015_FIXVALUE)検索
        If VALUE1.Items.Count = 0 Then
            Try
                'DataBase接続文字
                Using SQLcon = sm.getConnection,
                      SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    SqlConnection.ClearPool(SQLcon)
                    With SQLcmd.Parameters
                        .Add("@P1", SqlDbType.NVarChar, 20).Value = C_DEFAULT_DATAKEY
                        .Add("@P2", SqlDbType.NVarChar, 20).Value = CLAS
                        .Add("@P3", SqlDbType.Date).Value = Date.Now
                        .Add("@P4", SqlDbType.Date).Value = Date.Now
                        .Add("@P5", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE
                    End With
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        Dim val(5) As String
                        Dim keyCode As String = ""

                        While SQLdr.Read
                            keyCode = Convert.ToString(SQLdr("KEYCODE"))
                            If keyCode <> "" Then

                                For i As Integer = 1 To 5
                                    val(i) = Convert.ToString(SQLdr(String.Format("VALUE{0}", i)))
                                Next
                                VALUE1.Items.Add(New ListItem(val(1), keyCode))
                                VALUE2.Items.Add(New ListItem(val(2), keyCode))
                                VALUE3.Items.Add(New ListItem(val(3), keyCode))
                                VALUE4.Items.Add(New ListItem(val(4), keyCode))
                                VALUE5.Items.Add(New ListItem(val(5), keyCode))

                                LISTBOX1.Items.Add(New ListItem(val(1), keyCode))
                                LISTBOX2.Items.Add(New ListItem(val(2), keyCode))
                                LISTBOX3.Items.Add(New ListItem(val(3), keyCode))
                                LISTBOX4.Items.Add(New ListItem(val(4), keyCode))
                                LISTBOX5.Items.Add(New ListItem(val(5), keyCode))
                            End If
                        End While
                    End Using

                    ERR = C_MESSAGE_NO.NORMAL
                End Using 'SQLcon, SQLcmd
            Catch ex As Exception
                Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select DEFAULT"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                ERR = C_MESSAGE_NO.DB_ERROR
            End Try
        End If

    End Sub
    ''' <summary>
    ''' FixValueより取得したテーブルを返却
    ''' </summary>
    ''' <returns>DataTable </returns>
    Public Function GS0007FIXVALUETbl() As DataTable
        Dim retDt As DataTable = Nothing
        '●In PARAMチェック
        'PARAM01: CLAS
        If checkParam(METHOD_NAME, CLAS) <> C_MESSAGE_NO.NORMAL Then
            Throw New Exception(String.Format("CLAS Name Undefine CLAS={0}", CLAS))
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '初期値設定
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        Dim sqlStat As New StringBuilder
        'SQL文字の生成
        If String.IsNullOrEmpty(CLAS) Then
            '使うか不明で、Datatableを返却するためリストボックスと違い同じ規則性不要
            sqlStat.AppendLine("SELECT DISTINCT")
            sqlStat.AppendLine("      ,rtrim(CLASS)  AS CLASS")
            sqlStat.AppendLine("      ,rtrim(NAMES)  AS NAMES")
            sqlStat.AppendLine("  FROM  OIL.VIW0001_FIXVALUE")
            sqlStat.AppendLine(" WHERE  CAMPCODE   = @CAMPCODE")
            sqlStat.AppendLine("   AND  STYMD     <= @STYMD")
            sqlStat.AppendLine("   AND  ENDYMD    >= @ENDYMD")
            sqlStat.AppendLine("   AND  DELFLG    <> @DELFLG")
            If ADDITIONAL_CONDITION <> "" Then
                sqlStat.AppendLine(ADDITIONAL_CONDITION)
            End If
            If Me.ADDITIONAL_SORT_ORDER <> "" Then
                sqlStat.AppendLine(" ORDER BY " & Me.ADDITIONAL_SORT_ORDER)
            Else
                sqlStat.AppendLine(" ORDER BY KEYCODE")
            End If
        Else
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       rtrim(isnull(KEYCODE,''))  AS KEYCODE")
            For Each fieldName In {"VALUE1", "VALUE2", "VALUE3", "VALUE4", "VALUE5",
                                       "VALUE6", "VALUE7", "VALUE8", "VALUE9", "VALUE10",
                                       "VALUE11", "VALUE12", "VALUE13", "VALUE14", "VALUE15"}
                sqlStat.AppendFormat("      ,rtrim(isnull({0},''))   AS {0}", fieldName).AppendLine()
            Next fieldName
            sqlStat.AppendLine("      ,rtrim(isnull(NAMES,''))  AS NAMES")
            sqlStat.AppendLine("      ,rtrim(isnull(NAMEL,''))  AS NAMEL")
            sqlStat.AppendLine("      ,SYSTEMKEYFLG  AS SYSTEMKEYFLG")
            sqlStat.AppendLine("  FROM  OIL.VIW0001_FIXVALUE")
            sqlStat.AppendLine(" WHERE  CAMPCODE   = @CAMPCODE")
            sqlStat.AppendLine("   AND  CLASS      = @CLASS")
            sqlStat.AppendLine("   AND  STYMD     <= @STYMD")
            sqlStat.AppendLine("   AND  ENDYMD    >= @ENDYMD")
            sqlStat.AppendLine("   AND  DELFLG    <> @DELFLG")
            If ADDITIONAL_CONDITION <> "" Then
                sqlStat.AppendLine(ADDITIONAL_CONDITION)
            End If
            If Me.ADDITIONAL_SORT_ORDER <> "" Then
                sqlStat.AppendLine(" ORDER BY " & Me.ADDITIONAL_SORT_ORDER)
            Else
                sqlStat.AppendLine(" ORDER BY KEYCODE")
            End If
        End If

        Try
            'DataBase接続文字
            Using sqlCon = sm.getConnection,
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                SqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = CLAS
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = C_DELETE_FLG.DELETE

                End With
                Dim paramCampCode = sqlCmd.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar, 20)
                'パラメータのCOMPCODE、なければCOMPCODE"Default"で検索
                For Each campVal As String In {CAMPCODE, C_DEFAULT_DATAKEY}
                    paramCampCode.Value = campVal
                    Using sqlDa As New SqlDataAdapter(sqlCmd)
                        retDt = New DataTable
                        sqlDa.Fill(retDt)
                    End Using
                    'レコードがある場合はCOMPCODE="Default"で検索しない
                    If retDt IsNot Nothing AndAlso retDt.Rows.Count > 0 Then
                        Exit For
                    End If
                Next campVal
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0011_UPROFXLS Select DEFAULT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
        End Try
        Return retDt
    End Function
End Class

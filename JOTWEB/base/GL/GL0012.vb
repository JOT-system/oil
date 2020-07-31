Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' ロール情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0012RoleList
    Inherits GL0000
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' ROLENAME
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLENAME() As String
    ''' <summary>
    ''' OBJCODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OBJCODE() As String
    ''' <summary>
    ''' CAMPCODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' CLAS
    ''' </summary>
    ''' <remarks></remarks>
    Public Property CLAS() As String
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' 会社情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        'PARAM 01: OBJCODE
        If checkParam(METHOD_NAME, OBJCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM 02: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM EXTRA01: STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA02: ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
            ENDYMD = Date.Now
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
            SqlConnection.ClearPool(SQLcon)
            getRoleList(SQLcon)

        End Using
    End Sub
    ''' <summary>
    ''' ロール一覧取得
    ''' </summary>
    Protected Sub getRoleList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(OIS0009_ROLE)検索
        Try

            Dim SQLStr As String =
                    " SELECT DISTINCT                 " &
                    "         ROLE          as ROLE   , " &
                    "         ROLENAME      as ROLENAME " &
                    " FROM COM.OIS0009_ROLE             " &
                    " WHERE CAMPCODE        = @P1       " &
                    "   AND   STYMD        <= @P2       " &
                    "   AND   ENDYMD       >= @P3       " &
                    "   AND   OBJECT        = @P4       " &
                    "   AND   DELFLG       <> @P5       " &
                    "   ORDER BY ROLE , ROLENAME  "

            '〇ソート条件追加
            'Select Case DEFAULT_SORT
            '    Case C_DEFAULT_SORT.CODE, String.Empty
            '        SQLStr = SQLStr & " ORDER BY CODE , ROLENAME , SEQ "
            '    Case C_DEFAULT_SORT.NAMES
            '        SQLStr = SQLStr & " ORDER BY CODENAMES , ROLE , SEQ "
            '    Case C_DEFAULT_SORT.SEQ
            '        SQLStr = SQLStr & " ORDER BY SEQ , ROLE , ROLENAME "
            '    Case Else
            'End Select

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                With SQLcmd.Parameters
                    .Add("@P1", SqlDbType.VarChar, 20).Value = CAMPCODE
                    .Add("@P2", SqlDbType.Date).Value = Date.Now
                    .Add("@P3", SqlDbType.Date).Value = Date.Now
                    .Add("@P4", SqlDbType.VarChar, 20).Value = OBJCODE
                    .Add("@P5", SqlDbType.VarChar, 20).Value = C_DELETE_FLG.DELETE
                End With
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        LIST.Items.Add(New ListItem(Convert.ToString(SQLdr("ROLENAME")), Convert.ToString(SQLdr("ROLE"))))
                    End While
                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using
            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0012"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:OIS0009_ROLE Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Class


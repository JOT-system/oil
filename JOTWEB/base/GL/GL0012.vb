﻿Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

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
        'PARAM 01: ROLECODE
        If checkParam(METHOD_NAME, ROLECODE) Then
            Exit Sub
        End If
        'PARAM 02: OBJCODE
        If checkParam(METHOD_NAME, OBJCODE) Then
            Exit Sub
        End If
        'PARAM 03: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) Then
            Exit Sub
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < C_DEFAULT_YMD Then
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

            getRoleList(SQLcon)

        End Using
    End Sub
    ''' <summary>
    ''' ロール一覧取得
    ''' </summary>
    Protected Sub getRoleList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(S0006_ROLE)検索
        Try

            Dim SQLStr As String = _
                    " SELECT                 " &
                    "         CODE          as CODE   , " &
                    "         CODENAMES     as NAMES    " & _
                    " FROM S0006_ROLE                   " & _
                    " WHERE CAMPCODE        = @P1       " & _
                    "   AND   STYMD        <= @P3       " & _
                    "   AND   ENDYMD       >= @P2       " & _
                    "   AND   OBJECT        = @P4       " & _
                    "   AND   ROLE          = @P5       " & _
                    "   AND   DELFLG       <> '1'       "
            '〇ソート条件追加
            Select Case DEFAULT_SORT
                Case C_DEFAULT_SORT.CODE, String.Empty
                    SQLStr = SQLStr & " ORDER BY CODE , CODENAMES , SEQ "
                Case C_DEFAULT_SORT.NAMES
                    SQLStr = SQLStr & " ORDER BY CODENAMES , CODE , SEQ "
                Case C_DEFAULT_SORT.SEQ
                    SQLStr = SQLStr & " ORDER BY SEQ , CODE , CODENAMES "
                Case Else
            End Select

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.VarChar, 20)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.VarChar, 20)

            PARA1.Value = CAMPCODE
            PARA2.Value = STYMD
            PARA3.Value = ENDYMD
            PARA4.Value = OBJCODE
            PARA5.Value = ROLECODE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()


            While SQLdr.Read
                '○出力編集
                LIST.Items.Add(New ListItem(SQLdr("NAMES"), SQLdr("CODE")))
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0011"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM Select"
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


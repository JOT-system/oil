Option Strict On
Imports System.Data.SqlClient

''' <summary>
''' 基地情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0014PLANTList
    Inherits GL0000
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
        'PARAM 01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
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

            getPlantList(SQLcon)

        End Using
    End Sub
    ''' <summary>
    ''' 基地一覧取得
    ''' </summary>
    Protected Sub getPlantList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(OIM0009_PLANT)検索
        Try

            Dim SQLStr As String =
                    " SELECT DISTINCT                 " &
                    "         PLANTCODE     as PLANTCODE   , " &
                    "         PLANTNAME     as ROLENAME    , " &
                    "         SHIPPERCODE   as SHIPPERCODE " &
                    " FROM OIL.OIM0009_PLANT            " &
                    " WHERE   DELFLG       <> @P1       " &
                    "   ORDER BY PLANTCODE  "

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                SQLcmd.Parameters.Add("@P1", SqlDbType.VarChar, 1).Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        LIST.Items.Add(New ListItem(Convert.ToString(SQLdr("ROLENAME")), Convert.ToString(SQLdr("PLANTCODE"))))
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                End Using

            End Using
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0014"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:(OIM0009_PLANT Select"
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


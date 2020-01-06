Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 貨物駅一覧取得
''' </summary>
''' <remarks></remarks>
Public Class GL0015StationList
    Inherits GL0000
    ''' <summary>
    ''' STATIONCODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STATIONCODE() As String
    ''' <summary>
    ''' STATIONNAME
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STATIONNAME() As String
    ''' <summary>
    ''' DEPARRSTATIONFLG
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DEPARRSTATIONFLG() As String
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
    ''' 貨物駅情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        'PARAM 01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) Then
            Exit Sub
        End If
        'PARAM EXTRA01: STYMD
        If STYMD < C_DEFAULT_YMD Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA02: ENDYMD
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

            getStationList(SQLcon)

        End Using
    End Sub
    ''' <summary>
    ''' 貨物駅一覧取得
    ''' </summary>
    Protected Sub getStationList(ByVal SQLcon As SqlConnection)
        '●Leftボックス用会社取得
        '○ User権限によりDB(VIW0001_FIXVALUE)検索
        Try

            Dim SQLStr As String =
                        " SELECT DISTINCT                 " &
                        "         KEYCODE      as STATIONCODE   , " &
                        "         VALUE1       as STATIONNAME " &
                        " FROM OIL.VIW0001_FIXVALUE             "

            If DEPARRSTATIONFLG <> "" Then
                SQLStr &=
                        " WHERE LEN(CAMPCODE)     = @P0       " &
                        "   AND   RIGHT(CAMPCODE, 1) = @P1       " &
                        "   AND   CLASS              = @P2       " &
                        "   AND   STYMD             <= @P3       " &
                        "   AND   ENDYMD            >= @P4       " &
                        "   AND   DELFLG            <> @P5       " &
                        "   ORDER BY STATIONCODE  "
            Else
                SQLStr &=
                        " WHERE CAMPCODE        = @P1       " &
                        "   AND   CLASS         = @P2       " &
                        "   AND   STYMD        <= @P3       " &
                        "   AND   ENDYMD       >= @P4       " &
                        "   AND   DELFLG       <> @P5       " &
                        "   ORDER BY STATIONCODE  "
            End If

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", System.Data.SqlDbType.VarChar, 1)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.VarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.VarChar, 1)

            PARA0.Value = "7"
            If DEPARRSTATIONFLG <> "" Then
                PARA1.Value = DEPARRSTATIONFLG
            Else
                PARA1.Value = CAMPCODE
            End If
            PARA2.Value = CLAS
            PARA3.Value = Date.Now
            PARA4.Value = Date.Now
            PARA5.Value = C_DELETE_FLG.DELETE
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                LIST.Items.Add(New ListItem(SQLdr("STATIONNAME"), SQLdr("STATIONCODE")))
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing
        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "GL0015"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:VIW0001_FIXVALUE Select"
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


Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRCO0008WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "CO0008S"       'MAPID(選択)
    Public Const MAPID As String = "CO0008"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' VARIANT用リストを作成
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="PROFID"></param>
    ''' <param name="MAPID"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateVariantList(ByVal SQLcon As SqlConnection, ByVal PROFID As String, Optional ByVal MAPID As String = Nothing) As Hashtable

        Dim prmData As New Hashtable
        Dim lst As New ListBox

        'バリアント変数
        Try
            SQLcon.Open()       'DataBase接続(Open)

            '検索SQL文
            '※更新可能USERが操作可能なMAPに紐付く変数を取得
            '※現在の権限にて検索
            '　　★左BOX内容は、名称表示にも利用しているので、権限(参照＋更新)全てを対象とする
            Dim SQLStr As String =
                  " SELECT rtrim(A.MAPID) as MAPID , rtrim(A.VARIANT) as VARIANT , rtrim(A.VARIANTNAMES) as VARIANTNAMES " _
                & " FROM  S0023_PROFMVARI A " _
                & " WHERE (A.PROFID   = @P1 " _
                & "   or  A.PROFID   = '" & C_DEFAULT_DATAKEY & "') " _
                & "   and A.STYMD   <= @P2 " _
                & "   and A.ENDYMD  >= @P3 " _
                & "   and A.DELFLG  <> '1' " _
                & " GROUP BY A.MAPID , A.VARIANT , A.VARIANTNAMES " _
                & " ORDER BY A.MAPID , A.VARIANT "

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)

                PARA1.Value = PROFID
                PARA2.Value = Date.Now
                PARA3.Value = Date.Now

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        If IsNothing(MAPID) OrElse MAPID = SQLdr("MAPID") Then
                            lst.Items.Add(New ListItem(SQLdr("VARIANTNAMES"), SQLdr("MAPID") & "_" & SQLdr("VARIANT")))
                        End If
                    End While
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        Finally
            SQLcon.Close()      'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        prmData.Add(C_PARAMETERS.LP_LIST, lst)
        Return prmData

    End Function

End Class

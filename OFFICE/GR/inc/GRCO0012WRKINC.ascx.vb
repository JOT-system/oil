Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRCO0012WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "CO0012S"           'MAPID(条件)
    Public Const MAPID As String = "CO0012"             'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

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
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 端末IDパラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateTERMIDParam() As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0011TermList.LC_TERM_TYPE.ALL
        prmData.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) = GL0011TermList.C_VIEW_FORMAT_PATTERN.CODE

        Return prmData

    End Function

    ''' <summary>
    ''' 会社一覧パラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateCompParam() As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
        Return prmData
    End Function

    ''' <summary>
    ''' 権限一覧パラメーター
    ''' </summary>
    ''' <param name="I_CAMPCODE"></param>
    ''' <param name="I_SRVOBJECT"></param>
    ''' <param name="I_OBJECT"></param>
    ''' <param name="isList"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateRoleParam(ByVal I_CAMPCODE As String, ByVal I_SRVOBJECT As String, ByVal I_OBJECT As String, Optional ByVal isList As Boolean = False) As Hashtable

        Dim prmData As New Hashtable
        Dim WF_ListBoxROLE As New ListBox

        '○ ロール
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続(Open)

                '検索SQL文
                '※更新可能USERが操作可能なMAPに紐付く変数を取得
                '※現在の権限にて検索
                '　　★左BOX内容は、名称表示にも利用しているので、権限(参照＋更新)全てを対象とする
                ' 固定値マスタのサーバオブジェクトに定義されているOBJECTとロールマスタ内に定義されているOBJECTを含むもの
                Dim SQLStr As String =
                      " SELECT rtrim(A.OBJECT) as OBJECT, rtrim(A.ROLE) as NAMES , MAX(A.PERMITCODE) as CODE " _
                    & " FROM S0006_ROLE A " _
                    & "      INNER JOIN MC001_FIXVALUE B " _
                    & "      ON   B.CLASS = @P1 " _
                    & "      and  B.CAMPCODE=@P5 " _
                    & "      and  B.KEYCODE= A.OBJECT " _
                    & "      and  B.STYMD   <= @P2 " _
                    & "      and  B.ENDYMD  >= @P2 " _
                    & "      and  B.DELFLG  <> '1' " _
                    & " WHERE  A.STYMD   <= @P2 " _
                    & "   and  A.ENDYMD  >= @P3 " _
                    & "   and  A.DELFLG  <> '1' "

                If Not String.IsNullOrEmpty(I_OBJECT) Then SQLStr = SQLStr & " and A.OBJECT='" & I_OBJECT & "' "

                SQLStr = SQLStr _
                    & "GROUP BY A.OBJECT, A.ROLE " _
                    & "ORDER BY A.OBJECT, A.ROLE "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar)

                    PARA1.Value = I_SRVOBJECT
                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = Date.Now
                    PARA5.Value = I_CAMPCODE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            If isList Then
                                WF_ListBoxROLE.Items.Add(New ListItem(SQLdr("CODE"), SQLdr("NAMES")))
                            Else
                                WF_ListBoxROLE.Items.Add(New ListItem(SQLdr("OBJECT") & " " & SQLdr("NAMES"), SQLdr("CODE") & SQLdr("NAMES")))
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        prmData.Add(C_PARAMETERS.LP_LIST, WF_ListBoxROLE)
        Return prmData

    End Function

End Class

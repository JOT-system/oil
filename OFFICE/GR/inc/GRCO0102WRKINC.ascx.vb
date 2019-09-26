Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRCO0102WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "CO0102S"           'MAPID(条件)
    Public Const MAPID As String = "CO0102"             'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' 一覧表示選択情報
    ''' </summary>
    Public Class C_LIST_FUNSEL_DEFAULT
        'DEFAULTあり
        Public Const VISIBLE As String = "1"
        'DEFAULTなし
        Public Const INVISIBLE As String = "2"
    End Class

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 画面IDパラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_USERROLE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateMAPIDParam(ByVal I_COMPCODE As String, ByVal I_USERROLE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_LIST) = GetMAPIDData(I_COMPCODE, I_USERROLE)

        CreateMAPIDParam = prmData

    End Function

    ''' <summary>
    ''' 画面IDリスト取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_USERROLE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetMAPIDData(ByVal I_COMPCODE As String, ByVal I_USERROLE As String) As ListBox

        Dim MapList As New ListBox

        '○ 画面IDリストボックス作成
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                Dim SQLStr As String =
                      " SELECT" _
                    & "    RTRIM(S006.CODE)        AS MAPID" _
                    & "    , RTRIM(S006.CODENAMES) AS MAPNAMES" _
                    & " FROM" _
                    & "    S0006_ROLE S006" _
                    & "    INNER JOIN S0009_URL S009" _
                    & "        ON  S009.MAPID   = S006.CODE" _
                    & "        AND S009.STYMD  <= @P4" _
                    & "        AND S009.ENDYMD >= @P4" _
                    & "        AND S009.DELFLG <> @P5" _
                    & " WHERE" _
                    & "    S006.CAMPCODE    = @P1" _
                    & "    AND S006.OBJECT  = @P2" _
                    & "    AND S006.ROLE    = @P3" _
                    & "    AND S006.STYMD  <= @P4" _
                    & "    AND S006.ENDYMD >= @P4" _
                    & "    AND S006.DELFLG <> @P5" _
                    & " ORDER BY" _
                    & "    S006.CODE" _
                    & "    , S006.CODENAMES"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'オブジェクト
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'ロール
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '現在日付
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                    PARA1.Value = I_COMPCODE
                    PARA2.Value = C_ROLE_VARIANT.USER_PERTMIT
                    PARA3.Value = I_USERROLE
                    PARA4.Value = Date.Now
                    PARA5.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            '実行画面以外は対象外
                            If Right(SQLdr("MAPID"), 1) <> "S" AndAlso
                                Right(SQLdr("MAPID"), 3) <> "S_R" AndAlso
                                SQLdr("MAPID") <> "M00001" Then
                                MapList.Items.Add(New ListItem(SQLdr("MAPNAMES"), SQLdr("MAPID")))
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        GetMAPIDData = MapList

    End Function

End Class

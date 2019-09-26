Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRCO0004WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "CO0004S"           'MAPID(条件)
    Public Const MAPID As String = "CO0004"             'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 所属部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>役員、支店、部、車庫</remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.OFFICE_PLACE,
            GL0002OrgList.C_CATEGORY_LIST.OFFICER}

        CreateORGParam = prmData

    End Function

    ''' <summary>
    ''' 従業員パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Public Function CreateStaffCodeParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ALL

        CreateStaffCodeParam = prmData

    End Function

    ''' <summary>
    ''' 固定値マスタ検索
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_SUBCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateFixValueParam(ByVal I_COMPCODE As String, ByVal I_CLASS As String, Optional ByVal I_SUBCODE As String = "") As Hashtable

        Dim prmData As New Hashtable
        Dim FixValueList As New ListBox

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '○権限リストボックス作成
                Dim SQLStr As String =
                      " SELECT" _
                    & "    RTRIM(KEYCODE) AS KEYCODE" _
                    & "    , RTRIM(VALUE1) AS VALUE1" _
                    & "    , RTRIM(VALUE2) AS VALUE2" _
                    & "    , RTRIM(VALUE3) AS VALUE3" _
                    & "    , RTRIM(VALUE4) AS VALUE4" _
                    & "    , RTRIM(VALUE5) AS VALUE5" _
                    & " FROM" _
                    & "    MC001_FIXVALUE" _
                    & " WHERE" _
                    & "    CAMPCODE    = @P1" _
                    & "    AND CLASS   = @P2" _
                    & "    AND STYMD  <= @P3" _
                    & "    AND ENDYMD >= @P3" _
                    & "    AND DELFLG <> @P4"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '分類
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '現在日付
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1)         '削除フラグ

                    PARA1.Value = I_COMPCODE
                    PARA2.Value = I_CLASS
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            If String.IsNullOrEmpty(I_SUBCODE) Then
                                FixValueList.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
                            Else
                                FixValueList.Items.Add(New ListItem(SQLdr("VALUE" & I_SUBCODE), SQLdr("VALUE1")))
                            End If
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        prmData.Item(C_PARAMETERS.LP_LIST) = FixValueList
        CreateFixValueParam = prmData

    End Function

End Class

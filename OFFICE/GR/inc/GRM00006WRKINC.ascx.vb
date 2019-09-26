Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRM00006WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "M00006S"           'MAPID(条件)
    Public Const MAPID As String = "M00006"             'MAPID(実行)

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' ユーザーIDパラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateUserIDParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim UserList As New ListBox

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                '○ ユーザーIDリストボックス作成
                Dim SQLStr As String =
                      " SELECT" _
                    & "    RTRIM(S004.USERID)       AS USERID" _
                    & "    , RTRIM(S004.STAFFNAMEL) AS USERNAME" _
                    & " FROM" _
                    & "    S0005_AUTHOR S005" _
                    & "    INNER JOIN S0006_ROLE S006" _
                    & "        ON  S006.CAMPCODE = S005.CAMPCODE" _
                    & "        AND S006.OBJECT   = S005.OBJECT" _
                    & "        AND S006.ROLE     = S005.ROLE" _
                    & "        AND S006.STYMD   <= @P4" _
                    & "        AND S006.ENDYMD  >= @P4" _
                    & "        AND S006.DELFLG  <> @P5" _
                    & "    INNER JOIN S0004_USER S004" _
                    & "        ON  S004.USERID   = S006.CODE" _
                    & "        AND S004.STYMD   <= @P4" _
                    & "        AND S004.ENDYMD  >= @P4" _
                    & "        AND S004.DELFLG  <> @P5" _
                    & " WHERE" _
                    & "    S005.CAMPCODE   IN (@P1, '" & C_DEFAULT_DATAKEY & "')" _
                    & "    AND S005.USERID  = @P2" _
                    & "    AND S005.OBJECT  = @P3" _
                    & "    AND S005.STYMD  <= @P4" _
                    & "    AND S005.ENDYMD >= @P4" _
                    & "    AND S005.DELFLG <> @P5" _
                    & " GROUP BY" _
                    & "    S004.USERID" _
                    & "    , S004.STAFFNAMEL" _
                    & " ORDER BY" _
                    & "    S004.USERID" _
                    & "    , S004.STAFFNAMEL"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'ユーザーID
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '現在日付
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                    PARA1.Value = I_COMPCODE
                    PARA2.Value = CS0050SESSION.USERID
                    PARA3.Value = C_ROLE_VARIANT.USER_PROFILE
                    PARA4.Value = Date.Now
                    PARA5.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            UserList.Items.Add(New ListItem(SQLdr("USERNAME"), SQLdr("USERID")))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        prmData.Item(C_PARAMETERS.LP_LIST) = UserList
        CreateUserIDParam = prmData

    End Function

    ''' <summary>
    ''' 構造コードパラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStructParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim StructList As New ListBox

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                '○ 構造コードリストボックス作成
                Dim SQLStr As String =
                      " SELECT" _
                    & "    RTRIM(M006.STRUCT)   AS STRUCT" _
                    & "    , RTRIM(M006.USERID) AS USERID" _
                    & "    , RTRIM(M006.OBJECT) AS OBJECT" _
                    & " FROM" _
                    & "    S0005_AUTHOR S005" _
                    & "    INNER JOIN S0006_ROLE S006" _
                    & "        ON  S006.CAMPCODE = S005.CAMPCODE" _
                    & "        AND S006.OBJECT   = S005.OBJECT" _
                    & "        AND S006.ROLE     = S005.ROLE" _
                    & "        AND S006.STYMD   <= @P4" _
                    & "        AND S006.ENDYMD  >= @P4" _
                    & "        AND S006.DELFLG  <> @P5" _
                    & "    INNER JOIN M0006_STRUCT M006" _
                    & "        ON  M006.USERID   = S006.CODE" _
                    & " WHERE" _
                    & "    S005.CAMPCODE   IN (@P1, '" & C_DEFAULT_DATAKEY & "')" _
                    & "    AND S005.USERID IN (@P2, '" & C_DEFAULT_DATAKEY & "')" _
                    & "    AND S005.OBJECT  = @P3" _
                    & "    AND S005.STYMD  <= @P4" _
                    & "    AND S005.ENDYMD >= @P4" _
                    & "    AND S005.DELFLG <> @P5" _
                    & " GROUP BY" _
                    & "    M006.STRUCT" _
                    & "    , M006.USERID" _
                    & "    , M006.OBJECT" _
                    & " ORDER BY" _
                    & "    M006.STRUCT" _
                    & "    , M006.USERID" _
                    & "    , M006.OBJECT"

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        'ユーザーID
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '現在日付
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         '削除フラグ

                    PARA1.Value = I_COMPCODE
                    PARA2.Value = CS0050SESSION.USERID
                    PARA3.Value = C_ROLE_VARIANT.USER_PROFILE
                    PARA4.Value = Date.Now
                    PARA5.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            StructList.Items.Add(New ListItem(SQLdr("USERID") & "-" & SQLdr("OBJECT") & "-" & SQLdr("STRUCT"), SQLdr("STRUCT")))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
        End Try

        prmData.Item(C_PARAMETERS.LP_LIST) = StructList
        CreateStructParam = prmData

    End Function

    ''' <summary>
    ''' コードパラメーター
    ''' </summary>
    ''' <param name="IO_LIST"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_OBJECT"></param>
    ''' <returns></returns>
    ''' <remarks>画面のオブジェクトによりリストの内容を変える</remarks>
    Public Function CreateCodeParam(ByRef IO_LIST As LIST_BOX_CLASSIFICATION, ByVal I_COMPCODE As String, ByVal I_OBJECT As String) As Hashtable

        Dim prmData As New Hashtable
        Dim CodeList As New ListBox

        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection()
                SQLcon.Open()       'DataBase接続

                prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE

                Select Case I_OBJECT
                    Case "ARTTICLE1"        '品名1
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILTYPE"

                    Case "ARTTICLE2"        '品名2
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "PRODUCT1"

                    Case "CAMP"             '会社
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_COMPANY

                    Case "CUSTOMER"         '荷主
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "LORRY"            '業務車番
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST

                        Dim SQLStr As String =
                              " SELECT" _
                            & "    RTRIM(GSHABAN) AS GSHABAN" _
                            & " FROM" _
                            & "    MA006_SHABANORG" _
                            & " WHERE" _
                            & "    CAMPCODE    = @P1" _
                            & "    AND DELFLG <> @P2" _
                            & " GROUP BY" _
                            & "    GSHABAN" _
                            & " ORDER BY" _
                            & "    GSHABAN"

                        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)         '削除フラグ

                            PARA1.Value = I_COMPCODE
                            PARA2.Value = C_DELETE_FLG.DELETE

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader
                                While SQLdr.Read
                                    CodeList.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                                End While
                            End Using
                        End Using

                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "MAP"              '画面
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST

                        Dim SQLStr As String =
                              " SELECT" _
                            & "    RTRIM(MAPID)   AS MAPID" _
                            & "    , RTRIM(NAMES) AS MAPNAMES" _
                            & " FROM" _
                            & "    S0009_URL" _
                            & " WHERE" _
                            & "    STYMD      <= @P1" _
                            & "    AND ENDYMD >= @P1" _
                            & "    AND DELFLG <> @P2" _
                            & " ORDER BY" _
                            & "    MAPID"

                        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.Date)                '現在日付
                            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)         '削除フラグ

                            PARA1.Value = Date.Now
                            PARA2.Value = C_DELETE_FLG.DELETE

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader
                                While SQLdr.Read
                                    CodeList.Items.Add(New ListItem(SQLdr("MAPNAMES"), SQLdr("MAPID")))
                                End While
                            End Using
                        End Using

                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "ORG"              '組織
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST

                        Dim SQLStr As String =
                              " SELECT" _
                            & "    RTRIM(ORGCODE) AS ORGCODE" _
                            & "    , RTRIM(NAMES) AS ORGNAMES" _
                            & " FROM" _
                            & "    M0002_ORG" _
                            & " WHERE" _
                            & "    CAMPCODE    = @P1" _
                            & "    AND STYMD  <= @P2" _
                            & "    AND ENDYMD >= @P2" _
                            & "    AND DELFLG <> @P3" _
                            & " ORDER BY" _
                            & "    ORGCODE"

                        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '現在日付
                            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)         '削除フラグ

                            PARA1.Value = I_COMPCODE
                            PARA2.Value = Date.Now
                            PARA3.Value = C_DELETE_FLG.DELETE

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader
                                While SQLdr.Read
                                    CodeList.Items.Add(New ListItem(SQLdr("ORGNAMES"), SQLdr("ORGCODE")))
                                End While
                            End Using
                        End Using

                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "REPORT"           'レポート
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "STAFF"            '従業員
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "USER"             'ユーザー
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST

                        Dim SQLStr As String =
                              " SELECT" _
                            & "    RTRIM(USERID) AS USERID" _
                            & "    , RTRIM(STAFFNAMES) AS USERNAMES" _
                            & " FROM" _
                            & "    S0004_USER" _
                            & " WHERE" _
                            & "    CAMPCODE    = @P1" _
                            & "    AND STYMD  <= @P2" _
                            & "    AND ENDYMD >= @P2" _
                            & "    AND DELFLG <> @P3" _
                            & " ORDER BY" _
                            & "    USERID"

                        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '現在日付
                            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)         '削除フラグ

                            PARA1.Value = I_COMPCODE
                            PARA2.Value = Date.Now
                            PARA3.Value = C_DELETE_FLG.DELETE

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader
                                While SQLdr.Read
                                    CodeList.Items.Add(New ListItem(SQLdr("USERNAMES"), SQLdr("USERID")))
                                End While
                            End Using
                        End Using

                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList

                    Case "VEHICLE"          '統一車番
                        IO_LIST = LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST

                        Dim SQLStr As String =
                              " SELECT" _
                            & "    RTRIM(SHARYOTYPE) AS SHARYOTYPE" _
                            & "    , RTRIM(TSHABAN)  AS TSHABAN" _
                            & " FROM" _
                            & "    MA003_SHARYOB" _
                            & " WHERE" _
                            & "    CAMPCODE    = @P1" _
                            & "    AND STYMD  <= @P2" _
                            & "    AND ENDYMD >= @P2" _
                            & "    AND DELFLG <> @P3" _
                            & " GROUP BY" _
                            & "    SHARYOTYPE" _
                            & "    , TSHABAN"

                        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '現在日付
                            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)         '削除フラグ

                            PARA1.Value = I_COMPCODE
                            PARA2.Value = Date.Now
                            PARA3.Value = C_DELETE_FLG.DELETE

                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader
                                While SQLdr.Read
                                    CodeList.Items.Add(New ListItem(SQLdr("SHARYOTYPE") & SQLdr("TSHABAN"), SQLdr("SHARYOTYPE") & SQLdr("TSHABAN")))
                                End While
                            End Using
                        End Using

                        prmData.Item(C_PARAMETERS.LP_LIST) = CodeList
                    Case Else
                End Select
            End Using
        Catch ex As Exception
        End Try

        CreateCodeParam = prmData

    End Function

End Class

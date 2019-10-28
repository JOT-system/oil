﻿Imports System.Data.SqlClient
Imports System.Web.UI.WebControls


'■Leftボックス用コード取得
''' <summary>
''' Leftボックス用 色々なコード取得
''' </summary>
''' <remarks></remarks>
Public Class GS0017CODEget
    Inherits GS0000
    ''' <summary>
    ''' オブジェクトのコード一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_OBJ_CODE
        ''' <summary>
        ''' 会社コード
        ''' </summary>
        Public Const COMPANYCODE As String = "CAMP"
        ''' <summary>
        ''' ユーザーID
        ''' </summary>
        Public Const USERSID As String = "USER"
        ''' <summary>
        ''' 組織コード
        ''' </summary>
        Public Const ORGANIZATIONCODE As String = "ORG"
        ''' <summary>
        ''' 社員コード
        ''' </summary>
        Public Const STAFFCODE As String = "STAFF"
        ''' <summary>
        ''' 画面コード
        ''' </summary>
        Public Const SCREENSID As String = "MAP"
        ''' <summary>
        ''' 車両番号
        ''' </summary>
        Public Const LORRYNUMBER As String = "LORRY"
        ''' <summary>
        ''' 統一車番
        ''' </summary>
        Public Const VEHICLENUMBER As String = "VEHICLE"
        ''' <summary>
        ''' 荷主コード
        ''' </summary>
        Public Const CLIENTSCODE As String = "CLIENT"
    End Class
    ''' <summary>
    ''' オブジェクト
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks><para>CAMP:会社コード</para>
    ''' <para>USER:ユーザーID</para>
    ''' <para>ORG:組織コード</para>
    ''' <para>MAP:画面コード</para>
    ''' <para>STAFF:社員コード</para>
    ''' <para>LORRY:車両番号</para>
    ''' <para>VEHICLE:統一車番</para>
    ''' <para>CLIENT:荷主コード</para>
    ''' </remarks>
    Public Property OBJ() As String
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMP() As String
    ''' <summary>
    ''' 運用部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property UORG() As String
    ''' <summary>
    ''' コード一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CODE() As List(Of String)
    ''' <summary>
    ''' 名称一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAME() As List(Of String)
    ''' <summary>
    ''' 情報一覧
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LISTBOX() As ListBox

    Protected METHOD_NAME As String = "GS0017CODEget"
    ''' <summary>
    ''' 指定されたコードと名称の一覧を取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GS0017CODEget()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理
        CODE = New List(Of String)
        NAME = New List(Of String)
        Dim OBJTYPE As String = ""
        Dim SQLStr As String = ""
        'セッション制御宣言
        Dim sm As New CS0050SESSION
        'DataBase接続文字
        Dim SQLcon = sm.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Try

            Select Case OBJ

                Case C_OBJ_CODE.SCREENSID           '画面

                    '●Leftボックス用画面取得
                    OBJTYPE = "1"
                    SQLStr = _
                            "  SELECT                                 " _
                        & "         rtrim(A.MAPID)      as CODE ,   " _
                        & "         rtrim(A.NAMES)      as NAME     " _
                        & "    FROM S0009_URL A                     " _
                        & "   Where A.STYMD     <= @P1              " _
                        & "     and A.ENDYMD    >= @P1              " _
                        & "     and A.DELFLG    <> '1'              "

                Case C_OBJ_CODE.ORGANIZATIONCODE           '組織

                    '●Leftボックス用組織取得
                    OBJTYPE = "1"
                    SQLStr = _
                            "  SELECT                                 " _
                        & "         rtrim(A.ORGCODE)    as CODE ,   " _
                        & "         rtrim(A.NAMES)      as NAME     " _
                        & "    FROM M0002_ORG A                     " _
                        & "   Where A.CAMPCODE   = @P2              " _
                        & "     and A.STYMD     <= @P1              " _
                        & "     and A.ENDYMD    >= @P1              " _
                        & "     and A.DELFLG    <> '1'              "

                Case C_OBJ_CODE.USERSID          'ユーザ

                    '●Leftボックス用ユーザ取得
                    OBJTYPE = "1"
                    SQLStr = _
                            "  SELECT                                 " _
                        & "         rtrim(A.USERID)     as CODE ,   " _
                        & "         rtrim(A.STAFFNAMES) as NAME     " _
                        & "    FROM S0004_USER A                    " _
                        & "   Where A.CAMPCODE   = @P2              " _
                        & "     and A.STYMD     <= @P1              " _
                        & "     and A.ENDYMD    >= @P1              " _
                        & "     and A.DELFLG    <> '1'              "

                Case C_OBJ_CODE.COMPANYCODE          '会社

                    '●Leftボックス用会社取得
                    OBJTYPE = "1"
                    SQLStr = _
                            "  SELECT                                 " _
                        & "         rtrim(A.CAMPCODE)   as CODE ,   " _
                        & "         rtrim(A.NAMES)      as NAME     " _
                        & "    FROM M0001_CAMP A                    " _
                        & "   Where A.STYMD     <= @P1              " _
                        & "     and A.ENDYMD    >= @P1              " _
                        & "     and A.DELFLG    <> '1'              "

                Case C_OBJ_CODE.STAFFCODE          '従業員

                    '●Leftボックス用従業員取得
                    'OBJTYPE = "1"
                    'SQLStr = _


                Case C_OBJ_CODE.LORRYNUMBER          '車両

                    '●Leftボックス用車両取得
                    OBJTYPE = "2"
                    If String.IsNullOrEmpty(UORG) Then
                        SQLStr = _
                            " SELECT rtrim(A.GSHABAN)     as GSHABAN 	" _
                        & "   FROM MA006_SHABANORG A 	          		" _
                        & "  Where A.CAMPCODE   = @P2         	    " _
                        & "    and A.DELFLG    <> '1'          	    " _
                        & "  GROUP BY A.GSHABAN                 	"
                    Else
                        SQLStr = _
                            " SELECT rtrim(A.GSHABAN)     as GSHABAN 	" _
                        & "   FROM MA006_SHABANORG A 	          		" _
                        & "  Where A.CAMPCODE   = @P2         	    " _
                        & "    and A.MANGUORG   = @P3     		    " _
                        & "    and A.DELFLG    <> '1'          	    " _
                        & "  GROUP BY A.GSHABAN                 	"
                    End If

                Case C_OBJ_CODE.VEHICLENUMBER          '統一車番

                    '●Leftボックス用統一車番取得
                    OBJTYPE = "3"
                    SQLStr = _
                            " SELECT rtrim(A.SHARYOTYPE) 	as SHARYOTYPE ,	" _
                        & "        rtrim(B.TSHABAN) 	as TSHABAN  	" _
                        & "   FROM MA003_SHARYOB A 	          			" _
                        & "  Where A.CAMPCODE   = @P1         			" _
                        & "    and A.STYMD     <= @P2 					" _
                        & "    and A.ENDYMD    >= @P2 					" _
                        & "    and A.DELFLG    <> '1'          			" _
                        & " GROUP BY A.SHARYOTYPE , B.TSHABAN           "

                Case C_OBJ_CODE.CLIENTSCODE          '荷主

                    '●Leftボックス用荷主取得
                    OBJTYPE = "1"
                    SQLStr = _
                            " SELECT rtrim(A.TORICODE)    as CODE ,   " _
                        & "        rtrim(A.NAMES)       as NAME     " _
                        & "   FROM MC002_TORIHIKISAKI A             " _
                        & "  INNER JOIN MC003_TORIORG B             " _
                        & "     ON B.TORICODE   = A.TORICODE        " _
                        & "    and B.CAMPCODE   = @P2               " _
                        & "    and B.TORITYPE02 = 'NI'              " _
                        & "    and B.DELFLG    <> '1'               " _
                        & "  Where A.STYMD     <= @P1               " _
                        & "    and A.ENDYMD    >= @P1               " _
                        & "    and A.DELFLG    <> '1'               " _
                        & "  GROUP BY A.TORICODE , A.NAMES          "

                Case Else
                    Exit Sub
            End Select

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            PARA1.Value = Date.Now
            PARA2.Value = CAMP
            PARA3.Value = UORG
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            Try
                If IsNothing(LISTBOX) Then
                    LISTBOX = New ListBox
                Else
                    CType(LISTBOX, ListBox).Items.Clear()
                End If
            Catch ex As Exception
            End Try

            While SQLdr.Read

                '○出力編集
                '■■■　画面、組織、ユーザ、会社、荷主　■■■
                If OBJTYPE = "1" Then
                    CODE.Add(SQLdr("CODE"))
                    NAME.Add(SQLdr("NAME"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("NAME"), SQLdr("CODE")))
                End If

                '■■■　車両　■■■
                If OBJTYPE = "2" Then
                    CODE.Add(SQLdr("GSHABAN"))
                    NAME.Add(SQLdr("GSHABAN"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("GSHABAN"), SQLdr("GSHABAN")))
                End If

                '■■■　統一車番　■■■
                If OBJTYPE = "3" Then
                    CODE.Add(SQLdr("SHARYOTYPE") & SQLdr("TSHABAN"))
                    NAME.Add(SQLdr("SHARYOTYPE") & SQLdr("TSHABAN"))
                    LISTBOX.Items.Add(New ListItem(SQLdr("SHARYOTYPE") & SQLdr("TSHABAN"), SQLdr("SHARYOTYPE") & SQLdr("TSHABAN")))
                End If
            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0008_GROUP Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL
    End Sub

End Class

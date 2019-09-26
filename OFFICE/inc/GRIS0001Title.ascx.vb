Imports System.Drawing
Imports System.Data.SqlClient

Public Class GRIS0001Title
    Inherits UserControl

    Private Const MENUID As String = "M00001"
    Private Const LOGONID As String = "M00000"
    Private Const SCHEDULEID As String = "MB0006"

    Private Const CONFIG_ENV_KEY As String = "Environment"
    Private Const CONFIG_ENV_TEST As String = "TEST"
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION
    ''' <summary>
    ''' 全画面共通-タイトル設定
    ''' </summary>
    ''' <param name="I_MAPID">画面ID</param>
    ''' <param name="I_MAPVARI"></param>
    ''' <param name="I_USERCOMP">会社コード</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <remarks></remarks>
    Public Sub SetTitle(ByVal I_MAPID As String, ByVal I_MAPVARI As String, ByVal I_USERCOMP As String, ByRef O_RTN As String, Optional ByVal I_USERID As String = Nothing)
        'Dim CS0017RETURNURLget As New CS0017RETURNURLget    '画面戻先URL取得（旧仕様対応）
        Dim CS0017ForwardURL As New CS0017ForwardURL        '画面遷移先情報取得
        Dim GS0001CAMPget As New GS0001CAMPget              '会社情報取得
        Dim CS0015TITLEcamp As New CS0015TITLEcamp          '会社コード取得
        Dim CS0050Session As New CS0050SESSION

        '初期化
        O_RTN = C_MESSAGE_NO.NORMAL
        'ID、表題設定
        WF_TITLEID.Text = "ID: " & I_MAPID
        If I_MAPID = LOGONID Then
            'ID、表題設定
            WF_TITLEID.Text = "ID: Logon"
            WF_TITLETEXT.Text = "Welcome to FrontEnd Support System"
            WF_TITLECAMP.Text = ""
            '現在日付設定
            WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")
            'Dim TermClass As String = GetTermClass(O_RTN)
            'If TermClass = C_TERMCLASS.HEAD Then
            '    titlebox.Attributes("class") = "titlebox_HONSHA"
            'Else
            '    titlebox.Attributes("class") = "titlebox"
            'End If
        ElseIf I_MAPID = SCHEDULEID Then
            WF_TITLEID.Text = "ID: MB0006"
            WF_TITLETEXT.Text = "個人スケジュール"
            WF_TITLECAMP.Text = ""
            '現在日付設定
            WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        ElseIf I_MAPID = MENUID Then
            If IsNothing(I_USERID) OrElse I_USERID = "INIT" Then
                Exit Sub
            End If
            Dim WW_FIND As Boolean = False
            Try

                'DataBase接続文字
                Using SQLcon As IDbConnection = CS0050Session.getConnection
                    SQLcon.Open() 'DataBase接続(Open)

                    '検索SQL文
                    Dim SQLStr As String =
                         "SELECT rtrim(A.MAPNAMES) as NAMES " _
                       & " FROM  S0024_PROFMMAP A " _
                       & " Where  " _
                       & "       A.MAPIDP   = @P1 " _
                       & "   and A.VARIANTP = @P2 " _
                       & "   and A.TITLEKBN = 'H' " _
                       & "   and A.STYMD   <= @P3 " _
                       & "   and A.ENDYMD  >= @P4 " _
                       & "   and A.DELFLG  <> @P5 "
                    Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar, 50)
                        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.VarChar, 50)
                        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
                        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.VarChar, 1)
                        PARA1.Value = I_MAPID
                        PARA2.Value = I_MAPVARI
                        PARA3.Value = Date.Now
                        PARA4.Value = Date.Now
                        PARA5.Value = C_DELETE_FLG.DELETE
                        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            If SQLdr.HasRows = True Then
                                While SQLdr.Read
                                    WF_TITLETEXT.Text = SQLdr("NAMES")
                                    WW_FIND = True
                                End While
                            Else
                                WF_TITLETEXT.Text = "業務メニュー"
                                WW_FIND = False
                            End If

                        End Using
                    End Using
                End Using
            Catch ex As Exception
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
            If Not WW_FIND Then
                '自画面MAPID・変数より名称を取得
                CS0017ForwardURL.MAPID = I_MAPID
                CS0017ForwardURL.VARI = I_MAPVARI
                CS0017ForwardURL.getPreviusURL()
                If isNormal(CS0017ForwardURL.ERR) Then
                    '遷移先画面＝子画面　
                    If (I_MAPVARI <> C_DEFAULT_DATAKEY) Then
                        WF_TITLETEXT.Text = CS0017ForwardURL.NAMES
                    Else
                        WF_TITLETEXT.Text = "業務メニュー"
                    End If
                Else
                    '自画面MAPID・変数より名称を取得（旧仕様対応）
                    'CS0017RETURNURLget.MAPID = I_MAPID
                    'CS0017RETURNURLget.VARI = I_MAPVARI
                    'CS0017RETURNURLget.CS0017RETURNURLget()
                    'If isNormal(CS0017RETURNURLget.ERR) Then
                    ''遷移先画面＝子画面　
                    'If (I_MAPVARI <> C_DEFAULT_DATAKEY) Then
                    'WF_TITLETEXT.Text = CS0017RETURNURLget.NAMES
                    'Else
                    'WF_TITLETEXT.Text = "業務メニュー"
                    'End If
                    'Else
                    'O_RTN = CS0017RETURNURLget.ERR
                    Exit Sub
                    'End If
                End If
            End If
        Else
            If IsNothing(I_USERID) OrElse I_USERID = "INIT" Then
                Exit Sub
            End If

            '自画面MAPID・変数より名称を取得
            CS0017ForwardURL.MAPID = I_MAPID
            CS0017ForwardURL.VARI = I_MAPVARI
            CS0017ForwardURL.getPreviusURL()
            If isNormal(CS0017ForwardURL.ERR) Then
                '遷移先画面＝子画面　
                WF_TITLETEXT.Text = CS0017ForwardURL.NAMES
            Else
                '自画面MAPID・変数より名称を取得（旧仕様対応）
                'CS0017RETURNURLget.MAPID = I_MAPID
                'CS0017RETURNURLget.VARI = I_MAPVARI
                'CS0017RETURNURLget.CS0017RETURNURLget()
                'If isNormal(CS0017RETURNURLget.ERR) Then
                'WF_TITLETEXT.Text = CS0017RETURNURLget.NAMES
                'Else
                'O_RTN = CS0017RETURNURLget.ERR
                Exit Sub
                'End If
            End If
        End If

        '会社設定
        'ユーザID設定されている場合はユーザIDから取得する
        If String.IsNullOrEmpty(I_USERID) OrElse Not String.IsNullOrEmpty(I_USERCOMP) Then
            GS0001CAMPget.CAMPCODE = I_USERCOMP
            GS0001CAMPget.STYMD = Date.Now
            GS0001CAMPget.ENDYMD = Date.Now
            GS0001CAMPget.GS0001CAMPget()
            If isNormal(GS0001CAMPget.ERR) Then
                WF_TITLECAMP.Text = GS0001CAMPget.NAMES
            Else
                O_RTN = GS0001CAMPget.ERR
                Exit Sub
            End If
        Else
            Dim complist As ListBox = New ListBox()
            CS0015TITLEcamp.USERID = I_USERID
            CS0015TITLEcamp.List = complist
            CS0015TITLEcamp.CS0015TITLEcamp()
            If CS0015TITLEcamp.ERR = C_MESSAGE_NO.NORMAL Then
                WF_TITLECAMP.Text = complist.SelectedItem.Text
            Else
                O_RTN = GS0001CAMPget.ERR
                Exit Sub
            End If
        End If
        '現在日付設定
        WF_TITLEDATE.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")

        'タイトル部CSS設定
        ' Web.configに[appSettings][key="Environment"]の値により設定
        Select Case ConfigurationManager.AppSettings(CONFIG_ENV_KEY)
            Case CONFIG_ENV_TEST
                titlebox.Attributes("class") = "titlebox_TEST"
            Case Else
                titlebox.Attributes("class") = "titlebox"
        End Select
    End Sub

    ' ******************************************************************************
    ' ***  端末種別取得（全社サーバーか否か判定）                                ***     
    ' ***   2019/09/02利用中止                                                   ***     
    ' ******************************************************************************
    ''' <summary>
    ''' 端末種別取得
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetTermClass(ByRef O_RTN As String) As String
        Dim WW_TermClass As String = ""

        '○ ユーザ
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                        " SELECT TERMCLASS                " &
                        " FROM S0001_TERM                 " &
                        " WHERE TERMID        = @TERMID   " &
                        " AND   STYMD        <= getdate() " &
                        " AND   ENDYMD       >= getdate() " &
                        " AND   DELFLG       <> '1'       "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@TERMID", System.Data.SqlDbType.VarChar, 30)
                    PARA1.Value = CS0050Session.APSV_ID
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            WW_TermClass = SQLdr("TERMCLASS")
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return WW_TermClass
        End Try
        Return WW_TermClass

    End Function
End Class
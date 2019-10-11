Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net

Public Class MB0006SCHEDULE
    Inherits Page

    Private Const MAPID As String = "MB0006"
    '共通宣言
    '*共通関数宣言(BASEDLL)
    Private CS0001INIFILEget As New CS0001INIFILEget                'INIファイル内容取得
    Private CS0050Session As New CS0050SESSION                      'セッション管理
    Private CS0006TERMchk As New CS0006TERMchk                      'ローカルコンピュータ名存在チェック
    Private CS0008ONLINEstat As New CS0008ONLINEstat                'ONLINE状態
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    Private CS0016VARIget As New CS0016ProfMValue                   '変数情報取

    '検索結果格納ds
    Private MB0006tbl As New DataTable                              'Grid格納用テーブル
    Private MB0006INPtbl As New DataTable                           'チェック用テーブル
    Private MB0006UPDtbl As New DataTable                           'デフォルト用テーブル
    Private MB003_HSTAFF As New DataTable                           '更新用テーブル
    Private MB0006row As DataRow                                    '行のロウデータ
    Private MB0006INProw As DataRow                                 '行チェック用のロウデータ
    Private MB0006UPDrow As DataRow                                 'デフォルト用のロウデータ
    Private S0011_UPROFXLSrow As DataRow                            '更新用のロウデータ

    Private WW_RTN As String                                        'サブ用リターンコード
    Private WW_RTN_Detail As String                                 'サブ用リターンコード(項目名)
    Private WW_RTN_Action As String                                 'サブ用リターンコード(重複:Dub , 新規:Insert , 更新:Update)
    '共通処理結果
    Private WW_ERR_RTN As String = String.Empty                     'エラー復帰コード
    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load


        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_SELECT_SW"                  'セレクタ変更
                        FORMdisplay()
                    Case "WF_REP_TEXTchange"             'スケジュール変更
                        If WF_REP_LineCnt.Value = Nothing OrElse WF_REP_ColCnt.Value = Nothing Then
                        Else
                            DBupdate()
                            FORMdisplay()
                        End If
                    Case Else
                End Select

            End If
        Else
            '〇初期化処理
            Initialize()

            '〇メッセージセット
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        End If

        Master.LOGINCOMP = WF_TERMCAMP.Text
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    Protected Sub Initialize()

        'INIファイル取得
        CS0001INIFILEget.CS0001INIFILEget()
        If Not isNormal(CS0001INIFILEget.ERR) Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "システム管理者へ連絡して下さい(INI_File Not Find)")
            Exit Sub
        End If

        '○ APサーバー情報からAPサーバー設置会社(APSRVCamp)、APサーバー設置部署(APSRVOrg)取得
        CS0006TERMchk.TERMID = CS0050Session.APSV_ID
        CS0006TERMchk.CS0006TERMchk()
        If isNormal(CS0006TERMchk.ERR) Then
            CS0050Session.APSV_COMPANY = CS0006TERMchk.TERMCAMP
            CS0050Session.APSV_M_ORG = CS0006TERMchk.TERMORG
            CS0050Session.APSV_ORG = CS0006TERMchk.MORG
        Else
            Master.Output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0006TERMchk")
            Exit Sub
        End If


        '■ オンラインサービス判定 
        '○画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        CS0008ONLINEstat.COMPCODE = CS0050Session.APSV_COMPANY
        CS0008ONLINEstat.CS0008ONLINEstat()
        If isNormal(CS0008ONLINEstat.ERR) AndAlso CS0008ONLINEstat.ONLINESW <> 0 Then
        Else
            Master.Output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0008ONLINEstat")
            Exit Sub
        End If

        '■ 初期設定
        '○メッセージクリア
        Master.MAPID = MAPID
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()
        '〇端末情報の取得
        getTermData(WW_RTN)
        '〇処理日付設定
        '本日設定
        If WF_SELECTYYMM.Value = Nothing Then
            WF_SELECTYYMM.Value = Date.Now.ToString("yyyy年MM月")
            WF_SELECTYYMMDD.Value = Date.Now.ToString("yyyy/MM/dd")
        End If

        '三か月前の月初
        Dim WW_STDATE As Date = New DateTime(Date.Now.Year, Date.Now.Month, 1)
        Selected_STYYMMDD.Value = WW_STDATE.AddMonths(-3).ToString("yyyy/MM/dd")

        '六か月後の月末
        Dim WW_ENDDATE As Date = Date.Now
        WW_ENDDATE = WW_ENDDATE.AddMonths(6)
        Selected_ENDYYMMDD.Value = New DateTime(WW_ENDDATE.Year, WW_ENDDATE.Month, DateTime.DaysInMonth(WW_ENDDATE.Year, WW_ENDDATE.Month)).ToString("yyyy/MM/dd")

        'ユーザID(サブルーチン利用の為のクリア)
        If String.IsNullOrEmpty(CS0050Session.USERID) Then
            HttpContext.Current.Session("Userid") = ""
        End If

        '■ エリア初期値(変数)取得検索
        If WF_SELECTAREA.Value = Nothing Then
            '○エリア初期値(変数)取得
            CS0016VARIget.MAPID = MAPID
            CS0016VARIget.PROFID = C_DEFAULT_DATAKEY
            CS0016VARIget.CAMPCODE = WF_TERMCAMP.Text
            CS0016VARIget.VARI = C_DEFAULT_DATAKEY
            CS0016VARIget.FIELD = CS0050Session.APSV_ID
            CS0016VARIget.getInfo()
            If isNormal(CS0016VARIget.ERR) Then
                WF_SELECTAREA.Value = CS0016VARIget.VALUE
            Else
                Master.Output(CS0006TERMchk.ERR, C_MESSAGE_TYPE.ABORT, "CS0016VARIget")
                Exit Sub
            End If
        End If

        '■ セレクター取得
        '○地域セレクター取得
        INIT_SETECTOR_AREAget()

        '○日付セレクター取得
        INIT_SETECTOR_DATEget()

        '■ 画面表示
        '〇画面表示
        FORMdisplay()
    End Sub
    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonEND_Click() Handles WF_ButtonEND.Click
        'フォームを閉じる

        'アプリケーションを終了する
        Response.Write("<script language='javascript'> { window.close();}</script>")

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' スケジュール情報更新処理
    ''' </summary>
    Protected Sub DBupdate()

        '■ 初期処理
        Dim wTIMSTP As Integer = 0
        Dim wLineCnt As Integer = CInt(WF_REP_LineCnt.Value)
        Dim wColCnt As Integer = CInt(WF_REP_ColCnt.Value)

        '■ 範囲チェック(ユーザ空欄セル入力はエラー)
        If CInt(WF_REP_LineCnt.Value) < 0 OrElse CInt(WF_REP_LineCnt.Value) > (WF_HEADdate_YMD.Items.Count - 1) Then
            'メッセージセット
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If

        If CInt(WF_REP_ColCnt.Value) < 1 OrElse CInt(WF_REP_ColCnt.Value) > WF_HEADuser.Items.Count Then
            'メッセージセット
            Master.Output(C_MESSAGE_NO.FORMAT_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End If
        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection

            '■ スケジュールデータ取得
            Try
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "  SELECT     isnull(rtrim(MB6.CAMPCODE),'')        as  CAMPCODE ,         " _
                   & "             isnull(rtrim(MB6.USERID),'')          as  USERID ,           " _
                   & "             isnull(rtrim(MB6.SCHYMD),'')          as  SCHYMD ,           " _
                   & "             isnull(CAST(MB6.UPDTIMSTP as bigint),'0') as  TIMSTP         " _
                   & "  FROM       MB006_SCHEDULE                        MB6                    " _
                   & "  Where      MB6.CAMPCODE                           =  @P01               " _
                   & "        and  MB6.USERID                             =  @P02               " _
                   & "        and  MB6.SCHYMD                             =  @P03               " _
                   & "        and  MB6.DELFLG                            <>  '1'                "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    PARA1.Value = WF_TERMCAMP.Text
                    PARA2.Value = WF_HEADuser.Items(wColCnt - 1).Value
                    PARA3.Value = WF_HEADdate_YMD.Items(wLineCnt).Value
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        '■スケジュール内容をテーブル退避
                        While SQLdr.Read
                            wTIMSTP = SQLdr("TIMSTP")
                        End While
                    End Using
                End Using


            Catch ex As Exception
                'メッセージセット
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB006_SCHEDULE")

                Exit Sub
            End Try

            '■ 更新処理
            Try

                '更新SQL文
                Dim SQLStr As String =
                     " DECLARE @hensuu as bigint ; " _
                   & " set @hensuu = 0 ; " _
                   & " DECLARE hensuu CURSOR FOR " _
                   & " SELECT   CAST(UPDTIMSTP as bigint)          as hensuu             " _
                   & "  FROM    MB006_SCHEDULE                                           " _
                   & "  Where   CAMPCODE                           =  @P01               " _
                   & "    and   USERID                             =  @P02               " _
                   & "    and   SCHYMD                             =  @P03               " _
                   & "                                                                   " _
                   & " OPEN hensuu ;                                                     " _
                   & " FETCH NEXT FROM hensuu INTO @hensuu ;                             " _
                   & "                                                                   " _
                   & " IF ( @@FETCH_STATUS = 0 )                                         " _
                   & "   UPDATE MB006_SCHEDULE                                           " _
                   & "      SET SCHTEXT                            =  @P04 ,             " _
                   & "          DELFLG                             =  @P05 ,             " _
                   & "          UPDYMD                             =  @P07 ,             " _
                   & "          UPDUSER                            =  @P08 ,             " _
                   & "          UPDTERMID                          =  @P09 ,             " _
                   & "          RECEIVEYMD                         =  @P10               " _
                   & "   WHERE  CAMPCODE                           =  @P01               " _
                   & "      and USERID                             =  @P02               " _
                   & "      and SCHYMD                             =  @P03               " _
                   & "                                                                   " _
                   & " IF ( @@FETCH_STATUS <> 0 )                                        " _
                   & "   INSERT INTO MB006_SCHEDULE                                      " _
                   & "         (CAMPCODE ,                                               " _
                   & "          USERID ,                                                 " _
                   & "          SCHYMD ,                                                 " _
                   & "          SCHTEXT ,                                                " _
                   & "          DELFLG ,                                                 " _
                   & "          INITYMD ,                                                " _
                   & "          UPDYMD ,                                                 " _
                   & "          UPDUSER ,                                                " _
                   & "          UPDTERMID ,                                              " _
                   & "          RECEIVEYMD )                                             " _
                   & "   VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10) ;    " _
                   & " CLOSE hensuu ;                                                    " _
                   & " DEALLOCATE hensuu ;                                               "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.DateTime)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 500)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.SmallDateTime)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.DateTime)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                    Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)

                    Dim WW_DATENOW As DateTime = Date.Now
                    PARA01.Value = WF_TERMCAMP.Text                             'CAMPCODE
                    PARA02.Value = WF_HEADuser.Items(wColCnt - 1).Value         'USERID
                    PARA03.Value = WF_HEADdate_YMD.Items(wLineCnt).Value        'SCHYMD
                    PARA04.Value = CType(WF_Repeater.Items(wLineCnt).FindControl("WF_Rep_CHEDULE_" & wColCnt.ToString("00")), System.Web.UI.WebControls.TextBox).Text
                    '                                                           'SCHTEXT
                    PARA05.Value = C_DELETE_FLG.ALIVE                           'DELFLG
                    PARA06.Value = WW_DATENOW                                   'INITYMD
                    PARA07.Value = WW_DATENOW                                   'UPDYMD
                    PARA08.Value = WF_HEADuser.Items(wColCnt - 1).Value         'UPDUSER
                    PARA09.Value = CS0050Session.APSV_ID                        'UPDTERMID
                    PARA10.Value = C_DEFAULT_YMD                                'RECEIVEYMD
                    Dim cnt As Integer = 0
                    cnt = SQLcmd.ExecuteNonQuery()

                    'CLOSE
                End Using

            Catch ex As Exception
                'メッセージセット
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB006_SCHEDULE")

                Exit Sub
            End Try
        End Using
        'メッセージ表示
        '〇メッセージセット
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)


    End Sub

    ''' <summary>
    ''' 地域セレクター取得
    ''' </summary>
    Private Sub INIT_SETECTOR_AREAget()

        '■ 地域セレクタ取得　＆　列タイトル(ユーザ)用LIST作成
        '○初期クリア
        '地域ワークDB項目作成
        Dim M0006tbl As New DataTable
        Dim M0006row As DataRow
        M0006tbl.Clear()
        M0006tbl.Columns.Add("AREANAME", GetType(String))
        M0006tbl.Columns.Add("SEQ", GetType(Integer))

        WF_HEADuser.Items.Clear()

        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '構造検索SQL文
                Dim SQLStr As String =
                      "  SELECT     isnull(rtrim(M06.STRUCT),'')          as  AREANAME ,         " _
                    & "             isnull(rtrim(M06.CODE),'0')           as  USERID ,           " _
                    & "             isnull(rtrim(M06.CODENAMES),'0')      as  USERNAME ,         " _
                    & "             isnull(rtrim(M06.SEQ),'0')            as  SEQ                " _
                    & "  FROM       M0006_STRUCT                          M06                    " _
                    & "  Where      M06.CAMPCODE                           =  @P01               " _
                    & "        and  M06.OBJECT                             =  'SCHEDULE'         " _
                    & "        and  M06.ENDYMD                            >=  @P02               " _
                    & "        and  M06.STYMD                             <=  @P03               " _
                    & "        and  M06.DELFLG                            <>  '1'                " _
                    & "  ORDER BY M06.STRUCT , M06.SEQ                                           "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    PARA1.Value = WF_TERMCAMP.Text
                    PARA2.Value = Selected_ENDYYMMDD.Value
                    PARA3.Value = Selected_ENDYYMMDD.Value
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    'M0006tbl値設定
                    Dim wKEY As String = ""

                    While SQLdr.Read

                        '地域セレクタTBL作成
                        If SQLdr("AREANAME") <> wKEY Then
                            M0006row = M0006tbl.NewRow
                            M0006row("AREANAME") = SQLdr("AREANAME")
                            M0006row("SEQ") = SQLdr("SEQ")
                            M0006tbl.Rows.Add(M0006row)

                            wKEY = M0006row("AREANAME")

                        End If

                        '列タイトル(ユーザ)用LIST作成
                        If SQLdr("AREANAME") = WF_SELECTAREA.Value Then
                            WF_HEADuser.Items.Add(New ListItem(SQLdr("USERNAME"), SQLdr("USERID")))
                        End If

                    End While

                    '○地域セレクタ設定
                    'セレクタへ空行追加
                    Using wView As New DataView(M0006tbl) With {.Sort = "SEQ"}
                        WF_AREAselector.DataSource = wView
                        WF_AREAselector.DataBind()

                        '値設定
                        For i As Integer = 0 To WF_AREAselector.Items.Count - 1
                            CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_VALUE"), System.Web.UI.WebControls.Label).Text = wView.Item(i)("AREANAME")
                            CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_TEXT"), System.Web.UI.WebControls.Label).Text = "　" & wView.Item(i)("AREANAME")

                            'イベント追加
                            CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_TEXT"), System.Web.UI.WebControls.Label).Attributes.Add("onclick", "SELarea_Change('" & wView.Item(i)("AREANAME") & "');")
                        Next

                        'Close
                    End Using

                    M0006tbl.Dispose()
                    M0006tbl = Nothing

                    SQLdr.Dispose() 'Reader(Close)
                    SQLdr = Nothing
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MB006_SCHEDULE SELECT")

            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0006_STRUCT Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 日付セレクター取得
    ''' </summary>
    Private Sub INIT_SETECTOR_DATEget()

        '■ 日付セレクタ取得
        '○日付ワークDB項目準備
        Using MB005tbl As New DataTable
            MB005tbl.Clear()
            MB005tbl.Columns.Add("DATENAME", GetType(String))
            MB005tbl.Columns.Add("YYMMDD", GetType(String))

            '○日付ワークDB作成
            '条件指定(年月)編集
            Dim WW_DATE As Date = Selected_STYYMMDD.Value
            Dim WW_ENDDATE As Date = Selected_ENDYYMMDD.Value
            Do
                Dim DATErow As DataRow = MB005tbl.NewRow
                DATErow("DATENAME") = WW_DATE.ToString("yyyy年MM月")
                DATErow("YYMMDD") = WW_DATE.ToString("yyyy/MM/dd")
                MB005tbl.Rows.Add(DATErow)

                WW_DATE = WW_DATE.AddMonths(1)

            Loop Until WW_ENDDATE < WW_DATE

            '■ 日付セレクタ設定
            '○空明細追加
            WF_DATEselector.DataSource = MB005tbl
            WF_DATEselector.DataBind()

            '値設定
            For i As Integer = 0 To WF_DATEselector.Items.Count - 1
                CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_VALUE"), System.Web.UI.WebControls.Label).Text = MB005tbl.Rows(i)("YYMMDD")
                CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_TEXT"), System.Web.UI.WebControls.Label).Text = "　" & MB005tbl.Rows(i)("DATENAME")

                'イベント追加
                CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_TEXT"), System.Web.UI.WebControls.Label).Attributes.Add("onclick", "SELdate_Change('" & MB005tbl.Rows(i)("YYMMDD") & "');")
            Next

        End Using

    End Sub

    ''' <summary>
    ''' 画面表示処理
    ''' </summary>
    Protected Sub FORMdisplay()

        Dim RemoteIp As String = Request.UserHostAddress

        Dim RemoteIp3 As String = ""
        Dim ClientIP As String = ""
        Try

            RemoteIp = Request.UserHostAddress
            Dim ClientIphEntry As IPHostEntry = Dns.GetHostEntry(RemoteIp)
            For Each ipAddr As IPAddress In ClientIphEntry.AddressList
                'IPv4にする
                If ipAddr.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                    RemoteIp = ipAddr.ToString
                End If
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "クライアントIP取得失敗")
            Exit Sub
        End Try

        If RemoteIp.LastIndexOf(".") < 0 Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "クライアントIP取得失敗")
            Exit Sub
        Else
            RemoteIp3 = Mid(RemoteIp, 1, RemoteIp.LastIndexOf("."))
        End If


        Select Case RemoteIp3
            Case "172.16.101"  '本社
                WF_SELECTAREA.Value = "本社"
            Case "172.16.222"  '九州
                WF_SELECTAREA.Value = "本社"
            Case "172.16.134"  'ＤＯＣ
                WF_SELECTAREA.Value = "本社"
            Case "172.16.210"  'ニチユ
                WF_SELECTAREA.Value = "本社"

            Case "172.16.152"  '北海道支店
                WF_SELECTAREA.Value = "北海道"
            Case "172.16.221"  '苫小牧
                WF_SELECTAREA.Value = "北海道"
            Case "172.16.147"  '石狩
                WF_SELECTAREA.Value = "北海道"

            Case "172.16.166"  '東北支店
                WF_SELECTAREA.Value = "東北"
            Case "172.16.163"  '秋田
                WF_SELECTAREA.Value = "東北"
            Case "172.16.229"  '青森
                WF_SELECTAREA.Value = "東北"
            Case "172.16.140"  '八戸
                WF_SELECTAREA.Value = "東北"

            Case "172.16.215"  '関東支店
                WF_SELECTAREA.Value = "関東"
            Case "172.16.233"  '川崎
                WF_SELECTAREA.Value = "関東"
            Case "172.16.224"  '鹿島
                WF_SELECTAREA.Value = "関東"
            Case "172.16.131"  '八王子
                WF_SELECTAREA.Value = "関東"

            Case "172.16.160"  '根岸
                WF_SELECTAREA.Value = "関東LNG"
            Case "172.16.234"  '袖ケ浦1
                WF_SELECTAREA.Value = "関東LNG"
            Case "172.16.228"  '袖ケ浦2
                WF_SELECTAREA.Value = "関東LNG"
            Case "172.16.226"  '茨城
                WF_SELECTAREA.Value = "関東LNG"

            Case "172.16.232"  '新潟支店
                WF_SELECTAREA.Value = "新潟"
            Case "172.16.137"  '庄内
                WF_SELECTAREA.Value = "新潟"
            Case "172.16.146"  '上越
                WF_SELECTAREA.Value = "新潟"

            Case "172.16.164"  '中部支店
                WF_SELECTAREA.Value = "中部"
            Case "172.16.143"  '四日市
                WF_SELECTAREA.Value = "中部"
            Case "172.16.133"  '大井川
                WF_SELECTAREA.Value = "中部"

            Case "172.16.167"  '関西
                WF_SELECTAREA.Value = "関西"
            Case "172.16.227"  '姫路
                WF_SELECTAREA.Value = "関西"
            Case "172.16.219"  '水島
                WF_SELECTAREA.Value = "関西"
            Case Else
                'WF_SELECTAREA.Value = "北海道"
                WF_SELECTAREA.Value = "本社"

        End Select

        '強制置換
        '鹿島(植田PC)
        If RemoteIp = "172.16.224.102" Then
            WF_SELECTAREA.Value = "本社"
        End If


        '■ セレクター選択表示        ★★★共通
        '○地域セレクター
        For i As Integer = 0 To WF_AREAselector.Items.Count - 1
            '背景色
            If CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_VALUE"), System.Web.UI.WebControls.Label).Text = WF_SELECTAREA.Value Then
                CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_AREAselector.Items(i).FindControl("WF_SELarea_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If
        Next

        '○日付セレクター
        For i As Integer = 0 To WF_DATEselector.Items.Count - 1
            '背景色
            Dim wDATE1 As Date = CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_VALUE"), System.Web.UI.WebControls.Label).Text
            Dim wDATE2 As Date = WF_SELECTYYMMDD.Value

            If wDATE1.ToString("yyyyMM") = wDATE2.ToString("yyyyMM") Then
                CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;background-color:darksalmon;border: solid 1.0px black;"
            Else
                CType(WF_DATEselector.Items(i).FindControl("WF_SELdate_TEXT"), System.Web.UI.WebControls.Label).Style.Value = "height:1.5em;background-color:rgb(220,230,240);border: solid 1.0px black;"
            End If
        Next

        '■ 行タイトル・列タイトル取得
        '○リピータ行タイトル(日付)取得(MAP内LISTへ退避)
        FORM_DATEget()

        '○リピータ列タイトル(ユーザ)取得(MAP内LISTへ退避)
        FORM_USERget()

        '■ スケジュール情報取得(MB0006tbl設定)        ★★★共通
        FORM_DataGet(WF_SELECTYYMMDD.Value)

        '■ 画面表示設定        ★★★共通
        '画面スケジュールセット
        FORM_SCHEDULEset()

        '■ Close
        If Not IsNothing(MB0006tbl) Then
            MB0006tbl.Dispose()
            MB0006tbl = Nothing
        End If
    End Sub

    ''' <summary>
    ''' リピータ行タイトル(日付)取得
    ''' </summary>
    Private Sub FORM_DATEget()

        '★　リピータ情報は、要求都度、置き換えられる。(Hidden項目に情報を残す)

        '■ 日付セレクタ取得　＆　行タイトル(日付)用LIST作成

        '○日付ワークDB作成
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection

                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "  SELECT     isnull(rtrim(MB5.WORKINGYMD),'')      as  YMD ,              " _
                   & "             isnull(rtrim(MB5.WORKINGWEEK),'')     as  WEEKKBN ,          " _
                   & "             isnull(rtrim(MB5.WORKINGTEXT),'')     as  TEXT               " _
                   & "  FROM       MB005_CALENDAR                        MB5                    " _
                   & "  Where      MB5.CAMPCODE                           =  @P01               " _
                   & "        and  MB5.WORKINGYMD                        >=  @P02               " _
                   & "        and  MB5.WORKINGYMD                        <=  @P03               " _
                   & "        and  MB5.DELFLG                            <>  '1'                " _
                   & "  ORDER BY MB5.WORKINGYMD                                                 "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    PARA1.Value = WF_TERMCAMP.Text
                    PARA2.Value = New DateTime(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month, 1).ToString("yyyy/MM/dd")
                    PARA3.Value = New DateTime(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month, DateTime.DaysInMonth(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month)).ToString("yyyy/MM/dd")
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    '日付ワークTBL作成
                    WF_HEADdate_YMD.Items.Clear()
                    WF_HEADdate_WEEKKBN.Items.Clear()
                    WF_HEADdate_TEXT.Items.Clear()

                    While SQLdr.Read
                        Dim wDATE As Date = SQLdr("YMD")
                        WF_HEADdate_YMD.Items.Add(wDATE.ToString("yyyy/MM/dd"))
                        WF_HEADdate_WEEKKBN.Items.Add(SQLdr("WEEKKBN"))
                        WF_HEADdate_TEXT.Items.Add(SQLdr("TEXT"))
                    End While

                    SQLdr.Dispose() 'Reader(Close)
                    SQLdr = Nothing
                End Using
            End Using

        Catch ex As Exception
            'メッセージセット
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)

            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' リピータ列タイトル(ユーザ)取得
    ''' </summary>
    Private Sub FORM_USERget()

        '■ 地域セレクタ取得　＆　列タイトル(ユーザ)用LIST作成
        '○初期クリア
        '地域ワークDB項目作成
        WF_HEADuser.Items.Clear()

        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '構造検索SQL文
                Dim SQLStr As String =
                     "  SELECT     isnull(rtrim(M06.STRUCT),'')          as  AREANAME ,         " _
                   & "             isnull(rtrim(M06.CODE),'0')           as  USERID ,           " _
                   & "             isnull(rtrim(M06.CODENAMES),'0')      as  USERNAME ,         " _
                   & "             isnull(rtrim(M06.SEQ),'0')            as  SEQ                " _
                   & "  FROM       M0006_STRUCT                          M06                    " _
                   & "  Where      M06.CAMPCODE                           =  @P01               " _
                   & "        and  M06.OBJECT                             =  'SCHEDULE'         " _
                   & "        and  M06.ENDYMD                            >=  @P02               " _
                   & "        and  M06.STYMD                             <=  @P03               " _
                   & "        and  M06.DELFLG                            <>  '1'                " _
                   & "  ORDER BY M06.STRUCT , M06.SEQ                                           "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    PARA1.Value = WF_TERMCAMP.Text
                    PARA2.Value = New DateTime(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month, 1).ToString("yyyy/MM/dd")
                    PARA3.Value = New DateTime(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month, DateTime.DaysInMonth(CDate(WF_SELECTYYMMDD.Value).Year, CDate(WF_SELECTYYMMDD.Value).Month)).ToString("yyyy/MM/dd")
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    'M0006tbl値設定
                    While SQLdr.Read

                        '列タイトル(ユーザ)用LIST作成
                        If SQLdr("AREANAME") = WF_SELECTAREA.Value Then
                            WF_HEADuser.Items.Add(New ListItem(SQLdr("USERNAME"), SQLdr("USERID")))
                        End If

                    End While

                    'Close
                    SQLdr.Dispose() 'Reader(Close)
                    SQLdr = Nothing
                End Using
            End Using

        Catch ex As Exception
            'メッセージセット
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)

            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' スケジュールデータ取得処理
    ''' </summary>
    ''' <param name="I_YMD"></param>
    Private Sub FORM_DataGet(ByVal I_YMD As String)

        Dim wDate As Date

        '■ スケジュールデータ取得
        'DataBase接続文字
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "  SELECT     isnull(rtrim(MB5.CAMPCODE),'')        as  CAMPCODE ,         " _
                   & "             isnull(rtrim(MB5.WORKINGYMD),'')      as  YMD ,              " _
                   & "             isnull(rtrim(MB5.WORKINGWEEK),'')     as  WEEKKBN ,          " _
                   & "             isnull(rtrim(MB5.WORKINGYMD),'')      as  TEXT ,             " _
                   & "             isnull(rtrim(M06.CODE),'')            as  USERID ,           " _
                   & "             isnull(rtrim(M06.CODENAMES),'')       as  USERNAME ,         " _
                   & "             isnull(rtrim(M06.STRUCT),'')          as  AREANAME ,         " _
                   & "             isnull(rtrim(M06.GRCODE01),'')        as  ORGNAME ,          " _
                   & "             isnull(rtrim(M06.SEQ),'0')            as  SEQ ,              " _
                   & "             isnull(rtrim(MB6.SCHTEXT),'')         as  SCHTEXT ,          " _
                   & "             isnull(CAST(MB6.UPDTIMSTP as bigint),'0') as  TIMSTP         " _
                   & "  FROM       MB005_CALENDAR                        MB5                    " _
                   & "  INNER JOIN M0006_STRUCT                          M06                    " _
                   & "         ON  M06.CAMPCODE                           =  MB5.CAMPCODE       " _
                   & "        and  M06.OBJECT                             =  'SCHEDULE'         " _
                   & "        and  M06.STRUCT                             =  @P04               " _
                   & "        and  M06.ENDYMD                            >=  MB5.WORKINGYMD     " _
                   & "        and  M06.STYMD                             <=  MB5.WORKINGYMD     " _
                   & "        and  M06.DELFLG                            <>  '1'                " _
                   & "  LEFT JOIN  MB006_SCHEDULE                        MB6                    " _
                   & "         ON  MB6.CAMPCODE                           =  MB5.CAMPCODE       " _
                   & "        and  MB6.USERID                             =  M06.CODE           " _
                   & "        and  MB6.SCHYMD                             =  MB5.WORKINGYMD     " _
                   & "        and  MB6.DELFLG                            <>  '1'                " _
                   & "  Where      MB5.CAMPCODE                           =  @P01               " _
                   & "        and  MB5.WORKINGYMD                        >=  @P02               " _
                   & "        and  MB5.WORKINGYMD                        <=  @P03               " _
                   & "        and  MB5.DELFLG                            <>  '1'                " _
                   & "  ORDER BY M06.SEQ , MB5.WORKINGYMD                                       "

                Using SQLcmd = New SqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 50)
                    PARA1.Value = WF_TERMCAMP.Text
                    PARA2.Value = New DateTime(CDate(I_YMD).Year, CDate(I_YMD).Month, 1).ToString("yyyy/MM/dd")
                    PARA3.Value = New DateTime(CDate(I_YMD).Year, CDate(I_YMD).Month, DateTime.DaysInMonth(CDate(I_YMD).Year, CDate(I_YMD).Month)).ToString("yyyy/MM/dd")
                    PARA4.Value = WF_SELECTAREA.Value
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    '■スケジュール内容をテーブル退避
                    'MB0006テンポラリDB項目作成
                    MB0006tbl_ColumnsAdd()

                    'MB0006tbl値設定
                    While SQLdr.Read

                        '■抽出条件対象か判断
                        '選択画面情報によりデータ抽出
                        MB0006row = MB0006tbl.NewRow()

                        '○共通項目
                        MB0006row("LINECNT") = 0
                        MB0006row("OPERATION") = ""
                        MB0006row("TIMSTP") = SQLdr("TIMSTP")

                        '○画面固有項目
                        MB0006row("CAMPCODE") = SQLdr("CAMPCODE")
                        wDate = SQLdr("YMD")
                        MB0006row("YMD") = wDate.ToString("yyyy/MM/dd")

                        MB0006row("WEEKKBN") = SQLdr("WEEKKBN")
                        MB0006row("TEXT") = SQLdr("TEXT")
                        MB0006row("SEQ") = SQLdr("SEQ")
                        MB0006row("USERID") = SQLdr("USERID")
                        MB0006row("USERNAME") = SQLdr("USERNAME")
                        MB0006row("AREANAME") = SQLdr("AREANAME")
                        MB0006row("ORGNAME") = SQLdr("ORGNAME")
                        MB0006row("SCHTEXT") = SQLdr("SCHTEXT")

                        ' 取得結果を登録する
                        MB0006tbl.Rows.Add(MB0006row)
                    End While

                    SQLdr.Dispose() 'Reader(Close)
                    SQLdr = Nothing
                End Using

            End Using

        Catch ex As Exception
            'メッセージセット
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' スケジュールセット
    ''' </summary>
    Protected Sub FORM_SCHEDULEset()

        '■ タイトル・明細設定の準備
        '○行タイトル・列タイトル・明細(リピータ)へ空行追加　
        Dim WORKtbl As New DataTable
        Dim WORKrow As DataRow

        '列タイトル(1行)
        WORKtbl = New DataTable
        WORKtbl.Clear()
        WORKtbl.Columns.Add("NAME", GetType(String))
        WORKrow = WORKtbl.NewRow
        WORKrow("NAME") = ""
        WORKtbl.Rows.Add(WORKrow)
        WF_RepeaterHC.DataSource = WORKtbl
        WF_RepeaterHC.DataBind()

        '行タイトル(日付分の行数)
        WORKtbl = New DataTable
        WORKtbl.Clear()
        WORKtbl.Columns.Add("GDATE", GetType(String))
        WORKtbl.Columns.Add("GDATE_TEXT", GetType(String))

        For i As Integer = 0 To WF_HEADdate_YMD.Items.Count - 1
            WORKrow = WORKtbl.NewRow
            WORKrow("GDATE") = WF_HEADdate_YMD.Items(i).Text
            WORKrow("GDATE_TEXT") = WF_HEADdate_TEXT.Items(i).Text
            WORKtbl.Rows.Add(WORKrow)
        Next

        WF_RepeaterHL.DataSource = WORKtbl
        WF_RepeaterHL.DataBind()

        '明細(日付と同じ行数)
        WORKtbl = New DataTable
        WORKtbl.Clear()
        WORKtbl.Columns.Add("GDATE", GetType(String))
        WORKtbl.Columns.Add("GDATE_TEXT", GetType(String))

        For i As Integer = 0 To WF_HEADdate_YMD.Items.Count - 1
            WORKrow = WORKtbl.NewRow
            WORKrow("GDATE") = WF_HEADdate_YMD.Items(i).Text
            WORKrow("GDATE_TEXT") = WF_HEADdate_TEXT.Items(i).Text
            WORKtbl.Rows.Add(WORKrow)
        Next

        WF_Repeater.DataSource = WORKtbl
        WF_Repeater.DataBind()

        '■ タイトル設定
        '○行タイトル・列タイトル設定
        '列タイトル(1行)
        For i As Integer = 0 To WF_RepeaterHC.Items.Count - 1
            'クリア
            For j As Integer = 1 To 50
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USER_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).Text = ""
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USER_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).ForeColor = Color.Black
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USERNM_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).Text = ""
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USERNM_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).ForeColor = Color.Black
            Next

            'データセット
            For j As Integer = 0 To WF_HEADuser.Items.Count - 1
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USER_" & (j + 1).ToString("00")), System.Web.UI.WebControls.TextBox).Text = WF_HEADuser.Items(j).Value
                CType(WF_RepeaterHC.Items(0).FindControl("WF_Rep_USERNM_" & (j + 1).ToString("00")), System.Web.UI.WebControls.TextBox).Text = WF_HEADuser.Items(j).Text
            Next
        Next

        '行タイトル
        For i As Integer = 0 To WF_RepeaterHL.Items.Count - 1
            '日付
            CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE"), System.Web.UI.WebControls.TextBox).Text = CDate(WF_HEADdate_YMD.Items(i).Text).ToString("MM月dd日")
            'TEXT
            CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE_TEXT"), System.Web.UI.WebControls.TextBox).Text = WF_HEADdate_TEXT.Items(i).Text

            '曜日色
            Select Case WF_HEADdate_WEEKKBN.Items(i).Text
                Case "0"    '日曜日
                    CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE"), System.Web.UI.WebControls.TextBox).ForeColor = Color.Red
                Case "6"    '土曜日
                    CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE"), System.Web.UI.WebControls.TextBox).ForeColor = Color.Blue
                Case Else
                    CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE"), System.Web.UI.WebControls.TextBox).ForeColor = Color.Black
            End Select
            '法定・法定外休日は赤
            If WF_HEADdate_TEXT.Items(i).Text = Nothing Then
            Else
                CType(WF_RepeaterHL.Items(i).FindControl("WF_Rep_GDATE"), System.Web.UI.WebControls.TextBox).ForeColor = Color.Red
            End If

        Next

        '■ 明細設定
        '○明細設定
        'クリア・イベント追加
        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            'クリア
            For j As Integer = 1 To WF_HEADuser.Items.Count
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_CHEDULE_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).Text = ""

                'イベント追加
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_CHEDULE_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).Attributes.Add("onfocus", "Repeater_focus(" & i.ToString & ", " & j.ToString & ");")
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_CHEDULE_" & (j).ToString("00")), System.Web.UI.WebControls.TextBox).Attributes.Add("onchange", "Repeater_Change();")
            Next
        Next

        'データ設定
        Dim WW_USERID As String = ""
        Dim WW_SEQ As Integer = 0
        For i As Integer = 0 To MB0006tbl.Rows.Count - 1
            MB0006row = MB0006tbl.Rows(i)

            If MB0006row("USERID") <> WW_USERID Then
                WW_SEQ += 1
            End If

            Dim wDAY As Integer = CDate(MB0006row("YMD")).Day
            'Dim wINT As Integer = MB0006row("SEQ")
            Dim wINT As Integer = WW_SEQ
            If MB0006row("SEQ") >= 1 And MB0006row("SEQ") <= 50 Then
                CType(WF_Repeater.Items(wDAY - 1).FindControl("WF_Rep_CHEDULE_" & wINT.ToString("00")), System.Web.UI.WebControls.TextBox).Text = MB0006row("SCHTEXT")
                CType(WF_Repeater.Items(wDAY - 1).FindControl("WF_Rep_CHEDULE_" & wINT.ToString("00") & "_STP"), System.Web.UI.WebControls.Label).Text = MB0006row("TIMSTP")
            End If

            WW_USERID = MB0006row("USERID")
        Next

        'Close
        WORKtbl.Dispose()
        WORKtbl = Nothing

    End Sub

    ''' <summary>
    ''' MB0006tbl項目設定
    ''' </summary>
    Protected Sub MB0006tbl_ColumnsAdd()

        '○DB項目クリア
        MB0006tbl = New DataTable
        MB0006tbl.Clear()

        '○共通項目
        MB0006tbl.Columns.Add("LINECNT", GetType(String))           'DBの固定フィールド
        MB0006tbl.Columns.Add("OPERATION", GetType(String))         'DBの固定フィールド
        MB0006tbl.Columns.Add("TIMSTP", GetType(String))            'DBの固定フィールド

        '○画面固有項目
        MB0006tbl.Columns.Add("CAMPCODE", GetType(String))
        MB0006tbl.Columns.Add("YMD", GetType(String))
        MB0006tbl.Columns.Add("WEEKKBN", GetType(String))
        MB0006tbl.Columns.Add("TEXT", GetType(String))
        MB0006tbl.Columns.Add("SEQ", GetType(Integer))
        MB0006tbl.Columns.Add("USERID", GetType(String))
        MB0006tbl.Columns.Add("USERNAME", GetType(String))
        MB0006tbl.Columns.Add("AREANAME", GetType(String))
        MB0006tbl.Columns.Add("ORGNAME", GetType(String))
        MB0006tbl.Columns.Add("SCHTEXT", GetType(String))

    End Sub
    ''' <summary>
    ''' 端末情報を取得する  
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks>
    ''' <para>S0001_TERMを検索　IPADDRを見る　TERMID取得</para>
    ''' <para>TERMIDを基にM00006_STRUCTを検索</para>
    ''' <para >部署を基に運用ガイダンス表示</para>
    ''' </remarks>
    Private Sub getTermData(ByRef O_RTN As String)
        Dim GS0007FIXVALUElst As New GS0007FIXVALUElst      'FIXVALUE Get
        Dim RemoteIp As String = Request.UserHostAddress

        Dim RemoteIp3 As String = ""
        Dim ClientIP As String = ""
        Try

            RemoteIp = Request.UserHostAddress
            Dim ClientIphEntry As IPHostEntry = Dns.GetHostEntry(RemoteIp)
            For Each ipAddr As IPAddress In ClientIphEntry.AddressList
                'IPv4にする
                If ipAddr.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                    RemoteIp = ipAddr.ToString
                End If
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "クライアントIP取得失敗")
            Exit Sub
        End Try

        If RemoteIp.LastIndexOf(".") < 0 Then
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "クライアントIP取得失敗")
            Exit Sub
        Else
            RemoteIp3 = Mid(RemoteIp, 1, RemoteIp.LastIndexOf("."))
        End If

        '○ ユーザ
        Try
            Dim SQLStr0 As String =
                     " SELECT                                                                                                " _
                   & "         Z.TERMID                                                                      as TERMID       " _
                   & "       , Z.TERMCAMP                                                                    as COMPCODE     " _
                   & " FROM     S0001_TERM                                  Z                                                " _
                   & " WHERE                                                                                                 " _
                   & "         Z.IPADDR            = @P01                                                                    " _
                   & "   and   Z.TERMCLASS         = @P04                                                                    " _
                   & "   and   Z.STYMD            <= @P02                                                                    " _
                   & "   and   Z.ENDYMD           >= @P02                                                                    " _
                   & "   and   Z.DELFLG           <> @P03                                                                    "


            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Using SQLcmd0 As New SqlCommand(SQLStr0, SQLcon)
                    Dim PARA1 As SqlParameter = SQLcmd0.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd0.Parameters.Add("@P02", System.Data.SqlDbType.Date)
                    Dim PARA3 As SqlParameter = SQLcmd0.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 1)
                    Dim PARA4 As SqlParameter = SQLcmd0.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 1)

                    PARA1.Value = RemoteIp3
                    PARA2.Value = Date.Now
                    PARA3.Value = C_DELETE_FLG.DELETE
                    PARA4.Value = C_TERMCLASS.CLIENT

                    Dim SQLdr As SqlDataReader = SQLcmd0.ExecuteReader()
                    If SQLdr.Read Then
                        WF_TERMID.Text = SQLdr("TERMID")
                        WF_TERMCAMP.Text = SQLdr("COMPCODE")
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0001_TERM SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "getTermData"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0001_TERM SELECT"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
        End Try

    End Sub
End Class







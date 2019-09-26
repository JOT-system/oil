Imports System.IO
Imports System.Data.SqlClient
Imports System.Net
Imports System.ServiceProcess
Imports BASEDLL

Public Class GRCO0106LIBSEND
    Inherits Page
    '共通宣言
    '*共通関数宣言(BASEDLL)
    Private CS0050Session As New CS0050SESSION                      'セッション管理
    Private CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
    '検索結果格納ds
    Private CO0106tbl As DataTable                                  'データ格納用テーブル
    Private S0021_LIBSENDSTATtbl As DataTable                       'ジャーナル用テーブル

    '共通処理結果
    Private WW_RTN As String                                        'サブ用リターンコード
    Private WW_DUMMY As String

    Private WW_RTN_Detail As String                                 'サブ用リターンコード(項目名)
    Private WW_RTN_Action As String                                 'サブ用リターンコード(重複:Dub , 新規:Insert , 更新:Update)

    Dim WW_ERRLIST_ALL As List(Of String)                           'インポート全体のエラー
    Dim WW_ERRLIST As List(Of String)                               'インポート中の１セット分のエラー

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        '■■■ 画面モード（更新・参照）設定  ■■■
        ButtonCNTL()
        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                '〇初期処理
                Master.RecoverTable(CO0106tbl)
                Select Case WF_ButtonClick.Value
                    Case "SSTAT"                    '■■■ サービス確認ボタン処理 ■■■
                        WF_ButtonSSTAT_Click()
                    Case "PSTAT"                    '■■■ ping確認ボタン処理 ■■■
                        WF_ButtonPSTAT_Click()
                    Case "OSTAT"                    '■■■ オンライン確認ボタン処理 ■■■
                        WF_ButtonOSTAT_Click()
                    Case "OSTART"                  '■■■ オンライン開始ボタン処理 ■■■
                        WF_ButtonOSTART_Click()
                    Case "OSTOP"                    '■■■ オンライン停止ボタン処理 ■■■
                        WF_ButtonOSTOP_Click()
                    Case "SSTART"                  '■■■ サービス開始ボタン処理 ■■■
                        WF_ButtonSSTART_Click()
                    Case "SSTOP"                    '■■■ サービス停止ボタン処理 ■■■
                        WF_ButtonSSTOP_Click()
                    Case "SEND"                     '■■■ 配信ボタン処理 ■■■
                        WF_ButtonSEND_Click()
                    Case "WF_ALLSELECT"
                        WF_ButtonALLSELECT_Click()
                    Case "WF_ALLCANCEL"
                        WF_ButtonALLCANCEL_Click()
                    Case "WF_ButtonEND"
                        WF_ButtonEND_Click()
                    Case "WF_RadioButonClick"
                        WF_RadioButon_Click()
                    Case "WF_MEMOChange"
                        WF_MEMO_Change()
                    Case "WF_INITIALIZE"            '■■■ 初回状況取得処理 ■■■
                        WF_ButtonSTAT_Click()
                    Case Else

                End Select

            End If

        Else
            '〇初期化処理
            Initialize()
        End If
        '○Close
        If Not IsNothing(CO0106tbl) Then
            CO0106tbl.Dispose()
            CO0106tbl = Nothing
        End If
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○初期値設定
        rightview.resetindex()
        leftview.activeListBox()
        '〇 条件抽出画面情報退避
        MAPrefelence()
        '〇ヘルプ無
        Master.dispHelp = False
        '〇ドラックアンドドロップON
        Master.eventDrop = False

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○画面表示データ取得
        MAPDATAget()
        '〇バージョンの初期情報設定
        WF_VER.Text = Date.Now.ToString("yyyy/MM/dd HH:mm")
        '〇データの保存
        Master.SaveTable(CO0106tbl)
        '■■■ Detail初期設定 ■■■
        Repeater_init()

        WF_ButtonALLCANCEL.Enabled = False
        WF_ButtonALLSELECT.Enabled = False
        WF_ButtonEND.Enabled = False
        WF_ButtonSEND.Enabled = False
        WF_ButtonPSTAT.Enabled = False
        WF_ButtonSSTAT.Enabled = False
        WF_ButtonOSTAT.Enabled = False
        WF_ButtonOSTART.Enabled = False
        WF_ButtonOSTOP.Enabled = False
        WF_ButtonSSTART.Enabled = False
        WF_ButtonSSTOP.Enabled = False

        WF_ButtonClick.Value = "WF_INITIALIZE"
    End Sub
    ''' <summary>
    ''' 全選択ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLSELECT_Click()
        '全チェックボックスON
        For Each item As RepeaterItem In WF_Repeater.Items
            CType(item.FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = True
        Next

        WF_Repeater.Visible = True

    End Sub

    ''' <summary>
    ''' 全解除ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonALLCANCEL_Click()
        '全チェックボックスOFF
        For Each item As RepeaterItem In WF_Repeater.Items
            CType(item.FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False
        Next

    End Sub

    ''' <summary>
    ''' 状態確認ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSTAT_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_CHECKED_HOST = New ListBox

        For i As Integer = 0 To CO0106tbl.Rows.Count - 1
            Dim WW_RTN As String = ""
            Dim WW_ERR As String = C_MESSAGE_NO.NORMAL
            Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")

            'チェック済は取得済を反映
            If Not IsNothing(WW_CHECKED_HOST.Items.FindByValue(CO0106tbl.Rows(i)("TERMID"))) Then
                WW_RTN = WW_CHECKED_HOST.Items.FindByValue(CO0106tbl.Rows(i)("TERMID")).Text
            Else
                '対象チェック以外は処理しない
                If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

                WW_CheckBox = True

                S0021_SELECT(CO0106tbl.Rows(i))

                CO0106tbl.Rows(i)("NETSTAT") = ""

                'pingがOKの場合のみサービス稼働をチェックする
                RemoteSrvCheck(WW_ServerName, RemoteServiceCmd.C_GET_SERVICE_STATUS, WW_RTN)
                WW_CHECKED_HOST.Items.Add(New ListItem(WW_RTN, CO0106tbl.Rows(i)("TERMID")))
            End If

            Select Case WW_RTN
                Case "Running"
                    CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                    CO0106tbl.Rows(i)("SNOTES") = ""
                Case "Stopped"
                    CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                    CO0106tbl.Rows(i)("SNOTES") = ""
                Case "RunningJOB"
                    CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SNOTES") = "集配信実行中のためスキップ"
                Case Else
                    CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SNOTES") = "サービス異常か、接続できない"
                    WW_ERR = C_MESSAGE_NO.SELECT_DETAIL_ERROR
            End Select


            'If isNormal(WW_ERR) Then
            '対象チェック以外は処理しない
            If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

            'pingがOKの場合のみオンライン稼働をチェックする
            'RemoteSrvCheck(WW_ServerName, RemoteServiceCmd.C_GET_ONLINE_STATUS, WW_RTN)
            Dim WW_ServiceParam As String = RemoteServiceCmd.C_GET_ONLINE_STATUS & "," & CO0106tbl.Rows(i)("TERMID") & "," & CO0106tbl.Rows(i)("CAMPCODE") & "," & Master.USERID
            RemoteSrvCheck(WW_ServerName, WW_ServiceParam, WW_RTN)

            Select Case WW_RTN
                    Case "1"
                        CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "0"
                        CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case Else
                        CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "オンライン異常か、接続できない"
                End Select
            'End If
        Next

        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' ping確認ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPSTAT_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As Boolean = False
        Using WW_CHECKED = New ListBox

            For i As Integer = 0 To CO0106tbl.Rows.Count - 1
                'チェック済は取得済を反映
                If Not IsNothing(WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID"))) Then
                    WW_RTN = WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID")).Text
                Else
                    '対象チェック以外は処理しない
                    If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For
                    WW_CheckBox = True
                    'S0021検索
                    S0021_SELECT(CO0106tbl.Rows(i))
                    WW_RTN = CheckforPing(CO0106tbl.Rows(i)("IPADDR"))
                    WW_CHECKED.Items.Add(New ListItem(WW_RTN, CO0106tbl.Rows(i)("TERMID")))
                End If
                If WW_RTN Then
                    CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.OK
                Else
                    CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SERVICESTAT") = "　"
                End If

            Next
        End Using

        '〇表示再生成
        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' サービス確認ボタン処理   
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSSTAT_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As String = ""
        Using WW_CHECKED = New ListBox

            For i As Integer = 0 To CO0106tbl.Rows.Count - 1
                'チェック済は取得済を反映
                If Not IsNothing(WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID"))) Then
                    WW_RTN = WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID")).Text
                Else

                    '対象チェック以外は処理しない
                    If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

                    WW_CheckBox = True

                    S0021_SELECT(CO0106tbl.Rows(i))


                    'pingがOKの場合のみサービス稼働をチェックする
                    Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")
                    RemoteSrvCheck(WW_ServerName, RemoteServiceCmd.C_GET_SERVICE_STATUS, WW_RTN)
                    WW_CHECKED.Items.Add(New ListItem(WW_RTN, CO0106tbl.Rows(i)("TERMID")))
                End If

                Select Case WW_RTN
                    Case "Running"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "Stopped"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "RunningJOB"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "集配信実行中のためスキップ"
                    Case Else
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "サービス異常か、接続できない"
                End Select
            Next
        End Using


        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If
    End Sub

    ' ******************************************************************************
    ' ***  オンライン確認ボタン処理                                              ***　★
    ' ******************************************************************************
    Protected Sub WF_ButtonOSTAT_Click()

        Dim WW_CheckBox As Boolean = False

        For i As Integer = 0 To CO0106tbl.Rows.Count - 1

            '対象チェック以外は処理しない
            If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

            WW_CheckBox = True

            S0021_SELECT(CO0106tbl.Rows(i))

            Dim WW_RTN As String = ""
            'pingがOKの場合のみオンライン稼働をチェックする
            Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")
            Dim WW_ServiceParam As String = RemoteServiceCmd.C_GET_ONLINE_STATUS & "," & CO0106tbl.Rows(i)("TERMID") & "," & CO0106tbl.Rows(i)("CAMPCODE") & "," & Master.USERID
            RemoteSrvCheck(WW_ServerName, WW_ServiceParam, WW_RTN)

            Select Case WW_RTN
                Case "1"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                    CO0106tbl.Rows(i)("SNOTES") = ""
                Case "0"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                    CO0106tbl.Rows(i)("SNOTES") = ""
                Case Else
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SNOTES") = "オンライン異常か、接続できない"
            End Select

        Next

        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' オンライン開始ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOSTART_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As String = ""

        For i As Integer = 0 To CO0106tbl.Rows.Count - 1

            '対象チェック以外は処理しない
            If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

            WW_CheckBox = True

            '操作端末（配信元）の場合は、当然停止しない
            If CO0106tbl.Rows(i)("TERMID") = CS0050Session.APSV_ID Then Continue For

            S0021_SELECT(CO0106tbl.Rows(i))

            Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")
            Dim WW_ServiceParam As String = RemoteServiceCmd.C_START_ONLINE_STATUS & "," & CO0106tbl.Rows(i)("TERMID") & "," & CO0106tbl.Rows(i)("CAMPCODE") & "," & Master.USERID

            RemoteSrvCheck(WW_ServerName, WW_ServiceParam, WW_RTN)
            Select Case WW_RTN
                Case "1"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                    CO0106tbl.Rows(i)("ONOTES") = ""
                Case "0"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                    CO0106tbl.Rows(i)("ONOTES") = ""
                Case Else
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SNOTES") = "オンライン異常か、接続できない"
            End Select
            'End If

        Next

        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' オンライン停止ボタン処理 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOSTOP_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As String = ""

        For i As Integer = 0 To CO0106tbl.Rows.Count - 1

            '対象チェック以外は処理しない
            If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

            WW_CheckBox = True

            '操作端末（配信元）の場合は、当然オンライン停止しない
            If CO0106tbl.Rows(i)("TERMID") = CS0050Session.APSV_ID Then
                CO0106tbl.Rows(i)("SNOTES") = "当画面が起動しなくなるためオンライン停止できません"
                Continue For
            End If

            S0021_SELECT(CO0106tbl.Rows(i))

            Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")
            Dim WW_ServiceParam As String = RemoteServiceCmd.C_STOP_ONLINE_STATUS & "," & CO0106tbl.Rows(i)("TERMID") & "," & CO0106tbl.Rows(i)("CAMPCODE") & "," & Master.USERID

            RemoteSrvCheck(WW_ServerName, WW_ServiceParam, WW_RTN)
            Select Case WW_RTN
                Case "1"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                    CO0106tbl.Rows(i)("ONOTES") = ""
                Case "0"
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                    CO0106tbl.Rows(i)("ONOTES") = ""
                Case Else
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG
                    CO0106tbl.Rows(i)("SNOTES") = "オンライン異常か、接続できない"
            End Select
            'End If

        Next

        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    '''  サービス開始ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSSTART_Click()

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As String = ""
        Using WW_CHECKED = New ListBox
            For i As Integer = 0 To CO0106tbl.Rows.Count - 1
                'チェック済は取得済を反映
                If Not IsNothing(WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID"))) Then
                    WW_RTN = WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID")).Text
                Else

                    '対象チェック以外は処理しない
                    If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

                    WW_CheckBox = True

                    S0021_SELECT(CO0106tbl.Rows(i))

                    Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")

                    'pingがOKの場合のみサービス稼働をチェックする
                    RemoteSrvCheck(WW_ServerName, RemoteServiceCmd.C_START_SERVICE, WW_RTN)
                    WW_CHECKED.Items.Add(New ListItem(WW_RTN, CO0106tbl.Rows(i)("TERMID")))
                End If

                Select Case WW_RTN
                    Case "ServiceRunning"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "ServiceStopped"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "RunningJOB"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "集配信実行中のためスキップ"
                    Case Else
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "サービス異常か、接続できない"
                End Select
            Next
        End Using
        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If
    End Sub

    ''' <summary>
    ''' サービス停止ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSSTOP_Click()
        Dim CS0009MESSAGEout As New BASEDLL.CS0009MESSAGEout        'Message out

        Dim WW_CheckBox As Boolean = False
        Dim WW_RTN As String = ""
        Using WW_CHECKED = New ListBox
            For i As Integer = 0 To CO0106tbl.Rows.Count - 1
                'チェック済は取得済を反映
                If Not IsNothing(WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID"))) Then
                    WW_RTN = WW_CHECKED.Items.FindByValue(CO0106tbl.Rows(i)("TERMID")).Text
                Else

                    '対象チェック以外は処理しない
                    If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For

                    WW_CheckBox = True

                    S0021_SELECT(CO0106tbl.Rows(i))

                    Dim WW_ServerName As String = CO0106tbl.Rows(i)("IPADDR")

                    'pingがOKの場合のみサービス停止をチェックする
                    RemoteSrvCheck(WW_ServerName, RemoteServiceCmd.C_STOP_SERVICE, WW_RTN)
                    WW_CHECKED.Items.Add(New ListItem(WW_RTN, CO0106tbl.Rows(i)("TERMID")))
                End If

                Select Case WW_RTN
                    Case "ServiceRunning"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.RUNNING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "ServiceStopped"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING
                        CO0106tbl.Rows(i)("SNOTES") = ""
                    Case "RunningJOB"
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "集配信実行中のためスキップ"
                    Case Else
                        CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG
                        CO0106tbl.Rows(i)("SNOTES") = "サービス異常？？"
                End Select
            Next
        End Using
        Repeater_init()

        ButtonCNTL()

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 配信ボタン処理  
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSEND_Click()

        Dim WW_CheckBox As Boolean = False

        rightview.setErrorReport("")
        'バージョンチェック
        Dim WW_ERR As String = "OFF"
        If WF_VER.Text = "" Then
            'エラーレポート編集
            Dim WW_ERR_MES As String = ""
            WW_ERR_MES = "・配信バージョンを入力してください。（YYYY/MM/DD HH:MM）"
            rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
            WW_ERR = "ON"
        Else
            Dim WW_DATE As DateTime
            If DateTime.TryParse(WF_VER.Text, WW_DATE) Then
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・配信バージョンが正しく入力してください。（YYYY/MM/DD HH:MM）"
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERR = "ON"
            End If
            If WF_VER.Text.Length = 16 Then
            Else
                'エラーレポート編集
                Dim WW_ERR_MES As String = ""
                WW_ERR_MES = "・配信バージョンが正しく入力してください。（YYYY/MM/DD HH:MM）"
                rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                WW_ERR = "ON"
            End If
        End If
        If WW_ERR = "ON" Then
            Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '--------------------------------------
        '配信処理（バッチファイル作成）
        '--------------------------------------
        Dim WW_FILEWRITE As String = "OFF"
        Dim WW_CHECKED = New HashSet(Of String)

        Dim sw As System.IO.StreamWriter = Nothing
        Try
            'ファイルが存在するかチェック。存在したら消す
            If System.IO.File.Exists(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_BAT_SEND_FILE_PATH) Then System.IO.File.Delete(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_BAT_SEND_FILE_PATH)

            sw = New System.IO.StreamWriter(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_BAT_SEND_FILE_PATH, True, System.Text.Encoding.Default)

            For i As Integer = 0 To CO0106tbl.Rows.Count - 1
                '対象チェック以外は処理しない
                If CType(WF_Repeater.Items(i).FindControl("WF_Rep_CheckBox"), System.Web.UI.WebControls.CheckBox).Checked = False Then Continue For
                'チェック済は対象外
                If WW_CHECKED.Contains(CO0106tbl.Rows(i)("TERMID")) Then Continue For

                WW_CheckBox = True
                WW_CHECKED.Add(CO0106tbl.Rows(i)("TERMID"))

                '操作端末（配信元）の場合は、当然配信しない
                If CO0106tbl.Rows(i)("TERMID") = CS0050Session.APSV_ID Then Continue For
                '接続（PING）、オンラインまたは、サービス停止中の場合のみ、配信する
                If (CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING OrElse
                    CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG) AndAlso
                    CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                Else
                    Dim WW_ERR_MES As String = ""
                    WW_ERR_MES = "・接続できないかオンラインまたは、サービスが停止中ではないため配信出来ません。"
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & " 端末ID 　 --> " & CO0106tbl.Rows(i)("TERMID") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & " 設置場所  --> " & CO0106tbl.Rows(i)("TERMNAME") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & " 接続状態  --> " & CO0106tbl.Rows(i)("NETSTAT") & " , "
                    WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & " ｻｰﾋﾞｽ状態 --> " & CO0106tbl.Rows(i)("SERVICESTAT") & " , "
                    rightview.addErrorReport(ControlChars.NewLine & WW_ERR_MES)
                    WW_ERR = "ON"
                    Continue For
                End If

                'パラメタ設定
                Dim WW_IpAddr As String = CO0106tbl.Rows(i)("TERMID")

                'ファイル出力
                If WF_ALLsned.Checked = True Then
                    sw.WriteLine(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_PGM_FILE_PATH & " /" & CO0106tbl.Rows(i)("TERMID") & " /" & CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_VERSION_FILE_PATH & " /ALL")
                Else
                    sw.WriteLine(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_PGM_FILE_PATH & " /" & CO0106tbl.Rows(i)("TERMID") & " /" & CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_VERSION_FILE_PATH & " /BIN")
                End If
                sw.Flush()
                WW_FILEWRITE = "ON"
            Next
        Catch ex As Exception
            Throw ex
        Finally
            If Not IsNothing(sw) Then sw.Close()
        End Try

        '--------------------------------------
        'ライブラリ配信処理
        '--------------------------------------
        If WW_FILEWRITE = "ON" Then
            Dim WW_RTN As Integer = 0

            Dim oProc As New Process
            Dim oSInfo As New ProcessStartInfo
            Dim Rtn As Integer = 0
            Dim pwd As String = "pad1"

            'バージョンファイル更新
            Try
                sw = New System.IO.StreamWriter(CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_VERSION_FILE_PATH, False, System.Text.Encoding.Default)
                sw.WriteLine(WF_VER.Text)
                sw.Flush()
            Catch ex As Exception
                Throw ex
            Finally
                If sw Is Nothing = False Then sw.Close()
            End Try

            '--------------------------------------
            'ジョブ起動
            '--------------------------------------
            'password文字列をSecureStringに変換する
            Dim ssPwd As New System.Security.SecureString()
            For Each c As Char In pwd
                ssPwd.AppendChar(c)
            Next

            'ジョブ起動パラメタ設定
            With oSInfo
                .FileName = CS0050Session.SYSTEM_PATH & GRCO0106WRKINC.C_BAT_SEND_FILE_PATH
                .Arguments = ""
                .CreateNoWindow = True ' コンソール・ウィンドウを開かない
                .UseShellExecute = False ' シェル機能を使用しない
            End With

            oProc.StartInfo = oSInfo
            oProc.Start()

            '後始末
            oProc.Dispose()

            If WW_RTN <> 0 Then
                Master.output(C_MESSAGE_NO.FILE_SEND_ERROR, C_MESSAGE_TYPE.ERR)

                Exit Sub
            End If
        End If

        '■■■ Detail初期設定 ■■■
        Repeater_init()

        ButtonCNTL()
        WF_ALLsned.Checked = False

        WF_ButtonALLCANCEL_Click()

        '■■■ 画面（GridView）表示データ保存 ■■■
        Master.SaveTable(CO0106tbl)

        If WW_CheckBox Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        If WW_ERR = "ON" Then
            Master.output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR)

        End If

    End Sub
    ''' <summary>
    ''' 終了ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub
    ''' <summary>
    ''' 右ボックスのラジオボタン選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButon_Click()
        '〇RightBox処理（ラジオボタン選択）
        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If
    End Sub
    ''' <summary>
    ''' メモ欄変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_MEMO_Change()
        '〇RightBox処理（右Boxメモ変更時）
        rightview.MAPID = Master.MAPID
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***　★
    ' ******************************************************************************

    ' ***  CO0106tbl初期設定処理
    Protected Sub MAPDATAget()


        Dim WW_TODOKECODE As String = ""
        Dim WW_UORG As String = ""


        '■■■ 内部テーブル格納用データ取得 ■■■

        '取引先部署内容検索
        Try
            '■テーブル検索結果をテーブル退避
            'CO0106テンポラリDB項目作成
            CO0106tbl_ColumnsAdd()

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String

                '検索SQL文
                SQLStr =
                         " SELECT  isnull(rtrim(A.TERMID),'')      as TERMID        " _
                       & "        ,isnull(rtrim(A.TERMNAME),'')    as TERMNAME      " _
                       & "        ,isnull(rtrim(A.IPADDR),'')      as IPADDR        " _
                       & "        ,isnull(rtrim(B.TERMID),'')      as TERMID_B      " _
                       & "        ,isnull(rtrim(C.CAMPCODE),'')    as CAMPCODE      " _
                       & "        ,isnull(rtrim(D.NAMES),'')       as CAMPNAME      " _
                       & " FROM       S0001_TERM             A                      " _
                       & " LEFT  JOIN S0021_LIBSENDSTAT      B                      " _
                       & "         ON B.TERMID         = A.TERMID                   " _
                       & " LEFT  JOIN S0029_ONLINESTAT       C                      " _
                       & "       INNER JOIN M0001_CAMP       D                      " _
                       & "               ON D.CAMPCODE   = C.CAMPCODE               " _
                       & "              and D.STYMD     <= SYSDATETIME()            " _
                       & "              and D.ENDYMD    >= SYSDATETIME()            " _
                       & "              and D.DELFLG    <> '1'                      " _
                       & "         ON C.TERMID         = A.TERMID                   " _
                       & " WHERE      A.TERMCLASS     >= '2'                        " _
                       & "        and rtrim(A.IPADDR) <> ''                         " _
                       & "        and A.DELFLG        <> '1'                        " _
                       & " ORDER BY A.TERMID ASC                                    "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()


                    'CO0106tbl値設定
                    Dim WW_DATA_CNT As Integer = 0
                    While SQLdr.Read

                        '○テーブル初期化
                        Dim CO0106row As DataRow = CO0106tbl.NewRow()

                        '○データ設定
                        WW_DATA_CNT += 1
                        CO0106row("LINECNT") = WW_DATA_CNT
                        CO0106row("OPERATION") = ""
                        CO0106row("TIMSTP") = "0"
                        CO0106row("SELECT") = "1"
                        CO0106row("HIDDEN") = "0"

                        '画面毎の設定項目
                        CO0106row("TERMID") = SQLdr("TERMID")
                        CO0106row("SENDSTAT") = "　"
                        CO0106row("SENDTIME") = "　"
                        CO0106row("TERMNAME") = SQLdr("TERMNAME")
                        CO0106row("CAMPCODE") = SQLdr("CAMPCODE")
                        CO0106row("CAMPNAME") = SQLdr("CAMPNAME")
                        CO0106row("IPADDR") = SQLdr("IPADDR")

                        CO0106row("NETSTAT") = "　"
                        CO0106row("SERVICESTAT") = "　"
                        CO0106row("ONLINESTAT") = "　"
                        CO0106row("NOTES") = ""
                        CO0106row("SNOTES") = ""
                        CO0106row("ONOTES") = ""
                        CO0106row("COLOR") = "background-color:white"

                        'ライブラリ配信マスタが存在しない場合、追加する
                        If String.IsNullOrEmpty(SQLdr("TERMID_B")) Then S0021_Insert(CO0106row, WW_RTN)

                        CO0106tbl.Rows.Add(CO0106row)

                    End While

                    SQLdr.Dispose()
                    SQLdr = Nothing
                End Using

            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT)
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0021_LIBSENDSTAT Select"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ***  ボタン状態制御
    Private Sub ButtonCNTL()

        WF_ButtonALLCANCEL.Enabled = True
        WF_ButtonALLSELECT.Enabled = True
        WF_ButtonEND.Enabled = True
        WF_ButtonPSTAT.Enabled = True
        WF_ButtonSSTAT.Enabled = True
        WF_ButtonOSTAT.Enabled = True

        If Master.MAPpermitcode >= C_PERMISSION.UPDATE Then
            WF_ButtonSEND.Enabled = True
            WF_ButtonOSTART.Enabled = True
            WF_ButtonOSTOP.Enabled = True
            WF_ButtonSSTART.Enabled = True
            WF_ButtonSSTOP.Enabled = True
        Else
            WF_ButtonSEND.Enabled = False
            WF_ButtonOSTART.Enabled = False
            WF_ButtonOSTOP.Enabled = False
            WF_ButtonSSTART.Enabled = False
            WF_ButtonSSTOP.Enabled = False
        End If

    End Sub

    ''' <summary>
    ''' CO0106tblカラム設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CO0106tbl_ColumnsAdd()

        If IsNothing(CO0106tbl) Then CO0106tbl = New DataTable
        If CO0106tbl.Columns.Count <> 0 Then CO0106tbl.Columns.Clear()
        'CO0106テンポラリDB項目作成
        CO0106tbl.Clear()
        CO0106tbl.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        CO0106tbl.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        CO0106tbl.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        CO0106tbl.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        CO0106tbl.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド

        CO0106tbl.Columns.Add("TERMID", GetType(String))
        CO0106tbl.Columns.Add("TERMNAME", GetType(String))
        CO0106tbl.Columns.Add("CAMPCODE", GetType(String))
        CO0106tbl.Columns.Add("CAMPNAME", GetType(String))
        CO0106tbl.Columns.Add("IPADDR", GetType(String))
        CO0106tbl.Columns.Add("NETSTAT", GetType(String))
        CO0106tbl.Columns.Add("ONLINESTAT", GetType(String))
        CO0106tbl.Columns.Add("SERVICESTAT", GetType(String))
        CO0106tbl.Columns.Add("SENDSTAT", GetType(String))
        CO0106tbl.Columns.Add("SENDTIME", GetType(String))
        CO0106tbl.Columns.Add("NOTES", GetType(String))
        CO0106tbl.Columns.Add("SNOTES", GetType(String))
        CO0106tbl.Columns.Add("ONOTES", GetType(String))
        CO0106tbl.Columns.Add("COLOR", GetType(String))

    End Sub

    ''' <summary>
    ''' Detail初期設定（明細作成とイベント追加）
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_init()
        '■■■ Detail変数設定 ■■■

        '背景色の設定（薄い青）
        Dim WW_COLOR As String = "#EEFFFF"

        For i As Integer = 0 To CO0106tbl.Rows.Count - 1
            '薄い青
            WW_COLOR = "#EEFFFF"

            If CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                '薄い黄色
                WW_COLOR = "#FFFF99"
            End If

            If CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                '薄い黄色
                WW_COLOR = "#FFFF99"
            End If

            If CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("SENDSTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                '薄いオレンジ
                WW_COLOR = "#FFAD90"
            End If

            CO0106tbl.Rows(i)("COLOR") = "background-color:" & WW_COLOR
        Next

        Dim WW_TBLview As DataView = New DataView(CO0106tbl)
        WF_Repeater.DataSource = WW_TBLview
        WF_Repeater.DataBind()  'Bind処理記述を行っていないので空行だけ作成される。

        For i As Integer = 0 To WF_Repeater.Items.Count - 1
            '背景色の設定（薄い青）
            WW_COLOR = "#EEFFFF"

            If CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                '薄い黄色
                WW_COLOR = "#FFFF99"
            End If

            If CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                '薄い黄色
                WW_COLOR = "#FFFF99"
            End If

            If CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse CO0106tbl.Rows(i)("SENDSTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                '薄いオレンジ
                WW_COLOR = "#FFAD90"
            End If
            '端末ID
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_TERMID"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("TERMID")
            '会社コード
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_CAMPNAME"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("CAMPNAME")
            '端末名称
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_TERMNAME"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("TERMNAME")
            'IPアドレス
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_IPADDR"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("IPADDR")
            '接続状態
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NETSTAT"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("NETSTAT")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NETSTAT"), System.Web.UI.WebControls.Label).Style.Remove("color")
            If CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_NETSTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "RED")
            End If
            'オンライン状態
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_ONLINESTAT"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("ONLINESTAT")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_ONLINESTAT"), System.Web.UI.WebControls.Label).Style.Remove("color")
            If CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_ONLINESTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "RED")
            End If
            If CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_ONLINESTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "BLUE")
            End If
            'セービス状態
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SERVICESTAT"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("SERVICESTAT")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SERVICESTAT"), System.Web.UI.WebControls.Label).Style.Remove("color")
            If CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_SERVICESTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "RED")
            End If
            If CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.STOPPING Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_SERVICESTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "BLUE")
            End If
            '状態
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SENDSTAT"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("SENDSTAT")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SENDSTAT"), System.Web.UI.WebControls.Label).Style.Remove("color")
            If CO0106tbl.Rows(i)("SENDSTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_SENDSTAT"), System.Web.UI.WebControls.Label).Style.Add("color", "RED")
            End If
            '配信日時
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_SENDTIME"), System.Web.UI.WebControls.Label).Text = CO0106tbl.Rows(i)("SENDTIME")
            '備考
            CO0106tbl.Rows(i)("NOTES") = CO0106tbl.Rows(i)("SNOTES") & " " & CO0106tbl.Rows(i)("ONOTES")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Text = CO0106tbl.Rows(i)("NOTES")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Style.Remove("color")
            If CO0106tbl(i)("TERMID") = HttpContext.Current.Session("APSRVname") Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Style.Add("color", "BLUE")
            Else
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Style.Add("color", "RED")
            End If
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Style.Remove("background-color")
            CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Style.Add("background-color", WW_COLOR)

            If CO0106tbl.Rows(i)("NETSTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse
               CO0106tbl.Rows(i)("ONLINESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse
               CO0106tbl.Rows(i)("SERVICESTAT") = GRCO0106WRKINC.C_STATUS.NG OrElse
               CO0106tbl.Rows(i)("SENDSTAT") = GRCO0106WRKINC.C_STATUS.NG Then
                CType(WF_Repeater.Items(i).FindControl("WF_Rep_NOTES"), System.Web.UI.WebControls.TextBox).Text = CO0106tbl.Rows(i)("NOTES")
            End If

        Next

        WF_Repeater.Visible = True

    End Sub

    ''' <summary>
    ''' ジョブ制御テーブル取得
    ''' </summary>
    ''' <param name="I_SERVICE_NAME">取得対象サービス名</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function S0019_SELECT(ByVal I_SERVICE_NAME As String) As Integer
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite            'LogOutput DirString Get
        Dim WW_JOBSTAT As String = ""
        Try

            '○ 配信先端末へのDB接続文字列を決定
            Dim WW_DBcon As String = HttpContext.Current.Session("DBcon")

            Dim WW_ConnectStr As String = WW_DBcon.Replace(CS0050Session.APSV_ID, I_SERVICE_NAME)

            'DataBase接続文字
            Dim SQLcon As New SqlConnection(WW_ConnectStr)
            SQLcon.Open() 'DataBase接続(Open)

            Dim SQL_Str As String = ""
            '指定された端末IDより振分先を取得
            SQL_Str = _
                    " SELECT isnull(JOBSTAT,0) as JOBSTAT " & _
                    " FROM S0019_JOBCNTL       " & _
                    " WHERE TERMID       =  '" & I_SERVICE_NAME & "' " & _
                    " AND   DELFLG       <> '1' "
            Dim SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                WW_JOBSTAT = Val(SQLdr("JOBSTAT"))
            End While
            If SQLdr.HasRows = False Then
                WW_JOBSTAT = 0
            End If

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "SELECT S0019_JOBCNTL")
            CS0011LOGWRITE.INFSUBCLASS = "S0019_SELECT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:SELECT S0019_JOBCNTL"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return 100
        End Try

        Return WW_JOBSTAT

    End Function

    ''' <summary>
    ''' ライブラリ配信テーブル検索
    ''' </summary>
    ''' <param name="IO_ROW">検索/更新対象行</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function S0021_SELECT(ByRef IO_ROW As DataRow) As Integer
        Dim WW_JOBSTAT As String = ""
        Try

            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQL_Str As String = ""
                '指定された端末IDより振分先を取得
                SQL_Str = _
                        " SELECT  isnull(SENDSTAT,'') as SENDSTAT " & _
                        "       , SENDTIME            as SENDTIME " & _
                        "       , isnull(NOTES,'')    as NOTES    " & _
                        " FROM   S0021_LIBSENDSTAT                " & _
                        " WHERE TERMID       =  '" & IO_ROW("TERMID") & "' "

                Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        If SQLdr("SENDSTAT") = GRCO0106WRKINC.C_STATUS.OK Then
                            IO_ROW("SENDSTAT") = "　"
                        Else
                            IO_ROW("SENDSTAT") = SQLdr("SENDSTAT")
                        End If
                        If IsDBNull(SQLdr("SENDTIME")) Then
                            IO_ROW("SENDTIME") = ""
                        Else
                            Dim WW_DATE As Date = SQLdr("SENDTIME")
                            IO_ROW("SENDTIME") = WW_DATE.ToString("yyyy/MM/dd HH:mm")
                        End If
                        If IO_ROW("TERMID") = CS0050Session.APSV_ID Then
                            IO_ROW("NOTES") = "このサーバーのライブラリを配信します。オンライン停止はできません。"
                        Else
                            IO_ROW("NOTES") = SQLdr("NOTES")
                        End If
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using

            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "SELECT S0021_LIBSENDSTAT")

            CS0011LOGWRITE.INFSUBCLASS = "S0021_SELECT"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:SELECT S0021_LIBSENDSTAT"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return 100
        End Try

        Return 0

    End Function

    ''' <summary>
    ''' ライブラリ配信ＤＢ追加 
    ''' </summary>
    ''' <param name="I_ROW">登録対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub S0021_Insert(ByVal I_ROW As DataRow,
                               ByRef O_RTN As String)

        Dim WW_DATENOW As DateTime = Date.Now
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection

                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String = _
                       "INSERT INTO S0021_LIBSENDSTAT " _
                     & "             (TERMID , " _
                     & "              SENDSTAT , " _
                     & "              SENDTIME , " _
                     & "              NOTES , " _
                     & "              INITYMD , " _
                     & "              UPDYMD , " _
                     & "              UPDUSER , " _
                     & "              UPDTERMID , " _
                     & "              RECEIVEYMD ) " _
                     & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09); "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 10)
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.SmallDateTime)
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 200)
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.SmallDateTime)
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.DateTime)
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 30)
                    Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.DateTime)

                    PARA01.Value = I_ROW("TERMID")
                    PARA02.Value = I_ROW("SENDSTAT")
                    PARA03.Value = DBNull.Value
                    PARA04.Value = I_ROW("NOTES")
                    PARA05.Value = WW_DATENOW
                    PARA06.Value = WW_DATENOW
                    PARA07.Value = Master.USERID
                    PARA08.Value = Master.USERTERMID
                    PARA09.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()

                End Using
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "SELECT S0021_LIBSENDSTAT")

            CS0011LOGWRITE.INFSUBCLASS = "S0021_Insert"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSERT S0021_LIBSENDSTAT"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 指定されたサーバー（IPアドレス）に対しPINGコマンド実行
    ''' </summary>
    ''' <param name="I_SERVICE_NAME"></param>
    ''' <returns>接続結果　TRUE：成功　FALSE：失敗</returns>
    ''' <remarks></remarks>
    Private Function CheckforPing(ByVal I_SERVICE_NAME As String) As Boolean
        '接続確認（ping）
        Dim p As New System.Net.NetworkInformation.Ping()
        Dim reply As System.Net.NetworkInformation.PingReply = p.Send(I_SERVICE_NAME, 3000)
        If reply.Status = System.Net.NetworkInformation.IPStatus.Success Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 遷移時の引き渡しパラメータの取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MAPrefelence()

        '■■■ 選択画面の入力初期値設定 ■■■
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then                                                    '条件画面からの画面遷移
            If IsNothing(Master.MAPID) Then Master.MAPID = GRCO0106WRKINC.MAPID
            '○Grid情報保存先のファイル名
            Master.createXMLSaveFile()
        End If

    End Sub


#Region "<< SERVICE CONTROLS>>"
    '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    'http://internetcom.jp/developer/20090113/26.html
    '親サービスとの依存関係を何も確認せずにサービスを起動する
    '1.objWinServという名称の新規サービスコントローラを作成します。 
    '2.サービス名とマシン名を割り当てます（リモートで呼び出しを行うため）。 
    '3.objWinServオブジェクトをサービス起動ルーチンに提供します。 
    '4.起動ルーチンは最初に目的のサービスのステータスをチェックし、サービスが停止しているかを確認します。 
    '5.その後、起動ルーチンはサービスの起動を試みます。ここでは、起動の最大待ち時間を20秒に設定しました。
    '　サービスが起動しない場合は、タイムアウト例外がスローされます。

    Private Function StartService(ByVal iServiceName As String, ByVal iServName As String) As Boolean
        Dim objWinServ As New ServiceController
        objWinServ.ServiceName = iServiceName
        objWinServ.MachineName = iServName

        If objWinServ.Status = ServiceControllerStatus.Stopped Then
            Try
                objWinServ.Start()
                objWinServ.WaitForStatus(ServiceControllerStatus.Running, _
                   System.TimeSpan.FromSeconds(20))
            Catch ex As System.ServiceProcess.TimeoutException
                Master.output(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, objWinServ.DisplayName & "：開始タイムアウト" & ex.Message)
                Return False
            Catch e As Exception
                Master.output(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, objWinServ.DisplayName & "：開始できない。" & e.Message)
                Return False
            End Try

        End If

        Return True

    End Function

    Private Function StopService(ByVal iServiceName As String, ByVal iServName As String) As Boolean
        Dim objWinServ As New ServiceController
        objWinServ.ServiceName = iServiceName
        objWinServ.MachineName = iServName

        If objWinServ.Status = ServiceControllerStatus.Running Then
            Try
                objWinServ.Stop()
                objWinServ.WaitForStatus(ServiceControllerStatus.Stopped, _
                   System.TimeSpan.FromSeconds(20))
            Catch ex As System.ServiceProcess.TimeoutException
                Master.output(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, objWinServ.DisplayName & "：開始タイムアウト" & ex.Message)

                Return False
            Catch e As Exception
                Master.output(C_MESSAGE_NO.DLL_IF_ERROR, C_MESSAGE_TYPE.ABORT, objWinServ.DisplayName & "：開始できない。" & e.Message)
                Return False
            End Try

        End If

        Return True

    End Function


    'サービスが存在するかチェックする、サービスの有無に応じてブール値を返す
    Public Function CheckforService(ByVal iServiceName As String, ByVal iServerName As String) As Boolean
        Dim Exist As Boolean = False
        Dim objWinServ As New ServiceController
        Dim ServiceStatus As ServiceControllerStatus

        objWinServ.ServiceName = iServiceName
        objWinServ.MachineName = iServerName

        Try
            ServiceStatus = objWinServ.Status
            Exist = True
        Catch ex As Exception
        Finally
            objWinServ = Nothing
        End Try
        Return Exist
    End Function

    'サービスのステータスを調る
    Public Function GetServiceStatus(ByVal iServiceName As String, ByVal iServername As String) As String

        Dim ServiceStatus As New ServiceController
        ServiceStatus.ServiceName = iServiceName
        ServiceStatus.MachineName = iServername

        Try
            If ServiceStatus.Status = ServiceControllerStatus.Running Then
                Return "Running"
            ElseIf ServiceStatus.Status = _
               ServiceControllerStatus.Stopped Then
                Return "Stopped"
            Else
                Return "Intermidiate"
            End If
        Catch ex As Exception
            Return "Stopped"
        Finally
            ServiceStatus = Nothing
        End Try

    End Function

    Private Function ExecOtherApplication(ByVal AppName As String, ByVal Args As String)
        Dim oProc As New Process
        Dim oSInfo As New ProcessStartInfo
        Dim Rtn As Integer = 0

        'ジョブ起動
        With oSInfo
            .FileName = AppName
            .Arguments = Args
            '.CreateNoWindow = True ' コンソール・ウィンドウを開かない
            '.UseShellExecute = False ' // シェル機能を使用しない
        End With

        oProc.StartInfo = oSInfo
        oProc.Start()

        'アプリケーションの終了を待つ
        oProc.WaitForExit()
        Rtn = oProc.ExitCode

        oProc.Dispose()

        Return Rtn
    End Function
    '''
    Private Sub RemoteSrvCheck(ByVal I_IP As String, ByVal I_CMD As String, ByRef O_RTN As String)
        '●パラメータパターン
        '(サービス状態確認)
        'iCMD : ServiceCONF
        'oRtn : Running、RunningJOB、Stopped、Intermidiate
        '(サービス開始)
        'iCMD : StartService、StopService
        'oRtn : Stopped、ServiceRunning、ServiceNotRunning
        '(サービス停止)
        'iCMD : StopService
        'oRtn : Stopped、ServiceStopped、ServiceNotStopped

        '(オンラインサービス状態確認)
        'iCMD : ONLINESTATget
        'oRtn : オンラインステータス(0:業務サービス停止、1:業務サービス中)
        '(オンラインサービス更新)
        'iCMD : ONLINESTATset0、ONLINESTATset1
        'oRtn : オンラインステータス(0:業務サービス停止、1:業務サービス中)

        Try
            O_RTN = ""
            '○Web要求定義
            Dim WW_req As WebRequest = WebRequest.Create(HttpContext.Current.Request.Url.Scheme & "://" & I_IP & "/office/GR/GRCO0107SERVICE.ashx")

            ' 権限設定
            WW_req.Credentials = CredentialCache.DefaultCredentials

            '○ポスト・データの作成
            Dim WW_POSTitems As String = ""
            Dim WW_POSTitem As Hashtable = New Hashtable()

            '〇ポスト・データ編集(要求先アクションを指定：ServiceCONF、StartService、StopService）
            WW_POSTitem("text") = HttpUtility.UrlEncode(I_CMD, Encoding.UTF8)
            For Each k As String In WW_POSTitem.Keys
                WW_POSTitems = WW_POSTitem(k)
            Next

            Dim WW_data As Byte() = Encoding.UTF8.GetBytes(WW_POSTitems)
            WW_req.Method = "POST"
            WW_req.ContentType = "application/x-www-form-urlencoded"
            WW_req.ContentLength = WW_data.Length

            '○ポスト・データ書込み
            Dim reqStream As Stream = WW_req.GetRequestStream()
            reqStream.Write(WW_data, 0, WW_data.Length)
            reqStream.Close()

            '○ポスト実行
            Dim response As HttpWebResponse = CType(WW_req.GetResponse(), HttpWebResponse)

            '○結果取得
            Dim wRTN As String = ""
            Dim reader As New StreamReader(response.GetResponseStream())
            O_RTN = reader.ReadToEnd()

            '○Close
            reader.Close()
            response.Close()

        Catch ex As System.Net.WebException
            'HTTPプロトコルエラーかどうか調べる
            If ex.Status = System.Net.WebExceptionStatus.ProtocolError Then
                Select Case CType(ex.Response, HttpWebResponse).StatusCode
                    Case 301        'サービス確認時異常
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：サービス確認時異常" & ex.Message)
                        O_RTN = "err"
                    Case 302        'サービス開始時異常
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：サービス開始時異常" & ex.Message)
                        O_RTN = "err"
                    Case 303        'サービス停止時異常
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：サービス停止時異常" & ex.Message)
                        O_RTN = "err"
                    Case 304        'オンライン確認時異常
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：オンライン確認時異常" & ex.Message)
                        O_RTN = "err"
                    Case 305        'オンラインＤＢ更新時異常
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：オンラインＤＢ更新時異常" & ex.Message)
                        O_RTN = "err"
                    Case Else       '通信エラー
                        Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：オンラインＤＢ更新時異常" & ex.Message)
                        O_RTN = "err"
                End Select
            Else
                '通信エラー
                Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, I_IP & "：通信エラー" & ex.Message)
                O_RTN = "err"
            End If

        End Try

    End Sub
#End Region
End Class






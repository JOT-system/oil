Option Strict On

Imports System
Imports System.Collections
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Drawing
Imports BASEDLL

''' <summary>
''' マスターページクラス
''' </summary>
''' <remarks></remarks>
Public Class GRMasterPage
    Inherits MasterPage

#Region "<< BASEDLL.CS Series >>"
    ''' <summary>
    ''' セッション情報管理
    ''' </summary>
    Private CS0050SESSION As New CS0050SESSION              'セッション情報管理
    ''' <summary>
    ''' 明細画面の権限チェック
    ''' </summary>
    Private CS0007AUTHORmap As New CS0007CheckAuthority     '明細画面の権限チェック
    ''' <summary>
    ''' 画面 戻/先 URL取得
    ''' </summary>
    Private CS0017ForwardURL As New CS0017ForwardURL        '画面戻先URL取得
    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得
    ''' <summary>
    ''' TableData(Grid)退避
    ''' </summary>
    Private CS0031SaveTable As CS0031TABLEsave              'TableData(Grid)退避
    ''' <summary>
    ''' TableData(Grid)復元
    ''' </summary>
    Private CS0032RecoverTable As CS0032TABLERecover        'TableData(Grid)復元
    ''' <summary>
    ''' 例外文字排除 String Get
    ''' </summary>
    Private CS0010CHARstr As New CS0010CHARget              '例外文字排除 String Get
    ''' <summary>
    ''' 項目チェック
    ''' </summary>
    Private CS0036FCHECK As New CS0036FCHECK                '項目チェック
#End Region
#Region "<< INSTANCE DATA FIELD Series >>"
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    Private COMPANYCODE As String = String.Empty            '会社コード
#End Region

#Region "<< Event Handlers >>"
    ''' <summary>
    ''' ページ初期処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks >
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Init

        'ログオン及びスケジュールから呼ばれた場合はすべて無視
        If TypeOf Me.Page Is M00000LOGON OrElse
           TypeOf Me.Page Is MB0006SCHEDULE Then
            Return
        End If
        'セッションタイムアウト判定
        If IsNothing(Session(C_SESSION_KEY.USER_ID)) OrElse String.IsNullOrEmpty(Session(C_SESSION_KEY.USER_ID).ToString) Then
            Server.Transfer(C_URL.LOGIN)
            Exit Sub
        End If

        If IsPostBack Then
            'メッセージクリア
            footer.clear()
        Else
            'フッター初期化
            footer.Initialize()

            '画面間情報取得処理
            SetMAPValue()
        End If
        'オンラインサービス判定 
        Dim CS0008ONLINEstat As New CS0008ONLINEstat        'ONLINE状態
        CS0008ONLINEstat.COMPCODE = GetTargetComp()
        CS0008ONLINEstat.CS0008ONLINEstat()

        If Not isNormal(CS0008ONLINEstat.ERR) OrElse CS0008ONLINEstat.ONLINESW = 0 Then
            Server.Transfer(C_URL.LOGIN)
            Exit Sub
        End If

    End Sub

    ''' <summary>
    ''' ページロード処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>コンテンツページのロード処理後に実行される</remarks >
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try

            If IsPostBack Then
                Dim obj = Page.Master.FindControl("contents1").FindControl("WF_ButtonClick")
                Dim btnText As HtmlInputText = DirectCast(obj, HtmlInputText)
                '〇各ボタン押下処理
                If Not IsNothing(btnText) Then
                    Select Case btnText.Value
                        Case "HELP"
                            ShowHelp()
                    End Select

                End If

            Else
                '○ 全画面共通-タイトル設定
                Dim WW_RTN As String = String.Empty
                title.setTitle(MF_MAPID.Value, MF_MAPvariant.Value, COMPANYCODE, WW_RTN, CS0050SESSION.USERID)
                If Not isNormal(WW_RTN) Then
                    footer.output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "表題設定エラー")
                    Exit Sub
                End If

            End If

        Catch ex As Threading.ThreadAbortException
        Catch ex As Exception
        Finally
            'サーバー処理終了
            MF_SUBMIT.Value = "FALSE"
        End Try

    End Sub

    ''' <summary>
    ''' ページ表示前処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>カスタムページのロード処理後に実行される</remarks >
    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As EventArgs) Handles Me.PreRender
        If IsPostBack Then
            Dim obj = Page.Master.FindControl("contents1").FindControl("WF_ButtonClick")
            Dim btnText As HtmlInputText = DirectCast(obj, HtmlInputText)

            btnText.Value = String.Empty
        End If
    End Sub
#End Region

#Region "<< Public Methods >>"
    ''' <summary>
    ''' 画面間情報取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetMAPValue()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.LOGIN OrElse
           Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then                                                      'メニューからの画面遷移
            Dim USRCOMPCODE As String = String.Empty
            'メニュー情報のWF_SEL退避　
            If Not IsNothing(Page.PreviousPage) AndAlso Not IsNothing(Page.PreviousPage.Master) Then
                With Page.PreviousPage.Master
                    '〇前画面のMF退避
                    MF_MAPID.Value = DirectCast(.FindControl("MF_MAPID"), HiddenField).Value                         'MAPID
                    MF_MAPvariant.Value = DirectCast(.FindControl("MF_MAPvariant"), HiddenField).Value               'MAP変数
                    MF_MAPpermitcode.Value = DirectCast(.FindControl("MF_MAPpermitcode"), HiddenField).Value         'MAP権限
                End With
            Else
                MAPID = CS0050SESSION.VIEW_MAPID
                MF_MAPvariant.Value = CS0050SESSION.VIEW_MAP_VARIANT 'MAP変数
                MF_MAPpermitcode.Value = CS0050SESSION.VIEW_PERMIT   'MAP権限

            End If
            MF_VIEWID.Value = String.Empty                       '画面
            MF_XMLsaveF.Value = String.Empty                     '画面情報退避F
            '〇ユーザ権限情報取得
            CS0051UserInfo.USERID = CS0050SESSION.USERID

            CS0051UserInfo.getInfo()
            If isNormal(CS0051UserInfo.ERR) Then
                USERID = CS0051UserInfo.USERID
                ROLE_COMP = CS0051UserInfo.CAMPROLE
                ROLE_MAP = CS0051UserInfo.MAPROLE
                ROLE_ORG = CS0051UserInfo.ORGROLE
                PROF_REPORT = CS0051UserInfo.RPRTPROFID
                PROF_VIEW = CS0051UserInfo.VIEWPROFID
                USRCOMPCODE = CS0051UserInfo.CAMPCODE
                USER_ORG = CS0051UserInfo.ORG
            End If
            CS0051UserInfo.BelongtoServer()
            If isNormal(CS0051UserInfo.ERR) Then
                USERTERMID = CS0051UserInfo.SERVERID
            Else
                USERTERMID = CS0050SESSION.APSV_ID
            End If
            '○画面間の情報クリア
            Dim myWork = Page.Master.FindControl("contents1").FindControl("work")
            'ワーク領域存在時のみ処理
            If Not IsNothing(myWork) Then
                For Each ctl In myWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim meObj As TextBox = DirectCast(ctl, TextBox)
                        meObj.Text = String.Empty

                        If meObj.ClientID.Contains("CAMPCODE") Then
                            meObj.Text = USRCOMPCODE
                        End If
                    End If
                Next
            End If

        ElseIf Not IsNothing(Page.PreviousPage) AndAlso Not IsNothing(Page.PreviousPage.Master) Then

            With Page.PreviousPage.Master
                '〇メニュー情報のMF_SEL退避
                MF_MAPID.Value = DirectCast(.FindControl("MF_MAPID"), HiddenField).Value               'MAPID

                MF_MAPvariant.Value = DirectCast(.FindControl("MF_MAPvariant"), HiddenField).Value               'MAP変数
                MF_MAPpermitcode.Value = DirectCast(.FindControl("MF_MAPpermitcode"), HiddenField).Value         'MAP権限
                MF_VIEWID.Value = DirectCast(.FindControl("MF_VIEWID"), HiddenField).Value                       '画面
                MF_XMLsaveF.Value = DirectCast(.FindControl("MF_XMLsaveF"), HiddenField).Value                                                                                    '画面情報退避F
                '〇ユーザ権限情報取得
                MF_USERID.Value = DirectCast(.FindControl("MF_USERID"), HiddenField).Value
                MF_COMP_ROLE.Value = DirectCast(.FindControl("MF_COMP_ROLE"), HiddenField).Value
                MF_MAP_ROLE.Value = DirectCast(.FindControl("MF_MAP_ROLE"), HiddenField).Value
                MF_ORG_ROLE.Value = DirectCast(.FindControl("MF_ORG_ROLE"), HiddenField).Value
                MF_PROF_REPORT.Value = DirectCast(.FindControl("MF_PROF_REPORT"), HiddenField).Value
                MF_PROF_VIEW.Value = DirectCast(.FindControl("MF_PROF_VIEW"), HiddenField).Value
                MF_USER_ORG.Value = DirectCast(.FindControl("MF_USER_ORG"), HiddenField).Value
                MF_USERTERMID.Value = DirectCast(.FindControl("MF_USERTERMID"), HiddenField).Value

            End With
            '画面間情報ワーク領域取得
            Dim preWork = Page.PreviousPage.Master.FindControl("contents1").FindControl("work")
            Dim myWork = Page.Master.FindControl("contents1").FindControl("work")
            'ワーク領域存在時のみ処理
            If Not IsNothing(preWork) AndAlso Not IsNothing(myWork) Then
                For Each ctl In preWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim preObj As TextBox = DirectCast(ctl, TextBox)
                        Dim meObj As TextBox = DirectCast(myWork.FindControl(preObj.ClientID), TextBox)
                        If Not IsNothing(meObj) Then
                            meObj.Text = preObj.Text
                        End If
                    ElseIf TypeOf ctl Is ListBox Then
                        Dim preObj As ListBox = DirectCast(ctl, ListBox)
                        Dim meObj As ListBox = DirectCast(myWork.FindControl(preObj.ClientID), ListBox)
                        If Not IsNothing(meObj) Then
                            For Each item As ListItem In preObj.Items
                                meObj.Items.Add(item)
                            Next
                        End If
                    End If

                Next
            End If
        Else
            MAPID = CS0050SESSION.VIEW_MAPID
            MF_MAPvariant.Value = CS0050SESSION.VIEW_MAP_VARIANT 'MAP変数
            MF_MAPpermitcode.Value = CS0050SESSION.VIEW_PERMIT   'MAP権限
            '〇ユーザ権限情報取得
            Dim USRCOMPCODE As String = ""
            CS0051UserInfo.USERID = CS0050SESSION.USERID

            CS0051UserInfo.getInfo()
            If isNormal(CS0051UserInfo.ERR) Then
                USERID = CS0051UserInfo.USERID
                ROLE_COMP = CS0051UserInfo.CAMPROLE
                ROLE_MAP = CS0051UserInfo.MAPROLE
                ROLE_ORG = CS0051UserInfo.ORGROLE
                PROF_REPORT = CS0051UserInfo.RPRTPROFID
                PROF_VIEW = CS0051UserInfo.VIEWPROFID
                USRCOMPCODE = CS0051UserInfo.CAMPCODE
                USER_ORG = CS0051UserInfo.ORG
            End If

            '○画面間の情報クリア
            Dim myWork = Page.Master.FindControl("contents1").FindControl("work")
            'ワーク領域存在時のみ処理
            If Not IsNothing(myWork) Then
                For Each ctl In myWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim meObj As TextBox = DirectCast(ctl, TextBox)
                        meObj.Text = String.Empty

                        If meObj.ClientID.Contains("CAMPCODE") Then
                            meObj.Text = USRCOMPCODE
                        End If
                    End If
                Next
            End If

        End If

    End Sub

    ''' <summary>
    ''' ヘルプ画面表示
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ShowHelp()
        Dim HELPCAMP As String = GetTargetComp()
        footer.showHelp(MF_MAPID.Value, HELPCAMP, USERID)
    End Sub

    ''' <summary>
    ''' メッセージの設定処理
    ''' </summary>
    ''' <param name="msgNo"></param>
    ''' <param name="msgType"></param>
    ''' <param name="I_PARA01"></param>
    ''' <param name="I_PARA02"></param>
    ''' <remarks></remarks>
    Public Sub Output(ByVal msgNo As String, ByVal msgType As String, Optional ByVal I_PARA01 As String = Nothing, Optional ByVal I_PARA02 As String = Nothing)
        footer.output(msgNo, msgType, I_PARA01, I_PARA02)
    End Sub
    ''' <summary>
    ''' メッセージの設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ShowMessage()
        footer.outputMessage()
    End Sub

    ''' <summary>
    ''' 更新・参照権限の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <remarks>結果画面の権限判定用</remarks>
    Public Sub CheckParmissionCode(Optional ByVal COMPCODE As String = "")
        CS0007AUTHORmap.MAPID = MAPID
        CS0007AUTHORmap.ROLECODE_MAP = ROLE_MAP
        If Not String.IsNullOrEmpty(COMPCODE) Then
            CS0007AUTHORmap.COMPCODE = COMPCODE
            CS0007AUTHORmap.ROLECODE_COMP = ROLE_COMP
        End If
        CS0007AUTHORmap.check()
        If isNormal(CS0007AUTHORmap.ERR) Then
            If CS0007AUTHORmap.MAPPERMITCODE >= C_PERMISSION.REFERLANCE Then
                MAPpermitcode = CS0007AUTHORmap.MAPPERMITCODE
                Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
            Else
                Output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "画面:" & MAPID)
                Exit Sub
            End If
        Else
            Output(CS0007AUTHORmap.ERR, C_MESSAGE_TYPE.ABORT, "画面:" & MAPID)
            Exit Sub
        End If

    End Sub
    ''' <summary>
    ''' 前ページ遷移
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <remarks></remarks>
    Public Sub TransitionPrevPage(Optional ByRef COMPCODE As String = "")

        '〇画面戻先URL取得
        CS0017ForwardURL.MAPID = MF_MAPID.Value
        CS0017ForwardURL.VARI = MF_MAPvariant.Value
        If Not String.IsNullOrEmpty(COMPCODE) Then
            CS0017ForwardURL.CAMPCODE = COMPCODE
        Else
            Dim myWork = Page.Master.FindControl("contents1").FindControl("work")
            'ワーク領域存在時のみ処理
            If Not IsNothing(myWork) Then
                For Each ctl In myWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim meObj As TextBox = DirectCast(ctl, TextBox)
                        If meObj.ClientID.Contains("CAMPCODE") Then
                            CS0017ForwardURL.CAMPCODE = meObj.Text
                        End If
                    End If
                Next
            End If
        End If

        CS0017ForwardURL.getPreviusURL()
        If isNormal(CS0017ForwardURL.ERR) Then
            '次画面の変数セット
            CS0050SESSION.VIEW_MAP_VARIANT = CS0017ForwardURL.VARI_RETURN
            CS0050SESSION.VIEW_MAPID = CS0017ForwardURL.MAP_RETURN
            Me.MAPvariant = CS0017ForwardURL.VARI_RETURN
            Me.MAPID = CS0017ForwardURL.MAP_RETURN
            '画面遷移実行
            Server.Transfer("../" & CS0017ForwardURL.URL)
        Else
            footer.output(CS0017ForwardURL.ERR, BASEDLL.C_MESSAGE_TYPE.ABORT, "getPreviusURL")
        End If

    End Sub

    ''' <summary>
    ''' ページ遷移
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <remarks></remarks>
    Public Sub TransitionPage(Optional ByRef COMPCODE As String = "")

        '〇画面遷移先URL取得
        CS0017ForwardURL.MAPID = MF_MAPID.Value
        CS0017ForwardURL.VARI = MF_MAPvariant.Value
        If Not String.IsNullOrEmpty(COMPCODE) Then
            CS0017ForwardURL.CAMPCODE = COMPCODE
        Else
            Dim myWork = Page.Master.FindControl("contents1").FindControl("work")
            'ワーク領域存在時のみ処理
            If Not IsNothing(myWork) Then
                For Each ctl In myWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim meObj As TextBox = DirectCast(ctl, TextBox)
                        If meObj.ClientID.Contains("CAMPCODE") Then
                            CS0017ForwardURL.CAMPCODE = meObj.Text
                        End If
                    End If
                Next
            End If
        End If
        CS0017ForwardURL.getNextURL()
        If isNormal(CS0017ForwardURL.ERR) Then
            CS0050SESSION.VIEW_MAP_VARIANT = CS0017ForwardURL.VARI_RETURN
            CS0050SESSION.VIEW_MAPID = CS0017ForwardURL.MAP_RETURN
            Me.MAPvariant = CS0017ForwardURL.VARI_RETURN
            Me.MAPID = CS0017ForwardURL.MAP_RETURN
            Server.Transfer("../" & CS0017ForwardURL.URL)
        Else
            footer.output(CS0017ForwardURL.ERR, BASEDLL.C_MESSAGE_TYPE.ABORT, "getNextURL")
        End If

    End Sub

    ''' <summary>
    ''' 退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateXMLSaveFile()
        MF_XMLsaveF.Value = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            MF_USERID.Value & "-" & MF_MAPID.Value & "-" & MF_MAPvariant.Value & "-" & Date.Now.ToString("HHmmss") & ".txt"

    End Sub

    ''' <summary>
    ''' 画面表示データ保存
    ''' </summary>
    ''' <param name="IO_TABLE">テーブルデータ</param>
    ''' <param name="I_XMLsaveF ">テーブルデータ格納ファイル</param>
    ''' <returns>結果</returns>
    ''' <remarks></remarks>
    Public Function SaveTable(ByRef IO_TABLE As DataTable, Optional ByVal I_XMLsaveF As String = "") As Boolean
        Dim rtn As Boolean = True
        Try
            CS0031SaveTable = New CS0031TABLEsave With {
                .FILEDIR = XMLsaveF,
                .TBLDATA = IO_TABLE,
                .SAVEMODE = CS0031TABLEsave.SAVING_MODE.WITH_HEADER
            }
            If Not String.IsNullOrEmpty(I_XMLsaveF) Then
                CS0031SaveTable.FILEDIR = I_XMLsaveF
            End If
            CS0031SaveTable.CS0031TABLEsave()
            If Not isNormal(CS0031SaveTable.ERR) Then
                footer.output(CS0031SaveTable.ERR, C_MESSAGE_TYPE.ABORT, "CS0031TABLEsave")
                rtn = False
            End If
        Catch ex As Exception
        Finally
            CS0031SaveTable = Nothing
        End Try

        Return rtn

    End Function
    ''' <summary>
    ''' 画面表示データ復元
    ''' </summary>
    ''' <param name="IO_TABLE">テーブルデータ</param>
    ''' <param name="I_XMLsaveF ">テーブルデータ格納ファイル</param>
    ''' <returns>結果</returns>
    ''' <remarks></remarks>
    Public Function RecoverTable(ByRef IO_TABLE As DataTable, Optional ByVal I_XMLsaveF As String = "") As Boolean
        Dim rtn As Boolean = True
        Try
            CS0032RecoverTable = New CS0032TABLERecover With {
                .FILEDIR = XMLsaveF,
                .TBLDATA = IO_TABLE,
                .RECOVERMODE = CS0032TABLERecover.RECOVERY_MODE.WITH_HEADER
            }
            If Not String.IsNullOrEmpty(I_XMLsaveF) Then
                CS0032RecoverTable.FILEDIR = I_XMLsaveF
            End If
            CS0032RecoverTable.CS0032TABLERecover()
            If isNormal(CS0032RecoverTable.ERR) Then
                IO_TABLE = CS0032RecoverTable.OUTTBL
            Else
                footer.output(CS0032RecoverTable.ERR, C_MESSAGE_TYPE.ABORT, "CS0032TABLERecover")
                rtn = False
            End If
        Catch ex As Exception
        Finally
            CS0032RecoverTable = Nothing
        End Try

        Return rtn

    End Function

    ''' <summary>
    ''' 画面表示データのヘッダー部分のみ復元
    ''' </summary>
    ''' <param name="IO_TABLE">テーブルデータ</param>
    ''' <param name="I_XMLsaveF ">テーブルデータ格納ファイル</param>
    ''' <returns>結果</returns>
    ''' <remarks></remarks>
    Public Function CreateEmptyTable(ByRef IO_TABLE As DataTable, Optional ByVal I_XMLsaveF As String = "") As Boolean

        CS0032RecoverTable = New CS0032TABLERecover With {
            .FILEDIR = XMLsaveF,
            .TBLDATA = IO_TABLE,
            .RECOVERMODE = CS0032TABLERecover.RECOVERY_MODE.HEAD_ONLY
        }
        If Not String.IsNullOrEmpty(I_XMLsaveF) Then
            CS0032RecoverTable.FILEDIR = I_XMLsaveF
        End If
        CS0032RecoverTable.CS0032TABLERecover()
        If isNormal(CS0032RecoverTable.ERR) Then
            IO_TABLE = CS0032RecoverTable.OUTTBL
        Else
            footer.output(CS0032RecoverTable.ERR, C_MESSAGE_TYPE.ABORT, "CS0032TABLERecover")
            Return False
        End If

        Return True

    End Function
    ''' <summary>
    ''' 使用禁止文字を除去する
    ''' </summary>
    ''' <param name="IO_VALUE">除去対象文字列（除去済も設定される）</param>
    ''' <returns>除去後の文字列</returns>
    ''' <remarks></remarks>
    Public Function EraseCharToIgnore(ByRef IO_VALUE As String) As String
        '○ 入力文字置き換え ※画面PassWord内の使用禁止文字排除
        CS0010CHARstr.CHARIN = IO_VALUE
        CS0010CHARstr.CS0010CHARget()
        IO_VALUE = CS0010CHARstr.CHAROUT
        Return IO_VALUE
    End Function
    ''' <summary>
    ''' 使用禁止文字を除去する
    ''' </summary>
    ''' <param name="I_VALUE">除去対象文字列</param>
    ''' <param name="O_VALUE">除去後の文字列</param>
    ''' <returns>除去後の文字列</returns>
    ''' <remarks></remarks>
    Public Function EraseCharToIgnore(ByVal I_VALUE As String, ByRef O_VALUE As String) As String
        '○ 入力文字置き換え ※画面PassWord内の使用禁止文字排除
        CS0010CHARstr.CHARIN = I_VALUE
        CS0010CHARstr.CS0010CHARget()
        O_VALUE = CS0010CHARstr.CHAROUT
        Return O_VALUE
    End Function
    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="IO_VALUE">チェック対象の値</param>
    ''' <param name="O_MESSAGENO">エラーメッセージ</param>
    ''' <param name="O_CHECKREPORT">エラー内容</param>
    ''' <param name="I_LEN_SAME_FLG">固定桁数チェックフラグ</param>
    ''' <remarks></remarks>
    Public Sub CheckField(ByVal I_COMPCODE As String, ByVal I_FIELD As String, ByRef IO_VALUE As String, ByRef O_MESSAGENO As String, ByRef O_CHECKREPORT As String, Optional ByVal I_LEN_SAME_FLG As Boolean = False)
        CS0036FCHECK.CAMPCODE = I_COMPCODE                              '会社コード
        CS0036FCHECK.MAPID = Me.MAPID                                   '画面ID
        CS0036FCHECK.FIELD = I_FIELD                                    '項目名
        CS0036FCHECK.VALUE = IO_VALUE                                   '値
        CS0036FCHECK.SAMEFLG = I_LEN_SAME_FLG                           '固定桁数チェックフラグ
        CS0036FCHECK.check()

        O_MESSAGENO = CS0036FCHECK.ERR
        O_CHECKREPORT = CS0036FCHECK.CHECKREPORT

        If isNormal(CS0036FCHECK.ERR) Then
            IO_VALUE = CS0036FCHECK.VALUEOUT
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック処理チェック存在確認
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="IO_TBL" >チェック用DATAFIELDテーブル</param>
    ''' <returns>存在：TRUE　未存在：FALSE</returns>
    ''' <remarks></remarks>
    Public Function ExistCheckTable(ByVal I_COMPCODE As String, ByVal I_FIELD As String,
                                    ByRef IO_TBL As DataTable) As Boolean

        CS0036FCHECK.CAMPCODE = I_COMPCODE                          '会社コード
        CS0036FCHECK.MAPID = Me.MAPID                               '画面ID
        CS0036FCHECK.FIELD = I_FIELD                                '項目名
        CS0036FCHECK.TBL = IO_TBL                                   'S0013_DATAFIELDテーブル
        Return CS0036FCHECK.existsCheckField()

    End Function
    ''' <summary>
    ''' 単項目チェック処理チェックテーブル保持版
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="IO_VALUE">チェック対象の値</param>
    ''' <param name="O_MESSAGENO">エラーメッセージ</param>
    ''' <param name="O_CHECKREPORT">エラー内容</param>
    ''' <param name="IO_TBL" >チェック用DATAFIELDテーブル</param>
    ''' <returns>チェック後の対象値　エラー時はEMPTY</returns>
    ''' <remarks></remarks>
    Public Function CheckFieldForTable(ByVal I_COMPCODE As String, ByVal I_FIELD As String, ByRef IO_VALUE As String, ByRef O_MESSAGENO As String, ByRef O_CHECKREPORT As String, ByRef IO_TBL As DataTable) As String

        CS0036FCHECK.CAMPCODE = I_COMPCODE                          '会社コード
        CS0036FCHECK.MAPID = Me.MAPID                               '画面ID
        CS0036FCHECK.FIELD = I_FIELD                                '項目名
        CS0036FCHECK.VALUE = IO_VALUE                               '値
        CS0036FCHECK.TBL = IO_TBL                                   'S0013_DATAFIELDテーブル
        CS0036FCHECK.CS0036FCHECK()

        O_MESSAGENO = CS0036FCHECK.ERR
        O_CHECKREPORT = CS0036FCHECK.CHECKREPORT

        If isNormal(CS0036FCHECK.ERR) Then
            IO_VALUE = CS0036FCHECK.VALUEOUT
            Return IO_VALUE
        Else
            Return String.Empty
        End If

    End Function

    ''' <summary>
    ''' 単項目チェック処理チェックテーブル保持版
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">チェック対象の値</param>
    ''' <param name="O_MESSAGENO">エラーメッセージ</param>
    ''' <param name="O_CHECKREPORT">エラー内容</param>
    ''' <param name="O_VALUE">チェック後の値</param>
    ''' <param name="IO_TBL" >チェック用DATAFIELDテーブル</param>
    ''' <returns>チェック後の対象値　エラー時はEMPTY</returns>
    ''' <remarks></remarks>
    Public Function CheckFieldForTable(ByVal I_COMPCODE As String, ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_MESSAGENO As String, ByRef O_CHECKREPORT As String, ByRef O_VALUE As String, ByRef IO_TBL As DataTable) As String

        CS0036FCHECK.CAMPCODE = I_COMPCODE                          '会社コード
        CS0036FCHECK.MAPID = Me.MAPID                               '画面ID
        CS0036FCHECK.FIELD = I_FIELD                                '項目名
        CS0036FCHECK.VALUE = I_VALUE                               '値
        CS0036FCHECK.TBL = IO_TBL                                   'S0013_DATAFIELDテーブル
        CS0036FCHECK.CS0036FCHECK()

        O_MESSAGENO = CS0036FCHECK.ERR
        O_CHECKREPORT = CS0036FCHECK.CHECKREPORT
        O_VALUE = CS0036FCHECK.VALUEOUT
        Return O_VALUE
    End Function
    ''' <summary>
    ''' 項目に対する変数の取得
    ''' </summary>
    ''' <param name="I_COMPCODE" >会社コード</param>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="O_VALUE">出力変数</param>
    ''' <param name="O_RTN" >成功可否</param>
    ''' <returns >出力変数</returns>
    ''' <remarks></remarks>
    Public Function GetFirstValue(ByVal I_COMPCODE As String, ByVal I_FIELD As String, ByRef O_VALUE As String, Optional ByRef O_RTN As String = Nothing) As String
        Dim CS0016ProfMValue As New CS0016ProfMValue               '変数情報取

        '○変数設定処理 
        CS0016ProfMValue.PROFID = Me.PROF_VIEW
        CS0016ProfMValue.MAPID = Me.MAPID
        CS0016ProfMValue.CAMPCODE = I_COMPCODE
        CS0016ProfMValue.VARI = MAPvariant
        CS0016ProfMValue.FIELD = I_FIELD
        CS0016ProfMValue.getInfo()
        O_RTN = CS0016ProfMValue.ERR
        If isNormal(CS0016ProfMValue.ERR) Then
            O_VALUE = CS0016ProfMValue.VALUE
        Else
            O_VALUE = Nothing
        End If
        GetFirstValue = O_VALUE
    End Function

    ''' <summary>
    ''' ポップアップ確認画面表示
    ''' </summary>
    ''' <param name="I_MSGNO"></param>
    ''' <param name="I_PARA01"></param>
    ''' <param name="I_PARA02"></param>
    Public Sub ConfirmWindow(ByVal I_MSGNO As String, Optional ByVal I_PARA01 As String = Nothing, Optional ByVal I_PARA02 As String = Nothing)

        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim obj = Page.Master.FindControl("contents1").FindControl("WF_ButtonClick")
        Dim btnText As HtmlInputText = DirectCast(obj, HtmlInputText)
        Dim objMessage As New Label With {.Text = ""}

        CS0009MESSAGEout.MESSAGENO = I_MSGNO
        CS0009MESSAGEout.NAEIW = C_MESSAGE_TYPE.INF
        CS0009MESSAGEout.MESSAGEBOX = objMessage
        If Not String.IsNullOrEmpty(I_PARA01) Then CS0009MESSAGEout.PARA01 = I_PARA01
        If Not String.IsNullOrEmpty(I_PARA02) Then CS0009MESSAGEout.PARA02 = I_PARA02
        CS0009MESSAGEout.CS0009MESSAGEout()
        If Not IsNothing(btnText) AndAlso isNormal(CS0009MESSAGEout.ERR) Then
            MF_AGAIN.Value = btnText.Value
            MF_ALT_MSG.Value = objMessage.Text
            MF_ALERT.Value = "TRUE"
        End If

    End Sub
    ''' <summary>
    ''' ポップアップ確認画面判定
    ''' </summary>
    ''' <returns>OK=True キャンセル=False</returns>
    Public Function ConfirmOK() As Boolean
        Return MF_ALERT.Value = "OK"
    End Function
#End Region
#Region "<< Local Methods >>"

    ''' <summary>
    ''' 画面及びパラメータに設定されている会社コードを取得する
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Function GetTargetComp() As String

        '〇会社コード取得
        Dim COMPCODE As String = String.Empty
        Dim mySearch = Page.Master.FindControl("contents1")
        Dim myWork = Page.Master.FindControl("contents1").FindControl("work")

        '検索条件枠から取得
        If Not IsNothing(mySearch) Then
            For Each ctl In mySearch.Controls
                ' 検索条件領域内TextBoxが処理対象
                If TypeOf ctl Is TextBox Then
                    Dim meObj As TextBox = DirectCast(ctl, TextBox)
                    If meObj.ClientID.Contains("WF_CAMPCODE") Then COMPCODE = meObj.Text
                End If
            Next
        End If
        If String.IsNullOrEmpty(COMPCODE) Then
            If Not IsNothing(myWork) Then
                For Each ctl In myWork.Controls
                    ' ワーク領域内TextBoxが処理対象
                    If TypeOf ctl Is TextBox Then
                        Dim meObj As TextBox = DirectCast(ctl, TextBox)
                        If meObj.ClientID.Contains("CAMPCODE") Then COMPCODE = meObj.Text
                    End If
                Next
            End If
        End If
        Return COMPCODE
    End Function
#End Region

#Region "<< Property Accessor >>"
    ''' <summary>
    ''' ヘルプ有無設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Property dispHelp As Boolean
        Get
            If MF_HELP.Value = "TRUE" Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(value As Boolean)
            If value Then
                MF_HELP.Value = "TRUE"
                footer.EnabledHelp()
            Else
                MF_HELP.Value = "FALSE"
                footer.DisabledHelp()
            End If
        End Set
    End Property

    ''' <summary>
    ''' DROP有無設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Property eventDrop As Boolean
        Get
            If MF_DROP.Value = "TRUE" Then
                Return True
            Else
                Return False
            End If
        End Get
        Set(value As Boolean)
            Dim myForm = DirectCast(Page.Master.FindControl("GRMasterPage"), System.Web.UI.HtmlControls.HtmlForm)
            myForm.Attributes.Remove("ondrop")
            If value = True Then
                MF_DROP.Value = "TRUE"
                myForm.Attributes.Add("ondrop", "f_dragEvent(event);")
            Else
                MF_DROP.Value = "FALSE"
                myForm.Attributes.Add("ondrop", "f_dragEventCancel(event);")
            End If
        End Set
    End Property
    ''' <summary>
    ''' MAPID
    ''' </summary>
    Property MAPID As String
        Get
            Return MF_MAPID.Value
        End Get
        Set(value As String)
            MF_MAPID.Value = value
        End Set
    End Property

    ''' <summary>
    ''' MAPvariant
    ''' </summary>
    Property MAPvariant As String
        Get
            Return MF_MAPvariant.Value
        End Get
        Set(value As String)
            MF_MAPvariant.Value = value
        End Set
    End Property
    ''' <summary>
    ''' MAPpermitcode
    ''' </summary>
    Property MAPpermitcode As String
        Get
            Return MF_MAPpermitcode.Value
        End Get
        Set(value As String)
            MF_MAPpermitcode.Value = value
        End Set
    End Property

    ''' <summary>
    ''' VIEWID
    ''' </summary>
    Property VIEWID As String
        Get
            Return MF_VIEWID.Value
        End Get
        Set(value As String)
            MF_VIEWID.Value = value
        End Set
    End Property
    ''' <summary>
    ''' XMLsaveF
    ''' </summary>
    Property XMLsaveF As String
        Get
            Return MF_XMLsaveF.Value
        End Get
        Set(value As String)
            MF_XMLsaveF.Value = value
        End Set
    End Property
    ''' <summary>
    ''' PROF_VIEW
    ''' </summary>
    Property PROF_VIEW As String
        Get
            Return MF_PROF_VIEW.Value
        End Get
        Set(value As String)
            MF_PROF_VIEW.Value = value
        End Set
    End Property
    ''' <summary>
    ''' PROF_REPORT
    ''' </summary>
    Property PROF_REPORT As String
        Get
            Return MF_PROF_REPORT.Value
        End Get
        Set(value As String)
            MF_PROF_REPORT.Value = value
        End Set
    End Property
    ''' <summary>
    ''' ROLE_COMP
    ''' </summary>
    Property ROLE_COMP As String
        Get
            Return MF_COMP_ROLE.Value
        End Get
        Set(value As String)
            MF_COMP_ROLE.Value = value
        End Set
    End Property
    ''' <summary>
    ''' ROLE_MAP
    ''' </summary>
    Property ROLE_MAP As String
        Get
            Return MF_MAP_ROLE.Value
        End Get
        Set(value As String)
            MF_MAP_ROLE.Value = value
        End Set
    End Property
    ''' <summary>
    ''' ROLE_ORG
    ''' </summary>
    Property ROLE_ORG As String
        Get
            Return MF_ORG_ROLE.Value
        End Get
        Set(value As String)
            MF_ORG_ROLE.Value = value
        End Set
    End Property
    ''' <summary>
    ''' USERID
    ''' </summary>
    Property USERID As String
        Get
            Return MF_USERID.Value
        End Get
        Set(value As String)
            MF_USERID.Value = value
        End Set
    End Property
    ''' <summary>
    ''' USER_ORG
    ''' </summary>
    Property USER_ORG As String
        Get
            Return MF_USER_ORG.Value
        End Get
        Set(value As String)
            MF_USER_ORG.Value = value
        End Set
    End Property
    ''' <summary>
    ''' COMPANYCODE
    ''' </summary>
    Property LOGINCOMP As String
        Get
            Return Nothing
        End Get
        Set(value As String)
            COMPANYCODE = value
        End Set
    End Property
    ''' <summary>
    ''' USERTERMID
    ''' </summary>
    Property USERTERMID As String
        Get
            Return MF_USERTERMID.Value
        End Get
        Set(value As String)
            MF_USERTERMID.Value = value
        End Set
    End Property
#End Region
End Class
'Option Strict On
'Option Explicit On

Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 組織マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class OIM0002OrgSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    'Private Const CONST_ORGCODE_INFOSYS As String = "010006"        '組織コード_情報システム部
    'Private Const CONST_ORGCODE_OIL As String = "010007"            '組織コード_石油部

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                  '検索ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '終了ボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        WF_FIELD_Change()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case "WF_RIGHT_VIEW_DBClick"        '右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                'メモ欄更新
                        WF_RIGHTBOX_Change()
                    Case "HELP"                         'ヘルプ表示
                        WF_HELP_Click()
                End Select
            End If
        Else
            '○ 初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        TxtCampCode.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        Master.MAPID = OIM0002WRKINC.MAPIDS
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU _
            OrElse Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            'ログインユーザー情報をWRKINCにセット


            TxtCampCodeMy.Text = Master.USERCAMP             '会社コード
            TxtOrgCodeMy.Text = Master.USER_ORG              '組織コード

            work.WF_SEL_CAMPCODE.Text = Master.USERCAMP             '会社コード
            work.WF_SEL_ORGCODE.Text = Master.USER_ORG              '組織コード

            '画面の入力項目にセット
            '会社コード
            'Master.GetFirstValue(work.WF_SEL_CAMPCODE2.Text, "CAMPCODE2", TxtCampCode.Text)
            TxtCampCode.Text = ""
            '組織コード
            'Master.GetFirstValue(work.WF_SEL_ORGCODE2.Text, "ORGCODE2", TxtOrgCode.Text)
            TxtOrgCode.Text = ""
            'ステータス選択
            RdBSearch1.Checked = True
            RdBSearch2.Checked = False

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0002L Then   '実行画面からの遷移
            '〇画面項目設定処理
            '会社コード
            TxtCampCodeMy.Text = work.WF_SEL_CAMPCODE.Text
            '運用部署
            TxtOrgCodeMy.Text = work.WF_SEL_ORGCODE.Text
            '会社コード2
            TxtCampCode.Text = work.WF_SEL_CAMPCODE2.Text
            '組織コード2
            TxtOrgCode.Text = work.WF_SEL_ORGCODE2.Text
            'ステータス選択
            If work.WF_SEL_SELECT.Text = 0 Then
                RdBSearch1.Checked = True
                RdBSearch2.Checked = False
            Else
                RdBSearch1.Checked = False
                RdBSearch2.Checked = True
            End If
        End If

        '会社コード・組織コードを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtCampCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0002WRKINC.MAPIDS
        rightview.MAPID = OIM0002WRKINC.MAPIDL
        rightview.COMPCODE = TxtCampCode.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", TxtCampCode.Text, txtCampName.Text, WW_DUMMY)
        '組織コード
        CODENAME_get("ORGCODE", TxtOrgCode.Text, txtOrgName.Text, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(TxtCampCodeMy.Text)
        '運用部署
        Master.EraseCharToIgnore(TxtOrgCodeMy.Text)

        '会社コード2
        Master.EraseCharToIgnore(TxtCampCode.Text)
        '組織コード2
        Master.EraseCharToIgnore(TxtOrgCode.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE2.Text = TxtCampCode.Text
        '組織コード
        work.WF_SEL_ORGCODE2.Text = TxtOrgCode.Text

        '検索条件
        If RdBSearch1.Checked = True Then
            work.WF_SEL_SELECT.Text = "0"                   '削除除く
        End If
        If RdBSearch2.Checked = True Then
            work.WF_SEL_SELECT.Text = "1"                   '削除のみ
        End If

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(TxtCampCodeMy.Text)
        End If

        Master.CheckParmissionCode(TxtCampCode.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(TxtCampCodeMy.Text, "CAMPCODE", TxtCampCodeMy.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", TxtCampCodeMy.Text, txtCampNameMy.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & TxtCampCodeMy.Text)
                TxtCampCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If

        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtCampCodeMy.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '運用部署
        WW_TEXT = TxtOrgCodeMy.Text
        Master.CheckField(TxtCampCodeMy.Text, "ORGCODE", TxtOrgCodeMy.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                TxtOrgCodeMy.Text = ""
            Else
                '存在チェック
                CODENAME_get("UORG", TxtOrgCodeMy.Text, txtOrgNameMy.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "組織コード : " & TxtOrgCodeMy.Text)
                    TxtOrgCodeMy.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtOrgCodeMy.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '会社コード２
        Master.CheckField(TxtCampCodeMy.Text, "CAMPCODE2", TxtCampCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", TxtCampCode.Text, txtCampName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & TxtCampCode.Text)
                TxtCampCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtCampCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '組織コード２
        WW_TEXT = TxtOrgCode.Text
        Master.CheckField(TxtCampCodeMy.Text, "ORGCODE2", TxtOrgCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                TxtOrgCode.Text = ""
            Else
                '存在チェック
                CODENAME_get("ORGCODE", TxtOrgCode.Text, txtOrgName.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "組織コード : " & TxtOrgCode.Text)
                    TxtOrgCode.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtOrgCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Dim prmData As New Hashtable
                Select Case WF_FIELD.Value
                    Case "WF_CAMPCODE"       '会社コード
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = TxtCampCode.Text
                    Case "WF_ORGCODE"       '組織コード
                        'prmData = work.CreateORGParam2(TxtCampCode.Text)
                        Dim AUTHORITYALL_FLG As String = "0"
                        'If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                        If TxtCampCode.Text = "" Then '会社コードが空の場合
                            AUTHORITYALL_FLG = "1"
                        Else '会社コードに入力済みの場合
                            AUTHORITYALL_FLG = "2"
                        End If
                        prmData = work.CreateORGParam(TxtCampCode.Text, AUTHORITYALL_FLG)
                End Select
                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()

            End With

        End If

    End Sub


    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", TxtCampCode.Text, txtCampName.Text, WW_RTN_SW)
            '組織コード
            Case "WF_ORGCODE"
                CODENAME_get("ORGCODE", TxtOrgCode.Text, txtOrgName.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                '### like検索を実施するため、存在チェックは外す(20191223) ###########################################
                'Case "TxtStationCode"
                '    '何もしない(like検索をするにあたって、「マスタが存在しない」旨を未出力とするため)
                Case Else
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End Select
        End If

    End Sub


    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                TxtCampCode.Text = WW_SelectValue
                txtCampName.Text = WW_SelectText
                TxtCampCode.Focus()

            Case "WF_ORGCODE"              '組織コード
                TxtOrgCode.Text = WW_SelectValue
                txtOrgName.Text = WW_SelectText
                TxtOrgCode.Focus()

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                TxtCampCode.Focus()
            Case "WF_ORGCODE"              '組織コード
                TxtOrgCode.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(TxtCampCode.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.ShowHelp()

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        'prmData.Item(C_PARAMETERS.LP_COMPANY) = TxtCampCode.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '組織コード
                    Dim AUTHORITYALL_FLG As String = "0"
                    If TxtCampCode.Text = "" Then '会社コードが空の場合
                        AUTHORITYALL_FLG = "1"
                    Else '会社コードに入力済みの場合
                        AUTHORITYALL_FLG = "2"
                    End If
                    prmData = work.CreateORGParam(TxtCampCodeMy.Text, AUTHORITYALL_FLG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORGCODE"             '組織コード
                    Dim AUTHORITYALL_FLG As String = "0"
                    If TxtCampCode.Text = "" Then '会社コードが空の場合
                        AUTHORITYALL_FLG = "1"
                    Else '会社コードに入力済みの場合
                        AUTHORITYALL_FLG = "2"
                    End If
                    prmData = work.CreateORGParam(TxtCampCode.Text, AUTHORITYALL_FLG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    Protected Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RdBSearch1.CheckedChanged

    End Sub
End Class

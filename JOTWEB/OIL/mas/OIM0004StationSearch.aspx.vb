'Option Strict On
'Option Explicit On

Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 貨物車マスタ登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class OIM0004StationSearch
    Inherits Page

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

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

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        Master.MAPID = OIM0004WRKINC.MAPIDS
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
            '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
            '運用部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", WF_UORG.Text)
            '貨物駅コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STATIONCODE", TxtStationCode.Text)
            '貨物コード枝番
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "BRANCH", TxtBranch.Text)
            '発着駅フラグ
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG", TxtDepArrStation.Text)
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0004L Then   '実行画面からの遷移
            '〇画面項目設定処理
            '会社コード
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            '貨物駅コード
            TxtStationCode.Text = work.WF_SEL_STATIONCODE.Text
            '貨物コード枝番
            TxtBranch.Text = work.WF_SEL_BRANCH.Text
            '発着駅フラグ
            TxtDepArrStation.Text = work.WF_SEL_DEPARRSTATIONFLG.Text
        End If

        '貨物駅コード・貨物コード枝番を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtStationCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtBranch.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0004WRKINC.MAPIDS
        rightview.MAPID = OIM0004WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        '201104-追加-START
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF
        '201104-追加-END

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        '貨物駅コード
        CODENAME_get("STATIONCODE", TxtStationCode.Text & TxtBranch.Text, LblStationCode.Text, WW_DUMMY)
        ''貨物コード枝番
        'CODENAME_get("BRANCH", TxtBranch.Text, LblBranch.Text, WW_DUMMY)
        '発着駅フラグ
        CODENAME_get("DEPARRSTATIONFLG", TxtDepArrStation.Text, LblDepArrStation.Text, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        '運用部署
        Master.EraseCharToIgnore(WF_UORG.Text)
        '貨物駅コード
        Master.EraseCharToIgnore(TxtStationCode.Text)
        '貨物コード枝番
        Master.EraseCharToIgnore(TxtBranch.Text)
        '発着駅フラグ
        Master.EraseCharToIgnore(TxtDepArrStation.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '運用部署
        work.WF_SEL_UORG.Text = WF_UORG.Text
        '貨物駅コード
        work.WF_SEL_STATIONCODE.Text = TxtStationCode.Text
        '貨物コード枝番
        work.WF_SEL_BRANCH.Text = TxtBranch.Text
        '発着駅フラグ
        work.WF_SEL_DEPARRSTATIONFLG.Text = TxtDepArrStation.Text

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
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
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '運用部署
        WW_TEXT = WF_UORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "UORG", WF_UORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_UORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "運用部署 : " & WF_UORG.Text)
                    WF_UORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_UORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '貨物駅コード
        Master.CheckField(WF_CAMPCODE.Text, "STATIONCODE", TxtStationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '### like検索を実施するため、存在チェックは外す(20191223) ###########################################
            ''存在チェック
            'CODENAME_get("STATIONCODE", TxtStationCode.Text & TxtBranch.Text, LblStationCode.Text, WW_RTN_SW)
            'If Not isNormal(WW_RTN_SW) Then
            '    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
            '                  "貨物駅コード : " & TxtStationCode.Text & "　" &
            '                  "貨物コード枝番 : " & TxtBranch.Text)
            '    TxtStationCode.Focus()
            '    O_RTN = "ERR"
            '    Exit Sub
            'End If
            '####################################################################################################
        Else
            'ポップアップを表示(needsPopUp:=True)
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "貨物駅コード", needsPopUp:=True)
            TxtStationCode.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '発着駅フラグ
        Master.CheckField(WF_CAMPCODE.Text, "DEPARRSTATIONFLG", TxtDepArrStation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPARRSTATIONFLG", TxtDepArrStation.Text, LblDepArrStation.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発着駅フラグ : " & TxtDepArrStation.Text)
                TxtDepArrStation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            'ポップアップを表示(needsPopUp:=True)
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発着駅フラグ", needsPopUp:=True)
            TxtDepArrStation.Focus()
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
                '会社コード
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                '運用部署
                If WF_FIELD.Value = "WF_UORG" Then
                    prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                End If

                '貨物車コード 
                If WF_FIELD.Value = "TxtStationCode" Then
                    'prmData = work.CreateSTATIONPTParam(Master.USER_ORG, TxtStationCode.Text & TxtBranch.Text)
                    prmData = work.CreateSTATIONPTParam(WF_CAMPCODE.Text, TxtStationCode.Text & TxtBranch.Text)
                End If

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
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '貨物駅コード
            Case "TxtStationCode"
                CODENAME_get("STATIONCODE", TxtStationCode.Text & TxtBranch.Text, LblStationCode.Text, WW_RTN_SW)
                '貨物コード枝番
                'Case "TxtGoodsStationCodeBranch"
                '    CODENAME_get("BRANCH", TxtBranch.Text, LblBranch.Text, WW_RTN_SW)
            '発着駅フラグ
            Case "TxtDepArrStation"
                CODENAME_get("DEPARRSTATIONFLG", TxtDepArrStation.Text, LblDepArrStation.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                '### like検索を実施するため、存在チェックは外す(20191223) ###########################################
                '貨物駅コード
                Case "TxtStationCode"
                    '何もしない(like検索をするにあたって、「マスタが存在しない」旨を未出力とするため)
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
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_UORG"              '運用部署
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()

            Case "TxtStationCode"       '貨物車コード
                If WW_SelectValue = "" Then
                    TxtStationCode.Text = ""
                    LblStationCode.Text = ""
                    TxtBranch.Text = ""
                Else
                    TxtStationCode.Text = WW_SelectValue.Substring(0, 4)
                    LblStationCode.Text = WW_SelectText
                    TxtBranch.Text = WW_SelectValue.Substring(4)
                End If
                TxtStationCode.Focus()

            Case "TxtDepArrStation"     '発着駅フラグ
                TxtDepArrStation.Text = WW_SelectValue
                LblDepArrStation.Text = WW_SelectText
                TxtDepArrStation.Focus()

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
                WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                WF_UORG.Focus()
            Case "TxtStationCode"       '貨物車コード
                TxtStationCode.Focus()
            Case "TxtDepArrStation"     '発着駅フラグ
                TxtDepArrStation.Focus()
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

        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONCODE"      '貨物駅コード
                    prmData = work.CreateSTATIONPTParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DEPARRSTATIONFLG" '発着駅フラグ
                    prmData = work.CreateSTATIONPTParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

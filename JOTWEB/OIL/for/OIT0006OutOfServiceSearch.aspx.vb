'Option Strict On
'Option Explicit On

Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 回送検索画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0006OutOfServiceSearch
    Inherits System.Web.UI.Page

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                  '検索ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '戻るボタン押下
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
        Master.MAPID = OIT0006WRKINC.MAPIDS
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()
    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", Me.WF_CAMPCODE.Text)
            '運用部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", Me.WF_UORG.Text)
            '営業所
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", Me.TxtSalesOffice.Text)
            '年月日
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATESTART", Me.TxtDateStart.Text)
            '列車番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TRAINNO", Me.TxtTrainNumber.Text)
            '状態
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STATUS", Me.TxtStatus.Text)
            '目的
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DEPARRSTATIONFLG", Me.TxtObjective.Text)
            '着駅
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", Me.TxtArrstationCode.Text)
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0006L Then   '一覧画面からの遷移
            '〇画面項目設定処理
            '会社コード
            Me.WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '運用部署
            Me.WF_UORG.Text = work.WF_SEL_UORG.Text
            '営業所
            Me.TxtSalesOffice.Text = work.WF_SEL_SALESOFFICECODEMAP.Text
            '年月日
            Me.TxtDateStart.Text = work.WF_SEL_DATE.Text
            '列車番号
            Me.TxtTrainNumber.Text = work.WF_SEL_TRAINNUMBER.Text
            '状態
            Me.TxtStatus.Text = work.WF_SEL_STATUSCODE.Text
            '目的
            Me.TxtObjective.Text = work.WF_SEL_OBJECTIVECODEMAP.Text
            '着駅
            Me.TxtArrstationCode.Text = work.WF_SEL_ARRIVALSTATIONMAP.Text

        End If

        '営業所・列車番号・目的・状態・着駅を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtSalesOffice.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTrainNumber.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtStatus.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtObjective.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtArrstationCode.Attributes("onkeyPress") = "CheckNum()"

        '年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtDateStart.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0006WRKINC.MAPIDS
        rightview.MAPID = OIT0006WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)
        '営業所
        CODENAME_get("OFFICECODE", Me.TxtSalesOffice.Text, Me.LblSalesOfficeName.Text, WW_DUMMY)
        '状態
        CODENAME_get("STATUS", Me.TxtStatus.Text, Me.LblStatusName.Text, WW_DUMMY)
        '目的
        CODENAME_get("OBJECTIVECODE", Me.TxtObjective.Text, Me.LblObjective.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(Me.WF_CAMPCODE.Text)
        '運用部署
        Master.EraseCharToIgnore(Me.WF_UORG.Text)
        '営業所
        Master.EraseCharToIgnore(Me.TxtSalesOffice.Text)
        '年月日
        Master.EraseCharToIgnore(Me.TxtDateStart.Text)
        '列車番号
        Master.EraseCharToIgnore(Me.TxtTrainNumber.Text)
        '状態
        Master.EraseCharToIgnore(Me.TxtStatus.Text)
        '目的
        Master.EraseCharToIgnore(Me.TxtObjective.Text)
        '着駅
        Master.EraseCharToIgnore(Me.TxtArrstationCode.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE.Text = Me.WF_CAMPCODE.Text
        '運用部署
        work.WF_SEL_UORG.Text = Me.WF_UORG.Text
        '営業所
        work.WF_SEL_SALESOFFICECODEMAP.Text = Me.TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICECODE.Text = Me.TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICE.Text = Me.LblSalesOfficeName.Text
        '年月日
        work.WF_SEL_DATE.Text = Me.TxtDateStart.Text
        '列車番号
        work.WF_SEL_TRAINNUMBER.Text = Me.TxtTrainNumber.Text
        '状態
        work.WF_SEL_STATUSCODE.Text = Me.TxtStatus.Text
        work.WF_SEL_STATUS.Text = Me.LblStatusName.Text
        '目的
        work.WF_SEL_OBJECTIVECODEMAP.Text = Me.TxtObjective.Text
        work.WF_SEL_OBJECTIVECODE.Text = Me.TxtObjective.Text
        work.WF_SEL_OBJECTIVENAME.Text = Me.LblObjective.Text
        '着駅
        work.WF_SEL_ARRIVALSTATIONMAP.Text = Me.TxtArrstationCode.Text
        work.WF_SEL_ARRIVALSTATION.Text = Me.TxtArrstationCode.Text
        work.WF_SEL_ARRIVALSTATIONNM.Text = Me.LblArrstationName.Text

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
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", Me.WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & Me.WF_CAMPCODE.Text)
                Me.WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '運用部署
        WW_TEXT = Me.WF_UORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "UORG", Me.WF_UORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                Me.WF_UORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "運用部署 : " & Me.WF_UORG.Text)
                    Me.WF_UORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            Me.WF_UORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '営業所
        If Me.TxtSalesOffice.Text <> "" Then
            Master.CheckField(Me.WF_CAMPCODE.Text, "OFFICECODE", Me.TxtSalesOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所", needsPopUp:=True)
                Me.TxtSalesOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '年月日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", Me.TxtDateStart.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(Me.TxtDateStart.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "年月日", needsPopUp:=True)
            Me.TxtDateStart.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '列車番号
        If Me.TxtTrainNumber.Text <> "" Then
            Master.CheckField(Me.WF_CAMPCODE.Text, "TRAINNUMBER", Me.TxtTrainNumber.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                Me.TxtTrainNumber.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '状態
        If Me.TxtStatus.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "STATUS", Me.TxtStatus.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                Me.TxtStatus.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '目的
        If Me.TxtObjective.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "OBJECTIVECODE", Me.TxtObjective.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                Me.TxtObjective.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '着駅
        If Me.TxtArrstationCode.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "ARRSTATION", Me.TxtArrstationCode.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                Me.TxtArrstationCode.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
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
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = Me.WF_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(Me.WF_CAMPCODE.Text)
                    End If

                    '営業所
                    If WF_FIELD.Value = "TxtSalesOffice" Then
                        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, Me.TxtSalesOffice.Text)
                    End If

                    '状態
                    If WF_FIELD.Value = "TxtStatus" Then
                        prmData = work.CreateSALESOFFICEParam(Me.WF_CAMPCODE.Text, Me.TxtStatus.Text)
                        '★抽出条件追加
                        Dim condition As String = ""
                        condition &= " AND KEYCODE IN ("
                        condition &= "   '" + BaseDllConst.CONST_KAISOUSTATUS_100 + "'"
                        condition &= " , '" + BaseDllConst.CONST_KAISOUSTATUS_250 + "'"
                        condition &= " , '" + BaseDllConst.CONST_KAISOUSTATUS_500 + "'"
                        condition &= " )"
                        prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = condition
                    End If

                    '目的
                    If WF_FIELD.Value = "TxtObjective" Then
                        prmData = work.CreateSALESOFFICEParam(Me.WF_CAMPCODE.Text, Me.TxtObjective.Text)
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtArrstationCode" Then
                        '〇 画面(受注営業所).テキストボックスが未設定
                        If Me.TxtSalesOffice.Text = "" Then
                            prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", Me.TxtArrstationCode.Text)
                        Else
                            prmData = work.CreateSTATIONPTParam(Me.TxtSalesOffice.Text + "2", Me.TxtArrstationCode.Text)
                        End If
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "TxtDateStart"
                            .WF_Calendar.Text = Me.TxtDateStart.Text
                    End Select
                    .ActiveCalendar()

                End If
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
                CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_RTN_SW)
            '営業所
            Case "TxtSalesOffice"
                CODENAME_get("OFFICECODE", Me.TxtSalesOffice.Text, Me.LblSalesOfficeName.Text, WW_RTN_SW)
            '状態
            Case "TxtStatus"
                CODENAME_get("STATUS", Me.TxtStatus.Text, Me.LblStatusName.Text, WW_RTN_SW)
            '目的
            Case "TxtObjective"
                CODENAME_get("OBJECTIVECODE", Me.TxtObjective.Text, Me.LblObjective.Text, WW_RTN_SW)
            '着駅
            Case "TxtArrstationCode"
                CODENAME_get("ARRSTATION", Me.TxtArrstationCode.Text, Me.LblArrstationName.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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
                Me.WF_CAMPCODE.Text = WW_SelectValue
                Me.WF_CAMPCODE_TEXT.Text = WW_SelectText
                Me.WF_CAMPCODE.Focus()

            Case "WF_UORG"              '運用部署
                Me.WF_UORG.Text = WW_SelectValue
                Me.WF_UORG_TEXT.Text = WW_SelectText
                Me.WF_UORG.Focus()

            Case "TxtSalesOffice"       '営業所

                '営業所が変更された場合
                If Me.TxtSalesOffice.Text <> WW_SelectValue Then
                    '着駅の検索条件を初期化する。
                    Me.TxtArrstationCode.Text = ""
                    Me.LblArrstationName.Text = ""

                End If

                Me.TxtSalesOffice.Text = WW_SelectValue
                Me.LblSalesOfficeName.Text = WW_SelectText
                Me.TxtSalesOffice.Focus()

            Case "TxtDateStart"         '年月日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtDateStart.Text = ""
                    Else
                        Me.TxtDateStart.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtDateStart.Focus()

            Case "TxtStatus"            '状態
                Me.TxtStatus.Text = WW_SelectValue
                Me.LblStatusName.Text = WW_SelectText
                Me.TxtStatus.Focus()

            Case "TxtObjective"         '目的
                Me.TxtObjective.Text = WW_SelectValue
                Me.LblObjective.Text = WW_SelectText
                Me.TxtObjective.Focus()

            Case "TxtArrstationCode"    '着駅
                Me.TxtArrstationCode.Text = WW_SelectValue
                Me.LblArrstationName.Text = WW_SelectText
                Me.TxtArrstationCode.Focus()

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
                Me.WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                Me.WF_UORG.Focus()
            Case "TxtSalesOffice"       '営業所
                Me.TxtSalesOffice.Focus()
            Case "TxtDateStart"         '年月日
                Me.TxtDateStart.Focus()
            Case "TxtStatus"            '状態
                Me.TxtStatus.Focus()
            Case "TxtObjective"         '目的
                Me.TxtObjective.Focus()
            Case "TxtArrstationCode"    '着駅
                Me.TxtArrstationCode.Focus()
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
                Case "OFFICECODE"       '営業所
                    prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATUS"           '状態
                    prmData = work.CreateORDERSTATUSParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUSTATUS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OBJECTIVECODE"    '目的
                    prmData = work.CreateORDERSTATUSParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ARRSTATION"       '着駅
                    '〇 画面(受注営業所).テキストボックスが未設定
                    If Me.TxtSalesOffice.Text = "" Then
                        prmData = work.CreateSTATIONPTParam(Master.USER_ORG + "2", Me.TxtArrstationCode.Text)
                    Else
                        prmData = work.CreateSTATIONPTParam(Me.TxtSalesOffice.Text + "2", Me.TxtArrstationCode.Text)
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class
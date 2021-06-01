''************************************************************
' タンク車マスタメンテ検索画面
' 作成日 2019/11/08
' 更新日 2021/05/25
' 作成者 JOT遠藤
' 更新者 JOT伊草
'
' 修正履歴:2019/11/08 新規作成
'         :2021/05/25 1)検索項目に「運用基地コード」「リース先」を追加
''************************************************************
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class OIM0005TankSearch
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
                    Case "WF_ButtonKINOENE"             '甲子貨車マスタメンテボタン押下
                        WF_ButtonKINOENE_Click()
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

        '○ 画面ID設定
        Master.MAPID = OIM0005WRKINC.MAPIDS

        WF_TANKNUMBER_CODE.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then         'メニューからの画面遷移
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)               '会社コード
            Master.GetFirstValue(work.WF_SEL_ORG.Text, "ORG", WF_ORG.Text)                              '組織コード
            Master.GetFirstValue(work.WF_SEL_TANKNUMBER.Text, "TANKNUMBER", WF_TANKNUMBER_CODE.Text)    'JOT車番
            Master.GetFirstValue(work.WF_SEL_MODEL.Text, "MODEL", WF_MODEL_CODE.Text)                   '型式
            Master.GetFirstValue(work.WF_SEL_USEDFLG.Text, "USEPROPRIETY", WF_USEDFLG_CODE.Text)        '利用フラグ
            '運用基地コード
            Master.GetFirstValue(work.WF_SEL_OPERATIONBASECODE_S.Text, "OPERATIONBASECODE", WF_OPERATIONBASECODE.Text)
            'リース先コード
            WF_LEACECODE_NONE.Checked = True
            WF_LEACECODE_OT.Checked = False
            WF_LEACECODE_USARMY.Checked = False

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005L Then   '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text                      '組織コード
            WF_TANKNUMBER_CODE.Text = work.WF_SEL_TANKNUMBER.Text   'JOT車番
            WF_MODEL_CODE.Text = work.WF_SEL_MODEL.Text             '型式
            WF_USEDFLG_CODE.Text = work.WF_SEL_USEDFLG.Text         '利用フラグ
            '運用基地コード
            WF_OPERATIONBASECODE.Text = work.WF_SEL_OPERATIONBASECODE_S.Text
            'リース先コード
            If String.IsNullOrEmpty(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_OT.Checked = False
                WF_LEACECODE_USARMY.Checked = False
                WF_LEACECODE_NONE.Checked = True
            ElseIf "11".Equals(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_USARMY.Checked = False
                WF_LEACECODE_NONE.Checked = False
                WF_LEACECODE_OT.Checked = True
            ElseIf "71".Equals(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_NONE.Checked = False
                WF_LEACECODE_OT.Checked = False
                WF_LEACECODE_USARMY.Checked = True
            End If
        Else
            '画面項目設定処理（甲子貨車マスタメンテ画面からの遷移）
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text                      '組織コード
            WF_TANKNUMBER_CODE.Text = work.WF_SEL_TANKNUMBER.Text   'JOT車番
            WF_MODEL_CODE.Text = work.WF_SEL_MODEL.Text             '型式
            WF_USEDFLG_CODE.Text = work.WF_SEL_USEDFLG.Text         '利用フラグ
            '運用基地コード
            WF_OPERATIONBASECODE.Text = work.WF_SEL_OPERATIONBASECODE_S.Text
            'リース先コード
            If String.IsNullOrEmpty(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_OT.Checked = False
                WF_LEACECODE_USARMY.Checked = False
                WF_LEACECODE_NONE.Checked = True
            ElseIf "11".Equals(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_USARMY.Checked = False
                WF_LEACECODE_NONE.Checked = False
                WF_LEACECODE_OT.Checked = True
            ElseIf "71".Equals(work.WF_SEL_LEASECODE_S.Text) Then
                WF_LEACECODE_NONE.Checked = False
                WF_LEACECODE_OT.Checked = False
                WF_LEACECODE_USARMY.Checked = True
            End If
        End If

        'JOT車番・利用フラグを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_TANKNUMBER_CODE.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_USEDFLG_CODE.Attributes("onkeyPress") = "CheckNum()"

        '甲子貨車マスタメンテボタンは、関東支店、甲子営業所、石油部、情シスのみ表示
        If Master.USER_ORG = "011401" OrElse
           Master.USER_ORG = "011202" OrElse
           Master.USER_ORG = "010007" OrElse
           Master.USER_ORG = "010006" Then
            WF_ButtonKINOENE.Visible = True
        Else
            WF_ButtonKINOENE.Visible = False
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0005WRKINC.MAPIDS
        rightview.MAPID = OIM0005WRKINC.MAPIDL
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
        'CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)            '会社コード
        'CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)                           '組織コード
        CODENAME_get("TANKNUMBER", WF_TANKNUMBER_CODE.Text, WF_TANKNUMBER_NAME.Text, WW_DUMMY)  'JOT車番
        'CODENAME_get("MODEL", WF_MODEL_CODE.Text, WF_MODEL_NAME.Text, WW_DUMMY)                '型式
        CODENAME_get("USEDFLG", WF_USEDFLG_CODE.Text, WF_USEDFLG_NAME.Text, WW_DUMMY)           '利用フラグ
        CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASENAME.Text, WW_DUMMY)    '運用基地コード

    End Sub

    ''' <summary>
    ''' 甲子貨車マスタメンテボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonKINOENE_Click()

        ''○ 画面レイアウト設定（特殊処理のため、会社コード＋"1"(011）でVIEWを登録）
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移（特殊処理のため、会社コード＋"1"(011）でVIEWを登録）
            Master.TransitionPage(WF_CAMPCODE.Text & "1")
        End If

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_TANKNUMBER_CODE.Text)       'JOT車番
        Master.EraseCharToIgnore(WF_MODEL_CODE.Text)            '型式
        Master.EraseCharToIgnore(WF_USEDFLG_CODE.Text)          '利用フラグ
        Master.EraseCharToIgnore(WF_OPERATIONBASECODE.Text)     '運用基地コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text                        '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text                                  '組織コード
        work.WF_SEL_TANKNUMBER.Text = WF_TANKNUMBER_CODE.Text               'JOT車番
        work.WF_SEL_MODEL.Text = WF_MODEL_CODE.Text                         '型式
        work.WF_SEL_USEDFLG.Text = WF_USEDFLG_CODE.Text                     '利用フラグ
        work.WF_SEL_OPERATIONBASECODE_S.Text = WF_OPERATIONBASECODE.Text    '運用基地コード
        'リース先コード
        If WF_LEACECODE_NONE.Checked Then
            work.WF_SEL_LEASECODE_S.Text = ""
        ElseIf WF_LEACECODE_OT.Checked Then
            work.WF_SEL_LEASECODE_S.Text = "11"
        ElseIf WF_LEACECODE_USARMY.Checked Then
            work.WF_SEL_LEASECODE_S.Text = "71"
        End If

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
        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_LINE_ERR As String = ""

        '○ 単項目チェック
        'JOT車番
        Master.CheckField(WF_CAMPCODE.Text, "TANKNUMBER", WF_TANKNUMBER_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WF_TANKNUMBER_CODE.Text <> "" Then
                CODENAME_get("TANKNUMBER", WF_TANKNUMBER_CODE.Text, WF_TANKNUMBER_NAME.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "JOT車番 : " & WF_TANKNUMBER_CODE.Text, needsPopUp:=True)
                    WF_TANKNUMBER_CODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "会社コード", needsPopUp:=True)
            WF_TANKNUMBER_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '型式
        Master.CheckField(WF_CAMPCODE.Text, "MODEL", WF_MODEL_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_MODEL_CODE.Text <> "" Then
                '存在チェック
                CODENAME_get("MODEL", WF_MODEL_CODE.Text, WF_MODEL_NAME.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "型式 : " & WF_MODEL_CODE.Text, needsPopUp:=True)
                    WF_MODEL_CODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "型式", needsPopUp:=True)
            WF_MODEL_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '利用フラグ
        Master.CheckField(WF_CAMPCODE.Text, "USEDFLG", WF_USEDFLG_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_USEDFLG_CODE.Text <> "" Then
                '存在チェック
                CODENAME_get("USEDFLG", WF_USEDFLG_CODE.Text, WF_USEDFLG_NAME.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "利用フラグ : " & WF_USEDFLG_CODE.Text, needsPopUp:=True)
                    WF_USEDFLG_CODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "利用フラグ", needsPopUp:=True)
            WF_USEDFLG_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
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
                '会社コード
                Dim prmData As New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                'JOT車番
                If WF_FIELD.Value = "WF_TANKNUMBER" Then
                    prmData = work.CreateTankParam(WF_CAMPCODE.Text, "TANKNUMBER")
                End If

                '型式
                If WF_FIELD.Value = "WF_MODEL" Then
                    prmData = work.CreateTankParam(WF_CAMPCODE.Text, "TANKMODEL")
                End If

                '運用基地コード
                If WF_FIELD.Value = "WF_OPERATIONBASECODE" Then
                    prmData = work.CreateBaseParam(WF_CAMPCODE.Text, WF_OPERATIONBASECODE.Text)
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
            Case "WF_TANKNUMBER"        'JOT車番
                CODENAME_get("TANKNUMBER", WF_TANKNUMBER_CODE.Text, WF_TANKNUMBER_NAME.Text, WW_RTN_SW)
                'Case "WF_MODEL"        '型式
                '    CODENAME_get("TANKMODEL", WF_MODEL_CODE.Text, WF_MODEL_NAME.Text, WW_RTN_SW)
            Case "WF_USEDFLG"           '利用フラグ
                CODENAME_get("USEDFLG", WF_USEDFLG_CODE.Text, WF_USEDFLG_NAME.Text, WW_RTN_SW)
            Case "WF_OPERATIONBASECODE" '運用基地コード
                CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASENAME.Text, WW_RTN_SW)
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
            Case "WF_TANKNUMBER"          'JOT車番
                WF_TANKNUMBER_CODE.Text = WW_SelectValue
                WF_TANKNUMBER_NAME.Text = WW_SelectText
                WF_TANKNUMBER_CODE.Focus()

            Case "WF_MODEL"             '型式
                WF_MODEL_CODE.Text = WW_SelectValue
                'WF_MODEL_NAME.Text = WW_SelectText
                WF_MODEL_CODE.Focus()

            Case "WF_USEDFLG"           '利用フラグ
                WF_USEDFLG_CODE.Text = WW_SelectValue
                WF_USEDFLG_NAME.Text = WW_SelectText
                WF_USEDFLG_CODE.Focus()

            Case "WF_OPERATIONBASECODE" '運用基地コード
                WF_OPERATIONBASECODE.Text = WW_SelectValue
                WF_OPERATIONBASENAME.Text = WW_SelectText
                WF_OPERATIONBASECODE.Focus()
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
            Case "WF_TANKNUMBER"        'JOT車番
                WF_TANKNUMBER_CODE.Focus()
            Case "WF_MODEL"             '型式
                WF_MODEL_CODE.Focus()
            Case "WF_USEDFLG"           '利用フラグ
                WF_USEDFLG_CODE.Focus()
            Case "WF_OPERATIONBASECODE" '運用基地C
                WF_OPERATIONBASECODE.Focus()

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
                Case "ORG"              '運用部署
                    prmData = work.CreateORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TANKNUMBER"       'JOT車番
                    prmData = work.CreateTankParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MODEL"            '型式
                    prmData = work.CreateTankParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKMODEL, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USEDFLG"          '利用フラグ
                    prmData = work.CreateTankParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BASE"             '運用基地
                    prmData = work.CreateBaseParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BASE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

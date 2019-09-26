Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 従業員マスタ登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMB0001SELECT
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
                    Case "WF_ButtonDO"                  '実行ボタン押下
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

        '○ 画面ID設定
        Master.MAPID = GRMB0001WRKINC.MAPIDS

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then             'メニューからの画面遷移
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)           '会社コード
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)                 '有効年月日(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)               '有効年月日(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MORG", WF_MORG.Text)                   '管理部署
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "HORG", WF_HORG.Text)                   '配属部署
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text)           '職務区分
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text)         '従業員
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MB0001 Then       '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_STYMD.Text = work.WF_SEL_STYMD.Text                  '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text                '有効年月日(To)
            WF_MORG.Text = work.WF_SEL_MORG.Text                    '管理部署
            WF_HORG.Text = work.WF_SEL_HORG.Text                    '配属部署
            WF_STAFFKBN.Text = work.WF_SEL_STAFFKBN.Text            '職務区分
            WF_STAFFCODE.Text = work.WF_SEL_STAFFCODE.Text          '従業員
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = GRMB0001WRKINC.MAPIDS
        rightview.MAPID = GRMB0001WRKINC.MAPID
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)             '会社コード
        CODENAME_get("MORG", WF_MORG.Text, WF_MORG_TEXT.Text, WW_DUMMY)                         '管理部署
        CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)                         '配属部署
        CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_DUMMY)             '職務区分
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)          '従業員

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_MORG.Text)              '管理部署
        Master.eraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.eraseCharToIgnore(WF_STAFFKBN.Text)          '職務区分
        Master.eraseCharToIgnore(WF_STAFFCODE.Text)         '従業員

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text            '会社コード
        work.WF_SEL_STYMD.Text = WF_STYMD.Text                  '有効年月日(From)
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text             '有効年月日(From) → 有効年月日(To)
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text            '有効年月日(To)
        End If
        work.WF_SEL_MORG.Text = WF_MORG.Text                    '管理部署
        work.WF_SEL_HORG.Text = WF_HORG.Text                    '配属部署
        work.WF_SEL_STAFFKBN.Text = WF_STAFFKBN.Text            '職務区分
        work.WF_SEL_STAFFCODE.Text = WF_STAFFCODE.Text          '従業員

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.transitionPage()
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
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '有効年月日(From)
        Master.checkFIeld(WF_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(From) : " & WF_STYMD.Text)
            WF_STYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '有効年月日(To)
        Master.checkFIeld(WF_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(To) : " & WF_ENDYMD.Text)
            WF_ENDYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '日付大小チェック
        If WF_STYMD.Text <> "" AndAlso WF_ENDYMD.Text <> "" Then
            Dim WW_DATE_ST As Date
            Dim WW_DATE_END As Date
            Try
                Date.TryParse(WF_STYMD.Text, WW_DATE_ST)
                Date.TryParse(WF_ENDYMD.Text, WW_DATE_END)

                If WW_DATE_ST > WW_DATE_END Then
                    Master.output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                    WF_STYMD.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, WF_STYMD.Text & ":" & WF_ENDYMD.Text)
                WF_STYMD.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '管理部署
        WW_TEXT = WF_MORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "MORG", WF_MORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_MORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("MORG", WF_MORG.Text, WF_MORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "管理部署 : " & WF_MORG.Text)
                    WF_MORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_MORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '配属部署
        WW_TEXT = WF_HORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "HORG", WF_HORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_HORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "配属部署 : " & WF_HORG.Text)
                    WF_HORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_HORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '職務区分
        WW_TEXT = WF_STAFFKBN.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFKBN.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "職務区分 : " & WF_STAFFKBN.Text)
                    WF_STAFFKBN.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFKBN.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '従業員
        WW_TEXT = WF_STAFFCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "従業員 : " & WF_STAFFCODE.Text)
                    WF_STAFFCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.transitionPrevPage()

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
                        '従業員
                        WF_LeftboxOpen.Value = "STAFFTABLEOpen"
                        Dim prmData = work.CreateStaffCodeParam(WF_CAMPCODE.Text)
                        .seTTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeTable()

                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .activeCalendar()

                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_MORG"          '管理部署
                                prmData = work.CreateMORGParam(WF_CAMPCODE.Text)
                            Case "WF_HORG"          '配属部署
                                prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
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
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            Case "WF_MORG"              '管理部署
                CODENAME_get("MORG", WF_MORG.Text, WF_MORG_TEXT.Text, WW_RTN_SW)
            Case "WF_HORG"              '配属部署
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFKBN"          '職務区分
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFCODE"         '従業員
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValue = leftview.getActiveValue(0)
            WW_SelectText = leftview.getActiveValue(1)
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_STYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
                WF_ENDYMD.Focus()

            Case "WF_MORG"              '管理部署
                WF_MORG.Text = WW_SelectValue
                WF_MORG_TEXT.Text = WW_SelectText
                WF_MORG.Focus()

            Case "WF_HORG"              '配属部署
                WF_HORG.Text = WW_SelectValue
                WF_HORG_TEXT.Text = WW_SelectText
                WF_HORG.Focus()

            Case "WF_STAFFKBN"          '職務区分
                WF_STAFFKBN.Text = WW_SelectValue
                WF_STAFFKBN_TEXT.Text = WW_SelectText
                WF_STAFFKBN.Focus()

            Case "WF_STAFFCODE"         '従業員
                WF_STAFFCODE.Text = Mid(WW_SelectValue, InStr(WW_SelectValue, "=") + 1, Len(WW_SelectValue))
                WF_STAFFCODE_TEXT.Text = Mid(WW_SelectText, InStr(WW_SelectText, "=") + 1, Len(WW_SelectText))
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_MORG"              '管理部署
                WF_MORG.Focus()
            Case "WF_HORG"              '配属部署
                WF_HORG.Focus()
            Case "WF_STAFFKBN"          '職務区分
                WF_STAFFKBN.Focus()
            Case "WF_STAFFCODE"         '従業員
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.showHelp()

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
                Case "MORG"             '管理部署
                    prmData = work.CreateMORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"             '配属部署
                    prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"        '従業員
                    prmData = work.CreateStaffCodeParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

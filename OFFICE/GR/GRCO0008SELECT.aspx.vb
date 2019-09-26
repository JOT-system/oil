Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' メニュー項目メンテナンス画面（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRCO0008SELECT
    Inherits Page

    '共通処理結果
    Private WW_ERRCODE As String
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '○各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"
                        WF_ButtonEND_Click()
                    Case "WF_ButtonSel"
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"         '○フィールドダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_LeftBoxSelectClick"
                        WF_FIELD_Change()
                    Case "WF_ListboxDBclick"        '○ListBoxダブルクリック処理
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"
                        WF_LEFTBOX_SELECT_CLICK()
                    Case "WF_RIGHT_VIEW_DBClick"
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"            '○右Boxメモ変更時処理
                        WF_RIGHTBOX_Change()
                    Case Else
                End Select
            End If
        Else
            '○初期化処理
            Initialize()
        End If
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○初期値設定
        WF_STYMD.Focus()
        WF_FIELD.Value = ""

        '○画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○画面戻先URL取得
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_MAPIDPF.Text)           '親画面ID(From)
        Master.eraseCharToIgnore(WF_MAPIDPT.Text)           '親画面ID(To)
        Master.eraseCharToIgnore(WF_MAPIDF.Text)            '子画面ID(From)
        Master.eraseCharToIgnore(WF_MAPIDT.Text)            '子画面ID(To)

        '○ チェック処理
        INP_Check(WW_ERRCODE)
        If isNormal(WW_ERRCODE) Then
            Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_ERRCODE, C_MESSAGE_TYPE.ERR)
            Exit Sub
        End If

        '○条件選択画面の入力値退避(選択情報のWF_SEL退避) 
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text

        work.WF_SEL_STYMD.Text = WF_STYMD.Text
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If

        work.WF_SEL_MAPIDPF.Text = WF_MAPIDPF.Text
        If WF_MAPIDPT.Text = "" Then
            work.WF_SEL_MAPIDPT.Text = WF_MAPIDPF.Text
        Else
            work.WF_SEL_MAPIDPT.Text = WF_MAPIDPT.Text
        End If

        work.WF_SEL_MAPIDF.Text = WF_MAPIDF.Text
        If WF_MAPIDT.Text = "" Then
            work.WF_SEL_MAPIDT.Text = WF_MAPIDF.Text
        Else
            work.WF_SEL_MAPIDT.Text = WF_MAPIDT.Text
        End If

        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.transitionPage()
        End If

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '○フィールドダブルクリック時処理
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text)
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text

                    End Select
                    .activeCalendar()
                End If
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールド変更時処理
    ''' </summary>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
            Case "WF_MAPIDPF"           '親画面ID(From)
                CODENAME_get("MAPID", WF_MAPIDPF.Text, WF_MAPIDPF_Text.Text, WW_RTN_SW)
            Case "WF_MAPIDPT"           '親画面ID(To)
                CODENAME_get("MAPID", WF_MAPIDPT.Text, WF_MAPIDPT_Text.Text, WW_RTN_SW)
            Case "WF_MAPIDF"            '子画面ID(From)
                CODENAME_get("MAPID", WF_MAPIDF.Text, WF_MAPIDF_Text.Text, WW_RTN_SW)
            Case "WF_MAPIDT"            '子画面ID(To)
                CODENAME_get("MAPID", WF_MAPIDT.Text, WF_MAPIDT_Text.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '○ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub

    ''' <summary>
    ''' ○TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_CLICK()
        WW_LeftBoxReSet()
    End Sub

    ''' <summary>
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '○右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectTEXT As String = ""
        Dim WW_SelectValue As String = ""

        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectTEXT = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectTEXT
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE.Focus()

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()

            Case "WF_MAPIDPF"           '親画面ID(From)
                WF_MAPIDPF_Text.Text = WW_SelectTEXT
                WF_MAPIDPF.Text = WW_SelectValue
                WF_MAPIDPF.Focus()
            Case "WF_MAPIDPT"           '親画面ID(To)
                WF_MAPIDPT_Text.Text = WW_SelectTEXT
                WF_MAPIDPT.Text = WW_SelectValue
                WF_MAPIDPT.Focus()

            Case "WF_MAPIDF"            '子画面ID(From)
                WF_MAPIDF_Text.Text = WW_SelectTEXT
                WF_MAPIDF.Text = WW_SelectValue
                WF_MAPIDF.Focus()
            Case "WF_MAPIDT"            '子画面ID(To)
                WF_MAPIDT_Text.Text = WW_SelectTEXT
                WF_MAPIDT.Text = WW_SelectValue
                WF_MAPIDT.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_MAPIDPF"           '親画面ID(From)
                WF_MAPIDPF.Focus()
            Case "WF_MAPIDPT"           '親画面ID(To)
                WF_MAPIDPT.Focus()
            Case "WF_MAPIDF"            '子画面ID(From)
                WF_MAPIDF.Focus()
            Case "WF_MAPIDT"            '子画面ID(To)
                WF_MAPIDT.Focus()
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_MAPIDPF.Text)           '親画面ID(From)
        Master.eraseCharToIgnore(WF_MAPIDPT.Text)           '親画面ID(To)
        Master.eraseCharToIgnore(WF_MAPIDF.Text)            '子画面ID(From)
        Master.eraseCharToIgnore(WF_MAPIDT.Text)            '子画面ID(To)

        '○チェック処理
        INP_Check(WW_DUMMY)

    End Sub
    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then                       'メニューからの画面遷移
            '○ワーク初期化
            work.Initialize()
            '選択情報のWF_SELクリア
            work.WF_SEL_STYMD.Text = ""         '有効年月日(From)
            work.WF_SEL_ENDYMD.Text = ""        '有効年月日(To)
            work.WF_SEL_MAPIDPF.Text = ""       '親画面ID(From)
            work.WF_SEL_MAPIDPT.Text = ""       '親画面ID(To)
            work.WF_SEL_MAPIDF.Text = ""        '子画面ID(From)
            work.WF_SEL_MAPIDT.Text = ""        '子画面ID(To)

            '○初期変数設定処理
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)       '会社コード
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)             '有効年月日(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)           '有効年月日(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAPIDPF", WF_MAPIDPF.Text)         '親画面ID(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAPIDPT", WF_MAPIDPT.Text)         '親画面ID(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAPIDF", WF_MAPIDF.Text)           '子画面ID(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MAPIDT", WF_MAPIDT.Text)           '子画面ID(To)

            '○RightBox情報設定
            rightview.MAPID = GRCO0008WRKINC.MAPID
            rightview.MAPIDS = GRCO0008WRKINC.MAPIDS
            rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.CO0008 Then                 '実行画面からの画面遷移

            '○画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
            WF_STYMD.Text = work.WF_SEL_STYMD.Text              '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text            '有効年月日(To)
            WF_MAPIDPF.Text = work.WF_SEL_MAPIDPF.Text          '親画面ID(From)
            WF_MAPIDPT.Text = work.WF_SEL_MAPIDPT.Text          '親画面ID(To)
            WF_MAPIDF.Text = work.WF_SEL_MAPIDF.Text            '子画面ID(From)
            WF_MAPIDT.Text = work.WF_SEL_MAPIDT.Text            '子画面ID(To)

            '○RightBox情報設定
            rightview.MAPID = GRCO0008WRKINC.MAPID
            rightview.MAPIDS = GRCO0008WRKINC.MAPIDS
            rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

        End If

        '○名称設定
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)             '会社コード
        CODENAME_get("MAPID", WF_MAPIDPF.Text, WF_MAPIDPF_Text.Text, WW_DUMMY)                  '親画面ID(From)
        CODENAME_get("MAPID", WF_MAPIDPT.Text, WF_MAPIDPT_Text.Text, WW_DUMMY)                  '親画面ID(To)
        CODENAME_get("MAPID", WF_MAPIDF.Text, WF_MAPIDF_Text.Text, WW_DUMMY)                    '子画面ID(From)
        CODENAME_get("MAPID", WF_MAPIDT.Text, WF_MAPIDT_Text.Text, WW_DUMMY)                    '子画面ID(To)

    End Sub

    ''' <summary>
    ''' 入力項目チェック処理
    ''' </summary>
    ''' <param name="O_RTNCODE">成否判定</param>
    ''' <remarks></remarks>
    Protected Sub INP_Check(ByRef O_RTNCODE As String)

        O_RTNCODE = C_MESSAGE_NO.NORMAL

        '○ 入力項目チェック
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_TEXT As String = ""

        '○会社コードチェック
        'WF_MAPIDPF.Text
        WW_TEXT = WF_CAMPCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_CAMPCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    WF_CAMPCODE.Focus()
                    Exit Sub
                End If
            End If
        Else
            O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            WF_CAMPCODE.Focus()
            Exit Sub
        End If

        '○日付チェック
        'WF_STYMD.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            O_RTNCODE = WW_CS0024FCHECKERR
            WF_STYMD.Focus()
            Exit Sub
        End If

        'WF_ENDYMD.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            O_RTNCODE = WW_CS0024FCHECKERR
            WF_ENDYMD.Focus()
            Exit Sub
        End If

        '○親画面IDチェック
        'WF_MAPIDPF.Text
        WW_TEXT = WF_MAPIDPF.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "MAPIDPF", WF_MAPIDPF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_MAPIDPF.Text = ""
            Else
                '存在チェック
                CODENAME_get("MAPID", WF_MAPIDPF.Text, WF_MAPIDPF_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    WF_MAPIDPF.Focus()
                    Exit Sub
                End If
            End If
        Else
            O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            WF_MAPIDPF.Focus()
            Exit Sub
        End If

        'WF_MAPIDPT.Text
        WW_TEXT = WF_MAPIDPT.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "MAPIDPT", WF_MAPIDPT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_MAPIDPT.Text = ""
            Else
                '存在チェック
                CODENAME_get("MAPID", WF_MAPIDPT.Text, WF_MAPIDPT_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    WF_MAPIDPT.Focus()
                    Exit Sub
                End If
            End If
        Else
            O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            WF_MAPIDPT.Focus()
            Exit Sub
        End If

        '○子画面IDチェック
        'WF_MAPIDF.Text
        WW_TEXT = WF_MAPIDF.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "MAPIDF", WF_MAPIDF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_MAPIDF.Text = ""
            Else
                '存在チェック
                CODENAME_get("MAPID", WF_MAPIDF.Text, WF_MAPIDF_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    WF_MAPIDF.Focus()
                    Exit Sub
                End If
            End If
        Else
            O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            WF_MAPIDF.Focus()
            Exit Sub
        End If

        'WF_MAPIDT.Text
        WW_TEXT = WF_MAPIDT.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "MAPIDT", WF_MAPIDT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_MAPIDT.Text = ""
            Else
                '存在チェック
                CODENAME_get("MAPID", WF_MAPIDT.Text, WF_MAPIDT_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    WF_MAPIDT.Focus()
                    Exit Sub
                End If
            End If
        Else
            O_RTNCODE = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            WF_MAPIDT.Focus()
            Exit Sub
        End If

        '○関連チェック
        '日付関連チェック
        If WF_STYMD.Text <> "" AndAlso WF_ENDYMD.Text <> "" Then
            Try
                Dim WW_Date_From As Date
                Date.TryParse(WF_STYMD.Text, WW_Date_From)
                Dim WW_Date_To As Date
                Date.TryParse(WF_ENDYMD.Text, WW_Date_To)

                If WW_Date_From > WW_Date_To Then
                    O_RTNCODE = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                    WF_STYMD.Focus()
                    Exit Sub
                End If
            Catch ex As Exception
                O_RTNCODE = C_MESSAGE_NO.DATE_FORMAT_ERROR
                WF_STYMD.Focus()
                Exit Sub
            End Try
        End If

        '親画面関連チェック
        If WF_MAPIDPF.Text <> "" AndAlso WF_MAPIDPT.Text <> "" Then
            If WF_MAPIDPF.Text > WF_MAPIDPT.Text Then
                O_RTNCODE = C_MESSAGE_NO.START_END_RELATION_ERROR
                WF_MAPIDPF.Focus()
                Exit Sub
            End If
        End If

        '子画面関連チェック
        If WF_MAPIDF.Text <> "" AndAlso WF_MAPIDT.Text <> "" Then
            If WF_MAPIDF.Text > WF_MAPIDT.Text Then
                O_RTNCODE = C_MESSAGE_NO.START_END_RELATION_ERROR
                WF_MAPIDF.Focus()
                Exit Sub
            End If
        End If

    End Sub

    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************

    ''' <summary>
    ''' 名称取得＆チェック
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE <> "" Then
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "MAPID"            '画面ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_URL, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(WF_CAMPCODE.Text))
                Case Else
                    O_TEXT = ""
            End Select
        End If

    End Sub

End Class

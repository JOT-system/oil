Imports OFFICE.GRIS0005LeftBox


Public Class GRT00011SELECT
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION
    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERR_SW As String                                     '
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String                                     '
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String                                      '


    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                              '■実行ボタン押下時処理
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                             '■終了ボタン押下時処理
                        WF_ButtonEND_Click()
                    Case "WF_ButtonSel"                             '■左ボックス選択ボタン押下時処理
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                             '■左ボックスキャンセルボタン押下時処理
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"                         '■入力領域ダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_TextChange"                            '■入力領域変更時処理
                        WW_LeftBoxReSet()
                    Case "WF_ListboxDBclick"                        '■左ボックスダブルクリック時処理
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"                    '■左ボックス選択処理
                        WF_LEFTBOX_SELECT_Click()
                    Case "WF_RIGHT_VIEW_DBClick"                    '■右ボックス表示時処理
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                            '■右ボックスメモ欄変更時処理
                        WF_RIGHTBOX_Change()
                End Select
            End If
        Else
            '初期化処理
            Initialize()
        End If

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

        '○初期値設定
        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '■■■ 選択画面の入力初期値設定 ■■■　…　画面固有処理
        WW_MAPValueSet(WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then Exit Sub
    End Sub
    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '■■■ チェック処理 ■■■
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■■■ セッション変数　反映 ■■■

        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '出庫日　
        work.WF_SEL_STYMD.Text = WF_STYMD.Text
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If
        '運用部署
        work.WF_SEL_UORG.Text = WF_UORG.Text
        '従業員コード
        work.WF_SEL_STAFFCODE.Text = WF_STAFFCODE.Text
        '従業員名
        work.WF_SEL_STAFFNAME.Text = WF_STAFFNAME.Text

        work.WF_SEL_VIEWID.Text = rightview.getViewId(WF_CAMPCODE.Text)
        '■■■ 画面遷移先URL取得 ■■■
        Master.VIEWID = work.WF_SEL_VIEWID.Text
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        work.WF_SEL_MAPvariant.Text = Master.MAPvariant
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.transitionPage()
        End If

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇フィールドダブルクリック時処理
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_STYMD.Text

                    End Select
                    .activeCalendar()
                ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_STAFFCODE Then
                    '従業員の場合テーブル表記する
                    Dim prmData As Hashtable = work.createSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text)
                    .seTTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeTable()
                    WF_LeftboxOpen.Value = "OpenTbl"
                Else
                    Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

                    If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_ORG Then
                        prmData = work.createORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                    ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_STAFFCODE Then
                        prmData = work.createSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text)
                    End If
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '〇ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' '〇TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_Click()
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
        '〇右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()


        Dim values As String() = leftview.getActiveValue

        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"
                '会社コード　 
                WF_CAMPCODE_Text.Text = values(1)
                WF_CAMPCODE.Text = values(0)
                WF_CAMPCODE.Focus()

            Case "WF_STYMD"
                '出庫日(FROM)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = values(0)
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"
                '出庫日(TO)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = values(0)
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()

            Case "WF_UORG"
                '運用部署
                WF_UORG_Text.Text = values(1)
                WF_UORG.Text = values(0)
                WF_UORG.Focus()

            Case "WF_STAFFCODE"
                '従業員 
                For Each Data As String In values
                    Select Case Data.Split("=")(0)
                        Case "CODE"
                            WF_STAFFCODE.Text = Data.Split("=")(1)
                        Case "NAMES"
                            WF_STAFFCODE_Text.Text = Data.Split("=")(1)
                    End Select
                Next
                WF_STAFFCODE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"
                '会社コード　 
                WF_CAMPCODE.Focus()

            Case "WF_UORG"
                '受注部署 
                WF_UORG.Focus()

            Case "WF_STAFFCODE"
                '出荷部署　　 
                WF_STAFFCODE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""
        WF_UORG_Text.Text = ""
        WF_STAFFCODE_Text.Text = ""

        '■■■ チェック処理 ■■■
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■名称設定
        SetNameValue()

    End Sub
    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        '■■■ 選択画面の入力初期値設定 ■■■
        If IsNothing(Master.MAPID) Then
            Master.MAPID = GRT00011WRKINC.MAPIDS
        End If
        'メニューからの画面遷移
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then
            work.initialize()
            '権限、変数
            work.WF_SEL_MAPvariant.Text = Master.MAPvariant
            work.WF_SEL_MAPpermitcode.Text = Master.MAPpermitcode
            work.WF_SEL_PERMIT_ORG.Text = Master.USER_ORG
            '○画面項目設定（変数より）処理
            SetInitialValue()
            '実行画面からの画面遷移
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00011 Then                                                   '実行画面からの画面遷移
            '■■■ 実行画面からの画面遷移 ■■■
            '○画面項目設定処理
            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '出庫日　
            WF_STYMD.Text = work.WF_SEL_STYMD.Text
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text
            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            '従業員コード
            WF_STAFFCODE.Text = work.WF_SEL_STAFFCODE.Text
            '従業員名称
            WF_STAFFNAME.Text = work.WF_SEL_STAFFNAME.Text

        End If

        '○RightBox情報設定
        rightview.MAPID = GRT00011WRKINC.MAPID
        rightview.MAPIDS = GRT00011WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '■名称設定
        SetNameValue()

    End Sub

    ''' <summary>
    ''' 変数設定用処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()

        '■■■ 変数設定処理 ■■■
        Master.XMLsaveF = work.WF_SEL_MAPvariant.Text
        '年度(From)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)
        '年度(To)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)
        '会社コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
        '運用部署
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", WF_UORG.Text)
        '従業員コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text)
        '従業員名称
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFNAME", WF_STAFFNAME.Text)

    End Sub

    ''' <summary>
    ''' 名称設定処理      
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetNameValue()

        '■名称設定
        '会社コード　
        CodeToName("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)
        '出荷部署
        CodeToName("UORG", WF_UORG.Text, WF_UORG_Text.Text, WW_DUMMY)
        '乗務員
        CodeToName("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_Text.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">コード値</param>
    ''' <param name="O_TEXT">名称</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "CAMPCODE" '会社コード
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "UORG" '部署
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))                     '部署
                Case "STAFFCODE" '乗務員
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.createSTAFFParam(WF_CAMPCODE.Text, String.Empty))               '従業員
            End Select
        End If

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckParameters(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        '■■■ 入力文字置き換え ■■■
        '   画面PassWord内の使用禁止文字排除
        '会社コード WF_CAMPCODE.Text
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        '出庫日(FROM) WF_STYMD.Text
        Master.EraseCharToIgnore(WF_STYMD.Text)
        '出庫日(TO) WF_ENDYMD.Text
        Master.EraseCharToIgnore(WF_ENDYMD.Text)
        '運用部署 WF_UORG.Text
        Master.EraseCharToIgnore(WF_UORG.Text)
        '従業員コード WF_STAFFCODE.Text
        Master.EraseCharToIgnore(WF_STAFFCODE.Text)
        '従業員名 WF_STAFFNAME.Text
        Master.EraseCharToIgnore(WF_STAFFNAME.Text)
        '■■■ 入力項目チェック ■■■
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CHECK As String = ""
        WF_FIELD.Value = ""

        '●会社コード WF_CAMPCODE.Text
        '①単項目チェック
        WW_CHECK = WF_CAMPCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_CAMPCODE.Text) Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, O_RTN)
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_CAMPCODE.Focus()
            Exit Sub
        End If

        '●出庫日(FROM) WF_STYMD.Text
        '①単項目チェック
        Dim WW_STYMD As Date
        WW_CHECK = WF_STYMD.Text
        Master.CheckField(WF_CAMPCODE.Text, "STYMD", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WW_CHECK, WW_STYMD)
                WF_STYMD.Text = WW_CHECK
            Catch ex As Exception
                WF_STYMD.Text = C_DEFAULT_YMD
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STYMD.Focus()
            Exit Sub
        End If

        '●出庫日(TO) WF_ENDYMD.Text
        '①単項目チェック
        Dim WW_ENDYMD As Date
        WW_CHECK = WF_ENDYMD.Text
        Master.CheckField(WF_CAMPCODE.Text, "ENDYMD", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WW_CHECK, WW_ENDYMD)
                WF_ENDYMD.Text = WW_CHECK
            Catch ex As Exception
                WF_ENDYMD.Text = C_MAX_YMD
                WW_ENDYMD = C_MAX_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_ENDYMD.Focus()
            Exit Sub
        End If

        '②関連チェック(開始＞終了)
        If WF_STYMD.Text <> "" And WF_ENDYMD.Text <> "" Then
            If WW_STYMD > WW_ENDYMD Then
                Master.Output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                WF_STYMD.Focus()
                Exit Sub
            End If
        End If

        '●運用部署 WF_UORG.Text
        '①単項目チェック
        WW_CHECK = WF_UORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "UORG", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If WF_UORG.Text <> "" Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, WF_UORG.Text, WF_UORG_Text.Text, O_RTN, work.createORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    WF_UORG.Focus()
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_UORG.Focus()
            Exit Sub
        End If

        '●従業員 WF_STAFFCODE.Text
        '①単項目チェック
        WW_CHECK = WF_STAFFCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFCODE", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If WF_STAFFCODE.Text <> "" Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WF_STAFFCODE.Text, WF_STAFFCODE_Text.Text, O_RTN, work.createSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text))
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    WF_UORG.Focus()
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STAFFCODE.Focus()
            Exit Sub
        End If
        '●従業員名 WF_STAFFNAME
        '①単項目チェック
        WW_CHECK = WF_STAFFNAME.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFNAMES", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STAFFCODE.Focus()
            Exit Sub
        End If

        '正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
    End Sub


End Class
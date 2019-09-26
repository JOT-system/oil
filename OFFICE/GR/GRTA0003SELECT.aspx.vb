Imports OFFICE.GRIS0005LeftBox

Public Class GRTA0003SELECT
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション管理
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
        SetMapValue()

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
        '名称設定処理 
        SetNameValue()
        '■■■ セッション変数　反映 ■■■
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        work.WF_SEL_HORG.Text = WF_HORG.Text
        work.WF_SEL_TAISHOYM.Text = CDate(WF_TAISHOYM.Text).ToString("yyyy/MM")
        '画面遷移実行
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)
        Master.checkParmissionCode(WF_CAMPCODE.Text)
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
                        Case "WF_TAISHOYM"
                            .WF_Calendar.Text = String.Format("{0}/01", WF_TAISHOYM.Text)
                    End Select
                    .activeCalendar()
                Else
                    Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            prmData = work.CreateHORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE, WF_TAISHOYM.Text & "/01")
                    End Select
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
            Case "WF_HORG"
                WF_HORG_Text.Text = values(1)
                WF_HORG.Text = values(0)
                WF_HORG.Focus()
            Case "WF_TAISHOYM"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_TAISHOYM.Text = ""
                    Else
                        WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WF_TAISHOYM.Focus()
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
            Case "WF_HORG"
                WF_HORG.Focus()
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
        WF_HORG_Text.Text = ""

        '■■■ チェック処理 ■■■
        WW_ERR_SW = C_MESSAGE_NO.NORMAL
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
    Protected Sub SetMapValue()

        '■■■ 選択画面の入力初期値設定 ■■■]        
        If IsNothing(Master.MAPID) Then
            Master.MAPID = GRTA0003WRKINC.MAPIDS
        End If
        '　メニューから遷移/業務画面戻り判定　
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then                                                      'メニューからの画面遷移
            work.Initialize()
            '○画面項目設定（変数より）処理
            SetInitialValue()
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0003 Then     '実行画面からの画面遷移
            '画面設定
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            WF_HORG.Text = work.WF_SEL_HORG.Text
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text
        End If

        '○RightBox情報設定
        rightview.MAPID = GRTA0001WRKINC.MAPID
        rightview.MAPIDS = GRTA0001WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■名称設定
        SetNameValue()
    End Sub
    ''' <summary>
    ''' 変数設定用処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()

        '■■■ 変数設定処理 ■■■

        '会社コード
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)

        '配属部署
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "HORG", WF_HORG.Text)

        '対象年変数設定
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text)
        If IsDate(WF_TAISHOYM.Text) Then
            WF_TAISHOYM.Text = CDate(WF_TAISHOYM.Text).ToString("yyyy/MM")
        Else
            WF_TAISHOYM.Text = WF_TAISHOYM.Text
        End If
    End Sub
    ''' <summary>
    ''' 名称設定処理      
    ''' </summary>
    ''' <remarks></remarks>                                                       
    Protected Sub SetNameValue()

        '■名称設定
        '会社コード　
        CodeToName("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)
        '配属部署　
        CodeToName("HORG", WF_HORG.Text, WF_HORG_Text.Text, WW_DUMMY)

    End Sub
    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD">フィールド名</param>
    ''' <param name="I_VALUE">コード値</param>
    ''' <param name="O_TEXT">名称</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "CAMPCODE"
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)                     '会社コード
                Case "HORG"
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateHORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE, WF_TAISHOYM.Text & "/01"))                     '出荷部署
                Case Else
                    O_TEXT = String.Empty
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

        ' 会社コード
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)
        ' 対象年月
        Master.eraseCharToIgnore(WF_TAISHOYM.Text)
        ' 配属部署
        Master.eraseCharToIgnore(WF_HORG.Text)

        '■■■ 入力項目チェック ■■■
        Dim WW_CS0024FCHECKERR As String = String.Empty
        Dim WW_CS0024FCHECKREPORT As String = String.Empty
        Dim WW_CHECK As String = String.Empty
        WF_FIELD.Value = ""

        ' 会社コード
        '①単項目チェック
        WW_CHECK = WF_CAMPCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_CAMPCODE.Text) Then
                leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, O_RTN)
                If Not isNormal(O_RTN) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
            O_RTN = WW_CS0024FCHECKERR
            WF_CAMPCODE.Focus()
            Exit Sub
        End If

        'WF_TAISHOYY.Text
        '①単項目チェック
        WW_CHECK = WF_TAISHOYM.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "TAISHOYM", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Dim WW_Date As Date
            Try
                '日付チェック
                Date.TryParse(WF_TAISHOYM.Text & "/01", WW_Date)
                If WW_Date < C_DEFAULT_YMD Then
                    Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "対象年月 : " & WF_TAISHOYM.Text)
                    WF_TAISHOYM.Focus()
                    O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    Exit Sub
                End If
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "対象年月 : " & WF_TAISHOYM.Text)
                WF_TAISHOYM.Focus()
                O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                Exit Sub
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_TAISHOYM.Focus()
            Exit Sub
        End If

        '配属部署
        '①単項目チェック
        WW_CHECK = WF_HORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "HORG", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_HORG.Text) Then
                leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, WF_HORG.Text, WF_HORG_Text.Text, O_RTN, work.CreateHORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE, WF_TAISHOYM.Text & "/01"))
                If Not isNormal(O_RTN) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_HORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_HORG.Focus()
            Exit Sub
        End If

        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
    End Sub
End Class
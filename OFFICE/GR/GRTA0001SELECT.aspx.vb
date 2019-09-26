Imports OFFICE.GRIS0005LeftBox

Public Class GRTA0001SELECT
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' セッション情報
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション情報
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
                    Case "WF_ButtonDO"                      '■ 実行ボタンクリック時処理
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                     '■ 終了ボタンクリック時処理
                        WF_ButtonEND_Click()
                    Case "WF_ButtonSel"                     '■ 左ボックス選択ボタン押下時処理
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                     '■ 左ボックスキャンセルボタン押下時処理
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"                 '■ 入力領域ダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_TextChange"                    '■ 入力領域変更時処理
                        WW_LeftBoxReSet()
                    Case "WF_ListboxDBclick"                '■ 左ボックスダブルクリック処理
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"            '■ 左ボックス選択時処理
                        WF_LEFTBOX_SELECT_Click()
                    Case "WF_RIGHT_VIEW_DBClick"            '■ 右ボックス表示処理
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                    '■ メモ欄保存処理
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
        WW_ERR_SW = C_MESSAGE_NO.NORMAL
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '名称設定処理 
        SetValueName()
        '■■■ セッション変数　反映 ■■■
        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '出庫日　
        work.WF_SEL_SHUKODATEF.Text = WF_SHUKODATEF.Text
        '出荷部署
        work.WF_SEL_SHIPORG.Text = WF_SHIPORG.Text
        '出荷部署名称
        work.WF_SEL_SHIPORGNAME.Text = WF_SHIPORG_Text.Text

        '機能選択
        If WF_right_SW1.Checked = True Then
            '乗務員別
            work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.DRIVER
        ElseIf WF_right_SW2.Checked = True Then
            '車番別
            work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.CARNUM
        ElseIf WF_right_SW3.Checked = True Then
            '出荷場所別
            work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.DESTPOS
        End If
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
                        Case "WF_SHUKODATEF"
                            .WF_Calendar.Text = WF_SHUKODATEF.Text
                    End Select
                    .activeCalendar()
                Else
                    Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            prmData = work.createSHIPORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
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

            Case "WF_SHUKODATEF"
                '出庫日(FROM)
                '日付(FROM)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    WF_SHUKODATEF.Text = WW_DATE
                Catch ex As Exception
                End Try
                WF_SHUKODATEF.Focus()

            Case "WF_SHIPORG"
                '出荷部署 
                WF_SHIPORG_Text.Text = values(1)
                WF_SHIPORG.Text = values(0)
                WF_SHIPORG.Focus()
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
            Case "WF_SHIPORG"
                '出荷部署　　 
                WF_SHIPORG.Focus()
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
        WF_SHIPORG_Text.Text = ""

        '■■■ チェック処理 ■■■
        WW_ERR_SW = C_MESSAGE_NO.NORMAL
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■名称設定
        SetValueName()

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
        If IsNothing(Master.MAPID) Then Master.MAPID = GRTA0001WRKINC.MAPIDS

        'メニューからの画面遷移
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then           'メニューからの画面遷移
            work.initialize()
            '○画面項目設定（変数より）処理
            SetInitialValue()
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.TA0001 Then     '実行画面からの画面遷移
            '■■■ 実行画面からの画面遷移 ■■■
            '○画面項目設定
            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '出庫日
            WF_SHUKODATEF.Text = work.WF_SEL_SHUKODATEF.Text
            '出荷部署
            WF_SHIPORG.Text = work.WF_SEL_SHIPORG.Text
            WF_SHIPORG_Text.Text = work.WF_SEL_SHIPORGNAME.Text
            '機能選択
            If work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.DRIVER Then
                '乗務員別
                WF_right_SW1.Checked = True
            ElseIf work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.CARNUM Then
                '車番別
                WF_right_SW2.Checked = True
            ElseIf work.WF_SEL_FUNCSEL.Text = GRTA0001WRKINC.C_LIST_FUNSEL.DESTPOS Then
                '出荷場所別
                WF_right_SW3.Checked = True
            End If
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
        SetValueName()
    End Sub

    ''' <summary>
    ''' 変数設定用処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()

        '■■■ 変数設定処理 ■■■

        '会社コード
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)

        '出庫日(FROM)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHUKODATEF", WF_SHUKODATEF.Text)
        '機能選択
        Dim WW_FUNCSEL As String = ""
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "FUNCSEL", WW_FUNCSEL)

        Select Case WW_FUNCSEL
            Case GRTA0001WRKINC.C_LIST_FUNSEL.DRIVER   '乗務員別
                WF_right_SW1.Checked = True
                WF_right_SW2.Checked = False
                WF_right_SW3.Checked = False
            Case GRTA0001WRKINC.C_LIST_FUNSEL.CARNUM    '車番別
                WF_right_SW1.Checked = False
                WF_right_SW2.Checked = True
                WF_right_SW3.Checked = False
            Case GRTA0001WRKINC.C_LIST_FUNSEL.DESTPOS    '出荷場所別
                WF_right_SW1.Checked = False
                WF_right_SW2.Checked = False
                WF_right_SW3.Checked = True
        End Select
        '出荷部署
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHIPORG", WF_SHIPORG.Text)
    End Sub

    ''' <summary>
    ''' 名称設定処理      
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetValueName()

        '会社コード　
        CodeToName("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)
        '出荷部署
        CodeToName("SHIPORG", WF_SHIPORG.Text, WF_SHIPORG_Text.Text, WW_DUMMY)

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
                Case "SHIPORG"
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createSHIPORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))                     '出荷部署
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

        '■■■ 入力文字置き換え ■■■
        '会社コード WF_CAMPCODE.Text
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)
        '出庫日(FROM) WF_SHUKODATEF.Text
        Master.eraseCharToIgnore(WF_SHUKODATEF.Text)
        '出荷部署 WF_SHIPORG.Text
        Master.eraseCharToIgnore(WF_SHIPORG.Text)
        '■■■ 入力項目チェック ■■■
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""

        '●会社コード WF_CAMPCODE.Text
        '①単項目チェック
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_CAMPCODE.Text) Then
                leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, O_RTN)
                If Not isNormal(O_RTN) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_CAMPCODE.Focus()
            Exit Sub
        End If

        '●出庫日(FROM) WF_SHUKODATEF.Text
        '入力チェック(出庫日)
        If String.IsNullOrEmpty(WF_SHUKODATEF.Text) Then
            Master.output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)
            O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            Exit Sub
        End If

        '●出庫日(FROM) WF_SHUKODATEF.Text
        '①単項目チェック
        Master.checkFIeld(WF_CAMPCODE.Text, "SHUKODATEF", WF_SHUKODATEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Dim WW_SHUKODATEF As Date
            Try
                Date.TryParse(WF_SHUKODATEF.Text, WW_SHUKODATEF)
            Catch ex As Exception
                WW_SHUKODATEF = C_DEFAULT_YMD
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_SHUKODATEF.Focus()
            Exit Sub
        End If

        '●出荷部署 WF_SHIPORG.Text
        '①単項目チェック
        Master.checkFIeld(WF_CAMPCODE.Text, "SHIPORG", WF_SHIPORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_SHIPORG.Text) Then
                leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, WF_SHIPORG.Text, WF_SHIPORG_Text.Text, O_RTN, work.createSHIPORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))
                If Not isNormal(O_RTN) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHIPORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_SHIPORG.Focus()
            Exit Sub
        End If
        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

End Class

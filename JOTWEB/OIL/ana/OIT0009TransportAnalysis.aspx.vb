Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 品種マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class OIT0009TransportAnalysis
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

        Master.MAPID = OIT0009WRKINC.MAPIDTA

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
        Dim WW_TEXT As String = ""

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0009WRKINC.MAPIDTA
        'rightview.MAPID = OIT0009WRKINC.MAPIDL
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '〇営業所選択欄
        Dim paramData As Hashtable = work.CreateFIXParam(Master.USER_ORG)
        Me.tileSalesOffice.ListBoxClassification = LIST_BOX_CLASSIFICATION.LC_SALESOFFICE
        Me.tileSalesOffice.ParamData = paramData
        Me.tileSalesOffice.LeftObj = leftview
        Me.tileSalesOffice.chklGrc0001SelectionBox.RepeatLayout = RepeatLayout.Table
        Me.tileSalesOffice.chklGrc0001SelectionBox.RepeatDirection = RepeatDirection.Horizontal
        Me.tileSalesOffice.chklGrc0001SelectionBox.RepeatColumns = 7
        Me.tileSalesOffice.SetTileValues()

        '〇日付入力欄
        '日報は当日
        Me.txtDailyStYmd.Text = Now.ToString("yyyy/MM/dd")
        Me.txtDailyEdYmd.Text = Now.ToString("yyyy/MM/dd")
        '月報は月初～月末
        Me.txtMonthlyStYmd.Text = New Date(Now.Year, Now.Month, 1).ToString("yyyy/MM/dd")
        Me.txtMonthlyEdYmd.Text = New Date(Now.Year, Now.Month + 1, 1).AddDays(-1).ToString("yyyy/MM/dd")

        '○ 名称設定処理
    End Sub


    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(Master.USERCAMP)
        End If

        Master.CheckParmissionCode(Master.USERCAMP)
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
        ''営業所コード
        'WW_TEXT = WF_OFFICECODE.Text
        'Master.CheckField(Master.USERCAMP, "OFFICECODE", WF_OFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        'If isNormal(WW_CS0024FCHECKERR) Then
        '    If String.IsNullOrEmpty(WW_TEXT) Then
        '        WF_OFFICECODE.Text = ""
        '    Else
        '        '存在チェック
        '        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
        '        If Not isNormal(WW_RTN_SW) Then
        '            Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "営業所コード : " & WF_OFFICECODE.Text, needsPopUp:=True)
        '            WF_OFFICECODE.Focus()
        '            O_RTN = "ERR"
        '            Exit Sub
        '        End If
        '    End If
        'Else
        '    Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所コード", needsPopUp:=True)
        '    WF_OFFICECODE.Focus()
        '    O_RTN = "ERR"
        '    Exit Sub
        'End If

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


    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
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
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "txtDailyStYmd"     '日報 - 期間FROM
                                .WF_Calendar.Text = Me.txtDailyStYmd.Text
                            Case "txtDailyEdYmd"     '日報 - 期間FROM
                                .WF_Calendar.Text = Me.txtDailyEdYmd.Text
                        End Select
                        .ActiveCalendar()
                End Select
            End With

        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

    End Sub

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        'Dim WW_SelectValue As String = ""
        'Dim WW_SelectText As String = ""
        Dim wkDate As Date

        '○ 選択内容を取得
        'If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
        '    WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
        '    WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
        '    WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        'End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            'Case Me.WF_OFFICECODE.ID
            '    '営業所コード
            '    Me.WF_OFFICECODE.Text = WW_SelectValue
            '    Me.WF_OFFICECODE_TEXT.Text = WW_SelectText
            '    Me.WF_OFFICECODE.Focus()
            Case Me.txtDailyStYmd.ID
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        Me.txtDailyStYmd.Text = ""
                    Else
                        Me.txtDailyStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                Me.txtDailyStYmd.Focus()

            Case Me.txtDailyEdYmd.ID
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, wkDate)
                    If wkDate < CDate(C_DEFAULT_YMD) Then
                        Me.txtDailyEdYmd.Text = ""
                    Else
                        Me.txtDailyEdYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                Me.txtDailyEdYmd.Focus()
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
            'Case Me.WF_OFFICECODE.ID
            '    '営業所コード
            '    Me.WF_OFFICECODE.Focus()
            Case Me.txtDailyStYmd.ID
                Me.txtDailyStYmd.Focus()
            Case Me.txtDailyEdYmd.ID
                Me.txtDailyEdYmd.Focus()
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

        rightview.InitViewID(Master.USERCAMP, WW_DUMMY)

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

        Try
            Select Case I_FIELD
                'Case "OFFICECODE"
                '    '営業所コード
                '    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                'Case "SHIPPERCODE"
                '    '荷主コード
                '    prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                'Case "PLANTCODE"
                '    '基地コード
                '    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                'Case "BIGOILCODE"
                '    '油種大分類コード
                '    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                'Case "MIDDLEOILCODE"
                '    '油種中分類コード
                '    prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                'Case "OTOILCODE"
                '    'OT油種コード
                '    prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STOCKFLG"
                    '在庫管理対象フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class

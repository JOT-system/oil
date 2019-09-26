Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 品名マスタ（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMD0001SELECT
    Inherits Page

    '共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
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
                    Case "WF_Field_DBClick"
                        WF_Field_DBClick()
                    Case "WF_ListboxDBclick"
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"
                        WF_LEFTBOX_SELECT_CLICK()
                    Case "WF_RIGHT_VIEW_DBClick"
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"
                        WF_RIGHTBOX_Change()
                    Case Else
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
    Protected Sub Initialize()
        '○初期値設定
        WF_STYMD.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        Master.MAPID = GRMD0001WRKINC.MAPIDS

        leftview.activeListBox()

        '○画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)

        '○チェック処理
        WW_Check(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_SEL_STYMD.Text = WF_STYMD.Text              '有効年月日
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If

        Master.VIEWID = rightview.getViewId(work.WF_SEL_CAMPCODE.Text)

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.transitionPage()
        End If

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' leftBOX選択ボタン押下時処理
    ''' </summary>
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
                WF_CAMPCODE_TEXT.Text = WW_SelectTEXT
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
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
        rightview.initViewID(work.WF_SEL_CAMPCODE.Text, WW_DUMMY)
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
    ''' 左ボックスフィールドダブルクリック時
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
                    Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

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
    ''' 左ボックスキャンセルボタン押下時処理
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
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_TEXT.Text = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)

        '○チェック処理
        WW_Check(WW_DUMMY)

        '○名称設定
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)          '会社コード

    End Sub
    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移
            '○ワーク初期化
            work.initialize()

            '○初期変数設定処理
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)       '会社コード
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)             '有効年月日(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)           '有効年月日(To)

            '○RightBox情報設定
            rightview.MAPID = GRMD0001WRKINC.MAPID
            rightview.MAPIDS = GRMD0001WRKINC.MAPIDS
            rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MD0001 Then         '実行画面からの画面遷移
            '○画面項目設定処理                                       
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード                            
            WF_STYMD.Text = work.WF_SEL_STYMD.Text              '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text            '有効年月日(To)

            '○RightBox情報設定
            rightview.MAPID = GRMD0001WRKINC.MAPID
            rightview.MAPIDS = GRMD0001WRKINC.MAPIDS
            rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        End If

        '○名称設定処理
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)          '会社コード

    End Sub

    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        '○初期設定
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_STYMD As Date
        Dim WW_ENDYMD As Date

        '○単項目チェック
        '会社コード WF_CAMPCODE.Text
        WW_TEXT = WF_CAMPCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WW_TEXT = "" Then
                WF_CAMPCODE.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = WW_RTN_SW
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
            Exit Sub
        End If

        '有効年月日(From) WF_STYMD.Text
        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_STYMD.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STYMD.Focus()
            O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
            Exit Sub
        End If

        '有効年月日(To)
        '初期設定
        If WF_ENDYMD.Text = Nothing Then
            WF_ENDYMD.Text = WF_STYMD.Text
        End If

        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_ENDYMD.Text, WW_ENDYMD)
            Catch ex As Exception
                WW_ENDYMD = C_MAX_YMD
            End Try
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ENDYMD.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '日付関連チェック
        If WF_STYMD.Text <> "" AndAlso WF_ENDYMD.Text <> "" Then
            If WW_STYMD > WW_ENDYMD Then
                Master.output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                WF_STYMD.Focus()
                O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                Exit Sub
            End If
        End If

    End Sub

End Class

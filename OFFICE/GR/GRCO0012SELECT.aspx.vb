Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' サーバ権限マスタ（条件）
''' </summary>
''' <remarks>
''' </remarks>
Public Class GRCO0012SELECT
    Inherits Page

    '○ 共通処理結果
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
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonDO"                  '実行ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '終了ボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FIELD_DBClick()
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
        If String.IsNullOrEmpty(Master.MAPID) Then Master.MAPID = GRCO0012WRKINC.MAPIDS

        WF_STYMD.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()

        '○ 画面の値設定
        WW_MAPValueSet()
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
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()
        '■■■ 入力文字置き換え ■■■　…　画面固有処理
        '   画面PassWord内の使用禁止文字排除
        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.eraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.eraseCharToIgnore(WF_TERMIDF.Text)           '端末ID(From)
        Master.eraseCharToIgnore(WF_TERMIDF.Text)           '端末ID(To)
        '■■■ 入力項目チェック ■■■　…　画面固有処理
        '○ チェック処理
        Dim WW_ERR_SW As String = C_MESSAGE_NO.NORMAL
        WW_Check(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■■■ バリアント反映 ■■■　…　画面固有処理
        work.WF_SEL_STYMD.Text = WF_STYMD.Text
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If

        work.WF_SEL_TERMIDF.Text = WF_TERMIDF.Text
        If WF_TERMIDT.Text = "" Then
            work.WF_SEL_TERMIDT.Text = WF_TERMIDF.Text
        Else
            work.WF_SEL_TERMIDT.Text = WF_TERMIDT.Text
        End If

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.getViewId(work.WF_SEL_CAMPCODE.Text)
        '画面遷移実行
        Master.checkParmissionCode(work.WF_SEL_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.transitionPage()
        End If

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
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        Select Case WF_LeftMViewChange.Value
                            Case LIST_BOX_CLASSIFICATION.LC_TERM
                                prmData = work.CreateTERMIDParam()
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
            End With
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

        Dim values As String() = leftview.getActiveValue

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_STYMD"         '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = WW_DATE.ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"        '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = WW_DATE.ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WF_ENDYMD.Focus()

            Case "WF_TERMIDF"       '端末ID(From)
                WF_TERMIDF.Text = values(0)
                WF_TERMIDF_Text.Value = values(1)
                WF_TERMIDF.Focus()

            Case "WF_TERMIDT"       '端末ID(To)
                WF_TERMIDT.Text = values(0)
                WF_TERMIDT_Text.Value = values(1)
                WF_TERMIDT.Focus()
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
            Case "WF_STYMD"         '有効年月日(From)
                WF_STYMD.Focus()
            Case "WF_ENDYMD"        '有効年月日(To)
                WF_ENDYMD.Focus()
            Case "WF_TERMIDF"       '端末ID(From)
                WF_TERMIDF.Focus()
            Case "WF_TERMIDT"       '端末ID(To)
                WF_TERMIDT.Focus()
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

        rightview.initViewID(work.WF_SEL_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  初期値設定処理                                                        ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '■■■ 選択画面の入力初期値設定 ■■■　…　画面固有処理
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then             'メニューからの画面遷移
            work.Initialize()
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)                 '有効年月日(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)               '有効年月日(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TERMIDFrom", WF_TERMIDF.Text)          '端末ID(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TERMIDTo", WF_TERMIDT.Text)            '端末ID(To)
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.CO0012 Then       '実行画面からの遷移
            '画面項目設定処理
            WF_STYMD.Text = work.WF_SEL_STYMD.Text              '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text            '有効年月日(To)
            WF_TERMIDF.Text = work.WF_SEL_TERMIDF.Text          '端末ID(From)
            WF_TERMIDT.Text = work.WF_SEL_TERMIDT.Text          '端末ID(To)
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = GRCO0012WRKINC.MAPIDS
        rightview.MAPID = GRCO0012WRKINC.MAPID
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック

        '有効年月日(From)
        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(From) : " & WF_STYMD.Text)
            WF_STYMD.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '有効年月日(To)
        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(To) : " & WF_ENDYMD.Text)
            WF_ENDYMD.Focus()
            O_RTN = WW_CS0024FCHECKERR
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
                    O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                    Exit Sub
                End If
            Catch ex As Exception
                Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, WF_STYMD.Text & ":" & WF_ENDYMD.Text)
                WF_STYMD.Focus()
                O_RTN = C_MESSAGE_NO.DATE_FORMAT_ERROR
                Exit Sub
            End Try
        End If

        '端末ID(From)
        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TERMIDF", WF_TERMIDF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("TERMID", WF_TERMIDF.Text, WF_TERMIDF_Text.Value, O_RTN)
            If Not isNormal(O_RTN) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "端末ID(From) : " & WF_TERMIDF.Text)
                WF_TERMIDF.Focus()
                O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "端末ID(From) : " & WF_TERMIDF.Text)
            WF_TERMIDF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '端末ID(To)
        Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TERMIDT", WF_TERMIDT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("TERMID", WF_TERMIDT.Text, WF_TERMIDT_Text.Value, O_RTN)
            If Not isNormal(O_RTN) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "端末ID(To) : " & WF_TERMIDT.Text)
                WF_TERMIDT.Focus()
                O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "端末ID(To) : " & WF_TERMIDT.Text)
            WF_TERMIDT.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '端末ID大小チェック
        If WF_TERMIDF.Text <> "" AndAlso WF_TERMIDT.Text <> "" Then
            If WF_TERMIDF.Text > WF_TERMIDT.Text Then
                Master.output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                WF_TERMIDF.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '○ 正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub
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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "TERMID"            '端末ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TERM, I_VALUE, O_TEXT, O_RTN, work.CreateTERMIDParam())
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

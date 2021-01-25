Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0024PrioritySearch
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
        Master.MAPID = OIM0024WRKINC.MAPIDS

        WF_OFFICECODE.Focus()
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
            Master.GetFirstValue(work.WF_SEL_OFFICECODE.Text, "OFFICECODE", WF_OFFICECODE.Text)             '管轄営業所
            Master.GetFirstValue(work.WF_SEL_OILCODE.Text, "OILCODE", WF_OILCODE.Text)                      '油種コード
            Master.GetFirstValue(work.WF_SEL_SEGMENTOILCODE.Text, "SEGMENTOILCODE", WF_SEGMENTOILCODE.Text) '油種細分コード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0024L Then   '実行画面からの遷移
            '画面項目設定処理
            WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE.Text                                                '管轄営業所
            WF_OILCODE.Text = work.WF_SEL_OILCODE.Text                                                      '油種コード
            WF_SEGMENTOILCODE.Text = work.WF_SEL_SEGMENTOILCODE.Text                                        '油種細分コード
        End If

        ''入力制限(数値0～9、.)
        'WF_LOAD.Attributes("onkeyPress") = "CheckNumDot()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0024WRKINC.MAPIDS
        rightview.MAPID = OIM0024WRKINC.MAPIDL
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_DUMMY)               '管轄営業所
        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_DUMMY)                        '油種コード
        CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_DUMMY)   '油種細分コード

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_OFFICECODE.Text)        '管轄営業所
        Master.EraseCharToIgnore(WF_OILCODE.Text)           '油種コード
        Master.EraseCharToIgnore(WF_SEGMENTOILCODE.Text)    '油種細分コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_OFFICECODE.Text = WF_OFFICECODE.Text            '管轄営業所
        work.WF_SEL_OILCODE.Text = WF_OILCODE.Text                  '油種コード
        work.WF_SEL_SEGMENTOILCODE.Text = WF_SEGMENTOILCODE.Text    '油種細分コード

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
        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_LINE_ERR As String = ""
        Dim dateErrFlag As String = ""

        '○ 単項目チェック
        '管轄営業所
        WW_TEXT = WF_OFFICECODE.Text
        Master.CheckField(Master.USERCAMP, "OFFICECODE", WF_OFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_OFFICECODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "管轄営業所 : " & WF_OFFICECODE.Text, needsPopUp:=True)
                    WF_OFFICECODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "管轄営業所", needsPopUp:=True)
            WF_OFFICECODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '油種コード
        WW_TEXT = WF_OILCODE.Text
        Master.CheckField(Master.USERCAMP, "OILCODE", WF_OILCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_OILCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "油種コード : " & WF_OILCODE.Text, needsPopUp:=True)
                    WF_OILCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "油種コード", needsPopUp:=True)
            WF_OILCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '油種細分コード
        WW_TEXT = WF_SEGMENTOILCODE.Text
        Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_SEGMENTOILCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "油種細分コード : " & WF_SEGMENTOILCODE.Text, needsPopUp:=True)
                    WF_SEGMENTOILCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "油種細分コード", needsPopUp:=True)
            WF_SEGMENTOILCODE.Focus()
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

                Select Case WF_LeftMViewChange.Value

                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR

                    Case Else

                        Dim prmData As New Hashtable

                        Select Case WF_FIELD.Value
                            Case WF_OFFICECODE.ID
                                '管轄営業所
                                prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                            Case WF_OILCODE.ID
                                '油種コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
                            Case WF_SEGMENTOILCODE.ID
                                '油種細分コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "SEGMENTOILCODE")
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
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
            Case WF_OFFICECODE.ID
                '関連項目
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
                '管轄営業所
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)

            Case WF_OILCODE.ID
                '関連項目
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)
                '油種コード
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)

            Case WF_SEGMENTOILCODE.ID
                '関連項目
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
                '油種細分コード
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)

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
            Case WF_OFFICECODE.ID
                '管轄営業所
                WF_OFFICECODE.Text = WW_SelectValue
                WF_OFFICECODE_TEXT.Text = WW_SelectText
                WF_OFFICECODE.Focus()

                '関連項目処理
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)                       '油種コード
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)  '油種細分コード

            Case WF_OILCODE.ID
                '油種コード
                WF_OILCODE.Text = WW_SelectValue
                WF_OILCODE_TEXT.Text = WW_SelectText
                WF_OILCODE.Focus()

                '関連項目処理
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)              '管轄営業所
                CODENAME_get("SEGMENTOILCODE", WF_SEGMENTOILCODE.Text, WF_SEGMENTOILCODE_TEXT.Text, WW_RTN_SW)  '油種細分コード

            Case WF_SEGMENTOILCODE.ID
                '油種細分コード
                WF_SEGMENTOILCODE.Text = WW_SelectValue
                WF_SEGMENTOILCODE_TEXT.Text = WW_SelectText
                WF_SEGMENTOILCODE.Focus()

                '関連項目処理
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)              '管轄営業所
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)                       '油種コード
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
            Case WF_OFFICECODE.ID
                '管轄営業所
                WF_OFFICECODE.Focus()

            Case WF_OILCODE.ID
                '油種コード
                WF_OILCODE.Focus()

            Case WF_SEGMENTOILCODE.ID
                '油種細分コード
                WF_SEGMENTOILCODE.Focus()
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
                Case "OFFICECODE"
                    '管轄営業所
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILCODE"
                    '油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SEGMENTOILCODE"
                    '油種細分コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "SEGMENTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
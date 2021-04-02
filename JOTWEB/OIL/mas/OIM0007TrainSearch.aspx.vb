Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0007TrainSearch
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
                    Case "WF_ButtonDO"                  ' 検索ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 ' 戻るボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_Field_DBClick"             ' フィールドダブルクリック
                        WF_FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        ' フィールドチェンジ
                        WF_FIELD_Change()
                    Case "WF_ButtonSel"                 ' (左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 ' (左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            ' 左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case "WF_RIGHT_VIEW_DBClick"        ' 右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                ' メモ欄更新
                        WF_RIGHTBOX_Change()
                    Case "HELP"                         ' ヘルプ表示
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
        Master.MAPID = OIM0007WRKINC.MAPIDS

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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then
            ' メニューからの画面遷移

            ' 画面間の情報クリア
            work.Initialize()

            ' 初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_OFFICECODE.Text, "OFFICECODE", WF_OFFICECODE.Text)     ' 管轄受注営業所
            Master.GetFirstValue(work.WF_SEL_TRAINNO.Text, "TRAINNO", WF_TRAINNO.Text)              ' 本線列車番号
            Master.GetFirstValue(work.WF_SEL_TSUMI.Text, "TSUMI", WF_TSUMI.Text)                    ' 積置フラグ

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0007L Then
            ' 実行画面（一覧）からの遷移

            ' 画面項目設定処理
            WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE.Text    ' 管轄受注営業所
            WF_TRAINNO.Text = work.WF_SEL_TRAINNO.Text          ' 本線列車番号
            WF_TSUMI.Text = work.WF_SEL_TSUMI.Text              ' 積置フラグ

        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0007WRKINC.MAPIDS
        rightview.MAPID = OIM0007WRKINC.MAPIDL
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_OFFICECODE.Text)        ' 管轄受注営業所
        Master.EraseCharToIgnore(WF_TRAINNO.Text)           ' 本線列車番号
        Master.EraseCharToIgnore(WF_TSUMI.Text)             ' 積置フラグ

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_OFFICECODE.Text = WF_OFFICECODE.Text    ' 管轄受注営業所
        work.WF_SEL_TRAINNO.Text = WF_TRAINNO.Text          ' 本線列車番号
        work.WF_SEL_TSUMI.Text = WF_TSUMI.Text              ' 積置フラグ

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(Master.USERCAMP)
        End If

        Master.CheckParmissionCode(Master.USERCAMP)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            ' 画面遷移
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        ' 管轄受注営業所
        Master.CheckField(Master.USERCAMP, "OFFICECODE", WF_OFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WF_OFFICECODE.Text <> "" Then
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "営業所 : " & WF_OFFICECODE.Text, needsPopUp:=True)
                    WF_OFFICECODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所", needsPopUp:=True)
            WF_OFFICECODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 本線列車番号
        Master.CheckField(Master.USERCAMP, "TRAINNO", WF_TRAINNO.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WF_TRAINNO.Text <> "" Then
                CODENAME_get("TRAINNO", WF_TRAINNO.Text, WF_TRAINNO_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "本線列車番号 : " & WF_TRAINNO.Text, needsPopUp:=True)
                    WF_TRAINNO.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "本線列車番号", needsPopUp:=True)
            WF_TRAINNO.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        ' 積置フラグ
        Master.CheckField(Master.USERCAMP, "TSUMI", WF_TSUMI.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WF_TSUMI.Text <> "" Then
                CODENAME_get("TSUMI", WF_TSUMI.Text, WF_TSUMI_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "積置フラグ : " & WF_TSUMI.Text, needsPopUp:=True)
                    WF_TSUMI.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "積置フラグ", needsPopUp:=True)
            WF_TSUMI.Focus()
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
                Dim prmData As New Hashtable

                ' 管轄受注営業所
                If WF_FIELD.Value = WF_OFFICECODE.ID Then
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                End If
                ' 本線列車番号
                If WF_FIELD.Value = WF_TRAINNO.ID Then
                    If String.IsNullOrEmpty(WF_OFFICECODE.Text) Then
                        '管轄受注営業所コード未設定の場合、所属組織コードで検索する
                        prmData = work.CreateTrainNoParam(Master.USER_ORG)
                    Else
                        prmData = work.CreateTrainNoParam(WF_OFFICECODE.Text)
                    End If
                End If
                ' 積置フラグ
                If WF_FIELD.Value = WF_TSUMI.ID Then
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TSUMI")
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
            Case WF_OFFICECODE.ID           ' 管轄受注営業所
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
            Case WF_TRAINNO.ID              ' 本線列車番号
                CODENAME_get("TRAINNO", WF_TRAINNO.Text, WF_TRAINNO_TEXT.Text, WW_RTN_SW)
            Case WF_TSUMI.ID                ' 積置フラグ
                CODENAME_get("TSUMI", WF_TSUMI.Text, WF_TSUMI_TEXT.Text, WW_RTN_SW)
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
            Case WF_OFFICECODE.ID           ' 管轄受注営業所
                WF_OFFICECODE.Text = WW_SelectValue
                WF_OFFICECODE_TEXT.Text = WW_SelectText
                WF_OFFICECODE.Focus()
            Case WF_TRAINNO.ID              ' 本線列車番号
                WF_TRAINNO.Text = WW_SelectValue
                WF_TRAINNO_TEXT.Text = WW_SelectText
                WF_TRAINNO.Focus()
            Case WF_TSUMI.ID                ' 積置フラグ
                WF_TSUMI.Text = WW_SelectValue
                WF_TSUMI_TEXT.Text = WW_SelectText
                WF_TSUMI.Focus()
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
            Case WF_OFFICECODE.ID       ' 管轄受注営業所
                WF_OFFICECODE.Focus()
            Case WF_TRAINNO.ID          ' 本線列車番号
                WF_TRAINNO.Focus()
            Case WF_TSUMI.ID            ' 積置フラグ
                WF_TSUMI.Focus()
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
                Case "OFFICECODE"   ' 管轄受注営業所
                    prmData = work.CreateOfficeCodeParam(Master.USER_ORG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TRAINNO"      ' 本線列車番号
                    If String.IsNullOrEmpty(WF_OFFICECODE.Text) Then
                        '管轄受注営業所コード未設定の場合、所属組織コードで検索する
                        prmData = work.CreateTrainNoParam(Master.USER_ORG, I_VALUE)
                    Else
                        prmData = work.CreateTrainNoParam(WF_OFFICECODE.Text, I_VALUE)
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TSUMI"        ' 積置フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "TSUMI")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
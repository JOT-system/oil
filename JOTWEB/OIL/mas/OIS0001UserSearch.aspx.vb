''************************************************************
' ユーザIDマスタメンテ検索画面
' 作成日 2019/11/14
' 更新日 2019/11/14
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' ユーザIDマスタ登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class OIS0001UserSearch
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo            'ユーザ情報取得

    ''' <summary>
    ''' 共通処理結果
    ''' </summary>
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_ORGCODE_INFOSYS As String = "010006"        '組織コード_情報システム部
    Private Const CONST_ORGCODE_OIL As String = "010007"            '組織コード_石油部

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
        Master.MAPID = OIS0001WRKINC.MAPIDS

        WF_CAMPCODE_CODE.Focus()
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

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then         'メニューからの画面遷移
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE_CODE.Text)       '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD_CODE.Text)             '有効年月日(From)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD_CODE.Text)           '有効年月日(To)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ORG", WF_ORG_CODE.Text)                 '組織コード
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIS0001L Then   '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE_CODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
            WF_STYMD_CODE.Text = work.WF_SEL_STYMD.Text              '有効年月日(From)
            WF_ENDYMD_CODE.Text = work.WF_SEL_ENDYMD.Text            '有効年月日(To)
            WF_ORG_CODE.Text = work.WF_SEL_ORG.Text                  '組織コード
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIS0001WRKINC.MAPIDS
        rightview.MAPID = OIS0001WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE_CODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE_CODE.Text, WF_CAMPCODE_NAME.Text, WW_DUMMY)         '会社コード
        CODENAME_get("ORG", WF_ORG_CODE.Text, WF_ORG_NAME.Text, WW_DUMMY)                        '組織コード

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE_CODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_STYMD_CODE.Text)             '有効年月日(From)
        Master.EraseCharToIgnore(WF_ENDYMD_CODE.Text)            '有効年月日(To)
        Master.EraseCharToIgnore(WF_ORG_CODE.Text)               '組織コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE_CODE.Text        '会社コード
        work.WF_SEL_STYMD.Text = WF_STYMD_CODE.Text              '有効年月日(From)
        If WF_ENDYMD_CODE.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD_CODE.Text         '有効年月日(From) → 有効年月日(To)
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD_CODE.Text        '有効年月日(To)
        End If
        work.WF_SEL_ORG.Text = WF_ORG_CODE.Text                  '組織コード

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(WF_CAMPCODE_CODE.Text)
        End If

        Master.CheckParmissionCode(WF_CAMPCODE_CODE.Text)
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

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE_CODE.Text, "CAMPCODE", WF_CAMPCODE_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE_CODE.Text, WF_CAMPCODE_NAME.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE_CODE.Text)
                WF_CAMPCODE_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '有効年月日(From)
        Master.CheckField(WF_CAMPCODE_CODE.Text, "STYMD", WF_STYMD_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(From) : " & WF_STYMD_CODE.Text)
            WF_STYMD_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '有効年月日(To)
        Master.CheckField(WF_CAMPCODE_CODE.Text, "ENDYMD", WF_ENDYMD_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "有効年月日(To) : " & WF_ENDYMD_CODE.Text)
            WF_ENDYMD_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '日付大小チェック
        If WF_STYMD_CODE.Text <> "" AndAlso WF_ENDYMD_CODE.Text <> "" Then
            Dim WW_DATE_ST As Date
            Dim WW_DATE_END As Date
            Try
                Date.TryParse(WF_STYMD_CODE.Text, WW_DATE_ST)
                Date.TryParse(WF_ENDYMD_CODE.Text, WW_DATE_END)

                If WW_DATE_ST > WW_DATE_END Then
                    Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                    WF_STYMD_CODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, WF_STYMD_CODE.Text & ":" & WF_ENDYMD_CODE.Text)
                WF_STYMD_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '組織コード
        WW_TEXT = WF_ORG_CODE.Text
        Master.CheckField(WF_CAMPCODE_CODE.Text, "ORG", WF_ORG_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_ORG_CODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("ORG", WF_ORG_CODE.Text, WF_ORG_NAME.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "組織コード : " & WF_ORG_CODE.Text)
                    WF_ORG_CODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ORG_CODE.Focus()
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
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD_CODE.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD_CODE.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        'フィールドによってパラメータを変える
                        Dim prmData As New Hashtable

                        Select Case WF_FIELD.Value
                            Case "WF_CAMPCODE"       '会社コード
                                If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                                Else
                                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ROLE
                                End If
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE_CODE.Text

                            Case "WF_ORG"       '組織コード
                                Dim AUTHORITYALL_FLG As String = "0"
                                If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                    If WF_CAMPCODE_CODE.Text = "" Then '会社コードが空の場合
                                        AUTHORITYALL_FLG = "1"
                                    Else '会社コードに入力済みの場合
                                        AUTHORITYALL_FLG = "2"
                                    End If
                                End If
                                prmData = work.CreateORGParam(WF_CAMPCODE_CODE.Text, AUTHORITYALL_FLG)
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
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE_CODE.Text, WF_CAMPCODE_NAME.Text, WW_RTN_SW)
            Case "WF_ORG"               '組織コード
                CODENAME_get("ORG", WF_ORG_CODE.Text, WF_ORG_NAME.Text, WW_RTN_SW)
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
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE_CODE.Text = WW_SelectValue
                WF_CAMPCODE_NAME.Text = WW_SelectText
                WF_CAMPCODE_CODE.Focus()

            Case "WF_STYMD"             '有効年月日(From)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD_CODE.Text = ""
                    Else
                        WF_STYMD_CODE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD_CODE.Focus()

            Case "WF_ENDYMD"            '有効年月日(To)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD_CODE.Text = ""
                    Else
                        WF_ENDYMD_CODE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD_CODE.Focus()

            Case "WF_ORG"               '組織コード
                WF_ORG_CODE.Text = WW_SelectValue
                WF_ORG_NAME.Text = WW_SelectText
                WF_ORG_CODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""  '★

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE_CODE.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD_CODE.Focus()
            Case "WF_ENDYMD"            '有効年月日(To)
                WF_ENDYMD_CODE.Focus()
            Case "WF_ORG"               '組織コード
                WF_ORG_CODE.Focus()
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

        rightview.InitViewID(WF_CAMPCODE_CODE.Text, WW_DUMMY)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE_CODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"              '組織コード
                    prmData = work.CreateORGParam(WF_CAMPCODE_CODE.Text, 0)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

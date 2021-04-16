''************************************************************
' 品種マスタメンテ検索画面
' 作成日 2020/11/09
' 更新日 2021/01/26
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/11/09 新規作成
'         :2021/01/26 営業所コード選択範囲について修正
''************************************************************
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 品種マスタ登録（検索）
''' </summary>
''' <remarks></remarks>
Public Class OIM0003ProductSearch
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

        Master.MAPID = OIM0003WRKINC.MAPIDS

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
        Dim WW_TEXT As String = ""

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_OFFICECODE.Text, "OFFICECODE", WF_OFFICECODE.Text)              '営業所コード
            Master.GetFirstValue(work.WF_SEL_SHIPPERCODE.Text, "SHIPPERCODE", WF_SHIPPERCODE.Text)           '荷主コード
            Master.GetFirstValue(work.WF_SEL_PLANTCODE.Text, "PLANTCODE", WF_PLANTCODE.Text)                 '基地コード
            Master.GetFirstValue(work.WF_SEL_BIGOILCODE.Text, "BIGOILCODE", WF_BIGOILCODE.Text)              '油種大分類コード
            Master.GetFirstValue(work.WF_SEL_MIDDLEOILCODE.Text, "MIDDLEOILCODE", WF_MIDDLEOILCODE.Text)     '油種中分類コード
            Master.GetFirstValue(work.WF_SEL_OILCODE.Text, "OILCODE", WF_OILCODE.Text)                       '油種コード
            Master.GetFirstValue(work.WF_SEL_DELFLG.Text, "DELFLG", WW_TEXT)                                 '削除フラグ
            If WW_TEXT = C_DELETE_FLG.DELETE Then
                WF_DELFLG_DELETED.Checked = True
                WF_DELFLG_NOTDELETED.Checked = False
            ElseIf WW_TEXT <> C_DELETE_FLG.DELETE Then
                WF_DELFLG_DELETED.Checked = False
                WF_DELFLG_NOTDELETED.Checked = True
            Else
                WF_DELFLG_DELETED.Checked = False
                WF_DELFLG_NOTDELETED.Checked = True
            End If

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0003L Then   '実行画面からの遷移
            '〇画面項目設定処理
            WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE.Text             '営業所コード
            WF_SHIPPERCODE.Text = work.WF_SEL_SHIPPERCODE.Text           '荷主コード
            WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE.Text               '基地コード
            WF_BIGOILCODE.Text = work.WF_SEL_BIGOILCODE.Text             '油種大分類コード
            WF_MIDDLEOILCODE.Text = work.WF_SEL_MIDDLEOILCODE.Text       '油種中分類コード
            WF_OILCODE.Text = work.WF_SEL_OILCODE.Text                   '油種コード
            If work.WF_SEL_DELFLG.Text = C_DELETE_FLG.DELETE Then        '削除フラグ
                WF_DELFLG_NOTDELETED.Checked = False
                WF_DELFLG_DELETED.Checked = True
            Else
                WF_DELFLG_NOTDELETED.Checked = True
                WF_DELFLG_DELETED.Checked = False
            End If
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0003WRKINC.MAPIDS
        rightview.MAPID = OIM0003WRKINC.MAPIDL
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)                  '営業所コード
        CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)               '荷主コード
        CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE.Text, WW_RTN_SW)                          '基地コード
        CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)                  '油種大分類コード
        CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)         '油種中分類コード
        CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)                           '油種コード

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_OFFICECODE.Text)                '営業所コード
        Master.EraseCharToIgnore(WF_SHIPPERCODE.Text)               '荷主コード
        Master.EraseCharToIgnore(WF_PLANTCODE.Text)                 '基地コード
        Master.EraseCharToIgnore(WF_BIGOILCODE.Text)                '油種大分類コード
        Master.EraseCharToIgnore(WF_MIDDLEOILCODE.Text)             '油種中分類コード
        Master.EraseCharToIgnore(WF_OILCODE.Text)                   '油種コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_OFFICECODE.Text = WF_OFFICECODE.Text         '営業所コード
        work.WF_SEL_SHIPPERCODE.Text = WF_SHIPPERCODE.Text       '荷主コード
        work.WF_SEL_PLANTCODE.Text = WF_PLANTCODE.Text           '基地コード
        work.WF_SEL_BIGOILCODE.Text = WF_BIGOILCODE.Text         '油種大分類コード
        work.WF_SEL_MIDDLEOILCODE.Text = WF_MIDDLEOILCODE.Text   '油種中分類コード
        work.WF_SEL_OILCODE.Text = WF_OILCODE.Text               '油種コード

        If WF_DELFLG_NOTDELETED.Checked = True Then                 '削除フラグ
            work.WF_SEL_DELFLG.Text = C_DELETE_FLG.ALIVE             '削除除く
        ElseIf WF_DELFLG_DELETED.Checked = True Then
            work.WF_SEL_DELFLG.Text = C_DELETE_FLG.DELETE            '削除のみ
        Else
            work.WF_SEL_DELFLG.Text = ""                             '指定なし（暫定）
        End If

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
        '営業所コード
        WW_TEXT = WF_OFFICECODE.Text
        Master.CheckField(Master.USERCAMP, "OFFICECODE", WF_OFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_OFFICECODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "営業所コード : " & WF_OFFICECODE.Text, needsPopUp:=True)
                    WF_OFFICECODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所コード", needsPopUp:=True)
            WF_OFFICECODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '荷主コード
        WW_TEXT = WF_SHIPPERCODE.Text
        Master.CheckField(Master.USERCAMP, "SHIPPERCODE", WF_SHIPPERCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_SHIPPERCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "荷主コード : " & WF_SHIPPERCODE.Text, needsPopUp:=True)
                    WF_SHIPPERCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "荷主コード", needsPopUp:=True)
            WF_SHIPPERCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '基地コード
        WW_TEXT = WF_PLANTCODE.Text
        Master.CheckField(Master.USERCAMP, "PLANTCODE", WF_PLANTCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_PLANTCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "基地コード : " & WF_SHIPPERCODE.Text, needsPopUp:=True)
                    WF_PLANTCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "基地コード", needsPopUp:=True)
            WF_PLANTCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '油種大分類コード
        WW_TEXT = WF_BIGOILCODE.Text
        Master.CheckField(Master.USERCAMP, "BIGOILCODE", WF_BIGOILCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_BIGOILCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "油種大分類コード : " & WF_BIGOILCODE.Text, needsPopUp:=True)
                    WF_BIGOILCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "油種大分類コード", needsPopUp:=True)
            WF_BIGOILCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '油種中分類コード
        WW_TEXT = WF_MIDDLEOILCODE.Text
        Master.CheckField(Master.USERCAMP, "MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_MIDDLEOILCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "油種中分類コード : " & WF_MIDDLEOILCODE.Text, needsPopUp:=True)
                    WF_MIDDLEOILCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "油種中分類コード", needsPopUp:=True)
            WF_MIDDLEOILCODE.Focus()
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
                Select Case WF_FIELD.Value
                    Case WF_OFFICECODE.ID
                        '営業所コード
                        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG)
                    Case WF_SHIPPERCODE.ID
                        '荷主コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                    Case WF_PLANTCODE.ID
                        '基地コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    Case WF_BIGOILCODE.ID
                        '油種大分類コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    Case WF_MIDDLEOILCODE.ID
                        '油種中分類コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                    Case WF_OILCODE.ID
                        '油種コード
                        prmData = work.CreateFIXParam(Master.USERCAMP, "OILCODE")
                End Select

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
            Case WF_OFFICECODE.ID
                '営業所コード
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
            Case WF_SHIPPERCODE.ID
                '荷主コード
                CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)
            Case WF_PLANTCODE.ID
                '基地コード
                CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_MIDDLEOILCODE.ID
                '油種中分類コード
                CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE_TEXT.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_OILCODE.ID
                '油種コード
                CODENAME_get("OILCODE", WF_OILCODE.Text, WF_OILCODE_TEXT.Text, WW_RTN_SW)
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
                '営業所コード
                WF_OFFICECODE.Text = WW_SelectValue
                WF_OFFICECODE_TEXT.Text = WW_SelectText
                WF_OFFICECODE.Focus()
            Case WF_SHIPPERCODE.ID
                '荷主コード
                WF_SHIPPERCODE.Text = WW_SelectValue
                WF_SHIPPERCODE_TEXT.Text = WW_SelectText
                WF_SHIPPERCODE.Focus()
            Case WF_PLANTCODE.ID
                '基地コード
                WF_PLANTCODE.Text = WW_SelectValue
                WF_PLANTCODE_TEXT.Text = WW_SelectText
                WF_PLANTCODE.Focus()
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                WF_BIGOILCODE.Text = WW_SelectValue
                WF_BIGOILCODE_TEXT.Text = WW_SelectText
                WF_BIGOILCODE.Focus()
            Case WF_MIDDLEOILCODE.ID
                '油種中分類コード
                WF_MIDDLEOILCODE.Text = WW_SelectValue
                WF_MIDDLEOILCODE_TEXT.Text = WW_SelectText
                WF_MIDDLEOILCODE.Focus()
            Case WF_OILCODE.ID
                '油種コード
                WF_OILCODE.Text = WW_SelectValue
                WF_OILCODE_TEXT.Text = WW_SelectText
                WF_OILCODE.Focus()
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
                '営業所コード
                WF_OFFICECODE.Focus()
            Case WF_SHIPPERCODE.ID
                '荷主コード
                WF_SHIPPERCODE.Focus()
            Case WF_PLANTCODE.ID
                '基地コード
                WF_PLANTCODE.Focus()
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                WF_BIGOILCODE.Focus()
            Case WF_MIDDLEOILCODE.ID
                '油種中分類コード
                WF_MIDDLEOILCODE.Focus()
            Case WF_OILCODE.ID
                '油種コード
                WF_OILCODE.Focus()
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
                    '営業所コード
                    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERCODE"
                    '荷主コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    '基地コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"
                    '油種大分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"
                    '油種中分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTOILCODE"
                    'OT油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
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

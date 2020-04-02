''************************************************************
' 貨車連結順序表テーブル検索画面
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
''' 貨車連結順序表テーブル登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class OIT0002LinkSearch
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
        Master.MAPID = OIT0002WRKINC.MAPIDS

        WF_CAMPCODE.Focus()
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
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)       '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ORG", WF_ORG.Text)                '組織コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "RETSTATION", WF_RETSTATION_CODE.Text)   '空車着駅
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD_CODE.Text)             '有効年月日(From)
            WF_STYMD_CODE.Text = Format(CDate(WF_STYMD_CODE.Text).AddDays(0), "yyyy/MM/dd")
            'Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD_CODE.Text)           '有効年月日(To)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WF_TRAINNO_CODE.Text)         '本線列車

            'ステータス選択
            WF_SW1.Checked = True
            WF_SW2.Checked = False
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002L Then   '一覧画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text        '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text        　　　　　'組織コード
            WF_RETSTATION_CODE.Text = work.WF_SEL_RETSTATION.Text    '空車着駅
            WF_STYMD_CODE.Text = work.WF_SEL_STYMD.Text              '有効年月日(From)
            'WF_ENDYMD_CODE.Text = work.WF_SEL_ENDYMD.Text            '有効年月日(To)
            WF_TRAINNO_CODE.Text = work.WF_SEL_TRAINNO.Text          '本線列車

            'ステータス選択
            If work.WF_SEL_SELECT.Text = "1" Then
                WF_SW1.Checked = True
                WF_SW2.Checked = False
            Else
                WF_SW1.Checked = False
                WF_SW2.Checked = True
            End If
        End If

        '空車着駅（発駅）・列車番号を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_RETSTATION_CODE.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_TRAINNO_CODE.Attributes("onkeyPress") = "CheckNum()"

        '利用可能日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_STYMD_CODE.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0002WRKINC.MAPIDS
        rightview.MAPID = OIT0002WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)         '会社コード
        CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)         　　　　　　　 '組織コード
        CODENAME_get("RETSTATION", WF_RETSTATION_CODE.Text, WF_RETSTATION_NAME.Text, WW_DUMMY)   '空車発駅
        CODENAME_get("TRAINNO", WF_TRAINNO_CODE.Text, WF_TRAINNO_NAME.Text, WW_DUMMY)         　 '本線列車

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_RETSTATION_CODE.Text)        '空着発駅
        Master.EraseCharToIgnore(WF_STYMD_CODE.Text)             '有効年月日(From)
        'Master.EraseCharToIgnore(WF_ENDYMD_CODE.Text)            '有効年月日(To)
        Master.EraseCharToIgnore(WF_TRAINNO_CODE.Text)           '本線列車

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text        　　　　　'組織コード
        work.WF_SEL_RETSTATION.Text = WF_RETSTATION_CODE.Text    '空着着駅
        work.WF_SEL_STYMD.Text = WF_STYMD_CODE.Text              '有効年月日(From)
        'work.WF_SEL_ENDYMD.Text = WF_ENDYMD_CODE.Text      　'有効年月日(To)
        work.WF_SEL_TRAINNO.Text = WF_TRAINNO_CODE.Text    　　　'本線列車
        If WF_SW1.Checked = True Then
            work.WF_SEL_SELECT.Text = "1"                   '利用可のみ表示
        End If
        If WF_SW2.Checked = True Then
            work.WF_SEL_SELECT.Text = "0"                   '全て表示
        End If

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
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
        Dim dateErrFlag As String = ""
        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_LINE_ERR As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text, needsPopUp:=True)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '空車着駅（発駅）
        If WF_RETSTATION_CODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "空車着駅", needsPopUp:=True)
            WF_RETSTATION_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(WF_CAMPCODE.Text, "RETSTATION", WF_RETSTATION_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("RETSTATION", WF_RETSTATION_CODE.Text, WF_RETSTATION_NAME.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "空車着駅 : " & WF_RETSTATION_CODE.Text, needsPopUp:=True)
                WF_RETSTATION_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            WF_RETSTATION_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '開始日
        '存在チェック
        If WF_STYMD_CODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "利用可能日", needsPopUp:=True)
            WF_STYMD_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        WW_CheckDate(WF_STYMD_CODE.Text, "利用可能日（開始）", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            WF_STYMD_CODE.Focus()
            WW_CheckMES1 = "利用可能日（開始）入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            WF_STYMD_CODE.Text = CDate(WF_STYMD_CODE.Text).ToString("yyyy/MM/dd")
        End If

        '本線列車
        If WF_TRAINNO_CODE.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", WF_TRAINNO_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WF_TRAINNO_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If
        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

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
                        End Select
                        .ActiveCalendar()
                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_RETSTATION"       '空車着駅（発駅）
                                prmData = work.CreateSTATIONPTParam(WF_CAMPCODE.Text, "STATIONPATTERN", "1")
                            Case "WF_TRAINNO"          '本線列車
                                prmData = work.CreateFIXParam(WF_ORG.Text, "TRAINNUMBER")
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
            Case "WF_RETSTATION"          '空車着駅（発駅）
                CODENAME_get("RETSTATION", WF_RETSTATION_CODE.Text, WF_RETSTATION_NAME.Text, WW_RTN_SW)
            Case "WF_TRAINNO"          '本線列車
                CODENAME_get("TRAINNO", WF_TRAINNO_CODE.Text, WF_TRAINNO_NAME.Text, WW_RTN_SW)
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
            Case "WF_RETSTATION"        '空着着駅（発駅）
                WF_RETSTATION_CODE.Text = WW_SelectValue
                WF_RETSTATION_NAME.Text = WW_SelectText
                WF_RETSTATION_CODE.Focus()

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

            Case "WF_TRAINNO"           '本線列車
                WF_TRAINNO_CODE.Text = WW_SelectValue
                WF_TRAINNO_NAME.Text = WW_SelectText
                WF_TRAINNO_CODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        'WF_LeftMViewChange.Value = ""  '★

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_RETSTATION"        '空車着駅（発駅）
                WF_RETSTATION_CODE.Focus()
            Case "WF_STYMD"             '有効年月日(From)
                WF_STYMD_CODE.Focus()
            'Case "WF_ENDYMD"            '有効年月日(To)
            '    WF_ENDYMD_CODE.Focus()
            Case "WF_TRAINNO"           '本線列車
                WF_TRAINNO_CODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        'WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"             '組織コード
                    prmData = work.CreateORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "RETSTATION"       '空車着駅（発駅）
                    prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "STATIONPATTERN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

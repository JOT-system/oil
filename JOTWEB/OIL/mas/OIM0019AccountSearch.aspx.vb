''************************************************************
' 勘定科目マスタメンテ検索画面
' 作成日 2021/01/25
' 更新日 
' 作成者 JOT伊草
' 更新者 
'
' 修正履歴:2021/01/25 新規作成
''************************************************************
Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0019AccountSearch
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
        Master.MAPID = OIM0019WRKINC.MAPIDS

        WF_FROMYMD.Focus()
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
            Master.GetFirstValue(work.WF_SEL_FROMYMD.Text, "FROMYMD", WF_FROMYMD.Text)                                  '適用開始年月日
            Master.GetFirstValue(work.WF_SEL_ENDYMD.Text, "ENDYMD", WF_ENDYMD.Text)                                     '適用終了年月日
            Master.GetFirstValue(work.WF_SEL_ACCOUNTCODE.Text, "ACCOUNTCODE", WF_ACCOUNTCODE.Text)                      '科目コード
            Master.GetFirstValue(work.WF_SEL_SEGMENTCODE.Text, "SEGMENTCODE", WF_SEGMENTCODE.Text)                      'セグメント
            Master.GetFirstValue(work.WF_SEL_SEGMENTBRANCHCODE.Text, "SEGMENTBRANCHCODE", WF_SEGMENTBRANCHCODE.Text)    'セグメント枝番
            Master.GetFirstValue(work.WF_SEL_ACCOUNTTYPE.Text, "ACCOUNTTYPE", WF_ACCOUNTTYPE.Text)                      '科目区分
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0019L Then   '実行画面からの遷移
            '画面項目設定処理
            WF_FROMYMD.Text = work.WF_SEL_FROMYMD.Text                      '適用開始年月日
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text                        '適用終了年月日
            WF_ACCOUNTCODE.Text = work.WF_SEL_ACCOUNTCODE.Text              '科目コード
            WF_SEGMENTCODE.Text = work.WF_SEL_SEGMENTCODE.Text              'セグメント
            WF_SEGMENTBRANCHCODE.Text = work.WF_SEL_SEGMENTBRANCHCODE.Text  'セグメント枝番
            WF_ACCOUNTTYPE.Text = work.WF_SEL_ACCOUNTTYPE.Text              '科目区分
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0019WRKINC.MAPIDS
        rightview.MAPID = OIM0019WRKINC.MAPIDL
        rightview.COMPCODE = Master.USERCAMP
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_DUMMY)    '科目コード
        CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_DUMMY)    'セグメント
        CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_DUMMY)    '科目区分

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_FROMYMD.Text)           '適用開始年月日
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '適用終了年月日
        Master.EraseCharToIgnore(WF_ACCOUNTCODE.Text)       '科目コード
        Master.EraseCharToIgnore(WF_SEGMENTCODE.Text)       'セグメント
        Master.EraseCharToIgnore(WF_SEGMENTBRANCHCODE.Text) 'セグメント枝番
        Master.EraseCharToIgnore(WF_ACCOUNTTYPE.Text)       '科目区分

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_FROMYMD.Text = WF_FROMYMD.Text                      '適用開始年月日
        work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text                        '適用終了年月日
        work.WF_SEL_ACCOUNTCODE.Text = WF_ACCOUNTCODE.Text              '科目コード
        work.WF_SEL_SEGMENTCODE.Text = WF_SEGMENTCODE.Text              'セグメント
        work.WF_SEL_SEGMENTBRANCHCODE.Text = WF_SEGMENTBRANCHCODE.Text  'セグメント枝番
        work.WF_SEL_ACCOUNTTYPE.Text = WF_ACCOUNTTYPE.Text              '科目区分

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
        '適用開始年月日
        Master.CheckField(Master.USERCAMP, "FROMYMD", WF_FROMYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_FROMYMD.Text <> "" Then
                '年月日チェック
                WW_CheckDate(WF_FROMYMD.Text, "適用開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WF_FROMYMD.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    WF_FROMYMD.Text = CDate(WF_FROMYMD.Text).ToString("yyyy/MM/dd")
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "適用開始年月日", needsPopUp:=True)
            WF_FROMYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '適用終了年月日
        Master.CheckField(Master.USERCAMP, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_ENDYMD.Text <> "" Then
                '年月日チェック
                WW_CheckDate(WF_ENDYMD.Text, "適用終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WF_ENDYMD.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    WF_ENDYMD.Text = CDate(WF_ENDYMD.Text).ToString("yyyy/MM/dd")
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "適用終了年月日", needsPopUp:=True)
            WF_ENDYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '科目コード
        WW_TEXT = WF_ACCOUNTCODE.Text
        Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", WF_ACCOUNTCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_ACCOUNTCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "科目コード : " & WF_ACCOUNTCODE.Text, needsPopUp:=True)
                    WF_ACCOUNTCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "科目コード", needsPopUp:=True)
            WF_ACCOUNTCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        'セグメント
        WW_TEXT = WF_SEGMENTCODE.Text
        Master.CheckField(Master.USERCAMP, "SEGMENTCODE", WF_SEGMENTCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_SEGMENTCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "セグメント : " & WF_SEGMENTCODE.Text, needsPopUp:=True)
                    WF_SEGMENTCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "セグメント", needsPopUp:=True)
            WF_SEGMENTCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        'セグメント枝番
        WW_TEXT = WF_SEGMENTBRANCHCODE.Text
        Master.CheckField(Master.USERCAMP, "SEGMENTBRANCHCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_SEGMENTBRANCHCODE.Text = ""
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "セグメント枝番", needsPopUp:=True)
            WF_SEGMENTBRANCHCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '科目区分
        WW_TEXT = WF_ACCOUNTTYPE.Text
        Master.CheckField(Master.USERCAMP, "ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If String.IsNullOrEmpty(WW_TEXT) Then
                WF_ACCOUNTTYPE.Text = ""
            Else
                '存在チェック
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "科目区分 : " & WF_ACCOUNTTYPE.Text, needsPopUp:=True)
                    WF_ACCOUNTTYPE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "科目区分", needsPopUp:=True)
            WF_ACCOUNTTYPE.Focus()
            O_RTN = "ERR"
            Exit Sub
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

                    Case Else

                        Dim prmData As New Hashtable

                        Select Case WF_FIELD.Value
                            Case WF_ACCOUNTCODE.ID
                                '科目コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTCODE")
                            Case WF_SEGMENTCODE.ID
                                'セグメント
                                prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTSEGMENTCODE")
                            Case WF_ACCOUNTTYPE.ID
                                '科目区分
                                prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTTYPE")
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
            Case WF_ACCOUNTCODE.ID
                '関連項目
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)
                '科目コード
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)

            Case WF_SEGMENTCODE.ID
                '関連項目
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)
                'セグメント
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)

            Case WF_ACCOUNTTYPE.ID
                '関連項目
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)
                '科目区分
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)

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
            Case WF_ACCOUNTCODE.ID
                '関連項目処理
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)  'セグメント
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)  '科目区分

                '科目コード
                WF_ACCOUNTCODE.Text = WW_SelectValue
                WF_ACCOUNTCODE_TEXT.Text = WW_SelectText
                WF_ACCOUNTCODE.Focus()

            Case WF_SEGMENTCODE.ID
                '関連項目処理
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)  '科目コード
                CODENAME_get("ACCOUNTTYPE", WF_ACCOUNTTYPE.Text, WF_ACCOUNTTYPE_TEXT.Text, WW_RTN_SW)  '科目区分

                'セグメント
                WF_SEGMENTCODE.Text = WW_SelectValue
                WF_SEGMENTCODE_TEXT.Text = WW_SelectText
                WF_SEGMENTCODE.Focus()

            Case WF_ACCOUNTTYPE.ID
                '関連項目処理
                CODENAME_get("ACCOUNTCODE", WF_ACCOUNTCODE.Text, WF_ACCOUNTCODE_TEXT.Text, WW_RTN_SW)  '科目コード
                CODENAME_get("SEGMENTCODE", WF_SEGMENTCODE.Text, WF_SEGMENTCODE_TEXT.Text, WW_RTN_SW)  'セグメント

                '科目区分
                WF_ACCOUNTTYPE.Text = WW_SelectValue
                WF_ACCOUNTTYPE_TEXT.Text = WW_SelectText
                WF_ACCOUNTTYPE.Focus()

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
            Case WF_ACCOUNTCODE.ID
                '科目コード
                WF_ACCOUNTCODE.Focus()

            Case WF_SEGMENTCODE.ID
                'セグメント
                WF_SEGMENTCODE.Focus()

            Case WF_ACCOUNTTYPE.ID
                '科目区分
                WF_ACCOUNTTYPE.Focus()
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
                Case "ACCOUNTCODE"
                    '科目コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SEGMENTCODE"
                    'セグメント
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTSEGMENTCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ACCOUNTTYPE"
                    '科目区分
                    prmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTTYPE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
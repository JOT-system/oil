Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 届先マスタ入力（JX,COSMO）（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMC0006SELECT_JC
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
            '○ 初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '画面ID設定
        Master.MAPID = GRMC0006WRKINC.MAPIDS_JC

        '○初期値設定
        WF_STYMD.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.EraseCharToIgnore(WF_TODOKECODE.Text)        '届先コード
        Master.EraseCharToIgnore(WF_TODOKENAME.Text)        '届先名称
        Master.EraseCharToIgnore(WF_POSTNUM.Text)           '郵便番号
        Master.EraseCharToIgnore(WF_ADDR.Text)              '住所
        Master.EraseCharToIgnore(WF_TEL.Text)               '電話番号
        Master.EraseCharToIgnore(WF_FAX.Text)               'FAX番号
        Master.EraseCharToIgnore(WF_CITIES.Text)            '市町村コード
        Master.EraseCharToIgnore(WF_CLASS.Text)             '分類
        Master.EraseCharToIgnore(WF_PREFECTURES1.Text)      '都道府県１
        Master.EraseCharToIgnore(WF_PREFECTURES2.Text)      '都道府県２
        Master.EraseCharToIgnore(WF_PREFECTURES3.Text)      '都道府県３
        Master.EraseCharToIgnore(WF_PREFECTURES4.Text)      '都道府県４
        Master.EraseCharToIgnore(WF_PREFECTURES5.Text)      '都道府県５

        '○チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○セッション変数　反映
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text            '会社コード
        work.WF_SEL_STYMD.Text = WF_STYMD.Text                  '有効年月日
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If

        work.WF_SEL_TORIRADIO.Text = Me.rblToriCode.SelectedValue   '取引先コード

        work.WF_SEL_TODOKECODE.Text = WF_TODOKECODE.Text        '届先コード
        work.WF_SEL_TODOKENAME.Text = WF_TODOKENAME.Text        '届先名称
        work.WF_SEL_POSTNUM.Text = WF_POSTNUM.Text              '郵便番号
        work.WF_SEL_ADDR.Text = WF_ADDR.Text                    '住所
        work.WF_SEL_TEL.Text = WF_TEL.Text                      '電話番号
        work.WF_SEL_FAX.Text = WF_FAX.Text                      'FAX番号
        work.WF_SEL_CITIES.Text = WF_CITIES.Text                '市町村コード
        work.WF_SEL_CLASS.Text = WF_CLASS.Text                  '分類
        work.WF_SEL_PREFECTURES1.Text = WF_PREFECTURES1.Text    '都道府県１
        work.WF_SEL_PREFECTURES2.Text = WF_PREFECTURES2.Text    '都道府県２
        work.WF_SEL_PREFECTURES3.Text = WF_PREFECTURES3.Text    '都道府県３
        work.WF_SEL_PREFECTURES4.Text = WF_PREFECTURES4.Text    '都道府県４
        work.WF_SEL_PREFECTURES5.Text = WF_PREFECTURES5.Text    '都道府県５

        Master.VIEWID = rightview.GetViewId(WF_CAMPCODE.Text)

        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.TransitionPage()
        End If

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
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
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                    'フィールドによってパラメーターを変える
                    Select Case WF_FIELD.Value
                        Case "WF_TORICODEF", "WF_TORICODET"         '取引先
                            prmData = work.CreateTORIParam(WF_CAMPCODE.Text)
                        Case "WF_TODOKECODE"                        '届先コード
                            prmData = work.CreateTODOKEJCParam(WF_CAMPCODE.Text, Me.rblToriCode.SelectedValue)
                        Case "WF_CITIES"                            '市町村コード
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "CITIES")
                        Case "WF_CLASS"                             '分類
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "CLASS")
                        Case "WF_PREFECTURES1", "WF_PREFECTURES2", "WF_PREFECTURES3",
                             "WF_PREFECTURES4", "WF_PREFECTURES5"   '都道府県
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES")
                    End Select

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_ENDYMD.Text
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If

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
        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '○右Boxメモ変更時処理
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
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
                WF_CAMPCODE_Text.Text = WW_SelectTEXT
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

            Case "WF_TODOKECODE"        '届先コード
                WF_TODOKECODE_Text.Text = WW_SelectTEXT
                WF_TODOKECODE.Text = WW_SelectValue
                WF_TODOKECODE.Focus()

            Case "WF_CITIES"            '市町村コード
                WF_CITIES_Text.Text = WW_SelectTEXT
                WF_CITIES.Text = WW_SelectValue
                WF_CITIES.Focus()

            Case "WF_CLASS"             '分類
                WF_CLASS_Text.Text = WW_SelectTEXT
                WF_CLASS.Text = WW_SelectValue
                WF_CLASS.Focus()

            Case "WF_PREFECTURES1"             '都道府県１
                WF_PREFECTURES1_Text.Text = WW_SelectTEXT
                WF_PREFECTURES1.Text = WW_SelectValue
                WF_PREFECTURES1.Focus()

            Case "WF_PREFECTURES2"             '都道府県２
                WF_PREFECTURES2_Text.Text = WW_SelectTEXT
                WF_PREFECTURES2.Text = WW_SelectValue
                WF_PREFECTURES2.Focus()

            Case "WF_PREFECTURES3"             '都道府県３
                WF_PREFECTURES3_Text.Text = WW_SelectTEXT
                WF_PREFECTURES3.Text = WW_SelectValue
                WF_PREFECTURES3.Focus()

            Case "WF_PREFECTURES4"             '都道府県４
                WF_PREFECTURES4_Text.Text = WW_SelectTEXT
                WF_PREFECTURES4.Text = WW_SelectValue
                WF_PREFECTURES4.Focus()

            Case "WF_PREFECTURES5"             '都道府県５
                WF_PREFECTURES5_Text.Text = WW_SelectTEXT
                WF_PREFECTURES5.Text = WW_SelectValue
                WF_PREFECTURES5.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
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
            Case "WF_TODOKECODE"        '届先コード
                WF_TODOKECODE.Focus()
            Case "WF_CITIES"            '市町村コード
                WF_CITIES.Focus()
            Case "WF_CLASS"             '分類
                WF_CLASS.Focus()
            Case "WF_PREFECTURES1"       '都道府県１
                WF_PREFECTURES1.Focus()
            Case "WF_PREFECTURES2"       '都道府県２
                WF_PREFECTURES2.Focus()
            Case "WF_PREFECTURES3"       '都道府県３
                WF_PREFECTURES3.Focus()
            Case "WF_PREFECTURES4"       '都道府県４
                WF_PREFECTURES4.Focus()
            Case "WF_PREFECTURES5"       '都道府県５
                WF_PREFECTURES5.Focus()

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

        WF_CAMPCODE_Text.Text = String.Empty            '会社
        WF_TODOKECODE_Text.Text = String.Empty          '届先
        WF_CITIES_Text.Text = String.Empty              '市町村
        WF_CLASS_Text.Text = String.Empty               '分類
        WF_PREFECTURES1_Text.Text = String.Empty        '都道府県１
        WF_PREFECTURES2_Text.Text = String.Empty        '都道府県２
        WF_PREFECTURES3_Text.Text = String.Empty        '都道府県３
        WF_PREFECTURES4_Text.Text = String.Empty        '都道府県４
        WF_PREFECTURES5_Text.Text = String.Empty        '都道府県５

        '○入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(WF_STYMD.Text)             '有効年月日(From)
        Master.EraseCharToIgnore(WF_ENDYMD.Text)            '有効年月日(To)
        Master.EraseCharToIgnore(WF_TODOKECODE.Text)        '届先コード
        Master.EraseCharToIgnore(WF_TODOKENAME.Text)        '届先名称
        Master.EraseCharToIgnore(WF_POSTNUM.Text)           '郵便番号
        Master.EraseCharToIgnore(WF_ADDR.Text)              '住所
        Master.EraseCharToIgnore(WF_TEL.Text)               '電話番号
        Master.EraseCharToIgnore(WF_FAX.Text)               'FAX番号
        Master.EraseCharToIgnore(WF_CITIES.Text)            '市町村コード
        Master.EraseCharToIgnore(WF_CLASS.Text)             '分類
        Master.EraseCharToIgnore(WF_PREFECTURES1.Text)      '都道府県１
        Master.EraseCharToIgnore(WF_PREFECTURES2.Text)      '都道府県２
        Master.EraseCharToIgnore(WF_PREFECTURES3.Text)      '都道府県３
        Master.EraseCharToIgnore(WF_PREFECTURES4.Text)      '都道府県４
        Master.EraseCharToIgnore(WF_PREFECTURES5.Text)      '都道府県５

        '○チェック処理
        WW_Check(WW_DUMMY)

        '○名称設定処理
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                                                                                      '会社コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, WF_TODOKECODE.Text, WF_TODOKECODE_Text.Text, WW_DUMMY, work.CreateTODOKEJCParam(WF_CAMPCODE.Text, Me.rblToriCode.SelectedValue))    '届先コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CITIES.Text, WF_CITIES_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "CITIES"))                                       '市町村コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CLASS.Text, WF_CLASS_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "CLASS"))                                          '分類
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES1.Text, WF_PREFECTURES1_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県１
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES2.Text, WF_PREFECTURES2_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県２
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES3.Text, WF_PREFECTURES3_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県３
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES4.Text, WF_PREFECTURES4_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県４
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES5.Text, WF_PREFECTURES5_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県５

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then                   'メニューからの画面遷移
            '○初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)                   '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)                         '有効年月日(From)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)                       '有効年月日(To)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TORICODE", Me.rblToriCode.SelectedValue)       '取引先(JX,COSMO)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TODOKECODE", WF_TODOKECODE.Text)               '届先コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TODOKENAME", WF_TODOKENAME.Text)               '届先名称　
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "POSTNUM", WF_POSTNUM.Text)                     '郵便番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ADDR", WF_ADDR.Text)                           '住所
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TEL", WF_TEL.Text)                             '電話番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "FAX", WF_FAX.Text)                             'FAX番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CITIES", WF_CITIES.Text)                       '市町村コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CLASS", WF_CLASS.Text)                         '分類
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "PREFECTURES1", WF_PREFECTURES1.Text)           '都道府県１
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "PREFECTURES2", WF_PREFECTURES2.Text)           '都道府県２
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "PREFECTURES3", WF_PREFECTURES3.Text)           '都道府県３
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "PREFECTURES4", WF_PREFECTURES4.Text)           '都道府県４
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "PREFECTURES5", WF_PREFECTURES5.Text)           '都道府県５
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MC0006_JC Then          '実行画面からの画面遷移
            '○画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_STYMD.Text = work.WF_SEL_STYMD.Text                  '有効年月日(From)
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text                '有効年月日(To)

            If Me.rblToriCode.Items.FindByValue(work.WF_SEL_TORIRADIO.Text) IsNot Nothing Then
                Me.rblToriCode.SelectedValue = work.WF_SEL_TORIRADIO.Text
            End If

            WF_TODOKECODE.Text = work.WF_SEL_TODOKECODE.Text        '届先コード
            WF_TODOKENAME.Text = work.WF_SEL_TODOKENAME.Text        '届先名称
            WF_POSTNUM.Text = work.WF_SEL_POSTNUM.Text              '郵便番号
            WF_ADDR.Text = work.WF_SEL_ADDR.Text                    '住所
            WF_TEL.Text = work.WF_SEL_TEL.Text                      '電話番号
            WF_FAX.Text = work.WF_SEL_FAX.Text                      'FAX番号
            WF_CITIES.Text = work.WF_SEL_CITIES.Text                '市町村コード
            WF_CLASS.Text = work.WF_SEL_CLASS.Text                  '分類
            WF_PREFECTURES1.Text = work.WF_SEL_PREFECTURES1.Text    '都道府県１
            WF_PREFECTURES2.Text = work.WF_SEL_PREFECTURES2.Text    '都道府県２
            WF_PREFECTURES3.Text = work.WF_SEL_PREFECTURES3.Text    '都道府県３
            WF_PREFECTURES4.Text = work.WF_SEL_PREFECTURES4.Text    '都道府県４
            WF_PREFECTURES5.Text = work.WF_SEL_PREFECTURES5.Text    '都道府県５
        End If

        '○ RightBox情報設定
        rightview.MAPID = GRMC0006WRKINC.MAPID_JC
        rightview.MAPIDS = GRMC0006WRKINC.MAPIDS_JC
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("届先入力", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○名称設定処理
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                                                                                      '会社コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, WF_TODOKECODE.Text, WF_TODOKECODE_Text.Text, WW_DUMMY, work.CreateTODOKEJCParam(WF_CAMPCODE.Text, Me.rblToriCode.SelectedValue))    '届先コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CITIES.Text, WF_CITIES_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "CITIES"))                                       '市町村コード
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CLASS.Text, WF_CLASS_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "CLASS"))                                          '分類
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES1.Text, WF_PREFECTURES1_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県１
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES2.Text, WF_PREFECTURES2_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県２
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES3.Text, WF_PREFECTURES3_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県３
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES4.Text, WF_PREFECTURES4_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県４
        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES5.Text, WF_PREFECTURES5_Text.Text, WW_DUMMY, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))                      '都道府県５

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">成否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        '○初期設定
        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""
        Dim WW_STYMD As Date
        Dim WW_ENDYMD As Date

        '○会社コード
        WW_TEXT = WF_CAMPCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            If WW_TEXT = "" Then
                WF_CAMPCODE.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○有効年月日(From)
        Master.CheckField(WF_CAMPCODE.Text, "STYMD", WF_STYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_STYMD.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○有効年月日(To)
        If WF_ENDYMD.Text = Nothing Then
            WF_ENDYMD.Text = WF_STYMD.Text
        End If

        Master.CheckField(WF_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WF_ENDYMD.Text, WW_ENDYMD)
            Catch ex As Exception
                WW_ENDYMD = C_MAX_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ENDYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_STYMD.Text <> "" AndAlso WF_ENDYMD.Text <> "" Then
            If WW_STYMD > WW_ENDYMD Then
                Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                WF_STYMD.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○届先コード
        WW_TEXT = WF_TODOKECODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "TODOKECODE", WF_TODOKECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_TODOKECODE.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DISTINATION, WF_TODOKECODE.Text, WF_TODOKECODE_Text.Text, WW_RTN_SW, work.CreateTODOKEJCParam(WF_CAMPCODE.Text, Me.rblToriCode.SelectedValue))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_TODOKECODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TODOKECODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○届先名称
        Master.CheckField(WF_CAMPCODE.Text, "TODOKENAME", WF_TODOKENAME.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TODOKENAME.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○郵便番号
        Master.CheckField(WF_CAMPCODE.Text, "POSTNUM", WF_POSTNUM.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_POSTNUM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○住所
        Master.CheckField(WF_CAMPCODE.Text, "ADDR", WF_ADDR.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ADDR.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○電話番号
        Master.CheckField(WF_CAMPCODE.Text, "TEL", WF_TEL.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TEL.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○FAX番号
        Master.CheckField(WF_CAMPCODE.Text, "FAX", WF_FAX.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_FAX.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○市町村コード
        WW_TEXT = WF_CITIES.Text
        Master.CheckField(WF_CAMPCODE.Text, "CITIES", WF_CITIES.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_CITIES.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CITIES.Text, WF_CITIES_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "CITIES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CITIES.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CITIES.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○分類
        WW_TEXT = WF_CLASS.Text
        Master.CheckField(WF_CAMPCODE.Text, "CLASS", WF_CLASS.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_CLASS.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_CLASS.Text, WF_CLASS_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "CLASS"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CLASS.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CLASS.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○都道府県１
        WW_TEXT = WF_PREFECTURES1.Text
        Master.CheckField(WF_CAMPCODE.Text, "PREFECTURES1", WF_PREFECTURES1.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_PREFECTURES1.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES1.Text, WF_PREFECTURES1_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_PREFECTURES1.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_PREFECTURES1.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○都道府県２
        WW_TEXT = WF_PREFECTURES2.Text
        Master.CheckField(WF_CAMPCODE.Text, "PREFECTURES2", WF_PREFECTURES2.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_PREFECTURES2.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES2.Text, WF_PREFECTURES2_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_PREFECTURES2.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_PREFECTURES2.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○都道府県３
        WW_TEXT = WF_PREFECTURES3.Text
        Master.CheckField(WF_CAMPCODE.Text, "PREFECTURES3", WF_PREFECTURES3.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_PREFECTURES3.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES3.Text, WF_PREFECTURES3_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_PREFECTURES3.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_PREFECTURES3.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○都道府県４
        WW_TEXT = WF_PREFECTURES4.Text
        Master.CheckField(WF_CAMPCODE.Text, "PREFECTURES4", WF_PREFECTURES4.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_PREFECTURES4.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES4.Text, WF_PREFECTURES4_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_PREFECTURES4.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_PREFECTURES4.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○都道府県５
        WW_TEXT = WF_PREFECTURES5.Text
        Master.CheckField(WF_CAMPCODE.Text, "PREFECTURES5", WF_PREFECTURES5.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WW_TEXT = "" Then
                WF_PREFECTURES5.Text = ""
            Else
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, WF_PREFECTURES5.Text, WF_PREFECTURES5_Text.Text, WW_RTN_SW, work.CreateFIXParam(WF_CAMPCODE.Text, "PREFECTURES"))
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_PREFECTURES5.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_PREFECTURES5.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

End Class

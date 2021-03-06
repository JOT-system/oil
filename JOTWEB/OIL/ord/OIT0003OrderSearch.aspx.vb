﻿'Option Strict On
'Option Explicit On

Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 受注検索画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OrderSearch
    Inherits System.Web.UI.Page

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
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

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        Master.MAPID = OIT0003WRKINC.MAPIDS
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
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
            '運用部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", WF_UORG.Text)
            '営業所
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", TxtSalesOffice.Text)
            '年月日(積込日From(検索用))
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATESTART", TxtDateStart.Text)
            '年月日(積込日To(検索用))
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DATEEND", TxtDateEnd.Text)
            '発日(検索用)
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "DEPDATESTART", TxtDepDateStart.Text)
            '列車番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "TRAINNO", TxtTrainNumber.Text)
            '荷卸地
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UNLOADING", TxtUnloading.Text)
            '状態
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STATUS", TxtStatus.Text)
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003L Then   '一覧画面からの遷移
            '〇画面項目設定処理
            '会社コード
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            '営業所
            TxtSalesOffice.Text = work.WF_SEL_SALESOFFICECODEMAP.Text
            '年月日(積込日From(検索用))
            TxtDateStart.Text = work.WF_SEL_DATE.Text
            '年月日(積込日To(検索用))
            TxtDateEnd.Text = work.WF_SEL_DATE_TO.Text
            '発日(検索用)
            TxtDepDateStart.Text = work.WF_SEL_SEARCH_DEPDATE.Text
            '列車番号
            TxtTrainNumber.Text = work.WF_SEL_TRAINNUMBER.Text
            '荷卸地
            TxtUnloading.Text = work.WF_SEL_UNLOADINGCODE.Text
            '状態
            TxtStatus.Text = work.WF_SEL_STATUSCODE.Text
            '### 20201126 START 指摘票対応(No233)全体 ################################
            '受注キャンセルフラグ
            If work.WF_SEL_ORDERCANCELFLG.Text = "1" Then
                Me.ChkOrderCancelFlg.Checked = True
            Else
                Me.ChkOrderCancelFlg.Checked = False
            End If
            '### 20201126 END   指摘票対応(No233)全体 ################################
        End If

        '営業所・列車番号・荷卸地・状態を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtSalesOffice.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTrainNumber.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtUnloading.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtStatus.Attributes("onkeyPress") = "CheckNum()"

        '(予定)積込日・(予定)発日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtDateStart.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtDateEnd.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtDepDateStart.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0003WRKINC.MAPIDS
        rightview.MAPID = OIT0003WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        '営業所
        CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_DUMMY)
        '荷卸地
        CODENAME_get("UNLOADING", TxtUnloading.Text, LblUnloadingName.Text, WW_DUMMY)
        '状態
        CODENAME_get("STATUS", TxtStatus.Text, LblStatusName.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        '運用部署
        Master.EraseCharToIgnore(WF_UORG.Text)
        '営業所
        Master.EraseCharToIgnore(TxtSalesOffice.Text)
        '年月日(積込日From(検索用))
        Master.EraseCharToIgnore(TxtDateStart.Text)
        '年月日(積込日To(検索用))
        Master.EraseCharToIgnore(TxtDateEnd.Text)
        '発日(検索用)
        Master.EraseCharToIgnore(TxtDepDateStart.Text)
        '列車番号
        Master.EraseCharToIgnore(TxtTrainNumber.Text)
        '荷卸地
        Master.EraseCharToIgnore(TxtUnloading.Text)
        '状態
        Master.EraseCharToIgnore(TxtStatus.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '運用部署
        work.WF_SEL_UORG.Text = WF_UORG.Text
        '営業所
        work.WF_SEL_SALESOFFICECODEMAP.Text = TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICECODE.Text = TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICE.Text = LblSalesOfficeName.Text
        '年月日(積込日From(検索用))
        work.WF_SEL_DATE.Text = TxtDateStart.Text
        '年月日(積込日To(検索用))
        work.WF_SEL_DATE_TO.Text = TxtDateEnd.Text
        '発日(検索用)
        work.WF_SEL_SEARCH_DEPDATE.Text = TxtDepDateStart.Text
        '列車番号
        work.WF_SEL_TRAINNUMBER.Text = TxtTrainNumber.Text
        '荷卸地
        work.WF_SEL_UNLOADINGCODE.Text = TxtUnloading.Text
        work.WF_SEL_UNLOADING.Text = LblUnloadingName.Text
        '状態
        work.WF_SEL_STATUSCODE.Text = TxtStatus.Text
        work.WF_SEL_STATUS.Text = LblStatusName.Text

        '### 20201126 START 指摘票対応(No233)全体 ################################
        '受注キャンセルフラグ
        If Me.ChkOrderCancelFlg.Checked = True Then
            work.WF_SEL_ORDERCANCELFLG.Text = "1"
        Else
            work.WF_SEL_ORDERCANCELFLG.Text = "0"
        End If
        '### 20201126 END   指摘票対応(No233)全体 ################################

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
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '運用部署
        WW_TEXT = WF_UORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "UORG", WF_UORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_UORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "運用部署 : " & WF_UORG.Text)
                    WF_UORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_UORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '営業所
        If TxtSalesOffice.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "OFFICECODE", TxtSalesOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所", needsPopUp:=True)
                TxtSalesOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '年月日(積込日(検索用))
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", TxtDateStart.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDateStart.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "積込日From", needsPopUp:=True)
            TxtDateStart.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '年月日(積込日To(検索用))
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", TxtDateEnd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDateEnd.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "積込日To", needsPopUp:=True)
            TxtDateEnd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '発日(検索用)
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STDEPYMD", TxtDepDateStart.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDepDateStart.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "発日", needsPopUp:=True)
            TxtDepDateStart.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '列車番号
        If TxtTrainNumber.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "TRAINNUMBER", TxtTrainNumber.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                TxtTrainNumber.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        ''荷卸地
        'If TxtUnloading.Text <> "" Then
        '    Master.CheckField(WF_CAMPCODE.Text, "UNLOADING", TxtUnloading.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        '    If Not isNormal(WW_CS0024FCHECKERR) Then
        '        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
        '        TxtUnloading.Focus()
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        'End If

        '状態
        If TxtStatus.Text <> "" Then
            Master.CheckField(WF_CAMPCODE.Text, "STATUS", TxtStatus.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
                TxtStatus.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
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
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                    End If

                    '営業所
                    If WF_FIELD.Value = "TxtSalesOffice" Then
                        'prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, TxtSalesOffice.Text)
                        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtSalesOffice.Text)
                    End If

                    '荷卸地
                    If WF_FIELD.Value = "TxtUnloading" Then
                        'prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, TxtSalesOffice.Text)
                        If TxtSalesOffice.Text = "" Then
                            prmData = work.CreateSALESOFFICEParam("01", "")
                        Else
                            prmData = work.CreateSALESOFFICEParam(TxtSalesOffice.Text, "")
                        End If
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "TxtDateStart"
                            .WF_Calendar.Text = TxtDateStart.Text
                        Case "TxtDateEnd"
                            .WF_Calendar.Text = TxtDateEnd.Text
                        Case "TxtDepDateStart"
                            .WF_Calendar.Text = TxtDepDateStart.Text
                            'Case "TxtDateEnd"
                            '    .WF_Calendar.Text = TxtDateEnd.Text
                    End Select
                    .ActiveCalendar()

                End If
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
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '営業所
            Case "TxtSalesOffice"
                CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_RTN_SW)
            '荷卸地
            Case "TxtUnloading"
                CODENAME_get("UNLOADING", TxtUnloading.Text, LblUnloadingName.Text, WW_RTN_SW)
            '状態
            Case "TxtStatus"
                CODENAME_get("STATUS", TxtStatus.Text, LblStatusName.Text, WW_RTN_SW)
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
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_UORG"              '運用部署
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()

            Case "TxtSalesOffice"       '営業所
                TxtSalesOffice.Text = WW_SelectValue
                LblSalesOfficeName.Text = WW_SelectText
                TxtSalesOffice.Focus()

            Case "TxtDateStart"         '年月日(積込日From(検索用))
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtDateStart.Text = ""
                    Else
                        TxtDateStart.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDateStart.Focus()
            Case "TxtDateEnd"         '年月日(積込日To(検索用))
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtDateEnd.Text = ""
                    Else
                        Me.TxtDateEnd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                Me.TxtDateEnd.Focus()
            Case "TxtDepDateStart"      '発日(検索用)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtDepDateStart.Text = ""
                    Else
                        TxtDepDateStart.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDepDateStart.Focus()
            Case "TxtUnloading"       '荷卸地
                TxtUnloading.Text = WW_SelectValue
                LblUnloadingName.Text = WW_SelectText
                TxtUnloading.Focus()

            Case "TxtStatus"          '状態
                TxtStatus.Text = WW_SelectValue
                LblStatusName.Text = WW_SelectText
                TxtStatus.Focus()

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
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                WF_UORG.Focus()
            Case "TxtSalesOffice"       '営業所
                TxtSalesOffice.Focus()
            Case "TxtDateStart"         '年月日(積込日From(検索用))
                TxtDateStart.Focus()
            Case "TxtDateEnd"           '年月日(積込日To(検索用))
                TxtDateEnd.Focus()
            Case "TxtDepDateStart"      '発日(検索用)
                TxtDepDateStart.Focus()
            Case "TxtUnloading"         '荷卸地
                TxtUnloading.Focus()
            Case "TxtStatus"            '状態
                TxtStatus.Focus()
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
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OFFICECODE"       '営業所
                    prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UNLOADING"        '荷受人
                    prmData = work.CreateORDERSTATUSParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATUS"           '状態
                    prmData = work.CreateORDERSTATUSParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class
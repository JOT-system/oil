﻿''************************************************************
' 貨車連結順序表テーブル検索画面
' 作成日  :2020/07/27
' 更新日  :2020/07/27
' 作成者  :森川
' 更新車  :森川
'
' 修正履歴:新規作成
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

        '    ○ メニューからの画面遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)           '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ORG", WF_ORG.Text)                     '組織コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "BTRAINNO", Me.TxtBTrainNo.Text)        '返送列車番号
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "EMPARRDATE", Me.TxtEmparrDate.Text)    '空車着日

            '○ 一覧画面からの遷移
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002L Then
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text                '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text                          '組織コード
            Me.TxtBTrainNo.Text = work.WF_SEL_SEARCH_BTRAINNO.Text      '返送列車番号
            Me.TxtEmparrDate.Text = work.WF_SEL_SEARCH_EMPARRDATE.Text  '空車着日
            '### 20201216 START 指摘票対応(No263)全体 #######################################
            Me.WF_STYMD_CODE.Text = work.WF_SEL_SEARCH_AVAILABLEDATE.Text   '利用可能日
            '### 20201216 END   指摘票対応(No263)全体 #######################################

        End If

        '返送列車番号を入力するテキストボックスは数値(0～9),英語(A～Z)のみ可能とする。
        Me.TxtBTrainNo.Attributes("onkeyPress") = "CheckNumAZ()"
        '空車着日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtEmparrDate.Attributes("onkeyPress") = "CheckCalendar()"

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
        CODENAME_get("BTRAINNO", Me.TxtBTrainNo.Text, Me.LblBTrainNo.Text, WW_DUMMY)        '返送列車番号

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.EraseCharToIgnore(TxtBTrainNo.Text)          '返送列車番号
        Master.EraseCharToIgnore(TxtEmparrDate.Text)        '空車着日
        Master.EraseCharToIgnore(WF_STYMD_CODE.Text)        '利用可能日

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text                '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text        　　　　　        '組織コード
        work.WF_SEL_SEARCH_BTRAINNO.Text = Me.TxtBTrainNo.Text      '返送列車番号
        work.WF_SEL_SEARCH_BTRAINNAME.Text = Me.LblBTrainNo.Text    '返送列車名
        work.WF_SEL_SEARCH_EMPARRDATE.Text = Me.TxtEmparrDate.Text  '空車着日
        '### 20201216 START 指摘票対応(No263)全体 #######################################
        work.WF_SEL_SEARCH_AVAILABLEDATE.Text = Me.WF_STYMD_CODE.Text   '利用可能日
        '### 20201216 END   指摘票対応(No263)全体 #######################################

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

        '○ 返送列車番号
        If Me.TxtBTrainNo.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BTRAINNO", Me.TxtBTrainNo.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Me.TxtBTrainNo.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '○ 空車着日
        '存在チェック
        If Me.TxtEmparrDate.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "空車着日", needsPopUp:=True)
            Me.TxtEmparrDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        WW_CheckDate(Me.TxtEmparrDate.Text, "空車着日", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            Me.TxtEmparrDate.Focus()
            WW_CheckMES1 = "空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            Me.TxtEmparrDate.Text = CDate(Me.TxtEmparrDate.Text).ToString("yyyy/MM/dd")
        End If

        '### 20201216 START 指摘票対応(No263)全体 #######################################
        '○ 利用可能日
        If Me.WF_STYMD_CODE.Text <> "" Then
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", Me.WF_STYMD_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                Try
                    Date.TryParse(Me.WF_STYMD_CODE.Text, WW_STYMD)
                Catch ex As Exception
                    WW_STYMD = C_DEFAULT_YMD
                End Try
            Else
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "利用可能日", needsPopUp:=True)
                Me.WF_STYMD_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        End If
        '### 20201216 END   指摘票対応(No263)全体 #######################################

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
                            Case "TxtEmparrDate"    '空車着日
                                .WF_Calendar.Text = TxtEmparrDate.Text
                        End Select
                        .ActiveCalendar()
                    Case Else
                        '以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = Me.WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "TxtBTrainNo"          '返送列車番号
                                prmData = work.CreateSTATIONPTParam(Me.WF_CAMPCODE.Text, Me.TxtBTrainNo.Text)
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
            Case "TxtBTrainNo"          '返送列車番号
                CODENAME_get("BTRAINNO", Me.TxtBTrainNo.Text, Me.LblBTrainNo.Text, WW_RTN_SW)
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
            Case "TxtBTrainNo"               '返送列車番号

                '〇 KeyCodeが重複し、名称(Value1)が異なる場合の取得術
                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                    Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                    WW_SelectValue = selectedItem.Value
                    WW_SelectText = selectedItem.Text
                End If

                If WW_SelectText = "" Then
                    Me.TxtBTrainNo.Text = ""
                    Me.LblBTrainNo.Text = ""
                    Exit Select
                End If

                Me.TxtBTrainNo.Text = WW_SelectValue
                Try
                    Me.LblBTrainNo.Text = WW_SelectText.Substring(5)
                    If Me.LblBTrainNo.Text = "" Then Me.LblBTrainNo.Text = WW_SelectText
                Catch ex As Exception
                    Me.LblBTrainNo.Text = WW_SelectText
                End Try
                Me.TxtBTrainNo.Focus()

            Case "TxtEmparrDate"             '空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtEmparrDate.Text = ""
                    Else
                        Me.TxtEmparrDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                Me.TxtEmparrDate.Focus()

            Case "WF_STYMD"                  '利用可能日
                '### 20201216 START 指摘票対応(No263)全体 #######################################
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.WF_STYMD_CODE.Text = ""
                    Else
                        Me.WF_STYMD_CODE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                Me.WF_STYMD_CODE.Focus()
                '### 20201216 END   指摘票対応(No263)全体 #######################################
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
            Case "TxtBTrainNo"           '返送列車番号
                Me.TxtBTrainNo.Focus()
            Case "TxtEmparrDate"         '空車着日
                Me.TxtEmparrDate.Focus()

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
                Case "CAMPCODE"        '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"             '組織コード
                    prmData = work.CreateORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BTRAINNO"        '返送列車番号
                    prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "BTRAINNUMBER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BTRAINNUMBER, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

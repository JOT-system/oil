Option Strict On
''************************************************************
' 在庫管理表検索画面
' 作成日 2019/11/08
' 更新日 2019/11/08
' 作成者 JOT遠藤
' 更新者 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 在庫管理表登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class OIT0004OilStockSearch
    Inherits Page
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
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
                If Not {"WF_ButtonDO", "WF_ButtonEND"}.Contains(WF_ButtonClick.Value) Then
                    GetLastUpdate()
                End If
            End If
        Else
            '○ 初期化処理
            Initialize()
            GetLastUpdate()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = OIT0004WRKINC.MAPIDS

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
            Master.GetFirstValue(work.WF_SEL_ORG.Text, "ORG", WF_ORG.Text)       '組織コード
            'Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", TxtSalesOffice.Text) '営業所
            'Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "SHIPPER", TxtShipper.Text) '荷主
            'Master.GetFirstValue(work.WF_SEL_CONSIGNEE.Text, "CONSIGNEE", WF_CONSIGNEE_CODE.Text)       '油槽所

            Dim prmData As New Hashtable
            Dim shipperCode As String = ""
            Dim consigneeCode As String = ""
            prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text
            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtSalesOffice.Text)
            leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, WW_DUMMY, prmData)
            If {"302001", "301901"}.Contains(Master.USER_ORG) Then
                leftview.WF_LeftListBox.Items.Clear()
                leftview.WF_LeftListBox.Items.Add(New ListItem("根岸営業所", "011402"))
            End If
            If leftview.WF_LeftListBox.Items IsNot Nothing Then
                '一旦根岸(011402)'本当はログインユーザーのORG
                Dim foundItem = leftview.WF_LeftListBox.Items.FindByValue("011402")
                'Dim foundItem = leftview.WF_LeftListBox.Items.FindByValue(Master.USER_ORG)
                If foundItem IsNot Nothing Then
                    TxtSalesOffice.Text = foundItem.Value
                Else
                    TxtSalesOffice.Text = leftview.WF_LeftListBox.Items(0).Value

                End If

                If TxtSalesOffice.Text <> "" Then
                    GetDefRelateValues(TxtSalesOffice.Text, shipperCode, consigneeCode)
                End If

            End If

            prmData = work.CreateFIXParam(TxtSalesOffice.Text, "JOINTMASTER")
            leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, WW_DUMMY, prmData)
            If shipperCode <> "" Then
                TxtShipper.Text = shipperCode
            ElseIf leftview.WF_LeftListBox.Items IsNot Nothing Then
                TxtShipper.Text = leftview.WF_LeftListBox.Items(0).Value
            End If

            Dim additionalCond As String = " and VALUE2 != '9' "
            prmData = work.CreateFIXParam(TxtSalesOffice.Text, "CONSIGNEEPATTERN", I_ADDITIONALCONDITION:=additionalCond)
            leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, WW_DUMMY, prmData)
            If consigneeCode <> "" Then
                Me.WF_CONSIGNEE_CODE.Text = consigneeCode
            ElseIf leftview.WF_LeftListBox.Items IsNot Nothing Then
                Me.WF_CONSIGNEE_CODE.Text = leftview.WF_LeftListBox.Items(0).Value
            End If
            Master.GetFirstValue(work.WF_SEL_STYMD.Text, "STYMD", WF_STYMD_CODE.Text)       '年月日
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0004C Then   '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text            '組織コード
            TxtSalesOffice.Text = work.WF_SEL_SALESOFFICECODEMAP.Text '営業所
            TxtShipper.Text = work.WF_SEL_SHIPPERCODE.Text
            WF_CONSIGNEE_CODE.Text = work.WF_SEL_CONSIGNEE.Text            '油槽所
            WF_STYMD_CODE.Text = work.WF_SEL_STYMD.Text            '年月日
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0004WRKINC.MAPIDS
        rightview.MAPID = OIT0004WRKINC.MAPIDC
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF
        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        'CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)         '会社コード
        'CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)         '組織コード
        '営業所
        CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_DUMMY)
        CODENAME_get("SHIPPER", TxtShipper.Text, LblShipperName.Text, WW_DUMMY)
        CODENAME_get("CONSIGNEE", WF_CONSIGNEE_CODE.Text, WF_CONSIGNEE_NAME.Text, WW_DUMMY)         '油槽所

    End Sub


    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CONSIGNEE_CODE.Text)      '油槽所
        Master.EraseCharToIgnore(WF_STYMD_CODE.Text)          '年月日

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text                  '組織コード
        '営業所
        work.WF_SEL_SALESOFFICECODEMAP.Text = TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICECODE.Text = TxtSalesOffice.Text
        work.WF_SEL_SALESOFFICE.Text = LblSalesOfficeName.Text

        work.WF_SEL_SHIPPERCODE.Text = TxtShipper.Text      '荷主
        work.WF_SEL_SHIPPERNAME.Text = LblShipperName.Text  '荷主名
        work.WF_SEL_CONSIGNEE.Text = WF_CONSIGNEE_CODE.Text '油槽所
        work.WF_SEL_CONSIGNEENAME.Text = WF_CONSIGNEE_NAME.Text   '油槽所名
        work.WF_SEL_STYMD.Text = WF_STYMD_CODE.Text         '年月日


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
        '営業所
        Master.CheckField(WF_CAMPCODE.Text, "OFFICECODE", TxtSalesOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "営業所 : " & TxtSalesOffice.Text, needsPopUp:=True)
                TxtSalesOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "営業所", needsPopUp:=True)
            TxtSalesOffice.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '荷主 Shipper
        'Master.CheckField(WF_CAMPCODE.Text, "CONSIGNEE", WF_CONSIGNEE_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        Master.CheckField(WF_CAMPCODE.Text, "SHIPPER", TxtShipper.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SHIPPER", TxtShipper.Text, LblShipperName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "荷主 : " & TxtShipper.Text, needsPopUp:=True)
                TxtShipper.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "荷主", needsPopUp:=True)
            TxtShipper.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '油槽所 TxtSalesOffice.Text
        'Master.CheckField(WF_CAMPCODE.Text, "CONSIGNEE", WF_CONSIGNEE_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        Master.CheckField(WF_CAMPCODE.Text, "CONSIGNEE", WF_CONSIGNEE_CODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CONSIGNEE", WF_CONSIGNEE_CODE.Text, WF_CONSIGNEE_NAME.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "油槽所 : " & WF_CONSIGNEE_CODE.Text, needsPopUp:=True)
                WF_CONSIGNEE_CODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "油槽所", needsPopUp:=True)
            WF_CONSIGNEE_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '年月日
        '存在チェック
        If WF_STYMD_CODE.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "年月日", needsPopUp:=True)
            WF_STYMD_CODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        WW_CheckDate(WF_STYMD_CODE.Text, "年月日", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            WF_STYMD_CODE.Focus()
            WW_CheckMES1 = "年月日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            WF_STYMD_CODE.Text = CDate(WF_STYMD_CODE.Text).ToString("yyyy/MM/dd")
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
            If Not DateTime.IsLeapYear(CInt(chkLeapYear)) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf CInt(getMonth) >= 13 OrElse CInt(getDay) >= 32 Then
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
        Me.WF_CAMPCODE.Text = Master.USERCAMP
        work.WF_SEL_CAMPCODE.Text = Master.USERCAMP
        '○ 前画面遷移
        Master.TransitionPrevPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                WF_LeftMViewChange.Value = Integer.Parse(WF_LeftMViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case CInt(WF_LeftMViewChange.Value)
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_STYMD"         '年月日
                                .WF_Calendar.Text = CDate(WF_STYMD_CODE.Text).ToString("yyyy/MM/dd")
                        End Select
                        .ActiveCalendar()
                    Case LIST_BOX_CLASSIFICATION.LC_ORG
                        '組織コード(営業所)

                    Case Else
                        '会社コード
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        '営業所
                        If WF_FIELD.Value = "TxtSalesOffice" Then
                            'prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, TxtSalesOffice.Text)
                            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtSalesOffice.Text)
                        End If
                        '荷主
                        If WF_FIELD.Value = "TxtShipper" Then
                            Dim additionalCond As String = " and KEYCODE != '9999999999' " 'キグナス除く
                            prmData = work.CreateFIXParam(TxtSalesOffice.Text, "JOINTMASTER", I_ADDITIONALCONDITION:=additionalCond)
                        End If
                        '油槽所
                        If WF_FIELD.Value = "WF_CONSIGNEE" Then
                            'prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "CONSIGNEEPATTERN")
                            Dim additionalCond As String = " and VALUE2 != '9' "
                            prmData = work.CreateFIXParam(TxtSalesOffice.Text, "CONSIGNEEPATTERN", I_ADDITIONALCONDITION:=additionalCond)
                        End If
                        Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                        .SetListBox(enumVal, WW_DUMMY, prmData)
                        If Master.USER_ORG = "011203" AndAlso WF_FIELD.Value = "TxtSalesOffice" Then
                            If leftview.WF_LeftListBox.Items.FindByValue("012402") Is Nothing Then
                                leftview.WF_LeftListBox.Items.Add(New ListItem("三重塩浜営業所", "012402"))
                            End If

                        End If
                        If {"302001", "301901"}.Contains(Master.USER_ORG) Then
                            If WF_FIELD.Value = "TxtSalesOffice" Then
                                leftview.WF_LeftListBox.Items.Clear()
                                leftview.WF_LeftListBox.Items.Add(New ListItem("根岸営業所", "011402"))
                            End If
                            If WF_FIELD.Value = "WF_CONSIGNEE" Then
                                leftview.WF_LeftListBox.Items.Clear()
                                If Master.USER_ORG = "302001" Then
                                    leftview.WF_LeftListBox.Items.Add(New ListItem("ENEOS北信油槽所", "10"))
                                Else
                                    leftview.WF_LeftListBox.Items.Add(New ListItem("ENEOS甲府油槽所", "20"))
                                End If
                            End If
                        End If
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
        Dim fieldName As String = ""
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '営業所
            Case "TxtSalesOffice"
                CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_RTN_SW)
                fieldName = "営業所"
                If TxtSalesOffice.Text <> "" AndAlso LblSalesOfficeName.Text <> "" Then
                    Dim shippersCode As String = ""
                    Dim consigneeCode As String = ""
                    GetDefRelateValues(TxtSalesOffice.Text, shippersCode, consigneeCode)
                    TxtShipper.Text = shippersCode
                    WF_CONSIGNEE_CODE.Text = consigneeCode
                    CODENAME_get("SHIPPER", TxtShipper.Text, LblShipperName.Text, WW_RTN_SW)
                    CODENAME_get("CONSIGNEE", WF_CONSIGNEE_CODE.Text, WF_CONSIGNEE_NAME.Text, WW_RTN_SW)
                End If
            Case "TxtShipper"
                CODENAME_get("SHIPPER", TxtShipper.Text, LblShipperName.Text, WW_RTN_SW)
                fieldName = "荷主"
            Case "WF_CONSIGNEE"        '油槽所
                CODENAME_get("CONSIGNEE", WF_CONSIGNEE_CODE.Text, WF_CONSIGNEE_NAME.Text, WW_RTN_SW)
                fieldName = "油槽所"
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR, fieldName)
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
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex.ToString
            WW_SelectValue = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "TxtSalesOffice"       '営業所
                TxtSalesOffice.Text = WW_SelectValue
                LblSalesOfficeName.Text = WW_SelectText
                If TxtSalesOffice.Text <> "" AndAlso LblSalesOfficeName.Text <> "" Then
                    Dim shippersCode As String = ""
                    Dim consigneeCode As String = ""
                    GetDefRelateValues(TxtSalesOffice.Text, shippersCode, consigneeCode)
                    TxtShipper.Text = shippersCode
                    WF_CONSIGNEE_CODE.Text = consigneeCode
                    CODENAME_get("SHIPPER", TxtShipper.Text, LblShipperName.Text, WW_RTN_SW)
                    CODENAME_get("CONSIGNEE", WF_CONSIGNEE_CODE.Text, WF_CONSIGNEE_NAME.Text, WW_RTN_SW)
                End If
                TxtSalesOffice.Focus()
            Case "TxtShipper"          '荷主
                TxtShipper.Text = WW_SelectValue
                LblShipperName.Text = WW_SelectText
                TxtShipper.Focus()

            Case "WF_CONSIGNEE"          '油槽所
                WF_CONSIGNEE_CODE.Text = WW_SelectValue
                WF_CONSIGNEE_NAME.Text = WW_SelectText
                WF_CONSIGNEE_CODE.Focus()

            Case "WF_STYMD"             '年月日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        WF_STYMD_CODE.Text = ""
                    Else
                        WF_STYMD_CODE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD_CODE.Focus()
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
            Case "WF_CONSIGNEE"          '油槽所
                WF_CONSIGNEE_CODE.Focus()
            Case "WF_"
                TxtShipper.Focus()
            Case "WF_STYMD"          '年月日
                WF_STYMD_CODE.Focus()
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
                Case "ORG"             '運用部署
                    prmData = work.CreateORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OFFICECODE"       '営業所
                    prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPER"        '荷主
                    Dim additionalCond As String = " and KEYCODE != '9999999999' " 'キグナス除く
                    prmData = work.CreateFIXParam(TxtSalesOffice.Text, "JOINTMASTER", I_ADDITIONALCONDITION:=additionalCond)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CONSIGNEE"        '油槽所
                    'WF_CAMPCODE.Text
                    Dim additionalCond As String = " and VALUE2 != '9' "
                    prmData = work.CreateFIXParam(TxtSalesOffice.Text, "CONSIGNEEPATTERN", I_ADDITIONALCONDITION:=additionalCond)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 営業所に紐づく初期の荷主、荷受人コードを取得
    ''' </summary>
    ''' <param name="officeCode">営業所コード</param>
    ''' <param name="shippersCode">[OUT]荷主コード</param>
    ''' <param name="consigneeCode">[OUT]荷受人コード</param>
    Protected Sub GetDefRelateValues(ByVal officeCode As String, ByRef shippersCode As String, ByRef consigneeCode As String)
        Try
            '営業所コードが未設定なら何もしない
            If officeCode = "" Then
                Return
            End If

            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT FX.VALUE1 AS SHIPPERSCODE")
            sqlStat.AppendLine("      ,FX.VALUE2 AS CONSIGNEECODE")
            sqlStat.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
            sqlStat.AppendLine(" WHERE FX.CAMPCODE = @CAMPCODE")
            sqlStat.AppendLine("   AND FX.CLASS    = @CLASS")
            sqlStat.AppendLine("   AND FX.KEYCODE  = @OFFICE")
            sqlStat.AppendLine("   AND FX.DELFLG   = @DELFLG")
            'DataBase接続文字
            Using sqlCon = CS0050SESSION.getConnection,
                  sqlCmd = New SqlClient.SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() 'DataBase接続(Open)
                SqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@CAMPCODE", SqlDbType.NVarChar).Value = "01"
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "STOCKSELECTDEFOFFICE"
                    .Add("@OFFICE", SqlDbType.NVarChar).Value = officeCode
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Using sqlDr As SqlClient.SqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows Then
                        sqlDr.Read()
                        shippersCode = Convert.ToString(sqlDr("SHIPPERSCODE"))
                        consigneeCode = Convert.ToString(sqlDr("CONSIGNEECODE"))
                    End If

                    If Master.USER_ORG = "302001" Then
                        consigneeCode = "10"
                    ElseIf Master.USER_ORG = "301901" Then
                        consigneeCode = "20"
                    End If

                End Using 'sqlDr
            End Using 'sqlCon, sqlCmd

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub
    ''' <summary>
    ''' 最終更新者取得
    ''' </summary>
    Public Sub GetLastUpdate()
        Try
            Dim valueHasNone = "―"
            Dim valueNotFixed = "未"
            Dim valueFixed = "済"
            divUpdateInfo.Visible = False
            divConsigneeUpdateInfo10.Visible = False
            divConsigneeUpdateInfo20.Visible = False
            '一旦初期化
            Me.WF_UpdateUser.Text = valueHasNone
            Me.WF_UpdateDtm.Text = valueHasNone
            Me.WF_ConsigneeUser10.Text = valueHasNone
            Me.WF_ConsigneeUpdateDtm10.Text = valueHasNone
            Me.WF_ConsigneeUser20.Text = valueHasNone
            Me.WF_ConsigneeUpdateDtm20.Text = valueHasNone
            Me.WF_ConsigneeFixStatus10.Text = valueNotFixed
            Me.WF_ConsigneeFixStatus20.Text = valueNotFixed
            Dim officeCode As String = ""
            '取得出来ない条件の場合はスキップ
            If Me.TxtSalesOffice.Text.Trim = "" OrElse
               Me.LblSalesOfficeName.Text = "" OrElse
               Me.TxtShipper.Text.Trim = "" OrElse
               Me.LblShipperName.Text = "" OrElse
               Me.WF_CONSIGNEE_CODE.Text.Trim = "" OrElse
               Me.WF_CONSIGNEE_NAME.Text = "" OrElse
               Me.WF_STYMD_CODE.Text.Trim = "" OrElse
               IsDate(Me.WF_STYMD_CODE.Text) = False Then
                Return
            End If
            '通常更新情報と油槽所更新情報のフィールド設定
            Dim fieldItems = {New With {.updUserField = "UPDUSER", .updUserObj = Me.WF_UpdateUser, .updDtmField = "UPDYMD", .updDtmObj = Me.WF_UpdateDtm, .consigneeCode = Me.WF_CONSIGNEE_CODE.Text, .getFixedData = False, .fixValObj = New Label},
                             New With {.updUserField = "ENEOSUPDUSER", .updUserObj = Me.WF_ConsigneeUser10, .updDtmField = "ENEOSUPDYMD", .updDtmObj = Me.WF_ConsigneeUpdateDtm10, .consigneeCode = "10", .getFixedData = True, .fixValObj = Me.WF_ConsigneeFixStatus10},
                             New With {.updUserField = "ENEOSUPDUSER", .updUserObj = Me.WF_ConsigneeUser20, .updDtmField = "ENEOSUPDYMD", .updDtmObj = Me.WF_ConsigneeUpdateDtm20, .consigneeCode = "20", .getFixedData = True, .fixValObj = Me.WF_ConsigneeFixStatus20}}
            For Each fieldItem In fieldItems
                Dim sqlStat As New StringBuilder
                sqlStat.AppendFormat("SELECT format(SQ.{0},'yyyy/MM/dd HH:mm') AS {0}", fieldItem.updDtmField).AppendLine()
                sqlStat.AppendFormat("      ,isnull(UM.STAFFNAMEL, SQ.{0})     AS {0}", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("FROM (")
                sqlStat.AppendFormat("SELECT {0}", fieldItem.updDtmField).AppendLine()
                sqlStat.AppendFormat("      ,{0}", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("  FROM OIL.OIT0009_UKEIREOILSTOCK WITH(nolock)")
                sqlStat.AppendLine(" WHERE STOCKYMD       between @STOCKYMD and DATEADD(DAY, 30, @STOCKYMD)")
                'sqlStat.AppendLine(" WHERE 1=1")
                sqlStat.AppendLine("   AND OFFICECODE     = @OFFICECODE")
                sqlStat.AppendLine("   AND SHIPPERSCODE   = @SHIPPERSCODE")
                sqlStat.AppendLine("   AND CONSIGNEECODE  = @CONSIGNEECODE")
                sqlStat.AppendFormat("   AND {0}       <> 'BATCH'", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("   AND DELFLG         = @DELFLG")
                sqlStat.AppendLine(" UNION ALL")
                sqlStat.AppendFormat("SELECT {0}", fieldItem.updDtmField).AppendLine()
                sqlStat.AppendFormat("      ,{0}", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("  FROM OIL.OIT0001_OILSTOCK WITH(nolock)")
                sqlStat.AppendLine(" WHERE STOCKYMD       between @STOCKYMD and DATEADD(DAY, 30, @STOCKYMD)")
                'sqlStat.AppendLine(" WHERE 1=1")
                sqlStat.AppendLine("   AND OFFICECODE     = @OFFICECODE")
                sqlStat.AppendLine("   AND SHIPPERSCODE   = @SHIPPERSCODE")
                sqlStat.AppendLine("   AND CONSIGNEECODE  = @CONSIGNEECODE")
                sqlStat.AppendFormat("   AND {0}       <> 'BATCH'", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("   AND DELFLG         = @DELFLG")
                sqlStat.AppendLine(" ) SQ")
                sqlStat.AppendLine(" LEFT JOIN com.OIS0004_USER UM WITH(nolock)")
                sqlStat.AppendFormat("   ON UM.USERID = SQ.{0}", fieldItem.updUserField).AppendLine()
                sqlStat.AppendLine("  AND @STOCKYMD between UM.STYMD and UM.ENDYMD")
                sqlStat.AppendLine("  AND DELFLG    = @DELFLG")
                sqlStat.AppendFormat(" ORDER BY SQ.{0} DESC", fieldItem.updDtmField).AppendLine()

                Dim sqlStatFixed = New StringBuilder
                sqlStatFixed.AppendLine("SELECT 1")
                sqlStatFixed.AppendLine("  FROM OIL.OIT0001_OILSTOCK OS with(nolock)")
                sqlStatFixed.AppendLine(" WHERE OS.STOCKYMD      = @TODAY")
                sqlStatFixed.AppendLine("   AND OS.OFFICECODE    = @OFFICECODE")
                sqlStatFixed.AppendLine("   AND OS.SHIPPERSCODE  = @SHIPPERSCODE")
                sqlStatFixed.AppendLine("   AND OS.CONSIGNEECODE = @CONSIGNEECODE")
                sqlStatFixed.AppendLine("   AND OS.FIXEDYMD      IS NOT NULL")
                sqlStatFixed.AppendLine("   AND OS.DELFLG        = @DELFLG")
                'DataBase接続文字
                Using sqlCon = CS0050SESSION.getConnection,
                      sqlCmd = New SqlClient.SqlCommand(sqlStat.ToString, sqlCon)
                    sqlCon.Open() 'DataBase接続(Open)
                    SqlConnection.ClearPool(sqlCon)
                    With sqlCmd.Parameters
                        .Add("@STOCKYMD", SqlDbType.Date).Value = CDate(Me.WF_STYMD_CODE.Text)
                        .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.TxtSalesOffice.Text
                        .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = Me.TxtShipper.Text
                        .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = fieldItem.consigneeCode
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                        .Add("@TODAY", SqlDbType.Date).Value = Now.AddDays(1).ToString("yyyy/MM/dd")
                    End With

                    Using sqlDr As SqlClient.SqlDataReader = sqlCmd.ExecuteReader()
                        If sqlDr.HasRows Then
                            sqlDr.Read()
                            Dim userName As String = Convert.ToString(sqlDr(fieldItem.updUserField))
                            Dim updDtm As String = Convert.ToString(sqlDr(fieldItem.updDtmField))
                            If userName = "" Then
                                userName = "―"
                                updDtm = "―"
                            End If
                            fieldItem.updUserObj.Text = userName
                            fieldItem.updDtmObj.Text = updDtm
                        End If
                    End Using 'sqlDr

                    If fieldItem.getFixedData Then
                        sqlCmd.CommandText = sqlStatFixed.ToString
                        Using sqlDr As SqlClient.SqlDataReader = sqlCmd.ExecuteReader()
                            If sqlDr.HasRows Then
                                fieldItem.fixValObj.Text = valueFixed
                            Else
                                fieldItem.fixValObj.Text = valueNotFixed
                            End If
                        End Using 'sqlDr
                    End If
                End Using 'sqlCon, sqlCmd

            Next

            divUpdateInfo.Visible = True
            If {"302001"}.Contains(Master.USER_ORG) Then
                '北信・甲府しか操作できない前提のため、他の油槽所を開放するなら要変更
                divConsigneeUpdateInfo10.Visible = True
            ElseIf {"301901"}.Contains(Master.USER_ORG) Then
                divConsigneeUpdateInfo20.Visible = True
            Else

                If {"10", "20"}.Contains(Me.WF_CONSIGNEE_CODE.Text) Then
                    divConsigneeUpdateInfo10.Visible = True
                    divConsigneeUpdateInfo20.Visible = True
                End If
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0004S LASTUPDATE_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004S LASTUPDATE_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return
        End Try
    End Sub
End Class

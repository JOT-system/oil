Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 車両付属情報（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRMA0004SELECT
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
            '○ 各ボタン押下処理
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
            '○初期化
            initialize()
        End If

    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub initialize()
        '○初期値設定
        Master.MAPID = GRMA0004WRKINC.MAPIDS
        WF_YYF.Focus()
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()
        WF_FIELD.Value = ""

        '○画面の値設定
        WW_MAPValueSet()
    End Sub
    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 画面戻先URL取得
        Master.transitionPrevPage()

    End Sub
    ''' <summary>
    ''' 検索実行処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_YYF.Text)               '年度(From)
        Master.eraseCharToIgnore(WF_YYT.Text)               '年度(To)
        Master.eraseCharToIgnore(WF_MORG.Text)              '管理部署
        Master.eraseCharToIgnore(WF_SORG.Text)              '設置部署
        Master.eraseCharToIgnore(WF_OILTYPE1.Text)          '油種(1)
        Master.eraseCharToIgnore(WF_OILTYPE2.Text)          '油種(2)
        Master.eraseCharToIgnore(WF_OWNCODEF.Text)          '荷主(From)
        Master.eraseCharToIgnore(WF_OWNCODET.Text)          '荷主(To)
        Master.eraseCharToIgnore(WF_SHARYOTYPE1.Text)       '車両タイプ(1)
        Master.eraseCharToIgnore(WF_SHARYOTYPE2.Text)       '車両タイプ(2)
        Master.eraseCharToIgnore(WF_SHARYOTYPE3.Text)       '車両タイプ(3)
        Master.eraseCharToIgnore(WF_SHARYOTYPE4.Text)       '車両タイプ(4)
        Master.eraseCharToIgnore(WF_SHARYOTYPE5.Text)       '車両タイプ(5)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text                '会社コード
        work.WF_SEL_YYF.Text = WF_YYF.Text                          '年度
        If WF_YYT.Text = "" Then
            work.WF_SEL_YYT.Text = WF_YYF.Text
        Else
            work.WF_SEL_YYT.Text = WF_YYT.Text
        End If
        work.WF_SEL_MORG.Text = WF_MORG.Text                        '管理部署
        work.WF_SEL_SORG.Text = WF_SORG.Text                        '設置部署
        work.WF_SEL_OILTYPE1.Text = WF_OILTYPE1.Text                '油種(1)
        work.WF_SEL_OILTYPE2.Text = WF_OILTYPE2.Text                '油種(2)
        work.WF_SEL_OWNCODE1.Text = WF_OWNCODEF.Text                '荷主
        If WF_OWNCODET.Text = "" Then
            work.WF_SEL_OWNCODE2.Text = WF_OWNCODEF.Text
        Else
            work.WF_SEL_OWNCODE2.Text = WF_OWNCODET.Text
        End If
        work.WF_SEL_SHARYOTYPE1.Text = WF_SHARYOTYPE1.Text          '車両タイプ(1)
        work.WF_SEL_SHARYOTYPE2.Text = WF_SHARYOTYPE2.Text          '車両タイプ(2)
        work.WF_SEL_SHARYOTYPE3.Text = WF_SHARYOTYPE3.Text          '車両タイプ(3)
        work.WF_SEL_SHARYOTYPE4.Text = WF_SHARYOTYPE4.Text          '車両タイプ(4)
        work.WF_SEL_SHARYOTYPE5.Text = WF_SHARYOTYPE5.Text          '車両タイプ(5)

        '○右ボックスからViewID取得
        Master.VIEWID = rightview.getViewId(work.WF_SEL_CAMPCODE.Text)

        '○ 画面遷移実行
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '○画面遷移先URL取得
            Master.transitionPage()
        End If

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

            Case "WF_MORG"              '管理部署
                WF_MORG_Text.Text = WW_SelectTEXT
                WF_MORG.Text = WW_SelectValue
                WF_MORG.Focus()

            Case "WF_SORG"              '設置部署
                WF_SORG_Text.Text = WW_SelectTEXT
                WF_SORG.Text = WW_SelectValue
                WF_SORG.Focus()

            Case "WF_OILTYPE1"          '油種(1)
                WF_OILTYPE1_Text.Text = WW_SelectTEXT
                WF_OILTYPE1.Text = WW_SelectValue
                WF_OILTYPE1.Focus()
            Case "WF_OILTYPE2"          '油種(2)
                WF_OILTYPE2_Text.Text = WW_SelectTEXT
                WF_OILTYPE2.Text = WW_SelectValue
                WF_OILTYPE2.Focus()

            Case "WF_OWNCODEF"          '荷主(From)
                WF_OWNCODEF_Text.Text = WW_SelectTEXT
                WF_OWNCODEF.Text = WW_SelectValue
                WF_OWNCODEF.Focus()
            Case "WF_OWNCODET"          '荷主(To)
                WF_OWNCODET_Text.Text = WW_SelectTEXT
                WF_OWNCODET.Text = WW_SelectValue
                WF_OWNCODET.Focus()

            Case "WF_SHARYOTYPE1"       '車両タイプ(1)
                WF_SHARYOTYPE1_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE1.Text = WW_SelectValue
                WF_SHARYOTYPE1.Focus()
            Case "WF_SHARYOTYPE2"       '車両タイプ(2)
                WF_SHARYOTYPE2_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE2.Text = WW_SelectValue
                WF_SHARYOTYPE2.Focus()
            Case "WF_SHARYOTYPE3"       '車両タイプ(3)
                WF_SHARYOTYPE3_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE3.Text = WW_SelectValue
                WF_SHARYOTYPE3.Focus()
            Case "WF_SHARYOTYPE4"       '車両タイプ(4)
                WF_SHARYOTYPE4_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE4.Text = WW_SelectValue
                WF_SHARYOTYPE4.Focus()
            Case "WF_SHARYOTYPE5"       '車両タイプ(5)
                WF_SHARYOTYPE5_Text.Text = WW_SelectTEXT
                WF_SHARYOTYPE5.Text = WW_SelectValue
                WF_SHARYOTYPE5.Focus()

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
            Case "WF_MORG"              '管理部署
                WF_MORG.Focus()
            Case "WF_SORG"              '設置部署
                WF_SORG.Focus()
            Case "WF_OILTYPE1"          '油種(1)
                WF_OILTYPE1.Focus()
            Case "WF_OILTYPE2"          '油種(2)
                WF_OILTYPE2.Focus()
            Case "WF_OWNCODEF"          '荷主(From)
                WF_OWNCODEF.Focus()
            Case "WF_OWNCODET"          '荷主(To)
                WF_OWNCODET.Focus()
            Case "WF_SHARYOTYPE1"       '車両タイプ(1)
                WF_SHARYOTYPE1.Focus()
            Case "WF_SHARYOTYPE2"       '車両タイプ(2)
                WF_SHARYOTYPE2.Focus()
            Case "WF_SHARYOTYPE3"       '車両タイプ(3)
                WF_SHARYOTYPE3.Focus()
            Case "WF_SHARYOTYPE4"       '車両タイプ(4)
                WF_SHARYOTYPE4.Focus()
            Case "WF_SHARYOTYPE5"       '車両タイプ(5)
                WF_SHARYOTYPE5.Focus()
        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text)

                    Select Case WF_LeftMViewChange.Value
                        Case LIST_BOX_CLASSIFICATION.LC_ORG
                            If WF_FIELD.Value = "WF_MORG" Then
                                prmData = work.CreateORGParam(WF_CAMPCODE.Text, True)
                            Else
                                prmData = work.CreateORGParam(WF_CAMPCODE.Text, False)
                            End If
                        Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                            prmData = work.CreateTODOParam(WF_CAMPCODE.Text)
                        Case 999
                            prmData = work.CreateFIXParam(WF_CAMPCODE.Text, "SHARYOTYPE")
                    End Select

                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case Else
                    End Select
                    .activeCalendar()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""          '会社
        WF_MORG_Text.Text = ""              '管理部署
        WF_SORG_Text.Text = ""              '設置部署
        WF_OILTYPE1_Text.Text = ""          '油種(1)
        WF_OILTYPE2_Text.Text = ""          '油種(2)
        WF_OWNCODEF_Text.Text = ""          '荷主(From)
        WF_OWNCODET_Text.Text = ""          '荷主(To)
        WF_SHARYOTYPE1_Text.Text = ""       '車両タイプ(1)
        WF_SHARYOTYPE2_Text.Text = ""       '車両タイプ(2)
        WF_SHARYOTYPE3_Text.Text = ""       '車両タイプ(3)
        WF_SHARYOTYPE4_Text.Text = ""       '車両タイプ(4)
        WF_SHARYOTYPE5_Text.Text = ""       '車両タイプ(5)

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_YYF.Text)               '年度(From)
        Master.eraseCharToIgnore(WF_YYT.Text)               '年度(To)
        Master.eraseCharToIgnore(WF_MORG.Text)              '管理部署
        Master.eraseCharToIgnore(WF_SORG.Text)              '設置部署
        Master.eraseCharToIgnore(WF_OILTYPE1.Text)          '油種(1)
        Master.eraseCharToIgnore(WF_OILTYPE2.Text)          '油種(2)
        Master.eraseCharToIgnore(WF_OWNCODEF.Text)          '荷主(From)
        Master.eraseCharToIgnore(WF_OWNCODET.Text)          '荷主(To)
        Master.eraseCharToIgnore(WF_SHARYOTYPE1.Text)       '車両タイプ(1)
        Master.eraseCharToIgnore(WF_SHARYOTYPE2.Text)       '車両タイプ(2)
        Master.eraseCharToIgnore(WF_SHARYOTYPE3.Text)       '車両タイプ(3)
        Master.eraseCharToIgnore(WF_SHARYOTYPE4.Text)       '車両タイプ(4)
        Master.eraseCharToIgnore(WF_SHARYOTYPE5.Text)       '車両タイプ(5)

        '○ チェック処理
        WW_Check(WW_ERR_SW)

        '○ 名称設定
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                 '会社コード
        CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_DUMMY)                             '管理部署
        CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_DUMMY)                             '設置部署
        CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_DUMMY)                  '油種(1)
        CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_DUMMY)                  '油種(2)
        CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_DUMMY)                  '荷主(From)
        CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_DUMMY)                  '荷主(To)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_DUMMY)         '車両タイプ(1)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_DUMMY)         '車両タイプ(2)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_DUMMY)         '車両タイプ(3)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_DUMMY)         '車両タイプ(4)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_DUMMY)         '車両タイプ(5)

    End Sub

    ' ******************************************************************************
    ' ***  rightBOX関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '○右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ' ***  初期値設定処理
    Protected Sub WW_MAPValueSet()

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移
            '○ワーク初期化
            work.Initialize()

            '○初期変数設定処理
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "YYF", WF_YYF.Text)                         '年度(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "YYT", WF_YYT.Text)                         '年度(To)
            '○初期変数設定処理
            '年度(From)
            If Len(WF_YYF.Text) <> 4 AndAlso Len(WF_YYF.Text) <> 0 Then
                '変数がyyyy形式設定以外の場合
                Dim WW_date As Date
                Try
                    Date.TryParse(WF_YYF.Text, WW_date)
                Catch ex As Exception
                    WW_date = C_DEFAULT_YMD
                End Try

                If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
                    WF_YYF.Text = (WW_date.Year - 1).ToString()
                Else
                    WF_YYF.Text = (WW_date.Year).ToString()
                End If
            End If
            '年度(To)
            If Len(WF_YYT.Text) <> 4 AndAlso Len(WF_YYT.Text) <> 0 Then
                '変数がyyyy形式設定以外の場合
                Dim WW_date As Date
                Try
                    Date.TryParse(WF_YYT.Text, WW_date)
                Catch ex As Exception
                    WW_date = C_DEFAULT_YMD
                End Try

                If WW_date.ToString("MM") = "01" OrElse WW_date.ToString("MM") = "02" OrElse WW_date.ToString("MM") = "03" Then
                    WF_YYT.Text = (WW_date.Year - 1).ToString()
                Else
                    WF_YYT.Text = (WW_date.Year).ToString()
                End If
            End If

            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)               '会社コード
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "MORG", WF_MORG.Text)                       '管理部署
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SORG", WF_SORG.Text)                       '設置部署
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE1", WF_OILTYPE1.Text)               '油種(1)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE2", WF_OILTYPE2.Text)               '油種(2)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "OWNCODEF", WF_OWNCODEF.Text)               '荷主(From)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "OWNCODET", WF_OWNCODET.Text)               '荷主(To)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE1", WF_SHARYOTYPE1.Text)         '車両タイプ(1)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE2", WF_SHARYOTYPE2.Text)         '車両タイプ(2)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE3", WF_SHARYOTYPE3.Text)         '車両タイプ(3)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE4", WF_SHARYOTYPE4.Text)         '車両タイプ(4)
            Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPE5", WF_SHARYOTYPE5.Text)         '車両タイプ(5)

            '○RightBox情報設定
            rightview.MAPID = GRMA0004WRKINC.MAPID
            rightview.MAPIDS = GRMA0004WRKINC.MAPIDS
            rightview.COMPCODE = WF_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MA0004 Then         '実行画面からの画面遷移

            '○画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text                '会社コード
            WF_YYF.Text = work.WF_SEL_YYF.Text                          '年度(From)
            WF_YYT.Text = work.WF_SEL_YYT.Text                          '年度(To)
            WF_MORG.Text = work.WF_SEL_MORG.Text                        '管理部署
            WF_SORG.Text = work.WF_SEL_SORG.Text                        '設置部署
            WF_OILTYPE1.Text = work.WF_SEL_OILTYPE1.Text                '油種(1)
            WF_OILTYPE2.Text = work.WF_SEL_OILTYPE2.Text                '油種(2)
            WF_OWNCODEF.Text = work.WF_SEL_OWNCODE1.Text                '荷主(From)
            WF_OWNCODET.Text = work.WF_SEL_OWNCODE2.Text                '荷主(To)
            WF_SHARYOTYPE1.Text = work.WF_SEL_SHARYOTYPE1.Text          '車両タイプ(1)
            WF_SHARYOTYPE2.Text = work.WF_SEL_SHARYOTYPE2.Text          '車両タイプ(2)
            WF_SHARYOTYPE3.Text = work.WF_SEL_SHARYOTYPE3.Text          '車両タイプ(3)
            WF_SHARYOTYPE4.Text = work.WF_SEL_SHARYOTYPE4.Text          '車両タイプ(4)
            WF_SHARYOTYPE5.Text = work.WF_SEL_SHARYOTYPE5.Text          '車両タイプ(5)

            '○RightBox情報設定
            rightview.MAPID = GRMA0004WRKINC.MAPID
            rightview.MAPIDS = GRMA0004WRKINC.MAPIDS
            rightview.COMPCODE = WF_CAMPCODE.Text
            rightview.MAPVARI = Master.MAPvariant
            rightview.PROFID = Master.PROF_VIEW
            rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If
        End If

        '○ 名称設定
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)                 '会社コード
        CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_DUMMY)                             '管理部署
        CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_DUMMY)                             '設置部署
        CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_DUMMY)                  '油種(1)
        CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_DUMMY)                  '油種(2)
        CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_DUMMY)                  '荷主(From)
        CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_DUMMY)                  '荷主(To)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_DUMMY)         '車両タイプ(1)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_DUMMY)         '車両タイプ(2)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_DUMMY)         '車両タイプ(3)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_DUMMY)         '車両タイプ(4)
        CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_DUMMY)         '車両タイプ(5)

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)


        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 入力項目チェック
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""

        '会社コード WF_CAMPCODE 
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_CAMPCODE.Text <> "" Then
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '年度 WF_YYF.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "YYF", WF_YYF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_YYF.Text <> "" AndAlso (WF_YYF.Text <= "2000" OrElse WF_YYF.Text >= "2099") Then
                Master.output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)
                WF_YYF.Focus()
                O_RTN = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_YYF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '年度 WF_YYT.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "YYT", WF_YYT.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_YYT.Text <> "" AndAlso (WF_YYT.Text <= "2000" OrElse WF_YYT.Text >= "2099") Then
                '範囲エラー
                Master.output(C_MESSAGE_NO.NUMBER_RANGE_ERROR, C_MESSAGE_TYPE.ERR)
                WF_YYT.Focus()
                O_RTN = C_MESSAGE_NO.NUMBER_RANGE_ERROR
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_YYT.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_YYF.Text <> "" AndAlso WF_YYT.Text <> "" Then
            Dim WW_YYF As Integer = 0
            Dim WW_YYT As Integer = 0
            Try
                Integer.TryParse(WF_YYF.Text, WW_YYF)
                Integer.TryParse(WF_YYT.Text, WW_YYT)
                If WW_YYF > WW_YYT Then
                    Master.output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                    WF_YYF.Focus()
                    O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                    Exit Sub
                End If
            Catch ex As Exception
            End Try
        End If

        '管理部署 WF_MORG 
        Master.checkFIeld(WF_CAMPCODE.Text, "MORG", WF_MORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_MORG.Text <> "" Then
                CODENAME_get("MORG", WF_MORG.Text, WF_MORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_MORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_MORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '設置部署 WF_SORG
        Master.checkFIeld(WF_CAMPCODE.Text, "SORG", WF_SORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SORG.Text <> "" Then
                CODENAME_get("SORG", WF_SORG.Text, WF_SORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '油種 WF_OILTYPE1
        Master.checkFIeld(WF_CAMPCODE.Text, "OILTYPE1", WF_OILTYPE1.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE1.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE1.Text, WF_OILTYPE1_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE1.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE1.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '油種 WF_OILTYPE2
        Master.checkFIeld(WF_CAMPCODE.Text, "OILTYPE2", WF_OILTYPE2.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE2.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE2.Text, WF_OILTYPE2_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE2.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE2.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '荷主 WF_OWNCODEF
        Master.checkFIeld(WF_CAMPCODE.Text, "OWNCONT1", WF_OWNCODEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OWNCODEF.Text <> "" Then
                CODENAME_get("OWNCONT", WF_OWNCODEF.Text, WF_OWNCODEF_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OWNCODEF.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '荷主 WF_OWNCODET
        Master.checkFIeld(WF_CAMPCODE.Text, "OWNCONT2", WF_OWNCODET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OWNCODET.Text <> "" Then
                CODENAME_get("OWNCONT", WF_OWNCODET.Text, WF_OWNCODET_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OWNCODET.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_OWNCODET.Text <> "" AndAlso WF_OWNCODEF.Text <> "" AndAlso
           WF_OWNCODET.Text < WF_OWNCODEF.Text Then
            Master.output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
            WF_OWNCODEF.Focus()
            O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE1
        Master.checkFIeld(WF_CAMPCODE.Text, "SHARYOTYPE1", WF_SHARYOTYPE1.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE1.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE1.Text, WF_SHARYOTYPE1_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE1.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE1.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If


        '車両タイプ WF_SHARYOTYPE2
        Master.checkFIeld(WF_CAMPCODE.Text, "SHARYOTYPE2", WF_SHARYOTYPE2.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE2.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE2.Text, WF_SHARYOTYPE2_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE2.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE2.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE3
        Master.checkFIeld(WF_CAMPCODE.Text, "SHARYOTYPE3", WF_SHARYOTYPE3.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE3.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE3.Text, WF_SHARYOTYPE3_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE3.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE3.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE4
        Master.checkFIeld(WF_CAMPCODE.Text, "SHARYOTYPE4", WF_SHARYOTYPE4.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE4.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE4.Text, WF_SHARYOTYPE4_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE4.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE4.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '車両タイプ WF_SHARYOTYPE5
        Master.checkFIeld(WF_CAMPCODE.Text, "SHARYOTYPE5", WF_SHARYOTYPE5.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHARYOTYPE5.Text <> "" Then
                CODENAME_get("SHARYOTYPE", WF_SHARYOTYPE5.Text, WF_SHARYOTYPE5_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHARYOTYPE5.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHARYOTYPE5.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If
        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub


    ' ******************************************************************************
    ' ***  サブルーチン                                                          ***
    ' ******************************************************************************


    ''' <summary>
    ''' 左リストボックスより名称取得とチェックを行う
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        If I_VALUE <> "" Then
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "MORG"             '管理部署
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, True))
                Case "SORG"             '設置部署
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, False))
                Case "OILTYPE"          '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(WF_CAMPCODE.Text))
                Case "OWNCONT"          '荷主
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, work.CreateTODOParam(WF_CAMPCODE.Text))
                Case "SHARYOTYPE"       '車両タイプ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(WF_CAMPCODE.Text, "SHARYOTYPE"))
            End Select
        End If

    End Sub

End Class

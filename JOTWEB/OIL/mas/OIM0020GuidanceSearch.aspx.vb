Option Strict On
Imports JOTWEB.GRIS0005LeftBox
Imports JOTWEB.GRC0001TILESELECTORWRKINC
''' <summary>
''' ガイダンス検索画面クラス
''' </summary>
Public Class OIM0020GuidanceSearch
    Inherits System.Web.UI.Page

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
        Master.MAPID = OIM0020WRKINC.MAPIDS

        txtFromYmd.Focus()
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
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)               '会社コード
            Master.GetFirstValue(work.WF_SEL_ORG.Text, "ORG", WF_ORG.Text)                              '組織コード
            Master.GetFirstValue(work.WF_SEL_FROMYMD.Text, "FROMYMD", txtFromYmd.Text)    'JOT車番
            Master.GetFirstValue(work.WF_SEL_ENDYMD.Text, "ENDYMD", txtEndYmd.Text)                   '型式
            Dim chklList = work.GetNewDisplayFlags()
            If Not {"jot_sys_1", "jot_oil_1"}.Contains(Master.ROLE_MAP) Then
                Dim prmData = work.CreateFIXParam(Master.USER_ORG)
                leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_BELONGTOOFFICE, WW_DUMMY, prmData)
                Dim officeCodes = (From litm As ListItem In leftview.WF_LeftListBox.Items.Cast(Of ListItem) Select litm.Value)
                chklList = (From itm In chklList Where officeCodes.Contains(itm.OfficeCode)).ToList
            End If
            If chklList IsNot Nothing AndAlso chklList.Count <> 0 Then
                chklList = (From itm In chklList Order By itm.DispOrder).ToList
            End If
            work.WF_SEL_DISPFLAGS_LIST.Text = work.EncodeDisplayFlags(chklList)
            Me.chklFlags.DataSource = chklList
            Me.chklFlags.DataTextField = "DispName"
            Me.chklFlags.DataValueField = "FieldName"
            Me.chklFlags.DataBind()
            'Master.GetFirstValue(work.WF_SEL_USEDFLG.Text, "USEPROPRIETY", WF_USEDFLG_CODE.Text)        '利用フラグ
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0020L Then   '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text                      '組織コード
            txtFromYmd.Text = work.WF_SEL_FROMYMD.Text   'JOT車番
            txtEndYmd.Text = work.WF_SEL_ENDYMD.Text             '型式
            Dim chklList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)
            Me.chklFlags.DataSource = chklList
            Me.chklFlags.DataTextField = "DispName"
            Me.chklFlags.DataValueField = "FieldName"
            Me.chklFlags.DataBind()

        End If

        ''JOT車番・利用フラグを入力するテキストボックスは数値(0～9)のみ可能とする。
        'Me.WF_TANKNUMBER_CODE.Attributes("onkeyPress") = "CheckNum()"
        'Me.WF_USEDFLG_CODE.Attributes("onkeyPress") = "CheckNum()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIM0020WRKINC.MAPIDS
        rightview.MAPID = OIM0020WRKINC.MAPIDL
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        '201104-追加-START
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF
        '201104-追加-END

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        'CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)                            '組織コード
    End Sub
    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text          '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text                    '組織コード
        work.WF_SEL_FROMYMD.Text = txtFromYmd.Text
        work.WF_SEL_ENDYMD.Text = txtEndYmd.Text
        Dim chklList = work.DecodeDisplayFlags(work.WF_SEL_DISPFLAGS_LIST.Text)
        chklList = work.SetSelectedDispFlags(Me.chklFlags, chklList)
        work.WF_SEL_DISPFLAGS_LIST.Text = work.EncodeDisplayFlags(chklList)
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
        Dim WW_LINEERR_SW As String = ""
        Dim WW_DUMMY As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_LINE_ERR As String = ""

        '○ 単項目チェック
        '掲載開始日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FROMYMD", txtFromYmd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            'ポップアップを表示(needsPopUp:=True)
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "掲載開始日", needsPopUp:=True)
            txtFromYmd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '掲載終了日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", txtEndYmd.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            'ポップアップを表示(needsPopUp:=True)
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "掲載終了日", needsPopUp:=True)
            txtEndYmd.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        If txtFromYmd.Text <> "" AndAlso txtEndYmd.Text <> "" Then
            Dim fromDtm As Date = CDate(txtFromYmd.Text)
            Dim toDtm As Date = CDate(txtEndYmd.Text)
            If fromDtm > toDtm Then
                'ポップアップを表示(needsPopUp:=True)
                Master.Output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                txtFromYmd.Focus()
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
                Integer.Parse(WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                If enumVal = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_FROMYMD"
                            .WF_Calendar.Text = txtFromYmd.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = txtEndYmd.Text
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

    End Sub


    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()


        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_FROMYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        txtFromYmd.Text = ""
                    Else
                        txtFromYmd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                txtFromYmd.Focus()
            Case "WF_ENDYMD"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                        txtEndYmd.Text = ""
                    Else
                        txtEndYmd.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                txtEndYmd.Focus()
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
            Case "WF_FROMYMD"
                txtFromYmd.Focus()
            Case "WF_ENDYMD"
                txtEndYmd.Focus()
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
    End Sub
    ''' <summary>
    ''' チェックボックスデータバインド時イベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>チェックの状態を設定する</remarks>
    Private Sub chklFlags_DataBinding(sender As Object, e As EventArgs) Handles chklFlags.DataBinding
        Dim chklObj As CheckBoxList = DirectCast(sender, CheckBoxList)
        Dim chkBindItm As List(Of OIM0020WRKINC.DisplayFlag) = DirectCast(chklObj.DataSource, List(Of OIM0020WRKINC.DisplayFlag))
        For i = 0 To chklObj.Items.Count - 1 Step 1
            chklObj.Items(i).Selected = chkBindItm(i).Checked
        Next
    End Sub

End Class
'Option Strict On
'Option Explicit On

Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 受注業務検索画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OrderBusinessSearch
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
                    'Case "WF_Field_DBClick"             'フィールドダブルクリック
                    '    WF_FIELD_DBClick()
                    'Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                    '    WF_FIELD_Change()
                    'Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                    '    WF_ButtonSel_Click()
                    'Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                    '    WF_ButtonCan_Click()
                    'Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                    '    WF_ButtonSel_Click()
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
        Master.MAPID = OIT0003WRKINC.MAPIDB
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'メニューからの画面遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", Me.WF_CAMPCODE.Text)
            '運用部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", Me.WF_UORG.Text)

            '一覧画面からの遷移
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003L Then
            '〇画面項目設定処理
            '会社コード
            Me.WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '運用部署
            Me.WF_UORG.Text = work.WF_SEL_UORG.Text

        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0003WRKINC.MAPIDB
        rightview.MAPID = OIT0003WRKINC.MAPIDL
        rightview.COMPCODE = Me.WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", Me.WF_CAMPCODE.Text, Me.WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", Me.WF_UORG.Text, Me.WF_UORG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        '会社コード
        Master.EraseCharToIgnore(Me.WF_CAMPCODE.Text)
        '運用部署
        Master.EraseCharToIgnore(Me.WF_UORG.Text)

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        '会社コード
        work.WF_SEL_CAMPCODE.Text = Me.WF_CAMPCODE.Text
        '運用部署
        work.WF_SEL_UORG.Text = Me.WF_UORG.Text

        '営業所
        '※選択された営業所の判断
        If rbTohokuSendai.Checked = True Then                   '東北(仙台新港)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "010402"
            work.WF_SEL_SALESOFFICECODE.Text = "010402"
            work.WF_SEL_SALESOFFICE.Text = "仙台新港営業所"
        ElseIf rbKantoGoi.Checked = True Then                   '関東(五井)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "011201"
            work.WF_SEL_SALESOFFICECODE.Text = "011201"
            work.WF_SEL_SALESOFFICE.Text = "五井営業所"
        ElseIf rbKantoKinoene.Checked = True Then               '関東(甲子)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "011202"
            work.WF_SEL_SALESOFFICECODE.Text = "011202"
            work.WF_SEL_SALESOFFICE.Text = "甲子営業所"
        ElseIf rbKantoSodegaura.Checked = True Then             '関東(袖ヶ浦)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "011203"
            work.WF_SEL_SALESOFFICECODE.Text = "011203"
            work.WF_SEL_SALESOFFICE.Text = "袖ヶ浦営業所"
        ElseIf rbKantoNegishi.Checked = True Then               '関東(根岸)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "011402"
            work.WF_SEL_SALESOFFICECODE.Text = "011402"
            work.WF_SEL_SALESOFFICE.Text = "根岸営業所"
        ElseIf rbChubuYokkaichi.Checked = True Then             '中部(四日市)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "012401"
            work.WF_SEL_SALESOFFICECODE.Text = "012401"
            work.WF_SEL_SALESOFFICE.Text = "四日市営業所"
        ElseIf rbChubuMieShiohama.Checked = True Then           '中部(三重塩浜)
            work.WF_SEL_SALESOFFICECODEMAP.Text = "012402"
            work.WF_SEL_SALESOFFICECODE.Text = "012402"
            work.WF_SEL_SALESOFFICE.Text = "三重塩浜営業所"
        End If

        'オーダー
        '※選択された日付の判断((予定)発日の運行予定)
        If rbRunDay.Checked = True Then                         '当日運行
            work.WF_SEL_SEARCH_DEPDATE.Text = DateTime.Today
        ElseIf rbRunNextDay.Checked = True Then                 '翌日運行
            work.WF_SEL_SEARCH_DEPDATE.Text = DateTime.Today.AddDays(1)
        ElseIf rbRunTwoDayLater.Checked = True Then             '翌々日以降
            work.WF_SEL_SEARCH_DEPDATE.Text = DateTime.Today.AddDays(2)
        End If

        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(Me.WF_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(Me.WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.TransitionPage()
        End If

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
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

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

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

End Class
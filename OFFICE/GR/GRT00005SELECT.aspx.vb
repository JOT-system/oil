Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox

Public Class GRT00005SELECT
    Inherits Page

    '共通関数宣言(BASEDLL)
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                      'セッション管理
    '共通処理結果
    ''' <summary>
    ''' 共通用エラーID保持枠
    ''' </summary>
    Private WW_ERR_SW As String                                     '
    ''' <summary>
    ''' 共通用戻値保持枠
    ''' </summary>
    Private WW_RTN_SW As String                                     '
    ''' <summary>
    ''' 共通用引数虚数設定用枠（使用は非推奨）
    ''' </summary>
    Private WW_DUMMY As String                                      '

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then
            '■■■ 各ボタン押下処理 ■■■
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonRESTART"                         '■再開ボタン押下時処理
                        WF_ButtonRESTART_Click()
                    Case "WF_ButtonDO"                              '■実行ボタン押下時処理
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                             '■終了ボタン押下時処理
                        WF_ButtonEND_Click()
                    Case "WF_ButtonSel"                             '■左ボックス選択ボタン押下時処理
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                             '■左ボックスキャンセルボタン押下時処理
                        WF_ButtonCan_Click()
                    Case "WF_Field_DBClick"                         '■入力領域ダブルクリック時処理
                        WF_Field_DBClick()
                    Case "WF_TextChange"                            '■入力領域変更時処理
                        WW_LeftBoxReSet()
                    Case "WF_ListboxDBclick"                        '■左ボックス選択時処理
                        WF_LEFTBOX_DBClick()
                    Case "WF_LeftBoxSelectClick"                    '■左ボックス選択処理
                        WF_LEFTBOX_SELECT_Click()
                    Case "WF_RIGHT_VIEW_DBClick"                    '■右ボックス表示時処理
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                            '■右ボックスメモ欄変更時処理
                        WF_RIGHTBOX_Change()
                    Case "WF_ButtonCHECK"                           '■確認ボタンクリック時処理
                        WF_ButtonCHECK()
                    Case "WF_BEFORE"                                '■過去月遷移ボタンクリック時処理
                        WF_BFOREYMDCHECK()
                    Case "WF_AFTER"                                 '■未来月遷移ボタンクリック時処理
                        WF_AFTERYMDCHECK()
                End Select
            End If
        Else
            '初期化処理
            Initialize()
        End If
        '〇カレンダーの作成
        getImportData()
    End Sub
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

        '○初期値設定
        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '■■■ 選択画面の入力初期値設定 ■■■　…　画面固有処理
        SetMapValue(WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then Exit Sub
        '〇日報作成済一覧作成
        getNIPPODAYList()
        '再開ボタン制御
        If System.IO.File.Exists(work.WF_T5_XMLsaveTmp.Text) Then
            WF_Restart.Value = "TRUE"
        Else
            WF_Restart.Value = "FALSE"
        End If

    End Sub
    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 再開ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRESTART_Click()

        '■■■ チェック処理 ■■■
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub
        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '出庫日　
        work.WF_SEL_STYMD.Text = WF_STYMD.Text
        work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        '運用部署
        work.WF_SEL_UORG.Text = WF_UORG.Text
        '従業員コード
        work.WF_SEL_STAFFCODE.Text = WF_STAFFCODE.Text
        '従業員名
        work.WF_SEL_STAFFNAME.Text = WF_STAFFNAME.Text
        '取込確認年月
        work.WF_SEL_IMPYM.Text = WF_IMPYM.Text

        work.WF_SEL_VIEWID.Text = rightview.getViewId(WF_CAMPCODE.Text)
        work.WF_SEL_VIEWID_DTL.Text = rightview.getViewDtlId(WF_CAMPCODE.Text)
        work.WF_SEL_BUTTON.Text = GRT00005WRKINC.LC_BTN_TYPE.BTN_RESTART
        '■■■ 画面遷移先URL取得 ■■■
        Master.VIEWID = work.WF_SEL_VIEWID.Text
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        work.WF_SEL_MAPvariant.Text = Master.MAPvariant
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.transitionPage()
        End If
    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '■■■ チェック処理 ■■■
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■■■ セッション変数　反映 ■■■

        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '出庫日　
        work.WF_SEL_STYMD.Text = WF_STYMD.Text
        If WF_ENDYMD.Text = "" Then
            work.WF_SEL_ENDYMD.Text = WF_STYMD.Text
        Else
            work.WF_SEL_ENDYMD.Text = WF_ENDYMD.Text
        End If
        '運用部署
        work.WF_SEL_UORG.Text = WF_UORG.Text
        '従業員コード
        work.WF_SEL_STAFFCODE.Text = WF_STAFFCODE.Text
        '従業員名
        work.WF_SEL_STAFFNAME.Text = WF_STAFFNAME.Text
        '取込確認年月
        work.WF_SEL_IMPYM.Text = WF_IMPYM.Text

        work.WF_SEL_VIEWID.Text = rightview.getViewId(WF_CAMPCODE.Text)
        work.WF_SEL_VIEWID_DTL.Text = rightview.getViewDtlId(WF_CAMPCODE.Text)
        work.WF_SEL_BUTTON.Text = GRT00005WRKINC.LC_BTN_TYPE.BTN_DO

        '■■■ 画面遷移先URL取得 ■■■
        Master.VIEWID = work.WF_SEL_VIEWID.Text
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        work.WF_SEL_MAPvariant.Text = Master.MAPvariant
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.transitionPage()
        End If

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇フィールドダブルクリック時処理
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "WF_STYMD"
                            .WF_Calendar.Text = WF_STYMD.Text
                        Case "WF_ENDYMD"
                            .WF_Calendar.Text = WF_STYMD.Text

                    End Select
                    .activeCalendar()
                ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_STAFFCODE Then
                    '従業員の場合テーブル表記する
                    Dim prmData As Hashtable = work.CreateSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text, WF_STYMD.Text, WF_ENDYMD.Text)
                    .seTTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeTable()
                    WF_LeftboxOpen.Value = "OpenTbl"
                Else
                    Dim prmData As Hashtable = work.CreateFIXParam(WF_CAMPCODE.Text)

                    If WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_ORG Then
                        prmData = work.createORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE)
                    ElseIf WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_STAFFCODE Then
                        prmData = work.CreateSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text, WF_STYMD.Text, WF_ENDYMD.Text)
                    End If
                    .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .activeListBox()
                End If
            End With
        End If

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '〇ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' '〇TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_Click()
        WW_LeftBoxReSet()
    End Sub
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
        '〇右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 確認ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonCHECK()
        Dim wDATE As Date
        Try
            wDATE = WF_STYMD.Text
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        WF_IMPYM.Text = wDATE.Year & "/" & wDATE.Month.ToString("00")
        '〇日報作成済一覧作成
        getNIPPODAYList()
    End Sub
    ''' <summary>
    ''' 確認ボタン押下時処理
    ''' </summary>
    Protected Sub WF_BFOREYMDCHECK()
        Dim wDATE As Date
        Try
            wDATE = WF_IMPYM.Text & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        wDATE = wDATE.AddMonths(-1)
        WF_IMPYM.Text = wDATE.Year & "/" & wDATE.Month.ToString("00")
        '〇日報作成済一覧作成
        getNIPPODAYList()
    End Sub
    ''' <summary>
    ''' 確認ボタン押下時処理
    ''' </summary>
    Protected Sub WF_AFTERYMDCHECK()
        Dim wDATE As Date
        Try
            wDATE = WF_IMPYM.Text & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        wDATE = wDATE.AddMonths(1)
        WF_IMPYM.Text = wDATE.Year & "/" & wDATE.Month.ToString("00")
        '〇日報作成済一覧作成
        getNIPPODAYList()
    End Sub
    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()


        Dim values As String() = leftview.getActiveValue

        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"
                '会社コード　 
                WF_CAMPCODE_Text.Text = values(1)
                WF_CAMPCODE.Text = values(0)
                WF_CAMPCODE.Focus()

            Case "WF_STYMD"
                '出庫日(FROM)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_STYMD.Text = ""
                    Else
                        WF_STYMD.Text = values(0)
                    End If
                Catch ex As Exception
                End Try
                WF_STYMD.Focus()

            Case "WF_ENDYMD"
                '出庫日(TO)
                Dim WW_DATE As Date
                Try
                    Date.TryParse(values(0), WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        WF_ENDYMD.Text = ""
                    Else
                        WF_ENDYMD.Text = values(0)
                    End If
                Catch ex As Exception

                End Try
                WF_ENDYMD.Focus()

            Case "WF_UORG"
                '運用部署
                WF_UORG_Text.Text = values(1)
                WF_UORG.Text = values(0)
                WF_UORG.Focus()

            Case "WF_STAFFCODE"
                '従業員 
                For Each Data As String In values
                    Select Case Data.Split("=")(0)
                        Case "CODE"
                            WF_STAFFCODE.Text = Data.Split("=")(1)
                        Case "NAMES"
                            WF_STAFFCODE_Text.Text = Data.Split("=")(1)
                    End Select
                Next
                WF_STAFFCODE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"
                '会社コード　 
                WF_CAMPCODE.Focus()

            Case "WF_UORG"
                '受注部署 
                WF_UORG.Focus()

            Case "WF_STAFFCODE"
                '出荷部署　　 
                WF_STAFFCODE.Focus()

        End Select

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_LeftboxOpen.Value = ""
        WF_FIELD.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""
        WF_UORG_Text.Text = ""
        WF_STAFFCODE_Text.Text = ""

        '■■■ チェック処理 ■■■
        CheckParameters(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then Exit Sub

        '■名称設定
        SetNameValue()

    End Sub
    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub SetMapValue(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        '■■■ 選択画面の入力初期値設定 ■■■
        If IsNothing(Master.MAPID) Then
            Master.MAPID = GRT00005WRKINC.MAPIDS
        End If
        'メニューからの画面遷移
        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then
            work.Initialize()
            '権限、変数
            work.WF_SEL_MAPvariant.Text = Master.MAPvariant
            work.WF_SEL_MAPpermitcode.Text = Master.MAPpermitcode
            work.WF_SEL_PERMIT_ORG.Text = Master.USER_ORG
            '一時保存パス
            work.WF_T5_XMLsaveTmp.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-T00005I-TMP.txt"
            work.WF_T5_XMLsaveTmp9.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-T00005I9-TMP.txt"
            work.WF_T5_XMLsavePARM.Text = CS0050Session.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-T00005S-PARM.txt"
            'パラメータファイルが存在する場合はパラメータを取得する
            Dim T0005PARMtbl As DataTable = New DataTable

            If System.IO.File.Exists(work.WF_T5_XMLsavePARM.Text) Then
                If Not Master.RecoverTable(T0005PARMtbl, work.WF_T5_XMLsavePARM.Text) Then
                    O_RTN = C_MESSAGE_NO.FILE_IO_ERROR
                    Exit Sub
                End If

                With T0005PARMtbl.Rows(0)
                    '会社コード　
                    work.WF_SEL_CAMPCODE.Text = .Item("CAMPCODE")
                    '出庫年月日開始　
                    work.WF_SEL_STYMD.Text = .Item("STYMD")
                    '出庫年月日終了　
                    work.WF_SEL_ENDYMD.Text = .Item("ENDYMD")
                    '運用部署
                    work.WF_SEL_UORG.Text = .Item("UORG")
                    '従業員コード
                    work.WF_SEL_STAFFCODE.Text = .Item("STAFFCODE")
                    work.WF_SEL_STAFFNAME.Text = .Item("STAFFNAME")
                    '取込確認年月
                    work.WF_SEL_IMPYM.Text = .Item("IMPYM")
                End With
                '会社コード　
                WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
                '出庫日　
                WF_STYMD.Text = work.WF_SEL_STYMD.Text
                WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text
                '運用部署
                WF_UORG.Text = work.WF_SEL_UORG.Text
                '従業員コード
                WF_STAFFCODE.Text = work.WF_SEL_STAFFCODE.Text
                '従業員名称
                WF_STAFFNAME.Text = work.WF_SEL_STAFFNAME.Text
                '取込確認年月
                WF_IMPYM.Text = work.WF_SEL_IMPYM.Text
            Else
                '○画面項目設定（変数より）処理
                SetInitialValue()
            End If

            '実行画面からの画面遷移
        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00005I Then                                                   '実行画面からの画面遷移
            '■■■ 実行画面からの画面遷移 ■■■
            '○画面項目設定処理
            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '出庫日　
            WF_STYMD.Text = work.WF_SEL_STYMD.Text
            WF_ENDYMD.Text = work.WF_SEL_ENDYMD.Text
            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            '従業員コード
            WF_STAFFCODE.Text = work.WF_SEL_STAFFCODE.Text
            '従業員名称
            WF_STAFFNAME.Text = work.WF_SEL_STAFFNAME.Text
            '取込確認年月
            WF_IMPYM.Text = work.WF_SEL_IMPYM.Text
        End If

        '○RightBox情報設定
        rightview.MAPID = GRT00005WRKINC.MAPIDI
        rightview.MAPID_DTL = GRT00005WRKINC.MAPID
        rightview.MAPIDS = GRT00005WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定（一覧画面）", "画面レイアウト設定（明細画面）", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '■名称設定
        SetNameValue()

    End Sub

    ''' <summary>
    ''' 変数設定用処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetInitialValue()

        '■■■ 変数設定処理 ■■■
        Master.XMLsaveF = work.WF_SEL_MAPvariant.Text
        '年度(From)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STYMD", WF_STYMD.Text)
        '年度(To)
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "ENDYMD", WF_ENDYMD.Text)
        '会社コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
        '運用部署
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", WF_UORG.Text)
        '従業員コード
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text)
        '従業員名称
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "STAFFNAME", WF_STAFFNAME.Text)
        '日報確認日付
        Dim wDATE As Date
        Try
            wDATE = WF_STYMD.Text
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        WF_IMPYM.Text = wDATE.Year & "/" & wDATE.Month.ToString("00")
    End Sub

    ''' <summary>
    ''' 名称設定処理      
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub SetNameValue()

        '■名称設定
        '会社コード　
        CodeToName("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)
        '出荷部署
        CodeToName("UORG", WF_UORG.Text, WF_UORG_Text.Text, WW_DUMMY)
        '乗務員
        CodeToName("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_Text.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CodeToName(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(I_VALUE) Then
            Select Case I_FIELD
                Case "CAMPCODE" '会社コード
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
                Case "UORG" '部署
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))                     '部署
                Case "STAFFCODE" '乗務員
                    leftview.CodeToName(GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, work.CreateSTAFFParam(WF_CAMPCODE.Text, String.Empty, WF_STYMD.Text, WF_ENDYMD.Text))               '従業員
            End Select
        End If

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckParameters(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        '■■■ 入力文字置き換え ■■■
        '   画面PassWord内の使用禁止文字排除
        '会社コード WF_CAMPCODE.Text
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)
        '出庫日(FROM) WF_STYMD.Text
        Master.EraseCharToIgnore(WF_STYMD.Text)
        '出庫日(TO) WF_ENDYMD.Text
        Master.EraseCharToIgnore(WF_ENDYMD.Text)
        '運用部署 WF_UORG.Text
        Master.EraseCharToIgnore(WF_UORG.Text)
        '従業員コード WF_STAFFCODE.Text
        Master.EraseCharToIgnore(WF_STAFFCODE.Text)
        '従業員名 WF_STAFFNAME.Text
        Master.EraseCharToIgnore(WF_STAFFNAME.Text)
        '■■■ 入力項目チェック ■■■
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CHECK As String = ""
        WF_FIELD.Value = ""

        '●会社コード WF_CAMPCODE.Text
        '①単項目チェック
        WW_CHECK = WF_CAMPCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "CAMPCODE", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If Not String.IsNullOrEmpty(WF_CAMPCODE.Text) Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, O_RTN)
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_CAMPCODE.Focus()
            Exit Sub
        End If

        '●出庫日(FROM) WF_STYMD.Text
        '①単項目チェック
        Dim WW_STYMD As Date
        WW_CHECK = WF_STYMD.Text
        Master.CheckField(WF_CAMPCODE.Text, "STYMD", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WW_CHECK, WW_STYMD)
                WF_STYMD.Text = WW_CHECK
            Catch ex As Exception
                WF_STYMD.Text = C_DEFAULT_YMD
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STYMD.Focus()
            Exit Sub
        End If

        '●出庫日(TO) WF_ENDYMD.Text
        '①単項目チェック
        Dim WW_ENDYMD As Date
        WW_CHECK = WF_ENDYMD.Text
        Master.CheckField(WF_CAMPCODE.Text, "ENDYMD", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(WW_CHECK, WW_ENDYMD)
                WF_ENDYMD.Text = WW_CHECK
            Catch ex As Exception
                WF_ENDYMD.Text = C_MAX_YMD
                WW_ENDYMD = C_MAX_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_ENDYMD.Focus()
            Exit Sub
        End If

        '②関連チェック(開始＞終了)
        If WF_STYMD.Text <> "" And WF_ENDYMD.Text <> "" Then
            If WW_STYMD > WW_ENDYMD Then
                Master.Output(C_MESSAGE_NO.START_END_RELATION_ERROR, C_MESSAGE_TYPE.ERR)
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                WF_STYMD.Focus()
                Exit Sub
            End If
        End If

        '●運用部署 WF_UORG.Text
        '①単項目チェック
        WW_CHECK = WF_UORG.Text
        Master.CheckField(WF_CAMPCODE.Text, "UORG", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If WF_UORG.Text <> "" Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, WF_UORG.Text, WF_UORG_Text.Text, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, C_PERMISSION.REFERLANCE))
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    WF_UORG.Focus()
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_UORG.Focus()
            Exit Sub
        End If

        '●従業員 WF_STAFFCODE.Text
        '①単項目チェック
        WW_CHECK = WF_STAFFCODE.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFCODE", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '③存在チェック(LeftBoxチェック)
            If WF_STAFFCODE.Text <> "" Then
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, WF_STAFFCODE.Text, WF_STAFFCODE_Text.Text, O_RTN, work.CreateSTAFFParam(WF_CAMPCODE.Text, WF_UORG.Text, WF_STYMD.Text, WF_ENDYMD.Text))
                If Not isNormal(O_RTN) Then
                    Master.Output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    WF_UORG.Focus()
                    Exit Sub
                End If
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STAFFCODE.Focus()
            Exit Sub
        End If
        '●従業員名 WF_STAFFNAME
        '①単項目チェック
        WW_CHECK = WF_STAFFNAME.Text
        Master.CheckField(WF_CAMPCODE.Text, "STAFFNAMES", WW_CHECK, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            O_RTN = WW_CS0024FCHECKERR
            WF_STAFFCODE.Focus()
            Exit Sub
        End If

        '正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)
    End Sub
    ''' <summary>
    ''' 日報作成済日付の取得
    ''' </summary>
    Protected Sub GetNippoDayList()
        Using SqlCon As SqlConnection = CS0050Session.getConnection
            SqlCon.Open()

            Dim wSTYMD As Date = WF_IMPYM.Text & "/01"
            Dim wEndYMD As Date = wSTYMD.Year & "/" & wSTYMD.Month & "/" & DateTime.DaysInMonth(wSTYMD.Year, wSTYMD.Month).ToString("00")
            Dim SQLStr As String =
                          " SELECT " _
                        & " DAY(YMD) as DAY " _
                        & " FROM " _
                        & "     T0005_NIPPO " _
                        & " WHERE " _
                        & "        CAMPCODE = @CAMPCODE " _
                        & "   and  YMD >= @STYMD   " _
                        & "   and  YMD <= @ENDYMD  " _
                        & "   and  DELFLG <> '1'   "
            If Not String.IsNullOrEmpty(WF_UORG.Text) Then SQLStr &= "   and SHIPORG = @ORGCODE "
            SQLStr = SQLStr _
                        & " GROUP BY YMD           " _
                        & " ORDER BY YMD           "
            Using SQLCmd As New SqlCommand(SQLStr, SqlCon)
                Dim P_CAMPCODE As SqlParameter = SQLCmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_ORGCODE As SqlParameter = SQLCmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_STYMD As SqlParameter = SQLCmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                Dim P_ENDYMD As SqlParameter = SQLCmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                P_CAMPCODE.Value = WF_CAMPCODE.Text
                P_ORGCODE.Value = WF_UORG.Text
                P_STYMD.Value = wSTYMD
                P_ENDYMD.Value = wEndYMD
                Using SQLdr As SqlDataReader = SQLCmd.ExecuteReader()
                    WF_LISTDAY.Value = String.Empty
                    While SQLdr.Read
                        If Not String.IsNullOrEmpty(WF_LISTDAY.Value) Then WF_LISTDAY.Value &= "|"
                        WF_LISTDAY.Value &= SQLdr("DAY")
                    End While

                End Using
            End Using

        End Using

    End Sub
    ''' <summary>
    ''' 取込済一覧の作成
    ''' </summary>
    Protected Sub GetImportData()

        Dim wStYMD As Date = WF_IMPYM.Text & "/01"
        Dim wEndYMD As Date = wStYMD.Year & "/" & wStYMD.Month & "/" & DateTime.DaysInMonth(wStYMD.Year, wStYMD.Month).ToString("00")

        Dim outTable = New Table() With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTData = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim outTData2 = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}

        Dim outCell As TableCell
        Dim outCell2 As TableCell
        Dim List As String() = WF_LISTDAY.Value.Split("|")
        For day As Integer = 1 To wEndYMD.Day
            Dim tday As Date = WF_IMPYM.Text & "/" & day.ToString("00")

            outCell = New TableCell
            outCell.Text = day
            '選択日付を開始日に設定する処理
            outCell.Attributes.Add("onclick", "setSTYMD('" & WF_IMPYM.Text & "/" & day.ToString("00") & "');")
            If tday.DayOfWeek = DayOfWeek.Sunday Then outCell.CssClass = "WeekEnd"
            outTData.Cells.Add(outCell)

            outCell2 = New TableCell
            '当日以降は　"-"表示、　日報あるなら”〇”　ないなら”×”
            outCell2.Text = If(Date.Now < tday, "-", If(List.Contains(day), "〇", "×"))
            outTData2.Cells.Add(outCell2)
            If day Mod 7 = 0 Then
                outTable.Rows.Add(outTData)
                outTData = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
                outTable.Rows.Add(outTData2)
                outTData2 = New TableRow With {.ViewStateMode = UI.ViewStateMode.Disabled}
            End If
        Next
        outTable.Rows.Add(outTData)
        outTable.Rows.Add(outTData2)
        WF_NIPPO_CALENDAR.Controls.Add(outTable)
    End Sub



End Class
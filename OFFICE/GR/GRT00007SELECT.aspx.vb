Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 営業勤怠登録（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00007SELECT
    Inherits Page

    Private T0007COM As New GRT0007COM                          '勤怠共通
    Private CS0050SESSION As New BASEDLL.CS0050SESSION          'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String
    Private WW_ERRCODE As String

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then

            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                '○ 各ボタン押下処理
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonRESTART"             '再開ボタン押下
                        WF_ButtonRESTART_Click()
                    Case "WF_ButtonDO"                  '実行ボタン押下
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '終了ボタン押下
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
        Master.MAPID = GRT00007WRKINC.MAPIDS

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""

        leftview.activeListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

        If System.IO.File.Exists(work.WF_T7SEL_XMLsaveTmp.Text) Then
            WF_Restart.Value = "TRUE"
        Else
            WF_Restart.Value = "FALSE"
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ メニューからの画面遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            WF_CAMPCODE.Text = work.WF_T7SEL_CAMPCODE.Text                            '会社コード
            Master.getFirstValue(WF_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text)    '対象年月
            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                WF_TAISHOYM.Text = Date.Now.Year.ToString() & "/" & Date.Now.Month.ToString()
            End Try

            Master.getFirstValue(WF_CAMPCODE.Text, "HORG", WF_HORG.Text)            '配属部署

            Master.getFirstValue(WF_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text)    '職務区分

            Master.getFirstValue(WF_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text)  '乗務員コード

            '一時保存パス
            work.WF_T7SEL_XMLsaveTmp.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & CS0050SESSION.USERID & "-T00007-TMP.txt"
            work.WF_T7SEL_XMLsavePARM.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & CS0050SESSION.USERID & "-T00007S-PARM.txt"

        End If

        '○ 一覧画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007I OrElse
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007INJS OrElse
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007IKNK OrElse
            Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00007IJKT Then

            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_T7SEL_CAMPCODE.Text                    '会社コード
            WF_TAISHOYM.Text = work.WF_T7SEL_TAISHOYM.Text                    '申請年月
            WF_HORG.Text = work.WF_T7SEL_HORG.Text                            '配属部署
            WF_STAFFKBN.Text = work.WF_T7SEL_STAFFKBN.Text                    '職務区分
            WF_STAFFCODE.Text = work.WF_T7SEL_STAFFCODE.Text                  '従業員コード
            WF_STAFFNAME.Text = work.WF_T7SEL_STAFFNAME.Text                  '従業員名称

        End If

        '○ RightBox情報設定
        Select Case WF_CAMPCODE.Text
            Case "02"       '株式会社エネックス
                rightview.MAPID = GRT00007WRKINC.MAPIDI
                rightview.MAPID_DTL = GRT00007WRKINC.MAPID
            Case "03"       '近畿石油輸送株式会社
                rightview.MAPID = GRT00007WRKINC.MAPIDIKNK
                rightview.MAPID_DTL = GRT00007WRKINC.MAPIDKNK
            Case "04"       '株式会社ニュージェイズ
                rightview.MAPID = GRT00007WRKINC.MAPIDINJS
                rightview.MAPID_DTL = GRT00007WRKINC.MAPIDNJS
            Case "05"       '株式会社ＪＫトランス
                rightview.MAPID = GRT00007WRKINC.MAPIDIJKT
                rightview.MAPID_DTL = GRT00007WRKINC.MAPIDJKT
        End Select
        rightview.MAPIDS = GRT00007WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定（一覧画面）", "画面レイアウト設定（明細画面）", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)         '会社コード
        CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)                     '配属部署
        CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_DUMMY)         '承認区分
        CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_DUMMY)      '従業員コード

    End Sub

    ''' <summary>
    ''' 再開ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRESTART_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_TAISHOYM.Text)          '申請年月
        Master.eraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.eraseCharToIgnore(WF_STAFFKBN.Text)          '職務区分
        Master.eraseCharToIgnore(WF_STAFFCODE.Text)         '従業員コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_T7SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_T7SEL_TAISHOYM.Text = WF_TAISHOYM.Text        '申請年月
        work.WF_T7SEL_HORG.Text = WF_HORG.Text                '配属部署
        work.WF_T7SEL_STAFFKBN.Text = WF_STAFFKBN.Text        '職務区分
        work.WF_T7SEL_STAFFCODE.Text = WF_STAFFCODE.Text      '従業員コード
        work.WF_T7SEL_STAFFNAME.Text = WF_STAFFNAME.Text      '従業員名称

        work.WF_T7SEL_BUTTON.Text = "RESTART"

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)

        work.WF_SEL_VIEWID.Text = rightview.getViewId(WF_CAMPCODE.Text)
        work.WF_SEL_VIEWID_DTL.Text = rightview.getViewDtlId(WF_CAMPCODE.Text)

        '勤怠締テーブル取得
        Dim WW_LIMITFLG As String = "0"
        T0007COM.T00008get(WF_CAMPCODE.Text,
                                 WF_HORG.Text,
                                 CDate(WF_TAISHOYM.Text).ToString("yyyy/MM"),
                                 WW_LIMITFLG,
                                 WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
            Exit Sub
        End If

        work.WF_T7SEL_LIMITFLG.Text = WW_LIMITFLG

        '権限テーブル取得
        Dim WW_PERMITCODE As String = "0"
        T0007COM.OrgCheck(WF_HORG.Text, WW_PERMITCODE, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR")
            Exit Sub
        End If

        work.WF_T7SEL_PERMITCODE.Text = WW_PERMITCODE

        '○ SQL異常対応
        work.SQLAbnormalityRepair()

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.transitionPage(work.WF_T7SEL_CAMPCODE.Text)
        End If

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_TAISHOYM.Text)          '申請年月
        Master.eraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.eraseCharToIgnore(WF_STAFFKBN.Text)          '職務区分
        Master.eraseCharToIgnore(WF_STAFFCODE.Text)         '従業員コード

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_T7SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_T7SEL_TAISHOYM.Text = WF_TAISHOYM.Text        '申請年月
        work.WF_T7SEL_HORG.Text = WF_HORG.Text                '配属部署
        work.WF_T7SEL_STAFFKBN.Text = WF_STAFFKBN.Text        '職務区分
        work.WF_T7SEL_STAFFCODE.Text = WF_STAFFCODE.Text      '従業員コード
        work.WF_T7SEL_STAFFNAME.Text = WF_STAFFNAME.Text      '従業員名称

        work.WF_T7SEL_BUTTON.Text = "DO"

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)
        work.WF_SEL_VIEWID.Text = rightview.getViewId(WF_CAMPCODE.Text)
        work.WF_SEL_VIEWID_DTL.Text = rightview.getViewDtlId(WF_CAMPCODE.Text)

        '勤怠締テーブル取得
        Dim WW_LIMITFLG As String = "0"
        T0007COM.T00008get(WF_CAMPCODE.Text,
                                 WF_HORG.Text,
                                 CDate(WF_TAISHOYM.Text).ToString("yyyy/MM"),
                                 WW_LIMITFLG,
                                 WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "T0008_KINTAISTAT")
            Exit Sub
        End If

        work.WF_T7SEL_LIMITFLG.Text = WW_LIMITFLG

        '権限テーブル取得
        Dim WW_PERMITCODE As String = "0"
        T0007COM.OrgCheck(WF_HORG.Text, WW_PERMITCODE, WW_ERRCODE)
        If WW_ERRCODE <> C_MESSAGE_NO.NORMAL Then
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0012_SRVAUTHOR")
            Exit Sub
        End If

        work.WF_T7SEL_PERMITCODE.Text = WW_PERMITCODE

        '○ SQL異常対応
        work.SQLAbnormalityRepair()

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.transitionPage(work.WF_T7SEL_CAMPCODE.Text)
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

        '○ 単項目チェック
        '会社コード
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '対象年月
        Master.checkFIeld(WF_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_TAISHOYM.Text <> "" Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                    WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "対象年月 : " & WF_TAISHOYM.Text)
                    WF_TAISHOYM.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End Try
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TAISHOYM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '配属部署
        WW_TEXT = WF_HORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "HORG", WF_HORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_HORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "配属部署 : " & WF_HORG.Text)
                    WF_HORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_HORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '職務区分
        WW_TEXT = WF_STAFFKBN.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STAFFKBN", WF_STAFFKBN.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFKBN.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "職務区分 : " & WF_STAFFKBN.Text)
                    WF_STAFFKBN.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFKBN.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '従業員
        WW_TEXT = WF_STAFFCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "STAFFCODE", WF_STAFFCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_STAFFCODE.Text = ""
            Else
                '存在チェック
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "従業員 : " & WF_STAFFCODE.Text)
                    WF_STAFFCODE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_STAFFCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        '〇フィールドダブルクリック処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) OrElse WF_LeftMViewChange.Value = "" Then
        Else
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
                            Case "WF_TAISHOYM"        '申請年月
                                .WF_Calendar.Text = WF_TAISHOYM.Text & "/01"
                        End Select
                        .activeCalendar()

                    Case Else
                        '上記以外

                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_HORG"          '配属部署
                                prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                            Case "WF_STAFFKBN"      '職務区分
                                prmData = work.CreateStaffKbnParam(WF_CAMPCODE.Text)
                            Case "WF_STAFFCODE"     '従業員
                                prmData = work.getStaffCodeList(WF_CAMPCODE.Text, WF_TAISHOYM.Text, WF_HORG.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
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
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            Case "WF_HORG"              '配属部署
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFKBN"          '職務区分
                CODENAME_get("STAFFKBN", WF_STAFFKBN.Text, WF_STAFFKBN_TEXT.Text, WW_RTN_SW)
            Case "WF_STAFFCODE"         '従業員
                CODENAME_get("STAFFCODE", WF_STAFFCODE.Text, WF_STAFFCODE_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

        Dim WW_SelectValues As String() = Nothing

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValues = leftview.getActiveValue
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValues(0)
                WF_CAMPCODE_TEXT.Text = WW_SelectValues(1)
                WF_CAMPCODE.Focus()

            Case "WF_TAISHOYM"          '申請年月
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValues(0), WW_DATE)
                    WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                Catch ex As Exception
                End Try
                WF_TAISHOYM.Focus()

            Case "WF_HORG"          '配属部署
                WF_HORG.Text = WW_SelectValues(0)
                WF_HORG_TEXT.Text = WW_SelectValues(1)
                WF_HORG.Focus()

            Case "WF_STAFFKBN"      '職務区分
                WF_STAFFKBN.Text = WW_SelectValues(0)
                WF_STAFFKBN_TEXT.Text = WW_SelectValues(1)
                WF_STAFFKBN.Focus()

            Case "WF_STAFFCODE"     '従業員
                WF_STAFFCODE.Text = WW_SelectValues(0)
                WF_STAFFCODE_TEXT.Text = WW_SelectValues(1)
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
            Case "WF_TAISHOYM"          '申請年月
                WF_TAISHOYM.Focus()
            Case "WF_HORG"              '配属部署
                WF_HORG.Focus()
            Case "WF_STAFFKBN"          '職務区分
                WF_STAFFKBN.Focus()
            Case "WF_STAFFCODE"         '従業員
                WF_STAFFCODE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        WF_RightboxOpen.Value = ""
        Master.showHelp()

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
                Case "HORG"             '配属部署
                    prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFKBN"         '職務区分
                    prmData = work.CreateStaffKbnParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STAFFCODE"        '従業員
                    prmData = work.getStaffCodeList(WF_CAMPCODE.Text, WF_TAISHOYM.Text, WF_HORG.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STAFFCODE, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
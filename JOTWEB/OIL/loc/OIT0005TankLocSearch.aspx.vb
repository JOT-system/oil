Option Strict On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
Imports JOTWEB.GRC0001TILESELECTORWRKINC

''' <summary>
''' タンク所在管理検索画面クラス
''' </summary>
''' <remarks>
'''  作成日 2020/03/12
'''  更新日 2020/03/12
'''  作成者 JOT三宅(弘)
'''  更新者 JOT三宅(弘)
'''
'''  修正履歴:
'''         :
''' </remarks>
Public Class OIT0005TankLocSearch
    Inherits System.Web.UI.Page
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private OIT0005ReportTbl As DataTable                           '帳票用テーブル

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String
    ''' <summary>
    ''' ロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    Case "WF_ButtonKoukenList"          '交検一覧ﾀﾞｳﾝﾛｰﾄﾞボタン押下
                        WF_ButtonKoukenList_Click()
                    Case "WF_ButtonHaizokuList"         '配属表ﾀﾞｳﾝﾛｰﾄﾞボタン押下
                        WF_ButtonHaizokuList_Click()
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
        Master.MAPID = OIT0005WRKINC.MAPIDS

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

            Dim prmData As New Hashtable
            Dim shipperCode As String = ""
            Dim consigneeCode As String = ""
            prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text
            Dim dummyTxtSalesOffice As String = ""
            prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, dummyTxtSalesOffice)
            leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, WW_DUMMY, prmData)
            If leftview.WF_LeftListBox.Items IsNot Nothing Then
                '一旦根岸(011402)'本当はログインユーザーのORG
                Dim foundItem = leftview.WF_LeftListBox.Items.FindByValue("011402")
                'Dim foundItem = leftview.WF_LeftListBox.Items.FindByValue(Master.USER_ORG)
                If foundItem IsNot Nothing Then
                    dummyTxtSalesOffice = foundItem.Value
                Else
                    dummyTxtSalesOffice = leftview.WF_LeftListBox.Items(0).Value
                End If

            End If
            '〇仮置き
            'Dim paramData As Hashtable = work.CreateSALESOFFICEParam(Master.USER_ORG, dummyTxtSalesOffice)
            Dim paramData As Hashtable = work.CreateFIXParam(Master.USER_ORG)
            Me.tileSalesOffice.ListBoxClassification = LIST_BOX_CLASSIFICATION.LC_BELONGTOOFFICE
            Me.tileSalesOffice.ParamData = paramData
            Me.tileSalesOffice.LeftObj = leftview
            Me.tileSalesOffice.SetTileValues()

            If {"jot_sys_1", "jot_oil_1"}.Contains(Master.ROLE_MAP) Then
                Me.tileSalesOffice.SelectAll()
                'OTと在日米軍につきデフォルトは未選択状態
                Me.tileSalesOffice.UnSelectItem("110001")
                Me.tileSalesOffice.UnSelectItem("710001")
            Else
                Me.tileSalesOffice.SelectSingleItem(Master.USER_ORG)
            End If
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005C Then   '実行画面からの遷移
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text            '会社コード
            WF_ORG.Text = work.WF_SEL_ORG.Text            '組織コード
            Me.tileSalesOffice.Recover(work.WF_SEL_SALESOFFICE_TILES.Text) '所属先（タイル選択）
        End If

        '○交検一覧ダウンロード関連
        'WF_STYMD.Text = Now.ToShortDateString()

        '年月ドロップダウンの生成
        Dim ymDdl As New DropDownList
        Dim dt As Date = Now
        For pMonth As Integer = 6 To -6 Step -1
            ymDdl.Items.Add(dt.AddMonths(pMonth).ToString("yyyy/MM"))
            If pMonth = 0 Then
                ymDdl.SelectedValue = dt.AddMonths(pMonth).ToString("yyyy/MM")
            End If
        Next
        If ymDdl.Items.Count > 0 Then
            '交検一覧用
            Me.ddlKoukenListYearMonth.Items.AddRange(ymDdl.Items.Cast(Of ListItem).ToArray)
            Me.ddlKoukenListYearMonth.SelectedIndex = ymDdl.SelectedIndex
            '配属表用
            Me.ddlHaizokuListYearMonth.Items.AddRange(ymDdl.Items.Cast(Of ListItem).ToArray)
            Me.ddlHaizokuListYearMonth.SelectedIndex = ymDdl.SelectedIndex
        End If

        '○ RightBox情報設定
        rightview.MAPIDS = OIT0005WRKINC.MAPIDS
        rightview.MAPID = OIT0005WRKINC.MAPIDC
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

    End Sub

    ''' <summary>
    ''' 交検一覧ﾀﾞｳﾝﾛｰﾄﾞボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonKoukenList_Click()

        Try

            '******************************
            '選択項目チェック
            '******************************
            '選択項目が1つもない場合
            If Me.tileSalesOffice.HasSelectedValue = False Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "所属先", needsPopUp:=True)
                Me.tileSalesOffice.Focus()
                Exit Sub
            End If

            '↓未確定のためコメントアウト（複数で出せるようにしておく）
            ''選択項目が複数ある場合
            'If Me.tileSalesOffice.GetSelectedListData.Items.Count > 1 Then
            '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "選択可能な所属先は一つのみ", needsPopUp:=True)
            '    Me.tileSalesOffice.Focus()
            '    Exit Sub
            'End If

            '選択データ取得
            Dim officeCodeDic As New Dictionary(Of String, String)
            For Each item As ListItem In Me.tileSalesOffice.GetSelectedListData().Items
                officeCodeDic.Add(item.Value, item.Text)
            Next
            '取得開始日付取得（Default：Now）
            Dim beginDate As Date = Nothing
            If Not Date.TryParse(ddlKoukenListYearMonth.SelectedValue + "/01", beginDate) Then
                beginDate = Now
            End If
            '取得終了日付計算（取得開始日付の翌月末日）
            Dim endDate As Date = beginDate.AddDays(beginDate.Day * -1 + 1).AddMonths(2).AddDays(-1)

            '******************************
            '出力データ取得
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                ExcelKoukenListDataGet(SQLcon, officeCodeDic.Keys.ToArray, beginDate, endDate)
            End Using

            '******************************
            '出力データ生成
            '******************************
            Dim url As String
            url = OIT0005CustomReport.CreateKoukenList(Master.MAPID, officeCodeDic, beginDate, OIT0005ReportTbl)

            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005S DLKoukenList")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005S DLKoukenList"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()
        End Try

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 交検一覧ﾀﾞｳﾝﾛｰﾄﾞボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonHaizokuList_Click()

        Try

            '******************************
            '選択項目チェック
            '******************************
            '選択項目が1つもない場合
            If Me.tileSalesOffice.HasSelectedValue = False Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "所属先", needsPopUp:=True)
                Me.tileSalesOffice.Focus()
                Exit Sub
            End If

            '↓未確定のためコメントアウト（複数で出せるようにしておく）
            ''選択項目が複数ある場合
            'If Me.tileSalesOffice.GetSelectedListData.Items.Count > 1 Then
            '    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "選択可能な所属先は一つのみ", needsPopUp:=True)
            '    Me.tileSalesOffice.Focus()
            '    Exit Sub
            'End If

            '選択データ取得
            Dim officeCodeDic As New Dictionary(Of String, String)
            For Each item As ListItem In Me.tileSalesOffice.GetSelectedListData().Items
                officeCodeDic.Add(item.Value, item.Text)
            Next
            '取得開始日付取得（Default：Now）
            Dim beginDate As Date = Nothing
            If Not Date.TryParse(ddlHaizokuListYearMonth.SelectedValue + "/01", beginDate) Then
                beginDate = Now
            End If
            '取得終了日付計算（取得開始日付の末日）
            Dim endDate As Date = beginDate.AddDays(beginDate.Day * -1 + 1).AddMonths(1).AddDays(-1)

            '******************************
            '出力データ取得
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                ExcelHaizokuListDataGet(SQLcon, officeCodeDic.Keys.ToArray, beginDate, endDate)
            End Using

            '******************************
            '出力データ生成
            '******************************
            Dim url As String
            url = OIT0005CustomReport.CreateHaizokuList(Master.MAPID, officeCodeDic, beginDate, OIT0005ReportTbl)

            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005S DLKoukenList")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005S DLKoukenList"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()
        End Try

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_SEL_ORG.Text = WF_ORG.Text                  '組織コード
        '営業所
        work.WF_SEL_SALESOFFICE_TILES.Text = Me.tileSalesOffice.GetListItemsStr
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
        '所属先
        'Master.CheckField(WF_CAMPCODE.Text, "OFFICECODE", TxtSalesOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        'If isNormal(WW_CS0024FCHECKERR) Then
        '    CODENAME_get("OFFICECODE", TxtSalesOffice.Text, LblSalesOfficeName.Text, WW_RTN_SW)
        '    If Not isNormal(WW_RTN_SW) Then
        '        Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "所属先 : " & TxtSalesOffice.Text, needsPopUp:=True)
        '        TxtSalesOffice.Focus()
        '        O_RTN = "ERR"
        '        Exit Sub
        '    End If
        'Else
        '    Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "所属先", needsPopUp:=True)
        '    TxtSalesOffice.Focus()
        '    O_RTN = "ERR"
        '    Exit Sub
        'End If
        '選択項目が1つもない場合
        If Me.tileSalesOffice.HasSelectedValue = False Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "所属先", needsPopUp:=True)
            Me.tileSalesOffice.Focus()
            O_RTN = "ERR"
            Return
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
                WF_LeftMViewChange.Value = Integer.Parse(WF_LeftMViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case CInt(WF_LeftMViewChange.Value)
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ''日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        'Select Case WF_FIELD.Value
                        '    Case WF_STYMD.ID         '年月日
                        '        .WF_Calendar.Text = CDate(WF_STYMD.Text).ToString("yyyy/MM/dd")
                        'End Select
                        '.ActiveCalendar()
                    Case LIST_BOX_CLASSIFICATION.LC_ORG
                        '組織コード(所属先)

                    Case Else
                        '会社コード
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        '所属先
                        If WF_FIELD.Value = "TxtSalesOffice" Then
                            'prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, TxtSalesOffice.Text)
                            'prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtSalesOffice.Text)
                        End If

                        Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                        .SetListBox(enumVal, WW_DUMMY, prmData)
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

            Case Else

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
        'Select Case WF_FIELD.Value
        '    Case WF_STYMD.ID
        '        Dim WW_DATE As Date
        '        Try
        '            Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
        '            If WW_DATE < CDate(C_DEFAULT_YMD) Then
        '                WF_STYMD.Text = ""
        '            Else
        '                WF_STYMD.Text = leftview.WF_Calendar.Text
        '            End If
        '        Catch ex As Exception
        '        End Try
        '        WF_STYMD.Focus()
        '    Case Else

        'End Select

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
            Case "TxtSalesOffice"
                'TxtSalesOffice.Focus()
                'Case WF_STYMD.ID
                '    WF_STYMD.Focus()
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
                Case "OFFICECODE"       '所属先
                    prmData = work.CreateSALESOFFICEParam(WF_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  帳票関連処理                                                          ***
    ' ******************************************************************************
    Protected Sub ExcelKoukenListDataGet(ByVal SQLcon As SqlConnection,
                                         ByVal officeCodes As String(),
                                         ByVal beginDate As Date,
                                         ByVal endDate As Date)

        If IsNothing(OIT0005ReportTbl) Then
            OIT0005ReportTbl = New DataTable
        End If

        If OIT0005ReportTbl.Columns.Count <> 0 Then
            OIT0005ReportTbl.Columns.Clear()
        End If

        OIT0005ReportTbl.Clear()

        Dim officeCodeInStat As String = String.Join(",", (From officeCode In officeCodes Select "'" & officeCode & "'"))

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As New StringBuilder
        With SQLStr
            .AppendLine(" SELECT ")
            .AppendLine("     TNK.OPERATIONBASECODE   AS OFFICECODE ")
            .AppendLine("   , TNK.JRINSPECTIONDATE    AS JRINSPECTIONDATE ")
            .AppendLine("   , TNK.TANKNUMBER          AS TANKNUMBER ")
            .AppendLine("   , TNK.JRALLINSPECTIONDATE AS JRALLINSPECTIONDATE ")
            .AppendLine("   , SZI.LASTOILCODE         AS LASTOILCODE ")
            .AppendLine("   , SZI.LASTOILNAME         AS LASTOILNAME ")
            .AppendLine(" FROM ")
            .AppendLine("   OIL.OIM0005_TANK TNK ")
            .AppendLine("   LEFT JOIN OIL.OIT0005_SHOZAI SZI ")
            .AppendLine("     ON SZI.TANKNUMBER = TNK.TANKNUMBER ")
            .AppendLine("     AND SZI.DELFLG = @DELFLG ")
            .AppendLine(" WHERE ")
            .AppendLine("   TNK.DELFLG = @DELFLG ")
            If Not String.IsNullOrEmpty(officeCodeInStat) Then
                .AppendFormat("   AND TNK.OPERATIONBASECODE IN ({0}) ", officeCodeInStat).AppendLine()
            End If
            .AppendLine("   AND TNK.JRINSPECTIONDATE BETWEEN @BEGINDATE AND @ENDDATE ")
            .AppendLine(" ORDER BY ")
            .AppendLine("   TNK.OPERATIONBASECODE ")
            .AppendLine("   , TNK.JRINSPECTIONDATE ")
            .AppendLine("   , TNK.TANKNUMBER DESC ")
        End With

        Try
            Using SQLcmd As New SqlCommand(SQLStr.ToString(), SQLcon)

                Dim P_BEGINDATE As SqlParameter = SQLcmd.Parameters.Add("@BEGINDATE", SqlDbType.Date)       '出力開始日
                Dim P_ENDDATE As SqlParameter = SQLcmd.Parameters.Add("@ENDDATE", SqlDbType.Date)           '出力終了日
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)      '削除フラグ
                P_BEGINDATE.Value = beginDate.ToShortDateString()
                P_ENDDATE.Value = endDate.ToShortDateString()
                P_DELFLG.Value = C_DELETE_FLG.ALIVE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0005ReportTbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0005ReportTbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005S EXCEL_KOUKENLIST_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005S EXCEL_KOUKENLIST_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    Protected Sub ExcelHaizokuListDataGet(ByVal SQLcon As SqlConnection,
                                          ByVal officeCodes As String(),
                                          ByVal beginDate As Date,
                                          ByVal endDate As Date)

        If IsNothing(OIT0005ReportTbl) Then
            OIT0005ReportTbl = New DataTable
        End If

        If OIT0005ReportTbl.Columns.Count <> 0 Then
            OIT0005ReportTbl.Columns.Clear()
        End If

        OIT0005ReportTbl.Clear()

        Dim officeCodeInStat As String = String.Join(",", (From officeCode In officeCodes Select "'" & officeCode & "'"))

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        Dim SQLStr As New StringBuilder
        With SQLStr
            .AppendLine(" SELECT ")
            .AppendLine("     TNK.OPERATIONBASECODE AS OFFICECODE ")
            .AppendLine("   , TNK.JRINSPECTIONDATE  AS JRINSPECTIONDATE ")
            .AppendLine("   , TNK.LOAD               AS LOAD ")
            .AppendLine("   , TNK.TANKNUMBER        AS TANKNUMBER ")
            .AppendLine("   , TNK.MIDDLEOILCODE     AS MIDDLEOILCODE ")
            .AppendLine("   , SZI.TANKSITUATION     AS TANKSITUATION ")
            .AppendLine("   , TNK.MARKCODE          AS MARKCODE ")
            .AppendLine("   , ( ")
            .AppendLine("      SELECT distinct ")
            .AppendLine("         MIN(DTL.ACTUALACCDATE) ")
            .AppendLine("     FROM ")
            .AppendLine("       OIL.OIT0003_DETAIL DTL ")
            .AppendLine("     WHERE ")
            .AppendLine("       DTL.TANKNO = TNK.TANKNUMBER ")
            .AppendLine("       AND DTL.DELFLG = @DELFLG ")
            .AppendLine("   )                       AS ACTUALACCDATE ")
            .AppendLine(" FROM ")
            .AppendLine("   OIL.OIM0005_TANK TNK ")
            .AppendLine("   LEFT JOIN OIL.OIT0005_SHOZAI SZI ")
            .AppendLine("     ON SZI.TANKNUMBER = TNK.TANKNUMBER ")
            .AppendLine("     AND SZI.DELFLG = @DELFLG ")
            .AppendLine(" WHERE ")
            .AppendLine("   ISNULL(TNK.OPERATIONBASECODE, '') <> '' ")
            .AppendLine("   AND TNK.DELFLG = @DELFLG ")
            If Not String.IsNullOrEmpty(officeCodeInStat) Then
                .AppendFormat("   AND TNK.OPERATIONBASECODE IN ({0}) ", officeCodeInStat).AppendLine()
            End If
            .AppendLine("   AND TNK.JRINSPECTIONDATE BETWEEN @BEGINDATE AND @ENDDATE ")
            .AppendLine(" ORDER BY ")
            .AppendLine("   TNK.OPERATIONBASECODE ")
            .AppendLine("   , TNK.JRINSPECTIONDATE ")
            .AppendLine("   , TNK.TANKNUMBER DESC ")
        End With

        Try
            Using SQLcmd As New SqlCommand(SQLStr.ToString(), SQLcon)

                Dim P_BEGINDATE As SqlParameter = SQLcmd.Parameters.Add("@BEGINDATE", SqlDbType.Date)       '出力開始日
                Dim P_ENDDATE As SqlParameter = SQLcmd.Parameters.Add("@ENDDATE", SqlDbType.Date)           '出力終了日
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)      '削除フラグ
                P_BEGINDATE.Value = beginDate.ToShortDateString()
                P_ENDDATE.Value = endDate.ToShortDateString()
                P_DELFLG.Value = C_DELETE_FLG.ALIVE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0005ReportTbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0005ReportTbl.Load(SQLdr)
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0005S EXCEL_HAIZOKULIST_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0005S EXCEL_HAIZOKULIST_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

End Class
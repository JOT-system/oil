Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' タンク所在入力画面クラス
''' </summary>
Public Class OIT0005TankLocDetail
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIT0005tbl As DataTable                                 '一覧格納用テーブル

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0005tbl, work.WF_LISTSEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "btnCommonConfirmOk"
                            UpdateConfirmOk_Click()
                    End Select

                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            WF_BOXChange.Value = "detailbox"

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0005tbl) Then
                OIT0005tbl.Clear()
                OIT0005tbl.Dispose()
                OIT0005tbl = Nothing
            End If

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0005WRKINC.MAPIDD
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        '        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)
        Master.RecoverTable(OIT0005tbl, work.WF_LISTSEL_INPTBL.Text)
        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        '        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ''○ 検索画面からの遷移
        'If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005L Then
        '    'Grid情報保存先のファイル名
        '    Master.CreateXMLSaveFile()
        'End If
        Dim selectedDr As DataRow = (From dr As DataRow In Me.OIT0005tbl Where Convert.ToString(dr("TANKNUMBER")) = work.WF_LISTSEL_TANKNUMBER.Text).FirstOrDefault
        If selectedDr IsNot Nothing Then
            WF_Sel_TANKNUMBER.Text = Convert.ToString(selectedDr("TANKNUMBER"))
            Dim txtObj As TextBox
            Dim masterCph = DirectCast(Master.FindControl("contents1"), ContentPlaceHolder)
            For Each col As DataColumn In OIT0005tbl.Columns
                txtObj = DirectCast(masterCph.FindControl("Txt" & col.ColumnName), TextBox)
                If txtObj IsNot Nothing Then
                    txtObj.Text = Convert.ToString(selectedDr(col.ColumnName))
                End If
            Next col
            '油種名をラベルに設定
            Me.LblOilCodeText.Text = Convert.ToString(selectedDr("OILNAME"))
            Me.LblLastOilCodeText.Text = Convert.ToString(selectedDr("LASTOILNAME"))
            'コードより名称を設定
            CODENAME_get("BRANCHCODE", TxtBranchCode.Text, LblBranchCodeText.Text, WW_DUMMY)
            CODENAME_get("OFFICECODE", TxtOfficeCode.Text, LblOfficeCodeText.Text, WW_DUMMY)
            CODENAME_get("LOCATIONCODE", TxtLocationCode.Text, LblLocationCodeText.Text, WW_DUMMY)
            CODENAME_get("TANKSTATUS", TxtTankStatus.Text, LblTankStatusText.Text, WW_DUMMY)
            CODENAME_get("LOADINGKBN", TxtLoadingKbn.Text, LblLoadingKbnText.Text, WW_DUMMY)
        End If

    End Sub



    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)
        '############# おためし #############
        If isNormal(WW_ERR_SW) = False Then
            Return
        End If

        Master.Output(C_MESSAGE_NO.OIL_CONFIRM_UPDATE_TANKLOCATION, C_MESSAGE_TYPE.QUES, needsPopUp:=True, messageBoxTitle:="", IsConfirm:=True)

    End Sub
    ''' <summary>
    ''' 更新OKボタンクリック時
    ''' </summary>
    Protected Sub UpdateConfirmOk_Click()
        'Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.INF, "まだ機能未実装です！", needsPopUp:=True)
        'Return
        Dim updateDr As DataRow = Me.OIT0005tbl.NewRow
        '画面表示時の内容を更新データRowに移す
        Dim targetRow As DataRow = (From dr As DataRow In Me.OIT0005tbl Where dr("TANKNUMBER").Equals(Me.WF_Sel_TANKNUMBER.Text)).FirstOrDefault
        For Each colObj As DataColumn In Me.OIT0005tbl.Columns
            updateDr(colObj.ColumnName) = targetRow(colObj.ColumnName)
        Next
        '画面に表示している内容を更新データRowに書き込む
        updateDr("BRANCHCODE") = Me.TxtBranchCode.Text
        updateDr("OFFICECODE") = Me.TxtOfficeCode.Text
        updateDr("LOCATIONCODE") = Me.TxtLocationCode.Text
        updateDr("TANKSTATUS") = Me.TxtTankStatus.Text
        updateDr("LOADINGKBN") = Me.TxtLoadingKbn.Text
        updateDr("EMPARRDATE") = If(Me.TxtEmpArrDate.Text = "", CType(DBNull.Value, Object), Me.TxtEmpArrDate.Text)
        updateDr("ACTUALEMPARRDATE") = If(Me.TxtActualEmpArrDate.Text = "", CType(DBNull.Value, Object), Me.TxtActualEmpArrDate.Text)
        updateDr("OILCODE") = Me.TxtOilCode.Text
        updateDr("OILNAME") = Me.LblOilCodeText.Text
        updateDr("ORDERINGTYPE") = Me.TxtOrderingType.Text
        updateDr("ORDERINGOILNAME") = Me.TxtOrderingOilName.Text
        updateDr("LASTOILCODE") = Me.TxtLastOilCode.Text
        updateDr("LASTOILNAME") = Me.LblLastOilCodeText.Text
        updateDr("PREORDERINGTYPE") = Me.TxtPreOrderingType.Text
        updateDr("PREORDERINGOILNAME") = Me.TxtPreOrderingOilName.Text
        updateDr("UPDYMD") = Now.ToString("yyyy/MM/dd HH:mm:ss.FFF")
        updateDr("UPDUSER") = Master.USERID
        updateDr("UPDTERMID") = Master.USERTERMID
        updateDr("RECEIVEYMD") = C_DEFAULT_YMD

        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            UpdateShozai(sqlCon, Nothing, updateDr)
        End Using

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        'DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        ''○画面切替設定
        'WF_BOXChange.Value = "headerbox"

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                If CInt(WF_LeftMViewChange.Value) <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    '会社コード
                    Dim prmData As New Hashtable

                    'フィールドによってパラメーターを変える
                    Select Case WW_FIELD
                        Case "TxtBranchCode" '管轄支店コード
                            prmData = work.CreateFIXParam(Master.USER_ORG)
                        Case "TxtOfficeCode" '所属営業所コード
                            prmData = work.CreateFIXParam(Master.USER_ORG)
                        Case "TxtLocationCode" '所在地コード
                            Dim targetOffice As String = Me.TxtOfficeCode.Text
                            prmData = work.CreateFIXParam(targetOffice)
                        Case "TxtTankStatus" '所属営業所コード
                            prmData = work.CreateFIXParam("ZZ")
                            'prmData.Add(GRIS0005LeftBox.C_PARAMETERS.LP_ADDITINALCONDITION, "KEYCODE <> ''")
                        Case "TxtLoadingKbn" '所在地コード
                            prmData = work.CreateFIXParam("ZZ")
                        Case Else


                    End Select
                    Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                    .SetListBox(enumVal, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '空車着日（予定）
                        Case "TxtEmpArrDate"
                            .WF_Calendar.Text = TxtEmpArrDate.Text
                        '空車着日（実績）
                        Case "TxtActualEmpArrDate"
                            .WF_Calendar.Text = TxtActualEmpArrDate.Text

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
        Dim mesParam As String = ""
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "TxtBranchCode" '管轄支店コード
                CODENAME_get("BRANCHCODE", TxtBranchCode.Text, LblBranchCodeText.Text, WW_RTN_SW)
                mesParam = LblBranchCode.Text
            Case "TxtOfficeCode" '所属営業所コード
                CODENAME_get("OFFICECODE", TxtOfficeCode.Text, LblOfficeCodeText.Text, WW_RTN_SW)
                mesParam = LblOfficeCode.Text
                TxtLocationCode.Text = ""
                LblLocationCodeText.Text = ""
            Case "TxtLocationCode" '所在地コード
                CODENAME_get("LOCATIONCODE", TxtLocationCode.Text, LblLocationCodeText.Text, WW_RTN_SW)
                mesParam = LblLocationCode.Text
            Case "TxtTankStatus" 'タンク車状況
                CODENAME_get("TANKSTATUS", TxtTankStatus.Text, LblTankStatusText.Text, WW_RTN_SW)
                mesParam = LblTankStatus.Text
            Case "TxtLoadingKbn" '積車区分
                CODENAME_get("LOADINGKBN", TxtLoadingKbn.Text, LblLoadingKbnText.Text, WW_RTN_SW)
                mesParam = LblLoadingKbn.Text
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR, mesParam)
        End If
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
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
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "TxtBranchCode" '管轄支店コード
                    TxtBranchCode.Text = WW_SelectValue
                    LblBranchCodeText.Text = WW_SelectText
                    TxtBranchCode.Focus()
                Case "TxtOfficeCode" '所属営業所コード
                    TxtOfficeCode.Text = WW_SelectValue
                    LblOfficeCodeText.Text = WW_SelectText
                    TxtOfficeCode.Focus()
                    TxtLocationCode.Text = ""
                    LblLocationCodeText.Text = ""
                Case "TxtLocationCode" '所在地コード
                    TxtLocationCode.Text = WW_SelectValue
                    LblLocationCodeText.Text = WW_SelectText
                    TxtLocationCode.Focus()
                Case "TxtTankStatus" 'タンク車状況
                    TxtTankStatus.Text = WW_SelectValue
                    LblTankStatusText.Text = WW_SelectText
                    TxtTankStatus.Focus()
                Case "TxtLoadingKbn" '積車区分
                    TxtLoadingKbn.Text = WW_SelectValue
                    LblLoadingKbnText.Text = WW_SelectText
                    TxtLoadingKbn.Focus()
                Case "TxtEmpArrDate"       '空車着日（予定）
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < CDate(C_DEFAULT_YMD) Then
                            TxtEmpArrDate.Text = ""
                        Else
                            TxtEmpArrDate.Text = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try
                    TxtEmpArrDate.Focus()
                Case "TxtActualEmpArrDate"       '空車着日（実績）
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < CDate(C_DEFAULT_YMD) Then
                            TxtActualEmpArrDate.Text = ""
                        Else
                            TxtActualEmpArrDate.Text = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try
                    TxtActualEmpArrDate.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "TxtBranchCode" '管轄支店コード
                    TxtBranchCode.Focus()
                Case "TxtOfficeCode" '所属営業所コード
                    TxtOfficeCode.Focus()
                Case "TxtLocationCode" '所在地コード
                    TxtLocationCode.Focus()
                Case "TxtTankStatus" 'タンク車状況
                    TxtTankStatus.Focus()
                Case "TxtLoadingKbn" '積車区分
                    TxtLoadingKbn.Focus()
                Case "TxtEmpArrDate"       '空車着日（予定）
                    TxtEmpArrDate.Focus()
                Case "TxtActualEmpArrDate"       '空車着日（実績）
                    TxtActualEmpArrDate.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Dim intVal As Integer = 0
                If Integer.TryParse(WF_RightViewChange.Value, intVal) Then
                    WF_RightViewChange.Value = intVal.ToString
                End If
            Catch ex As Exception
                Exit Sub
            End Try
            Dim enumVal = DirectCast([Enum].ToObject(GetType(GRIS0004RightBox.RIGHT_TAB_INDEX), CInt(WF_RightViewChange.Value)), GRIS0004RightBox.RIGHT_TAB_INDEX)
            rightview.SelectIndex(enumVal)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_UniqueKeyCHECK As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now.ToString("yyyy/MM/dd")
        CS0025AUTHORget.ENDYMD = Date.Now.ToString("yyyy/MM/dd")
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            'WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            'WW_CheckMES2 = ""
            'WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            'WW_LINE_ERR = "ERR"
            'O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Return
        End If
        '○ 単項目チェック
        '単項目、一覧チェックを行うリスト定義（FieldName:データフィールドマスタのFIELDに合わせる
        '                                      TextObject:チェック対象のテキストボックス
        '                                      DispName:項目名称
        '                                      NeedsListCheck:リストチェック必要有無)
        Dim checkSingleAndLists = {New With {.FieldName = "BRANCHCODE", .TextObject = Me.TxtBranchCode, .DispName = LblBranchCode.Text, .NeedsListCheck = True},
                                   New With {.FieldName = "OFFICECODE", .TextObject = Me.TxtOfficeCode, .DispName = LblOfficeCode.Text, .NeedsListCheck = True},
                                   New With {.FieldName = "LOCATIONCODE", .TextObject = Me.TxtLocationCode, .DispName = LblLocationCode.Text, .NeedsListCheck = True},
                                   New With {.FieldName = "TANKSTATUS", .TextObject = Me.TxtTankStatus, .DispName = LblTankStatus.Text, .NeedsListCheck = True},
                                   New With {.FieldName = "LOADINGKBN", .TextObject = Me.TxtLoadingKbn, .DispName = LblLoadingKbn.Text, .NeedsListCheck = True},
                                   New With {.FieldName = "EMPARRDATE", .TextObject = Me.TxtEmpArrDate, .DispName = LblEmpArrDate.Text, .NeedsListCheck = False},
                                   New With {.FieldName = "ACTUALEMPARRDATE", .TextObject = Me.TxtActualEmpArrDate, .DispName = LblActualEmpArrDate.Text, .NeedsListCheck = False}
                                  }
        '各チェック対象項目をループ
        For Each chkItm In checkSingleAndLists
            '○ 単項目チェック
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, chkItm.FieldName, chkItm.TextObject.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                'リストチェック
                If chkItm.NeedsListCheck = False Then
                    'リストチェック不要な場合は次にスキップ
                    Continue For
                End If
                CODENAME_get(chkItm.FieldName, chkItm.TextObject.Text, WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, chkItm.DispName, needsPopUp:=True)
                    O_RTN = "ERR"
                    chkItm.TextObject.Focus()
                    Return
                End If
            Else
                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, chkItm.DispName, needsPopUp:=True)
                O_RTN = WW_CS0024FCHECKERR
                chkItm.TextObject.Focus()
                Return
            End If

        Next chkItm

    End Sub

    '''' <summary>
    '''' エラーレポート編集
    '''' </summary>
    '''' <param name="MESSAGE1"></param>
    '''' <param name="MESSAGE2"></param>
    '''' <param name="OIT0005row"></param>
    '''' <remarks></remarks>
    'Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIT0005row As DataRow = Nothing)

    '    Dim WW_ERR_MES As String = ""
    '    WW_ERR_MES = MESSAGE1
    '    If MESSAGE2 <> "" Then
    '        WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
    '    End If

    '    'If Not IsNothing(OIM0004row) Then
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅コード       =" & OIM0004row("STATIONCODE") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物コード枝番     =" & OIM0004row("BRANCH") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称         =" & OIM0004row("STATONNAME") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称カナ     =" & OIM0004row("STATIONNAMEKANA") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称     =" & OIM0004row("TYPENAME") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称カナ =" & OIM0004row("TYPENAMEKANA") & " , "
    '    '    WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0004row("DELFLG")
    '    'End If

    '    rightview.AddErrorReport(WW_ERR_MES)

    'End Sub
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

        Try
            Select Case I_FIELD
                Case "BRANCHCODE" '管轄支店コード
                    prmData = work.CreateFIXParam(Master.USER_ORG, "BRANCH")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BRANCH, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OFFICECODE" '所属営業所コード
                    prmData = work.CreateFIXParam(Master.USER_ORG, "BELONGTOOFFICE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BELONGTOOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LOCATIONCODE" '所在地コード
                    prmData = work.CreateFIXParam(Me.TxtOfficeCode.Text, "BRANCHOFFICESTATION")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BRANCHOFFICESTATION, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TANKSTATUS" 'タンク車状況
                    prmData = work.CreateFIXParam("ZZ", "TANKSTATUS")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKSTATUS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LOADINGKBN" '積車区分
                    prmData = work.CreateFIXParam("ZZ", "LOADINGKBN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_LOADINGKBN, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "UORG"             '運用部署
                    'prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' オーダー基本情報更新処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト</param>
    ''' <param name="targetRow">更新対象行</param>
    Private Sub UpdateShozai(sqlCon As SqlConnection, sqlTran As SqlTransaction, targetRow As DataRow)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE OIL.OIT0005_SHOZAI")
        sqlStat.AppendLine("   SET  BRANCHCODE         = @BRANCHCODE")
        sqlStat.AppendLine("       ,OFFICECODE         = @OFFICECODE")
        sqlStat.AppendLine("       ,LOCATIONCODE       = @LOCATIONCODE")
        sqlStat.AppendLine("       ,TANKSTATUS         = @TANKSTATUS")
        sqlStat.AppendLine("       ,LOADINGKBN         = @LOADINGKBN")
        sqlStat.AppendLine("       ,EMPARRDATE         = @EMPARRDATE")
        sqlStat.AppendLine("       ,ACTUALEMPARRDATE   = @ACTUALEMPARRDATE")
        sqlStat.AppendLine("       ,OILCODE            = @OILCODE")
        sqlStat.AppendLine("       ,OILNAME            = @OILNAME")
        sqlStat.AppendLine("       ,ORDERINGTYPE       = @ORDERINGTYPE")
        sqlStat.AppendLine("       ,ORDERINGOILNAME    = @ORDERINGOILNAME")
        sqlStat.AppendLine("       ,LASTOILCODE        = @LASTOILCODE")
        sqlStat.AppendLine("       ,LASTOILNAME        = @LASTOILNAME")
        sqlStat.AppendLine("       ,PREORDERINGTYPE    = @PREORDERINGTYPE")
        sqlStat.AppendLine("       ,PREORDERINGOILNAME = @PREORDERINGOILNAME")
        sqlStat.AppendLine("       ,UPDYMD             = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER            = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID          = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD         = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE TANKNUMBER = @TANKNUMBER")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("TANKNUMBER", SqlDbType.NVarChar).Value = targetRow("TANKNUMBER")
                .Add("BRANCHCODE", SqlDbType.NVarChar).Value = targetRow("BRANCHCODE")
                .Add("OFFICECODE", SqlDbType.NVarChar).Value = targetRow("OFFICECODE")
                .Add("LOCATIONCODE", SqlDbType.NVarChar).Value = targetRow("LOCATIONCODE")
                .Add("TANKSTATUS", SqlDbType.NVarChar).Value = targetRow("TANKSTATUS")
                .Add("LOADINGKBN", SqlDbType.NVarChar).Value = targetRow("LOADINGKBN")
                .Add("EMPARRDATE", SqlDbType.Date).Value = targetRow("EMPARRDATE")
                .Add("ACTUALEMPARRDATE", SqlDbType.Date).Value = targetRow("ACTUALEMPARRDATE")
                .Add("OILCODE", SqlDbType.NVarChar).Value = targetRow("OILCODE")
                .Add("OILNAME", SqlDbType.NVarChar).Value = targetRow("OILNAME")
                .Add("ORDERINGTYPE", SqlDbType.NVarChar).Value = targetRow("ORDERINGTYPE")
                .Add("ORDERINGOILNAME", SqlDbType.NVarChar).Value = targetRow("ORDERINGOILNAME")
                .Add("LASTOILCODE", SqlDbType.NVarChar).Value = targetRow("LASTOILCODE")
                .Add("LASTOILNAME", SqlDbType.NVarChar).Value = targetRow("LASTOILNAME")
                .Add("PREORDERINGTYPE", SqlDbType.NVarChar).Value = targetRow("PREORDERINGTYPE")
                .Add("PREORDERINGOILNAME", SqlDbType.NVarChar).Value = targetRow("PREORDERINGOILNAME")
                .Add("UPDYMD", SqlDbType.DateTime).Value = targetRow("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = targetRow("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = targetRow("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = targetRow("RECEIVEYMD")
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using
        CS0020JOURNAL.TABLENM = "OIT0005_SHOZAI"
        CS0020JOURNAL.ACTION = "UPDATE"
        CS0020JOURNAL.ROW = targetRow
        CS0020JOURNAL.CS0020JOURNAL()
        If Not isNormal(CS0020JOURNAL.ERR) Then
            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
            Return
        End If
    End Sub
End Class